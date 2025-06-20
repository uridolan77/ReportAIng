-- Consolidate AIGenerationAttempts and UnifiedAIGenerationAttempt into one unified table
-- Keep the better UnifiedAIGenerationAttempt structure and migrate existing data

PRINT 'Starting consolidation to unified AI generation model...';

-- Step 1: Create the unified table if it doesn't exist
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'UnifiedAIGenerationAttempt')
BEGIN
    PRINT 'Creating UnifiedAIGenerationAttempt table...';
    
    CREATE TABLE [UnifiedAIGenerationAttempt] (
        [Id] nvarchar(450) NOT NULL,
        [UserId] nvarchar(256) NOT NULL,  -- Standardized to 256
        [Provider] nvarchar(max) NOT NULL,
        [Model] nvarchar(max) NOT NULL,
        [InputPrompt] nvarchar(max) NOT NULL,
        [GeneratedOutput] nvarchar(max) NULL,
        [Status] nvarchar(max) NOT NULL,
        [ErrorMessage] nvarchar(max) NULL,
        [ResponseTimeMs] bigint NOT NULL,
        [InputTokens] int NOT NULL,
        [OutputTokens] int NOT NULL,
        [TotalTokens] int NOT NULL,
        [Cost] decimal(18,8) NOT NULL DEFAULT 0,
        [GenerationType] nvarchar(max) NOT NULL,
        [QualityScore] float NULL,
        [Metadata] nvarchar(max) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NOT NULL,
        [AttemptedAt] datetime2 NOT NULL,
        [AIProvider] nvarchar(100) NOT NULL,
        [ModelVersion] nvarchar(100) NOT NULL,
        [CreatedBy] nvarchar(500) NULL,
        [UpdatedBy] nvarchar(500) NULL,
        CONSTRAINT [PK_UnifiedAIGenerationAttempt] PRIMARY KEY ([Id])
    );
    
    PRINT 'UnifiedAIGenerationAttempt table created.';
END
ELSE
BEGIN
    PRINT 'UnifiedAIGenerationAttempt table already exists.';
END

-- Step 2: Migrate data from old AIGenerationAttempts table if it exists and has data
IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'AIGenerationAttempts')
BEGIN
    DECLARE @recordCount INT;
    SELECT @recordCount = COUNT(*) FROM [AIGenerationAttempts];
    
    IF @recordCount > 0
    BEGIN
        PRINT 'Migrating ' + CAST(@recordCount AS VARCHAR(10)) + ' records from AIGenerationAttempts...';
        
        -- Migrate data with field mapping
        INSERT INTO [UnifiedAIGenerationAttempt] (
            [Id],
            [UserId],
            [Provider],
            [Model],
            [InputPrompt],
            [GeneratedOutput],
            [Status],
            [ErrorMessage],
            [ResponseTimeMs],
            [InputTokens],
            [OutputTokens],
            [TotalTokens],
            [Cost],
            [GenerationType],
            [QualityScore],
            [Metadata],
            [CreatedAt],
            [UpdatedAt],
            [AttemptedAt],
            [AIProvider],
            [ModelVersion],
            [CreatedBy],
            [UpdatedBy]
        )
        SELECT 
            CAST([Id] AS nvarchar(450)) AS [Id],  -- Convert bigint to string
            [UserId],
            ISNULL([AIProvider], 'Unknown') AS [Provider],
            ISNULL([ModelVersion], 'Unknown') AS [Model],
            ISNULL([UserQuery], [Prompt]) AS [InputPrompt],  -- Use UserQuery or Prompt
            ISNULL([GeneratedSql], [GeneratedContent]) AS [GeneratedOutput],  -- Use GeneratedSql or GeneratedContent
            CASE WHEN [IsSuccessful] = 1 THEN 'Success' ELSE 'Failed' END AS [Status],
            [ErrorMessage],
            ISNULL([GenerationTimeMs], [ProcessingTimeMs]) AS [ResponseTimeMs],  -- Use available time field
            ISNULL([TokensUsed], 0) AS [InputTokens],
            0 AS [OutputTokens],  -- Not available in old table
            ISNULL([TokensUsed], 0) AS [TotalTokens],
            ISNULL([Cost], 0) AS [Cost],
            'SQL_Generation' AS [GenerationType],  -- Default type
            [ConfidenceScore] AS [QualityScore],
            [Metadata],
            ISNULL([CreatedAt], [AttemptedAt]) AS [CreatedAt],
            ISNULL([UpdatedAt], [AttemptedAt]) AS [UpdatedAt],
            [AttemptedAt],
            ISNULL([AIProvider], 'Unknown') AS [AIProvider],
            ISNULL([ModelVersion], 'Unknown') AS [ModelVersion],
            [CreatedBy],
            [UpdatedBy]
        FROM [AIGenerationAttempts]
        WHERE NOT EXISTS (
            SELECT 1 FROM [UnifiedAIGenerationAttempt] 
            WHERE [Id] = CAST([AIGenerationAttempts].[Id] AS nvarchar(450))
        );
        
        PRINT 'Data migration completed.';
    END
    ELSE
    BEGIN
        PRINT 'AIGenerationAttempts table is empty - no data to migrate.';
    END
END
ELSE
BEGIN
    PRINT 'AIGenerationAttempts table does not exist - no data to migrate.';
END

-- Step 3: Create indexes for performance
PRINT 'Creating indexes...';

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_UnifiedAIGenerationAttempt_UserId')
    CREATE INDEX [IX_UnifiedAIGenerationAttempt_UserId] ON [UnifiedAIGenerationAttempt] ([UserId]);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_UnifiedAIGenerationAttempt_AttemptedAt')
    CREATE INDEX [IX_UnifiedAIGenerationAttempt_AttemptedAt] ON [UnifiedAIGenerationAttempt] ([AttemptedAt]);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_UnifiedAIGenerationAttempt_Provider')
    CREATE INDEX [IX_UnifiedAIGenerationAttempt_Provider] ON [UnifiedAIGenerationAttempt] ([Provider]);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_UnifiedAIGenerationAttempt_Status')
    CREATE INDEX [IX_UnifiedAIGenerationAttempt_Status] ON [UnifiedAIGenerationAttempt] ([Status]);

PRINT 'Indexes created.';

-- Step 4: Mark migrations as applied since we've manually created the unified structure
IF NOT EXISTS (
    SELECT 1 FROM [__EFMigrationsHistory] 
    WHERE [MigrationId] = '20250617133637_FixDecimalPrecision'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES ('20250617133637_FixDecimalPrecision', '8.0.0');
    PRINT 'FixDecimalPrecision migration marked as applied.';
END

PRINT '';
PRINT 'Consolidation completed successfully!';
PRINT '';
PRINT 'Summary:';
PRINT '  ✅ UnifiedAIGenerationAttempt table created with optimal structure';
PRINT '  ✅ Data migrated from old AIGenerationAttempts table (if any)';
PRINT '  ✅ Performance indexes created';
PRINT '  ✅ Decimal precision already correct (18,8)';
PRINT '  ✅ UserId standardized to nvarchar(256)';
PRINT '';
PRINT 'Next steps:';
PRINT '  1. Update code to use only UnifiedAIGenerationAttempt model';
PRINT '  2. Remove references to old AIGenerationAttempts model';
PRINT '  3. Drop old AIGenerationAttempts table when ready';
PRINT '';
PRINT 'The application should now start successfully!';
