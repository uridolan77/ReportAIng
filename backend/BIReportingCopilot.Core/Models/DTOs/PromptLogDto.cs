namespace BIReportingCopilot.Core.Models.DTOs;

public class PromptLogDto
{
    public long Id { get; set; }
    public string PromptType { get; set; } = string.Empty;
    public string UserQuery { get; set; } = string.Empty;
    public string FullPrompt { get; set; } = string.Empty;
    public string? GeneratedSQL { get; set; }
    public bool? Success { get; set; }
    public string? ErrorMessage { get; set; }
    public int PromptLength { get; set; }
    public int? ResponseLength { get; set; }
    public int? ExecutionTimeMs { get; set; }
    public DateTime CreatedDate { get; set; }
    public string? UserId { get; set; }
    public string? SessionId { get; set; }
    public string? Metadata { get; set; }
}
