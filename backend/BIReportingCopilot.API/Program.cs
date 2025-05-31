using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Text;
using BIReportingCopilot.Core.Interfaces;
using BIReportingCopilot.Core.Configuration;
using BIReportingCopilot.Core.Constants;
using BIReportingCopilot.Infrastructure.Data;
using BIReportingCopilot.Infrastructure.Services;
using BIReportingCopilot.API.Middleware;
using BIReportingCopilot.API.Hubs;
using BIReportingCopilot.API.HealthChecks;
using BIReportingCopilot.Infrastructure.Health;
using FluentValidation;
using FluentValidation.AspNetCore;
using MediatR;
using BIReportingCopilot.Infrastructure.Configuration;
using BIReportingCopilot.API.Configuration;
using BIReportingCopilot.Infrastructure.Behaviors;
using BIReportingCopilot.Infrastructure.Performance;
using BIReportingCopilot.Infrastructure.Security;
using BIReportingCopilot.Infrastructure.AI;
using BIReportingCopilot.Infrastructure.Middleware;
using BIReportingCopilot.Core.Commands;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Hangfire;
using BIReportingCopilot.API.Versioning;
using BIReportingCopilot.Core.Validation;
using BIReportingCopilot.Infrastructure.Health;

var builder = WebApplication.CreateBuilder(args);

// Configure Key Vault (selective loading to avoid JWT override)
var keyVaultUrl = builder.Configuration["KeyVault:Url"];
if (!string.IsNullOrEmpty(keyVaultUrl))
{
    // Load Key Vault but exclude JWT settings to prevent override
    builder.Configuration.AddAzureKeyVault(
        new Uri(keyVaultUrl),
        new Azure.Identity.DefaultAzureCredential(),
        new SelectiveSecretManager());
    Log.Information("Azure Key Vault configured with selective secret loading: {KeyVaultUrl}", keyVaultUrl);
}

// Use User Secrets in development
if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddUserSecrets<Program>();
}

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.ApplicationInsights(builder.Configuration.GetConnectionString("ApplicationInsights"), TelemetryConverter.Traces)
    .CreateLogger();

builder.Host.UseSerilog();

// Validate critical configuration early
try
{
    builder.Configuration.ValidateStartupConfiguration();
    Log.Information("Configuration validation passed");
}
catch (Exception ex)
{
    Log.Fatal(ex, "Configuration validation failed");
    throw;
}

// Add validated configuration
builder.Services.AddValidatedConfiguration(builder.Configuration);

// Configure OpenAI settings
builder.Services.Configure<BIReportingCopilot.Core.Configuration.OpenAIConfiguration>(
    builder.Configuration.GetSection("OpenAI"));
builder.Services.Configure<BIReportingCopilot.Core.Configuration.AzureOpenAIConfiguration>(
    builder.Configuration.GetSection("AzureOpenAI"));
builder.Services.Configure<BIReportingCopilot.Core.Configuration.AIServiceConfiguration>(config =>
{
    builder.Configuration.GetSection("OpenAI").Bind(config.OpenAI);
    builder.Configuration.GetSection("AzureOpenAI").Bind(config.AzureOpenAI);
});

// JWT settings are already configured by AddValidatedConfiguration above

// Add services to the container
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Configure JSON serialization to use PascalCase to match frontend expectations
        options.JsonSerializerOptions.PropertyNamingPolicy = null; // Use original property names (PascalCase)
        options.JsonSerializerOptions.WriteIndented = builder.Environment.IsDevelopment();
    });

// Add API versioning support
builder.Services.AddApiVersioningSupport();
builder.Services.AddVersionedSwagger();

// Add FluentValidation
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();
builder.Services.AddValidatorsFromAssemblyContaining<QueryRequestValidator>();

// Configure application settings
builder.Services.Configure<BIReportingCopilot.Core.Configuration.ApplicationSettings>(
    builder.Configuration.GetSection(BIReportingCopilot.Core.Configuration.ApplicationSettings.SectionName));

// Configure security settings
builder.Services.Configure<BIReportingCopilot.Infrastructure.Security.RateLimitingConfiguration>(
    builder.Configuration.GetSection("RateLimiting"));
builder.Services.Configure<BIReportingCopilot.Infrastructure.Security.SecretsConfiguration>(
    builder.Configuration.GetSection("Secrets"));
builder.Services.Configure<BIReportingCopilot.Infrastructure.Security.SqlValidationConfiguration>(
    builder.Configuration.GetSection("SqlValidation"));

// Configure Entity Framework
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (!string.IsNullOrEmpty(connectionString))
{
    builder.Services.AddDbContext<BICopilotContext>(options =>
        options.UseSqlServer(connectionString));
}
else
{
    // Use in-memory database for development
    builder.Services.AddDbContext<BICopilotContext>(options =>
        options.UseInMemoryDatabase("BICopilotDev"));
}

// Configure Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["Secret"];

if (string.IsNullOrEmpty(secretKey))
{
    throw new InvalidOperationException($"JWT Secret not found in configuration. Available keys: {string.Join(", ", jwtSettings.GetChildren().Select(x => x.Key))}");
}

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        // Debug: Log the JWT validation settings
        Log.Information("JWT Validation Settings - Issuer: {Issuer}, Audience: {Audience}, SecretLength: {SecretLength}, SecretPrefix: {SecretPrefix}",
            jwtSettings["Issuer"], jwtSettings["Audience"], secretKey?.Length ?? 0, secretKey?.Substring(0, Math.Min(10, secretKey.Length)) + "...");

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(secretKey)),
            ValidateIssuer = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidateAudience = true,
            ValidAudience = jwtSettings["Audience"],
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };

        // Configure JWT for SignalR and add debugging
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
                {
                    context.Token = accessToken;
                }
                return Task.CompletedTask;
            },
            OnAuthenticationFailed = context =>
            {
                Log.Warning("JWT Authentication failed: {Exception}", context.Exception.Message);
                return Task.CompletedTask;
            },
            OnTokenValidated = context =>
            {
                Log.Information("JWT Token validated successfully for user: {User}",
                    context.Principal?.Identity?.Name ?? "Unknown");
                return Task.CompletedTask;
            },
            OnChallenge = context =>
            {
                Log.Warning("JWT Authentication challenge: {Error} - {ErrorDescription}",
                    context.Error, context.ErrorDescription);
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(builder.Configuration.GetSection("AllowedOrigins").Get<string[]>() ?? new[] { "http://localhost:3000" })
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

// Configure SignalR
builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = builder.Environment.IsDevelopment();
    options.KeepAliveInterval = TimeSpan.FromSeconds(15);
    options.ClientTimeoutInterval = TimeSpan.FromSeconds(30);
});

// Configure Redis Cache
var redisConnectionString = builder.Configuration.GetConnectionString("Redis");
if (!string.IsNullOrEmpty(redisConnectionString))
{
    builder.Services.AddStackExchangeRedisCache(options =>
    {
        options.Configuration = redisConnectionString;
        options.InstanceName = "BICopilot";
    });
}
else
{
    builder.Services.AddMemoryCache();
    builder.Services.AddDistributedMemoryCache(); // Fallback for enhanced services
}

// Add OpenAI client for enhanced AI service
builder.Services.AddSingleton(provider =>
{
    var configuration = provider.GetRequiredService<IConfiguration>();
    var logger = provider.GetRequiredService<ILogger<Program>>();

    // Try Azure OpenAI first, then fallback to OpenAI
    var azureConfig = configuration.GetSection("AzureOpenAI");
    var azureEndpoint = azureConfig["Endpoint"];
    var azureApiKey = azureConfig["ApiKey"];

    if (!string.IsNullOrEmpty(azureEndpoint) && !string.IsNullOrEmpty(azureApiKey))
    {
        logger.LogInformation("Configuring Azure OpenAI client with endpoint: {Endpoint}", azureEndpoint);
        return new Azure.AI.OpenAI.OpenAIClient(new Uri(azureEndpoint), new Azure.AzureKeyCredential(azureApiKey));
    }

    // Fallback to regular OpenAI
    var openAIConfig = configuration.GetSection("OpenAI");
    var openAIApiKey = openAIConfig["ApiKey"];

    if (!string.IsNullOrEmpty(openAIApiKey) && openAIApiKey != "your-openai-api-key-here")
    {
        logger.LogInformation("Configuring OpenAI client");
        return new Azure.AI.OpenAI.OpenAIClient(openAIApiKey);
    }

    logger.LogWarning("No valid OpenAI configuration found. AI features will use fallback responses.");
    // Return a mock client for development - this will be handled by the service layer
    return new Azure.AI.OpenAI.OpenAIClient("mock-key");
});

// ===== REPOSITORY LAYER =====
// Unified UserRepository implements both IUserRepository and IUserEntityRepository
builder.Services.AddScoped<BIReportingCopilot.Infrastructure.Repositories.UserRepository>();
builder.Services.AddScoped<IUserRepository>(provider => provider.GetRequiredService<BIReportingCopilot.Infrastructure.Repositories.UserRepository>());
builder.Services.AddScoped<BIReportingCopilot.Infrastructure.Interfaces.IUserEntityRepository>(provider => provider.GetRequiredService<BIReportingCopilot.Infrastructure.Repositories.UserRepository>());
builder.Services.AddScoped<ITokenRepository, BIReportingCopilot.Infrastructure.Repositories.TokenRepository>();
builder.Services.AddScoped<IMfaChallengeRepository, BIReportingCopilot.Infrastructure.Repositories.MfaChallengeRepository>();

// ===== AI PROVIDERS & FACTORY =====
builder.Services.AddScoped<BIReportingCopilot.Infrastructure.AI.Providers.OpenAIProvider>();
builder.Services.AddScoped<BIReportingCopilot.Infrastructure.AI.Providers.AzureOpenAIProvider>();
builder.Services.AddScoped<IAIProviderFactory, BIReportingCopilot.Infrastructure.AI.Providers.AIProviderFactory>();

// ===== AI/ML ENHANCEMENT SERVICES =====
builder.Services.AddScoped<BIReportingCopilot.Infrastructure.AI.ISemanticCacheService, BIReportingCopilot.Infrastructure.AI.SemanticCacheService>();
builder.Services.AddScoped<BIReportingCopilot.Infrastructure.AI.MLAnomalyDetector>();
builder.Services.AddScoped<BIReportingCopilot.Infrastructure.AI.FeedbackLearningEngine>();
builder.Services.AddScoped<BIReportingCopilot.Infrastructure.AI.PromptOptimizer>();
builder.Services.AddScoped<ISemanticAnalyzer, BIReportingCopilot.Infrastructure.AI.SemanticAnalyzer>();
builder.Services.AddScoped<IQueryClassifier, BIReportingCopilot.Infrastructure.AI.QueryClassifier>();
builder.Services.AddScoped<IContextManager, BIReportingCopilot.Infrastructure.AI.ContextManager>();
builder.Services.AddScoped<IQueryOptimizer, BIReportingCopilot.Infrastructure.AI.QueryOptimizer>();
builder.Services.AddScoped<IQueryProcessor, BIReportingCopilot.Infrastructure.AI.EnhancedQueryProcessor>();

// ===== PERFORMANCE & MONITORING =====
builder.Services.AddScoped<BIReportingCopilot.Infrastructure.Performance.StreamingDataService>();
builder.Services.AddSingleton<BIReportingCopilot.Infrastructure.Monitoring.IMetricsCollector, BIReportingCopilot.Infrastructure.Monitoring.MetricsCollector>();

// ===== MESSAGING & EVENTS =====
builder.Services.AddScoped<BIReportingCopilot.Infrastructure.Messaging.IEventBus, BIReportingCopilot.Infrastructure.Messaging.InMemoryEventBus>();
builder.Services.Configure<BIReportingCopilot.Infrastructure.Messaging.EventBusConfiguration>(
    builder.Configuration.GetSection("EventBus"));

// ===== CACHING & REDIS =====
// Configure Redis settings
builder.Services.Configure<BIReportingCopilot.Core.Configuration.RedisConfiguration>(
    builder.Configuration.GetSection("Redis"));

// Conditionally register Redis services based on configuration
var redisConfig = builder.Configuration.GetSection("Redis").Get<BIReportingCopilot.Core.Configuration.RedisConfiguration>();
if (redisConfig?.Enabled == true)
{
    // Register Redis connection multiplexer
    builder.Services.AddSingleton<StackExchange.Redis.IConnectionMultiplexer>(provider =>
    {
        var config = provider.GetRequiredService<IOptions<BIReportingCopilot.Core.Configuration.RedisConfiguration>>().Value;
        var connectionString = config.GetConnectionStringWithOptions();

        if (string.IsNullOrEmpty(connectionString))
        {
            throw new InvalidOperationException("Redis is enabled but no valid connection string is configured");
        }

        return StackExchange.Redis.ConnectionMultiplexer.Connect(connectionString);
    });

    // Register Redis distributed cache
    builder.Services.AddStackExchangeRedisCache(options =>
    {
        var config = redisConfig.GetConnectionStringWithOptions();
        if (!string.IsNullOrEmpty(config))
        {
            options.Configuration = config;
        }
    });

    Log.Information("Redis caching enabled with connection: {ConnectionString}", redisConfig.ConnectionString);
}
else
{
    // Register a null implementation for IConnectionMultiplexer when Redis is disabled
    builder.Services.AddSingleton<StackExchange.Redis.IConnectionMultiplexer>(provider => null!);

    // Use in-memory caching instead
    builder.Services.AddMemoryCache();

    Log.Information("Redis caching disabled - using in-memory caching");
}

// Unified cache service with built-in distributed caching support
builder.Services.AddSingleton<ICacheService, BIReportingCopilot.Infrastructure.Performance.CacheService>();

// ===== CONFIGURATION SECTIONS =====
builder.Services.Configure<BIReportingCopilot.Infrastructure.Security.RateLimitingConfiguration>(
    builder.Configuration.GetSection("RateLimit"));
builder.Services.Configure<BIReportingCopilot.Infrastructure.Services.ShardingConfiguration>(
    builder.Configuration.GetSection("Sharding"));

// ===== EVENT HANDLERS =====
builder.Services.AddScoped<BIReportingCopilot.Infrastructure.Messaging.EventHandlers.QueryExecutedEventHandler>();
builder.Services.AddScoped<BIReportingCopilot.Infrastructure.Messaging.EventHandlers.FeedbackReceivedEventHandler>();
builder.Services.AddScoped<BIReportingCopilot.Infrastructure.Messaging.EventHandlers.AnomalyDetectedEventHandler>();
builder.Services.AddScoped<BIReportingCopilot.Infrastructure.Messaging.EventHandlers.CacheInvalidatedEventHandler>();
builder.Services.AddScoped<BIReportingCopilot.Infrastructure.Messaging.EventHandlers.PerformanceMetricsEventHandler>();

// ===== CORE APPLICATION SERVICES =====
// Base services
builder.Services.AddScoped<IAIService, BIReportingCopilot.Infrastructure.AI.AIService>();
builder.Services.AddScoped<IQueryService, QueryService>();
builder.Services.AddScoped<ISchemaService, BIReportingCopilot.Infrastructure.Services.SchemaService>(); // Unified with built-in caching
builder.Services.AddScoped<ISqlQueryService, BIReportingCopilot.Infrastructure.Services.SqlQueryService>();
builder.Services.AddScoped<IPromptService, BIReportingCopilot.Infrastructure.Services.PromptService>();
builder.Services.AddScoped<IVisualizationService, BIReportingCopilot.Infrastructure.Services.VisualizationService>();

// Streaming query service with factory pattern
builder.Services.AddScoped<IStreamingSqlQueryService>(provider =>
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

// ===== BUSINESS SERVICES =====
builder.Services.AddScoped<IUserService, BIReportingCopilot.Infrastructure.Services.UserService>();
builder.Services.AddScoped<IAuditService, BIReportingCopilot.Infrastructure.Services.AuditService>();
builder.Services.AddScoped<IAuthenticationService, BIReportingCopilot.Infrastructure.Services.AuthenticationService>();
builder.Services.AddScoped<IMfaService, BIReportingCopilot.Infrastructure.Services.MfaService>();
builder.Services.AddScoped<IEmailService, BIReportingCopilot.Infrastructure.Services.EmailService>();
builder.Services.AddScoped<ISmsService, BIReportingCopilot.Infrastructure.Services.SmsService>();

// ===== SECURITY SERVICES =====
builder.Services.AddScoped<BIReportingCopilot.Infrastructure.Security.ISqlQueryValidator, BIReportingCopilot.Infrastructure.Security.SqlQueryValidator>();
builder.Services.AddScoped<BIReportingCopilot.Infrastructure.Security.IRateLimitingService, BIReportingCopilot.Infrastructure.Security.DistributedRateLimitingService>();
builder.Services.AddScoped<BIReportingCopilot.Infrastructure.Security.ISecretsManagementService, BIReportingCopilot.Infrastructure.Security.SecretsManagementService>();
builder.Services.AddScoped<BIReportingCopilot.Infrastructure.Security.IEnhancedSqlQueryValidator, BIReportingCopilot.Infrastructure.Security.EnhancedSqlQueryValidator>();

// ===== SERVICE ENHANCEMENTS =====
// Enhanced AI service with built-in resilience, monitoring, and adaptive capabilities
// (No decorators needed - functionality consolidated into base service)

// Query service enhancements (keeping minimal decorators for now)
builder.Services.Decorate<IQueryService, BIReportingCopilot.Infrastructure.Resilience.ResilientQueryService>();
builder.Services.Decorate<IQueryService, BIReportingCopilot.Infrastructure.Monitoring.TracedQueryService>();

// ===== INFRASTRUCTURE SERVICES =====
builder.Services.AddScoped<BIReportingCopilot.Core.Interfaces.IPasswordHasher, BIReportingCopilot.Infrastructure.Security.PasswordHasher>();
builder.Services.AddScoped<IConnectionStringProvider, SecureConnectionStringProvider>();
builder.Services.AddScoped<IDatabaseInitializationService, DatabaseInitializationService>();
builder.Services.AddScoped<ITuningService, TuningService>();
builder.Services.AddScoped<IAITuningSettingsService, AITuningSettingsService>();
builder.Services.AddScoped<IBusinessContextAutoGenerator, BusinessContextAutoGenerator>();

// ===== STARTUP & HEALTH SERVICES =====
builder.Services.AddSingleton<IStartupHealthValidator, StartupHealthValidator>();
builder.Services.AddHostedService<StartupValidationService>();

// ===== FRAMEWORK SERVICES =====
// AutoMapper
builder.Services.AddAutoMapper(typeof(Program), typeof(BIReportingCopilot.Infrastructure.Mapping.QueryHistoryMappingProfile));

// MediatR with assemblies
builder.Services.AddMediatR(cfg => {
    cfg.RegisterServicesFromAssembly(typeof(ExecuteQueryCommand).Assembly);
    cfg.RegisterServicesFromAssembly(typeof(Program).Assembly);
    cfg.RegisterServicesFromAssembly(typeof(BIReportingCopilot.Infrastructure.Handlers.ExecuteQueryCommandHandler).Assembly);
});

// MediatR pipeline behaviors
builder.Services.AddScoped(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
builder.Services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

// FluentValidation
builder.Services.AddValidatorsFromAssembly(typeof(ExecuteQueryCommand).Assembly);

// Hangfire (skip in test environment)
if (!builder.Environment.IsEnvironment("Test"))
{
    builder.Services.ConfigureHangfire(builder.Configuration);
}

// Response compression
builder.Services.AddOptimizedResponseCompression();

// Configure Swagger/OpenAPI (already configured by AddVersionedSwagger above)
builder.Services.AddEndpointsApiExplorer();

// Configure Fast Health Checks (using cached status from startup validation)
var healthChecks = builder.Services.AddHealthChecks();

// Add fast health checks that return cached status
healthChecks.AddCheck<FastOpenAIHealthCheck>("openai");
healthChecks.AddCheck<BIReportingCopilot.Infrastructure.Health.BIDatabaseHealthCheck>("bidatabase");
healthChecks.AddCheck<BIReportingCopilot.Infrastructure.Health.DefaultDatabaseHealthCheck>("defaultdb");
healthChecks.AddCheck<FastRedisHealthCheck>("redis");

// Add security health checks
builder.Services.AddSecurityHealthChecks();

// Configure Application Insights
if (!string.IsNullOrEmpty(builder.Configuration.GetConnectionString("ApplicationInsights")))
{
    builder.Services.AddApplicationInsightsTelemetry(options =>
    {
        options.ConnectionString = builder.Configuration.GetConnectionString("ApplicationInsights");
        options.EnableAdaptiveSampling = true;
        options.EnableQuickPulseMetricStream = true;
    });
}

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    var apiVersionDescriptionProvider = app.Services.GetRequiredService<Microsoft.AspNetCore.Mvc.ApiExplorer.IApiVersionDescriptionProvider>();
    app.UseVersionedSwagger(apiVersionDescriptionProvider);
}

// Disable HTTPS redirection for development
// app.UseHttpsRedirection();

// Add response compression
app.UseResponseCompression();

// Custom middleware
app.UseGlobalExceptionHandler(); // Global exception handling
app.UseCorrelationId();
app.UseMiddleware<RequestLoggingMiddleware>();
app.UseMiddleware<EnhancedRateLimitingMiddleware>(); // Enhanced distributed rate limiting

app.UseCors("AllowFrontend");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Configure SignalR hubs
app.MapHub<QueryStatusHub>("/hubs/query-status");
app.MapHub<QueryHub>("/hubs/query");

// Configure health checks
app.MapHealthChecks("/health", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        var response = new
        {
            status = report.Status.ToString(),
            timestamp = DateTime.UtcNow,
            totalDuration = report.TotalDuration.TotalMilliseconds,
            checks = report.Entries.Select(entry => new
            {
                name = entry.Key,
                status = entry.Value.Status.ToString(),
                description = entry.Value.Description,
                duration = entry.Value.Duration.TotalMilliseconds,
                exception = entry.Value.Exception?.Message,
                data = entry.Value.Data
            })
        };
        await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(response, new System.Text.Json.JsonSerializerOptions
        {
            PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase,
            WriteIndented = true
        }));
    }
});

app.MapHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready"),
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        var response = new
        {
            status = report.Status.ToString(),
            timestamp = DateTime.UtcNow,
            checks = report.Entries.Where(entry => entry.Value.Tags.Contains("ready")).Select(entry => new
            {
                name = entry.Key,
                status = entry.Value.Status.ToString(),
                description = entry.Value.Description
            })
        };
        await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(response));
    }
});

// Initialize database and seed data (only if using SQL Server)
if (!string.IsNullOrEmpty(builder.Configuration.GetConnectionString("DefaultConnection")))
{
    using (var scope = app.Services.CreateScope())
    {
        var dbInitService = scope.ServiceProvider.GetRequiredService<IDatabaseInitializationService>();
        try
        {
            await dbInitService.InitializeAsync();
            Log.Information("Database initialization completed successfully");
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Database initialization failed - continuing with in-memory database");
        }
    }
}
else
{
    Log.Information("Using in-memory database for development");
}

// Configure Hangfire Dashboard (only in development or with proper authentication)
if (app.Environment.IsDevelopment())
{
    app.UseHangfireDashboard("/hangfire");
}

// Configure recurring jobs (skip in test environment)
if (!app.Environment.IsEnvironment("Test"))
{
    app.ConfigureRecurringJobs();
}

Log.Information("BI Reporting Copilot API starting up...");

try
{
    await app.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

// Make Program class accessible for testing
public partial class Program { }
