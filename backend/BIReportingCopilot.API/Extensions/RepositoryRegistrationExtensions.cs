using BIReportingCopilot.Core.Interfaces.Repository;
using BIReportingCopilot.Core.Interfaces.Security;
using BIReportingCopilot.Core.Interfaces.Analytics;
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

        // Template Management Repositories
        services.AddScoped<ITemplateImprovementRepository, TemplateImprovementSuggestionRepository>();
        services.AddScoped<IEnhancedPromptTemplateRepository, EnhancedPromptTemplateRepository>();

        // Template Management Services
        services.AddScoped<ITemplatePerformanceService, BIReportingCopilot.Infrastructure.Services.TemplatePerformanceService>();
        services.AddScoped<IABTestingService, BIReportingCopilot.Infrastructure.Services.ABTestingService>();
        services.AddScoped<ITemplateManagementService, BIReportingCopilot.Infrastructure.Services.TemplateManagementService>();

        // Template Usage Tracking Integration
        services.AddScoped<BIReportingCopilot.Infrastructure.Services.TrackedPromptService>();
        services.AddScoped<BIReportingCopilot.Infrastructure.Extensions.TemplatePerformanceMonitoringService>();

        // Background Services
        services.AddHostedService<BIReportingCopilot.Infrastructure.Services.ABTestAutomationService>();
        services.AddHostedService<BIReportingCopilot.API.Hubs.AnalyticsUpdateService>();

        return services;
    }
}
