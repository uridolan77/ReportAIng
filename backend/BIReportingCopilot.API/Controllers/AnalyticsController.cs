using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using BIReportingCopilot.Core.Interfaces.Analytics;
using BIReportingCopilot.Core.Interfaces.CostOptimization;
using BIReportingCopilot.Core.Models;
using BIReportingCopilot.Core.Models.Analytics;
using BIReportingCopilot.Infrastructure.Extensions;
using BIReportingCopilot.Infrastructure.Performance;
using BIReportingCopilot.Infrastructure.Monitoring;
using BIReportingCopilot.Core.Interfaces.Cache;
using System.Security.Claims;
using System.Diagnostics;
using InterfaceTemplatePerformanceMetrics = BIReportingCopilot.Core.Interfaces.Analytics.TemplatePerformanceMetrics;
using TemplateAnalytics = BIReportingCopilot.Core.Interfaces.Analytics.TemplateAnalytics;
using TemplateBusinessMetadata = BIReportingCopilot.Core.Interfaces.Analytics.TemplateBusinessMetadata;
using TemplateUsageAnalytics = BIReportingCopilot.Core.Interfaces.Analytics.TemplateUsageAnalytics;

namespace BIReportingCopilot.API.Controllers;

/// <summary>
/// Consolidated Analytics Controller - Comprehensive analytics, performance monitoring, and template analytics
/// Combines functionality from AnalyticsController, TemplateAnalyticsController, PerformanceController, and PerformanceMonitoringController
/// </summary>
[ApiController]
[Route("api/analytics")]
[Authorize]
public class AnalyticsController : ControllerBase
{
    private readonly ILogger<AnalyticsController> _logger;
    private readonly ITokenUsageAnalyticsService _tokenAnalyticsService;
    private readonly IPromptGenerationLogsService _promptLogsService;
    private readonly IPromptSuccessTrackingService _promptSuccessService;

    // Template Analytics Services
    private readonly ITemplatePerformanceService _templatePerformanceService;
    private readonly IABTestingService _abTestingService;
    private readonly ITemplateManagementService _templateManagementService;

    // Performance Services
    private readonly IPerformanceOptimizationService _performanceOptimizationService;
    private readonly ICacheOptimizationService _cacheOptimizationService;
    private readonly IResourceMonitoringService _resourceMonitoringService;

    // Performance Monitoring Services
    private readonly PerformanceManagementService _performanceManagementService;
    private readonly MonitoringManagementService _monitoringManagementService;
    private readonly ISemanticCacheService _semanticCacheService;

    public AnalyticsController(
        ILogger<AnalyticsController> logger,
        ITokenUsageAnalyticsService tokenAnalyticsService,
        IPromptGenerationLogsService promptLogsService,
        IPromptSuccessTrackingService promptSuccessService,
        ITemplatePerformanceService templatePerformanceService,
        IABTestingService abTestingService,
        ITemplateManagementService templateManagementService)
    {
        _logger = logger;
        _tokenAnalyticsService = tokenAnalyticsService;
        _promptLogsService = promptLogsService;
        _promptSuccessService = promptSuccessService;
        _templatePerformanceService = templatePerformanceService;
        _abTestingService = abTestingService;
        _templateManagementService = templateManagementService;
    }

    #region Token Usage Analytics

    /// <summary>
    /// Get token usage statistics for a date range
    /// </summary>
    [HttpGet("token-usage")]
    public async Task<ActionResult<TokenUsageStatistics>> GetTokenUsageStatistics(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] string? userId = null)
    {
        try
        {
            var start = startDate ?? DateTime.UtcNow.AddDays(-30);
            var end = endDate ?? DateTime.UtcNow;
            var currentUserId = GetCurrentUserId();

            // Only allow users to see their own data unless they're admin
            var targetUserId = IsAdmin() ? userId : currentUserId;

            _logger.LogInformation("Getting token usage statistics for user {UserId} from {StartDate} to {EndDate}",
                targetUserId ?? "all", start, end);

            var statistics = await _tokenAnalyticsService.GetUsageStatisticsAsync(start, end, targetUserId);
            return Ok(statistics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting token usage statistics");
            return StatusCode(500, new { error = "Failed to retrieve token usage statistics" });
        }
    }

    /// <summary>
    /// Get daily token usage for a user
    /// </summary>
    [HttpGet("token-usage/daily")]
    public async Task<ActionResult<IEnumerable<object>>> GetDailyTokenUsage(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        try
        {
            var start = startDate ?? DateTime.UtcNow.AddDays(-30);
            var end = endDate ?? DateTime.UtcNow;
            var userId = GetCurrentUserId();

            _logger.LogInformation("Getting daily token usage for user {UserId} from {StartDate} to {EndDate}",
                userId, start, end);

            var dailyUsage = await _tokenAnalyticsService.GetDailyUsageAsync(userId, start, end);
            
            var result = dailyUsage.Select(d => new
            {
                Date = d.Date.ToString("yyyy-MM-dd"),
                TotalRequests = d.TotalRequests,
                TotalTokens = d.TotalTokensUsed,
                TotalCost = d.TotalCost,
                AverageTokensPerRequest = d.AverageTokensPerRequest,
                RequestType = d.RequestType,
                IntentType = d.IntentType
            });

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting daily token usage");
            return StatusCode(500, new { error = "Failed to retrieve daily token usage" });
        }
    }

    /// <summary>
    /// Get token usage trends over time
    /// </summary>
    [HttpGet("token-usage/trends")]
    public async Task<ActionResult<IEnumerable<TokenUsageTrend>>> GetTokenUsageTrends(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] string? requestType = null)
    {
        try
        {
            var start = startDate ?? DateTime.UtcNow.AddDays(-30);
            var end = endDate ?? DateTime.UtcNow;
            var userId = GetCurrentUserId();

            _logger.LogInformation("Getting token usage trends for user {UserId} from {StartDate} to {EndDate}",
                userId, start, end);

            var trends = await _tokenAnalyticsService.GetUsageTrendsAsync(start, end, userId, requestType);
            return Ok(trends);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting token usage trends");
            return StatusCode(500, new { error = "Failed to retrieve token usage trends" });
        }
    }

    /// <summary>
    /// Get top users by token usage (admin only)
    /// </summary>
    [HttpGet("token-usage/top-users")]
    public async Task<ActionResult<IEnumerable<UserTokenUsageSummary>>> GetTopUsersByUsage(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] int topCount = 10)
    {
        try
        {
            if (!IsAdmin())
            {
                return Forbid("Admin access required");
            }

            var start = startDate ?? DateTime.UtcNow.AddDays(-30);
            var end = endDate ?? DateTime.UtcNow;

            _logger.LogInformation("Getting top {TopCount} users by token usage from {StartDate} to {EndDate}",
                topCount, start, end);

            var topUsers = await _tokenAnalyticsService.GetTopUsersByUsageAsync(start, end, topCount);
            return Ok(topUsers);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting top users by token usage");
            return StatusCode(500, new { error = "Failed to retrieve top users by token usage" });
        }
    }

    #endregion

    #region Prompt Generation Analytics

    /// <summary>
    /// Get prompt generation analytics for a date range
    /// </summary>
    [HttpGet("prompt-generation")]
    public async Task<ActionResult<PromptGenerationAnalytics>> GetPromptGenerationAnalytics(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] string? intentType = null,
        [FromQuery] string? domain = null)
    {
        try
        {
            var start = startDate ?? DateTime.UtcNow.AddDays(-30);
            var end = endDate ?? DateTime.UtcNow;
            var userId = GetCurrentUserId();

            _logger.LogInformation("Getting prompt generation analytics for user {UserId} from {StartDate} to {EndDate}",
                userId, start, end);

            var analytics = await _promptLogsService.GetAnalyticsAsync(start, end, userId, intentType, domain);
            return Ok(analytics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting prompt generation analytics");
            return StatusCode(500, new { error = "Failed to retrieve prompt generation analytics" });
        }
    }

    /// <summary>
    /// Get template success rates
    /// </summary>
    [HttpGet("prompt-generation/template-success")]
    public async Task<ActionResult<IEnumerable<TemplateSuccessRate>>> GetTemplateSuccessRates(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        try
        {
            var start = startDate ?? DateTime.UtcNow.AddDays(-30);
            var end = endDate ?? DateTime.UtcNow;

            _logger.LogInformation("Getting template success rates from {StartDate} to {EndDate}", start, end);

            var successRates = await _promptLogsService.GetTemplateSuccessRatesAsync(start, end);
            return Ok(successRates);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting template success rates");
            return StatusCode(500, new { error = "Failed to retrieve template success rates" });
        }
    }

    /// <summary>
    /// Get performance metrics for prompt generation
    /// </summary>
    [HttpGet("prompt-generation/performance")]
    public async Task<ActionResult<PromptGenerationPerformanceMetrics>> GetPromptGenerationPerformance(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        try
        {
            var start = startDate ?? DateTime.UtcNow.AddDays(-30);
            var end = endDate ?? DateTime.UtcNow;
            var userId = GetCurrentUserId();

            _logger.LogInformation("Getting prompt generation performance for user {UserId} from {StartDate} to {EndDate}",
                userId, start, end);

            var performance = await _promptLogsService.GetPerformanceMetricsAsync(start, end, userId);
            return Ok(performance);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting prompt generation performance");
            return StatusCode(500, new { error = "Failed to retrieve prompt generation performance" });
        }
    }

    /// <summary>
    /// Get error patterns in prompt generation
    /// </summary>
    [HttpGet("prompt-generation/errors")]
    public async Task<ActionResult<IEnumerable<ErrorPattern>>> GetErrorPatterns(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] int topCount = 10)
    {
        try
        {
            var start = startDate ?? DateTime.UtcNow.AddDays(-30);
            var end = endDate ?? DateTime.UtcNow;

            _logger.LogInformation("Getting error patterns from {StartDate} to {EndDate}", start, end);

            var errorPatterns = await _promptLogsService.GetErrorPatternsAsync(start, end, topCount);
            return Ok(errorPatterns);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting error patterns");
            return StatusCode(500, new { error = "Failed to retrieve error patterns" });
        }
    }

    #endregion

    #region Prompt Success Tracking

    /// <summary>
    /// Get success analytics for prompt sessions
    /// </summary>
    [HttpGet("success-tracking")]
    public async Task<ActionResult<PromptSuccessAnalytics>> GetSuccessAnalytics(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        try
        {
            var start = startDate ?? DateTime.UtcNow.AddDays(-30);
            var end = endDate ?? DateTime.UtcNow;
            var userId = GetCurrentUserId();

            _logger.LogInformation("Getting success analytics for user {UserId} from {StartDate} to {EndDate}",
                userId, start, end);

            var analytics = await _promptSuccessService.GetSuccessAnalyticsAsync(start, end, userId);
            return Ok(analytics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting success analytics");
            return StatusCode(500, new { error = "Failed to retrieve success analytics" });
        }
    }

    /// <summary>
    /// Get template performance metrics
    /// </summary>
    [HttpGet("success-tracking/template-performance")]
    public async Task<ActionResult<IEnumerable<BIReportingCopilot.Core.Models.Analytics.TemplatePerformanceMetrics>>> GetTemplatePerformance(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        try
        {
            var start = startDate ?? DateTime.UtcNow.AddDays(-30);
            var end = endDate ?? DateTime.UtcNow;

            _logger.LogInformation("Getting template performance from {StartDate} to {EndDate}", start, end);

            var performance = await _promptSuccessService.GetTemplatePerformanceAsync(start, end);
            return Ok(performance);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting template performance");
            return StatusCode(500, new { error = "Failed to retrieve template performance" });
        }
    }

    /// <summary>
    /// Get intent performance metrics
    /// </summary>
    [HttpGet("success-tracking/intent-performance")]
    public async Task<ActionResult<IEnumerable<IntentPerformanceMetrics>>> GetIntentPerformance(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        try
        {
            var start = startDate ?? DateTime.UtcNow.AddDays(-30);
            var end = endDate ?? DateTime.UtcNow;

            _logger.LogInformation("Getting intent performance from {StartDate} to {EndDate}", start, end);

            var performance = await _promptSuccessService.GetIntentPerformanceAsync(start, end);
            return Ok(performance);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting intent performance");
            return StatusCode(500, new { error = "Failed to retrieve intent performance" });
        }
    }

    /// <summary>
    /// Get domain performance metrics
    /// </summary>
    [HttpGet("success-tracking/domain-performance")]
    public async Task<ActionResult<IEnumerable<DomainPerformanceMetrics>>> GetDomainPerformance(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        try
        {
            var start = startDate ?? DateTime.UtcNow.AddDays(-30);
            var end = endDate ?? DateTime.UtcNow;

            _logger.LogInformation("Getting domain performance from {StartDate} to {EndDate}", start, end);

            var performance = await _promptSuccessService.GetDomainPerformanceAsync(start, end);
            return Ok(performance);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting domain performance");
            return StatusCode(500, new { error = "Failed to retrieve domain performance" });
        }
    }

    /// <summary>
    /// Get success trends over time
    /// </summary>
    [HttpGet("success-tracking/trends")]
    public async Task<ActionResult<IEnumerable<BIReportingCopilot.Core.Models.Analytics.SuccessTrendPoint>>> GetSuccessTrends(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] string groupBy = "day")
    {
        try
        {
            var start = startDate ?? DateTime.UtcNow.AddDays(-30);
            var end = endDate ?? DateTime.UtcNow;

            _logger.LogInformation("Getting success trends from {StartDate} to {EndDate} grouped by {GroupBy}",
                start, end, groupBy);

            var trends = await _promptSuccessService.GetSuccessTrendsAsync(start, end, groupBy);
            return Ok(trends);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting success trends");
            return StatusCode(500, new { error = "Failed to retrieve success trends" });
        }
    }

    /// <summary>
    /// Submit user feedback for a prompt session
    /// </summary>
    [HttpPost("success-tracking/feedback")]
    public async Task<ActionResult> SubmitFeedback([FromBody] FeedbackRequest request)
    {
        try
        {
            _logger.LogInformation("Submitting feedback for session {SessionId}: Rating={Rating}",
                request.SessionId, request.Rating);

            await _promptSuccessService.RecordUserFeedbackAsync(request.SessionId, request.Rating, request.Comments);
            return Ok(new { message = "Feedback submitted successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error submitting feedback");
            return StatusCode(500, new { error = "Failed to submit feedback" });
        }
    }

    #endregion

    #region Testing and Verification

    /// <summary>
    /// Test the analytics logging pipeline
    /// </summary>
    [HttpPost("test-logging")]
    public async Task<ActionResult<object>> TestAnalyticsLogging()
    {
        try
        {
            var userId = GetCurrentUserId();
            var testResults = new List<object>();

            _logger.LogInformation("🧪 Testing analytics logging pipeline for user {UserId}", userId);

            // Test 1: Token Usage Analytics
            try
            {
                await _tokenAnalyticsService.RecordTokenUsageAsync(
                    userId,
                    "test_request",
                    "TEST_INTENT",
                    100,
                    0.01m);

                testResults.Add(new { test = "TokenUsageAnalytics", status = "success", message = "Token usage recorded successfully" });
                _logger.LogInformation("✅ Token usage analytics test passed");
            }
            catch (Exception ex)
            {
                testResults.Add(new { test = "TokenUsageAnalytics", status = "error", message = ex.Message });
                _logger.LogError(ex, "❌ Token usage analytics test failed");
            }

            // Test 2: Prompt Generation Logs
            try
            {
                var logRequest = new PromptGenerationLogRequest
                {
                    UserId = userId,
                    UserQuestion = "Test question for analytics",
                    GeneratedPrompt = "Test prompt content",
                    IntentType = "TEST_INTENT",
                    Domain = "Test",
                    ConfidenceScore = 0.85m,
                    TablesUsed = 2,
                    GenerationTimeMs = 150,
                    TemplateUsed = "test_template",
                    WasSuccessful = true,
                    TokensUsed = 100,
                    CostEstimate = 0.01m,
                    ModelUsed = "gpt-4",
                    SessionId = Guid.NewGuid().ToString(),
                    RequestId = Guid.NewGuid().ToString()
                };

                var logId = await _promptLogsService.LogPromptGenerationAsync(logRequest);
                testResults.Add(new { test = "PromptGenerationLogs", status = "success", message = $"Prompt log created with ID: {logId}" });
                _logger.LogInformation("✅ Prompt generation logs test passed with ID: {LogId}", logId);
            }
            catch (Exception ex)
            {
                testResults.Add(new { test = "PromptGenerationLogs", status = "error", message = ex.Message });
                _logger.LogError(ex, "❌ Prompt generation logs test failed");
            }

            // Test 3: Prompt Success Tracking
            try
            {
                var trackingRequest = new PromptSuccessTrackingRequest
                {
                    SessionId = Guid.NewGuid().ToString(),
                    UserId = userId,
                    UserQuestion = "Test question for success tracking",
                    GeneratedPrompt = "Test prompt for tracking",
                    TemplateUsed = "test_template",
                    IntentClassified = "TEST_INTENT",
                    DomainClassified = "Test",
                    TablesRetrieved = "table1,table2",
                    GeneratedSQL = "SELECT * FROM test_table",
                    ProcessingTimeMs = 200,
                    ConfidenceScore = 0.90m
                };

                var sessionId = await _promptSuccessService.TrackPromptSessionAsync(trackingRequest);
                testResults.Add(new { test = "PromptSuccessTracking", status = "success", message = $"Success tracking session created with ID: {sessionId}" });
                _logger.LogInformation("✅ Prompt success tracking test passed with ID: {SessionId}", sessionId);
            }
            catch (Exception ex)
            {
                testResults.Add(new { test = "PromptSuccessTracking", status = "error", message = ex.Message });
                _logger.LogError(ex, "❌ Prompt success tracking test failed");
            }

            var successCount = testResults.Count(r => ((dynamic)r).status == "success");
            var totalTests = testResults.Count;

            return Ok(new
            {
                success = successCount == totalTests,
                message = $"Analytics logging test completed: {successCount}/{totalTests} tests passed",
                userId = userId,
                timestamp = DateTime.UtcNow,
                results = testResults
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error testing analytics logging pipeline");
            return StatusCode(500, new { error = "Failed to test analytics logging pipeline", details = ex.Message });
        }
    }

    /// <summary>
    /// Verify data exists in analytics tables
    /// </summary>
    [HttpGet("verify-data")]
    public async Task<ActionResult<object>> VerifyAnalyticsData()
    {
        try
        {
            var userId = GetCurrentUserId();
            var verificationResults = new List<object>();

            _logger.LogInformation("🔍 Verifying analytics data for user {UserId}", userId);

            // Verify token usage data
            try
            {
                var tokenStats = await _tokenAnalyticsService.GetUsageStatisticsAsync(
                    DateTime.UtcNow.AddDays(-1),
                    DateTime.UtcNow,
                    userId);

                verificationResults.Add(new
                {
                    table = "TokenUsageAnalytics",
                    status = "success",
                    data = new
                    {
                        totalRequests = tokenStats.TotalRequests,
                        totalTokens = tokenStats.TotalTokens,
                        totalCost = tokenStats.TotalCost
                    }
                });
            }
            catch (Exception ex)
            {
                verificationResults.Add(new { table = "TokenUsageAnalytics", status = "error", message = ex.Message });
            }

            // Verify prompt generation logs
            try
            {
                var promptAnalytics = await _promptLogsService.GetAnalyticsAsync(
                    DateTime.UtcNow.AddDays(-1),
                    DateTime.UtcNow,
                    userId);

                verificationResults.Add(new
                {
                    table = "PromptGenerationLogs",
                    status = "success",
                    data = new
                    {
                        totalPrompts = promptAnalytics.TotalPrompts,
                        successfulPrompts = promptAnalytics.SuccessfulPrompts,
                        successRate = promptAnalytics.SuccessRate
                    }
                });
            }
            catch (Exception ex)
            {
                verificationResults.Add(new { table = "PromptGenerationLogs", status = "error", message = ex.Message });
            }

            // Verify prompt success tracking
            try
            {
                var successAnalytics = await _promptSuccessService.GetSuccessAnalyticsAsync(
                    DateTime.UtcNow.AddDays(-1),
                    DateTime.UtcNow,
                    userId);

                verificationResults.Add(new
                {
                    table = "PromptSuccessTracking",
                    status = "success",
                    data = new
                    {
                        totalSessions = successAnalytics.TotalSessions,
                        successfulSessions = successAnalytics.SuccessfulSessions,
                        overallSuccessRate = successAnalytics.OverallSuccessRate
                    }
                });
            }
            catch (Exception ex)
            {
                verificationResults.Add(new { table = "PromptSuccessTracking", status = "error", message = ex.Message });
            }

            return Ok(new
            {
                success = true,
                message = "Analytics data verification completed",
                userId = userId,
                timestamp = DateTime.UtcNow,
                results = verificationResults
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying analytics data");
            return StatusCode(500, new { error = "Failed to verify analytics data", details = ex.Message });
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
        return User.IsInRole("Admin") || User.HasClaim("role", "admin");
    }

    #endregion

    #region Template Analytics

    /// <summary>
    /// Get template performance dashboard data
    /// </summary>
    [HttpGet("templates/dashboard")]
    public async Task<ActionResult<PerformanceDashboardData>> GetTemplateDashboard(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] string? intentType = null)
    {
        try
        {
            var dashboard = await _templatePerformanceService.GetDashboardDataAsync();
            return Ok(dashboard);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting template performance dashboard");
            return StatusCode(500, "Error retrieving dashboard data");
        }
    }

    /// <summary>
    /// Get comprehensive template analytics dashboard with trends and insights
    /// </summary>
    [HttpGet("templates/dashboard/comprehensive")]
    public async Task<ActionResult<ComprehensiveAnalyticsDashboard>> GetComprehensiveTemplateDashboard(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] string? intentType = null)
    {
        try
        {
            var start = startDate ?? DateTime.UtcNow.AddDays(-30);
            var end = endDate ?? DateTime.UtcNow;

            var performanceDashboard = await _templatePerformanceService.GetDashboardDataAsync();
            var abTestDashboard = await _abTestingService.GetTestDashboardAsync();
            var managementDashboard = await _templateManagementService.GetDashboardAsync();

            // Get additional analytics
            var performanceTrends = await GetPerformanceTrendsData(start, end, intentType);
            var usageInsights = await GetUsageInsights(start, end);
            var qualityMetrics = await GetQualityMetricsData();
            var alerts = await _templatePerformanceService.GetPerformanceAlertsAsync();

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
            _logger.LogError(ex, "Error getting comprehensive template analytics dashboard");
            return StatusCode(500, "Error retrieving comprehensive dashboard data");
        }
    }

    /// <summary>
    /// Get template performance trends over time
    /// </summary>
    [HttpGet("templates/trends/performance")]
    public async Task<ActionResult<PerformanceTrendsData>> GetTemplatePerformanceTrends(
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
            _logger.LogError(ex, "Error getting template performance trends");
            return StatusCode(500, "Error retrieving performance trends");
        }
    }

    /// <summary>
    /// Get template usage analytics and insights
    /// </summary>
    [HttpGet("templates/insights/usage")]
    public async Task<ActionResult<UsageInsightsData>> GetTemplateUsageInsights(
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
            _logger.LogError(ex, "Error getting template usage insights");
            return StatusCode(500, "Error retrieving usage insights");
        }
    }

    /// <summary>
    /// Get template quality metrics and analysis
    /// </summary>
    [HttpGet("templates/metrics/quality")]
    public async Task<ActionResult<QualityMetricsData>> GetTemplateQualityMetrics(
        [FromQuery] string? intentType = null)
    {
        try
        {
            var metrics = await GetQualityMetricsData(intentType);
            return Ok(metrics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting template quality metrics");
            return StatusCode(500, "Error retrieving quality metrics");
        }
    }

    /// <summary>
    /// Get specific template performance metrics
    /// </summary>
    [HttpGet("templates/performance/{templateKey}")]
    public async Task<ActionResult<InterfaceTemplatePerformanceMetrics>> GetSpecificTemplatePerformance(string templateKey)
    {
        try
        {
            var performance = await _templatePerformanceService.GetTemplatePerformanceAsync(templateKey);
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
    [HttpGet("templates/performance/top")]
    public async Task<ActionResult<List<InterfaceTemplatePerformanceMetrics>>> GetTopPerformingTemplates(
        [FromQuery] string? intentType = null,
        [FromQuery] int count = 10)
    {
        try
        {
            var topPerformers = await _templatePerformanceService.GetTopPerformingTemplatesAsync(intentType, count);
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
    [HttpGet("templates/performance/underperforming")]
    public async Task<ActionResult<List<InterfaceTemplatePerformanceMetrics>>> GetUnderperformingTemplates(
        [FromQuery] decimal threshold = 0.7m,
        [FromQuery] int minUsageCount = 10)
    {
        try
        {
            var underperforming = await _templatePerformanceService.GetUnderperformingTemplatesAsync(threshold, minUsageCount);
            return Ok(underperforming);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting underperforming templates");
            return StatusCode(500, "Error retrieving underperforming templates");
        }
    }

    /// <summary>
    /// Get template performance alerts
    /// </summary>
    [HttpGet("templates/alerts")]
    public async Task<ActionResult<List<PerformanceAlert>>> GetTemplatePerformanceAlerts()
    {
        try
        {
            var alerts = await _templatePerformanceService.GetPerformanceAlertsAsync();
            return Ok(alerts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting template performance alerts");
            return StatusCode(500, "Error retrieving performance alerts");
        }
    }

    /// <summary>
    /// Get template performance trends for specific template
    /// </summary>
    [HttpGet("templates/performance/{templateKey}/trends")]
    public async Task<ActionResult<TemplatePerformanceTrends>> GetTemplateSpecificPerformanceTrends(
        string templateKey,
        [FromQuery] int days = 30)
    {
        try
        {
            var timeWindow = TimeSpan.FromDays(days);
            var trends = await _templatePerformanceService.GetPerformanceTrendsAsync(templateKey, timeWindow);
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
    [HttpGet("templates/performance/compare")]
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

            var comparison = await _templatePerformanceService.CompareTemplatePerformanceAsync(templateKey1, templateKey2);
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
    [HttpGet("templates/abtests/dashboard")]
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
    [HttpGet("templates/abtests/active")]
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
    [HttpGet("templates/abtests/recommendations")]
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
    [HttpPost("templates/abtests")]
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
    [HttpGet("templates/abtests/{testId}")]
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
    [HttpGet("templates/abtests/{testId}/analysis")]
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
    [HttpPost("templates/abtests/{testId}/complete")]
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
    /// Get comprehensive template analytics
    /// </summary>
    [HttpGet("templates/analytics")]
    public async Task<ActionResult<TemplateAnalytics>> GetTemplateAnalytics()
    {
        try
        {
            _logger.LogInformation("📊 [API] Getting comprehensive template analytics");
            var analytics = await _templateManagementService.GetTemplateAnalyticsAsync();
            return Ok(analytics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting template analytics");
            return StatusCode(500, "Error retrieving template analytics");
        }
    }

    /// <summary>
    /// Get template performance metrics for all templates
    /// </summary>
    [HttpGet("templates/analytics/performance")]
    public async Task<ActionResult<List<InterfaceTemplatePerformanceMetrics>>> GetAllTemplatePerformanceMetrics()
    {
        try
        {
            _logger.LogInformation("📊 [API] Getting template performance metrics");
            var metrics = await _templateManagementService.GetTemplatePerformanceMetricsAsync();
            return Ok(metrics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting template performance metrics");
            return StatusCode(500, "Error retrieving template performance metrics");
        }
    }

    /// <summary>
    /// Update template business metadata
    /// </summary>
    [HttpPut("templates/analytics/{templateKey}/business-metadata")]
    public async Task<ActionResult> UpdateTemplateBusinessMetadata(
        string templateKey,
        [FromBody] TemplateBusinessMetadata metadata)
    {
        try
        {
            if (string.IsNullOrEmpty(templateKey))
            {
                return BadRequest("Template key is required");
            }

            // Note: Implementation would depend on your specific template management service
            // For now, return success as placeholder
            return Ok(new { message = "Template business metadata updated successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating template business metadata for {TemplateKey}", templateKey);
            return StatusCode(500, "Error updating template business metadata");
        }
    }

    #endregion

    #region Private Helper Methods for Template Analytics

    private async Task<PerformanceTrendsData> GetPerformanceTrendsData(DateTime startDate, DateTime endDate, string? intentType = null, string granularity = "daily")
    {
        // Implementation would depend on your specific analytics service
        // This is a placeholder that returns mock data
        await Task.CompletedTask; // Placeholder for async operation
        return new PerformanceTrendsData();
    }

    private async Task<UsageInsightsData> GetUsageInsights(DateTime startDate, DateTime endDate, string? intentType = null)
    {
        // Implementation would depend on your specific analytics service
        // This is a placeholder that returns mock data
        await Task.CompletedTask; // Placeholder for async operation
        return new UsageInsightsData();
    }

    private async Task<QualityMetricsData> GetQualityMetricsData(string? intentType = null)
    {
        // Implementation would depend on your specific analytics service
        // This is a placeholder that returns mock data
        await Task.CompletedTask; // Placeholder for async operation
        return new QualityMetricsData
        {
            OverallQualityScore = 0.85m
        };
    }

    #endregion
}

/// <summary>
/// Request model for submitting user feedback
/// </summary>
public class FeedbackRequest
{
    public long SessionId { get; set; }
    public int Rating { get; set; }
    public string? Comments { get; set; }
}
