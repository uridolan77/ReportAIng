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

    public SecureConnectionStringProvider(
        IConfiguration configuration,
        ILogger<SecureConnectionStringProvider> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<string> GetConnectionStringAsync(string name)
    {
        if (_cache.TryGetValue(name, out var cached))
            return cached;

        var connectionString = _configuration.GetConnectionString(name);

        if (string.IsNullOrEmpty(connectionString))
        {
            _logger.LogError("Connection string '{Name}' not found", name);
            throw new InvalidOperationException($"Connection string '{name}' not configured");
        }

        // Replace any remaining placeholders
        connectionString = await ReplacePlaceholdersAsync(connectionString);

        _cache[name] = connectionString;
        return connectionString;
    }

    private async Task<string> ReplacePlaceholdersAsync(string connectionString)
    {
        // Replace {azurevault:vault-name:secret-name} placeholders
        var pattern = @"\{azurevault:([^:]+):([^}]+)\}";
        var matches = System.Text.RegularExpressions.Regex.Matches(connectionString, pattern);

        foreach (System.Text.RegularExpressions.Match match in matches)
        {
            var vaultName = match.Groups[1].Value;
            var secretName = match.Groups[2].Value;

            try
            {
                var secretValue = await GetSecretFromKeyVaultAsync(vaultName, secretName);
                connectionString = connectionString.Replace(match.Value, secretValue);
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

        var secret = await client.GetSecretAsync(secretName);
        return secret.Value.Value;
    }
}
