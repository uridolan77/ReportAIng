using BIReportingCopilot.Core.Models;
using System.Security.Claims;

namespace BIReportingCopilot.Core.Interfaces;

public interface IAuthenticationService
{
    Task<AuthenticationResult> AuthenticateAsync(LoginRequest request);
    Task<AuthenticationResult> RefreshTokenAsync(string refreshToken);
    Task<bool> ValidateTokenAsync(string token);
    Task<UserInfo> GetUserInfoAsync(string userId);
    Task<bool> RevokeTokenAsync(string token);
    ClaimsPrincipal GetPrincipalFromToken(string token);

    // MFA methods
    Task<MfaSetupResult> SetupMfaAsync(string userId, MfaSetupRequest request);
    Task<bool> DisableMfaAsync(string userId, string verificationCode);
    Task<MfaChallenge> InitiateMfaAsync(string userId);
    Task<AuthenticationResult> ValidateMfaAsync(string challengeId, string code);
    Task<string[]> GenerateBackupCodesAsync(string userId);
    Task<bool> ValidateBackupCodeAsync(string userId, string backupCode);
}

/// <summary>
/// Unified user repository interface combining domain model and entity operations
/// Consolidates previous UserRepository and UserEntityRepository functionality
/// </summary>
public interface IUserRepository
{
    #region Authentication & Core Operations
    Task<User?> ValidateCredentialsAsync(string username, string password);
    Task<User?> GetByIdAsync(string userId);
    Task<User?> GetByUsernameAsync(string username);
    Task<User?> GetByEmailAsync(string email);
    Task<List<string>> GetUserPermissionsAsync(string userId);
    Task<List<string>> GetUserRolesAsync(string userId);
    Task<User> CreateUserAsync(User user);
    Task<User> CreateUserWithPasswordAsync(User user, string password);
    Task<bool> UpdatePasswordAsync(string userId, string newPassword);
    Task<User> UpdateUserAsync(User user);
    Task<bool> DeleteUserAsync(string userId);
    #endregion

    #region Enhanced Domain Model Operations
    Task<User?> GetUserByIdAsync(string userId);
    Task<User?> GetUserByUsernameAsync(string username);
    Task<User?> GetUserByEmailAsync(string email);
    Task<User> CreateUserAsync(User user, string passwordHash);
    Task<List<User>> GetActiveUsersAsync();
    Task<List<User>> SearchUsersAsync(string searchTerm, int page = 1, int pageSize = 20);
    #endregion

    #region Authentication & Security
    Task<bool> UpdatePasswordHashAsync(string userId, string passwordHash);
    Task<bool> UpdateLastLoginAsync(string userId, DateTime loginDate);
    Task<bool> UsernameExistsAsync(string username);
    Task<bool> EmailExistsAsync(string email);
    Task<string?> GetPasswordHashAsync(string userId);
    #endregion

    #region MFA Operations
    Task<bool> UpdateMfaSettingsAsync(string userId, bool enabled, string? secret, MfaMethod method);
    Task<bool> UpdatePhoneNumberAsync(string userId, string? phoneNumber, bool isVerified);
    Task<bool> UpdateBackupCodesAsync(string userId, string[] backupCodes);
    Task<bool> UpdateLastMfaValidationAsync(string userId, DateTime validationDate);
    #endregion

    #region User Activity & Analytics
    Task<bool> UpdateActivitySummaryAsync(string userId, UserActivitySummary activitySummary);
    Task<UserActivitySummary?> GetActivitySummaryAsync(string userId);
    Task RecordUserActivityAsync(string userId, string activityType, Dictionary<string, object>? metadata = null);
    #endregion

    #region User Preferences
    Task<bool> UpdatePreferencesAsync(string userId, UserPreferences preferences);
    Task<UserPreferences?> GetPreferencesAsync(string userId);
    #endregion

    #region Bulk Operations
    Task<List<User>> GetUsersByIdsAsync(string[] userIds);
    Task<List<User>> GetUsersByRoleAsync(string role);
    Task<bool> BulkUpdateUserStatusAsync(string[] userIds, bool isActive);
    #endregion

    #region Statistics & Reporting
    Task<int> GetTotalUserCountAsync();
    Task<int> GetActiveUserCountAsync();
    Task<List<User>> GetUsersCreatedInRangeAsync(DateTime startDate, DateTime endDate);
    Task<Dictionary<string, int>> GetUserLoginStatsAsync(DateTime startDate, DateTime endDate);
    #endregion
}

public interface ITokenRepository
{
    Task StoreRefreshTokenAsync(string userId, string token, DateTime expiresAt);
    Task<TokenInfo?> GetRefreshTokenAsync(string token);
    Task<string?> GetUserIdFromRefreshTokenAsync(string token);
    Task UpdateRefreshTokenAsync(string oldToken, string newToken, DateTime expiresAt);
    Task RevokeRefreshTokenAsync(string token);
    Task RevokeAllUserTokensAsync(string userId);
    Task CleanupExpiredTokensAsync();
}

// MFA-related interfaces
public interface IMfaService
{
    // Status and Setup
    Task<MfaStatus?> GetMfaStatusAsync(string userId);
    Task<MfaSetupInfo?> SetupMfaAsync(string userId, MfaMethod method);
    Task<bool> VerifyMfaSetupAsync(string userId, string verificationCode);
    Task<bool> DisableMfaAsync(string userId, string verificationCode);

    // Validation and Challenges
    Task<bool> ValidateMfaCodeAsync(string userId, string code);
    Task<bool> ValidateMfaAsync(MfaChallengeRequest request);
    Task<MfaChallengeResponse?> SendMfaChallengeAsync(string userId, MfaMethod method);

    // Backup Codes
    Task<string[]> GenerateBackupCodesAsync(string userId);
    Task<int> GetBackupCodesCountAsync(string userId);
    Task<int> GetRemainingBackupCodesCountAsync(string userId);

    // Testing/Utility
    Task<bool> TestSmsDeliveryAsync(string phoneNumber);

    // Legacy methods for backward compatibility
    Task<string> GenerateSecretAsync();
    Task<string> GenerateQrCodeAsync(string secret, string userEmail, string issuer);
    Task<bool> ValidateTotpAsync(string secret, string code);
    Task<string> GenerateSmsCodeAsync();
    Task<bool> SendSmsAsync(string phoneNumber, string code);
    Task<string> GenerateEmailCodeAsync();
    Task<bool> SendEmailCodeAsync(string email, string code);
    Task<string[]> GenerateBackupCodesAsync(int count = 8);
}

public interface IMfaChallengeRepository
{
    Task<MfaChallenge> CreateChallengeAsync(MfaChallenge challenge);
    Task<MfaChallenge?> GetChallengeAsync(string challengeId);
    Task<bool> MarkChallengeAsUsedAsync(string challengeId);
    Task CleanupExpiredChallengesAsync();
    Task<MfaChallenge?> GetActiveChallengeAsync(string userId, MfaMethod method);
}

// Communication service interfaces
public interface IEmailService
{
    Task<bool> SendAsync(string to, string subject, string body);
    Task<bool> SendAsync(string[] to, string subject, string body);
    Task<bool> SendMfaCodeAsync(string toEmail, string mfaCode);
}

public interface ISmsService
{
    Task<bool> SendAsync(string phoneNumber, string message);
    Task<bool> SendSmsAsync(string phoneNumber, string message);
    Task<bool> TestDeliveryAsync(string phoneNumber);
}
