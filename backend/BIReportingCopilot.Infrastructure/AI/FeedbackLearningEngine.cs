using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using BIReportingCopilot.Core.Models;
using BIReportingCopilot.Infrastructure.Data;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace BIReportingCopilot.Infrastructure.AI;

/// <summary>
/// Engine that learns from user feedback to improve AI query generation
/// </summary>
public class FeedbackLearningEngine
{
    private readonly BICopilotContext _context;
    private readonly ILogger _logger;
    private readonly Dictionary<string, LearningPattern> _patternCache;

    public FeedbackLearningEngine(BICopilotContext context, ILogger logger)
    {
        _context = context;
        _logger = logger;
        _patternCache = new Dictionary<string, LearningPattern>();
    }

    /// <summary>
    /// Get learning insights for a given prompt
    /// </summary>
    public async Task<LearningInsights> GetLearningInsightsAsync(string prompt)
    {
        try
        {
            var promptPattern = ExtractPromptPattern(prompt);
            var cacheKey = $"insights_{promptPattern}";

            if (_patternCache.TryGetValue(cacheKey, out var cachedPattern))
            {
                return cachedPattern.Insights;
            }

            // Analyze historical feedback for similar prompts
            var similarPrompts = await FindSimilarPromptsAsync(prompt);
            var feedbackData = await GetFeedbackDataAsync(similarPrompts);

            var insights = new LearningInsights
            {
                PromptPattern = promptPattern,
                SuccessfulPatterns = ExtractSuccessfulPatterns(feedbackData),
                CommonMistakes = ExtractCommonMistakes(feedbackData),
                OptimizationSuggestions = GenerateOptimizationSuggestions(feedbackData),
                ConfidenceModifier = CalculateConfidenceModifier(feedbackData),
                SampleCount = feedbackData.Count
            };

            // Cache the insights
            _patternCache[cacheKey] = new LearningPattern { Insights = insights, LastUpdated = DateTime.UtcNow };

            return insights;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting learning insights for prompt");
            return new LearningInsights { PromptPattern = ExtractPromptPattern(prompt) };
        }
    }

    /// <summary>
    /// Process user feedback to update learning models
    /// </summary>
    public async Task ProcessFeedbackAsync(string originalPrompt, string generatedSQL, QueryFeedback feedback, string userId)
    {
        try
        {
            var feedbackEntry = new AIFeedbackEntry
            {
                OriginalPrompt = originalPrompt,
                GeneratedSQL = generatedSQL,
                Rating = FeedbackLearningEngineExtensions.GetRatingFromFeedback(feedback),
                FeedbackText = feedback.Comments,
                UserId = userId,
                Timestamp = DateTime.UtcNow,
                PromptPattern = ExtractPromptPattern(originalPrompt),
                SQLPattern = ExtractSQLPattern(generatedSQL),
                IsSuccessful = FeedbackLearningEngineExtensions.GetRatingFromFeedback(feedback) >= 4 // Consider 4+ stars as successful
            };

            _context.AIFeedbackEntries.Add(feedbackEntry);
            await _context.SaveChangesAsync();

            // Update learning patterns
            await UpdateLearningPatternsAsync(feedbackEntry);

            _logger.LogInformation("Processed feedback: Rating {Rating}, Pattern: {Pattern}",
                FeedbackLearningEngineExtensions.GetRatingFromFeedback(feedback), feedbackEntry.PromptPattern);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing feedback");
        }
    }

    /// <summary>
    /// Enhance confidence score based on learning
    /// </summary>
    public async Task<double> EnhanceConfidenceWithLearningAsync(
        double baseConfidence,
        string prompt,
        string generatedSQL,
        LearningInsights insights)
    {
        try
        {
            var enhancement = insights.ConfidenceModifier;

            // Additional factors
            if (insights.SampleCount > 10)
            {
                enhancement += 0.1; // More data = higher confidence
            }

            if (insights.SuccessfulPatterns.Any(p => prompt.Contains(p, StringComparison.OrdinalIgnoreCase)))
            {
                enhancement += 0.15; // Matches successful patterns
            }

            if (insights.CommonMistakes.Any(m => generatedSQL.Contains(m, StringComparison.OrdinalIgnoreCase)))
            {
                enhancement -= 0.2; // Contains common mistake patterns
            }

            var enhancedConfidence = Math.Max(0.0, Math.Min(1.0, baseConfidence + enhancement));

            return enhancedConfidence;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error enhancing confidence, using base confidence");
            return baseConfidence;
        }
    }

    /// <summary>
    /// Get personalized query suggestions based on user patterns
    /// </summary>
    public async Task<string[]> GetPersonalizedSuggestionsAsync(string context, SchemaMetadata schema)
    {
        try
        {
            // Get popular patterns from successful queries
            var popularPatterns = await _context.AIFeedbackEntries
                .Where(f => f.IsSuccessful && f.Timestamp > DateTime.UtcNow.AddDays(-30))
                .GroupBy(f => f.PromptPattern)
                .OrderByDescending(g => g.Count())
                .Take(5)
                .Select(g => g.Key)
                .ToListAsync();

            var suggestions = new List<string>();

            foreach (var pattern in popularPatterns)
            {
                var suggestion = GenerateSuggestionFromPattern(pattern, schema);
                if (!string.IsNullOrEmpty(suggestion))
                {
                    suggestions.Add(suggestion);
                }
            }

            return suggestions.ToArray();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error generating personalized suggestions");
            return Array.Empty<string>();
        }
    }

    /// <summary>
    /// Get insight context for enhanced insight generation
    /// </summary>
    public async Task<InsightContext> GetInsightContextAsync(string query)
    {
        try
        {
            var queryPattern = ExtractSQLPattern(query);

            var relatedInsights = await _context.AIFeedbackEntries
                .Where(f => f.SQLPattern == queryPattern && f.IsSuccessful)
                .Select(f => f.FeedbackText)
                .Where(f => !string.IsNullOrEmpty(f))
                .Take(10)
                .ToListAsync();

            return new InsightContext
            {
                QueryPattern = queryPattern,
                RelatedInsights = relatedInsights,
                ContextualHints = ExtractContextualHints(relatedInsights)
            };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error getting insight context");
            return new InsightContext { QueryPattern = ExtractSQLPattern(query) };
        }
    }

    /// <summary>
    /// Get learning statistics
    /// </summary>
    public async Task<LearningStatistics> GetLearningStatisticsAsync()
    {
        try
        {
            var stats = await _context.AIFeedbackEntries
                .GroupBy(f => 1)
                .Select(g => new
                {
                    TotalFeedback = g.Count(),
                    AverageRating = g.Average(f => f.Rating),
                    UniqueUsers = g.Select(f => f.UserId).Distinct().Count()
                })
                .FirstOrDefaultAsync();

            var totalGenerations = await _context.AIGenerationAttempts.CountAsync();

            var popularPatterns = await _context.AIFeedbackEntries
                .Where(f => f.IsSuccessful)
                .GroupBy(f => f.PromptPattern)
                .OrderByDescending(g => g.Count())
                .Take(10)
                .ToDictionaryAsync(g => g.Key, g => g.Count());

            return new LearningStatistics
            {
                TotalGenerations = totalGenerations,
                TotalFeedbackItems = stats?.TotalFeedback ?? 0,
                AverageRating = stats?.AverageRating ?? 0.0,
                AverageConfidence = await CalculateAverageConfidenceAsync(),
                UniqueUsers = stats?.UniqueUsers ?? 0,
                PopularPatterns = popularPatterns
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting learning statistics");
            return new LearningStatistics();
        }
    }

    private async Task<List<string>> FindSimilarPromptsAsync(string prompt)
    {
        var pattern = ExtractPromptPattern(prompt);

        return await _context.AIFeedbackEntries
            .Where(f => f.PromptPattern == pattern)
            .Select(f => f.OriginalPrompt)
            .Distinct()
            .Take(50)
            .ToListAsync();
    }

    private async Task<List<AIFeedbackEntry>> GetFeedbackDataAsync(List<string> prompts)
    {
        return await _context.AIFeedbackEntries
            .Where(f => prompts.Contains(f.OriginalPrompt))
            .OrderByDescending(f => f.Timestamp)
            .Take(100)
            .ToListAsync();
    }

    private string ExtractPromptPattern(string prompt)
    {
        // Simplified pattern extraction - categorize by key words
        var lowerPrompt = prompt.ToLower();

        if (lowerPrompt.Contains("show") || lowerPrompt.Contains("display") || lowerPrompt.Contains("list"))
            return "display_query";
        if (lowerPrompt.Contains("count") || lowerPrompt.Contains("total") || lowerPrompt.Contains("number"))
            return "count_query";
        if (lowerPrompt.Contains("average") || lowerPrompt.Contains("mean") || lowerPrompt.Contains("avg"))
            return "average_query";
        if (lowerPrompt.Contains("sum") || lowerPrompt.Contains("total"))
            return "sum_query";
        if (lowerPrompt.Contains("group") || lowerPrompt.Contains("by"))
            return "group_query";
        if (lowerPrompt.Contains("join") || lowerPrompt.Contains("combine"))
            return "join_query";
        if (lowerPrompt.Contains("filter") || lowerPrompt.Contains("where") || lowerPrompt.Contains("condition"))
            return "filter_query";

        return "general_query";
    }

    private string ExtractSQLPattern(string sql)
    {
        var upperSQL = sql.ToUpper();

        if (upperSQL.Contains("GROUP BY"))
            return "grouped_query";
        if (upperSQL.Contains("JOIN"))
            return "joined_query";
        if (upperSQL.Contains("UNION"))
            return "union_query";
        if (upperSQL.Contains("SUBQUERY") || upperSQL.Contains("(SELECT"))
            return "subquery";
        if (upperSQL.Contains("ORDER BY"))
            return "ordered_query";
        if (upperSQL.Contains("HAVING"))
            return "having_query";

        return "simple_query";
    }

    private List<string> ExtractSuccessfulPatterns(List<AIFeedbackEntry> feedbackData)
    {
        return feedbackData
            .Where(f => f.IsSuccessful)
            .SelectMany(f => ExtractKeywords(f.OriginalPrompt))
            .GroupBy(k => k)
            .OrderByDescending(g => g.Count())
            .Take(10)
            .Select(g => g.Key)
            .ToList();
    }

    private List<string> ExtractCommonMistakes(List<AIFeedbackEntry> feedbackData)
    {
        return feedbackData
            .Where(f => !f.IsSuccessful)
            .SelectMany(f => ExtractKeywords(f.GeneratedSQL))
            .GroupBy(k => k)
            .OrderByDescending(g => g.Count())
            .Take(5)
            .Select(g => g.Key)
            .ToList();
    }

    private List<string> GenerateOptimizationSuggestions(List<AIFeedbackEntry> feedbackData)
    {
        var suggestions = new List<string>();

        var successfulQueries = feedbackData.Where(f => f.IsSuccessful).ToList();
        var unsuccessfulQueries = feedbackData.Where(f => !f.IsSuccessful).ToList();

        if (successfulQueries.Any() && unsuccessfulQueries.Any())
        {
            suggestions.Add("Use more specific table and column names");
            suggestions.Add("Include proper JOIN conditions");
            suggestions.Add("Add appropriate WHERE clauses for filtering");
        }

        return suggestions;
    }

    private double CalculateConfidenceModifier(List<AIFeedbackEntry> feedbackData)
    {
        if (!feedbackData.Any()) return 0.0;

        var successRate = feedbackData.Count(f => f.IsSuccessful) / (double)feedbackData.Count;
        return (successRate - 0.5) * 0.3; // Scale between -0.15 and +0.15
    }

    private List<string> ExtractKeywords(string text)
    {
        var words = Regex.Split(text.ToLower(), @"\W+")
            .Where(w => w.Length > 3)
            .Where(w => !IsStopWord(w))
            .ToList();

        return words;
    }

    private bool IsStopWord(string word)
    {
        var stopWords = new HashSet<string> { "the", "and", "for", "are", "but", "not", "you", "all", "can", "had", "her", "was", "one", "our", "out", "day", "get", "has", "him", "his", "how", "its", "may", "new", "now", "old", "see", "two", "who", "boy", "did", "man", "men", "put", "say", "she", "too", "use" };
        return stopWords.Contains(word);
    }

    private async Task UpdateLearningPatternsAsync(AIFeedbackEntry feedbackEntry)
    {
        // Clear cache for this pattern to force refresh
        var cacheKey = $"insights_{feedbackEntry.PromptPattern}";
        _patternCache.Remove(cacheKey);
    }

    private string GenerateSuggestionFromPattern(string pattern, SchemaMetadata schema)
    {
        return pattern switch
        {
            "display_query" => $"Show me all records from {schema.Tables.FirstOrDefault()?.Name ?? "table"}",
            "count_query" => $"Count the total number of records in {schema.Tables.FirstOrDefault()?.Name ?? "table"}",
            "average_query" => "Calculate the average value of a numeric column",
            "sum_query" => "Sum up values in a numeric column",
            "group_query" => "Group data by a specific column and show counts",
            _ => ""
        };
    }

    private List<string> ExtractContextualHints(List<string> relatedInsights)
    {
        return relatedInsights
            .SelectMany(insight => insight.Split('.', StringSplitOptions.RemoveEmptyEntries))
            .Where(hint => hint.Trim().Length > 10)
            .Take(5)
            .ToList();
    }

    private async Task<double> CalculateAverageConfidenceAsync()
    {
        return await _context.AIGenerationAttempts
            .Where(a => a.ConfidenceScore.HasValue)
            .AverageAsync(a => a.ConfidenceScore!.Value);
    }

    private class LearningPattern
    {
        public LearningInsights Insights { get; set; } = new();
        public DateTime LastUpdated { get; set; }
    }
}

/// <summary>
/// Learning insights for a prompt pattern
/// </summary>
public class LearningInsights
{
    public string PromptPattern { get; set; } = string.Empty;
    public List<string> SuccessfulPatterns { get; set; } = new();
    public List<string> CommonMistakes { get; set; } = new();
    public List<string> OptimizationSuggestions { get; set; } = new();
    public double ConfidenceModifier { get; set; }
    public int SampleCount { get; set; }
}

/// <summary>
/// Context for insight generation
/// </summary>
public class InsightContext
{
    public string QueryPattern { get; set; } = string.Empty;
    public List<string> RelatedInsights { get; set; } = new();
    public List<string> ContextualHints { get; set; } = new();
}

/// <summary>
/// AI feedback entry for learning
/// </summary>
public class AIFeedbackEntry
{
    public long Id { get; set; }
    public string OriginalPrompt { get; set; } = string.Empty;
    public string GeneratedSQL { get; set; } = string.Empty;
    public int Rating { get; set; }
    public string? FeedbackText { get; set; }
    public string UserId { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string PromptPattern { get; set; } = string.Empty;
    public string SQLPattern { get; set; } = string.Empty;
    public bool IsSuccessful { get; set; }
}

/// <summary>
/// Extension methods for FeedbackLearningEngine
/// </summary>
public static class FeedbackLearningEngineExtensions
{
    /// <summary>
    /// Convert QueryFeedback to rating (since Core.Models.QueryFeedback doesn't have Rating property)
    /// </summary>
    public static int GetRatingFromFeedback(QueryFeedback feedback)
    {
        // Map feedback string to rating
        return feedback.Feedback?.ToLowerInvariant() switch
        {
            "positive" => 5,
            "neutral" => 3,
            "negative" => 1,
            _ => 3 // Default to neutral
        };
    }
}
