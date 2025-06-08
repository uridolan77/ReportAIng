using NUnit.Framework;
using Moq;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using FluentAssertions;
using BIReportingCopilot.API.Controllers;
using BIReportingCopilot.Core.Interfaces;
using BIReportingCopilot.Core.Models;
using System.Security.Claims;

namespace BIReportingCopilot.Tests.Unit.Controllers;

[TestFixture]
public class DashboardControllerTests
{
    private Mock<ILogger<UnifiedDashboardController>> _mockLogger;
    private Mock<IQueryService> _mockQueryService;
    private Mock<IUserService> _mockUserService;
    private Mock<IAuditService> _mockAuditService;
    private UnifiedDashboardController _controller;

    [SetUp]
    public void Setup()
    {
        _mockLogger = new Mock<ILogger<UnifiedDashboardController>>();
        _mockQueryService = new Mock<IQueryService>();
        _mockUserService = new Mock<IUserService>();
        _mockAuditService = new Mock<IAuditService>();

        _controller = new UnifiedDashboardController(
            _mockLogger.Object,
            _mockUserService.Object,
            _mockAuditService.Object,
            _mockQueryService.Object);

        // Setup user context
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, "test-user-id"),
            new Claim(ClaimTypes.Name, "testuser")
        };
        var identity = new ClaimsIdentity(claims, "TestAuthType");
        var claimsPrincipal = new ClaimsPrincipal(identity);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = claimsPrincipal
            }
        };
    }

    [Test]
    public async Task GetDashboardOverview_WithValidUser_ReturnsOverview()
    {
        // Arrange
        var userId = "test-user-id";
        var userActivities = new List<UserActivity>
        {
            new UserActivity { Id = "1", UserId = userId, Action = "QUERY_EXECUTED", Timestamp = DateTime.UtcNow.AddHours(-1) },
            new UserActivity { Id = "2", UserId = userId, Action = "QUERY_EXECUTED", Timestamp = DateTime.UtcNow.AddDays(-2) },
            new UserActivity { Id = "3", UserId = userId, Action = "LOGIN", Timestamp = DateTime.UtcNow.AddDays(-5) }
        };

        var queryHistory = new List<QueryHistoryItem>
        {
            new QueryHistoryItem
            {
                Id = "1",
                Question = "Show me sales data",
                Sql = "SELECT * FROM Sales",
                Timestamp = DateTime.UtcNow.AddHours(-1),
                Successful = true,
                ExecutionTimeMs = 150,
                UserId = userId
            }
        };

        var auditLogs = new List<AuditLogEntry>
        {
            new AuditLogEntry
            {
                Id = 1,
                UserId = userId,
                Action = "QUERY_EXECUTED",
                Timestamp = DateTime.UtcNow.AddHours(-1),
                Details = System.Text.Json.JsonSerializer.Serialize(new { ExecutionTimeMs = 150 })
            }
        };

        _mockUserService.Setup(x => x.GetUserActivityAsync(userId, It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(userActivities);

        _mockQueryService.Setup(x => x.GetQueryHistoryAsync(userId, 1, 5))
            .ReturnsAsync(queryHistory);

        _mockAuditService.Setup(x => x.GetAuditLogsAsync(null, It.IsAny<DateTime>(), It.IsAny<DateTime>(), null))
            .ReturnsAsync(auditLogs);

        _mockAuditService.Setup(x => x.GetAuditLogsAsync(userId, It.IsAny<DateTime>(), It.IsAny<DateTime>(), "QUERY_EXECUTED"))
            .ReturnsAsync(auditLogs);

        // Act
        var result = await _controller.GetOverview();

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        okResult!.Value.Should().NotBeNull();

        var overview = okResult.Value as DashboardOverview;
        overview.Should().NotBeNull();
        overview!.UserActivity.Should().NotBeNull();
        overview.RecentQueries.Should().NotBeNull();
        overview.RecentQueries.Should().HaveCount(1);
        overview.SystemMetrics.Should().NotBeNull();
        overview.QuickStats.Should().NotBeNull();
    }

    [Test]
    public async Task GetDashboardOverview_WithException_ReturnsInternalServerError()
    {
        // Arrange
        _mockUserService.Setup(x => x.GetUserActivityAsync(It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _controller.GetOverview();

        // Assert
        result.Result.Should().BeOfType<ObjectResult>();
        var objectResult = result.Result as ObjectResult;
        objectResult!.StatusCode.Should().Be(500);
    }

    [Test]
    public async Task GetUserActivity_WithValidParameters_ReturnsActivity()
    {
        // Arrange
        var userId = "test-user-id";
        var days = 7;
        var userActivities = new List<UserActivity>
        {
            new UserActivity { Id = "1", UserId = userId, Action = "QUERY_EXECUTED", Timestamp = DateTime.UtcNow.AddDays(-1) },
            new UserActivity { Id = "2", UserId = userId, Action = "QUERY_EXECUTED", Timestamp = DateTime.UtcNow.AddDays(-3) },
            new UserActivity { Id = "3", UserId = userId, Action = "LOGIN", Timestamp = DateTime.UtcNow.AddDays(-5) }
        };

        _mockUserService.Setup(x => x.GetUserActivityAsync(userId, It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(userActivities);

        // Act
        var result = await _controller.GetRecentActivity(days);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        okResult!.Value.Should().NotBeNull();

        var activities = okResult.Value as List<ActivityItem>;
        activities.Should().NotBeNull();
        activities!.Should().HaveCount(3);
    }

    [Test]
    public async Task GetAnalytics_WithValidDays_ReturnsAnalytics()
    {
        // Arrange
        var days = 30;

        // Act
        var result = await _controller.GetAnalytics(days);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        okResult!.Value.Should().NotBeNull();

        var analytics = okResult.Value as UsageAnalytics;
        analytics.Should().NotBeNull();
        analytics!.QueryTrends.Should().NotBeNull();
        analytics.PopularTables.Should().NotBeNull();
        analytics.PerformanceMetrics.Should().NotBeNull();
        analytics.ErrorAnalysis.Should().NotBeNull();
    }

    [Test]
    public async Task GetRecommendations_WithValidUser_ReturnsRecommendations()
    {
        // Arrange - No specific setup needed as this uses internal logic

        // Act
        var result = await _controller.GetRecommendations();

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        okResult!.Value.Should().NotBeNull();

        var recommendations = okResult.Value as List<Recommendation>;
        recommendations.Should().NotBeNull();
        recommendations!.Should().NotBeEmpty();
    }

}
