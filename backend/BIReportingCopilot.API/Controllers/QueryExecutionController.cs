using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using MediatR;
using BIReportingCopilot.Core.Interfaces;
using BIReportingCopilot.Core.Interfaces.Query;
using BIReportingCopilot.Core.Interfaces.AI;
using BIReportingCopilot.Core.Interfaces.Security;
using BIReportingCopilot.Core.Models;
using BIReportingCopilot.Core.Commands;
using BIReportingCopilot.API.Hubs;
using System.Security.Claims;
using SchemaService = BIReportingCopilot.Core.Interfaces.Schema.ISchemaService;

namespace BIReportingCopilot.API.Controllers;

/// <summary>
/// Core query execution controller for natural language and SQL queries
/// </summary>
[ApiController]
[Route("api/query")]
[Authorize]
public class QueryExecutionController : ControllerBase
{
    private readonly ILogger<QueryExecutionController> _logger;
    private readonly IMediator _mediator;
    private readonly IQueryService _queryService;
    private readonly IAIService _aiService;
    private readonly SchemaService _schemaService;
    private readonly ISqlQueryService _sqlQueryService;
    private readonly IAuditService _auditService;
    private readonly IHubContext<QueryStatusHub> _hubContext;

    public QueryExecutionController(
        ILogger<QueryExecutionController> logger,
        IMediator mediator,
        IQueryService queryService,
        IAIService aiService,
        SchemaService schemaService,
        ISqlQueryService sqlQueryService,
        IAuditService auditService,
        IHubContext<QueryStatusHub> hubContext)
    {
        _logger = logger;
        _mediator = mediator;
        _queryService = queryService;
        _aiService = aiService;
        _schemaService = schemaService;
        _sqlQueryService = sqlQueryService;
        _auditService = auditService;
        _hubContext = hubContext;
    }

    /// <summary>
    /// Execute a natural language query
    /// </summary>
    [HttpPost("execute")]
    public async Task<ActionResult<QueryResponse>> ExecuteNaturalLanguageQuery([FromBody] QueryRequest request)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "anonymous";
            
            _logger.LogInformation("Processing natural language query for user {UserId}: {Query}",
                userId, request.Question);

            // Send initial status
            await _hubContext.Clients.User(userId).SendAsync("QueryStatusUpdate", new
            {
                Status = "processing",
                Message = "Processing your query...",
                Timestamp = DateTime.UtcNow
            });

            var command = new ProcessQueryCommand
            {
                Question = request.Question,
                UserId = userId,
                SessionId = request.SessionId,
                Options = request.Options
            };

            var response = await _mediator.Send(command);

            // Send completion status
            await _hubContext.Clients.User(userId).SendAsync("QueryStatusUpdate", new
            {
                Status = "completed",
                Message = "Query completed successfully",
                Timestamp = DateTime.UtcNow
            });

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing natural language query: {Query}", request.Question);
            
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "anonymous";
            await _hubContext.Clients.User(userId).SendAsync("QueryStatusUpdate", new
            {
                Status = "error",
                Message = "An error occurred while processing your query",
                Timestamp = DateTime.UtcNow
            });

            return StatusCode(500, new { error = "An error occurred while processing your query" });
        }
    }

    /// <summary>
    /// Execute raw SQL query
    /// </summary>
    [HttpPost("execute-sql")]
    public async Task<ActionResult<QueryResponse>> ExecuteRawSQL([FromBody] SqlExecutionRequest request)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "anonymous";
            
            _logger.LogInformation("Executing raw SQL for user {UserId}", userId);

            // Validate SQL (basic security check)
            if (string.IsNullOrWhiteSpace(request.Sql))
            {
                return BadRequest(new { error = "SQL query cannot be empty" });
            }

            // Execute SQL
            var result = await _sqlQueryService.ExecuteQueryAsync(request.Sql);

            // Audit the query execution
            await _auditService.LogAsync("SQL_EXECUTION", userId, "Query", request.Sql,
                new {
                    Success = result.IsSuccessful,
                    ExecutionTimeMs = result.ExecutionTimeMs,
                    Error = result.ErrorMessage
                });

            var response = new QueryResponse
            {
                Success = result.IsSuccessful,
                Data = result.Data,
                Columns = result.Columns?.ToArray(),
                Result = result,
                ExecutionTimeMs = (int)result.ExecutionTimeMs,
                Error = result.ErrorMessage
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing raw SQL: {Sql}", request.Sql);
            return StatusCode(500, new { error = "An error occurred while executing the SQL query" });
        }
    }

    /// <summary>
    /// Refresh schema cache
    /// </summary>
    [HttpPost("refresh-schema")]
    public async Task<ActionResult<object>> RefreshSchema()
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "anonymous";
            _logger.LogInformation("Refreshing schema cache for user {UserId}", userId);

            await _schemaService.RefreshSchemaAsync();
            
            return Ok(new { 
                success = true, 
                message = "Schema cache refreshed successfully",
                timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing schema cache");
            return StatusCode(500, new { error = "Failed to refresh schema cache" });
        }
    }

    /// <summary>
    /// Get debug schema information
    /// </summary>
    [HttpGet("debug-schema")]
    public async Task<ActionResult<object>> GetDebugSchema()
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "anonymous";
            _logger.LogInformation("Getting debug schema for user {UserId}", userId);

            var schema = await _schemaService.GetSchemaAsync();
            
            var debugInfo = new
            {
                TablesCount = schema.Tables.Count,
                Tables = schema.Tables.Select(t => new
                {
                    t.Name,
                    ColumnsCount = t.Columns.Count,
                    Columns = t.Columns.Select(c => new
                    {
                        c.Name,
                        c.DataType,
                        c.IsNullable,
                        c.IsPrimaryKey
                    }).ToList()
                }).ToList(),
                CacheTimestamp = DateTime.UtcNow
            };

            return Ok(debugInfo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting debug schema");
            return StatusCode(500, new { error = "Failed to get debug schema" });
        }
    }
}

// SqlExecutionRequest is already defined elsewhere
