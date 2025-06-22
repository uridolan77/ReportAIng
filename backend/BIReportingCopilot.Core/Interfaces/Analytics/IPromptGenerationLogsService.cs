namespace BIReportingCopilot.Core.Interfaces.Analytics;

/// <summary>
/// Service for logging and analyzing prompt generation activities
/// </summary>
public interface IPromptGenerationLogsService
{
    /// <summary>
    /// Log a prompt generation session
    /// </summary>
    Task<long> LogPromptGenerationAsync(PromptGenerationLogRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update prompt generation log with execution results
    /// </summary>
    Task UpdateExecutionResultsAsync(long logId, bool sqlGenerated, bool queryExecuted,
        string? errorMessage = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update prompt generation log with user feedback
    /// </summary>
    Task UpdateUserFeedbackAsync(long logId, decimal rating, string? feedback = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get prompt generation logs for a user
    /// </summary>
    Task<IEnumerable<PromptGenerationLogRecord>> GetUserLogsAsync(string userId,
        DateTime? startDate = null, DateTime? endDate = null, int? limit = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get prompt generation logs by session
    /// </summary>
    Task<IEnumerable<PromptGenerationLogRecord>> GetSessionLogsAsync(string sessionId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get prompt generation analytics
    /// </summary>
    Task<PromptGenerationAnalytics> GetAnalyticsAsync(DateTime startDate, DateTime endDate, 
        string? userId = null, string? intentType = null, string? domain = null, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get success rate analytics by template
    /// </summary>
    Task<IEnumerable<TemplateSuccessRate>> GetTemplateSuccessRatesAsync(DateTime startDate, DateTime endDate, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get performance metrics for prompt generation
    /// </summary>
    Task<PromptGenerationPerformanceMetrics> GetPerformanceMetricsAsync(DateTime startDate, DateTime endDate, 
        string? userId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get most common error patterns
    /// </summary>
    Task<IEnumerable<ErrorPattern>> GetErrorPatternsAsync(DateTime startDate, DateTime endDate, 
        int topCount = 10, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get prompt generation trends over time
    /// </summary>
    Task<IEnumerable<PromptGenerationTrend>> GetTrendsAsync(DateTime startDate, DateTime endDate, 
        string groupBy = "day", string? userId = null, CancellationToken cancellationToken = default);
}

/// <summary>
/// Request model for logging prompt generation
/// </summary>
public class PromptGenerationLogRequest
{
    public string UserId { get; set; } = string.Empty;
    public string UserQuestion { get; set; } = string.Empty;
    public string GeneratedPrompt { get; set; } = string.Empty;
    public string IntentType { get; set; } = string.Empty;
    public string Domain { get; set; } = string.Empty;
    public decimal ConfidenceScore { get; set; }
    public int TablesUsed { get; set; }
    public int GenerationTimeMs { get; set; }
    public string? TemplateUsed { get; set; }
    public bool WasSuccessful { get; set; } = true;
    public string? ErrorMessage { get; set; }
    public string? ExtractedEntities { get; set; }
    public string? TimeContext { get; set; }
    public int? TokensUsed { get; set; }
    public decimal? CostEstimate { get; set; }
    public string? ModelUsed { get; set; }
    public long? PromptTemplateId { get; set; }
    public string? SessionId { get; set; }
    public string? RequestId { get; set; }
}

/// <summary>
/// Prompt generation analytics summary
/// </summary>
public class PromptGenerationAnalytics
{
    public int TotalPrompts { get; set; }
    public int SuccessfulPrompts { get; set; }
    public int FailedPrompts { get; set; }
    public decimal SuccessRate { get; set; }
    public decimal AverageConfidenceScore { get; set; }
    public int AverageGenerationTimeMs { get; set; }
    public int AverageTablesUsed { get; set; }
    public int? TotalTokensUsed { get; set; }
    public decimal? TotalCost { get; set; }
    public Dictionary<string, int> IntentTypeBreakdown { get; set; } = new();
    public Dictionary<string, int> DomainBreakdown { get; set; } = new();
    public Dictionary<string, int> TemplateUsageBreakdown { get; set; } = new();
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}

/// <summary>
/// Template success rate analytics
/// </summary>
public class TemplateSuccessRate
{
    public string TemplateName { get; set; } = string.Empty;
    public int TotalUsage { get; set; }
    public int SuccessfulUsage { get; set; }
    public decimal SuccessRate { get; set; }
    public decimal AverageConfidenceScore { get; set; }
    public int AverageGenerationTimeMs { get; set; }
}

/// <summary>
/// Performance metrics for prompt generation
/// </summary>
public class PromptGenerationPerformanceMetrics
{
    public int AverageGenerationTimeMs { get; set; }
    public int MedianGenerationTimeMs { get; set; }
    public int P95GenerationTimeMs { get; set; }
    public int P99GenerationTimeMs { get; set; }
    public decimal AveragePromptLength { get; set; }
    public decimal AverageTokensUsed { get; set; }
    public decimal AverageCost { get; set; }
    public Dictionary<string, int> PerformanceBuckets { get; set; } = new();
}

/// <summary>
/// Error pattern analysis
/// </summary>
public class ErrorPattern
{
    public string ErrorType { get; set; } = string.Empty;
    public string ErrorMessage { get; set; } = string.Empty;
    public int Frequency { get; set; }
    public decimal Percentage { get; set; }
    public string? CommonIntentType { get; set; }
    public string? CommonDomain { get; set; }
}

/// <summary>
/// Prompt generation log record
/// </summary>
public class PromptGenerationLogRecord
{
    public long Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string UserQuestion { get; set; } = string.Empty;
    public string GeneratedPrompt { get; set; } = string.Empty;
    public int PromptLength { get; set; }
    public string IntentType { get; set; } = string.Empty;
    public string Domain { get; set; } = string.Empty;
    public decimal ConfidenceScore { get; set; }
    public int TablesUsed { get; set; }
    public int GenerationTimeMs { get; set; }
    public string? TemplateUsed { get; set; }
    public bool WasSuccessful { get; set; }
    public string? ErrorMessage { get; set; }
    public bool SQLGenerated { get; set; }
    public bool QueryExecuted { get; set; }
    public string? ExtractedEntities { get; set; }
    public string? TimeContext { get; set; }
    public int? TokensUsed { get; set; }
    public decimal? CostEstimate { get; set; }
    public string? ModelUsed { get; set; }
    public long? PromptTemplateId { get; set; }
    public decimal? UserRating { get; set; }
    public string? UserFeedback { get; set; }
    public string? SessionId { get; set; }
    public string? RequestId { get; set; }
    public DateTime Timestamp { get; set; }
}

/// <summary>
/// Prompt generation trend data
/// </summary>
public class PromptGenerationTrend
{
    public DateTime Period { get; set; }
    public int TotalPrompts { get; set; }
    public int SuccessfulPrompts { get; set; }
    public decimal SuccessRate { get; set; }
    public decimal AverageConfidenceScore { get; set; }
    public int AverageGenerationTimeMs { get; set; }
}
