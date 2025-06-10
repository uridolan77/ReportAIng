using BIReportingCopilot.Core.Models;

namespace BIReportingCopilot.Core.Interfaces;

/// <summary>
/// Interface for semantic cache service that provides intelligent caching based on query semantics
/// </summary>
public interface ISemanticCacheService
{
    /// <summary>
    /// Get cached result for a semantically similar query
    /// </summary>
    Task<QueryResponse?> GetSemanticCacheAsync(string query, double similarityThreshold = 0.8);

    /// <summary>
    /// Store query result in semantic cache
    /// </summary>
    Task SetSemanticCacheAsync(string query, QueryResponse response, TimeSpan? expiry = null);

    /// <summary>
    /// Find semantically similar queries
    /// </summary>
    Task<List<EnhancedSemanticCacheEntry>> FindSimilarQueriesAsync(string query, int limit = 5);

    /// <summary>
    /// Get cache statistics
    /// </summary>
    Task<CacheStatistics> GetCacheStatisticsAsync();

    /// <summary>
    /// Clear semantic cache
    /// </summary>
    Task ClearSemanticCacheAsync();

    /// <summary>
    /// Invalidate cache entries by pattern
    /// </summary>
    Task InvalidateCacheByPatternAsync(string pattern);

    /// <summary>
    /// Get cache entry by key
    /// </summary>
    Task<EnhancedSemanticCacheEntry?> GetCacheEntryAsync(string key);

    /// <summary>
    /// Update cache entry metadata
    /// </summary>
    Task UpdateCacheEntryMetadataAsync(string key, Dictionary<string, object> metadata);

    /// <summary>
    /// Get cache performance metrics
    /// </summary>
    Task<CachePerformanceMetrics> GetCachePerformanceMetricsAsync();

    /// <summary>
    /// Optimize cache based on usage patterns
    /// </summary>
    Task OptimizeCacheAsync();

    /// <summary>
    /// Get cache health status
    /// </summary>
    Task<CacheHealthStatus> GetCacheHealthAsync();

    /// <summary>
    /// Cache a semantic query with its result
    /// </summary>
    Task CacheSemanticQueryAsync(string naturalLanguageQuery, string sqlQuery, QueryResponse response, TimeSpan? expiry = null);

    /// <summary>
    /// Get semantically similar queries from cache
    /// </summary>
    Task<List<SemanticCacheResult>> GetSemanticallySimilarAsync(string query, double threshold = 0.8, int limit = 5);

    /// <summary>
    /// Get semantically similar query result from cache
    /// </summary>
    Task<SemanticCacheResult?> GetSemanticallySimilarAsync(string naturalLanguageQuery, string sqlQuery);

    /// <summary>
    /// Invalidate cache entries based on data changes
    /// </summary>
    Task InvalidateByDataChangeAsync(string tableName, string changeType);

    /// <summary>
    /// Clean up expired cache entries
    /// </summary>
    Task CleanupExpiredEntriesAsync();
}

/// <summary>
/// Cache performance metrics
/// </summary>
public class CachePerformanceMetrics
{
    public double HitRate { get; set; }
    public double MissRate { get; set; }
    public TimeSpan AverageRetrievalTime { get; set; }
    public long TotalRequests { get; set; }
    public long TotalHits { get; set; }
    public long TotalMisses { get; set; }
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

    // Additional properties expected by Infrastructure services
    public TimeSpan AverageHitTime { get; set; }
    public TimeSpan AverageMissTime { get; set; }
    public double HitRatio { get; set; }
}

/// <summary>
/// Cache health status
/// </summary>
public class CacheHealthStatus
{
    public bool IsHealthy { get; set; }
    public string Status { get; set; } = string.Empty;
    public List<string> Issues { get; set; } = new();
    public Dictionary<string, object> Metrics { get; set; } = new();
    public DateTime CheckedAt { get; set; } = DateTime.UtcNow;

    // Additional properties expected by Infrastructure services
    public DateTime LastChecked { get; set; } = DateTime.UtcNow;
}
