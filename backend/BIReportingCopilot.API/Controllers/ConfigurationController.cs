using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using BIReportingCopilot.Infrastructure.Configuration;
using BIReportingCopilot.Core.Configuration;

namespace BIReportingCopilot.API.Controllers;

/// <summary>
/// Configuration management controller for admin operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class ConfigurationController : ControllerBase
{
    private readonly ConfigurationService _configurationService;
    private readonly ConfigurationMigrationService _migrationService;
    private readonly ILogger<ConfigurationController> _logger;

    public ConfigurationController(
        ConfigurationService configurationService,
        ConfigurationMigrationService migrationService,
        ILogger<ConfigurationController> logger)
    {
        _configurationService = configurationService;
        _migrationService = migrationService;
        _logger = logger;
    }

    /// <summary>
    /// Get all configuration sections
    /// </summary>
    [HttpGet("sections")]
    public ActionResult<IEnumerable<string>> GetSections()
    {
        try
        {
            var sections = _configurationService.GetAllSections();
            return Ok(sections);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting configuration sections");
            return StatusCode(500, "Error retrieving configuration sections");
        }
    }

    /// <summary>
    /// Get configuration section as JSON
    /// </summary>
    [HttpGet("sections/{sectionName}")]
    public ActionResult<string> GetSection(string sectionName)
    {
        try
        {
            if (!_configurationService.SectionExists(sectionName))
            {
                return NotFound($"Configuration section '{sectionName}' not found");
            }

            var json = _configurationService.GetConfigurationAsJson(sectionName);
            return Ok(json);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting configuration section {SectionName}", sectionName);
            return StatusCode(500, $"Error retrieving configuration section '{sectionName}'");
        }
    }

    /// <summary>
    /// Get application settings
    /// </summary>
    [HttpGet("application")]
    public ActionResult<ApplicationSettings> GetApplicationSettings()
    {
        try
        {
            var settings = _configurationService.GetApplicationSettings();
            return Ok(settings);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting application settings");
            return StatusCode(500, "Error retrieving application settings");
        }
    }

    /// <summary>
    /// Get AI configuration
    /// </summary>
    [HttpGet("ai")]
    public ActionResult<AIConfiguration> GetAISettings()
    {
        try
        {
            var settings = _configurationService.GetAISettings();
            // Remove sensitive information
            settings.OpenAIApiKey = string.IsNullOrEmpty(settings.OpenAIApiKey) ? "" : "***CONFIGURED***";
            return Ok(settings);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting AI settings");
            return StatusCode(500, "Error retrieving AI settings");
        }
    }

    /// <summary>
    /// Get security configuration (sanitized)
    /// </summary>
    [HttpGet("security")]
    public ActionResult<object> GetSecuritySettings()
    {
        try
        {
            var settings = _configurationService.GetSecuritySettings();
            
            // Return sanitized version without sensitive data
            var sanitized = new
            {
                settings.JwtIssuer,
                settings.JwtAudience,
                settings.AccessTokenExpirationMinutes,
                settings.RefreshTokenExpirationMinutes,
                settings.MaxLoginAttempts,
                settings.LockoutDurationMinutes,
                settings.MinPasswordLength,
                settings.RequireDigit,
                settings.RequireLowercase,
                settings.RequireUppercase,
                settings.RequireNonAlphanumeric,
                settings.EnableTwoFactorAuthentication,
                settings.EnableRateLimit,
                settings.EnableSqlValidation,
                settings.MaxQueryLength,
                settings.MaxQueryComplexity,
                settings.EnableHttpsRedirection,
                settings.EnableSecurityHeaders,
                settings.EnableAuditLogging,
                JwtSecretConfigured = !string.IsNullOrEmpty(settings.JwtSecret)
            };
            
            return Ok(sanitized);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting security settings");
            return StatusCode(500, "Error retrieving security settings");
        }
    }

    /// <summary>
    /// Get performance configuration
    /// </summary>
    [HttpGet("performance")]
    public ActionResult<PerformanceConfiguration> GetPerformanceSettings()
    {
        try
        {
            var settings = _configurationService.GetPerformanceSettings();
            return Ok(settings);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting performance settings");
            return StatusCode(500, "Error retrieving performance settings");
        }
    }

    /// <summary>
    /// Get cache configuration
    /// </summary>
    [HttpGet("cache")]
    public ActionResult<BIReportingCopilot.Core.Configuration.CacheConfiguration> GetCacheSettings()
    {
        try
        {
            var settings = _configurationService.GetCacheSettings();
            // Sanitize connection string
            if (!string.IsNullOrEmpty(settings.RedisConnectionString))
            {
                settings.RedisConnectionString = "***CONFIGURED***";
            }
            return Ok(settings);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting cache settings");
            return StatusCode(500, "Error retrieving cache settings");
        }
    }

    /// <summary>
    /// Get feature flags
    /// </summary>
    [HttpGet("features")]
    public ActionResult<FeatureConfiguration> GetFeatureFlags()
    {
        try
        {
            var settings = _configurationService.GetFeatureFlags();
            return Ok(settings);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting feature flags");
            return StatusCode(500, "Error retrieving feature flags");
        }
    }

    /// <summary>
    /// Validate all configurations
    /// </summary>
    [HttpPost("validate")]
    public async Task<ActionResult<object>> ValidateConfiguration()
    {
        try
        {
            var validationResult = await _configurationService.ValidateAllConfigurationsAsync();
            var migrationResult = await _migrationService.ValidateMigrationAsync();
            
            return Ok(new
            {
                Configuration = validationResult,
                Migration = migrationResult,
                ValidatedAt = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating configuration");
            return StatusCode(500, "Error validating configuration");
        }
    }

    /// <summary>
    /// Reload configuration cache
    /// </summary>
    [HttpPost("reload")]
    public ActionResult ReloadConfiguration()
    {
        try
        {
            _configurationService.ReloadConfiguration();
            _logger.LogInformation("Configuration cache reloaded by admin user");
            return Ok(new { Message = "Configuration cache reloaded successfully", ReloadedAt = DateTime.UtcNow });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reloading configuration");
            return StatusCode(500, "Error reloading configuration");
        }
    }

    /// <summary>
    /// Refresh specific configuration section
    /// </summary>
    [HttpPost("sections/{sectionName}/refresh")]
    public ActionResult RefreshSection(string sectionName)
    {
        try
        {
            if (!_configurationService.SectionExists(sectionName))
            {
                return NotFound($"Configuration section '{sectionName}' not found");
            }

            _configurationService.RefreshSection(sectionName);
            _logger.LogInformation("Configuration section {SectionName} refreshed by admin user", sectionName);
            
            return Ok(new 
            { 
                Message = $"Configuration section '{sectionName}' refreshed successfully", 
                RefreshedAt = DateTime.UtcNow 
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing configuration section {SectionName}", sectionName);
            return StatusCode(500, $"Error refreshing configuration section '{sectionName}'");
        }
    }

    /// <summary>
    /// Get migration status
    /// </summary>
    [HttpGet("migration/status")]
    public ActionResult<object> GetMigrationStatus()
    {
        try
        {
            var status = _migrationService.GetMigrationStatus();
            return Ok(status);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting migration status");
            return StatusCode(500, "Error retrieving migration status");
        }
    }
}
