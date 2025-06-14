namespace BIReportingCopilot.Core.Models;

/// <summary>
/// Unified semantic cache entry for AI-powered caching
/// </summary>
public class UnifiedSemanticCacheEntry
{
    /// <summary>
    /// Unique identifier for the cache entry
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Cache key
    /// </summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>
    /// Original query or prompt
    /// </summary>
    public string Query { get; set; } = string.Empty;

    /// <summary>
    /// Cached value as JSON
    /// </summary>
    public string Value { get; set; } = string.Empty;

    /// <summary>
    /// Semantic embedding vector (as JSON array)
    /// </summary>
    public string? EmbeddingVector { get; set; }

    /// <summary>
    /// Cache entry type (query, prompt, result, etc.)
    /// </summary>
    public string EntryType { get; set; } = string.Empty;

    /// <summary>
    /// Similarity threshold for semantic matching
    /// </summary>
    public double SimilarityThreshold { get; set; } = 0.8;

    /// <summary>
    /// Number of times this entry has been accessed
    /// </summary>
    public int AccessCount { get; set; }

    /// <summary>
    /// Last access timestamp
    /// </summary>
    public DateTime LastAccessedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Cache entry expiration timestamp
    /// </summary>
    public DateTime ExpiresAt { get; set; }

    /// <summary>
    /// Additional metadata as JSON
    /// </summary>
    public string? Metadata { get; set; }

    /// <summary>
    /// Created timestamp
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Updated timestamp
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Query hash for unique identification (for compatibility with BICopilotContext)
    /// </summary>
    public string QueryHash { get; set; } = string.Empty;

    /// <summary>
    /// Original query text (for compatibility with BICopilotContext)
    /// </summary>
    public string OriginalQuery { get; set; } = string.Empty;

    /// <summary>
    /// Normalized query text (for compatibility with BICopilotContext)
    /// </summary>
    public string NormalizedQuery { get; set; } = string.Empty;

    /// <summary>
    /// User who created the record (for compatibility with BICopilotContext)
    /// </summary>
    public string? CreatedBy { get; set; }

    /// <summary>
    /// User who last updated the record (for compatibility with BICopilotContext)
    /// </summary>
    public string? UpdatedBy { get; set; }

    /// <summary>
    /// Generated SQL query (for compatibility with SemanticCacheService)
    /// </summary>
    public string? GeneratedSql { get; set; }

    /// <summary>
    /// Result data as dictionary (for compatibility with SemanticCacheService)
    /// </summary>
    public Dictionary<string, object>? ResultData { get; set; }
}
