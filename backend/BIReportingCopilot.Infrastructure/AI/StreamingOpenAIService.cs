using Azure.AI.OpenAI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using BIReportingCopilot.Core.Interfaces;
using BIReportingCopilot.Core.Models;
using BIReportingCopilot.Core.Configuration;
using BIReportingCopilot.Core.Constants;
using CoreModels = BIReportingCopilot.Core.Models;
using System.Text.Json;
using System.Text;
using System.Runtime.CompilerServices;
using Azure;

namespace BIReportingCopilot.Infrastructure.AI;

/// <summary>
/// Advanced streaming OpenAI service with sophisticated prompt engineering and context management
/// </summary>
public class StreamingOpenAIService : IStreamingOpenAIService
{
    private readonly OpenAIClient _client;
    private readonly IConfiguration _configuration;
    private readonly ILogger<StreamingOpenAIService> _logger;
    private readonly ICacheService _cacheService;
    private readonly AIServiceConfiguration _aiConfig;
    private readonly bool _isConfigured;
    private readonly PromptTemplateManager _promptManager;
    private readonly IContextManager _contextManager;

    public StreamingOpenAIService(
        OpenAIClient client,
        IConfiguration configuration,
        ILogger<StreamingOpenAIService> logger,
        ICacheService cacheService,
        IContextManager contextManager)
    {
        _client = client;
        _configuration = configuration;
        _logger = logger;
        _cacheService = cacheService;

        // Load AI configuration
        _aiConfig = new AIServiceConfiguration();
        configuration.GetSection("OpenAI").Bind(_aiConfig.OpenAI);
        configuration.GetSection("AzureOpenAI").Bind(_aiConfig.AzureOpenAI);

        _isConfigured = _aiConfig.HasValidConfiguration;
        _promptManager = new PromptTemplateManager();
        _contextManager = contextManager;

        if (!_isConfigured)
        {
            _logger.LogWarning("OpenAI service is not properly configured. Fallback responses will be used.");
        }
        else
        {
            var configType = _aiConfig.PreferAzureOpenAI ? "Azure OpenAI" : "OpenAI";
            _logger.LogInformation("Streaming OpenAI service configured with {ConfigType}", configType);
        }
    }

    public async IAsyncEnumerable<StreamingResponse> GenerateSQLStreamAsync(
        string prompt,
        CoreModels.SchemaMetadata? schema = null,
        QueryContext? context = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        if (!_isConfigured)
        {
            yield return new StreamingResponse
            {
                Type = StreamingResponseType.Error,
                Content = "OpenAI service is not configured",
                IsComplete = true
            };
            yield break;
        }

        // Build sophisticated prompt with context
        var enhancedPrompt = await _promptManager.BuildSQLGenerationPromptAsync(prompt, schema, context);

        // Check cache first
        var cacheKey = $"{ApplicationConstants.CacheKeys.SQLGeneration}:{ComputeHash(enhancedPrompt)}";
        var cachedResult = await _cacheService.GetAsync<string>(cacheKey);

        if (!string.IsNullOrEmpty(cachedResult))
        {
            _logger.LogDebug("Returning cached SQL generation result");
            yield return new StreamingResponse
            {
                Type = StreamingResponseType.SQLGeneration,
                Content = cachedResult,
                IsComplete = true,
                Metadata = new { Source = "Cache" }
            };
            yield break;
        }

        // Stream the response
        IAsyncEnumerable<StreamingResponse>? streamingResults = null;
        StreamingResponse? errorResponse = null;

        try
        {
            streamingResults = StreamChatCompletionAsync(enhancedPrompt, StreamingResponseType.SQLGeneration, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in streaming SQL generation");
            errorResponse = new StreamingResponse
            {
                Type = StreamingResponseType.Error,
                Content = $"Error generating SQL: {ex.Message}",
                IsComplete = true
            };
        }

        if (errorResponse != null)
        {
            yield return errorResponse;
            yield break;
        }

        if (streamingResults != null)
        {
            await foreach (var chunk in streamingResults)
            {
                yield return chunk;

                // Cache the complete result
                if (chunk.IsComplete && !string.IsNullOrEmpty(chunk.Content))
                {
                    try
                    {
                        await _cacheService.SetAsync(cacheKey, chunk.Content, TimeSpan.FromHours(1));
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to cache SQL generation result");
                    }
                }
            }
        }
    }

    public async IAsyncEnumerable<StreamingResponse> GenerateInsightStreamAsync(
        string query,
        object[] data,
        AnalysisContext? context = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        if (!_isConfigured)
        {
            yield return new StreamingResponse
            {
                Type = StreamingResponseType.Error,
                Content = "OpenAI service is not configured",
                IsComplete = true
            };
            yield break;
        }

        var insightPrompt = await _promptManager.BuildInsightGenerationPromptAsync(query, data, context);

        IAsyncEnumerable<StreamingResponse>? streamingResults = null;
        StreamingResponse? errorResponse = null;

        try
        {
            streamingResults = StreamChatCompletionAsync(insightPrompt, StreamingResponseType.Insight, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in streaming insight generation");
            errorResponse = new StreamingResponse
            {
                Type = StreamingResponseType.Error,
                Content = $"Error generating insights: {ex.Message}",
                IsComplete = true
            };
        }

        if (errorResponse != null)
        {
            yield return errorResponse;
            yield break;
        }

        if (streamingResults != null)
        {
            await foreach (var chunk in streamingResults)
            {
                yield return chunk;
            }
        }
    }

    public async IAsyncEnumerable<StreamingResponse> GenerateExplanationStreamAsync(
        string sql,
        StreamingQueryComplexity complexity = StreamingQueryComplexity.Medium,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        if (!_isConfigured)
        {
            yield return new StreamingResponse
            {
                Type = StreamingResponseType.Error,
                Content = "OpenAI service is not configured",
                IsComplete = true
            };
            yield break;
        }

        var explanationPrompt = await _promptManager.BuildSQLExplanationPromptAsync(sql, complexity);

        IAsyncEnumerable<StreamingResponse>? streamingResults = null;
        StreamingResponse? errorResponse = null;

        try
        {
            streamingResults = StreamChatCompletionAsync(explanationPrompt, StreamingResponseType.Explanation, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in streaming SQL explanation");
            errorResponse = new StreamingResponse
            {
                Type = StreamingResponseType.Error,
                Content = $"Error generating explanation: {ex.Message}",
                IsComplete = true
            };
        }

        if (errorResponse != null)
        {
            yield return errorResponse;
            yield break;
        }

        if (streamingResults != null)
        {
            await foreach (var chunk in streamingResults)
            {
                yield return chunk;
            }
        }
    }

    private async IAsyncEnumerable<StreamingResponse> StreamChatCompletionAsync(
        string prompt,
        StreamingResponseType responseType,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var chatCompletionsOptions = new ChatCompletionsOptions()
        {
            DeploymentName = _aiConfig.PreferAzureOpenAI ?
                _aiConfig.AzureOpenAI.DeploymentName :
                _aiConfig.OpenAI.Model,
            Messages =
            {
                new ChatRequestSystemMessage(GetSystemPromptForType(responseType)),
                new ChatRequestUserMessage(prompt)
            },
            Temperature = GetTemperatureForType(responseType),
            MaxTokens = GetMaxTokensForType(responseType),
            FrequencyPenalty = _aiConfig.OpenAI.FrequencyPenalty,
            PresencePenalty = _aiConfig.OpenAI.PresencePenalty
        };

        var streamingResponse = await _client.GetChatCompletionsStreamingAsync(chatCompletionsOptions, cancellationToken);
        var contentBuilder = new StringBuilder();
        var chunkCount = 0;

        await foreach (var streamingChatCompletionsUpdate in streamingResponse.WithCancellation(cancellationToken))
        {
            if (streamingChatCompletionsUpdate.ContentUpdate != null)
            {
                var content = streamingChatCompletionsUpdate.ContentUpdate;
                contentBuilder.Append(content);
                chunkCount++;

                yield return new StreamingResponse
                {
                    Type = responseType,
                    Content = content,
                    IsComplete = false,
                    ChunkIndex = chunkCount,
                    Metadata = new {
                        PartialContent = contentBuilder.ToString(),
                        ChunkSize = content.Length
                    }
                };
            }

            if (streamingChatCompletionsUpdate.FinishReason != null)
            {
                var finalContent = contentBuilder.ToString();

                // Clean up the final content based on response type
                finalContent = CleanResponseContent(finalContent, responseType);

                yield return new StreamingResponse
                {
                    Type = responseType,
                    Content = finalContent,
                    IsComplete = true,
                    ChunkIndex = chunkCount + 1,
                    Metadata = new {
                        TotalChunks = chunkCount + 1,
                        FinishReason = streamingChatCompletionsUpdate.FinishReason.ToString(),
                        TotalTokens = finalContent.Length // Approximate
                    }
                };
            }
        }
    }

    private string GetSystemPromptForType(StreamingResponseType responseType)
    {
        return responseType switch
        {
            StreamingResponseType.SQLGeneration => _promptManager.GetSQLSystemPrompt(),
            StreamingResponseType.Insight => _promptManager.GetInsightSystemPrompt(),
            StreamingResponseType.Explanation => _promptManager.GetExplanationSystemPrompt(),
            _ => "You are a helpful AI assistant for business intelligence and data analysis."
        };
    }

    private float GetTemperatureForType(StreamingResponseType responseType)
    {
        return responseType switch
        {
            StreamingResponseType.SQLGeneration => 0.1f, // Very deterministic for SQL
            StreamingResponseType.Insight => 0.3f, // Slightly creative for insights
            StreamingResponseType.Explanation => 0.2f, // Balanced for explanations
            _ => 0.1f
        };
    }

    private int GetMaxTokensForType(StreamingResponseType responseType)
    {
        return responseType switch
        {
            StreamingResponseType.SQLGeneration => 1000,
            StreamingResponseType.Insight => 800,
            StreamingResponseType.Explanation => 1200,
            _ => 500
        };
    }

    private string CleanResponseContent(string content, StreamingResponseType responseType)
    {
        if (responseType == StreamingResponseType.SQLGeneration)
        {
            // Clean SQL content
            return CleanSqlResult(content);
        }

        return content.Trim();
    }

    private string CleanSqlResult(string sqlResult)
    {
        if (string.IsNullOrEmpty(sqlResult))
            return string.Empty;

        // Remove markdown code blocks
        sqlResult = sqlResult.Replace("```sql", "").Replace("```", "").Trim();

        // Remove common prefixes
        var prefixes = new[] { "SQL:", "Query:", "SELECT" };
        foreach (var prefix in prefixes)
        {
            if (sqlResult.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                sqlResult = sqlResult.Substring(prefix.Length).Trim();
                if (sqlResult.StartsWith(":"))
                    sqlResult = sqlResult.Substring(1).Trim();
                break;
            }
        }

        return sqlResult;
    }

    private string ComputeHash(string input)
    {
        using var sha256 = System.Security.Cryptography.SHA256.Create();
        var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
        return Convert.ToBase64String(hash)[..16]; // First 16 characters for cache key
    }
}


