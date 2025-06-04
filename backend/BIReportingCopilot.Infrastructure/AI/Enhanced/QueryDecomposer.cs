using Microsoft.Extensions.Logging;
using BIReportingCopilot.Core.Models;
using BIReportingCopilot.Core.Interfaces;

namespace BIReportingCopilot.Infrastructure.AI.Enhanced;

/// <summary>
/// Analyzes query complexity to determine decomposition needs
/// </summary>
public class ComplexityAnalyzer
{
    private readonly ILogger _logger;

    public ComplexityAnalyzer(ILogger logger)
    {
        _logger = logger;
    }

    public async Task<QueryComplexityAnalysis> AnalyzeComplexityAsync(
        string query,
        SemanticAnalysis semanticAnalysis,
        SchemaMetadata schema)
    {
        var analysis = new QueryComplexityAnalysis
        {
            Query = query,
            ComplexityScore = 0
        };

        // Analyze various complexity factors
        analysis.AggregationCount = semanticAnalysis.Entities.Count(e => e.Type == EntityType.Aggregation);
        analysis.FilterConditions = semanticAnalysis.Entities.Count(e => e.Type == EntityType.Condition);
        analysis.RequiredJoins = EstimateRequiredJoins(semanticAnalysis, schema);
        analysis.HasComplexTemporal = HasComplexTemporalLogic(semanticAnalysis);
        analysis.EntityCount = semanticAnalysis.Entities.Count;

        // Calculate overall complexity score
        analysis.ComplexityScore = CalculateComplexityScore(analysis);

        return analysis;
    }

    private int EstimateRequiredJoins(SemanticAnalysis semanticAnalysis, SchemaMetadata schema)
    {
        var tableEntities = semanticAnalysis.Entities
            .Where(e => e.Type == EntityType.Table)
            .Select(e => e.Text.ToLowerInvariant())
            .Distinct()
            .ToList();

        return Math.Max(0, tableEntities.Count - 1);
    }

    private bool HasComplexTemporalLogic(SemanticAnalysis semanticAnalysis)
    {
        var temporalEntities = semanticAnalysis.Entities.Where(e => e.Type == EntityType.DateRange).ToList();
        return temporalEntities.Count > 1 ||
               temporalEntities.Any(e => e.Text.Contains("compare") || e.Text.Contains("trend"));
    }

    private int CalculateComplexityScore(QueryComplexityAnalysis analysis)
    {
        var score = 0;
        score += analysis.AggregationCount * 2;
        score += analysis.FilterConditions;
        score += analysis.RequiredJoins * 3;
        score += analysis.HasComplexTemporal ? 4 : 0;
        score += analysis.EntityCount > 5 ? 2 : 0;

        return score;
    }
}

/// <summary>
/// Query complexity analysis result
/// </summary>
public class QueryComplexityAnalysis
{
    public string Query { get; set; } = string.Empty;
    public int ComplexityScore { get; set; }
    public int AggregationCount { get; set; }
    public int FilterConditions { get; set; }
    public int RequiredJoins { get; set; }
    public bool HasComplexTemporal { get; set; }
    public int EntityCount { get; set; }
}

/// <summary>
/// Decomposes complex queries into simpler sub-queries for better SQL generation
/// Implements Enhancement 8: Schema-Aware SQL Generation with Decomposition
/// </summary>
public class QueryDecomposer
{
    private readonly ILogger<QueryDecomposer> _logger;
    private readonly IAIService _aiService;
    private readonly ISchemaService _schemaService;
    private readonly ComplexityAnalyzer _complexityAnalyzer;

    public QueryDecomposer(
        ILogger<QueryDecomposer> logger,
        IAIService aiService,
        ISchemaService schemaService)
    {
        _logger = logger;
        _aiService = aiService;
        _schemaService = schemaService;
        _complexityAnalyzer = new ComplexityAnalyzer(logger);
    }

    /// <summary>
    /// Decompose a complex query into manageable sub-queries
    /// </summary>
    public async Task<QueryDecomposition> DecomposeQueryAsync(
        string naturalLanguageQuery,
        SemanticAnalysis semanticAnalysis,
        SchemaMetadata schema)
    {
        try
        {
            _logger.LogDebug("Starting query decomposition for: {Query}", naturalLanguageQuery);

            // Step 1: Analyze query complexity
            var complexity = await _complexityAnalyzer.AnalyzeComplexityAsync(
                naturalLanguageQuery, semanticAnalysis, schema);

            // Step 2: Determine if decomposition is needed
            if (!ShouldDecompose(complexity))
            {
                _logger.LogDebug("Query is simple enough, no decomposition needed");
                return CreateSimpleDecomposition(naturalLanguageQuery, semanticAnalysis);
            }

            // Step 3: Identify decomposition strategy
            var strategy = DetermineDecompositionStrategy(complexity, semanticAnalysis);
            _logger.LogDebug("Using decomposition strategy: {Strategy}", strategy);

            // Step 4: Decompose based on strategy
            var decomposition = await DecomposeUsingStrategyAsync(
                naturalLanguageQuery, semanticAnalysis, schema, strategy);

            // Step 5: Validate and optimize decomposition
            await ValidateDecompositionAsync(decomposition, schema);

            _logger.LogDebug("Query decomposed into {SubQueryCount} sub-queries",
                decomposition.SubQueries.Count);

            return decomposition;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in query decomposition");
            return CreateFallbackDecomposition(naturalLanguageQuery, semanticAnalysis);
        }
    }

    /// <summary>
    /// Determine if a query should be decomposed
    /// </summary>
    private bool ShouldDecompose(QueryComplexityAnalysis complexity)
    {
        // Decompose if:
        // 1. High complexity score
        // 2. Multiple aggregations
        // 3. Multiple table joins required
        // 4. Complex temporal logic
        // 5. Multiple filtering conditions

        return complexity.ComplexityScore > 7 ||
               complexity.RequiredJoins > 2 ||
               complexity.AggregationCount > 2 ||
               complexity.FilterConditions > 3 ||
               complexity.HasComplexTemporal;
    }

    /// <summary>
    /// Determine the best decomposition strategy
    /// </summary>
    private DecompositionStrategy DetermineDecompositionStrategy(
        QueryComplexityAnalysis complexity,
        SemanticAnalysis semanticAnalysis)
    {
        // Strategy 1: Temporal Decomposition
        if (complexity.HasComplexTemporal && semanticAnalysis.Entities.Any(e => e.Type == EntityType.DateRange))
        {
            return DecompositionStrategy.Temporal;
        }

        // Strategy 2: Entity-Based Decomposition
        if (complexity.RequiredJoins > 2)
        {
            return DecompositionStrategy.EntityBased;
        }

        // Strategy 3: Aggregation Decomposition
        if (complexity.AggregationCount > 2)
        {
            return DecompositionStrategy.Aggregation;
        }

        // Strategy 4: Filter-First Decomposition
        if (complexity.FilterConditions > 3)
        {
            return DecompositionStrategy.FilterFirst;
        }

        // Default: Hierarchical decomposition
        return DecompositionStrategy.Hierarchical;
    }

    /// <summary>
    /// Decompose query using the selected strategy
    /// </summary>
    private async Task<QueryDecomposition> DecomposeUsingStrategyAsync(
        string query,
        SemanticAnalysis semanticAnalysis,
        SchemaMetadata schema,
        DecompositionStrategy strategy)
    {
        return strategy switch
        {
            DecompositionStrategy.Temporal => await DecomposeTemporalAsync(query, semanticAnalysis, schema),
            DecompositionStrategy.EntityBased => await DecomposeEntityBasedAsync(query, semanticAnalysis, schema),
            DecompositionStrategy.Aggregation => await DecomposeAggregationAsync(query, semanticAnalysis, schema),
            DecompositionStrategy.FilterFirst => await DecomposeFilterFirstAsync(query, semanticAnalysis, schema),
            DecompositionStrategy.Hierarchical => await DecomposeHierarchicalAsync(query, semanticAnalysis, schema),
            _ => CreateSimpleDecomposition(query, semanticAnalysis)
        };
    }

    /// <summary>
    /// Temporal decomposition: Break down by time periods
    /// </summary>
    private async Task<QueryDecomposition> DecomposeTemporalAsync(
        string query,
        SemanticAnalysis semanticAnalysis,
        SchemaMetadata schema)
    {
        var decomposition = new QueryDecomposition
        {
            OriginalQuery = query,
            Strategy = DecompositionStrategy.Temporal,
            SubQueries = new List<SubQuery>()
        };

        // Extract temporal entities
        var temporalEntities = semanticAnalysis.Entities
            .Where(e => e.Type == EntityType.DateRange)
            .ToList();

        if (temporalEntities.Any())
        {
            // Create base query without temporal constraints
            var baseQuery = RemoveTemporalConstraints(query, temporalEntities);

            // Create sub-queries for each time period
            foreach (var temporalEntity in temporalEntities)
            {
                var subQuery = new SubQuery
                {
                    Query = $"{baseQuery} for {temporalEntity.Text}",
                    Type = SubQueryType.Temporal,
                    Priority = 1,
                    Dependencies = new List<int>(),
                    ExpectedTables = PredictTablesForQuery(baseQuery, schema),
                    Metadata = new Dictionary<string, object>
                    {
                        ["temporal_constraint"] = temporalEntity.ResolvedValue ?? temporalEntity.Text,
                        ["original_temporal_text"] = temporalEntity.Text
                    }
                };

                decomposition.SubQueries.Add(subQuery);
            }
        }

        return decomposition;
    }

    /// <summary>
    /// Entity-based decomposition: Break down by main entities
    /// </summary>
    private async Task<QueryDecomposition> DecomposeEntityBasedAsync(
        string query,
        SemanticAnalysis semanticAnalysis,
        SchemaMetadata schema)
    {
        var decomposition = new QueryDecomposition
        {
            OriginalQuery = query,
            Strategy = DecompositionStrategy.EntityBased,
            SubQueries = new List<SubQuery>()
        };

        // Group entities by table
        var tableEntities = semanticAnalysis.Entities
            .Where(e => e.Type == EntityType.Table || e.Type == EntityType.Column)
            .GroupBy(e => PredictTableForEntity(e, schema))
            .ToList();

        var priority = 1;
        foreach (var tableGroup in tableEntities)
        {
            if (tableGroup.Key != null)
            {
                var entityTexts = tableGroup.Select(e => e.Text).ToList();
                var subQueryText = BuildEntitySubQuery(query, entityTexts);

                var subQuery = new SubQuery
                {
                    Query = subQueryText,
                    Type = SubQueryType.EntityBased,
                    Priority = priority++,
                    Dependencies = new List<int>(),
                    ExpectedTables = new List<string> { tableGroup.Key },
                    Metadata = new Dictionary<string, object>
                    {
                        ["primary_table"] = tableGroup.Key,
                        ["entities"] = entityTexts
                    }
                };

                decomposition.SubQueries.Add(subQuery);
            }
        }

        return decomposition;
    }

    /// <summary>
    /// Aggregation decomposition: Break down by aggregation types
    /// </summary>
    private async Task<QueryDecomposition> DecomposeAggregationAsync(
        string query,
        SemanticAnalysis semanticAnalysis,
        SchemaMetadata schema)
    {
        var decomposition = new QueryDecomposition
        {
            OriginalQuery = query,
            Strategy = DecompositionStrategy.Aggregation,
            SubQueries = new List<SubQuery>()
        };

        // Extract aggregation entities
        var aggregationEntities = semanticAnalysis.Entities
            .Where(e => e.Type == EntityType.Aggregation)
            .ToList();

        var priority = 1;
        foreach (var aggEntity in aggregationEntities)
        {
            var subQueryText = BuildAggregationSubQuery(query, aggEntity);

            var subQuery = new SubQuery
            {
                Query = subQueryText,
                Type = SubQueryType.Aggregation,
                Priority = priority++,
                Dependencies = new List<int>(),
                ExpectedTables = PredictTablesForQuery(subQueryText, schema),
                Metadata = new Dictionary<string, object>
                {
                    ["aggregation_type"] = aggEntity.Text,
                    ["aggregation_function"] = MapToSQLFunction(aggEntity.Text)
                }
            };

            decomposition.SubQueries.Add(subQuery);
        }

        return decomposition;
    }

    /// <summary>
    /// Filter-first decomposition: Apply filters before aggregations
    /// </summary>
    private async Task<QueryDecomposition> DecomposeFilterFirstAsync(
        string query,
        SemanticAnalysis semanticAnalysis,
        SchemaMetadata schema)
    {
        var decomposition = new QueryDecomposition
        {
            OriginalQuery = query,
            Strategy = DecompositionStrategy.FilterFirst,
            SubQueries = new List<SubQuery>()
        };

        // Extract filter conditions
        var filterEntities = semanticAnalysis.Entities
            .Where(e => e.Type == EntityType.Condition)
            .ToList();

        // Step 1: Create filtering sub-query
        if (filterEntities.Any())
        {
            var filterQuery = BuildFilterSubQuery(query, filterEntities);

            var filterSubQuery = new SubQuery
            {
                Query = filterQuery,
                Type = SubQueryType.Filter,
                Priority = 1,
                Dependencies = new List<int>(),
                ExpectedTables = PredictTablesForQuery(filterQuery, schema),
                Metadata = new Dictionary<string, object>
                {
                    ["filter_conditions"] = filterEntities.Select(e => e.Text).ToList()
                }
            };

            decomposition.SubQueries.Add(filterSubQuery);
        }

        // Step 2: Create aggregation sub-query that depends on filtering
        var aggregationEntities = semanticAnalysis.Entities
            .Where(e => e.Type == EntityType.Aggregation)
            .ToList();

        if (aggregationEntities.Any())
        {
            var aggQuery = BuildAggregationFromFilteredData(query, aggregationEntities);

            var aggSubQuery = new SubQuery
            {
                Query = aggQuery,
                Type = SubQueryType.Aggregation,
                Priority = 2,
                Dependencies = new List<int> { 0 }, // Depends on filter sub-query
                ExpectedTables = PredictTablesForQuery(aggQuery, schema),
                Metadata = new Dictionary<string, object>
                {
                    ["depends_on_filtering"] = true,
                    ["aggregations"] = aggregationEntities.Select(e => e.Text).ToList()
                }
            };

            decomposition.SubQueries.Add(aggSubQuery);
        }

        return decomposition;
    }

    /// <summary>
    /// Hierarchical decomposition: Break down into logical layers
    /// </summary>
    private async Task<QueryDecomposition> DecomposeHierarchicalAsync(
        string query,
        SemanticAnalysis semanticAnalysis,
        SchemaMetadata schema)
    {
        var decomposition = new QueryDecomposition
        {
            OriginalQuery = query,
            Strategy = DecompositionStrategy.Hierarchical,
            SubQueries = new List<SubQuery>()
        };

        // Layer 1: Data retrieval
        var dataRetrievalQuery = ExtractDataRetrievalPart(query, semanticAnalysis);
        if (!string.IsNullOrEmpty(dataRetrievalQuery))
        {
            decomposition.SubQueries.Add(new SubQuery
            {
                Query = dataRetrievalQuery,
                Type = SubQueryType.DataRetrieval,
                Priority = 1,
                Dependencies = new List<int>(),
                ExpectedTables = PredictTablesForQuery(dataRetrievalQuery, schema)
            });
        }

        // Layer 2: Processing/Aggregation
        var processingQuery = ExtractProcessingPart(query, semanticAnalysis);
        if (!string.IsNullOrEmpty(processingQuery))
        {
            decomposition.SubQueries.Add(new SubQuery
            {
                Query = processingQuery,
                Type = SubQueryType.Processing,
                Priority = 2,
                Dependencies = new List<int> { 0 },
                ExpectedTables = new List<string>()
            });
        }

        // Layer 3: Presentation/Formatting
        var presentationQuery = ExtractPresentationPart(query, semanticAnalysis);
        if (!string.IsNullOrEmpty(presentationQuery))
        {
            decomposition.SubQueries.Add(new SubQuery
            {
                Query = presentationQuery,
                Type = SubQueryType.Presentation,
                Priority = 3,
                Dependencies = new List<int> { 1 },
                ExpectedTables = new List<string>()
            });
        }

        return decomposition;
    }

    /// <summary>
    /// Validate the decomposition makes sense
    /// </summary>
    private async Task ValidateDecompositionAsync(QueryDecomposition decomposition, SchemaMetadata schema)
    {
        // Check for circular dependencies
        if (HasCircularDependencies(decomposition))
        {
            _logger.LogWarning("Circular dependencies detected in decomposition, simplifying");
            SimplifyDependencies(decomposition);
        }

        // Validate table references
        foreach (var subQuery in decomposition.SubQueries)
        {
            var validTables = subQuery.ExpectedTables
                .Where(table => schema.Tables.Any(t => t.Name.Equals(table, StringComparison.OrdinalIgnoreCase)))
                .ToList();

            subQuery.ExpectedTables = validTables;
        }

        // Ensure at least one sub-query exists
        if (!decomposition.SubQueries.Any())
        {
            _logger.LogWarning("No valid sub-queries generated, creating fallback");
            decomposition.SubQueries.Add(new SubQuery
            {
                Query = decomposition.OriginalQuery,
                Type = SubQueryType.Simple,
                Priority = 1,
                Dependencies = new List<int>(),
                ExpectedTables = new List<string>()
            });
        }
    }

    // Helper methods for decomposition
    private QueryDecomposition CreateSimpleDecomposition(string query, SemanticAnalysis semanticAnalysis)
    {
        return new QueryDecomposition
        {
            OriginalQuery = query,
            Strategy = DecompositionStrategy.Simple,
            SubQueries = new List<SubQuery>
            {
                new SubQuery
                {
                    Query = query,
                    Type = SubQueryType.Simple,
                    Priority = 1,
                    Dependencies = new List<int>(),
                    ExpectedTables = new List<string>()
                }
            }
        };
    }

    private QueryDecomposition CreateFallbackDecomposition(string query, SemanticAnalysis semanticAnalysis)
    {
        return CreateSimpleDecomposition(query, semanticAnalysis);
    }

    private string RemoveTemporalConstraints(string query, List<SemanticEntity> temporalEntities)
    {
        var cleanedQuery = query;
        foreach (var entity in temporalEntities)
        {
            cleanedQuery = cleanedQuery.Replace(entity.Text, "", StringComparison.OrdinalIgnoreCase);
        }
        return cleanedQuery.Trim();
    }

    private string? PredictTableForEntity(SemanticEntity entity, SchemaMetadata schema)
    {
        // Simple table prediction based on entity text
        var entityText = entity.Text.ToLowerInvariant();

        return schema.Tables.FirstOrDefault(t =>
            t.Name.ToLowerInvariant().Contains(entityText) ||
            entityText.Contains(t.Name.ToLowerInvariant()) ||
            t.Columns.Any(c => c.Name.ToLowerInvariant().Contains(entityText)))?.Name;
    }

    private List<string> PredictTablesForQuery(string query, SchemaMetadata schema)
    {
        var queryLower = query.ToLowerInvariant();
        return schema.Tables
            .Where(t => queryLower.Contains(t.Name.ToLowerInvariant()) ||
                       t.Columns.Any(c => queryLower.Contains(c.Name.ToLowerInvariant())))
            .Select(t => t.Name)
            .ToList();
    }

    private string BuildEntitySubQuery(string originalQuery, List<string> entityTexts)
    {
        return $"Show {string.Join(", ", entityTexts)} from {originalQuery}";
    }

    private string BuildAggregationSubQuery(string originalQuery, SemanticEntity aggEntity)
    {
        return $"Calculate {aggEntity.Text} from {originalQuery}";
    }

    private string BuildFilterSubQuery(string originalQuery, List<SemanticEntity> filterEntities)
    {
        var conditions = string.Join(" and ", filterEntities.Select(e => e.Text));
        return $"Filter data where {conditions} from {originalQuery}";
    }

    private string BuildAggregationFromFilteredData(string originalQuery, List<SemanticEntity> aggEntities)
    {
        var aggregations = string.Join(", ", aggEntities.Select(e => e.Text));
        return $"Calculate {aggregations} from filtered data";
    }

    private string MapToSQLFunction(string aggregationText)
    {
        return aggregationText.ToLowerInvariant() switch
        {
            "sum" or "total" => "SUM",
            "count" => "COUNT",
            "average" or "avg" => "AVG",
            "maximum" or "max" => "MAX",
            "minimum" or "min" => "MIN",
            _ => "SUM"
        };
    }

    private string ExtractDataRetrievalPart(string query, SemanticAnalysis semanticAnalysis)
    {
        // Extract the basic data retrieval part
        var tableEntities = semanticAnalysis.Entities.Where(e => e.Type == EntityType.Table).ToList();
        if (tableEntities.Any())
        {
            return $"Get data from {string.Join(", ", tableEntities.Select(e => e.Text))}";
        }
        return string.Empty;
    }

    private string ExtractProcessingPart(string query, SemanticAnalysis semanticAnalysis)
    {
        // Extract aggregation and processing logic
        var aggEntities = semanticAnalysis.Entities.Where(e => e.Type == EntityType.Aggregation).ToList();
        if (aggEntities.Any())
        {
            return $"Process data with {string.Join(", ", aggEntities.Select(e => e.Text))}";
        }
        return string.Empty;
    }

    private string ExtractPresentationPart(string query, SemanticAnalysis semanticAnalysis)
    {
        // Extract sorting and presentation logic
        var sortEntities = semanticAnalysis.Entities.Where(e => e.Type == EntityType.Sort).ToList();
        if (sortEntities.Any())
        {
            return $"Present results sorted by {string.Join(", ", sortEntities.Select(e => e.Text))}";
        }
        return string.Empty;
    }

    private bool HasCircularDependencies(QueryDecomposition decomposition)
    {
        // Simple cycle detection
        for (int i = 0; i < decomposition.SubQueries.Count; i++)
        {
            if (decomposition.SubQueries[i].Dependencies.Contains(i))
            {
                return true; // Self-dependency
            }
        }
        return false;
    }

    private void SimplifyDependencies(QueryDecomposition decomposition)
    {
        // Remove circular dependencies by clearing them
        for (int i = 0; i < decomposition.SubQueries.Count; i++)
        {
            decomposition.SubQueries[i].Dependencies = decomposition.SubQueries[i].Dependencies
                .Where(dep => dep != i && dep < decomposition.SubQueries.Count)
                .ToList();
        }
    }
}

/// <summary>
/// Query decomposition result
/// </summary>
public class QueryDecomposition
{
    public string OriginalQuery { get; set; } = string.Empty;
    public DecompositionStrategy Strategy { get; set; }
    public List<SubQuery> SubQueries { get; set; } = new();
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Individual sub-query in decomposition
/// </summary>
public class SubQuery
{
    public string Query { get; set; } = string.Empty;
    public SubQueryType Type { get; set; }
    public int Priority { get; set; }
    public List<int> Dependencies { get; set; } = new(); // Indices of sub-queries this depends on
    public List<string> ExpectedTables { get; set; } = new();
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Decomposition strategies
/// </summary>
public enum DecompositionStrategy
{
    Simple,
    Temporal,
    EntityBased,
    Aggregation,
    FilterFirst,
    Hierarchical
}

/// <summary>
/// Sub-query types
/// </summary>
public enum SubQueryType
{
    Simple,
    Temporal,
    EntityBased,
    Aggregation,
    Filter,
    DataRetrieval,
    Processing,
    Presentation
}
