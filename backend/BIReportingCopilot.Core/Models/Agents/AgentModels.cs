using System.ComponentModel.DataAnnotations;

namespace BIReportingCopilot.Core.Models.Agents;

/// <summary>
/// Base agent capabilities and metadata
/// </summary>
public class AgentCapabilities
{
    public string AgentType { get; set; } = string.Empty;
    public List<string> SupportedOperations { get; set; } = new();
    public Dictionary<string, object> Metadata { get; set; } = new();
    public double PerformanceScore { get; set; } = 1.0;
    public DateTime LastHealthCheck { get; set; } = DateTime.UtcNow;
    public bool IsAvailable { get; set; } = true;
}

/// <summary>
/// Agent execution context
/// </summary>
public class AgentContext
{
    public string CorrelationId { get; set; } = Guid.NewGuid().ToString();
    public string CurrentAgent { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string SessionId { get; set; } = string.Empty;
    public Dictionary<string, object> Properties { get; set; } = new();
    public DateTime StartTime { get; set; } = DateTime.UtcNow;
    public CancellationToken CancellationToken { get; set; } = default;
}

/// <summary>
/// Generic agent request
/// </summary>
public class AgentRequest
{
    public string RequestId { get; set; } = Guid.NewGuid().ToString();
    public string RequestType { get; set; } = string.Empty;
    public object Payload { get; set; } = new();
    public Dictionary<string, object> Parameters { get; set; } = new();
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Generic agent response
/// </summary>
public class AgentResponse
{
    public string RequestId { get; set; } = string.Empty;
    public bool Success { get; set; }
    public object? Result { get; set; }
    public string? ErrorMessage { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();
    public TimeSpan ExecutionTime { get; set; }
    public DateTime CompletedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Agent health status
/// </summary>
public class HealthStatus
{
    public bool IsHealthy { get; set; }
    public string Status { get; set; } = "Unknown";
    public List<string> Issues { get; set; } = new();
    public Dictionary<string, object> Metrics { get; set; } = new();
    public DateTime CheckedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Query intent analysis result
/// </summary>
public class QueryIntent
{
    public string QueryType { get; set; } = string.Empty; // SELECT, INSERT, UPDATE, DELETE, etc.
    public List<EntityReference> Entities { get; set; } = new();
    public BusinessContext BusinessContext { get; set; } = new();
    public double Confidence { get; set; }
    public List<QueryAmbiguity> Ambiguities { get; set; } = new();
    public QueryComplexity Complexity { get; set; } = new();
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Entity reference in query
/// </summary>
public class EntityReference
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty; // Table, Column, Function, etc.
    public string? Schema { get; set; }
    public double Confidence { get; set; }
    public List<string> Aliases { get; set; } = new();
}

/// <summary>
/// Business context for query
/// </summary>
public class BusinessContext
{
    public string Domain { get; set; } = string.Empty; // Finance, Sales, Marketing, etc.
    public List<string> BusinessTerms { get; set; } = new();
    public string? TimeFrame { get; set; }
    public List<string> Metrics { get; set; } = new();
    public Dictionary<string, object> Properties { get; set; } = new();
}

/// <summary>
/// Query ambiguity detection
/// </summary>
public class QueryAmbiguity
{
    public string Type { get; set; } = string.Empty; // EntityAmbiguity, TemporalAmbiguity, etc.
    public string Description { get; set; } = string.Empty;
    public List<string> PossibleResolutions { get; set; } = new();
    public double Severity { get; set; } // 0.0 to 1.0
}

/// <summary>
/// Query complexity assessment
/// </summary>
public class QueryComplexity
{
    public ComplexityLevel Level { get; set; } = ComplexityLevel.Low;
    public int TableCount { get; set; }
    public int JoinCount { get; set; }
    public int AggregationCount { get; set; }
    public int SubqueryCount { get; set; }
    public bool HasWindowFunctions { get; set; }
    public bool HasRecursiveCTE { get; set; }
    public double ComplexityScore { get; set; } // 0.0 to 1.0
    public Dictionary<string, object> Factors { get; set; } = new();
}

/// <summary>
/// Complexity levels
/// </summary>
public enum ComplexityLevel
{
    Low = 1,
    Medium = 2,
    High = 3,
    VeryHigh = 4
}

/// <summary>
/// Relevant table for query
/// </summary>
public class RelevantTable
{
    public string TableName { get; set; } = string.Empty;
    public string? SchemaName { get; set; }
    public double RelevanceScore { get; set; }
    public List<string> RelevantColumns { get; set; } = new();
    public string ReasonForInclusion { get; set; } = string.Empty;
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Schema context for query generation
/// </summary>
public class SchemaContext
{
    public List<RelevantTable> Tables { get; set; } = new();
    public List<JoinPath> AvailableJoins { get; set; } = new();
    public List<string> AvailableFunctions { get; set; } = new();
    public Dictionary<string, object> BusinessRules { get; set; } = new();
    public Dictionary<string, object> Constraints { get; set; } = new();
}

/// <summary>
/// Join path between tables
/// </summary>
public class JoinPath
{
    public string FromTable { get; set; } = string.Empty;
    public string ToTable { get; set; } = string.Empty;
    public string JoinType { get; set; } = "INNER"; // INNER, LEFT, RIGHT, FULL
    public List<JoinCondition> Conditions { get; set; } = new();
    public double PerformanceScore { get; set; }
    public bool IsRecommended { get; set; }
}

/// <summary>
/// Join condition
/// </summary>
public class JoinCondition
{
    public string LeftColumn { get; set; } = string.Empty;
    public string RightColumn { get; set; } = string.Empty;
    public string Operator { get; set; } = "=";
}

/// <summary>
/// Generated SQL result
/// </summary>
public class GeneratedSql
{
    public string Sql { get; set; } = string.Empty;
    public string Explanation { get; set; } = string.Empty;
    public double Confidence { get; set; }
    public List<string> Warnings { get; set; } = new();
    public PerformanceHints PerformanceHints { get; set; } = new();
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// SQL validation result
/// </summary>
public class SqlValidationResult
{
    public bool IsValid { get; set; }
    public List<ValidationError> Errors { get; set; } = new();
    public List<ValidationWarning> Warnings { get; set; } = new();
    public List<string> Suggestions { get; set; } = new();
    public double QualityScore { get; set; }
}

/// <summary>
/// Validation error
/// </summary>
public class ValidationError
{
    public string Type { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public int? LineNumber { get; set; }
    public int? ColumnNumber { get; set; }
    public string Severity { get; set; } = "Error";
}

/// <summary>
/// Validation warning
/// </summary>
public class ValidationWarning
{
    public string Type { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Recommendation { get; set; } = string.Empty;
    public string Severity { get; set; } = "Warning";
}

/// <summary>
/// Performance hints for SQL optimization
/// </summary>
public class PerformanceHints
{
    public List<string> IndexSuggestions { get; set; } = new();
    public List<string> OptimizationTips { get; set; } = new();
    public double EstimatedExecutionTime { get; set; }
    public double EstimatedResourceUsage { get; set; }
    public Dictionary<string, object> Metrics { get; set; } = new();
}

/// <summary>
/// Agent communication message envelope
/// </summary>
public class AgentMessage<T>
{
    public string CorrelationId { get; set; } = Guid.NewGuid().ToString();
    public string SourceAgent { get; set; } = string.Empty;
    public string TargetAgent { get; set; } = string.Empty;
    public T Payload { get; set; } = default!;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public AgentContext Context { get; set; } = new();
    public Dictionary<string, object> Headers { get; set; } = new();
}

/// <summary>
/// Agent communication log entry
/// </summary>
public class AgentCommunicationLog
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string CorrelationId { get; set; } = string.Empty;
    public string SourceAgent { get; set; } = string.Empty;
    public string TargetAgent { get; set; } = string.Empty;
    public string MessageType { get; set; } = string.Empty;
    public bool Success { get; set; }
    public long ExecutionTimeMs { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Complete query processing result
/// </summary>
public class QueryProcessingResult
{
    public string WorkflowId { get; set; } = string.Empty;
    public string OriginalQuery { get; set; } = string.Empty;
    public QueryIntent? QueryIntent { get; set; }
    public SchemaContext? SchemaContext { get; set; }
    public GeneratedSql? GeneratedSql { get; set; }
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public TimeSpan TotalExecutionTime { get; set; }
    public List<WorkflowStep> Steps { get; set; } = new();
}

/// <summary>
/// Workflow definition for agent coordination
/// </summary>
public class WorkflowDefinition
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<WorkflowStep> Steps { get; set; } = new();
    public Dictionary<string, object> Configuration { get; set; } = new();
}

/// <summary>
/// Individual workflow step
/// </summary>
public class WorkflowStep
{
    public string StepName { get; set; } = string.Empty;
    public string AgentType { get; set; } = string.Empty;
    public int Order { get; set; }
    public bool IsRequired { get; set; } = true;
    public object? Input { get; set; }
    public object? Output { get; set; }
    public string Status { get; set; } = "Pending";
    public string? ErrorMessage { get; set; }
    public TimeSpan ExecutionTime { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Workflow execution result
/// </summary>
public class WorkflowResult
{
    public string WorkflowId { get; set; } = string.Empty;
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public TimeSpan TotalExecutionTime { get; set; }
    public List<WorkflowStep> Steps { get; set; } = new();
}

/// <summary>
/// Workflow status monitoring
/// </summary>
public class WorkflowStatus
{
    public string WorkflowId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public double Progress { get; set; }
    public DateTime LastActivity { get; set; }
    public List<string> ActiveAgents { get; set; } = new();
    public Dictionary<string, object> Metrics { get; set; } = new();
}

/// <summary>
/// Workflow failure information
/// </summary>
public class WorkflowFailure
{
    public string WorkflowId { get; set; } = string.Empty;
    public string FailedStep { get; set; } = string.Empty;
    public string ErrorMessage { get; set; } = string.Empty;
    public DateTime FailureTime { get; set; }
    public Dictionary<string, object> Context { get; set; } = new();
}

/// <summary>
/// Recovery strategy definition
/// </summary>
public class RecoveryStrategy
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Dictionary<string, object> Parameters { get; set; } = new();
}

/// <summary>
/// Recovery attempt result
/// </summary>
public class RecoveryAttempt
{
    public string Strategy { get; set; } = string.Empty;
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public TimeSpan ExecutionTime { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Recovery operation result
/// </summary>
public class RecoveryResult
{
    public string WorkflowId { get; set; } = string.Empty;
    public bool Success { get; set; }
    public string OriginalError { get; set; } = string.Empty;
    public string? RecoveryStrategy { get; set; }
    public List<RecoveryAttempt> RecoveryAttempts { get; set; } = new();
}

/// <summary>
/// Orchestration metrics for monitoring and optimization
/// </summary>
public class OrchestrationMetrics
{
    public string? WorkflowId { get; set; }
    public int TotalRequests { get; set; }
    public int SuccessfulRequests { get; set; }
    public int FailedRequests { get; set; }
    public double AverageExecutionTime { get; set; }
    public Dictionary<string, int> AgentUtilization { get; set; } = new();
    public DateTime LastActivity { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();
}
