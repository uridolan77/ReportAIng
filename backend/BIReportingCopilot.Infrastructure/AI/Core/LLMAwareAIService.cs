using BIReportingCopilot.Core.Interfaces;
using BIReportingCopilot.Core.Models;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Security.Claims;

namespace BIReportingCopilot.Infrastructure.AI.Core;

/// <summary>
/// Enhanced AI service that uses LLM Management system for intelligent provider and model selection
/// </summary>
public class LLMAwareAIService : ILLMAwareAIService
{
    private readonly IAIService _baseAIService;
    private readonly ILLMManagementService _llmManagementService;
    private readonly ILogger<LLMAwareAIService> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public LLMAwareAIService(
        IAIService baseAIService,
        ILLMManagementService llmManagementService,
        ILogger<LLMAwareAIService> logger,
        IHttpContextAccessor httpContextAccessor)
    {
        _baseAIService = baseAIService;
        _llmManagementService = llmManagementService;
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;
    }

    #region Use Case Specific Methods

    public async Task<string> GenerateSQLWithManagedModelAsync(string prompt, string? providerId = null, string? modelId = null, CancellationToken cancellationToken = default)
    {
        return await GenerateCompletionWithManagedModelAsync(prompt, "SQL", providerId, modelId, cancellationToken);
    }

    public async Task<string> GenerateInsightsWithManagedModelAsync(string prompt, string? providerId = null, string? modelId = null, CancellationToken cancellationToken = default)
    {
        return await GenerateCompletionWithManagedModelAsync(prompt, "Insights", providerId, modelId, cancellationToken);
    }

    public async Task<string> GenerateCompletionWithManagedModelAsync(string prompt, string useCase, string? providerId = null, string? modelId = null, CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        var requestId = Guid.NewGuid().ToString();
        
        try
        {
            // Get the best model for this use case
            var model = await GetModelForRequestAsync(useCase, providerId, modelId);
            if (model == null)
            {
                _logger.LogWarning("No suitable model found for use case {UseCase}. Falling back to base AI service.", useCase);
                // Fallback to base AI service for SQL generation
                return await _baseAIService.GenerateSQLAsync(prompt, cancellationToken);
            }

            // For now, delegate to base service since we don't have direct provider access
            // TODO: Implement provider-specific logic when provider factory circular dependency is resolved
            _logger.LogInformation("Using model {ModelId} from provider {ProviderId} for {UseCase}",
                model.ModelId, model.ProviderId, useCase);

            // Generate completion using base service
            var response = await _baseAIService.GenerateSQLAsync(prompt, cancellationToken);
            
            stopwatch.Stop();

            // Log usage
            await LogUsageAsync(new LLMUsageLog
            {
                RequestId = requestId,
                UserId = GetCurrentUserId(),
                ProviderId = model.ProviderId,
                ModelId = model.ModelId,
                RequestType = useCase,
                RequestText = prompt,
                ResponseText = response,
                InputTokens = EstimateTokens(prompt),
                OutputTokens = EstimateTokens(response),
                TotalTokens = EstimateTokens(prompt) + EstimateTokens(response),
                Cost = CalculateCost(model, EstimateTokens(prompt) + EstimateTokens(response)),
                DurationMs = stopwatch.ElapsedMilliseconds,
                Success = !string.IsNullOrEmpty(response),
                Timestamp = DateTime.UtcNow,
                Metadata = new Dictionary<string, object>
                {
                    { "useCase", useCase },
                    { "temperature", model.Temperature },
                    { "maxTokens", model.MaxTokens }
                }
            });

            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            
            _logger.LogError(ex, "Error generating completion with managed model for use case {UseCase}", useCase);
            
            // Log failed usage
            await LogUsageAsync(new LLMUsageLog
            {
                RequestId = requestId,
                UserId = GetCurrentUserId(),
                ProviderId = providerId ?? "unknown",
                ModelId = modelId ?? "unknown",
                RequestType = useCase,
                RequestText = prompt,
                ResponseText = "",
                InputTokens = EstimateTokens(prompt),
                OutputTokens = 0,
                TotalTokens = EstimateTokens(prompt),
                Cost = 0,
                DurationMs = stopwatch.ElapsedMilliseconds,
                Success = false,
                ErrorMessage = ex.Message,
                Timestamp = DateTime.UtcNow
            });

            // Fallback to base AI service
            return await _baseAIService.GenerateSQLAsync(prompt, cancellationToken);
        }
    }

    public async IAsyncEnumerable<StreamingResponse> GenerateSQLStreamWithManagedModelAsync(
        string prompt,
        SchemaMetadata? schema = null,
        QueryContext? context = null,
        string? providerId = null,
        string? modelId = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var requestId = Guid.NewGuid().ToString();
        var stopwatch = Stopwatch.StartNew();

        // Get the best model for SQL generation
        var model = await GetModelForRequestAsync("SQL", providerId, modelId);
        if (model == null)
        {
            _logger.LogWarning("No suitable model found for SQL generation. Falling back to base AI service.");
            await foreach (var response in _baseAIService.GenerateSQLStreamAsync(prompt, schema, context, cancellationToken))
            {
                yield return response;
            }
            yield break;
        }

        IAsyncEnumerable<StreamingResponse> streamingSource;

        try
        {
            // For now, use the base service streaming but log the usage
            // TODO: Implement provider-specific streaming
            streamingSource = _baseAIService.GenerateSQLStreamAsync(prompt, schema, context, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in managed streaming SQL generation");

            // Fallback to base service
            streamingSource = _baseAIService.GenerateSQLStreamAsync(prompt, schema, context, cancellationToken);
        }

        var fullResponse = "";
        await foreach (var response in streamingSource)
        {
            fullResponse += response.Content;
            yield return response;
        }

        stopwatch.Stop();

        // Log usage after streaming completes
        try
        {
            await LogUsageAsync(new LLMUsageLog
            {
                RequestId = requestId,
                UserId = GetCurrentUserId(),
                ProviderId = model.ProviderId,
                ModelId = model.ModelId,
                RequestType = "SQL_Stream",
                RequestText = prompt,
                ResponseText = fullResponse,
                InputTokens = EstimateTokens(prompt),
                OutputTokens = EstimateTokens(fullResponse),
                TotalTokens = EstimateTokens(prompt) + EstimateTokens(fullResponse),
                Cost = CalculateCost(model, EstimateTokens(prompt) + EstimateTokens(fullResponse)),
                DurationMs = stopwatch.ElapsedMilliseconds,
                Success = !string.IsNullOrEmpty(fullResponse),
                Timestamp = DateTime.UtcNow,
                Metadata = new Dictionary<string, object>
                {
                    { "streaming", true },
                    { "hasSchema", schema != null },
                    { "hasContext", context != null }
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error logging usage for streaming SQL generation");
        }
    }

    #endregion

    #region Helper Methods

    private async Task<LLMModelConfig?> GetModelForRequestAsync(string useCase, string? providerId = null, string? modelId = null)
    {
        try
        {
            // If specific model is requested, try to get it
            if (!string.IsNullOrEmpty(modelId))
            {
                var specificModel = await _llmManagementService.GetModelAsync(modelId);
                if (specificModel != null && specificModel.IsEnabled)
                {
                    return specificModel;
                }
            }

            // Get the best model for the use case
            return await _llmManagementService.GetDefaultModelAsync(useCase);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting model for use case {UseCase}", useCase);
            return null;
        }
    }

    private static AIOptions CreateAIOptionsFromModel(LLMModelConfig model)
    {
        return new AIOptions
        {
            Temperature = model.Temperature,
            MaxTokens = model.MaxTokens,
            FrequencyPenalty = model.FrequencyPenalty,
            PresencePenalty = model.PresencePenalty,
            AdditionalParameters = new Dictionary<string, object>
            {
                { "top_p", model.TopP }
            }
        };
    }

    private static int EstimateTokens(string text)
    {
        // Simple token estimation (roughly 4 characters per token)
        return string.IsNullOrEmpty(text) ? 0 : (int)Math.Ceiling(text.Length / 4.0);
    }

    private static decimal CalculateCost(LLMModelConfig model, int totalTokens)
    {
        return model.CostPerToken * totalTokens;
    }

    private string GetCurrentUserId()
    {
        try
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext?.User?.Identity?.IsAuthenticated == true)
            {
                return httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                    ?? httpContext.User.FindFirst("sub")?.Value
                    ?? httpContext.User.Identity.Name
                    ?? "unknown";
            }
            return "anonymous";
        }
        catch
        {
            return "system";
        }
    }

    public async Task LogUsageAsync(LLMUsageLog usageLog)
    {
        try
        {
            await _llmManagementService.LogUsageAsync(usageLog);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error logging LLM usage");
        }
    }

    public async Task<LLMModelConfig?> GetBestModelForUseCaseAsync(string useCase, string? preferredProviderId = null)
    {
        return await GetModelForRequestAsync(useCase, preferredProviderId, null);
    }

    public async Task<List<LLMProviderStatus>> GetAvailableProvidersAsync()
    {
        return await _llmManagementService.GetProviderHealthStatusAsync();
    }

    #endregion

    #region Delegate to Base Service (IAIService implementation)

    // Note: GenerateCompletionAsync is not part of IAIService interface, so we don't implement it here

    public Task<string> GenerateSQLAsync(string prompt)
    {
        return GenerateSQLWithManagedModelAsync(prompt);
    }

    public Task<string> GenerateSQLAsync(string prompt, CancellationToken cancellationToken = default)
    {
        return GenerateSQLWithManagedModelAsync(prompt, cancellationToken: cancellationToken);
    }

    public Task<string> GenerateInsightAsync(string query, object[] data)
    {
        return _baseAIService.GenerateInsightAsync(query, data);
    }

    public Task<string> GenerateVisualizationConfigAsync(string query, ColumnMetadata[] columns, object[] data)
    {
        return _baseAIService.GenerateVisualizationConfigAsync(query, columns, data);
    }

    public Task<double> CalculateConfidenceScoreAsync(string naturalLanguageQuery, string generatedSQL)
    {
        return _baseAIService.CalculateConfidenceScoreAsync(naturalLanguageQuery, generatedSQL);
    }

    public Task<string[]> GenerateQuerySuggestionsAsync(string context, SchemaMetadata schema)
    {
        return _baseAIService.GenerateQuerySuggestionsAsync(context, schema);
    }

    public Task<bool> ValidateQueryIntentAsync(string naturalLanguageQuery)
    {
        return _baseAIService.ValidateQueryIntentAsync(naturalLanguageQuery);
    }

    public IAsyncEnumerable<StreamingResponse> GenerateSQLStreamAsync(string prompt, SchemaMetadata? schema = null, QueryContext? context = null, CancellationToken cancellationToken = default)
    {
        return GenerateSQLStreamWithManagedModelAsync(prompt, schema, context, cancellationToken: cancellationToken);
    }

    public IAsyncEnumerable<StreamingResponse> GenerateInsightStreamAsync(string query, object[] data, AnalysisContext? context = null, CancellationToken cancellationToken = default)
    {
        return _baseAIService.GenerateInsightStreamAsync(query, data, context, cancellationToken);
    }

    public IAsyncEnumerable<StreamingResponse> GenerateExplanationStreamAsync(string sql, StreamingQueryComplexity complexity = StreamingQueryComplexity.Medium, CancellationToken cancellationToken = default)
    {
        return _baseAIService.GenerateExplanationStreamAsync(sql, complexity, cancellationToken);
    }

    // Note: GenerateCompletionAsync is not part of IAIService interface
    // This method is provided for compatibility - delegates to base service
    public async Task<string> GenerateCompletionAsync(string prompt, AIOptions options)
    {
        // Delegate to base service SQL generation as a fallback
        return await _baseAIService.GenerateSQLAsync(prompt);
    }

    #endregion
}
