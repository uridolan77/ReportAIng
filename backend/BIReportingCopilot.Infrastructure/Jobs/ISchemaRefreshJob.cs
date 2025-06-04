using Microsoft.Extensions.Logging;

namespace BIReportingCopilot.Infrastructure.Jobs;

/// <summary>
/// Interface for schema refresh background job
/// DEPRECATED: Use IBackgroundJob instead. This interface is kept for backward compatibility.
/// </summary>
[Obsolete("Use IBackgroundJob instead. This interface will be removed in future versions.")]
public interface ISchemaRefreshJob : IBackgroundJob
{
    /// <summary>
    /// Refresh all schemas
    /// </summary>
    Task RefreshAllSchemas();
}

/// <summary>
/// Unified schema refresh background job implementation
/// </summary>
public class SchemaRefreshJob : ISchemaRefreshJob
{
    private readonly ILogger<SchemaRefreshJob> _logger;

    public string JobName => "Schema Refresh Job";

    public SchemaRefreshJob(ILogger<SchemaRefreshJob> logger)
    {
        _logger = logger;
    }

    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting {JobName}", JobName);

        try
        {
            await RefreshAllSchemas();
            _logger.LogInformation("{JobName} completed successfully", JobName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{JobName} failed", JobName);
            throw;
        }
    }

    public async Task RefreshAllSchemas()
    {
        _logger.LogInformation("Refreshing all schemas");

        // Schema refresh operations:
        // 1. Refresh database schema metadata
        // 2. Update cached table information
        // 3. Refresh column metadata
        // 4. Update business context mappings

        await Task.Delay(200); // Simulate schema refresh work
        _logger.LogDebug("Schema refresh operations completed");
    }
}
