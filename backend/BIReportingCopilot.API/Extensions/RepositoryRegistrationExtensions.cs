using BIReportingCopilot.Core.Interfaces.Repository;
using BIReportingCopilot.Infrastructure.Repositories;

namespace BIReportingCopilot.API.Extensions;

/// <summary>
/// Extension methods for repository service registrations
/// </summary>
public static class RepositoryRegistrationExtensions
{
    /// <summary>
    /// Register all repository services with proper interface mappings
    /// </summary>
    public static IServiceCollection AddRepositoryServices(this IServiceCollection services)
    {
        // Unified UserRepository implements multiple interfaces
        services.AddScoped<UserRepository>();
        services.AddScoped<IUserRepository>(provider => provider.GetRequiredService<UserRepository>());
        services.AddScoped<IUserEntityRepository>(provider => provider.GetRequiredService<UserRepository>());

        // Token Repository 
        services.AddScoped<TokenRepository>();
        services.AddScoped<ITokenRepository>(provider => provider.GetRequiredService<TokenRepository>());

        // MFA Challenge Repository
        services.AddScoped<IMfaChallengeRepository, MfaChallengeRepository>();

        return services;
    }
}
