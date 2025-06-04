using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using BIReportingCopilot.Core.Interfaces;
using BIReportingCopilot.Core.Models;
using BIReportingCopilot.Infrastructure.AI.Enhanced;
using BIReportingCopilot.Infrastructure.Configuration;
using BIReportingCopilot.Infrastructure.Performance;
using System.Diagnostics;

namespace BIReportingCopilot.Infrastructure.Migration;

/// <summary>
/// Service for managing migration from legacy to enhanced AI services
/// Provides A/B testing, gradual rollout, and performance comparison
/// </summary>
public class EnhancedAIMigrationService
{
    private readonly ILogger<EnhancedAIMigrationService> _logger;
    private readonly IQueryProcessor _legacyProcessor;
    private readonly EnhancedQueryProcessorV2 _enhancedProcessor;
    private readonly ICacheService _cacheService;
    private readonly FeatureFlags _featureFlags;
    private readonly MigrationSettings _migrationSettings;
    private readonly MigrationMetricsCollector _metricsCollector;

    public EnhancedAIMigrationService(
        ILogger<EnhancedAIMigrationService> logger,
        IQueryProcessor legacyProcessor,
        EnhancedQueryProcessorV2 enhancedProcessor,
        ICacheService cacheService,
        IOptions<FeatureFlags> featureFlags,
        IOptions<MigrationSettings> migrationSettings)
    {
        _logger = logger;
        _legacyProcessor = legacyProcessor;
        _enhancedProcessor = enhancedProcessor;
        _cacheService = cacheService;
        _featureFlags = featureFlags.Value;
        _migrationSettings = migrationSettings.Value;
        _metricsCollector = new MigrationMetricsCollector(logger, cacheService);
    }

    /// <summary>
    /// Process query using appropriate processor based on migration settings
    /// </summary>
    public async Task<ProcessedQuery> ProcessQueryAsync(string naturalLanguageQuery, string userId)
    {
        try
        {
            // Determine which processor to use
            var useEnhanced = await ShouldUseEnhancedProcessorAsync(userId);
            
            if (useEnhanced)
            {
                return await ProcessWithEnhancedAsync(naturalLanguageQuery, userId);
            }
            else
            {
                return await ProcessWithLegacyAsync(naturalLanguageQuery, userId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in migration service, falling back to legacy processor");
            return await ProcessWithLegacyAsync(naturalLanguageQuery, userId);
        }
    }

    /// <summary>
    /// Process query with A/B testing comparison
    /// </summary>
    public async Task<ProcessedQuery> ProcessQueryWithComparisonAsync(string naturalLanguageQuery, string userId)
    {
        if (!_migrationSettings.EnableComparisonLogging)
        {
            return await ProcessQueryAsync(naturalLanguageQuery, userId);
        }

        try
        {
            var stopwatch = Stopwatch.StartNew();
            
            // Determine primary processor
            var useEnhanced = await ShouldUseEnhancedProcessorAsync(userId);
            
            // Run comparison if enabled
            var shouldCompare = await ShouldRunComparisonAsync();
            
            if (shouldCompare)
            {
                return await RunComparisonAsync(naturalLanguageQuery, userId, useEnhanced);
            }
            else
            {
                return useEnhanced 
                    ? await ProcessWithEnhancedAsync(naturalLanguageQuery, userId)
                    : await ProcessWithLegacyAsync(naturalLanguageQuery, userId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in comparison processing");
            return await ProcessWithLegacyAsync(naturalLanguageQuery, userId);
        }
    }

    /// <summary>
    /// Get migration status and metrics
    /// </summary>
    public async Task<MigrationStatus> GetMigrationStatusAsync()
    {
        try
        {
            var metrics = await _metricsCollector.GetMigrationMetricsAsync();
            
            return new MigrationStatus
            {
                IsEnhancedEnabled = _featureFlags.EnableEnhancedQueryProcessor,
                RolloutPercentage = _featureFlags.RolloutPercentage,
                AdminOnlyMode = _featureFlags.AdminOnlyMode,
                EnabledUserCount = await GetEnabledUserCountAsync(),
                TotalQueries = metrics.TotalQueries,
                EnhancedQueries = metrics.EnhancedQueries,
                LegacyQueries = metrics.LegacyQueries,
                AverageEnhancedPerformance = metrics.AverageEnhancedPerformance,
                AverageLegacyPerformance = metrics.AverageLegacyPerformance,
                EnhancedSuccessRate = metrics.EnhancedSuccessRate,
                LegacySuccessRate = metrics.LegacySuccessRate,
                RecommendedAction = DetermineRecommendedAction(metrics),
                LastUpdated = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting migration status");
            return new MigrationStatus
            {
                IsEnhancedEnabled = false,
                RecommendedAction = "Error retrieving status",
                LastUpdated = DateTime.UtcNow
            };
        }
    }

    /// <summary>
    /// Gradually increase rollout percentage based on performance
    /// </summary>
    public async Task<bool> AutoMigrateAsync()
    {
        if (!_migrationSettings.EnableAutoMigration)
        {
            _logger.LogDebug("Auto-migration is disabled");
            return false;
        }

        try
        {
            var metrics = await _metricsCollector.GetMigrationMetricsAsync();
            var improvement = CalculateImprovement(metrics);

            _logger.LogInformation("Auto-migration check: {Improvement:P2} improvement detected", improvement);

            if (improvement >= _migrationSettings.AutoMigrationThreshold / 100.0)
            {
                var newPercentage = Math.Min(100, _featureFlags.RolloutPercentage + 10);
                await UpdateRolloutPercentageAsync(newPercentage);
                
                _logger.LogInformation("Auto-migration: Increased rollout to {Percentage}%", newPercentage);
                return true;
            }
            else if (improvement < -_migrationSettings.RollbackThreshold / 100.0 && 
                     _migrationSettings.EnableAutoRollback)
            {
                var newPercentage = Math.Max(0, _featureFlags.RolloutPercentage - 20);
                await UpdateRolloutPercentageAsync(newPercentage);
                
                _logger.LogWarning("Auto-rollback: Decreased rollout to {Percentage}% due to performance degradation", newPercentage);
                return true;
            }

            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in auto-migration");
            return false;
        }
    }

    // Private methods

    private async Task<bool> ShouldUseEnhancedProcessorAsync(string userId)
    {
        // Check if enhanced processor is globally enabled
        if (!_featureFlags.EnableEnhancedQueryProcessor)
        {
            return false;
        }

        // Admin-only mode
        if (_featureFlags.AdminOnlyMode)
        {
            return await IsAdminUserAsync(userId);
        }

        // Specific user list
        if (!string.IsNullOrEmpty(_featureFlags.EnabledUsers))
        {
            var enabledUsers = _featureFlags.EnabledUsers.Split(',', StringSplitOptions.RemoveEmptyEntries);
            if (enabledUsers.Contains(userId, StringComparer.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        // Percentage-based rollout
        if (_featureFlags.RolloutPercentage > 0)
        {
            var userHash = userId.GetHashCode();
            var userPercentile = Math.Abs(userHash % 100);
            return userPercentile < _featureFlags.RolloutPercentage;
        }

        return false;
    }

    private async Task<ProcessedQuery> ProcessWithEnhancedAsync(string query, string userId)
    {
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            var result = await _enhancedProcessor.ProcessQueryAsync(query, userId);
            
            // Add migration metadata
            result.Metadata["processor_type"] = "enhanced";
            result.Metadata["processing_time_ms"] = stopwatch.ElapsedMilliseconds;
            
            // Record metrics
            await _metricsCollector.RecordEnhancedQueryAsync(query, userId, stopwatch.ElapsedMilliseconds, true, result.Confidence);
            
            _logger.LogDebug("Enhanced processor completed in {ElapsedMs}ms", stopwatch.ElapsedMilliseconds);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Enhanced processor failed, falling back to legacy");
            
            // Record failure
            await _metricsCollector.RecordEnhancedQueryAsync(query, userId, stopwatch.ElapsedMilliseconds, false, 0);
            
            if (_featureFlags.EnableFallbackOnError)
            {
                return await ProcessWithLegacyAsync(query, userId);
            }
            
            throw;
        }
    }

    private async Task<ProcessedQuery> ProcessWithLegacyAsync(string query, string userId)
    {
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            var result = await _legacyProcessor.ProcessQueryAsync(query, userId);
            
            // Add migration metadata
            result.Metadata["processor_type"] = "legacy";
            result.Metadata["processing_time_ms"] = stopwatch.ElapsedMilliseconds;
            
            // Record metrics
            await _metricsCollector.RecordLegacyQueryAsync(query, userId, stopwatch.ElapsedMilliseconds, true, result.Confidence);
            
            _logger.LogDebug("Legacy processor completed in {ElapsedMs}ms", stopwatch.ElapsedMilliseconds);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Legacy processor failed");
            
            // Record failure
            await _metricsCollector.RecordLegacyQueryAsync(query, userId, stopwatch.ElapsedMilliseconds, false, 0);
            throw;
        }
    }

    private async Task<ProcessedQuery> RunComparisonAsync(string query, string userId, bool primaryIsEnhanced)
    {
        var tasks = new List<Task<(ProcessedQuery Result, string Type, long ElapsedMs, bool Success)>>();
        
        // Primary processor
        if (primaryIsEnhanced)
        {
            tasks.Add(RunProcessorAsync(query, userId, "enhanced", () => _enhancedProcessor.ProcessQueryAsync(query, userId)));
            tasks.Add(RunProcessorAsync(query, userId, "legacy", () => _legacyProcessor.ProcessQueryAsync(query, userId)));
        }
        else
        {
            tasks.Add(RunProcessorAsync(query, userId, "legacy", () => _legacyProcessor.ProcessQueryAsync(query, userId)));
            tasks.Add(RunProcessorAsync(query, userId, "enhanced", () => _enhancedProcessor.ProcessQueryAsync(query, userId)));
        }

        var results = await Task.WhenAll(tasks);
        
        // Log comparison results
        await LogComparisonResultsAsync(query, userId, results);
        
        // Return primary result
        var primaryResult = results[0].Result;
        primaryResult.Metadata["comparison_run"] = true;
        primaryResult.Metadata["comparison_timestamp"] = DateTime.UtcNow;
        
        return primaryResult;
    }

    private async Task<(ProcessedQuery Result, string Type, long ElapsedMs, bool Success)> RunProcessorAsync(
        string query, string userId, string type, Func<Task<ProcessedQuery>> processor)
    {
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            var result = await processor();
            return (result, type, stopwatch.ElapsedMilliseconds, true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Processor {Type} failed during comparison", type);
            
            var fallbackResult = new ProcessedQuery
            {
                OriginalQuery = query,
                Sql = $"-- {type} processor failed",
                Explanation = $"Error in {type} processor",
                Confidence = 0,
                Metadata = new Dictionary<string, object> { ["error"] = true }
            };
            
            return (fallbackResult, type, stopwatch.ElapsedMilliseconds, false);
        }
    }

    private async Task LogComparisonResultsAsync(
        string query, 
        string userId, 
        (ProcessedQuery Result, string Type, long ElapsedMs, bool Success)[] results)
    {
        var enhanced = results.FirstOrDefault(r => r.Type == "enhanced");
        var legacy = results.FirstOrDefault(r => r.Type == "legacy");

        var comparison = new
        {
            Query = query,
            UserId = userId,
            Timestamp = DateTime.UtcNow,
            Enhanced = new
            {
                Success = enhanced.Success,
                ElapsedMs = enhanced.ElapsedMs,
                Confidence = enhanced.Success ? enhanced.Result.Confidence : 0,
                SqlLength = enhanced.Success ? enhanced.Result.Sql.Length : 0
            },
            Legacy = new
            {
                Success = legacy.Success,
                ElapsedMs = legacy.ElapsedMs,
                Confidence = legacy.Success ? legacy.Result.Confidence : 0,
                SqlLength = legacy.Success ? legacy.Result.Sql.Length : 0
            },
            PerformanceImprovement = enhanced.Success && legacy.Success 
                ? (legacy.ElapsedMs - enhanced.ElapsedMs) / (double)legacy.ElapsedMs 
                : 0,
            ConfidenceImprovement = enhanced.Success && legacy.Success 
                ? enhanced.Result.Confidence - legacy.Result.Confidence 
                : 0
        };

        _logger.LogInformation("Comparison result: {Comparison}", System.Text.Json.JsonSerializer.Serialize(comparison));
        
        // Store comparison data for analysis
        await _cacheService.SetAsync($"comparison:{DateTime.UtcNow:yyyyMMddHHmmss}:{userId}", comparison, TimeSpan.FromDays(7));
    }

    private async Task<bool> ShouldRunComparisonAsync()
    {
        if (_migrationSettings.ComparisonPercentage <= 0) return false;
        
        var random = new Random();
        return random.Next(100) < _migrationSettings.ComparisonPercentage;
    }

    private async Task<bool> IsAdminUserAsync(string userId)
    {
        // This would check against your user management system
        // For now, return false as placeholder
        return false;
    }

    private async Task<int> GetEnabledUserCountAsync()
    {
        if (string.IsNullOrEmpty(_featureFlags.EnabledUsers)) return 0;
        
        return _featureFlags.EnabledUsers.Split(',', StringSplitOptions.RemoveEmptyEntries).Length;
    }

    private string DetermineRecommendedAction(MigrationMetrics metrics)
    {
        var improvement = CalculateImprovement(metrics);
        
        if (improvement > 0.15) return "Increase rollout percentage";
        if (improvement < -0.10) return "Consider rollback";
        if (metrics.EnhancedSuccessRate < 0.95) return "Monitor enhanced processor stability";
        if (_featureFlags.RolloutPercentage < 100 && improvement > 0.05) return "Continue gradual rollout";
        
        return "Maintain current settings";
    }

    private double CalculateImprovement(MigrationMetrics metrics)
    {
        if (metrics.AverageLegacyPerformance <= 0) return 0;
        
        var performanceImprovement = (metrics.AverageLegacyPerformance - metrics.AverageEnhancedPerformance) / metrics.AverageLegacyPerformance;
        var successRateImprovement = metrics.EnhancedSuccessRate - metrics.LegacySuccessRate;
        
        // Weighted combination: 70% performance, 30% success rate
        return (performanceImprovement * 0.7) + (successRateImprovement * 0.3);
    }

    private async Task UpdateRolloutPercentageAsync(int newPercentage)
    {
        // This would update the configuration
        // For now, just log the change
        _logger.LogInformation("Rollout percentage would be updated to {Percentage}%", newPercentage);
    }
}
