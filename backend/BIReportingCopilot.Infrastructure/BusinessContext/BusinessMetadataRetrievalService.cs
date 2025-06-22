using Microsoft.Extensions.Logging;
using BIReportingCopilot.Core.Interfaces.BusinessContext;
using BIReportingCopilot.Core.Interfaces.Business;
using BIReportingCopilot.Core.Models.BusinessContext;
using BIReportingCopilot.Core.DTOs;

namespace BIReportingCopilot.Infrastructure.BusinessContext;

/// <summary>
/// Service for retrieving relevant business metadata based on context analysis
/// </summary>
public class BusinessMetadataRetrievalService : IBusinessMetadataRetrievalService
{
    private readonly IBusinessTableManagementService _businessTableService;
    private readonly IGlossaryManagementService _glossaryService;
    private readonly ISemanticMatchingService _semanticMatchingService;
    private readonly ILogger<BusinessMetadataRetrievalService> _logger;

    public BusinessMetadataRetrievalService(
        IBusinessTableManagementService businessTableService,
        IGlossaryManagementService glossaryService,
        ISemanticMatchingService semanticMatchingService,
        ILogger<BusinessMetadataRetrievalService> logger)
    {
        _businessTableService = businessTableService;
        _glossaryService = glossaryService;
        _semanticMatchingService = semanticMatchingService;
        _logger = logger;
    }

    public async Task<ContextualBusinessSchema> GetRelevantBusinessMetadataAsync(
        BusinessContextProfile profile,
        int maxTables = 5)
    {
        _logger.LogInformation("Retrieving business metadata for intent: {Intent}, domain: {Domain}", 
            profile.Intent.Type, profile.Domain.Name);

        // Find relevant tables
        var relevantTables = await FindRelevantTablesAsync(profile, maxTables);
        
        // Get columns for relevant tables
        var tableIds = relevantTables.Select(t => t.Id).ToList();
        var tableColumns = new Dictionary<long, List<BusinessColumnInfo>>();
        
        foreach (var tableId in tableIds)
        {
            var columns = await FindRelevantColumnsAsync(new List<long> { tableId }, profile);
            tableColumns[tableId] = columns;
        }

        // Find relevant glossary terms
        var glossaryTerms = await FindRelevantGlossaryTermsAsync(profile.BusinessTerms);

        // Discover table relationships
        var tableNames = relevantTables.Select(t => $"{t.SchemaName}.{t.TableName}").ToList();
        var relationships = await DiscoverTableRelationshipsAsync(tableNames);

        // Generate business rules
        var businessRules = await GenerateBusinessRulesAsync(relevantTables, profile);

        // Calculate complexity
        var complexity = CalculateSchemaComplexity(relevantTables, relationships);

        // Calculate overall relevance score
        var relevanceScore = CalculateRelevanceScore(profile, relevantTables, glossaryTerms);

        return new ContextualBusinessSchema
        {
            RelevantTables = relevantTables,
            TableColumns = tableColumns,
            RelevantGlossaryTerms = glossaryTerms,
            BusinessRules = businessRules,
            TableRelationships = relationships,
            RelevanceScore = relevanceScore,
            Complexity = complexity,
            SuggestedIndexes = GenerateIndexSuggestions(relevantTables, profile),
            PartitioningHints = GeneratePartitioningHints(relevantTables, profile)
        };
    }

    public async Task<List<BusinessTableInfoDto>> FindRelevantTablesAsync(
        BusinessContextProfile profile, 
        int maxTables = 5)
    {
        var allTables = new List<BusinessTableInfoDto>();

        // 1. Semantic search based on user question
        var semanticTables = await _semanticMatchingService.SemanticTableSearchAsync(
            profile.OriginalQuestion, maxTables * 2);
        allTables.AddRange(semanticTables);

        // 2. Domain-based filtering
        if (!string.IsNullOrEmpty(profile.Domain.Name))
        {
            var domainTables = await GetTablesByDomainAsync(profile.Domain.Name);
            allTables.AddRange(domainTables);
        }

        // 3. Entity-based matching
        foreach (var entity in profile.Entities.Where(e => e.Type == EntityType.Table))
        {
            var entityTables = await GetTablesByEntityAsync(entity);
            allTables.AddRange(entityTables);
        }

        // 4. Business term matching
        foreach (var term in profile.BusinessTerms)
        {
            var termTables = await GetTablesByBusinessTermAsync(term);
            allTables.AddRange(termTables);
        }

        // Score and rank tables
        var scoredTables = await ScoreTablesAsync(allTables.Distinct().ToList(), profile);

        // Return top tables
        return scoredTables
            .OrderByDescending(t => t.RelevanceScore)
            .Take(maxTables)
            .Select(t => t.Table)
            .ToList();
    }

    public async Task<List<BusinessColumnInfo>> FindRelevantColumnsAsync(
        List<long> tableIds, 
        BusinessContextProfile profile)
    {
        var allColumns = new List<BusinessColumnInfo>();

        foreach (var tableId in tableIds)
        {
            // Get table details with columns
            var table = await _businessTableService.GetBusinessTableAsync(tableId);
            if (table?.Columns != null)
            {
                allColumns.AddRange(table.Columns.Select(c => new BusinessColumnInfo
                {
                    ColumnName = c.ColumnName,
                    BusinessMeaning = c.BusinessMeaning,
                    BusinessContext = c.BusinessContext,
                    IsKeyColumn = c.IsKeyColumn
                }));
            }
        }

        // Filter columns based on context
        var relevantColumns = new List<BusinessColumnInfo>();

        // 1. Columns matching identified metrics
        foreach (var metric in profile.IdentifiedMetrics)
        {
            var metricColumns = allColumns.Where(c => 
                c.BusinessMeaning.Contains(metric, StringComparison.OrdinalIgnoreCase) ||
                c.ColumnName.Contains(metric, StringComparison.OrdinalIgnoreCase)).ToList();
            relevantColumns.AddRange(metricColumns);
        }

        // 2. Columns matching identified dimensions
        foreach (var dimension in profile.IdentifiedDimensions)
        {
            var dimensionColumns = allColumns.Where(c => 
                c.BusinessMeaning.Contains(dimension, StringComparison.OrdinalIgnoreCase) ||
                c.ColumnName.Contains(dimension, StringComparison.OrdinalIgnoreCase)).ToList();
            relevantColumns.AddRange(dimensionColumns);
        }

        // 3. Key columns (always include)
        var keyColumns = allColumns.Where(c => c.IsKeyColumn).ToList();
        relevantColumns.AddRange(keyColumns);

        // 4. Time-related columns if time context exists
        if (profile.TimeContext != null)
        {
            var timeColumns = allColumns.Where(c => 
                c.ColumnName.Contains("date", StringComparison.OrdinalIgnoreCase) ||
                c.ColumnName.Contains("time", StringComparison.OrdinalIgnoreCase) ||
                c.BusinessMeaning.Contains("date", StringComparison.OrdinalIgnoreCase) ||
                c.BusinessMeaning.Contains("time", StringComparison.OrdinalIgnoreCase)).ToList();
            relevantColumns.AddRange(timeColumns);
        }

        return relevantColumns.Distinct().ToList();
    }

    public async Task<List<BusinessGlossaryDto>> FindRelevantGlossaryTermsAsync(
        List<string> businessTerms)
    {
        var glossaryTerms = new List<BusinessGlossaryDto>();

        foreach (var term in businessTerms)
        {
            try
            {
                var searchResults = await _glossaryService.SearchGlossaryTermsAsync(term);
                glossaryTerms.AddRange(searchResults);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error searching glossary for term: {Term}", term);
            }
        }

        return glossaryTerms.Distinct().ToList();
    }

    public async Task<List<TableRelationship>> DiscoverTableRelationshipsAsync(
        List<string> tableNames)
    {
        // This would analyze foreign key relationships and business relationships
        // For now, return empty list - would be implemented with actual relationship discovery
        await Task.CompletedTask;
        return new List<TableRelationship>();
    }

    // Helper methods
    private async Task<List<BusinessTableInfoDto>> GetTablesByDomainAsync(string domainName)
    {
        // This would filter tables by domain classification
        var allTables = await _businessTableService.GetBusinessTablesAsync();
        return allTables.Where(t => 
            t.DomainClassification?.Contains(domainName, StringComparison.OrdinalIgnoreCase) == true)
            .ToList();
    }

    private async Task<List<BusinessTableInfoDto>> GetTablesByEntityAsync(BusinessEntity entity)
    {
        // This would find tables matching the entity
        if (!string.IsNullOrEmpty(entity.MappedTableName))
        {
            var allTables = await _businessTableService.GetBusinessTablesAsync();
            return allTables.Where(t => 
                t.TableName.Equals(entity.MappedTableName, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }
        return new List<BusinessTableInfoDto>();
    }

    private async Task<List<BusinessTableInfoDto>> GetTablesByBusinessTermAsync(string term)
    {
        // This would find tables containing the business term
        var allTables = await _businessTableService.GetBusinessTablesAsync();
        return allTables.Where(t => 
            t.BusinessPurpose.Contains(term, StringComparison.OrdinalIgnoreCase) ||
            t.BusinessContext.Contains(term, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    private async Task<List<(BusinessTableInfoDto Table, double RelevanceScore)>> ScoreTablesAsync(
        List<BusinessTableInfoDto> tables, 
        BusinessContextProfile profile)
    {
        var scoredTables = new List<(BusinessTableInfoDto Table, double RelevanceScore)>();

        foreach (var table in tables)
        {
            var score = await CalculateTableRelevanceScore(table, profile);
            scoredTables.Add((table, score));
        }

        return scoredTables;
    }

    private async Task<double> CalculateTableRelevanceScore(
        BusinessTableInfoDto table, 
        BusinessContextProfile profile)
    {
        double score = 0.0;

        // Domain match
        if (table.DomainClassification?.Contains(profile.Domain.Name, StringComparison.OrdinalIgnoreCase) == true)
        {
            score += 0.3;
        }

        // Business term matches
        foreach (var term in profile.BusinessTerms)
        {
            if (table.BusinessPurpose.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                table.BusinessContext.Contains(term, StringComparison.OrdinalIgnoreCase))
            {
                score += 0.2;
            }
        }

        // Semantic similarity
        var semanticScore = await _semanticMatchingService.CalculateSemanticSimilarityAsync(
            profile.OriginalQuestion, 
            $"{table.BusinessPurpose} {table.BusinessContext}");
        score += semanticScore * 0.5;

        return Math.Min(score, 1.0);
    }

    private async Task<List<BusinessRule>> GenerateBusinessRulesAsync(
        List<BusinessTableInfoDto> tables, 
        BusinessContextProfile profile)
    {
        // This would generate business rules based on table metadata
        await Task.CompletedTask;
        return new List<BusinessRule>();
    }

    private SchemaComplexity CalculateSchemaComplexity(
        List<BusinessTableInfoDto> tables, 
        List<TableRelationship> relationships)
    {
        var tableCount = tables.Count;
        var relationshipCount = relationships.Count;

        return (tableCount, relationshipCount) switch
        {
            (1, _) => SchemaComplexity.Simple,
            (<= 3, <= 2) => SchemaComplexity.Moderate,
            (<= 6, <= 5) => SchemaComplexity.Complex,
            _ => SchemaComplexity.VeryComplex
        };
    }

    private double CalculateRelevanceScore(
        BusinessContextProfile profile,
        List<BusinessTableInfoDto> tables,
        List<BusinessGlossaryDto> glossaryTerms)
    {
        // Calculate overall relevance based on various factors
        var factors = new List<double>
        {
            profile.ConfidenceScore,
            tables.Any() ? 1.0 : 0.0,
            glossaryTerms.Any() ? 1.0 : 0.0
        };

        return factors.Average();
    }

    private List<string> GenerateIndexSuggestions(
        List<BusinessTableInfoDto> tables, 
        BusinessContextProfile profile)
    {
        var suggestions = new List<string>();

        // Add index suggestions based on query patterns
        if (profile.Intent.Type == IntentType.Aggregation)
        {
            suggestions.Add("Consider indexes on grouping columns");
        }

        if (profile.TimeContext != null)
        {
            suggestions.Add("Consider indexes on date/time columns");
        }

        return suggestions;
    }

    private List<string> GeneratePartitioningHints(
        List<BusinessTableInfoDto> tables, 
        BusinessContextProfile profile)
    {
        var hints = new List<string>();

        if (profile.TimeContext != null)
        {
            hints.Add("Consider date-based partitioning for time-series queries");
        }

        return hints;
    }
}
