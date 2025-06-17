-- Check BusinessSchemaVersions table structure and relationships

USE [BIReportingCopilot_Dev]
GO

PRINT '=== CHECKING BusinessSchemaVersions TABLE ===';

-- Check if table exists and its structure
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'BusinessSchemaVersions')
BEGIN
    PRINT 'BusinessSchemaVersions table structure:';
    SELECT 
        COLUMN_NAME as ColumnName,
        DATA_TYPE as DataType,
        IS_NULLABLE as IsNullable,
        COLUMN_DEFAULT as DefaultValue,
        CHARACTER_MAXIMUM_LENGTH as MaxLength
    FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'BusinessSchemaVersions'
    ORDER BY ORDINAL_POSITION;

    PRINT '';
    PRINT 'Checking for required columns in BusinessSchemaVersions:';
    SELECT 
        'IsCurrent' as RequiredColumn,
        CASE WHEN EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'BusinessSchemaVersions' AND COLUMN_NAME = 'IsCurrent') 
             THEN '✓ EXISTS' ELSE '❌ MISSING' END as Status
    UNION ALL
    SELECT 
        'IsActive' as RequiredColumn,
        CASE WHEN EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'BusinessSchemaVersions' AND COLUMN_NAME = 'IsActive') 
             THEN '✓ EXISTS' ELSE '❌ MISSING' END as Status;

    PRINT '';
    PRINT 'Sample data from BusinessSchemaVersions:';
    SELECT TOP 3 * FROM BusinessSchemaVersions;

END
ELSE
BEGIN
    PRINT '❌ BusinessSchemaVersions table does not exist!';
END

PRINT '';
PRINT '=== CHECKING SchemaTableContexts TABLE ===';

-- Check SchemaTableContexts structure  
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'SchemaTableContexts')
BEGIN
    PRINT 'Checking for required columns in SchemaTableContexts:';
    SELECT 
        'IsActive' as RequiredColumn,
        CASE WHEN EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'SchemaTableContexts' AND COLUMN_NAME = 'IsActive') 
             THEN '✓ EXISTS' ELSE '❌ MISSING' END as Status
    UNION ALL
    SELECT 
        'IsDefault' as RequiredColumn,
        CASE WHEN EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'SchemaTableContexts' AND COLUMN_NAME = 'IsDefault') 
             THEN '✓ EXISTS' ELSE '❌ MISSING' END as Status;

    PRINT '';
    PRINT 'Sample data from SchemaTableContexts:';
    SELECT TOP 3 
        Id, SchemaVersionId, TableName, SchemaName, BusinessPurpose
    FROM SchemaTableContexts;
END

PRINT '';
PRINT '=== CHECKING SchemaColumnContexts TABLE ===';

-- Check SchemaColumnContexts structure
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'SchemaColumnContexts')
BEGIN
    PRINT 'Checking for required columns in SchemaColumnContexts:';
    SELECT 
        'IsActive' as RequiredColumn,
        CASE WHEN EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'SchemaColumnContexts' AND COLUMN_NAME = 'IsActive') 
             THEN '✓ EXISTS' ELSE '❌ MISSING' END as Status
    UNION ALL
    SELECT 
        'IsDefault' as RequiredColumn,
        CASE WHEN EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'SchemaColumnContexts' AND COLUMN_NAME = 'IsDefault') 
             THEN '✓ EXISTS' ELSE '❌ MISSING' END as Status;

    PRINT '';
    PRINT 'Sample data from SchemaColumnContexts:';
    SELECT TOP 3 
        Id, TableContextId, ColumnName, DataType, BusinessMeaning
    FROM SchemaColumnContexts;
END

PRINT '';
PRINT '=== NAVIGATION RELATIONSHIP CHECK ===';
PRINT 'Expected navigation path:';
PRINT 'BusinessSchemas -> BusinessSchemaVersions -> SchemaTableContexts -> SchemaColumnContexts';
PRINT '';

-- Check if all foreign key relationships exist
PRINT 'Foreign key relationships:';
SELECT 
    fk.name AS ForeignKeyName,
    tp.name AS ParentTable,
    cp.name AS ParentColumn,
    tr.name AS ReferencedTable,
    cr.name AS ReferencedColumn
FROM sys.foreign_keys fk
INNER JOIN sys.tables tp ON fk.parent_object_id = tp.object_id
INNER JOIN sys.tables tr ON fk.referenced_object_id = tr.object_id
INNER JOIN sys.foreign_key_columns fkc ON fkc.constraint_object_id = fk.object_id
INNER JOIN sys.columns cp ON fkc.parent_column_id = cp.column_id AND fkc.parent_object_id = cp.object_id
INNER JOIN sys.columns cr ON fkc.referenced_column_id = cr.column_id AND fkc.referenced_object_id = cr.object_id
WHERE tp.name IN ('BusinessSchemaVersions', 'SchemaTableContexts', 'SchemaColumnContexts')
   OR tr.name IN ('BusinessSchemas', 'BusinessSchemaVersions', 'SchemaTableContexts')
ORDER BY tp.name, fk.name;
