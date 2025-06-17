-- Fix remaining IDENTITY column issues
-- Only fixes tables that still need fixing

PRINT 'Checking which tables need fixing...';

-- =====================================================
-- Check SemanticCacheEntries
-- =====================================================
IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'SemanticCacheEntries')
BEGIN
    PRINT 'SemanticCacheEntries table exists - checking if it needs fixing...';
    
    -- Check if Id column is int IDENTITY (needs fixing) or nvarchar (already fixed)
    IF EXISTS (
        SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
        WHERE TABLE_NAME = 'SemanticCacheEntries' 
        AND COLUMN_NAME = 'Id' 
        AND DATA_TYPE = 'int'
    )
    BEGIN
        PRINT 'SemanticCacheEntries needs fixing (Id is int)';
        
        -- Backup and recreate
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
        
        -- Restore data (convert Id to string)
        IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'SemanticCacheEntries_Backup')
        BEGIN
            INSERT INTO [SemanticCacheEntries] ([Id], [QueryHash], [OriginalQuery], [NormalizedQuery], [CreatedAt], [ExpiresAt], [LastAccessedAt], [AccessCount], [EmbeddingVector], [SimilarityThreshold])
            SELECT CAST([Id] AS nvarchar(450)), [QueryHash], [OriginalQuery], [NormalizedQuery], [CreatedAt], [ExpiresAt], [LastAccessedAt], [AccessCount], [EmbeddingVector], [SimilarityThreshold]
            FROM [SemanticCacheEntries_Backup];
            
            DROP TABLE [SemanticCacheEntries_Backup];
        END
        
        PRINT 'SemanticCacheEntries fixed!';
    END
    ELSE
    BEGIN
        PRINT 'SemanticCacheEntries already has correct structure (Id is nvarchar)';
    END
END
ELSE
BEGIN
    PRINT 'SemanticCacheEntries table does not exist - will be created by migration';
END

-- =====================================================
-- Fix AIGenerationAttempts (int IDENTITY -> bigint IDENTITY)
-- =====================================================
PRINT 'Fixing AIGenerationAttempts...';

-- Drop foreign key constraints first
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

-- Backup existing data
SELECT * INTO [AIGenerationAttempts_Backup] FROM [AIGenerationAttempts];

-- Drop original table
DROP TABLE [AIGenerationAttempts];

-- Create new table with bigint IDENTITY
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
    [Cost] decimal(18,8) NULL,  -- Updated precision
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

-- Restore data
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

-- Create indexes
CREATE INDEX [IX_AIGenerationAttempts_UserId] ON [AIGenerationAttempts] ([UserId]);
CREATE INDEX [IX_AIGenerationAttempts_AttemptedAt] ON [AIGenerationAttempts] ([AttemptedAt]);
CREATE INDEX [IX_AIGenerationAttempts_IsSuccessful_AttemptedAt] ON [AIGenerationAttempts] ([IsSuccessful], [AttemptedAt]);

PRINT 'AIGenerationAttempts fixed!';

PRINT '';
PRINT 'IDENTITY fixes completed!';
PRINT 'You can now run the Entity Framework migration.';
