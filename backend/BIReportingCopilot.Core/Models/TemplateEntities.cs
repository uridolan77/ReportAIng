using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BIReportingCopilot.Core.Models;

/// <summary>
/// Core model for prompt template (matches Infrastructure entity structure)
/// </summary>
public class PromptTemplateEntity
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public decimal? SuccessRate { get; set; }
    public int UsageCount { get; set; }
    public string? Parameters { get; set; }
    public string TemplateKey { get; set; } = string.Empty;
    public string IntentType { get; set; } = string.Empty;
    public int Priority { get; set; } = 1;
    public string? Tags { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedDate { get; set; }
    // Match actual database column names
    public DateTime? LastUsedDate { get; set; } // NEW: Track when template was last used
    public DateTime? LastBusinessReviewDate { get; set; } // Maps to LastBusinessReviewDate column
    public string? BusinessPurpose { get; set; }
    public string? RelatedBusinessTerms { get; set; }
    public string? BusinessFriendlyName { get; set; }
    public string? NaturalLanguageDescription { get; set; }
    public string? BusinessRules { get; set; }
    public string? RelationshipContext { get; set; }
    public string? DataGovernanceLevel { get; set; }
    public decimal? ImportanceScore { get; set; } = 0.5m; // Make nullable to match database
    public decimal? UsageFrequency { get; set; } // Change to decimal to match database
    public string? BusinessMetadata { get; set; } // JSON metadata
}

/// <summary>
/// Core model for template performance metrics
/// </summary>
public class TemplatePerformanceMetricsEntity
{
    public long Id { get; set; }
    public long TemplateId { get; set; }
    public string TemplateKey { get; set; } = string.Empty;
    public string IntentType { get; set; } = string.Empty;
    public int TotalUsages { get; set; } = 0;
    public int SuccessfulUsages { get; set; } = 0;
    public decimal SuccessRate { get; set; } = 0;
    public decimal AverageConfidenceScore { get; set; } = 0;
    public int AverageProcessingTimeMs { get; set; } = 0;
    public decimal? AverageUserRating { get; set; }
    public DateTime? LastUsedDate { get; set; }
    public DateTime AnalysisDate { get; set; } = DateTime.UtcNow;
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedDate { get; set; } = DateTime.UtcNow;
    public Dictionary<string, object>? AdditionalMetrics { get; set; } // JSON
    
    // Navigation property
    public virtual PromptTemplateEntity? Template { get; set; }
}

/// <summary>
/// Core model for A/B testing template variations
/// </summary>
public class TemplateABTestEntity
{
    public long Id { get; set; }
    public string TestName { get; set; } = string.Empty;
    public long OriginalTemplateId { get; set; }
    public long VariantTemplateId { get; set; }
    public int TrafficSplitPercent { get; set; } = 50;
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string Status { get; set; } = "active"; // active, paused, completed, cancelled
    public decimal? OriginalSuccessRate { get; set; }
    public decimal? VariantSuccessRate { get; set; }
    public decimal? StatisticalSignificance { get; set; }
    public long? WinnerTemplateId { get; set; }
    public string? TestResults { get; set; } // JSON object with detailed results
    public DateTime? CompletedDate { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedDate { get; set; }

    // Additional properties for analytics
    public decimal? ConfidenceLevel { get; set; }
    public string? Conclusion { get; set; }
    
    // Navigation properties
    public virtual PromptTemplateEntity? OriginalTemplate { get; set; }
    public virtual PromptTemplateEntity? VariantTemplate { get; set; }
    public virtual PromptTemplateEntity? WinnerTemplate { get; set; }
}

/// <summary>
/// Core model for template improvement suggestions
/// </summary>
public class TemplateImprovementSuggestionEntity
{
    public long Id { get; set; }
    public long TemplateId { get; set; }
    public string SuggestionType { get; set; } = string.Empty; // performance, accuracy, user_feedback, content_quality
    public string CurrentVersion { get; set; } = string.Empty;
    public string SuggestedChanges { get; set; } = string.Empty; // JSON object with suggested modifications
    public string ReasoningExplanation { get; set; } = string.Empty;
    public decimal? ExpectedImprovementPercent { get; set; }
    public int BasedOnDataPoints { get; set; } = 0;
    public string Status { get; set; } = "pending"; // pending, approved, rejected, implemented
    public string? ReviewedBy { get; set; }
    public DateTime? ReviewedDate { get; set; }
    public string? ReviewComments { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedDate { get; set; }

    // Additional properties for template analytics
    public string TemplateKey { get; set; } = string.Empty;
    public string TemplateName { get; set; } = string.Empty;
    public string ImprovementType { get; set; } = string.Empty;
    public decimal? ConfidenceScore { get; set; }
    public string? ReviewNotes { get; set; }
    public DateTime? ImplementedDate { get; set; }

    // Additional properties for mapping compatibility
    public string? Title { get; set; }
    public string? Description { get; set; }
    public int Priority { get; set; } = 1;

    // Navigation property
    public virtual PromptTemplateEntity? Template { get; set; }
}

/// <summary>
/// Search criteria for template queries
/// </summary>
public class TemplateSearchCriteria
{
    public string? SearchTerm { get; set; }
    public string? IntentType { get; set; }
    public bool? IsActive { get; set; }
    public List<string>? Tags { get; set; }
    public int? MinPriority { get; set; }
    public int? MaxPriority { get; set; }
    public DateTime? CreatedAfter { get; set; }
    public DateTime? CreatedBefore { get; set; }
    public string? CreatedBy { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? SortBy { get; set; }
    public bool SortDescending { get; set; } = false;
}
