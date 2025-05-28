using System.ComponentModel.DataAnnotations;

namespace BIReportingCopilot.Core.Configuration;

public class OpenAIConfiguration
{
    public const string SectionName = "OpenAI";

    [Required]
    public string ApiKey { get; set; } = string.Empty;

    public string Endpoint { get; set; } = "https://api.openai.com/v1";

    [Required]
    public string Model { get; set; } = "gpt-4";

    [Range(1, 4000)]
    public int MaxTokens { get; set; } = 2000;

    [Range(0.0, 2.0)]
    public float Temperature { get; set; } = 0.1f;

    [Range(1, 10)]
    public int MaxRetries { get; set; } = 3;

    [Range(1, 300)]
    public int TimeoutSeconds { get; set; } = 30;

    [Range(0.0, 2.0)]
    public float FrequencyPenalty { get; set; } = 0.0f;

    [Range(0.0, 2.0)]
    public float PresencePenalty { get; set; } = 0.0f;

    public bool IsConfigured => !string.IsNullOrEmpty(ApiKey) && ApiKey != "your-openai-api-key-here";
}

public class AzureOpenAIConfiguration
{
    public const string SectionName = "AzureOpenAI";

    public string Endpoint { get; set; } = string.Empty;

    public string ApiKey { get; set; } = string.Empty;

    [Required]
    public string DeploymentName { get; set; } = "gpt-4";

    public string ApiVersion { get; set; } = "2024-02-15-preview";

    [Range(1, 4000)]
    public int MaxTokens { get; set; } = 2000;

    [Range(0.0, 2.0)]
    public float Temperature { get; set; } = 0.1f;

    [Range(1, 10)]
    public int MaxRetries { get; set; } = 3;

    [Range(1, 300)]
    public int TimeoutSeconds { get; set; } = 30;

    public bool IsConfigured => !string.IsNullOrEmpty(Endpoint) && !string.IsNullOrEmpty(ApiKey);
}

public class AIServiceConfiguration
{
    public OpenAIConfiguration OpenAI { get; set; } = new();
    public AzureOpenAIConfiguration AzureOpenAI { get; set; } = new();

    public bool HasValidConfiguration => AzureOpenAI.IsConfigured || OpenAI.IsConfigured;

    public bool PreferAzureOpenAI => AzureOpenAI.IsConfigured;
}
