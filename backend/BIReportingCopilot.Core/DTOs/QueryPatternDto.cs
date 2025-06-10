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
