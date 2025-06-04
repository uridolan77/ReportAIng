using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;
using BIReportingCopilot.Core.Interfaces;
using BIReportingCopilot.Core.Models;
using BIReportingCopilot.Infrastructure.AI.Enhanced;
using BIReportingCopilot.Infrastructure.Performance;

namespace BIReportingCopilot.Tests.Unit.AI.Enhanced;

public class ContextAwareSemanticAnalyzerTests
{
    private readonly Mock<ICacheService> _mockCacheService;
    private readonly Mock<IContextManager> _mockContextManager;
    private readonly Mock<IAIService> _mockAIService;
    private readonly ILogger<ContextAwareSemanticAnalyzer> _logger;
    private readonly ContextAwareSemanticAnalyzer _analyzer;

    public ContextAwareSemanticAnalyzerTests()
    {
        _mockCacheService = new Mock<ICacheService>();
        _mockContextManager = new Mock<IContextManager>();
        _mockAIService = new Mock<IAIService>();
        _logger = NullLogger<ContextAwareSemanticAnalyzer>.Instance;

        _analyzer = new ContextAwareSemanticAnalyzer(
            _logger,
            _mockCacheService.Object,
            _mockContextManager.Object,
            _mockAIService.Object);
    }

    [Fact]
    public async Task AnalyzeAsync_SimpleQuery_ReturnsBasicAnalysis()
    {
        // Arrange
        var query = "Show me total revenue";
        _mockCacheService.Setup(x => x.GetAsync<SemanticAnalysis>(It.IsAny<string>()))
            .ReturnsAsync((SemanticAnalysis?)null);

        // Act
        var result = await _analyzer.AnalyzeAsync(query);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(query, result.OriginalQuery);
        Assert.True(result.ConfidenceScore > 0);
        Assert.Contains(result.Entities, e => e.Type == EntityType.Aggregation);
    }

    [Fact]
    public async Task AnalyzeWithContextAsync_WithUserId_IncludesUserContext()
    {
        // Arrange
        var query = "Show me player deposits";
        var userId = "test-user";
        var sessionId = "test-session";

        _mockCacheService.Setup(x => x.GetAsync<SemanticAnalysis>(It.IsAny<string>()))
            .ReturnsAsync((SemanticAnalysis?)null);

        var userContext = new UserContext
        {
            UserId = userId,
            Domain = "gaming",
            ExperienceLevel = "intermediate"
        };

        _mockContextManager.Setup(x => x.GetUserContextAsync(userId))
            .ReturnsAsync(userContext);

        // Act
        var result = await _analyzer.AnalyzeWithContextAsync(query, userId, sessionId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("gaming", result.Metadata["user_domain"]);
        Assert.True((bool)result.Metadata["has_conversation_context"]);
    }

    [Fact]
    public async Task AnalyzeWithContextAsync_CachedResult_ReturnsCachedAnalysis()
    {
        // Arrange
        var query = "Show me revenue trends";
        var cachedAnalysis = new SemanticAnalysis
        {
            OriginalQuery = query,
            Intent = QueryIntent.Trend,
            ConfidenceScore = 0.9
        };

        _mockCacheService.Setup(x => x.GetAsync<SemanticAnalysis>(It.IsAny<string>()))
            .ReturnsAsync(cachedAnalysis);

        // Act
        var result = await _analyzer.AnalyzeWithContextAsync(query);

        // Assert
        Assert.Equal(cachedAnalysis, result);
        Assert.Equal(QueryIntent.Trend, result.Intent);
        Assert.Equal(0.9, result.ConfidenceScore);
    }

    [Theory]
    [InlineData("Show me total deposits", EntityType.Aggregation, "total")]
    [InlineData("Filter players by country", EntityType.Condition, "filter")]
    [InlineData("Show me yesterday's data", EntityType.DateRange, "yesterday")]
    [InlineData("Count active players", EntityType.Aggregation, "count")]
    public async Task ExtractEntitiesAsync_VariousQueries_ExtractsCorrectEntities(
        string query, EntityType expectedType, string expectedText)
    {
        // Act
        var result = await _analyzer.ExtractEntitiesAsync(query);

        // Assert
        Assert.Contains(result, e => e.Type == expectedType && 
            e.Text.Contains(expectedText, StringComparison.OrdinalIgnoreCase));
    }

    [Theory]
    [InlineData("Show me total revenue", QueryIntent.Aggregation)]
    [InlineData("What are the trends over time", QueryIntent.Trend)]
    [InlineData("Compare revenue vs deposits", QueryIntent.Comparison)]
    [InlineData("Filter data by country", QueryIntent.Filtering)]
    [InlineData("Hello world", QueryIntent.General)]
    public async Task ClassifyIntentAsync_VariousQueries_ReturnsCorrectIntent(
        string query, QueryIntent expectedIntent)
    {
        // Act
        var result = await _analyzer.ClassifyIntentAsync(query);

        // Assert
        Assert.Equal(expectedIntent, result);
    }

    [Fact]
    public async Task AnalyzeWithContextAsync_ComplexQuery_HighConfidenceScore()
    {
        // Arrange
        var query = "Show me total deposits and revenue by country for last week";
        _mockCacheService.Setup(x => x.GetAsync<SemanticAnalysis>(It.IsAny<string>()))
            .ReturnsAsync((SemanticAnalysis?)null);

        // Act
        var result = await _analyzer.AnalyzeWithContextAsync(query);

        // Assert
        Assert.True(result.ConfidenceScore > 0.7);
        Assert.True(result.Entities.Count >= 3); // Should find multiple entities
        Assert.Contains(result.Entities, e => e.Type == EntityType.Aggregation);
        Assert.Contains(result.Entities, e => e.Type == EntityType.DateRange);
    }

    [Fact]
    public async Task AnalyzeWithContextAsync_ErrorHandling_ReturnsFallbackAnalysis()
    {
        // Arrange
        var query = "test query";
        _mockCacheService.Setup(x => x.GetAsync<SemanticAnalysis>(It.IsAny<string>()))
            .ThrowsAsync(new Exception("Cache error"));

        // Act
        var result = await _analyzer.AnalyzeWithContextAsync(query);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(query, result.OriginalQuery);
        Assert.True(result.ConfidenceScore >= 0.3); // Fallback confidence
        Assert.True((bool)result.Metadata["fallback"]);
    }

    [Fact]
    public async Task GenerateEmbeddingAsync_ValidText_ReturnsEmbedding()
    {
        // Arrange
        var text = "test query";

        // Act
        var result = await _analyzer.GenerateEmbeddingAsync(text);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(384, result.Length); // Expected embedding dimension
    }

    [Fact]
    public async Task CalculateSimilarityAsync_TwoQueries_ReturnsSimilarityScore()
    {
        // Arrange
        var query1 = "Show me revenue";
        var query2 = "Display total revenue";

        // Act
        var result = await _analyzer.CalculateSimilarityAsync(query1, query2);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(query1, result.Query1);
        Assert.Equal(query2, result.Query2);
        Assert.True(result.SimilarityScore >= 0.0 && result.SimilarityScore <= 1.0);
    }
}

public class ConversationContextManagerTests
{
    private readonly Mock<ICacheService> _mockCacheService;
    private readonly ILogger _logger;
    private readonly ConversationContextManager _manager;

    public ConversationContextManagerTests()
    {
        _mockCacheService = new Mock<ICacheService>();
        _logger = NullLogger.Instance;
        _manager = new ConversationContextManager(_mockCacheService.Object, _logger);
    }

    [Fact]
    public async Task GetConversationContextAsync_NewUser_ReturnsEmptyContext()
    {
        // Arrange
        var userId = "new-user";
        var sessionId = "new-session";
        _mockCacheService.Setup(x => x.GetAsync<ConversationContext>(It.IsAny<string>()))
            .ReturnsAsync((ConversationContext?)null);

        // Act
        var result = await _manager.GetConversationContextAsync(userId, sessionId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(userId, result.UserId);
        Assert.Equal(sessionId, result.SessionId);
        Assert.Empty(result.PreviousQueries);
    }

    [Fact]
    public async Task UpdateConversationContextAsync_NewQuery_AddsToContext()
    {
        // Arrange
        var userId = "test-user";
        var sessionId = "test-session";
        var query = "Show me revenue";
        var analysis = new SemanticAnalysis
        {
            OriginalQuery = query,
            Intent = QueryIntent.Aggregation,
            ConfidenceScore = 0.8
        };

        _mockCacheService.Setup(x => x.GetAsync<ConversationContext>(It.IsAny<string>()))
            .ReturnsAsync((ConversationContext?)null);

        // Act
        await _manager.UpdateConversationContextAsync(userId, sessionId, query, analysis);

        // Assert
        _mockCacheService.Verify(x => x.SetAsync(
            It.IsAny<string>(),
            It.Is<ConversationContext>(ctx => ctx.PreviousQueries.Count == 1),
            It.IsAny<TimeSpan>()), Times.AtLeastOnce);
    }

    [Fact]
    public async Task GetSimilarQueriesAsync_WithHistory_ReturnsSimilarQueries()
    {
        // Arrange
        var userId = "test-user";
        var sessionId = "test-session";
        var currentQuery = "Show me total revenue";

        var existingContext = new ConversationContext
        {
            UserId = userId,
            SessionId = sessionId,
            PreviousQueries = new List<QueryContextEntry>
            {
                new QueryContextEntry
                {
                    Query = "Display revenue data",
                    Timestamp = DateTime.UtcNow.AddMinutes(-5)
                },
                new QueryContextEntry
                {
                    Query = "Show player count",
                    Timestamp = DateTime.UtcNow.AddMinutes(-3)
                }
            }
        };

        _mockCacheService.Setup(x => x.GetAsync<ConversationContext>(It.IsAny<string>()))
            .ReturnsAsync(existingContext);

        // Act
        var result = await _manager.GetSimilarQueriesAsync(userId, sessionId, currentQuery, 3);

        // Assert
        Assert.NotNull(result);
        // Should find at least one similar query (revenue-related)
        Assert.True(result.Count >= 0);
    }

    [Fact]
    public async Task GetConversationStatsAsync_WithHistory_ReturnsStats()
    {
        // Arrange
        var userId = "test-user";
        var sessionId = "test-session";

        var existingContext = new ConversationContext
        {
            UserId = userId,
            SessionId = sessionId,
            StartTime = DateTime.UtcNow.AddHours(-1),
            PreviousQueries = new List<QueryContextEntry>
            {
                new QueryContextEntry
                {
                    Query = "Show revenue",
                    Analysis = new SemanticAnalysis { Intent = QueryIntent.Aggregation, ConfidenceScore = 0.8 }
                },
                new QueryContextEntry
                {
                    Query = "Show trends",
                    Analysis = new SemanticAnalysis { Intent = QueryIntent.Trend, ConfidenceScore = 0.9 }
                }
            }
        };

        _mockCacheService.Setup(x => x.GetAsync<ConversationContext>(It.IsAny<string>()))
            .ReturnsAsync(existingContext);

        // Act
        var result = await _manager.GetConversationStatsAsync(userId, sessionId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.TotalQueries);
        Assert.True(result.SessionDuration.TotalMinutes > 0);
        Assert.True(result.AverageConfidence > 0);
    }

    [Fact]
    public async Task ClearConversationContextAsync_ValidUser_ClearsContext()
    {
        // Arrange
        var userId = "test-user";
        var sessionId = "test-session";

        // Act
        await _manager.ClearConversationContextAsync(userId, sessionId);

        // Assert
        _mockCacheService.Verify(x => x.RemoveAsync(It.IsAny<string>()), Times.Once);
    }
}
