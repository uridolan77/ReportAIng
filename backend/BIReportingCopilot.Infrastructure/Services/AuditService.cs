using BIReportingCopilot.Core.Interfaces;
using BIReportingCopilot.Core.Models;
using BIReportingCopilot.Infrastructure.Data;
using BIReportingCopilot.Infrastructure.Data.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace BIReportingCopilot.Infrastructure.Services;

public class AuditService : IAuditService
{
    private readonly ILogger<AuditService> _logger;
    private readonly BICopilotContext _context;

    public AuditService(ILogger<AuditService> logger, BICopilotContext context)
    {
        _logger = logger;
        _context = context;
    }

    public async Task LogAsync(string action, string userId, string? entityType = null, string? entityId = null,
                              object? details = null, string? ipAddress = null, string? userAgent = null)
    {
        try
        {
            var auditEntry = new AuditLogEntity
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

            _context.AuditLog.Add(auditEntry);
            await _context.SaveChangesAsync();

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
            // Log to audit table
            var auditDetails = new
            {
                SessionId = sessionId,
                NaturalLanguageQuery = naturalLanguageQuery,
                GeneratedSQL = generatedSQL,
                ExecutionTimeMs = executionTimeMs,
                Error = error
            };

            await LogAsync("QUERY_EXECUTED", userId, "Query", sessionId, auditDetails);

            // Also log to query history
            var queryHistory = new Core.Models.QueryHistoryEntity
            {
                UserId = userId,
                SessionId = sessionId,
                Query = naturalLanguageQuery,
                GeneratedSql = generatedSQL,
                ExecutionTimeMs = executionTimeMs,
                IsSuccessful = successful,
                ErrorMessage = error,
                ExecutedAt = DateTime.UtcNow
            };

            _context.QueryHistories.Add(queryHistory);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Query execution logged for user: {UserId}, successful: {Successful}", userId, successful);
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
            var auditEntry = new AuditLogEntity
            {
                Action = $"SECURITY_{eventType}",
                UserId = userId,
                EntityType = "Security",
                Details = details,
                IpAddress = ipAddress,
                Timestamp = DateTime.UtcNow,
                Severity = severity
            };

            _context.AuditLog.Add(auditEntry);
            await _context.SaveChangesAsync();

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
            var query = _context.AuditLog.AsQueryable();

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
            var securityEvents = await _context.AuditLog
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
            var queryHistory = await _context.QueryHistory
                .Where(q => q.QueryTimestamp >= from && q.QueryTimestamp <= to)
                .ToListAsync();

            var totalQueries = queryHistory.Count;
            var uniqueUsers = queryHistory.Select(q => q.UserId).Distinct().Count();
            var successfulQueries = queryHistory.Count(q => q.IsSuccessful);
            var averageResponseTime = queryHistory.Where(q => q.ExecutionTimeMs.HasValue)
                .Average(q => q.ExecutionTimeMs!.Value);

            var queriesByUser = queryHistory
                .GroupBy(q => q.UserId)
                .ToDictionary(g => g.Key, g => g.Count());

            var queriesByHour = queryHistory
                .GroupBy(q => q.QueryTimestamp.Hour)
                .ToDictionary(g => g.Key.ToString(), g => g.Count());

            var mostPopularQueries = queryHistory
                .GroupBy(q => q.NaturalLanguageQuery.ToLowerInvariant())
                .OrderByDescending(g => g.Count())
                .Take(10)
                .Select(g => g.Key)
                .ToList();

            var slowestQueries = queryHistory
                .Where(q => q.ExecutionTimeMs.HasValue)
                .OrderByDescending(q => q.ExecutionTimeMs!.Value)
                .Take(10)
                .Select(q => q.NaturalLanguageQuery)
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
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating usage report for period {From} to {To}", from, to);
            return new UsageReport { From = from, To = to };
        }
    }

    private AuditLogEntry MapToAuditLogEntry(AuditLogEntity entity)
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
}
