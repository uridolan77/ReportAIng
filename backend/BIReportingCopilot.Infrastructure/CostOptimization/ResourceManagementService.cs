using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;
using System.Collections.Concurrent;
using BIReportingCopilot.Core.Interfaces.CostOptimization;
using BIReportingCopilot.Core.Models;
using BIReportingCopilot.Infrastructure.Data;
using BIReportingCopilot.Infrastructure.Data.Entities;
using BIReportingCopilot.Infrastructure.Security;

namespace BIReportingCopilot.Infrastructure.CostOptimization;

/// <summary>
/// Advanced resource management service with sophisticated quotas, priority-based processing, and enhanced throttling
/// </summary>
public class ResourceManagementService : IResourceManagementService
{
    private readonly BICopilotContext _context;
    private readonly ILogger<ResourceManagementService> _logger;
    private readonly IDistributedCache _cache;
    private readonly IRateLimitingService _rateLimitingService;
    
    private readonly ConcurrentDictionary<string, ResourceQuota> _quotaCache = new();
    private readonly ConcurrentDictionary<string, int> _userPriorities = new();
    private readonly ConcurrentDictionary<string, CircuitBreakerState> _circuitBreakers = new();
    private readonly ConcurrentQueue<QueuedRequest> _requestQueue = new();
    private readonly ConcurrentDictionary<string, double> _resourceLoads = new();
    private readonly SemaphoreSlim _quotaSemaphore = new(1, 1);
    private readonly Timer _quotaResetTimer;
    private readonly Timer _circuitBreakerTimer;

    public ResourceManagementService(
        BICopilotContext context,
        ILogger<ResourceManagementService> logger,
        IDistributedCache cache,
        IRateLimitingService rateLimitingService)
    {
        _context = context;
        _logger = logger;
        _cache = cache;
        _rateLimitingService = rateLimitingService;
        
        InitializeDefaultPriorities();
        InitializeDefaultCircuitBreakers();
        
        // Set up timers for periodic tasks
        _quotaResetTimer = new Timer(ResetExpiredQuotas, null, TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1));
        _circuitBreakerTimer = new Timer(UpdateCircuitBreakers, null, TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(30));
    }

    #region Resource Quotas

    public async Task<ResourceQuota> CreateResourceQuotaAsync(ResourceQuota quota, CancellationToken cancellationToken = default)
    {
        try
        {
            var entity = new ResourceQuotaEntity
            {
                UserId = quota.UserId,
                ResourceType = quota.ResourceType,
                MaxQuantity = quota.MaxQuantity,
                PeriodSeconds = (long)quota.Period.TotalSeconds,
                CurrentUsage = quota.CurrentUsage,
                ResetDate = quota.ResetDate,
                IsActive = quota.IsActive,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = "system"
            };

            _context.ResourceQuotas.Add(entity);
            await _context.SaveChangesAsync(cancellationToken);

            quota.Id = entity.Id.ToString();
            
            // Cache the quota for quick access
            var cacheKey = $"{quota.UserId}:{quota.ResourceType}";
            _quotaCache[cacheKey] = quota;

            _logger.LogInformation("Created resource quota for user {UserId}, resource {ResourceType}: {MaxQuantity} per {Period}", 
                quota.UserId, quota.ResourceType, quota.MaxQuantity, quota.Period);

            return quota;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating resource quota");
            throw;
        }
    }

    public async Task<ResourceQuota> UpdateResourceQuotaAsync(ResourceQuota quota, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!long.TryParse(quota.Id, out var id))
                throw new ArgumentException("Invalid quota ID");

            var entity = await _context.ResourceQuotas
                .FirstOrDefaultAsync(q => q.Id == id, cancellationToken);

            if (entity == null)
                throw new ArgumentException($"Resource quota not found: {quota.Id}");

            entity.MaxQuantity = quota.MaxQuantity;
            entity.PeriodSeconds = (long)quota.Period.TotalSeconds;
            entity.ResetDate = quota.ResetDate;
            entity.IsActive = quota.IsActive;
            entity.UpdatedDate = DateTime.UtcNow;
            entity.UpdatedBy = "system";

            await _context.SaveChangesAsync(cancellationToken);

            // Update cache
            var cacheKey = $"{quota.UserId}:{quota.ResourceType}";
            _quotaCache[cacheKey] = quota;

            _logger.LogInformation("Updated resource quota: {QuotaId}", quota.Id);

            return quota;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating resource quota");
            throw;
        }
    }

    public async Task<ResourceQuota?> GetResourceQuotaAsync(string userId, string resourceType, CancellationToken cancellationToken = default)
    {
        try
        {
            var cacheKey = $"{userId}:{resourceType}";
            
            // Check cache first
            if (_quotaCache.TryGetValue(cacheKey, out var cachedQuota))
            {
                return cachedQuota;
            }

            // Query database
            var entity = await _context.ResourceQuotas
                .FirstOrDefaultAsync(q => q.UserId == userId && q.ResourceType == resourceType && q.IsActive, cancellationToken);

            if (entity == null)
                return null;

            var quota = MapToResourceQuota(entity);
            _quotaCache[cacheKey] = quota;

            return quota;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting resource quota for user {UserId}, resource {ResourceType}", userId, resourceType);
            return null;
        }
    }

    public async Task<List<ResourceQuota>> GetUserResourceQuotasAsync(string userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var entities = await _context.ResourceQuotas
                .Where(q => q.UserId == userId && q.IsActive)
                .ToListAsync(cancellationToken);

            return entities.Select(MapToResourceQuota).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting resource quotas for user {UserId}", userId);
            return new List<ResourceQuota>();
        }
    }

    public async Task<bool> DeleteResourceQuotaAsync(string quotaId, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!long.TryParse(quotaId, out var id))
                return false;

            var entity = await _context.ResourceQuotas
                .FirstOrDefaultAsync(q => q.Id == id, cancellationToken);

            if (entity == null)
                return false;

            entity.IsActive = false;
            entity.UpdatedDate = DateTime.UtcNow;
            entity.UpdatedBy = "system";

            await _context.SaveChangesAsync(cancellationToken);

            // Remove from cache
            var cacheKey = $"{entity.UserId}:{entity.ResourceType}";
            _quotaCache.TryRemove(cacheKey, out _);

            _logger.LogInformation("Deleted resource quota: {QuotaId}", quotaId);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting resource quota: {QuotaId}", quotaId);
            return false;
        }
    }

    #endregion

    #region Resource Usage Tracking

    public async Task TrackResourceUsageAsync(ResourceUsageEntry usage, CancellationToken cancellationToken = default)
    {
        try
        {
            var entity = new ResourceUsageEntity
            {
                UserId = usage.UserId,
                ResourceType = usage.ResourceType,
                ResourceId = usage.ResourceId,
                Quantity = usage.Quantity,
                DurationMs = usage.DurationMs,
                Cost = usage.Cost,
                Timestamp = usage.Timestamp,
                RequestId = usage.RequestId,
                Metadata = JsonSerializer.Serialize(usage.Metadata),
                CreatedDate = DateTime.UtcNow,
                CreatedBy = usage.UserId
            };

            _context.ResourceUsage.Add(entity);
            await _context.SaveChangesAsync(cancellationToken);

            // Update quota usage
            await UpdateQuotaUsageAsync(usage.UserId, usage.ResourceType, usage.Quantity, cancellationToken);

            _logger.LogDebug("Tracked resource usage: {UserId} used {Quantity} of {ResourceType}", 
                usage.UserId, usage.Quantity, usage.ResourceType);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error tracking resource usage");
        }
    }

    public async Task<bool> CheckResourceQuotaAsync(string userId, string resourceType, int requestedQuantity, CancellationToken cancellationToken = default)
    {
        try
        {
            var quota = await GetResourceQuotaAsync(userId, resourceType, cancellationToken);
            if (quota == null)
            {
                // No quota defined, allow the request
                return true;
            }

            // Check if quota needs reset
            if (DateTime.UtcNow >= quota.ResetDate)
            {
                await ResetQuotaUsageAsync(userId, resourceType, cancellationToken);
                quota.CurrentUsage = 0;
            }

            var projectedUsage = quota.CurrentUsage + requestedQuantity;
            var isAllowed = projectedUsage <= quota.MaxQuantity;

            if (!isAllowed)
            {
                _logger.LogWarning("Resource quota exceeded for user {UserId}, resource {ResourceType}: {ProjectedUsage}/{MaxQuantity}", 
                    userId, resourceType, projectedUsage, quota.MaxQuantity);
            }

            return isAllowed;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking resource quota");
            return true; // Fail open
        }
    }

    public async Task<Dictionary<string, int>> GetCurrentResourceUsageAsync(string userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var quotas = await GetUserResourceQuotasAsync(userId, cancellationToken);
            return quotas.ToDictionary(q => q.ResourceType, q => q.CurrentUsage);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting current resource usage for user {UserId}", userId);
            return new Dictionary<string, int>();
        }
    }

    public async Task ResetResourceUsageAsync(string userId, string resourceType, CancellationToken cancellationToken = default)
    {
        try
        {
            await ResetQuotaUsageAsync(userId, resourceType, cancellationToken);
            _logger.LogInformation("Reset resource usage for user {UserId}, resource {ResourceType}", userId, resourceType);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resetting resource usage");
        }
    }

    #endregion

    #region Priority-Based Processing

    public async Task<int> GetUserPriorityAsync(string userId, CancellationToken cancellationToken = default)
    {
        try
        {
            if (_userPriorities.TryGetValue(userId, out var priority))
            {
                return priority;
            }

            // Try to get from cache
            var cacheKey = $"user_priority:{userId}";
            var cachedPriority = await _cache.GetStringAsync(cacheKey, cancellationToken);
            if (!string.IsNullOrEmpty(cachedPriority) && int.TryParse(cachedPriority, out var parsed))
            {
                _userPriorities[userId] = parsed;
                return parsed;
            }

            // Default priority for new users
            var defaultPriority = 5; // Medium priority
            await SetUserPriorityAsync(userId, defaultPriority, cancellationToken);
            return defaultPriority;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user priority for {UserId}", userId);
            return 5; // Default medium priority
        }
    }

    public async Task SetUserPriorityAsync(string userId, int priority, CancellationToken cancellationToken = default)
    {
        try
        {
            // Validate priority range (1-10, where 1 is highest priority)
            priority = Math.Max(1, Math.Min(10, priority));

            _userPriorities[userId] = priority;

            // Cache the priority
            var cacheKey = $"user_priority:{userId}";
            await _cache.SetStringAsync(cacheKey, priority.ToString(),
                new DistributedCacheEntryOptions { SlidingExpiration = TimeSpan.FromHours(24) },
                cancellationToken);

            _logger.LogInformation("Set user priority for {UserId}: {Priority}", userId, priority);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting user priority");
        }
    }

    public async Task<List<string>> GetQueuedRequestsAsync(string resourceType, CancellationToken cancellationToken = default)
    {
        try
        {
            var queuedRequests = _requestQueue
                .Where(r => r.ResourceType == resourceType)
                .OrderBy(r => r.Priority)
                .ThenBy(r => r.QueuedAt)
                .Select(r => r.RequestId)
                .ToList();

            return queuedRequests;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting queued requests");
            return new List<string>();
        }
    }

    public async Task<bool> QueueRequestAsync(string requestId, string userId, string resourceType, int priority, CancellationToken cancellationToken = default)
    {
        try
        {
            var userPriority = await GetUserPriorityAsync(userId, cancellationToken);
            var effectivePriority = Math.Min(priority, userPriority); // Use the higher priority (lower number)

            var queuedRequest = new QueuedRequest
            {
                RequestId = requestId,
                UserId = userId,
                ResourceType = resourceType,
                Priority = effectivePriority,
                QueuedAt = DateTime.UtcNow
            };

            _requestQueue.Enqueue(queuedRequest);

            _logger.LogDebug("Queued request {RequestId} for user {UserId} with priority {Priority}",
                requestId, userId, effectivePriority);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error queueing request");
            return false;
        }
    }

    #endregion

    #region Circuit Breakers

    public async Task<CircuitBreakerState> GetCircuitBreakerStateAsync(string serviceName, CancellationToken cancellationToken = default)
    {
        try
        {
            if (_circuitBreakers.TryGetValue(serviceName, out var state))
            {
                return state;
            }

            // Create default circuit breaker state
            var defaultState = new CircuitBreakerState
            {
                Id = Guid.NewGuid().ToString(),
                ServiceName = serviceName,
                State = "Closed",
                FailureCount = 0,
                FailureThreshold = 5,
                Timeout = TimeSpan.FromMinutes(1),
                LastFailure = DateTime.MinValue,
                IsEnabled = true,
                Metadata = new Dictionary<string, object>()
            };

            _circuitBreakers[serviceName] = defaultState;
            return defaultState;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting circuit breaker state for {ServiceName}", serviceName);
            return new CircuitBreakerState { ServiceName = serviceName, State = "Closed" };
        }
    }

    public async Task UpdateCircuitBreakerStateAsync(string serviceName, string state, CancellationToken cancellationToken = default)
    {
        try
        {
            if (_circuitBreakers.TryGetValue(serviceName, out var circuitBreaker))
            {
                circuitBreaker.State = state;

                if (state == "Open")
                {
                    circuitBreaker.NextRetry = DateTime.UtcNow.Add(circuitBreaker.Timeout);
                }
                else if (state == "Closed")
                {
                    circuitBreaker.FailureCount = 0;
                    circuitBreaker.NextRetry = null;
                }

                _logger.LogInformation("Updated circuit breaker state for {ServiceName}: {State}", serviceName, state);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating circuit breaker state");
        }
    }

    public async Task<bool> IsServiceAvailableAsync(string serviceName, CancellationToken cancellationToken = default)
    {
        try
        {
            var circuitBreaker = await GetCircuitBreakerStateAsync(serviceName, cancellationToken);

            if (!circuitBreaker.IsEnabled)
                return true;

            switch (circuitBreaker.State)
            {
                case "Closed":
                    return true;
                case "Open":
                    // Check if timeout has passed
                    if (circuitBreaker.NextRetry.HasValue && DateTime.UtcNow >= circuitBreaker.NextRetry.Value)
                    {
                        // Move to half-open state
                        await UpdateCircuitBreakerStateAsync(serviceName, "HalfOpen", cancellationToken);
                        return true;
                    }
                    return false;
                case "HalfOpen":
                    return true;
                default:
                    return true;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking service availability");
            return true; // Fail open
        }
    }

    public async Task RecordServiceFailureAsync(string serviceName, CancellationToken cancellationToken = default)
    {
        try
        {
            var circuitBreaker = await GetCircuitBreakerStateAsync(serviceName, cancellationToken);
            circuitBreaker.FailureCount++;
            circuitBreaker.LastFailure = DateTime.UtcNow;

            if (circuitBreaker.FailureCount >= circuitBreaker.FailureThreshold)
            {
                await UpdateCircuitBreakerStateAsync(serviceName, "Open", cancellationToken);
                _logger.LogWarning("Circuit breaker opened for service {ServiceName} after {FailureCount} failures",
                    serviceName, circuitBreaker.FailureCount);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recording service failure");
        }
    }

    public async Task RecordServiceSuccessAsync(string serviceName, CancellationToken cancellationToken = default)
    {
        try
        {
            var circuitBreaker = await GetCircuitBreakerStateAsync(serviceName, cancellationToken);

            if (circuitBreaker.State == "HalfOpen")
            {
                // Success in half-open state, close the circuit
                await UpdateCircuitBreakerStateAsync(serviceName, "Closed", cancellationToken);
                _logger.LogInformation("Circuit breaker closed for service {ServiceName} after successful request", serviceName);
            }
            else if (circuitBreaker.State == "Closed" && circuitBreaker.FailureCount > 0)
            {
                // Reset failure count on success
                circuitBreaker.FailureCount = Math.Max(0, circuitBreaker.FailureCount - 1);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recording service success");
        }
    }

    #endregion

    #region Load Balancing

    public async Task<string> SelectOptimalResourceAsync(string resourceType, CancellationToken cancellationToken = default)
    {
        try
        {
            var availableResources = GetAvailableResources(resourceType);
            if (!availableResources.Any())
            {
                throw new InvalidOperationException($"No available resources of type {resourceType}");
            }

            // Select resource with lowest load
            var optimalResource = availableResources
                .OrderBy(r => _resourceLoads.GetValueOrDefault(r, 0.0))
                .First();

            _logger.LogDebug("Selected optimal resource {ResourceId} for type {ResourceType}",
                optimalResource, resourceType);

            return optimalResource;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error selecting optimal resource");
            throw;
        }
    }

    public async Task UpdateResourceLoadAsync(string resourceId, double loadPercentage, CancellationToken cancellationToken = default)
    {
        try
        {
            _resourceLoads[resourceId] = Math.Max(0.0, Math.Min(1.0, loadPercentage));

            _logger.LogDebug("Updated resource load for {ResourceId}: {LoadPercentage:P}",
                resourceId, loadPercentage);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating resource load");
        }
    }

    public async Task<Dictionary<string, double>> GetResourceLoadStatsAsync(string resourceType, CancellationToken cancellationToken = default)
    {
        try
        {
            var resources = GetAvailableResources(resourceType);
            var loadStats = new Dictionary<string, double>();

            foreach (var resource in resources)
            {
                loadStats[resource] = _resourceLoads.GetValueOrDefault(resource, 0.0);
            }

            return loadStats;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting resource load stats");
            return new Dictionary<string, double>();
        }
    }

    #endregion

    #region Helper Methods

    private void InitializeDefaultPriorities()
    {
        // Initialize with some default user priorities
        _userPriorities["admin"] = 1; // Highest priority
        _userPriorities["premium"] = 3; // High priority
        _userPriorities["standard"] = 5; // Medium priority
        _userPriorities["basic"] = 7; // Low priority
    }

    private void InitializeDefaultCircuitBreakers()
    {
        var defaultServices = new[] { "ai-service", "database", "cache", "external-api" };

        foreach (var service in defaultServices)
        {
            _circuitBreakers[service] = new CircuitBreakerState
            {
                Id = Guid.NewGuid().ToString(),
                ServiceName = service,
                State = "Closed",
                FailureCount = 0,
                FailureThreshold = 5,
                Timeout = TimeSpan.FromMinutes(1),
                LastFailure = DateTime.MinValue,
                IsEnabled = true,
                Metadata = new Dictionary<string, object>()
            };
        }
    }

    private async Task UpdateQuotaUsageAsync(string userId, string resourceType, int quantity, CancellationToken cancellationToken)
    {
        try
        {
            await _quotaSemaphore.WaitAsync(cancellationToken);

            var quota = await GetResourceQuotaAsync(userId, resourceType, cancellationToken);
            if (quota != null)
            {
                quota.CurrentUsage += quantity;

                // Update in database
                if (long.TryParse(quota.Id, out var id))
                {
                    var entity = await _context.ResourceQuotas
                        .FirstOrDefaultAsync(q => q.Id == id, cancellationToken);

                    if (entity != null)
                    {
                        entity.CurrentUsage = quota.CurrentUsage;
                        entity.UpdatedDate = DateTime.UtcNow;
                        await _context.SaveChangesAsync(cancellationToken);
                    }
                }

                // Update cache
                var cacheKey = $"{userId}:{resourceType}";
                _quotaCache[cacheKey] = quota;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating quota usage");
        }
        finally
        {
            _quotaSemaphore.Release();
        }
    }

    private async Task ResetQuotaUsageAsync(string userId, string resourceType, CancellationToken cancellationToken)
    {
        try
        {
            await _quotaSemaphore.WaitAsync(cancellationToken);

            var quota = await GetResourceQuotaAsync(userId, resourceType, cancellationToken);
            if (quota != null)
            {
                quota.CurrentUsage = 0;
                quota.ResetDate = DateTime.UtcNow.Add(quota.Period);

                // Update in database
                if (long.TryParse(quota.Id, out var id))
                {
                    var entity = await _context.ResourceQuotas
                        .FirstOrDefaultAsync(q => q.Id == id, cancellationToken);

                    if (entity != null)
                    {
                        entity.CurrentUsage = 0;
                        entity.ResetDate = quota.ResetDate;
                        entity.UpdatedDate = DateTime.UtcNow;
                        await _context.SaveChangesAsync(cancellationToken);
                    }
                }

                // Update cache
                var cacheKey = $"{userId}:{resourceType}";
                _quotaCache[cacheKey] = quota;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resetting quota usage");
        }
        finally
        {
            _quotaSemaphore.Release();
        }
    }

    private List<string> GetAvailableResources(string resourceType)
    {
        // This would typically come from a service registry or configuration
        // For now, return mock resources based on type
        return resourceType.ToLower() switch
        {
            "ai-service" => new List<string> { "ai-service-1", "ai-service-2", "ai-service-3" },
            "database" => new List<string> { "db-primary", "db-replica-1", "db-replica-2" },
            "cache" => new List<string> { "cache-node-1", "cache-node-2" },
            _ => new List<string> { $"{resourceType}-default" }
        };
    }

    private void ResetExpiredQuotas(object? state)
    {
        try
        {
            var now = DateTime.UtcNow;
            var expiredQuotas = _quotaCache.Values
                .Where(q => q.ResetDate <= now)
                .ToList();

            foreach (var quota in expiredQuotas)
            {
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await ResetQuotaUsageAsync(quota.UserId, quota.ResourceType, CancellationToken.None);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error in background quota reset");
                    }
                });
            }

            if (expiredQuotas.Any())
            {
                _logger.LogInformation("Reset {Count} expired quotas", expiredQuotas.Count);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in quota reset timer");
        }
    }

    private void UpdateCircuitBreakers(object? state)
    {
        try
        {
            var now = DateTime.UtcNow;
            var circuitBreakersToUpdate = _circuitBreakers.Values
                .Where(cb => cb.State == "Open" && cb.NextRetry.HasValue && now >= cb.NextRetry.Value)
                .ToList();

            foreach (var circuitBreaker in circuitBreakersToUpdate)
            {
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await UpdateCircuitBreakerStateAsync(circuitBreaker.ServiceName, "HalfOpen", CancellationToken.None);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error updating circuit breaker state");
                    }
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in circuit breaker timer");
        }
    }

    private static ResourceQuota MapToResourceQuota(ResourceQuotaEntity entity)
    {
        return new ResourceQuota
        {
            Id = entity.Id.ToString(),
            UserId = entity.UserId,
            ResourceType = entity.ResourceType,
            MaxQuantity = entity.MaxQuantity,
            Period = TimeSpan.FromSeconds(entity.PeriodSeconds),
            CurrentUsage = entity.CurrentUsage,
            ResetDate = entity.ResetDate,
            IsActive = entity.IsActive,
            CreatedAt = entity.CreatedDate,
            UpdatedAt = entity.UpdatedDate ?? entity.CreatedDate
        };
    }

    public void Dispose()
    {
        _quotaResetTimer?.Dispose();
        _circuitBreakerTimer?.Dispose();
        _quotaSemaphore?.Dispose();
    }

    #endregion

    private class QueuedRequest
    {
        public string RequestId { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string ResourceType { get; set; } = string.Empty;
        public int Priority { get; set; }
        public DateTime QueuedAt { get; set; }
    }
}
