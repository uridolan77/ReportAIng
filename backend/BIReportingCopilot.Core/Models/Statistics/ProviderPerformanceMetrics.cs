namespace BIReportingCopilot.Core.Models.Statistics;

/// <summary>
/// Performance metrics for AI providers
/// </summary>
public class ProviderPerformanceMetrics
{
    /// <summary>
    /// Provider identifier
    /// </summary>
    public string ProviderId { get; set; } = string.Empty;

    /// <summary>
    /// Provider name
    /// </summary>
    public string ProviderName { get; set; } = string.Empty;

    /// <summary>
    /// Total number of requests
    /// </summary>
    public long TotalRequests { get; set; }

    /// <summary>
    /// Number of successful requests
    /// </summary>
    public long SuccessfulRequests { get; set; }

    /// <summary>
    /// Number of failed requests
    /// </summary>
    public long FailedRequests { get; set; }

    /// <summary>
    /// Average response time in milliseconds
    /// </summary>
    public double AverageResponseTime { get; set; }

    /// <summary>
    /// Minimum response time in milliseconds
    /// </summary>
    public double MinResponseTime { get; set; }

    /// <summary>
    /// Maximum response time in milliseconds
    /// </summary>
    public double MaxResponseTime { get; set; }

    /// <summary>
    /// Success rate percentage
    /// </summary>
    public double SuccessRate { get; set; }

    /// <summary>
    /// Error rate percentage
    /// </summary>
    public double ErrorRate { get; set; }

    /// <summary>
    /// Total cost incurred
    /// </summary>
    public decimal TotalCost { get; set; }

    /// <summary>
    /// Average cost per request
    /// </summary>
    public decimal AverageCostPerRequest { get; set; }

    /// <summary>
    /// Provider availability percentage
    /// </summary>
    public double Availability { get; set; }

    /// <summary>
    /// Last health check timestamp
    /// </summary>
    public DateTime LastHealthCheck { get; set; }

    /// <summary>
    /// Performance trend over time
    /// </summary>
    public Dictionary<DateTime, double> PerformanceTrend { get; set; } = new();

    /// <summary>
    /// Error distribution by type
    /// </summary>
    public Dictionary<string, int> ErrorDistribution { get; set; } = new();

    /// <summary>
    /// Last update timestamp
    /// </summary>
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
}
