using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;
using BIReportingCopilot.Core.Interfaces;
using BIReportingCopilot.Core.Models;
using BIReportingCopilot.Infrastructure.AI.Enhanced;

namespace BIReportingCopilot.Tests.Unit.AI;

public class QueryDecomposerTests
{
    private readonly Mock<IAIService> _mockAIService;
    private readonly Mock<ISchemaService> _mockSchemaService;
    private readonly ILogger<QueryDecomposer> _logger;
    private readonly QueryDecomposer _decomposer;
    private readonly SchemaMetadata _testSchema;

    public QueryDecomposerTests()
    {
        _mockAIService = new Mock<IAIService>();
        _mockSchemaService = new Mock<ISchemaService>();
        _logger = NullLogger<QueryDecomposer>.Instance;
        _decomposer = new QueryDecomposer(_logger, _mockAIService.Object, _mockSchemaService.Object);

        _testSchema = CreateTestSchema();
    }

    [Fact]
    public async Task DecomposeQueryAsync_SimpleQuery_ReturnsSimpleDecomposition()
    {
        // Arrange
        var query = "Show me total revenue";
        var semanticAnalysis = new SemanticAnalysis
        {
            OriginalQuery = query,
            Intent = QueryIntent.Aggregation,
            Entities = new List<SemanticEntity>
            {
                new SemanticEntity { Text = "total", Type = EntityType.Aggregation, Confidence = 0.9 },
                new SemanticEntity { Text = "revenue", Type = EntityType.Column, Confidence = 0.8 }
            }
        };

        // Act
        var result = await _decomposer.DecomposeQueryAsync(query, semanticAnalysis, _testSchema);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(query, result.OriginalQuery);
        Assert.Equal(DecompositionStrategy.Simple, result.Strategy);
        Assert.Single(result.SubQueries);
        Assert.Equal(SubQueryType.Simple, result.SubQueries[0].Type);
    }

    [Fact]
    public async Task DecomposeQueryAsync_ComplexTemporalQuery_ReturnsTemporalDecomposition()
    {
        // Arrange
        var query = "Show me revenue trends for last week and this month";
        var semanticAnalysis = new SemanticAnalysis
        {
            OriginalQuery = query,
            Intent = QueryIntent.Trend,
            Entities = new List<SemanticEntity>
            {
                new SemanticEntity { Text = "revenue", Type = EntityType.Column, Confidence = 0.9 },
                new SemanticEntity { Text = "last week", Type = EntityType.DateRange, Confidence = 0.8 },
                new SemanticEntity { Text = "this month", Type = EntityType.DateRange, Confidence = 0.8 }
            }
        };

        // Act
        var result = await _decomposer.DecomposeQueryAsync(query, semanticAnalysis, _testSchema);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(DecompositionStrategy.Temporal, result.Strategy);
        Assert.True(result.SubQueries.Count >= 2); // Should have sub-queries for each time period
        Assert.All(result.SubQueries, sq => Assert.Equal(SubQueryType.Temporal, sq.Type));
    }

    [Fact]
    public async Task DecomposeQueryAsync_MultipleAggregations_ReturnsAggregationDecomposition()
    {
        // Arrange
        var query = "Show me total revenue, average deposits, and count of players";
        var semanticAnalysis = new SemanticAnalysis
        {
            OriginalQuery = query,
            Intent = QueryIntent.Aggregation,
            Entities = new List<SemanticEntity>
            {
                new SemanticEntity { Text = "total", Type = EntityType.Aggregation, Confidence = 0.9 },
                new SemanticEntity { Text = "average", Type = EntityType.Aggregation, Confidence = 0.9 },
                new SemanticEntity { Text = "count", Type = EntityType.Aggregation, Confidence = 0.9 },
                new SemanticEntity { Text = "revenue", Type = EntityType.Column, Confidence = 0.8 },
                new SemanticEntity { Text = "deposits", Type = EntityType.Column, Confidence = 0.8 },
                new SemanticEntity { Text = "players", Type = EntityType.Table, Confidence = 0.8 }
            }
        };

        // Act
        var result = await _decomposer.DecomposeQueryAsync(query, semanticAnalysis, _testSchema);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(DecompositionStrategy.Aggregation, result.Strategy);
        Assert.Equal(3, result.SubQueries.Count); // One for each aggregation
        Assert.All(result.SubQueries, sq => Assert.Equal(SubQueryType.Aggregation, sq.Type));
    }

    [Fact]
    public async Task DecomposeQueryAsync_MultipleTablesRequired_ReturnsEntityBasedDecomposition()
    {
        // Arrange
        var query = "Show me player data with country and currency information";
        var semanticAnalysis = new SemanticAnalysis
        {
            OriginalQuery = query,
            Intent = QueryIntent.General,
            Entities = new List<SemanticEntity>
            {
                new SemanticEntity { Text = "player", Type = EntityType.Table, Confidence = 0.9 },
                new SemanticEntity { Text = "country", Type = EntityType.Table, Confidence = 0.8 },
                new SemanticEntity { Text = "currency", Type = EntityType.Table, Confidence = 0.8 }
            }
        };

        // Act
        var result = await _decomposer.DecomposeQueryAsync(query, semanticAnalysis, _testSchema);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(DecompositionStrategy.EntityBased, result.Strategy);
        Assert.True(result.SubQueries.Count >= 2); // Should have sub-queries for different entities
    }

    [Fact]
    public async Task DecomposeQueryAsync_FilterFirstScenario_ReturnsFilterFirstDecomposition()
    {
        // Arrange
        var query = "Show me total revenue for active players in USA with deposits > 100";
        var semanticAnalysis = new SemanticAnalysis
        {
            OriginalQuery = query,
            Intent = QueryIntent.Aggregation,
            Entities = new List<SemanticEntity>
            {
                new SemanticEntity { Text = "total", Type = EntityType.Aggregation, Confidence = 0.9 },
                new SemanticEntity { Text = "active", Type = EntityType.Condition, Confidence = 0.8 },
                new SemanticEntity { Text = "USA", Type = EntityType.Condition, Confidence = 0.8 },
                new SemanticEntity { Text = "> 100", Type = EntityType.Condition, Confidence = 0.7 },
                new SemanticEntity { Text = "deposits", Type = EntityType.Condition, Confidence = 0.7 }
            }
        };

        // Act
        var result = await _decomposer.DecomposeQueryAsync(query, semanticAnalysis, _testSchema);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(DecompositionStrategy.FilterFirst, result.Strategy);
        Assert.True(result.SubQueries.Count >= 2); // Filter + Aggregation
        Assert.Contains(result.SubQueries, sq => sq.Type == SubQueryType.Filter);
        Assert.Contains(result.SubQueries, sq => sq.Type == SubQueryType.Aggregation);
    }

    [Theory]
    [InlineData(5, false)] // Low complexity - no decomposition
    [InlineData(8, true)]  // High complexity - decomposition needed
    [InlineData(10, true)] // Very high complexity - decomposition needed
    public async Task DecomposeQueryAsync_VariousComplexityLevels_ReturnsAppropriateStrategy(
        int complexityScore, bool shouldDecompose)
    {
        // Arrange
        var query = "Test query";
        var semanticAnalysis = new SemanticAnalysis
        {
            OriginalQuery = query,
            Intent = QueryIntent.General,
            Entities = new List<SemanticEntity>()
        };

        // Mock complexity analysis to return specific score
        var complexityAnalysis = new QueryComplexityAnalysis
        {
            ComplexityScore = complexityScore,
            AggregationCount = complexityScore / 3,
            FilterConditions = complexityScore / 4,
            RequiredJoins = complexityScore / 5
        };

        // Act
        var result = await _decomposer.DecomposeQueryAsync(query, semanticAnalysis, _testSchema);

        // Assert
        Assert.NotNull(result);
        if (shouldDecompose)
        {
            Assert.NotEqual(DecompositionStrategy.Simple, result.Strategy);
        }
        else
        {
            Assert.Equal(DecompositionStrategy.Simple, result.Strategy);
            Assert.Single(result.SubQueries);
        }
    }

    [Fact]
    public async Task DecomposeQueryAsync_ErrorHandling_ReturnsFallbackDecomposition()
    {
        // Arrange
        var query = "Test query";
        var semanticAnalysis = new SemanticAnalysis
        {
            OriginalQuery = query,
            Intent = QueryIntent.General,
            Entities = new List<SemanticEntity>()
        };

        // Mock schema service to throw exception
        _mockSchemaService.Setup(x => x.GetSchemaMetadataAsync(It.IsAny<string>()))
            .ThrowsAsync(new Exception("Schema error"));

        // Act
        var result = await _decomposer.DecomposeQueryAsync(query, semanticAnalysis, _testSchema);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(query, result.OriginalQuery);
        Assert.Single(result.SubQueries);
        Assert.Equal(SubQueryType.Simple, result.SubQueries[0].Type);
    }

    [Fact]
    public async Task DecomposeQueryAsync_HierarchicalStrategy_ReturnsLayeredDecomposition()
    {
        // Arrange
        var query = "Complex hierarchical query with multiple layers";
        var semanticAnalysis = new SemanticAnalysis
        {
            OriginalQuery = query,
            Intent = QueryIntent.General,
            Entities = new List<SemanticEntity>
            {
                new SemanticEntity { Text = "data", Type = EntityType.Table, Confidence = 0.8 },
                new SemanticEntity { Text = "process", Type = EntityType.Aggregation, Confidence = 0.7 },
                new SemanticEntity { Text = "format", Type = EntityType.Sort, Confidence = 0.6 }
            }
        };

        // Force hierarchical strategy by creating high complexity
        var highComplexityAnalysis = new QueryComplexityAnalysis
        {
            ComplexityScore = 12, // High complexity
            AggregationCount = 1,
            FilterConditions = 2,
            RequiredJoins = 3,
            HasComplexTemporal = true
        };

        // Act
        var result = await _decomposer.DecomposeQueryAsync(query, semanticAnalysis, _testSchema);

        // Assert
        Assert.NotNull(result);
        if (result.Strategy == DecompositionStrategy.Hierarchical)
        {
            Assert.True(result.SubQueries.Count >= 2);
            // Check for proper dependency ordering
            var priorities = result.SubQueries.Select(sq => sq.Priority).ToList();
            Assert.Equal(priorities.OrderBy(p => p).ToList(), priorities);
        }
    }

    private SchemaMetadata CreateTestSchema()
    {
        return new SchemaMetadata
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
                        new ColumnMetadata { Name = "TotalDepositsAmount", DataType = "decimal" },
                        new ColumnMetadata { Name = "CountryID", DataType = "int" },
                        new ColumnMetadata { Name = "CurrencyID", DataType = "int" }
                    }
                },
                new TableMetadata
                {
                    Name = "tbl_Daily_actions_players",
                    Columns = new List<ColumnMetadata>
                    {
                        new ColumnMetadata { Name = "PlayerID", DataType = "int" },
                        new ColumnMetadata { Name = "PlayerName", DataType = "varchar" },
                        new ColumnMetadata { Name = "CountryID", DataType = "int" }
                    }
                },
                new TableMetadata
                {
                    Name = "tbl_Countries",
                    Columns = new List<ColumnMetadata>
                    {
                        new ColumnMetadata { Name = "CountryID", DataType = "int" },
                        new ColumnMetadata { Name = "CountryName", DataType = "varchar" }
                    }
                },
                new TableMetadata
                {
                    Name = "tbl_Currencies",
                    Columns = new List<ColumnMetadata>
                    {
                        new ColumnMetadata { Name = "CurrencyID", DataType = "int" },
                        new ColumnMetadata { Name = "CurrencyCode", DataType = "varchar" }
                    }
                }
            }
        };
    }
}

public class ComplexityAnalyzerTests
{
    private readonly ILogger _logger;
    private readonly ComplexityAnalyzer _analyzer;

    public ComplexityAnalyzerTests()
    {
        _logger = NullLogger.Instance;
        _analyzer = new ComplexityAnalyzer(_logger);
    }

    [Fact]
    public async Task AnalyzeComplexityAsync_SimpleQuery_ReturnsLowComplexity()
    {
        // Arrange
        var query = "Show me revenue";
        var semanticAnalysis = new SemanticAnalysis
        {
            Entities = new List<SemanticEntity>
            {
                new SemanticEntity { Type = EntityType.Column, Text = "revenue" }
            }
        };
        var schema = new SchemaMetadata();

        // Act
        var result = await _analyzer.AnalyzeComplexityAsync(query, semanticAnalysis, schema);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.ComplexityScore < 5);
        Assert.Equal(0, result.AggregationCount);
        Assert.Equal(0, result.FilterConditions);
    }

    [Fact]
    public async Task AnalyzeComplexityAsync_ComplexQuery_ReturnsHighComplexity()
    {
        // Arrange
        var query = "Show me total revenue and average deposits by country with filters";
        var semanticAnalysis = new SemanticAnalysis
        {
            Entities = new List<SemanticEntity>
            {
                new SemanticEntity { Type = EntityType.Aggregation, Text = "total" },
                new SemanticEntity { Type = EntityType.Aggregation, Text = "average" },
                new SemanticEntity { Type = EntityType.Table, Text = "revenue" },
                new SemanticEntity { Type = EntityType.Table, Text = "deposits" },
                new SemanticEntity { Type = EntityType.Table, Text = "country" },
                new SemanticEntity { Type = EntityType.Condition, Text = "filter1" },
                new SemanticEntity { Type = EntityType.Condition, Text = "filter2" },
                new SemanticEntity { Type = EntityType.DateRange, Text = "last week" },
                new SemanticEntity { Type = EntityType.DateRange, Text = "this month" }
            }
        };
        var schema = new SchemaMetadata();

        // Act
        var result = await _analyzer.AnalyzeComplexityAsync(query, semanticAnalysis, schema);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.ComplexityScore > 8);
        Assert.Equal(2, result.AggregationCount);
        Assert.Equal(2, result.FilterConditions);
        Assert.True(result.HasComplexTemporal);
    }
}
