using BIReportingCopilot.Core.Configuration;
using BIReportingCopilot.Core.Interfaces.Cache;
using BIReportingCopilot.Infrastructure.Performance;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace BIReportingCopilot.API.Extensions;

/// <summary>
/// Extension methods for caching service registration
/// </summary>
public static class CacheServiceExtensions
{
    /// <summary>
    /// Add caching services with Redis support
    /// </summary>
    public static IServiceCollection AddCachingServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Configure Cache settings
        services.Configure<CacheConfiguration>(configuration.GetSection("Cache"));

        // Conditionally register Redis services based on configuration
        var cacheConfig = configuration.GetSection("Cache").Get<CacheConfiguration>();
        if (cacheConfig?.EnableRedis == true)
        {
            // Register Redis connection multiplexer
            services.AddSingleton<IConnectionMultiplexer>(provider =>
            {
                var config = provider.GetRequiredService<IOptions<CacheConfiguration>>().Value;
                var connectionString = config.GetRedisConnectionStringWithOptions();

                if (string.IsNullOrEmpty(connectionString))
                {
                    throw new InvalidOperationException("Redis is enabled but no valid connection string is configured");
                }

                return ConnectionMultiplexer.Connect(connectionString);
            });

            // Register Redis distributed cache
            services.AddStackExchangeRedisCache(options =>
            {
                var config = cacheConfig.GetRedisConnectionStringWithOptions();
                if (!string.IsNullOrEmpty(config))
                {
                    options.Configuration = config;
                }
            });
        }
        else
        {
            // Register a null implementation for IConnectionMultiplexer when Redis is disabled
            services.AddSingleton<IConnectionMultiplexer>(provider => null!);

            // Use in-memory distributed cache when Redis is disabled
            services.AddDistributedMemoryCache();
        }        // Always register IMemoryCache for local caching needs
        services.AddMemoryCache();

        // Unified cache service with built-in distributed caching support
        services.AddSingleton<ICacheService, CacheService>();

        return services;
    }
}
