using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace BIReportingCopilot.Infrastructure.Performance;

/// <summary>
/// Performance timing attribute for measuring method execution time
/// </summary>
public class PerformanceTimingService
{
    private readonly PerformanceOptimizer _performanceOptimizer;
    private readonly ILogger<PerformanceTimingService> _logger;

    public PerformanceTimingService(PerformanceOptimizer performanceOptimizer, ILogger<PerformanceTimingService> logger)
    {
        _performanceOptimizer = performanceOptimizer;
        _logger = logger;
    }

    /// <summary>
    /// Execute an operation with performance timing
    /// </summary>
    public async Task<T> ExecuteWithTimingAsync<T>(string operationName, Func<Task<T>> operation)
    {
        var stopwatch = Stopwatch.StartNew();
        try
        {
            var result = await operation();
            stopwatch.Stop();
            _performanceOptimizer.TrackExecutionTime(operationName, stopwatch.Elapsed);
            
            _logger.LogDebug("Operation {Operation} completed in {Duration:F2}ms", 
                operationName, stopwatch.Elapsed.TotalMilliseconds);
            
            return result;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _performanceOptimizer.TrackExecutionTime($"{operationName}_ERROR", stopwatch.Elapsed);
            _logger.LogError(ex, "Operation {Operation} failed after {Duration:F2}ms", 
                operationName, stopwatch.Elapsed.TotalMilliseconds);
            throw;
        }
    }

    /// <summary>
    /// Execute an operation with performance timing (void return)
    /// </summary>
    public async Task ExecuteWithTimingAsync(string operationName, Func<Task> operation)
    {
        var stopwatch = Stopwatch.StartNew();
        try
        {
            await operation();
            stopwatch.Stop();
            _performanceOptimizer.TrackExecutionTime(operationName, stopwatch.Elapsed);
            
            _logger.LogDebug("Operation {Operation} completed in {Duration:F2}ms", 
                operationName, stopwatch.Elapsed.TotalMilliseconds);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _performanceOptimizer.TrackExecutionTime($"{operationName}_ERROR", stopwatch.Elapsed);
            _logger.LogError(ex, "Operation {Operation} failed after {Duration:F2}ms", 
                operationName, stopwatch.Elapsed.TotalMilliseconds);
            throw;
        }
    }

    /// <summary>
    /// Execute a synchronous operation with performance timing
    /// </summary>
    public T ExecuteWithTiming<T>(string operationName, Func<T> operation)
    {
        var stopwatch = Stopwatch.StartNew();
        try
        {
            var result = operation();
            stopwatch.Stop();
            _performanceOptimizer.TrackExecutionTime(operationName, stopwatch.Elapsed);
            
            _logger.LogDebug("Operation {Operation} completed in {Duration:F2}ms", 
                operationName, stopwatch.Elapsed.TotalMilliseconds);
            
            return result;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _performanceOptimizer.TrackExecutionTime($"{operationName}_ERROR", stopwatch.Elapsed);
            _logger.LogError(ex, "Operation {Operation} failed after {Duration:F2}ms", 
                operationName, stopwatch.Elapsed.TotalMilliseconds);
            throw;
        }
    }
}
