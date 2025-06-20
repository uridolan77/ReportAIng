-- Manual application of Phase2A_AgentCommunicationLogs migration
-- This script applies the migration without the problematic IDENTITY column change

PRINT 'Starting Phase2A_AgentCommunicationLogs migration...';

-- 1. Create AgentCommunicationLogs table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AgentCommunicationLogs]') AND type in (N'U'))
BEGIN
    PRINT 'Creating AgentCommunicationLogs table...';
    CREATE TABLE [dbo].[AgentCommunicationLogs] (
        [Id] nvarchar(50) NOT NULL,
        [CorrelationId] nvarchar(50) NULL,
        [SourceAgent] nvarchar(100) NOT NULL,
        [TargetAgent] nvarchar(100) NOT NULL,
        [MessageType] nvarchar(100) NOT NULL,
        [MessageContent] nvarchar(max) NULL,
        [ResponseContent] nvarchar(max) NULL,
        [Success] bit NOT NULL,
        [ErrorMessage] nvarchar(2000) NULL,
        [ProcessingTimeMs] int NOT NULL,
        [MetadataJson] nvarchar(4000) NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_AgentCommunicationLogs] PRIMARY KEY ([Id])
    );
    
    CREATE NONCLUSTERED INDEX [IX_AgentCommunicationLogs_CorrelationId] ON [dbo].[AgentCommunicationLogs] ([CorrelationId]);
    CREATE NONCLUSTERED INDEX [IX_AgentCommunicationLogs_CreatedAt] ON [dbo].[AgentCommunicationLogs] ([CreatedAt]);
    CREATE NONCLUSTERED INDEX [IX_AgentCommunicationLogs_SourceAgent_TargetAgent] ON [dbo].[AgentCommunicationLogs] ([SourceAgent], [TargetAgent]);
    
    PRINT '✅ AgentCommunicationLogs table created';
END
ELSE
BEGIN
    PRINT '✅ AgentCommunicationLogs table already exists';
END

-- 2. Add missing columns to SchemaMetadata (already done manually, but check)
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[SchemaMetadata]') AND name = 'BusinessContext')
BEGIN
    ALTER TABLE [dbo].[SchemaMetadata] ADD [BusinessContext] nvarchar(max) NULL;
    PRINT '✅ Added BusinessContext to SchemaMetadata';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[SchemaMetadata]') AND name = 'BusinessDomain')
BEGIN
    ALTER TABLE [dbo].[SchemaMetadata] ADD [BusinessDomain] nvarchar(max) NULL;
    PRINT '✅ Added BusinessDomain to SchemaMetadata';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[SchemaMetadata]') AND name = 'BusinessImportance')
BEGIN
    ALTER TABLE [dbo].[SchemaMetadata] ADD [BusinessImportance] decimal(18,2) NULL;
    PRINT '✅ Added BusinessImportance to SchemaMetadata';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[SchemaMetadata]') AND name = 'QueryIntents')
BEGIN
    ALTER TABLE [dbo].[SchemaMetadata] ADD [QueryIntents] nvarchar(max) NULL;
    PRINT '✅ Added QueryIntents to SchemaMetadata';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[SchemaMetadata]') AND name = 'SemanticSynonyms')
BEGIN
    ALTER TABLE [dbo].[SchemaMetadata] ADD [SemanticSynonyms] nvarchar(max) NULL;
    PRINT '✅ Added SemanticSynonyms to SchemaMetadata';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[SchemaMetadata]') AND name = 'UsageExamples')
BEGIN
    ALTER TABLE [dbo].[SchemaMetadata] ADD [UsageExamples] nvarchar(max) NULL;
    PRINT '✅ Added UsageExamples to SchemaMetadata';
END

-- 3. Fix decimal precision for SemanticSchemaMapping.ConfidenceScore
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SemanticSchemaMapping]') AND type in (N'U'))
BEGIN
    -- Check current precision
    DECLARE @CurrentType NVARCHAR(50);
    SELECT @CurrentType = DATA_TYPE + '(' + CAST(NUMERIC_PRECISION AS VARCHAR) + ',' + CAST(NUMERIC_SCALE AS VARCHAR) + ')'
    FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'SemanticSchemaMapping' AND COLUMN_NAME = 'ConfidenceScore';
    
    IF @CurrentType != 'decimal(5,4)'
    BEGIN
        PRINT 'Updating ConfidenceScore precision to decimal(5,4)...';
        ALTER TABLE [dbo].[SemanticSchemaMapping] ALTER COLUMN [ConfidenceScore] decimal(5,4) NOT NULL;
        PRINT '✅ Updated ConfidenceScore precision';
    END
    ELSE
    BEGIN
        PRINT '✅ ConfidenceScore precision already correct';
    END
END

-- 4. Mark migration as applied
IF NOT EXISTS (SELECT * FROM __EFMigrationsHistory WHERE MigrationId = '20250620070334_Phase2A_AgentCommunicationLogs')
BEGIN
    INSERT INTO __EFMigrationsHistory (MigrationId, ProductVersion)
    VALUES ('20250620070334_Phase2A_AgentCommunicationLogs', '8.0.0');
    PRINT '✅ Migration marked as applied in __EFMigrationsHistory';
END

PRINT 'Phase2A_AgentCommunicationLogs migration completed successfully!';
