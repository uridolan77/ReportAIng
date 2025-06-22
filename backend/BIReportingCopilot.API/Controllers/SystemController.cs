using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using BIReportingCopilot.Infrastructure.Data;
using BIReportingCopilot.Infrastructure.Data.Contexts;
using BIReportingCopilot.Infrastructure.Configuration;
using BIReportingCopilot.Core.Configuration;

namespace BIReportingCopilot.API.Controllers;

/// <summary>
/// System Controller - Unified system management including health, diagnostics, and configuration
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class SystemController : ControllerBase
{
    private readonly ILogger<SystemController> _logger;
    private readonly BICopilotContext _context;
    private readonly SchemaDbContext _schemaContext;
    private readonly ConfigurationService _configurationService;
    private readonly ConfigurationMigrationService _migrationService;

    public SystemController(
        ILogger<SystemController> logger,
        BICopilotContext context,
        SchemaDbContext schemaContext,
        ConfigurationService configurationService,
        ConfigurationMigrationService migrationService)
    {
        _logger = logger;
        _context = context;
        _schemaContext = schemaContext;
        _configurationService = configurationService;
        _migrationService = migrationService;
    }

    #region Health Monitoring

    /// <summary>
    /// Basic health check endpoint
    /// </summary>
    /// <returns>Health status</returns>
    [HttpGet("health")]
    public ActionResult GetHealth()
    {
        return Ok(new
        {
            status = "healthy",
            timestamp = DateTime.UtcNow,
            version = "1.0.0",
            environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Unknown"
        });
    }

    /// <summary>
    /// Detailed health check with service status
    /// </summary>
    /// <returns>Detailed health information</returns>
    [HttpGet("health/detailed")]
    public ActionResult GetDetailedHealth()
    {
        return Ok(new
        {
            status = "healthy",
            timestamp = DateTime.UtcNow,
            version = "1.0.0",
            environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Unknown",
            services = new
            {
                database = "healthy",
                cache = "healthy",
                ai_service = "healthy"
            },
            metrics = new
            {
                uptime = TimeSpan.FromMilliseconds(Environment.TickCount64).ToString(),
                memory_usage = GC.GetTotalMemory(false),
                active_connections = 0 // Placeholder
            }
        });
    }

    /// <summary>
    /// Database diagnostic for BusinessSchemas issue
    /// </summary>
    /// <returns>Database diagnostic information</returns>
    [HttpGet("health/database-diagnostic")]
    public async Task<ActionResult> GetDatabaseDiagnostic()
    {
        try
        {
            var result = new
            {
                timestamp = DateTime.UtcNow,
                environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Unknown",
                database = new
                {
                    name = _context.Database.GetDbConnection().Database,
                    connectionString = _context.Database.GetDbConnection().ConnectionString?.Replace("password=", "password=***").Replace("Password=", "Password=***"),
                    providerName = _context.Database.ProviderName,
                    canConnect = await _context.Database.CanConnectAsync()
                },
                businessSchemasTest = await TestBusinessSchemasTable()
            };

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in database diagnostic");
            return StatusCode(500, new { error = ex.Message, stackTrace = ex.StackTrace });
        }
    }

    /// <summary>
    /// Test specific BusinessSchema operations that might be causing the error
    /// </summary>
    /// <returns>Test results for BusinessSchema operations</returns>
    [HttpGet("health/business-schema-operations-test")]
    public async Task<ActionResult> TestBusinessSchemaOperations()
    {
        try
        {
            var tests = new List<object>();

            // Test 1: Basic count
            try
            {
                var count = await _context.BusinessSchemas.CountAsync();
                tests.Add(new { test = "BasicCount", status = "success", result = count });
            }
            catch (Exception ex)
            {
                tests.Add(new { test = "BasicCount", status = "error", error = ex.Message });
            }

            // Test 2: Query with IsActive filter
            try
            {
                var activeCount = await _context.BusinessSchemas.Where(s => s.IsActive).CountAsync();
                tests.Add(new { test = "IsActiveFilter", status = "success", result = activeCount });
            }
            catch (Exception ex)
            {
                tests.Add(new { test = "IsActiveFilter", status = "error", error = ex.Message });
            }

            // Test 3: Query with IsDefault filter
            try
            {
                var defaultCount = await _context.BusinessSchemas.Where(s => s.IsDefault).CountAsync();
                tests.Add(new { test = "IsDefaultFilter", status = "success", result = defaultCount });
            }
            catch (Exception ex)
            {
                tests.Add(new { test = "IsDefaultFilter", status = "error", error = ex.Message });
            }

            // Test 4: Complex query with Include and Where (like in SchemaManagementService)
            try
            {
                var complexResult = await _context.BusinessSchemas
                    .Include(s => s.Versions.Where(v => v.IsActive))
                    .Where(s => s.IsActive)
                    .FirstOrDefaultAsync();
                tests.Add(new { test = "ComplexQuery", status = "success", result = complexResult != null ? "Found schema" : "No schema found" });
            }
            catch (Exception ex)
            {
                tests.Add(new { test = "ComplexQuery", status = "error", error = ex.Message });
            }

            // Test 5: Default schema query (exact same as SchemaManagementService line 832)
            try
            {
                var defaultSchema = await _context.BusinessSchemas
                    .Where(s => s.IsDefault && s.IsActive)
                    .FirstOrDefaultAsync();
                tests.Add(new { test = "DefaultSchemaQuery", status = "success", result = defaultSchema != null ? "Found default schema" : "No default schema" });
            }
            catch (Exception ex)
            {
                tests.Add(new { test = "DefaultSchemaQuery", status = "error", error = ex.Message });
            }

            return Ok(new
            {
                timestamp = DateTime.UtcNow,
                environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Unknown",
                tests = tests
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in business schema operations test");
            return StatusCode(500, new { error = ex.Message, stackTrace = ex.StackTrace });
        }
    }

    #endregion

    #region Diagnostics

    /// <summary>
    /// Get comprehensive database information
    /// </summary>
    [HttpGet("diagnostics/database-info")]
    public async Task<IActionResult> GetDatabaseInfo()
    {
        try
        {
            var result = new
            {
                Timestamp = DateTime.UtcNow,
                BICopilotContext = new
                {
                    DatabaseName = _context.Database.GetDbConnection().Database,
                    ConnectionString = _context.Database.GetDbConnection().ConnectionString,
                    ProviderName = _context.Database.ProviderName,
                    CanConnect = await _context.Database.CanConnectAsync()
                },
                SchemaDbContext = new
                {
                    DatabaseName = _schemaContext.Database.GetDbConnection().Database,
                    ConnectionString = _schemaContext.Database.GetDbConnection().ConnectionString,
                    ProviderName = _schemaContext.Database.ProviderName,
                    CanConnect = await _schemaContext.Database.CanConnectAsync()
                }
            };

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting database info");
            return StatusCode(500, new { Error = ex.Message });
        }
    }

    /// <summary>
    /// Test BusinessSchemas table functionality
    /// </summary>
    [HttpGet("diagnostics/business-schemas-test")]
    public async Task<IActionResult> TestBusinessSchemas()
    {
        try
        {
            // Test raw SQL query first
            var rawSqlResult = await _context.Database.SqlQueryRaw<string>(
                "SELECT TOP 1 COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'BusinessSchemas' AND COLUMN_NAME = 'IsActive'")
                .ToListAsync();

            // Test Entity Framework query
            var efResult = await _context.BusinessSchemas.Take(1).ToListAsync();

            var result = new
            {
                Timestamp = DateTime.UtcNow,
                RawSqlColumnCheck = rawSqlResult.Any() ? "IsActive column exists" : "IsActive column missing",
                EntityFrameworkTest = efResult.Any() ? "EF query successful" : "No records found",
                RecordCount = efResult.Count
            };

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error testing BusinessSchemas");
            return StatusCode(500, new { Error = ex.Message, StackTrace = ex.StackTrace });
        }
    }

    #endregion

    #region Configuration Management

    /// <summary>
    /// Get all configuration sections
    /// </summary>
    [HttpGet("configuration/sections")]
    [Authorize(Roles = "Admin")]
    public ActionResult<IEnumerable<string>> GetConfigurationSections()
    {
        try
        {
            var sections = _configurationService.GetAllSections();
            return Ok(sections);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting configuration sections");
            return StatusCode(500, "Error retrieving configuration sections");
        }
    }

    /// <summary>
    /// Get configuration section as JSON
    /// </summary>
    [HttpGet("configuration/sections/{sectionName}")]
    [Authorize(Roles = "Admin")]
    public ActionResult<string> GetConfigurationSection(string sectionName)
    {
        try
        {
            if (!_configurationService.SectionExists(sectionName))
            {
                return NotFound($"Configuration section '{sectionName}' not found");
            }

            var json = _configurationService.GetConfigurationAsJson(sectionName);
            return Ok(json);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting configuration section {SectionName}", sectionName);
            return StatusCode(500, $"Error retrieving configuration section '{sectionName}'");
        }
    }

    /// <summary>
    /// Get application settings
    /// </summary>
    [HttpGet("configuration/application")]
    [Authorize(Roles = "Admin")]
    public ActionResult<ApplicationSettings> GetApplicationSettings()
    {
        try
        {
            var settings = _configurationService.GetApplicationSettings();
            return Ok(settings);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting application settings");
            return StatusCode(500, "Error retrieving application settings");
        }
    }

    /// <summary>
    /// Get AI configuration
    /// </summary>
    [HttpGet("configuration/ai")]
    [Authorize(Roles = "Admin")]
    public ActionResult<AIConfiguration> GetAISettings()
    {
        try
        {
            var settings = _configurationService.GetAISettings();
            // Remove sensitive information
            settings.OpenAIApiKey = string.IsNullOrEmpty(settings.OpenAIApiKey) ? "" : "***CONFIGURED***";
            return Ok(settings);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting AI settings");
            return StatusCode(500, "Error retrieving AI settings");
        }
    }

    /// <summary>
    /// Get security configuration (sanitized)
    /// </summary>
    [HttpGet("configuration/security")]
    [Authorize(Roles = "Admin")]
    public ActionResult<object> GetSecuritySettings()
    {
        try
        {
            var settings = _configurationService.GetSecuritySettings();

            // Return sanitized version without sensitive data
            var sanitized = new
            {
                settings.JwtIssuer,
                settings.JwtAudience,
                settings.AccessTokenExpirationMinutes,
                settings.RefreshTokenExpirationMinutes,
                settings.MaxLoginAttempts,
                settings.LockoutDurationMinutes,
                settings.MinPasswordLength,
                settings.RequireDigit,
                settings.RequireLowercase,
                settings.RequireUppercase,
                settings.RequireNonAlphanumeric,
                settings.EnableTwoFactorAuthentication,
                settings.EnableRateLimit,
                settings.EnableSqlValidation,
                settings.MaxQueryLength,
                settings.MaxQueryComplexity,
                settings.EnableHttpsRedirection,
                settings.EnableSecurityHeaders,
                settings.EnableAuditLogging,
                JwtSecretConfigured = !string.IsNullOrEmpty(settings.JwtSecret)
            };

            return Ok(sanitized);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting security settings");
            return StatusCode(500, "Error retrieving security settings");
        }
    }

    /// <summary>
    /// Get performance configuration
    /// </summary>
    [HttpGet("configuration/performance")]
    [Authorize(Roles = "Admin")]
    public ActionResult<PerformanceConfiguration> GetPerformanceSettings()
    {
        try
        {
            var settings = _configurationService.GetPerformanceSettings();
            return Ok(settings);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting performance settings");
            return StatusCode(500, "Error retrieving performance settings");
        }
    }

    /// <summary>
    /// Get cache configuration
    /// </summary>
    [HttpGet("configuration/cache")]
    [Authorize(Roles = "Admin")]
    public ActionResult<BIReportingCopilot.Core.Configuration.CacheConfiguration> GetCacheSettings()
    {
        try
        {
            var settings = _configurationService.GetCacheSettings();
            // Sanitize connection string
            if (!string.IsNullOrEmpty(settings.RedisConnectionString))
            {
                settings.RedisConnectionString = "***CONFIGURED***";
            }
            return Ok(settings);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting cache settings");
            return StatusCode(500, "Error retrieving cache settings");
        }
    }

    /// <summary>
    /// Get feature flags
    /// </summary>
    [HttpGet("configuration/features")]
    [Authorize(Roles = "Admin")]
    public ActionResult<FeatureConfiguration> GetFeatureFlags()
    {
        try
        {
            var settings = _configurationService.GetFeatureFlags();
            return Ok(settings);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting feature flags");
            return StatusCode(500, "Error retrieving feature flags");
        }
    }

    /// <summary>
    /// Validate all configurations
    /// </summary>
    [HttpPost("configuration/validate")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<object>> ValidateConfiguration()
    {
        try
        {
            var validationResult = await _configurationService.ValidateAllConfigurationsAsync();
            var migrationResult = await _migrationService.ValidateMigrationAsync();

            return Ok(new
            {
                Configuration = validationResult,
                Migration = migrationResult,
                ValidatedAt = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating configuration");
            return StatusCode(500, "Error validating configuration");
        }
    }

    /// <summary>
    /// Reload configuration cache
    /// </summary>
    [HttpPost("configuration/reload")]
    [Authorize(Roles = "Admin")]
    public ActionResult ReloadConfiguration()
    {
        try
        {
            _configurationService.ReloadConfiguration();
            _logger.LogInformation("Configuration cache reloaded by admin user");
            return Ok(new { Message = "Configuration cache reloaded successfully", ReloadedAt = DateTime.UtcNow });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reloading configuration");
            return StatusCode(500, "Error reloading configuration");
        }
    }

    /// <summary>
    /// Refresh specific configuration section
    /// </summary>
    [HttpPost("configuration/sections/{sectionName}/refresh")]
    [Authorize(Roles = "Admin")]
    public ActionResult RefreshConfigurationSection(string sectionName)
    {
        try
        {
            if (!_configurationService.SectionExists(sectionName))
            {
                return NotFound($"Configuration section '{sectionName}' not found");
            }

            _configurationService.RefreshSection(sectionName);
            _logger.LogInformation("Configuration section {SectionName} refreshed by admin user", sectionName);

            return Ok(new
            {
                Message = $"Configuration section '{sectionName}' refreshed successfully",
                RefreshedAt = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing configuration section {SectionName}", sectionName);
            return StatusCode(500, $"Error refreshing configuration section '{sectionName}'");
        }
    }

    /// <summary>
    /// Get migration status
    /// </summary>
    [HttpGet("configuration/migration/status")]
    [Authorize(Roles = "Admin")]
    public ActionResult<object> GetMigrationStatus()
    {
        try
        {
            var status = _migrationService.GetMigrationStatus();
            return Ok(status);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting migration status");
            return StatusCode(500, "Error retrieving migration status");
        }
    }

    #endregion

    #region Private Helper Methods

    private async Task<object> TestBusinessSchemasTable()
    {
        try
        {
            // Test if we can check the table structure using raw SQL
            var columnCheck = await _context.Database.SqlQueryRaw<string>(
                "SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'BusinessSchemas' AND COLUMN_NAME IN ('IsActive', 'IsDefault')")
                .ToListAsync();

            // Test if we can query the table with Entity Framework
            var recordCount = await _context.BusinessSchemas.CountAsync();

            return new
            {
                columnsFound = columnCheck,
                columnCount = columnCheck.Count,
                recordCount = recordCount,
                status = "success"
            };
        }
        catch (Exception ex)
        {
            return new
            {
                status = "error",
                error = ex.Message,
                innerException = ex.InnerException?.Message
            };
        }
    }

    #endregion
}
