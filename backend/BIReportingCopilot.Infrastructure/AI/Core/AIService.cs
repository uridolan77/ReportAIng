using Azure.AI.OpenAI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using BIReportingCopilot.Core.Interfaces;
using BIReportingCopilot.Core.Models;
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

namespace BIReportingCopilot.Infrastructure.AI.Core;

/// <summary>
/// Enhanced AI service with built-in resilience, monitoring, and adaptive capabilities
/// Consolidates multiple decorator patterns into a single, comprehensive service
/// </summary>
public class AIService : IAIService
{
    private readonly IAIProviderFactory _providerFactory;
    private readonly ILogger<AIService> _logger;
    private readonly ICacheService _cacheService;
    private readonly IPromptService _promptService;
    private readonly IContextManager _contextManager;
    private readonly List<QueryExample> _examples;
    private readonly IAIProvider _provider = null!;

    // Resilience policies
    private IAsyncPolicy<string> _retryPolicy = null!;
    private IAsyncPolicy<string> _circuitBreakerPolicy = null!;
    private IAsyncPolicy<string> _combinedPolicy = null!;

    // Monitoring
    private static readonly ActivitySource ActivitySource = new("BIReportingCopilot.AIService");
    private readonly IMetricsCollector? _metricsCollector;

    // Adaptive learning components
    private readonly LearningService? _learningService;

    public AIService(
        IAIProviderFactory providerFactory,
        ILogger<AIService> logger,
        ICacheService cacheService,
        IContextManager contextManager,
        IPromptService promptService,
        IMetricsCollector? metricsCollector = null,
        LearningService? learningService = null)
    {
        _providerFactory = providerFactory;
        _logger = logger;
        _cacheService = cacheService;
        _contextManager = contextManager;
        _promptService = promptService;
        _metricsCollector = metricsCollector;
        _learningService = learningService;
        _examples = InitializeExamples();

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
                if (_learningService != null)
                {
                    try
                    {
                        var learningInsights = await _learningService.GetLearningInsightsAsync("system");
                        _logger.LogDebug("Retrieved learning insights for prompt optimization");
                        // Note: Prompt optimization logic can be added here when needed
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to retrieve learning insights");
                    }
                }

                var enhancedPrompt = BuildEnhancedPrompt(finalPrompt);

                var options = new AIOptions
                {
                    SystemMessage = "You are an expert SQL developer. Generate only valid SQL queries without explanations. Always use WITH (NOLOCK) hints on all table references for better read performance in reporting scenarios. Format as: FROM TableName WITH (NOLOCK) or FROM TableName alias WITH (NOLOCK) - never use AS keyword with table hints.",
                    Temperature = 0.1f,
                    MaxTokens = 2000,
                    FrequencyPenalty = 0.0f,
                    PresencePenalty = 0.0f,
                    TimeoutSeconds = 60 // Increased timeout for better reliability
                };

                // Log the full prompt being sent to AI
                _logger.LogInformation("AI SQL Generation Request - System Message: {SystemMessage}", options.SystemMessage);
                _logger.LogInformation("AI SQL Generation Request - User Prompt: {UserPrompt}",
                    enhancedPrompt.Length > 2000 ? enhancedPrompt.Substring(0, 2000) + "..." : enhancedPrompt);
                _logger.LogInformation("AI SQL Generation Request - Options: Temperature={Temperature}, MaxTokens={MaxTokens}, TimeoutSeconds={TimeoutSeconds}",
                    options.Temperature, options.MaxTokens, options.TimeoutSeconds);

                var generatedSQL = await _provider.GenerateCompletionAsync(enhancedPrompt, options, combinedCts.Token);

                if (string.IsNullOrEmpty(generatedSQL))
                {
                    _logger.LogWarning("AI service returned empty result for prompt: {Prompt}",
                        finalPrompt.Length > 500 ? finalPrompt.Substring(0, 500) + "..." : finalPrompt);
                    throw new InvalidOperationException("AI service returned empty result");
                }

                // Log the AI response before cleaning
                _logger.LogInformation("AI SQL Generation Response (raw): {RawResponse}",
                    generatedSQL.Length > 1000 ? generatedSQL.Substring(0, 1000) + "..." : generatedSQL);

                // Clean up the response
                generatedSQL = CleanSQLResponse(generatedSQL);

                // Log the cleaned response
                _logger.LogInformation("AI SQL Generation Response (cleaned): {CleanedSQL}",
                    generatedSQL.Length > 1000 ? generatedSQL.Substring(0, 1000) + "..." : generatedSQL);

                stopwatch.Stop();
                activity?.SetTag("success", true);
                activity?.SetTag("duration_ms", stopwatch.ElapsedMilliseconds);
                activity?.SetTag("response.length", generatedSQL.Length);

                // Record metrics
                _metricsCollector?.RecordQueryExecution("sql_generation", stopwatch.ElapsedMilliseconds, true, 0);

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
            throw;
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
            throw;
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

    public Task<double> CalculateConfidenceScoreAsync(string naturalLanguageQuery, string generatedSQL)
    {
        // Implement confidence calculation logic
        var score = 0.8; // Default confidence

        // Add various confidence factors
        if (generatedSQL.Contains("SELECT", StringComparison.OrdinalIgnoreCase)) score += 0.1;
        if (generatedSQL.Contains("FROM", StringComparison.OrdinalIgnoreCase)) score += 0.1;
        if (!generatedSQL.Contains("ERROR", StringComparison.OrdinalIgnoreCase)) score += 0.1;

        return Task.FromResult(Math.Min(1.0, score));
    }

    public Task<string[]> GenerateQuerySuggestionsAsync(string context, SchemaMetadata schema)
    {
        // Get current date for recent queries
        var today = DateTime.Now;
        var yesterday = today.AddDays(-1).ToString("yyyy-MM-dd");
        var lastWeek = today.AddDays(-7).ToString("yyyy-MM-dd");
        var thisMonth = today.ToString("yyyy-MM");

        var suggestions = new List<string>
        {
            // Basic queries with correct field values
            $"Show me total deposits for yesterday ({yesterday})",
            $"Top 10 players by deposits in the last 7 days",
            $"Show me daily revenue for the last week",
            $"Count of active players yesterday",

            // Status-specific queries (will use actual database values)
            "Show me all blocked players from the last 7 days",
            "Count of active players by brand this month",
            "Show me players by status for the last 30 days",
            "List all active players with deposits today",

            // Payment method queries
            "Show me deposits by payment method for the last week",
            "Total CreditCard deposits vs Neteller deposits this month",
            "Show me players using Skrill payment method",

            // Game type queries
            "Show me revenue by game type (Slot vs Table vs Live) for last week",
            "Top 10 Slot games by revenue this month",
            "Show me Live casino performance vs Sports betting",

            // Country and currency analysis
            $"Revenue breakdown by country for last week",
            "Show me players by currency (USD, EUR, GBP) this month",

            // Bonus analysis
            "Show me active bonus balances by player",
            "Total expired bonuses this month",
            "Players with pending bonus status",

            // Gaming analytics
            $"Show me casino vs sports betting revenue for last week",
            $"Total bets and wins for {thisMonth}",
            $"Show me player activity for the last 3 days"
        };

        // Add schema-specific suggestions with recent dates
        foreach (var table in schema.Tables.Take(2))
        {
            if (table.Name.ToLower().Contains("daily") || table.Name.ToLower().Contains("action"))
            {
                suggestions.Add($"Show me top 20 records from {table.Name} for yesterday");
                if (table.Columns.Any(c => c.Name.ToLower().Contains("date")))
                {
                    suggestions.Add($"Show me {table.Name} data for the last 3 days");
                }
            }
        }

        return Task.FromResult(suggestions.Take(8).ToArray());
    }

    public Task<bool> ValidateQueryIntentAsync(string naturalLanguageQuery)
    {
        // Implement query intent validation
        var lowerQuery = naturalLanguageQuery.ToLowerInvariant();

        // Check for SQL injection attempts
        var dangerousPatterns = new[] { "drop", "delete", "truncate", "alter", "create", "insert", "update" };
        if (dangerousPatterns.Any(pattern => lowerQuery.Contains(pattern)))
        {
            return Task.FromResult(false);
        }

        // Check for valid query patterns
        var validPatterns = new[] { "show", "get", "find", "list", "count", "sum", "average", "total" };
        return Task.FromResult(validPatterns.Any(pattern => lowerQuery.Contains(pattern)));
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

        if (_promptService == null)
        {
            yield return new StreamingResponse { Content = "Prompt service not configured", IsComplete = true };
            yield break;
        }

        string enhancedPrompt;
        try
        {
            enhancedPrompt = await _promptService.BuildSQLGenerationPromptAsync(prompt, schema, context);
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

        if (_promptService == null)
        {
            yield return new StreamingResponse { Content = "Prompt service not configured", IsComplete = true };
            yield break;
        }

        string insightPrompt;
        try
        {
            insightPrompt = await _promptService.BuildInsightGenerationPromptAsync(query, data, context);
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

        if (_promptService == null)
        {
            yield return new StreamingResponse { Content = "Prompt service not configured", IsComplete = true };
            yield break;
        }

        string explanationPrompt;
        try
        {
            explanationPrompt = await _promptService.BuildSQLExplanationPromptAsync(sql, complexity);
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
        // Enhanced prompt with NOLOCK instructions
        return $@"Generate SQL for: {originalPrompt}

IMPORTANT REQUIREMENTS:
- Always add WITH (NOLOCK) hint to all table references for better read performance
- Correct format: FROM TableName alias WITH (NOLOCK) or FROM TableName WITH (NOLOCK)
- NEVER use: FROM TableName WITH (NOLOCK) AS alias (this causes syntax errors)
- Use only SELECT statements for reporting queries
- Include proper WHERE clauses for filtering
- Use meaningful column aliases

EXAMPLES:
✅ Correct: FROM tbl_Daily_actions dap WITH (NOLOCK)
✅ Correct: JOIN tbl_Players p WITH (NOLOCK) ON dap.PlayerID = p.PlayerID
❌ Wrong: FROM tbl_Daily_actions WITH (NOLOCK) AS dap";
    }

    private string CleanSQLResponse(string sql)
    {
        if (string.IsNullOrWhiteSpace(sql))
            return sql;

        // Remove markdown formatting
        sql = sql.Replace("```sql", "").Replace("```", "").Trim();

        // Remove common AI response prefixes
        var prefixes = new[] { "SQL:", "sql:", "Query:", "query:", "SELECT:", "select:" };
        foreach (var prefix in prefixes)
        {
            if (sql.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                sql = sql.Substring(prefix.Length).Trim();
                break;
            }
        }

        // Remove any leading/trailing whitespace and newlines
        sql = sql.Trim('\r', '\n', ' ', '\t');

        return sql;
    }

    private string GenerateDataPreview(object[] data)
    {
        if (data.Length == 0) return "No data available";

        var preview = data.Take(3).Select(item => item?.ToString() ?? "null").ToArray();
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
        if (_learningService == null)
        {
            _logger.LogDebug("Learning service not available, skipping feedback processing");
            return;
        }

        try
        {
            // Convert QueryFeedback to UserFeedback
            var userFeedback = new BIReportingCopilot.Core.Models.UserFeedback
            {
                UserId = userId,
                QueryId = feedback.QueryId,
                Rating = feedback.Feedback == "positive" ? 5 : feedback.Feedback == "negative" ? 1 : 3,
                Comments = feedback.Comments,
                FeedbackType = feedback.Feedback,
                Category = "General",
                ProvidedAt = DateTime.UtcNow,
                IsProcessed = false,
                Metadata = new Dictionary<string, object>
                {
                    ["originalPrompt"] = originalPrompt,
                    ["generatedSQL"] = generatedSQL,
                    ["suggestedImprovement"] = feedback.SuggestedImprovement ?? ""
                }
            };

            await _learningService.ProcessFeedbackAsync(userFeedback);
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
        if (_learningService == null)
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
            // Use the learning service to get insights and derive statistics
            var insights = await _learningService.GenerateLearningInsightsAsync();
            return new LearningStatistics
            {
                TotalGenerations = insights.SuccessfulPatterns.Count,
                TotalFeedbackItems = insights.CommonMistakes.Count,
                AverageRating = insights.PerformanceInsights.Values.Any() ? insights.PerformanceInsights.Values.Average() : 0.0,
                AverageConfidence = 0.8,
                UniqueUsers = 1, // Placeholder - would need additional tracking
                PopularPatterns = insights.SuccessfulPatterns.ToDictionary(p => p, p => 1),
                LastUpdated = insights.GeneratedAt
            };
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
