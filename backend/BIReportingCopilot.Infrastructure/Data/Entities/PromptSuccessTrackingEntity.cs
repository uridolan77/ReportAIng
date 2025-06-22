using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BIReportingCopilot.Infrastructure.Data.Entities;

/// <summary>
/// Entity for tracking end-to-end prompt success metrics and user feedback
/// </summary>
[Table("PromptSuccessTracking")]
public class PromptSuccessTrackingEntity
{
    /// <summary>
    /// Unique identifier for the success tracking record
    /// </summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    /// <summary>
    /// Session ID for tracking related operations
    /// </summary>
    [Required]
    [MaxLength(128)]
    public string SessionId { get; set; } = string.Empty;

    /// <summary>
    /// User ID who initiated the request
    /// </summary>
    [Required]
    [MaxLength(512)]
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// Original user question
    /// </summary>
    [Required]
    [MaxLength(2000)]
    public string UserQuestion { get; set; } = string.Empty;

    /// <summary>
    /// Generated prompt sent to AI
    /// </summary>
    [Required]
    [Column(TypeName = "nvarchar(max)")]
    public string GeneratedPrompt { get; set; } = string.Empty;

    /// <summary>
    /// Template used for prompt generation
    /// </summary>
    [MaxLength(100)]
    public string? TemplateUsed { get; set; }

    /// <summary>
    /// Classified intent type
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string IntentClassified { get; set; } = string.Empty;

    /// <summary>
    /// Classified business domain
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string DomainClassified { get; set; } = string.Empty;

    /// <summary>
    /// Tables retrieved for context (JSON array)
    /// </summary>
    [MaxLength(1000)]
    public string? TablesRetrieved { get; set; }

    /// <summary>
    /// Generated SQL query
    /// </summary>
    [Column(TypeName = "nvarchar(max)")]
    public string? GeneratedSQL { get; set; }

    /// <summary>
    /// Whether SQL execution was successful
    /// </summary>
    public bool? SQLExecutionSuccess { get; set; }

    /// <summary>
    /// SQL execution error message if failed
    /// </summary>
    [MaxLength(2000)]
    public string? SQLExecutionError { get; set; }

    /// <summary>
    /// User feedback rating (1-5 scale)
    /// </summary>
    public int? UserFeedbackRating { get; set; }

    /// <summary>
    /// User feedback comments
    /// </summary>
    [MaxLength(1000)]
    public string? UserFeedbackComments { get; set; }

    /// <summary>
    /// Total processing time in milliseconds
    /// </summary>
    public int ProcessingTimeMs { get; set; }

    /// <summary>
    /// Overall confidence score for the operation
    /// </summary>
    [Column(TypeName = "decimal(5,4)")]
    public decimal ConfidenceScore { get; set; }

    /// <summary>
    /// When this record was created
    /// </summary>
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// When this record was last updated
    /// </summary>
    public DateTime? UpdatedDate { get; set; }

    /// <summary>
    /// Calculate overall success score based on multiple factors
    /// </summary>
    public double CalculateOverallSuccessScore()
    {
        var scores = new List<double>();

        // Prompt generation success (always true if we have a record)
        scores.Add(1.0);

        // SQL generation success
        if (!string.IsNullOrEmpty(GeneratedSQL))
        {
            scores.Add(1.0);
        }
        else
        {
            scores.Add(0.0);
        }

        // SQL execution success
        if (SQLExecutionSuccess.HasValue)
        {
            scores.Add(SQLExecutionSuccess.Value ? 1.0 : 0.0);
        }

        // User satisfaction (if provided)
        if (UserFeedbackRating.HasValue)
        {
            // Convert 1-5 scale to 0-1 scale
            scores.Add((UserFeedbackRating.Value - 1) / 4.0);
        }

        // Confidence score
        scores.Add((double)ConfidenceScore);

        return scores.Average();
    }

    /// <summary>
    /// Update with SQL execution results
    /// </summary>
    public void UpdateSQLExecution(bool success, string? errorMessage = null, int? executionTimeMs = null)
    {
        SQLExecutionSuccess = success;
        SQLExecutionError = errorMessage;
        
        if (executionTimeMs.HasValue)
        {
            ProcessingTimeMs += executionTimeMs.Value;
        }
        
        UpdatedDate = DateTime.UtcNow;
    }

    /// <summary>
    /// Update with user feedback
    /// </summary>
    public void UpdateUserFeedback(int rating, string? comments = null)
    {
        if (rating >= 1 && rating <= 5)
        {
            UserFeedbackRating = rating;
        }
        
        if (!string.IsNullOrEmpty(comments))
        {
            UserFeedbackComments = comments;
        }
        
        UpdatedDate = DateTime.UtcNow;
    }

    /// <summary>
    /// Check if this session represents a successful end-to-end operation
    /// </summary>
    public bool IsSuccessfulSession()
    {
        return !string.IsNullOrEmpty(GeneratedSQL) && 
               SQLExecutionSuccess == true && 
               (UserFeedbackRating == null || UserFeedbackRating >= 3);
    }

    /// <summary>
    /// Get performance category based on processing time
    /// </summary>
    public string GetPerformanceCategory()
    {
        return ProcessingTimeMs switch
        {
            < 1000 => "Fast",
            < 3000 => "Normal",
            < 10000 => "Slow",
            _ => "Very Slow"
        };
    }
}
