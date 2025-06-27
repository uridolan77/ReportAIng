namespace BIReportingCopilot.Core.Models.ProcessFlow;

/// <summary>
/// Process flow session model
/// </summary>
public class ProcessFlowSession
{
    public string Id { get; set; } = string.Empty;
    public string SessionId { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string Query { get; set; } = string.Empty;
    public string QueryType { get; set; } = "enhanced";
    public string Status { get; set; } = "running";
    public DateTime StartTime { get; set; } = DateTime.UtcNow;
    public DateTime? EndTime { get; set; }
    public long? TotalDurationMs { get; set; }
    public decimal? OverallConfidence { get; set; }
    public string? GeneratedSQL { get; set; }
    public string? ExecutionResult { get; set; }
    public string? ErrorMessage { get; set; }
    public string? Metadata { get; set; }
    public string? ConversationId { get; set; }
    public string? MessageId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public List<ProcessFlowStep> Steps { get; set; } = new();
    public List<ProcessFlowLog> Logs { get; set; } = new();
    public ProcessFlowTransparency? Transparency { get; set; }
}

/// <summary>
/// Process flow step model
/// </summary>
public class ProcessFlowStep
{
    public string Id { get; set; } = string.Empty;
    public string SessionId { get; set; } = string.Empty;
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
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public List<ProcessFlowLog> Logs { get; set; } = new();
    public List<ProcessFlowStep> SubSteps { get; set; } = new();
    public ProcessFlowStep? ParentStep { get; set; }
}

/// <summary>
/// Process flow log model
/// </summary>
public class ProcessFlowLog
{
    public string Id { get; set; } = string.Empty;
    public string SessionId { get; set; } = string.Empty;
    public string? StepId { get; set; }
    public string LogLevel { get; set; } = "Info";
    public string Message { get; set; } = string.Empty;
    public string? Details { get; set; }
    public string? Exception { get; set; }
    public string? Source { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Process flow transparency model
/// </summary>
public class ProcessFlowTransparency
{
    public string Id { get; set; } = string.Empty;
    public string SessionId { get; set; } = string.Empty;
    public string? Model { get; set; }
    public decimal? Temperature { get; set; }
    public int? MaxTokens { get; set; }
    public int? PromptTokens { get; set; }
    public int? CompletionTokens { get; set; }
    public int? TotalTokens { get; set; }
    public decimal? EstimatedCost { get; set; }
    public decimal? Confidence { get; set; }
    public long? AIProcessingTimeMs { get; set; }
    public int ApiCallCount { get; set; } = 0;
    public string? PromptDetails { get; set; }
    public string? ResponseAnalysis { get; set; }
    public string? QualityMetrics { get; set; }
    public string? OptimizationSuggestions { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Process flow status enumeration
/// </summary>
public static class ProcessFlowStatus
{
    public const string Pending = "pending";
    public const string Running = "running";
    public const string Completed = "completed";
    public const string Error = "error";
    public const string Cancelled = "cancelled";
    public const string Skipped = "skipped";
}

/// <summary>
/// Process flow log levels
/// </summary>
public static class ProcessFlowLogLevel
{
    public const string Debug = "Debug";
    public const string Info = "Info";
    public const string Warning = "Warning";
    public const string Error = "Error";
    public const string Critical = "Critical";
}

/// <summary>
/// Standard process flow step identifiers
/// </summary>
public static class ProcessFlowSteps
{
    public const string Authentication = "auth";
    public const string RequestValidation = "request-validation";
    public const string SemanticAnalysis = "semantic-analysis";
    public const string IntentDetection = "intent-detection";
    public const string EntityExtraction = "entity-extraction";
    public const string SchemaRetrieval = "schema-retrieval";
    public const string RelationshipDiscovery = "relationship-discovery";
    public const string JoinGeneration = "join-generation";
    public const string DateFilterGeneration = "date-filter-generation";
    public const string AggregationGeneration = "aggregation-generation";
    public const string PromptBuilding = "prompt-building";
    public const string ContextGathering = "context-gathering";
    public const string PromptAssembly = "prompt-assembly";
    public const string AIGeneration = "ai-generation";
    public const string OpenAIRequest = "openai-request";
    public const string ResponseParsing = "response-parsing";
    public const string SQLValidation = "sql-validation";
    public const string SQLExecution = "sql-execution";
    public const string QueryExecution = "query-execution";
    public const string ResultProcessing = "result-processing";
    public const string ResponseAssembly = "response-assembly";
}

/// <summary>
/// Process flow step definitions with metadata
/// </summary>
public static class ProcessFlowStepDefinitions
{
    public static readonly Dictionary<string, ProcessFlowStepDefinition> Steps = new()
    {
        [ProcessFlowSteps.Authentication] = new("Authentication & Authorization", "Validating JWT token and user permissions", 1),
        [ProcessFlowSteps.RequestValidation] = new("Request Validation", "Validating request parameters and structure", 2),
        [ProcessFlowSteps.SemanticAnalysis] = new("Semantic Analysis", "Analyzing query intent and extracting semantic meaning", 3),
        [ProcessFlowSteps.IntentDetection] = new("Intent Detection", "Determining query type and business intent", 3, ProcessFlowSteps.SemanticAnalysis),
        [ProcessFlowSteps.EntityExtraction] = new("Entity Extraction", "Identifying tables, columns, and business entities", 3, ProcessFlowSteps.SemanticAnalysis),
        [ProcessFlowSteps.SchemaRetrieval] = new("Schema Retrieval", "Fetching relevant database schema information", 4),
        [ProcessFlowSteps.RelationshipDiscovery] = new("Relationship Discovery", "Discovering foreign key relationships between tables", 4, ProcessFlowSteps.SchemaRetrieval),
        [ProcessFlowSteps.JoinGeneration] = new("JOIN Generation", "Generating optimal SQL JOINs based on relationships", 5),
        [ProcessFlowSteps.DateFilterGeneration] = new("Date Filter Generation", "Creating SQL date filters from time context", 5),
        [ProcessFlowSteps.AggregationGeneration] = new("Aggregation Generation", "Building SQL aggregations and grouping logic", 5),
        [ProcessFlowSteps.PromptBuilding] = new("Prompt Construction", "Building enhanced prompt with schema and business rules", 6),
        [ProcessFlowSteps.ContextGathering] = new("Context Gathering", "Collecting schema, examples, and business rules", 6, ProcessFlowSteps.PromptBuilding),
        [ProcessFlowSteps.PromptAssembly] = new("Prompt Assembly", "Assembling final prompt for AI model", 6, ProcessFlowSteps.PromptBuilding),
        [ProcessFlowSteps.AIGeneration] = new("AI SQL Generation", "Calling OpenAI API to generate SQL query", 6),
        [ProcessFlowSteps.OpenAIRequest] = new("OpenAI API Call", "Sending request to OpenAI GPT-4 model", 6, ProcessFlowSteps.AIGeneration),
        [ProcessFlowSteps.ResponseParsing] = new("Response Parsing", "Parsing and validating AI response", 6, ProcessFlowSteps.AIGeneration),
        [ProcessFlowSteps.SQLValidation] = new("SQL Validation", "Validating generated SQL syntax and semantics", 7),
        [ProcessFlowSteps.SQLExecution] = new("SQL Execution", "Executing validated SQL query against database", 8),
        [ProcessFlowSteps.QueryExecution] = new("Query Execution", "Running SQL query on database", 8, ProcessFlowSteps.SQLExecution),
        [ProcessFlowSteps.ResultProcessing] = new("Result Processing", "Processing and formatting query results", 8, ProcessFlowSteps.SQLExecution),
        [ProcessFlowSteps.ResponseAssembly] = new("Response Assembly", "Assembling final response with results and metadata", 9)
    };
}

/// <summary>
/// Process flow step definition
/// </summary>
public record ProcessFlowStepDefinition(string Name, string Description, int Order, string? ParentStepId = null);

/// <summary>
/// Process flow metrics for analytics
/// </summary>
public class ProcessFlowMetrics
{
    public string SessionId { get; set; } = string.Empty;
    public TimeSpan TotalDuration { get; set; }
    public Dictionary<string, TimeSpan> StepDurations { get; set; } = new();
    public Dictionary<string, decimal> StepConfidences { get; set; } = new();
    public int TotalSteps { get; set; }
    public int CompletedSteps { get; set; }
    public int ErrorSteps { get; set; }
    public decimal OverallSuccessRate { get; set; }
    public int TotalTokens { get; set; }
    public decimal TotalCost { get; set; }
    public int ApiCalls { get; set; }
}

/// <summary>
/// Process flow event for real-time updates
/// </summary>
public class ProcessFlowEvent
{
    public string SessionId { get; set; } = string.Empty;
    public string EventType { get; set; } = string.Empty; // step_update, log_added, session_completed
    public string? StepId { get; set; }
    public object? Data { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Process flow event types
/// </summary>
public static class ProcessFlowEventTypes
{
    public const string SessionStarted = "session_started";
    public const string SessionCompleted = "session_completed";
    public const string SessionError = "session_error";
    public const string StepStarted = "step_started";
    public const string StepCompleted = "step_completed";
    public const string StepError = "step_error";
    public const string StepUpdated = "step_updated";
    public const string LogAdded = "log_added";
    public const string TransparencyUpdated = "transparency_updated";
}
