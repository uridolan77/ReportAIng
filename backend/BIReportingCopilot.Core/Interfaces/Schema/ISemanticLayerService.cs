using BIReportingCopilot.Core.DTOs;
using BIReportingCopilot.Core.Models;

namespace BIReportingCopilot.Core.Interfaces.Schema;

/// <summary>
/// Interface for semantic layer service that bridges raw database schemas with business-friendly terminology
/// </summary>
public interface ISemanticLayerService
{
    /// <summary>
    /// Get business-friendly schema description for LLM consumption
    /// </summary>
    /// <param name="naturalLanguageQuery">The user's natural language query</param>
    /// <param name="maxTokens">Maximum tokens to use in the description</param>
    /// <returns>Business-friendly schema description</returns>
    Task<string> GetBusinessFriendlySchemaAsync(string naturalLanguageQuery, int maxTokens = 2000);

    /// <summary>
    /// Update semantic metadata for a table
    /// </summary>
    /// <param name="tableId">Table ID to update</param>
    /// <param name="request">Semantic metadata update request</param>
    /// <returns>True if successful</returns>
    Task<bool> UpdateTableSemanticMetadataAsync(long tableId, UpdateTableSemanticRequest request);

    /// <summary>
    /// Create or update semantic schema mapping
    /// </summary>
    /// <param name="request">Semantic mapping creation request</param>
    /// <returns>Created mapping ID</returns>
    Task<long> CreateSemanticMappingAsync(CreateSemanticMappingRequest request);
}

/// <summary>
/// Interface for dynamic schema contextualization service
/// </summary>
public interface IDynamicSchemaContextualizationService
{
    /// <summary>
    /// Get contextually relevant schema elements for a given natural language query
    /// </summary>
    /// <param name="naturalLanguageQuery">The user's natural language query</param>
    /// <param name="relevanceThreshold">Minimum relevance score threshold</param>
    /// <param name="maxTables">Maximum number of tables to return</param>
    /// <param name="maxColumnsPerTable">Maximum columns per table</param>
    /// <returns>Contextualized schema result</returns>
    Task<ContextualizedSchemaResult> GetRelevantSchemaAsync(
        string naturalLanguageQuery, 
        double relevanceThreshold = 0.7,
        int maxTables = 10,
        int maxColumnsPerTable = 15);
}

/// <summary>
/// Result of contextualized schema analysis
/// </summary>
public class ContextualizedSchemaResult
{
    public string Query { get; set; } = string.Empty;
    public QueryAnalysisResult QueryAnalysis { get; set; } = new();
    public List<EnhancedBusinessTableDto> RelevantTables { get; set; } = new();
    public List<EnhancedBusinessColumnDto> RelevantColumns { get; set; } = new();
    public List<BusinessGlossaryDto> RelevantGlossaryTerms { get; set; } = new();
    public List<string> BusinessTermsUsed { get; set; } = new();
    public decimal ConfidenceScore { get; set; } = 0.0m;
    public int TokenEstimate { get; set; } = 0;
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
}

// QueryAnalysisResult is defined in BIReportingCopilot.Core.Models
