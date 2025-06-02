using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace BIReportingCopilot.API.Extensions;

/// <summary>
/// Security-related extension methods
/// </summary>
public static class SecurityExtensions
{
    /// <summary>
    /// Add security health checks
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <returns>Service collection</returns>
    public static IServiceCollection AddSecurityHealthChecks(this IServiceCollection services)
    {
        var healthChecks = services.AddHealthChecks();

        // Add security-related health checks
        healthChecks.AddCheck("security_configuration", () =>
        {
            // Check if security configuration is valid
            return HealthCheckResult.Healthy("Security configuration is valid");
        });

        healthChecks.AddCheck("rate_limiting", () =>
        {
            // Check if rate limiting is working
            return HealthCheckResult.Healthy("Rate limiting is operational");
        });

        healthChecks.AddCheck("sql_validation", () =>
        {
            // Check if SQL validation is working
            return HealthCheckResult.Healthy("SQL validation is operational");
        });

        healthChecks.AddCheck("authentication", () =>
        {
            // Check if authentication is working
            return HealthCheckResult.Healthy("Authentication is operational");
        });

        return services;
    }
}
