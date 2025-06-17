using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace BIReportingCopilot.Infrastructure.Performance;

/// <summary>
/// Performance monitoring and optimization service
/// </summary>
public class PerformanceOptimizer
{
    private readonly IMemoryCache _cache;
    private readonly ILogger<PerformanceOptimizer> _logger;
    private readonly Dictionary<string, DateTime> _lastExecutionTimes = new();
    private readonly Dictionary<string, TimeSpan> _averageExecutionTimes = new();

    public PerformanceOptimizer(IMemoryCache cache, ILogger<PerformanceOptimizer> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    /// <summary>
    /// Track execution time for performance analysis
    /// </summary>
    public void TrackExecutionTime(string operationName, TimeSpan executionTime)
    {
        _lastExecutionTimes[operationName] = DateTime.UtcNow;
        
        if (_averageExecutionTimes.ContainsKey(operationName))
        {
            // Simple moving average (can be enhanced with exponential moving average)
            _averageExecutionTimes[operationName] = TimeSpan.FromMilliseconds(
                (_averageExecutionTimes[operationName].TotalMilliseconds + executionTime.TotalMilliseconds) / 2);
        }
        else
        {
            _averageExecutionTimes[operationName] = executionTime;
        }

        // Log slow operations
        if (executionTime.TotalSeconds > 5)
        {
            _logger.LogWarning("Slow operation detected: {Operation} took {Duration:F2} seconds", 
                operationName, executionTime.TotalSeconds);
        }
    }

    /// <summary>
    /// Get performance metrics for an operation
    /// </summary>
    public PerformanceMetrics GetPerformanceMetrics(string operationName)
    {
        return new PerformanceMetrics
        {
            OperationName = operationName,
            AverageExecutionTime = _averageExecutionTimes.GetValueOrDefault(operationName),
            LastExecutionTime = _lastExecutionTimes.GetValueOrDefault(operationName)
        };
    }

    /// <summary>
    /// Get all performance metrics
    /// </summary>
    public List<PerformanceMetrics> GetAllPerformanceMetrics()
    {
        return _averageExecutionTimes.Keys
            .Select(GetPerformanceMetrics)
            .OrderByDescending(m => m.AverageExecutionTime)
            .ToList();
    }

    /// <summary>
    /// Clear performance metrics
    /// </summary>
    public void ClearMetrics()
    {
        _lastExecutionTimes.Clear();
        _averageExecutionTimes.Clear();
        _logger.LogInformation("Performance metrics cleared");
    }

    /// <summary>
    /// Check if an operation should use cache based on performance history
    /// </summary>
    public bool ShouldUseCache(string operationName, TimeSpan cacheThreshold)
    {
        if (_averageExecutionTimes.TryGetValue(operationName, out var avgTime))
        {
            return avgTime >= cacheThreshold;
        }
        return false;
    }
}

/// <summary>
/// Performance metrics for an operation
/// </summary>
public class PerformanceMetrics
{
    public string OperationName { get; set; } = string.Empty;
    public TimeSpan AverageExecutionTime { get; set; }
    public DateTime LastExecutionTime { get; set; }
}
