using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using BIReportingCopilot.Infrastructure.Configuration;
using BIReportingCopilot.Core.Configuration;
using BIReportingCopilot.Core.Interfaces;

namespace BIReportingCopilot.Infrastructure.Monitoring;

/// <summary>
/// Unified monitoring management service consolidating metrics collection, tracing, and logging
/// Replaces MetricsCollector, TracedQueryService, and CorrelatedLogger
/// </summary>
public class MonitoringManagementService : IMetricsCollector, IDisposable
{
    private readonly ILogger<MonitoringManagementService> _logger;
    private readonly UnifiedConfigurationService _configurationService;
    private readonly MonitoringConfiguration _monitoringConfig;
    private readonly Meter _meter;
    private readonly ConcurrentDictionary<string, Counter<long>> _counters;
    private readonly ConcurrentDictionary<string, Histogram<double>> _histograms;
    private readonly ConcurrentDictionary<string, double> _gaugeValues;
    private readonly Timer? _metricsTimer;
    private readonly ActivitySource _activitySource;

    // Standard metrics
    private Counter<long> _queryExecutionsTotal;
    private Counter<long> _aiOperationsTotal;
    private Counter<long> _cacheOperationsTotal;
    private Counter<long> _errorsTotal;
    private Histogram<double> _queryDuration;
    private Histogram<double> _aiOperationDuration;
    private Histogram<double> _requestDuration;

    public MonitoringManagementService(
        ILogger<MonitoringManagementService> logger,
        UnifiedConfigurationService configurationService)
    {
        _logger = logger;
        _configurationService = configurationService;
        _monitoringConfig = configurationService.GetMonitoringSettings();
        _meter = new Meter("BIReportingCopilot", "1.0.0");
        _counters = new ConcurrentDictionary<string, Counter<long>>();
        _histograms = new ConcurrentDictionary<string, Histogram<double>>();
        _gaugeValues = new ConcurrentDictionary<string, double>();
        _activitySource = new ActivitySource("BIReportingCopilot");

        InitializeStandardMetrics();

        if (_monitoringConfig.EnableMetrics)
        {
            _metricsTimer = new Timer(CollectSystemMetrics, null, TimeSpan.Zero, TimeSpan.FromSeconds(30));
        }

        _logger.LogInformation("Monitoring management service initialized");
    }

    #region Metrics Collection

    /// <summary>
    /// Record query execution metrics
    /// </summary>
    public void RecordQueryExecution(string queryType, long durationMs, bool success, int rowCount = 0)
    {
        if (!_monitoringConfig.EnableMetrics) return;

        var tags = new TagList
        {
            { "type", queryType },
            { "success", success.ToString().ToLower() },
            { "row_count_range", GetRowCountRange(rowCount) }
        };

        _queryExecutionsTotal.Add(1, tags);
        _queryDuration.Record(durationMs, tags);

        if (_monitoringConfig.EnableRequestLogging)
        {
            _logger.LogDebug("Query executed: {QueryType}, {Duration}ms, Success: {Success}, Rows: {RowCount}",
                queryType, durationMs, success, rowCount);
        }
    }

    /// <summary>
    /// Record AI operation metrics
    /// </summary>
    public void RecordAIOperation(string operation, long durationMs, bool success, double? confidence = null)
    {
        if (!_monitoringConfig.EnableMetrics) return;

        var tags = new TagList
        {
            { "operation", operation },
            { "success", success.ToString().ToLower() },
            { "confidence_range", GetConfidenceRange(confidence) }
        };

        _aiOperationsTotal.Add(1, tags);
        _aiOperationDuration.Record(durationMs, tags);

        if (_monitoringConfig.EnableRequestLogging)
        {
            _logger.LogDebug("AI operation: {Operation}, {Duration}ms, Success: {Success}, Confidence: {Confidence:P1}",
                operation, durationMs, success, confidence ?? 0.0);
        }
    }

    /// <summary>
    /// Record cache operation metrics
    /// </summary>
    public void RecordCacheOperation(string operation, bool hit, long durationMs = 0)
    {
        if (!_monitoringConfig.EnableMetrics) return;

        var tags = new TagList
        {
            { "operation", operation },
            { "hit", hit.ToString().ToLower() }
        };

        _cacheOperationsTotal.Add(1, tags);

        if (_monitoringConfig.EnableRequestLogging)
        {
            _logger.LogDebug("Cache operation: {Operation}, Hit: {Hit}, {Duration}ms",
                operation, hit, durationMs);
        }
    }

    /// <summary>
    /// Record error metrics
    /// </summary>
    public void RecordError(string errorType, string source, Exception? exception = null)
    {
        if (!_monitoringConfig.EnableMetrics) return;

        var tags = new TagList
        {
            { "error_type", errorType },
            { "source", source },
            { "exception_type", exception?.GetType().Name ?? "Unknown" }
        };

        _errorsTotal.Add(1, tags);

        if (_monitoringConfig.EnableErrorLogging)
        {
            _logger.LogError(exception, "Error recorded: {ErrorType} from {Source}", errorType, source);
        }
    }

    /// <summary>
    /// Record HTTP request metrics
    /// </summary>
    public void RecordRequestDuration(string method, string endpoint, int statusCode, long durationMs)
    {
        if (!_monitoringConfig.EnableMetrics) return;

        var tags = new TagList
        {
            { "method", method },
            { "endpoint", endpoint },
            { "status_code", statusCode.ToString() },
            { "status_class", GetStatusClass(statusCode) }
        };

        _requestDuration.Record(durationMs, tags);

        if (_monitoringConfig.EnableRequestLogging)
        {
            _logger.LogDebug("Request: {Method} {Endpoint}, {StatusCode}, {Duration}ms",
                method, endpoint, statusCode, durationMs);
        }
    }

    /// <summary>
    /// Set gauge value
    /// </summary>
    public void SetGaugeValue(string name, double value)
    {
        if (!_monitoringConfig.EnableMetrics) return;
        _gaugeValues.AddOrUpdate(name, value, (key, oldValue) => value);
    }

    /// <summary>
    /// Get current metrics snapshot
    /// </summary>
    public MonitoringSnapshot GetSnapshot()
    {
        return new MonitoringSnapshot
        {
            Timestamp = DateTimeOffset.UtcNow,
            MemoryUsageMB = GC.GetTotalMemory(false) / 1024.0 / 1024.0,
            ActiveConnections = _gaugeValues.GetValueOrDefault("active_connections", 0),
            CpuUsagePercent = _gaugeValues.GetValueOrDefault("cpu_usage", 0),
            TotalQueries = _gaugeValues.GetValueOrDefault("total_queries", 0),
            TotalAIOperations = _gaugeValues.GetValueOrDefault("total_ai_operations", 0),
            TotalCacheOperations = _gaugeValues.GetValueOrDefault("total_cache_operations", 0),
            TotalErrors = _gaugeValues.GetValueOrDefault("total_errors", 0),
            IsHealthy = _gaugeValues.GetValueOrDefault("total_errors", 0) < 10 // Simple health check
        };
    }

    #region IMetricsCollector Implementation

    /// <summary>
    /// Record query execution (IMetricsCollector interface)
    /// </summary>
    void IMetricsCollector.RecordQueryExecution(string queryType, long executionTimeMs, bool isSuccessful, int rowCount)
    {
        RecordQueryExecution(queryType, executionTimeMs, isSuccessful, rowCount);
    }

    /// <summary>
    /// Record histogram value (IMetricsCollector interface)
    /// </summary>
    public void RecordHistogram(string name, double value)
    {
        if (!_monitoringConfig.EnableMetrics) return;

        var histogram = _histograms.GetOrAdd(name, _ => _meter.CreateHistogram<double>(name));
        histogram.Record(value);
    }

    /// <summary>
    /// Record histogram value with tags (IMetricsCollector interface)
    /// </summary>
    public void RecordHistogram(string name, double value, TagList? tags = null)
    {
        if (!_monitoringConfig.EnableMetrics) return;

        var histogram = _histograms.GetOrAdd(name, _ => _meter.CreateHistogram<double>(name));
        histogram.Record(value, tags ?? new TagList());
    }

    /// <summary>
    /// Increment counter (IMetricsCollector interface)
    /// </summary>
    public void IncrementCounter(string name, TagList? tags = null)
    {
        if (!_monitoringConfig.EnableMetrics) return;

        var counter = _counters.GetOrAdd(name, _ => _meter.CreateCounter<long>(name));
        counter.Add(1, tags ?? new TagList());
    }

    /// <summary>
    /// Record execution time (IMetricsCollector interface)
    /// </summary>
    public void RecordExecutionTime(string operationName, TimeSpan duration)
    {
        RecordHistogram($"{operationName}_duration_ms", duration.TotalMilliseconds);
    }

    /// <summary>
    /// Record cache operation (IMetricsCollector interface)
    /// </summary>
    public void RecordCacheOperation(string cacheType, bool isHit)
    {
        RecordCacheOperation(cacheType, isHit, 0);
    }

    /// <summary>
    /// Record error (IMetricsCollector interface)
    /// </summary>
    void IMetricsCollector.RecordError(string errorType, string? details)
    {
        RecordError(errorType, details ?? "application", null);
    }

    /// <summary>
    /// Record error with exception (IMetricsCollector interface)
    /// </summary>
    void IMetricsCollector.RecordError(string errorType, string? details, Exception? exception)
    {
        RecordError(errorType, details ?? "application", exception);
    }

    /// <summary>
    /// Record user activity (IMetricsCollector interface)
    /// </summary>
    public void RecordUserActivity(string userId, string activityType)
    {
        if (!_monitoringConfig.EnableMetrics) return;

        var tags = new TagList
        {
            { "user_id", userId },
            { "activity_type", activityType }
        };

        IncrementCounter("user_activity_total", tags);
    }

    /// <summary>
    /// Get metrics snapshot (IMetricsCollector interface)
    /// </summary>
    public async Task<Dictionary<string, object>> GetMetricsSnapshotAsync()
    {
        await Task.CompletedTask;

        var snapshot = GetSnapshot();
        return new Dictionary<string, object>
        {
            ["timestamp"] = snapshot.Timestamp,
            ["memory_usage_mb"] = snapshot.MemoryUsageMB,
            ["active_connections"] = snapshot.ActiveConnections,
            ["cpu_usage_percent"] = snapshot.CpuUsagePercent,
            ["total_queries"] = snapshot.TotalQueries,
            ["total_ai_operations"] = snapshot.TotalAIOperations,
            ["total_cache_operations"] = snapshot.TotalCacheOperations,
            ["total_errors"] = snapshot.TotalErrors,
            ["is_healthy"] = snapshot.IsHealthy
        };
    }

    /// <summary>
    /// Record value (IMetricsCollector interface)
    /// </summary>
    public void RecordValue(string name, double value)
    {
        SetGaugeValue(name, value);
    }

    #endregion

    #endregion

    #region Distributed Tracing

    /// <summary>
    /// Start a new activity for distributed tracing
    /// </summary>
    public Activity? StartActivity(string name, ActivityKind kind = ActivityKind.Internal)
    {
        return _activitySource.StartActivity(name, kind);
    }

    /// <summary>
    /// Execute operation with tracing
    /// </summary>
    public async Task<T> TraceOperationAsync<T>(string operationName, Func<Activity?, Task<T>> operation)
    {
        using var activity = StartActivity(operationName);

        try
        {
            activity?.SetTag("operation.name", operationName);
            activity?.SetTag("operation.start_time", DateTimeOffset.UtcNow.ToString());

            var result = await operation(activity);

            activity?.SetTag("operation.success", "true");
            return result;
        }
        catch (Exception ex)
        {
            activity?.SetTag("operation.success", "false");
            activity?.SetTag("operation.error", ex.Message);
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);

            RecordError("operation_error", operationName, ex);
            throw;
        }
    }

    /// <summary>
    /// Execute operation with tracing (non-async)
    /// </summary>
    public T TraceOperation<T>(string operationName, Func<Activity?, T> operation)
    {
        using var activity = StartActivity(operationName);

        try
        {
            activity?.SetTag("operation.name", operationName);
            activity?.SetTag("operation.start_time", DateTimeOffset.UtcNow.ToString());

            var result = operation(activity);

            activity?.SetTag("operation.success", "true");
            return result;
        }
        catch (Exception ex)
        {
            activity?.SetTag("operation.success", "false");
            activity?.SetTag("operation.error", ex.Message);
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);

            RecordError("operation_error", operationName, ex);
            throw;
        }
    }

    #endregion

    #region Correlated Logging

    /// <summary>
    /// Log with correlation context
    /// </summary>
    public void LogWithCorrelation(LogLevel level, string message, params object[] args)
    {
        var correlationId = Activity.Current?.Id ?? Guid.NewGuid().ToString();

        using var scope = _logger.BeginScope(new Dictionary<string, object>
        {
            ["CorrelationId"] = correlationId,
            ["TraceId"] = Activity.Current?.TraceId.ToString() ?? "unknown",
            ["SpanId"] = Activity.Current?.SpanId.ToString() ?? "unknown"
        });

        _logger.Log(level, message, args);
    }

    /// <summary>
    /// Log information with correlation
    /// </summary>
    public void LogInformationWithCorrelation(string message, params object[] args)
    {
        LogWithCorrelation(LogLevel.Information, message, args);
    }

    /// <summary>
    /// Log error with correlation
    /// </summary>
    public void LogErrorWithCorrelation(Exception exception, string message, params object[] args)
    {
        var correlationId = Activity.Current?.Id ?? Guid.NewGuid().ToString();

        using var scope = _logger.BeginScope(new Dictionary<string, object>
        {
            ["CorrelationId"] = correlationId,
            ["TraceId"] = Activity.Current?.TraceId.ToString() ?? "unknown",
            ["SpanId"] = Activity.Current?.SpanId.ToString() ?? "unknown",
            ["ExceptionType"] = exception.GetType().Name,
            ["ExceptionMessage"] = exception.Message
        });

        _logger.LogError(exception, message, args);
        RecordError("logged_error", "application", exception);
    }

    #endregion

    #region Private Helper Methods

    private void InitializeStandardMetrics()
    {
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

        _requestDuration = _meter.CreateHistogram<double>(
            "request_duration_ms",
            "HTTP request duration in milliseconds");
    }

    private void CollectSystemMetrics(object? state)
    {
        if (!_monitoringConfig.EnablePerformanceCounters) return;

        try
        {
            using var process = Process.GetCurrentProcess();
            SetGaugeValue("thread_count", process.Threads.Count);
            SetGaugeValue("handle_count", process.HandleCount);
            SetGaugeValue("working_set_mb", process.WorkingSet64 / 1024.0 / 1024.0);
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

    #endregion

    public void Dispose()
    {
        _metricsTimer?.Dispose();
        _meter?.Dispose();
        _activitySource?.Dispose();
        _logger.LogInformation("Monitoring management service disposed");
    }
}

/// <summary>
/// Monitoring snapshot with comprehensive metrics
/// </summary>
public class MonitoringSnapshot
{
    public DateTimeOffset Timestamp { get; set; }
    public double MemoryUsageMB { get; set; }
    public double ActiveConnections { get; set; }
    public double CpuUsagePercent { get; set; }
    public double TotalQueries { get; set; }
    public double TotalAIOperations { get; set; }
    public double TotalCacheOperations { get; set; }
    public double TotalErrors { get; set; }
    public bool IsHealthy { get; set; }
}