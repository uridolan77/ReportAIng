using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using BIReportingCopilot.Core.Models.DTOs;
using BIReportingCopilot.Infrastructure.Data;
using BIReportingCopilot.Infrastructure.Configuration;
using BIReportingCopilot.Core.Configuration;
using System.Runtime.CompilerServices;
using System.Diagnostics;

namespace BIReportingCopilot.Infrastructure.Performance;

/// <summary>
/// Unified performance management service consolidating streaming, metrics, and monitoring
/// Replaces StreamingDataService, StreamingQueryService, MetricsCollector, and TracedQueryService
/// </summary>
public class PerformanceManagementService
{
    private readonly BICopilotContext _context;
    private readonly ConfigurationService _configurationService;
    private readonly ILogger<PerformanceManagementService> _logger;
    private readonly PerformanceConfiguration _performanceConfig;
    private readonly Dictionary<string, PerformanceMetrics> _metricsCache;
    private readonly object _metricsLock = new();

    public PerformanceManagementService(
        BICopilotContext context,
        ConfigurationService configurationService,
        ILogger<PerformanceManagementService> logger)
    {
        _context = context;
        _configurationService = configurationService;
        _logger = logger;
        _performanceConfig = configurationService.GetPerformanceSettings();
        _metricsCache = new Dictionary<string, PerformanceMetrics>();
    }

    #region Streaming Operations

    /// <summary>
    /// Stream business tables in batches with performance tracking
    /// </summary>
    public async IAsyncEnumerable<BusinessTableInfoOptimizedDto> StreamBusinessTablesAsync(
        int? batchSize = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var effectiveBatchSize = batchSize ?? _performanceConfig.StreamingBatchSize;
        var stopwatch = Stopwatch.StartNew();
        var totalCount = 0;

        try
        {
            var totalRecords = await _context.BusinessTableInfo.CountAsync(t => t.IsActive, cancellationToken);
            var processedCount = 0;

            _logger.LogInformation("Starting to stream {TotalCount} business tables in batches of {BatchSize}",
                totalRecords, effectiveBatchSize);

            while (processedCount < totalRecords)
            {
                var batch = await _context.BusinessTableInfo
                    .Where(t => t.IsActive)
                    .OrderBy(t => t.Id)
                    .Skip(processedCount)
                    .Take(effectiveBatchSize)
                    .Select(t => new BusinessTableInfoOptimizedDto
                    {
                        Id = t.Id,
                        TableName = t.TableName,
                        SchemaName = t.SchemaName,
                        BusinessPurpose = t.BusinessPurpose,
                        BusinessContext = t.BusinessContext,
                        IsActive = t.IsActive,
                        UpdatedDate = t.UpdatedDate,
                        UpdatedBy = t.UpdatedBy,
                        ColumnCount = t.Columns.Count(c => c.IsActive),
                        CreatedDate = t.CreatedDate
                    })
                    .AsNoTracking()
                    .ToListAsync(cancellationToken);

                foreach (var item in batch)
                {
                    yield return item;
                    totalCount++;
                }

                processedCount += batch.Count;

                if (batch.Count < effectiveBatchSize)
                    break;

                // Configurable delay between batches
                await Task.Delay(_performanceConfig.StreamingDelayMs, cancellationToken);
            }

            // Record streaming metrics
            RecordStreamingMetrics("BusinessTables", totalCount, stopwatch.Elapsed);
        }
        finally
        {
            stopwatch.Stop();
            _logger.LogInformation("Completed streaming {TotalCount} business tables in {ElapsedMs}ms",
                totalCount, stopwatch.ElapsedMilliseconds);
        }
    }

    /// <summary>
    /// Stream query results with performance optimization
    /// </summary>
    public async IAsyncEnumerable<T> StreamQueryResultsAsync<T>(
        IQueryable<T> query,
        int? batchSize = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
        where T : class
    {
        var effectiveBatchSize = batchSize ?? _performanceConfig.StreamingBatchSize;
        var stopwatch = Stopwatch.StartNew();
        var totalCount = 0;

        try
        {
            var totalRecords = await query.CountAsync(cancellationToken);
            var processedCount = 0;

            _logger.LogInformation("Starting to stream {TotalCount} query results in batches of {BatchSize}",
                totalRecords, effectiveBatchSize);

            while (processedCount < totalRecords)
            {
                var batch = await query
                    .Skip(processedCount)
                    .Take(effectiveBatchSize)
                    .AsNoTracking()
                    .ToListAsync(cancellationToken);

                foreach (var item in batch)
                {
                    yield return item;
                    totalCount++;
                }

                processedCount += batch.Count;

                if (batch.Count < effectiveBatchSize)
                    break;

                await Task.Delay(_performanceConfig.StreamingDelayMs, cancellationToken);
            }

            // Record streaming metrics
            RecordStreamingMetrics(typeof(T).Name, totalCount, stopwatch.Elapsed);
        }
        finally
        {
            stopwatch.Stop();
            _logger.LogInformation("Completed streaming {TotalCount} {TypeName} results in {ElapsedMs}ms",
                totalCount, typeof(T).Name, stopwatch.ElapsedMilliseconds);
        }
    }

    /// <summary>
    /// Batch process data with comprehensive performance tracking
    /// </summary>
    public async Task<BatchOperationResult<TResult>> BatchProcessAsync<TInput, TResult>(
        IEnumerable<TInput> items,
        Func<TInput, Task<TResult>> processor,
        int? batchSize = null,
        CancellationToken cancellationToken = default)
    {
        var effectiveBatchSize = batchSize ?? _performanceConfig.StreamingBatchSize;
        var stopwatch = Stopwatch.StartNew();
        var result = new BatchOperationResult<TResult>();
        var itemsList = items.ToList();

        _logger.LogInformation("Starting batch processing of {TotalItems} items in batches of {BatchSize}",
            itemsList.Count, effectiveBatchSize);

        try
        {
            for (int i = 0; i < itemsList.Count; i += effectiveBatchSize)
            {
                var batch = itemsList.Skip(i).Take(effectiveBatchSize);
                var batchTasks = batch.Select(async item =>
                {
                    try
                    {
                        var processedItem = await processor(item);
                        return new { Success = true, Result = processedItem, Error = (string?)null };
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing item in batch");
                        return new { Success = false, Result = default(TResult)!, Error = ex.Message };
                    }
                });

                var batchResults = await Task.WhenAll(batchTasks);

                foreach (var batchResult in batchResults)
                {
                    result.TotalProcessed++;

                    if (batchResult.Success)
                    {
                        result.SuccessCount++;
                        result.Results.Add(batchResult.Result);
                    }
                    else
                    {
                        result.ErrorCount++;
                        result.Errors.Add(batchResult.Error!);
                    }
                }

                cancellationToken.ThrowIfCancellationRequested();

                if (i + effectiveBatchSize < itemsList.Count)
                {
                    await Task.Delay(_performanceConfig.StreamingDelayMs, cancellationToken);
                }
            }

            result.ExecutionTime = stopwatch.Elapsed;

            // Record batch processing metrics
            RecordBatchProcessingMetrics(typeof(TInput).Name, result);

            return result;
        }
        finally
        {
            stopwatch.Stop();
            _logger.LogInformation("Batch processing completed. Processed: {TotalProcessed}, Success: {SuccessCount}, Errors: {ErrorCount}, Time: {ExecutionTime}ms",
                result.TotalProcessed, result.SuccessCount, result.ErrorCount, stopwatch.ElapsedMilliseconds);
        }
    }

    #endregion

    #region Performance Metrics

    /// <summary>
    /// Record query execution metrics
    /// </summary>
    public void RecordQueryMetrics(string queryType, TimeSpan executionTime, int resultCount, bool success)
    {
        lock (_metricsLock)
        {
            var key = $"query_{queryType}";
            if (!_metricsCache.TryGetValue(key, out var metrics))
            {
                metrics = new PerformanceMetrics { OperationType = queryType };
                _metricsCache[key] = metrics;
            }

            metrics.TotalOperations++;
            metrics.TotalExecutionTime += executionTime;
            metrics.AverageExecutionTime = TimeSpan.FromMilliseconds(
                metrics.TotalExecutionTime.TotalMilliseconds / metrics.TotalOperations);

            if (success)
            {
                metrics.SuccessCount++;
                metrics.TotalResultCount += resultCount;
            }
            else
            {
                metrics.ErrorCount++;
            }

            metrics.LastUpdated = DateTime.UtcNow;
        }

        _logger.LogDebug("Recorded query metrics for {QueryType}: {ExecutionTime}ms, {ResultCount} results, Success: {Success}",
            queryType, executionTime.TotalMilliseconds, resultCount, success);
    }

    /// <summary>
    /// Get performance metrics summary
    /// </summary>
    public PerformanceMetricsSummary GetPerformanceMetrics()
    {
        lock (_metricsLock)
        {
            var summary = new PerformanceMetricsSummary
            {
                GeneratedAt = DateTime.UtcNow,
                TotalOperations = _metricsCache.Values.Sum(m => m.TotalOperations),
                TotalSuccessCount = _metricsCache.Values.Sum(m => m.SuccessCount),
                TotalErrorCount = _metricsCache.Values.Sum(m => m.ErrorCount),
                AverageExecutionTime = _metricsCache.Values.Any()
                    ? TimeSpan.FromMilliseconds(_metricsCache.Values.Average(m => m.AverageExecutionTime.TotalMilliseconds))
                    : TimeSpan.Zero,
                MetricsByType = _metricsCache.ToDictionary(kvp => kvp.Key, kvp => kvp.Value)
            };

            summary.SuccessRate = summary.TotalOperations > 0
                ? (double)summary.TotalSuccessCount / summary.TotalOperations
                : 0;

            return summary;
        }
    }

    /// <summary>
    /// Reset performance metrics
    /// </summary>
    public void ResetMetrics()
    {
        lock (_metricsLock)
        {
            _metricsCache.Clear();
            _logger.LogInformation("Performance metrics cache cleared");
        }
    }

    /// <summary>
    /// Get performance metrics for a date range
    /// </summary>
    public async Task<List<PerformanceMetrics>> GetPerformanceMetricsAsync(DateTime from, DateTime to, string? category = null)
    {
        await Task.CompletedTask; // For async signature compatibility

        lock (_metricsLock)
        {
            var metrics = _metricsCache.Values
                .Where(m => m.LastUpdated >= from && m.LastUpdated <= to)
                .Where(m => category == null || m.OperationType.Contains(category, StringComparison.OrdinalIgnoreCase))
                .ToList();

            return metrics;
        }
    }

    /// <summary>
    /// Get current performance snapshot
    /// </summary>
    public async Task<PerformanceMetricsSummary> GetCurrentPerformanceSnapshotAsync()
    {
        await Task.CompletedTask; // For async signature compatibility
        return GetPerformanceMetrics();
    }

    /// <summary>
    /// Get query performance breakdown
    /// </summary>
    public async Task<List<QueryPerformanceMetrics>> GetQueryPerformanceBreakdownAsync()
    {
        await Task.CompletedTask; // For async signature compatibility

        lock (_metricsLock)
        {
            var queryMetrics = _metricsCache.Values
                .Where(m => m.OperationType.StartsWith("query_"))
                .Select(m => new QueryPerformanceMetrics
                {
                    QueryType = m.OperationType.Replace("query_", ""),
                    AverageExecutionTime = m.AverageExecutionTime,
                    TotalExecutions = m.TotalOperations,
                    SuccessRate = m.TotalOperations > 0 ? (double)m.SuccessCount / m.TotalOperations * 100 : 100,
                    P95ExecutionTime = m.AverageExecutionTime.Add(TimeSpan.FromMilliseconds(m.AverageExecutionTime.TotalMilliseconds * 0.2)), // Approximation
                    P99ExecutionTime = m.AverageExecutionTime.Add(TimeSpan.FromMilliseconds(m.AverageExecutionTime.TotalMilliseconds * 0.5))  // Approximation
                })
                .ToList();

            return queryMetrics;
        }
    }

    #endregion

    #region Private Helper Methods

    private void RecordStreamingMetrics(string dataType, int totalCount, TimeSpan executionTime)
    {
        lock (_metricsLock)
        {
            var key = $"streaming_{dataType}";
            if (!_metricsCache.TryGetValue(key, out var metrics))
            {
                metrics = new PerformanceMetrics { OperationType = $"Streaming_{dataType}" };
                _metricsCache[key] = metrics;
            }

            metrics.TotalOperations++;
            metrics.TotalExecutionTime += executionTime;
            metrics.TotalResultCount += totalCount;
            metrics.AverageExecutionTime = TimeSpan.FromMilliseconds(
                metrics.TotalExecutionTime.TotalMilliseconds / metrics.TotalOperations);
            metrics.LastUpdated = DateTime.UtcNow;
        }
    }

    private void RecordBatchProcessingMetrics<TResult>(string inputType, BatchOperationResult<TResult> result)
    {
        lock (_metricsLock)
        {
            var key = $"batch_{inputType}";
            if (!_metricsCache.TryGetValue(key, out var metrics))
            {
                metrics = new PerformanceMetrics { OperationType = $"Batch_{inputType}" };
                _metricsCache[key] = metrics;
            }

            metrics.TotalOperations++;
            metrics.TotalExecutionTime += result.ExecutionTime;
            metrics.SuccessCount += result.SuccessCount;
            metrics.ErrorCount += result.ErrorCount;
            metrics.AverageExecutionTime = TimeSpan.FromMilliseconds(
                metrics.TotalExecutionTime.TotalMilliseconds / metrics.TotalOperations);
            metrics.LastUpdated = DateTime.UtcNow;
        }
    }

    #endregion
}

/// <summary>
/// Performance metrics for tracking operations
/// </summary>
public class PerformanceMetrics
{
    public string OperationType { get; set; } = string.Empty;
    public int TotalOperations { get; set; }
    public int SuccessCount { get; set; }
    public int ErrorCount { get; set; }
    public TimeSpan TotalExecutionTime { get; set; }
    public TimeSpan AverageExecutionTime { get; set; }
    public int TotalResultCount { get; set; }
    public DateTime LastUpdated { get; set; }
}

/// <summary>
/// Performance metrics summary
/// </summary>
public class PerformanceMetricsSummary
{
    public DateTime GeneratedAt { get; set; }
    public int TotalOperations { get; set; }
    public int TotalSuccessCount { get; set; }
    public int TotalErrorCount { get; set; }
    public double SuccessRate { get; set; }
    public TimeSpan AverageExecutionTime { get; set; }
    public Dictionary<string, PerformanceMetrics> MetricsByType { get; set; } = new();
}

/// <summary>
/// Batch operation result with performance tracking
/// </summary>
public class BatchOperationResult<T>
{
    public int TotalProcessed { get; set; }
    public int SuccessCount { get; set; }
    public int ErrorCount { get; set; }
    public TimeSpan ExecutionTime { get; set; }
    public List<T> Results { get; set; } = new();
    public List<string> Errors { get; set; } = new();
    public double SuccessRate => TotalProcessed > 0 ? (double)SuccessCount / TotalProcessed : 0;
}

/// <summary>
/// Streaming data result with metadata
/// </summary>
public class StreamingDataResult<T>
{
    public IAsyncEnumerable<T> Data { get; set; } = null!;
    public int? TotalCount { get; set; }
    public int BatchSize { get; set; }
    public bool HasMore { get; set; }
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Query performance metrics
/// </summary>
public class QueryPerformanceMetrics
{
    public string QueryType { get; set; } = string.Empty;
    public TimeSpan AverageExecutionTime { get; set; }
    public int TotalExecutions { get; set; }
    public double SuccessRate { get; set; }
    public TimeSpan P95ExecutionTime { get; set; }
    public TimeSpan P99ExecutionTime { get; set; }
}
