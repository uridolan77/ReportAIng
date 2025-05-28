using System.ComponentModel.DataAnnotations;

namespace BIReportingCopilot.Core.Configuration;

/// <summary>
/// Configuration settings for Redis cache
/// </summary>
public class RedisConfiguration
{
    /// <summary>
    /// Whether Redis is enabled
    /// </summary>
    public bool Enabled { get; set; } = false;

    /// <summary>
    /// Redis connection string
    /// </summary>
    [Required]
    public string ConnectionString { get; set; } = "localhost:6379";

    /// <summary>
    /// Redis database number
    /// </summary>
    public int Database { get; set; } = 0;

    /// <summary>
    /// Connection timeout in milliseconds
    /// </summary>
    public int ConnectTimeout { get; set; } = 5000;

    /// <summary>
    /// Sync timeout in milliseconds
    /// </summary>
    public int SyncTimeout { get; set; } = 5000;

    /// <summary>
    /// Whether to abort on connect failure
    /// </summary>
    public bool AbortOnConnectFail { get; set; } = false;

    /// <summary>
    /// Number of connection retries
    /// </summary>
    public int ConnectRetry { get; set; } = 3;

    /// <summary>
    /// Get the full connection string with options
    /// </summary>
    public string GetConnectionStringWithOptions()
    {
        if (!Enabled)
            return string.Empty;

        var options = new List<string>
        {
            ConnectionString,
            $"connectTimeout={ConnectTimeout}",
            $"syncTimeout={SyncTimeout}",
            $"abortConnect={AbortOnConnectFail.ToString().ToLower()}",
            $"connectRetry={ConnectRetry}"
        };

        if (Database > 0)
        {
            options.Add($"defaultDatabase={Database}");
        }

        return string.Join(",", options);
    }
}
