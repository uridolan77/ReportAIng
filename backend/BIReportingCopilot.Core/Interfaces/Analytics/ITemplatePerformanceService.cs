using BIReportingCopilot.Core.Models.Analytics;

using BIReportingCopilot.Core.Models;

namespace BIReportingCopilot.Core.Interfaces.Analytics;

/// <summary>
/// Service for real-time template performance tracking and analytics
/// </summary>
public interface ITemplatePerformanceService
{
    /// <summary>
    /// Track template usage and update performance metrics
    /// </summary>
    Task TrackTemplateUsageAsync(string templateKey, bool wasSuccessful, decimal confidenceScore, int processingTimeMs, string? userId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update user rating for a template
    /// </summary>
    Task UpdateUserRatingAsync(string templateKey, decimal rating, string? userId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get performance metrics for a specific template
    /// </summary>
    Task<TemplatePerformanceMetrics?> GetTemplatePerformanceAsync(string templateKey, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get performance metrics for multiple templates
    /// </summary>
    Task<List<TemplatePerformanceMetrics>> GetTemplatePerformanceAsync(List<string> templateKeys, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get top performing templates by intent type
    /// </summary>
    Task<List<TemplatePerformanceMetrics>> GetTopPerformingTemplatesAsync(string? intentType = null, int count = 10, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get underperforming templates that need attention
    /// </summary>
    Task<List<TemplatePerformanceMetrics>> GetUnderperformingTemplatesAsync(decimal threshold = 0.7m, int minUsageCount = 10, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get performance trends for a template over time
    /// </summary>
    Task<TemplatePerformanceTrends> GetPerformanceTrendsAsync(string templateKey, TimeSpan timeWindow, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get aggregated performance metrics by intent type
    /// </summary>
    Task<Dictionary<string, TemplatePerformanceMetrics>> GetPerformanceByIntentTypeAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get performance comparison between templates
    /// </summary>
    Task<TemplatePerformanceComparison> CompareTemplatePerformanceAsync(string templateKey1, string templateKey2, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get recently used templates with performance data
    /// </summary>
    Task<List<TemplatePerformanceMetrics>> GetRecentlyUsedTemplatesAsync(int days = 30, CancellationToken cancellationToken = default);

    /// <summary>
    /// Calculate and update performance metrics for all templates
    /// </summary>
    Task RecalculateAllMetricsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get performance analytics dashboard data
    /// </summary>
    Task<PerformanceDashboardData> GetDashboardDataAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Export performance data for analysis
    /// </summary>
    Task<byte[]> ExportPerformanceDataAsync(DateTime startDate, DateTime endDate, BIReportingCopilot.Core.Models.Analytics.ExportFormat format = BIReportingCopilot.Core.Models.Analytics.ExportFormat.CSV, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get performance alerts for templates requiring attention
    /// </summary>
    Task<List<PerformanceAlert>> GetPerformanceAlertsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get template usage patterns and insights
    /// </summary>
    Task<TemplateUsageInsights> GetUsageInsightsAsync(string templateKey, CancellationToken cancellationToken = default);
}



/// <summary>
/// Template performance trends over time
/// </summary>
public class TemplatePerformanceTrends
{
    public string TemplateKey { get; set; } = string.Empty;
    public TimeSpan TimeWindow { get; set; }
    public List<PerformanceDataPoint> DataPoints { get; set; } = new();
    public decimal TrendDirection { get; set; } // Positive = improving, Negative = declining
    public Dictionary<string, decimal> TrendMetrics { get; set; } = new();
    public DateTime AnalysisDate { get; set; }
}

/// <summary>
/// Performance comparison between two templates
/// </summary>
public class TemplatePerformanceComparison
{
    public TemplatePerformanceMetrics Template1 { get; set; } = new();
    public TemplatePerformanceMetrics Template2 { get; set; } = new();
    public Dictionary<string, ComparisonResult> MetricComparisons { get; set; } = new();
    public string BetterPerformingTemplate { get; set; } = string.Empty;
    public decimal OverallPerformanceDifference { get; set; }
    public List<string> KeyDifferences { get; set; } = new();
    public DateTime ComparisonDate { get; set; }
}

/// <summary>
/// Performance dashboard data
/// </summary>
public class PerformanceDashboardData
{
    public int TotalTemplates { get; set; }
    public int ActiveTemplates { get; set; }
    public decimal OverallSuccessRate { get; set; }
    public int TotalUsagesToday { get; set; }
    public List<TemplatePerformanceMetrics> TopPerformers { get; set; } = new();
    public List<TemplatePerformanceMetrics> NeedsAttention { get; set; } = new();
    public Dictionary<string, int> UsageByIntentType { get; set; } = new();
    public List<PerformanceDataPoint> RecentTrends { get; set; } = new();
    public DateTime GeneratedDate { get; set; }
}

/// <summary>
/// Performance alert for templates requiring attention
/// </summary>
public class PerformanceAlert
{
    public string TemplateKey { get; set; } = string.Empty;
    public string TemplateName { get; set; } = string.Empty;
    public AlertSeverity Severity { get; set; }
    public string AlertType { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public Dictionary<string, object> AlertData { get; set; } = new();
    public DateTime CreatedDate { get; set; }
    public bool IsAcknowledged { get; set; }
}

/// <summary>
/// Template usage insights and patterns
/// </summary>
public class TemplateUsageInsights
{
    public string TemplateKey { get; set; } = string.Empty;
    public List<UsagePattern> UsagePatterns { get; set; } = new();
    public Dictionary<string, int> UsageByTimeOfDay { get; set; } = new();
    public Dictionary<string, int> UsageByDayOfWeek { get; set; } = new();
    public List<string> CommonUserTypes { get; set; } = new();
    public List<string> SuccessFactors { get; set; } = new();
    public List<string> FailureFactors { get; set; } = new();
    public DateTime AnalysisDate { get; set; }
}

#region Supporting Models

public class PerformanceDataPoint
{
    public DateTime Timestamp { get; set; }
    public decimal SuccessRate { get; set; }
    public decimal AverageConfidenceScore { get; set; }
    public int UsageCount { get; set; }
    public decimal? UserRating { get; set; }
    public int ProcessingTime { get; set; }
}

public class ComparisonResult
{
    public string MetricName { get; set; } = string.Empty;
    public decimal Value1 { get; set; }
    public decimal Value2 { get; set; }
    public decimal Difference { get; set; }
    public decimal PercentageDifference { get; set; }
    public string BetterTemplate { get; set; } = string.Empty;
}



public enum AlertSeverity
{
    Low,
    Medium,
    High,
    Critical
}



#endregion
