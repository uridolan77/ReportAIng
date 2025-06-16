using BIReportingCopilot.Core.Models;

namespace BIReportingCopilot.Core.Interfaces.Cache;

/// <summary>
/// Core cache service interface
/// </summary>
public interface ICacheService
{    Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class;
    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default);
    Task RemoveAsync(string key, CancellationToken cancellationToken = default);
    Task RemovePatternAsync(string pattern, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default);
    Task ClearAsync(CancellationToken cancellationToken = default);
    Task ClearAllAsync(CancellationToken cancellationToken = default);
    Task<CacheStatistics> GetStatisticsAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Semantic cache service interface for AI-powered caching
/// </summary>
public interface ISemanticCacheService
{    Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class;
    Task<T?> GetSemanticAsync<T>(string query, double similarityThreshold = 0.8, CancellationToken cancellationToken = default) where T : class;
    Task SetAsync<T>(string key, T value, TimeSpan? expiry = null, CancellationToken cancellationToken = default);
    Task SetSemanticAsync<T>(string query, T value, TimeSpan? expiry = null, CancellationToken cancellationToken = default);
    Task RemoveAsync(string key, CancellationToken cancellationToken = default);
    Task RemoveSemanticAsync(string query, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default);
    Task<List<Models.SemanticCacheEntry>> FindSimilarAsync(string query, double threshold = 0.8, int maxResults = 10, CancellationToken cancellationToken = default);
    Task<SemanticCacheStatistics> GetStatisticsAsync(CancellationToken cancellationToken = default);
    Task<Models.CachePerformanceMetrics> GetCachePerformanceMetricsAsync(CancellationToken cancellationToken = default);
    Task OptimizeCacheAsync(CancellationToken cancellationToken = default);
    Task InvalidateCacheByPatternAsync(string pattern, CancellationToken cancellationToken = default);
    Task ClearSemanticCacheAsync(CancellationToken cancellationToken = default);
}

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
/// Semantic cache statistics
/// </summary>
public class SemanticCacheStatistics : CacheStatistics
{
    public long SemanticHits { get; set; }
    public long ExactHits { get; set; }
    public double AverageSimilarityScore { get; set; }
    public long VectorOperations { get; set; }
}

// SemanticCacheEntry moved to Core.Models.SemanticCaching.cs to eliminate duplicates
// Use: BIReportingCopilot.Core.Models.SemanticCacheEntry instead
