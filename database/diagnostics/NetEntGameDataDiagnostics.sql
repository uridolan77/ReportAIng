-- =============================================
-- NetEnt Game Data Diagnostics
-- Investigating why "NetEnt games performance this month" returns 0 rows
-- =============================================

PRINT '=== NetEnt Game Data Diagnostics ===';
PRINT 'Current Date: ' + CAST(GETDATE() AS VARCHAR(50));
PRINT 'Current Month Start: ' + CAST(DATEADD(MONTH, DATEDIFF(MONTH, 0, GETDATE()), 0) AS VARCHAR(50));
PRINT 'Current Month End: ' + CAST(DATEADD(MONTH, DATEDIFF(MONTH, 0, GETDATE()) + 1, 0) AS VARCHAR(50));
PRINT '';

-- 1. Check if Games table has NetEnt games
PRINT '1. Checking Games table for NetEnt provider...';
SELECT 'Games Table - NetEnt Count' as CheckType, 
       COUNT(*) as NetEntGamesCount,
       COUNT(DISTINCT Provider) as UniqueProviders
FROM dbo.Games g WITH (NOLOCK)
WHERE g.Provider = 'NetEnt';

-- Show sample NetEnt games
SELECT TOP 5 'Sample NetEnt Games' as CheckType,
       g.GameID, g.GameName, g.Provider, g.GameType
FROM dbo.Games g WITH (NOLOCK)
WHERE g.Provider = 'NetEnt';

-- 2. Check all providers in Games table (to see if NetEnt is named differently)
PRINT '2. Checking all providers in Games table...';
SELECT 'All Providers' as CheckType,
       g.Provider, 
       COUNT(*) as GameCount
FROM dbo.Games g WITH (NOLOCK)
GROUP BY g.Provider
ORDER BY g.Provider;

-- 3. Check if there's any game data for this month
PRINT '3. Checking tbl_Daily_actions_games for this month...';
SELECT 'Daily Actions Games - This Month' as CheckType,
       COUNT(*) as TotalRecords,
       COUNT(DISTINCT GameID) as UniqueGames,
       MIN(GameDate) as EarliestDate,
       MAX(GameDate) as LatestDate
FROM [DailyActionsDB].[common].[tbl_Daily_actions_games] dag WITH (NOLOCK)
WHERE dag.GameDate >= DATEADD(MONTH, DATEDIFF(MONTH, 0, GETDATE()), 0)
  AND dag.GameDate < DATEADD(MONTH, DATEDIFF(MONTH, 0, GETDATE()) + 1, 0);

-- 4. Check GameID ranges in both tables
PRINT '4. Checking GameID ranges...';
SELECT 'Games Table GameID Range' as CheckType,
       MIN(GameID) as MinGameID,
       MAX(GameID) as MaxGameID,
       COUNT(*) as TotalGames
FROM dbo.Games g WITH (NOLOCK);

SELECT 'Daily Actions Games - GameID Range This Month' as CheckType,
       MIN(GameID) as MinGameID,
       MAX(GameID) as MaxGameID,
       COUNT(DISTINCT GameID) as UniqueGames
FROM [DailyActionsDB].[common].[tbl_Daily_actions_games] dag WITH (NOLOCK)
WHERE dag.GameDate >= DATEADD(MONTH, DATEDIFF(MONTH, 0, GETDATE()), 0)
  AND dag.GameDate < DATEADD(MONTH, DATEDIFF(MONTH, 0, GETDATE()) + 1, 0);

-- 5. Test the join logic with sample data
PRINT '5. Testing join logic...';
SELECT TOP 5 'Join Test - This Month' as CheckType,
       dag.GameID as DagGameID,
       dag.GameID - 1000000 as CalculatedGameID,
       g.GameID as GamesGameID,
       g.GameName,
       g.Provider
FROM [DailyActionsDB].[common].[tbl_Daily_actions_games] dag WITH (NOLOCK)
INNER JOIN dbo.Games g WITH (NOLOCK) ON g.GameID = dag.GameID - 1000000
WHERE dag.GameDate >= DATEADD(MONTH, DATEDIFF(MONTH, 0, GETDATE()), 0)
  AND dag.GameDate < DATEADD(MONTH, DATEDIFF(MONTH, 0, GETDATE()) + 1, 0);

-- 6. Check for NetEnt games specifically with the join
PRINT '6. Checking NetEnt games with join for this month...';
SELECT 'NetEnt Games This Month' as CheckType,
       COUNT(*) as RecordCount,
       COUNT(DISTINCT g.GameName) as UniqueGames,
       SUM(dag.RealBetAmount) as TotalBets,
       SUM(dag.NetGamingRevenue) as TotalRevenue
FROM [DailyActionsDB].[common].[tbl_Daily_actions_games] dag WITH (NOLOCK)
INNER JOIN dbo.Games g WITH (NOLOCK) ON g.GameID = dag.GameID - 1000000
WHERE dag.GameDate >= DATEADD(MONTH, DATEDIFF(MONTH, 0, GETDATE()), 0)
  AND dag.GameDate < DATEADD(MONTH, DATEDIFF(MONTH, 0, GETDATE()) + 1, 0)
  AND g.Provider = 'NetEnt';

-- 7. Check recent dates in tbl_Daily_actions_games
PRINT '7. Checking recent dates in game data...';
SELECT 'Recent Game Data Dates' as CheckType,
       GameDate,
       COUNT(*) as RecordCount,
       COUNT(DISTINCT GameID) as UniqueGames
FROM [DailyActionsDB].[common].[tbl_Daily_actions_games] dag WITH (NOLOCK)
WHERE dag.GameDate >= DATEADD(day, -30, GETDATE())
GROUP BY GameDate
ORDER BY GameDate DESC;

-- 8. Check if there are any NetEnt games in recent data (last 30 days)
PRINT '8. Checking NetEnt games in last 30 days...';
SELECT 'NetEnt Games Last 30 Days' as CheckType,
       COUNT(*) as RecordCount,
       COUNT(DISTINCT g.GameName) as UniqueGames,
       MIN(dag.GameDate) as EarliestDate,
       MAX(dag.GameDate) as LatestDate
FROM [DailyActionsDB].[common].[tbl_Daily_actions_games] dag WITH (NOLOCK)
INNER JOIN dbo.Games g WITH (NOLOCK) ON g.GameID = dag.GameID - 1000000
WHERE dag.GameDate >= DATEADD(day, -30, GETDATE())
  AND g.Provider = 'NetEnt';

-- 9. Show sample NetEnt game data if any exists
SELECT TOP 5 'Sample NetEnt Game Data' as CheckType,
       dag.GameDate,
       g.GameName,
       g.Provider,
       dag.RealBetAmount,
       dag.NetGamingRevenue
FROM [DailyActionsDB].[common].[tbl_Daily_actions_games] dag WITH (NOLOCK)
INNER JOIN dbo.Games g WITH (NOLOCK) ON g.GameID = dag.GameID - 1000000
WHERE dag.GameDate >= DATEADD(day, -30, GETDATE())
  AND g.Provider = 'NetEnt'
ORDER BY dag.GameDate DESC;

PRINT '=== End of NetEnt Game Data Diagnostics ===';
