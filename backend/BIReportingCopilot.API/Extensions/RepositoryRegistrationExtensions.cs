using BIReportingCopilot.Core.Interfaces.Repository;
using BIReportingCopilot.Core.Interfaces.Security;
using BIReportingCopilot.Infrastructure.Repositories;
using BIReportingCopilot.Infrastructure.Interfaces;

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
        services.AddScoped<BIReportingCopilot.Core.Interfaces.Repository.IUserRepository>(provider => provider.GetRequiredService<UserRepository>());
        services.AddScoped<BIReportingCopilot.Infrastructure.Interfaces.IUserRepository>(provider => provider.GetRequiredService<UserRepository>());
        services.AddScoped<IUserEntityRepository>(provider => provider.GetRequiredService<UserRepository>());

        // Token Repository
        services.AddScoped<TokenRepository>();
        services.AddScoped<BIReportingCopilot.Core.Interfaces.Repository.ITokenRepository>(provider => provider.GetRequiredService<TokenRepository>());
        services.AddScoped<BIReportingCopilot.Infrastructure.Interfaces.ITokenRepository>(provider => provider.GetRequiredService<TokenRepository>());

        // MFA Challenge Repository
        services.AddScoped<IMfaChallengeRepository, MfaChallengeRepository>();

        return services;
    }
}
