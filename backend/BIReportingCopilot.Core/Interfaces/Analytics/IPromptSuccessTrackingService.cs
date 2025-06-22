using BIReportingCopilot.Core.Models.Analytics;

namespace BIReportingCopilot.Core.Interfaces.Analytics;

/// <summary>
/// Service for tracking prompt generation success rates and analytics
/// </summary>
public interface IPromptSuccessTrackingService
{
    /// <summary>
    /// Track a prompt generation session with success metrics
    /// </summary>
    Task<long> TrackPromptSessionAsync(PromptSessionTracking session, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update session with SQL execution results
    /// </summary>
    Task UpdateSQLExecutionResultAsync(long sessionId, SQLExecutionResult result, CancellationToken cancellationToken = default);

    /// <summary>
    /// Record user feedback for a prompt session
    /// </summary>
    Task RecordUserFeedbackAsync(long sessionId, UserFeedback feedback, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get success rate for a specific template
    /// </summary>
    Task<TemplateSuccessMetrics> GetTemplateSuccessRateAsync(string templateKey, TimeSpan? timeWindow = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get success rate for a specific intent type
    /// </summary>
    Task<IntentSuccessMetrics> GetIntentSuccessRateAsync(string intentType, TimeSpan? timeWindow = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get overall system success metrics
    /// </summary>
    Task<SystemSuccessMetrics> GetSystemSuccessMetricsAsync(TimeSpan? timeWindow = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get user-specific success metrics
    /// </summary>
    Task<UserSuccessMetrics> GetUserSuccessMetricsAsync(string userId, TimeSpan? timeWindow = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get trending success metrics over time
    /// </summary>
    Task<List<SuccessTrendPoint>> GetSuccessTrendsAsync(TimeSpan timeWindow, TimeSpan granularity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get top performing templates
    /// </summary>
    Task<List<TemplatePerformanceRanking>> GetTopPerformingTemplatesAsync(int topCount = 10, TimeSpan? timeWindow = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get underperforming templates that need improvement
    /// </summary>
    Task<List<TemplatePerformanceRanking>> GetUnderperformingTemplatesAsync(double successRateThreshold = 0.7, int minUsageCount = 10, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get detailed session analytics for admin dashboard
    /// </summary>
    Task<SessionAnalytics> GetSessionAnalyticsAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Export success tracking data for analysis
    /// </summary>
    Task<byte[]> ExportSuccessDataAsync(DateTime startDate, DateTime endDate, ExportFormat format = ExportFormat.CSV, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get real-time performance metrics for monitoring
    /// </summary>
    Task<RealTimeMetrics> GetRealTimeMetricsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get performance alerts based on thresholds
    /// </summary>
    Task<List<PerformanceAlert>> GetPerformanceAlertsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Update template performance metrics (called by background service)
    /// </summary>
    Task UpdateTemplatePerformanceMetricsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Clean up old tracking data based on retention policy
    /// </summary>
    Task CleanupOldTrackingDataAsync(int retentionDays = 90, CancellationToken cancellationToken = default);
}

/// <summary>
/// Enums and supporting types
/// </summary>
public enum ExportFormat
{
    CSV,
    JSON,
    Excel
}

public enum AlertSeverity
{
    Info,
    Warning,
    Critical
}

public enum MetricType
{
    SuccessRate,
    ResponseTime,
    ConfidenceScore,
    UserSatisfaction,
    ErrorRate
}
