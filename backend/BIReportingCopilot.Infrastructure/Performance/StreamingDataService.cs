using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using BIReportingCopilot.Core.Models.DTOs;
using BIReportingCopilot.Infrastructure.Data;
using System.Runtime.CompilerServices;

namespace BIReportingCopilot.Infrastructure.Performance;

/// <summary>
/// Service for streaming large datasets efficiently
/// </summary>
public class StreamingDataService
{
    private readonly BICopilotContext _context;
    private readonly ILogger<StreamingDataService> _logger;
    private const int DefaultBatchSize = 1000;

    public StreamingDataService(BICopilotContext context, ILogger<StreamingDataService> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Stream business tables in batches to avoid memory issues
    /// </summary>
    public async IAsyncEnumerable<BusinessTableInfoOptimizedDto> StreamBusinessTablesAsync(
        int batchSize = DefaultBatchSize,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var totalCount = await _context.BusinessTableInfo.CountAsync(t => t.IsActive, cancellationToken);
        var processedCount = 0;

        _logger.LogInformation("Starting to stream {TotalCount} business tables in batches of {BatchSize}",
            totalCount, batchSize);

        while (processedCount < totalCount)
        {
            var batch = await _context.BusinessTableInfo
                .Where(t => t.IsActive)
                .OrderBy(t => t.Id)
                .Skip(processedCount)
                .Take(batchSize)
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
            }

            processedCount += batch.Count;

            if (batch.Count < batchSize)
                break; // No more data

            // Small delay to prevent overwhelming the database
            await Task.Delay(10, cancellationToken);
        }

        _logger.LogInformation("Completed streaming {ProcessedCount} business tables", processedCount);
    }

    /// <summary>
    /// Stream query patterns with efficient pagination
    /// </summary>
    public async IAsyncEnumerable<QueryPatternOptimizedDto> StreamQueryPatternsAsync(
        int batchSize = DefaultBatchSize,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var totalCount = await _context.QueryPatterns.CountAsync(p => p.IsActive, cancellationToken);
        var processedCount = 0;

        _logger.LogInformation("Starting to stream {TotalCount} query patterns in batches of {BatchSize}",
            totalCount, batchSize);

        while (processedCount < totalCount)
        {
            var batch = await _context.QueryPatterns
                .Where(p => p.IsActive)
                .OrderBy(p => p.Id)
                .Skip(processedCount)
                .Take(batchSize)
                .Select(p => new QueryPatternOptimizedDto
                {
                    Id = p.Id,
                    PatternName = p.PatternName,
                    NaturalLanguagePattern = p.NaturalLanguagePattern,
                    Description = p.Description,
                    Priority = p.Priority,
                    UsageCount = p.UsageCount,
                    IsActive = p.IsActive,
                    CreatedDate = p.CreatedDate,
                    UpdatedDate = p.UpdatedDate
                })
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            foreach (var item in batch)
            {
                yield return item;
            }

            processedCount += batch.Count;

            if (batch.Count < batchSize)
                break;

            await Task.Delay(10, cancellationToken);
        }

        _logger.LogInformation("Completed streaming {ProcessedCount} query patterns", processedCount);
    }

    /// <summary>
    /// Stream large query results efficiently
    /// </summary>
    public async IAsyncEnumerable<T> StreamQueryResultsAsync<T>(
        IQueryable<T> query,
        int batchSize = DefaultBatchSize,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
        where T : class
    {
        var totalCount = await query.CountAsync(cancellationToken);
        var processedCount = 0;

        _logger.LogInformation("Starting to stream {TotalCount} query results in batches of {BatchSize}",
            totalCount, batchSize);

        while (processedCount < totalCount)
        {
            var batch = await query
                .Skip(processedCount)
                .Take(batchSize)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            foreach (var item in batch)
            {
                yield return item;
            }

            processedCount += batch.Count;

            if (batch.Count < batchSize)
                break;

            await Task.Delay(5, cancellationToken);
        }

        _logger.LogInformation("Completed streaming {ProcessedCount} query results", processedCount);
    }

    /// <summary>
    /// Get streaming data result with metadata
    /// </summary>
    public async Task<StreamingDataResult<T>> GetStreamingDataResultAsync<T>(
        IAsyncEnumerable<T> data,
        int? totalCount = null,
        int batchSize = DefaultBatchSize)
    {
        return new StreamingDataResult<T>
        {
            Data = data,
            TotalCount = totalCount,
            BatchSize = batchSize,
            HasMore = totalCount.HasValue && totalCount > batchSize
        };
    }

    /// <summary>
    /// Batch process data with performance tracking
    /// </summary>
    public async Task<BatchOperationResult<TResult>> BatchProcessAsync<TInput, TResult>(
        IEnumerable<TInput> items,
        Func<TInput, Task<TResult>> processor,
        int batchSize = DefaultBatchSize,
        CancellationToken cancellationToken = default)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var result = new BatchOperationResult<TResult>();
        var itemsList = items.ToList();

        _logger.LogInformation("Starting batch processing of {TotalItems} items in batches of {BatchSize}",
            itemsList.Count, batchSize);

        for (int i = 0; i < itemsList.Count; i += batchSize)
        {
            var batch = itemsList.Skip(i).Take(batchSize);
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

            // Check for cancellation
            cancellationToken.ThrowIfCancellationRequested();

            // Small delay between batches
            if (i + batchSize < itemsList.Count)
            {
                await Task.Delay(50, cancellationToken);
            }
        }

        stopwatch.Stop();
        result.ExecutionTime = stopwatch.Elapsed;

        _logger.LogInformation("Batch processing completed. Processed: {TotalProcessed}, Success: {SuccessCount}, Errors: {ErrorCount}, Time: {ExecutionTime}ms",
            result.TotalProcessed, result.SuccessCount, result.ErrorCount, result.ExecutionTime.TotalMilliseconds);

        return result;
    }
}
