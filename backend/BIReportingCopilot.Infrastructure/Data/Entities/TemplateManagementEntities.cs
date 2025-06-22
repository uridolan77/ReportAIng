using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BIReportingCopilot.Infrastructure.Data.Entities;

/// <summary>
/// Entity for tracking template performance metrics
/// </summary>
public class TemplatePerformanceMetricsEntity : BaseEntity
{
    [Key]
    public long Id { get; set; }
    
    [Required]
    public long TemplateId { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string TemplateKey { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(50)]
    public string IntentType { get; set; } = string.Empty;
    
    public int TotalUsages { get; set; } = 0;
    public int SuccessfulUsages { get; set; } = 0;
    
    [Column(TypeName = "decimal(5,4)")]
    public decimal SuccessRate { get; set; } = 0;
    
    [Column(TypeName = "decimal(3,2)")]
    public decimal AverageConfidenceScore { get; set; } = 0;
    
    public int AverageProcessingTimeMs { get; set; } = 0;
    
    [Column(TypeName = "decimal(3,2)")]
    public decimal? AverageUserRating { get; set; }
    
    public DateTime? LastUsedDate { get; set; }
    
    // Navigation property
    [ForeignKey(nameof(TemplateId))]
    public virtual PromptTemplateEntity Template { get; set; } = null!;
}

/// <summary>
/// Entity for A/B testing template variations
/// </summary>
public class TemplateABTestEntity : BaseEntity
{
    [Key]
    public long Id { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string TestName { get; set; } = string.Empty;
    
    [Required]
    public long OriginalTemplateId { get; set; }
    
    [Required]
    public long VariantTemplateId { get; set; }
    
    [Range(1, 99)]
    public int TrafficSplitPercent { get; set; } = 50;
    
    [Required]
    public DateTime StartDate { get; set; }
    
    public DateTime? EndDate { get; set; }
    
    [Required]
    [MaxLength(20)]
    public string Status { get; set; } = "active"; // active, paused, completed, cancelled
    
    [Column(TypeName = "decimal(5,4)")]
    public decimal? OriginalSuccessRate { get; set; }
    
    [Column(TypeName = "decimal(5,4)")]
    public decimal? VariantSuccessRate { get; set; }
    
    [Column(TypeName = "decimal(5,4)")]
    public decimal? StatisticalSignificance { get; set; }
    
    public long? WinnerTemplateId { get; set; }
    
    [Column(TypeName = "nvarchar(max)")]
    public string? TestResults { get; set; } // JSON object with detailed results
    
    public DateTime? CompletedDate { get; set; }
    
    // Navigation properties
    [ForeignKey(nameof(OriginalTemplateId))]
    public virtual PromptTemplateEntity OriginalTemplate { get; set; } = null!;
    
    [ForeignKey(nameof(VariantTemplateId))]
    public virtual PromptTemplateEntity VariantTemplate { get; set; } = null!;
    
    [ForeignKey(nameof(WinnerTemplateId))]
    public virtual PromptTemplateEntity? WinnerTemplate { get; set; }
}

/// <summary>
/// Entity for storing template improvement suggestions
/// </summary>
public class TemplateImprovementSuggestionEntity : BaseEntity
{
    [Key]
    public long Id { get; set; }
    
    [Required]
    public long TemplateId { get; set; }
    
    [Required]
    [MaxLength(50)]
    public string SuggestionType { get; set; } = string.Empty; // performance, accuracy, user_feedback, content_quality
    
    [Required]
    [MaxLength(20)]
    public string CurrentVersion { get; set; } = string.Empty;
    
    [Required]
    [Column(TypeName = "nvarchar(max)")]
    public string SuggestedChanges { get; set; } = string.Empty; // JSON object with suggested modifications
    
    [Required]
    [Column(TypeName = "nvarchar(max)")]
    public string ReasoningExplanation { get; set; } = string.Empty;
    
    [Column(TypeName = "decimal(5,2)")]
    public decimal? ExpectedImprovementPercent { get; set; }
    
    public int BasedOnDataPoints { get; set; } = 0;
    
    [Required]
    [MaxLength(20)]
    public string Status { get; set; } = "pending"; // pending, approved, rejected, implemented
    
    [MaxLength(512)]
    public string? ReviewedBy { get; set; }
    
    public DateTime? ReviewedDate { get; set; }
    
    [MaxLength(1000)]
    public string? ReviewComments { get; set; }
    
    // Navigation property
    [ForeignKey(nameof(TemplateId))]
    public virtual PromptTemplateEntity Template { get; set; } = null!;
}
