namespace BIReportingCopilot.Core.Interfaces;

/// <summary>
/// Interface for password hashing operations
/// </summary>
public interface IPasswordHasher
{
    /// <summary>
    /// Hash a password using a secure algorithm
    /// </summary>
    /// <param name="password">Plain text password</param>
    /// <returns>Hashed password</returns>
    string HashPassword(string password);

    /// <summary>
    /// Verify a password against its hash
    /// </summary>
    /// <param name="password">Plain text password</param>
    /// <param name="hashedPassword">Hashed password to verify against</param>
    /// <returns>True if password matches, false otherwise</returns>
    bool VerifyPassword(string password, string hashedPassword);

    /// <summary>
    /// Check if a password hash needs to be rehashed (e.g., due to algorithm updates)
    /// </summary>
    /// <param name="hashedPassword">Hashed password to check</param>
    /// <returns>True if rehashing is needed, false otherwise</returns>
    bool NeedsRehash(string hashedPassword);
}
