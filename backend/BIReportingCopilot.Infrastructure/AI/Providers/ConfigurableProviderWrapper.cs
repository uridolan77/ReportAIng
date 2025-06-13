using BIReportingCopilot.Core.Interfaces;
using BIReportingCopilot.Core.Interfaces.AI;
using BIReportingCopilot.Core.Models;
using Microsoft.Extensions.Logging;
using System.Runtime.CompilerServices;

namespace BIReportingCopilot.Infrastructure.AI.Providers;

/// <summary>
/// Wrapper that makes any AI provider configurable at runtime
/// </summary>
public class ConfigurableProviderWrapper : IConfigurableAIProvider
{
    private readonly IAIProvider _baseProvider;
    private readonly ILogger<ConfigurableProviderWrapper> _logger;
    private LLMProviderConfig? _currentConfig;

    public string ProviderName => _baseProvider.ProviderName;
    public string ProviderId => _baseProvider.ProviderId;
    public bool IsConfigured => _currentConfig != null && !string.IsNullOrEmpty(_currentConfig.ApiKey);

    public ConfigurableProviderWrapper(IAIProvider baseProvider, ILogger<ConfigurableProviderWrapper> logger)
    {
        _baseProvider = baseProvider;
        _logger = logger;
    }

    public void Configure(LLMProviderConfig config)
    {
        _currentConfig = config;
        
        // Configure the base provider based on its type
        if (_baseProvider is OpenAIProvider openAIProvider)
        {
            ConfigureOpenAIProvider(openAIProvider, config);
        }
        else if (_baseProvider is AzureOpenAIProvider azureProvider)
        {
            ConfigureAzureOpenAIProvider(azureProvider, config);
        }
        
        _logger.LogInformation("Configured provider {ProviderName} with managed settings", config.Name);
    }

    public bool IsConfiguredWith(LLMProviderConfig config)
    {
        return _currentConfig?.ProviderId == config.ProviderId &&
               _currentConfig?.ApiKey == config.ApiKey &&
               _currentConfig?.Endpoint == config.Endpoint;
    }

    public LLMProviderConfig? GetCurrentConfiguration()
    {
        return _currentConfig;
    }

    public Task<string> GenerateCompletionAsync(string prompt, AIOptions options)
    {
        if (!IsConfigured)
        {
            throw new InvalidOperationException("Provider is not configured. Call Configure() first.");
        }
        
        return _baseProvider.GenerateCompletionAsync(prompt, options);
    }

    public Task<string> GenerateCompletionAsync(string prompt, AIOptions options, CancellationToken cancellationToken)
    {
        if (!IsConfigured)
        {
            throw new InvalidOperationException("Provider is not configured. Call Configure() first.");
        }
        
        return _baseProvider.GenerateCompletionAsync(prompt, options, cancellationToken);
    }

    public async IAsyncEnumerable<StreamingResponse> GenerateCompletionStreamAsync(
        string prompt,
        AIOptions options,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        if (!IsConfigured)
        {
            throw new InvalidOperationException("Provider is not configured. Call Configure() first.");
        }

        // Check if base provider supports streaming
        if (_baseProvider is IStreamingAIProvider streamingProvider)
        {
            await foreach (var response in streamingProvider.GenerateStreamingCompletionAsync(prompt, options, cancellationToken))
            {
                yield return response;
            }
        }
        else
        {
            // Fallback to non-streaming
            var result = await _baseProvider.GenerateCompletionAsync(prompt, options, cancellationToken);
            yield return new StreamingResponse { Content = result, IsComplete = true };
        }
    }

    public async IAsyncEnumerable<StreamingResponse> GenerateStreamingCompletionAsync(
        string prompt,
        AIOptions options,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await foreach (var response in GenerateCompletionStreamAsync(prompt, options, cancellationToken))
        {
            yield return response;
        }
    }

    private void ConfigureOpenAIProvider(OpenAIProvider provider, LLMProviderConfig config)
    {
        try
        {
            // Use reflection to set private fields or properties
            var apiKeyField = typeof(OpenAIProvider).GetField("_apiKey", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            if (apiKeyField != null)
            {
                apiKeyField.SetValue(provider, config.ApiKey);
            }

            // Set endpoint if provided
            if (!string.IsNullOrEmpty(config.Endpoint))
            {
                var endpointField = typeof(OpenAIProvider).GetField("_endpoint", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                
                if (endpointField != null)
                {
                    endpointField.SetValue(provider, config.Endpoint);
                }
            }

            // Set organization if provided
            if (!string.IsNullOrEmpty(config.Organization))
            {
                var orgField = typeof(OpenAIProvider).GetField("_organization", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                
                if (orgField != null)
                {
                    orgField.SetValue(provider, config.Organization);
                }
            }

            _logger.LogDebug("Configured OpenAI provider with API key and endpoint");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to configure OpenAI provider with reflection");
            throw new InvalidOperationException("Failed to configure OpenAI provider", ex);
        }
    }

    private void ConfigureAzureOpenAIProvider(AzureOpenAIProvider provider, LLMProviderConfig config)
    {
        try
        {
            // Use reflection to set private fields or properties
            var apiKeyField = typeof(AzureOpenAIProvider).GetField("_apiKey", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            if (apiKeyField != null)
            {
                apiKeyField.SetValue(provider, config.ApiKey);
            }

            // Set endpoint (required for Azure)
            if (!string.IsNullOrEmpty(config.Endpoint))
            {
                var endpointField = typeof(AzureOpenAIProvider).GetField("_endpoint", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                
                if (endpointField != null)
                {
                    endpointField.SetValue(provider, config.Endpoint);
                }
            }

            // Set additional Azure-specific settings from config.Settings
            if (config.Settings.ContainsKey("apiVersion"))
            {
                var apiVersionField = typeof(AzureOpenAIProvider).GetField("_apiVersion", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                
                if (apiVersionField != null)
                {
                    apiVersionField.SetValue(provider, config.Settings["apiVersion"].ToString());
                }
            }

            _logger.LogDebug("Configured Azure OpenAI provider with API key and endpoint");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to configure Azure OpenAI provider with reflection");
            throw new InvalidOperationException("Failed to configure Azure OpenAI provider", ex);
        }
    }

    // =============================================================================
    // MISSING INTERFACE METHOD IMPLEMENTATIONS
    // =============================================================================

    /// <summary>
    /// Configure async (IConfigurableAIProvider interface)
    /// </summary>
    public async Task ConfigureAsync(Dictionary<string, object> configuration, CancellationToken cancellationToken = default)
    {
        try
        {
            var config = new LLMProviderConfig
            {
                ProviderId = ProviderId,
                Name = ProviderName,
                ApiKey = configuration.ContainsKey("apiKey") ? configuration["apiKey"].ToString()! : string.Empty,
                Endpoint = configuration.ContainsKey("endpoint") ? configuration["endpoint"].ToString() : null,
                Organization = configuration.ContainsKey("organization") ? configuration["organization"].ToString() : null,
                Settings = configuration.Where(kvp => !new[] { "apiKey", "endpoint", "organization" }.Contains(kvp.Key))
                                      .ToDictionary(kvp => kvp.Key, kvp => kvp.Value)
            };

            Configure(config);
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error configuring provider async");
            throw;
        }
    }

    /// <summary>
    /// Get configuration async (IConfigurableAIProvider interface)
    /// </summary>
    public async Task<Dictionary<string, object>> GetConfigurationAsync(CancellationToken cancellationToken = default)
    {
        var config = new Dictionary<string, object>();

        if (_currentConfig != null)
        {
            config["providerId"] = _currentConfig.ProviderId;
            config["name"] = _currentConfig.Name;
            config["apiKey"] = "***"; // Masked for security
            if (!string.IsNullOrEmpty(_currentConfig.Endpoint))
                config["endpoint"] = _currentConfig.Endpoint;
            if (!string.IsNullOrEmpty(_currentConfig.Organization))
                config["organization"] = _currentConfig.Organization;

            foreach (var setting in _currentConfig.Settings)
            {
                config[setting.Key] = setting.Value;
            }
        }

        return await Task.FromResult(config);
    }

    /// <summary>
    /// Validate configuration async (IConfigurableAIProvider interface)
    /// </summary>
    public async Task<bool> ValidateConfigurationAsync(Dictionary<string, object> configuration, CancellationToken cancellationToken = default)
    {
        try
        {
            // Basic validation
            if (!configuration.ContainsKey("apiKey") || string.IsNullOrEmpty(configuration["apiKey"].ToString()))
                return false;

            // Provider-specific validation
            if (ProviderId == "azure-openai")
            {
                if (!configuration.ContainsKey("endpoint") || string.IsNullOrEmpty(configuration["endpoint"].ToString()))
                    return false;
            }

            return await Task.FromResult(true);
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Test connection async (IConfigurableAIProvider interface)
    /// </summary>
    public async Task<bool> TestConnectionAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            if (!IsConfigured)
                return false;

            var testRequest = new AIRequest
            {
                Prompt = "Test",
                MaxTokens = 10,
                Temperature = 0.1f
            };

            var response = await GenerateResponseAsync(testRequest, cancellationToken);
            return response.Success;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Get supported configuration keys (IConfigurableAIProvider interface)
    /// </summary>
    public IEnumerable<string> GetSupportedConfigurationKeys()
    {
        var keys = new List<string> { "apiKey" };

        if (ProviderId == "azure-openai")
        {
            keys.AddRange(new[] { "endpoint", "apiVersion", "deploymentName" });
        }
        else if (ProviderId == "openai")
        {
            keys.AddRange(new[] { "endpoint", "organization" });
        }

        return keys;
    }

    /// <summary>
    /// Get default configuration (IConfigurableAIProvider interface)
    /// </summary>
    public Dictionary<string, object> GetDefaultConfiguration()
    {
        var config = new Dictionary<string, object>
        {
            ["apiKey"] = string.Empty
        };

        if (ProviderId == "azure-openai")
        {
            config["endpoint"] = string.Empty;
            config["apiVersion"] = "2023-12-01-preview";
            config["deploymentName"] = "gpt-4";
        }
        else if (ProviderId == "openai")
        {
            config["endpoint"] = "https://api.openai.com/v1";
            config["organization"] = string.Empty;
        }

        return config;
    }

    /// <summary>
    /// Generate response async (IAIProvider interface)
    /// </summary>
    public async Task<AIResponse> GenerateResponseAsync(AIRequest request, CancellationToken cancellationToken = default)
    {
        if (!IsConfigured)
        {
            return new AIResponse
            {
                Content = string.Empty,
                Success = false,
                Error = "Provider is not configured",
                Provider = ProviderId
            };
        }

        return await _baseProvider.GenerateResponseAsync(request, cancellationToken);
    }

    /// <summary>
    /// Check if provider is available
    /// </summary>
    public async Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default)
    {
        if (!IsConfigured)
            return false;

        return await _baseProvider.IsAvailableAsync(cancellationToken);
    }

    /// <summary>
    /// Get provider metrics
    /// </summary>
    public async Task<AIProviderMetrics> GetMetricsAsync(CancellationToken cancellationToken = default)
    {
        var baseMetrics = await _baseProvider.GetMetricsAsync(cancellationToken);

        // Add configuration status to metrics
        baseMetrics.Configuration["configured"] = IsConfigured;
        baseMetrics.Configuration["config_provider"] = "ConfigurableWrapper";

        return baseMetrics;
    }
}

/// <summary>
/// Interface for providers that support streaming
/// </summary>
public interface IStreamingAIProvider
{
    IAsyncEnumerable<StreamingResponse> GenerateStreamingCompletionAsync(
        string prompt, 
        AIOptions options, 
        CancellationToken cancellationToken = default);
}
