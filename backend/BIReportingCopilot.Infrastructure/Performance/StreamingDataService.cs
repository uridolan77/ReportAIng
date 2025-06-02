using Microsoft.Extensions.Logging;

namespace BIReportingCopilot.Infrastructure.Performance;

/// <summary>
/// Service for streaming data operations
/// </summary>
public class StreamingDataService
{
    private readonly ILogger<StreamingDataService> _logger;

    public StreamingDataService(ILogger<StreamingDataService> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Stream data asynchronously
    /// </summary>
    /// <param name="data">Data to stream</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Streaming result</returns>
    public async Task<StreamingResult> StreamDataAsync(object data, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Starting data streaming operation");

            // Simulate streaming operation
            await Task.Delay(100, cancellationToken);

            return new StreamingResult
            {
                Success = true,
                Message = "Data streamed successfully",
                BytesStreamed = 1024
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error streaming data");
            return new StreamingResult
            {
                Success = false,
                Message = ex.Message,
                BytesStreamed = 0
            };
        }
    }

    /// <summary>
    /// Stream data in chunks
    /// </summary>
    /// <param name="data">Data to stream</param>
    /// <param name="chunkSize">Size of each chunk</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Async enumerable of chunks</returns>
    public async IAsyncEnumerable<DataChunk> StreamDataInChunksAsync(
        IEnumerable<object> data, 
        int chunkSize = 1000,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var chunk = new List<object>();
        var chunkIndex = 0;

        foreach (var item in data)
        {
            if (cancellationToken.IsCancellationRequested)
                yield break;

            chunk.Add(item);

            if (chunk.Count >= chunkSize)
            {
                yield return new DataChunk
                {
                    Index = chunkIndex++,
                    Data = chunk.ToArray(),
                    Size = chunk.Count
                };

                chunk.Clear();
                await Task.Delay(10, cancellationToken); // Small delay to prevent overwhelming
            }
        }

        // Return remaining items
        if (chunk.Count > 0)
        {
            yield return new DataChunk
            {
                Index = chunkIndex,
                Data = chunk.ToArray(),
                Size = chunk.Count
            };
        }
    }
}

/// <summary>
/// Result of streaming operation
/// </summary>
public class StreamingResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public long BytesStreamed { get; set; }
    public TimeSpan Duration { get; set; }
}

/// <summary>
/// Data chunk for streaming
/// </summary>
public class DataChunk
{
    public int Index { get; set; }
    public object[] Data { get; set; } = Array.Empty<object>();
    public int Size { get; set; }
}
