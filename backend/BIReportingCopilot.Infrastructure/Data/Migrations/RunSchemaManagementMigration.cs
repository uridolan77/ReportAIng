using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BIReportingCopilot.Infrastructure.Data.Migrations;

/// <summary>
/// Migration runner for Schema Management tables
/// </summary>
public class SchemaManagementMigrationRunner
{
    private readonly BICopilotContext _context;
    private readonly ILogger<SchemaManagementMigrationRunner> _logger;

    public SchemaManagementMigrationRunner(BICopilotContext context, ILogger<SchemaManagementMigrationRunner> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Run the schema management migration
    /// </summary>
    public async Task RunMigrationAsync()
    {
        try
        {
            _logger.LogInformation("Starting Schema Management migration...");

            // Read the migration script
            var migrationScript = await File.ReadAllTextAsync(
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, 
                "Data", "Migrations", "AddBusinessSchemaManagement.sql"));

            // Execute the migration script
            await _context.Database.ExecuteSqlRawAsync(migrationScript);

            _logger.LogInformation("Schema Management migration completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error running Schema Management migration");
            throw;
        }
    }

    /// <summary>
    /// Check if schema management tables exist
    /// </summary>
    public async Task<bool> SchemaManagementTablesExistAsync()
    {
        try
        {
            var tableExistsQuery = @"
                SELECT COUNT(*) 
                FROM INFORMATION_SCHEMA.TABLES 
                WHERE TABLE_NAME IN ('BusinessSchemas', 'BusinessSchemaVersions', 'SchemaTableContexts', 
                                     'SchemaColumnContexts', 'SchemaGlossaryTerms', 'SchemaRelationships', 
                                     'UserSchemaPreferences')";

            var tableCount = await _context.Database.SqlQueryRaw<int>(tableExistsQuery).FirstOrDefaultAsync();
            return tableCount == 7; // All 7 tables should exist
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if schema management tables exist");
            return false;
        }
    }

    /// <summary>
    /// Initialize default schema if needed
    /// </summary>
    public async Task InitializeDefaultSchemaAsync()
    {
        try
        {
            // Check if default schema already exists
            var defaultSchemaExists = await _context.BusinessSchemas
                .AnyAsync(s => s.Name == "Default Schema" && s.IsDefault);

            if (!defaultSchemaExists)
            {
                _logger.LogInformation("Creating default business schema...");

                var defaultSchema = new Core.Models.SchemaManagement.BusinessSchema
                {
                    Id = Guid.NewGuid(),
                    Name = "Default Schema",
                    Description = "Default business context schema for auto-generated content",
                    CreatedBy = "System",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedBy = "System",
                    UpdatedAt = DateTime.UtcNow,
                    IsActive = true,
                    IsDefault = true,
                    Tags = "[\"System\", \"Default\"]"
                };

                _context.BusinessSchemas.Add(defaultSchema);

                var defaultVersion = new Core.Models.SchemaManagement.BusinessSchemaVersion
                {
                    Id = Guid.NewGuid(),
                    SchemaId = defaultSchema.Id,
                    VersionNumber = 1,
                    VersionName = "v1.0",
                    Description = "Initial default schema version",
                    CreatedBy = "System",
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true,
                    IsCurrent = true,
                    ChangeLog = "[{\"Timestamp\":\"" + DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ") + "\",\"Type\":\"Created\",\"Category\":\"Schema\",\"Item\":\"Default Schema\",\"Description\":\"Initial default schema creation\"}]"
                };

                _context.BusinessSchemaVersions.Add(defaultVersion);

                await _context.SaveChangesAsync();

                _logger.LogInformation("Default business schema created successfully");
            }
            else
            {
                _logger.LogInformation("Default business schema already exists");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initializing default schema");
            throw;
        }
    }
}
