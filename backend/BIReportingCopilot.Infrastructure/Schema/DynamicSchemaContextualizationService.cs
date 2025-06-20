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

            // Step 6: Store this mapping for future use
            await StoreMappingAsync(naturalLanguageQuery, queryAnalysis, relevantTables, relevantColumns);

            var result = new ContextualizedSchemaResult
            {
                Query = naturalLanguageQuery,
                QueryAnalysis = queryAnalysis,
                RelevantTables = enrichedSchema.Tables,
                RelevantColumns = enrichedSchema.Columns,
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

        // Score tables based on business terms
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

        // Score tables based on similar mappings
        foreach (var mapping in similarMappings)
        {
            foreach (var table in mapping.RelevantTables)
            {
                var boost = mapping.ConfidenceScore * 0.6m;
                tableRelevance[table.TableName] = tableRelevance.GetValueOrDefault(table.TableName, 0) + boost;
            }
        }

        // Score tables based on domain classification
        if (!string.IsNullOrEmpty(analysis.QueryCategory))
        {
            var domainTables = await _context.BusinessTableInfo
                .Where(t => t.DomainClassification.ToLower().Contains(analysis.QueryCategory.ToLower()) && t.IsActive)
                .ToListAsync();

            foreach (var table in domainTables)
            {
                var key = $"{table.SchemaName}.{table.TableName}";
                tableRelevance[key] = tableRelevance.GetValueOrDefault(key, 0) + 0.5m;
            }
        }

        // Convert to DTOs and sort by relevance
        var result = tableRelevance
            .OrderByDescending(kvp => kvp.Value)
            .Take(maxTables)
            .Select(kvp => new TableRelevanceDto
            {
                TableName = kvp.Key.Contains('.') ? kvp.Key.Split('.')[1] : kvp.Key,
                SchemaName = kvp.Key.Contains('.') ? kvp.Key.Split('.')[0] : "dbo",
                RelevanceScore = kvp.Value,
                ReasonForRelevance = GetRelevanceReason(kvp.Key, analysis)
            })
            .ToList();

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

                // Score columns based on business terms
                foreach (var term in analysis.BusinessTerms)
                {
                    foreach (var column in tableInfo.Columns.Where(c => c.IsActive))
                    {
                        if (column.BusinessMeaning.ToLower().Contains(term.ToLower()) ||
                            column.ColumnName.ToLower().Contains(term.ToLower()) ||
                            (!string.IsNullOrEmpty(column.NaturalLanguageAliases) && 
                             column.NaturalLanguageAliases.ToLower().Contains(term.ToLower())))
                        {
                            columnRelevance[column.ColumnName] = columnRelevance.GetValueOrDefault(column.ColumnName, 0) + 0.9m;
                        }
                    }
                }

                // Boost key columns and frequently used columns
                foreach (var column in tableInfo.Columns.Where(c => c.IsActive))
                {
                    if (column.IsKeyColumn)
                        columnRelevance[column.ColumnName] = columnRelevance.GetValueOrDefault(column.ColumnName, 0) + 0.3m;
                    
                    if (column.UsageFrequency > 0.5m)
                        columnRelevance[column.ColumnName] = columnRelevance.GetValueOrDefault(column.ColumnName, 0) + (column.UsageFrequency * 0.2m);
                }

                // Add top columns for this table
                var topColumns = columnRelevance
                    .OrderByDescending(kvp => kvp.Value)
                    .Take(maxColumnsPerTable)
                    .Select(kvp => new ColumnRelevanceDto
                    {
                        TableName = table.TableName,
                        ColumnName = kvp.Key,
                        RelevanceScore = kvp.Value,
                        ReasonForRelevance = GetColumnRelevanceReason(kvp.Key, analysis)
                    });

                allColumns.AddRange(topColumns);
            }
        }

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
            IsActive = entity.IsActive
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
            IsActive = entity.IsActive
        };
    }

    // Additional helper methods would be implemented here...
    private async Task<List<string>> ExtractBusinessTermsAsync(string query) => new List<string>();
    private string DetermineQueryCategory(string query) => "General";
    private async Task<List<string>> ExtractEntitiesAsync(string query) => new List<string>();
    private string DetermineQueryIntent(string query, List<string> terms) => "General";
    private double AssessQueryComplexityScore(string query, List<string> terms) => 0.5;
    private string GetRelevanceReason(string table, QueryAnalysisResult analysis) => "Business term match";
    private string GetColumnRelevanceReason(string column, QueryAnalysisResult analysis) => "Business term match";
    private decimal CalculateOverallConfidence(List<TableRelevanceDto> tables, List<ColumnRelevanceDto> columns) => 0.8m;
    private int EstimateTokenUsage(EnrichedSchemaResult schema) => schema.Tables.Count * 50 + schema.Columns.Count * 20;
    private async Task StoreMappingAsync(string query, QueryAnalysisResult analysis, List<TableRelevanceDto> tables, List<ColumnRelevanceDto> columns) { }
    private async Task<ContextualizedSchemaResult> GetFallbackSchemaAsync(string query) => new ContextualizedSchemaResult { Query = query };
}

// Supporting classes
public class EnrichedSchemaResult
{
    public List<EnhancedBusinessTableDto> Tables { get; set; } = new();
    public List<EnhancedBusinessColumnDto> Columns { get; set; } = new();
}
