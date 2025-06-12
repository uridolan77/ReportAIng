using BIReportingCopilot.Core.Interfaces;
using BIReportingCopilot.Core.Configuration;
using BIReportingCopilot.Core.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;
using System.Runtime.CompilerServices;

namespace BIReportingCopilot.Infrastructure.AI.Providers;

/// <summary>
/// Factory for creating AI providers based on LLM Management system configuration
/// </summary>
public class AIProviderFactory : IAIProviderFactory
{
    private readonly IServiceProvider _serviceProvider;
    private readonly AIServiceConfiguration _config;
    private readonly ILogger<AIProviderFactory> _logger;
    private readonly Dictionary<string, Func<IAIProvider>> _providerFactories;

    public AIProviderFactory(
        IServiceProvider serviceProvider,
        IOptions<AIServiceConfiguration> config,
        ILogger<AIProviderFactory> logger)
    {
        _serviceProvider = serviceProvider;
        _config = config.Value;
        _logger = logger;

        _providerFactories = new Dictionary<string, Func<IAIProvider>>(StringComparer.OrdinalIgnoreCase)
        {
            ["OpenAI"] = () => _serviceProvider.GetRequiredService<OpenAIProvider>(),
            ["AzureOpenAI"] = () => _serviceProvider.GetRequiredService<AzureOpenAIProvider>()
        };
    }

    public IAIProvider GetProvider()
    {
        try
        {
            // Try to get LLM Management service (lazy loading to avoid circular dependency)
            var llmManagementService = _serviceProvider.GetService<ILLMManagementService>();

            if (llmManagementService != null)
            {
                // Get the default provider from LLM Management system
                var providers = llmManagementService.GetProvidersAsync().GetAwaiter().GetResult();
                var defaultProvider = providers.FirstOrDefault(p => p.IsDefault && p.IsEnabled);

                if (defaultProvider != null)
                {
                    _logger.LogInformation("Using LLM managed provider: {ProviderName} ({ProviderId})",
                        defaultProvider.Name, defaultProvider.ProviderId);
                    return GetManagedProvider(defaultProvider);
                }

                // Fallback to first enabled provider
                var enabledProvider = providers.FirstOrDefault(p => p.IsEnabled);
                if (enabledProvider != null)
                {
                    _logger.LogInformation("Using first enabled LLM provider: {ProviderName} ({ProviderId})",
                        enabledProvider.Name, enabledProvider.ProviderId);
                    return GetManagedProvider(enabledProvider);
                }
            }

            _logger.LogWarning("No enabled providers found in LLM Management system. Falling back to configuration-based provider.");
            return GetLegacyProvider();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting provider from LLM Management system. Falling back to configuration-based provider.");
            return GetLegacyProvider();
        }
    }

    /// <summary>
    /// Get provider using legacy configuration (fallback)
    /// </summary>
    private IAIProvider GetLegacyProvider()
    {
        // Prefer Azure OpenAI if configured, fallback to OpenAI
        if (_config.PreferAzureOpenAI && _config.AzureOpenAI.IsConfigured)
        {
            _logger.LogInformation("Using legacy Azure OpenAI provider");
            return GetProvider("AzureOpenAI");
        }

        if (_config.OpenAI.IsConfigured)
        {
            _logger.LogInformation("Using legacy OpenAI provider");
            return GetProvider("OpenAI");
        }

        _logger.LogWarning("No AI provider is properly configured. Using fallback provider.");
        return new FallbackAIProvider(_logger);
    }

    public IAIProvider GetProvider(string providerName)
    {
        if (string.IsNullOrEmpty(providerName))
        {
            throw new ArgumentException("Provider name cannot be null or empty", nameof(providerName));
        }

        try
        {
            // Try to get LLM Management service (lazy loading to avoid circular dependency)
            var llmManagementService = _serviceProvider.GetService<ILLMManagementService>();

            if (llmManagementService != null)
            {
                // First try to get from LLM Management system
                var providers = llmManagementService.GetProvidersAsync().GetAwaiter().GetResult();
                var managedProvider = providers.FirstOrDefault(p =>
                    p.ProviderId.Equals(providerName, StringComparison.OrdinalIgnoreCase) ||
                    p.Name.Equals(providerName, StringComparison.OrdinalIgnoreCase));

                if (managedProvider != null && managedProvider.IsEnabled)
                {
                    _logger.LogInformation("Using LLM managed provider: {ProviderName} ({ProviderId})",
                        managedProvider.Name, managedProvider.ProviderId);
                    return GetManagedProvider(managedProvider);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting managed provider {ProviderName}. Falling back to legacy provider.", providerName);
        }

        // Fallback to legacy provider factory
        if (_providerFactories.TryGetValue(providerName, out var factory))
        {
            var provider = factory();
            if (provider.IsConfigured)
            {
                return provider;
            }

            _logger.LogWarning("Provider {ProviderName} is not properly configured", providerName);
            throw new InvalidOperationException($"Provider {providerName} is not properly configured");
        }

        throw new ArgumentException($"Unknown provider: {providerName}", nameof(providerName));
    }

    /// <summary>
    /// Create a provider instance from LLM Management configuration
    /// </summary>
    private IAIProvider GetManagedProvider(LLMProviderConfig config)
    {
        if (!_providerFactories.TryGetValue(config.Type, out var factory))
        {
            throw new ArgumentException($"Unknown provider type: {config.Type}");
        }

        var baseProvider = factory();

        // Wrap the provider to make it configurable
        var configurableProvider = new ConfigurableProviderWrapper(
            baseProvider,
            _serviceProvider.GetRequiredService<ILogger<ConfigurableProviderWrapper>>());

        // Configure it with the managed settings
        configurableProvider.Configure(config);

        return configurableProvider;
    }

    public IEnumerable<string> GetAvailableProviders()
    {
        return _providerFactories.Keys.Where(name =>
        {
            try
            {
                var provider = _providerFactories[name]();
                return provider.IsConfigured;
            }
            catch
            {
                return false;
            }
        });
    }
}

/// <summary>
/// Fallback AI provider that returns mock responses when no real provider is configured
/// </summary>
internal class FallbackAIProvider : IAIProvider
{
    private readonly ILogger _logger;

    public string ProviderName => "Fallback";
    public bool IsConfigured => true; // Always available as fallback

    public FallbackAIProvider(ILogger logger)
    {
        _logger = logger;
    }

    public Task<string> GenerateCompletionAsync(string prompt, AIOptions options)
    {
        return GenerateCompletionAsync(prompt, options, CancellationToken.None);
    }

    public Task<string> GenerateCompletionAsync(string prompt, AIOptions options, CancellationToken cancellationToken)
    {
        _logger.LogError("AI provider not configured - cannot generate completion");
        throw new InvalidOperationException("AI service not configured. Please configure OpenAI or Azure OpenAI.");
    }

    public async IAsyncEnumerable<StreamingResponse> GenerateCompletionStreamAsync(
        string prompt,
        AIOptions options,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        _logger.LogWarning("Using fallback AI provider for streaming - returning mock response");

        yield return new StreamingResponse
        {
            Type = StreamingResponseType.SQLGeneration,
            Content = "-- Fallback response: AI service not configured",
            IsComplete = false,
            ChunkIndex = 0
        };

        // Simulate streaming delay
        await Task.Delay(100, cancellationToken);

        yield return new StreamingResponse
        {
            Type = StreamingResponseType.SQLGeneration,
            Content = "\nSELECT * FROM [Table] WHERE 1=1",
            IsComplete = false,
            ChunkIndex = 1
        };

        await Task.Delay(100, cancellationToken);

        yield return new StreamingResponse
        {
            Type = StreamingResponseType.SQLGeneration,
            Content = string.Empty,
            IsComplete = true,
            ChunkIndex = 2
        };
    }
}
