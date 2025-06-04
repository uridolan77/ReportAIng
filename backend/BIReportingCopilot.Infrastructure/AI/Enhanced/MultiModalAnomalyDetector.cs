using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using BIReportingCopilot.Core.Interfaces;
using BIReportingCopilot.Core.Models;
using BIReportingCopilot.Infrastructure.Performance;

namespace BIReportingCopilot.Infrastructure.AI.Enhanced;

/// <summary>
/// Multi-modal anomaly detection system for BI data analysis
/// Implements Enhancement 11: Multi-Modal Anomaly Detection
/// </summary>
public class MultiModalAnomalyDetector
{
    private readonly ILogger<MultiModalAnomalyDetector> _logger;
    private readonly ICacheService _cacheService;
    private readonly StatisticalAnomalyDetector _statisticalDetector;
    private readonly TemporalAnomalyDetector _temporalDetector;
    private readonly PatternAnomalyDetector _patternDetector;
    private readonly BusinessRuleAnomalyDetector _businessRuleDetector;
    private readonly AnomalyConfiguration _config;
    private readonly AnomalyAlertManager _alertManager;

    public MultiModalAnomalyDetector(
        ILogger<MultiModalAnomalyDetector> logger,
        ICacheService cacheService,
        IOptions<AnomalyConfiguration> config)
    {
        _logger = logger;
        _cacheService = cacheService;
        _config = config.Value;
        
        _statisticalDetector = new StatisticalAnomalyDetector(logger, config.Value);
        _temporalDetector = new TemporalAnomalyDetector(logger, config.Value);
        _patternDetector = new PatternAnomalyDetector(logger, config.Value);
        _businessRuleDetector = new BusinessRuleAnomalyDetector(logger, config.Value);
        _alertManager = new AnomalyAlertManager(logger, cacheService);
    }

    /// <summary>
    /// Detect anomalies in query results using multiple detection methods
    /// </summary>
    public async Task<AnomalyDetectionResult> DetectAnomaliesAsync(
        QueryResult queryResult,
        SemanticAnalysis semanticAnalysis,
        string? userId = null)
    {
        try
        {
            _logger.LogDebug("Starting multi-modal anomaly detection for query result with {RowCount} rows", 
                queryResult.Data?.Count ?? 0);

            var detectionTasks = new List<Task<List<Anomaly>>>();

            // Run different detection methods in parallel
            if (_config.EnableStatisticalDetection)
            {
                detectionTasks.Add(_statisticalDetector.DetectAnomaliesAsync(queryResult, semanticAnalysis));
            }

            if (_config.EnableTemporalDetection)
            {
                detectionTasks.Add(_temporalDetector.DetectAnomaliesAsync(queryResult, semanticAnalysis));
            }

            if (_config.EnablePatternDetection)
            {
                detectionTasks.Add(_patternDetector.DetectAnomaliesAsync(queryResult, semanticAnalysis));
            }

            if (_config.EnableBusinessRuleDetection)
            {
                detectionTasks.Add(_businessRuleDetector.DetectAnomaliesAsync(queryResult, semanticAnalysis));
            }

            // Wait for all detection methods to complete
            var detectionResults = await Task.WhenAll(detectionTasks);

            // Combine and analyze results
            var allAnomalies = detectionResults.SelectMany(anomalies => anomalies).ToList();
            var consolidatedAnomalies = await ConsolidateAnomaliesAsync(allAnomalies);
            var rankedAnomalies = await RankAnomaliesBySeverityAsync(consolidatedAnomalies);

            // Generate insights and recommendations
            var insights = await GenerateAnomalyInsightsAsync(rankedAnomalies, queryResult, semanticAnalysis);
            var recommendations = await GenerateRecommendationsAsync(rankedAnomalies, semanticAnalysis);

            var result = new AnomalyDetectionResult
            {
                Anomalies = rankedAnomalies,
                TotalAnomalies = rankedAnomalies.Count,
                HighSeverityCount = rankedAnomalies.Count(a => a.Severity == AnomalySeverity.High),
                MediumSeverityCount = rankedAnomalies.Count(a => a.Severity == AnomalySeverity.Medium),
                LowSeverityCount = rankedAnomalies.Count(a => a.Severity == AnomalySeverity.Low),
                Insights = insights,
                Recommendations = recommendations,
                DetectionMethods = detectionTasks.Count,
                ProcessingTime = DateTime.UtcNow,
                Metadata = new Dictionary<string, object>
                {
                    ["detection_timestamp"] = DateTime.UtcNow,
                    ["user_id"] = userId ?? "anonymous",
                    ["query_row_count"] = queryResult.Data?.Count ?? 0,
                    ["detection_methods_used"] = GetEnabledDetectionMethods()
                }
            };

            // Send alerts for high-severity anomalies
            await ProcessAnomalyAlertsAsync(rankedAnomalies, userId);

            _logger.LogDebug("Anomaly detection completed: {TotalAnomalies} anomalies found ({HighSeverity} high severity)",
                result.TotalAnomalies, result.HighSeverityCount);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in multi-modal anomaly detection");
            return new AnomalyDetectionResult
            {
                Anomalies = new List<Anomaly>(),
                ProcessingTime = DateTime.UtcNow,
                Metadata = new Dictionary<string, object> { ["error"] = true }
            };
        }
    }

    /// <summary>
    /// Get anomaly detection history and trends
    /// </summary>
    public async Task<AnomalyTrendAnalysis> GetAnomalyTrendsAsync(TimeSpan period, string? userId = null)
    {
        try
        {
            var endTime = DateTime.UtcNow;
            var startTime = endTime - period;
            
            var historicalAnomalies = await GetHistoricalAnomaliesAsync(startTime, endTime, userId);
            
            return new AnomalyTrendAnalysis
            {
                Period = period,
                TotalAnomalies = historicalAnomalies.Count,
                AnomaliesByDay = GroupAnomaliesByDay(historicalAnomalies),
                AnomaliesByType = GroupAnomaliesByType(historicalAnomalies),
                AnomaliesBySeverity = GroupAnomaliesBySeverity(historicalAnomalies),
                TrendDirection = CalculateTrendDirection(historicalAnomalies),
                MostCommonAnomalies = GetMostCommonAnomalies(historicalAnomalies),
                RecommendedActions = GenerateTrendRecommendations(historicalAnomalies),
                GeneratedAt = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating anomaly trend analysis");
            return new AnomalyTrendAnalysis
            {
                Period = period,
                GeneratedAt = DateTime.UtcNow
            };
        }
    }

    /// <summary>
    /// Configure anomaly detection thresholds and rules
    /// </summary>
    public async Task UpdateDetectionConfigurationAsync(AnomalyDetectionConfiguration configuration)
    {
        try
        {
            _logger.LogInformation("Updating anomaly detection configuration");

            // Update statistical thresholds
            if (configuration.StatisticalThresholds != null)
            {
                await _statisticalDetector.UpdateThresholdsAsync(configuration.StatisticalThresholds);
            }

            // Update temporal detection parameters
            if (configuration.TemporalParameters != null)
            {
                await _temporalDetector.UpdateParametersAsync(configuration.TemporalParameters);
            }

            // Update business rules
            if (configuration.BusinessRules != null)
            {
                await _businessRuleDetector.UpdateRulesAsync(configuration.BusinessRules);
            }

            // Store configuration
            await _cacheService.SetAsync("anomaly_detection_config", configuration, TimeSpan.FromDays(30));

            _logger.LogInformation("Anomaly detection configuration updated successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating anomaly detection configuration");
        }
    }

    /// <summary>
    /// Train anomaly detection models with historical data
    /// </summary>
    public async Task TrainDetectionModelsAsync(List<QueryResult> historicalData, string? userId = null)
    {
        try
        {
            _logger.LogInformation("Training anomaly detection models with {DataCount} historical queries", 
                historicalData.Count);

            // Train statistical models
            await _statisticalDetector.TrainAsync(historicalData);

            // Train temporal models
            await _temporalDetector.TrainAsync(historicalData);

            // Train pattern recognition models
            await _patternDetector.TrainAsync(historicalData);

            // Update model metadata
            var modelMetadata = new AnomalyModelMetadata
            {
                TrainingDataCount = historicalData.Count,
                LastTrainingDate = DateTime.UtcNow,
                ModelVersion = "1.0",
                UserId = userId
            };

            await _cacheService.SetAsync("anomaly_model_metadata", modelMetadata, TimeSpan.FromDays(30));

            _logger.LogInformation("Anomaly detection model training completed");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error training anomaly detection models");
        }
    }

    // Private methods

    private async Task<List<Anomaly>> ConsolidateAnomaliesAsync(List<Anomaly> anomalies)
    {
        var consolidated = new List<Anomaly>();
        var processedAnomalies = new HashSet<string>();

        foreach (var anomaly in anomalies.OrderByDescending(a => a.Confidence))
        {
            var key = $"{anomaly.Type}_{anomaly.AffectedColumn}_{anomaly.AffectedRows?.FirstOrDefault()}";
            
            if (!processedAnomalies.Contains(key))
            {
                // Check for similar anomalies to merge
                var similarAnomalies = anomalies
                    .Where(a => a.Type == anomaly.Type && 
                               a.AffectedColumn == anomaly.AffectedColumn &&
                               Math.Abs(a.Confidence - anomaly.Confidence) < 0.2)
                    .ToList();

                if (similarAnomalies.Count > 1)
                {
                    // Merge similar anomalies
                    var mergedAnomaly = MergeAnomalies(similarAnomalies);
                    consolidated.Add(mergedAnomaly);
                    
                    foreach (var similar in similarAnomalies)
                    {
                        var similarKey = $"{similar.Type}_{similar.AffectedColumn}_{similar.AffectedRows?.FirstOrDefault()}";
                        processedAnomalies.Add(similarKey);
                    }
                }
                else
                {
                    consolidated.Add(anomaly);
                    processedAnomalies.Add(key);
                }
            }
        }

        return consolidated;
    }

    private Anomaly MergeAnomalies(List<Anomaly> anomalies)
    {
        var primary = anomalies.OrderByDescending(a => a.Confidence).First();
        
        return new Anomaly
        {
            Id = Guid.NewGuid().ToString(),
            Type = primary.Type,
            Severity = anomalies.Max(a => a.Severity),
            Confidence = anomalies.Average(a => a.Confidence),
            Description = $"Multiple {primary.Type} anomalies detected",
            AffectedColumn = primary.AffectedColumn,
            AffectedRows = anomalies.SelectMany(a => a.AffectedRows ?? new List<int>()).Distinct().ToList(),
            ExpectedValue = primary.ExpectedValue,
            ActualValue = primary.ActualValue,
            DetectionMethod = string.Join(", ", anomalies.Select(a => a.DetectionMethod).Distinct()),
            DetectedAt = anomalies.Min(a => a.DetectedAt),
            Metadata = new Dictionary<string, object>
            {
                ["merged_anomaly_count"] = anomalies.Count,
                ["source_anomaly_ids"] = anomalies.Select(a => a.Id).ToList()
            }
        };
    }

    private async Task<List<Anomaly>> RankAnomaliesBySeverityAsync(List<Anomaly> anomalies)
    {
        // Apply business context and domain knowledge for ranking
        foreach (var anomaly in anomalies)
        {
            var contextualSeverity = await CalculateContextualSeverityAsync(anomaly);
            anomaly.Severity = CombineSeverities(anomaly.Severity, contextualSeverity);
        }

        return anomalies
            .OrderByDescending(a => a.Severity)
            .ThenByDescending(a => a.Confidence)
            .ToList();
    }

    private async Task<AnomalySeverity> CalculateContextualSeverityAsync(Anomaly anomaly)
    {
        // Apply business rules to determine contextual severity
        if (anomaly.AffectedColumn?.ToLowerInvariant().Contains("revenue") == true)
        {
            return AnomalySeverity.High; // Revenue anomalies are always high priority
        }

        if (anomaly.AffectedColumn?.ToLowerInvariant().Contains("deposit") == true)
        {
            return AnomalySeverity.Medium; // Deposit anomalies are medium priority
        }

        if (anomaly.Confidence > 0.9)
        {
            return AnomalySeverity.High; // High confidence anomalies
        }

        return AnomalySeverity.Low;
    }

    private AnomalySeverity CombineSeverities(AnomalySeverity original, AnomalySeverity contextual)
    {
        return (AnomalySeverity)Math.Max((int)original, (int)contextual);
    }

    private async Task<List<AnomalyInsight>> GenerateAnomalyInsightsAsync(
        List<Anomaly> anomalies, 
        QueryResult queryResult, 
        SemanticAnalysis semanticAnalysis)
    {
        var insights = new List<AnomalyInsight>();

        // Generate insights based on anomaly patterns
        var anomalyGroups = anomalies.GroupBy(a => a.Type).ToList();
        
        foreach (var group in anomalyGroups)
        {
            var insight = new AnomalyInsight
            {
                Type = InsightType.Pattern,
                Title = $"{group.Key} Anomaly Pattern",
                Description = GenerateInsightDescription(group.Key, group.ToList()),
                Confidence = group.Average(a => a.Confidence),
                AffectedAnomalies = group.Select(a => a.Id).ToList()
            };
            
            insights.Add(insight);
        }

        // Generate severity-based insights
        var highSeverityAnomalies = anomalies.Where(a => a.Severity == AnomalySeverity.High).ToList();
        if (highSeverityAnomalies.Any())
        {
            insights.Add(new AnomalyInsight
            {
                Type = InsightType.Alert,
                Title = "High Severity Anomalies Detected",
                Description = $"Found {highSeverityAnomalies.Count} high-severity anomalies requiring immediate attention",
                Confidence = 0.95,
                AffectedAnomalies = highSeverityAnomalies.Select(a => a.Id).ToList()
            });
        }

        return insights;
    }

    private async Task<List<AnomalyRecommendation>> GenerateRecommendationsAsync(
        List<Anomaly> anomalies, 
        SemanticAnalysis semanticAnalysis)
    {
        var recommendations = new List<AnomalyRecommendation>();

        // Generate recommendations based on anomaly types
        var statisticalAnomalies = anomalies.Where(a => a.Type == AnomalyType.Statistical).ToList();
        if (statisticalAnomalies.Any())
        {
            recommendations.Add(new AnomalyRecommendation
            {
                Type = RecommendationType.Investigation,
                Title = "Investigate Statistical Outliers",
                Description = "Review data collection processes and validate unusual statistical patterns",
                Priority = RecommendationPriority.Medium,
                EstimatedEffort = "2-4 hours",
                AffectedAnomalies = statisticalAnomalies.Select(a => a.Id).ToList()
            });
        }

        var temporalAnomalies = anomalies.Where(a => a.Type == AnomalyType.Temporal).ToList();
        if (temporalAnomalies.Any())
        {
            recommendations.Add(new AnomalyRecommendation
            {
                Type = RecommendationType.Monitoring,
                Title = "Monitor Temporal Patterns",
                Description = "Set up alerts for unusual temporal patterns and trends",
                Priority = RecommendationPriority.High,
                EstimatedEffort = "1-2 hours",
                AffectedAnomalies = temporalAnomalies.Select(a => a.Id).ToList()
            });
        }

        return recommendations;
    }

    private async Task ProcessAnomalyAlertsAsync(List<Anomaly> anomalies, string? userId)
    {
        var highSeverityAnomalies = anomalies.Where(a => a.Severity == AnomalySeverity.High).ToList();
        
        foreach (var anomaly in highSeverityAnomalies)
        {
            await _alertManager.SendAnomalyAlertAsync(anomaly, userId);
        }
    }

    private List<string> GetEnabledDetectionMethods()
    {
        var methods = new List<string>();
        
        if (_config.EnableStatisticalDetection) methods.Add("Statistical");
        if (_config.EnableTemporalDetection) methods.Add("Temporal");
        if (_config.EnablePatternDetection) methods.Add("Pattern");
        if (_config.EnableBusinessRuleDetection) methods.Add("BusinessRule");
        
        return methods;
    }

    private async Task<List<Anomaly>> GetHistoricalAnomaliesAsync(DateTime startTime, DateTime endTime, string? userId)
    {
        // In a real implementation, this would query a database
        // For now, return empty list
        return new List<Anomaly>();
    }

    private Dictionary<DateTime, int> GroupAnomaliesByDay(List<Anomaly> anomalies)
    {
        return anomalies
            .GroupBy(a => a.DetectedAt.Date)
            .ToDictionary(g => g.Key, g => g.Count());
    }

    private Dictionary<AnomalyType, int> GroupAnomaliesByType(List<Anomaly> anomalies)
    {
        return anomalies
            .GroupBy(a => a.Type)
            .ToDictionary(g => g.Key, g => g.Count());
    }

    private Dictionary<AnomalySeverity, int> GroupAnomaliesBySeverity(List<Anomaly> anomalies)
    {
        return anomalies
            .GroupBy(a => a.Severity)
            .ToDictionary(g => g.Key, g => g.Count());
    }

    private TrendDirection CalculateTrendDirection(List<Anomaly> anomalies)
    {
        if (anomalies.Count < 2) return TrendDirection.Stable;

        var recentAnomalies = anomalies.Where(a => a.DetectedAt > DateTime.UtcNow.AddDays(-7)).Count();
        var olderAnomalies = anomalies.Where(a => a.DetectedAt <= DateTime.UtcNow.AddDays(-7)).Count();

        if (recentAnomalies > olderAnomalies * 1.2) return TrendDirection.Increasing;
        if (recentAnomalies < olderAnomalies * 0.8) return TrendDirection.Decreasing;
        
        return TrendDirection.Stable;
    }

    private List<AnomalyTypeFrequency> GetMostCommonAnomalies(List<Anomaly> anomalies)
    {
        return anomalies
            .GroupBy(a => a.Type)
            .Select(g => new AnomalyTypeFrequency
            {
                Type = g.Key,
                Count = g.Count(),
                Percentage = (double)g.Count() / anomalies.Count * 100
            })
            .OrderByDescending(f => f.Count)
            .Take(5)
            .ToList();
    }

    private List<string> GenerateTrendRecommendations(List<Anomaly> anomalies)
    {
        var recommendations = new List<string>();

        if (anomalies.Count > 50)
        {
            recommendations.Add("High anomaly frequency detected - consider reviewing data quality processes");
        }

        var revenueAnomalies = anomalies.Count(a => a.AffectedColumn?.ToLowerInvariant().Contains("revenue") == true);
        if (revenueAnomalies > 5)
        {
            recommendations.Add("Multiple revenue anomalies detected - prioritize financial data validation");
        }

        return recommendations;
    }

    private string GenerateInsightDescription(AnomalyType type, List<Anomaly> anomalies)
    {
        return type switch
        {
            AnomalyType.Statistical => $"Detected {anomalies.Count} statistical outliers with average confidence {anomalies.Average(a => a.Confidence):P1}",
            AnomalyType.Temporal => $"Found {anomalies.Count} temporal anomalies indicating unusual time-based patterns",
            AnomalyType.Pattern => $"Identified {anomalies.Count} pattern anomalies suggesting data irregularities",
            AnomalyType.BusinessRule => $"Discovered {anomalies.Count} business rule violations requiring attention",
            _ => $"Detected {anomalies.Count} anomalies of type {type}"
        };
    }
}
