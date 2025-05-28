using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using BIReportingCopilot.Infrastructure.Data;
using BIReportingCopilot.Core.Interfaces;
using BIReportingCopilot.Infrastructure.Security;
using BIReportingCopilot.Infrastructure.Services;
using BIReportingCopilot.Infrastructure.Repositories;
using BIReportingCopilot.Infrastructure.Performance;

namespace BIReportingCopilot.Tests.Integration;

public class TestWebApplicationFactory<TStartup> : WebApplicationFactory<TStartup> where TStartup : class
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((context, config) =>
        {
            config.AddJsonFile("appsettings.Test.json", optional: false, reloadOnChange: false);
        });

        builder.ConfigureServices(services =>
        {
            // Remove the existing DbContext registration
            var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<BICopilotContext>));
            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            // Use unique database name for each test to avoid conflicts
            var databaseName = $"TestDatabase_{Guid.NewGuid()}";
            services.AddDbContext<BICopilotContext>(options =>
            {
                options.UseInMemoryDatabase(databaseName);
                options.EnableSensitiveDataLogging();
            });

            // Add required caching services for testing
            services.AddMemoryCache();

            // Replace services with test implementations
            services.AddScoped<IPasswordHasher, PasswordHasher>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<ITokenRepository, TokenRepository>();
            services.AddScoped<IAuthenticationService, EnhancedAuthenticationService>();

            // Mock external services for testing
            services.AddScoped<ICacheService, TestCacheService>();
            services.AddScoped<IAuditService, AuditService>();

            // Remove Hangfire services for testing
            var hangfireDescriptors = services.Where(d =>
                d.ServiceType.FullName?.Contains("Hangfire") == true ||
                d.ImplementationType?.FullName?.Contains("Hangfire") == true)
                .ToList();

            foreach (var hangfireDescriptor in hangfireDescriptors)
            {
                services.Remove(hangfireDescriptor);
            }

            // Build service provider and seed test data
            var serviceProvider = services.BuildServiceProvider();
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<BICopilotContext>();
            var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();

            SeedTestData(context, passwordHasher);
        });

        builder.UseEnvironment("Test");
    }

    private static void SeedTestData(BICopilotContext context, IPasswordHasher passwordHasher)
    {
        try
        {
            context.Database.EnsureCreated();

            // Check if data already exists to avoid conflicts
            if (context.Users.Any())
            {
                return; // Data already seeded
            }

            // Create test users with unique IDs to avoid conflicts
            var uniqueId = Guid.NewGuid().ToString("N")[..8];
            var testUsers = new[]
            {
                new Infrastructure.Data.Entities.UserEntity
                {
                    Id = $"test-user-1-{uniqueId}",
                    Username = "testuser",
                    Email = "test@example.com",
                    DisplayName = "Test User",
                    Roles = "User",
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                },
                new Infrastructure.Data.Entities.UserEntity
                {
                    Id = $"test-admin-1-{uniqueId}",
                    Username = "testadmin",
                    Email = "admin@example.com",
                    DisplayName = "Test Admin",
                    Roles = "Admin",
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                },
                new Infrastructure.Data.Entities.UserEntity
                {
                    Id = $"test-inactive-1-{uniqueId}",
                    Username = "inactiveuser",
                    Email = "inactive@example.com",
                    DisplayName = "Inactive User",
                    Roles = "User",
                    IsActive = false,
                    CreatedDate = DateTime.UtcNow
                }
            };

            context.Users.AddRange(testUsers);
            context.SaveChanges();
        }
        catch (Exception ex)
        {
            // Log the error but don't fail the test setup
            Console.WriteLine($"Warning: Failed to seed test data: {ex.Message}");
        }
    }
}
