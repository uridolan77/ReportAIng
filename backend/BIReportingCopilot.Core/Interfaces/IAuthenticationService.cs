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

public interface IUserRepository
{
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
    Task<MfaStatus?> GetMfaStatusAsync(Guid userId);
    Task<MfaSetupInfo?> SetupMfaAsync(Guid userId, MfaMethod method);
    Task<bool> VerifySetupAsync(Guid userId, string verificationCode);
    Task<bool> DisableMfaAsync(Guid userId, string verificationCode);
    
    // Validation and Challenges
    Task<bool> ValidateMfaCodeAsync(Guid userId, string code);
    Task<MfaChallengeResponse> SendMfaChallengeAsync(Guid userId, MfaMethod method);
    
    // Backup Codes
    Task<string[]> GenerateBackupCodesAsync(Guid userId);
    Task<int> GetBackupCodesCountAsync(Guid userId);
    
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
