namespace BIReportingCopilot.Core.Models;

/// <summary>
/// Unified AI generation attempt for tracking AI model usage and performance
/// </summary>
public class UnifiedAIGenerationAttempt
{
    /// <summary>
    /// Unique identifier for the generation attempt
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// User ID who initiated the generation
    /// </summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// AI provider used (OpenAI, Azure, etc.)
    /// </summary>
    public string Provider { get; set; } = string.Empty;

    /// <summary>
    /// AI model used (gpt-4, gpt-3.5-turbo, etc.)
    /// </summary>
    public string Model { get; set; } = string.Empty;

    /// <summary>
    /// Input prompt or query
    /// </summary>
    public string InputPrompt { get; set; } = string.Empty;

    /// <summary>
    /// Generated output
    /// </summary>
    public string? GeneratedOutput { get; set; }

    /// <summary>
    /// Generation status (success, failed, timeout, etc.)
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Error message if generation failed
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Response time in milliseconds
    /// </summary>
    public long ResponseTimeMs { get; set; }

    /// <summary>
    /// Number of tokens in the input
    /// </summary>
    public int InputTokens { get; set; }

    /// <summary>
    /// Number of tokens in the output
    /// </summary>
    public int OutputTokens { get; set; }

    /// <summary>
    /// Total tokens used
    /// </summary>
    public int TotalTokens { get; set; }

    /// <summary>
    /// Cost of the generation in USD
    /// </summary>
    public decimal Cost { get; set; }

    /// <summary>
    /// Generation type (sql, insight, explanation, etc.)
    /// </summary>
    public string GenerationType { get; set; } = string.Empty;

    /// <summary>
    /// Quality score (if available)
    /// </summary>
    public double? QualityScore { get; set; }

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
    /// When the attempt was made (for compatibility with BICopilotContext)
    /// </summary>
    public DateTime AttemptedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// AI provider used (for compatibility with BICopilotContext)
    /// </summary>
    public string AIProvider { get; set; } = string.Empty;

    /// <summary>
    /// Model version (for compatibility with BICopilotContext)
    /// </summary>
    public string ModelVersion { get; set; } = string.Empty;

    /// <summary>
    /// User who created the record (for compatibility with BICopilotContext)
    /// </summary>
    public string? CreatedBy { get; set; }

    /// <summary>
    /// User who last updated the record (for compatibility with BICopilotContext)
    /// </summary>
    public string? UpdatedBy { get; set; }
}
