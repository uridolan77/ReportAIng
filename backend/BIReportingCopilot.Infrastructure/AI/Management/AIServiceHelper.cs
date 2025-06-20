using BIReportingCopilot.Core.Interfaces.AI;
using BIReportingCopilot.Core.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BIReportingCopilot.Infrastructure.AI.Management;

/// <summary>
/// Helper class for AI service operations and structured response generation
/// </summary>
public class AIServiceHelper
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<AIServiceHelper> _logger;

    public AIServiceHelper(
        IServiceProvider serviceProvider,
        ILogger<AIServiceHelper> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    /// <summary>
    /// Generate structured response using AI provider directly for JSON responses
    /// </summary>
    public async Task<string> GenerateStructuredResponseAsync(string prompt)
    {
        try
        {
            // Enhanced prompt with explicit JSON instructions
            var enhancedPrompt = $@"{prompt}

CRITICAL INSTRUCTIONS:
- Respond with ONLY valid JSON
- Do not include any explanatory text before or after the JSON
- Do not use markdown formatting or code blocks
- Start your response with {{ and end with }}
- Ensure all JSON strings are properly quoted
- Use double quotes for all JSON keys and string values";

            // Log the full request being sent to OpenAI
            _logger.LogInformation("🚀 SENDING TO OPENAI - Prompt Length: {Length} characters", enhancedPrompt.Length);
            _logger.LogInformation("🚀 OPENAI REQUEST PROMPT: {Prompt}", enhancedPrompt.Length > 1000 ? enhancedPrompt.Substring(0, 1000) + "... [TRUNCATED]" : enhancedPrompt);

            // Use AI provider directly for structured responses
            _logger.LogInformation("🤖 Getting AI provider for direct completion...");
            var provider = await GetAIProviderAsync();

            var options = new AIOptions
            {
                SystemMessage = "You are a business analyst. Respond with ONLY valid JSON.",
                Temperature = 0.2f,
                MaxTokens = 1000,
                TimeoutSeconds = 120  // Increased timeout to 2 minutes for complex business analysis
            };

            _logger.LogInformation("🤖 Calling AI provider GenerateCompletionAsync...");
            var response = await provider.GenerateCompletionAsync(enhancedPrompt, options);

            // Log the full response from OpenAI
            _logger.LogInformation("📥 RECEIVED FROM OPENAI - Response Length: {Length} characters", response?.Length ?? 0);
            _logger.LogInformation("📥 OPENAI RESPONSE: '{Response}'", response ?? "[NULL RESPONSE]");

            return response ?? string.Empty;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error in GenerateStructuredResponseAsync");
            throw;
        }
    }

    /// <summary>
    /// Get AI provider for direct completion calls
    /// </summary>
    private async Task<IAIProvider> GetAIProviderAsync()
    {
        // Use the AI provider factory to get the configured provider
        var providerFactory = _serviceProvider.GetRequiredService<IAIProviderFactory>();
        var supportedProviders = providerFactory.GetSupportedProviders();

        if (supportedProviders.Any())
        {
            var primaryProvider = supportedProviders.First();
            _logger.LogDebug("🔧 Using AI provider: {Provider}", primaryProvider);
            return await providerFactory.CreateProviderAsync(primaryProvider);
        }

        throw new InvalidOperationException("No AI providers are configured");
    }
}
