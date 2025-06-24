using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BIReportingCopilot.Core.Interfaces;
using BIReportingCopilot.Core.Interfaces.Security;
using BIReportingCopilot.Core.Interfaces.Query;
using BIReportingCopilot.Core.Models;
using BIReportingCopilot.Core.Configuration;
using BIReportingCopilot.Core;
using BIReportingCopilot.Infrastructure.Interfaces;
using System.Linq;

namespace BIReportingCopilot.Infrastructure.Authentication;

/// <summary>
/// Consolidated authentication service with comprehensive security features
/// </summary>
public class AuthenticationService : IAuthenticationService
{
    private readonly ILogger<AuthenticationService> _logger;
    private readonly IUserRepository _userRepository;
    private readonly ITokenRepository _tokenRepository;
    private readonly Core.Interfaces.Security.IPasswordHasher _passwordHasher;
    private readonly Core.Interfaces.Security.IAuditService _auditService;
    private readonly SecurityConfiguration _securitySettings;
    private readonly Core.Interfaces.Cache.ICacheService _cacheService;
    private readonly Core.Interfaces.Security.IMfaService _mfaService;
    private readonly BIReportingCopilot.Core.Interfaces.Security.IMfaChallengeRepository _mfaChallengeRepository;

    public AuthenticationService(
        ILogger<AuthenticationService> logger,
        IUserRepository userRepository,
        ITokenRepository tokenRepository,
        Core.Interfaces.Security.IPasswordHasher passwordHasher,
        Core.Interfaces.Security.IAuditService auditService,
        IOptions<SecurityConfiguration> securitySettings,
        Core.Interfaces.Cache.ICacheService cacheService,
        Core.Interfaces.Security.IMfaService mfaService,
        BIReportingCopilot.Core.Interfaces.Security.IMfaChallengeRepository mfaChallengeRepository)
    {
        _logger = logger;
        _userRepository = userRepository;
        _tokenRepository = tokenRepository;
        _passwordHasher = passwordHasher;
        _auditService = auditService;
        _securitySettings = securitySettings.Value;
        _cacheService = cacheService;
        _mfaService = mfaService;
        _mfaChallengeRepository = mfaChallengeRepository;
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

            // Check if MFA is enabled and required
            if (user.IsMfaEnabled && _securitySettings.EnableTwoFactorAuthentication)
            {
                // Check if this is a request with MFA code
                if (request is LoginWithMfaRequest mfaRequest && !string.IsNullOrEmpty(mfaRequest.MfaCode))
                {
                    // Validate MFA code
                    if (!string.IsNullOrEmpty(mfaRequest.ChallengeId))
                    {
                        var mfaResult = await ValidateMfaAsync(mfaRequest.ChallengeId, mfaRequest.MfaCode);
                        return mfaResult;
                    }
                    else
                    {
                        // Direct TOTP validation for convenience
                        bool isValid = false;
                        if (user.MfaMethod == MfaMethod.TOTP)
                        {
                            isValid = await _mfaService.ValidateTotpAsync(user.MfaSecret!, mfaRequest.MfaCode);
                        }

                        if (!isValid)
                        {
                            isValid = await ValidateBackupCodeAsync(user.Id, mfaRequest.MfaCode);
                        }

                        if (!isValid)
                        {
                            return new AuthenticationResult
                            {
                                Success = false,
                                ErrorMessage = "Invalid MFA code."
                            };
                        }

                        // Continue with token generation below
                    }
                }
                else
                {
                    // Initiate MFA challenge
                    var mfaChallenge = await InitiateMfaAsync(user.Id);

                    return new AuthenticationResult
                    {
                        Success = false,
                        RequiresMfa = true,
                        MfaChallenge = mfaChallenge,
                        ErrorMessage = "Multi-factor authentication required."
                    };
                }
            }

            // Generate tokens
            var accessToken = await GenerateAccessTokenAsync(user);
            var refreshToken = Guid.NewGuid().ToString();
            await _tokenRepository.StoreRefreshTokenAsync(user.Id, refreshToken, DateTime.UtcNow.AddMinutes(_securitySettings.RefreshTokenExpirationMinutes));

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
                ExpiresAt = DateTime.UtcNow.AddMinutes(_securitySettings.AccessTokenExpirationMinutes),
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
            await _tokenRepository.StoreRefreshTokenAsync(user.Id, newRefreshToken, DateTime.UtcNow.AddMinutes(_securitySettings.JwtRefreshTokenExpirationMinutes));

            // Log token refresh
            await _auditService.LogAsync(ApplicationConstants.AuditActions.TokenRefreshed, user.Id, ApplicationConstants.EntityTypes.User, user.Id);

            return new AuthenticationResult
            {
                Success = true,
                AccessToken = accessToken,
                RefreshToken = newRefreshToken,
                ExpiresAt = DateTime.UtcNow.AddMinutes(_securitySettings.JwtAccessTokenExpirationMinutes)
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
            var key = Encoding.ASCII.GetBytes(_securitySettings.JwtSecret ?? throw new InvalidOperationException("JWT Secret is not configured"));

            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = _securitySettings.ValidateIssuerSigningKey,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = _securitySettings.ValidateIssuer,
                ValidIssuer = _securitySettings.JwtIssuer,
                ValidateAudience = _securitySettings.ValidateAudience,
                ValidAudience = _securitySettings.JwtAudience,
                ValidateLifetime = _securitySettings.ValidateLifetime,
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
            var key = Encoding.ASCII.GetBytes(_securitySettings.JwtSecret ?? throw new InvalidOperationException("JWT Secret is not configured"));

            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = _securitySettings.ValidateIssuerSigningKey,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = _securitySettings.ValidateIssuer,
                ValidIssuer = _securitySettings.JwtIssuer,
                ValidateAudience = _securitySettings.ValidateAudience,
                ValidAudience = _securitySettings.JwtAudience,
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
            _securitySettings.JwtIssuer, _securitySettings.JwtAudience, _securitySettings.JwtSecret?.Length ?? 0,
            _securitySettings.JwtSecret?.Substring(0, Math.Min(10, _securitySettings.JwtSecret?.Length ?? 0)) + "...");

        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_securitySettings.JwtSecret ?? throw new InvalidOperationException("JWT Secret is not configured"));

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

        var now = DateTime.UtcNow;
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            NotBefore = now,
            IssuedAt = now,
            Expires = now.AddMinutes(_securitySettings.JwtAccessTokenExpirationMinutes),
            Issuer = _securitySettings.JwtIssuer,
            Audience = _securitySettings.JwtAudience,
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        var tokenString = tokenHandler.WriteToken(token);

        // Debug: Log token timing information
        _logger.LogInformation("JWT Token Generated - NotBefore: {NotBefore}, IssuedAt: {IssuedAt}, Expires: {Expires}, Current UTC: {CurrentUtc}",
            tokenDescriptor.NotBefore, tokenDescriptor.IssuedAt, tokenDescriptor.Expires, DateTime.UtcNow);

        return tokenString;
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

    // MFA Implementation
    public async Task<MfaSetupResult> SetupMfaAsync(string userId, MfaSetupRequest request)
    {
        try
        {
            _logger.LogInformation("Setting up MFA for user: {UserId} with method: {Method}", userId, request.Method);

            if (!_securitySettings.EnableTwoFactorAuthentication)
            {
                return new MfaSetupResult
                {
                    Success = false,
                    ErrorMessage = "Multi-factor authentication is not enabled."
                };
            }

            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                return new MfaSetupResult
                {
                    Success = false,
                    ErrorMessage = "User not found."
                };
            }

            switch (request.Method)
            {
                case MfaMethod.TOTP:
                    return await SetupTotpAsync(user);

                case MfaMethod.SMS:
                    if (string.IsNullOrEmpty(request.PhoneNumber))
                    {
                        return new MfaSetupResult
                        {
                            Success = false,
                            ErrorMessage = "Phone number is required for SMS MFA."
                        };
                    }
                    return await SetupSmsAsync(user, request.PhoneNumber);

                case MfaMethod.Email:
                    return await SetupEmailAsync(user);

                default:
                    return new MfaSetupResult
                    {
                        Success = false,
                        ErrorMessage = "Unsupported MFA method."
                    };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting up MFA for user: {UserId}", userId);
            return new MfaSetupResult
            {
                Success = false,
                ErrorMessage = "An error occurred while setting up MFA."
            };
        }
    }

    public async Task<bool> DisableMfaAsync(string userId, string verificationCode)
    {
        try
        {
            _logger.LogInformation("Disabling MFA for user: {UserId}", userId);

            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                _logger.LogWarning("User not found for MFA disable: {UserId}", userId);
                return false;
            }

            if (!user.IsMfaEnabled)
            {
                _logger.LogWarning("MFA is not enabled for user: {UserId}", userId);
                return false;
            }

            // Validate the current MFA code before disabling
            bool isValid = false;
            switch (user.MfaMethod)
            {
                case MfaMethod.TOTP:
                    isValid = await _mfaService.ValidateTotpAsync(user.MfaSecret!, verificationCode);
                    break;
                case MfaMethod.SMS:
                case MfaMethod.Email:
                    // For SMS/Email, we'll allow backup codes for disabling
                    isValid = await ValidateBackupCodeAsync(userId, verificationCode);
                    break;
            }

            if (!isValid)
            {
                _logger.LogWarning("Invalid verification code for MFA disable: {UserId}", userId);
                return false;
            }

            // Disable MFA
            user.IsMfaEnabled = false;
            user.MfaSecret = null;
            user.MfaMethod = MfaMethod.None;
            user.BackupCodes = Array.Empty<string>();
            user.PhoneNumber = null;
            user.IsPhoneNumberVerified = false;

            await _userRepository.UpdateUserAsync(user);
            await _auditService.LogAsync("MFA_DISABLED", userId, "User", userId);

            _logger.LogInformation("MFA disabled successfully for user: {UserId}", userId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error disabling MFA for user: {UserId}", userId);
            return false;
        }
    }

    public async Task<MfaChallenge> InitiateMfaAsync(string userId)
    {
        try
        {
            _logger.LogInformation("Initiating MFA challenge for user: {UserId}", userId);

            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null || !user.IsMfaEnabled)
            {
                throw new ArgumentException("User not found or MFA not enabled");
            }

            var challenge = new MfaChallenge
            {
                UserId = userId,
                Method = user.MfaMethod.ToString(),
                ExpiresAt = DateTime.UtcNow.AddMinutes(5) // 5-minute expiration
            };

            switch (user.MfaMethod)
            {
                case MfaMethod.TOTP:
                    // For TOTP, no challenge needed - user generates code from their app
                    break;

                case MfaMethod.SMS:
                    if (!string.IsNullOrEmpty(user.PhoneNumber))
                    {
                        var smsCode = await _mfaService.GenerateSmsCodeAsync(userId);
                        challenge.Challenge = smsCode;
                        await _mfaService.SendSmsAsync(user.PhoneNumber, smsCode);
                    }
                    break;

                case MfaMethod.Email:
                    var emailCode = await _mfaService.GenerateEmailCodeAsync(userId);
                    challenge.Challenge = emailCode;
                    await _mfaService.SendEmailCodeAsync(user.Email, emailCode);
                    break;
            }

            var createdChallenge = await _mfaChallengeRepository.CreateChallengeAsync(challenge);

            _logger.LogInformation("MFA challenge initiated for user: {UserId} with method: {Method}",
                userId, user.MfaMethod);

            return createdChallenge;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initiating MFA challenge for user: {UserId}", userId);
            throw;
        }
    }

    public async Task<AuthenticationResult> ValidateMfaAsync(string challengeId, string code)
    {
        try
        {
            _logger.LogInformation("Validating MFA challenge: {ChallengeId}", challengeId);

            var challenge = await _mfaChallengeRepository.GetChallengeAsync(challengeId);
            if (challenge == null)
            {
                return new AuthenticationResult
                {
                    Success = false,
                    ErrorMessage = "Invalid or expired MFA challenge."
                };
            }

            var user = await _userRepository.GetByIdAsync(challenge.UserId);
            if (user == null)
            {
                return new AuthenticationResult
                {
                    Success = false,
                    ErrorMessage = "User not found."
                };
            }

            bool isValid = false;
            bool isBackupCode = false;

            // Try to validate the code based on MFA method
            switch (challenge.Method)
            {
                case "TOTP":
                    isValid = await _mfaService.ValidateTotpAsync(user.MfaSecret!, code);
                    break;

                case "SMS":
                case "Email":
                    isValid = challenge.Challenge == code;
                    break;
            }

            // If regular validation fails, try backup codes
            if (!isValid)
            {
                isValid = await ValidateBackupCodeAsync(user.Id, code);
                isBackupCode = isValid;
            }

            if (!isValid)
            {
                _logger.LogWarning("Invalid MFA code for challenge: {ChallengeId}", challengeId);
                return new AuthenticationResult
                {
                    Success = false,
                    ErrorMessage = "Invalid verification code."
                };
            }

            // Mark challenge as used
            await _mfaChallengeRepository.MarkChallengeAsUsedAsync(challengeId);

            // Update last MFA validation date
            user.LastMfaValidationDate = DateTime.UtcNow;
            await _userRepository.UpdateUserAsync(user);

            // Generate tokens
            var accessToken = await GenerateAccessTokenAsync(user);
            var refreshToken = Guid.NewGuid().ToString();
            await _tokenRepository.StoreRefreshTokenAsync(user.Id, refreshToken,
                DateTime.UtcNow.AddMinutes(_securitySettings.JwtRefreshTokenExpirationMinutes));

            // Log successful MFA validation
            await _auditService.LogAsync("MFA_VALIDATED", user.Id, "User", user.Id,
                isBackupCode ? "Backup code used" : "MFA code validated");

            _logger.LogInformation("MFA validation successful for user: {UserId}", user.Id);

            return new AuthenticationResult
            {
                Success = true,
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresAt = DateTime.UtcNow.AddMinutes(_securitySettings.JwtAccessTokenExpirationMinutes),
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
            _logger.LogError(ex, "Error validating MFA challenge: {ChallengeId}", challengeId);
            return new AuthenticationResult
            {
                Success = false,
                ErrorMessage = "An error occurred during MFA validation."
            };
        }
    }

    public async Task<string[]> GenerateBackupCodesAsync(string userId)
    {
        try
        {
            _logger.LogInformation("Generating backup codes for user: {UserId}", userId);

            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                throw new ArgumentException("User not found");
            }

            if (!user.IsMfaEnabled)
            {
                throw new InvalidOperationException("MFA must be enabled to generate backup codes");
            }

            var backupCodesResult = await _mfaService.GenerateBackupCodesAsync(userId);

            // Hash the backup codes before storing
            user.BackupCodes = backupCodesResult.BackupCodes.Select(code => _passwordHasher.HashPassword(code)).ToArray();
            await _userRepository.UpdateUserAsync(user);

            await _auditService.LogAsync("BACKUP_CODES_GENERATED", userId, "User", userId);

            _logger.LogInformation("Generated {Count} backup codes for user: {UserId}", backupCodesResult.BackupCodes.Count, userId);

            // Return the plain text codes to the user (only time they can see them)
            return backupCodesResult.BackupCodes.ToArray();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating backup codes for user: {UserId}", userId);
            throw;
        }
    }

    public async Task<bool> ValidateBackupCodeAsync(string userId, string backupCode)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null || !user.IsMfaEnabled || user.BackupCodes == null || user.BackupCodes.Length == 0)
            {
                return false;
            }

            // Check if the backup code matches any stored hash
            for (int i = 0; i < user.BackupCodes.Length; i++)
            {
                if (_passwordHasher.VerifyPassword(backupCode, user.BackupCodes[i]))
                {
                    // Remove the used backup code
                    var backupCodesList = user.BackupCodes.ToList();
                    backupCodesList.RemoveAt(i);
                    user.BackupCodes = backupCodesList.ToArray();

                    await _userRepository.UpdateUserAsync(user);
                    await _auditService.LogAsync("BACKUP_CODE_USED", userId, "User", userId);

                    _logger.LogInformation("Backup code validated and consumed for user: {UserId}", userId);
                    return true;
                }
            }

            _logger.LogWarning("Invalid backup code for user: {UserId}", userId);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating backup code for user: {UserId}", userId);
            return false;
        }
    }

    private async Task<MfaSetupResult> SetupTotpAsync(User user)
    {
        var secret = await _mfaService.GenerateSecretAsync(user.Id);
        var qrCode = await _mfaService.GenerateQrCodeAsync(user.Id, secret);
        var backupCodesResult = await _mfaService.GenerateBackupCodesAsync(user.Id);

        // Store the secret and setup info
        user.IsMfaEnabled = true;
        user.MfaSecret = secret;
        user.MfaMethod = MfaMethod.TOTP;
        user.BackupCodes = backupCodesResult.BackupCodes.Select(code => _passwordHasher.HashPassword(code)).ToArray();

        await _userRepository.UpdateUserAsync(user);
        await _auditService.LogAsync("MFA_SETUP_TOTP", user.Id, "User", user.Id);

        return new MfaSetupResult
        {
            Success = true,
            Secret = secret,
            QrCode = qrCode,
            BackupCodes = backupCodesResult.BackupCodes
        };
    }

    private async Task<MfaSetupResult> SetupSmsAsync(User user, string phoneNumber)
    {
        var backupCodesResult = await _mfaService.GenerateBackupCodesAsync(user.Id);

        user.IsMfaEnabled = true;
        user.MfaMethod = MfaMethod.SMS;
        user.PhoneNumber = phoneNumber;
        user.IsPhoneNumberVerified = true; // Assume verified for now
        user.BackupCodes = backupCodesResult.BackupCodes.Select(code => _passwordHasher.HashPassword(code)).ToArray();

        await _userRepository.UpdateUserAsync(user);
        await _auditService.LogAsync("MFA_SETUP_SMS", user.Id, "User", user.Id);

        return new MfaSetupResult
        {
            Success = true,
            BackupCodes = backupCodesResult.BackupCodes
        };
    }

    private async Task<MfaSetupResult> SetupEmailAsync(User user)
    {
        var backupCodesResult = await _mfaService.GenerateBackupCodesAsync(user.Id);

        user.IsMfaEnabled = true;
        user.MfaMethod = MfaMethod.Email;
        user.BackupCodes = backupCodesResult.BackupCodes.Select(code => _passwordHasher.HashPassword(code)).ToArray();

        await _userRepository.UpdateUserAsync(user);
        await _auditService.LogAsync("MFA_SETUP_EMAIL", user.Id, "User", user.Id);

        return new MfaSetupResult
        {
            Success = true,
            BackupCodes = backupCodesResult.BackupCodes
        };
    }

    // =============================================================================
    // MISSING INTERFACE METHOD IMPLEMENTATIONS
    // =============================================================================

    /// <summary>
    /// Authenticate user with cancellation token support
    /// </summary>
    public async Task<AuthenticationResult> AuthenticateAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        return await AuthenticateAsync(request);
    }

    /// <summary>
    /// Refresh token with proper request object
    /// </summary>
    public async Task<AuthenticationResult> RefreshTokenAsync(RefreshTokenRequest request, CancellationToken cancellationToken = default)
    {
        return await RefreshTokenAsync(request.RefreshToken);
    }

    /// <summary>
    /// Validate token with cancellation token support
    /// </summary>
    public async Task<bool> ValidateTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        return await ValidateTokenAsync(token);
    }

    /// <summary>
    /// Get user from token
    /// </summary>
    public async Task<UserInfo?> GetUserFromTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        try
        {
            var principal = GetPrincipalFromToken(token);
            var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
                return null;

            return await GetUserInfoAsync(userId);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Revoke all user tokens
    /// </summary>
    public async Task<bool> RevokeAllUserTokensAsync(string userId, CancellationToken cancellationToken = default)
    {
        try
        {
            // This would need to be implemented with a proper token repository
            // For now, return true as a stub
            _logger.LogInformation("Revoking all tokens for user: {UserId}", userId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error revoking all tokens for user: {UserId}", userId);
            return false;
        }
    }

    /// <summary>
    /// Get active sessions for user
    /// </summary>
    public async Task<List<UserSession>> GetActiveSessionsAsync(string userId, CancellationToken cancellationToken = default)
    {
        try
        {
            // This would need to be implemented with a proper session repository
            // For now, return empty list as a stub
            _logger.LogInformation("Getting active sessions for user: {UserId}", userId);
            return new List<UserSession>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting active sessions for user: {UserId}", userId);
            return new List<UserSession>();
        }
    }

    /// <summary>
    /// Terminate specific session
    /// </summary>
    public async Task<bool> TerminateSessionAsync(string sessionId, CancellationToken cancellationToken = default)
    {
        try
        {
            // This would need to be implemented with a proper session repository
            // For now, return true as a stub
            _logger.LogInformation("Terminating session: {SessionId}", sessionId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error terminating session: {SessionId}", sessionId);
            return false;
        }
    }

    /// <summary>
    /// Login with MFA
    /// </summary>
    public async Task<AuthenticationResult> LoginWithMfaAsync(LoginWithMfaRequest request, CancellationToken cancellationToken = default)
    {
        return await AuthenticateAsync(request);
    }

    /// <summary>
    /// Initiate MFA challenge
    /// </summary>
    public async Task<MfaChallenge> InitiateMfaChallengeAsync(string userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var challenge = await InitiateMfaAsync(userId);
            return new MfaChallenge
            {
                ChallengeId = challenge.ChallengeId,
                Method = challenge.Method,
                Challenge = challenge.Challenge,
                ExpiresAt = challenge.ExpiresAt
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initiating MFA challenge for user: {UserId}", userId);
            throw;
        }
    }

    /// <summary>
    /// Validate MFA with proper request object
    /// </summary>
    public async Task<MfaValidationResult> ValidateMfaAsync(MfaValidationRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await ValidateMfaAsync(request.ChallengeId, request.Code);
            return new MfaValidationResult
            {
                Success = result.Success,
                ErrorMessage = result.ErrorMessage
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating MFA");
            return new MfaValidationResult
            {
                Success = false,
                ErrorMessage = "An error occurred during MFA validation."
            };
        }
    }

    /// <summary>
    /// Revoke a specific token
    /// </summary>
    public async Task<bool> RevokeTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Revoking token");

            // For refresh tokens, revoke from repository
            await _tokenRepository.RevokeRefreshTokenAsync(token);

            // For access tokens, we would need to add them to a blacklist
            // This is a simplified implementation
            var cacheKey = $"revoked_token:{token}";
            await _cacheService.SetAsync(cacheKey, true, TimeSpan.FromMinutes(_securitySettings.JwtAccessTokenExpirationMinutes));

            _logger.LogInformation("Token revoked successfully");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error revoking token");
            return false;
        }
    }
}

public class AccountLockoutInfo
{
    public string Username { get; set; } = string.Empty;
    public DateTime LockedUntil { get; set; }
    public int FailedAttempts { get; set; }
}