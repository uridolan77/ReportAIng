# ReportAIng Database Schema Design

## 🗄️ Database Overview

The ReportAIng system uses a multi-database architecture with a primary system database for business metadata and AI configurations, plus external connections to business data sources.

**Primary Database**: `BIReportingCopilot_Dev`  
**External Data**: `DailyActionsDB` (Remote: 185.64.56.157)

## 📊 Entity Relationship Diagram

```
┌─────────────────────┐    ┌─────────────────────┐    ┌─────────────────────┐
│   BusinessTableInfo │────│ BusinessColumnInfo  │    │   BusinessGlossary  │
│                     │    │                     │    │                     │
│ + Id (PK)           │    │ + Id (PK)           │    │ + Id (PK)           │
│ + TableName         │    │ + TableInfoId (FK)  │    │ + Term              │
│ + SchemaName        │    │ + ColumnName        │    │ + Definition        │
│ + BusinessPurpose   │    │ + BusinessMeaning   │    │ + Category          │
│ + BusinessContext   │    │ + BusinessContext   │    │ + Domain            │
│ + PrimaryUseCase    │    │ + DataExamples      │    │ + Synonyms          │
│ + BusinessRules     │    │ + ValidationRules   │    │ + RelatedTerms      │
│ + IsActive          │    │ + IsKeyColumn       │    │ + IsActive          │
│ + CreatedDate       │    │ + IsActive          │    │ + CreatedDate       │
│ + UpdatedDate       │    │ + CreatedDate       │    │ + UpdatedDate       │
│ + CreatedBy         │    │ + UpdatedDate       │    │ + CreatedBy         │
│ + UpdatedBy         │    │ + CreatedBy         │    │ + UpdatedBy         │
└─────────────────────┘    │ + UpdatedBy         │    └─────────────────────┘
                           └─────────────────────┘
                                     │
                                     │ 1:N
                                     │
┌─────────────────────┐    ┌─────────────────────┐    ┌─────────────────────┐
│   PromptTemplates   │    │   LLMModelConfigs   │    │   LLMProviderConfigs│
│                     │    │                     │    │                     │
│ + Id (PK)           │    │ + Id (PK)           │    │ + Id (PK)           │
│ + Name              │    │ + ModelId           │    │ + ProviderId        │
│ + Version           │    │ + ProviderId (FK)   │    │ + ProviderName      │
│ + Content           │    │ + ModelName         │    │ + BaseUrl           │
│ + Description       │    │ + MaxTokens         │    │ + ApiKeyName        │
│ + Category          │    │ + Temperature       │    │ + IsActive          │
│ + Parameters        │    │ + TopP              │    │ + SupportedModels   │
│ + IsActive          │    │ + FrequencyPenalty  │    │ + RateLimits        │
│ + UsageCount        │    │ + PresencePenalty   │    │ + CreatedDate       │
│ + CreatedDate       │    │ + IsActive          │    │ + UpdatedDate       │
│ + UpdatedDate       │    │ + CreatedDate       │    │ + CreatedBy         │
│ + CreatedBy         │    │ + UpdatedDate       │    │ + UpdatedBy         │
│ + UpdatedBy         │    │ + CreatedBy         │    └─────────────────────┘
└─────────────────────┘    │ + UpdatedBy         │
                           └─────────────────────┘
                                     │
                                     │ N:1
                                     │
┌─────────────────────┐    ┌─────────────────────┐    ┌─────────────────────┐
│    LLMUsageLogs     │    │    QueryPatterns    │    │   UserProfiles      │
│                     │    │                     │    │                     │
│ + Id (PK)           │    │ + Id (PK)           │    │ + Id (PK)           │
│ + UserId            │    │ + PatternName       │    │ + UserId            │
│ + ModelId (FK)      │    │ + Description       │    │ + UserName          │
│ + ProviderId (FK)   │    │ + QueryTemplate     │    │ + Email             │
│ + RequestTokens     │    │ + BusinessDomain    │    │ + Role              │
│ + ResponseTokens    │    │ + UseCase           │    │ + Preferences       │
│ + TotalCost         │    │ + ExampleQueries    │    │ + DefaultDomain     │
│ + ExecutionTime     │    │ + SuccessRate       │    │ + IsActive          │
│ + RequestType       │    │ + UsageCount        │    │ + LastLoginDate     │
│ + Success           │    │ + IsActive          │    │ + CreatedDate       │
│ + ErrorMessage      │    │ + CreatedDate       │    │ + UpdatedDate       │
│ + CreatedDate       │    │ + UpdatedDate       │    │ + CreatedBy         │
│ + Metadata          │    │ + CreatedBy         │    │ + UpdatedBy         │
└─────────────────────┘    │ + UpdatedBy         │    └─────────────────────┘
                           └─────────────────────┘
```

## 🏗️ Core Business Metadata Tables

### BusinessTableInfo

**Purpose**: Central repository for business table metadata and semantic information

```sql
CREATE TABLE [dbo].[BusinessTableInfo] (
    [Id] bigint IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [TableName] nvarchar(128) NOT NULL,
    [SchemaName] nvarchar(128) NOT NULL DEFAULT 'common',
    [BusinessPurpose] nvarchar(500) NULL,
    [BusinessContext] nvarchar(2000) NULL,
    [PrimaryUseCase] nvarchar(500) NULL,
    [CommonQueryPatterns] nvarchar(4000) NULL, -- JSON array
    [BusinessRules] nvarchar(2000) NULL,
    
    -- Enhanced semantic metadata
    [DomainClassification] nvarchar(200) NULL,
    [NaturalLanguageAliases] nvarchar(2000) NULL, -- JSON array
    [BusinessProcesses] nvarchar(2000) NULL, -- JSON array
    [AnalyticalUseCases] nvarchar(2000) NULL, -- JSON array
    [ReportingCategories] nvarchar(1000) NULL, -- JSON array
    [VectorSearchKeywords] nvarchar(2000) NULL, -- JSON array
    [BusinessGlossaryTerms] nvarchar(2000) NULL, -- JSON array
    [LLMContextHints] nvarchar(2000) NULL, -- JSON array
    [QueryComplexityHints] nvarchar(1000) NULL, -- JSON array
    [SemanticRelationships] nvarchar(4000) NULL, -- JSON object
    [UsagePatterns] nvarchar(2000) NULL, -- JSON object
    [DataQualityIndicators] nvarchar(2000) NULL, -- JSON object
    [RelationshipSemantics] nvarchar(2000) NULL, -- JSON object
    [DataGovernancePolicies] nvarchar(2000) NULL, -- JSON object
    
    -- Scoring and metrics
    [ImportanceScore] decimal(5,2) DEFAULT 0.0,
    [UsageFrequency] decimal(5,2) DEFAULT 0.0,
    [SemanticCoverageScore] decimal(5,2) DEFAULT 0.0,
    [LastAnalyzed] datetime2 NULL,
    [BusinessOwner] nvarchar(256) NULL,
    
    -- Audit fields
    [IsActive] bit NOT NULL DEFAULT 1,
    [CreatedDate] datetime2 NOT NULL DEFAULT GETUTCDATE(),
    [UpdatedDate] datetime2 NULL,
    [CreatedBy] nvarchar(256) NOT NULL,
    [UpdatedBy] nvarchar(256) NULL,
    
    -- Indexes
    INDEX IX_BusinessTableInfo_TableSchema NONCLUSTERED ([SchemaName], [TableName]),
    INDEX IX_BusinessTableInfo_Domain NONCLUSTERED ([DomainClassification]),
    INDEX IX_BusinessTableInfo_Active NONCLUSTERED ([IsActive]) WHERE [IsActive] = 1
);
```

### BusinessColumnInfo

**Purpose**: Detailed column-level business metadata and semantic information

```sql
CREATE TABLE [dbo].[BusinessColumnInfo] (
    [Id] bigint IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [TableInfoId] bigint NOT NULL,
    [ColumnName] nvarchar(128) NOT NULL,
    [BusinessMeaning] nvarchar(500) NULL,
    [BusinessContext] nvarchar(1000) NULL,
    [DataExamples] nvarchar(2000) NULL, -- JSON array
    [ValidationRules] nvarchar(1000) NULL,
    
    -- Enhanced semantic metadata
    [NaturalLanguageAliases] nvarchar(1000) NULL, -- JSON array
    [BusinessDataType] nvarchar(500) NULL,
    [SemanticTags] nvarchar(1000) NULL, -- JSON array
    [AnalyticalContext] nvarchar(500) NULL,
    [ReportingLabels] nvarchar(1000) NULL, -- JSON array
    [DataClassification] nvarchar(200) NULL,
    [SensitivityLevel] nvarchar(100) NULL,
    [BusinessGlossaryMappings] nvarchar(1000) NULL, -- JSON array
    [CalculationLogic] nvarchar(2000) NULL,
    [DataLineage] nvarchar(2000) NULL, -- JSON object
    [QualityRules] nvarchar(1000) NULL, -- JSON array
    [UsageContext] nvarchar(1000) NULL, -- JSON object
    
    -- Column characteristics
    [IsKeyColumn] bit NOT NULL DEFAULT 0,
    [IsSensitiveData] bit NOT NULL DEFAULT 0,
    [IsCalculated] bit NOT NULL DEFAULT 0,
    [IsAggregatable] bit NOT NULL DEFAULT 1,
    [IsFilterable] bit NOT NULL DEFAULT 1,
    [IsSortable] bit NOT NULL DEFAULT 1,
    
    -- Metrics
    [UsageFrequency] decimal(5,2) DEFAULT 0.0,
    [DataQualityScore] decimal(5,2) DEFAULT 0.0,
    [BusinessRelevanceScore] decimal(5,2) DEFAULT 0.0,
    
    -- Audit fields
    [IsActive] bit NOT NULL DEFAULT 1,
    [CreatedDate] datetime2 NOT NULL DEFAULT GETUTCDATE(),
    [UpdatedDate] datetime2 NULL,
    [CreatedBy] nvarchar(256) NOT NULL,
    [UpdatedBy] nvarchar(256) NULL,
    
    -- Foreign key
    CONSTRAINT FK_BusinessColumnInfo_TableInfo 
        FOREIGN KEY ([TableInfoId]) REFERENCES [BusinessTableInfo]([Id]),
    
    -- Indexes
    INDEX IX_BusinessColumnInfo_TableId NONCLUSTERED ([TableInfoId]),
    INDEX IX_BusinessColumnInfo_ColumnName NONCLUSTERED ([ColumnName]),
    INDEX IX_BusinessColumnInfo_Active NONCLUSTERED ([IsActive]) WHERE [IsActive] = 1
);
```

### BusinessGlossary

**Purpose**: Business term definitions and relationships for semantic understanding

```sql
CREATE TABLE [dbo].[BusinessGlossary] (
    [Id] bigint IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [Term] nvarchar(200) NOT NULL,
    [Definition] nvarchar(2000) NOT NULL,
    [Category] nvarchar(200) NULL,
    [Domain] nvarchar(200) NULL,
    [Synonyms] nvarchar(1000) NULL, -- JSON array
    [RelatedTerms] nvarchar(1000) NULL, -- JSON array
    [BusinessContext] nvarchar(1000) NULL,
    [UsageExamples] nvarchar(2000) NULL, -- JSON array
    [DataSources] nvarchar(1000) NULL, -- JSON array
    [CalculationRules] nvarchar(2000) NULL,
    [BusinessRules] nvarchar(2000) NULL,
    [Stakeholders] nvarchar(500) NULL, -- JSON array
    [ApprovalStatus] nvarchar(50) DEFAULT 'Draft',
    [ApprovedBy] nvarchar(256) NULL,
    [ApprovedDate] datetime2 NULL,
    [Version] nvarchar(20) DEFAULT '1.0',
    
    -- Audit fields
    [IsActive] bit NOT NULL DEFAULT 1,
    [CreatedDate] datetime2 NOT NULL DEFAULT GETUTCDATE(),
    [UpdatedDate] datetime2 NULL,
    [CreatedBy] nvarchar(256) NOT NULL,
    [UpdatedBy] nvarchar(256) NULL,
    
    -- Indexes
    INDEX IX_BusinessGlossary_Term NONCLUSTERED ([Term]),
    INDEX IX_BusinessGlossary_Category NONCLUSTERED ([Category]),
    INDEX IX_BusinessGlossary_Domain NONCLUSTERED ([Domain]),
    INDEX IX_BusinessGlossary_Active NONCLUSTERED ([IsActive]) WHERE [IsActive] = 1
);
```

## 🤖 AI & LLM Configuration Tables

### LLMProviderConfigs

**Purpose**: Configuration for different LLM providers (OpenAI, Azure OpenAI, etc.)

```sql
CREATE TABLE [dbo].[LLMProviderConfigs] (
    [Id] bigint IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [ProviderId] nvarchar(100) NOT NULL UNIQUE,
    [ProviderName] nvarchar(200) NOT NULL,
    [BaseUrl] nvarchar(500) NULL,
    [ApiKeyName] nvarchar(100) NULL,
    [AuthenticationType] nvarchar(50) DEFAULT 'ApiKey',
    [SupportedModels] nvarchar(2000) NULL, -- JSON array
    [RateLimits] nvarchar(1000) NULL, -- JSON object
    [DefaultParameters] nvarchar(2000) NULL, -- JSON object
    [HealthCheckEndpoint] nvarchar(500) NULL,
    [Priority] int DEFAULT 1,
    [CostPerToken] decimal(10,8) DEFAULT 0.0,
    
    -- Audit fields
    [IsActive] bit NOT NULL DEFAULT 1,
    [CreatedDate] datetime2 NOT NULL DEFAULT GETUTCDATE(),
    [UpdatedDate] datetime2 NULL,
    [CreatedBy] nvarchar(256) NOT NULL,
    [UpdatedBy] nvarchar(256) NULL
);
```

### LLMModelConfigs

**Purpose**: Specific model configurations and parameters

```sql
CREATE TABLE [dbo].[LLMModelConfigs] (
    [Id] bigint IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [ModelId] nvarchar(100) NOT NULL,
    [ProviderId] bigint NOT NULL,
    [ModelName] nvarchar(200) NOT NULL,
    [ModelVersion] nvarchar(50) NULL,
    [MaxTokens] int DEFAULT 4000,
    [Temperature] decimal(3,2) DEFAULT 0.7,
    [TopP] decimal(3,2) DEFAULT 1.0,
    [FrequencyPenalty] decimal(3,2) DEFAULT 0.0,
    [PresencePenalty] decimal(3,2) DEFAULT 0.0,
    [UseCase] nvarchar(200) NULL, -- SQL_GENERATION, INSIGHT_GENERATION, etc.
    [CostPerInputToken] decimal(10,8) DEFAULT 0.0,
    [CostPerOutputToken] decimal(10,8) DEFAULT 0.0,
    [PerformanceRating] decimal(3,2) DEFAULT 0.0,
    
    -- Audit fields
    [IsActive] bit NOT NULL DEFAULT 1,
    [CreatedDate] datetime2 NOT NULL DEFAULT GETUTCDATE(),
    [UpdatedDate] datetime2 NULL,
    [CreatedBy] nvarchar(256) NOT NULL,
    [UpdatedBy] nvarchar(256) NULL,
    
    -- Foreign key
    CONSTRAINT FK_LLMModelConfigs_Provider 
        FOREIGN KEY ([ProviderId]) REFERENCES [LLMProviderConfigs]([Id])
);
```

### PromptTemplates

**Purpose**: Template management for different types of AI prompts

```sql
CREATE TABLE [dbo].[PromptTemplates] (
    [Id] bigint IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [Name] nvarchar(200) NOT NULL,
    [Version] nvarchar(20) DEFAULT '1.0',
    [Content] nvarchar(MAX) NOT NULL,
    [Description] nvarchar(1000) NULL,
    [Category] nvarchar(100) NULL, -- SQL_GENERATION, INSIGHT_GENERATION, etc.
    [UseCase] nvarchar(200) NULL,
    [Parameters] nvarchar(2000) NULL, -- JSON object
    [ExpectedTokens] int NULL,
    [SuccessRate] decimal(5,2) DEFAULT 0.0,
    [UsageCount] bigint DEFAULT 0,
    [LastUsed] datetime2 NULL,
    
    -- Audit fields
    [IsActive] bit NOT NULL DEFAULT 1,
    [CreatedDate] datetime2 NOT NULL DEFAULT GETUTCDATE(),
    [UpdatedDate] datetime2 NULL,
    [CreatedBy] nvarchar(256) NOT NULL,
    [UpdatedBy] nvarchar(256) NULL,
    
    -- Indexes
    INDEX IX_PromptTemplates_Name NONCLUSTERED ([Name]),
    INDEX IX_PromptTemplates_Category NONCLUSTERED ([Category]),
    INDEX IX_PromptTemplates_Active NONCLUSTERED ([IsActive]) WHERE [IsActive] = 1
);
```

## 📊 Analytics & Usage Tracking Tables

### LLMUsageLogs

**Purpose**: Track LLM usage for cost optimization and analytics

```sql
CREATE TABLE [dbo].[LLMUsageLogs] (
    [Id] bigint IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [UserId] nvarchar(256) NULL,
    [SessionId] nvarchar(100) NULL,
    [ModelId] bigint NOT NULL,
    [ProviderId] bigint NOT NULL,
    [RequestType] nvarchar(100) NOT NULL, -- SQL_GENERATION, COMPLETION, etc.
    [RequestTokens] int NOT NULL,
    [ResponseTokens] int NOT NULL,
    [TotalTokens] int NOT NULL,
    [TotalCost] decimal(10,6) NOT NULL,
    [ExecutionTimeMs] int NOT NULL,
    [Success] bit NOT NULL,
    [ErrorMessage] nvarchar(2000) NULL,
    [RequestHash] nvarchar(64) NULL, -- For caching
    [Metadata] nvarchar(MAX) NULL, -- JSON object
    [CreatedDate] datetime2 NOT NULL DEFAULT GETUTCDATE(),
    
    -- Foreign keys
    CONSTRAINT FK_LLMUsageLogs_Model 
        FOREIGN KEY ([ModelId]) REFERENCES [LLMModelConfigs]([Id]),
    CONSTRAINT FK_LLMUsageLogs_Provider 
        FOREIGN KEY ([ProviderId]) REFERENCES [LLMProviderConfigs]([Id]),
    
    -- Indexes
    INDEX IX_LLMUsageLogs_UserId NONCLUSTERED ([UserId]),
    INDEX IX_LLMUsageLogs_Date NONCLUSTERED ([CreatedDate]),
    INDEX IX_LLMUsageLogs_Model NONCLUSTERED ([ModelId]),
    INDEX IX_LLMUsageLogs_Hash NONCLUSTERED ([RequestHash])
);
```

### QueryPatterns

**Purpose**: Store and analyze successful query patterns for learning

```sql
CREATE TABLE [dbo].[QueryPatterns] (
    [Id] bigint IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [PatternName] nvarchar(200) NOT NULL,
    [Description] nvarchar(1000) NULL,
    [QueryTemplate] nvarchar(MAX) NOT NULL,
    [BusinessDomain] nvarchar(200) NULL,
    [UseCase] nvarchar(200) NULL,
    [ExampleQueries] nvarchar(MAX) NULL, -- JSON array
    [SuccessRate] decimal(5,2) DEFAULT 0.0,
    [UsageCount] bigint DEFAULT 0,
    [AverageExecutionTime] int DEFAULT 0,
    [ComplexityScore] decimal(3,2) DEFAULT 0.0,
    [BusinessValue] decimal(3,2) DEFAULT 0.0,
    
    -- Audit fields
    [IsActive] bit NOT NULL DEFAULT 1,
    [CreatedDate] datetime2 NOT NULL DEFAULT GETUTCDATE(),
    [UpdatedDate] datetime2 NULL,
    [CreatedBy] nvarchar(256) NOT NULL,
    [UpdatedBy] nvarchar(256) NULL
);
```

## 🔗 Relationships and Constraints

### Primary Relationships

1. **BusinessTableInfo → BusinessColumnInfo** (1:N)
   - One table can have multiple columns
   - Cascade delete for data integrity

2. **LLMProviderConfigs → LLMModelConfigs** (1:N)
   - One provider can have multiple models
   - Restrict delete to prevent orphaned models

3. **LLMModelConfigs → LLMUsageLogs** (1:N)
   - Track usage per model
   - Restrict delete for audit trail

### Data Integrity Rules

- All tables include audit fields (Created/Updated Date/By)
- Soft delete pattern using IsActive flag
- JSON validation for complex fields
- Proper indexing for performance
- Foreign key constraints for referential integrity

## 🎯 Design Principles

1. **Semantic Richness**: Extensive metadata for AI understanding
2. **Audit Trail**: Complete tracking of changes and usage
3. **Performance**: Optimized indexes and query patterns
4. **Flexibility**: JSON fields for extensible metadata
5. **Scalability**: Designed for growth and high volume
6. **Data Quality**: Validation rules and constraints
