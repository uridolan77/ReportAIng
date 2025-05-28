using Azure.AI.OpenAI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using BIReportingCopilot.Core.Interfaces;
using BIReportingCopilot.Core.Models;
using BIReportingCopilot.Core.Configuration;
using System.Text.Json;
using System.Text;
using Azure;

namespace BIReportingCopilot.Infrastructure.AI;

public class EnhancedOpenAIService : IOpenAIService
{
    private readonly OpenAIClient _client;
    private readonly IConfiguration _configuration;
    private readonly ILogger<EnhancedOpenAIService> _logger;
    private readonly List<QueryExample> _examples;
    private readonly AIServiceConfiguration _aiConfig;
    private readonly bool _isConfigured;

    public EnhancedOpenAIService(
        OpenAIClient client,
        IConfiguration configuration,
        ILogger<EnhancedOpenAIService> logger)
    {
        _client = client;
        _configuration = configuration;
        _logger = logger;
        _examples = InitializeExamples();

        // Load AI configuration
        _aiConfig = new AIServiceConfiguration();
        configuration.GetSection("OpenAI").Bind(_aiConfig.OpenAI);
        configuration.GetSection("AzureOpenAI").Bind(_aiConfig.AzureOpenAI);

        _isConfigured = _aiConfig.HasValidConfiguration;

        if (!_isConfigured)
        {
            _logger.LogWarning("OpenAI service is not properly configured. Fallback responses will be used.");
        }
        else
        {
            var configType = _aiConfig.PreferAzureOpenAI ? "Azure OpenAI" : "OpenAI";
            _logger.LogInformation("OpenAI service configured with {ConfigType}", configType);
        }
    }

    public async Task<string> GenerateSQLAsync(string prompt)
    {
        return await GenerateSQLAsync(prompt, CancellationToken.None);
    }

    public async Task<string> GenerateSQLAsync(string prompt, CancellationToken cancellationToken)
    {
        _logger.LogInformation("GenerateSQLAsync called with prompt: '{Prompt}', IsConfigured: {IsConfigured}", prompt, _isConfigured);

        if (!_isConfigured)
        {
            _logger.LogWarning("OpenAI not configured, returning fallback SQL");
            return GenerateFallbackSQL(prompt);
        }

        try
        {
            _logger.LogInformation("Building enhanced prompt...");
            var enhancedPrompt = BuildEnhancedPrompt(prompt);

            var deploymentName = _aiConfig.PreferAzureOpenAI ?
                _aiConfig.AzureOpenAI.DeploymentName :
                _aiConfig.OpenAI.Model;

            _logger.LogInformation("OpenAI API call details - PreferAzureOpenAI: {PreferAzure}, DeploymentName: {DeploymentName}",
                _aiConfig.PreferAzureOpenAI, deploymentName);

            var chatCompletionsOptions = new ChatCompletionsOptions()
            {
                DeploymentName = deploymentName,
                Messages =
                {
                    new ChatRequestSystemMessage(GetSystemPrompt()),
                    new ChatRequestUserMessage(enhancedPrompt)
                },
                Temperature = _aiConfig.PreferAzureOpenAI ?
                    _aiConfig.AzureOpenAI.Temperature :
                    _aiConfig.OpenAI.Temperature,
                MaxTokens = _aiConfig.PreferAzureOpenAI ?
                    _aiConfig.AzureOpenAI.MaxTokens :
                    _aiConfig.OpenAI.MaxTokens,
                FrequencyPenalty = _aiConfig.OpenAI.FrequencyPenalty,
                PresencePenalty = _aiConfig.OpenAI.PresencePenalty
            };

            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            var timeoutSeconds = _aiConfig.PreferAzureOpenAI ?
                _aiConfig.AzureOpenAI.TimeoutSeconds :
                _aiConfig.OpenAI.TimeoutSeconds;
            cts.CancelAfter(TimeSpan.FromSeconds(timeoutSeconds));

            _logger.LogInformation("Making OpenAI API call with timeout: {TimeoutSeconds}s...", timeoutSeconds);
            var response = await _client.GetChatCompletionsAsync(chatCompletionsOptions, cts.Token);

            _logger.LogInformation("OpenAI API call successful. Response received with {ChoiceCount} choices",
                response.Value.Choices.Count);

            var sqlResult = response.Value.Choices[0].Message.Content;

            // Clean up the SQL result
            sqlResult = CleanSqlResult(sqlResult);

            _logger.LogInformation("Generated SQL for prompt: {PromptLength} chars -> {SqlLength} chars. SQL: {GeneratedSQL}",
                prompt.Length, sqlResult.Length, sqlResult);

            return sqlResult;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("SQL generation timed out for prompt: {Prompt}", prompt);
            return GenerateFallbackSQL(prompt);
        }
        catch (RequestFailedException ex)
        {
            _logger.LogError(ex, "Azure OpenAI request failed: {Error}, Status: {Status}, ErrorCode: {ErrorCode}",
                ex.Message, ex.Status, ex.ErrorCode);
            return GenerateFallbackSQL(prompt);
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

            var vizPrompt = $@"
Based on the following query and data structure, suggest the best visualization type:

Query: {query}
Columns: {columnInfo}
Row Count: {data.Length}

Return a JSON configuration for the visualization with:
- type: (bar, line, pie, table, scatter, etc.)
- title: descriptive title
- xAxis: column name for x-axis (if applicable)
- yAxis: column name for y-axis (if applicable)
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
        try
        {
            var score = 0.5; // Base confidence

            // Query complexity analysis
            var queryWords = naturalLanguageQuery.ToLowerInvariant().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var sqlWords = generatedSQL.ToLowerInvariant().Split(' ', StringSplitOptions.RemoveEmptyEntries);

            // Boost confidence for clear intent keywords
            var intentKeywords = new[] { "show", "get", "find", "count", "sum", "average", "total", "list" };
            if (queryWords.Any(w => intentKeywords.Contains(w)))
                score += 0.2;

            // Boost confidence for proper SQL structure
            var sqlKeywords = new[] { "select", "from", "where", "group", "order", "having" };
            var foundKeywords = sqlWords.Count(w => sqlKeywords.Contains(w));
            score += Math.Min(0.3, foundKeywords * 0.1);

            // Check for query complexity alignment
            if (IsComplexQuery(naturalLanguageQuery) && IsComplexSQL(generatedSQL))
                score += 0.1;
            else if (!IsComplexQuery(naturalLanguageQuery) && !IsComplexSQL(generatedSQL))
                score += 0.1;
            else
                score -= 0.1;

            // Check for keyword alignment
            if (HasKeywordAlignment(naturalLanguageQuery, generatedSQL))
                score += 0.1;

            // Reduce confidence for complex queries
            if (queryWords.Length > 15)
                score -= 0.1;

            // Boost confidence for joins (indicates relationship understanding)
            if (generatedSQL.ToLowerInvariant().Contains("join"))
                score += 0.1;

            // Reduce confidence for potential issues
            if (generatedSQL.Contains("*") && !naturalLanguageQuery.ToLowerInvariant().Contains("all"))
                score -= 0.05;

            // Validate SQL syntax (basic check)
            if (!IsValidSQLStructure(generatedSQL))
                score -= 0.4; // More severe penalty for invalid SQL

            // Additional checks for obviously invalid SQL
            var trimmedSql = generatedSQL.Trim().ToLowerInvariant();
            if (trimmedSql == "select from" || trimmedSql.StartsWith("select from"))
                score = 0.1; // Very low confidence for incomplete SQL

            if (trimmedSql.Contains("select  from") || trimmedSql.Contains("select\tfrom"))
                score = 0.1; // Very low confidence for missing columns

            // Check for completely invalid SQL statements
            if (trimmedSql.Contains("invalid") ||
                trimmedSql.Contains("error") ||
                !trimmedSql.Contains("select") ||
                (trimmedSql.Contains("select") && !trimmedSql.Contains("from")))
                score = 0.1; // Very low confidence for invalid SQL

            return Math.Max(0.1, Math.Min(1.0, score));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating confidence score");
            return 0.5; // Default moderate confidence
        }
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
        // Basic validation - check if it looks like a data query
        var queryWords = new[] { "show", "get", "find", "list", "count", "sum", "average", "total", "how many", "what" };
        var lowerQuery = naturalLanguageQuery.ToLowerInvariant();

        return queryWords.Any(word => lowerQuery.Contains(word));
    }

    private List<QueryExample> InitializeExamples()
    {
        return new List<QueryExample>
        {
            new("Show me sales by month",
                "SELECT MONTH(OrderDate) as Month, SUM(Total) as Sales FROM Orders GROUP BY MONTH(OrderDate) ORDER BY Month"),
            new("Count customers by country",
                "SELECT Country, COUNT(*) as CustomerCount FROM Customers GROUP BY Country ORDER BY CustomerCount DESC"),
            new("Top 10 products by revenue",
                "SELECT TOP 10 ProductName, SUM(Quantity * Price) as Revenue FROM OrderDetails od JOIN Products p ON od.ProductId = p.Id GROUP BY ProductName ORDER BY Revenue DESC"),
            new("Average order value last 30 days",
                "SELECT AVG(Total) as AverageOrderValue FROM Orders WHERE OrderDate >= DATEADD(day, -30, GETDATE())"),
            new("Monthly growth rate",
                "SELECT YEAR(OrderDate) as Year, MONTH(OrderDate) as Month, SUM(Total) as MonthlyTotal FROM Orders GROUP BY YEAR(OrderDate), MONTH(OrderDate) ORDER BY Year, Month")
        };
    }

    private string GetSystemPrompt()
    {
        return @"You are an expert SQL Server database analyst and business intelligence specialist with deep expertise in T-SQL.

CORE IDENTITY:
- Expert in SQL Server T-SQL syntax, functions, and optimization
- Specialized in business intelligence and analytics queries
- Focus on generating secure, efficient, and maintainable SQL

CRITICAL REQUIREMENTS:
1. SECURITY: Only generate SELECT statements - absolutely no DDL/DML operations
2. SYNTAX: Use proper SQL Server T-SQL syntax and built-in functions
3. PERFORMANCE: Include appropriate indexes hints and query optimization
4. FILTERING: Always include meaningful WHERE clauses for data filtering
5. RELATIONSHIPS: Use proper JOINs with explicit join conditions
6. SORTING: Add ORDER BY for logical result ordering
7. LIMITING: Use TOP N or OFFSET/FETCH for result pagination
8. FORMATTING: Return clean, well-formatted SQL without explanations

ADVANCED FEATURES TO LEVERAGE:
- Window functions (ROW_NUMBER, RANK, LAG, LEAD)
- Common Table Expressions (CTEs) for complex logic
- CASE statements for conditional logic
- Date/time functions (DATEADD, DATEDIFF, FORMAT)
- Aggregate functions with OVER clauses
- String functions (STRING_AGG, CONCAT, SUBSTRING)
- JSON functions when appropriate

BUSINESS INTELLIGENCE FOCUS:
- Prioritize analytical queries (aggregations, trends, comparisons)
- Include time-based analysis when dates are involved
- Group data meaningfully for reporting
- Calculate percentages, growth rates, and KPIs
- Consider data quality and null handling

RESPONSE FORMAT:
- Return ONLY the SQL query
- No markdown formatting or explanations
- Ensure query is executable and syntactically correct
- Use consistent indentation and formatting

EXAMPLES:
" + string.Join("\n", _examples.Select(e => $"Human: {e.Question}\nSQL: {e.Sql}\n"));
    }

    private string BuildEnhancedPrompt(string originalPrompt)
    {
        var promptBuilder = new StringBuilder();

        // Analyze the query intent
        var queryIntent = AnalyzeQueryIntent(originalPrompt);
        var queryComplexity = DetermineQueryComplexity(originalPrompt);

        // Add context-aware instructions
        promptBuilder.AppendLine("QUERY ANALYSIS:");
        promptBuilder.AppendLine($"Intent: {queryIntent}");
        promptBuilder.AppendLine($"Complexity: {queryComplexity}");
        promptBuilder.AppendLine();

        // Add the original prompt
        promptBuilder.AppendLine("USER REQUEST:");
        promptBuilder.AppendLine(originalPrompt);
        promptBuilder.AppendLine();

        // Add specific guidance based on intent
        promptBuilder.AppendLine("SPECIFIC GUIDANCE:");
        switch (queryIntent)
        {
            case QueryIntent.Aggregation:
                promptBuilder.AppendLine("- Use appropriate aggregate functions (SUM, COUNT, AVG, MIN, MAX)");
                promptBuilder.AppendLine("- Include GROUP BY for meaningful groupings");
                promptBuilder.AppendLine("- Consider HAVING clause for aggregate filtering");
                break;
            case QueryIntent.Trend:
                promptBuilder.AppendLine("- Include time-based grouping (YEAR, MONTH, QUARTER)");
                promptBuilder.AppendLine("- Use window functions for running totals or comparisons");
                promptBuilder.AppendLine("- Order by time periods for trend visualization");
                break;
            case QueryIntent.Comparison:
                promptBuilder.AppendLine("- Use CASE statements for conditional logic");
                promptBuilder.AppendLine("- Consider RANK() or ROW_NUMBER() for rankings");
                promptBuilder.AppendLine("- Include percentage calculations where relevant");
                break;
            case QueryIntent.Filtering:
                promptBuilder.AppendLine("- Use precise WHERE clauses");
                promptBuilder.AppendLine("- Consider date ranges with DATEADD/DATEDIFF");
                promptBuilder.AppendLine("- Use IN, LIKE, or BETWEEN operators as appropriate");
                break;
            default:
                promptBuilder.AppendLine("- Focus on clear, readable query structure");
                promptBuilder.AppendLine("- Include appropriate filtering and sorting");
                break;
        }
        promptBuilder.AppendLine();

        // Find and include relevant example
        var relevantExample = FindMostRelevantExample(originalPrompt);
        if (relevantExample != null)
        {
            promptBuilder.AppendLine("SIMILAR EXAMPLE:");
            promptBuilder.AppendLine($"Human: {relevantExample.Question}");
            promptBuilder.AppendLine($"SQL: {relevantExample.Sql}");
            promptBuilder.AppendLine();
        }

        // Add performance considerations
        if (queryComplexity == QueryComplexity.High)
        {
            promptBuilder.AppendLine("PERFORMANCE CONSIDERATIONS:");
            promptBuilder.AppendLine("- Use CTEs for complex subqueries");
            promptBuilder.AppendLine("- Consider indexing implications");
            promptBuilder.AppendLine("- Limit result sets with TOP or WHERE clauses");
            promptBuilder.AppendLine();
        }

        return promptBuilder.ToString();
    }

    private QueryExample? FindMostRelevantExample(string query)
    {
        var queryWords = query.ToLowerInvariant().Split(' ', StringSplitOptions.RemoveEmptyEntries);

        return _examples
            .Select(example => new {
                Example = example,
                Score = CalculateRelevanceScore(queryWords, example.Question.ToLowerInvariant())
            })
            .Where(x => x.Score > 0)
            .OrderByDescending(x => x.Score)
            .FirstOrDefault()?.Example;
    }

    private int CalculateRelevanceScore(string[] queryWords, string exampleQuestion)
    {
        var exampleWords = exampleQuestion.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return queryWords.Count(word => exampleWords.Contains(word));
    }

    private string CleanSqlResult(string sql)
    {
        // Remove markdown formatting if present
        sql = sql.Replace("```sql", "").Replace("```", "").Trim();

        // Remove any explanatory text before or after the SQL
        var lines = sql.Split('\n');
        var sqlLines = lines.Where(line =>
            !line.TrimStart().StartsWith("--") &&
            !string.IsNullOrWhiteSpace(line) &&
            !line.ToLowerInvariant().Contains("explanation") &&
            !line.ToLowerInvariant().Contains("this query")).ToList();

        return string.Join("\n", sqlLines).Trim();
    }

    private string GenerateDataPreview(object[] data)
    {
        if (data.Length == 0) return "No data";

        var preview = data.Take(3).Select((item, index) => $"Row {index + 1}: {item}");
        return string.Join("\n", preview) + (data.Length > 3 ? $"\n... and {data.Length - 3} more rows" : "");
    }

    private bool IsComplexQuery(string query)
    {
        var complexWords = new[] { "join", "group", "aggregate", "sum", "count", "average", "multiple", "compare" };
        return complexWords.Any(word => query.ToLowerInvariant().Contains(word));
    }

    private bool IsComplexSQL(string sql)
    {
        var complexKeywords = new[] { "JOIN", "GROUP BY", "HAVING", "UNION", "SUBQUERY", "CTE" };
        return complexKeywords.Any(keyword => sql.ToUpperInvariant().Contains(keyword));
    }

    private bool HasKeywordAlignment(string query, string sql)
    {
        var queryKeywords = ExtractKeywords(query.ToLowerInvariant());
        var sqlKeywords = ExtractKeywords(sql.ToLowerInvariant());

        return queryKeywords.Intersect(sqlKeywords).Any();
    }

    private string[] ExtractKeywords(string text)
    {
        var keywords = new[] { "sum", "count", "average", "total", "group", "order", "top", "recent", "last", "first" };
        return keywords.Where(keyword => text.Contains(keyword)).ToArray();
    }

    private bool IsValidSQLStructure(string sql)
    {
        try
        {
            var trimmedSql = sql.Trim().ToLowerInvariant();

            // Must start with SELECT
            if (!trimmedSql.StartsWith("select"))
                return false;

            // Must contain FROM
            if (!trimmedSql.Contains("from"))
                return false;

            // Check for balanced parentheses
            var openParens = sql.Count(c => c == '(');
            var closeParens = sql.Count(c => c == ')');
            if (openParens != closeParens)
                return false;

            // Check for dangerous keywords (should not be present in SELECT queries)
            var dangerousKeywords = new[] { "drop", "delete", "insert", "update", "alter", "create", "truncate" };
            if (dangerousKeywords.Any(keyword => trimmedSql.Contains(keyword)))
                return false;

            // Basic structure validation - must have columns between SELECT and FROM
            var selectFromPattern = @"select\s+(.+?)\s+from\s+(\w+)";
            var match = System.Text.RegularExpressions.Regex.Match(trimmedSql, selectFromPattern,
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);

            if (!match.Success)
                return false;

            // Check if there are actual columns specified (not just whitespace)
            var columns = match.Groups[1].Value.Trim();
            if (string.IsNullOrWhiteSpace(columns))
                return false;

            return true;
        }
        catch
        {
            return false;
        }
    }

    private string GenerateFallbackSQL(string prompt)
    {
        _logger.LogInformation("Generating fallback SQL for prompt: {Prompt}", prompt);

        var lowerPrompt = prompt.ToLowerInvariant();

        if (lowerPrompt.Contains("revenue") || lowerPrompt.Contains("sales") || lowerPrompt.Contains("total"))
        {
            return @"SELECT
                        YEAR(OrderDate) as Year,
                        MONTH(OrderDate) as Month,
                        SUM(TotalAmount) as Revenue,
                        COUNT(*) as OrderCount
                     FROM Orders
                     WHERE OrderDate >= DATEADD(YEAR, -2, GETDATE())
                     GROUP BY YEAR(OrderDate), MONTH(OrderDate)
                     ORDER BY Year DESC, Month DESC";
        }

        if (lowerPrompt.Contains("customer") || lowerPrompt.Contains("user"))
        {
            // Check if the prompt asks for country-specific data
            if (lowerPrompt.Contains("country") || lowerPrompt.Contains("by country"))
            {
                return @"SELECT
                            Country,
                            COUNT(*) as CustomerCount
                         FROM Customers
                         GROUP BY Country
                         ORDER BY CustomerCount DESC";
            }

            return @"SELECT
                        COUNT(DISTINCT CustomerId) as TotalCustomers,
                        COUNT(CASE WHEN LastOrderDate >= DATEADD(DAY, -30, GETDATE()) THEN 1 END) as ActiveCustomers,
                        AVG(DATEDIFF(DAY, FirstOrderDate, LastOrderDate)) as AvgCustomerLifespanDays
                     FROM Customers";
        }

        if (lowerPrompt.Contains("product") || lowerPrompt.Contains("item"))
        {
            return @"SELECT
                        p.ProductName,
                        p.Category,
                        SUM(oi.Quantity) as TotalSold,
                        SUM(oi.Quantity * oi.UnitPrice) as TotalRevenue
                     FROM Products p
                     JOIN OrderItems oi ON p.ProductId = oi.ProductId
                     JOIN Orders o ON oi.OrderId = o.OrderId
                     WHERE o.OrderDate >= DATEADD(MONTH, -6, GETDATE())
                     GROUP BY p.ProductId, p.ProductName, p.Category
                     ORDER BY TotalRevenue DESC";
        }

        if (lowerPrompt.Contains("top") || lowerPrompt.Contains("best") || lowerPrompt.Contains("highest"))
        {
            return @"SELECT TOP 10
                        ProductName,
                        SUM(Quantity * UnitPrice) as Revenue
                     FROM OrderItems oi
                     JOIN Products p ON oi.ProductId = p.ProductId
                     JOIN Orders o ON oi.OrderId = o.OrderId
                     WHERE o.OrderDate >= DATEADD(MONTH, -3, GETDATE())
                     GROUP BY p.ProductId, ProductName
                     ORDER BY Revenue DESC";
        }

        // Default fallback query
        return @"SELECT
                    'Sample Data' as DataType,
                    COUNT(*) as RecordCount,
                    GETDATE() as QueryTime
                 FROM (
                    SELECT 1 as SampleRecord
                    UNION ALL SELECT 2
                    UNION ALL SELECT 3
                    UNION ALL SELECT 4
                    UNION ALL SELECT 5
                 ) as SampleData";
    }

    private string GenerateFallbackInsight(string query, object[] data)
    {
        var rowCount = data.Length;
        var insights = new List<string>();

        if (rowCount == 0)
        {
            insights.Add("• No data was returned by this query. Consider adjusting your filters or date ranges.");
        }
        else if (rowCount == 1)
        {
            insights.Add("• This query returned a single result, which may indicate a specific lookup or aggregated metric.");
        }
        else if (rowCount > 100)
        {
            insights.Add($"• Large dataset returned ({rowCount} rows). Consider adding filters to focus on specific segments.");
        }
        else
        {
            insights.Add($"• Query returned {rowCount} rows of data, which is a manageable dataset for analysis.");
        }

        // Add query-specific insights
        var lowerQuery = query.ToLowerInvariant();
        if (lowerQuery.Contains("group by"))
        {
            insights.Add("• This appears to be an aggregated view. Look for patterns in the grouping categories.");
        }

        if (lowerQuery.Contains("order by"))
        {
            insights.Add("• Results are sorted, making it easy to identify top or bottom performers.");
        }

        if (lowerQuery.Contains("date"))
        {
            insights.Add("• Time-based analysis detected. Consider trends over different time periods.");
        }

        insights.Add("• For deeper insights, consider connecting to OpenAI services in your configuration.");

        return string.Join("\n", insights);
    }

    private QueryIntent AnalyzeQueryIntent(string prompt)
    {
        var lowerPrompt = prompt.ToLowerInvariant();

        // Check for aggregation keywords
        if (lowerPrompt.Contains("sum") || lowerPrompt.Contains("total") || lowerPrompt.Contains("count") ||
            lowerPrompt.Contains("average") || lowerPrompt.Contains("avg") || lowerPrompt.Contains("max") ||
            lowerPrompt.Contains("min") || lowerPrompt.Contains("group"))
        {
            return QueryIntent.Aggregation;
        }

        // Check for trend keywords
        if (lowerPrompt.Contains("trend") || lowerPrompt.Contains("over time") || lowerPrompt.Contains("monthly") ||
            lowerPrompt.Contains("yearly") || lowerPrompt.Contains("growth") || lowerPrompt.Contains("change"))
        {
            return QueryIntent.Trend;
        }

        // Check for comparison keywords
        if (lowerPrompt.Contains("compare") || lowerPrompt.Contains("vs") || lowerPrompt.Contains("versus") ||
            lowerPrompt.Contains("top") || lowerPrompt.Contains("best") || lowerPrompt.Contains("worst") ||
            lowerPrompt.Contains("rank") || lowerPrompt.Contains("highest") || lowerPrompt.Contains("lowest"))
        {
            return QueryIntent.Comparison;
        }

        // Check for filtering keywords
        if (lowerPrompt.Contains("where") || lowerPrompt.Contains("filter") || lowerPrompt.Contains("only") ||
            lowerPrompt.Contains("specific") || lowerPrompt.Contains("between") || lowerPrompt.Contains("since"))
        {
            return QueryIntent.Filtering;
        }

        return QueryIntent.General;
    }

    private QueryComplexity DetermineQueryComplexity(string prompt)
    {
        var lowerPrompt = prompt.ToLowerInvariant();
        var complexityScore = 0;

        // Check for complex operations
        if (lowerPrompt.Contains("join") || lowerPrompt.Contains("multiple tables")) complexityScore += 2;
        if (lowerPrompt.Contains("subquery") || lowerPrompt.Contains("nested")) complexityScore += 2;
        if (lowerPrompt.Contains("window function") || lowerPrompt.Contains("running total")) complexityScore += 2;
        if (lowerPrompt.Contains("pivot") || lowerPrompt.Contains("unpivot")) complexityScore += 2;
        if (lowerPrompt.Contains("recursive") || lowerPrompt.Contains("cte")) complexityScore += 2;

        // Check for multiple conditions
        if (lowerPrompt.Contains("and") || lowerPrompt.Contains("or")) complexityScore += 1;
        if (lowerPrompt.Contains("group by") && lowerPrompt.Contains("having")) complexityScore += 1;
        if (lowerPrompt.Contains("order by")) complexityScore += 1;

        // Check for advanced functions
        if (lowerPrompt.Contains("case when") || lowerPrompt.Contains("conditional")) complexityScore += 1;
        if (lowerPrompt.Contains("date") && (lowerPrompt.Contains("calculation") || lowerPrompt.Contains("difference"))) complexityScore += 1;

        return complexityScore switch
        {
            >= 4 => QueryComplexity.High,
            >= 2 => QueryComplexity.Medium,
            _ => QueryComplexity.Low
        };
    }
}

public class QueryExample
{
    public string Question { get; }
    public string Sql { get; }

    public QueryExample(string question, string sql)
    {
        Question = question;
        Sql = sql;
    }
}
