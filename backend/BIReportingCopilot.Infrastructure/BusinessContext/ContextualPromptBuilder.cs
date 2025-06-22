using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;
using BIReportingCopilot.Core.Interfaces.BusinessContext;
using BIReportingCopilot.Core.Interfaces.AI;
using BIReportingCopilot.Core.Models.BusinessContext;
using BIReportingCopilot.Core.Models.PromptGeneration;
using BIReportingCopilot.Core.Models;

namespace BIReportingCopilot.Infrastructure.BusinessContext;

/// <summary>
/// Service for building contextually-aware prompts for LLM consumption
/// </summary>
public class ContextualPromptBuilder : IContextualPromptBuilder
{
    private readonly IPromptService _promptService;
    private readonly ILogger<ContextualPromptBuilder> _logger;
    private readonly IVectorEmbeddingService _vectorService;

    public ContextualPromptBuilder(
        IPromptService promptService,
        ILogger<ContextualPromptBuilder> logger,
        IVectorEmbeddingService vectorService)
    {
        _promptService = promptService;
        _logger = logger;
        _vectorService = vectorService;
    }

    public async Task<string> BuildBusinessAwarePromptAsync(
        string userQuestion, 
        BusinessContextProfile profile, 
        ContextualBusinessSchema schema)
    {
        _logger.LogInformation("Building business-aware prompt for intent: {Intent}", profile.Intent.Type);

        // Select optimal template
        var template = await SelectOptimalTemplateAsync(profile);

        // Build context sections
        var businessContext = BuildBusinessContextSection(profile);
        var schemaContext = BuildSchemaContextSection(schema);
        var examplesContext = await BuildExamplesContextAsync(profile);
        var rulesContext = BuildBusinessRulesSection(schema.BusinessRules);

        // Build the complete prompt
        var promptBuilder = new StringBuilder();

        // System context
        promptBuilder.AppendLine("You are an expert business intelligence analyst with deep knowledge of SQL and business data analysis.");
        promptBuilder.AppendLine();

        // Business context
        promptBuilder.AppendLine("## Business Context");
        promptBuilder.AppendLine(businessContext);
        promptBuilder.AppendLine();

        // Schema context
        promptBuilder.AppendLine("## Database Schema Context");
        promptBuilder.AppendLine(schemaContext);
        promptBuilder.AppendLine();

        // Business rules
        if (schema.BusinessRules.Any())
        {
            promptBuilder.AppendLine("## Business Rules");
            promptBuilder.AppendLine(rulesContext);
            promptBuilder.AppendLine();
        }

        // Examples
        if (!string.IsNullOrEmpty(examplesContext))
        {
            promptBuilder.AppendLine("## Similar Query Examples");
            promptBuilder.AppendLine(examplesContext);
            promptBuilder.AppendLine();
        }

        // Task instruction
        promptBuilder.AppendLine("## Task");
        promptBuilder.AppendLine($"Convert the following business question into a SQL query:");
        promptBuilder.AppendLine($"**Question:** {userQuestion}");
        promptBuilder.AppendLine();

        // Intent-specific instructions
        promptBuilder.AppendLine(GetIntentSpecificInstructions(profile.Intent));
        promptBuilder.AppendLine();

        // Quality requirements
        promptBuilder.AppendLine("## Requirements");
        promptBuilder.AppendLine("- Generate syntactically correct SQL");
        promptBuilder.AppendLine("- Use appropriate table and column names from the schema");
        promptBuilder.AppendLine("- Include meaningful column aliases");
        promptBuilder.AppendLine("- Optimize for performance where possible");
        promptBuilder.AppendLine("- Follow SQL best practices");

        if (profile.TimeContext != null)
        {
            promptBuilder.AppendLine($"- Apply time filtering based on: {profile.TimeContext.RelativeExpression}");
        }

        promptBuilder.AppendLine();
        promptBuilder.AppendLine("Provide only the SQL query without additional explanation.");

        return promptBuilder.ToString();
    }

    public async Task<PromptTemplate> SelectOptimalTemplateAsync(BusinessContextProfile profile)
    {
        try
        {
            _logger.LogDebug("Selecting optimal template for intent: {Intent}", profile.Intent.Type);

            // Use vector embeddings to find the most relevant template
            var intentTypeString = profile.Intent.Type.ToString();
            var relevantTemplates = await _vectorService.FindRelevantPromptTemplatesAsync(
                profile.OriginalQuestion,
                intentTypeString,
                topK: 3);

            if (relevantTemplates.Any())
            {
                var bestTemplate = relevantTemplates.First();
                _logger.LogDebug("Selected template: {TemplateKey} with relevance score: {Score}",
                    bestTemplate.TemplateKey, bestTemplate.RelevanceScore);

                return new PromptTemplate
                {
                    Name = bestTemplate.Name,
                    Content = bestTemplate.Content,
                    Category = bestTemplate.IntentType,
                    TemplateKey = bestTemplate.TemplateKey,
                    Priority = bestTemplate.Priority
                };
            }

            // Fallback to intent-based selection
            var templateCategory = profile.Intent.Type switch
            {
                IntentType.Analytical => "bcapb_analytical_template",
                IntentType.Operational => "bcapb_operational_efficiency_template",
                IntentType.Exploratory => "bcapb_player_behavior_template",
                IntentType.Comparison => "bcapb_comparison_template",
                IntentType.Aggregation => "bcapb_gaming_revenue_template",
                IntentType.Trend => "bcapb_trend_template",
                IntentType.Detail => "bcapb_performance_monitoring_template",
                _ => "bcapb_analytical_template"
            };

            _logger.LogDebug("Using fallback template category: {Category}", templateCategory);
            return await GetDefaultTemplateAsync();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error selecting optimal template, using default");
            return await GetDefaultTemplateAsync();
        }
    }

    public async Task<string> EnrichPromptWithBusinessContextAsync(
        string basePrompt, 
        ContextualBusinessSchema schema)
    {
        var enrichedPrompt = new StringBuilder(basePrompt);

        // Add business context enrichment
        if (schema.RelevantTables.Any())
        {
            enrichedPrompt.AppendLine();
            enrichedPrompt.AppendLine("## Additional Business Context");
            
            foreach (var table in schema.RelevantTables)
            {
                enrichedPrompt.AppendLine($"- **{table.TableName}**: {table.BusinessPurpose}");
            }
        }

        // Add glossary context
        if (schema.RelevantGlossaryTerms.Any())
        {
            enrichedPrompt.AppendLine();
            enrichedPrompt.AppendLine("## Business Glossary");
            
            foreach (var term in schema.RelevantGlossaryTerms)
            {
                enrichedPrompt.AppendLine($"- **{term.Term}**: {term.Definition}");
            }
        }

        return enrichedPrompt.ToString();
    }

    public async Task<List<BIReportingCopilot.Core.Models.PromptGeneration.QueryExample>> FindRelevantExamplesAsync(
        BusinessContextProfile profile,
        int maxExamples = 3)
    {
        try
        {
            _logger.LogDebug("Finding relevant examples for query: {Query}", profile.OriginalQuestion);

            // Use vector embeddings to find semantically similar query examples
            var relevantExamples = await _vectorService.FindRelevantQueryExamplesAsync(
                profile.OriginalQuestion,
                profile.Domain.Name,
                maxExamples);

            if (relevantExamples.Any())
            {
                _logger.LogDebug("Found {Count} relevant examples using vector search", relevantExamples.Count);

                return relevantExamples.Select(example => new BIReportingCopilot.Core.Models.PromptGeneration.QueryExample
                {
                    NaturalLanguageQuery = example.NaturalLanguageQuery,
                    GeneratedSql = example.GeneratedSql,
                    BusinessContext = example.BusinessContext,
                    IntentType = example.IntentType,
                    Domain = example.Domain,
                    SuccessRate = (double)example.SuccessRate,
                    RelevanceScore = example.RelevanceScore
                }).ToList();
            }

            // Fallback to intent-based examples
            _logger.LogDebug("No vector examples found, using fallback examples for intent: {Intent}", profile.Intent.Type);
            return profile.Intent.Type switch
            {
                IntentType.Aggregation => GetAggregationExamples(),
                IntentType.Trend => GetTrendExamples(),
                IntentType.Comparison => GetComparisonExamples(),
                _ => new List<BIReportingCopilot.Core.Models.PromptGeneration.QueryExample>()
            };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error finding relevant examples, using fallback");
            return GetFallbackExamples(profile.Intent.Type);
        }
    }

    // Helper methods
    private string BuildBusinessContextSection(BusinessContextProfile profile)
    {
        var context = new StringBuilder();

        context.AppendLine($"**Query Intent:** {profile.Intent.Type} - {profile.Intent.Description}");
        context.AppendLine($"**Confidence Score:** {profile.Intent.ConfidenceScore:P1}");
        context.AppendLine($"**Business Domain:** {profile.Domain.Name}");

        if (profile.IdentifiedMetrics.Any())
        {
            context.AppendLine($"**Key Metrics:** {string.Join(", ", profile.IdentifiedMetrics)}");
        }

        if (profile.IdentifiedDimensions.Any())
        {
            context.AppendLine($"**Dimensions:** {string.Join(", ", profile.IdentifiedDimensions)}");
        }

        if (profile.TimeContext != null)
        {
            context.AppendLine($"**Time Context:** {profile.TimeContext.RelativeExpression} (Granularity: {profile.TimeContext.Granularity})");
        }

        if (profile.BusinessTerms.Any())
        {
            context.AppendLine($"**Business Terms:** {string.Join(", ", profile.BusinessTerms)}");
        }

        if (profile.Entities.Any())
        {
            var entitySummary = profile.Entities
                .GroupBy(e => e.Type)
                .Select(g => $"{g.Key}: {string.Join(", ", g.Select(e => e.Name))}")
                .ToList();
            context.AppendLine($"**Identified Entities:** {string.Join("; ", entitySummary)}");
        }

        if (profile.ComparisonTerms.Any())
        {
            context.AppendLine($"**Comparison Terms:** {string.Join(", ", profile.ComparisonTerms)}");
        }

        return context.ToString();
    }

    private string BuildSchemaContextSection(ContextualBusinessSchema schema)
    {
        var context = new StringBuilder();
        
        foreach (var table in schema.RelevantTables)
        {
            context.AppendLine($"### {table.SchemaName}.{table.TableName}");
            context.AppendLine($"**Purpose:** {table.BusinessPurpose}");
            
            if (schema.TableColumns.TryGetValue(table.Id, out var columns))
            {
                context.AppendLine("**Key Columns:**");
                foreach (var column in columns.Take(10)) // Limit to avoid prompt bloat
                {
                    var keyIndicator = column.IsKeyColumn ? " (PK)" : "";
                    context.AppendLine($"- `{column.ColumnName}` ({column.DataType}){keyIndicator}: {column.BusinessMeaning}");
                }
            }
            context.AppendLine();
        }

        return context.ToString();
    }

    private async Task<string> BuildExamplesContextAsync(BusinessContextProfile profile)
    {
        var examples = await FindRelevantExamplesAsync(profile, 2);
        if (!examples.Any()) return string.Empty;

        var context = new StringBuilder();
        
        foreach (var example in examples)
        {
            context.AppendLine($"**Question:** {example.NaturalLanguageQuery}");
            context.AppendLine($"**SQL:** {example.GeneratedSql}");
            context.AppendLine();
        }

        return context.ToString();
    }

    private string BuildBusinessRulesSection(List<BusinessRule> rules)
    {
        if (!rules.Any()) return string.Empty;

        var context = new StringBuilder();
        
        foreach (var rule in rules)
        {
            context.AppendLine($"- **{rule.Type}:** {rule.Description}");
        }

        return context.ToString();
    }

    private string GetIntentSpecificInstructions(BIReportingCopilot.Core.Models.BusinessContext.QueryIntent intent)
    {
        return intent.Type switch
        {
            IntentType.Analytical => 
                "Focus on complex analysis with appropriate aggregations, grouping, and calculations.",
            
            IntentType.Aggregation => 
                "Use GROUP BY clauses and aggregate functions (SUM, COUNT, AVG, etc.) as appropriate.",
            
            IntentType.Trend => 
                "Include time-based ordering and consider using window functions for trend analysis.",
            
            IntentType.Comparison => 
                "Structure the query to enable clear comparisons between different segments or time periods.",
            
            IntentType.Detail => 
                "Focus on retrieving detailed records with appropriate filtering and sorting.",
            
            IntentType.Exploratory => 
                "Provide a query that helps explore and understand the data structure and patterns.",
            
            _ => "Generate an appropriate SQL query based on the business context provided."
        };
    }

    private async Task<PromptTemplate> GetDefaultTemplateAsync()
    {
        return new PromptTemplate
        {
            Name = "Default Business Query",
            Content = "Convert the business question to SQL using the provided schema context.",
            Category = "general_query"
        };
    }

    // Mock example methods
    private List<BIReportingCopilot.Core.Models.PromptGeneration.QueryExample> GetAggregationExamples()
    {
        return new List<BIReportingCopilot.Core.Models.PromptGeneration.QueryExample>
        {
            new()
            {
                NaturalLanguageQuery = "What is the total revenue by country?",
                GeneratedSql = "SELECT Country, SUM(Revenue) as TotalRevenue FROM tbl_Daily_actions GROUP BY Country ORDER BY TotalRevenue DESC",
                BusinessContext = "Revenue aggregation by geographic dimension"
            }
        };
    }

    private List<BIReportingCopilot.Core.Models.PromptGeneration.QueryExample> GetTrendExamples()
    {
        return new List<BIReportingCopilot.Core.Models.PromptGeneration.QueryExample>
        {
            new()
            {
                NaturalLanguageQuery = "Show revenue trend over the last 6 months",
                GeneratedSql = "SELECT DATE_TRUNC('month', ActionDate) as Month, SUM(Revenue) as MonthlyRevenue FROM tbl_Daily_actions WHERE ActionDate >= DATEADD(month, -6, GETDATE()) GROUP BY DATE_TRUNC('month', ActionDate) ORDER BY Month",
                BusinessContext = "Time-series revenue analysis"
            }
        };
    }

    private List<BIReportingCopilot.Core.Models.PromptGeneration.QueryExample> GetComparisonExamples()
    {
        return new List<BIReportingCopilot.Core.Models.PromptGeneration.QueryExample>
        {
            new()
            {
                NaturalLanguageQuery = "Compare revenue between this month and last month",
                GeneratedSql = "SELECT CASE WHEN MONTH(ActionDate) = MONTH(GETDATE()) THEN 'This Month' ELSE 'Last Month' END as Period, SUM(Revenue) as TotalRevenue FROM tbl_Daily_actions WHERE ActionDate >= DATEADD(month, -2, GETDATE()) GROUP BY CASE WHEN MONTH(ActionDate) = MONTH(GETDATE()) THEN 'This Month' ELSE 'Last Month' END",
                BusinessContext = "Period-over-period revenue comparison"
            }
        };
    }

    private List<BIReportingCopilot.Core.Models.PromptGeneration.QueryExample> GetFallbackExamples(IntentType intentType)
    {
        return intentType switch
        {
            IntentType.Aggregation => GetAggregationExamples(),
            IntentType.Trend => GetTrendExamples(),
            IntentType.Comparison => GetComparisonExamples(),
            IntentType.Analytical => GetAnalyticalExamples(),
            IntentType.Operational => GetOperationalExamples(),
            IntentType.Exploratory => GetExploratoryExamples(),
            _ => new List<BIReportingCopilot.Core.Models.PromptGeneration.QueryExample>()
        };
    }

    private List<BIReportingCopilot.Core.Models.PromptGeneration.QueryExample> GetAnalyticalExamples()
    {
        return new List<BIReportingCopilot.Core.Models.PromptGeneration.QueryExample>
        {
            new()
            {
                NaturalLanguageQuery = "What is the average revenue per user by country?",
                GeneratedSql = "SELECT Country, AVG(Revenue) as AvgRevenuePerUser, COUNT(DISTINCT UserId) as UserCount FROM tbl_Daily_actions WHERE Revenue > 0 GROUP BY Country ORDER BY AvgRevenuePerUser DESC",
                BusinessContext = "Revenue per user analysis by geographic dimension"
            }
        };
    }

    private List<BIReportingCopilot.Core.Models.PromptGeneration.QueryExample> GetOperationalExamples()
    {
        return new List<BIReportingCopilot.Core.Models.PromptGeneration.QueryExample>
        {
            new()
            {
                NaturalLanguageQuery = "Show daily active users for the last week",
                GeneratedSql = "SELECT CAST(ActionDate AS DATE) as Date, COUNT(DISTINCT UserId) as DailyActiveUsers FROM tbl_Daily_actions WHERE ActionDate >= DATEADD(day, -7, GETDATE()) GROUP BY CAST(ActionDate AS DATE) ORDER BY Date",
                BusinessContext = "Daily active user monitoring for operational insights"
            }
        };
    }

    private List<BIReportingCopilot.Core.Models.PromptGeneration.QueryExample> GetExploratoryExamples()
    {
        return new List<BIReportingCopilot.Core.Models.PromptGeneration.QueryExample>
        {
            new()
            {
                NaturalLanguageQuery = "What are the top 10 actions by frequency?",
                GeneratedSql = "SELECT TOP 10 ActionType, COUNT(*) as ActionCount, COUNT(DISTINCT UserId) as UniqueUsers FROM tbl_Daily_actions GROUP BY ActionType ORDER BY ActionCount DESC",
                BusinessContext = "Exploratory analysis of user action patterns"
            }
        };
    }
}
