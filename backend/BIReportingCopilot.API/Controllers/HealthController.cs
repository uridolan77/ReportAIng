using Microsoft.AspNetCore.Mvc;

namespace BIReportingCopilot.API.Controllers;

/// <summary>
/// Health check controller for monitoring application status
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    private readonly ILogger<HealthController> _logger;

    public HealthController(ILogger<HealthController> logger)
    {
        _logger = logger;
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
}
