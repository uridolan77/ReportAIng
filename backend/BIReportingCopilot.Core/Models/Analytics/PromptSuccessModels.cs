namespace BIReportingCopilot.Core.Models.Analytics;

/// <summary>
/// Model for tracking a complete prompt generation session
/// </summary>
public class PromptSessionTracking
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
    public bool? SQLExecutionSuccess { get; set; }
    public string? SQLExecutionError { get; set; }
    public int? UserFeedbackRating { get; set; }
    public string? UserFeedbackComments { get; set; }
    public int ProcessingTimeMs { get; set; }
    public decimal ConfidenceScore { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? UpdatedDate { get; set; }
}

/// <summary>
/// Model for SQL execution results
/// </summary>
public class SQLExecutionResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public int ExecutionTimeMs { get; set; }
    public int RowsReturned { get; set; }
    public string? GeneratedSQL { get; set; }
}

/// <summary>
/// Model for user feedback
/// </summary>
public class UserFeedback
{
    public int Rating { get; set; } // 1-5 scale
    public string? Comments { get; set; }
    public bool WasHelpful { get; set; }
    public List<string> IssueCategories { get; set; } = new();
    public string? SuggestedImprovement { get; set; }
}

/// <summary>
/// Template success metrics
/// </summary>
public class TemplateSuccessMetrics
{
    public string TemplateKey { get; set; } = string.Empty;
    public string TemplateName { get; set; } = string.Empty;
    public string IntentType { get; set; } = string.Empty;
    public int TotalUsages { get; set; }
    public int SuccessfulUsages { get; set; }
    public decimal SuccessRate { get; set; }
    public decimal AverageConfidenceScore { get; set; }
    public int AverageProcessingTimeMs { get; set; }
    public decimal? AverageUserRating { get; set; }
    public DateTime? LastUsedDate { get; set; }
    public TimeSpan AnalysisWindow { get; set; }
}

/// <summary>
/// Intent type success metrics
/// </summary>
public class IntentSuccessMetrics
{
    public string IntentType { get; set; } = string.Empty;
    public int TotalQueries { get; set; }
    public int SuccessfulQueries { get; set; }
    public decimal SuccessRate { get; set; }
    public decimal AverageConfidenceScore { get; set; }
    public int AverageProcessingTimeMs { get; set; }
    public List<string> TopTemplatesUsed { get; set; } = new();
    public List<string> CommonFailureReasons { get; set; } = new();
    public TimeSpan AnalysisWindow { get; set; }
}

/// <summary>
/// System-wide success metrics
/// </summary>
public class SystemSuccessMetrics
{
    public int TotalQueries { get; set; }
    public int SuccessfulQueries { get; set; }
    public decimal OverallSuccessRate { get; set; }
    public decimal AverageConfidenceScore { get; set; }
    public int AverageProcessingTimeMs { get; set; }
    public int UniqueUsers { get; set; }
    public decimal AverageUserRating { get; set; }
    public Dictionary<string, decimal> SuccessRateByIntent { get; set; } = new();
    public Dictionary<string, decimal> SuccessRateByDomain { get; set; } = new();
    public Dictionary<string, int> ErrorCategoryCounts { get; set; } = new();
    public TimeSpan AnalysisWindow { get; set; }
    public DateTime LastUpdated { get; set; }
}

/// <summary>
/// User-specific success metrics
/// </summary>
public class UserSuccessMetrics
{
    public string UserId { get; set; } = string.Empty;
    public int TotalQueries { get; set; }
    public int SuccessfulQueries { get; set; }
    public decimal SuccessRate { get; set; }
    public decimal AverageConfidenceScore { get; set; }
    public List<string> PreferredIntents { get; set; } = new();
    public List<string> PreferredDomains { get; set; } = new();
    public List<string> MostUsedTemplates { get; set; } = new();
    public decimal AverageSessionDuration { get; set; }
    public DateTime FirstQueryDate { get; set; }
    public DateTime LastQueryDate { get; set; }
    public TimeSpan AnalysisWindow { get; set; }
}

/// <summary>
/// Success trend data point
/// </summary>
public class SuccessTrendPoint
{
    public DateTime Timestamp { get; set; }
    public decimal SuccessRate { get; set; }
    public int TotalQueries { get; set; }
    public int SuccessfulQueries { get; set; }
    public decimal AverageConfidenceScore { get; set; }
    public int AverageProcessingTimeMs { get; set; }
    public decimal AverageUserRating { get; set; }
}

/// <summary>
/// Template performance ranking
/// </summary>
public class TemplatePerformanceRanking
{
    public string TemplateKey { get; set; } = string.Empty;
    public string TemplateName { get; set; } = string.Empty;
    public string IntentType { get; set; } = string.Empty;
    public decimal SuccessRate { get; set; }
    public int UsageCount { get; set; }
    public decimal AverageUserRating { get; set; }
    public int AverageProcessingTimeMs { get; set; }
    public decimal PerformanceScore { get; set; } // Composite score
    public int Rank { get; set; }
    public string PerformanceCategory { get; set; } = string.Empty; // "Excellent", "Good", "Needs Improvement"
}

/// <summary>
/// Session analytics for admin dashboard
/// </summary>
public class SessionAnalytics
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int TotalSessions { get; set; }
    public int UniqueSessions { get; set; }
    public int UniqueUsers { get; set; }
    public decimal AverageSessionDuration { get; set; }
    public Dictionary<string, int> SessionsByHour { get; set; } = new();
    public Dictionary<string, int> SessionsByDay { get; set; } = new();
    public Dictionary<string, int> SessionsByIntent { get; set; } = new();
    public Dictionary<string, int> SessionsByDomain { get; set; } = new();
    public List<TopUserActivity> TopUsers { get; set; } = new();
    public List<PopularQuery> PopularQueries { get; set; } = new();
}

/// <summary>
/// Real-time metrics for monitoring
/// </summary>
public class RealTimeMetrics
{
    public DateTime Timestamp { get; set; }
    public int ActiveSessions { get; set; }
    public int QueriesLastHour { get; set; }
    public decimal SuccessRateLastHour { get; set; }
    public int AverageResponseTimeMs { get; set; }
    public int ErrorsLastHour { get; set; }
    public decimal SystemLoad { get; set; }
    public int ConcurrentUsers { get; set; }
    public Dictionary<string, object> AdditionalMetrics { get; set; } = new();
}

/// <summary>
/// Performance alert
/// </summary>
public class PerformanceAlert
{
    public string AlertId { get; set; } = string.Empty;
    public string AlertType { get; set; } = string.Empty;
    public AlertSeverity Severity { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string MetricName { get; set; } = string.Empty;
    public decimal CurrentValue { get; set; }
    public decimal ThresholdValue { get; set; }
    public DateTime TriggeredAt { get; set; }
    public bool IsResolved { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public Dictionary<string, object> AdditionalData { get; set; } = new();
}

/// <summary>
/// Supporting models
/// </summary>
public class TopUserActivity
{
    public string UserId { get; set; } = string.Empty;
    public int QueryCount { get; set; }
    public decimal SuccessRate { get; set; }
    public DateTime LastActivity { get; set; }
}

public class PopularQuery
{
    public string QueryPattern { get; set; } = string.Empty;
    public int Count { get; set; }
    public decimal SuccessRate { get; set; }
    public string IntentType { get; set; } = string.Empty;
}

public enum AlertSeverity
{
    Info,
    Warning,
    Critical
}

public enum ExportFormat
{
    CSV,
    JSON,
    Excel
}
