using Microsoft.Extensions.Logging;
using BIReportingCopilot.Infrastructure.Performance;

namespace BIReportingCopilot.Infrastructure.Migration;

/// <summary>
/// Migration status information
/// </summary>
public class MigrationStatus
{
    public bool IsEnhancedEnabled { get; set; }
    public int RolloutPercentage { get; set; }
    public bool AdminOnlyMode { get; set; }
    public int EnabledUserCount { get; set; }
    public long TotalQueries { get; set; }
    public long EnhancedQueries { get; set; }
    public long LegacyQueries { get; set; }
    public double AverageEnhancedPerformance { get; set; }
    public double AverageLegacyPerformance { get; set; }
    public double EnhancedSuccessRate { get; set; }
    public double LegacySuccessRate { get; set; }
    public string RecommendedAction { get; set; } = string.Empty;
    public DateTime LastUpdated { get; set; }
}

/// <summary>
/// Migration metrics for analysis
/// </summary>
public class MigrationMetrics
{
    public long TotalQueries { get; set; }
    public long EnhancedQueries { get; set; }
    public long LegacyQueries { get; set; }
    public double AverageEnhancedPerformance { get; set; }
    public double AverageLegacyPerformance { get; set; }
    public double EnhancedSuccessRate { get; set; }
    public double LegacySuccessRate { get; set; }
    public double AverageEnhancedConfidence { get; set; }
    public double AverageLegacyConfidence { get; set; }
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
}

/// <summary>
/// Collects and analyzes migration metrics
/// </summary>
public class MigrationMetricsCollector
{
    private readonly ILogger _logger;
    private readonly ICacheService _cacheService;
    private readonly string _metricsKeyPrefix = "migration_metrics";

    public MigrationMetricsCollector(ILogger logger, ICacheService cacheService)
    {
        _logger = logger;
        _cacheService = cacheService;
    }

    /// <summary>
    /// Record enhanced processor query execution
    /// </summary>
    public async Task RecordEnhancedQueryAsync(string query, string userId, long elapsedMs, bool success, double confidence)
    {
        try
        {
            var metric = new QueryMetric
            {
                Query = query,
                UserId = userId,
                ProcessorType = "enhanced",
                ElapsedMs = elapsedMs,
                Success = success,
                Confidence = confidence,
                Timestamp = DateTime.UtcNow
            };

            await StoreMetricAsync(metric);
            _logger.LogDebug("Recorded enhanced query metric: {ElapsedMs}ms, Success: {Success}", elapsedMs, success);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recording enhanced query metric");
        }
    }

    /// <summary>
    /// Record legacy processor query execution
    /// </summary>
    public async Task RecordLegacyQueryAsync(string query, string userId, long elapsedMs, bool success, double confidence)
    {
        try
        {
            var metric = new QueryMetric
            {
                Query = query,
                UserId = userId,
                ProcessorType = "legacy",
                ElapsedMs = elapsedMs,
                Success = success,
                Confidence = confidence,
                Timestamp = DateTime.UtcNow
            };

            await StoreMetricAsync(metric);
            _logger.LogDebug("Recorded legacy query metric: {ElapsedMs}ms, Success: {Success}", elapsedMs, success);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recording legacy query metric");
        }
    }

    /// <summary>
    /// Get aggregated migration metrics for analysis
    /// </summary>
    public async Task<MigrationMetrics> GetMigrationMetricsAsync(TimeSpan? period = null)
    {
        try
        {
            var lookbackPeriod = period ?? TimeSpan.FromDays(7);
            var startTime = DateTime.UtcNow - lookbackPeriod;
            
            var metrics = await GetMetricsInPeriodAsync(startTime, DateTime.UtcNow);
            
            var enhancedMetrics = metrics.Where(m => m.ProcessorType == "enhanced").ToList();
            var legacyMetrics = metrics.Where(m => m.ProcessorType == "legacy").ToList();

            return new MigrationMetrics
            {
                TotalQueries = metrics.Count,
                EnhancedQueries = enhancedMetrics.Count,
                LegacyQueries = legacyMetrics.Count,
                AverageEnhancedPerformance = enhancedMetrics.Any() ? enhancedMetrics.Average(m => m.ElapsedMs) : 0,
                AverageLegacyPerformance = legacyMetrics.Any() ? legacyMetrics.Average(m => m.ElapsedMs) : 0,
                EnhancedSuccessRate = enhancedMetrics.Any() ? enhancedMetrics.Count(m => m.Success) / (double)enhancedMetrics.Count : 0,
                LegacySuccessRate = legacyMetrics.Any() ? legacyMetrics.Count(m => m.Success) / (double)legacyMetrics.Count : 0,
                AverageEnhancedConfidence = enhancedMetrics.Where(m => m.Success).Any() ? enhancedMetrics.Where(m => m.Success).Average(m => m.Confidence) : 0,
                AverageLegacyConfidence = legacyMetrics.Where(m => m.Success).Any() ? legacyMetrics.Where(m => m.Success).Average(m => m.Confidence) : 0,
                PeriodStart = startTime,
                PeriodEnd = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting migration metrics");
            return new MigrationMetrics();
        }
    }

    /// <summary>
    /// Get detailed comparison report
    /// </summary>
    public async Task<ComparisonReport> GetComparisonReportAsync(TimeSpan? period = null)
    {
        try
        {
            var metrics = await GetMigrationMetricsAsync(period);
            var lookbackPeriod = period ?? TimeSpan.FromDays(7);
            
            return new ComparisonReport
            {
                Period = lookbackPeriod,
                TotalQueries = metrics.TotalQueries,
                EnhancedQueries = metrics.EnhancedQueries,
                LegacyQueries = metrics.LegacyQueries,
                PerformanceImprovement = CalculatePerformanceImprovement(metrics),
                ConfidenceImprovement = metrics.AverageEnhancedConfidence - metrics.AverageLegacyConfidence,
                SuccessRateImprovement = metrics.EnhancedSuccessRate - metrics.LegacySuccessRate,
                Recommendation = GenerateRecommendation(metrics),
                GeneratedAt = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating comparison report");
            return new ComparisonReport
            {
                Recommendation = "Error generating report",
                GeneratedAt = DateTime.UtcNow
            };
        }
    }

    /// <summary>
    /// Clean up old metrics to prevent storage bloat
    /// </summary>
    public async Task CleanupOldMetricsAsync(TimeSpan retentionPeriod)
    {
        try
        {
            var cutoffTime = DateTime.UtcNow - retentionPeriod;
            var keysToRemove = new List<string>();

            // This is a simplified cleanup - in practice, you'd need to iterate through stored keys
            // and remove those older than the cutoff time
            
            _logger.LogInformation("Cleaned up metrics older than {CutoffTime}", cutoffTime);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cleaning up old metrics");
        }
    }

    // Private methods

    private async Task StoreMetricAsync(QueryMetric metric)
    {
        var key = $"{_metricsKeyPrefix}:{DateTime.UtcNow:yyyyMMddHHmmss}:{Guid.NewGuid():N}";
        await _cacheService.SetAsync(key, metric, TimeSpan.FromDays(30)); // Keep metrics for 30 days
    }

    private async Task<List<QueryMetric>> GetMetricsInPeriodAsync(DateTime startTime, DateTime endTime)
    {
        // This is a simplified implementation
        // In practice, you'd need a more efficient way to query metrics by time range
        var metrics = new List<QueryMetric>();
        
        // For now, return empty list as we don't have a way to efficiently query cache by pattern
        // In a real implementation, you'd use a database or time-series database for metrics
        
        return metrics;
    }

    private double CalculatePerformanceImprovement(MigrationMetrics metrics)
    {
        if (metrics.AverageLegacyPerformance <= 0) return 0;
        
        return (metrics.AverageLegacyPerformance - metrics.AverageEnhancedPerformance) / metrics.AverageLegacyPerformance;
    }

    private string GenerateRecommendation(MigrationMetrics metrics)
    {
        var performanceImprovement = CalculatePerformanceImprovement(metrics);
        var confidenceImprovement = metrics.AverageEnhancedConfidence - metrics.AverageLegacyConfidence;
        var successRateImprovement = metrics.EnhancedSuccessRate - metrics.LegacySuccessRate;

        if (performanceImprovement > 0.2 && confidenceImprovement > 0.1 && successRateImprovement >= 0)
        {
            return "Strong improvement detected. Recommend increasing rollout percentage.";
        }
        else if (performanceImprovement > 0.1 && successRateImprovement >= 0)
        {
            return "Moderate improvement detected. Continue gradual rollout.";
        }
        else if (successRateImprovement < -0.05 || performanceImprovement < -0.1)
        {
            return "Performance degradation detected. Consider rollback or investigation.";
        }
        else
        {
            return "Mixed results. Continue monitoring before making changes.";
        }
    }
}

/// <summary>
/// Individual query execution metric
/// </summary>
public class QueryMetric
{
    public string Query { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string ProcessorType { get; set; } = string.Empty; // "enhanced" or "legacy"
    public long ElapsedMs { get; set; }
    public bool Success { get; set; }
    public double Confidence { get; set; }
    public DateTime Timestamp { get; set; }
}

/// <summary>
/// Detailed comparison report
/// </summary>
public class ComparisonReport
{
    public TimeSpan Period { get; set; }
    public long TotalQueries { get; set; }
    public long EnhancedQueries { get; set; }
    public long LegacyQueries { get; set; }
    public double PerformanceImprovement { get; set; }
    public double ConfidenceImprovement { get; set; }
    public double SuccessRateImprovement { get; set; }
    public string Recommendation { get; set; } = string.Empty;
    public DateTime GeneratedAt { get; set; }
}

/// <summary>
/// Migration phase definitions
/// </summary>
public enum MigrationPhase
{
    Planning,
    AdminTesting,
    LimitedRollout,
    GradualRollout,
    FullRollout,
    Complete,
    Rollback
}

/// <summary>
/// Migration plan with phases and criteria
/// </summary>
public class MigrationPlan
{
    public MigrationPhase CurrentPhase { get; set; }
    public List<MigrationPhaseDefinition> Phases { get; set; } = new();
    public DateTime StartDate { get; set; }
    public DateTime? EstimatedCompletionDate { get; set; }
    public List<string> CompletedMilestones { get; set; } = new();
    public List<string> RemainingMilestones { get; set; } = new();
}

/// <summary>
/// Definition of a migration phase
/// </summary>
public class MigrationPhaseDefinition
{
    public MigrationPhase Phase { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int RolloutPercentage { get; set; }
    public TimeSpan MinimumDuration { get; set; }
    public List<string> SuccessCriteria { get; set; } = new();
    public List<string> ExitCriteria { get; set; } = new();
    public bool IsCompleted { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? CompletionDate { get; set; }
}
