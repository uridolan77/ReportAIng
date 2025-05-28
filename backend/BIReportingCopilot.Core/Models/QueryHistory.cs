using System.ComponentModel.DataAnnotations;

namespace BIReportingCopilot.Core.Models;

/// <summary>
/// Entity for tracking query history and analytics
/// </summary>
public class QueryHistoryEntity
{
    public int Id { get; set; }
    
    [Required]
    [MaxLength(500)]
    public string UserId { get; set; } = string.Empty;
    
    [Required]
    public string Query { get; set; } = string.Empty;
    
    public string? GeneratedSql { get; set; }
    
    public DateTime ExecutedAt { get; set; } = DateTime.UtcNow;
    
    public int ExecutionTimeMs { get; set; }
    
    public bool IsSuccessful { get; set; }
    
    public string? ErrorMessage { get; set; }
    
    public int RowCount { get; set; }
    
    public double ConfidenceScore { get; set; }
    
    [MaxLength(100)]
    public string? DatabaseName { get; set; }
    
    [MaxLength(100)]
    public string? SchemaName { get; set; }
    
    public string? TablesUsed { get; set; } // JSON array of table names
    
    public string? ColumnsUsed { get; set; } // JSON array of column names
    
    public string? QueryType { get; set; } // SELECT, INSERT, UPDATE, etc.
    
    public string? QueryComplexity { get; set; } // Simple, Medium, Complex
    
    public string? Metadata { get; set; } // JSON for additional metadata
    
    public bool FromCache { get; set; }
    
    public string? CacheKey { get; set; }
    
    public string? UserAgent { get; set; }
    
    public string? IpAddress { get; set; }
    
    public string? SessionId { get; set; }
}

/// <summary>
/// Entity for AI generation attempts and feedback
/// </summary>
public class AIGenerationAttempt
{
    public int Id { get; set; }
    
    [Required]
    [MaxLength(500)]
    public string UserId { get; set; } = string.Empty;
    
    [Required]
    public string UserQuery { get; set; } = string.Empty;
    
    public string? GeneratedSql { get; set; }
    
    public string? AIProvider { get; set; }
    
    public string? ModelVersion { get; set; }
    
    public DateTime AttemptedAt { get; set; } = DateTime.UtcNow;
    
    public int GenerationTimeMs { get; set; }
    
    public bool IsSuccessful { get; set; }
    
    public string? ErrorMessage { get; set; }
    
    public double ConfidenceScore { get; set; }
    
    public string? PromptTemplate { get; set; }
    
    public string? ContextData { get; set; } // JSON for schema context, etc.
    
    public int TokensUsed { get; set; }
    
    public decimal? Cost { get; set; }
    
    public string? Feedback { get; set; } // User feedback on the generation
    
    public int? Rating { get; set; } // 1-5 rating
    
    public bool WasExecuted { get; set; }
    
    public bool WasModified { get; set; }
    
    public string? ModifiedSql { get; set; }
    
    public string? Metadata { get; set; } // JSON for additional metadata
}

/// <summary>
/// Entity for AI feedback and learning
/// </summary>
public class AIFeedbackEntry
{
    public int Id { get; set; }
    
    [Required]
    [MaxLength(500)]
    public string UserId { get; set; } = string.Empty;
    
    public int? QueryHistoryId { get; set; }
    
    public int? GenerationAttemptId { get; set; }
    
    [Required]
    public string OriginalQuery { get; set; } = string.Empty;
    
    public string? GeneratedSql { get; set; }
    
    public string? CorrectedSql { get; set; }
    
    [Required]
    public string FeedbackType { get; set; } = string.Empty; // Positive, Negative, Correction, Suggestion
    
    public int Rating { get; set; } // 1-5 rating
    
    public string? Comments { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public bool IsProcessed { get; set; }
    
    public DateTime? ProcessedAt { get; set; }
    
    public string? ProcessingResult { get; set; }
    
    public string? Tags { get; set; } // JSON array of tags
    
    public string? Category { get; set; } // Syntax, Logic, Performance, etc.
    
    public string? Metadata { get; set; } // JSON for additional metadata
    
    // Navigation properties
    public QueryHistoryEntity? QueryHistory { get; set; }
    public AIGenerationAttempt? GenerationAttempt { get; set; }
}

/// <summary>
/// Entity for semantic cache entries
/// </summary>
public class SemanticCacheEntry
{
    public int Id { get; set; }
    
    [Required]
    public string QueryHash { get; set; } = string.Empty; // Hash of normalized query
    
    [Required]
    public string OriginalQuery { get; set; } = string.Empty;
    
    [Required]
    public string NormalizedQuery { get; set; } = string.Empty;
    
    public string? GeneratedSql { get; set; }
    
    public string? ResultData { get; set; } // JSON of cached result
    
    public string? ResultMetadata { get; set; } // JSON of result metadata
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime LastAccessedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime ExpiresAt { get; set; }
    
    public int AccessCount { get; set; } = 1;
    
    public double SemanticSimilarityThreshold { get; set; } = 0.85;
    
    public string? SemanticVector { get; set; } // JSON array of embedding vector
    
    public string? DatabaseContext { get; set; } // JSON of database/schema context
    
    public string? Tags { get; set; } // JSON array of tags
    
    public bool IsActive { get; set; } = true;
    
    public string? Metadata { get; set; } // JSON for additional metadata
    
    public long SizeBytes { get; set; }
    
    public string? CompressionType { get; set; }
}
