# OpenAI Service Configuration Guide

This guide explains how to configure the OpenAI service for the BI Reporting Copilot application.

## Configuration Options

The application supports both **Azure OpenAI** and **OpenAI** services. Azure OpenAI is preferred when both are configured.

### Azure OpenAI Configuration (Recommended)

Azure OpenAI provides enterprise-grade security, compliance, and regional data residency.

```json
{
  "AzureOpenAI": {
    "Endpoint": "https://your-resource-name.openai.azure.com/",
    "ApiKey": "your-azure-openai-api-key",
    "DeploymentName": "gpt-4",
    "ApiVersion": "2024-02-15-preview",
    "MaxTokens": 2000,
    "Temperature": 0.1,
    "MaxRetries": 3,
    "TimeoutSeconds": 30
  }
}
```

### OpenAI Configuration

Standard OpenAI API configuration:

```json
{
  "OpenAI": {
    "ApiKey": "sk-your-openai-api-key",
    "Endpoint": "https://api.openai.com/v1",
    "Model": "gpt-4",
    "MaxTokens": 2000,
    "Temperature": 0.1,
    "MaxRetries": 3,
    "TimeoutSeconds": 30,
    "FrequencyPenalty": 0.0,
    "PresencePenalty": 0.0
  }
}
```

## Configuration Methods

### 1. User Secrets (Development)

For development, use user secrets to store API keys securely:

```bash
cd backend/BIReportingCopilot.API
dotnet user-secrets set "AzureOpenAI:ApiKey" "your-api-key"
dotnet user-secrets set "AzureOpenAI:Endpoint" "https://your-resource.openai.azure.com/"
```

### 2. Environment Variables

Set environment variables for production:

```bash
export AzureOpenAI__ApiKey="your-api-key"
export AzureOpenAI__Endpoint="https://your-resource.openai.azure.com/"
export AzureOpenAI__DeploymentName="gpt-4"
```

### 3. Azure Key Vault (Production)

Configure Key Vault URL in appsettings:

```json
{
  "KeyVault": {
    "Url": "https://your-keyvault.vault.azure.net/"
  }
}
```

Store secrets in Key Vault:
- `AzureOpenAI--ApiKey`
- `AzureOpenAI--Endpoint`

## Configuration Parameters

| Parameter | Description | Default | Range |
|-----------|-------------|---------|-------|
| `ApiKey` | API key for authentication | Required | - |
| `Endpoint` | Service endpoint URL | Required | - |
| `DeploymentName/Model` | Model deployment name | "gpt-4" | - |
| `MaxTokens` | Maximum tokens per request | 2000 | 1-4000 |
| `Temperature` | Response randomness | 0.1 | 0.0-2.0 |
| `MaxRetries` | Retry attempts on failure | 3 | 1-10 |
| `TimeoutSeconds` | Request timeout | 30 | 1-300 |

## Fallback Behavior

When OpenAI services are not configured or unavailable, the application provides intelligent fallback responses:

- **SQL Generation**: Pattern-based SQL templates
- **Insights**: Rule-based analysis of query results
- **Visualizations**: Default chart configurations

## Health Checks

The application includes health checks for OpenAI services:

- **Endpoint**: `/health`
- **Status**: Shows configuration type (Azure OpenAI/OpenAI)
- **Validation**: Tests service connectivity and configuration

## Troubleshooting

### Common Issues

1. **"OpenAI API key not configured"**
   - Verify API key is set in configuration
   - Check user secrets or environment variables

2. **"Request failed with status 401"**
   - Verify API key is correct and active
   - Check endpoint URL format

3. **"Request timed out"**
   - Increase `TimeoutSeconds` value
   - Check network connectivity

4. **"Deployment not found"**
   - Verify `DeploymentName` matches Azure OpenAI deployment
   - Check deployment status in Azure portal

### Logging

Enable debug logging to troubleshoot issues:

```json
{
  "Logging": {
    "LogLevel": {
      "BIReportingCopilot.Infrastructure.AI": "Debug"
    }
  }
}
```

## Security Best Practices

1. **Never commit API keys** to source control
2. **Use Azure Key Vault** for production secrets
3. **Rotate API keys** regularly
4. **Monitor usage** and set up alerts
5. **Use managed identities** when possible
6. **Implement rate limiting** to prevent abuse

## Testing Configuration

Test your configuration using the health check endpoint:

```bash
curl https://your-app-url/health
```

Or use the built-in test methods in the application.
