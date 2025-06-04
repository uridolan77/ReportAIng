using Microsoft.Extensions.Logging;
using BIReportingCopilot.Core.Interfaces;
using BIReportingCopilot.Core.Models;
using BIReportingCopilot.Infrastructure.Performance;

namespace BIReportingCopilot.Infrastructure.AI.Enhanced;

/// <summary>
/// Multi-dimensional confidence scorer implementing Enhancement 10
/// Provides comprehensive confidence assessment across multiple dimensions
/// </summary>
public class MultiDimensionalConfidenceScorer
{
    private readonly ILogger _logger;
    private readonly Dictionary<string, float> _weights;

    public MultiDimensionalConfidenceScorer(ILogger logger)
    {
        _logger = logger;
        _weights = new Dictionary<string, float>
        {
            ["model_confidence"] = 0.3f,
            ["schema_alignment"] = 0.25f,
            ["execution_validity"] = 0.25f,
            ["historical_performance"] = 0.2f
        };
    }

    public async Task<OverallConfidenceResult> CalculateOverallConfidenceAsync(
        string naturalLanguageQuery,
        string generatedSQL,
        SemanticAnalysis semanticAnalysis,
        GeneratedQuery generatedQuery)
    {
        try
        {
            var scores = new Dictionary<string, float>();

            // Model confidence (30% weight)
            scores["model_confidence"] = CalculateModelConfidence(semanticAnalysis, generatedQuery);

            // Schema alignment (25% weight)
            scores["schema_alignment"] = await CalculateSchemaAlignmentAsync(generatedSQL);

            // Execution validity (25% weight)
            scores["execution_validity"] = await CalculateExecutionValidityAsync(generatedSQL);

            // Historical performance (20% weight)
            scores["historical_performance"] = await CalculateHistoricalPerformanceAsync(naturalLanguageQuery);

            // Calculate weighted overall confidence
            var overallConfidence = scores.Sum(kvp => kvp.Value * _weights[kvp.Key]);

            var result = new OverallConfidenceResult
            {
                OverallConfidence = overallConfidence,
                ComponentScores = scores.ToDictionary(kvp => kvp.Key, kvp => (double)kvp.Value),
                Recommendation = GetRecommendation(overallConfidence),
                ConfidenceLevel = GetConfidenceLevel(overallConfidence)
            };

            _logger.LogDebug("Multi-dimensional confidence calculated: {Confidence:P2}", overallConfidence);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating multi-dimensional confidence");
            return new OverallConfidenceResult
            {
                OverallConfidence = 0.5,
                ComponentScores = new Dictionary<string, double>(),
                Recommendation = "Unable to assess confidence due to error",
                ConfidenceLevel = ConfidenceLevel.Medium
            };
        }
    }

    private float CalculateModelConfidence(SemanticAnalysis semanticAnalysis, GeneratedQuery generatedQuery)
    {
        // Combine semantic analysis confidence with SQL generation confidence
        var semanticWeight = 0.6f;
        var sqlWeight = 0.4f;

        return (float)(semanticAnalysis.ConfidenceScore * semanticWeight +
                      generatedQuery.ConfidenceScore * sqlWeight);
    }

    private async Task<float> CalculateSchemaAlignmentAsync(string sql)
    {
        // Analyze how well the SQL aligns with schema
        var score = 0.5f; // Base score

        // Check for proper table references
        if (ContainsValidTableReferences(sql))
        {
            score += 0.3f;
        }

        // Check for proper column references
        if (ContainsValidColumnReferences(sql))
        {
            score += 0.2f;
        }

        return Math.Min(1.0f, score);
    }

    private async Task<float> CalculateExecutionValidityAsync(string sql)
    {
        // Basic SQL syntax and execution validity checks
        var score = 0.5f; // Base score

        // Check for basic SQL structure
        if (HasValidSQLStructure(sql))
        {
            score += 0.3f;
        }

        // Check for potential execution issues
        if (!HasPotentialExecutionIssues(sql))
        {
            score += 0.2f;
        }

        return Math.Min(1.0f, score);
    }

    private async Task<float> CalculateHistoricalPerformanceAsync(string query)
    {
        // Placeholder for historical performance analysis
        // Would analyze similar queries' success rates
        return 0.7f; // Default good performance
    }

    private string GetRecommendation(float confidence)
    {
        return confidence switch
        {
            >= 0.8f => "High confidence - Query ready for execution",
            >= 0.6f => "Medium confidence - Review recommended",
            >= 0.4f => "Low confidence - Manual review required",
            _ => "Very low confidence - Consider rephrasing query"
        };
    }

    private ConfidenceLevel GetConfidenceLevel(float confidence)
    {
        return confidence switch
        {
            >= 0.8f => ConfidenceLevel.High,
            >= 0.6f => ConfidenceLevel.Medium,
            >= 0.4f => ConfidenceLevel.Low,
            _ => ConfidenceLevel.VeryLow
        };
    }

    private bool ContainsValidTableReferences(string sql)
    {
        // Check if SQL contains valid table references
        var commonTables = new[] { "tbl_daily_actions", "tbl_countries", "tbl_currencies", "tbl_daily_actions_players" };
        return commonTables.Any(table => sql.Contains(table, StringComparison.OrdinalIgnoreCase));
    }

    private bool ContainsValidColumnReferences(string sql)
    {
        // Check if SQL contains valid column references
        var commonColumns = new[] { "playerid", "countryid", "currencyid", "totaldeposits", "totalrevenue" };
        return commonColumns.Any(column => sql.Contains(column, StringComparison.OrdinalIgnoreCase));
    }

    private bool HasValidSQLStructure(string sql)
    {
        var sqlLower = sql.ToLowerInvariant();
        return sqlLower.Contains("select") && !sql.StartsWith("--");
    }

    private bool HasPotentialExecutionIssues(string sql)
    {
        // Check for common execution issues
        var issues = new[]
        {
            "select *", // Avoid SELECT *
            "1=1", // Suspicious conditions
            "drop", "delete", "update", "insert" // Non-SELECT operations
        };

        return issues.Any(issue => sql.Contains(issue, StringComparison.OrdinalIgnoreCase));
    }
}

/// <summary>
/// Overall confidence assessment result
/// </summary>
public class OverallConfidenceResult
{
    public double OverallConfidence { get; set; }
    public Dictionary<string, double> ComponentScores { get; set; } = new();
    public string Recommendation { get; set; } = string.Empty;
    public ConfidenceLevel ConfidenceLevel { get; set; }
}

/// <summary>
/// Confidence levels
/// </summary>
public enum ConfidenceLevel
{
    VeryLow,
    Low,
    Medium,
    High
}

/// <summary>
/// Enhanced query processor implementing both Enhancement 6 and 8
/// Combines context-aware query classification with schema-aware SQL generation
/// </summary>
public class EnhancedQueryProcessorV2 : IQueryProcessor
{
    private readonly ILogger<EnhancedQueryProcessorV2> _logger;
    private readonly ContextAwareSemanticAnalyzer _semanticAnalyzer;
    private readonly SchemaAwareSQLGenerator _sqlGenerator;
    private readonly ICacheService _cacheService;
    private readonly ISchemaService _schemaService;
    private readonly MultiDimensionalConfidenceScorer _confidenceScorer;

    public EnhancedQueryProcessorV2(
        ILogger<EnhancedQueryProcessorV2> logger,
        ICacheService cacheService,
        ISchemaService schemaService,
        IAIService aiService,
        IContextManager contextManager)
    {
        _logger = logger;
        _cacheService = cacheService;
        _schemaService = schemaService;

        // Initialize enhanced components
        _semanticAnalyzer = new ContextAwareSemanticAnalyzer(
            logger, cacheService, contextManager, aiService);
        _sqlGenerator = new SchemaAwareSQLGenerator(
            logger, aiService, schemaService, cacheService);
        _confidenceScorer = new MultiDimensionalConfidenceScorer(logger);
    }

    /// <summary>
    /// Process query with enhanced context awareness and schema-aware SQL generation
    /// </summary>
    public async Task<ProcessedQuery> ProcessQueryAsync(string naturalLanguageQuery, string userId)
    {
        try
        {
            _logger.LogInformation("Processing enhanced query v2 for user {UserId}: {Query}",
                userId, naturalLanguageQuery);

            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            // Step 1: Enhanced Semantic Analysis with Context
            var semanticAnalysis = await _semanticAnalyzer.AnalyzeWithContextAsync(
                naturalLanguageQuery, userId, GenerateSessionId(userId));

            _logger.LogDebug("Enhanced semantic analysis completed in {ElapsedMs}ms with confidence: {Confidence}",
                stopwatch.ElapsedMilliseconds, semanticAnalysis.ConfidenceScore);

            // Step 2: Schema-Aware SQL Generation with Decomposition
            var generatedQuery = await _sqlGenerator.GenerateAsync(
                naturalLanguageQuery, semanticAnalysis, userId);

            _logger.LogDebug("Schema-aware SQL generation completed in {ElapsedMs}ms with confidence: {Confidence}",
                stopwatch.ElapsedMilliseconds, generatedQuery.ConfidenceScore);

            // Step 3: Multi-Dimensional Confidence Assessment
            var overallConfidence = await _confidenceScorer.CalculateOverallConfidenceAsync(
                naturalLanguageQuery, generatedQuery.SQL, semanticAnalysis, generatedQuery);

            _logger.LogDebug("Multi-dimensional confidence assessment completed: {Confidence}",
                overallConfidence.OverallConfidence);

            // Step 4: Create Enhanced Processed Query Result
            var processedQuery = new ProcessedQuery
            {
                OriginalQuery = naturalLanguageQuery,
                Sql = generatedQuery.SQL,
                Explanation = BuildEnhancedExplanation(semanticAnalysis, generatedQuery, overallConfidence),
                Confidence = overallConfidence.OverallConfidence,
                AlternativeQueries = generatedQuery.Alternatives,
                SemanticEntities = semanticAnalysis.Entities,
                Classification = CreateQueryClassification(semanticAnalysis),
                UsedSchema = await GetUsedSchemaAsync(generatedQuery),
                Metadata = CreateEnhancedMetadata(semanticAnalysis, generatedQuery, overallConfidence, stopwatch.ElapsedMilliseconds)
            };

            // Step 5: Cache the enhanced result
            await CacheEnhancedProcessedQueryAsync(naturalLanguageQuery, userId, processedQuery);

            _logger.LogInformation("Enhanced query processing completed in {ElapsedMs}ms with overall confidence: {Confidence}",
                stopwatch.ElapsedMilliseconds, processedQuery.Confidence);

            return processedQuery;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in enhanced query processing v2");
            return await CreateFallbackProcessedQuery(naturalLanguageQuery, userId);
        }
    }

    /// <summary>
    /// Generate enhanced query suggestions with context awareness
    /// </summary>
    public async Task<List<string>> GenerateQuerySuggestionsAsync(string context, string userId)
    {
        try
        {
            _logger.LogDebug("Generating enhanced query suggestions for user {UserId} with context: {Context}",
                userId, context);

            var suggestions = new List<string>();

            // Get conversation context for personalized suggestions
            var conversationContext = await GetConversationContextAsync(userId);

            // Get schema metadata for domain-specific suggestions
            var schema = await _schemaService.GetSchemaMetadataAsync();

            // Generate context-aware suggestions
            if (!string.IsNullOrEmpty(context))
            {
                var contextAnalysis = await _semanticAnalyzer.AnalyzeWithContextAsync(context, userId);
                suggestions.AddRange(await GenerateContextualSuggestions(contextAnalysis, conversationContext, schema));
            }

            // Add conversation-based suggestions
            suggestions.AddRange(await GenerateConversationBasedSuggestions(conversationContext, schema));

            // Add domain-specific suggestions
            suggestions.AddRange(await GenerateDomainSpecificSuggestions(userId, schema));

            // Remove duplicates and rank by relevance
            var rankedSuggestions = suggestions
                .Distinct()
                .Take(8)
                .ToList();

            _logger.LogDebug("Generated {SuggestionCount} enhanced query suggestions", rankedSuggestions.Count);
            return rankedSuggestions;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating enhanced query suggestions");
            return await GenerateFallbackSuggestions();
        }
    }

    /// <summary>
    /// Calculate semantic similarity with enhanced context awareness
    /// </summary>
    public async Task<double> CalculateSemanticSimilarityAsync(string query1, string query2)
    {
        try
        {
            // Analyze both queries
            var analysis1 = await _semanticAnalyzer.AnalyzeAsync(query1);
            var analysis2 = await _semanticAnalyzer.AnalyzeAsync(query2);

            // Calculate multi-dimensional similarity
            var similarity = await CalculateEnhancedSimilarityAsync(analysis1, analysis2);

            _logger.LogDebug("Enhanced semantic similarity calculated: {Similarity} for queries: '{Query1}' vs '{Query2}'",
                similarity, query1, query2);

            return similarity;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating enhanced semantic similarity");
            return 0.0;
        }
    }

    /// <summary>
    /// Find similar queries with enhanced context and semantic understanding
    /// </summary>
    public async Task<List<ProcessedQuery>> FindSimilarQueriesAsync(string query, string userId, int limit = 5)
    {
        try
        {
            _logger.LogDebug("Finding enhanced similar queries for user {UserId}: {Query}", userId, query);

            var similarQueries = new List<ProcessedQuery>();

            // Get conversation context for similarity search
            var conversationContext = await GetConversationContextAsync(userId);

            // Search in conversation history first
            var conversationSimilar = await FindSimilarInConversationAsync(query, conversationContext, limit);
            similarQueries.AddRange(conversationSimilar);

            // Search in cached queries if we need more results
            if (similarQueries.Count < limit)
            {
                var cachedSimilar = await FindSimilarInCacheAsync(query, userId, limit - similarQueries.Count);
                similarQueries.AddRange(cachedSimilar);
            }

            _logger.LogDebug("Found {SimilarCount} enhanced similar queries", similarQueries.Count);
            return similarQueries.Take(limit).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error finding enhanced similar queries");
            return new List<ProcessedQuery>();
        }
    }

    // Helper methods for enhanced processing

    private string BuildEnhancedExplanation(
        SemanticAnalysis semanticAnalysis,
        GeneratedQuery generatedQuery,
        OverallConfidenceResult confidenceResult)
    {
        var explanation = $"Enhanced Query Analysis:\n\n";

        explanation += $"Intent: {semanticAnalysis.Intent}\n";
        explanation += $"Entities Found: {semanticAnalysis.Entities.Count}\n";
        explanation += $"Keywords: {string.Join(", ", semanticAnalysis.Keywords.Take(5))}\n\n";

        explanation += $"SQL Generation:\n";
        explanation += $"{generatedQuery.Explanation}\n\n";

        explanation += $"Confidence Assessment:\n";
        explanation += $"Overall Confidence: {confidenceResult.OverallConfidence:P1}\n";
        explanation += $"Recommendation: {confidenceResult.Recommendation}\n";

        if (confidenceResult.ComponentScores.Any())
        {
            explanation += "\nConfidence Breakdown:\n";
            foreach (var (component, score) in confidenceResult.ComponentScores)
            {
                explanation += $"- {component}: {score:P1}\n";
            }
        }

        return explanation;
    }

    private QueryClassification CreateQueryClassification(SemanticAnalysis semanticAnalysis)
    {
        return new QueryClassification
        {
            Category = MapIntentToCategory(semanticAnalysis.Intent),
            Complexity = DetermineComplexity(semanticAnalysis),
            RequiredJoins = EstimateJoins(semanticAnalysis),
            PredictedTables = PredictTables(semanticAnalysis),
            EstimatedExecutionTime = EstimateExecutionTime(semanticAnalysis),
            RecommendedVisualization = RecommendVisualization(semanticAnalysis),
            ConfidenceScore = semanticAnalysis.ConfidenceScore,
            OptimizationSuggestions = GenerateOptimizationSuggestions(semanticAnalysis)
        };
    }

    private async Task<SchemaMetadata> GetUsedSchemaAsync(GeneratedQuery generatedQuery)
    {
        var fullSchema = await _schemaService.GetSchemaMetadataAsync();

        // Extract tables mentioned in the generated SQL
        var usedTableNames = ExtractTableNamesFromSQL(generatedQuery.SQL, fullSchema);

        var usedTables = fullSchema.Tables
            .Where(t => usedTableNames.Contains(t.Name, StringComparer.OrdinalIgnoreCase))
            .ToList();

        return new SchemaMetadata
        {
            Tables = usedTables,
            DatabaseName = fullSchema.DatabaseName,
            Version = fullSchema.Version,
            LastUpdated = fullSchema.LastUpdated
        };
    }

    private Dictionary<string, object> CreateEnhancedMetadata(
        SemanticAnalysis semanticAnalysis,
        GeneratedQuery generatedQuery,
        OverallConfidenceResult confidenceResult,
        long processingTimeMs)
    {
        var metadata = new Dictionary<string, object>
        {
            ["processing_time_ms"] = processingTimeMs,
            ["enhancement_version"] = "v2",
            ["semantic_analysis_confidence"] = semanticAnalysis.ConfidenceScore,
            ["sql_generation_confidence"] = generatedQuery.ConfidenceScore,
            ["overall_confidence"] = confidenceResult.OverallConfidence,
            ["entity_count"] = semanticAnalysis.Entities.Count,
            ["keyword_count"] = semanticAnalysis.Keywords.Count,
            ["has_conversation_context"] = semanticAnalysis.Metadata.ContainsKey("has_conversation_context"),
            ["decomposition_strategy"] = generatedQuery.Metadata.GetValueOrDefault("decomposition_strategy", "none"),
            ["sub_query_count"] = generatedQuery.Metadata.GetValueOrDefault("sub_query_count", 0),
            ["confidence_components"] = confidenceResult.ComponentScores
        };

        // Add semantic analysis metadata
        foreach (var (key, value) in semanticAnalysis.Metadata)
        {
            metadata[$"semantic_{key}"] = value;
        }

        // Add SQL generation metadata
        foreach (var (key, value) in generatedQuery.Metadata)
        {
            metadata[$"sql_{key}"] = value;
        }

        return metadata;
    }

    private async Task CacheEnhancedProcessedQueryAsync(string query, string userId, ProcessedQuery processedQuery)
    {
        try
        {
            var cacheKey = $"enhanced_processed_query:{userId}:{query.GetHashCode()}";
            await _cacheService.SetAsync(cacheKey, processedQuery, TimeSpan.FromHours(2));

            _logger.LogDebug("Cached enhanced processed query with key: {CacheKey}", cacheKey);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error caching enhanced processed query");
        }
    }

    private async Task<ProcessedQuery> CreateFallbackProcessedQuery(string query, string userId)
    {
        return new ProcessedQuery
        {
            OriginalQuery = query,
            Sql = $"-- Unable to process query: {query}",
            Explanation = "Enhanced processing failed, using fallback",
            Confidence = 0.1,
            AlternativeQueries = new List<string>(),
            SemanticEntities = new List<SemanticEntity>(),
            Classification = new QueryClassification(),
            UsedSchema = new SchemaMetadata(),
            Metadata = new Dictionary<string, object>
            {
                ["fallback"] = true,
                ["enhancement_version"] = "v2",
                ["processing_timestamp"] = DateTime.UtcNow
            }
        };
    }

    private string GenerateSessionId(string userId)
    {
        // Simple session ID generation - could be more sophisticated
        return $"{userId}_{DateTime.UtcNow:yyyyMMdd}";
    }

    private async Task<ConversationContext> GetConversationContextAsync(string userId)
    {
        // This would integrate with the ConversationContextManager
        // For now, return empty context
        return new ConversationContext
        {
            UserId = userId,
            SessionId = GenerateSessionId(userId),
            StartTime = DateTime.UtcNow,
            PreviousQueries = new List<QueryContextEntry>(),
            UserPatterns = new Dictionary<string, object>(),
            SessionMetadata = new Dictionary<string, object>()
        };
    }

    private async Task<List<string>> GenerateContextualSuggestions(
        SemanticAnalysis analysis,
        ConversationContext context,
        SchemaMetadata schema)
    {
        var suggestions = new List<string>();

        // Generate suggestions based on entities found
        foreach (var entity in analysis.Entities.Take(3))
        {
            if (entity.Type == EntityType.Table)
            {
                suggestions.Add($"Show me more details about {entity.Text}");
                suggestions.Add($"What are the trends for {entity.Text}?");
            }
            else if (entity.Type == EntityType.Aggregation)
            {
                suggestions.Add($"Compare {entity.Text} across different periods");
            }
        }

        return suggestions;
    }

    private async Task<List<string>> GenerateConversationBasedSuggestions(
        ConversationContext context,
        SchemaMetadata schema)
    {
        var suggestions = new List<string>();

        // Generate suggestions based on conversation patterns
        if (context.UserPatterns.ContainsKey("common_entities"))
        {
            var commonEntities = context.UserPatterns["common_entities"] as Dictionary<string, int>;
            if (commonEntities != null)
            {
                foreach (var entity in commonEntities.Take(2))
                {
                    suggestions.Add($"Show me recent activity for {entity.Key}");
                }
            }
        }

        return suggestions;
    }

    private async Task<List<string>> GenerateDomainSpecificSuggestions(string userId, SchemaMetadata schema)
    {
        // Gaming domain specific suggestions
        return new List<string>
        {
            "Show me yesterday's revenue by country",
            "Top 10 players by deposits this week",
            "Compare casino vs sports betting revenue",
            "Show me player retention trends"
        };
    }

    private async Task<List<string>> GenerateFallbackSuggestions()
    {
        return new List<string>
        {
            "Show me total revenue for yesterday",
            "Count of active players",
            "Top countries by player count",
            "Recent deposit trends"
        };
    }

    private async Task<double> CalculateEnhancedSimilarityAsync(
        SemanticAnalysis analysis1,
        SemanticAnalysis analysis2)
    {
        var similarity = 0.0;

        // Intent similarity (30% weight)
        if (analysis1.Intent == analysis2.Intent)
        {
            similarity += 0.3;
        }

        // Entity similarity (40% weight)
        var commonEntities = analysis1.Entities
            .Select(e => e.Text.ToLowerInvariant())
            .Intersect(analysis2.Entities.Select(e => e.Text.ToLowerInvariant()))
            .Count();

        var totalEntities = analysis1.Entities.Count + analysis2.Entities.Count;
        if (totalEntities > 0)
        {
            similarity += (double)commonEntities / totalEntities * 0.4;
        }

        // Keyword similarity (30% weight)
        var commonKeywords = analysis1.Keywords
            .Intersect(analysis2.Keywords, StringComparer.OrdinalIgnoreCase)
            .Count();

        var totalKeywords = analysis1.Keywords.Count + analysis2.Keywords.Count;
        if (totalKeywords > 0)
        {
            similarity += (double)commonKeywords / totalKeywords * 0.3;
        }

        return Math.Min(1.0, similarity);
    }

    private async Task<List<ProcessedQuery>> FindSimilarInConversationAsync(
        string query,
        ConversationContext context,
        int limit)
    {
        // This would search in conversation history
        // For now, return empty list
        return new List<ProcessedQuery>();
    }

    private async Task<List<ProcessedQuery>> FindSimilarInCacheAsync(string query, string userId, int limit)
    {
        // This would search in cached queries
        // For now, return empty list
        return new List<ProcessedQuery>();
    }

    // Mapping and utility methods
    private QueryCategory MapIntentToCategory(QueryIntent intent)
    {
        return intent switch
        {
            QueryIntent.Aggregation => QueryCategory.Aggregation,
            QueryIntent.Trend => QueryCategory.Trend,
            QueryIntent.Comparison => QueryCategory.Comparison,
            QueryIntent.Filtering => QueryCategory.Filtering,
            _ => QueryCategory.Reporting
        };
    }

    private QueryComplexity DetermineComplexity(SemanticAnalysis analysis)
    {
        var score = analysis.Entities.Count + analysis.Keywords.Count;
        return score switch
        {
            <= 5 => QueryComplexity.Low,
            <= 10 => QueryComplexity.Medium,
            _ => QueryComplexity.High
        };
    }

    private List<string> EstimateJoins(SemanticAnalysis analysis)
    {
        return analysis.Entities
            .Where(e => e.Type == EntityType.Table)
            .Select(e => e.Text)
            .ToList();
    }

    private List<string> PredictTables(SemanticAnalysis analysis)
    {
        return analysis.Entities
            .Where(e => e.Type == EntityType.Table)
            .Select(e => e.Text)
            .ToList();
    }

    private TimeSpan EstimateExecutionTime(SemanticAnalysis analysis)
    {
        var complexity = DetermineComplexity(analysis);
        return complexity switch
        {
            QueryComplexity.Low => TimeSpan.FromSeconds(1),
            QueryComplexity.Medium => TimeSpan.FromSeconds(3),
            QueryComplexity.High => TimeSpan.FromSeconds(10),
            _ => TimeSpan.FromSeconds(5)
        };
    }

    private VisualizationType RecommendVisualization(SemanticAnalysis analysis)
    {
        if (analysis.Entities.Any(e => e.Type == EntityType.Aggregation))
        {
            return VisualizationType.BarChart;
        }
        if (analysis.Entities.Any(e => e.Type == EntityType.DateRange))
        {
            return VisualizationType.LineChart;
        }
        return VisualizationType.Table;
    }

    private List<string> GenerateOptimizationSuggestions(SemanticAnalysis analysis)
    {
        var suggestions = new List<string>();

        if (analysis.Entities.Count > 5)
        {
            suggestions.Add("Consider breaking down this complex query");
        }

        if (!analysis.Entities.Any(e => e.Type == EntityType.DateRange))
        {
            suggestions.Add("Consider adding time constraints for better performance");
        }

        return suggestions;
    }

    private List<string> ExtractTableNamesFromSQL(string sql, SchemaMetadata schema)
    {
        var tableNames = new List<string>();
        var sqlLower = sql.ToLowerInvariant();

        foreach (var table in schema.Tables)
        {
            if (sqlLower.Contains(table.Name.ToLowerInvariant()))
            {
                tableNames.Add(table.Name);
            }
        }

        return tableNames;
    }
}
