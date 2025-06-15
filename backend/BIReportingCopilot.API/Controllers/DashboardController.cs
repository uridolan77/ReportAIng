using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MediatR;
using BIReportingCopilot.Core.Interfaces;
using BIReportingCopilot.Core.Interfaces.Data;
using BIReportingCopilot.Core.Interfaces.Query;
using BIReportingCopilot.Core.Interfaces.Services;
using BIReportingCopilot.Core.Models;
using BIReportingCopilot.Core.Commands;
using System.Security.Claims;
using System.Text.Json;

namespace BIReportingCopilot.API.Controllers;

/// <summary>
/// Dashboard Controller - Provides dashboard operations and multi-modal dashboard management
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly ILogger<DashboardController> _logger;
    private readonly IUserService _userService;
    private readonly IAuditService _auditService;
    private readonly IQueryService _queryService;
    private readonly IMediator _mediator;

    public DashboardController(
        ILogger<DashboardController> logger,
        IUserService userService,
        IAuditService auditService,
        IQueryService queryService,
        IMediator mediator)
    {
        _logger = logger;
        _userService = userService;
        _auditService = auditService;
        _queryService = queryService;
        _mediator = mediator;
    }

    #region Basic Dashboard Operations

    /// <summary>
    /// Get dashboard overview with key metrics
    /// </summary>
    /// <returns>Dashboard overview data</returns>
    [HttpGet("overview")]
    public async Task<ActionResult<DashboardOverview>> GetOverview()
    {
        try
        {
            var userId = GetCurrentUserId();
            _logger.LogInformation("Getting dashboard overview for user {UserId}", userId);

            var overview = new DashboardOverview
            {
                UserActivity = await GetUserActivitySummaryAsync(userId, 30),
                RecentQueries = await GetRecentQueriesAsync(userId, 5),
                SystemMetrics = await GetSystemMetricsAsync(),
                QuickStats = await GetQuickStatsAsync(userId)
            };

            return Ok(overview);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting dashboard overview");
            return StatusCode(500, new { error = "An error occurred while loading dashboard" });
        }
    }

    /// <summary>
    /// Get usage analytics for the current user
    /// </summary>
    /// <param name="days">Number of days to analyze (default: 30)</param>
    /// <returns>Usage analytics data</returns>
    [HttpGet("analytics")]
    public async Task<ActionResult<UsageAnalytics>> GetAnalytics([FromQuery] int days = 30)
    {
        try
        {
            var userId = GetCurrentUserId();
            _logger.LogInformation("Getting usage analytics for user {UserId}, last {Days} days", userId, days);

            var analytics = new UsageAnalytics
            {
                QueryTrends = await GetQueryTrendsAsync(userId, days),
                PopularTables = await GetPopularTablesAsync(userId, days),
                PerformanceMetrics = await GetPerformanceMetricsAsync(userId, days),
                ErrorAnalysis = await GetErrorAnalysisAsync(userId, days)
            };

            return Ok(analytics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting usage analytics");
            return StatusCode(500, new { error = "An error occurred while loading analytics" });
        }
    }

    /// <summary>
    /// Get system-wide statistics (admin only)
    /// </summary>
    /// <param name="days">Number of days to analyze (default: 30)</param>
    /// <returns>System statistics</returns>
    [HttpGet("system-stats")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<SystemStatistics>> GetSystemStats([FromQuery] int days = 30)
    {
        try
        {
            _logger.LogInformation("Getting system statistics for last {Days} days", days);

            var stats = new SystemStatistics
            {
                TotalUsers = await GetTotalUsersAsync(),
                TotalQueries = await GetTotalQueriesAsync(days),
                AverageResponseTime = await GetAverageResponseTimeAsync(days),
                ErrorRate = await GetErrorRateAsync(days),
                TopUsers = await GetTopUsersAsync(days),
                ResourceUsage = await GetResourceUsageAsync()
            };

            return Ok(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting system statistics");
            return StatusCode(500, new { error = "An error occurred while loading system statistics" });
        }
    }

    /// <summary>
    /// Get recent activity feed
    /// </summary>
    /// <param name="limit">Number of activities to return (default: 20)</param>
    /// <returns>Recent activity items</returns>
    [HttpGet("activity")]
    public async Task<ActionResult<List<ActivityItem>>> GetRecentActivity([FromQuery] int limit = 20)
    {
        try
        {
            var userId = GetCurrentUserId();
            _logger.LogInformation("Getting recent activity for user {UserId}", userId);

            var activities = await GetRecentActivitiesAsync(userId, limit);
            return Ok(activities);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting recent activity");
            return StatusCode(500, new { error = "An error occurred while loading recent activity" });
        }
    }

    /// <summary>
    /// Get personalized recommendations
    /// </summary>
    /// <returns>Personalized recommendations</returns>
    [HttpGet("recommendations")]
    public async Task<ActionResult<List<Recommendation>>> GetRecommendations()
    {
        try
        {
            var userId = GetCurrentUserId();
            _logger.LogInformation("Getting recommendations for user {UserId}", userId);

            var recommendations = await GenerateRecommendationsAsync(userId);
            return Ok(recommendations);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting recommendations");
            return StatusCode(500, new { error = "An error occurred while loading recommendations" });
        }
    }

    #endregion

    #region Multi-Modal Dashboard Operations

    /// <summary>
    /// Create a new multi-modal dashboard
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateDashboard([FromBody] CreateDashboardRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();
            _logger.LogInformation("🎨 Creating dashboard '{Name}' for user {UserId}", request.Name, userId);

            var command = new CreateDashboardCommand
            {
                Request = request,
                UserId = userId
            };

            var dashboard = await _mediator.Send(command);

            _logger.LogInformation("🎨 Dashboard '{Name}' created with ID {DashboardId} for user {UserId}", 
                dashboard.Name, dashboard.DashboardId, userId);

            return Ok(new
            {
                success = true,
                data = dashboard,
                metadata = new
                {
                    dashboard_id = dashboard.DashboardId,
                    widget_count = dashboard.Widgets.Count,
                    created_at = dashboard.CreatedAt
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error creating dashboard");
            return StatusCode(500, new { success = false, error = "Internal server error creating dashboard" });
        }
    }

    /// <summary>
    /// Generate dashboard from natural language description
    /// </summary>
    [HttpPost("generate")]
    public async Task<IActionResult> GenerateDashboard([FromBody] GenerateDashboardRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();
            _logger.LogInformation("🤖 Generating dashboard from description for user {UserId}: {Description}", 
                userId, request.Description);

            var command = new GenerateDashboardFromDescriptionCommand
            {
                Description = request.Description,
                UserId = userId,
                Schema = request.Schema
            };

            var dashboard = await _mediator.Send(command);

            _logger.LogInformation("🤖 AI-generated dashboard '{Name}' created for user {UserId} with {WidgetCount} widgets", 
                dashboard.Name, userId, dashboard.Widgets.Count);

            return Ok(new
            {
                success = true,
                data = dashboard,
                metadata = new
                {
                    dashboard_id = dashboard.DashboardId,
                    widget_count = dashboard.Widgets.Count,
                    ai_generated = true,
                    description = request.Description
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error generating dashboard from description");
            return StatusCode(500, new { success = false, error = "Internal server error generating dashboard" });
        }
    }

    /// <summary>
    /// Get dashboard by ID
    /// </summary>
    [HttpGet("{dashboardId}")]
    public async Task<IActionResult> GetDashboard(string dashboardId)
    {
        try
        {
            var userId = GetCurrentUserId();
            _logger.LogInformation("📋 Getting dashboard {DashboardId} for user {UserId}", dashboardId, userId);

            var query = new GetDashboardQuery
            {
                DashboardId = dashboardId,
                UserId = userId
            };

            var dashboard = await _mediator.Send(query);

            if (dashboard == null)
            {
                return NotFound(new { success = false, error = "Dashboard not found or access denied" });
            }

            return Ok(new
            {
                success = true,
                data = dashboard,
                metadata = new
                {
                    dashboard_id = dashboard.DashboardId,
                    widget_count = dashboard.Widgets.Count,
                    last_viewed = dashboard.LastViewedAt,
                    is_public = dashboard.IsPublic
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error getting dashboard {DashboardId}", dashboardId);
            return StatusCode(500, new { success = false, error = "Internal server error getting dashboard" });
        }
    }

    /// <summary>
    /// Get user's dashboards
    /// </summary>
    [HttpGet("list")]
    public async Task<IActionResult> GetUserDashboards([FromQuery] DashboardFilter? filter)
    {
        try
        {
            var userId = GetCurrentUserId();
            _logger.LogInformation("📋 Getting dashboards for user {UserId}", userId);

            var query = new GetUserDashboardsQuery
            {
                UserId = userId,
                Filter = filter
            };

            var dashboards = await _mediator.Send(query);

            _logger.LogInformation("📋 Retrieved {Count} dashboards for user {UserId}", dashboards.Count, userId);

            return Ok(new
            {
                success = true,
                data = dashboards,
                metadata = new
                {
                    dashboard_count = dashboards.Count,
                    public_dashboards = dashboards.Count(d => d.IsPublic),
                    template_dashboards = dashboards.Count(d => d.IsTemplate)
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error getting dashboards for user");
            return StatusCode(500, new { success = false, error = "Internal server error getting dashboards" });
        }
    }

    /// <summary>
    /// Update dashboard
    /// </summary>
    [HttpPut("{dashboardId}")]
    public async Task<IActionResult> UpdateDashboard(string dashboardId, [FromBody] UpdateDashboardRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();
            _logger.LogInformation("🔄 Updating dashboard {DashboardId} for user {UserId}", dashboardId, userId);

            var command = new UpdateDashboardCommand
            {
                DashboardId = dashboardId,
                Request = request,
                UserId = userId
            };

            var dashboard = await _mediator.Send(command);

            _logger.LogInformation("🔄 Dashboard {DashboardId} updated successfully", dashboardId);

            return Ok(new
            {
                success = true,
                data = dashboard,
                metadata = new
                {
                    dashboard_id = dashboard.DashboardId,
                    updated_at = dashboard.UpdatedAt
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error updating dashboard {DashboardId}", dashboardId);
            return StatusCode(500, new { success = false, error = "Internal server error updating dashboard" });
        }
    }

    /// <summary>
    /// Delete dashboard
    /// </summary>
    [HttpDelete("{dashboardId}")]
    public async Task<IActionResult> DeleteDashboard(string dashboardId)
    {
        try
        {
            var userId = GetCurrentUserId();
            _logger.LogInformation("🗑️ Deleting dashboard {DashboardId} for user {UserId}", dashboardId, userId);

            var command = new DeleteDashboardCommand
            {
                DashboardId = dashboardId,
                UserId = userId
            };

            var result = await _mediator.Send(command);

            if (result)
            {
                _logger.LogInformation("🗑️ Dashboard {DashboardId} deleted successfully", dashboardId);
                return Ok(new { success = true, message = "Dashboard deleted successfully" });
            }
            else
            {
                return NotFound(new { success = false, error = "Dashboard not found or access denied" });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error deleting dashboard {DashboardId}", dashboardId);
            return StatusCode(500, new { success = false, error = "Internal server error deleting dashboard" });
        }
    }

    /// <summary>
    /// Add widget to dashboard
    /// </summary>
    [HttpPost("{dashboardId}/widgets")]
    public async Task<IActionResult> AddWidget(string dashboardId, [FromBody] CreateWidgetRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();
            _logger.LogInformation("🧩 Adding widget '{Title}' to dashboard {DashboardId}", request.Title, dashboardId);

            var command = new AddWidgetToDashboardCommand
            {
                DashboardId = dashboardId,
                Request = request,
                UserId = userId
            };

            var widget = await _mediator.Send(command);

            _logger.LogInformation("🧩 Widget '{Title}' added to dashboard {DashboardId}", widget.Title, dashboardId);

            return Ok(new
            {
                success = true,
                data = widget,
                metadata = new
                {
                    widget_id = widget.WidgetId,
                    dashboard_id = dashboardId,
                    widget_type = widget.Type
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error adding widget to dashboard {DashboardId}", dashboardId);
            return StatusCode(500, new { success = false, error = "Internal server error adding widget" });
        }
    }

    /// <summary>
    /// Get dashboard templates
    /// </summary>
    [HttpGet("templates")]
    public async Task<IActionResult> GetDashboardTemplates([FromQuery] string? category = null)
    {
        try
        {
            _logger.LogInformation("📋 Getting dashboard templates for category: {Category}", category ?? "all");

            var query = new GetDashboardTemplatesQuery
            {
                Category = category
            };

            var templates = await _mediator.Send(query);

            _logger.LogInformation("📋 Retrieved {Count} dashboard templates", templates.Count);

            return Ok(new
            {
                success = true,
                data = templates,
                metadata = new
                {
                    template_count = templates.Count,
                    category = category ?? "all",
                    categories = templates.Select(t => t.Category).Distinct().ToList()
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error getting dashboard templates");
            return StatusCode(500, new { success = false, error = "Internal server error getting templates" });
        }
    }

    #endregion

    #region Helper Methods

    private string GetCurrentUserId()
    {
        return User.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
               User.FindFirst("sub")?.Value ??
               User.Identity?.Name ??
               "anonymous";
    }

    // Placeholder helper methods - implement based on original DashboardController logic
    private async Task<UserActivitySummary> GetUserActivitySummaryAsync(string userId, int days)
    {
        var allActivities = await _userService.GetUserActivityAsync(userId);
        var activities = allActivities.Cast<UserActivity>().ToList();
        return new UserActivitySummary
        {
            TotalQueries = activities.Count,
            QueriesThisWeek = activities.Count(a => a.Timestamp > DateTime.UtcNow.AddDays(-7)),
            QueriesThisMonth = activities.Count(a => a.Timestamp > DateTime.UtcNow.AddDays(-30)),
            LastActivity = activities.Any() ? activities.Max(a => a.Timestamp) : DateTime.MinValue
        };
    }

    private async Task<List<QueryHistoryItem>> GetRecentQueriesAsync(string userId, int limit)
    {
        try
        {
            var queryHistory = await _queryService.GetQueryHistoryAsync(userId, 1, limit);
            return queryHistory.Select(q => new QueryHistoryItem
            {
                Id = q.Id,
                Question = q.OriginalQuery, // UnifiedQueryHistoryEntity.OriginalQuery -> QueryHistoryItem.Question
                Sql = q.GeneratedSql, // UnifiedQueryHistoryEntity.GeneratedSql -> QueryHistoryItem.Sql
                ExecutionTimeMs = (int)q.ExecutionTimeMs, // Already int in both, just cast long to int
                Timestamp = q.ExecutedAt, // UnifiedQueryHistoryEntity.ExecutedAt -> QueryHistoryItem.Timestamp (DateTime)
                Successful = q.Status == "Success", // UnifiedQueryHistoryEntity.Status -> QueryHistoryItem.Successful (bool)
                Confidence = 1.0, // Default confidence score
                Error = q.ErrorMessage, // UnifiedQueryHistoryEntity.ErrorMessage -> QueryHistoryItem.Error
                UserId = q.UserId, // Both have UserId
                SessionId = q.SessionId // Both have SessionId
            }).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting recent queries for user {UserId}", userId);
            return new List<QueryHistoryItem>();
        }
    }

    private async Task<SystemMetrics> GetSystemMetricsAsync()
    {
        try
        {
            var auditLogs = await _auditService.GetAuditLogsAsync("", DateTime.UtcNow.AddHours(-1), DateTime.UtcNow);
            var recentQueries = auditLogs.OfType<AuditLogEntry>().Where(a => a.Action == "QUERY_EXECUTED").ToList();

            var averageQueryTime = recentQueries.Any()
                ? recentQueries.Average(q => ExtractExecutionTime(q.Details))
                : 0;

            return new SystemMetrics
            {
                DatabaseConnections = 1,
                CacheHitRate = 85.5m,
                AverageQueryTime = (int)averageQueryTime,
                SystemUptime = TimeSpan.FromHours(24)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting system metrics");
            return new SystemMetrics
            {
                DatabaseConnections = 1,
                CacheHitRate = 85.5m,
                AverageQueryTime = 1250,
                SystemUptime = TimeSpan.FromHours(24)
            };
        }
    }

    private async Task<QuickStats> GetQuickStatsAsync(string userId)
    {
        try
        {
            var allUserActivities = await _userService.GetUserActivityAsync(userId);
            var userActivities = allUserActivities.Cast<UserActivity>().ToList();
            return new QuickStats
            {
                TotalQueries = userActivities.Count,
                QueriesThisWeek = userActivities.Count(a => a.Timestamp > DateTime.UtcNow.AddDays(-7)),
                AverageQueryTime = 1250,
                FavoriteTable = "tbl_Daily_actions"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting quick stats for user {UserId}", userId);
            return new QuickStats
            {
                TotalQueries = 0,
                QueriesThisWeek = 0,
                AverageQueryTime = 0,
                FavoriteTable = "N/A"
            };
        }
    }

    // Placeholder methods - implement full logic from original DashboardController
    private async Task<List<ActivityItem>> GetRecentActivitiesAsync(string userId, int limit) => new();
    private async Task<List<Recommendation>> GenerateRecommendationsAsync(string userId) => new();
    private async Task<QueryTrends> GetQueryTrendsAsync(string userId, int days) => new();
    private async Task<List<PopularTable>> GetPopularTablesAsync(string userId, int days) => new();
    private async Task<PerformanceMetrics> GetPerformanceMetricsAsync(string userId, int days) => new();
    private async Task<Core.Models.ErrorAnalysis> GetErrorAnalysisAsync(string userId, int days) => new();
    private async Task<int> GetTotalUsersAsync() => 0;
    private async Task<int> GetTotalQueriesAsync(int days) => 0;
    private async Task<double> GetAverageResponseTimeAsync(int days) => 0;
    private async Task<double> GetErrorRateAsync(int days) => 0;
    private async Task<List<TopUser>> GetTopUsersAsync(int days) => new();
    private async Task<ResourceUsage> GetResourceUsageAsync() => new();

    private double ExtractExecutionTime(object? details) => 1250.0; // Placeholder

    #endregion
}

#region Request Models

public class GenerateDashboardRequest
{
    public string Description { get; set; } = string.Empty;
    public SchemaMetadata? Schema { get; set; }
}

#endregion
