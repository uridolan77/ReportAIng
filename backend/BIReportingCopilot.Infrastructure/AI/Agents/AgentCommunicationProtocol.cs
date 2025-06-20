using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Caching.Distributed;
using BIReportingCopilot.Core.Interfaces.Agents;
using BIReportingCopilot.Core.Models.Agents;
using BIReportingCopilot.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Collections.Concurrent;
using Polly;
using Polly.Extensions.Http;

namespace BIReportingCopilot.Infrastructure.AI.Agents;

/// <summary>
/// Agent-to-Agent (A2A) Communication Protocol Implementation
/// </summary>
public class AgentCommunicationProtocol : IAgentCommunicationProtocol
{
    private readonly ILogger<AgentCommunicationProtocol> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly IDistributedCache _cache;
    private readonly BICopilotContext _context;
    private readonly ConcurrentDictionary<string, ISpecializedAgent> _registeredAgents;
    private readonly IAsyncPolicy _retryPolicy;

    public AgentCommunicationProtocol(
        ILogger<AgentCommunicationProtocol> logger,
        IServiceProvider serviceProvider,
        IDistributedCache cache,
        BICopilotContext context)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _cache = cache;
        _context = context;
        _registeredAgents = new ConcurrentDictionary<string, ISpecializedAgent>();
        
        // Configure retry policy for agent communication
        _retryPolicy = Policy
            .Handle<Exception>()
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: retryAttempt => TimeSpan.FromMilliseconds(Math.Pow(2, retryAttempt) * 100),
                onRetry: (outcome, timespan, retryCount, context) =>
                {
                    _logger.LogWarning("Retry {RetryCount} for agent communication after {Delay}ms", 
                        retryCount, timespan.TotalMilliseconds);
                });
    }

    #region IAgentCommunicationProtocol Implementation

    public async Task<TResponse> SendMessageAsync<TRequest, TResponse>(
        string targetAgent, 
        TRequest message, 
        AgentContext context,
        TimeSpan? timeout = null)
    {
        var correlationId = context.CorrelationId;
        var effectiveTimeout = timeout ?? TimeSpan.FromSeconds(30);
        
        _logger.LogDebug("📡 Sending A2A message from {SourceAgent} to {TargetAgent} (Correlation: {CorrelationId})", 
            context.CurrentAgent, targetAgent, correlationId);

        try
        {
            // 1. Validate target agent availability
            var targetService = await ResolveAgentServiceAsync(targetAgent);
            if (targetService == null)
            {
                throw new InvalidOperationException($"Target agent '{targetAgent}' is not available");
            }

            // 2. Create communication envelope
            var envelope = new AgentMessage<TRequest>
            {
                CorrelationId = correlationId,
                SourceAgent = context.CurrentAgent,
                TargetAgent = targetAgent,
                Payload = message,
                Timestamp = DateTime.UtcNow,
                Context = context,
                Headers = new Dictionary<string, object>
                {
                    ["Timeout"] = effectiveTimeout.TotalSeconds,
                    ["MessageType"] = typeof(TRequest).Name,
                    ["ExpectedResponseType"] = typeof(TResponse).Name
                }
            };

            // 3. Send message with timeout and retry
            var response = await _retryPolicy.ExecuteAsync(async () =>
            {
                using var cts = new CancellationTokenSource(effectiveTimeout);
                return await SendWithTimeoutAsync<TRequest, TResponse>(targetService, envelope, cts.Token);
            });

            // 4. Log successful communication
            await LogCommunicationAsync<TRequest>(envelope, response, true);

            _logger.LogDebug("✅ A2A message sent successfully to {TargetAgent}", targetAgent);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Failed to send A2A message to {TargetAgent}", targetAgent);
            await LogCommunicationAsync<TRequest>(null, null, false, ex);
            throw;
        }
    }

    public async Task BroadcastEventAsync<TEvent>(TEvent eventData, AgentContext context, List<string>? targetAgents = null)
    {
        var correlationId = context.CorrelationId;
        
        _logger.LogDebug("📢 Broadcasting event {EventType} (Correlation: {CorrelationId})", 
            typeof(TEvent).Name, correlationId);

        try
        {
            var agents = targetAgents ?? _registeredAgents.Keys.ToList();
            var broadcastTasks = new List<Task>();

            foreach (var agentType in agents)
            {
                if (agentType == context.CurrentAgent) continue; // Don't broadcast to self

                var broadcastTask = Task.Run(async () =>
                {
                    try
                    {
                        var agent = await ResolveAgentServiceAsync(agentType);
                        if (agent != null)
                        {
                            var eventRequest = new AgentRequest
                            {
                                RequestType = "BroadcastEvent",
                                Payload = eventData!,
                                Parameters = new Dictionary<string, object>
                                {
                                    ["EventType"] = typeof(TEvent).Name,
                                    ["SourceAgent"] = context.CurrentAgent
                                }
                            };

                            await agent.ProcessAsync(eventRequest, context);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to broadcast event to agent {AgentType}", agentType);
                    }
                });

                broadcastTasks.Add(broadcastTask);
            }

            await Task.WhenAll(broadcastTasks);
            
            _logger.LogDebug("✅ Event broadcast completed to {AgentCount} agents", broadcastTasks.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Failed to broadcast event {EventType}", typeof(TEvent).Name);
            throw;
        }
    }

    public async Task<List<AgentCapabilities>> DiscoverAgentCapabilitiesAsync()
    {
        _logger.LogDebug("🔍 Discovering agent capabilities");

        try
        {
            var capabilities = new List<AgentCapabilities>();

            foreach (var kvp in _registeredAgents)
            {
                try
                {
                    var agent = kvp.Value;
                    var healthStatus = await agent.GetHealthStatusAsync();
                    
                    var agentCapabilities = agent.Capabilities;
                    agentCapabilities.IsAvailable = healthStatus.IsHealthy;
                    agentCapabilities.LastHealthCheck = DateTime.UtcNow;
                    
                    capabilities.Add(agentCapabilities);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to get capabilities for agent {AgentType}", kvp.Key);
                }
            }

            _logger.LogDebug("✅ Discovered capabilities for {AgentCount} agents", capabilities.Count);
            return capabilities;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Failed to discover agent capabilities");
            throw;
        }
    }

    public async Task RegisterAgentAsync(ISpecializedAgent agent)
    {
        _logger.LogInformation("📝 Registering agent {AgentType}", agent.AgentType);

        try
        {
            _registeredAgents.AddOrUpdate(agent.AgentType, agent, (key, existing) => agent);
            
            // Cache agent capabilities
            var cacheKey = $"agent_capabilities_{agent.AgentType}";
            var capabilitiesJson = JsonSerializer.Serialize(agent.Capabilities);
            await _cache.SetStringAsync(cacheKey, capabilitiesJson, new DistributedCacheEntryOptions
            {
                SlidingExpiration = TimeSpan.FromMinutes(30)
            });

            _logger.LogInformation("✅ Agent {AgentType} registered successfully", agent.AgentType);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Failed to register agent {AgentType}", agent.AgentType);
            throw;
        }
    }

    public async Task UnregisterAgentAsync(string agentType)
    {
        _logger.LogInformation("🗑️ Unregistering agent {AgentType}", agentType);

        try
        {
            _registeredAgents.TryRemove(agentType, out _);
            
            // Remove from cache
            var cacheKey = $"agent_capabilities_{agentType}";
            await _cache.RemoveAsync(cacheKey);

            _logger.LogInformation("✅ Agent {AgentType} unregistered successfully", agentType);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Failed to unregister agent {AgentType}", agentType);
            throw;
        }
    }

    public async Task<List<AgentCommunicationLog>> GetCommunicationLogsAsync(string? correlationId = null, DateTime? since = null)
    {
        try
        {
            var query = _context.Set<AgentCommunicationLogEntity>().AsQueryable();

            if (!string.IsNullOrEmpty(correlationId))
            {
                query = query.Where(log => log.CorrelationId == correlationId);
            }

            if (since.HasValue)
            {
                query = query.Where(log => log.CreatedAt >= since.Value);
            }

            var entities = await query
                .OrderByDescending(log => log.CreatedAt)
                .Take(1000) // Limit results
                .ToListAsync();

            return entities.Select(entity => new AgentCommunicationLog
            {
                Id = entity.Id,
                CorrelationId = entity.CorrelationId,
                SourceAgent = entity.SourceAgent,
                TargetAgent = entity.TargetAgent,
                MessageType = entity.MessageType,
                Success = entity.Success,
                ExecutionTimeMs = entity.ExecutionTimeMs,
                ErrorMessage = entity.ErrorMessage,
                CreatedAt = entity.CreatedAt,
                Metadata = string.IsNullOrEmpty(entity.MetadataJson) 
                    ? new Dictionary<string, object>() 
                    : JsonSerializer.Deserialize<Dictionary<string, object>>(entity.MetadataJson) ?? new Dictionary<string, object>()
            }).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Failed to get communication logs");
            throw;
        }
    }

    #endregion

    #region Private Helper Methods

    private async Task<ISpecializedAgent?> ResolveAgentServiceAsync(string agentType)
    {
        // First try registered agents
        if (_registeredAgents.TryGetValue(agentType, out var registeredAgent))
        {
            return registeredAgent;
        }

        // Try to resolve from DI container
        try
        {
            return agentType switch
            {
                "QueryUnderstanding" => _serviceProvider.GetService<IQueryUnderstandingAgent>(),
                "SchemaNavigation" => _serviceProvider.GetService<ISchemaNavigationAgent>(),
                "SqlGeneration" => _serviceProvider.GetService<ISqlGenerationAgent>(),
                "Execution" => _serviceProvider.GetService<IExecutionAgent>(),
                "Visualization" => _serviceProvider.GetService<IVisualizationAgent>(),
                _ => null
            };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to resolve agent service for {AgentType}", agentType);
            return null;
        }
    }

    private async Task<TResponse> SendWithTimeoutAsync<TRequest, TResponse>(
        ISpecializedAgent targetAgent, 
        AgentMessage<TRequest> envelope, 
        CancellationToken cancellationToken)
    {
        var request = new AgentRequest
        {
            RequestId = envelope.CorrelationId,
            RequestType = "ProcessMessage",
            Payload = envelope.Payload!,
            Parameters = envelope.Headers
        };

        var response = await targetAgent.ProcessAsync(request, envelope.Context);

        if (!response.Success)
        {
            throw new InvalidOperationException($"Agent communication failed: {response.ErrorMessage}");
        }

        if (response.Result is TResponse typedResult)
        {
            return typedResult;
        }

        // Try to deserialize if it's a JSON string
        if (response.Result is string jsonResult)
        {
            try
            {
                var deserializedResult = JsonSerializer.Deserialize<TResponse>(jsonResult);
                if (deserializedResult != null)
                {
                    return deserializedResult;
                }
            }
            catch (JsonException ex)
            {
                _logger.LogWarning(ex, "Failed to deserialize agent response as {ResponseType}", typeof(TResponse).Name);
            }
        }

        throw new InvalidOperationException($"Agent response type mismatch. Expected {typeof(TResponse).Name}, got {response.Result?.GetType().Name}");
    }

    private async Task LogCommunicationAsync<TRequest>(
        AgentMessage<TRequest>? envelope, 
        object? response, 
        bool success, 
        Exception? exception = null)
    {
        try
        {
            var logEntry = new AgentCommunicationLogEntity
            {
                Id = Guid.NewGuid().ToString(),
                CorrelationId = envelope?.CorrelationId ?? "unknown",
                SourceAgent = envelope?.SourceAgent ?? "unknown",
                TargetAgent = envelope?.TargetAgent ?? "unknown",
                MessageType = typeof(TRequest).Name,
                Success = success,
                ExecutionTimeMs = 0, // Would be calculated from timing
                ErrorMessage = exception?.Message,
                CreatedAt = DateTime.UtcNow,
                MetadataJson = JsonSerializer.Serialize(new Dictionary<string, object>
                {
                    ["HasResponse"] = response != null,
                    ["ResponseType"] = response?.GetType().Name ?? "null",
                    ["ExceptionType"] = exception?.GetType().Name ?? "none"
                })
            };

            _context.Set<AgentCommunicationLogEntity>().Add(logEntry);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to log agent communication");
        }
    }

    #endregion
}

/// <summary>
/// Entity for storing agent communication logs in database
/// </summary>
public class AgentCommunicationLogEntity
{
    public string Id { get; set; } = string.Empty;
    public string CorrelationId { get; set; } = string.Empty;
    public string SourceAgent { get; set; } = string.Empty;
    public string TargetAgent { get; set; } = string.Empty;
    public string MessageType { get; set; } = string.Empty;
    public bool Success { get; set; }
    public long ExecutionTimeMs { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? MetadataJson { get; set; }
}
