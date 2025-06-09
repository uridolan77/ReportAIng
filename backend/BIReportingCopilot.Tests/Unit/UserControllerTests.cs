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

namespace BIReportingCopilot.Tests.Unit;

[TestFixture]
public class UserControllerTests
{
    private Mock<ILogger<UserController>> _mockLogger;
    private Mock<IUserService> _mockUserService;
    private UserController _controller;

    [SetUp]
    public void Setup()
    {
        _mockLogger = new Mock<ILogger<UserController>>();
        _mockUserService = new Mock<IUserService>();

        _controller = new UserController(_mockLogger.Object, _mockUserService.Object);

        // Setup user context
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, "test-user-id"),
            new Claim(ClaimTypes.Name, "testuser")
        };
        var identity = new ClaimsIdentity(claims, "TestAuthType");
        var claimsPrincipal = new ClaimsPrincipal(identity);

        var httpContext = new DefaultHttpContext
        {
            User = claimsPrincipal,
            Connection = { RemoteIpAddress = System.Net.IPAddress.Parse("127.0.0.1") }
        };
        httpContext.Request.Headers["User-Agent"] = "Test User Agent";

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };
    }

    [Test]
    public async Task GetProfile_WithValidUser_ReturnsUserProfile()
    {
        // Arrange
        var userId = "test-user-id";
        var userInfo = new UserInfo
        {
            Id = userId,
            Username = "testuser",
            Email = "test@example.com",
            DisplayName = "Test User",
            Roles = new string[] { "User", "Analyst" },
            LastLogin = DateTime.UtcNow.AddDays(-1)
        };

        _mockUserService.Setup(x => x.GetUserAsync(userId))
            .ReturnsAsync(userInfo);

        // Act
        var result = await _controller.GetProfile();

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        okResult!.Value.Should().NotBeNull();

        var profile = okResult.Value as UserInfo;
        profile.Should().NotBeNull();
        profile!.Id.Should().Be(userId);
        profile.Username.Should().Be("testuser");
        profile.Email.Should().Be("test@example.com");
        profile.Roles.Should().BeEquivalentTo(new[] { "User", "Analyst" });
    }

    [Test]
    public async Task GetProfile_WithNonExistentUser_ReturnsNotFound()
    {
        // Arrange
        var userId = "test-user-id";
        _mockUserService.Setup(x => x.GetUserAsync(userId))
            .ReturnsAsync((UserInfo?)null);

        // Act
        var result = await _controller.GetProfile();

        // Assert
        result.Result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Test]
    public async Task UpdateProfile_WithValidData_ReturnsUpdatedProfile()
    {
        // Arrange
        var userId = "test-user-id";
        var updateRequest = new UserInfo
        {
            Id = userId,
            DisplayName = "Updated Test User",
            Email = "updated@example.com",
            Preferences = new UserPreferences
            {
                DefaultChartType = "bar",
                Timezone = "UTC"
            }
        };

        var updatedUserInfo = new UserInfo
        {
            Id = userId,
            Preferences = new UserPreferences
            {
                DefaultChartType = "bar",
                Timezone = "UTC"
            }
        };

        _mockUserService.Setup(x => x.UpdateUserPreferencesAsync(userId, updateRequest.Preferences))
            .ReturnsAsync(updatedUserInfo);

        // Act
        var result = await _controller.UpdateProfile(updateRequest);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        okResult!.Value.Should().NotBeNull();

        var userInfo = okResult.Value as UserInfo;
        userInfo.Should().NotBeNull();
        userInfo!.Preferences.DefaultChartType.Should().Be("bar");
        userInfo.Preferences.Timezone.Should().Be("UTC");
    }

    [Test]
    public async Task GetActivity_WithValidParameters_ReturnsUserActivity()
    {
        // Arrange
        var userId = "test-user-id";
        var days = 7;
        var userActivities = new List<UserActivity>
        {
            new UserActivity
            {
                Id = "1",
                UserId = userId,
                Action = "QUERY_EXECUTED",
                Timestamp = DateTime.UtcNow.AddDays(-1),
                IpAddress = "127.0.0.1"
            },
            new UserActivity
            {
                Id = "2",
                UserId = userId,
                Action = "LOGIN",
                Timestamp = DateTime.UtcNow.AddDays(-2),
                IpAddress = "127.0.0.1"
            }
        };

        _mockUserService.Setup(x => x.GetUserActivityAsync(userId, It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(userActivities);

        // Act
        var result = await _controller.GetActivity(days);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        okResult!.Value.Should().NotBeNull();

        var activity = okResult.Value as UserActivitySummary;
        activity.Should().NotBeNull();
        activity!.TotalQueries.Should().BeGreaterThanOrEqualTo(0);
        activity.QueriesThisWeek.Should().BeGreaterThanOrEqualTo(0);
        activity.QueriesThisMonth.Should().BeGreaterThanOrEqualTo(0);
    }

    [Test]
    public async Task UpdateLastLogin_WithValidUser_ReturnsSuccess()
    {
        // Arrange
        var userId = "test-user-id";

        // Act
        var result = await _controller.UpdateLastLogin();

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().NotBeNull();

        // Verify that LogUserActivityAsync was called
        _mockUserService.Verify(x => x.LogUserActivityAsync(It.Is<UserActivity>(
            activity => activity.UserId == userId &&
                       activity.Action == "LOGIN" &&
                       activity.EntityType == "USER" &&
                       activity.IpAddress == "127.0.0.1" &&
                       activity.UserAgent == "Test User Agent"
        )), Times.Once);
    }

    [Test]
    public async Task UpdateLastLogin_WithException_ReturnsInternalServerError()
    {
        // Arrange
        _mockUserService.Setup(x => x.LogUserActivityAsync(It.IsAny<UserActivity>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _controller.UpdateLastLogin();

        // Assert
        result.Should().BeOfType<ObjectResult>();
        var objectResult = result as ObjectResult;
        objectResult!.StatusCode.Should().Be(500);
    }

    [Test]
    public async Task GetSessions_WithValidUser_ReturnsSessions()
    {
        // Arrange
        var userId = "test-user-id";
        var userActivities = new List<UserActivity>
        {
            new UserActivity
            {
                Id = "session-1",
                UserId = userId,
                Action = "LOGIN",
                Timestamp = DateTime.UtcNow.AddHours(-2),
                IpAddress = "127.0.0.1",
                UserAgent = "Chrome Browser"
            },
            new UserActivity
            {
                Id = "session-2",
                UserId = userId,
                Action = "LOGIN",
                Timestamp = DateTime.UtcNow.AddDays(-1),
                IpAddress = "192.168.1.100",
                UserAgent = "Firefox Browser"
            }
        };

        _mockUserService.Setup(x => x.GetUserActivityAsync(userId, It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(userActivities);

        // Act
        var result = await _controller.GetSessions();

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        okResult!.Value.Should().NotBeNull();

        var sessions = okResult.Value as List<UserSession>;
        sessions.Should().NotBeNull();
        sessions!.Should().HaveCount(2);

        var firstSession = sessions.First();
        firstSession.SessionId.Should().Be("session-1");
        firstSession.UserId.Should().Be(userId);
        firstSession.IpAddress.Should().Be("127.0.0.1");
        firstSession.UserAgent.Should().Be("Chrome Browser");
        firstSession.IsActive.Should().BeTrue(); // Within 24 hours

        var secondSession = sessions.Last();
        secondSession.SessionId.Should().Be("session-2");
        secondSession.IsActive.Should().BeFalse(); // Older than 24 hours
    }

    [Test]
    public async Task GetSessions_WithNoLoginActivities_ReturnsEmptyList()
    {
        // Arrange
        var userId = "test-user-id";
        var userActivities = new List<UserActivity>
        {
            new UserActivity
            {
                Id = "1",
                UserId = userId,
                Action = "QUERY_EXECUTED", // Not a LOGIN action
                Timestamp = DateTime.UtcNow.AddHours(-1)
            }
        };

        _mockUserService.Setup(x => x.GetUserActivityAsync(userId, It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(userActivities);

        // Act
        var result = await _controller.GetSessions();

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        okResult!.Value.Should().NotBeNull();

        var sessions = okResult.Value as List<UserSession>;
        sessions.Should().NotBeNull();
        sessions!.Should().BeEmpty();
    }

    [Test]
    public async Task GetPreferences_WithValidUser_ReturnsPreferences()
    {
        // Arrange
        var userId = "test-user-id";
        var userInfo = new UserInfo
        {
            Id = userId,
            Preferences = new UserPreferences
            {
                DefaultChartType = "bar",
                Timezone = "UTC",
                NotificationSettings = new NotificationSettings
                {
                    Email = true,
                    InApp = false
                }
            }
        };

        _mockUserService.Setup(x => x.GetUserAsync(userId))
            .ReturnsAsync(userInfo);

        // Act
        var result = await _controller.GetPreferences();

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        okResult!.Value.Should().NotBeNull();

        var userPrefs = okResult.Value as UserPreferences;
        userPrefs.Should().NotBeNull();
        userPrefs!.DefaultChartType.Should().Be("bar");
        userPrefs.Timezone.Should().Be("UTC");
    }

    [Test]
    public async Task UpdatePreferences_WithValidData_ReturnsUpdatedPreferences()
    {
        // Arrange
        var userId = "test-user-id";
        var updateRequest = new UserPreferences
        {
            DefaultChartType = "line",
            Timezone = "EST",
            MaxRowsPerQuery = 2000
        };

        var updatedUserInfo = new UserInfo
        {
            Id = userId,
            Preferences = new UserPreferences
            {
                DefaultChartType = "line",
                Timezone = "EST",
                MaxRowsPerQuery = 2000
            }
        };

        _mockUserService.Setup(x => x.UpdateUserPreferencesAsync(userId, updateRequest))
            .ReturnsAsync(updatedUserInfo);

        // Act
        var result = await _controller.UpdatePreferences(updateRequest);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        okResult!.Value.Should().NotBeNull();

        var userInfo = okResult.Value as UserInfo;
        userInfo.Should().NotBeNull();
        userInfo!.Preferences.DefaultChartType.Should().Be("line");
        userInfo.Preferences.Timezone.Should().Be("EST");
        userInfo.Preferences.MaxRowsPerQuery.Should().Be(2000);
    }
}
