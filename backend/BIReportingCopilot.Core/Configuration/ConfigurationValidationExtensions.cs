using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.ComponentModel.DataAnnotations;

namespace BIReportingCopilot.Core.Configuration;

/// <summary>
/// Extensions for configuration validation and registration
/// </summary>
public static class ConfigurationValidationExtensions
{
    /// <summary>
    /// Adds and validates all application configuration sections using unified configuration models
    /// </summary>
    public static IServiceCollection AddValidatedConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        // Register and validate unified AI configuration
        services.AddOptions<AIConfiguration>()
            .Bind(configuration.GetSection("AI"))
            .ValidateDataAnnotations()
            .Validate(ValidateOpenAISettings, "AI configuration is invalid");

        // Register and validate unified security configuration
        services.AddOptions<SecurityConfiguration>()
            .Bind(configuration.GetSection("Security"))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        // Register and validate unified performance configuration
        services.AddOptions<PerformanceConfiguration>()
            .Bind(configuration.GetSection("Performance"))
            .ValidateDataAnnotations()
            .Validate(ValidateQuerySettings, "Performance configuration is invalid")
            .ValidateOnStart();

        // Register and validate unified cache configuration
        services.AddOptions<CacheConfiguration>()
            .Bind(configuration.GetSection("Cache"))
            .ValidateDataAnnotations()
            .Validate(ValidateCacheSettings, "Cache configuration is invalid")
            .ValidateOnStart();

        // Register and validate unified database configuration
        services.AddOptions<DatabaseConfiguration>()
            .Bind(configuration.GetSection("Database"))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        // Register and validate unified monitoring configuration
        services.AddOptions<MonitoringConfiguration>()
            .Bind(configuration.GetSection("Monitoring"))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        // Register unified feature configuration (no validation needed)
        services.AddOptions<FeatureConfiguration>()
            .Bind(configuration.GetSection("Features"));

        return services;
    }

    /// <summary>
    /// Validates OpenAI-specific settings (allows empty/test configurations)
    /// </summary>
    private static bool ValidateOpenAISettings(AIConfiguration settings)
    {
        // Allow empty API key for testing/development
        if (string.IsNullOrEmpty(settings.OpenAIApiKey) || settings.OpenAIApiKey == "test-api-key")
        {
            return true;
        }

        // Validate API key format (basic check) for real keys
        if (!settings.OpenAIApiKey.StartsWith("sk-"))
        {
            return false;
        }

        // Validate model name
        var validModels = new[] { "gpt-3.5-turbo", "gpt-4", "gpt-4-turbo", "gpt-4o" };
        if (!validModels.Contains(settings.OpenAIModel))
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// Validates query-specific settings
    /// </summary>
    private static bool ValidateQuerySettings(PerformanceConfiguration settings)
    {
        // Ensure query timeout is reasonable
        if (settings.DefaultQueryTimeoutSeconds <= 0 || settings.DefaultQueryTimeoutSeconds > 300)
        {
            return false;
        }

        // Validate max rows per query
        if (settings.MaxRowsPerQuery <= 0 || settings.MaxRowsPerQuery > 1000000)
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// Validates cache-specific settings
    /// </summary>
    private static bool ValidateCacheSettings(CacheConfiguration settings)
    {
        // If distributed cache is enabled, Redis connection string must be provided
        if (settings.EnableDistributedCache && string.IsNullOrWhiteSpace(settings.RedisConnectionString))
        {
            return false;
        }

        // Schema cache should be longer than query cache
        if (settings.SchemaCacheExpirationMinutes < settings.QueryCacheExpirationMinutes)
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// Validates configuration using data annotations
    /// </summary>
    public static void ValidateConfiguration<T>(T configuration) where T : class
    {
        var context = new ValidationContext(configuration);
        var results = new List<ValidationResult>();

        if (!Validator.TryValidateObject(configuration, context, results, true))
        {
            var errors = results.Select(r => r.ErrorMessage).ToList();
            throw new InvalidOperationException($"Configuration validation failed for {typeof(T).Name}: {string.Join(", ", errors)}");
        }
    }

    /// <summary>
    /// Gets a validated configuration section
    /// </summary>
    public static T GetValidatedSection<T>(this IConfiguration configuration, string sectionName) where T : class, new()
    {
        var section = configuration.GetSection(sectionName);
        var config = section.Get<T>() ?? new T();

        ValidateConfiguration(config);

        return config;
    }

    /// <summary>
    /// Validates all critical configuration sections at startup
    /// </summary>
    public static void ValidateStartupConfiguration(this IConfiguration configuration)
    {
        try
        {
            // Validate all critical unified configuration sections
            configuration.GetValidatedSection<SecurityConfiguration>("Security");
            configuration.GetValidatedSection<DatabaseConfiguration>("Database");
            configuration.GetValidatedSection<PerformanceConfiguration>("Performance");
            configuration.GetValidatedSection<CacheConfiguration>("Cache");
            configuration.GetValidatedSection<MonitoringConfiguration>("Monitoring");

            // AI validation is optional - service will use fallback if not configured
            try
            {
                configuration.GetValidatedSection<AIConfiguration>("AI");
            }
            catch (Exception ex)
            {
                // Log warning but don't fail startup for AI configuration issues
                Console.WriteLine($"Warning: AI configuration validation failed: {ex.Message}. Service will use fallback responses.");
            }

            // Validate connection strings
            ValidateConnectionStrings(configuration);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Critical configuration validation failed. Please check your appsettings.json file.", ex);
        }
    }

    /// <summary>
    /// Validates required connection strings
    /// </summary>
    private static void ValidateConnectionStrings(IConfiguration configuration)
    {
        var connectionStrings = configuration.GetSection("ConnectionStrings");

        var defaultConnection = connectionStrings["DefaultConnection"];
        if (string.IsNullOrWhiteSpace(defaultConnection))
        {
            throw new InvalidOperationException("DefaultConnection connection string is required");
        }

        // Validate Redis connection if distributed caching is enabled
        var cacheSettings = configuration.GetSection("Cache").Get<CacheConfiguration>();
        if (cacheSettings?.EnableDistributedCache == true)
        {
            var redisConnection = connectionStrings["Redis"] ?? cacheSettings.RedisConnectionString;
            if (string.IsNullOrWhiteSpace(redisConnection))
            {
                throw new InvalidOperationException("Redis connection string is required when distributed caching is enabled");
            }
        }
    }

    /// <summary>
    /// Extension to safely get configuration values with defaults
    /// </summary>
    public static T GetValueOrDefault<T>(this IConfiguration configuration, string key, T defaultValue)
    {
        try
        {
            var value = configuration.GetValue<T>(key);
            return value ?? defaultValue;
        }
        catch
        {
            return defaultValue;
        }
    }

    /// <summary>
    /// Checks if a feature flag is enabled
    /// </summary>
    public static bool IsFeatureEnabled(this IConfiguration configuration, string featureName)
    {
        var featureFlags = configuration.GetSection("Features").Get<FeatureConfiguration>();

        return featureName switch
        {
            "EnableQuerySuggestions" => featureFlags?.EnableQuerySuggestions ?? true,
            "EnableAutoVisualization" => featureFlags?.EnableAutoVisualization ?? true,
            "EnableRealTimeUpdates" => featureFlags?.EnableRealTimeUpdates ?? true,
            "EnableAdvancedAnalytics" => featureFlags?.EnableAdvancedAnalytics ?? false,
            "EnableDataExport" => featureFlags?.EnableDataExport ?? true,
            "EnableSchemaInference" => featureFlags?.EnableSchemaInference ?? false,
            "EnableMachineLearning" => featureFlags?.EnableMachineLearning ?? true,
            "EnableAIOptimization" => featureFlags?.EnableAIOptimization ?? true,
            "EnableExperimentalFeatures" => featureFlags?.EnableExperimentalFeatures ?? false,
            _ => false
        };
    }
}
