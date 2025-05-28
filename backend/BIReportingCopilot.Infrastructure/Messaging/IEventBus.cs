using System.Text.Json;

namespace BIReportingCopilot.Infrastructure.Messaging;

/// <summary>
/// Event bus interface for publishing and subscribing to domain events
/// </summary>
public interface IEventBus
{
    Task PublishAsync<T>(T @event, CancellationToken cancellationToken = default) where T : class, IEvent;
    Task PublishAsync<T>(T @event, string routingKey, CancellationToken cancellationToken = default) where T : class, IEvent;
    Task SubscribeAsync<T>(Func<T, CancellationToken, Task> handler, CancellationToken cancellationToken = default) where T : class, IEvent;
    Task SubscribeAsync<T>(string routingKey, Func<T, CancellationToken, Task> handler, CancellationToken cancellationToken = default) where T : class, IEvent;
    Task StartAsync(CancellationToken cancellationToken = default);
    Task StopAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Base interface for all domain events
/// </summary>
public interface IEvent
{
    string EventId { get; }
    DateTime Timestamp { get; }
    string EventType { get; }
    string Source { get; }
    Dictionary<string, object> Metadata { get; }
}

/// <summary>
/// Base implementation for domain events
/// </summary>
public abstract class BaseEvent : IEvent
{
    public string EventId { get; } = Guid.NewGuid().ToString();
    public DateTime Timestamp { get; } = DateTime.UtcNow;
    public abstract string EventType { get; }
    public virtual string Source { get; } = "BIReportingCopilot";
    public Dictionary<string, object> Metadata { get; } = new();

    protected BaseEvent()
    {
        Metadata["MachineName"] = Environment.MachineName;
        Metadata["ProcessId"] = Environment.ProcessId;
    }
}

/// <summary>
/// Event published when a query is executed
/// </summary>
public class QueryExecutedEvent : BaseEvent
{
    public override string EventType => "QueryExecuted";
    
    public string UserId { get; set; } = string.Empty;
    public string NaturalLanguageQuery { get; set; } = string.Empty;
    public string GeneratedSQL { get; set; } = string.Empty;
    public bool IsSuccessful { get; set; }
    public long ExecutionTimeMs { get; set; }
    public int RowCount { get; set; }
    public double? ConfidenceScore { get; set; }
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Event published when user provides feedback
/// </summary>
public class FeedbackReceivedEvent : BaseEvent
{
    public override string EventType => "FeedbackReceived";
    
    public string UserId { get; set; } = string.Empty;
    public string QueryId { get; set; } = string.Empty;
    public int Rating { get; set; }
    public string? Comments { get; set; }
    public string FeedbackType { get; set; } = string.Empty;
}

/// <summary>
/// Event published when an anomaly is detected
/// </summary>
public class AnomalyDetectedEvent : BaseEvent
{
    public override string EventType => "AnomalyDetected";
    
    public string UserId { get; set; } = string.Empty;
    public string AnomalyType { get; set; } = string.Empty;
    public double AnomalyScore { get; set; }
    public string RiskLevel { get; set; } = string.Empty;
    public List<string> DetectedAnomalies { get; set; } = new();
    public string Query { get; set; } = string.Empty;
}

/// <summary>
/// Event published when cache is invalidated
/// </summary>
public class CacheInvalidatedEvent : BaseEvent
{
    public override string EventType => "CacheInvalidated";
    
    public string CacheType { get; set; } = string.Empty;
    public string InvalidationReason { get; set; } = string.Empty;
    public List<string> AffectedKeys { get; set; } = new();
    public string? TableName { get; set; }
}

/// <summary>
/// Event published when data schema changes
/// </summary>
public class SchemaChangedEvent : BaseEvent
{
    public override string EventType => "SchemaChanged";
    
    public string TableName { get; set; } = string.Empty;
    public string ChangeType { get; set; } = string.Empty; // Added, Modified, Deleted
    public string? ColumnName { get; set; }
    public Dictionary<string, object> ChangeDetails { get; set; } = new();
}

/// <summary>
/// Event published when AI model is retrained
/// </summary>
public class ModelRetrainedEvent : BaseEvent
{
    public override string EventType => "ModelRetrained";
    
    public string ModelType { get; set; } = string.Empty;
    public int TrainingSamples { get; set; }
    public double ModelAccuracy { get; set; }
    public TimeSpan TrainingDuration { get; set; }
    public string ModelVersion { get; set; } = string.Empty;
}

/// <summary>
/// Event published when system performance metrics are collected
/// </summary>
public class PerformanceMetricsEvent : BaseEvent
{
    public override string EventType => "PerformanceMetrics";
    
    public double CpuUsagePercent { get; set; }
    public double MemoryUsageMB { get; set; }
    public int ActiveConnections { get; set; }
    public double CacheHitRate { get; set; }
    public long TotalQueries { get; set; }
    public double AverageResponseTimeMs { get; set; }
}

/// <summary>
/// Event handler interface
/// </summary>
public interface IEventHandler<in T> where T : class, IEvent
{
    Task HandleAsync(T @event, CancellationToken cancellationToken = default);
}

/// <summary>
/// Event serialization helper
/// </summary>
public static class EventSerializer
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
    };

    public static string Serialize<T>(T @event) where T : class, IEvent
    {
        return JsonSerializer.Serialize(@event, Options);
    }

    public static T? Deserialize<T>(string json) where T : class, IEvent
    {
        return JsonSerializer.Deserialize<T>(json, Options);
    }

    public static object? Deserialize(string json, Type eventType)
    {
        return JsonSerializer.Deserialize(json, eventType, Options);
    }
}

/// <summary>
/// Event bus configuration
/// </summary>
public class EventBusConfiguration
{
    public string ConnectionString { get; set; } = string.Empty;
    public string ExchangeName { get; set; } = "bi-reporting-events";
    public string QueuePrefix { get; set; } = "bi-reporting";
    public bool EnableRetries { get; set; } = true;
    public int MaxRetryAttempts { get; set; } = 3;
    public TimeSpan RetryDelay { get; set; } = TimeSpan.FromSeconds(5);
    public bool EnableDeadLetterQueue { get; set; } = true;
    public int MaxConcurrentHandlers { get; set; } = 10;
    public TimeSpan MessageTimeout { get; set; } = TimeSpan.FromMinutes(5);
}
