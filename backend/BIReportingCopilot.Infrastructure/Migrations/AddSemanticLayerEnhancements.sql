-- Migration script for Phase 2: Semantic Layer Enhancement
-- Adds enhanced semantic metadata fields to existing entities and creates new semantic layer tables

PRINT '=== PHASE 2: SEMANTIC LAYER ENHANCEMENT MIGRATION ===';
PRINT 'Starting migration at: ' + CONVERT(VARCHAR, GETDATE(), 120);
PRINT '';

-- =====================================================
-- 0. Create base tables if they don't exist
-- =====================================================

-- Create BusinessTableInfo table if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[BusinessTableInfo]') AND type in (N'U'))
BEGIN
    PRINT 'Creating BusinessTableInfo table...';
    CREATE TABLE [dbo].[BusinessTableInfo] (
        [Id] bigint IDENTITY(1,1) NOT NULL,
        [TableName] nvarchar(128) NOT NULL,
        [SchemaName] nvarchar(128) NOT NULL DEFAULT 'common',
        [BusinessPurpose] nvarchar(500) NULL,
        [BusinessContext] nvarchar(2000) NULL,
        [PrimaryUseCase] nvarchar(500) NULL,
        [CommonQueryPatterns] nvarchar(4000) NULL,
        [BusinessRules] nvarchar(2000) NULL,
        [IsActive] bit NOT NULL DEFAULT 1,
        [CreatedDate] datetime2 NOT NULL DEFAULT GETUTCDATE(),
        [UpdatedDate] datetime2 NULL,
        [CreatedBy] nvarchar(256) NOT NULL DEFAULT 'System',
        [UpdatedBy] nvarchar(256) NULL,
        CONSTRAINT [PK_BusinessTableInfo] PRIMARY KEY CLUSTERED ([Id] ASC)
    );
    PRINT '✅ BusinessTableInfo table created';
END
ELSE
BEGIN
    PRINT '✅ BusinessTableInfo table already exists';
END

-- Create BusinessColumnInfo table if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[BusinessColumnInfo]') AND type in (N'U'))
BEGIN
    PRINT 'Creating BusinessColumnInfo table...';
    CREATE TABLE [dbo].[BusinessColumnInfo] (
        [Id] bigint IDENTITY(1,1) NOT NULL,
        [TableInfoId] bigint NOT NULL,
        [ColumnName] nvarchar(128) NOT NULL,
        [BusinessMeaning] nvarchar(500) NULL,
        [BusinessContext] nvarchar(1000) NULL,
        [DataExamples] nvarchar(2000) NULL,
        [ValidationRules] nvarchar(1000) NULL,
        [IsKeyColumn] bit NOT NULL DEFAULT 0,
        [IsActive] bit NOT NULL DEFAULT 1,
        [CreatedDate] datetime2 NOT NULL DEFAULT GETUTCDATE(),
        [UpdatedDate] datetime2 NULL,
        [CreatedBy] nvarchar(256) NOT NULL DEFAULT 'System',
        [UpdatedBy] nvarchar(256) NULL,
        CONSTRAINT [PK_BusinessColumnInfo] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [FK_BusinessColumnInfo_BusinessTableInfo] FOREIGN KEY ([TableInfoId]) REFERENCES [dbo].[BusinessTableInfo] ([Id]) ON DELETE CASCADE
    );
    PRINT '✅ BusinessColumnInfo table created';
END
ELSE
BEGIN
    PRINT '✅ BusinessColumnInfo table already exists';
END

-- Create BusinessGlossary table if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[BusinessGlossary]') AND type in (N'U'))
BEGIN
    PRINT 'Creating BusinessGlossary table...';
    CREATE TABLE [dbo].[BusinessGlossary] (
        [Id] bigint IDENTITY(1,1) NOT NULL,
        [Term] nvarchar(200) NOT NULL,
        [Definition] nvarchar(2000) NOT NULL,
        [BusinessContext] nvarchar(1000) NULL,
        [Synonyms] nvarchar(1000) NULL,
        [RelatedTerms] nvarchar(1000) NULL,
        [Category] nvarchar(100) NULL,
        [IsActive] bit NOT NULL DEFAULT 1,
        [UsageCount] int NOT NULL DEFAULT 0,
        [LastUsed] datetime2 NULL,
        [CreatedDate] datetime2 NOT NULL DEFAULT GETUTCDATE(),
        [UpdatedDate] datetime2 NULL,
        [CreatedBy] nvarchar(256) NOT NULL DEFAULT 'System',
        [UpdatedBy] nvarchar(256) NULL,
        CONSTRAINT [PK_BusinessGlossary] PRIMARY KEY CLUSTERED ([Id] ASC)
    );
    PRINT '✅ BusinessGlossary table created';
END
ELSE
BEGIN
    PRINT '✅ BusinessGlossary table already exists';
END

-- Create SchemaMetadata table if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SchemaMetadata]') AND type in (N'U'))
BEGIN
    PRINT 'Creating SchemaMetadata table...';
    CREATE TABLE [dbo].[SchemaMetadata] (
        [Id] bigint IDENTITY(1,1) NOT NULL,
        [DatabaseName] nvarchar(128) NOT NULL DEFAULT '',
        [SchemaName] nvarchar(128) NOT NULL DEFAULT '',
        [TableName] nvarchar(128) NOT NULL DEFAULT '',
        [ColumnName] nvarchar(128) NOT NULL DEFAULT '',
        [DataType] nvarchar(128) NOT NULL DEFAULT '',
        [IsNullable] bit NOT NULL DEFAULT 0,
        [IsPrimaryKey] bit NOT NULL DEFAULT 0,
        [IsForeignKey] bit NOT NULL DEFAULT 0,
        [BusinessDescription] nvarchar(500) NULL,
        [SemanticTags] nvarchar(500) NULL,
        [SampleValues] nvarchar(500) NULL,
        [LastUpdated] datetime2 NOT NULL DEFAULT GETUTCDATE(),
        [IsActive] bit NOT NULL DEFAULT 1,
        CONSTRAINT [PK_SchemaMetadata] PRIMARY KEY CLUSTERED ([Id] ASC)
    );
    PRINT '✅ SchemaMetadata table created';
END
ELSE
BEGIN
    PRINT '✅ SchemaMetadata table already exists';
END

PRINT '';
PRINT '=== BASE TABLES VERIFICATION COMPLETE ===';
PRINT '';

-- =====================================================
-- 1. Enhance BusinessTableInfo with semantic metadata
-- =====================================================

PRINT 'Enhancing BusinessTableInfo with semantic metadata...';

-- Add new columns to BusinessTableInfo table
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[BusinessTableInfo]') AND name = 'DomainClassification')
BEGIN
    ALTER TABLE [dbo].[BusinessTableInfo] ADD [DomainClassification] NVARCHAR(1000) NOT NULL DEFAULT '';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[BusinessTableInfo]') AND name = 'NaturalLanguageAliases')
BEGIN
    ALTER TABLE [dbo].[BusinessTableInfo] ADD [NaturalLanguageAliases] NVARCHAR(2000) NOT NULL DEFAULT '';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[BusinessTableInfo]') AND name = 'UsagePatterns')
BEGIN
    ALTER TABLE [dbo].[BusinessTableInfo] ADD [UsagePatterns] NVARCHAR(4000) NOT NULL DEFAULT '';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[BusinessTableInfo]') AND name = 'DataQualityIndicators')
BEGIN
    ALTER TABLE [dbo].[BusinessTableInfo] ADD [DataQualityIndicators] NVARCHAR(1000) NOT NULL DEFAULT '';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[BusinessTableInfo]') AND name = 'RelationshipSemantics')
BEGIN
    ALTER TABLE [dbo].[BusinessTableInfo] ADD [RelationshipSemantics] NVARCHAR(2000) NOT NULL DEFAULT '';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[BusinessTableInfo]') AND name = 'ImportanceScore')
BEGIN
    ALTER TABLE [dbo].[BusinessTableInfo] ADD [ImportanceScore] DECIMAL(5,4) NOT NULL DEFAULT 0.5;
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[BusinessTableInfo]') AND name = 'UsageFrequency')
BEGIN
    ALTER TABLE [dbo].[BusinessTableInfo] ADD [UsageFrequency] DECIMAL(5,4) NOT NULL DEFAULT 0.0;
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[BusinessTableInfo]') AND name = 'LastAnalyzed')
BEGIN
    ALTER TABLE [dbo].[BusinessTableInfo] ADD [LastAnalyzed] DATETIME2 NULL;
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[BusinessTableInfo]') AND name = 'BusinessOwner')
BEGIN
    ALTER TABLE [dbo].[BusinessTableInfo] ADD [BusinessOwner] NVARCHAR(500) NOT NULL DEFAULT '';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[BusinessTableInfo]') AND name = 'DataGovernancePolicies')
BEGIN
    ALTER TABLE [dbo].[BusinessTableInfo] ADD [DataGovernancePolicies] NVARCHAR(1000) NOT NULL DEFAULT '';
END

-- =====================================================
-- 2. Enhance BusinessColumnInfo with semantic metadata
-- =====================================================

-- Add new columns to BusinessColumnInfo table
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[BusinessColumnInfo]') AND name = 'NaturalLanguageAliases')
BEGIN
    ALTER TABLE [dbo].[BusinessColumnInfo] ADD [NaturalLanguageAliases] NVARCHAR(1000) NOT NULL DEFAULT '';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[BusinessColumnInfo]') AND name = 'ValueExamples')
BEGIN
    ALTER TABLE [dbo].[BusinessColumnInfo] ADD [ValueExamples] NVARCHAR(2000) NOT NULL DEFAULT '';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[BusinessColumnInfo]') AND name = 'DataLineage')
BEGIN
    ALTER TABLE [dbo].[BusinessColumnInfo] ADD [DataLineage] NVARCHAR(1000) NOT NULL DEFAULT '';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[BusinessColumnInfo]') AND name = 'CalculationRules')
BEGIN
    ALTER TABLE [dbo].[BusinessColumnInfo] ADD [CalculationRules] NVARCHAR(2000) NOT NULL DEFAULT '';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[BusinessColumnInfo]') AND name = 'SemanticTags')
BEGIN
    ALTER TABLE [dbo].[BusinessColumnInfo] ADD [SemanticTags] NVARCHAR(1000) NOT NULL DEFAULT '';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[BusinessColumnInfo]') AND name = 'BusinessDataType')
BEGIN
    ALTER TABLE [dbo].[BusinessColumnInfo] ADD [BusinessDataType] NVARCHAR(500) NOT NULL DEFAULT '';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[BusinessColumnInfo]') AND name = 'ConstraintsAndRules')
BEGIN
    ALTER TABLE [dbo].[BusinessColumnInfo] ADD [ConstraintsAndRules] NVARCHAR(1000) NOT NULL DEFAULT '';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[BusinessColumnInfo]') AND name = 'DataQualityScore')
BEGIN
    ALTER TABLE [dbo].[BusinessColumnInfo] ADD [DataQualityScore] DECIMAL(5,4) NOT NULL DEFAULT 0.0;
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[BusinessColumnInfo]') AND name = 'UsageFrequency')
BEGIN
    ALTER TABLE [dbo].[BusinessColumnInfo] ADD [UsageFrequency] DECIMAL(5,4) NOT NULL DEFAULT 0.0;
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[BusinessColumnInfo]') AND name = 'PreferredAggregation')
BEGIN
    ALTER TABLE [dbo].[BusinessColumnInfo] ADD [PreferredAggregation] NVARCHAR(500) NOT NULL DEFAULT '';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[BusinessColumnInfo]') AND name = 'RelatedBusinessTerms')
BEGIN
    ALTER TABLE [dbo].[BusinessColumnInfo] ADD [RelatedBusinessTerms] NVARCHAR(1000) NOT NULL DEFAULT '';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[BusinessColumnInfo]') AND name = 'IsSensitiveData')
BEGIN
    ALTER TABLE [dbo].[BusinessColumnInfo] ADD [IsSensitiveData] BIT NOT NULL DEFAULT 0;
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[BusinessColumnInfo]') AND name = 'IsCalculatedField')
BEGIN
    ALTER TABLE [dbo].[BusinessColumnInfo] ADD [IsCalculatedField] BIT NOT NULL DEFAULT 0;
END

-- =====================================================
-- 3. Enhance BusinessGlossary with semantic metadata
-- =====================================================

-- Add new columns to BusinessGlossary table
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[BusinessGlossary]') AND name = 'Domain')
BEGIN
    ALTER TABLE [dbo].[BusinessGlossary] ADD [Domain] NVARCHAR(200) NOT NULL DEFAULT '';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[BusinessGlossary]') AND name = 'Examples')
BEGIN
    ALTER TABLE [dbo].[BusinessGlossary] ADD [Examples] NVARCHAR(2000) NOT NULL DEFAULT '';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[BusinessGlossary]') AND name = 'MappedTables')
BEGIN
    ALTER TABLE [dbo].[BusinessGlossary] ADD [MappedTables] NVARCHAR(1000) NOT NULL DEFAULT '';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[BusinessGlossary]') AND name = 'MappedColumns')
BEGIN
    ALTER TABLE [dbo].[BusinessGlossary] ADD [MappedColumns] NVARCHAR(1000) NOT NULL DEFAULT '';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[BusinessGlossary]') AND name = 'HierarchicalRelations')
BEGIN
    ALTER TABLE [dbo].[BusinessGlossary] ADD [HierarchicalRelations] NVARCHAR(1000) NOT NULL DEFAULT '';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[BusinessGlossary]') AND name = 'PreferredCalculation')
BEGIN
    ALTER TABLE [dbo].[BusinessGlossary] ADD [PreferredCalculation] NVARCHAR(500) NOT NULL DEFAULT '';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[BusinessGlossary]') AND name = 'DisambiguationRules')
BEGIN
    ALTER TABLE [dbo].[BusinessGlossary] ADD [DisambiguationRules] NVARCHAR(1000) NOT NULL DEFAULT '';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[BusinessGlossary]') AND name = 'BusinessOwner')
BEGIN
    ALTER TABLE [dbo].[BusinessGlossary] ADD [BusinessOwner] NVARCHAR(500) NOT NULL DEFAULT '';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[BusinessGlossary]') AND name = 'RegulationReferences')
BEGIN
    ALTER TABLE [dbo].[BusinessGlossary] ADD [RegulationReferences] NVARCHAR(1000) NOT NULL DEFAULT '';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[BusinessGlossary]') AND name = 'ConfidenceScore')
BEGIN
    ALTER TABLE [dbo].[BusinessGlossary] ADD [ConfidenceScore] DECIMAL(5,4) NOT NULL DEFAULT 1.0;
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[BusinessGlossary]') AND name = 'AmbiguityScore')
BEGIN
    ALTER TABLE [dbo].[BusinessGlossary] ADD [AmbiguityScore] DECIMAL(5,4) NOT NULL DEFAULT 0.0;
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[BusinessGlossary]') AND name = 'ContextualVariations')
BEGIN
    ALTER TABLE [dbo].[BusinessGlossary] ADD [ContextualVariations] NVARCHAR(1000) NOT NULL DEFAULT '';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[BusinessGlossary]') AND name = 'LastValidated')
BEGIN
    ALTER TABLE [dbo].[BusinessGlossary] ADD [LastValidated] DATETIME2 NULL;
END

-- =====================================================
-- 4. Enhance SchemaMetadata with business context
-- =====================================================

-- Add new columns to SchemaMetadata table
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[SchemaMetadata]') AND name = 'BusinessFriendlyName')
BEGIN
    ALTER TABLE [dbo].[SchemaMetadata] ADD [BusinessFriendlyName] NVARCHAR(500) NULL;
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[SchemaMetadata]') AND name = 'NaturalLanguageDescription')
BEGIN
    ALTER TABLE [dbo].[SchemaMetadata] ADD [NaturalLanguageDescription] NVARCHAR(2000) NULL;
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[SchemaMetadata]') AND name = 'BusinessRules')
BEGIN
    ALTER TABLE [dbo].[SchemaMetadata] ADD [BusinessRules] NVARCHAR(2000) NULL;
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[SchemaMetadata]') AND name = 'ImportanceScore')
BEGIN
    ALTER TABLE [dbo].[SchemaMetadata] ADD [ImportanceScore] DECIMAL(5,4) NOT NULL DEFAULT 0.5;
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[SchemaMetadata]') AND name = 'UsageFrequency')
BEGIN
    ALTER TABLE [dbo].[SchemaMetadata] ADD [UsageFrequency] DECIMAL(5,4) NOT NULL DEFAULT 0.0;
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[SchemaMetadata]') AND name = 'RelationshipContext')
BEGIN
    ALTER TABLE [dbo].[SchemaMetadata] ADD [RelationshipContext] NVARCHAR(2000) NULL;
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[SchemaMetadata]') AND name = 'DataGovernanceLevel')
BEGIN
    ALTER TABLE [dbo].[SchemaMetadata] ADD [DataGovernanceLevel] NVARCHAR(100) NULL;
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[SchemaMetadata]') AND name = 'LastBusinessReview')
BEGIN
    ALTER TABLE [dbo].[SchemaMetadata] ADD [LastBusinessReview] DATETIME2 NULL;
END

-- =====================================================
-- 5. Create new semantic layer tables
-- =====================================================

-- Create SemanticSchemaMapping table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SemanticSchemaMapping]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[SemanticSchemaMapping] (
        [Id] BIGINT IDENTITY(1,1) NOT NULL,
        [QueryIntent] NVARCHAR(1000) NOT NULL,
        [RelevantTables] NVARCHAR(4000) NOT NULL DEFAULT '',
        [RelevantColumns] NVARCHAR(4000) NOT NULL DEFAULT '',
        [BusinessTerms] NVARCHAR(2000) NOT NULL DEFAULT '',
        [QueryCategory] NVARCHAR(1000) NOT NULL DEFAULT '',
        [ConfidenceScore] DECIMAL(5,4) NOT NULL DEFAULT 0.0,
        [SemanticVector] NVARCHAR(4000) NOT NULL DEFAULT '',
        [UsageCount] INT NOT NULL DEFAULT 0,
        [LastUsed] DATETIME2 NULL,
        [IsActive] BIT NOT NULL DEFAULT 1,
        [CreatedBy] NVARCHAR(MAX) NULL,
        [CreatedDate] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [UpdatedBy] NVARCHAR(MAX) NULL,
        [UpdatedDate] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        CONSTRAINT [PK_SemanticSchemaMapping] PRIMARY KEY CLUSTERED ([Id] ASC)
    );

    PRINT 'Created SemanticSchemaMapping table';
END

-- Create BusinessDomain table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[BusinessDomain]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[BusinessDomain] (
        [Id] BIGINT IDENTITY(1,1) NOT NULL,
        [DomainName] NVARCHAR(200) NOT NULL,
        [Description] NVARCHAR(1000) NOT NULL DEFAULT '',
        [RelatedTables] NVARCHAR(2000) NOT NULL DEFAULT '',
        [KeyConcepts] NVARCHAR(2000) NOT NULL DEFAULT '',
        [CommonQueries] NVARCHAR(1000) NOT NULL DEFAULT '',
        [BusinessOwner] NVARCHAR(500) NOT NULL DEFAULT '',
        [RelatedDomains] NVARCHAR(1000) NOT NULL DEFAULT '',
        [ImportanceScore] DECIMAL(5,4) NOT NULL DEFAULT 0.5,
        [IsActive] BIT NOT NULL DEFAULT 1,
        [CreatedBy] NVARCHAR(MAX) NULL,
        [CreatedDate] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [UpdatedBy] NVARCHAR(MAX) NULL,
        [UpdatedDate] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        CONSTRAINT [PK_BusinessDomain] PRIMARY KEY CLUSTERED ([Id] ASC)
    );

    PRINT 'Created BusinessDomain table';
END

-- =====================================================
-- 6. Create indexes for performance
-- =====================================================

-- Indexes for SemanticSchemaMapping
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[SemanticSchemaMapping]') AND name = N'IX_SemanticSchemaMapping_QueryCategory')
BEGIN
    CREATE NONCLUSTERED INDEX [IX_SemanticSchemaMapping_QueryCategory] ON [dbo].[SemanticSchemaMapping] ([QueryCategory]);
END

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[SemanticSchemaMapping]') AND name = N'IX_SemanticSchemaMapping_ConfidenceScore')
BEGIN
    CREATE NONCLUSTERED INDEX [IX_SemanticSchemaMapping_ConfidenceScore] ON [dbo].[SemanticSchemaMapping] ([ConfidenceScore] DESC);
END

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[SemanticSchemaMapping]') AND name = N'IX_SemanticSchemaMapping_UsageCount')
BEGIN
    CREATE NONCLUSTERED INDEX [IX_SemanticSchemaMapping_UsageCount] ON [dbo].[SemanticSchemaMapping] ([UsageCount] DESC);
END

-- Indexes for BusinessDomain
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[BusinessDomain]') AND name = N'IX_BusinessDomain_DomainName')
BEGIN
    CREATE NONCLUSTERED INDEX [IX_BusinessDomain_DomainName] ON [dbo].[BusinessDomain] ([DomainName]);
END

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[BusinessDomain]') AND name = N'IX_BusinessDomain_ImportanceScore')
BEGIN
    CREATE NONCLUSTERED INDEX [IX_BusinessDomain_ImportanceScore] ON [dbo].[BusinessDomain] ([ImportanceScore] DESC);
END

-- Indexes for enhanced BusinessTableInfo
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[BusinessTableInfo]') AND name = N'IX_BusinessTableInfo_DomainClassification')
BEGIN
    CREATE NONCLUSTERED INDEX [IX_BusinessTableInfo_DomainClassification] ON [dbo].[BusinessTableInfo] ([DomainClassification]);
END

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[BusinessTableInfo]') AND name = N'IX_BusinessTableInfo_ImportanceScore')
BEGIN
    CREATE NONCLUSTERED INDEX [IX_BusinessTableInfo_ImportanceScore] ON [dbo].[BusinessTableInfo] ([ImportanceScore] DESC);
END

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[BusinessTableInfo]') AND name = N'IX_BusinessTableInfo_UsageFrequency')
BEGIN
    CREATE NONCLUSTERED INDEX [IX_BusinessTableInfo_UsageFrequency] ON [dbo].[BusinessTableInfo] ([UsageFrequency] DESC);
END

-- Indexes for enhanced BusinessGlossary
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[BusinessGlossary]') AND name = N'IX_BusinessGlossary_Domain')
BEGIN
    CREATE NONCLUSTERED INDEX [IX_BusinessGlossary_Domain] ON [dbo].[BusinessGlossary] ([Domain]);
END

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[BusinessGlossary]') AND name = N'IX_BusinessGlossary_ConfidenceScore')
BEGIN
    CREATE NONCLUSTERED INDEX [IX_BusinessGlossary_ConfidenceScore] ON [dbo].[BusinessGlossary] ([ConfidenceScore] DESC);
END

-- =====================================================
-- 7. Insert sample semantic data
-- =====================================================

-- Insert sample business domains
IF NOT EXISTS (SELECT 1 FROM [dbo].[BusinessDomain] WHERE [DomainName] = 'Finance')
BEGIN
    INSERT INTO [dbo].[BusinessDomain] ([DomainName], [Description], [KeyConcepts], [ImportanceScore], [CreatedBy])
    VALUES ('Finance', 'Financial data and metrics including revenue, costs, and profitability', '["Revenue", "Profit", "Cost", "Budget", "ROI"]', 0.9, 'System');
END

IF NOT EXISTS (SELECT 1 FROM [dbo].[BusinessDomain] WHERE [DomainName] = 'Customer')
BEGIN
    INSERT INTO [dbo].[BusinessDomain] ([DomainName], [Description], [KeyConcepts], [ImportanceScore], [CreatedBy])
    VALUES ('Customer', 'Customer data including demographics, behavior, and engagement', '["Customer", "User", "Player", "Engagement", "Retention"]', 0.8, 'System');
END

IF NOT EXISTS (SELECT 1 FROM [dbo].[BusinessDomain] WHERE [DomainName] = 'Gaming')
BEGIN
    INSERT INTO [dbo].[BusinessDomain] ([DomainName], [Description], [KeyConcepts], [ImportanceScore], [CreatedBy])
    VALUES ('Gaming', 'Gaming-specific metrics and player activity data', '["Games", "Bets", "Wins", "Losses", "Activity"]', 0.9, 'System');
END

PRINT 'Phase 2 Semantic Layer Enhancement migration completed successfully.';
