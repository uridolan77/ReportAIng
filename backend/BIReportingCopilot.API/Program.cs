using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Text;
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
using BIReportingCopilot.Core.Interfaces.Review;
using BIReportingCopilot.Core.Interfaces.CostOptimization;
using BIReportingCopilot.Core.Configuration;
using BIReportingCopilot.Infrastructure.Data;
// Import all the reorganized service namespaces
using BIReportingCopilot.Infrastructure.Authentication;
using BIReportingCopilot.Infrastructure.Business;
using BIReportingCopilot.Infrastructure.Query;
using BIReportingCopilot.Infrastructure.Schema;
using BIReportingCopilot.Infrastructure.Visualization;
using BIReportingCopilot.Infrastructure.Jobs;
using BIReportingCopilot.Infrastructure.Messaging;
using BIReportingCopilot.Infrastructure.AI.Management;
using BIReportingCopilot.Infrastructure.Monitoring;
using BIReportingCopilot.Infrastructure.Review;
using SignalRProgressReporter = BIReportingCopilot.Infrastructure.Messaging.SignalRProgressReporter;
using BIReportingCopilot.API.Middleware;
using BIReportingCopilot.API.Hubs;

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
using BIReportingCopilot.Core.Commands;
using Hangfire;
using BIReportingCopilot.Core.Models;
using BIReportingCopilot.API.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Configure Key Vault (selective loading to avoid JWT override)
var keyVaultUrl = builder.Configuration["KeyVault:Url"];
if (!string.IsNullOrEmpty(keyVaultUrl))
{
    try
    {
        // Load Key Vault but exclude JWT settings to prevent override
        builder.Configuration.AddAzureKeyVault(
            new Uri(keyVaultUrl),
            new Azure.Identity.DefaultAzureCredential(),
            new SelectiveSecretManager());
        Log.Information("Azure Key Vault configured with selective secret loading: {KeyVaultUrl}", keyVaultUrl);
    }
    catch (Exception ex)
    {
        Log.Error(ex, "Failed to configure Azure Key Vault: {KeyVaultUrl}", keyVaultUrl);
    }
}
else
{
    Log.Warning("KeyVault:Url not configured - Azure Key Vault integration disabled");
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

// Register connection string provider before configuration validation
builder.Services.AddScoped<IConnectionStringProvider, SecureConnectionStringProvider>();

// Configuration validation
try
{
    builder.Configuration.ValidateStartupConfiguration();
    Log.Information("Configuration validation completed successfully");
}
catch (Exception ex)
{
    Log.Fatal(ex, "Configuration validation failed");
    throw;
}

// Add validated configuration
builder.Services.AddValidatedConfiguration(builder.Configuration);

// Configure OpenAI settings with placeholder resolution
builder.Services.Configure<BIReportingCopilot.Core.Configuration.OpenAIConfiguration>(config =>
{
    builder.Configuration.GetSection("OpenAI").Bind(config);

    // Use the existing SecureConnectionStringProvider to resolve placeholders
    if (config.ApiKey.StartsWith("{azurevault:"))
    {
        try
        {
            // Create a temporary connection string provider to resolve the placeholder
            var connectionStringProvider = new BIReportingCopilot.Infrastructure.Configuration.SecureConnectionStringProvider(
                builder.Configuration,
                new Microsoft.Extensions.Logging.Abstractions.NullLogger<BIReportingCopilot.Infrastructure.Configuration.SecureConnectionStringProvider>());

            // Use the existing ReplacePlaceholdersAsync method via reflection
            var method = typeof(BIReportingCopilot.Infrastructure.Configuration.SecureConnectionStringProvider)
                .GetMethod("ReplacePlaceholdersAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            if (method != null)
            {
                var task = (Task<string>)method.Invoke(connectionStringProvider, new object[] { config.ApiKey });
                var resolvedApiKey = task.GetAwaiter().GetResult();
                config.ApiKey = resolvedApiKey;
                Log.Information("OpenAI API Key resolved from Azure Key Vault");
            }
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Failed to resolve OpenAI API Key from Azure Key Vault, keeping placeholder");
        }
    }
});

builder.Services.Configure<BIReportingCopilot.Core.Configuration.AzureOpenAIConfiguration>(config =>
{
    builder.Configuration.GetSection("AzureOpenAI").Bind(config);

    // Resolve Azure OpenAI API key placeholder if present
    if (config.ApiKey.StartsWith("{azurevault:"))
    {
        try
        {
            var connectionStringProvider = new BIReportingCopilot.Infrastructure.Configuration.SecureConnectionStringProvider(
                builder.Configuration,
                new Microsoft.Extensions.Logging.Abstractions.NullLogger<BIReportingCopilot.Infrastructure.Configuration.SecureConnectionStringProvider>());

            var method = typeof(BIReportingCopilot.Infrastructure.Configuration.SecureConnectionStringProvider)
                .GetMethod("ReplacePlaceholdersAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            if (method != null)
            {
                var task = (Task<string>)method.Invoke(connectionStringProvider, new object[] { config.ApiKey });
                var resolvedApiKey = task.GetAwaiter().GetResult();
                config.ApiKey = resolvedApiKey;
                Log.Information("Azure OpenAI API Key resolved from Azure Key Vault");
            }
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Failed to resolve Azure OpenAI API Key from Azure Key Vault, keeping placeholder");
        }
    }
});

builder.Services.Configure<BIReportingCopilot.Core.Configuration.AIServiceConfiguration>(config =>
{
    builder.Configuration.GetSection("OpenAI").Bind(config.OpenAI);
    builder.Configuration.GetSection("AzureOpenAI").Bind(config.AzureOpenAI);

    // Resolve placeholders for both providers using the same method
    if (config.OpenAI.ApiKey.StartsWith("{azurevault:"))
    {
        try
        {
            var connectionStringProvider = new BIReportingCopilot.Infrastructure.Configuration.SecureConnectionStringProvider(
                builder.Configuration,
                new Microsoft.Extensions.Logging.Abstractions.NullLogger<BIReportingCopilot.Infrastructure.Configuration.SecureConnectionStringProvider>());

            var method = typeof(BIReportingCopilot.Infrastructure.Configuration.SecureConnectionStringProvider)
                .GetMethod("ReplacePlaceholdersAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            if (method != null)
            {
                var task = (Task<string>)method.Invoke(connectionStringProvider, new object[] { config.OpenAI.ApiKey });
                config.OpenAI.ApiKey = task.GetAwaiter().GetResult();
            }
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Failed to resolve OpenAI API Key in AIServiceConfiguration");
        }
    }

    if (config.AzureOpenAI.ApiKey.StartsWith("{azurevault:"))
    {
        try
        {
            var connectionStringProvider = new BIReportingCopilot.Infrastructure.Configuration.SecureConnectionStringProvider(
                builder.Configuration,
                new Microsoft.Extensions.Logging.Abstractions.NullLogger<BIReportingCopilot.Infrastructure.Configuration.SecureConnectionStringProvider>());

            var method = typeof(BIReportingCopilot.Infrastructure.Configuration.SecureConnectionStringProvider)
                .GetMethod("ReplacePlaceholdersAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            if (method != null)
            {
                var task = (Task<string>)method.Invoke(connectionStringProvider, new object[] { config.AzureOpenAI.ApiKey });
                config.AzureOpenAI.ApiKey = task.GetAwaiter().GetResult();
            }
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Failed to resolve Azure OpenAI API Key in AIServiceConfiguration");
        }
    }
});

// JWT settings are already configured by AddValidatedConfiguration above

// Add services to the container
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Configure JSON serialization to use camelCase to match frontend expectations
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.WriteIndented = builder.Environment.IsDevelopment();
    });

// Add Swagger documentation
builder.Services.AddSwaggerGen(options =>
{
    // Include XML comments
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }

    // Security definitions
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
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

    // Custom schema ID generator to handle duplicate class names
    options.CustomSchemaIds(type =>
    {
        // Use full type name including namespace to avoid conflicts
        var fullName = type.FullName ?? type.Name;

        // Handle generic types
        if (type.IsGenericType)
        {
            var genericTypeName = type.GetGenericTypeDefinition().Name;
            var genericArgs = string.Join("", type.GetGenericArguments().Select(t => t.Name));
            return $"{genericTypeName.Split('`')[0]}Of{genericArgs}";
        }

        // Replace dots with underscores for cleaner schema names
        return fullName.Replace(".", "_").Replace("+", "_");
    });

    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "BI Reporting Copilot API",
        Version = "v1",
        Description = "AI-powered Business Intelligence reporting and query generation API",
        Contact = new OpenApiContact
        {
            Name = "BI Reporting Copilot Team",
            Email = "support@bireportingcopilot.com"
        },
        License = new OpenApiLicense
        {
            Name = "MIT License",
            Url = new Uri("https://opensource.org/licenses/MIT")
        }
    });
});

// Add FluentValidation
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();
builder.Services.AddValidatorsFromAssemblyContaining<BIReportingCopilot.Core.QueryRequestValidator>();

// Configure application settings (using configuration)
builder.Services.Configure<BIReportingCopilot.Core.Configuration.ApplicationSettings>(
    builder.Configuration.GetSection(BIReportingCopilot.Core.Configuration.ApplicationSettings.SectionName));

// Configure security settings
builder.Services.Configure<BIReportingCopilot.Infrastructure.Security.RateLimitingConfiguration>(
    builder.Configuration.GetSection("RateLimiting"));
builder.Services.Configure<BIReportingCopilot.Infrastructure.Security.SecretsConfiguration>(
    builder.Configuration.GetSection("Secrets"));
builder.Services.Configure<BIReportingCopilot.Infrastructure.Security.SqlValidationConfiguration>(
    builder.Configuration.GetSection("SqlValidation"));

// Configure Entity Framework with bounded contexts (Enhancement #4: Database Context Optimization)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// Configure SQL Server options for all contexts
void ConfigureSqlServerOptions(SqlServerDbContextOptionsBuilder sqlOptions)
{
    sqlOptions.EnableRetryOnFailure(
        maxRetryCount: 3,
        maxRetryDelay: TimeSpan.FromSeconds(30),
        errorNumbersToAdd: null);
    sqlOptions.CommandTimeout(30);
}

if (!string.IsNullOrEmpty(connectionString))
{
    // Legacy context for backward compatibility
    builder.Services.AddDbContext<BICopilotContext>(options =>
        options.UseSqlServer(connectionString, sqlOptions => ConfigureSqlServerOptions(sqlOptions)));

    // Bounded contexts for better separation of concerns
    builder.Services.AddDbContext<BIReportingCopilot.Infrastructure.Data.Contexts.SecurityDbContext>(options =>
        options.UseSqlServer(connectionString, sqlOptions => ConfigureSqlServerOptions(sqlOptions)));

    builder.Services.AddDbContext<BIReportingCopilot.Infrastructure.Data.Contexts.TuningDbContext>(options =>
        options.UseSqlServer(connectionString, sqlOptions => ConfigureSqlServerOptions(sqlOptions)));

    builder.Services.AddDbContext<BIReportingCopilot.Infrastructure.Data.Contexts.QueryDbContext>(options =>
        options.UseSqlServer(connectionString, sqlOptions => ConfigureSqlServerOptions(sqlOptions)));

    builder.Services.AddDbContext<BIReportingCopilot.Infrastructure.Data.Contexts.SchemaDbContext>(options =>
        options.UseSqlServer(connectionString, sqlOptions => ConfigureSqlServerOptions(sqlOptions)));

    builder.Services.AddDbContext<BIReportingCopilot.Infrastructure.Data.Contexts.MonitoringDbContext>(options =>
        options.UseSqlServer(connectionString, sqlOptions => ConfigureSqlServerOptions(sqlOptions)));

    builder.Services.AddDbContext<BIReportingCopilot.Infrastructure.Data.Contexts.TransparencyDbContext>(options =>
        options.UseSqlServer(connectionString, sqlOptions => ConfigureSqlServerOptions(sqlOptions)));
}
else
{
    // Use in-memory database for development
    builder.Services.AddDbContext<BICopilotContext>(options =>
        options.UseInMemoryDatabase("BICopilotDev"));

    // In-memory bounded contexts for development
    builder.Services.AddDbContext<BIReportingCopilot.Infrastructure.Data.Contexts.SecurityDbContext>(options =>
        options.UseInMemoryDatabase("SecurityDev"));

    builder.Services.AddDbContext<BIReportingCopilot.Infrastructure.Data.Contexts.TuningDbContext>(options =>
        options.UseInMemoryDatabase("TuningDev"));

    builder.Services.AddDbContext<BIReportingCopilot.Infrastructure.Data.Contexts.QueryDbContext>(options =>
        options.UseInMemoryDatabase("QueryDev"));

    builder.Services.AddDbContext<BIReportingCopilot.Infrastructure.Data.Contexts.SchemaDbContext>(options =>
        options.UseInMemoryDatabase("SchemaDev"));

    builder.Services.AddDbContext<BIReportingCopilot.Infrastructure.Data.Contexts.MonitoringDbContext>(options =>
        options.UseInMemoryDatabase("MonitoringDev"));

    builder.Services.AddDbContext<BIReportingCopilot.Infrastructure.Data.Contexts.TransparencyDbContext>(options =>
        options.UseInMemoryDatabase("TransparencyDev"));
}

// DbContext factory for managing bounded contexts
builder.Services.AddScoped<BIReportingCopilot.Infrastructure.Data.Contexts.IDbContextFactory, BIReportingCopilot.Infrastructure.Data.Contexts.DbContextFactory>();

// Context migration service for transitioning from monolithic to bounded contexts
builder.Services.AddScoped<BIReportingCopilot.Infrastructure.Data.Migration.ContextMigrationService>();

// Service migration helper for systematic service migration
builder.Services.AddScoped<BIReportingCopilot.Infrastructure.Data.Migration.ServiceMigrationHelper>();

// Migration status tracker for comprehensive migration monitoring
builder.Services.AddScoped<BIReportingCopilot.Infrastructure.Data.Migration.MigrationStatusTracker>();

// Configure Authentication - Temporarily disabled for testing
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["Secret"];

// Temporarily skip JWT validation to test database fix
if (string.IsNullOrEmpty(secretKey))
{
    Log.Warning("JWT Secret not found in configuration - using default for testing");
    secretKey = "temporary-test-secret-key-for-database-testing-32-chars-minimum";
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
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(secretKey!)),
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

// Redis Cache configuration is handled below in the caching section

// Add OpenAI client for AI service
builder.Services.AddSingleton(provider =>
{
    var logger = provider.GetRequiredService<ILogger<Program>>();

    // Get the resolved configurations from the options
    var openAIOptions = provider.GetRequiredService<Microsoft.Extensions.Options.IOptions<BIReportingCopilot.Core.Configuration.OpenAIConfiguration>>();
    var azureOpenAIOptions = provider.GetRequiredService<Microsoft.Extensions.Options.IOptions<BIReportingCopilot.Core.Configuration.AzureOpenAIConfiguration>>();

    var openAIConfig = openAIOptions.Value;
    var azureConfig = azureOpenAIOptions.Value;

    // Try Azure OpenAI first if configured
    if (!string.IsNullOrEmpty(azureConfig.Endpoint) && !string.IsNullOrEmpty(azureConfig.ApiKey))
    {
        logger.LogInformation("Configuring Azure OpenAI client with endpoint: {Endpoint}", azureConfig.Endpoint);
        return new Azure.AI.OpenAI.OpenAIClient(new Uri(azureConfig.Endpoint), new Azure.AzureKeyCredential(azureConfig.ApiKey));
    }

    // Fallback to regular OpenAI
    logger.LogInformation("🔍 DEBUG: OpenAI API Key value: '{ApiKey}' (Length: {Length})",
        openAIConfig.ApiKey?.Substring(0, Math.Min(20, openAIConfig.ApiKey?.Length ?? 0)) + "...",
        openAIConfig.ApiKey?.Length ?? 0);

    if (!string.IsNullOrEmpty(openAIConfig.ApiKey) && openAIConfig.ApiKey != "your-openai-api-key-here")
    {
        // Check if it's still an Azure Key Vault placeholder (shouldn't be after resolution)
        if (openAIConfig.ApiKey.StartsWith("{azurevault:"))
        {
            logger.LogWarning("🚨 OpenAI API Key is still an Azure Key Vault placeholder after resolution: {ApiKey}", openAIConfig.ApiKey);
            logger.LogWarning("Azure Key Vault resolution failed. Using fallback.");
        }
        else
        {
            logger.LogInformation("✅ Configuring OpenAI client with resolved API key");
            return new Azure.AI.OpenAI.OpenAIClient(openAIConfig.ApiKey);
        }
    }

    logger.LogWarning("No valid OpenAI configuration found. AI features will use fallback responses.");
    // Return a mock client for development - this will be handled by the service layer
    return new Azure.AI.OpenAI.OpenAIClient("mock-key");
});

// ===== REPOSITORY LAYER =====
// Unified repository registrations with proper interface mappings
builder.Services.AddRepositoryServices();

// ===== BUSINESS CONTEXT SERVICES =====
// Business Context Aware Prompt Building services
builder.Services.AddBusinessContextServices();

// ===== HTTP CONTEXT ACCESSOR =====
// Required for services that need access to HTTP context (like LLMAwareAIService)
builder.Services.AddHttpContextAccessor();

// ===== AI PROVIDERS & FACTORY =====
builder.Services.AddScoped<BIReportingCopilot.Infrastructure.AI.Providers.OpenAIProvider>();
builder.Services.AddScoped<BIReportingCopilot.Infrastructure.AI.Providers.AzureOpenAIProvider>();
builder.Services.AddScoped<IAIProviderFactory, BIReportingCopilot.Infrastructure.AI.Providers.AIProviderFactory>();

// ===== LLM MANAGEMENT SERVICES =====
builder.Services.AddScoped<ILLMManagementService, BIReportingCopilot.Infrastructure.AI.Management.LLMManagementService>();
builder.Services.AddScoped<BIReportingCopilot.Infrastructure.AI.Management.LLMManagementService>();

// ===== COST MANAGEMENT SERVICES =====
builder.Services.AddScoped<BIReportingCopilot.Core.Interfaces.CostOptimization.ICostManagementService, BIReportingCopilot.Infrastructure.CostOptimization.CostManagementService>();
builder.Services.AddScoped<BIReportingCopilot.Core.Interfaces.CostOptimization.ICostAnalyticsService, BIReportingCopilot.Infrastructure.CostOptimization.CostAnalyticsService>();

// ===== LLM-AWARE AI SERVICE =====
// Register the base AI service directly (without circular dependency)
builder.Services.AddScoped<BIReportingCopilot.Infrastructure.AI.Core.AIService>();

// Register the LLM-aware service as a decorator
builder.Services.AddScoped<ILLMAwareAIService, BIReportingCopilot.Infrastructure.AI.Core.LLMAwareAIService>();

// Register IAIService to use the LLM-aware service
builder.Services.AddScoped<IAIService>(provider => provider.GetRequiredService<ILLMAwareAIService>());

// ===== CONSOLIDATED SERVICES (ROUND 4, 5 & 6 CLEANUP) =====

// Configuration service - consolidates all configuration management
builder.Services.AddSingleton<BIReportingCopilot.Infrastructure.Configuration.ConfigurationService>();

// Configuration migration service for backward compatibility during transition
builder.Services.AddSingleton<BIReportingCopilot.Infrastructure.Configuration.ConfigurationMigrationService>();

// ===== PERFORMANCE OPTIMIZATION SERVICES =====
// Performance monitoring and optimization (Phase 5 additions)
builder.Services.AddScoped<IPerformanceOptimizationService, BIReportingCopilot.Infrastructure.CostOptimization.PerformanceOptimizationService>();
builder.Services.AddScoped<ICacheOptimizationService, BIReportingCopilot.Infrastructure.CostOptimization.CacheOptimizationService>();

// Use MonitoringManagementService as IResourceMonitoringService implementation
builder.Services.AddScoped<IResourceMonitoringService>(provider =>
    provider.GetRequiredService<BIReportingCopilot.Infrastructure.Monitoring.MonitoringManagementService>());

// Unified performance management service - consolidates streaming, metrics, and monitoring
builder.Services.AddScoped<BIReportingCopilot.Infrastructure.Performance.PerformanceManagementService>();

// Unified health management service - consolidates all health checks
builder.Services.AddScoped<BIReportingCopilot.Infrastructure.Health.HealthManagementService>();

// Unified background job management service - consolidates all background operations
builder.Services.AddScoped<BIReportingCopilot.Infrastructure.Jobs.BackgroundJobManagementService>();

// Unified monitoring management service - consolidates metrics, tracing, and logging
builder.Services.AddScoped<BIReportingCopilot.Infrastructure.Monitoring.MonitoringManagementService>();

// Unified notification management service - consolidates email, SMS, and real-time notifications
builder.Services.AddScoped<BIReportingCopilot.Infrastructure.Messaging.NotificationManagementService>();

// Unified security management service - consolidates rate limiting, SQL validation, and security monitoring
builder.Services.AddScoped<BIReportingCopilot.Infrastructure.Security.SecurityManagementService>();

// ===== AI/ML ENHANCEMENT SERVICES =====
// Removed old Infrastructure ISemanticCacheService registration - using unified service instead

// ===== AI SERVICES =====
// Note: AI services are available but disabled for Phase 3A infrastructure setup

// AI configuration (now part of AIConfiguration)
// Note: AI features are now configured through the AIConfiguration
// in ApplicationSettings section

// Feature flags and migration settings are now part of ApplicationSettings
// These configurations have been consolidated into the configuration model

// Phase 2 Services - Available but disabled for Phase 3A infrastructure setup

// Phase 3 Services - Phase 3A: Infrastructure Ready
// Phase 3 Status and Management Service
builder.Services.AddScoped<BIReportingCopilot.Infrastructure.AI.Management.Phase3StatusService>();

// TODO: Enable individual Phase 3 services after fixing compilation errors
// Phase 3B: Streaming Analytics (Next)
// Phase 3C: NLU (After fixing duplicates)
// Phase 3D: Federated Learning & Quantum Security (Final)

// Phase 3 Configuration
builder.Services.Configure<BIReportingCopilot.Infrastructure.AI.Management.Phase3Configuration>(
    builder.Configuration.GetSection("Phase3"));

// Individual Phase 3 feature configurations will be enabled as features are activated

// Unified query analysis service - consolidates SemanticAnalyzer and QueryClassifier
builder.Services.AddScoped<BIReportingCopilot.Infrastructure.AI.Analysis.QueryAnalysisService>();
// TODO: Fix ambiguous interface references
// builder.Services.AddScoped<ISemanticAnalyzer>(provider =>
//     provider.GetRequiredService<BIReportingCopilot.Infrastructure.AI.Analysis.QueryAnalysisService>());
// builder.Services.AddScoped<IQueryClassifier>(provider =>
//     provider.GetRequiredService<BIReportingCopilot.Infrastructure.AI.Analysis.QueryAnalysisService>());

// Unified prompt management service - consolidates ContextManager and PromptOptimizer
builder.Services.AddScoped<BIReportingCopilot.Infrastructure.AI.Management.PromptManagementService>();
// TODO: Fix ambiguous interface references
// builder.Services.AddScoped<IContextManager>(provider =>
//     provider.GetRequiredService<BIReportingCopilot.Infrastructure.AI.Management.PromptManagementService>());

// Unified learning service - consolidates MLAnomalyDetector and FeedbackLearningEngine
builder.Services.AddScoped<BIReportingCopilot.Infrastructure.AI.Core.LearningService>();
// Query processor service - critical for application functionality
builder.Services.AddScoped<IQueryProcessor, BIReportingCopilot.Infrastructure.AI.Core.QueryProcessor>();
// Register missing dependencies for QueryProcessor
builder.Services.AddScoped<BIReportingCopilot.Core.Interfaces.IQueryOptimizer, BIReportingCopilot.Infrastructure.AI.Analysis.QueryOptimizer>();
// Register IQueryClassifier from QueryAnalysisService (consolidated service)
builder.Services.AddScoped<BIReportingCopilot.Core.Interfaces.IQueryClassifier>(provider =>
    provider.GetRequiredService<BIReportingCopilot.Infrastructure.AI.Analysis.QueryAnalysisService>());

// ===== ML & ANOMALY DETECTION SERVICES =====
// Unified learning service provides all ML functionality including anomaly detection and feedback learning
// Individual services consolidated into LearningService for better maintainability

// Anomaly detection functionality is now integrated into LearningService

// Removed duplicate ISemanticCacheService registration - using unified service instead

// ===== PERFORMANCE & MONITORING =====
// StreamingDataService functionality consolidated into PerformanceManagementService
// Unified monitoring service - MonitoringManagementService implements both IMetricsCollector and monitoring functionality
builder.Services.AddScoped<BIReportingCopilot.Infrastructure.Monitoring.MonitoringManagementService>();
// MetricsCollector functionality consolidated into MonitoringManagementService
// Register IMetricsCollector interface to use MonitoringManagementService
builder.Services.AddScoped<BIReportingCopilot.Core.Interfaces.Monitoring.IMetricsCollector>(provider =>
    provider.GetRequiredService<BIReportingCopilot.Infrastructure.Monitoring.MonitoringManagementService>());

// ===== MESSAGING & EVENTS =====
builder.Services.AddScoped<BIReportingCopilot.Infrastructure.Messaging.IEventBus, BIReportingCopilot.Infrastructure.Messaging.InMemoryEventBus>();
builder.Services.Configure<BIReportingCopilot.Infrastructure.Messaging.EventBusConfiguration>(
    builder.Configuration.GetSection("EventBus"));

// ===== REAL-TIME NOTIFICATIONS =====
builder.Services.AddScoped<BIReportingCopilot.Infrastructure.RealTime.IRealTimeNotificationService, BIReportingCopilot.Infrastructure.RealTime.RealTimeNotificationService>();

// ===== CACHING & REDIS =====
// Configure Cache settings (includes Redis configuration)
builder.Services.Configure<BIReportingCopilot.Core.Configuration.CacheConfiguration>(
    builder.Configuration.GetSection("Cache"));

// Conditionally register Redis services based on configuration
var cacheConfig = builder.Configuration.GetSection("Cache").Get<BIReportingCopilot.Core.Configuration.CacheConfiguration>();
if (cacheConfig?.EnableRedis == true)
{
    // Register Redis connection multiplexer
    builder.Services.AddSingleton<StackExchange.Redis.IConnectionMultiplexer>(provider =>
    {
        var config = provider.GetRequiredService<IOptions<BIReportingCopilot.Core.Configuration.CacheConfiguration>>().Value;
        var connectionString = config.GetRedisConnectionStringWithOptions();

        if (string.IsNullOrEmpty(connectionString))
        {
            throw new InvalidOperationException("Redis is enabled but no valid connection string is configured");
        }

        return StackExchange.Redis.ConnectionMultiplexer.Connect(connectionString);
    });

    // Register Redis distributed cache
    builder.Services.AddStackExchangeRedisCache(options =>
    {
        var config = cacheConfig.GetRedisConnectionStringWithOptions();
        if (!string.IsNullOrEmpty(config))
        {
            options.Configuration = config;
        }
    });

    Log.Information("Redis caching enabled with connection: {ConnectionString}", cacheConfig.RedisConnectionString);
}
else
{
    // Register a null implementation for IConnectionMultiplexer when Redis is disabled
    builder.Services.AddSingleton<StackExchange.Redis.IConnectionMultiplexer>(provider => null!);

    // Use in-memory distributed cache when Redis is disabled
    builder.Services.AddDistributedMemoryCache();

    Log.Information("Redis caching disabled - using in-memory distributed caching");
}

// Always register IMemoryCache for local caching needs
builder.Services.AddMemoryCache();

// Unified cache service with built-in distributed caching support
builder.Services.AddSingleton<BIReportingCopilot.Core.Interfaces.Cache.ICacheService, BIReportingCopilot.Infrastructure.Performance.CacheService>();

// ===== SEMANTIC CACHE & VECTOR SEARCH SERVICES =====
// Register vector search service for semantic similarity
builder.Services.AddScoped<IVectorSearchService, BIReportingCopilot.Infrastructure.AI.Intelligence.InMemoryVectorSearchService>();

// Register semantic cache service (implements Core interface directly)        builder.Services.AddScoped<BIReportingCopilot.Core.Interfaces.AI.ISemanticCacheService, BIReportingCopilot.Infrastructure.AI.Caching.SemanticCacheService>();

// ===== CONFIGURATION SECTIONS =====
builder.Services.Configure<BIReportingCopilot.Infrastructure.Security.RateLimitingConfiguration>(
    builder.Configuration.GetSection("RateLimit"));
builder.Services.Configure<BIReportingCopilot.Infrastructure.Security.SqlValidationConfiguration>(
    builder.Configuration.GetSection("SqlValidation"));

// ===== EVENT HANDLERS =====
builder.Services.AddScoped<BIReportingCopilot.Infrastructure.Messaging.EventHandlers.QueryExecutedEventHandler>();
builder.Services.AddScoped<BIReportingCopilot.Infrastructure.Messaging.EventHandlers.FeedbackReceivedEventHandler>();
builder.Services.AddScoped<BIReportingCopilot.Infrastructure.Messaging.EventHandlers.AnomalyDetectedEventHandler>();
builder.Services.AddScoped<BIReportingCopilot.Infrastructure.Messaging.EventHandlers.CacheInvalidatedEventHandler>();
builder.Services.AddScoped<BIReportingCopilot.Infrastructure.Messaging.EventHandlers.PerformanceMetricsEventHandler>();

// ===== CORE APPLICATION SERVICES =====
// Base services
builder.Services.AddScoped<IQueryProgressNotifier, BIReportingCopilot.API.Hubs.SignalRQueryProgressNotifier>();
builder.Services.AddScoped<IQueryService, BIReportingCopilot.Infrastructure.Query.QueryService>();
builder.Services.AddScoped<ISchemaService, BIReportingCopilot.Infrastructure.Schema.SchemaService>(); // Unified with built-in caching
builder.Services.AddScoped<ISqlQueryService, BIReportingCopilot.Infrastructure.Query.SqlQueryService>();
builder.Services.AddScoped<IPromptService, BIReportingCopilot.Infrastructure.AI.Management.PromptService>();

// ===== AI SERVICES (Strategic Enhancements #2 & #3) =====
// TODO: Fix interface implementations
// builder.Services.AddScoped<INLUService, BIReportingCopilot.Infrastructure.AI.Intelligence.NLUService>();
builder.Services.AddScoped<BIReportingCopilot.Core.Interfaces.Query.ISchemaOptimizationService, BIReportingCopilot.Infrastructure.AI.Intelligence.OptimizationService>();
builder.Services.AddScoped<IVectorSearchService, BIReportingCopilot.Infrastructure.AI.Intelligence.InMemoryVectorSearchService>();
builder.Services.AddScoped<IQueryIntelligenceService, BIReportingCopilot.Infrastructure.AI.Intelligence.IntelligenceService>();
// Real-Time Streaming Service - ENABLED for feature development
builder.Services.AddScoped<IRealTimeStreamingService, BIReportingCopilot.Infrastructure.AI.Streaming.StreamingService>();

// ===== PHASE 2A: ENHANCED MULTI-AGENT ARCHITECTURE =====
// Specialized agents for intelligent query processing
builder.Services.AddScoped<BIReportingCopilot.Core.Interfaces.Agents.IQueryUnderstandingAgent, BIReportingCopilot.Infrastructure.AI.Agents.QueryUnderstandingAgent>();
builder.Services.AddScoped<BIReportingCopilot.Core.Interfaces.Agents.ISchemaNavigationAgent, BIReportingCopilot.Infrastructure.AI.Agents.SchemaNavigationAgent>();
builder.Services.AddScoped<BIReportingCopilot.Core.Interfaces.Agents.ISqlGenerationAgent, BIReportingCopilot.Infrastructure.AI.Agents.SqlGenerationAgent>();
builder.Services.AddScoped<BIReportingCopilot.Core.Interfaces.Agents.IIntelligentAgentOrchestrator, BIReportingCopilot.Infrastructure.AI.Agents.IntelligentAgentOrchestrator>();
// Agent-to-Agent communication protocol
builder.Services.AddScoped<BIReportingCopilot.Core.Interfaces.Agents.IAgentCommunicationProtocol, BIReportingCopilot.Infrastructure.AI.Agents.AgentCommunicationProtocol>();

// ===== PHASE 2B: AI TRANSPARENCY FOUNDATION =====
// Transparency repositories
builder.Services.AddScoped<BIReportingCopilot.Infrastructure.Interfaces.ITransparencyRepository, BIReportingCopilot.Infrastructure.Data.Repositories.TransparencyRepository>();
// Transparency services
builder.Services.AddScoped<BIReportingCopilot.Core.Interfaces.Transparency.ITransparencyService, BIReportingCopilot.Infrastructure.Transparency.TransparencyService>();

// Transparency and explainability services
builder.Services.AddScoped<BIReportingCopilot.Infrastructure.Transparency.IPromptConstructionTracer, BIReportingCopilot.Infrastructure.Transparency.PromptConstructionTracer>();

// ===== AI ANALYTICS & LOGGING SERVICES =====
// Token usage analytics service
builder.Services.AddScoped<BIReportingCopilot.Core.Interfaces.Analytics.ITokenUsageAnalyticsService, BIReportingCopilot.Infrastructure.Analytics.TokenUsageAnalyticsService>();

// Prompt generation logging service
builder.Services.AddScoped<BIReportingCopilot.Core.Interfaces.Analytics.IPromptGenerationLogsService, BIReportingCopilot.Infrastructure.Analytics.PromptGenerationLogsService>();

// Prompt success tracking service
builder.Services.AddScoped<BIReportingCopilot.Core.Interfaces.Analytics.IPromptSuccessTrackingService, BIReportingCopilot.Infrastructure.Analytics.PromptSuccessTrackingService>();

// ===== MODULAR DASHBOARD SERVICES =====
builder.Services.AddScoped<BIReportingCopilot.Infrastructure.AI.Dashboard.DashboardCreationService>();
builder.Services.AddScoped<BIReportingCopilot.Infrastructure.AI.Dashboard.DashboardTemplateService>();
builder.Services.AddScoped<IMultiModalDashboardService, BIReportingCopilot.Infrastructure.AI.Dashboard.MultiModalDashboardService>();
// TODO: Fix ambiguous interface reference
// builder.Services.AddScoped<IVisualizationService, BIReportingCopilot.Infrastructure.Visualization.VisualizationService>();

// Performance Optimization Services
builder.Services.AddHostedService<BIReportingCopilot.Infrastructure.Performance.AutoOptimizationService>();

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
builder.Services.AddScoped<IUserService, BIReportingCopilot.Infrastructure.Authentication.UserService>();
builder.Services.AddScoped<BIReportingCopilot.Core.Interfaces.Data.IAuditService, BIReportingCopilot.Infrastructure.Data.AuditService>();
builder.Services.AddScoped<IAuthenticationService, BIReportingCopilot.Infrastructure.Authentication.AuthenticationService>();
// MFA Service registration
builder.Services.AddScoped<BIReportingCopilot.Core.Interfaces.Security.IMfaService, BIReportingCopilot.Infrastructure.Authentication.MfaService>();
// Unified notification services - NotificationManagementService implements both IEmailService and ISmsService
builder.Services.AddScoped<IEmailService>(provider =>
    provider.GetRequiredService<BIReportingCopilot.Infrastructure.Messaging.NotificationManagementService>());
builder.Services.AddScoped<ISmsService>(provider =>
    provider.GetRequiredService<BIReportingCopilot.Infrastructure.Messaging.NotificationManagementService>());

// ===== SECURITY SERVICES =====
// Consolidated SQL validator - SqlQueryValidator implements both interfaces
builder.Services.AddScoped<BIReportingCopilot.Infrastructure.Security.SqlQueryValidator>();
builder.Services.AddScoped<BIReportingCopilot.Infrastructure.Security.ISqlQueryValidator>(provider =>
    provider.GetRequiredService<BIReportingCopilot.Infrastructure.Security.SqlQueryValidator>());
builder.Services.AddScoped<BIReportingCopilot.Infrastructure.Security.IEnhancedSqlQueryValidator>(provider =>
    provider.GetRequiredService<BIReportingCopilot.Infrastructure.Security.SqlQueryValidator>());

builder.Services.AddScoped<BIReportingCopilot.Infrastructure.Security.IRateLimitingService>(provider =>
    provider.GetRequiredService<BIReportingCopilot.Infrastructure.Security.SecurityManagementService>());
builder.Services.AddScoped<BIReportingCopilot.Infrastructure.Security.ISecretsManagementService, BIReportingCopilot.Infrastructure.Security.SecretsManagementService>();

// ===== SERVICE ENHANCEMENTS =====
// Simplified service registration - functionality built into base services
// Removed complex decorator patterns for better maintainability and performance

// ===== INFRASTRUCTURE SERVICES =====
// Password hasher service registration
builder.Services.AddScoped<BIReportingCopilot.Core.Interfaces.Security.IPasswordHasher, BIReportingCopilot.Infrastructure.Security.PasswordHasher>();
// Core infrastructure services
// TODO: Fix interface reference after interface consolidation
// builder.Services.AddScoped<BIReportingCopilot.Core.Interfaces.Cache.ICacheService, BIReportingCopilot.Infrastructure.Performance.CacheService>();
builder.Services.AddScoped<BIReportingCopilot.Core.Interfaces.Security.IAuditService, BIReportingCopilot.Infrastructure.Data.AuditService>();
builder.Services.AddScoped<BIReportingCopilot.Core.Interfaces.AI.ISemanticAnalyzer, BIReportingCopilot.Infrastructure.AI.Analysis.QueryAnalysisService>();
builder.Services.AddScoped<BIReportingCopilot.Infrastructure.Data.Contexts.IDbContextFactory, BIReportingCopilot.Infrastructure.Data.Contexts.DbContextFactory>();
builder.Services.AddScoped<BIReportingCopilot.Core.Interfaces.IContextManager, BIReportingCopilot.Infrastructure.AI.Management.PromptManagementService>();
// AI services
builder.Services.AddScoped<BIReportingCopilot.Core.Interfaces.AI.IAdvancedNLUService, BIReportingCopilot.Infrastructure.AI.Intelligence.NLUService>();
builder.Services.AddScoped<BIReportingCopilot.Core.Interfaces.AI.ISemanticCacheService, BIReportingCopilot.Infrastructure.AI.Caching.SemanticCacheService>();
builder.Services.AddScoped<BIReportingCopilot.Core.Interfaces.Business.IAITuningSettingsService, BIReportingCopilot.Infrastructure.Business.AITuningSettingsService>();

// Register UserRepository for both Core and Infrastructure interfaces - REMOVED (consolidated above)

// Register TuningService for Infrastructure interface - REMOVED (interface moved to Core)

// Register TokenRepository for both Core and Infrastructure interfaces - REMOVED (consolidated above)

// Register SchemaManagementService for Infrastructure interface (used by simplified SchemaController)
builder.Services.AddScoped<BIReportingCopilot.Infrastructure.Interfaces.ISchemaManagementService, BIReportingCopilot.Infrastructure.Schema.SchemaManagementService>();
builder.Services.AddScoped<BIReportingCopilot.Infrastructure.Schema.DatabaseSchemaDiscoveryService>();

// Register enhanced semantic layer services
builder.Services.AddScoped<BIReportingCopilot.Core.Interfaces.Schema.ISemanticLayerService, BIReportingCopilot.Infrastructure.Schema.SemanticLayerService>();
builder.Services.AddScoped<BIReportingCopilot.Core.Interfaces.Schema.IDynamicSchemaContextualizationService, BIReportingCopilot.Infrastructure.Schema.DynamicSchemaContextualizationService>();

// Phase 2: Enhanced Semantic Layer Services
builder.Services.AddScoped<BIReportingCopilot.Core.Interfaces.Schema.IEnhancedSemanticLayerService, BIReportingCopilot.Infrastructure.Schema.EnhancedSemanticLayerService>();

// Register business metadata population service
builder.Services.AddScoped<BIReportingCopilot.Infrastructure.Schema.BusinessMetadataPopulationService>();

// ===== PHASE 3: ENHANCED SQL VALIDATION PIPELINE =====
// Enhanced semantic SQL validator with self-correction capabilities
builder.Services.AddScoped<BIReportingCopilot.Core.Interfaces.Validation.IEnhancedSemanticSqlValidator, BIReportingCopilot.Infrastructure.Validation.EnhancedSemanticSqlValidator>();

// Dry-run execution service for SQL validation without full execution
builder.Services.AddScoped<BIReportingCopilot.Core.Interfaces.Validation.IDryRunExecutionService, BIReportingCopilot.Infrastructure.Query.DryRunExecutionService>();

// SQL self-correction service with LLM feedback loops
builder.Services.AddScoped<BIReportingCopilot.Core.Interfaces.Validation.ISqlSelfCorrectionService, BIReportingCopilot.Infrastructure.AI.SqlSelfCorrectionService>();

// ===== PHASE 4: HUMAN-IN-LOOP REVIEW SERVICES =====
// Human review workflow management
builder.Services.AddScoped<IHumanReviewService, HumanReviewService>();

// Approval workflow management
builder.Services.AddScoped<IApprovalWorkflowService, ApprovalWorkflowService>();

// Human feedback collection and learning
builder.Services.AddScoped<IHumanFeedbackService, HumanFeedbackService>();

// Review notifications
builder.Services.AddScoped<IReviewNotificationService, ReviewNotificationService>();

// Review configuration management
builder.Services.AddScoped<IReviewConfigurationService, ReviewConfigurationService>();

// AI Learning Service for human feedback processing
builder.Services.AddScoped<BIReportingCopilot.Core.Interfaces.AI.IAILearningService, BIReportingCopilot.Infrastructure.AI.AILearningService>();

// IConnectionStringProvider already registered before configuration validation
builder.Services.AddScoped<IDatabaseInitializationService, BIReportingCopilot.Infrastructure.Data.DatabaseInitializationService>();
// Focused tuning services (Enhancement #1: Refactor "God" Services)
builder.Services.AddScoped<IBusinessTableManagementService, BIReportingCopilot.Infrastructure.Business.BusinessTableManagementService>();
// Query Pattern Management Service - register the concrete implementation and wrapper
builder.Services.AddScoped<BIReportingCopilot.Infrastructure.Query.QueryPatternManagementService>();
builder.Services.AddScoped<BIReportingCopilot.Core.Interfaces.Business.IQueryPatternManagementService, BIReportingCopilot.Infrastructure.Business.BusinessQueryPatternManagementService>();
builder.Services.AddScoped<IGlossaryManagementService, BIReportingCopilot.Infrastructure.Business.GlossaryManagementService>();
builder.Services.AddScoped<IQueryCacheService, BIReportingCopilot.Infrastructure.Query.QueryCacheService>();

// Main tuning service (now delegates to focused services and uses bounded contexts)
builder.Services.AddScoped<BIReportingCopilot.Core.Interfaces.Tuning.ITuningService, BIReportingCopilot.Infrastructure.Business.TuningService>();

// Register AI tuning settings service (implements Core interface directly)
// TODO: Fix missing interface
// builder.Services.AddScoped<BIReportingCopilot.Core.Interfaces.IAITuningSettingsService, BIReportingCopilot.Infrastructure.Business.AITuningSettingsService>();

builder.Services.AddScoped<IBusinessContextAutoGenerator, BIReportingCopilot.Infrastructure.AI.Management.BusinessContextAutoGenerator>();
builder.Services.AddScoped<IQuerySuggestionService, BIReportingCopilot.Infrastructure.Query.QuerySuggestionService>();
// Register SignalRProgressReporter with QueryStatusHub context
builder.Services.AddScoped<IProgressReporter>(provider =>
{
    var hubContext = provider.GetRequiredService<IHubContext<QueryStatusHub>>();
    var logger = provider.GetRequiredService<ILogger<SignalRProgressReporter>>();
    return new SignalRProgressReporter(hubContext, logger);
});

// ===== STARTUP & HEALTH SERVICES =====
// StartupHealthValidator and StartupValidationService functionality consolidated into HealthManagementService

// ===== FRAMEWORK SERVICES =====
// AutoMapper
builder.Services.AddAutoMapper(typeof(Program), typeof(BIReportingCopilot.Infrastructure.Data.QueryHistoryMappingProfile));

// MediatR with assemblies (including reorganized handlers)
builder.Services.AddMediatR(cfg => {
    cfg.RegisterServicesFromAssembly(typeof(ExecuteQueryCommand).Assembly);
    cfg.RegisterServicesFromAssembly(typeof(Program).Assembly);
    cfg.RegisterServicesFromAssembly(typeof(BIReportingCopilot.Infrastructure.Schema.SchemaManagementService).Assembly);
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

// Configure Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();

// Configure Fast Health Checks (using cached status from startup validation)
var healthChecks = builder.Services.AddHealthChecks();

// Add fast health checks that return cached status
// TODO: Fix missing health check classes
// healthChecks.AddCheck<BIReportingCopilot.API.HealthChecks.BIDatabaseHealthCheck>("bidatabase");

// Add configuration health checks (Enhancement #3: Configuration Management)
healthChecks.AddCheck<BIReportingCopilot.Infrastructure.Health.ConfigurationHealthCheck>("configuration");
// TODO: Fix missing health check class
// healthChecks.AddCheck<BIReportingCopilot.Infrastructure.Health.ConfigurationPerformanceHealthCheck>("configuration-performance");

// Add bounded contexts health check (Enhancement #4: Database Context Optimization)
healthChecks.AddCheck<BIReportingCopilot.Infrastructure.Health.BoundedContextsHealthCheck>("bounded-contexts");

// Log health check configuration
Log.Information("Health checks configured - BIDatabase, Configuration, Configuration Performance, and Bounded Contexts health checks registered");

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

// Build the application
var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "BI Reporting Copilot API v1");
        options.RoutePrefix = "swagger";
        options.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.List);
        options.DefaultModelsExpandDepth(-1);
        options.DisplayRequestDuration();
    });
}

// Disable HTTPS redirection for development
// app.UseHttpsRedirection();

// Add response compression
app.UseResponseCompression();

// Enable static files (for embeddings management UI)
app.UseStaticFiles();

// Custom middleware
app.UseGlobalExceptionHandler(); // Global exception handling
app.UseCorrelationId();
app.UseMiddleware<RequestLoggingMiddleware>();
app.UseMiddleware<RateLimitingMiddleware>(); // Distributed rate limiting

app.UseCors("AllowFrontend");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Configure SignalR hubs
app.MapHub<QueryStatusHub>("/hubs/query-status");
app.MapHub<QueryHub>("/hubs/query");
// Real-time streaming hub for streaming features
app.MapHub<BIReportingCopilot.Infrastructure.AI.Streaming.StreamingHub>("/hubs/streaming");
// Transparency hub for real-time transparency updates
app.MapHub<BIReportingCopilot.Infrastructure.Hubs.TransparencyHub>("/hubs/transparency");
// Phase 5: Cost Control & Performance Optimization hubs
app.MapHub<CostMonitoringHub>("/hubs/cost-monitoring");
app.MapHub<PerformanceMonitoringHub>("/hubs/performance-monitoring");
app.MapHub<ResourceMonitoringHub>("/hubs/resource-monitoring");
// Template Analytics hub for real-time analytics updates
app.MapHub<TemplateAnalyticsHub>("/hubs/template-analytics");

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
