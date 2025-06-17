using System.ComponentModel.DataAnnotations;

namespace BIReportingCopilot.Core.DTOs;

/// <summary>
/// Query pattern DTO
/// </summary>
public class QueryPatternDto
{
    public long Id { get; set; }
    public string PatternId { get; set; } = string.Empty;
    public string PatternName { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string NaturalLanguagePattern { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Pattern { get; set; } = string.Empty;
    public string BusinessContext { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public List<string> Keywords { get; set; } = new();
    public List<string> RequiredTables { get; set; } = new();
    public string SqlTemplate { get; set; } = string.Empty;
    public int Priority { get; set; } = 1;
    public bool IsActive { get; set; } = true;
    public int UsageCount { get; set; }
    public DateTime? LastUsedDate { get; set; }
    public DateTime? LastUsed { get; set; }
    public DateTime CreatedDate { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? UpdatedDate { get; set; }
    public string? UpdatedBy { get; set; }
}

/// <summary>
/// Create query pattern request DTO
/// </summary>
public class CreateQueryPatternRequest
{
    [Required]
    [StringLength(100)]
    public string PatternName { get; set; } = string.Empty;

    [Required]
    [StringLength(500)]
    public string NaturalLanguagePattern { get; set; } = string.Empty;

    [Required]
    [StringLength(2000)]
    public string SqlTemplate { get; set; } = string.Empty;

    [StringLength(500)]
    public string Description { get; set; } = string.Empty;

    [StringLength(1000)]
    public string BusinessContext { get; set; } = string.Empty;

    public List<string> Keywords { get; set; } = new();

    public List<string> RequiredTables { get; set; } = new();

    [Range(1, 10)]
    public int Priority { get; set; } = 1;

    public bool IsActive { get; set; } = true;
}

/// <summary>
/// Update query pattern request DTO
/// </summary>
public class UpdateQueryPatternRequest
{
    [StringLength(200)]
    public string? PatternName { get; set; }

    [StringLength(500)]
    public string? NaturalLanguagePattern { get; set; }

    [StringLength(4000)]
    public string? SqlTemplate { get; set; }

    [StringLength(1000)]
    public string? Description { get; set; }

    [StringLength(1000)]
    public string? BusinessContext { get; set; }

    public List<string>? Keywords { get; set; }
    public List<string>? RequiredTables { get; set; }
    
    [Range(1, 10)]
    public int? Priority { get; set; }
    
    public bool? IsActive { get; set; }
}

/// <summary>
/// Query pattern match result DTO
/// </summary>
public class QueryPatternMatchDto
{
    public QueryPatternDto Pattern { get; set; } = new();
    public double MatchScore { get; set; }
    public List<string> MatchedKeywords { get; set; } = new();
    public string SuggestedSql { get; set; } = string.Empty;
    public Dictionary<string, string> ParameterMappings { get; set; } = new();
}

/// <summary>
/// Query pattern analysis result DTO
/// </summary>
public class QueryPatternAnalysisDto
{
    public string InputQuery { get; set; } = string.Empty;
    public List<QueryPatternMatchDto> Matches { get; set; } = new();
    public QueryPatternMatchDto? BestMatch { get; set; }
    public double BestMatchScore { get; set; }
    public List<string> ExtractedKeywords { get; set; } = new();
    public List<string> IdentifiedTables { get; set; } = new();
    public List<string> Suggestions { get; set; } = new();
    public TimeSpan ProcessingTime { get; set; }
}

/// <summary>
/// Query pattern statistics DTO
/// </summary>
public class QueryPatternStatisticsDto
{
    public int TotalPatterns { get; set; }
    public int ActivePatterns { get; set; }
    public int InactivePatterns { get; set; }
    public QueryPatternDto? MostUsedPattern { get; set; }
    public QueryPatternDto? HighestPriorityPattern { get; set; }
    public List<QueryPatternUsageDto> RecentUsage { get; set; } = new();
    public Dictionary<string, int> CategoryDistribution { get; set; } = new();
    public Dictionary<string, int> KeywordFrequency { get; set; } = new();
    public DateTime? LastPatternCreated { get; set; }
    public string? LastPatternCreatedBy { get; set; }
}

/// <summary>
/// Query pattern usage DTO
/// </summary>
public class QueryPatternUsageDto
{
    public long PatternId { get; set; }
    public string PatternName { get; set; } = string.Empty;
    public int UsageCount { get; set; }
    public DateTime LastUsed { get; set; }
    public string LastUsedBy { get; set; } = string.Empty;
    public double AverageMatchScore { get; set; }
    public List<string> CommonVariations { get; set; } = new();
}
