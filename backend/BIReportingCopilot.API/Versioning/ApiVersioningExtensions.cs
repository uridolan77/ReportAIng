using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.SwaggerGen;
using Microsoft.OpenApi.Models;
using BIReportingCopilot.Core.Models;
using Microsoft.AspNetCore.Mvc.Versioning;

namespace BIReportingCopilot.API.Versioning;

/// <summary>
/// Extensions for configuring API versioning
/// </summary>
public static class ApiVersioningExtensions
{
    /// <summary>
    /// Add API versioning to the service collection
    /// </summary>
    public static IServiceCollection AddApiVersioningSupport(this IServiceCollection services)
    {
        services.AddApiVersioning(options =>
        {
            // Default version when none is specified
            options.DefaultApiVersion = new ApiVersion(1, 0);
            options.AssumeDefaultVersionWhenUnspecified = true;

            // Version reading strategies
            options.ApiVersionReader = ApiVersionReader.Combine(
                new UrlSegmentApiVersionReader(),           // /api/v1/queries
                new HeaderApiVersionReader("X-Version"),    // X-Version: 1.0
                new QueryStringApiVersionReader("version")  // ?version=1.0
            );

            // Version format
            options.ApiVersionSelector = new CurrentImplementationApiVersionSelector(options);
        });

        services.AddVersionedApiExplorer(setup =>
        {
            setup.GroupNameFormat = "'v'VVV";
            setup.SubstituteApiVersionInUrl = true;
        });

        return services;
    }

    /// <summary>
    /// Configure Swagger for API versioning
    /// </summary>
    public static IServiceCollection AddVersionedSwagger(this IServiceCollection services)
    {
        services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();

        services.AddSwaggerGen(options =>
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

            // Custom operation filters
            options.OperationFilter<ApiVersionOperationFilter>();
            options.DocumentFilter<ApiVersionDocumentFilter>();
        });

        return services;
    }

    /// <summary>
    /// Use versioned Swagger UI
    /// </summary>
    public static IApplicationBuilder UseVersionedSwagger(this IApplicationBuilder app, IApiVersionDescriptionProvider provider)
    {
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            foreach (var description in provider.ApiVersionDescriptions.OrderByDescending(v => v.ApiVersion))
            {
                options.SwaggerEndpoint(
                    $"/swagger/{description.GroupName}/swagger.json",
                    $"BI Reporting Copilot API {description.GroupName.ToUpperInvariant()}");
            }

            options.RoutePrefix = "swagger";
            options.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.List);
            options.DefaultModelsExpandDepth(-1);
            options.DisplayRequestDuration();
        });

        return app;
    }
}

/// <summary>
/// Configures Swagger options for API versioning
/// </summary>
public class ConfigureSwaggerOptions : IConfigureOptions<SwaggerGenOptions>
{
    private readonly IApiVersionDescriptionProvider _provider;

    public ConfigureSwaggerOptions(IApiVersionDescriptionProvider provider)
    {
        _provider = provider;
    }

    public void Configure(SwaggerGenOptions options)
    {
        foreach (var description in _provider.ApiVersionDescriptions)
        {
            options.SwaggerDoc(description.GroupName, CreateInfoForApiVersion(description));
        }
    }

    private static OpenApiInfo CreateInfoForApiVersion(ApiVersionDescription description)
    {
        var info = new OpenApiInfo
        {
            Title = "BI Reporting Copilot API",
            Version = description.ApiVersion.ToString(),
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
        };

        if (description.IsDeprecated)
        {
            info.Description += " (This API version has been deprecated)";
        }

        // Version-specific descriptions
        switch (description.ApiVersion.MajorVersion)
        {
            case 1:
                info.Description += "\n\nVersion 1.0 includes core query generation, schema discovery, and basic AI features.";
                break;
            case 2:
                info.Description += "\n\nVersion 2.0 adds advanced AI/ML features, semantic caching, and enhanced security.";
                break;
        }

        return info;
    }
}

/// <summary>
/// Operation filter to add version information to Swagger operations
/// </summary>
public class ApiVersionOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var apiDescription = context.ApiDescription;
        operation.Deprecated |= apiDescription.IsDeprecated();

        if (operation.Parameters == null)
            return;

        // Remove version parameter from documentation if it's in the URL
        foreach (var parameter in operation.Parameters)
        {
            var description = apiDescription.ParameterDescriptions
                .First(p => p.Name == parameter.Name);

            if (parameter.Description == null)
            {
                parameter.Description = description.ModelMetadata?.Description;
            }
        }
    }
}

/// <summary>
/// Document filter to add version information to Swagger documents
/// </summary>
public class ApiVersionDocumentFilter : IDocumentFilter
{
    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
    {
        // Add version-specific tags
        swaggerDoc.Tags = new List<OpenApiTag>
        {
            new OpenApiTag
            {
                Name = "Queries",
                Description = "Natural language query processing and SQL generation"
            },
            new OpenApiTag
            {
                Name = "Schema",
                Description = "Database schema discovery and metadata management"
            },
            new OpenApiTag
            {
                Name = "Authentication",
                Description = "User authentication and authorization"
            },
            new OpenApiTag
            {
                Name = "Feedback",
                Description = "User feedback and AI model improvement"
            },
            new OpenApiTag
            {
                Name = "Analytics",
                Description = "Usage analytics and performance metrics"
            }
        };

        // Add common responses
        swaggerDoc.Components.Responses = new Dictionary<string, OpenApiResponse>
        {
            ["BadRequest"] = new OpenApiResponse
            {
                Description = "Bad request - validation errors or malformed input"
            },
            ["Unauthorized"] = new OpenApiResponse
            {
                Description = "Unauthorized - authentication required"
            },
            ["Forbidden"] = new OpenApiResponse
            {
                Description = "Forbidden - insufficient permissions"
            },
            ["NotFound"] = new OpenApiResponse
            {
                Description = "Not found - requested resource does not exist"
            },
            ["TooManyRequests"] = new OpenApiResponse
            {
                Description = "Too many requests - rate limit exceeded"
            },
            ["InternalServerError"] = new OpenApiResponse
            {
                Description = "Internal server error - unexpected system error"
            }
        };
    }
}

/// <summary>
/// Base controller with versioning support
/// </summary>
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[Produces("application/json")]
public abstract class VersionedApiController : ControllerBase
{
    /// <summary>
    /// Get the current API version
    /// </summary>
    protected ApiVersion CurrentApiVersion => HttpContext.GetRequestedApiVersion() ?? new ApiVersion(1, 0);

    /// <summary>
    /// Create a standardized OK response
    /// </summary>
    protected IActionResult Ok<T>(T data, ApiMetadata? metadata = null)
    {
        var response = ApiResponse<T>.Ok(data, metadata);
        response.ApiVersion = CurrentApiVersion.ToString();
        return base.Ok(response);
    }

    /// <summary>
    /// Create a standardized error response
    /// </summary>
    protected IActionResult Error<T>(string code, string message, object? details = null)
    {
        var response = ApiResponse<T>.CreateError(code, message, details);
        response.ApiVersion = CurrentApiVersion.ToString();
        return BadRequest(response);
    }

    /// <summary>
    /// Create a standardized not found response
    /// </summary>
    protected IActionResult NotFound<T>(string resource, string identifier)
    {
        var response = ApiResponseExtensions.NotFound<T>(resource, identifier);
        response.ApiVersion = CurrentApiVersion.ToString();
        return base.NotFound(response);
    }

    /// <summary>
    /// Create a standardized validation error response
    /// </summary>
    protected IActionResult ValidationError<T>(Dictionary<string, string[]> validationErrors)
    {
        var response = ApiResponse<T>.ValidationError(validationErrors);
        response.ApiVersion = CurrentApiVersion.ToString();
        return BadRequest(response);
    }

    /// <summary>
    /// Create a paginated response
    /// </summary>
    protected IActionResult PagedOk<T>(
        IEnumerable<T> items,
        int totalCount,
        int page,
        int pageSize,
        long? processingTimeMs = null)
    {
        var response = PagedApiResponse<T>.Create(items, totalCount, page, pageSize, processingTimeMs);
        response.ApiVersion = CurrentApiVersion.ToString();
        return base.Ok(response);
    }
}

/// <summary>
/// API version constants
/// </summary>
public static class ApiVersions
{
    public static readonly ApiVersion V1_0 = new(1, 0);
    public static readonly ApiVersion V2_0 = new(2, 0);
}

/// <summary>
/// Attribute to mark API versions as deprecated
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class ApiVersionDeprecatedAttribute : Attribute
{
    public string? Message { get; set; }
    public DateTime? DeprecationDate { get; set; }
    public DateTime? SunsetDate { get; set; }
}
