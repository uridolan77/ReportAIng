using BIReportingCopilot.Core.Models;
using BIReportingCopilot.Core.Models.Enhanced;
using BIReportingCopilot.Core.Models.BusinessContext;
using Microsoft.Extensions.Logging;
using System.Text;

namespace BIReportingCopilot.Infrastructure.BusinessContext.Enhanced;

/// <summary>
/// Enhanced prompt template with null-safe business metadata access
/// Provides sophisticated prompt generation with business context integration
/// </summary>
public class EnhancedPromptTemplate
{
    private readonly ILogger<EnhancedPromptTemplate> _logger;

    public EnhancedPromptTemplate(ILogger<EnhancedPromptTemplate> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Generate enhanced SQL prompt with business context and metadata
    /// </summary>
    public string GenerateEnhancedSqlPrompt(
        string userQuery,
        BusinessProfile businessProfile,
        ContextualSchema contextualSchema,
        TokenBudget tokenBudget)
    {
        try
        {
            _logger.LogDebug("🎯 [ENHANCED-PROMPT] Generating enhanced prompt for query: {Query}", userQuery);

            var prompt = new StringBuilder();

            // 1. System context and role definition
            prompt.AppendLine(GenerateSystemContext(businessProfile));
            prompt.AppendLine();

            // 2. Business domain context
            prompt.AppendLine(GenerateBusinessDomainContext(businessProfile));
            prompt.AppendLine();

            // 3. Schema context with business metadata
            prompt.AppendLine(GenerateSchemaContext(contextualSchema));
            prompt.AppendLine();

            // 4. Business rules and constraints
            prompt.AppendLine(GenerateBusinessRulesContext(contextualSchema));
            prompt.AppendLine();

            // 5. Query intent and entity context
            prompt.AppendLine(GenerateQueryIntentContext(businessProfile, userQuery));
            prompt.AppendLine();

            // 6. SQL generation instructions
            prompt.AppendLine(GenerateSqlInstructions(businessProfile, tokenBudget));
            prompt.AppendLine();

            // 7. User query
            prompt.AppendLine($"User Query: {userQuery}");
            prompt.AppendLine();
            prompt.AppendLine("Generate the SQL query:");

            var finalPrompt = prompt.ToString();
            _logger.LogDebug("✅ [ENHANCED-PROMPT] Generated prompt with {Length} characters", finalPrompt.Length);

            return finalPrompt;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ [ENHANCED-PROMPT] Failed to generate enhanced prompt");
            return GenerateFallbackPrompt(userQuery, contextualSchema);
        }
    }

    /// <summary>
    /// Generate system context with role definition
    /// </summary>
    private string GenerateSystemContext(BusinessProfile businessProfile)
    {
        var domain = businessProfile?.Domain?.Name ?? "Business Intelligence";
        var intent = businessProfile?.Intent?.Type ?? "Analysis";

        return $@"You are an expert SQL analyst specializing in {domain} data analysis.
Your role is to generate precise, efficient SQL queries for {intent.ToLower()} purposes.
You have deep understanding of business logic, data relationships, and performance optimization.

Key Responsibilities:
- Generate syntactically correct and semantically meaningful SQL
- Apply appropriate business logic and domain knowledge
- Ensure optimal performance and proper indexing usage
- Follow data governance and security best practices";
    }

    /// <summary>
    /// Generate business domain context
    /// </summary>
    private string GenerateBusinessDomainContext(BusinessProfile businessProfile)
    {
        var context = new StringBuilder();
        context.AppendLine("=== BUSINESS DOMAIN CONTEXT ===");

        if (businessProfile?.Domain != null)
        {
            context.AppendLine($"Domain: {businessProfile.Domain.Name}");
            
            if (!string.IsNullOrEmpty(businessProfile.Domain.Description))
            {
                context.AppendLine($"Domain Description: {businessProfile.Domain.Description}");
            }

            if (businessProfile.Domain.KeyConcepts?.Any() == true)
            {
                context.AppendLine("Key Business Concepts:");
                foreach (var concept in businessProfile.Domain.KeyConcepts)
                {
                    context.AppendLine($"- {concept}");
                }
            }
        }

        if (businessProfile?.Intent != null)
        {
            context.AppendLine($"Query Intent: {businessProfile.Intent.Type}");
            context.AppendLine($"Confidence: {businessProfile.Intent.Confidence:F2}");
            
            if (!string.IsNullOrEmpty(businessProfile.Intent.Description))
            {
                context.AppendLine($"Intent Description: {businessProfile.Intent.Description}");
            }
        }

        return context.ToString();
    }

    /// <summary>
    /// Generate schema context with business metadata
    /// </summary>
    private string GenerateSchemaContext(ContextualSchema contextualSchema)
    {
        var context = new StringBuilder();
        context.AppendLine("=== DATABASE SCHEMA CONTEXT ===");

        if (contextualSchema?.RelevantTables?.Any() == true)
        {
            foreach (var table in contextualSchema.RelevantTables)
            {
                context.AppendLine($"\nTable: {table.TableName}");
                context.AppendLine($"Business Purpose: {table.BusinessPurpose ?? "Not specified"}");
                context.AppendLine($"Relevance Score: {table.RelevanceScore:F2}");

                if (table.Columns?.Any() == true)
                {
                    context.AppendLine("Columns:");
                    foreach (var column in table.Columns)
                    {
                        context.AppendLine($"  - {column.ColumnName} ({column.DataType})");
                        
                        // Null-safe access to business metadata
                        if (!string.IsNullOrEmpty(column.BusinessFriendlyName))
                        {
                            context.AppendLine($"    Business Name: {column.BusinessFriendlyName}");
                        }
                        
                        if (!string.IsNullOrEmpty(column.NaturalLanguageDescription))
                        {
                            context.AppendLine($"    Description: {column.NaturalLanguageDescription}");
                        }
                        
                        if (!string.IsNullOrEmpty(column.ValueExamples))
                        {
                            context.AppendLine($"    Example Values: {column.ValueExamples}");
                        }
                        
                        if (!string.IsNullOrEmpty(column.ConstraintsAndRules))
                        {
                            context.AppendLine($"    Constraints: {column.ConstraintsAndRules}");
                        }
                    }
                }
            }
        }

        if (contextualSchema?.GlossaryTerms?.Any() == true)
        {
            context.AppendLine("\n=== BUSINESS GLOSSARY ===");
            foreach (var term in contextualSchema.GlossaryTerms)
            {
                if (!string.IsNullOrEmpty(term.Term) && !string.IsNullOrEmpty(term.Definition))
                {
                    context.AppendLine($"- {term.Term}: {term.Definition}");
                }
            }
        }

        return context.ToString();
    }

    /// <summary>
    /// Generate business rules and constraints context
    /// </summary>
    private string GenerateBusinessRulesContext(ContextualSchema contextualSchema)
    {
        var context = new StringBuilder();
        context.AppendLine("=== BUSINESS RULES & CONSTRAINTS ===");

        var hasRules = false;

        if (contextualSchema?.RelevantTables?.Any() == true)
        {
            foreach (var table in contextualSchema.RelevantTables)
            {
                if (table.Columns?.Any() == true)
                {
                    var columnsWithRules = table.Columns
                        .Where(c => !string.IsNullOrEmpty(c.ConstraintsAndRules))
                        .ToList();

                    if (columnsWithRules.Any())
                    {
                        context.AppendLine($"\n{table.TableName} Rules:");
                        foreach (var column in columnsWithRules)
                        {
                            context.AppendLine($"- {column.ColumnName}: {column.ConstraintsAndRules}");
                        }
                        hasRules = true;
                    }
                }
            }
        }

        if (!hasRules)
        {
            context.AppendLine("- Follow standard SQL best practices");
            context.AppendLine("- Use appropriate data types and constraints");
            context.AppendLine("- Ensure referential integrity in joins");
        }

        return context.ToString();
    }

    /// <summary>
    /// Generate query intent and entity context
    /// </summary>
    private string GenerateQueryIntentContext(BusinessProfile businessProfile, string userQuery)
    {
        var context = new StringBuilder();
        context.AppendLine("=== QUERY ANALYSIS ===");

        if (businessProfile?.Entities?.Any() == true)
        {
            context.AppendLine("Identified Entities:");
            foreach (var entity in businessProfile.Entities)
            {
                context.AppendLine($"- {entity.Name} ({entity.Type})");
                if (entity.Confidence.HasValue)
                {
                    context.AppendLine($"  Confidence: {entity.Confidence.Value:F2}");
                }
                if (!string.IsNullOrEmpty(entity.Value))
                {
                    context.AppendLine($"  Value: {entity.Value}");
                }
            }
        }

        if (businessProfile?.Intent != null)
        {
            context.AppendLine($"\nQuery Intent: {businessProfile.Intent.Type}");
            if (businessProfile.Intent.RequiredAggregations?.Any() == true)
            {
                context.AppendLine("Required Aggregations:");
                foreach (var agg in businessProfile.Intent.RequiredAggregations)
                {
                    context.AppendLine($"- {agg}");
                }
            }
        }

        return context.ToString();
    }

    /// <summary>
    /// Generate SQL generation instructions
    /// </summary>
    private string GenerateSqlInstructions(BusinessProfile businessProfile, TokenBudget tokenBudget)
    {
        var instructions = new StringBuilder();
        instructions.AppendLine("=== SQL GENERATION INSTRUCTIONS ===");

        instructions.AppendLine("Requirements:");
        instructions.AppendLine("1. Generate syntactically correct SQL Server T-SQL");
        instructions.AppendLine("2. Use proper table aliases for readability");
        instructions.AppendLine("3. Include appropriate WHERE clauses for filtering");
        instructions.AppendLine("4. Use proper JOIN syntax when multiple tables are involved");
        instructions.AppendLine("5. Apply appropriate aggregation functions when needed");
        instructions.AppendLine("6. Include ORDER BY clause for ranking/sorting queries");
        instructions.AppendLine("7. Use TOP clause for limit queries");

        if (businessProfile?.Intent?.Type == "Operational")
        {
            instructions.AppendLine("8. Focus on operational metrics and KPIs");
            instructions.AppendLine("9. Include date/time filtering for recent data");
        }
        else if (businessProfile?.Intent?.Type == "Analytical")
        {
            instructions.AppendLine("8. Focus on analytical insights and trends");
            instructions.AppendLine("9. Include appropriate grouping and aggregation");
        }

        instructions.AppendLine("\nPerformance Considerations:");
        instructions.AppendLine("- Use indexed columns in WHERE clauses");
        instructions.AppendLine("- Avoid SELECT * when specific columns are needed");
        instructions.AppendLine("- Use appropriate JOIN types (INNER, LEFT, etc.)");
        
        if (tokenBudget != null)
        {
            instructions.AppendLine($"- Keep result set reasonable (consider TOP {Math.Min(tokenBudget.MaxTotalTokens / 100, 1000)})");
        }

        return instructions.ToString();
    }

    /// <summary>
    /// Generate fallback prompt when enhanced generation fails
    /// </summary>
    private string GenerateFallbackPrompt(string userQuery, ContextualSchema contextualSchema)
    {
        var prompt = new StringBuilder();
        
        prompt.AppendLine("You are a SQL expert. Generate a SQL query based on the following:");
        prompt.AppendLine();
        
        if (contextualSchema?.RelevantTables?.Any() == true)
        {
            prompt.AppendLine("Available Tables:");
            foreach (var table in contextualSchema.RelevantTables)
            {
                prompt.AppendLine($"- {table.TableName}");
                if (table.Columns?.Any() == true)
                {
                    var columnNames = table.Columns.Select(c => c.ColumnName).Take(5);
                    prompt.AppendLine($"  Columns: {string.Join(", ", columnNames)}");
                }
            }
            prompt.AppendLine();
        }
        
        prompt.AppendLine($"User Query: {userQuery}");
        prompt.AppendLine();
        prompt.AppendLine("Generate the SQL query:");
        
        return prompt.ToString();
    }
}
