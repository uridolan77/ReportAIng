-- Step 1: Drop all foreign key constraints that reference the problematic tables
-- Run this first, then run the main fix script

PRINT 'Finding and dropping foreign key constraints...';

-- Drop foreign keys referencing AIGenerationAttempts
DECLARE @sql NVARCHAR(MAX) = '';
SELECT @sql = @sql + 'ALTER TABLE [' + SCHEMA_NAME(fk.schema_id) + '].[' + OBJECT_NAME(fk.parent_object_id) + '] DROP CONSTRAINT [' + fk.name + '];' + CHAR(13)
FROM sys.foreign_keys fk
INNER JOIN sys.foreign_key_columns fkc ON fk.object_id = fkc.constraint_object_id
WHERE fkc.referenced_object_id = OBJECT_ID('AIGenerationAttempts');

IF @sql <> ''
BEGIN
    PRINT 'Dropping foreign key constraints referencing AIGenerationAttempts:';
    PRINT @sql;
    EXEC sp_executesql @sql;
END
ELSE
BEGIN
    PRINT 'No foreign key constraints found referencing AIGenerationAttempts';
END

-- Drop foreign keys referencing AIFeedbackEntries
SET @sql = '';
SELECT @sql = @sql + 'ALTER TABLE [' + SCHEMA_NAME(fk.schema_id) + '].[' + OBJECT_NAME(fk.parent_object_id) + '] DROP CONSTRAINT [' + fk.name + '];' + CHAR(13)
FROM sys.foreign_keys fk
INNER JOIN sys.foreign_key_columns fkc ON fk.object_id = fkc.constraint_object_id
WHERE fkc.referenced_object_id = OBJECT_ID('AIFeedbackEntries');

IF @sql <> ''
BEGIN
    PRINT 'Dropping foreign key constraints referencing AIFeedbackEntries:';
    PRINT @sql;
    EXEC sp_executesql @sql;
END
ELSE
BEGIN
    PRINT 'No foreign key constraints found referencing AIFeedbackEntries';
END

-- Drop foreign keys referencing SemanticCacheEntries
SET @sql = '';
SELECT @sql = @sql + 'ALTER TABLE [' + SCHEMA_NAME(fk.schema_id) + '].[' + OBJECT_NAME(fk.parent_object_id) + '] DROP CONSTRAINT [' + fk.name + '];' + CHAR(13)
FROM sys.foreign_keys fk
INNER JOIN sys.foreign_key_columns fkc ON fk.object_id = fkc.constraint_object_id
WHERE fkc.referenced_object_id = OBJECT_ID('SemanticCacheEntries');

IF @sql <> ''
BEGIN
    PRINT 'Dropping foreign key constraints referencing SemanticCacheEntries:';
    PRINT @sql;
    EXEC sp_executesql @sql;
END
ELSE
BEGIN
    PRINT 'No foreign key constraints found referencing SemanticCacheEntries';
END

PRINT 'Foreign key constraint removal completed!';
PRINT 'Now you can run the main fix_identity_tables.sql script';
