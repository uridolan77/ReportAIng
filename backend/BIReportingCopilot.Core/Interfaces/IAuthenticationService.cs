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
