# 🔍 **Enhanced Schema Contextualization System - Deep Analysis & Comprehensive Fix Plan**

## 📊 **Executive Summary - UPDATED AFTER TEST RESULTS ANALYSIS**

**🎯 LATEST TEST RESULTS ANALYSIS (2025-06-27):**

### **✅ PROGRESS MADE**
- **Domain Classification**: ✅ FIXED - Now correctly shows "Banking" (was "Gaming")
- **BusinessDomainDetector**: ✅ WORKING - Score 0.36 with proper keyword matching
- **Intent Detection**: ✅ WORKING - Correctly identified as "Aggregation"

### **❌ CRITICAL ISSUES REMAINING**
- **Table Selection**: ❌ FAILED - Only `tbl_Daily_actions` returned (should be Players, Countries, GBP_transactions)
- **Column Retrieval**: ❌ FAILED - Still returning ALL 56,408+ columns instead of selective ~15-18
- **Enhanced Logging**: ❌ MISSING - No `[ENHANCED-SCHEMA-SERVICE]` tags visible in logs
- **Gaming Table Filtering**: ❌ FAILED - `tbl_Daily_actions` is a gaming table but wasn't filtered out

### **🔍 ROOT CAUSE ANALYSIS**
The Enhanced Schema Contextualization System is **partially working** but has critical flaws in:
1. **Table Filtering Logic**: Gaming tables not being properly excluded
2. **Column Selection**: No selective column retrieval implemented
3. **Service Logging**: Enhanced logging not appearing in pipeline tests
4. **Scoring System**: Gaming table penalties not strong enough or not applied

---

## 🔬 **PHASE 1: CORE ROUTING & SERVICE ARCHITECTURE ANALYSIS**

### **Current State - Service Resolution ✅ CONFIRMED**

**✅ Pipeline Test Service Resolution:**
```csharp
// AIPipelineTestController.cs:27
private readonly IBusinessMetadataRetrievalService _metadataService;

// Line 685: Correct service is being called
var schemaMetadata = await _metadataService.GetRelevantBusinessMetadataAsync(businessProfile, maxTables);
```

**✅ Service Registration:**
```csharp
// ServiceRegistrationExtensions.cs:184
services.AddScoped<IBusinessMetadataRetrievalService, BusinessMetadataRetrievalService>();
```

### **❌ CRITICAL ISSUE IDENTIFIED: Enhanced Logic Not Executing**

**Problem**: Our enhanced deposit query detection logic exists but isn't being triggered properly.

**Root Cause Analysis:**
1. **Fallback Logic Override**: When semantic search fails, system uses `GetBusinessTablesAsync()` which bypasses our scoring
2. **Scoring Logic Placement**: Enhanced scoring is in `CalculateTableRelevanceScore()` but may not be reached
3. **Query Analysis Issues**: Business profile may not contain the expected intent types

### **Service Architecture Map:**
```
Pipeline Test → BusinessMetadataRetrievalService → FindRelevantTablesAsync() → ScoreTablesAsync() → CalculateTableRelevanceScore()
                                                                                                    ↑
                                                                                            Enhanced Logic Here
```

---

## 🎯 **PHASE 2: TABLE FILTERING & SCORING ANALYSIS**

### **Current Table Retrieval Flow:**

**Step 1: Multiple Search Methods**
```csharp
// BusinessMetadataRetrievalService.cs:131-191
1. Semantic search (SemanticTableSearchAsync)
2. Domain-based filtering (GetTablesByDomainAsync) 
3. Entity-based matching (GetTablesByEntityAsync)
4. Business term matching (GetTablesByBusinessTermAsync)
5. FALLBACK: GetBusinessTablesAsync() ← PROBLEM SOURCE
```

**Step 2: Scoring & Ranking**
```csharp
// Line 212: Our enhanced logic should execute here
var scoredTables = await ScoreTablesAsync(allTables.Distinct().ToList(), profile);
```

### **❌ CRITICAL ISSUES IN TABLE FILTERING:**

#### **Issue 2.1: Fallback Logic Bypass**
```csharp
// Lines 194-208: PROBLEM
if (!allTables.Any())
{
    var allActiveTables = await _businessTableService.GetBusinessTablesAsync();
    allTables.AddRange(allActiveTables.Take(maxTables * 2)); // Gets ALL tables!
}
```
**Impact**: When searches fail, system gets ALL tables, including gaming tables.

#### **Issue 2.2: Enhanced Scoring Logic Issues**
```csharp
// Lines 599-604: Deposit query detection
bool isDepositQuery = (profile.Intent.Type == IntentType.Aggregation || profile.Intent.Type == IntentType.Operational) &&
                    (originalQueryLower.Contains("deposit") && (...)) ||
                    (originalQueryLower.Contains("depositor") && (...));
```
**Potential Issues**:
- Intent type might not match expected values
- Query text analysis might fail
- Scoring penalties might not be strong enough

#### **Issue 2.3: Gaming Table Penalty Logic**
```csharp
// Lines 656-673: Gaming table detection
if (tableLower.Contains("game") && !tableLower.Contains("deposit") &&
    !tableLower.Contains("transaction") && !tableLower.Contains("financial"))
{
    // Complex gaming table detection logic
    if (tableLower.Contains("games") || tableLower == "games" || ...)
    {
        isGamingTable = true;
    }
}
```
**Potential Issues**:
- Logic might be too restrictive
- Gaming tables might not match detection patterns
- Penalty (-1.5) might not be strong enough vs other bonuses

---

## 📊 **PHASE 3: COLUMN RETRIEVAL PERFORMANCE ANALYSIS**

### **❌ CRITICAL PERFORMANCE ISSUES:**

#### **Issue 3.1: ALL Columns Retrieved**
```csharp
// BusinessMetadataRetrievalService.cs:242-243
var table = await _businessTableService.GetBusinessTableAsync(tableId);
if (table?.Columns != null)
{
    // Processes ALL columns for each table
    allColumns.AddRange(table.Columns.Select(c => {
        // Maps ALL fields for each column
        return new BusinessColumnInfo { ... };
    }));
}
```

#### **Issue 3.2: ALL Fields Retrieved Per Column**
```csharp
// Lines 255-272: EVERY field is populated
return new BusinessColumnInfo
{
    Id = c.Id,
    ColumnName = c.ColumnName,
    DataType = dataType,
    BusinessName = GetFirstAlias(c.NaturalLanguageAliases) ?? c.ColumnName,
    BusinessMeaning = c.BusinessMeaning,
    BusinessPurpose = c.BusinessMeaning,
    BusinessContext = c.BusinessContext,
    SemanticContext = c.SemanticTags ?? string.Empty,
    DataExamples = ParseCommaSeparatedString(c.DataExamples),
    SampleValues = ParseCommaSeparatedString(c.ValueExamples),
    IsKey = c.IsKeyColumn,
    IsKeyColumn = c.IsKeyColumn,
    IsRequired = false,
    ValidationRules = c.ValidationRules,
    RelevanceScore = (double)c.SemanticRelevanceScore
};
```

#### **Issue 3.3: Database Query Performance**
```csharp
// BusinessTableManagementService.cs:170-172
var table = await _context.BusinessTableInfo
    .Include(t => t.Columns.Where(c => c.IsActive))  // Loads ALL active columns
    .FirstOrDefaultAsync(t => t.Id == id && t.IsActive);
```

### **Performance Impact:**
- **Token Usage**: Massive token consumption from unnecessary fields
- **Memory Usage**: High memory footprint from full object graphs
- **Database Load**: N+1 queries and excessive data loading
- **Response Time**: Slow response due to data volume

---

## 🚀 **COMPREHENSIVE 3-PHASE FIX PLAN**

### **PHASE 1: CRITICAL ROUTING FIXES** ⚡ *IMMEDIATE*

#### **Step 1.1: Add Service Identification & Debug Logging**
- Add unique service identifiers to track execution path
- Add detailed logging for deposit query detection
- Add logging for gaming table penalty application
- Verify business profile intent types and query content

#### **Step 1.2: Fix Fallback Logic**
- Prevent fallback to "all tables" when enhanced scoring should apply
- Implement minimum relevance thresholds
- Add query-specific table filtering before fallback

#### **Step 1.3: Strengthen Enhanced Scoring**
- Increase gaming table penalties (from -1.5 to -3.0)
- Add more deposit query detection patterns
- Implement progressive scoring with confidence levels

### **PHASE 2: INTELLIGENT TABLE FILTERING** 🎯 *HIGH PRIORITY*

#### **Step 2.1: Enhanced Query Classification**
- Implement comprehensive financial query detection
- Add query category classification (Financial, Gaming, Analytics)
- Create domain-specific scoring matrices

#### **Step 2.2: Smart Table Scoring System**
- **Financial Boost**: +4.0 for deposit/transaction tables
- **Gaming Penalty**: -3.0 for gaming tables in financial queries
- **Context Awareness**: Different scoring for different domains
- **Learning System**: Track successful queries for pattern learning

#### **Step 2.3: Intelligent Search Strategy**
- Implement search method prioritization
- Add query-specific search strategies
- Create domain-specific table whitelists/blacklists

### **PHASE 3: OPTIMIZED COLUMN RETRIEVAL** 🚀 *MEDIUM PRIORITY*

#### **Step 3.1: Selective Column Retrieval**
- Implement query-intent-based column filtering
- Add column relevance scoring and ranking
- Limit columns based on token budget constraints

#### **Step 3.2: Minimal Field Loading**
- Load only essential fields: Name, Type, Business Purpose
- Implement conditional field loading for detailed metadata
- Add lazy loading for non-critical fields

#### **Step 3.3: Performance Optimization**
- Implement intelligent caching strategies
- Add batch processing for multiple tables
- Optimize database queries with selective loading

---

## 🎯 **IMMEDIATE ACTION PLAN**

### **Next Steps (Phase 1.1):**
1. **Add Debug Logging** to identify why enhanced logic isn't working
2. **Verify Business Profile** content and intent types
3. **Test Deposit Query Detection** with actual query data
4. **Strengthen Gaming Table Penalties** to ensure they override other scores

### **Success Metrics:**
- ✅ Gaming tables eliminated from deposit queries
- ✅ Relevant financial tables prioritized
- ✅ Column count reduced by 70%+
- ✅ Response time improved by 50%+

---

**Ready to implement Phase 1.1 with detailed debugging to identify the root cause of why our enhanced logic isn't working as expected.**

---

## 🔧 **DETAILED TECHNICAL ANALYSIS**

### **Current Service Dependencies & Flow:**

```mermaid
graph TD
    A[AIPipelineTestController] --> B[IBusinessMetadataRetrievalService]
    B --> C[BusinessMetadataRetrievalService]
    C --> D[FindRelevantTablesAsync]
    D --> E[SemanticTableSearchAsync]
    D --> F[GetTablesByDomainAsync]
    D --> G[GetTablesByEntityAsync]
    D --> H[GetTablesByBusinessTermAsync]
    D --> I[FALLBACK: GetBusinessTablesAsync]
    I --> J[ScoreTablesAsync]
    J --> K[CalculateTableRelevanceScore]
    K --> L[Enhanced Deposit Logic]
    K --> M[Gaming Table Penalties]
```

### **Service Registration Analysis:**

**✅ Confirmed Registrations:**
```csharp
// Program.cs:930 - DynamicSchemaContextualizationService (NOT used by Pipeline)
builder.Services.AddScoped<IDynamicSchemaContextualizationService, DynamicSchemaContextualizationService>();

// ServiceRegistrationExtensions.cs:184 - BusinessMetadataRetrievalService (USED by Pipeline)
services.AddScoped<IBusinessMetadataRetrievalService, BusinessMetadataRetrievalService>();
```

**No Conflicts Found**: Pipeline Test correctly uses `BusinessMetadataRetrievalService`.

### **Business Profile Analysis:**

**Expected Profile Structure for "Top 10 depositors yesterday from UK":**
```csharp
BusinessContextProfile {
    OriginalQuestion: "Top 10 depositors yesterday from UK",
    Intent: { Type: IntentType.Aggregation },  // or Operational
    Domain: { Name: "Financial" },             // or Gaming
    BusinessTerms: ["depositors", "top", "UK"],
    IdentifiedMetrics: ["deposit amount"],
    IdentifiedDimensions: ["country", "date"],
    TimeContext: { Period: "yesterday" }
}
```

### **Enhanced Logic Execution Points:**

**1. Deposit Query Detection (Line 599):**
```csharp
bool isDepositQuery =
    (profile.Intent.Type == IntentType.Aggregation || profile.Intent.Type == IntentType.Operational) &&
    ((originalQueryLower.Contains("deposit") && (originalQueryLower.Contains("top") || ...)) ||
     (originalQueryLower.Contains("depositor") && (originalQueryLower.Contains("top") || ...)));
```

**2. Gaming Table Penalty (Line 669):**
```csharp
if (isGamingTable) {
    score -= 1.5; // Current penalty
    _logger.LogDebug("🎮 Penalizing gaming table for deposit query: {Table} (-1.5)", table.TableName);
}
```

---

## 🚨 **IDENTIFIED ROOT CAUSES**

### **Root Cause #1: Fallback Logic Dominance**
**Problem**: When semantic/domain searches return few results, fallback loads ALL tables.
**Evidence**:
```csharp
// Line 199: Fallback loads up to maxTables * 2 = 20 tables
allTables.AddRange(allActiveTables.Take(maxTables * 2));
```
**Impact**: Gaming tables enter the scoring pipeline despite enhanced logic.

### **Root Cause #2: Insufficient Gaming Table Penalties**
**Problem**: -1.5 penalty may not overcome positive scores from other factors.
**Evidence**: Standard scoring can add +0.3 (domain) + +0.2 (terms) + +0.5 (semantic) = +1.0
**Impact**: Gaming tables with +1.0 base score only get reduced to -0.5, still positive.

### **Root Cause #3: Intent Type Mismatch**
**Problem**: Business profile intent might not match expected `Aggregation` or `Operational`.
**Evidence**: Need to verify actual intent types being generated.
**Impact**: Deposit query detection fails, enhanced logic never executes.

### **Root Cause #4: Column Retrieval Performance**
**Problem**: ALL columns with ALL fields loaded for every table.
**Evidence**:
```csharp
// Loads complete column objects with 15+ fields each
var table = await _businessTableService.GetBusinessTableAsync(tableId);
allColumns.AddRange(table.Columns.Select(c => new BusinessColumnInfo { /* 15+ fields */ }));
```
**Impact**: Massive token usage, slow performance.

---

## 🎯 **PHASE 1 IMPLEMENTATION DETAILS**

### **Step 1.1: Enhanced Debug Logging**

**Add to BusinessMetadataRetrievalService.cs:**
```csharp
public async Task<ContextualBusinessSchema> GetRelevantBusinessMetadataAsync(
    BusinessContextProfile profile, int maxTables = 5)
{
    // Add service identification
    _logger.LogInformation("🔍 [ENHANCED-SCHEMA-SERVICE] Starting metadata retrieval for query: {Query}",
        profile.OriginalQuestion);

    // Log business profile details
    _logger.LogInformation("📊 [PROFILE-DEBUG] Intent: {Intent}, Domain: {Domain}, Terms: {Terms}",
        profile.Intent.Type, profile.Domain.Name, string.Join(", ", profile.BusinessTerms));

    // Continue with existing logic...
}
```

**Add to CalculateTableRelevanceScore:**
```csharp
private async Task<double> CalculateTableRelevanceScore(
    BusinessTableInfoDto table, BusinessContextProfile profile)
{
    _logger.LogInformation("🔍 [SCORING-DEBUG] Evaluating table: {Schema}.{Table} for query: {Query}",
        table.SchemaName, table.TableName, profile.OriginalQuestion);

    // Log deposit query detection
    bool isDepositQuery = /* existing logic */;
    _logger.LogInformation("🏦 [DEPOSIT-DEBUG] Deposit query detected: {IsDeposit} for table: {Table}",
        isDepositQuery, table.TableName);

    // Continue with existing logic...
}
```

### **Step 1.2: Strengthen Gaming Table Penalties**

**Increase penalty from -1.5 to -3.0:**
```csharp
if (isGamingTable)
{
    score -= 3.0; // Increased from -1.5
    _logger.LogWarning("🎮 [PENALTY-DEBUG] STRONG gaming table penalty applied: {Table} (-3.0)", table.TableName);
}
```

### **Step 1.3: Improve Fallback Logic**

**Add relevance threshold before fallback:**
```csharp
// Before fallback, check if we have any relevant tables
var relevantTablesCount = allTables.Count(t => /* has some relevance criteria */);
if (relevantTablesCount == 0)
{
    _logger.LogWarning("⚠️ [FALLBACK-DEBUG] No relevant tables found, applying fallback with enhanced filtering");

    // Apply domain-specific filtering even in fallback
    var allActiveTables = await _businessTableService.GetBusinessTablesAsync();
    var filteredTables = allActiveTables.Where(t =>
        !IsGamingTableForFinancialQuery(t, profile)).Take(maxTables);
    allTables.AddRange(filteredTables);
}
```

---

## 📈 **SUCCESS METRICS & VALIDATION**

### **Phase 1 Success Criteria:**
- [ ] Enhanced debug logging shows deposit query detection working
- [ ] Gaming table penalties are applied and logged
- [ ] Gaming tables score below relevance threshold
- [ ] Only financial/relevant tables returned for deposit queries

### **Performance Targets:**
- **Table Count**: Reduce from 10+ to 3-5 relevant tables
- **Column Count**: Reduce from 100+ to 20-30 essential columns
- **Response Time**: Improve from 2-3s to <1s
- **Token Usage**: Reduce by 60-70%

### **Test Queries for Validation:**
1. `"Top 10 depositors yesterday from UK"` → Should return: players, deposits, countries tables
2. `"Gaming revenue by country"` → Should return: games, revenue, countries tables
3. `"Daily active users last week"` → Should return: users, activity, date tables

---

---

## 🚀 **COMPREHENSIVE IMPLEMENTATION PLAN - UPDATED**

### **🎯 PHASE 1: CRITICAL TABLE FILTERING FIXES** ⚡ *IMMEDIATE*

#### **Step 1.1: Fix Gaming Table Detection Logic**

**Problem**: `tbl_Daily_actions` is a gaming table but not being filtered out for deposit queries.

**Root Cause**: The gaming table detection logic in `IsGamingTableForFinancialQuery` is not correctly identifying `tbl_Daily_actions` as a gaming table.

**Solution**:
```csharp
// File: BusinessMetadataRetrievalService.cs
// Method: IsGamingTableForFinancialQuery (around line 2508)

private bool IsGamingTableForFinancialQuery(BusinessTableInfoDto table, BusinessContextProfile profile)
{
    var tableLower = table.TableName.ToLower();
    var originalQueryLower = profile.OriginalQuestion.ToLower();

    // Enhanced gaming table detection
    bool isGamingTable = false;

    // CRITICAL FIX: Add tbl_Daily_actions to gaming table detection
    if (tableLower == "tbl_daily_actions" ||
        tableLower == "daily_actions" ||
        tableLower.Contains("game") ||
        tableLower.Contains("gaming") ||
        tableLower.Contains("bet") ||
        tableLower.Contains("casino") ||
        tableLower.Contains("sport"))
    {
        isGamingTable = true;
    }

    // Check if this is a financial query
    bool isFinancialQuery = originalQueryLower.Contains("deposit") ||
                           originalQueryLower.Contains("depositor") ||
                           originalQueryLower.Contains("transaction") ||
                           originalQueryLower.Contains("payment") ||
                           originalQueryLower.Contains("financial");

    if (isGamingTable && isFinancialQuery)
    {
        _logger.LogWarning("🚫 [DOMAIN-FILTER] Excluded: {Table} - Gaming table excluded from financial query", table.TableName);
        return true; // Exclude this table
    }

    return false;
}
```

#### **Step 1.2: Strengthen Gaming Table Penalties**

**Problem**: Gaming table penalties (-1.5) are not strong enough to overcome positive scores.

**Solution**:
```csharp
// File: BusinessMetadataRetrievalService.cs
// Method: CalculateTableRelevanceScore (around line 669)

if (isGamingTable)
{
    score -= 5.0; // Increased from -1.5 to -5.0 (CRITICAL PENALTY)
    _logger.LogWarning("🎮 [GAMING-PENALTY] CRITICAL gaming table penalty applied: {Table} (-5.0)", table.TableName);
}
```

#### **Step 1.3: Add Enhanced Service Logging**

**Problem**: No `[ENHANCED-SCHEMA-SERVICE]` tags appearing in pipeline test logs.

**Solution**:
```csharp
// File: BusinessMetadataRetrievalService.cs
// Method: GetRelevantBusinessMetadataAsync (around line 42)

public async Task<ContextualBusinessSchema> GetRelevantBusinessMetadataAsync(
    BusinessContextProfile profile, int maxTables = 5)
{
    try
    {
        // Add service identification and comprehensive debug logging
        _logger.LogInformation("� [ENHANCED-SCHEMA-SERVICE] Starting metadata retrieval for query: {Query}",
            profile.OriginalQuestion);

        _logger.LogInformation("📊 [ENHANCED-SCHEMA-SERVICE] Profile - Intent: {Intent}, Domain: {Domain}, Score: {Score:F2}",
            profile.Intent.Type, profile.Domain.Name, profile.Domain.RelevanceScore);

        // Continue with existing logic...
        var result = await FindRelevantTablesAsync(profile, maxTables);

        _logger.LogInformation("✅ [ENHANCED-SCHEMA-SERVICE] Enhanced schema contextualization complete - Tables: {Count}",
            result.Count);

        return await BuildContextualSchema(result, profile);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "❌ [ENHANCED-SCHEMA-SERVICE] Error in enhanced schema contextualization: {Error}", ex.Message);
        throw;
    }
}
```

### **🎯 PHASE 2: SELECTIVE COLUMN RETRIEVAL** �🚀 *HIGH PRIORITY*

#### **Step 2.1: Implement Query-Intent-Based Column Filtering**

**Problem**: ALL 56,408+ columns being retrieved instead of selective ~15-18 columns.

**Solution**:
```csharp
// File: BusinessMetadataRetrievalService.cs
// Method: GetRelevantColumnsForTable (NEW METHOD)

private async Task<List<BusinessColumnInfo>> GetRelevantColumnsForTable(
    BusinessTableInfoDto table, BusinessContextProfile profile, int maxColumns = 15)
{
    _logger.LogDebug("🔍 [SELECTIVE-COLUMNS] Filtering columns for table: {Table}", table.TableName);

    var allColumns = await _businessTableService.GetBusinessTableAsync(table.Id);
    if (allColumns?.Columns == null) return new List<BusinessColumnInfo>();

    var relevantColumns = new List<BusinessColumnInfo>();
    var originalQueryLower = profile.OriginalQuestion.ToLower();

    // Priority 1: Key columns (always include)
    var keyColumns = allColumns.Columns.Where(c => c.IsKeyColumn).Take(3);

    // Priority 2: Query-specific columns
    var querySpecificColumns = allColumns.Columns.Where(c =>
        IsColumnRelevantToQuery(c, originalQueryLower)).Take(8);

    // Priority 3: High-importance columns
    var importantColumns = allColumns.Columns
        .Where(c => c.SemanticRelevanceScore > 0.7)
        .OrderByDescending(c => c.SemanticRelevanceScore)
        .Take(4);

    // Combine and deduplicate
    var selectedColumns = keyColumns
        .Concat(querySpecificColumns)
        .Concat(importantColumns)
        .Distinct()
        .Take(maxColumns);

    foreach (var column in selectedColumns)
    {
        relevantColumns.Add(MapToBusinessColumnInfo(column));
    }

    _logger.LogInformation("✅ [SELECTIVE-COLUMNS] Column filtering complete: {Selected}/{Total} columns selected for {Table}",
        relevantColumns.Count, allColumns.Columns.Count, table.TableName);

    return relevantColumns;
}

private bool IsColumnRelevantToQuery(BusinessColumnInfoEntity column, string queryLower)
{
    var columnLower = column.ColumnName.ToLower();
    var aliasesLower = column.NaturalLanguageAliases?.ToLower() ?? "";
    var meaningLower = column.BusinessMeaning?.ToLower() ?? "";

    // Deposit query specific columns
    if (queryLower.Contains("deposit"))
    {
        return columnLower.Contains("deposit") ||
               columnLower.Contains("amount") ||
               columnLower.Contains("player") ||
               columnLower.Contains("country") ||
               columnLower.Contains("date") ||
               aliasesLower.Contains("deposit") ||
               meaningLower.Contains("deposit");
    }

    // Add more query-specific logic here
    return false;
}
```

#### **Step 2.2: Optimize Database Queries**

**Problem**: Loading ALL columns with ALL fields for every table.

**Solution**:
```csharp
// File: BusinessTableManagementService.cs
// Method: GetBusinessTableAsync (around line 170)

public async Task<BusinessTableInfoEntity> GetBusinessTableAsync(int id, bool includeAllColumns = false)
{
    if (includeAllColumns)
    {
        // Full load (existing behavior)
        return await _context.BusinessTableInfo
            .Include(t => t.Columns.Where(c => c.IsActive))
            .FirstOrDefaultAsync(t => t.Id == id && t.IsActive);
    }
    else
    {
        // Selective load (NEW - for performance)
        return await _context.BusinessTableInfo
            .Include(t => t.Columns.Where(c => c.IsActive &&
                (c.IsKeyColumn || c.SemanticRelevanceScore > 0.7)))
            .FirstOrDefaultAsync(t => t.Id == id && t.IsActive);
    }
}
```

### **🎯 PHASE 3: VERIFICATION & TESTING** 🧪 *IMMEDIATE*

#### **Step 3.1: Add Comprehensive Debug Logging**

**Solution**: Add detailed logging to track execution flow:
```csharp
// Add to each critical method in BusinessMetadataRetrievalService.cs

_logger.LogInformation("🔍 [ENHANCED-SCHEMA-SERVICE] Step 1: Finding relevant tables");
_logger.LogInformation("🔍 [ENHANCED-SCHEMA-SERVICE] Step 2: Applying domain-specific filtering");
_logger.LogInformation("🔍 [ENHANCED-SCHEMA-SERVICE] Step 3: Scoring and ranking tables");
_logger.LogInformation("🔍 [ENHANCED-SCHEMA-SERVICE] Step 4: Selective column retrieval");
```

#### **Step 3.2: Create Test Validation Script**

**Solution**: Update the PowerShell test script:
```powershell
# File: backend/test-enhanced-schema.ps1

$testQuery = "Top 10 depositors yesterday from UK"
$response = Invoke-RestMethod -Uri "https://localhost:55244/api/pipeline/test" -Method POST -Body (@{
    query = $testQuery
    requestedSteps = @("BusinessContextAnalysis", "TokenBudgetManagement", "SchemaRetrieval")
} | ConvertTo-Json) -ContentType "application/json" -SkipCertificateCheck

# Validation checks
$tables = $response.results.SchemaRetrieval.schemaMetadata.relevantTables
$totalColumns = ($response.results.SchemaRetrieval.schemaMetadata.tableColumns.PSObject.Properties.Value | Measure-Object -Sum Count).Sum

Write-Host "🧪 ENHANCED SCHEMA CONTEXTUALIZATION TEST RESULTS:" -ForegroundColor Cyan
Write-Host "📊 Domain: $($response.results.BusinessContextAnalysis.domain)" -ForegroundColor White
Write-Host "📊 Tables: $($tables.Count)" -ForegroundColor White
Write-Host "📊 Total Columns: $totalColumns" -ForegroundColor White

# Check for gaming tables
$gamingTables = $tables | Where-Object { $_.tableName -like "*game*" -or $_.tableName -eq "tbl_Daily_actions" }
if ($gamingTables.Count -eq 0) {
    Write-Host "✅ Gaming Table Filtering: WORKING" -ForegroundColor Green
} else {
    Write-Host "❌ Gaming Table Filtering: FAILED (Found: $($gamingTables.tableName -join ', '))" -ForegroundColor Red
}

# Check column count
if ($totalColumns -lt 50) {
    Write-Host "✅ Column Filtering: WORKING ($totalColumns columns)" -ForegroundColor Green
} else {
    Write-Host "❌ Column Filtering: FAILED ($totalColumns columns - should be <50)" -ForegroundColor Red
}
```

---

## 📋 **IMPLEMENTATION CHECKLIST**

### **Immediate Actions (Phase 1):**
- [ ] **Fix Gaming Table Detection**: Update `IsGamingTableForFinancialQuery` to include `tbl_Daily_actions`
- [ ] **Strengthen Penalties**: Increase gaming table penalty from -1.5 to -5.0
- [ ] **Add Enhanced Logging**: Add `[ENHANCED-SCHEMA-SERVICE]` tags throughout the service
- [ ] **Test Domain Classification**: Verify "Banking" domain is correctly detected

### **High Priority Actions (Phase 2):**
- [ ] **Implement Selective Column Retrieval**: Add `GetRelevantColumnsForTable` method
- [ ] **Optimize Database Queries**: Add selective loading option to `GetBusinessTableAsync`
- [ ] **Add Query-Intent Mapping**: Implement `IsColumnRelevantToQuery` logic
- [ ] **Test Column Filtering**: Verify <50 columns returned instead of 56,408+

### **Verification Actions (Phase 3):**
- [ ] **Run Test Script**: Execute enhanced test validation
- [ ] **Monitor Logs**: Check for `[ENHANCED-SCHEMA-SERVICE]` tags
- [ ] **Validate Results**: Ensure only relevant tables (Players, Countries, GBP_transactions)
- [ ] **Performance Check**: Verify response time improvement

---

## 🎯 **EXPECTED RESULTS AFTER IMPLEMENTATION**

### **✅ Success Criteria:**
1. **Domain Classification**: "Banking" (✅ Already working)
2. **Table Selection**: Only Players, Countries, GBP_transactions tables
3. **Gaming Table Exclusion**: No `tbl_Daily_actions` or other gaming tables
4. **Column Count**: ~15-20 relevant columns (not 56,408+)
5. **Enhanced Logging**: `[ENHANCED-SCHEMA-SERVICE]` tags visible in logs
6. **Performance**: <2 second response time

### **🔍 Debug Logs to Watch For:**
```
🔍 [ENHANCED-SCHEMA-SERVICE] Starting metadata retrieval for query: Top 10 depositors yesterday from UK
📊 [ENHANCED-SCHEMA-SERVICE] Profile - Intent: Aggregation, Domain: Banking, Score: 0.36
🚫 [DOMAIN-FILTER] Excluded: tbl_Daily_actions - Gaming table excluded from financial query
🎮 [GAMING-PENALTY] CRITICAL gaming table penalty applied: tbl_Daily_actions (-5.0)
✅ [SELECTIVE-COLUMNS] Column filtering complete: 18/156 columns selected for Players
✅ [ENHANCED-SCHEMA-SERVICE] Enhanced schema contextualization complete - Tables: 3
```

**🚀 Ready to implement the comprehensive fix plan with focus on gaming table exclusion and selective column retrieval.**
