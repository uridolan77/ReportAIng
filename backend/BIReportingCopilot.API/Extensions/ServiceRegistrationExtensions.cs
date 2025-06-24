using BIReportingCopilot.Core.Interfaces;
using BIReportingCopilot.Core.Interfaces.AI;
using BIReportingCopilot.Core.Interfaces.Repository;
using BIReportingCopilot.Core.Interfaces.Services;
using BIReportingCopilot.Core.Interfaces.Query;
using BIReportingCopilot.Core.Interfaces.Security;
using BIReportingCopilot.Core.Interfaces.Streaming;
using BIReportingCopilot.Core.Interfaces.Visualization;
using BIReportingCopilot.Core.Interfaces.Business;
using BIReportingCopilot.Core.Interfaces.Messaging;
using BIReportingCopilot.Core.Interfaces.Cache;
using BIReportingCopilot.Core.Interfaces.Schema;
using BIReportingCopilot.Core.Interfaces.Tuning;
using BIReportingCopilot.Core.Interfaces.BusinessContext;
using BIReportingCopilot.Infrastructure.AI.Management;
using BIReportingCopilot.Infrastructure.Authentication;
using BIReportingCopilot.Infrastructure.Business;
using BIReportingCopilot.Infrastructure.Query;
using BIReportingCopilot.Infrastructure.Schema;
using BIReportingCopilot.Infrastructure.Visualization;
using BIReportingCopilot.Infrastructure.Messaging;
using BIReportingCopilot.Infrastructure.Security;
using BIReportingCopilot.Infrastructure.Performance;
using BIReportingCopilot.Infrastructure.AI;
using BIReportingCopilot.Infrastructure.Repositories;
using BIReportingCopilot.Infrastructure.Data;
using BIReportingCopilot.Infrastructure.Configuration;
using BIReportingCopilot.Infrastructure.BusinessContext;
using BIReportingCopilot.Infrastructure.BusinessContext.Enhanced;
using Microsoft.AspNetCore.SignalR;
using BIReportingCopilot.API.Hubs;

namespace BIReportingCopilot.API.Extensions;

/// <summary>
/// Extension methods for organizing service registrations
/// </summary>
public static class ServiceRegistrationExtensions
{
    /// <summary>
    /// Add core domain services
    /// </summary>
    public static IServiceCollection AddCoreDomainServices(this IServiceCollection services)
    {
        // Repository Layer
        services.AddScoped<UserRepository>();
        services.AddScoped<IUserRepository>(provider => provider.GetRequiredService<UserRepository>());
        services.AddScoped<BIReportingCopilot.Infrastructure.Repositories.IUserEntityRepository>(provider => provider.GetRequiredService<UserRepository>());
        services.AddScoped<BIReportingCopilot.Infrastructure.Interfaces.IUserRepository>(provider => provider.GetRequiredService<UserRepository>());
          services.AddScoped<ITokenRepository, TokenRepository>();
        services.AddScoped<BIReportingCopilot.Infrastructure.Interfaces.ITokenRepository, TokenRepository>();
        services.AddScoped<IMfaChallengeRepository, MfaChallengeRepository>();

        // Configuration services
        services.AddScoped<IConnectionStringProvider, SecureConnectionStringProvider>();

        // Core Business Services
        services.AddScoped<IQueryService, BIReportingCopilot.Infrastructure.Query.QueryService>();
        services.AddScoped<ISchemaService, BIReportingCopilot.Infrastructure.Schema.SchemaService>();
        services.AddScoped<ISqlQueryService, BIReportingCopilot.Infrastructure.Query.SqlQueryService>();
        // IPromptService is registered in RepositoryRegistrationExtensions with TrackedPromptService decorator

        return services;
    }

    /// <summary>
    /// Add AI and ML services
    /// </summary>
    public static IServiceCollection AddAIServices(this IServiceCollection services)
    {
        // AI Providers & Factory
        services.AddScoped<BIReportingCopilot.Infrastructure.AI.Providers.OpenAIProvider>();
        services.AddScoped<BIReportingCopilot.Infrastructure.AI.Providers.AzureOpenAIProvider>();
        services.AddScoped<IAIProviderFactory, BIReportingCopilot.Infrastructure.AI.Providers.AIProviderFactory>();

        // LLM Management Services
        services.AddScoped<ILLMManagementService, LLMManagementService>();

        // AI Core Services
        services.AddScoped<BIReportingCopilot.Infrastructure.AI.Core.AIService>();
        services.AddScoped<ILLMAwareAIService, BIReportingCopilot.Infrastructure.AI.Core.LLMAwareAIService>();
        services.AddScoped<IAIService>(provider => provider.GetRequiredService<ILLMAwareAIService>());

        // Query Processing
        services.AddScoped<IQueryProcessor, BIReportingCopilot.Infrastructure.AI.Core.QueryProcessor>();
        services.AddScoped<BIReportingCopilot.Core.Interfaces.IQueryOptimizer, BIReportingCopilot.Infrastructure.AI.Analysis.QueryOptimizer>();
        services.AddScoped<BIReportingCopilot.Core.Interfaces.IQueryClassifier>(provider =>
            provider.GetRequiredService<BIReportingCopilot.Infrastructure.AI.Analysis.QueryAnalysisService>());

        // Analysis Services
        services.AddScoped<BIReportingCopilot.Infrastructure.AI.Analysis.QueryAnalysisService>();
        services.AddScoped<BIReportingCopilot.Core.Interfaces.AI.ISemanticAnalyzer>(provider =>
            provider.GetRequiredService<BIReportingCopilot.Infrastructure.AI.Analysis.QueryAnalysisService>());

        // Management Services
        services.AddScoped<BIReportingCopilot.Infrastructure.AI.Management.PromptManagementService>();
        services.AddScoped<BIReportingCopilot.Core.Interfaces.IContextManager>(provider =>
            provider.GetRequiredService<BIReportingCopilot.Infrastructure.AI.Management.PromptManagementService>());

        // Intelligence Services
        services.AddScoped<BIReportingCopilot.Core.Interfaces.Query.ISchemaOptimizationService, BIReportingCopilot.Infrastructure.AI.Intelligence.OptimizationService>();
        services.AddScoped<IVectorSearchService, BIReportingCopilot.Infrastructure.AI.Intelligence.InMemoryVectorSearchService>();
        services.AddScoped<IQueryIntelligenceService, BIReportingCopilot.Infrastructure.AI.Intelligence.IntelligenceService>();
        services.AddScoped<BIReportingCopilot.Core.Interfaces.AI.IAdvancedNLUService, BIReportingCopilot.Infrastructure.AI.Intelligence.NLUService>();

        // Caching Services
        services.AddScoped<BIReportingCopilot.Core.Interfaces.AI.ISemanticCacheService, BIReportingCopilot.Infrastructure.AI.Caching.SemanticCacheService>();

        // Learning Services
        services.AddScoped<BIReportingCopilot.Infrastructure.AI.Core.LearningService>();

        return services;
    }

    /// <summary>
    /// Add security services
    /// </summary>
    public static IServiceCollection AddSecurityServices(this IServiceCollection services)
    {
        // Unified SQL Validator
        services.AddScoped<BIReportingCopilot.Infrastructure.Security.SqlQueryValidator>();
        services.AddScoped<BIReportingCopilot.Infrastructure.Security.ISqlQueryValidator>(provider =>
            provider.GetRequiredService<BIReportingCopilot.Infrastructure.Security.SqlQueryValidator>());
        services.AddScoped<BIReportingCopilot.Infrastructure.Security.IEnhancedSqlQueryValidator>(provider =>
            provider.GetRequiredService<BIReportingCopilot.Infrastructure.Security.SqlQueryValidator>());

        // Authentication & Authorization
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IAuthenticationService, AuthenticationService>();
        services.AddScoped<BIReportingCopilot.Core.Interfaces.Security.IMfaService, BIReportingCopilot.Infrastructure.Authentication.MfaService>();
        services.AddScoped<BIReportingCopilot.Core.Interfaces.Security.IPasswordHasher, BIReportingCopilot.Infrastructure.Security.PasswordHasher>();

        // Security Management
        services.AddScoped<SecurityManagementService>();
        services.AddScoped<BIReportingCopilot.Infrastructure.Security.IRateLimitingService>(provider =>
            provider.GetRequiredService<SecurityManagementService>());
        services.AddScoped<BIReportingCopilot.Infrastructure.Security.ISecretsManagementService, BIReportingCopilot.Infrastructure.Security.SecretsManagementService>();

        return services;
    }

    /// <summary>
    /// Add business domain services
    /// </summary>
    public static IServiceCollection AddBusinessServices(this IServiceCollection services)
    {
        // Schema Management
        services.AddScoped<BIReportingCopilot.Infrastructure.Interfaces.ISchemaManagementService, BIReportingCopilot.Infrastructure.Schema.SchemaManagementService>();
        services.AddScoped<BIReportingCopilot.Infrastructure.Schema.DatabaseSchemaDiscoveryService>();
        services.AddScoped<BIReportingCopilot.Core.Interfaces.Schema.IBusinessSchemaService, BIReportingCopilot.Infrastructure.Schema.BusinessSchemaService>();

        // Business Management
        services.AddScoped<IBusinessTableManagementService, BusinessTableManagementService>();
        services.AddScoped<IGlossaryManagementService, GlossaryManagementService>();
        services.AddScoped<BIReportingCopilot.Core.Interfaces.Business.IAITuningSettingsService, AITuningSettingsService>();

        // Query Management
        services.AddScoped<BIReportingCopilot.Infrastructure.Query.QueryPatternManagementService>();
        services.AddScoped<BIReportingCopilot.Core.Interfaces.Business.IQueryPatternManagementService, BIReportingCopilot.Infrastructure.Business.BusinessQueryPatternManagementService>();
        services.AddScoped<IQueryCacheService, BIReportingCopilot.Infrastructure.Query.QueryCacheService>();
        services.AddScoped<IQuerySuggestionService, BIReportingCopilot.Infrastructure.Query.QuerySuggestionService>();

        // AI Context
        services.AddScoped<IBusinessContextAutoGenerator, BIReportingCopilot.Infrastructure.AI.Management.BusinessContextAutoGenerator>();

        // Tuning Services - Registered in Program.cs

        return services;
    }

    /// <summary>
    /// Add Business Context Aware Prompt Building services
    /// </summary>
    public static IServiceCollection AddBusinessContextServices(this IServiceCollection services)
    {
        // Business Context Analysis
        services.AddScoped<IBusinessContextAnalyzer, BusinessContextAnalyzer>();
        services.AddScoped<IBusinessMetadataRetrievalService, BusinessMetadataRetrievalService>();
        services.AddScoped<IContextualPromptBuilder, ContextualPromptBuilder>();

        // Enhanced Business Context Analysis - Register dependencies first
        services.AddScoped<IIntentClassificationEnsemble, IntentClassificationEnsemble>();
        services.AddScoped<IEntityExtractionPipeline, EntityExtractionPipeline>();
        services.AddScoped<IAdvancedDomainDetector, AdvancedDomainDetector>();
        services.AddScoped<IConfidenceValidationSystem, ConfidenceValidationSystem>();
        services.AddScoped<IBusinessTermExtractor, BusinessTermExtractor>();
        services.AddScoped<ITimeContextAnalyzer, TimeContextAnalyzer>();
        services.AddScoped<IUserFeedbackLearner, UserFeedbackLearner>();

        // Additional dependencies for EntityExtractionPipeline
        services.AddScoped<IBusinessTermMatcher, BusinessTermMatcher>();
        services.AddScoped<ISemanticEntityLinker, SemanticEntityLinker>();

        // Additional dependencies for ConfidenceValidationSystem
        services.AddScoped<BIReportingCopilot.Core.Interfaces.BusinessContext.IUserFeedbackRepository, UserFeedbackRepository>();

        // Token Budget Management
        services.AddScoped<ITokenBudgetManager, TokenBudgetManager>();

        // Register the main enhanced analyzer
        services.AddScoped<IEnhancedBusinessContextAnalyzer, EnhancedBusinessContextAnalyzer>();

        // Semantic Matching (placeholder - would need actual implementation)
        services.AddScoped<ISemanticMatchingService, SemanticMatchingService>();

        // HttpClient for external API calls (OpenAI, etc.)
        services.AddHttpClient();

        // Vector Embedding Service for semantic search
        services.AddScoped<IVectorEmbeddingService, VectorEmbeddingService>();

        return services;
    }

    /// <summary>
    /// Add performance and monitoring services
    /// </summary>
    public static IServiceCollection AddPerformanceServices(this IServiceCollection services)
    {
        // Performance Management
        services.AddScoped<PerformanceManagementService>();

        // Monitoring
        services.AddScoped<BIReportingCopilot.Infrastructure.Monitoring.MonitoringManagementService>();
        services.AddScoped<BIReportingCopilot.Core.Interfaces.Monitoring.IMetricsCollector>(provider =>
            provider.GetRequiredService<BIReportingCopilot.Infrastructure.Monitoring.MonitoringManagementService>());

        // Health Management
        services.AddScoped<BIReportingCopilot.Infrastructure.Health.HealthManagementService>();

        // Background Jobs
        services.AddScoped<BIReportingCopilot.Infrastructure.Jobs.BackgroundJobManagementService>();

        // Auto-optimization
        services.AddHostedService<BIReportingCopilot.Infrastructure.Performance.AutoOptimizationService>();

        return services;
    }

    /// <summary>
    /// Add streaming and real-time services
    /// </summary>
    public static IServiceCollection AddStreamingServices(this IServiceCollection services)
    {
        // Real-time streaming
        services.AddScoped<IRealTimeStreamingService, BIReportingCopilot.Infrastructure.AI.Streaming.StreamingService>();

        // Streaming SQL Query Service
        services.AddScoped<IStreamingSqlQueryService>(provider =>
        {
            var innerService = provider.GetRequiredService<ISqlQueryService>();
            var connectionStringProvider = provider.GetRequiredService<IConnectionStringProvider>();
            var logger = provider.GetRequiredService<ILogger<StreamingSqlQueryService>>();

            string connectionString;
            try
            {
                connectionString = connectionStringProvider.GetConnectionStringAsync("BIDatabase").GetAwaiter().GetResult();
            }
            catch
            {
                connectionString = provider.GetRequiredService<IConfiguration>().GetConnectionString("DefaultConnection") ?? "Data Source=:memory:";
            }

            return new StreamingSqlQueryService(innerService, connectionString, logger);
        });

        return services;
    }

    /// <summary>
    /// Add dashboard and visualization services
    /// </summary>
    public static IServiceCollection AddVisualizationServices(this IServiceCollection services)
    {
        // Dashboard Services
        services.AddScoped<BIReportingCopilot.Infrastructure.AI.Dashboard.DashboardCreationService>();
        services.AddScoped<BIReportingCopilot.Infrastructure.AI.Dashboard.DashboardTemplateService>();
        services.AddScoped<IMultiModalDashboardService, BIReportingCopilot.Infrastructure.AI.Dashboard.MultiModalDashboardService>();

        return services;
    }

    /// <summary>
    /// Add messaging and notification services
    /// </summary>
    public static IServiceCollection AddMessagingServices(this IServiceCollection services)
    {
        // Event Bus
        services.AddScoped<BIReportingCopilot.Infrastructure.Messaging.IEventBus, BIReportingCopilot.Infrastructure.Messaging.InMemoryEventBus>();

        // Unified Notification Management
        services.AddScoped<BIReportingCopilot.Infrastructure.Messaging.NotificationManagementService>();
        services.AddScoped<IEmailService>(provider =>
            provider.GetRequiredService<BIReportingCopilot.Infrastructure.Messaging.NotificationManagementService>());
        services.AddScoped<ISmsService>(provider =>
            provider.GetRequiredService<BIReportingCopilot.Infrastructure.Messaging.NotificationManagementService>());

        // Progress Reporting
        services.AddScoped<IQueryProgressNotifier, BIReportingCopilot.API.Hubs.NoOpQueryProgressNotifier>();
        services.AddScoped<IProgressReporter>(provider =>
        {
            var hubContext = provider.GetRequiredService<IHubContext<BIReportingCopilot.API.Hubs.QueryStatusHub>>();
            var logger = provider.GetRequiredService<ILogger<BIReportingCopilot.Infrastructure.Messaging.SignalRProgressReporter>>();
            return new BIReportingCopilot.Infrastructure.Messaging.SignalRProgressReporter(hubContext, logger);
        });

        // Event Handlers
        services.AddScoped<BIReportingCopilot.Infrastructure.Messaging.EventHandlers.QueryExecutedEventHandler>();
        services.AddScoped<BIReportingCopilot.Infrastructure.Messaging.EventHandlers.FeedbackReceivedEventHandler>();
        services.AddScoped<BIReportingCopilot.Infrastructure.Messaging.EventHandlers.AnomalyDetectedEventHandler>();
        services.AddScoped<BIReportingCopilot.Infrastructure.Messaging.EventHandlers.CacheInvalidatedEventHandler>();
        services.AddScoped<BIReportingCopilot.Infrastructure.Messaging.EventHandlers.PerformanceMetricsEventHandler>();

        return services;
    }

    /// <summary>
    /// Add infrastructure and data services
    /// </summary>
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
    {
        // Core Infrastructure
        services.AddScoped<IAuditService, BIReportingCopilot.Infrastructure.Data.AuditService>();
        services.AddScoped<BIReportingCopilot.Core.Interfaces.Security.IAuditService, BIReportingCopilot.Infrastructure.Data.AuditService>();
        services.AddScoped<IDatabaseInitializationService, BIReportingCopilot.Infrastructure.Data.DatabaseInitializationService>();

        // Database Context Management
        services.AddScoped<BIReportingCopilot.Infrastructure.Data.Contexts.IDbContextFactory, BIReportingCopilot.Infrastructure.Data.Contexts.DbContextFactory>();
        services.AddScoped<BIReportingCopilot.Infrastructure.Data.Migration.ContextMigrationService>();
        services.AddScoped<BIReportingCopilot.Infrastructure.Data.Migration.ServiceMigrationHelper>();
        services.AddScoped<BIReportingCopilot.Infrastructure.Data.Migration.MigrationStatusTracker>();

        // Configuration Services
        services.AddSingleton<BIReportingCopilot.Infrastructure.Configuration.ConfigurationService>();
        services.AddSingleton<BIReportingCopilot.Infrastructure.Configuration.ConfigurationMigrationService>();

        // Phase 3 Services
        services.AddScoped<BIReportingCopilot.Infrastructure.AI.Management.Phase3StatusService>();

        return services;
    }
}
