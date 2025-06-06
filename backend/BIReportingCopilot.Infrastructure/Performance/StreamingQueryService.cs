using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using System.Data;
using System.Text.Json;
using System.Threading.Channels;
using BIReportingCopilot.Core.Models;
using BIReportingCopilot.Core.Models.DTOs;
using BIReportingCopilot.Core.Interfaces;

namespace BIReportingCopilot.Infrastructure.Performance;

public interface IStreamingSqlQueryService : ISqlQueryService
{
    IAsyncEnumerable<Dictionary<string, object>> ExecuteSelectQueryStreamingAsync(
        string sql,
        QueryOptions? options = null,
        CancellationToken cancellationToken = default);

    IAsyncEnumerable<StreamingQueryChunk> ExecuteSelectQueryChunkedAsync(
        string sql,
        QueryOptions? options = null,
        CancellationToken cancellationToken = default);

    Task<StreamingQueryMetadata> GetStreamingQueryMetadataAsync(
        string sql,
        QueryOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Advanced streaming with backpressure control for very large datasets
    /// </summary>
    IAsyncEnumerable<StreamingQueryChunk> ExecuteSelectQueryWithBackpressureAsync(
        string sql,
        QueryOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Stream query results with real-time progress reporting
    /// </summary>
    IAsyncEnumerable<StreamingProgressUpdate> ExecuteSelectQueryWithProgressAsync(
        string sql,
        QueryOptions? options = null,
        CancellationToken cancellationToken = default);
}

public class StreamingQueryChunk
{
    public int ChunkIndex { get; set; }
    public List<Dictionary<string, object>> Data { get; set; } = new();
    public int TotalRowsInChunk { get; set; }
    public bool IsLastChunk { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public int ProcessingTimeMs { get; set; }
}

public class StreamingQueryMetadata
{
    public ColumnMetadata[] Columns { get; set; } = Array.Empty<ColumnMetadata>();
    public long EstimatedRowCount { get; set; }
    public int ChunkSize { get; set; }
    public int EstimatedChunks { get; set; }
    public bool SupportsStreaming { get; set; }
    public string QueryComplexity { get; set; } = "Unknown";
}

public class StreamingProgressUpdate
{
    public int RowsProcessed { get; set; }
    public long EstimatedTotalRows { get; set; }
    public double ProgressPercentage { get; set; }
    public TimeSpan ElapsedTime { get; set; }
    public TimeSpan EstimatedTimeRemaining { get; set; }
    public double RowsPerSecond { get; set; }
    public string Status { get; set; } = "Processing";
    public Dictionary<string, object>? CurrentRow { get; set; }
    public bool IsCompleted { get; set; }
    public string? ErrorMessage { get; set; }
}

public class StreamingQueryResult : IAsyncEnumerable<Dictionary<string, object>>
{
    private readonly string _connectionString;
    private readonly string _sql;
    private readonly QueryOptions _options;
    private readonly ILogger? _logger;

    public StreamingQueryResult(string connectionString, string sql, QueryOptions options, ILogger? logger = null)
    {
        _connectionString = connectionString;
        _sql = sql;
        _options = options;
        _logger = logger;
    }

    public async IAsyncEnumerator<Dictionary<string, object>> GetAsyncEnumerator(
        CancellationToken cancellationToken = default)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        using var command = new SqlCommand(_sql, connection)
        {
            CommandTimeout = _options.TimeoutSeconds
        };

        using var reader = await command.ExecuteReaderAsync(
            CommandBehavior.SequentialAccess,
            cancellationToken);

        var columns = Enumerable.Range(0, reader.FieldCount)
            .Select(i => reader.GetName(i))
            .ToArray();

        var rowCount = 0;
        var startTime = DateTime.UtcNow;

        while (await reader.ReadAsync(cancellationToken))
        {
            if (rowCount >= _options.MaxRows)
            {
                _logger?.LogInformation("Streaming query reached max rows limit: {MaxRows}", _options.MaxRows);
                yield break;
            }

            var row = new Dictionary<string, object>();
            for (var i = 0; i < columns.Length; i++)
            {
                row[columns[i]] = reader.IsDBNull(i) ? null : reader.GetValue(i);
            }

            yield return row;
            rowCount++;

            // Log progress every 1000 rows
            if (rowCount % 1000 == 0)
            {
                var elapsed = (DateTime.UtcNow - startTime).TotalSeconds;
                _logger?.LogDebug("Streaming query progress: {RowCount} rows in {Elapsed:F2}s", rowCount, elapsed);
            }
        }

        var totalElapsed = (DateTime.UtcNow - startTime).TotalSeconds;
        _logger?.LogInformation("Streaming query completed: {RowCount} rows in {Elapsed:F2}s", rowCount, totalElapsed);
    }
}

public class StreamingSqlQueryService : IStreamingSqlQueryService
{
    private readonly ISqlQueryService _innerService;
    private readonly string _connectionString;
    private readonly ILogger<StreamingSqlQueryService> _logger;

    public StreamingSqlQueryService(
        ISqlQueryService innerService,
        string connectionString,
        ILogger<StreamingSqlQueryService> logger)
    {
        _innerService = innerService;
        _connectionString = connectionString;
        _logger = logger;
    }

    public IAsyncEnumerable<Dictionary<string, object>> ExecuteSelectQueryStreamingAsync(
        string sql,
        QueryOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        options ??= new QueryOptions();
        return new StreamingQueryResult(_connectionString, sql, options, _logger);
    }

    public async IAsyncEnumerable<StreamingQueryChunk> ExecuteSelectQueryChunkedAsync(
        string sql,
        QueryOptions? options = null,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        options ??= new QueryOptions();
        var chunkSize = options.ChunkSize ?? 1000;
        var chunkIndex = 0;
        var chunk = new List<Dictionary<string, object>>();
        var chunkStartTime = DateTime.UtcNow;

        await foreach (var row in ExecuteSelectQueryStreamingAsync(sql, options, cancellationToken))
        {
            chunk.Add(row);

            if (chunk.Count >= chunkSize)
            {
                var processingTime = (int)(DateTime.UtcNow - chunkStartTime).TotalMilliseconds;
                yield return new StreamingQueryChunk
                {
                    ChunkIndex = chunkIndex++,
                    Data = new List<Dictionary<string, object>>(chunk),
                    TotalRowsInChunk = chunk.Count,
                    IsLastChunk = false,
                    ProcessingTimeMs = processingTime
                };

                chunk.Clear();
                chunkStartTime = DateTime.UtcNow;
            }
        }

        // Return final chunk if it has data
        if (chunk.Count > 0)
        {
            var processingTime = (int)(DateTime.UtcNow - chunkStartTime).TotalMilliseconds;
            yield return new StreamingQueryChunk
            {
                ChunkIndex = chunkIndex,
                Data = chunk,
                TotalRowsInChunk = chunk.Count,
                IsLastChunk = true,
                ProcessingTimeMs = processingTime
            };
        }
    }

    public async Task<StreamingQueryMetadata> GetStreamingQueryMetadataAsync(
        string sql,
        QueryOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Get column information by executing a limited query
            var limitedSql = $"SELECT TOP 1 * FROM ({sql}) AS SubQuery";
            var sampleResult = await _innerService.ExecuteSelectQueryAsync(limitedSql, options, cancellationToken);

            // Estimate row count (this is a simplified approach)
            var estimatedRowCount = await EstimateRowCountAsync(sql, cancellationToken);
            var chunkSize = options?.ChunkSize ?? 1000;

            return new StreamingQueryMetadata
            {
                Columns = sampleResult.Metadata.Columns,
                EstimatedRowCount = estimatedRowCount,
                ChunkSize = chunkSize,
                EstimatedChunks = (int)Math.Ceiling((double)estimatedRowCount / chunkSize),
                SupportsStreaming = true,
                QueryComplexity = DetermineQueryComplexity(sql)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting streaming query metadata for SQL: {Sql}", sql);
            return new StreamingQueryMetadata
            {
                SupportsStreaming = false,
                QueryComplexity = "Error"
            };
        }
    }

    private async Task<long> EstimateRowCountAsync(string sql, CancellationToken cancellationToken)
    {
        try
        {
            // Simple row count estimation - replace SELECT with COUNT(*)
            var countSql = $"SELECT COUNT(*) FROM ({sql}) AS CountQuery";
            var countResult = await _innerService.ExecuteSelectQueryAsync(countSql, new QueryOptions { TimeoutSeconds = 30 }, cancellationToken);

            if (countResult.IsSuccessful && countResult.Data?.Length > 0)
            {
                var firstRow = countResult.Data[0] as Dictionary<string, object>;
                if (firstRow?.Values.FirstOrDefault() is long count)
                    return count;
                if (firstRow?.Values.FirstOrDefault() is int intCount)
                    return intCount;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not estimate row count for query, using default estimate");
        }

        return 10000; // Default estimate
    }

    private string DetermineQueryComplexity(string sql)
    {
        var lowerSql = sql.ToLowerInvariant();
        var complexityScore = 0;

        if (lowerSql.Contains("join")) complexityScore += 2;
        if (lowerSql.Contains("group by")) complexityScore += 1;
        if (lowerSql.Contains("order by")) complexityScore += 1;
        if (lowerSql.Contains("having")) complexityScore += 1;
        if (lowerSql.Contains("union")) complexityScore += 2;
        if (lowerSql.Contains("with")) complexityScore += 2; // CTE
        if (lowerSql.Contains("window")) complexityScore += 2;

        return complexityScore switch
        {
            >= 5 => "High",
            >= 3 => "Medium",
            _ => "Low"
        };
    }

    // Delegate all other methods to the inner service
    public Task<QueryResult> ExecuteSelectQueryAsync(string sql, QueryOptions? options = null)
        => _innerService.ExecuteSelectQueryAsync(sql, options);

    public Task<QueryResult> ExecuteSelectQueryAsync(string sql, QueryOptions? options, CancellationToken cancellationToken)
        => _innerService.ExecuteSelectQueryAsync(sql, options, cancellationToken);

    public Task<bool> ValidateSqlAsync(string sql)
        => _innerService.ValidateSqlAsync(sql);

    public Task<string> OptimizeSqlAsync(string sql)
        => _innerService.OptimizeSqlAsync(sql);

    public Task<QueryExecutionPlan> GetExecutionPlanAsync(string sql)
        => _innerService.GetExecutionPlanAsync(sql);

    public Task<QueryPerformanceMetrics> GetQueryPerformanceAsync(string sql)
        => _innerService.GetQueryPerformanceAsync(sql);

    public Task<bool> TestConnectionAsync(string? dataSource = null)
        => _innerService.TestConnectionAsync(dataSource);

    public Task<List<string>> GetAvailableDataSourcesAsync()
        => _innerService.GetAvailableDataSourcesAsync();

    public async IAsyncEnumerable<StreamingQueryChunk> ExecuteSelectQueryWithBackpressureAsync(
        string sql,
        QueryOptions? options = null,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        options ??= new QueryOptions();
        var chunkSize = options.ChunkSize ?? 1000;
        var maxBufferSize = 10; // Maximum chunks to buffer

        // Create bounded channel for backpressure control
        var channel = Channel.CreateBounded<StreamingQueryChunk>(new BoundedChannelOptions(maxBufferSize)
        {
            FullMode = BoundedChannelFullMode.Wait,
            SingleReader = true,
            SingleWriter = true
        });

        // Producer task
        _ = Task.Run(async () =>
        {
            try
            {
                await ProduceDataWithBackpressureAsync(sql, options, channel.Writer, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in streaming query producer");
                channel.Writer.TryComplete(ex);
            }
            finally
            {
                channel.Writer.TryComplete();
            }
        }, cancellationToken);

        // Consumer with automatic backpressure
        await foreach (var chunk in channel.Reader.ReadAllAsync(cancellationToken))
        {
            yield return chunk;
        }
    }

    public async IAsyncEnumerable<StreamingProgressUpdate> ExecuteSelectQueryWithProgressAsync(
        string sql,
        QueryOptions? options = null,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        options ??= new QueryOptions();
        var startTime = DateTime.UtcNow;
        var rowsProcessed = 0;
        var estimatedTotalRows = await EstimateRowCountAsync(sql, cancellationToken);
        var progressReportInterval = 1000; // Report progress every 1000 rows

        await foreach (var row in ExecuteSelectQueryStreamingAsync(sql, options, cancellationToken))
        {
            rowsProcessed++;

            // Report progress at intervals
            if (rowsProcessed % progressReportInterval == 0 || rowsProcessed == 1)
            {
                var elapsed = DateTime.UtcNow - startTime;
                var rowsPerSecond = rowsProcessed / Math.Max(elapsed.TotalSeconds, 0.1);
                var progressPercentage = estimatedTotalRows > 0 ? (double)rowsProcessed / estimatedTotalRows * 100 : 0;
                var estimatedTimeRemaining = estimatedTotalRows > 0 && rowsPerSecond > 0
                    ? TimeSpan.FromSeconds((estimatedTotalRows - rowsProcessed) / rowsPerSecond)
                    : TimeSpan.Zero;

                yield return new StreamingProgressUpdate
                {
                    RowsProcessed = rowsProcessed,
                    EstimatedTotalRows = estimatedTotalRows,
                    ProgressPercentage = Math.Min(progressPercentage, 100),
                    ElapsedTime = elapsed,
                    EstimatedTimeRemaining = estimatedTimeRemaining,
                    RowsPerSecond = rowsPerSecond,
                    Status = "Processing",
                    CurrentRow = row,
                    IsCompleted = false
                };
            }
        }

        // Final progress update
        var finalElapsed = DateTime.UtcNow - startTime;
        yield return new StreamingProgressUpdate
        {
            RowsProcessed = rowsProcessed,
            EstimatedTotalRows = Math.Max(estimatedTotalRows, rowsProcessed),
            ProgressPercentage = 100,
            ElapsedTime = finalElapsed,
            EstimatedTimeRemaining = TimeSpan.Zero,
            RowsPerSecond = rowsProcessed / Math.Max(finalElapsed.TotalSeconds, 0.1),
            Status = "Completed",
            IsCompleted = true
        };
    }

    private async Task ProduceDataWithBackpressureAsync(
        string sql,
        QueryOptions options,
        ChannelWriter<StreamingQueryChunk> writer,
        CancellationToken cancellationToken)
    {
        var chunkSize = options.ChunkSize ?? 1000;

        await foreach (var chunk in ExecuteSelectQueryChunkedAsync(sql, options, cancellationToken))
        {
            await writer.WriteAsync(chunk, cancellationToken);

            // Add small delay to prevent overwhelming the consumer
            if (chunk.TotalRowsInChunk >= chunkSize)
            {
                await Task.Delay(10, cancellationToken); // 10ms breathing room
            }
        }
    }
}
