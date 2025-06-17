-- Diagnostic query to check what BusinessSchema-related tables are missing
-- This will help identify the complete database schema structure needed

USE [BIReportingCopilot_Dev]
GO

PRINT '=== CHECKING BUSINESSSCHEMA-RELATED TABLES ===';
PRINT '';

-- Check what tables exist that are related to BusinessSchemas
PRINT 'Tables that exist with "Schema" or "Business" in the name:';
SELECT 
    TABLE_SCHEMA as SchemaName,
    TABLE_NAME as TableName,
    TABLE_TYPE as TableType
FROM INFORMATION_SCHEMA.TABLES 
WHERE TABLE_NAME LIKE '%Schema%' 
   OR TABLE_NAME LIKE '%Business%'
ORDER BY TABLE_NAME;

PRINT '';
PRINT '=== CHECKING EXPECTED TABLES FROM ENTITY FRAMEWORK MODEL ===';

-- Expected tables based on the Include statements in SchemaManagementService:
-- .Include(s => s.Versions.Where(v => v.IsCurrent))
-- .ThenInclude(v => v.TableContexts)  
-- .ThenInclude(t => t.ColumnContexts)

DECLARE @ExpectedTables TABLE (
    TableName NVARCHAR(128),
    Purpose NVARCHAR(255),
    Required BIT
);

INSERT INTO @ExpectedTables VALUES 
    ('BusinessSchemas', 'Main schema definitions', 1),
    ('SchemaVersions', 'Schema versions (s.Versions)', 1),
    ('SchemaTableContexts', 'Table contexts (v.TableContexts)', 1),
    ('SchemaColumnContexts', 'Column contexts (t.ColumnContexts)', 1),
    ('BusinessSchemaVersions', 'Alternative name for versions', 0),
    ('TableContexts', 'Alternative name for table contexts', 0),
    ('ColumnContexts', 'Alternative name for column contexts', 0);

PRINT '';
PRINT 'Expected vs Actual table existence:';
SELECT 
    et.TableName,
    et.Purpose,
    et.Required,
    CASE 
        WHEN EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = et.TableName)
        THEN '✓ EXISTS' 
        ELSE '❌ MISSING' 
    END as Status
FROM @ExpectedTables et
ORDER BY et.Required DESC, et.TableName;

PRINT '';
PRINT '=== CHECKING BUSINESSSCHEMAS RELATIONSHIPS ===';

-- Check foreign key relationships from BusinessSchemas
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'BusinessSchemas')
BEGIN
    PRINT 'Foreign keys FROM BusinessSchemas table:';
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
    WHERE tp.name = 'BusinessSchemas'
    ORDER BY fk.name;

    PRINT '';
    PRINT 'Foreign keys TO BusinessSchemas table:';
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
    WHERE tr.name = 'BusinessSchemas'
    ORDER BY fk.name;
END

PRINT '';
PRINT '=== BUSINESSSCHEMAS TABLE STRUCTURE ===';
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'BusinessSchemas')
BEGIN
    SELECT 
        COLUMN_NAME as ColumnName,
        DATA_TYPE as DataType,
        IS_NULLABLE as IsNullable,
        COLUMN_DEFAULT as DefaultValue,
        CHARACTER_MAXIMUM_LENGTH as MaxLength
    FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'BusinessSchemas'
    ORDER BY ORDINAL_POSITION;
END

PRINT '';
PRINT '=== SAMPLE DATA CHECK ===';
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'BusinessSchemas')
BEGIN
    PRINT 'BusinessSchemas sample data:';
    SELECT TOP 3 
        Id,
        Name,
        [Description],
        IsActive,
        IsDefault,
        CreatedAt,
        UpdatedAt
    FROM BusinessSchemas;
END

PRINT '';
PRINT '=== ENTITY FRAMEWORK NAVIGATION PROPERTIES ANALYSIS ===';
PRINT 'Based on SchemaManagementService.cs, the expected navigation structure is:';
PRINT 'BusinessSchema.Versions -> SchemaVersion.TableContexts -> TableContext.ColumnContexts';
PRINT '';
PRINT 'Missing tables that need to be created:';
PRINT '1. SchemaVersions (or BusinessSchemaVersions)';
PRINT '2. Proper TableContexts with foreign keys';
PRINT '3. Proper ColumnContexts with foreign keys';
PRINT '4. All tables need IsActive and IsDefault columns';
