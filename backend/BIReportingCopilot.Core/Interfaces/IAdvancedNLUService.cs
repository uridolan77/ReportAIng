using BIReportingCopilot.Core.Models;

namespace BIReportingCopilot.Core.Interfaces;

/// <summary>
/// Advanced Natural Language Understanding service interface
/// Provides deep semantic analysis, intent recognition, and context management
/// </summary>
public interface IAdvancedNLUService
{
    /// <summary>
    /// Perform comprehensive NLU analysis on natural language query
    /// </summary>
    Task<AdvancedNLUResult> AnalyzeQueryAsync(
        string naturalLanguageQuery, 
        string userId, 
        NLUAnalysisContext? context = null);

    /// <summary>
    /// Classify query intent with confidence scoring
    /// </summary>
    Task<IntentAnalysis> ClassifyIntentAsync(string query, string? userId = null);

    /// <summary>
    /// Extract entities and their relationships from query
    /// </summary>
    Task<EntityAnalysis> ExtractEntitiesAsync(string query, SchemaMetadata? schema = null);

    /// <summary>
    /// Analyze query context and conversation history
    /// </summary>
    Task<ContextualAnalysis> AnalyzeContextAsync(
        string query, 
        string userId, 
        List<string>? conversationHistory = null);

    /// <summary>
    /// Generate query suggestions based on NLU analysis
    /// </summary>
    Task<List<QuerySuggestion>> GenerateSmartSuggestionsAsync(
        string partialQuery, 
        string userId, 
        SchemaMetadata? schema = null);

    /// <summary>
    /// Improve query based on NLU analysis and user intent
    /// </summary>
    Task<QueryImprovement> SuggestQueryImprovementsAsync(
        string originalQuery, 
        AdvancedNLUResult nluResult);

    /// <summary>
    /// Analyze conversation patterns and user preferences
    /// </summary>
    Task<ConversationAnalysis> AnalyzeConversationAsync(
        string userId, 
        TimeSpan? analysisWindow = null);

    /// <summary>
    /// Train NLU models with user feedback and domain data
    /// </summary>
    Task TrainModelsAsync(List<NLUTrainingData> trainingData, string? domain = null);

    /// <summary>
    /// Get NLU performance metrics and model statistics
    /// </summary>
    Task<NLUMetrics> GetMetricsAsync();

    /// <summary>
    /// Update NLU configuration and model parameters
    /// </summary>
    Task UpdateConfigurationAsync(NLUConfiguration configuration);
}

/// <summary>
/// Enhanced schema optimization service interface
/// Provides intelligent database optimization and performance analysis
/// </summary>
public interface ISchemaOptimizationService
{
    /// <summary>
    /// Analyze query performance and suggest optimizations
    /// </summary>
    Task<QueryOptimizationResult> AnalyzeQueryPerformanceAsync(
        string sql, 
        SchemaMetadata schema, 
        QueryExecutionMetrics? metrics = null);

    /// <summary>
    /// Generate automated index suggestions based on query patterns
    /// </summary>
    Task<List<IndexSuggestion>> SuggestIndexesAsync(
        List<QueryHistoryItem> queryHistory, 
        SchemaMetadata schema);

    /// <summary>
    /// Optimize SQL query for better performance
    /// </summary>
    Task<SqlOptimizationResult> OptimizeSqlAsync(
        string originalSql, 
        SchemaMetadata schema, 
        OptimizationGoals? goals = null);

    /// <summary>
    /// Analyze schema health and suggest improvements
    /// </summary>
    Task<SchemaHealthAnalysis> AnalyzeSchemaHealthAsync(SchemaMetadata schema);

    /// <summary>
    /// Generate query execution plan analysis
    /// </summary>
    Task<ExecutionPlanAnalysis> AnalyzeExecutionPlanAsync(
        string sql, 
        SchemaMetadata schema);

    /// <summary>
    /// Suggest query rewrites for better performance
    /// </summary>
    Task<List<QueryRewrite>> SuggestQueryRewritesAsync(
        string originalSql, 
        SchemaMetadata schema);

    /// <summary>
    /// Monitor query performance trends and patterns
    /// </summary>
    Task<PerformanceTrendAnalysis> AnalyzePerformanceTrendsAsync(
        List<QueryHistoryItem> queryHistory, 
        TimeSpan analysisWindow);

    /// <summary>
    /// Generate automated database maintenance recommendations
    /// </summary>
    Task<List<MaintenanceRecommendation>> GenerateMaintenanceRecommendationsAsync(
        SchemaMetadata schema, 
        List<QueryHistoryItem> queryHistory);

    /// <summary>
    /// Get schema optimization metrics and statistics
    /// </summary>
    Task<SchemaOptimizationMetrics> GetOptimizationMetricsAsync();
}

/// <summary>
/// Query intelligence service combining NLU and schema optimization
/// </summary>
public interface IQueryIntelligenceService
{
    /// <summary>
    /// Perform comprehensive query analysis combining NLU and performance optimization
    /// </summary>
    Task<QueryIntelligenceResult> AnalyzeQueryAsync(
        string naturalLanguageQuery, 
        string userId, 
        SchemaMetadata schema);

    /// <summary>
    /// Generate intelligent query suggestions based on user intent and performance
    /// </summary>
    Task<List<IntelligentQuerySuggestion>> GenerateIntelligentSuggestionsAsync(
        string userId, 
        SchemaMetadata schema, 
        string? context = null);

    /// <summary>
    /// Provide real-time query assistance and optimization hints
    /// </summary>
    Task<QueryAssistance> GetQueryAssistanceAsync(
        string partialQuery, 
        string userId, 
        SchemaMetadata schema);

    /// <summary>
    /// Learn from user interactions to improve suggestions
    /// </summary>
    Task LearnFromInteractionAsync(
        string userId, 
        string query, 
        QueryResponse response, 
        UserFeedback? feedback = null);
}
