namespace BIReportingCopilot.Core.Interfaces;

/// <summary>
/// Performance metrics for cache operations
/// </summary>
public class CachePerformanceMetrics
{
    /// <summary>
    /// Total number of cache operations
    /// </summary>
    public long TotalOperations { get; set; }

    /// <summary>
    /// Number of cache hits
    /// </summary>
    public long CacheHits { get; set; }

    /// <summary>
    /// Number of cache misses
    /// </summary>
    public long CacheMisses { get; set; }

    /// <summary>
    /// Cache hit rate percentage
    /// </summary>
    public double HitRate { get; set; }

    /// <summary>
    /// Cache miss rate percentage
    /// </summary>
    public double MissRate { get; set; }

    /// <summary>
    /// Average response time in milliseconds
    /// </summary>
    public double AverageResponseTime { get; set; }

    /// <summary>
    /// Total memory usage in bytes
    /// </summary>
    public long MemoryUsage { get; set; }

    /// <summary>
    /// Number of cached items
    /// </summary>
    public long ItemCount { get; set; }

    /// <summary>
    /// Number of evicted items
    /// </summary>
    public long EvictedItems { get; set; }

    /// <summary>
    /// Cache efficiency score
    /// </summary>
    public double EfficiencyScore { get; set; }

    /// <summary>
    /// Last update timestamp
    /// </summary>
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
}
