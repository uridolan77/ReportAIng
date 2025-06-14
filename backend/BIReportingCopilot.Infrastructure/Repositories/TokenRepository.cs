using BIReportingCopilot.Core.Interfaces;
using BIReportingCopilot.Core.Interfaces.Security;
using BIReportingCopilot.Core.Models;
using BIReportingCopilot.Infrastructure.Data;
using BIReportingCopilot.Infrastructure.Data.Entities;
using BIReportingCopilot.Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BIReportingCopilot.Infrastructure.Repositories;

public class TokenRepository : ITokenRepository
{
    private readonly BICopilotContext _context;
    private readonly ILogger<TokenRepository> _logger;

    public TokenRepository(BICopilotContext context, ILogger<TokenRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task StoreRefreshTokenAsync(string userId, string token, DateTime expiresAt)
    {
        try
        {
            var refreshTokenEntity = new Infrastructure.Data.Entities.RefreshTokenEntity
            {
                Token = token,
                UserId = userId,
                ExpiresAt = expiresAt,
                CreatedDate = DateTime.UtcNow,
                IsRevoked = false
            };

            _context.RefreshTokens.Add(refreshTokenEntity);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Refresh token stored for user {UserId}", userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error storing refresh token for user {UserId}", userId);
            throw;
        }
    }

    public async Task<RefreshTokenInfo?> GetRefreshTokenAsync(string token)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                return null;
            }

            var refreshTokenEntity = await _context.RefreshTokens
                .FirstOrDefaultAsync(t => t.Token == token && !t.IsRevoked && t.ExpiresAt > DateTime.UtcNow);

            if (refreshTokenEntity == null)
            {
                return null;
            }

            return new RefreshTokenInfo
            {
                Token = refreshTokenEntity.Token,
                UserId = refreshTokenEntity.UserId,
                ExpiresAt = refreshTokenEntity.ExpiresAt,
                CreatedAt = refreshTokenEntity.CreatedDate,
                IsRevoked = refreshTokenEntity.IsRevoked
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting refresh token");
            return null;
        }
    }

    public async Task<string?> GetUserIdFromRefreshTokenAsync(string token)
    {
        try
        {
            var tokenInfo = await GetRefreshTokenAsync(token);
            return tokenInfo?.UserId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user ID from refresh token");
            return null;
        }
    }

    public async Task UpdateRefreshTokenAsync(string oldToken, string newToken, DateTime expiresAt)
    {
        try
        {
            var oldTokenEntity = await _context.RefreshTokens
                .FirstOrDefaultAsync(t => t.Token == oldToken && !t.IsRevoked);

            if (oldTokenEntity != null)
            {
                // Revoke the old token
                oldTokenEntity.IsRevoked = true;
                oldTokenEntity.UpdatedDate = DateTime.UtcNow;

                // Create new token
                var newTokenEntity = new Infrastructure.Data.Entities.RefreshTokenEntity
                {
                    Token = newToken,
                    UserId = oldTokenEntity.UserId,
                    ExpiresAt = expiresAt,
                    CreatedDate = DateTime.UtcNow,
                    IsRevoked = false
                };

                _context.RefreshTokens.Add(newTokenEntity);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Refresh token updated for user {UserId}", oldTokenEntity.UserId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating refresh token");
            throw;
        }
    }

    public async Task RevokeRefreshTokenAsync(string token)
    {
        try
        {
            var tokenEntity = await _context.RefreshTokens
                .FirstOrDefaultAsync(t => t.Token == token && !t.IsRevoked);

            if (tokenEntity != null)
            {
                tokenEntity.IsRevoked = true;
                tokenEntity.UpdatedDate = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                _logger.LogInformation("Refresh token revoked for user {UserId}", tokenEntity.UserId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error revoking refresh token");
            throw;
        }
    }

    public async Task RevokeAllUserTokensAsync(string userId)
    {
        try
        {
            var userTokens = await _context.RefreshTokens
                .Where(t => t.UserId == userId && !t.IsRevoked)
                .ToListAsync();

            foreach (var token in userTokens)
            {
                token.IsRevoked = true;
                token.UpdatedDate = DateTime.UtcNow;
            }

            if (userTokens.Any())
            {
                await _context.SaveChangesAsync();
                _logger.LogInformation("All {Count} tokens revoked for user {UserId}", userTokens.Count, userId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error revoking all tokens for user {UserId}", userId);
            throw;
        }
    }

    public async Task CleanupExpiredTokensAsync()
    {
        try
        {
            var expiredTokens = await _context.RefreshTokens
                .Where(t => t.ExpiresAt < DateTime.UtcNow || t.IsRevoked)
                .ToListAsync();

            if (expiredTokens.Any())
            {
                _context.RefreshTokens.RemoveRange(expiredTokens);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Cleaned up {Count} expired tokens", expiredTokens.Count);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cleaning up expired tokens");
            throw;
        }
    }
}
