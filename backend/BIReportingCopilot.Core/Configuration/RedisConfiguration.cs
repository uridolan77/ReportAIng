using System.ComponentModel.DataAnnotations;

namespace BIReportingCopilot.Core.Configuration;

/// <summary>
/// Redis configuration settings
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
    public string ConnectionString { get; set; } = string.Empty;

    /// <summary>
    /// Redis host
    /// </summary>
    public string Host { get; set; } = "localhost";

    /// <summary>
    /// Redis port
    /// </summary>
    [Range(1, 65535, ErrorMessage = "Port must be between 1 and 65535")]
    public int Port { get; set; } = 6379;

    /// <summary>
    /// Redis password
    /// </summary>
    public string? Password { get; set; }

    /// <summary>
    /// Redis database number
    /// </summary>
    [Range(0, 15, ErrorMessage = "Database must be between 0 and 15")]
    public int Database { get; set; } = 0;

    /// <summary>
    /// Connection timeout in milliseconds
    /// </summary>
    [Range(1000, 30000, ErrorMessage = "Connection timeout must be between 1000 and 30000 milliseconds")]
    public int ConnectionTimeoutMs { get; set; } = 5000;

    /// <summary>
    /// Command timeout in milliseconds
    /// </summary>
    [Range(1000, 30000, ErrorMessage = "Command timeout must be between 1000 and 30000 milliseconds")]
    public int CommandTimeoutMs { get; set; } = 5000;

    /// <summary>
    /// Whether to use SSL
    /// </summary>
    public bool UseSsl { get; set; } = false;

    /// <summary>
    /// SSL host
    /// </summary>
    public string? SslHost { get; set; }

    /// <summary>
    /// Whether to abort connection on connect fail
    /// </summary>
    public bool AbortOnConnectFail { get; set; } = false;

    /// <summary>
    /// Key prefix for cache keys
    /// </summary>
    public string KeyPrefix { get; set; } = "bi-copilot";

    /// <summary>
    /// Default expiration time in minutes
    /// </summary>
    [Range(1, 1440, ErrorMessage = "Default expiration must be between 1 and 1440 minutes")]
    public int DefaultExpirationMinutes { get; set; } = 60;

    /// <summary>
    /// Maximum number of connections
    /// </summary>
    [Range(1, 1000, ErrorMessage = "Max connections must be between 1 and 1000")]
    public int MaxConnections { get; set; } = 100;

    /// <summary>
    /// Whether to enable compression
    /// </summary>
    public bool EnableCompression { get; set; } = true;

    /// <summary>
    /// Whether to enable clustering
    /// </summary>
    public bool EnableClustering { get; set; } = false;

    /// <summary>
    /// Cluster endpoints
    /// </summary>
    public List<string> ClusterEndpoints { get; set; } = new();

    /// <summary>
    /// Get connection string with options
    /// </summary>
    /// <returns>Formatted connection string</returns>
    public string GetConnectionStringWithOptions()
    {
        if (!string.IsNullOrEmpty(ConnectionString))
        {
            return ConnectionString;
        }

        var connectionStringBuilder = new List<string>();

        // Add host and port
        connectionStringBuilder.Add($"{Host}:{Port}");

        // Add password if provided
        if (!string.IsNullOrEmpty(Password))
        {
            connectionStringBuilder.Add($"password={Password}");
        }

        // Add database
        if (Database != 0)
        {
            connectionStringBuilder.Add($"defaultDatabase={Database}");
        }

        // Add timeouts
        connectionStringBuilder.Add($"connectTimeout={ConnectionTimeoutMs}");
        connectionStringBuilder.Add($"syncTimeout={CommandTimeoutMs}");

        // Add SSL settings
        if (UseSsl)
        {
            connectionStringBuilder.Add("ssl=true");
            if (!string.IsNullOrEmpty(SslHost))
            {
                connectionStringBuilder.Add($"sslHost={SslHost}");
            }
        }

        // Add other options
        connectionStringBuilder.Add($"abortConnect={AbortOnConnectFail.ToString().ToLower()}");

        return string.Join(",", connectionStringBuilder);
    }
}
