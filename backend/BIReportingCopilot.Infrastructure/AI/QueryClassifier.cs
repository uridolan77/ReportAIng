using Microsoft.Extensions.Logging;
using BIReportingCopilot.Core.Interfaces;
using BIReportingCopilot.Core.Models;
using System.Text.RegularExpressions;

namespace BIReportingCopilot.Infrastructure.AI;

public class QueryClassifier : IQueryClassifier
{
    private readonly ILogger<QueryClassifier> _logger;
    private readonly ISemanticAnalyzer _semanticAnalyzer;
    private readonly ICacheService _cacheService;

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

    private readonly Dictionary<QueryComplexity, ComplexityThreshold> _complexityThresholds = new()
    {
        [QueryComplexity.Low] = new() { MaxJoins = 1, MaxAggregations = 2, MaxConditions = 3 },
        [QueryComplexity.Medium] = new() { MaxJoins = 3, MaxAggregations = 5, MaxConditions = 7 },
        [QueryComplexity.High] = new() { MaxJoins = int.MaxValue, MaxAggregations = int.MaxValue, MaxConditions = int.MaxValue }
    };

    public QueryClassifier(
        ILogger<QueryClassifier> logger,
        ISemanticAnalyzer semanticAnalyzer,
        ICacheService cacheService)
    {
        _logger = logger;
        _semanticAnalyzer = semanticAnalyzer;
        _cacheService = cacheService;
    }

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
            }

            // Get semantic analysis
            var semanticAnalysis = await _semanticAnalyzer.AnalyzeAsync(query);

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

    public async Task<QueryComplexityScore> AnalyzeComplexityAsync(string query)
    {
        try
        {
            var semanticAnalysis = await _semanticAnalyzer.AnalyzeAsync(query);
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

    public async Task<List<string>> SuggestOptimizationsAsync(string query)
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

        return suggestions;
    }

    public async Task<bool> RequiresJoinAsync(string query, SchemaMetadata schema)
    {
        var lowerQuery = query.ToLowerInvariant();
        
        // Direct join keywords
        if (lowerQuery.Contains("join") || lowerQuery.Contains("inner") || lowerQuery.Contains("left") || lowerQuery.Contains("right"))
        {
            return true;
        }

        // Multiple table references
        var tableCount = schema.Tables.Count(table => 
            lowerQuery.Contains(table.Name.ToLowerInvariant()));
        
        return tableCount > 1;
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
        return semanticAnalysis.Entities.Count(e => e.Type == EntityType.Aggregation);
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

    private class ComplexityThreshold
    {
        public int MaxJoins { get; set; }
        public int MaxAggregations { get; set; }
        public int MaxConditions { get; set; }
    }
}
