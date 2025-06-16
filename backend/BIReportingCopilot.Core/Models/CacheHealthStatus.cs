namespace BIReportingCopilot.Core.Models;

using System.ComponentModel.DataAnnotations.Schema;

/// <summary>
/// Cache health status information
/// </summary>
public class CacheHealthStatus
{
    /// <summary>
    /// Whether the cache is healthy
    /// </summary>
    public bool IsHealthy { get; set; }

    /// <summary>
    /// List of health issues if any
    /// </summary>
    [NotMapped]
    public List<string> Issues { get; set; } = new();

    /// <summary>
    /// Cache hit rate (0.0 to 1.0)
    /// </summary>
    public double HitRate { get; set; }

    /// <summary>
    /// Total number of cache entries
    /// </summary>
    public int TotalEntries { get; set; }

    /// <summary>
    /// When the health check was performed
    /// </summary>
    public DateTime LastChecked { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Additional health metrics
    /// </summary>
    [NotMapped]
    public Dictionary<string, object> AdditionalMetrics { get; set; } = new();

    /// <summary>
    /// Overall health score (0.0 to 1.0)
    /// </summary>
    public double HealthScore => IsHealthy ? (Issues.Count == 0 ? 1.0 : 0.8) : 0.0;
}
