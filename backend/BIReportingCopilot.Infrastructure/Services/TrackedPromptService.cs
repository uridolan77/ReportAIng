using Microsoft.Extensions.Logging;
using BIReportingCopilot.Core.Interfaces.AI;
using BIReportingCopilot.Core.Interfaces.Analytics;
using BIReportingCopilot.Core.Models;
using BIReportingCopilot.Infrastructure.AI.Management;
using System.Collections.Concurrent;

namespace BIReportingCopilot.Infrastructure.Services;

/// <summary>
/// Wrapper service that adds template usage tracking to the existing PromptService
/// </summary>
public class TrackedPromptService : IPromptService
{
    private readonly PromptService _innerPromptService;
    private readonly ITemplatePerformanceService _performanceService;
    private readonly IABTestingService _abTestingService;
    private readonly ILogger<TrackedPromptService> _logger;
    private readonly ConcurrentDictionary<string, TemplateUsageContext> _activeUsages = new();

    public TrackedPromptService(
        PromptService innerPromptService,
        ITemplatePerformanceService performanceService,
        IABTestingService abTestingService,
        ILogger<TrackedPromptService> logger)
    {
        _innerPromptService = innerPromptService;
        _performanceService = performanceService;
        _abTestingService = abTestingService;
        _logger = logger;
    }

    public async Task<string> GetPromptAsync(string promptKey, CancellationToken cancellationToken = default)
    {
        var usageId = Guid.NewGuid().ToString();
        var startTime = DateTime.UtcNow;

        try
        {
            _logger.LogDebug("🎯 [TRACKED] Getting prompt for key: {PromptKey} (Usage: {UsageId})", promptKey, usageId);

            // Track usage start
            _activeUsages[usageId] = new TemplateUsageContext
            {
                TemplateKey = promptKey,
                StartTime = startTime,
                UsageId = usageId
            };

            // Get prompt from inner service
            var prompt = await _innerPromptService.GetPromptAsync(promptKey, cancellationToken);

            // Track successful retrieval
            var processingTime = (int)(DateTime.UtcNow - startTime).TotalMilliseconds;
            await TrackTemplateRetrievalAsync(promptKey, true, processingTime, usageId);

            return prompt;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ [TRACKED] Error getting prompt for key: {PromptKey}", promptKey);
            
            // Track failed retrieval
            var processingTime = (int)(DateTime.UtcNow - startTime).TotalMilliseconds;
            await TrackTemplateRetrievalAsync(promptKey, false, processingTime, usageId);
            
            throw;
        }
        finally
        {
            _activeUsages.TryRemove(usageId, out _);
        }
    }

    public async Task<string> GetPromptAsync(string promptKey, Dictionary<string, object>? parameters = null, CancellationToken cancellationToken = default)
    {
        var usageId = Guid.NewGuid().ToString();
        var startTime = DateTime.UtcNow;

        try
        {
            _logger.LogDebug("🎯 [TRACKED] Getting prompt with parameters for key: {PromptKey} (Usage: {UsageId})", promptKey, usageId);

            // Track usage start
            _activeUsages[usageId] = new TemplateUsageContext
            {
                TemplateKey = promptKey,
                StartTime = startTime,
                UsageId = usageId,
                Parameters = parameters
            };

            // Get prompt from inner service
            var prompt = await _innerPromptService.GetPromptAsync(promptKey, parameters, cancellationToken);

            // Track successful retrieval
            var processingTime = (int)(DateTime.UtcNow - startTime).TotalMilliseconds;
            await TrackTemplateRetrievalAsync(promptKey, true, processingTime, usageId);

            return prompt;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ [TRACKED] Error getting prompt with parameters for key: {PromptKey}", promptKey);
            
            // Track failed retrieval
            var processingTime = (int)(DateTime.UtcNow - startTime).TotalMilliseconds;
            await TrackTemplateRetrievalAsync(promptKey, false, processingTime, usageId);
            
            throw;
        }
        finally
        {
            _activeUsages.TryRemove(usageId, out _);
        }
    }

    public async Task<string> FormatPromptAsync(string template, Dictionary<string, object> parameters, CancellationToken cancellationToken = default)
    {
        // This method doesn't use a specific template key, so we don't track it
        return await _innerPromptService.FormatPromptAsync(template, parameters, cancellationToken);
    }

    public async Task<string> BuildQueryPromptAsync(string naturalLanguageQuery, SchemaMetadata schema, string? context = null)
    {
        var usageId = Guid.NewGuid().ToString();
        var startTime = DateTime.UtcNow;
        var templateKey = "sql_generation"; // This is the template key used in the inner service

        try
        {
            _logger.LogDebug("🎯 [TRACKED] Building query prompt for: {Query} (Usage: {UsageId})", 
                naturalLanguageQuery.Substring(0, Math.Min(50, naturalLanguageQuery.Length)), usageId);

            // Check for A/B testing - select template variant if applicable
            var selectedTemplate = await SelectTemplateForABTestAsync(templateKey, null);
            if (selectedTemplate.IsVariant && selectedTemplate.TestId.HasValue)
            {
                _logger.LogInformation("🧪 [A/B-TEST] Using variant template for test {TestId}: {TemplateKey}", 
                    selectedTemplate.TestId, selectedTemplate.SelectedTemplateKey);
                templateKey = selectedTemplate.SelectedTemplateKey;
            }

            // Track usage start
            _activeUsages[usageId] = new TemplateUsageContext
            {
                TemplateKey = templateKey,
                StartTime = startTime,
                UsageId = usageId,
                Query = naturalLanguageQuery,
                Schema = schema,
                Context = context,
                ABTestId = selectedTemplate.TestId
            };

            // Build prompt using inner service
            var prompt = await _innerPromptService.BuildQueryPromptAsync(naturalLanguageQuery, schema, context);

            // Track successful prompt building
            var processingTime = (int)(DateTime.UtcNow - startTime).TotalMilliseconds;
            await TrackTemplateUsageAsync(templateKey, true, 0.8m, processingTime, usageId, selectedTemplate.TestId);

            return prompt;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ [TRACKED] Error building query prompt");
            
            // Track failed prompt building
            var processingTime = (int)(DateTime.UtcNow - startTime).TotalMilliseconds;
            await TrackTemplateUsageAsync(templateKey, false, 0.0m, processingTime, usageId);
            
            throw;
        }
        finally
        {
            _activeUsages.TryRemove(usageId, out _);
        }
    }

    public async Task<PromptDetails> BuildDetailedQueryPromptAsync(string naturalLanguageQuery, SchemaMetadata schema, string? context = null)
    {
        var usageId = Guid.NewGuid().ToString();
        var startTime = DateTime.UtcNow;
        var templateKey = "sql_generation";

        try
        {
            _logger.LogDebug("🎯 [TRACKED] Building detailed query prompt for: {Query} (Usage: {UsageId})", 
                naturalLanguageQuery.Substring(0, Math.Min(50, naturalLanguageQuery.Length)), usageId);

            // Check for A/B testing
            var selectedTemplate = await SelectTemplateForABTestAsync(templateKey, null);
            if (selectedTemplate.IsVariant && selectedTemplate.TestId.HasValue)
            {
                templateKey = selectedTemplate.SelectedTemplateKey;
            }

            // Track usage start
            _activeUsages[usageId] = new TemplateUsageContext
            {
                TemplateKey = templateKey,
                StartTime = startTime,
                UsageId = usageId,
                Query = naturalLanguageQuery,
                Schema = schema,
                Context = context,
                ABTestId = selectedTemplate.TestId
            };

            // Build detailed prompt using inner service
            var promptDetails = await _innerPromptService.BuildDetailedQueryPromptAsync(naturalLanguageQuery, schema, context);

            // Update prompt details with tracking information
            if (promptDetails != null)
            {
                promptDetails.TemplateKey = templateKey;
                promptDetails.UsageId = usageId;
                promptDetails.ABTestId = selectedTemplate.TestId;
            }

            // Track successful prompt building
            var processingTime = (int)(DateTime.UtcNow - startTime).TotalMilliseconds;
            await TrackTemplateUsageAsync(templateKey, true, 0.8m, processingTime, usageId, selectedTemplate.TestId);

            return promptDetails ?? new PromptDetails();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ [TRACKED] Error building detailed query prompt");
            
            // Track failed prompt building
            var processingTime = (int)(DateTime.UtcNow - startTime).TotalMilliseconds;
            await TrackTemplateUsageAsync(templateKey, false, 0.0m, processingTime, usageId);
            
            throw;
        }
        finally
        {
            _activeUsages.TryRemove(usageId, out _);
        }
    }

    /// <summary>
    /// Track the success/failure of SQL generation using a template
    /// </summary>
    public async Task TrackSqlGenerationResultAsync(string usageId, bool wasSuccessful, decimal confidenceScore, string? userId = null)
    {
        try
        {
            if (_activeUsages.TryGetValue(usageId, out var context))
            {
                _logger.LogDebug("🎯 [TRACKED] Tracking SQL generation result for usage {UsageId}: Success={Success}, Confidence={Confidence}", 
                    usageId, wasSuccessful, confidenceScore);

                var totalProcessingTime = (int)(DateTime.UtcNow - context.StartTime).TotalMilliseconds;

                // Track template performance
                await _performanceService.TrackTemplateUsageAsync(
                    context.TemplateKey, 
                    wasSuccessful, 
                    confidenceScore, 
                    totalProcessingTime, 
                    userId);

                // Track A/B test interaction if applicable
                if (context.ABTestId.HasValue)
                {
                    await _abTestingService.RecordTestInteractionAsync(
                        context.ABTestId.Value, 
                        context.TemplateKey, 
                        wasSuccessful, 
                        confidenceScore, 
                        userId);
                }

                _logger.LogInformation("✅ [TRACKED] Successfully tracked template usage: {TemplateKey}, Success: {Success}, Time: {Time}ms", 
                    context.TemplateKey, wasSuccessful, totalProcessingTime);
            }
            else
            {
                _logger.LogWarning("⚠️ [TRACKED] Usage context not found for usage ID: {UsageId}", usageId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ [TRACKED] Error tracking SQL generation result for usage {UsageId}", usageId);
        }
    }

    /// <summary>
    /// Update user rating for a template usage
    /// </summary>
    public async Task TrackUserRatingAsync(string usageId, decimal rating, string? userId = null)
    {
        try
        {
            if (_activeUsages.TryGetValue(usageId, out var context))
            {
                await _performanceService.UpdateUserRatingAsync(context.TemplateKey, rating, userId);
                _logger.LogInformation("✅ [TRACKED] User rating tracked: {TemplateKey}, Rating: {Rating}", context.TemplateKey, rating);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ [TRACKED] Error tracking user rating for usage {UsageId}", usageId);
        }
    }

    #region Private Methods

    private async Task<TemplateSelectionResult> SelectTemplateForABTestAsync(string templateKey, string? userId)
    {
        try
        {
            // For now, we'll use a simple intent type mapping
            // In a real implementation, this would be more sophisticated
            var intentType = MapTemplateKeyToIntentType(templateKey);
            
            if (!string.IsNullOrEmpty(intentType))
            {
                return await _abTestingService.SelectTemplateForUserAsync(userId ?? "anonymous", intentType);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error selecting template for A/B test, using original template: {TemplateKey}", templateKey);
        }

        // Fallback to original template
        return new TemplateSelectionResult
        {
            SelectedTemplateKey = templateKey,
            IsVariant = false,
            SelectionReason = "No A/B test active"
        };
    }

    private static string MapTemplateKeyToIntentType(string templateKey)
    {
        return templateKey switch
        {
            "sql_generation" => "sql_generation",
            "insight_generation" => "insight_generation",
            "explanation" => "explanation",
            "optimization" => "optimization",
            _ => "general"
        };
    }

    private async Task TrackTemplateRetrievalAsync(string templateKey, bool wasSuccessful, int processingTime, string usageId)
    {
        try
        {
            // Track basic template retrieval (not full usage)
            _logger.LogDebug("🎯 [TRACKED] Template retrieval: {TemplateKey}, Success: {Success}, Time: {Time}ms", 
                templateKey, wasSuccessful, processingTime);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error tracking template retrieval");
        }
    }

    private async Task TrackTemplateUsageAsync(string templateKey, bool wasSuccessful, decimal confidenceScore, int processingTime, string usageId, long? abTestId = null)
    {
        try
        {
            // Track template performance
            await _performanceService.TrackTemplateUsageAsync(templateKey, wasSuccessful, confidenceScore, processingTime);

            // Track A/B test interaction if applicable
            if (abTestId.HasValue)
            {
                await _abTestingService.RecordTestInteractionAsync(abTestId.Value, templateKey, wasSuccessful, confidenceScore);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error tracking template usage");
        }
    }

    #endregion

    #region Missing IPromptService Interface Methods

    public async Task SavePromptAsync(string promptKey, string template, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("🎯 [TRACKED] Saving prompt for key: {PromptKey}", promptKey);
            await _innerPromptService.SavePromptAsync(promptKey, template, cancellationToken);
            _logger.LogInformation("✅ [TRACKED] Prompt saved successfully: {PromptKey}", promptKey);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ [TRACKED] Error saving prompt for key: {PromptKey}", promptKey);
            throw;
        }
    }

    public async Task<List<string>> GetPromptKeysAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("🎯 [TRACKED] Getting all prompt keys");
            var keys = await _innerPromptService.GetPromptKeysAsync(cancellationToken);
            _logger.LogDebug("✅ [TRACKED] Retrieved {Count} prompt keys", keys.Count);
            return keys;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ [TRACKED] Error getting prompt keys");
            throw;
        }
    }

    public async Task<string> BuildSQLGenerationPromptAsync(string prompt, SchemaMetadata? schema = null, BIReportingCopilot.Core.Models.QueryContext? context = null)
    {
        var usageId = Guid.NewGuid().ToString();
        var startTime = DateTime.UtcNow;
        var templateKey = "sql_generation";

        try
        {
            _logger.LogDebug("🎯 [TRACKED] Building SQL generation prompt (Usage: {UsageId})", usageId);

            // Check for A/B testing
            var selectedTemplate = await SelectTemplateForABTestAsync(templateKey, null);
            if (selectedTemplate.IsVariant && selectedTemplate.TestId.HasValue)
            {
                templateKey = selectedTemplate.SelectedTemplateKey;
            }

            // Track usage start
            _activeUsages[usageId] = new TemplateUsageContext
            {
                TemplateKey = templateKey,
                StartTime = startTime,
                UsageId = usageId,
                Query = prompt,
                Schema = schema,
                Context = context?.BusinessDomain,
                ABTestId = selectedTemplate.TestId
            };

            // Build prompt using inner service
            var builtPrompt = await _innerPromptService.BuildSQLGenerationPromptAsync(prompt, schema, context);

            // Track successful prompt building
            var processingTime = (int)(DateTime.UtcNow - startTime).TotalMilliseconds;
            await TrackTemplateUsageAsync(templateKey, true, 0.8m, processingTime, usageId, selectedTemplate.TestId);

            return builtPrompt;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ [TRACKED] Error building SQL generation prompt");

            // Track failed prompt building
            var processingTime = (int)(DateTime.UtcNow - startTime).TotalMilliseconds;
            await TrackTemplateUsageAsync(templateKey, false, 0.0m, processingTime, usageId);

            throw;
        }
        finally
        {
            _activeUsages.TryRemove(usageId, out _);
        }
    }

    #endregion
}

/// <summary>
/// Context information for tracking template usage
/// </summary>
public class TemplateUsageContext
{
    public string TemplateKey { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public string UsageId { get; set; } = string.Empty;
    public string? Query { get; set; }
    public SchemaMetadata? Schema { get; set; }
    public string? Context { get; set; }
    public Dictionary<string, object>? Parameters { get; set; }
    public long? ABTestId { get; set; }
}

/// <summary>
/// Extended PromptDetails with tracking information
/// </summary>
public static class PromptDetailsExtensions
{
    public static string? TemplateKey { get; set; }
    public static string? UsageId { get; set; }
    public static long? ABTestId { get; set; }
}
