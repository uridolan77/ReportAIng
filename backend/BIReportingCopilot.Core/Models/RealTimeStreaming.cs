using System.ComponentModel.DataAnnotations;

namespace BIReportingCopilot.Core.Models;

/// <summary>
/// Real-time streaming session
/// </summary>
public class StreamingSession
{
    public string SessionId { get; set; } = Guid.NewGuid().ToString();
    public string UserId { get; set; } = string.Empty;
    public string ConnectionId { get; set; } = string.Empty;
    public StreamingConfiguration Configuration { get; set; } = new();
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
    public DateTime LastActivity { get; set; } = DateTime.UtcNow;
    public SessionStatus Status { get; set; } = SessionStatus.Active;
    public List<StreamSubscription> Subscriptions { get; set; } = new();
    public StreamingMetrics Metrics { get; set; } = new();
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Streaming configuration
/// </summary>
public class StreamingConfiguration
{
    public TimeSpan UpdateInterval { get; set; } = TimeSpan.FromSeconds(5);
    public int MaxEventsPerSecond { get; set; } = 100;
    public bool EnableRealTimeAlerts { get; set; } = true;
    public bool EnableAnomalyDetection { get; set; } = true;
    public List<string> EnabledEventTypes { get; set; } = new();
    public Dictionary<string, object> CustomSettings { get; set; } = new();
}

/// <summary>
/// Data stream event
/// </summary>
public class DataStreamEvent
{
    public string EventId { get; set; } = Guid.NewGuid().ToString();
    public string EventType { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public Dictionary<string, object> Data { get; set; } = new();
    public EventPriority Priority { get; set; } = EventPriority.Normal;
    public List<string> Tags { get; set; } = new();
    public string? UserId { get; set; }
    public string? SessionId { get; set; }
}

/// <summary>
/// Query stream event
/// </summary>
public class QueryStreamEvent
{
    public string QueryId { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string Query { get; set; } = string.Empty;
    public string GeneratedSQL { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public int ExecutionTimeMs { get; set; }
    public bool Success { get; set; }
    public string? Error { get; set; }
    public QueryMetrics Metrics { get; set; } = new();
    public Dictionary<string, object> Context { get; set; } = new();

    // Additional properties expected by Infrastructure services
    public string EventType { get; set; } = "QueryExecution";
}

/// <summary>
/// Real-time dashboard
/// </summary>
public class RealTimeDashboard
{
    public string DashboardId { get; set; } = Guid.NewGuid().ToString();
    public string UserId { get; set; } = string.Empty;
    public string Title { get; set; } = "Real-Time Dashboard";
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    public List<StreamingSession> ActiveSessions { get; set; } = new();
    public RealTimeMetrics RealTimeMetrics { get; set; } = new();
    public List<LiveChart> LiveCharts { get; set; } = new();
    public List<RealTimeAlert> StreamingAlerts { get; set; } = new();
    public List<PerformanceIndicator> PerformanceIndicators { get; set; } = new();
    public TrendAnalysis TrendAnalysis { get; set; } = new();

    // Additional properties expected by Infrastructure services
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    public bool IsLive { get; set; } = true;
    public List<RealTimeRecommendation> Recommendations { get; set; } = new();
    public DashboardLayout Layout { get; set; } = new();
}

/// <summary>
/// Streaming analytics result
/// </summary>
public class StreamingAnalyticsResult
{
    public TimeSpan TimeWindow { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public int TotalEvents { get; set; }
    public Dictionary<string, int> EventsByType { get; set; } = new();
    public double AverageProcessingTime { get; set; }
    public ThroughputMetrics ThroughputMetrics { get; set; } = new();
    public int AnomalyCount { get; set; }
    public PerformanceMetrics PerformanceMetrics { get; set; } = new();
    public UserActivitySummary UserActivitySummary { get; set; } = new();
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;

    // Additional properties expected by Infrastructure services
    public int ActiveSessions { get; set; }
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Stream subscription
/// </summary>
public class StreamSubscription
{
    public string SubscriptionId { get; set; } = Guid.NewGuid().ToString();
    public string UserId { get; set; } = string.Empty;
    public string EventType { get; set; } = string.Empty;
    public Dictionary<string, object> Filters { get; set; } = new();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;
    public SubscriptionConfiguration Configuration { get; set; } = new();
}

/// <summary>
/// Real-time metrics
/// </summary>
public class RealTimeMetrics
{
    public int ActiveUsers { get; set; }
    public int QueriesPerMinute { get; set; }
    public double AverageResponseTime { get; set; }
    public int ErrorRate { get; set; }
    public double SystemLoad { get; set; }
    public int CacheHitRate { get; set; }
    public List<MetricDataPoint> TimeSeries { get; set; } = new();
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

    // Additional properties expected by Infrastructure services
    public int ActiveConnections { get; set; }
    public double QueriesPerSecond { get; set; }
}

/// <summary>
/// Live chart for real-time visualization
/// </summary>
public class LiveChart
{
    public string ChartId { get; set; } = Guid.NewGuid().ToString();
    public string Title { get; set; } = string.Empty;
    public ChartType Type { get; set; } = ChartType.Line;
    public List<ChartDataSeries> DataSeries { get; set; } = new();
    public ChartConfiguration Configuration { get; set; } = new();
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    public TimeSpan UpdateInterval { get; set; } = TimeSpan.FromSeconds(5);
    public bool IsRealTime { get; set; } = true;
}

/// <summary>
/// Real-time alert
/// </summary>
public class RealTimeAlert
{
    public string AlertId { get; set; } = Guid.NewGuid().ToString();
    public string UserId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public AlertSeverity Severity { get; set; } = AlertSeverity.Info;
    public AlertType Type { get; set; } = AlertType.Performance;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? AcknowledgedAt { get; set; }
    public string? AcknowledgedBy { get; set; }
    public AlertCondition Condition { get; set; } = new();
    public Dictionary<string, object> Data { get; set; } = new();
    public List<string> Actions { get; set; } = new();
}

/// <summary>
/// Performance indicator
/// </summary>
public class PerformanceIndicator
{
    public string Name { get; set; } = string.Empty;
    public double Value { get; set; }
    public double Target { get; set; }
    public string Unit { get; set; } = string.Empty;
    public IndicatorStatus Status { get; set; } = IndicatorStatus.Normal;
    public TrendDirection Trend { get; set; } = TrendDirection.Stable;
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    public List<double> History { get; set; } = new();
}

/// <summary>
/// Real-time recommendation
/// </summary>
public class RealTimeRecommendation
{
    public string RecommendationId { get; set; } = Guid.NewGuid().ToString();
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public RecommendationType Type { get; set; } = RecommendationType.Performance;
    public RecommendationPriority Priority { get; set; } = RecommendationPriority.Medium;
    public double Confidence { get; set; }
    public List<string> Actions { get; set; } = new();
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Streaming metrics
/// </summary>
public class StreamingMetrics
{
    public int EventsProcessed { get; set; }
    public int EventsPerSecond { get; set; }
    public double AverageLatency { get; set; }
    public int ErrorCount { get; set; }
    public DateTime StartTime { get; set; } = DateTime.UtcNow;
    public TimeSpan Uptime => DateTime.UtcNow - StartTime;
    public Dictionary<string, object> CustomMetrics { get; set; } = new();
}

/// <summary>
/// Streaming performance metrics
/// </summary>
public class StreamingPerformanceMetrics
{
    public double TotalThroughput { get; set; }
    public double AverageLatency { get; set; }
    public double ErrorRate { get; set; }
    public int ActiveSessions { get; set; }
    public int TotalEvents { get; set; }
    public Dictionary<string, double> EventTypeBreakdown { get; set; } = new();
    public List<PerformanceDataPoint> PerformanceHistory { get; set; } = new();
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;

    // Additional properties expected by Infrastructure services
    public int TotalStreams { get; set; }
    public double ThroughputPerSecond { get; set; }
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    public int ActiveStreams { get; set; }
}

// Supporting classes
public class SubscriptionConfiguration
{
    public TimeSpan BufferTime { get; set; } = TimeSpan.FromSeconds(1);
    public int MaxBufferSize { get; set; } = 100;
    public bool EnableBatching { get; set; } = true;
    public Dictionary<string, object> CustomSettings { get; set; } = new();
}

public class MetricDataPoint
{
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string MetricName { get; set; } = string.Empty;
    public double Value { get; set; }
    public Dictionary<string, object> Tags { get; set; } = new();
}

public class ChartDataSeries
{
    public string Name { get; set; } = string.Empty;
    public List<DataPoint> Data { get; set; } = new();
    public SeriesConfiguration Configuration { get; set; } = new();
}

public class ChartConfiguration
{
    public string XAxisLabel { get; set; } = string.Empty;
    public string YAxisLabel { get; set; } = string.Empty;
    public bool ShowLegend { get; set; } = true;
    public bool EnableZoom { get; set; } = true;
    public bool EnableAnimation { get; set; } = true;
    public Dictionary<string, object> CustomOptions { get; set; } = new();
}

public class AlertCondition
{
    public string MetricName { get; set; } = string.Empty;
    public ComparisonOperator Operator { get; set; } = ComparisonOperator.GreaterThan;
    public double Threshold { get; set; }
    public TimeSpan EvaluationWindow { get; set; } = TimeSpan.FromMinutes(5);
    public int ConsecutiveViolations { get; set; } = 1;
}

public class ThroughputMetrics
{
    public double EventsPerSecond { get; set; }
    public double QueriesPerMinute { get; set; }
    public double DataVolumePerSecond { get; set; }
    public double PeakThroughput { get; set; }
    public DateTime PeakTime { get; set; }
}

public class UserActivitySummary
{
    public int ActiveUsers { get; set; }
    public int TotalQueries { get; set; }
    public double AverageSessionDuration { get; set; }
    public Dictionary<string, int> ActivityByType { get; set; } = new();
    public List<UserActivityPattern> Patterns { get; set; } = new();

    // Additional properties for compatibility with API controllers
    public int QueriesThisWeek { get; set; }
    public int QueriesThisMonth { get; set; }
    public DateTime LastActivity { get; set; } = DateTime.UtcNow;
}

public class UserActivityPattern
{
    public string PatternType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public double Frequency { get; set; }
    public TimeSpan Duration { get; set; }
}

public class SeriesConfiguration
{
    public string Color { get; set; } = string.Empty;
    public LineStyle LineStyle { get; set; } = LineStyle.Solid;
    public int LineWidth { get; set; } = 2;
    public bool ShowMarkers { get; set; } = true;
    public Dictionary<string, object> CustomOptions { get; set; } = new();
}

public class PerformanceDataPoint
{
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public double Throughput { get; set; }
    public double Latency { get; set; }
    public double ErrorRate { get; set; }
    public int ActiveSessions { get; set; }
}

/// <summary>
/// Time series data point for charts and analytics
/// </summary>
public class TimeSeriesDataPoint
{
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public double Value { get; set; }
    public string? Label { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Performance history point for tracking over time
/// </summary>
public class PerformanceHistoryPoint
{
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public double Throughput { get; set; }
    public double Latency { get; set; }
    public double ErrorRate { get; set; }
    public int ActiveSessions { get; set; }
    public Dictionary<string, double> Metrics { get; set; } = new();
}

// Enumerations
public enum SessionStatus
{
    Active,
    Inactive,
    Paused,
    Terminated
}

public enum EventPriority
{
    Low,
    Normal,
    High,
    Critical
}

public enum AlertSeverity
{
    Info,
    Warning,
    Error,
    Critical
}

public enum AlertType
{
    Performance,
    Error,
    Security,
    Business,
    System
}

public enum IndicatorStatus
{
    Normal,
    Warning,
    Critical,
    Unknown
}

public enum TrendDirection
{
    Improving,
    Stable,
    Degrading,
    Volatile
}

public enum ComparisonOperator
{
    GreaterThan,
    LessThan,
    Equals,
    NotEquals,
    GreaterThanOrEqual,
    LessThanOrEqual
}

public enum LineStyle
{
    Solid,
    Dashed,
    Dotted,
    DashDot
}

public enum ChartType
{
    Line,
    Bar,
    Pie,
    Area,
    Scatter,
    Gauge,
    Heatmap
}

/// <summary>
/// Streaming query response
/// </summary>
public class StreamingQueryResponse
{
    public string QueryId { get; set; } = string.Empty;
    public string SessionId { get; set; } = string.Empty;
    public StreamingResponseType Type { get; set; } = StreamingResponseType.SQLGeneration; // Added missing property
    public string Content { get; set; } = string.Empty; // Added missing property
    public double Progress { get; set; } = 0.0; // Added missing property
    public string? PartialSql { get; set; } // Added missing property
    public string? GeneratedSql { get; set; } // Added missing property
    public bool IsComplete { get; set; }
    public int RowCount { get; set; }
    public object Data { get; set; } = new();
    public string? ErrorMessage { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Streaming insight response
/// </summary>
public class StreamingInsightResponse
{
    public string InsightId { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty; // Added missing property
    public double Progress { get; set; } = 0.0; // Added missing property
    public string? PartialInsight { get; set; } // Added missing property
    public bool IsComplete { get; set; } // Added missing property
    public DateTime Timestamp { get; set; } = DateTime.UtcNow; // Added missing property
    public double Confidence { get; set; }
    public Dictionary<string, object> Data { get; set; } = new();
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Streaming analytics update
/// </summary>
public class StreamingAnalyticsUpdate
{
    public string UpdateId { get; set; } = string.Empty;
    public string SessionId { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty; // Added missing property
    public string MetricType { get; set; } = string.Empty;
    public Dictionary<string, object> Metrics { get; set; } = new();
    public List<string> Trends { get; set; } = new(); // Added missing property
    public List<string> Alerts { get; set; } = new(); // Added missing property
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Streaming session configuration
/// </summary>
public class StreamingSessionConfig
{
    public string SessionId { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public Dictionary<string, object> Configuration { get; set; } = new();
    public TimeSpan Timeout { get; set; } = TimeSpan.FromMinutes(30);
    public int MaxConnections { get; set; } = 100;
    public bool EnableCompression { get; set; } = true;
}

/// <summary>
/// Streaming session information
/// </summary>
public class StreamingSessionInfo
{
    public string SessionId { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime StartedAt { get; set; } = DateTime.UtcNow; // Added missing property
    public DateTime? EndTime { get; set; }
    public DateTime? EndedAt { get; set; } // Added missing property
    public string Status { get; set; } = string.Empty;
    public StreamingSessionConfig Configuration { get; set; } = new(); // Added missing property
    public int ActiveConnections { get; set; }
    public int ConnectionCount { get; set; } // Added missing property
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Streaming alert
/// </summary>
public class StreamingAlert
{
    public string AlertId { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public Dictionary<string, object> Context { get; set; } = new();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow; // Added missing property
    public bool IsAcknowledged { get; set; }
}
