using BIReportingCopilot.Core.Interfaces;
using BIReportingCopilot.Core.Constants;
using BIReportingCopilot.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace BIReportingCopilot.Infrastructure.Services;

public interface IDatabaseInitializationService
{
    Task InitializeAsync();
}

public class DatabaseInitializationService : IDatabaseInitializationService
{
    private readonly BICopilotContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IConfiguration _configuration;
    private readonly ILogger<DatabaseInitializationService> _logger;

    public DatabaseInitializationService(
        BICopilotContext context,
        IPasswordHasher passwordHasher,
        IConfiguration configuration,
        ILogger<DatabaseInitializationService> logger)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task InitializeAsync()
    {
        try
        {
            // Check if we're using an in-memory database
            var isInMemory = _context.Database.ProviderName == "Microsoft.EntityFrameworkCore.InMemory";

            if (isInMemory)
            {
                // For in-memory database, just ensure it's created
                await _context.Database.EnsureCreatedAsync();
                _logger.LogInformation("In-memory database created successfully");
            }
            else
            {
                // For relational databases, apply migrations
                await _context.Database.MigrateAsync();
                _logger.LogInformation("Database migrations applied successfully");
            }

            // Initialize admin user password if not set
            await InitializeAdminUserAsync();

            _logger.LogInformation("Database initialization completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during database initialization");
            throw;
        }
    }

    private async Task InitializeAdminUserAsync()
    {
        try
        {
            var adminUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == ApplicationConstants.Roles.Admin.ToLower());

            if (adminUser == null)
            {
                _logger.LogWarning("Admin user not found in database");
                return;
            }

            // Check if admin user already has a password hash
            if (!string.IsNullOrEmpty(adminUser.PasswordHash))
            {
                _logger.LogInformation("Admin user already has password set");
                return;
            }

            // Get default admin password from configuration or use default
            var defaultPassword = _configuration["DefaultAdminPassword"] ?? "Admin123!";

            // Hash the password and update the user
            adminUser.PasswordHash = _passwordHasher.HashPassword(defaultPassword);
            adminUser.UpdatedDate = DateTime.UtcNow;
            adminUser.UpdatedBy = ApplicationConstants.EntityTypes.System;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Admin user password initialized successfully");
            _logger.LogWarning("Default admin credentials - Username: {Username}, Password: {Password}",
                ApplicationConstants.Roles.Admin.ToLower(), defaultPassword);
            _logger.LogWarning("Please change the default admin password after first login");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initializing admin user password");
            throw;
        }
    }
}
