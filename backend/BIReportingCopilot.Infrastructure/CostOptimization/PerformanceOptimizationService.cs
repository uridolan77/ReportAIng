using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using BIReportingCopilot.Core.Interfaces.CostOptimization;
using BIReportingCopilot.Core.Models;
using BIReportingCopilot.Infrastructure.Data;
using BIReportingCopilot.Infrastructure.Data.Entities;

namespace BIReportingCopilot.Infrastructure.CostOptimization;

/// <summary>
/// Performance optimization engine for analysis, monitoring, and automatic tuning
/// </summary>
public class PerformanceOptimizationService : IPerformanceOptimizationService
{
    private readonly BICopilotContext _context;
    private readonly ILogger<PerformanceOptimizationService> _logger;
    private readonly Dictionary<string, List<PerformanceDataPoint>> _performanceHistory = new();
    private readonly Dictionary<string, PerformanceBenchmark> _benchmarks = new();
    private readonly Dictionary<string, DateTime> _lastOptimizationRun = new();
    private readonly TimeSpan _optimizationCooldown = TimeSpan.FromHours(1);

    public PerformanceOptimizationService(
        BICopilotContext context,
        ILogger<PerformanceOptimizationService> logger)
    {
        _context = context;
        _logger = logger;
        InitializeDefaultBenchmarks();
    }

    #region Performance Analysis

    public async Task<PerformanceMetrics> AnalyzePerformanceAsync(string entityId, string entityType, CancellationToken cancellationToken = default)
    {
        try
        {
            var key = $"{entityType}:{entityId}";
            var metrics = new PerformanceMetrics();

            // Get recent performance data
            var recentData = await GetRecentPerformanceDataAsync(entityId, entityType, TimeSpan.FromDays(7), cancellationToken);
            
            if (recentData.Any())
            {
                metrics.AverageResponseTime = recentData.Average(d => d.ResponseTimeMs);
                metrics.ThroughputPerSecond = CalculateThroughput(recentData);
                metrics.ErrorRate = CalculateErrorRate(recentData);
                metrics.P95ResponseTime = CalculatePercentile(recentData.Select(d => d.ResponseTimeMs), 0.95);
                metrics.P99ResponseTime = CalculatePercentile(recentData.Select(d => d.ResponseTimeMs), 0.99);
                metrics.TotalRequests = recentData.Count;
                metrics.SuccessfulRequests = recentData.Count(d => d.IsSuccess);
                metrics.FailedRequests = recentData.Count(d => !d.IsSuccess);
            }

            // Calculate performance score
            metrics.PerformanceScore = CalculatePerformanceScore(metrics);
            metrics.LastAnalyzed = DateTime.UtcNow;

            _logger.LogInformation("Analyzed performance for {EntityType}:{EntityId} - Score: {Score:F2}", 
                entityType, entityId, metrics.PerformanceScore);

            return metrics;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing performance for {EntityType}:{EntityId}", entityType, entityId);
            return new PerformanceMetrics { LastAnalyzed = DateTime.UtcNow };
        }
    }

    public async Task<List<PerformanceBottleneck>> IdentifyBottlenecksAsync(string entityId, string entityType, CancellationToken cancellationToken = default)
    {
        try
        {
            var bottlenecks = new List<PerformanceBottleneck>();
            var recentData = await GetRecentPerformanceDataAsync(entityId, entityType, TimeSpan.FromDays(1), cancellationToken);

            if (!recentData.Any())
                return bottlenecks;

            // Identify slow operations
            var slowOperations = recentData
                .Where(d => d.ResponseTimeMs > 5000) // > 5 seconds
                .GroupBy(d => d.OperationType)
                .Select(g => new PerformanceBottleneck
                {
                    Id = Guid.NewGuid().ToString(),
                    Type = "SlowOperation",
                    EntityId = entityId,
                    EntityType = entityType,
                    Description = $"Slow {g.Key} operations detected",
                    Severity = g.Average(d => d.ResponseTimeMs) > 10000 ? "High" : "Medium",
                    ImpactScore = CalculateImpactScore(g.ToList()),
                    Recommendations = GenerateSlowOperationRecommendations(g.Key, g.Average(d => d.ResponseTimeMs)),
                    DetectedAt = DateTime.UtcNow
                })
                .ToList();

            bottlenecks.AddRange(slowOperations);

            // Identify high error rates
            var errorRateByOperation = recentData
                .GroupBy(d => d.OperationType)
                .Where(g => g.Count(d => !d.IsSuccess) / (double)g.Count() > 0.05) // > 5% error rate
                .Select(g => new PerformanceBottleneck
                {
                    Id = Guid.NewGuid().ToString(),
                    Type = "HighErrorRate",
                    EntityId = entityId,
                    EntityType = entityType,
                    Description = $"High error rate in {g.Key} operations",
                    Severity = "High",
                    ImpactScore = g.Count(d => !d.IsSuccess) / (double)g.Count(),
                    Recommendations = GenerateErrorRateRecommendations(g.Key),
                    DetectedAt = DateTime.UtcNow
                })
                .ToList();

            bottlenecks.AddRange(errorRateByOperation);

            // Identify memory issues
            var memoryBottlenecks = await IdentifyMemoryBottlenecksAsync(entityId, entityType, recentData, cancellationToken);
            bottlenecks.AddRange(memoryBottlenecks);

            _logger.LogInformation("Identified {Count} performance bottlenecks for {EntityType}:{EntityId}", 
                bottlenecks.Count, entityType, entityId);

            return bottlenecks;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error identifying bottlenecks for {EntityType}:{EntityId}", entityType, entityId);
            return new List<PerformanceBottleneck>();
        }
    }

    public async Task<List<PerformanceOptimizationSuggestion>> GetOptimizationSuggestionsAsync(string entityId, string entityType, CancellationToken cancellationToken = default)
    {
        try
        {
            var suggestions = new List<PerformanceOptimizationSuggestion>();
            var bottlenecks = await IdentifyBottlenecksAsync(entityId, entityType, cancellationToken);
            var metrics = await AnalyzePerformanceAsync(entityId, entityType, cancellationToken);

            // Generate suggestions based on bottlenecks
            foreach (var bottleneck in bottlenecks)
            {
                var suggestion = new PerformanceOptimizationSuggestion
                {
                    Id = Guid.NewGuid().ToString(),
                    Type = $"Fix{bottleneck.Type}",
                    Title = $"Optimize {bottleneck.Type}",
                    Description = $"Address {bottleneck.Description.ToLower()}",
                    EstimatedImprovement = EstimateImprovement(bottleneck),
                    Priority = bottleneck.Severity == "High" ? "High" : "Medium",
                    Implementation = GenerateImplementationSteps(bottleneck),
                    Benefits = bottleneck.Recommendations,
                    Requirements = GenerateRequirements(bottleneck),
                    CreatedAt = DateTime.UtcNow,
                    IsImplemented = false
                };

                suggestions.Add(suggestion);
            }

            // Generate general optimization suggestions
            if (metrics.PerformanceScore < 0.7)
            {
                suggestions.Add(new PerformanceOptimizationSuggestion
                {
                    Id = Guid.NewGuid().ToString(),
                    Type = "GeneralOptimization",
                    Title = "General Performance Optimization",
                    Description = "Implement general performance improvements",
                    EstimatedImprovement = 0.2,
                    Priority = "Medium",
                    Implementation = "Review and optimize database queries, implement caching, optimize algorithms",
                    Benefits = new List<string> { "Improved response times", "Better user experience", "Reduced resource usage" },
                    Requirements = new List<string> { "Performance analysis", "Code review", "Testing" },
                    CreatedAt = DateTime.UtcNow,
                    IsImplemented = false
                });
            }

            _logger.LogInformation("Generated {Count} optimization suggestions for {EntityType}:{EntityId}", 
                suggestions.Count, entityType, entityId);

            return suggestions;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting optimization suggestions for {EntityType}:{EntityId}", entityType, entityId);
            return new List<PerformanceOptimizationSuggestion>();
        }
    }

    #endregion

    #region Performance Benchmarking

    public async Task<PerformanceBenchmark> CreateBenchmarkAsync(string name, string category, double value, string unit, CancellationToken cancellationToken = default)
    {
        try
        {
            var benchmark = new PerformanceBenchmark
            {
                Id = Guid.NewGuid().ToString(),
                Name = name,
                Category = category,
                BaselineValue = value,
                CurrentValue = value,
                Unit = unit,
                BaselineDate = DateTime.UtcNow,
                MeasurementDate = DateTime.UtcNow,
                Metadata = new Dictionary<string, object>
                {
                    ["created_by"] = "system",
                    ["version"] = "1.0"
                }
            };

            _benchmarks[benchmark.Id] = benchmark;

            _logger.LogInformation("Created performance benchmark: {Name} = {Value} {Unit}", 
                name, value, unit);

            return benchmark;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating benchmark");
            throw;
        }
    }

    public async Task<PerformanceBenchmark> UpdateBenchmarkAsync(string benchmarkId, double newValue, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!_benchmarks.TryGetValue(benchmarkId, out var benchmark))
                throw new ArgumentException($"Benchmark not found: {benchmarkId}");

            benchmark.CurrentValue = newValue;
            benchmark.MeasurementDate = DateTime.UtcNow;

            _logger.LogInformation("Updated benchmark {Name}: {OldValue} -> {NewValue} {Unit} (Improvement: {Improvement:P})", 
                benchmark.Name, benchmark.BaselineValue, newValue, benchmark.Unit, benchmark.ImprovementPercentage / 100);

            return benchmark;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating benchmark: {BenchmarkId}", benchmarkId);
            throw;
        }
    }

    public async Task<List<PerformanceBenchmark>> GetBenchmarksAsync(string? category = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var benchmarks = _benchmarks.Values.ToList();

            if (!string.IsNullOrEmpty(category))
            {
                benchmarks = benchmarks
                    .Where(b => b.Category.Equals(category, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            return benchmarks.OrderBy(b => b.Category).ThenBy(b => b.Name).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting benchmarks");
            return new List<PerformanceBenchmark>();
        }
    }

    public async Task<Dictionary<string, double>> GetPerformanceImprovementsAsync(DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var improvements = new Dictionary<string, double>();
            var benchmarks = await GetBenchmarksAsync(cancellationToken: cancellationToken);

            foreach (var benchmark in benchmarks)
            {
                if (startDate.HasValue && benchmark.MeasurementDate < startDate.Value)
                    continue;
                if (endDate.HasValue && benchmark.MeasurementDate > endDate.Value)
                    continue;

                improvements[benchmark.Name] = benchmark.ImprovementPercentage;
            }

            return improvements;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting performance improvements");
            return new Dictionary<string, double>();
        }
    }

    #endregion

    #region Performance Monitoring

    public async Task RecordPerformanceMetricAsync(PerformanceMetricsEntry metric, CancellationToken cancellationToken = default)
    {
        try
        {
            var entity = new PerformanceMetricsEntity
            {
                MetricName = metric.MetricName,
                Category = metric.Category,
                Value = metric.Value,
                Unit = metric.Unit,
                Timestamp = metric.Timestamp,
                EntityId = metric.EntityId,
                EntityType = metric.EntityType,
                Tags = JsonSerializer.Serialize(metric.Tags),
                CreatedDate = DateTime.UtcNow,
                CreatedBy = "system"
            };

            _context.PerformanceMetricsEntries.Add(entity);
            await _context.SaveChangesAsync(cancellationToken);

            // Also store in memory for quick access
            var key = $"{metric.EntityType}:{metric.EntityId}";
            if (!_performanceHistory.ContainsKey(key))
            {
                _performanceHistory[key] = new List<PerformanceDataPoint>();
            }

            _performanceHistory[key].Add(new PerformanceDataPoint
            {
                Timestamp = metric.Timestamp,
                ResponseTimeMs = metric.Value,
                OperationType = metric.MetricName,
                IsSuccess = !metric.Tags.ContainsKey("error") || !Convert.ToBoolean(metric.Tags["error"])
            });

            // Keep only recent data in memory (last 1000 entries)
            if (_performanceHistory[key].Count > 1000)
            {
                _performanceHistory[key] = _performanceHistory[key]
                    .OrderByDescending(d => d.Timestamp)
                    .Take(1000)
                    .ToList();
            }

            _logger.LogDebug("Recorded performance metric: {MetricName} = {Value} {Unit}",
                metric.MetricName, metric.Value, metric.Unit);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recording performance metric");
        }
    }

    public async Task<List<PerformanceMetricsEntry>> GetPerformanceMetricsAsync(string metricName, DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var query = _context.PerformanceMetricsEntries
                .Where(m => m.MetricName == metricName);

            if (startDate.HasValue)
                query = query.Where(m => m.Timestamp >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(m => m.Timestamp <= endDate.Value);

            var entities = await query
                .OrderByDescending(m => m.Timestamp)
                .Take(1000)
                .ToListAsync(cancellationToken);

            return entities.Select(MapToPerformanceMetricsEntry).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting performance metrics");
            return new List<PerformanceMetricsEntry>();
        }
    }

    public async Task<Dictionary<string, double>> GetAggregatedMetricsAsync(string metricName, string aggregationType, TimeSpan period, CancellationToken cancellationToken = default)
    {
        try
        {
            var startDate = DateTime.UtcNow.Subtract(period);
            var metrics = await GetPerformanceMetricsAsync(metricName, startDate, null, cancellationToken);

            if (!metrics.Any())
                return new Dictionary<string, double>();

            var result = new Dictionary<string, double>();

            switch (aggregationType.ToLower())
            {
                case "average":
                    result["value"] = metrics.Average(m => m.Value);
                    break;
                case "sum":
                    result["value"] = metrics.Sum(m => m.Value);
                    break;
                case "min":
                    result["value"] = metrics.Min(m => m.Value);
                    break;
                case "max":
                    result["value"] = metrics.Max(m => m.Value);
                    break;
                case "count":
                    result["value"] = metrics.Count;
                    break;
                case "percentile":
                    result["p50"] = CalculatePercentile(metrics.Select(m => m.Value), 0.5);
                    result["p95"] = CalculatePercentile(metrics.Select(m => m.Value), 0.95);
                    result["p99"] = CalculatePercentile(metrics.Select(m => m.Value), 0.99);
                    break;
                default:
                    result["value"] = metrics.Average(m => m.Value);
                    break;
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting aggregated metrics");
            return new Dictionary<string, double>();
        }
    }

    #endregion

    #region Automatic Performance Tuning

    public async Task<TuningResult> AutoTunePerformanceAsync(string entityId, string entityType, CancellationToken cancellationToken = default)
    {
        try
        {
            var key = $"{entityType}:{entityId}";

            // Check cooldown period
            if (_lastOptimizationRun.TryGetValue(key, out var lastRun) &&
                DateTime.UtcNow - lastRun < _optimizationCooldown)
            {
                return new TuningResult
                {
                    Id = Guid.NewGuid().ToString(),
                    EntityId = entityId,
                    EntityType = entityType,
                    Success = false,
                    Message = "Optimization is in cooldown period",
                    ExecutedAt = DateTime.UtcNow
                };
            }

            var metrics = await AnalyzePerformanceAsync(entityId, entityType, cancellationToken);
            var suggestions = await GetOptimizationSuggestionsAsync(entityId, entityType, cancellationToken);

            var appliedOptimizations = new List<string>();
            var improvementScore = 0.0;

            // Apply automatic optimizations
            foreach (var suggestion in suggestions.Where(s => s.Priority == "High"))
            {
                var applied = await ApplyAutomaticOptimizationAsync(suggestion, cancellationToken);
                if (applied)
                {
                    appliedOptimizations.Add(suggestion.Title);
                    improvementScore += suggestion.EstimatedImprovement;
                }
            }

            _lastOptimizationRun[key] = DateTime.UtcNow;

            var result = new TuningResult
            {
                Id = Guid.NewGuid().ToString(),
                EntityId = entityId,
                EntityType = entityType,
                Success = appliedOptimizations.Any(),
                Message = appliedOptimizations.Any() ?
                    $"Applied {appliedOptimizations.Count} optimizations" :
                    "No automatic optimizations available",
                ImprovementScore = improvementScore,
                OptimizationSteps = appliedOptimizations,
                ExecutedAt = DateTime.UtcNow,
                Metadata = new Dictionary<string, object>
                {
                    ["baseline_score"] = metrics.PerformanceScore,
                    ["suggestions_count"] = suggestions.Count,
                    ["applied_count"] = appliedOptimizations.Count
                }
            };

            _logger.LogInformation("Auto-tuned performance for {EntityType}:{EntityId} - Applied {Count} optimizations",
                entityType, entityId, appliedOptimizations.Count);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error auto-tuning performance for {EntityType}:{EntityId}", entityType, entityId);
            return new TuningResult
            {
                Id = Guid.NewGuid().ToString(),
                EntityId = entityId,
                EntityType = entityType,
                Success = false,
                Message = $"Error during auto-tuning: {ex.Message}",
                ExecutedAt = DateTime.UtcNow
            };
        }
    }

    public async Task<List<TuningResult>> GetTuningHistoryAsync(string entityId, string entityType, CancellationToken cancellationToken = default)
    {
        try
        {
            // This would typically query a database table
            // For now, return mock data
            return new List<TuningResult>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting tuning history");
            return new List<TuningResult>();
        }
    }

    public async Task<bool> ApplyPerformanceOptimizationAsync(string optimizationId, CancellationToken cancellationToken = default)
    {
        try
        {
            // This would implement specific optimization logic
            // For now, just log and return success
            _logger.LogInformation("Applied performance optimization: {OptimizationId}", optimizationId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error applying performance optimization: {OptimizationId}", optimizationId);
            return false;
        }
    }

    #endregion

    #region Performance Alerting

    public async Task<List<ResourceMonitoringAlert>> GetActivePerformanceAlertsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            // Generate alerts based on performance thresholds
            var alerts = new List<ResourceMonitoringAlert>();

            foreach (var kvp in _performanceHistory)
            {
                var recentData = kvp.Value
                    .Where(d => d.Timestamp >= DateTime.UtcNow.AddMinutes(-15))
                    .ToList();

                if (!recentData.Any()) continue;

                var avgResponseTime = recentData.Average(d => d.ResponseTimeMs);
                var errorRate = recentData.Count(d => !d.IsSuccess) / (double)recentData.Count;

                // High response time alert
                if (avgResponseTime > 10000) // > 10 seconds
                {
                    alerts.Add(new ResourceMonitoringAlert
                    {
                        Id = Guid.NewGuid().ToString(),
                        AlertType = "HighResponseTime",
                        ResourceType = "Performance",
                        Severity = avgResponseTime > 30000 ? "High" : "Medium",
                        Message = $"High response time detected: {avgResponseTime:F0}ms average",
                        Threshold = 10000,
                        CurrentValue = avgResponseTime,
                        TriggeredAt = DateTime.UtcNow,
                        IsResolved = false,
                        Metadata = new Dictionary<string, object>
                        {
                            ["entity_key"] = kvp.Key,
                            ["sample_count"] = recentData.Count
                        }
                    });
                }

                // High error rate alert
                if (errorRate > 0.1) // > 10% error rate
                {
                    alerts.Add(new ResourceMonitoringAlert
                    {
                        Id = Guid.NewGuid().ToString(),
                        AlertType = "HighErrorRate",
                        ResourceType = "Performance",
                        Severity = "High",
                        Message = $"High error rate detected: {errorRate:P}",
                        Threshold = 0.1,
                        CurrentValue = errorRate,
                        TriggeredAt = DateTime.UtcNow,
                        IsResolved = false,
                        Metadata = new Dictionary<string, object>
                        {
                            ["entity_key"] = kvp.Key,
                            ["error_count"] = recentData.Count(d => !d.IsSuccess),
                            ["total_count"] = recentData.Count
                        }
                    });
                }
            }

            return alerts;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting active performance alerts");
            return new List<ResourceMonitoringAlert>();
        }
    }

    public async Task<ResourceMonitoringAlert> CreatePerformanceAlertAsync(ResourceMonitoringAlert alert, CancellationToken cancellationToken = default)
    {
        try
        {
            alert.Id = Guid.NewGuid().ToString();
            alert.TriggeredAt = DateTime.UtcNow;

            _logger.LogInformation("Created performance alert: {AlertType} - {Message}", alert.AlertType, alert.Message);

            return alert;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating performance alert");
            throw;
        }
    }

    public async Task<bool> ResolvePerformanceAlertAsync(string alertId, string resolutionNotes, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Resolved performance alert: {AlertId} - {Notes}", alertId, resolutionNotes);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resolving performance alert: {AlertId}", alertId);
            return false;
        }
    }

    #endregion

    #region Helper Methods

    private void InitializeDefaultBenchmarks()
    {
        var defaultBenchmarks = new[]
        {
            new PerformanceBenchmark
            {
                Id = "query_response_time",
                Name = "Query Response Time",
                Category = "Database",
                BaselineValue = 2000,
                CurrentValue = 2000,
                Unit = "ms",
                BaselineDate = DateTime.UtcNow,
                MeasurementDate = DateTime.UtcNow
            },
            new PerformanceBenchmark
            {
                Id = "ai_generation_time",
                Name = "AI Generation Time",
                Category = "AI",
                BaselineValue = 5000,
                CurrentValue = 5000,
                Unit = "ms",
                BaselineDate = DateTime.UtcNow,
                MeasurementDate = DateTime.UtcNow
            },
            new PerformanceBenchmark
            {
                Id = "cache_hit_rate",
                Name = "Cache Hit Rate",
                Category = "Cache",
                BaselineValue = 0.8,
                CurrentValue = 0.8,
                Unit = "ratio",
                BaselineDate = DateTime.UtcNow,
                MeasurementDate = DateTime.UtcNow
            }
        };

        foreach (var benchmark in defaultBenchmarks)
        {
            _benchmarks[benchmark.Id] = benchmark;
        }
    }

    private async Task<List<PerformanceDataPoint>> GetRecentPerformanceDataAsync(string entityId, string entityType, TimeSpan timeWindow, CancellationToken cancellationToken)
    {
        var key = $"{entityType}:{entityId}";
        var cutoff = DateTime.UtcNow.Subtract(timeWindow);

        if (_performanceHistory.TryGetValue(key, out var data))
        {
            return data.Where(d => d.Timestamp >= cutoff).ToList();
        }

        return new List<PerformanceDataPoint>();
    }

    private static double CalculateThroughput(List<PerformanceDataPoint> data)
    {
        if (!data.Any()) return 0;

        var timeSpan = data.Max(d => d.Timestamp) - data.Min(d => d.Timestamp);
        return timeSpan.TotalSeconds > 0 ? data.Count / timeSpan.TotalSeconds : 0;
    }

    private static double CalculateErrorRate(List<PerformanceDataPoint> data)
    {
        if (!data.Any()) return 0;
        return data.Count(d => !d.IsSuccess) / (double)data.Count;
    }

    private static double CalculatePercentile(IEnumerable<double> values, double percentile)
    {
        var sorted = values.OrderBy(x => x).ToList();
        if (!sorted.Any()) return 0;

        var index = (int)Math.Ceiling(percentile * sorted.Count) - 1;
        return sorted[Math.Max(0, Math.Min(index, sorted.Count - 1))];
    }

    private static double CalculatePerformanceScore(PerformanceMetrics metrics)
    {
        var score = 1.0;

        // Penalize high response times
        if (metrics.AverageResponseTime > 5000)
            score -= 0.3;
        else if (metrics.AverageResponseTime > 2000)
            score -= 0.1;

        // Penalize high error rates
        if (metrics.ErrorRate > 0.1)
            score -= 0.4;
        else if (metrics.ErrorRate > 0.05)
            score -= 0.2;

        // Penalize low throughput
        if (metrics.ThroughputPerSecond < 1)
            score -= 0.2;

        return Math.Max(0, score);
    }

    private static double CalculateImpactScore(List<PerformanceDataPoint> data)
    {
        if (!data.Any()) return 0;

        var avgResponseTime = data.Average(d => d.ResponseTimeMs);
        var errorRate = data.Count(d => !d.IsSuccess) / (double)data.Count;

        // Impact score based on response time and error rate
        var timeImpact = Math.Min(1.0, avgResponseTime / 10000.0); // Normalize to 10 seconds
        var errorImpact = Math.Min(1.0, errorRate * 10); // Amplify error rate impact

        return (timeImpact + errorImpact) / 2;
    }

    private static List<string> GenerateSlowOperationRecommendations(string operationType, double avgResponseTime)
    {
        var recommendations = new List<string>();

        if (operationType.Contains("Query", StringComparison.OrdinalIgnoreCase))
        {
            recommendations.Add("Optimize database queries with proper indexing");
            recommendations.Add("Consider query result caching");
            recommendations.Add("Review and optimize WHERE clauses");
        }
        else if (operationType.Contains("AI", StringComparison.OrdinalIgnoreCase))
        {
            recommendations.Add("Consider using faster AI models for simple queries");
            recommendations.Add("Implement request batching");
            recommendations.Add("Add response caching for similar requests");
        }
        else
        {
            recommendations.Add("Profile the operation to identify bottlenecks");
            recommendations.Add("Consider asynchronous processing");
            recommendations.Add("Implement caching where appropriate");
        }

        if (avgResponseTime > 30000)
        {
            recommendations.Add("Consider breaking down the operation into smaller chunks");
            recommendations.Add("Implement timeout and retry mechanisms");
        }

        return recommendations;
    }

    private static List<string> GenerateErrorRateRecommendations(string operationType)
    {
        return new List<string>
        {
            "Implement better error handling and retry logic",
            "Add input validation to prevent errors",
            "Monitor and fix underlying service issues",
            "Implement circuit breaker pattern",
            "Add comprehensive logging for error analysis"
        };
    }

    private async Task<List<PerformanceBottleneck>> IdentifyMemoryBottlenecksAsync(string entityId, string entityType, List<PerformanceDataPoint> data, CancellationToken cancellationToken)
    {
        var bottlenecks = new List<PerformanceBottleneck>();

        // This would typically analyze memory usage patterns
        // For now, return empty list as memory metrics aren't available in the current data structure

        return bottlenecks;
    }

    private static double EstimateImprovement(PerformanceBottleneck bottleneck)
    {
        return bottleneck.Type switch
        {
            "SlowOperation" => 0.3,
            "HighErrorRate" => 0.4,
            "MemoryLeak" => 0.5,
            _ => 0.2
        };
    }

    private static string GenerateImplementationSteps(PerformanceBottleneck bottleneck)
    {
        return bottleneck.Type switch
        {
            "SlowOperation" => "1. Profile the operation, 2. Identify bottlenecks, 3. Optimize code/queries, 4. Test performance",
            "HighErrorRate" => "1. Analyze error logs, 2. Fix root causes, 3. Improve error handling, 4. Monitor error rates",
            "MemoryLeak" => "1. Profile memory usage, 2. Identify leak sources, 3. Fix memory management, 4. Monitor memory usage",
            _ => "1. Analyze the issue, 2. Develop solution, 3. Implement changes, 4. Test and monitor"
        };
    }

    private static List<string> GenerateRequirements(PerformanceBottleneck bottleneck)
    {
        return bottleneck.Type switch
        {
            "SlowOperation" => new List<string> { "Performance profiling tools", "Database access", "Code review" },
            "HighErrorRate" => new List<string> { "Error logging access", "System monitoring", "Code changes" },
            "MemoryLeak" => new List<string> { "Memory profiling tools", "System access", "Code analysis" },
            _ => new List<string> { "System access", "Monitoring tools", "Development resources" }
        };
    }

    private async Task<bool> ApplyAutomaticOptimizationAsync(PerformanceOptimizationSuggestion suggestion, CancellationToken cancellationToken)
    {
        try
        {
            // This would implement specific automatic optimizations
            // For now, only simulate applying safe optimizations

            if (suggestion.Type == "CacheOptimization" || suggestion.Type == "ConfigurationTuning")
            {
                _logger.LogInformation("Applied automatic optimization: {Title}", suggestion.Title);
                return true;
            }

            return false; // Don't apply potentially risky optimizations automatically
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error applying automatic optimization");
            return false;
        }
    }

    private static PerformanceMetricsEntry MapToPerformanceMetricsEntry(PerformanceMetricsEntity entity)
    {
        return new PerformanceMetricsEntry
        {
            Id = entity.Id.ToString(),
            MetricName = entity.MetricName,
            Category = entity.Category,
            Value = entity.Value,
            Unit = entity.Unit,
            Timestamp = entity.Timestamp,
            EntityId = entity.EntityId,
            EntityType = entity.EntityType,
            Tags = string.IsNullOrEmpty(entity.Tags) ?
                new Dictionary<string, object>() :
                JsonSerializer.Deserialize<Dictionary<string, object>>(entity.Tags) ?? new Dictionary<string, object>()
        };
    }

    #endregion

    private class PerformanceDataPoint
    {
        public DateTime Timestamp { get; set; }
        public double ResponseTimeMs { get; set; }
        public string OperationType { get; set; } = string.Empty;
        public bool IsSuccess { get; set; }
    }
}
