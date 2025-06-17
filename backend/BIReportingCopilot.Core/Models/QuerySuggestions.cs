using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace BIReportingCopilot.Core.Models.QuerySuggestions;

/// <summary>
/// Represents a category for organizing query suggestions
/// </summary>
public class SuggestionCategory
{
    public long Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;

    public int DisplayOrder { get; set; } = 0;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [StringLength(256)]
    public string? CreatedBy { get; set; }

    [StringLength(256)]
    public string? UpdatedBy { get; set; }

    // Navigation properties
    public virtual ICollection<QuerySuggestion> Suggestions { get; set; } = new List<QuerySuggestion>();

    // Compatibility properties for existing code
    public string CategoryKey
    {
        get => Name.Replace(" ", "-").ToLowerInvariant();
        set => Name = value.Replace("-", " ").ToTitleCase();
    }
    public string Title
    {
        get => Name;
        set => Name = value;
    }
    public string? Icon { get; set; } // Compatibility property, not stored in database
    public int SortOrder
    {
        get => DisplayOrder;
        set => DisplayOrder = value;
    }
    public DateTime CreatedDate
    {
        get => CreatedAt;
        set => CreatedAt = value;
    }
    public DateTime? UpdatedDate
    {
        get => UpdatedAt;
        set => UpdatedAt = value ?? DateTime.UtcNow;
    }
}

// Extension method for string manipulation
public static class StringExtensions
{
    public static string ToTitleCase(this string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(input.ToLower());
    }
}

/// <summary>
/// Represents a query suggestion with metadata and analytics
/// </summary>
public class QuerySuggestion
{
    public long Id { get; set; }

    public long CategoryId { get; set; }

    private string _queryText = string.Empty;

    [Required]
    [StringLength(500)]
    public string QueryText
    {
        get => _queryText;
        set
        {
            _queryText = value;
            Text = value; // Keep Text in sync
        }
    }

    [Required]
    [StringLength(500)]
    public string Text { get; set; } = string.Empty; // Mapped property - alias for QueryText to fix interface conflicts

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

    public decimal Relevance { get; set; } = 0.8m; // Added missing property for NLU service compatibility

    // Properties expected by Infrastructure services
    public string Query { get; set; } = string.Empty;
    public List<string> Keywords { get; set; } = new();
    public List<string> RequiredTables { get; set; } = new();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public decimal Confidence { get; set; } = 0.8m;
    public string Source { get; set; } = "manual";

    /// <summary>
    /// Error message (for compatibility)
    /// </summary>
    public string? ErrorMessage { get; set; }

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
