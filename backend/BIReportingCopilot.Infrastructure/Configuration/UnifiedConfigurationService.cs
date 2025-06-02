using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using BIReportingCopilot.Core.Configuration;

namespace BIReportingCopilot.Infrastructure.Configuration;

/// <summary>
/// Unified configuration service consolidating all configuration management
/// Replaces ApplicationSettings, ConfigurationModels, and related configuration files
/// </summary>
public class UnifiedConfigurationService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<UnifiedConfigurationService> _logger;
    private readonly Dictionary<string, object> _configurationCache;
    private readonly object _cacheLock = new();

    public UnifiedConfigurationService(
        IConfiguration configuration,
        ILogger<UnifiedConfigurationService> logger)
    {
        _configuration = configuration;
        _logger = logger;
        _configurationCache = new Dictionary<string, object>();
    }

    /// <summary>
    /// Get configuration section with validation and caching
    /// </summary>
    public T GetConfiguration<T>(string sectionName) where T : class, new()
    {
        lock (_cacheLock)
        {
            var cacheKey = $"{typeof(T).Name}_{sectionName}";

            if (_configurationCache.TryGetValue(cacheKey, out var cachedValue))
            {
                return (T)cachedValue;
            }

            var config = new T();
            _configuration.GetSection(sectionName).Bind(config);

            // Validate configuration
            var validationResults = ValidateConfiguration(config);
            if (validationResults.Any())
            {
                var errors = string.Join(", ", validationResults.Select(r => r.ErrorMessage));
                _logger.LogError("Configuration validation failed for {SectionName}: {Errors}", sectionName, errors);
                throw new InvalidOperationException($"Configuration validation failed for {sectionName}: {errors}");
            }

            _configurationCache[cacheKey] = config;
            _logger.LogDebug("Configuration loaded and cached for {SectionName}", sectionName);

            return config;
        }
    }

    /// <summary>
    /// Get application settings with all unified configuration
    /// </summary>
    public UnifiedApplicationSettings GetApplicationSettings()
    {
        return GetConfiguration<UnifiedApplicationSettings>("Application");
    }

    /// <summary>
    /// Get AI settings
    /// </summary>
    public AIConfiguration GetAISettings()
    {
        return GetConfiguration<AIConfiguration>("AI");
    }

    /// <summary>
    /// Get security settings
    /// </summary>
    public SecurityConfiguration GetSecuritySettings()
    {
        lock (_cacheLock)
        {
            var cacheKey = $"{typeof(SecurityConfiguration).Name}_Security";

            if (_configurationCache.TryGetValue(cacheKey, out var cachedValue))
            {
                return (SecurityConfiguration)cachedValue;
            }

            var config = new SecurityConfiguration();

            // Bind JWT settings from JwtSettings section
            var jwtSection = _configuration.GetSection("JwtSettings");
            config.JwtSecret = jwtSection["Secret"] ?? string.Empty;
            config.JwtIssuer = jwtSection["Issuer"] ?? string.Empty;
            config.JwtAudience = jwtSection["Audience"] ?? string.Empty;
            config.AccessTokenExpirationMinutes = jwtSection.GetValue<int>("AccessTokenExpirationMinutes", 60);
            config.RefreshTokenExpirationMinutes = jwtSection.GetValue<int>("RefreshTokenExpirationMinutes", 43200);

            // Bind other security settings from Application:Security section
            var securitySection = _configuration.GetSection("Application:Security");
            securitySection.Bind(config);

            // Validate configuration
            var validationResults = ValidateConfiguration(config);
            if (validationResults.Any())
            {
                var errors = string.Join(", ", validationResults.Select(r => r.ErrorMessage));
                _logger.LogError("Configuration validation failed for Security: {Errors}", errors);
                throw new InvalidOperationException($"Configuration validation failed for Security: {errors}");
            }

            _configurationCache[cacheKey] = config;
            _logger.LogDebug("Configuration loaded and cached for Security");

            return config;
        }
    }

    /// <summary>
    /// Get database settings
    /// </summary>
    public DatabaseConfiguration GetDatabaseSettings()
    {
        lock (_cacheLock)
        {
            var cacheKey = $"{typeof(DatabaseConfiguration).Name}_Database";

            if (_configurationCache.TryGetValue(cacheKey, out var cachedValue))
            {
                return (DatabaseConfiguration)cachedValue;
            }

            var config = new DatabaseConfiguration();

            // Bind connection string from ConnectionStrings section
            // Note: This will be resolved by the SecureConnectionStringProvider during DI configuration
            config.ConnectionString = _configuration.GetConnectionString("DefaultConnection") ?? string.Empty;

            // Bind other database settings from Application:Database section
            var databaseSection = _configuration.GetSection("Application:Database");
            databaseSection.Bind(config);

            // Validate configuration
            var validationResults = ValidateConfiguration(config);
            if (validationResults.Any())
            {
                var errors = string.Join(", ", validationResults.Select(r => r.ErrorMessage));
                _logger.LogError("Configuration validation failed for Database: {Errors}", errors);
                throw new InvalidOperationException($"Configuration validation failed for Database: {errors}");
            }

            _configurationCache[cacheKey] = config;
            _logger.LogDebug("Configuration loaded and cached for Database");

            return config;
        }
    }

    /// <summary>
    /// Get performance settings
    /// </summary>
    public PerformanceConfiguration GetPerformanceSettings()
    {
        return GetConfiguration<PerformanceConfiguration>("Performance");
    }



    /// <summary>
    /// Get caching settings
    /// </summary>
    public CacheConfiguration GetCacheSettings()
    {
        return GetConfiguration<CacheConfiguration>("Cache");
    }

    /// <summary>
    /// Get monitoring settings
    /// </summary>
    public MonitoringConfiguration GetMonitoringSettings()
    {
        return GetConfiguration<MonitoringConfiguration>("Monitoring");
    }

    /// <summary>
    /// Get feature flags
    /// </summary>
    public FeatureConfiguration GetFeatureFlags()
    {
        return GetConfiguration<FeatureConfiguration>("Features");
    }

    /// <summary>
    /// Get notification settings
    /// </summary>
    public NotificationConfiguration GetNotificationSettings()
    {
        return GetConfiguration<NotificationConfiguration>("Notifications");
    }

    /// <summary>
    /// Validate all configurations
    /// </summary>
    public async Task<ConfigurationValidationResult> ValidateAllConfigurationsAsync()
    {
        var result = new ConfigurationValidationResult();

        try
        {
            // Validate each configuration section
            var validationTasks = new[]
            {
                ValidateConfigurationSection<UnifiedApplicationSettings>("Application"),
                ValidateConfigurationSection<AIConfiguration>("AI"),
                ValidateSecurityConfigurationSection(),
                ValidateConfigurationSection<PerformanceConfiguration>("Performance"),
                ValidateDatabaseConfigurationSection(),
                ValidateConfigurationSection<CacheConfiguration>("Cache"),
                ValidateConfigurationSection<MonitoringConfiguration>("Monitoring"),
                ValidateConfigurationSection<FeatureConfiguration>("Features"),
                ValidateConfigurationSection<NotificationConfiguration>("Notifications")
            };

            var validationResults = await Task.WhenAll(validationTasks);

            foreach (var validationResult in validationResults)
            {
                result.SectionResults.Add(validationResult);
                if (!validationResult.IsValid)
                {
                    result.IsValid = false;
                    result.Errors.AddRange(validationResult.Errors);
                }
            }

            _logger.LogInformation("Configuration validation completed. Valid: {IsValid}, Errors: {ErrorCount}",
                result.IsValid, result.Errors.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during configuration validation");
            result.IsValid = false;
            result.Errors.Add($"Configuration validation failed: {ex.Message}");
        }

        return result;
    }

    /// <summary>
    /// Reload configuration cache
    /// </summary>
    public void ReloadConfiguration()
    {
        lock (_cacheLock)
        {
            _configurationCache.Clear();
            _logger.LogInformation("Configuration cache cleared and will be reloaded on next access");
        }
    }

    /// <summary>
    /// Get configuration as JSON for debugging
    /// </summary>
    public string GetConfigurationAsJson(string sectionName)
    {
        var section = _configuration.GetSection(sectionName);
        var values = section.GetChildren().ToDictionary(x => x.Key, x => x.Value);
        return JsonSerializer.Serialize(values, new JsonSerializerOptions { WriteIndented = true });
    }

    private async Task<ConfigurationSectionValidationResult> ValidateSecurityConfigurationSection()
    {
        var result = new ConfigurationSectionValidationResult
        {
            SectionName = "Security",
            ConfigurationType = typeof(SecurityConfiguration).Name
        };

        try
        {
            var config = GetSecuritySettings();
            var validationResults = ValidateConfiguration(config);

            result.IsValid = !validationResults.Any();
            result.Errors = validationResults.Select(r => r.ErrorMessage ?? "Unknown validation error").ToList();
        }
        catch (Exception ex)
        {
            result.IsValid = false;
            result.Errors.Add($"Failed to load configuration: {ex.Message}");
        }

        return result;
    }

    private async Task<ConfigurationSectionValidationResult> ValidateDatabaseConfigurationSection()
    {
        var result = new ConfigurationSectionValidationResult
        {
            SectionName = "Database",
            ConfigurationType = typeof(DatabaseConfiguration).Name
        };

        try
        {
            var config = GetDatabaseSettings();
            var validationResults = ValidateConfiguration(config);

            result.IsValid = !validationResults.Any();
            result.Errors = validationResults.Select(r => r.ErrorMessage ?? "Unknown validation error").ToList();
        }
        catch (Exception ex)
        {
            result.IsValid = false;
            result.Errors.Add($"Failed to load configuration: {ex.Message}");
        }

        return result;
    }

    private async Task<ConfigurationSectionValidationResult> ValidateConfigurationSection<T>(string sectionName)
        where T : class, new()
    {
        var result = new ConfigurationSectionValidationResult
        {
            SectionName = sectionName,
            ConfigurationType = typeof(T).Name
        };

        try
        {
            var config = GetConfiguration<T>(sectionName);
            var validationResults = ValidateConfiguration(config);

            result.IsValid = !validationResults.Any();
            result.Errors = validationResults.Select(r => r.ErrorMessage ?? "Unknown validation error").ToList();
        }
        catch (Exception ex)
        {
            result.IsValid = false;
            result.Errors.Add($"Failed to load configuration: {ex.Message}");
        }

        return result;
    }

    private List<ValidationResult> ValidateConfiguration<T>(T configuration)
    {
        var validationResults = new List<ValidationResult>();
        var validationContext = new ValidationContext(configuration);

        Validator.TryValidateObject(configuration, validationContext, validationResults, true);

        return validationResults;
    }
}

/// <summary>
/// Configuration validation result
/// </summary>
public class ConfigurationValidationResult
{
    public bool IsValid { get; set; } = true;
    public List<string> Errors { get; set; } = new();
    public List<ConfigurationSectionValidationResult> SectionResults { get; set; } = new();
    public DateTime ValidatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Configuration section validation result
/// </summary>
public class ConfigurationSectionValidationResult
{
    public string SectionName { get; set; } = string.Empty;
    public string ConfigurationType { get; set; } = string.Empty;
    public bool IsValid { get; set; } = true;
    public List<string> Errors { get; set; } = new();
}
