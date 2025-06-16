using Azure.Core;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace BIReportingCopilot.Infrastructure.Configuration;

public static class KeyVaultConfigurationExtensions
{
    public static IConfigurationBuilder AddAzureKeyVault(
        this IConfigurationBuilder builder,
        string keyVaultUrl,
        bool useEnvironmentCredential = true)
    {
        if (string.IsNullOrEmpty(keyVaultUrl))
            return builder;

        var credential = useEnvironmentCredential
            ? (TokenCredential)new DefaultAzureCredential()
            : new ManagedIdentityCredential();

        var client = new SecretClient(new Uri(keyVaultUrl), credential);
        builder.AddAzureKeyVault(client, new KeyVaultSecretManager());

        return builder;
    }
}

public class KeyVaultSecretManager : Azure.Extensions.AspNetCore.Configuration.Secrets.KeyVaultSecretManager
{
    private const string KeyVaultSeparator = "--";
    private const string ConfigSeparator = ":";

    public override string GetKey(KeyVaultSecret secret)
    {
        // Transform Key Vault secret names to configuration keys
        // e.g., "ConnectionStrings--DefaultConnection" -> "ConnectionStrings:DefaultConnection"
        return secret.Name.Replace(KeyVaultSeparator, ConfigSeparator);
    }

    public override bool Load(SecretProperties properties)
    {
        // Only load enabled secrets
        return properties.Enabled ?? false;
    }
}

// Secure connection string provider
public interface IConnectionStringProvider
{
    Task<string> GetConnectionStringAsync(string name);
}

public class SecureConnectionStringProvider : IConnectionStringProvider
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<SecureConnectionStringProvider> _logger;
    private readonly Dictionary<string, string> _cache = new();
    private readonly SemaphoreSlim _semaphore = new(1, 1);
    private static readonly Dictionary<string, (string Value, DateTime CachedAt)> _globalCache = new();
    private static readonly TimeSpan _cacheExpiry = TimeSpan.FromMinutes(5);

    public SecureConnectionStringProvider(
        IConfiguration configuration,
        ILogger<SecureConnectionStringProvider> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<string> GetConnectionStringAsync(string name)
    {
        _logger.LogDebug("Getting connection string for '{Name}'", name);

        // Check local cache first
        if (_cache.TryGetValue(name, out var cached))
        {
            _logger.LogDebug("Using cached connection string for '{Name}'", name);
            return cached;
        }

        // Check global cache with expiry
        var cacheKey = $"{name}";
        if (_globalCache.TryGetValue(cacheKey, out var globalCached) && 
            DateTime.UtcNow - globalCached.CachedAt < _cacheExpiry)
        {
            _logger.LogDebug("Using global cached connection string for '{Name}' (age: {Age})", 
                name, DateTime.UtcNow - globalCached.CachedAt);
            _cache[name] = globalCached.Value;
            return globalCached.Value;
        }

        await _semaphore.WaitAsync();
        try
        {
            // Double-check after acquiring lock
            if (_cache.TryGetValue(name, out cached))
            {
                return cached;
            }

            var connectionString = _configuration.GetConnectionString(name);

            if (string.IsNullOrEmpty(connectionString))
            {
                _logger.LogError("Connection string '{Name}' not found", name);
                throw new InvalidOperationException($"Connection string '{name}' not configured");
            }

            // Replace any remaining placeholders
            connectionString = await ReplacePlaceholdersAsync(connectionString);

            // Log connection string details (without sensitive data)
            try
            {
                var builder = new Microsoft.Data.SqlClient.SqlConnectionStringBuilder(connectionString);
                _logger.LogInformation("Connection string resolved for '{Name}' - Server: {Server}, Database: {Database}, Length: {Length}",
                    name, builder.DataSource, builder.InitialCatalog, connectionString.Length);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not parse connection string for '{Name}', Length: {Length}", name, connectionString.Length);
            }

            // Cache both locally and globally
            _cache[name] = connectionString;
            _globalCache[cacheKey] = (connectionString, DateTime.UtcNow);
            
            return connectionString;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    private async Task<string> ReplacePlaceholdersAsync(string connectionString)
    {
        // Replace {azurevault:vault-name:secret-name} placeholders
        var pattern = @"\{azurevault:([^:]+):([^}]+)\}";
        var matches = System.Text.RegularExpressions.Regex.Matches(connectionString, pattern);

        if (matches.Count > 0)
        {
            _logger.LogInformation("Resolving {MatchCount} Azure Key Vault placeholders", matches.Count);
        }

        foreach (System.Text.RegularExpressions.Match match in matches)
        {
            var vaultName = match.Groups[1].Value;
            var secretName = match.Groups[2].Value;

            try
            {
                var secretValue = await GetSecretFromKeyVaultAsync(vaultName, secretName);
                connectionString = connectionString.Replace(match.Value, secretValue);
                _logger.LogDebug("Successfully replaced placeholder for secret '{SecretName}'", secretName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve secret '{SecretName}' from vault '{VaultName}'",
                    secretName, vaultName);
                throw;
            }
        }

        return connectionString;
    }

    private async Task<string> GetSecretFromKeyVaultAsync(string vaultName, string secretName)
    {
        var keyVaultUrl = $"https://{vaultName}.vault.azure.net/";
        var client = new SecretClient(new Uri(keyVaultUrl), new DefaultAzureCredential());

        try
        {
            var secret = await client.GetSecretAsync(secretName);
            _logger.LogDebug("Successfully retrieved secret '{SecretName}' from Key Vault", secretName);
            return secret.Value.Value;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve secret '{SecretName}' from Key Vault '{KeyVaultUrl}'",
                secretName, keyVaultUrl);
            throw;
        }
    }
}
