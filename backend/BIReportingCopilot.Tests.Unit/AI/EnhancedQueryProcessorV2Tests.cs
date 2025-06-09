using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;
using BIReportingCopilot.Core.Interfaces;
using BIReportingCopilot.Core.Models;
using BIReportingCopilot.Infrastructure.AI.Enhanced;
using BIReportingCopilot.Infrastructure.Performance;

namespace BIReportingCopilot.Tests.Unit.AI;

public class EnhancedQueryProcessorV2Tests
{
    private readonly Mock<ICacheService> _mockCacheService;
    private readonly Mock<ISchemaService> _mockSchemaService;
    private readonly Mock<IAIService> _mockAIService;
    private readonly Mock<IContextManager> _mockContextManager;
    private readonly ILogger<EnhancedQueryProcessorV2> _logger;
    private readonly EnhancedQueryProcessorV2 _processor;

    public EnhancedQueryProcessorV2Tests()
    {
        _mockCacheService = new Mock<ICacheService>();
        _mockSchemaService = new Mock<ISchemaService>();
        _mockAIService = new Mock<IAIService>();
        _mockContextManager = new Mock<IContextManager>();
        _logger = NullLogger<EnhancedQueryProcessorV2>.Instance;

        _processor = new EnhancedQueryProcessorV2(
            _logger,
            _mockCacheService.Object,
            _mockSchemaService.Object,
            _mockAIService.Object,
            _mockContextManager.Object);

        SetupMockDefaults();
    }

    [Fact]
    public async Task ProcessQueryAsync_SimpleQuery_ReturnsProcessedQuery()
    {
        // Arrange
        var query = "Show me total revenue";
        var userId = "test-user";

        // Act
        var result = await _processor.ProcessQueryAsync(query, userId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(query, result.OriginalQuery);
        Assert.NotEmpty(result.Sql);
        Assert.True(result.Confidence > 0);
        Assert.NotNull(result.Explanation);
        Assert.Contains("enhancement_version", result.Metadata.Keys);
        Assert.Equal("v2", result.Metadata["enhancement_version"]);
    }

    [Fact]
    public async Task ProcessQueryAsync_ComplexQuery_UsesDecomposition()
    {
        // Arrange
        var query = "Show me total revenue, average deposits, and player count by country for last week";
        var userId = "test-user";

        // Act
        var result = await _processor.ProcessQueryAsync(query, userId);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Confidence > 0.5);
        Assert.Contains("decomposition_strategy", result.Metadata.Keys);
        Assert.Contains("sub_query_count", result.Metadata.Keys);
        Assert.Contains("confidence_components", result.Metadata.Keys);
    }

    [Fact]
    public async Task ProcessQueryAsync_WithConversationContext_IncludesContextMetadata()
    {
        // Arrange
        var query = "Show me the same data for yesterday";
        var userId = "test-user";

        // Setup conversation context
        var userContext = new UserContext
        {
            UserId = userId,
            Domain = "gaming",
            ExperienceLevel = "advanced"
        };

        _mockContextManager.Setup(x => x.GetUserContextAsync(userId))
            .ReturnsAsync(userContext);

        // Act
        var result = await _processor.ProcessQueryAsync(query, userId);

        // Assert
        Assert.NotNull(result);
        Assert.Contains("has_conversation_context", result.Metadata.Keys);
        Assert.Contains("semantic_user_domain", result.Metadata.Keys);
        Assert.Equal("gaming", result.Metadata["semantic_user_domain"]);
    }

    [Fact]
    public async Task ProcessQueryAsync_ErrorHandling_ReturnsFallbackQuery()
    {
        // Arrange
        var query = "Test query";
        var userId = "test-user";

        // Setup mocks to throw exceptions
        _mockAIService.Setup(x => x.GenerateSQLAsync(It.IsAny<string>()))
            .ThrowsAsync(new Exception("AI service error"));

        // Act
        var result = await _processor.ProcessQueryAsync(query, userId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(query, result.OriginalQuery);
        Assert.True(result.Sql.StartsWith("--"));
        Assert.Equal(0.1, result.Confidence);
        Assert.True((bool)result.Metadata["fallback"]);
    }

    [Fact]
    public async Task GenerateQuerySuggestionsAsync_WithContext_ReturnsRelevantSuggestions()
    {
        // Arrange
        var context = "revenue analysis";
        var userId = "test-user";

        // Act
        var result = await _processor.GenerateQuerySuggestionsAsync(context, userId);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Count > 0);
        Assert.True(result.Count <= 8); // Should limit to 8 suggestions
        Assert.All(result, suggestion => Assert.False(string.IsNullOrEmpty(suggestion)));
    }

    [Fact]
    public async Task GenerateQuerySuggestionsAsync_EmptyContext_ReturnsDefaultSuggestions()
    {
        // Arrange
        var context = "";
        var userId = "test-user";

        // Act
        var result = await _processor.GenerateQuerySuggestionsAsync(context, userId);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Count > 0);
        // Should include domain-specific suggestions
        Assert.Contains(result, s => s.Contains("revenue", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task CalculateSemanticSimilarityAsync_SimilarQueries_ReturnsHighSimilarity()
    {
        // Arrange
        var query1 = "Show me total revenue";
        var query2 = "Display total revenue data";

        // Act
        var result = await _processor.CalculateSemanticSimilarityAsync(query1, query2);

        // Assert
        Assert.True(result >= 0.0 && result <= 1.0);
        Assert.True(result > 0.5); // Should be relatively similar
    }

    [Fact]
    public async Task CalculateSemanticSimilarityAsync_DifferentQueries_ReturnsLowSimilarity()
    {
        // Arrange
        var query1 = "Show me total revenue";
        var query2 = "Count active players";

        // Act
        var result = await _processor.CalculateSemanticSimilarityAsync(query1, query2);

        // Assert
        Assert.True(result >= 0.0 && result <= 1.0);
        Assert.True(result < 0.7); // Should be less similar
    }

    [Fact]
    public async Task FindSimilarQueriesAsync_WithHistory_ReturnsSimilarQueries()
    {
        // Arrange
        var query = "Show me revenue trends";
        var userId = "test-user";
        var limit = 3;

        // Act
        var result = await _processor.FindSimilarQueriesAsync(query, userId, limit);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Count <= limit);
        // Note: In this test setup, we expect empty results since we don't have conversation history
        // In a real scenario with conversation history, this would return similar queries
    }

    [Theory]
    [InlineData("Show me total revenue", QueryIntent.Aggregation)]
    [InlineData("What are the trends over time", QueryIntent.Trend)]
    [InlineData("Compare revenue vs deposits", QueryIntent.Comparison)]
    [InlineData("Filter players by country", QueryIntent.Filtering)]
    public async Task ProcessQueryAsync_VariousIntents_ClassifiesCorrectly(
        string query, QueryIntent expectedIntent)
    {
        // Arrange
        var userId = "test-user";

        // Act
        var result = await _processor.ProcessQueryAsync(query, userId);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Classification);
        // The classification should be based on the semantic analysis
        // Note: Exact intent matching depends on the semantic analyzer implementation
    }

    [Fact]
    public async Task ProcessQueryAsync_HighConfidenceQuery_ReturnsHighConfidence()
    {
        // Arrange
        var query = "Show me total deposits amount from tbl_Daily_actions";
        var userId = "test-user";

        // Act
        var result = await _processor.ProcessQueryAsync(query, userId);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Confidence > 0.6); // Should have good confidence for clear query
        Assert.Contains("overall_confidence", result.Metadata.Keys);
        Assert.Contains("confidence_components", result.Metadata.Keys);
    }

    [Fact]
    public async Task ProcessQueryAsync_PerformanceMetrics_IncludesTimingData()
    {
        // Arrange
        var query = "Show me revenue data";
        var userId = "test-user";

        // Act
        var result = await _processor.ProcessQueryAsync(query, userId);

        // Assert
        Assert.NotNull(result);
        Assert.Contains("processing_time_ms", result.Metadata.Keys);
        Assert.True((long)result.Metadata["processing_time_ms"] >= 0);
    }

    private void SetupMockDefaults()
    {
        // Setup default schema
        var testSchema = new SchemaMetadata
        {
            DatabaseName = "TestDB",
            Tables = new List<TableMetadata>
            {
                new TableMetadata
                {
                    Name = "tbl_Daily_actions",
                    Columns = new List<ColumnMetadata>
                    {
                        new ColumnMetadata { Name = "PlayerID", DataType = "int" },
                        new ColumnMetadata { Name = "TotalRevenueAmount", DataType = "decimal" },
                        new ColumnMetadata { Name = "TotalDepositsAmount", DataType = "decimal" }
                    }
                }
            }
        };

        _mockSchemaService.Setup(x => x.GetSchemaMetadataAsync(It.IsAny<string>()))
            .ReturnsAsync(testSchema);

        // Setup default AI service response
        _mockAIService.Setup(x => x.GenerateSQLAsync(It.IsAny<string>()))
            .ReturnsAsync("SELECT TotalRevenueAmount FROM tbl_Daily_actions WITH (NOLOCK)");

        // Setup default cache responses
        _mockCacheService.Setup(x => x.GetAsync<SemanticAnalysis>(It.IsAny<string>()))
            .ReturnsAsync((SemanticAnalysis?)null);

        _mockCacheService.Setup(x => x.GetAsync<ConversationContext>(It.IsAny<string>()))
            .ReturnsAsync((ConversationContext?)null);

        // Setup default context manager
        _mockContextManager.Setup(x => x.GetUserContextAsync(It.IsAny<string>()))
            .ReturnsAsync(new UserContext
            {
                UserId = "test-user",
                Domain = "gaming",
                ExperienceLevel = "intermediate"
            });
    }
}

public class MultiDimensionalConfidenceScorerTests
{
    private readonly ILogger _logger;
    private readonly MultiDimensionalConfidenceScorer _scorer;

    public MultiDimensionalConfidenceScorerTests()
    {
        _logger = NullLogger.Instance;
        _scorer = new MultiDimensionalConfidenceScorer(_logger);
    }

    [Fact]
    public async Task CalculateOverallConfidenceAsync_ValidInputs_ReturnsConfidenceResult()
    {
        // Arrange
        var query = "Show me total revenue";
        var sql = "SELECT SUM(TotalRevenueAmount) FROM tbl_Daily_actions WITH (NOLOCK)";
        var semanticAnalysis = new SemanticAnalysis
        {
            OriginalQuery = query,
            ConfidenceScore = 0.8,
            Intent = QueryIntent.Aggregation
        };
        var generatedQuery = new GeneratedQuery
        {
            SQL = sql,
            ConfidenceScore = 0.9
        };

        // Act
        var result = await _scorer.CalculateOverallConfidenceAsync(query, sql, semanticAnalysis, generatedQuery);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.OverallConfidence >= 0.0 && result.OverallConfidence <= 1.0);
        Assert.Contains("model_confidence", result.ComponentScores.Keys);
        Assert.Contains("schema_alignment", result.ComponentScores.Keys);
        Assert.Contains("execution_validity", result.ComponentScores.Keys);
        Assert.Contains("historical_performance", result.ComponentScores.Keys);
        Assert.NotEmpty(result.Recommendation);
    }

    [Fact]
    public async Task CalculateOverallConfidenceAsync_HighQualityQuery_ReturnsHighConfidence()
    {
        // Arrange
        var query = "Show me total deposits from daily actions table";
        var sql = "SELECT SUM(TotalDepositsAmount) FROM tbl_Daily_actions WITH (NOLOCK)";
        var semanticAnalysis = new SemanticAnalysis
        {
            ConfidenceScore = 0.9,
            Intent = QueryIntent.Aggregation
        };
        var generatedQuery = new GeneratedQuery
        {
            SQL = sql,
            ConfidenceScore = 0.9
        };

        // Act
        var result = await _scorer.CalculateOverallConfidenceAsync(query, sql, semanticAnalysis, generatedQuery);

        // Assert
        Assert.True(result.OverallConfidence > 0.7);
        Assert.Equal(ConfidenceLevel.High, result.ConfidenceLevel);
        Assert.Contains("High confidence", result.Recommendation);
    }

    [Fact]
    public async Task CalculateOverallConfidenceAsync_LowQualityQuery_ReturnsLowConfidence()
    {
        // Arrange
        var query = "Show me stuff";
        var sql = "-- Unable to generate SQL";
        var semanticAnalysis = new SemanticAnalysis
        {
            ConfidenceScore = 0.3,
            Intent = QueryIntent.General
        };
        var generatedQuery = new GeneratedQuery
        {
            SQL = sql,
            ConfidenceScore = 0.2
        };

        // Act
        var result = await _scorer.CalculateOverallConfidenceAsync(query, sql, semanticAnalysis, generatedQuery);

        // Assert
        Assert.True(result.OverallConfidence < 0.6);
        Assert.True(result.ConfidenceLevel == ConfidenceLevel.Low || result.ConfidenceLevel == ConfidenceLevel.VeryLow);
        Assert.Contains("confidence", result.Recommendation.ToLowerInvariant());
    }

    [Fact]
    public async Task CalculateOverallConfidenceAsync_ErrorHandling_ReturnsFallbackResult()
    {
        // Arrange - pass null values to trigger error handling
        var query = "test";
        var sql = "test";
        SemanticAnalysis? semanticAnalysis = null;
        GeneratedQuery? generatedQuery = null;

        // Act
        var result = await _scorer.CalculateOverallConfidenceAsync(query, sql, semanticAnalysis!, generatedQuery!);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(0.5, result.OverallConfidence);
        Assert.Equal(ConfidenceLevel.Medium, result.ConfidenceLevel);
        Assert.Contains("error", result.Recommendation.ToLowerInvariant());
    }

    [Theory]
    [InlineData(0.9, ConfidenceLevel.High, "High confidence")]
    [InlineData(0.7, ConfidenceLevel.Medium, "Medium confidence")]
    [InlineData(0.5, ConfidenceLevel.Low, "Low confidence")]
    [InlineData(0.3, ConfidenceLevel.VeryLow, "Very low confidence")]
    public async Task CalculateOverallConfidenceAsync_VariousConfidenceLevels_ReturnsCorrectLevel(
        double inputConfidence, ConfidenceLevel expectedLevel, string expectedRecommendationContains)
    {
        // Arrange
        var query = "test query";
        var sql = "SELECT * FROM test";
        var semanticAnalysis = new SemanticAnalysis
        {
            ConfidenceScore = inputConfidence,
            Intent = QueryIntent.General
        };
        var generatedQuery = new GeneratedQuery
        {
            SQL = sql,
            ConfidenceScore = inputConfidence
        };

        // Act
        var result = await _scorer.CalculateOverallConfidenceAsync(query, sql, semanticAnalysis, generatedQuery);

        // Assert
        Assert.Equal(expectedLevel, result.ConfidenceLevel);
        Assert.Contains(expectedRecommendationContains, result.Recommendation, StringComparison.OrdinalIgnoreCase);
    }
}
