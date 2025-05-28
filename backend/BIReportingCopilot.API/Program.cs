using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
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
builder.Services.AddControllers();

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

// Register repositories
builder.Services.AddScoped<IUserRepository, BIReportingCopilot.Infrastructure.Repositories.UserRepository>();
builder.Services.AddScoped<ITokenRepository, BIReportingCopilot.Infrastructure.Repositories.TokenRepository>();
builder.Services.AddScoped<BIReportingCopilot.Infrastructure.Interfaces.IUserEntityRepository, BIReportingCopilot.Infrastructure.Repositories.UserEntityRepository>();

// Register AI providers and factory
builder.Services.AddScoped<BIReportingCopilot.Infrastructure.AI.Providers.OpenAIProvider>();
builder.Services.AddScoped<BIReportingCopilot.Infrastructure.AI.Providers.AzureOpenAIProvider>();
builder.Services.AddScoped<IAIProviderFactory, BIReportingCopilot.Infrastructure.AI.Providers.AIProviderFactory>();

// Register performance and monitoring services
builder.Services.AddScoped<BIReportingCopilot.Infrastructure.Performance.StreamingDataService>();
builder.Services.AddSingleton<BIReportingCopilot.Infrastructure.Monitoring.IMetricsCollector, BIReportingCopilot.Infrastructure.Monitoring.MetricsCollector>();

// Register AI/ML enhancement services
builder.Services.AddScoped<BIReportingCopilot.Infrastructure.AI.ISemanticCacheService, BIReportingCopilot.Infrastructure.AI.SemanticCacheService>();
builder.Services.AddScoped<BIReportingCopilot.Infrastructure.AI.MLAnomalyDetector>();

// Register scalability services
builder.Services.AddScoped<BIReportingCopilot.Infrastructure.Messaging.IEventBus, BIReportingCopilot.Infrastructure.Messaging.InMemoryEventBus>();
builder.Services.Configure<BIReportingCopilot.Infrastructure.Messaging.EventBusConfiguration>(
    builder.Configuration.GetSection("EventBus"));

// Register distributed cache service
builder.Services.AddStackExchangeRedisCache(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("Redis");
    if (!string.IsNullOrEmpty(connectionString))
    {
        options.Configuration = connectionString;
    }
});

// Register Redis connection multiplexer for advanced caching features
var redisConnectionString = builder.Configuration.GetConnectionString("Redis");
if (!string.IsNullOrEmpty(redisConnectionString))
{
    builder.Services.AddSingleton<StackExchange.Redis.IConnectionMultiplexer>(provider =>
        StackExchange.Redis.ConnectionMultiplexer.Connect(redisConnectionString));

    builder.Services.AddScoped<BIReportingCopilot.Infrastructure.Caching.DistributedCacheService>();
    builder.Services.Configure<BIReportingCopilot.Infrastructure.Caching.DistributedCacheConfiguration>(
        builder.Configuration.GetSection("DistributedCache"));
}

// Register sharded schema service
builder.Services.Configure<BIReportingCopilot.Infrastructure.Services.ShardingConfiguration>(
    builder.Configuration.GetSection("Sharding"));

// Register event handlers
builder.Services.AddScoped<BIReportingCopilot.Infrastructure.Messaging.EventHandlers.QueryExecutedEventHandler>();
builder.Services.AddScoped<BIReportingCopilot.Infrastructure.Messaging.EventHandlers.FeedbackReceivedEventHandler>();
builder.Services.AddScoped<BIReportingCopilot.Infrastructure.Messaging.EventHandlers.AnomalyDetectedEventHandler>();
builder.Services.AddScoped<BIReportingCopilot.Infrastructure.Messaging.EventHandlers.CacheInvalidatedEventHandler>();
builder.Services.AddScoped<BIReportingCopilot.Infrastructure.Messaging.EventHandlers.PerformanceMetricsEventHandler>();

// Register core application services - Consolidated and unified
builder.Services.AddScoped<IAIService, BIReportingCopilot.Infrastructure.AI.AIService>(); // Base AI service
builder.Services.AddScoped<IQueryService, QueryService>(); // Base query service

// Register AI enhancement decorators
builder.Services.Decorate<IAIService, BIReportingCopilot.Infrastructure.AI.AdaptiveAIService>();

// Register resilient services as decorators
builder.Services.Decorate<IAIService, BIReportingCopilot.Infrastructure.Resilience.ResilientAIService>();
builder.Services.Decorate<IQueryService, BIReportingCopilot.Infrastructure.Resilience.ResilientQueryService>();

// Register traced services as final decorators
builder.Services.Decorate<IQueryService, BIReportingCopilot.Infrastructure.Monitoring.TracedQueryService>();

// No legacy service registrations needed - using unified IAIService directly
builder.Services.AddScoped<ISchemaService, BIReportingCopilot.Infrastructure.Services.SchemaService>();
builder.Services.AddScoped<ISqlQueryService, BIReportingCopilot.Infrastructure.Services.SqlQueryService>();
builder.Services.AddScoped<IStreamingSqlQueryService>(provider =>
{
    var innerService = provider.GetRequiredService<ISqlQueryService>();
    var connectionStringProvider = provider.GetRequiredService<IConnectionStringProvider>();
    var logger = provider.GetRequiredService<ILogger<StreamingSqlQueryService>>();

    // Try to get BIDatabase connection string, fallback to DefaultConnection
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
}); // Streaming query service
builder.Services.AddScoped<IPromptService, BIReportingCopilot.Infrastructure.Services.PromptService>();
builder.Services.AddScoped<IVisualizationService, BIReportingCopilot.Infrastructure.Services.VisualizationService>(); // Consolidated visualization service
builder.Services.AddSingleton<ICacheService, BIReportingCopilot.Infrastructure.Performance.CacheService>(); // Unified cache service - singleton for performance
builder.Services.AddScoped<IUserService, BIReportingCopilot.Infrastructure.Services.UserService>();
builder.Services.AddScoped<IAuditService, BIReportingCopilot.Infrastructure.Services.AuditService>();
builder.Services.AddScoped<IAuthenticationService, BIReportingCopilot.Infrastructure.Services.AuthenticationService>();
builder.Services.AddScoped<IMfaService, BIReportingCopilot.Infrastructure.Services.MfaService>();
builder.Services.AddScoped<IMfaChallengeRepository, BIReportingCopilot.Infrastructure.Repositories.MfaChallengeRepository>();
builder.Services.AddScoped<IEmailService, BIReportingCopilot.Infrastructure.Services.EmailService>();
builder.Services.AddScoped<ISmsService, BIReportingCopilot.Infrastructure.Services.SmsService>();

// Register new advanced AI/ML services
builder.Services.AddScoped<ISemanticAnalyzer, BIReportingCopilot.Infrastructure.AI.SemanticAnalyzer>();
builder.Services.AddScoped<IQueryClassifier, BIReportingCopilot.Infrastructure.AI.QueryClassifier>();
builder.Services.AddScoped<IContextManager, BIReportingCopilot.Infrastructure.AI.ContextManager>();
builder.Services.AddScoped<IQueryOptimizer, BIReportingCopilot.Infrastructure.AI.QueryOptimizer>();
builder.Services.AddScoped<IQueryProcessor, BIReportingCopilot.Infrastructure.AI.EnhancedQueryProcessor>();
builder.Services.AddScoped<BIReportingCopilot.Infrastructure.Security.ISqlQueryValidator, BIReportingCopilot.Infrastructure.Security.SqlQueryValidator>();

// Register enhanced security services
builder.Services.AddScoped<BIReportingCopilot.Infrastructure.Security.IRateLimitingService, BIReportingCopilot.Infrastructure.Security.DistributedRateLimitingService>();
builder.Services.AddScoped<BIReportingCopilot.Infrastructure.Security.ISecretsManagementService, BIReportingCopilot.Infrastructure.Security.SecretsManagementService>();
builder.Services.AddScoped<BIReportingCopilot.Infrastructure.Security.IEnhancedSqlQueryValidator, BIReportingCopilot.Infrastructure.Security.EnhancedSqlQueryValidator>();

// Configure AutoMapper
builder.Services.AddAutoMapper(typeof(Program));

// Configure MediatR
builder.Services.AddMediatR(cfg => {
    cfg.RegisterServicesFromAssembly(typeof(ExecuteQueryCommand).Assembly); // Core assembly
    cfg.RegisterServicesFromAssembly(typeof(Program).Assembly); // API assembly
    cfg.RegisterServicesFromAssembly(typeof(BIReportingCopilot.Infrastructure.Handlers.ExecuteQueryCommandHandler).Assembly); // Infrastructure assembly
});

// Add MediatR pipeline behaviors
builder.Services.AddScoped(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
builder.Services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

// Add validators
builder.Services.AddValidatorsFromAssembly(typeof(ExecuteQueryCommand).Assembly);

// Configure Hangfire (skip in test environment)
if (!builder.Environment.IsEnvironment("Test"))
{
    builder.Services.ConfigureHangfire(builder.Configuration);
}

// Add enhanced services
builder.Services.AddScoped<BIReportingCopilot.Core.Interfaces.IPasswordHasher, BIReportingCopilot.Infrastructure.Security.PasswordHasher>();
builder.Services.AddScoped<IConnectionStringProvider, SecureConnectionStringProvider>();
builder.Services.AddScoped<IDatabaseInitializationService, DatabaseInitializationService>();
builder.Services.AddScoped<ITuningService, TuningService>();
builder.Services.AddScoped<IAITuningSettingsService, AITuningSettingsService>();
builder.Services.AddScoped<IBusinessContextAutoGenerator, BusinessContextAutoGenerator>();

// Add startup health validation services
builder.Services.AddSingleton<IStartupHealthValidator, StartupHealthValidator>();
builder.Services.AddHostedService<StartupValidationService>();

// Decorate schema service with caching
builder.Services.Decorate<ISchemaService, CachedSchemaService>();

// Enhanced services are now registered directly above

// Add response compression
builder.Services.AddOptimizedResponseCompression();

// Configure Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "BI Reporting Copilot API",
        Version = "v1",
        Description = "AI-Powered Business Intelligence Reporting Copilot API",
        Contact = new OpenApiContact
        {
            Name = "BI Copilot Team",
            Email = "support@company.com"
        }
    });

    // Configure JWT authentication in Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token in the text input below.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });

    // Include XML comments
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
});

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

app.UseHttpsRedirection();

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
