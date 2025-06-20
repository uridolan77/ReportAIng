using BIReportingCopilot.Core.Models.Agents;
using BIReportingCopilot.Core.Models;
using AgentQueryIntent = BIReportingCopilot.Core.Models.Agents.QueryIntent;
using AgentQueryComplexity = BIReportingCopilot.Core.Models.Agents.QueryComplexity;
using AgentSchemaContext = BIReportingCopilot.Core.Models.Agents.SchemaContext;

namespace BIReportingCopilot.Core.Interfaces.Agents;

/// <summary>
/// Base interface for all specialized agents
/// </summary>
public interface ISpecializedAgent
{
    /// <summary>
    /// Agent type identifier
    /// </summary>
    string AgentType { get; }
    
    /// <summary>
    /// Agent capabilities and metadata
    /// </summary>
    AgentCapabilities Capabilities { get; }
    
    /// <summary>
    /// Process a generic agent request
    /// </summary>
    Task<AgentResponse> ProcessAsync(AgentRequest request, AgentContext context);
    
    /// <summary>
    /// Get current health status
    /// </summary>
    Task<HealthStatus> GetHealthStatusAsync();
    
    /// <summary>
    /// Initialize agent with configuration
    /// </summary>
    Task InitializeAsync(Dictionary<string, object> configuration);
    
    /// <summary>
    /// Shutdown agent gracefully
    /// </summary>
    Task ShutdownAsync();
}

/// <summary>
/// Query Understanding Agent - Natural language interpretation specialist
/// </summary>
public interface IQueryUnderstandingAgent : ISpecializedAgent
{
    /// <summary>
    /// Analyze natural language query to extract intent
    /// </summary>
    Task<AgentQueryIntent> AnalyzeIntentAsync(string naturalLanguage, AgentContext? context = null);

    /// <summary>
    /// Assess query complexity for routing decisions
    /// </summary>
    Task<AgentQueryComplexity> AssessComplexityAsync(string query, AgentContext? context = null);
    
    /// <summary>
    /// Detect ambiguities in natural language query
    /// </summary>
    Task<List<QueryAmbiguity>> DetectAmbiguitiesAsync(string query, AgentContext? context = null);
    
    /// <summary>
    /// Extract business entities from query
    /// </summary>
    Task<List<EntityReference>> ExtractEntitiesAsync(string query, AgentContext? context = null);
    
    /// <summary>
    /// Classify query type (SELECT, aggregation, etc.)
    /// </summary>
    Task<string> ClassifyQueryTypeAsync(string query, AgentContext? context = null);
}

/// <summary>
/// Schema Navigation Agent - Intelligent schema discovery specialist
/// </summary>
public interface ISchemaNavigationAgent : ISpecializedAgent
{
    /// <summary>
    /// Discover relevant tables based on query intent
    /// </summary>
    Task<List<RelevantTable>> DiscoverRelevantTablesAsync(AgentQueryIntent intent, AgentContext? context = null);

    /// <summary>
    /// Build comprehensive schema context for query generation
    /// </summary>
    Task<AgentSchemaContext> BuildContextAsync(List<RelevantTable> tables, AgentContext? context = null);
    
    /// <summary>
    /// Suggest optimal join paths between tables
    /// </summary>
    Task<List<JoinPath>> SuggestOptimalJoinsAsync(List<RelevantTable> tables, AgentContext? context = null);
    
    /// <summary>
    /// Find related tables through foreign key relationships
    /// </summary>
    Task<List<RelevantTable>> FindRelatedTablesAsync(string tableName, int maxDepth = 2, AgentContext? context = null);
    
    /// <summary>
    /// Get table metadata and column information
    /// </summary>
    Task<SchemaMetadata> GetTableMetadataAsync(string tableName, AgentContext? context = null);
    
    /// <summary>
    /// Validate table and column existence
    /// </summary>
    Task<bool> ValidateSchemaElementsAsync(List<EntityReference> entities, AgentContext? context = null);
}

/// <summary>
/// SQL Generation Agent - Optimized SQL creation specialist
/// </summary>
public interface ISqlGenerationAgent : ISpecializedAgent
{
    /// <summary>
    /// Generate optimized SQL from intent and schema context
    /// </summary>
    Task<GeneratedSql> GenerateOptimizedSqlAsync(AgentQueryIntent intent, AgentSchemaContext context, AgentContext? agentContext = null);
    
    /// <summary>
    /// Validate generated SQL for syntax and semantic correctness
    /// </summary>
    Task<SqlValidationResult> ValidateSqlAsync(string sql, AgentContext? context = null);
    
    /// <summary>
    /// Optimize existing SQL with performance hints
    /// </summary>
    Task<string> OptimizeSqlAsync(string sql, PerformanceHints hints, AgentContext? context = null);
    
    /// <summary>
    /// Generate multiple SQL variations for A/B testing
    /// </summary>
    Task<List<GeneratedSql>> GenerateSqlVariationsAsync(AgentQueryIntent intent, AgentSchemaContext context, int variationCount = 3, AgentContext? agentContext = null);
    
    /// <summary>
    /// Explain SQL query in natural language
    /// </summary>
    Task<string> ExplainSqlAsync(string sql, AgentContext? context = null);
    
    /// <summary>
    /// Generate performance hints for SQL query
    /// </summary>
    Task<PerformanceHints> GeneratePerformanceHintsAsync(string sql, AgentSchemaContext context, AgentContext? agentContext = null);
}

/// <summary>
/// Execution Agent - Query execution and optimization specialist
/// </summary>
public interface IExecutionAgent : ISpecializedAgent
{
    /// <summary>
    /// Execute SQL query with performance monitoring
    /// </summary>
    Task<QueryExecutionResult> ExecuteQueryAsync(string sql, Dictionary<string, object>? parameters = null, AgentContext? context = null);
    
    /// <summary>
    /// Execute query with streaming results for large datasets
    /// </summary>
    IAsyncEnumerable<QueryResultBatch> ExecuteStreamingQueryAsync(string sql, Dictionary<string, object>? parameters = null, AgentContext? context = null);
    
    /// <summary>
    /// Predict query execution performance
    /// </summary>
    Task<QueryPerformancePrediction> PredictPerformanceAsync(string sql, AgentContext? context = null);
    
    /// <summary>
    /// Monitor query execution in real-time
    /// </summary>
    Task<QueryExecutionMetrics> MonitorExecutionAsync(string queryId, AgentContext? context = null);
    
    /// <summary>
    /// Cancel running query
    /// </summary>
    Task<bool> CancelQueryAsync(string queryId, AgentContext? context = null);
}

/// <summary>
/// Visualization Agent - Data visualization and chart generation specialist
/// </summary>
public interface IVisualizationAgent : ISpecializedAgent
{
    /// <summary>
    /// Suggest optimal visualization types for query results
    /// </summary>
    Task<List<VisualizationSuggestion>> SuggestVisualizationsAsync(QueryExecutionResult queryResult, AgentContext? context = null);
    
    /// <summary>
    /// Generate chart configuration for specific visualization type
    /// </summary>
    Task<ChartConfiguration> GenerateChartConfigAsync(QueryExecutionResult queryResult, string visualizationType, AgentContext? context = null);
    
    /// <summary>
    /// Create dashboard layout from multiple queries
    /// </summary>
    Task<DashboardLayout> CreateDashboardLayoutAsync(List<QueryExecutionResult> queryResults, AgentContext? context = null);
    
    /// <summary>
    /// Generate insights and annotations for visualizations
    /// </summary>
    Task<List<VisualizationInsight>> GenerateInsightsAsync(QueryExecutionResult queryResult, AgentContext? context = null);
}

/// <summary>
/// Agent-to-Agent (A2A) Communication Protocol
/// </summary>
public interface IAgentCommunicationProtocol
{
    /// <summary>
    /// Send message to target agent and wait for response
    /// </summary>
    Task<TResponse> SendMessageAsync<TRequest, TResponse>(
        string targetAgent, 
        TRequest message, 
        AgentContext context,
        TimeSpan? timeout = null);
    
    /// <summary>
    /// Broadcast event to multiple agents
    /// </summary>
    Task BroadcastEventAsync<TEvent>(TEvent eventData, AgentContext context, List<string>? targetAgents = null);
    
    /// <summary>
    /// Discover available agents and their capabilities
    /// </summary>
    Task<List<AgentCapabilities>> DiscoverAgentCapabilitiesAsync();
    
    /// <summary>
    /// Register agent for communication
    /// </summary>
    Task RegisterAgentAsync(ISpecializedAgent agent);
    
    /// <summary>
    /// Unregister agent from communication
    /// </summary>
    Task UnregisterAgentAsync(string agentType);
    
    /// <summary>
    /// Get communication logs for monitoring
    /// </summary>
    Task<List<AgentCommunicationLog>> GetCommunicationLogsAsync(string? correlationId = null, DateTime? since = null);
}

/// <summary>
/// Intelligent Agent Orchestrator - Coordinates multiple agents
/// </summary>
public interface IIntelligentAgentOrchestrator
{
    /// <summary>
    /// Process query using optimal agent selection and coordination
    /// </summary>
    Task<QueryResponse> ProcessQueryAsync(QueryRequest request, string userId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Select optimal agents for specific query intent
    /// </summary>
    Task<List<string>> SelectOptimalAgentsAsync(AgentQueryIntent intent, AgentContext context);
    
    /// <summary>
    /// Execute agents in parallel with coordination
    /// </summary>
    Task<Dictionary<string, AgentResponse>> ExecuteAgentsInParallelAsync(
        List<string> agentTypes, 
        AgentRequest request, 
        AgentContext context);
    
    /// <summary>
    /// Aggregate results from multiple agents
    /// </summary>
    Task<QueryResponse> AggregateResultsAsync(Dictionary<string, AgentResponse> agentResults, AgentContext context);
    
    /// <summary>
    /// Monitor agent orchestration performance
    /// </summary>
    Task<OrchestrationMetrics> GetOrchestrationMetricsAsync(string? correlationId = null);
}

// Supporting models for new interfaces

public class QueryExecutionResult
{
    public string QueryId { get; set; } = Guid.NewGuid().ToString();
    public bool Success { get; set; }
    public object? Data { get; set; }
    public int RowCount { get; set; }
    public TimeSpan ExecutionTime { get; set; }
    public string? ErrorMessage { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();
}

public class QueryResultBatch
{
    public int BatchNumber { get; set; }
    public object[] Rows { get; set; } = Array.Empty<object>();
    public bool IsLastBatch { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();
}

public class QueryPerformancePrediction
{
    public TimeSpan EstimatedExecutionTime { get; set; }
    public double EstimatedResourceUsage { get; set; }
    public double ConfidenceScore { get; set; }
    public List<string> PerformanceWarnings { get; set; } = new();
}

public class QueryExecutionMetrics
{
    public string QueryId { get; set; } = string.Empty;
    public TimeSpan ElapsedTime { get; set; }
    public double CpuUsage { get; set; }
    public double MemoryUsage { get; set; }
    public int RowsProcessed { get; set; }
    public Dictionary<string, object> AdditionalMetrics { get; set; } = new();
}

public class VisualizationSuggestion
{
    public string Type { get; set; } = string.Empty; // bar, line, pie, scatter, etc.
    public double RelevanceScore { get; set; }
    public string Reasoning { get; set; } = string.Empty;
    public Dictionary<string, object> Configuration { get; set; } = new();
}

public class ChartConfiguration
{
    public string ChartType { get; set; } = string.Empty;
    public Dictionary<string, object> Settings { get; set; } = new();
    public List<string> DataColumns { get; set; } = new();
    public Dictionary<string, object> Styling { get; set; } = new();
}

public class DashboardLayout
{
    public string LayoutId { get; set; } = Guid.NewGuid().ToString();
    public List<DashboardWidget> Widgets { get; set; } = new();
    public Dictionary<string, object> LayoutSettings { get; set; } = new();
}

public class DashboardWidget
{
    public string WidgetId { get; set; } = Guid.NewGuid().ToString();
    public string Type { get; set; } = string.Empty;
    public ChartConfiguration Configuration { get; set; } = new();
    public Dictionary<string, object> Position { get; set; } = new();
}

public class VisualizationInsight
{
    public string Type { get; set; } = string.Empty; // trend, outlier, correlation, etc.
    public string Description { get; set; } = string.Empty;
    public double Confidence { get; set; }
    public Dictionary<string, object> Data { get; set; } = new();
}

public class OrchestrationMetrics
{
    public string CorrelationId { get; set; } = string.Empty;
    public TimeSpan TotalExecutionTime { get; set; }
    public int AgentsInvolved { get; set; }
    public Dictionary<string, TimeSpan> AgentExecutionTimes { get; set; } = new();
    public double SuccessRate { get; set; }
    public List<string> Errors { get; set; } = new();
}
