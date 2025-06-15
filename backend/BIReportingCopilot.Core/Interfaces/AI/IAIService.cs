using BIReportingCopilot.Core.Models;
using BIReportingCopilot.Core.Models.QuerySuggestions;

namespace BIReportingCopilot.Core.Interfaces.AI;

/// <summary>
/// Query processing result
/// </summary>
public class QueryProcessingResult
{
    public string GeneratedSql { get; set; } = string.Empty;
    public QueryAnalysisResult Analysis { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
    public List<string> Suggestions { get; set; } = new();
    public double ConfidenceScore { get; set; }
    public TimeSpan ProcessingTime { get; set; }

    // Additional properties expected by Infrastructure services
    public string GeneratedSQL
    {
        get => GeneratedSql;
        set => GeneratedSql = value;
    }

    public double Confidence
    {
        get => ConfidenceScore;
        set => ConfidenceScore = value;
    }

    public SemanticAnalysis SemanticAnalysis { get; set; } = new();
    public QueryClassificationResult Classification { get; set; } = new();
    public Dictionary<string, object> Metadata { get; set; } = new();
    
    // Additional properties referenced in Infrastructure
    public bool Success { get; set; } = true;
    public string Error { get; set; } = string.Empty;
    public string Query { get; set; } = string.Empty;
    public string Context { get; set; } = string.Empty;
}

/// <summary>
/// AI service status
/// </summary>
public class AIServiceStatus
{
    public bool IsHealthy { get; set; }
    public string Status { get; set; } = string.Empty;
    public Dictionary<string, object> Metrics { get; set; } = new();
    public DateTime LastUpdated { get; set; }
    public TimeSpan ResponseTime { get; set; }
}

/// <summary>
/// Semantic analysis result
/// </summary>
public class SemanticAnalysisResult
{
    public string Text { get; set; } = string.Empty;
    public List<string> Keywords { get; set; } = new();
    public List<EntityExtraction> Entities { get; set; } = new();
    public SentimentAnalysis Sentiment { get; set; } = new();
    public double ConfidenceScore { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();
    
    // Additional properties referenced in Infrastructure
    public double Confidence
    {
        get => ConfidenceScore;
        set => ConfidenceScore = value;
    }
    
    public string Intent { get; set; } = string.Empty;
}

/// <summary>
/// Vector search result
/// </summary>
public class VectorSearchResult
{
    public string DocumentId { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public double SimilarityScore { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();
    
    // Additional properties referenced in Infrastructure
    public string Id
    {
        get => DocumentId;
        set => DocumentId = value;
    }
    
    public string Text
    {
        get => Content;
        set => Content = value;
    }
    
    public double Score
    {
        get => SimilarityScore;
        set => SimilarityScore = value;
    }
}

/// <summary>
/// Vector document
/// </summary>
public class VectorDocument
{
    public string Id { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public float[] Vector { get; set; } = Array.Empty<float>();
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Vector index statistics
/// </summary>
public class VectorIndexStatistics
{
    public long TotalDocuments { get; set; }
    public long IndexSize { get; set; }
    public DateTime LastUpdated { get; set; }
    public Dictionary<string, object> Metrics { get; set; } = new();
    
    // Additional properties referenced in Infrastructure
    public double AverageQueryTime { get; set; }
    public long TotalQueries { get; set; }
}

/// <summary>
/// Core AI service interface for AI-powered operations
/// </summary>
public interface IAIService
{
    Task<string> GenerateQueryAsync(string naturalLanguageQuery, SchemaMetadata? schema = null, CancellationToken cancellationToken = default);
    Task<QueryProcessingResult> ProcessNaturalLanguageAsync(string query, string? context = null, CancellationToken cancellationToken = default);
    Task<List<QuerySuggestion>> GetQuerySuggestionsAsync(string partialQuery, SchemaMetadata? schema = null, CancellationToken cancellationToken = default);
    Task<QueryValidationResult> ValidateQueryAsync(string query, CancellationToken cancellationToken = default);
    Task<QueryOptimizationResult> OptimizeQueryAsync(string query, CancellationToken cancellationToken = default);
    Task<SemanticAnalysisResult> AnalyzeSemanticContentAsync(string content, CancellationToken cancellationToken = default);
    Task<double> CalculateSemanticSimilarityAsync(string text1, string text2, CancellationToken cancellationToken = default);
    Task<List<EntityExtraction>> ExtractEntitiesAsync(string text, CancellationToken cancellationToken = default);
    Task<NLUResult> AnalyzeIntentAsync(string text, CancellationToken cancellationToken = default);    Task<bool> IsHealthyAsync(CancellationToken cancellationToken = default);
    Task<AIServiceStatus> GetStatusAsync(CancellationToken cancellationToken = default);
    Task<IAsyncEnumerable<string>> GenerateSQLStreamAsync(string prompt, SchemaMetadata schema, string context, CancellationToken cancellationToken = default);
    
    // Additional methods referenced in Infrastructure
    Task<string> GenerateSQLAsync(string prompt, SchemaMetadata? schema = null, CancellationToken cancellationToken = default);
    Task<double> CalculateConfidenceScoreAsync(string query, string sql, CancellationToken cancellationToken = default);
    Task<string> GenerateVisualizationConfigAsync(string sql, SchemaMetadata? schema = null, CancellationToken cancellationToken = default);
    Task<IAsyncEnumerable<string>> GenerateInsightStreamAsync(string data, string context, CancellationToken cancellationToken = default);
    Task<string> GenerateInsightAsync(string data, string context, CancellationToken cancellationToken = default);
    Task<float[]> GenerateEmbeddingAsync(string text, CancellationToken cancellationToken = default);
    Task<bool> ValidateQueryIntentAsync(string query, string expectedIntent, CancellationToken cancellationToken = default);
    Task<List<QuerySuggestion>> GenerateQuerySuggestionsAsync(string context, SchemaMetadata? schema = null, CancellationToken cancellationToken = default);
}

/// <summary>
/// Semantic analyzer interface for semantic analysis operations
/// </summary>
public interface ISemanticAnalyzer
{
    Task<SemanticAnalysisResult> AnalyzeAsync(string text, CancellationToken cancellationToken = default);
    Task<double> CalculateSimilarityAsync(string text1, string text2, CancellationToken cancellationToken = default);
    Task<List<string>> ExtractKeywordsAsync(string text, int maxKeywords = 10, CancellationToken cancellationToken = default);
    Task<List<EntityExtraction>> ExtractEntitiesAsync(string text, CancellationToken cancellationToken = default);
    Task<SentimentAnalysis> AnalyzeSentimentAsync(string text, CancellationToken cancellationToken = default);
    Task<TextClassification> ClassifyTextAsync(string text, List<string> categories, CancellationToken cancellationToken = default);
    Task<SemanticContext> BuildContextAsync(string text, SchemaMetadata? schema = null, CancellationToken cancellationToken = default);
}

/// <summary>
/// Vector search service interface for vector-based search operations
/// </summary>
public interface IVectorSearchService
{
    Task<List<VectorSearchResult>> SearchAsync(string query, int maxResults = 10, double similarityThreshold = 0.7, CancellationToken cancellationToken = default);
    Task<List<VectorSearchResult>> SearchByVectorAsync(float[] vector, int maxResults = 10, double similarityThreshold = 0.7, CancellationToken cancellationToken = default);
    Task<string> IndexDocumentAsync(VectorDocument document, CancellationToken cancellationToken = default);
    Task<bool> UpdateDocumentAsync(string documentId, VectorDocument document, CancellationToken cancellationToken = default);
    Task<bool> DeleteDocumentAsync(string documentId, CancellationToken cancellationToken = default);
    Task<float[]> GenerateEmbeddingAsync(string text, CancellationToken cancellationToken = default);    Task<VectorIndexStatistics> GetIndexStatisticsAsync(CancellationToken cancellationToken = default);
    Task<bool> IsHealthyAsync(CancellationToken cancellationToken = default);
    Task<List<string>> FindSimilarQueriesByTextAsync(string text, int maxResults = 10, double threshold = 0.7, CancellationToken cancellationToken = default);
    
    // Additional methods referenced in Infrastructure
    Task StoreQueryEmbeddingAsync(string queryId, string query, float[] embedding, CancellationToken cancellationToken = default);
    Task ClearIndexAsync(CancellationToken cancellationToken = default);
    Task InvalidateByPatternAsync(string pattern, CancellationToken cancellationToken = default);
    Task<List<VectorSearchResult>> FindSimilarQueriesAsync(string query, int maxResults = 10, double threshold = 0.7, CancellationToken cancellationToken = default);
    Task<VectorSearchMetrics> GetMetricsAsync(CancellationToken cancellationToken = default);
    Task OptimizeIndexAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Semantic context
/// </summary>
public class SemanticContext
{
    public string Text { get; set; } = string.Empty;
    public List<string> RelatedTables { get; set; } = new();
    public List<string> RelatedColumns { get; set; } = new();
    public List<string> Keywords { get; set; } = new();
    public List<EntityExtraction> Entities { get; set; } = new();
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Vector search metrics
/// </summary>
public class VectorSearchMetrics
{
    public long DocumentCount { get; set; }
    public long QueryCount { get; set; }
    public double AverageQueryTime { get; set; }
    public long MemoryUsage { get; set; }
    public DateTime LastUpdated { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();
    
    // Additional properties required by InMemoryVectorSearchService
    public long TotalEmbeddings { get; set; }
    public long TotalSearches { get; set; }
    public double AverageSearchTime { get; set; }
    public double CacheHitRate { get; set; }
    public long IndexSizeBytes { get; set; }
    public DateTime LastOptimized { get; set; }
    public long TotalQueries { get; set; }
    public Dictionary<string, object> PerformanceMetrics { get; set; } = new();
}
