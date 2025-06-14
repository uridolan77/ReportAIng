using System.ComponentModel.DataAnnotations;

namespace BIReportingCopilot.Core.Models;

// UserInfo already exists in other files - removed duplicate

// UserSession, UserActivity, and User already exist in other files - removed duplicates

// AuditLogEntry, SecurityReport, and UsageReport already exist in other files - removed duplicates

/// <summary>
/// Security finding model
/// </summary>
public class SecurityFinding
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public SecuritySeverity Severity { get; set; }
    public string Category { get; set; } = string.Empty;
    public DateTime DetectedAt { get; set; } = DateTime.UtcNow;
    public string? UserId { get; set; }
    public string? Resource { get; set; }
    public Dictionary<string, object> Details { get; set; } = new();
}

/// <summary>
/// Usage statistic model
/// </summary>
public class UsageStatistic
{
    public string Metric { get; set; } = string.Empty;
    public double Value { get; set; }
    public string Unit { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public Dictionary<string, object> Dimensions { get; set; } = new();
}

/// <summary>
/// Security report type enumeration
/// </summary>
public enum SecurityReportType
{
    LoginAttempts,
    AccessViolations,
    DataAccess,
    SystemChanges,
    UserActivity
}

/// <summary>
/// Security severity enumeration
/// </summary>
public enum SecuritySeverity
{
    Low,
    Medium,
    High,
    Critical
}
