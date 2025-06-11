using BIReportingCopilot.Core.Interfaces;
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

    public async IAsyncEnumerable<StreamingResponse> GenerateStreamingCompletionAsync(
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
