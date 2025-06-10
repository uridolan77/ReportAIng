using System.ComponentModel.DataAnnotations;

namespace BIReportingCopilot.Core.Models;

/// <summary>
/// Unified Query History Entity - Consolidates Core and Infrastructure QueryHistoryEntity classes
/// Replaces: Core/Models/QueryHistory.cs::QueryHistoryEntity, Infrastructure/Data/Entities/BaseEntity.cs::QueryHistoryEntity
/// </summary>
public class UnifiedQueryHistoryEntity
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

    // Execution Information
    public DateTime ExecutedAt { get; set; } = DateTime.UtcNow;
    public int ExecutionTimeMs { get; set; }
    public bool IsSuccessful { get; set; }
    public string? ErrorMessage { get; set; }
    public int RowCount { get; set; }

    // AI and Confidence Information
    public double ConfidenceScore { get; set; }
    public byte? UserFeedback { get; set; }

    // Database Context Information
    [MaxLength(100)]
    public string? DatabaseName { get; set; }

    [MaxLength(100)]
    public string? SchemaName { get; set; }

    public string? TablesUsed { get; set; } // JSON array of table names
    public string? ColumnsUsed { get; set; } // JSON array of column names

    // Query Classification
    public string? QueryType { get; set; } // SELECT, INSERT, UPDATE, etc.
    public string? QueryComplexity { get; set; } // Simple, Medium, Complex

    // Caching Information
    public bool FromCache { get; set; }
    public string? CacheKey { get; set; }

    // Request Context
    public string? UserAgent { get; set; }
    public string? IpAddress { get; set; }

    // Metadata and Extensions
    public string? Metadata { get; set; } // JSON for additional metadata

    // Base Entity Properties (from Infrastructure BaseEntity)
    public string CreatedBy { get; set; } = string.Empty;
    public string UpdatedBy { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;

    // Compatibility Properties (aliases for different naming conventions)
    public string NaturalLanguageQuery => Query; // Alias for Infrastructure compatibility
    public string GeneratedSQL => GeneratedSql ?? string.Empty; // Alias for Infrastructure compatibility
    public int? ResultRowCount => RowCount; // Alias for Infrastructure compatibility
    public decimal? ConfidenceScoreDecimal => (decimal)ConfidenceScore; // Alias for Infrastructure compatibility
    public DateTime QueryTimestamp => ExecutedAt; // Alias for Infrastructure compatibility
    public DateTime Timestamp => ExecutedAt; // Alias for Core compatibility
    public byte? QueryComplexityByte => QueryComplexity switch // Alias for Infrastructure compatibility
    {
        "Simple" or "Low" => 1,
        "Medium" => 2,
        "Complex" or "High" => 3,
        _ => null
    };
}

/// <summary>
/// Unified AI Generation Attempt Entity - Enhanced version from Core models
/// </summary>
public class UnifiedAIGenerationAttempt
{
    public long Id { get; set; }

    [Required]
    [MaxLength(500)]
    public string UserId { get; set; } = string.Empty;

    [Required]
    public string UserQuery { get; set; } = string.Empty;

    public string? GeneratedSql { get; set; }

    // AI Provider Information
    public string? AIProvider { get; set; }
    public string? ModelVersion { get; set; }

    // Timing and Performance
    public DateTime AttemptedAt { get; set; } = DateTime.UtcNow;
    public int GenerationTimeMs { get; set; }
    public bool IsSuccessful { get; set; }
    public string? ErrorMessage { get; set; }
    public double ConfidenceScore { get; set; }

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
    public string CreatedBy { get; set; } = string.Empty;
    public string UpdatedBy { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;
}

/// <summary>
/// Unified AI Feedback Entry Entity - Enhanced version from Core models
/// </summary>
public class UnifiedAIFeedbackEntry
{
    public long Id { get; set; }

    [Required]
    [MaxLength(500)]
    public string UserId { get; set; } = string.Empty;

    // Relationships
    public long? QueryHistoryId { get; set; }
    public long? GenerationAttemptId { get; set; }

    // Query Information
    [Required]
    public string OriginalQuery { get; set; } = string.Empty;
    public string? GeneratedSql { get; set; }
    public string? CorrectedSql { get; set; }

    // Feedback Details
    [Required]
    public string FeedbackType { get; set; } = string.Empty; // Positive, Negative, Correction, Suggestion
    public int Rating { get; set; } // 1-5 rating
    public string? Comments { get; set; }

    // Processing Information
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsProcessed { get; set; }
    public DateTime? ProcessedAt { get; set; }
    public string? ProcessingResult { get; set; }

    // Classification and Metadata
    public string? Category { get; set; }
    public string? Tags { get; set; } // JSON array of tags
    public string? Metadata { get; set; }

    // Base Entity Properties
    public string CreatedBy { get; set; } = string.Empty;
    public string UpdatedBy { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;

    // Compatibility Properties
    public string QueryId { get; set; } = string.Empty; // For backward compatibility
    public DateTime Timestamp => CreatedAt; // Alias for compatibility

    // Navigation Properties
    public UnifiedQueryHistoryEntity? QueryHistory { get; set; }
    public UnifiedAIGenerationAttempt? GenerationAttempt { get; set; }
}

/// <summary>
/// Unified Semantic Cache Entry Entity - Enhanced version from Core models
/// </summary>
public class UnifiedSemanticCacheEntry
{
    public int Id { get; set; }

    // Query Identification
    [Required]
    public string QueryHash { get; set; } = string.Empty; // Hash of normalized query

    [Required]
    public string OriginalQuery { get; set; } = string.Empty;

    [Required]
    public string NormalizedQuery { get; set; } = string.Empty;

    public string? GeneratedSql { get; set; }

    // Cache Data
    public string? ResultData { get; set; } // JSON of cached result
    public string? ResultMetadata { get; set; } // JSON of result metadata

    // Cache Lifecycle
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime LastAccessedAt { get; set; } = DateTime.UtcNow;
    public DateTime ExpiresAt { get; set; }
    public int AccessCount { get; set; } = 1;

    // Semantic Analysis
    public double SemanticSimilarityThreshold { get; set; } = 0.85;
    public string? SemanticVector { get; set; } // JSON array of embedding vector

    // Context and Classification
    public string? DatabaseContext { get; set; } // JSON of database/schema context
    public string? Tags { get; set; } // JSON array of tags

    // Storage Optimization
    public long SizeBytes { get; set; }
    public string? CompressionType { get; set; }

    // Metadata and Extensions
    public string? Metadata { get; set; } // JSON for additional metadata

    // Additional properties expected by Infrastructure services
    public string SerializedResponse { get; set; } = string.Empty;

    // Base Entity Properties
    public string CreatedBy { get; set; } = string.Empty;
    public string UpdatedBy { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;
}
