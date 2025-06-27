# Query Processing Pipeline Fix Plan

## Current Issue Analysis

### Problem Statement
The query "Top 10 depositors yesterday from UK" should generate:
```sql
SELECT TOP 10 da.PlayerID,
    SUM(da.Deposits) AS Deposit
FROM [DailyActionsDB].[common].[tbl_Daily_actions] da
INNER JOIN [DailyActionsDB].[common].tbl_Daily_actions_players dap (NOLOCK) ON dap.PlayerID = da.PlayerID
INNER JOIN [DailyActionsDB].[common].tbl_Countries c (NOLOCK) ON c.CountryID = dap.CountryID
WHERE da.Date = '2025-06-25'
AND c.CountryIntlCode = 'GB'
GROUP BY da.PlayerID
ORDER BY SUM(da.Deposits) desc
```

### Current Issues Identified

1. **Business Context Analysis Issues:**
   - Missing proper table mapping for entities
   - Incorrect domain classification (Gaming vs Banking/Finance)
   - Empty mappedTableName and mappedColumnName fields
   - Missing time context parsing for "yesterday"
   - Incorrect related tables (tbl_Daily_actions only, missing players and countries)

2. **Schema Retrieval Issues:**
   - Not identifying required related tables (players, countries)
   - Missing foreign key relationships
   - Incorrect table selection for domain

3. **SQL Generation Issues:**
   - Not generating proper JOINs
   - Missing date calculations for "yesterday"
   - Not mapping business terms to actual columns

## Comprehensive Fix Plan

### Phase 1: Business Context Analysis Enhancement

#### Step 1.1: Fix Entity Extraction and Mapping
**File:** `BIReportingCopilot.Infrastructure/BusinessContext/Enhanced/EnhancedBusinessContextAnalyzer.cs`

**Issues to Fix:**
- [ ] Entity mapping is not connecting to actual database schema
- [ ] mappedTableName and mappedColumnName are empty
- [ ] Entity types are too generic

**Actions:**
1. **Enhance Entity Mapping Logic:**
   ```csharp
   // Add schema-aware entity mapping
   private async Task<List<BusinessEntity>> MapEntitiesToSchema(List<BusinessEntity> entities, SchemaMetadata schema)
   {
       foreach (var entity in entities)
       {
           switch (entity.Type)
           {
               case BusinessEntityType.Table:
                   entity.MappedTableName = await FindBestTableMatch(entity.Name, schema);
                   break;
               case BusinessEntityType.Dimension:
                   var (table, column) = await FindDimensionMapping(entity.Name, schema);
                   entity.MappedTableName = table;
                   entity.MappedColumnName = column;
                   break;
               case BusinessEntityType.Metric:
                   var (metricTable, metricColumn) = await FindMetricMapping(entity.Name, schema);
                   entity.MappedTableName = metricTable;
                   entity.MappedColumnName = metricColumn;
                   break;
           }
       }
       return entities;
   }
   ```

2. **Add Business Term Dictionary:**
   ```csharp
   private static readonly Dictionary<string, TableColumnMapping> BusinessTermMappings = new()
   {
       { "depositors", new("tbl_Daily_actions", "PlayerID", "Deposits") },
       { "players", new("tbl_Daily_actions_players", "PlayerID") },
       { "countries", new("tbl_Countries", "CountryID", "CountryIntlCode") },
       { "UK", new("tbl_Countries", "CountryIntlCode", "GB") },
       { "deposits", new("tbl_Daily_actions", "Deposits") }
   };
   ```

#### Step 1.2: Fix Domain Classification
**Issues to Fix:**
- [ ] "depositors" query classified as "Gaming" instead of "Banking/Finance"
- [ ] Domain affects table selection

**Actions:**
1. **Update Domain Classification Logic:**
   ```csharp
   private BusinessDomain ClassifyDomain(string query, List<BusinessEntity> entities)
   {
       // Banking/Finance keywords
       if (ContainsFinancialTerms(query, entities))
           return new BusinessDomain { Name = "Banking", RelevanceScore = 0.9 };
       
       // Gaming keywords  
       if (ContainsGamingTerms(query, entities))
           return new BusinessDomain { Name = "Gaming", RelevanceScore = 0.8 };
           
       return new BusinessDomain { Name = "General", RelevanceScore = 0.5 };
   }
   ```

#### Step 1.3: Fix Time Context Parsing
**Issues to Fix:**
- [ ] "yesterday" not parsed into actual date
- [ ] timeContext is null

**Actions:**
1. **Enhance Time Context Parser:**
   ```csharp
   private TimeContext ParseTimeContext(string query)
   {
       if (query.Contains("yesterday"))
       {
           return new TimeContext
           {
               StartDate = DateTime.Today.AddDays(-1),
               EndDate = DateTime.Today.AddDays(-1),
               RelativeExpression = "yesterday",
               Granularity = TimeGranularity.Day
           };
       }
       // Add more time parsing logic...
   }
   ```

### Phase 2: Schema Retrieval Enhancement

#### Step 2.1: Fix Related Table Discovery
**File:** `BIReportingCopilot.Infrastructure/Schema/BusinessSchemaService.cs`

**Issues to Fix:**
- [ ] Not identifying foreign key relationships
- [ ] Missing related tables (players, countries)

**Actions:**
1. **Enhance Related Table Discovery:**
   ```csharp
   public async Task<List<string>> GetRelatedTables(string primaryTable, List<BusinessEntity> entities)
   {
       var relatedTables = new List<string> { primaryTable };
       
       // Add tables based on foreign key relationships
       var foreignKeys = await GetForeignKeyRelationships(primaryTable);
       relatedTables.AddRange(foreignKeys.Select(fk => fk.ReferencedTable));
       
       // Add tables based on business entities
       foreach (var entity in entities)
       {
           if (!string.IsNullOrEmpty(entity.MappedTableName) && 
               !relatedTables.Contains(entity.MappedTableName))
           {
               relatedTables.Add(entity.MappedTableName);
           }
       }
       
       return relatedTables;
   }
   ```

#### Step 2.2: Add Foreign Key Relationship Mapping
**Actions:**
1. **Create Foreign Key Discovery Service:**
   ```csharp
   public class ForeignKeyRelationshipService
   {
       public async Task<List<ForeignKeyRelationship>> GetRelationships(string tableName)
       {
           // Query INFORMATION_SCHEMA to get FK relationships
           var sql = @"
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
               INNER JOIN sys.columns cp ON fkc.parent_object_id = cp.object_id AND fkc.parent_column_id = cp.column_id
               INNER JOIN sys.columns cr ON fkc.referenced_object_id = cr.object_id AND fkc.referenced_column_id = cr.column_id
               WHERE tp.name = @tableName OR tr.name = @tableName";
           
           return await ExecuteQuery<ForeignKeyRelationship>(sql, new { tableName });
       }
   }
   ```

### Phase 3: SQL Generation Enhancement

#### Step 3.1: Fix JOIN Generation
**File:** `BIReportingCopilot.Infrastructure/AI/Agents/SqlGenerationAgent.cs`

**Issues to Fix:**
- [ ] Not generating proper JOINs between related tables
- [ ] Missing NOLOCK hints
- [ ] Incorrect table aliases

**Actions:**
1. **Enhance JOIN Generation Logic:**
   ```csharp
   private string GenerateJoins(List<string> tables, List<ForeignKeyRelationship> relationships)
   {
       var joins = new StringBuilder();
       var processedTables = new HashSet<string> { tables[0] }; // Start with main table
       
       foreach (var table in tables.Skip(1))
       {
           var relationship = relationships.FirstOrDefault(r => 
               (r.ParentTable == table && processedTables.Contains(r.ReferencedTable)) ||
               (r.ReferencedTable == table && processedTables.Contains(r.ParentTable)));
               
           if (relationship != null)
           {
               var alias1 = GetTableAlias(relationship.ParentTable);
               var alias2 = GetTableAlias(relationship.ReferencedTable);
               
               joins.AppendLine($"INNER JOIN [{relationship.ParentTable}] {alias1} (NOLOCK) ON {alias1}.{relationship.ParentColumn} = {alias2}.{relationship.ReferencedColumn}");
               processedTables.Add(table);
           }
       }
       
       return joins.ToString();
   }
   ```

#### Step 3.2: Fix Date Handling
**Actions:**
1. **Add Date Calculation Logic:**
   ```csharp
   private string ProcessTimeContext(TimeContext timeContext)
   {
       if (timeContext?.RelativeExpression == "yesterday")
       {
           return $"da.Date = '{timeContext.StartDate:yyyy-MM-dd}'";
       }
       // Add more date processing...
   }
   ```

#### Step 3.3: Fix Aggregation and Grouping
**Actions:**
1. **Enhance Aggregation Logic:**
   ```csharp
   private SqlQuery GenerateAggregationQuery(BusinessContextProfile profile)
   {
       var metrics = profile.Entities.Where(e => e.Type == BusinessEntityType.Metric);
       var dimensions = profile.Entities.Where(e => e.Type == BusinessEntityType.Dimension);
       
       var selectClause = BuildSelectClause(metrics, dimensions);
       var groupByClause = BuildGroupByClause(dimensions);
       var orderByClause = BuildOrderByClause(metrics);
       
       return new SqlQuery
       {
           Select = selectClause,
           GroupBy = groupByClause,
           OrderBy = orderByClause
       };
   }
   ```

### Phase 4: Integration and Testing

#### Step 4.1: Update ProcessFlow Integration
**File:** `BIReportingCopilot.API/Controllers/QueryController.cs`

**Actions:**
1. **Add detailed step tracking:**
   ```csharp
   // Step 1: Business Context Analysis
   await _processFlowTracker.StartStepAsync(ProcessFlowSteps.SemanticAnalysis);
   var businessProfile = await _businessContextAnalyzer.AnalyzeBusinessContextAsync(userQuestion, userId);
   await _processFlowTracker.SetStepOutputAsync(ProcessFlowSteps.SemanticAnalysis, businessProfile);
   
   // Step 2: Schema Retrieval
   await _processFlowTracker.StartStepAsync(ProcessFlowSteps.SchemaRetrieval);
   var relatedTables = await _schemaService.GetRelatedTables(businessProfile);
   await _processFlowTracker.SetStepOutputAsync(ProcessFlowSteps.SchemaRetrieval, relatedTables);
   
   // Step 3: SQL Generation
   await _processFlowTracker.StartStepAsync(ProcessFlowSteps.SqlGeneration);
   var sqlQuery = await _sqlGenerationAgent.GenerateSQL(businessProfile, relatedTables);
   await _processFlowTracker.SetStepOutputAsync(ProcessFlowSteps.SqlGeneration, sqlQuery);
   ```

#### Step 4.2: Add Comprehensive Testing
**Actions:**
1. **Create test cases for each step:**
   ```csharp
   [Test]
   public async Task BusinessContextAnalysis_DepositorQuery_ShouldMapCorrectly()
   {
       var query = "Top 10 depositors yesterday from UK";
       var result = await _analyzer.AnalyzeBusinessContextAsync(query, "test-user");
       
       Assert.That(result.Domain.Name, Is.EqualTo("Banking"));
       Assert.That(result.Entities.Any(e => e.MappedTableName == "tbl_Daily_actions"));
       Assert.That(result.TimeContext, Is.Not.Null);
       Assert.That(result.TimeContext.RelativeExpression, Is.EqualTo("yesterday"));
   }
   ```

## Implementation Priority

### High Priority (Week 1)
1. ✅ Fix Business Context Analysis entity mapping
2. ✅ Fix time context parsing
3. ✅ Add foreign key relationship discovery

### Medium Priority (Week 2)  
4. ✅ Fix SQL JOIN generation
5. ✅ Add proper aggregation logic
6. ✅ Update domain classification

### Low Priority (Week 3)
7. ✅ Add comprehensive testing
8. ✅ Performance optimization
9. ✅ Documentation updates

## Success Criteria

- [ ] Query "Top 10 depositors yesterday from UK" generates correct SQL
- [ ] Business entities properly mapped to schema
- [ ] Time references correctly parsed
- [ ] Related tables automatically discovered
- [ ] JOINs generated based on foreign keys
- [ ] All ProcessFlow steps show detailed information

## Next Steps

1. Start with Phase 1.1 - Fix Entity Extraction and Mapping
2. Test each component individually
3. Integration testing with full pipeline
4. Performance testing with complex queries
