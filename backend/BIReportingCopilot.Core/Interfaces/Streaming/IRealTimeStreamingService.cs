using BIReportingCopilot.Core.Models;

namespace BIReportingCopilot.Core.Interfaces.Streaming;

/// <summary>
/// Real-time streaming service interface for live data processing
/// </summary>
public interface IRealTimeStreamingService
{
    Task<string> StartStreamingQueryAsync(StreamingQueryRequest request, CancellationToken cancellationToken = default);
    Task StopStreamingQueryAsync(string streamId, CancellationToken cancellationToken = default);
    Task<StreamingQueryStatus> GetStreamingStatusAsync(string streamId, CancellationToken cancellationToken = default);
    Task<List<StreamingQueryInfo>> GetActiveStreamsAsync(string? userId = null, CancellationToken cancellationToken = default);
    IAsyncEnumerable<StreamingDataPoint> GetStreamingDataAsync(string streamId, CancellationToken cancellationToken = default);
    Task<bool> UpdateStreamingConfigurationAsync(string streamId, StreamingConfiguration config, CancellationToken cancellationToken = default);
    Task<StreamingMetrics> GetStreamingMetricsAsync(string streamId, CancellationToken cancellationToken = default);
    Task<List<StreamingAlert>> GetStreamingAlertsAsync(string streamId, CancellationToken cancellationToken = default);
    Task<bool> CreateStreamingAlertAsync(string streamId, StreamingAlertRule rule, CancellationToken cancellationToken = default);
    Task<string> CreateRealTimeAlertAsync(RealTimeAlert alert, string userId, CancellationToken cancellationToken = default);

    // Additional methods needed by handlers
    Task<StreamingSession> StartStreamingSessionAsync(string userId, StreamingConfiguration configuration);
    Task<bool> StopStreamingSessionAsync(string sessionId, string userId);
    Task ProcessDataStreamEventAsync(DataStreamEvent streamEvent);
    Task ProcessQueryStreamEventAsync(QueryStreamEvent queryEvent);
    Task<RealTimeDashboard> GetRealTimeDashboardAsync(string userId);
}

/// <summary>
/// Streaming query request
/// </summary>
public class StreamingQueryRequest
{
    public string Query { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public StreamingConfiguration Configuration { get; set; } = new();
    public List<StreamingAlertRule> AlertRules { get; set; } = new();
    public Dictionary<string, object> Parameters { get; set; } = new();
}

// StreamingConfiguration has been consolidated into Core.Models.StreamingConfiguration
// See: Core/Models/RealTimeStreaming.cs

/// <summary>
/// Streaming query status
/// </summary>
public class StreamingQueryStatus
{
    public string StreamId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty; // active, paused, stopped, error
    public DateTime StartedAt { get; set; }
    public DateTime? LastUpdateAt { get; set; }
    public int DataPointsGenerated { get; set; }
    public string? LastError { get; set; }
    public StreamingMetrics Metrics { get; set; } = new();
}

/// <summary>
/// Streaming query information
/// </summary>
public class StreamingQueryInfo
{
    public string StreamId { get; set; } = string.Empty;
    public string Query { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public StreamingConfiguration Configuration { get; set; } = new();
    public StreamingMetrics Metrics { get; set; } = new();
}

/// <summary>
/// Streaming data point
/// </summary>
public class StreamingDataPoint
{
    public DateTime Timestamp { get; set; }
    public Dictionary<string, object> Data { get; set; } = new();
    public string StreamId { get; set; } = string.Empty;
    public long SequenceNumber { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();
}

// StreamingMetrics has been consolidated into Core.Models.StreamingMetrics
// See: Core/Models/RealTimeStreaming.cs

/// <summary>
/// Streaming alert rule
/// </summary>
public class StreamingAlertRule
{
    public string RuleId { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Condition { get; set; } = string.Empty; // e.g., "value > 100"
    public string TargetField { get; set; } = string.Empty;
    public string AlertType { get; set; } = "threshold"; // threshold, anomaly, pattern
    public bool IsEnabled { get; set; } = true;
    public List<string> NotificationChannels { get; set; } = new();
    public Dictionary<string, object> Parameters { get; set; } = new();
}

/// <summary>
/// Streaming alert
/// </summary>
public class StreamingAlert
{
    public string AlertId { get; set; } = Guid.NewGuid().ToString();
    public string RuleId { get; set; } = string.Empty;
    public string StreamId { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Severity { get; set; } = "medium"; // low, medium, high, critical
    public DateTime TriggeredAt { get; set; }
    public bool IsAcknowledged { get; set; } = false;
    public Dictionary<string, object> Context { get; set; } = new();
}

// RealTimeAlert class is defined in BIReportingCopilot.Core.Models.RealTimeStreaming
