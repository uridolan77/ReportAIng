using Microsoft.Extensions.Logging;

namespace BIReportingCopilot.Infrastructure.Jobs;

/// <summary>
/// Unified interface for all background jobs
/// Consolidates ICleanupJob and ISchemaRefreshJob into a single, flexible interface
/// </summary>
public interface IBackgroundJob
{
    /// <summary>
    /// Job name for identification and logging
    /// </summary>
    string JobName { get; }

    /// <summary>
    /// Execute the background job
    /// </summary>
    Task ExecuteAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Interface for cleanup background job
/// DEPRECATED: Use IBackgroundJob instead. This interface is kept for backward compatibility.
/// </summary>
[Obsolete("Use IBackgroundJob instead. This interface will be removed in future versions.")]
public interface ICleanupJob : IBackgroundJob
{
    /// <summary>
    /// Perform cleanup operations
    /// </summary>
    Task PerformCleanup();
}

/// <summary>
/// Unified cleanup background job implementation
/// </summary>
public class CleanupJob : ICleanupJob
{
    private readonly ILogger<CleanupJob> _logger;

    public string JobName => "Cleanup Job";

    public CleanupJob(ILogger<CleanupJob> logger)
    {
        _logger = logger;
    }

    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting {JobName}", JobName);

        try
        {
            await PerformCleanup();
            _logger.LogInformation("{JobName} completed successfully", JobName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{JobName} failed", JobName);
            throw;
        }
    }

    public async Task PerformCleanup()
    {
        _logger.LogInformation("Performing cleanup operations");

        // Cleanup operations:
        // 1. Remove expired cache entries
        // 2. Clean up old log files
        // 3. Remove temporary files
        // 4. Clean up old query history records

        await Task.Delay(100); // Simulate cleanup work
        _logger.LogDebug("Cleanup operations completed");
    }
}
