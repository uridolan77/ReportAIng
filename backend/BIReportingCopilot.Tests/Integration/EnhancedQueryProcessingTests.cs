using BIReportingCopilot.Core.Interfaces;
using BIReportingCopilot.Core.Models;
using BIReportingCopilot.Core.Models.BusinessContext;
using BIReportingCopilot.Infrastructure.Query;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace BIReportingCopilot.Tests.Integration;

/// <summary>
/// Integration tests for the enhanced query processing pipeline
/// </summary>
public class EnhancedQueryProcessingTests : IClassFixture<TestFixture>
{
    private readonly TestFixture _fixture;
    private readonly IServiceProvider _serviceProvider;

    public EnhancedQueryProcessingTests(TestFixture fixture)
    {
        _fixture = fixture;
        _serviceProvider = _fixture.ServiceProvider;
    }

    [Fact]
    public async Task ProcessQueryAsync_WithValidQuery_ShouldGenerateEnhancedSql()
    {
        // Arrange
        var service = _serviceProvider.GetRequiredService<IEnhancedQueryProcessingService>();
        var userQuestion = "Show me top 10 depositors yesterday from UK";
        var userId = "test-user-123";

        // Act
        var result = await service.ProcessQueryAsync(userQuestion, userId);

        // Assert
        Assert.True(result.Success);
        Assert.NotEmpty(result.GeneratedSql);
        Assert.NotNull(result.BusinessProfile);
        Assert.True(result.OverallConfidence > 0);
        Assert.Contains("SELECT", result.GeneratedSql);
    }

    [Fact]
    public async Task ProcessQueryAsync_WithComplexQuery_ShouldIncludeJoins()
    {
        // Arrange
        var service = _serviceProvider.GetRequiredService<IEnhancedQueryProcessingService>();
        var userQuestion = "What are the total deposits by country and currency last month?";
        var userId = "test-user-456";

        // Act
        var result = await service.ProcessQueryAsync(userQuestion, userId);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.JoinResult);
        Assert.True(result.JoinResult.Success);
        Assert.NotEmpty(result.JoinResult.JoinClause);
        Assert.Contains("JOIN", result.GeneratedSql);
    }

    [Fact]
    public async Task ProcessQueryAsync_WithTimeContext_ShouldIncludeDateFilter()
    {
        // Arrange
        var service = _serviceProvider.GetRequiredService<IEnhancedQueryProcessingService>();
        var userQuestion = "Show deposits from yesterday";
        var userId = "test-user-789";

        // Act
        var result = await service.ProcessQueryAsync(userQuestion, userId);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.DateFilterResult);
        Assert.True(result.DateFilterResult.Success);
        Assert.NotEmpty(result.DateFilterResult.WhereClause);
        Assert.Contains("WHERE", result.GeneratedSql);
    }

    [Fact]
    public async Task ProcessQueryAsync_WithAggregationIntent_ShouldIncludeGroupBy()
    {
        // Arrange
        var service = _serviceProvider.GetRequiredService<IEnhancedQueryProcessingService>();
        var userQuestion = "Total revenue by country";
        var userId = "test-user-101";

        // Act
        var result = await service.ProcessQueryAsync(userQuestion, userId);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.AggregationResult);
        Assert.True(result.AggregationResult.Success);
        Assert.NotEmpty(result.AggregationResult.GroupByClause);
        Assert.Contains("GROUP BY", result.GeneratedSql);
    }
}

/// <summary>
/// Unit tests for SQL JOIN Generator Service
/// </summary>
public class SqlJoinGeneratorServiceTests
{
    private readonly Mock<ILogger<SqlJoinGeneratorService>> _mockLogger;
    private readonly Mock<IForeignKeyRelationshipService> _mockRelationshipService;
    private readonly SqlJoinGeneratorService _service;

    public SqlJoinGeneratorServiceTests()
    {
        _mockLogger = new Mock<ILogger<SqlJoinGeneratorService>>();
        _mockRelationshipService = new Mock<IForeignKeyRelationshipService>();
        _service = new SqlJoinGeneratorService(_mockLogger.Object, _mockRelationshipService.Object);
    }

    [Fact]
    public async Task GenerateJoinsAsync_WithSingleTable_ShouldReturnEmptyJoin()
    {
        // Arrange
        var tableNames = new List<string> { "tbl_Daily_actions" };

        // Act
        var result = await _service.GenerateJoinsAsync(tableNames);

        // Assert
        Assert.True(result.Success);
        Assert.Empty(result.JoinClause);
        Assert.Single(result.TableAliases);
    }

    [Fact]
    public async Task GenerateJoinsAsync_WithRelatedTables_ShouldGenerateJoins()
    {
        // Arrange
        var tableNames = new List<string> { "tbl_Daily_actions", "tbl_Countries" };
        var relationships = new List<ForeignKeyRelationship>
        {
            new ForeignKeyRelationship
            {
                ParentTable = "tbl_Daily_actions",
                ParentColumn = "CountryId",
                ReferencedTable = "tbl_Countries",
                ReferencedColumn = "Id",
                IsEnabled = true
            }
        };

        _mockRelationshipService
            .Setup(x => x.GetRelationshipsForTablesAsync(It.IsAny<List<string>>(), It.IsAny<string>()))
            .ReturnsAsync(relationships);

        _mockRelationshipService
            .Setup(x => x.GenerateJoinPathsAsync(It.IsAny<List<string>>(), It.IsAny<string>()))
            .ReturnsAsync(new List<JoinPath>
            {
                new JoinPath
                {
                    FromTable = "tbl_Daily_actions",
                    ToTable = "tbl_Countries",
                    JoinConditions = new List<JoinCondition>
                    {
                        new JoinCondition
                        {
                            LeftTable = "tbl_Daily_actions",
                            LeftColumn = "CountryId",
                            RightTable = "tbl_Countries",
                            RightColumn = "Id"
                        }
                    },
                    IsOptimal = true
                }
            });

        // Act
        var result = await _service.GenerateJoinsAsync(tableNames);

        // Assert
        Assert.True(result.Success);
        Assert.NotEmpty(result.JoinClause);
        Assert.Contains("JOIN", result.JoinClause);
        Assert.Contains("WITH (NOLOCK)", result.JoinClause);
        Assert.Equal(2, result.TableAliases.Count);
    }

    [Fact]
    public void GenerateTableAliases_WithMultipleTables_ShouldCreateUniqueAliases()
    {
        // Arrange
        var tableNames = new List<string> 
        { 
            "tbl_Daily_actions", 
            "tbl_Countries", 
            "tbl_Currencies" 
        };

        // Act
        var aliases = _service.GenerateTableAliases(tableNames);

        // Assert
        Assert.Equal(3, aliases.Count);
        Assert.All(aliases.Values, alias => Assert.NotEmpty(alias));
        Assert.Equal(aliases.Values.Count, aliases.Values.Distinct().Count()); // All unique
    }
}

/// <summary>
/// Unit tests for SQL Date Filter Service
/// </summary>
public class SqlDateFilterServiceTests
{
    private readonly Mock<ILogger<SqlDateFilterService>> _mockLogger;
    private readonly SqlDateFilterService _service;

    public SqlDateFilterServiceTests()
    {
        _mockLogger = new Mock<ILogger<SqlDateFilterService>>();
        _service = new SqlDateFilterService(_mockLogger.Object);
    }

    [Fact]
    public async Task GenerateDateFilterAsync_WithYesterdayTimeRange_ShouldGenerateCorrectFilter()
    {
        // Arrange
        var timeRange = new TimeRange
        {
            StartDate = DateTime.Today.AddDays(-1),
            EndDate = DateTime.Today,
            RelativeExpression = "yesterday",
            Granularity = TimeGranularity.Day
        };
        var dateColumns = new List<string> { "Date", "CreatedDate" };

        // Act
        var result = await _service.GenerateDateFilterAsync(timeRange, dateColumns);

        // Assert
        Assert.True(result.Success);
        Assert.NotEmpty(result.WhereClause);
        Assert.Contains("Date >=", result.WhereClause);
        Assert.Contains(DateTime.Today.AddDays(-1).ToString("yyyy-MM-dd"), result.WhereClause);
    }

    [Fact]
    public async Task GenerateDateFilterAsync_WithNoTimeRange_ShouldReturnEmptyFilter()
    {
        // Arrange
        TimeRange? timeRange = null;
        var dateColumns = new List<string> { "Date" };

        // Act
        var result = await _service.GenerateDateFilterAsync(timeRange, dateColumns);

        // Assert
        Assert.True(result.Success);
        Assert.Empty(result.WhereClause);
    }

    [Fact]
    public async Task ValidateDateColumnsAsync_WithValidColumns_ShouldReturnValid()
    {
        // Arrange
        var dateColumns = new List<string> { "Date", "CreatedDate", "UpdatedDate" };
        var timeRange = new TimeRange
        {
            StartDate = DateTime.Today,
            EndDate = DateTime.Today.AddDays(1),
            Granularity = TimeGranularity.Day
        };

        // Act
        var result = await _service.ValidateDateColumnsAsync(dateColumns, timeRange);

        // Assert
        Assert.True(result.IsValid);
        Assert.Equal(3, result.ValidColumns.Count);
        Assert.Empty(result.InvalidColumns);
        Assert.NotEmpty(result.RecommendedColumn);
    }
}

/// <summary>
/// Unit tests for SQL Aggregation Service
/// </summary>
public class SqlAggregationServiceTests
{
    private readonly Mock<ILogger<SqlAggregationService>> _mockLogger;
    private readonly SqlAggregationService _service;

    public SqlAggregationServiceTests()
    {
        _mockLogger = new Mock<ILogger<SqlAggregationService>>();
        _service = new SqlAggregationService(_mockLogger.Object);
    }

    [Fact]
    public async Task GenerateAggregationAsync_WithAnalyticsIntent_ShouldGenerateAggregation()
    {
        // Arrange
        var profile = new BusinessContextProfile
        {
            Intent = new BusinessIntent { Type = IntentType.Analytics },
            BusinessTerms = new List<string> { "total", "depositor", "country" },
            ConfidenceScore = 0.8
        };
        var availableColumns = new List<string> 
        { 
            "DepositAmount", 
            "CountryName", 
            "Date", 
            "PlayerId" 
        };

        // Act
        var result = await _service.GenerateAggregationAsync(profile, availableColumns);

        // Assert
        Assert.True(result.Success);
        Assert.NotEmpty(result.SelectClause);
        Assert.Contains("SELECT", result.SelectClause);
        Assert.True(result.Metrics.Any() || result.Dimensions.Any());
    }

    [Fact]
    public async Task ValidateAggregationAsync_WithValidMetricsAndDimensions_ShouldReturnValid()
    {
        // Arrange
        var metrics = new List<SqlMetric>
        {
            new SqlMetric { ColumnName = "DepositAmount", Function = "SUM", Alias = "TotalDeposits" }
        };
        var dimensions = new List<SqlDimension>
        {
            new SqlDimension { ColumnName = "CountryName", Alias = "Country" }
        };
        var availableColumns = new List<string> { "DepositAmount", "CountryName", "Date" };

        // Act
        var result = await _service.ValidateAggregationAsync(metrics, dimensions, availableColumns);

        // Assert
        Assert.True(result.IsValid);
        Assert.Single(result.ValidMetrics);
        Assert.Single(result.ValidDimensions);
        Assert.Empty(result.InvalidMetrics);
        Assert.Empty(result.InvalidDimensions);
    }
}
