using Hangfire;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using BIReportingCopilot.Core.Interfaces;
using BIReportingCopilot.Core.Models;
using BIReportingCopilot.Infrastructure.Data;
using BIReportingCopilot.Infrastructure.Configuration;
using BIReportingCopilot.Core.Configuration;

namespace BIReportingCopilot.Infrastructure.Services;

/// <summary>
/// Unified background job management service consolidating all background operations
/// Replaces CleanupJob, SchemaRefreshJob, and related job functionality
/// </summary>
public class BackgroundJobManagementService
{
    private readonly BICopilotContext _context;
    private readonly ISchemaService _schemaService;
    private readonly IAuditService _auditService;
    private readonly ICacheService _cacheService;
    private readonly IQueryService _queryService;
    private readonly ConfigurationService _configurationService;
    private readonly ILogger<BackgroundJobManagementService> _logger;
    private readonly IHubContext<Hub> _hubContext;
    private readonly PerformanceConfiguration _performanceConfig;

    public BackgroundJobManagementService(
        BICopilotContext context,
        ISchemaService schemaService,
        IAuditService auditService,
        ICacheService cacheService,
        IQueryService queryService,
        ConfigurationService configurationService,
        ILogger<BackgroundJobManagementService> logger,
        IHubContext<Hub> hubContext)
    {
        _context = context;
        _schemaService = schemaService;
        _auditService = auditService;
        _cacheService = cacheService;
        _queryService = queryService;
        _configurationService = configurationService;
        _logger = logger;
        _hubContext = hubContext;
        _performanceConfig = configurationService.GetPerformanceSettings();
    }

    #region Cleanup Operations

    /// <summary>
    /// Perform comprehensive system cleanup
    /// </summary>
    [DisableConcurrentExecution(timeoutInSeconds: 10 * 60)]
    public async Task PerformSystemCleanupAsync()
    {
        _logger.LogInformation("Starting comprehensive system cleanup");

        try
        {
            var cleanupTasks = new[]
            {
                CleanupQueryHistoryAsync(),
                CleanupExpiredCacheAsync(),
                CleanupAuditLogsAsync(),
                CleanupInactiveSessionsAsync(),
                CleanupTempFilesAsync(),
                CleanupPerformanceMetricsAsync()
            };

            await Task.WhenAll(cleanupTasks);

            await _auditService.LogAsync(
                "SYSTEM_CLEANUP_COMPLETED",
                "System",
                "BackgroundJob",
                null,
                new { CleanupTasks = cleanupTasks.Length });

            _logger.LogInformation("System cleanup completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during system cleanup");
            throw;
        }
    }

    private async Task CleanupQueryHistoryAsync()
    {
        var retentionDays = _performanceConfig.RetentionDays;
        var cutoffDate = DateTime.UtcNow.AddDays(-retentionDays);

        var deletedCount = await _context.QueryHistory
            .Where(q => q.QueryTimestamp < cutoffDate)
            .ExecuteDeleteAsync();

        _logger.LogInformation("Deleted {Count} old query history records", deletedCount);
    }

    private async Task CleanupExpiredCacheAsync()
    {
        var expiredCacheEntries = await _context.QueryCache
            .Where(c => c.ExpiryTimestamp < DateTime.UtcNow)
            .Select(c => c.Id)
            .ToListAsync();

        if (expiredCacheEntries.Any())
        {
            await _context.QueryCache
                .Where(c => expiredCacheEntries.Contains(c.Id))
                .ExecuteDeleteAsync();

            _logger.LogInformation("Deleted {Count} expired cache entries", expiredCacheEntries.Count);
        }
    }

    private async Task CleanupAuditLogsAsync()
    {
        var retentionDays = _performanceConfig.RetentionDays * 12; // Keep audit logs longer
        var cutoffDate = DateTime.UtcNow.AddDays(-retentionDays);

        var deletedCount = await _context.AuditLog
            .Where(a => a.Timestamp < cutoffDate && a.Severity != "Critical")
            .ExecuteDeleteAsync();

        _logger.LogInformation("Deleted {Count} old audit log entries", deletedCount);
    }

    private async Task CleanupInactiveSessionsAsync()
    {
        var sessionTimeout = TimeSpan.FromHours(24);
        var cutoffTime = DateTime.UtcNow.Subtract(sessionTimeout);

        try
        {
            // Mark inactive sessions
            var inactiveSessions = await _context.UserSessions
                .Where(s => s.LastActivity < cutoffTime && s.IsActive)
                .ToListAsync();

            foreach (var session in inactiveSessions)
            {
                session.IsActive = false;
            }

            if (inactiveSessions.Any())
            {
                await _context.SaveChangesAsync();
                _logger.LogInformation("Marked {Count} sessions as inactive", inactiveSessions.Count);
            }

            // Clean up expired refresh tokens
            var expiredTokens = await _context.RefreshTokens
                .Where(t => t.ExpiresAt < DateTime.UtcNow || t.IsRevoked)
                .ToListAsync();

            if (expiredTokens.Any())
            {
                _context.RefreshTokens.RemoveRange(expiredTokens);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Deleted {Count} expired refresh tokens", expiredTokens.Count);
            }

            // Delete very old inactive sessions
            var oldSessionCutoff = DateTime.UtcNow.AddDays(-30);
            var deletedSessionCount = await _context.UserSessions
                .Where(s => !s.IsActive && s.LastActivity < oldSessionCutoff)
                .ExecuteDeleteAsync();

            if (deletedSessionCount > 0)
            {
                _logger.LogInformation("Deleted {Count} old inactive sessions", deletedSessionCount);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during session cleanup");
        }
    }

    private async Task CleanupTempFilesAsync()
    {
        try
        {
            // Clean up temporary export files older than 1 day
            var tempFilesCutoff = DateTime.UtcNow.AddDays(-1);
            var tempFiles = await _context.TempFiles
                .Where(f => f.CreatedAt < tempFilesCutoff)
                .ToListAsync();

            if (tempFiles.Any())
            {
                _context.TempFiles.RemoveRange(tempFiles);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Deleted {Count} temporary files", tempFiles.Count);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error during temp files cleanup");
        }
    }

    private async Task CleanupPerformanceMetricsAsync()
    {
        try
        {
            // Clean up old performance metrics (keep last 30 days)
            var metricsCutoff = DateTime.UtcNow.AddDays(-30);
            var deletedMetrics = await _context.PerformanceMetrics
                .Where(m => m.Timestamp < metricsCutoff)
                .ExecuteDeleteAsync();

            if (deletedMetrics > 0)
            {
                _logger.LogInformation("Deleted {Count} old performance metrics", deletedMetrics);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error during performance metrics cleanup");
        }
    }

    #endregion

    #region Schema Refresh Operations

    /// <summary>
    /// Refresh all database schemas
    /// </summary>
    [AutomaticRetry(Attempts = 3)]
    public async Task RefreshAllSchemasAsync()
    {
        _logger.LogInformation("Starting comprehensive schema refresh");

        try
        {
            // Get configured data sources - use BIDatabase as the primary data source
            var dataSources = new[] { "BIDatabase" };

            foreach (var dataSource in dataSources)
            {
                await RefreshSchemaForDataSourceAsync(dataSource);
            }

            await _auditService.LogAsync(
                "SCHEMA_REFRESH_COMPLETED",
                "System",
                "SchemaMetadata",
                null,
                new { DataSourceCount = dataSources.Length });

            // Notify all connected clients
            await _hubContext.Clients.All.SendAsync(
                "SystemAlert",
                new {
                    Type = "SchemaRefresh",
                    Message = "Database schema has been refreshed",
                    Level = "info"
                });

            _logger.LogInformation("Schema refresh completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during schema refresh");
            throw;
        }
    }

    /// <summary>
    /// Refresh schema for specific data source
    /// </summary>
    public async Task RefreshSchemaForDataSourceAsync(string dataSource)
    {
        _logger.LogInformation("Refreshing schema for data source: {DataSource}", dataSource);

        try
        {
            var oldSchema = await _schemaService.GetSchemaMetadataAsync(dataSource);
            var newSchema = await _schemaService.RefreshSchemaMetadataAsync(dataSource);

            // Detect and handle schema changes
            var changes = DetectSchemaChanges(oldSchema, newSchema);

            if (changes.Any())
            {
                _logger.LogWarning("Schema changes detected: {ChangeCount} changes", changes.Count);

                foreach (var change in changes)
                {
                    await _auditService.LogAsync(
                        "SCHEMA_CHANGE_DETECTED",
                        "System",
                        "SchemaMetadata",
                        change.ObjectName,
                        change);
                }

                // Invalidate affected caches
                await InvalidateAffectedCachesAsync(changes);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing schema for data source: {DataSource}", dataSource);
            throw;
        }
    }

    #endregion

    #region Job Scheduling and Management

    /// <summary>
    /// Schedule all recurring background jobs
    /// </summary>
    public void ScheduleRecurringJobs()
    {
        _logger.LogInformation("Scheduling recurring background jobs");

        // Schedule system cleanup job
        RecurringJob.AddOrUpdate(
            "system-cleanup",
            () => PerformSystemCleanupAsync(),
            Cron.Daily(2)); // Run at 2 AM daily

        // Schedule schema refresh job
        RecurringJob.AddOrUpdate(
            "schema-refresh",
            () => RefreshAllSchemasAsync(),
            Cron.Hourly()); // Run hourly

        _logger.LogInformation("Recurring background jobs scheduled successfully");
    }

    #endregion

    #region Private Helper Methods

    private List<SchemaChangeEvent> DetectSchemaChanges(SchemaMetadata oldSchema, SchemaMetadata newSchema)
    {
        var changes = new List<SchemaChangeEvent>();

        // Detect new tables
        var newTables = newSchema.Tables
            .Where(t => !oldSchema.Tables.Any(ot => ot.Name == t.Name))
            .Select(t => new SchemaChangeEvent
            {
                Id = Guid.NewGuid().ToString(),
                ChangeType = "TABLE_ADDED",
                ObjectName = t.Name,
                ObjectType = "TABLE",
                NewValue = new Dictionary<string, object> { ["table"] = t }
            });

        changes.AddRange(newTables);
        return changes;
    }

    private async Task InvalidateAffectedCachesAsync(List<SchemaChangeEvent> changes)
    {
        try
        {
            _logger.LogInformation("Invalidating caches affected by {ChangeCount} schema changes", changes.Count);

            foreach (var change in changes)
            {
                if (change.ObjectType == "TABLE")
                {
                    await _queryService.InvalidateQueryCacheAsync($"*{change.ObjectName}*");
                }
            }

            await _cacheService.RemovePatternAsync("schema:*");
            await _cacheService.RemovePatternAsync("table:*");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error invalidating caches after schema changes");
        }
    }

    #endregion
}

/// <summary>
/// Schema change event for tracking database schema modifications
/// </summary>
public class SchemaChangeEvent
{
    public string Id { get; set; } = string.Empty;
    public string ChangeType { get; set; } = string.Empty;
    public string ObjectName { get; set; } = string.Empty;
    public string ObjectType { get; set; } = string.Empty;
    public Dictionary<string, object>? OldValue { get; set; }
    public Dictionary<string, object>? NewValue { get; set; }
}