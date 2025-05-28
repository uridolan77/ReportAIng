using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Azure.Security.KeyVault.Secrets;

namespace BIReportingCopilot.API.Configuration;

/// <summary>
/// Custom secret manager that excludes JWT-related secrets from Azure Key Vault
/// to prevent overriding local JWT configuration
/// </summary>
public class SelectiveSecretManager : KeyVaultSecretManager
{
    public override bool Load(SecretProperties secret)
    {
        // Exclude JWT-related secrets to prevent overriding local configuration
        var secretName = secret.Name.ToLowerInvariant();

        // Skip JWT-related secrets
        if (secretName.Contains("jwt") ||
            secretName.Contains("jwtsettings") ||
            secretName.StartsWith("jwt-") ||
            secretName.EndsWith("-jwt"))
        {
            return false;
        }

        // Load all other secrets (including database credentials)
        return base.Load(secret);
    }

    public override string GetKey(KeyVaultSecret secret)
    {
        return base.GetKey(secret);
    }
}
