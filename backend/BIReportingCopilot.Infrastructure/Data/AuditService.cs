using BIReportingCopilot.Core.Interfaces;
using BIReportingCopilot.Core.Interfaces.Security;
using BIReportingCopilot.Core.Interfaces.Data;
using BIReportingCopilot.Core.Models;
using BIReportingCopilot.Infrastructure.Data;
using BIReportingCopilot.Infrastructure.Data.Entities;
using BIReportingCopilot.Infrastructure.Data.Contexts;
using ContextType = BIReportingCopilot.Infrastructure.Data.Contexts.ContextType;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace BIReportingCopilot.Infrastructure.Data;

/// <summary>
/// Enhanced AuditService using bounded contexts for better performance and maintainability
/// Uses SecurityDbContext for audit logs and QueryDbContext for query history
/// Implements both Security.IAuditService and Data.IAuditService interfaces
/// </summary>
public class AuditService : BIReportingCopilot.Core.Interfaces.Security.IAuditService, BIReportingCopilot.Core.Interfaces.Data.IAuditService
{
    private readonly ILogger<AuditService> _logger;
    private readonly IDbContextFactory _contextFactory;

    public AuditService(ILogger<AuditService> logger, IDbContextFactory contextFactory)
    {
        _logger = logger;
        _contextFactory = contextFactory;
    }

    public async Task LogAsync(string action, string userId, string? entityType = null, string? entityId = null,
                              object? details = null, string? ipAddress = null, string? userAgent = null)
    {
        try
        {
            await _contextFactory.ExecuteWithContextAsync(ContextType.Security, async context =>
            {
                var securityContext = (SecurityDbContext)context;

                var auditEntry = new Infrastructure.Data.Entities.AuditLogEntity
                {
                    Action = action,
                    UserId = userId,
                    EntityType = entityType,
                    EntityId = entityId,
                    Details = details != null ? JsonSerializer.Serialize(details) : null,
                    IpAddress = ipAddress,
                    UserAgent = userAgent,
                    Timestamp = DateTime.UtcNow,
                    Severity = "Info"
                };

                securityContext.AuditLog.Add(auditEntry);
                await securityContext.SaveChangesAsync();
            });

            _logger.LogInformation("Audit log created: {Action} by {UserId}", action, userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create audit log entry for action: {Action}, user: {UserId}", action, userId);
        }
    }

    public async Task LogQueryAsync(string userId, string sessionId, string naturalLanguageQuery, string generatedSQL,
                                   bool successful, int executionTimeMs, string? error = null)
    {
        try
        {
            // Log to audit table and query history using multiple contexts
            await _contextFactory.ExecuteWithMultipleContextsAsync(
                new[] { ContextType.Security, ContextType.Query },
                async contexts =>
                {
                    var securityContext = (SecurityDbContext)contexts[ContextType.Security];
                    var queryContext = (QueryDbContext)contexts[ContextType.Query];

                    // Log to audit table
                    var auditDetails = new
                    {
                        SessionId = sessionId,
                        NaturalLanguageQuery = naturalLanguageQuery,
                        GeneratedSQL = generatedSQL,
                        ExecutionTimeMs = executionTimeMs,
                        Error = error
                    };

                    var auditEntry = new Infrastructure.Data.Entities.AuditLogEntity
                    {
                        Action = "QUERY_EXECUTED",
                        UserId = userId,
                        EntityType = "Query",
                        EntityId = sessionId,
                        Details = JsonSerializer.Serialize(auditDetails),
                        Timestamp = DateTime.UtcNow,
                        Severity = "Info"
                    };

                    securityContext.AuditLog.Add(auditEntry);

                    // Also log to query history (using Unified entity)
                    var queryHistory = new Core.Models.UnifiedQueryHistoryEntity
                    {
                        UserId = userId,
                        SessionId = sessionId,
                        Query = naturalLanguageQuery,
                        GeneratedSql = generatedSQL,
                        ExecutionTimeMs = executionTimeMs,
                        IsSuccessful = successful,
                        ErrorMessage = error,
                        ExecutedAt = DateTime.UtcNow,
                        CreatedBy = userId,
                        UpdatedBy = userId,
                        CreatedDate = DateTime.UtcNow,
                        LastUpdated = DateTime.UtcNow,
                        IsActive = true
                    };

                    queryContext.QueryHistory.Add(queryHistory);

                    // Save both contexts
                    await securityContext.SaveChangesAsync();
                    await queryContext.SaveChangesAsync();

                    _logger.LogInformation("✅ QUERY HISTORY SAVED - User: {UserId}, Question: '{Question}', Successful: {Successful}, QueryHistoryId: {QueryHistoryId}",
                        userId, naturalLanguageQuery, successful, queryHistory.Id);

                    return Task.CompletedTask;
                });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to log query execution for user: {UserId}", userId);
        }
    }

    public async Task LogSecurityEventAsync(string eventType, string userId, string? details = null,
                                           string? ipAddress = null, string severity = "Info")
    {
        try
        {
            var auditEntry = new Infrastructure.Data.Entities.AuditLogEntity
            {
                Action = $"SECURITY_{eventType}",
                UserId = userId,
                EntityType = "Security",
                Details = details,
                IpAddress = ipAddress,
                Timestamp = DateTime.UtcNow,
                Severity = severity
            };

            await _contextFactory.ExecuteWithContextAsync(ContextType.Security, async dbContext =>
            {
                var securityContext = (SecurityDbContext)dbContext;
                securityContext.AuditLog.Add(auditEntry);
                await securityContext.SaveChangesAsync();
            });

            _logger.LogWarning("Security event logged: {EventType} for user: {UserId}, severity: {Severity}",
                eventType, userId, severity);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to log security event: {EventType} for user: {UserId}", eventType, userId);
        }
    }

    public async Task<List<AuditLogEntry>> GetAuditLogsAsync(string? userId = null, DateTime? from = null,
                                                            DateTime? to = null, string? action = null)
    {
        try
        {
            return await _contextFactory.ExecuteWithContextAsync(ContextType.Security, async dbContext =>
            {
                var securityContext = (SecurityDbContext)dbContext;
                var query = securityContext.AuditLog.AsQueryable();

                if (!string.IsNullOrEmpty(userId))
                {
                    query = query.Where(a => a.UserId == userId);
                }

                if (from.HasValue)
                {
                    query = query.Where(a => a.Timestamp >= from.Value);
                }

                if (to.HasValue)
                {
                    query = query.Where(a => a.Timestamp <= to.Value);
                }

                if (!string.IsNullOrEmpty(action))
                {
                    query = query.Where(a => a.Action.Contains(action));
                }

                var entities = await query
                    .OrderByDescending(a => a.Timestamp)
                    .Take(1000) // Limit to 1000 records
                    .ToListAsync();

                return entities.Select(MapToAuditLogEntry).ToList();
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving audit logs");
            return new List<AuditLogEntry>();
        }
    }

    public async Task<SecurityReport> GenerateSecurityReportAsync(DateTime from, DateTime to)
    {
        try
        {
            return await _contextFactory.ExecuteWithContextAsync(ContextType.Security, async dbContext =>
            {
                var securityContext = (SecurityDbContext)dbContext;
                var securityEvents = await securityContext.AuditLog
                    .Where(a => a.Timestamp >= from && a.Timestamp <= to && a.Action.StartsWith("SECURITY_"))
                    .ToListAsync();

                var failedLogins = securityEvents.Count(e => e.Action == "SECURITY_LOGIN_FAILED");
                var successfulLogins = securityEvents.Count(e => e.Action == "SECURITY_LOGIN_SUCCESS");
                var suspiciousActivities = securityEvents.Count(e => e.Severity == "Warning" || e.Severity == "Error");

                var topFailedLoginIPs = securityEvents
                    .Where(e => e.Action == "SECURITY_LOGIN_FAILED" && !string.IsNullOrEmpty(e.IpAddress))
                    .GroupBy(e => e.IpAddress)
                    .OrderByDescending(g => g.Count())
                    .Take(10)
                    .ToDictionary(g => g.Key!, g => g.Count());

                var report = new SecurityReport
                {
                    From = from,
                    To = to,
                    TotalSecurityEvents = securityEvents.Count,
                    FailedLoginAttempts = failedLogins,
                    SuccessfulLogins = successfulLogins,
                    SuspiciousActivities = suspiciousActivities,
                    TopFailedLoginIPs = topFailedLoginIPs,
                    SecurityScore = CalculateSecurityScore(failedLogins, successfulLogins, suspiciousActivities)
                };

                _logger.LogInformation("Security report generated for period {From} to {To}", from, to);
                return report;
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating security report for period {From} to {To}", from, to);
            return new SecurityReport { From = from, To = to };
        }
    }

    public async Task<UsageReport> GenerateUsageReportAsync(DateTime from, DateTime to)
    {
        try
        {
            return await _contextFactory.ExecuteWithContextAsync(ContextType.Query, async dbContext =>
            {
                var queryContext = (QueryDbContext)dbContext;
                var queryHistory = await queryContext.QueryHistory
                    .Where(q => q.ExecutedAt >= from && q.ExecutedAt <= to)
                    .ToListAsync();

                var totalQueries = queryHistory.Count;
                var uniqueUsers = queryHistory.Select(q => q.UserId).Distinct().Count();
                var successfulQueries = queryHistory.Count(q => q.IsSuccessful);
                var averageResponseTime = queryHistory.Any() ? queryHistory.Average(q => q.ExecutionTimeMs) : 0;

                var queriesByUser = queryHistory
                    .GroupBy(q => q.UserId)
                    .ToDictionary(g => g.Key, g => g.Count());

                var queriesByHour = queryHistory
                    .GroupBy(q => q.ExecutedAt.Hour)
                    .ToDictionary(g => g.Key.ToString(), g => g.Count());

                var mostPopularQueries = queryHistory
                    .GroupBy(q => q.Query.ToLowerInvariant())
                    .OrderByDescending(g => g.Count())
                    .Take(10)
                    .Select(g => g.Key)
                    .ToList();

                var slowestQueries = queryHistory
                    .OrderByDescending(q => q.ExecutionTimeMs)
                    .Take(10)
                    .Select(q => q.Query)
                    .ToList();

                var report = new UsageReport
                {
                    From = from,
                    To = to,
                    TotalQueries = totalQueries,
                    UniqueUsers = uniqueUsers,
                    AverageResponseTime = averageResponseTime,
                    SuccessRate = totalQueries > 0 ? (double)successfulQueries / totalQueries : 0,
                    QueriesByUser = queriesByUser,
                    QueriesByHour = queriesByHour,
                    MostPopularQueries = mostPopularQueries,
                    SlowestQueries = slowestQueries
                };

                _logger.LogInformation("Usage report generated for period {From} to {To}", from, to);
                return report;
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating usage report for period {From} to {To}", from, to);
            return new UsageReport { From = from, To = to };
        }
    }

    private AuditLogEntry MapToAuditLogEntry(Infrastructure.Data.Entities.AuditLogEntity entity)
    {
        object? details = null;
        if (!string.IsNullOrEmpty(entity.Details))
        {
            try
            {
                details = JsonSerializer.Deserialize<object>(entity.Details);
            }
            catch
            {
                details = entity.Details; // Fallback to string if deserialization fails
            }
        }

        return new AuditLogEntry
        {
            Id = entity.Id,
            Action = entity.Action,
            UserId = entity.UserId,
            EntityType = entity.EntityType,
            EntityId = entity.EntityId,
            Details = details,
            IpAddress = entity.IpAddress,
            UserAgent = entity.UserAgent,
            Timestamp = entity.Timestamp,
            Severity = entity.Severity ?? "Info"
        };
    }

    private double CalculateSecurityScore(int failedLogins, int successfulLogins, int suspiciousActivities)
    {
        // Simple security score calculation
        var totalLogins = failedLogins + successfulLogins;
        if (totalLogins == 0) return 1.0;

        var failureRate = (double)failedLogins / totalLogins;
        var suspiciousRate = totalLogins > 0 ? (double)suspiciousActivities / totalLogins : 0;

        // Score decreases with higher failure and suspicious activity rates
        var score = 1.0 - (failureRate * 0.5) - (suspiciousRate * 0.3);
        return Math.Max(0.0, Math.Min(1.0, score));
    }

    // =============================================================================
    // MISSING INTERFACE METHOD IMPLEMENTATIONS
    // =============================================================================

    /// <summary>
    /// Log action with proper interface signature
    /// </summary>
    public async Task LogActionAsync(string action, string userId, string entityType, string entityId, Dictionary<string, object>? metadata = null)
    {
        var details = metadata != null ? JsonSerializer.Serialize(metadata) : null;
        await LogSecurityEventAsync(action, userId, details, null, "Info");
    }

    /// <summary>
    /// Log error with proper interface signature
    /// </summary>
    public async Task LogErrorAsync(string error, string userId, string? details = null, Dictionary<string, object>? metadata = null)
    {
        await LogSecurityEventAsync("ERROR", userId, details ?? error, null, "Error");
    }

    /// <summary>
    /// Get audit logs with proper interface signature (Data.IAuditService)
    /// </summary>
    public async Task<IEnumerable<object>> GetAuditLogsAsync(string userId, DateTime? from = null, DateTime? to = null, int pageSize = 100, int page = 1)
    {
        var auditLogs = await GetAuditLogsAsync(userId, from, to, null); // Call the existing method

        // Apply pagination
        var pagedResults = auditLogs
            .Skip((page - 1) * pageSize)
            .Take(pageSize);

        return pagedResults.Cast<object>();
    }

    /// <summary>
    /// Get audit logs with proper interface signature (Security.IAuditService)
    /// </summary>
    public async Task<IEnumerable<object>> GetAuditLogsAsync(string? userId = null, DateTime? from = null, DateTime? to = null)
    {
        var auditLogs = await GetAuditLogsAsync(userId, from, to, null); // Call the existing method
        return auditLogs.Cast<object>();
    }

    /// <summary>
    /// Get query history for a user (Data.IAuditService)
    /// </summary>
    public async Task<IEnumerable<object>> GetQueryHistoryAsync(string userId, int pageSize = 10, int page = 1)
    {
        try
        {
            return await _contextFactory.ExecuteWithContextAsync(ContextType.Query, async dbContext =>
            {
                var queryContext = (QueryDbContext)dbContext;
                var queryHistory = await queryContext.QueryHistory
                    .Where(q => q.UserId == userId)
                    .OrderByDescending(q => q.ExecutedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                return queryHistory.Cast<object>();
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving query history for user: {UserId}", userId);
            return new List<object>();
        }
    }
}
