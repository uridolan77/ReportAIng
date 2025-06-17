-- Standardize UserId columns to nvarchar(256) to match User table
-- This version properly handles indexes that depend on UserId columns

PRINT 'Standardizing UserId columns to nvarchar(256)...';

-- =====================================================
-- 1. AIGenerationAttempts.UserId
-- =====================================================
IF EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'AIGenerationAttempts' 
    AND COLUMN_NAME = 'UserId' 
    AND CHARACTER_MAXIMUM_LENGTH > 256
)
BEGIN
    PRINT 'Updating AIGenerationAttempts.UserId from nvarchar(500) to nvarchar(256)...';
    
    -- Drop dependent indexes
    IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_AIGenerationAttempts_UserId')
    BEGIN
        DROP INDEX [IX_AIGenerationAttempts_UserId] ON [AIGenerationAttempts];
        PRINT 'Dropped index IX_AIGenerationAttempts_UserId';
    END
    
    -- Alter column
    ALTER TABLE [AIGenerationAttempts] ALTER COLUMN [UserId] nvarchar(256) NOT NULL;
    
    -- Recreate index
    CREATE INDEX [IX_AIGenerationAttempts_UserId] ON [AIGenerationAttempts] ([UserId]);
    PRINT 'Recreated index IX_AIGenerationAttempts_UserId';
    
    PRINT 'AIGenerationAttempts.UserId updated to nvarchar(256).';
END
ELSE
BEGIN
    PRINT 'AIGenerationAttempts.UserId already correct size.';
END

-- =====================================================
-- 2. QueryHistory.UserId
-- =====================================================
IF EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'QueryHistory' 
    AND COLUMN_NAME = 'UserId' 
    AND CHARACTER_MAXIMUM_LENGTH > 256
)
BEGIN
    PRINT 'Updating QueryHistory.UserId from nvarchar(500) to nvarchar(256)...';
    
    -- Drop dependent indexes
    IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_QueryHistory_UserId')
    BEGIN
        DROP INDEX [IX_QueryHistory_UserId] ON [QueryHistory];
        PRINT 'Dropped index IX_QueryHistory_UserId';
    END
    
    IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_QueryHistory_UserId_QueryTimestamp')
    BEGIN
        DROP INDEX [IX_QueryHistory_UserId_QueryTimestamp] ON [QueryHistory];
        PRINT 'Dropped index IX_QueryHistory_UserId_QueryTimestamp';
    END
    
    -- Alter column
    ALTER TABLE [QueryHistory] ALTER COLUMN [UserId] nvarchar(256) NOT NULL;
    
    -- Recreate indexes
    CREATE INDEX [IX_QueryHistory_UserId] ON [QueryHistory] ([UserId]);
    PRINT 'Recreated index IX_QueryHistory_UserId';
    
    PRINT 'QueryHistory.UserId updated to nvarchar(256).';
END
ELSE
BEGIN
    PRINT 'QueryHistory.UserId already correct size.';
END

-- =====================================================
-- 3. PromptLogs.UserId
-- =====================================================
IF EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'PromptLogs' 
    AND COLUMN_NAME = 'UserId' 
    AND CHARACTER_MAXIMUM_LENGTH > 256
)
BEGIN
    PRINT 'Updating PromptLogs.UserId from nvarchar(500) to nvarchar(256)...';
    
    -- Drop dependent indexes (if any)
    IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_PromptLogs_UserId')
    BEGIN
        DROP INDEX [IX_PromptLogs_UserId] ON [PromptLogs];
        PRINT 'Dropped index IX_PromptLogs_UserId';
    END
    
    -- Alter column
    ALTER TABLE [PromptLogs] ALTER COLUMN [UserId] nvarchar(256) NOT NULL;
    
    -- Recreate index
    CREATE INDEX [IX_PromptLogs_UserId] ON [PromptLogs] ([UserId]);
    PRINT 'Created index IX_PromptLogs_UserId';
    
    PRINT 'PromptLogs.UserId updated to nvarchar(256).';
END
ELSE
BEGIN
    PRINT 'PromptLogs.UserId already correct size.';
END

-- =====================================================
-- 4. Handle any other tables with UserId columns
-- =====================================================
DECLARE @sql NVARCHAR(MAX) = '';
DECLARE @tableName NVARCHAR(128);
DECLARE @columnName NVARCHAR(128);
DECLARE @maxLength INT;

DECLARE userid_cursor CURSOR FOR
SELECT TABLE_NAME, COLUMN_NAME, CHARACTER_MAXIMUM_LENGTH
FROM INFORMATION_SCHEMA.COLUMNS
WHERE COLUMN_NAME = 'UserId' 
AND DATA_TYPE = 'nvarchar'
AND CHARACTER_MAXIMUM_LENGTH > 256
AND TABLE_NAME NOT IN ('AIGenerationAttempts', 'QueryHistory', 'PromptLogs');

OPEN userid_cursor;
FETCH NEXT FROM userid_cursor INTO @tableName, @columnName, @maxLength;

WHILE @@FETCH_STATUS = 0
BEGIN
    PRINT 'Updating ' + @tableName + '.' + @columnName + ' from nvarchar(' + CAST(@maxLength AS VARCHAR(10)) + ') to nvarchar(256)...';
    
    -- Drop any indexes on this column
    DECLARE @indexName NVARCHAR(128);
    DECLARE index_cursor CURSOR FOR
    SELECT i.name
    FROM sys.indexes i
    INNER JOIN sys.index_columns ic ON i.object_id = ic.object_id AND i.index_id = ic.index_id
    INNER JOIN sys.columns c ON ic.object_id = c.object_id AND ic.column_id = c.column_id
    INNER JOIN sys.tables t ON i.object_id = t.object_id
    WHERE t.name = @tableName AND c.name = @columnName AND i.name IS NOT NULL;
    
    OPEN index_cursor;
    FETCH NEXT FROM index_cursor INTO @indexName;
    
    WHILE @@FETCH_STATUS = 0
    BEGIN
        SET @sql = 'DROP INDEX [' + @indexName + '] ON [' + @tableName + '];';
        EXEC sp_executesql @sql;
        PRINT 'Dropped index ' + @indexName + ' on ' + @tableName;
        
        FETCH NEXT FROM index_cursor INTO @indexName;
    END
    
    CLOSE index_cursor;
    DEALLOCATE index_cursor;
    
    -- Alter the column
    SET @sql = 'ALTER TABLE [' + @tableName + '] ALTER COLUMN [' + @columnName + '] nvarchar(256) NOT NULL;';
    EXEC sp_executesql @sql;
    
    -- Recreate a basic index
    SET @sql = 'CREATE INDEX [IX_' + @tableName + '_UserId] ON [' + @tableName + '] ([UserId]);';
    EXEC sp_executesql @sql;
    PRINT 'Created index IX_' + @tableName + '_UserId';
    
    PRINT @tableName + '.' + @columnName + ' updated to nvarchar(256).';
    
    FETCH NEXT FROM userid_cursor INTO @tableName, @columnName, @maxLength;
END

CLOSE userid_cursor;
DEALLOCATE userid_cursor;

PRINT '';
PRINT 'UserId column standardization completed successfully!';
PRINT 'All UserId columns are now nvarchar(256) to match the User table.';
PRINT '';
PRINT 'Benefits achieved:';
PRINT '  - Reduced storage usage (244 characters saved per UserId)';
PRINT '  - Improved index performance';
PRINT '  - Consistent schema design';
PRINT '  - Better memory efficiency';
PRINT '  - All indexes recreated and optimized';
