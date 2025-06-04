using Microsoft.Extensions.Logging;
using BIReportingCopilot.Core.Interfaces;
using BIReportingCopilot.Core.Models;

namespace BIReportingCopilot.Infrastructure.AI.Enhanced;

/// <summary>
/// Advanced intent classifier with context awareness and confidence scoring
/// Uses both rule-based and AI-powered classification
/// </summary>
public class IntentClassifier
{
    private readonly IAIService _aiService;
    private readonly ILogger _logger;
    private readonly Dictionary<QueryIntent, IntentPattern> _intentPatterns;
    private readonly Dictionary<QueryIntent, List<string>> _intentKeywords;

    public IntentClassifier(IAIService aiService, ILogger logger)
    {
        _aiService = aiService;
        _logger = logger;
        _intentPatterns = InitializeIntentPatterns();
        _intentKeywords = InitializeIntentKeywords();
    }

    /// <summary>
    /// Classify intent with conversation context
    /// </summary>
    public async Task<QueryIntent> ClassifyWithContextAsync(
        string query, 
        ConversationContext conversationContext, 
        List<SemanticEntity> entities)
    {
        try
        {
            _logger.LogDebug("Classifying intent for query: {Query}", query);

            // Step 1: Rule-based classification
            var ruleBasedIntent = ClassifyUsingRules(query, entities);
            var ruleBasedConfidence = CalculateRuleBasedConfidence(query, ruleBasedIntent, entities);

            // Step 2: Context-aware classification
            var contextIntent = ClassifyUsingContext(query, conversationContext, entities);
            var contextConfidence = CalculateContextConfidence(contextIntent, conversationContext);

            // Step 3: AI-powered classification (if available and needed)
            QueryIntent aiIntent = QueryIntent.General;
            double aiConfidence = 0.0;
            
            if (ruleBasedConfidence < 0.8 || contextConfidence < 0.7)
            {
                var aiResult = await ClassifyUsingAIAsync(query, entities, conversationContext);
                aiIntent = aiResult.Intent;
                aiConfidence = aiResult.Confidence;
            }

            // Step 4: Combine results using weighted voting
            var finalIntent = CombineIntentClassifications(
                (ruleBasedIntent, ruleBasedConfidence),
                (contextIntent, contextConfidence),
                (aiIntent, aiConfidence));

            _logger.LogDebug("Intent classification result: {Intent} (rule: {RuleConf:F2}, context: {ContextConf:F2}, ai: {AiConf:F2})",
                finalIntent, ruleBasedConfidence, contextConfidence, aiConfidence);

            return finalIntent;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in intent classification");
            return QueryIntent.General;
        }
    }

    /// <summary>
    /// Rule-based intent classification using patterns and keywords
    /// </summary>
    private QueryIntent ClassifyUsingRules(string query, List<SemanticEntity> entities)
    {
        var lowerQuery = query.ToLowerInvariant();
        var scores = new Dictionary<QueryIntent, double>();

        // Initialize scores
        foreach (var intent in Enum.GetValues<QueryIntent>())
        {
            scores[intent] = 0.0;
        }

        // Score based on keyword patterns
        foreach (var (intent, keywords) in _intentKeywords)
        {
            var keywordScore = keywords.Count(keyword => lowerQuery.Contains(keyword.ToLowerInvariant()));
            scores[intent] += keywordScore * 2.0; // Weight keyword matches highly
        }

        // Score based on entity types
        foreach (var entity in entities)
        {
            switch (entity.Type)
            {
                case EntityType.Aggregation:
                    scores[QueryIntent.Aggregation] += 3.0;
                    break;
                case EntityType.DateRange:
                    scores[QueryIntent.Trend] += 2.0;
                    break;
                case EntityType.Condition:
                    scores[QueryIntent.Filtering] += 2.0;
                    break;
                case EntityType.Sort:
                    scores[QueryIntent.Comparison] += 1.5;
                    break;
            }
        }

        // Apply pattern-based scoring
        foreach (var (intent, pattern) in _intentPatterns)
        {
            var patternScore = EvaluatePattern(lowerQuery, pattern);
            scores[intent] += patternScore;
        }

        // Return intent with highest score
        var bestIntent = scores.OrderByDescending(kvp => kvp.Value).First();
        return bestIntent.Value > 1.0 ? bestIntent.Key : QueryIntent.General;
    }

    /// <summary>
    /// Context-aware intent classification using conversation history
    /// </summary>
    private QueryIntent ClassifyUsingContext(
        string query, 
        ConversationContext conversationContext, 
        List<SemanticEntity> entities)
    {
        if (!conversationContext.PreviousQueries.Any())
        {
            return QueryIntent.General;
        }

        // Analyze intent patterns in conversation
        var recentIntents = conversationContext.PreviousQueries
            .TakeLast(3)
            .Select(q => q.Analysis?.Intent ?? QueryIntent.General)
            .ToList();

        // Check for intent continuation patterns
        if (recentIntents.Count >= 2)
        {
            var lastIntent = recentIntents.Last();
            var secondLastIntent = recentIntents[recentIntents.Count - 2];

            // If user is drilling down (e.g., aggregation -> filtering)
            if (lastIntent == QueryIntent.Aggregation && ContainsFilteringKeywords(query))
            {
                return QueryIntent.Filtering;
            }

            // If user is comparing after aggregation
            if (lastIntent == QueryIntent.Aggregation && ContainsComparisonKeywords(query))
            {
                return QueryIntent.Comparison;
            }

            // If user is looking at trends after filtering
            if (lastIntent == QueryIntent.Filtering && ContainsTemporalKeywords(query))
            {
                return QueryIntent.Trend;
            }
        }

        // Check for entity continuity
        var previousEntities = conversationContext.PreviousQueries
            .SelectMany(q => q.Analysis?.Entities ?? new List<SemanticEntity>())
            .Select(e => e.Text.ToLowerInvariant())
            .Distinct()
            .ToList();

        var currentEntityTexts = entities.Select(e => e.Text.ToLowerInvariant()).ToList();
        var entityOverlap = previousEntities.Intersect(currentEntityTexts).Count();

        // If high entity overlap, likely continuing same type of analysis
        if (entityOverlap >= 2 && recentIntents.Any())
        {
            var dominantIntent = recentIntents
                .GroupBy(i => i)
                .OrderByDescending(g => g.Count())
                .First().Key;

            if (dominantIntent != QueryIntent.General)
            {
                return dominantIntent;
            }
        }

        return QueryIntent.General;
    }

    /// <summary>
    /// AI-powered intent classification for complex cases
    /// </summary>
    private async Task<(QueryIntent Intent, double Confidence)> ClassifyUsingAIAsync(
        string query, 
        List<SemanticEntity> entities, 
        ConversationContext conversationContext)
    {
        try
        {
            var prompt = BuildIntentClassificationPrompt(query, entities, conversationContext);
            var response = await _aiService.GenerateSQLAsync(prompt); // Reusing AI service

            // Parse AI response (simplified - would need more robust parsing)
            var intent = ParseIntentFromAIResponse(response);
            var confidence = ParseConfidenceFromAIResponse(response);

            return (intent, confidence);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in AI-powered intent classification");
            return (QueryIntent.General, 0.0);
        }
    }

    /// <summary>
    /// Combine multiple intent classifications using weighted voting
    /// </summary>
    private QueryIntent CombineIntentClassifications(
        (QueryIntent Intent, double Confidence) ruleResult,
        (QueryIntent Intent, double Confidence) contextResult,
        (QueryIntent Intent, double Confidence) aiResult)
    {
        var votes = new Dictionary<QueryIntent, double>();

        // Weight rule-based result (40%)
        if (ruleResult.Confidence > 0.5)
        {
            votes[ruleResult.Intent] = votes.GetValueOrDefault(ruleResult.Intent, 0) + ruleResult.Confidence * 0.4;
        }

        // Weight context result (35%)
        if (contextResult.Confidence > 0.5)
        {
            votes[contextResult.Intent] = votes.GetValueOrDefault(contextResult.Intent, 0) + contextResult.Confidence * 0.35;
        }

        // Weight AI result (25%)
        if (aiResult.Confidence > 0.5)
        {
            votes[aiResult.Intent] = votes.GetValueOrDefault(aiResult.Intent, 0) + aiResult.Confidence * 0.25;
        }

        // Return intent with highest weighted vote
        if (votes.Any())
        {
            var winner = votes.OrderByDescending(kvp => kvp.Value).First();
            return winner.Value > 0.3 ? winner.Key : QueryIntent.General;
        }

        return QueryIntent.General;
    }

    /// <summary>
    /// Calculate confidence for rule-based classification
    /// </summary>
    private double CalculateRuleBasedConfidence(string query, QueryIntent intent, List<SemanticEntity> entities)
    {
        if (intent == QueryIntent.General) return 0.3;

        var confidence = 0.5; // Base confidence

        // Boost confidence based on keyword matches
        if (_intentKeywords.ContainsKey(intent))
        {
            var keywordMatches = _intentKeywords[intent]
                .Count(keyword => query.ToLowerInvariant().Contains(keyword.ToLowerInvariant()));
            confidence += keywordMatches * 0.1;
        }

        // Boost confidence based on relevant entities
        var relevantEntityCount = entities.Count(e => IsEntityRelevantToIntent(e, intent));
        confidence += relevantEntityCount * 0.1;

        return Math.Min(1.0, confidence);
    }

    /// <summary>
    /// Calculate confidence for context-based classification
    /// </summary>
    private double CalculateContextConfidence(QueryIntent intent, ConversationContext conversationContext)
    {
        if (intent == QueryIntent.General || !conversationContext.PreviousQueries.Any())
        {
            return 0.3;
        }

        var recentIntents = conversationContext.PreviousQueries
            .TakeLast(3)
            .Select(q => q.Analysis?.Intent ?? QueryIntent.General)
            .ToList();

        var intentFrequency = recentIntents.Count(i => i == intent) / (double)recentIntents.Count;
        return 0.5 + (intentFrequency * 0.4); // Base 0.5 + up to 0.4 for frequency
    }

    // Helper methods
    private bool ContainsFilteringKeywords(string query)
    {
        var filterKeywords = new[] { "where", "filter", "only", "specific", "exclude", "include" };
        return filterKeywords.Any(k => query.ToLowerInvariant().Contains(k));
    }

    private bool ContainsComparisonKeywords(string query)
    {
        var comparisonKeywords = new[] { "compare", "vs", "versus", "difference", "better", "worse", "higher", "lower" };
        return comparisonKeywords.Any(k => query.ToLowerInvariant().Contains(k));
    }

    private bool ContainsTemporalKeywords(string query)
    {
        var temporalKeywords = new[] { "trend", "over time", "growth", "change", "monthly", "yearly", "daily" };
        return temporalKeywords.Any(k => query.ToLowerInvariant().Contains(k));
    }

    private bool IsEntityRelevantToIntent(SemanticEntity entity, QueryIntent intent)
    {
        return intent switch
        {
            QueryIntent.Aggregation => entity.Type == EntityType.Aggregation,
            QueryIntent.Trend => entity.Type == EntityType.DateRange,
            QueryIntent.Filtering => entity.Type == EntityType.Condition,
            QueryIntent.Comparison => entity.Type == EntityType.Sort || entity.Type == EntityType.Condition,
            _ => false
        };
    }

    private double EvaluatePattern(string query, IntentPattern pattern)
    {
        var score = 0.0;
        
        foreach (var requiredKeyword in pattern.RequiredKeywords)
        {
            if (query.Contains(requiredKeyword.ToLowerInvariant()))
            {
                score += 2.0;
            }
        }

        foreach (var optionalKeyword in pattern.OptionalKeywords)
        {
            if (query.Contains(optionalKeyword.ToLowerInvariant()))
            {
                score += 1.0;
            }
        }

        return score;
    }

    private string BuildIntentClassificationPrompt(string query, List<SemanticEntity> entities, ConversationContext conversationContext)
    {
        var prompt = $@"Classify the intent of this query: ""{query}""

Available intents: Aggregation, Trend, Comparison, Filtering, General

Entities found: {string.Join(", ", entities.Select(e => $"{e.Text} ({e.Type})"))}

Previous queries in conversation:
{string.Join("\n", conversationContext.PreviousQueries.TakeLast(3).Select(q => $"- {q.Query}"))}

Respond with: INTENT: [intent] CONFIDENCE: [0.0-1.0]";

        return prompt;
    }

    private QueryIntent ParseIntentFromAIResponse(string response)
    {
        // Simple parsing - would need more robust implementation
        if (response.Contains("INTENT:"))
        {
            var intentPart = response.Split("INTENT:")[1].Split("CONFIDENCE:")[0].Trim();
            if (Enum.TryParse<QueryIntent>(intentPart, true, out var intent))
            {
                return intent;
            }
        }
        return QueryIntent.General;
    }

    private double ParseConfidenceFromAIResponse(string response)
    {
        // Simple parsing - would need more robust implementation
        if (response.Contains("CONFIDENCE:"))
        {
            var confidencePart = response.Split("CONFIDENCE:")[1].Trim();
            if (double.TryParse(confidencePart, out var confidence))
            {
                return Math.Max(0.0, Math.Min(1.0, confidence));
            }
        }
        return 0.5;
    }

    private Dictionary<QueryIntent, IntentPattern> InitializeIntentPatterns()
    {
        return new Dictionary<QueryIntent, IntentPattern>
        {
            [QueryIntent.Aggregation] = new IntentPattern
            {
                RequiredKeywords = new[] { "sum", "total", "count", "average", "max", "min" },
                OptionalKeywords = new[] { "group", "by", "aggregate" }
            },
            [QueryIntent.Trend] = new IntentPattern
            {
                RequiredKeywords = new[] { "trend", "over time", "growth", "change" },
                OptionalKeywords = new[] { "monthly", "yearly", "daily", "weekly" }
            },
            [QueryIntent.Comparison] = new IntentPattern
            {
                RequiredKeywords = new[] { "compare", "vs", "versus", "difference" },
                OptionalKeywords = new[] { "better", "worse", "higher", "lower" }
            },
            [QueryIntent.Filtering] = new IntentPattern
            {
                RequiredKeywords = new[] { "where", "filter", "only", "specific" },
                OptionalKeywords = new[] { "exclude", "include", "condition" }
            }
        };
    }

    private Dictionary<QueryIntent, List<string>> InitializeIntentKeywords()
    {
        return new Dictionary<QueryIntent, List<string>>
        {
            [QueryIntent.Aggregation] = new() { "sum", "total", "count", "average", "avg", "maximum", "minimum", "max", "min", "group" },
            [QueryIntent.Trend] = new() { "trend", "over time", "growth", "change", "monthly", "yearly", "daily", "weekly", "timeline" },
            [QueryIntent.Comparison] = new() { "compare", "vs", "versus", "difference", "better", "worse", "higher", "lower", "top", "bottom" },
            [QueryIntent.Filtering] = new() { "where", "filter", "only", "specific", "exclude", "include", "condition", "criteria" }
        };
    }
}

/// <summary>
/// Intent pattern for rule-based classification
/// </summary>
public class IntentPattern
{
    public string[] RequiredKeywords { get; set; } = Array.Empty<string>();
    public string[] OptionalKeywords { get; set; } = Array.Empty<string>();
}
