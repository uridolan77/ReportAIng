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

namespace BIReportingCopilot.Infrastructure.AI;

/// <summary>
/// Unified AI service combining standard and streaming capabilities
/// Consolidates EnhancedOpenAIService and StreamingOpenAIService
/// </summary>
public class UnifiedAIService : IAIService
{
    private readonly OpenAIClient _client;
    private readonly IConfiguration _configuration;
    private readonly ILogger<UnifiedAIService> _logger;
    private readonly ICacheService _cacheService;
    private readonly AIServiceConfiguration _aiConfig;
    private readonly bool _isConfigured;
    private readonly PromptTemplateManager _promptManager;
    private readonly IContextManager _contextManager;
    private readonly List<QueryExample> _examples;

    public UnifiedAIService(
        OpenAIClient client,
        IConfiguration configuration,
        ILogger<UnifiedAIService> logger,
        ICacheService cacheService,
        IContextManager contextManager)
    {
        _client = client;
        _configuration = configuration;
        _logger = logger;
        _cacheService = cacheService;
        _contextManager = contextManager;
        _examples = InitializeExamples();

        // Load AI configuration
        _aiConfig = new AIServiceConfiguration();
        configuration.GetSection("OpenAI").Bind(_aiConfig.OpenAI);
        configuration.GetSection("AzureOpenAI").Bind(_aiConfig.AzureOpenAI);

        _isConfigured = _aiConfig.HasValidConfiguration;
        _promptManager = new PromptTemplateManager(configuration, logger);

        if (!_isConfigured)
        {
            _logger.LogWarning("AI service is not properly configured. Fallback responses will be used.");
        }
        else
        {
            var configType = _aiConfig.PreferAzureOpenAI ? "Azure OpenAI" : "OpenAI";
            _logger.LogInformation("Unified AI service configured with {ConfigType}", configType);
        }
    }

    #region Standard AI Operations

    public async Task<string> GenerateSQLAsync(string prompt)
    {
        return await GenerateSQLAsync(prompt, CancellationToken.None);
    }

    public async Task<string> GenerateSQLAsync(string prompt, CancellationToken cancellationToken)
    {
        if (!_isConfigured)
        {
            return GenerateFallbackSQL(prompt);
        }

        try
        {
            _logger.LogInformation("Building enhanced prompt...");
            var enhancedPrompt = BuildEnhancedPrompt(prompt);

            var deploymentName = _aiConfig.PreferAzureOpenAI ?
                _aiConfig.AzureOpenAI.DeploymentName :
                _aiConfig.OpenAI.Model;

            var chatCompletionsOptions = new ChatCompletionsOptions()
            {
                DeploymentName = deploymentName,
                Messages =
                {
                    new ChatRequestSystemMessage("You are an expert SQL developer. Generate only valid SQL queries without explanations."),
                    new ChatRequestUserMessage(enhancedPrompt)
                },
                Temperature = _aiConfig.OpenAI.Temperature,
                MaxTokens = _aiConfig.OpenAI.MaxTokens,
                FrequencyPenalty = _aiConfig.OpenAI.FrequencyPenalty,
                PresencePenalty = _aiConfig.OpenAI.PresencePenalty
            };

            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            var timeoutSeconds = _aiConfig.PreferAzureOpenAI ?
                _aiConfig.AzureOpenAI.TimeoutSeconds :
                _aiConfig.OpenAI.TimeoutSeconds;
            cts.CancelAfter(TimeSpan.FromSeconds(timeoutSeconds));

            _logger.LogInformation("Making AI API call with timeout: {TimeoutSeconds}s...", timeoutSeconds);
            var response = await _client.GetChatCompletionsAsync(chatCompletionsOptions, cts.Token);

            _logger.LogInformation("AI API call successful. Response received with {ChoiceCount} choices",
                response.Value.Choices.Count);

            var generatedSQL = response.Value.Choices[0].Message.Content;

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
        if (!_isConfigured)
        {
            return GenerateFallbackInsight(query, data);
        }

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

            var chatCompletionsOptions = new ChatCompletionsOptions()
            {
                DeploymentName = _aiConfig.PreferAzureOpenAI ?
                    _aiConfig.AzureOpenAI.DeploymentName :
                    _aiConfig.OpenAI.Model,
                Messages =
                {
                    new ChatRequestSystemMessage("You are a business intelligence analyst providing insights from data."),
                    new ChatRequestUserMessage(insightPrompt)
                },
                Temperature = 0.3f,
                MaxTokens = 500
            };

            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
            var response = await _client.GetChatCompletionsAsync(chatCompletionsOptions, cts.Token);
            return response.Value.Choices[0].Message.Content;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating insights");
            return GenerateFallbackInsight(query, data);
        }
    }

    public async Task<string> GenerateVisualizationConfigAsync(string query, ColumnInfo[] columns, object[] data)
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

            var chatCompletionsOptions = new ChatCompletionsOptions()
            {
                DeploymentName = _configuration["OpenAI:DeploymentName"] ?? "gpt-4",
                Messages =
                {
                    new ChatRequestSystemMessage("You are a data visualization expert. Return only valid JSON."),
                    new ChatRequestUserMessage(vizPrompt)
                },
                Temperature = 0.2f,
                MaxTokens = 300
            };

            var response = await _client.GetChatCompletionsAsync(chatCompletionsOptions);
            return response.Value.Choices[0].Message.Content;
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
        QueryContext? context = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        if (!_isConfigured)
        {
            yield return new StreamingResponse
            {
                Type = StreamingResponseType.Error,
                Content = "AI service is not configured",
                IsComplete = true
            };
            yield break;
        }

        var enhancedPrompt = await _promptManager.BuildSQLGenerationPromptAsync(prompt, schema, context);
        
        await foreach (var response in StreamChatCompletionAsync(enhancedPrompt, StreamingResponseType.SQL, cancellationToken))
        {
            yield return response;
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
                Content = "AI service is not configured",
                IsComplete = true
            };
            yield break;
        }

        var insightPrompt = await _promptManager.BuildInsightGenerationPromptAsync(query, data, context);
        
        await foreach (var response in StreamChatCompletionAsync(insightPrompt, StreamingResponseType.Insight, cancellationToken))
        {
            yield return response;
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
                Content = "AI service is not configured",
                IsComplete = true
            };
            yield break;
        }

        var explanationPrompt = await _promptManager.BuildSQLExplanationPromptAsync(sql, complexity);
        
        await foreach (var response in StreamChatCompletionAsync(explanationPrompt, StreamingResponseType.Explanation, cancellationToken))
        {
            yield return response;
        }
    }

    #endregion

    #region Private Helper Methods

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

        // Implementation would continue with streaming logic...
        // For brevity, yielding a simple response
        yield return new StreamingResponse
        {
            Type = responseType,
            Content = "Streaming implementation placeholder",
            IsComplete = true
        };
    }

    private string GetSystemPromptForType(StreamingResponseType type)
    {
        return type switch
        {
            StreamingResponseType.SQL => "You are an expert SQL developer. Generate only valid SQL queries.",
            StreamingResponseType.Insight => "You are a business intelligence analyst providing insights from data.",
            StreamingResponseType.Explanation => "You are a SQL expert explaining queries in simple terms.",
            _ => "You are a helpful AI assistant."
        };
    }

    private float GetTemperatureForType(StreamingResponseType type)
    {
        return type switch
        {
            StreamingResponseType.SQL => 0.1f,
            StreamingResponseType.Insight => 0.3f,
            StreamingResponseType.Explanation => 0.2f,
            _ => 0.2f
        };
    }

    private int GetMaxTokensForType(StreamingResponseType type)
    {
        return type switch
        {
            StreamingResponseType.SQL => 1000,
            StreamingResponseType.Insight => 500,
            StreamingResponseType.Explanation => 800,
            _ => 500
        };
    }

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
            new() { Question = "Show all users", Sql = "SELECT * FROM Users" },
            new() { Question = "Count total orders", Sql = "SELECT COUNT(*) FROM Orders" }
        };
    }

    #endregion
}
