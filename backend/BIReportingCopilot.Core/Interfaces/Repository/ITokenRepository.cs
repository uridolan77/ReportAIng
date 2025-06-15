using BIReportingCopilot.Core.Models;
using System.Threading.Tasks;

namespace BIReportingCopilot.Core.Interfaces.Repository
{
    public interface ITokenRepository
    {
        Task StoreRefreshTokenAsync(string userId, string token, DateTime expiresAt);
        Task<RefreshTokenInfo?> GetRefreshTokenAsync(string token);
        Task<string?> GetUserIdFromRefreshTokenAsync(string token);
        Task UpdateRefreshTokenAsync(string oldToken, string newToken, DateTime expiresAt);
        Task RevokeRefreshTokenAsync(string token);
        Task RevokeAllUserTokensAsync(string userId);
        Task CleanupExpiredTokensAsync();
    }
}
