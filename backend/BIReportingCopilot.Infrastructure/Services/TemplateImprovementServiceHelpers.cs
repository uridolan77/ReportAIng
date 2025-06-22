using BIReportingCopilot.Core.Models.Analytics;
using BIReportingCopilot.Core.Models;
using System.Text;
using System.Text.Json;
using QualityIssue = BIReportingCopilot.Core.Models.Analytics.QualityIssue;

namespace BIReportingCopilot.Infrastructure.Services;

/// <summary>
/// Helper methods for Template Improvement Service
/// </summary>
public partial class TemplateImprovementService
{
    private async Task<List<TemplatePerformanceMetricsEntity>> GetUnderperformingTemplatesAsync(
        double performanceThreshold, 
        int minDataPoints, 
        CancellationToken cancellationToken)
    {
        var allTemplates = await _templateRepository.GetAllAsync(cancellationToken);
        var underperforming = new List<TemplatePerformanceMetricsEntity>();

        foreach (var template in allTemplates)
        {
            var performance = await _performanceRepository.GetByTemplateIdAsync(template.Id, cancellationToken);
            if (performance != null && 
                performance.TotalUsages >= minDataPoints && 
                (double)performance.SuccessRate < performanceThreshold)
            {
                underperforming.Add(performance);
            }
        }

        return underperforming;
    }

    private async Task<OptimizationContent> ApplyOptimizationStrategyAsync(
        string originalContent, 
        OptimizationStrategy strategy, 
        TemplatePerformanceMetricsEntity? performance, 
        CancellationToken cancellationToken)
    {
        var changes = new List<OptimizationChange>();
        var optimizedContent = originalContent;

        switch (strategy)
        {
            case OptimizationStrategy.PerformanceFocused:
                optimizedContent = OptimizeForPerformance(originalContent, changes);
                break;
            case OptimizationStrategy.AccuracyFocused:
                optimizedContent = OptimizeForAccuracy(originalContent, changes);
                break;
            case OptimizationStrategy.UserSatisfactionFocused:
                optimizedContent = OptimizeForUserSatisfaction(originalContent, changes);
                break;
            case OptimizationStrategy.ResponseTimeFocused:
                optimizedContent = OptimizeForResponseTime(originalContent, changes);
                break;
            case OptimizationStrategy.Balanced:
                optimizedContent = OptimizeBalanced(originalContent, changes);
                break;
        }

        return new OptimizationContent
        {
            Content = optimizedContent,
            Changes = changes,
            ExpectedImprovement = CalculateExpectedImprovement(changes),
            ConfidenceScore = CalculateOptimizationConfidence(changes),
            Reasoning = GenerateOptimizationReasoning(strategy, changes),
            MetricPredictions = PredictOptimizationMetrics(changes, performance)
        };
    }

    private string OptimizeForPerformance(string content, List<OptimizationChange> changes)
    {
        var optimized = content;

        // Simplify complex instructions
        if (content.Length > 1000)
        {
            optimized = content.Substring(0, 800) + "...";
            changes.Add(new OptimizationChange
            {
                ChangeType = "Content Simplification",
                Description = "Reduced template length for better performance",
                ImpactScore = 0.7m
            });
        }

        // Add performance-focused instructions
        if (!content.Contains("concise"))
        {
            optimized = "Be concise and direct. " + optimized;
            changes.Add(new OptimizationChange
            {
                ChangeType = "Performance Instruction",
                Description = "Added conciseness instruction",
                ImpactScore = 0.5m
            });
        }

        return optimized;
    }

    private string OptimizeForAccuracy(string content, List<OptimizationChange> changes)
    {
        var optimized = content;

        // Add accuracy instructions
        if (!content.Contains("accurate"))
        {
            optimized = "Provide accurate and precise information. " + optimized;
            changes.Add(new OptimizationChange
            {
                ChangeType = "Accuracy Enhancement",
                Description = "Added accuracy instruction",
                ImpactScore = 0.8m
            });
        }

        // Add verification steps
        if (!content.Contains("verify"))
        {
            optimized += "\n\nVerify your response for accuracy before providing it.";
            changes.Add(new OptimizationChange
            {
                ChangeType = "Verification Step",
                Description = "Added verification instruction",
                ImpactScore = 0.6m
            });
        }

        return optimized;
    }

    private string OptimizeForUserSatisfaction(string content, List<OptimizationChange> changes)
    {
        var optimized = content;

        // Add user-friendly language
        if (!content.Contains("helpful"))
        {
            optimized = "Be helpful and user-friendly. " + optimized;
            changes.Add(new OptimizationChange
            {
                ChangeType = "User Experience",
                Description = "Added user-friendly instruction",
                ImpactScore = 0.7m
            });
        }

        // Add examples if missing
        if (!content.Contains("example"))
        {
            optimized += "\n\nProvide examples when helpful.";
            changes.Add(new OptimizationChange
            {
                ChangeType = "Example Addition",
                Description = "Added example instruction",
                ImpactScore = 0.8m
            });
        }

        return optimized;
    }

    private string OptimizeForResponseTime(string content, List<OptimizationChange> changes)
    {
        var optimized = content;

        // Reduce complexity
        var sentences = content.Split('.').Where(s => !string.IsNullOrWhiteSpace(s)).ToArray();
        if (sentences.Length > 5)
        {
            optimized = string.Join(". ", sentences.Take(5)) + ".";
            changes.Add(new OptimizationChange
            {
                ChangeType = "Complexity Reduction",
                Description = "Reduced number of instructions for faster processing",
                ImpactScore = 0.9m
            });
        }

        return optimized;
    }

    private string OptimizeBalanced(string content, List<OptimizationChange> changes)
    {
        var optimized = content;

        // Apply moderate optimizations from all strategies
        if (!content.Contains("clear") && !content.Contains("accurate"))
        {
            optimized = "Provide clear and accurate responses. " + optimized;
            changes.Add(new OptimizationChange
            {
                ChangeType = "Balanced Enhancement",
                Description = "Added clarity and accuracy instruction",
                ImpactScore = 0.7m
            });
        }

        return optimized;
    }

    private decimal CalculateExpectedImprovement(List<OptimizationChange> changes)
    {
        return changes.Any() ? changes.Average(c => c.ImpactScore) * 100 : 0;
    }

    private decimal CalculateOptimizationConfidence(List<OptimizationChange> changes)
    {
        var baseConfidence = 0.6m;
        var changeBonus = changes.Count * 0.1m;
        return Math.Min(0.95m, baseConfidence + changeBonus);
    }

    private string GenerateOptimizationReasoning(OptimizationStrategy strategy, List<OptimizationChange> changes)
    {
        var reasoning = $"Applied {strategy} optimization strategy. ";
        reasoning += $"Made {changes.Count} changes: ";
        reasoning += string.Join(", ", changes.Select(c => c.ChangeType));
        return reasoning;
    }

    private Dictionary<string, decimal> PredictOptimizationMetrics(
        List<OptimizationChange> changes, 
        TemplatePerformanceMetricsEntity? performance)
    {
        var predictions = new Dictionary<string, decimal>();

        if (performance != null)
        {
            var improvementFactor = 1 + (CalculateExpectedImprovement(changes) / 100);
            predictions["PredictedSuccessRate"] = Math.Min(1.0m, performance.SuccessRate * improvementFactor);
            predictions["PredictedConfidenceScore"] = Math.Min(1.0m, performance.AverageConfidenceScore * improvementFactor);
            predictions["PredictedResponseTime"] = performance.AverageProcessingTimeMs * (2 - improvementFactor);
        }

        return predictions;
    }

    private List<FeedbackTheme> GenerateFeedbackThemes(TemplatePerformanceMetricsEntity? performance)
    {
        var themes = new List<FeedbackTheme>();

        if (performance != null)
        {
            if (performance.SuccessRate < 0.8m)
            {
                themes.Add(new FeedbackTheme
                {
                    Theme = "Accuracy Issues",
                    Frequency = (int)(performance.TotalUsages * (1 - performance.SuccessRate)),
                    Sentiment = -0.3m,
                    ExampleComments = new List<string> { "Results not accurate", "Needs improvement" }
                });
            }

            if (performance.AverageUserRating < 4.0m)
            {
                themes.Add(new FeedbackTheme
                {
                    Theme = "User Experience",
                    Frequency = (int)(performance.TotalUsages * 0.3),
                    Sentiment = -0.1m,
                    ExampleComments = new List<string> { "Could be more helpful", "Needs better examples" }
                });
            }
        }

        return themes;
    }

    private Dictionary<string, decimal> GenerateSentimentDistribution(TemplatePerformanceMetricsEntity? performance)
    {
        var distribution = new Dictionary<string, decimal>();

        if (performance != null)
        {
            var positiveRatio = performance.SuccessRate;
            distribution["Positive"] = positiveRatio * 100;
            distribution["Neutral"] = (1 - positiveRatio) * 50;
            distribution["Negative"] = (1 - positiveRatio) * 50;
        }

        return distribution;
    }

    private List<ImprovementSuggestionFromFeedback> GenerateImprovementSuggestionsFromFeedback(
        TemplatePerformanceMetricsEntity? performance)
    {
        var suggestions = new List<ImprovementSuggestionFromFeedback>();

        if (performance != null)
        {
            if (performance.SuccessRate < 0.8m)
            {
                suggestions.Add(new ImprovementSuggestionFromFeedback
                {
                    Suggestion = "Improve accuracy by adding more specific instructions",
                    Frequency = (int)(performance.TotalUsages * 0.4),
                    Priority = 0.8m,
                    SupportingComments = new List<string> { "More specific guidance needed" }
                });
            }
        }

        return suggestions;
    }

    private async Task<TemplateVariant> GenerateVariantAsync(
        PromptTemplateEntity template,
        VariantType variantType,
        TemplatePerformanceMetricsEntity? performance,
        CancellationToken cancellationToken)
    {
        var variant = new TemplateVariant
        {
            OriginalTemplateKey = template.TemplateKey ?? string.Empty,
            Type = variantType,
            VariantContent = ApplyVariantTransformation(template.Content, variantType),
            ExpectedPerformanceChange = CalculateExpectedVariantPerformance(variantType),
            ConfidenceScore = 0.7m,
            GenerationReasoning = $"Generated {variantType} variant to test different approach",
            GeneratedDate = DateTime.UtcNow,
            GeneratedBy = "ML_System"
        };

        return variant;
    }

    private string ApplyVariantTransformation(string originalContent, VariantType variantType)
    {
        return variantType switch
        {
            VariantType.ContentVariation => "Alternative approach: " + originalContent,
            VariantType.StructureVariation => RearrangeContentStructure(originalContent),
            VariantType.StyleVariation => "In a different style: " + originalContent,
            VariantType.ComplexityVariation => SimplifyContent(originalContent),
            _ => originalContent
        };
    }

    private string RearrangeContentStructure(string content)
    {
        var sentences = content.Split('.').Where(s => !string.IsNullOrWhiteSpace(s)).ToArray();
        if (sentences.Length > 1)
        {
            return string.Join(". ", sentences.Reverse()) + ".";
        }
        return content;
    }

    private string SimplifyContent(string content)
    {
        return content.Replace("complex", "simple").Replace("detailed", "basic");
    }

    private decimal CalculateExpectedVariantPerformance(VariantType variantType)
    {
        return variantType switch
        {
            VariantType.ContentVariation => 0.15m,
            VariantType.StructureVariation => 0.10m,
            VariantType.StyleVariation => 0.08m,
            VariantType.ComplexityVariation => 0.12m,
            _ => 0.05m
        };
    }

    private async Task CreateImprovementSuggestionAsync(TemplateImprovementSuggestion suggestion, CancellationToken cancellationToken)
    {
        var template = await _templateRepository.GetByKeyAsync(suggestion.TemplateKey, cancellationToken);
        if (template == null) return;

        var entity = new TemplateImprovementSuggestionEntity
        {
            TemplateId = template.Id,
            SuggestionType = suggestion.Type.ToString(),
            CurrentVersion = suggestion.CurrentVersion,
            SuggestedChanges = suggestion.SuggestedChanges,
            ReasoningExplanation = suggestion.ReasoningExplanation,
            ExpectedImprovementPercent = suggestion.ExpectedImprovementPercent,
            BasedOnDataPoints = suggestion.BasedOnDataPoints,
            Status = suggestion.Status.ToString(),
            CreatedDate = DateTime.UtcNow,
            TemplateKey = suggestion.TemplateKey,
            TemplateName = suggestion.TemplateName
        };

        await _improvementRepository.CreateAsync(entity, cancellationToken);
    }

    private async Task<List<SuccessfulPattern>> GetSuccessfulPatternsAsync(string intentType, CancellationToken cancellationToken)
    {
        // Simulate successful patterns analysis - in production, this would analyze actual successful templates
        var patterns = new List<SuccessfulPattern>
        {
            new SuccessfulPattern
            {
                Title = "Clear Instructions Pattern",
                Description = "Templates with clear, step-by-step instructions perform better",
                Suggestion = "Add numbered steps and clear action items",
                ExpectedImpact = 0.15m,
                ImplementationEffort = 0.3m,
                BasedOnPatterns = new List<string> { "Step-by-step format", "Action-oriented language" },
                SimilarTemplates = new List<string> { "template_1", "template_2" }
            },
            new SuccessfulPattern
            {
                Title = "Example-Rich Content",
                Description = "Templates with concrete examples show higher success rates",
                Suggestion = "Include 2-3 relevant examples in the template",
                ExpectedImpact = 0.20m,
                ImplementationEffort = 0.4m,
                BasedOnPatterns = new List<string> { "Example inclusion", "Concrete demonstrations" },
                SimilarTemplates = new List<string> { "template_3", "template_4" }
            }
        };

        return patterns;
    }

    private ImprovementType DetermineImprovementType(SuccessfulPattern pattern)
    {
        return pattern.Title.Contains("Example") ? ImprovementType.ExampleAddition :
               pattern.Title.Contains("Instruction") ? ImprovementType.InstructionClarification :
               ImprovementType.ContentOptimization;
    }

    private decimal CalculatePriority(decimal expectedImpact, decimal implementationEffort)
    {
        return expectedImpact / Math.Max(0.1m, implementationEffort);
    }

    private decimal CalculateOverallQualityScore(string templateContent)
    {
        var score = 0.5m; // Base score

        // Length factor
        if (templateContent.Length > 100 && templateContent.Length < 1000)
            score += 0.1m;

        // Structure factor
        if (templateContent.Contains("\n") || templateContent.Contains("."))
            score += 0.1m;

        // Instruction clarity
        if (templateContent.ToLower().Contains("step") || templateContent.ToLower().Contains("example"))
            score += 0.2m;

        // Completeness
        if (templateContent.Length > 200)
            score += 0.1m;

        return Math.Min(1.0m, score);
    }

    private Dictionary<string, decimal> AnalyzeQualityDimensions(string templateContent)
    {
        return new Dictionary<string, decimal>
        {
            ["Clarity"] = CalculateClarityScore(templateContent),
            ["Completeness"] = CalculateCompletenessScore(templateContent),
            ["Structure"] = CalculateStructureScore(templateContent),
            ["Relevance"] = CalculateRelevanceScore(templateContent),
            ["Specificity"] = CalculateSpecificityScore(templateContent)
        };
    }

    private decimal CalculateClarityScore(string content)
    {
        var score = 0.5m;
        if (content.Contains("clear") || content.Contains("specific")) score += 0.2m;
        if (content.Split('.').Length > 2) score += 0.1m;
        return Math.Min(1.0m, score);
    }

    private decimal CalculateCompletenessScore(string content)
    {
        var score = Math.Min(1.0m, content.Length / 500m);
        return score;
    }

    private decimal CalculateStructureScore(string content)
    {
        var score = 0.3m;
        if (content.Contains("\n")) score += 0.3m;
        if (content.Split('.').Length > 3) score += 0.2m;
        if (content.Contains("1.") || content.Contains("•")) score += 0.2m;
        return Math.Min(1.0m, score);
    }

    private decimal CalculateRelevanceScore(string content)
    {
        return 0.7m; // Simplified - would analyze domain relevance
    }

    private decimal CalculateSpecificityScore(string content)
    {
        var score = 0.4m;
        if (content.Contains("example") || content.Contains("specific")) score += 0.3m;
        if (content.Length > 300) score += 0.2m;
        return Math.Min(1.0m, score);
    }

    private List<QualityIssue> IdentifyQualityIssues(string templateContent)
    {
        var issues = new List<QualityIssue>();

        if (templateContent.Length < 50)
        {
            issues.Add(new QualityIssue
            {
                IssueType = "Length",
                Description = "Template is too short",
                Severity = 0.7m,
                Suggestion = "Add more detailed instructions"
            });
        }

        if (!templateContent.Contains("."))
        {
            issues.Add(new QualityIssue
            {
                IssueType = "Structure",
                Description = "Lacks proper sentence structure",
                Severity = 0.5m,
                Suggestion = "Break into clear sentences"
            });
        }

        return issues;
    }

    private List<QualityStrength> IdentifyQualityStrengths(string templateContent)
    {
        var strengths = new List<QualityStrength>();

        if (templateContent.Contains("example"))
        {
            strengths.Add(new QualityStrength
            {
                StrengthType = "Examples",
                Description = "Contains helpful examples",
                ImpactScore = 0.8m
            });
        }

        if (templateContent.Length > 200)
        {
            strengths.Add(new QualityStrength
            {
                StrengthType = "Completeness",
                Description = "Comprehensive content",
                ImpactScore = 0.6m
            });
        }

        return strengths;
    }

    private List<string> GenerateContentImprovementSuggestions(string templateContent)
    {
        var suggestions = new List<string>();

        if (templateContent.Length < 100)
            suggestions.Add("Add more detailed instructions");

        if (!templateContent.Contains("example"))
            suggestions.Add("Include concrete examples");

        if (!templateContent.Contains("\n"))
            suggestions.Add("Improve structure with line breaks");

        return suggestions;
    }

    private ReadabilityMetrics AnalyzeReadability(string templateContent)
    {
        return new ReadabilityMetrics
        {
            OverallScore = CalculateReadabilityScore(templateContent),
            SentenceComplexity = CalculateSentenceComplexity(templateContent),
            VocabularyLevel = CalculateVocabularyLevel(templateContent),
            ReadingLevel = 0.7m,
            ReadingLevelText = "Intermediate"
        };
    }

    private decimal CalculateReadabilityScore(string content)
    {
        var sentences = content.Split('.').Length;
        var words = content.Split(' ').Length;
        var avgWordsPerSentence = sentences > 0 ? words / sentences : 0;

        // Ideal range: 15-20 words per sentence
        if (avgWordsPerSentence >= 15 && avgWordsPerSentence <= 20)
            return 0.9m;
        else if (avgWordsPerSentence >= 10 && avgWordsPerSentence <= 25)
            return 0.7m;
        else
            return 0.5m;
    }

    private decimal CalculateSentenceComplexity(string content)
    {
        var sentences = content.Split('.').Where(s => !string.IsNullOrWhiteSpace(s)).ToArray();
        var avgLength = sentences.Any() ? sentences.Average(s => s.Length) : 0;
        return Math.Min(1.0m, (decimal)avgLength / 100);
    }

    private decimal CalculateVocabularyLevel(string content)
    {
        var words = content.Split(' ').Where(w => !string.IsNullOrWhiteSpace(w)).ToArray();
        var uniqueWords = words.Distinct().Count();
        var vocabularyRatio = words.Length > 0 ? (decimal)uniqueWords / words.Length : 0;
        return Math.Min(1.0m, vocabularyRatio * 2);
    }

    private StructureAnalysis AnalyzeStructure(string templateContent)
    {
        return new StructureAnalysis
        {
            OverallScore = CalculateStructureScore(templateContent),
            HasClearSections = templateContent.Contains("\n"),
            HasNumberedSteps = templateContent.Contains("1.") || templateContent.Contains("2."),
            HasBulletPoints = templateContent.Contains("•") || templateContent.Contains("-"),
            ComplexityScore = CalculateComplexityScore(templateContent)
        };
    }

    private decimal CalculateComplexityScore(string content)
    {
        var factors = 0;
        if (content.Length > 500) factors++;
        if (content.Split('.').Length > 5) factors++;
        if (content.Contains("if") || content.Contains("when")) factors++;
        return Math.Min(1.0m, factors / 3m);
    }

    private ContentCompleteness AnalyzeCompleteness(string templateContent)
    {
        return new ContentCompleteness
        {
            OverallScore = CalculateCompletenessScore(templateContent),
            HasInstructions = templateContent.ToLower().Contains("step") || templateContent.ToLower().Contains("do"),
            HasExamples = templateContent.ToLower().Contains("example"),
            HasContext = templateContent.Length > 100,
            MissingElements = IdentifyMissingElements(templateContent)
        };
    }

    private List<string> IdentifyMissingElements(string content)
    {
        var missing = new List<string>();

        if (!content.ToLower().Contains("example"))
            missing.Add("Examples");

        if (!content.Contains("\n"))
            missing.Add("Structure");

        if (content.Length < 100)
            missing.Add("Detail");

        return missing;
    }
}

/// <summary>
/// Helper class for optimization content
/// </summary>
public class OptimizationContent
{
    public string Content { get; set; } = string.Empty;
    public List<OptimizationChange> Changes { get; set; } = new();
    public decimal ExpectedImprovement { get; set; }
    public decimal ConfidenceScore { get; set; }
    public string Reasoning { get; set; } = string.Empty;
    public Dictionary<string, decimal> MetricPredictions { get; set; } = new();
}

/// <summary>
/// Helper class for successful patterns
/// </summary>
public class SuccessfulPattern
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Suggestion { get; set; } = string.Empty;
    public decimal ExpectedImpact { get; set; }
    public decimal ImplementationEffort { get; set; }
    public List<string> BasedOnPatterns { get; set; } = new();
    public List<string> SimilarTemplates { get; set; } = new();
}
