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
    /// Get top performing templates
    /// </summary>
    [HttpGet("dashboard/top-templates")]
    public async Task<ActionResult<List<TemplatePerformanceRanking>>> GetTopTemplates()
    {
        try
        {
            // Mock implementation - replace with actual service calls
            var topTemplates = new List<TemplatePerformanceRanking>
            {
                new()
                {
                    TemplateKey = "analytical_gaming_template",
                    TemplateName = "Gaming Analytics Template",
                    IntentType = "Analytical",
                    SuccessRate = 0.92m,
                    UsageCount = 1250,
                    AverageUserRating = 4.3m,
                    AverageProcessingTimeMs = 245,
                    PerformanceScore = 95.2m,
                    Rank = 1,
                    PerformanceCategory = "Excellent"
                }
            };

            return Ok(topTemplates);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving top templates");
            return StatusCode(500, "Error retrieving top templates");
        }
    }

    /// <summary>
    /// Get all templates for management
    /// </summary>
    [HttpGet("templates")]
    public async Task<ActionResult<List<TemplateManagementInfo>>> GetAllTemplates()
    {
        try
        {
            // Mock implementation - replace with actual template service
            var templates = await GetMockTemplates();
            return Ok(templates);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving templates");
            return StatusCode(500, "Error retrieving templates");
        }
    }

    /// <summary>
    /// Get template metrics summary
    /// </summary>
    [HttpGet("templates/metrics")]
    public async Task<ActionResult<TemplateMetricsSummary>> GetTemplateMetrics()
    {
        try
        {
            var templates = await GetMockTemplates();
            var metrics = new TemplateMetricsSummary
            {
                TotalTemplates = templates.Count,
                ActiveTemplates = templates.Count(t => t.IsActive),
                AverageSuccessRate = templates.Average(t => t.SuccessRate ?? 0),
                TemplatesNeedingReview = templates.Count(t => (t.SuccessRate ?? 0) < 0.7m)
            };

            return Ok(metrics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving template metrics");
            return StatusCode(500, "Error retrieving template metrics");
        }
    }

    /// <summary>
    /// Get specific template details
    /// </summary>
    [HttpGet("templates/{id}")]
    public async Task<ActionResult<TemplateManagementInfo>> GetTemplate(int id)
    {
        try
        {
            var templates = await GetMockTemplates();
            var template = templates.FirstOrDefault(t => t.Id == id);
            
            if (template == null)
                return NotFound("Template not found");

            return Ok(template);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving template {TemplateId}", id);
            return StatusCode(500, "Error retrieving template");
        }
    }

    /// <summary>
    /// Create new template
    /// </summary>
    [HttpPost("templates")]
    public async Task<ActionResult<TemplateManagementInfo>> CreateTemplate([FromBody] TemplateCreateRequest request)
    {
        try
        {
            // Mock implementation - replace with actual template service
            var newTemplate = new TemplateManagementInfo
            {
                Id = new Random().Next(1000, 9999),
                Key = request.Key,
                Name = request.Name,
                IntentType = request.IntentType,
                Content = request.Content,
                Description = request.Description,
                Priority = request.Priority,
                IsActive = request.IsActive,
                Tags = request.Tags,
                Version = request.Version,
                Category = request.Category,
                CreatedDate = DateTime.UtcNow,
                UpdatedDate = DateTime.UtcNow
            };

            return CreatedAtAction(nameof(GetTemplate), new { id = newTemplate.Id }, newTemplate);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating template");
            return StatusCode(500, "Error creating template");
        }
    }

    /// <summary>
    /// Update existing template
    /// </summary>
    [HttpPut("templates/{id}")]
    public async Task<ActionResult<TemplateManagementInfo>> UpdateTemplate(int id, [FromBody] TemplateUpdateRequest request)
    {
        try
        {
            var templates = await GetMockTemplates();
            var template = templates.FirstOrDefault(t => t.Id == id);
            
            if (template == null)
                return NotFound("Template not found");

            // Update template properties
            template.Key = request.Key;
            template.Name = request.Name;
            template.IntentType = request.IntentType;
            template.Content = request.Content;
            template.Description = request.Description;
            template.Priority = request.Priority;
            template.IsActive = request.IsActive;
            template.Tags = request.Tags;
            template.Version = request.Version;
            template.Category = request.Category;
            template.UpdatedDate = DateTime.UtcNow;

            return Ok(template);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating template {TemplateId}", id);
            return StatusCode(500, "Error updating template");
        }
    }

    /// <summary>
    /// Delete template
    /// </summary>
    [HttpDelete("templates/{id}")]
    public async Task<ActionResult> DeleteTemplate(int id)
    {
        try
        {
            // Mock implementation - replace with actual template service
            await Task.Delay(100); // Simulate async operation
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting template {TemplateId}", id);
            return StatusCode(500, "Error deleting template");
        }
    }

    /// <summary>
    /// Duplicate template
    /// </summary>
    [HttpPost("templates/{id}/duplicate")]
    public async Task<ActionResult<TemplateManagementInfo>> DuplicateTemplate(int id)
    {
        try
        {
            var templates = await GetMockTemplates();
            var originalTemplate = templates.FirstOrDefault(t => t.Id == id);
            
            if (originalTemplate == null)
                return NotFound("Template not found");

            var duplicatedTemplate = new TemplateManagementInfo
            {
                Id = new Random().Next(1000, 9999),
                Key = originalTemplate.Key + "_copy",
                Name = originalTemplate.Name + " (Copy)",
                IntentType = originalTemplate.IntentType,
                Content = originalTemplate.Content,
                Description = originalTemplate.Description,
                Priority = originalTemplate.Priority,
                IsActive = false, // Start as inactive
                Tags = originalTemplate.Tags,
                Version = "1.0",
                Category = originalTemplate.Category,
                CreatedDate = DateTime.UtcNow,
                UpdatedDate = DateTime.UtcNow
            };

            return CreatedAtAction(nameof(GetTemplate), new { id = duplicatedTemplate.Id }, duplicatedTemplate);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error duplicating template {TemplateId}", id);
            return StatusCode(500, "Error duplicating template");
        }
    }

    /// <summary>
    /// Get template performance analytics
    /// </summary>
    [HttpGet("templates/{id}/performance")]
    public async Task<ActionResult<TemplatePerformanceAnalytics>> GetTemplatePerformance(int id)
    {
        try
        {
            // Mock implementation - replace with actual analytics service
            var performance = new TemplatePerformanceAnalytics
            {
                TemplateId = id,
                SuccessRate = 0.87m,
                UsageCount = 1250,
                AverageRating = 4.3m,
                AverageProcessingTime = 245,
                Insights = new List<string>
                {
                    "Performance is above average for this intent type",
                    "User satisfaction has improved by 12% this month",
                    "Response time is within acceptable limits",
                    "Consider A/B testing with variant templates"
                }
            };

            return Ok(performance);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving template performance {TemplateId}", id);
            return StatusCode(500, "Error retrieving template performance");
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

    private async Task<List<TemplateManagementInfo>> GetMockTemplates()
    {
        // Mock implementation - replace with actual template repository
        await Task.Delay(50); // Simulate async operation

        return new List<TemplateManagementInfo>
        {
            new()
            {
                Id = 1,
                Key = "analytical_gaming_template",
                Name = "Gaming Analytics Template",
                IntentType = "Analytical",
                Content = "Generate analytical SQL for gaming metrics: {USER_QUESTION}\n\nBusiness Context:\n{BUSINESS_CONTEXT}\n\nRelevant Tables:\n{RELEVANT_TABLES}",
                Description = "Template for analytical queries in gaming domain",
                Priority = 100,
                IsActive = true,
                Tags = "gaming,analytics,metrics",
                Version = "2.1",
                Category = "Gaming",
                UsageCount = 1250,
                SuccessRate = 0.87m,
                AverageRating = 4.3m,
                CreatedDate = DateTime.UtcNow.AddDays(-30),
                UpdatedDate = DateTime.UtcNow.AddDays(-5)
            },
            new()
            {
                Id = 2,
                Key = "comparison_template",
                Name = "Comparison Analysis Template",
                IntentType = "Comparison",
                Content = "Generate comparison SQL for: {USER_QUESTION}\n\nComparison Guidelines:\n- Use appropriate comparison operators\n- Include percentage changes\n- Show side-by-side metrics",
                Description = "Template for comparison queries across different dimensions",
                Priority = 90,
                IsActive = true,
                Tags = "comparison,analysis",
                Version = "1.5",
                Category = "General",
                UsageCount = 890,
                SuccessRate = 0.92m,
                AverageRating = 4.1m,
                CreatedDate = DateTime.UtcNow.AddDays(-45),
                UpdatedDate = DateTime.UtcNow.AddDays(-10)
            },
            new()
            {
                Id = 3,
                Key = "trend_analysis_template",
                Name = "Trend Analysis Template",
                IntentType = "Trend",
                Content = "Generate trend analysis SQL for: {USER_QUESTION}\n\nTrend Analysis Requirements:\n- Include time-based grouping\n- Calculate growth rates\n- Identify patterns",
                Description = "Template for trend analysis over time periods",
                Priority = 85,
                IsActive = true,
                Tags = "trend,time-series,analysis",
                Version = "1.8",
                Category = "Analytics",
                UsageCount = 650,
                SuccessRate = 0.78m,
                AverageRating = 3.9m,
                CreatedDate = DateTime.UtcNow.AddDays(-60),
                UpdatedDate = DateTime.UtcNow.AddDays(-15)
            }
        };
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
/// Template performance ranking model
/// </summary>
public class TemplatePerformanceRanking
{
    public string TemplateKey { get; set; } = string.Empty;
    public string TemplateName { get; set; } = string.Empty;
    public string IntentType { get; set; } = string.Empty;
    public decimal SuccessRate { get; set; }
    public int UsageCount { get; set; }
    public decimal AverageUserRating { get; set; }
    public int AverageProcessingTimeMs { get; set; }
    public decimal PerformanceScore { get; set; }
    public int Rank { get; set; }
    public string PerformanceCategory { get; set; } = string.Empty;
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

/// <summary>
/// Template management information
/// </summary>
public class TemplateManagementInfo
{
    public int Id { get; set; }
    public string Key { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string IntentType { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int Priority { get; set; }
    public bool IsActive { get; set; }
    public string? Tags { get; set; }
    public string Version { get; set; } = "1.0";
    public string? Category { get; set; }
    public int? UsageCount { get; set; }
    public decimal? SuccessRate { get; set; }
    public decimal? AverageRating { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime UpdatedDate { get; set; }
}

/// <summary>
/// Template metrics summary
/// </summary>
public class TemplateMetricsSummary
{
    public int TotalTemplates { get; set; }
    public int ActiveTemplates { get; set; }
    public decimal AverageSuccessRate { get; set; }
    public int TemplatesNeedingReview { get; set; }
}

/// <summary>
/// Template create request
/// </summary>
public class TemplateCreateRequest
{
    public string Key { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string IntentType { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int Priority { get; set; } = 100;
    public bool IsActive { get; set; } = true;
    public string? Tags { get; set; }
    public string Version { get; set; } = "1.0";
    public string? Category { get; set; }
}

/// <summary>
/// Template update request
/// </summary>
public class TemplateUpdateRequest
{
    public string Key { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string IntentType { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int Priority { get; set; }
    public bool IsActive { get; set; }
    public string? Tags { get; set; }
    public string Version { get; set; } = string.Empty;
    public string? Category { get; set; }
}

/// <summary>
/// Template performance analytics
/// </summary>
public class TemplatePerformanceAnalytics
{
    public int TemplateId { get; set; }
    public decimal SuccessRate { get; set; }
    public int UsageCount { get; set; }
    public decimal? AverageRating { get; set; }
    public int AverageProcessingTime { get; set; }
    public List<string> Insights { get; set; } = new();
}

#endregion
