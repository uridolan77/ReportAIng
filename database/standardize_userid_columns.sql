-- Standardize UserId columns to nvarchar(256) to match User table
-- This optimizes storage and improves performance

PRINT 'Standardizing UserId columns to nvarchar(256)...';

-- Check and update AIGenerationAttempts.UserId
IF EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'AIGenerationAttempts' 
    AND COLUMN_NAME = 'UserId' 
    AND CHARACTER_MAXIMUM_LENGTH > 256
)
BEGIN
    PRINT 'Updating AIGenerationAttempts.UserId from nvarchar(500) to nvarchar(256)...';
    ALTER TABLE [AIGenerationAttempts] ALTER COLUMN [UserId] nvarchar(256) NOT NULL;
    PRINT 'AIGenerationAttempts.UserId updated.';
END
ELSE
BEGIN
    PRINT 'AIGenerationAttempts.UserId already correct size.';
END

-- Check and update AIFeedbackEntries.UserId (if it exists)
IF EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'AIFeedbackEntries' 
    AND COLUMN_NAME = 'UserId' 
    AND CHARACTER_MAXIMUM_LENGTH > 256
)
BEGIN
    PRINT 'Updating AIFeedbackEntries.UserId from nvarchar(500) to nvarchar(256)...';
    ALTER TABLE [AIFeedbackEntries] ALTER COLUMN [UserId] nvarchar(256) NOT NULL;
    PRINT 'AIFeedbackEntries.UserId updated.';
END

-- Check and update QueryHistory.UserId
IF EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'QueryHistory' 
    AND COLUMN_NAME = 'UserId' 
    AND CHARACTER_MAXIMUM_LENGTH > 256
)
BEGIN
    PRINT 'Updating QueryHistory.UserId from nvarchar(500) to nvarchar(256)...';
    ALTER TABLE [QueryHistory] ALTER COLUMN [UserId] nvarchar(256) NOT NULL;
    PRINT 'QueryHistory.UserId updated.';
END
ELSE
BEGIN
    PRINT 'QueryHistory.UserId already correct size.';
END

-- Check and update PromptLogs.UserId
IF EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'PromptLogs' 
    AND COLUMN_NAME = 'UserId' 
    AND CHARACTER_MAXIMUM_LENGTH > 256
)
BEGIN
    PRINT 'Updating PromptLogs.UserId from nvarchar(500) to nvarchar(256)...';
    ALTER TABLE [PromptLogs] ALTER COLUMN [UserId] nvarchar(256) NOT NULL;
    PRINT 'PromptLogs.UserId updated.';
END
ELSE
BEGIN
    PRINT 'PromptLogs.UserId already correct size.';
END

-- Check and update any other tables with UserId columns
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
AND TABLE_NAME NOT IN ('AIGenerationAttempts', 'AIFeedbackEntries', 'QueryHistory', 'PromptLogs');

OPEN userid_cursor;
FETCH NEXT FROM userid_cursor INTO @tableName, @columnName, @maxLength;

WHILE @@FETCH_STATUS = 0
BEGIN
    PRINT 'Updating ' + @tableName + '.' + @columnName + ' from nvarchar(' + CAST(@maxLength AS VARCHAR(10)) + ') to nvarchar(256)...';
    SET @sql = 'ALTER TABLE [' + @tableName + '] ALTER COLUMN [' + @columnName + '] nvarchar(256) NOT NULL;';
    EXEC sp_executesql @sql;
    PRINT @tableName + '.' + @columnName + ' updated.';
    
    FETCH NEXT FROM userid_cursor INTO @tableName, @columnName, @maxLength;
END

CLOSE userid_cursor;
DEALLOCATE userid_cursor;

PRINT '';
PRINT 'UserId column standardization completed!';
PRINT 'All UserId columns are now nvarchar(256) to match the User table.';
PRINT '';
PRINT 'Benefits:';
PRINT '  - Reduced storage usage (244 characters saved per UserId)';
PRINT '  - Improved index performance';
PRINT '  - Consistent schema design';
PRINT '  - Better memory efficiency';
