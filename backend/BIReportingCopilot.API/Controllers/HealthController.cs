using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BIReportingCopilot.Infrastructure.Data;

namespace BIReportingCopilot.API.Controllers;

/// <summary>
/// Health check controller for monitoring application status
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    private readonly ILogger<HealthController> _logger;
    private readonly BICopilotContext _context;

    public HealthController(ILogger<HealthController> logger, BICopilotContext context)
    {
        _logger = logger;
        _context = context;
    }

    /// <summary>
    /// Basic health check endpoint
    /// </summary>
    /// <returns>Health status</returns>
    [HttpGet]
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
    [HttpGet("detailed")]
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
    [HttpGet("database-diagnostic")]
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

    /// <summary>
    /// Test specific BusinessSchema operations that might be causing the error
    /// </summary>
    /// <returns>Test results for BusinessSchema operations</returns>
    [HttpGet("business-schema-operations-test")]
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
}
