using System.ComponentModel.DataAnnotations;

namespace BIReportingCopilot.Core.Models;

/// <summary>
/// Query History Entity - Consolidates Core and Infrastructure QueryHistoryEntity classes
/// </summary>
public class QueryHistoryEntity
{
    // Primary Key
    public long Id { get; set; }

    // Core Query Information
    [Required]
    [MaxLength(500)]
    public string UserId { get; set; } = string.Empty;

    [Required]
    public string SessionId { get; set; } = string.Empty;

    [Required]
    public string Query { get; set; } = string.Empty;

    public string? GeneratedSql { get; set; }
    public string? ExecutedSql { get; set; }
    public bool IsSuccessful { get; set; }
    public string? ErrorMessage { get; set; }
    public int? RowCount { get; set; }
    public double? ExecutionTimeMs { get; set; }

    // Timestamps
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Base Entity Properties
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }

    // Query Metadata
    public string? QueryType { get; set; }
    public string? DataSource { get; set; }
    public string? DatabaseName { get; set; }
    public string? SchemaName { get; set; }

    // Performance Metrics
    public double? ConfidenceScore { get; set; }
    public string? OptimizationSuggestions { get; set; }
    public string? PerformanceMetrics { get; set; }

    // User Interaction
    public string? UserFeedback { get; set; }
    public int? UserRating { get; set; }
    public bool? WasHelpful { get; set; }

    // Caching and Optimization
    public bool CacheHit { get; set; }
    public string? CacheKey { get; set; }
    public DateTime? CacheExpiry { get; set; }

    // AI and ML Metrics
    public double? NLUConfidence { get; set; }
    public double? QueryIntelligenceScore { get; set; }
    public double? OptimizationScore { get; set; }

    // Additional Context
    public string? Tags { get; set; }
    public string? Notes { get; set; }
    public string? Metadata { get; set; } // JSON for additional metadata

    // Compatibility Properties for different naming conventions
    public string? NaturalLanguageQuery => Query;
    public string? SQL => GeneratedSql;
    public DateTime? Timestamp => CreatedAt;
    public string? User => UserId;
    public bool? Success => IsSuccessful;
    public string? Error => ErrorMessage;
    public double? Duration => ExecutionTimeMs;
    public double? Confidence => ConfidenceScore;
    public string? Feedback => UserFeedback;
    public int? Rating => UserRating;

    // Properties expected by Infrastructure services
    /// <summary>
    /// Executed at timestamp
    /// </summary>
    public DateTime ExecutedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Result count (settable property)
    /// </summary>
    public int ResultCount { get; set; }

    // Additional properties expected by Infrastructure services
    /// <summary>
    /// Query timestamp (alias for ExecutedAt)
    /// </summary>
    public DateTime QueryTimestamp => ExecutedAt;
}

/// <summary>
/// AI Generation Attempt Entity - Enhanced version from Core models
/// </summary>
public class AIGenerationAttempt
{
    public long Id { get; set; }

    // Core Information
    [Required]
    public string UserId { get; set; } = string.Empty;

    [Required]
    public string SessionId { get; set; } = string.Empty;

    [Required]
    public string Prompt { get; set; } = string.Empty;

    public string? GeneratedContent { get; set; }
    public bool IsSuccessful { get; set; }
    public string? ErrorMessage { get; set; }

    // Timestamps
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }

    // AI Context
    public string? PromptTemplate { get; set; }
    public string? ContextData { get; set; } // JSON for schema context, etc.

    // Cost and Usage Tracking
    public int TokensUsed { get; set; }
    public decimal? Cost { get; set; }

    // User Feedback
    public string? Feedback { get; set; } // User feedback on the generation
    public int? Rating { get; set; } // 1-5 rating

    // Execution Tracking
    public bool WasExecuted { get; set; }
    public bool WasModified { get; set; }
    public string? ModifiedSql { get; set; }

    // Metadata and Extensions
    public string? Metadata { get; set; } // JSON for additional metadata

    // Base Entity Properties
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Performance Metrics
    public double? ProcessingTimeMs { get; set; }
    public double? ConfidenceScore { get; set; }

    // Model Information
    public string? ModelName { get; set; }
    public string? ModelVersion { get; set; }
    public string? ProviderName { get; set; }

    // Request Configuration
    public string? RequestParameters { get; set; } // JSON of AI request parameters

    // Compatibility Properties
    public string QueryId { get; set; } = string.Empty; // For backward compatibility
    public DateTime Timestamp => CreatedAt; // Alias for compatibility

    // Navigation Properties
    public QueryHistoryEntity? QueryHistory { get; set; }
    public AIGenerationAttempt? GenerationAttempt { get; set; }
}

/// <summary>
/// AI Feedback Entry Entity - Enhanced version from Core models
/// </summary>
public class AIFeedbackEntry
{
    public long Id { get; set; }

    // Core Information
    [Required]
    public string UserId { get; set; } = string.Empty;

    [Required]
    public string QueryId { get; set; } = string.Empty; // Reference to query

    [Required]
    public string FeedbackType { get; set; } = string.Empty; // "rating", "correction", "suggestion"

    public string? FeedbackContent { get; set; }
    public int? Rating { get; set; } // 1-5 rating
    public bool? IsHelpful { get; set; }

    // Timestamps
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Context
    public string? OriginalQuery { get; set; }
    public string? GeneratedSql { get; set; }
    public string? CorrectedSql { get; set; }

    // Processing Status
    public bool IsProcessed { get; set; }
    public DateTime? ProcessedAt { get; set; }
    public string? ProcessingNotes { get; set; }

    // Metadata
    public string? Metadata { get; set; } // JSON for additional metadata

    // Base Entity Properties
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
