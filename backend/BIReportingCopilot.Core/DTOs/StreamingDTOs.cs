namespace BIReportingCopilot.Core.DTOs;

// ============================================================================
// STREAMING API REQUEST/RESPONSE DTOS
// ============================================================================

/// <summary>
/// Base streaming response for all streaming endpoints
/// </summary>
public class StreamingQueryResponse
{
    public string SessionId { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty; // "progress", "result", "error", "complete"
    public object? Data { get; set; }
    public DateTime Timestamp { get; set; }
    public int SequenceNumber { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Query processing progress for streaming
/// </summary>
public class QueryProcessingProgress
{
    public string Phase { get; set; } = string.Empty; // "parsing", "understanding", "generation", "execution", "insights"
    public double Progress { get; set; } // 0.0 to 1.0
    public string CurrentStep { get; set; } = string.Empty;
    public double Confidence { get; set; }
    public TimeSpan EstimatedTimeRemaining { get; set; }
    public Dictionary<string, object> PhaseMetadata { get; set; } = new();
    public List<string> CompletedSteps { get; set; } = new();
    public List<string> RemainingSteps { get; set; } = new();
}

/// <summary>
/// Streaming insight generation response
/// </summary>
public class StreamingInsightResponse
{
    public string Type { get; set; } = string.Empty; // "insight_generation", "analysis", "recommendation"
    public string Content { get; set; } = string.Empty;
    public string PartialInsight { get; set; } = string.Empty;
    public double Progress { get; set; }
    public DateTime Timestamp { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();
    public bool IsComplete { get; set; }
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Streaming analytics update
/// </summary>
public class StreamingAnalyticsUpdate
{
    public string UpdateType { get; set; } = string.Empty; // "metric", "alert", "trend", "anomaly"
    public string MetricName { get; set; } = string.Empty;
    public object Value { get; set; } = new();
    public DateTime Timestamp { get; set; }
    public string Severity { get; set; } = string.Empty; // "Info", "Warning", "Error", "Critical"
    public Dictionary<string, object> Context { get; set; } = new();
    public List<string> Tags { get; set; } = new();
}

/// <summary>
/// Streaming session information
/// </summary>
public class StreamingSession
{
    public string SessionId { get; set; } = string.Empty;
    public string Query { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty; // "starting", "processing", "completed", "error", "cancelled"
    public double Progress { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public List<StreamingPhase> Phases { get; set; } = new();
    public StreamingPhase? CurrentPhase { get; set; }
    public string? ErrorMessage { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Individual phase in streaming processing
/// </summary>
public class StreamingPhase
{
    public string PhaseName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty; // "pending", "active", "completed", "error"
    public double Progress { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public List<string> Steps { get; set; } = new();
    public string? CurrentStep { get; set; }
    public Dictionary<string, object> Results { get; set; } = new();
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Streaming options for configuration
/// </summary>
public class StreamingOptions
{
    public bool EnableProgressUpdates { get; set; } = true;
    public bool EnableDetailedMetrics { get; set; } = false;
    public int UpdateIntervalMs { get; set; } = 500;
    public int MaxRetries { get; set; } = 3;
    public TimeSpan Timeout { get; set; } = TimeSpan.FromMinutes(5);
    public Dictionary<string, object> CustomOptions { get; set; } = new();
}

/// <summary>
/// Real-time transparency update for streaming
/// </summary>
public class StreamingTransparencyUpdate
{
    public string TraceId { get; set; } = string.Empty;
    public string UpdateType { get; set; } = string.Empty; // "confidence", "step", "alternative", "optimization"
    public object UpdateData { get; set; } = new();
    public DateTime Timestamp { get; set; }
    public double CurrentConfidence { get; set; }
    public string CurrentPhase { get; set; } = string.Empty;
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Streaming cost monitoring update
/// </summary>
public class StreamingCostUpdate
{
    public string SessionId { get; set; } = string.Empty;
    public double CurrentCost { get; set; }
    public double EstimatedTotalCost { get; set; }
    public int TokensUsed { get; set; }
    public int EstimatedTotalTokens { get; set; }
    public string CostBreakdown { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public List<string> CostAlerts { get; set; } = new();
}

/// <summary>
/// Streaming performance monitoring update
/// </summary>
public class StreamingPerformanceUpdate
{
    public string SessionId { get; set; } = string.Empty;
    public double ResponseTime { get; set; }
    public double Throughput { get; set; }
    public int ActiveConnections { get; set; }
    public double CpuUsage { get; set; }
    public double MemoryUsage { get; set; }
    public DateTime Timestamp { get; set; }
    public Dictionary<string, double> CustomMetrics { get; set; } = new();
}

/// <summary>
/// Request to start streaming query processing
/// </summary>
public class StartStreamingRequest
{
    public string Query { get; set; } = string.Empty;
    public string? UserId { get; set; }
    public StreamingOptions Options { get; set; } = new();
    public Dictionary<string, object> Context { get; set; } = new();
    public bool EnableTransparency { get; set; } = true;
    public bool EnableCostMonitoring { get; set; } = true;
}

/// <summary>
/// Request to cancel streaming session
/// </summary>
public class CancelStreamingRequest
{
    public string SessionId { get; set; } = string.Empty;
    public string? Reason { get; set; }
    public bool ForceCancel { get; set; } = false;
}

/// <summary>
/// Streaming health status
/// </summary>
public class StreamingHealthStatus
{
    public bool IsHealthy { get; set; }
    public int ActiveSessions { get; set; }
    public int TotalSessions { get; set; }
    public double AverageResponseTime { get; set; }
    public double ErrorRate { get; set; }
    public DateTime LastUpdate { get; set; }
    public List<string> Issues { get; set; } = new();
    public Dictionary<string, object> Metrics { get; set; } = new();
}

/// <summary>
/// Streaming configuration settings
/// </summary>
public class StreamingConfiguration
{
    public int MaxConcurrentSessions { get; set; } = 100;
    public TimeSpan SessionTimeout { get; set; } = TimeSpan.FromMinutes(10);
    public int BufferSize { get; set; } = 1024;
    public bool EnableCompression { get; set; } = true;
    public bool EnableHeartbeat { get; set; } = true;
    public TimeSpan HeartbeatInterval { get; set; } = TimeSpan.FromSeconds(30);
    public Dictionary<string, object> AdvancedSettings { get; set; } = new();
}

/// <summary>
/// Streaming analytics summary
/// </summary>
public class StreamingAnalyticsSummary
{
    public int TotalSessions { get; set; }
    public int ActiveSessions { get; set; }
    public int CompletedSessions { get; set; }
    public int ErrorSessions { get; set; }
    public double AverageSessionDuration { get; set; }
    public double AverageResponseTime { get; set; }
    public double TotalDataTransferred { get; set; }
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public Dictionary<string, int> SessionsByType { get; set; } = new();
    public Dictionary<string, double> PerformanceMetrics { get; set; } = new();
}

/// <summary>
/// Real-time streaming event
/// </summary>
public class StreamingEvent
{
    public string EventId { get; set; } = string.Empty;
    public string EventType { get; set; } = string.Empty;
    public string SessionId { get; set; } = string.Empty;
    public object EventData { get; set; } = new();
    public DateTime Timestamp { get; set; }
    public string Severity { get; set; } = string.Empty;
    public Dictionary<string, object> Properties { get; set; } = new();
}
