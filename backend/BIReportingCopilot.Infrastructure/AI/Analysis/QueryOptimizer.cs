using Microsoft.Extensions.Logging;
using BIReportingCopilot.Core.Interfaces;
using BIReportingCopilot.Core.Interfaces.AI;
using BIReportingCopilot.Core.Interfaces.Query;
using BIReportingCopilot.Core.Models;
using System.Text.RegularExpressions;
using IQueryOptimizer = BIReportingCopilot.Core.Interfaces.IQueryOptimizer;
using SqlCandidate = BIReportingCopilot.Core.Models.SqlCandidate;
using OptimizedQuery = BIReportingCopilot.Core.Models.OptimizedQuery;
using UserContext = BIReportingCopilot.Core.Models.UserContext;
using SchemaContext = BIReportingCopilot.Core.Models.SchemaContext;
using QueryPerformanceAnalysis = BIReportingCopilot.Core.Models.QueryPerformanceAnalysis;
using QueryImprovement = BIReportingCopilot.Core.Models.QueryImprovement;
using QueryValidationResult = BIReportingCopilot.Core.Models.QueryValidationResult;

namespace BIReportingCopilot.Infrastructure.AI.Analysis;

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
                ConfidenceScore = bestCandidate.ConfidenceScore,
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
                ConfidenceScore = 0.8, // High initial score for primary candidate
                Explanation = "Primary candidate generated with full context",
                RequiredTables = schema.RelevantTables.Select(t => t.Name).ToList(),
                EstimatedComplexity = QueryComplexity.Medium,
                EstimatedExecutionTime = TimeSpan.FromMilliseconds(500)
            });

            // Generate alternative candidates with different approaches
            if (analysis.Intent == QueryIntent.Aggregation)
            {
                var aggregationSql = await GenerateAggregationCandidate(analysis, schema);
                if (!string.IsNullOrEmpty(aggregationSql) && aggregationSql != primarySql)
                {
                    candidates.Add(new SqlCandidate
                    {
                        Sql = aggregationSql,
                        ConfidenceScore = 0.7,
                        Explanation = "Optimized for aggregation operations",
                        RequiredTables = schema.RelevantTables.Select(t => t.Name).ToList(),
                        EstimatedComplexity = QueryComplexity.Medium,
                        EstimatedExecutionTime = TimeSpan.FromMilliseconds(400)
                    });
                }
            }

            if (analysis.Intent == QueryIntent.Trend)
            {
                var trendSql = await GenerateTrendCandidate(analysis, schema);
                if (!string.IsNullOrEmpty(trendSql) && trendSql != primarySql)
                {
                    candidates.Add(new SqlCandidate
                    {
                        Sql = trendSql,
                        ConfidenceScore = 0.7,
                        Explanation = "Optimized for time-series analysis",
                        RequiredTables = schema.RelevantTables.Select(t => t.Name).ToList(),
                        EstimatedComplexity = QueryComplexity.Medium,
                        EstimatedExecutionTime = TimeSpan.FromMilliseconds(600)
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
                        ConfidenceScore = 0.6,
                        Explanation = "Simplified version for better performance",
                        RequiredTables = schema.RelevantTables.Take(3).Select(t => t.Name).ToList(),
                        EstimatedComplexity = QueryComplexity.Low,
                        EstimatedExecutionTime = TimeSpan.FromMilliseconds(300)
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
                    ConfidenceScore = 0.5,
                    Explanation = "Fallback candidate due to error",
                    RequiredTables = new List<string>(),
                    EstimatedComplexity = QueryComplexity.Medium,
                    EstimatedExecutionTime = TimeSpan.FromMilliseconds(1000)
                }
            };
        }
    }

    public Task<QueryPerformanceAnalysis> AnalyzePerformanceAsync(string sql)
    {
        try
        {
            var analysis = new QueryPerformanceAnalysis
            {
                EstimatedExecutionTime = EstimateExecutionTime(AnalyzeQueryComplexity(sql), sql),
                PerformanceIssues = GeneratePerformanceWarnings(sql),
                OptimizationSuggestions = GenerateIndexRecommendations(sql, null),
                Confidence = 0.8
            };

            return Task.FromResult(analysis);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing query performance");
            return Task.FromResult(new QueryPerformanceAnalysis
            {
                EstimatedExecutionTime = TimeSpan.FromSeconds(1),
                PerformanceIssues = new List<string> { "Unable to analyze performance" },
                OptimizationSuggestions = new List<string>(),
                Confidence = 0.5
            });
        }
    }

    public Task<List<QueryImprovement>> SuggestImprovementsAsync(string sql)
    {
        try
        {
            var improvements = new List<QueryImprovement>();

            // Check for SELECT *
            if (sql.Contains("SELECT *", StringComparison.OrdinalIgnoreCase))
            {
                improvements.Add(new QueryImprovement
                {
                    OriginalQuery = sql,
                    ImprovedQuery = sql.Replace("SELECT *", "SELECT [specific columns]"),
                    Suggestions = new List<ImprovementSuggestion>
                    {
                        new ImprovementSuggestion
                        {
                            Type = "Column Selection",
                            Description = "Using SELECT * can impact performance",
                            Example = "Select only the columns you need",
                            Impact = 0.7
                        }
                    },
                    ImprovementScore = 0.7,
                    Reasoning = "Using SELECT * can impact performance"
                });
            }

            // Check for missing WHERE clause
            if (!sql.Contains("WHERE", StringComparison.OrdinalIgnoreCase))
            {
                improvements.Add(new QueryImprovement
                {
                    OriginalQuery = sql,
                    ImprovedQuery = sql + " WHERE [add filter conditions]",
                    Suggestions = new List<ImprovementSuggestion>
                    {
                        new ImprovementSuggestion
                        {
                            Type = "Filtering",
                            Description = "Query may return large result set",
                            Example = "Add WHERE clause to filter results",
                            Impact = 0.8
                        }
                    },
                    ImprovementScore = 0.8,
                    Reasoning = "Query may return large result set without filtering"
                });
            }

            // Check for ORDER BY without LIMIT
            if (sql.Contains("ORDER BY", StringComparison.OrdinalIgnoreCase) &&
                !sql.Contains("LIMIT", StringComparison.OrdinalIgnoreCase))
            {
                improvements.Add(new QueryImprovement
                {
                    OriginalQuery = sql,
                    ImprovedQuery = sql + " LIMIT 1000",
                    Suggestions = new List<ImprovementSuggestion>
                    {
                        new ImprovementSuggestion
                        {
                            Type = "Result Limiting",
                            Description = "ORDER BY without LIMIT can be inefficient",
                            Example = "Add LIMIT clause to restrict result size",
                            Impact = 0.6
                        }
                    },
                    ImprovementScore = 0.6,
                    Reasoning = "ORDER BY without LIMIT can be inefficient for large datasets"
                });
            }

            return Task.FromResult(improvements);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error suggesting improvements");
            return Task.FromResult(new List<QueryImprovement>());
        }
    }

    public Task<QueryValidationResult> ValidateQueryAsync(string sql, SchemaMetadata schema)
    {
        try
        {
            var result = new QueryValidationResult
            {
                IsValid = true,
                Errors = new List<string>(),
                Warnings = new List<string>(),
                Suggestions = new List<string>()
            };

            // Basic SQL validation
            if (string.IsNullOrWhiteSpace(sql))
            {
                result.IsValid = false;
                result.Errors.Add("SQL query cannot be empty");
                return Task.FromResult(result);
            }

            // Check for basic SQL structure
            if (!sql.Contains("SELECT", StringComparison.OrdinalIgnoreCase))
            {
                result.IsValid = false;
                result.Errors.Add("Query must contain SELECT statement");
            }

            // Check for potential SQL injection patterns
            var dangerousPatterns = new[] { "DROP", "DELETE", "TRUNCATE", "ALTER", "CREATE", "INSERT", "UPDATE" };
            foreach (var pattern in dangerousPatterns)
            {
                if (sql.Contains(pattern, StringComparison.OrdinalIgnoreCase))
                {
                    result.Warnings.Add($"Potentially dangerous SQL keyword detected: {pattern}");
                }
            }

            // Performance suggestions
            if (sql.Contains("SELECT *", StringComparison.OrdinalIgnoreCase))
            {
                result.Suggestions.Add("Consider selecting specific columns instead of *");
            }

            return Task.FromResult(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating query");
            return Task.FromResult(new QueryValidationResult
            {
                IsValid = false,
                Errors = new List<string> { $"Validation error: {ex.Message}" },
                Warnings = new List<string>(),
                Suggestions = new List<string>()
            });
        }
    }

    public Task<QueryPerformancePrediction> PredictPerformanceAsync(string sql, SchemaMetadata? schema)
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

            return Task.FromResult(prediction);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error predicting query performance");
            return Task.FromResult(new QueryPerformancePrediction
            {
                EstimatedExecutionTime = TimeSpan.FromSeconds(1),
                EstimatedRowCount = 1000,
                ComplexityScore = 0.5,
                PerformanceWarnings = new List<string> { "Unable to analyze performance" },
                IndexRecommendations = new List<string>()
            });
        }
    }

    public Task<string> RewriteForPerformanceAsync(string sql)
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
            return Task.FromResult(optimizedSql);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rewriting SQL for performance");
            return Task.FromResult(sql); // Return original SQL if optimization fails
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

            candidate.ConfidenceScore = confidence;

            // Update candidate analysis
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
        prompt += "\nIMPORTANT: Always add WITH (NOLOCK) hint to all table references for better read performance.";

        return prompt;
    }

    private async Task<string> GenerateAggregationCandidate(SemanticAnalysis analysis, SchemaContext schema)
    {
        var aggregationPrompt = $"Generate a SQL query optimized for aggregation operations: {analysis.OriginalQuery}\n";
        aggregationPrompt += "Focus on efficient GROUP BY, aggregate functions, and proper indexing considerations. Always use WITH (NOLOCK) hints.";

        return await _aiService.GenerateSQLAsync(aggregationPrompt);
    }

    private async Task<string> GenerateTrendCandidate(SemanticAnalysis analysis, SchemaContext schema)
    {
        var trendPrompt = $"Generate a SQL query optimized for time-series trend analysis: {analysis.OriginalQuery}\n";
        trendPrompt += "Focus on date-based grouping, time windows, and trend calculations. Always use WITH (NOLOCK) hints.";

        return await _aiService.GenerateSQLAsync(trendPrompt);
    }

    private async Task<string> GenerateSimplifiedCandidate(SemanticAnalysis analysis, SchemaContext schema)
    {
        var simplifiedPrompt = $"Generate a simplified, high-performance SQL query: {analysis.OriginalQuery}\n";
        simplifiedPrompt += "Focus on essential data only, minimal joins, and fast execution. Always use WITH (NOLOCK) hints.";

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

    // Interface implementation
    public async Task<QueryOptimizationResult> OptimizeAsync(string query, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Optimizing query: {Query}", query);

            // Create a basic semantic analysis for the query
            var analysis = new SemanticAnalysis
            {
                OriginalQuery = query,
                Intent = QueryIntent.Unknown,
                Entities = new List<SemanticEntity>(),
                Keywords = ExtractKeywords(query)
            };

            // Create a basic schema context
            var schema = new SchemaContext
            {
                RelevantTables = new List<TableMetadata>(),
                SuggestedJoins = new List<string>()
            };

            // Create a basic user context
            var context = new UserContext
            {
                PreferredTables = new List<string>(),
                CommonFilters = new List<string>()
            };

            // Generate candidates
            var candidates = await GenerateCandidatesAsync(analysis, schema);

            // Optimize using existing method
            var optimizedQuery = await OptimizeAsync(candidates, context);

            // Convert to QueryOptimizationResult
            return new QueryOptimizationResult
            {
                OptimizedSql = optimizedQuery.Sql,
                OriginalSql = query,
                Explanation = optimizedQuery.Explanation,
                ConfidenceScore = optimizedQuery.ConfidenceScore,
                PerformanceImprovementEstimate = optimizedQuery.PerformancePrediction?.EstimatedExecutionTime.TotalMilliseconds ?? 0,
                OptimizationSteps = optimizedQuery.OptimizationApplied,
                Warnings = optimizedQuery.PerformancePrediction?.PerformanceWarnings ?? new List<string>(),
                IndexRecommendations = optimizedQuery.PerformancePrediction?.IndexRecommendations ?? new List<string>()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error optimizing query: {Query}", query);
            return new QueryOptimizationResult
            {
                OptimizedSql = query,
                OriginalSql = query,
                Explanation = "Unable to optimize query due to error",
                ConfidenceScore = 0.5,
                PerformanceImprovementEstimate = 0,
                OptimizationSteps = new List<string> { "No optimization applied" },
                Warnings = new List<string> { "Unable to analyze performance" },
                IndexRecommendations = new List<string>()
            };
        }
    }

    private List<string> ExtractKeywords(string query)
    {
        var keywords = new List<string>();
        var words = query.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        foreach (var word in words)
        {
            var cleanWord = word.Trim().ToLowerInvariant();
            if (cleanWord.Length > 3 && !IsCommonWord(cleanWord))
            {
                keywords.Add(cleanWord);
            }
        }

        return keywords;
    }

    private bool IsCommonWord(string word)
    {
        var commonWords = new[] { "the", "and", "for", "are", "but", "not", "you", "all", "can", "had", "her", "was", "one", "our", "out", "day", "get", "has", "him", "his", "how", "man", "new", "now", "old", "see", "two", "way", "who", "boy", "did", "its", "let", "put", "say", "she", "too", "use" };
        return commonWords.Contains(word);
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
        // Analysis is now handled through the ConfidenceScore and Explanation properties
        // Additional analysis can be added to the Explanation if needed
        var sql = candidate.Sql;
        var analysisNotes = new List<string>();

        if (sql.Contains("WHERE", StringComparison.OrdinalIgnoreCase))
            analysisNotes.Add("includes filtering");
        if (sql.Contains("ORDER BY", StringComparison.OrdinalIgnoreCase))
            analysisNotes.Add("results are ordered");
        if (!sql.Contains("SELECT *", StringComparison.OrdinalIgnoreCase))
            analysisNotes.Add("specific column selection");

        if (analysisNotes.Any())
        {
            candidate.Explanation += $" ({string.Join(", ", analysisNotes)})";
        }
    }

    private string GenerateExplanation(SqlCandidate bestCandidate, string optimizedSql)
    {
        var explanation = $"Selected query based on: {bestCandidate.Explanation}\n";
        explanation += $"Confidence Score: {bestCandidate.ConfidenceScore:F2}\n";

        explanation += $"Required Tables: {string.Join(", ", bestCandidate.RequiredTables)}\n";

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
