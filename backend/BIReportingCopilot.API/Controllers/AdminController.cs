using Microsoft.AspNetCore.Mvc;

namespace BIReportingCopilot.API.Controllers;

/// <summary>
/// Admin controller for managing BCAPB system administration
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AdminController : ControllerBase
{
    private readonly ILogger<AdminController> _logger;

    public AdminController(ILogger<AdminController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Get dashboard metrics overview
    /// </summary>
    [HttpGet("dashboard/metrics")]
    public async Task<ActionResult<DashboardMetrics>> GetDashboardMetrics()
    {
        try
        {
            // Mock implementation - replace with actual service calls
            var dashboardMetrics = new DashboardMetrics
            {
                TotalQueries = 1250,
                SuccessRate = 0.87m,
                AvgResponseTime = 245,
                ActiveUsers = 23,
                QueriesChange = CalculateChange(1250, "queries"),
                SuccessRateChange = CalculateChange(0.87m, "successRate"),
                ResponseTimeChange = CalculateChange(245, "responseTime"),
                ActiveUsersChange = CalculateChange(23, "activeUsers"),
                LastUpdated = DateTime.UtcNow
            };

            return Ok(dashboardMetrics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving dashboard metrics");
            return StatusCode(500, "Error retrieving dashboard metrics");
        }
    }

    /// <summary>
    /// Get active system alerts
    /// </summary>
    [HttpGet("dashboard/alerts")]
    public async Task<ActionResult<List<PerformanceAlert>>> GetActiveAlerts()
    {
        try
        {
            // Mock implementation - replace with actual service calls
            var alerts = new List<PerformanceAlert>
            {
                new()
                {
                    AlertId = "alert-001",
                    AlertType = "Performance",
                    Severity = AlertSeverity.Warning,
                    Title = "Response Time Above Threshold",
                    Description = "Average response time has exceeded 500ms threshold",
                    MetricName = "ResponseTime",
                    CurrentValue = 650,
                    ThresholdValue = 500,
                    TriggeredAt = DateTime.UtcNow.AddMinutes(-15),
                    IsResolved = false
                }
            };

            return Ok(alerts.Where(a => !a.IsResolved).ToList());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving alerts");
            return StatusCode(500, "Error retrieving alerts");
        }
    }







    /// <summary>
    /// Dismiss an alert
    /// </summary>
    [HttpPost("alerts/{alertId}/dismiss")]
    public async Task<ActionResult> DismissAlert(string alertId)
    {
        try
        {
            // Mock implementation - replace with actual alert service
            await Task.Delay(100); // Simulate async operation
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error dismissing alert {AlertId}", alertId);
            return StatusCode(500, "Error dismissing alert");
        }
    }

    #region Private Helper Methods

    private decimal CalculateChange(decimal currentValue, string metricType)
    {
        // Mock implementation - replace with actual historical data comparison
        var random = new Random();
        return (decimal)(random.NextDouble() * 20 - 10); // Random change between -10% and +10%
    }



    #endregion
}

#region Admin Data Models

/// <summary>
/// Performance alert model
/// </summary>
public class PerformanceAlert
{
    public string AlertId { get; set; } = string.Empty;
    public string AlertType { get; set; } = string.Empty;
    public AlertSeverity Severity { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string MetricName { get; set; } = string.Empty;
    public decimal CurrentValue { get; set; }
    public decimal ThresholdValue { get; set; }
    public DateTime TriggeredAt { get; set; }
    public bool IsResolved { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public Dictionary<string, object> AdditionalData { get; set; } = new();
}



/// <summary>
/// Alert severity enum
/// </summary>
public enum AlertSeverity
{
    Info,
    Warning,
    Critical
}

/// <summary>
/// Dashboard metrics overview
/// </summary>
public class DashboardMetrics
{
    public int TotalQueries { get; set; }
    public decimal SuccessRate { get; set; }
    public int AvgResponseTime { get; set; }
    public int ActiveUsers { get; set; }
    public decimal QueriesChange { get; set; }
    public decimal SuccessRateChange { get; set; }
    public decimal ResponseTimeChange { get; set; }
    public decimal ActiveUsersChange { get; set; }
    public DateTime LastUpdated { get; set; }
}



#endregion
