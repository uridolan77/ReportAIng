using System.Security.Cryptography;
using System.Text;
using BIReportingCopilot.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace BIReportingCopilot.Infrastructure.Security;

/// <summary>
/// Secure password hasher using PBKDF2 with SHA-256
/// </summary>
public class PasswordHasher : IPasswordHasher
{
    private readonly ILogger<PasswordHasher> _logger;
    private const int SaltSize = 32; // 256 bits
    private const int HashSize = 32; // 256 bits
    private const int Iterations = 100000; // OWASP recommended minimum

    public PasswordHasher(ILogger<PasswordHasher> logger)
    {
        _logger = logger;
    }

    public string HashPassword(string password)
    {
        if (string.IsNullOrEmpty(password))
            throw new ArgumentException("Password cannot be null or empty", nameof(password));

        try
        {
            // Generate a random salt
            using var rng = RandomNumberGenerator.Create();
            var salt = new byte[SaltSize];
            rng.GetBytes(salt);

            // Hash the password with the salt
            using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Iterations, HashAlgorithmName.SHA256);
            var hash = pbkdf2.GetBytes(HashSize);

            // Combine salt and hash for storage
            var hashBytes = new byte[SaltSize + HashSize];
            Array.Copy(salt, 0, hashBytes, 0, SaltSize);
            Array.Copy(hash, 0, hashBytes, SaltSize, HashSize);

            // Return as base64 string with iteration count prefix
            return $"{Iterations}:{Convert.ToBase64String(hashBytes)}";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error hashing password");
            throw;
        }
    }

    public bool VerifyPassword(string password, string hashedPassword)
    {
        if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(hashedPassword))
            return false;

        try
        {
            // Parse the stored hash
            var parts = hashedPassword.Split(':');
            if (parts.Length != 2)
                return false;

            if (!int.TryParse(parts[0], out var iterations))
                return false;

            var hashBytes = Convert.FromBase64String(parts[1]);
            if (hashBytes.Length != SaltSize + HashSize)
                return false;

            // Extract salt and hash
            var salt = new byte[SaltSize];
            var hash = new byte[HashSize];
            Array.Copy(hashBytes, 0, salt, 0, SaltSize);
            Array.Copy(hashBytes, SaltSize, hash, 0, HashSize);

            // Hash the provided password with the same salt
            using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, iterations, HashAlgorithmName.SHA256);
            var testHash = pbkdf2.GetBytes(HashSize);

            // Compare hashes using constant-time comparison
            return CryptographicOperations.FixedTimeEquals(hash, testHash);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error verifying password");
            return false;
        }
    }

    public bool NeedsRehash(string hashedPassword)
    {
        if (string.IsNullOrEmpty(hashedPassword))
            return true;

        try
        {
            var parts = hashedPassword.Split(':');
            if (parts.Length != 2)
                return true;

            if (!int.TryParse(parts[0], out var iterations))
                return true;

            // Rehash if using fewer iterations than current standard
            return iterations < Iterations;
        }
        catch
        {
            return true;
        }
    }
}
