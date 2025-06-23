using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using BIReportingCopilot.Core.Interfaces.Analytics;
using BIReportingCopilot.Core.Models.Analytics;
using System.Security.Claims;

namespace BIReportingCopilot.API.Controllers;

/// <summary>
/// Template Analytics Controller - Handles template performance, A/B testing, and analytics
/// </summary>
[ApiController]
[Route("api/templateanalytics")]
// [Authorize] // Temporarily disabled for development
public class TemplateAnalyticsController : ControllerBase
{
    private readonly ILogger<TemplateAnalyticsController> _logger;

    public TemplateAnalyticsController(
        ILogger<TemplateAnalyticsController> logger)
    {
        _logger = logger;
    }

    #region Comprehensive Analytics Dashboard

    /// <summary>
    /// Get comprehensive analytics dashboard data
    /// </summary>
    [HttpGet("dashboard/comprehensive")]
    public async Task<ActionResult<object>> GetComprehensiveDashboard(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] string? intentType = null)
    {
        try
        {
            var start = startDate ?? DateTime.UtcNow.AddDays(-30);
            var end = endDate ?? DateTime.UtcNow;

            _logger.LogInformation("Getting comprehensive dashboard data from {StartDate} to {EndDate}", start, end);

            // Mock data for now - replace with actual service calls
            var dashboardData = new
            {
                performanceOverview = new
                {
                    totalTemplates = 45,
                    activeTemplates = 38,
                    totalUsage = 12847,
                    overallSuccessRate = 0.94,
                    totalCostSavings = 2847.50m,
                    performanceScore = 0.89
                },
                abTestingOverview = new
                {
                    totalActiveTests = 3,
                    completedTests = 12,
                    runningTests = 3,
                    pendingTests = 2
                },
                activeAlerts = new[]
                {
                    new { Id = 1, Severity = "warning", Message = "Template 'legacy_query_builder' showing decreased performance", Timestamp = DateTime.UtcNow.AddHours(-2) },
                    new { Id = 2, Severity = "info", Message = "A/B test for 'sql_optimization' template completed successfully", Timestamp = DateTime.UtcNow.AddHours(-6) }
                },
                performanceTrends = new[]
                {
                    new { Date = start.AddDays(0).ToString("yyyy-MM-dd"), SuccessRate = 0.92, Usage = 145, ResponseTime = 1.2 },
                    new { Date = start.AddDays(1).ToString("yyyy-MM-dd"), SuccessRate = 0.94, Usage = 167, ResponseTime = 1.1 },
                    new { Date = start.AddDays(2).ToString("yyyy-MM-dd"), SuccessRate = 0.91, Usage = 134, ResponseTime = 1.3 },
                    new { Date = start.AddDays(3).ToString("yyyy-MM-dd"), SuccessRate = 0.96, Usage = 189, ResponseTime = 1.0 },
                    new { Date = start.AddDays(4).ToString("yyyy-MM-dd"), SuccessRate = 0.93, Usage = 156, ResponseTime = 1.2 }
                },
                topPerformingTemplates = new[]
                {
                    new { TemplateKey = "sql_generation_advanced", SuccessRate = 0.98, Usage = 2847, ResponseTime = 0.9 },
                    new { TemplateKey = "data_analysis_comprehensive", SuccessRate = 0.96, Usage = 2134, ResponseTime = 1.1 },
                    new { TemplateKey = "report_generation_standard", SuccessRate = 0.94, Usage = 1876, ResponseTime = 1.3 }
                }
            };

            return Ok(dashboardData);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting comprehensive dashboard data");
            return StatusCode(500, new { error = "Failed to retrieve dashboard data" });
        }
    }

    /// <summary>
    /// Get performance trends data
    /// </summary>
    [HttpGet("trends")]
    public async Task<ActionResult<object>> GetPerformanceTrends(
        [FromQuery] string? templateKey = null,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] string granularity = "daily")
    {
        try
        {
            var start = startDate ?? DateTime.UtcNow.AddDays(-30);
            var end = endDate ?? DateTime.UtcNow;

            _logger.LogInformation("Getting performance trends for template {TemplateKey} from {StartDate} to {EndDate}", 
                templateKey ?? "all", start, end);

            // Mock trends data
            var dataPoints = new List<object>();
            var days = (int)(end - start).TotalDays;

            for (int i = 0; i <= days; i++)
            {
                var date = start.AddDays(i);
                dataPoints.Add(new
                {
                    timestamp = date,
                    averageSuccessRate = 0.90 + (Random.Shared.NextDouble() * 0.1),
                    averageConfidenceScore = 0.85 + (Random.Shared.NextDouble() * 0.15),
                    totalUsage = Random.Shared.Next(100, 300),
                    averageResponseTime = 1.0 + (Random.Shared.NextDouble() * 0.5),
                    errorCount = Random.Shared.Next(0, 10)
                });
            }

            var trends = new
            {
                granularity = granularity,
                dataPoints = dataPoints,
                summary = new
                {
                    totalDataPoints = dataPoints.Count,
                    dateRange = new { start, end }
                }
            };

            return Ok(trends);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting performance trends");
            return StatusCode(500, new { error = "Failed to retrieve performance trends" });
        }
    }

    /// <summary>
    /// Get usage insights data
    /// </summary>
    [HttpGet("insights/usage")]
    public async Task<ActionResult<object>> GetUsageInsights(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] string? intentType = null)
    {
        try
        {
            var start = startDate ?? DateTime.UtcNow.AddDays(-30);
            var end = endDate ?? DateTime.UtcNow;

            _logger.LogInformation("Getting usage insights from {StartDate} to {EndDate}", start, end);

            var insights = new
            {
                totalUsage = 12847,
                averageSuccessRate = 0.94,
                usageGrowth = 0.15,
                peakUsageHours = new[] { 9, 10, 14, 15, 16 },
                usageByIntentType = new Dictionary<string, int>
                {
                    { "sql_generation", 4521 },
                    { "data_analysis", 3847 },
                    { "report_generation", 2156 },
                    { "query_optimization", 1523 },
                    { "other", 800 }
                },
                topPerformingTemplates = new[]
                {
                    new { templateKey = "sql_generation_advanced", templateName = "Advanced SQL Generation", successRate = 0.98, totalUsages = 2847 },
                    new { templateKey = "data_analysis_comprehensive", templateName = "Comprehensive Data Analysis", successRate = 0.96, totalUsages = 2134 },
                    new { templateKey = "report_generation_standard", templateName = "Standard Report Generation", successRate = 0.94, totalUsages = 1876 }
                },
                underperformingTemplates = new[]
                {
                    new { templateKey = "legacy_query_builder", templateName = "Legacy Query Builder", successRate = 0.72, totalUsages = 456 },
                    new { templateKey = "basic_analysis_v1", templateName = "Basic Analysis V1", successRate = 0.78, totalUsages = 234 },
                    new { templateKey = "old_report_generator", templateName = "Old Report Generator", successRate = 0.81, totalUsages = 123 }
                },
                insights = new[]
                {
                    "SQL Generation templates show highest usage with 35.2% of total requests",
                    "Peak usage occurs during business hours (9-10 AM and 2-4 PM)",
                    "Overall success rate has improved by 15% compared to last month",
                    "Legacy templates are showing decreased performance and should be updated"
                }
            };

            return Ok(insights);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting usage insights");
            return StatusCode(500, new { error = "Failed to retrieve usage insights" });
        }
    }

    /// <summary>
    /// Get quality metrics data
    /// </summary>
    [HttpGet("metrics/quality")]
    public async Task<ActionResult<object>> GetQualityMetrics(
        [FromQuery] string? intentType = null)
    {
        try
        {
            _logger.LogInformation("Getting quality metrics for intent type {IntentType}", intentType ?? "all");

            var qualityMetrics = new
            {
                OverallQualityScore = 0.87,
                QualityDistribution = new[]
                {
                    new { Range = "90-100%", Count = 12, Percentage = 26.7 },
                    new { Range = "80-89%", Count = 18, Percentage = 40.0 },
                    new { Range = "70-79%", Count = 10, Percentage = 22.2 },
                    new { Range = "60-69%", Count = 4, Percentage = 8.9 },
                    new { Range = "Below 60%", Count = 1, Percentage = 2.2 }
                },
                QualityFactors = new
                {
                    Accuracy = 0.92,
                    Relevance = 0.89,
                    Completeness = 0.85,
                    Clarity = 0.91,
                    Efficiency = 0.88
                },
                ImprovementSuggestions = new[]
                {
                    "Consider adding more context to templates with low relevance scores",
                    "Review templates with completion rates below 85%",
                    "Optimize response time for templates exceeding 2 seconds"
                }
            };

            return Ok(qualityMetrics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting quality metrics");
            return StatusCode(500, new { error = "Failed to retrieve quality metrics" });
        }
    }

    /// <summary>
    /// Get real-time analytics data
    /// </summary>
    [HttpGet("realtime")]
    public async Task<ActionResult<object>> GetRealTimeAnalytics()
    {
        try
        {
            _logger.LogInformation("Getting real-time analytics data");

            var realTimeData = new
            {
                activeUsers = Random.Shared.Next(15, 45),
                queriesPerMinute = Random.Shared.Next(80, 150),
                currentSuccessRate = 0.90 + (Random.Shared.NextDouble() * 0.1),
                averageResponseTime = 1.0 + (Random.Shared.NextDouble() * 0.5),
                errorsInLastHour = Random.Shared.Next(0, 15),
                lastUpdated = DateTime.UtcNow,
                recentActivities = new[]
                {
                    new { Timestamp = DateTime.UtcNow.AddMinutes(-1), Event = "Template executed", TemplateKey = "sql_generation_advanced", UserId = "user123" },
                    new { Timestamp = DateTime.UtcNow.AddMinutes(-2), Event = "A/B test started", TemplateKey = "data_analysis_v2", UserId = "admin" },
                    new { Timestamp = DateTime.UtcNow.AddMinutes(-3), Event = "Performance alert", TemplateKey = "legacy_query_builder", UserId = "system" }
                },
                systemHealth = new
                {
                    status = "healthy",
                    responseTime = 1.2,
                    errorRate = 0.02,
                    throughput = 145.7
                }
            };

            return Ok(realTimeData);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting real-time analytics");
            return StatusCode(500, new { error = "Failed to retrieve real-time analytics" });
        }
    }

    #endregion

    #region Performance Dashboard

    /// <summary>
    /// Get performance dashboard data
    /// </summary>
    [HttpGet("dashboard")]
    public async Task<ActionResult<object>> GetPerformanceDashboard(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] string? intentType = null)
    {
        try
        {
            var start = startDate ?? DateTime.UtcNow.AddDays(-30);
            var end = endDate ?? DateTime.UtcNow;

            _logger.LogInformation("Getting performance dashboard data from {StartDate} to {EndDate}", start, end);

            var dashboardData = new
            {
                Summary = new
                {
                    TotalTemplates = 45,
                    ActiveTemplates = 38,
                    AverageSuccessRate = 0.94,
                    AverageResponseTime = 1.2,
                    TotalUsage = 12847
                },
                PerformanceMetrics = new[]
                {
                    new { TemplateKey = "sql_generation_advanced", SuccessRate = 0.98, ResponseTime = 0.9, Usage = 2847 },
                    new { TemplateKey = "data_analysis_comprehensive", SuccessRate = 0.96, ResponseTime = 1.1, Usage = 2134 },
                    new { TemplateKey = "report_generation_standard", SuccessRate = 0.94, ResponseTime = 1.3, Usage = 1876 }
                },
                Alerts = new[]
                {
                    new { Id = 1, Severity = "warning", Message = "Template performance degraded", TemplateKey = "legacy_query_builder" },
                    new { Id = 2, Severity = "info", Message = "A/B test completed", TemplateKey = "sql_optimization" }
                }
            };

            return Ok(dashboardData);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting performance dashboard data");
            return StatusCode(500, new { error = "Failed to retrieve performance dashboard data" });
        }
    }

    /// <summary>
    /// Get performance alerts
    /// </summary>
    [HttpGet("alerts")]
    public async Task<ActionResult<object[]>> GetPerformanceAlerts(
        [FromQuery] string? severity = null,
        [FromQuery] bool? resolved = null)
    {
        try
        {
            _logger.LogInformation("Getting performance alerts with severity {Severity}, resolved {Resolved}",
                severity ?? "all", resolved?.ToString() ?? "all");

            var alerts = new[]
            {
                new { Id = 1, Severity = "warning", Message = "Template 'legacy_query_builder' showing decreased performance",
                      TemplateKey = "legacy_query_builder", Timestamp = DateTime.UtcNow.AddHours(-2), Resolved = false },
                new { Id = 2, Severity = "info", Message = "A/B test for 'sql_optimization' template completed successfully",
                      TemplateKey = "sql_optimization", Timestamp = DateTime.UtcNow.AddHours(-6), Resolved = true },
                new { Id = 3, Severity = "error", Message = "Template 'data_export_v1' failing with high error rate",
                      TemplateKey = "data_export_v1", Timestamp = DateTime.UtcNow.AddHours(-12), Resolved = false }
            };

            // Filter by severity and resolved status if specified
            var filteredAlerts = alerts.AsEnumerable();

            if (!string.IsNullOrEmpty(severity))
            {
                filteredAlerts = filteredAlerts.Where(a => a.Severity.Equals(severity, StringComparison.OrdinalIgnoreCase));
            }

            if (resolved.HasValue)
            {
                filteredAlerts = filteredAlerts.Where(a => a.Resolved == resolved.Value);
            }

            return Ok(filteredAlerts.ToArray());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting performance alerts");
            return StatusCode(500, new { error = "Failed to retrieve performance alerts" });
        }
    }

    #endregion

    #region A/B Testing

    /// <summary>
    /// Get A/B tests
    /// </summary>
    [HttpGet("abtests")]
    public async Task<ActionResult<object[]>> GetABTests(
        [FromQuery] string? status = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        try
        {
            _logger.LogInformation("Getting A/B tests with status {Status}, page {Page}, pageSize {PageSize}",
                status ?? "all", page, pageSize);

            var abTests = new[]
            {
                new {
                    Id = 1,
                    TestName = "SQL Generation Optimization",
                    OriginalTemplateKey = "sql_generation_v1",
                    VariantTemplateKey = "sql_generation_v2",
                    Status = "running",
                    TrafficSplitPercent = 50,
                    StartDate = DateTime.UtcNow.AddDays(-7),
                    EndDate = DateTime.UtcNow.AddDays(7),
                    OriginalSuccessRate = 0.92,
                    VariantSuccessRate = 0.96,
                    StatisticalSignificance = 0.85
                },
                new {
                    Id = 2,
                    TestName = "Data Analysis Enhancement",
                    OriginalTemplateKey = "data_analysis_v1",
                    VariantTemplateKey = "data_analysis_v2",
                    Status = "completed",
                    TrafficSplitPercent = 30,
                    StartDate = DateTime.UtcNow.AddDays(-21),
                    EndDate = DateTime.UtcNow.AddDays(-7),
                    OriginalSuccessRate = 0.89,
                    VariantSuccessRate = 0.94,
                    StatisticalSignificance = 0.95
                }
            };

            // Filter by status if specified
            var filteredTests = abTests.AsEnumerable();
            if (!string.IsNullOrEmpty(status))
            {
                filteredTests = filteredTests.Where(t => t.Status.Equals(status, StringComparison.OrdinalIgnoreCase));
            }

            // Apply pagination
            var pagedTests = filteredTests
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToArray();

            return Ok(pagedTests);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting A/B tests");
            return StatusCode(500, new { error = "Failed to retrieve A/B tests" });
        }
    }

    /// <summary>
    /// Get specific A/B test
    /// </summary>
    [HttpGet("abtests/{testId}")]
    public async Task<ActionResult<object>> GetABTest(int testId)
    {
        try
        {
            _logger.LogInformation("Getting A/B test {TestId}", testId);

            // Mock A/B test data
            var abTest = new
            {
                Id = testId,
                TestName = "SQL Generation Optimization",
                OriginalTemplateKey = "sql_generation_v1",
                VariantTemplateKey = "sql_generation_v2",
                Status = "running",
                TrafficSplitPercent = 50,
                StartDate = DateTime.UtcNow.AddDays(-7),
                EndDate = DateTime.UtcNow.AddDays(7),
                CreatedBy = "admin@company.com",
                OriginalUsageCount = 1247,
                VariantUsageCount = 1189,
                OriginalSuccessRate = 0.92,
                VariantSuccessRate = 0.96,
                StatisticalSignificance = 0.85,
                ConfidenceLevel = 95,
                MinimumSampleSize = 1000
            };

            return Ok(abTest);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting A/B test {TestId}", testId);
            return StatusCode(500, new { error = "Failed to retrieve A/B test" });
        }
    }

    #endregion

    #region Template Management

    /// <summary>
    /// Get template management dashboard
    /// </summary>
    [HttpGet("templates/dashboard")]
    public async Task<ActionResult<object>> GetTemplateManagementDashboard()
    {
        try
        {
            _logger.LogInformation("Getting template management dashboard");

            var dashboard = new
            {
                Summary = new
                {
                    TotalTemplates = 45,
                    ActiveTemplates = 38,
                    DraftTemplates = 5,
                    ArchivedTemplates = 2,
                    RecentlyUpdated = 8
                },
                TemplatesByCategory = new[]
                {
                    new { Category = "SQL Generation", Count = 15, Active = 13 },
                    new { Category = "Data Analysis", Count = 12, Active = 11 },
                    new { Category = "Report Generation", Count = 8, Active = 7 },
                    new { Category = "Query Optimization", Count = 6, Active = 5 },
                    new { Category = "Other", Count = 4, Active = 2 }
                },
                RecentActivity = new[]
                {
                    new { Action = "Updated", TemplateKey = "sql_generation_advanced", User = "john.doe", Timestamp = DateTime.UtcNow.AddHours(-2) },
                    new { Action = "Created", TemplateKey = "data_export_v3", User = "jane.smith", Timestamp = DateTime.UtcNow.AddHours(-4) },
                    new { Action = "Archived", TemplateKey = "legacy_report_v1", User = "admin", Timestamp = DateTime.UtcNow.AddHours(-6) }
                }
            };

            return Ok(dashboard);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting template management dashboard");
            return StatusCode(500, new { error = "Failed to retrieve template management dashboard" });
        }
    }

    /// <summary>
    /// Get template with metrics
    /// </summary>
    [HttpGet("templates/{templateKey}")]
    public async Task<ActionResult<object>> GetTemplateWithMetrics(string templateKey)
    {
        try
        {
            _logger.LogInformation("Getting template {TemplateKey} with metrics", templateKey);

            var template = new
            {
                TemplateKey = templateKey,
                Name = "Advanced SQL Generation",
                Description = "Generates optimized SQL queries for complex data analysis",
                Category = "SQL Generation",
                Version = "2.1.0",
                Status = "active",
                CreatedBy = "john.doe@company.com",
                CreatedDate = DateTime.UtcNow.AddDays(-45),
                LastModified = DateTime.UtcNow.AddDays(-3),
                Metrics = new
                {
                    TotalUsage = 2847,
                    SuccessRate = 0.98,
                    AverageResponseTime = 0.9,
                    ErrorRate = 0.02,
                    UserSatisfaction = 4.7,
                    CostPerExecution = 0.003m
                },
                PerformanceTrend = new[]
                {
                    new { Date = DateTime.UtcNow.AddDays(-7).ToString("yyyy-MM-dd"), Usage = 145, SuccessRate = 0.97 },
                    new { Date = DateTime.UtcNow.AddDays(-6).ToString("yyyy-MM-dd"), Usage = 167, SuccessRate = 0.98 },
                    new { Date = DateTime.UtcNow.AddDays(-5).ToString("yyyy-MM-dd"), Usage = 134, SuccessRate = 0.96 },
                    new { Date = DateTime.UtcNow.AddDays(-4).ToString("yyyy-MM-dd"), Usage = 189, SuccessRate = 0.99 },
                    new { Date = DateTime.UtcNow.AddDays(-3).ToString("yyyy-MM-dd"), Usage = 156, SuccessRate = 0.98 }
                }
            };

            return Ok(template);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting template {TemplateKey}", templateKey);
            return StatusCode(500, new { error = "Failed to retrieve template" });
        }
    }

    /// <summary>
    /// Search templates
    /// </summary>
    [HttpPost("templates/search")]
    public async Task<ActionResult<object>> SearchTemplates([FromBody] object searchRequest)
    {
        try
        {
            _logger.LogInformation("Searching templates with request: {SearchRequest}", searchRequest);

            // Mock search results
            var searchResults = new
            {
                results = new[]
                {
                    new {
                        templateKey = "sql_generation_advanced",
                        name = "Advanced SQL Generation",
                        category = "SQL Generation",
                        description = "Generates optimized SQL queries for complex data analysis",
                        successRate = 0.98,
                        usage = 2847,
                        lastUsed = DateTime.UtcNow.AddHours(-2)
                    },
                    new {
                        templateKey = "data_analysis_comprehensive",
                        name = "Comprehensive Data Analysis",
                        category = "Data Analysis",
                        description = "Performs detailed analysis of datasets with insights",
                        successRate = 0.96,
                        usage = 2134,
                        lastUsed = DateTime.UtcNow.AddHours(-4)
                    },
                    new {
                        templateKey = "report_generation_standard",
                        name = "Standard Report Generation",
                        category = "Report Generation",
                        description = "Creates standard business reports from data",
                        successRate = 0.94,
                        usage = 1876,
                        lastUsed = DateTime.UtcNow.AddHours(-6)
                    }
                },
                totalCount = 3,
                page = 1,
                pageSize = 10
            };

            return Ok(searchResults);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching templates");
            return StatusCode(500, new { error = "Failed to search templates" });
        }
    }

    #endregion

    #region Helper Methods

    private string GetCurrentUserId()
    {
        return User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "anonymous";
    }

    private bool IsAdmin()
    {
        return User.IsInRole("Admin") || User.HasClaim("role", "Admin");
    }

    #endregion
}
