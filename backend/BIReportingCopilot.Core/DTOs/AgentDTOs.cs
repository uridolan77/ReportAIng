using BIReportingCopilot.Core.Models.BusinessContext;

namespace BIReportingCopilot.Core.DTOs;

// ============================================================================
// INTELLIGENT AGENTS API REQUEST/RESPONSE DTOS
// ============================================================================

/// <summary>
/// Request for agent orchestration
/// </summary>
public class OrchestrationRequest
{
    public string TaskType { get; set; } = string.Empty;
    public string? UserId { get; set; }
    public Dictionary<string, object> Parameters { get; set; } = new();
    public List<string> RequiredAgents { get; set; } = new();
    public string Priority { get; set; } = "Normal"; // "Low", "Normal", "High", "Critical"
    public TimeSpan? Timeout { get; set; }
}

/// <summary>
/// Result of agent orchestration
/// </summary>
public class AgentOrchestrationResult
{
    public string OrchestrationId { get; set; } = string.Empty;
    public string TaskType { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public TimeSpan TotalDuration { get; set; }
    public string Status { get; set; } = string.Empty; // "processing", "completed", "failed", "cancelled"
    public List<AgentResult> AgentResults { get; set; } = new();
    public Dictionary<string, object> Metadata { get; set; } = new();
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Individual agent result within orchestration
/// </summary>
public class AgentResult
{
    public string AgentId { get; set; } = string.Empty;
    public string TaskType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty; // "pending", "processing", "completed", "failed"
    public object? Result { get; set; }
    public TimeSpan ProcessingTime { get; set; }
    public double Confidence { get; set; }
    public string? ErrorMessage { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Agent capabilities information for API responses
/// </summary>
public class AgentCapabilitiesDto
{
    public string AgentId { get; set; } = string.Empty;
    public string AgentName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<string> Capabilities { get; set; } = new();
    public List<string> SupportedTaskTypes { get; set; } = new();
    public Dictionary<string, object> PerformanceMetrics { get; set; } = new();
    public string Version { get; set; } = "1.0";
    public bool IsAvailable { get; set; } = true;
    public DateTime LastHealthCheck { get; set; }
}

/// <summary>
/// Request for schema navigation
/// </summary>
public class SchemaNavigationRequest
{
    public string Query { get; set; } = string.Empty;
    public string? UserId { get; set; }
    public List<string>? PreferredSchemas { get; set; }
    public int MaxTables { get; set; } = 10;
    public bool IncludeRelationships { get; set; } = true;
    public string NavigationStrategy { get; set; } = "relevance"; // "relevance", "breadth", "depth"
}

/// <summary>
/// Result of schema navigation
/// </summary>
public class SchemaNavigationResult
{
    public string NavigationId { get; set; } = string.Empty;
    public string Query { get; set; } = string.Empty;
    public List<TableRelevanceInfo> RelevantTables { get; set; } = new();
    public List<AgentTableRelationship> Relationships { get; set; } = new();
    public List<string> NavigationPath { get; set; } = new();
    public double Confidence { get; set; }
    public TimeSpan ProcessingTime { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Table relevance information
/// </summary>
public class TableRelevanceInfo
{
    public string TableName { get; set; } = string.Empty;
    public string Schema { get; set; } = string.Empty;
    public double RelevanceScore { get; set; }
    public string Reason { get; set; } = string.Empty;
    public List<string> RelevantColumns { get; set; } = new();
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Table relationship information for agent navigation
/// </summary>
public class AgentTableRelationship
{
    public string FromTable { get; set; } = string.Empty;
    public string ToTable { get; set; } = string.Empty;
    public string RelationshipType { get; set; } = string.Empty; // "Foreign Key", "One-to-Many", "Many-to-Many"
    public string JoinCondition { get; set; } = string.Empty;
    public double Strength { get; set; } = 1.0;
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Request for query understanding
/// </summary>
public class QueryUnderstandingRequest
{
    public string Query { get; set; } = string.Empty;
    public string? UserId { get; set; }
    public string? Context { get; set; }
    public bool IncludeEntities { get; set; } = true;
    public bool IncludeIntent { get; set; } = true;
    public string Language { get; set; } = "en";
}

/// <summary>
/// Result of query understanding
/// </summary>
public class QueryUnderstandingResult
{
    public string AnalysisId { get; set; } = string.Empty;
    public string Query { get; set; } = string.Empty;
    public QueryIntent Intent { get; set; } = new();
    public List<ExtractedEntity> Entities { get; set; } = new();
    public string QueryComplexity { get; set; } = string.Empty; // "Simple", "Medium", "Complex", "Very Complex"
    public List<string> EstimatedTables { get; set; } = new();
    public double Confidence { get; set; }
    public TimeSpan ProcessingTime { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Extracted entity from query
/// </summary>
public class ExtractedEntity
{
    public string EntityType { get; set; } = string.Empty; // "TimeRange", "Metric", "Dimension", "Filter"
    public string Value { get; set; } = string.Empty;
    public double Confidence { get; set; }
    public object? Position { get; set; } // Start and end positions in the query
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Agent communication log entry
/// </summary>
public class AgentCommunicationLog
{
    public string LogId { get; set; } = string.Empty;
    public string FromAgent { get; set; } = string.Empty;
    public string ToAgent { get; set; } = string.Empty;
    public string MessageType { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string Status { get; set; } = string.Empty; // "sent", "delivered", "failed", "acknowledged"
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Agent performance metrics
/// </summary>
public class AgentPerformanceMetrics
{
    public string AgentId { get; set; } = string.Empty;
    public int TotalTasks { get; set; }
    public int CompletedTasks { get; set; }
    public int FailedTasks { get; set; }
    public double SuccessRate { get; set; }
    public double AverageResponseTime { get; set; }
    public double AverageConfidence { get; set; }
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public Dictionary<string, object> DetailedMetrics { get; set; } = new();
}

/// <summary>
/// Agent health status
/// </summary>
public class AgentHealthStatus
{
    public string AgentId { get; set; } = string.Empty;
    public bool IsHealthy { get; set; }
    public string Status { get; set; } = string.Empty; // "online", "offline", "degraded", "maintenance"
    public DateTime LastHealthCheck { get; set; }
    public List<string> Issues { get; set; } = new();
    public Dictionary<string, object> HealthMetrics { get; set; } = new();
}

/// <summary>
/// Agent configuration settings
/// </summary>
public class AgentConfiguration
{
    public string AgentId { get; set; } = string.Empty;
    public Dictionary<string, object> Settings { get; set; } = new();
    public List<string> EnabledFeatures { get; set; } = new();
    public Dictionary<string, double> Thresholds { get; set; } = new();
    public TimeSpan Timeout { get; set; } = TimeSpan.FromMinutes(5);
    public int MaxRetries { get; set; } = 3;
}

/// <summary>
/// Agent task queue status
/// </summary>
public class AgentTaskQueueStatus
{
    public string AgentId { get; set; } = string.Empty;
    public int PendingTasks { get; set; }
    public int ProcessingTasks { get; set; }
    public int CompletedTasks { get; set; }
    public double AverageWaitTime { get; set; }
    public double AverageProcessingTime { get; set; }
    public DateTime LastUpdate { get; set; }
}

/// <summary>
/// Multi-agent collaboration request
/// </summary>
public class CollaborationRequest
{
    public string CollaborationId { get; set; } = string.Empty;
    public List<string> ParticipatingAgents { get; set; } = new();
    public string TaskType { get; set; } = string.Empty;
    public Dictionary<string, object> SharedContext { get; set; } = new();
    public string CoordinationStrategy { get; set; } = "sequential"; // "sequential", "parallel", "hierarchical"
    public TimeSpan Timeout { get; set; } = TimeSpan.FromMinutes(10);
}

/// <summary>
/// Multi-agent collaboration result
/// </summary>
public class CollaborationResult
{
    public string CollaborationId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty; // "completed", "failed", "partial"
    public List<AgentResult> AgentContributions { get; set; } = new();
    public object? CombinedResult { get; set; }
    public TimeSpan TotalDuration { get; set; }
    public double OverallConfidence { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();
}
