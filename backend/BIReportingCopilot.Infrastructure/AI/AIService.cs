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
using CoreModels = BIReportingCopilot.Core.Models;

namespace BIReportingCopilot.Infrastructure.AI;

/// <summary>
/// AI service combining standard and streaming capabilities using provider pattern
/// Consolidates EnhancedOpenAIService and StreamingOpenAIService with improved architecture
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

    public AIService(
        IAIProviderFactory providerFactory,
        ILogger<AIService> logger,
        ICacheService cacheService,
        IContextManager contextManager)
    {
        _providerFactory = providerFactory;
        _logger = logger;
        _cacheService = cacheService;
        _contextManager = contextManager;
        _examples = InitializeExamples();
        _promptManager = new PromptTemplateManager();

        // Get the appropriate provider
        _provider = _providerFactory.GetProvider();

        _logger.LogInformation("AI service initialized with provider: {ProviderName}", _provider.ProviderName);
    }

    #region Standard AI Operations

    public async Task<string> GenerateSQLAsync(string prompt)
    {
        return await GenerateSQLAsync(prompt, CancellationToken.None);
    }

    public async Task<string> GenerateSQLAsync(string prompt, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Building enhanced prompt...");
            var enhancedPrompt = BuildEnhancedPrompt(prompt);

            var options = new AIOptions
            {
                SystemMessage = "You are an expert SQL developer. Generate only valid SQL queries without explanations.",
                Temperature = 0.1f,
                MaxTokens = 2000,
                FrequencyPenalty = 0.0f,
                PresencePenalty = 0.0f,
                TimeoutSeconds = 30
            };

            var generatedSQL = await _provider.GenerateCompletionAsync(enhancedPrompt, options, cancellationToken);

            // Clean up the response
            generatedSQL = CleanSQLResponse(generatedSQL);

            _logger.LogInformation("Generated SQL: {SQL}", generatedSQL);
            return generatedSQL;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating SQL from prompt: {ExceptionType} - {Message}",
                ex.GetType().Name, ex.Message);
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
        var enhancedPrompt = await _promptManager.BuildSQLGenerationPromptAsync(prompt, schema, context);

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
        var insightPrompt = await _promptManager.BuildInsightGenerationPromptAsync(query, data, context);

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
        var explanationPrompt = await _promptManager.BuildSQLExplanationPromptAsync(sql, complexity);

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

    #endregion
}
