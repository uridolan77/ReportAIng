using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using BIReportingCopilot.Core.Configuration;

namespace BIReportingCopilot.Infrastructure.Configuration;

/// <summary>
/// Service to help migrate from IConfiguration/IOptions to ConfigurationService
/// Provides backward compatibility during the migration period
/// </summary>
public class ConfigurationMigrationService
{
    private readonly ConfigurationService _unifiedConfig;
    private readonly IConfiguration _legacyConfiguration;
    private readonly ILogger<ConfigurationMigrationService> _logger;

    public ConfigurationMigrationService(
        ConfigurationService unifiedConfig,
        IConfiguration legacyConfiguration,
        ILogger<ConfigurationMigrationService> logger)
    {
        _unifiedConfig = unifiedConfig;
        _legacyConfiguration = legacyConfiguration;
        _logger = logger;
    }

    /// <summary>
    /// Get configuration with automatic migration from legacy patterns
    /// </summary>
    public T GetConfiguration<T>(string? sectionName = null) where T : class, new()
    {
        try
        {
            // Try unified configuration first
            if (!string.IsNullOrEmpty(sectionName))
            {
                return _unifiedConfig.GetConfiguration<T>(sectionName);
            }

            // Handle specific configuration types
            return typeof(T).Name switch
            {
                nameof(AIConfiguration) => (T)(object)_unifiedConfig.GetAISettings(),
                nameof(SecurityConfiguration) => (T)(object)_unifiedConfig.GetSecuritySettings(),
                nameof(DatabaseConfiguration) => (T)(object)_unifiedConfig.GetDatabaseSettings(),
                nameof(PerformanceConfiguration) => (T)(object)_unifiedConfig.GetPerformanceSettings(),
                nameof(CacheConfiguration) => (T)(object)_unifiedConfig.GetCacheSettings(),
                nameof(MonitoringConfiguration) => (T)(object)_unifiedConfig.GetMonitoringSettings(),
                nameof(FeatureConfiguration) => (T)(object)_unifiedConfig.GetFeatureFlags(),
                nameof(NotificationConfiguration) => (T)(object)_unifiedConfig.GetNotificationSettings(),
                nameof(ApplicationSettings) => (T)(object)_unifiedConfig.GetApplicationSettings(),
                _ => throw new NotSupportedException($"Configuration type {typeof(T).Name} is not supported by migration service")
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting configuration for type {ConfigurationType}", typeof(T).Name);
            throw;
        }
    }

    /// <summary>
    /// Get configuration value with fallback to legacy configuration
    /// </summary>
    public string GetConfigurationValue(string key, string defaultValue = "")
    {
        try
        {
            // Try to get from legacy configuration first for backward compatibility
            var value = _legacyConfiguration[key];
            if (!string.IsNullOrEmpty(value))
            {
                return value;
            }

            return defaultValue;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error getting configuration value for key {Key}, returning default", key);
            return defaultValue;
        }
    }

    /// <summary>
    /// Get connection string with fallback
    /// </summary>
    public string GetConnectionString(string name)
    {
        try
        {
            // Try unified configuration first
            var dbConfig = _unifiedConfig.GetDatabaseSettings();
            if (!string.IsNullOrEmpty(dbConfig.ConnectionString) && name == "DefaultConnection")
            {
                return dbConfig.ConnectionString;
            }

            // Fallback to legacy configuration
            return _legacyConfiguration.GetConnectionString(name) ?? string.Empty;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting connection string {Name}", name);
            return string.Empty;
        }
    }

    /// <summary>
    /// Create IOptions wrapper for legacy services
    /// </summary>
    public IOptions<T> CreateOptionsWrapper<T>() where T : class, new()
    {
        try
        {
            var config = GetConfiguration<T>();
            return new OptionsWrapper<T>(config);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating options wrapper for type {ConfigurationType}", typeof(T).Name);
            throw;
        }
    }

    /// <summary>
    /// Validate migration compatibility
    /// </summary>
    public async Task<MigrationValidationResult> ValidateMigrationAsync()
    {
        var result = new MigrationValidationResult();

        try
        {
            // Test each configuration type
            var testTypes = new[]
            {
                typeof(AIConfiguration),
                typeof(SecurityConfiguration),
                typeof(DatabaseConfiguration),
                typeof(PerformanceConfiguration),
                typeof(CacheConfiguration),
                typeof(MonitoringConfiguration),
                typeof(FeatureConfiguration),
                typeof(NotificationConfiguration),
                typeof(ApplicationSettings)
            };

            foreach (var type in testTypes)
            {
                try
                {
                    var method = typeof(ConfigurationMigrationService)
                        .GetMethod(nameof(GetConfiguration))!
                        .MakeGenericMethod(type);
                    
                    method.Invoke(this, new object?[] { null });
                    
                    result.SuccessfulMigrations.Add(type.Name);
                }
                catch (Exception ex)
                {
                    result.FailedMigrations.Add(type.Name, ex.Message);
                    result.IsValid = false;
                }
            }

            _logger.LogInformation("Migration validation completed. Success: {SuccessCount}, Failed: {FailedCount}",
                result.SuccessfulMigrations.Count, result.FailedMigrations.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during migration validation");
            result.IsValid = false;
            result.FailedMigrations.Add("ValidationProcess", ex.Message);
        }

        return result;
    }

    /// <summary>
    /// Get migration status for monitoring
    /// </summary>
    public MigrationStatus GetMigrationStatus()
    {
        return new MigrationStatus
        {
            UnifiedConfigurationAvailable = _unifiedConfig != null,
            LegacyConfigurationAvailable = _legacyConfiguration != null,
            MigrationServiceActive = true,
            LastChecked = DateTime.UtcNow
        };
    }
}

/// <summary>
/// Simple options wrapper for backward compatibility
/// </summary>
public class OptionsWrapper<T> : IOptions<T> where T : class
{
    public T Value { get; }

    public OptionsWrapper(T value)
    {
        Value = value;
    }
}

/// <summary>
/// Migration validation result
/// </summary>
public class MigrationValidationResult
{
    public bool IsValid { get; set; } = true;
    public List<string> SuccessfulMigrations { get; set; } = new();
    public Dictionary<string, string> FailedMigrations { get; set; } = new();
    public DateTime ValidatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Migration status information
/// </summary>
public class MigrationStatus
{
    public bool UnifiedConfigurationAvailable { get; set; }
    public bool LegacyConfigurationAvailable { get; set; }
    public bool MigrationServiceActive { get; set; }
    public DateTime LastChecked { get; set; }
}
