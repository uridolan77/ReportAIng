namespace BIReportingCopilot.Core.Models.Statistics;

/// <summary>
/// Health status for cache services
/// </summary>
public class CacheHealthStatus
{
    /// <summary>
    /// Whether the cache is healthy
    /// </summary>
    public bool IsHealthy { get; set; }

    /// <summary>
    /// Cache service status
    /// </summary>
    public string Status { get; set; } = "Unknown";

    /// <summary>
    /// Total memory usage in bytes
    /// </summary>
    public long MemoryUsage { get; set; }

    /// <summary>
    /// Maximum memory limit in bytes
    /// </summary>
    public long MemoryLimit { get; set; }

    /// <summary>
    /// Memory usage percentage
    /// </summary>
    public double MemoryUsagePercentage { get; set; }

    /// <summary>
    /// Number of cached items
    /// </summary>
    public long ItemCount { get; set; }

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
    /// Number of evicted items
    /// </summary>
    public long EvictedItems { get; set; }

    /// <summary>
    /// Connection status to cache backend
    /// </summary>
    public bool IsConnected { get; set; }

    /// <summary>
    /// Last connection attempt timestamp
    /// </summary>
    public DateTime LastConnectionAttempt { get; set; }

    /// <summary>
    /// Error message if unhealthy
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Additional health metrics
    /// </summary>
    public Dictionary<string, object> AdditionalMetrics { get; set; } = new();

    /// <summary>
    /// Last health check timestamp
    /// </summary>
    public DateTime LastHealthCheck { get; set; } = DateTime.UtcNow;
}
