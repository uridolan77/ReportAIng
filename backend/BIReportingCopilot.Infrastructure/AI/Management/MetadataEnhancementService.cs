using BIReportingCopilot.Infrastructure.Data;
using BIReportingCopilot.Infrastructure.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BIReportingCopilot.Infrastructure.AI.Management;

/// <summary>
/// Simple service for enhancing existing business metadata by populating empty semantic fields
/// </summary>
public class MetadataEnhancementService
{
    private readonly BICopilotContext _context;
    private readonly ILogger<MetadataEnhancementService> _logger;

    public MetadataEnhancementService(
        BICopilotContext context,
        ILogger<MetadataEnhancementService> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Enhancement modes
    /// </summary>
    public enum EnhancementMode
    {
        EmptyFieldsOnly,    // Only populate empty fields (default)
        AllFields          // Improve all fields
    }

    /// <summary>
    /// Entity types for enhancement
    /// </summary>
    public enum EntityType
    {
        BusinessColumnInfo,
        BusinessTableInfo,
        BusinessGlossary
    }

    /// <summary>
    /// Enhancement request configuration
    /// </summary>
    public class MetadataEnhancementRequest
    {
        public EntityType EntityType { get; set; } = EntityType.BusinessColumnInfo;
        public EnhancementMode Mode { get; set; } = EnhancementMode.EmptyFieldsOnly;
        public int BatchSize { get; set; } = 50;
        public bool PreviewOnly { get; set; } = false;

        // Legacy properties for backward compatibility (will be removed)
        public List<string>? TargetTables { get; set; }
        public List<string>? TargetFields { get; set; }
        public double QualityThreshold { get; set; } = 0.8;
        public decimal? CostBudget { get; set; }
    }

    /// <summary>
    /// Enhancement result
    /// </summary>
    public class MetadataEnhancementResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int ColumnsProcessed { get; set; }
        public int TablesProcessed { get; set; }
        public int GlossaryTermsProcessed { get; set; }
        public int FieldsEnhanced { get; set; }
        public decimal TotalCost { get; set; }
        public TimeSpan ProcessingTime { get; set; }
        public List<string> Warnings { get; set; } = new();
        public List<string> Errors { get; set; } = new();
        public Dictionary<string, int> FieldEnhancementCounts { get; set; } = new();
    }

    /// <summary>
    /// Enhancement status for a specific entity type
    /// </summary>
    public class EntityEnhancementStatus
    {
        public int TotalRecords { get; set; }
        public int EnhancedRecords { get; set; }
        public double CoveragePercentage { get; set; }
    }

    /// <summary>
    /// Overall enhancement status
    /// </summary>
    public class EnhancementStatus
    {
        public EntityEnhancementStatus BusinessColumnInfo { get; set; } = new();
        public EntityEnhancementStatus BusinessTableInfo { get; set; } = new();
        public EntityEnhancementStatus BusinessGlossary { get; set; } = new();
    }

    /// <summary>
    /// Start metadata enhancement process (simplified demo version)
    /// </summary>
    public async Task<MetadataEnhancementResult> EnhanceMetadataAsync(
        MetadataEnhancementRequest request, 
        string userId,
        CancellationToken cancellationToken = default)
    {
        var result = new MetadataEnhancementResult();
        var startTime = DateTime.UtcNow;

        try
        {
            _logger.LogInformation("🚀 Starting metadata enhancement - EntityType: {EntityType}, Mode: {Mode}, User: {UserId}",
                request.EntityType, request.Mode, userId);

            // Process based on entity type
            switch (request.EntityType)
            {
                case EntityType.BusinessColumnInfo:
                    var columnResult = await EnhanceBusinessColumnInfoAsync(request, userId, cancellationToken);
                    result.ColumnsProcessed = columnResult.RecordsProcessed;
                    result.FieldsEnhanced = columnResult.FieldsEnhanced;
                    break;

                case EntityType.BusinessTableInfo:
                    var tableResult = await EnhanceBusinessTableInfoAsync(request, userId, cancellationToken);
                    result.TablesProcessed = tableResult.RecordsProcessed;
                    result.FieldsEnhanced = tableResult.FieldsEnhanced;
                    break;

                case EntityType.BusinessGlossary:
                    var glossaryResult = await EnhanceBusinessGlossaryAsync(request, userId, cancellationToken);
                    result.GlossaryTermsProcessed = glossaryResult.RecordsProcessed;
                    result.FieldsEnhanced = glossaryResult.FieldsEnhanced;
                    break;

                default:
                    throw new ArgumentException($"Unsupported entity type: {request.EntityType}");
            }

            result.ProcessingTime = DateTime.UtcNow - startTime;
            result.Success = true;
            result.Message = $"Enhanced {result.FieldsEnhanced} fields across {result.ColumnsProcessed + result.TablesProcessed + result.GlossaryTermsProcessed} records";

            _logger.LogInformation("✅ Metadata enhancement completed - EntityType: {EntityType}, Fields: {Fields}",
                request.EntityType, result.FieldsEnhanced);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error during metadata enhancement");
            result.Success = false;
            result.Message = $"Enhancement failed: {ex.Message}";
            result.Errors.Add(ex.Message);
            return result;
        }
    }

    /// <summary>
    /// Enhancement result for a specific entity type
    /// </summary>
    private class EntityEnhancementResult
    {
        public int RecordsProcessed { get; set; }
        public int FieldsEnhanced { get; set; }
    }

    /// <summary>
    /// Enhance BusinessColumnInfo records (simplified demo version)
    /// </summary>
    private async Task<EntityEnhancementResult> EnhanceBusinessColumnInfoAsync(
        MetadataEnhancementRequest request, 
        string userId,
        CancellationToken cancellationToken)
    {
        var result = new EntityEnhancementResult();

        try
        {
            var query = _context.BusinessColumnInfo.AsQueryable();

            // Filter for records with empty semantic fields (using actual available fields)
            if (request.Mode == EnhancementMode.EmptyFieldsOnly)
            {
                query = query.Where(c =>
                    string.IsNullOrEmpty(c.SemanticContext) ||
                    string.IsNullOrEmpty(c.AnalyticalContext) ||
                    string.IsNullOrEmpty(c.ConceptualRelationships) ||
                    string.IsNullOrEmpty(c.DomainSpecificTerms) ||
                    string.IsNullOrEmpty(c.QueryIntentMapping) ||
                    string.IsNullOrEmpty(c.BusinessQuestionTypes) ||
                    string.IsNullOrEmpty(c.SemanticSynonyms) ||
                    string.IsNullOrEmpty(c.BusinessMetrics) ||
                    string.IsNullOrEmpty(c.LLMPromptHints) ||
                    string.IsNullOrEmpty(c.VectorSearchTags) ||
                    c.SemanticRelevanceScore == null || c.SemanticRelevanceScore == 0);
            }

            var columns = await query.Take(request.BatchSize).ToListAsync(cancellationToken);

            _logger.LogInformation("📊 Found {Count} BusinessColumnInfo records to enhance", columns.Count);

            foreach (var column in columns)
            {
                var enhanced = EnhanceColumnMetadata(column);
                if (enhanced > 0)
                {
                    result.FieldsEnhanced += enhanced;
                    if (!request.PreviewOnly)
                    {
                        column.UpdatedBy = userId;
                        column.UpdatedDate = DateTime.UtcNow;
                    }
                }
                result.RecordsProcessed++;
            }

            if (!request.PreviewOnly && result.FieldsEnhanced > 0)
            {
                await _context.SaveChangesAsync(cancellationToken);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in EnhanceBusinessColumnInfoAsync");
            return result;
        }
    }

    /// <summary>
    /// Comprehensive enhancement for all semantic fields
    /// </summary>
    private int EnhanceColumnMetadata(BusinessColumnInfoEntity column)
    {
        var fieldsEnhanced = 0;

        // Only enhance empty fields to avoid overwriting existing data

        if (string.IsNullOrEmpty(column.SemanticContext))
        {
            column.SemanticContext = $"Gaming industry data field: {column.BusinessMeaning ?? column.ColumnName}";
            fieldsEnhanced++;
        }

        if (string.IsNullOrEmpty(column.AnalyticalContext))
        {
            column.AnalyticalContext = "Used for business intelligence reporting and analysis";
            fieldsEnhanced++;
        }

        if (column.SemanticRelevanceScore == null || column.SemanticRelevanceScore == 0)
        {
            column.SemanticRelevanceScore = 0.7M; // Default relevance
            fieldsEnhanced++;
        }

        if (string.IsNullOrEmpty(column.ConceptualRelationships))
        {
            column.ConceptualRelationships = "[\"player_data\", \"gaming_metrics\", \"business_intelligence\"]";
            fieldsEnhanced++;
        }

        if (string.IsNullOrEmpty(column.DomainSpecificTerms))
        {
            column.DomainSpecificTerms = "[\"gaming\", \"casino\", \"player\", \"analytics\"]";
            fieldsEnhanced++;
        }

        if (string.IsNullOrEmpty(column.QueryIntentMapping))
        {
            column.QueryIntentMapping = "{\"reporting\": 0.8, \"analytics\": 0.7, \"monitoring\": 0.6}";
            fieldsEnhanced++;
        }

        if (string.IsNullOrEmpty(column.BusinessQuestionTypes))
        {
            column.BusinessQuestionTypes = "[\"performance\", \"trends\", \"comparisons\", \"summaries\"]";
            fieldsEnhanced++;
        }

        if (string.IsNullOrEmpty(column.SemanticSynonyms))
        {
            column.SemanticSynonyms = $"[\"{column.ColumnName}\", \"{column.BusinessMeaning ?? column.ColumnName}\"]";
            fieldsEnhanced++;
        }

        if (string.IsNullOrEmpty(column.BusinessMetrics))
        {
            column.BusinessMetrics = "[\"count\", \"sum\", \"average\", \"trend\"]";
            fieldsEnhanced++;
        }

        if (string.IsNullOrEmpty(column.LLMPromptHints))
        {
            column.LLMPromptHints = $"[\"Use for {column.BusinessMeaning ?? column.ColumnName}\", \"Gaming industry context\", \"Business reporting\"]";
            fieldsEnhanced++;
        }

        if (string.IsNullOrEmpty(column.VectorSearchTags))
        {
            column.VectorSearchTags = "[\"gaming\", \"data\", \"analytics\", \"business\"]";
            fieldsEnhanced++;
        }

        return fieldsEnhanced;
    }

    /// <summary>
    /// Enhance BusinessTableInfo records (simplified demo version)
    /// </summary>
    private async Task<EntityEnhancementResult> EnhanceBusinessTableInfoAsync(
        MetadataEnhancementRequest request, string userId, CancellationToken cancellationToken)
    {
        var result = new EntityEnhancementResult();

        try
        {
            var query = _context.BusinessTableInfo.AsQueryable();

            if (request.Mode == EnhancementMode.EmptyFieldsOnly)
            {
                query = query.Where(t =>
                    string.IsNullOrEmpty(t.SemanticDescription) ||
                    string.IsNullOrEmpty(t.LLMContextHints) ||
                    string.IsNullOrEmpty(t.VectorSearchKeywords) ||
                    string.IsNullOrEmpty(t.SemanticRelationships) ||
                    string.IsNullOrEmpty(t.BusinessProcesses) ||
                    string.IsNullOrEmpty(t.AnalyticalUseCases));
            }

            var tables = await query.Take(request.BatchSize).ToListAsync(cancellationToken);

            foreach (var table in tables)
            {
                var enhanced = EnhanceTableMetadata(table);
                if (enhanced > 0)
                {
                    result.FieldsEnhanced += enhanced;
                    if (!request.PreviewOnly)
                    {
                        table.UpdatedBy = userId;
                        table.UpdatedDate = DateTime.UtcNow;
                    }
                }
                result.RecordsProcessed++;
            }

            if (!request.PreviewOnly && result.FieldsEnhanced > 0)
            {
                await _context.SaveChangesAsync(cancellationToken);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in EnhanceBusinessTableInfoAsync");
            return result;
        }
    }

    /// <summary>
    /// Comprehensive table enhancement for all semantic fields
    /// </summary>
    private int EnhanceTableMetadata(BusinessTableInfoEntity table)
    {
        var fieldsEnhanced = 0;

        // Only enhance empty fields to avoid overwriting existing data

        if (string.IsNullOrEmpty(table.SemanticDescription))
        {
            table.SemanticDescription = $"Gaming industry table: {table.BusinessPurpose ?? table.TableName}";
            fieldsEnhanced++;
        }

        if (string.IsNullOrEmpty(table.LLMContextHints))
        {
            table.LLMContextHints = "[\"gaming\", \"casino\", \"player_data\", \"analytics\"]";
            fieldsEnhanced++;
        }

        if (string.IsNullOrEmpty(table.VectorSearchKeywords))
        {
            table.VectorSearchKeywords = "[\"gaming\", \"casino\", \"player\", \"data\", \"analytics\", \"business\"]";
            fieldsEnhanced++;
        }

        if (string.IsNullOrEmpty(table.SemanticRelationships))
        {
            table.SemanticRelationships = "[\"player_data\", \"gaming_metrics\", \"business_reporting\"]";
            fieldsEnhanced++;
        }

        if (string.IsNullOrEmpty(table.BusinessProcesses))
        {
            table.BusinessProcesses = "[\"reporting\", \"analytics\", \"monitoring\", \"business_intelligence\"]";
            fieldsEnhanced++;
        }

        if (string.IsNullOrEmpty(table.AnalyticalUseCases))
        {
            table.AnalyticalUseCases = "[\"performance_analysis\", \"trend_analysis\", \"comparative_analysis\", \"summary_reporting\"]";
            fieldsEnhanced++;
        }

        return fieldsEnhanced;
    }

    /// <summary>
    /// Enhance BusinessGlossary records (simplified demo version)
    /// </summary>
    private async Task<EntityEnhancementResult> EnhanceBusinessGlossaryAsync(
        MetadataEnhancementRequest request, string userId, CancellationToken cancellationToken)
    {
        var result = new EntityEnhancementResult();

        try
        {
            var query = _context.BusinessGlossary.AsQueryable();

            if (request.Mode == EnhancementMode.EmptyFieldsOnly)
            {
                query = query.Where(g => 
                    string.IsNullOrEmpty(g.ContextualVariations) ||
                    string.IsNullOrEmpty(g.QueryPatterns));
            }

            var glossaryTerms = await query.Take(request.BatchSize).ToListAsync(cancellationToken);

            foreach (var term in glossaryTerms)
            {
                var enhanced = EnhanceGlossaryMetadata(term);
                if (enhanced > 0)
                {
                    result.FieldsEnhanced += enhanced;
                    if (!request.PreviewOnly)
                    {
                        term.UpdatedBy = userId;
                        term.UpdatedDate = DateTime.UtcNow;
                    }
                }
                result.RecordsProcessed++;
            }

            if (!request.PreviewOnly && result.FieldsEnhanced > 0)
            {
                await _context.SaveChangesAsync(cancellationToken);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in EnhanceBusinessGlossaryAsync");
            return result;
        }
    }

    /// <summary>
    /// Simple glossary enhancement for demonstration
    /// </summary>
    private int EnhanceGlossaryMetadata(BusinessGlossaryEntity term)
    {
        var fieldsEnhanced = 0;

        if (string.IsNullOrEmpty(term.ContextualVariations))
        {
            term.ContextualVariations = $"Gaming context variations for: {term.Term}";
            fieldsEnhanced++;
        }

        if (string.IsNullOrEmpty(term.QueryPatterns))
        {
            term.QueryPatterns = "[\"What is\", \"Show me\", \"Calculate\"]";
            fieldsEnhanced++;
        }

        return fieldsEnhanced;
    }

    /// <summary>
    /// Get enhancement status for all entity types
    /// </summary>
    public async Task<EnhancementStatus> GetEnhancementStatusAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("📊 Getting enhancement status");

            var status = new EnhancementStatus();

            // BusinessColumnInfo status
            var totalColumns = await _context.BusinessColumnInfo.CountAsync(cancellationToken);
            var enhancedColumns = await _context.BusinessColumnInfo
                .Where(c => !string.IsNullOrEmpty(c.SemanticContext) ||
                           !string.IsNullOrEmpty(c.AnalyticalContext) ||
                           c.SemanticRelevanceScore > 0)
                .CountAsync(cancellationToken);

            status.BusinessColumnInfo = new EntityEnhancementStatus
            {
                TotalRecords = totalColumns,
                EnhancedRecords = enhancedColumns,
                CoveragePercentage = totalColumns > 0 ? Math.Round((double)enhancedColumns / totalColumns * 100, 1) : 0
            };

            // BusinessTableInfo status
            var totalTables = await _context.BusinessTableInfo.CountAsync(cancellationToken);
            var enhancedTables = await _context.BusinessTableInfo
                .Where(t => !string.IsNullOrEmpty(t.SemanticDescription) ||
                           !string.IsNullOrEmpty(t.LLMContextHints))
                .CountAsync(cancellationToken);

            status.BusinessTableInfo = new EntityEnhancementStatus
            {
                TotalRecords = totalTables,
                EnhancedRecords = enhancedTables,
                CoveragePercentage = totalTables > 0 ? Math.Round((double)enhancedTables / totalTables * 100, 1) : 0
            };

            // BusinessGlossary status
            var totalGlossary = await _context.BusinessGlossary.CountAsync(cancellationToken);
            var enhancedGlossary = await _context.BusinessGlossary
                .Where(g => !string.IsNullOrEmpty(g.ContextualVariations) ||
                           !string.IsNullOrEmpty(g.QueryPatterns))
                .CountAsync(cancellationToken);

            status.BusinessGlossary = new EntityEnhancementStatus
            {
                TotalRecords = totalGlossary,
                EnhancedRecords = enhancedGlossary,
                CoveragePercentage = totalGlossary > 0 ? Math.Round((double)enhancedGlossary / totalGlossary * 100, 1) : 0
            };

            _logger.LogInformation("📊 Enhancement status retrieved - Columns: {ColumnCoverage}%, Tables: {TableCoverage}%, Glossary: {GlossaryCoverage}%",
                status.BusinessColumnInfo.CoveragePercentage,
                status.BusinessTableInfo.CoveragePercentage,
                status.BusinessGlossary.CoveragePercentage);

            return status;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error getting enhancement status");
            throw;
        }
    }
}
