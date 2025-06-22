using Microsoft.Extensions.Logging;
using BIReportingCopilot.Core.Models.BusinessContext;
using BIReportingCopilot.Core.Interfaces.AI;
using BIReportingCopilot.Core.Interfaces.Cache;
using BIReportingCopilot.Core.Extensions;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace BIReportingCopilot.Infrastructure.BusinessContext.Enhanced;

/// <summary>
/// Advanced multi-model intent classification ensemble that replaces all legacy intent classification
/// </summary>
public class IntentClassificationEnsemble : IIntentClassificationEnsemble
{
    private readonly IAIService _aiService;
    private readonly ICacheService _cacheService;
    private readonly ILogger<IntentClassificationEnsemble> _logger;
    
    // Pre-compiled patterns for performance
    private static readonly Dictionary<IntentType, List<Regex>> IntentPatterns = new()
    {
        [IntentType.Aggregation] = new()
        {
            new(@"\b(total|sum|count|average|avg|max|min|aggregate)\b", RegexOptions.IgnoreCase | RegexOptions.Compiled),
            new(@"\b(how many|how much|what is the total)\b", RegexOptions.IgnoreCase | RegexOptions.Compiled),
            new(@"\bgroup by\b", RegexOptions.IgnoreCase | RegexOptions.Compiled)
        },
        [IntentType.Trend] = new()
        {
            new(@"\b(trend|over time|time series|growth|decline|change)\b", RegexOptions.IgnoreCase | RegexOptions.Compiled),
            new(@"\b(last \d+ (days?|weeks?|months?|years?))\b", RegexOptions.IgnoreCase | RegexOptions.Compiled),
            new(@"\b(year over year|month over month|daily|weekly|monthly)\b", RegexOptions.IgnoreCase | RegexOptions.Compiled)
        },
        [IntentType.Comparison] = new()
        {
            new(@"\b(compare|versus|vs|against|difference|better|worse)\b", RegexOptions.IgnoreCase | RegexOptions.Compiled),
            new(@"\b(top \d+|bottom \d+|highest|lowest|best|worst)\b", RegexOptions.IgnoreCase | RegexOptions.Compiled),
            new(@"\b(between .+ and .+)\b", RegexOptions.IgnoreCase | RegexOptions.Compiled)
        },
        [IntentType.Detail] = new()
        {
            new(@"\b(list|show|display|details|specific|individual)\b", RegexOptions.IgnoreCase | RegexOptions.Compiled),
            new(@"\b(who|what|where|when|which)\b", RegexOptions.IgnoreCase | RegexOptions.Compiled),
            new(@"\b(records|rows|entries|items)\b", RegexOptions.IgnoreCase | RegexOptions.Compiled)
        },
        [IntentType.Exploratory] = new()
        {
            new(@"\b(find|discover|explore|unusual|anomaly|pattern|insight)\b", RegexOptions.IgnoreCase | RegexOptions.Compiled),
            new(@"\b(what if|why|how|correlation|relationship)\b", RegexOptions.IgnoreCase | RegexOptions.Compiled),
            new(@"\b(outlier|exception|strange|unexpected)\b", RegexOptions.IgnoreCase | RegexOptions.Compiled)
        },
        [IntentType.Operational] = new()
        {
            new(@"\b(current|latest|recent|today|now|active)\b", RegexOptions.IgnoreCase | RegexOptions.Compiled),
            new(@"\b(status|state|condition|health|performance)\b", RegexOptions.IgnoreCase | RegexOptions.Compiled),
            new(@"\b(real.?time|live|immediate)\b", RegexOptions.IgnoreCase | RegexOptions.Compiled)
        }
    };

    // Semantic keywords for each intent type
    private static readonly Dictionary<IntentType, List<string>> SemanticKeywords = new()
    {
        [IntentType.Aggregation] = new() { "sum", "total", "count", "average", "aggregate", "rollup", "summary" },
        [IntentType.Trend] = new() { "trend", "time", "growth", "change", "evolution", "progression", "temporal" },
        [IntentType.Comparison] = new() { "compare", "versus", "difference", "contrast", "benchmark", "relative" },
        [IntentType.Detail] = new() { "detail", "specific", "individual", "particular", "exact", "precise" },
        [IntentType.Exploratory] = new() { "explore", "discover", "find", "analyze", "investigate", "research" },
        [IntentType.Operational] = new() { "operational", "current", "status", "real-time", "immediate", "live" },
        [IntentType.Analytical] = new() { "analyze", "analysis", "insight", "intelligence", "complex", "deep" }
    };

    public IntentClassificationEnsemble(
        IAIService aiService,
        ICacheService cacheService,
        ILogger<IntentClassificationEnsemble> logger)
    {
        _aiService = aiService;
        _cacheService = cacheService;
        _logger = logger;
    }

    public async Task<QueryIntent> ClassifyWithConfidenceAsync(string userQuestion)
    {
        var cacheKey = $"intent_classification:{userQuestion.GetHashCode()}";
        var (found, cachedResult) = await _cacheService.TryGetAsync<QueryIntent>(cacheKey);
        if (found && cachedResult != null)
        {
            return cachedResult;
        }

        _logger.LogInformation("Classifying intent for question: {Question}", userQuestion.Substring(0, Math.Min(100, userQuestion.Length)));

        // Run all classification methods in parallel
        var classificationTasks = new[]
        {
            ClassifyWithPatternMatchingAsync(userQuestion),
            ClassifyWithSemanticAnalysisAsync(userQuestion),
            ClassifyWithAIModelAsync(userQuestion),
            ClassifyWithKeywordAnalysisAsync(userQuestion)
        };

        var results = await Task.WhenAll(classificationTasks);

        // Ensemble aggregation with weighted voting
        var finalIntent = AggregateClassificationResults(results, userQuestion);

        // Cache result for 1 hour
        await _cacheService.SetAsync(cacheKey, finalIntent, TimeSpan.FromHours(1));

        _logger.LogInformation("Intent classified as {Intent} with confidence {Confidence:F3}", 
            finalIntent.Type, finalIntent.ConfidenceScore);

        return finalIntent;
    }

    private async Task<IntentClassificationResult> ClassifyWithPatternMatchingAsync(string userQuestion)
    {
        var scores = new Dictionary<IntentType, double>();
        
        foreach (var (intentType, patterns) in IntentPatterns)
        {
            var matchCount = patterns.Count(pattern => pattern.IsMatch(userQuestion));
            scores[intentType] = (double)matchCount / patterns.Count;
        }

        var bestMatch = scores.OrderByDescending(kvp => kvp.Value).First();
        
        return new IntentClassificationResult
        {
            IntentType = bestMatch.Key,
            ConfidenceScore = Math.Min(bestMatch.Value * 0.8, 0.95), // Cap at 95% for pattern matching
            Method = "PatternMatching",
            Details = $"Matched {bestMatch.Value:P0} of patterns"
        };
    }

    private async Task<IntentClassificationResult> ClassifyWithSemanticAnalysisAsync(string userQuestion)
    {
        var questionWords = userQuestion.ToLower()
            .Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Where(w => w.Length > 2)
            .ToList();

        var scores = new Dictionary<IntentType, double>();

        foreach (var (intentType, keywords) in SemanticKeywords)
        {
            var matchingKeywords = keywords.Count(keyword => 
                questionWords.Any(word => 
                    word.Contains(keyword) || 
                    CalculateLevenshteinSimilarity(word, keyword) > 0.8));
            
            scores[intentType] = (double)matchingKeywords / keywords.Count;
        }

        var bestMatch = scores.OrderByDescending(kvp => kvp.Value).First();
        
        return new IntentClassificationResult
        {
            IntentType = bestMatch.Key,
            ConfidenceScore = bestMatch.Value * 0.7, // Lower weight for keyword matching
            Method = "SemanticAnalysis",
            Details = $"Semantic similarity: {bestMatch.Value:P0}"
        };
    }

    private async Task<IntentClassificationResult> ClassifyWithAIModelAsync(string userQuestion)
    {
        try
        {
            var prompt = $@"Classify this business question into exactly ONE of these intent types:

AGGREGATION: Questions asking for totals, sums, counts, averages, or other aggregate calculations
TREND: Questions about changes over time, growth patterns, or temporal analysis  
COMPARISON: Questions comparing different entities, time periods, or segments
DETAIL: Questions asking for specific records, lists, or detailed information
EXPLORATORY: Questions seeking to discover patterns, anomalies, or insights
OPERATIONAL: Questions about current status, real-time data, or operational metrics
ANALYTICAL: Complex analytical questions requiring deep analysis

Question: {userQuestion}

Respond with ONLY the intent type (e.g., AGGREGATION) followed by a confidence score 0.0-1.0.
Format: INTENT_TYPE|0.XX";

            var response = await _aiService.GenerateSQLAsync(prompt);
            var parts = response.Trim().Split('|');
            
            if (parts.Length == 2 && 
                Enum.TryParse<IntentType>(parts[0], true, out var intentType) &&
                double.TryParse(parts[1], out var confidence))
            {
                return new IntentClassificationResult
                {
                    IntentType = intentType,
                    ConfidenceScore = Math.Min(confidence, 0.98), // Cap AI confidence
                    Method = "AIModel",
                    Details = $"AI classification: {parts[0]}"
                };
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "AI model classification failed, using fallback");
        }

        // Fallback to analytical intent with low confidence
        return new IntentClassificationResult
        {
            IntentType = IntentType.Analytical,
            ConfidenceScore = 0.3,
            Method = "AIModel_Fallback",
            Details = "AI model failed, using fallback"
        };
    }

    private async Task<IntentClassificationResult> ClassifyWithKeywordAnalysisAsync(string userQuestion)
    {
        // Advanced keyword analysis with context
        var words = userQuestion.ToLower().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var scores = new Dictionary<IntentType, double>();

        // Analyze question structure and keywords
        if (words.Any(w => w.StartsWith("how many") || w.StartsWith("how much") || w.Contains("total")))
            scores[IntentType.Aggregation] = 0.9;
        
        if (words.Any(w => w.Contains("over time") || w.Contains("trend") || w.Contains("growth")))
            scores[IntentType.Trend] = 0.85;
        
        if (words.Any(w => w.Contains("compare") || w.Contains("vs") || w.Contains("versus")))
            scores[IntentType.Comparison] = 0.8;
        
        if (words.Any(w => w.Contains("list") || w.Contains("show") || w.Contains("details")))
            scores[IntentType.Detail] = 0.75;
        
        if (words.Any(w => w.Contains("find") || w.Contains("discover") || w.Contains("unusual")))
            scores[IntentType.Exploratory] = 0.7;
        
        if (words.Any(w => w.Contains("current") || w.Contains("now") || w.Contains("status")))
            scores[IntentType.Operational] = 0.65;

        var bestMatch = scores.Any() ? 
            scores.OrderByDescending(kvp => kvp.Value).First() :
            new KeyValuePair<IntentType, double>(IntentType.Analytical, 0.5);

        return new IntentClassificationResult
        {
            IntentType = bestMatch.Key,
            ConfidenceScore = bestMatch.Value * 0.6, // Lower weight for simple keyword analysis
            Method = "KeywordAnalysis",
            Details = $"Keyword analysis score: {bestMatch.Value:F2}"
        };
    }

    private QueryIntent AggregateClassificationResults(IntentClassificationResult[] results, string userQuestion)
    {
        // Weighted ensemble voting
        var weights = new Dictionary<string, double>
        {
            ["AIModel"] = 0.4,
            ["PatternMatching"] = 0.3,
            ["SemanticAnalysis"] = 0.2,
            ["KeywordAnalysis"] = 0.1
        };

        var intentScores = new Dictionary<IntentType, double>();
        var totalWeight = 0.0;
        var methodDetails = new List<string>();

        foreach (var result in results)
        {
            var weight = weights.GetValueOrDefault(result.Method, 0.1);
            var weightedScore = result.ConfidenceScore * weight;
            
            if (!intentScores.ContainsKey(result.IntentType))
                intentScores[result.IntentType] = 0;
            
            intentScores[result.IntentType] += weightedScore;
            totalWeight += weight;
            methodDetails.Add($"{result.Method}: {result.IntentType} ({result.ConfidenceScore:F2})");
        }

        // Normalize scores
        foreach (var key in intentScores.Keys.ToList())
        {
            intentScores[key] /= totalWeight;
        }

        var finalIntent = intentScores.OrderByDescending(kvp => kvp.Value).First();
        
        return new QueryIntent
        {
            Type = finalIntent.Key,
            ConfidenceScore = finalIntent.Value,
            Description = GetIntentDescription(finalIntent.Key),
            Keywords = ExtractRelevantKeywords(userQuestion, finalIntent.Key),
            Metadata = new Dictionary<string, object>
            {
                ["ensemble_methods"] = methodDetails,
                ["all_scores"] = intentScores,
                ["classification_timestamp"] = DateTime.UtcNow
            }
        };
    }

    private static string GetIntentDescription(IntentType intentType) => intentType switch
    {
        IntentType.Aggregation => "Aggregate calculations and summaries",
        IntentType.Trend => "Time-based analysis and trends",
        IntentType.Comparison => "Comparative analysis between entities",
        IntentType.Detail => "Detailed record retrieval",
        IntentType.Exploratory => "Data exploration and discovery",
        IntentType.Operational => "Operational and real-time queries",
        IntentType.Analytical => "Complex analytical queries",
        _ => "General business query"
    };

    private static List<string> ExtractRelevantKeywords(string userQuestion, IntentType intentType)
    {
        var words = userQuestion.ToLower().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var relevantKeywords = SemanticKeywords.GetValueOrDefault(intentType, new List<string>());
        
        return words.Where(word => 
            relevantKeywords.Any(keyword => 
                word.Contains(keyword) || 
                CalculateLevenshteinSimilarity(word, keyword) > 0.7))
            .Distinct()
            .ToList();
    }

    private static double CalculateLevenshteinSimilarity(string s1, string s2)
    {
        if (string.IsNullOrEmpty(s1) || string.IsNullOrEmpty(s2))
            return 0;

        var distance = CalculateLevenshteinDistance(s1, s2);
        var maxLength = Math.Max(s1.Length, s2.Length);
        return 1.0 - (double)distance / maxLength;
    }

    private static int CalculateLevenshteinDistance(string s1, string s2)
    {
        var matrix = new int[s1.Length + 1, s2.Length + 1];

        for (int i = 0; i <= s1.Length; i++)
            matrix[i, 0] = i;
        for (int j = 0; j <= s2.Length; j++)
            matrix[0, j] = j;

        for (int i = 1; i <= s1.Length; i++)
        {
            for (int j = 1; j <= s2.Length; j++)
            {
                var cost = s1[i - 1] == s2[j - 1] ? 0 : 1;
                matrix[i, j] = Math.Min(
                    Math.Min(matrix[i - 1, j] + 1, matrix[i, j - 1] + 1),
                    matrix[i - 1, j - 1] + cost);
            }
        }

        return matrix[s1.Length, s2.Length];
    }
}

public record IntentClassificationResult
{
    public IntentType IntentType { get; init; }
    public double ConfidenceScore { get; init; }
    public string Method { get; init; } = string.Empty;
    public string Details { get; init; } = string.Empty;
}

public interface IIntentClassificationEnsemble
{
    Task<QueryIntent> ClassifyWithConfidenceAsync(string userQuestion);
}
