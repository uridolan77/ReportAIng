using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using BIReportingCopilot.Infrastructure.Data;
using System.Text;
using System.Text.RegularExpressions;

namespace BIReportingCopilot.Infrastructure.AI;

/// <summary>
/// Optimizes prompts based on historical feedback and learning patterns
/// </summary>
public class PromptOptimizer
{
    private readonly BICopilotContext _context;
    private readonly ILogger _logger;
    private readonly Dictionary<string, OptimizationRule> _optimizationRules;

    public PromptOptimizer(BICopilotContext context, ILogger logger)
    {
        _context = context;
        _logger = logger;
        _optimizationRules = InitializeOptimizationRules();
    }

    /// <summary>
    /// Optimize a prompt based on learning insights
    /// </summary>
    public async Task<string> OptimizePromptAsync(string originalPrompt, LearningInsights insights)
    {
        try
        {
            var optimizedPrompt = originalPrompt;

            // Apply learning-based optimizations
            optimizedPrompt = ApplyLearningOptimizations(optimizedPrompt, insights);

            // Apply rule-based optimizations
            optimizedPrompt = ApplyRuleBasedOptimizations(optimizedPrompt);

            // Add context enhancements
            optimizedPrompt = await AddContextEnhancementsAsync(optimizedPrompt);

            _logger.LogDebug("Prompt optimized from {OriginalLength} to {OptimizedLength} characters",
                originalPrompt.Length, optimizedPrompt.Length);

            return optimizedPrompt;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error optimizing prompt, using original");
            return originalPrompt;
        }
    }

    /// <summary>
    /// Enhance insight based on context and learning
    /// </summary>
    public async Task<string> EnhanceInsightAsync(string baseInsight, InsightContext context, object[] data)
    {
        try
        {
            var enhancedInsight = new StringBuilder(baseInsight);

            // Add contextual hints
            if (context.ContextualHints.Any())
            {
                enhancedInsight.AppendLine("\n**Additional Context:**");
                foreach (var hint in context.ContextualHints.Take(3))
                {
                    enhancedInsight.AppendLine($"• {hint.Trim()}");
                }
            }

            // Add pattern-specific insights
            var patternInsights = await GetPatternSpecificInsightsAsync(context.QueryPattern);
            if (patternInsights.Any())
            {
                enhancedInsight.AppendLine("\n**Pattern-Specific Insights:**");
                foreach (var insight in patternInsights.Take(2))
                {
                    enhancedInsight.AppendLine($"• {insight}");
                }
            }

            // Add data quality observations
            var dataQualityInsights = AnalyzeDataQuality(data);
            if (dataQualityInsights.Any())
            {
                enhancedInsight.AppendLine("\n**Data Quality Notes:**");
                foreach (var insight in dataQualityInsights)
                {
                    enhancedInsight.AppendLine($"• {insight}");
                }
            }

            return enhancedInsight.ToString();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error enhancing insight, using base insight");
            return baseInsight;
        }
    }

    private string ApplyLearningOptimizations(string prompt, LearningInsights insights)
    {
        var optimizedPrompt = prompt;

        // Add successful patterns as context
        if (insights.SuccessfulPatterns.Any())
        {
            var successfulContext = string.Join(", ", insights.SuccessfulPatterns.Take(3));
            optimizedPrompt = $"Context: Focus on {successfulContext}. Query: {optimizedPrompt}";
        }

        // Add optimization suggestions as guidance
        if (insights.OptimizationSuggestions.Any())
        {
            var suggestions = string.Join("; ", insights.OptimizationSuggestions.Take(2));
            optimizedPrompt += $" (Guidelines: {suggestions})";
        }

        // Warn about common mistakes
        if (insights.CommonMistakes.Any())
        {
            var mistakes = string.Join(", ", insights.CommonMistakes.Take(2));
            optimizedPrompt += $" (Avoid: {mistakes})";
        }

        return optimizedPrompt;
    }

    private string ApplyRuleBasedOptimizations(string prompt)
    {
        var optimizedPrompt = prompt;

        foreach (var rule in _optimizationRules.Values)
        {
            if (rule.Condition(optimizedPrompt))
            {
                optimizedPrompt = rule.Transformation(optimizedPrompt);
            }
        }

        return optimizedPrompt;
    }

    private async Task<string> AddContextEnhancementsAsync(string prompt)
    {
        try
        {
            // Add schema context hints
            var schemaHints = await GetRelevantSchemaHintsAsync(prompt);
            if (schemaHints.Any())
            {
                var hints = string.Join(", ", schemaHints.Take(3));
                prompt += $" (Schema context: {hints})";
            }

            // Add temporal context if relevant
            if (ContainsTemporalKeywords(prompt))
            {
                prompt += " (Note: Use appropriate date/time functions and consider timezone)";
            }

            // Add aggregation context if relevant
            if (ContainsAggregationKeywords(prompt))
            {
                prompt += " (Note: Consider GROUP BY clauses and NULL handling)";
            }

            return prompt;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error adding context enhancements");
            return prompt;
        }
    }

    private async Task<List<string>> GetRelevantSchemaHintsAsync(string prompt)
    {
        try
        {
            // Extract potential table/column names from prompt
            var words = ExtractWords(prompt);

            var hints = await _context.BusinessTableInfo
                .Where(t => words.Any(w => t.TableName.Contains(w) || t.BusinessPurpose.Contains(w)))
                .Select(t => $"{t.TableName} ({t.BusinessPurpose})")
                .Take(3)
                .ToListAsync();

            return hints;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error getting schema hints");
            return new List<string>();
        }
    }

    private async Task<List<string>> GetPatternSpecificInsightsAsync(string queryPattern)
    {
        try
        {
            var insights = await _context.AIFeedbackEntries
                .Where(f => f.Category == queryPattern && f.Rating >= 4 && !string.IsNullOrEmpty(f.Comments))
                .Select(f => f.Comments!)
                .Distinct()
                .Take(5)
                .ToListAsync();

            return insights.Where(i => i.Length > 20 && i.Length < 200).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error getting pattern-specific insights");
            return new List<string>();
        }
    }

    private List<string> AnalyzeDataQuality(object[] data)
    {
        var insights = new List<string>();

        if (data.Length == 0)
        {
            insights.Add("No data returned - consider checking filters or data availability");
            return insights;
        }

        if (data.Length == 1)
        {
            insights.Add("Single result returned - this might indicate very specific filtering");
        }
        else if (data.Length > 1000)
        {
            insights.Add("Large result set returned - consider adding filters for better performance");
        }

        // Check for potential data quality issues (simplified)
        var sampleData = data.Take(10).ToArray();
        if (sampleData.Any(d => d?.ToString()?.Contains("null") == true))
        {
            insights.Add("Some null values detected - consider handling missing data appropriately");
        }

        return insights;
    }

    private Dictionary<string, OptimizationRule> InitializeOptimizationRules()
    {
        return new Dictionary<string, OptimizationRule>
        {
            ["add_table_context"] = new OptimizationRule
            {
                Name = "Add Table Context",
                Condition = prompt => !prompt.Contains("table") && !prompt.Contains("from"),
                Transformation = prompt => $"Generate SQL query for the appropriate table: {prompt}"
            },
            ["clarify_aggregation"] = new OptimizationRule
            {
                Name = "Clarify Aggregation",
                Condition = prompt => ContainsAggregationKeywords(prompt) && !prompt.Contains("group"),
                Transformation = prompt => $"{prompt} (Include appropriate grouping if needed)"
            },
            ["add_limit_guidance"] = new OptimizationRule
            {
                Name = "Add Limit Guidance",
                Condition = prompt => prompt.ToLower().Contains("show") && !prompt.Contains("limit") && !prompt.Contains("top"),
                Transformation = prompt => $"{prompt} (Consider adding LIMIT/TOP for large datasets)"
            },
            ["temporal_clarity"] = new OptimizationRule
            {
                Name = "Temporal Clarity",
                Condition = prompt => ContainsTemporalKeywords(prompt) && !ContainsDateFormat(prompt),
                Transformation = prompt => $"{prompt} (Specify date format and range clearly)"
            },
            ["join_guidance"] = new OptimizationRule
            {
                Name = "Join Guidance",
                Condition = prompt => ContainsMultipleEntities(prompt) && !prompt.Contains("join"),
                Transformation = prompt => $"{prompt} (Consider relationships between entities)"
            }
        };
    }

    private static bool ContainsTemporalKeywords(string prompt)
    {
        var temporalKeywords = new[] { "date", "time", "day", "month", "year", "recent", "last", "today", "yesterday", "week" };
        return temporalKeywords.Any(keyword => prompt.Contains(keyword, StringComparison.OrdinalIgnoreCase));
    }

    private static bool ContainsAggregationKeywords(string prompt)
    {
        var aggregationKeywords = new[] { "count", "sum", "average", "total", "max", "min", "group" };
        return aggregationKeywords.Any(keyword => prompt.Contains(keyword, StringComparison.OrdinalIgnoreCase));
    }

    private static bool ContainsDateFormat(string prompt)
    {
        return Regex.IsMatch(prompt, @"\d{4}-\d{2}-\d{2}|\d{2}/\d{2}/\d{4}|yyyy|mm|dd", RegexOptions.IgnoreCase);
    }

    private static bool ContainsMultipleEntities(string prompt)
    {
        var entityKeywords = new[] { "customer", "order", "product", "user", "account", "transaction", "invoice", "payment" };
        return entityKeywords.Count(keyword => prompt.Contains(keyword, StringComparison.OrdinalIgnoreCase)) > 1;
    }

    private static List<string> ExtractWords(string text)
    {
        return Regex.Split(text.ToLower(), @"\W+")
            .Where(w => w.Length > 2)
            .ToList();
    }

    private class OptimizationRule
    {
        public string Name { get; set; } = string.Empty;
        public Func<string, bool> Condition { get; set; } = _ => false;
        public Func<string, string> Transformation { get; set; } = s => s;
    }
}
