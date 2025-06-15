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
    /// Query timestamp (alias for ExecutedAt for compatibility)
    /// </summary>
    public DateTime QueryTimestamp => ExecutedAt;

    /// <summary>
    /// Created timestamp
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Updated timestamp
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Properties expected by Infrastructure services
    /// <summary>
    /// Session ID for the query
    /// </summary>
    public string SessionId { get; set; } = string.Empty;

    /// <summary>
    /// Natural language query (settable property)
    /// </summary>
    public string Query { get; set; } = string.Empty;

    /// <summary>
    /// Whether the query was successful (settable property)
    /// </summary>
    public bool IsSuccessful { get; set; }

    /// <summary>
    /// User who created the record
    /// </summary>
    public string? CreatedBy { get; set; }

    /// <summary>
    /// User who last updated the record
    /// </summary>
    public string? UpdatedBy { get; set; }

    /// <summary>
    /// Created date (settable property)
    /// </summary>
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Last updated date (settable property)
    /// </summary>
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

    // Additional properties expected by Infrastructure services
    /// <summary>
    /// Confidence score of the query result
    /// </summary>
    public double ConfidenceScore { get; set; }

    /// <summary>
    /// User feedback on the query
    /// </summary>
    public string? UserFeedback { get; set; }

    /// <summary>
    /// Query complexity rating
    /// </summary>
    public string? QueryComplexity { get; set; }

    /// <summary>
    /// Database name where query was executed
    /// </summary>
    public string? DatabaseName { get; set; }

    /// <summary>
    /// Schema name where query was executed
    /// </summary>
    public string? SchemaName { get; set; }

    /// <summary>
    /// Whether the record is active
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// SQL query (alias for GeneratedSql for compatibility)
    /// </summary>
    public string Sql => GeneratedSql;

    /// <summary>
    /// Query explanation
    /// </summary>
    public string? Explanation { get; set; }

    /// <summary>
    /// Confidence score as property
    /// </summary>
    public double Confidence => ConfidenceScore;

    /// <summary>
    /// Query classification
    /// </summary>
    public string? Classification { get; set; }
}
