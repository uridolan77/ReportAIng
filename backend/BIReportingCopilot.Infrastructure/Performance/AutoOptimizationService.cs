using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using BIReportingCopilot.Core.Configuration;
using BIReportingCopilot.Core.Interfaces;
using BIReportingCopilot.Core.Interfaces.AI;
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
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly PerformanceConfiguration _config;
    private readonly Timer _optimizationTimer;
    private readonly Timer _monitoringTimer;
    private readonly SemaphoreSlim _optimizationSemaphore;
    private bool _disposed;

    public AutoOptimizationService(
        ILogger<AutoOptimizationService> logger,
        IServiceScopeFactory serviceScopeFactory,
        IOptions<PerformanceConfiguration> config)
    {
        _logger = logger;
        _serviceScopeFactory = serviceScopeFactory;
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
            using var scope = _serviceScopeFactory.CreateScope();
            var performanceService = scope.ServiceProvider.GetRequiredService<PerformanceManagementService>();
            var monitoringService = scope.ServiceProvider.GetRequiredService<MonitoringManagementService>();

            var stopwatch = Stopwatch.StartNew();

            // Collect current performance metrics
            var metrics = await performanceService.GetCurrentPerformanceSnapshotAsync();

            // Record monitoring metrics
            monitoringService.RecordHistogram("monitoring_cycle_duration", stopwatch.ElapsedMilliseconds);

            // Check for performance issues
            await CheckPerformanceThresholds(metrics, scope.ServiceProvider);

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
            using var scope = _serviceScopeFactory.CreateScope();
            var performanceService = scope.ServiceProvider.GetRequiredService<PerformanceManagementService>();
            var cacheService = scope.ServiceProvider.GetRequiredService<ISemanticCacheService>();
            var monitoringService = scope.ServiceProvider.GetRequiredService<MonitoringManagementService>();

            _logger.LogInformation("🔧 Starting automatic optimization cycle");
            var stopwatch = Stopwatch.StartNew();

            // Get current performance metrics
            var metrics = await performanceService.GetCurrentPerformanceSnapshotAsync();
            var cacheMetrics = await cacheService.GetCachePerformanceMetricsAsync();

            var optimizationsApplied = 0;

            // Cache optimizations
            optimizationsApplied += await OptimizeCache(cacheMetrics, cacheService);

            // Memory optimizations
            optimizationsApplied += await OptimizeMemory(metrics);

            // Query optimizations
            optimizationsApplied += await OptimizeQueries(metrics, performanceService);

            // Database connection optimizations
            optimizationsApplied += await OptimizeDatabaseConnections(metrics);

            stopwatch.Stop();

            _logger.LogInformation("✅ Optimization cycle completed in {Duration}ms. Applied {Count} optimizations",
                stopwatch.ElapsedMilliseconds, optimizationsApplied);

            // Record optimization metrics
            monitoringService.RecordHistogram("optimization_cycle_duration", stopwatch.ElapsedMilliseconds);
            monitoringService.RecordHistogram("optimizations_applied", optimizationsApplied);
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

    private async Task<int> OptimizeCache(Core.Interfaces.CachePerformanceMetrics cacheMetrics, ISemanticCacheService cacheService)
    {
        var optimizations = 0;

        try
        {
            // Optimize cache if hit rate is low
            if (cacheMetrics.HitRate < 0.7)
            {
                _logger.LogInformation("🗄️ Cache hit rate is {HitRate:P1}, optimizing cache", cacheMetrics.HitRate);
                await cacheService.OptimizeCacheAsync();
                optimizations++;
            }

            // Clear expired entries if cache is getting full
            if (cacheMetrics.TotalRequests > 10000)
            {
                _logger.LogInformation("🧹 Cache has {Requests} requests, cleaning up expired entries", cacheMetrics.TotalRequests);
                await cacheService.CleanupExpiredEntriesAsync();
                optimizations++;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error optimizing cache");
        }

        return optimizations;
    }

    private Task<int> OptimizeMemory(dynamic metrics)
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

        return Task.FromResult(optimizations);
    }

    private async Task<int> OptimizeQueries(dynamic metrics, PerformanceManagementService performanceService)
    {
        var optimizations = 0;

        try
        {
            // Get query performance data
            var queryMetrics = await performanceService.GetQueryPerformanceBreakdownAsync();

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

    private Task<int> OptimizeDatabaseConnections(dynamic metrics)
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

        return Task.FromResult(optimizations);
    }

    private async Task CheckPerformanceThresholds(dynamic metrics, IServiceProvider serviceProvider)
    {
        try
        {
            var cacheService = serviceProvider.GetRequiredService<ISemanticCacheService>();
            var performanceService = serviceProvider.GetRequiredService<PerformanceManagementService>();

            var cacheMetrics = await cacheService.GetCachePerformanceMetricsAsync();

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
            var recentMetrics = await performanceService.GetPerformanceMetricsAsync(
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
