using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BIReportingCopilot.Infrastructure.Data;
using BIReportingCopilot.Infrastructure.Data.Contexts;

namespace BIReportingCopilot.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DiagnosticController : ControllerBase
{
    private readonly BICopilotContext _context;
    private readonly SchemaDbContext _schemaContext;
    private readonly ILogger<DiagnosticController> _logger;

    public DiagnosticController(
        BICopilotContext context,
        SchemaDbContext schemaContext,
        ILogger<DiagnosticController> logger)
    {
        _context = context;
        _schemaContext = schemaContext;
        _logger = logger;
    }

    [HttpGet("database-info")]
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

    [HttpGet("business-schemas-test")]
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
}
