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
    /// Adds and validates all application configuration sections
    /// </summary>
    public static IServiceCollection AddValidatedConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        // Register and validate JWT settings
        services.AddOptions<JwtSettings>()
            .Bind(configuration.GetSection(JwtSettings.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        // Register and validate OpenAI settings (optional - fallback behavior if not configured)
        services.AddOptions<OpenAISettings>()
            .Bind(configuration.GetSection(OpenAISettings.SectionName))
            .ValidateDataAnnotations()
            .Validate(ValidateOpenAISettings, "OpenAI configuration is invalid");

        // Register and validate rate limit settings
        services.AddOptions<RateLimitSettings>()
            .Bind(configuration.GetSection(RateLimitSettings.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        // Register and validate query settings
        services.AddOptions<QuerySettings>()
            .Bind(configuration.GetSection(QuerySettings.SectionName))
            .ValidateDataAnnotations()
            .Validate(ValidateQuerySettings, "Query configuration is invalid")
            .ValidateOnStart();

        // Register and validate security settings
        services.AddOptions<SecuritySettings>()
            .Bind(configuration.GetSection(SecuritySettings.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        // Register and validate cache settings
        services.AddOptions<CacheSettings>()
            .Bind(configuration.GetSection(CacheSettings.SectionName))
            .ValidateDataAnnotations()
            .Validate(ValidateCacheSettings, "Cache configuration is invalid")
            .ValidateOnStart();

        // Register and validate background job settings
        services.AddOptions<BackgroundJobSettings>()
            .Bind(configuration.GetSection(BackgroundJobSettings.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        // Register feature flags (no validation needed)
        services.AddOptions<FeatureFlagSettings>()
            .Bind(configuration.GetSection(FeatureFlagSettings.SectionName));

        return services;
    }

    /// <summary>
    /// Validates OpenAI-specific settings (allows empty/test configurations)
    /// </summary>
    private static bool ValidateOpenAISettings(OpenAISettings settings)
    {
        // Allow empty API key for testing/development
        if (string.IsNullOrEmpty(settings.ApiKey) || settings.ApiKey == "test-api-key")
        {
            return true;
        }

        // Validate API key format (basic check) for real keys
        if (!settings.ApiKey.StartsWith("sk-"))
        {
            return false;
        }

        // Validate model name
        var validModels = new[] { "gpt-3.5-turbo", "gpt-4", "gpt-4-turbo", "gpt-4o" };
        if (!validModels.Contains(settings.Model))
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// Validates query-specific settings
    /// </summary>
    private static bool ValidateQuerySettings(QuerySettings settings)
    {
        // Ensure cache expiration is reasonable compared to timeout
        if (settings.CacheExpirationSeconds < settings.DefaultTimeoutSeconds)
        {
            return false;
        }

        // Validate blocked keywords are not empty if validation is enabled
        if (settings.EnableQueryValidation && !settings.BlockedKeywords.Any())
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// Validates cache-specific settings
    /// </summary>
    private static bool ValidateCacheSettings(CacheSettings settings)
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
            // Validate all critical sections
            configuration.GetValidatedSection<JwtSettings>(JwtSettings.SectionName);

            // OpenAI validation is optional - service will use fallback if not configured
            try
            {
                configuration.GetValidatedSection<OpenAISettings>(OpenAISettings.SectionName);
            }
            catch (Exception ex)
            {
                // Log warning but don't fail startup for OpenAI configuration issues
                Console.WriteLine($"Warning: OpenAI configuration validation failed: {ex.Message}. Service will use fallback responses.");
            }

            configuration.GetValidatedSection<QuerySettings>(QuerySettings.SectionName);
            configuration.GetValidatedSection<SecuritySettings>(SecuritySettings.SectionName);
            configuration.GetValidatedSection<CacheSettings>(CacheSettings.SectionName);
            configuration.GetValidatedSection<BackgroundJobSettings>(BackgroundJobSettings.SectionName);

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
        var cacheSettings = configuration.GetSection(CacheSettings.SectionName).Get<CacheSettings>();
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
        var featureFlags = configuration.GetSection(FeatureFlagSettings.SectionName).Get<FeatureFlagSettings>();

        return featureName switch
        {
            "EnableQuerySuggestions" => featureFlags?.EnableQuerySuggestions ?? true,
            "EnableAutoVisualization" => featureFlags?.EnableAutoVisualization ?? true,
            "EnableRealTimeUpdates" => featureFlags?.EnableRealTimeUpdates ?? true,
            "EnableAdvancedAnalytics" => featureFlags?.EnableAdvancedAnalytics ?? false,
            "EnableDataExport" => featureFlags?.EnableDataExport ?? true,
            "EnableSchemaInference" => featureFlags?.EnableSchemaInference ?? false,
            "EnableAuditLogging" => featureFlags?.EnableAuditLogging ?? true,
            "EnablePerformanceMetrics" => featureFlags?.EnablePerformanceMetrics ?? true,
            "EnableHealthChecks" => featureFlags?.EnableHealthChecks ?? true,
            _ => false
        };
    }
}
