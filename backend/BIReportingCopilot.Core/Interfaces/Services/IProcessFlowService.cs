using BIReportingCopilot.Core.Models.ProcessFlow;

namespace BIReportingCopilot.Core.Interfaces.Services;

/// <summary>
/// Service interface for managing process flow tracking and transparency
/// </summary>
public interface IProcessFlowService
{
    /// <summary>
    /// Start a new process flow session
    /// </summary>
    Task<ProcessFlowSession> StartSessionAsync(string sessionId, string userId, string query, string queryType = "enhanced", string? conversationId = null, string? messageId = null);

    /// <summary>
    /// Update process flow session status and metadata
    /// </summary>
    Task UpdateSessionAsync(string sessionId, ProcessFlowSessionUpdate update);

    /// <summary>
    /// Complete a process flow session
    /// </summary>
    Task CompleteSessionAsync(string sessionId, ProcessFlowSessionCompletion completion);

    /// <summary>
    /// Add or update a process flow step
    /// </summary>
    Task<ProcessFlowStep> AddOrUpdateStepAsync(string sessionId, ProcessFlowStepUpdate stepUpdate);

    /// <summary>
    /// Update step status
    /// </summary>
    Task UpdateStepStatusAsync(string sessionId, string stepId, string status, string? errorMessage = null);

    /// <summary>
    /// Add a log entry to the process flow
    /// </summary>
    Task AddLogAsync(string sessionId, string? stepId, string logLevel, string message, string? details = null, string? source = null);

    /// <summary>
    /// Set transparency information for a session
    /// </summary>
    Task SetTransparencyAsync(string sessionId, ProcessFlowTransparency transparency);

    /// <summary>
    /// Get complete process flow session with all steps and logs
    /// </summary>
    Task<ProcessFlowSession?> GetSessionAsync(string sessionId);

    /// <summary>
    /// Get process flow sessions for a user
    /// </summary>
    Task<IEnumerable<ProcessFlowSession>> GetUserSessionsAsync(string userId, int limit = 50);

    /// <summary>
    /// Get process flow sessions by conversation
    /// </summary>
    Task<IEnumerable<ProcessFlowSession>> GetConversationSessionsAsync(string conversationId);

    /// <summary>
    /// Get step performance analytics
    /// </summary>
    Task<IEnumerable<ProcessFlowStepPerformance>> GetStepPerformanceAsync();

    /// <summary>
    /// Get session summary statistics
    /// </summary>
    Task<ProcessFlowSummary> GetSessionSummaryAsync(string sessionId);

    /// <summary>
    /// Delete old process flow data (cleanup)
    /// </summary>
    Task CleanupOldDataAsync(TimeSpan retentionPeriod);

    /// <summary>
    /// Get token usage trends analytics
    /// </summary>
    Task<IEnumerable<object>> GetTokenUsageTrendsAsync(DateTime startDate, DateTime endDate, string? userId = null, string? requestType = null);

    /// <summary>
    /// Get top users by token usage analytics
    /// </summary>
    Task<IEnumerable<object>> GetTopUsersByTokenUsageAsync(DateTime startDate, DateTime endDate, int topCount = 10);

    /// <summary>
    /// Get success analytics
    /// </summary>
    Task<object> GetSuccessAnalyticsAsync(DateTime startDate, DateTime endDate, string? userId = null);
}

/// <summary>
/// Process flow session update model
/// </summary>
public class ProcessFlowSessionUpdate
{
    public string? Status { get; set; }
    public DateTime? EndTime { get; set; }
    public long? TotalDurationMs { get; set; }
    public decimal? OverallConfidence { get; set; }
    public string? GeneratedSQL { get; set; }
    public string? ExecutionResult { get; set; }
    public string? ErrorMessage { get; set; }
    public string? Metadata { get; set; }
}

/// <summary>
/// Process flow session completion model
/// </summary>
public class ProcessFlowSessionCompletion
{
    public string Status { get; set; } = "completed";
    public DateTime EndTime { get; set; } = DateTime.UtcNow;
    public long TotalDurationMs { get; set; }
    public decimal? OverallConfidence { get; set; }
    public string? GeneratedSQL { get; set; }
    public string? ExecutionResult { get; set; }
    public string? ErrorMessage { get; set; }
    public string? Metadata { get; set; }
}

/// <summary>
/// Process flow step update model
/// </summary>
public class ProcessFlowStepUpdate
{
    public string StepId { get; set; } = string.Empty;
    public string? ParentStepId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int StepOrder { get; set; }
    public string Status { get; set; } = "pending";
    public DateTime? StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public long? DurationMs { get; set; }
    public decimal? Confidence { get; set; }
    public string? InputData { get; set; }
    public string? OutputData { get; set; }
    public string? ErrorMessage { get; set; }
    public string? Metadata { get; set; }
    public int RetryCount { get; set; } = 0;
}

/// <summary>
/// Process flow step performance analytics
/// </summary>
public class ProcessFlowStepPerformance
{
    public string StepId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int ExecutionCount { get; set; }
    public double AvgDurationMs { get; set; }
    public long MinDurationMs { get; set; }
    public long MaxDurationMs { get; set; }
    public double AvgConfidence { get; set; }
    public int SuccessCount { get; set; }
    public int ErrorCount { get; set; }
    public double SuccessRate { get; set; }
}

/// <summary>
/// Process flow session summary
/// </summary>
public class ProcessFlowSummary
{
    public string SessionId { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string Query { get; set; } = string.Empty;
    public string QueryType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public long? TotalDurationMs { get; set; }
    public decimal? OverallConfidence { get; set; }
    public string? ConversationId { get; set; }
    public string? MessageId { get; set; }
    public string? Model { get; set; }
    public int? TotalTokens { get; set; }
    public decimal? EstimatedCost { get; set; }
    public int ApiCallCount { get; set; }
    public int TotalSteps { get; set; }
    public int CompletedSteps { get; set; }
    public int ErrorSteps { get; set; }
    public int TotalLogs { get; set; }
    public int ErrorLogs { get; set; }
}
