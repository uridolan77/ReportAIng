-- Migration: Fix QuerySuggestions Id column data type mismatch
-- This fixes the data type mismatch where Entity Framework expects BIGINT but database has INT

USE [BIReportingCopilot_Dev];
GO

PRINT 'Starting QuerySuggestions Id column data type fix...';

-- Check current data type
DECLARE @CurrentDataType NVARCHAR(50);
SELECT @CurrentDataType = DATA_TYPE 
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'QuerySuggestions' AND COLUMN_NAME = 'Id';

PRINT 'Current QuerySuggestions.Id column data type: ' + ISNULL(@CurrentDataType, 'NOT FOUND');

-- Only proceed if the column is currently INT
IF @CurrentDataType = 'int'
BEGIN
    PRINT 'Converting QuerySuggestions.Id from INT to BIGINT...';
    
    -- Step 1: Check if there are any foreign key constraints referencing this table
    IF EXISTS (
        SELECT 1 
        FROM sys.foreign_keys fk
        INNER JOIN sys.foreign_key_columns fkc ON fk.object_id = fkc.constraint_object_id
        INNER JOIN sys.columns c ON fkc.referenced_object_id = c.object_id AND fkc.referenced_column_id = c.column_id
        INNER JOIN sys.tables t ON c.object_id = t.object_id
        WHERE t.name = 'QuerySuggestions' AND c.name = 'Id'
    )
    BEGIN
        PRINT 'Found foreign key constraints referencing QuerySuggestions.Id. Dropping them temporarily...';
        
        -- Get all foreign keys that reference QuerySuggestions.Id
        DECLARE @FKName NVARCHAR(128);
        DECLARE @FKTable NVARCHAR(128);
        DECLARE fk_cursor CURSOR FOR
        SELECT fk.name, t2.name as referencing_table
        FROM sys.foreign_keys fk
        INNER JOIN sys.foreign_key_columns fkc ON fk.object_id = fkc.constraint_object_id
        INNER JOIN sys.columns c ON fkc.referenced_object_id = c.object_id AND fkc.referenced_column_id = c.column_id
        INNER JOIN sys.tables t ON c.object_id = t.object_id
        INNER JOIN sys.tables t2 ON fk.parent_object_id = t2.object_id
        WHERE t.name = 'QuerySuggestions' AND c.name = 'Id';
        
        OPEN fk_cursor;
        FETCH NEXT FROM fk_cursor INTO @FKName, @FKTable;
        
        WHILE @@FETCH_STATUS = 0
        BEGIN
            PRINT 'Dropping foreign key: ' + @FKName + ' from table: ' + @FKTable;
            DECLARE @DropSQL NVARCHAR(MAX) = 'ALTER TABLE [dbo].[' + @FKTable + '] DROP CONSTRAINT [' + @FKName + ']';
            EXEC sp_executesql @DropSQL;
            FETCH NEXT FROM fk_cursor INTO @FKName, @FKTable;
        END
        
        CLOSE fk_cursor;
        DEALLOCATE fk_cursor;
    END
    
    -- Step 2: Convert the primary key column
    PRINT 'Converting QuerySuggestions.Id from INT IDENTITY to BIGINT IDENTITY...';
    
    -- This requires recreating the table since we can't directly alter an IDENTITY column's data type
    -- First, create a backup of the data
    SELECT * INTO #QuerySuggestions_Backup FROM [dbo].[QuerySuggestions];
    PRINT 'Created backup of QuerySuggestions data.';
    
    -- Drop the original table
    DROP TABLE [dbo].[QuerySuggestions];
    PRINT 'Dropped original QuerySuggestions table.';
    
    -- Recreate the table with BIGINT Id
    CREATE TABLE [dbo].[QuerySuggestions] (
        [Id] BIGINT IDENTITY(1,1) NOT NULL,
        [CategoryId] BIGINT NOT NULL,
        [QueryText] NVARCHAR(500) NOT NULL,
        [Text] NVARCHAR(500) NOT NULL,
        [Description] NVARCHAR(200) NOT NULL,
        [DefaultTimeFrame] NVARCHAR(50) NULL,
        [SortOrder] INT NOT NULL DEFAULT(0),
        [IsActive] BIT NOT NULL DEFAULT(1),
        [TargetTables] NVARCHAR(500) NULL,
        [Complexity] TINYINT NOT NULL DEFAULT(1),
        [RequiredPermissions] NVARCHAR(200) NULL,
        [Tags] NVARCHAR(300) NULL,
        [UsageCount] INT NOT NULL DEFAULT(0),
        [LastUsed] DATETIME2(7) NULL,
        [Relevance] DECIMAL(3,2) NOT NULL DEFAULT(0.8),
        [Query] NVARCHAR(500) NOT NULL DEFAULT(''),
        [Keywords] NVARCHAR(500) NULL,
        [RequiredTables] NVARCHAR(500) NULL,
        [CreatedAt] DATETIME2(7) NOT NULL DEFAULT(GETUTCDATE()),
        [Confidence] DECIMAL(3,2) NOT NULL DEFAULT(0.8),
        [Source] NVARCHAR(100) NOT NULL DEFAULT('manual'),
        [ErrorMessage] NVARCHAR(MAX) NULL,
        [CreatedDate] DATETIME2(7) NOT NULL DEFAULT(GETUTCDATE()),
        [CreatedBy] NVARCHAR(256) NULL,
        [UpdatedBy] NVARCHAR(256) NULL,
        [UpdatedDate] DATETIME2(7) NULL,
        CONSTRAINT [PK_QuerySuggestions] PRIMARY KEY CLUSTERED ([Id] ASC)
    );
    PRINT 'Created new QuerySuggestions table with BIGINT Id.';
    
    -- Restore the data (excluding the Id column since it will be auto-generated)
    SET IDENTITY_INSERT [dbo].[QuerySuggestions] ON;
    
    INSERT INTO [dbo].[QuerySuggestions] (
        [Id], [CategoryId], [QueryText], [Text], [Description], [DefaultTimeFrame], 
        [SortOrder], [IsActive], [TargetTables], [Complexity], [RequiredPermissions], 
        [Tags], [UsageCount], [LastUsed], [Relevance], [Query], [Keywords], 
        [RequiredTables], [CreatedAt], [Confidence], [Source], [ErrorMessage], 
        [CreatedDate], [CreatedBy], [UpdatedBy], [UpdatedDate]
    )
    SELECT 
        [Id], 
        [CategoryId], 
        ISNULL([QueryText], '') as [QueryText],
        ISNULL([Text], ISNULL([QueryText], '')) as [Text],
        ISNULL([Description], '') as [Description],
        [DefaultTimeFrame],
        ISNULL([SortOrder], 0) as [SortOrder],
        ISNULL([IsActive], 1) as [IsActive],
        [TargetTables],
        ISNULL([Complexity], 1) as [Complexity],
        [RequiredPermissions],
        [Tags],
        ISNULL([UsageCount], 0) as [UsageCount],
        [LastUsed],
        ISNULL([Relevance], 0.8) as [Relevance],
        ISNULL([Query], ISNULL([QueryText], '')) as [Query],
        [Keywords],
        [RequiredTables],
        ISNULL([CreatedAt], ISNULL([CreatedDate], GETUTCDATE())) as [CreatedAt],
        ISNULL([Confidence], 0.8) as [Confidence],
        ISNULL([Source], 'manual') as [Source],
        [ErrorMessage],
        ISNULL([CreatedDate], GETUTCDATE()) as [CreatedDate],
        [CreatedBy],
        [UpdatedBy],
        [UpdatedDate]
    FROM #QuerySuggestions_Backup;
    
    SET IDENTITY_INSERT [dbo].[QuerySuggestions] OFF;
    PRINT 'Restored data to new QuerySuggestions table.';
    
    -- Drop the backup table
    DROP TABLE #QuerySuggestions_Backup;
    PRINT 'Dropped backup table.';
    
    -- Step 3: Recreate foreign key constraints
    IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'SuggestionCategories')
    BEGIN
        PRINT 'Recreating foreign key constraint to SuggestionCategories...';
        ALTER TABLE [dbo].[QuerySuggestions] 
        ADD CONSTRAINT [FK_QuerySuggestions_SuggestionCategories_CategoryId] 
        FOREIGN KEY ([CategoryId]) REFERENCES [dbo].[SuggestionCategories]([Id]) ON DELETE CASCADE;
        PRINT 'Foreign key constraint to SuggestionCategories recreated successfully.';
    END
    
    -- Step 4: Create indexes
    PRINT 'Creating indexes...';
    CREATE NONCLUSTERED INDEX [IX_QuerySuggestions_CategoryId_IsActive_SortOrder] 
    ON [dbo].[QuerySuggestions] ([CategoryId] ASC, [IsActive] ASC, [SortOrder] ASC);
    
    CREATE NONCLUSTERED INDEX [IX_QuerySuggestions_UsageCount_LastUsed] 
    ON [dbo].[QuerySuggestions] ([UsageCount] DESC, [LastUsed] DESC);
    
    CREATE NONCLUSTERED INDEX [IX_QuerySuggestions_IsActive] 
    ON [dbo].[QuerySuggestions] ([IsActive] ASC);
    
    CREATE NONCLUSTERED INDEX [IX_QuerySuggestions_Relevance] 
    ON [dbo].[QuerySuggestions] ([Relevance] DESC);
    
    PRINT 'QuerySuggestions Id column conversion completed successfully!';
END
ELSE IF @CurrentDataType = 'bigint'
BEGIN
    PRINT 'QuerySuggestions.Id is already BIGINT. No conversion needed.';
END
ELSE
BEGIN
    PRINT 'ERROR: QuerySuggestions table or Id column not found!';
END

PRINT 'QuerySuggestions Id data type fix completed.';
