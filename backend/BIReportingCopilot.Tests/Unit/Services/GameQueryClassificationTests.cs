using BIReportingCopilot.Infrastructure.AI;
using BIReportingCopilot.Infrastructure.Services;
using BIReportingCopilot.Core.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using Xunit;

namespace BIReportingCopilot.Tests.Unit.Services;

/// <summary>
/// Tests for enhanced game query classification and context-aware prompt engineering
/// </summary>
public class GameQueryClassificationTests
{
    private readonly Mock<ILogger<PromptManagementService>> _mockLogger;
    private readonly Mock<IMemoryCache> _mockCache;
    private readonly PromptManagementService _promptManagementService;

    public GameQueryClassificationTests()
    {
        _mockLogger = new Mock<ILogger<PromptManagementService>>();
        _mockCache = new Mock<IMemoryCache>();
        _promptManagementService = new PromptManagementService(_mockLogger.Object, _mockCache.Object);
    }

    [Theory]
    [InlineData("show me game performance by provider", true)]
    [InlineData("top games by revenue", true)]
    [InlineData("NetEnt slot performance", true)]
    [InlineData("casino game analytics", true)]
    [InlineData("RealBetAmount by game type", true)]
    [InlineData("gaming revenue by provider", true)]
    [InlineData("show me deposits today", false)]
    [InlineData("player activity by country", false)]
    [InlineData("bonus balances", false)]
    [InlineData("total revenue by brand", false)]
    public void IsGameRelatedQuery_ShouldCorrectlyIdentifyGameQueries(string query, bool expectedIsGameQuery)
    {
        // This test would require making the IsGameRelatedQuery method public or internal
        // For now, we'll test the behavior through the table relevance calculation
        
        // Arrange
        var schema = CreateTestSchema();
        var semanticAnalysis = new SemanticAnalysis
        {
            OriginalQuery = query,
            Intent = "analytics",
            Keywords = query.Split(' ').ToList(),
            Entities = new List<NamedEntity>()
        };

        // Act - Test through GetRelevantSchemaAsync which uses the game detection logic
        var result = _promptManagementService.GetRelevantSchemaAsync(query, schema).Result;

        // Assert
        if (expectedIsGameQuery)
        {
            // Should include game-related tables
            Assert.Contains(result.RelevantTables, t => t.Name.Contains("games") || t.Name.Contains("Daily_actions_games"));
        }
        else
        {
            // Should prioritize non-game tables for non-game queries
            var gameTableCount = result.RelevantTables.Count(t => t.Name.Contains("games"));
            var nonGameTableCount = result.RelevantTables.Count(t => !t.Name.Contains("games"));
            
            // For non-game queries, non-game tables should be prioritized
            Assert.True(nonGameTableCount >= gameTableCount);
        }
    }

    [Fact]
    public void GetRelevantSchema_GameQuery_ShouldIncludeGameTables()
    {
        // Arrange
        var gameQuery = "show me top NetEnt games by revenue";
        var schema = CreateTestSchema();

        // Act
        var result = _promptManagementService.GetRelevantSchemaAsync(gameQuery, schema).Result;

        // Assert
        Assert.Contains(result.RelevantTables, t => t.Name == "tbl_Daily_actions_games");
        Assert.Contains(result.RelevantTables, t => t.Name == "Games");
    }

    [Fact]
    public void GetRelevantSchema_DepositQuery_ShouldPrioritizeMainTable()
    {
        // Arrange
        var depositQuery = "show me deposits today";
        var schema = CreateTestSchema();

        // Act
        var result = _promptManagementService.GetRelevantSchemaAsync(depositQuery, schema).Result;

        // Assert
        Assert.Contains(result.RelevantTables, t => t.Name == "tbl_Daily_actions");
        // Game tables should have lower priority for deposit queries
        var mainTableIndex = result.RelevantTables.FindIndex(t => t.Name == "tbl_Daily_actions");
        var gameTableIndex = result.RelevantTables.FindIndex(t => t.Name == "tbl_Daily_actions_games");
        
        if (gameTableIndex >= 0)
        {
            Assert.True(mainTableIndex < gameTableIndex, "Main table should have higher priority than game tables for deposit queries");
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
                    Schema = "common",
                    Description = "Main daily actions table",
                    Columns = new List<ColumnMetadata>
                    {
                        new ColumnMetadata { Name = "PlayerID", DataType = "bigint", IsPrimaryKey = true },
                        new ColumnMetadata { Name = "Date", DataType = "date" },
                        new ColumnMetadata { Name = "Deposits", DataType = "decimal" },
                        new ColumnMetadata { Name = "WhiteLabelID", DataType = "int" }
                    }
                },
                new TableMetadata
                {
                    Name = "tbl_Daily_actions_games",
                    Schema = "common", 
                    Description = "Game-specific daily actions",
                    Columns = new List<ColumnMetadata>
                    {
                        new ColumnMetadata { Name = "PlayerID", DataType = "bigint" },
                        new ColumnMetadata { Name = "GameID", DataType = "bigint" },
                        new ColumnMetadata { Name = "GameDate", DataType = "date" },
                        new ColumnMetadata { Name = "RealBetAmount", DataType = "decimal" },
                        new ColumnMetadata { Name = "RealWinAmount", DataType = "decimal" },
                        new ColumnMetadata { Name = "NetGamingRevenue", DataType = "decimal" }
                    }
                },
                new TableMetadata
                {
                    Name = "Games",
                    Schema = "dbo",
                    Description = "Games master table",
                    Columns = new List<ColumnMetadata>
                    {
                        new ColumnMetadata { Name = "GameID", DataType = "int", IsPrimaryKey = true },
                        new ColumnMetadata { Name = "GameName", DataType = "nvarchar" },
                        new ColumnMetadata { Name = "Provider", DataType = "nvarchar" },
                        new ColumnMetadata { Name = "SubProvider", DataType = "nvarchar" },
                        new ColumnMetadata { Name = "GameType", DataType = "nvarchar" }
                    }
                },
                new TableMetadata
                {
                    Name = "tbl_Daily_actions_players",
                    Schema = "common",
                    Description = "Player demographics",
                    Columns = new List<ColumnMetadata>
                    {
                        new ColumnMetadata { Name = "PlayerID", DataType = "bigint", IsPrimaryKey = true },
                        new ColumnMetadata { Name = "Username", DataType = "nvarchar" },
                        new ColumnMetadata { Name = "CountryID", DataType = "int" }
                    }
                },
                new TableMetadata
                {
                    Name = "tbl_Countries",
                    Schema = "common",
                    Description = "Countries lookup",
                    Columns = new List<ColumnMetadata>
                    {
                        new ColumnMetadata { Name = "CountryID", DataType = "int", IsPrimaryKey = true },
                        new ColumnMetadata { Name = "CountryName", DataType = "nvarchar" }
                    }
                }
            }
        };
    }
}
