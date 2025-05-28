using Xunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using BIReportingCopilot.Infrastructure.Messaging;
using BIReportingCopilot.Infrastructure.Messaging.EventHandlers;
using BIReportingCopilot.Tests.Infrastructure;
using BIReportingCopilot.Tests.Infrastructure.Fixtures;

namespace BIReportingCopilot.Tests.Integration;

/// <summary>
/// Integration tests for event bus functionality
/// </summary>
[Collection("WebApplication")]
public class EventBusIntegrationTests : IntegrationTestBase
{
    private readonly InMemoryEventBus _eventBus;
    private readonly List<IEvent> _capturedEvents;
    private readonly QueryExecutedEventHandler _queryHandler;

    public EventBusIntegrationTests()
    {
        _capturedEvents = new List<IEvent>();
        
        var config = Options.Create(new EventBusConfiguration
        {
            MaxConcurrentHandlers = 5,
            EnableRetries = true,
            MaxRetryAttempts = 3
        });

        _eventBus = new InMemoryEventBus(GetLogger<InMemoryEventBus>(), config);
        
        _queryHandler = new QueryExecutedEventHandler(
            GetLogger<QueryExecutedEventHandler>(),
            GetService<BIReportingCopilot.Infrastructure.Monitoring.IMetricsCollector>(),
            GetService<BIReportingCopilot.Infrastructure.AI.ISemanticCacheService>(),
            GetService<BIReportingCopilot.Infrastructure.AI.MLAnomalyDetector>());
    }

    [Fact]
    public async Task PublishAsync_WithValidEvent_ShouldPublishSuccessfully()
    {
        // Arrange
        await _eventBus.StartAsync();
        
        var queryEvent = new QueryExecutedEvent
        {
            UserId = "test-user",
            NaturalLanguageQuery = "Show me all customers",
            GeneratedSQL = "SELECT * FROM Customers",
            IsSuccessful = true,
            ExecutionTimeMs = 150,
            RowCount = 100,
            ConfidenceScore = 0.85
        };

        // Act
        await _eventBus.PublishAsync(queryEvent);

        // Wait for processing
        await Task.Delay(500);

        // Assert
        queryEvent.EventId.Should().NotBeNullOrEmpty();
        queryEvent.Timestamp.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        queryEvent.EventType.Should().Be("QueryExecuted");
    }

    [Fact]
    public async Task SubscribeAsync_WithEventHandler_ShouldReceiveEvents()
    {
        // Arrange
        await _eventBus.StartAsync();
        
        var receivedEvents = new List<QueryExecutedEvent>();
        
        await _eventBus.SubscribeAsync<QueryExecutedEvent>(async (evt, ct) =>
        {
            receivedEvents.Add(evt);
            await Task.CompletedTask;
        });

        var testEvent = new QueryExecutedEvent
        {
            UserId = "test-user",
            NaturalLanguageQuery = "Count orders",
            GeneratedSQL = "SELECT COUNT(*) FROM Orders",
            IsSuccessful = true,
            ExecutionTimeMs = 200,
            RowCount = 1
        };

        // Act
        await _eventBus.PublishAsync(testEvent);

        // Wait for processing
        await AssertEventuallyAsync(() => receivedEvents.Count > 0, TimeSpan.FromSeconds(2));

        // Assert
        receivedEvents.Should().HaveCount(1);
        receivedEvents[0].UserId.Should().Be("test-user");
        receivedEvents[0].NaturalLanguageQuery.Should().Be("Count orders");
    }

    [Fact]
    public async Task EventHandlers_WithQueryExecutedEvent_ShouldProcessCorrectly()
    {
        // Arrange
        await _eventBus.StartAsync();
        
        // Subscribe the actual handler
        await _eventBus.SubscribeAsync<QueryExecutedEvent>(_queryHandler.HandleAsync);

        var queryEvent = new QueryExecutedEvent
        {
            UserId = "test-user",
            NaturalLanguageQuery = "Show sales data",
            GeneratedSQL = "SELECT * FROM Sales WHERE Date >= '2024-01-01'",
            IsSuccessful = true,
            ExecutionTimeMs = 300,
            RowCount = 500,
            ConfidenceScore = 0.92
        };

        var logCapture = CaptureLogsFor<QueryExecutedEventHandler>();

        // Act
        await _eventBus.PublishAsync(queryEvent);

        // Wait for processing
        await Task.Delay(1000);

        // Assert
        logCapture.AssertLogEntry(LogLevel.Information, "Processing QueryExecutedEvent for user test-user");
    }

    [Fact]
    public async Task EventBus_WithMultipleSubscribers_ShouldDeliverToAll()
    {
        // Arrange
        await _eventBus.StartAsync();
        
        var handler1Events = new List<FeedbackReceivedEvent>();
        var handler2Events = new List<FeedbackReceivedEvent>();

        await _eventBus.SubscribeAsync<FeedbackReceivedEvent>(async (evt, ct) =>
        {
            handler1Events.Add(evt);
            await Task.CompletedTask;
        });

        await _eventBus.SubscribeAsync<FeedbackReceivedEvent>(async (evt, ct) =>
        {
            handler2Events.Add(evt);
            await Task.CompletedTask;
        });

        var feedbackEvent = new FeedbackReceivedEvent
        {
            UserId = "test-user",
            QueryId = "query-123",
            Rating = 4,
            Comments = "Good query",
            FeedbackType = "Rating"
        };

        // Act
        await _eventBus.PublishAsync(feedbackEvent);

        // Wait for processing
        await AssertEventuallyAsync(() => handler1Events.Count > 0 && handler2Events.Count > 0, 
            TimeSpan.FromSeconds(2));

        // Assert
        handler1Events.Should().HaveCount(1);
        handler2Events.Should().HaveCount(1);
        handler1Events[0].Rating.Should().Be(4);
        handler2Events[0].Rating.Should().Be(4);
    }

    [Fact]
    public async Task EventBus_WithFailingHandler_ShouldRetryAndContinue()
    {
        // Arrange
        await _eventBus.StartAsync();
        
        var attemptCount = 0;
        var successfulEvents = new List<AnomalyDetectedEvent>();

        await _eventBus.SubscribeAsync<AnomalyDetectedEvent>(async (evt, ct) =>
        {
            attemptCount++;
            if (attemptCount <= 2) // Fail first 2 attempts
            {
                throw new InvalidOperationException("Simulated handler failure");
            }
            successfulEvents.Add(evt);
            await Task.CompletedTask;
        });

        var anomalyEvent = new AnomalyDetectedEvent
        {
            UserId = "test-user",
            AnomalyType = "HighFrequency",
            AnomalyScore = 0.85,
            RiskLevel = "Medium",
            DetectedAnomalies = new List<string> { "Unusual query frequency" },
            Query = "SELECT * FROM SensitiveData"
        };

        var logCapture = CaptureLogsFor<InMemoryEventBus>();

        // Act
        await _eventBus.PublishAsync(anomalyEvent);

        // Wait for retries and processing
        await AssertEventuallyAsync(() => successfulEvents.Count > 0, TimeSpan.FromSeconds(5));

        // Assert
        successfulEvents.Should().HaveCount(1);
        attemptCount.Should().Be(3); // Initial attempt + 2 retries
        logCapture.HasLogEntry(LogLevel.Warning, "Retrying event").Should().BeTrue();
    }

    [Fact]
    public async Task EventBus_WithRoutingKeys_ShouldRouteCorrectly()
    {
        // Arrange
        await _eventBus.StartAsync();
        
        var securityEvents = new List<AnomalyDetectedEvent>();
        var performanceEvents = new List<PerformanceMetricsEvent>();

        await _eventBus.SubscribeAsync<AnomalyDetectedEvent>("security.*", async (evt, ct) =>
        {
            securityEvents.Add(evt);
            await Task.CompletedTask;
        });

        await _eventBus.SubscribeAsync<PerformanceMetricsEvent>("performance.*", async (evt, ct) =>
        {
            performanceEvents.Add(evt);
            await Task.CompletedTask;
        });

        var securityEvent = new AnomalyDetectedEvent
        {
            UserId = "test-user",
            AnomalyType = "SecurityThreat",
            AnomalyScore = 0.95,
            RiskLevel = "High"
        };

        var performanceEvent = new PerformanceMetricsEvent
        {
            CpuUsagePercent = 75.5,
            MemoryUsageMB = 512,
            ActiveConnections = 25,
            CacheHitRate = 0.85,
            TotalQueries = 1000,
            AverageResponseTimeMs = 150
        };

        // Act
        await _eventBus.PublishAsync(securityEvent, "security.anomaly");
        await _eventBus.PublishAsync(performanceEvent, "performance.metrics");

        // Wait for processing
        await AssertEventuallyAsync(() => securityEvents.Count > 0 && performanceEvents.Count > 0, 
            TimeSpan.FromSeconds(2));

        // Assert
        securityEvents.Should().HaveCount(1);
        performanceEvents.Should().HaveCount(1);
        securityEvents[0].AnomalyType.Should().Be("SecurityThreat");
        performanceEvents[0].CpuUsagePercent.Should().Be(75.5);
    }

    [Fact]
    public async Task EventSerialization_ShouldPreserveEventData()
    {
        // Arrange
        var originalEvent = new SchemaChangedEvent
        {
            TableName = "Customers",
            ChangeType = "Modified",
            ColumnName = "Email",
            ChangeDetails = new Dictionary<string, object>
            {
                ["OldType"] = "varchar(100)",
                ["NewType"] = "varchar(255)",
                ["Reason"] = "Increased email length limit"
            }
        };

        // Act
        var serialized = EventSerializer.Serialize(originalEvent);
        var deserialized = EventSerializer.Deserialize<SchemaChangedEvent>(serialized);

        // Assert
        deserialized.Should().NotBeNull();
        deserialized!.EventId.Should().Be(originalEvent.EventId);
        deserialized.TableName.Should().Be("Customers");
        deserialized.ChangeType.Should().Be("Modified");
        deserialized.ColumnName.Should().Be("Email");
        deserialized.ChangeDetails.Should().ContainKey("OldType");
        deserialized.ChangeDetails["NewType"].Should().Be("varchar(255)");
    }

    [Fact]
    public async Task EventBus_UnderLoad_ShouldHandleConcurrentEvents()
    {
        // Arrange
        await _eventBus.StartAsync();
        
        var processedEvents = new List<QueryExecutedEvent>();
        var lockObject = new object();

        await _eventBus.SubscribeAsync<QueryExecutedEvent>(async (evt, ct) =>
        {
            // Simulate some processing time
            await Task.Delay(10, ct);
            
            lock (lockObject)
            {
                processedEvents.Add(evt);
            }
        });

        var events = Enumerable.Range(0, 50)
            .Select(i => new QueryExecutedEvent
            {
                UserId = $"user-{i % 5}",
                NaturalLanguageQuery = $"Query {i}",
                GeneratedSQL = $"SELECT * FROM Table{i}",
                IsSuccessful = true,
                ExecutionTimeMs = 100 + i,
                RowCount = i * 10
            })
            .ToList();

        // Act
        var publishTasks = events.Select(evt => _eventBus.PublishAsync(evt));
        await Task.WhenAll(publishTasks);

        // Wait for all events to be processed
        await AssertEventuallyAsync(() => processedEvents.Count == events.Count, TimeSpan.FromSeconds(10));

        // Assert
        processedEvents.Should().HaveCount(events.Count);
        
        // Verify all events were processed
        var processedQueries = processedEvents.Select(e => e.NaturalLanguageQuery).ToHashSet();
        var originalQueries = events.Select(e => e.NaturalLanguageQuery).ToHashSet();
        processedQueries.Should().BeEquivalentTo(originalQueries);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _eventBus?.StopAsync().Wait(TimeSpan.FromSeconds(5));
            _eventBus?.Dispose();
        }
        base.Dispose(disposing);
    }
}
