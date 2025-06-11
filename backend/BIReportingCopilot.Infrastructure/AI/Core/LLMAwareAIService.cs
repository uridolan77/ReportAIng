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
    private readonly IAIProviderFactory _providerFactory;
    private readonly ILogger<LLMAwareAIService> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public LLMAwareAIService(
        IAIService baseAIService,
        ILLMManagementService llmManagementService,
        IAIProviderFactory providerFactory,
        ILogger<LLMAwareAIService> logger,
        IHttpContextAccessor httpContextAccessor)
    {
        _baseAIService = baseAIService;
        _llmManagementService = llmManagementService;
        _providerFactory = providerFactory;
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
                return await _baseAIService.GenerateCompletionAsync(prompt, new AIOptions(), cancellationToken);
            }

            // Get the provider
            var provider = _providerFactory.GetProvider(model.ProviderId);
            
            // Create AI options from model configuration
            var options = CreateAIOptionsFromModel(model);
            
            // Generate completion
            var response = await provider.GenerateCompletionAsync(prompt, options);
            
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

            // Fallback to base service
            return await _baseAIService.GenerateCompletionAsync(prompt, new AIOptions(), cancellationToken);
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
        
        try
        {
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

            // For now, use the base service streaming but log the usage
            // TODO: Implement provider-specific streaming
            var fullResponse = "";
            await foreach (var response in _baseAIService.GenerateSQLStreamAsync(prompt, schema, context, cancellationToken))
            {
                fullResponse += response.Content;
                yield return response;
            }

            stopwatch.Stop();

            // Log usage after streaming completes
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
            _logger.LogError(ex, "Error in managed streaming SQL generation");
            
            // Fallback to base service
            await foreach (var response in _baseAIService.GenerateSQLStreamAsync(prompt, schema, context, cancellationToken))
            {
                yield return response;
            }
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
            TopP = model.TopP,
            FrequencyPenalty = model.FrequencyPenalty,
            PresencePenalty = model.PresencePenalty
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

    public Task<string> GenerateCompletionAsync(string prompt, AIOptions options, CancellationToken cancellationToken = default)
    {
        return _baseAIService.GenerateCompletionAsync(prompt, options, cancellationToken);
    }

    public Task<string> GenerateSQLAsync(string prompt, CancellationToken cancellationToken = default)
    {
        return GenerateSQLWithManagedModelAsync(prompt, cancellationToken: cancellationToken);
    }

    public Task<string> GenerateInsightsAsync(string data, CancellationToken cancellationToken = default)
    {
        return GenerateInsightsWithManagedModelAsync(data, cancellationToken: cancellationToken);
    }

    public Task<double> CalculateConfidenceScoreAsync(string query, string sql, CancellationToken cancellationToken = default)
    {
        return _baseAIService.CalculateConfidenceScoreAsync(query, sql, cancellationToken);
    }

    public IAsyncEnumerable<StreamingResponse> GenerateSQLStreamAsync(string prompt, SchemaMetadata? schema = null, QueryContext? context = null, CancellationToken cancellationToken = default)
    {
        return GenerateSQLStreamWithManagedModelAsync(prompt, schema, context, cancellationToken: cancellationToken);
    }

    public Task<string> GenerateCompletionAsync(string prompt, AIOptions options)
    {
        return GenerateCompletionAsync(prompt, options, CancellationToken.None);
    }

    #endregion
}
