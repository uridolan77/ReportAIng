using BIReportingCopilot.Core.Interfaces.Analytics;

namespace BIReportingCopilot.Core.Models.Analytics;

/// <summary>
/// Comprehensive analytics dashboard combining all analytics data
/// </summary>
public class ComprehensiveAnalyticsDashboard
{
    public PerformanceDashboardData PerformanceOverview { get; set; } = new();
    public ABTestDashboard ABTestingOverview { get; set; } = new();
    public TemplateManagementDashboard ManagementOverview { get; set; } = new();
    public PerformanceTrendsData PerformanceTrends { get; set; } = new();
    public UsageInsightsData UsageInsights { get; set; } = new();
    public QualityMetricsData QualityMetrics { get; set; } = new();
    public List<PerformanceAlert> ActiveAlerts { get; set; } = new();
    public DateRange DateRange { get; set; } = new();
    public DateTime GeneratedDate { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Performance trends data over time
/// </summary>
public class PerformanceTrendsData
{
    public List<PerformanceTrendDataPoint> DataPoints { get; set; } = new();
    public DateRange TimeRange { get; set; } = new();
    public string Granularity { get; set; } = "daily";
    public string? IntentType { get; set; }
    public DateTime GeneratedDate { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Performance trend data point
/// </summary>
public class PerformanceTrendDataPoint
{
    public DateTime Timestamp { get; set; }
    public decimal AverageSuccessRate { get; set; }
    public decimal AverageConfidenceScore { get; set; }
    public int TotalUsage { get; set; }
    public int ActiveTemplates { get; set; }
    public decimal AverageResponseTime { get; set; }
    public int ErrorCount { get; set; }
}

/// <summary>
/// Usage insights and analytics data
/// </summary>
public class UsageInsightsData
{
    public int TotalUsage { get; set; }
    public decimal AverageSuccessRate { get; set; }
    public Dictionary<string, int> UsageByIntentType { get; set; } = new();
    public List<TemplatePerformanceMetrics> TopPerformingTemplates { get; set; } = new();
    public List<TemplatePerformanceMetrics> UnderperformingTemplates { get; set; } = new();
    public List<UsageInsight> Insights { get; set; } = new();
    public DateRange TimeRange { get; set; } = new();
    public DateTime GeneratedDate { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Usage insight with recommendations
/// </summary>
public class UsageInsight
{
    public string Type { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Impact { get; set; } = string.Empty; // High, Medium, Low
    public string Recommendation { get; set; } = string.Empty;
    public Dictionary<string, object> Data { get; set; } = new();
    public DateTime GeneratedDate { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Quality metrics and analysis data
/// </summary>
public class QualityMetricsData
{
    public decimal OverallQualityScore { get; set; }
    public decimal AverageConfidenceScore { get; set; }
    public Dictionary<string, int> QualityDistribution { get; set; } = new();
    public int TotalTemplatesAnalyzed { get; set; }
    public int TemplatesAboveThreshold { get; set; }
    public int TemplatesBelowThreshold { get; set; }
    public List<QualityMetric> DetailedMetrics { get; set; } = new();
    public string? IntentType { get; set; }
    public DateTime GeneratedDate { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Individual quality metric
/// </summary>
public class QualityMetric
{
    public string MetricName { get; set; } = string.Empty;
    public decimal Value { get; set; }
    public decimal Threshold { get; set; }
    public bool IsAboveThreshold { get; set; }
    public string Unit { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

/// <summary>
/// Date range for analytics queries
/// </summary>
public class DateRange
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    
    public TimeSpan Duration => EndDate - StartDate;
    public int DaysInRange => (int)Duration.TotalDays;
}

/// <summary>
/// Performance alert for monitoring
/// </summary>
public class PerformanceAlert
{
    public string AlertId { get; set; } = Guid.NewGuid().ToString();
    public string AlertType { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public AlertSeverity Severity { get; set; }
    public string TemplateKey { get; set; } = string.Empty;
    public string IntentType { get; set; } = string.Empty;
    public Dictionary<string, object> AlertData { get; set; } = new();
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public DateTime? ResolvedDate { get; set; }
    public bool IsResolved { get; set; }
    public string? ResolvedBy { get; set; }
    public string? ResolutionNotes { get; set; }
}

/// <summary>
/// Alert severity levels
/// </summary>
public enum AlertSeverity
{
    Low,
    Medium,
    High,
    Critical
}

/// <summary>
/// Real-time analytics data for live monitoring
/// </summary>
public class RealTimeAnalyticsData
{
    public int ActiveUsers { get; set; }
    public int QueriesPerMinute { get; set; }
    public decimal CurrentSuccessRate { get; set; }
    public decimal AverageResponseTime { get; set; }
    public int ErrorsInLastHour { get; set; }
    public List<RecentActivity> RecentActivities { get; set; } = new();
    public Dictionary<string, int> ActiveTemplateUsage { get; set; } = new();
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Recent activity item
/// </summary>
public class RecentActivity
{
    public string ActivityType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string TemplateKey { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public bool WasSuccessful { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Analytics export configuration
/// </summary>
public class AnalyticsExportConfig
{
    public ExportFormat Format { get; set; } = ExportFormat.CSV;
    public DateRange DateRange { get; set; } = new();
    public List<string> IncludedMetrics { get; set; } = new();
    public string? IntentTypeFilter { get; set; }
    public bool IncludeCharts { get; set; } = false;
    public bool IncludeRawData { get; set; } = true;
    public string ExportedBy { get; set; } = string.Empty;
    public DateTime RequestedDate { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Analytics export result
/// </summary>
public class AnalyticsExportResult
{
    public string ExportId { get; set; } = Guid.NewGuid().ToString();
    public byte[] Data { get; set; } = Array.Empty<byte>();
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public AnalyticsExportConfig Configuration { get; set; } = new();
    public DateTime GeneratedDate { get; set; } = DateTime.UtcNow;
    public TimeSpan GenerationTime { get; set; }
}
