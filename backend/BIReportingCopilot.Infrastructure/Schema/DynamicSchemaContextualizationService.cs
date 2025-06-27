using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using BIReportingCopilot.Core.DTOs;
using BIReportingCopilot.Core.Models;
using BIReportingCopilot.Core.Interfaces.AI;
using BIReportingCopilot.Core.Interfaces.Schema;
using BIReportingCopilot.Infrastructure.Data;
using BIReportingCopilot.Infrastructure.Data.Entities;
using QueryAnalysisResult = BIReportingCopilot.Core.Models.QueryAnalysisResult;

namespace BIReportingCopilot.Infrastructure.Schema;

/// <summary>
/// Service for dynamic schema contextualization - provides only relevant schema elements to LLM
/// based on query intent and semantic similarity
/// </summary>
public class DynamicSchemaContextualizationService : IDynamicSchemaContextualizationService
{
    private readonly ILogger<DynamicSchemaContextualizationService> _logger;
    private readonly BICopilotContext _context;
    private readonly IVectorSearchService _vectorSearchService;
    private readonly IAIService _aiService;

    public DynamicSchemaContextualizationService(
        ILogger<DynamicSchemaContextualizationService> logger,
        BICopilotContext context,
        IVectorSearchService vectorSearchService,
        IAIService aiService)
    {
        _logger = logger;
        _context = context;
        _vectorSearchService = vectorSearchService;
        _aiService = aiService;
    }

    /// <summary>
    /// Get contextually relevant schema elements for a given natural language query
    /// </summary>
    public async Task<ContextualizedSchemaResult> GetRelevantSchemaAsync(
        string naturalLanguageQuery, 
        double relevanceThreshold = 0.7,
        int maxTables = 10,
        int maxColumnsPerTable = 15)
    {
        try
        {
            _logger.LogInformation("🔍 Getting relevant schema for query: {Query}", naturalLanguageQuery);

            // Step 1: Analyze query intent and extract business terms
            var queryAnalysis = await AnalyzeQueryIntentAsync(naturalLanguageQuery);

            // Step 2: Find semantically similar previous mappings
            var similarMappings = await FindSimilarMappingsAsync(naturalLanguageQuery, relevanceThreshold);

            // Step 3: Get relevant tables based on business terms and semantic similarity
            var relevantTables = await GetRelevantTablesAsync(queryAnalysis, similarMappings, maxTables);

            // Step 4: Get relevant columns for the selected tables
            var relevantColumns = await GetRelevantColumnsAsync(relevantTables, queryAnalysis, maxColumnsPerTable);

            // Step 5: Enrich with business context
            var enrichedSchema = await EnrichWithBusinessContextAsync(relevantTables, relevantColumns);

            // Step 6: Get relevant business glossary terms
            var relevantGlossaryTerms = await GetRelevantGlossaryTermsAsync(queryAnalysis, relevantTables);

            // Step 7: Store this mapping for future use
            await StoreMappingAsync(naturalLanguageQuery, queryAnalysis, relevantTables, relevantColumns);

            var result = new ContextualizedSchemaResult
            {
                Query = naturalLanguageQuery,
                QueryAnalysis = queryAnalysis,
                RelevantTables = enrichedSchema.Tables,
                RelevantColumns = enrichedSchema.Columns,
                RelevantGlossaryTerms = relevantGlossaryTerms,
                BusinessTermsUsed = queryAnalysis.BusinessTerms,
                ConfidenceScore = CalculateOverallConfidence(relevantTables, relevantColumns),
                TokenEstimate = EstimateTokenUsage(enrichedSchema),
                GeneratedAt = DateTime.UtcNow
            };

            _logger.LogInformation("✅ Found {TableCount} relevant tables and {ColumnCount} columns", 
                result.RelevantTables.Count, result.RelevantColumns.Count);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error getting relevant schema for query: {Query}", naturalLanguageQuery);
            
            // Fallback to basic schema if contextualization fails
            return await GetFallbackSchemaAsync(naturalLanguageQuery);
        }
    }

    /// <summary>
    /// Analyze the natural language query to understand intent and extract business terms
    /// </summary>
    private async Task<QueryAnalysisResult> AnalyzeQueryIntentAsync(string query)
    {
        try
        {
            // Extract business terms from the query by matching against glossary
            var businessTerms = await ExtractBusinessTermsAsync(query);

            // Determine query category (Reporting, Analytics, Lookup, etc.)
            var category = DetermineQueryCategory(query);

            // Extract entities mentioned (table names, column concepts)
            var entities = await ExtractEntitiesAsync(query);

            return new QueryAnalysisResult
            {
                OriginalQuery = query,
                BusinessTerms = businessTerms,
                QueryCategory = category,
                ExtractedEntities = entities,
                Intent = DetermineQueryIntent(query, businessTerms),
                Complexity = AssessQueryComplexityScore(query, businessTerms)
            };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error analyzing query intent, using basic analysis");
            return new QueryAnalysisResult
            {
                OriginalQuery = query,
                QueryCategory = "General",
                Intent = "Unknown",
                Complexity = 0.5
            };
        }
    }

    /// <summary>
    /// Find similar query mappings using vector search
    /// </summary>
    private async Task<List<SemanticSchemaMappingDto>> FindSimilarMappingsAsync(string query, double threshold)
    {
        try
        {
            // Use vector search to find semantically similar queries
            var searchResults = await _vectorSearchService.SearchAsync(query, maxResults: 5, similarityThreshold: threshold);

            var mappingIds = searchResults.Select(r => long.Parse(r.DocumentId)).ToList();

            var mappings = await _context.SemanticSchemaMappings
                .Where(m => mappingIds.Contains(m.Id) && m.IsActive)
                .ToListAsync();

            return mappings.Select(MapToDto).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error finding similar mappings");
            return new List<SemanticSchemaMappingDto>();
        }
    }

    /// <summary>
    /// Get relevant tables based on query analysis and similar mappings
    /// </summary>
    private async Task<List<TableRelevanceDto>> GetRelevantTablesAsync(
        QueryAnalysisResult analysis, 
        List<SemanticSchemaMappingDto> similarMappings, 
        int maxTables)
    {
        var tableRelevance = new Dictionary<string, decimal>();

        // Get all business tables for intelligent analysis
        var businessTables = await _context.BusinessTableInfo
            .Where(t => t.IsActive)
            .ToListAsync();

        _logger.LogInformation("🔍 Analyzing {TableCount} tables for query intent: {Intent}, category: {Category}",
            businessTables.Count, analysis.Intent, analysis.QueryCategory);

        // INTELLIGENT DOMAIN-SPECIFIC SCORING
        foreach (var table in businessTables)
        {
            var key = $"{table.SchemaName}.{table.TableName}";
            decimal score = 0;

            // 1. High-priority scoring for specific query intents
            var tableLower = table.TableName.ToLowerInvariant();
            var purposeLower = table.BusinessPurpose?.ToLowerInvariant() ?? "";
            var originalQueryLower = analysis.OriginalQuery.ToLowerInvariant();

            // SMART INTENT DETECTION: Check both explicit intent and query content
            bool isDepositQuery = analysis.Intent == "TopDepositors" ||
                                (originalQueryLower.Contains("deposit") &&
                                 (originalQueryLower.Contains("top") || originalQueryLower.Contains("sum") || originalQueryLower.Contains("total")));

            if (isDepositQuery)
            {
                _logger.LogDebug("🏦 Applying deposit query scoring for table: {Table}", table.TableName);

                // Prioritize financial/transaction tables for deposit queries
                if (tableLower.Contains("deposit") || tableLower.Contains("transaction") ||
                    tableLower.Contains("payment") || tableLower.Contains("financial") ||
                    purposeLower.Contains("deposit") || purposeLower.Contains("financial"))
                {
                    score += 2.0m; // Very high priority for financial tables
                    _logger.LogDebug("💰 High priority financial table: {Table} (+2.0)", table.TableName);
                }

                // Prioritize daily action tables (likely contain deposit data)
                if (tableLower.Contains("daily") && tableLower.Contains("action"))
                {
                    score += 1.8m; // High priority for daily action tables
                    _logger.LogDebug("📊 Daily action table: {Table} (+1.8)", table.TableName);
                }

                // Prioritize player/user tables for identification
                if (tableLower.Contains("player") || tableLower.Contains("user") ||
                    tableLower.Contains("customer") || tableLower.Contains("account") ||
                    purposeLower.Contains("player") || purposeLower.Contains("user"))
                {
                    score += 1.5m; // High priority for player tables
                    _logger.LogDebug("👤 Player identification table: {Table} (+1.5)", table.TableName);
                }

                // Geographic tables for country filtering
                if (tableLower.Contains("country") || tableLower.Contains("location") ||
                    tableLower.Contains("region") || purposeLower.Contains("geographic"))
                {
                    score += 1.5m; // High priority for geographic filtering
                    _logger.LogDebug("🌍 Geographic table: {Table} (+1.5)", table.TableName);
                }

                // EXCLUDE gaming-specific tables for financial queries
                if (tableLower.Contains("game") && !tableLower.Contains("deposit") &&
                    !tableLower.Contains("transaction") && !tableLower.Contains("financial") &&
                    !tableLower.Contains("action"))
                {
                    score -= 1.0m; // Penalize pure gaming tables for financial queries
                    _logger.LogDebug("🎮 Penalizing pure gaming table: {Table} (-1.0)", table.TableName);
                }
            }

            // 2. Business term matching with weighted scoring
            foreach (var term in analysis.BusinessTerms)
            {
                var termLower = term.ToLowerInvariant();

                // Exact table name match (highest weight)
                if (table.TableName.ToLowerInvariant().Contains(termLower))
                {
                    score += 1.2m;
                }

                // Business purpose match (high weight)
                if (table.BusinessPurpose?.ToLowerInvariant().Contains(termLower) == true)
                {
                    score += 0.8m;
                }

                // Business context match (medium weight)
                if (table.BusinessContext?.ToLowerInvariant().Contains(termLower) == true)
                {
                    score += 0.6m;
                }
            }

            // 3. Domain classification boost
            if (!string.IsNullOrEmpty(analysis.QueryCategory))
            {
                var categoryLower = analysis.QueryCategory.ToLowerInvariant();
                if (table.DomainClassification?.ToLowerInvariant().Contains(categoryLower) == true)
                {
                    score += 0.7m;
                }
            }

            // 4. Table importance and usage frequency
            if (table.ImportanceScore > 0.7m)
            {
                score += 0.3m;
            }

            // Only include tables with meaningful relevance
            if (score > 0.5m)
            {
                tableRelevance[key] = score;
            }
        }

        // 5. Score tables based on business glossary terms
        foreach (var term in analysis.BusinessTerms)
        {
            var glossaryEntry = await _context.BusinessGlossary
                .FirstOrDefaultAsync(g => g.Term.ToLower() == term.ToLower() && g.IsActive);

            if (glossaryEntry != null && !string.IsNullOrEmpty(glossaryEntry.MappedTables))
            {
                var mappedTables = JsonSerializer.Deserialize<List<string>>(glossaryEntry.MappedTables) ?? new List<string>();
                foreach (var table in mappedTables)
                {
                    tableRelevance[table] = tableRelevance.GetValueOrDefault(table, 0) + 0.8m;
                }
            }
        }

        // 6. Boost from similar mappings (but with lower weight to prevent over-reliance)
        foreach (var mapping in similarMappings)
        {
            foreach (var table in mapping.RelevantTables)
            {
                var boost = mapping.ConfidenceScore * 0.4m; // Reduced from 0.6m
                var key = table.TableName;
                if (tableRelevance.ContainsKey(key))
                {
                    tableRelevance[key] += boost;
                }
            }
        }

        // 7. Convert to DTOs and apply intelligent filtering
        var result = tableRelevance
            .Where(kvp => kvp.Value > 0.5m) // Only include tables with meaningful relevance
            .OrderByDescending(kvp => kvp.Value)
            .Take(Math.Min(maxTables, 5)) // Limit to max 5 tables for focused results
            .Select(kvp => new TableRelevanceDto
            {
                TableName = kvp.Key.Contains('.') ? kvp.Key.Split('.')[1] : kvp.Key,
                SchemaName = kvp.Key.Contains('.') ? kvp.Key.Split('.')[0] : "dbo",
                RelevanceScore = kvp.Value,
                ReasonForRelevance = GetRelevanceReason(kvp.Key, analysis)
            })
            .ToList();

        _logger.LogInformation("✅ Selected {TableCount} relevant tables with scores: {Scores}",
            result.Count, string.Join(", ", result.Select(r => $"{r.TableName}:{r.RelevanceScore:F2}")));

        return result;
    }

    /// <summary>
    /// Get relevant columns for the selected tables
    /// </summary>
    private async Task<List<ColumnRelevanceDto>> GetRelevantColumnsAsync(
        List<TableRelevanceDto> tables, 
        QueryAnalysisResult analysis, 
        int maxColumnsPerTable)
    {
        var allColumns = new List<ColumnRelevanceDto>();

        _logger.LogInformation("🔍 Analyzing columns for {TableCount} tables with intent: {Intent}",
            tables.Count, analysis.Intent);

        foreach (var table in tables)
        {
            var tableInfo = await _context.BusinessTableInfo
                .Include(t => t.Columns)
                .FirstOrDefaultAsync(t => t.TableName == table.TableName &&
                                        t.SchemaName == table.SchemaName &&
                                        t.IsActive);

            if (tableInfo?.Columns != null)
            {
                var columnRelevance = new Dictionary<string, decimal>();

                foreach (var column in tableInfo.Columns.Where(c => c.IsActive))
                {
                    decimal score = 0;

                    // INTELLIGENT COLUMN SCORING BASED ON QUERY INTENT

                    // 1. Essential columns for specific query intents
                    var columnLower = column.ColumnName.ToLowerInvariant();
                    var businessMeaningLower = column.BusinessMeaning?.ToLowerInvariant() ?? "";
                    var originalQueryLower = analysis.OriginalQuery.ToLowerInvariant();

                    // SMART INTENT DETECTION: Check both explicit intent and query content
                    bool isTopDepositorsQuery = analysis.Intent == "TopDepositors" ||
                                              (originalQueryLower.Contains("top") && originalQueryLower.Contains("deposit"));

                    bool isAggregationQuery = analysis.Intent == "Aggregation" ||
                                            originalQueryLower.Contains("sum") || originalQueryLower.Contains("total");

                    // Enhanced scoring for deposit-related queries (TopDepositors OR Aggregation with deposits)
                    if (isTopDepositorsQuery || (isAggregationQuery && originalQueryLower.Contains("deposit")))
                    {
                        _logger.LogDebug("🎯 Applying deposit query scoring for column: {Column}", column.ColumnName);

                        // Amount/Value columns (highest priority for deposit queries)
                        if (columnLower.Contains("amount") || columnLower.Contains("value") ||
                            columnLower.Contains("deposit") || columnLower.Contains("sum") ||
                            businessMeaningLower.Contains("amount") || businessMeaningLower.Contains("deposit"))
                        {
                            score += 2.5m; // Very high priority
                            _logger.LogDebug("💰 High priority amount column: {Column} (+2.5)", column.ColumnName);
                        }

                        // Player identification columns
                        if (columnLower.Contains("player") || columnLower.Contains("user") ||
                            columnLower.Contains("customer") || columnLower.Contains("id") ||
                            businessMeaningLower.Contains("player") || businessMeaningLower.Contains("identifier"))
                        {
                            score += 2.0m; // High priority
                            _logger.LogDebug("👤 Player identification column: {Column} (+2.0)", column.ColumnName);
                        }

                        // Geographic columns for filtering (UK, country, etc.)
                        if (columnLower.Contains("country") || columnLower.Contains("location") ||
                            columnLower.Contains("region") || columnLower.Contains("nationality") ||
                            businessMeaningLower.Contains("country") || businessMeaningLower.Contains("geographic"))
                        {
                            score += 2.0m; // High priority for geographic filtering
                            _logger.LogDebug("🌍 Geographic column: {Column} (+2.0)", column.ColumnName);
                        }

                        // Date/Time columns for temporal filtering ("yesterday", etc.)
                        if (columnLower.Contains("date") || columnLower.Contains("time") ||
                            columnLower.Contains("created") || columnLower.Contains("timestamp") ||
                            businessMeaningLower.Contains("date") || businessMeaningLower.Contains("time"))
                        {
                            score += 1.8m; // High priority for time filtering
                            _logger.LogDebug("📅 Date/time column: {Column} (+1.8)", column.ColumnName);
                        }

                        // EXCLUDE irrelevant columns for financial queries
                        if (columnLower.Contains("game") && !columnLower.Contains("deposit") &&
                            !columnLower.Contains("amount") && !columnLower.Contains("transaction"))
                        {
                            score -= 0.5m; // Penalize pure gaming columns
                            _logger.LogDebug("🎮 Penalizing gaming column: {Column} (-0.5)", column.ColumnName);
                        }
                    }

                    // 2. Business term matching with context awareness
                    foreach (var term in analysis.BusinessTerms)
                    {
                        var termLower = term.ToLowerInvariant();

                        // Direct column name match
                        if (column.ColumnName.ToLowerInvariant().Contains(termLower))
                        {
                            score += 1.5m;
                        }

                        // Business meaning match
                        if (column.BusinessMeaning?.ToLowerInvariant().Contains(termLower) == true)
                        {
                            score += 1.2m;
                        }

                        // Natural language aliases match
                        if (!string.IsNullOrEmpty(column.NaturalLanguageAliases) &&
                            column.NaturalLanguageAliases.ToLowerInvariant().Contains(termLower))
                        {
                            score += 1.0m;
                        }
                    }

                    // 3. Essential column types (always include key columns but with moderate weight)
                    if (column.IsKeyColumn)
                    {
                        score += 0.8m; // Reduced from 0.3m but still important
                    }

                    // 4. Usage frequency boost (but not too high to avoid noise)
                    if (column.UsageFrequency > 0.7m)
                    {
                        score += 0.4m;
                    }

                    // 5. Data quality consideration
                    if (column.DataQualityScore > 8.0m)
                    {
                        score += 0.2m;
                    }

                    // Only include columns with meaningful relevance
                    if (score > 0.8m)
                    {
                        columnRelevance[column.ColumnName] = score;
                    }
                }

                // Select top columns with intelligent limits
                var maxColumns = analysis.Intent == "TopDepositors" ?
                    Math.Min(maxColumnsPerTable, 8) : // Focused selection for specific intents
                    Math.Min(maxColumnsPerTable, 12); // Slightly more for general queries

                var topColumns = columnRelevance
                    .Where(kvp => kvp.Value > 0.8m) // Only meaningful columns
                    .OrderByDescending(kvp => kvp.Value)
                    .Take(maxColumns)
                    .Select(kvp => new ColumnRelevanceDto
                    {
                        TableName = table.TableName,
                        ColumnName = kvp.Key,
                        RelevanceScore = kvp.Value,
                        ReasonForRelevance = GetColumnRelevanceReason(kvp.Key, analysis)
                    });

                allColumns.AddRange(topColumns);

                _logger.LogDebug("📊 Table {TableName}: Selected {ColumnCount} columns with scores: {Scores}",
                    table.TableName, topColumns.Count(),
                    string.Join(", ", topColumns.Select(c => $"{c.ColumnName}:{c.RelevanceScore:F1}")));
            }
        }

        _logger.LogInformation("✅ Total selected columns: {ColumnCount} across {TableCount} tables",
            allColumns.Count, tables.Count);

        return allColumns;
    }

    /// <summary>
    /// Enrich schema with business context and metadata
    /// </summary>
    private async Task<EnrichedSchemaResult> EnrichWithBusinessContextAsync(
        List<TableRelevanceDto> tables, 
        List<ColumnRelevanceDto> columns)
    {
        var enrichedTables = new List<EnhancedBusinessTableDto>();
        var enrichedColumns = new List<EnhancedBusinessColumnDto>();

        foreach (var table in tables)
        {
            var tableInfo = await _context.BusinessTableInfo
                .FirstOrDefaultAsync(t => t.TableName == table.TableName && 
                                        t.SchemaName == table.SchemaName && 
                                        t.IsActive);

            if (tableInfo != null)
            {
                enrichedTables.Add(MapToEnhancedDto(tableInfo));
            }
        }

        foreach (var column in columns)
        {
            var columnInfo = await _context.BusinessColumnInfo
                .FirstOrDefaultAsync(c => c.ColumnName == column.ColumnName && c.IsActive);

            if (columnInfo != null)
            {
                var enhancedColumn = MapToEnhancedColumnDto(columnInfo);
                enhancedColumn.TableName = column.TableName; // Set the table name from the column relevance
                enrichedColumns.Add(enhancedColumn);
            }
        }

        return new EnrichedSchemaResult
        {
            Tables = enrichedTables,
            Columns = enrichedColumns
        };
    }

    /// <summary>
    /// Get relevant business glossary terms for the query and selected tables
    /// </summary>
    private async Task<List<BusinessGlossaryDto>> GetRelevantGlossaryTermsAsync(
        QueryAnalysisResult analysis,
        List<TableRelevanceDto> tables)
    {
        var glossaryTerms = new List<BusinessGlossaryDto>();

        try
        {
            _logger.LogInformation("🔍 Retrieving relevant glossary terms for query analysis");

            // 1. Get terms based on business terms from query analysis
            foreach (var businessTerm in analysis.BusinessTerms)
            {
                var matchingTerms = await _context.BusinessGlossary
                    .Where(g => g.IsActive &&
                        (g.Term.ToLower().Contains(businessTerm.ToLower()) ||
                         g.Definition.ToLower().Contains(businessTerm.ToLower()) ||
                         g.Synonyms.ToLower().Contains(businessTerm.ToLower())))
                    .ToListAsync();

                foreach (var term in matchingTerms)
                {
                    glossaryTerms.Add(MapGlossaryToDto(term));
                }
            }

            // 2. Get terms based on domain classification
            if (!string.IsNullOrEmpty(analysis.QueryCategory))
            {
                var domainTerms = await _context.BusinessGlossary
                    .Where(g => g.IsActive &&
                        (g.Domain.ToLower() == analysis.QueryCategory.ToLower() ||
                         g.Category.ToLower().Contains(analysis.QueryCategory.ToLower())))
                    .Take(10)
                    .ToListAsync();

                foreach (var term in domainTerms)
                {
                    glossaryTerms.Add(MapGlossaryToDto(term));
                }
            }

            // 3. Get terms mapped to the selected tables
            foreach (var table in tables.Take(5)) // Limit to top 5 tables
            {
                var tableTerms = await _context.BusinessGlossary
                    .Where(g => g.IsActive &&
                        !string.IsNullOrEmpty(g.MappedTables) &&
                        g.MappedTables.ToLower().Contains(table.TableName.ToLower()))
                    .Take(5)
                    .ToListAsync();

                foreach (var term in tableTerms)
                {
                    glossaryTerms.Add(MapGlossaryToDto(term));
                }
            }

            // Remove duplicates and return top terms
            var uniqueTerms = glossaryTerms
                .GroupBy(t => t.Term)
                .Select(g => g.First())
                .OrderByDescending(t => t.ConfidenceScore)
                .Take(15) // Limit to 15 most relevant terms
                .ToList();

            _logger.LogInformation("✅ Found {Count} relevant glossary terms", uniqueTerms.Count);
            return uniqueTerms;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving relevant glossary terms");
            return new List<BusinessGlossaryDto>();
        }
    }

    /// <summary>
    /// Map BusinessGlossaryEntity to BusinessGlossaryDto
    /// </summary>
    private BusinessGlossaryDto MapGlossaryToDto(BusinessGlossaryEntity entity)
    {
        var synonyms = new List<string>();
        var relatedTerms = new List<string>();
        var examples = new List<string>();

        try
        {
            if (!string.IsNullOrEmpty(entity.Synonyms))
                synonyms = JsonSerializer.Deserialize<List<string>>(entity.Synonyms) ?? new List<string>();

            if (!string.IsNullOrEmpty(entity.RelatedTerms))
                relatedTerms = JsonSerializer.Deserialize<List<string>>(entity.RelatedTerms) ?? new List<string>();

            if (!string.IsNullOrEmpty(entity.Examples))
                examples = JsonSerializer.Deserialize<List<string>>(entity.Examples) ?? new List<string>();
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex, "Error deserializing glossary term JSON fields for term: {Term}", entity.Term);
        }

        return new BusinessGlossaryDto
        {
            Id = entity.Id,
            Term = entity.Term,
            Definition = entity.Definition,
            BusinessContext = entity.BusinessContext,
            Synonyms = synonyms,
            RelatedTerms = relatedTerms,
            Category = entity.Category,
            Domain = entity.Domain ?? string.Empty,
            Examples = examples,
            MappedTables = entity.MappedTables ?? string.Empty,
            MappedColumns = entity.MappedColumns ?? string.Empty,
            ConfidenceScore = (double)entity.ConfidenceScore,
            UsageCount = entity.UsageCount,
            IsActive = entity.IsActive
        };
    }

    // Helper methods for mapping and analysis
    private SemanticSchemaMappingDto MapToDto(SemanticSchemaMappingEntity entity)
    {
        return new SemanticSchemaMappingDto
        {
            Id = entity.Id,
            QueryIntent = entity.QueryIntent,
            RelevantTables = string.IsNullOrEmpty(entity.RelevantTables) 
                ? new List<TableRelevanceDto>() 
                : JsonSerializer.Deserialize<List<TableRelevanceDto>>(entity.RelevantTables) ?? new List<TableRelevanceDto>(),
            RelevantColumns = string.IsNullOrEmpty(entity.RelevantColumns)
                ? new List<ColumnRelevanceDto>()
                : JsonSerializer.Deserialize<List<ColumnRelevanceDto>>(entity.RelevantColumns) ?? new List<ColumnRelevanceDto>(),
            BusinessTerms = string.IsNullOrEmpty(entity.BusinessTerms)
                ? new List<string>()
                : JsonSerializer.Deserialize<List<string>>(entity.BusinessTerms) ?? new List<string>(),
            QueryCategory = entity.QueryCategory,
            ConfidenceScore = entity.ConfidenceScore,
            UsageCount = entity.UsageCount,
            LastUsed = entity.LastUsed,
            IsActive = entity.IsActive
        };
    }

    private EnhancedBusinessTableDto MapToEnhancedDto(BusinessTableInfoEntity entity)
    {
        return new EnhancedBusinessTableDto
        {
            Id = entity.Id,
            TableName = entity.TableName,
            SchemaName = entity.SchemaName,
            BusinessPurpose = entity.BusinessPurpose,
            BusinessContext = entity.BusinessContext,
            PrimaryUseCase = entity.PrimaryUseCase,
            BusinessRules = entity.BusinessRules,
            DomainClassification = entity.DomainClassification,
            NaturalLanguageAliases = string.IsNullOrEmpty(entity.NaturalLanguageAliases)
                ? new List<string>()
                : JsonSerializer.Deserialize<List<string>>(entity.NaturalLanguageAliases) ?? new List<string>(),
            ImportanceScore = entity.ImportanceScore,
            UsageFrequency = entity.UsageFrequency,
            LastAnalyzed = entity.LastAnalyzed,
            BusinessOwner = entity.BusinessOwner,
            IsActive = entity.IsActive,
            // Use only existing entity properties to avoid compilation errors
            // Enhanced metadata will be populated by the AI enhancement service
        };
    }

    private EnhancedBusinessColumnDto MapToEnhancedColumnDto(BusinessColumnInfoEntity entity)
    {
        return new EnhancedBusinessColumnDto
        {
            Id = entity.Id,
            TableInfoId = entity.TableInfoId,
            ColumnName = entity.ColumnName,
            BusinessMeaning = entity.BusinessMeaning,
            BusinessContext = entity.BusinessContext,
            ValidationRules = entity.ValidationRules,
            NaturalLanguageAliases = string.IsNullOrEmpty(entity.NaturalLanguageAliases)
                ? new List<string>()
                : JsonSerializer.Deserialize<List<string>>(entity.NaturalLanguageAliases) ?? new List<string>(),
            BusinessDataType = entity.BusinessDataType,
            DataQualityScore = entity.DataQualityScore,
            UsageFrequency = entity.UsageFrequency,
            PreferredAggregation = entity.PreferredAggregation,
            IsKeyColumn = entity.IsKeyColumn,
            IsSensitiveData = entity.IsSensitiveData,
            IsCalculatedField = entity.IsCalculatedField,
            IsActive = entity.IsActive,
            // Use only existing entity properties to avoid compilation errors
            // Enhanced metadata will be populated by the AI enhancement service
        };
    }

    #region Intelligent Query Analysis Implementation

    /// <summary>
    /// Extract business terms from query using intelligent pattern matching and domain knowledge
    /// </summary>
    private async Task<List<string>> ExtractBusinessTermsAsync(string query)
    {
        var terms = new List<string>();
        var lowerQuery = query.ToLowerInvariant();

        // Financial/Banking terms
        var financialTerms = new[] { "deposit", "depositor", "withdrawal", "transaction", "payment", "balance", "amount", "money", "cash", "fund" };
        var geographicTerms = new[] { "country", "region", "location", "uk", "usa", "canada", "europe", "asia", "city", "state" };
        var timeTerms = new[] { "yesterday", "today", "last week", "last month", "year", "daily", "weekly", "monthly", "recent", "latest" };
        var aggregationTerms = new[] { "top", "bottom", "highest", "lowest", "sum", "total", "count", "average", "max", "min", "best", "worst" };
        var playerTerms = new[] { "player", "user", "customer", "account", "member", "client", "person", "individual" };

        // Check for financial terms
        foreach (var term in financialTerms)
        {
            if (lowerQuery.Contains(term))
            {
                terms.Add(term);
            }
        }

        // Check for geographic terms
        foreach (var term in geographicTerms)
        {
            if (lowerQuery.Contains(term))
            {
                terms.Add(term);
            }
        }

        // Check for time terms
        foreach (var term in timeTerms)
        {
            if (lowerQuery.Contains(term))
            {
                terms.Add(term);
            }
        }

        // Check for aggregation terms
        foreach (var term in aggregationTerms)
        {
            if (lowerQuery.Contains(term))
            {
                terms.Add(term);
            }
        }

        // Check for player terms
        foreach (var term in playerTerms)
        {
            if (lowerQuery.Contains(term))
            {
                terms.Add(term);
            }
        }

        // Extract numbers (for "top 10", amounts, etc.)
        var numberMatches = System.Text.RegularExpressions.Regex.Matches(lowerQuery, @"\b\d+\b");
        foreach (System.Text.RegularExpressions.Match match in numberMatches)
        {
            terms.Add($"number_{match.Value}");
        }

        return terms.Distinct().ToList();
    }

    /// <summary>
    /// Determine query category based on intelligent analysis
    /// </summary>
    private string DetermineQueryCategory(string query)
    {
        var lowerQuery = query.ToLowerInvariant();

        // Financial/Banking queries
        if (lowerQuery.Contains("deposit") || lowerQuery.Contains("withdrawal") ||
            lowerQuery.Contains("transaction") || lowerQuery.Contains("payment") ||
            lowerQuery.Contains("balance") || lowerQuery.Contains("amount"))
        {
            return "Financial";
        }

        // Player/Gaming queries
        if (lowerQuery.Contains("player") || lowerQuery.Contains("game") ||
            lowerQuery.Contains("session") || lowerQuery.Contains("activity"))
        {
            return "Gaming";
        }

        // Geographic/Location queries
        if (lowerQuery.Contains("country") || lowerQuery.Contains("region") ||
            lowerQuery.Contains("uk") || lowerQuery.Contains("usa") ||
            lowerQuery.Contains("location"))
        {
            return "Geographic";
        }

        // Analytical queries (top, sum, count, etc.)
        if (lowerQuery.Contains("top") || lowerQuery.Contains("sum") ||
            lowerQuery.Contains("count") || lowerQuery.Contains("total") ||
            lowerQuery.Contains("average") || lowerQuery.Contains("highest"))
        {
            return "Analytical";
        }

        return "General";
    }

    /// <summary>
    /// Extract entities from query using intelligent pattern matching
    /// </summary>
    private async Task<List<string>> ExtractEntitiesAsync(string query)
    {
        var entities = new List<string>();
        var lowerQuery = query.ToLowerInvariant();

        // Table entities based on query content
        if (lowerQuery.Contains("deposit"))
        {
            entities.Add("deposits");
            entities.Add("transactions");
            entities.Add("players");
        }

        if (lowerQuery.Contains("country") || lowerQuery.Contains("uk") || lowerQuery.Contains("location"))
        {
            entities.Add("countries");
            entities.Add("players");
        }

        if (lowerQuery.Contains("player") || lowerQuery.Contains("user") || lowerQuery.Contains("customer"))
        {
            entities.Add("players");
        }

        if (lowerQuery.Contains("yesterday") || lowerQuery.Contains("daily") || lowerQuery.Contains("date"))
        {
            entities.Add("daily_actions");
            entities.Add("transactions");
        }

        return entities.Distinct().ToList();
    }

    /// <summary>
    /// Determine query intent based on analysis
    /// </summary>
    private string DetermineQueryIntent(string query, List<string> terms)
    {
        var lowerQuery = query.ToLowerInvariant();

        if (lowerQuery.Contains("top") && (lowerQuery.Contains("deposit") || lowerQuery.Contains("amount")))
        {
            return "TopDepositors";
        }

        if (lowerQuery.Contains("sum") || lowerQuery.Contains("total"))
        {
            return "Aggregation";
        }

        if (lowerQuery.Contains("count") || lowerQuery.Contains("number"))
        {
            return "Counting";
        }

        if (lowerQuery.Contains("yesterday") || lowerQuery.Contains("last") || lowerQuery.Contains("recent"))
        {
            return "TimeFiltered";
        }

        if (lowerQuery.Contains("country") || lowerQuery.Contains("uk") || lowerQuery.Contains("region"))
        {
            return "GeographicFiltered";
        }

        return "General";
    }

    /// <summary>
    /// Assess query complexity based on multiple factors
    /// </summary>
    private double AssessQueryComplexityScore(string query, List<string> terms)
    {
        double complexity = 0.3; // Base complexity

        // Add complexity for aggregations
        if (query.ToLowerInvariant().Contains("top") || query.ToLowerInvariant().Contains("sum"))
            complexity += 0.2;

        // Add complexity for time filtering
        if (terms.Any(t => t.Contains("yesterday") || t.Contains("daily")))
            complexity += 0.2;

        // Add complexity for geographic filtering
        if (terms.Any(t => t.Contains("country") || t.Contains("uk")))
            complexity += 0.2;

        // Add complexity for multiple entities
        if (terms.Count > 5)
            complexity += 0.1;

        return Math.Min(complexity, 1.0);
    }

    /// <summary>
    /// Get relevance reason for table selection
    /// </summary>
    private string GetRelevanceReason(string table, QueryAnalysisResult analysis)
    {
        var reasons = new List<string>();

        if (analysis.QueryCategory == "Financial" && table.ToLowerInvariant().Contains("deposit"))
            reasons.Add("Financial domain match");

        if (analysis.QueryCategory == "Geographic" && table.ToLowerInvariant().Contains("country"))
            reasons.Add("Geographic domain match");

        if (analysis.QueryCategory == "Gaming" && table.ToLowerInvariant().Contains("player"))
            reasons.Add("Gaming domain match");

        if (analysis.BusinessTerms.Any(t => table.ToLowerInvariant().Contains(t)))
            reasons.Add("Business term match");

        return reasons.Any() ? string.Join("; ", reasons) : "General relevance";
    }

    /// <summary>
    /// Get relevance reason for column selection
    /// </summary>
    private string GetColumnRelevanceReason(string column, QueryAnalysisResult analysis)
    {
        var reasons = new List<string>();

        if (analysis.BusinessTerms.Any(t => column.ToLowerInvariant().Contains(t)))
            reasons.Add("Business term match");

        if (analysis.Intent == "TopDepositors" && column.ToLowerInvariant().Contains("amount"))
            reasons.Add("Amount field for top depositors");

        if (analysis.Intent == "GeographicFiltered" && column.ToLowerInvariant().Contains("country"))
            reasons.Add("Geographic filtering field");

        return reasons.Any() ? string.Join("; ", reasons) : "General relevance";
    }

    /// <summary>
    /// Calculate overall confidence based on table and column relevance
    /// </summary>
    private decimal CalculateOverallConfidence(List<TableRelevanceDto> tables, List<ColumnRelevanceDto> columns)
    {
        if (!tables.Any()) return 0.1m;

        var avgTableRelevance = tables.Average(t => (double)t.RelevanceScore);
        var avgColumnRelevance = columns.Any() ? columns.Average(c => (double)c.RelevanceScore) : 0.5;

        return (decimal)((avgTableRelevance * 0.6) + (avgColumnRelevance * 0.4));
    }

    /// <summary>
    /// Estimate token usage more accurately
    /// </summary>
    private int EstimateTokenUsage(EnrichedSchemaResult schema)
    {
        // More conservative token estimation
        var tableTokens = schema.Tables.Count * 30; // Reduced from 50
        var columnTokens = schema.Columns.Count * 15; // Reduced from 20
        var metadataTokens = schema.Columns.Count * 10; // Additional metadata

        return tableTokens + columnTokens + metadataTokens;
    }

    /// <summary>
    /// Store mapping for future learning
    /// </summary>
    private async Task StoreMappingAsync(string query, QueryAnalysisResult analysis, List<TableRelevanceDto> tables, List<ColumnRelevanceDto> columns)
    {
        try
        {
            // Store the successful mapping for future semantic similarity matching
            _logger.LogDebug("Storing mapping for query: {Query} with {TableCount} tables", query, tables.Count);
            // Implementation would store in cache or database for learning
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to store mapping for query: {Query}", query);
        }
    }

    /// <summary>
    /// Get fallback schema when analysis fails
    /// </summary>
    private async Task<ContextualizedSchemaResult> GetFallbackSchemaAsync(string query)
    {
        return new ContextualizedSchemaResult
        {
            Query = query,
            RelevantTables = new List<EnhancedBusinessTableDto>(),
            RelevantColumns = new List<EnhancedBusinessColumnDto>(),
            BusinessTermsUsed = new List<string>(),
            ConfidenceScore = 0.1m,
            TokenEstimate = 100,
            GeneratedAt = DateTime.UtcNow
        };
    }

    #endregion
}

// Supporting classes
public class EnrichedSchemaResult
{
    public List<EnhancedBusinessTableDto> Tables { get; set; } = new();
    public List<EnhancedBusinessColumnDto> Columns { get; set; } = new();
}
