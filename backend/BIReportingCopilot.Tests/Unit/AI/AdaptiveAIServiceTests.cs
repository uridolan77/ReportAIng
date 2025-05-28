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

namespace BIReportingCopilot.Tests.Unit.AI;

/// <summary>
/// Unit tests for AdaptiveAIService
/// </summary>
public class AdaptiveAIServiceTests : IntegrationTestBase
{
    private readonly Mock<IAIService> _mockInnerService;
    private readonly AdaptiveAIService _adaptiveService;
    private readonly MockServiceBuilder _mockBuilder;

    public AdaptiveAIServiceTests()
    {
        _mockBuilder = new MockServiceBuilder();
        _mockInnerService = _mockBuilder.For<IAIService>();

        _adaptiveService = new AdaptiveAIService(
            _mockInnerService.Object,
            DbContext,
            GetLogger<AdaptiveAIService>(),
            MemoryCache);
    }

    [Fact]
    public async Task GenerateSQLAsync_WithValidPrompt_ShouldReturnSQL()
    {
        // Arrange
        var prompt = "Show me all customers";
        var expectedSQL = "SELECT * FROM Customers";

        _mockInnerService
            .Setup(x => x.GenerateSQLAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedSQL);

        // Act
        var result = await _adaptiveService.GenerateSQLAsync(prompt);

        // Assert
        result.Should().Be(expectedSQL);
        _mockInnerService.Verify(x => x.GenerateSQLAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GenerateSQLAsync_WithLearningData_ShouldOptimizePrompt()
    {
        // Arrange
        var prompt = "Show customers";
        var userId = "test-user";

        // Seed learning data
        await SeedDatabaseAsync(context =>
        {
            var feedback = TestDataBuilders.AIFeedback()
                .With(f => f.OriginalQuery, "Show customers")
                .With(f => f.Rating, 5)
                .With(f => f.FeedbackType, "Positive")
                .With(f => f.Comments, "display_query")
                .Build();

            context.AIFeedbackEntries.Add(feedback);
        });

        _mockInnerService
            .Setup(x => x.GenerateSQLAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("SELECT * FROM Customers");

        // Act
        var result = await _adaptiveService.GenerateSQLAsync(prompt);

        // Assert
        result.Should().NotBeNullOrEmpty();

        // Verify that the prompt was optimized (inner service called with enhanced prompt)
        _mockInnerService.Verify(x => x.GenerateSQLAsync(
            It.Is<string>(p => p.Length > prompt.Length), // Optimized prompt should be longer
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CalculateConfidenceScoreAsync_WithLearningInsights_ShouldEnhanceConfidence()
    {
        // Arrange
        var prompt = "Count customers";
        var sql = "SELECT COUNT(*) FROM Customers";
        var baseConfidence = 0.7;

        // Seed successful pattern data
        await SeedDatabaseAsync(context =>
        {
            var feedback = TestDataBuilders.AIFeedback()
                .With(f => f.OriginalQuery, "Count all customers")
                .With(f => f.GeneratedSql, "SELECT COUNT(*) FROM Customers")
                .With(f => f.Rating, 5)
                .With(f => f.FeedbackType, "Positive")
                .With(f => f.Comments, "count_query")
                .Build();

            context.AIFeedbackEntries.Add(feedback);
        });

        _mockInnerService
            .Setup(x => x.CalculateConfidenceScoreAsync(prompt, sql))
            .ReturnsAsync(baseConfidence);

        // Act
        var result = await _adaptiveService.CalculateConfidenceScoreAsync(prompt, sql);

        // Assert
        result.Should().BeGreaterThan(baseConfidence); // Should be enhanced due to successful patterns
    }

    [Fact]
    public async Task ProcessFeedbackAsync_WithValidFeedback_ShouldStoreFeedback()
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

        // Act
        await _adaptiveService.ProcessFeedbackAsync(prompt, sql, feedback, userId);

        // Assert
        var storedFeedback = await DbContext.AIFeedbackEntries
            .Where(f => f.UserId == userId && f.OriginalQuery == prompt)
            .FirstOrDefaultAsync();

        storedFeedback.Should().NotBeNull();
        storedFeedback!.FeedbackType.Should().Be("positive");
        storedFeedback.Comments.Should().Be(feedback.Comments);
        storedFeedback.IsProcessed.Should().BeFalse();
    }

    [Fact]
    public async Task GenerateQuerySuggestionsAsync_WithUserPatterns_ShouldIncludePersonalizedSuggestions()
    {
        // Arrange
        var context = "sales analysis";
        var schema = TestDataBuilders.SchemaMetadata().Build();
        var baseSuggestions = new[] { "Show total sales", "Count orders" };

        // Seed user pattern data
        await SeedDatabaseAsync(dbContext =>
        {
            var feedback = TestDataBuilders.AIFeedback()
                .With(f => f.OriginalQuery, "Show monthly sales trends")
                .With(f => f.Rating, 5)
                .With(f => f.FeedbackType, "Positive")
                .With(f => f.Comments, "display_query")
                .With(f => f.CreatedAt, DateTime.UtcNow.AddDays(-5))
                .Build();

            dbContext.AIFeedbackEntries.Add(feedback);
        });

        _mockInnerService
            .Setup(x => x.GenerateQuerySuggestionsAsync(context, schema))
            .ReturnsAsync(baseSuggestions);

        // Act
        var result = await _adaptiveService.GenerateQuerySuggestionsAsync(context, schema);

        // Assert
        result.Should().NotBeNull();
        result.Length.Should().BeGreaterThanOrEqualTo(baseSuggestions.Length);
    }

    [Fact]
    public async Task GetLearningStatisticsAsync_ShouldReturnAccurateStatistics()
    {
        // Arrange
        await SeedDatabaseAsync(context =>
        {
            // Add generation attempts
            var attempts = TestDataBuilders.AIGenerationAttempt()
                .BuildMany(10, (builder, index) =>
                    builder.With(a => a.UserId, $"user-{index % 3}"));

            foreach (var attempt in attempts)
            {
                context.AIGenerationAttempts.Add(attempt);
            }

            // Add feedback entries
            var feedbackEntries = TestDataBuilders.AIFeedback()
                .BuildMany(15, (builder, index) =>
                    builder.With(f => f.Rating, index % 5 + 1)
                           .With(f => f.UserId, $"user-{index % 3}"));

            foreach (var feedback in feedbackEntries)
            {
                context.AIFeedbackEntries.Add(feedback);
            }
        });

        // Act
        var statistics = await _adaptiveService.GetLearningStatisticsAsync();

        // Assert
        statistics.Should().NotBeNull();
        statistics.TotalGenerations.Should().Be(10);
        statistics.TotalFeedbackItems.Should().Be(15);
        statistics.UniqueUsers.Should().Be(3);
        statistics.AverageRating.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task GenerateSQLAsync_WhenInnerServiceFails_ShouldHandleGracefully()
    {
        // Arrange
        var prompt = "Show customers";
        var logCapture = CaptureLogsFor<AdaptiveAIService>();

        _mockInnerService
            .Setup(x => x.GenerateSQLAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("AI service unavailable"));

        // Act & Assert
        var exception = await AssertThrowsAsync<InvalidOperationException>(
            () => _adaptiveService.GenerateSQLAsync(prompt));

        exception.Message.Should().Contain("AI service unavailable");
        logCapture.HasErrors.Should().BeTrue();
        logCapture.AssertLogEntry(LogLevel.Error, "Error in adaptive SQL generation");
    }

    [Fact]
    public async Task GenerateSQLAsync_WithCancellationToken_ShouldRespectCancellation()
    {
        // Arrange
        var prompt = "Show customers";
        var cts = new CancellationTokenSource();
        cts.Cancel();

        _mockInnerService
            .Setup(x => x.GenerateSQLAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns((string p, CancellationToken ct) =>
            {
                ct.ThrowIfCancellationRequested();
                return Task.FromResult("SELECT * FROM Customers");
            });

        // Act & Assert
        await AssertThrowsAsync<OperationCanceledException>(
            () => _adaptiveService.GenerateSQLAsync(prompt, cts.Token));
    }

    [Fact]
    public async Task GenerateInsightAsync_WithLearningContext_ShouldEnhanceInsight()
    {
        // Arrange
        var query = "SELECT COUNT(*) FROM Orders";
        var data = new object[] { new { Count = 1000 } };
        var baseInsight = "There are 1000 orders in the system.";

        // Seed insight context data
        await SeedDatabaseAsync(context =>
        {
            var feedback = TestDataBuilders.AIFeedback()
                .With(f => f.GeneratedSql, query)
                .With(f => f.Comments, "Consider seasonal trends when analyzing order counts")
                .With(f => f.FeedbackType, "Positive")
                .With(f => f.Category, "simple_query")
                .Build();

            context.AIFeedbackEntries.Add(feedback);
        });

        _mockInnerService
            .Setup(x => x.GenerateInsightAsync(query, data))
            .ReturnsAsync(baseInsight);

        // Act
        var result = await _adaptiveService.GenerateInsightAsync(query, data);

        // Assert
        result.Should().NotBeNullOrEmpty();
        result.Length.Should().BeGreaterThan(baseInsight.Length); // Should be enhanced
        result.Should().Contain(baseInsight); // Should include original insight
    }

    public override void Dispose()
    {
        _mockBuilder?.Reset();
        base.Dispose();
    }
}

/// <summary>
/// Extension methods for test data builders specific to AI testing
/// </summary>
public static class AITestDataExtensions
{
    public static TestDataBuilder<AIGenerationAttempt> AIGenerationAttempt()
    {
        return new TestDataBuilder<AIGenerationAttempt>()
            .With(a => a.Id, Random.Shared.Next(1, int.MaxValue))
            .With(a => a.UserQuery, "Show me data")
            .With(a => a.PromptTemplate, "Context: Focus on display queries. Query: Show me data")
            .With(a => a.GeneratedSql, "SELECT * FROM TestTable")
            .With(a => a.AttemptedAt, DateTime.UtcNow)
            .With(a => a.IsSuccessful, true)
            .With(a => a.ConfidenceScore, 0.8)
            .With(a => a.UserId, "test-user");
    }
}
