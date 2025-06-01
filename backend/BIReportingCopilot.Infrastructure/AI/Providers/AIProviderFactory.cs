using BIReportingCopilot.Core.Interfaces;
using BIReportingCopilot.Core.Configuration;
using BIReportingCopilot.Core.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;

namespace BIReportingCopilot.Infrastructure.AI.Providers;

/// <summary>
/// Factory for creating AI providers based on configuration
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
        // Prefer Azure OpenAI if configured, fallback to OpenAI
        if (_config.PreferAzureOpenAI && _config.AzureOpenAI.IsConfigured)
        {
            _logger.LogInformation("Using Azure OpenAI provider");
            return GetProvider("AzureOpenAI");
        }

        if (_config.OpenAI.IsConfigured)
        {
            _logger.LogInformation("Using OpenAI provider");
            return GetProvider("OpenAI");
        }

        _logger.LogWarning("No AI provider is properly configured. Using fallback provider.");
        // Return a fallback provider that returns mock responses
        return new FallbackAIProvider(_logger);
    }

    public IAIProvider GetProvider(string providerName)
    {
        if (string.IsNullOrEmpty(providerName))
        {
            throw new ArgumentException("Provider name cannot be null or empty", nameof(providerName));
        }

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
        CancellationToken cancellationToken = default)
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
