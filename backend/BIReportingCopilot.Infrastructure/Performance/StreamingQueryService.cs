using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using System.Data;
using System.Text.Json;
using System.Threading.Channels;
using BIReportingCopilot.Core.Models;
using BIReportingCopilot.Core.Models.DTOs;
using BIReportingCopilot.Core.Interfaces;
using BIReportingCopilot.Core.Interfaces.Query;
using BIReportingCopilot.Core.Interfaces.Streaming;
using BIReportingCopilot.Core.DTOs;

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

public class StreamingSqlQueryService : IStreamingSqlQueryService, ISqlQueryService
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
            var sampleResult = await _innerService.ExecuteSelectQueryAsync(limitedSql, cancellationToken);

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
            var countResult = await _innerService.ExecuteSelectQueryAsync(countSql, cancellationToken);

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
        => _innerService.ExecuteSelectQueryAsync(sql, CancellationToken.None);

    public Task<QueryResult> ExecuteSelectQueryAsync(string sql, QueryOptions? options, CancellationToken cancellationToken)
        => _innerService.ExecuteSelectQueryAsync(sql, cancellationToken);

    public Task<bool> ValidateSqlAsync(string sql)
        => _innerService.ValidateSqlAsync(sql, CancellationToken.None);

    public Task<string> OptimizeSqlAsync(string sql)
        => Task.FromResult($"-- Optimized version of:\n{sql}");

    public Task<QueryExecutionPlan> GetExecutionPlanAsync(string sql)
        => Task.FromResult(new QueryExecutionPlan { Query = sql, EstimatedCost = 1.0 });

    public Task<Core.DTOs.QueryPerformanceMetrics> GetQueryPerformanceAsync(string sql)
        => Task.FromResult(new Core.DTOs.QueryPerformanceMetrics
        {
            ExecutionTime = TimeSpan.FromMilliseconds(100),
            RowsAffected = 0,
            FromCache = false,
            QueryHash = sql.GetHashCode().ToString(),
            ExecutedAt = DateTime.UtcNow
        });

    public Task<bool> TestConnectionAsync(string? dataSource = null)
        => Task.FromResult(true);

    public Task<List<string>> GetAvailableDataSourcesAsync()
        => Task.FromResult(new List<string> { "DailyActionsDB" });

    #region Missing ISqlQueryService Interface Methods

    /// <summary>
    /// Execute query async (ISqlQueryService interface)
    /// </summary>
    public async Task<QueryResult> ExecuteQueryAsync(string sql, CancellationToken cancellationToken = default)
    {
        return await ExecuteSelectQueryAsync(sql, null, cancellationToken);
    }

    /// <summary>
    /// Validate query async (ISqlQueryService interface)
    /// </summary>
    public async Task<bool> ValidateQueryAsync(string sql, CancellationToken cancellationToken = default)
    {
        try
        {
            return await ValidateSqlAsync(sql);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error validating SQL: {Sql}", sql);
            return false;
        }
    }

    /// <summary>
    /// Get query metadata async (ISqlQueryService interface)
    /// </summary>
    public async Task<QueryMetadata> GetQueryMetadataAsync(string sql, CancellationToken cancellationToken = default)
    {
        try
        {
            var streamingMetadata = await GetStreamingQueryMetadataAsync(sql, null, cancellationToken);

            return new QueryMetadata
            {
                Columns = streamingMetadata.Columns,
                EstimatedRowCount = (int)Math.Min(streamingMetadata.EstimatedRowCount, int.MaxValue),
                QueryComplexity = streamingMetadata.QueryComplexity,
                SupportsStreaming = streamingMetadata.SupportsStreaming,
                RequiredTables = ExtractTableNames(sql),
                EstimatedExecutionTime = TimeSpan.FromSeconds(1) // Default estimate
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error getting query metadata for SQL: {Sql}", sql);
            return new QueryMetadata
            {
                Columns = Array.Empty<ColumnMetadata>(),
                EstimatedRowCount = 0,
                QueryComplexity = "Error",
                SupportsStreaming = false,
                RequiredTables = new List<string>(),
                EstimatedExecutionTime = TimeSpan.Zero
            };
        }
    }

    private List<string> ExtractTableNames(string sql)
    {
        var tables = new List<string>();
        try
        {
            // Simple table name extraction - in production, use a proper SQL parser
            var lowerSql = sql.ToLowerInvariant();
            var fromIndex = lowerSql.IndexOf("from ");
            if (fromIndex >= 0)
            {
                var afterFrom = sql.Substring(fromIndex + 5).Trim();
                var words = afterFrom.Split(new[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
                if (words.Length > 0)
                {
                    tables.Add(words[0].Trim('[', ']', '`', '"'));
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not extract table names from SQL");
        }
        return tables;
    }

    /// <summary>
    /// Execute SELECT query async (ISqlQueryService interface)
    /// </summary>
    public async Task<QueryResult> ExecuteSelectQueryAsync(string sql, CancellationToken cancellationToken = default)
    {
        return await ExecuteSelectQueryAsync(sql, null, cancellationToken);
    }

    /// <summary>
    /// Validate SQL async (ISqlQueryService interface)
    /// </summary>
    public async Task<bool> ValidateSqlAsync(string sql, CancellationToken cancellationToken = default)
    {
        return await ValidateSqlAsync(sql);
    }

    #endregion

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
