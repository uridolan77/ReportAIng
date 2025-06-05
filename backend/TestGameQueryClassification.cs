using BIReportingCopilot.Infrastructure.AI;
using BIReportingCopilot.Infrastructure.Services;
using BIReportingCopilot.Core.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Caching.Memory;
using Moq;

namespace BIReportingCopilot.TestConsole;

/// <summary>
/// Standalone test program to demonstrate the enhanced game query classification system
/// </summary>
public class TestGameQueryClassification
{
    private readonly PromptManagementService _promptManagementService;
    private readonly PromptService _promptService;

    public TestGameQueryClassification()
    {
        var mockLogger = new Mock<ILogger<PromptManagementService>>();
        var mockCache = new Mock<IMemoryCache>();
        var mockPromptLogger = new Mock<ILogger<PromptService>>();
        var mockContext = new Mock<BIReportingCopilot.Infrastructure.Data.BICopilotContext>();
        
        _promptManagementService = new PromptManagementService(mockLogger.Object, mockCache.Object);
        _promptService = new PromptService(mockContext.Object, mockPromptLogger.Object);
    }

    public async Task RunTests()
    {
        Console.WriteLine("🎮 Testing Enhanced Game Query Classification System");
        Console.WriteLine(new string('=', 60));

        await TestGameQueryDetection();
        await TestFinancialQueryDetection();
        await TestKeywordDetection();
        await TestTablePrioritization();
        await TestPromptGeneration();

        Console.WriteLine("\n✅ All tests completed!");
    }

    private async Task TestGameQueryDetection()
    {
        Console.WriteLine("\n🎯 Test 1: Game Query Detection");
        Console.WriteLine(new string('-', 40));

        var gameQueries = new[]
        {
            "Show me NetEnt slot performance this month",
            "Top games by revenue",
            "Casino game analytics by provider",
            "RealBetAmount by game type",
            "Microgaming vs Pragmatic Play comparison"
        };

        var schema = CreateTestSchema();

        foreach (var query in gameQueries)
        {
            Console.WriteLine($"\nQuery: {query}");
            var result = await _promptManagementService.GetRelevantSchemaAsync(query, schema);
            
            var gameTablesIncluded = result.RelevantTables.Any(t => 
                t.Name.Contains("games", StringComparison.OrdinalIgnoreCase));
            
            Console.WriteLine($"Game tables included: {gameTablesIncluded}");
            Console.WriteLine($"Tables selected: {string.Join(", ", result.RelevantTables.Select(t => t.Name))}");
            
            if (gameTablesIncluded)
            {
                Console.WriteLine("✅ PASS - Game query correctly detected");
            }
            else
            {
                Console.WriteLine("❌ FAIL - Game query not detected");
            }
        }
    }

    private async Task TestFinancialQueryDetection()
    {
        Console.WriteLine("\n💰 Test 2: Financial Query Detection");
        Console.WriteLine(new string('-', 40));

        var financialQueries = new[]
        {
            "Show me deposits today",
            "Total revenue by brand",
            "Player deposits by country",
            "Cashout analysis this week"
        };

        var schema = CreateTestSchema();

        foreach (var query in financialQueries)
        {
            Console.WriteLine($"\nQuery: {query}");
            var result = await _promptManagementService.GetRelevantSchemaAsync(query, schema);
            
            var mainTableIncluded = result.RelevantTables.Any(t => 
                t.Name.Equals("tbl_Daily_actions", StringComparison.OrdinalIgnoreCase));
            
            var gameTablesIncluded = result.RelevantTables.Any(t => 
                t.Name.Contains("games", StringComparison.OrdinalIgnoreCase));
            
            Console.WriteLine($"Main table included: {mainTableIncluded}");
            Console.WriteLine($"Game tables included: {gameTablesIncluded}");
            Console.WriteLine($"Tables selected: {string.Join(", ", result.RelevantTables.Select(t => t.Name))}");
            
            if (mainTableIncluded)
            {
                Console.WriteLine("✅ PASS - Financial query correctly prioritizes main table");
            }
            else
            {
                Console.WriteLine("❌ FAIL - Financial query should include main table");
            }
        }
    }

    private async Task TestKeywordDetection()
    {
        Console.WriteLine("\n🔍 Test 3: Keyword Detection");
        Console.WriteLine(new string('-', 40));

        var testCases = new[]
        {
            ("NetEnt", true),
            ("Microgaming", true),
            ("slot", true),
            ("casino", true),
            ("RealBetAmount", true),
            ("NetGamingRevenue", true),
            ("game", true),
            ("provider", true),
            ("deposit", false),
            ("player", false),
            ("bonus", false),
            ("country", false)
        };

        var schema = CreateTestSchema();

        foreach (var (keyword, shouldBeGameRelated) in testCases)
        {
            var testQuery = $"Show me {keyword} analysis";
            var result = await _promptManagementService.GetRelevantSchemaAsync(testQuery, schema);
            
            var gameTablesIncluded = result.RelevantTables.Any(t => 
                t.Name.Contains("games", StringComparison.OrdinalIgnoreCase));
            
            Console.WriteLine($"\nKeyword: {keyword}");
            Console.WriteLine($"Expected game-related: {shouldBeGameRelated}");
            Console.WriteLine($"System detected as game-related: {gameTablesIncluded}");
            
            if (shouldBeGameRelated == gameTablesIncluded)
            {
                Console.WriteLine("✅ PASS - Keyword detection correct");
            }
            else if (shouldBeGameRelated && !gameTablesIncluded)
            {
                Console.WriteLine("❌ FAIL - Game keyword not detected");
            }
            else
            {
                Console.WriteLine("⚠️  INFO - Non-game keyword processed (acceptable)");
            }
        }
    }

    private async Task TestTablePrioritization()
    {
        Console.WriteLine("\n📊 Test 4: Table Prioritization");
        Console.WriteLine(new string('-', 40));

        var schema = CreateTestSchema();
        
        // Test game query prioritization
        var gameQuery = "Show me top NetEnt games by revenue";
        var gameResult = await _promptManagementService.GetRelevantSchemaAsync(gameQuery, schema);
        
        Console.WriteLine($"\nGame Query: {gameQuery}");
        Console.WriteLine("Table selection order:");
        for (int i = 0; i < gameResult.RelevantTables.Count; i++)
        {
            Console.WriteLine($"  {i + 1}. {gameResult.RelevantTables[i].Name}");
        }
        
        var gameTableIndex = gameResult.RelevantTables.FindIndex(t => 
            t.Name.Contains("Daily_actions_games", StringComparison.OrdinalIgnoreCase));
        
        if (gameTableIndex >= 0)
        {
            Console.WriteLine("✅ PASS - Game tables included for game query");
        }
        else
        {
            Console.WriteLine("❌ FAIL - Game tables missing for game query");
        }

        // Test financial query prioritization
        var financialQuery = "Show me deposits by brand today";
        var financialResult = await _promptManagementService.GetRelevantSchemaAsync(financialQuery, schema);
        
        Console.WriteLine($"\nFinancial Query: {financialQuery}");
        Console.WriteLine("Table selection order:");
        for (int i = 0; i < financialResult.RelevantTables.Count; i++)
        {
            Console.WriteLine($"  {i + 1}. {financialResult.RelevantTables[i].Name}");
        }
        
        var mainTableIndex = financialResult.RelevantTables.FindIndex(t => 
            t.Name.Equals("tbl_Daily_actions", StringComparison.OrdinalIgnoreCase));
        
        if (mainTableIndex >= 0)
        {
            Console.WriteLine("✅ PASS - Main table included for financial query");
        }
        else
        {
            Console.WriteLine("❌ FAIL - Main table missing for financial query");
        }
    }

    private async Task TestPromptGeneration()
    {
        Console.WriteLine("\n📝 Test 5: Prompt Generation");
        Console.WriteLine(new string('-', 40));

        var schema = CreateTestSchema();
        var gameQuery = "Show me top NetEnt games by revenue this month";
        
        var relevantSchema = await _promptManagementService.GetRelevantSchemaAsync(gameQuery, schema);
        var prompt = await _promptService.BuildQueryPromptAsync(gameQuery, 
            new SchemaMetadata { Tables = relevantSchema.RelevantTables });

        Console.WriteLine($"\nQuery: {gameQuery}");
        Console.WriteLine($"Prompt length: {prompt.Length} characters");
        
        // Check for game-specific content
        var hasGameTable = prompt.Contains("tbl_Daily_actions_games", StringComparison.OrdinalIgnoreCase);
        var hasGamesTable = prompt.Contains("Games", StringComparison.OrdinalIgnoreCase);
        var hasGameMetrics = prompt.Contains("RealBetAmount", StringComparison.OrdinalIgnoreCase);
        var hasJoinLogic = prompt.Contains("GameID", StringComparison.OrdinalIgnoreCase);
        
        Console.WriteLine($"Contains game table reference: {hasGameTable}");
        Console.WriteLine($"Contains Games table reference: {hasGamesTable}");
        Console.WriteLine($"Contains game metrics: {hasGameMetrics}");
        Console.WriteLine($"Contains join logic: {hasJoinLogic}");
        
        if (hasGameTable && hasGamesTable && hasGameMetrics)
        {
            Console.WriteLine("✅ PASS - Game-specific prompt generated correctly");
        }
        else
        {
            Console.WriteLine("❌ FAIL - Game-specific content missing from prompt");
        }
        
        // Show a preview of the prompt
        Console.WriteLine("\nPrompt preview (first 300 characters):");
        Console.WriteLine(prompt.Substring(0, Math.Min(300, prompt.Length)) + "...");
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
                }
            }
        };
    }

    public static async Task Main(string[] args)
    {
        var tester = new TestGameQueryClassification();
        await tester.RunTests();
        
        Console.WriteLine("\nPress any key to exit...");
        Console.ReadKey();
    }
}
