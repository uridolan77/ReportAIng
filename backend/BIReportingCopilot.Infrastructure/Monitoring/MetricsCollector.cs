using BIReportingCopilot.Core.Interfaces;
using System.Diagnostics;

namespace BIReportingCopilot.Infrastructure.Monitoring;

/// <summary>
/// Implementation of metrics collector
/// </summary>
public class MetricsCollector : IMetricsCollector
{
    private readonly Dictionary<string, double> _metrics = new();
    private readonly object _lock = new();

    // Interface methods from Core.Interfaces.IMetricsCollector
    public void RecordQueryExecution(string queryType, long executionTimeMs, bool isSuccessful, int rowCount)
    {
        lock (_lock)
        {
            _metrics[$"query_executions_{queryType}"] = _metrics.GetValueOrDefault($"query_executions_{queryType}", 0) + 1;
            _metrics[$"query_duration_{queryType}"] = executionTimeMs;
            _metrics[$"query_success_{queryType}"] = isSuccessful ? 1 : 0;
            _metrics[$"query_rows_{queryType}"] = rowCount;
        }
    }

    public void RecordHistogram(string name, double value)
    {
        lock (_lock)
        {
            _metrics[name] = value;
        }
    }

    public void IncrementCounter(string name, TagList? tags = null)
    {
        lock (_lock)
        {
            var key = BuildMetricKey(name, tags);
            _metrics[key] = _metrics.GetValueOrDefault(key, 0) + 1;
        }
    }

    public void SetGaugeValue(string name, double value)
    {
        lock (_lock)
        {
            _metrics[name] = value;
        }
    }

    public void RecordExecutionTime(string operationName, TimeSpan duration)
    {
        lock (_lock)
        {
            _metrics[$"{operationName}_duration_ms"] = duration.TotalMilliseconds;
        }
    }

    public void RecordCacheOperation(string cacheType, bool isHit)
    {
        lock (_lock)
        {
            _metrics[$"cache_{cacheType}_operations"] = _metrics.GetValueOrDefault($"cache_{cacheType}_operations", 0) + 1;
            _metrics[$"cache_{cacheType}_hits"] = _metrics.GetValueOrDefault($"cache_{cacheType}_hits", 0) + (isHit ? 1 : 0);
        }
    }

    public void RecordError(string errorType, string? details = null)
    {
        lock (_lock)
        {
            _metrics[$"errors_{errorType}"] = _metrics.GetValueOrDefault($"errors_{errorType}", 0) + 1;
        }
    }

    public void RecordError(string errorType, string? details, Exception? exception)
    {
        RecordError(errorType, details);
    }

    public void RecordUserActivity(string userId, string activityType)
    {
        lock (_lock)
        {
            _metrics[$"user_activity_{activityType}"] = _metrics.GetValueOrDefault($"user_activity_{activityType}", 0) + 1;
        }
    }

    public async Task<Dictionary<string, object>> GetMetricsSnapshotAsync()
    {
        await Task.CompletedTask;
        lock (_lock)
        {
            return _metrics.ToDictionary(kvp => kvp.Key, kvp => (object)kvp.Value);
        }
    }

    public void RecordHistogram(string name, double value, TagList? tags = null)
    {
        lock (_lock)
        {
            var key = BuildMetricKey(name, tags);
            _metrics[key] = value;
        }
    }

    public void RecordValue(string name, double value)
    {
        lock (_lock)
        {
            _metrics[name] = value;
        }
    }

    // Additional helper methods
    public void RecordCounter(string name, double value = 1, Dictionary<string, string>? tags = null)
    {
        lock (_lock)
        {
            var key = BuildMetricKey(name, tags);
            _metrics[key] = _metrics.GetValueOrDefault(key, 0) + value;
        }
    }

    public void RecordGauge(string name, double value, Dictionary<string, string>? tags = null)
    {
        lock (_lock)
        {
            var key = BuildMetricKey(name, tags);
            _metrics[key] = value;
        }
    }

    public void RecordExecutionTime(string name, TimeSpan duration, Dictionary<string, string>? tags = null)
    {
        var key = BuildMetricKey($"{name}_duration_ms", tags);
        lock (_lock)
        {
            _metrics[key] = duration.TotalMilliseconds;
        }
    }

    public IDisposable StartTimer(string name, Dictionary<string, string>? tags = null)
    {
        return new MetricTimer(this, name, tags);
    }

    public Task<Dictionary<string, double>> GetCurrentMetricsAsync()
    {
        lock (_lock)
        {
            return Task.FromResult(new Dictionary<string, double>(_metrics));
        }
    }

    private string BuildMetricKey(string name, Dictionary<string, string>? tags)
    {
        if (tags == null || tags.Count == 0)
            return name;

        var tagString = string.Join(",", tags.Select(kvp => $"{kvp.Key}={kvp.Value}"));
        return $"{name}[{tagString}]";
    }

    private string BuildMetricKey(string name, TagList? tags)
    {
        if (tags == null || tags.Value.Count == 0)
            return name;

        var tagString = string.Join(",", tags.Value.Select(kvp => $"{kvp.Key}={kvp.Value}"));
        return $"{name}[{tagString}]";
    }

    private class MetricTimer : IDisposable
    {
        private readonly MetricsCollector _collector;
        private readonly string _name;
        private readonly Dictionary<string, string>? _tags;
        private readonly DateTime _startTime;

        public MetricTimer(MetricsCollector collector, string name, Dictionary<string, string>? tags)
        {
            _collector = collector;
            _name = name;
            _tags = tags;
            _startTime = DateTime.UtcNow;
        }

        public void Dispose()
        {
            var duration = DateTime.UtcNow - _startTime;
            _collector.RecordExecutionTime(_name, duration, _tags);
        }
    }
}
