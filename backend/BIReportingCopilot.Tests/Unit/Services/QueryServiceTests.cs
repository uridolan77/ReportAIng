using NUnit.Framework;
using Moq;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using FluentAssertions;
using BIReportingCopilot.Infrastructure.Services;
using BIReportingCopilot.Infrastructure.Data;
using BIReportingCopilot.Infrastructure.Data.Entities;
using BIReportingCopilot.Core.Interfaces;
using BIReportingCopilot.Core.Models;
using System.Text.Json;

namespace BIReportingCopilot.Tests.Unit.Services;

[TestFixture]
public class QueryServiceTests
{
    private Mock<ILogger<QueryService>> _mockLogger;
    private Mock<IAIService> _mockAIService;
    private Mock<ISchemaService> _mockSchemaService;
    private Mock<ISqlQueryService> _mockSqlQueryService;
    private Mock<IAuditService> _mockAuditService;
    private Mock<ICacheService> _mockCacheService;
    private Mock<IPromptService> _mockPromptService;
    private Mock<IAITuningSettingsService> _mockSettingsService;
    private BICopilotContext _context;
    private QueryService _queryService;

    [SetUp]
    public void Setup()
    {
        _mockLogger = new Mock<ILogger<QueryService>>();
        _mockAIService = new Mock<IAIService>();
        _mockSchemaService = new Mock<ISchemaService>();
        _mockSqlQueryService = new Mock<ISqlQueryService>();
        _mockAuditService = new Mock<IAuditService>();
        _mockCacheService = new Mock<ICacheService>();
        _mockPromptService = new Mock<IPromptService>();
        _mockSettingsService = new Mock<IAITuningSettingsService>();

        var options = new DbContextOptionsBuilder<BICopilotContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new BICopilotContext(options);

        _queryService = new QueryService(
            _mockLogger.Object,
            _mockAIService.Object,
            _mockSchemaService.Object,
            _mockSqlQueryService.Object,
            _mockCacheService.Object,
            _mockAuditService.Object,
            _mockPromptService.Object,
            _mockSettingsService.Object,
            _context
        );
    }

    [TearDown]
    public void TearDown()
    {
        _context.Dispose();
    }

    [Test]
    public async Task GetQueryHistoryAsync_WithValidUserId_ReturnsQueryHistory()
    {
        // Arrange
        var userId = "test-user-id";
        var auditLogs = new List<AuditLogEntry>
        {
            new AuditLogEntry
            {
                Id = 1,
                UserId = userId,
                Action = "QUERY_EXECUTED",
                Timestamp = DateTime.UtcNow.AddHours(-1),
                Details = JsonSerializer.Serialize(new
                {
                    NaturalLanguageQuery = "Show me sales data",
                    GeneratedSQL = "SELECT * FROM Sales",
                    ExecutionTimeMs = 150,
                    Error = (string?)null
                })
            },
            new AuditLogEntry
            {
                Id = 2,
                UserId = userId,
                Action = "QUERY_EXECUTED",
                Timestamp = DateTime.UtcNow.AddHours(-2),
                Details = JsonSerializer.Serialize(new
                {
                    NaturalLanguageQuery = "Get customer count",
                    GeneratedSQL = "SELECT COUNT(*) FROM Customers",
                    ExecutionTimeMs = 75,
                    Error = "Table not found"
                })
            }
        };

        _mockAuditService.Setup(x => x.GetAuditLogsAsync(userId, null, null, "QUERY_EXECUTED"))
            .ReturnsAsync(auditLogs);

        // Act
        var result = await _queryService.GetQueryHistoryAsync(userId, 1, 10);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);

        var firstQuery = result.First();
        firstQuery.Question.Should().Be("Show me sales data");
        firstQuery.Sql.Should().Be("SELECT * FROM Sales");
        firstQuery.ExecutionTimeMs.Should().Be(150);
        firstQuery.Successful.Should().BeTrue();
        firstQuery.Error.Should().BeNull();

        var secondQuery = result.Last();
        secondQuery.Question.Should().Be("Get customer count");
        secondQuery.Sql.Should().Be("SELECT COUNT(*) FROM Customers");
        secondQuery.ExecutionTimeMs.Should().Be(75);
        secondQuery.Successful.Should().BeFalse();
        secondQuery.Error.Should().Be("Table not found");
    }

    [Test]
    public async Task GetQueryHistoryAsync_WithPagination_ReturnsCorrectPage()
    {
        // Arrange
        var userId = "test-user-id";
        var auditLogs = Enumerable.Range(1, 25)
            .Select(i => new AuditLogEntry
            {
                Id = i,
                UserId = userId,
                Action = "QUERY_EXECUTED",
                Timestamp = DateTime.UtcNow.AddHours(-i),
                Details = JsonSerializer.Serialize(new
                {
                    NaturalLanguageQuery = $"Query {i}",
                    GeneratedSQL = $"SELECT * FROM Table{i}",
                    ExecutionTimeMs = i * 10
                })
            })
            .ToList();

        _mockAuditService.Setup(x => x.GetAuditLogsAsync(userId, null, null, "QUERY_EXECUTED"))
            .ReturnsAsync(auditLogs);

        // Act
        var result = await _queryService.GetQueryHistoryAsync(userId, 2, 10);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(10);
        result.First().Question.Should().Be("Query 11");
        result.Last().Question.Should().Be("Query 20");
    }

    [Test]
    public async Task GetQueryHistoryAsync_WithEmptyAuditLogs_ReturnsEmptyList()
    {
        // Arrange
        var userId = "test-user-id";
        _mockAuditService.Setup(x => x.GetAuditLogsAsync(userId, null, null, "QUERY_EXECUTED"))
            .ReturnsAsync(new List<AuditLogEntry>());

        // Act
        var result = await _queryService.GetQueryHistoryAsync(userId, 1, 10);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Test]
    public async Task GetQueryHistoryAsync_WithException_ReturnsEmptyListAndLogsError()
    {
        // Arrange
        var userId = "test-user-id";
        _mockAuditService.Setup(x => x.GetAuditLogsAsync(userId, null, null, "QUERY_EXECUTED"))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _queryService.GetQueryHistoryAsync(userId, 1, 10);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error retrieving query history")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Test]
    public async Task InvalidateQueryCacheAsync_WithValidPattern_CallsCacheService()
    {
        // Arrange
        var pattern = "*sales*";

        // Act
        await _queryService.InvalidateQueryCacheAsync(pattern);

        // Assert
        _mockCacheService.Verify(x => x.RemovePatternAsync(pattern), Times.Once);
    }

    [Test]
    public void ExtractNaturalLanguageQuery_WithValidJson_ReturnsQuery()
    {
        // Arrange
        var details = JsonSerializer.Serialize(new
        {
            NaturalLanguageQuery = "Show me sales data",
            GeneratedSQL = "SELECT * FROM Sales"
        });

        // Act
        var result = _queryService.GetType()
            .GetMethod("ExtractNaturalLanguageQuery", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.Invoke(_queryService, new object[] { details }) as string;

        // Assert
        result.Should().Be("Show me sales data");
    }

    [Test]
    public void ExtractExecutionTime_WithValidJson_ReturnsTime()
    {
        // Arrange
        var details = JsonSerializer.Serialize(new
        {
            ExecutionTimeMs = 150.5
        });

        // Act
        var result = _queryService.GetType()
            .GetMethod("ExtractExecutionTime", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.Invoke(_queryService, new object[] { details });

        // Assert
        result.Should().Be(150.5);
    }

    [Test]
    public void ExtractSuccessStatus_WithNoError_ReturnsTrue()
    {
        // Arrange
        var details = JsonSerializer.Serialize(new
        {
            NaturalLanguageQuery = "Show me sales data",
            Error = (string?)null
        });

        // Act
        var result = _queryService.GetType()
            .GetMethod("ExtractSuccessStatus", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.Invoke(_queryService, new object[] { details });

        // Assert
        result.Should().Be(true);
    }

    [Test]
    public void ExtractSuccessStatus_WithError_ReturnsFalse()
    {
        // Arrange
        var details = JsonSerializer.Serialize(new
        {
            NaturalLanguageQuery = "Show me sales data",
            Error = "Table not found"
        });

        // Act
        var result = _queryService.GetType()
            .GetMethod("ExtractSuccessStatus", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.Invoke(_queryService, new object[] { details });

        // Assert
        result.Should().Be(false);
    }
}
