using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using BIReportingCopilot.Core.Interfaces;
using BIReportingCopilot.Core.Models;

namespace BIReportingCopilot.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly ILogger<AuthController> _logger;
    private readonly IAuthenticationService _authService;

    public AuthController(ILogger<AuthController> logger, IAuthenticationService authService)
    {
        _logger = logger;
        _authService = authService;
    }

    /// <summary>
    /// Authenticate user with username and password
    /// </summary>
    /// <param name="request">Login credentials</param>
    /// <returns>Authentication result with JWT token</returns>
    [HttpPost("login")]
    public async Task<ActionResult<AuthenticationResult>> Login([FromBody] LoginRequest request)
    {
        try
        {
            _logger.LogInformation("Login attempt for user: {Username}", request.Username);

            var result = await _authService.AuthenticateAsync(request);

            if (result.Success)
            {
                _logger.LogInformation("Successful login for user: {Username}", request.Username);

                // Debug: Log the token details
                if (!string.IsNullOrEmpty(result.AccessToken))
                {
                    try
                    {
                        var tokenParts = result.AccessToken.Split('.');
                        if (tokenParts.Length == 3)
                        {
                            var payload = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(
                                System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(tokenParts[1] + "==")));
                            _logger.LogInformation("Generated token - Issuer: {Issuer}, Audience: {Audience}",
                                payload?.GetValueOrDefault("iss"), payload?.GetValueOrDefault("aud"));
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to decode token for debugging");
                    }
                }

                return Ok(result);
            }
            else
            {
                _logger.LogWarning("Failed login attempt for user: {Username}", request.Username);
                return Unauthorized(new { error = "Invalid username or password" });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login for user: {Username}", request.Username);
            return StatusCode(500, new { error = "An error occurred during authentication" });
        }
    }

    /// <summary>
    /// Register a new user account (placeholder - not implemented in interface)
    /// </summary>
    /// <param name="request">Registration details</param>
    /// <returns>Registration result</returns>
    [HttpPost("register")]
    public ActionResult<AuthenticationResult> Register([FromBody] User request)
    {
        try
        {
            _logger.LogInformation("Registration attempt for user: {Username}", request.Username);

            // Registration not implemented in current interface
            return BadRequest(new { error = "Registration not yet implemented" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during registration for user: {Username}", request.Username);
            return StatusCode(500, new { error = "An error occurred during registration" });
        }
    }

    /// <summary>
    /// Refresh JWT token
    /// </summary>
    /// <param name="request">Refresh token request</param>
    /// <returns>New JWT token</returns>
    [HttpPost("refresh")]
    public async Task<ActionResult<AuthenticationResult>> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        try
        {
            _logger.LogInformation("Token refresh attempt");

            var result = await _authService.RefreshTokenAsync(request.RefreshToken);

            if (result.Success)
            {
                return Ok(result);
            }
            else
            {
                return Unauthorized(new { error = "Invalid refresh token" });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during token refresh");
            return StatusCode(500, new { error = "An error occurred during token refresh" });
        }
    }

    /// <summary>
    /// Logout user and invalidate tokens
    /// </summary>
    /// <param name="request">Logout request</param>
    /// <returns>Success status</returns>
    [HttpPost("logout")]
    [Authorize]
    public async Task<ActionResult> Logout([FromBody] RefreshTokenRequest request)
    {
        try
        {
            var userId = User.FindFirst("sub")?.Value ?? User.FindFirst("user_id")?.Value;
            _logger.LogInformation("Logout attempt for user: {UserId}", userId);

            await _authService.RevokeTokenAsync(request.RefreshToken);

            return Ok(new { message = "Logged out successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during logout");
            return StatusCode(500, new { error = "An error occurred during logout" });
        }
    }

    /// <summary>
    /// Validate JWT token
    /// </summary>
    /// <returns>Token validation result</returns>
    [HttpGet("validate")]
    [Authorize]
    public ActionResult ValidateToken()
    {
        try
        {
            var userId = User.FindFirst("sub")?.Value ?? User.FindFirst("user_id")?.Value;
            var username = User.FindFirst("username")?.Value ?? User.Identity?.Name;

            return Ok(new
            {
                valid = true,
                userId = userId,
                username = username,
                expires = User.FindFirst("exp")?.Value
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating token");
            return StatusCode(500, new { error = "An error occurred during token validation" });
        }
    }

    /// <summary>
    /// Request password reset (placeholder - not implemented in interface)
    /// </summary>
    /// <param name="request">Password reset request</param>
    /// <returns>Success status</returns>
    [HttpPost("forgot-password")]
    public ActionResult ForgotPassword([FromBody] User request)
    {
        try
        {
            _logger.LogInformation("Password reset request for email: {Email}", request.Email);

            // Password reset not implemented in current interface
            return Ok(new { message = "Password reset not yet implemented" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during password reset request");
            return StatusCode(500, new { error = "An error occurred during password reset request" });
        }
    }

    /// <summary>
    /// Reset password with token (placeholder - not implemented in interface)
    /// </summary>
    /// <param name="request">Password reset confirmation</param>
    /// <returns>Success status</returns>
    [HttpPost("reset-password")]
    public ActionResult ResetPassword([FromBody] RefreshTokenRequest request)
    {
        try
        {
            _logger.LogInformation("Password reset confirmation attempt");

            // Password reset not implemented in current interface
            return Ok(new { message = "Password reset not yet implemented" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during password reset");
            return StatusCode(500, new { error = "An error occurred during password reset" });
        }
    }
}
