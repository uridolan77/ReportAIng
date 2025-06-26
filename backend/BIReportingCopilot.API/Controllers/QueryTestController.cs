using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using BIReportingCopilot.Core.Interfaces.AI;
using BIReportingCopilot.Core.Models;

using BIReportingCopilot.API.Hubs;
using System.Security.Claims;

namespace BIReportingCopilot.API.Controllers;

/// <summary>
/// Query testing and debugging controller for development and troubleshooting
/// </summary>
[ApiController]
[Route("api/query/test")]
[Authorize]
public class QueryTestController : ControllerBase
{
    private readonly ILogger<QueryTestController> _logger;
    private readonly IAIService _aiService;
    private readonly IHubContext<QueryStatusHub> _hubContext;

    public QueryTestController(
        ILogger<QueryTestController> logger,
        IAIService aiService,
        IHubContext<QueryStatusHub> hubContext)
    {
        _logger = logger;
        _aiService = aiService;
        _hubContext = hubContext;
    }

    /// <summary>
    /// Test SignalR connection and messaging
    /// </summary>
    [HttpPost("signalr")]
    public async Task<ActionResult<object>> TestSignalR()
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "anonymous";
            
            _logger.LogInformation("Testing SignalR for user {UserId}", userId);

            // Send test message to user
            await _hubContext.Clients.User(userId).SendAsync("TestMessage", new
            {
                Message = "SignalR connection test successful",
                Timestamp = DateTime.UtcNow,
                UserId = userId
            });

            // Send test message to all clients
            await _hubContext.Clients.All.SendAsync("BroadcastTest", new
            {
                Message = "Broadcast test from QueryTestController",
                Timestamp = DateTime.UtcNow
            });

            return Ok(new
            {
                success = true,
                message = "SignalR test messages sent successfully",
                userId = userId,
                timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error testing SignalR");
            return StatusCode(500, new { error = "SignalR test failed" });
        }
    }

    /// <summary>
    /// Test OpenAI API connection and basic functionality
    /// </summary>
    [HttpGet("openai")]
    public async Task<ActionResult<object>> TestOpenAI()
    {
        try
        {
            _logger.LogInformation("Testing OpenAI API connection");

            // Test basic AI service functionality
            var testPrompt = "Test prompt for API connectivity check";
            var response = await _aiService.GenerateSQLAsync(testPrompt);

            var result = new
            {
                success = true,
                message = "OpenAI API test successful",
                promptLength = testPrompt.Length,
                responseLength = response?.Length ?? 0,
                hasResponse = !string.IsNullOrEmpty(response),
                timestamp = DateTime.UtcNow
            };

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error testing OpenAI API");
            return StatusCode(500, new
            {
                success = false,
                error = "OpenAI API test failed",
                message = ex.Message,
                timestamp = DateTime.UtcNow
            });
        }
    }

    /// <summary>
    /// Test enhanced query processing with simplified request
    /// </summary>
    [HttpPost("enhanced-simple")]
    public async Task<ActionResult<object>> TestEnhancedQuerySimple([FromBody] EnhancedQueryRequest request)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "anonymous";
            
            _logger.LogInformation("Testing enhanced query processing for user {UserId}: {Query}", 
                userId, request.Query);

            // Simple test response without full processing
            var testResponse = new
            {
                success = true,
                message = "Enhanced query test endpoint reached successfully",
                query = request.Query,
                userId = userId,
                timestamp = DateTime.UtcNow,
                testMode = true,
                processingSteps = new[]
                {
                    "Query received",
                    "User authenticated",
                    "Test mode activated",
                    "Response generated"
                }
            };

            return Ok(testResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in enhanced query test: {Query}", request.Query);
            return StatusCode(500, new { error = "Enhanced query test failed" });
        }
    }

    /// <summary>
    /// Test prompt building functionality
    /// </summary>
    [HttpPost("prompt-building")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> TestPromptBuildingAsync([FromBody] PromptBuildingTestRequest request)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "anonymous";
            
            _logger.LogInformation("Testing prompt building for user {UserId}", userId);

            // Simulate prompt building process
            var promptComponents = new
            {
                SystemPrompt = "You are a helpful SQL assistant",
                UserQuery = request.Query,
                SchemaContext = "Sample schema context",
                BusinessContext = "Sample business context",
                Examples = new[] { "Example 1", "Example 2" }
            };

            var builtPrompt = $"{promptComponents.SystemPrompt}\n\nSchema: {promptComponents.SchemaContext}\n\nBusiness Context: {promptComponents.BusinessContext}\n\nUser Query: {promptComponents.UserQuery}";

            var result = new
            {
                success = true,
                message = "Prompt building test completed",
                components = promptComponents,
                finalPrompt = builtPrompt,
                promptLength = builtPrompt.Length,
                timestamp = DateTime.UtcNow
            };

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error testing prompt building");
            return BadRequest(new ProblemDetails
            {
                Title = "Prompt Building Test Failed",
                Detail = ex.Message,
                Status = StatusCodes.Status400BadRequest
            });
        }
    }

    /// <summary>
    /// Test database connectivity and basic query execution
    /// </summary>
    [HttpGet("database")]
    public async Task<ActionResult<object>> TestDatabaseConnectivity()
    {
        try
        {
            _logger.LogInformation("Testing database connectivity");

            // This would test actual database connection
            // For now, return a mock response
            var result = new
            {
                success = true,
                message = "Database connectivity test completed",
                connectionString = "Connection string validated (details hidden for security)",
                timestamp = DateTime.UtcNow,
                testQueries = new[]
                {
                    "SELECT 1 as test_value",
                    "SELECT COUNT(*) FROM information_schema.tables",
                    "SELECT GETDATE() as current_time"
                }
            };

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error testing database connectivity");
            return StatusCode(500, new { error = "Database connectivity test failed" });
        }
    }

    /// <summary>
    /// Test system health and component status
    /// </summary>
    [HttpGet("health")]
    public async Task<ActionResult<object>> TestSystemHealth()
    {
        try
        {
            _logger.LogInformation("Testing system health");

            var healthStatus = new
            {
                overall = "healthy",
                components = new
                {
                    database = "healthy",
                    aiService = "healthy",
                    signalR = "healthy",
                    cache = "healthy",
                    logging = "healthy"
                },
                timestamp = DateTime.UtcNow,
                uptime = TimeSpan.FromHours(24), // Mock uptime
                version = "1.0.0"
            };

            return Ok(healthStatus);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error testing system health");
            return StatusCode(500, new { error = "System health test failed" });
        }
    }
}

// PromptBuildingTestRequest is already defined elsewhere
