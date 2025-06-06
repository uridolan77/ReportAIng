using System.ComponentModel.DataAnnotations;

namespace BIReportingCopilot.Core.DTOs.QuerySuggestions;

/// <summary>
/// DTO for suggestion category information
/// </summary>
public class SuggestionCategoryDto
{
    public long Id { get; set; }
    public string CategoryKey { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Icon { get; set; }
    public string? Description { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; }
    public int SuggestionCount { get; set; }
}

/// <summary>
/// DTO for query suggestion information
/// </summary>
public class QuerySuggestionDto
{
    public long Id { get; set; }
    public long CategoryId { get; set; }
    public string CategoryKey { get; set; } = string.Empty;
    public string CategoryTitle { get; set; } = string.Empty;
    public string QueryText { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? DefaultTimeFrame { get; set; }
    public string? DefaultTimeFrameDisplay { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; }
    public List<string> TargetTables { get; set; } = new();
    public byte Complexity { get; set; }
    public List<string> RequiredPermissions { get; set; } = new();
    public List<string> Tags { get; set; } = new();
    public int UsageCount { get; set; }
    public DateTime? LastUsed { get; set; }
    public DateTime CreatedDate { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
}

/// <summary>
/// DTO for grouped suggestions by category
/// </summary>
public class GroupedSuggestionsDto
{
    public SuggestionCategoryDto Category { get; set; } = new();
    public List<QuerySuggestionDto> Suggestions { get; set; } = new();
}

/// <summary>
/// DTO for creating/updating suggestion categories
/// </summary>
public class CreateUpdateSuggestionCategoryDto
{
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
}

/// <summary>
/// DTO for creating/updating query suggestions
/// </summary>
public class CreateUpdateQuerySuggestionDto
{
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

    public List<string> TargetTables { get; set; } = new();

    [Range(1, 3)]
    public byte Complexity { get; set; } = 1;

    public List<string> RequiredPermissions { get; set; } = new();

    public List<string> Tags { get; set; } = new();
}

/// <summary>
/// DTO for time frame definitions
/// </summary>
public class TimeFrameDefinitionDto
{
    public long Id { get; set; }
    public string TimeFrameKey { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string SqlExpression { get; set; } = string.Empty;
    public int SortOrder { get; set; }
    public bool IsActive { get; set; }
}

/// <summary>
/// DTO for recording suggestion usage
/// </summary>
public class RecordSuggestionUsageDto
{
    [Required]
    public long SuggestionId { get; set; }

    [StringLength(50)]
    public string? SessionId { get; set; }

    [StringLength(50)]
    public string? TimeFrameUsed { get; set; }

    public int? ResultCount { get; set; }

    public int? ExecutionTimeMs { get; set; }

    public bool WasSuccessful { get; set; } = true;

    [Range(0, 1)]
    public byte? UserFeedback { get; set; }
}

/// <summary>
/// DTO for suggestion usage analytics
/// </summary>
public class SuggestionUsageAnalyticsDto
{
    public long SuggestionId { get; set; }
    public string QueryText { get; set; } = string.Empty;
    public string CategoryTitle { get; set; } = string.Empty;
    public int TotalUsage { get; set; }
    public DateTime? LastUsed { get; set; }
    public double AverageExecutionTime { get; set; }
    public double SuccessRate { get; set; }
    public double? AverageUserFeedback { get; set; }
    public List<string> PopularTimeFrames { get; set; } = new();
}

/// <summary>
/// DTO for suggestion search and filtering
/// </summary>
public class SuggestionSearchDto
{
    public string? SearchTerm { get; set; }
    public string? CategoryKey { get; set; }
    public List<string> Tags { get; set; } = new();
    public byte? MinComplexity { get; set; }
    public byte? MaxComplexity { get; set; }
    public bool? IsActive { get; set; } = true;
    public int Skip { get; set; } = 0;
    public int Take { get; set; } = 50;
    public string SortBy { get; set; } = "SortOrder";
    public bool SortDescending { get; set; } = false;
}

/// <summary>
/// DTO for suggestion search results
/// </summary>
public class SuggestionSearchResultDto
{
    public List<QuerySuggestionDto> Suggestions { get; set; } = new();
    public int TotalCount { get; set; }
    public int Skip { get; set; }
    public int Take { get; set; }
}
