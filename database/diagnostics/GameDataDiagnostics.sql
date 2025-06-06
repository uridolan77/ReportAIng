-- =============================================
-- Game Data Diagnostics
-- Troubleshooting empty results for NetEnt games query
-- =============================================

-- 1. Check if Games table has any NetEnt games
SELECT 'Games Table - NetEnt Count' as CheckType, COUNT(*) as RecordCount
FROM dbo.Games g WITH (NOLOCK)
WHERE g.Provider = 'NetEnt';

-- 2. Check all providers in Games table
SELECT 'Games Table - All Providers' as CheckType, g.Provider, COUNT(*) as GameCount
FROM dbo.Games g WITH (NOLOCK)
GROUP BY g.Provider
ORDER BY GameCount DESC;

-- 3. Check GameID ranges in Games table
SELECT 'Games Table - GameID Range' as CheckType, 
       MIN(GameID) as MinGameID, 
       MAX(GameID) as MaxGameID,
       COUNT(*) as TotalGames
FROM dbo.Games g WITH (NOLOCK);

-- 4. Check GameID ranges in tbl_Daily_actions_games for this month
SELECT 'Daily Actions Games - GameID Range This Month' as CheckType,
       MIN(GameID) as MinGameID, 
       MAX(GameID) as MaxGameID,
       COUNT(DISTINCT GameID) as UniqueGames,
       COUNT(*) as TotalRecords
FROM [DailyActionsDB].[common].[tbl_Daily_actions_games] dag WITH (NOLOCK)
WHERE dag.GameDate >= DATEADD(MONTH, DATEDIFF(MONTH, 0, GETDATE()), 0)
  AND dag.GameDate < DATEADD(MONTH, DATEDIFF(MONTH, 0, GETDATE()) + 1, 0);

-- 5. Check if there are any records in tbl_Daily_actions_games for this month
SELECT 'Daily Actions Games - This Month Count' as CheckType, COUNT(*) as RecordCount
FROM [DailyActionsDB].[common].[tbl_Daily_actions_games] dag WITH (NOLOCK)
WHERE dag.GameDate >= DATEADD(MONTH, DATEDIFF(MONTH, 0, GETDATE()), 0)
  AND dag.GameDate < DATEADD(MONTH, DATEDIFF(MONTH, 0, GETDATE()) + 1, 0);

-- 6. Test the join condition with sample data
SELECT 'Join Test - Sample Results' as CheckType,
       dag.GameID as DagGameID,
       dag.GameID - 1000000 as CalculatedGameID,
       g.GameID as GamesTableGameID,
       g.GameName,
       g.Provider
FROM [DailyActionsDB].[common].[tbl_Daily_actions_games] dag WITH (NOLOCK)
INNER JOIN dbo.Games g WITH (NOLOCK) ON g.GameID = dag.GameID - 1000000
WHERE dag.GameDate >= DATEADD(MONTH, DATEDIFF(MONTH, 0, GETDATE()), 0)
  AND dag.GameDate < DATEADD(MONTH, DATEDIFF(MONTH, 0, GETDATE()) + 1, 0)
ORDER BY dag.GameID
OFFSET 0 ROWS FETCH NEXT 10 ROWS ONLY;

-- 7. Check for NetEnt games with the join (without date filter first)
SELECT 'NetEnt Games Join Test' as CheckType,
       COUNT(*) as MatchingRecords
FROM [DailyActionsDB].[common].[tbl_Daily_actions_games] dag WITH (NOLOCK)
INNER JOIN dbo.Games g WITH (NOLOCK) ON g.GameID = dag.GameID - 1000000
WHERE g.Provider = 'NetEnt';

-- 8. Alternative join test (in case the offset is different)
SELECT 'Alternative Join Test' as CheckType,
       COUNT(*) as MatchingRecords
FROM [DailyActionsDB].[common].[tbl_Daily_actions_games] dag WITH (NOLOCK)
INNER JOIN dbo.Games g WITH (NOLOCK) ON g.GameID = dag.GameID
WHERE dag.GameDate >= DATEADD(MONTH, DATEDIFF(MONTH, 0, GETDATE()), 0)
  AND dag.GameDate < DATEADD(MONTH, DATEDIFF(MONTH, 0, GETDATE()) + 1, 0);

-- 9. Check recent dates in tbl_Daily_actions_games
SELECT 'Recent Dates in Daily Actions Games' as CheckType,
       MAX(GameDate) as LatestDate,
       MIN(GameDate) as EarliestDate,
       COUNT(DISTINCT GameDate) as UniqueDates
FROM [DailyActionsDB].[common].[tbl_Daily_actions_games] dag WITH (NOLOCK);

-- 10. Check current month date range
SELECT 'Current Month Range' as CheckType,
       DATEADD(MONTH, DATEDIFF(MONTH, 0, GETDATE()), 0) as MonthStart,
       DATEADD(MONTH, DATEDIFF(MONTH, 0, GETDATE()) + 1, 0) as MonthEnd,
       GETDATE() as CurrentDate;
