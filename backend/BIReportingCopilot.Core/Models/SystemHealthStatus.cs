namespace BIReportingCopilot.Core.Models;

/// <summary>
/// System health status information
/// </summary>
public class SystemHealthStatus
{
    /// <summary>
    /// Whether the system is healthy
    /// </summary>
    public bool IsHealthy { get; set; }

    /// <summary>
    /// Overall health status
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// When the health check was performed
    /// </summary>
    public DateTime LastChecked { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Health status of individual components
    /// </summary>
    public Dictionary<string, ComponentHealthStatus> Components { get; set; } = new();

    /// <summary>
    /// Additional health metrics
    /// </summary>
    public Dictionary<string, object> Metrics { get; set; } = new();

    /// <summary>
    /// Overall health score (0.0 to 1.0)
    /// </summary>
    public double HealthScore => IsHealthy ? (Components.Values.All(c => c.IsHealthy) ? 1.0 : 0.8) : 0.0;

    /// <summary>
    /// List of health issues if any
    /// </summary>
    public List<string> Issues => Components.Values
        .Where(c => !c.IsHealthy)
        .Select(c => c.Details)
        .ToList();
}

/// <summary>
/// Component health status information
/// </summary>
public class ComponentHealthStatus
{
    /// <summary>
    /// Whether the component is healthy
    /// </summary>
    public bool IsHealthy { get; set; }

    /// <summary>
    /// Component health status
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Additional details about the component health
    /// </summary>
    public string Details { get; set; } = string.Empty;

    /// <summary>
    /// When the component health was checked
    /// </summary>
    public DateTime LastChecked { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Component-specific metrics
    /// </summary>
    public Dictionary<string, object> Metrics { get; set; } = new();
}

/// <summary>
/// Health check result
/// </summary>
public class HealthCheckResult
{
    /// <summary>
    /// When the health check was performed
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Whether the health check passed
    /// </summary>
    public bool IsHealthy { get; set; }

    /// <summary>
    /// Health check status
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Additional details about the health check
    /// </summary>
    public string Details { get; set; } = string.Empty;

    /// <summary>
    /// Response time for the health check
    /// </summary>
    public long ResponseTimeMs { get; set; }

    /// <summary>
    /// Component-specific health check results
    /// </summary>
    public Dictionary<string, HealthCheckComponentResult> ComponentResults { get; set; } = new();
}

/// <summary>
/// Health check component result
/// </summary>
public class HealthCheckComponentResult
{
    /// <summary>
    /// Whether the component health check passed
    /// </summary>
    public bool IsHealthy { get; set; }

    /// <summary>
    /// Component health check status
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Additional details about the component health check
    /// </summary>
    public string Details { get; set; } = string.Empty;

    /// <summary>
    /// Response time for the component health check
    /// </summary>
    public long ResponseTimeMs { get; set; }

    /// <summary>
    /// Component-specific metrics
    /// </summary>
    public Dictionary<string, object> Metrics { get; set; } = new();
}
