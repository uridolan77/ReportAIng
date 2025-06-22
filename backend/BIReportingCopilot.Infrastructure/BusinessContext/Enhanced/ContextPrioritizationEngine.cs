using Microsoft.Extensions.Logging;
using BIReportingCopilot.Core.Models.BusinessContext;
using BIReportingCopilot.Core.DTOs;
using BIReportingCopilot.Core.Interfaces.Cache;
using BIReportingCopilot.Core.Extensions;
using BIReportingCopilot.Core.Interfaces.Schema;
using BIReportingCopilot.Infrastructure.Data.Entities;

namespace BIReportingCopilot.Infrastructure.BusinessContext.Enhanced;

/// <summary>
/// Advanced context prioritization engine using knapsack algorithm for optimal context selection
/// </summary>
public class ContextPrioritizationEngine : IContextPrioritizationEngine
{
    private readonly ITokenBudgetManager _tokenBudgetManager;
    private readonly ICacheService _cacheService;
    private readonly ILogger<ContextPrioritizationEngine> _logger;

    // Context section importance weights by intent type
    private static readonly Dictionary<IntentType, ContextImportanceWeights> ImportanceWeights = new()
    {
        [IntentType.Aggregation] = new()
        {
            TableDefinitions = 0.9,
            ColumnDefinitions = 0.8,
            BusinessRules = 0.7,
            Examples = 0.8,
            Relationships = 0.6,
            GlossaryTerms = 0.4,
            PerformanceHints = 0.3
        },
        [IntentType.Trend] = new()
        {
            TableDefinitions = 0.8,
            ColumnDefinitions = 0.9,
            BusinessRules = 0.6,
            Examples = 0.8,
            Relationships = 0.7,
            GlossaryTerms = 0.4,
            PerformanceHints = 0.3
        },
        [IntentType.Comparison] = new()
        {
            TableDefinitions = 0.9,
            ColumnDefinitions = 0.8,
            BusinessRules = 0.8,
            Examples = 0.7,
            Relationships = 0.8,
            GlossaryTerms = 0.5,
            PerformanceHints = 0.4
        },
        [IntentType.Detail] = new()
        {
            TableDefinitions = 0.9,
            ColumnDefinitions = 0.9,
            BusinessRules = 0.6,
            Examples = 0.6,
            Relationships = 0.7,
            GlossaryTerms = 0.5,
            PerformanceHints = 0.3
        },
        [IntentType.Exploratory] = new()
        {
            TableDefinitions = 0.7,
            ColumnDefinitions = 0.7,
            BusinessRules = 0.8,
            Examples = 0.9,
            Relationships = 0.8,
            GlossaryTerms = 0.7,
            PerformanceHints = 0.4
        },
        [IntentType.Operational] = new()
        {
            TableDefinitions = 0.8,
            ColumnDefinitions = 0.8,
            BusinessRules = 0.9,
            Examples = 0.6,
            Relationships = 0.6,
            GlossaryTerms = 0.4,
            PerformanceHints = 0.7
        },
        [IntentType.Analytical] = new()
        {
            TableDefinitions = 0.8,
            ColumnDefinitions = 0.8,
            BusinessRules = 0.7,
            Examples = 0.8,
            Relationships = 0.7,
            GlossaryTerms = 0.5,
            PerformanceHints = 0.4
        }
    };

    // Performance tracking
    private readonly Dictionary<string, PrioritizationMetrics> _prioritizationMetrics = new();
    private readonly object _metricsLock = new();

    public ContextPrioritizationEngine(
        ITokenBudgetManager tokenBudgetManager,
        ICacheService cacheService,
        ILogger<ContextPrioritizationEngine> logger)
    {
        _tokenBudgetManager = tokenBudgetManager;
        _cacheService = cacheService;
        _logger = logger;
    }

    public async Task<List<ContextSection>> PrioritizeContextSectionsAsync(
        ContextualBusinessSchema schema,
        BusinessContextProfile profile,
        TokenBudget tokenBudget)
    {
        var cacheKey = $"prioritized_context:{schema.GetHashCode()}:{profile.Intent.Type}:{tokenBudget.AvailableContextTokens}";
        
        var (found, cachedSections) = await _cacheService.TryGetAsync<List<ContextSection>>(cacheKey);
        if (found && cachedSections != null)
        {
            _logger.LogDebug("Retrieved cached prioritized context sections");
            return cachedSections;
        }

        var startTime = DateTime.UtcNow;

        try
        {
            _logger.LogInformation("Prioritizing context sections for {Intent} with {TokenBudget} token budget",
                profile.Intent.Type, tokenBudget.AvailableContextTokens);

            // Create all possible context sections
            var allSections = await CreateAllContextSectionsAsync(schema, profile);
            
            // Calculate importance and efficiency scores for each section
            var scoredSections = await CalculateSectionScoresAsync(allSections, profile);
            
            // Apply knapsack algorithm to find optimal selection
            var optimalSections = SolveContextKnapsackProblem(scoredSections, tokenBudget.AvailableContextTokens);
            
            // Apply final ordering and adjustments
            var finalSections = await ApplyFinalOrderingAsync(optimalSections, profile);
            
            // Cache result for 30 minutes
            await _cacheService.SetAsync(cacheKey, finalSections, TimeSpan.FromMinutes(30));
            
            var duration = (DateTime.UtcNow - startTime).TotalMilliseconds;
            await RecordPrioritizationMetricsAsync("context_prioritization", duration, allSections.Count, finalSections.Count);
            
            _logger.LogInformation("Context prioritization completed in {Duration}ms: {Total} → {Selected} sections, {TokensUsed}/{TokenBudget} tokens",
                duration, allSections.Count, finalSections.Count, 
                finalSections.Sum(s => s.TokenCount), tokenBudget.AvailableContextTokens);

            return finalSections;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in context prioritization");
            return new List<ContextSection>();
        }
    }

    public async Task<ContextOptimizationResult> OptimizeContextSelectionAsync(
        List<ContextSection> candidateSections,
        BusinessContextProfile profile,
        int tokenBudget,
        OptimizationStrategy strategy = OptimizationStrategy.Balanced)
    {
        var startTime = DateTime.UtcNow;

        try
        {
            var result = new ContextOptimizationResult
            {
                Strategy = strategy,
                TokenBudget = tokenBudget,
                CandidateSections = candidateSections.Count
            };

            // Apply strategy-specific scoring
            var strategyScoredSections = ApplyStrategyScoring(candidateSections, profile, strategy);
            
            // Solve optimization problem
            var optimizedSections = strategy switch
            {
                OptimizationStrategy.MaxRelevance => SolveMaxRelevanceProblem(strategyScoredSections, tokenBudget),
                OptimizationStrategy.MaxCoverage => SolveMaxCoverageProblem(strategyScoredSections, tokenBudget),
                OptimizationStrategy.MinTokens => SolveMinTokensProblem(strategyScoredSections, tokenBudget),
                OptimizationStrategy.Balanced => SolveContextKnapsackProblem(strategyScoredSections, tokenBudget),
                _ => SolveContextKnapsackProblem(strategyScoredSections, tokenBudget)
            };

            result.SelectedSections = optimizedSections;
            result.TotalTokensUsed = optimizedSections.Sum(s => s.TokenCount);
            result.AverageRelevanceScore = optimizedSections.Any() ? optimizedSections.Average(s => s.RelevanceScore) : 0;
            result.TokenUtilization = tokenBudget > 0 ? (double)result.TotalTokensUsed / tokenBudget : 0;
            result.OptimizationDuration = (DateTime.UtcNow - startTime).TotalMilliseconds;

            _logger.LogDebug("Context optimization ({Strategy}): {Selected}/{Candidates} sections, {Tokens}/{Budget} tokens, {Relevance:F3} avg relevance",
                strategy, result.SelectedSections.Count, result.CandidateSections, result.TotalTokensUsed, tokenBudget, result.AverageRelevanceScore);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in context optimization");
            return new ContextOptimizationResult { Strategy = strategy, TokenBudget = tokenBudget };
        }
    }

    public async Task<PrioritizationAnalysisReport> GenerateAnalysisReportAsync(
        IntentType? intentFilter = null,
        TimeSpan? timeWindow = null)
    {
        var report = new PrioritizationAnalysisReport
        {
            GeneratedAt = DateTime.UtcNow,
            IntentFilter = intentFilter,
            TimeWindow = timeWindow ?? TimeSpan.FromDays(7)
        };

        lock (_metricsLock)
        {
            foreach (var (operation, metrics) in _prioritizationMetrics)
            {
                var avgDuration = metrics.TotalOperations > 0 ? metrics.TotalDuration / metrics.TotalOperations : 0;
                var avgInputSections = metrics.TotalOperations > 0 ? (double)metrics.TotalInputSections / metrics.TotalOperations : 0;
                var avgOutputSections = metrics.TotalOperations > 0 ? (double)metrics.TotalOutputSections / metrics.TotalOperations : 0;
                var selectionRatio = avgInputSections > 0 ? avgOutputSections / avgInputSections : 0;

                report.OperationAnalytics.Add(new PrioritizationOperationAnalytics
                {
                    OperationType = operation,
                    TotalOperations = metrics.TotalOperations,
                    AverageDuration = avgDuration,
                    AverageInputSections = avgInputSections,
                    AverageOutputSections = avgOutputSections,
                    SelectionRatio = selectionRatio,
                    LastOperation = metrics.LastOperation
                });
            }
        }

        return report;
    }

    private async Task<List<ContextSection>> CreateAllContextSectionsAsync(
        ContextualBusinessSchema schema,
        BusinessContextProfile profile)
    {
        var sections = new List<ContextSection>();

        // Create table definition sections
        foreach (var table in schema.RelevantTables)
        {
            var tableContent = BuildTableDefinitionContent(table);
            sections.Add(new ContextSection
            {
                Type = "table_definition",
                Content = tableContent,
                TokenCount = _tokenBudgetManager.CountTokens(tableContent, "schema"),
                RelevanceScore = table.RelevanceScore,
                Metadata = new Dictionary<string, object>
                {
                    ["table_id"] = table.Id,
                    ["table_name"] = table.TableName,
                    ["section_category"] = "TableDefinitions"
                }
            });
        }

        // Create column definition sections
        foreach (var tableColumns in schema.TableColumns)
        {
            foreach (var column in tableColumns.Value)
            {
                var columnContent = BuildColumnDefinitionContent(column);
                sections.Add(new ContextSection
                {
                    Type = "column_definition",
                    Content = columnContent,
                    TokenCount = _tokenBudgetManager.CountTokens(columnContent, "schema"),
                    RelevanceScore = column.RelevanceScore,
                    Metadata = new Dictionary<string, object>
                    {
                        ["column_id"] = column.Id,
                        ["column_name"] = column.ColumnName,
                        ["table_id"] = tableColumns.Key,
                        ["section_category"] = "ColumnDefinitions"
                    }
                });
            }
        }

        // Create business rules sections
        foreach (var rule in schema.BusinessRules)
        {
            var ruleContent = BuildBusinessRuleContent(rule);
            sections.Add(new ContextSection
            {
                Type = "business_rule",
                Content = ruleContent,
                TokenCount = _tokenBudgetManager.CountTokens(ruleContent, "rules"),
                RelevanceScore = rule.RelevanceScore,
                Metadata = new Dictionary<string, object>
                {
                    ["rule_id"] = rule.Id,
                    ["rule_type"] = rule.Type,
                    ["section_category"] = "BusinessRules"
                }
            });
        }

        // Create example sections (using RelevantQueryExample from schema)
        var examples = schema.RelevantExamples ?? new List<RelevantQueryExample>();
        foreach (var example in examples)
        {
            var exampleContent = BuildExampleContent(example);
            sections.Add(new ContextSection
            {
                Type = "example",
                Content = exampleContent,
                TokenCount = _tokenBudgetManager.CountTokens(exampleContent, "examples"),
                RelevanceScore = example.RelevanceScore,
                Metadata = new Dictionary<string, object>
                {
                    ["example_id"] = example.ExampleId,
                    ["example_type"] = example.IntentType,
                    ["section_category"] = "Examples"
                }
            });
        }

        // Create relationship sections
        foreach (var relationship in schema.TableRelationships)
        {
            var relationshipContent = BuildRelationshipContent(relationship);
            sections.Add(new ContextSection
            {
                Type = "relationship",
                Content = relationshipContent,
                TokenCount = _tokenBudgetManager.CountTokens(relationshipContent, "schema"),
                RelevanceScore = 0.6, // Default relevance for relationships
                Metadata = new Dictionary<string, object>
                {
                    ["relationship_from"] = relationship.FromTable,
                    ["relationship_to"] = relationship.ToTable,
                    ["relationship_type"] = relationship.Type,
                    ["section_category"] = "Relationships"
                }
            });
        }

        // Create glossary sections
        foreach (var term in schema.RelevantGlossaryTerms)
        {
            var glossaryContent = BuildGlossaryContent(term);
            sections.Add(new ContextSection
            {
                Type = "glossary",
                Content = glossaryContent,
                TokenCount = _tokenBudgetManager.CountTokens(glossaryContent, "glossary"),
                RelevanceScore = term.RelevanceScore,
                Metadata = new Dictionary<string, object>
                {
                    ["term_id"] = term.Id,
                    ["term"] = term.Term,
                    ["section_category"] = "GlossaryTerms"
                }
            });
        }

        return sections;
    }

    private async Task<List<ContextSection>> CalculateSectionScoresAsync(
        List<ContextSection> sections,
        BusinessContextProfile profile)
    {
        var importanceWeights = GetImportanceWeights(profile.Intent.Type);

        foreach (var section in sections)
        {
            // Get section category from metadata
            var category = section.Metadata.GetValueOrDefault("section_category", "Unknown").ToString();
            
            // Calculate importance score based on intent type
            var importanceScore = GetCategoryImportance(category!, importanceWeights);
            
            // Calculate efficiency score (relevance per token)
            var efficiencyScore = section.TokenCount > 0 ? section.RelevanceScore / section.TokenCount : 0;
            
            // Calculate final priority score
            var priorityScore = (section.RelevanceScore * 0.4) + (importanceScore * 0.4) + (efficiencyScore * 0.2);
            
            // Store calculated scores in metadata
            section.Metadata["importance_score"] = importanceScore;
            section.Metadata["efficiency_score"] = efficiencyScore;
            section.Metadata["priority_score"] = priorityScore;
        }

        return sections;
    }

    private List<ContextSection> SolveContextKnapsackProblem(List<ContextSection> sections, int tokenBudget)
    {
        if (!sections.Any() || tokenBudget <= 0)
            return new List<ContextSection>();

        var n = sections.Count;
        
        // Use priority score as value for knapsack algorithm
        var values = sections.Select(s => (int)(Convert.ToDouble(s.Metadata.GetValueOrDefault("priority_score", 0.0)) * 1000)).ToArray();
        var weights = sections.Select(s => s.TokenCount).ToArray();

        // Dynamic programming solution
        var dp = new int[n + 1, tokenBudget + 1];
        var selected = new bool[n + 1, tokenBudget + 1];

        // Fill DP table
        for (int i = 1; i <= n; i++)
        {
            for (int w = 1; w <= tokenBudget; w++)
            {
                var itemWeight = weights[i - 1];
                var itemValue = values[i - 1];

                if (itemWeight <= w)
                {
                    var includeValue = itemValue + dp[i - 1, w - itemWeight];
                    var excludeValue = dp[i - 1, w];

                    if (includeValue > excludeValue)
                    {
                        dp[i, w] = includeValue;
                        selected[i, w] = true;
                    }
                    else
                    {
                        dp[i, w] = excludeValue;
                    }
                }
                else
                {
                    dp[i, w] = dp[i - 1, w];
                }
            }
        }

        // Backtrack to find selected sections
        var result = new List<ContextSection>();
        int currentWeight = tokenBudget;
        
        for (int i = n; i > 0 && currentWeight > 0; i--)
        {
            if (selected[i, currentWeight])
            {
                result.Add(sections[i - 1]);
                currentWeight -= weights[i - 1];
            }
        }

        // Sort by importance for final ordering
        return result.OrderByDescending(s => s.Metadata.GetValueOrDefault("importance_score", 0.0)).ToList();
    }

    private List<ContextSection> SolveMaxRelevanceProblem(List<ContextSection> sections, int tokenBudget)
    {
        // Greedy approach: select highest relevance sections that fit in budget
        var sortedSections = sections.OrderByDescending(s => s.RelevanceScore).ToList();
        var result = new List<ContextSection>();
        var usedTokens = 0;

        foreach (var section in sortedSections)
        {
            if (usedTokens + section.TokenCount <= tokenBudget)
            {
                result.Add(section);
                usedTokens += section.TokenCount;
            }
        }

        return result;
    }

    private List<ContextSection> SolveMaxCoverageProblem(List<ContextSection> sections, int tokenBudget)
    {
        // Try to include at least one section from each category
        var sectionsByCategory = sections.GroupBy(s => s.Metadata.GetValueOrDefault("section_category", "Unknown")).ToList();
        var result = new List<ContextSection>();
        var usedTokens = 0;

        // First pass: one section per category
        foreach (var categoryGroup in sectionsByCategory)
        {
            var bestSection = categoryGroup.OrderByDescending(s => s.RelevanceScore).First();
            if (usedTokens + bestSection.TokenCount <= tokenBudget)
            {
                result.Add(bestSection);
                usedTokens += bestSection.TokenCount;
            }
        }

        // Second pass: fill remaining budget with highest relevance sections
        var remainingSections = sections.Except(result).OrderByDescending(s => s.RelevanceScore);
        foreach (var section in remainingSections)
        {
            if (usedTokens + section.TokenCount <= tokenBudget)
            {
                result.Add(section);
                usedTokens += section.TokenCount;
            }
        }

        return result;
    }

    private List<ContextSection> SolveMinTokensProblem(List<ContextSection> sections, int tokenBudget)
    {
        // Select sections with best relevance-to-token ratio
        var sortedSections = sections
            .Where(s => s.TokenCount > 0)
            .OrderByDescending(s => s.RelevanceScore / s.TokenCount)
            .ToList();

        var result = new List<ContextSection>();
        var usedTokens = 0;

        foreach (var section in sortedSections)
        {
            if (usedTokens + section.TokenCount <= tokenBudget)
            {
                result.Add(section);
                usedTokens += section.TokenCount;
            }
        }

        return result;
    }

    private List<ContextSection> ApplyStrategyScoring(
        List<ContextSection> sections,
        BusinessContextProfile profile,
        OptimizationStrategy strategy)
    {
        foreach (var section in sections)
        {
            var adjustedScore = strategy switch
            {
                OptimizationStrategy.MaxRelevance => section.RelevanceScore,
                OptimizationStrategy.MaxCoverage => section.RelevanceScore * 0.8 + 0.2, // Boost all sections slightly
                OptimizationStrategy.MinTokens => section.TokenCount > 0 ? section.RelevanceScore / Math.Log(section.TokenCount + 1) : section.RelevanceScore,
                OptimizationStrategy.Balanced => section.RelevanceScore,
                _ => section.RelevanceScore
            };

            section.Metadata["strategy_adjusted_score"] = adjustedScore;
        }

        return sections;
    }

    private async Task<List<ContextSection>> ApplyFinalOrderingAsync(
        List<ContextSection> sections,
        BusinessContextProfile profile)
    {
        // Order sections for optimal prompt flow
        var orderedSections = new List<ContextSection>();

        // 1. Table definitions first
        orderedSections.AddRange(sections.Where(s => s.Type == "table_definition").OrderByDescending(s => s.RelevanceScore));

        // 2. Column definitions
        orderedSections.AddRange(sections.Where(s => s.Type == "column_definition").OrderByDescending(s => s.RelevanceScore));

        // 3. Relationships
        orderedSections.AddRange(sections.Where(s => s.Type == "relationship").OrderByDescending(s => s.RelevanceScore));

        // 4. Business rules
        orderedSections.AddRange(sections.Where(s => s.Type == "business_rule").OrderByDescending(s => s.RelevanceScore));

        // 5. Examples
        orderedSections.AddRange(sections.Where(s => s.Type == "example").OrderByDescending(s => s.RelevanceScore));

        // 6. Glossary terms
        orderedSections.AddRange(sections.Where(s => s.Type == "glossary").OrderByDescending(s => s.RelevanceScore));

        return orderedSections;
    }

    private ContextImportanceWeights GetImportanceWeights(IntentType intentType)
    {
        return ImportanceWeights.GetValueOrDefault(intentType, ImportanceWeights[IntentType.Analytical]);
    }

    private double GetCategoryImportance(string category, ContextImportanceWeights weights)
    {
        return category switch
        {
            "TableDefinitions" => weights.TableDefinitions,
            "ColumnDefinitions" => weights.ColumnDefinitions,
            "BusinessRules" => weights.BusinessRules,
            "Examples" => weights.Examples,
            "Relationships" => weights.Relationships,
            "GlossaryTerms" => weights.GlossaryTerms,
            "PerformanceHints" => weights.PerformanceHints,
            _ => 0.5
        };
    }

    // Content building methods
    private string BuildTableDefinitionContent(BusinessTableInfoDto table)
    {
        return $"Table: {table.SchemaName}.{table.TableName}\n" +
               $"Purpose: {table.BusinessPurpose}\n" +
               $"Context: {table.BusinessContext}\n" +
               $"Use Case: {table.PrimaryUseCase}";
    }

    private string BuildColumnDefinitionContent(BusinessColumnInfo column)
    {
        return $"Column: {column.ColumnName} ({column.DataType})\n" +
               $"Meaning: {column.BusinessMeaning}\n" +
               $"Context: {column.BusinessContext}";
    }

    private string BuildBusinessRuleContent(BusinessRule rule)
    {
        return $"Rule: {rule.Description}\n" +
               $"Type: {rule.Type}\n" +
               $"SQL: {rule.SqlExpression}";
    }

    private string BuildExampleContent(RelevantQueryExample example)
    {
        return $"Example: {example.BusinessContext}\n" +
               $"Query: {example.NaturalLanguageQuery}\n" +
               $"SQL: {example.GeneratedSql}";
    }

    private string BuildRelationshipContent(TableRelationship relationship)
    {
        return $"Relationship: {relationship.FromTable} → {relationship.ToTable}\n" +
               $"Type: {relationship.Type}\n" +
               $"Keys: {relationship.FromColumn} = {relationship.ToColumn}\n" +
               $"Business Meaning: {relationship.BusinessMeaning}";
    }

    private string BuildGlossaryContent(BusinessGlossaryDto term)
    {
        return $"Term: {term.Term}\n" +
               $"Definition: {term.Definition}\n" +
               $"Context: {term.BusinessContext}";
    }

    private async Task RecordPrioritizationMetricsAsync(string operation, double duration, int inputSections, int outputSections)
    {
        lock (_metricsLock)
        {
            if (!_prioritizationMetrics.ContainsKey(operation))
            {
                _prioritizationMetrics[operation] = new PrioritizationMetrics();
            }

            var metrics = _prioritizationMetrics[operation];
            metrics.TotalOperations++;
            metrics.TotalDuration += duration;
            metrics.TotalInputSections += inputSections;
            metrics.TotalOutputSections += outputSections;
            metrics.LastOperation = DateTime.UtcNow;
        }
    }
}

// Supporting data structures
public record ContextImportanceWeights
{
    public double TableDefinitions { get; init; }
    public double ColumnDefinitions { get; init; }
    public double BusinessRules { get; init; }
    public double Examples { get; init; }
    public double Relationships { get; init; }
    public double GlossaryTerms { get; init; }
    public double PerformanceHints { get; init; }
}

public enum OptimizationStrategy
{
    MaxRelevance,
    MaxCoverage,
    MinTokens,
    Balanced
}

public record ContextOptimizationResult
{
    public OptimizationStrategy Strategy { get; set; }
    public int TokenBudget { get; set; }
    public int CandidateSections { get; set; }
    public List<ContextSection> SelectedSections { get; set; } = new();
    public int TotalTokensUsed { get; set; }
    public double AverageRelevanceScore { get; set; }
    public double TokenUtilization { get; set; }
    public double OptimizationDuration { get; set; }
}

public class PrioritizationMetrics
{
    public int TotalOperations { get; set; }
    public double TotalDuration { get; set; }
    public int TotalInputSections { get; set; }
    public int TotalOutputSections { get; set; }
    public DateTime LastOperation { get; set; } = DateTime.UtcNow;
}

public record PrioritizationAnalysisReport
{
    public DateTime GeneratedAt { get; init; }
    public IntentType? IntentFilter { get; init; }
    public TimeSpan TimeWindow { get; init; }
    public List<PrioritizationOperationAnalytics> OperationAnalytics { get; init; } = new();
}

public record PrioritizationOperationAnalytics
{
    public string OperationType { get; init; } = string.Empty;
    public int TotalOperations { get; init; }
    public double AverageDuration { get; init; }
    public double AverageInputSections { get; init; }
    public double AverageOutputSections { get; init; }
    public double SelectionRatio { get; init; }
    public DateTime LastOperation { get; init; }
}

public interface IContextPrioritizationEngine
{
    Task<List<ContextSection>> PrioritizeContextSectionsAsync(ContextualBusinessSchema schema, BusinessContextProfile profile, TokenBudget tokenBudget);
    Task<ContextOptimizationResult> OptimizeContextSelectionAsync(List<ContextSection> candidateSections, BusinessContextProfile profile, int tokenBudget, OptimizationStrategy strategy = OptimizationStrategy.Balanced);
    Task<PrioritizationAnalysisReport> GenerateAnalysisReportAsync(IntentType? intentFilter = null, TimeSpan? timeWindow = null);
}
