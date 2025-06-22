using Microsoft.Extensions.Logging;
using BIReportingCopilot.Core.Models.BusinessContext;
using BIReportingCopilot.Core.Interfaces.Cache;
using System.Text;

namespace BIReportingCopilot.Infrastructure.BusinessContext.Enhanced;

/// <summary>
/// Progressive context building system that builds context incrementally with priority-based section addition
/// </summary>
public class ProgressiveContextBuilder : IProgressiveContextBuilder
{
    private readonly ITokenBudgetManager _tokenBudgetManager;
    private readonly IContextPrioritizationEngine _prioritizationEngine;
    private readonly ICacheService _cacheService;
    private readonly ILogger<ProgressiveContextBuilder> _logger;

    // Template structure for different intent types
    private static readonly Dictionary<IntentType, PromptTemplate> PromptTemplates = new()
    {
        [IntentType.Aggregation] = new()
        {
            Name = "Aggregation Template",
            SystemPrompt = "You are an expert SQL analyst specializing in aggregation queries and business metrics.",
            ContextSections = new[] { "schema", "business", "examples", "rules" },
            ClosingInstructions = "Generate a SQL query that calculates the requested aggregations accurately."
        },
        [IntentType.Trend] = new()
        {
            SystemPrompt = "You are an expert SQL analyst specializing in time-series analysis and trend identification.",
            ContextSections = new[] { "schema", "business", "examples", "temporal" },
            ClosingInstructions = "Generate a SQL query that analyzes trends over time with appropriate time groupings."
        },
        [IntentType.Comparison] = new()
        {
            SystemPrompt = "You are an expert SQL analyst specializing in comparative analysis and benchmarking.",
            ContextSections = new[] { "schema", "business", "examples", "relationships" },
            ClosingInstructions = "Generate a SQL query that compares entities or metrics as requested."
        },
        [IntentType.Detail] = new()
        {
            SystemPrompt = "You are an expert SQL analyst specializing in detailed data retrieval and record-level queries.",
            ContextSections = new[] { "schema", "business", "examples" },
            ClosingInstructions = "Generate a SQL query that retrieves the specific details requested."
        },
        [IntentType.Exploratory] = new()
        {
            SystemPrompt = "You are an expert SQL analyst specializing in data exploration and discovery.",
            ContextSections = new[] { "business", "schema", "examples", "glossary" },
            ClosingInstructions = "Generate a SQL query that explores the data to find insights and patterns."
        },
        [IntentType.Operational] = new()
        {
            SystemPrompt = "You are an expert SQL analyst specializing in operational queries and real-time data.",
            ContextSections = new[] { "schema", "business", "rules", "performance" },
            ClosingInstructions = "Generate an efficient SQL query for operational use with appropriate performance considerations."
        },
        [IntentType.Analytical] = new()
        {
            SystemPrompt = "You are an expert SQL analyst specializing in complex analytical queries and business intelligence.",
            ContextSections = new[] { "schema", "business", "examples", "rules", "relationships" },
            ClosingInstructions = "Generate a comprehensive SQL query that addresses the analytical requirements."
        }
    };

    // Performance tracking
    private readonly Dictionary<string, ProgressiveBuildMetrics> _buildMetrics = new();
    private readonly object _metricsLock = new();

    public ProgressiveContextBuilder(
        ITokenBudgetManager tokenBudgetManager,
        IContextPrioritizationEngine prioritizationEngine,
        ICacheService cacheService,
        ILogger<ProgressiveContextBuilder> logger)
    {
        _tokenBudgetManager = tokenBudgetManager;
        _prioritizationEngine = prioritizationEngine;
        _cacheService = cacheService;
        _logger = logger;
    }

    public async Task<ProgressiveBuildResult> BuildOptimalPromptAsync(
        string userQuestion,
        BusinessContextProfile profile,
        ContextualBusinessSchema schema,
        int maxTokens = 4000,
        int reservedResponseTokens = 500)
    {
        var buildId = Guid.NewGuid().ToString("N")[..8];
        var startTime = DateTime.UtcNow;

        _logger.LogInformation("Starting progressive context build {BuildId} for {Intent} query", buildId, profile.Intent.Type);

        try
        {
            var result = new ProgressiveBuildResult
            {
                BuildId = buildId,
                IntentType = profile.Intent.Type,
                UserQuestion = userQuestion,
                MaxTokens = maxTokens,
                BuildSteps = new List<ProgressiveBuildStep>()
            };

            // Step 1: Create token budget
            var tokenBudget = await _tokenBudgetManager.CreateTokenBudgetAsync(profile, maxTokens, reservedResponseTokens);
            result.TokenBudget = tokenBudget;
            
            await RecordBuildStep(result, "TokenBudgetCreation", $"Created budget: {tokenBudget.AvailableContextTokens} tokens available");

            // Step 2: Get prioritized context sections
            var prioritizedSections = await _prioritizationEngine.PrioritizeContextSectionsAsync(schema, profile, tokenBudget);
            
            await RecordBuildStep(result, "ContextPrioritization", $"Prioritized {prioritizedSections.Count} context sections");

            // Step 3: Progressive context assembly
            var assemblyResult = await AssembleContextProgressivelyAsync(prioritizedSections, profile, tokenBudget, buildId);
            result.SelectedSections = assemblyResult.SelectedSections;
            result.RejectedSections = assemblyResult.RejectedSections;
            
            await RecordBuildStep(result, "ProgressiveAssembly", $"Selected {assemblyResult.SelectedSections.Count} sections, rejected {assemblyResult.RejectedSections.Count}");

            // Step 4: Build final prompt
            var finalPrompt = await BuildFinalPromptAsync(userQuestion, profile, assemblyResult.SelectedSections, buildId);
            result.FinalPrompt = finalPrompt;
            result.FinalTokenCount = _tokenBudgetManager.CountTokens(finalPrompt);
            
            await RecordBuildStep(result, "FinalPromptAssembly", $"Built final prompt: {result.FinalTokenCount} tokens");

            // Step 5: Validate and optimize
            var validationResult = await ValidateAndOptimizePromptAsync(result, tokenBudget);
            if (validationResult.RequiresOptimization)
            {
                result = await OptimizePromptAsync(result, validationResult.OptimizationSuggestions);
                await RecordBuildStep(result, "PromptOptimization", $"Applied {validationResult.OptimizationSuggestions.Count} optimizations");
            }

            result.BuildDuration = (DateTime.UtcNow - startTime).TotalMilliseconds;
            result.TokenUtilization = tokenBudget.AvailableContextTokens > 0 ? 
                (double)result.FinalTokenCount / tokenBudget.MaxTotalTokens : 0;

            await RecordBuildMetricsAsync("progressive_build", result);

            _logger.LogInformation("Progressive build {BuildId} completed in {Duration}ms: {TokenCount}/{MaxTokens} tokens ({Utilization:P1})",
                buildId, result.BuildDuration, result.FinalTokenCount, maxTokens, result.TokenUtilization);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in progressive context building {BuildId}", buildId);
            return CreateFallbackResult(buildId, userQuestion, profile.Intent.Type, ex.Message);
        }
    }

    public async Task<ContextAdaptationResult> AdaptContextForFeedbackAsync(
        ProgressiveBuildResult originalResult,
        UserFeedback feedback)
    {
        var adaptationId = Guid.NewGuid().ToString("N")[..8];
        var startTime = DateTime.UtcNow;

        _logger.LogInformation("Adapting context {AdaptationId} based on feedback for build {BuildId}", 
            adaptationId, originalResult.BuildId);

        try
        {
            var adaptationResult = new ContextAdaptationResult
            {
                AdaptationId = adaptationId,
                OriginalBuildId = originalResult.BuildId,
                FeedbackType = feedback.Type,
                AdaptationSteps = new List<AdaptationStep>()
            };

            // Analyze feedback and determine adaptation strategy
            var adaptationStrategy = AnalyzeFeedbackAndDetermineStrategy(feedback, originalResult);
            adaptationResult.Strategy = adaptationStrategy;

            await RecordAdaptationStep(adaptationResult, "FeedbackAnalysis", $"Strategy: {adaptationStrategy}");

            // Apply adaptation based on strategy
            var adaptedSections = await ApplyAdaptationStrategyAsync(originalResult.SelectedSections, adaptationStrategy, feedback);
            adaptationResult.AdaptedSections = adaptedSections;

            await RecordAdaptationStep(adaptationResult, "SectionAdaptation", $"Adapted {adaptedSections.Count} sections");

            // Rebuild prompt with adapted context
            var adaptedPrompt = await RebuildPromptWithAdaptedContextAsync(
                originalResult.UserQuestion, 
                originalResult.IntentType, 
                adaptedSections);
            
            adaptationResult.AdaptedPrompt = adaptedPrompt;
            adaptationResult.AdaptedTokenCount = _tokenBudgetManager.CountTokens(adaptedPrompt);

            await RecordAdaptationStep(adaptationResult, "PromptRebuild", $"Rebuilt prompt: {adaptationResult.AdaptedTokenCount} tokens");

            adaptationResult.AdaptationDuration = (DateTime.UtcNow - startTime).TotalMilliseconds;
            adaptationResult.TokenDifference = adaptationResult.AdaptedTokenCount - originalResult.FinalTokenCount;

            _logger.LogInformation("Context adaptation {AdaptationId} completed in {Duration}ms: {TokenDiff:+#;-#;0} token change",
                adaptationId, adaptationResult.AdaptationDuration, adaptationResult.TokenDifference);

            return adaptationResult;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in context adaptation {AdaptationId}", adaptationId);
            return new ContextAdaptationResult 
            { 
                AdaptationId = adaptationId, 
                OriginalBuildId = originalResult.BuildId,
                ErrorMessage = ex.Message 
            };
        }
    }

    public async Task<ProgressiveBuildAnalysisReport> GenerateBuildAnalysisReportAsync(
        IntentType? intentFilter = null,
        TimeSpan? timeWindow = null)
    {
        var report = new ProgressiveBuildAnalysisReport
        {
            GeneratedAt = DateTime.UtcNow,
            IntentFilter = intentFilter,
            TimeWindow = timeWindow ?? TimeSpan.FromDays(7)
        };

        lock (_metricsLock)
        {
            foreach (var (operation, metrics) in _buildMetrics)
            {
                var avgDuration = metrics.TotalBuilds > 0 ? metrics.TotalDuration / metrics.TotalBuilds : 0;
                var avgTokens = metrics.TotalBuilds > 0 ? (double)metrics.TotalTokens / metrics.TotalBuilds : 0;
                var avgUtilization = metrics.TotalBuilds > 0 ? metrics.TotalUtilization / metrics.TotalBuilds : 0;

                report.BuildAnalytics.Add(new ProgressiveBuildAnalytics
                {
                    OperationType = operation,
                    TotalBuilds = metrics.TotalBuilds,
                    AverageDuration = avgDuration,
                    AverageTokenCount = avgTokens,
                    AverageUtilization = avgUtilization,
                    SuccessRate = metrics.TotalBuilds > 0 ? (double)metrics.SuccessfulBuilds / metrics.TotalBuilds : 0,
                    LastBuild = metrics.LastBuild
                });
            }
        }

        return report;
    }

    private async Task<ContextAssemblyResult> AssembleContextProgressivelyAsync(
        List<ContextSection> prioritizedSections,
        BusinessContextProfile profile,
        TokenBudget tokenBudget,
        string buildId)
    {
        var result = new ContextAssemblyResult
        {
            SelectedSections = new List<ContextSection>(),
            RejectedSections = new List<ContextSection>()
        };

        var currentTokens = 0;
        var template = GetPromptTemplate(profile.Intent.Type);

        // Progressive assembly by section type priority
        foreach (var sectionType in template.ContextSections)
        {
            var sectionsOfType = prioritizedSections.Where(s => MapSectionTypeToCategory(s.Type) == sectionType).ToList();
            
            foreach (var section in sectionsOfType.OrderByDescending(s => s.RelevanceScore))
            {
                if (currentTokens + section.TokenCount <= tokenBudget.AvailableContextTokens)
                {
                    result.SelectedSections.Add(section);
                    currentTokens += section.TokenCount;
                    
                    _logger.LogDebug("Build {BuildId}: Added {SectionType} section ({Tokens} tokens, {Total} total)",
                        buildId, section.Type, section.TokenCount, currentTokens);
                }
                else
                {
                    result.RejectedSections.Add(section);
                    
                    _logger.LogDebug("Build {BuildId}: Rejected {SectionType} section ({Tokens} tokens, would exceed budget)",
                        buildId, section.Type, section.TokenCount);
                }
            }
        }

        return result;
    }

    private async Task<string> BuildFinalPromptAsync(
        string userQuestion,
        BusinessContextProfile profile,
        List<ContextSection> selectedSections,
        string buildId)
    {
        var template = GetPromptTemplate(profile.Intent.Type);
        var promptBuilder = new StringBuilder();

        // System prompt
        promptBuilder.AppendLine(template.SystemPrompt);
        promptBuilder.AppendLine();

        // Context sections in order
        foreach (var sectionType in template.ContextSections)
        {
            var sectionsOfType = selectedSections.Where(s => MapSectionTypeToCategory(s.Type) == sectionType).ToList();
            
            if (sectionsOfType.Any())
            {
                promptBuilder.AppendLine($"## {sectionType.ToUpper()} CONTEXT");
                
                foreach (var section in sectionsOfType)
                {
                    promptBuilder.AppendLine(section.Content);
                    promptBuilder.AppendLine();
                }
            }
        }

        // User question
        promptBuilder.AppendLine("## USER QUESTION");
        promptBuilder.AppendLine(userQuestion);
        promptBuilder.AppendLine();

        // Closing instructions
        promptBuilder.AppendLine("## INSTRUCTIONS");
        promptBuilder.AppendLine(template.ClosingInstructions);

        return promptBuilder.ToString();
    }

    private async Task<PromptValidationResult> ValidateAndOptimizePromptAsync(
        ProgressiveBuildResult result,
        TokenBudget tokenBudget)
    {
        var validationResult = new PromptValidationResult
        {
            RequiresOptimization = false,
            OptimizationSuggestions = new List<string>()
        };

        // Check token budget utilization
        if (result.TokenUtilization < 0.7)
        {
            validationResult.RequiresOptimization = true;
            validationResult.OptimizationSuggestions.Add("Low token utilization - consider adding more context");
        }

        if (result.TokenUtilization > 0.95)
        {
            validationResult.RequiresOptimization = true;
            validationResult.OptimizationSuggestions.Add("High token utilization - consider reducing context");
        }

        // Check section balance
        var sectionTypes = result.SelectedSections.GroupBy(s => s.Type).ToDictionary(g => g.Key, g => g.Count());
        
        if (!sectionTypes.ContainsKey("table_definition"))
        {
            validationResult.RequiresOptimization = true;
            validationResult.OptimizationSuggestions.Add("Missing table definitions - essential for SQL generation");
        }

        return validationResult;
    }

    private async Task<ProgressiveBuildResult> OptimizePromptAsync(
        ProgressiveBuildResult result,
        List<string> optimizationSuggestions)
    {
        // Apply optimizations based on suggestions
        foreach (var suggestion in optimizationSuggestions)
        {
            if (suggestion.Contains("Low token utilization"))
            {
                // Try to add more context from rejected sections
                var additionalSections = result.RejectedSections
                    .OrderByDescending(s => s.RelevanceScore)
                    .Take(3)
                    .ToList();

                result.SelectedSections.AddRange(additionalSections);
                result.RejectedSections.RemoveAll(s => additionalSections.Contains(s));
            }
            else if (suggestion.Contains("High token utilization"))
            {
                // Remove lowest relevance sections
                var sectionsToRemove = result.SelectedSections
                    .OrderBy(s => s.RelevanceScore)
                    .Take(2)
                    .ToList();

                result.RejectedSections.AddRange(sectionsToRemove);
                result.SelectedSections.RemoveAll(s => sectionsToRemove.Contains(s));
            }
        }

        // Rebuild final prompt
        result.FinalPrompt = await BuildFinalPromptAsync(result.UserQuestion, 
            new BusinessContextProfile { Intent = new QueryIntent { Type = result.IntentType } }, 
            result.SelectedSections, result.BuildId);
        
        result.FinalTokenCount = _tokenBudgetManager.CountTokens(result.FinalPrompt);

        return result;
    }

    private AdaptationStrategy AnalyzeFeedbackAndDetermineStrategy(UserFeedback feedback, ProgressiveBuildResult originalResult)
    {
        return feedback.Type switch
        {
            FeedbackType.TooMuchContext => AdaptationStrategy.ReduceContext,
            FeedbackType.TooLittleContext => AdaptationStrategy.ExpandContext,
            FeedbackType.IrrelevantContext => AdaptationStrategy.RefineRelevance,
            FeedbackType.MissingInformation => AdaptationStrategy.AddSpecificContext,
            FeedbackType.IncorrectSQL => AdaptationStrategy.ImproveExamples,
            _ => AdaptationStrategy.Balanced
        };
    }

    private async Task<List<ContextSection>> ApplyAdaptationStrategyAsync(
        List<ContextSection> originalSections,
        AdaptationStrategy strategy,
        UserFeedback feedback)
    {
        return strategy switch
        {
            AdaptationStrategy.ReduceContext => originalSections.OrderByDescending(s => s.RelevanceScore).Take(originalSections.Count / 2).ToList(),
            AdaptationStrategy.ExpandContext => originalSections, // Would add more sections from rejected list
            AdaptationStrategy.RefineRelevance => originalSections.Where(s => s.RelevanceScore > 0.7).ToList(),
            AdaptationStrategy.AddSpecificContext => originalSections, // Would add specific sections based on feedback
            AdaptationStrategy.ImproveExamples => originalSections.Where(s => s.Type != "example").Concat(
                originalSections.Where(s => s.Type == "example").OrderByDescending(s => s.RelevanceScore).Take(2)).ToList(),
            _ => originalSections
        };
    }

    private async Task<string> RebuildPromptWithAdaptedContextAsync(
        string userQuestion,
        IntentType intentType,
        List<ContextSection> adaptedSections)
    {
        var profile = new BusinessContextProfile { Intent = new QueryIntent { Type = intentType } };
        return await BuildFinalPromptAsync(userQuestion, profile, adaptedSections, "adapted");
    }

    private PromptTemplate GetPromptTemplate(IntentType intentType)
    {
        return PromptTemplates.GetValueOrDefault(intentType, PromptTemplates[IntentType.Analytical]);
    }

    private string MapSectionTypeToCategory(string sectionType)
    {
        return sectionType switch
        {
            "table_definition" => "schema",
            "column_definition" => "schema",
            "relationship" => "relationships",
            "business_rule" => "rules",
            "example" => "examples",
            "glossary" => "glossary",
            _ => "business"
        };
    }

    private ProgressiveBuildResult CreateFallbackResult(string buildId, string userQuestion, IntentType intentType, string errorMessage)
    {
        return new ProgressiveBuildResult
        {
            BuildId = buildId,
            IntentType = intentType,
            UserQuestion = userQuestion,
            FinalPrompt = $"Generate SQL for: {userQuestion}",
            FinalTokenCount = 50,
            ErrorMessage = errorMessage,
            BuildSteps = new List<ProgressiveBuildStep>
            {
                new() { StepName = "Fallback", Description = "Created fallback prompt due to error", Timestamp = DateTime.UtcNow }
            }
        };
    }

    private async Task RecordBuildStep(ProgressiveBuildResult result, string stepName, string description)
    {
        result.BuildSteps.Add(new ProgressiveBuildStep
        {
            StepName = stepName,
            Description = description,
            Timestamp = DateTime.UtcNow
        });
    }

    private async Task RecordAdaptationStep(ContextAdaptationResult result, string stepName, string description)
    {
        result.AdaptationSteps.Add(new AdaptationStep
        {
            StepName = stepName,
            Description = description,
            Timestamp = DateTime.UtcNow
        });
    }

    private async Task RecordBuildMetricsAsync(string operation, ProgressiveBuildResult result)
    {
        lock (_metricsLock)
        {
            if (!_buildMetrics.ContainsKey(operation))
            {
                _buildMetrics[operation] = new ProgressiveBuildMetrics();
            }

            var metrics = _buildMetrics[operation];
            metrics.TotalBuilds++;
            metrics.TotalDuration += result.BuildDuration;
            metrics.TotalTokens += result.FinalTokenCount;
            metrics.TotalUtilization += result.TokenUtilization;
            
            if (string.IsNullOrEmpty(result.ErrorMessage))
            {
                metrics.SuccessfulBuilds++;
            }
            
            metrics.LastBuild = DateTime.UtcNow;
        }
    }
}

// Supporting data structures
public record PromptTemplate
{
    public string Name { get; init; } = string.Empty;
    public string SystemPrompt { get; init; } = string.Empty;
    public string[] ContextSections { get; init; } = Array.Empty<string>();
    public string ClosingInstructions { get; init; } = string.Empty;
}

public record ContextAssemblyResult
{
    public List<ContextSection> SelectedSections { get; init; } = new();
    public List<ContextSection> RejectedSections { get; init; } = new();
}

public record PromptValidationResult
{
    public bool RequiresOptimization { get; set; }
    public List<string> OptimizationSuggestions { get; set; } = new();
}

public enum AdaptationStrategy
{
    ReduceContext,
    ExpandContext,
    RefineRelevance,
    AddSpecificContext,
    ImproveExamples,
    Balanced
}

public enum FeedbackType
{
    TooMuchContext,
    TooLittleContext,
    IrrelevantContext,
    MissingInformation,
    IncorrectSQL,
    Perfect
}

public record UserFeedback
{
    public FeedbackType Type { get; init; }
    public string Description { get; init; } = string.Empty;
    public List<string> SpecificIssues { get; init; } = new();
}

public record ProgressiveBuildResult
{
    public string BuildId { get; set; } = string.Empty;
    public IntentType IntentType { get; set; }
    public string UserQuestion { get; set; } = string.Empty;
    public int MaxTokens { get; set; }
    public TokenBudget? TokenBudget { get; set; }
    public List<ContextSection> SelectedSections { get; set; } = new();
    public List<ContextSection> RejectedSections { get; set; } = new();
    public string FinalPrompt { get; set; } = string.Empty;
    public int FinalTokenCount { get; set; }
    public double TokenUtilization { get; set; }
    public double BuildDuration { get; set; }
    public List<ProgressiveBuildStep> BuildSteps { get; set; } = new();
    public string? ErrorMessage { get; set; }
}

public record ProgressiveBuildStep
{
    public string StepName { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public DateTime Timestamp { get; init; }
}

public record ContextAdaptationResult
{
    public string AdaptationId { get; set; } = string.Empty;
    public string OriginalBuildId { get; set; } = string.Empty;
    public FeedbackType FeedbackType { get; set; }
    public AdaptationStrategy Strategy { get; set; }
    public List<ContextSection> AdaptedSections { get; set; } = new();
    public string AdaptedPrompt { get; set; } = string.Empty;
    public int AdaptedTokenCount { get; set; }
    public int TokenDifference { get; set; }
    public double AdaptationDuration { get; set; }
    public List<AdaptationStep> AdaptationSteps { get; set; } = new();
    public string? ErrorMessage { get; set; }
}

public record AdaptationStep
{
    public string StepName { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public DateTime Timestamp { get; init; }
}

public class ProgressiveBuildMetrics
{
    public int TotalBuilds { get; set; }
    public int SuccessfulBuilds { get; set; }
    public double TotalDuration { get; set; }
    public int TotalTokens { get; set; }
    public double TotalUtilization { get; set; }
    public DateTime LastBuild { get; set; } = DateTime.UtcNow;
}

public record ProgressiveBuildAnalysisReport
{
    public DateTime GeneratedAt { get; init; }
    public IntentType? IntentFilter { get; init; }
    public TimeSpan TimeWindow { get; init; }
    public List<ProgressiveBuildAnalytics> BuildAnalytics { get; init; } = new();
}

public record ProgressiveBuildAnalytics
{
    public string OperationType { get; init; } = string.Empty;
    public int TotalBuilds { get; init; }
    public double AverageDuration { get; init; }
    public double AverageTokenCount { get; init; }
    public double AverageUtilization { get; init; }
    public double SuccessRate { get; init; }
    public DateTime LastBuild { get; init; }
}

public interface IProgressiveContextBuilder
{
    Task<ProgressiveBuildResult> BuildOptimalPromptAsync(string userQuestion, BusinessContextProfile profile, ContextualBusinessSchema schema, int maxTokens = 4000, int reservedResponseTokens = 500);
    Task<ContextAdaptationResult> AdaptContextForFeedbackAsync(ProgressiveBuildResult originalResult, UserFeedback feedback);
    Task<ProgressiveBuildAnalysisReport> GenerateBuildAnalysisReportAsync(IntentType? intentFilter = null, TimeSpan? timeWindow = null);
}
