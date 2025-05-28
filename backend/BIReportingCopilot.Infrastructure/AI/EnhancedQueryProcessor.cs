using Microsoft.Extensions.Logging;
using BIReportingCopilot.Core.Interfaces;
using BIReportingCopilot.Core.Models;

namespace BIReportingCopilot.Infrastructure.AI;

/// <summary>
/// Enhanced query processor with semantic analysis, context management, and intelligent optimization
/// </summary>
public class EnhancedQueryProcessor : IQueryProcessor
{
    private readonly ISemanticAnalyzer _semanticAnalyzer;
    private readonly IContextManager _contextManager;
    private readonly IQueryOptimizer _queryOptimizer;
    private readonly IQueryClassifier _queryClassifier;
    private readonly IOpenAIService _openAIService;
    private readonly ISchemaService _schemaService;
    private readonly ILogger<EnhancedQueryProcessor> _logger;
    private readonly ICacheService _cacheService;

    public EnhancedQueryProcessor(
        ISemanticAnalyzer semanticAnalyzer,
        IContextManager contextManager,
        IQueryOptimizer queryOptimizer,
        IQueryClassifier queryClassifier,
        IOpenAIService openAIService,
        ISchemaService schemaService,
        ILogger<EnhancedQueryProcessor> logger,
        ICacheService cacheService)
    {
        _semanticAnalyzer = semanticAnalyzer;
        _contextManager = contextManager;
        _queryOptimizer = queryOptimizer;
        _queryClassifier = queryClassifier;
        _openAIService = openAIService;
        _schemaService = schemaService;
        _logger = logger;
        _cacheService = cacheService;
    }

    public async Task<ProcessedQuery> ProcessQueryAsync(string naturalLanguageQuery, string userId)
    {
        try
        {
            _logger.LogInformation("Processing enhanced query for user {UserId}: {Query}", userId, naturalLanguageQuery);

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

            // Step 5: Generate SQL Candidates
            var sqlCandidates = await _queryOptimizer.GenerateCandidatesAsync(semanticAnalysis, schemaContext);
            _logger.LogDebug("Generated {CandidateCount} SQL candidates", sqlCandidates.Count);

            // Step 6: Optimize and Select Best Query
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
            _logger.LogError(ex, "Error processing enhanced query");
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
                var contextAnalysis = await _semanticAnalyzer.AnalyzeAsync(context);
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
            return new List<string>
            {
                "Show me the total count of records",
                "What are the top 10 items by value?",
                "Show me data from the last 30 days"
            };
        }
    }

    public async Task<double> CalculateSemanticSimilarityAsync(string query1, string query2)
    {
        try
        {
            var similarity = await _semanticAnalyzer.CalculateSimilarityAsync(query1, query2);
            return similarity.SimilarityScore;
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

    private async Task<List<string>> GenerateContextualSuggestions(
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

        return suggestions;
    }

    private async Task<List<string>> GeneratePatternBasedSuggestions(UserContext userContext)
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

        return suggestions;
    }

    private async Task<List<string>> GenerateSchemaBasedSuggestions(SchemaMetadata schema, UserContext userContext)
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

        return suggestions;
    }

    private bool IsCommonBusinessTable(string tableName)
    {
        var commonTables = new[] { "orders", "customers", "products", "sales", "transactions", "users", "invoices", "payments" };
        return commonTables.Any(ct => tableName.ToLowerInvariant().Contains(ct));
    }

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
            var fallbackSql = await _openAIService.GenerateSQLAsync(naturalLanguageQuery);
            var fallbackConfidence = await _openAIService.CalculateConfidenceScoreAsync(naturalLanguageQuery, fallbackSql);

            return new ProcessedQuery
            {
                Sql = fallbackSql,
                Explanation = "Generated using fallback processing due to error in enhanced pipeline",
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
}

// Interface definition for the enhanced query processor
public interface IQueryProcessor
{
    Task<ProcessedQuery> ProcessQueryAsync(string naturalLanguageQuery, string userId);
    Task<List<string>> GenerateQuerySuggestionsAsync(string context, string userId);
    Task<double> CalculateSemanticSimilarityAsync(string query1, string query2);
    Task<List<ProcessedQuery>> FindSimilarQueriesAsync(string query, string userId, int limit = 5);
}
