# Business Metadata Population Guide

This guide provides multiple approaches to populate business metadata for your BI Co-Pilot semantic layer. Choose the approach that best fits your needs and organizational structure.

## 🎯 Overview

The semantic layer requires rich business metadata to provide accurate, contextual schema information to the LLM. This metadata includes:

- **Business Purpose**: What the table/column is used for
- **Domain Classification**: Business domain (Finance, Customer, Gaming, etc.)
- **Natural Language Aliases**: Business-friendly names
- **Usage Patterns**: How the data is typically queried
- **Data Quality Indicators**: Completeness and accuracy scores
- **Governance Policies**: Access rules and data sensitivity

## 📊 Approach 1: Automated Population Service

**Best for**: Large databases, initial population, AI-enhanced metadata

### Using the API

```bash
# Populate all tables automatically
POST /api/business-metadata/populate-all?useAI=true&overwriteExisting=false

# Populate a specific table
POST /api/business-metadata/populate-table/common/tbl_Daily_actions?useAI=true

# Preview metadata without saving
GET /api/business-metadata/preview/common/tbl_Daily_actions?useAI=true
```

### Using the Service Directly

```csharp
// Inject the service
private readonly BusinessMetadataPopulationService _populationService;

// Populate all tables
var result = await _populationService.PopulateAllTablesAsync(useAI: true, overwriteExisting: false);

// Populate specific table
var tableInfo = new DatabaseTableInfo { SchemaName = "common", TableName = "tbl_Daily_actions" };
var tableResult = await _populationService.PopulateTableMetadataAsync(tableInfo, useAI: true);
```

### Features

- ✅ **AI-Enhanced**: Uses LLM to generate intelligent business descriptions
- ✅ **Rule-Based Fallback**: Falls back to pattern-based analysis if AI fails
- ✅ **Batch Processing**: Can process all tables at once
- ✅ **Smart Detection**: Automatically detects domains, importance, and sensitivity
- ✅ **Incremental**: Skips existing metadata unless overwrite is specified

## 📝 Approach 2: Manual SQL Population

**Best for**: Precise control, specific tables, immediate results

### Step 1: Run Discovery Query

```sql
-- Execute this to see what tables need metadata
EXEC sp_executesql N'
SELECT 
    t.TABLE_SCHEMA as SchemaName,
    t.TABLE_NAME as TableName,
    (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = t.TABLE_SCHEMA AND TABLE_NAME = t.TABLE_NAME) as ColumnCount,
    CASE 
        WHEN bti.Id IS NOT NULL THEN ''Has Metadata''
        ELSE ''Needs Metadata''
    END as MetadataStatus
FROM INFORMATION_SCHEMA.TABLES t
LEFT JOIN [dbo].[BusinessTableInfo] bti ON t.TABLE_SCHEMA = bti.SchemaName AND t.TABLE_NAME = bti.TableName
WHERE t.TABLE_TYPE = ''BASE TABLE''
ORDER BY CASE WHEN bti.Id IS NULL THEN 0 ELSE 1 END, t.TABLE_SCHEMA, t.TABLE_NAME;'
```

### Step 2: Use SQL Templates

```sql
-- Template for populating table metadata
MERGE [dbo].[BusinessTableInfo] AS target
USING (VALUES (
    'common',                                              -- SchemaName
    'tbl_Daily_actions',                                   -- TableName
    'Main statistics table holding all player statistics aggregated by player by day', -- BusinessPurpose
    'Core table for daily reporting and player activity analysis', -- BusinessContext
    'Daily player activity reporting and financial analysis', -- PrimaryUseCase
    '["Daily revenue reports", "Player activity analysis"]', -- CommonQueryPatterns (JSON)
    'Data is aggregated daily. Always filter by date ranges for performance.', -- BusinessRules
    'Gaming',                                              -- DomainClassification
    '["Daily Stats", "Player Statistics", "Gaming Metrics"]', -- NaturalLanguageAliases (JSON)
    0.9,                                                   -- ImportanceScore (0.0-1.0)
    0.8,                                                   -- UsageFrequency (0.0-1.0)
    'Business Team',                                       -- BusinessOwner
    '["Internal", "Daily Reporting"]',                     -- DataGovernancePolicies (JSON)
    'System'                                               -- CreatedBy
)) AS source (SchemaName, TableName, BusinessPurpose, BusinessContext, PrimaryUseCase, CommonQueryPatterns, BusinessRules, DomainClassification, NaturalLanguageAliases, ImportanceScore, UsageFrequency, BusinessOwner, DataGovernancePolicies, CreatedBy)
ON target.SchemaName = source.SchemaName AND target.TableName = source.TableName
WHEN MATCHED THEN UPDATE SET [... update fields ...]
WHEN NOT MATCHED THEN INSERT [... insert fields ...];
```

### Features

- ✅ **Immediate Results**: Changes take effect immediately
- ✅ **Precise Control**: Exact control over all metadata fields
- ✅ **Version Control**: SQL scripts can be version controlled
- ✅ **Batch Execution**: Multiple tables can be processed in one script

## 📋 Approach 3: CSV Bulk Import

**Best for**: Collaborative metadata definition, bulk updates, business user input

### Step 1: Download Template

Use the provided CSV template: `BusinessMetadataTemplate.csv`

### Step 2: Fill in Business Information

```csv
SchemaName,TableName,BusinessPurpose,BusinessContext,PrimaryUseCase,DomainClassification,NaturalLanguageAliases,ImportanceScore,UsageFrequency,BusinessOwner,DataGovernancePolicies
common,tbl_Daily_actions,"Main statistics table","Core daily reporting table","Daily reporting",Gaming,"Daily Stats|Player Statistics",0.9,0.8,Business Team,Internal|Daily Reporting
common,tbl_users,"User account information","Customer registration data","User management",Customer,"Users|Customers|Players",0.95,0.7,Customer Team,Confidential|GDPR
```

### Step 3: Import Using SQL

```sql
-- Create temporary table and bulk insert
CREATE TABLE #TempMetadata (
    SchemaName NVARCHAR(128),
    TableName NVARCHAR(128),
    BusinessPurpose NVARCHAR(500),
    BusinessContext NVARCHAR(2000),
    PrimaryUseCase NVARCHAR(500),
    DomainClassification NVARCHAR(1000),
    NaturalLanguageAliases NVARCHAR(2000),
    ImportanceScore DECIMAL(5,4),
    UsageFrequency DECIMAL(5,4),
    BusinessOwner NVARCHAR(500),
    DataGovernancePolicies NVARCHAR(1000)
);

-- Bulk insert from CSV
BULK INSERT #TempMetadata
FROM 'C:\path\to\your\BusinessMetadata.csv'
WITH (
    FIELDTERMINATOR = ',',
    ROWTERMINATOR = '\n',
    FIRSTROW = 2,
    TABLOCK
);

-- Merge into BusinessTableInfo
MERGE [dbo].[BusinessTableInfo] AS target
USING #TempMetadata AS source
ON target.SchemaName = source.SchemaName AND target.TableName = source.TableName
WHEN MATCHED THEN UPDATE SET [... update fields ...]
WHEN NOT MATCHED THEN INSERT [... insert fields ...];

DROP TABLE #TempMetadata;
```

### Features

- ✅ **Collaborative**: Business users can contribute metadata
- ✅ **Bulk Processing**: Handle many tables at once
- ✅ **Structured Format**: Consistent data entry
- ✅ **Review Process**: Can be reviewed before import

## 🏗️ Approach 4: Programmatic Population

**Best for**: Integration with existing systems, custom business logic

### Example C# Implementation

```csharp
public async Task PopulateFromBusinessSystemAsync()
{
    // Get table information from your business system
    var businessTables = await _businessSystemService.GetTableDefinitionsAsync();
    
    foreach (var businessTable in businessTables)
    {
        var tableEntity = new BusinessTableInfoEntity
        {
            SchemaName = businessTable.Schema,
            TableName = businessTable.Name,
            BusinessPurpose = businessTable.Purpose,
            BusinessContext = businessTable.Context,
            DomainClassification = businessTable.Domain,
            ImportanceScore = businessTable.Priority / 10.0m,
            BusinessOwner = businessTable.Owner,
            CreatedBy = "BusinessSystemSync",
            CreatedDate = DateTime.UtcNow
        };
        
        _context.BusinessTableInfo.Add(tableEntity);
    }
    
    await _context.SaveChangesAsync();
}
```

### Features

- ✅ **System Integration**: Sync with existing business systems
- ✅ **Custom Logic**: Implement organization-specific rules
- ✅ **Automated Updates**: Keep metadata in sync with business changes
- ✅ **Validation**: Add custom validation logic

## 🎯 Recommended Workflow

### Phase 1: Initial Population
1. **Run Discovery Query** to identify tables needing metadata
2. **Use Automated Service** for initial population of all tables
3. **Review and Refine** the generated metadata

### Phase 2: Business Refinement
1. **Collaborate with Business Users** to refine descriptions
2. **Use CSV Import** for bulk updates from business input
3. **Add Domain-Specific Terms** to business glossary

### Phase 3: Ongoing Maintenance
1. **Set up Automated Monitoring** for new tables
2. **Implement Feedback Loops** from query results
3. **Regular Reviews** with business stakeholders

## 📊 Quality Guidelines

### Business Purpose
- **Clear and Concise**: 1-2 sentences explaining what the table stores
- **Business Language**: Use terms business users understand
- **Specific**: Avoid generic terms like "data table"

### Domain Classification
- **Finance**: Revenue, costs, payments, financial metrics
- **Customer**: User data, demographics, preferences
- **Gaming**: Bets, games, player activity, gaming metrics
- **Analytics**: Reports, summaries, aggregated data
- **Security**: Audit logs, access controls, compliance

### Importance Score (0.0 - 1.0)
- **0.9-1.0**: Critical business tables (revenue, customers)
- **0.7-0.8**: Important operational tables
- **0.5-0.6**: Supporting tables
- **0.3-0.4**: Configuration/lookup tables
- **0.1-0.2**: Temporary/staging tables

### Usage Frequency (0.0 - 1.0)
- **0.8-1.0**: Daily reporting tables
- **0.6-0.7**: Weekly/monthly reporting
- **0.4-0.5**: Ad-hoc analysis tables
- **0.2-0.3**: Occasional use
- **0.0-0.1**: Rarely accessed

## 🚀 Next Steps

1. **Choose Your Approach**: Select the method that best fits your organization
2. **Start with High-Priority Tables**: Focus on most important business tables first
3. **Test the Semantic Layer**: Use the populated metadata to test query generation
4. **Iterate and Improve**: Refine metadata based on query results and user feedback
5. **Establish Governance**: Create processes for ongoing metadata maintenance

The semantic layer will become more intelligent and accurate as you add richer business metadata!
