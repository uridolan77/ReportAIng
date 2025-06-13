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
}
