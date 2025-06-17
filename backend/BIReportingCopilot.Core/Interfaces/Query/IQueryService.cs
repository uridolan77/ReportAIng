using BIReportingCopilot.Core.Models;
using BIReportingCopilot.Core.Models.QuerySuggestions;
using BIReportingCopilot.Core.Interfaces.AI;

namespace BIReportingCopilot.Core.Interfaces.Query;

/// <summary>
/// Core query service interface for executing database queries
/// </summary>
public interface IQueryService
{
    Task<QueryResult> ExecuteQueryAsync(QueryRequest request, CancellationToken cancellationToken = default);
    Task<QueryResult> ExecuteSelectQueryAsync(string sqlQuery, CancellationToken cancellationToken = default);
    Task<bool> ValidateQueryAsync(string sqlQuery, CancellationToken cancellationToken = default);
    Task<List<UnifiedQueryHistoryEntity>> GetQueryHistoryAsync(string userId, int pageSize = 50, int pageNumber = 1, CancellationToken cancellationToken = default);
    Task<QueryResult> GetCachedQueryResultAsync(string cacheKey, CancellationToken cancellationToken = default);
    Task SaveQueryResultAsync(string cacheKey, QueryResult result, TimeSpan? expiry = null, CancellationToken cancellationToken = default);
    Task<QueryPerformanceMetrics> GetPerformanceMetricsAsync(CancellationToken cancellationToken = default);

    // Methods expected by Infrastructure services
    Task<QueryResponse> ProcessQueryAsync(QueryRequest request, CancellationToken cancellationToken = default);
    Task<bool> SubmitFeedbackAsync(string queryId, string feedback, CancellationToken cancellationToken = default);
    Task<bool> SubmitFeedbackAsync(QueryFeedback feedback, string userId, CancellationToken cancellationToken = default);
    Task<List<QuerySuggestion>> GetQuerySuggestionsAsync(string partialQuery, CancellationToken cancellationToken = default);
    Task<List<QuerySuggestion>> GetQuerySuggestionsAsync(string userId, string context, CancellationToken cancellationToken = default);
    Task<QueryResult?> GetCachedQueryAsync(string cacheKey, CancellationToken cancellationToken = default);
    Task CacheQueryAsync(string cacheKey, QueryResult result, CancellationToken cancellationToken = default);
    Task<QueryPerformanceMetrics> GetQueryPerformanceAsync(CancellationToken cancellationToken = default);
    Task<List<QuerySuggestion>> GetSmartSuggestionsAsync(string query, CancellationToken cancellationToken = default);
    Task<QueryResult> OptimizeQueryAsync(string query, CancellationToken cancellationToken = default);
    Task InvalidateQueryCacheAsync(string pattern, CancellationToken cancellationToken = default);
    Task<QueryResult> ProcessAdvancedQueryAsync(QueryRequest request, CancellationToken cancellationToken = default);
    Task<double> CalculateSemanticSimilarityAsync(string query1, string query2, CancellationToken cancellationToken = default);
    Task<List<UnifiedQueryHistoryEntity>> FindSimilarQueriesAsync(string query, CancellationToken cancellationToken = default);
    Task<List<UnifiedQueryHistoryEntity>> FindSimilarQueriesAsync(string query, string userId, int limit, CancellationToken cancellationToken = default);
}

/// <summary>
/// Query processor interface for processing natural language queries
/// </summary>
public interface IQueryProcessor
{
    Task<string> ProcessNaturalLanguageQueryAsync(string naturalLanguageQuery, string? context = null, CancellationToken cancellationToken = default);
    Task<QueryProcessingResult> ProcessQueryWithAnalysisAsync(string naturalLanguageQuery, string? context = null, CancellationToken cancellationToken = default);
    Task<QueryProcessingResult> ProcessQueryAsync(string query, string userId, CancellationToken cancellationToken = default);
    Task<bool> ValidateQueryIntentAsync(string query, CancellationToken cancellationToken = default);
    Task<List<QuerySuggestion>> GetQuerySuggestionsAsync(string partialQuery, CancellationToken cancellationToken = default);
}

/// <summary>
/// Query cache service interface
/// </summary>
public interface IQueryCacheService
{
    Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class;
    Task SetAsync<T>(string key, T value, TimeSpan? expiry = null, CancellationToken cancellationToken = default) where T : class;
    Task RemoveAsync(string key, CancellationToken cancellationToken = default);
    Task RemovePatternAsync(string pattern, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default);
    Task ClearAllAsync(CancellationToken cancellationToken = default);
    Task<CacheStatistics> GetStatisticsAsync(CancellationToken cancellationToken = default);

    // Additional methods expected by Infrastructure services
    Task InvalidateByPatternAsync(string pattern, CancellationToken cancellationToken = default);
    Task CacheQueryAsync(string key, object result, TimeSpan? expiration = null, CancellationToken cancellationToken = default);
    Task<T?> GetCachedQueryAsync<T>(string key, CancellationToken cancellationToken = default) where T : class;
}

/// <summary>
/// Query pattern management service interface
/// </summary>
public interface IQueryPatternManagementService
{
    Task<List<QueryPattern>> GetPatternsAsync(CancellationToken cancellationToken = default);
    Task<QueryPattern?> GetPatternByIdAsync(string patternId, CancellationToken cancellationToken = default);
    Task<string> CreatePatternAsync(QueryPattern pattern, CancellationToken cancellationToken = default);
    Task<bool> UpdatePatternAsync(QueryPattern pattern, CancellationToken cancellationToken = default);
    Task<bool> DeletePatternAsync(string patternId, CancellationToken cancellationToken = default);
    Task<List<QueryPattern>> FindSimilarPatternsAsync(string query, double threshold = 0.8, CancellationToken cancellationToken = default);
}

// QueryProcessingResult definition moved to Core.Interfaces.AI.IAIService.cs - removed duplicate

/// <summary>
/// Cache statistics
/// </summary>
public class CacheStatistics
{
    public long TotalKeys { get; set; }
    public long HitCount { get; set; }
    public long MissCount { get; set; }
    public double HitRate => TotalRequests > 0 ? (double)HitCount / TotalRequests : 0;
    public long TotalRequests => HitCount + MissCount;
    public long MemoryUsage { get; set; }
    public DateTime LastUpdated { get; set; }
}

/// <summary>
/// Query performance metrics
/// </summary>
public class QueryPerformanceMetrics
{
    public int TotalQueries { get; set; }
    public double AverageExecutionTime { get; set; }
    public double MedianExecutionTime { get; set; }
    public double P95ExecutionTime { get; set; }
    public double P99ExecutionTime { get; set; }
    public int SuccessfulQueries { get; set; }
    public int FailedQueries { get; set; }
    public double SuccessRate { get; set; }
    public DateTime LastUpdated { get; set; }

    // Properties expected by Infrastructure services
    public double CacheHitRate { get; set; }
    public double ErrorRate { get; set; }
}

// =============================================================================
// MISSING QUERY INTERFACES
// =============================================================================

/// <summary>
/// SQL Query service interface
/// </summary>
public interface ISqlQueryService
{
    Task<QueryResult> ExecuteQueryAsync(string sql, CancellationToken cancellationToken = default);
    Task<bool> ValidateQueryAsync(string sql, CancellationToken cancellationToken = default);
    Task<QueryMetadata> GetQueryMetadataAsync(string sql, CancellationToken cancellationToken = default);

    // Additional methods expected by Infrastructure services
    Task<QueryResult> ExecuteSelectQueryAsync(string sql, CancellationToken cancellationToken = default);
    Task<QueryResult> ExecuteSelectQueryAsync(string sql, QueryOptions options, CancellationToken cancellationToken = default);
    Task<bool> ValidateSqlAsync(string sql, CancellationToken cancellationToken = default);
}

/// <summary>
/// Schema optimization service interface
/// </summary>
public interface ISchemaOptimizationService
{
    Task<SchemaOptimizationResult> OptimizeSchemaAsync(SchemaMetadata schema, CancellationToken cancellationToken = default);
    Task<List<BIReportingCopilot.Core.Models.IndexRecommendation>> GetIndexRecommendationsAsync(string tableName, CancellationToken cancellationToken = default);

    // Additional methods expected by Infrastructure services
    Task<QueryOptimizationResult> AnalyzeQueryPerformanceAsync(string query, CancellationToken cancellationToken = default);
    Task<QueryOptimizationResult> AnalyzeQueryPerformanceAsync(string sql, SchemaMetadata schema, QueryExecutionMetrics? metrics = null);
    Task<BIReportingCopilot.Core.Models.SchemaOptimizationMetrics> GetOptimizationMetricsAsync(CancellationToken cancellationToken = default);
    Task<BIReportingCopilot.Core.Models.SqlOptimizationResult> OptimizeSqlAsync(string sql, CancellationToken cancellationToken = default);
    Task<List<BIReportingCopilot.Core.Models.IndexSuggestion>> SuggestIndexesAsync(string tableName, CancellationToken cancellationToken = default);
}

/// <summary>
/// Query intelligence service interface
/// </summary>
public interface IQueryIntelligenceService
{
    Task<QueryIntelligenceResult> AnalyzeQueryAsync(string query, string sqlQuery, SchemaMetadata schema);
    Task<List<IntelligentQuerySuggestion>> GenerateIntelligentSuggestionsAsync(string context, SchemaMetadata schema, string? userId = null);
    Task<QueryAssistance> GetQueryAssistanceAsync(string partialQuery, string context, SchemaMetadata schema);
    Task LearnFromInteractionAsync(string query, string sqlQuery, QueryResponse response, UserFeedback? feedback = null);
}

/// <summary>
/// Schema optimization result
/// </summary>
public class SchemaOptimizationResult
{
    public List<string> Recommendations { get; set; } = new();
    public double ImprovementScore { get; set; }
    public Dictionary<string, object> Metrics { get; set; } = new();
}

// IndexRecommendation has been consolidated into Core.Models.IndexRecommendation
// See: Core/Models/AIModels.cs

// Note: Schema optimization classes moved to Core.Models to avoid ambiguous references

// Duplicate IQueryCacheService removed - using the one defined above

// Note: ISchemaService and ICacheService have been moved to their respective namespaces
// to avoid ambiguous references. Use:
// - BIReportingCopilot.Core.Interfaces.Schema.ISchemaService
// - BIReportingCopilot.Core.Interfaces.Cache.ICacheService
