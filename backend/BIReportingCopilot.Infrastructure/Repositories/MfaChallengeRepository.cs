using Microsoft.Extensions.Logging;
using BIReportingCopilot.Core.Interfaces;
using BIReportingCopilot.Core.Interfaces.Security;
using BIReportingCopilot.Core.Models;
using BIReportingCopilot.Infrastructure.Data;
using BIReportingCopilot.Infrastructure.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace BIReportingCopilot.Infrastructure.Repositories;

/// <summary>
/// Repository for managing MFA challenges
/// </summary>
public class MfaChallengeRepository : IMfaChallengeRepository
{
    private readonly BICopilotContext _context;
    private readonly ILogger<MfaChallengeRepository> _logger;

    public MfaChallengeRepository(
        BICopilotContext context,
        ILogger<MfaChallengeRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<MfaChallenge> CreateChallengeAsync(MfaChallenge challenge, CancellationToken cancellationToken = default)
    {
        try
        {
            challenge.ChallengeId = Guid.NewGuid().ToString();
            
            var entity = new Infrastructure.Data.Entities.MfaChallengeEntity
            {
                Id = challenge.ChallengeId,
                UserId = challenge.UserId,
                MfaMethod = challenge.Method.ToString(),
                ChallengeCode = challenge.Challenge ?? string.Empty,
                ExpiresAt = challenge.ExpiresAt,
                IsUsed = challenge.IsUsed,
                DeliveryAddress = null, // Will be set when challenge is delivered
                AttemptCount = 0,
                CreatedDate = DateTime.UtcNow
            };

            _context.MfaChallenges.Add(entity);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Created MFA challenge {ChallengeId} for user {UserId}", 
                challenge.ChallengeId, challenge.UserId);

            return challenge;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating MFA challenge for user {UserId}", challenge.UserId);
            throw;
        }
    }

    public async Task<MfaChallenge?> GetChallengeAsync(string challengeId, CancellationToken cancellationToken = default)
    {
        try
        {
            var entity = await _context.MfaChallenges
                .Where(c => c.Id == challengeId && !c.IsUsed && c.ExpiresAt > DateTime.UtcNow)
                .FirstOrDefaultAsync(cancellationToken);

            if (entity == null)
            {
                _logger.LogWarning("MFA challenge {ChallengeId} not found or expired", challengeId);
                return null;
            }

            return new MfaChallenge
            {
                ChallengeId = entity.Id,
                UserId = entity.UserId,
                Method = entity.MfaMethod,
                Challenge = entity.ChallengeCode,
                ExpiresAt = entity.ExpiresAt,
                IsUsed = entity.IsUsed
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving MFA challenge {ChallengeId}", challengeId);
            return null;
        }
    }

    public async Task<bool> MarkChallengeAsUsedAsync(string challengeId)
    {
        try
        {
            var entity = await _context.MfaChallenges
                .Where(c => c.Id == challengeId)
                .FirstOrDefaultAsync();

            if (entity == null)
            {
                _logger.LogWarning("MFA challenge {ChallengeId} not found", challengeId);
                return false;
            }

            entity.IsUsed = true;
            entity.UpdatedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Marked MFA challenge {ChallengeId} as used", challengeId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking MFA challenge {ChallengeId} as used", challengeId);
            return false;
        }
    }

    public async Task CleanupExpiredChallengesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var expiredChallenges = await _context.MfaChallenges
                .Where(c => c.ExpiresAt < DateTime.UtcNow || c.IsUsed)
                .ToListAsync(cancellationToken);

            if (expiredChallenges.Any())
            {
                _context.MfaChallenges.RemoveRange(expiredChallenges);
                await _context.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Cleaned up {Count} expired MFA challenges", expiredChallenges.Count);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cleaning up expired MFA challenges");
        }
    }

    public async Task<List<MfaChallenge>> GetActiveChallengesAsync(string userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var entities = await _context.MfaChallenges
                .Where(c => c.UserId == userId && !c.IsUsed && c.ExpiresAt > DateTime.UtcNow)
                .OrderByDescending(c => c.CreatedDate)
                .ToListAsync(cancellationToken);

            return entities.Select(entity => new MfaChallenge
            {
                ChallengeId = entity.Id,
                UserId = entity.UserId,
                Method = entity.MfaMethod,
                Challenge = entity.ChallengeCode,
                ExpiresAt = entity.ExpiresAt,
                IsUsed = entity.IsUsed
            }).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting active MFA challenges for user: {UserId}", userId);
            return new List<MfaChallenge>();
        }
    }

    public async Task UpdateChallengeAsync(MfaChallenge challenge, CancellationToken cancellationToken = default)
    {
        try
        {
            var entity = await _context.MfaChallenges
                .Where(c => c.Id == challenge.ChallengeId)
                .FirstOrDefaultAsync(cancellationToken);

            if (entity == null)
            {
                _logger.LogWarning("MFA challenge {ChallengeId} not found for update", challenge.ChallengeId);
                return;
            }

            entity.MfaMethod = challenge.Method.ToString();
            entity.ChallengeCode = challenge.Challenge ?? string.Empty;
            entity.ExpiresAt = challenge.ExpiresAt;
            entity.IsUsed = challenge.IsUsed;
            entity.UpdatedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Updated MFA challenge {ChallengeId}", challenge.ChallengeId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating MFA challenge {ChallengeId}", challenge.ChallengeId);
            throw;
        }
    }

    public async Task DeleteChallengeAsync(string challengeId, CancellationToken cancellationToken = default)
    {
        try
        {
            var entity = await _context.MfaChallenges
                .Where(c => c.Id == challengeId)
                .FirstOrDefaultAsync(cancellationToken);

            if (entity == null)
            {
                _logger.LogWarning("MFA challenge {ChallengeId} not found for deletion", challengeId);
                return;
            }

            _context.MfaChallenges.Remove(entity);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Deleted MFA challenge {ChallengeId}", challengeId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting MFA challenge {ChallengeId}", challengeId);
            throw;
        }
    }

    public async Task<MfaChallenge?> GetActiveChallengeAsync(string userId, MfaMethod method)
    {
        try
        {
            var entity = await _context.MfaChallenges
                .Where(c => c.UserId == userId && 
                           c.MfaMethod == method.ToString() && 
                           !c.IsUsed && 
                           c.ExpiresAt > DateTime.UtcNow)
                .OrderByDescending(c => c.CreatedDate)
                .FirstOrDefaultAsync();

            if (entity == null)
                return null;

            return new MfaChallenge
            {
                ChallengeId = entity.Id,
                UserId = entity.UserId,
                Method = method.ToString(),
                Challenge = entity.ChallengeCode,
                ExpiresAt = entity.ExpiresAt,
                IsUsed = entity.IsUsed
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting active MFA challenge for user: {UserId}", userId);
            return null;
        }
    }
}
