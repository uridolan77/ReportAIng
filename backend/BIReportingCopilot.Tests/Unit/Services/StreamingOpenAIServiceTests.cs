using Xunit;
using Moq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using BIReportingCopilot.Infrastructure.AI;
using BIReportingCopilot.Core.Interfaces;
using BIReportingCopilot.Core.Models;
using BIReportingCopilot.Infrastructure.Monitoring;
using Azure.AI.OpenAI;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using System.Linq;

namespace BIReportingCopilot.Tests.Unit.Services;

/// <summary>
/// Unit tests for AIService (formerly StreamingOpenAIService)
/// </summary>
public class AIServiceTests
{
    private readonly Mock<IAIProviderFactory> _mockProviderFactory;
    private readonly Mock<ILogger<AIService>> _mockLogger;
    private readonly Mock<ICacheService> _mockCacheService;
    private readonly Mock<IContextManager> _mockContextManager;
    private readonly Mock<IMetricsCollector> _mockMetricsCollector;
    private readonly AIService _service;

    public AIServiceTests()
    {
        _mockProviderFactory = new Mock<IAIProviderFactory>();
        _mockLogger = new Mock<ILogger<AIService>>();
        _mockCacheService = new Mock<ICacheService>();
        _mockContextManager = new Mock<IContextManager>();
        _mockMetricsCollector = new Mock<IMetricsCollector>();

        // Setup mock provider with streaming support
        var mockProvider = new Mock<IAIProvider>();
        mockProvider.Setup(x => x.ProviderName).Returns("TestProvider");

        // Setup streaming response - different responses based on prompt content
        mockProvider.Setup(x => x.GenerateCompletionStreamAsync(It.IsAny<string>(), It.IsAny<AIOptions>(), It.IsAny<CancellationToken>()))
            .Returns<string, AIOptions, CancellationToken>((prompt, options, token) => GetMockStreamingResponse(prompt));

        _mockProviderFactory.Setup(x => x.GetProvider()).Returns(mockProvider.Object);

        _service = new AIService(
            _mockProviderFactory.Object,
            _mockLogger.Object,
            _mockCacheService.Object,
            _mockContextManager.Object,
            _mockMetricsCollector.Object);
    }

    private static async IAsyncEnumerable<StreamingResponse> GetMockStreamingResponse(string prompt)
    {
        // Return different responses based on prompt content
        if (prompt.Contains("Generate SQL for:"))
        {
            yield return new StreamingResponse { Content = "SELECT * FROM Users", IsComplete = true, Type = StreamingResponseType.SQLGeneration };
        }
        else if (prompt.Contains("Analyze the following query"))
        {
            yield return new StreamingResponse { Content = "This query shows customer insights", IsComplete = true, Type = StreamingResponseType.Insight };
        }
        else if (prompt.Contains("Explain the following SQL"))
        {
            yield return new StreamingResponse { Content = "This SQL query selects all users", IsComplete = true, Type = StreamingResponseType.Explanation };
        }
        else if (string.IsNullOrEmpty(prompt))
        {
            yield return new StreamingResponse { Content = "Error: Prompt cannot be null or empty", IsComplete = true, Type = StreamingResponseType.Error };
        }
        else
        {
            // Default response
            yield return new StreamingResponse { Content = "Default response", IsComplete = true, Type = StreamingResponseType.SQLGeneration };
        }

        await Task.CompletedTask; // To make it async
    }

    [Fact]
    public async Task GenerateSQLStreamAsync_WithValidPrompt_ReturnsStreamingResponse()
    {
        // Arrange
        var prompt = "Show me all customers";
        var schema = new SchemaMetadata
        {
            Tables = new List<TableMetadata>
            {
                new TableMetadata
                {
                    Name = "Customers",
                    Description = "Customer information",
                    Columns = new List<ColumnMetadata>
                    {
                        new ColumnMetadata { Name = "Id", DataType = "int", IsPrimaryKey = true },
                        new ColumnMetadata { Name = "Name", DataType = "varchar(100)" }
                    }
                }
            }
        };

        var context = new QueryContext
        {
            UserId = "test-user",
            SessionId = "test-session",
            PreferredComplexity = StreamingQueryComplexity.Simple
        };

        _mockCacheService.Setup(x => x.GetAsync<string>(It.IsAny<string>()))
            .ReturnsAsync((string?)null);

        // Act & Assert
        var responses = new List<StreamingResponse>();
        await foreach (var response in _service.GenerateSQLStreamAsync(prompt, schema, context))
        {
            responses.Add(response);
            Assert.NotNull(response);
            Assert.True(response.Type == StreamingResponseType.SQLGeneration ||
                       response.Type == StreamingResponseType.Error);
        }

        // Should have at least one response
        Assert.NotEmpty(responses);
    }

    [Fact]
    public async Task GenerateSQLStreamAsync_WhenNotConfigured_ReturnsErrorResponse()
    {
        // Arrange - Create a service with null provider (simulating not configured)
        var mockProviderFactory = new Mock<IAIProviderFactory>();
        mockProviderFactory.Setup(x => x.GetProvider()).Returns((IAIProvider)null!);

        var unconfiguredService = new AIService(
            mockProviderFactory.Object,
            _mockLogger.Object,
            _mockCacheService.Object,
            _mockContextManager.Object,
            _mockMetricsCollector.Object);

        var prompt = "Show me all customers";

        // Act
        var responses = new List<StreamingResponse>();
        await foreach (var response in unconfiguredService.GenerateSQLStreamAsync(prompt))
        {
            responses.Add(response);
        }

        // Assert
        Assert.Single(responses);
        var errorResponse = responses[0];
        Assert.Contains("not configured", errorResponse.Content);
        Assert.True(errorResponse.IsComplete);
    }

    [Fact]
    public async Task GenerateInsightStreamAsync_WithValidData_ReturnsStreamingResponse()
    {
        // Arrange
        var query = "SELECT COUNT(*) as CustomerCount FROM Customers";
        var data = new object[] { new { CustomerCount = 100 } };
        var context = new AnalysisContext
        {
            BusinessGoal = "Understand customer base",
            Type = AnalysisType.Descriptive
        };

        // Act
        var responses = new List<StreamingResponse>();
        await foreach (var response in _service.GenerateInsightStreamAsync(query, data, context))
        {
            responses.Add(response);
            Assert.NotNull(response);
            Assert.True(response.Type == StreamingResponseType.Insight ||
                       response.Type == StreamingResponseType.Error);
        }

        // Should have at least one response
        Assert.NotEmpty(responses);
    }

    [Fact]
    public async Task GenerateExplanationStreamAsync_WithValidSQL_ReturnsStreamingResponse()
    {
        // Arrange
        var sql = "SELECT c.Name, COUNT(o.Id) as OrderCount FROM Customers c LEFT JOIN Orders o ON c.Id = o.CustomerId GROUP BY c.Name";
        var complexity = StreamingQueryComplexity.Complex;

        // Act
        var responses = new List<StreamingResponse>();
        await foreach (var response in _service.GenerateExplanationStreamAsync(sql, complexity))
        {
            responses.Add(response);
            Assert.NotNull(response);
            Assert.True(response.Type == StreamingResponseType.Explanation ||
                       response.Type == StreamingResponseType.Error);
        }

        // Should have at least one response
        Assert.NotEmpty(responses);
    }

    [Fact]
    public async Task GenerateSQLStreamAsync_WithNullPrompt_ReturnsErrorResponse()
    {
        // Arrange
        string? prompt = null;

        // Act
        var responses = new List<StreamingResponse>();
        await foreach (var response in _service.GenerateSQLStreamAsync(prompt!))
        {
            responses.Add(response);
        }

        // Assert
        Assert.NotEmpty(responses);
        var errorResponse = responses.FirstOrDefault(r => r.Type == StreamingResponseType.Error);
        Assert.NotNull(errorResponse);
    }

    [Fact]
    public void StreamingResponse_Properties_AreSetCorrectly()
    {
        // Arrange & Act
        var response = new StreamingResponse
        {
            Type = StreamingResponseType.SQLGeneration,
            Content = "SELECT * FROM Users",
            IsComplete = true,
            ChunkIndex = 1,
            Metadata = new { Source = "Test" }
        };

        // Assert
        Assert.Equal(StreamingResponseType.SQLGeneration, response.Type);
        Assert.Equal("SELECT * FROM Users", response.Content);
        Assert.True(response.IsComplete);
        Assert.Equal(1, response.ChunkIndex);
        Assert.NotNull(response.Metadata);
        Assert.True(response.Timestamp > DateTime.MinValue);
    }

    [Theory]
    [InlineData(StreamingQueryComplexity.Simple)]
    [InlineData(StreamingQueryComplexity.Medium)]
    [InlineData(StreamingQueryComplexity.Complex)]
    [InlineData(StreamingQueryComplexity.Expert)]
    public async Task GenerateExplanationStreamAsync_WithDifferentComplexities_HandlesCorrectly(StreamingQueryComplexity complexity)
    {
        // Arrange
        var sql = "SELECT * FROM Users WHERE Active = 1";

        // Act
        var responses = new List<StreamingResponse>();
        await foreach (var response in _service.GenerateExplanationStreamAsync(sql, complexity))
        {
            responses.Add(response);
        }

        // Assert
        Assert.NotEmpty(responses);
        // Should handle all complexity levels without throwing
    }

    [Theory]
    [InlineData(AnalysisType.Descriptive)]
    [InlineData(AnalysisType.Diagnostic)]
    [InlineData(AnalysisType.Predictive)]
    [InlineData(AnalysisType.Prescriptive)]
    public async Task GenerateInsightStreamAsync_WithDifferentAnalysisTypes_HandlesCorrectly(AnalysisType analysisType)
    {
        // Arrange
        var query = "SELECT AVG(Revenue) FROM Sales";
        var data = new object[] { new { Revenue = 1000.50 } };
        var context = new AnalysisContext { Type = analysisType };

        // Act
        var responses = new List<StreamingResponse>();
        await foreach (var response in _service.GenerateInsightStreamAsync(query, data, context))
        {
            responses.Add(response);
        }

        // Assert
        Assert.NotEmpty(responses);
        // Should handle all analysis types without throwing
    }
}
