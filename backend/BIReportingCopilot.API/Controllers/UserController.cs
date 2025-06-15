using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using BIReportingCopilot.Core.Interfaces;
using BIReportingCopilot.Core.Interfaces.Services;
using BIReportingCopilot.Core.Models;
using System.Security.Claims;

namespace BIReportingCopilot.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UserController : ControllerBase
{
    private readonly ILogger<UserController> _logger;
    private readonly IUserService _userService;

    public UserController(ILogger<UserController> logger, IUserService userService)
    {
        _logger = logger;
        _userService = userService;
    }

    /// <summary>
    /// Get current user profile
    /// </summary>
    /// <returns>User profile information</returns>
    [HttpGet("profile")]
    public async Task<ActionResult<UserInfo>> GetProfile()
    {
        try
        {
            var userId = GetCurrentUserId();
            _logger.LogInformation("Getting profile for user {UserId}", userId);

            var profile = await _userService.GetUserAsync(userId);
            if (profile == null)
            {
                return NotFound(new { error = "User profile not found" });
            }

            return Ok(profile);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user profile");
            return StatusCode(500, new { error = "An error occurred while retrieving user profile" });
        }
    }

    /// <summary>
    /// Update user profile
    /// </summary>
    /// <param name="profile">Updated profile information</param>
    /// <returns>Updated user profile</returns>
    [HttpPut("profile")]
    public async Task<ActionResult<UserInfo>> UpdateProfile([FromBody] UserInfo profile)
    {
        try
        {
            var userId = GetCurrentUserId();
            _logger.LogInformation("Updating profile for user {UserId}", userId);

            await _userService.UpdateUserPreferencesAsync(userId, profile.Preferences);
            var updatedProfile = await _userService.GetUserAsync(userId);
            return Ok(updatedProfile);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user profile");
            return StatusCode(500, new { error = "An error occurred while updating user profile" });
        }
    }

    /// <summary>
    /// Get user preferences
    /// </summary>
    /// <returns>User preferences</returns>
    [HttpGet("preferences")]
    public async Task<ActionResult<UserPreferences>> GetPreferences()
    {
        try
        {
            var userId = GetCurrentUserId();
            _logger.LogInformation("Getting preferences for user {UserId}", userId);

            var user = await _userService.GetUserAsync(userId);
            var preferences = user?.Preferences ?? new UserPreferences();
            return Ok(preferences);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user preferences");
            return StatusCode(500, new { error = "An error occurred while retrieving user preferences" });
        }
    }

    /// <summary>
    /// Update user preferences
    /// </summary>
    /// <param name="preferences">Updated preferences</param>
    /// <returns>Updated user preferences</returns>
    [HttpPut("preferences")]
    public async Task<ActionResult<UserPreferences>> UpdatePreferences([FromBody] UserPreferences preferences)
    {
        try
        {
            var userId = GetCurrentUserId();
            _logger.LogInformation("Updating preferences for user {UserId}", userId);

            await _userService.UpdateUserPreferencesAsync(userId, preferences);
            var updatedPreferences = preferences;
            return Ok(updatedPreferences);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user preferences");
            return StatusCode(500, new { error = "An error occurred while updating user preferences" });
        }
    }

    /// <summary>
    /// Get user activity summary
    /// </summary>
    /// <param name="days">Number of days to look back (default: 30)</param>
    /// <returns>User activity summary</returns>
    [HttpGet("activity")]
    public async Task<ActionResult<UserActivitySummary>> GetActivity([FromQuery] int days = 30)
    {
        try
        {
            var userId = GetCurrentUserId();
            _logger.LogInformation("Getting activity summary for user {UserId}, last {Days} days", userId, days);

            var activities = await _userService.GetUserActivityAsync(userId);
            var activityList = activities.Cast<UserActivity>().ToList();
            var activity = new UserActivitySummary
            {
                TotalQueries = activityList.Count,
                QueriesThisWeek = activityList.Count(a => a.Timestamp > DateTime.UtcNow.AddDays(-7)),
                QueriesThisMonth = activityList.Count(a => a.Timestamp > DateTime.UtcNow.AddDays(-30)),
                LastActivity = activityList.Any() ? activityList.Max(a => a.Timestamp) : DateTime.MinValue
            };
            return Ok(activity);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user activity");
            return StatusCode(500, new { error = "An error occurred while retrieving user activity" });
        }
    }

    /// <summary>
    /// Get user permissions
    /// </summary>
    /// <returns>User permissions and roles</returns>
    [HttpGet("permissions")]
    public async Task<ActionResult<UserPermissions>> GetPermissions()
    {
        try
        {
            var userId = GetCurrentUserId();
            _logger.LogInformation("Getting permissions for user {UserId}", userId);

            var permissionsList = await _userService.GetUserPermissionsAsync(userId);
            var permissions = new UserPermissions
            {
                Permissions = permissionsList.ToList(),
                Roles = new List<string>(),
                FeatureAccess = new Dictionary<string, bool>(),
                AllowedDatabases = new List<string>()
            };
            return Ok(permissions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user permissions");
            return StatusCode(500, new { error = "An error occurred while retrieving user permissions" });
        }
    }

    /// <summary>
    /// Update last login timestamp
    /// </summary>
    /// <returns>Success status</returns>
    [HttpPost("login")]
    public async Task<ActionResult> UpdateLastLogin()
    {
        try
        {
            var userId = GetCurrentUserId();
            _logger.LogInformation("Updating last login for user {UserId}", userId);

            // Create a user activity record for login
            var loginActivity = new UserActivity
            {
                Id = Guid.NewGuid().ToString(),
                UserId = userId,
                Action = "LOGIN",
                EntityType = "USER",
                EntityId = userId,
                Details = new Dictionary<string, object>
                {
                    { "Description", "User logged in" },
                    { "LoginTime", DateTime.UtcNow }
                },
                Timestamp = DateTime.UtcNow,
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                UserAgent = HttpContext.Request.Headers["User-Agent"].ToString()
            };

            await _userService.LogUserActivityAsync(userId, "LOGIN");

            return Ok(new { message = "Last login updated successfully", timestamp = DateTime.UtcNow });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating last login");
            return StatusCode(500, new { error = "An error occurred while updating last login" });
        }
    }

    /// <summary>
    /// Get user sessions
    /// </summary>
    /// <returns>Active user sessions</returns>
    [HttpGet("sessions")]
    public async Task<ActionResult<List<UserSession>>> GetSessions()
    {
        try
        {
            var userId = GetCurrentUserId();
            _logger.LogInformation("Getting sessions for user {UserId}", userId);

            // Get recent user activities to simulate sessions
            var userActivities = await _userService.GetUserActivityAsync(userId);
            var loginActivities = userActivities.Cast<UserActivity>().ToList();
            var loginSessions = loginActivities
                .Where(a => a.Action == "LOGIN")
                .OrderByDescending(a => a.Timestamp)
                .Take(10)
                .Select(a => new UserSession
                {
                    SessionId = a.Id,
                    UserId = userId,
                    StartTime = a.Timestamp,
                    LastActivity = a.Timestamp,
                    IpAddress = a.IpAddress ?? "Unknown",
                    UserAgent = a.UserAgent ?? "Unknown",
                    IsActive = a.Timestamp > DateTime.UtcNow.AddHours(-24)
                })
                .ToList();

            return Ok(loginSessions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user sessions");
            return StatusCode(500, new { error = "An error occurred while retrieving user sessions" });
        }
    }

    private string GetCurrentUserId()
    {
        return User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "anonymous";
    }
}
