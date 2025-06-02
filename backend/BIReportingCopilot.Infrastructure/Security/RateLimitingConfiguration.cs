using System.ComponentModel.DataAnnotations;

namespace BIReportingCopilot.Infrastructure.Security;

/// <summary>
/// Rate limiting configuration
/// </summary>
public class RateLimitingConfiguration
{
    /// <summary>
    /// Whether rate limiting is enabled
    /// </summary>
    public bool EnableRateLimiting { get; set; } = true;

    /// <summary>
    /// Key prefix for rate limiting cache keys
    /// </summary>
    public string KeyPrefix { get; set; } = "bi-reporting";

    /// <summary>
    /// Default window size in seconds
    /// </summary>
    [Range(1, 86400, ErrorMessage = "Window size must be between 1 and 86400 seconds")]
    public int DefaultWindowSizeSeconds { get; set; } = 3600;

    /// <summary>
    /// Whether to fail open (allow requests) when rate limiting service fails
    /// </summary>
    public bool FailOpen { get; set; } = true;

    /// <summary>
    /// Rate limiting policies
    /// </summary>
    public List<RateLimitPolicy> Policies { get; set; } = new();

    /// <summary>
    /// Requests per window for general API
    /// </summary>
    [Range(1, 10000, ErrorMessage = "Requests per window must be between 1 and 10000")]
    public int RequestsPerWindow { get; set; } = 100;

    /// <summary>
    /// Window size in minutes
    /// </summary>
    [Range(1, 1440, ErrorMessage = "Window size must be between 1 and 1440 minutes")]
    public int WindowSizeMinutes { get; set; } = 1;

    /// <summary>
    /// Whether to enable per-user limits
    /// </summary>
    public bool EnablePerUserLimits { get; set; } = true;

    /// <summary>
    /// Whether to enable per-endpoint limits
    /// </summary>
    public bool EnablePerEndpointLimits { get; set; } = true;

    /// <summary>
    /// Endpoint-specific limits
    /// </summary>
    public Dictionary<string, EndpointLimit> EndpointLimits { get; set; } = new();
}



/// <summary>
/// Endpoint-specific rate limit
/// </summary>
public class EndpointLimit
{
    /// <summary>
    /// Requests per window for this endpoint
    /// </summary>
    [Range(1, 10000, ErrorMessage = "Requests per window must be between 1 and 10000")]
    public int RequestsPerWindow { get; set; }

    /// <summary>
    /// Window size in minutes for this endpoint
    /// </summary>
    [Range(1, 1440, ErrorMessage = "Window size must be between 1 and 1440 minutes")]
    public int WindowSizeMinutes { get; set; }
}
