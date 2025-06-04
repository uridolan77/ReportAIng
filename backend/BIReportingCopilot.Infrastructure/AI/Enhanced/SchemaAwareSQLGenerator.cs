using Microsoft.Extensions.Logging;
using BIReportingCopilot.Core.Interfaces;
using BIReportingCopilot.Core.Models;
using BIReportingCopilot.Infrastructure.Performance;

namespace BIReportingCopilot.Infrastructure.AI.Enhanced;

/// <summary>
/// Enhanced SQL generator with schema awareness and query decomposition
/// Implements Enhancement 8: Schema-Aware SQL Generation with Decomposition
/// </summary>
public class SchemaAwareSQLGenerator
{
    private readonly ILogger<SchemaAwareSQLGenerator> _logger;
    private readonly IAIService _aiService;
    private readonly ISchemaService _schemaService;
    private readonly ICacheService _cacheService;
    private readonly QueryDecomposer _queryDecomposer;
    private readonly SchemaRelationshipAnalyzer _relationshipAnalyzer;
    private readonly SQLOptimizer _sqlOptimizer;

    public SchemaAwareSQLGenerator(
        ILogger<SchemaAwareSQLGenerator> logger,
        IAIService aiService,
        ISchemaService schemaService,
        ICacheService cacheService)
    {
        _logger = logger;
        _aiService = aiService;
        _schemaService = schemaService;
        _cacheService = cacheService;
        _queryDecomposer = new QueryDecomposer(logger, aiService, schemaService);
        _relationshipAnalyzer = new SchemaRelationshipAnalyzer(logger);
        _sqlOptimizer = new SQLOptimizer(logger);
    }

    /// <summary>
    /// Generate SQL with schema awareness and decomposition
    /// </summary>
    public async Task<GeneratedQuery> GenerateAsync(
        string naturalLanguageQuery,
        SemanticAnalysis semanticAnalysis,
        string? userId = null)
    {
        try
        {
            _logger.LogDebug("Starting schema-aware SQL generation for: {Query}", naturalLanguageQuery);

            // Step 1: Get schema metadata
            var schema = await _schemaService.GetSchemaMetadataAsync();

            // Step 2: Analyze schema relationships
            var relationships = await _relationshipAnalyzer.AnalyzeRelationshipsAsync(
                semanticAnalysis, schema);

            // Step 3: Decompose complex queries
            var decomposition = await _queryDecomposer.DecomposeQueryAsync(
                naturalLanguageQuery, semanticAnalysis, schema);

            // Step 4: Generate SQL for each sub-query
            var subQueryResults = await GenerateSubQuerySQLAsync(
                decomposition, schema, relationships);

            // Step 5: Combine and optimize
            var finalSQL = await CombineAndOptimizeAsync(
                subQueryResults, decomposition, schema);

            // Step 6: Calculate confidence and create result
            var result = new GeneratedQuery
            {
                SQL = finalSQL.SQL,
                Explanation = finalSQL.Explanation,
                ConfidenceScore = await CalculateConfidenceAsync(finalSQL, semanticAnalysis, schema),
                Alternatives = await GenerateAlternativesAsync(naturalLanguageQuery, semanticAnalysis, schema),
                ExecutionPlan = await GenerateExecutionPlanAsync(finalSQL.SQL, schema),
                Metadata = new Dictionary<string, object>
                {
                    ["decomposition_strategy"] = decomposition.Strategy.ToString(),
                    ["sub_query_count"] = decomposition.SubQueries.Count,
                    ["schema_relationships_used"] = relationships.Count,
                    ["generation_timestamp"] = DateTime.UtcNow
                }
            };

            _logger.LogDebug("Schema-aware SQL generation completed with confidence: {Confidence}", 
                result.ConfidenceScore);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in schema-aware SQL generation");
            return await CreateFallbackQuery(naturalLanguageQuery);
        }
    }

    /// <summary>
    /// Generate SQL for each sub-query in the decomposition
    /// </summary>
    private async Task<List<SubQuerySQLResult>> GenerateSubQuerySQLAsync(
        QueryDecomposition decomposition,
        SchemaMetadata schema,
        List<SchemaRelationship> relationships)
    {
        var results = new List<SubQuerySQLResult>();

        foreach (var subQuery in decomposition.SubQueries.OrderBy(sq => sq.Priority))
        {
            try
            {
                _logger.LogDebug("Generating SQL for sub-query: {Query}", subQuery.Query);

                // Build context-aware prompt for this sub-query
                var prompt = await BuildSubQueryPromptAsync(subQuery, schema, relationships, results);

                // Generate SQL using AI service
                var sql = await _aiService.GenerateSQLAsync(prompt);

                // Validate and optimize the generated SQL
                var optimizedSQL = await _sqlOptimizer.OptimizeAsync(sql, subQuery, schema);

                var result = new SubQuerySQLResult
                {
                    SubQuery = subQuery,
                    SQL = optimizedSQL.SQL,
                    Explanation = optimizedSQL.Explanation,
                    Confidence = optimizedSQL.Confidence,
                    UsedTables = ExtractUsedTables(optimizedSQL.SQL, schema),
                    EstimatedCost = await EstimateQueryCostAsync(optimizedSQL.SQL, schema)
                };

                results.Add(result);
                _logger.LogDebug("Sub-query SQL generated with confidence: {Confidence}", result.Confidence);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating SQL for sub-query: {Query}", subQuery.Query);
                
                // Add fallback result
                results.Add(new SubQuerySQLResult
                {
                    SubQuery = subQuery,
                    SQL = $"-- Error generating SQL for: {subQuery.Query}",
                    Explanation = "Fallback due to generation error",
                    Confidence = 0.1,
                    UsedTables = new List<string>(),
                    EstimatedCost = 0
                });
            }
        }

        return results;
    }

    /// <summary>
    /// Build context-aware prompt for sub-query SQL generation
    /// </summary>
    private async Task<string> BuildSubQueryPromptAsync(
        SubQuery subQuery,
        SchemaMetadata schema,
        List<SchemaRelationship> relationships,
        List<SubQuerySQLResult> previousResults)
    {
        var prompt = $@"Generate SQL for this business question: ""{subQuery.Query}""

SCHEMA CONTEXT:
{BuildSchemaContext(subQuery.ExpectedTables, schema)}

RELATIONSHIPS:
{BuildRelationshipContext(subQuery.ExpectedTables, relationships)}

QUERY TYPE: {subQuery.Type}
PRIORITY: {subQuery.Priority}";

        // Add dependency context if this sub-query depends on others
        if (subQuery.Dependencies.Any())
        {
            prompt += "\n\nDEPENDENCIES:";
            foreach (var depIndex in subQuery.Dependencies)
            {
                if (depIndex < previousResults.Count)
                {
                    var depResult = previousResults[depIndex];
                    prompt += $"\nDepends on: {depResult.SubQuery.Query}";
                    prompt += $"\nPrevious SQL: {depResult.SQL}";
                }
            }
        }

        prompt += @"

REQUIREMENTS:
- Use WITH (NOLOCK) hints for better read performance
- Include proper JOIN conditions based on relationships
- Use appropriate aggregation functions
- Add meaningful column aliases
- Optimize for performance

Generate only the SQL query without explanations.";

        return prompt;
    }

    /// <summary>
    /// Combine sub-query results into final optimized SQL
    /// </summary>
    private async Task<CombinedSQLResult> CombineAndOptimizeAsync(
        List<SubQuerySQLResult> subQueryResults,
        QueryDecomposition decomposition,
        SchemaMetadata schema)
    {
        if (subQueryResults.Count == 1)
        {
            // Single query - just optimize it
            var singleResult = subQueryResults.First();
            var optimized = await _sqlOptimizer.FinalOptimizeAsync(singleResult.SQL, schema);
            
            return new CombinedSQLResult
            {
                SQL = optimized.SQL,
                Explanation = optimized.Explanation,
                CombinationStrategy = "single_query"
            };
        }

        // Multiple queries - combine based on strategy
        return decomposition.Strategy switch
        {
            DecompositionStrategy.Temporal => await CombineTemporalQueries(subQueryResults, schema),
            DecompositionStrategy.EntityBased => await CombineEntityQueries(subQueryResults, schema),
            DecompositionStrategy.Aggregation => await CombineAggregationQueries(subQueryResults, schema),
            DecompositionStrategy.FilterFirst => await CombineFilterFirstQueries(subQueryResults, schema),
            DecompositionStrategy.Hierarchical => await CombineHierarchicalQueries(subQueryResults, schema),
            _ => await CombineDefaultQueries(subQueryResults, schema)
        };
    }

    /// <summary>
    /// Combine temporal queries using UNION or temporal joins
    /// </summary>
    private async Task<CombinedSQLResult> CombineTemporalQueries(
        List<SubQuerySQLResult> subQueryResults,
        SchemaMetadata schema)
    {
        var validQueries = subQueryResults.Where(r => !r.SQL.StartsWith("--")).ToList();
        
        if (validQueries.Count <= 1)
        {
            return new CombinedSQLResult
            {
                SQL = validQueries.FirstOrDefault()?.SQL ?? "-- No valid queries to combine",
                Explanation = "Single temporal query or no valid queries",
                CombinationStrategy = "temporal_single"
            };
        }

        // Combine using UNION ALL for temporal aggregation
        var combinedSQL = string.Join("\nUNION ALL\n", validQueries.Select(r => $"({r.SQL})"));
        
        var finalSQL = $@"-- Combined temporal query
WITH combined_temporal AS (
{combinedSQL}
)
SELECT * FROM combined_temporal
ORDER BY [Date] -- Assuming date column exists";

        return new CombinedSQLResult
        {
            SQL = finalSQL,
            Explanation = "Combined multiple temporal queries using UNION ALL",
            CombinationStrategy = "temporal_union"
        };
    }

    /// <summary>
    /// Combine entity-based queries using appropriate joins
    /// </summary>
    private async Task<CombinedSQLResult> CombineEntityQueries(
        List<SubQuerySQLResult> subQueryResults,
        SchemaMetadata schema)
    {
        var validQueries = subQueryResults.Where(r => !r.SQL.StartsWith("--")).ToList();
        
        if (validQueries.Count <= 1)
        {
            return new CombinedSQLResult
            {
                SQL = validQueries.FirstOrDefault()?.SQL ?? "-- No valid queries to combine",
                Explanation = "Single entity query or no valid queries",
                CombinationStrategy = "entity_single"
            };
        }

        // Create CTEs for each entity query and join them
        var ctes = new List<string>();
        var joinConditions = new List<string>();

        for (int i = 0; i < validQueries.Count; i++)
        {
            var query = validQueries[i];
            var cteName = $"entity_{i + 1}";
            ctes.Add($"{cteName} AS (\n{query.SQL}\n)");
            
            if (i > 0)
            {
                // Simple join condition - would need more sophisticated logic
                joinConditions.Add($"LEFT JOIN {cteName} ON entity_1.PlayerID = {cteName}.PlayerID");
            }
        }

        var finalSQL = $@"-- Combined entity-based query
WITH {string.Join(",\n", ctes)}
SELECT *
FROM entity_1
{string.Join("\n", joinConditions)}";

        return new CombinedSQLResult
        {
            SQL = finalSQL,
            Explanation = "Combined entity queries using CTEs and joins",
            CombinationStrategy = "entity_join"
        };
    }

    /// <summary>
    /// Combine aggregation queries
    /// </summary>
    private async Task<CombinedSQLResult> CombineAggregationQueries(
        List<SubQuerySQLResult> subQueryResults,
        SchemaMetadata schema)
    {
        var validQueries = subQueryResults.Where(r => !r.SQL.StartsWith("--")).ToList();
        
        // For aggregation queries, we typically want to combine the aggregations
        var combinedSQL = $@"-- Combined aggregation query
SELECT 
    {string.Join(",\n    ", validQueries.Select((q, i) => $"agg_{i + 1}.result AS aggregation_{i + 1}"))}
FROM ({validQueries.First().SQL}) agg_1";

        for (int i = 1; i < validQueries.Count; i++)
        {
            combinedSQL += $"\nCROSS JOIN ({validQueries[i].SQL}) agg_{i + 1}";
        }

        return new CombinedSQLResult
        {
            SQL = combinedSQL,
            Explanation = "Combined multiple aggregation queries using CROSS JOIN",
            CombinationStrategy = "aggregation_cross_join"
        };
    }

    /// <summary>
    /// Combine filter-first queries using nested structure
    /// </summary>
    private async Task<CombinedSQLResult> CombineFilterFirstQueries(
        List<SubQuerySQLResult> subQueryResults,
        SchemaMetadata schema)
    {
        var validQueries = subQueryResults.Where(r => !r.SQL.StartsWith("--")).ToList();
        
        if (validQueries.Count <= 1)
        {
            return new CombinedSQLResult
            {
                SQL = validQueries.FirstOrDefault()?.SQL ?? "-- No valid queries to combine",
                Explanation = "Single filter query or no valid queries",
                CombinationStrategy = "filter_single"
            };
        }

        // First query should be the filter, subsequent queries use the filtered data
        var filterQuery = validQueries.First();
        var aggregationQueries = validQueries.Skip(1).ToList();

        var finalSQL = $@"-- Filter-first combined query
WITH filtered_data AS (
{filterQuery.SQL}
)";

        if (aggregationQueries.Any())
        {
            finalSQL += $@"
SELECT 
    {string.Join(",\n    ", aggregationQueries.Select((q, i) => $"-- Aggregation {i + 1} would be applied here"))}
FROM filtered_data";
        }
        else
        {
            finalSQL += "\nSELECT * FROM filtered_data";
        }

        return new CombinedSQLResult
        {
            SQL = finalSQL,
            Explanation = "Applied filters first, then aggregations on filtered data",
            CombinationStrategy = "filter_first"
        };
    }

    /// <summary>
    /// Combine hierarchical queries using layered CTEs
    /// </summary>
    private async Task<CombinedSQLResult> CombineHierarchicalQueries(
        List<SubQuerySQLResult> subQueryResults,
        SchemaMetadata schema)
    {
        var validQueries = subQueryResults.Where(r => !r.SQL.StartsWith("--")).ToList();
        
        var ctes = new List<string>();
        for (int i = 0; i < validQueries.Count; i++)
        {
            var layerName = validQueries[i].SubQuery.Type.ToString().ToLowerInvariant();
            ctes.Add($"{layerName}_layer AS (\n{validQueries[i].SQL}\n)");
        }

        var finalSQL = $@"-- Hierarchical combined query
WITH {string.Join(",\n", ctes)}
SELECT * FROM {validQueries.Last().SubQuery.Type.ToString().ToLowerInvariant()}_layer";

        return new CombinedSQLResult
        {
            SQL = finalSQL,
            Explanation = "Combined queries in hierarchical layers using CTEs",
            CombinationStrategy = "hierarchical_layers"
        };
    }

    /// <summary>
    /// Default combination strategy
    /// </summary>
    private async Task<CombinedSQLResult> CombineDefaultQueries(
        List<SubQuerySQLResult> subQueryResults,
        SchemaMetadata schema)
    {
        var validQueries = subQueryResults.Where(r => !r.SQL.StartsWith("--")).ToList();
        
        if (!validQueries.Any())
        {
            return new CombinedSQLResult
            {
                SQL = "-- No valid queries to combine",
                Explanation = "All sub-queries failed to generate",
                CombinationStrategy = "fallback"
            };
        }

        // Just use the first valid query as fallback
        return new CombinedSQLResult
        {
            SQL = validQueries.First().SQL,
            Explanation = "Using first valid sub-query as fallback",
            CombinationStrategy = "default_first"
        };
    }

    // Helper methods
    private string BuildSchemaContext(List<string> expectedTables, SchemaMetadata schema)
    {
        var relevantTables = schema.Tables
            .Where(t => expectedTables.Contains(t.Name, StringComparer.OrdinalIgnoreCase))
            .ToList();

        if (!relevantTables.Any())
        {
            relevantTables = schema.Tables.Take(3).ToList(); // Fallback to first few tables
        }

        var context = "";
        foreach (var table in relevantTables)
        {
            context += $"\nTable: {table.Name}";
            context += $"\nColumns: {string.Join(", ", table.Columns.Take(10).Select(c => $"{c.Name} ({c.DataType})"))}";
            if (!string.IsNullOrEmpty(table.Description))
            {
                context += $"\nDescription: {table.Description}";
            }
            context += "\n";
        }

        return context;
    }

    private string BuildRelationshipContext(List<string> expectedTables, List<SchemaRelationship> relationships)
    {
        var relevantRelationships = relationships
            .Where(r => expectedTables.Contains(r.FromTable, StringComparer.OrdinalIgnoreCase) ||
                       expectedTables.Contains(r.ToTable, StringComparer.OrdinalIgnoreCase))
            .ToList();

        if (!relevantRelationships.Any())
        {
            return "No specific relationships identified for these tables.";
        }

        var context = "";
        foreach (var rel in relevantRelationships)
        {
            context += $"\n{rel.FromTable}.{rel.FromColumn} -> {rel.ToTable}.{rel.ToColumn} ({rel.RelationshipType})";
        }

        return context;
    }

    private List<string> ExtractUsedTables(string sql, SchemaMetadata schema)
    {
        var usedTables = new List<string>();
        var sqlLower = sql.ToLowerInvariant();

        foreach (var table in schema.Tables)
        {
            if (sqlLower.Contains(table.Name.ToLowerInvariant()))
            {
                usedTables.Add(table.Name);
            }
        }

        return usedTables;
    }

    private async Task<double> EstimateQueryCostAsync(string sql, SchemaMetadata schema)
    {
        // Simple cost estimation based on query complexity
        var cost = 1.0;
        
        var sqlLower = sql.ToLowerInvariant();
        if (sqlLower.Contains("join")) cost += 2.0;
        if (sqlLower.Contains("group by")) cost += 1.5;
        if (sqlLower.Contains("order by")) cost += 1.0;
        if (sqlLower.Contains("union")) cost += 2.5;

        return cost;
    }

    private async Task<double> CalculateConfidenceAsync(
        CombinedSQLResult sqlResult, 
        SemanticAnalysis semanticAnalysis, 
        SchemaMetadata schema)
    {
        var confidence = 0.5; // Base confidence

        // Boost confidence based on successful generation
        if (!sqlResult.SQL.StartsWith("--"))
        {
            confidence += 0.3;
        }

        // Boost confidence based on schema alignment
        var usedTables = ExtractUsedTables(sqlResult.SQL, schema);
        if (usedTables.Any())
        {
            confidence += 0.2;
        }

        return Math.Min(1.0, confidence);
    }

    private async Task<List<string>> GenerateAlternativesAsync(
        string naturalLanguageQuery, 
        SemanticAnalysis semanticAnalysis, 
        SchemaMetadata schema)
    {
        // Placeholder for alternative generation
        return new List<string>();
    }

    private async Task<string> GenerateExecutionPlanAsync(string sql, SchemaMetadata schema)
    {
        // Placeholder for execution plan generation
        return "Execution plan analysis not implemented";
    }

    private async Task<GeneratedQuery> CreateFallbackQuery(string naturalLanguageQuery)
    {
        return new GeneratedQuery
        {
            SQL = $"-- Unable to generate SQL for: {naturalLanguageQuery}",
            Explanation = "Fallback due to generation error",
            ConfidenceScore = 0.1,
            Alternatives = new List<string>(),
            ExecutionPlan = "No execution plan available",
            Metadata = new Dictionary<string, object>
            {
                ["fallback"] = true,
                ["generation_timestamp"] = DateTime.UtcNow
            }
        };
    }
}

// Supporting classes and data structures will be added in the next file...
