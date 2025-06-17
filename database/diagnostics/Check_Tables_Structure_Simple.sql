-- Fixed query to check BusinessSchemaVersions and related tables

USE [BIReportingCopilot_Dev]
GO

PRINT '=== CHECKING BusinessSchemaVersions TABLE ===';

IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'BusinessSchemaVersions')
BEGIN
    PRINT 'BusinessSchemaVersions table structure:';
    SELECT 
        COLUMN_NAME as ColumnName,
        DATA_TYPE as DataType,
        IS_NULLABLE as IsNullable,
        COLUMN_DEFAULT as DefaultValue
    FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'BusinessSchemaVersions'
    ORDER BY ORDINAL_POSITION;

    PRINT '';
    PRINT 'Sample data from BusinessSchemaVersions:';
    SELECT TOP 3 * FROM BusinessSchemaVersions;
END
ELSE
BEGIN
    PRINT '❌ BusinessSchemaVersions table does not exist!';
END

PRINT '';
PRINT '=== CHECKING SchemaTableContexts COLUMNS ===';

IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'SchemaTableContexts')
BEGIN
    PRINT 'SchemaTableContexts table structure:';
    SELECT 
        COLUMN_NAME as ColumnName,
        DATA_TYPE as DataType,
        IS_NULLABLE as IsNullable
    FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'SchemaTableContexts'
    ORDER BY ORDINAL_POSITION;    PRINT '';
    PRINT 'Row count in SchemaTableContexts:';
    SELECT COUNT(*) as [RowCount] FROM SchemaTableContexts;
END

PRINT '';
PRINT '=== CHECKING SchemaColumnContexts COLUMNS ===';

IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'SchemaColumnContexts')
BEGIN
    PRINT 'SchemaColumnContexts table structure:';
    SELECT 
        COLUMN_NAME as ColumnName,
        DATA_TYPE as DataType,
        IS_NULLABLE as IsNullable
    FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'SchemaColumnContexts'
    ORDER BY ORDINAL_POSITION;    PRINT '';
    PRINT 'Row count in SchemaColumnContexts:';
    SELECT COUNT(*) as [RowCount] FROM SchemaColumnContexts;
END
