using BIReportingCopilot.Core.Models;

namespace BIReportingCopilot.Core.Interfaces;

/// <summary>
/// Interface for AI providers that can be configured at runtime
/// </summary>
public interface IConfigurableAIProvider : IAIProvider
{
    /// <summary>
    /// Configure the provider with runtime settings
    /// </summary>
    void Configure(LLMProviderConfig config);

    /// <summary>
    /// Check if the provider is configured with the given settings
    /// </summary>
    bool IsConfiguredWith(LLMProviderConfig config);

    /// <summary>
    /// Get the current configuration
    /// </summary>
    LLMProviderConfig? GetCurrentConfiguration();
}
