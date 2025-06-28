# UK Mapping Fix - Test Guide

## Issue Fixed

**Problem**: The Enhanced Schema Contextualization System was incorrectly mapping "UK" queries to:
- ❌ `tbl_Countries.CountryIntlCode = 'GB'` 
- ❌ Joining `tbl_Daily_actionsGBP_transactions` table
- ❌ Using currency filtering instead of country filtering

**Root Cause**: Incorrect entity mappings in `SchemaEntityLinker.cs`

**Solution**: Updated entity mappings to correctly map "UK" to:
- ✅ `tbl_Daily_actions_players.Country = 'UK'`
- ✅ Proper table relationships for player demographics
- ✅ Country-based filtering instead of currency-based

## Changes Made

### File: `backend/BIReportingCopilot.Infrastructure/BusinessContext/Enhanced/SchemaEntityLinker.cs`

**Before (Incorrect):**
```csharp
{ "uk", new("tbl_Countries", "CountryIntlCode", "GB") },
{ "united kingdom", new("tbl_Countries", "CountryIntlCode", "GB") },
{ "gb", new("tbl_Countries", "CountryIntlCode", "GB") },
{ "britain", new("tbl_Countries", "CountryIntlCode", "GB") },
{ "england", new("tbl_Countries", "CountryIntlCode", "GB") },
{ "from uk", new("tbl_Countries", "CountryIntlCode", "GB") },
```

**After (Correct):**
```csharp
{ "uk", new("tbl_Daily_actions_players", "Country", "UK") },
{ "united kingdom", new("tbl_Daily_actions_players", "Country", "United Kingdom") },
{ "gb", new("tbl_Daily_actions_players", "Country", "UK") },
{ "britain", new("tbl_Daily_actions_players", "Country", "UK") },
{ "england", new("tbl_Daily_actions_players", "Country", "UK") },
{ "from uk", new("tbl_Daily_actions_players", "Country", "UK") },
```

## Expected SQL Improvement

### Before Fix (Incorrect SQL):
```sql
SELECT TOP 10 da.PlayerID, SUM(da.Deposits) AS TotalDeposits 
FROM tbl_Daily_actions AS da 
JOIN tbl_Daily_actions_players AS dap ON da.PlayerID = dap.PlayerID 
JOIN tbl_Daily_actionsGBP_transactions AS dat ON da.PlayerID = dat.PlayerID 
WHERE da.Date = DATEADD(day, -1, GETDATE()) 
    AND dat.CurrencyCode = 'GBP'  -- ❌ Wrong: Using currency instead of country
GROUP BY da.PlayerID 
ORDER BY TotalDeposits DESC;
```

### After Fix (Expected Correct SQL):
```sql
SELECT TOP 10
    dap.Alias as PlayerName,
    dap.Country,
    da.Deposits as DepositAmount,
    dap.Currency,
    da.Date as DepositDate
FROM tbl_Daily_actions da
JOIN tbl_Daily_actions_players dap ON da.PlayerID = dap.PlayerID
WHERE da.Deposits > 0
    AND dap.Country = 'UK'  -- ✅ Correct: Using country filtering
    AND CAST(da.Date AS DATE) = CAST(DATEADD(day, -1, GETDATE()) AS DATE)
ORDER BY da.Deposits DESC
```

## Test Cases

### Test 1: Basic UK Query
**Query**: "Top 10 depositors from UK yesterday"

**Expected Behavior**:
- ✅ Maps "UK" to `tbl_Daily_actions_players.Country = 'UK'`
- ✅ Includes `tbl_Daily_actions` and `tbl_Daily_actions_players` tables
- ✅ Uses proper JOIN on `PlayerID`
- ✅ Filters by `dap.Country = 'UK'`
- ✅ No incorrect currency table joins

### Test 2: Alternative UK Terms
**Queries**:
- "Show depositors from United Kingdom"
- "Players from Britain yesterday"
- "Top depositors from England"

**Expected Behavior**:
- ✅ All variations map to `tbl_Daily_actions_players.Country`
- ✅ Consistent table selection and filtering logic

### Test 3: Currency vs Country Disambiguation
**Query**: "UK depositors with GBP currency"

**Expected Behavior**:
- ✅ "UK" maps to `dap.Country = 'UK'`
- ✅ "GBP" maps to currency filtering (separate concern)
- ✅ Both filters applied correctly without confusion

## Testing Instructions

### 1. API Testing
```bash
# Test the query that was failing
curl -X POST "https://localhost:55244/api/query-execution/execute" \
  -H "Authorization: Bearer {jwt_token}" \
  -H "Content-Type: application/json" \
  -d '{
    "question": "Top 10 depositors from UK yesterday",
    "sessionId": "test-uk-fix-001"
  }'
```

### 2. Schema Contextualization Testing
```bash
# Test schema retrieval for UK queries
curl -X POST "https://localhost:55244/api/semantic-layer/analyze-enhanced" \
  -H "Authorization: Bearer {jwt_token}" \
  -H "Content-Type: application/json" \
  -d '{
    "query": "depositors from UK",
    "relevanceThreshold": 0.6,
    "maxTables": 5
  }'
```

### 3. Entity Linking Testing
```bash
# Test entity extraction and linking
curl -X POST "https://localhost:55244/api/semantic-layer/extract-entities" \
  -H "Authorization: Bearer {jwt_token}" \
  -H "Content-Type: application/json" \
  -d '{
    "query": "UK depositors yesterday",
    "includeSemanticMapping": true
  }'
```

## Validation Checklist

- [ ] "UK" entity maps to `tbl_Daily_actions_players` table
- [ ] Country filtering uses `Country` column, not `CountryIntlCode`
- [ ] No incorrect joins to `tbl_Daily_actionsGBP_transactions`
- [ ] No currency-based filtering for country queries
- [ ] Proper table relationships maintained
- [ ] Alternative UK terms (Britain, England) work correctly
- [ ] Generated SQL matches expected pattern from testing plan
- [ ] Business context confidence scores improve
- [ ] Schema retrieval includes correct tables

## Success Criteria

1. **Correct Table Selection**: Query retrieves `tbl_Daily_actions` and `tbl_Daily_actions_players`
2. **Proper Filtering**: Uses `dap.Country = 'UK'` instead of currency filtering
3. **No Wrong Joins**: Eliminates incorrect `tbl_Daily_actionsGBP_transactions` joins
4. **Improved Confidence**: Business context confidence scores >70%
5. **SQL Quality**: Generated SQL matches testing plan expectations

## Impact

This fix addresses the core issue causing incorrect SQL generation for geographic queries, specifically:

- ✅ Fixes "Top 10 depositors from UK yesterday" query
- ✅ Improves all country-based filtering queries
- ✅ Eliminates currency/country confusion in entity mapping
- ✅ Aligns with actual database schema structure
- ✅ Supports Enhanced Schema Contextualization System goals

The fix ensures that the AI receives correct guidance about table relationships and filtering logic for geographic queries, leading to production-quality SQL generation that matches the expected patterns in the testing plan.
