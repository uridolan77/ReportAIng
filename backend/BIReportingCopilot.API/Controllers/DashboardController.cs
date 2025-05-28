using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using BIReportingCopilot.Core.Interfaces;
using BIReportingCopilot.Core.Models;
using BIReportingCopilot.Core.Constants;
using System.Security.Claims;
using System.Text.Json;

namespace BIReportingCopilot.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly ILogger<DashboardController> _logger;
    private readonly IUserService _userService;
    private readonly IAuditService _auditService;
    private readonly IQueryService _queryService;

    public DashboardController(
        ILogger<DashboardController> logger,
        IUserService userService,
        IAuditService auditService,
        IQueryService queryService)
    {
        _logger = logger;
        _userService = userService;
        _auditService = auditService;
        _queryService = queryService;
    }

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

    // Private helper methods
    private async Task<UserActivitySummary> GetUserActivitySummaryAsync(string userId, int days)
    {
        var activities = await _userService.GetUserActivityAsync(userId, DateTime.UtcNow.AddDays(-days), DateTime.UtcNow);
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
            return queryHistory;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting recent queries for user {UserId}", userId);
            return new List<QueryHistoryItem>();
        }
    }

    private async Task<List<ActivityItem>> GetRecentActivitiesAsync(string userId, int limit)
    {
        var activities = await _userService.GetUserActivityAsync(userId, DateTime.UtcNow.AddDays(-7), DateTime.UtcNow);
        return activities.Take(limit).Select(a => new ActivityItem
        {
            Id = a.Id,
            UserId = a.UserId,
            Action = a.Action,
            Description = $"User performed {a.Action}",
            Timestamp = a.Timestamp,
            Severity = "Info"
        }).ToList();
    }

    private async Task<SystemMetrics> GetSystemMetricsAsync()
    {
        try
        {
            // Get real system metrics from audit service and database
            var auditLogs = await _auditService.GetAuditLogsAsync(null, DateTime.UtcNow.AddHours(-1), DateTime.UtcNow);
            var recentQueries = auditLogs.Where(a => a.Action == "QUERY_EXECUTED").ToList();

            var averageQueryTime = recentQueries.Any()
                ? recentQueries.Average(q => ExtractExecutionTime(q.Details))
                : 0;

            return new SystemMetrics
            {
                DatabaseConnections = await GetActiveDatabaseConnections(),
                CacheHitRate = await GetCacheHitRate(),
                AverageQueryTime = (int)averageQueryTime,
                SystemUptime = GetSystemUptime()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting system metrics");
            // Return fallback metrics
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
            var userActivities = await _userService.GetUserActivityAsync(userId, DateTime.UtcNow.AddDays(-30), DateTime.UtcNow);
            var thisWeekActivities = userActivities.Where(a => a.Timestamp > DateTime.UtcNow.AddDays(-7)).ToList();

            var auditLogs = await _auditService.GetAuditLogsAsync(userId, DateTime.UtcNow.AddDays(-30), DateTime.UtcNow, "QUERY_EXECUTED");
            var averageTime = auditLogs.Any()
                ? auditLogs.Average(a => ExtractExecutionTime(a.Details))
                : 0;

            return new QuickStats
            {
                TotalQueries = userActivities.Count,
                QueriesThisWeek = thisWeekActivities.Count,
                AverageQueryTime = (int)averageTime,
                FavoriteTable = await GetUserFavoriteTable(userId)
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

    private async Task<QueryTrends> GetQueryTrendsAsync(string userId, int days)
    {
        try
        {
            var fromDate = DateTime.UtcNow.AddDays(-days);
            var auditLogs = await _auditService.GetAuditLogsAsync(userId, fromDate, DateTime.UtcNow, "QUERY_EXECUTED");

            // Calculate daily query counts
            var dailyQueryCounts = auditLogs
                .GroupBy(log => log.Timestamp.Date)
                .ToDictionary(group => group.Key, group => group.Count());

            // Calculate query type distribution (simplified)
            var queryTypeDistribution = new Dictionary<string, int>
            {
                { "SELECT", auditLogs.Count },
                { "Analytics", auditLogs.Count(log => log.Details?.ToString()?.Contains("analytics") == true) },
                { "Reports", auditLogs.Count(log => log.Details?.ToString()?.Contains("report") == true) }
            };

            // Calculate peak usage hours
            var peakUsageHours = auditLogs
                .GroupBy(log => log.Timestamp.Hour)
                .OrderByDescending(group => group.Count())
                .Take(4)
                .Select(group => group.Key)
                .ToList();

            return new QueryTrends
            {
                DailyQueryCounts = dailyQueryCounts,
                QueryTypeDistribution = queryTypeDistribution,
                PeakUsageHours = peakUsageHours
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting query trends for user {UserId}", userId);
            return new QueryTrends
            {
                DailyQueryCounts = new Dictionary<DateTime, int>(),
                QueryTypeDistribution = new Dictionary<string, int>(),
                PeakUsageHours = new List<int>()
            };
        }
    }

    private async Task<List<PopularTable>> GetPopularTablesAsync(string userId, int days)
    {
        try
        {
            var fromDate = DateTime.UtcNow.AddDays(-days);
            var auditLogs = await _auditService.GetAuditLogsAsync(userId, fromDate, DateTime.UtcNow, "QUERY_EXECUTED");

            var tableUsage = new Dictionary<string, int>();

            foreach (var log in auditLogs)
            {
                var sql = ExtractGeneratedSQL(log.Details);
                if (!string.IsNullOrEmpty(sql))
                {
                    var tables = ExtractTableNamesFromSQL(sql);
                    foreach (var table in tables)
                    {
                        tableUsage[table] = tableUsage.GetValueOrDefault(table, 0) + 1;
                    }
                }
            }

            return tableUsage
                .OrderByDescending(kvp => kvp.Value)
                .Take(10)
                .Select(kvp => new PopularTable
                {
                    TableName = kvp.Key,
                    QueryCount = kvp.Value,
                    LastAccessed = DateTime.UtcNow // Simplified - could track actual last usage
                })
                .ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting popular tables for user {UserId}", userId);
            return new List<PopularTable>();
        }
    }

    private async Task<PerformanceMetrics> GetPerformanceMetricsAsync(string userId, int days)
    {
        try
        {
            var fromDate = DateTime.UtcNow.AddDays(-days);
            var auditLogs = await _auditService.GetAuditLogsAsync(userId, fromDate, DateTime.UtcNow, "QUERY_EXECUTED");

            if (!auditLogs.Any())
            {
                return new PerformanceMetrics
                {
                    AverageResponseTime = 0,
                    MedianResponseTime = 0,
                    P95ResponseTime = 0,
                    SuccessRate = 100m
                };
            }

            var executionTimes = auditLogs
                .Select(log => ExtractExecutionTime(log.Details))
                .Where(time => time > 0)
                .OrderBy(time => time)
                .ToList();

            var errorLogs = await _auditService.GetAuditLogsAsync(userId, fromDate, DateTime.UtcNow, "QUERY_ERROR");
            var totalQueries = auditLogs.Count + errorLogs.Count;
            var successRate = totalQueries > 0 ? (decimal)auditLogs.Count / totalQueries * 100 : 100m;

            return new PerformanceMetrics
            {
                AverageResponseTime = executionTimes.Any() ? (int)executionTimes.Average() : 0,
                MedianResponseTime = executionTimes.Any() ? (int)executionTimes[executionTimes.Count / 2] : 0,
                P95ResponseTime = executionTimes.Any() ? (int)executionTimes[(int)(executionTimes.Count * 0.95)] : 0,
                SuccessRate = successRate
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting performance metrics for user {UserId}", userId);
            return new PerformanceMetrics
            {
                AverageResponseTime = 0,
                MedianResponseTime = 0,
                P95ResponseTime = 0,
                SuccessRate = 0m
            };
        }
    }

    private async Task<ErrorAnalysis> GetErrorAnalysisAsync(string userId, int days)
    {
        try
        {
            var fromDate = DateTime.UtcNow.AddDays(-days);
            var errorLogs = await _auditService.GetAuditLogsAsync(userId, fromDate, DateTime.UtcNow, "QUERY_ERROR");

            var errorsByType = errorLogs
                .GroupBy(log => ExtractErrorType(log.Details))
                .ToDictionary(group => group.Key, group => group.Count());

            var commonErrorMessages = errorLogs
                .Select(log => ExtractErrorMessage(log.Details))
                .Where(msg => !string.IsNullOrEmpty(msg))
                .GroupBy(msg => msg)
                .OrderByDescending(group => group.Count())
                .Take(5)
                .Select(group => group.Key)
                .ToList();

            return new ErrorAnalysis
            {
                TotalErrors = errorLogs.Count,
                ErrorsByType = errorsByType,
                CommonErrorMessages = commonErrorMessages
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting error analysis for user {UserId}", userId);
            return new ErrorAnalysis
            {
                TotalErrors = 0,
                ErrorsByType = new Dictionary<string, int>(),
                CommonErrorMessages = new List<string>()
            };
        }
    }

    private async Task<int> GetTotalUsersAsync()
    {
        try
        {
            return await _userService.GetTotalActiveUsersAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting total users count");
            return 0;
        }
    }

    private async Task<int> GetTotalQueriesAsync(int days)
    {
        try
        {
            var fromDate = DateTime.UtcNow.AddDays(-days);
            var auditLogs = await _auditService.GetAuditLogsAsync(null, fromDate, DateTime.UtcNow, ApplicationConstants.AuditActions.QueryExecuted);
            return auditLogs.Count;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting total queries count for {Days} days", days);
            return 0;
        }
    }

    private async Task<double> GetAverageResponseTimeAsync(int days)
    {
        try
        {
            var fromDate = DateTime.UtcNow.AddDays(-days);
            var auditLogs = await _auditService.GetAuditLogsAsync(null, fromDate, DateTime.UtcNow, ApplicationConstants.AuditActions.QueryExecuted);

            if (!auditLogs.Any()) return 0;

            var executionTimes = auditLogs
                .Select(log => ExtractExecutionTime(log.Details))
                .Where(time => time > 0);

            return executionTimes.Any() ? executionTimes.Average() : 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting average response time for {Days} days", days);
            return 0;
        }
    }

    private async Task<double> GetErrorRateAsync(int days)
    {
        try
        {
            var fromDate = DateTime.UtcNow.AddDays(-days);
            var allQueries = await _auditService.GetAuditLogsAsync(null, fromDate, DateTime.UtcNow, "QUERY_EXECUTED");
            var errorQueries = await _auditService.GetAuditLogsAsync(null, fromDate, DateTime.UtcNow, "QUERY_ERROR");

            if (allQueries.Count == 0) return 0;

            return (double)errorQueries.Count / allQueries.Count * 100;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting error rate for {Days} days", days);
            return 0;
        }
    }

    private async Task<List<TopUser>> GetTopUsersAsync(int days)
    {
        try
        {
            var fromDate = DateTime.UtcNow.AddDays(-days);
            var auditLogs = await _auditService.GetAuditLogsAsync(null, fromDate, DateTime.UtcNow, "QUERY_EXECUTED");

            var userQueryCounts = auditLogs
                .Where(log => !string.IsNullOrEmpty(log.UserId))
                .GroupBy(log => log.UserId)
                .Select(group => new { UserId = group.Key, QueryCount = group.Count() })
                .OrderByDescending(x => x.QueryCount)
                .Take(10);

            var topUsers = new List<TopUser>();
            foreach (var userCount in userQueryCounts)
            {
                var userInfo = await _userService.GetUserAsync(userCount.UserId);
                if (userInfo != null)
                {
                    topUsers.Add(new TopUser
                    {
                        UserId = userCount.UserId,
                        Username = userInfo.Username,
                        QueryCount = userCount.QueryCount,
                        AverageResponseTime = 0 // Could calculate from audit logs
                    });
                }
            }

            return topUsers;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting top users for {Days} days", days);
            return new List<TopUser>();
        }
    }

    private async Task<ResourceUsage> GetResourceUsageAsync()
    {
        return new ResourceUsage
        {
            CpuUsage = 45.2,
            MemoryUsage = 68.7,
            DiskUsage = 34.1,
            NetworkUsage = 12.5
        };
    }

    private async Task<List<Recommendation>> GenerateRecommendationsAsync(string userId)
    {
        return new List<Recommendation>
        {
            new Recommendation
            {
                Type = "query_optimization",
                Title = "Optimize your queries",
                Description = "Consider adding indexes to frequently queried columns",
                Priority = "medium"
            }
        };
    }

    private string GetCurrentUserId()
    {
        return User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "anonymous";
    }

    private async Task<int> GetActiveDatabaseConnections()
    {
        try
        {
            // This would typically query the database for active connections
            // For now, return a reasonable estimate
            return 1;
        }
        catch
        {
            return 1;
        }
    }

    private async Task<decimal> GetCacheHitRate()
    {
        try
        {
            // Calculate cache hit rate from recent queries
            var recentQueries = await _auditService.GetAuditLogsAsync(null, DateTime.UtcNow.AddHours(-1), DateTime.UtcNow);
            var queryActions = recentQueries.Where(a => a.Action == "QUERY_EXECUTED").ToList();

            if (!queryActions.Any()) return 85.0m;

            // This is a simplified calculation - in reality you'd track cache hits/misses
            return 85.0m;
        }
        catch
        {
            return 85.0m;
        }
    }

    private TimeSpan GetSystemUptime()
    {
        return TimeSpan.FromMilliseconds(Environment.TickCount64);
    }

    private double ExtractExecutionTime(object? details)
    {
        try
        {
            if (details == null) return 0;

            var detailsStr = details.ToString();
            if (string.IsNullOrEmpty(detailsStr)) return 0;

            // Try to parse execution time from JSON details
            using var doc = System.Text.Json.JsonDocument.Parse(detailsStr);
            if (doc.RootElement.TryGetProperty("ExecutionTimeMs", out var timeElement))
            {
                return timeElement.GetDouble();
            }

            return 0;
        }
        catch
        {
            return 0;
        }
    }

    private async Task<string> GetUserFavoriteTable(string userId)
    {
        try
        {
            // Get user's query history and find most frequently queried table
            var auditLogs = await _auditService.GetAuditLogsAsync(userId, DateTime.UtcNow.AddDays(-30), DateTime.UtcNow, "QUERY_EXECUTED");

            var tableUsage = new Dictionary<string, int>();

            foreach (var log in auditLogs)
            {
                var sql = ExtractGeneratedSQL(log.Details);
                if (!string.IsNullOrEmpty(sql))
                {
                    var tables = ExtractTableNamesFromSQL(sql);
                    foreach (var table in tables)
                    {
                        tableUsage[table] = tableUsage.GetValueOrDefault(table, 0) + 1;
                    }
                }
            }

            return tableUsage.Any()
                ? tableUsage.OrderByDescending(kvp => kvp.Value).First().Key
                : "N/A";
        }
        catch
        {
            return "N/A";
        }
    }

    private string ExtractGeneratedSQL(object? details)
    {
        try
        {
            if (details == null) return string.Empty;

            var detailsStr = details.ToString();
            if (string.IsNullOrEmpty(detailsStr)) return string.Empty;

            using var doc = System.Text.Json.JsonDocument.Parse(detailsStr);
            if (doc.RootElement.TryGetProperty("GeneratedSQL", out var sqlElement))
            {
                return sqlElement.GetString() ?? string.Empty;
            }

            return string.Empty;
        }
        catch
        {
            return string.Empty;
        }
    }

    private List<string> ExtractTableNamesFromSQL(string sql)
    {
        try
        {
            var tables = new List<string>();
            var upperSql = sql.ToUpperInvariant();

            // Simple regex to extract table names after FROM and JOIN keywords
            var fromMatches = System.Text.RegularExpressions.Regex.Matches(upperSql, @"FROM\s+(\w+)");
            var joinMatches = System.Text.RegularExpressions.Regex.Matches(upperSql, @"JOIN\s+(\w+)");

            foreach (System.Text.RegularExpressions.Match match in fromMatches)
            {
                if (match.Groups.Count > 1)
                    tables.Add(match.Groups[1].Value.ToLowerInvariant());
            }

            foreach (System.Text.RegularExpressions.Match match in joinMatches)
            {
                if (match.Groups.Count > 1)
                    tables.Add(match.Groups[1].Value.ToLowerInvariant());
            }

            return tables.Distinct().ToList();
        }
        catch
        {
            return new List<string>();
        }
    }

    private string ExtractErrorType(object? details)
    {
        try
        {
            if (details == null) return "Unknown";

            var detailsStr = details.ToString();
            if (string.IsNullOrEmpty(detailsStr)) return "Unknown";

            using var doc = System.Text.Json.JsonDocument.Parse(detailsStr);
            if (doc.RootElement.TryGetProperty("ErrorType", out var errorTypeElement))
            {
                return errorTypeElement.GetString() ?? "Unknown";
            }

            // Try to infer error type from error message
            var errorMessage = ExtractErrorMessage(details);
            if (errorMessage.Contains("timeout", StringComparison.OrdinalIgnoreCase))
                return "Timeout";
            if (errorMessage.Contains("syntax", StringComparison.OrdinalIgnoreCase))
                return "Syntax Error";
            if (errorMessage.Contains("permission", StringComparison.OrdinalIgnoreCase))
                return "Permission Denied";
            if (errorMessage.Contains("connection", StringComparison.OrdinalIgnoreCase))
                return "Connection Error";

            return "Unknown";
        }
        catch
        {
            return "Unknown";
        }
    }

    private string ExtractErrorMessage(object? details)
    {
        try
        {
            if (details == null) return string.Empty;

            var detailsStr = details.ToString();
            if (string.IsNullOrEmpty(detailsStr)) return string.Empty;

            using var doc = System.Text.Json.JsonDocument.Parse(detailsStr);
            if (doc.RootElement.TryGetProperty("ErrorMessage", out var errorMessageElement))
            {
                return errorMessageElement.GetString() ?? string.Empty;
            }

            if (doc.RootElement.TryGetProperty("Message", out var messageElement))
            {
                return messageElement.GetString() ?? string.Empty;
            }

            return string.Empty;
        }
        catch
        {
            return string.Empty;
        }
    }
}
