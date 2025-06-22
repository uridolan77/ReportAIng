using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using BIReportingCopilot.Core.Interfaces.Analytics;
using BIReportingCopilot.Core.Models.Analytics;
using BIReportingCopilot.Infrastructure.Extensions;

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
    public async Task<ActionResult<PerformanceDashboardData>> GetDashboard()
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
    /// Get template performance metrics
    /// </summary>
    [HttpGet("performance/{templateKey}")]
    public async Task<ActionResult<TemplatePerformanceMetrics>> GetTemplatePerformance(string templateKey)
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
    public async Task<ActionResult<List<TemplatePerformanceMetrics>>> GetTopPerformingTemplates(
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
    public async Task<ActionResult<List<TemplatePerformanceMetrics>>> GetUnderperformingTemplates(
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
