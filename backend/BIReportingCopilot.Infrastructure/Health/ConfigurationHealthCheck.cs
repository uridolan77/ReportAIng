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
    private readonly ConfigurationService _configurationService;
    private readonly ConfigurationMigrationService _migrationService;
    private readonly ILogger<ConfigurationHealthCheck> _logger;

    public ConfigurationHealthCheck(
        ConfigurationService configurationService,
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
