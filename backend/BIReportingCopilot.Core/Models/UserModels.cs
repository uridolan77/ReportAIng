namespace BIReportingCopilot.Core.Models;

// UserActivitySummary moved to RealTimeStreaming.cs to avoid duplicates

public class DailyActivity
{
    public DateTime Date { get; set; }
    public int QueryCount { get; set; }
    public double AverageResponseTime { get; set; }
}

/// <summary>
/// User permissions and access control
/// </summary>
public class UserPermissions
{
    public List<string> Roles { get; set; } = new();
    public List<string> Permissions { get; set; } = new();
    public Dictionary<string, bool> FeatureAccess { get; set; } = new();
    public List<string> AllowedDatabases { get; set; } = new();
}
