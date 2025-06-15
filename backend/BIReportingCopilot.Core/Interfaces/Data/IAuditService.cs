namespace BIReportingCopilot.Core.Interfaces.Data;

/// <summary>
/// Interface for audit logging services
/// </summary>
public interface IAuditService
{
    /// <summary>
    /// Log a general action
    /// </summary>
    Task LogAsync(string action, string userId, string? entityType = null, string? entityId = null,
                  object? details = null, string? ipAddress = null, string? userAgent = null);

    /// <summary>
    /// Log a query execution
    /// </summary>
    Task LogQueryAsync(string userId, string sessionId, string naturalLanguageQuery, string generatedSQL,
                       bool successful, int executionTimeMs, string? error = null);

    /// <summary>
    /// Log a security event
    /// </summary>
    Task LogSecurityEventAsync(string eventType, string userId, string? details = null,
                               string? ipAddress = null, string severity = "Info");

    /// <summary>
    /// Get audit logs for a user
    /// </summary>
    Task<IEnumerable<object>> GetAuditLogsAsync(string userId, DateTime? from = null, DateTime? to = null, int pageSize = 100, int page = 1);

    /// <summary>
    /// Get query history for a user
    /// </summary>
    Task<IEnumerable<object>> GetQueryHistoryAsync(string userId, int pageSize = 10, int page = 1);
}
