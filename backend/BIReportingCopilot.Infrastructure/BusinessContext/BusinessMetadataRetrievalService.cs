using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using BIReportingCopilot.Core.Interfaces;
using BIReportingCopilot.Core.Interfaces.BusinessContext;
using BIReportingCopilot.Core.Interfaces.Business;
using BIReportingCopilot.Core.Models.BusinessContext;
using BIReportingCopilot.Core.Models;
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
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<BusinessMetadataRetrievalService> _logger;

    public BusinessMetadataRetrievalService(
        IBusinessTableManagementService businessTableService,
        IGlossaryManagementService glossaryService,
        ISemanticMatchingService semanticMatchingService,
        IServiceProvider serviceProvider,
        ILogger<BusinessMetadataRetrievalService> logger)
    {
        _businessTableService = businessTableService;
        _glossaryService = glossaryService;
        _semanticMatchingService = semanticMatchingService;
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task<ContextualBusinessSchema> GetRelevantBusinessMetadataAsync(
        BusinessContextProfile profile,
        int maxTables = 5)
    {
        try
        {
            // Add service identification and comprehensive debug logging
            _logger.LogInformation("🔍 [ENHANCED-SCHEMA-SERVICE] Starting metadata retrieval for query: {Query}",
                profile.OriginalQuestion);

            _logger.LogInformation("📊 [PROFILE-DEBUG] Intent: {Intent}, Domain: {Domain}, DomainScore: {DomainScore:F2}, Terms: {Terms}",
                profile.Intent.Type, profile.Domain.Name, profile.Domain.RelevanceScore, string.Join(", ", profile.BusinessTerms));

            // Log domain metadata for debugging
            if (profile.Domain.Metadata != null && profile.Domain.Metadata.Any())
            {
                _logger.LogDebug("🔍 [DOMAIN-METADATA] Domain detection metadata: {Metadata}",
                    string.Join(", ", profile.Domain.Metadata.Select(kvp => $"{kvp.Key}={kvp.Value}")));
            }

            _logger.LogInformation("🎯 [PROFILE-DEBUG] Entities: {Entities}, Metrics: {Metrics}, Dimensions: {Dimensions}",
                string.Join(", ", profile.Entities.Select(e => e.Name)),
                string.Join(", ", profile.IdentifiedMetrics),
                string.Join(", ", profile.IdentifiedDimensions));

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
                    _logger.LogWarning("🚨 [MAIN-COLUMN-DEBUG] About to call FindRelevantColumnsAsync for table ID {TableId}", tableId);
                    var columns = await FindRelevantColumnsAsync(new List<long> { tableId }, profile);
                    tableColumns[tableId] = columns;
                    _logger.LogWarning("🚨 [MAIN-COLUMN-DEBUG] FindRelevantColumnsAsync returned {ColumnCount} columns for table ID {TableId}", columns.Count, tableId);
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

            // Enhanced service completion logging
            _logger.LogInformation("✅ [ENHANCED-SCHEMA-SERVICE] Enhanced schema contextualization complete - Tables: {TableCount}, Columns: {ColumnCount}",
                result.RelevantTables.Count,
                result.TableColumns.Values.SelectMany(c => c).Count());

            // Log table details for debugging
            foreach (var table in result.RelevantTables)
            {
                _logger.LogDebug("📋 [ENHANCED-SCHEMA-SERVICE] Selected table: {Schema}.{Table} - {BusinessName}",
                    table.SchemaName, table.TableName, table.BusinessName);
            }

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
            _logger.LogInformation("🔍 Starting intelligent table search for profile: {Intent} - {Domain}",
                profile.Intent.Type, profile.Domain.Name);

            _logger.LogError("🚨 [DEBUG-TEST] Code execution reached line 164 - about to start intelligent search strategy");

            // INTELLIGENT SEARCH STRATEGY based on query classification
            var allTables = new List<BusinessTableInfoDto>();
            try
            {
                _logger.LogWarning("🚨 [STEP-1] About to call ClassifyQuery");
                var queryClassification = ClassifyQuery(profile.OriginalQuestion, profile);
                _logger.LogWarning("🚨 [STEP-2] ClassifyQuery completed, result: {Category}", queryClassification.Category);

                _logger.LogWarning("🚨 [STEP-3] About to execute intelligent search strategy for {Category} query", queryClassification.Category);
                allTables = await ExecuteIntelligentSearchStrategy(profile, queryClassification, maxTables);
                _logger.LogWarning("🚨 [STEP-4] ExecuteIntelligentSearchStrategy completed");

                _logger.LogWarning("🚨 [INTELLIGENT-SEARCH-DEBUG] Intelligent search strategy returned {Count} tables", allTables.Count);
                _logger.LogInformation("🎯 [INTELLIGENT-SEARCH] Found {Count} tables using intelligent strategy", allTables.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ [INTELLIGENT-SEARCH-ERROR] Intelligent search strategy failed, will use fallback");
                allTables = new List<BusinessTableInfoDto>(); // Reset to empty to trigger fallback
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
            foreach (var entity in profile.Entities.Where(e => e.Type == BIReportingCopilot.Core.Models.BusinessContext.EntityType.Table))
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
                _logger.LogWarning("⚠️ [FALLBACK-DEBUG] No tables found through specific searches, using fallback to get all active tables");
                _logger.LogInformation("🔍 [FALLBACK-DEBUG] Query: {Query}, Intent: {Intent}, Domain: {Domain}",
                    profile.OriginalQuestion, profile.Intent.Type, profile.Domain.Name);

                try
                {
                    var allActiveTables = await _businessTableService.GetBusinessTablesAsync();
                    _logger.LogInformation("📊 [FALLBACK-DEBUG] Retrieved {TotalCount} active tables from database", allActiveTables.Count);

                    // Apply domain-specific filtering even in fallback
                    var filteredTables = ApplyQuerySpecificFallbackFiltering(allActiveTables, profile);
                    _logger.LogInformation("🔍 [FALLBACK-DEBUG] After query-specific filtering: {FilteredCount} tables (from {OriginalCount})",
                        filteredTables.Count, allActiveTables.Count);

                    allTables.AddRange(filteredTables.Take(maxTables * 2)); // Limit fallback
                    _logger.LogWarning("⚠️ [FALLBACK-DEBUG] Added {Count} tables via enhanced fallback (limited from {Total})",
                        allTables.Count, filteredTables.Count);

                    // Log table names for debugging
                    _logger.LogDebug("📋 [FALLBACK-DEBUG] Enhanced fallback tables: {Tables}",
                        string.Join(", ", allTables.Select(t => t.TableName)));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "❌ Even fallback table retrieval failed");
                    throw;
                }
            }

            // CRITICAL FIX: Proper deduplication by table name and schema
            // The Distinct() method doesn't work because BusinessTableInfoDto doesn't implement proper equality
            var uniqueTables = allTables
                .GroupBy(t => new { t.TableName, t.SchemaName })
                .Select(g => g.First()) // Take first occurrence of each unique table
                .ToList();

            _logger.LogInformation("🔍 [DEDUPLICATION-FIX] Deduplicated {OriginalCount} tables to {UniqueCount} unique tables",
                allTables.Count, uniqueTables.Count);

            // Log duplicate detection details for debugging
            var duplicateGroups = allTables
                .GroupBy(t => new { t.TableName, t.SchemaName })
                .Where(g => g.Count() > 1)
                .ToList();

            if (duplicateGroups.Any())
            {
                _logger.LogWarning("🚨 [DUPLICATE-DETECTION] Found {DuplicateGroupCount} tables with duplicates:",
                    duplicateGroups.Count);
                foreach (var group in duplicateGroups)
                {
                    _logger.LogWarning("   📋 {Schema}.{Table}: {Count} duplicates",
                        group.Key.SchemaName, group.Key.TableName, group.Count());
                }
            }

            // Score and rank unique tables only
            _logger.LogInformation("🔍 [SCORING-DEBUG] Scoring and ranking {Count} unique tables", uniqueTables.Count);
            var scoredTables = await ScoreTablesAsync(uniqueTables, profile);

            // Log all scored tables for debugging
            _logger.LogInformation("📊 [SCORING-DEBUG] All table scores:");
            foreach (var (table, score) in scoredTables.OrderByDescending(t => t.RelevanceScore))
            {
                _logger.LogInformation("   📋 {Schema}.{Table}: {Score:F2}",
                    table.SchemaName, table.TableName, score);
            }

            // Return top tables
            var result = scoredTables
                .OrderByDescending(t => t.RelevanceScore)
                .Take(maxTables)
                .Select(t => t.Table)
                .ToList();

            _logger.LogInformation("✅ [FINAL-RESULT] Selected {ResultCount} unique tables from {TotalCount} candidates: {SelectedTables}",
                result.Count, uniqueTables.Count,
                string.Join(", ", result.Select(t => $"{t.SchemaName}.{t.TableName}")));

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error in FindRelevantTablesAsync: {ErrorMessage}", ex.Message);
            throw;
        }
    }

    /// <summary>
    /// Execute intelligent search strategy based on query classification
    /// </summary>
    private async Task<List<BusinessTableInfoDto>> ExecuteIntelligentSearchStrategy(
        BusinessContextProfile profile,
        QueryClassificationResult classification,
        int maxTables)
    {
        var allTables = new List<BusinessTableInfoDto>();

        _logger.LogInformation("🎯 [SEARCH-STRATEGY] Executing {Category} search strategy (Confidence: {Confidence:F2})",
            classification.Category, classification.Confidence);

        // ENHANCED DEBUG: Log classification details
        _logger.LogWarning("🚨 [CLASSIFICATION-DEBUG] Query: '{Query}' classified as {Category} with confidence {Confidence:F2}",
            profile.OriginalQuestion, classification.Category, classification.Confidence);

        _logger.LogWarning("🚨 [CLASSIFICATION-DEBUG] Category scores: {Scores}",
            string.Join(", ", classification.CategoryScores.Select(kvp => $"{kvp.Key}:{kvp.Value:F1}")));

        // Apply domain-specific search strategies
        switch (classification.Category)
        {
            case QueryCategory.Financial:
                _logger.LogWarning("💰 [FINANCIAL-STRATEGY] Executing financial search strategy for deposit query");
                allTables.AddRange(await ExecuteFinancialSearchStrategy(profile, classification, maxTables));
                break;

            case QueryCategory.Gaming:
                _logger.LogWarning("🎮 [GAMING-STRATEGY] Executing gaming search strategy");
                allTables.AddRange(await ExecuteGamingSearchStrategy(profile, classification, maxTables));
                break;

            case QueryCategory.Analytics:
                _logger.LogWarning("📊 [ANALYTICS-STRATEGY] Executing analytics search strategy");
                allTables.AddRange(await ExecuteAnalyticsSearchStrategy(profile, classification, maxTables));
                break;

            default:
                _logger.LogWarning("🔍 [GENERIC-STRATEGY] Executing generic search strategy");
                allTables.AddRange(await ExecuteGenericSearchStrategy(profile, classification, maxTables));
                break;
        }

        _logger.LogWarning("🚨 [PRE-FILTER-DEBUG] Found {Count} tables before filtering: {Tables}",
            allTables.Count, string.Join(", ", allTables.Select(t => t.TableName)));

        // Apply domain-specific table filtering
        var filteredTables = ApplyDomainSpecificFiltering(allTables, classification);

        _logger.LogWarning("🚨 [POST-FILTER-DEBUG] Found {Count} tables after filtering: {Tables}",
            filteredTables.Count, string.Join(", ", filteredTables.Select(t => t.TableName)));

        _logger.LogInformation("📊 [SEARCH-STRATEGY] Strategy result: {Original} → {Filtered} tables",
            allTables.Count, filteredTables.Count);

        return filteredTables;
    }

    /// <summary>
    /// Execute financial-specific search strategy
    /// </summary>
    private async Task<List<BusinessTableInfoDto>> ExecuteFinancialSearchStrategy(
        BusinessContextProfile profile,
        QueryClassificationResult classification,
        int maxTables)
    {
        var tables = new List<BusinessTableInfoDto>();

        _logger.LogWarning("🚨 [FINANCIAL-SEARCH-DEBUG] Starting financial search strategy execution");
        _logger.LogDebug("💰 [FINANCIAL-SEARCH] Executing financial search strategy");

        // Priority 1: Direct financial table search
        var financialTerms = new[] { "deposit", "transaction", "payment", "financial", "revenue", "balance" };
        foreach (var term in financialTerms.Take(3))
        {
            try
            {
                var termTables = await _businessTableService.SearchBusinessTablesAsync(term);
                tables.AddRange(termTables);
                _logger.LogDebug("💰 Found {Count} tables for financial term: {Term}", termTables.Count, term);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "⚠️ Financial term search failed for: {Term}", term);
            }
        }

        // Priority 2: Customer/Player tables (important for financial analysis)
        var customerTerms = new[] { "player", "user", "customer" };
        foreach (var term in customerTerms.Take(2))
        {
            try
            {
                var customerTables = await _businessTableService.SearchBusinessTablesAsync(term);
                tables.AddRange(customerTables);
                _logger.LogDebug("👤 Found {Count} customer tables for term: {Term}", customerTables.Count, term);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "⚠️ Customer term search failed for: {Term}", term);
            }
        }

        // Priority 3: Geographic tables (common in financial analysis)
        try
        {
            var geoTables = await _businessTableService.SearchBusinessTablesAsync("country");
            tables.AddRange(geoTables);
            _logger.LogDebug("🌍 Found {Count} geographic tables", geoTables.Count);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "⚠️ Geographic table search failed");
        }

        // Fallback: Semantic search
        await AddSemanticSearchResults(tables, profile, maxTables);

        _logger.LogWarning("🚨 [FINANCIAL-SEARCH-DEBUG] Financial search strategy completed with {Count} tables", tables.Count);
        if (tables.Any())
        {
            _logger.LogWarning("🚨 [FINANCIAL-SEARCH-DEBUG] Found tables: {Tables}",
                string.Join(", ", tables.Select(t => t.TableName)));
        }

        return tables;
    }

    /// <summary>
    /// Execute gaming-specific search strategy
    /// </summary>
    private async Task<List<BusinessTableInfoDto>> ExecuteGamingSearchStrategy(
        BusinessContextProfile profile,
        QueryClassificationResult classification,
        int maxTables)
    {
        var tables = new List<BusinessTableInfoDto>();

        _logger.LogDebug("🎮 [GAMING-SEARCH] Executing gaming search strategy");

        // Gaming-specific terms
        var gamingTerms = new[] { "game", "games", "gaming", "player", "session" };
        foreach (var term in gamingTerms.Take(3))
        {
            try
            {
                var termTables = await _businessTableService.SearchBusinessTablesAsync(term);
                tables.AddRange(termTables);
                _logger.LogDebug("🎮 Found {Count} tables for gaming term: {Term}", termTables.Count, term);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "⚠️ Gaming term search failed for: {Term}", term);
            }
        }

        await AddSemanticSearchResults(tables, profile, maxTables);
        return tables;
    }

    /// <summary>
    /// Execute analytics-specific search strategy
    /// </summary>
    private async Task<List<BusinessTableInfoDto>> ExecuteAnalyticsSearchStrategy(
        BusinessContextProfile profile,
        QueryClassificationResult classification,
        int maxTables)
    {
        var tables = new List<BusinessTableInfoDto>();

        _logger.LogDebug("📊 [ANALYTICS-SEARCH] Executing analytics search strategy");

        // Analytics-specific terms
        var analyticsTerms = new[] { "metric", "kpi", "analytics", "dashboard", "report" };
        foreach (var term in analyticsTerms.Take(3))
        {
            try
            {
                var termTables = await _businessTableService.SearchBusinessTablesAsync(term);
                tables.AddRange(termTables);
                _logger.LogDebug("📊 Found {Count} tables for analytics term: {Term}", termTables.Count, term);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "⚠️ Analytics term search failed for: {Term}", term);
            }
        }

        await AddSemanticSearchResults(tables, profile, maxTables);
        return tables;
    }

    /// <summary>
    /// Execute generic search strategy for unknown categories
    /// </summary>
    private async Task<List<BusinessTableInfoDto>> ExecuteGenericSearchStrategy(
        BusinessContextProfile profile,
        QueryClassificationResult classification,
        int maxTables)
    {
        var tables = new List<BusinessTableInfoDto>();

        _logger.LogDebug("🔍 [GENERIC-SEARCH] Executing generic search strategy");

        // Use business terms from profile
        foreach (var term in profile.BusinessTerms.Take(3))
        {
            try
            {
                var termTables = await _businessTableService.SearchBusinessTablesAsync(term);
                tables.AddRange(termTables);
                _logger.LogDebug("🔍 Found {Count} tables for business term: {Term}", termTables.Count, term);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "⚠️ Business term search failed for: {Term}", term);
            }
        }

        await AddSemanticSearchResults(tables, profile, maxTables);
        return tables;
    }

    /// <summary>
    /// Add semantic search results as fallback
    /// </summary>
    private async Task AddSemanticSearchResults(List<BusinessTableInfoDto> tables,
        BusinessContextProfile profile, int maxTables)
    {
        try
        {
            _logger.LogDebug("🔍 Adding semantic search results as fallback");
            var semanticTables = await _semanticMatchingService.SemanticTableSearchAsync(
                profile.OriginalQuestion, maxTables * 2);
            tables.AddRange(semanticTables);
            _logger.LogDebug("✅ Semantic search found {Count} additional tables", semanticTables.Count);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "⚠️ Semantic table search failed");
        }
    }

    /// <summary>
    /// Apply domain-specific filtering to remove irrelevant tables
    /// </summary>
    private List<BusinessTableInfoDto> ApplyDomainSpecificFiltering(
        List<BusinessTableInfoDto> tables,
        QueryClassificationResult classification)
    {
        var filteredTables = new List<BusinessTableInfoDto>();

        _logger.LogWarning("🚨 [DOMAIN-FILTER-DEBUG] Applying {Category} domain filtering to {Count} tables",
            classification.Category, tables.Count);

        // CRITICAL FIX: Proper deduplication before filtering
        var uniqueTables = tables
            .GroupBy(t => new { t.TableName, t.SchemaName })
            .Select(g => g.First())
            .ToList();

        _logger.LogWarning("🔧 [DOMAIN-FILTER-DEBUG] Deduplicated {OriginalCount} to {UniqueCount} tables before filtering",
            tables.Count, uniqueTables.Count);

        foreach (var table in uniqueTables)
        {
            bool shouldInclude = true;
            string exclusionReason = "";

            _logger.LogWarning("🔍 [DOMAIN-FILTER-DEBUG] Evaluating table: {Table}", table.TableName);

            // Apply category-specific filtering
            switch (classification.Category)
            {
                case QueryCategory.Financial:
                    _logger.LogWarning("💰 [FINANCIAL-FILTER] Checking if {Table} is gaming table for financial query", table.TableName);
                    if (IsGamingTableForFinancialQuery(table))
                    {
                        shouldInclude = false;
                        exclusionReason = "Gaming table excluded from financial query";
                        _logger.LogWarning("🚫 [FINANCIAL-FILTER] EXCLUDED gaming table: {Table}", table.TableName);
                    }
                    else
                    {
                        _logger.LogWarning("✅ [FINANCIAL-FILTER] INCLUDED financial table: {Table}", table.TableName);
                    }
                    break;

                case QueryCategory.Gaming:
                    _logger.LogWarning("🎮 [GAMING-FILTER] Gaming queries include most tables: {Table}", table.TableName);
                    // Gaming queries can include most tables
                    break;

                case QueryCategory.Analytics:
                    _logger.LogWarning("📊 [ANALYTICS-FILTER] Analytics queries are broad: {Table}", table.TableName);
                    // Analytics queries are broad, minimal filtering
                    break;

                default:
                    _logger.LogWarning("🔍 [DEFAULT-FILTER] Default filtering for: {Table}", table.TableName);
                    break;
            }

            if (shouldInclude)
            {
                filteredTables.Add(table);
                _logger.LogWarning("✅ [DOMAIN-FILTER] INCLUDED: {Table}", table.TableName);
            }
            else
            {
                _logger.LogWarning("❌ [DOMAIN-FILTER] EXCLUDED: {Table} - {Reason}", table.TableName, exclusionReason);
            }
        }

        _logger.LogInformation("📊 [DOMAIN-FILTER] Filtering complete: {Included}/{Total} tables included",
            filteredTables.Count, uniqueTables.Count);

        return filteredTables;
    }

    public async Task<List<BusinessColumnInfo>> FindRelevantColumnsAsync(
        List<long> tableIds,
        BusinessContextProfile profile)
    {
        _logger.LogWarning("🚨 [ENHANCED-COLUMN-DEBUG] FindRelevantColumnsAsync called for {TableCount} tables: {TableIds}",
            tableIds.Count, string.Join(", ", tableIds));

        _logger.LogInformation("🔍 [OPTIMIZED-COLUMNS] Starting optimized column retrieval for {TableCount} tables", tableIds.Count);

        // ENHANCED QUERY CLASSIFICATION for column selection
        var queryClassification = ClassifyQuery(profile.OriginalQuestion, profile);

        _logger.LogWarning("🚨 [ENHANCED-COLUMN-DEBUG] Query classification: {Category}, Confidence: {Confidence}",
            queryClassification.Category, queryClassification.IsHighConfidence ? "High" : queryClassification.IsMediumConfidence ? "Medium" : "Low");

        // PERFORMANCE OPTIMIZATION: Batch processing for multiple tables
        var allColumns = await ProcessTablesInBatches(tableIds, queryClassification, profile);

        _logger.LogWarning("🚨 [ENHANCED-COLUMN-DEBUG] Batch processing returned {ColumnCount} columns from {TableCount} tables",
            allColumns.Count, tableIds.Count);

        _logger.LogInformation("⚡ [PERFORMANCE] Batch processing complete: {ColumnCount} columns from {TableCount} tables",
            allColumns.Count, tableIds.Count);

        // INTELLIGENT COLUMN FILTERING with query-intent-based scoring
        var relevantColumns = await ApplyIntelligentColumnFiltering(allColumns, queryClassification, profile);

        _logger.LogWarning("🚨 [ENHANCED-COLUMN-DEBUG] Final result: {Selected}/{Total} columns selected",
            relevantColumns.Count, allColumns.Count);

        _logger.LogInformation("✅ [SELECTIVE-COLUMNS] Column filtering complete: {Selected}/{Total} columns selected",
            relevantColumns.Count, allColumns.Count);

        return relevantColumns;
    }

    /// <summary>
    /// Process tables in batches for optimal performance
    /// </summary>
    private async Task<List<BusinessColumnInfo>> ProcessTablesInBatches(
        List<long> tableIds,
        QueryClassificationResult classification,
        BusinessContextProfile profile)
    {
        const int batchSize = 5; // Process 5 tables at a time
        var allColumns = new List<BusinessColumnInfo>();

        _logger.LogDebug("⚡ [BATCH-PROCESSING] Processing {TableCount} tables in batches of {BatchSize}",
            tableIds.Count, batchSize);

        // Process tables in batches to avoid overwhelming the database
        for (int i = 0; i < tableIds.Count; i += batchSize)
        {
            var batch = tableIds.Skip(i).Take(batchSize).ToList();
            _logger.LogDebug("📦 [BATCH-PROCESSING] Processing batch {BatchNumber}: {TableIds}",
                (i / batchSize) + 1, string.Join(", ", batch));

            // Process batch with intelligent caching
            var batchColumns = await ProcessTableBatchWithCaching(batch, classification, profile);
            allColumns.AddRange(batchColumns);

            _logger.LogDebug("✅ [BATCH-PROCESSING] Batch {BatchNumber} complete: {ColumnCount} columns",
                (i / batchSize) + 1, batchColumns.Count);
        }

        return allColumns;
    }

    /// <summary>
    /// Process a batch of tables with intelligent caching
    /// </summary>
    private async Task<List<BusinessColumnInfo>> ProcessTableBatchWithCaching(
        List<long> tableIds,
        QueryClassificationResult classification,
        BusinessContextProfile profile)
    {
        var batchColumns = new List<BusinessColumnInfo>();

        // Check cache first for each table
        var uncachedTableIds = new List<long>();
        var cacheHits = 0;

        foreach (var tableId in tableIds)
        {
            var cacheKey = GenerateTableCacheKey(tableId, classification);
            var cachedColumns = await GetCachedTableColumns(cacheKey);

            if (cachedColumns != null)
            {
                batchColumns.AddRange(cachedColumns);
                cacheHits++;
                _logger.LogDebug("💾 [CACHE-HIT] Retrieved {ColumnCount} columns for table {TableId} from cache",
                    cachedColumns.Count, tableId);
            }
            else
            {
                uncachedTableIds.Add(tableId);
            }
        }

        _logger.LogDebug("📊 [CACHE-STATS] Cache hits: {Hits}/{Total} tables", cacheHits, tableIds.Count);

        // Process uncached tables
        foreach (var tableId in uncachedTableIds)
        {
            var table = await GetTableWithSelectiveColumns(tableId, classification, profile);
            if (table?.Columns != null)
            {
                var mappedColumns = await MapColumnsWithMinimalFields(table, classification, profile);
                batchColumns.AddRange(mappedColumns);

                // Cache the results for future use
                var cacheKey = GenerateTableCacheKey(tableId, classification);
                await CacheTableColumns(cacheKey, mappedColumns);

                _logger.LogDebug("💾 [CACHE-STORE] Cached {ColumnCount} columns for table {TableId}",
                    mappedColumns.Count, tableId);
            }
        }

        return batchColumns;
    }

    /// <summary>
    /// Generate cache key for table columns based on classification
    /// </summary>
    private string GenerateTableCacheKey(long tableId, QueryClassificationResult classification)
    {
        // Include classification category and confidence in cache key for context-aware caching
        return $"table_columns_{tableId}_{classification.Category}_{(classification.IsHighConfidence ? "high" : classification.IsMediumConfidence ? "medium" : "low")}";
    }

    /// <summary>
    /// Get cached table columns (placeholder for actual cache implementation)
    /// </summary>
    private async Task<List<BusinessColumnInfo>?> GetCachedTableColumns(string cacheKey)
    {
        try
        {
            // TODO: Implement actual caching with IMemoryCache or Redis
            // For now, return null to indicate cache miss
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "⚠️ [CACHE-ERROR] Failed to retrieve from cache: {CacheKey}", cacheKey);
            return null;
        }
    }

    /// <summary>
    /// Cache table columns (placeholder for actual cache implementation)
    /// </summary>
    private async Task CacheTableColumns(string cacheKey, List<BusinessColumnInfo> columns)
    {
        try
        {
            // TODO: Implement actual caching with IMemoryCache or Redis
            // Cache for 15 minutes with sliding expiration
            _logger.LogDebug("💾 [CACHE-PLACEHOLDER] Would cache {ColumnCount} columns with key: {CacheKey}",
                columns.Count, cacheKey);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "⚠️ [CACHE-ERROR] Failed to store in cache: {CacheKey}", cacheKey);
        }
    }

    /// <summary>
    /// Get table with selective column loading based on query classification
    /// </summary>
    private async Task<BusinessTableInfoDto?> GetTableWithSelectiveColumns(
        long tableId,
        QueryClassificationResult classification,
        BusinessContextProfile profile)
    {
        try
        {
            // For now, use the existing method but we could optimize this later
            // to only load columns that match the query classification
            var table = await _businessTableService.GetBusinessTableAsync(tableId);

            if (table?.Columns != null)
            {
                // Apply pre-filtering to reduce column count early
                var preFilteredColumns = ApplyColumnPreFiltering(table.Columns, classification, profile);
                table.Columns = preFilteredColumns;

                _logger.LogDebug("🔍 [PRE-FILTER] Reduced columns from {Original} to {Filtered} for table {Table}",
                    table.Columns.Count + (preFilteredColumns.Count - table.Columns.Count),
                    preFilteredColumns.Count, table.TableName);
            }

            return table;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error loading table {TableId} with selective columns", tableId);
            return null;
        }
    }

    /// <summary>
    /// Apply column pre-filtering to reduce processing load
    /// </summary>
    private List<BusinessColumnInfoDto> ApplyColumnPreFiltering(
        List<BusinessColumnInfoDto> columns,
        QueryClassificationResult classification,
        BusinessContextProfile profile)
    {
        var queryLower = profile.OriginalQuestion.ToLowerInvariant();
        var filteredColumns = new List<BusinessColumnInfoDto>();

        foreach (var column in columns)
        {
            bool shouldInclude = false;

            // Always include key columns
            if (column.IsKeyColumn)
            {
                shouldInclude = true;
            }
            // Include columns that match query terms
            else if (ContainsRelevantTerms(column, queryLower, classification))
            {
                shouldInclude = true;
            }
            // Include high semantic relevance columns
            else if (column.SemanticRelevanceScore > 0.7)
            {
                shouldInclude = true;
            }

            if (shouldInclude)
            {
                filteredColumns.Add(column);
            }
        }

        // Ensure we have at least some columns (fallback)
        if (!filteredColumns.Any() && columns.Any())
        {
            // Include top 10 columns by semantic relevance
            filteredColumns.AddRange(columns.OrderByDescending(c => c.SemanticRelevanceScore).Take(10));
        }

        return filteredColumns;
    }

    /// <summary>
    /// Check if column contains relevant terms for the query
    /// </summary>
    private bool ContainsRelevantTerms(BusinessColumnInfoDto column, string queryLower, QueryClassificationResult classification)
    {
        var columnLower = column.ColumnName.ToLowerInvariant();
        var meaningLower = column.BusinessMeaning?.ToLowerInvariant() ?? "";
        var contextLower = column.BusinessContext?.ToLowerInvariant() ?? "";

        // Check for direct term matches
        var searchText = $"{columnLower} {meaningLower} {contextLower}";

        // Category-specific term matching
        switch (classification.Category)
        {
            case QueryCategory.Financial:
                var financialTerms = new[] { "deposit", "amount", "value", "balance", "transaction", "payment", "revenue", "cost" };
                return financialTerms.Any(term => searchText.Contains(term));

            case QueryCategory.Gaming:
                var gamingTerms = new[] { "game", "bet", "win", "loss", "session", "player", "round" };
                return gamingTerms.Any(term => searchText.Contains(term));

            default:
                // Generic term matching from query
                var queryWords = queryLower.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                return queryWords.Any(word => word.Length > 3 && searchText.Contains(word));
        }
    }

    /// <summary>
    /// Map columns with enhanced minimal field loading and lazy loading capabilities
    /// </summary>
    private async Task<List<BusinessColumnInfo>> MapColumnsWithMinimalFields(
        BusinessTableInfoDto table,
        QueryClassificationResult classification,
        BusinessContextProfile profile)
    {
        var mappedColumns = new List<BusinessColumnInfo>();

        _logger.LogDebug("🔧 [MINIMAL-LOADING] Processing {Count} columns for table {Table} with {Category} classification",
            table.Columns.Count, table.TableName, classification.Category);

        foreach (var c in table.Columns)
        {
            var dataType = MapBusinessDataTypeToSqlType(c.BusinessDataType, c.ColumnName);

            // ESSENTIAL FIELDS ONLY - Core fields required for SQL generation
            var column = new BusinessColumnInfo
            {
                Id = c.Id,
                ColumnName = c.ColumnName,
                DataType = dataType,
                BusinessName = GetFirstAlias(c.NaturalLanguageAliases) ?? c.ColumnName,
                BusinessMeaning = c.BusinessMeaning,
                BusinessPurpose = c.BusinessMeaning,
                IsKey = c.IsKeyColumn,
                IsKeyColumn = c.IsKeyColumn,
                RelevanceScore = (double)c.SemanticRelevanceScore,
                IsRequired = false // Default value
            };

            // CONDITIONAL DETAILED METADATA LOADING based on query confidence and category
            var loadingStrategy = DetermineFieldLoadingStrategy(classification, c);

            switch (loadingStrategy)
            {
                case FieldLoadingStrategy.Essential:
                    // Already loaded above - no additional fields
                    _logger.LogDebug("📋 [LOADING-STRATEGY] Essential loading for column: {Column}", c.ColumnName);
                    break;

                case FieldLoadingStrategy.Standard:
                    // Load commonly needed fields
                    column.BusinessContext = c.BusinessContext;
                    column.SemanticContext = c.SemanticTags ?? string.Empty;
                    _logger.LogDebug("📊 [LOADING-STRATEGY] Standard loading for column: {Column}", c.ColumnName);
                    break;

                case FieldLoadingStrategy.Comprehensive:
                    // Load all available metadata
                    column.BusinessContext = c.BusinessContext;
                    column.SemanticContext = c.SemanticTags ?? string.Empty;
                    column.DataExamples = ParseCommaSeparatedString(c.DataExamples);
                    column.SampleValues = ParseCommaSeparatedString(c.ValueExamples);
                    column.ValidationRules = c.ValidationRules;
                    _logger.LogDebug("🔍 [LOADING-STRATEGY] Comprehensive loading for column: {Column}", c.ColumnName);
                    break;
            }

            // LAZY LOADING MARKER - Mark fields that can be loaded later if needed
            if (loadingStrategy != FieldLoadingStrategy.Comprehensive)
            {
                // Store original column reference for potential lazy loading
                column.LazyLoadingAvailable = true;
                column.SourceColumnId = c.Id;
            }

            mappedColumns.Add(column);
        }

        _logger.LogInformation("✅ [MINIMAL-LOADING] Mapped {Count} columns with optimized field loading", mappedColumns.Count);
        return mappedColumns;
    }

    /// <summary>
    /// Field loading strategies for performance optimization
    /// </summary>
    private enum FieldLoadingStrategy
    {
        Essential,      // Only core fields needed for SQL generation
        Standard,       // Core + commonly used fields
        Comprehensive   // All available metadata
    }

    /// <summary>
    /// Determine the appropriate field loading strategy for a column
    /// </summary>
    private FieldLoadingStrategy DetermineFieldLoadingStrategy(QueryClassificationResult classification, BusinessColumnInfoDto column)
    {
        // High confidence queries get more detailed metadata
        if (classification.IsHighConfidence)
        {
            // Key columns and high relevance columns get comprehensive loading
            if (column.IsKeyColumn || column.SemanticRelevanceScore > 0.8)
            {
                return FieldLoadingStrategy.Comprehensive;
            }
            return FieldLoadingStrategy.Standard;
        }

        // Medium confidence queries get standard loading for important columns
        if (classification.IsMediumConfidence)
        {
            if (column.IsKeyColumn || column.SemanticRelevanceScore > 0.7)
            {
                return FieldLoadingStrategy.Standard;
            }
            return FieldLoadingStrategy.Essential;
        }

        // Low confidence queries get minimal loading
        if (column.IsKeyColumn)
        {
            return FieldLoadingStrategy.Standard;
        }

        return FieldLoadingStrategy.Essential;
    }

    /// <summary>
    /// Apply intelligent column filtering with query-intent-based scoring
    /// </summary>
    private async Task<List<BusinessColumnInfo>> ApplyIntelligentColumnFiltering(
        List<BusinessColumnInfo> allColumns,
        QueryClassificationResult classification,
        BusinessContextProfile profile)
    {
        var relevantColumns = new List<BusinessColumnInfo>();
        var originalQueryLower = profile.OriginalQuestion.ToLowerInvariant();

        _logger.LogInformation("🔍 [COLUMN-FILTER] Applying intelligent filtering to {TotalColumns} columns for {Category} query",
            allColumns.Count, classification.Category);

        // Score and rank columns based on query classification
        var scoredColumns = new List<(BusinessColumnInfo Column, double Score)>();

        foreach (var column in allColumns)
        {
            var score = CalculateColumnRelevanceScore(column, classification, profile, originalQueryLower);
            scoredColumns.Add((column, score));
        }

        // Sort by score and apply token budget constraints
        var sortedColumns = scoredColumns.OrderByDescending(x => x.Score).ToList();

        _logger.LogDebug("📊 [COLUMN-SCORING] Top 10 column scores:");
        foreach (var (column, score) in sortedColumns.Take(10))
        {
            _logger.LogDebug("   📋 {Column}: {Score:F2}", column.ColumnName, score);
        }

        // Apply token budget constraints (limit columns based on estimated token usage)
        var maxColumns = CalculateMaxColumnsForTokenBudget(classification, profile);
        var selectedColumns = sortedColumns.Take(maxColumns).Select(x => x.Column).ToList();

        _logger.LogWarning("🔍 [COLUMN-FILTER-DEBUG] Classification: {Category}, Confidence: {Confidence}, MaxColumns: {MaxColumns}",
            classification.Category, classification.IsHighConfidence ? "High" : classification.IsMediumConfidence ? "Medium" : "Low", maxColumns);

        _logger.LogWarning("🔍 [COLUMN-FILTER-DEBUG] Input columns: {InputCount}, Selected columns: {SelectedCount}",
            allColumns.Count, selectedColumns.Count);

        // Log top selected columns for debugging
        _logger.LogWarning("🔍 [COLUMN-FILTER-DEBUG] Top 5 selected columns:");
        foreach (var (column, score) in sortedColumns.Take(5))
        {
            _logger.LogWarning("   📋 {Column} (Score: {Score:F2})", column.ColumnName, score);
        }

        _logger.LogInformation("✅ [COLUMN-FILTER] Selected {Selected}/{Total} columns (Token budget limit: {MaxColumns})",
            selectedColumns.Count, allColumns.Count, maxColumns);

        return selectedColumns;
    }

    /// <summary>
    /// Calculate column relevance score based on query classification
    /// </summary>
    private double CalculateColumnRelevanceScore(
        BusinessColumnInfo column,
        QueryClassificationResult classification,
        BusinessContextProfile profile,
        string queryLower)
    {
        double score = 0;
        var columnLower = column.ColumnName.ToLowerInvariant();
        var businessMeaningLower = column.BusinessMeaning?.ToLowerInvariant() ?? "";

        // Base semantic relevance score
        score += column.RelevanceScore * 2.0;

        // Key column boost
        if (column.IsKeyColumn)
        {
            score += 3.0;
        }

        // Category-specific scoring
        switch (classification.Category)
        {
            case QueryCategory.Financial:
                score += CalculateFinancialColumnScore(column, columnLower, businessMeaningLower, queryLower);
                break;

            case QueryCategory.Gaming:
                score += CalculateGamingColumnScore(column, columnLower, businessMeaningLower, queryLower);
                break;

            case QueryCategory.Analytics:
                score += CalculateAnalyticsColumnScore(column, columnLower, businessMeaningLower, queryLower);
                break;

            default:
                score += CalculateGenericColumnScore(column, columnLower, businessMeaningLower, queryLower);
                break;
        }

        return score;
    }

    /// <summary>
    /// Calculate maximum columns allowed based on token budget
    /// </summary>
    private int CalculateMaxColumnsForTokenBudget(QueryClassificationResult classification, BusinessContextProfile profile)
    {
        // Base limits by query type
        var baseLimit = classification.Category switch
        {
            QueryCategory.Financial => 15,  // Financial queries need more detail
            QueryCategory.Gaming => 12,     // Gaming queries moderate detail
            QueryCategory.Analytics => 20,  // Analytics queries need comprehensive data
            QueryCategory.Operational => 8, // Operational queries need minimal data
            QueryCategory.Reporting => 18,  // Reporting queries need good coverage
            _ => 10                         // Default limit
        };

        // Adjust based on confidence
        if (classification.IsHighConfidence)
        {
            return (int)(baseLimit * 1.2); // 20% more columns for high confidence
        }
        else if (classification.IsMediumConfidence)
        {
            return baseLimit;
        }
        else
        {
            return (int)(baseLimit * 0.8); // 20% fewer columns for low confidence
        }
    }

    /// <summary>
    /// Calculate financial-specific column scoring
    /// </summary>
    private double CalculateFinancialColumnScore(BusinessColumnInfo column, string columnLower, string meaningLower, string queryLower)
    {
        double score = 0;

        // High priority financial columns
        var financialTerms = new[] { "deposit", "amount", "value", "balance", "transaction", "payment", "revenue", "cost", "price" };
        foreach (var term in financialTerms)
        {
            if (columnLower.Contains(term) || meaningLower.Contains(term))
            {
                score += 4.0;
                break;
            }
        }

        // Customer identification columns
        var customerTerms = new[] { "player", "user", "customer", "account", "id" };
        foreach (var term in customerTerms)
        {
            if (columnLower.Contains(term) || meaningLower.Contains(term))
            {
                score += 2.0;
                break;
            }
        }

        // Geographic columns
        var geoTerms = new[] { "country", "region", "location" };
        foreach (var term in geoTerms)
        {
            if (columnLower.Contains(term) || meaningLower.Contains(term))
            {
                score += 1.5;
                break;
            }
        }

        // Time-related columns
        var timeTerms = new[] { "date", "time", "created", "updated", "timestamp" };
        foreach (var term in timeTerms)
        {
            if (columnLower.Contains(term) || meaningLower.Contains(term))
            {
                score += 1.0;
                break;
            }
        }

        return score;
    }

    /// <summary>
    /// Calculate gaming-specific column scoring
    /// </summary>
    private double CalculateGamingColumnScore(BusinessColumnInfo column, string columnLower, string meaningLower, string queryLower)
    {
        double score = 0;

        // Gaming-specific columns
        var gamingTerms = new[] { "game", "bet", "win", "loss", "session", "round", "spin" };
        foreach (var term in gamingTerms)
        {
            if (columnLower.Contains(term) || meaningLower.Contains(term))
            {
                score += 4.0;
                break;
            }
        }

        // Player columns
        var playerTerms = new[] { "player", "user" };
        foreach (var term in playerTerms)
        {
            if (columnLower.Contains(term) || meaningLower.Contains(term))
            {
                score += 2.0;
                break;
            }
        }

        return score;
    }

    /// <summary>
    /// Calculate analytics-specific column scoring
    /// </summary>
    private double CalculateAnalyticsColumnScore(BusinessColumnInfo column, string columnLower, string meaningLower, string queryLower)
    {
        double score = 0;

        // Analytics/metrics columns
        var analyticsTerms = new[] { "metric", "kpi", "count", "total", "average", "sum" };
        foreach (var term in analyticsTerms)
        {
            if (columnLower.Contains(term) || meaningLower.Contains(term))
            {
                score += 3.0;
                break;
            }
        }

        return score;
    }

    /// <summary>
    /// Calculate generic column scoring
    /// </summary>
    private double CalculateGenericColumnScore(BusinessColumnInfo column, string columnLower, string meaningLower, string queryLower)
    {
        double score = 0;

        // Check for query term matches
        var queryWords = queryLower.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        foreach (var word in queryWords.Where(w => w.Length > 3))
        {
            if (columnLower.Contains(word) || meaningLower.Contains(word))
            {
                score += 1.0;
            }
        }

        return score;
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

    public async Task<List<BIReportingCopilot.Core.Models.BusinessContext.TableRelationship>> DiscoverTableRelationshipsAsync(
        List<string> tableNames)
    {
        try
        {
            _logger.LogDebug("🔍 Discovering table relationships for {Count} tables", tableNames.Count);

            // Use the ForeignKeyRelationshipService to get actual relationships
            var foreignKeyService = _serviceProvider.GetRequiredService<IForeignKeyRelationshipService>();
            var fkRelationships = await foreignKeyService.GetRelationshipsForTablesAsync(tableNames);

            // Convert ForeignKeyRelationship to TableRelationship
            var tableRelationships = fkRelationships.Select(fk => new BIReportingCopilot.Core.Models.BusinessContext.TableRelationship
            {
                FromTable = fk.ParentTable,
                ToTable = fk.ReferencedTable,
                FromColumn = fk.ParentColumn,
                ToColumn = fk.ReferencedColumn,
                Type = BIReportingCopilot.Core.Models.BusinessContext.RelationshipType.OneToMany, // Default to OneToMany for FK relationships
                BusinessMeaning = GenerateBusinessMeaning(fk)
            }).ToList();

            _logger.LogDebug("✅ Discovered {Count} table relationships", tableRelationships.Count);
            return tableRelationships;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error discovering table relationships");
            return new List<BIReportingCopilot.Core.Models.BusinessContext.TableRelationship>();
        }
    }

    private string GenerateBusinessMeaning(ForeignKeyRelationship fk)
    {
        var parentTableName = ExtractTableName(fk.ParentTable);
        var referencedTableName = ExtractTableName(fk.ReferencedTable);

        return $"{parentTableName} references {referencedTableName} through {fk.ParentColumn}";
    }

    private string ExtractTableName(string fullTableName)
    {
        var parts = fullTableName.Split('.');
        return parts.Length > 1 ? parts[1] : fullTableName;
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
        var originalQueryLower = profile.OriginalQuestion.ToLowerInvariant();
        var tableLower = table.TableName.ToLowerInvariant();
        var purposeLower = table.BusinessPurpose?.ToLowerInvariant() ?? "";

        _logger.LogInformation("🔍 [SCORING-DEBUG] Evaluating table: {Schema}.{Table} for query: {Query}",
            table.SchemaName, table.TableName, profile.OriginalQuestion);

        _logger.LogDebug("📋 [TABLE-DEBUG] Table details - Purpose: {Purpose}, Domain: {Domain}, Context: {Context}",
            table.BusinessPurpose, table.DomainClassification, table.BusinessContext);

        // ENHANCED QUERY CLASSIFICATION SYSTEM
        var queryClassification = ClassifyQuery(profile.OriginalQuestion, profile);

        _logger.LogInformation("🎯 [CLASSIFICATION] Query classified as: {Category} (Confidence: {Confidence:F2})",
            queryClassification.Category, queryClassification.Confidence);

        // ENHANCED DEPOSIT QUERY DETECTION (legacy support)
        bool isDepositQuery = IsDepositRelatedQuery(originalQueryLower, profile.Intent.Type);

        _logger.LogInformation("🏦 [DEPOSIT-DEBUG] Deposit query detected: {IsDeposit} for table: {Table} (Intent: {Intent})",
            isDepositQuery, table.TableName, profile.Intent.Type);

        if (originalQueryLower.Contains("deposit") || originalQueryLower.Contains("depositor"))
        {
            _logger.LogDebug("💰 [DEPOSIT-DEBUG] Query contains deposit terms: deposit={HasDeposit}, depositor={HasDepositor}",
                originalQueryLower.Contains("deposit"), originalQueryLower.Contains("depositor"));
        }

        // SMART TABLE SCORING SYSTEM based on query classification
        score += ApplySmartTableScoring(table, queryClassification, tableLower, purposeLower);

        if (isDepositQuery)
        {
            _logger.LogInformation("🏦 [LEGACY-SCORING] Applying legacy deposit scoring for table: {Table}", table.TableName);

            // Get confidence level for progressive scoring
            var confidenceLevel = GetDepositQueryConfidence(originalQueryLower);
            _logger.LogDebug("📊 [CONFIDENCE] Deposit query confidence: {Level}", confidenceLevel);

            // PRIORITIZE FINANCIAL/TRANSACTION TABLES with progressive scoring (legacy support)
            if (tableLower.Contains("deposit") || tableLower.Contains("transaction") ||
                tableLower.Contains("payment") || tableLower.Contains("financial") ||
                purposeLower.Contains("deposit") || purposeLower.Contains("financial"))
            {
                var financialBoost = confidenceLevel == "HIGH" ? 1.0 : 0.5; // Reduced since smart scoring handles this
                score += financialBoost;
                _logger.LogDebug("💰 Legacy financial table boost: {Table} (+{Boost})", table.TableName, financialBoost);
            }

            // PRIORITIZE DAILY ACTION TABLES (likely contain deposit data)
            if (tableLower.Contains("daily") && tableLower.Contains("action"))
            {
                // Give extra boost to main daily actions table (not gaming-specific ones)
                if (!tableLower.Contains("game"))
                {
                    score += 2.2; // Very high priority for main daily action table
                    _logger.LogDebug("📊 Main daily action table: {Table} (+2.2)", table.TableName);
                }
                else
                {
                    score += 1.0; // Lower priority for gaming-specific daily action tables
                    _logger.LogDebug("📊 Gaming daily action table: {Table} (+1.0)", table.TableName);
                }
            }

            // PRIORITIZE PLAYER/USER TABLES
            if (tableLower.Contains("player") || tableLower.Contains("user") ||
                tableLower.Contains("customer") || tableLower.Contains("account") ||
                purposeLower.Contains("player") || purposeLower.Contains("user"))
            {
                score += 1.5; // High priority for player tables
                _logger.LogDebug("👤 Player identification table: {Table} (+1.5)", table.TableName);
            }

            // GEOGRAPHIC TABLES for country filtering
            if (tableLower.Contains("country") || tableLower.Contains("location") ||
                tableLower.Contains("region") || purposeLower.Contains("geographic"))
            {
                score += 1.5; // High priority for geographic filtering
                _logger.LogDebug("🌍 Geographic table: {Table} (+1.5)", table.TableName);
            }

            // PENALIZE GAMING TABLES for financial queries
            bool isGamingTable = false;

            // Pure gaming tables (always penalize for deposit queries)
            if (tableLower.Contains("game") && !tableLower.Contains("deposit") &&
                !tableLower.Contains("transaction") && !tableLower.Contains("financial"))
            {
                _logger.LogDebug("🎮 [GAMING-DEBUG] Table contains 'game': {Table}, checking gaming patterns", table.TableName);

                // Check for gaming-specific patterns
                if (tableLower.Contains("games") ||  // tbl_Daily_actions_games, Games
                    tableLower == "games" ||         // Pure Games table
                    tableLower.Contains("_games") || // Any table ending with _games
                    (tableLower.Contains("game") && !tableLower.Contains("action"))) // Pure game tables
                {
                    isGamingTable = true;
                    _logger.LogInformation("🎮 [GAMING-DEBUG] Gaming table pattern matched: {Table} (Pattern: {Pattern})",
                        table.TableName,
                        tableLower.Contains("games") ? "contains_games" :
                        tableLower == "games" ? "equals_games" :
                        tableLower.Contains("_games") ? "ends_with_games" : "pure_game_table");
                }
            }

            _logger.LogInformation("🎮 [GAMING-DEBUG] Gaming table classification: {Table} = {IsGaming}",
                table.TableName, isGamingTable);

            if (isGamingTable)
            {
                score -= 3.0; // VERY STRONG penalty for gaming tables in deposit queries (increased from -1.5)
                _logger.LogWarning("🎮 [PENALTY-DEBUG] VERY STRONG gaming table penalty applied: {Table} (-3.0), Score now: {Score}",
                    table.TableName, score);
            }
        }

        // STANDARD SCORING (for all queries)

        // Domain match
        if (table.DomainClassification?.Contains(profile.Domain.Name, StringComparison.OrdinalIgnoreCase) == true)
        {
            score += 0.3;
            _logger.LogDebug("🏷️ Domain match: {Domain} (+0.3)", profile.Domain.Name);
        }

        // Business term matches
        foreach (var term in profile.BusinessTerms)
        {
            if (table.BusinessPurpose.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                table.BusinessContext.Contains(term, StringComparison.OrdinalIgnoreCase))
            {
                score += 0.2;
                _logger.LogDebug("📝 Business term match: {Term} (+0.2)", term);
            }
        }

        // Semantic similarity
        try
        {
            var semanticScore = await _semanticMatchingService.CalculateSemanticSimilarityAsync(
                profile.OriginalQuestion,
                $"{table.BusinessPurpose} {table.BusinessContext}");
            score += semanticScore * 0.5;
            _logger.LogDebug("🧠 Semantic similarity: {Score:F2} (+{Boost:F2})", semanticScore, semanticScore * 0.5);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "⚠️ Semantic similarity calculation failed for table {Table}", table.TableName);
        }

        // Importance and usage frequency boost
        if (table.ImportanceScore > 0.7m)
        {
            score += 0.3;
            _logger.LogDebug("⭐ High importance table: {Score} (+0.3)", table.ImportanceScore);
        }

        if (table.UsageFrequency > 0.7m)
        {
            score += 0.2;
            _logger.LogDebug("📈 High usage frequency: {Frequency} (+0.2)", table.UsageFrequency);
        }

        var finalScore = Math.Min(score, 3.0); // Allow higher scores for deposit queries
        _logger.LogDebug("✅ Final score for {Table}: {Score:F2}", table.TableName, finalScore);

        return finalScore;
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
        List<BIReportingCopilot.Core.Models.BusinessContext.TableRelationship> relationships)
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

    /// <summary>
    /// Enhanced query classification system for domain-specific table selection
    /// </summary>
    public enum QueryCategory
    {
        Financial,
        Gaming,
        Analytics,
        Operational,
        Reporting,
        Unknown
    }

    /// <summary>
    /// Query classification result with confidence and context
    /// </summary>
    public class QueryClassificationResult
    {
        public QueryCategory Category { get; set; }
        public double Confidence { get; set; }
        public List<string> DetectedPatterns { get; set; } = new();
        public Dictionary<string, double> CategoryScores { get; set; } = new();
        public bool IsHighConfidence => Confidence >= 0.8;
        public bool IsMediumConfidence => Confidence >= 0.6 && Confidence < 0.8;
    }

    /// <summary>
    /// Comprehensive query classification with domain-specific pattern matching
    /// </summary>
    private QueryClassificationResult ClassifyQuery(string query, BusinessContextProfile profile)
    {
        var queryLower = query.ToLowerInvariant();
        var result = new QueryClassificationResult();

        _logger.LogInformation("🔍 [QUERY-CLASSIFICATION] Analyzing query: {Query}", query);

        // Financial patterns
        var financialScore = CalculateFinancialScore(queryLower, profile, result.DetectedPatterns);
        result.CategoryScores["Financial"] = financialScore;

        // Gaming patterns
        var gamingScore = CalculateGamingScore(queryLower, profile, result.DetectedPatterns);
        result.CategoryScores["Gaming"] = gamingScore;

        // Analytics patterns
        var analyticsScore = CalculateAnalyticsScore(queryLower, profile, result.DetectedPatterns);
        result.CategoryScores["Analytics"] = analyticsScore;

        // Operational patterns
        var operationalScore = CalculateOperationalScore(queryLower, profile, result.DetectedPatterns);
        result.CategoryScores["Operational"] = operationalScore;

        // Reporting patterns
        var reportingScore = CalculateReportingScore(queryLower, profile, result.DetectedPatterns);
        result.CategoryScores["Reporting"] = reportingScore;

        // Determine primary category and confidence
        var maxScore = result.CategoryScores.Values.Max();
        var primaryCategory = result.CategoryScores.FirstOrDefault(x => x.Value == maxScore);

        result.Category = Enum.Parse<QueryCategory>(primaryCategory.Key);
        result.Confidence = Math.Min(maxScore / 10.0, 1.0); // Normalize to 0-1 range

        _logger.LogInformation("📊 [QUERY-CLASSIFICATION] Result: {Category} (Confidence: {Confidence:F2}) - Patterns: {Patterns}",
            result.Category, result.Confidence, string.Join(", ", result.DetectedPatterns));

        _logger.LogDebug("📈 [QUERY-CLASSIFICATION] All scores: {Scores}",
            string.Join(", ", result.CategoryScores.Select(kvp => $"{kvp.Key}:{kvp.Value:F1}")));

        return result;
    }

    /// <summary>
    /// Calculate financial query score based on comprehensive pattern matching
    /// </summary>
    private double CalculateFinancialScore(string queryLower, BusinessContextProfile profile, List<string> detectedPatterns)
    {
        double score = 0;

        // Core financial terms (high weight)
        var financialTerms = new[] { "deposit", "depositor", "depositing", "deposits", "payment", "transaction",
                                   "financial", "revenue", "income", "profit", "cost", "expense", "balance",
                                   "amount", "value", "money", "cash", "fund", "budget", "billing" };

        foreach (var term in financialTerms)
        {
            if (queryLower.Contains(term))
            {
                score += 3.0;
                detectedPatterns.Add($"financial-term:{term}");
            }
        }

        // Financial aggregation patterns (medium weight)
        var aggregationTerms = new[] { "total", "sum", "average", "count", "top", "highest", "lowest", "best", "worst" };
        foreach (var term in aggregationTerms)
        {
            if (queryLower.Contains(term))
            {
                score += 1.5;
                detectedPatterns.Add($"financial-aggregation:{term}");
            }
        }

        // Geographic context (indicates customer/financial analysis)
        var geoTerms = new[] { "country", "region", "from", "by country", "by region", "uk", "us", "europe" };
        foreach (var term in geoTerms)
        {
            if (queryLower.Contains(term))
            {
                score += 1.0;
                detectedPatterns.Add($"financial-geo:{term}");
            }
        }

        // Time context (common in financial reporting)
        var timeTerms = new[] { "yesterday", "today", "daily", "weekly", "monthly", "last", "this", "period" };
        foreach (var term in timeTerms)
        {
            if (queryLower.Contains(term))
            {
                score += 0.5;
                detectedPatterns.Add($"financial-time:{term}");
            }
        }

        // Intent type boost
        if (profile.Intent.Type == IntentType.Aggregation || profile.Intent.Type == IntentType.Operational)
        {
            score += 1.0;
            detectedPatterns.Add($"financial-intent:{profile.Intent.Type}");
        }

        return score;
    }

    /// <summary>
    /// Calculate gaming query score based on gaming-specific patterns
    /// </summary>
    private double CalculateGamingScore(string queryLower, BusinessContextProfile profile, List<string> detectedPatterns)
    {
        double score = 0;

        // Core gaming terms
        var gamingTerms = new[] { "game", "games", "gaming", "play", "player", "bet", "betting", "win", "loss",
                                "jackpot", "bonus", "spin", "round", "session", "rtp", "volatility" };

        foreach (var term in gamingTerms)
        {
            if (queryLower.Contains(term))
            {
                score += 3.0;
                detectedPatterns.Add($"gaming-term:{term}");
            }
        }

        // Gaming metrics
        var gamingMetrics = new[] { "revenue", "ggr", "ngr", "active users", "retention", "engagement" };
        foreach (var metric in gamingMetrics)
        {
            if (queryLower.Contains(metric))
            {
                score += 2.0;
                detectedPatterns.Add($"gaming-metric:{metric}");
            }
        }

        return score;
    }

    /// <summary>
    /// Calculate analytics query score
    /// </summary>
    private double CalculateAnalyticsScore(string queryLower, BusinessContextProfile profile, List<string> detectedPatterns)
    {
        double score = 0;

        // Analytics terms
        var analyticsTerms = new[] { "analyze", "analysis", "trend", "pattern", "correlation", "insight",
                                   "dashboard", "report", "metric", "kpi", "performance" };

        foreach (var term in analyticsTerms)
        {
            if (queryLower.Contains(term))
            {
                score += 2.0;
                detectedPatterns.Add($"analytics-term:{term}");
            }
        }

        // Intent type boost
        if (profile.Intent.Type == IntentType.Analytical)
        {
            score += 3.0;
            detectedPatterns.Add($"analytics-intent:{profile.Intent.Type}");
        }

        return score;
    }

    /// <summary>
    /// Calculate operational query score
    /// </summary>
    private double CalculateOperationalScore(string queryLower, BusinessContextProfile profile, List<string> detectedPatterns)
    {
        double score = 0;

        // Operational terms
        var operationalTerms = new[] { "list", "show", "get", "find", "search", "lookup", "view", "display" };

        foreach (var term in operationalTerms)
        {
            if (queryLower.Contains(term))
            {
                score += 1.5;
                detectedPatterns.Add($"operational-term:{term}");
            }
        }

        // Intent type boost
        if (profile.Intent.Type == IntentType.Operational)
        {
            score += 2.0;
            detectedPatterns.Add($"operational-intent:{profile.Intent.Type}");
        }

        return score;
    }

    /// <summary>
    /// Calculate reporting query score
    /// </summary>
    private double CalculateReportingScore(string queryLower, BusinessContextProfile profile, List<string> detectedPatterns)
    {
        double score = 0;

        // Reporting terms
        var reportingTerms = new[] { "report", "summary", "overview", "breakdown", "detail", "export" };

        foreach (var term in reportingTerms)
        {
            if (queryLower.Contains(term))
            {
                score += 2.0;
                detectedPatterns.Add($"reporting-term:{term}");
            }
        }

        // Intent type boost
        if (profile.Intent.Type == IntentType.Analytical)
        {
            score += 3.0;
            detectedPatterns.Add($"analytical-intent:{profile.Intent.Type}");
        }

        return score;
    }

    /// <summary>
    /// Apply smart table scoring based on query classification and table characteristics
    /// </summary>
    private double ApplySmartTableScoring(BusinessTableInfoDto table, QueryClassificationResult classification,
        string tableLower, string purposeLower)
    {
        double score = 0;

        _logger.LogDebug("🎯 [SMART-SCORING] Applying smart scoring for table: {Table}, Category: {Category}, Confidence: {Confidence:F2}",
            table.TableName, classification.Category, classification.Confidence);

        switch (classification.Category)
        {
            case QueryCategory.Financial:
                score += ApplyFinancialTableScoring(table, classification, tableLower, purposeLower);
                break;

            case QueryCategory.Gaming:
                score += ApplyGamingTableScoring(table, classification, tableLower, purposeLower);
                break;

            case QueryCategory.Analytics:
                score += ApplyAnalyticsTableScoring(table, classification, tableLower, purposeLower);
                break;

            case QueryCategory.Operational:
                score += ApplyOperationalTableScoring(table, classification, tableLower, purposeLower);
                break;

            case QueryCategory.Reporting:
                score += ApplyReportingTableScoring(table, classification, tableLower, purposeLower);
                break;

            default:
                score += ApplyGenericTableScoring(table, classification, tableLower, purposeLower);
                break;
        }

        _logger.LogDebug("📊 [SMART-SCORING] Smart scoring result for {Table}: +{Score:F2}", table.TableName, score);
        return score;
    }

    /// <summary>
    /// Apply financial-specific table scoring with enhanced boosts and penalties
    /// </summary>
    private double ApplyFinancialTableScoring(BusinessTableInfoDto table, QueryClassificationResult classification,
        string tableLower, string purposeLower)
    {
        double score = 0;

        // FINANCIAL BOOST (+4.0 for high confidence, +3.0 for medium, +2.0 for low)
        var financialTerms = new[] { "deposit", "transaction", "payment", "financial", "revenue", "income",
                                   "balance", "amount", "money", "cash", "billing" };

        bool isFinancialTable = financialTerms.Any(term => tableLower.Contains(term) || purposeLower.Contains(term));

        if (isFinancialTable)
        {
            var boost = classification.IsHighConfidence ? 4.0 :
                       classification.IsMediumConfidence ? 3.0 : 2.0;
            score += boost;
            _logger.LogInformation("💰 [FINANCIAL-BOOST] Financial table boost: {Table} (+{Boost})", table.TableName, boost);
        }

        // Player/Customer tables (important for financial analysis)
        var customerTerms = new[] { "player", "user", "customer", "account", "profile" };
        bool isCustomerTable = customerTerms.Any(term => tableLower.Contains(term) || purposeLower.Contains(term));

        if (isCustomerTable)
        {
            score += 2.0;
            _logger.LogDebug("👤 [CUSTOMER-BOOST] Customer table boost: {Table} (+2.0)", table.TableName);
        }

        // Geographic tables (common in financial analysis)
        var geoTerms = new[] { "country", "region", "location", "geography" };
        bool isGeoTable = geoTerms.Any(term => tableLower.Contains(term) || purposeLower.Contains(term));

        if (isGeoTable)
        {
            score += 1.5;
            _logger.LogDebug("🌍 [GEO-BOOST] Geographic table boost: {Table} (+1.5)", table.TableName);
        }

        // GAMING PENALTY (-3.0) - Strong penalty for gaming tables in financial queries
        if (IsGamingTableForFinancialQuery(table))
        {
            score -= 3.0;
            _logger.LogWarning("🎮 [GAMING-PENALTY] Gaming table penalty in financial query: {Table} (-3.0)", table.TableName);
        }

        return score;
    }

    /// <summary>
    /// Apply gaming-specific table scoring
    /// </summary>
    private double ApplyGamingTableScoring(BusinessTableInfoDto table, QueryClassificationResult classification,
        string tableLower, string purposeLower)
    {
        double score = 0;

        // Gaming table boost
        var gamingTerms = new[] { "game", "games", "gaming", "bet", "spin", "round", "session" };
        bool isGamingTable = gamingTerms.Any(term => tableLower.Contains(term) || purposeLower.Contains(term));

        if (isGamingTable)
        {
            var boost = classification.IsHighConfidence ? 4.0 :
                       classification.IsMediumConfidence ? 3.0 : 2.0;
            score += boost;
            _logger.LogInformation("🎮 [GAMING-BOOST] Gaming table boost: {Table} (+{Boost})", table.TableName, boost);
        }

        // Player tables are relevant for gaming
        var playerTerms = new[] { "player", "user" };
        bool isPlayerTable = playerTerms.Any(term => tableLower.Contains(term) || purposeLower.Contains(term));

        if (isPlayerTable)
        {
            score += 2.0;
            _logger.LogDebug("👤 [PLAYER-BOOST] Player table boost for gaming: {Table} (+2.0)", table.TableName);
        }

        return score;
    }

    /// <summary>
    /// Apply analytics-specific table scoring
    /// </summary>
    private double ApplyAnalyticsTableScoring(BusinessTableInfoDto table, QueryClassificationResult classification,
        string tableLower, string purposeLower)
    {
        double score = 0;

        // Analytics/metrics tables
        var analyticsTerms = new[] { "metric", "kpi", "performance", "analytics", "dashboard", "report" };
        bool isAnalyticsTable = analyticsTerms.Any(term => tableLower.Contains(term) || purposeLower.Contains(term));

        if (isAnalyticsTable)
        {
            score += 3.0;
            _logger.LogDebug("📊 [ANALYTICS-BOOST] Analytics table boost: {Table} (+3.0)", table.TableName);
        }

        return score;
    }

    /// <summary>
    /// Apply operational-specific table scoring
    /// </summary>
    private double ApplyOperationalTableScoring(BusinessTableInfoDto table, QueryClassificationResult classification,
        string tableLower, string purposeLower)
    {
        double score = 0;

        // Operational tables (general boost)
        score += 1.0;
        _logger.LogDebug("⚙️ [OPERATIONAL-BOOST] Operational table boost: {Table} (+1.0)", table.TableName);

        return score;
    }

    /// <summary>
    /// Apply reporting-specific table scoring
    /// </summary>
    private double ApplyReportingTableScoring(BusinessTableInfoDto table, QueryClassificationResult classification,
        string tableLower, string purposeLower)
    {
        double score = 0;

        // Reporting tables
        var reportingTerms = new[] { "report", "summary", "dashboard", "export" };
        bool isReportingTable = reportingTerms.Any(term => tableLower.Contains(term) || purposeLower.Contains(term));

        if (isReportingTable)
        {
            score += 2.5;
            _logger.LogDebug("📋 [REPORTING-BOOST] Reporting table boost: {Table} (+2.5)", table.TableName);
        }

        return score;
    }

    /// <summary>
    /// Apply generic table scoring for unknown categories
    /// </summary>
    private double ApplyGenericTableScoring(BusinessTableInfoDto table, QueryClassificationResult classification,
        string tableLower, string purposeLower)
    {
        double score = 0;

        // Generic boost based on confidence
        score += classification.Confidence * 1.0;
        _logger.LogDebug("🔍 [GENERIC-BOOST] Generic table boost: {Table} (+{Boost:F2})", table.TableName, score);

        return score;
    }

    /// <summary>
    /// Get confidence level for deposit query detection
    /// </summary>
    private string GetDepositQueryConfidence(string queryLower)
    {
        int confidenceScore = 0;

        // Core deposit terms
        if (queryLower.Contains("deposit") || queryLower.Contains("depositor")) confidenceScore += 3;

        // Analysis patterns
        if (queryLower.Contains("top") || queryLower.Contains("highest") || queryLower.Contains("best")) confidenceScore += 2;

        // Geographic context
        if (queryLower.Contains("country") || queryLower.Contains("from") || queryLower.Contains("uk") || queryLower.Contains("region")) confidenceScore += 2;

        // Time context
        if (queryLower.Contains("yesterday") || queryLower.Contains("daily") || queryLower.Contains("last")) confidenceScore += 2;

        // Financial context
        if (queryLower.Contains("amount") || queryLower.Contains("value") || queryLower.Contains("financial")) confidenceScore += 1;

        return confidenceScore >= 7 ? "HIGH" : confidenceScore >= 4 ? "MEDIUM" : "LOW";
    }

    /// <summary>
    /// Enhanced deposit query detection with comprehensive pattern matching
    /// </summary>
    private bool IsDepositRelatedQuery(string queryLower, IntentType intentType)
    {
        _logger.LogDebug("🔍 [DEPOSIT-DETECTION] Analyzing query: {Query} with intent: {Intent}", queryLower, intentType);

        // Support multiple intent types for deposit queries
        bool hasValidIntent = intentType == IntentType.Aggregation ||
                             intentType == IntentType.Operational ||
                             intentType == IntentType.Analytical ||
                             intentType == IntentType.Exploratory;

        if (!hasValidIntent)
        {
            _logger.LogDebug("❌ [DEPOSIT-DETECTION] Intent type {Intent} not valid for deposit queries", intentType);
            return false;
        }

        // Enhanced deposit term detection
        bool hasDepositTerms = queryLower.Contains("deposit") || queryLower.Contains("depositor") ||
                              queryLower.Contains("depositing") || queryLower.Contains("deposits");

        // Enhanced aggregation/analysis patterns
        bool hasAnalysisPatterns = queryLower.Contains("top") || queryLower.Contains("sum") ||
                                  queryLower.Contains("total") || queryLower.Contains("highest") ||
                                  queryLower.Contains("best") || queryLower.Contains("most") ||
                                  queryLower.Contains("average") || queryLower.Contains("count") ||
                                  queryLower.Contains("list") || queryLower.Contains("show") ||
                                  queryLower.Contains("find") || queryLower.Contains("get");

        // Financial context patterns
        bool hasFinancialContext = queryLower.Contains("amount") || queryLower.Contains("value") ||
                                  queryLower.Contains("money") || queryLower.Contains("payment") ||
                                  queryLower.Contains("transaction") || queryLower.Contains("financial");

        // Geographic/demographic patterns (common in deposit analysis)
        bool hasGeographicContext = queryLower.Contains("country") || queryLower.Contains("region") ||
                                   queryLower.Contains("from") || queryLower.Contains("in") ||
                                   queryLower.Contains("by country") || queryLower.Contains("by region");

        // Time-based patterns (common in deposit analysis)
        bool hasTimeContext = queryLower.Contains("yesterday") || queryLower.Contains("today") ||
                             queryLower.Contains("last") || queryLower.Contains("this") ||
                             queryLower.Contains("daily") || queryLower.Contains("weekly") ||
                             queryLower.Contains("monthly") || queryLower.Contains("period");

        bool isDepositQuery = hasDepositTerms && (hasAnalysisPatterns || hasFinancialContext);

        // Additional confidence boost for comprehensive patterns
        bool hasHighConfidence = isDepositQuery && (hasGeographicContext || hasTimeContext);

        _logger.LogInformation("💰 [DEPOSIT-DETECTION] Result: {IsDeposit} (Confidence: {Confidence}) - Terms: {HasTerms}, Analysis: {HasAnalysis}, Financial: {HasFinancial}, Geographic: {HasGeo}, Time: {HasTime}",
            isDepositQuery, hasHighConfidence ? "HIGH" : "NORMAL", hasDepositTerms, hasAnalysisPatterns, hasFinancialContext, hasGeographicContext, hasTimeContext);

        return isDepositQuery;
    }

    /// <summary>
    /// Apply query-specific filtering to fallback tables to prevent irrelevant tables
    /// </summary>
    private List<BusinessTableInfoDto> ApplyQuerySpecificFallbackFiltering(
        List<BusinessTableInfoDto> allTables,
        BusinessContextProfile profile)
    {
        var originalQueryLower = profile.OriginalQuestion.ToLowerInvariant();
        var filteredTables = new List<BusinessTableInfoDto>();

        _logger.LogInformation("🔍 [FALLBACK-FILTER] Applying query-specific filtering for: {Query}", profile.OriginalQuestion);

        // Detect if this is a deposit/financial query
        bool isDepositQuery = (profile.Intent.Type == IntentType.Aggregation || profile.Intent.Type == IntentType.Operational) &&
                            (originalQueryLower.Contains("deposit") || originalQueryLower.Contains("depositor") ||
                             originalQueryLower.Contains("financial") || originalQueryLower.Contains("transaction"));

        foreach (var table in allTables)
        {
            var tableLower = table.TableName.ToLowerInvariant();
            var purposeLower = table.BusinessPurpose?.ToLowerInvariant() ?? "";
            bool shouldInclude = true;
            string exclusionReason = "";

            if (isDepositQuery)
            {
                // For deposit queries, exclude pure gaming tables
                if (IsGamingTableForFinancialQuery(table))
                {
                    shouldInclude = false;
                    exclusionReason = "Gaming table excluded from financial query";
                }
                // Prioritize financial, player, and geographic tables
                else if (tableLower.Contains("deposit") || tableLower.Contains("transaction") ||
                        tableLower.Contains("player") || tableLower.Contains("user") ||
                        tableLower.Contains("country") || tableLower.Contains("daily") ||
                        purposeLower.Contains("financial") || purposeLower.Contains("player"))
                {
                    shouldInclude = true; // High priority tables
                }
            }

            if (shouldInclude)
            {
                filteredTables.Add(table);
                _logger.LogDebug("✅ [FALLBACK-FILTER] Included: {Table}", table.TableName);
            }
            else
            {
                _logger.LogDebug("❌ [FALLBACK-FILTER] Excluded: {Table} - {Reason}", table.TableName, exclusionReason);
            }
        }

        _logger.LogInformation("📊 [FALLBACK-FILTER] Filtering complete: {Included}/{Total} tables included",
            filteredTables.Count, allTables.Count);

        return filteredTables;
    }

    /// <summary>
    /// Check if a table is a pure gaming table that should be excluded from financial queries
    /// IMPORTANT: tbl_Daily_actions contains deposit data and should NOT be excluded!
    /// </summary>
    private bool IsGamingTableForFinancialQuery(BusinessTableInfoDto table)
    {
        var tableLower = table.TableName.ToLowerInvariant();

        _logger.LogWarning("🔍 [GAMING-DETECTION-DEBUG] Analyzing table: {Table} (lowercase: {TableLower})",
            table.TableName, tableLower);

        // CRITICAL FIX: Only exclude PURE gaming tables, NOT financial/deposit tables
        bool isGamingTable = false;

        // 1. Pure gaming tables (exclude these)
        if (tableLower == "games" ||                       // Pure games table
            tableLower.Contains("_games") ||               // Any table ending with _games
            tableLower.Contains("tbl_daily_actions_games") || // Gaming-specific daily actions
            tableLower == "game_sessions" ||               // Game session data
            tableLower.Contains("game_rounds") ||          // Game round data
            tableLower.Contains("casino_games"))           // Casino game data
        {
            isGamingTable = true;
            _logger.LogWarning("🎮 [GAMING-DETECTION] Pure gaming table detected: {Table}", table.TableName);
        }

        // 2. Gaming-related keywords (exclude if contains gaming terms but not financial terms)
        if (!isGamingTable && tableLower.Contains("game") &&
            !tableLower.Contains("deposit") &&
            !tableLower.Contains("transaction") &&
            !tableLower.Contains("financial") &&
            !tableLower.Contains("payment") &&
            !tableLower.Contains("daily_actions"))  // IMPORTANT: Don't exclude daily_actions
        {
            isGamingTable = true;
            _logger.LogDebug("🎮 [GAMING-DETECTION] Gaming keyword table detected: {Table}", table.TableName);
        }

        // 3. Additional pure gaming indicators (but not mixed tables)
        if (!isGamingTable &&
            (tableLower.Contains("bet") || tableLower.Contains("casino") || tableLower.Contains("sport")) &&
            !tableLower.Contains("deposit") &&
            !tableLower.Contains("transaction") &&
            !tableLower.Contains("daily_actions"))
        {
            isGamingTable = true;
            _logger.LogDebug("🎮 [GAMING-DETECTION] Gaming indicator table detected: {Table}", table.TableName);
        }

        // IMPORTANT: Never exclude financial/deposit tables
        if (tableLower.Contains("deposit") ||
            tableLower.Contains("transaction") ||
            tableLower.Contains("financial") ||
            tableLower == "tbl_daily_actions" ||
            tableLower.Contains("player") ||
            tableLower.Contains("country"))
        {
            isGamingTable = false;
            _logger.LogWarning("💰 [FINANCIAL-TABLE] Financial/deposit table preserved: {Table}", table.TableName);
        }

        _logger.LogWarning("🎯 [GAMING-DETECTION-RESULT] Table {Table} is gaming table: {IsGaming}",
            table.TableName, isGamingTable);

        if (isGamingTable)
        {
            _logger.LogWarning("🚫 [DOMAIN-FILTER] Pure gaming table identified for exclusion from financial query: {Table}", table.TableName);
        }
        else
        {
            _logger.LogWarning("✅ [DOMAIN-FILTER] Table {Table} is NOT a gaming table - will be included", table.TableName);
        }

        return isGamingTable;
    }
}
