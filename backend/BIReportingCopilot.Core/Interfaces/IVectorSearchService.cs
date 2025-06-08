using BIReportingCopilot.Core.Models;

namespace BIReportingCopilot.Core.Interfaces;

/// <summary>
/// Interface for vector search operations supporting semantic caching
/// Provides embedding-based similarity search for natural language queries
/// </summary>
public interface IVectorSearchService
{
    /// <summary>
    /// Generate embedding vector for a text query
    /// </summary>
    Task<float[]> GenerateEmbeddingAsync(string text, CancellationToken cancellationToken = default);

    /// <summary>
    /// Store query embedding with metadata for future similarity search
    /// </summary>
    Task<string> StoreQueryEmbeddingAsync(
        string queryText, 
        string sqlQuery, 
        QueryResponse response, 
        float[] embedding,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Find semantically similar queries using vector similarity search
    /// </summary>
    Task<List<SemanticSearchResult>> FindSimilarQueriesAsync(
        float[] queryEmbedding, 
        double similarityThreshold = 0.8, 
        int maxResults = 5,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Find semantically similar queries by text (generates embedding internally)
    /// </summary>
    Task<List<SemanticSearchResult>> FindSimilarQueriesByTextAsync(
        string queryText, 
        double similarityThreshold = 0.8, 
        int maxResults = 5,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Calculate cosine similarity between two embedding vectors
    /// </summary>
    double CalculateCosineSimilarity(float[] vector1, float[] vector2);

    /// <summary>
    /// Update embedding for an existing query (for retraining/optimization)
    /// </summary>
    Task<bool> UpdateQueryEmbeddingAsync(
        string embeddingId, 
        float[] newEmbedding,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete embedding by ID
    /// </summary>
    Task<bool> DeleteQueryEmbeddingAsync(string embeddingId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get embedding statistics and health metrics
    /// </summary>
    Task<VectorSearchMetrics> GetMetricsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Batch process multiple queries for embedding generation
    /// </summary>
    Task<List<BatchEmbeddingResult>> GenerateBatchEmbeddingsAsync(
        List<string> texts, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Optimize vector index for better search performance
    /// </summary>
    Task OptimizeIndexAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Clear the entire vector index
    /// </summary>
    Task ClearIndexAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Invalidate embeddings by pattern (e.g., by query pattern or metadata)
    /// </summary>
    Task InvalidateByPatternAsync(string pattern, CancellationToken cancellationToken = default);
}

/// <summary>
/// Result of semantic search operation
/// </summary>
public class SemanticSearchResult
{
    public string EmbeddingId { get; set; } = string.Empty;
    public string OriginalQuery { get; set; } = string.Empty;
    public string SqlQuery { get; set; } = string.Empty;
    public QueryResponse? CachedResponse { get; set; }
    public double SimilarityScore { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime LastAccessed { get; set; }
    public int AccessCount { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Metrics for vector search performance
/// </summary>
public class VectorSearchMetrics
{
    public int TotalEmbeddings { get; set; }
    public int TotalSearches { get; set; }
    public double AverageSearchTime { get; set; }
    public double CacheHitRate { get; set; }
    public DateTime LastOptimized { get; set; }
    public long IndexSizeBytes { get; set; }
    public Dictionary<string, object> PerformanceMetrics { get; set; } = new();
}

/// <summary>
/// Result of batch embedding generation
/// </summary>
public class BatchEmbeddingResult
{
    public string Text { get; set; } = string.Empty;
    public float[] Embedding { get; set; } = Array.Empty<float>();
    public bool Success { get; set; }
    public string? Error { get; set; }
    public TimeSpan ProcessingTime { get; set; }
}
