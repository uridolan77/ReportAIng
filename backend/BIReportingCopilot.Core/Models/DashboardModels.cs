namespace BIReportingCopilot.Core.Models;

public class DashboardOverview
{
    public UserActivitySummary UserActivity { get; set; } = new();
    public List<QueryHistoryItem> RecentQueries { get; set; } = new();
    public SystemMetrics SystemMetrics { get; set; } = new();
    public QuickStats QuickStats { get; set; } = new();
}

public class SystemMetrics
{
    public int DatabaseConnections { get; set; }
    public decimal CacheHitRate { get; set; }
    public double AverageQueryTime { get; set; }
    public TimeSpan SystemUptime { get; set; }
}

public class QuickStats
{
    public int TotalQueries { get; set; }
    public int QueriesThisWeek { get; set; }
    public double AverageQueryTime { get; set; }
    public string FavoriteTable { get; set; } = string.Empty;
}

public class UsageAnalytics
{
    public QueryTrends QueryTrends { get; set; } = new();
    public List<PopularTable> PopularTables { get; set; } = new();
    public PerformanceMetrics PerformanceMetrics { get; set; } = new();
    public ErrorAnalysis ErrorAnalysis { get; set; } = new();
}

public class QueryTrends
{
    public Dictionary<DateTime, int> DailyQueryCounts { get; set; } = new();
    public Dictionary<string, int> QueryTypeDistribution { get; set; } = new();
    public List<int> PeakUsageHours { get; set; } = new();
}

public class PopularTable
{
    public string TableName { get; set; } = string.Empty;
    public int QueryCount { get; set; }
    public double AverageResponseTime { get; set; }
    public DateTime LastAccessed { get; set; }
}

// PerformanceMetrics moved to PerformanceModels.cs to avoid duplicates

public class ErrorAnalysis
{
    public int TotalErrors { get; set; }
    public Dictionary<string, int> ErrorsByType { get; set; } = new();
    public List<string> CommonErrorMessages { get; set; } = new();
}

public class SystemStatistics
{
    public int TotalUsers { get; set; }
    public int TotalQueries { get; set; }
    public double AverageResponseTime { get; set; }
    public double ErrorRate { get; set; }
    public List<TopUser> TopUsers { get; set; } = new();
    public ResourceUsage ResourceUsage { get; set; } = new();
}

public class TopUser
{
    public string UserId { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public int QueryCount { get; set; }
    public double AverageResponseTime { get; set; }
}

public class ResourceUsage
{
    public double CpuUsage { get; set; }
    public double MemoryUsage { get; set; }
    public double DiskUsage { get; set; }
    public double NetworkUsage { get; set; }
}

public class ActivityItem
{
    public string Id { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string Severity { get; set; } = "Info";
    public Dictionary<string, object> Metadata { get; set; } = new();
}

public class Recommendation
{
    public string Type { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Priority { get; set; } = "low";
    public string? ActionUrl { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
