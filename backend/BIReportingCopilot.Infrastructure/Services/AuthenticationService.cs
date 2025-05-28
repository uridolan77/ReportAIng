using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BIReportingCopilot.Core.Interfaces;
using BIReportingCopilot.Core.Models;
using BIReportingCopilot.Core.Configuration;
using BIReportingCopilot.Core.Constants;
using System.Linq;

namespace BIReportingCopilot.Infrastructure.Services;

/// <summary>
/// Consolidated authentication service with comprehensive security features
/// </summary>
public class AuthenticationService : IAuthenticationService
{
    private readonly ILogger<AuthenticationService> _logger;
    private readonly IUserRepository _userRepository;
    private readonly ITokenRepository _tokenRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IAuditService _auditService;
    private readonly JwtSettings _jwtSettings;
    private readonly SecuritySettings _securitySettings;
    private readonly ICacheService _cacheService;

    public AuthenticationService(
        ILogger<AuthenticationService> logger,
        IUserRepository userRepository,
        ITokenRepository tokenRepository,
        IPasswordHasher passwordHasher,
        IAuditService auditService,
        IOptions<JwtSettings> jwtSettings,
        IOptions<SecuritySettings> securitySettings,
        ICacheService cacheService)
    {
        _logger = logger;
        _userRepository = userRepository;
        _tokenRepository = tokenRepository;
        _passwordHasher = passwordHasher;
        _auditService = auditService;
        _jwtSettings = jwtSettings.Value;
        _securitySettings = securitySettings.Value;
        _cacheService = cacheService;
    }

    public async Task<AuthenticationResult> AuthenticateAsync(LoginRequest request)
    {
        try
        {
            _logger.LogInformation("Authentication attempt for user: {Username}", request.Username);

            // Check for account lockout
            if (await IsAccountLockedAsync(request.Username))
            {
                _logger.LogWarning("Authentication failed - account locked: {Username}", request.Username);
                await LogSecurityEventAsync(request.Username, ApplicationConstants.AuditActions.SecurityViolation, "Account locked");

                return new AuthenticationResult
                {
                    Success = false,
                    ErrorMessage = "Account is temporarily locked due to multiple failed login attempts."
                };
            }

            // Validate credentials
            var user = await _userRepository.ValidateCredentialsAsync(request.Username, request.Password);
            if (user == null)
            {
                await HandleFailedLoginAsync(request.Username);
                return new AuthenticationResult
                {
                    Success = false,
                    ErrorMessage = "Invalid username or password."
                };
            }

            // Check if user is active
            if (!user.IsActive)
            {
                _logger.LogWarning("Authentication failed - inactive user: {Username}", request.Username);
                await LogSecurityEventAsync(request.Username, ApplicationConstants.AuditActions.SecurityViolation, "Inactive user login attempt");

                return new AuthenticationResult
                {
                    Success = false,
                    ErrorMessage = "Account is disabled."
                };
            }

            // Clear failed login attempts on successful authentication
            await ClearFailedLoginAttemptsAsync(request.Username);

            // Generate tokens
            var accessToken = await GenerateAccessTokenAsync(user);
            var refreshToken = Guid.NewGuid().ToString();
            await _tokenRepository.StoreRefreshTokenAsync(user.Id, refreshToken, DateTime.UtcNow.AddMinutes(_jwtSettings.RefreshTokenExpirationMinutes));

            // Update last login
            user.LastLoginDate = DateTime.UtcNow;
            await _userRepository.UpdateUserAsync(user);

            // Log successful authentication
            await _auditService.LogAsync(ApplicationConstants.AuditActions.Login, user.Id, ApplicationConstants.EntityTypes.User, user.Id);

            _logger.LogInformation("Authentication successful for user: {Username}", request.Username);

            return new AuthenticationResult
            {
                Success = true,
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes),
                User = new UserInfo
                {
                    Id = user.Id,
                    Username = user.Username,
                    Email = user.Email,
                    DisplayName = user.DisplayName,
                    Roles = GetUserRoles(user.Roles),
                    LastLogin = user.LastLoginDate
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Authentication error for user: {Username}", request.Username);
            return new AuthenticationResult
            {
                Success = false,
                ErrorMessage = "An error occurred during authentication."
            };
        }
    }

    public async Task<AuthenticationResult> RefreshTokenAsync(string refreshToken)
    {
        try
        {
            var storedToken = await _tokenRepository.GetRefreshTokenAsync(refreshToken);
            if (storedToken == null || storedToken.ExpiresAt < DateTime.UtcNow)
            {
                _logger.LogWarning("Invalid or expired refresh token used");
                return new AuthenticationResult
                {
                    Success = false,
                    ErrorMessage = "Invalid or expired refresh token."
                };
            }

            var userId = await _tokenRepository.GetUserIdFromRefreshTokenAsync(refreshToken);
            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("Refresh token has no associated user");
                return new AuthenticationResult
                {
                    Success = false,
                    ErrorMessage = "Invalid refresh token."
                };
            }

            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null || !user.IsActive)
            {
                _logger.LogWarning("Refresh token used for inactive user: {UserId}", userId);
                return new AuthenticationResult
                {
                    Success = false,
                    ErrorMessage = "User account is not active."
                };
            }

            // Revoke old refresh token
            await _tokenRepository.RevokeRefreshTokenAsync(refreshToken);

            // Generate new tokens
            var accessToken = await GenerateAccessTokenAsync(user);
            var newRefreshToken = Guid.NewGuid().ToString();
            await _tokenRepository.StoreRefreshTokenAsync(user.Id, newRefreshToken, DateTime.UtcNow.AddMinutes(_jwtSettings.RefreshTokenExpirationMinutes));

            // Log token refresh
            await _auditService.LogAsync(ApplicationConstants.AuditActions.TokenRefreshed, user.Id, ApplicationConstants.EntityTypes.User, user.Id);

            return new AuthenticationResult
            {
                Success = true,
                AccessToken = accessToken,
                RefreshToken = newRefreshToken,
                ExpiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing token");
            return new AuthenticationResult
            {
                Success = false,
                ErrorMessage = "An error occurred while refreshing the token."
            };
        }
    }

    public async Task<bool> ValidateTokenAsync(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_jwtSettings.Secret);

            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = _jwtSettings.ValidateIssuerSigningKey,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = _jwtSettings.ValidateIssuer,
                ValidIssuer = _jwtSettings.Issuer,
                ValidateAudience = _jwtSettings.ValidateAudience,
                ValidAudience = _jwtSettings.Audience,
                ValidateLifetime = _jwtSettings.ValidateLifetime,
                ClockSkew = TimeSpan.Zero
            };

            tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Token validation failed");
            return false;
        }
    }

    public async Task<bool> RevokeTokenAsync(string refreshToken)
    {
        try
        {
            await _tokenRepository.RevokeRefreshTokenAsync(refreshToken);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error revoking token");
            return false;
        }
    }

    public async Task<UserInfo> GetUserInfoAsync(string userId)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                throw new ArgumentException($"User with ID {userId} not found");

            return new UserInfo
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                DisplayName = user.DisplayName,
                Roles = GetUserRoles(user.Roles),
                LastLogin = user.LastLoginDate
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user info for user: {UserId}", userId);
            throw;
        }
    }

    public ClaimsPrincipal GetPrincipalFromToken(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_jwtSettings.Secret);

            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = _jwtSettings.ValidateIssuerSigningKey,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = _jwtSettings.ValidateIssuer,
                ValidIssuer = _jwtSettings.Issuer,
                ValidateAudience = _jwtSettings.ValidateAudience,
                ValidAudience = _jwtSettings.Audience,
                ValidateLifetime = false, // Don't validate lifetime for principal extraction
                ClockSkew = TimeSpan.Zero
            };

            var principal = tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);
            return principal;
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Failed to extract principal from token");
            throw new SecurityTokenException("Invalid token", ex);
        }
    }

    private async Task<string> GenerateAccessTokenAsync(User user)
    {
        // Debug: Log the JWT settings being used
        _logger.LogInformation("JWT Settings - Issuer: {Issuer}, Audience: {Audience}, Secret: {SecretLength} chars, SecretPrefix: {SecretPrefix}",
            _jwtSettings.Issuer, _jwtSettings.Audience, _jwtSettings.Secret?.Length ?? 0,
            _jwtSettings.Secret?.Substring(0, Math.Min(10, _jwtSettings.Secret.Length)) + "...");

        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_jwtSettings.Secret);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Name, user.Username),
            new(ClaimTypes.Email, user.Email),
            new("display_name", user.DisplayName)
        };

        // Add role claims
        var roles = GetUserRoles(user.Roles);
        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role.Trim())));

        // Add permission claims
        var permissions = await _userRepository.GetUserPermissionsAsync(user.Id);
        claims.AddRange(permissions.Select(permission => new Claim("permission", permission)));

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes),
            Issuer = _jwtSettings.Issuer,
            Audience = _jwtSettings.Audience,
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    private async Task<bool> IsAccountLockedAsync(string username)
    {
        var cacheKey = $"lockout:{username}";
        var lockoutInfo = await _cacheService.GetAsync<AccountLockoutInfo>(cacheKey);

        return lockoutInfo != null && lockoutInfo.LockedUntil > DateTime.UtcNow;
    }

    private async Task HandleFailedLoginAsync(string username)
    {
        var cacheKey = $"failed_attempts:{username}";
        var attemptsObj = await _cacheService.GetAsync<object>(cacheKey);
        var attempts = attemptsObj != null ? Convert.ToInt32(attemptsObj) : 0;
        attempts++;

        await _cacheService.SetAsync(cacheKey, (object)attempts, TimeSpan.FromMinutes(_securitySettings.LockoutDurationMinutes));

        if (attempts >= _securitySettings.MaxLoginAttempts)
        {
            var lockoutKey = $"lockout:{username}";
            var lockoutInfo = new AccountLockoutInfo
            {
                Username = username,
                LockedUntil = DateTime.UtcNow.AddMinutes(_securitySettings.LockoutDurationMinutes),
                FailedAttempts = attempts
            };

            await _cacheService.SetAsync(lockoutKey, lockoutInfo, TimeSpan.FromMinutes(_securitySettings.LockoutDurationMinutes));

            _logger.LogWarning("Account locked for user: {Username} after {Attempts} failed attempts", username, attempts);
            await LogSecurityEventAsync(username, ApplicationConstants.AuditActions.SecurityViolation, $"Account locked after {attempts} failed attempts");
        }
        else
        {
            _logger.LogWarning("Failed login attempt {Attempts}/{MaxAttempts} for user: {Username}", attempts, _securitySettings.MaxLoginAttempts, username);
        }
    }

    private async Task ClearFailedLoginAttemptsAsync(string username)
    {
        var cacheKey = $"failed_attempts:{username}";
        await _cacheService.RemoveAsync(cacheKey);

        var lockoutKey = $"lockout:{username}";
        await _cacheService.RemoveAsync(lockoutKey);
    }

    private async Task LogSecurityEventAsync(string username, string action, string details)
    {
        try
        {
            await _auditService.LogAsync(action, username, ApplicationConstants.EntityTypes.User, username, details);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to log security event for user: {Username}", username);
        }
    }

    private static string[] GetUserRoles(string[] roles)
    {
        return roles ?? Array.Empty<string>();
    }
}

public class AccountLockoutInfo
{
    public string Username { get; set; } = string.Empty;
    public DateTime LockedUntil { get; set; }
    public int FailedAttempts { get; set; }
}