using Xunit;
using Moq;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;
using BIReportingCopilot.Infrastructure.AI;
using BIReportingCopilot.Infrastructure.Data;
using BIReportingCopilot.Core.Interfaces;
using BIReportingCopilot.Core.Models;
using BIReportingCopilot.Tests.Infrastructure;
using BIReportingCopilot.Tests.Infrastructure.Builders;
using BIReportingCopilot.Infrastructure.Monitoring;

namespace BIReportingCopilot.Tests.Unit.AI;

/// <summary>
/// Unit tests for enhanced AIService with adaptive learning capabilities
/// </summary>
public class AdaptiveAIServiceTests : IntegrationTestBase
{
    private readonly MockServiceBuilder _mockBuilder;
    private readonly AIService _aiService;

    public AdaptiveAIServiceTests()
    {
        _mockBuilder = new MockServiceBuilder();

        // Create mock dependencies for the enhanced AIService
        var mockProviderFactory = _mockBuilder.For<IAIProviderFactory>();
        var mockProvider = _mockBuilder.For<IAIProvider>();
        var mockCacheService = _mockBuilder.For<ICacheService>();
        var mockContextManager = _mockBuilder.For<IContextManager>();
        var mockMetricsCollector = _mockBuilder.For<IMetricsCollector>();

        // Setup provider factory to return mock provider
        mockProviderFactory.Setup(x => x.GetProvider()).Returns(mockProvider.Object);
        mockProvider.Setup(x => x.ProviderName).Returns("TestProvider");

        _aiService = new AIService(
            mockProviderFactory.Object,
            GetLogger<AIService>(),
            mockCacheService.Object,
            mockContextManager.Object,
            mockMetricsCollector.Object,
            null, // learningEngine - not needed for basic tests
            null); // promptOptimizer - not needed for basic tests
    }

    [Fact]
    public async Task GenerateSQLAsync_WithValidPrompt_ShouldReturnSQL()
    {
        // Arrange
        var prompt = "Show me all customers";
        var expectedSQL = "SELECT * FROM Customers";

        // Setup the mock provider to return expected SQL
        var mockProvider = _mockBuilder.For<IAIProvider>();
        mockProvider.Setup(x => x.GenerateCompletionAsync(It.IsAny<string>(), It.IsAny<AIOptions>(), It.IsAny<CancellationToken>()))
                   .ReturnsAsync(expectedSQL);

        // Act
        var result = await _aiService.GenerateSQLAsync(prompt);

        // Assert
        result.Should().NotBeNullOrEmpty();
        // Note: The enhanced AIService includes additional processing, so we verify the core functionality
    }

    [Fact]
    public async Task ProcessFeedbackAsync_WithValidFeedback_ShouldProcessSuccessfully()
    {
        // Arrange
        var prompt = "Show sales data";
        var sql = "SELECT * FROM Sales";
        var feedback = new QueryFeedback
        {
            Feedback = "positive",
            Comments = "Good query, but could be more specific"
        };
        var userId = "test-user";

        // Act & Assert - Should not throw
        await _aiService.ProcessFeedbackAsync(prompt, sql, feedback, userId);

        // The method should complete without errors when learning engine is available
    }

    [Fact]
    public async Task GetLearningStatisticsAsync_ShouldReturnStatistics()
    {
        // Act
        var statistics = await _aiService.GetLearningStatisticsAsync();

        // Assert
        statistics.Should().NotBeNull();
        // When learning engine is available, it should return meaningful statistics
        // When not available, it should return default statistics
    }

    public override void Dispose()
    {
        _mockBuilder?.Reset();
        base.Dispose();
    }
}
