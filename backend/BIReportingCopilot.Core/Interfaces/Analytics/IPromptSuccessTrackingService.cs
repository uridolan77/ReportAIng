namespace BIReportingCopilot.Core.Interfaces.Analytics;

/// <summary>
/// Service for tracking prompt generation success rates and analytics
/// </summary>
public interface IPromptSuccessTrackingService
{
    /// <summary>
    /// Track a prompt generation session with success metrics
    /// </summary>
    Task<long> TrackPromptSessionAsync(PromptSuccessTrackingRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update session with SQL execution results
    /// </summary>
    Task UpdateSQLExecutionResultAsync(long sessionId, bool success, string? errorMessage = null, int? executionTimeMs = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Record user feedback for a prompt session
    /// </summary>
    Task RecordUserFeedbackAsync(long sessionId, int rating, string? comments = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get success tracking sessions for a user
    /// </summary>
    Task<IEnumerable<PromptSuccessTrackingRecord>> GetUserSessionsAsync(string userId, DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get success tracking sessions by session ID
    /// </summary>
    Task<PromptSuccessTrackingRecord?> GetSessionAsync(string sessionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get success analytics for a date range
    /// </summary>
    Task<PromptSuccessAnalytics> GetSuccessAnalyticsAsync(DateTime startDate, DateTime endDate, string? userId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get template performance metrics
    /// </summary>
    Task<IEnumerable<TemplatePerformanceMetrics>> GetTemplatePerformanceAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get intent classification performance
    /// </summary>
    Task<IEnumerable<IntentPerformanceMetrics>> GetIntentPerformanceAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get domain classification performance
    /// </summary>
    Task<IEnumerable<DomainPerformanceMetrics>> GetDomainPerformanceAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get success trends over time
    /// </summary>
    Task<IEnumerable<SuccessTrendPoint>> GetSuccessTrendsAsync(DateTime startDate, DateTime endDate, string groupBy = "day", CancellationToken cancellationToken = default);
}

/// <summary>
/// Request model for tracking prompt success
/// </summary>
public class PromptSuccessTrackingRequest
{
    public string SessionId { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string UserQuestion { get; set; } = string.Empty;
    public string GeneratedPrompt { get; set; } = string.Empty;
    public string? TemplateUsed { get; set; }
    public string IntentClassified { get; set; } = string.Empty;
    public string DomainClassified { get; set; } = string.Empty;
    public string? TablesRetrieved { get; set; }
    public string? GeneratedSQL { get; set; }
    public int ProcessingTimeMs { get; set; }
    public decimal ConfidenceScore { get; set; }
}

/// <summary>
/// Success analytics summary
/// </summary>
public class PromptSuccessAnalytics
{
    public int TotalSessions { get; set; }
    public int SuccessfulSessions { get; set; }
    public decimal OverallSuccessRate { get; set; }
    public decimal AverageConfidenceScore { get; set; }
    public int AverageProcessingTimeMs { get; set; }
    public int SessionsWithUserFeedback { get; set; }
    public decimal AverageUserRating { get; set; }
    public Dictionary<string, int> IntentBreakdown { get; set; } = new();
    public Dictionary<string, int> DomainBreakdown { get; set; } = new();
    public Dictionary<string, decimal> TemplateSuccessRates { get; set; } = new();
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}

/// <summary>
/// Template performance metrics
/// </summary>
public class TemplatePerformanceMetrics
{
    public string TemplateName { get; set; } = string.Empty;
    public int TotalUsage { get; set; }
    public int SuccessfulUsage { get; set; }
    public decimal SuccessRate { get; set; }
    public decimal AverageConfidenceScore { get; set; }
    public int AverageProcessingTimeMs { get; set; }
    public decimal? AverageUserRating { get; set; }
}

/// <summary>
/// Intent performance metrics
/// </summary>
public class IntentPerformanceMetrics
{
    public string IntentType { get; set; } = string.Empty;
    public int TotalSessions { get; set; }
    public int SuccessfulSessions { get; set; }
    public decimal SuccessRate { get; set; }
    public decimal AverageConfidenceScore { get; set; }
    public int AverageProcessingTimeMs { get; set; }
}

/// <summary>
/// Domain performance metrics
/// </summary>
public class DomainPerformanceMetrics
{
    public string DomainName { get; set; } = string.Empty;
    public int TotalSessions { get; set; }
    public int SuccessfulSessions { get; set; }
    public decimal SuccessRate { get; set; }
    public decimal AverageConfidenceScore { get; set; }
    public int AverageProcessingTimeMs { get; set; }
}

/// <summary>
/// Prompt success tracking record
/// </summary>
public class PromptSuccessTrackingRecord
{
    public long Id { get; set; }
    public string SessionId { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string UserQuestion { get; set; } = string.Empty;
    public string GeneratedPrompt { get; set; } = string.Empty;
    public string? TemplateUsed { get; set; }
    public string IntentClassified { get; set; } = string.Empty;
    public string DomainClassified { get; set; } = string.Empty;
    public string? TablesRetrieved { get; set; }
    public string? GeneratedSQL { get; set; }
    public bool SQLExecutionSuccess { get; set; }
    public string? SQLExecutionError { get; set; }
    public int? SQLExecutionTimeMs { get; set; }
    public int ProcessingTimeMs { get; set; }
    public decimal ConfidenceScore { get; set; }
    public int? UserFeedbackRating { get; set; }
    public string? UserFeedbackComments { get; set; }
    public DateTime CreatedDate { get; set; }
}

/// <summary>
/// Success trend data point
/// </summary>
public class SuccessTrendPoint
{
    public DateTime Period { get; set; }
    public int TotalSessions { get; set; }
    public int SuccessfulSessions { get; set; }
    public decimal SuccessRate { get; set; }
    public decimal AverageConfidenceScore { get; set; }
    public int AverageProcessingTimeMs { get; set; }
}
