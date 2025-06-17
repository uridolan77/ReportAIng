-- Migration: Fix SuggestionCategories Id column data type mismatch
-- This fixes the data type mismatch where Entity Framework expects BIGINT but database has INT

USE [BIReportingCopilot_Dev];
GO

PRINT 'Starting SuggestionCategories Id column data type fix...';

-- Check current data type
DECLARE @CurrentDataType NVARCHAR(50);
SELECT @CurrentDataType = DATA_TYPE 
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'SuggestionCategories' AND COLUMN_NAME = 'Id';

PRINT 'Current Id column data type: ' + ISNULL(@CurrentDataType, 'NOT FOUND');

-- Only proceed if the column is currently INT
IF @CurrentDataType = 'int'
BEGIN
    PRINT 'Converting SuggestionCategories.Id from INT to BIGINT...';
    
    -- Step 1: Check if there are any foreign key constraints referencing this table
    IF EXISTS (
        SELECT 1 
        FROM sys.foreign_keys fk
        INNER JOIN sys.foreign_key_columns fkc ON fk.object_id = fkc.constraint_object_id
        INNER JOIN sys.columns c ON fkc.referenced_object_id = c.object_id AND fkc.referenced_column_id = c.column_id
        INNER JOIN sys.tables t ON c.object_id = t.object_id
        WHERE t.name = 'SuggestionCategories' AND c.name = 'Id'
    )
    BEGIN
        PRINT 'Found foreign key constraints. Dropping them temporarily...';
        
        -- Drop foreign key constraint from QuerySuggestions table
        IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_QuerySuggestions_SuggestionCategories_CategoryId')
        BEGIN
            PRINT 'Dropping FK_QuerySuggestions_SuggestionCategories_CategoryId...';
            ALTER TABLE [dbo].[QuerySuggestions] DROP CONSTRAINT [FK_QuerySuggestions_SuggestionCategories_CategoryId];
        END
        
        -- Check for any other foreign keys
        DECLARE @FKName NVARCHAR(128);
        DECLARE fk_cursor CURSOR FOR
        SELECT fk.name
        FROM sys.foreign_keys fk
        INNER JOIN sys.foreign_key_columns fkc ON fk.object_id = fkc.constraint_object_id
        INNER JOIN sys.columns c ON fkc.referenced_object_id = c.object_id AND fkc.referenced_column_id = c.column_id
        INNER JOIN sys.tables t ON c.object_id = t.object_id
        WHERE t.name = 'SuggestionCategories' AND c.name = 'Id';
        
        OPEN fk_cursor;
        FETCH NEXT FROM fk_cursor INTO @FKName;
        
        WHILE @@FETCH_STATUS = 0
        BEGIN
            PRINT 'Dropping foreign key: ' + @FKName;
            DECLARE @DropSQL NVARCHAR(MAX) = 'ALTER TABLE [dbo].[QuerySuggestions] DROP CONSTRAINT [' + @FKName + ']';
            EXEC sp_executesql @DropSQL;
            FETCH NEXT FROM fk_cursor INTO @FKName;
        END
        
        CLOSE fk_cursor;
        DEALLOCATE fk_cursor;
    END
    
    -- Step 2: Convert CategoryId in QuerySuggestions table to BIGINT first (if it exists and is INT)
    IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'QuerySuggestions' AND COLUMN_NAME = 'CategoryId')
    BEGIN
        DECLARE @CategoryIdDataType NVARCHAR(50);
        SELECT @CategoryIdDataType = DATA_TYPE
        FROM INFORMATION_SCHEMA.COLUMNS
        WHERE TABLE_NAME = 'QuerySuggestions' AND COLUMN_NAME = 'CategoryId';

        IF @CategoryIdDataType = 'int'
        BEGIN
            PRINT 'Converting QuerySuggestions.CategoryId from INT to BIGINT...';

            -- Check if there are NULL values in CategoryId
            DECLARE @NullCount INT;
            SELECT @NullCount = COUNT(*) FROM [dbo].[QuerySuggestions] WHERE [CategoryId] IS NULL;

            IF @NullCount > 0
            BEGIN
                PRINT 'Found ' + CAST(@NullCount AS NVARCHAR(10)) + ' NULL values in CategoryId. Creating default category...';

                -- Create a default category if it doesn't exist
                IF NOT EXISTS (SELECT 1 FROM [dbo].[SuggestionCategories] WHERE [Name] = 'Default')
                BEGIN
                    INSERT INTO [dbo].[SuggestionCategories] ([Name], [Description], [IsActive], [DisplayOrder], [CreatedAt], [UpdatedAt], [CreatedBy])
                    VALUES ('Default', 'Default category for uncategorized suggestions', 1, 999, GETUTCDATE(), GETUTCDATE(), 'System');
                    PRINT 'Created default category.';
                END

                -- Get the default category ID
                DECLARE @DefaultCategoryId INT;
                SELECT @DefaultCategoryId = [Id] FROM [dbo].[SuggestionCategories]
                WHERE [Name] = 'Default';

                -- Update NULL CategoryId values to point to default category
                UPDATE [dbo].[QuerySuggestions]
                SET [CategoryId] = @DefaultCategoryId
                WHERE [CategoryId] IS NULL;

                PRINT 'Updated NULL CategoryId values to default category (' + CAST(@DefaultCategoryId AS NVARCHAR(10)) + ').';
            END

            -- Now convert to BIGINT
            ALTER TABLE [dbo].[QuerySuggestions] ALTER COLUMN [CategoryId] BIGINT NOT NULL;
            PRINT 'QuerySuggestions.CategoryId converted to BIGINT successfully.';
        END
        ELSE
        BEGIN
            PRINT 'QuerySuggestions.CategoryId is already BIGINT or does not exist.';
        END
    END
    
    -- Step 3: Convert the primary key column
    PRINT 'Converting SuggestionCategories.Id from INT IDENTITY to BIGINT IDENTITY...';
    
    -- This requires recreating the table since we can't directly alter an IDENTITY column's data type
    -- First, create a backup of the data
    SELECT * INTO #SuggestionCategories_Backup FROM [dbo].[SuggestionCategories];
    PRINT 'Created backup of SuggestionCategories data.';
    
    -- Drop the original table
    DROP TABLE [dbo].[SuggestionCategories];
    PRINT 'Dropped original SuggestionCategories table.';
    
    -- Recreate the table with BIGINT Id
    CREATE TABLE [dbo].[SuggestionCategories] (
        [Id] BIGINT IDENTITY(1,1) NOT NULL,
        [Name] NVARCHAR(100) NOT NULL,
        [Description] NVARCHAR(500) NULL,
        [IsActive] BIT NOT NULL DEFAULT(1),
        [DisplayOrder] INT NOT NULL DEFAULT(0),
        [CreatedAt] DATETIME2(7) NOT NULL,
        [UpdatedAt] DATETIME2(7) NOT NULL,
        [CreatedBy] NVARCHAR(256) NULL,
        [UpdatedBy] NVARCHAR(256) NULL,
        CONSTRAINT [PK_SuggestionCategories] PRIMARY KEY CLUSTERED ([Id] ASC)
    );
    PRINT 'Created new SuggestionCategories table with BIGINT Id.';
    
    -- Restore the data with IDENTITY_INSERT
    SET IDENTITY_INSERT [dbo].[SuggestionCategories] ON;

    INSERT INTO [dbo].[SuggestionCategories] ([Id], [Name], [Description], [IsActive], [DisplayOrder], [CreatedAt], [UpdatedAt], [CreatedBy], [UpdatedBy])
    SELECT [Id],
           [Name],
           [Description],
           [IsActive],
           [DisplayOrder],
           [CreatedAt],
           [UpdatedAt],
           [CreatedBy],
           [UpdatedBy]
    FROM #SuggestionCategories_Backup;

    SET IDENTITY_INSERT [dbo].[SuggestionCategories] OFF;
    PRINT 'Restored data to new SuggestionCategories table.';
    
    -- Drop the backup table
    DROP TABLE #SuggestionCategories_Backup;
    PRINT 'Dropped backup table.';
    
    -- Step 4: Recreate foreign key constraints
    IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'QuerySuggestions')
    BEGIN
        PRINT 'Recreating foreign key constraint...';
        ALTER TABLE [dbo].[QuerySuggestions] 
        ADD CONSTRAINT [FK_QuerySuggestions_SuggestionCategories_CategoryId] 
        FOREIGN KEY ([CategoryId]) REFERENCES [dbo].[SuggestionCategories]([Id]) ON DELETE CASCADE;
        PRINT 'Foreign key constraint recreated successfully.';
    END
    
    -- Step 5: Create indexes
    PRINT 'Creating indexes...';
    CREATE NONCLUSTERED INDEX [IX_SuggestionCategories_IsActive_DisplayOrder] 
    ON [dbo].[SuggestionCategories] ([IsActive] ASC, [DisplayOrder] ASC);
    
    PRINT 'SuggestionCategories Id column conversion completed successfully!';
END
ELSE IF @CurrentDataType = 'bigint'
BEGIN
    PRINT 'SuggestionCategories.Id is already BIGINT. No conversion needed.';
END
ELSE
BEGIN
    PRINT 'ERROR: SuggestionCategories table or Id column not found!';
END

PRINT 'SuggestionCategories Id data type fix completed.';
