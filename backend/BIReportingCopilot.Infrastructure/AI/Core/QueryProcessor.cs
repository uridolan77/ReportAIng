using Microsoft.Extensions.Logging;
using BIReportingCopilot.Core.Interfaces;
using BIReportingCopilot.Core.Interfaces.AI;
using BIReportingCopilot.Core.Interfaces.Query;
using BIReportingCopilot.Core.Models;
using BIReportingCopilot.Core.Models.QuerySuggestions;
using IContextManager = BIReportingCopilot.Core.Interfaces.IContextManager;
using IQueryOptimizer = BIReportingCopilot.Core.Interfaces.IQueryOptimizer;
using IQueryClassifier = BIReportingCopilot.Core.Interfaces.IQueryClassifier;

namespace BIReportingCopilot.Infrastructure.AI.Core;

/// <summary>
/// Query processor with semantic analysis, context management, and intelligent optimization
/// </summary>
public class QueryProcessor : IQueryProcessor
{
    private readonly ISemanticAnalyzer _semanticAnalyzer;
    private readonly IContextManager _contextManager;
    private readonly IQueryOptimizer _queryOptimizer;
    private readonly IQueryClassifier _queryClassifier;
    private readonly IAIService _aiService;
    private readonly ISchemaService _schemaService;
    private readonly ILogger<QueryProcessor> _logger;
    private readonly ICacheService _cacheService;

    public QueryProcessor(
        ISemanticAnalyzer semanticAnalyzer,
        IContextManager contextManager,
        IQueryOptimizer queryOptimizer,
        IQueryClassifier queryClassifier,
        IAIService aiService,
        ISchemaService schemaService,
        ILogger<QueryProcessor> logger,
        ICacheService cacheService)
    {
        _semanticAnalyzer = semanticAnalyzer;
        _contextManager = contextManager;
        _queryOptimizer = queryOptimizer;
        _queryClassifier = queryClassifier;
        _aiService = aiService;
        _schemaService = schemaService;
        _logger = logger;
        _cacheService = cacheService;
    }

    public async Task<ProcessedQuery> ProcessQueryAsync(string naturalLanguageQuery, string userId)
    {
        try
        {
            _logger.LogInformation("Processing query for user {UserId}: {Query}", userId, naturalLanguageQuery);

            // Step 1: Semantic Analysis
            var semanticAnalysis = await _semanticAnalyzer.AnalyzeAsync(naturalLanguageQuery);
            _logger.LogDebug("Semantic analysis completed with confidence: {Confidence}", semanticAnalysis.ConfidenceScore);

            // Step 2: Get User Context
            var userContext = await _contextManager.GetUserContextAsync(userId);
            _logger.LogDebug("Retrieved user context for domain: {Domain}", userContext.Domain);

            // Step 3: Get Schema Context
            var fullSchema = await _schemaService.GetSchemaMetadataAsync();
            var schemaContext = await _contextManager.GetRelevantSchemaAsync(naturalLanguageQuery, fullSchema);
            _logger.LogDebug("Identified {TableCount} relevant tables", schemaContext.RelevantTables.Count);

            // Step 4: Query Classification
            var classification = await _queryClassifier.ClassifyQueryAsync(naturalLanguageQuery);
            _logger.LogDebug("Query classified as {Category} with {Complexity} complexity",
                classification.Category, classification.Complexity);

            // Step 5: Schema-aware query decomposition
            var decomposition = await DecomposeQueryWithSchemaAsync(naturalLanguageQuery, schemaContext, classification);
            _logger.LogDebug("Query decomposed into {ComponentCount} components", decomposition.Components.Count);

            // Step 6: Generate SQL Candidates with decomposition
            // Convert SemanticAnalysisResult to SemanticAnalysis for compatibility
            var semanticAnalysisForOptimizer = new SemanticAnalysis
            {
                OriginalQuery = naturalLanguageQuery,
                Entities = semanticAnalysis.Entities,
                Keywords = new List<string>(), // Extract from metadata if needed
                Intent = QueryIntent.General, // Default value
                Confidence = semanticAnalysis.Confidence
            };

            var sqlCandidates = await _queryOptimizer.GenerateCandidatesAsync(semanticAnalysisForOptimizer, schemaContext);
            _logger.LogDebug("Generated {CandidateCount} SQL candidates", sqlCandidates.Count);

            // Step 7: Optimize and Select Best Query
            var optimizedQuery = await _queryOptimizer.OptimizeAsync(sqlCandidates, userContext);
            _logger.LogDebug("Selected optimized query with confidence: {Confidence}", optimizedQuery.ConfidenceScore);

            // Step 7: Create Processed Query Result
            var processedQuery = new ProcessedQuery
            {
                Sql = optimizedQuery.Sql,
                Explanation = optimizedQuery.Explanation,
                Confidence = optimizedQuery.ConfidenceScore,
                AlternativeQueries = optimizedQuery.Alternatives,
                SemanticEntities = semanticAnalysis.Entities,
                Classification = classification,
                UsedSchema = schemaContext
            };

            // Step 8: Cache the result for similar queries
            await CacheProcessedQueryAsync(naturalLanguageQuery, processedQuery);

            _logger.LogInformation("Query processing completed successfully with confidence: {Confidence}",
                processedQuery.Confidence);

            return processedQuery;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing query");
            return await CreateFallbackProcessedQuery(naturalLanguageQuery, userId);
        }
    }

    public async Task<List<string>> GenerateQuerySuggestionsAsync(string context, string userId)
    {
        try
        {
            var userContext = await _contextManager.GetUserContextAsync(userId);
            var schema = await _schemaService.GetSchemaMetadataAsync();

            var suggestions = new List<string>();

            // Add context-based suggestions
            if (!string.IsNullOrEmpty(context))
            {
                var contextAnalysisResult = await _semanticAnalyzer.AnalyzeAsync(context);
                // Convert SemanticAnalysisResult to SemanticAnalysis for compatibility
                var contextAnalysis = new SemanticAnalysis
                {
                    OriginalQuery = context,
                    Entities = contextAnalysisResult.Entities,
                    Keywords = new List<string>(),
                    Intent = QueryIntent.General,
                    Confidence = contextAnalysisResult.Confidence
                };
                suggestions.AddRange(await GenerateContextualSuggestions(contextAnalysis, userContext, schema));
            }

            // Add user pattern-based suggestions
            suggestions.AddRange(await GeneratePatternBasedSuggestions(userContext));

            // Add schema-based suggestions
            suggestions.AddRange(await GenerateSchemaBasedSuggestions(schema, userContext));

            // Remove duplicates and limit results
            return suggestions.Distinct().Take(8).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating query suggestions");
            var today = DateTime.Now;
            var yesterday = today.AddDays(-1).ToString("yyyy-MM-dd");

            return new List<string>
            {
                $"Show me total deposits for yesterday ({yesterday})",
                "Top 10 players by deposits in the last 7 days",
                "Show me daily revenue for the last week"
            };
        }
    }

    public async Task<double> CalculateSemanticSimilarityAsync(string query1, string query2)
    {
        try
        {
            // TODO: Implement CalculateSimilarityAsync in ISemanticAnalyzer
            // var similarity = await _semanticAnalyzer.CalculateSimilarityAsync(query1, query2);
            // return similarity.SimilarityScore;
            return 0.5; // Default similarity score
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating semantic similarity");
            return 0.0;
        }
    }

    public async Task<List<ProcessedQuery>> FindSimilarQueriesAsync(string query, string userId, int limit = 5)
    {
        try
        {
            var userContext = await _contextManager.GetUserContextAsync(userId);
            var similarQueries = new List<ProcessedQuery>();

            // Get user's query patterns
            var patterns = await _contextManager.GetQueryPatternsAsync(userId);

            foreach (var pattern in patterns.Take(limit * 2)) // Get more to filter
            {
                var similarity = await CalculateSemanticSimilarityAsync(query, pattern.Pattern);
                if (similarity > 0.7) // High similarity threshold
                {
                    // Try to get cached processed query
                    var cacheKey = $"processed_query:{pattern.Pattern.GetHashCode()}";
                    var cachedQuery = await _cacheService.GetAsync<ProcessedQuery>(cacheKey);

                    if (cachedQuery != null)
                    {
                        similarQueries.Add(cachedQuery);
                    }
                }
            }

            return similarQueries.Take(limit).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error finding similar queries");
            return new List<ProcessedQuery>();
        }
    }

    /// <summary>
    /// Decompose complex queries into manageable components with schema awareness
    /// </summary>
    private async Task<QueryDecomposition> DecomposeQueryWithSchemaAsync(
        string naturalLanguageQuery,
        SchemaContext schemaContext,
        QueryClassification classification)
    {
        try
        {
            var decomposition = new QueryDecomposition
            {
                OriginalQuery = naturalLanguageQuery,
                Components = new List<QueryComponent>(),
                ExecutionOrder = new List<int>(),
                Dependencies = new Dictionary<int, List<int>>()
            };

            // Analyze query complexity and break down if needed
            if (classification.Complexity == QueryComplexity.High ||
                classification.RequiredJoins.Count > 2)
            {
                // Complex query - decompose into sub-queries
                decomposition.Components.AddRange(await DecomposeComplexQueryAsync(naturalLanguageQuery, schemaContext));
            }
            else if (classification.Category == QueryCategory.Aggregation)
            {
                // Aggregation query - decompose by aggregation functions
                decomposition.Components.AddRange(await DecomposeAggregationQueryAsync(naturalLanguageQuery, schemaContext));
            }
            else
            {
                // Simple query - single component
                decomposition.Components.Add(new QueryComponent
                {
                    Id = 1,
                    Type = QueryComponentType.Primary,
                    Description = "Main query execution",
                    Query = naturalLanguageQuery,
                    RequiredTables = schemaContext.RelevantTables.Select(t => t.Name).ToList(),
                    EstimatedComplexity = classification.Complexity
                });
            }

            // Determine execution order based on dependencies
            decomposition.ExecutionOrder = DetermineExecutionOrder(decomposition.Components);

            _logger.LogDebug("Query decomposed into {ComponentCount} components with execution order: {Order}",
                decomposition.Components.Count, string.Join(" -> ", decomposition.ExecutionOrder));

            return decomposition;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error decomposing query");
            return new QueryDecomposition
            {
                OriginalQuery = naturalLanguageQuery,
                Components = new List<QueryComponent>
                {
                    new QueryComponent
                    {
                        Id = 1,
                        Type = QueryComponentType.Primary,
                        Description = "Fallback single component",
                        Query = naturalLanguageQuery,
                        RequiredTables = schemaContext.RelevantTables.Select(t => t.Name).ToList(),
                        EstimatedComplexity = QueryComplexity.Medium
                    }
                },
                ExecutionOrder = new List<int> { 1 },
                Dependencies = new Dictionary<int, List<int>>()
            };
        }
    }

    /// <summary>
    /// Decompose complex queries with multiple joins or subqueries
    /// </summary>
    private Task<List<QueryComponent>> DecomposeComplexQueryAsync(string query, SchemaContext schemaContext)
    {
        var components = new List<QueryComponent>();

        // Identify main data retrieval component
        components.Add(new QueryComponent
        {
            Id = 1,
            Type = QueryComponentType.DataRetrieval,
            Description = "Primary data retrieval",
            Query = ExtractPrimaryDataQuery(query, schemaContext),
            RequiredTables = GetPrimaryTables(schemaContext),
            EstimatedComplexity = QueryComplexity.Medium
        });

        // Identify join components
        var joinTables = GetJoinTables(schemaContext);
        for (int i = 0; i < joinTables.Count; i++)
        {
            components.Add(new QueryComponent
            {
                Id = i + 2,
                Type = QueryComponentType.Join,
                Description = $"Join with {joinTables[i]}",
                Query = $"Join data with {joinTables[i]}",
                RequiredTables = new List<string> { joinTables[i] },
                EstimatedComplexity = QueryComplexity.Low
            });
        }

        // Identify aggregation component if needed
        if (ContainsAggregation(query))
        {
            components.Add(new QueryComponent
            {
                Id = components.Count + 1,
                Type = QueryComponentType.Aggregation,
                Description = "Aggregate results",
                Query = ExtractAggregationPart(query),
                RequiredTables = new List<string>(),
                EstimatedComplexity = QueryComplexity.Low
            });
        }

        return Task.FromResult(components);
    }

    /// <summary>
    /// Decompose aggregation queries by function type
    /// </summary>
    private Task<List<QueryComponent>> DecomposeAggregationQueryAsync(string query, SchemaContext schemaContext)
    {
        var components = new List<QueryComponent>();

        // Base data component
        components.Add(new QueryComponent
        {
            Id = 1,
            Type = QueryComponentType.DataRetrieval,
            Description = "Retrieve base data for aggregation",
            Query = ExtractBaseDataQuery(query, schemaContext),
            RequiredTables = schemaContext.RelevantTables.Select(t => t.Name).ToList(),
            EstimatedComplexity = QueryComplexity.Low
        });

        // Aggregation component
        components.Add(new QueryComponent
        {
            Id = 2,
            Type = QueryComponentType.Aggregation,
            Description = "Apply aggregation functions",
            Query = ExtractAggregationPart(query),
            RequiredTables = new List<string>(),
            EstimatedComplexity = QueryComplexity.Medium
        });

        return Task.FromResult(components);
    }

    private Task<List<string>> GenerateContextualSuggestions(
        SemanticAnalysis contextAnalysis,
        UserContext userContext,
        SchemaMetadata schema)
    {
        var suggestions = new List<string>();

        // Based on semantic entities found
        foreach (var entity in contextAnalysis.Entities.Take(3))
        {
            switch (entity.Type)
            {
                case EntityType.Table:
                    suggestions.Add($"Show me all data from {entity.Text}");
                    suggestions.Add($"Count records in {entity.Text}");
                    break;
                case EntityType.Aggregation:
                    suggestions.Add($"Calculate {entity.Text} by category");
                    break;
                case EntityType.DateRange:
                    suggestions.Add($"Show trends over {entity.Text}");
                    break;
            }
        }

        // Based on query intent
        switch (contextAnalysis.Intent)
        {
            case QueryIntent.Aggregation:
                suggestions.Add("Group results by category");
                suggestions.Add("Calculate totals and averages");
                break;
            case QueryIntent.Trend:
                suggestions.Add("Show monthly trends");
                suggestions.Add("Compare year-over-year growth");
                break;
            case QueryIntent.Comparison:
                suggestions.Add("Compare top performers");
                suggestions.Add("Show differences between categories");
                break;
        }

        return Task.FromResult(suggestions);
    }

    private Task<List<string>> GeneratePatternBasedSuggestions(UserContext userContext)
    {
        var suggestions = new List<string>();

        // Based on user's preferred tables
        foreach (var table in userContext.PreferredTables.Take(3))
        {
            suggestions.Add($"Show recent data from {table}");
            suggestions.Add($"Analyze {table} performance");
        }

        // Based on user's common filters
        foreach (var filter in userContext.CommonFilters.Take(2))
        {
            suggestions.Add($"Filter data {filter}");
        }

        // Based on user's domain
        switch (userContext.Domain.ToLowerInvariant())
        {
            case "sales":
                suggestions.Add("Show sales performance this month");
                suggestions.Add("Top performing products");
                break;
            case "marketing":
                suggestions.Add("Campaign performance analysis");
                suggestions.Add("Customer acquisition metrics");
                break;
            case "finance":
                suggestions.Add("Revenue breakdown by category");
                suggestions.Add("Cost analysis trends");
                break;
        }

        return Task.FromResult(suggestions);
    }

    private Task<List<string>> GenerateSchemaBasedSuggestions(SchemaMetadata schema, UserContext userContext)
    {
        var suggestions = new List<string>();

        // Based on available tables
        var relevantTables = schema.Tables
            .Where(t => userContext.PreferredTables.Contains(t.Name) ||
                       IsCommonBusinessTable(t.Name))
            .Take(3);

        foreach (var table in relevantTables)
        {
            suggestions.Add($"Show summary of {table.Name}");

            // Find date columns for trend suggestions
            var dateColumns = table.Columns.Where(c =>
                c.DataType.Contains("date", StringComparison.OrdinalIgnoreCase) ||
                c.DataType.Contains("time", StringComparison.OrdinalIgnoreCase));

            if (dateColumns.Any())
            {
                suggestions.Add($"Show {table.Name} trends over time");
            }

            // Find numeric columns for aggregation suggestions
            var numericColumns = table.Columns.Where(c =>
                c.DataType.Contains("int", StringComparison.OrdinalIgnoreCase) ||
                c.DataType.Contains("decimal", StringComparison.OrdinalIgnoreCase) ||
                c.DataType.Contains("float", StringComparison.OrdinalIgnoreCase));

            if (numericColumns.Any())
            {
                var column = numericColumns.First();
                suggestions.Add($"Calculate total {column.Name} from {table.Name}");
            }
        }

        return Task.FromResult(suggestions);
    }

    private bool IsCommonBusinessTable(string tableName)
    {
        var commonTables = new[] { "orders", "customers", "products", "sales", "transactions", "users", "invoices", "payments" };
        return commonTables.Any(ct => tableName.ToLowerInvariant().Contains(ct));
    }

    #region Query Decomposition Helper Methods

    /// <summary>
    /// Determine optimal execution order for query components
    /// </summary>
    private List<int> DetermineExecutionOrder(List<QueryComponent> components)
    {
        // Simple ordering: DataRetrieval -> Join -> Aggregation -> Filtering
        return components
            .OrderBy(c => GetComponentPriority(c.Type))
            .Select(c => c.Id)
            .ToList();
    }

    private int GetComponentPriority(QueryComponentType type)
    {
        return type switch
        {
            QueryComponentType.DataRetrieval => 1,
            QueryComponentType.Join => 2,
            QueryComponentType.Filtering => 3,
            QueryComponentType.Aggregation => 4,
            QueryComponentType.Sorting => 5,
            _ => 6
        };
    }

    private string ExtractPrimaryDataQuery(string query, SchemaContext schemaContext)
    {
        var primaryTable = schemaContext.RelevantTables.FirstOrDefault()?.Name ?? "main_table";
        return $"Retrieve data from {primaryTable}";
    }

    private List<string> GetPrimaryTables(SchemaContext schemaContext)
    {
        return schemaContext.RelevantTables.Take(1).Select(t => t.Name).ToList();
    }

    private List<string> GetJoinTables(SchemaContext schemaContext)
    {
        return schemaContext.RelevantTables.Skip(1).Select(t => t.Name).ToList();
    }

    private bool ContainsAggregation(string query)
    {
        var aggregationKeywords = new[] { "sum", "count", "avg", "max", "min", "total", "average" };
        return aggregationKeywords.Any(k => query.Contains(k, StringComparison.OrdinalIgnoreCase));
    }

    private string ExtractAggregationPart(string query)
    {
        if (query.Contains("sum", StringComparison.OrdinalIgnoreCase))
            return "Calculate sum";
        if (query.Contains("count", StringComparison.OrdinalIgnoreCase))
            return "Count records";
        if (query.Contains("avg", StringComparison.OrdinalIgnoreCase) || query.Contains("average", StringComparison.OrdinalIgnoreCase))
            return "Calculate average";
        if (query.Contains("max", StringComparison.OrdinalIgnoreCase))
            return "Find maximum";
        if (query.Contains("min", StringComparison.OrdinalIgnoreCase))
            return "Find minimum";

        return "Apply aggregation";
    }

    private string ExtractBaseDataQuery(string query, SchemaContext schemaContext)
    {
        var primaryTable = schemaContext.RelevantTables.FirstOrDefault()?.Name ?? "main_table";
        return $"Select base data from {primaryTable}";
    }

    #endregion

    private async Task CacheProcessedQueryAsync(string originalQuery, ProcessedQuery processedQuery)
    {
        try
        {
            var cacheKey = $"processed_query:{originalQuery.GetHashCode()}";
            await _cacheService.SetAsync(cacheKey, processedQuery, TimeSpan.FromHours(2));
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to cache processed query");
        }
    }

    private async Task<ProcessedQuery> CreateFallbackProcessedQuery(string naturalLanguageQuery, string userId)
    {
        try
        {
            // Use the basic OpenAI service as fallback
            var fallbackSql = await _aiService.GenerateSQLAsync(naturalLanguageQuery);
            var fallbackConfidence = await _aiService.CalculateConfidenceScoreAsync(naturalLanguageQuery, fallbackSql);

            return new ProcessedQuery
            {
                Sql = fallbackSql,
                Explanation = "Generated using fallback processing due to error in pipeline",
                Confidence = fallbackConfidence * 0.7, // Reduce confidence for fallback
                AlternativeQueries = new List<SqlCandidate>(),
                SemanticEntities = new List<SemanticEntity>(),
                Classification = new QueryClassification
                {
                    Category = QueryCategory.Lookup,
                    Complexity = QueryComplexity.Medium,
                    ConfidenceScore = 0.5
                },
                UsedSchema = new SchemaContext()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating fallback processed query");
            return new ProcessedQuery
            {
                Sql = "SELECT 'Error processing query' as Message",
                Explanation = "Unable to process query due to system error",
                Confidence = 0.1,
                AlternativeQueries = new List<SqlCandidate>(),
                SemanticEntities = new List<SemanticEntity>(),
                Classification = new QueryClassification(),
                UsedSchema = new SchemaContext()
            };
        }
    }

    #region Missing Interface Method Implementations

    /// <summary>
    /// Process natural language query (IQueryProcessor interface)
    /// </summary>
    public async Task<string> ProcessNaturalLanguageQueryAsync(string naturalLanguageQuery, string? context = null, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("🔍 Processing natural language query: {Query}", naturalLanguageQuery);

            // Get schema context
            var schema = await _schemaService.GetSchemaMetadataAsync();

            // Use AI service to generate SQL
            var sql = await _aiService.GenerateSQLAsync(naturalLanguageQuery, cancellationToken);

            _logger.LogInformation("✅ Generated SQL: {SQL}", sql);
            return sql;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error processing natural language query");
            return "SELECT 'Error processing query' as Message";
        }
    }

    /// <summary>
    /// Process query with analysis (IQueryProcessor interface)
    /// </summary>
    public async Task<QueryProcessingResult> ProcessQueryWithAnalysisAsync(string naturalLanguageQuery, string? context = null, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("🔍 Processing query with analysis: {Query}", naturalLanguageQuery);

            // Semantic analysis
            var semanticAnalysis = await _semanticAnalyzer.AnalyzeAsync(naturalLanguageQuery, cancellationToken);

            // Query classification
            var classification = await _queryClassifier.ClassifyAsync(naturalLanguageQuery, cancellationToken);

            // Generate SQL
            var sql = await _aiService.GenerateSQLAsync(naturalLanguageQuery, cancellationToken);

            // Calculate confidence
            var confidence = await _aiService.CalculateConfidenceScoreAsync(naturalLanguageQuery, sql);

            return new QueryProcessingResult
            {
                GeneratedSQL = sql,
                Confidence = confidence,
                SemanticAnalysis = semanticAnalysis,
                Classification = classification,
                ProcessingTime = TimeSpan.FromMilliseconds(100), // Placeholder
                Suggestions = await GenerateQuerySuggestionsAsync(naturalLanguageQuery, cancellationToken),
                Warnings = ExtractWarnings(sql),
                Metadata = new Dictionary<string, object>
                {
                    ["context"] = context ?? string.Empty,
                    ["timestamp"] = DateTime.UtcNow
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error processing query with analysis");
            return new QueryProcessingResult
            {
                GeneratedSQL = "SELECT 'Error processing query' as Message",
                Confidence = 0.1,
                ProcessingTime = TimeSpan.FromMilliseconds(50),
                Suggestions = new List<QuerySuggestion>(),
                Warnings = new List<string> { "Error occurred during processing" },
                Metadata = new Dictionary<string, object>()
            };
        }
    }

    /// <summary>
    /// Validate query intent (IQueryProcessor interface)
    /// </summary>
    public async Task<bool> ValidateQueryIntentAsync(string query, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("🔍 Validating query intent: {Query}", query);

            // Use AI service to validate intent
            var isValid = await _aiService.ValidateQueryIntentAsync(query);

            // Additional validation using semantic analysis
            var semanticAnalysis = await _semanticAnalyzer.AnalyzeAsync(query, cancellationToken);
            var hasValidIntent = semanticAnalysis.Intent != QueryIntent.Unknown && semanticAnalysis.ConfidenceScore > 0.5;

            var result = isValid && hasValidIntent;
            _logger.LogDebug("✅ Query intent validation result: {IsValid}", result);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error validating query intent");
            return false;
        }
    }

    /// <summary>
    /// Get query suggestions (IQueryProcessor interface)
    /// </summary>
    public async Task<List<QuerySuggestion>> GetQuerySuggestionsAsync(string partialQuery, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("🔍 Getting query suggestions for: {PartialQuery}", partialQuery);

            var suggestions = new List<QuerySuggestion>();

            // Get schema-based suggestions
            var schema = await _schemaService.GetSchemaMetadataAsync();
            var schemaSuggestions = await GenerateSchemaBasedSuggestions(schema, new UserContext { Domain = "general" });

            // Convert to QuerySuggestion objects
            foreach (var suggestion in schemaSuggestions.Take(5))
            {
                suggestions.Add(new QuerySuggestion
                {
                    Id = Guid.NewGuid().ToString(),
                    Text = suggestion,
                    Description = $"Suggested query based on: {partialQuery}",
                    Category = "Schema-based",
                    Confidence = 0.8,
                    UsageCount = 0,
                    CreatedAt = DateTime.UtcNow,
                    Metadata = new Dictionary<string, object>
                    {
                        ["source"] = "schema_analysis",
                        ["partial_query"] = partialQuery
                    }
                });
            }

            // Add contextual suggestions if partial query has content
            if (!string.IsNullOrWhiteSpace(partialQuery))
            {
                var contextualSuggestions = await GenerateContextualSuggestionsFromPartial(partialQuery);
                suggestions.AddRange(contextualSuggestions);
            }

            _logger.LogDebug("✅ Generated {Count} query suggestions", suggestions.Count);
            return suggestions.Take(10).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error getting query suggestions");
            return new List<QuerySuggestion>
            {
                new QuerySuggestion
                {
                    Id = Guid.NewGuid().ToString(),
                    Text = "Show me recent data",
                    Description = "Default suggestion",
                    Category = "Default",
                    Confidence = 0.5,
                    CreatedAt = DateTime.UtcNow
                }
            };
        }
    }

    #endregion

    #region Helper Methods for Interface Implementations

    private List<string> ExtractWarnings(string sql)
    {
        var warnings = new List<string>();

        if (sql.Contains("SELECT *", StringComparison.OrdinalIgnoreCase))
            warnings.Add("Using SELECT * may impact performance");

        if (sql.Contains("DELETE", StringComparison.OrdinalIgnoreCase) && !sql.Contains("WHERE", StringComparison.OrdinalIgnoreCase))
            warnings.Add("DELETE without WHERE clause detected");

        return warnings;
    }

    private async Task<List<QuerySuggestion>> GenerateContextualSuggestionsFromPartial(string partialQuery)
    {
        var suggestions = new List<QuerySuggestion>();

        try
        {
            // Analyze partial query for context
            var lowerPartial = partialQuery.ToLowerInvariant();

            if (lowerPartial.Contains("show") || lowerPartial.Contains("get"))
            {
                suggestions.Add(new QuerySuggestion
                {
                    Id = Guid.NewGuid().ToString(),
                    Text = $"{partialQuery} from last 7 days",
                    Description = "Add time filter",
                    Category = "Time-based",
                    Confidence = 0.7,
                    CreatedAt = DateTime.UtcNow
                });
            }

            if (lowerPartial.Contains("total") || lowerPartial.Contains("sum"))
            {
                suggestions.Add(new QuerySuggestion
                {
                    Id = Guid.NewGuid().ToString(),
                    Text = $"{partialQuery} grouped by category",
                    Description = "Add grouping",
                    Category = "Aggregation",
                    Confidence = 0.7,
                    CreatedAt = DateTime.UtcNow
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error generating contextual suggestions");
        }

        return suggestions;
    }

    #endregion
}
