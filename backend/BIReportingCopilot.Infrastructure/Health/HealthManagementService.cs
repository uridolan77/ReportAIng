using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using BIReportingCopilot.Infrastructure.Data;
using BIReportingCopilot.Infrastructure.Configuration;
using BIReportingCopilot.Core.Configuration;
using System.Diagnostics;

namespace BIReportingCopilot.Infrastructure.Health;

/// <summary>
/// Unified health management service consolidating all health check functionality
/// Replaces FastHealthChecks, SecurityHealthCheck, StartupHealthValidator, and StartupValidationService
/// </summary>
public class HealthManagementService : IHealthCheck
{
    private readonly BICopilotContext _context;
    private readonly ConfigurationService _configurationService;
    private readonly ILogger<HealthManagementService> _logger;
    private readonly MonitoringConfiguration _monitoringConfig;
    private readonly Dictionary<string, HealthCheckResult> _healthCheckCache;
    private readonly object _cacheLock = new();
    private DateTime _lastCacheUpdate = DateTime.MinValue;

    public HealthManagementService(
        BICopilotContext context,
        ConfigurationService configurationService,
        ILogger<HealthManagementService> logger)
    {
        _context = context;
        _configurationService = configurationService;
        _logger = logger;
        _monitoringConfig = configurationService.GetMonitoringSettings();
        _healthCheckCache = new Dictionary<string, HealthCheckResult>();
    }

    /// <summary>
    /// Main health check implementation
    /// </summary>
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var stopwatch = Stopwatch.StartNew();

            // Use cached results if available and recent
            if (ShouldUseCachedResults())
            {
                return GetCachedOverallHealth();
            }

            var healthChecks = new Dictionary<string, HealthCheckResult>();

            // Perform all health checks
            var checkTasks = new[]
            {
                PerformDatabaseHealthCheckAsync(cancellationToken),
                PerformConfigurationHealthCheckAsync(cancellationToken),
                PerformSecurityHealthCheckAsync(cancellationToken),
                PerformPerformanceHealthCheckAsync(cancellationToken),
                PerformStartupValidationAsync(cancellationToken)
            };

            var results = await Task.WhenAll(checkTasks);

            healthChecks["Database"] = results[0];
            healthChecks["Configuration"] = results[1];
            healthChecks["Security"] = results[2];
            healthChecks["Performance"] = results[3];
            healthChecks["StartupValidation"] = results[4];

            // Cache results
            lock (_cacheLock)
            {
                foreach (var (key, healthResult) in healthChecks)
                {
                    _healthCheckCache[key] = healthResult;
                }
                _lastCacheUpdate = DateTime.UtcNow;
            }

            // Determine overall health
            var overallStatus = healthChecks.Values.All(r => r.Status == HealthStatus.Healthy)
                ? HealthStatus.Healthy
                : healthChecks.Values.Any(r => r.Status == HealthStatus.Unhealthy)
                    ? HealthStatus.Unhealthy
                    : HealthStatus.Degraded;

            var overallData = healthChecks.ToDictionary(
                kvp => kvp.Key,
                kvp => (object)new { Status = kvp.Value.Status.ToString(), Description = kvp.Value.Description }
            );

            overallData["TotalCheckTime"] = stopwatch.ElapsedMilliseconds;
            overallData["CheckedAt"] = DateTime.UtcNow;

            stopwatch.Stop();

            var result = new HealthCheckResult(
                overallStatus,
                $"Overall health: {overallStatus}. Completed {healthChecks.Count} checks in {stopwatch.ElapsedMilliseconds}ms",
                data: overallData
            );

            _logger.LogInformation("Health check completed: {Status} in {ElapsedMs}ms",
                overallStatus, stopwatch.ElapsedMilliseconds);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Health check failed with exception");
            return HealthCheckResult.Unhealthy("Health check failed with exception", ex);
        }
    }

    /// <summary>
    /// Get detailed health status for all components
    /// </summary>
    public async Task<DetailedHealthStatus> GetDetailedHealthStatusAsync(CancellationToken cancellationToken = default)
    {
        var status = new DetailedHealthStatus
        {
            CheckedAt = DateTime.UtcNow,
            OverallStatus = HealthStatus.Healthy
        };

        try
        {
            // Perform individual health checks
            status.DatabaseHealth = await PerformDatabaseHealthCheckAsync(cancellationToken);
            status.ConfigurationHealth = await PerformConfigurationHealthCheckAsync(cancellationToken);
            status.SecurityHealth = await PerformSecurityHealthCheckAsync(cancellationToken);
            status.PerformanceHealth = await PerformPerformanceHealthCheckAsync(cancellationToken);
            status.StartupValidationHealth = await PerformStartupValidationAsync(cancellationToken);

            // Determine overall status
            var allChecks = new[]
            {
                status.DatabaseHealth,
                status.ConfigurationHealth,
                status.SecurityHealth,
                status.PerformanceHealth,
                status.StartupValidationHealth
            };

            if (allChecks.Any(c => c.Status == HealthStatus.Unhealthy))
                status.OverallStatus = HealthStatus.Unhealthy;
            else if (allChecks.Any(c => c.Status == HealthStatus.Degraded))
                status.OverallStatus = HealthStatus.Degraded;

            status.Summary = $"Overall: {status.OverallStatus}. " +
                           $"Healthy: {allChecks.Count(c => c.Status == HealthStatus.Healthy)}, " +
                           $"Degraded: {allChecks.Count(c => c.Status == HealthStatus.Degraded)}, " +
                           $"Unhealthy: {allChecks.Count(c => c.Status == HealthStatus.Unhealthy)}";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Detailed health check failed");
            status.OverallStatus = HealthStatus.Unhealthy;
            status.Summary = $"Health check failed: {ex.Message}";
        }

        return status;
    }

    /// <summary>
    /// Perform startup validation
    /// </summary>
    public async Task<StartupValidationResult> ValidateStartupAsync(CancellationToken cancellationToken = default)
    {
        var result = new StartupValidationResult
        {
            StartedAt = DateTime.UtcNow,
            IsValid = true
        };

        try
        {
            _logger.LogInformation("Starting application startup validation");

            // Configuration validation
            var configValidation = await _configurationService.ValidateAllConfigurationsAsync();
            result.ConfigurationValid = configValidation.IsValid;
            if (!configValidation.IsValid)
            {
                result.IsValid = false;
                result.ValidationErrors.AddRange(configValidation.Errors);
            }

            // Database connectivity
            try
            {
                await _context.Database.CanConnectAsync(cancellationToken);
                result.DatabaseConnectivity = true;
            }
            catch (Exception ex)
            {
                result.DatabaseConnectivity = false;
                result.IsValid = false;
                result.ValidationErrors.Add($"Database connectivity failed: {ex.Message}");
            }

            // Essential services validation
            result.EssentialServicesValid = await ValidateEssentialServicesAsync();
            if (!result.EssentialServicesValid)
            {
                result.IsValid = false;
                result.ValidationErrors.Add("Essential services validation failed");
            }

            result.CompletedAt = DateTime.UtcNow;
            result.ValidationDuration = result.CompletedAt - result.StartedAt;

            _logger.LogInformation("Startup validation completed: {IsValid} in {Duration}ms",
                result.IsValid, result.ValidationDuration.TotalMilliseconds);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Startup validation failed with exception");
            result.IsValid = false;
            result.ValidationErrors.Add($"Startup validation exception: {ex.Message}");
        }

        return result;
    }

    #region Private Health Check Methods

    private async Task<HealthCheckResult> PerformDatabaseHealthCheckAsync(CancellationToken cancellationToken)
    {
        try
        {
            var stopwatch = Stopwatch.StartNew();

            // Test database connectivity
            var canConnect = await _context.Database.CanConnectAsync(cancellationToken);
            if (!canConnect)
            {
                return HealthCheckResult.Unhealthy("Cannot connect to database");
            }

            // Test a simple query
            var tableCount = await _context.BusinessTableInfo.CountAsync(cancellationToken);

            stopwatch.Stop();

            var data = new Dictionary<string, object>
            {
                ["CanConnect"] = canConnect,
                ["TableCount"] = tableCount,
                ["ResponseTime"] = stopwatch.ElapsedMilliseconds
            };

            return HealthCheckResult.Healthy($"Database healthy. {tableCount} tables available. Response time: {stopwatch.ElapsedMilliseconds}ms", data);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Database health check failed");
            return HealthCheckResult.Unhealthy($"Database health check failed: {ex.Message}");
        }
    }

    private async Task<HealthCheckResult> PerformConfigurationHealthCheckAsync(CancellationToken cancellationToken)
    {
        try
        {
            var validationResult = await _configurationService.ValidateAllConfigurationsAsync();

            if (validationResult.IsValid)
            {
                return HealthCheckResult.Healthy("All configurations are valid", new Dictionary<string, object>
                {
                    ["ValidatedSections"] = validationResult.SectionResults.Count,
                    ["ValidatedAt"] = validationResult.ValidatedAt
                });
            }
            else
            {
                var data = new Dictionary<string, object>
                {
                    ["ErrorCount"] = validationResult.Errors.Count,
                    ["Errors"] = validationResult.Errors.Take(5).ToList()
                };
                return HealthCheckResult.Degraded($"Configuration validation issues: {string.Join(", ", validationResult.Errors.Take(3))}", null, data);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Configuration health check failed");
            return HealthCheckResult.Unhealthy($"Configuration health check failed: {ex.Message}");
        }
    }

    private async Task<HealthCheckResult> PerformSecurityHealthCheckAsync(CancellationToken cancellationToken)
    {
        try
        {
            var securityConfig = _configurationService.GetSecuritySettings();
            var issues = new List<string>();

            // Check JWT configuration
            if (string.IsNullOrEmpty(securityConfig.JwtSecret) || securityConfig.JwtSecret.Length < 32)
            {
                issues.Add("JWT secret is not properly configured");
            }

            // Check rate limiting
            if (!securityConfig.EnableRateLimit)
            {
                issues.Add("Rate limiting is disabled");
            }

            // Check HTTPS
            if (!securityConfig.EnableHttpsRedirection)
            {
                issues.Add("HTTPS redirection is disabled");
            }

            if (issues.Any())
            {
                var securityData = new Dictionary<string, object>
                {
                    ["Issues"] = issues,
                    ["IssueCount"] = issues.Count
                };
                return HealthCheckResult.Degraded($"Security configuration issues: {string.Join(", ", issues)}", null, securityData);
            }

            return HealthCheckResult.Healthy("Security configuration is properly set up", new Dictionary<string, object>
            {
                ["RateLimitingEnabled"] = securityConfig.EnableRateLimit,
                ["HttpsRedirectionEnabled"] = securityConfig.EnableHttpsRedirection,
                ["AuditLoggingEnabled"] = securityConfig.EnableAuditLogging
            });
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Security health check failed");
            return HealthCheckResult.Unhealthy($"Security health check failed: {ex.Message}");
        }
    }

    private async Task<HealthCheckResult> PerformPerformanceHealthCheckAsync(CancellationToken cancellationToken)
    {
        try
        {
            var performanceConfig = _configurationService.GetPerformanceSettings();

            // Check if performance features are enabled
            var enabledFeatures = new List<string>();
            if (performanceConfig.EnableQueryCaching) enabledFeatures.Add("QueryCaching");
            if (performanceConfig.EnablePerformanceMetrics) enabledFeatures.Add("PerformanceMetrics");
            if (performanceConfig.EnableStreaming) enabledFeatures.Add("Streaming");

            return HealthCheckResult.Healthy($"Performance features enabled: {string.Join(", ", enabledFeatures)}", new Dictionary<string, object>
            {
                ["EnabledFeatures"] = enabledFeatures,
                ["MaxConcurrentQueries"] = performanceConfig.MaxConcurrentQueries,
                ["DefaultTimeout"] = performanceConfig.DefaultQueryTimeoutSeconds,
                ["StreamingBatchSize"] = performanceConfig.StreamingBatchSize
            });
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Performance health check failed");
            return HealthCheckResult.Unhealthy($"Performance health check failed: {ex.Message}");
        }
    }

    private async Task<HealthCheckResult> PerformStartupValidationAsync(CancellationToken cancellationToken)
    {
        try
        {
            var validationResult = await ValidateStartupAsync(cancellationToken);

            if (validationResult.IsValid)
            {
                return HealthCheckResult.Healthy($"Startup validation passed in {validationResult.ValidationDuration.TotalMilliseconds}ms", new Dictionary<string, object>
                {
                    ["ConfigurationValid"] = validationResult.ConfigurationValid,
                    ["DatabaseConnectivity"] = validationResult.DatabaseConnectivity,
                    ["EssentialServicesValid"] = validationResult.EssentialServicesValid,
                    ["ValidationDuration"] = validationResult.ValidationDuration.TotalMilliseconds
                });
            }
            else
            {
                var startupData = new Dictionary<string, object>
                {
                    ["ErrorCount"] = validationResult.ValidationErrors.Count,
                    ["Errors"] = validationResult.ValidationErrors.Take(3).ToList()
                };
                return HealthCheckResult.Degraded($"Startup validation issues: {validationResult.ValidationErrors.Count} errors", null, startupData);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Startup validation health check failed");
            return HealthCheckResult.Unhealthy($"Startup validation health check failed: {ex.Message}");
        }
    }

    private async Task<bool> ValidateEssentialServicesAsync()
    {
        try
        {
            // Validate that essential services are properly configured
            var aiConfig = _configurationService.GetAISettings();
            var cacheConfig = _configurationService.GetCacheSettings();

            // Check if essential AI features are enabled
            if (!aiConfig.EnableSemanticAnalysis || !aiConfig.EnableQueryClassification)
            {
                return false;
            }

            // Check cache configuration
            if (cacheConfig.EnableDistributedCache && string.IsNullOrEmpty(cacheConfig.RedisConnectionString))
            {
                return false;
            }

            return true;
        }
        catch
        {
            return false;
        }
    }

    private bool ShouldUseCachedResults()
    {
        return DateTime.UtcNow - _lastCacheUpdate < TimeSpan.FromSeconds(30) && _healthCheckCache.Any();
    }

    private HealthCheckResult GetCachedOverallHealth()
    {
        lock (_cacheLock)
        {
            var overallStatus = _healthCheckCache.Values.All(r => r.Status == HealthStatus.Healthy)
                ? HealthStatus.Healthy
                : _healthCheckCache.Values.Any(r => r.Status == HealthStatus.Unhealthy)
                    ? HealthStatus.Unhealthy
                    : HealthStatus.Degraded;

            var data = _healthCheckCache.ToDictionary(
                kvp => kvp.Key,
                kvp => (object)new { Status = kvp.Value.Status.ToString(), Description = kvp.Value.Description }
            );

            data["CachedResult"] = true;
            data["CacheAge"] = (DateTime.UtcNow - _lastCacheUpdate).TotalSeconds;

            return new HealthCheckResult(overallStatus, $"Cached health status: {overallStatus}", data: data);
        }
    }

    #endregion
}

/// <summary>
/// Detailed health status for all components
/// </summary>
public class DetailedHealthStatus
{
    public DateTime CheckedAt { get; set; }
    public HealthStatus OverallStatus { get; set; }
    public string Summary { get; set; } = string.Empty;
    public HealthCheckResult DatabaseHealth { get; set; } = HealthCheckResult.Unhealthy("Not checked");
    public HealthCheckResult ConfigurationHealth { get; set; } = HealthCheckResult.Unhealthy("Not checked");
    public HealthCheckResult SecurityHealth { get; set; } = HealthCheckResult.Unhealthy("Not checked");
    public HealthCheckResult PerformanceHealth { get; set; } = HealthCheckResult.Unhealthy("Not checked");
    public HealthCheckResult StartupValidationHealth { get; set; } = HealthCheckResult.Unhealthy("Not checked");
}

/// <summary>
/// Startup validation result
/// </summary>
public class StartupValidationResult
{
    public DateTime StartedAt { get; set; }
    public DateTime CompletedAt { get; set; }
    public TimeSpan ValidationDuration { get; set; }
    public bool IsValid { get; set; }
    public bool ConfigurationValid { get; set; }
    public bool DatabaseConnectivity { get; set; }
    public bool EssentialServicesValid { get; set; }
    public List<string> ValidationErrors { get; set; } = new();
}
