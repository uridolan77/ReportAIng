using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using BIReportingCopilot.Core.Interfaces.Schema;
using BIReportingCopilot.Core.Interfaces.AI;
using BIReportingCopilot.Core.Models;
using BIReportingCopilot.Infrastructure.Data;
using BIReportingCopilot.Infrastructure.Data.Entities;

namespace BIReportingCopilot.Infrastructure.Schema;

/// <summary>
/// Enhanced semantic layer service that builds upon existing schema management
/// with richer semantic metadata and dynamic contextualization
/// </summary>
public class EnhancedSemanticLayerService : IEnhancedSemanticLayerService
{
    private readonly ILogger<EnhancedSemanticLayerService> _logger;
    private readonly BICopilotContext _context;
    private readonly IVectorSearchService _vectorSearchService;
    private readonly IAIService _aiService;
    private readonly ISemanticCacheService _cacheService;

    public EnhancedSemanticLayerService(
        ILogger<EnhancedSemanticLayerService> logger,
        BICopilotContext context,
        IVectorSearchService vectorSearchService,
        IAIService aiService,
        ISemanticCacheService cacheService)
    {
        _logger = logger;
        _context = context;
        _vectorSearchService = vectorSearchService;
        _aiService = aiService;
        _cacheService = cacheService;
    }

    /// <summary>
    /// Get semantically enriched schema information for a query
    /// </summary>
    public async Task<EnhancedSchemaResult> GetEnhancedSchemaAsync(
        string naturalLanguageQuery, 
        double relevanceThreshold = 0.7,
        int maxTables = 10,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("🧠 Getting enhanced schema for query: {Query}", naturalLanguageQuery);

            // Step 1: Check semantic cache first
            var cacheKey = $"enhanced_schema:{naturalLanguageQuery.GetHashCode()}";
            var cachedResult = await _cacheService.GetAsync<EnhancedSchemaResult>(cacheKey, cancellationToken);
            if (cachedResult != null)
            {
                _logger.LogDebug("✅ Enhanced schema cache hit");
                return cachedResult;
            }

            // Step 2: Analyze query intent and extract business concepts
            var queryAnalysis = await AnalyzeQuerySemantics(naturalLanguageQuery, cancellationToken);

            // Step 3: Find relevant tables using enhanced semantic matching
            var relevantTables = await FindSemanticRelevantTables(queryAnalysis, relevanceThreshold, maxTables, cancellationToken);

            // Step 4: Enrich with business glossary context
            var enrichedTables = await EnrichWithBusinessGlossary(relevantTables, queryAnalysis, cancellationToken);

            // Step 5: Generate LLM-optimized context
            var llmContext = await GenerateLLMContext(enrichedTables, queryAnalysis, cancellationToken);

            var result = new EnhancedSchemaResult
            {
                Query = naturalLanguageQuery,
                QueryAnalysis = queryAnalysis,
                RelevantTables = enrichedTables,
                LLMContext = llmContext,
                GeneratedAt = DateTime.UtcNow,
                ConfidenceScore = CalculateOverallConfidence(enrichedTables)
            };

            // Cache the result
            await _cacheService.SetAsync(cacheKey, result, TimeSpan.FromHours(2), cancellationToken);

            _logger.LogInformation("✅ Enhanced schema generated with {TableCount} tables, confidence: {Confidence:P2}", 
                enrichedTables.Count, result.ConfidenceScore);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error generating enhanced schema for query: {Query}", naturalLanguageQuery);
            throw;
        }
    }

    /// <summary>
    /// Analyze query semantics to understand intent and business concepts
    /// </summary>
    private async Task<QuerySemanticAnalysis> AnalyzeQuerySemantics(string query, CancellationToken cancellationToken)
    {
        _logger.LogDebug("🔍 Analyzing query semantics");

        // Use existing AI service for semantic analysis
        var semanticAnalysis = await _aiService.AnalyzeSemanticContentAsync(query, cancellationToken);
        
        // Extract business terms from the query
        var businessTerms = await ExtractBusinessTerms(query, cancellationToken);
        
        // Classify query intent
        var queryIntent = await ClassifyQueryIntent(query, cancellationToken);

        return new QuerySemanticAnalysis
        {
            Query = query,
            Intent = queryIntent,
            BusinessTerms = businessTerms,
            Keywords = semanticAnalysis.Keywords,
            Entities = semanticAnalysis.Entities,
            ConfidenceScore = semanticAnalysis.ConfidenceScore
        };
    }

    /// <summary>
    /// Find semantically relevant tables using enhanced matching
    /// </summary>
    private async Task<List<EnhancedTableInfo>> FindSemanticRelevantTables(
        QuerySemanticAnalysis analysis, 
        double threshold, 
        int maxTables, 
        CancellationToken cancellationToken)
    {
        _logger.LogDebug("🎯 Finding semantically relevant tables");

        var relevantTables = new List<EnhancedTableInfo>();

        // Get all business table info with enhanced semantic metadata
        var allTables = await _context.BusinessTableInfo
            .Include(t => t.Columns)
            .Where(t => t.IsActive)
            .ToListAsync(cancellationToken);

        foreach (var table in allTables)
        {
            var relevanceScore = await CalculateTableRelevance(table, analysis, cancellationToken);
            
            if (relevanceScore >= threshold)
            {
                var enhancedTable = await CreateEnhancedTableInfo(table, relevanceScore, analysis, cancellationToken);
                relevantTables.Add(enhancedTable);
            }
        }

        // Sort by relevance and take top results
        return relevantTables
            .OrderByDescending(t => t.RelevanceScore)
            .Take(maxTables)
            .ToList();
    }

    /// <summary>
    /// Calculate semantic relevance score for a table
    /// </summary>
    private async Task<double> CalculateTableRelevance(
        BusinessTableInfoEntity table, 
        QuerySemanticAnalysis analysis, 
        CancellationToken cancellationToken)
    {
        var scores = new List<double>();

        // 1. Business purpose similarity
        if (!string.IsNullOrEmpty(table.BusinessPurpose))
        {
            var purposeScore = await _aiService.CalculateSemanticSimilarityAsync(
                analysis.Query, table.BusinessPurpose, cancellationToken);
            scores.Add(purposeScore * 0.3); // 30% weight
        }

        // 2. Semantic description similarity
        if (!string.IsNullOrEmpty(table.SemanticDescription))
        {
            var semanticScore = await _aiService.CalculateSemanticSimilarityAsync(
                analysis.Query, table.SemanticDescription, cancellationToken);
            scores.Add(semanticScore * 0.25); // 25% weight
        }

        // 3. Business terms overlap
        var businessTermsScore = CalculateBusinessTermsOverlap(table, analysis.BusinessTerms);
        scores.Add(businessTermsScore * 0.2); // 20% weight

        // 4. Vector search similarity (if available)
        if (!string.IsNullOrEmpty(table.VectorSearchKeywords))
        {
            var vectorScore = await CalculateVectorSimilarity(table, analysis.Query, cancellationToken);
            scores.Add(vectorScore * 0.15); // 15% weight
        }

        // 5. Usage frequency and importance
        var importanceScore = (double)table.ImportanceScore * (double)table.UsageFrequency;
        scores.Add(importanceScore * 0.1); // 10% weight

        return scores.Any() ? scores.Average() : 0.0;
    }

    /// <summary>
    /// Extract business terms from query using business glossary
    /// </summary>
    private async Task<List<string>> ExtractBusinessTerms(string query, CancellationToken cancellationToken)
    {
        var businessTerms = new List<string>();
        
        var glossaryTerms = await _context.BusinessGlossary
            .Where(g => g.IsActive)
            .ToListAsync(cancellationToken);

        foreach (var term in glossaryTerms)
        {
            // Check if term or its synonyms appear in the query
            if (query.Contains(term.Term, StringComparison.OrdinalIgnoreCase))
            {
                businessTerms.Add(term.Term);
                continue;
            }

            // Check synonyms
            if (!string.IsNullOrEmpty(term.Synonyms))
            {
                try
                {
                    var synonyms = JsonSerializer.Deserialize<List<string>>(term.Synonyms) ?? new List<string>();
                    if (synonyms.Any(s => query.Contains(s, StringComparison.OrdinalIgnoreCase)))
                    {
                        businessTerms.Add(term.Term);
                    }
                }
                catch (JsonException)
                {
                    // Handle malformed JSON gracefully
                }
            }
        }

        return businessTerms.Distinct().ToList();
    }

    /// <summary>
    /// Classify query intent for better contextualization
    /// </summary>
    private async Task<QueryIntent> ClassifyQueryIntent(string query, CancellationToken cancellationToken)
    {
        // Use existing AI service for intent analysis
        var nluResult = await _aiService.AnalyzeIntentAsync(query, cancellationToken);
        
        // Map to our QueryIntent enum
        return nluResult.Intent switch
        {
            "reporting" => QueryIntent.Reporting,
            "analytics" => QueryIntent.Analytics,
            "lookup" => QueryIntent.Lookup,
            "aggregation" => QueryIntent.Aggregation,
            "comparison" => QueryIntent.Comparison,
            "trend" => QueryIntent.TrendAnalysis,
            _ => QueryIntent.General
        };
    }

    /// <summary>
    /// Calculate business terms overlap score
    /// </summary>
    private double CalculateBusinessTermsOverlap(BusinessTableInfoEntity table, List<string> queryBusinessTerms)
    {
        if (!queryBusinessTerms.Any()) return 0.0;

        var tableTerms = new List<string>();
        
        // Extract terms from BusinessGlossaryTerms
        if (!string.IsNullOrEmpty(table.BusinessGlossaryTerms))
        {
            try
            {
                var terms = JsonSerializer.Deserialize<List<string>>(table.BusinessGlossaryTerms) ?? new List<string>();
                tableTerms.AddRange(terms);
            }
            catch (JsonException) { }
        }

        if (!tableTerms.Any()) return 0.0;

        var overlap = queryBusinessTerms.Intersect(tableTerms, StringComparer.OrdinalIgnoreCase).Count();
        return (double)overlap / Math.Max(queryBusinessTerms.Count, tableTerms.Count);
    }

    /// <summary>
    /// Calculate vector similarity using vector search service
    /// </summary>
    private async Task<double> CalculateVectorSimilarity(
        BusinessTableInfoEntity table, 
        string query, 
        CancellationToken cancellationToken)
    {
        try
        {
            // Use vector search keywords as the content to compare against
            var keywords = table.VectorSearchKeywords;
            if (string.IsNullOrEmpty(keywords)) return 0.0;

            return await _aiService.CalculateSemanticSimilarityAsync(query, keywords, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error calculating vector similarity for table {TableName}", table.TableName);
            return 0.0;
        }
    }

    /// <summary>
    /// Create enhanced table info with semantic enrichment
    /// </summary>
    private async Task<EnhancedTableInfo> CreateEnhancedTableInfo(
        BusinessTableInfoEntity table,
        double relevanceScore,
        QuerySemanticAnalysis analysis,
        CancellationToken cancellationToken)
    {
        var enhancedColumns = new List<EnhancedColumnInfo>();

        // Process columns with semantic relevance
        foreach (var column in table.Columns.Where(c => c.IsActive))
        {
            var columnRelevance = await CalculateColumnRelevance(column, analysis, cancellationToken);

            if (columnRelevance > 0.3) // Only include relevant columns
            {
                enhancedColumns.Add(new EnhancedColumnInfo
                {
                    ColumnName = column.ColumnName,
                    BusinessMeaning = column.BusinessMeaning,
                    BusinessContext = column.BusinessContext,
                    SemanticContext = column.SemanticContext,
                    DataType = column.BusinessDataType,
                    RelevanceScore = columnRelevance,
                    NaturalLanguageAliases = ParseJsonArray(column.NaturalLanguageAliases),
                    SemanticSynonyms = ParseJsonArray(column.SemanticSynonyms),
                    BusinessMetrics = ParseJsonArray(column.BusinessMetrics),
                    QueryIntentMapping = ParseJsonObject(column.QueryIntentMapping),
                    IsKeyColumn = column.IsKeyColumn,
                    IsSensitiveData = column.IsSensitiveData
                });
            }
        }

        return new EnhancedTableInfo
        {
            TableName = table.TableName,
            SchemaName = table.SchemaName,
            BusinessPurpose = table.BusinessPurpose,
            BusinessContext = table.BusinessContext,
            SemanticDescription = table.SemanticDescription,
            RelevanceScore = relevanceScore,
            Columns = enhancedColumns,
            BusinessProcesses = ParseJsonArray(table.BusinessProcesses),
            AnalyticalUseCases = ParseJsonArray(table.AnalyticalUseCases),
            ReportingCategories = ParseJsonArray(table.ReportingCategories),
            BusinessGlossaryTerms = ParseJsonArray(table.BusinessGlossaryTerms),
            LLMContextHints = ParseJsonArray(table.LLMContextHints),
            ImportanceScore = table.ImportanceScore,
            SemanticCoverageScore = table.SemanticCoverageScore
        };
    }

    /// <summary>
    /// Calculate semantic relevance score for a column
    /// </summary>
    private async Task<double> CalculateColumnRelevance(
        BusinessColumnInfoEntity column,
        QuerySemanticAnalysis analysis,
        CancellationToken cancellationToken)
    {
        var scores = new List<double>();

        // 1. Business meaning similarity
        if (!string.IsNullOrEmpty(column.BusinessMeaning))
        {
            var meaningScore = await _aiService.CalculateSemanticSimilarityAsync(
                analysis.Query, column.BusinessMeaning, cancellationToken);
            scores.Add(meaningScore * 0.3);
        }

        // 2. Semantic context similarity
        if (!string.IsNullOrEmpty(column.SemanticContext))
        {
            var contextScore = await _aiService.CalculateSemanticSimilarityAsync(
                analysis.Query, column.SemanticContext, cancellationToken);
            scores.Add(contextScore * 0.25);
        }

        // 3. Natural language aliases match
        var aliasScore = CalculateAliasMatch(column.NaturalLanguageAliases, analysis.Keywords);
        scores.Add(aliasScore * 0.2);

        // 4. Business metrics relevance
        var metricsScore = CalculateMetricsRelevance(column.BusinessMetrics, analysis.Intent);
        scores.Add(metricsScore * 0.15);

        // 5. Usage frequency and importance
        var usageScore = (double)column.UsageFrequency * (double)column.SemanticRelevanceScore;
        scores.Add(usageScore * 0.1);

        return scores.Any() ? scores.Average() : 0.0;
    }

    /// <summary>
    /// Enrich tables with business glossary context
    /// </summary>
    private async Task<List<EnhancedTableInfo>> EnrichWithBusinessGlossary(
        List<EnhancedTableInfo> tables,
        QuerySemanticAnalysis analysis,
        CancellationToken cancellationToken)
    {
        _logger.LogDebug("📚 Enriching with business glossary context");

        var relevantGlossaryTerms = await GetRelevantGlossaryTerms(analysis.BusinessTerms, cancellationToken);

        foreach (var table in tables)
        {
            // Add relevant glossary terms to table context
            var tableGlossaryTerms = relevantGlossaryTerms
                .Where(term => table.BusinessGlossaryTerms.Contains(term.Term, StringComparer.OrdinalIgnoreCase))
                .ToList();

            table.GlossaryContext = tableGlossaryTerms.ToDictionary(
                term => term.Term,
                term => new GlossaryTermContext
                {
                    Definition = term.Definition,
                    BusinessContext = term.BusinessContext,
                    Examples = ParseJsonArray(term.Examples),
                    DisambiguationRules = ParseJsonArray(term.DisambiguationRules)
                });
        }

        return tables;
    }

    /// <summary>
    /// Generate LLM-optimized context for the schema
    /// </summary>
    private async Task<LLMOptimizedContext> GenerateLLMContext(
        List<EnhancedTableInfo> tables,
        QuerySemanticAnalysis analysis,
        CancellationToken cancellationToken)
    {
        _logger.LogDebug("🤖 Generating LLM-optimized context");

        var contextBuilder = new LLMContextBuilder();

        // Add query-specific context
        contextBuilder.AddQueryContext(analysis);

        // Add table-specific context with prioritization
        foreach (var table in tables.OrderByDescending(t => t.RelevanceScore))
        {
            contextBuilder.AddTableContext(table, analysis.Intent);
        }

        // Add business glossary context
        contextBuilder.AddGlossaryContext(tables.SelectMany(t => t.GlossaryContext.Values));

        // Generate optimized prompts
        var prompts = await GenerateContextualPrompts(analysis, tables, cancellationToken);

        return new LLMOptimizedContext
        {
            ContextSummary = contextBuilder.BuildSummary(),
            PrioritizedTables = tables.OrderByDescending(t => t.RelevanceScore).ToList(),
            BusinessGlossaryContext = tables.SelectMany(t => t.GlossaryContext).ToDictionary(kvp => kvp.Key, kvp => kvp.Value),
            QuerySpecificHints = prompts.QueryHints,
            SchemaNavigationHints = prompts.NavigationHints,
            OptimizationSuggestions = prompts.OptimizationSuggestions,
            ConfidenceScore = CalculateOverallConfidence(tables)
        };
    }

    /// <summary>
    /// Helper methods for parsing JSON arrays and objects
    /// </summary>
    private List<string> ParseJsonArray(string? json)
    {
        if (string.IsNullOrEmpty(json)) return new List<string>();

        try
        {
            return JsonSerializer.Deserialize<List<string>>(json) ?? new List<string>();
        }
        catch (JsonException)
        {
            return new List<string>();
        }
    }

    private Dictionary<string, object> ParseJsonObject(string? json)
    {
        if (string.IsNullOrEmpty(json)) return new Dictionary<string, object>();

        try
        {
            return JsonSerializer.Deserialize<Dictionary<string, object>>(json) ?? new Dictionary<string, object>();
        }
        catch (JsonException)
        {
            return new Dictionary<string, object>();
        }
    }

    /// <summary>
    /// Calculate overall confidence score
    /// </summary>
    private double CalculateOverallConfidence(List<EnhancedTableInfo> tables)
    {
        if (!tables.Any()) return 0.0;

        var avgRelevance = tables.Average(t => t.RelevanceScore);
        var coverageScore = tables.Average(t => (double)t.SemanticCoverageScore);
        var tableCount = Math.Min(tables.Count / 5.0, 1.0); // Normalize table count

        return (avgRelevance * 0.5 + coverageScore * 0.3 + tableCount * 0.2);
    }

    // Additional helper methods would be implemented here...
    private double CalculateAliasMatch(string aliases, List<string> keywords) => 0.5; // Placeholder
    private double CalculateMetricsRelevance(string metrics, QueryIntent intent) => 0.5; // Placeholder
    private async Task<List<BusinessGlossaryEntity>> GetRelevantGlossaryTerms(List<string> terms, CancellationToken ct) => new(); // Placeholder
    private async Task<ContextualPrompts> GenerateContextualPrompts(QuerySemanticAnalysis analysis, List<EnhancedTableInfo> tables, CancellationToken ct) => new(); // Placeholder
}
