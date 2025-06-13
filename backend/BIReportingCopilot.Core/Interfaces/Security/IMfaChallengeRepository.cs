using BIReportingCopilot.Core.Models;

namespace BIReportingCopilot.Core.Interfaces.Security;

/// <summary>
/// Repository interface for MFA challenge operations
/// </summary>
public interface IMfaChallengeRepository
{
    /// <summary>
    /// Create a new MFA challenge
    /// </summary>
    /// <param name="challenge">MFA challenge to create</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task<MfaChallenge> CreateChallengeAsync(MfaChallenge challenge, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get MFA challenge by ID
    /// </summary>
    /// <param name="challengeId">Challenge ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task<MfaChallenge?> GetChallengeAsync(string challengeId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get active challenges for a user
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task<List<MfaChallenge>> GetActiveChallengesAsync(string userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update MFA challenge
    /// </summary>
    /// <param name="challenge">Challenge to update</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task UpdateChallengeAsync(MfaChallenge challenge, CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete MFA challenge
    /// </summary>
    /// <param name="challengeId">Challenge ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task DeleteChallengeAsync(string challengeId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Clean up expired challenges
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    Task CleanupExpiredChallengesAsync(CancellationToken cancellationToken = default);

    // Methods expected by Infrastructure services
    /// <summary>
    /// Get active challenge for a user with specific method
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="method">MFA method</param>
    Task<MfaChallenge?> GetActiveChallengeAsync(string userId, MfaMethod method);

    /// <summary>
    /// Mark challenge as used
    /// </summary>
    /// <param name="challengeId">Challenge ID</param>
    Task<bool> MarkChallengeAsUsedAsync(string challengeId);
}
