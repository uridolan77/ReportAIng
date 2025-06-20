using BIReportingCopilot.Core.Models;

namespace BIReportingCopilot.Core.Interfaces.Schema;

/// <summary>
/// Interface for enhanced semantic layer service that builds upon existing schema management
/// with richer semantic metadata and dynamic contextualization
/// </summary>
public interface IEnhancedSemanticLayerService
{
    /// <summary>
    /// Get semantically enriched schema information for a query
    /// </summary>
    /// <param name="naturalLanguageQuery">The natural language query to analyze</param>
    /// <param name="relevanceThreshold">Minimum relevance score threshold (0.0 to 1.0)</param>
    /// <param name="maxTables">Maximum number of tables to return</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Enhanced schema result with semantic enrichment</returns>
    Task<EnhancedSchemaResult> GetEnhancedSchemaAsync(
        string naturalLanguageQuery, 
        double relevanceThreshold = 0.7,
        int maxTables = 10,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Enrich existing schema metadata with semantic information
    /// </summary>
    /// <param name="request">Semantic enrichment request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Semantic enrichment response</returns>
    Task<SemanticEnrichmentResponse> EnrichSchemaMetadataAsync(
        SemanticEnrichmentRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Update semantic metadata for a specific table
    /// </summary>
    /// <param name="tableName">Name of the table to update</param>
    /// <param name="schemaName">Schema name</param>
    /// <param name="semanticMetadata">Semantic metadata to apply</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Success indicator</returns>
    Task<bool> UpdateTableSemanticMetadataAsync(
        string tableName,
        string schemaName,
        TableSemanticMetadata semanticMetadata,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Update semantic metadata for a specific column
    /// </summary>
    /// <param name="tableName">Name of the table</param>
    /// <param name="columnName">Name of the column to update</param>
    /// <param name="semanticMetadata">Semantic metadata to apply</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Success indicator</returns>
    Task<bool> UpdateColumnSemanticMetadataAsync(
        string tableName,
        string columnName,
        ColumnSemanticMetadata semanticMetadata,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Generate semantic embeddings for schema elements
    /// </summary>
    /// <param name="forceRegeneration">Whether to force regeneration of existing embeddings</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Number of embeddings generated</returns>
    Task<int> GenerateSemanticEmbeddingsAsync(
        bool forceRegeneration = false,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Validate semantic metadata completeness and consistency
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Validation result with issues and recommendations</returns>
    Task<SemanticValidationResult> ValidateSemanticMetadataAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get semantic similarity between two schema elements
    /// </summary>
    /// <param name="element1">First schema element</param>
    /// <param name="element2">Second schema element</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Similarity score (0.0 to 1.0)</returns>
    Task<double> GetSemanticSimilarityAsync(
        SchemaElement element1,
        SchemaElement element2,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Find semantically similar schema elements
    /// </summary>
    /// <param name="referenceElement">Reference schema element</param>
    /// <param name="similarityThreshold">Minimum similarity threshold</param>
    /// <param name="maxResults">Maximum number of results</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of similar schema elements with similarity scores</returns>
    Task<List<SimilarSchemaElement>> FindSimilarSchemaElementsAsync(
        SchemaElement referenceElement,
        double similarityThreshold = 0.7,
        int maxResults = 10,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get business glossary terms relevant to a query
    /// </summary>
    /// <param name="query">Natural language query</param>
    /// <param name="maxTerms">Maximum number of terms to return</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of relevant business glossary terms</returns>
    Task<List<RelevantGlossaryTerm>> GetRelevantGlossaryTermsAsync(
        string query,
        int maxTerms = 20,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Generate LLM-optimized context for a specific query intent
    /// </summary>
    /// <param name="query">Natural language query</param>
    /// <param name="intent">Query intent</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>LLM-optimized context</returns>
    Task<LLMOptimizedContext> GenerateLLMContextAsync(
        string query,
        QueryIntent intent,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Table semantic metadata for updates
/// </summary>
public class TableSemanticMetadata
{
    public string SemanticDescription { get; set; } = string.Empty;
    public List<string> BusinessProcesses { get; set; } = new();
    public List<string> AnalyticalUseCases { get; set; } = new();
    public List<string> ReportingCategories { get; set; } = new();
    public Dictionary<string, string> SemanticRelationships { get; set; } = new();
    public List<string> QueryComplexityHints { get; set; } = new();
    public List<string> BusinessGlossaryTerms { get; set; } = new();
    public decimal SemanticCoverageScore { get; set; } = 0.5m;
    public List<string> LLMContextHints { get; set; } = new();
    public List<string> VectorSearchKeywords { get; set; } = new();
}

/// <summary>
/// Column semantic metadata for updates
/// </summary>
public class ColumnSemanticMetadata
{
    public string SemanticContext { get; set; } = string.Empty;
    public List<string> ConceptualRelationships { get; set; } = new();
    public List<string> DomainSpecificTerms { get; set; } = new();
    public Dictionary<string, string> QueryIntentMapping { get; set; } = new();
    public List<string> BusinessQuestionTypes { get; set; } = new();
    public List<string> SemanticSynonyms { get; set; } = new();
    public string AnalyticalContext { get; set; } = string.Empty;
    public List<string> BusinessMetrics { get; set; } = new();
    public decimal SemanticRelevanceScore { get; set; } = 0.5m;
    public List<string> LLMPromptHints { get; set; } = new();
    public List<string> VectorSearchTags { get; set; } = new();
}

/// <summary>
/// Schema element for similarity comparison
/// </summary>
public class SchemaElement
{
    public string Type { get; set; } = string.Empty; // "table" or "column"
    public string Name { get; set; } = string.Empty;
    public string SchemaName { get; set; } = string.Empty;
    public string TableName { get; set; } = string.Empty; // For columns
    public string BusinessMeaning { get; set; } = string.Empty;
    public string SemanticContext { get; set; } = string.Empty;
    public List<string> Tags { get; set; } = new();
}

/// <summary>
/// Similar schema element with similarity score
/// </summary>
public class SimilarSchemaElement
{
    public SchemaElement Element { get; set; } = new();
    public double SimilarityScore { get; set; } = 0.0;
    public string SimilarityReason { get; set; } = string.Empty;
}

/// <summary>
/// Relevant business glossary term
/// </summary>
public class RelevantGlossaryTerm
{
    public string Term { get; set; } = string.Empty;
    public string Definition { get; set; } = string.Empty;
    public string BusinessContext { get; set; } = string.Empty;
    public double RelevanceScore { get; set; } = 0.0;
    public List<string> RelatedTables { get; set; } = new();
    public List<string> RelatedColumns { get; set; } = new();
}

/// <summary>
/// Semantic validation result
/// </summary>
public class SemanticValidationResult
{
    public bool IsValid { get; set; } = true;
    public List<SemanticValidationIssue> Issues { get; set; } = new();
    public List<string> Recommendations { get; set; } = new();
    public double OverallCompletenessScore { get; set; } = 0.0;
    public Dictionary<string, double> TableCompletenessScores { get; set; } = new();
}

/// <summary>
/// Semantic validation issue
/// </summary>
public class SemanticValidationIssue
{
    public string Severity { get; set; } = string.Empty; // "Error", "Warning", "Info"
    public string Category { get; set; } = string.Empty; // "Completeness", "Consistency", "Quality"
    public string Description { get; set; } = string.Empty;
    public string TableName { get; set; } = string.Empty;
    public string ColumnName { get; set; } = string.Empty;
    public string Recommendation { get; set; } = string.Empty;
}
