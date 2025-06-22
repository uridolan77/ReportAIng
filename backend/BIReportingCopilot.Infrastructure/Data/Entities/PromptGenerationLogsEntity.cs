using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BIReportingCopilot.Infrastructure.Data.Entities;

/// <summary>
/// Entity for tracking prompt generation details and analytics
/// </summary>
[Table("PromptGenerationLogs")]
public class PromptGenerationLogsEntity
{
    /// <summary>
    /// Unique identifier for the prompt generation log
    /// </summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    /// <summary>
    /// User ID who initiated the prompt generation
    /// </summary>
    [Required]
    [MaxLength(256)]
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// Original user question that triggered prompt generation
    /// </summary>
    [Required]
    [MaxLength(1000)]
    public string UserQuestion { get; set; } = string.Empty;

    /// <summary>
    /// The generated prompt sent to the AI model
    /// </summary>
    [Required]
    [Column(TypeName = "nvarchar(max)")]
    public string GeneratedPrompt { get; set; } = string.Empty;

    /// <summary>
    /// Length of the generated prompt in characters
    /// </summary>
    public int PromptLength { get; set; }

    /// <summary>
    /// Classified intent type (e.g., Reporting, Analytical, Comparative)
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string IntentType { get; set; } = string.Empty;

    /// <summary>
    /// Business domain classification
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string Domain { get; set; } = string.Empty;

    /// <summary>
    /// Confidence score for the prompt generation
    /// </summary>
    [Column(TypeName = "decimal(5,4)")]
    public decimal ConfidenceScore { get; set; }

    /// <summary>
    /// Number of database tables used in prompt context
    /// </summary>
    public int TablesUsed { get; set; }

    /// <summary>
    /// Time taken to generate the prompt in milliseconds
    /// </summary>
    public int GenerationTimeMs { get; set; }

    /// <summary>
    /// Template used for prompt generation
    /// </summary>
    [MaxLength(100)]
    public string? TemplateUsed { get; set; }

    /// <summary>
    /// Whether the prompt generation was successful
    /// </summary>
    public bool WasSuccessful { get; set; } = true;

    /// <summary>
    /// Error message if generation failed
    /// </summary>
    [MaxLength(1000)]
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Timestamp when the prompt was generated
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Extracted entities from the user question (JSON)
    /// </summary>
    [MaxLength(500)]
    public string? ExtractedEntities { get; set; }

    /// <summary>
    /// Time context information (JSON)
    /// </summary>
    [MaxLength(200)]
    public string? TimeContext { get; set; }

    /// <summary>
    /// Number of tokens used in the prompt
    /// </summary>
    public int? TokensUsed { get; set; }

    /// <summary>
    /// Estimated cost for this prompt generation
    /// </summary>
    [Column(TypeName = "decimal(10,4)")]
    public decimal? CostEstimate { get; set; }

    /// <summary>
    /// AI model used for generation
    /// </summary>
    [MaxLength(50)]
    public string? ModelUsed { get; set; }

    /// <summary>
    /// User rating for the generated prompt (1-5 scale)
    /// </summary>
    [Column(TypeName = "decimal(3,2)")]
    public decimal? UserRating { get; set; }

    /// <summary>
    /// Whether SQL was successfully generated from this prompt
    /// </summary>
    public bool? SqlGeneratedSuccessfully { get; set; }

    /// <summary>
    /// Whether the generated query executed successfully
    /// </summary>
    public bool? QueryExecutedSuccessfully { get; set; }

    /// <summary>
    /// User feedback comments
    /// </summary>
    [MaxLength(500)]
    public string? UserFeedback { get; set; }

    /// <summary>
    /// Reference to the prompt template used
    /// </summary>
    public long? PromptTemplateId { get; set; }

    /// <summary>
    /// Session ID for tracking related operations
    /// </summary>
    [MaxLength(100)]
    public string? SessionId { get; set; }

    /// <summary>
    /// Request ID for correlation across services
    /// </summary>
    [MaxLength(100)]
    public string? RequestId { get; set; }

    /// <summary>
    /// Calculate success rate for this prompt
    /// </summary>
    public double GetSuccessRate()
    {
        var factors = new List<bool?>
        {
            WasSuccessful,
            SqlGeneratedSuccessfully,
            QueryExecutedSuccessfully
        };

        var validFactors = factors.Where(f => f.HasValue).ToList();
        if (!validFactors.Any()) return 0.0;

        return validFactors.Count(f => f.Value) / (double)validFactors.Count;
    }

    /// <summary>
    /// Update execution results
    /// </summary>
    public void UpdateExecutionResults(bool sqlGenerated, bool queryExecuted, string? errorMessage = null)
    {
        SqlGeneratedSuccessfully = sqlGenerated;
        QueryExecutedSuccessfully = queryExecuted;
        
        if (!string.IsNullOrEmpty(errorMessage))
        {
            ErrorMessage = errorMessage;
            WasSuccessful = false;
        }
    }
}
