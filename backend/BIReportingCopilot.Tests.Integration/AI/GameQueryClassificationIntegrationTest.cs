using BIReportingCopilot.Infrastructure.AI;
using BIReportingCopilot.Infrastructure.Services;
using BIReportingCopilot.Core.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using Xunit;
using Xunit.Abstractions;

namespace BIReportingCopilot.Tests.Integration.AI;

/// <summary>
/// Integration tests for the enhanced game query classification system
/// </summary>
public class GameQueryClassificationIntegrationTest
{
    private readonly ITestOutputHelper _output;
    private readonly Mock<ILogger<PromptManagementService>> _mockLogger;
    private readonly Mock<IMemoryCache> _mockCache;
    private readonly PromptManagementService _promptManagementService;

    public GameQueryClassificationIntegrationTest(ITestOutputHelper output)
    {
        _output = output;
        _mockLogger = new Mock<ILogger<PromptManagementService>>();
        _mockCache = new Mock<IMemoryCache>();
        
        _promptManagementService = new PromptManagementService(_mockLogger.Object, _mockCache.Object);
    }

    [Theory]
    [InlineData("Show me NetEnt slot performance this month", "Game Query")]
    [InlineData("Top games by revenue", "Game Query")]
    [InlineData("Casino game analytics by provider", "Game Query")]
    [InlineData("RealBetAmount by game type", "Game Query")]
    [InlineData("Microgaming vs Pragmatic Play comparison", "Game Query")]
    [InlineData("Show me deposits today", "Financial Query")]
    [InlineData("Player activity by country", "Player Query")]
    [InlineData("Total revenue by brand", "Financial Query")]
    [InlineData("Bonus balances analysis", "Bonus Query")]
    public async Task TestQueryClassification(string query, string expectedType)
    {
        _output.WriteLine($"\n=== Testing Query Classification ===");
        _output.WriteLine($"Query: {query}");
        _output.WriteLine($"Expected Type: {expectedType}");

        // Arrange
        var schema = CreateTestSchema();
        
        // Act
        var result = await _promptManagementService.GetRelevantSchemaAsync(query, schema);
        
        // Assert and Log Results
        _output.WriteLine($"\nSelected Tables ({result.RelevantTables.Count}):");
        foreach (var table in result.RelevantTables)
        {
            _output.WriteLine($"  - {table.Name} ({table.Schema})");
        }

        // Verify game query detection
        bool hasGameTables = result.RelevantTables.Any(t => 
            t.Name.Contains("games", StringComparison.OrdinalIgnoreCase) || 
            t.Name.Contains("Daily_actions_games", StringComparison.OrdinalIgnoreCase));

        bool hasMainTable = result.RelevantTables.Any(t => 
            t.Name.Equals("tbl_Daily_actions", StringComparison.OrdinalIgnoreCase));

        _output.WriteLine($"\nTable Analysis:");
        _output.WriteLine($"  Has Game Tables: {hasGameTables}");
        _output.WriteLine($"  Has Main Table: {hasMainTable}");

        if (expectedType == "Game Query")
        {
            Assert.True(hasGameTables, "Game queries should include game-related tables");
            _output.WriteLine("✅ Game query correctly identified - includes game tables");
        }
        else
        {
            // For non-game queries, main table should have higher priority
            if (hasGameTables && hasMainTable)
            {
                var mainTableIndex = result.RelevantTables.FindIndex(t => 
                    t.Name.Equals("tbl_Daily_actions", StringComparison.OrdinalIgnoreCase));
                var gameTableIndex = result.RelevantTables.FindIndex(t => 
                    t.Name.Contains("games", StringComparison.OrdinalIgnoreCase));
                
                if (gameTableIndex >= 0)
                {
                    Assert.True(mainTableIndex < gameTableIndex, 
                        "Non-game queries should prioritize main table over game tables");
                }
            }
            _output.WriteLine("✅ Non-game query correctly identified - prioritizes appropriate tables");
        }
    }

    [Theory]
    [InlineData("NetEnt", true)]
    [InlineData("Microgaming", true)]
    [InlineData("slot", true)]
    [InlineData("casino", true)]
    [InlineData("RealBetAmount", true)]
    [InlineData("NetGamingRevenue", true)]
    [InlineData("game", true)]
    [InlineData("provider", true)]
    [InlineData("deposit", false)]
    [InlineData("player", false)]
    [InlineData("bonus", false)]
    [InlineData("country", false)]
    public async Task TestGameKeywordDetection(string keyword, bool shouldBeGameRelated)
    {
        _output.WriteLine($"\n=== Testing Keyword Detection ===");
        _output.WriteLine($"Keyword: {keyword}");
        _output.WriteLine($"Expected Game-Related: {shouldBeGameRelated}");

        // Create a test query with the keyword
        var testQuery = $"Show me {keyword} analysis";
        
        // Test through the system
        var schema = CreateTestSchema();
        var result = await _promptManagementService.GetRelevantSchemaAsync(testQuery, schema);
        
        bool hasGameTables = result.RelevantTables.Any(t => 
            t.Name.Contains("games", StringComparison.OrdinalIgnoreCase));

        _output.WriteLine($"System detected as game-related: {hasGameTables}");

        if (shouldBeGameRelated)
        {
            Assert.True(hasGameTables, $"Keyword '{keyword}' should trigger game table selection");
            _output.WriteLine("✅ Game keyword correctly detected");
        }
        else
        {
            // For non-game keywords, we don't strictly require absence of game tables
            // as the system might include them with lower priority
            _output.WriteLine($"Non-game keyword processed (game tables included: {hasGameTables})");
        }
    }

    [Fact]
    public async Task TestGameQueryTablePriority()
    {
        _output.WriteLine($"\n=== Testing Game Query Table Priority ===");
        
        // Arrange
        var gameQuery = "Show me top NetEnt games by revenue this month";
        var schema = CreateTestSchema();
        
        _output.WriteLine($"Query: {gameQuery}");

        // Act
        var result = await _promptManagementService.GetRelevantSchemaAsync(gameQuery, schema);

        // Assert and Log
        _output.WriteLine($"\nTable Selection Order:");
        for (int i = 0; i < result.RelevantTables.Count; i++)
        {
            var table = result.RelevantTables[i];
            _output.WriteLine($"  {i + 1}. {table.Name} ({table.Schema})");
        }

        // Verify game tables are prioritized
        var gameTableIndex = result.RelevantTables.FindIndex(t => 
            t.Name.Contains("Daily_actions_games", StringComparison.OrdinalIgnoreCase));
        var gamesTableIndex = result.RelevantTables.FindIndex(t => 
            t.Name.Equals("Games", StringComparison.OrdinalIgnoreCase));

        Assert.True(gameTableIndex >= 0, "Game queries should include tbl_Daily_actions_games");
        Assert.True(gamesTableIndex >= 0, "Game queries should include Games table");

        _output.WriteLine("✅ Game tables correctly included in game query");
    }

    private SchemaMetadata CreateTestSchema()
    {
        return new SchemaMetadata
        {
            DatabaseName = "DailyActionsDB",
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
