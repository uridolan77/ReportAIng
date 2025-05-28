namespace BIReportingCopilot.Core.Models;

/// <summary>
/// User activity tracking and analytics
/// Note: UserProfile has been consolidated into the main User class
/// </summary>
public class UserActivitySummary
{
    public int TotalQueries { get; set; }
    public int QueriesThisWeek { get; set; }
    public int QueriesThisMonth { get; set; }
    public double AverageQueryTime { get; set; }
    public DateTime LastActivity { get; set; }
    public List<DailyActivity> DailyActivities { get; set; } = new();
    public Dictionary<string, int> QueryTypeBreakdown { get; set; } = new();
}

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
