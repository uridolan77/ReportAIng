using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using BIReportingCopilot.Infrastructure.Configuration;
using BIReportingCopilot.Infrastructure.Data.Contexts;

namespace BIReportingCopilot.Infrastructure.Health;

/// <summary>
/// Health check for configuration validation and migration status
/// </summary>
public class ConfigurationHealthCheck : IHealthCheck
{
    private readonly UnifiedConfigurationService _configurationService;
    private readonly ConfigurationMigrationService _migrationService;
    private readonly ILogger<ConfigurationHealthCheck> _logger;

    public ConfigurationHealthCheck(
        UnifiedConfigurationService configurationService,
        ConfigurationMigrationService migrationService,
        ILogger<ConfigurationHealthCheck> logger)
    {
        _configurationService = configurationService;
        _migrationService = migrationService;
        _logger = logger;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var data = new Dictionary<string, object>();
            var issues = new List<string>();

            // Check configuration validation
            var configValidation = await _configurationService.ValidateAllConfigurationsAsync();
            data["ConfigurationValid"] = configValidation.IsValid;
            data["ConfigurationErrors"] = configValidation.Errors.Count;

            if (!configValidation.IsValid)
            {
                issues.AddRange(configValidation.Errors.Take(3)); // Limit to first 3 errors
            }

            // Check migration status
            var migrationStatus = _migrationService.GetMigrationStatus();
            data["UnifiedConfigurationAvailable"] = migrationStatus.UnifiedConfigurationAvailable;
            data["LegacyConfigurationAvailable"] = migrationStatus.LegacyConfigurationAvailable;
            data["MigrationServiceActive"] = migrationStatus.MigrationServiceActive;

            if (!migrationStatus.UnifiedConfigurationAvailable)
            {
                issues.Add("Unified configuration service not available");
            }

            // Check critical configurations
            try
            {
                var aiConfig = _configurationService.GetAISettings();
                data["AIConfigurationLoaded"] = true;
                data["OpenAIConfigured"] = !string.IsNullOrEmpty(aiConfig.OpenAIApiKey);
            }
            catch (Exception ex)
            {
                data["AIConfigurationLoaded"] = false;
                issues.Add($"AI configuration error: {ex.Message}");
            }

            try
            {
                var securityConfig = _configurationService.GetSecuritySettings();
                data["SecurityConfigurationLoaded"] = true;
                data["JWTConfigured"] = !string.IsNullOrEmpty(securityConfig.JwtSecret);
            }
            catch (Exception ex)
            {
                data["SecurityConfigurationLoaded"] = false;
                issues.Add($"Security configuration error: {ex.Message}");
            }

            try
            {
                var dbConfig = _configurationService.GetDatabaseSettings();
                data["DatabaseConfigurationLoaded"] = true;
                data["ConnectionStringConfigured"] = !string.IsNullOrEmpty(dbConfig.ConnectionString);
            }
            catch (Exception ex)
            {
                data["DatabaseConfigurationLoaded"] = false;
                issues.Add($"Database configuration error: {ex.Message}");
            }

            // Determine health status
            if (issues.Count == 0)
            {
                return HealthCheckResult.Healthy("All configurations are valid and loaded successfully", data);
            }
            else if (issues.Count <= 2)
            {
                return HealthCheckResult.Degraded($"Configuration issues detected: {string.Join("; ", issues)}", null, data);
            }
            else
            {
                return HealthCheckResult.Unhealthy($"Multiple configuration issues: {string.Join("; ", issues)}", null, data);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during configuration health check");
            return HealthCheckResult.Unhealthy($"Configuration health check failed: {ex.Message}");
        }
    }
}

/// <summary>
/// Health check for configuration performance monitoring
/// </summary>
public class ConfigurationPerformanceHealthCheck : IHealthCheck
{
    private readonly UnifiedConfigurationService _configurationService;
    private readonly ILogger<ConfigurationPerformanceHealthCheck> _logger;

    public ConfigurationPerformanceHealthCheck(
        UnifiedConfigurationService configurationService,
        ILogger<ConfigurationPerformanceHealthCheck> logger)
    {
        _configurationService = configurationService;
        _logger = logger;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var data = new Dictionary<string, object>();
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            // Test configuration loading performance
            var configLoadTimes = new Dictionary<string, long>();

            // Test AI configuration
            var aiStopwatch = System.Diagnostics.Stopwatch.StartNew();
            _ = _configurationService.GetAISettings();
            aiStopwatch.Stop();
            configLoadTimes["AI"] = aiStopwatch.ElapsedMilliseconds;

            // Test Security configuration
            var securityStopwatch = System.Diagnostics.Stopwatch.StartNew();
            _ = _configurationService.GetSecuritySettings();
            securityStopwatch.Stop();
            configLoadTimes["Security"] = securityStopwatch.ElapsedMilliseconds;

            // Test Database configuration
            var dbStopwatch = System.Diagnostics.Stopwatch.StartNew();
            _ = _configurationService.GetDatabaseSettings();
            dbStopwatch.Stop();
            configLoadTimes["Database"] = dbStopwatch.ElapsedMilliseconds;

            // Test Performance configuration
            var perfStopwatch = System.Diagnostics.Stopwatch.StartNew();
            _ = _configurationService.GetPerformanceSettings();
            perfStopwatch.Stop();
            configLoadTimes["Performance"] = perfStopwatch.ElapsedMilliseconds;

            stopwatch.Stop();

            data["TotalLoadTimeMs"] = stopwatch.ElapsedMilliseconds;
            data["ConfigurationLoadTimes"] = configLoadTimes;
            data["AverageLoadTimeMs"] = configLoadTimes.Values.Average();
            data["MaxLoadTimeMs"] = configLoadTimes.Values.Max();

            // Performance thresholds
            const int warningThresholdMs = 100;
            const int criticalThresholdMs = 500;

            var maxTime = configLoadTimes.Values.Max();
            var avgTime = configLoadTimes.Values.Average();

            if (maxTime > criticalThresholdMs)
            {
                return HealthCheckResult.Unhealthy(
                    $"Configuration loading is too slow. Max time: {maxTime}ms, Average: {avgTime:F1}ms", 
                    null, data);
            }
            else if (maxTime > warningThresholdMs)
            {
                return HealthCheckResult.Degraded(
                    $"Configuration loading is slower than expected. Max time: {maxTime}ms, Average: {avgTime:F1}ms", 
                    null, data);
            }
            else
            {
                return HealthCheckResult.Healthy(
                    $"Configuration loading performance is good. Max time: {maxTime}ms, Average: {avgTime:F1}ms", 
                    data);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during configuration performance health check");
            return HealthCheckResult.Unhealthy($"Configuration performance health check failed: {ex.Message}");
        }
    }
}

/// <summary>
/// Health check for bounded database contexts (Enhancement #4: Database Context Optimization)
/// </summary>
public class BoundedContextsHealthCheck : IHealthCheck
{
    private readonly IDbContextFactory _contextFactory;
    private readonly ILogger<BoundedContextsHealthCheck> _logger;

    public BoundedContextsHealthCheck(
        IDbContextFactory contextFactory,
        ILogger<BoundedContextsHealthCheck> logger)
    {
        _contextFactory = contextFactory;
        _logger = logger;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var data = new Dictionary<string, object>();
            var issues = new List<string>();

            // Check all bounded contexts
            var connectionHealth = await _contextFactory.GetConnectionHealthAsync();

            foreach (var (contextType, isHealthy) in connectionHealth)
            {
                data[$"{contextType}ContextHealthy"] = isHealthy;
                if (!isHealthy)
                {
                    issues.Add($"{contextType} context connection failed");
                }
            }

            // Validate contexts can be created
            var validation = await _contextFactory.ValidateAllContextsAsync();
            data["AllContextsValid"] = validation.IsValid;
            data["SuccessfulContexts"] = validation.SuccessfulContexts.Count;
            data["FailedContexts"] = validation.FailedContexts.Count;

            if (!validation.IsValid)
            {
                issues.AddRange(validation.FailedContexts.Values.Take(3)); // Limit to first 3 errors
            }

            // Test basic operations on each context
            await TestContextOperationsAsync(data, issues);

            // Determine health status
            if (issues.Count == 0)
            {
                return HealthCheckResult.Healthy("All bounded contexts are healthy and operational", data);
            }
            else if (issues.Count <= 2)
            {
                return HealthCheckResult.Degraded($"Some bounded context issues detected: {string.Join("; ", issues)}", null, data);
            }
            else
            {
                return HealthCheckResult.Unhealthy($"Multiple bounded context issues: {string.Join("; ", issues)}", null, data);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during bounded contexts health check");
            return HealthCheckResult.Unhealthy($"Bounded contexts health check failed: {ex.Message}");
        }
    }

    private async Task TestContextOperationsAsync(Dictionary<string, object> data, List<string> issues)
    {
        try
        {
            // Test SecurityDbContext
            try
            {
                var securityContext = _contextFactory.CreateSecurityContext();
                var userCount = await securityContext.Users.CountAsync();
                data["SecurityContextUserCount"] = userCount;
            }
            catch (Exception ex)
            {
                issues.Add($"Security context operation failed: {ex.Message}");
            }

            // Test TuningDbContext
            try
            {
                var tuningContext = _contextFactory.CreateTuningContext();
                var tableCount = await tuningContext.BusinessTableInfo.CountAsync();
                data["TuningContextTableCount"] = tableCount;
            }
            catch (Exception ex)
            {
                issues.Add($"Tuning context operation failed: {ex.Message}");
            }

            // Test QueryDbContext
            try
            {
                var queryContext = _contextFactory.CreateQueryContext();
                var historyCount = await queryContext.QueryHistory.CountAsync();
                data["QueryContextHistoryCount"] = historyCount;
            }
            catch (Exception ex)
            {
                issues.Add($"Query context operation failed: {ex.Message}");
            }

            // Test SchemaDbContext
            try
            {
                var schemaContext = _contextFactory.CreateSchemaContext();
                var metadataCount = await schemaContext.SchemaMetadata.CountAsync();
                data["SchemaContextMetadataCount"] = metadataCount;
            }
            catch (Exception ex)
            {
                issues.Add($"Schema context operation failed: {ex.Message}");
            }

            // Test MonitoringDbContext
            try
            {
                var monitoringContext = _contextFactory.CreateMonitoringContext();
                var metricsCount = await monitoringContext.SystemMetrics.CountAsync();
                data["MonitoringContextMetricsCount"] = metricsCount;
            }
            catch (Exception ex)
            {
                issues.Add($"Monitoring context operation failed: {ex.Message}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error testing context operations");
            issues.Add($"Context operations test failed: {ex.Message}");
        }
    }
}
