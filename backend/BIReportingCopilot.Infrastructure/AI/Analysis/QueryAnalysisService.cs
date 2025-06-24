using Azure.AI.OpenAI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using BIReportingCopilot.Core.Interfaces;
using BIReportingCopilot.Core.Interfaces.AI;
using BIReportingCopilot.Core.Interfaces.Cache;
using BIReportingCopilot.Core.Interfaces.Query;
using BIReportingCopilot.Core.Models;
using System.Text.Json;
using System.Text.RegularExpressions;
using IQueryClassifier = BIReportingCopilot.Core.Interfaces.IQueryClassifier;
using ISemanticAnalyzer = BIReportingCopilot.Core.Interfaces.AI.ISemanticAnalyzer;

namespace BIReportingCopilot.Infrastructure.AI.Analysis;

/// <summary>
/// Unified query analysis service combining semantic analysis and query classification
/// Consolidates functionality from SemanticAnalyzer and QueryClassifier
/// </summary>
public class QueryAnalysisService : ISemanticAnalyzer, IQueryClassifier
{
    private readonly Lazy<OpenAIClient> _lazyClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<QueryAnalysisService> _logger;
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

    // Classification rules and patterns
    private readonly Dictionary<QueryCategory, List<string>> _categoryKeywords = new()
    {
        [QueryCategory.Reporting] = new() { "report", "summary", "overview", "dashboard", "status" },
        [QueryCategory.Analytics] = new() { "analyze", "analysis", "trend", "pattern", "correlation", "insight" },
        [QueryCategory.Lookup] = new() { "find", "search", "get", "show", "display", "list" },
        [QueryCategory.Aggregation] = new() { "sum", "count", "total", "average", "max", "min", "group" },
        [QueryCategory.Trend] = new() { "trend", "over time", "growth", "change", "monthly", "yearly", "progression" },
        [QueryCategory.Comparison] = new() { "compare", "vs", "versus", "difference", "between", "against" },
        [QueryCategory.Filtering] = new() { "where", "filter", "only", "specific", "exclude", "include" },
        [QueryCategory.Export] = new() { "export", "download", "extract", "save", "output" }
    };

    public QueryAnalysisService(
        Lazy<OpenAIClient> lazyClient,
        IConfiguration configuration,
        ILogger<QueryAnalysisService> logger,
        ICacheService cacheService)
    {
        _lazyClient = lazyClient;
        _configuration = configuration;
        _logger = logger;
        _cacheService = cacheService;
        _isConfigured = !string.IsNullOrEmpty(configuration["OpenAI:ApiKey"]) ||
                       !string.IsNullOrEmpty(configuration["AzureOpenAI:ApiKey"]);

        _logger.LogInformation("🔧 QueryAnalysisService initialized with lazy OpenAI client (prevents hanging)");
    }

    /// <summary>
    /// Get the OpenAI client when needed (lazy initialization)
    /// </summary>
    private OpenAIClient GetClient()
    {
        try
        {
            return _lazyClient.Value;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting OpenAI client, AI features will be limited");
            throw new InvalidOperationException("OpenAI client is not available", ex);
        }
    }

    #region ISemanticAnalyzer Implementation

    public async Task<SemanticAnalysisResult> AnalyzeAsync(string naturalLanguageQuery, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Starting semantic analysis for query: {Query}", naturalLanguageQuery);

            // Check cache first
            var cacheKey = $"semantic_analysis:{naturalLanguageQuery.GetHashCode()}";
            var cachedResult = await _cacheService.GetAsync<SemanticAnalysisResult>(cacheKey);
            if (cachedResult != null)
            {
                _logger.LogDebug("Returning cached semantic analysis");
                return cachedResult;
            }

            // Add timeout for AI operations
            using var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
            using var combinedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);

            _logger.LogDebug("🔍 Starting entity extraction with timeout");
            var entities = await ExtractEntitiesAsync(naturalLanguageQuery, combinedCts.Token);

            _logger.LogDebug("🔍 Starting keyword extraction");
            var keywords = ExtractKeywords(naturalLanguageQuery);

            _logger.LogDebug("🔍 Starting intent classification with timeout");
            var intent = await ClassifyIntentAsync(naturalLanguageQuery, combinedCts.Token);

            var analysis = new SemanticAnalysisResult
            {
                Text = naturalLanguageQuery,
                Keywords = keywords,
                Entities = entities,
                ConfidenceScore = CalculateConfidenceScore(entities, keywords, intent),
                Metadata = ExtractMetadata(naturalLanguageQuery, entities, keywords)
            };

            // Cache the result
            await _cacheService.SetAsync(cacheKey, analysis, TimeSpan.FromHours(1));

            _logger.LogDebug("Semantic analysis completed with confidence: {Confidence}", analysis.ConfidenceScore);
            return analysis;
        }
        catch (OperationCanceledException ex) when (ex.CancellationToken.IsCancellationRequested)
        {
            _logger.LogWarning("Semantic analysis timed out for query: {Query}", naturalLanguageQuery);
            return CreateFallbackSemanticAnalysisResult(naturalLanguageQuery);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during semantic analysis");
            return CreateFallbackSemanticAnalysisResult(naturalLanguageQuery);
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

            var client = GetClient();
            var response = await client.GetEmbeddingsAsync(embeddingsOptions);
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

    public Task<List<EntityExtraction>> ExtractEntitiesAsync(string query, CancellationToken cancellationToken = default)
    {
        var entities = new List<EntityExtraction>();
        var lowerQuery = query.ToLowerInvariant();

        // Extract entities using pattern matching
        foreach (var (entityType, patterns) in _entityPatterns)
        {
            foreach (var pattern in patterns)
            {
                var matches = Regex.Matches(lowerQuery, $@"\b{Regex.Escape(pattern)}\b", RegexOptions.IgnoreCase);
                foreach (Match match in matches)
                {                    entities.Add(new EntityExtraction
                    {
                        Text = match.Value,
                        Type = entityType.ToString(),
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
        var result = entities
            .GroupBy(e => new { e.Text, e.Type })
            .Select(g => g.First())
            .OrderBy(e => e.StartPosition)
            .ToList();

        return Task.FromResult(result);
    }

    public Task<BIReportingCopilot.Core.Models.QueryIntent> ClassifyIntentAsync(string query, CancellationToken cancellationToken = default)
    {
        var lowerQuery = query.ToLowerInvariant();

        // Aggregation intent
        if (lowerQuery.Contains("sum") || lowerQuery.Contains("total") || lowerQuery.Contains("count") ||
            lowerQuery.Contains("average") || lowerQuery.Contains("avg") || lowerQuery.Contains("group"))
        {
            return Task.FromResult(BIReportingCopilot.Core.Models.QueryIntent.Aggregation);
        }

        // Trend intent
        if (lowerQuery.Contains("trend") || lowerQuery.Contains("over time") || lowerQuery.Contains("growth") ||
            lowerQuery.Contains("change") || lowerQuery.Contains("monthly") || lowerQuery.Contains("yearly"))
        {
            return Task.FromResult(BIReportingCopilot.Core.Models.QueryIntent.Trend);
        }

        // Comparison intent
        if (lowerQuery.Contains("compare") || lowerQuery.Contains("vs") || lowerQuery.Contains("versus") ||
            lowerQuery.Contains("top") || lowerQuery.Contains("best") || lowerQuery.Contains("highest"))
        {
            return Task.FromResult(BIReportingCopilot.Core.Models.QueryIntent.Comparison);
        }

        // Filtering intent
        if (lowerQuery.Contains("where") || lowerQuery.Contains("filter") || lowerQuery.Contains("only") ||
            lowerQuery.Contains("specific") || lowerQuery.Contains("between"))
        {
            return Task.FromResult(BIReportingCopilot.Core.Models.QueryIntent.Filtering);
        }

        return Task.FromResult(BIReportingCopilot.Core.Models.QueryIntent.General);
    }

    #endregion

    #region IQueryClassifier Implementation

    public async Task<QueryClassification> ClassifyQueryAsync(string query)
    {
        try
        {
            _logger.LogDebug("Classifying query: {Query}", query);

            // Check cache first
            var cacheKey = $"query_classification:{query.GetHashCode()}";
            var cachedResult = await _cacheService.GetAsync<QueryClassification>(cacheKey);
            if (cachedResult != null)
            {
                return cachedResult;
            }            // Get semantic analysis (reuse from ISemanticAnalyzer implementation)
            var semanticAnalysisResult = await AnalyzeAsync(query);
            var semanticAnalysis = ConvertToSemanticAnalysis(semanticAnalysisResult);

            var classification = new QueryClassification
            {
                Category = ClassifyCategory(query, semanticAnalysis),
                Complexity = await DetermineComplexity(query, semanticAnalysis),
                RequiredJoins = PredictRequiredJoins(query, semanticAnalysis),
                PredictedTables = PredictTables(query, semanticAnalysis),
                EstimatedExecutionTime = EstimateExecutionTime(query, semanticAnalysis),
                RecommendedVisualization = RecommendVisualization(query, semanticAnalysis),
                ConfidenceScore = CalculateClassificationConfidence(query, semanticAnalysis),
                OptimizationSuggestions = GenerateOptimizationSuggestions(query, semanticAnalysis)
            };

            // Cache the result
            await _cacheService.SetAsync(cacheKey, classification, TimeSpan.FromMinutes(30));

            _logger.LogDebug("Query classified as {Category} with {Complexity} complexity",
                classification.Category, classification.Complexity);

            return classification;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error classifying query");
            return CreateFallbackClassification(query);
        }
    }

    public async Task<QueryComplexityScore> AnalyzeComplexityAsync(string query)    {
        try
        {
            var semanticAnalysisResult = await AnalyzeAsync(query);
            var semanticAnalysis = ConvertToSemanticAnalysis(semanticAnalysisResult);
            var factors = new List<ComplexityFactor>();
            var score = 0;

            // Analyze various complexity factors
            var joinCount = CountJoins(query);
            if (joinCount > 0)
            {
                factors.Add(new ComplexityFactor
                {
                    Name = "Joins",
                    Impact = joinCount * 2,
                    Description = $"Query requires {joinCount} table join(s)"
                });
                score += joinCount * 2;
            }

            var aggregationCount = CountAggregations(query, semanticAnalysis);
            if (aggregationCount > 0)
            {
                factors.Add(new ComplexityFactor
                {
                    Name = "Aggregations",
                    Impact = aggregationCount,
                    Description = $"Query uses {aggregationCount} aggregation function(s)"
                });
                score += aggregationCount;
            }

            var conditionCount = CountConditions(query);
            if (conditionCount > 2)
            {
                factors.Add(new ComplexityFactor
                {
                    Name = "Conditions",
                    Impact = conditionCount - 2,
                    Description = $"Query has {conditionCount} filter conditions"
                });
                score += conditionCount - 2;
            }

            var subqueryCount = CountSubqueries(query);
            if (subqueryCount > 0)
            {
                factors.Add(new ComplexityFactor
                {
                    Name = "Subqueries",
                    Impact = subqueryCount * 3,
                    Description = $"Query contains {subqueryCount} subquery/subqueries"
                });
                score += subqueryCount * 3;
            }

            var complexity = score switch
            {
                <= 3 => QueryComplexity.Low,
                <= 8 => QueryComplexity.Medium,
                _ => QueryComplexity.High
            };

            var recommendations = GenerateComplexityRecommendations(complexity, factors);

            return new QueryComplexityScore
            {
                Level = complexity,
                Score = score,
                Factors = factors,
                Recommendations = recommendations
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing query complexity");
            return new QueryComplexityScore
            {
                Level = QueryComplexity.Medium,
                Score = 5,
                Factors = new List<ComplexityFactor>(),
                Recommendations = new List<string> { "Unable to analyze complexity due to error" }
            };
        }
    }

    public Task<List<string>> SuggestOptimizationsAsync(string query)
    {
        var suggestions = new List<string>();
        var lowerQuery = query.ToLowerInvariant();

        // Check for SELECT *
        if (lowerQuery.Contains("select *"))
        {
            suggestions.Add("Consider selecting only the columns you need instead of using SELECT *");
        }

        // Check for missing WHERE clause
        if (!lowerQuery.Contains("where") && !lowerQuery.Contains("limit") && !lowerQuery.Contains("top"))
        {
            suggestions.Add("Consider adding a WHERE clause to limit the result set");
        }

        // Check for ORDER BY without LIMIT
        if (lowerQuery.Contains("order by") && !lowerQuery.Contains("limit") && !lowerQuery.Contains("top"))
        {
            suggestions.Add("Consider adding a LIMIT clause when using ORDER BY for better performance");
        }

        // Check for complex joins
        var joinCount = CountJoins(query);
        if (joinCount > 3)
        {
            suggestions.Add("Consider breaking down complex joins into smaller, more manageable queries");
        }

        // Check for functions in WHERE clause
        if (Regex.IsMatch(lowerQuery, @"where\s+\w+\s*\(\s*\w+\s*\)"))
        {
            suggestions.Add("Avoid using functions in WHERE clauses as they can prevent index usage");
        }

        return Task.FromResult(suggestions);
    }

    public Task<bool> RequiresJoinAsync(string query, SchemaMetadata schema)
    {
        var lowerQuery = query.ToLowerInvariant();

        // Direct join keywords
        if (lowerQuery.Contains("join") || lowerQuery.Contains("inner") || lowerQuery.Contains("left") || lowerQuery.Contains("right"))
        {
            return Task.FromResult(true);
        }

        // Multiple table references
        var tableCount = schema.Tables.Count(table =>
            lowerQuery.Contains(table.Name.ToLowerInvariant()));

        return Task.FromResult(tableCount > 1);
    }

    public Task<QueryComplexityScore> GetComplexityScoreAsync(string query)
    {
        return AnalyzeComplexityAsync(query);
    }

    public Task<BIReportingCopilot.Core.Models.QueryIntent> DetermineIntentAsync(string query)
    {
        return ClassifyIntentAsync(query);
    }

    public Task<List<string>> IdentifyRequiredTablesAsync(string query, SchemaMetadata schema)
    {
        try
        {
            var requiredTables = new List<string>();
            var lowerQuery = query.ToLowerInvariant();

            // Find table references in the query
            foreach (var table in schema.Tables)
            {
                if (lowerQuery.Contains(table.Name.ToLowerInvariant()))
                {
                    requiredTables.Add(table.Name);
                }
            }

            return Task.FromResult(requiredTables);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error identifying required tables");
            return Task.FromResult(new List<string>());
        }
    }

    public Task<VisualizationType> SuggestVisualizationAsync(string query)
    {
        try
        {
            var lowerQuery = query.ToLowerInvariant();

            // Aggregation queries -> Bar/Pie charts
            if (lowerQuery.Contains("sum") || lowerQuery.Contains("count") || lowerQuery.Contains("total"))
            {
                if (lowerQuery.Contains("group by") || lowerQuery.Contains("category"))
                    return Task.FromResult(VisualizationType.Bar);
                else
                    return Task.FromResult(VisualizationType.Pie);
            }

            // Trend queries -> Line charts
            if (lowerQuery.Contains("trend") || lowerQuery.Contains("over time") ||
                lowerQuery.Contains("monthly") || lowerQuery.Contains("yearly"))
            {
                return Task.FromResult(VisualizationType.Line);
            }

            // Comparison queries -> Bar charts
            if (lowerQuery.Contains("compare") || lowerQuery.Contains("vs") ||
                lowerQuery.Contains("top") || lowerQuery.Contains("best"))
            {
                return Task.FromResult(VisualizationType.Bar);
            }

            // Default to table for simple queries
            return Task.FromResult(VisualizationType.Table);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error suggesting visualization");
            return Task.FromResult(VisualizationType.Table);
        }
    }

    #endregion

    #region Private Helper Methods

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

    private double CalculateConfidenceScore(List<EntityExtraction> entities, List<string> keywords, QueryIntent intent)
    {
        var score = 0.5; // Base score

        // Boost for recognized entities
        score += Math.Min(0.3, entities.Count * 0.05);

        // Boost for clear intent
        if (intent != QueryIntent.General)
            score += 0.2;

        // Boost for meaningful keywords
        score += Math.Min(0.2, keywords.Count * 0.02);

        return Math.Min(1.0, score);
    }

    private Dictionary<string, object> ExtractMetadata(string query, List<EntityExtraction> entities, List<string> keywords)
    {
        return new Dictionary<string, object>
        {
            ["word_count"] = query.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length,
            ["entity_count"] = entities.Count,
            ["keyword_count"] = keywords.Count,            ["has_aggregation"] = entities.Any(e => e.Type == EntityType.Aggregation.ToString()),
            ["has_date_filter"] = entities.Any(e => e.Type == EntityType.DateRange.ToString()),
            ["complexity_level"] = "medium"
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

    private List<EntityExtraction> ExtractNumbers(string query)
    {
        var entities = new List<EntityExtraction>();
        var numberPattern = @"\b\d+(?:\.\d+)?\b";
        var matches = Regex.Matches(query, numberPattern);

        foreach (Match match in matches)
        {
            entities.Add(new EntityExtraction
            {
                Text = match.Value,                    Type = EntityType.Number.ToString(),
                Confidence = 0.9,
                StartPosition = match.Index,
                EndPosition = match.Index + match.Length,
                ResolvedValue = match.Value
            });
        }

        return entities;
    }

    private List<EntityExtraction> ExtractDates(string query)
    {
        var entities = new List<EntityExtraction>();
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
                entities.Add(new EntityExtraction
                {
                    Text = match.Value,
                    Type = EntityType.DateRange.ToString(),
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

    private SemanticAnalysisResult CreateFallbackSemanticAnalysisResult(string query)
    {
        return new SemanticAnalysisResult
        {
            Text = query,
            Entities = new List<EntityExtraction>(),
            Confidence = 0.3,
            Metadata = new Dictionary<string, object>
            {
                ["fallback"] = true,
                ["error"] = "Analysis failed"
            }
        };
    }

    private SemanticAnalysis CreateFallbackAnalysis(string query)
    {
        return new SemanticAnalysis
        {
            OriginalQuery = query,
            ProcessedQuery = query,
            Intent = QueryIntent.General,
            Entities = new List<EntityExtraction>(),
            Keywords = ExtractKeywords(query),
            ConfidenceScore = 0.3,
            Metadata = new Dictionary<string, object> { ["fallback"] = true }
        };
    }

    private QueryCategory ClassifyCategory(string query, SemanticAnalysis semanticAnalysis)
    {
        var lowerQuery = query.ToLowerInvariant();
        var scores = new Dictionary<QueryCategory, int>();

        // Initialize scores
        foreach (var category in Enum.GetValues<QueryCategory>())
        {
            scores[category] = 0;
        }

        // Score based on keywords
        foreach (var (category, keywords) in _categoryKeywords)
        {
            foreach (var keyword in keywords)
            {
                if (lowerQuery.Contains(keyword))
                {
                    scores[category] += 2;
                }
            }
        }

        // Score based on semantic analysis
        switch (semanticAnalysis.Intent)
        {
            case QueryIntent.Aggregation:
                scores[QueryCategory.Aggregation] += 3;
                break;
            case QueryIntent.Trend:
                scores[QueryCategory.Trend] += 3;
                break;
            case QueryIntent.Comparison:
                scores[QueryCategory.Comparison] += 3;
                break;
            case QueryIntent.Filtering:
                scores[QueryCategory.Filtering] += 3;
                break;
        }

        // Return category with highest score
        return scores.OrderByDescending(kvp => kvp.Value).First().Key;
    }

    private async Task<QueryComplexity> DetermineComplexity(string query, SemanticAnalysis semanticAnalysis)
    {
        var complexityScore = await AnalyzeComplexityAsync(query);
        return complexityScore.Level;
    }

    private List<string> PredictRequiredJoins(string query, SemanticAnalysis semanticAnalysis)
    {
        var joins = new List<string>();
        var lowerQuery = query.ToLowerInvariant();

        // Common join patterns
        var joinPatterns = new Dictionary<string, string>
        {
            ["customer"] = "customers.id = orders.customer_id",
            ["product"] = "products.id = order_items.product_id",
            ["order"] = "orders.id = order_items.order_id",
            ["user"] = "users.id = user_activities.user_id"
        };

        foreach (var (keyword, joinClause) in joinPatterns)
        {
            if (lowerQuery.Contains(keyword))
            {
                joins.Add(joinClause);
            }
        }

        return joins.Distinct().ToList();
    }

    private List<string> PredictTables(string query, SemanticAnalysis semanticAnalysis)
    {
        var tables = new List<string>();
        var lowerQuery = query.ToLowerInvariant();

        // Common table name patterns
        var tableKeywords = new Dictionary<string, string>
        {
            ["customer"] = "customers",
            ["order"] = "orders",
            ["product"] = "products",
            ["user"] = "users",
            ["sale"] = "sales",
            ["transaction"] = "transactions",
            ["payment"] = "payments",
            ["invoice"] = "invoices"
        };

        foreach (var (keyword, tableName) in tableKeywords)
        {
            if (lowerQuery.Contains(keyword))
            {
                tables.Add(tableName);
            }
        }

        return tables.Distinct().ToList();
    }

    private TimeSpan EstimateExecutionTime(string query, SemanticAnalysis semanticAnalysis)
    {
        var baseTime = TimeSpan.FromMilliseconds(100);
        var multiplier = 1.0;

        // Increase time based on complexity factors
        multiplier += CountJoins(query) * 0.5;
        multiplier += CountAggregations(query, semanticAnalysis) * 0.3;
        multiplier += CountSubqueries(query) * 0.8;

        if (query.ToLowerInvariant().Contains("order by"))
            multiplier += 0.2;

        return TimeSpan.FromMilliseconds(baseTime.TotalMilliseconds * multiplier);
    }

    private VisualizationType RecommendVisualization(string query, SemanticAnalysis semanticAnalysis)
    {
        var lowerQuery = query.ToLowerInvariant();

        // Time-based queries suggest line charts
        if (semanticAnalysis.Intent == QueryIntent.Trend ||
            lowerQuery.Contains("over time") || lowerQuery.Contains("monthly") || lowerQuery.Contains("yearly"))
        {
            return VisualizationType.LineChart;
        }

        // Aggregation queries suggest bar charts
        if (semanticAnalysis.Intent == QueryIntent.Aggregation ||
            lowerQuery.Contains("group by") || lowerQuery.Contains("sum") || lowerQuery.Contains("count"))
        {
            return VisualizationType.BarChart;
        }

        // Comparison queries suggest bar charts
        if (semanticAnalysis.Intent == QueryIntent.Comparison ||
            lowerQuery.Contains("top") || lowerQuery.Contains("compare"))
        {
            return VisualizationType.BarChart;
        }

        // Single value queries suggest KPI
        if (lowerQuery.Contains("total") && !lowerQuery.Contains("group"))
        {
            return VisualizationType.KPI;
        }

        // Default to table
        return VisualizationType.Table;
    }

    private double CalculateClassificationConfidence(string query, SemanticAnalysis semanticAnalysis)
    {
        var confidence = 0.5; // Base confidence

        // Boost for clear semantic intent
        if (semanticAnalysis.Intent != QueryIntent.General)
            confidence += 0.2;

        // Boost for recognized entities
        confidence += Math.Min(0.2, semanticAnalysis.Entities.Count * 0.03);

        // Boost for clear keywords
        confidence += Math.Min(0.1, semanticAnalysis.Keywords.Count * 0.01);

        return Math.Min(1.0, confidence);
    }

    private List<string> GenerateOptimizationSuggestions(string query, SemanticAnalysis semanticAnalysis)
    {
        var suggestions = new List<string>();

        if (CountJoins(query) > 2)
        {
            suggestions.Add("Consider using indexes on join columns for better performance");
        }

        if (!query.ToLowerInvariant().Contains("where") && !query.ToLowerInvariant().Contains("limit"))
        {
            suggestions.Add("Add WHERE clause or LIMIT to reduce result set size");
        }

        if (semanticAnalysis.Intent == QueryIntent.Aggregation && !query.ToLowerInvariant().Contains("group by"))
        {
            suggestions.Add("Consider using GROUP BY for aggregation queries");
        }

        return suggestions;
    }

    private int CountJoins(string query)
    {
        var lowerQuery = query.ToLowerInvariant();
        return Regex.Matches(lowerQuery, @"\bjoin\b").Count;
    }

    private int CountAggregations(string query, SemanticAnalysis semanticAnalysis)
    {
        return semanticAnalysis.Entities.Count(e => e.Type == EntityType.Aggregation.ToString());
    }

    private int CountConditions(string query)
    {
        var lowerQuery = query.ToLowerInvariant();
        return Regex.Matches(lowerQuery, @"\b(and|or)\b").Count + 1;
    }

    private int CountSubqueries(string query)
    {
        return Regex.Matches(query, @"\(.*select.*\)", RegexOptions.IgnoreCase).Count;
    }

    private List<string> GenerateComplexityRecommendations(QueryComplexity complexity, List<ComplexityFactor> factors)
    {
        var recommendations = new List<string>();

        switch (complexity)
        {
            case QueryComplexity.Low:
                recommendations.Add("Query complexity is low - should execute quickly");
                break;
            case QueryComplexity.Medium:
                recommendations.Add("Consider adding appropriate indexes for optimal performance");
                if (factors.Any(f => f.Name == "Joins"))
                    recommendations.Add("Ensure join columns are indexed");
                break;
            case QueryComplexity.High:
                recommendations.Add("High complexity query - consider breaking into smaller parts");
                recommendations.Add("Review execution plan and add necessary indexes");
                if (factors.Any(f => f.Name == "Subqueries"))
                    recommendations.Add("Consider converting subqueries to JOINs where possible");
                break;
        }

        return recommendations;
    }

    private QueryClassification CreateFallbackClassification(string query)
    {
        return new QueryClassification
        {
            Category = QueryCategory.Lookup,
            Complexity = QueryComplexity.Medium,
            RequiredJoins = new List<string>(),
            PredictedTables = new List<string>(),
            EstimatedExecutionTime = TimeSpan.FromSeconds(1),
            RecommendedVisualization = VisualizationType.Table,
            ConfidenceScore = 0.3,
            OptimizationSuggestions = new List<string> { "Unable to analyze query due to error" }
        };
    }

    #endregion

    #region Missing Interface Method Implementations

    /// <summary>
    /// Classify async (IQueryClassifier interface)
    /// </summary>
    public async Task<QueryClassificationResult> ClassifyAsync(string query, CancellationToken cancellationToken = default)
    {
        try
        {
            var classification = await ClassifyQueryAsync(query);

            return new QueryClassificationResult
            {
                QueryType = classification.Category.ToString(),
                Intent = classification.Category.ToString(),
                Confidence = classification.ConfidenceScore,
                Categories = new List<string> { classification.Category.ToString() },
                Scores = new Dictionary<string, double>
                {
                    [classification.Category.ToString()] = classification.ConfidenceScore
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error classifying query");
            return new QueryClassificationResult
            {
                QueryType = "Unknown",
                Intent = "General",
                Confidence = 0.5,
                Categories = new List<string> { "Unknown" },
                Scores = new Dictionary<string, double> { ["Unknown"] = 0.5 }
            };
        }
    }

    /// <summary>
    /// Calculate similarity async (ISemanticAnalyzer interface)
    /// </summary>
    public async Task<double> CalculateSimilarityAsync(string query1, string query2, CancellationToken cancellationToken = default)
    {
        var result = await CalculateSimilarityAsync(query1, query2);
        return result.SimilarityScore;
    }

    /// <summary>
    /// Extract keywords async (ISemanticAnalyzer interface)
    /// </summary>
    public async Task<List<string>> ExtractKeywordsAsync(string text, int maxKeywords = 10, CancellationToken cancellationToken = default)
    {
        var analysis = await AnalyzeAsync(text, cancellationToken);
        return analysis.Keywords.Take(maxKeywords).ToList();
    }

    /// <summary>
    /// Analyze sentiment async (ISemanticAnalyzer interface)
    /// </summary>
    public async Task<SentimentAnalysis> AnalyzeSentimentAsync(string text, CancellationToken cancellationToken = default)
    {
        try
        {
            // Simple sentiment analysis based on keywords
            var positiveWords = new[] { "good", "great", "excellent", "positive", "success", "high", "best", "increase", "improve" };
            var negativeWords = new[] { "bad", "poor", "terrible", "negative", "failure", "low", "worst", "decrease", "decline" };

            var words = text.ToLowerInvariant().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var positiveCount = words.Count(w => positiveWords.Contains(w));
            var negativeCount = words.Count(w => negativeWords.Contains(w));
            var totalCount = positiveCount + negativeCount;

            if (totalCount == 0)
            {
                return new SentimentAnalysis
                {
                    Sentiment = "Neutral",
                    Confidence = 0.5,
                    PositiveScore = 0.5,
                    NegativeScore = 0.5
                };
            }

            var positiveRatio = (double)positiveCount / totalCount;
            var sentiment = positiveRatio > 0.6 ? "Positive" : positiveRatio < 0.4 ? "Negative" : "Neutral";

            return new SentimentAnalysis
            {
                Sentiment = sentiment,
                Confidence = Math.Abs(positiveRatio - 0.5) * 2,
                PositiveScore = positiveRatio,
                NegativeScore = 1 - positiveRatio
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing sentiment for text: {Text}", text);
            return new SentimentAnalysis
            {
                Sentiment = "Neutral",
                Confidence = 0.0,
                PositiveScore = 0.5,
                NegativeScore = 0.5
            };
        }
    }

    /// <summary>
    /// Classify text async (ISemanticAnalyzer interface)
    /// </summary>
    public Task<TextClassification> ClassifyTextAsync(string text, List<string> categories, CancellationToken cancellationToken = default)
    {
        try
        {
            var lowerText = text.ToLowerInvariant();
            var scores = new List<ClassificationScore>();

            foreach (var category in categories)
            {
                var score = CalculateCategoryScore(lowerText, category.ToLowerInvariant());
                scores.Add(new ClassificationScore
                {
                    Category = category,
                    Score = score
                });
            }

            var bestScore = scores.OrderByDescending(s => s.Score).First();

            return Task.FromResult(new TextClassification
            {
                Category = bestScore.Category,
                Confidence = bestScore.Score,
                AllScores = scores
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error classifying text: {Text}", text);
            return Task.FromResult(new TextClassification
            {
                Category = categories.FirstOrDefault() ?? "Unknown",
                Confidence = 0.0,
                AllScores = categories.Select(c => new ClassificationScore { Category = c, Score = 0.0 }).ToList()
            });
        }
    }

    /// <summary>
    /// Build context async (ISemanticAnalyzer interface)
    /// </summary>
    public async Task<SemanticContext> BuildContextAsync(string text, SchemaMetadata? schema = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var analysis = await AnalyzeAsync(text, cancellationToken);
            var relatedTables = new List<string>();
            var relatedColumns = new List<string>();

            if (schema != null)
            {
                // Find related tables and columns based on text content
                var lowerText = text.ToLowerInvariant();
                relatedTables = schema.Tables
                    .Where(t => lowerText.Contains(t.Name.ToLowerInvariant()))
                    .Select(t => t.Name)
                    .ToList();

                relatedColumns = schema.Tables
                    .SelectMany(t => t.Columns)
                    .Where(c => lowerText.Contains(c.Name.ToLowerInvariant()))
                    .Select(c => c.Name)
                    .ToList();
            }

            return new SemanticContext
            {
                Text = text,
                RelatedTables = relatedTables,
                RelatedColumns = relatedColumns,
                Keywords = analysis.Keywords,
                Entities = analysis.Entities,
                Metadata = new Dictionary<string, object>
                {
                    ["confidence"] = analysis.ConfidenceScore,
                    ["timestamp"] = DateTime.UtcNow
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error building semantic context for text: {Text}", text);
            return new SemanticContext
            {
                Text = text,
                RelatedTables = new List<string>(),
                RelatedColumns = new List<string>(),
                Keywords = new List<string>(),
                Entities = new List<EntityExtraction>(),
                Metadata = new Dictionary<string, object>()
            };
        }
    }

    private double CalculateCategoryScore(string text, string category)
    {
        // Simple keyword-based scoring
        var categoryKeywords = _categoryKeywords.ContainsKey(Enum.Parse<QueryCategory>(category, true))
            ? _categoryKeywords[Enum.Parse<QueryCategory>(category, true)]
            : new List<string> { category };

        var words = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var matches = words.Count(w => categoryKeywords.Any(k => w.Contains(k)));

        return matches > 0 ? (double)matches / words.Length : 0.0;
    }

    /// <summary>
    /// Generate embedding async (ISemanticAnalyzer interface)
    /// </summary>
    public async Task<float[]> GenerateEmbeddingAsync(string text, CancellationToken cancellationToken = default)
    {
        return await GenerateEmbeddingAsync(text);
    }



    /// <summary>
    /// Analyze with context async (ISemanticAnalyzer interface)
    /// </summary>
    public async Task<SemanticAnalysis> AnalyzeWithContextAsync(string query, string? userId = null, string? sessionId = null, CancellationToken cancellationToken = default)
    {
        // Use existing analysis method and convert to SemanticAnalysis
        var result = await AnalyzeAsync(query, cancellationToken);
        return new SemanticAnalysis
        {
            OriginalQuery = query,
            Intent = Enum.TryParse<QueryIntent>(result.Intent, true, out var intent) ? intent : QueryIntent.General,
            ConfidenceScore = result.ConfidenceScore,
            Entities = result.Entities.ToList(),
            Keywords = result.Keywords.ToList(),
            Metadata = new Dictionary<string, object>
            {
                ["user_id"] = userId ?? "",                ["session_id"] = sessionId ?? "",
                ["has_context"] = !string.IsNullOrEmpty(userId)
            }
        };
    }

    /// <summary>
    /// Convert SemanticAnalysisResult to SemanticAnalysis for compatibility
    /// </summary>
    private static SemanticAnalysis ConvertToSemanticAnalysis(SemanticAnalysisResult result)
    {
        return new SemanticAnalysis
        {
            OriginalQuery = result.Text,
            Intent = Enum.TryParse<QueryIntent>(result.Intent, out var intent) ? intent : QueryIntent.Unknown,
            Entities = result.Entities ?? new List<EntityExtraction>(),
            Keywords = result.Keywords ?? new List<string>(),
            ConfidenceScore = result.ConfidenceScore,
            Confidence = result.ConfidenceScore,
            ProcessedQuery = result.Text,
            Metadata = result.Metadata ?? new Dictionary<string, object>()
        };
    }

    #endregion
}
