-- Complete Database Fix Script
-- This script handles all remaining migration issues in one go

PRINT 'Starting complete database fix...';

-- =====================================================
-- 1. Create missing tables that the migration expects
-- =====================================================

-- Create PromptLogs table if it doesn't exist
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'PromptLogs')
BEGIN
    PRINT 'Creating PromptLogs table...';
    CREATE TABLE [PromptLogs] (
        [Id] bigint IDENTITY(1,1) NOT NULL,
        [UserId] nvarchar(500) NOT NULL,
        [PromptText] nvarchar(max) NOT NULL,
        [ResponseText] nvarchar(max) NULL,
        [CreatedAt] datetime2 NOT NULL DEFAULT GETUTCDATE(),
        [PromptHash] nvarchar(64) NULL,
        [TemplateId] bigint NULL,
        CONSTRAINT [PK_PromptLogs] PRIMARY KEY ([Id])
    );
    PRINT 'PromptLogs table created.';
END
ELSE
BEGIN
    PRINT 'PromptLogs table already exists.';
    
    -- Add missing columns if they don't exist
    IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'PromptLogs' AND COLUMN_NAME = 'PromptHash')
    BEGIN
        ALTER TABLE [PromptLogs] ADD [PromptHash] nvarchar(64) NULL;
        PRINT 'Added PromptHash column to PromptLogs.';
    END
    
    IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'PromptLogs' AND COLUMN_NAME = 'TemplateId')
    BEGIN
        ALTER TABLE [PromptLogs] ADD [TemplateId] bigint NULL;
        PRINT 'Added TemplateId column to PromptLogs.';
    END
END

-- Create QueryHistory table if it doesn't exist
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'QueryHistory')
BEGIN
    PRINT 'Creating QueryHistory table...';
    CREATE TABLE [QueryHistory] (
        [Id] bigint IDENTITY(1,1) NOT NULL,
        [UserId] nvarchar(500) NOT NULL,
        [Query] nvarchar(max) NOT NULL,
        [GeneratedSql] nvarchar(max) NULL,
        [IsSuccessful] bit NOT NULL,
        [ExecutionTimeMs] bigint NOT NULL,
        [CreatedAt] datetime2 NOT NULL DEFAULT GETUTCDATE(),
        [UpdatedAt] datetime2 NOT NULL DEFAULT GETUTCDATE(),
        [SessionId] nvarchar(100) NULL,
        [ErrorMessage] nvarchar(max) NULL,
        [ConfidenceScore] float NULL,
        [CreatedBy] nvarchar(500) NULL,
        [UpdatedBy] nvarchar(500) NULL,
        [UserFeedback] nvarchar(max) NULL,
        [QueryComplexity] nvarchar(max) NULL,
        [ResultRowCount] int NULL,
        [UpdatedDate] datetime2 NULL,
        CONSTRAINT [PK_QueryHistory] PRIMARY KEY ([Id])
    );
    PRINT 'QueryHistory table created.';
END
ELSE
BEGIN
    PRINT 'QueryHistory table already exists.';
END

-- =====================================================
-- 2. Ensure SemanticCacheEntries has correct structure
-- =====================================================

IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'SemanticCacheEntries')
BEGIN
    PRINT 'Checking SemanticCacheEntries structure...';
    
    -- Check if Id column is correct type
    IF EXISTS (
        SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
        WHERE TABLE_NAME = 'SemanticCacheEntries' 
        AND COLUMN_NAME = 'Id' 
        AND DATA_TYPE = 'int'
    )
    BEGIN
        PRINT 'SemanticCacheEntries has wrong Id type - recreating table...';
        
        -- Backup and recreate
        IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'SemanticCacheEntries_Backup')
            DROP TABLE [SemanticCacheEntries_Backup];
            
        SELECT * INTO [SemanticCacheEntries_Backup] FROM [SemanticCacheEntries];
        DROP TABLE [SemanticCacheEntries];
        
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
        
        -- Try to restore data (convert Id to string)
        IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'SemanticCacheEntries_Backup')
        BEGIN
            INSERT INTO [SemanticCacheEntries] ([Id], [QueryHash], [OriginalQuery], [NormalizedQuery], [CreatedAt], [ExpiresAt], [LastAccessedAt], [AccessCount], [SimilarityThreshold])
            SELECT CAST([Id] AS nvarchar(450)), [QueryHash], [OriginalQuery], [NormalizedQuery], [CreatedAt], [ExpiresAt], [LastAccessedAt], [AccessCount], ISNULL([SimilarityThreshold], 0.8)
            FROM [SemanticCacheEntries_Backup];
            
            DROP TABLE [SemanticCacheEntries_Backup];
        END
        
        PRINT 'SemanticCacheEntries recreated with correct structure.';
    END
    ELSE
    BEGIN
        PRINT 'SemanticCacheEntries already has correct structure.';
    END
END
ELSE
BEGIN
    PRINT 'SemanticCacheEntries will be created by migration.';
END

-- =====================================================
-- 3. Add missing columns to existing tables
-- =====================================================

-- Add missing columns to AIGenerationAttempts if they don't exist
PRINT 'Checking AIGenerationAttempts for missing columns...';

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'AIGenerationAttempts' AND COLUMN_NAME = 'Prompt')
    ALTER TABLE [AIGenerationAttempts] ADD [Prompt] nvarchar(max) NOT NULL DEFAULT '';

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'AIGenerationAttempts' AND COLUMN_NAME = 'GeneratedContent')
    ALTER TABLE [AIGenerationAttempts] ADD [GeneratedContent] nvarchar(max) NULL;

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'AIGenerationAttempts' AND COLUMN_NAME = 'CompletedAt')
    ALTER TABLE [AIGenerationAttempts] ADD [CompletedAt] datetime2 NULL;

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'AIGenerationAttempts' AND COLUMN_NAME = 'CreatedAt')
    ALTER TABLE [AIGenerationAttempts] ADD [CreatedAt] datetime2 NOT NULL DEFAULT '1900-01-01T00:00:00.000';

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'AIGenerationAttempts' AND COLUMN_NAME = 'CreatedBy')
    ALTER TABLE [AIGenerationAttempts] ADD [CreatedBy] nvarchar(max) NULL;

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'AIGenerationAttempts' AND COLUMN_NAME = 'ModelName')
    ALTER TABLE [AIGenerationAttempts] ADD [ModelName] nvarchar(max) NULL;

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'AIGenerationAttempts' AND COLUMN_NAME = 'ProcessingTimeMs')
    ALTER TABLE [AIGenerationAttempts] ADD [ProcessingTimeMs] float NULL;

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'AIGenerationAttempts' AND COLUMN_NAME = 'ProviderName')
    ALTER TABLE [AIGenerationAttempts] ADD [ProviderName] nvarchar(max) NULL;

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'AIGenerationAttempts' AND COLUMN_NAME = 'QueryHistoryId')
    ALTER TABLE [AIGenerationAttempts] ADD [QueryHistoryId] bigint NULL;

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'AIGenerationAttempts' AND COLUMN_NAME = 'QueryId')
    ALTER TABLE [AIGenerationAttempts] ADD [QueryId] nvarchar(max) NOT NULL DEFAULT '';

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'AIGenerationAttempts' AND COLUMN_NAME = 'RequestParameters')
    ALTER TABLE [AIGenerationAttempts] ADD [RequestParameters] nvarchar(max) NULL;

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'AIGenerationAttempts' AND COLUMN_NAME = 'UpdatedAt')
    ALTER TABLE [AIGenerationAttempts] ADD [UpdatedAt] datetime2 NULL;

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'AIGenerationAttempts' AND COLUMN_NAME = 'GenerationAttemptId')
    ALTER TABLE [AIGenerationAttempts] ADD [GenerationAttemptId] bigint NULL;

-- Add missing columns to AIFeedbackEntries if they don't exist
PRINT 'Checking AIFeedbackEntries for missing columns...';

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'AIFeedbackEntries' AND COLUMN_NAME = 'CreatedBy')
    ALTER TABLE [AIFeedbackEntries] ADD [CreatedBy] nvarchar(500) NULL;

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'AIFeedbackEntries' AND COLUMN_NAME = 'CreatedDate')
    ALTER TABLE [AIFeedbackEntries] ADD [CreatedDate] datetime2 NOT NULL DEFAULT '1900-01-01T00:00:00.000';

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'AIFeedbackEntries' AND COLUMN_NAME = 'IsActive')
    ALTER TABLE [AIFeedbackEntries] ADD [IsActive] bit NOT NULL DEFAULT 0;

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'AIFeedbackEntries' AND COLUMN_NAME = 'IsHelpful')
    ALTER TABLE [AIFeedbackEntries] ADD [IsHelpful] bit NOT NULL DEFAULT 0;

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'AIFeedbackEntries' AND COLUMN_NAME = 'LastUpdated')
    ALTER TABLE [AIFeedbackEntries] ADD [LastUpdated] datetime2 NOT NULL DEFAULT '1900-01-01T00:00:00.000';

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'AIFeedbackEntries' AND COLUMN_NAME = 'QueryId')
    ALTER TABLE [AIFeedbackEntries] ADD [QueryId] nvarchar(max) NOT NULL DEFAULT '';

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'AIFeedbackEntries' AND COLUMN_NAME = 'UpdatedAt')
    ALTER TABLE [AIFeedbackEntries] ADD [UpdatedAt] datetime2 NOT NULL DEFAULT '1900-01-01T00:00:00.000';

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'AIFeedbackEntries' AND COLUMN_NAME = 'UpdatedBy')
    ALTER TABLE [AIFeedbackEntries] ADD [UpdatedBy] nvarchar(500) NULL;

-- =====================================================
-- 4. Create performance indexes
-- =====================================================

PRINT 'Creating performance indexes...';

-- AIGenerationAttempts indexes
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_AIGenerationAttempts_UserId')
    CREATE INDEX [IX_AIGenerationAttempts_UserId] ON [AIGenerationAttempts] ([UserId]);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_AIGenerationAttempts_AttemptedAt')
    CREATE INDEX [IX_AIGenerationAttempts_AttemptedAt] ON [AIGenerationAttempts] ([AttemptedAt]);

-- AIFeedbackEntries indexes
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_AIFeedbackEntries_SubmittedAt')
    CREATE INDEX [IX_AIFeedbackEntries_SubmittedAt] ON [AIFeedbackEntries] ([SubmittedAt]);

-- SemanticCacheEntries indexes
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_SemanticCacheEntries_QueryHash')
    CREATE INDEX [IX_SemanticCacheEntries_QueryHash] ON [SemanticCacheEntries] ([QueryHash]);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_SemanticCacheEntries_CreatedAt')
    CREATE INDEX [IX_SemanticCacheEntries_CreatedAt] ON [SemanticCacheEntries] ([CreatedAt]);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_SemanticCacheEntries_ExpiresAt')
    CREATE INDEX [IX_SemanticCacheEntries_ExpiresAt] ON [SemanticCacheEntries] ([ExpiresAt]);

PRINT 'Complete database fix completed successfully!';
PRINT '';
PRINT 'Summary of changes:';
PRINT '  - Created missing tables (PromptLogs, QueryHistory if needed)';
PRINT '  - Fixed SemanticCacheEntries structure if needed';
PRINT '  - Added missing columns to AIGenerationAttempts and AIFeedbackEntries';
PRINT '  - Created performance indexes';
PRINT '';
PRINT 'You can now run the Entity Framework migration.';
