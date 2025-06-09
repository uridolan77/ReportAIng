using Azure.AI.OpenAI;
using BIReportingCopilot.Core.Interfaces;
using BIReportingCopilot.Core.Models;
using BIReportingCopilot.Core.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Runtime.CompilerServices;

namespace BIReportingCopilot.Infrastructure.AI.Providers;

/// <summary>
/// OpenAI provider implementation for AI operations
/// </summary>
public class OpenAIProvider : IAIProvider
{
    private readonly OpenAIClient _client;
    private readonly OpenAIConfiguration _config;
    private readonly ILogger<OpenAIProvider> _logger;

    public string ProviderName => "OpenAI";
    public bool IsConfigured => _config.IsConfigured;

    public OpenAIProvider(
        OpenAIClient client,
        IOptions<OpenAIConfiguration> config,
        ILogger<OpenAIProvider> logger)
    {
        _client = client;
        _config = config.Value;
        _logger = logger;
    }

    public async Task<string> GenerateCompletionAsync(string prompt, AIOptions options)
    {
        return await GenerateCompletionAsync(prompt, options, CancellationToken.None);
    }

    public async Task<string> GenerateCompletionAsync(string prompt, AIOptions options, CancellationToken cancellationToken)
    {
        if (!IsConfigured)
        {
            throw new InvalidOperationException("OpenAI provider is not properly configured");
        }

        try
        {
            var chatCompletionsOptions = new ChatCompletionsOptions()
            {
                DeploymentName = _config.Model,
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

            // Log the OpenAI API request details
            _logger.LogInformation("OpenAI API Request - Model: {Model}, Temperature: {Temperature}, MaxTokens: {MaxTokens}",
                _config.Model, options.Temperature, options.MaxTokens);
            _logger.LogInformation("OpenAI API Request - System Message: {SystemMessage}",
                options.SystemMessage ?? "You are a helpful AI assistant.");
            _logger.LogInformation("OpenAI API Request - User Prompt: {UserPrompt}",
                prompt.Length > 1500 ? prompt.Substring(0, 1500) + "..." : prompt);

            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(TimeSpan.FromSeconds(options.TimeoutSeconds));

            var response = await _client.GetChatCompletionsAsync(chatCompletionsOptions, cts.Token);

            if (response?.Value?.Choices?.Count > 0)
            {
                var content = response.Value.Choices[0].Message.Content;

                // Log the OpenAI API response
                _logger.LogInformation("OpenAI API Response - Content: {Content}",
                    content.Length > 1500 ? content.Substring(0, 1500) + "..." : content);
                _logger.LogInformation("OpenAI API Response - Usage: PromptTokens={PromptTokens}, CompletionTokens={CompletionTokens}, TotalTokens={TotalTokens}",
                    response.Value.Usage?.PromptTokens ?? 0,
                    response.Value.Usage?.CompletionTokens ?? 0,
                    response.Value.Usage?.TotalTokens ?? 0);

                return content;
            }

            _logger.LogWarning("OpenAI API returned no choices in response");
            throw new InvalidOperationException("No response received from OpenAI");
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("OpenAI request timed out after {TimeoutSeconds} seconds", options.TimeoutSeconds);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating completion with OpenAI: {Message}", ex.Message);
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
                Content = "OpenAI provider is not properly configured",
                IsComplete = true
            };
            yield break;
        }

        var chatCompletionsOptions = new ChatCompletionsOptions()
        {
            DeploymentName = _config.Model,
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
            _logger.LogError(ex, "Failed to initialize OpenAI streaming: {Message}", ex.Message);
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
        var streamingResponse = await _client.GetChatCompletionsStreamingAsync(options, cancellationToken);
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
}
