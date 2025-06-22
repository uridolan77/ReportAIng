// Program.cs or ServiceConfiguration.cs
namespace ReportAIng.API
{
    public static class ServiceConfiguration
    {
        public static IServiceCollection AddBusinessContextServices(
            this IServiceCollection services, 
            IConfiguration configuration)
        {
            // Core Services
            services.AddScoped<IBusinessContextAnalyzer, BusinessContextAnalyzer>();
            services.AddScoped<IBusinessMetadataRetrievalService, BusinessMetadataRetrievalService>();
            services.AddScoped<IContextualPromptBuilder, ContextualPromptBuilder>();
            services.AddScoped<ISemanticMatchingService, SemanticMatchingService>();

            // Repository Registration
            services.AddScoped<IBusinessTableRepository, BusinessTableRepository>();
            services.AddScoped<IBusinessColumnRepository, BusinessColumnRepository>();
            services.AddScoped<IBusinessGlossaryRepository, BusinessGlossaryRepository>();
            services.AddScoped<IBusinessDomainRepository, BusinessDomainRepository>();
            services.AddScoped<IPromptTemplateRepository, PromptTemplateRepository>();
            services.AddScoped<IQueryExampleRepository, QueryExampleRepository>();

            // Configuration
            services.Configure<BusinessMetadataConfig>(
                configuration.GetSection("BusinessMetadata"));
            services.Configure<PromptBuilderConfig>(
                configuration.GetSection("PromptBuilder"));
            services.Configure<SemanticSearchConfig>(
                configuration.GetSection("SemanticSearch"));

            // MediatR Handlers
            services.AddMediatR(cfg => {
                cfg.RegisterServicesFromAssembly(typeof(GenerateBusinessPromptHandler).Assembly);
                cfg.AddBehavior<IPipelineBehavior<GenerateBusinessPromptCommand, BusinessPromptResult>, 
                    LoggingBehavior<GenerateBusinessPromptCommand, BusinessPromptResult>>();
                cfg.AddBehavior<IPipelineBehavior<GenerateBusinessPromptCommand, BusinessPromptResult>, 
                    ValidationBehavior<GenerateBusinessPromptCommand, BusinessPromptResult>>();
            });

            // Validators
            services.AddValidatorsFromAssemblyContaining<BusinessPromptRequestValidator>();

            // Background Services
            services.AddHostedService<PromptOptimizationService>();
            services.AddHostedService<MetadataSyncService>();

            // Caching
            services.AddMemoryCache();
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = configuration.GetConnectionString("Redis");
                options.InstanceName = "BIReportingCopilot";
            });

            // Health Checks
            services.AddHealthChecks()
                .AddDbContextCheck<BIReportingContext>("database")
                .AddCheck<BusinessMetadataHealthCheck>("business_metadata")
                .AddCheck<AIServicesHealthCheck>("ai_services");

            return services;
        }
    }
}

// Application/Behaviors/LoggingBehavior.cs
namespace ReportAIng.Application.Behaviors
{
    public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;
        private readonly ICurrentUserService _currentUserService;

        public LoggingBehavior(
            ILogger<LoggingBehavior<TRequest, TResponse>> logger,
            ICurrentUserService currentUserService)
        {
            _logger = logger;
            _currentUserService = currentUserService;
        }

        public async Task<TResponse> Handle(
            TRequest request, 
            RequestHandlerDelegate<TResponse> next, 
            CancellationToken cancellationToken)
        {
            var requestName = typeof(TRequest).Name;
            var userId = _currentUserService.UserId ?? "Anonymous";
            
            _logger.LogInformation("Handling {RequestName} for user {UserId}", 
                requestName, userId);

            var stopwatch = Stopwatch.StartNew();
            
            try
            {
                var response = await next();
                
                stopwatch.Stop();
                
                _logger.LogInformation("Handled {RequestName} in {ElapsedMs}ms", 
                    requestName, stopwatch.ElapsedMilliseconds);
                
                return response;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                
                _logger.LogError(ex, "Error handling {RequestName} after {ElapsedMs}ms", 
                    requestName, stopwatch.ElapsedMilliseconds);
                
                throw;
            }
        }
    }
}

// Application/Behaviors/ValidationBehavior.cs
namespace ReportAIng.Application.Behaviors
{
    public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        private readonly IEnumerable<IValidator<TRequest>> _validators;

        public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
        {
            _validators = validators;
        }

        public async Task<TResponse> Handle(
            TRequest request, 
            RequestHandlerDelegate<TResponse> next, 
            CancellationToken cancellationToken)
        {
            if (_validators.Any())
            {
                var context = new ValidationContext<TRequest>(request);
                
                var validationResults = await Task.WhenAll(
                    _validators.Select(v => v.ValidateAsync(context, cancellationToken)));
                
                var failures = validationResults
                    .SelectMany(r => r.Errors)
                    .Where(f => f != null)
                    .ToList();

                if (failures.Count != 0)
                {
                    throw new ValidationException(failures);
                }
            }
            
            return await next();
        }
    }
}

// Infrastructure/Services/PromptOptimizationService.cs
namespace ReportAIng.Infrastructure.Services
{
    public class PromptOptimizationService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<PromptOptimizationService> _logger;
        private readonly IConfiguration _configuration;
        private readonly TimeSpan _optimizationInterval;

        public PromptOptimizationService(
            IServiceProvider serviceProvider,
            ILogger<PromptOptimizationService> logger,
            IConfiguration configuration)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _configuration = configuration;
            _optimizationInterval = TimeSpan.FromHours(
                configuration.GetValue<int>("PromptOptimization:IntervalHours", 6));
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await OptimizePromptsAsync(stoppingToken);
                    await Task.Delay(_optimizationInterval, stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in prompt optimization service");
                    await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
                }
            }
        }

        private async Task OptimizePromptsAsync(CancellationToken cancellationToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var usageLogRepository = scope.ServiceProvider.GetRequiredService<ILLMUsageLogRepository>();
            var promptTemplateRepository = scope.ServiceProvider.GetRequiredService<IPromptTemplateRepository>();
            
            _logger.LogInformation("Starting prompt optimization");

            // Analyze usage patterns
            var usageLogs = await usageLogRepository.GetRecentLogsAsync(days: 7);
            var successRateByTemplate = AnalyzeSuccessRates(usageLogs);

            // Update template priorities based on success rates
            foreach (var (templateKey, successRate) in successRateByTemplate)
            {
                if (successRate < 0.7) // Below 70% success rate
                {
                    _logger.LogWarning("Template {TemplateKey} has low success rate: {Rate:P}", 
                        templateKey, successRate);
                    
                    // Could trigger automated improvements or alerts
                }
            }

            // Identify common failure patterns
            var failurePatterns = IdentifyFailurePatterns(usageLogs);
            foreach (var pattern in failurePatterns)
            {
                _logger.LogInformation("Common failure pattern identified: {Pattern}", pattern);
            }

            _logger.LogInformation("Prompt optimization completed");
        }

        private Dictionary<string, double> AnalyzeSuccessRates(List<LLMUsageLog> logs)
        {
            return logs
                .GroupBy(l => l.TemplateKey)
                .ToDictionary(
                    g => g.Key ?? "Unknown",
                    g => g.Count(l => l.WasSuccessful) / (double)g.Count()
                );
        }

        private List<string> IdentifyFailurePatterns(List<LLMUsageLog> logs)
        {
            // Analyze failed queries for common patterns
            var failedLogs = logs.Where(l => !l.WasSuccessful);
            // Implementation would use pattern recognition
            return new List<string>();
        }
    }
}

// Infrastructure/Services/MetadataSyncService.cs
namespace ReportAIng.Infrastructure.Services
{
    public class MetadataSyncService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<MetadataSyncService> _logger;
        private readonly TimeSpan _syncInterval;

        public MetadataSyncService(
            IServiceProvider serviceProvider,
            ILogger<MetadataSyncService> logger,
            IConfiguration configuration)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _syncInterval = TimeSpan.FromHours(
                configuration.GetValue<int>("MetadataSync:IntervalHours", 24));
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await SyncMetadataAsync(stoppingToken);
                    await Task.Delay(_syncInterval, stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in metadata sync service");
                    await Task.Delay(TimeSpan.FromMinutes(30), stoppingToken);
                }
            }
        }

        private async Task SyncMetadataAsync(CancellationToken cancellationToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var tableRepository = scope.ServiceProvider.GetRequiredService<IBusinessTableRepository>();
            var semanticService = scope.ServiceProvider.GetRequiredService<ISemanticMatchingService>();
            
            _logger.LogInformation("Starting metadata synchronization");

            // Update vector embeddings for tables
            var tables = await tableRepository.GetActiveTablesAsync(1000);
            
            foreach (var table in tables)
            {
                if (cancellationToken.IsCancellationRequested) break;

                try
                {
                    // Generate embedding for semantic search
                    var embeddingText = $"{table.BusinessPurpose} {table.BusinessContext} " +
                                      $"{table.NaturalLanguageDescription}";
                    
                    var embedding = await semanticService.GenerateEmbeddingAsync(embeddingText);
                    
                    // Store embedding (implementation would save to database or vector store)
                    await StoreTableEmbeddingAsync(table.Id, embedding);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error generating embedding for table {TableId}", table.Id);
                }
            }

            _logger.LogInformation("Metadata synchronization completed");
        }

        private async Task StoreTableEmbeddingAsync(long tableId, double[] embedding)
        {
            // Implementation would store embeddings in a vector database
            await Task.CompletedTask;
        }
    }
}

// Infrastructure/HealthChecks/BusinessMetadataHealthCheck.cs
namespace ReportAIng.Infrastructure.HealthChecks
{
    public class BusinessMetadataHealthCheck : IHealthCheck
    {
        private readonly IBusinessTableRepository _tableRepository;
        private readonly IBusinessGlossaryRepository _glossaryRepository;

        public BusinessMetadataHealthCheck(
            IBusinessTableRepository tableRepository,
            IBusinessGlossaryRepository glossaryRepository)
        {
            _tableRepository = tableRepository;
            _glossaryRepository = glossaryRepository;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            try
            {
                // Check if we have active tables
                var activeTables = await _tableRepository.GetActiveTablesAsync(1);
                if (!activeTables.Any())
                {
                    return HealthCheckResult.Degraded("No active business tables found");
                }

                // Check if we have glossary terms
                var glossaryTerms = await _glossaryRepository.GetActiveTermsAsync();
                if (!glossaryTerms.Any())
                {
                    return HealthCheckResult.Degraded("No active glossary terms found");
                }

                var data = new Dictionary<string, object>
                {
                    ["ActiveTables"] = activeTables.Count,
                    ["GlossaryTerms"] = glossaryTerms.Count
                };

                return HealthCheckResult.Healthy("Business metadata is healthy", data);
            }
            catch (Exception ex)
            {
                return HealthCheckResult.Unhealthy("Business metadata check failed", ex);
            }
        }
    }
}

// Infrastructure/HealthChecks/AIServicesHealthCheck.cs
namespace ReportAIng.Infrastructure.HealthChecks
{
    public class AIServicesHealthCheck : IHealthCheck
    {
        private readonly IOpenAIService _openAIService;
        private readonly IConfiguration _configuration;

        public AIServicesHealthCheck(
            IOpenAIService openAIService,
            IConfiguration configuration)
        {
            _openAIService = openAIService;
            _configuration = configuration;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            try
            {
                // Test OpenAI connectivity
                var testPrompt = "Test prompt for health check";
                var response = await _openAIService.GetCompletionAsync(
                    testPrompt, 
                    new AIModelConfig { MaxTokens = 10 });

                if (string.IsNullOrEmpty(response))
                {
                    return HealthCheckResult.Degraded("AI service returned empty response");
                }

                return HealthCheckResult.Healthy("AI services are operational");
            }
            catch (Exception ex)
            {
                return HealthCheckResult.Unhealthy("AI service check failed", ex);
            }
        }
    }
}