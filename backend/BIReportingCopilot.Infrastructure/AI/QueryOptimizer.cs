using Microsoft.Extensions.Logging;
using BIReportingCopilot.Core.Interfaces;
using BIReportingCopilot.Core.Models;
using System.Text.RegularExpressions;

namespace BIReportingCopilot.Infrastructure.AI;

public class QueryOptimizer : IQueryOptimizer
{
    private readonly IAIService _aiService;
    private readonly ISqlQueryService _sqlQueryService;
    private readonly ILogger<QueryOptimizer> _logger;
    private readonly ICacheService _cacheService;

    public QueryOptimizer(
        IAIService aiService,
        ISqlQueryService sqlQueryService,
        ILogger<QueryOptimizer> logger,
        ICacheService cacheService)
    {
        _aiService = aiService;
        _sqlQueryService = sqlQueryService;
        _logger = logger;
        _cacheService = cacheService;
    }

    public async Task<OptimizedQuery> OptimizeAsync(List<SqlCandidate> candidates, UserContext context)
    {
        try
        {
            _logger.LogDebug("Optimizing {CandidateCount} SQL candidates", candidates.Count);

            if (!candidates.Any())
            {
                throw new ArgumentException("No SQL candidates provided for optimization");
            }

            // Score and rank candidates
            var scoredCandidates = new List<SqlCandidate>();

            foreach (var candidate in candidates)
            {
                var enhancedCandidate = await EnhanceCandidateAsync(candidate, context);
                scoredCandidates.Add(enhancedCandidate);
            }

            // Select the best candidate
            var bestCandidate = scoredCandidates.OrderByDescending(c => c.Score).First();

            // Apply additional optimizations
            var optimizedSql = await ApplyOptimizationsAsync(bestCandidate.Sql, context);

            // Generate performance prediction
            var performancePrediction = await PredictPerformanceAsync(optimizedSql, null);

            var optimizedQuery = new OptimizedQuery
            {
                Sql = optimizedSql,
                Explanation = GenerateExplanation(bestCandidate, optimizedSql),
                ConfidenceScore = bestCandidate.Score,
                Alternatives = scoredCandidates.Where(c => c != bestCandidate).Take(3).ToList(),
                OptimizationApplied = GetAppliedOptimizations(bestCandidate.Sql, optimizedSql),
                PerformancePrediction = performancePrediction
            };

            _logger.LogDebug("Query optimization completed with confidence: {Confidence}", optimizedQuery.ConfidenceScore);
            return optimizedQuery;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during query optimization");
            return CreateFallbackOptimizedQuery(candidates.FirstOrDefault());
        }
    }

    public async Task<List<SqlCandidate>> GenerateCandidatesAsync(SemanticAnalysis analysis, SchemaContext schema)
    {
        try
        {
            var candidates = new List<SqlCandidate>();

            // Generate primary candidate using enhanced prompt
            var enhancedPrompt = BuildEnhancedPrompt(analysis, schema);
            var primarySql = await _aiService.GenerateSQLAsync(enhancedPrompt);

            candidates.Add(new SqlCandidate
            {
                Sql = primarySql,
                Score = 0.8, // High initial score for primary candidate
                Reasoning = "Primary candidate generated with full context",
                Strengths = new List<string> { "Uses semantic analysis", "Schema-aware" },
                Weaknesses = new List<string>()
            });

            // Generate alternative candidates with different approaches
            if (analysis.Intent == Core.Models.QueryIntent.Aggregation)
            {
                var aggregationSql = await GenerateAggregationCandidate(analysis, schema);
                if (!string.IsNullOrEmpty(aggregationSql) && aggregationSql != primarySql)
                {
                    candidates.Add(new SqlCandidate
                    {
                        Sql = aggregationSql,
                        Score = 0.7,
                        Reasoning = "Optimized for aggregation operations",
                        Strengths = new List<string> { "Efficient aggregation", "Proper grouping" },
                        Weaknesses = new List<string> { "May be less flexible" }
                    });
                }
            }

            if (analysis.Intent == Core.Models.QueryIntent.Trend)
            {
                var trendSql = await GenerateTrendCandidate(analysis, schema);
                if (!string.IsNullOrEmpty(trendSql) && trendSql != primarySql)
                {
                    candidates.Add(new SqlCandidate
                    {
                        Sql = trendSql,
                        Score = 0.7,
                        Reasoning = "Optimized for time-series analysis",
                        Strengths = new List<string> { "Time-based ordering", "Trend analysis" },
                        Weaknesses = new List<string> { "May require date filtering" }
                    });
                }
            }

            // Generate simplified candidate for complex queries
            if (analysis.Entities.Count > 5)
            {
                var simplifiedSql = await GenerateSimplifiedCandidate(analysis, schema);
                if (!string.IsNullOrEmpty(simplifiedSql))
                {
                    candidates.Add(new SqlCandidate
                    {
                        Sql = simplifiedSql,
                        Score = 0.6,
                        Reasoning = "Simplified version for better performance",
                        Strengths = new List<string> { "Better performance", "Simpler structure" },
                        Weaknesses = new List<string> { "May be less comprehensive" }
                    });
                }
            }

            _logger.LogDebug("Generated {CandidateCount} SQL candidates", candidates.Count);
            return candidates;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating SQL candidates");

            // Return fallback candidate
            var fallbackSql = await _aiService.GenerateSQLAsync(analysis.OriginalQuery);
            return new List<SqlCandidate>
            {
                new SqlCandidate
                {
                    Sql = fallbackSql,
                    Score = 0.5,
                    Reasoning = "Fallback candidate due to error",
                    Strengths = new List<string> { "Basic functionality" },
                    Weaknesses = new List<string> { "Limited optimization" }
                }
            };
        }
    }

    public async Task<QueryPerformancePrediction> PredictPerformanceAsync(string sql, SchemaMetadata? schema)
    {
        try
        {
            var prediction = new QueryPerformancePrediction();

            // Analyze query complexity
            var complexity = AnalyzeQueryComplexity(sql);
            prediction.ComplexityScore = complexity;

            // Estimate execution time based on complexity
            prediction.EstimatedExecutionTime = EstimateExecutionTime(complexity, sql);

            // Estimate row count
            prediction.EstimatedRowCount = EstimateRowCount(sql);

            // Generate performance warnings
            prediction.PerformanceWarnings = GeneratePerformanceWarnings(sql);

            // Generate index recommendations
            prediction.IndexRecommendations = GenerateIndexRecommendations(sql, schema);

            return prediction;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error predicting query performance");
            return new QueryPerformancePrediction
            {
                EstimatedExecutionTime = TimeSpan.FromSeconds(1),
                EstimatedRowCount = 1000,
                ComplexityScore = 0.5,
                PerformanceWarnings = new List<string> { "Unable to analyze performance" },
                IndexRecommendations = new List<string>()
            };
        }
    }

    public async Task<string> RewriteForPerformanceAsync(string sql)
    {
        try
        {
            var optimizations = new List<string>();
            var optimizedSql = sql;

            // Remove unnecessary SELECT *
            if (sql.Contains("SELECT *", StringComparison.OrdinalIgnoreCase))
            {
                // This would require more sophisticated parsing in a real implementation
                optimizations.Add("Consider selecting specific columns instead of *");
            }

            // Add LIMIT if missing for large result sets
            if (!sql.Contains("LIMIT", StringComparison.OrdinalIgnoreCase) &&
                !sql.Contains("TOP", StringComparison.OrdinalIgnoreCase))
            {
                if (sql.Contains("ORDER BY", StringComparison.OrdinalIgnoreCase))
                {
                    optimizedSql += " LIMIT 1000";
                    optimizations.Add("Added LIMIT clause for performance");
                }
            }

            // Optimize JOIN order (simplified)
            if (CountJoins(sql) > 2)
            {
                optimizations.Add("Consider optimizing JOIN order for better performance");
            }

            _logger.LogDebug("Applied {OptimizationCount} performance optimizations", optimizations.Count);
            return optimizedSql;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rewriting SQL for performance");
            return sql; // Return original SQL if optimization fails
        }
    }

    private async Task<SqlCandidate> EnhanceCandidateAsync(SqlCandidate candidate, UserContext context)
    {
        try
        {
            // Calculate confidence score based on various factors
            var confidence = await _aiService.CalculateConfidenceScoreAsync("", candidate.Sql);

            // Adjust score based on user context
            if (context.PreferredTables.Any(table => candidate.Sql.Contains(table, StringComparison.OrdinalIgnoreCase)))
            {
                confidence += 0.1; // Boost for using user's preferred tables
            }

            // Analyze SQL quality
            var qualityScore = AnalyzeSqlQuality(candidate.Sql);
            confidence = (confidence + qualityScore) / 2;

            candidate.Score = confidence;

            // Update strengths and weaknesses based on analysis
            UpdateCandidateAnalysis(candidate);

            return candidate;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error enhancing SQL candidate");
            return candidate;
        }
    }

    private string BuildEnhancedPrompt(SemanticAnalysis analysis, SchemaContext schema)
    {
        var prompt = $"Natural language query: {analysis.OriginalQuery}\n\n";

        prompt += "Semantic Analysis:\n";
        prompt += $"- Intent: {analysis.Intent}\n";
        prompt += $"- Entities: {string.Join(", ", analysis.Entities.Select(e => $"{e.Text} ({e.Type})"))}\n";
        prompt += $"- Keywords: {string.Join(", ", analysis.Keywords)}\n\n";

        prompt += "Available Tables:\n";
        foreach (var table in schema.RelevantTables)
        {
            prompt += $"- {table.Name}: {string.Join(", ", table.Columns.Select(c => c.Name))}\n";
        }

        if (schema.SuggestedJoins.Any())
        {
            prompt += "\nSuggested Joins:\n";
            foreach (var join in schema.SuggestedJoins)
            {
                prompt += $"- {join}\n";
            }
        }

        prompt += "\nGenerate an optimized SQL query that addresses the user's request.";

        return prompt;
    }

    private async Task<string> GenerateAggregationCandidate(SemanticAnalysis analysis, SchemaContext schema)
    {
        var aggregationPrompt = $"Generate a SQL query optimized for aggregation operations: {analysis.OriginalQuery}\n";
        aggregationPrompt += "Focus on efficient GROUP BY, aggregate functions, and proper indexing considerations.";

        return await _aiService.GenerateSQLAsync(aggregationPrompt);
    }

    private async Task<string> GenerateTrendCandidate(SemanticAnalysis analysis, SchemaContext schema)
    {
        var trendPrompt = $"Generate a SQL query optimized for time-series trend analysis: {analysis.OriginalQuery}\n";
        trendPrompt += "Focus on date-based grouping, time windows, and trend calculations.";

        return await _aiService.GenerateSQLAsync(trendPrompt);
    }

    private async Task<string> GenerateSimplifiedCandidate(SemanticAnalysis analysis, SchemaContext schema)
    {
        var simplifiedPrompt = $"Generate a simplified, high-performance SQL query: {analysis.OriginalQuery}\n";
        simplifiedPrompt += "Focus on essential data only, minimal joins, and fast execution.";

        return await _aiService.GenerateSQLAsync(simplifiedPrompt);
    }

    private async Task<string> ApplyOptimizationsAsync(string sql, UserContext context)
    {
        var optimizedSql = sql;

        // Apply user-specific optimizations
        if (context.CommonFilters.Any(f => f.Contains("last month")) &&
            !sql.Contains("WHERE", StringComparison.OrdinalIgnoreCase))
        {
            // Add common date filter if missing
            optimizedSql = AddDateFilter(optimizedSql);
        }

        // Apply performance optimizations
        optimizedSql = await RewriteForPerformanceAsync(optimizedSql);

        return optimizedSql;
    }

    private string AddDateFilter(string sql)
    {
        // Simplified date filter addition
        if (sql.Contains("FROM", StringComparison.OrdinalIgnoreCase) &&
            !sql.Contains("WHERE", StringComparison.OrdinalIgnoreCase))
        {
            var fromIndex = sql.LastIndexOf("FROM", StringComparison.OrdinalIgnoreCase);
            var tableEndIndex = sql.IndexOf(' ', fromIndex + 5);
            if (tableEndIndex == -1) tableEndIndex = sql.Length;

            var beforeTable = sql.Substring(0, tableEndIndex);
            var afterTable = sql.Substring(tableEndIndex);

            return beforeTable + " WHERE CreatedDate >= DATEADD(month, -1, GETDATE())" + afterTable;
        }

        return sql;
    }

    private double AnalyzeQueryComplexity(string sql)
    {
        var complexity = 0.0;
        var lowerSql = sql.ToLowerInvariant();

        // Count complexity factors
        complexity += CountJoins(sql) * 0.2;
        complexity += CountSubqueries(sql) * 0.3;
        complexity += CountAggregations(sql) * 0.1;
        complexity += CountConditions(sql) * 0.05;

        if (lowerSql.Contains("union")) complexity += 0.2;
        if (lowerSql.Contains("having")) complexity += 0.1;
        if (lowerSql.Contains("order by")) complexity += 0.05;

        return Math.Min(1.0, complexity);
    }

    private TimeSpan EstimateExecutionTime(double complexity, string sql)
    {
        var baseTime = 100; // milliseconds
        var multiplier = 1 + (complexity * 5); // Scale based on complexity

        // Additional factors
        if (sql.Contains("ORDER BY", StringComparison.OrdinalIgnoreCase)) multiplier *= 1.2;
        if (CountJoins(sql) > 3) multiplier *= 1.5;

        return TimeSpan.FromMilliseconds(baseTime * multiplier);
    }

    private int EstimateRowCount(string sql)
    {
        var baseCount = 1000;

        // Adjust based on query characteristics
        if (sql.Contains("WHERE", StringComparison.OrdinalIgnoreCase)) baseCount /= 2;
        if (sql.Contains("GROUP BY", StringComparison.OrdinalIgnoreCase)) baseCount /= 5;
        if (sql.Contains("LIMIT", StringComparison.OrdinalIgnoreCase) || sql.Contains("TOP", StringComparison.OrdinalIgnoreCase))
        {
            var limitMatch = Regex.Match(sql, @"LIMIT\s+(\d+)", RegexOptions.IgnoreCase);
            var topMatch = Regex.Match(sql, @"TOP\s+(\d+)", RegexOptions.IgnoreCase);

            if (limitMatch.Success && int.TryParse(limitMatch.Groups[1].Value, out var limitValue))
                return Math.Min(baseCount, limitValue);
            if (topMatch.Success && int.TryParse(topMatch.Groups[1].Value, out var topValue))
                return Math.Min(baseCount, topValue);
        }

        return baseCount;
    }

    private List<string> GeneratePerformanceWarnings(string sql)
    {
        var warnings = new List<string>();
        var lowerSql = sql.ToLowerInvariant();

        if (lowerSql.Contains("select *"))
            warnings.Add("Using SELECT * may impact performance - consider selecting specific columns");

        if (!lowerSql.Contains("where") && !lowerSql.Contains("limit"))
            warnings.Add("Query may return large result set - consider adding WHERE clause or LIMIT");

        if (CountJoins(sql) > 3)
            warnings.Add("Multiple joins detected - ensure proper indexing on join columns");

        if (lowerSql.Contains("order by") && !lowerSql.Contains("limit"))
            warnings.Add("ORDER BY without LIMIT may be inefficient for large datasets");

        if (CountSubqueries(sql) > 1)
            warnings.Add("Multiple subqueries detected - consider using JOINs for better performance");

        return warnings;
    }

    private List<string> GenerateIndexRecommendations(string sql, SchemaMetadata? schema)
    {
        var recommendations = new List<string>();

        // Extract table and column references for index recommendations
        var joinColumns = ExtractJoinColumns(sql);
        var whereColumns = ExtractWhereColumns(sql);
        var orderColumns = ExtractOrderByColumns(sql);

        foreach (var column in joinColumns)
            recommendations.Add($"Consider adding index on {column} for join performance");

        foreach (var column in whereColumns)
            recommendations.Add($"Consider adding index on {column} for filter performance");

        foreach (var column in orderColumns)
            recommendations.Add($"Consider adding index on {column} for sorting performance");

        return recommendations.Distinct().ToList();
    }

    private double AnalyzeSqlQuality(string sql)
    {
        var quality = 0.5; // Base quality

        // Positive factors
        if (sql.Contains("WHERE", StringComparison.OrdinalIgnoreCase)) quality += 0.1;
        if (sql.Contains("ORDER BY", StringComparison.OrdinalIgnoreCase)) quality += 0.05;
        if (!sql.Contains("SELECT *", StringComparison.OrdinalIgnoreCase)) quality += 0.1;

        // Negative factors
        if (CountSubqueries(sql) > 2) quality -= 0.1;
        if (CountJoins(sql) > 4) quality -= 0.1;

        return Math.Max(0.1, Math.Min(1.0, quality));
    }

    private void UpdateCandidateAnalysis(SqlCandidate candidate)
    {
        var sql = candidate.Sql;

        // Update strengths
        if (sql.Contains("WHERE", StringComparison.OrdinalIgnoreCase))
            candidate.Strengths.Add("Includes filtering");
        if (sql.Contains("ORDER BY", StringComparison.OrdinalIgnoreCase))
            candidate.Strengths.Add("Results are ordered");
        if (!sql.Contains("SELECT *", StringComparison.OrdinalIgnoreCase))
            candidate.Strengths.Add("Specific column selection");

        // Update weaknesses
        if (CountJoins(sql) > 3)
            candidate.Weaknesses.Add("Complex joins may impact performance");
        if (!sql.Contains("WHERE", StringComparison.OrdinalIgnoreCase))
            candidate.Weaknesses.Add("No filtering - may return large dataset");
    }

    private string GenerateExplanation(SqlCandidate bestCandidate, string optimizedSql)
    {
        var explanation = $"Selected query based on: {bestCandidate.Reasoning}\n";
        explanation += $"Confidence Score: {bestCandidate.Score:F2}\n";

        if (bestCandidate.Strengths.Any())
            explanation += $"Strengths: {string.Join(", ", bestCandidate.Strengths)}\n";

        if (optimizedSql != bestCandidate.Sql)
            explanation += "Additional optimizations were applied for better performance.";

        return explanation;
    }

    private List<string> GetAppliedOptimizations(string originalSql, string optimizedSql)
    {
        var optimizations = new List<string>();

        if (optimizedSql.Length > originalSql.Length)
        {
            if (optimizedSql.Contains("LIMIT") && !originalSql.Contains("LIMIT"))
                optimizations.Add("Added LIMIT clause");
            if (optimizedSql.Contains("WHERE") && !originalSql.Contains("WHERE"))
                optimizations.Add("Added WHERE clause");
        }

        return optimizations;
    }

    private OptimizedQuery CreateFallbackOptimizedQuery(SqlCandidate? candidate)
    {
        return new OptimizedQuery
        {
            Sql = candidate?.Sql ?? "SELECT 'Error optimizing query' as Message",
            Explanation = "Fallback optimization due to error",
            ConfidenceScore = 0.3,
            Alternatives = new List<SqlCandidate>(),
            OptimizationApplied = new List<string>(),
            PerformancePrediction = new QueryPerformancePrediction
            {
                EstimatedExecutionTime = TimeSpan.FromSeconds(1),
                EstimatedRowCount = 1,
                ComplexityScore = 0.5
            }
        };
    }

    // Helper methods for SQL analysis
    private int CountJoins(string sql) => Regex.Matches(sql, @"\bJOIN\b", RegexOptions.IgnoreCase).Count;
    private int CountSubqueries(string sql) => Regex.Matches(sql, @"\(.*SELECT.*\)", RegexOptions.IgnoreCase).Count;
    private int CountAggregations(string sql) => Regex.Matches(sql, @"\b(SUM|COUNT|AVG|MAX|MIN)\b", RegexOptions.IgnoreCase).Count;
    private int CountConditions(string sql) => Regex.Matches(sql, @"\b(AND|OR)\b", RegexOptions.IgnoreCase).Count + 1;

    private List<string> ExtractJoinColumns(string sql)
    {
        var columns = new List<string>();
        var joinMatches = Regex.Matches(sql, @"JOIN\s+\w+\s+ON\s+(\w+\.\w+)\s*=\s*(\w+\.\w+)", RegexOptions.IgnoreCase);

        foreach (Match match in joinMatches)
        {
            columns.Add(match.Groups[1].Value);
            columns.Add(match.Groups[2].Value);
        }

        return columns;
    }

    private List<string> ExtractWhereColumns(string sql)
    {
        var columns = new List<string>();
        var whereMatches = Regex.Matches(sql, @"WHERE\s+.*?(\w+\.\w+|\w+)\s*[=<>]", RegexOptions.IgnoreCase);

        foreach (Match match in whereMatches)
        {
            columns.Add(match.Groups[1].Value);
        }

        return columns;
    }

    private List<string> ExtractOrderByColumns(string sql)
    {
        var columns = new List<string>();
        var orderMatches = Regex.Matches(sql, @"ORDER\s+BY\s+(.*?)(?:\s+LIMIT|\s*$)", RegexOptions.IgnoreCase);

        foreach (Match match in orderMatches)
        {
            var orderColumns = match.Groups[1].Value.Split(',');
            foreach (var col in orderColumns)
            {
                var cleanColumn = col.Trim().Split(' ')[0]; // Remove ASC/DESC
                columns.Add(cleanColumn);
            }
        }

        return columns;
    }
}
