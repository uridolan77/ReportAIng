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
            .Configure(config =>
            {
                // Bind JWT settings from JwtSettings section
                var jwtSection = configuration.GetSection("JwtSettings");
                config.JwtSecret = jwtSection["Secret"] ?? string.Empty;
                config.JwtIssuer = jwtSection["Issuer"] ?? string.Empty;
                config.JwtAudience = jwtSection["Audience"] ?? string.Empty;
                config.AccessTokenExpirationMinutes = jwtSection.GetValue<int>("AccessTokenExpirationMinutes", 60);
                config.RefreshTokenExpirationMinutes = jwtSection.GetValue<int>("RefreshTokenExpirationMinutes", 43200);

                // Bind other security settings from Application:Security section
                var securitySection = configuration.GetSection("Application:Security");
                securitySection.Bind(config);
            })
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
            .Configure(config =>
            {
                // Bind connection string from ConnectionStrings section
                config.ConnectionString = configuration.GetConnectionString("DefaultConnection") ?? string.Empty;

                // Bind other database settings from Application:Database section
                var databaseSection = configuration.GetSection("Application:Database");
                databaseSection.Bind(config);
            })
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
    /// Validates AI-specific settings (allows empty/test configurations)
    /// </summary>
    private static bool ValidateOpenAISettings(AIConfiguration settings)
    {
        // Basic validation for AI configuration structure
        if (settings.Core == null)
        {
            return false;
        }

        // Validate core AI settings
        if (settings.Core.MaxRetries < 0 || settings.Core.MaxRetries > 10)
        {
            return false;
        }

        if (settings.Core.Timeout <= TimeSpan.Zero || settings.Core.Timeout > TimeSpan.FromMinutes(5))
        {
            return false;
        }

        // Validate semantic cache settings if enabled
        if (settings.SemanticCache?.EnableVectorSearch == true)
        {
            if (settings.SemanticCache.SimilarityThreshold < 0 || settings.SemanticCache.SimilarityThreshold > 1)
            {
                return false;
            }
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
            // Validate JWT settings (critical for authentication)
            ValidateJwtSettings(configuration);

            // Validate connection strings (critical for database access)
            ValidateConnectionStrings(configuration);

            // Validate optional sections with fallbacks
            ValidateOptionalSections(configuration);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Critical configuration validation failed. Please check your appsettings.json file.", ex);
        }
    }

    /// <summary>
    /// Validates JWT settings which are critical for authentication
    /// </summary>
    private static void ValidateJwtSettings(IConfiguration configuration)
    {
        var jwtSection = configuration.GetSection("JwtSettings");

        var secret = jwtSection["Secret"];
        var issuer = jwtSection["Issuer"];
        var audience = jwtSection["Audience"];

        if (string.IsNullOrWhiteSpace(secret))
        {
            throw new InvalidOperationException("JWT Secret is required in JwtSettings section");
        }

        if (secret.Length < 32)
        {
            throw new InvalidOperationException("JWT Secret must be at least 32 characters long");
        }

        if (string.IsNullOrWhiteSpace(issuer))
        {
            throw new InvalidOperationException("JWT Issuer is required in JwtSettings section");
        }

        if (string.IsNullOrWhiteSpace(audience))
        {
            throw new InvalidOperationException("JWT Audience is required in JwtSettings section");
        }
    }

    /// <summary>
    /// Validates optional configuration sections with fallbacks
    /// </summary>
    private static void ValidateOptionalSections(IConfiguration configuration)
    {
        // These sections are optional and will use defaults if not configured
        try
        {
            // Cache settings - optional, will use in-memory cache if not configured
            var cacheSection = configuration.GetSection("Cache");
            if (cacheSection.Exists())
            {
                configuration.GetValidatedSection<CacheConfiguration>("Cache");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Warning: Cache configuration validation failed: {ex.Message}. Using default cache settings.");
        }

        try
        {
            // Performance settings - optional, will use defaults
            var perfSection = configuration.GetSection("Performance");
            if (perfSection.Exists())
            {
                configuration.GetValidatedSection<PerformanceConfiguration>("Performance");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Warning: Performance configuration validation failed: {ex.Message}. Using default performance settings.");
        }

        try
        {
            // Monitoring settings - optional, will use defaults
            var monitoringSection = configuration.GetSection("Monitoring");
            if (monitoringSection.Exists())
            {
                configuration.GetValidatedSection<MonitoringConfiguration>("Monitoring");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Warning: Monitoring configuration validation failed: {ex.Message}. Using default monitoring settings.");
        }

        // AI validation is optional - service will use fallback if not configured
        try
        {
            var aiSection = configuration.GetSection("AI");
            if (aiSection.Exists())
            {
                configuration.GetValidatedSection<AIConfiguration>("AI");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Warning: AI configuration validation failed: {ex.Message}. Service will use fallback responses.");
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
        try
        {
            var cacheSection = configuration.GetSection("Cache");
            var enableDistributedCache = cacheSection.GetValue<bool>("EnableDistributedCache", false);

            if (enableDistributedCache)
            {
                var redisConnection = connectionStrings["Redis"] ?? cacheSection["RedisConnectionString"];
                if (string.IsNullOrWhiteSpace(redisConnection))
                {
                    Console.WriteLine("Warning: Distributed caching is enabled but Redis connection string is not configured. Falling back to in-memory cache.");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Warning: Could not validate Redis configuration: {ex.Message}. Using in-memory cache.");
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
