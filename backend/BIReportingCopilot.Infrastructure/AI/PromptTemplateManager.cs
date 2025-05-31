using BIReportingCopilot.Core.Interfaces;
using BIReportingCopilot.Core.Models;
using CoreModels = BIReportingCopilot.Core.Models;
using System.Text;
using System.Text.Json;

namespace BIReportingCopilot.Infrastructure.AI;

/// <summary>
/// Advanced prompt template manager with sophisticated prompt engineering
/// </summary>
public class PromptTemplateManager
{
    private readonly Dictionary<string, string> _templates;
    private readonly Dictionary<StreamingQueryComplexity, string> _complexityModifiers;

    public PromptTemplateManager()
    {
        _templates = InitializeTemplates();
        _complexityModifiers = InitializeComplexityModifiers();
    }

    public async Task<string> BuildSQLGenerationPromptAsync(
        string userPrompt,
        CoreModels.SchemaMetadata? schema = null,
        QueryContext? context = null)
    {
        var promptBuilder = new StringBuilder();

        // Add context-aware instructions
        promptBuilder.AppendLine(GetSQLSystemPrompt());
        promptBuilder.AppendLine();

        // Add schema information if available
        if (schema != null)
        {
            promptBuilder.AppendLine("## Database Schema:");
            promptBuilder.AppendLine(FormatSchemaForPrompt(schema));
            promptBuilder.AppendLine();
        }

        // Add user context and preferences
        if (context != null)
        {
            promptBuilder.AppendLine("## Context:");
            if (!string.IsNullOrEmpty(context.BusinessDomain))
            {
                promptBuilder.AppendLine($"Business Domain: {context.BusinessDomain}");
            }

            if (context.PreviousQueries.Any())
            {
                promptBuilder.AppendLine("Previous queries in this session:");
                foreach (var prevQuery in context.PreviousQueries.TakeLast(3))
                {
                    promptBuilder.AppendLine($"- {prevQuery}");
                }
            }

            promptBuilder.AppendLine($"Preferred complexity: {context.PreferredComplexity}");
            promptBuilder.AppendLine();
        }

        // Add complexity-specific instructions
        var complexity = context?.PreferredComplexity ?? StreamingQueryComplexity.Medium;
        if (_complexityModifiers.ContainsKey(complexity))
        {
            promptBuilder.AppendLine(_complexityModifiers[complexity]);
            promptBuilder.AppendLine();
        }

        // Add examples based on context
        promptBuilder.AppendLine("## Examples:");
        promptBuilder.AppendLine(GetRelevantExamples(userPrompt, schema));
        promptBuilder.AppendLine();

        // Add the user's request
        promptBuilder.AppendLine("## User Request:");
        promptBuilder.AppendLine(userPrompt);
        promptBuilder.AppendLine();

        // Add specific instructions for response format
        promptBuilder.AppendLine("## Response Instructions:");
        promptBuilder.AppendLine("- Return only the SQL query, no explanations");
        promptBuilder.AppendLine("- Use proper SQL formatting and indentation");
        promptBuilder.AppendLine("- Include comments for complex logic");
        promptBuilder.AppendLine("- Optimize for performance when possible");
        promptBuilder.AppendLine("- Use appropriate JOINs and WHERE clauses");
        promptBuilder.AppendLine("- Always add WITH (NOLOCK) hint to all table references for better read performance");
        promptBuilder.AppendLine("- Correct format: FROM TableName alias WITH (NOLOCK) - never use AS keyword with table hints");
        promptBuilder.AppendLine("- Example: SELECT * FROM TableName t WITH (NOLOCK) WHERE condition");

        return promptBuilder.ToString();
    }

    public async Task<string> BuildInsightGenerationPromptAsync(
        string query,
        object[] data,
        AnalysisContext? context = null)
    {
        var promptBuilder = new StringBuilder();

        promptBuilder.AppendLine(GetInsightSystemPrompt());
        promptBuilder.AppendLine();

        // Add analysis context
        if (context != null)
        {
            promptBuilder.AppendLine("## Analysis Context:");
            if (!string.IsNullOrEmpty(context.BusinessGoal))
            {
                promptBuilder.AppendLine($"Business Goal: {context.BusinessGoal}");
            }
            if (!string.IsNullOrEmpty(context.TimeFrame))
            {
                promptBuilder.AppendLine($"Time Frame: {context.TimeFrame}");
            }
            if (context.KeyMetrics.Any())
            {
                promptBuilder.AppendLine($"Key Metrics: {string.Join(", ", context.KeyMetrics)}");
            }
            promptBuilder.AppendLine($"Analysis Type: {context.Type}");
            promptBuilder.AppendLine();
        }

        // Add query information
        promptBuilder.AppendLine("## SQL Query:");
        promptBuilder.AppendLine($"```sql\n{query}\n```");
        promptBuilder.AppendLine();

        // Add data summary
        promptBuilder.AppendLine("## Data Summary:");
        promptBuilder.AppendLine($"Total records: {data.Length}");

        if (data.Length > 0)
        {
            var sampleData = data.Take(5).ToArray();
            promptBuilder.AppendLine("Sample data (first 5 rows):");
            promptBuilder.AppendLine(JsonSerializer.Serialize(sampleData, new JsonSerializerOptions
            {
                WriteIndented = true
            }));
        }
        promptBuilder.AppendLine();

        // Add instructions based on analysis type
        var analysisType = context?.Type ?? AnalysisType.Descriptive;
        promptBuilder.AppendLine("## Analysis Instructions:");
        promptBuilder.AppendLine(GetAnalysisInstructions(analysisType));

        return promptBuilder.ToString();
    }

    public async Task<string> BuildSQLExplanationPromptAsync(string sql, StreamingQueryComplexity complexity)
    {
        var promptBuilder = new StringBuilder();

        promptBuilder.AppendLine(GetExplanationSystemPrompt());
        promptBuilder.AppendLine();

        promptBuilder.AppendLine("## SQL Query to Explain:");
        promptBuilder.AppendLine($"```sql\n{sql}\n```");
        promptBuilder.AppendLine();

        promptBuilder.AppendLine("## Explanation Requirements:");
        promptBuilder.AppendLine(GetExplanationRequirements(complexity));

        return promptBuilder.ToString();
    }

    public string GetSQLSystemPrompt()
    {
        return @"You are an expert SQL developer and database architect specializing in business intelligence queries.
Your role is to generate optimized, secure, and efficient SQL queries based on natural language requests.

Key principles:
- Always prioritize data security and prevent SQL injection
- Generate performant queries with proper indexing considerations
- Use clear, readable SQL with appropriate formatting
- Include meaningful aliases and comments for complex logic
- Follow SQL best practices and standards
- Consider business context when interpreting requests
- Always use WITH (NOLOCK) hints on all table references for better read performance in reporting scenarios
- Format table hints as: FROM TableName alias WITH (NOLOCK) - never use AS keyword with table hints";
    }

    public string GetInsightSystemPrompt()
    {
        return @"You are a senior business intelligence analyst with expertise in data interpretation and business insights.
Your role is to analyze query results and provide actionable business insights.

Key principles:
- Focus on business value and actionable recommendations
- Identify trends, patterns, and anomalies in the data
- Provide context-aware analysis based on business goals
- Use clear, non-technical language for business stakeholders
- Highlight key metrics and their business implications
- Suggest follow-up questions or deeper analysis opportunities";
    }

    public string GetExplanationSystemPrompt()
    {
        return @"You are a database expert and technical educator specializing in SQL query explanation.
Your role is to break down complex SQL queries into understandable explanations.

Key principles:
- Explain queries step-by-step in logical order
- Use clear, educational language appropriate for the audience
- Highlight performance considerations and optimization opportunities
- Explain the business logic behind the query structure
- Identify potential issues or improvements
- Make complex concepts accessible to different skill levels";
    }

    private Dictionary<string, string> InitializeTemplates()
    {
        return new Dictionary<string, string>
        {
            ["basic_select"] = "SELECT {columns} FROM {table} WHERE {conditions}",
            ["join_query"] = "SELECT {columns} FROM {main_table} JOIN {join_table} ON {join_condition} WHERE {conditions}",
            ["aggregation"] = "SELECT {group_columns}, {aggregates} FROM {table} GROUP BY {group_columns} HAVING {having_conditions}",
            ["window_function"] = "SELECT {columns}, {window_function} OVER (PARTITION BY {partition} ORDER BY {order}) FROM {table}",
            ["cte_query"] = "WITH {cte_name} AS ({cte_query}) SELECT {columns} FROM {cte_name} WHERE {conditions}"
        };
    }

    private Dictionary<StreamingQueryComplexity, string> InitializeComplexityModifiers()
    {
        return new Dictionary<StreamingQueryComplexity, string>
        {
            [StreamingQueryComplexity.Simple] = @"
## Complexity Guidelines (Simple):
- Use basic SELECT, WHERE, and simple JOINs
- Avoid complex subqueries or window functions
- Keep the query straightforward and readable
- Focus on essential columns only",

            [StreamingQueryComplexity.Medium] = @"
## Complexity Guidelines (Medium):
- Use JOINs, subqueries, and basic aggregations
- Include GROUP BY and HAVING clauses when needed
- Use window functions for ranking and running totals
- Balance performance with functionality",

            [StreamingQueryComplexity.Complex] = @"
## Complexity Guidelines (Complex):
- Use advanced SQL features like CTEs, window functions, and complex subqueries
- Implement sophisticated business logic
- Use multiple levels of nesting when necessary
- Optimize for complex analytical requirements",

            [StreamingQueryComplexity.Expert] = @"
## Complexity Guidelines (Expert):
- Use the full range of SQL capabilities
- Implement advanced analytical functions and recursive CTEs
- Create highly optimized queries for large datasets
- Use database-specific features for maximum performance"
        };
    }

    private string FormatSchemaForPrompt(CoreModels.SchemaMetadata schema)
    {
        var schemaBuilder = new StringBuilder();

        foreach (var table in schema.Tables)
        {
            schemaBuilder.AppendLine($"### Table: {table.Name}");
            if (!string.IsNullOrEmpty(table.Description))
            {
                schemaBuilder.AppendLine($"Description: {table.Description}");
            }
            schemaBuilder.AppendLine($"Estimated rows: {table.RowCount:N0}");
            schemaBuilder.AppendLine("Columns:");

            foreach (var column in table.Columns)
            {
                var columnInfo = $"- {column.Name} ({column.DataType})";
                if (column.IsPrimaryKey) columnInfo += " [PK]";
                if (column.IsForeignKey) columnInfo += " [FK]";
                if (!column.IsNullable) columnInfo += " [NOT NULL]";
                if (!string.IsNullOrEmpty(column.Description))
                {
                    columnInfo += $" - {column.Description}";
                }
                schemaBuilder.AppendLine(columnInfo);
            }
            schemaBuilder.AppendLine();
        }

        return schemaBuilder.ToString();
    }

    private string GetRelevantExamples(string userPrompt, CoreModels.SchemaMetadata? schema)
    {
        // This would be enhanced with ML-based example selection in a full implementation
        return @"
Example 1 - Basic aggregation:
User: ""Show me total sales by month""
SQL: SELECT YEAR(order_date) as year, MONTH(order_date) as month, SUM(total_amount) as total_sales
     FROM orders
     GROUP BY YEAR(order_date), MONTH(order_date)
     ORDER BY year, month;

Example 2 - Join with filtering:
User: ""Find customers who placed orders in the last 30 days""
SQL: SELECT DISTINCT c.customer_name, c.email
     FROM customers c
     JOIN orders o ON c.customer_id = o.customer_id
     WHERE o.order_date >= DATEADD(day, -30, GETDATE());";
    }

    private string GetAnalysisInstructions(AnalysisType analysisType)
    {
        return analysisType switch
        {
            AnalysisType.Descriptive => @"
- Summarize what the data shows
- Identify key patterns and trends
- Highlight notable values (highest, lowest, averages)
- Describe the distribution of data",

            AnalysisType.Diagnostic => @"
- Explain why certain patterns exist
- Identify potential causes for anomalies
- Compare against expected values or benchmarks
- Suggest areas for deeper investigation",

            AnalysisType.Predictive => @"
- Identify trends that might continue
- Highlight leading indicators
- Suggest potential future scenarios
- Recommend monitoring strategies",

            AnalysisType.Prescriptive => @"
- Provide specific actionable recommendations
- Suggest optimization opportunities
- Recommend next steps or interventions
- Prioritize actions by potential impact",

            _ => "Provide comprehensive analysis covering key insights and recommendations."
        };
    }

    private string GetExplanationRequirements(StreamingQueryComplexity complexity)
    {
        return complexity switch
        {
            StreamingQueryComplexity.Simple => @"
- Explain in simple terms suitable for beginners
- Focus on basic SQL concepts
- Use analogies to make concepts clear
- Avoid technical jargon",

            StreamingQueryComplexity.Medium => @"
- Provide moderate technical detail
- Explain intermediate SQL concepts
- Include performance considerations
- Suitable for users with some SQL knowledge",

            StreamingQueryComplexity.Complex => @"
- Provide detailed technical explanation
- Cover advanced SQL concepts and optimizations
- Explain complex business logic
- Suitable for experienced SQL users",

            StreamingQueryComplexity.Expert => @"
- Provide comprehensive technical analysis
- Cover all optimization and design considerations
- Explain advanced database concepts
- Suitable for database professionals",

            _ => "Provide clear explanation appropriate for the query complexity."
        };
    }
}
