using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BIReportingCopilot.Core.Models;

/// <summary>
/// Enhanced semantic cache entry with vector embeddings
/// </summary>
public class EnhancedSemanticCacheEntry
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    [Required]
    public string OriginalQuery { get; set; } = string.Empty;
    
    [Required]
    public string NormalizedQuery { get; set; } = string.Empty;
    
    [Required]
    public string SqlQuery { get; set; } = string.Empty;
    
    public string SerializedResponse { get; set; } = string.Empty;
    
    /// <summary>
    /// Vector embedding for semantic similarity search
    /// </summary>
    [NotMapped]
    public float[] Embedding { get; set; } = Array.Empty<float>();
    
    /// <summary>
    /// Embedding model used for generating the vector
    /// </summary>
    public string EmbeddingModel { get; set; } = string.Empty;
    
    /// <summary>
    /// Semantic features extracted from the query
    /// </summary>
    [NotMapped]
    public SemanticFeatures SemanticFeatures { get; set; } = new();
    
    /// <summary>
    /// Query classification and intent
    /// </summary>
    [NotMapped]
    public CacheQueryClassification Classification { get; set; } = new();
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime LastAccessedAt { get; set; } = DateTime.UtcNow;
    public DateTime ExpiresAt { get; set; }
    
    public int AccessCount { get; set; } = 0;
    public double ConfidenceScore { get; set; }

    /// <summary>
    /// Performance metrics for this cache entry
    /// </summary>
    [NotMapped]
    public CachePerformanceMetrics Performance { get; set; } = new();

    /// <summary>
    /// Similarity score for search results (computed at query time)
    /// </summary>
    public double SimilarityScore { get; set; }
    
    /// <summary>
    /// Additional metadata for advanced features
    /// </summary>
    [NotMapped]
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Semantic features extracted from natural language queries
/// </summary>
public class SemanticFeatures
{
    /// <summary>
    /// Key entities mentioned in the query (tables, columns, values)
    /// </summary>
    [NotMapped]
    public List<string> Entities { get; set; } = new();
    
    /// <summary>
    /// Query intent (aggregation, filtering, sorting, etc.)
    /// </summary>
    [NotMapped]
    public List<string> Intents { get; set; } = new();
    
    /// <summary>
    /// Temporal expressions (yesterday, last week, etc.)
    /// </summary>
    [NotMapped]
    public List<string> TemporalExpressions { get; set; } = new();
    
    /// <summary>
    /// Numerical expressions and comparisons
    /// </summary>
    [NotMapped]
    public List<string> NumericalExpressions { get; set; } = new();
    
    /// <summary>
    /// Business domain concepts
    /// </summary>
    [NotMapped]
    public List<string> DomainConcepts { get; set; } = new();
    
    /// <summary>
    /// Complexity score of the query
    /// </summary>
    public double ComplexityScore { get; set; }
    
    /// <summary>
    /// Language and linguistic features
    /// </summary>
    [NotMapped]
    public LinguisticFeatures Linguistic { get; set; } = new();
}

/// <summary>
/// Linguistic features for advanced NLU
/// </summary>
public class LinguisticFeatures
{
    public string Language { get; set; } = "en";
    public List<string> Keywords { get; set; } = new();
    public List<string> Phrases { get; set; } = new();
    public string SentimentPolarity { get; set; } = "neutral";
    public double SentimentScore { get; set; } = 0.0;
    public List<string> NamedEntities { get; set; } = new();
    public string QueryType { get; set; } = "unknown";
}

/// <summary>
/// Query classification for better caching strategies
/// </summary>
public class CacheQueryClassification
{
    /// <summary>
    /// Primary category (analytical, operational, exploratory)
    /// </summary>
    public string Category { get; set; } = "analytical";
    
    /// <summary>
    /// Specific type (aggregation, filter, join, etc.)
    /// </summary>
    public string Type { get; set; } = "unknown";
    
    /// <summary>
    /// Business domain (finance, marketing, operations, etc.)
    /// </summary>
    public string Domain { get; set; } = "general";
    
    /// <summary>
    /// Complexity level (simple, medium, complex)
    /// </summary>
    public string Complexity { get; set; } = "medium";
    
    /// <summary>
    /// Expected result size category
    /// </summary>
    public string ResultSize { get; set; } = "medium";
    
    /// <summary>
    /// Caching strategy recommendation
    /// </summary>
    public string CachingStrategy { get; set; } = "standard";
    
    /// <summary>
    /// Classification confidence score
    /// </summary>
    public double Confidence { get; set; } = 0.0;
}

/// <summary>
/// Performance metrics for semantic cache entries (specific to semantic caching)
/// </summary>
public class SemanticCachePerformanceMetrics
{
    public TimeSpan OriginalExecutionTime { get; set; }
    public TimeSpan AverageCacheRetrievalTime { get; set; }
    public double SpeedupRatio { get; set; }
    public long ResultSizeBytes { get; set; }
    public int HitCount { get; set; }
    public DateTime LastHit { get; set; }
    public List<CacheAccessLog> AccessHistory { get; set; } = new();
}

/// <summary>
/// Cache access log entry
/// </summary>
public class CacheAccessLog
{
    public DateTime AccessTime { get; set; }
    public string UserId { get; set; } = string.Empty;
    public TimeSpan RetrievalTime { get; set; }
    public double SimilarityScore { get; set; }
    public string AccessType { get; set; } = "hit"; // hit, miss, partial
}

/// <summary>
/// Configuration for semantic caching behavior
/// </summary>
public class SemanticCacheConfiguration
{
    /// <summary>
    /// Minimum similarity threshold for cache hits
    /// </summary>
    public double SimilarityThreshold { get; set; } = 0.85;
    
    /// <summary>
    /// Maximum number of similar queries to return
    /// </summary>
    public int MaxSimilarQueries { get; set; } = 5;
    
    /// <summary>
    /// Default cache expiration time
    /// </summary>
    public TimeSpan DefaultExpiration { get; set; } = TimeSpan.FromHours(24);
    
    /// <summary>
    /// Maximum cache size (number of entries)
    /// </summary>
    public int MaxCacheSize { get; set; } = 10000;
    
    /// <summary>
    /// Enable/disable vector-based semantic search
    /// </summary>
    public bool EnableVectorSearch { get; set; } = true;
    
    /// <summary>
    /// Embedding model to use for vector generation
    /// </summary>
    public string EmbeddingModel { get; set; } = "text-embedding-ada-002";
    
    /// <summary>
    /// Batch size for embedding generation
    /// </summary>
    public int EmbeddingBatchSize { get; set; } = 100;
    
    /// <summary>
    /// Enable automatic cache optimization
    /// </summary>
    public bool EnableAutoOptimization { get; set; } = true;
    
    /// <summary>
    /// Cache cleanup interval
    /// </summary>
    public TimeSpan CleanupInterval { get; set; } = TimeSpan.FromHours(6);
}

/// <summary>
/// Result of semantic cache lookup
/// </summary>
public class SemanticCacheResult
{
    public bool IsHit { get; set; }
    public double SimilarityScore { get; set; }
    public QueryResponse? CachedResponse { get; set; }
    public EnhancedSemanticCacheEntry? CacheEntry { get; set; }
    public List<EnhancedSemanticCacheEntry> SimilarQueries { get; set; } = new();
    public TimeSpan LookupTime { get; set; }
    public string CacheStrategy { get; set; } = "vector";
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Semantic cache entry (alias for compatibility)
/// </summary>
public class SemanticCacheEntry : EnhancedSemanticCacheEntry
{
    /// <summary>
    /// Serialized response data
    /// </summary>
    public new string SerializedResponse { get; set; } = string.Empty;

    // Additional properties expected by Infrastructure services
    public string QueryHash { get; set; } = string.Empty;
    public string CreatedBy { get; set; } = string.Empty;
    public string UpdatedBy { get; set; } = string.Empty;
    public string GeneratedSql { get; set; } = string.Empty;
    
    [NotMapped]
    public Dictionary<string, object> ResultData { get; set; } = new();
}

/// <summary>
/// Semantic cache statistics
/// </summary>
public class SemanticCacheStatistics
{
    /// <summary>
    /// Total number of cache entries
    /// </summary>
    public long TotalEntries { get; set; }

    /// <summary>
    /// Cache hit count
    /// </summary>
    public long HitCount { get; set; }

    /// <summary>
    /// Cache miss count
    /// </summary>
    public long MissCount { get; set; }

    /// <summary>
    /// Cache hit rate (0.0 to 1.0)
    /// </summary>
    public double HitRate => (HitCount + MissCount) > 0 ? (double)HitCount / (HitCount + MissCount) : 0.0;

    /// <summary>
    /// Total size in bytes
    /// </summary>
    public long TotalSizeBytes { get; set; }

    /// <summary>
    /// Average similarity score
    /// </summary>
    public double AverageSimilarityScore { get; set; }

    /// <summary>
    /// Last updated timestamp
    /// </summary>
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Performance metrics
    /// </summary>
    public Dictionary<string, object> PerformanceMetrics { get; set; } = new();

    /// <summary>
    /// Semantic cache hits (alias for HitCount)
    /// </summary>
    public long SemanticCacheHits => HitCount;
}
