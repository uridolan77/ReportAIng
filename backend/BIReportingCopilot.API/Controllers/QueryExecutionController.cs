using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using MediatR;
using BIReportingCopilot.Core.Interfaces;
using BIReportingCopilot.Core.Interfaces.Query;
using BIReportingCopilot.Core.Interfaces.AI;
using BIReportingCopilot.Core.Interfaces.Security;
using BIReportingCopilot.Core.Interfaces.BusinessContext;
using BIReportingCopilot.Core.Models;
using BIReportingCopilot.Core.Models.BusinessContext;
using BIReportingCopilot.Core.Commands;
using BIReportingCopilot.API.Hubs;
using BIReportingCopilot.Infrastructure.BusinessContext.Enhanced;
using System.Security.Claims;
using SchemaService = BIReportingCopilot.Core.Interfaces.Schema.ISchemaService;

namespace BIReportingCopilot.API.Controllers;

/// <summary>
/// Core query execution controller for natural language and SQL queries
/// </summary>
[ApiController]
[Route("api/query-execution")]
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

    // Enhanced Schema Contextualization System dependencies
    private readonly IBusinessContextAnalyzer _businessContextAnalyzer;
    private readonly IBusinessMetadataRetrievalService _metadataService;
    private readonly IContextualPromptBuilder _promptBuilder;
    private readonly ITokenBudgetManager _tokenBudgetManager;

    public QueryExecutionController(
        ILogger<QueryExecutionController> logger,
        IMediator mediator,
        IQueryService queryService,
        IAIService aiService,
        SchemaService schemaService,
        ISqlQueryService sqlQueryService,
        IAuditService auditService,
        IHubContext<QueryStatusHub> hubContext,
        IBusinessContextAnalyzer businessContextAnalyzer,
        IBusinessMetadataRetrievalService metadataService,
        IContextualPromptBuilder promptBuilder,
        ITokenBudgetManager tokenBudgetManager)
    {
        _logger = logger;
        _mediator = mediator;
        _queryService = queryService;
        _aiService = aiService;
        _schemaService = schemaService;
        _sqlQueryService = sqlQueryService;
        _auditService = auditService;
        _hubContext = hubContext;
        _businessContextAnalyzer = businessContextAnalyzer;
        _metadataService = metadataService;
        _promptBuilder = promptBuilder;
        _tokenBudgetManager = tokenBudgetManager;
    }

    /// <summary>
    /// Execute a natural language query with Enhanced Schema Contextualization System
    /// </summary>
    [HttpPost("execute")]
    public async Task<ActionResult<QueryResponse>> ExecuteNaturalLanguageQuery([FromBody] QueryRequest request)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "anonymous";

            _logger.LogInformation("🚀 [ENHANCED-PIPELINE] Processing natural language query for user {UserId}: {Query}",
                userId, request.Question);

            // Send initial status
            await _hubContext.Clients.User(userId).SendAsync("QueryStatusUpdate", new
            {
                Status = "processing",
                Message = "Processing your query with enhanced context...",
                Timestamp = DateTime.UtcNow
            });

            // Step 1: Business Context Analysis
            _logger.LogInformation("🔍 [ENHANCED-PIPELINE] Step 1: Analyzing business context");
            await _hubContext.Clients.User(userId).SendAsync("QueryStatusUpdate", new
            {
                Status = "processing",
                Message = "Analyzing business context...",
                Timestamp = DateTime.UtcNow
            });

            BusinessContextProfile businessProfile;
            try
            {
                businessProfile = await _businessContextAnalyzer.AnalyzeUserQuestionAsync(
                    request.Question, userId);

                if (businessProfile.ConfidenceScore < 0.3)
                {
                    _logger.LogWarning("⚠️ [ENHANCED-PIPELINE] Low confidence business context analysis: {Confidence:P2} for query: {Query}",
                        businessProfile.ConfidenceScore, request.Question);
                }

                _logger.LogInformation("✅ [ENHANCED-PIPELINE] Business context analysis completed - Confidence: {Confidence:P2}, Domain: {Domain}, Intent: {Intent}",
                    businessProfile.ConfidenceScore, businessProfile.Domain.Name, businessProfile.Intent.Type);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ [ENHANCED-PIPELINE] Business context analysis failed for query: {Query}", request.Question);
                await _hubContext.Clients.User(userId).SendAsync("QueryStatusUpdate", new
                {
                    Status = "error",
                    Message = "Failed to analyze business context",
                    Timestamp = DateTime.UtcNow
                });
                return StatusCode(500, new {
                    success = false,
                    error = "Unable to analyze query context",
                    details = ex.Message
                });
            }

            // Step 2: Token Budget Management
            _logger.LogInformation("📊 [ENHANCED-PIPELINE] Step 2: Managing token budget");
            TokenBudget tokenBudget;
            try
            {
                tokenBudget = await _tokenBudgetManager.CreateTokenBudgetAsync(
                    businessProfile, 4000, 500); // Use default values since QueryOptions doesn't have MaxTokens

                _logger.LogInformation("✅ [ENHANCED-PIPELINE] Token budget created - Max: {MaxTokens}, Reserved: {ReservedTokens}",
                    tokenBudget.MaxTotalTokens, tokenBudget.ReservedResponseTokens);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ [ENHANCED-PIPELINE] Token budget management failed for query: {Query}", request.Question);
                await _hubContext.Clients.User(userId).SendAsync("QueryStatusUpdate", new
                {
                    Status = "error",
                    Message = "Failed to manage token budget",
                    Timestamp = DateTime.UtcNow
                });
                return StatusCode(500, new {
                    success = false,
                    error = "Unable to manage token budget",
                    details = ex.Message
                });
            }

            // Step 3: Schema Retrieval with Business Metadata
            _logger.LogInformation("🗄️ [ENHANCED-PIPELINE] Step 3: Retrieving relevant schema with business metadata");
            await _hubContext.Clients.User(userId).SendAsync("QueryStatusUpdate", new
            {
                Status = "processing",
                Message = "Retrieving relevant database schema...",
                Timestamp = DateTime.UtcNow
            });

            ContextualBusinessSchema schemaMetadata;
            try
            {
                schemaMetadata = await _metadataService.GetRelevantBusinessMetadataAsync(
                    businessProfile, 5); // Use default value since QueryOptions doesn't have MaxTables

                if (!schemaMetadata.RelevantTables.Any())
                {
                    _logger.LogWarning("⚠️ [ENHANCED-PIPELINE] No relevant tables found for query: {Query}", request.Question);
                    await _hubContext.Clients.User(userId).SendAsync("QueryStatusUpdate", new
                    {
                        Status = "error",
                        Message = "No relevant tables found for your query",
                        Timestamp = DateTime.UtcNow
                    });
                    return BadRequest(new {
                        success = false,
                        error = "No relevant tables found for your query. Please try rephrasing your question."
                    });
                }

                _logger.LogInformation("✅ [ENHANCED-PIPELINE] Schema retrieval completed - Tables: {TableCount}, Columns: {ColumnCount}",
                    schemaMetadata.RelevantTables.Count,
                    schemaMetadata.RelevantTables.Sum(t => t.Columns.Count));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ [ENHANCED-PIPELINE] Schema retrieval failed for query: {Query}", request.Question);
                await _hubContext.Clients.User(userId).SendAsync("QueryStatusUpdate", new
                {
                    Status = "error",
                    Message = "Failed to retrieve relevant database schema",
                    Timestamp = DateTime.UtcNow
                });
                return StatusCode(500, new {
                    success = false,
                    error = "Unable to retrieve relevant database schema",
                    details = ex.Message
                });
            }

            // Step 4: Enhanced Prompt Building
            _logger.LogInformation("📝 [ENHANCED-PIPELINE] Step 4: Building business-aware prompt");
            await _hubContext.Clients.User(userId).SendAsync("QueryStatusUpdate", new
            {
                Status = "processing",
                Message = "Building enhanced AI prompt...",
                Timestamp = DateTime.UtcNow
            });

            string enhancedPrompt;
            try
            {
                enhancedPrompt = await _promptBuilder.BuildBusinessAwarePromptAsync(
                    request.Question, businessProfile, schemaMetadata);

                if (string.IsNullOrEmpty(enhancedPrompt))
                {
                    _logger.LogWarning("⚠️ [ENHANCED-PIPELINE] Empty prompt generated for query: {Query}", request.Question);
                    throw new InvalidOperationException("Enhanced prompt generation resulted in empty prompt");
                }

                _logger.LogInformation("✅ [ENHANCED-PIPELINE] Enhanced prompt built - Length: {PromptLength} characters",
                    enhancedPrompt.Length);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ [ENHANCED-PIPELINE] Enhanced prompt building failed for query: {Query}", request.Question);
                await _hubContext.Clients.User(userId).SendAsync("QueryStatusUpdate", new
                {
                    Status = "error",
                    Message = "Failed to build enhanced AI prompt",
                    Timestamp = DateTime.UtcNow
                });
                return StatusCode(500, new {
                    success = false,
                    error = "Unable to build enhanced AI prompt",
                    details = ex.Message
                });
            }

            // Step 5: Continue with existing AI generation pipeline
            _logger.LogInformation("🤖 [ENHANCED-PIPELINE] Step 5: Sending enhanced command to processing pipeline");
            await _hubContext.Clients.User(userId).SendAsync("QueryStatusUpdate", new
            {
                Status = "processing",
                Message = "Generating SQL with enhanced context...",
                Timestamp = DateTime.UtcNow
            });

            QueryResponse response;
            try
            {
                var command = new ProcessQueryCommand
                {
                    Question = request.Question,
                    UserId = userId,
                    SessionId = request.SessionId,
                    Options = request.Options,
                    // Enhanced properties for schema context
                    BusinessProfile = businessProfile,
                    SchemaMetadata = schemaMetadata,
                    EnhancedPrompt = enhancedPrompt
                };

                response = await _mediator.Send(command);

                if (!response.Success)
                {
                    _logger.LogError("❌ [ENHANCED-PIPELINE] Query processing failed - Error: {Error}", response.Error);
                    await _hubContext.Clients.User(userId).SendAsync("QueryStatusUpdate", new
                    {
                        Status = "error",
                        Message = "Query processing failed",
                        Timestamp = DateTime.UtcNow
                    });
                    return BadRequest(response);
                }

                // Send completion status
                await _hubContext.Clients.User(userId).SendAsync("QueryStatusUpdate", new
                {
                    Status = "completed",
                    Message = "Query completed successfully with enhanced context",
                    Timestamp = DateTime.UtcNow
                });

                _logger.LogInformation("🎉 [ENHANCED-PIPELINE] Query processing completed successfully - Success: {Success}, Confidence: {Confidence:P2}",
                    response.Success, response.Confidence);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ [ENHANCED-PIPELINE] Command processing failed for query: {Query}", request.Question);
                await _hubContext.Clients.User(userId).SendAsync("QueryStatusUpdate", new
                {
                    Status = "error",
                    Message = "Command processing failed",
                    Timestamp = DateTime.UtcNow
                });
                return StatusCode(500, new {
                    success = false,
                    error = "Command processing failed",
                    details = ex.Message
                });
            }

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ [ENHANCED-PIPELINE] Error processing enhanced natural language query: {Query}", request.Question);

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "anonymous";
            await _hubContext.Clients.User(userId).SendAsync("QueryStatusUpdate", new
            {
                Status = "error",
                Message = "An error occurred while processing your query with enhanced context",
                Timestamp = DateTime.UtcNow
            });

            return StatusCode(500, new {
                success = false,
                error = "Query execution failed with enhanced pipeline",
                details = ex.Message
            });
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
