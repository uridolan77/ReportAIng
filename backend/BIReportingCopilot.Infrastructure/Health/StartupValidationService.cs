using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BIReportingCopilot.Infrastructure.Health;

/// <summary>
/// Background service for startup validation
/// </summary>
public class StartupValidationService : BackgroundService
{
    private readonly IStartupHealthValidator _validator;
    private readonly ILogger<StartupValidationService> _logger;

    public StartupValidationService(
        IStartupHealthValidator validator,
        ILogger<StartupValidationService> logger)
    {
        _validator = validator;
        _logger = logger;
    }

    /// <summary>
    /// Execute the startup validation
    /// </summary>
    /// <param name="stoppingToken">Stopping token</param>
    /// <returns>Task</returns>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            _logger.LogInformation("Starting startup validation service");

            // Wait a bit for the application to fully start
            await Task.Delay(1000, stoppingToken);

            // Perform startup validation
            var result = await _validator.ValidateStartupHealthAsync(stoppingToken);

            if (result.IsValid)
            {
                _logger.LogInformation("Startup validation completed successfully");
            }
            else
            {
                _logger.LogWarning("Startup validation completed with errors: {Errors}",
                    string.Join(", ", result.ValidationErrors));
            }

            // Store the result for health checks to use
            StartupValidationCache.SetResult(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in startup validation service");

            // Store error result
            var errorResult = new StartupValidationResult
            {
                IsValid = false,
                ValidationErrors = { $"Startup validation service failed: {ex.Message}" },
                CompletedAt = DateTime.UtcNow
            };
            StartupValidationCache.SetResult(errorResult);
        }
    }
}

/// <summary>
/// Cache for startup validation results
/// </summary>
public static class StartupValidationCache
{
    private static StartupValidationResult? _cachedResult;
    private static readonly object _lock = new();

    /// <summary>
    /// Set the validation result
    /// </summary>
    /// <param name="result">Validation result</param>
    public static void SetResult(StartupValidationResult result)
    {
        lock (_lock)
        {
            _cachedResult = result;
        }
    }

    /// <summary>
    /// Get the cached validation result
    /// </summary>
    /// <returns>Cached result or null if not available</returns>
    public static StartupValidationResult? GetResult()
    {
        lock (_lock)
        {
            return _cachedResult;
        }
    }

    /// <summary>
    /// Check if validation is complete
    /// </summary>
    /// <returns>True if validation is complete</returns>
    public static bool IsValidationComplete()
    {
        lock (_lock)
        {
            return _cachedResult != null;
        }
    }
}
