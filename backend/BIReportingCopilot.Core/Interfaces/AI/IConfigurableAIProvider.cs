namespace BIReportingCopilot.Core.Interfaces.AI;

/// <summary>
/// Interface for configurable AI providers that can be dynamically configured
/// </summary>
public interface IConfigurableAIProvider : IAIProvider
{
    /// <summary>
    /// Configure the provider with new settings
    /// </summary>
    /// <param name="configuration">Provider configuration</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task ConfigureAsync(Dictionary<string, object> configuration, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get current provider configuration
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    Task<Dictionary<string, object>> GetConfigurationAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Validate configuration settings
    /// </summary>
    /// <param name="configuration">Configuration to validate</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task<bool> ValidateConfigurationAsync(Dictionary<string, object> configuration, CancellationToken cancellationToken = default);

    /// <summary>
    /// Test connection with current configuration
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    Task<bool> TestConnectionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get supported configuration keys
    /// </summary>
    IEnumerable<string> GetSupportedConfigurationKeys();

    /// <summary>
    /// Get default configuration values
    /// </summary>
    Dictionary<string, object> GetDefaultConfiguration();
}
