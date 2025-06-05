# Game Query Examples - Context-Aware Query Classification

This document demonstrates how the enhanced context-aware query classification system automatically detects game-related queries and generates appropriate SQL with the correct tables and business context.

## Example 1: Game Performance by Provider

### User Query:
```
"Show me NetEnt slot performance this month"
```

### System Detection:
- **Keywords Detected:** NetEnt (provider), slot (game type), performance (analytics)
- **Query Classification:** Game-related query
- **Tables Selected:** 
  - Primary: `tbl_Daily_actions_games`
  - Secondary: `Games` (for provider and game metadata)
- **Business Rules Applied:** Game analytics rules, provider filtering

### Generated SQL:
```sql
SELECT TOP 100 g.GameName, g.Provider, g.SubProvider, g.GameType,
       SUM(dag.RealBetAmount) AS RealBetAmount,
       SUM(dag.RealWinAmount) AS RealWinAmount,
       SUM(dag.BonusBetAmount) AS BonusBetAmount,
       SUM(dag.BonusWinAmount) AS BonusWinAmount,
       SUM(dag.NetGamingRevenue) AS NetGamingRevenue,
       SUM(dag.NumberofRealBets) AS NumberofRealBets,
       SUM(dag.NumberofSessions) AS NumberofSessions
FROM [DailyActionsDB].[common].[tbl_Daily_actions_games] dag WITH (NOLOCK)
INNER JOIN dbo.Games g WITH (NOLOCK) ON g.GameID = dag.GameID - 1000000
WHERE dag.GameDate >= DATEADD(month, -1, CAST(GETDATE() AS DATE))
  AND g.Provider = 'NetEnt'
  AND g.GameType LIKE '%slot%'
GROUP BY g.GameName, g.Provider, g.SubProvider, g.GameType
ORDER BY NetGamingRevenue DESC
```

## Example 2: Top Games by Revenue

### User Query:
```
"What are the top 10 games by revenue this week?"
```

### System Detection:
- **Keywords Detected:** games, revenue (gaming metrics), top (ranking)
- **Query Classification:** Game-related query with ranking
- **Tables Selected:**
  - Primary: `tbl_Daily_actions_games`
  - Secondary: `Games`
- **Business Rules Applied:** Game analytics, revenue calculation, TOP clause

### Generated SQL:
```sql
SELECT TOP 10 g.GameName, g.Provider, g.GameType,
       SUM(dag.RealBetAmount) AS TotalBets,
       SUM(dag.RealWinAmount) AS TotalWins,
       SUM(dag.NetGamingRevenue) AS NetRevenue,
       SUM(dag.NumberofSessions) AS TotalSessions
FROM [DailyActionsDB].[common].[tbl_Daily_actions_games] dag WITH (NOLOCK)
INNER JOIN dbo.Games g WITH (NOLOCK) ON g.GameID = dag.GameID - 1000000
WHERE dag.GameDate >= DATEADD(day, -7, CAST(GETDATE() AS DATE))
GROUP BY g.GameName, g.Provider, g.GameType
ORDER BY NetRevenue DESC
```

## Example 3: Provider Performance Comparison

### User Query:
```
"Compare gaming revenue between Microgaming and Pragmatic Play providers"
```

### System Detection:
- **Keywords Detected:** gaming, revenue, Microgaming, Pragmatic Play (providers)
- **Query Classification:** Game-related comparative analysis
- **Tables Selected:**
  - Primary: `tbl_Daily_actions_games`
  - Secondary: `Games`
- **Business Rules Applied:** Provider comparison, revenue aggregation

### Generated SQL:
```sql
SELECT g.Provider,
       SUM(dag.RealBetAmount) AS TotalBets,
       SUM(dag.RealWinAmount) AS TotalWins,
       SUM(dag.NetGamingRevenue) AS NetRevenue,
       COUNT(DISTINCT g.GameID) AS NumberOfGames,
       SUM(dag.NumberofSessions) AS TotalSessions
FROM [DailyActionsDB].[common].[tbl_Daily_actions_games] dag WITH (NOLOCK)
INNER JOIN dbo.Games g WITH (NOLOCK) ON g.GameID = dag.GameID - 1000000
WHERE g.Provider IN ('Microgaming', 'Pragmatic Play')
  AND dag.GameDate >= DATEADD(day, -30, CAST(GETDATE() AS DATE))
GROUP BY g.Provider
ORDER BY NetRevenue DESC
```

## Example 4: Game Type Analysis

### User Query:
```
"Show me casino game performance by game type today"
```

### System Detection:
- **Keywords Detected:** casino, game, performance, game type
- **Query Classification:** Game-related analysis by category
- **Tables Selected:**
  - Primary: `tbl_Daily_actions_games`
  - Secondary: `Games`
- **Business Rules Applied:** Game type grouping, today's date filter

### Generated SQL:
```sql
SELECT g.GameType,
       COUNT(DISTINCT g.GameID) AS NumberOfGames,
       SUM(dag.RealBetAmount) AS TotalBets,
       SUM(dag.RealWinAmount) AS TotalWins,
       SUM(dag.NetGamingRevenue) AS NetRevenue,
       SUM(dag.NumberofRealBets) AS TotalBetCount,
       SUM(dag.NumberofSessions) AS TotalSessions,
       AVG(dag.NetGamingRevenue) AS AvgRevenuePerGame
FROM [DailyActionsDB].[common].[tbl_Daily_actions_games] dag WITH (NOLOCK)
INNER JOIN dbo.Games g WITH (NOLOCK) ON g.GameID = dag.GameID - 1000000
WHERE dag.GameDate = CAST(GETDATE() AS DATE)
GROUP BY g.GameType
ORDER BY NetRevenue DESC
```

## Example 5: Player Game Activity

### User Query:
```
"Show me top players by game sessions this month"
```

### System Detection:
- **Keywords Detected:** players, game, sessions (gaming metrics)
- **Query Classification:** Game + Player analysis
- **Tables Selected:**
  - Primary: `tbl_Daily_actions_games`
  - Secondary: `Games`, `tbl_Daily_actions_players`
- **Business Rules Applied:** Player demographics, game sessions aggregation

### Generated SQL:
```sql
SELECT TOP 100 dag.PlayerID, p.Username,
       COUNT(DISTINCT g.GameID) AS GamesPlayed,
       SUM(dag.NumberofSessions) AS TotalSessions,
       SUM(dag.RealBetAmount) AS TotalBets,
       SUM(dag.NetGamingRevenue) AS NetRevenue
FROM [DailyActionsDB].[common].[tbl_Daily_actions_games] dag WITH (NOLOCK)
INNER JOIN dbo.Games g WITH (NOLOCK) ON g.GameID = dag.GameID - 1000000
INNER JOIN [DailyActionsDB].[common].[tbl_Daily_actions_players] p WITH (NOLOCK) ON dag.PlayerID = p.PlayerID
WHERE dag.GameDate >= DATEADD(month, -1, CAST(GETDATE() AS DATE))
GROUP BY dag.PlayerID, p.Username
ORDER BY TotalSessions DESC
```

## Contrast: Non-Game Query

### User Query:
```
"Show me deposits by brand today"
```

### System Detection:
- **Keywords Detected:** deposits, brand (no game keywords)
- **Query Classification:** Financial query (NOT game-related)
- **Tables Selected:**
  - Primary: `tbl_Daily_actions` (main financial table)
  - Secondary: `whitelabels` (for brand information)
- **Business Rules Applied:** Financial rules, deposit column usage

### Generated SQL:
```sql
SELECT da.WhiteLabelID, w.Name as BrandName,
       SUM(da.Deposits) as TotalDeposits,
       COUNT(DISTINCT da.PlayerID) as PlayerCount,
       AVG(da.Deposits) as AvgDepositPerPlayer
FROM [DailyActionsDB].[common].[tbl_Daily_actions] da WITH (NOLOCK)
INNER JOIN [DailyActionsDB].[common].[whitelabels] w WITH (NOLOCK) ON da.WhiteLabelID = w.WhiteLabelID
WHERE da.Date = CAST(GETDATE() AS DATE)
  AND da.Deposits > 0
GROUP BY da.WhiteLabelID, w.Name
ORDER BY TotalDeposits DESC
```

## Key Benefits Demonstrated

1. **Automatic Table Selection:** Game queries automatically include game-specific tables
2. **Correct Join Logic:** Proper GameID transformation (GameID - 1000000)
3. **Relevant Metrics:** Game-specific columns like RealBetAmount, NetGamingRevenue
4. **Business Context:** Provider names, game types, and gaming terminology
5. **Performance Optimization:** NOLOCK hints and appropriate indexing
6. **Query Classification:** Clear distinction between game and non-game queries

## Technical Implementation

The system uses:
- **Keyword Detection:** Extensive gaming vocabulary recognition
- **Table Relevance Scoring:** Dynamic scoring based on query intent
- **Business Rules Engine:** Context-specific SQL generation rules
- **Template System:** Game-specific prompt templates and examples
- **Learning Capability:** Continuous improvement based on successful queries

This intelligent classification ensures that users get accurate, optimized SQL queries regardless of their technical SQL knowledge, while maintaining the business context and domain expertise required for gaming analytics.
