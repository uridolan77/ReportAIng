using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace BIReportingCopilot.Infrastructure.Monitoring;

/// <summary>
/// Metrics collector for performance monitoring and observability
/// </summary>
public class MetricsCollector : IMetricsCollector, IDisposable
{
    private readonly ILogger<MetricsCollector> _logger;
    private readonly Meter _meter;
    private readonly ConcurrentDictionary<string, Counter<long>> _counters;
    private readonly ConcurrentDictionary<string, Histogram<double>> _histograms;
    private readonly ConcurrentDictionary<string, ObservableGauge<double>> _gauges;
    private readonly ConcurrentDictionary<string, double> _gaugeValues;
    private readonly Timer _metricsTimer;

    // Counters
    private readonly Counter<long> _queryExecutionsTotal;
    private readonly Counter<long> _aiOperationsTotal;
    private readonly Counter<long> _cacheOperationsTotal;
    private readonly Counter<long> _errorsTotal;

    // Histograms
    private readonly Histogram<double> _queryDuration;
    private readonly Histogram<double> _aiOperationDuration;
    private readonly Histogram<double> _cacheOperationDuration;
    private readonly Histogram<double> _requestDuration;

    // Gauges
    private readonly ObservableGauge<double> _activeConnections;
    private readonly ObservableGauge<double> _memoryUsage;
    private readonly ObservableGauge<double> _cpuUsage;

    public MetricsCollector(ILogger<MetricsCollector> logger)
    {
        _logger = logger;
        _meter = new Meter("BIReportingCopilot", "1.0.0");
        _counters = new ConcurrentDictionary<string, Counter<long>>();
        _histograms = new ConcurrentDictionary<string, Histogram<double>>();
        _gauges = new ConcurrentDictionary<string, ObservableGauge<double>>();
        _gaugeValues = new ConcurrentDictionary<string, double>();

        // Initialize standard metrics
        _queryExecutionsTotal = _meter.CreateCounter<long>(
            "query_executions_total",
            "Total number of query executions");

        _aiOperationsTotal = _meter.CreateCounter<long>(
            "ai_operations_total",
            "Total number of AI operations");

        _cacheOperationsTotal = _meter.CreateCounter<long>(
            "cache_operations_total",
            "Total number of cache operations");

        _errorsTotal = _meter.CreateCounter<long>(
            "errors_total",
            "Total number of errors");

        _queryDuration = _meter.CreateHistogram<double>(
            "query_duration_ms",
            "Query execution duration in milliseconds");

        _aiOperationDuration = _meter.CreateHistogram<double>(
            "ai_operation_duration_ms",
            "AI operation duration in milliseconds");

        _cacheOperationDuration = _meter.CreateHistogram<double>(
            "cache_operation_duration_ms",
            "Cache operation duration in milliseconds");

        _requestDuration = _meter.CreateHistogram<double>(
            "request_duration_ms",
            "HTTP request duration in milliseconds");

        _activeConnections = _meter.CreateObservableGauge<double>(
            "active_connections",
            "Number of active database connections",
            () => _gaugeValues.GetValueOrDefault("active_connections", 0));

        _memoryUsage = _meter.CreateObservableGauge<double>(
            "memory_usage_mb",
            "Memory usage in megabytes",
            () => GC.GetTotalMemory(false) / 1024.0 / 1024.0);

        _cpuUsage = _meter.CreateObservableGauge<double>(
            "cpu_usage_percent",
            "CPU usage percentage",
            () => _gaugeValues.GetValueOrDefault("cpu_usage", 0));

        // Start periodic metrics collection
        _metricsTimer = new Timer(CollectSystemMetrics, null, TimeSpan.Zero, TimeSpan.FromSeconds(30));

        _logger.LogInformation("Metrics collector initialized");
    }

    public void RecordQueryExecution(string queryType, long durationMs, bool success, int rowCount = 0)
    {
        var tags = new TagList
        {
            ["type"] = queryType,
            ["success"] = success.ToString().ToLower(),
            ["row_count_range"] = GetRowCountRange(rowCount)
        };

        _queryExecutionsTotal.Add(1, tags);
        _queryDuration.Record(durationMs, tags);

        _logger.LogDebug("Recorded query execution: {QueryType}, {Duration}ms, Success: {Success}, Rows: {RowCount}",
            queryType, durationMs, success, rowCount);
    }

    public void RecordAIOperation(string operation, long durationMs, bool success, double? confidence = null)
    {
        var tags = new TagList
        {
            ["operation"] = operation,
            ["success"] = success.ToString().ToLower(),
            ["confidence_range"] = GetConfidenceRange(confidence)
        };

        _aiOperationsTotal.Add(1, tags);
        _aiOperationDuration.Record(durationMs, tags);

        _logger.LogDebug("Recorded AI operation: {Operation}, {Duration}ms, Success: {Success}, Confidence: {Confidence:P1}",
            operation, durationMs, success, confidence ?? 0.0);
    }

    public void RecordCacheOperation(string operation, bool hit, long durationMs = 0)
    {
        var tags = new TagList
        {
            ["operation"] = operation,
            ["hit"] = hit.ToString().ToLower()
        };

        _cacheOperationsTotal.Add(1, tags);
        _cacheOperationDuration.Record(durationMs, tags);

        _logger.LogDebug("Recorded cache operation: {Operation}, Hit: {Hit}, {Duration}ms",
            operation, hit, durationMs);
    }

    public void RecordError(string errorType, string source, Exception? exception = null)
    {
        var tags = new TagList
        {
            ["error_type"] = errorType,
            ["source"] = source,
            ["exception_type"] = exception?.GetType().Name ?? "Unknown"
        };

        _errorsTotal.Add(1, tags);

        _logger.LogDebug("Recorded error: {ErrorType} from {Source}, Exception: {ExceptionType}",
            errorType, source, exception?.GetType().Name ?? "Unknown");
    }

    public void RecordRequestDuration(string method, string endpoint, int statusCode, long durationMs)
    {
        var tags = new TagList
        {
            ["method"] = method,
            ["endpoint"] = endpoint,
            ["status_code"] = statusCode.ToString(),
            ["status_class"] = GetStatusClass(statusCode)
        };

        _requestDuration.Record(durationMs, tags);

        _logger.LogDebug("Recorded request: {Method} {Endpoint}, {StatusCode}, {Duration}ms",
            method, endpoint, statusCode, durationMs);
    }

    public void SetGaugeValue(string name, double value)
    {
        _gaugeValues.AddOrUpdate(name, value, (key, oldValue) => value);
    }

    public void IncrementCounter(string name, TagList? tags = null)
    {
        var counter = _counters.GetOrAdd(name, key => _meter.CreateCounter<long>(key));
        counter.Add(1, tags ?? new TagList());
    }

    public void RecordHistogram(string name, double value, TagList? tags = null)
    {
        var histogram = _histograms.GetOrAdd(name, key => _meter.CreateHistogram<double>(key));
        histogram.Record(value, tags ?? new TagList());
    }

    public MetricsSnapshot GetSnapshot()
    {
        return new MetricsSnapshot
        {
            Timestamp = DateTimeOffset.UtcNow,
            MemoryUsageMB = GC.GetTotalMemory(false) / 1024.0 / 1024.0,
            ActiveConnections = _gaugeValues.GetValueOrDefault("active_connections", 0),
            CpuUsagePercent = _gaugeValues.GetValueOrDefault("cpu_usage", 0),
            TotalQueries = _gaugeValues.GetValueOrDefault("total_queries", 0),
            TotalAIOperations = _gaugeValues.GetValueOrDefault("total_ai_operations", 0),
            TotalCacheOperations = _gaugeValues.GetValueOrDefault("total_cache_operations", 0),
            TotalErrors = _gaugeValues.GetValueOrDefault("total_errors", 0)
        };
    }

    private void CollectSystemMetrics(object? state)
    {
        try
        {
            // Collect CPU usage (simplified)
            using var process = Process.GetCurrentProcess();
            var cpuUsage = process.TotalProcessorTime.TotalMilliseconds / Environment.ProcessorCount / Environment.TickCount * 100;
            SetGaugeValue("cpu_usage", Math.Min(100, Math.Max(0, cpuUsage)));

            // Collect other system metrics as needed
            SetGaugeValue("thread_count", process.Threads.Count);
            SetGaugeValue("handle_count", process.HandleCount);

        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to collect system metrics");
        }
    }

    private static string GetRowCountRange(int rowCount)
    {
        return rowCount switch
        {
            0 => "0",
            <= 10 => "1-10",
            <= 100 => "11-100",
            <= 1000 => "101-1000",
            <= 10000 => "1001-10000",
            _ => "10000+"
        };
    }

    private static string GetConfidenceRange(double? confidence)
    {
        if (!confidence.HasValue) return "unknown";

        return confidence.Value switch
        {
            < 0.3 => "low",
            < 0.7 => "medium",
            _ => "high"
        };
    }

    private static string GetStatusClass(int statusCode)
    {
        return statusCode switch
        {
            >= 200 and < 300 => "2xx",
            >= 300 and < 400 => "3xx",
            >= 400 and < 500 => "4xx",
            >= 500 => "5xx",
            _ => "other"
        };
    }

    public void Dispose()
    {
        _metricsTimer?.Dispose();
        _meter?.Dispose();
        _logger.LogInformation("Metrics collector disposed");
    }
}

/// <summary>
/// Interface for metrics collection
/// </summary>
public interface IMetricsCollector
{
    void RecordQueryExecution(string queryType, long durationMs, bool success, int rowCount = 0);
    void RecordAIOperation(string operation, long durationMs, bool success, double? confidence = null);
    void RecordCacheOperation(string operation, bool hit, long durationMs = 0);
    void RecordError(string errorType, string source, Exception? exception = null);
    void RecordRequestDuration(string method, string endpoint, int statusCode, long durationMs);
    void SetGaugeValue(string name, double value);
    void IncrementCounter(string name, TagList? tags = null);
    void RecordHistogram(string name, double value, TagList? tags = null);
    MetricsSnapshot GetSnapshot();
}

/// <summary>
/// Snapshot of current metrics
/// </summary>
public class MetricsSnapshot
{
    public DateTimeOffset Timestamp { get; set; }
    public double MemoryUsageMB { get; set; }
    public double ActiveConnections { get; set; }
    public double CpuUsagePercent { get; set; }
    public double TotalQueries { get; set; }
    public double TotalAIOperations { get; set; }
    public double TotalCacheOperations { get; set; }
    public double TotalErrors { get; set; }
}
