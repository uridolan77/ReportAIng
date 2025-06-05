# Context-Aware Query Classification & Intelligent Prompt Engineering

## Overview

The BI Reporting Copilot now includes enhanced context-aware query classification that automatically detects the intent of user queries and includes the most relevant database tables and business context in AI prompts. This significantly improves SQL generation accuracy, especially for game-related analytics.

## Key Features

### 1. Game Query Detection
The system automatically identifies game-related queries using an extensive keyword library:

**Game Keywords:**
- Basic: game, games, gaming, provider, providers, slot, slots, casino
- Providers: netent, microgaming, pragmatic, evolution, playtech, yggdrasil
- Game Types: gametype, table games, live casino, sports betting, virtual, scratch
- Metrics: realbetamount, realwinamount, netgamingrevenue, numberofsessions

### 2. Intelligent Table Selection
Based on query intent, the system automatically selects the most relevant tables:

**For Game Queries:**
- Primary: `tbl_Daily_actions_games` (game-specific metrics)
- Secondary: `Games` (game metadata - names, providers, types)
- Supporting: Main daily actions table (lower priority)

**For Financial Queries:**
- Primary: `tbl_Daily_actions` (main financial metrics)
- Supporting: Player, country, currency tables as needed

**For Player Queries:**
- Primary: `tbl_Daily_actions` + `tbl_Daily_actions_players`
- Supporting: Geographic and demographic tables

### 3. Enhanced Business Rules
The system includes specific business rules for different query types:

**Game Analytics Rules:**
```
- Use tbl_Daily_actions_games as primary table for game analytics
- Join with Games table using: g.GameID = dag.GameID - 1000000
- Games table contains: GameName, Provider, SubProvider, GameType
- Gaming metrics: RealBetAmount, RealWinAmount, BonusBetAmount, BonusWinAmount
- NetGamingRevenue = total bets - total wins (house edge)
- GROUP BY game attributes like GameName, Provider, GameType for analysis
```

**Financial Rules:**
```
- For deposits, ALWAYS use 'Deposits' column, NEVER 'Amount'
- Use SUM() for aggregations with proper GROUP BY
- Filter by Date for time-based analysis
```

### 4. Game-Specific SQL Examples
The system provides relevant SQL examples based on query intent:

**Game Performance by Provider:**
```sql
SELECT g.Provider, g.SubProvider, g.GameType,
       SUM(dag.RealBetAmount) AS RealBetAmount,
       SUM(dag.RealWinAmount) AS RealWinAmount,
       SUM(dag.NetGamingRevenue) AS NetGamingRevenue,
       SUM(dag.NumberofRealBets) AS NumberofRealBets
FROM [DailyActionsDB].[common].[tbl_Daily_actions_games] dag WITH (NOLOCK)
INNER JOIN dbo.Games g WITH (NOLOCK) ON g.GameID = dag.GameID - 1000000
WHERE dag.GameDate >= '2025-01-01'
GROUP BY g.Provider, g.SubProvider, g.GameType
ORDER BY NetGamingRevenue DESC
```

**Top Games by Revenue:**
```sql
SELECT TOP 10 g.GameName, g.Provider, g.GameType,
       SUM(dag.RealBetAmount) AS RealBetAmount,
       SUM(dag.RealWinAmount) AS RealWinAmount,
       SUM(dag.NetGamingRevenue) AS NetGamingRevenue
FROM [DailyActionsDB].[common].[tbl_Daily_actions_games] dag WITH (NOLOCK)
INNER JOIN dbo.Games g WITH (NOLOCK) ON g.GameID = dag.GameID - 1000000
WHERE dag.GameDate >= DATEADD(day, -7, CAST(GETDATE() AS DATE))
GROUP BY g.GameName, g.Provider, g.GameType
ORDER BY NetGamingRevenue DESC
```

## Implementation Details

### Table Relevance Scoring
The system uses a sophisticated scoring algorithm to determine table relevance:

1. **Base Scores:**
   - Main daily actions: 0.9
   - Game daily actions: 0.7 (higher for game queries)
   - Games master: 0.5 (higher for game queries)
   - Player demographics: 0.6
   - Lookup tables: 0.4

2. **Query-Specific Boosts:**
   - Game queries get +0.9 for game tables
   - Deposit queries get +0.8 for main table
   - Player queries get +0.6 for player tables

3. **Penalties:**
   - Game tables get -0.5 for non-game queries
   - Bonus tables get -0.8 for non-bonus queries

### Enhanced Prompt Templates
The SQL generation template now includes game-specific instructions:

```
CRITICAL: For game analytics, use tbl_Daily_actions_games and join with Games table
CRITICAL: Games table join: g.GameID = dag.GameID - 1000000 (subtract 1 million)
For game queries: GROUP BY game attributes (GameName, Provider, GameType) for analysis
```

## Usage Examples

### Example 1: Game Performance Query
**User Input:** "Show me NetEnt slot performance this month"

**System Response:**
- Detects: Game-related query (keywords: NetEnt, slot, performance)
- Selects Tables: tbl_Daily_actions_games, Games
- Applies Rules: Game analytics rules, provider filtering
- Generates: SQL with proper game table joins and NetEnt filtering

### Example 2: Mixed Query
**User Input:** "Top players by game revenue today"

**System Response:**
- Detects: Game + Player query
- Selects Tables: tbl_Daily_actions_games, Games, tbl_Daily_actions_players
- Applies Rules: Game analytics + player demographics rules
- Generates: SQL with game revenue aggregation by player

### Example 3: Financial Query
**User Input:** "Show me deposits by brand today"

**System Response:**
- Detects: Financial query (no game keywords)
- Selects Tables: tbl_Daily_actions, whitelabels
- Applies Rules: Financial rules, brand analysis
- Generates: SQL using Deposits column with brand grouping

## Benefits

1. **Improved Accuracy:** Automatic selection of relevant tables reduces errors
2. **Better Context:** Game-specific business rules improve SQL quality
3. **Reduced Tokens:** Only relevant schema information is included in prompts
4. **Faster Response:** Optimized prompts lead to faster AI processing
5. **Domain Expertise:** Built-in gaming industry knowledge

## Future Enhancements

1. **Machine Learning:** Train models on successful query patterns
2. **User Preferences:** Learn individual user query patterns
3. **Advanced Relationships:** Automatic join path detection
4. **Performance Optimization:** Query execution time prediction
5. **Semantic Understanding:** Natural language entity extraction

## Configuration

The system can be configured through:
- Business table information in the database
- Query patterns and examples
- Business glossary terms
- Prompt templates

All configurations are stored in the database and can be updated through the admin interface.
