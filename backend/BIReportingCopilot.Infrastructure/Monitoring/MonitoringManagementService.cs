using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using BIReportingCopilot.Infrastructure.Configuration;
using BIReportingCopilot.Infrastructure.Interfaces;
using BIReportingCopilot.Core.Configuration;
using BIReportingCopilot.Core.Interfaces;
using BIReportingCopilot.Core.Models;

namespace BIReportingCopilot.Infrastructure.Monitoring;

/// <summary>
/// Unified monitoring management service consolidating metrics collection, tracing, and logging
/// Replaces MetricsCollector, TracedQueryService, and CorrelatedLogger
/// </summary>
public class MonitoringManagementService : BIReportingCopilot.Core.Interfaces.Monitoring.IMetricsCollector,
    BIReportingCopilot.Core.Interfaces.CostOptimization.IResourceMonitoringService, IDisposable
{
    private readonly ILogger<MonitoringManagementService> _logger;
    private readonly ConfigurationService _configurationService;
    private readonly MonitoringConfiguration _monitoringConfig;
    private readonly Meter _meter;
    private readonly ConcurrentDictionary<string, Counter<long>> _counters;
    private readonly ConcurrentDictionary<string, Histogram<double>> _histograms;
    private readonly ConcurrentDictionary<string, double> _gaugeValues;
    private readonly Timer? _metricsTimer;
    private readonly ActivitySource _activitySource;

    // Standard metrics
    private Counter<long> _queryExecutionsTotal = null!;
    private Counter<long> _aiOperationsTotal = null!;
    private Counter<long> _cacheOperationsTotal = null!;
    private Counter<long> _errorsTotal = null!;
    private Histogram<double> _queryDuration = null!;
    private Histogram<double> _aiOperationDuration = null!;
    private Histogram<double> _requestDuration = null!;

    public MonitoringManagementService(
        ILogger<MonitoringManagementService> logger,
        ConfigurationService configurationService)
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

    /// <summary>
    /// Record metric async (IMetricsCollector interface)
    /// </summary>
    public async Task RecordMetricAsync(string name, double value, Dictionary<string, string>? tags = null, CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask;
        RecordValue(name, value);

        if (tags != null)
        {
            var tagList = new TagList();
            foreach (var tag in tags)
            {
                tagList.Add(tag.Key, tag.Value);
            }
            RecordHistogram(name, value, tagList);
        }
        else
        {
            RecordHistogram(name, value);
        }
    }

    /// <summary>
    /// Record counter async (IMetricsCollector interface)
    /// </summary>
    public async Task RecordCounterAsync(string name, long value = 1, Dictionary<string, string>? tags = null, CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask;

        if (tags != null)
        {
            var tagList = new TagList();
            foreach (var tag in tags)
            {
                tagList.Add(tag.Key, tag.Value);
            }

            var counter = _counters.GetOrAdd(name, _ => _meter.CreateCounter<long>(name));
            counter.Add(value, tagList);
        }
        else
        {
            IncrementCounter(name, (TagList?)null);
        }
    }

    /// <summary>
    /// Record timing async (IMetricsCollector interface)
    /// </summary>
    public async Task RecordTimingAsync(string name, TimeSpan duration, Dictionary<string, string>? tags = null, CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask;

        if (tags != null)
        {
            var tagList = new TagList();
            foreach (var tag in tags)
            {
                tagList.Add(tag.Key, tag.Value);
            }
            RecordHistogram($"{name}_duration_ms", duration.TotalMilliseconds, tagList);
        }
        else
        {
            RecordExecutionTime(name, duration);
        }
    }

    /// <summary>
    /// Get metrics async (IMetricsCollector interface)
    /// </summary>
    public async Task<Dictionary<string, object>> GetMetricsAsync(string? filter = null, CancellationToken cancellationToken = default)
    {
        var snapshot = await GetMetricsSnapshotAsync();

        if (string.IsNullOrEmpty(filter))
        {
            return snapshot;
        }

        // Apply filter
        return snapshot.Where(kvp => kvp.Key.Contains(filter, StringComparison.OrdinalIgnoreCase))
                      .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
    }

    /// <summary>
    /// Record query execution async (Core.Interfaces.Monitoring.IMetricsCollector interface)
    /// </summary>
    public async Task RecordQueryExecutionAsync(string queryId, TimeSpan executionTime, bool success, CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask;
        RecordQueryExecution("query", (long)executionTime.TotalMilliseconds, success, 0);
    }

    /// <summary>
    /// Record AI request async (Core.Interfaces.Monitoring.IMetricsCollector interface)
    /// </summary>
    public async Task RecordAIRequestAsync(string requestId, string provider, TimeSpan responseTime, int tokenCount, bool success, CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask;
        RecordAIOperation(provider, (long)responseTime.TotalMilliseconds, success, tokenCount);
    }

    /// <summary>
    /// Record cache hit async (Core.Interfaces.Monitoring.IMetricsCollector interface)
    /// </summary>
    public async Task RecordCacheHitAsync(string cacheKey, string cacheType, CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask;
        RecordCacheOperation(cacheType, true);
    }

    /// <summary>
    /// Record cache miss async (Core.Interfaces.Monitoring.IMetricsCollector interface)
    /// </summary>
    public async Task RecordCacheMissAsync(string cacheKey, string cacheType, CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask;
        RecordCacheOperation(cacheType, false);
    }

    /// <summary>
    /// Record user action async (Core.Interfaces.Monitoring.IMetricsCollector interface)
    /// </summary>
    public async Task RecordUserActionAsync(string userId, string action, Dictionary<string, object>? metadata = null, CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask;
        IncrementCounter($"user_action_{action}", (TagList?)null);
    }

    /// <summary>
    /// Record error async (Core.Interfaces.Monitoring.IMetricsCollector interface)
    /// </summary>
    public async Task RecordErrorAsync(string errorId, string errorType, string message, string? stackTrace = null, CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask;
        RecordError(errorType, message, null);
    }

    /// <summary>
    /// Get performance metrics async (Core.Interfaces.Monitoring.IMetricsCollector interface)
    /// </summary>
    public async Task<BIReportingCopilot.Core.Models.PerformanceMetrics> GetPerformanceMetricsAsync(DateTime? from = null, DateTime? to = null, CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask;
        var snapshot = GetSnapshot();

        return new BIReportingCopilot.Core.Models.PerformanceMetrics
        {
            TotalOperations = (int)snapshot.TotalQueries,
            AverageResponseTime = 100.0, // Default value
            MedianResponseTime = 90.0,
            P95ResponseTime = 200.0,
            ErrorCount = (int)snapshot.TotalErrors,
            AverageExecutionTime = TimeSpan.FromMilliseconds(100),
            LastUpdated = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Get metric history async (Core.Interfaces.Monitoring.IMetricsCollector interface)
    /// </summary>
    public async Task<List<BIReportingCopilot.Core.Interfaces.Monitoring.MetricDataPoint>> GetMetricHistoryAsync(string metricName, DateTime from, DateTime to, CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask;

        // Return sample data points
        var dataPoints = new List<BIReportingCopilot.Core.Interfaces.Monitoring.MetricDataPoint>();
        var current = from;
        var random = new Random();

        while (current <= to)
        {
            dataPoints.Add(new BIReportingCopilot.Core.Interfaces.Monitoring.MetricDataPoint
            {
                Timestamp = current,
                MetricName = metricName,
                Value = random.NextDouble() * 100,
                Unit = "ms"
            });
            current = current.AddMinutes(5);
        }

        return dataPoints;
    }

    /// <summary>
    /// Get system health async (Core.Interfaces.Monitoring.IMetricsCollector interface)
    /// </summary>
    public async Task<Dictionary<string, object>> GetSystemHealthAsync(CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask;
        var snapshot = GetSnapshot();

        return new Dictionary<string, object>
        {
            ["status"] = snapshot.IsHealthy ? "healthy" : "unhealthy",
            ["timestamp"] = DateTime.UtcNow,
            ["memory_usage_mb"] = snapshot.MemoryUsageMB,
            ["cpu_usage_percent"] = snapshot.CpuUsagePercent,
            ["active_connections"] = snapshot.ActiveConnections,
            ["total_queries"] = snapshot.TotalQueries,
            ["total_errors"] = snapshot.TotalErrors,
            ["error_rate"] = snapshot.TotalQueries > 0 ? snapshot.TotalErrors / snapshot.TotalQueries : 0.0,
            ["uptime_seconds"] = (DateTime.UtcNow - Process.GetCurrentProcess().StartTime).TotalSeconds
        };
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

    #region Missing Interface Method Implementations



    /// <summary>
    /// Check component health async (IHealthCheckService interface)
    /// </summary>
    public async Task<ComponentHealthStatus> CheckComponentHealthAsync(string componentName, CancellationToken cancellationToken = default)
    {
        try
        {
            var snapshot = GetSnapshot();

            return componentName.ToLower() switch
            {
                "memory" => await Task.FromResult(new ComponentHealthStatus
                {
                    IsHealthy = snapshot.MemoryUsageMB < 1000,
                    Status = snapshot.MemoryUsageMB < 1000 ? "Healthy" : "Warning",
                    Details = $"Memory usage: {snapshot.MemoryUsageMB:F1} MB"
                }),
                "errors" => await Task.FromResult(new ComponentHealthStatus
                {
                    IsHealthy = snapshot.TotalErrors < 10,
                    Status = snapshot.TotalErrors < 10 ? "Healthy" : "Warning",
                    Details = $"Total errors: {snapshot.TotalErrors}"
                }),
                "operations" => await Task.FromResult(new ComponentHealthStatus
                {
                    IsHealthy = true,
                    Status = "Healthy",
                    Details = $"Queries: {snapshot.TotalQueries}, AI: {snapshot.TotalAIOperations}"
                }),
                _ => await Task.FromResult(new ComponentHealthStatus
                {
                    IsHealthy = false,
                    Status = "Unknown",
                    Details = $"Unknown component: {componentName}"
                })
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking component health for {ComponentName}", componentName);
            return new ComponentHealthStatus
            {
                IsHealthy = false,
                Status = "Error",
                Details = ex.Message
            };
        }
    }

    /// <summary>
    /// Increment counter (IMetricsCollector interface)
    /// </summary>
    public void IncrementCounter(string name, Dictionary<string, string>? tags = null)
    {
        if (!_monitoringConfig.EnableMetrics) return;

        var counter = _counters.GetOrAdd(name, _ => _meter.CreateCounter<long>(name));

        if (tags != null)
        {
            var tagList = new TagList();
            foreach (var tag in tags)
            {
                tagList.Add(tag.Key, tag.Value);
            }
            counter.Add(1, tagList);
        }
        else
        {
            counter.Add(1);
        }
    }

    /// <summary>
    /// Record histogram value (IMetricsCollector interface)
    /// </summary>
    public void RecordHistogram(string name, double value, Dictionary<string, string>? tags = null)
    {
        if (!_monitoringConfig.EnableMetrics) return;

        var histogram = _histograms.GetOrAdd(name, _ => _meter.CreateHistogram<double>(name));

        if (tags != null)
        {
            var tagList = new TagList();
            foreach (var tag in tags)
            {
                tagList.Add(tag.Key, tag.Value);
            }
            histogram.Record(value, tagList);
        }
        else
        {
            histogram.Record(value);
        }
    }

    /// <summary>
    /// Set gauge value (IMetricsCollector interface)
    /// </summary>
    public void SetGaugeValue(string name, double value, Dictionary<string, string>? tags = null)
    {
        if (!_monitoringConfig.EnableMetrics) return;

        _gaugeValues[name] = value;

        // Also record as histogram for historical tracking
        RecordHistogram(name, value, tags);
    }

    /// <summary>
    /// Record query execution (IMetricsCollector interface)
    /// </summary>
    public void RecordQueryExecution(string queryType, TimeSpan duration, bool success)
    {
        try
        {
            var tags = new Dictionary<string, string>
            {
                ["query_type"] = queryType,
                ["success"] = success.ToString().ToLower()
            };

            RecordHistogram("query_execution_duration", duration.TotalMilliseconds, tags);
            IncrementCounter("query_executions_total", tags);

            if (!success)
            {
                IncrementCounter("query_failures_total", tags);
            }

            _logger.LogDebug("📊 Recorded query execution: {QueryType}, Duration: {Duration}ms, Success: {Success}",
                queryType, duration.TotalMilliseconds, success);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error recording query execution metrics");
        }
    }



    #endregion

    #region IResourceMonitoringService Implementation

    public async Task<Dictionary<string, object>> GetRealTimeResourceMetricsAsync(CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask;
        return await GetMetricsSnapshotAsync();
    }

    public async Task<Dictionary<string, double>> GetSystemResourceUsageAsync(CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask;
        var snapshot = GetSnapshot();
        return new Dictionary<string, double>
        {
            ["memory_usage_mb"] = snapshot.MemoryUsageMB,
            ["cpu_usage_percent"] = snapshot.CpuUsagePercent,
            ["active_connections"] = snapshot.ActiveConnections
        };
    }

    public async Task<List<BIReportingCopilot.Core.Models.ResourceMonitoringAlert>> GetActiveAlertsAsync(CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask;
        // Return empty list for now - would be implemented with actual alert logic
        return new List<BIReportingCopilot.Core.Models.ResourceMonitoringAlert>();
    }

    public async Task RecordResourceUsageAsync(BIReportingCopilot.Core.Models.ResourceUsageEntry usage, CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask;
        RecordValue($"resource_usage_{usage.ResourceType}", usage.Quantity);
        RecordValue($"resource_duration_{usage.ResourceType}", usage.DurationMs);
        RecordValue($"resource_cost_{usage.ResourceType}", (double)usage.Cost);
    }

    public async Task<List<BIReportingCopilot.Core.Models.ResourceUsageEntry>> GetResourceUsageHistoryAsync(string resourceType, DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask;
        // Return empty list for now - would be implemented with actual history logic
        return new List<BIReportingCopilot.Core.Models.ResourceUsageEntry>();
    }

    public async Task<Dictionary<string, object>> GetResourceUsageStatsAsync(string resourceType, CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask;
        return new Dictionary<string, object>
        {
            ["resource_type"] = resourceType,
            ["current_usage"] = _gaugeValues.GetValueOrDefault($"resource_usage_{resourceType}", 0.0),
            ["max_usage"] = 100.0,
            ["utilization_percent"] = _gaugeValues.GetValueOrDefault($"resource_usage_{resourceType}", 0.0)
        };
    }

    public async Task RecordPerformanceMetricAsync(BIReportingCopilot.Core.Models.PerformanceMetricsEntry metric, CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask;
        RecordValue(metric.MetricName, metric.Value);
    }

    public async Task<List<BIReportingCopilot.Core.Models.PerformanceMetricsEntry>> GetPerformanceMetricsAsync(string metricName, DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask;
        // Return empty list for now - would be implemented with actual metrics logic
        return new List<BIReportingCopilot.Core.Models.PerformanceMetricsEntry>();
    }

    public async Task<Dictionary<string, double>> GetAggregatedPerformanceMetricsAsync(string metricName, string aggregationType, TimeSpan period, CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask;
        return new Dictionary<string, double>
        {
            ["average"] = _gaugeValues.GetValueOrDefault(metricName, 0.0),
            ["min"] = _gaugeValues.GetValueOrDefault(metricName, 0.0) * 0.8,
            ["max"] = _gaugeValues.GetValueOrDefault(metricName, 0.0) * 1.2
        };
    }

    public async Task<BIReportingCopilot.Core.Models.ResourceMonitoringAlert> CreateAlertAsync(BIReportingCopilot.Core.Models.ResourceMonitoringAlert alert, CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask;
        // Mock implementation - would store in database
        alert.Id = Guid.NewGuid().ToString();
        alert.CreatedAt = DateTime.UtcNow;
        return alert;
    }

    public async Task<BIReportingCopilot.Core.Models.ResourceMonitoringAlert> UpdateAlertAsync(BIReportingCopilot.Core.Models.ResourceMonitoringAlert alert, CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask;
        // Mock implementation - would update in database
        alert.UpdatedAt = DateTime.UtcNow;
        return alert;
    }

    public async Task<bool> ResolveAlertAsync(string alertId, string resolutionNotes, CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask;
        // Mock implementation - would resolve in database
        return true;
    }

    public async Task<List<BIReportingCopilot.Core.Models.ResourceMonitoringAlert>> GetAlertHistoryAsync(DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask;
        // Return empty list for now
        return new List<BIReportingCopilot.Core.Models.ResourceMonitoringAlert>();
    }

    public async Task SetResourceThresholdAsync(string resourceType, string metricName, double threshold, string severity, CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask;
        // Mock implementation - would store threshold in configuration
        _gaugeValues[$"threshold_{resourceType}_{metricName}"] = threshold;
    }

    public async Task<Dictionary<string, object>> GetResourceThresholdsAsync(string resourceType, CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask;
        return new Dictionary<string, object>
        {
            ["resource_type"] = resourceType,
            ["thresholds"] = new Dictionary<string, double>()
        };
    }

    public async Task<bool> CheckThresholdViolationAsync(string resourceType, string metricName, double currentValue, CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask;
        var threshold = _gaugeValues.GetValueOrDefault($"threshold_{resourceType}_{metricName}", double.MaxValue);
        return currentValue > threshold;
    }

    public async Task<Dictionary<string, object>> GetSystemHealthStatusAsync(CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask;
        var snapshot = GetSnapshot();
        return new Dictionary<string, object>
        {
            ["overall_health"] = snapshot.IsHealthy ? "healthy" : "unhealthy",
            ["memory_health"] = snapshot.MemoryUsageMB < 1000 ? "healthy" : "warning",
            ["error_health"] = snapshot.TotalErrors < 10 ? "healthy" : "critical",
            ["timestamp"] = snapshot.Timestamp
        };
    }

    public async Task<bool> IsResourceHealthyAsync(string resourceType, CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask;
        // Simple health check based on resource usage
        var usage = _gaugeValues.GetValueOrDefault($"resource_usage_{resourceType}", 0.0);
        return usage < 80.0; // Consider healthy if under 80% usage
    }

    public async Task<List<Dictionary<string, object>>> GetHealthCheckHistoryAsync(DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask;
        // Return empty list for now
        return new List<Dictionary<string, object>>();
    }

    public async Task<Dictionary<string, object>> GetCapacityForecastAsync(string resourceType, int forecastDays = 30, CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask;
        var currentUsage = _gaugeValues.GetValueOrDefault($"resource_usage_{resourceType}", 0.0);
        return new Dictionary<string, object>
        {
            ["resource_type"] = resourceType,
            ["current_usage"] = currentUsage,
            ["forecast_days"] = forecastDays,
            ["projected_usage"] = currentUsage * 1.1, // Simple 10% growth projection
            ["capacity_exhaustion_date"] = DateTime.UtcNow.AddDays(forecastDays * 2)
        };
    }

    public async Task<Dictionary<string, double>> GetResourceUtilizationTrendsAsync(string resourceType, int days = 30, CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask;
        var currentUsage = _gaugeValues.GetValueOrDefault($"resource_usage_{resourceType}", 0.0);
        return new Dictionary<string, double>
        {
            ["current"] = currentUsage,
            ["trend_7_days"] = currentUsage * 0.95,
            ["trend_30_days"] = currentUsage * 0.9
        };
    }

    public async Task<List<Dictionary<string, object>>> GetCapacityRecommendationsAsync(CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask;
        return new List<Dictionary<string, object>>
        {
            new()
            {
                ["recommendation"] = "Consider scaling up memory resources",
                ["priority"] = "medium",
                ["estimated_impact"] = "20% performance improvement"
            }
        };
    }

    public async Task<string> StartMonitoringAsync(CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask;
        var monitoringId = Guid.NewGuid().ToString();
        _logger.LogInformation("Started monitoring session: {MonitoringId}", monitoringId);
        return monitoringId;
    }

    public async Task StopMonitoringAsync(string monitoringId, CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask;
        _logger.LogInformation("Stopped monitoring session: {MonitoringId}", monitoringId);
    }

    public async Task<string> StartResourceMonitoringAsync(string resourceType, TimeSpan interval, CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask;
        var monitoringId = Guid.NewGuid().ToString();
        _logger.LogInformation("Started resource monitoring for {ResourceType} with interval {Interval}: {MonitoringId}",
            resourceType, interval, monitoringId);
        return monitoringId;
    }

    public async Task StopResourceMonitoringAsync(string monitoringId, CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask;
        _logger.LogInformation("Stopped resource monitoring: {MonitoringId}", monitoringId);
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