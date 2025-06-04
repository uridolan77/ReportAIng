using Microsoft.Extensions.Logging;
using BIReportingCopilot.Core.Interfaces;
using BIReportingCopilot.Core.Models;
using BIReportingCopilot.Infrastructure.Performance;

namespace BIReportingCopilot.Infrastructure.AI.Enhanced;

// Supporting models and classes for Adaptive Query Optimizer

/// <summary>
/// Query characteristics for optimization analysis
/// </summary>
public class QueryCharacteristics
{
    public string SQL { get; set; } = string.Empty;
    public QueryType QueryType { get; set; }
    public List<string> TablesInvolved { get; set; } = new();
    public int JoinCount { get; set; }
    public int FilterCount { get; set; }
    public int AggregationCount { get; set; }
    public int OrderByCount { get; set; }
    public bool HasSubqueries { get; set; }
    public int EstimatedComplexity { get; set; }
    public QueryIntent SemanticIntent { get; set; }
    public int EntityCount { get; set; }
}

/// <summary>
/// Query performance record for learning
/// </summary>
public class QueryPerformanceRecord
{
    public string SQL { get; set; } = string.Empty;
    public TimeSpan ExecutionTime { get; set; }
    public int RowsReturned { get; set; }
    public bool Success { get; set; }
    public string? UserId { get; set; }
    public DateTime Timestamp { get; set; }
}

/// <summary>
/// Query optimization definition
/// </summary>
public class QueryOptimization
{
    public OptimizationType Type { get; set; }
    public string Description { get; set; } = string.Empty;
    public int Priority { get; set; }
    public double EstimatedImprovement { get; set; }
    public Dictionary<string, object> Parameters { get; set; } = new();
}

/// <summary>
/// Optimization result
/// </summary>
public class OptimizedQueryResult
{
    public string OriginalSQL { get; set; } = string.Empty;
    public string OptimizedSQL { get; set; } = string.Empty;
    public List<QueryOptimization> AppliedOptimizations { get; set; } = new();
    public PerformanceEstimate EstimatedImprovement { get; set; } = new();
    public double Confidence { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Performance estimate
/// </summary>
public class PerformanceEstimate
{
    public double EstimatedImprovementPercentage { get; set; }
    public double EstimatedExecutionTimeMs { get; set; }
    public double Confidence { get; set; }
}

/// <summary>
/// Optimization recommendation
/// </summary>
public class OptimizationRecommendation
{
    public string Type { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public double EstimatedImpact { get; set; }
    public int Priority { get; set; }
}

/// <summary>
/// SQL analysis result
/// </summary>
public class SQLAnalysis
{
    public string SQL { get; set; } = string.Empty;
    public List<string> TablesUsed { get; set; } = new();
    public int JoinCount { get; set; }
    public int FilterConditions { get; set; }
    public int AggregationCount { get; set; }
    public bool HasOrderBy { get; set; }
    public bool HasSubqueries { get; set; }
    public int ComplexityScore { get; set; }
}

/// <summary>
/// Query types
/// </summary>
public enum QueryType
{
    Select,
    Insert,
    Update,
    Delete,
    Unknown
}

/// <summary>
/// Optimization types
/// </summary>
public enum OptimizationType
{
    AddNolockHints,
    OptimizeJoinOrder,
    AddIndexHints,
    RewriteSubquery,
    OptimizeWhereClause,
    AddQueryHints
}

/// <summary>
/// Query performance tracker
/// </summary>
public class QueryPerformanceTracker
{
    private readonly ILogger _logger;
    private readonly ICacheService _cacheService;

    public QueryPerformanceTracker(ILogger logger, ICacheService cacheService)
    {
        _logger = logger;
        _cacheService = cacheService;
    }

    public async Task RecordPerformanceAsync(QueryPerformanceRecord record)
    {
        try
        {
            var key = $"query_performance:{DateTime.UtcNow:yyyyMMddHHmmss}:{Guid.NewGuid():N}";
            await _cacheService.SetAsync(key, record, TimeSpan.FromDays(30));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recording query performance");
        }
    }

    public async Task<List<QueryPerformanceRecord>> GetPerformanceHistoryAsync(QueryCharacteristics characteristics)
    {
        // Simplified implementation - in practice would query by characteristics
        return new List<QueryPerformanceRecord>();
    }
}

/// <summary>
/// Optimization rule engine
/// </summary>
public class OptimizationRuleEngine
{
    private readonly ILogger _logger;

    public OptimizationRuleEngine(ILogger logger)
    {
        _logger = logger;
    }

    public async Task<List<QueryOptimization>> GenerateRuleBasedOptimizationsAsync(
        string sql,
        QueryCharacteristics characteristics)
    {
        var optimizations = new List<QueryOptimization>();

        // Rule 1: Add NOLOCK hints for SELECT queries
        if (characteristics.QueryType == QueryType.Select && !sql.Contains("NOLOCK"))
        {
            optimizations.Add(new QueryOptimization
            {
                Type = OptimizationType.AddNolockHints,
                Description = "Add NOLOCK hints for better read performance",
                Priority = 8,
                EstimatedImprovement = 0.2
            });
        }

        // Rule 2: Optimize complex joins
        if (characteristics.JoinCount > 2)
        {
            optimizations.Add(new QueryOptimization
            {
                Type = OptimizationType.OptimizeJoinOrder,
                Description = "Optimize join order for better performance",
                Priority = 7,
                EstimatedImprovement = 0.15
            });
        }

        // Rule 3: Add query hints for complex queries
        if (characteristics.EstimatedComplexity > 10)
        {
            optimizations.Add(new QueryOptimization
            {
                Type = OptimizationType.AddQueryHints,
                Description = "Add query hints for complex queries",
                Priority = 6,
                EstimatedImprovement = 0.1
            });
        }

        return optimizations;
    }
}

/// <summary>
/// Adaptive query optimizer implementing Enhancement 9: Adaptive Query Optimization
/// Learns from execution patterns and optimizes queries based on historical performance
/// </summary>
public class AdaptiveQueryOptimizer
{
    private readonly ILogger<AdaptiveQueryOptimizer> _logger;
    private readonly ICacheService _cacheService;
    private readonly ISchemaService _schemaService;
    private readonly QueryPerformanceTracker _performanceTracker;
    private readonly OptimizationRuleEngine _ruleEngine;

    public AdaptiveQueryOptimizer(
        ILogger<AdaptiveQueryOptimizer> logger,
        ICacheService cacheService,
        ISchemaService schemaService)
    {
        _logger = logger;
        _cacheService = cacheService;
        _schemaService = schemaService;
        _performanceTracker = new QueryPerformanceTracker(logger, cacheService);
        _ruleEngine = new OptimizationRuleEngine(logger);
    }

    /// <summary>
    /// Optimize query based on learned patterns and performance history
    /// </summary>
    public async Task<OptimizedQueryResult> OptimizeQueryAsync(
        string originalSQL,
        SemanticAnalysis semanticAnalysis,
        SchemaMetadata schema,
        string? userId = null)
    {
        try
        {
            _logger.LogDebug("Starting adaptive query optimization for SQL: {SQL}", originalSQL);

            // Step 1: Analyze query characteristics
            var queryCharacteristics = await AnalyzeQueryCharacteristicsAsync(originalSQL, semanticAnalysis, schema);

            // Step 2: Get historical performance data
            var performanceHistory = await _performanceTracker.GetPerformanceHistoryAsync(queryCharacteristics);

            // Step 3: Apply learned optimizations
            var optimizations = await GenerateOptimizationsAsync(originalSQL, queryCharacteristics, performanceHistory);

            // Step 4: Apply optimizations
            var optimizedSQL = await ApplyOptimizationsAsync(originalSQL, optimizations, schema);

            // Step 5: Estimate performance improvement
            var performanceEstimate = await EstimatePerformanceImprovementAsync(originalSQL, optimizedSQL, queryCharacteristics);

            var result = new OptimizedQueryResult
            {
                OriginalSQL = originalSQL,
                OptimizedSQL = optimizedSQL,
                AppliedOptimizations = optimizations,
                EstimatedImprovement = performanceEstimate,
                Confidence = CalculateOptimizationConfidence(optimizations, performanceHistory),
                Metadata = new Dictionary<string, object>
                {
                    ["optimization_timestamp"] = DateTime.UtcNow,
                    ["query_characteristics"] = queryCharacteristics,
                    ["performance_history_count"] = performanceHistory.Count,
                    ["user_id"] = userId ?? "anonymous"
                }
            };

            _logger.LogDebug("Adaptive optimization completed with {OptimizationCount} optimizations applied",
                optimizations.Count);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in adaptive query optimization");
            return new OptimizedQueryResult
            {
                OriginalSQL = originalSQL,
                OptimizedSQL = originalSQL,
                AppliedOptimizations = new List<QueryOptimization>(),
                EstimatedImprovement = new PerformanceEstimate(),
                Confidence = 0.5,
                Metadata = new Dictionary<string, object> { ["error"] = true }
            };
        }
    }

    /// <summary>
    /// Record query execution performance for learning
    /// </summary>
    public async Task RecordQueryPerformanceAsync(
        string sql,
        TimeSpan executionTime,
        int rowsReturned,
        bool success,
        string? userId = null)
    {
        try
        {
            var performance = new QueryPerformanceRecord
            {
                SQL = sql,
                ExecutionTime = executionTime,
                RowsReturned = rowsReturned,
                Success = success,
                UserId = userId,
                Timestamp = DateTime.UtcNow
            };

            await _performanceTracker.RecordPerformanceAsync(performance);
            _logger.LogDebug("Recorded query performance: {ExecutionTime}ms, {RowsReturned} rows",
                executionTime.TotalMilliseconds, rowsReturned);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recording query performance");
        }
    }

    /// <summary>
    /// Get optimization recommendations for a query pattern
    /// </summary>
    public async Task<List<OptimizationRecommendation>> GetOptimizationRecommendationsAsync(
        string sql,
        SchemaMetadata schema)
    {
        try
        {
            var recommendations = new List<OptimizationRecommendation>();

            // Analyze query for common optimization opportunities
            var analysis = await AnalyzeSQLForOptimizationsAsync(sql, schema);

            // Generate recommendations based on analysis
            recommendations.AddRange(await GenerateIndexRecommendationsAsync(analysis, schema));
            recommendations.AddRange(await GenerateJoinOptimizationRecommendationsAsync(analysis, schema));
            recommendations.AddRange(await GenerateFilterOptimizationRecommendationsAsync(analysis, schema));
            recommendations.AddRange(await GenerateAggregationOptimizationRecommendationsAsync(analysis, schema));

            // Rank recommendations by potential impact
            recommendations = recommendations
                .OrderByDescending(r => r.EstimatedImpact)
                .Take(10)
                .ToList();

            _logger.LogDebug("Generated {RecommendationCount} optimization recommendations", recommendations.Count);
            return recommendations;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating optimization recommendations");
            return new List<OptimizationRecommendation>();
        }
    }

    // Private methods

    private async Task<QueryCharacteristics> AnalyzeQueryCharacteristicsAsync(
        string sql,
        SemanticAnalysis semanticAnalysis,
        SchemaMetadata schema)
    {
        var characteristics = new QueryCharacteristics
        {
            SQL = sql,
            QueryType = DetermineQueryType(sql),
            TablesInvolved = ExtractTablesFromSQL(sql, schema),
            JoinCount = CountJoins(sql),
            FilterCount = CountFilters(sql),
            AggregationCount = CountAggregations(sql),
            OrderByCount = CountOrderBy(sql),
            HasSubqueries = HasSubqueries(sql),
            EstimatedComplexity = CalculateComplexity(sql),
            SemanticIntent = semanticAnalysis.Intent,
            EntityCount = semanticAnalysis.Entities.Count
        };

        return characteristics;
    }

    private async Task<List<QueryOptimization>> GenerateOptimizationsAsync(
        string sql,
        QueryCharacteristics characteristics,
        List<QueryPerformanceRecord> performanceHistory)
    {
        var optimizations = new List<QueryOptimization>();

        // Rule-based optimizations
        optimizations.AddRange(await _ruleEngine.GenerateRuleBasedOptimizationsAsync(sql, characteristics));

        // Performance-based optimizations
        optimizations.AddRange(await GeneratePerformanceBasedOptimizationsAsync(characteristics, performanceHistory));

        // Schema-aware optimizations
        optimizations.AddRange(await GenerateSchemaAwareOptimizationsAsync(sql, characteristics));

        return optimizations.OrderByDescending(o => o.Priority).ToList();
    }

    private async Task<string> ApplyOptimizationsAsync(
        string originalSQL,
        List<QueryOptimization> optimizations,
        SchemaMetadata schema)
    {
        var optimizedSQL = originalSQL;

        foreach (var optimization in optimizations.OrderByDescending(o => o.Priority))
        {
            try
            {
                optimizedSQL = await ApplyOptimizationAsync(optimizedSQL, optimization, schema);
                _logger.LogDebug("Applied optimization: {Type}", optimization.Type);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to apply optimization: {Type}", optimization.Type);
            }
        }

        return optimizedSQL;
    }

    private async Task<string> ApplyOptimizationAsync(
        string sql,
        QueryOptimization optimization,
        SchemaMetadata schema)
    {
        return optimization.Type switch
        {
            OptimizationType.AddNolockHints => AddNolockHints(sql, schema),
            OptimizationType.OptimizeJoinOrder => OptimizeJoinOrder(sql),
            OptimizationType.AddIndexHints => AddIndexHints(sql, optimization.Parameters),
            OptimizationType.RewriteSubquery => RewriteSubquery(sql),
            OptimizationType.OptimizeWhereClause => OptimizeWhereClause(sql),
            OptimizationType.AddQueryHints => AddQueryHints(sql, optimization.Parameters),
            _ => sql
        };
    }

    private string AddNolockHints(string sql, SchemaMetadata schema)
    {
        var optimizedSQL = sql;

        foreach (var table in schema.Tables)
        {
            var pattern = $@"\b{table.Name}\b(?!\s+WITH\s*\(NOLOCK\))";
            var replacement = $"{table.Name} WITH (NOLOCK)";

            optimizedSQL = System.Text.RegularExpressions.Regex.Replace(
                optimizedSQL, pattern, replacement,
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        }

        return optimizedSQL;
    }

    private string OptimizeJoinOrder(string sql)
    {
        // Simple join order optimization
        // In practice, this would be much more sophisticated
        return sql;
    }

    private string AddIndexHints(string sql, Dictionary<string, object> parameters)
    {
        if (parameters.ContainsKey("index_name"))
        {
            var indexName = parameters["index_name"].ToString();
            // Add index hint logic here
        }
        return sql;
    }

    private string RewriteSubquery(string sql)
    {
        // Subquery rewriting logic
        return sql;
    }

    private string OptimizeWhereClause(string sql)
    {
        // WHERE clause optimization logic
        return sql;
    }

    private string AddQueryHints(string sql, Dictionary<string, object> parameters)
    {
        if (!sql.Contains("OPTION", StringComparison.OrdinalIgnoreCase))
        {
            sql += "\nOPTION (RECOMPILE)";
        }
        return sql;
    }

    private async Task<List<QueryOptimization>> GeneratePerformanceBasedOptimizationsAsync(
        QueryCharacteristics characteristics,
        List<QueryPerformanceRecord> performanceHistory)
    {
        var optimizations = new List<QueryOptimization>();

        // Analyze performance patterns
        if (performanceHistory.Any())
        {
            var avgExecutionTime = performanceHistory.Average(p => p.ExecutionTime.TotalMilliseconds);

            if (avgExecutionTime > 5000) // Slow queries
            {
                optimizations.Add(new QueryOptimization
                {
                    Type = OptimizationType.AddNolockHints,
                    Description = "Add NOLOCK hints to improve read performance",
                    Priority = 8,
                    EstimatedImprovement = 0.3
                });
            }

            if (characteristics.JoinCount > 2)
            {
                optimizations.Add(new QueryOptimization
                {
                    Type = OptimizationType.OptimizeJoinOrder,
                    Description = "Optimize join order based on table sizes",
                    Priority = 7,
                    EstimatedImprovement = 0.2
                });
            }
        }

        return optimizations;
    }

    private async Task<List<QueryOptimization>> GenerateSchemaAwareOptimizationsAsync(
        string sql,
        QueryCharacteristics characteristics)
    {
        var optimizations = new List<QueryOptimization>();

        // Schema-specific optimizations
        if (characteristics.TablesInvolved.Contains("tbl_Daily_actions"))
        {
            optimizations.Add(new QueryOptimization
            {
                Type = OptimizationType.AddIndexHints,
                Description = "Use clustered index on tbl_Daily_actions",
                Priority = 6,
                EstimatedImprovement = 0.15,
                Parameters = new Dictionary<string, object> { ["index_name"] = "PK_Daily_actions" }
            });
        }

        return optimizations;
    }

    // Helper methods for SQL analysis
    private QueryType DetermineQueryType(string sql)
    {
        var sqlLower = sql.ToLowerInvariant().Trim();

        if (sqlLower.StartsWith("select")) return QueryType.Select;
        if (sqlLower.StartsWith("insert")) return QueryType.Insert;
        if (sqlLower.StartsWith("update")) return QueryType.Update;
        if (sqlLower.StartsWith("delete")) return QueryType.Delete;

        return QueryType.Unknown;
    }

    private List<string> ExtractTablesFromSQL(string sql, SchemaMetadata schema)
    {
        var tables = new List<string>();
        var sqlLower = sql.ToLowerInvariant();

        foreach (var table in schema.Tables)
        {
            if (sqlLower.Contains(table.Name.ToLowerInvariant()))
            {
                tables.Add(table.Name);
            }
        }

        return tables;
    }

    private int CountJoins(string sql)
    {
        var joinKeywords = new[] { " join ", " inner join ", " left join ", " right join ", " full join " };
        return joinKeywords.Sum(keyword =>
            System.Text.RegularExpressions.Regex.Matches(sql, keyword,
                System.Text.RegularExpressions.RegexOptions.IgnoreCase).Count);
    }

    private int CountFilters(string sql)
    {
        var whereCount = System.Text.RegularExpressions.Regex.Matches(sql, @"\bwhere\b",
            System.Text.RegularExpressions.RegexOptions.IgnoreCase).Count;
        var andCount = System.Text.RegularExpressions.Regex.Matches(sql, @"\band\b",
            System.Text.RegularExpressions.RegexOptions.IgnoreCase).Count;

        return whereCount + andCount;
    }

    private int CountAggregations(string sql)
    {
        var aggFunctions = new[] { "sum(", "count(", "avg(", "max(", "min(" };
        return aggFunctions.Sum(func =>
            System.Text.RegularExpressions.Regex.Matches(sql, func,
                System.Text.RegularExpressions.RegexOptions.IgnoreCase).Count);
    }

    private int CountOrderBy(string sql)
    {
        return System.Text.RegularExpressions.Regex.Matches(sql, @"\border\s+by\b",
            System.Text.RegularExpressions.RegexOptions.IgnoreCase).Count;
    }

    private bool HasSubqueries(string sql)
    {
        var openParens = sql.Count(c => c == '(');
        var selectCount = System.Text.RegularExpressions.Regex.Matches(sql, @"\bselect\b",
            System.Text.RegularExpressions.RegexOptions.IgnoreCase).Count;

        return selectCount > 1 && openParens > 0;
    }

    private int CalculateComplexity(string sql)
    {
        var complexity = 1; // Base complexity
        complexity += CountJoins(sql) * 2;
        complexity += CountFilters(sql);
        complexity += CountAggregations(sql) * 2;
        complexity += CountOrderBy(sql);
        complexity += HasSubqueries(sql) ? 3 : 0;

        return complexity;
    }

    private async Task<PerformanceEstimate> EstimatePerformanceImprovementAsync(
        string originalSQL,
        string optimizedSQL,
        QueryCharacteristics characteristics)
    {
        // Simple estimation based on optimizations applied
        var estimatedImprovement = 0.0;

        if (optimizedSQL.Contains("WITH (NOLOCK)") && !originalSQL.Contains("WITH (NOLOCK)"))
        {
            estimatedImprovement += 0.2; // 20% improvement from NOLOCK
        }

        if (optimizedSQL.Contains("OPTION (RECOMPILE)") && !originalSQL.Contains("OPTION (RECOMPILE)"))
        {
            estimatedImprovement += 0.1; // 10% improvement from recompile hint
        }

        return new PerformanceEstimate
        {
            EstimatedImprovementPercentage = estimatedImprovement,
            EstimatedExecutionTimeMs = characteristics.EstimatedComplexity * 100 * (1 - estimatedImprovement),
            Confidence = 0.7
        };
    }

    private double CalculateOptimizationConfidence(
        List<QueryOptimization> optimizations,
        List<QueryPerformanceRecord> performanceHistory)
    {
        var baseConfidence = 0.5;

        // Increase confidence based on number of optimizations
        baseConfidence += optimizations.Count * 0.1;

        // Increase confidence based on performance history
        if (performanceHistory.Count > 5)
        {
            baseConfidence += 0.2;
        }

        return Math.Min(1.0, baseConfidence);
    }

    private async Task<SQLAnalysis> AnalyzeSQLForOptimizationsAsync(string sql, SchemaMetadata schema)
    {
        return new SQLAnalysis
        {
            SQL = sql,
            TablesUsed = ExtractTablesFromSQL(sql, schema),
            JoinCount = CountJoins(sql),
            FilterConditions = CountFilters(sql),
            AggregationCount = CountAggregations(sql),
            HasOrderBy = CountOrderBy(sql) > 0,
            HasSubqueries = HasSubqueries(sql),
            ComplexityScore = CalculateComplexity(sql)
        };
    }

    private async Task<List<OptimizationRecommendation>> GenerateIndexRecommendationsAsync(
        SQLAnalysis analysis,
        SchemaMetadata schema)
    {
        var recommendations = new List<OptimizationRecommendation>();

        if (analysis.FilterConditions > 0)
        {
            recommendations.Add(new OptimizationRecommendation
            {
                Type = "Index",
                Description = "Consider adding indexes on filtered columns",
                EstimatedImpact = 0.4,
                Priority = 8
            });
        }

        return recommendations;
    }

    private async Task<List<OptimizationRecommendation>> GenerateJoinOptimizationRecommendationsAsync(
        SQLAnalysis analysis,
        SchemaMetadata schema)
    {
        var recommendations = new List<OptimizationRecommendation>();

        if (analysis.JoinCount > 2)
        {
            recommendations.Add(new OptimizationRecommendation
            {
                Type = "Join Optimization",
                Description = "Consider optimizing join order for better performance",
                EstimatedImpact = 0.3,
                Priority = 7
            });
        }

        return recommendations;
    }

    private async Task<List<OptimizationRecommendation>> GenerateFilterOptimizationRecommendationsAsync(
        SQLAnalysis analysis,
        SchemaMetadata schema)
    {
        var recommendations = new List<OptimizationRecommendation>();

        if (analysis.FilterConditions > 3)
        {
            recommendations.Add(new OptimizationRecommendation
            {
                Type = "Filter Optimization",
                Description = "Consider reordering WHERE conditions for better performance",
                EstimatedImpact = 0.2,
                Priority = 6
            });
        }

        return recommendations;
    }

    private async Task<List<OptimizationRecommendation>> GenerateAggregationOptimizationRecommendationsAsync(
        SQLAnalysis analysis,
        SchemaMetadata schema)
    {
        var recommendations = new List<OptimizationRecommendation>();

        if (analysis.AggregationCount > 1)
        {
            recommendations.Add(new OptimizationRecommendation
            {
                Type = "Aggregation Optimization",
                Description = "Consider using materialized views for complex aggregations",
                EstimatedImpact = 0.5,
                Priority = 9
            });
        }

        return recommendations;
    }
}
