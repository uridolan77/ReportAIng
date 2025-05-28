using Moq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Caching.Memory;
using System.Diagnostics;
using BIReportingCopilot.Core.Interfaces;
using BIReportingCopilot.Core.Models;
using BIReportingCopilot.Infrastructure.Messaging;
using BIReportingCopilot.Infrastructure.Monitoring;
using BIReportingCopilot.Infrastructure.AI;

namespace BIReportingCopilot.Tests.Infrastructure.Builders;

/// <summary>
/// Builder for creating mock services with pre-configured behaviors
/// </summary>
public class MockServiceBuilder
{
    private readonly Dictionary<Type, Mock> _mocks = new();

    /// <summary>
    /// Create or get a mock for the specified service type
    /// </summary>
    public Mock<T> For<T>() where T : class
    {
        if (_mocks.TryGetValue(typeof(T), out var existingMock))
        {
            return (Mock<T>)existingMock;
        }

        var mock = new Mock<T>();
        _mocks[typeof(T)] = mock;
        return mock;
    }

    /// <summary>
    /// Get the mock object instance
    /// </summary>
    public T Object<T>() where T : class
    {
        return For<T>().Object;
    }

    /// <summary>
    /// Create a mock AI service with default behaviors
    /// </summary>
    public MockServiceBuilder WithMockAIService(Action<Mock<IAIService>>? configure = null)
    {
        var mock = For<IAIService>();

        // Default behaviors
        mock.Setup(x => x.GenerateSQLAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("SELECT * FROM TestTable");

        mock.Setup(x => x.CalculateConfidenceScoreAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(0.85);

        mock.Setup(x => x.GenerateQuerySuggestionsAsync(It.IsAny<string>(), It.IsAny<SchemaMetadata>()))
            .ReturnsAsync(new[] { "Show all customers", "Count total orders", "Average sales amount" });

        mock.Setup(x => x.GenerateInsightAsync(It.IsAny<string>(), It.IsAny<object[]>()))
            .ReturnsAsync("This data shows a positive trend in sales.");

        mock.Setup(x => x.ValidateQueryIntentAsync(It.IsAny<string>()))
            .ReturnsAsync(true);

        configure?.Invoke(mock);
        return this;
    }

    /// <summary>
    /// Create a mock query service with default behaviors
    /// </summary>
    public MockServiceBuilder WithMockQueryService(Action<Mock<IQueryService>>? configure = null)
    {
        var mock = For<IQueryService>();

        // Default behaviors
        mock.Setup(x => x.ProcessQueryAsync(It.IsAny<QueryRequest>(), It.IsAny<string>()))
            .ReturnsAsync(new QueryResponse
            {
                Success = true,
                Result = new QueryResult
                {
                    Data = new object[] { },
                    Metadata = new QueryMetadata { RowCount = 0 }
                },
                ExecutionTimeMs = 100,
                Sql = "SELECT * FROM TestTable"
            });

        configure?.Invoke(mock);
        return this;
    }

    /// <summary>
    /// Create a mock schema service with default behaviors
    /// </summary>
    public MockServiceBuilder WithMockSchemaService(Action<Mock<ISchemaService>>? configure = null)
    {
        var mock = For<ISchemaService>();

        // Default behaviors
        mock.Setup(x => x.GetSchemaAsync(It.IsAny<string>()))
            .ReturnsAsync(new SchemaMetadata
            {
                DatabaseName = "TestDatabase",
                Tables = new List<TableMetadata>
                {
                    new TableMetadata
                    {
                        Name = "Customers",
                        Schema = "dbo",
                        Columns = new List<ColumnMetadata>
                        {
                            new ColumnMetadata { Name = "Id", DataType = "int", IsPrimaryKey = true },
                            new ColumnMetadata { Name = "Name", DataType = "varchar", MaxLength = 255 },
                            new ColumnMetadata { Name = "Email", DataType = "varchar", MaxLength = 255 }
                        },
                        RowCount = 1000
                    }
                },
                LastUpdated = DateTime.UtcNow
            });

        mock.Setup(x => x.GetTableNamesAsync(It.IsAny<string>()))
            .ReturnsAsync(new List<string> { "Customers", "Orders", "Products" });

        configure?.Invoke(mock);
        return this;
    }

    /// <summary>
    /// Create a mock cache service with default behaviors
    /// </summary>
    public MockServiceBuilder WithMockCacheService(Action<Mock<ICacheService>>? configure = null)
    {
        var mock = For<ICacheService>();

        // Default behaviors - cache misses by default
        mock.Setup(x => x.GetAsync<It.IsAnyType>(It.IsAny<string>()))
            .Returns<string>(key => Task.FromResult(default(It.IsAnyType)));

        mock.Setup(x => x.SetAsync(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<TimeSpan?>()))
            .Returns(Task.CompletedTask);

        mock.Setup(x => x.RemoveAsync(It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        mock.Setup(x => x.ExistsAsync(It.IsAny<string>()))
            .ReturnsAsync(false);

        configure?.Invoke(mock);
        return this;
    }

    /// <summary>
    /// Create a mock event bus with default behaviors
    /// </summary>
    public MockServiceBuilder WithMockEventBus(Action<Mock<IEventBus>>? configure = null)
    {
        var mock = For<IEventBus>();

        // Default behaviors
        mock.Setup(x => x.PublishAsync(It.IsAny<IEvent>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        mock.Setup(x => x.PublishAsync(It.IsAny<IEvent>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        mock.Setup(x => x.StartAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        mock.Setup(x => x.StopAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        configure?.Invoke(mock);
        return this;
    }

    /// <summary>
    /// Create a mock metrics collector with default behaviors
    /// </summary>
    public MockServiceBuilder WithMockMetricsCollector(Action<Mock<IMetricsCollector>>? configure = null)
    {
        var mock = For<IMetricsCollector>();

        // Default behaviors - all methods are void, so just verify they can be called
        mock.Setup(x => x.IncrementCounter(It.IsAny<string>(), It.IsAny<TagList?>()));
        mock.Setup(x => x.RecordHistogram(It.IsAny<string>(), It.IsAny<double>(), It.IsAny<TagList?>()));
        mock.Setup(x => x.SetGaugeValue(It.IsAny<string>(), It.IsAny<double>()));

        configure?.Invoke(mock);
        return this;
    }

    /// <summary>
    /// Create a mock semantic cache service with default behaviors
    /// </summary>
    public MockServiceBuilder WithMockSemanticCacheService(Action<Mock<ISemanticCacheService>>? configure = null)
    {
        var mock = For<ISemanticCacheService>();

        // Default behaviors - cache misses by default
        mock.Setup(x => x.GetSemanticallySimilarAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync((SemanticCacheResult?)null);

        mock.Setup(x => x.CacheSemanticQueryAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<QueryResponse>(),
                It.IsAny<TimeSpan?>()))
            .Returns(Task.CompletedTask);

        mock.Setup(x => x.GetCacheStatisticsAsync())
            .ReturnsAsync(new SemanticCacheStatistics
            {
                TotalEntries = 100,
                TotalAccesses = 500,
                HitRate = 0.75,
                LastUpdated = DateTime.UtcNow
            });

        configure?.Invoke(mock);
        return this;
    }

    /// <summary>
    /// Create a mock logger with default behaviors
    /// </summary>
    public MockServiceBuilder WithMockLogger<T>(Action<Mock<ILogger<T>>>? configure = null)
    {
        var mock = For<ILogger<T>>();

        // Default behavior - allow all logging calls
        mock.Setup(x => x.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()))
            .Verifiable();

        configure?.Invoke(mock);
        return this;
    }

    /// <summary>
    /// Create a failing AI service for error testing
    /// </summary>
    public MockServiceBuilder WithFailingAIService(string errorMessage = "AI service error")
    {
        var mock = For<IAIService>();

        mock.Setup(x => x.GenerateSQLAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException(errorMessage));

        mock.Setup(x => x.CalculateConfidenceScoreAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ThrowsAsync(new InvalidOperationException(errorMessage));

        return this;
    }

    /// <summary>
    /// Create a slow AI service for timeout testing
    /// </summary>
    public MockServiceBuilder WithSlowAIService(TimeSpan delay)
    {
        var mock = For<IAIService>();

        mock.Setup(x => x.GenerateSQLAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(async (string prompt, CancellationToken ct) =>
            {
                await Task.Delay(delay, ct);
                return "SELECT * FROM TestTable";
            });

        return this;
    }

    /// <summary>
    /// Create a cache service that simulates cache hits
    /// </summary>
    public MockServiceBuilder WithCacheHits<T>(Dictionary<string, T> cachedItems) where T : class
    {
        var mock = For<ICacheService>();

        mock.Setup(x => x.GetAsync<T>(It.IsAny<string>()))
            .Returns<string>(key => Task.FromResult(cachedItems.GetValueOrDefault(key)));

        mock.Setup(x => x.ExistsAsync(It.IsAny<string>()))
            .Returns<string>(key => Task.FromResult(cachedItems.ContainsKey(key)));

        return this;
    }

    /// <summary>
    /// Verify that a method was called on a mock
    /// </summary>
    public MockServiceBuilder Verify<T>(Action<Mock<T>> verification) where T : class
    {
        var mock = For<T>();
        verification(mock);
        return this;
    }

    /// <summary>
    /// Get all created mocks
    /// </summary>
    public Dictionary<Type, Mock> GetAllMocks()
    {
        return new Dictionary<Type, Mock>(_mocks);
    }

    /// <summary>
    /// Verify all mocks
    /// </summary>
    public void VerifyAll()
    {
        foreach (var mock in _mocks.Values)
        {
            mock.VerifyAll();
        }
    }

    /// <summary>
    /// Reset all mocks
    /// </summary>
    public void Reset()
    {
        foreach (var mock in _mocks.Values)
        {
            mock.Reset();
        }
    }
}

/// <summary>
/// Extension methods for easier mock configuration
/// </summary>
public static class MockServiceBuilderExtensions
{
    /// <summary>
    /// Configure a mock to return specific values for specific inputs
    /// </summary>
    public static MockServiceBuilder WithAIServiceResponses(this MockServiceBuilder builder,
        Dictionary<string, string> promptToSqlMap)
    {
        var mock = builder.For<IAIService>();

        foreach (var kvp in promptToSqlMap)
        {
            mock.Setup(x => x.GenerateSQLAsync(kvp.Key, It.IsAny<CancellationToken>()))
                .ReturnsAsync(kvp.Value);
        }

        return builder;
    }

    /// <summary>
    /// Configure a mock to simulate database query results
    /// </summary>
    public static MockServiceBuilder WithQueryResults(this MockServiceBuilder builder,
        Dictionary<string, QueryResponse> sqlToResultMap)
    {
        var mock = builder.For<IQueryService>();

        foreach (var kvp in sqlToResultMap)
        {
            mock.Setup(x => x.ProcessQueryAsync(It.IsAny<QueryRequest>(), It.IsAny<string>()))
                .ReturnsAsync(kvp.Value);
        }

        return builder;
    }

    /// <summary>
    /// Configure event bus to capture published events
    /// </summary>
    public static MockServiceBuilder WithEventCapture(this MockServiceBuilder builder,
        List<IEvent> capturedEvents)
    {
        var mock = builder.For<IEventBus>();

        mock.Setup(x => x.PublishAsync(It.IsAny<IEvent>(), It.IsAny<CancellationToken>()))
            .Callback<IEvent, CancellationToken>((evt, ct) => capturedEvents.Add(evt))
            .Returns(Task.CompletedTask);

        mock.Setup(x => x.PublishAsync(It.IsAny<IEvent>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Callback<IEvent, string, CancellationToken>((evt, key, ct) => capturedEvents.Add(evt))
            .Returns(Task.CompletedTask);

        return builder;
    }
}
