using Microsoft.EntityFrameworkCore;
using BIReportingCopilot.Infrastructure.Data;
using BIReportingCopilot.Infrastructure.Query;
using BIReportingCopilot.Core.Interfaces;

namespace BIReportingCopilot.API
{
    public class DiagnoseGameDataIssue
    {
        public static async Task RunDiagnostics(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var queryService = scope.ServiceProvider.GetRequiredService<ISqlQueryService>();
            
            Console.WriteLine("🔍 DIAGNOSING GAME DATA ISSUE");
            Console.WriteLine("=====================================");
            
            var diagnosticQueries = new[]
            {
                // Check NetEnt games in Games table
                @"SELECT COUNT(*) as NetEntGamesCount 
                  FROM dbo.Games g WITH (NOLOCK) 
                  WHERE g.Provider = 'NetEnt'",
                
                // Check all providers
                @"SELECT TOP 10 g.Provider, COUNT(*) as GameCount 
                  FROM dbo.Games g WITH (NOLOCK) 
                  GROUP BY g.Provider 
                  ORDER BY GameCount DESC",
                
                // Check GameID ranges
                @"SELECT 'Games' as TableName, MIN(GameID) as MinID, MAX(GameID) as MaxID, COUNT(*) as Total
                  FROM dbo.Games g WITH (NOLOCK)
                  UNION ALL
                  SELECT 'DailyActionsGames' as TableName, MIN(GameID) as MinID, MAX(GameID) as MaxID, COUNT(DISTINCT GameID) as Total
                  FROM [DailyActionsDB].[common].[tbl_Daily_actions_games] WITH (NOLOCK)",
                
                // Check recent data in daily actions games
                @"SELECT COUNT(*) as ThisMonthRecords
                  FROM [DailyActionsDB].[common].[tbl_Daily_actions_games] dag WITH (NOLOCK)
                  WHERE dag.GameDate >= DATEADD(MONTH, DATEDIFF(MONTH, 0, GETDATE()), 0)
                    AND dag.GameDate < DATEADD(MONTH, DATEDIFF(MONTH, 0, GETDATE()) + 1, 0)",
                
                // Test join with sample
                @"SELECT TOP 5 
                    dag.GameID as DagGameID,
                    dag.GameID - 1000000 as CalculatedGameID,
                    g.GameID as GamesTableGameID,
                    g.GameName,
                    g.Provider
                  FROM [DailyActionsDB].[common].[tbl_Daily_actions_games] dag WITH (NOLOCK)
                  INNER JOIN dbo.Games g WITH (NOLOCK) ON g.GameID = dag.GameID - 1000000
                  WHERE dag.GameDate >= DATEADD(DAY, -7, GETDATE())",
                
                // Check for NetEnt with join (no date filter)
                @"SELECT COUNT(*) as NetEntJoinCount
                  FROM [DailyActionsDB].[common].[tbl_Daily_actions_games] dag WITH (NOLOCK)
                  INNER JOIN dbo.Games g WITH (NOLOCK) ON g.GameID = dag.GameID - 1000000
                  WHERE g.Provider = 'NetEnt'",
                
                // Check latest dates
                @"SELECT MAX(GameDate) as LatestDate, MIN(GameDate) as EarliestDate
                  FROM [DailyActionsDB].[common].[tbl_Daily_actions_games] WITH (NOLOCK)"
            };
            
            for (int i = 0; i < diagnosticQueries.Length; i++)
            {
                try
                {
                    Console.WriteLine($"\n📊 Query {i + 1}:");
                    Console.WriteLine($"SQL: {diagnosticQueries[i]}");
                    
                    var result = await queryService.ExecuteSelectQueryAsync(diagnosticQueries[i]);
                    
                    if (result.IsSuccessful && result.Data?.Length > 0)
                    {
                        Console.WriteLine("✅ Results:");
                        foreach (var row in result.Data.Take(10))
                        {
                            if (row is Dictionary<string, object> dict)
                            {
                                var values = string.Join(" | ", dict.Values.Select(v => v?.ToString() ?? "NULL"));
                                Console.WriteLine($"   {values}");
                            }
                            else
                            {
                                Console.WriteLine($"   {row?.ToString() ?? "NULL"}");
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine($"❌ No results or error: {result.Metadata.Error}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Exception: {ex.Message}");
                }
                
                await Task.Delay(500); // Small delay between queries
            }
            
            Console.WriteLine("\n🔍 DIAGNOSIS COMPLETE");
            Console.WriteLine("=====================================");
            
            // Provide recommendations
            Console.WriteLine("\n💡 RECOMMENDATIONS:");
            Console.WriteLine("1. Check if NetEnt games exist in Games table");
            Console.WriteLine("2. Verify GameID offset (should be GameID - 1000000)");
            Console.WriteLine("3. Confirm data exists for current month");
            Console.WriteLine("4. Check provider name spelling (case sensitive)");
        }
    }
}
