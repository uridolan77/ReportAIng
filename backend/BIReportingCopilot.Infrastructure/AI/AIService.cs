using Azure.AI.OpenAI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using BIReportingCopilot.Core.Interfaces;
using BIReportingCopilot.Core.Models;
using BIReportingCopilot.Core.Configuration;
using System.Text.Json;
using System.Text;
using Azure;
using System.Runtime.CompilerServices;
using Polly;
using Polly.Extensions.Http;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using BIReportingCopilot.Infrastructure.Monitoring;
using CoreModels = BIReportingCopilot.Core.Models;

namespace BIReportingCopilot.Infrastructure.AI;

/// <summary>
/// Enhanced AI service with built-in resilience, monitoring, and adaptive capabilities
/// Consolidates multiple decorator patterns into a single, comprehensive service
/// </summary>
public class AIService : IAIService
{
    private readonly IAIProviderFactory _providerFactory;
    private readonly ILogger<AIService> _logger;
    private readonly ICacheService _cacheService;
    private readonly PromptTemplateManager _promptManager;
    private readonly IContextManager _contextManager;
    private readonly List<QueryExample> _examples;
    private readonly IAIProvider _provider;

    // Resilience policies
    private IAsyncPolicy<string> _retryPolicy = null!;
    private IAsyncPolicy<string> _circuitBreakerPolicy = null!;
    private IAsyncPolicy<string> _combinedPolicy = null!;

    // Monitoring
    private static readonly ActivitySource ActivitySource = new("BIReportingCopilot.AIService");
    private readonly IMetricsCollector? _metricsCollector;

    // Adaptive learning components
    private readonly FeedbackLearningEngine? _learningEngine;
    private readonly PromptOptimizer? _promptOptimizer;

    public AIService(
        IAIProviderFactory providerFactory,
        ILogger<AIService> logger,
        ICacheService cacheService,
        IContextManager contextManager,
        IMetricsCollector? metricsCollector = null,
        FeedbackLearningEngine? learningEngine = null,
        PromptOptimizer? promptOptimizer = null)
    {
        _providerFactory = providerFactory;
        _logger = logger;
        _cacheService = cacheService;
        _contextManager = contextManager;
        _metricsCollector = metricsCollector;
        _learningEngine = learningEngine;
        _promptOptimizer = promptOptimizer;
        _examples = InitializeExamples();
        _promptManager = new PromptTemplateManager();

        // Get the appropriate provider
        _provider = _providerFactory.GetProvider();

        // Initialize resilience policies
        InitializeResiliencePolicies();

        _logger.LogInformation("Enhanced AI service with adaptive learning initialized with provider: {ProviderName}", _provider?.ProviderName ?? "None");
    }

    #region Standard AI Operations

    public async Task<string> GenerateSQLAsync(string prompt)
    {
        return await GenerateSQLAsync(prompt, CancellationToken.None);
    }

    public async Task<string> GenerateSQLAsync(string prompt, CancellationToken cancellationToken)
    {
        using var activity = ActivitySource.StartActivity("GenerateSQL");
        var stopwatch = Stopwatch.StartNew();

        try
        {
            // Add trace tags
            activity?.SetTag("prompt.length", prompt.Length);
            activity?.SetTag("operation", "sql_generation");

            _logger.LogInformation("Building enhanced prompt for SQL generation...");

            return await _combinedPolicy.ExecuteAsync(async () =>
            {
                using var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
                using var combinedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);

                // Apply adaptive learning if available
                string finalPrompt = prompt;
                if (_learningEngine != null && _promptOptimizer != null)
                {
                    try
                    {
                        var learningInsights = await _learningEngine.GetLearningInsightsAsync(prompt);
                        finalPrompt = await _promptOptimizer.OptimizePromptAsync(prompt, learningInsights);
                        _logger.LogDebug("Applied adaptive learning optimization to prompt");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to apply adaptive learning, using original prompt");
                        finalPrompt = prompt;
                    }
                }

                var enhancedPrompt = BuildEnhancedPrompt(finalPrompt);

                var options = new AIOptions
                {
                    SystemMessage = "You are an expert SQL developer. Generate only valid SQL queries without explanations.",
                    Temperature = 0.1f,
                    MaxTokens = 2000,
                    FrequencyPenalty = 0.0f,
                    PresencePenalty = 0.0f,
                    TimeoutSeconds = 30
                };

                var generatedSQL = await _provider.GenerateCompletionAsync(enhancedPrompt, options, combinedCts.Token);

                if (string.IsNullOrEmpty(generatedSQL))
                {
                    throw new InvalidOperationException("AI service returned empty result");
                }

                // Clean up the response
                generatedSQL = CleanSQLResponse(generatedSQL);

                stopwatch.Stop();
                activity?.SetTag("success", true);
                activity?.SetTag("duration_ms", stopwatch.ElapsedMilliseconds);

                // Record metrics
                _metricsCollector?.RecordQueryExecution("sql_generation", stopwatch.ElapsedMilliseconds, true);

                _logger.LogInformation("Generated SQL successfully in {Duration}ms", stopwatch.ElapsedMilliseconds);
                return generatedSQL;
            });
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            activity?.SetTag("error", true);
            activity?.SetTag("error.type", ex.GetType().Name);
            activity?.SetTag("error.message", ex.Message);

            // Record error metrics
            _metricsCollector?.RecordError("sql_generation", "AIService", ex);

            _logger.LogError(ex, "Failed to generate SQL after all retry attempts for prompt: {Prompt}", prompt);
            return GenerateFallbackSQL(prompt);
        }
    }

    public async Task<string> GenerateInsightAsync(string query, object[] data)
    {
        try
        {
            var dataPreview = GenerateDataPreview(data);
            var insightPrompt = $@"
Analyze the following query results and provide business insights:

Query: {query}
Data Preview: {dataPreview}

Provide 2-3 key insights about the data, focusing on:
1. Notable patterns or trends
2. Business implications
3. Potential areas for further investigation

Keep insights concise and actionable.";

            var options = new AIOptions
            {
                SystemMessage = "You are a business intelligence analyst providing insights from data.",
                Temperature = 0.3f,
                MaxTokens = 500,
                TimeoutSeconds = 30
            };

            return await _provider.GenerateCompletionAsync(insightPrompt, options);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating insights");
            return GenerateFallbackInsight(query, data);
        }
    }

    public async Task<string> GenerateVisualizationConfigAsync(string query, ColumnMetadata[] columns, object[] data)
    {
        try
        {
            var columnInfo = string.Join(", ", columns.Select(c => $"{c.Name} ({c.DataType})"));
            var dataPreview = GenerateDataPreview(data);

            var vizPrompt = $@"
Based on this query and data, suggest the best visualization:

Query: {query}
Columns: {columnInfo}
Data Preview: {dataPreview}

Return a JSON object with:
- type: chart type (bar, line, pie, table, scatter, area)
- title: descriptive title
- xAxis: column for x-axis (if applicable)
- yAxis: column for y-axis (if applicable)
- groupBy: column for grouping (if applicable)

Return only valid JSON.";

            var options = new AIOptions
            {
                SystemMessage = "You are a data visualization expert. Return only valid JSON.",
                Temperature = 0.2f,
                MaxTokens = 300,
                TimeoutSeconds = 30
            };

            return await _provider.GenerateCompletionAsync(vizPrompt, options);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating visualization config");
            return """{"type": "table", "title": "Query Results"}""";
        }
    }

    public async Task<double> CalculateConfidenceScoreAsync(string naturalLanguageQuery, string generatedSQL)
    {
        // Implement confidence calculation logic
        var score = 0.8; // Default confidence

        // Add various confidence factors
        if (generatedSQL.Contains("SELECT", StringComparison.OrdinalIgnoreCase)) score += 0.1;
        if (generatedSQL.Contains("FROM", StringComparison.OrdinalIgnoreCase)) score += 0.1;
        if (!generatedSQL.Contains("ERROR", StringComparison.OrdinalIgnoreCase)) score += 0.1;

        return Math.Min(1.0, score);
    }

    public async Task<string[]> GenerateQuerySuggestionsAsync(string context, SchemaMetadata schema)
    {
        var suggestions = new List<string>
        {
            "Show me the total count of records",
            "What are the top 10 items by value?",
            "Show me data from the last 30 days",
            "Group results by category",
            "Show me the average values"
        };

        // Add schema-specific suggestions
        foreach (var table in schema.Tables.Take(3))
        {
            suggestions.Add($"Show me all data from {table.Name}");
            if (table.Columns.Any(c => c.Name.ToLower().Contains("date")))
            {
                suggestions.Add($"Show me recent {table.Name} records");
            }
        }

        return suggestions.Take(8).ToArray();
    }

    public async Task<bool> ValidateQueryIntentAsync(string naturalLanguageQuery)
    {
        // Implement query intent validation
        var lowerQuery = naturalLanguageQuery.ToLowerInvariant();

        // Check for SQL injection attempts
        var dangerousPatterns = new[] { "drop", "delete", "truncate", "alter", "create", "insert", "update" };
        if (dangerousPatterns.Any(pattern => lowerQuery.Contains(pattern)))
        {
            return false;
        }

        // Check for valid query patterns
        var validPatterns = new[] { "show", "get", "find", "list", "count", "sum", "average", "total" };
        return validPatterns.Any(pattern => lowerQuery.Contains(pattern));
    }

    #endregion

    #region Streaming Operations

    public async IAsyncEnumerable<StreamingResponse> GenerateSQLStreamAsync(
        string prompt,
        SchemaMetadata? schema = null,
        CoreModels.QueryContext? context = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        if (_provider == null)
        {
            yield return new StreamingResponse { Content = "AI service not configured", IsComplete = true };
            yield break;
        }

        if (_promptManager == null)
        {
            yield return new StreamingResponse { Content = "Prompt manager not configured", IsComplete = true };
            yield break;
        }

        string enhancedPrompt;
        try
        {
            enhancedPrompt = await _promptManager.BuildSQLGenerationPromptAsync(prompt, schema, context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error building SQL generation prompt");
            enhancedPrompt = $"Generate SQL for: {prompt}"; // Fallback prompt
        }

        var options = new AIOptions
        {
            SystemMessage = "You are an expert SQL developer. Generate only valid SQL queries.",
            Temperature = 0.1f,
            MaxTokens = 1000,
            TimeoutSeconds = 30
        };

        await foreach (var response in _provider.GenerateCompletionStreamAsync(enhancedPrompt, options, cancellationToken))
        {
            yield return response;
        }
    }

    public async IAsyncEnumerable<StreamingResponse> GenerateInsightStreamAsync(
        string query,
        object[] data,
        CoreModels.AnalysisContext? context = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        if (_provider == null)
        {
            yield return new StreamingResponse { Content = "AI service not configured", IsComplete = true };
            yield break;
        }

        if (_promptManager == null)
        {
            yield return new StreamingResponse { Content = "Prompt manager not configured", IsComplete = true };
            yield break;
        }

        string insightPrompt;
        try
        {
            insightPrompt = await _promptManager.BuildInsightGenerationPromptAsync(query, data, context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error building insight generation prompt");
            insightPrompt = $"Analyze the following query and provide insights: {query}"; // Fallback prompt
        }

        var options = new AIOptions
        {
            SystemMessage = "You are a business intelligence analyst providing insights from data.",
            Temperature = 0.3f,
            MaxTokens = 500,
            TimeoutSeconds = 30
        };

        await foreach (var response in _provider.GenerateCompletionStreamAsync(insightPrompt, options, cancellationToken))
        {
            yield return response;
        }
    }

    public async IAsyncEnumerable<StreamingResponse> GenerateExplanationStreamAsync(
        string sql,
        CoreModels.StreamingQueryComplexity complexity = CoreModels.StreamingQueryComplexity.Medium,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        if (_provider == null)
        {
            yield return new StreamingResponse { Content = "AI service not configured", IsComplete = true };
            yield break;
        }

        if (_promptManager == null)
        {
            yield return new StreamingResponse { Content = "Prompt manager not configured", IsComplete = true };
            yield break;
        }

        string explanationPrompt;
        try
        {
            explanationPrompt = await _promptManager.BuildSQLExplanationPromptAsync(sql, complexity);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error building SQL explanation prompt");
            explanationPrompt = $"Explain the following SQL query: {sql}"; // Fallback prompt
        }

        var options = new AIOptions
        {
            SystemMessage = "You are a SQL expert explaining queries in simple terms.",
            Temperature = 0.2f,
            MaxTokens = 800,
            TimeoutSeconds = 30
        };

        await foreach (var response in _provider.GenerateCompletionStreamAsync(explanationPrompt, options, cancellationToken))
        {
            yield return response;
        }
    }

    #endregion

    #region Private Helper Methods



    private string BuildEnhancedPrompt(string originalPrompt)
    {
        // Simplified version - full implementation would be more sophisticated
        return $"Generate SQL for: {originalPrompt}";
    }

    private string CleanSQLResponse(string sql)
    {
        // Remove markdown formatting and clean up the SQL
        return sql.Replace("```sql", "").Replace("```", "").Trim();
    }

    private string GenerateFallbackSQL(string prompt)
    {
        return "SELECT 'Fallback response - AI service not configured' as Message";
    }

    private string GenerateFallbackInsight(string query, object[] data)
    {
        return "AI service not configured. Unable to generate insights.";
    }

    private string GenerateDataPreview(object[] data)
    {
        if (data.Length == 0) return "No data available";

        var preview = data.Take(3).Select(item => item.ToString()).ToArray();
        return string.Join(", ", preview) + (data.Length > 3 ? "..." : "");
    }

    private List<QueryExample> InitializeExamples()
    {
        return new List<QueryExample>
        {
            new() { NaturalLanguage = "Show all users", SQL = "SELECT * FROM Users" },
            new() { NaturalLanguage = "Count total orders", SQL = "SELECT COUNT(*) FROM Orders" }
        };
    }

    private void InitializeResiliencePolicies()
    {
        // Configure retry policy with exponential backoff
        _retryPolicy = Policy
            .HandleResult<string>(r => string.IsNullOrEmpty(r))
            .Or<HttpRequestException>()
            .Or<TaskCanceledException>()
            .Or<TimeoutException>()
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                onRetry: (outcome, timespan, retryCount, context) =>
                {
                    _logger.LogWarning("AI service retry {RetryCount} after {Delay}ms. Reason: {Reason}",
                        retryCount, timespan.TotalMilliseconds, outcome.Exception?.Message ?? "Empty result");
                });

        // Configure circuit breaker
        _circuitBreakerPolicy = Policy
            .HandleResult<string>(r => string.IsNullOrEmpty(r))
            .Or<HttpRequestException>()
            .Or<TaskCanceledException>()
            .Or<TimeoutException>()
            .CircuitBreakerAsync(
                handledEventsAllowedBeforeBreaking: 5,
                durationOfBreak: TimeSpan.FromMinutes(1),
                onBreak: (exception, duration) =>
                {
                    _logger.LogError("AI service circuit breaker opened for {Duration}ms. Reason: {Reason}",
                        duration.TotalMilliseconds, exception.Exception?.Message ?? "Multiple failures");
                },
                onReset: () =>
                {
                    _logger.LogInformation("AI service circuit breaker reset - service recovered");
                },
                onHalfOpen: () =>
                {
                    _logger.LogInformation("AI service circuit breaker half-open - testing service");
                });

        // Combine policies: retry -> circuit breaker
        _combinedPolicy = Policy.WrapAsync(_retryPolicy, _circuitBreakerPolicy);
    }

    /// <summary>
    /// Process user feedback to improve future generations (adaptive learning)
    /// </summary>
    public async Task ProcessFeedbackAsync(string originalPrompt, string generatedSQL, QueryFeedback feedback, string userId)
    {
        if (_learningEngine == null)
        {
            _logger.LogDebug("Learning engine not available, skipping feedback processing");
            return;
        }

        try
        {
            await _learningEngine.ProcessFeedbackAsync(originalPrompt, generatedSQL, feedback, userId);
            _logger.LogInformation("Processed feedback for user {UserId}, feedback: {Feedback}", userId, feedback.Feedback);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing feedback for user {UserId}", userId);
        }
    }

    /// <summary>
    /// Get learning statistics for monitoring (adaptive learning)
    /// </summary>
    public async Task<LearningStatistics> GetLearningStatisticsAsync()
    {
        if (_learningEngine == null)
        {
            return new LearningStatistics
            {
                TotalGenerations = 0,
                TotalFeedbackItems = 0,
                AverageRating = 0.0,
                AverageConfidence = 0.0,
                UniqueUsers = 0,
                PopularPatterns = new Dictionary<string, int>(),
                LastUpdated = DateTime.UtcNow
            };
        }

        try
        {
            return await _learningEngine.GetLearningStatisticsAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting learning statistics");
            return new LearningStatistics();
        }
    }

    #endregion
}

/// <summary>
/// Learning statistics for monitoring
/// </summary>
public class LearningStatistics
{
    public int TotalGenerations { get; set; }
    public int TotalFeedbackItems { get; set; }
    public double AverageRating { get; set; }
    public double AverageConfidence { get; set; }
    public int UniqueUsers { get; set; }
    public Dictionary<string, int> PopularPatterns { get; set; } = new();
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
}
