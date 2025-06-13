namespace BIReportingCopilot.Core.Models;

/// <summary>
/// Unified query history entity for tracking query execution history
/// </summary>
public class UnifiedQueryHistoryEntity
{
    /// <summary>
    /// Unique identifier for the query history entry
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// User ID who executed the query
    /// </summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// Original natural language query
    /// </summary>
    public string OriginalQuery { get; set; } = string.Empty;

    /// <summary>
    /// Generated SQL query
    /// </summary>
    public string GeneratedSql { get; set; } = string.Empty;

    /// <summary>
    /// Query execution status
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Execution time in milliseconds
    /// </summary>
    public long ExecutionTimeMs { get; set; }

    /// <summary>
    /// Number of rows returned
    /// </summary>
    public int RowCount { get; set; }

    /// <summary>
    /// Error message if query failed
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Query metadata as JSON
    /// </summary>
    public string? Metadata { get; set; }

    /// <summary>
    /// Query result data as JSON (for caching)
    /// </summary>
    public string? ResultData { get; set; }

    /// <summary>
    /// Query execution timestamp
    /// </summary>
    public DateTime ExecutedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Created timestamp
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Updated timestamp
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
