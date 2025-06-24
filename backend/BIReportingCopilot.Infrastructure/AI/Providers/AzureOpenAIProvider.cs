using Azure.AI.OpenAI;
using BIReportingCopilot.Core.Interfaces;
using BIReportingCopilot.Core.Interfaces.AI;
using BIReportingCopilot.Core.Models;
using BIReportingCopilot.Core.Configuration;
using BIReportingCopilot.Infrastructure.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Runtime.CompilerServices;

namespace BIReportingCopilot.Infrastructure.AI.Providers;

/// <summary>
/// Azure OpenAI provider implementation for AI operations
/// </summary>
public class AzureOpenAIProvider : IAIProvider
{
    private readonly Lazy<OpenAIClient> _lazyClient;
    private readonly AzureOpenAIConfiguration _config;
    private readonly ILogger<AzureOpenAIProvider> _logger;

    public string ProviderName => "AzureOpenAI";
    public string ProviderId => "azure-openai";
    public bool IsConfigured => _config.IsConfigured;

    public AzureOpenAIProvider(
        Lazy<OpenAIClient> lazyClient,
        IOptions<AzureOpenAIConfiguration> config,
        ILogger<AzureOpenAIProvider> logger)
    {
        _lazyClient = lazyClient;
        _config = config.Value;
        _logger = logger;

        _logger.LogInformation("🔧 AzureOpenAIProvider initialized with lazy client (prevents hanging)");
    }

    /// <summary>
    /// Get the OpenAI client when needed (lazy initialization)
    /// </summary>
    private OpenAIClient GetClient()
    {
        try
        {
            return _lazyClient.Value;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting OpenAI client in AzureOpenAIProvider");
            throw new InvalidOperationException("OpenAI client is not available", ex);
        }
    }

    public async Task<string> GenerateCompletionAsync(string prompt, AIOptions options)
    {
        return await GenerateCompletionAsync(prompt, options, CancellationToken.None);
    }

    public async Task<string> GenerateCompletionAsync(string prompt, AIOptions options, CancellationToken cancellationToken)
    {
        if (!IsConfigured)
        {
            throw new InvalidOperationException("Azure OpenAI provider is not properly configured");
        }

        try
        {
            var chatCompletionsOptions = new ChatCompletionsOptions()
            {
                DeploymentName = _config.DeploymentName,
                Messages =
                {
                    new ChatRequestSystemMessage(options.SystemMessage ?? "You are a helpful AI assistant."),
                    new ChatRequestUserMessage(prompt)
                },
                Temperature = options.Temperature,
                MaxTokens = options.MaxTokens,
                FrequencyPenalty = options.FrequencyPenalty,
                PresencePenalty = options.PresencePenalty
            };

            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(TimeSpan.FromSeconds(options.TimeoutSeconds));

            var client = GetClient();
            var response = await client.GetChatCompletionsAsync(chatCompletionsOptions, cts.Token);

            if (response?.Value?.Choices?.Count > 0)
            {
                return response.Value.Choices[0].Message.Content;
            }

            throw new InvalidOperationException("No response received from Azure OpenAI");
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Azure OpenAI request timed out after {TimeoutSeconds} seconds", options.TimeoutSeconds);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating completion with Azure OpenAI: {Message}", ex.Message);
            throw;
        }
    }

    public async IAsyncEnumerable<StreamingResponse> GenerateCompletionStreamAsync(
        string prompt,
        AIOptions options,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        if (!IsConfigured)
        {
            yield return new StreamingResponse
            {
                Type = StreamingResponseType.Error,
                Content = "Azure OpenAI provider is not properly configured",
                IsComplete = true
            };
            yield break;
        }

        var chatCompletionsOptions = new ChatCompletionsOptions()
        {
            DeploymentName = _config.DeploymentName,
            Messages =
            {
                new ChatRequestSystemMessage(options.SystemMessage ?? "You are a helpful AI assistant."),
                new ChatRequestUserMessage(prompt)
            },
            Temperature = options.Temperature,
            MaxTokens = options.MaxTokens,
            FrequencyPenalty = options.FrequencyPenalty,
            PresencePenalty = options.PresencePenalty
        };

        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        cts.CancelAfter(TimeSpan.FromSeconds(options.TimeoutSeconds));

        IAsyncEnumerable<StreamingResponse> stream;

        try
        {
            stream = GetStreamingResponsesAsync(chatCompletionsOptions, cts.Token);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize Azure OpenAI streaming: {Message}", ex.Message);
            stream = CreateErrorStreamAsync($"Error: {ex.Message}");
        }

        await foreach (var response in stream.WithCancellation(cts.Token))
        {
            yield return response;
        }
    }

    private async IAsyncEnumerable<StreamingResponse> GetStreamingResponsesAsync(
        ChatCompletionsOptions options,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var client = GetClient();
        var streamingResponse = await client.GetChatCompletionsStreamingAsync(options, cancellationToken);
        int chunkIndex = 0;

        await foreach (var update in streamingResponse.WithCancellation(cancellationToken))
        {
            if (update.ContentUpdate != null && !string.IsNullOrEmpty(update.ContentUpdate))
            {
                yield return new StreamingResponse
                {
                    Type = StreamingResponseType.SQLGeneration,
                    Content = update.ContentUpdate,
                    IsComplete = false,
                    ChunkIndex = chunkIndex++
                };
            }
        }

        yield return new StreamingResponse
        {
            Type = StreamingResponseType.SQLGeneration,
            Content = string.Empty,
            IsComplete = true,
            ChunkIndex = chunkIndex
        };
    }

    private IAsyncEnumerable<StreamingResponse> CreateErrorStreamAsync(string errorMessage)
    {
        return CreateErrorStreamInternal(errorMessage);
    }

    private async IAsyncEnumerable<StreamingResponse> CreateErrorStreamInternal(string errorMessage)
    {
        yield return new StreamingResponse
        {
            Type = StreamingResponseType.Error,
            Content = errorMessage,
            IsComplete = true
        };
    }

    // =============================================================================
    // MISSING INTERFACE METHOD IMPLEMENTATIONS
    // =============================================================================

    /// <summary>
    /// Generate response async (IAIProvider interface)
    /// </summary>
    public async Task<AIResponse> GenerateResponseAsync(AIRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var options = new AIOptions
            {
                Temperature = (float)request.Temperature,
                MaxTokens = request.MaxTokens,
                SystemMessage = request.SystemMessage,
                TimeoutSeconds = 30 // Default timeout
            };

            var content = await GenerateCompletionAsync(request.Prompt, options, cancellationToken);

            return new AIResponse
            {
                Content = content,
                Success = true,
                TokensUsed = content.Length / 4, // Rough estimate
                Model = _config.DeploymentName,
                Provider = ProviderId
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating AI response");
            return new AIResponse
            {
                Content = string.Empty,
                Success = false,
                Error = ex.Message,
                Model = _config.DeploymentName,
                Provider = ProviderId
            };
        }
    }

    /// <summary>
    /// Check if provider is available
    /// </summary>
    public async Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            if (!IsConfigured)
                return false;

            // Simple test request to check availability
            var testRequest = new AIRequest
            {
                Prompt = "Test",
                MaxTokens = 10,
                Temperature = 0.1
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
    /// Get provider metrics
    /// </summary>
    public async Task<AIProviderMetrics> GetMetricsAsync(CancellationToken cancellationToken = default)
    {
        return await Task.FromResult(new AIProviderMetrics
        {
            ProviderId = ProviderId,
            ProviderName = ProviderName,
            IsAvailable = IsConfigured,
            ResponseTimeMs = 0, // Would track actual response times
            RequestCount = 0, // Would track actual request count
            ErrorCount = 0, // Would track actual error count
            LastUsed = DateTime.UtcNow,
            Configuration = new Dictionary<string, object>
            {
                ["deployment"] = _config.DeploymentName,
                ["endpoint"] = _config.Endpoint,
                ["configured"] = IsConfigured
            }
        });
    }
}
