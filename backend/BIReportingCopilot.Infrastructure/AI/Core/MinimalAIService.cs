using BIReportingCopilot.Core.Interfaces.AI;
using BIReportingCopilot.Core.Interfaces.Cache;
using BIReportingCopilot.Core.Models;
using BIReportingCopilot.Core.Models.QuerySuggestions;
using BIReportingCopilot.Infrastructure.AI.Providers;
using Microsoft.Extensions.Logging;
using System.Linq;
using NLUResult = BIReportingCopilot.Core.Interfaces.AI.NLUResult;

namespace BIReportingCopilot.Infrastructure.AI.Core;

/// <summary>
/// Minimal AI service implementation to avoid circular dependencies during startup
/// This service has minimal dependencies and initializes heavy services lazily
/// </summary>
public class MinimalAIService : IAIService
{
    private readonly IAIProviderFactory _providerFactory;
    private readonly ILogger<AIService> _logger;
    private readonly ICacheService _cacheService;
    private IAIProvider? _provider;

    public MinimalAIService(
        IAIProviderFactory providerFactory,
        ILogger<AIService> logger,
        ICacheService cacheService)
    {
        _providerFactory = providerFactory;
        _logger = logger;
        _cacheService = cacheService;
        
        _logger.LogInformation("🚀 Minimal AI service initialized (prevents startup hanging)");
    }

    #region Core AI Methods

    public async Task<string> GenerateSQLAsync(string prompt, CancellationToken cancellationToken = default)
    {
        try
        {
            var provider = await GetProviderAsync();
            var options = new AIOptions
            {
                MaxTokens = 1500,
                Temperature = 0.1f,
                TimeoutSeconds = 90  // Increased timeout for better reliability
            };

            var result = await provider.GenerateCompletionAsync(
                $"Generate SQL for: {prompt}", 
                options, 
                cancellationToken);

            return result ?? "SELECT 1 -- Error generating SQL";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating SQL for prompt: {Prompt}", prompt);
            return "SELECT 1 -- Error generating SQL";
        }
    }

    public async Task<string> GenerateInsightAsync(string query, object[] data)
    {
        try
        {
            var provider = await GetProviderAsync();
            var dataPreview = GenerateDataPreview(data);
            var prompt = $"Analyze this query result and provide insights:\nQuery: {query}\nData: {dataPreview}";
            
            var options = new AIOptions
            {
                MaxTokens = 1000,
                Temperature = 0.3f,
                TimeoutSeconds = 90  // Increased timeout for better reliability
            };

            var result = await provider.GenerateCompletionAsync(prompt, options);
            return result ?? "No insights available";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating insight for query: {Query}", query);
            return "No insights available";
        }
    }

    public async Task<double> CalculateConfidenceScoreAsync(string query, string sql, CancellationToken cancellationToken = default)
    {
        try
        {
            // Simple confidence calculation based on query complexity
            var queryWords = query.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;
            var sqlComplexity = sql.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;
            
            // Basic heuristic: shorter queries with reasonable SQL length get higher confidence
            var baseScore = Math.Max(0.5, 1.0 - (Math.Abs(queryWords - sqlComplexity / 3.0) / 20.0));
            return Math.Min(1.0, baseScore);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating confidence score");
            return 0.5; // Default confidence
        }
    }

    #endregion

    #region Provider Management

    private async Task<IAIProvider> GetProviderAsync()
    {
        if (_provider == null)
        {
            _logger.LogDebug("🔍 [MINIMAL-AI] Creating AI provider via factory...");
            _provider = await _providerFactory.CreateProviderAsync("openai");
            _logger.LogDebug("🔍 [MINIMAL-AI] AI provider created: {ProviderType}", _provider?.GetType().Name ?? "null");
        }
        return _provider;
    }

    #endregion

    #region Helper Methods

    private string GenerateDataPreview(object[] data)
    {
        if (data == null || data.Length == 0)
            return "No data";

        var preview = string.Join(", ", data.Take(5).Select(d => d?.ToString() ?? "null"));
        if (data.Length > 5)
            preview += $" ... ({data.Length} total items)";
        
        return preview;
    }

    #endregion

    #region Minimal Interface Implementations

    public async Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var provider = await GetProviderAsync();
            return provider != null;
        }
        catch
        {
            return false;
        }
    }

    public async Task<string> GenerateQueryAsync(string query, SchemaMetadata? schema = null, CancellationToken cancellationToken = default)
    {
        return await GenerateSQLAsync(query, cancellationToken);
    }

    public async Task<QueryProcessingResult> ProcessNaturalLanguageAsync(string query, string? context = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var sql = await GenerateSQLAsync(query, cancellationToken);
            var confidence = await CalculateConfidenceScoreAsync(query, sql, cancellationToken);
            
            return new QueryProcessingResult
            {
                GeneratedSQL = sql,
                ConfidenceScore = confidence,
                ProcessingTime = TimeSpan.FromMilliseconds(100), // Placeholder
                Success = true
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing natural language query");
            return new QueryProcessingResult
            {
                Success = false,
                Error = ex.Message
            };
        }
    }

    public async Task<NLUResult> AnalyzeIntentAsync(string text, CancellationToken cancellationToken = default)
    {
        // Simple intent analysis
        return new NLUResult
        {
            Intent = text.ToLower().Contains("select") ? "query" : "unknown",
            Confidence = 0.7,
            Entities = new List<EntityExtraction>()
        };
    }

    public async Task<bool> IsHealthyAsync(CancellationToken cancellationToken = default)
    {
        return await IsAvailableAsync(cancellationToken);
    }

    public async Task<AIServiceStatus> GetStatusAsync(CancellationToken cancellationToken = default)
    {
        var isHealthy = await IsHealthyAsync(cancellationToken);
        return new AIServiceStatus
        {
            IsHealthy = isHealthy,
            Status = isHealthy ? "Healthy" : "Unhealthy",
            LastUpdated = DateTime.UtcNow
        };
    }

    public async Task<AIServiceMetrics> GetMetricsAsync(CancellationToken cancellationToken = default)
    {
        return new AIServiceMetrics
        {
            TotalRequests = 0,
            SuccessfulRequests = 0,
            FailedRequests = 0,
            AverageResponseTime = 0,
            LastHealthCheck = DateTime.UtcNow,
            IsAvailable = await IsAvailableAsync(cancellationToken),
            ProviderName = "Minimal AI Service",
            Version = "1.0.0"
        };
    }

    #endregion

    #region Missing Interface Methods

    public async Task<List<QuerySuggestion>> GetQuerySuggestionsAsync(string partialQuery, SchemaMetadata? schema = null, CancellationToken cancellationToken = default)
    {
        await Task.Delay(1, cancellationToken);
        return new List<QuerySuggestion>
        {
            new QuerySuggestion { Text = "SELECT * FROM " + partialQuery, Confidence = 0.8m }
        };
    }

    public async Task<QueryValidationResult> ValidateQueryAsync(string query, CancellationToken cancellationToken = default)
    {
        await Task.Delay(1, cancellationToken);
        return new QueryValidationResult
        {
            IsValid = true,
            Errors = new List<string>(),
            Suggestions = new List<string>()
        };
    }

    public async Task<QueryOptimizationResult> OptimizeQueryAsync(string query, CancellationToken cancellationToken = default)
    {
        await Task.Delay(1, cancellationToken);
        return new QueryOptimizationResult
        {
            OptimizedQuery = query,
            ImprovementScore = 0,
            Suggestions = new List<OptimizationSuggestion>()
        };
    }

    public async Task<SemanticAnalysisResult> AnalyzeSemanticContentAsync(string content, CancellationToken cancellationToken = default)
    {
        await Task.Delay(1, cancellationToken);
        return new SemanticAnalysisResult
        {
            Keywords = new List<string>(),
            Entities = new List<EntityExtraction>(),
            Sentiment = new SentimentAnalysis { Confidence = 0.5, Sentiment = "Neutral" }
        };
    }

    public async Task<double> CalculateSemanticSimilarityAsync(string text1, string text2, CancellationToken cancellationToken = default)
    {
        await Task.Delay(1, cancellationToken);
        return 0.5; // Default similarity
    }

    public async Task<List<EntityExtraction>> ExtractEntitiesAsync(string text, CancellationToken cancellationToken = default)
    {
        await Task.Delay(1, cancellationToken);
        return new List<EntityExtraction>();
    }

    public async Task<string> GenerateSQLAsync(string prompt, SchemaMetadata? schema = null, CancellationToken cancellationToken = default)
    {
        return await GenerateSQLAsync(prompt, cancellationToken);
    }

    public async Task<IAsyncEnumerable<string>> GenerateSQLStreamAsync(string prompt, SchemaMetadata schema, string context, CancellationToken cancellationToken = default)
    {
        await Task.Delay(1, cancellationToken);
        return GetSQLStreamAsync(prompt);
    }

    private async IAsyncEnumerable<string> GetSQLStreamAsync(string prompt)
    {
        yield return "SELECT ";
        yield return "* ";
        yield return "FROM ";
        yield return "table";
    }

    public async Task<string> GenerateVisualizationConfigAsync(string sql, SchemaMetadata? schema = null, CancellationToken cancellationToken = default)
    {
        await Task.Delay(1, cancellationToken);
        return "{ \"type\": \"bar\", \"data\": [] }";
    }

    public async Task<IAsyncEnumerable<string>> GenerateInsightStreamAsync(string data, string context, CancellationToken cancellationToken = default)
    {
        await Task.Delay(1, cancellationToken);
        return GetEmptyAsyncEnumerable();
    }

    private async IAsyncEnumerable<string> GetEmptyAsyncEnumerable()
    {
        yield break;
    }

    public async Task<string> GenerateInsightAsync(string data, string context, CancellationToken cancellationToken = default)
    {
        await Task.Delay(1, cancellationToken);
        return "Generated insight";
    }

    public async Task<float[]> GenerateEmbeddingAsync(string text, CancellationToken cancellationToken = default)
    {
        await Task.Delay(1, cancellationToken);
        return new float[384]; // Standard embedding size
    }

    public async Task<bool> ValidateQueryIntentAsync(string query, string expectedIntent, CancellationToken cancellationToken = default)
    {
        await Task.Delay(1, cancellationToken);
        return true;
    }

    public async Task<List<QuerySuggestion>> GenerateQuerySuggestionsAsync(string context, SchemaMetadata? schema = null, CancellationToken cancellationToken = default)
    {
        await Task.Delay(1, cancellationToken);
        return new List<QuerySuggestion>
        {
            new QuerySuggestion { Text = "SELECT * FROM table", Confidence = 0.8m }
        };
    }

    #endregion
}
