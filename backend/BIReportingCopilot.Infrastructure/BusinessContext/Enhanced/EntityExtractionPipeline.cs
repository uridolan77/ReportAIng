using Microsoft.Extensions.Logging;
using BIReportingCopilot.Core.Models.BusinessContext;
using BIReportingCopilot.Core.Interfaces.AI;
using BIReportingCopilot.Core.Interfaces.Cache;
using BIReportingCopilot.Core.Extensions;
using BIReportingCopilot.Infrastructure.BusinessContext.Enhanced;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace BIReportingCopilot.Infrastructure.BusinessContext.Enhanced;

/// <summary>
/// Advanced entity extraction pipeline that replaces all legacy entity extraction
/// </summary>
public class EntityExtractionPipeline : IEntityExtractionPipeline
{
    private readonly IAIService _aiService;
    private readonly ICacheService _cacheService;
    private readonly IBusinessTermMatcher _termMatcher;
    private readonly ISemanticEntityLinker _entityLinker;
    private readonly ILogger<EntityExtractionPipeline> _logger;

    // Pre-compiled regex patterns for different entity types
    private static readonly Dictionary<EntityType, List<Regex>> EntityPatterns = new()
    {
        [EntityType.Table] = new()
        {
            new(@"\b(table|from|join)\s+(\w+)", RegexOptions.IgnoreCase | RegexOptions.Compiled),
            new(@"\b(users?|customers?|players?|sales?|orders?|products?|transactions?)\b", RegexOptions.IgnoreCase | RegexOptions.Compiled),
            new(@"\b(revenue|profit|cost|expense|income)\s+(table|data)", RegexOptions.IgnoreCase | RegexOptions.Compiled)
        },
        [EntityType.Column] = new()
        {
            new(@"\b(column|field|attribute)\s+(\w+)", RegexOptions.IgnoreCase | RegexOptions.Compiled),
            new(@"\b(id|name|date|time|amount|count|total|sum)\b", RegexOptions.IgnoreCase | RegexOptions.Compiled),
            new(@"\b(\w+)_(id|name|date|amount|count)\b", RegexOptions.IgnoreCase | RegexOptions.Compiled)
        },
        [EntityType.Metric] = new()
        {
            new(@"\b(total|sum|count|average|avg|max|min|revenue|profit|sales)\b", RegexOptions.IgnoreCase | RegexOptions.Compiled),
            new(@"\b(kpi|metric|measure|indicator)\b", RegexOptions.IgnoreCase | RegexOptions.Compiled),
            new(@"\b(conversion|retention|churn|growth)\s+(rate|ratio)\b", RegexOptions.IgnoreCase | RegexOptions.Compiled)
        },
        [EntityType.Dimension] = new()
        {
            new(@"\b(by|group by|dimension)\s+(\w+)", RegexOptions.IgnoreCase | RegexOptions.Compiled),
            new(@"\b(country|region|state|city|category|type|status)\b", RegexOptions.IgnoreCase | RegexOptions.Compiled),
            new(@"\b(gender|age|segment|platform|device)\b", RegexOptions.IgnoreCase | RegexOptions.Compiled)
        },
        [EntityType.TimeReference] = new()
        {
            new(@"\b(last|past|previous|next)\s+(\d+)\s+(day|week|month|quarter|year)s?\b", RegexOptions.IgnoreCase | RegexOptions.Compiled),
            new(@"\b(today|yesterday|tomorrow|this\s+(week|month|year))\b", RegexOptions.IgnoreCase | RegexOptions.Compiled),
            new(@"\b(\d{4}|\d{1,2}/\d{1,2}/\d{4}|\d{1,2}-\d{1,2}-\d{4})\b", RegexOptions.IgnoreCase | RegexOptions.Compiled)
        },
        [EntityType.ComparisonValue] = new()
        {
            new(@"\b(greater than|less than|equal to|between)\s+(\d+|\$\d+)\b", RegexOptions.IgnoreCase | RegexOptions.Compiled),
            new(@"\b(top|bottom|highest|lowest)\s+(\d+)\b", RegexOptions.IgnoreCase | RegexOptions.Compiled),
            new(@"\b(above|below|over|under)\s+(\d+|\$\d+)\b", RegexOptions.IgnoreCase | RegexOptions.Compiled)
        }
    };

    // Business term categories for enhanced matching
    private static readonly Dictionary<string, EntityType> BusinessTermCategories = new()
    {
        // Tables
        ["users"] = EntityType.Table, ["customers"] = EntityType.Table, ["players"] = EntityType.Table,
        ["sales"] = EntityType.Table, ["orders"] = EntityType.Table, ["transactions"] = EntityType.Table,
        ["products"] = EntityType.Table, ["inventory"] = EntityType.Table, ["sessions"] = EntityType.Table,
        
        // Metrics
        ["revenue"] = EntityType.Metric, ["profit"] = EntityType.Metric, ["cost"] = EntityType.Metric,
        ["conversion"] = EntityType.Metric, ["retention"] = EntityType.Metric, ["churn"] = EntityType.Metric,
        ["arpu"] = EntityType.Metric, ["ltv"] = EntityType.Metric, ["cac"] = EntityType.Metric,
        
        // Dimensions
        ["country"] = EntityType.Dimension, ["region"] = EntityType.Dimension, ["platform"] = EntityType.Dimension,
        ["device"] = EntityType.Dimension, ["category"] = EntityType.Dimension, ["segment"] = EntityType.Dimension,
        ["gender"] = EntityType.Dimension, ["age"] = EntityType.Dimension, ["status"] = EntityType.Dimension
    };

    public EntityExtractionPipeline(
        IAIService aiService,
        ICacheService cacheService,
        IBusinessTermMatcher termMatcher,
        ISemanticEntityLinker entityLinker,
        ILogger<EntityExtractionPipeline> logger)
    {
        _aiService = aiService;
        _cacheService = cacheService;
        _termMatcher = termMatcher;
        _entityLinker = entityLinker;
        _logger = logger;
    }

    public async Task<List<BusinessEntity>> ExtractEntitiesAsync(string userQuestion)
    {
        var cacheKey = $"entity_extraction:{userQuestion.GetHashCode()}";
        var (found, cachedResult) = await _cacheService.TryGetAsync<List<BusinessEntity>>(cacheKey);
        if (found && cachedResult != null)
        {
            return cachedResult;
        }

        _logger.LogInformation("Extracting entities from question: {Question}", 
            userQuestion.Substring(0, Math.Min(100, userQuestion.Length)));

        // Run extraction methods in parallel
        var extractionTasks = new[]
        {
            ExtractWithPatternMatchingAsync(userQuestion),
            ExtractWithBusinessTermMatchingAsync(userQuestion),
            ExtractWithAIModelAsync(userQuestion),
            ExtractWithSemanticAnalysisAsync(userQuestion)
        };

        var results = await Task.WhenAll(extractionTasks);

        // Merge and deduplicate entities
        var allEntities = results.SelectMany(r => r).ToList();
        var mergedEntities = await MergeAndDeduplicateEntitiesAsync(allEntities);

        // Link entities to schema
        var linkedEntities = await _entityLinker.LinkToSchemaAsync(mergedEntities, userQuestion);

        // Score and filter entities
        var scoredEntities = await ScoreEntitiesAsync(linkedEntities, userQuestion);
        var finalEntities = scoredEntities.Where(e => e.ConfidenceScore > 0.6).ToList();

        // Cache result for 30 minutes
        await _cacheService.SetAsync(cacheKey, finalEntities, TimeSpan.FromMinutes(30));

        _logger.LogInformation("Extracted {Count} entities with confidence > 0.6", finalEntities.Count);

        return finalEntities;
    }

    private async Task<List<BusinessEntity>> ExtractWithPatternMatchingAsync(string userQuestion)
    {
        var entities = new List<BusinessEntity>();

        foreach (var (entityType, patterns) in EntityPatterns)
        {
            foreach (var pattern in patterns)
            {
                var matches = pattern.Matches(userQuestion);
                foreach (Match match in matches)
                {
                    var entityName = match.Groups.Count > 2 ? match.Groups[2].Value : match.Groups[1].Value;
                    if (!string.IsNullOrWhiteSpace(entityName) && entityName.Length > 1)
                    {
                        entities.Add(new BusinessEntity
                        {
                            Name = entityName.Trim(),
                            Type = entityType,
                            OriginalText = match.Value,
                            ConfidenceScore = 0.7, // Pattern matching gets moderate confidence
                            ExtractionMethod = "PatternMatching",
                            Position = match.Index,
                            Metadata = new Dictionary<string, object>
                            {
                                ["pattern"] = pattern.ToString(),
                                ["match_groups"] = match.Groups.Cast<Group>().Select(g => g.Value).ToList()
                            }
                        });
                    }
                }
            }
        }

        return entities;
    }

    private async Task<List<BusinessEntity>> ExtractWithBusinessTermMatchingAsync(string userQuestion)
    {
        var entities = new List<BusinessEntity>();
        var words = userQuestion.ToLower().Split(' ', StringSplitOptions.RemoveEmptyEntries);

        foreach (var word in words)
        {
            // Direct term matching
            if (BusinessTermCategories.TryGetValue(word, out var entityType))
            {
                entities.Add(new BusinessEntity
                {
                    Name = word,
                    Type = entityType,
                    OriginalText = word,
                    ConfidenceScore = 0.9, // High confidence for direct business term matches
                    ExtractionMethod = "BusinessTermMatching",
                    Position = userQuestion.IndexOf(word, StringComparison.OrdinalIgnoreCase)
                });
            }

            // Fuzzy matching for similar terms
            var similarTerms = await _termMatcher.FindSimilarTermsAsync(word);
            foreach (var (term, similarity) in similarTerms.Where(t => t.similarity > 0.8))
            {
                if (BusinessTermCategories.TryGetValue(term, out var fuzzyEntityType))
                {
                    entities.Add(new BusinessEntity
                    {
                        Name = term,
                        Type = fuzzyEntityType,
                        OriginalText = word,
                        ConfidenceScore = similarity * 0.8, // Reduce confidence for fuzzy matches
                        ExtractionMethod = "FuzzyBusinessTermMatching",
                        Position = userQuestion.IndexOf(word, StringComparison.OrdinalIgnoreCase),
                        Metadata = new Dictionary<string, object>
                        {
                            ["original_word"] = word,
                            ["matched_term"] = term,
                            ["similarity"] = similarity
                        }
                    });
                }
            }
        }

        return entities;
    }

    private async Task<List<BusinessEntity>> ExtractWithAIModelAsync(string userQuestion)
    {
        try
        {
            var prompt = $@"Extract business entities from this question. Identify and classify each entity:

ENTITY TYPES:
- Table: Database tables or data sources
- Column: Database columns or fields  
- Metric: Measures, KPIs, calculations (revenue, count, average, etc.)
- Dimension: Grouping attributes (country, category, time period, etc.)
- TimeReference: Time-related expressions (dates, periods, ranges)
- ComparisonValue: Comparison operators and values (greater than, top 10, etc.)

Question: {userQuestion}

Return ONLY a JSON array of entities:
[
  {{
    ""name"": ""entity_name"",
    ""type"": ""Table|Column|Metric|Dimension|TimeReference|ComparisonValue"",
    ""originalText"": ""text_from_question"",
    ""confidence"": 0.95
  }}
]";

            var response = await _aiService.GenerateSQLAsync(prompt);
            
            // Try to parse JSON response
            try
            {
                var jsonEntities = JsonSerializer.Deserialize<List<JsonEntityResult>>(response);
                return jsonEntities?.Select(je => new BusinessEntity
                {
                    Name = je.name,
                    Type = Enum.Parse<EntityType>(je.type, true),
                    OriginalText = je.originalText,
                    ConfidenceScore = Math.Min(je.confidence, 0.95), // Cap AI confidence
                    ExtractionMethod = "AIModel",
                    Position = userQuestion.IndexOf(je.originalText, StringComparison.OrdinalIgnoreCase)
                }).ToList() ?? new List<BusinessEntity>();
            }
            catch (JsonException ex)
            {
                _logger.LogWarning(ex, "Failed to parse AI model JSON response: {Response}", response);
                return new List<BusinessEntity>();
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "AI model entity extraction failed");
            return new List<BusinessEntity>();
        }
    }

    private async Task<List<BusinessEntity>> ExtractWithSemanticAnalysisAsync(string userQuestion)
    {
        var entities = new List<BusinessEntity>();
        var words = userQuestion.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        // Analyze word context and relationships
        for (int i = 0; i < words.Length; i++)
        {
            var word = words[i].ToLower().Trim('?', '.', ',', '!');
            
            // Context-based entity type inference
            var entityType = InferEntityTypeFromContext(word, words, i);
            if (entityType.HasValue)
            {
                var confidence = CalculateContextualConfidence(word, words, i, entityType.Value);
                if (confidence > 0.5)
                {
                    entities.Add(new BusinessEntity
                    {
                        Name = word,
                        Type = entityType.Value,
                        OriginalText = words[i],
                        ConfidenceScore = confidence,
                        ExtractionMethod = "SemanticAnalysis",
                        Position = userQuestion.IndexOf(words[i], StringComparison.OrdinalIgnoreCase),
                        Metadata = new Dictionary<string, object>
                        {
                            ["context_words"] = GetContextWords(words, i, 2),
                            ["position_in_sentence"] = i
                        }
                    });
                }
            }
        }

        return entities;
    }

    private EntityType? InferEntityTypeFromContext(string word, string[] words, int position)
    {
        var contextBefore = position > 0 ? words[position - 1].ToLower() : "";
        var contextAfter = position < words.Length - 1 ? words[position + 1].ToLower() : "";

        // Table indicators
        if (contextBefore.Contains("from") || contextBefore.Contains("join") || contextAfter.Contains("table"))
            return EntityType.Table;

        // Metric indicators
        if (contextBefore.Contains("total") || contextBefore.Contains("sum") || contextBefore.Contains("count") ||
            contextBefore.Contains("average") || contextBefore.Contains("max") || contextBefore.Contains("min"))
            return EntityType.Metric;

        // Dimension indicators
        if (contextBefore.Contains("by") || contextBefore.Contains("group") || contextAfter.Contains("wise"))
            return EntityType.Dimension;

        // Time reference indicators
        if (contextBefore.Contains("last") || contextBefore.Contains("past") || contextBefore.Contains("next") ||
            word.Contains("day") || word.Contains("week") || word.Contains("month") || word.Contains("year"))
            return EntityType.TimeReference;

        return null;
    }

    private double CalculateContextualConfidence(string word, string[] words, int position, EntityType entityType)
    {
        var baseConfidence = 0.6;
        var contextWords = GetContextWords(words, position, 2);
        
        // Boost confidence based on strong contextual indicators
        var strongIndicators = entityType switch
        {
            EntityType.Table => new[] { "from", "join", "table", "data" },
            EntityType.Metric => new[] { "total", "sum", "count", "average", "revenue", "profit" },
            EntityType.Dimension => new[] { "by", "group", "category", "type", "segment" },
            EntityType.TimeReference => new[] { "last", "past", "during", "since", "until" },
            _ => Array.Empty<string>()
        };

        var indicatorMatches = contextWords.Count(cw => strongIndicators.Contains(cw.ToLower()));
        return Math.Min(baseConfidence + (indicatorMatches * 0.15), 0.95);
    }

    private List<string> GetContextWords(string[] words, int position, int windowSize)
    {
        var start = Math.Max(0, position - windowSize);
        var end = Math.Min(words.Length - 1, position + windowSize);
        
        return words.Skip(start).Take(end - start + 1).ToList();
    }

    private async Task<List<BusinessEntity>> MergeAndDeduplicateEntitiesAsync(List<BusinessEntity> entities)
    {
        var merged = new List<BusinessEntity>();
        var processed = new HashSet<string>();

        // Group similar entities
        var groups = entities.GroupBy(e => new { Name = e.Name.ToLower(), Type = e.Type });

        foreach (var group in groups)
        {
            var key = $"{group.Key.Name}_{group.Key.Type}";
            if (processed.Contains(key)) continue;

            var bestEntity = group.OrderByDescending(e => e.ConfidenceScore).First();
            
            // Merge metadata from all similar entities
            var allMethods = group.Select(e => e.ExtractionMethod).Distinct().ToList();
            var avgConfidence = group.Average(e => e.ConfidenceScore);
            
            bestEntity.ConfidenceScore = Math.Min(avgConfidence * 1.1, 0.98); // Boost for multiple confirmations
            bestEntity.Metadata["extraction_methods"] = allMethods;
            bestEntity.Metadata["confirmation_count"] = group.Count();
            
            merged.Add(bestEntity);
            processed.Add(key);
        }

        return merged;
    }

    private async Task<List<BusinessEntity>> ScoreEntitiesAsync(List<BusinessEntity> entities, string userQuestion)
    {
        foreach (var entity in entities)
        {
            // Apply additional scoring factors
            var positionScore = CalculatePositionScore(entity.Position, userQuestion.Length);
            var lengthScore = CalculateLengthScore(entity.Name);
            var methodScore = CalculateMethodScore(entity.ExtractionMethod);
            
            // Combine scores with weights
            entity.ConfidenceScore = (entity.ConfidenceScore * 0.6) + 
                                   (positionScore * 0.15) + 
                                   (lengthScore * 0.1) + 
                                   (methodScore * 0.15);
            
            entity.Metadata["position_score"] = positionScore;
            entity.Metadata["length_score"] = lengthScore;
            entity.Metadata["method_score"] = methodScore;
        }

        return entities.OrderByDescending(e => e.ConfidenceScore).ToList();
    }

    private double CalculatePositionScore(int position, int totalLength)
    {
        // Entities mentioned earlier in the question are often more important
        var relativePosition = (double)position / totalLength;
        return 1.0 - (relativePosition * 0.3); // Max 30% penalty for late position
    }

    private double CalculateLengthScore(string entityName)
    {
        // Prefer entities with reasonable length (not too short or too long)
        var length = entityName.Length;
        if (length < 2) return 0.3;
        if (length >= 2 && length <= 15) return 1.0;
        if (length > 15) return Math.Max(0.5, 1.0 - ((length - 15) * 0.05));
        return 0.5;
    }

    private double CalculateMethodScore(string method)
    {
        return method switch
        {
            "BusinessTermMatching" => 1.0,
            "AIModel" => 0.9,
            "PatternMatching" => 0.8,
            "FuzzyBusinessTermMatching" => 0.7,
            "SemanticAnalysis" => 0.6,
            _ => 0.5
        };
    }

    private record JsonEntityResult(string name, string type, string originalText, double confidence);
}

public interface IEntityExtractionPipeline
{
    Task<List<BusinessEntity>> ExtractEntitiesAsync(string userQuestion);
}

public interface IBusinessTermMatcher
{
    Task<List<(string term, double similarity)>> FindSimilarTermsAsync(string searchTerm);
}

public interface ISemanticEntityLinker
{
    Task<List<BusinessEntity>> LinkToSchemaAsync(List<BusinessEntity> entities, string userQuestion);
}
