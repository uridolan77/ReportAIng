# BI Reporting Copilot - Query Processing Pipeline Testing Guide

## Table of Contents
1. [Prerequisites and Setup](#prerequisites-and-setup)
2. [Business Metadata Preparation](#business-metadata-preparation)
3. [Enhanced Schema Contextualization System Testing](#enhanced-schema-contextualization-system-testing)
4. [Query Processing Pipeline Validation](#query-processing-pipeline-validation)
5. [AI Generation and Validation](#ai-generation-and-validation)
6. [End-to-End Chat Testing](#end-to-end-chat-testing)
7. [Troubleshooting and Enhancement](#troubleshooting-and-enhancement)

---

## Prerequisites and Setup

### 1. Database Connection Verification

#### Step 1.1: Verify BIDatabase Connection
```bash
# Start the server
dotnet run --project backend/BIReportingCopilot.API

# Test health endpoint
curl http://localhost:55244/health
```

**Expected Response:**
```json
{
  "status": "Healthy",
  "checks": [
    {
      "name": "bidatabase",
      "status": "Healthy",
      "description": "BIDatabase connection successful"
    }
  ]
}
```

#### Step 1.2: Validate Connection String
Ensure `appsettings.Development.json` contains:
```json
{
  "ConnectionStrings": {
    "BIDatabase": "data source=185.64.56.157;initial catalog=DailyActionsDB;persist security info=True;user id=ReportsUser;password=Rep%83%us!;TrustServerCertificate=True;Encrypt=True;"
  }
}
```

### 2. Required Business Metadata Tables

#### Step 2.1: Verify Table Existence
```sql
-- Check if business metadata tables exist
SELECT TABLE_NAME 
FROM INFORMATION_SCHEMA.TABLES 
WHERE TABLE_NAME IN ('BusinessTableInfo', 'BusinessColumnInfo', 'BusinessGlossary')
```

#### Step 2.2: Check Data Population
```sql
-- Verify BusinessTableInfo has data
SELECT COUNT(*) as TableCount FROM [BIReportingCopilot_Dev].[dbo].[BusinessTableInfo];

-- Verify BusinessColumnInfo has data
SELECT COUNT(*) as ColumnCount FROM [BIReportingCopilot_Dev].[dbo].[BusinessColumnInfo];

-- Verify BusinessGlossary has data
SELECT COUNT(*) as GlossaryCount FROM [BIReportingCopilot_Dev].[dbo].[BusinessGlossary];
```

### 3. Server Startup Validation

#### Step 3.1: Check Server Logs
Monitor startup logs for:
- ✅ Database initialization completed successfully
- ✅ Health checks configured
- ✅ All bounded contexts are healthy
- ❌ No login failures or connection errors

#### Step 3.2: Validate API Endpoints
```bash
# Test schema endpoint
curl http://localhost:55244/api/schema/tables

# Test business metadata endpoint
curl http://localhost:55244/api/business-metadata/tables
```

---

## Business Metadata Preparation

### 1. BusinessColumnInfo Enhancement

#### Step 1.1: Identify Empty Fields
```sql
-- Find columns with empty optional fields
SELECT 
    TableName,
    ColumnName,
    CASE WHEN ValueExamples IS NULL OR ValueExamples = '' THEN 'Missing' ELSE 'Present' END as ValueExamples,
    CASE WHEN LLMPromptHints IS NULL OR LLMPromptHints = '' THEN 'Missing' ELSE 'Present' END as LLMPromptHints,
    CASE WHEN SemanticTags IS NULL OR SemanticTags = '' THEN 'Missing' ELSE 'Present' END as SemanticTags,
    CASE WHEN BusinessFriendlyName IS NULL OR BusinessFriendlyName = '' THEN 'Missing' ELSE 'Present' END as BusinessFriendlyName,
    CASE WHEN NaturalLanguageDescription IS NULL OR NaturalLanguageDescription = '' THEN 'Missing' ELSE 'Present' END as NaturalLanguageDescription
FROM [BIReportingCopilot_Dev].[dbo].[BusinessColumnInfo]
WHERE 
    ValueExamples IS NULL OR ValueExamples = '' OR
    LLMPromptHints IS NULL OR LLMPromptHints = '' OR
    SemanticTags IS NULL OR SemanticTags = '' OR
    BusinessFriendlyName IS NULL OR BusinessFriendlyName = '' OR
    NaturalLanguageDescription IS NULL OR NaturalLanguageDescription = ''
ORDER BY TableName, ColumnName;
```

#### Step 1.2: AI-Assisted Metadata Population
```bash
# Use the business metadata enhancement endpoint
curl -X POST http://localhost:55244/api/business-metadata/enhance \
  -H "Content-Type: application/json" \
  -d '{
    "tableName": "tbl_Daily_actions",
    "enhanceEmptyFields": true,
    "useAI": true
  }'
```

#### Step 1.3: Manual Enhancement Examples
```sql
-- Example: Enhance deposit-related columns
UPDATE [BIReportingCopilot_Dev].[dbo].[BusinessColumnInfo]
SET 
    ValueExamples = '100.50, 250.00, 1000.00',
    LLMPromptHints = 'Financial amount in decimal format representing player deposits',
    SemanticTags = 'financial,deposit,amount,currency',
    BusinessFriendlyName = 'Deposit Amount',
    NaturalLanguageDescription = 'The monetary amount deposited by a player in their account'
WHERE TableName = 'tbl_Daily_actions' AND ColumnName = 'deposit_amount';

-- Example: Enhance country-related columns
UPDATE [BIReportingCopilot_Dev].[dbo].[BusinessColumnInfo]
SET 
    ValueExamples = 'UK, US, DE, FR, IT',
    LLMPromptHints = 'Two-letter country code following ISO 3166-1 alpha-2 standard',
    SemanticTags = 'geography,country,location,iso-code',
    BusinessFriendlyName = 'Country Code',
    NaturalLanguageDescription = 'ISO country code representing the player''s location'
WHERE TableName = 'countries' AND ColumnName = 'country_code';
```

### 2. BusinessTableInfo Validation

#### Step 2.1: Check Table Completeness
```sql
-- Verify all important tables are documented
SELECT 
    t.TABLE_NAME,
    CASE WHEN bt.TableName IS NOT NULL THEN 'Documented' ELSE 'Missing' END as BusinessDocumentation
FROM INFORMATION_SCHEMA.TABLES t
LEFT JOIN [BIReportingCopilot_Dev].[dbo].[BusinessTableInfo] bt ON t.TABLE_NAME = bt.TableName
WHERE t.TABLE_TYPE = 'BASE TABLE'
    AND t.TABLE_NAME IN ('tbl_Daily_actions', 'players', 'countries', 'Games')
ORDER BY t.TABLE_NAME;
```

#### Step 2.2: Enhance Table Descriptions
```sql
-- Example: Enhance main tables
UPDATE [BIReportingCopilot_Dev].[dbo].[BusinessTableInfo]
SET 
    BusinessPurpose = 'Core transactional table containing all player daily activities including deposits, withdrawals, and gaming actions',
    DataSources = 'Real-time player activity feeds, payment processors, gaming platforms',
    UpdateFrequency = 'Real-time',
    ImportanceScore = 0.95
WHERE TableName = 'tbl_Daily_actions';
```

### 3. Business Glossary Enhancement

#### Step 3.1: Add Key Business Terms
```sql
-- Insert important business terms
INSERT INTO [BIReportingCopilot_Dev].[dbo].[BusinessGlossary] 
(Term, Definition, Category, RelatedTables, RelatedColumns, BusinessContext)
VALUES 
('Deposit', 'Money transferred by a player into their gaming account', 'Financial', 'tbl_Daily_actions', 'deposit_amount', 'Player funding and financial transactions'),
('Withdrawal', 'Money transferred from a player gaming account to external account', 'Financial', 'tbl_Daily_actions', 'withdrawal_amount', 'Player cashout and financial transactions'),
('Player Lifetime Value', 'Total monetary value a player brings over their entire relationship', 'Analytics', 'tbl_Daily_actions,players', 'deposit_amount,player_id', 'Business intelligence and player analytics');
```

---

## Enhanced Schema Contextualization System Testing

### 1. Schema Retrieval Testing

#### Step 1.1: Test Financial Query Schema Retrieval
```bash
# Test deposit query schema contextualization
curl -X POST http://localhost:55244/api/schema/contextualize \
  -H "Content-Type: application/json" \
  -d '{
    "query": "Top 10 depositors yesterday from UK",
    "includeBusinessMetadata": true,
    "maxTables": 5
  }'
```

**Expected Response Structure:**
```json
{
  "relevantTables": [
    {
      "tableName": "tbl_Daily_actions",
      "relevanceScore": 0.95,
      "reason": "Contains deposit data and transaction dates",
      "columns": [
        {
          "columnName": "deposit_amount",
          "businessFriendlyName": "Deposit Amount",
          "dataType": "decimal(18,2)"
        }
      ]
    },
    {
      "tableName": "players",
      "relevanceScore": 0.85,
      "reason": "Contains player information for joining"
    },
    {
      "tableName": "countries",
      "relevanceScore": 0.80,
      "reason": "Contains country information for UK filtering"
    }
  ],
  "excludedTables": ["Games", "tbl_Daily_actions_games"]
}
```

#### Step 1.2: Validate Gaming Table Exclusion
```bash
# Test that gaming tables are excluded from financial queries
curl -X POST http://localhost:55244/api/schema/contextualize \
  -H "Content-Type: application/json" \
  -d '{
    "query": "Show me total deposits by country last month",
    "includeBusinessMetadata": true
  }'
```

**Validation Points:**
- ✅ `tbl_Daily_actions` should be included (contains deposits)
- ✅ `countries` should be included (for country grouping)
- ❌ `Games` should be excluded (not relevant to deposits)
- ❌ `tbl_Daily_actions_games` should be excluded (gaming-specific)

### 2. Intent-Based Table Filtering

#### Step 2.1: Test Different Query Intents
```bash
# Financial Intent
curl -X POST http://localhost:55244/api/schema/contextualize \
  -H "Content-Type: application/json" \
  -d '{"query": "Revenue analysis by player segment"}'

# Gaming Intent  
curl -X POST http://localhost:55244/api/schema/contextualize \
  -H "Content-Type: application/json" \
  -d '{"query": "Most popular games this week"}'

# Player Analytics Intent
curl -X POST http://localhost:55244/api/schema/contextualize \
  -H "Content-Type: application/json" \
  -d '{"query": "Player retention rates by country"}'
```

### 3. Selective Field Retrieval Testing

#### Step 3.1: Verify Column Selection
```bash
# Test that only relevant columns are returned
curl -X POST http://localhost:55244/api/schema/contextualize \
  -H "Content-Type: application/json" \
  -d '{
    "query": "Average deposit amount by country",
    "includeBusinessMetadata": true,
    "selectiveFields": true
  }'
```

**Expected Behavior:**
- Only deposit-related columns from `tbl_Daily_actions`
- Only country-related columns from `countries`
- Player ID for joining
- Date columns for filtering

---

## Query Processing Pipeline Validation

### 1. Phase-by-Phase Testing

#### Phase 1: Query Analysis and Intent Recognition
```bash
curl -X POST http://localhost:55244/api/chat/analyze-intent \
  -H "Content-Type: application/json" \
  -d '{
    "message": "Show me the top 10 depositors from UK yesterday",
    "sessionId": "test-session-001"
  }'
```

**Expected Response:**
```json
{
  "intent": "financial_analysis",
  "entities": [
    {"type": "metric", "value": "depositors"},
    {"type": "limit", "value": "10"},
    {"type": "country", "value": "UK"},
    {"type": "timeframe", "value": "yesterday"}
  ],
  "queryType": "aggregation",
  "confidence": 0.92
}
```

#### Phase 2: Schema Contextualization
```bash
curl -X POST http://localhost:55244/api/chat/contextualize-schema \
  -H "Content-Type: application/json" \
  -d '{
    "sessionId": "test-session-001",
    "intent": "financial_analysis",
    "entities": [...]
  }'
```

#### Phase 3: Business Context Integration
```bash
curl -X POST http://localhost:55244/api/chat/integrate-business-context \
  -H "Content-Type: application/json" \
  -d '{
    "sessionId": "test-session-001",
    "schemaContext": {...},
    "businessMetadata": true
  }'
```

#### Phase 4: AI Prompt Generation
```bash
curl -X POST http://localhost:55244/api/chat/generate-prompt \
  -H "Content-Type: application/json" \
  -d '{
    "sessionId": "test-session-001",
    "enhancedContext": {...}
  }'
```

#### Phase 5: SQL Generation
```bash
curl -X POST http://localhost:55244/api/chat/generate-sql \
  -H "Content-Type: application/json" \
  -d '{
    "sessionId": "test-session-001",
    "prompt": "...",
    "validateSyntax": true
  }'
```

#### Phase 6: SQL Validation and Optimization
```bash
curl -X POST http://localhost:55244/api/chat/validate-sql \
  -H "Content-Type: application/json" \
  -d '{
    "sql": "SELECT TOP 10 p.player_id, SUM(da.deposit_amount) as total_deposits FROM tbl_Daily_actions da...",
    "schemaContext": {...}
  }'
```

#### Phase 7: Query Execution
```bash
curl -X POST http://localhost:55244/api/chat/execute-query \
  -H "Content-Type: application/json" \
  -d '{
    "sessionId": "test-session-001",
    "validatedSql": "...",
    "timeout": 30
  }'
```

#### Phase 8: Result Processing and Formatting
```bash
curl -X POST http://localhost:55244/api/chat/format-results \
  -H "Content-Type: application/json" \
  -d '{
    "sessionId": "test-session-001",
    "rawResults": [...],
    "formatType": "table"
  }'
```

### 2. End-to-End Pipeline Test
```bash
# Complete pipeline test
curl -X POST http://localhost:55244/api/chat/process \
  -H "Content-Type: application/json" \
  -d '{
    "message": "Show me the top 10 depositors from UK yesterday",
    "sessionId": "test-session-001",
    "includeSteps": true,
    "validateAll": true
  }'
```

---

## AI Generation and Validation

### 1. Prompt Template Testing

#### Step 1.1: Test Enhanced Business Metadata in Prompts
```bash
curl -X POST http://localhost:55244/api/ai/test-prompt \
  -H "Content-Type: application/json" \
  -d '{
    "query": "Top depositors from UK",
    "includeBusinessMetadata": true,
    "includeExamples": true,
    "includeConstraints": true
  }'
```

#### Step 1.2: Validate Prompt Quality
Check that generated prompts include:
- ✅ Business-friendly column names
- ✅ Value examples for context
- ✅ Semantic tags for understanding
- ✅ Constraints and rules
- ✅ Related business terms

### 2. SQL Generation Accuracy

#### Step 2.1: Test Common Query Patterns
```bash
# Aggregation queries
curl -X POST http://localhost:55244/api/ai/generate-sql \
  -H "Content-Type: application/json" \
  -d '{
    "prompt": "Generate SQL for: Total deposits by country last month",
    "validateJoins": true,
    "optimizePerformance": true
  }'

# Filtering queries
curl -X POST http://localhost:55244/api/ai/generate-sql \
  -H "Content-Type: application/json" \
  -d '{
    "prompt": "Generate SQL for: Players from UK who deposited more than 1000",
    "validateJoins": true
  }'

# Time-based queries
curl -X POST http://localhost:55244/api/ai/generate-sql \
  -H "Content-Type: application/json" \
  -d '{
    "prompt": "Generate SQL for: Daily deposit trends last 30 days",
    "validateJoins": true
  }'
```

#### Step 2.2: Validate Generated SQL
```sql
-- Expected SQL structure for "Top 10 depositors from UK yesterday"
SELECT TOP 10 
    p.player_id,
    p.player_name,
    SUM(da.deposit_amount) as total_deposits
FROM tbl_Daily_actions da
INNER JOIN players p ON da.player_id = p.player_id
INNER JOIN countries c ON p.country_id = c.country_id
WHERE c.country_code = 'UK'
    AND CAST(da.action_date AS DATE) = CAST(DATEADD(day, -1, GETDATE()) AS DATE)
    AND da.deposit_amount > 0
GROUP BY p.player_id, p.player_name
ORDER BY total_deposits DESC;
```

### 3. SQL Validation Testing

#### Step 3.1: Column Mismatch Detection
```bash
curl -X POST http://localhost:55244/api/sql/validate \
  -H "Content-Type: application/json" \
  -d '{
    "sql": "SELECT invalid_column FROM tbl_Daily_actions",
    "checkColumns": true,
    "checkTables": true
  }'
```

#### Step 3.2: Syntax Error Detection
```bash
curl -X POST http://localhost:55244/api/sql/validate \
  -H "Content-Type: application/json" \
  -d '{
    "sql": "SELECT * FROM tbl_Daily_actions WHERE",
    "checkSyntax": true
  }'
```

---

## End-to-End Chat Testing

### 1. Sample Test Queries

#### Test Case 1: Financial Analysis
```bash
curl -X POST http://localhost:55244/api/chat \
  -H "Content-Type: application/json" \
  -d '{
    "message": "What were the total deposits by country last week?",
    "sessionId": "test-financial-001"
  }'
```

**Expected Flow:**
1. Intent: financial_analysis
2. Tables: tbl_Daily_actions, countries, players
3. SQL: Aggregation with proper joins and date filtering
4. Results: Country-wise deposit totals

#### Test Case 2: Player Analytics
```bash
curl -X POST http://localhost:55244/api/chat \
  -H "Content-Type: application/json" \
  -d '{
    "message": "Show me players who deposited more than 500 euros yesterday",
    "sessionId": "test-player-001"
  }'
```

#### Test Case 3: Time-based Analysis
```bash
curl -X POST http://localhost:55244/api/chat \
  -H "Content-Type: application/json" \
  -d '{
    "message": "Compare deposit trends between this month and last month",
    "sessionId": "test-trends-001"
  }'
```

### 2. Error Handling Testing

#### Test Case 4: Ambiguous Query
```bash
curl -X POST http://localhost:55244/api/chat \
  -H "Content-Type: application/json" \
  -d '{
    "message": "Show me the data",
    "sessionId": "test-error-001"
  }'
```

**Expected Response:**
```json
{
  "status": "clarification_needed",
  "message": "Could you please be more specific about what data you'd like to see?",
  "suggestions": [
    "Player data",
    "Deposit data", 
    "Game data"
  ]
}
```

#### Test Case 5: Invalid Date Range
```bash
curl -X POST http://localhost:55244/api/chat \
  -H "Content-Type: application/json" \
  -d '{
    "message": "Show deposits from next year",
    "sessionId": "test-error-002"
  }'
```

### 3. Real-time Monitoring

#### Step 3.1: Enable Enhanced Monitoring
```bash
curl -X POST http://localhost:55244/api/monitoring/enable \
  -H "Content-Type: application/json" \
  -d '{
    "sessionId": "test-session-001",
    "detailedSteps": true,
    "performanceMetrics": true,
    "tokenUsage": true
  }'
```

#### Step 3.2: Monitor Pipeline Execution
```bash
# Get real-time pipeline status
curl http://localhost:55244/api/monitoring/pipeline-status/test-session-001

# Get performance metrics
curl http://localhost:55244/api/monitoring/performance/test-session-001

# Get token usage
curl http://localhost:55244/api/monitoring/tokens/test-session-001
```

---

## Troubleshooting and Enhancement

### 1. Common Issues and Solutions

#### Issue 1: Gaming Tables in Financial Queries
**Problem:** Enhanced Schema Contextualization returns gaming tables for deposit queries

**Solution:**
```bash
# Check intent scoring weights
curl http://localhost:55244/api/schema/debug-scoring \
  -H "Content-Type: application/json" \
  -d '{
    "query": "total deposits",
    "showScoring": true
  }'

# Adjust gaming table penalty
curl -X POST http://localhost:55244/api/schema/configure \
  -H "Content-Type: application/json" \
  -d '{
    "gamingTablePenalty": 0.8,
    "financialTableBoost": 1.2
  }'
```

#### Issue 2: All Columns Retrieved
**Problem:** System returns all columns instead of selective fields

**Solution:**
```bash
# Enable selective field retrieval
curl -X POST http://localhost:55244/api/schema/configure \
  -H "Content-Type: application/json" \
  -d '{
    "selectiveFields": true,
    "maxColumnsPerTable": 10,
    "relevanceThreshold": 0.6
  }'
```

#### Issue 3: Empty Business Metadata Fields
**Problem:** Optional business metadata fields are empty

**Solution:**
```sql
-- Use AI to populate empty fields
EXEC sp_EnhanceBusinessMetadata 
    @TableName = 'tbl_Daily_actions',
    @UseAI = 1,
    @EnhanceEmptyOnly = 1;
```

### 2. Performance Optimization

#### Step 2.1: Query Performance Analysis
```bash
curl -X POST http://localhost:55244/api/performance/analyze \
  -H "Content-Type: application/json" \
  -d '{
    "sql": "SELECT * FROM tbl_Daily_actions WHERE...",
    "includeExecutionPlan": true,
    "suggestOptimizations": true
  }'
```

#### Step 2.2: Schema Retrieval Optimization
```bash
# Configure caching
curl -X POST http://localhost:55244/api/schema/configure-cache \
  -H "Content-Type: application/json" \
  -d '{
    "enableCaching": true,
    "cacheTimeout": 3600,
    "maxCacheSize": 1000
  }'
```

### 3. Monitoring and Logging

#### Step 3.1: Enable Detailed Logging
```json
// In appsettings.Development.json
{
  "Logging": {
    "LogLevel": {
      "BIReportingCopilot.Infrastructure.AI": "Debug",
      "BIReportingCopilot.Infrastructure.Schema": "Debug",
      "BIReportingCopilot.Infrastructure.Chat": "Debug"
    }
  }
}
```

#### Step 3.2: Monitor Key Metrics
```bash
# Get pipeline performance metrics
curl http://localhost:55244/api/metrics/pipeline-performance

# Get AI usage statistics
curl http://localhost:55244/api/metrics/ai-usage

# Get error rates
curl http://localhost:55244/api/metrics/error-rates
```

### 4. Future Enhancement Recommendations

#### 4.1: Advanced Business Metadata
- Implement semantic similarity scoring
- Add column relationship mapping
- Enhance business rule validation

#### 4.2: Improved AI Integration
- Fine-tune prompts based on success rates
- Implement query result validation
- Add context-aware error correction

#### 4.3: Enhanced Monitoring
- Real-time performance dashboards
- Automated optimization suggestions
- Predictive query performance analysis

---

## Advanced Testing Scenarios

### 1. Complex Query Testing

#### Scenario 1: Multi-Table Joins with Business Logic
```bash
curl -X POST http://localhost:55244/api/chat \
  -H "Content-Type: application/json" \
  -d '{
    "message": "Show me high-value players (deposits > 10000) from European countries who have been active in the last 30 days",
    "sessionId": "test-complex-001"
  }'
```

**Expected SQL Pattern:**
```sql
SELECT
    p.player_id,
    p.player_name,
    c.country_name,
    SUM(da.deposit_amount) as total_deposits,
    MAX(da.action_date) as last_activity
FROM tbl_Daily_actions da
INNER JOIN players p ON da.player_id = p.player_id
INNER JOIN countries c ON p.country_id = c.country_id
WHERE c.region = 'Europe'
    AND da.action_date >= DATEADD(day, -30, GETDATE())
GROUP BY p.player_id, p.player_name, c.country_name
HAVING SUM(da.deposit_amount) > 10000
ORDER BY total_deposits DESC;
```

#### Scenario 2: Time-Series Analysis
```bash
curl -X POST http://localhost:55244/api/chat \
  -H "Content-Type: application/json" \
  -d '{
    "message": "Show daily deposit trends for the last 3 months with moving average",
    "sessionId": "test-timeseries-001"
  }'
```

#### Scenario 3: Comparative Analysis
```bash
curl -X POST http://localhost:55244/api/chat \
  -H "Content-Type: application/json" \
  -d '{
    "message": "Compare deposit patterns between weekdays and weekends for UK players",
    "sessionId": "test-comparative-001"
  }'
```

### 2. Edge Case Testing

#### Edge Case 1: Empty Result Sets
```bash
curl -X POST http://localhost:55244/api/chat \
  -H "Content-Type: application/json" \
  -d '{
    "message": "Show deposits from Antarctica yesterday",
    "sessionId": "test-edge-001"
  }'
```

**Expected Response:**
```json
{
  "status": "success",
  "message": "No data found matching your criteria.",
  "results": [],
  "suggestions": [
    "Try a different date range",
    "Check available countries",
    "Broaden your search criteria"
  ]
}
```

#### Edge Case 2: Large Date Ranges
```bash
curl -X POST http://localhost:55244/api/chat \
  -H "Content-Type: application/json" \
  -d '{
    "message": "Show all deposits from the beginning of time",
    "sessionId": "test-edge-002"
  }'
```

#### Edge Case 3: Ambiguous Country References
```bash
curl -X POST http://localhost:55244/api/chat \
  -H "Content-Type: application/json" \
  -d '{
    "message": "Show deposits from Georgia",
    "sessionId": "test-edge-003"
  }'
```

### 3. Performance Stress Testing

#### Test 3.1: Concurrent User Simulation
```bash
# Create multiple concurrent sessions
for i in {1..10}; do
  curl -X POST http://localhost:55244/api/chat \
    -H "Content-Type: application/json" \
    -d "{
      \"message\": \"Show top depositors from session $i\",
      \"sessionId\": \"stress-test-$i\"
    }" &
done
wait
```

#### Test 3.2: Large Result Set Handling
```bash
curl -X POST http://localhost:55244/api/chat \
  -H "Content-Type: application/json" \
  -d '{
    "message": "Show all player transactions for the last year",
    "sessionId": "test-large-001",
    "pagination": {
      "pageSize": 1000,
      "maxResults": 50000
    }
  }'
```

#### Test 3.3: Complex Aggregation Performance
```bash
curl -X POST http://localhost:55244/api/chat \
  -H "Content-Type: application/json" \
  -d '{
    "message": "Calculate player lifetime value with retention rates by country and month",
    "sessionId": "test-complex-agg-001"
  }'
```

---

## Database Schema Validation

### 1. Schema Consistency Checks

#### Check 1: Foreign Key Relationships
```sql
-- Verify foreign key relationships exist
SELECT
    fk.name AS ForeignKeyName,
    tp.name AS ParentTable,
    cp.name AS ParentColumn,
    tr.name AS ReferencedTable,
    cr.name AS ReferencedColumn
FROM sys.foreign_keys fk
INNER JOIN sys.tables tp ON fk.parent_object_id = tp.object_id
INNER JOIN sys.tables tr ON fk.referenced_object_id = tr.object_id
INNER JOIN sys.foreign_key_columns fkc ON fk.object_id = fkc.constraint_object_id
INNER JOIN sys.columns cp ON fkc.parent_column_id = cp.column_id AND fkc.parent_object_id = cp.object_id
INNER JOIN sys.columns cr ON fkc.referenced_column_id = cr.column_id AND fkc.referenced_object_id = cr.object_id
WHERE tp.name IN ('tbl_Daily_actions', 'players', 'countries')
ORDER BY tp.name, fk.name;
```

#### Check 2: Data Type Consistency
```sql
-- Verify business data types match SQL data types
SELECT
    bci.TableName,
    bci.ColumnName,
    bci.BusinessDataType,
    c.DATA_TYPE as SQLDataType,
    CASE
        WHEN bci.BusinessDataType = 'Integer' AND c.DATA_TYPE IN ('int', 'bigint') THEN 'Match'
        WHEN bci.BusinessDataType = 'Text' AND c.DATA_TYPE IN ('varchar', 'nvarchar', 'text') THEN 'Match'
        WHEN bci.BusinessDataType = 'Decimal' AND c.DATA_TYPE IN ('decimal', 'numeric', 'money') THEN 'Match'
        WHEN bci.BusinessDataType = 'Date' AND c.DATA_TYPE IN ('date', 'datetime', 'datetime2') THEN 'Match'
        ELSE 'Mismatch'
    END as TypeConsistency
FROM [BIReportingCopilot_Dev].[dbo].[BusinessColumnInfo] bci
INNER JOIN INFORMATION_SCHEMA.COLUMNS c ON bci.TableName = c.TABLE_NAME AND bci.ColumnName = c.COLUMN_NAME
WHERE TypeConsistency = 'Mismatch'
ORDER BY bci.TableName, bci.ColumnName;
```

#### Check 3: Index Optimization
```sql
-- Check for missing indexes on frequently queried columns
SELECT
    t.name AS TableName,
    c.name AS ColumnName,
    CASE WHEN i.index_id IS NULL THEN 'Missing Index' ELSE 'Indexed' END as IndexStatus
FROM sys.tables t
INNER JOIN sys.columns c ON t.object_id = c.object_id
LEFT JOIN sys.index_columns ic ON c.object_id = ic.object_id AND c.column_id = ic.column_id
LEFT JOIN sys.indexes i ON ic.object_id = i.object_id AND ic.index_id = i.index_id
WHERE t.name IN ('tbl_Daily_actions', 'players', 'countries')
    AND c.name IN ('player_id', 'country_id', 'action_date', 'deposit_amount')
    AND (i.index_id IS NULL OR i.type = 0) -- No index or heap
ORDER BY t.name, c.name;
```

### 2. Data Quality Validation

#### Validation 1: Data Completeness
```sql
-- Check for missing critical data
SELECT
    'tbl_Daily_actions' as TableName,
    COUNT(*) as TotalRecords,
    COUNT(player_id) as PlayerIdCount,
    COUNT(action_date) as ActionDateCount,
    COUNT(CASE WHEN deposit_amount > 0 THEN 1 END) as DepositRecords,
    COUNT(CASE WHEN withdrawal_amount > 0 THEN 1 END) as WithdrawalRecords
FROM tbl_Daily_actions
WHERE action_date >= DATEADD(day, -30, GETDATE());

-- Check player data completeness
SELECT
    'players' as TableName,
    COUNT(*) as TotalPlayers,
    COUNT(country_id) as PlayersWithCountry,
    COUNT(registration_date) as PlayersWithRegDate,
    CAST(COUNT(country_id) * 100.0 / COUNT(*) as decimal(5,2)) as CountryCompleteness
FROM players;
```

#### Validation 2: Data Consistency
```sql
-- Check for orphaned records
SELECT 'Orphaned Daily Actions' as Issue, COUNT(*) as Count
FROM tbl_Daily_actions da
LEFT JOIN players p ON da.player_id = p.player_id
WHERE p.player_id IS NULL

UNION ALL

SELECT 'Orphaned Players' as Issue, COUNT(*) as Count
FROM players p
LEFT JOIN countries c ON p.country_id = c.country_id
WHERE c.country_id IS NULL;
```

#### Validation 3: Business Rule Compliance
```sql
-- Check business rule violations
SELECT
    'Negative Deposits' as Rule,
    COUNT(*) as Violations
FROM tbl_Daily_actions
WHERE deposit_amount < 0

UNION ALL

SELECT
    'Future Dates' as Rule,
    COUNT(*) as Violations
FROM tbl_Daily_actions
WHERE action_date > GETDATE()

UNION ALL

SELECT
    'Invalid Country Codes' as Rule,
    COUNT(*) as Violations
FROM countries
WHERE LEN(country_code) != 2 OR country_code NOT LIKE '[A-Z][A-Z]';
```

---

## API Integration Testing

### 1. Authentication and Authorization

#### Test 1.1: JWT Token Validation
```bash
# Get authentication token
curl -X POST http://localhost:55244/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "username": "testuser",
    "password": "testpass"
  }'

# Use token for authenticated requests
curl -X POST http://localhost:55244/api/chat \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -d '{
    "message": "Show deposits",
    "sessionId": "auth-test-001"
  }'
```

#### Test 1.2: Role-Based Access Control
```bash
# Test admin-only endpoints
curl -X GET http://localhost:55244/api/admin/system-metrics \
  -H "Authorization: Bearer ADMIN_TOKEN"

# Test user-level access
curl -X POST http://localhost:55244/api/chat \
  -H "Authorization: Bearer USER_TOKEN" \
  -d '{
    "message": "Show my deposits",
    "sessionId": "user-test-001"
  }'
```

### 2. Rate Limiting and Throttling

#### Test 2.1: Rate Limit Testing
```bash
# Test rate limiting (adjust based on configured limits)
for i in {1..100}; do
  curl -X POST http://localhost:55244/api/chat \
    -H "Content-Type: application/json" \
    -d "{
      \"message\": \"Test message $i\",
      \"sessionId\": \"rate-test-001\"
    }"
  sleep 0.1
done
```

#### Test 2.2: Concurrent Session Limits
```bash
# Test maximum concurrent sessions per user
for i in {1..20}; do
  curl -X POST http://localhost:55244/api/chat/start-session \
    -H "Content-Type: application/json" \
    -d "{
      \"userId\": \"testuser\",
      \"sessionId\": \"concurrent-$i\"
    }" &
done
wait
```

### 3. Error Handling and Recovery

#### Test 3.1: Database Connection Failure
```bash
# Simulate database connectivity issues
curl -X POST http://localhost:55244/api/admin/simulate-db-failure \
  -H "Content-Type: application/json" \
  -d '{"duration": 30}'

# Test system behavior during outage
curl -X POST http://localhost:55244/api/chat \
  -H "Content-Type: application/json" \
  -d '{
    "message": "Show deposits",
    "sessionId": "failure-test-001"
  }'
```

#### Test 3.2: AI Service Failure
```bash
# Simulate AI service unavailability
curl -X POST http://localhost:55244/api/admin/simulate-ai-failure \
  -H "Content-Type: application/json" \
  -d '{"duration": 60}'

# Test fallback mechanisms
curl -X POST http://localhost:55244/api/chat \
  -H "Content-Type: application/json" \
  -d '{
    "message": "Generate report",
    "sessionId": "ai-failure-test-001",
    "useFallback": true
  }'
```

#### Test 3.3: Timeout Handling
```bash
# Test long-running query timeout
curl -X POST http://localhost:55244/api/chat \
  -H "Content-Type: application/json" \
  -d '{
    "message": "Calculate complex analytics for all time",
    "sessionId": "timeout-test-001",
    "timeout": 5
  }'
```

---

## Monitoring and Analytics

### 1. Real-Time Monitoring Setup

#### Setup 1.1: Enable Comprehensive Logging
```json
// Add to appsettings.Development.json
{
  "Monitoring": {
    "EnableDetailedLogging": true,
    "LogPerformanceMetrics": true,
    "LogAIInteractions": true,
    "LogDatabaseQueries": true,
    "LogUserInteractions": true
  },
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "BIReportingCopilot.Infrastructure.Monitoring": "Debug",
        "BIReportingCopilot.Infrastructure.AI": "Debug"
      }
    },
    "WriteTo": [
      {
        "Name": "File",
        "Args": {
          "path": "logs/pipeline-.log",
          "rollingInterval": "Day",
          "retainedFileCountLimit": 30
        }
      }
    ]
  }
}
```

#### Setup 1.2: Configure Performance Counters
```bash
# Enable performance monitoring
curl -X POST http://localhost:55244/api/monitoring/configure \
  -H "Content-Type: application/json" \
  -d '{
    "enablePerformanceCounters": true,
    "trackMemoryUsage": true,
    "trackCpuUsage": true,
    "trackDatabaseConnections": true,
    "trackAITokenUsage": true,
    "samplingInterval": 5000
  }'
```

### 2. Performance Metrics Collection

#### Metric 2.1: Pipeline Performance
```bash
# Get pipeline step performance
curl http://localhost:55244/api/metrics/pipeline-performance?timeRange=1h

# Get average response times
curl http://localhost:55244/api/metrics/response-times?groupBy=endpoint

# Get throughput metrics
curl http://localhost:55244/api/metrics/throughput?timeRange=24h
```

#### Metric 2.2: AI Usage Analytics
```bash
# Get AI token usage
curl http://localhost:55244/api/metrics/ai-tokens?timeRange=24h

# Get AI model performance
curl http://localhost:55244/api/metrics/ai-performance?model=gpt-4

# Get cost analytics
curl http://localhost:55244/api/metrics/ai-costs?timeRange=30d
```

#### Metric 2.3: Database Performance
```bash
# Get query performance metrics
curl http://localhost:55244/api/metrics/database-performance?timeRange=1h

# Get connection pool status
curl http://localhost:55244/api/metrics/connection-pool

# Get slow query analysis
curl http://localhost:55244/api/metrics/slow-queries?threshold=5000
```

### 3. Alert Configuration

#### Alert 3.1: Performance Alerts
```bash
# Configure response time alerts
curl -X POST http://localhost:55244/api/alerts/configure \
  -H "Content-Type: application/json" \
  -d '{
    "alertType": "response_time",
    "threshold": 5000,
    "timeWindow": "5m",
    "severity": "warning",
    "notificationChannels": ["email", "slack"]
  }'

# Configure error rate alerts
curl -X POST http://localhost:55244/api/alerts/configure \
  -H "Content-Type: application/json" \
  -d '{
    "alertType": "error_rate",
    "threshold": 0.05,
    "timeWindow": "10m",
    "severity": "critical",
    "notificationChannels": ["email", "pagerduty"]
  }'
```

#### Alert 3.2: Business Metric Alerts
```bash
# Configure AI cost alerts
curl -X POST http://localhost:55244/api/alerts/configure \
  -H "Content-Type: application/json" \
  -d '{
    "alertType": "ai_cost",
    "threshold": 100,
    "timeWindow": "1d",
    "severity": "warning",
    "notificationChannels": ["email"]
  }'
```

---

## Conclusion

This comprehensive guide provides a complete framework for testing, validating, and monitoring the BI Reporting Copilot query processing pipeline. The guide covers:

- **Prerequisites and Setup**: Ensuring proper database connectivity and metadata preparation
- **Business Metadata Enhancement**: Populating and validating business context information
- **Schema Contextualization Testing**: Verifying intelligent table and column selection
- **Pipeline Validation**: Testing each phase of the query processing workflow
- **AI Integration**: Validating prompt generation and SQL creation accuracy
- **End-to-End Testing**: Complete user journey validation with real scenarios
- **Advanced Testing**: Complex queries, edge cases, and performance stress testing
- **Database Validation**: Schema consistency and data quality checks
- **API Integration**: Authentication, rate limiting, and error handling
- **Monitoring and Analytics**: Real-time performance tracking and alerting

### Best Practices for Implementation

1. **Start with Prerequisites**: Always verify database connectivity and metadata completeness before proceeding
2. **Test Incrementally**: Validate each pipeline phase individually before testing end-to-end
3. **Use Real Data**: Test with actual business scenarios and data patterns
4. **Monitor Continuously**: Implement comprehensive monitoring from day one
5. **Document Issues**: Keep detailed records of issues and resolutions for future reference

### Next Steps

1. Follow this guide systematically to validate your implementation
2. Customize test cases based on your specific business requirements
3. Implement automated testing for continuous validation
4. Set up monitoring dashboards for ongoing system health
5. Regularly review and update business metadata for optimal performance

For additional support, advanced configurations, or custom implementations, refer to the detailed API documentation or contact the development team.
