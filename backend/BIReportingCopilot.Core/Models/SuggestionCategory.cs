using System.ComponentModel.DataAnnotations;

namespace BIReportingCopilot.Core.Models;

/// <summary>
/// Represents a category for organizing query suggestions
/// </summary>
public class SuggestionCategory
{
    public long Id { get; set; }

    [Required]
    [StringLength(50)]
    public string CategoryKey { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string Title { get; set; } = string.Empty;

    [StringLength(10)]
    public string? Icon { get; set; }

    [StringLength(200)]
    public string? Description { get; set; }

    public int SortOrder { get; set; } = 0;

    public bool IsActive { get; set; } = true;

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    [Required]
    [StringLength(256)]
    public string CreatedBy { get; set; } = string.Empty;

    public DateTime? UpdatedDate { get; set; }

    [StringLength(256)]
    public string? UpdatedBy { get; set; }

    // Navigation properties
    public virtual ICollection<QuerySuggestion> Suggestions { get; set; } = new List<QuerySuggestion>();
}

/// <summary>
/// Represents a query suggestion with metadata and analytics
/// </summary>
public class QuerySuggestion
{
    public long Id { get; set; }

    public long CategoryId { get; set; }

    [Required]
    [StringLength(500)]
    public string QueryText { get; set; } = string.Empty;

    [Required]
    [StringLength(200)]
    public string Description { get; set; } = string.Empty;

    [StringLength(50)]
    public string? DefaultTimeFrame { get; set; }

    public int SortOrder { get; set; } = 0;

    public bool IsActive { get; set; } = true;

    [StringLength(500)]
    public string? TargetTables { get; set; } // JSON array

    public byte Complexity { get; set; } = 1; // 1=Simple, 2=Medium, 3=Complex

    [StringLength(200)]
    public string? RequiredPermissions { get; set; } // JSON array

    [StringLength(300)]
    public string? Tags { get; set; } // JSON array

    public int UsageCount { get; set; } = 0;

    public DateTime? LastUsed { get; set; }

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    [Required]
    [StringLength(256)]
    public string CreatedBy { get; set; } = string.Empty;

    public DateTime? UpdatedDate { get; set; }

    [StringLength(256)]
    public string? UpdatedBy { get; set; }

    // Navigation properties
    public virtual SuggestionCategory Category { get; set; } = null!;
    public virtual ICollection<SuggestionUsageAnalytics> UsageAnalytics { get; set; } = new List<SuggestionUsageAnalytics>();
}

/// <summary>
/// Tracks usage analytics for query suggestions
/// </summary>
public class SuggestionUsageAnalytics
{
    public long Id { get; set; }

    public long SuggestionId { get; set; }

    [Required]
    [StringLength(256)]
    public string UserId { get; set; } = string.Empty;

    [StringLength(50)]
    public string? SessionId { get; set; }

    public DateTime UsedAt { get; set; } = DateTime.UtcNow;

    [StringLength(50)]
    public string? TimeFrameUsed { get; set; }

    public int? ResultCount { get; set; }

    public int? ExecutionTimeMs { get; set; }

    public bool WasSuccessful { get; set; } = true;

    public byte? UserFeedback { get; set; } // 1=Helpful, 0=Not helpful, NULL=No feedback

    // Navigation properties
    public virtual QuerySuggestion Suggestion { get; set; } = null!;
}

/// <summary>
/// Defines available time frames for queries
/// </summary>
public class TimeFrameDefinition
{
    public long Id { get; set; }

    [Required]
    [StringLength(50)]
    public string TimeFrameKey { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string DisplayName { get; set; } = string.Empty;

    [StringLength(200)]
    public string? Description { get; set; }

    [Required]
    [StringLength(500)]
    public string SqlExpression { get; set; } = string.Empty;

    public int SortOrder { get; set; } = 0;

    public bool IsActive { get; set; } = true;

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
}
