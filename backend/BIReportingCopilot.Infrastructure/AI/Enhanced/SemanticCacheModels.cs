using Microsoft.Extensions.Logging;
using BIReportingCopilot.Core.Models;
using BIReportingCopilot.Infrastructure.Performance;
using System.Numerics;

namespace BIReportingCopilot.Infrastructure.AI.Enhanced;

/// <summary>
/// Semantic cache entry with vector embedding
/// </summary>
public class SemanticCacheEntry
{
    public string Id { get; set; } = string.Empty;
    public string Query { get; set; } = string.Empty;
    public float[] QueryEmbedding { get; set; } = Array.Empty<float>();
    public CachedResult Result { get; set; } = new();
    public string? UserId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime LastAccessedAt { get; set; }
    public int AccessCount { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Similar cache entry with similarity score
/// </summary>
public class SimilarCacheEntry
{
    public SemanticCacheEntry CacheEntry { get; set; } = new();
    public double SimilarityScore { get; set; }
    public double RankingScore { get; set; }
    public CachedResult CachedResult { get; set; } = new();
}

/// <summary>
/// Similar query result
/// </summary>
public class SimilarQuery
{
    public string Query { get; set; } = string.Empty;
    public double SimilarityScore { get; set; }
    public DateTime LastUsed { get; set; }
    public int UsageCount { get; set; }
}

/// <summary>
/// Cache analytics and performance metrics
/// </summary>
public class CacheAnalytics
{
    public TimeSpan Period { get; set; }
    public int TotalQueries { get; set; }
    public int CacheHits { get; set; }
    public int CacheMisses { get; set; }
    public double HitRate { get; set; }
    public double AverageSimilarityScore { get; set; }
    public int UniqueQueries { get; set; }
    public long StorageUsed { get; set; }
    public List<string> OptimizationRecommendations { get; set; } = new();
    public Dictionary<string, double> PerformanceMetrics { get; set; } = new();
    public DateTime GeneratedAt { get; set; }
}

/// <summary>
/// Cache statistics for tracking
/// </summary>
public class CacheStatistics
{
    public int TotalQueries { get; set; }
    public int CacheHits { get; set; }
    public int CacheMisses { get; set; }
    public double TotalSimilarityScore { get; set; }
    public int SimilarityScoreCount { get; set; }
    public double AverageSimilarityScore { get; set; }
    public int UniqueQueries { get; set; }
}

/// <summary>
/// Semantic cache configuration
/// </summary>
public class SemanticCacheConfiguration
{
    public bool EnableSemanticCache { get; set; } = true;
    public double MinimumSimilarityThreshold { get; set; } = 0.85;
    public int MaxCacheEntries { get; set; } = 10000;
    public TimeSpan DefaultTtl { get; set; } = TimeSpan.FromHours(24);
    public int EmbeddingDimensions { get; set; } = 384;
    public string EmbeddingModel { get; set; } = "all-MiniLM-L6-v2";
    public bool EnableVectorOptimization { get; set; } = true;
    public bool EnableCacheAnalytics { get; set; } = true;
    public TimeSpan OptimizationInterval { get; set; } = TimeSpan.FromHours(6);
}

/// <summary>
/// Vector embedding service for generating query embeddings
/// </summary>
public class VectorEmbeddingService
{
    private readonly ILogger _logger;
    private readonly SemanticCacheConfiguration _config;

    public VectorEmbeddingService(ILogger logger, SemanticCacheConfiguration config)
    {
        _logger = logger;
        _config = config;
    }

    /// <summary>
    /// Generate vector embedding for a query
    /// </summary>
    public async Task<float[]> GenerateEmbeddingAsync(string text)
    {
        try
        {
            // In a real implementation, this would call a sentence transformer model
            // For now, we'll create a simple hash-based embedding
            var embedding = await GenerateSimpleEmbeddingAsync(text);
            
            _logger.LogDebug("Generated embedding for text: {Text} (length: {Length})", 
                text.Substring(0, Math.Min(50, text.Length)), embedding.Length);
            
            return embedding;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating embedding for text");
            return new float[_config.EmbeddingDimensions];
        }
    }

    /// <summary>
    /// Generate embeddings for multiple texts in batch
    /// </summary>
    public async Task<List<float[]>> GenerateBatchEmbeddingsAsync(List<string> texts)
    {
        var embeddings = new List<float[]>();
        
        foreach (var text in texts)
        {
            var embedding = await GenerateEmbeddingAsync(text);
            embeddings.Add(embedding);
        }
        
        return embeddings;
    }

    /// <summary>
    /// Get embedding model information
    /// </summary>
    public EmbeddingModelInfo GetModelInfo()
    {
        return new EmbeddingModelInfo
        {
            ModelName = _config.EmbeddingModel,
            Dimensions = _config.EmbeddingDimensions,
            MaxSequenceLength = 512,
            IsNormalized = true
        };
    }

    private async Task<float[]> GenerateSimpleEmbeddingAsync(string text)
    {
        // Simple embedding generation for demonstration
        // In production, use a proper sentence transformer model
        
        var embedding = new float[_config.EmbeddingDimensions];
        var hash = text.GetHashCode();
        var random = new Random(hash);
        
        // Generate normalized random vector based on text hash
        for (int i = 0; i < _config.EmbeddingDimensions; i++)
        {
            embedding[i] = (float)(random.NextDouble() * 2.0 - 1.0);
        }
        
        // Normalize the vector
        var magnitude = (float)Math.Sqrt(embedding.Sum(x => x * x));
        if (magnitude > 0)
        {
            for (int i = 0; i < embedding.Length; i++)
            {
                embedding[i] /= magnitude;
            }
        }
        
        return embedding;
    }
}

/// <summary>
/// Semantic similarity engine for comparing embeddings
/// </summary>
public class SemanticSimilarityEngine
{
    private readonly ILogger _logger;
    private readonly SemanticCacheConfiguration _config;

    public SemanticSimilarityEngine(ILogger logger, SemanticCacheConfiguration config)
    {
        _logger = logger;
        _config = config;
    }

    /// <summary>
    /// Calculate cosine similarity between two embeddings
    /// </summary>
    public double CalculateCosineSimilarity(float[] embedding1, float[] embedding2)
    {
        try
        {
            if (embedding1.Length != embedding2.Length)
            {
                _logger.LogWarning("Embedding dimensions mismatch: {Dim1} vs {Dim2}", 
                    embedding1.Length, embedding2.Length);
                return 0.0;
            }

            var dotProduct = 0.0;
            var magnitude1 = 0.0;
            var magnitude2 = 0.0;

            for (int i = 0; i < embedding1.Length; i++)
            {
                dotProduct += embedding1[i] * embedding2[i];
                magnitude1 += embedding1[i] * embedding1[i];
                magnitude2 += embedding2[i] * embedding2[i];
            }

            magnitude1 = Math.Sqrt(magnitude1);
            magnitude2 = Math.Sqrt(magnitude2);

            if (magnitude1 == 0.0 || magnitude2 == 0.0)
            {
                return 0.0;
            }

            var similarity = dotProduct / (magnitude1 * magnitude2);
            return Math.Max(0.0, Math.Min(1.0, similarity)); // Clamp to [0, 1]
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating cosine similarity");
            return 0.0;
        }
    }

    /// <summary>
    /// Calculate semantic distance between queries
    /// </summary>
    public double CalculateSemanticDistance(string query1, string query2)
    {
        // Simple semantic distance based on text features
        var distance = 0.0;

        // Length difference factor
        var lengthDiff = Math.Abs(query1.Length - query2.Length);
        var maxLength = Math.Max(query1.Length, query2.Length);
        distance += (lengthDiff / (double)maxLength) * 0.2;

        // Word overlap factor
        var words1 = query1.ToLowerInvariant().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var words2 = query2.ToLowerInvariant().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var commonWords = words1.Intersect(words2).Count();
        var totalWords = words1.Union(words2).Count();
        var wordOverlap = totalWords > 0 ? (double)commonWords / totalWords : 0.0;
        distance += (1.0 - wordOverlap) * 0.8;

        return Math.Max(0.0, Math.Min(1.0, distance));
    }

    /// <summary>
    /// Find most similar embeddings using approximate nearest neighbor search
    /// </summary>
    public async Task<List<SimilarityResult>> FindSimilarEmbeddingsAsync(
        float[] queryEmbedding, 
        List<float[]> candidateEmbeddings, 
        int maxResults = 10)
    {
        var results = new List<SimilarityResult>();

        for (int i = 0; i < candidateEmbeddings.Count; i++)
        {
            var similarity = CalculateCosineSimilarity(queryEmbedding, candidateEmbeddings[i]);
            
            if (similarity >= _config.MinimumSimilarityThreshold)
            {
                results.Add(new SimilarityResult
                {
                    Index = i,
                    Similarity = similarity
                });
            }
        }

        return results
            .OrderByDescending(r => r.Similarity)
            .Take(maxResults)
            .ToList();
    }
}

/// <summary>
/// Cache optimizer for performance and storage optimization
/// </summary>
public class CacheOptimizer
{
    private readonly ILogger _logger;
    private readonly ICacheService _cacheService;

    public CacheOptimizer(ILogger logger, ICacheService cacheService)
    {
        _logger = logger;
        _cacheService = cacheService;
    }

    /// <summary>
    /// Update optimization data with new cache entry
    /// </summary>
    public async Task UpdateOptimizationDataAsync(SemanticCacheEntry entry)
    {
        try
        {
            var optimizationKey = "semantic_cache:optimization_data";
            var data = await _cacheService.GetAsync<CacheOptimizationData>(optimizationKey) 
                ?? new CacheOptimizationData();

            data.TotalEntries++;
            data.LastUpdated = DateTime.UtcNow;
            
            // Track query patterns
            var queryLength = entry.Query.Length;
            data.QueryLengthDistribution[GetLengthBucket(queryLength)]++;
            
            // Track access patterns
            data.AccessPatterns[entry.UserId ?? "anonymous"]++;

            await _cacheService.SetAsync(optimizationKey, data, TimeSpan.FromDays(30));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating optimization data");
        }
    }

    /// <summary>
    /// Get optimization analysis and recommendations
    /// </summary>
    public async Task<OptimizationAnalysis> GetOptimizationAnalysisAsync(TimeSpan period)
    {
        try
        {
            var optimizationKey = "semantic_cache:optimization_data";
            var data = await _cacheService.GetAsync<CacheOptimizationData>(optimizationKey) 
                ?? new CacheOptimizationData();

            var recommendations = GenerateOptimizationRecommendations(data);
            var performanceMetrics = CalculatePerformanceMetrics(data);

            return new OptimizationAnalysis
            {
                Period = period,
                Recommendations = recommendations,
                PerformanceMetrics = performanceMetrics,
                OptimizationData = data,
                GeneratedAt = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating optimization analysis");
            return new OptimizationAnalysis
            {
                Period = period,
                GeneratedAt = DateTime.UtcNow
            };
        }
    }

    private List<string> GenerateOptimizationRecommendations(CacheOptimizationData data)
    {
        var recommendations = new List<string>();

        // Storage optimization
        if (data.TotalEntries > 8000)
        {
            recommendations.Add("Consider increasing cache cleanup frequency - approaching storage limits");
        }

        // Access pattern optimization
        var topUsers = data.AccessPatterns
            .OrderByDescending(kvp => kvp.Value)
            .Take(5)
            .ToList();

        if (topUsers.Any() && topUsers.First().Value > data.TotalEntries * 0.3)
        {
            recommendations.Add("Consider user-specific cache optimization for heavy users");
        }

        // Query pattern optimization
        var longQueries = data.QueryLengthDistribution.Where(kvp => kvp.Key > 500).Sum(kvp => kvp.Value);
        if (longQueries > data.TotalEntries * 0.2)
        {
            recommendations.Add("Consider query compression for long queries to optimize storage");
        }

        return recommendations;
    }

    private Dictionary<string, double> CalculatePerformanceMetrics(CacheOptimizationData data)
    {
        return new Dictionary<string, double>
        {
            ["total_entries"] = data.TotalEntries,
            ["avg_query_length"] = CalculateAverageQueryLength(data),
            ["user_diversity"] = data.AccessPatterns.Count,
            ["storage_efficiency"] = CalculateStorageEfficiency(data)
        };
    }

    private double CalculateAverageQueryLength(CacheOptimizationData data)
    {
        var totalLength = 0.0;
        var totalQueries = 0;

        foreach (var (lengthBucket, count) in data.QueryLengthDistribution)
        {
            totalLength += lengthBucket * count;
            totalQueries += count;
        }

        return totalQueries > 0 ? totalLength / totalQueries : 0.0;
    }

    private double CalculateStorageEfficiency(CacheOptimizationData data)
    {
        // Simple efficiency calculation based on entry count and distribution
        var efficiency = 1.0;
        
        if (data.TotalEntries > 5000)
        {
            efficiency *= 0.9; // Reduce efficiency for large caches
        }

        return efficiency;
    }

    private int GetLengthBucket(int length)
    {
        return (length / 100) * 100; // Bucket by hundreds
    }
}

/// <summary>
/// Embedding model information
/// </summary>
public class EmbeddingModelInfo
{
    public string ModelName { get; set; } = string.Empty;
    public int Dimensions { get; set; }
    public int MaxSequenceLength { get; set; }
    public bool IsNormalized { get; set; }
}

/// <summary>
/// Similarity search result
/// </summary>
public class SimilarityResult
{
    public int Index { get; set; }
    public double Similarity { get; set; }
}

/// <summary>
/// Cache optimization data
/// </summary>
public class CacheOptimizationData
{
    public int TotalEntries { get; set; }
    public DateTime LastUpdated { get; set; }
    public Dictionary<int, int> QueryLengthDistribution { get; set; } = new();
    public Dictionary<string, int> AccessPatterns { get; set; } = new();
}

/// <summary>
/// Optimization analysis result
/// </summary>
public class OptimizationAnalysis
{
    public TimeSpan Period { get; set; }
    public List<string> Recommendations { get; set; } = new();
    public Dictionary<string, double> PerformanceMetrics { get; set; } = new();
    public CacheOptimizationData OptimizationData { get; set; } = new();
    public DateTime GeneratedAt { get; set; }
}
