using System.ComponentModel.DataAnnotations;

namespace BIReportingCopilot.Core.Models.DTOs;

public class PromptTemplateDto
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
    public DateTime? UpdatedDate { get; set; }
    public decimal? SuccessRate { get; set; }
    public int UsageCount { get; set; }
    public Dictionary<string, object> Parameters { get; set; } = new();
}

public class CreatePromptTemplateRequest
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(20)]
    public string Version { get; set; } = string.Empty;

    [Required]
    public string Content { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;

    public Dictionary<string, object>? Parameters { get; set; }
}

public class PromptTemplateTestRequest
{
    [Required]
    public string Question { get; set; } = string.Empty;

    public string? Schema { get; set; }

    public string? Context { get; set; }

    public Dictionary<string, object>? AdditionalParameters { get; set; }
}

public class PromptTemplateTestResult
{
    public string ProcessedPrompt { get; set; } = string.Empty;
    public string TemplateName { get; set; } = string.Empty;
    public string TemplateVersion { get; set; } = string.Empty;
    public Dictionary<string, string> ReplacedVariables { get; set; } = new();
    public int TokenCount { get; set; }
    public TimeSpan ProcessingTime { get; set; }
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime TestedAt { get; set; } = DateTime.UtcNow;
}
