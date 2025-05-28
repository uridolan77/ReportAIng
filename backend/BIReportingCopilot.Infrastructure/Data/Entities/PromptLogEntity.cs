using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BIReportingCopilot.Infrastructure.Data.Entities;

[Table("PromptLogs")]
public class PromptLogEntity
{
    [Key]
    public long Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string PromptType { get; set; } = string.Empty;

    [Required]
    [MaxLength(2000)]
    public string UserQuery { get; set; } = string.Empty;

    [Required]
    public string FullPrompt { get; set; } = string.Empty;

    public string? Metadata { get; set; }

    public string? GeneratedSQL { get; set; }

    public bool? Success { get; set; }

    public string? ErrorMessage { get; set; }

    public int PromptLength { get; set; }

    public int? ResponseLength { get; set; }

    public int? ExecutionTimeMs { get; set; }

    public DateTime CreatedDate { get; set; }

    [MaxLength(256)]
    public string? UserId { get; set; }

    [MaxLength(256)]
    public string? SessionId { get; set; }
}
