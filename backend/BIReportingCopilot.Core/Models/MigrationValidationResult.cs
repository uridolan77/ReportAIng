namespace BIReportingCopilot.Core.Models;

/// <summary>
/// Result of migration validation
/// </summary>
public class MigrationValidationResult
{
    public bool IsValid { get; set; }
    public List<string> Errors { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
    public List<string> ValidationSteps { get; set; } = new();
    public Dictionary<string, object> ValidationData { get; set; } = new();
    public DateTime ValidatedAt { get; set; } = DateTime.UtcNow;

    // Additional properties expected by Infrastructure layer
    public List<string> SuccessfulMigrations { get; set; } = new();
    public Dictionary<string, string> FailedMigrations { get; set; } = new();
}
