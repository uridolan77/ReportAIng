using BIReportingCopilot.Core.Interfaces.Cache;

namespace BIReportingCopilot.Core.Extensions;

/// <summary>
/// Extension methods for ICacheService to provide TryGetAsync functionality
/// </summary>
public static class CacheServiceExtensions
{
    /// <summary>
    /// Tries to get a reference type value from cache, returning true if found
    /// </summary>
    public static async Task<(bool found, T? value)> TryGetAsync<T>(this ICacheService cacheService, string key, CancellationToken cancellationToken = default) where T : class
    {
        try
        {
            var value = await cacheService.GetAsync<T>(key, cancellationToken);
            return (value != null, value);
        }
        catch
        {
            return (false, null);
        }
    }

    /// <summary>
    /// Tries to get a value type from cache, returning true if found
    /// </summary>
    public static async Task<(bool found, T value)> TryGetValueAsync<T>(this ICacheService cacheService, string key, CancellationToken cancellationToken = default) where T : struct
    {
        try
        {
            var boxedValue = await cacheService.GetAsync<object>(key, cancellationToken);
            if (boxedValue is T value)
            {
                return (true, value);
            }
            return (false, default(T));
        }
        catch
        {
            return (false, default(T));
        }
    }
}
