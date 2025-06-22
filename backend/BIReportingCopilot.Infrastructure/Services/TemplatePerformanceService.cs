using Microsoft.Extensions.Logging;
using BIReportingCopilot.Core.Interfaces.Analytics;
using BIReportingCopilot.Core.Interfaces.Repository;
using BIReportingCopilot.Core.Models.Analytics;
using BIReportingCopilot.Core.Models;
using BIReportingCopilot.Infrastructure.Data.Entities;
using AutoMapper;
using InterfaceTemplatePerformanceMetrics = BIReportingCopilot.Core.Interfaces.Analytics.TemplatePerformanceMetrics;
using InterfacePerformanceDataPoint = BIReportingCopilot.Core.Interfaces.Analytics.PerformanceDataPoint;
using InterfaceComparisonResult = BIReportingCopilot.Core.Interfaces.Analytics.ComparisonResult;

namespace BIReportingCopilot.Infrastructure.Services;

/// <summary>
/// Service implementation for real-time template performance tracking and analytics
/// </summary>
public class TemplatePerformanceService : ITemplatePerformanceService
{
    private readonly ITemplatePerformanceRepository _performanceRepository;
    private readonly IEnhancedPromptTemplateRepository _templateRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<TemplatePerformanceService> _logger;

    public TemplatePerformanceService(
        ITemplatePerformanceRepository performanceRepository,
        IEnhancedPromptTemplateRepository templateRepository,
        IMapper mapper,
        ILogger<TemplatePerformanceService> logger)
    {
        _performanceRepository = performanceRepository;
        _templateRepository = templateRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task TrackTemplateUsageAsync(string templateKey, bool wasSuccessful, decimal confidenceScore, int processingTimeMs, string? userId = null, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("📊 [ANALYTICS] Tracking template usage: {TemplateKey}, Success: {Success}, Confidence: {Confidence}",
                templateKey, wasSuccessful, confidenceScore);

            // Get template to find ID
            var template = await _templateRepository.GetByKeyAsync(templateKey, cancellationToken);
            if (template == null)
            {
                _logger.LogWarning("Template not found for usage tracking: {TemplateKey}", templateKey);
                return;
            }

            // Update performance metrics (existing separate table)
            await _performanceRepository.IncrementUsageAsync(template.Id, wasSuccessful, confidenceScore, processingTimeMs, cancellationToken);

            // Update template usage count and analytics columns (NEW)
            await _templateRepository.UpdateUsageCountAsync(template.Id, cancellationToken);

            // Update LastUsedDate in the new analytics columns
            await _templateRepository.UpdateLastUsedDateAsync(template.Id, cancellationToken);

            // Update success rate if we have enough data
            if (template.UsageCount > 0)
            {
                var newSuccessRate = wasSuccessful ?
                    ((template.SuccessRate ?? 0) * template.UsageCount + (wasSuccessful ? 100 : 0)) / (template.UsageCount + 1) :
                    ((template.SuccessRate ?? 0) * template.UsageCount) / (template.UsageCount + 1);

                await _templateRepository.UpdateSuccessRateAsync(template.Id, newSuccessRate, cancellationToken);
            }

            _logger.LogInformation("✅ [ANALYTICS] Successfully tracked usage for template {TemplateKey} - Usage Count: {UsageCount}",
                templateKey, template.UsageCount + 1);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ [ANALYTICS] Error tracking template usage for {TemplateKey}", templateKey);
            throw;
        }
    }

    public async Task UpdateUserRatingAsync(string templateKey, decimal rating, string? userId = null, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Updating user rating for template: {TemplateKey}, Rating: {Rating}", templateKey, rating);

            var template = await _templateRepository.GetByKeyAsync(templateKey, cancellationToken);
            if (template == null)
            {
                _logger.LogWarning("Template not found for rating update: {TemplateKey}", templateKey);
                return;
            }

            await _performanceRepository.UpdateUserRatingAsync(template.Id, rating, cancellationToken);

            _logger.LogInformation("Successfully updated rating for template {TemplateKey}: {Rating}", templateKey, rating);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user rating for {TemplateKey}", templateKey);
            throw;
        }
    }

    public async Task<BIReportingCopilot.Core.Interfaces.Analytics.TemplatePerformanceMetrics?> GetTemplatePerformanceAsync(string templateKey, CancellationToken cancellationToken = default)
    {
        try
        {
            var template = await _templateRepository.GetByKeyAsync(templateKey, cancellationToken);
            if (template == null)
            {
                return null;
            }

            var performanceEntity = await _performanceRepository.GetByTemplateIdAsync(template.Id, cancellationToken);
            if (performanceEntity == null)
            {
                return null;
            }

            return MapToPerformanceMetrics(performanceEntity, template);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting template performance for {TemplateKey}", templateKey);
            throw;
        }
    }

    public async Task<List<BIReportingCopilot.Core.Interfaces.Analytics.TemplatePerformanceMetrics>> GetTemplatePerformanceAsync(List<string> templateKeys, CancellationToken cancellationToken = default)
    {
        try
        {
            var results = new List<BIReportingCopilot.Core.Interfaces.Analytics.TemplatePerformanceMetrics>();

            foreach (var templateKey in templateKeys)
            {
                var performance = await GetTemplatePerformanceAsync(templateKey, cancellationToken);
                if (performance != null)
                {
                    results.Add(performance);
                }
            }

            return results;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting template performance for multiple templates");
            throw;
        }
    }

    public async Task<List<BIReportingCopilot.Core.Interfaces.Analytics.TemplatePerformanceMetrics>> GetTopPerformingTemplatesAsync(string? intentType = null, int count = 10, CancellationToken cancellationToken = default)
    {
        try
        {
            List<BIReportingCopilot.Core.Models.TemplatePerformanceMetricsEntity> entities;

            if (!string.IsNullOrEmpty(intentType))
            {
                entities = await _performanceRepository.GetByIntentTypeAsync(intentType, cancellationToken);
                entities = entities.OrderByDescending(x => x.SuccessRate).Take(count).ToList();
            }
            else
            {
                entities = await _performanceRepository.GetTopPerformingTemplatesAsync(count, cancellationToken);
            }

            var results = new List<BIReportingCopilot.Core.Interfaces.Analytics.TemplatePerformanceMetrics>();
            foreach (var entity in entities)
            {
                var template = await _templateRepository.GetByIdAsync(entity.TemplateId, cancellationToken);
                if (template != null)
                {
                    results.Add(MapToPerformanceMetrics(entity, template));
                }
            }

            return results;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting top performing templates");
            throw;
        }
    }

    public async Task<List<BIReportingCopilot.Core.Interfaces.Analytics.TemplatePerformanceMetrics>> GetUnderperformingTemplatesAsync(decimal threshold = 0.7m, int minUsageCount = 10, CancellationToken cancellationToken = default)
    {
        try
        {
            var entities = await _performanceRepository.GetUnderperformingTemplatesAsync(threshold, cancellationToken);
            entities = entities.Where(x => x.TotalUsages >= minUsageCount).ToList();

            var results = new List<BIReportingCopilot.Core.Interfaces.Analytics.TemplatePerformanceMetrics>();
            foreach (var entity in entities)
            {
                var template = await _templateRepository.GetByIdAsync(entity.TemplateId, cancellationToken);
                if (template != null)
                {
                    results.Add(MapToPerformanceMetrics(entity, template));
                }
            }

            return results;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting underperforming templates");
            throw;
        }
    }

    public async Task<TemplatePerformanceTrends> GetPerformanceTrendsAsync(string templateKey, TimeSpan timeWindow, CancellationToken cancellationToken = default)
    {
        try
        {
            // This would require historical data tracking - for now return basic trend
            var performance = await GetTemplatePerformanceAsync(templateKey, cancellationToken);
            if (performance == null)
            {
                throw new ArgumentException($"Template not found: {templateKey}");
            }

            // Simplified trend calculation - in a real implementation, you'd query historical data
            var trends = new TemplatePerformanceTrends
            {
                TemplateKey = templateKey,
                TimeWindow = timeWindow,
                DataPoints = new List<InterfacePerformanceDataPoint>
                {
                    new InterfacePerformanceDataPoint
                    {
                        Timestamp = DateTime.UtcNow,
                        SuccessRate = performance.SuccessRate,
                        AverageConfidenceScore = performance.AverageConfidenceScore,
                        UsageCount = performance.TotalUsages,
                        UserRating = performance.AverageUserRating,
                        ProcessingTime = performance.AverageProcessingTimeMs
                    }
                },
                TrendDirection = 0, // Would calculate based on historical data
                AnalysisDate = DateTime.UtcNow
            };

            return trends;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting performance trends for {TemplateKey}", templateKey);
            throw;
        }
    }

    public async Task<Dictionary<string, BIReportingCopilot.Core.Interfaces.Analytics.TemplatePerformanceMetrics>> GetPerformanceByIntentTypeAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var successRates = await _performanceRepository.GetSuccessRatesByIntentTypeAsync(cancellationToken);
            var usageCounts = await _performanceRepository.GetUsageCountsByIntentTypeAsync(cancellationToken);

            var results = new Dictionary<string, InterfaceTemplatePerformanceMetrics>();

            foreach (var intentType in successRates.Keys.Union(usageCounts.Keys))
            {
                var avgSuccessRate = successRates.GetValueOrDefault(intentType, 0);
                var totalUsage = usageCounts.GetValueOrDefault(intentType, 0);
                var avgProcessingTime = await _performanceRepository.GetAverageProcessingTimeAsync(intentType, cancellationToken);

                results[intentType] = new InterfaceTemplatePerformanceMetrics
                {
                    IntentType = intentType,
                    SuccessRate = avgSuccessRate,
                    TotalUsages = totalUsage,
                    AverageProcessingTimeMs = (int)avgProcessingTime,
                    UpdatedDate = DateTime.UtcNow
                };
            }

            return results;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting performance by intent type");
            throw;
        }
    }

    public async Task<TemplatePerformanceComparison> CompareTemplatePerformanceAsync(string templateKey1, string templateKey2, CancellationToken cancellationToken = default)
    {
        try
        {
            var performance1 = await GetTemplatePerformanceAsync(templateKey1, cancellationToken);
            var performance2 = await GetTemplatePerformanceAsync(templateKey2, cancellationToken);

            if (performance1 == null || performance2 == null)
            {
                throw new ArgumentException("One or both templates not found");
            }

            var comparison = new TemplatePerformanceComparison
            {
                Template1 = performance1,
                Template2 = performance2,
                ComparisonDate = DateTime.UtcNow
            };

            // Calculate metric comparisons
            comparison.MetricComparisons["SuccessRate"] = new InterfaceComparisonResult
            {
                MetricName = "Success Rate",
                Value1 = performance1.SuccessRate,
                Value2 = performance2.SuccessRate,
                Difference = performance2.SuccessRate - performance1.SuccessRate,
                PercentageDifference = performance1.SuccessRate > 0 ? 
                    ((performance2.SuccessRate - performance1.SuccessRate) / performance1.SuccessRate) * 100 : 0,
                BetterTemplate = performance2.SuccessRate > performance1.SuccessRate ? templateKey2 : templateKey1
            };

            comparison.MetricComparisons["ProcessingTime"] = new InterfaceComparisonResult
            {
                MetricName = "Processing Time",
                Value1 = performance1.AverageProcessingTimeMs,
                Value2 = performance2.AverageProcessingTimeMs,
                Difference = performance2.AverageProcessingTimeMs - performance1.AverageProcessingTimeMs,
                PercentageDifference = performance1.AverageProcessingTimeMs > 0 ? 
                    ((performance2.AverageProcessingTimeMs - performance1.AverageProcessingTimeMs) / performance1.AverageProcessingTimeMs) * 100 : 0,
                BetterTemplate = performance2.AverageProcessingTimeMs < performance1.AverageProcessingTimeMs ? templateKey2 : templateKey1
            };

            // Determine overall better performing template
            var successRateWeight = 0.6m;
            var processingTimeWeight = 0.4m;

            var score1 = (performance1.SuccessRate * successRateWeight) - 
                        ((performance1.AverageProcessingTimeMs / 1000m) * processingTimeWeight);
            var score2 = (performance2.SuccessRate * successRateWeight) - 
                        ((performance2.AverageProcessingTimeMs / 1000m) * processingTimeWeight);

            comparison.BetterPerformingTemplate = score2 > score1 ? templateKey2 : templateKey1;
            comparison.OverallPerformanceDifference = Math.Abs(score2 - score1);

            return comparison;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error comparing template performance between {Template1} and {Template2}", templateKey1, templateKey2);
            throw;
        }
    }

    public async Task<List<BIReportingCopilot.Core.Interfaces.Analytics.TemplatePerformanceMetrics>> GetRecentlyUsedTemplatesAsync(int days = 30, CancellationToken cancellationToken = default)
    {
        try
        {
            var entities = await _performanceRepository.GetRecentlyUsedAsync(days, cancellationToken);

            var results = new List<BIReportingCopilot.Core.Interfaces.Analytics.TemplatePerformanceMetrics>();
            foreach (var entity in entities)
            {
                var template = await _templateRepository.GetByIdAsync(entity.TemplateId, cancellationToken);
                if (template != null)
                {
                    results.Add(MapToPerformanceMetrics(entity, template));
                }
            }

            return results;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting recently used templates");
            throw;
        }
    }

    public async Task RecalculateAllMetricsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Starting recalculation of all template metrics");

            var allMetrics = await _performanceRepository.GetAllAsync(cancellationToken);

            foreach (var metric in allMetrics)
            {
                await _performanceRepository.RecalculateMetricsAsync(metric.TemplateId, cancellationToken);
            }

            _logger.LogInformation("Completed recalculation of {Count} template metrics", allMetrics.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recalculating all metrics");
            throw;
        }
    }

    public async Task<PerformanceDashboardData> GetDashboardDataAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var allTemplates = await _templateRepository.GetActiveTemplatesAsync(cancellationToken);
            var topPerformers = await GetTopPerformingTemplatesAsync(count: 5, cancellationToken: cancellationToken);
            var needsAttention = await GetUnderperformingTemplatesAsync(cancellationToken: cancellationToken);
            var usageByIntent = await _performanceRepository.GetUsageCountsByIntentTypeAsync(cancellationToken);
            var overallSuccessRate = await _performanceRepository.GetAverageSuccessRateAsync(cancellationToken: cancellationToken);

            // Calculate today's usage
            var todayUsage = 0; // Would need to implement daily usage tracking

            return new PerformanceDashboardData
            {
                TotalTemplates = allTemplates.Count,
                ActiveTemplates = allTemplates.Count(t => t.IsActive),
                OverallSuccessRate = overallSuccessRate,
                TotalUsagesToday = todayUsage,
                TopPerformers = topPerformers,
                NeedsAttention = needsAttention.Take(5).ToList(),
                UsageByIntentType = usageByIntent,
                GeneratedDate = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting dashboard data");
            throw;
        }
    }

    public async Task<byte[]> ExportPerformanceDataAsync(DateTime startDate, DateTime endDate, BIReportingCopilot.Core.Models.Analytics.ExportFormat format = BIReportingCopilot.Core.Models.Analytics.ExportFormat.CSV, CancellationToken cancellationToken = default)
    {
        try
        {
            var performanceData = await _performanceRepository.GetByUsageDateRangeAsync(startDate, endDate, cancellationToken);

            // Simple CSV export implementation
            if (format == BIReportingCopilot.Core.Models.Analytics.ExportFormat.CSV)
            {
                var csv = "TemplateKey,TemplateName,IntentType,TotalUsages,SuccessfulUsages,SuccessRate,AverageConfidenceScore,AverageProcessingTimeMs,AverageUserRating,LastUsedDate\n";
                
                foreach (var data in performanceData)
                {
                    var template = await _templateRepository.GetByIdAsync(data.TemplateId, cancellationToken);
                    csv += $"{data.TemplateKey},{template?.Name},{data.IntentType},{data.TotalUsages},{data.SuccessfulUsages},{data.SuccessRate},{data.AverageConfidenceScore},{data.AverageProcessingTimeMs},{data.AverageUserRating},{data.LastUsedDate}\n";
                }

                return System.Text.Encoding.UTF8.GetBytes(csv);
            }

            throw new NotImplementedException($"Export format {format} not implemented");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting performance data");
            throw;
        }
    }

    public async Task<List<BIReportingCopilot.Core.Interfaces.Analytics.PerformanceAlert>> GetPerformanceAlertsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var alerts = new List<BIReportingCopilot.Core.Interfaces.Analytics.PerformanceAlert>();

            // Get underperforming templates
            var underperforming = await GetUnderperformingTemplatesAsync(cancellationToken: cancellationToken);
            foreach (var template in underperforming)
            {
                alerts.Add(new BIReportingCopilot.Core.Interfaces.Analytics.PerformanceAlert
                {
                    TemplateKey = template.TemplateKey,
                    TemplateName = template.TemplateName,
                    Severity = template.SuccessRate < 0.5m ? BIReportingCopilot.Core.Interfaces.Analytics.AlertSeverity.High : BIReportingCopilot.Core.Interfaces.Analytics.AlertSeverity.Medium,
                    AlertType = "Low Success Rate",
                    Message = $"Template has low success rate: {template.SuccessRate:P2}",
                    CreatedDate = DateTime.UtcNow
                });
            }

            return alerts;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting performance alerts");
            throw;
        }
    }

    public async Task<TemplateUsageInsights> GetUsageInsightsAsync(string templateKey, CancellationToken cancellationToken = default)
    {
        try
        {
            var performance = await GetTemplatePerformanceAsync(templateKey, cancellationToken);
            if (performance == null)
            {
                throw new ArgumentException($"Template not found: {templateKey}");
            }

            // Basic insights implementation - would be enhanced with more detailed analytics
            var insights = new TemplateUsageInsights
            {
                TemplateKey = templateKey,
                UsagePatterns = new List<UsagePattern>
                {
                    new UsagePattern
                    {
                        PatternType = "Success Rate",
                        Description = performance.SuccessRate > 0.8m ? "High success rate" : "Needs improvement",
                        Frequency = (int)(performance.SuccessRate * 100),
                        Impact = performance.SuccessRate
                    }
                },
                AnalysisDate = DateTime.UtcNow
            };

            return insights;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting usage insights for {TemplateKey}", templateKey);
            throw;
        }
    }

    #region Private Methods

    private static BIReportingCopilot.Core.Interfaces.Analytics.TemplatePerformanceMetrics MapToPerformanceMetrics(BIReportingCopilot.Core.Models.TemplatePerformanceMetricsEntity entity, BIReportingCopilot.Core.Models.PromptTemplateEntity template)
    {
        return new BIReportingCopilot.Core.Interfaces.Analytics.TemplatePerformanceMetrics
        {
            TemplateKey = entity.TemplateKey,
            TemplateName = template.Name,
            IntentType = entity.IntentType,
            TotalUsages = entity.TotalUsages,
            SuccessfulUsages = entity.SuccessfulUsages,
            SuccessRate = entity.SuccessRate,
            AverageConfidenceScore = entity.AverageConfidenceScore,
            AverageProcessingTimeMs = entity.AverageProcessingTimeMs,
            AverageUserRating = entity.AverageUserRating,
            LastUsedDate = entity.LastUsedDate,
            CreatedDate = entity.CreatedDate,
            UpdatedDate = entity.UpdatedDate
        };
    }

    #endregion
}
