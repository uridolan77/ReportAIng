namespace BIReportingCopilot.Core.Models;

using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

/// <summary>
/// Cache performance metrics - Primary definition (consolidates all duplicates)
/// Used across Core.Models, Core.Interfaces, and Infrastructure.AI.Core.Models
/// </summary>
[Table("CachePerformanceMetrics")]
public class CachePerformanceMetrics
{
    /// <summary>
    /// Cache hit rate (0.0 to 1.0)
    /// </summary>
    public double HitRate { get; set; }

    /// <summary>
    /// Cache miss rate (0.0 to 1.0)
    /// </summary>
    public double MissRate { get; set; }

    /// <summary>
    /// Total number of cache requests
    /// </summary>
    public long TotalRequests { get; set; }

    /// <summary>
    /// Average response time for cache operations
    /// </summary>
    public TimeSpan AverageResponseTime { get; set; }

    /// <summary>
    /// Cache throughput (requests per second)
    /// </summary>
    public double ThroughputPerSecond { get; set; }

    /// <summary>
    /// Memory usage in bytes
    /// </summary>
    public long MemoryUsage { get; set; }

    /// <summary>
    /// Cache eviction rate (evictions per second)
    /// </summary>
    public double EvictionRate { get; set; }

    /// <summary>
    /// When these metrics were generated
    /// </summary>
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Additional performance metrics
    /// </summary>
    [NotMapped]
    public Dictionary<string, double> AdditionalMetrics { get; set; } = new();

    /// <summary>
    /// Performance efficiency score (0.0 to 1.0)
    /// </summary>
    public double EfficiencyScore => HitRate * 0.6 + (1.0 - (AverageResponseTime.TotalMilliseconds / 1000.0)) * 0.4;

    // Properties expected by Infrastructure services
    /// <summary>
    /// Average time for cache hits
    /// </summary>
    public TimeSpan AverageHitTime { get; set; }

    /// <summary>
    /// Average time for cache misses
    /// </summary>
    public TimeSpan AverageMissTime { get; set; }

    /// <summary>
    /// Hit ratio (alias for HitRate)
    /// </summary>
    public double HitRatio
    {
        get => HitRate;
        set => HitRate = value;
    }

    /// <summary>
    /// Last updated timestamp (alias for GeneratedAt)
    /// </summary>
    public DateTime LastUpdated
    {
        get => GeneratedAt;
        set => GeneratedAt = value;
    }
}
