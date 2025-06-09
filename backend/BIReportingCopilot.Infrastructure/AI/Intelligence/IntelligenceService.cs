using Microsoft.Extensions.Logging;
using BIReportingCopilot.Core.Interfaces;
using BIReportingCopilot.Core.Models;
using BIReportingCopilot.Core.Models.ML;
using System.Text.Json;

namespace BIReportingCopilot.Infrastructure.AI.Intelligence;

/// <summary>
/// Query intelligence service providing advanced analytics and insights
/// </summary>
public class IntelligenceService : IQueryIntelligenceService
{
    private readonly ILogger<IntelligenceService> _logger;
    private readonly ICacheService _cacheService;
    private readonly ISchemaService _schemaService;
    private readonly IAIService _aiService;

    public IntelligenceService(
        ILogger<IntelligenceService> logger,
        ICacheService cacheService,
        ISchemaService schemaService,
        IAIService aiService)
    {
        _logger = logger;
        _cacheService = cacheService;
        _schemaService = schemaService;
        _aiService = aiService;
    }

    public async Task<QueryIntelligenceResult> AnalyzeQueryIntelligenceAsync(string query, QueryExecutionMetrics metrics)
    {
        try
        {
            _logger.LogDebug("Analyzing query intelligence for: {Query}", query);

            var result = new QueryIntelligenceResult
            {
                Query = query,
                AnalyzedAt = DateTime.UtcNow,
                IntelligenceScore = 0.0,
                Insights = new List<QueryInsight>(),
                Recommendations = new List<QueryRecommendation>(),
                PerformanceAnalysis = new QueryPerformanceAnalysis(),
                SemanticAnalysis = new QuerySemanticAnalysis()
            };

            // Analyze query performance
            result.PerformanceAnalysis = await AnalyzePerformanceAsync(query, metrics);

            // Analyze semantic patterns
            result.SemanticAnalysis = await AnalyzeSemanticPatternsAsync(query);

            // Generate insights
            result.Insights = await GenerateQueryInsightsAsync(query, metrics);

            // Generate recommendations
            result.Recommendations = await GenerateRecommendationsAsync(query, metrics);

            // Calculate overall intelligence score
            result.IntelligenceScore = CalculateIntelligenceScore(result);

            _logger.LogDebug("Query intelligence analysis completed with score: {Score}", result.IntelligenceScore);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing query intelligence");
            return new QueryIntelligenceResult
            {
                Query = query,
                AnalyzedAt = DateTime.UtcNow,
                IntelligenceScore = 0.0,
                Insights = new List<QueryInsight>(),
                Recommendations = new List<QueryRecommendation>(),
                PerformanceAnalysis = new QueryPerformanceAnalysis(),
                SemanticAnalysis = new QuerySemanticAnalysis()
            };
        }
    }

    public async Task<List<QueryPattern>> IdentifyQueryPatternsAsync(List<string> queries, string userId)
    {
        try
        {
            _logger.LogDebug("Identifying query patterns for {QueryCount} queries", queries.Count);

            var patterns = new List<QueryPattern>();

            // Group similar queries
            var queryGroups = await GroupSimilarQueriesAsync(queries);

            foreach (var group in queryGroups)
            {
                var pattern = new QueryPattern
                {
                    PatternId = Guid.NewGuid().ToString(),
                    Pattern = ExtractCommonPattern(group),
                    Frequency = group.Count,
                    Examples = group.Take(3).ToList(),
                    Confidence = CalculatePatternConfidence(group),
                    Category = ClassifyPatternCategory(group),
                    UserId = userId,
                    IdentifiedAt = DateTime.UtcNow
                };

                patterns.Add(pattern);
            }

            // Sort by frequency and confidence
            patterns = patterns.OrderByDescending(p => p.Frequency)
                             .ThenByDescending(p => p.Confidence)
                             .ToList();

            _logger.LogDebug("Identified {PatternCount} query patterns", patterns.Count);

            return patterns;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error identifying query patterns");
            return new List<QueryPattern>();
        }
    }

    public async Task<QueryOptimizationSuggestion> SuggestQueryOptimizationAsync(string query, QueryExecutionMetrics metrics)
    {
        try
        {
            _logger.LogDebug("Generating optimization suggestions for query");

            var suggestion = new QueryOptimizationSuggestion
            {
                OriginalQuery = query,
                Suggestions = new List<OptimizationSuggestion>(),
                EstimatedImprovement = 0.0,
                Confidence = 0.0,
                GeneratedAt = DateTime.UtcNow
            };

            // Analyze execution time
            if (metrics.ExecutionTimeMs > 5000) // Slow query
            {
                suggestion.Suggestions.Add(new OptimizationSuggestion
                {
                    Type = "Performance",
                    Description = "Query execution time is high. Consider adding indexes or optimizing joins.",
                    Impact = "High",
                    Effort = "Medium"
                });
            }

            // Analyze result size
            if (metrics.RowCount > 10000) // Large result set
            {
                suggestion.Suggestions.Add(new OptimizationSuggestion
                {
                    Type = "ResultSize",
                    Description = "Large result set detected. Consider adding filters or pagination.",
                    Impact = "Medium",
                    Effort = "Low"
                });
            }

            // Analyze query complexity
            var complexityScore = CalculateQueryComplexity(query);
            if (complexityScore > 0.8)
            {
                suggestion.Suggestions.Add(new OptimizationSuggestion
                {
                    Type = "Complexity",
                    Description = "Complex query detected. Consider breaking into smaller queries.",
                    Impact = "Medium",
                    Effort = "High"
                });
            }

            // Calculate overall confidence and improvement estimate
            suggestion.Confidence = suggestion.Suggestions.Any() ? 0.8 : 0.3;
            suggestion.EstimatedImprovement = suggestion.Suggestions.Count * 0.2;

            return suggestion;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating optimization suggestions");
            return new QueryOptimizationSuggestion
            {
                OriginalQuery = query,
                Suggestions = new List<OptimizationSuggestion>(),
                EstimatedImprovement = 0.0,
                Confidence = 0.0,
                GeneratedAt = DateTime.UtcNow
            };
        }
    }

    public async Task<QueryTrendAnalysis> AnalyzeQueryTrendsAsync(string userId, TimeSpan timeWindow)
    {
        try
        {
            _logger.LogDebug("Analyzing query trends for user {UserId} over {TimeWindow}", userId, timeWindow);

            var endTime = DateTime.UtcNow;
            var startTime = endTime.Subtract(timeWindow);

            var analysis = new QueryTrendAnalysis
            {
                UserId = userId,
                TimeWindow = timeWindow,
                AnalyzedAt = DateTime.UtcNow,
                TrendMetrics = new Dictionary<string, double>(),
                PopularQueries = new List<string>(),
                EmergingPatterns = new List<string>(),
                PerformanceTrends = new Dictionary<string, double>()
            };

            // Get user's query history (simplified - would use actual data)
            var queryHistory = await GetUserQueryHistoryAsync(userId, startTime, endTime);

            // Analyze query frequency trends
            analysis.TrendMetrics["daily_query_count"] = queryHistory.Count / Math.Max(1, timeWindow.Days);
            analysis.TrendMetrics["unique_query_ratio"] = queryHistory.Distinct().Count() / (double)Math.Max(1, queryHistory.Count);

            // Identify popular queries
            analysis.PopularQueries = queryHistory
                .GroupBy(q => q)
                .OrderByDescending(g => g.Count())
                .Take(5)
                .Select(g => g.Key)
                .ToList();

            // Identify emerging patterns (simplified)
            analysis.EmergingPatterns = IdentifyEmergingPatterns(queryHistory);

            // Performance trends (simplified)
            analysis.PerformanceTrends["average_response_time"] = 1500; // Would calculate from actual metrics

            _logger.LogDebug("Query trend analysis completed for user {UserId}", userId);

            return analysis;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing query trends for user {UserId}", userId);
            return new QueryTrendAnalysis
            {
                UserId = userId,
                TimeWindow = timeWindow,
                AnalyzedAt = DateTime.UtcNow,
                TrendMetrics = new Dictionary<string, double>(),
                PopularQueries = new List<string>(),
                EmergingPatterns = new List<string>(),
                PerformanceTrends = new Dictionary<string, double>()
            };
        }
    }

    private double CalculateQueryComplexity(string query)
    {
        var complexity = 0.0;
        var lowerQuery = query.ToLowerInvariant();
        complexity += (lowerQuery.Split("join").Length - 1) * 0.2;
        complexity += (lowerQuery.Split("select").Length - 1) * 0.15;
        var aggregations = new[] { "sum", "count", "avg", "max", "min" };
        complexity += aggregations.Count(a => lowerQuery.Contains(a)) * 0.1;
        return Math.Min(1.0, complexity);
    }

    private double CalculatePerformanceScore(QueryExecutionMetrics metrics)
    {
        var score = 1.0;
        if (metrics.ExecutionTimeMs > 1000) score -= 0.3;
        if (metrics.ExecutionTimeMs > 5000) score -= 0.4;
        if (metrics.RowCount > 1000) score -= 0.1;
        if (metrics.RowCount > 10000) score -= 0.2;
        return Math.Max(0.0, score);
    }

    private List<string> IdentifyBottlenecks(QueryExecutionMetrics metrics)
    {
        var bottlenecks = new List<string>();
        if (metrics.ExecutionTimeMs > 3000) bottlenecks.Add("Slow execution time");
        if (metrics.RowCount > 10000) bottlenecks.Add("Large result set");
        return bottlenecks;
    }
}

    #region Private Helper Methods

    private double CalculateQueryComplexity(string query)
    {
        var complexity = 0.0;
        var lowerQuery = query.ToLowerInvariant();

        // Add complexity for joins
        complexity += (lowerQuery.Split("join").Length - 1) * 0.2;

        // Add complexity for subqueries
        complexity += (lowerQuery.Split("select").Length - 1) * 0.15;

        // Add complexity for aggregations
        var aggregations = new[] { "sum", "count", "avg", "max", "min" };
        complexity += aggregations.Count(a => lowerQuery.Contains(a)) * 0.1;

        return Math.Min(1.0, complexity);
    }

    private double CalculatePerformanceScore(QueryExecutionMetrics metrics)
    {
        var score = 1.0;

        // Penalize slow execution
        if (metrics.ExecutionTimeMs > 1000)
            score -= 0.3;
        if (metrics.ExecutionTimeMs > 5000)
            score -= 0.4;

        // Penalize large result sets
        if (metrics.RowCount > 1000)
            score -= 0.1;
        if (metrics.RowCount > 10000)
            score -= 0.2;

        return Math.Max(0.0, score);
    }

    private List<string> IdentifyBottlenecks(QueryExecutionMetrics metrics)
    {
        var bottlenecks = new List<string>();

        if (metrics.ExecutionTimeMs > 3000)
            bottlenecks.Add("Slow execution time");

        if (metrics.RowCount > 10000)
            bottlenecks.Add("Large result set");

        return bottlenecks;
    }

    private List<string> ExtractEntities(string query)
    {
        // Simplified entity extraction
        var entities = new List<string>();
        var words = query.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        foreach (var word in words)
        {
            if (char.IsUpper(word[0]) && word.Length > 2)
                entities.Add(word);
        }

        return entities.Distinct().ToList();
    }

    private string ClassifyIntent(string query)
    {
        var lowerQuery = query.ToLowerInvariant();

        if (lowerQuery.Contains("count") || lowerQuery.Contains("sum"))
            return "Aggregation";
        if (lowerQuery.Contains("show") || lowerQuery.Contains("list"))
            return "Display";
        if (lowerQuery.Contains("find") || lowerQuery.Contains("search"))
            return "Search";

        return "General";
    }

    #endregion
}
