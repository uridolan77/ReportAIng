using BIReportingCopilot.Core.Models;
using BIReportingCopilot.Core.Models.QuerySuggestions;

namespace BIReportingCopilot.Core.Interfaces.AI;



/// <summary>
/// ML services interface for machine learning operations
/// </summary>
public interface IMLServices
{
    Task<MLModelResult> TrainModelAsync(MLTrainingRequest request, CancellationToken cancellationToken = default);
    Task<MLPredictionResult> PredictAsync(MLPredictionRequest request, CancellationToken cancellationToken = default);
    Task<List<MLModel>> GetAvailableModelsAsync(CancellationToken cancellationToken = default);
    Task<MLModel?> GetModelAsync(string modelId, CancellationToken cancellationToken = default);
    Task<bool> DeleteModelAsync(string modelId, CancellationToken cancellationToken = default);
    Task<MLModelMetrics> GetModelMetricsAsync(string modelId, CancellationToken cancellationToken = default);
    Task<bool> UpdateModelAsync(string modelId, MLModelUpdate update, CancellationToken cancellationToken = default);
}

/// <summary>
/// NLU service interface for natural language understanding
/// </summary>
public interface INLUService
{
    Task<NLUResult> AnalyzeIntentAsync(string text, CancellationToken cancellationToken = default);
    Task<List<EntityExtraction>> ExtractEntitiesAsync(string text, CancellationToken cancellationToken = default);
    Task<SentimentAnalysis> AnalyzeSentimentAsync(string text, CancellationToken cancellationToken = default);
    Task<List<string>> GenerateKeywordsAsync(string text, int maxKeywords = 10, CancellationToken cancellationToken = default);
    Task<double> CalculateTextSimilarityAsync(string text1, string text2, CancellationToken cancellationToken = default);
    Task<TextClassification> ClassifyTextAsync(string text, List<string> categories, CancellationToken cancellationToken = default);
    Task<LanguageDetection> DetectLanguageAsync(string text, CancellationToken cancellationToken = default);
}

/// <summary>
/// Semantic cache service interface for AI-powered caching
/// </summary>
public interface ISemanticCacheService
{
    Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class;
    Task<T?> GetSemanticAsync<T>(string query, double similarityThreshold = 0.8, CancellationToken cancellationToken = default) where T : class;
    Task SetAsync<T>(string key, T value, TimeSpan? expiry = null, CancellationToken cancellationToken = default) where T : class;
    Task SetSemanticAsync<T>(string query, T value, TimeSpan? expiry = null, CancellationToken cancellationToken = default) where T : class;
    Task RemoveAsync(string key, CancellationToken cancellationToken = default);
    Task RemoveSemanticAsync(string query, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default);
    Task<List<SemanticCacheEntry>> FindSimilarAsync(string query, double threshold = 0.8, int maxResults = 10, CancellationToken cancellationToken = default);
    Task<SemanticCacheStatistics> GetStatisticsAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Vector search service interface for similarity search
/// </summary>
public interface IVectorSearchService
{
    Task<string> IndexDocumentAsync(string documentId, string content, Dictionary<string, object>? metadata = null, CancellationToken cancellationToken = default);
    Task<List<VectorSearchResult>> SearchAsync(string query, int maxResults = 10, double threshold = 0.7, CancellationToken cancellationToken = default);
    Task<List<VectorSearchResult>> SearchSimilarAsync(string documentId, int maxResults = 10, double threshold = 0.7, CancellationToken cancellationToken = default);
    Task<bool> DeleteDocumentAsync(string documentId, CancellationToken cancellationToken = default);
    Task<bool> UpdateDocumentAsync(string documentId, string content, Dictionary<string, object>? metadata = null, CancellationToken cancellationToken = default);
    Task<VectorSearchStatistics> GetStatisticsAsync(CancellationToken cancellationToken = default);
    Task<bool> ClearIndexAsync(CancellationToken cancellationToken = default);
    Task<float[]> GenerateEmbeddingAsync(string text, CancellationToken cancellationToken = default);
    Task<string> StoreQueryEmbeddingAsync(string queryId, string query, float[] embedding, CancellationToken cancellationToken = default);
}

/// <summary>
/// LLM-aware AI service interface for advanced AI operations
/// </summary>
public interface ILLMAwareAIService
{
    Task<string> GenerateSqlQueryAsync(string naturalLanguageQuery, string schema, CancellationToken cancellationToken = default);
    Task<bool> ValidateQueryIntentAsync(string query, CancellationToken cancellationToken = default);
    Task<QueryAnalysisResult> AnalyzeQueryComplexityAsync(string query, CancellationToken cancellationToken = default);
    Task<List<QuerySuggestion>> GenerateQuerySuggestionsAsync(string partialQuery, string schema, CancellationToken cancellationToken = default);
    Task<VisualizationRecommendation> RecommendVisualizationAsync(object[] data, string[] columnNames, CancellationToken cancellationToken = default);
    Task<string> ExplainQueryResultsAsync(object[] data, string originalQuery, CancellationToken cancellationToken = default);
    Task<bool> IsServiceHealthyAsync(CancellationToken cancellationToken = default);
    Task<AIServiceMetrics> GetServiceMetricsAsync(CancellationToken cancellationToken = default);
}

// =============================================================================
// MISSING AI INTERFACES
// =============================================================================

/// <summary>
/// Core AI service interface
/// </summary>
public interface IAIService
{
    Task<string> GenerateSQLAsync(string prompt, CancellationToken cancellationToken = default);
    Task<string> GenerateInsightAsync(string query, object[] data);
    Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default);
    Task<AIServiceMetrics> GetServiceMetricsAsync(CancellationToken cancellationToken = default);

    // Methods expected by Infrastructure services
    Task<string> GenerateVisualizationConfigAsync(string query, object[] data, CancellationToken cancellationToken = default);
    Task<IAsyncEnumerable<string>> GenerateSQLStreamAsync(string prompt, CancellationToken cancellationToken = default);
    Task<IAsyncEnumerable<string>> GenerateInsightStreamAsync(string query, object[] data, CancellationToken cancellationToken = default);
}

/// <summary>
/// AI Provider interface
/// </summary>
public interface IAIProvider
{
    string ProviderId { get; }
    string ProviderName { get; }
    Task<AIResponse> GenerateResponseAsync(AIRequest request, CancellationToken cancellationToken = default);
    Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default);
    Task<AIProviderMetrics> GetMetricsAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// AI Provider Factory interface
/// </summary>
public interface IAIProviderFactory
{
    IAIProvider CreateProvider(string providerType);
    Task<IAIProvider> CreateProviderAsync(string providerType, CancellationToken cancellationToken = default);
    List<string> GetSupportedProviders();
}

/// <summary>
/// Prompt service interface
/// </summary>
public interface IPromptService
{
    Task<string> GetPromptAsync(string promptKey, CancellationToken cancellationToken = default);
    Task<string> FormatPromptAsync(string template, Dictionary<string, object> parameters, CancellationToken cancellationToken = default);
}

/// <summary>
/// Context manager interface
/// </summary>
public interface IContextManager
{
    Task<Dictionary<string, object>> GetContextAsync(string contextKey, CancellationToken cancellationToken = default);
    Task SetContextAsync(string contextKey, Dictionary<string, object> context, CancellationToken cancellationToken = default);
}

/// <summary>
/// Semantic analyzer interface
/// </summary>
public interface ISemanticAnalyzer
{
    Task<SemanticAnalysisResult> AnalyzeAsync(string text, CancellationToken cancellationToken = default);
}

/// <summary>
/// Query classifier interface
/// </summary>
public interface IQueryClassifier
{
    Task<QueryClassificationResult> ClassifyAsync(string query, CancellationToken cancellationToken = default);
}

/// <summary>
/// Query optimizer interface
/// </summary>
public interface IQueryOptimizer
{
    Task<QueryOptimizationResult> OptimizeAsync(string query, CancellationToken cancellationToken = default);
}

/// <summary>
/// Advanced NLU service interface
/// </summary>
public interface IAdvancedNLUService
{
    Task<AdvancedNLUResult> ProcessAdvancedAsync(string text, CancellationToken cancellationToken = default);
}

/// <summary>
/// Multi-modal dashboard service interface
/// </summary>
public interface IMultiModalDashboardService
{
    Task<DashboardResult> CreateDashboardAsync(DashboardRequest request, CancellationToken cancellationToken = default);
}

/// <summary>
/// Business context auto generator interface
/// </summary>
public interface IBusinessContextAutoGenerator
{
    Task<BusinessContext> GenerateContextAsync(string domain, CancellationToken cancellationToken = default);
}

/// <summary>
/// Query analysis result
/// </summary>
public class QueryAnalysisResult
{
    public string QueryType { get; set; } = string.Empty;
    public int ComplexityScore { get; set; }
    public List<string> RequiredTables { get; set; } = new();
    public List<string> RequiredColumns { get; set; } = new();
    public TimeSpan EstimatedExecutionTime { get; set; }
    public List<string> Warnings { get; set; } = new();
    public List<string> Suggestions { get; set; } = new();
}

/// <summary>
/// AI service metrics
/// </summary>
public class AIServiceMetrics
{
    public int TotalRequests { get; set; }
    public int SuccessfulRequests { get; set; }
    public int FailedRequests { get; set; }
    public double AverageResponseTime { get; set; }
    public DateTime LastRequestTime { get; set; }
    public string ServiceStatus { get; set; } = "Unknown";
}

// =============================================================================
// SUPPORTING MODEL CLASSES
// =============================================================================

/// <summary>
/// AI provider capabilities
/// </summary>
public class AIProviderCapabilities
{
    public List<string> SupportedModels { get; set; } = new();
    public List<string> SupportedFeatures { get; set; } = new();
    public int MaxTokens { get; set; }
    public bool SupportsStreaming { get; set; }
    public bool SupportsEmbeddings { get; set; }
    public bool SupportsFunctionCalling { get; set; }
    public Dictionary<string, object> Limits { get; set; } = new();
}

/// <summary>
/// ML model result
/// </summary>
public class MLModelResult
{
    public string ModelId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public double Accuracy { get; set; }
    public Dictionary<string, object> Metrics { get; set; } = new();
    public DateTime TrainedAt { get; set; }
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// ML training request
/// </summary>
public class MLTrainingRequest
{
    public string ModelType { get; set; } = string.Empty;
    public object[] TrainingData { get; set; } = Array.Empty<object>();
    public Dictionary<string, object> Parameters { get; set; } = new();
    public string? ValidationData { get; set; }
}

/// <summary>
/// ML prediction request
/// </summary>
public class MLPredictionRequest
{
    public string ModelId { get; set; } = string.Empty;
    public object[] InputData { get; set; } = Array.Empty<object>();
    public Dictionary<string, object> Parameters { get; set; } = new();
}

/// <summary>
/// ML prediction result
/// </summary>
public class MLPredictionResult
{
    public object[] Predictions { get; set; } = Array.Empty<object>();
    public double[] Confidence { get; set; } = Array.Empty<double>();
    public Dictionary<string, object> Metadata { get; set; } = new();
    public DateTime PredictedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// ML model
/// </summary>
public class MLModel
{
    public string ModelId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public double Accuracy { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime LastUsedAt { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// ML model metrics
/// </summary>
public class MLModelMetrics
{
    public double Accuracy { get; set; }
    public double Precision { get; set; }
    public double Recall { get; set; }
    public double F1Score { get; set; }
    public int TotalPredictions { get; set; }
    public Dictionary<string, double> CustomMetrics { get; set; } = new();
}

/// <summary>
/// ML model update
/// </summary>
public class MLModelUpdate
{
    public string? Name { get; set; }
    public Dictionary<string, object>? Parameters { get; set; }
    public Dictionary<string, object>? Metadata { get; set; }
}

/// <summary>
/// NLU result
/// </summary>
public class NLUResult
{
    public string Intent { get; set; } = string.Empty;
    public double Confidence { get; set; }
    public List<EntityExtraction> Entities { get; set; } = new();
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Entity extraction result
/// </summary>
public class EntityExtraction
{
    public string Entity { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public double Confidence { get; set; }
    public int StartIndex { get; set; }
    public int EndIndex { get; set; }
    public Dictionary<string, object> Properties { get; set; } = new();
}

/// <summary>
/// Sentiment analysis result
/// </summary>
public class SentimentAnalysis
{
    public string Sentiment { get; set; } = string.Empty; // positive, negative, neutral
    public double Confidence { get; set; }
    public double PositiveScore { get; set; }
    public double NegativeScore { get; set; }
    public double NeutralScore { get; set; }
}

/// <summary>
/// Text classification result
/// </summary>
public class TextClassification
{
    public string Category { get; set; } = string.Empty;
    public double Confidence { get; set; }
    public List<ClassificationScore> AllScores { get; set; } = new();
}

/// <summary>
/// Classification score
/// </summary>
public class ClassificationScore
{
    public string Category { get; set; } = string.Empty;
    public double Score { get; set; }
}

/// <summary>
/// Language detection result
/// </summary>
public class LanguageDetection
{
    public string Language { get; set; } = string.Empty;
    public string LanguageCode { get; set; } = string.Empty;
    public double Confidence { get; set; }
}

/// <summary>
/// Semantic cache entry
/// </summary>
public class SemanticCacheEntry
{
    public string Key { get; set; } = string.Empty;
    public string Query { get; set; } = string.Empty;
    public object Value { get; set; } = new();
    public double SimilarityScore { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Semantic cache statistics
/// </summary>
public class SemanticCacheStatistics
{
    public long TotalEntries { get; set; }
    public long HitCount { get; set; }
    public long MissCount { get; set; }
    public long SemanticHitCount { get; set; }
    public double HitRate => (HitCount + MissCount) > 0 ? (double)HitCount / (HitCount + MissCount) : 0;
    public double SemanticHitRate => (SemanticHitCount + MissCount) > 0 ? (double)SemanticHitCount / (SemanticHitCount + MissCount) : 0;
    public long MemoryUsage { get; set; }
    public DateTime LastUpdated { get; set; }
}

/// <summary>
/// Vector search result
/// </summary>
public class VectorSearchResult
{
    public string DocumentId { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public double Score { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Vector search statistics
/// </summary>
public class VectorSearchStatistics
{
    public long TotalDocuments { get; set; }
    public long TotalSearches { get; set; }
    public double AverageSearchTime { get; set; }
    public long IndexSize { get; set; }
    public DateTime LastIndexUpdate { get; set; }
}

// ISemanticCacheService already defined above - removed duplicate
