-- Check the actual structure of the problematic tables
-- Run this first to see what columns exist

PRINT 'Checking table structures...';
PRINT '';

-- Check AIGenerationAttempts structure
IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'AIGenerationAttempts')
BEGIN
    PRINT 'AIGenerationAttempts columns:';
    SELECT 
        COLUMN_NAME,
        DATA_TYPE,
        IS_NULLABLE,
        CHARACTER_MAXIMUM_LENGTH,
        NUMERIC_PRECISION,
        NUMERIC_SCALE
    FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'AIGenerationAttempts'
    ORDER BY ORDINAL_POSITION;
    PRINT '';
END
ELSE
BEGIN
    PRINT 'AIGenerationAttempts table does not exist';
    PRINT '';
END

-- Check AIFeedbackEntries structure
IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'AIFeedbackEntries')
BEGIN
    PRINT 'AIFeedbackEntries columns:';
    SELECT 
        COLUMN_NAME,
        DATA_TYPE,
        IS_NULLABLE,
        CHARACTER_MAXIMUM_LENGTH,
        NUMERIC_PRECISION,
        NUMERIC_SCALE
    FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'AIFeedbackEntries'
    ORDER BY ORDINAL_POSITION;
    PRINT '';
END
ELSE
BEGIN
    PRINT 'AIFeedbackEntries table does not exist';
    PRINT '';
END

-- Check SemanticCacheEntries structure
IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'SemanticCacheEntries')
BEGIN
    PRINT 'SemanticCacheEntries columns:';
    SELECT 
        COLUMN_NAME,
        DATA_TYPE,
        IS_NULLABLE,
        CHARACTER_MAXIMUM_LENGTH,
        NUMERIC_PRECISION,
        NUMERIC_SCALE
    FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'SemanticCacheEntries'
    ORDER BY ORDINAL_POSITION;
    PRINT '';
END
ELSE
BEGIN
    PRINT 'SemanticCacheEntries table does not exist';
    PRINT '';
END

-- Check for foreign key constraints
PRINT 'Foreign key constraints referencing these tables:';
SELECT 
    fk.name AS constraint_name,
    OBJECT_NAME(fk.parent_object_id) AS referencing_table,
    OBJECT_NAME(fk.referenced_object_id) AS referenced_table,
    COL_NAME(fkc.parent_object_id, fkc.parent_column_id) AS referencing_column,
    COL_NAME(fkc.referenced_object_id, fkc.referenced_column_id) AS referenced_column
FROM sys.foreign_keys fk
INNER JOIN sys.foreign_key_columns fkc ON fk.object_id = fkc.constraint_object_id
WHERE fkc.referenced_object_id IN (
    OBJECT_ID('AIGenerationAttempts'),
    OBJECT_ID('AIFeedbackEntries'),
    OBJECT_ID('SemanticCacheEntries')
)
ORDER BY referenced_table, referencing_table;
