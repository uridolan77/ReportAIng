using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using BIReportingCopilot.Infrastructure.Data.Contexts;

namespace BIReportingCopilot.Infrastructure.Data.Migration;

/// <summary>
/// Service to help migrate from monolithic BICopilotContext to bounded contexts
/// </summary>
public class ContextMigrationService
{
    private readonly IDbContextFactory _contextFactory;
    private readonly ILogger<ContextMigrationService> _logger;

    public ContextMigrationService(
        IDbContextFactory contextFactory,
        ILogger<ContextMigrationService> logger)
    {
        _contextFactory = contextFactory;
        _logger = logger;
    }

    /// <summary>
    /// Migrate data from legacy context to bounded contexts
    /// </summary>
    public async Task<MigrationResult> MigrateToContextsAsync(bool dryRun = true)
    {
        var result = new MigrationResult();
        
        try
        {
            _logger.LogInformation("Starting context migration (DryRun: {DryRun})", dryRun);

            // Validate all contexts can be created
            var validation = await _contextFactory.ValidateAllContextsAsync();
            if (!validation.IsValid)
            {
                result.IsSuccessful = false;
                result.Errors.AddRange(validation.FailedContexts.Values);
                return result;
            }

            // Migrate security data
            await MigrateSecurityDataAsync(result, dryRun);

            // Migrate tuning data
            await MigrateTuningDataAsync(result, dryRun);

            // Migrate query data
            await MigrateQueryDataAsync(result, dryRun);

            // Migrate schema data
            await MigrateSchemaDataAsync(result, dryRun);

            // Migrate monitoring data
            await MigrateMonitoringDataAsync(result, dryRun);

            result.IsSuccessful = result.Errors.Count == 0;
            _logger.LogInformation("Context migration completed. Success: {IsSuccessful}, Errors: {ErrorCount}", 
                result.IsSuccessful, result.Errors.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during context migration");
            result.IsSuccessful = false;
            result.Errors.Add($"Migration failed: {ex.Message}");
        }

        return result;
    }

    private async Task MigrateSecurityDataAsync(MigrationResult result, bool dryRun)
    {
        try
        {
            using var legacyContext = _contextFactory.CreateLegacyContext();
            using var securityContext = _contextFactory.CreateSecurityContext();

            // Count records to migrate
            var userCount = await legacyContext.Users.CountAsync();
            var sessionCount = await legacyContext.UserSessions.CountAsync();
            var auditCount = await legacyContext.AuditLog.CountAsync();

            result.MigrationStats["Users"] = userCount;
            result.MigrationStats["UserSessions"] = sessionCount;
            result.MigrationStats["AuditLogs"] = auditCount;

            if (!dryRun)
            {
                // Migrate users
                var users = await legacyContext.Users.ToListAsync();
                securityContext.Users.AddRange(users);

                // Migrate sessions
                var sessions = await legacyContext.UserSessions.ToListAsync();
                securityContext.UserSessions.AddRange(sessions);

                // Migrate audit logs
                var auditLogs = await legacyContext.AuditLog.ToListAsync();
                securityContext.AuditLog.AddRange(auditLogs);

                await securityContext.SaveChangesAsync();
            }

            _logger.LogInformation("Security data migration: Users={UserCount}, Sessions={SessionCount}, AuditLogs={AuditCount}", 
                userCount, sessionCount, auditCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error migrating security data");
            result.Errors.Add($"Security migration error: {ex.Message}");
        }
    }

    private async Task MigrateTuningDataAsync(MigrationResult result, bool dryRun)
    {
        try
        {
            using var legacyContext = _contextFactory.CreateLegacyContext();
            using var tuningContext = _contextFactory.CreateTuningContext();

            // Count records to migrate
            var tableCount = await legacyContext.BusinessTableInfo.CountAsync();
            var columnCount = await legacyContext.BusinessColumnInfo.CountAsync();
            var patternCount = await legacyContext.QueryPatterns.CountAsync();
            var glossaryCount = await legacyContext.BusinessGlossary.CountAsync();

            result.MigrationStats["BusinessTables"] = tableCount;
            result.MigrationStats["BusinessColumns"] = columnCount;
            result.MigrationStats["QueryPatterns"] = patternCount;
            result.MigrationStats["GlossaryTerms"] = glossaryCount;

            if (!dryRun)
            {
                // Migrate business tables and columns
                var tables = await legacyContext.BusinessTableInfo.Include(t => t.Columns).ToListAsync();
                tuningContext.BusinessTableInfo.AddRange(tables);

                // Migrate query patterns
                var patterns = await legacyContext.QueryPatterns.ToListAsync();
                tuningContext.QueryPatterns.AddRange(patterns);

                // Migrate glossary
                var glossary = await legacyContext.BusinessGlossary.ToListAsync();
                tuningContext.BusinessGlossary.AddRange(glossary);

                await tuningContext.SaveChangesAsync();
            }

            _logger.LogInformation("Tuning data migration: Tables={TableCount}, Columns={ColumnCount}, Patterns={PatternCount}, Glossary={GlossaryCount}", 
                tableCount, columnCount, patternCount, glossaryCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error migrating tuning data");
            result.Errors.Add($"Tuning migration error: {ex.Message}");
        }
    }

    private async Task MigrateQueryDataAsync(MigrationResult result, bool dryRun)
    {
        try
        {
            using var legacyContext = _contextFactory.CreateLegacyContext();
            using var queryContext = _contextFactory.CreateQueryContext();

            // Count records to migrate
            var historyCount = await legacyContext.QueryHistories.CountAsync();
            var suggestionCount = await legacyContext.QuerySuggestions.CountAsync();

            result.MigrationStats["QueryHistory"] = historyCount;
            result.MigrationStats["QuerySuggestions"] = suggestionCount;

            if (!dryRun)
            {
                // Migrate query history
                var history = await legacyContext.QueryHistories.ToListAsync();
                queryContext.QueryHistories.AddRange(history);

                // Migrate suggestions
                var suggestions = await legacyContext.QuerySuggestions.Include(s => s.Category).ToListAsync();
                queryContext.QuerySuggestions.AddRange(suggestions);

                await queryContext.SaveChangesAsync();
            }

            _logger.LogInformation("Query data migration: History={HistoryCount}, Suggestions={SuggestionCount}", 
                historyCount, suggestionCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error migrating query data");
            result.Errors.Add($"Query migration error: {ex.Message}");
        }
    }

    private async Task MigrateSchemaDataAsync(MigrationResult result, bool dryRun)
    {
        try
        {
            using var legacyContext = _contextFactory.CreateLegacyContext();
            using var schemaContext = _contextFactory.CreateSchemaContext();

            // Count records to migrate
            var metadataCount = await legacyContext.SchemaMetadata.CountAsync();

            result.MigrationStats["SchemaMetadata"] = metadataCount;

            if (!dryRun)
            {
                // Migrate schema metadata
                var metadata = await legacyContext.SchemaMetadata.ToListAsync();
                schemaContext.SchemaMetadata.AddRange(metadata);

                await schemaContext.SaveChangesAsync();
            }

            _logger.LogInformation("Schema data migration: Metadata={MetadataCount}", metadataCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error migrating schema data");
            result.Errors.Add($"Schema migration error: {ex.Message}");
        }
    }

    private async Task MigrateMonitoringDataAsync(MigrationResult result, bool dryRun)
    {
        try
        {
            using var legacyContext = _contextFactory.CreateLegacyContext();
            using var monitoringContext = _contextFactory.CreateMonitoringContext();

            // For monitoring data, we typically don't migrate historical data
            // Instead, we start fresh with the new monitoring context
            result.MigrationStats["MonitoringData"] = 0;

            _logger.LogInformation("Monitoring data migration: Starting fresh (no historical data migrated)");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error migrating monitoring data");
            result.Errors.Add($"Monitoring migration error: {ex.Message}");
        }
    }

    /// <summary>
    /// Validate migration readiness
    /// </summary>
    public async Task<MigrationReadinessResult> ValidateMigrationReadinessAsync()
    {
        var result = new MigrationReadinessResult();

        try
        {
            // Check if all contexts can be created
            var contextValidation = await _contextFactory.ValidateAllContextsAsync();
            result.ContextsReady = contextValidation.IsValid;
            result.ContextErrors.AddRange(contextValidation.FailedContexts.Values);

            // Check if legacy context has data
            using var legacyContext = _contextFactory.CreateLegacyContext();
            result.HasLegacyData = await legacyContext.Users.AnyAsync() ||
                                   await legacyContext.BusinessTableInfo.AnyAsync() ||
                                   await legacyContext.QueryHistories.AnyAsync();

            // Check if bounded contexts are empty (safe to migrate)
            result.BoundedContextsEmpty = await CheckBoundedContextsEmptyAsync();

            result.IsReady = result.ContextsReady && result.HasLegacyData && result.BoundedContextsEmpty;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating migration readiness");
            result.IsReady = false;
            result.ContextErrors.Add($"Validation error: {ex.Message}");
        }

        return result;
    }

    private async Task<bool> CheckBoundedContextsEmptyAsync()
    {
        try
        {
            using var securityContext = _contextFactory.CreateSecurityContext();
            using var tuningContext = _contextFactory.CreateTuningContext();
            using var queryContext = _contextFactory.CreateQueryContext();

            var securityEmpty = !await securityContext.Users.AnyAsync();
            var tuningEmpty = !await tuningContext.BusinessTableInfo.AnyAsync();
            var queryEmpty = !await queryContext.QueryHistories.AnyAsync();

            return securityEmpty && tuningEmpty && queryEmpty;
        }
        catch
        {
            return false;
        }
    }
}

/// <summary>
/// Result of context migration operation
/// </summary>
public class MigrationResult
{
    public bool IsSuccessful { get; set; } = true;
    public List<string> Errors { get; set; } = new();
    public Dictionary<string, int> MigrationStats { get; set; } = new();
    public DateTime CompletedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Result of migration readiness validation
/// </summary>
public class MigrationReadinessResult
{
    public bool IsReady { get; set; }
    public bool ContextsReady { get; set; }
    public bool HasLegacyData { get; set; }
    public bool BoundedContextsEmpty { get; set; }
    public List<string> ContextErrors { get; set; } = new();
    public DateTime CheckedAt { get; set; } = DateTime.UtcNow;
}
