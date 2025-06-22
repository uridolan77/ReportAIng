using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using BIReportingCopilot.Core.Interfaces.Analytics;
using BIReportingCopilot.Core.Models.Analytics;
using BIReportingCopilot.Infrastructure.Extensions;
using InterfaceTemplatePerformanceMetrics = BIReportingCopilot.Core.Interfaces.Analytics.TemplatePerformanceMetrics;

namespace BIReportingCopilot.API.Controllers;

/// <summary>
/// Controller for template analytics and performance monitoring
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TemplateAnalyticsController : ControllerBase
{
    private readonly ITemplatePerformanceService _performanceService;
    private readonly IABTestingService _abTestingService;
    private readonly ITemplateManagementService _managementService;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<TemplateAnalyticsController> _logger;

    public TemplateAnalyticsController(
        ITemplatePerformanceService performanceService,
        IABTestingService abTestingService,
        ITemplateManagementService managementService,
        IServiceProvider serviceProvider,
        ILogger<TemplateAnalyticsController> logger)
    {
        _performanceService = performanceService;
        _abTestingService = abTestingService;
        _managementService = managementService;
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    /// <summary>
    /// Get performance dashboard data
    /// </summary>
    [HttpGet("dashboard")]
    public async Task<ActionResult<PerformanceDashboardData>> GetDashboard(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] string? intentType = null)
    {
        try
        {
            var dashboard = await _performanceService.GetDashboardDataAsync();
            return Ok(dashboard);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting performance dashboard");
            return StatusCode(500, "Error retrieving dashboard data");
        }
    }

    /// <summary>
    /// Get comprehensive analytics dashboard with trends and insights
    /// </summary>
    [HttpGet("dashboard/comprehensive")]
    public async Task<ActionResult<ComprehensiveAnalyticsDashboard>> GetComprehensiveDashboard(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] string? intentType = null)
    {
        try
        {
            var start = startDate ?? DateTime.UtcNow.AddDays(-30);
            var end = endDate ?? DateTime.UtcNow;

            var performanceDashboard = await _performanceService.GetDashboardDataAsync();
            var abTestDashboard = await _abTestingService.GetTestDashboardAsync();
            var managementDashboard = await _managementService.GetDashboardAsync();

            // Get additional analytics
            var performanceTrends = await GetPerformanceTrendsData(start, end, intentType);
            var usageInsights = await GetUsageInsights(start, end);
            var qualityMetrics = await GetQualityMetricsData();
            var alerts = await _performanceService.GetPerformanceAlertsAsync();

            var comprehensiveDashboard = new ComprehensiveAnalyticsDashboard
            {
                PerformanceOverview = performanceDashboard,
                ABTestingOverview = abTestDashboard,
                ManagementOverview = managementDashboard,
                PerformanceTrends = performanceTrends,
                UsageInsights = usageInsights,
                QualityMetrics = qualityMetrics,
                ActiveAlerts = alerts?.Select(a => new BIReportingCopilot.Core.Models.Analytics.PerformanceAlert
                {
                    AlertId = a.TemplateKey, // Use TemplateKey as AlertId
                    AlertType = a.AlertType,
                    Severity = (BIReportingCopilot.Core.Models.Analytics.AlertSeverity)a.Severity,
                    Title = a.AlertType,
                    Description = a.Message,
                    MetricName = "SuccessRate",
                    CurrentValue = 0, // Would need to get actual value
                    ThresholdValue = 0.7m, // Default threshold
                    TriggeredAt = a.CreatedDate,
                    IsResolved = a.IsAcknowledged,
                    ResolvedAt = null
                }).ToList() ?? new(),
                GeneratedDate = DateTime.UtcNow,
                DateRange = new DateRange { StartDate = start, EndDate = end }
            };

            return Ok(comprehensiveDashboard);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting comprehensive analytics dashboard");
            return StatusCode(500, "Error retrieving comprehensive dashboard data");
        }
    }

    /// <summary>
    /// Get performance trends over time
    /// </summary>
    [HttpGet("trends/performance")]
    public async Task<ActionResult<PerformanceTrendsData>> GetPerformanceTrends(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] string? intentType = null,
        [FromQuery] string? granularity = "daily")
    {
        try
        {
            var start = startDate ?? DateTime.UtcNow.AddDays(-30);
            var end = endDate ?? DateTime.UtcNow;

            var trends = await GetPerformanceTrendsData(start, end, intentType, granularity);
            return Ok(trends);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting performance trends");
            return StatusCode(500, "Error retrieving performance trends");
        }
    }

    /// <summary>
    /// Get usage analytics and insights
    /// </summary>
    [HttpGet("insights/usage")]
    public async Task<ActionResult<UsageInsightsData>> GetUsageInsights(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] string? intentType = null)
    {
        try
        {
            var start = startDate ?? DateTime.UtcNow.AddDays(-30);
            var end = endDate ?? DateTime.UtcNow;

            var insights = await GetUsageInsights(start, end, intentType);
            return Ok(insights);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting usage insights");
            return StatusCode(500, "Error retrieving usage insights");
        }
    }

    /// <summary>
    /// Get quality metrics and analysis
    /// </summary>
    [HttpGet("metrics/quality")]
    public async Task<ActionResult<QualityMetricsData>> GetQualityMetrics(
        [FromQuery] string? intentType = null)
    {
        try
        {
            var metrics = await GetQualityMetricsData(intentType);
            return Ok(metrics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting quality metrics");
            return StatusCode(500, "Error retrieving quality metrics");
        }
    }

    /// <summary>
    /// Get template performance metrics
    /// </summary>
    [HttpGet("performance/{templateKey}")]
    public async Task<ActionResult<InterfaceTemplatePerformanceMetrics>> GetTemplatePerformance(string templateKey)
    {
        try
        {
            var performance = await _performanceService.GetTemplatePerformanceAsync(templateKey);
            if (performance == null)
            {
                return NotFound($"Template not found: {templateKey}");
            }
            return Ok(performance);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting template performance for {TemplateKey}", templateKey);
            return StatusCode(500, "Error retrieving template performance");
        }
    }

    /// <summary>
    /// Get top performing templates
    /// </summary>
    [HttpGet("performance/top")]
    public async Task<ActionResult<List<InterfaceTemplatePerformanceMetrics>>> GetTopPerformingTemplates(
        [FromQuery] string? intentType = null,
        [FromQuery] int count = 10)
    {
        try
        {
            var topPerformers = await _performanceService.GetTopPerformingTemplatesAsync(intentType, count);
            return Ok(topPerformers);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting top performing templates");
            return StatusCode(500, "Error retrieving top performing templates");
        }
    }

    /// <summary>
    /// Get underperforming templates
    /// </summary>
    [HttpGet("performance/underperforming")]
    public async Task<ActionResult<List<InterfaceTemplatePerformanceMetrics>>> GetUnderperformingTemplates(
        [FromQuery] decimal threshold = 0.7m,
        [FromQuery] int minUsageCount = 10)
    {
        try
        {
            var underperforming = await _performanceService.GetUnderperformingTemplatesAsync(threshold, minUsageCount);
            return Ok(underperforming);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting underperforming templates");
            return StatusCode(500, "Error retrieving underperforming templates");
        }
    }

    /// <summary>
    /// Get performance alerts
    /// </summary>
    [HttpGet("alerts")]
    public async Task<ActionResult<List<PerformanceAlert>>> GetPerformanceAlerts()
    {
        try
        {
            var alerts = await _performanceService.GetPerformanceAlertsAsync();
            return Ok(alerts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting performance alerts");
            return StatusCode(500, "Error retrieving performance alerts");
        }
    }

    /// <summary>
    /// Get template performance trends
    /// </summary>
    [HttpGet("performance/{templateKey}/trends")]
    public async Task<ActionResult<TemplatePerformanceTrends>> GetPerformanceTrends(
        string templateKey,
        [FromQuery] int days = 30)
    {
        try
        {
            var timeWindow = TimeSpan.FromDays(days);
            var trends = await _performanceService.GetPerformanceTrendsAsync(templateKey, timeWindow);
            return Ok(trends);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting performance trends for {TemplateKey}", templateKey);
            return StatusCode(500, "Error retrieving performance trends");
        }
    }

    /// <summary>
    /// Compare template performance
    /// </summary>
    [HttpGet("performance/compare")]
    public async Task<ActionResult<TemplatePerformanceComparison>> CompareTemplatePerformance(
        [FromQuery] string templateKey1,
        [FromQuery] string templateKey2)
    {
        try
        {
            if (string.IsNullOrEmpty(templateKey1) || string.IsNullOrEmpty(templateKey2))
            {
                return BadRequest("Both template keys are required");
            }

            var comparison = await _performanceService.CompareTemplatePerformanceAsync(templateKey1, templateKey2);
            return Ok(comparison);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error comparing template performance");
            return StatusCode(500, "Error comparing template performance");
        }
    }

    /// <summary>
    /// Get A/B testing dashboard
    /// </summary>
    [HttpGet("abtests/dashboard")]
    public async Task<ActionResult<ABTestDashboard>> GetABTestDashboard()
    {
        try
        {
            var dashboard = await _abTestingService.GetTestDashboardAsync();
            return Ok(dashboard);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting A/B test dashboard");
            return StatusCode(500, "Error retrieving A/B test dashboard");
        }
    }

    /// <summary>
    /// Get active A/B tests
    /// </summary>
    [HttpGet("abtests/active")]
    public async Task<ActionResult<List<ABTestDetails>>> GetActiveABTests()
    {
        try
        {
            var activeTests = await _abTestingService.GetActiveTestsAsync();
            return Ok(activeTests);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting active A/B tests");
            return StatusCode(500, "Error retrieving active A/B tests");
        }
    }

    /// <summary>
    /// Get A/B test recommendations
    /// </summary>
    [HttpGet("abtests/recommendations")]
    public async Task<ActionResult<List<ABTestRecommendation>>> GetABTestRecommendations()
    {
        try
        {
            var recommendations = await _abTestingService.GetTestRecommendationsAsync();
            return Ok(recommendations);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting A/B test recommendations");
            return StatusCode(500, "Error retrieving A/B test recommendations");
        }
    }

    /// <summary>
    /// Create A/B test
    /// </summary>
    [HttpPost("abtests")]
    public async Task<ActionResult<ABTestResult>> CreateABTest([FromBody] ABTestRequest request)
    {
        try
        {
            if (string.IsNullOrEmpty(request.TestName) || string.IsNullOrEmpty(request.OriginalTemplateKey))
            {
                return BadRequest("Test name and original template key are required");
            }

            var result = await _abTestingService.CreateABTestAsync(request);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating A/B test");
            return StatusCode(500, "Error creating A/B test");
        }
    }

    /// <summary>
    /// Get A/B test details
    /// </summary>
    [HttpGet("abtests/{testId}")]
    public async Task<ActionResult<ABTestDetails>> GetABTest(long testId)
    {
        try
        {
            var test = await _abTestingService.GetABTestAsync(testId);
            if (test == null)
            {
                return NotFound($"A/B test not found: {testId}");
            }
            return Ok(test);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting A/B test {TestId}", testId);
            return StatusCode(500, "Error retrieving A/B test");
        }
    }

    /// <summary>
    /// Analyze A/B test results
    /// </summary>
    [HttpGet("abtests/{testId}/analysis")]
    public async Task<ActionResult<ABTestAnalysis>> AnalyzeABTest(long testId)
    {
        try
        {
            var analysis = await _abTestingService.AnalyzeTestResultsAsync(testId);
            return Ok(analysis);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing A/B test {TestId}", testId);
            return StatusCode(500, "Error analyzing A/B test");
        }
    }

    /// <summary>
    /// Complete A/B test
    /// </summary>
    [HttpPost("abtests/{testId}/complete")]
    public async Task<ActionResult<ImplementationResult>> CompleteABTest(
        long testId,
        [FromQuery] bool implementWinner = true)
    {
        try
        {
            var result = await _abTestingService.CompleteTestAsync(testId, implementWinner);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error completing A/B test {TestId}", testId);
            return StatusCode(500, "Error completing A/B test");
        }
    }

    /// <summary>
    /// Get template management dashboard
    /// </summary>
    [HttpGet("templates/dashboard")]
    public async Task<ActionResult<TemplateManagementDashboard>> GetTemplateManagementDashboard()
    {
        try
        {
            var dashboard = await _managementService.GetDashboardAsync();
            return Ok(dashboard);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting template management dashboard");
            return StatusCode(500, "Error retrieving template management dashboard");
        }
    }

    /// <summary>
    /// Get template with metrics
    /// </summary>
    [HttpGet("templates/{templateKey}")]
    public async Task<ActionResult<TemplateWithMetrics>> GetTemplateWithMetrics(string templateKey)
    {
        try
        {
            var template = await _managementService.GetTemplateAsync(templateKey);
            if (template == null)
            {
                return NotFound($"Template not found: {templateKey}");
            }
            return Ok(template);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting template with metrics for {TemplateKey}", templateKey);
            return StatusCode(500, "Error retrieving template");
        }
    }

    /// <summary>
    /// Get template recommendations
    /// </summary>
    [HttpGet("templates/{templateKey}/recommendations")]
    public async Task<ActionResult<List<TemplateRecommendation>>> GetTemplateRecommendations(string templateKey)
    {
        try
        {
            var recommendations = await _managementService.GetTemplateRecommendationsAsync(templateKey);
            return Ok(recommendations);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting template recommendations for {TemplateKey}", templateKey);
            return StatusCode(500, "Error retrieving template recommendations");
        }
    }

    /// <summary>
    /// Track user feedback for template
    /// </summary>
    [HttpPost("feedback")]
    public async Task<ActionResult> TrackUserFeedback([FromBody] UserFeedbackRequest request)
    {
        try
        {
            if (string.IsNullOrEmpty(request.UsageId) || request.Rating < 0 || request.Rating > 5)
            {
                return BadRequest("Valid usage ID and rating (0-5) are required");
            }

            await _serviceProvider.TrackTemplateUserFeedbackAsync(request.UsageId, request.Rating, request.UserId);
            return Ok(new { message = "Feedback tracked successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error tracking user feedback");
            return StatusCode(500, "Error tracking user feedback");
        }
    }

    /// <summary>
    /// Get real-time analytics data
    /// </summary>
    [HttpGet("realtime")]
    public async Task<ActionResult<RealTimeAnalyticsData>> GetRealTimeAnalytics()
    {
        try
        {
            var realtimeData = await GetRealTimeAnalyticsData();
            return Ok(realtimeData);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting real-time analytics");
            return StatusCode(500, "Error retrieving real-time analytics");
        }
    }

    /// <summary>
    /// Export comprehensive analytics data
    /// </summary>
    [HttpPost("export")]
    public async Task<ActionResult> ExportAnalyticsData([FromBody] AnalyticsExportConfig config)
    {
        try
        {
            var exportResult = await ExportAnalyticsDataAsync(config);

            return File(exportResult.Data, exportResult.ContentType, exportResult.FileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting analytics data");
            return StatusCode(500, "Error exporting analytics data");
        }
    }

    /// <summary>
    /// Export performance data
    /// </summary>
    [HttpGet("performance/export")]
    public async Task<ActionResult> ExportPerformanceData(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate,
        [FromQuery] ExportFormat format = ExportFormat.CSV)
    {
        try
        {
            var data = await _performanceService.ExportPerformanceDataAsync(startDate, endDate, format);

            var contentType = format switch
            {
                ExportFormat.CSV => "text/csv",
                ExportFormat.Excel => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                ExportFormat.JSON => "application/json",
                _ => "application/octet-stream"
            };

            var fileName = $"template_performance_{startDate:yyyyMMdd}_{endDate:yyyyMMdd}.{format.ToString().ToLower()}";

            return File(data, contentType, fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting performance data");
            return StatusCode(500, "Error exporting performance data");
        }
    }

    #region Helper Methods

    private async Task<PerformanceTrendsData> GetPerformanceTrendsData(DateTime startDate, DateTime endDate, string? intentType = null, string granularity = "daily")
    {
        // Get performance data points over time
        var templates = await _performanceService.GetTopPerformingTemplatesAsync(count: 100);

        if (!string.IsNullOrEmpty(intentType))
        {
            templates = templates.Where(t => t.IntentType == intentType).ToList();
        }

        var dataPoints = new List<PerformanceTrendDataPoint>();
        var current = startDate;
        var interval = granularity.ToLower() switch
        {
            "hourly" => TimeSpan.FromHours(1),
            "daily" => TimeSpan.FromDays(1),
            "weekly" => TimeSpan.FromDays(7),
            "monthly" => TimeSpan.FromDays(30),
            _ => TimeSpan.FromDays(1)
        };

        while (current <= endDate)
        {
            // In a real implementation, you'd query historical data for this time period
            var avgSuccessRate = templates.Any() ? templates.Average(t => (double)t.SuccessRate) : 0;
            var avgConfidence = templates.Any() ? templates.Average(t => (double)t.AverageConfidenceScore) : 0;
            var totalUsage = templates.Sum(t => t.TotalUsages);

            dataPoints.Add(new PerformanceTrendDataPoint
            {
                Timestamp = current,
                AverageSuccessRate = (decimal)avgSuccessRate,
                AverageConfidenceScore = (decimal)avgConfidence,
                TotalUsage = totalUsage,
                ActiveTemplates = templates.Count
            });

            current = current.Add(interval);
        }

        return new PerformanceTrendsData
        {
            DataPoints = dataPoints,
            TimeRange = new DateRange { StartDate = startDate, EndDate = endDate },
            Granularity = granularity,
            IntentType = intentType,
            GeneratedDate = DateTime.UtcNow
        };
    }

    private async Task<UsageInsightsData> GetUsageInsights(DateTime startDate, DateTime endDate, string? intentType = null)
    {
        var performanceByIntent = await _performanceService.GetPerformanceByIntentTypeAsync();
        var topPerformers = await _performanceService.GetTopPerformingTemplatesAsync(count: 10);
        var underperforming = await _performanceService.GetUnderperformingTemplatesAsync();

        // Calculate insights
        var totalUsage = topPerformers.Sum(t => t.TotalUsages);
        var avgSuccessRate = topPerformers.Any() ? topPerformers.Average(t => (double)t.SuccessRate) : 0;

        var insights = new List<UsageInsight>();

        // Add usage pattern insights
        if (performanceByIntent.Any())
        {
            var mostUsedIntent = performanceByIntent.OrderByDescending(p => p.Value.TotalUsages).First();
            insights.Add(new UsageInsight
            {
                Type = "MostUsedIntentType",
                Title = "Most Popular Intent Type",
                Description = $"{mostUsedIntent.Key} accounts for the highest usage with {mostUsedIntent.Value.TotalUsages} total uses",
                Impact = "High",
                Recommendation = "Consider optimizing templates for this intent type"
            });
        }

        // Add performance insights
        if (avgSuccessRate < 0.8)
        {
            insights.Add(new UsageInsight
            {
                Type = "LowSuccessRate",
                Title = "Below Average Success Rate",
                Description = $"Overall success rate is {avgSuccessRate:P2}, below the recommended 80%",
                Impact = "High",
                Recommendation = "Review underperforming templates and consider A/B testing improvements"
            });
        }

        return new UsageInsightsData
        {
            TotalUsage = totalUsage,
            AverageSuccessRate = (decimal)avgSuccessRate,
            UsageByIntentType = performanceByIntent.ToDictionary(p => p.Key, p => p.Value.TotalUsages),
            TopPerformingTemplates = topPerformers.Take(5).Select(t => new BIReportingCopilot.Core.Models.Analytics.TemplatePerformanceMetrics
            {
                TemplateKey = t.TemplateKey,
                TemplateName = t.TemplateName,
                IntentType = t.IntentType,
                TotalUsages = t.TotalUsages,
                SuccessfulUsages = t.SuccessfulUsages,
                SuccessRate = t.SuccessRate,
                AverageUserRating = t.AverageUserRating,
                AverageConfidenceScore = t.AverageConfidenceScore,
                AverageProcessingTimeMs = t.AverageProcessingTimeMs,
                LastUsedDate = t.LastUsedDate,
                CreatedDate = DateTime.UtcNow, // Default value
                UpdatedDate = DateTime.UtcNow // Default value
            }).ToList(),
            UnderperformingTemplates = underperforming.Take(5).Select(t => new BIReportingCopilot.Core.Models.Analytics.TemplatePerformanceMetrics
            {
                TemplateKey = t.TemplateKey,
                TemplateName = t.TemplateName,
                IntentType = t.IntentType,
                TotalUsages = t.TotalUsages,
                SuccessfulUsages = t.SuccessfulUsages,
                SuccessRate = t.SuccessRate,
                AverageUserRating = t.AverageUserRating,
                AverageConfidenceScore = t.AverageConfidenceScore,
                AverageProcessingTimeMs = t.AverageProcessingTimeMs,
                LastUsedDate = t.LastUsedDate,
                CreatedDate = DateTime.UtcNow, // Default value
                UpdatedDate = DateTime.UtcNow // Default value
            }).ToList(),
            Insights = insights,
            TimeRange = new DateRange { StartDate = startDate, EndDate = endDate },
            GeneratedDate = DateTime.UtcNow
        };
    }

    private async Task<QualityMetricsData> GetQualityMetricsData(string? intentType = null)
    {
        var allTemplates = await _performanceService.GetTopPerformingTemplatesAsync(count: 1000);

        if (!string.IsNullOrEmpty(intentType))
        {
            allTemplates = allTemplates.Where(t => t.IntentType == intentType).ToList();
        }

        var qualityDistribution = new Dictionary<string, int>
        {
            ["Excellent"] = allTemplates.Count(t => t.SuccessRate >= 0.9m),
            ["Good"] = allTemplates.Count(t => t.SuccessRate >= 0.8m && t.SuccessRate < 0.9m),
            ["Fair"] = allTemplates.Count(t => t.SuccessRate >= 0.7m && t.SuccessRate < 0.8m),
            ["Poor"] = allTemplates.Count(t => t.SuccessRate < 0.7m)
        };

        var avgQualityScore = allTemplates.Any() ? allTemplates.Average(t => (double)t.SuccessRate) * 100 : 0;
        var avgConfidenceScore = allTemplates.Any() ? allTemplates.Average(t => (double)t.AverageConfidenceScore) : 0;

        return new QualityMetricsData
        {
            OverallQualityScore = (decimal)avgQualityScore,
            AverageConfidenceScore = (decimal)avgConfidenceScore,
            QualityDistribution = qualityDistribution,
            TotalTemplatesAnalyzed = allTemplates.Count,
            TemplatesAboveThreshold = allTemplates.Count(t => t.SuccessRate >= 0.8m),
            TemplatesBelowThreshold = allTemplates.Count(t => t.SuccessRate < 0.8m),
            IntentType = intentType,
            GeneratedDate = DateTime.UtcNow
        };
    }

    private async Task<RealTimeAnalyticsData> GetRealTimeAnalyticsData()
    {
        var topTemplates = await _performanceService.GetTopPerformingTemplatesAsync(count: 10);
        var recentTemplates = await _performanceService.GetRecentlyUsedTemplatesAsync(days: 1);

        // Calculate real-time metrics
        var totalUsageToday = recentTemplates.Sum(t => t.TotalUsages);
        var avgSuccessRate = recentTemplates.Any() ? recentTemplates.Average(t => (double)t.SuccessRate) : 0;
        var avgResponseTime = recentTemplates.Any() ? recentTemplates.Average(t => (double)t.AverageProcessingTimeMs) : 0;

        // Simulate recent activities (in a real implementation, this would come from logs)
        var recentActivities = new List<RecentActivity>();
        foreach (var template in recentTemplates.Take(10))
        {
            recentActivities.Add(new RecentActivity
            {
                ActivityType = "TemplateUsage",
                Description = $"Template {template.TemplateKey} used",
                TemplateKey = template.TemplateKey,
                UserId = "system",
                WasSuccessful = template.SuccessRate > 0.8m,
                Timestamp = DateTime.UtcNow.AddMinutes(-Random.Shared.Next(0, 60))
            });
        }

        return new RealTimeAnalyticsData
        {
            ActiveUsers = Random.Shared.Next(10, 50), // Would come from session tracking
            QueriesPerMinute = totalUsageToday / (24 * 60), // Rough estimate
            CurrentSuccessRate = (decimal)avgSuccessRate,
            AverageResponseTime = (decimal)avgResponseTime,
            ErrorsInLastHour = Random.Shared.Next(0, 5), // Would come from error logs
            RecentActivities = recentActivities.OrderByDescending(a => a.Timestamp).ToList(),
            ActiveTemplateUsage = recentTemplates.ToDictionary(t => t.TemplateKey, t => t.TotalUsages),
            LastUpdated = DateTime.UtcNow
        };
    }

    private async Task<AnalyticsExportResult> ExportAnalyticsDataAsync(AnalyticsExportConfig config)
    {
        var startTime = DateTime.UtcNow;

        // Gather data based on configuration
        var performanceData = await _performanceService.ExportPerformanceDataAsync(
            config.DateRange.StartDate,
            config.DateRange.EndDate,
            config.Format);

        var fileName = $"analytics_export_{config.DateRange.StartDate:yyyyMMdd}_{config.DateRange.EndDate:yyyyMMdd}";
        var contentType = config.Format switch
        {
            ExportFormat.CSV => "text/csv",
            ExportFormat.Excel => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            ExportFormat.JSON => "application/json",
            _ => "application/octet-stream"
        };

        fileName += $".{config.Format.ToString().ToLower()}";

        var generationTime = DateTime.UtcNow - startTime;

        return new AnalyticsExportResult
        {
            Data = performanceData,
            FileName = fileName,
            ContentType = contentType,
            FileSizeBytes = performanceData.Length,
            Configuration = config,
            GeneratedDate = DateTime.UtcNow,
            GenerationTime = generationTime
        };
    }

    #endregion
}

/// <summary>
/// Request model for user feedback
/// </summary>
public class UserFeedbackRequest
{
    public string UsageId { get; set; } = string.Empty;
    public decimal Rating { get; set; }
    public string? UserId { get; set; }
    public string? Comments { get; set; }
}
