using System.ComponentModel.DataAnnotations;

namespace BIReportingCopilot.Core.Models;

// QueryHistoryEntity has been consolidated into UnifiedQueryHistoryEntity
// See: Core/Models/UnifiedQueryHistoryEntity.cs

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
    public DateTime AttemptedAt { get; set; } = DateTime.UtcNow;

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
    public string? AIProvider { get; set; }

    // Request Configuration
    public string? RequestParameters { get; set; } // JSON of AI request parameters

    // Compatibility Properties
    public string QueryId { get; set; } = string.Empty; // For backward compatibility
    public DateTime Timestamp => CreatedAt; // Alias for compatibility

    // Navigation Properties
    public UnifiedQueryHistoryEntity? QueryHistory { get; set; }
    public AIGenerationAttempt? GenerationAttempt { get; set; }
}

// AIFeedbackEntry has been consolidated into UnifiedAIFeedbackEntry
// See: Core/Models/UnifiedAIFeedbackEntry.cs
