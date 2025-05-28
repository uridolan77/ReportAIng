using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;
using System.Reflection;

namespace BIReportingCopilot.Infrastructure.Messaging;

/// <summary>
/// In-memory event bus implementation for development and testing
/// </summary>
public class InMemoryEventBus : IEventBus, IDisposable
{
    private readonly ILogger<InMemoryEventBus> _logger;
    private readonly EventBusConfiguration _config;
    private readonly ConcurrentDictionary<string, List<EventSubscription>> _subscriptions;
    private readonly ConcurrentQueue<EventMessage> _eventQueue;
    private readonly SemaphoreSlim _processingLock;
    private readonly CancellationTokenSource _cancellationTokenSource;
    private readonly Task _processingTask;
    private bool _isStarted;
    private bool _disposed;

    public InMemoryEventBus(ILogger<InMemoryEventBus> logger, IOptions<EventBusConfiguration> config)
    {
        _logger = logger;
        _config = config.Value;
        _subscriptions = new ConcurrentDictionary<string, List<EventSubscription>>();
        _eventQueue = new ConcurrentQueue<EventMessage>();
        _processingLock = new SemaphoreSlim(_config.MaxConcurrentHandlers, _config.MaxConcurrentHandlers);
        _cancellationTokenSource = new CancellationTokenSource();
        _processingTask = ProcessEventsAsync(_cancellationTokenSource.Token);
    }

    public async Task PublishAsync<T>(T @event, CancellationToken cancellationToken = default) where T : class, IEvent
    {
        await PublishAsync(@event, @event.EventType, cancellationToken);
    }

    public async Task PublishAsync<T>(T @event, string routingKey, CancellationToken cancellationToken = default) where T : class, IEvent
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(InMemoryEventBus));

        try
        {
            var eventMessage = new EventMessage
            {
                EventId = @event.EventId,
                EventType = @event.EventType,
                RoutingKey = routingKey,
                Payload = EventSerializer.Serialize(@event),
                Timestamp = @event.Timestamp,
                Source = @event.Source,
                Metadata = @event.Metadata
            };

            _eventQueue.Enqueue(eventMessage);

            _logger.LogDebug("Published event {EventType} with ID {EventId} to routing key {RoutingKey}", 
                @event.EventType, @event.EventId, routingKey);

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error publishing event {EventType} with ID {EventId}", 
                @event.EventType, @event.EventId);
            throw;
        }
    }

    public async Task SubscribeAsync<T>(Func<T, CancellationToken, Task> handler, CancellationToken cancellationToken = default) where T : class, IEvent
    {
        var eventType = typeof(T).Name;
        await SubscribeAsync(eventType, handler, cancellationToken);
    }

    public async Task SubscribeAsync<T>(string routingKey, Func<T, CancellationToken, Task> handler, CancellationToken cancellationToken = default) where T : class, IEvent
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(InMemoryEventBus));

        var subscription = new EventSubscription
        {
            RoutingKey = routingKey,
            EventType = typeof(T),
            Handler = async (eventData, ct) =>
            {
                var @event = EventSerializer.Deserialize<T>(eventData);
                if (@event != null)
                {
                    await handler(@event, ct);
                }
            }
        };

        _subscriptions.AddOrUpdate(routingKey, 
            new List<EventSubscription> { subscription },
            (key, existing) =>
            {
                existing.Add(subscription);
                return existing;
            });

        _logger.LogInformation("Subscribed to events with routing key {RoutingKey} for type {EventType}", 
            routingKey, typeof(T).Name);

        await Task.CompletedTask;
    }

    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        if (_isStarted)
            return;

        _isStarted = true;
        _logger.LogInformation("In-memory event bus started");
        await Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken = default)
    {
        if (!_isStarted)
            return;

        _isStarted = false;
        _cancellationTokenSource.Cancel();

        try
        {
            await _processingTask;
        }
        catch (OperationCanceledException)
        {
            // Expected when cancellation is requested
        }

        _logger.LogInformation("In-memory event bus stopped");
    }

    private async Task ProcessEventsAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Event processing started");

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                if (_eventQueue.TryDequeue(out var eventMessage))
                {
                    await ProcessEventAsync(eventMessage, cancellationToken);
                }
                else
                {
                    // No events to process, wait a bit
                    await Task.Delay(100, cancellationToken);
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in event processing loop");
                await Task.Delay(1000, cancellationToken); // Wait before retrying
            }
        }

        _logger.LogInformation("Event processing stopped");
    }

    private async Task ProcessEventAsync(EventMessage eventMessage, CancellationToken cancellationToken)
    {
        await _processingLock.WaitAsync(cancellationToken);

        try
        {
            var handlers = GetHandlersForEvent(eventMessage.RoutingKey);
            
            if (!handlers.Any())
            {
                _logger.LogDebug("No handlers found for event {EventType} with routing key {RoutingKey}", 
                    eventMessage.EventType, eventMessage.RoutingKey);
                return;
            }

            var tasks = handlers.Select(async handler =>
            {
                try
                {
                    using var timeoutCts = new CancellationTokenSource(_config.MessageTimeout);
                    using var combinedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);

                    await handler.Handler(eventMessage.Payload, combinedCts.Token);

                    _logger.LogDebug("Successfully processed event {EventId} with handler {HandlerType}", 
                        eventMessage.EventId, handler.EventType.Name);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing event {EventId} with handler {HandlerType}: {Error}", 
                        eventMessage.EventId, handler.EventType.Name, ex.Message);

                    if (_config.EnableRetries)
                    {
                        await RetryEventAsync(eventMessage, handler, ex);
                    }
                }
            });

            await Task.WhenAll(tasks);
        }
        finally
        {
            _processingLock.Release();
        }
    }

    private List<EventSubscription> GetHandlersForEvent(string routingKey)
    {
        var handlers = new List<EventSubscription>();

        // Exact match
        if (_subscriptions.TryGetValue(routingKey, out var exactHandlers))
        {
            handlers.AddRange(exactHandlers);
        }

        // Wildcard matching (simple implementation)
        foreach (var subscription in _subscriptions)
        {
            if (subscription.Key.Contains("*") && IsWildcardMatch(routingKey, subscription.Key))
            {
                handlers.AddRange(subscription.Value);
            }
        }

        return handlers;
    }

    private bool IsWildcardMatch(string routingKey, string pattern)
    {
        // Simple wildcard matching - replace * with .*
        var regexPattern = pattern.Replace("*", ".*");
        return System.Text.RegularExpressions.Regex.IsMatch(routingKey, regexPattern);
    }

    private async Task RetryEventAsync(EventMessage eventMessage, EventSubscription subscription, Exception lastException)
    {
        var retryCount = eventMessage.RetryCount + 1;
        
        if (retryCount > _config.MaxRetryAttempts)
        {
            _logger.LogError("Event {EventId} exceeded maximum retry attempts ({MaxRetries}). Moving to dead letter queue.", 
                eventMessage.EventId, _config.MaxRetryAttempts);
            
            if (_config.EnableDeadLetterQueue)
            {
                await SendToDeadLetterQueueAsync(eventMessage, lastException);
            }
            return;
        }

        eventMessage.RetryCount = retryCount;
        eventMessage.LastError = lastException.Message;

        // Calculate exponential backoff delay
        var delay = TimeSpan.FromMilliseconds(_config.RetryDelay.TotalMilliseconds * Math.Pow(2, retryCount - 1));
        
        _logger.LogWarning("Retrying event {EventId} (attempt {RetryCount}/{MaxRetries}) after {Delay}ms", 
            eventMessage.EventId, retryCount, _config.MaxRetryAttempts, delay.TotalMilliseconds);

        // Schedule retry
        _ = Task.Run(async () =>
        {
            await Task.Delay(delay);
            _eventQueue.Enqueue(eventMessage);
        });
    }

    private async Task SendToDeadLetterQueueAsync(EventMessage eventMessage, Exception lastException)
    {
        try
        {
            // In a real implementation, this would send to a dead letter queue
            // For now, just log the failed event
            _logger.LogError("Dead letter event {EventId}: {EventType} - {Error}", 
                eventMessage.EventId, eventMessage.EventType, lastException.Message);

            // Could store in database, file, or external dead letter queue
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending event {EventId} to dead letter queue", eventMessage.EventId);
        }
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;
        _cancellationTokenSource.Cancel();

        try
        {
            _processingTask?.Wait(TimeSpan.FromSeconds(5));
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error waiting for processing task to complete during disposal");
        }

        _cancellationTokenSource.Dispose();
        _processingLock.Dispose();
        
        _logger.LogInformation("In-memory event bus disposed");
    }

    private class EventMessage
    {
        public string EventId { get; set; } = string.Empty;
        public string EventType { get; set; } = string.Empty;
        public string RoutingKey { get; set; } = string.Empty;
        public string Payload { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public string Source { get; set; } = string.Empty;
        public Dictionary<string, object> Metadata { get; set; } = new();
        public int RetryCount { get; set; }
        public string? LastError { get; set; }
    }

    private class EventSubscription
    {
        public string RoutingKey { get; set; } = string.Empty;
        public Type EventType { get; set; } = typeof(object);
        public Func<string, CancellationToken, Task> Handler { get; set; } = (_, _) => Task.CompletedTask;
    }
}
