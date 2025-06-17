-- Diagnostic query to check BusinessSchemas table contents
-- This will help us understand why the API is returning auto-generated schema

USE [BIReportingCopilot_Dev]
GO

PRINT '=== BUSINESSSCHEMAS DIAGNOSTIC QUERY ===';
PRINT '';

-- Check if BusinessSchemas table exists
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'BusinessSchemas')
BEGIN
    PRINT '✓ BusinessSchemas table exists';
    
    -- Check table structure
    PRINT '';
    PRINT 'Table structure:';
    SELECT 
        COLUMN_NAME,
        DATA_TYPE,
        IS_NULLABLE,
        COLUMN_DEFAULT
    FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'BusinessSchemas'
    ORDER BY ORDINAL_POSITION;
    
    -- Count total records
    DECLARE @TotalCount INT;
    SELECT @TotalCount = COUNT(*) FROM BusinessSchemas;
    PRINT '';
    PRINT 'Total records in BusinessSchemas: ' + CAST(@TotalCount AS VARCHAR(10));
    
    IF @TotalCount > 0
    BEGIN
        -- Show all records with key fields
        PRINT '';
        PRINT 'All BusinessSchemas records:';
        SELECT 
            Id,
            Name,
            Description,
            IsActive,
            IsDefault,
            CreatedAt,
            UpdatedAt
        FROM BusinessSchemas
        ORDER BY CreatedAt DESC;
        
        -- Check for active schemas
        DECLARE @ActiveCount INT;
        SELECT @ActiveCount = COUNT(*) FROM BusinessSchemas WHERE IsActive = 1;
        PRINT '';
        PRINT 'Active schemas count: ' + CAST(@ActiveCount AS VARCHAR(10));
        
        -- Check for default schemas
        DECLARE @DefaultCount INT;
        SELECT @DefaultCount = COUNT(*) FROM BusinessSchemas WHERE IsDefault = 1;
        PRINT 'Default schemas count: ' + CAST(@DefaultCount AS VARCHAR(10));
        
        -- Check versions for each schema
        PRINT '';
        PRINT 'Schema versions:';
        SELECT 
            bs.Name as SchemaName,
            bs.IsActive as SchemaActive,
            bs.IsDefault as SchemaDefault,
            COUNT(sv.Id) as VersionCount,
            COUNT(CASE WHEN sv.IsCurrent = 1 THEN 1 END) as CurrentVersions,
            COUNT(CASE WHEN sv.IsActive = 1 THEN 1 END) as ActiveVersions
        FROM BusinessSchemas bs
        LEFT JOIN SchemaVersions sv ON bs.Id = sv.SchemaId
        GROUP BY bs.Id, bs.Name, bs.IsActive, bs.IsDefault
        ORDER BY bs.CreatedAt DESC;
        
    END
    ELSE
    BEGIN
        PRINT '⚠️ BusinessSchemas table is EMPTY - this explains the auto-generated fallback!';
    END
END
ELSE
BEGIN
    PRINT '❌ BusinessSchemas table does NOT exist!';
END

-- Check related tables
PRINT '';
PRINT '=== RELATED TABLES ===';

-- SchemaVersions
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'SchemaVersions')
BEGIN
    DECLARE @VersionCount INT;
    SELECT @VersionCount = COUNT(*) FROM SchemaVersions;
    PRINT 'SchemaVersions records: ' + CAST(@VersionCount AS VARCHAR(10));
END

-- SchemaTableContexts  
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'SchemaTableContexts')
BEGIN
    DECLARE @TableContextCount INT;
    SELECT @TableContextCount = COUNT(*) FROM SchemaTableContexts;
    PRINT 'SchemaTableContexts records: ' + CAST(@TableContextCount AS VARCHAR(10));
END

-- SchemaColumnContexts
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'SchemaColumnContexts')
BEGIN
    DECLARE @ColumnContextCount INT;
    SELECT @ColumnContextCount = COUNT(*) FROM SchemaColumnContexts;
    PRINT 'SchemaColumnContexts records: ' + CAST(@ColumnContextCount AS VARCHAR(10));
END

PRINT '';
PRINT '=== DIAGNOSIS COMPLETE ===';
PRINT '';
PRINT 'If BusinessSchemas is empty, you need to:';
PRINT '1. Run database migration for missing columns';
PRINT '2. Create/import some schema metadata';
PRINT '3. Or trigger auto-generation to populate the tables';
