# Live Demo Walkthrough - Enhanced Game Query Classification

This document provides a step-by-step walkthrough of how the enhanced context-aware query classification system processes different types of queries.

## Demo Query 1: "Show me top NetEnt games by revenue this month"

### Step 1: Query Reception
```
User Input: "Show me top NetEnt games by revenue this month"
System: Received query for processing
```

### Step 2: Game Detection (PromptManagementService.cs)
```csharp
// In GetRelevantSchemaAsync method
var lowerQuery = query.ToLowerInvariant(); // "show me top netent games by revenue this month"

// IsGameRelatedQuery check
var gameKeywords = ["game", "games", "gaming", "provider", "providers", "slot", "slots", "casino", "netent", ...];
// Finds: "netent" and "games" 
// Result: IsGameRelatedQuery = TRUE ✅
```

### Step 3: Table Relevance Calculation
```csharp
// For tbl_Daily_actions_games:
double score = 0.7; // Base score for game daily actions
if (IsGameRelatedQuery(lowerQuery)) {
    score += 0.9; // Major boost for game queries
}
// Final score: 1.6 → capped at 1.0 ✅

// For Games table:
double score = 0.5; // Base score for games master
if (IsGameRelatedQuery(lowerQuery)) {
    score += 0.8; // High boost for games master table
}
// Final score: 1.3 → capped at 1.0 ✅

// For tbl_Daily_actions (main table):
double score = 0.9; // High base score
if (IsGameRelatedQuery(lowerQuery)) {
    score += 0.3; // Lower priority for main table in game queries
}
// Final score: 1.2 → capped at 1.0 ✅
```

### Step 4: Table Selection Result
```
Selected Tables (in priority order):
1. tbl_Daily_actions_games (score: 1.0)
2. Games (score: 1.0) 
3. tbl_Daily_actions (score: 1.0)
4. tbl_Daily_actions_players (score: 0.6)
5. tbl_Countries (score: 0.4)
```

### Step 5: Business Rules Generation (PromptService.cs)
```csharp
// In GetBusinessRulesForQuery method
if (IsGameRelatedQuery(query)) {
    rules.Add("CRITICAL: For game analytics, use tbl_Daily_actions_games as the primary table");
    rules.Add("CRITICAL: Join with Games table using: g.GameID = dag.GameID - 1000000");
    rules.Add("Games table contains: GameName, Provider, SubProvider, GameType");
    rules.Add("Gaming metrics: RealBetAmount, RealWinAmount, BonusBetAmount, BonusWinAmount");
    rules.Add("NetGamingRevenue = total bets - total wins (house edge calculation)");
    rules.Add("For game analysis: GROUP BY g.GameName, g.Provider, g.SubProvider, g.GameType");
    // ... additional game-specific rules
}
```

### Step 6: Example Queries Inclusion
```csharp
// In GetRelevantExampleQueries method
if (IsGameRelatedQuery(query)) {
    examples.Add(@"
EXAMPLE: 'top games by revenue'
SQL: SELECT TOP 10 g.GameName, g.Provider, g.GameType,
            SUM(dag.RealBetAmount) AS TotalBets,
            SUM(dag.RealWinAmount) AS TotalWins,
            SUM(dag.NetGamingRevenue) AS NetRevenue
     FROM [DailyActionsDB].[common].[tbl_Daily_actions_games] dag WITH (NOLOCK)
     INNER JOIN dbo.Games g WITH (NOLOCK) ON g.GameID = dag.GameID - 1000000
     WHERE dag.GameDate >= DATEADD(day, -7, CAST(GETDATE() AS DATE))
     GROUP BY g.GameName, g.Provider, g.GameType
     ORDER BY NetRevenue DESC");
}
```

### Step 7: Final Prompt Assembly
```
Generated Prompt Contains:
✅ Game-specific table schema (tbl_Daily_actions_games, Games)
✅ Game business rules and join logic
✅ Gaming metrics descriptions
✅ Relevant SQL examples
✅ Provider-specific context (NetEnt)
✅ Revenue calculation guidance

Prompt Length: ~2,500 characters (optimized for game context)
```

### Step 8: Expected AI-Generated SQL
```sql
SELECT TOP 10 g.GameName, g.Provider, g.GameType,
       SUM(dag.RealBetAmount) AS TotalBets,
       SUM(dag.RealWinAmount) AS TotalWins,
       SUM(dag.NetGamingRevenue) AS NetRevenue,
       SUM(dag.NumberofSessions) AS TotalSessions
FROM [DailyActionsDB].[common].[tbl_Daily_actions_games] dag WITH (NOLOCK)
INNER JOIN dbo.Games g WITH (NOLOCK) ON g.GameID = dag.GameID - 1000000
WHERE dag.GameDate >= DATEADD(month, -1, CAST(GETDATE() AS DATE))
  AND g.Provider = 'NetEnt'
GROUP BY g.GameName, g.Provider, g.GameType
ORDER BY NetRevenue DESC
```

---

## Demo Query 2: "Show me deposits by brand today" (Control Test)

### Step 1: Query Reception
```
User Input: "Show me deposits by brand today"
System: Received query for processing
```

### Step 2: Game Detection
```csharp
var lowerQuery = "show me deposits by brand today";
// IsGameRelatedQuery check
// No game keywords found: "deposits", "brand", "today" are not in game keywords
// Result: IsGameRelatedQuery = FALSE ✅
```

### Step 3: Table Relevance Calculation
```csharp
// For tbl_Daily_actions:
double score = 0.9; // High base score for main table
if (lowerQuery.Contains("deposit")) {
    score += 0.8; // Major boost for deposit queries
}
// Final score: 1.7 → capped at 1.0 ✅

// For tbl_Daily_actions_games:
double score = 0.7; // Base score
if (!IsGameRelatedQuery(lowerQuery)) {
    score -= 0.5; // Penalty for game tables in non-game queries
}
// Final score: 0.2 (low priority) ✅

// For whitelabels:
if (lowerQuery.Contains("brand")) {
    score += 0.6; // Boost for brand queries
}
// Final score: 0.6 ✅
```

### Step 4: Table Selection Result
```
Selected Tables (in priority order):
1. tbl_Daily_actions (score: 1.0) ✅ Main table prioritized
2. whitelabels (score: 0.6) ✅ Brand context
3. tbl_Daily_actions_players (score: 0.6)
4. tbl_Countries (score: 0.4)
5. tbl_Daily_actions_games (score: 0.2) ✅ Deprioritized for non-game query
```

### Step 5: Business Rules Generation
```csharp
// NO game-specific rules added since IsGameRelatedQuery = false
// Standard financial rules applied:
rules.Add("CRITICAL: For deposit queries, ALWAYS use 'Deposits' column, NEVER 'Amount'");
rules.Add("For deposit totals: SUM(da.Deposits) - use this exact syntax");
rules.Add("For brand analysis: JOIN with whitelabels table using WhitelabelID");
```

### Step 6: Expected AI-Generated SQL
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

---

## Key Differences Demonstrated

### Game Query Processing:
- ✅ **Smart Detection**: Automatically identifies game-related keywords
- ✅ **Relevant Tables**: Prioritizes `tbl_Daily_actions_games` and `Games` tables
- ✅ **Gaming Context**: Includes gaming metrics, join logic, and provider information
- ✅ **Specialized Rules**: Game-specific business rules and SQL patterns

### Financial Query Processing:
- ✅ **Standard Detection**: Recognizes financial keywords (deposits, brand)
- ✅ **Appropriate Tables**: Prioritizes main financial table and brand lookup
- ✅ **Financial Context**: Focuses on deposits column and brand analysis
- ✅ **Standard Rules**: Traditional financial business rules

## Performance Benefits

### Before Enhancement:
```
All Queries → Same Generic Prompt → Inconsistent Results
- Generic table selection
- No domain-specific context
- Larger, unfocused prompts
- Lower accuracy for specialized queries
```

### After Enhancement:
```
Game Queries → Game-Optimized Prompt → Accurate Gaming SQL
Financial Queries → Finance-Optimized Prompt → Accurate Financial SQL
- Intelligent table selection
- Domain-specific context
- Focused, smaller prompts
- Higher accuracy for all query types
```

## Real-World Impact

1. **Improved Accuracy**: Game queries now automatically include the correct tables and joins
2. **Better Performance**: Smaller, focused prompts reduce AI processing time
3. **Domain Expertise**: Built-in gaming industry knowledge
4. **User Experience**: More accurate results without requiring technical SQL knowledge
5. **Scalability**: Easy to extend with additional domain-specific rules

The enhanced system transforms a generic SQL generation tool into an intelligent, context-aware business intelligence assistant that understands the nuances of gaming analytics.
