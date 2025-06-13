using BIReportingCopilot.Core.Interfaces;
using BIReportingCopilot.Core.Interfaces.AI;
using BIReportingCopilot.Core.Models;
using BIReportingCopilot.Core.Models.QuerySuggestions;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Security.Claims;

namespace BIReportingCopilot.Infrastructure.AI.Core;

/// <summary>
/// AI service that uses LLM Management system for intelligent provider and model selection
/// </summary>
public class LLMAwareAIService : ILLMAwareAIService
{
    private readonly BIReportingCopilot.Infrastructure.AI.Core.AIService _baseAIService;
    private readonly ILLMManagementService _llmManagementService;
    private readonly ILogger<LLMAwareAIService> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public LLMAwareAIService(
        BIReportingCopilot.Infrastructure.AI.Core.AIService baseAIService,
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

    #region Missing Interface Method Implementations

    /// <summary>
    /// Generate SQL query (ILLMAwareAIService interface)
    /// </summary>
    public async Task<string> GenerateSqlQueryAsync(string naturalLanguageQuery, string schema, CancellationToken cancellationToken = default)
    {
        try
        {
            // Use the managed model approach with schema context
            var prompt = $"Schema: {schema}\n\nQuery: {naturalLanguageQuery}";
            return await GenerateSQLWithManagedModelAsync(prompt, cancellationToken: cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating SQL query for: {Query}", naturalLanguageQuery);
            return await _baseAIService.GenerateSQLAsync(naturalLanguageQuery, cancellationToken);
        }
    }

    /// <summary>
    /// Validate query intent (ILLMAwareAIService interface)
    /// </summary>
    public async Task<bool> ValidateQueryIntentAsync(string query, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _baseAIService.ValidateQueryIntentAsync(query);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating query intent for: {Query}", query);
            return false;
        }
    }

    /// <summary>
    /// Analyze query complexity (ILLMAwareAIService interface)
    /// </summary>
    public async Task<BIReportingCopilot.Core.Interfaces.AI.QueryAnalysisResult> AnalyzeQueryComplexityAsync(string query, CancellationToken cancellationToken = default)
    {
        try
        {
            // Use managed model for analysis
            var analysisPrompt = $"Analyze the complexity of this query: {query}";
            var analysisResponse = await GenerateCompletionWithManagedModelAsync(analysisPrompt, "Analysis", cancellationToken: cancellationToken);

            // Parse the response and create analysis result
            return new BIReportingCopilot.Core.Interfaces.AI.QueryAnalysisResult
            {
                QueryType = DetermineQueryType(query),
                ComplexityScore = CalculateComplexityScore(query),
                RequiredTables = ExtractTableNames(query),
                RequiredColumns = ExtractColumnNames(query),
                EstimatedExecutionTime = EstimateExecutionTime(query),
                Warnings = ExtractWarnings(query),
                Suggestions = ExtractSuggestions(analysisResponse)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing query complexity for: {Query}", query);
            return new BIReportingCopilot.Core.Interfaces.AI.QueryAnalysisResult
            {
                QueryType = "Unknown",
                ComplexityScore = 5,
                EstimatedExecutionTime = TimeSpan.FromSeconds(1)
            };
        }
    }

    /// <summary>
    /// Generate query suggestions (ILLMAwareAIService interface)
    /// </summary>
    public async Task<List<QuerySuggestion>> GenerateQuerySuggestionsAsync(string partialQuery, string schema, CancellationToken cancellationToken = default)
    {
        try
        {
            var prompt = $"Schema: {schema}\n\nGenerate query suggestions for: {partialQuery}";
            var response = await GenerateCompletionWithManagedModelAsync(prompt, "Suggestions", cancellationToken: cancellationToken);

            // Parse response into suggestions
            var suggestions = ParseQuerySuggestions(response);
            return suggestions.Select(s => new QuerySuggestion
            {
                QueryText = s,
                Description = $"Suggested query based on: {partialQuery}",
                Confidence = 0.8,
                Category = "AI Generated"
            }).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating query suggestions for: {PartialQuery}", partialQuery);
            return new List<QuerySuggestion>();
        }
    }

    /// <summary>
    /// Recommend visualization (ILLMAwareAIService interface)
    /// </summary>
    public async Task<VisualizationRecommendation> RecommendVisualizationAsync(object[] data, string[] columnNames, CancellationToken cancellationToken = default)
    {
        try
        {
            var dataDescription = $"Columns: {string.Join(", ", columnNames)}, Rows: {data.Length}";
            var prompt = $"Recommend visualization for data: {dataDescription}";
            var response = await GenerateCompletionWithManagedModelAsync(prompt, "Visualization", cancellationToken: cancellationToken);

            return new VisualizationRecommendation
            {
                RecommendedType = ExtractVisualizationType(response),
                Confidence = 0.8,
                Reasoning = response,
                Configuration = new Dictionary<string, object>
                {
                    ["columns"] = columnNames,
                    ["dataSize"] = data.Length
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recommending visualization");
            return new VisualizationRecommendation
            {
                RecommendedType = "table",
                Confidence = 0.5,
                Reasoning = "Default recommendation due to error"
            };
        }
    }

    /// <summary>
    /// Explain query results (ILLMAwareAIService interface)
    /// </summary>
    public async Task<string> ExplainQueryResultsAsync(object[] data, string originalQuery, CancellationToken cancellationToken = default)
    {
        try
        {
            var dataDescription = $"Query: {originalQuery}\nResults: {data.Length} rows";
            var prompt = $"Explain these query results: {dataDescription}";
            return await GenerateCompletionWithManagedModelAsync(prompt, "Explanation", cancellationToken: cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error explaining query results");
            return $"Query '{originalQuery}' returned {data.Length} results.";
        }
    }

    /// <summary>
    /// Check service health (ILLMAwareAIService interface)
    /// </summary>
    public async Task<bool> IsServiceHealthyAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            // Check if LLM management service is healthy
            var providers = await _llmManagementService.GetProviderHealthStatusAsync();
            var hasHealthyProvider = providers.Any(p => p.IsHealthy);

            // Check if base AI service is healthy
            var baseServiceHealthy = await _baseAIService.IsAvailableAsync(cancellationToken);

            return hasHealthyProvider && baseServiceHealthy;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking service health");
            return false;
        }
    }

    /// <summary>
    /// Get service metrics (ILLMAwareAIService interface)
    /// </summary>
    public async Task<BIReportingCopilot.Core.Interfaces.AI.AIServiceMetrics> GetServiceMetricsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var baseMetrics = await _baseAIService.GetServiceMetricsAsync(cancellationToken);
            var usage = await _llmManagementService.GetUsageStatisticsAsync(DateTime.UtcNow.AddDays(-1), DateTime.UtcNow, cancellationToken);

            return new BIReportingCopilot.Core.Interfaces.AI.AIServiceMetrics
            {
                TotalRequests = baseMetrics.TotalRequests + (int)usage.TotalRequests,
                SuccessfulRequests = baseMetrics.SuccessfulRequests + (int)usage.SuccessfulRequests,
                FailedRequests = baseMetrics.FailedRequests + (int)usage.FailedRequests,
                AverageResponseTime = (baseMetrics.AverageResponseTime + usage.AverageResponseTime.TotalMilliseconds) / 2,
                LastRequestTime = DateTime.UtcNow,
                ServiceStatus = await IsServiceHealthyAsync(cancellationToken) ? "Healthy" : "Unhealthy"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting service metrics");
            return new BIReportingCopilot.Core.Interfaces.AI.AIServiceMetrics
            {
                ServiceStatus = "Error",
                LastRequestTime = DateTime.UtcNow
            };
        }
    }

    #endregion

    #region Helper Methods for Analysis

    private static string DetermineQueryType(string query)
    {
        var upperQuery = query.ToUpperInvariant();
        if (upperQuery.Contains("SELECT")) return "SELECT";
        if (upperQuery.Contains("INSERT")) return "INSERT";
        if (upperQuery.Contains("UPDATE")) return "UPDATE";
        if (upperQuery.Contains("DELETE")) return "DELETE";
        return "Unknown";
    }

    private static int CalculateComplexityScore(string query)
    {
        var score = 1;
        var upperQuery = query.ToUpperInvariant();

        if (upperQuery.Contains("JOIN")) score += 2;
        if (upperQuery.Contains("SUBQUERY") || upperQuery.Contains("(SELECT")) score += 3;
        if (upperQuery.Contains("GROUP BY")) score += 1;
        if (upperQuery.Contains("ORDER BY")) score += 1;
        if (upperQuery.Contains("HAVING")) score += 2;

        return Math.Min(score, 10);
    }

    private static List<string> ExtractTableNames(string query)
    {
        // Simple extraction - in production, use proper SQL parser
        var tables = new List<string>();
        var words = query.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        for (int i = 0; i < words.Length - 1; i++)
        {
            if (words[i].ToUpperInvariant() == "FROM" || words[i].ToUpperInvariant() == "JOIN")
            {
                tables.Add(words[i + 1].Trim(',', ';', '(', ')'));
            }
        }

        return tables.Distinct().ToList();
    }

    private static List<string> ExtractColumnNames(string query)
    {
        // Simple extraction - in production, use proper SQL parser
        var columns = new List<string>();
        var selectIndex = query.ToUpperInvariant().IndexOf("SELECT");
        var fromIndex = query.ToUpperInvariant().IndexOf("FROM");

        if (selectIndex >= 0 && fromIndex > selectIndex)
        {
            var columnsPart = query.Substring(selectIndex + 6, fromIndex - selectIndex - 6);
            columns.AddRange(columnsPart.Split(',').Select(c => c.Trim()));
        }

        return columns;
    }

    private static TimeSpan EstimateExecutionTime(string query)
    {
        var complexity = CalculateComplexityScore(query);
        return TimeSpan.FromMilliseconds(complexity * 100);
    }

    private static List<string> ExtractWarnings(string query)
    {
        var warnings = new List<string>();
        var upperQuery = query.ToUpperInvariant();

        if (!upperQuery.Contains("WHERE") && upperQuery.Contains("DELETE"))
            warnings.Add("DELETE without WHERE clause detected");

        if (upperQuery.Contains("SELECT *"))
            warnings.Add("SELECT * may impact performance");

        return warnings;
    }

    private static List<string> ExtractSuggestions(string analysisResponse)
    {
        // Parse AI response for suggestions
        return new List<string> { "Consider adding indexes for better performance" };
    }

    private static List<string> ParseQuerySuggestions(string response)
    {
        // Parse AI response for query suggestions
        return response.Split('\n', StringSplitOptions.RemoveEmptyEntries)
                      .Where(line => !string.IsNullOrWhiteSpace(line))
                      .Take(5)
                      .ToList();
    }

    private static string ExtractVisualizationType(string response)
    {
        var lowerResponse = response.ToLowerInvariant();
        if (lowerResponse.Contains("chart") || lowerResponse.Contains("bar")) return "bar";
        if (lowerResponse.Contains("line")) return "line";
        if (lowerResponse.Contains("pie")) return "pie";
        if (lowerResponse.Contains("scatter")) return "scatter";
        return "table";
    }

    #endregion
}
