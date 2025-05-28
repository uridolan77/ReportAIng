using Azure.AI.OpenAI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using BIReportingCopilot.Core.Interfaces;
using BIReportingCopilot.Core.Models;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace BIReportingCopilot.Infrastructure.AI;

public class SemanticAnalyzer : ISemanticAnalyzer
{
    private readonly OpenAIClient _client;
    private readonly IConfiguration _configuration;
    private readonly ILogger<SemanticAnalyzer> _logger;
    private readonly ICacheService _cacheService;
    private readonly bool _isConfigured;

    // Pre-defined entity patterns for quick recognition
    private readonly Dictionary<EntityType, List<string>> _entityPatterns = new()
    {
        [EntityType.Aggregation] = new() { "sum", "count", "average", "avg", "total", "max", "min", "group" },
        [EntityType.DateRange] = new() { "last", "past", "recent", "since", "until", "between", "month", "year", "day", "week" },
        [EntityType.Condition] = new() { "where", "filter", "only", "exclude", "include", "greater", "less", "equal" },
        [EntityType.Sort] = new() { "top", "bottom", "highest", "lowest", "best", "worst", "order", "sort" },
        [EntityType.Limit] = new() { "top", "first", "last", "limit", "maximum", "minimum" }
    };

    public SemanticAnalyzer(
        OpenAIClient client,
        IConfiguration configuration,
        ILogger<SemanticAnalyzer> logger,
        ICacheService cacheService)
    {
        _client = client;
        _configuration = configuration;
        _logger = logger;
        _cacheService = cacheService;
        _isConfigured = !string.IsNullOrEmpty(configuration["OpenAI:ApiKey"]) || 
                       !string.IsNullOrEmpty(configuration["AzureOpenAI:ApiKey"]);
    }

    public async Task<SemanticAnalysis> AnalyzeAsync(string naturalLanguageQuery)
    {
        try
        {
            _logger.LogDebug("Starting semantic analysis for query: {Query}", naturalLanguageQuery);

            // Check cache first
            var cacheKey = $"semantic_analysis:{naturalLanguageQuery.GetHashCode()}";
            var cachedResult = await _cacheService.GetAsync<SemanticAnalysis>(cacheKey);
            if (cachedResult != null)
            {
                _logger.LogDebug("Returning cached semantic analysis");
                return cachedResult;
            }

            var analysis = new SemanticAnalysis
            {
                OriginalQuery = naturalLanguageQuery,
                ProcessedQuery = naturalLanguageQuery.Trim()
            };

            // Extract entities using pattern matching
            analysis.Entities = await ExtractEntitiesAsync(naturalLanguageQuery);
            
            // Extract keywords
            analysis.Keywords = ExtractKeywords(naturalLanguageQuery);
            
            // Classify intent
            analysis.Intent = await ClassifyIntentAsync(naturalLanguageQuery);
            
            // Calculate confidence based on entity recognition and pattern matching
            analysis.ConfidenceScore = CalculateConfidenceScore(analysis);

            // Add metadata
            analysis.Metadata = ExtractMetadata(naturalLanguageQuery, analysis);

            // Cache the result
            await _cacheService.SetAsync(cacheKey, analysis, TimeSpan.FromHours(1));

            _logger.LogDebug("Semantic analysis completed with confidence: {Confidence}", analysis.ConfidenceScore);
            return analysis;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during semantic analysis");
            return CreateFallbackAnalysis(naturalLanguageQuery);
        }
    }

    public async Task<float[]> GenerateEmbeddingAsync(string text)
    {
        if (!_isConfigured)
        {
            // Return a simple hash-based embedding as fallback
            return GenerateSimpleEmbedding(text);
        }

        try
        {
            var cacheKey = $"embedding:{text.GetHashCode()}";
            var cachedEmbedding = await _cacheService.GetAsync<float[]>(cacheKey);
            if (cachedEmbedding != null)
            {
                return cachedEmbedding;
            }

            // Use OpenAI embeddings API
            var embeddingsOptions = new EmbeddingsOptions(
                deploymentName: _configuration["OpenAI:EmbeddingModel"] ?? "text-embedding-ada-002",
                input: new[] { text }
            );

            var response = await _client.GetEmbeddingsAsync(embeddingsOptions);
            var embedding = response.Value.Data[0].Embedding.ToArray();

            // Cache the embedding
            await _cacheService.SetAsync(cacheKey, embedding, TimeSpan.FromDays(7));

            return embedding;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating embedding for text: {Text}", text);
            return GenerateSimpleEmbedding(text);
        }
    }

    public async Task<SemanticSimilarity> CalculateSimilarityAsync(string query1, string query2)
    {
        try
        {
            var embedding1 = await GenerateEmbeddingAsync(query1);
            var embedding2 = await GenerateEmbeddingAsync(query2);

            var similarity = CalculateCosineSimilarity(embedding1, embedding2);

            var analysis1 = await AnalyzeAsync(query1);
            var analysis2 = await AnalyzeAsync(query2);

            return new SemanticSimilarity
            {
                Query1 = query1,
                Query2 = query2,
                SimilarityScore = similarity,
                CommonEntities = analysis1.Entities
                    .Where(e1 => analysis2.Entities.Any(e2 => e2.Text.Equals(e1.Text, StringComparison.OrdinalIgnoreCase)))
                    .Select(e => e.Text)
                    .ToList(),
                CommonKeywords = analysis1.Keywords
                    .Intersect(analysis2.Keywords, StringComparer.OrdinalIgnoreCase)
                    .ToList()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating semantic similarity");
            return new SemanticSimilarity
            {
                Query1 = query1,
                Query2 = query2,
                SimilarityScore = 0.0
            };
        }
    }

    public async Task<List<SemanticEntity>> ExtractEntitiesAsync(string query)
    {
        var entities = new List<SemanticEntity>();
        var lowerQuery = query.ToLowerInvariant();

        // Extract entities using pattern matching
        foreach (var (entityType, patterns) in _entityPatterns)
        {
            foreach (var pattern in patterns)
            {
                var matches = Regex.Matches(lowerQuery, $@"\b{Regex.Escape(pattern)}\b", RegexOptions.IgnoreCase);
                foreach (Match match in matches)
                {
                    entities.Add(new SemanticEntity
                    {
                        Text = match.Value,
                        Type = entityType,
                        Confidence = 0.8, // High confidence for pattern matches
                        StartPosition = match.Index,
                        EndPosition = match.Index + match.Length
                    });
                }
            }
        }

        // Extract numbers and dates
        entities.AddRange(ExtractNumbers(query));
        entities.AddRange(ExtractDates(query));

        // Remove duplicates and sort by position
        return entities
            .GroupBy(e => new { e.Text, e.Type })
            .Select(g => g.First())
            .OrderBy(e => e.StartPosition)
            .ToList();
    }

    public async Task<QueryIntent> ClassifyIntentAsync(string query)
    {
        var lowerQuery = query.ToLowerInvariant();

        // Aggregation intent
        if (lowerQuery.Contains("sum") || lowerQuery.Contains("total") || lowerQuery.Contains("count") ||
            lowerQuery.Contains("average") || lowerQuery.Contains("avg") || lowerQuery.Contains("group"))
        {
            return QueryIntent.Aggregation;
        }

        // Trend intent
        if (lowerQuery.Contains("trend") || lowerQuery.Contains("over time") || lowerQuery.Contains("growth") ||
            lowerQuery.Contains("change") || lowerQuery.Contains("monthly") || lowerQuery.Contains("yearly"))
        {
            return QueryIntent.Trend;
        }

        // Comparison intent
        if (lowerQuery.Contains("compare") || lowerQuery.Contains("vs") || lowerQuery.Contains("versus") ||
            lowerQuery.Contains("top") || lowerQuery.Contains("best") || lowerQuery.Contains("highest"))
        {
            return QueryIntent.Comparison;
        }

        // Filtering intent
        if (lowerQuery.Contains("where") || lowerQuery.Contains("filter") || lowerQuery.Contains("only") ||
            lowerQuery.Contains("specific") || lowerQuery.Contains("between"))
        {
            return QueryIntent.Filtering;
        }

        return QueryIntent.General;
    }

    private List<string> ExtractKeywords(string query)
    {
        var words = query.ToLowerInvariant()
            .Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Where(w => w.Length > 2 && !IsStopWord(w))
            .Distinct()
            .ToList();

        return words;
    }

    private bool IsStopWord(string word)
    {
        var stopWords = new HashSet<string> { "the", "and", "or", "but", "in", "on", "at", "to", "for", "of", "with", "by", "is", "are", "was", "were", "be", "been", "have", "has", "had", "do", "does", "did", "will", "would", "could", "should", "may", "might", "can", "a", "an", "this", "that", "these", "those" };
        return stopWords.Contains(word);
    }

    private double CalculateConfidenceScore(SemanticAnalysis analysis)
    {
        var score = 0.5; // Base score

        // Boost for recognized entities
        score += Math.Min(0.3, analysis.Entities.Count * 0.05);

        // Boost for clear intent
        if (analysis.Intent != QueryIntent.General)
            score += 0.2;

        // Boost for meaningful keywords
        score += Math.Min(0.2, analysis.Keywords.Count * 0.02);

        return Math.Min(1.0, score);
    }

    private Dictionary<string, object> ExtractMetadata(string query, SemanticAnalysis analysis)
    {
        return new Dictionary<string, object>
        {
            ["word_count"] = query.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length,
            ["entity_count"] = analysis.Entities.Count,
            ["keyword_count"] = analysis.Keywords.Count,
            ["has_aggregation"] = analysis.Entities.Any(e => e.Type == EntityType.Aggregation),
            ["has_date_filter"] = analysis.Entities.Any(e => e.Type == EntityType.DateRange),
            ["complexity_level"] = DetermineComplexity(analysis)
        };
    }

    private string DetermineComplexity(SemanticAnalysis analysis)
    {
        var entityCount = analysis.Entities.Count;
        var keywordCount = analysis.Keywords.Count;

        if (entityCount >= 5 || keywordCount >= 8)
            return "high";
        if (entityCount >= 3 || keywordCount >= 5)
            return "medium";
        return "low";
    }

    private List<SemanticEntity> ExtractNumbers(string query)
    {
        var entities = new List<SemanticEntity>();
        var numberPattern = @"\b\d+(?:\.\d+)?\b";
        var matches = Regex.Matches(query, numberPattern);

        foreach (Match match in matches)
        {
            entities.Add(new SemanticEntity
            {
                Text = match.Value,
                Type = EntityType.Number,
                Confidence = 0.9,
                StartPosition = match.Index,
                EndPosition = match.Index + match.Length,
                ResolvedValue = match.Value
            });
        }

        return entities;
    }

    private List<SemanticEntity> ExtractDates(string query)
    {
        var entities = new List<SemanticEntity>();
        var datePatterns = new[]
        {
            @"\b\d{4}-\d{2}-\d{2}\b", // YYYY-MM-DD
            @"\b\d{1,2}/\d{1,2}/\d{4}\b", // MM/DD/YYYY
            @"\b\d{1,2}-\d{1,2}-\d{4}\b", // MM-DD-YYYY
        };

        foreach (var pattern in datePatterns)
        {
            var matches = Regex.Matches(query, pattern);
            foreach (Match match in matches)
            {
                entities.Add(new SemanticEntity
                {
                    Text = match.Value,
                    Type = EntityType.DateRange,
                    Confidence = 0.95,
                    StartPosition = match.Index,
                    EndPosition = match.Index + match.Length,
                    ResolvedValue = match.Value
                });
            }
        }

        return entities;
    }

    private float[] GenerateSimpleEmbedding(string text)
    {
        // Simple hash-based embedding as fallback
        var hash = text.GetHashCode();
        var embedding = new float[384]; // Standard embedding size
        var random = new Random(hash);
        
        for (int i = 0; i < embedding.Length; i++)
        {
            embedding[i] = (float)(random.NextDouble() * 2 - 1); // Range [-1, 1]
        }
        
        return embedding;
    }

    private double CalculateCosineSimilarity(float[] vector1, float[] vector2)
    {
        if (vector1.Length != vector2.Length)
            return 0.0;

        var dotProduct = vector1.Zip(vector2, (a, b) => a * b).Sum();
        var magnitude1 = Math.Sqrt(vector1.Sum(x => x * x));
        var magnitude2 = Math.Sqrt(vector2.Sum(x => x * x));

        if (magnitude1 == 0 || magnitude2 == 0)
            return 0.0;

        return dotProduct / (magnitude1 * magnitude2);
    }

    private SemanticAnalysis CreateFallbackAnalysis(string query)
    {
        return new SemanticAnalysis
        {
            OriginalQuery = query,
            ProcessedQuery = query,
            Intent = QueryIntent.General,
            Entities = new List<SemanticEntity>(),
            Keywords = ExtractKeywords(query),
            ConfidenceScore = 0.3,
            Metadata = new Dictionary<string, object> { ["fallback"] = true }
        };
    }
}
