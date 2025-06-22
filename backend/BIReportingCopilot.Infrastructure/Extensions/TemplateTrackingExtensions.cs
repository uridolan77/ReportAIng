using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using BIReportingCopilot.Core.Interfaces.Analytics;
using BIReportingCopilot.Core.Models;
using BIReportingCopilot.Infrastructure.Services;

namespace BIReportingCopilot.Infrastructure.Extensions;

/// <summary>
/// Extension methods for template tracking integration
/// </summary>
public static class TemplateTrackingExtensions
{
    /// <summary>
    /// Track template usage result from SQL generation
    /// </summary>
    public static async Task TrackTemplateUsageFromSqlResultAsync(
        this IServiceProvider serviceProvider,
        GenerateSqlResponse sqlResult,
        string? userId = null)
    {
        try
        {
            var trackedPromptService = serviceProvider.GetService<TrackedPromptService>();
            if (trackedPromptService != null && sqlResult.PromptDetails?.UsageId != null)
            {
                await trackedPromptService.TrackSqlGenerationResultAsync(
                    sqlResult.PromptDetails.UsageId,
                    sqlResult.Success,
                    sqlResult.Confidence,
                    userId);
            }
        }
        catch (Exception ex)
        {
            var logger = serviceProvider.GetService<ILogger<TemplateTrackingExtensions>>();
            logger?.LogError(ex, "Error tracking template usage from SQL result");
        }
    }

    /// <summary>
    /// Track template usage result from query response
    /// </summary>
    public static async Task TrackTemplateUsageFromQueryResponseAsync(
        this IServiceProvider serviceProvider,
        QueryResponse queryResponse,
        PromptDetails? promptDetails,
        string? userId = null)
    {
        try
        {
            var trackedPromptService = serviceProvider.GetService<TrackedPromptService>();
            if (trackedPromptService != null && promptDetails?.UsageId != null)
            {
                // Calculate confidence based on query success and result quality
                var confidence = CalculateQueryConfidence(queryResponse);

                await trackedPromptService.TrackSqlGenerationResultAsync(
                    promptDetails.UsageId,
                    queryResponse.IsSuccessful,
                    confidence,
                    userId);
            }
        }
        catch (Exception ex)
        {
            var logger = serviceProvider.GetService<ILogger<TemplateTrackingExtensions>>();
            logger?.LogError(ex, "Error tracking template usage from query response");
        }
    }

    /// <summary>
    /// Track user feedback for template
    /// </summary>
    public static async Task TrackTemplateUserFeedbackAsync(
        this IServiceProvider serviceProvider,
        string usageId,
        decimal rating,
        string? userId = null)
    {
        try
        {
            var trackedPromptService = serviceProvider.GetService<TrackedPromptService>();
            if (trackedPromptService != null)
            {
                await trackedPromptService.TrackUserRatingAsync(usageId, rating, userId);
            }
        }
        catch (Exception ex)
        {
            var logger = serviceProvider.GetService<ILogger<TemplateTrackingExtensions>>();
            logger?.LogError(ex, "Error tracking template user feedback");
        }
    }

    /// <summary>
    /// Get template performance metrics for dashboard
    /// </summary>
    public static async Task<TemplatePerformanceMetrics?> GetTemplatePerformanceAsync(
        this IServiceProvider serviceProvider,
        string templateKey)
    {
        try
        {
            var performanceService = serviceProvider.GetService<ITemplatePerformanceService>();
            if (performanceService != null)
            {
                return await performanceService.GetTemplatePerformanceAsync(templateKey);
            }
        }
        catch (Exception ex)
        {
            var logger = serviceProvider.GetService<ILogger<TemplateTrackingExtensions>>();
            logger?.LogError(ex, "Error getting template performance for {TemplateKey}", templateKey);
        }

        return null;
    }

    /// <summary>
    /// Get A/B testing recommendations for templates
    /// </summary>
    public static async Task<List<ABTestRecommendation>> GetABTestRecommendationsAsync(
        this IServiceProvider serviceProvider)
    {
        try
        {
            var abTestingService = serviceProvider.GetService<IABTestingService>();
            if (abTestingService != null)
            {
                return await abTestingService.GetTestRecommendationsAsync();
            }
        }
        catch (Exception ex)
        {
            var logger = serviceProvider.GetService<ILogger<TemplateTrackingExtensions>>();
            logger?.LogError(ex, "Error getting A/B test recommendations");
        }

        return new List<ABTestRecommendation>();
    }

    /// <summary>
    /// Create A/B test for underperforming template
    /// </summary>
    public static async Task<ABTestResult?> CreateABTestForTemplateAsync(
        this IServiceProvider serviceProvider,
        string templateKey,
        string variantContent,
        string createdBy,
        string testName)
    {
        try
        {
            var abTestingService = serviceProvider.GetService<IABTestingService>();
            if (abTestingService != null)
            {
                var request = new ABTestRequest
                {
                    TestName = testName,
                    OriginalTemplateKey = templateKey,
                    VariantTemplateContent = variantContent,
                    TrafficSplitPercent = 50,
                    StartDate = DateTime.UtcNow,
                    EndDate = DateTime.UtcNow.AddDays(30),
                    MinimumSampleSize = 100,
                    SignificanceLevel = 0.05m,
                    CreatedBy = createdBy,
                    MetricsToTrack = new List<string> { "success_rate", "confidence_score", "processing_time" }
                };

                return await abTestingService.CreateABTestAsync(request);
            }
        }
        catch (Exception ex)
        {
            var logger = serviceProvider.GetService<ILogger<TemplateTrackingExtensions>>();
            logger?.LogError(ex, "Error creating A/B test for template {TemplateKey}", templateKey);
        }

        return null;
    }

    /// <summary>
    /// Get template improvement suggestions
    /// </summary>
    public static async Task<List<TemplateRecommendation>> GetTemplateImprovementSuggestionsAsync(
        this IServiceProvider serviceProvider,
        string templateKey)
    {
        try
        {
            var managementService = serviceProvider.GetService<ITemplateManagementService>();
            if (managementService != null)
            {
                return await managementService.GetTemplateRecommendationsAsync(templateKey);
            }
        }
        catch (Exception ex)
        {
            var logger = serviceProvider.GetService<ILogger<TemplateTrackingExtensions>>();
            logger?.LogError(ex, "Error getting template improvement suggestions for {TemplateKey}", templateKey);
        }

        return new List<TemplateRecommendation>();
    }

    /// <summary>
    /// Track template selection for analytics
    /// </summary>
    public static async Task TrackTemplateSelectionAsync(
        this IServiceProvider serviceProvider,
        string templateKey,
        string intentType,
        string? userId = null,
        Dictionary<string, object>? selectionMetadata = null)
    {
        try
        {
            var performanceService = serviceProvider.GetService<ITemplatePerformanceService>();
            if (performanceService != null)
            {
                // Track template selection as a lightweight usage event
                await performanceService.TrackTemplateUsageAsync(
                    templateKey,
                    true, // Selection is always successful
                    1.0m, // Full confidence for selection
                    0,    // No processing time for selection
                    userId);
            }
        }
        catch (Exception ex)
        {
            var logger = serviceProvider.GetService<ILogger<TemplateTrackingExtensions>>();
            logger?.LogError(ex, "Error tracking template selection for {TemplateKey}", templateKey);
        }
    }

    /// <summary>
    /// Get template health status for monitoring
    /// </summary>
    public static async Task<TemplateHealthStatus> GetTemplateHealthStatusAsync(
        this IServiceProvider serviceProvider,
        string templateKey)
    {
        try
        {
            var managementService = serviceProvider.GetService<ITemplateManagementService>();
            if (managementService != null)
            {
                var templateWithMetrics = await managementService.GetTemplateAsync(templateKey);
                return templateWithMetrics?.HealthStatus ?? TemplateHealthStatus.Fair;
            }
        }
        catch (Exception ex)
        {
            var logger = serviceProvider.GetService<ILogger<TemplateTrackingExtensions>>();
            logger?.LogError(ex, "Error getting template health status for {TemplateKey}", templateKey);
        }

        return TemplateHealthStatus.Fair;
    }

    /// <summary>
    /// Get performance alerts for templates
    /// </summary>
    public static async Task<List<PerformanceAlert>> GetTemplatePerformanceAlertsAsync(
        this IServiceProvider serviceProvider)
    {
        try
        {
            var performanceService = serviceProvider.GetService<ITemplatePerformanceService>();
            if (performanceService != null)
            {
                return await performanceService.GetPerformanceAlertsAsync();
            }
        }
        catch (Exception ex)
        {
            var logger = serviceProvider.GetService<ILogger<TemplateTrackingExtensions>>();
            logger?.LogError(ex, "Error getting template performance alerts");
        }

        return new List<PerformanceAlert>();
    }

    #region Private Helper Methods

    private static decimal CalculateQueryConfidence(QueryResponse queryResponse)
    {
        if (!queryResponse.IsSuccessful)
        {
            return 0.0m;
        }

        var confidence = 0.5m; // Base confidence

        // Increase confidence based on result quality
        if (queryResponse.Data?.Any() == true)
        {
            confidence += 0.3m; // Has data
        }

        if (queryResponse.Metadata?.RowCount > 0)
        {
            confidence += 0.1m; // Has rows
        }

        if (string.IsNullOrEmpty(queryResponse.Metadata?.Error))
        {
            confidence += 0.1m; // No errors
        }

        return Math.Min(1.0m, confidence);
    }

    #endregion
}

/// <summary>
/// Template tracking middleware for automatic usage tracking
/// </summary>
public class TemplateTrackingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<TemplateTrackingMiddleware> _logger;

    public TemplateTrackingMiddleware(RequestDelegate next, ILogger<TemplateTrackingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Track API endpoint usage for template-related operations
        if (IsTemplateRelatedEndpoint(context.Request.Path))
        {
            _logger.LogDebug("🎯 [MIDDLEWARE] Template-related endpoint accessed: {Path}", context.Request.Path);
            
            // Add tracking headers or context
            context.Items["TemplateTrackingEnabled"] = true;
            context.Items["TemplateTrackingStartTime"] = DateTime.UtcNow;
        }

        await _next(context);

        // Log completion for template-related operations
        if (context.Items.ContainsKey("TemplateTrackingEnabled"))
        {
            var startTime = (DateTime)context.Items["TemplateTrackingStartTime"]!;
            var duration = DateTime.UtcNow - startTime;
            
            _logger.LogDebug("🎯 [MIDDLEWARE] Template operation completed: {Path}, Duration: {Duration}ms", 
                context.Request.Path, duration.TotalMilliseconds);
        }
    }

    private static bool IsTemplateRelatedEndpoint(PathString path)
    {
        var pathValue = path.Value?.ToLowerInvariant() ?? string.Empty;
        return pathValue.Contains("/query") || 
               pathValue.Contains("/prompt") || 
               pathValue.Contains("/template") ||
               pathValue.Contains("/tuning");
    }
}

/// <summary>
/// Background service for template performance monitoring
/// </summary>
public class TemplatePerformanceMonitoringService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<TemplatePerformanceMonitoringService> _logger;
    private readonly TimeSpan _monitoringInterval = TimeSpan.FromMinutes(5);

    public TemplatePerformanceMonitoringService(
        IServiceProvider serviceProvider,
        ILogger<TemplatePerformanceMonitoringService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("🎯 [MONITOR] Template performance monitoring service started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await MonitorTemplatePerformanceAsync();
                await Task.Delay(_monitoringInterval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ [MONITOR] Error in template performance monitoring");
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }

        _logger.LogInformation("🎯 [MONITOR] Template performance monitoring service stopped");
    }

    private async Task MonitorTemplatePerformanceAsync()
    {
        using var scope = _serviceProvider.CreateScope();
        
        try
        {
            var performanceService = scope.ServiceProvider.GetService<ITemplatePerformanceService>();
            if (performanceService != null)
            {
                // Check for performance alerts
                var alerts = await performanceService.GetPerformanceAlertsAsync();
                
                foreach (var alert in alerts.Where(a => !a.IsAcknowledged))
                {
                    _logger.LogWarning("⚠️ [MONITOR] Template performance alert: {TemplateKey} - {Message}", 
                        alert.TemplateKey, alert.Message);
                }

                // Get underperforming templates
                var underperforming = await performanceService.GetUnderperformingTemplatesAsync();
                
                if (underperforming.Any())
                {
                    _logger.LogInformation("📊 [MONITOR] Found {Count} underperforming templates", underperforming.Count);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ [MONITOR] Error monitoring template performance");
        }
    }
}
