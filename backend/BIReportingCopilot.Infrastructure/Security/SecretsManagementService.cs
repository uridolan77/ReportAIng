using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Azure.Security.KeyVault.Secrets;
using Azure.Identity;
using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace BIReportingCopilot.Infrastructure.Security;

/// <summary>
/// Comprehensive secrets management service with Azure Key Vault integration and local encryption
/// </summary>
public class SecretsManagementService : ISecretsManagementService
{
    private readonly ILogger<SecretsManagementService> _logger;
    private readonly SecretsConfiguration _config;
    private readonly SecretClient? _keyVaultClient;
    private readonly ConcurrentDictionary<string, CachedSecret> _secretCache;
    private readonly SemaphoreSlim _refreshSemaphore;

    public SecretsManagementService(
        ILogger<SecretsManagementService> logger,
        IOptions<SecretsConfiguration> config)
    {
        _logger = logger;
        _config = config.Value;
        _secretCache = new ConcurrentDictionary<string, CachedSecret>();
        _refreshSemaphore = new SemaphoreSlim(1, 1);

        // Initialize Azure Key Vault client if configured
        if (!string.IsNullOrEmpty(_config.KeyVaultUrl))
        {
            try
            {
                var keyVaultUri = new Uri(_config.KeyVaultUrl);
                var credential = new DefaultAzureCredential();
                _keyVaultClient = new SecretClient(keyVaultUri, credential);
                _logger.LogInformation("Azure Key Vault client initialized: {KeyVaultUrl}", _config.KeyVaultUrl);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initialize Azure Key Vault client");
            }
        }
    }

    public async Task<string?> GetSecretAsync(string secretName, CancellationToken cancellationToken = default)
    {
        try
        {
            // Check cache first
            if (_secretCache.TryGetValue(secretName, out var cachedSecret) && !cachedSecret.IsExpired)
            {
                return cachedSecret.Value;
            }

            // Try to get from Key Vault
            if (_keyVaultClient != null)
            {
                try
                {
                    var response = await _keyVaultClient.GetSecretAsync(secretName, cancellationToken: cancellationToken);
                    var secretValue = response.Value.Value;

                    // Cache the secret
                    _secretCache[secretName] = new CachedSecret
                    {
                        Value = secretValue,
                        ExpiresAt = DateTime.UtcNow.AddMinutes(_config.CacheExpiryMinutes)
                    };

                    _logger.LogDebug("Retrieved secret {SecretName} from Key Vault", secretName);
                    return secretValue;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to retrieve secret {SecretName} from Key Vault", secretName);
                }
            }

            // Fallback to environment variables or configuration
            var envValue = Environment.GetEnvironmentVariable(secretName);
            if (!string.IsNullOrEmpty(envValue))
            {
                _logger.LogDebug("Retrieved secret {SecretName} from environment variable", secretName);
                return envValue;
            }

            _logger.LogWarning("Secret {SecretName} not found in any source", secretName);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving secret {SecretName}", secretName);
            return null;
        }
    }

    public async Task<bool> SetSecretAsync(string secretName, string secretValue, CancellationToken cancellationToken = default)
    {
        try
        {
            if (_keyVaultClient != null)
            {
                await _keyVaultClient.SetSecretAsync(secretName, secretValue, cancellationToken);
                
                // Update cache
                _secretCache[secretName] = new CachedSecret
                {
                    Value = secretValue,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(_config.CacheExpiryMinutes)
                };

                _logger.LogInformation("Secret {SecretName} stored in Key Vault", secretName);
                return true;
            }

            _logger.LogWarning("Key Vault client not available, cannot store secret {SecretName}", secretName);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error storing secret {SecretName}", secretName);
            return false;
        }
    }

    public async Task<bool> DeleteSecretAsync(string secretName, CancellationToken cancellationToken = default)
    {
        try
        {
            if (_keyVaultClient != null)
            {
                await _keyVaultClient.StartDeleteSecretAsync(secretName, cancellationToken);
                
                // Remove from cache
                _secretCache.TryRemove(secretName, out _);

                _logger.LogInformation("Secret {SecretName} deleted from Key Vault", secretName);
                return true;
            }

            _logger.LogWarning("Key Vault client not available, cannot delete secret {SecretName}", secretName);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting secret {SecretName}", secretName);
            return false;
        }
    }

    public async Task<Dictionary<string, string>> GetSecretsAsync(IEnumerable<string> secretNames, CancellationToken cancellationToken = default)
    {
        var results = new Dictionary<string, string>();
        var tasks = secretNames.Select(async name =>
        {
            var value = await GetSecretAsync(name, cancellationToken);
            if (value != null)
            {
                results[name] = value;
            }
        });

        await Task.WhenAll(tasks);
        return results;
    }

    public string EncryptString(string plainText, string? key = null)
    {
        try
        {
            var encryptionKey = key ?? _config.DefaultEncryptionKey ?? GenerateEncryptionKey();
            
            using var aes = Aes.Create();
            aes.Key = Convert.FromBase64String(encryptionKey);
            aes.GenerateIV();

            using var encryptor = aes.CreateEncryptor();
            var plainBytes = Encoding.UTF8.GetBytes(plainText);
            var encryptedBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);

            // Combine IV and encrypted data
            var result = new byte[aes.IV.Length + encryptedBytes.Length];
            Array.Copy(aes.IV, 0, result, 0, aes.IV.Length);
            Array.Copy(encryptedBytes, 0, result, aes.IV.Length, encryptedBytes.Length);

            return Convert.ToBase64String(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error encrypting string");
            throw;
        }
    }

    public string DecryptString(string encryptedText, string? key = null)
    {
        try
        {
            var encryptionKey = key ?? _config.DefaultEncryptionKey ?? throw new InvalidOperationException("No encryption key available");
            var encryptedData = Convert.FromBase64String(encryptedText);

            using var aes = Aes.Create();
            aes.Key = Convert.FromBase64String(encryptionKey);

            // Extract IV from the beginning of the encrypted data
            var iv = new byte[aes.IV.Length];
            var encryptedBytes = new byte[encryptedData.Length - iv.Length];
            
            Array.Copy(encryptedData, 0, iv, 0, iv.Length);
            Array.Copy(encryptedData, iv.Length, encryptedBytes, 0, encryptedBytes.Length);
            
            aes.IV = iv;

            using var decryptor = aes.CreateDecryptor();
            var decryptedBytes = decryptor.TransformFinalBlock(encryptedBytes, 0, encryptedBytes.Length);

            return Encoding.UTF8.GetString(decryptedBytes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error decrypting string");
            throw;
        }
    }

    public async Task RefreshSecretsAsync(CancellationToken cancellationToken = default)
    {
        await _refreshSemaphore.WaitAsync(cancellationToken);
        try
        {
            _logger.LogInformation("Refreshing cached secrets");

            var expiredSecrets = _secretCache
                .Where(kvp => kvp.Value.IsExpired)
                .Select(kvp => kvp.Key)
                .ToList();

            foreach (var secretName in expiredSecrets)
            {
                try
                {
                    var newValue = await GetSecretAsync(secretName, cancellationToken);
                    if (newValue != null)
                    {
                        _secretCache[secretName] = new CachedSecret
                        {
                            Value = newValue,
                            ExpiresAt = DateTime.UtcNow.AddMinutes(_config.CacheExpiryMinutes)
                        };
                    }
                    else
                    {
                        _secretCache.TryRemove(secretName, out _);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to refresh secret {SecretName}", secretName);
                }
            }

            _logger.LogInformation("Refreshed {Count} secrets", expiredSecrets.Count);
        }
        finally
        {
            _refreshSemaphore.Release();
        }
    }

    public async Task<SecretsHealthStatus> GetHealthStatusAsync(CancellationToken cancellationToken = default)
    {
        var status = new SecretsHealthStatus
        {
            IsHealthy = true,
            CheckedAt = DateTime.UtcNow,
            CachedSecretsCount = _secretCache.Count,
            ExpiredSecretsCount = _secretCache.Values.Count(s => s.IsExpired)
        };

        // Test Key Vault connectivity
        if (_keyVaultClient != null)
        {
            try
            {
                // Try to get a test secret or list secrets (limited operation)
                var testSecretName = "health-check-test";
                await _keyVaultClient.GetSecretAsync(testSecretName, cancellationToken: cancellationToken);
                status.KeyVaultConnectivity = true;
            }
            catch (Azure.RequestFailedException ex) when (ex.Status == 404)
            {
                // Secret not found is OK - means we can connect
                status.KeyVaultConnectivity = true;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Key Vault health check failed");
                status.KeyVaultConnectivity = false;
                status.IsHealthy = false;
                status.ErrorMessage = ex.Message;
            }
        }
        else
        {
            status.KeyVaultConnectivity = false;
        }

        return status;
    }

    private string GenerateEncryptionKey()
    {
        using var aes = Aes.Create();
        aes.GenerateKey();
        return Convert.ToBase64String(aes.Key);
    }

    private class CachedSecret
    {
        public string Value { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
        public bool IsExpired => DateTime.UtcNow > ExpiresAt;
    }
}

/// <summary>
/// Interface for secrets management service
/// </summary>
public interface ISecretsManagementService
{
    Task<string?> GetSecretAsync(string secretName, CancellationToken cancellationToken = default);
    Task<bool> SetSecretAsync(string secretName, string secretValue, CancellationToken cancellationToken = default);
    Task<bool> DeleteSecretAsync(string secretName, CancellationToken cancellationToken = default);
    Task<Dictionary<string, string>> GetSecretsAsync(IEnumerable<string> secretNames, CancellationToken cancellationToken = default);
    string EncryptString(string plainText, string? key = null);
    string DecryptString(string encryptedText, string? key = null);
    Task RefreshSecretsAsync(CancellationToken cancellationToken = default);
    Task<SecretsHealthStatus> GetHealthStatusAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Configuration for secrets management
/// </summary>
public class SecretsConfiguration
{
    public string? KeyVaultUrl { get; set; }
    public string? DefaultEncryptionKey { get; set; }
    public int CacheExpiryMinutes { get; set; } = 60;
    public bool EnableCaching { get; set; } = true;
    public List<string> RequiredSecrets { get; set; } = new();
}

/// <summary>
/// Health status for secrets management
/// </summary>
public class SecretsHealthStatus
{
    public bool IsHealthy { get; set; }
    public DateTime CheckedAt { get; set; }
    public bool KeyVaultConnectivity { get; set; }
    public int CachedSecretsCount { get; set; }
    public int ExpiredSecretsCount { get; set; }
    public string? ErrorMessage { get; set; }
}
