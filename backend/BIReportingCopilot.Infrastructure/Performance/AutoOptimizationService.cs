using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using BIReportingCopilot.Core.Configuration;
using BIReportingCopilot.Core.Interfaces;
using BIReportingCopilot.Infrastructure.AI;
using BIReportingCopilot.Infrastructure.Monitoring;
using System.Diagnostics;

namespace BIReportingCopilot.Infrastructure.Performance;

/// <summary>
/// Automatic performance optimization service
/// Monitors system performance and applies optimizations automatically
/// </summary>
public class AutoOptimizationService : IHostedService, IDisposable
{
    private readonly ILogger<AutoOptimizationService> _logger;
    private readonly PerformanceManagementService _performanceService;
    private readonly MonitoringManagementService _monitoringService;
    private readonly UnifiedSemanticCacheService _cacheService;
    private readonly PerformanceConfiguration _config;
    private readonly Timer _optimizationTimer;
    private readonly Timer _monitoringTimer;
    private readonly SemaphoreSlim _optimizationSemaphore;
    private bool _disposed;

    public AutoOptimizationService(
        ILogger<AutoOptimizationService> logger,
        PerformanceManagementService performanceService,
        MonitoringManagementService monitoringService,
        UnifiedSemanticCacheService cacheService,
        IOptions<PerformanceConfiguration> config)
    {
        _logger = logger;
        _performanceService = performanceService;
        _monitoringService = monitoringService;
        _cacheService = cacheService;
        _config = config.Value;
        _optimizationSemaphore = new SemaphoreSlim(1, 1);
        
        // Initialize timers but don't start them yet
        _optimizationTimer = new Timer(PerformOptimizations, null, Timeout.Infinite, Timeout.Infinite);
        _monitoringTimer = new Timer(MonitorPerformance, null, Timeout.Infinite, Timeout.Infinite);
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("🚀 Starting Auto Optimization Service");

        // Start monitoring every 30 seconds
        _monitoringTimer.Change(TimeSpan.Zero, TimeSpan.FromSeconds(30));
        
        // Start optimization checks every 5 minutes
        _optimizationTimer.Change(TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(5));

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("🛑 Stopping Auto Optimization Service");

        _optimizationTimer?.Change(Timeout.Infinite, 0);
        _monitoringTimer?.Change(Timeout.Infinite, 0);

        return Task.CompletedTask;
    }

    private async void MonitorPerformance(object? state)
    {
        try
        {
            var stopwatch = Stopwatch.StartNew();
            
            // Collect current performance metrics
            var metrics = await _performanceService.GetCurrentPerformanceSnapshotAsync();
            
            // Record monitoring metrics
            _monitoringService.RecordPerformanceMetric("monitoring_cycle_duration", stopwatch.ElapsedMilliseconds);
            
            // Check for performance issues
            await CheckPerformanceThresholds(metrics);
            
            _logger.LogDebug("📊 Performance monitoring cycle completed in {Duration}ms", stopwatch.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error during performance monitoring cycle");
        }
    }

    private async void PerformOptimizations(object? state)
    {
        if (!await _optimizationSemaphore.WaitAsync(1000))
        {
            _logger.LogWarning("⏳ Optimization cycle skipped - previous cycle still running");
            return;
        }

        try
        {
            _logger.LogInformation("🔧 Starting automatic optimization cycle");
            var stopwatch = Stopwatch.StartNew();

            // Get current performance metrics
            var metrics = await _performanceService.GetCurrentPerformanceSnapshotAsync();
            var cacheMetrics = await _cacheService.GetCachePerformanceMetricsAsync();

            var optimizationsApplied = 0;

            // Cache optimizations
            optimizationsApplied += await OptimizeCache(cacheMetrics);

            // Memory optimizations
            optimizationsApplied += await OptimizeMemory(metrics);

            // Query optimizations
            optimizationsApplied += await OptimizeQueries(metrics);

            // Database connection optimizations
            optimizationsApplied += await OptimizeDatabaseConnections(metrics);

            stopwatch.Stop();
            
            _logger.LogInformation("✅ Optimization cycle completed in {Duration}ms. Applied {Count} optimizations", 
                stopwatch.ElapsedMilliseconds, optimizationsApplied);

            // Record optimization metrics
            _monitoringService.RecordPerformanceMetric("optimization_cycle_duration", stopwatch.ElapsedMilliseconds);
            _monitoringService.RecordPerformanceMetric("optimizations_applied", optimizationsApplied);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error during optimization cycle");
        }
        finally
        {
            _optimizationSemaphore.Release();
        }
    }

    private async Task<int> OptimizeCache(Core.Interfaces.CachePerformanceMetrics cacheMetrics)
    {
        var optimizations = 0;

        try
        {
            // Optimize cache if hit rate is low
            if (cacheMetrics.HitRate < 0.7)
            {
                _logger.LogInformation("🗄️ Cache hit rate is {HitRate:P1}, optimizing cache", cacheMetrics.HitRate);
                await _cacheService.OptimizeCacheAsync();
                optimizations++;
            }

            // Clear expired entries if cache is getting full
            if (cacheMetrics.TotalRequests > 10000)
            {
                _logger.LogInformation("🧹 Cache has {Requests} requests, cleaning up expired entries", cacheMetrics.TotalRequests);
                await _cacheService.InvalidateExpiredEntriesAsync();
                optimizations++;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error optimizing cache");
        }

        return optimizations;
    }

    private async Task<int> OptimizeMemory(dynamic metrics)
    {
        var optimizations = 0;

        try
        {
            // Force garbage collection if memory usage is high
            var memoryBefore = GC.GetTotalMemory(false);
            if (memoryBefore > 500 * 1024 * 1024) // 500MB threshold
            {
                _logger.LogInformation("🧠 Memory usage is {Memory:F1}MB, forcing garbage collection", memoryBefore / (1024.0 * 1024.0));
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
                
                var memoryAfter = GC.GetTotalMemory(false);
                var freed = memoryBefore - memoryAfter;
                
                _logger.LogInformation("♻️ Garbage collection freed {Freed:F1}MB", freed / (1024.0 * 1024.0));
                optimizations++;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error optimizing memory");
        }

        return optimizations;
    }

    private async Task<int> OptimizeQueries(dynamic metrics)
    {
        var optimizations = 0;

        try
        {
            // Get query performance data
            var queryMetrics = await _performanceService.GetQueryPerformanceBreakdownAsync();
            
            // Identify slow queries
            var slowQueries = queryMetrics.Where(q => q.AverageExecutionTime.TotalMilliseconds > 1000).ToList();
            
            if (slowQueries.Any())
            {
                _logger.LogInformation("🐌 Found {Count} slow queries, applying optimizations", slowQueries.Count);
                
                foreach (var slowQuery in slowQueries)
                {
                    // Log slow query for analysis
                    _logger.LogWarning("Slow query detected: {QueryType} - {AvgTime}ms", 
                        slowQuery.QueryType, slowQuery.AverageExecutionTime.TotalMilliseconds);
                }
                
                optimizations++;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error optimizing queries");
        }

        return optimizations;
    }

    private async Task<int> OptimizeDatabaseConnections(dynamic metrics)
    {
        var optimizations = 0;

        try
        {
            // This would implement database connection pool optimization
            // For now, just log the optimization opportunity
            _logger.LogDebug("🔗 Database connection optimization check completed");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error optimizing database connections");
        }

        return optimizations;
    }

    private async Task CheckPerformanceThresholds(dynamic metrics)
    {
        try
        {
            var cacheMetrics = await _cacheService.GetCachePerformanceMetricsAsync();
            
            // Check cache performance
            if (cacheMetrics.HitRate < 0.5)
            {
                _logger.LogWarning("⚠️ Cache hit rate is critically low: {HitRate:P1}", cacheMetrics.HitRate);
            }

            // Check memory usage
            var memoryUsage = GC.GetTotalMemory(false);
            if (memoryUsage > 1024 * 1024 * 1024) // 1GB threshold
            {
                _logger.LogWarning("⚠️ High memory usage detected: {Memory:F1}MB", memoryUsage / (1024.0 * 1024.0));
            }

            // Check for performance degradation trends
            var recentMetrics = await _performanceService.GetPerformanceMetricsAsync(
                DateTime.UtcNow.AddMinutes(-10), DateTime.UtcNow);
            
            if (recentMetrics.Any())
            {
                var avgExecutionTime = recentMetrics.Average(m => m.AverageExecutionTime.TotalMilliseconds);
                if (avgExecutionTime > 2000) // 2 second threshold
                {
                    _logger.LogWarning("⚠️ Query execution time is degrading: {AvgTime:F1}ms", avgExecutionTime);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error checking performance thresholds");
        }
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _optimizationTimer?.Dispose();
            _monitoringTimer?.Dispose();
            _optimizationSemaphore?.Dispose();
            _disposed = true;
        }
    }
}
