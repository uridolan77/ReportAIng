-- Fix IDENTITY column issues for 3 tables
-- Run this script to resolve the migration IDENTITY errors

-- =====================================================
-- 1. Fix SemanticCacheEntries (int IDENTITY -> nvarchar)
-- =====================================================

-- Drop existing table if it exists
IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'SemanticCacheEntries')
BEGIN
    DROP TABLE [SemanticCacheEntries];
END

-- Create new SemanticCacheEntries table with nvarchar Id
CREATE TABLE [SemanticCacheEntries] (
    [Id] nvarchar(450) NOT NULL,
    [QueryHash] nvarchar(64) NOT NULL,
    [OriginalQuery] nvarchar(4000) NOT NULL,
    [NormalizedQuery] nvarchar(4000) NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [ExpiresAt] datetime2 NOT NULL,
    [LastAccessedAt] datetime2 NOT NULL,
    [AccessCount] int NOT NULL,
    [EmbeddingVector] nvarchar(max) NULL,
    [SimilarityThreshold] float NOT NULL,
    [CreatedBy] nvarchar(500) NULL,
    [UpdatedBy] nvarchar(500) NULL,
    [UpdatedAt] datetime2 NOT NULL,
    [EntryType] nvarchar(max) NOT NULL DEFAULT '',
    [Key] nvarchar(max) NOT NULL DEFAULT '',
    [Query] nvarchar(max) NOT NULL DEFAULT '',
    [Value] nvarchar(max) NOT NULL DEFAULT '',
    CONSTRAINT [PK_SemanticCacheEntries] PRIMARY KEY ([Id])
);

-- =====================================================
-- 2. Fix AIGenerationAttempts (int IDENTITY -> bigint IDENTITY)
-- =====================================================

-- First, find and drop all foreign key constraints referencing AIGenerationAttempts
DECLARE @sql NVARCHAR(MAX) = '';
SELECT @sql = @sql + 'ALTER TABLE [' + SCHEMA_NAME(fk.schema_id) + '].[' + OBJECT_NAME(fk.parent_object_id) + '] DROP CONSTRAINT [' + fk.name + '];' + CHAR(13)
FROM sys.foreign_keys fk
INNER JOIN sys.foreign_key_columns fkc ON fk.object_id = fkc.constraint_object_id
WHERE fkc.referenced_object_id = OBJECT_ID('AIGenerationAttempts');

IF @sql <> ''
BEGIN
    PRINT 'Dropping foreign key constraints:';
    PRINT @sql;
    EXEC sp_executesql @sql;
END

-- Backup existing data if table exists
IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'AIGenerationAttempts')
BEGIN
    -- Create backup table
    SELECT * INTO [AIGenerationAttempts_Backup] FROM [AIGenerationAttempts];

    -- Drop original table (now that FK constraints are removed)
    DROP TABLE [AIGenerationAttempts];
END

-- Create new AIGenerationAttempts table with bigint IDENTITY (using actual column structure)
CREATE TABLE [AIGenerationAttempts] (
    [Id] bigint IDENTITY(1,1) NOT NULL,
    [UserId] nvarchar(500) NOT NULL,
    [UserQuery] nvarchar(max) NOT NULL,
    [GeneratedSql] nvarchar(max) NULL,
    [AIProvider] nvarchar(100) NULL,
    [ModelVersion] nvarchar(100) NULL,
    [AttemptedAt] datetime2(7) NOT NULL,
    [GenerationTimeMs] int NOT NULL,
    [IsSuccessful] bit NOT NULL,
    [ErrorMessage] nvarchar(max) NULL,
    [ConfidenceScore] float NOT NULL,
    [PromptTemplate] nvarchar(max) NULL,
    [ContextData] nvarchar(max) NULL,
    [TokensUsed] int NOT NULL,
    [Cost] decimal(18,8) NULL,  -- Changed precision to match EF configuration
    [Feedback] nvarchar(max) NULL,
    [Rating] int NULL,
    [WasExecuted] bit NOT NULL,
    [WasModified] bit NOT NULL,
    [ModifiedSql] nvarchar(max) NULL,
    [Metadata] nvarchar(max) NULL,
    -- Additional columns from migration
    [Prompt] nvarchar(max) NOT NULL DEFAULT '',
    [GeneratedContent] nvarchar(max) NULL,
    [CompletedAt] datetime2 NULL,
    [CreatedAt] datetime2 NOT NULL DEFAULT '1900-01-01T00:00:00.000',
    [CreatedBy] nvarchar(max) NULL,
    [ModelName] nvarchar(max) NULL,
    [ProcessingTimeMs] float NULL,
    [ProviderName] nvarchar(max) NULL,
    [QueryHistoryId] bigint NULL,
    [QueryId] nvarchar(max) NOT NULL DEFAULT '',
    [RequestParameters] nvarchar(max) NULL,
    [UpdatedAt] datetime2 NULL,
    [GenerationAttemptId] bigint NULL,
    CONSTRAINT [PK_AIGenerationAttempts] PRIMARY KEY ([Id])
);

-- Restore data from backup if it exists
IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'AIGenerationAttempts_Backup')
BEGIN
    SET IDENTITY_INSERT [AIGenerationAttempts] ON;

    INSERT INTO [AIGenerationAttempts] (
        [Id], [UserId], [UserQuery], [GeneratedSql], [AIProvider], [ModelVersion],
        [AttemptedAt], [GenerationTimeMs], [IsSuccessful], [ErrorMessage], [ConfidenceScore],
        [PromptTemplate], [ContextData], [TokensUsed], [Cost], [Feedback], [Rating],
        [WasExecuted], [WasModified], [ModifiedSql], [Metadata]
    )
    SELECT
        [Id], [UserId], [UserQuery], [GeneratedSql], [AIProvider], [ModelVersion],
        [AttemptedAt], [GenerationTimeMs], [IsSuccessful], [ErrorMessage], [ConfidenceScore],
        [PromptTemplate], [ContextData], [TokensUsed], [Cost], [Feedback], [Rating],
        [WasExecuted], [WasModified], [ModifiedSql], [Metadata]
    FROM [AIGenerationAttempts_Backup];

    SET IDENTITY_INSERT [AIGenerationAttempts] OFF;

    -- Drop backup table
    DROP TABLE [AIGenerationAttempts_Backup];
END

PRINT 'AIGenerationAttempts table recreated with bigint IDENTITY';
PRINT 'Note: Foreign key constraints will be recreated by the migration';

-- =====================================================
-- 3. Fix AIFeedbackEntries (int IDENTITY -> nvarchar)
-- =====================================================

-- Backup existing data if table exists
IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'AIFeedbackEntries')
BEGIN
    -- Create backup table
    SELECT * INTO [AIFeedbackEntries_Backup] FROM [AIFeedbackEntries];
    
    -- Drop original table
    DROP TABLE [AIFeedbackEntries];
END

-- Create new AIFeedbackEntries table with nvarchar Id
CREATE TABLE [AIFeedbackEntries] (
    [Id] nvarchar(450) NOT NULL,
    [GenerationAttemptId] nvarchar(max) NOT NULL DEFAULT '',
    [FeedbackText] nvarchar(max) NULL,
    [Rating] int NULL,
    [SubmittedAt] datetime2 NOT NULL,
    [CreatedBy] nvarchar(500) NULL,
    [UpdatedBy] nvarchar(500) NULL,
    [CreatedDate] datetime2 NOT NULL DEFAULT '1900-01-01T00:00:00.000',
    [UpdatedAt] datetime2 NOT NULL DEFAULT '1900-01-01T00:00:00.000',
    [IsActive] bit NOT NULL DEFAULT 0,
    [IsHelpful] bit NOT NULL DEFAULT 0,
    [LastUpdated] datetime2 NOT NULL DEFAULT '1900-01-01T00:00:00.000',
    [QueryId] nvarchar(max) NOT NULL DEFAULT '',
    CONSTRAINT [PK_AIFeedbackEntries] PRIMARY KEY ([Id])
);

-- Restore data from backup if it exists (convert Id to string)
IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'AIFeedbackEntries_Backup')
BEGIN
    INSERT INTO [AIFeedbackEntries] (
        [Id], [GenerationAttemptId], [FeedbackText], [Rating], [SubmittedAt]
    )
    SELECT 
        CAST([Id] AS nvarchar(450)), 
        CAST([GenerationAttemptId] AS nvarchar(max)), 
        [FeedbackText], 
        [Rating], 
        [SubmittedAt]
    FROM [AIFeedbackEntries_Backup];
    
    -- Drop backup table
    DROP TABLE [AIFeedbackEntries_Backup];
END

-- =====================================================
-- Create indexes for performance
-- =====================================================

-- SemanticCacheEntries indexes
CREATE INDEX [IX_SemanticCacheEntries_QueryHash] ON [SemanticCacheEntries] ([QueryHash]);
CREATE INDEX [IX_SemanticCacheEntries_CreatedAt] ON [SemanticCacheEntries] ([CreatedAt]);
CREATE INDEX [IX_SemanticCacheEntries_ExpiresAt] ON [SemanticCacheEntries] ([ExpiresAt]);

-- AIGenerationAttempts indexes
CREATE INDEX [IX_AIGenerationAttempts_UserId] ON [AIGenerationAttempts] ([UserId]);
CREATE INDEX [IX_AIGenerationAttempts_AttemptedAt] ON [AIGenerationAttempts] ([AttemptedAt]);
CREATE INDEX [IX_AIGenerationAttempts_IsSuccessful_AttemptedAt] ON [AIGenerationAttempts] ([IsSuccessful], [AttemptedAt]);

-- AIFeedbackEntries indexes (skip GenerationAttemptId since it's nvarchar(max))
CREATE INDEX [IX_AIFeedbackEntries_SubmittedAt] ON [AIFeedbackEntries] ([SubmittedAt]);

PRINT 'IDENTITY table fixes completed successfully!';
PRINT 'Tables fixed:';
PRINT '  - SemanticCacheEntries: Id changed from int IDENTITY to nvarchar(450)';
PRINT '  - AIGenerationAttempts: Id changed from int IDENTITY to bigint IDENTITY';
PRINT '  - AIFeedbackEntries: Id changed from int IDENTITY to nvarchar(450)';
