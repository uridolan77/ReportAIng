-- Diagnostic script to check current database schema
-- Run this first to understand what tables exist before running the semantic layer migration

PRINT '=== CURRENT DATABASE SCHEMA ANALYSIS ===';
PRINT '';

-- Check if database exists and we have access
SELECT 
    DB_NAME() as CurrentDatabase,
    SYSTEM_USER as CurrentUser,
    GETDATE() as CheckTime;

PRINT '';
PRINT '=== EXISTING TABLES ===';

-- List all user tables in the database
SELECT 
    TABLE_SCHEMA,
    TABLE_NAME,
    TABLE_TYPE
FROM INFORMATION_SCHEMA.TABLES 
WHERE TABLE_TYPE = 'BASE TABLE'
ORDER BY TABLE_SCHEMA, TABLE_NAME;

PRINT '';
PRINT '=== BUSINESS TABLES CHECK ===';

-- Check specifically for business-related tables
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'BusinessTableInfo')
    PRINT '✅ BusinessTableInfo table exists'
ELSE
    PRINT '❌ BusinessTableInfo table does NOT exist';

IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'BusinessColumnInfo')
    PRINT '✅ BusinessColumnInfo table exists'
ELSE
    PRINT '❌ BusinessColumnInfo table does NOT exist';

IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'BusinessGlossary')
    PRINT '✅ BusinessGlossary table exists'
ELSE
    PRINT '❌ BusinessGlossary table does NOT exist';

IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'SchemaMetadata')
    PRINT '✅ SchemaMetadata table exists'
ELSE
    PRINT '❌ SchemaMetadata table does NOT exist';

PRINT '';
PRINT '=== POTENTIAL TABLE NAME VARIATIONS ===';

-- Check for potential variations in table names
SELECT 
    TABLE_NAME,
    'Potential match for BusinessTableInfo' as Notes
FROM INFORMATION_SCHEMA.TABLES 
WHERE TABLE_NAME LIKE '%Business%Table%' 
   OR TABLE_NAME LIKE '%Table%Info%'
   OR TABLE_NAME LIKE '%TableInfo%';

SELECT 
    TABLE_NAME,
    'Potential match for BusinessColumnInfo' as Notes
FROM INFORMATION_SCHEMA.TABLES 
WHERE TABLE_NAME LIKE '%Business%Column%' 
   OR TABLE_NAME LIKE '%Column%Info%'
   OR TABLE_NAME LIKE '%ColumnInfo%';

SELECT 
    TABLE_NAME,
    'Potential match for BusinessGlossary' as Notes
FROM INFORMATION_SCHEMA.TABLES 
WHERE TABLE_NAME LIKE '%Business%Glossary%' 
   OR TABLE_NAME LIKE '%Glossary%'
   OR TABLE_NAME LIKE '%Terms%';

SELECT 
    TABLE_NAME,
    'Potential match for SchemaMetadata' as Notes
FROM INFORMATION_SCHEMA.TABLES 
WHERE TABLE_NAME LIKE '%Schema%' 
   OR TABLE_NAME LIKE '%Metadata%';

PRINT '';
PRINT '=== EXISTING COLUMNS IN FOUND TABLES ===';

-- If BusinessTableInfo exists, show its current structure
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'BusinessTableInfo')
BEGIN
    PRINT 'BusinessTableInfo columns:';
    SELECT 
        COLUMN_NAME,
        DATA_TYPE,
        IS_NULLABLE,
        COLUMN_DEFAULT
    FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'BusinessTableInfo'
    ORDER BY ORDINAL_POSITION;
END

-- If BusinessColumnInfo exists, show its current structure
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'BusinessColumnInfo')
BEGIN
    PRINT 'BusinessColumnInfo columns:';
    SELECT 
        COLUMN_NAME,
        DATA_TYPE,
        IS_NULLABLE,
        COLUMN_DEFAULT
    FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'BusinessColumnInfo'
    ORDER BY ORDINAL_POSITION;
END

-- If BusinessGlossary exists, show its current structure
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'BusinessGlossary')
BEGIN
    PRINT 'BusinessGlossary columns:';
    SELECT 
        COLUMN_NAME,
        DATA_TYPE,
        IS_NULLABLE,
        COLUMN_DEFAULT
    FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'BusinessGlossary'
    ORDER BY ORDINAL_POSITION;
END

-- If SchemaMetadata exists, show its current structure
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'SchemaMetadata')
BEGIN
    PRINT 'SchemaMetadata columns:';
    SELECT 
        COLUMN_NAME,
        DATA_TYPE,
        IS_NULLABLE,
        COLUMN_DEFAULT
    FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'SchemaMetadata'
    ORDER BY ORDINAL_POSITION;
END

PRINT '';
PRINT '=== RECOMMENDATIONS ===';
PRINT 'Based on the results above:';
PRINT '1. If tables exist but with different names, update the migration script';
PRINT '2. If tables do not exist, you may need to run initial table creation first';
PRINT '3. Check if you are connected to the correct database';
PRINT '4. Verify you have the necessary permissions';

PRINT '';
PRINT '=== SCHEMA ANALYSIS COMPLETE ===';
