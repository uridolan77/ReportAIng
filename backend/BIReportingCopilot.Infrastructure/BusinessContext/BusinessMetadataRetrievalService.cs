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
        try
        {
            _logger.LogInformation("🔍 Retrieving business metadata for intent: {Intent}, domain: {Domain}",
                profile.Intent.Type, profile.Domain.Name);

            // Find relevant tables
            _logger.LogDebug("Step 1: Finding relevant tables (max: {MaxTables})", maxTables);
            var relevantTables = await FindRelevantTablesAsync(profile, maxTables);
            _logger.LogInformation("✅ Found {TableCount} relevant tables", relevantTables.Count);

            // Get columns for relevant tables
            _logger.LogDebug("Step 2: Finding relevant columns for {TableCount} tables", relevantTables.Count);
            var tableIds = relevantTables.Select(t => t.Id).ToList();
            var tableColumns = new Dictionary<long, List<BusinessColumnInfo>>();

            foreach (var tableId in tableIds)
            {
                try
                {
                    var columns = await FindRelevantColumnsAsync(new List<long> { tableId }, profile);
                    tableColumns[tableId] = columns;
                    _logger.LogDebug("✅ Found {ColumnCount} columns for table ID {TableId}", columns.Count, tableId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "❌ Error finding columns for table ID {TableId}", tableId);
                    tableColumns[tableId] = new List<BusinessColumnInfo>();
                }
            }

            // Find relevant glossary terms from multiple sources
            _logger.LogDebug("Step 3: Finding comprehensive glossary terms");
            var glossaryTerms = await FindComprehensiveGlossaryTermsAsync(profile, relevantTables, tableColumns);
            _logger.LogInformation("✅ Found {GlossaryCount} glossary terms", glossaryTerms.Count);

            // Discover table relationships
            _logger.LogDebug("Step 4: Discovering table relationships");
            var tableNames = relevantTables.Select(t => $"{t.SchemaName}.{t.TableName}").ToList();
            var relationships = await DiscoverTableRelationshipsAsync(tableNames);
            _logger.LogDebug("✅ Found {RelationshipCount} table relationships", relationships.Count);

            // Generate business rules
            _logger.LogDebug("Step 5: Generating business rules");
            var businessRules = await GenerateBusinessRulesAsync(relevantTables, profile);
            _logger.LogDebug("✅ Generated {RuleCount} business rules", businessRules.Count);

            // Calculate complexity
            _logger.LogDebug("Step 6: Calculating schema complexity");
            var complexity = CalculateSchemaComplexity(relevantTables, relationships);

            // Calculate overall relevance score
            _logger.LogDebug("Step 7: Calculating relevance score");
            var relevanceScore = CalculateRelevanceScore(profile, relevantTables, glossaryTerms);

            var result = new ContextualBusinessSchema
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

            _logger.LogInformation("🎉 Business metadata retrieval completed successfully: {TableCount} tables, {ColumnCount} columns, {GlossaryCount} terms, relevance: {RelevanceScore:F2}",
                result.RelevantTables.Count,
                result.TableColumns.Values.SelectMany(c => c).Count(),
                result.RelevantGlossaryTerms.Count,
                result.RelevanceScore);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error in GetRelevantBusinessMetadataAsync: {ErrorMessage}", ex.Message);
            throw;
        }
    }

    public async Task<List<BusinessTableInfoDto>> FindRelevantTablesAsync(
        BusinessContextProfile profile,
        int maxTables = 5)
    {
        try
        {
            var allTables = new List<BusinessTableInfoDto>();

            // 1. Semantic search based on user question
            try
            {
                _logger.LogDebug("🔍 Performing semantic table search for: {Question}", profile.OriginalQuestion);
                var semanticTables = await _semanticMatchingService.SemanticTableSearchAsync(
                    profile.OriginalQuestion, maxTables * 2);
                allTables.AddRange(semanticTables);
                _logger.LogDebug("✅ Semantic search found {Count} tables", semanticTables.Count);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "⚠️ Semantic table search failed, continuing with other methods");
            }

            // 2. Domain-based filtering
            if (!string.IsNullOrEmpty(profile.Domain.Name))
            {
                try
                {
                    _logger.LogDebug("🔍 Finding tables by domain: {Domain}", profile.Domain.Name);
                    var domainTables = await GetTablesByDomainAsync(profile.Domain.Name);
                    allTables.AddRange(domainTables);
                    _logger.LogDebug("✅ Domain search found {Count} tables", domainTables.Count);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "⚠️ Domain-based table search failed for domain: {Domain}", profile.Domain.Name);
                }
            }

            // 3. Entity-based matching
            foreach (var entity in profile.Entities.Where(e => e.Type == EntityType.Table))
            {
                try
                {
                    _logger.LogDebug("🔍 Finding tables by entity: {Entity}", entity.Name);
                    var entityTables = await GetTablesByEntityAsync(entity);
                    allTables.AddRange(entityTables);
                    _logger.LogDebug("✅ Entity search found {Count} tables for entity: {Entity}", entityTables.Count, entity.Name);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "⚠️ Entity-based table search failed for entity: {Entity}", entity.Name);
                }
            }

            // 4. Business term matching
            foreach (var term in profile.BusinessTerms)
            {
                try
                {
                    _logger.LogDebug("🔍 Finding tables by business term: {Term}", term);
                    var termTables = await GetTablesByBusinessTermAsync(term);
                    allTables.AddRange(termTables);
                    _logger.LogDebug("✅ Business term search found {Count} tables for term: {Term}", termTables.Count, term);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "⚠️ Business term table search failed for term: {Term}", term);
                }
            }

            // If no tables found through specific searches, get all active tables as fallback
            if (!allTables.Any())
            {
                _logger.LogWarning("⚠️ No tables found through specific searches, using fallback to get all active tables");
                try
                {
                    var allActiveTables = await _businessTableService.GetBusinessTablesAsync();
                    allTables.AddRange(allActiveTables.Take(maxTables * 2)); // Limit fallback
                    _logger.LogInformation("✅ Fallback found {Count} active tables", allTables.Count);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "❌ Even fallback table retrieval failed");
                    throw;
                }
            }

            // Score and rank tables
            _logger.LogDebug("🔍 Scoring and ranking {Count} unique tables", allTables.Distinct().Count());
            var scoredTables = await ScoreTablesAsync(allTables.Distinct().ToList(), profile);

            // Return top tables
            var result = scoredTables
                .OrderByDescending(t => t.RelevanceScore)
                .Take(maxTables)
                .Select(t => t.Table)
                .ToList();

            _logger.LogInformation("✅ FindRelevantTablesAsync completed: {ResultCount} tables selected from {TotalCount} candidates",
                result.Count, allTables.Distinct().Count());

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error in FindRelevantTablesAsync: {ErrorMessage}", ex.Message);
            throw;
        }
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
                _logger.LogDebug("Processing {ColumnCount} columns for table {Schema}.{Table}",
                    table.Columns.Count, table.SchemaName, table.TableName);

                // Use the business metadata directly - no need to query SchemaMetadata
                // The BusinessColumnInfo already contains all the information we need
                allColumns.AddRange(table.Columns.Select(c => {
                    var dataType = MapBusinessDataTypeToSqlType(c.BusinessDataType, c.ColumnName);
                    _logger.LogDebug("Column {Column}: BusinessDataType='{BusinessType}' -> MappedType='{MappedType}'",
                        c.ColumnName, c.BusinessDataType ?? "NULL", dataType);

                    return new BusinessColumnInfo
                    {
                        Id = c.Id,
                        ColumnName = c.ColumnName,
                        DataType = dataType,
                    BusinessName = GetFirstAlias(c.NaturalLanguageAliases) ?? c.ColumnName,
                    BusinessMeaning = c.BusinessMeaning,
                    BusinessPurpose = c.BusinessMeaning,
                    BusinessContext = c.BusinessContext,
                    SemanticContext = c.SemanticTags ?? string.Empty,
                    DataExamples = ParseCommaSeparatedString(c.DataExamples),
                    SampleValues = ParseCommaSeparatedString(c.ValueExamples),
                    IsKey = c.IsKeyColumn,
                    IsKeyColumn = c.IsKeyColumn,
                    IsRequired = false, // Default since we don't have nullable info
                    ValidationRules = c.ValidationRules,
                    RelevanceScore = (double)c.SemanticRelevanceScore
                    };
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

    /// <summary>
    /// Find comprehensive glossary terms from multiple sources
    /// </summary>
    private async Task<List<BusinessGlossaryDto>> FindComprehensiveGlossaryTermsAsync(
        BusinessContextProfile profile,
        List<BusinessTableInfoDto> relevantTables,
        Dictionary<long, List<BusinessColumnInfo>> tableColumns)
    {
        var allGlossaryTerms = new List<BusinessGlossaryDto>();

        try
        {
            // 1. Get glossary terms from business context profile
            if (profile.BusinessTerms?.Any() == true)
            {
                var profileTerms = await FindRelevantGlossaryTermsAsync(profile.BusinessTerms);
                allGlossaryTerms.AddRange(profileTerms);
                _logger.LogDebug("Found {Count} glossary terms from business context profile", profileTerms.Count);
            }

            // 2. Get glossary terms related to table domains and contexts
            var tableRelatedTerms = await FindTableRelatedGlossaryTermsAsync(relevantTables);
            allGlossaryTerms.AddRange(tableRelatedTerms);
            _logger.LogDebug("Found {Count} glossary terms from table contexts", tableRelatedTerms.Count);

            // 3. Get glossary terms related to column business meanings and contexts
            var columnRelatedTerms = await FindColumnRelatedGlossaryTermsAsync(tableColumns);
            allGlossaryTerms.AddRange(columnRelatedTerms);
            _logger.LogDebug("Found {Count} glossary terms from column contexts", columnRelatedTerms.Count);

            // 4. Get domain-specific glossary terms
            var domainTerms = await FindDomainSpecificGlossaryTermsAsync(profile.Domain?.Name);
            allGlossaryTerms.AddRange(domainTerms);
            _logger.LogDebug("Found {Count} glossary terms from domain context", domainTerms.Count);

            // Remove duplicates and return
            var uniqueTerms = allGlossaryTerms
                .GroupBy(t => t.Term)
                .Select(g => g.First())
                .OrderBy(t => t.Term)
                .ToList();

            _logger.LogInformation("Found {Total} total glossary terms ({Unique} unique) for business metadata context",
                allGlossaryTerms.Count, uniqueTerms.Count);

            return uniqueTerms;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error finding comprehensive glossary terms");
            return new List<BusinessGlossaryDto>();
        }
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



    /// <summary>
    /// Map business data types to appropriate SQL data types based on column name patterns
    /// </summary>
    private string MapBusinessDataTypeToSqlType(string? businessDataType, string columnName)
    {
        var columnLower = columnName.ToLowerInvariant();
        var businessTypeLower = (businessDataType ?? "").ToLowerInvariant().Trim();

        // If business data type is empty or null, use column name patterns
        if (string.IsNullOrEmpty(businessTypeLower))
        {
            return GetDataTypeFromColumnName(columnName);
        }

        // Map based on the actual business data types found in the database
        return businessTypeLower switch
        {
            "integer" => columnLower.Contains("id") ? "bigint" : "int",
            "decimal" => GetDecimalDataType(columnName),
            "text" => GetTextDataType(columnName),
            "boolean" or "bit" => "bit",
            "datetime" or "date" => "datetime2",
            "currency" or "money" => "decimal(18,2)",
            "guid" or "identifier" => "uniqueidentifier",
            "number" => columnLower.Contains("id") ? "bigint" : "int",
            "string" => GetTextDataType(columnName),
            _ => GetDataTypeFromColumnName(columnName)
        };
    }

    /// <summary>
    /// Determine decimal data type based on column name patterns
    /// </summary>
    private string GetDecimalDataType(string columnName)
    {
        var columnLower = columnName.ToLowerInvariant();

        // Exchange rates and financial calculations need high precision
        if (columnLower.Contains("rate") || columnLower.Contains("exchange"))
            return "decimal(19,4)";
        if (columnLower.Contains("amount") || columnLower.Contains("deposit") || columnLower.Contains("balance") ||
            columnLower.Contains("value") || columnLower.Contains("price") || columnLower.Contains("cost"))
            return "decimal(18,2)";
        if (columnLower.Contains("percentage") || columnLower.Contains("percent") || columnLower.Contains("score"))
            return "decimal(5,4)";

        return "decimal(18,2)"; // Default for decimal
    }

    /// <summary>
    /// Determine text data type based on column name patterns
    /// </summary>
    private string GetTextDataType(string columnName)
    {
        var columnLower = columnName.ToLowerInvariant();

        if (columnLower.Contains("description") || columnLower.Contains("comment") || columnLower.Contains("notes"))
            return "nvarchar(max)";
        if (columnLower.Contains("name") || columnLower.Contains("title"))
            return "nvarchar(255)";
        if (columnLower.Contains("code") || columnLower.Contains("key"))
            return "nvarchar(50)";
        if (columnLower.Contains("email"))
            return "nvarchar(320)";
        if (columnLower.Contains("phone"))
            return "nvarchar(20)";

        return "nvarchar(255)"; // Default for text
    }

    /// <summary>
    /// Determine data type based on column name patterns when business type is unknown
    /// </summary>
    private string GetDataTypeFromColumnName(string columnName)
    {
        var columnLower = columnName.ToLowerInvariant();

        // ID patterns
        if (columnLower.EndsWith("id") || columnLower.StartsWith("id") || columnLower == "id")
            return "bigint";

        // Money/Amount patterns
        if (columnLower.Contains("amount") || columnLower.Contains("deposit") || columnLower.Contains("balance") ||
            columnLower.Contains("value") || columnLower.Contains("price") || columnLower.Contains("cost"))
            return "decimal(18,2)";

        // Date patterns
        if (columnLower.Contains("date") || columnLower.Contains("time") || columnLower.Contains("created") ||
            columnLower.Contains("updated") || columnLower.Contains("modified"))
            return "datetime2";

        // Boolean patterns
        if (columnLower.StartsWith("is") || columnLower.StartsWith("has") || columnLower.StartsWith("can") ||
            columnLower.Contains("active") || columnLower.Contains("enabled"))
            return "bit";

        // Count/Number patterns
        if (columnLower.Contains("count") || columnLower.Contains("number") || columnLower.Contains("qty") ||
            columnLower.Contains("quantity"))
            return "int";

        // Default to text
        return "nvarchar(255)";
    }

    /// <summary>
    /// Find glossary terms related to table domains and business contexts
    /// </summary>
    private async Task<List<BusinessGlossaryDto>> FindTableRelatedGlossaryTermsAsync(List<BusinessTableInfoDto> tables)
    {
        var terms = new List<BusinessGlossaryDto>();

        foreach (var table in tables)
        {
            try
            {
                // Search by domain classification
                if (!string.IsNullOrEmpty(table.DomainClassification))
                {
                    var domainTerms = await _glossaryService.GetGlossaryTermsByCategoryAsync(table.DomainClassification);
                    terms.AddRange(domainTerms);
                }

                // Search by business purpose keywords
                if (!string.IsNullOrEmpty(table.BusinessPurpose))
                {
                    var keywords = ExtractKeywords(table.BusinessPurpose);
                    foreach (var keyword in keywords)
                    {
                        var keywordTerms = await _glossaryService.SearchGlossaryTermsAsync(keyword);
                        terms.AddRange(keywordTerms);
                    }
                }

                // Search by natural language aliases
                if (table.NaturalLanguageAliases?.Any() == true)
                {
                    foreach (var alias in table.NaturalLanguageAliases)
                    {
                        var aliasTerms = await _glossaryService.SearchGlossaryTermsAsync(alias);
                        terms.AddRange(aliasTerms);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error finding glossary terms for table {Schema}.{Table}",
                    table.SchemaName, table.TableName);
            }
        }

        return terms;
    }

    /// <summary>
    /// Find glossary terms related to column business meanings and contexts
    /// </summary>
    private async Task<List<BusinessGlossaryDto>> FindColumnRelatedGlossaryTermsAsync(
        Dictionary<long, List<BusinessColumnInfo>> tableColumns)
    {
        var terms = new List<BusinessGlossaryDto>();

        foreach (var columnList in tableColumns.Values)
        {
            foreach (var column in columnList)
            {
                try
                {
                    // Search by business meaning
                    if (!string.IsNullOrEmpty(column.BusinessMeaning))
                    {
                        var keywords = ExtractKeywords(column.BusinessMeaning);
                        foreach (var keyword in keywords)
                        {
                            var meaningTerms = await _glossaryService.SearchGlossaryTermsAsync(keyword);
                            terms.AddRange(meaningTerms);
                        }
                    }

                    // Search by related business terms
                    if (!string.IsNullOrEmpty(column.RelatedBusinessTerms))
                    {
                        var relatedTerms = column.RelatedBusinessTerms.Split(',', StringSplitOptions.RemoveEmptyEntries);
                        foreach (var term in relatedTerms)
                        {
                            var glossaryTerms = await _glossaryService.SearchGlossaryTermsAsync(term.Trim());
                            terms.AddRange(glossaryTerms);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error finding glossary terms for column {Column}", column.ColumnName);
                }
            }
        }

        return terms;
    }

    /// <summary>
    /// Find domain-specific glossary terms
    /// </summary>
    private async Task<List<BusinessGlossaryDto>> FindDomainSpecificGlossaryTermsAsync(string? domainName)
    {
        if (string.IsNullOrEmpty(domainName))
            return new List<BusinessGlossaryDto>();

        try
        {
            // Get all terms for the specific domain
            var domainTerms = await _glossaryService.GetGlossaryTermsByCategoryAsync(domainName);

            // Also search for common gaming/financial terms if relevant
            var additionalTerms = new List<BusinessGlossaryDto>();
            if (domainName.Contains("Gaming", StringComparison.OrdinalIgnoreCase))
            {
                var gamingTerms = await _glossaryService.GetGlossaryTermsByCategoryAsync("Gaming");
                additionalTerms.AddRange(gamingTerms);
            }
            if (domainName.Contains("Finance", StringComparison.OrdinalIgnoreCase))
            {
                var financeTerms = await _glossaryService.GetGlossaryTermsByCategoryAsync("Finance");
                additionalTerms.AddRange(financeTerms);
            }

            domainTerms.AddRange(additionalTerms);
            return domainTerms;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error finding domain-specific glossary terms for domain {Domain}", domainName);
            return new List<BusinessGlossaryDto>();
        }
    }

    /// <summary>
    /// Extract keywords from text for glossary term searching
    /// </summary>
    private List<string> ExtractKeywords(string text)
    {
        if (string.IsNullOrEmpty(text))
            return new List<string>();

        // Simple keyword extraction - split by common delimiters and filter
        var keywords = text
            .Split(new[] { ' ', ',', '.', ';', ':', '-', '(', ')', '[', ']' }, StringSplitOptions.RemoveEmptyEntries)
            .Where(word => word.Length > 3) // Filter out short words
            .Where(word => !IsCommonWord(word)) // Filter out common words
            .Select(word => word.Trim())
            .Distinct()
            .ToList();

        return keywords;
    }

    /// <summary>
    /// Check if a word is a common word that should be filtered out
    /// </summary>
    private bool IsCommonWord(string word)
    {
        var commonWords = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "the", "and", "for", "are", "but", "not", "you", "all", "can", "had", "her", "was", "one", "our", "out", "day", "get", "has", "him", "his", "how", "its", "may", "new", "now", "old", "see", "two", "who", "boy", "did", "man", "men", "put", "say", "she", "too", "use"
        };

        return commonWords.Contains(word);
    }

    /// <summary>
    /// Parse comma-separated string values based on the actual data format
    /// Examples from your data:
    /// - "1=EUR, 2=USD, 3=GBP, 4=CAD, 5=AUD"
    /// - "currency id, currency identifier, currency key, currency number, money id"
    /// - "primary_key,identifier,reference_data,master_data,financial"
    /// </summary>
    private List<string> ParseCommaSeparatedString(string? value)
    {
        if (string.IsNullOrEmpty(value))
            return new List<string>();

        try
        {
            // Split by comma and clean up each item
            var items = value
                .Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.Trim())
                .Where(s => !string.IsNullOrEmpty(s))
                .ToList();

            _logger.LogDebug("Parsed '{Value}' into {Count} items: [{Items}]",
                value, items.Count, string.Join(", ", items.Take(3)) + (items.Count > 3 ? "..." : ""));

            return items;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to parse comma-separated value: '{Value}'", value);
            return new List<string> { value }; // Return as single item if parsing fails
        }
    }

    /// <summary>
    /// Get the first alias from a comma-separated string of natural language aliases
    /// Example: "currency id, currency identifier, currency key" -> "currency id"
    /// </summary>
    private string? GetFirstAlias(string? aliases)
    {
        if (string.IsNullOrEmpty(aliases))
            return null;

        var firstAlias = aliases
            .Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
            .FirstOrDefault()?.Trim();

        return string.IsNullOrEmpty(firstAlias) ? null : firstAlias;
    }
}
