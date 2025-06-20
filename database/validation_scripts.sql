-- =====================================================
-- BusinessColumnInfo Validation Scripts
-- =====================================================
-- These scripts validate that all actual table columns are represented in BusinessColumnInfo
-- and identify any invented columns that don't exist in the actual tables

-- =====================================================
-- Script 1: Comprehensive Validation for All Tables
-- =====================================================

-- Create a temporary table to store validation results
IF OBJECT_ID('tempdb..#ValidationResults') IS NOT NULL DROP TABLE #ValidationResults
CREATE TABLE #ValidationResults (
    TableName NVARCHAR(100),
    Status NVARCHAR(50),
    ColumnName NVARCHAR(100),
    DataType NVARCHAR(50),
    IsNullable NVARCHAR(10),
    ValidationMessage NVARCHAR(500)
)

-- =====================================================
-- Validate tbl_Currencies
-- =====================================================
DECLARE @TableName NVARCHAR(100) = 'tbl_Currencies'
DECLARE @TableId INT = (SELECT Id FROM BusinessTableInfo WHERE TableName = @TableName)

-- Check for missing columns
INSERT INTO #ValidationResults
SELECT 
    @TableName as TableName,
    'MISSING' as Status,
    COLUMN_NAME as ColumnName,
    DATA_TYPE as DataType,
    IS_NULLABLE as IsNullable,
    'Column exists in actual table but missing from BusinessColumnInfo' as ValidationMessage
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'tbl_Currencies' 
AND TABLE_SCHEMA = 'common'
AND COLUMN_NAME NOT IN (
    SELECT ColumnName 
    FROM BusinessColumnInfo 
    WHERE TableInfoId = @TableId
)

-- Check for extra/invented columns
INSERT INTO #ValidationResults
SELECT 
    @TableName as TableName,
    'EXTRA' as Status,
    bci.ColumnName,
    'N/A' as DataType,
    'N/A' as IsNullable,
    'Column exists in BusinessColumnInfo but not in actual table (INVENTED)' as ValidationMessage
FROM BusinessColumnInfo bci
WHERE bci.TableInfoId = @TableId
AND bci.ColumnName NOT IN (
    SELECT COLUMN_NAME 
    FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'tbl_Currencies' 
    AND TABLE_SCHEMA = 'common'
)

-- =====================================================
-- Validate tbl_Countries
-- =====================================================
SET @TableName = 'tbl_Countries'
SET @TableId = (SELECT Id FROM BusinessTableInfo WHERE TableName = @TableName)

-- Check for missing columns
INSERT INTO #ValidationResults
SELECT 
    @TableName as TableName,
    'MISSING' as Status,
    COLUMN_NAME as ColumnName,
    DATA_TYPE as DataType,
    IS_NULLABLE as IsNullable,
    'Column exists in actual table but missing from BusinessColumnInfo' as ValidationMessage
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'tbl_Countries' 
AND TABLE_SCHEMA = 'common'
AND COLUMN_NAME NOT IN (
    SELECT ColumnName 
    FROM BusinessColumnInfo 
    WHERE TableInfoId = @TableId
)

-- Check for extra/invented columns
INSERT INTO #ValidationResults
SELECT 
    @TableName as TableName,
    'EXTRA' as Status,
    bci.ColumnName,
    'N/A' as DataType,
    'N/A' as IsNullable,
    'Column exists in BusinessColumnInfo but not in actual table (INVENTED)' as ValidationMessage
FROM BusinessColumnInfo bci
WHERE bci.TableInfoId = @TableId
AND bci.ColumnName NOT IN (
    SELECT COLUMN_NAME 
    FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'tbl_Countries' 
    AND TABLE_SCHEMA = 'common'
)

-- =====================================================
-- Validate tbl_White_labels
-- =====================================================
SET @TableName = 'tbl_White_labels'
SET @TableId = (SELECT Id FROM BusinessTableInfo WHERE TableName = @TableName)

-- Check for missing columns
INSERT INTO #ValidationResults
SELECT 
    @TableName as TableName,
    'MISSING' as Status,
    COLUMN_NAME as ColumnName,
    DATA_TYPE as DataType,
    IS_NULLABLE as IsNullable,
    'Column exists in actual table but missing from BusinessColumnInfo' as ValidationMessage
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'tbl_White_labels' 
AND TABLE_SCHEMA = 'common'
AND COLUMN_NAME NOT IN (
    SELECT ColumnName 
    FROM BusinessColumnInfo 
    WHERE TableInfoId = @TableId
)

-- Check for extra/invented columns
INSERT INTO #ValidationResults
SELECT 
    @TableName as TableName,
    'EXTRA' as Status,
    bci.ColumnName,
    'N/A' as DataType,
    'N/A' as IsNullable,
    'Column exists in BusinessColumnInfo but not in actual table (INVENTED)' as ValidationMessage
FROM BusinessColumnInfo bci
WHERE bci.TableInfoId = @TableId
AND bci.ColumnName NOT IN (
    SELECT COLUMN_NAME 
    FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'tbl_White_labels' 
    AND TABLE_SCHEMA = 'common'
)

-- =====================================================
-- Validate Games table
-- =====================================================
SET @TableName = 'Games'
SET @TableId = (SELECT Id FROM BusinessTableInfo WHERE TableName = @TableName)

-- Check for missing columns
INSERT INTO #ValidationResults
SELECT 
    @TableName as TableName,
    'MISSING' as Status,
    COLUMN_NAME as ColumnName,
    DATA_TYPE as DataType,
    IS_NULLABLE as IsNullable,
    'Column exists in actual table but missing from BusinessColumnInfo' as ValidationMessage
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'Games' 
AND TABLE_SCHEMA = 'dbo'
AND COLUMN_NAME NOT IN (
    SELECT ColumnName 
    FROM BusinessColumnInfo 
    WHERE TableInfoId = @TableId
)

-- Check for extra/invented columns
INSERT INTO #ValidationResults
SELECT
    @TableName as TableName,
    'EXTRA' as Status,
    bci.ColumnName,
    'N/A' as DataType,
    'N/A' as IsNullable,
    'Column exists in BusinessColumnInfo but not in actual table (INVENTED)' as ValidationMessage
FROM BusinessColumnInfo bci
WHERE bci.TableInfoId = @TableId
AND bci.ColumnName NOT IN (
    SELECT COLUMN_NAME
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 'Games'
    AND TABLE_SCHEMA = 'dbo'
)

-- =====================================================
-- Validate tbl_Daily_actions_games
-- =====================================================
SET @TableName = 'tbl_Daily_actions_games'
SET @TableId = (SELECT Id FROM BusinessTableInfo WHERE TableName = @TableName)

-- Check for missing columns
INSERT INTO #ValidationResults
SELECT
    @TableName as TableName,
    'MISSING' as Status,
    COLUMN_NAME as ColumnName,
    DATA_TYPE as DataType,
    IS_NULLABLE as IsNullable,
    'Column exists in actual table but missing from BusinessColumnInfo' as ValidationMessage
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'tbl_Daily_actions_games'
AND TABLE_SCHEMA = 'common'
AND COLUMN_NAME NOT IN (
    SELECT ColumnName
    FROM BusinessColumnInfo
    WHERE TableInfoId = @TableId
)

-- Check for extra/invented columns
INSERT INTO #ValidationResults
SELECT
    @TableName as TableName,
    'EXTRA' as Status,
    bci.ColumnName,
    'N/A' as DataType,
    'N/A' as IsNullable,
    'Column exists in BusinessColumnInfo but not in actual table (INVENTED)' as ValidationMessage
FROM BusinessColumnInfo bci
WHERE bci.TableInfoId = @TableId
AND bci.ColumnName NOT IN (
    SELECT COLUMN_NAME
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 'tbl_Daily_actions_games'
    AND TABLE_SCHEMA = 'common'
)

-- =====================================================
-- Validate tbl_Daily_actions
-- =====================================================
SET @TableName = 'tbl_Daily_actions'
SET @TableId = (SELECT Id FROM BusinessTableInfo WHERE TableName = @TableName)

-- Check for missing columns
INSERT INTO #ValidationResults
SELECT
    @TableName as TableName,
    'MISSING' as Status,
    COLUMN_NAME as ColumnName,
    DATA_TYPE as DataType,
    IS_NULLABLE as IsNullable,
    'Column exists in actual table but missing from BusinessColumnInfo' as ValidationMessage
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'tbl_Daily_actions'
AND TABLE_SCHEMA = 'common'
AND COLUMN_NAME NOT IN (
    SELECT ColumnName
    FROM BusinessColumnInfo
    WHERE TableInfoId = @TableId
)

-- Check for extra/invented columns
INSERT INTO #ValidationResults
SELECT
    @TableName as TableName,
    'EXTRA' as Status,
    bci.ColumnName,
    'N/A' as DataType,
    'N/A' as IsNullable,
    'Column exists in BusinessColumnInfo but not in actual table (INVENTED)' as ValidationMessage
FROM BusinessColumnInfo bci
WHERE bci.TableInfoId = @TableId
AND bci.ColumnName NOT IN (
    SELECT COLUMN_NAME
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 'tbl_Daily_actions'
    AND TABLE_SCHEMA = 'common'
)

-- =====================================================
-- Validate tbl_Daily_actions_players
-- =====================================================
SET @TableName = 'tbl_Daily_actions_players'
SET @TableId = (SELECT Id FROM BusinessTableInfo WHERE TableName = @TableName)

-- Check for missing columns
INSERT INTO #ValidationResults
SELECT
    @TableName as TableName,
    'MISSING' as Status,
    COLUMN_NAME as ColumnName,
    DATA_TYPE as DataType,
    IS_NULLABLE as IsNullable,
    'Column exists in actual table but missing from BusinessColumnInfo' as ValidationMessage
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'tbl_Daily_actions_players'
AND TABLE_SCHEMA = 'common'
AND COLUMN_NAME NOT IN (
    SELECT ColumnName
    FROM BusinessColumnInfo
    WHERE TableInfoId = @TableId
)

-- Check for extra/invented columns
INSERT INTO #ValidationResults
SELECT
    @TableName as TableName,
    'EXTRA' as Status,
    bci.ColumnName,
    'N/A' as DataType,
    'N/A' as IsNullable,
    'Column exists in BusinessColumnInfo but not in actual table (INVENTED)' as ValidationMessage
FROM BusinessColumnInfo bci
WHERE bci.TableInfoId = @TableId
AND bci.ColumnName NOT IN (
    SELECT COLUMN_NAME
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 'tbl_Daily_actions_players'
    AND TABLE_SCHEMA = 'common'
)

-- =====================================================
-- Validate tbl_Daily_actionsGBP_transactions
-- =====================================================
SET @TableName = 'tbl_Daily_actionsGBP_transactions'
SET @TableId = (SELECT Id FROM BusinessTableInfo WHERE TableName = @TableName)

-- Check for missing columns
INSERT INTO #ValidationResults
SELECT
    @TableName as TableName,
    'MISSING' as Status,
    COLUMN_NAME as ColumnName,
    DATA_TYPE as DataType,
    IS_NULLABLE as IsNullable,
    'Column exists in actual table but missing from BusinessColumnInfo' as ValidationMessage
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'tbl_Daily_actionsGBP_transactions'
AND TABLE_SCHEMA = 'common'
AND COLUMN_NAME NOT IN (
    SELECT ColumnName
    FROM BusinessColumnInfo
    WHERE TableInfoId = @TableId
)

-- Check for extra/invented columns
INSERT INTO #ValidationResults
SELECT
    @TableName as TableName,
    'EXTRA' as Status,
    bci.ColumnName,
    'N/A' as DataType,
    'N/A' as IsNullable,
    'Column exists in BusinessColumnInfo but not in actual table (INVENTED)' as ValidationMessage
FROM BusinessColumnInfo bci
WHERE bci.TableInfoId = @TableId
AND bci.ColumnName NOT IN (
    SELECT COLUMN_NAME
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 'tbl_Daily_actionsGBP_transactions'
    AND TABLE_SCHEMA = 'common'
)

-- =====================================================
-- DISPLAY VALIDATION RESULTS
-- =====================================================

-- Summary by Table and Status
SELECT
    '=== VALIDATION SUMMARY ===' as Section,
    TableName,
    Status,
    COUNT(*) as IssueCount
FROM #ValidationResults
GROUP BY TableName, Status
ORDER BY TableName, Status

-- Detailed Issues
SELECT
    '=== DETAILED ISSUES ===' as Section,
    TableName,
    Status,
    ColumnName,
    DataType,
    IsNullable,
    ValidationMessage
FROM #ValidationResults
ORDER BY TableName, Status, ColumnName

-- Overall Summary
SELECT
    '=== OVERALL SUMMARY ===' as Section,
    Status,
    COUNT(*) as TotalIssues
FROM #ValidationResults
GROUP BY Status
ORDER BY Status

-- Tables with Issues
SELECT
    '=== TABLES WITH ISSUES ===' as Section,
    TableName,
    COUNT(*) as TotalIssues,
    SUM(CASE WHEN Status = 'MISSING' THEN 1 ELSE 0 END) as MissingColumns,
    SUM(CASE WHEN Status = 'EXTRA' THEN 1 ELSE 0 END) as ExtraColumns
FROM #ValidationResults
GROUP BY TableName
ORDER BY TotalIssues DESC, TableName

-- Clean up
DROP TABLE #ValidationResults

-- =====================================================
-- INDIVIDUAL TABLE VALIDATION SCRIPTS
-- =====================================================
-- Use these scripts to validate individual tables

/*
-- Quick validation for a specific table
DECLARE @CheckTableName NVARCHAR(100) = 'tbl_Daily_actions'  -- Change this
DECLARE @CheckTableId INT = (SELECT Id FROM BusinessTableInfo WHERE TableName = @CheckTableName)
DECLARE @CheckSchema NVARCHAR(100) = 'common'  -- Change this if needed

SELECT
    'MISSING' as Status,
    COLUMN_NAME as ColumnName,
    DATA_TYPE as DataType
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = @CheckTableName
AND TABLE_SCHEMA = @CheckSchema
AND COLUMN_NAME NOT IN (
    SELECT ColumnName
    FROM BusinessColumnInfo
    WHERE TableInfoId = @CheckTableId
)
UNION ALL
SELECT
    'EXTRA' as Status,
    bci.ColumnName,
    'N/A' as DataType
FROM BusinessColumnInfo bci
WHERE bci.TableInfoId = @CheckTableId
AND bci.ColumnName NOT IN (
    SELECT COLUMN_NAME
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = @CheckTableName
    AND TABLE_SCHEMA = @CheckSchema
)
*/
