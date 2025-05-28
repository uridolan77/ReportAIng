namespace BIReportingCopilot.Core.Models;

public class UserProfile
{
    public string Id { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string? ProfilePictureUrl { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime LastLoginDate { get; set; }
    public bool IsActive { get; set; } = true;
    public List<string> Roles { get; set; } = new();
}

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

public class UserPermissions
{
    public List<string> Roles { get; set; } = new();
    public List<string> Permissions { get; set; } = new();
    public Dictionary<string, bool> FeatureAccess { get; set; } = new();
    public List<string> AllowedDatabases { get; set; } = new();
}

// UserSession is already defined in User.cs
