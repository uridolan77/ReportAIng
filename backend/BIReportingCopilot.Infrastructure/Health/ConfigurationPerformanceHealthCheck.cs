using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System.Diagnostics;

namespace BIReportingCopilot.Infrastructure.Health;

/// <summary>
/// Health check for configuration performance monitoring
/// </summary>
public class ConfigurationPerformanceHealthCheck : IHealthCheck
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<ConfigurationPerformanceHealthCheck> _logger;

    public ConfigurationPerformanceHealthCheck(
        IConfiguration configuration,
        ILogger<ConfigurationPerformanceHealthCheck> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var data = new Dictionary<string, object>();
        var issues = new List<string>();

        try
        {
            var stopwatch = Stopwatch.StartNew();

            // Test configuration access performance
            var configAccessTime = await MeasureConfigurationAccess();
            data["configuration_access_ms"] = configAccessTime.TotalMilliseconds;

            // Test critical configuration values
            var criticalConfigTime = await MeasureCriticalConfigurationAccess();
            data["critical_config_access_ms"] = criticalConfigTime.TotalMilliseconds;

            stopwatch.Stop();
            data["total_check_time_ms"] = stopwatch.Elapsed.TotalMilliseconds;

            // Check if configuration access is too slow
            if (configAccessTime.TotalMilliseconds > 100)
            {
                issues.Add($"Configuration access is slow ({configAccessTime.TotalMilliseconds:F2}ms)");
            }

            if (criticalConfigTime.TotalMilliseconds > 50)
            {
                issues.Add($"Critical configuration access is slow ({criticalConfigTime.TotalMilliseconds:F2}ms)");
            }

            // Check configuration provider count
            var providerCount = GetConfigurationProviderCount();
            data["configuration_providers"] = providerCount;

            if (providerCount > 10)
            {
                issues.Add($"Too many configuration providers ({providerCount}) may impact performance");
            }

            if (issues.Count == 0)
            {
                _logger.LogDebug("Configuration performance check passed in {Duration:F2}ms", stopwatch.Elapsed.TotalMilliseconds);
                return HealthCheckResult.Healthy("Configuration performance is optimal", data);
            }

            var message = $"Configuration performance issues: {string.Join("; ", issues)}";
            _logger.LogWarning("Configuration performance check failed: {Message}", message);
            return HealthCheckResult.Degraded(message, data: data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during configuration performance health check");
            return HealthCheckResult.Unhealthy(
                $"Configuration performance check failed: {ex.Message}",
                ex,
                data);
        }
    }

    private async Task<TimeSpan> MeasureConfigurationAccess()
    {
        var stopwatch = Stopwatch.StartNew();

        // Access various configuration sections
        var connectionString = _configuration.GetConnectionString("DefaultConnection");
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var appSettings = _configuration.GetSection("ApplicationSettings");
        var cacheSettings = _configuration.GetSection("Cache");
        var openAISettings = _configuration.GetSection("OpenAI");

        // Force evaluation
        _ = jwtSettings["Secret"];
        _ = appSettings["Environment"];
        _ = cacheSettings["EnableRedis"];
        _ = openAISettings["ApiKey"];

        stopwatch.Stop();
        return stopwatch.Elapsed;
    }

    private async Task<TimeSpan> MeasureCriticalConfigurationAccess()
    {
        var stopwatch = Stopwatch.StartNew();

        // Access critical configuration that should be fast
        var connectionString = _configuration.GetConnectionString("DefaultConnection");
        var environment = _configuration["ASPNETCORE_ENVIRONMENT"];
        var logging = _configuration.GetSection("Logging");

        stopwatch.Stop();
        return stopwatch.Elapsed;
    }

    private int GetConfigurationProviderCount()
    {
        try
        {
            if (_configuration is IConfigurationRoot configRoot)
            {
                return configRoot.Providers.Count();
            }
            return 1; // Fallback if we can't determine the count
        }
        catch
        {
            return 0;
        }
    }
}
