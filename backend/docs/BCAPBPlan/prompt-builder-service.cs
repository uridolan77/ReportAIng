// Application/Services/ContextualPromptBuilder.cs
namespace ReportAIng.Application.Services
{
    public class ContextualPromptBuilder : IContextualPromptBuilder
    {
        private readonly IPromptTemplateRepository _templateRepository;
        private readonly IQueryExampleRepository _exampleRepository;
        private readonly ILogger<ContextualPromptBuilder> _logger;
        private readonly IMemoryCache _cache;
        private readonly PromptBuilderConfig _config;

        private readonly Dictionary<IntentType, string> _intentTemplateMapping = new()
        {
            [IntentType.Analytical] = "analytical_query_template",
            [IntentType.Operational] = "operational_query_template",
            [IntentType.Exploratory] = "exploratory_query_template",
            [IntentType.Comparison] = "comparison_query_template",
            [IntentType.Aggregation] = "aggregation_query_template",
            [IntentType.Trend] = "trend_analysis_template",
            [IntentType.Detail] = "detail_query_template"
        };

        public ContextualPromptBuilder(
            IPromptTemplateRepository templateRepository,
            IQueryExampleRepository exampleRepository,
            ILogger<ContextualPromptBuilder> logger,
            IMemoryCache cache,
            IOptions<PromptBuilderConfig> config)
        {
            _templateRepository = templateRepository;
            _exampleRepository = exampleRepository;
            _logger = logger;
            _cache = cache;
            _config = config.Value;
        }

        public async Task<string> BuildBusinessAwarePromptAsync(
            string userQuestion,
            BusinessContextProfile profile,
            ContextualBusinessSchema schema)
        {
            _logger.LogInformation("Building business-aware prompt for intent: {Intent}", 
                profile.Intent.Type);

            // Select optimal template
            var template = await SelectOptimalTemplateAsync(profile);

            // Create prompt generation context
            var context = new PromptGenerationContext
            {
                UserQuestion = userQuestion,
                BusinessContext = profile,
                Schema = schema,
                SelectedTemplate = template,
                ComplexityLevel = DetermineComplexityLevel(profile, schema)
            };

            // Find relevant examples
            context.RelevantExamples = await FindRelevantExamplesAsync(profile, 3);

            // Build placeholder values
            context.PlaceholderValues = BuildPlaceholderValues(context);

            // Generate the prompt
            var basePrompt = RenderTemplate(template.Content, context.PlaceholderValues);

            // Enrich with business context
            var enrichedPrompt = await EnrichPromptWithBusinessContextAsync(basePrompt, schema);

            // Add optimization hints
            enrichedPrompt = AddOptimizationHints(enrichedPrompt, context);

            return enrichedPrompt;
        }

        public async Task<PromptTemplate> SelectOptimalTemplateAsync(BusinessContextProfile profile)
        {
            // Get template key based on intent
            var templateKey = _intentTemplateMapping.GetValueOrDefault(
                profile.Intent.Type, 
                "general_query_template");

            // Try to get from cache
            var cacheKey = $"prompt_template_{templateKey}";
            if (_cache.TryGetValue<PromptTemplate>(cacheKey, out var cachedTemplate))
            {
                return cachedTemplate!;
            }

            // Get from repository
            var template = await _templateRepository.GetByKeyAsync(templateKey);
            
            if (template == null)
            {
                _logger.LogWarning("Template not found for key: {Key}, using default", templateKey);
                template = await _templateRepository.GetByKeyAsync("general_query_template");
            }

            // Cache the template
            _cache.Set(cacheKey, template, TimeSpan.FromHours(1));

            return template!;
        }

        public async Task<string> EnrichPromptWithBusinessContextAsync(
            string basePrompt,
            ContextualBusinessSchema schema)
        {
            var enrichmentSections = new List<string>();

            // Add schema context section
            enrichmentSections.Add(BuildSchemaContextSection(schema));

            // Add business rules section
            if (schema.BusinessRules.Any())
            {
                enrichmentSections.Add(BuildBusinessRulesSection(schema.BusinessRules));
            }

            // Add glossary context
            if (schema.RelevantGlossaryTerms.Any())
            {
                enrichmentSections.Add(BuildGlossarySection(schema.RelevantGlossaryTerms));
            }

            // Add relationship context
            if (schema.Relationships.Any())
            {
                enrichmentSections.Add(BuildRelationshipSection(schema.Relationships));
            }

            // Add performance hints
            if (schema.Complexity >= SchemaComplexity.Complex)
            {
                enrichmentSections.Add(BuildPerformanceSection(schema));
            }

            // Combine all sections
            var enrichedPrompt = basePrompt + "\n\n" + string.Join("\n\n", enrichmentSections);

            return enrichedPrompt;
        }

        public async Task<List<QueryExample>> FindRelevantExamplesAsync(
            BusinessContextProfile profile,
            int maxExamples = 3)
        {
            var examples = new List<QueryExample>();

            // Find examples by intent type
            var intentExamples = await _exampleRepository.GetByIntentTypeAsync(
                profile.Intent.Type.ToString(), 
                maxExamples * 2);

            // Find examples by domain
            var domainExamples = await _exampleRepository.GetByDomainAsync(
                profile.Domain.Name, 
                maxExamples * 2);

            // Find examples by entities
            var entityExamples = new List<QueryExampleEntity>();
            foreach (var entity in profile.Entities.Where(e => e.Type == EntityType.Table))
            {
                var tableExamples = await _exampleRepository.GetByTableAsync(
                    entity.MappedTableName, 
                    maxExamples);
                entityExamples.AddRange(tableExamples);
            }

            // Score and rank all examples
            var allExamples = intentExamples
                .Concat(domainExamples)
                .Concat(entityExamples)
                .Distinct()
                .ToList();

            var scoredExamples = await ScoreExamplesAsync(allExamples, profile);

            // Select top examples
            return scoredExamples
                .OrderByDescending(e => e.RelevanceScore)
                .Take(maxExamples)
                .ToList();
        }

        // Private helper methods
        private Dictionary<string, string> BuildPlaceholderValues(PromptGenerationContext context)
        {
            var placeholders = new Dictionary<string, string>
            {
                ["USER_QUESTION"] = context.UserQuestion,
                ["INTENT_TYPE"] = context.BusinessContext.Intent.Type.ToString(),
                ["BUSINESS_DOMAIN"] = context.BusinessContext.Domain.Name,
                ["COMPLEXITY_LEVEL"] = context.ComplexityLevel.ToString(),
                ["CURRENT_DATE"] = DateTime.UtcNow.ToString("yyyy-MM-dd"),
                ["EXAMPLES"] = FormatExamples(context.RelevantExamples),
                ["PRIMARY_TABLES"] = FormatPrimaryTables(context.Schema.RelevantTables),
                ["TIME_CONTEXT"] = FormatTimeContext(context.BusinessContext.TimeContext),
                ["METRICS"] = string.Join(", ", context.BusinessContext.IdentifiedMetrics),
                ["DIMENSIONS"] = string.Join(", ", context.BusinessContext.IdentifiedDimensions)
            };

            return placeholders;
        }

        private string BuildSchemaContextSection(ContextualBusinessSchema schema)
        {
            var sb = new StringBuilder();
            sb.AppendLine("## Business Context");
            sb.AppendLine();
            sb.AppendLine("### Relevant Tables:");
            
            foreach (var table in schema.RelevantTables)
            {
                sb.AppendLine($"- **{table.SchemaName}.{table.TableName}**");
                sb.AppendLine($"  - Purpose: {table.BusinessPurpose}");
                sb.AppendLine($"  - Primary Use Case: {table.PrimaryUseCase}");
                
                if (schema.TableColumns.TryGetValue(table.Id, out var columns))
                {
                    sb.AppendLine("  - Key Columns:");
                    foreach (var col in columns.Take(5))
                    {
                        sb.AppendLine($"    - {col.ColumnName}: {col.BusinessMeaning}");
                    }
                }
                sb.AppendLine();
            }

            return sb.ToString();
        }

        private string BuildBusinessRulesSection(List<BusinessRule> rules)
        {
            var sb = new StringBuilder();
            sb.AppendLine("## Business Rules to Consider:");
            
            foreach (var rule in rules.OrderBy(r => r.Priority))
            {
                sb.AppendLine($"- {rule.Description}");
                if (!string.IsNullOrEmpty(rule.SqlExpression))
                {
                    sb.AppendLine($"  SQL: `{rule.SqlExpression}`");
                }
            }

            return sb.ToString();
        }

        private string BuildGlossarySection(List<BusinessGlossaryDto> terms)
        {
            var sb = new StringBuilder();
            sb.AppendLine("## Business Term Definitions:");
            
            foreach (var term in terms)
            {
                sb.AppendLine($"- **{term.Term}**: {term.Definition}");
                if (!string.IsNullOrEmpty(term.PreferredCalculation))
                {
                    sb.AppendLine($"  Calculation: `{term.PreferredCalculation}`");
                }
            }

            return sb.ToString();
        }

        private string BuildRelationshipSection(List<TableRelationship> relationships)
        {
            var sb = new StringBuilder();
            sb.AppendLine("## Table Relationships:");
            
            foreach (var rel in relationships)
            {
                sb.AppendLine($"- {rel.FromTable}.{rel.FromColumn} → {rel.ToTable}.{rel.ToColumn}");
                sb.AppendLine($"  Type: {rel.Type}, Meaning: {rel.BusinessMeaning}");
            }

            return sb.ToString();
        }

        private string BuildPerformanceSection(ContextualBusinessSchema schema)
        {
            var sb = new StringBuilder();
            sb.AppendLine("## Performance Considerations:");
            
            if (schema.SuggestedIndexes.Any())
            {
                sb.AppendLine("### Suggested Indexes:");
                foreach (var index in schema.SuggestedIndexes)
                {
                    sb.AppendLine($"- {index}");
                }
            }

            if (schema.PartitioningHints.Any())
            {
                sb.AppendLine("### Partitioning Hints:");
                foreach (var hint in schema.PartitioningHints)
                {
                    sb.AppendLine($"- {hint}");
                }
            }

            return sb.ToString();
        }

        private string FormatExamples(List<QueryExample> examples)
        {
            if (!examples.Any()) return "No specific examples available.";

            var sb = new StringBuilder();
            sb.AppendLine("### Query Examples:");
            
            foreach (var example in examples)
            {
                sb.AppendLine($"**Question**: {example.NaturalLanguageQuery}");
                sb.AppendLine($"**SQL**:");
                sb.AppendLine("```sql");
                sb.AppendLine(example.GeneratedSql);
                sb.AppendLine("```");
                sb.AppendLine();
            }

            return sb.ToString();
        }

        private string AddOptimizationHints(string prompt, PromptGenerationContext context)
        {
            var hints = new List<string>();

            // Add complexity-based hints
            if (context.ComplexityLevel >= PromptComplexityLevel.Advanced)
            {
                hints.Add("Consider using CTEs for complex queries");
                hints.Add("Optimize joins by starting with the smallest table");
            }

            // Add intent-based hints
            switch (context.BusinessContext.Intent.Type)
            {
                case IntentType.Aggregation:
                    hints.Add("Use appropriate GROUP BY clauses");
                    hints.Add("Consider window functions for running totals");
                    break;
                case IntentType.Trend:
                    hints.Add("Include proper date grouping");
                    hints.Add("Consider using date functions for period comparisons");
                    break;
                case IntentType.Comparison:
                    hints.Add("Use CASE statements or PIVOT for comparisons");
                    break;
            }

            if (hints.Any())
            {
                prompt += "\n\n## Optimization Hints:\n" + string.Join("\n", hints.Select(h => $"- {h}"));
            }

            return prompt;
        }

        private PromptComplexityLevel DetermineComplexityLevel(
            BusinessContextProfile profile,
            ContextualBusinessSchema schema)
        {
            // Factors for complexity
            var factors = new List<int>();

            // Schema complexity
            factors.Add(schema.Complexity switch
            {
                SchemaComplexity.Simple => 1,
                SchemaComplexity.Moderate => 2,
                SchemaComplexity.Complex => 3,
                SchemaComplexity.VeryComplex => 4,
                _ => 2
            });

            // Number of entities
            factors.Add(profile.Entities.Count > 5 ? 3 : profile.Entities.Count > 2 ? 2 : 1);

            // Intent complexity
            factors.Add(profile.Intent.Type switch
            {
                IntentType.Detail => 1,
                IntentType.Operational => 1,
                IntentType.Aggregation => 2,
                IntentType.Exploratory => 2,
                IntentType.Analytical => 3,
                IntentType.Comparison => 3,
                IntentType.Trend => 3,
                _ => 2
            });

            var avgComplexity = factors.Average();
            
            return avgComplexity switch
            {
                < 1.5 => PromptComplexityLevel.Basic,
                < 2.5 => PromptComplexityLevel.Standard,
                < 3.5 => PromptComplexityLevel.Advanced,
                _ => PromptComplexityLevel.Expert
            };
        }

        private string RenderTemplate(string templateContent, Dictionary<string, string> placeholders)
        {
            var result = templateContent;
            
            foreach (var (key, value) in placeholders)
            {
                result = result.Replace($"{{{key}}}", value);
            }

            return result;
        }
    }

    public class PromptBuilderConfig
    {
        public int MaxExamplesInPrompt { get; set; } = 3;
        public bool IncludeBusinessRules { get; set; } = true;
        public bool IncludePerformanceHints { get; set; } = true;
        public int MaxTablesInContext { get; set; } = 5;
        public int MaxColumnsPerTable { get; set; } = 10;
    }
}