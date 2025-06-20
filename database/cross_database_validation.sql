-- =====================================================
-- Cross-Database Validation Script
-- =====================================================
-- This script validates BusinessColumnInfo against actual tables in DailyActionsDB
-- BusinessColumnInfo is in BIReportingCopilot_Dev
-- Actual tables are in DailyActionsDB

USE BIReportingCopilot_Dev
GO

-- =====================================================
-- Check tbl_Daily_actions
-- =====================================================
DECLARE @TableName NVARCHAR(100) = 'tbl_Daily_actions'
DECLARE @TableId INT = (SELECT Id FROM BusinessTableInfo WHERE TableName = @TableName)

PRINT '=== VALIDATING ' + @TableName + ' ==='

-- Missing columns
SELECT 
    'MISSING' as Status,
    COLUMN_NAME as ColumnName,
    DATA_TYPE as DataType
FROM DailyActionsDB.INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'tbl_Daily_actions' 
AND TABLE_SCHEMA = 'common'
AND COLUMN_NAME NOT IN (
    SELECT ColumnName 
    FROM BusinessColumnInfo 
    WHERE TableInfoId = @TableId
)
ORDER BY COLUMN_NAME

-- Extra/Invented columns
SELECT 
    'EXTRA/INVENTED' as Status,
    bci.ColumnName,
    'N/A' as DataType
FROM BusinessColumnInfo bci
WHERE bci.TableInfoId = @TableId
AND bci.ColumnName NOT IN (
    SELECT COLUMN_NAME 
    FROM DailyActionsDB.INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'tbl_Daily_actions' 
    AND TABLE_SCHEMA = 'common'
)
ORDER BY bci.ColumnName

-- Summary
SELECT 
    @TableName as TableName,
    (SELECT COUNT(*) FROM DailyActionsDB.INFORMATION_SCHEMA.COLUMNS 
     WHERE TABLE_NAME = 'tbl_Daily_actions' AND TABLE_SCHEMA = 'common') as ActualColumns,
    (SELECT COUNT(*) FROM BusinessColumnInfo WHERE TableInfoId = @TableId) as BusinessColumns,
    (SELECT COUNT(*) FROM DailyActionsDB.INFORMATION_SCHEMA.COLUMNS 
     WHERE TABLE_NAME = 'tbl_Daily_actions' AND TABLE_SCHEMA = 'common') - 
    (SELECT COUNT(*) FROM BusinessColumnInfo WHERE TableInfoId = @TableId) as MissingCount

-- =====================================================
-- Check tbl_Daily_actions_players
-- =====================================================
SET @TableName = 'tbl_Daily_actions_players'
SET @TableId = (SELECT Id FROM BusinessTableInfo WHERE TableName = @TableName)

PRINT '=== VALIDATING ' + @TableName + ' ==='

-- Missing columns
SELECT 
    'MISSING' as Status,
    COLUMN_NAME as ColumnName,
    DATA_TYPE as DataType
FROM DailyActionsDB.INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'tbl_Daily_actions_players' 
AND TABLE_SCHEMA = 'common'
AND COLUMN_NAME NOT IN (
    SELECT ColumnName 
    FROM BusinessColumnInfo 
    WHERE TableInfoId = @TableId
)
ORDER BY COLUMN_NAME

-- Extra/Invented columns
SELECT 
    'EXTRA/INVENTED' as Status,
    bci.ColumnName,
    'N/A' as DataType
FROM BusinessColumnInfo bci
WHERE bci.TableInfoId = @TableId
AND bci.ColumnName NOT IN (
    SELECT COLUMN_NAME 
    FROM DailyActionsDB.INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'tbl_Daily_actions_players' 
    AND TABLE_SCHEMA = 'common'
)
ORDER BY bci.ColumnName

-- Summary
SELECT 
    @TableName as TableName,
    (SELECT COUNT(*) FROM DailyActionsDB.INFORMATION_SCHEMA.COLUMNS 
     WHERE TABLE_NAME = 'tbl_Daily_actions_players' AND TABLE_SCHEMA = 'common') as ActualColumns,
    (SELECT COUNT(*) FROM BusinessColumnInfo WHERE TableInfoId = @TableId) as BusinessColumns,
    (SELECT COUNT(*) FROM DailyActionsDB.INFORMATION_SCHEMA.COLUMNS 
     WHERE TABLE_NAME = 'tbl_Daily_actions_players' AND TABLE_SCHEMA = 'common') - 
    (SELECT COUNT(*) FROM BusinessColumnInfo WHERE TableInfoId = @TableId) as MissingCount

-- =====================================================
-- Check tbl_Daily_actionsGBP_transactions
-- =====================================================
SET @TableName = 'tbl_Daily_actionsGBP_transactions'
SET @TableId = (SELECT Id FROM BusinessTableInfo WHERE TableName = @TableName)

PRINT '=== VALIDATING ' + @TableName + ' ==='

-- Missing columns
SELECT 
    'MISSING' as Status,
    COLUMN_NAME as ColumnName,
    DATA_TYPE as DataType
FROM DailyActionsDB.INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'tbl_Daily_actionsGBP_transactions' 
AND TABLE_SCHEMA = 'common'
AND COLUMN_NAME NOT IN (
    SELECT ColumnName 
    FROM BusinessColumnInfo 
    WHERE TableInfoId = @TableId
)
ORDER BY COLUMN_NAME

-- Extra/Invented columns
SELECT 
    'EXTRA/INVENTED' as Status,
    bci.ColumnName,
    'N/A' as DataType
FROM BusinessColumnInfo bci
WHERE bci.TableInfoId = @TableId
AND bci.ColumnName NOT IN (
    SELECT COLUMN_NAME 
    FROM DailyActionsDB.INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'tbl_Daily_actionsGBP_transactions' 
    AND TABLE_SCHEMA = 'common'
)
ORDER BY bci.ColumnName

-- Summary
SELECT 
    @TableName as TableName,
    (SELECT COUNT(*) FROM DailyActionsDB.INFORMATION_SCHEMA.COLUMNS 
     WHERE TABLE_NAME = 'tbl_Daily_actionsGBP_transactions' AND TABLE_SCHEMA = 'common') as ActualColumns,
    (SELECT COUNT(*) FROM BusinessColumnInfo WHERE TableInfoId = @TableId) as BusinessColumns,
    (SELECT COUNT(*) FROM DailyActionsDB.INFORMATION_SCHEMA.COLUMNS 
     WHERE TABLE_NAME = 'tbl_Daily_actionsGBP_transactions' AND TABLE_SCHEMA = 'common') - 
    (SELECT COUNT(*) FROM BusinessColumnInfo WHERE TableInfoId = @TableId) as MissingCount

-- =====================================================
-- Overall Summary for All Tables
-- =====================================================
PRINT '=== OVERALL SUMMARY ==='

SELECT 
    bti.TableName,
    CASE bti.TableName
        WHEN 'Games' THEN 
            (SELECT COUNT(*) FROM DailyActionsDB.INFORMATION_SCHEMA.COLUMNS 
             WHERE TABLE_NAME = 'Games' AND TABLE_SCHEMA = 'dbo')
        ELSE 
            (SELECT COUNT(*) FROM DailyActionsDB.INFORMATION_SCHEMA.COLUMNS 
             WHERE TABLE_NAME = bti.TableName AND TABLE_SCHEMA = 'common')
    END as ActualColumns,
    COUNT(bci.Id) as BusinessColumns,
    CASE bti.TableName
        WHEN 'Games' THEN 
            (SELECT COUNT(*) FROM DailyActionsDB.INFORMATION_SCHEMA.COLUMNS 
             WHERE TABLE_NAME = 'Games' AND TABLE_SCHEMA = 'dbo') - COUNT(bci.Id)
        ELSE 
            (SELECT COUNT(*) FROM DailyActionsDB.INFORMATION_SCHEMA.COLUMNS 
             WHERE TABLE_NAME = bti.TableName AND TABLE_SCHEMA = 'common') - COUNT(bci.Id)
    END as MissingColumns,
    CASE 
        WHEN CASE bti.TableName
            WHEN 'Games' THEN 
                (SELECT COUNT(*) FROM DailyActionsDB.INFORMATION_SCHEMA.COLUMNS 
                 WHERE TABLE_NAME = 'Games' AND TABLE_SCHEMA = 'dbo')
            ELSE 
                (SELECT COUNT(*) FROM DailyActionsDB.INFORMATION_SCHEMA.COLUMNS 
                 WHERE TABLE_NAME = bti.TableName AND TABLE_SCHEMA = 'common')
        END = COUNT(bci.Id) THEN '✅ COMPLETE'
        WHEN CASE bti.TableName
            WHEN 'Games' THEN 
                (SELECT COUNT(*) FROM DailyActionsDB.INFORMATION_SCHEMA.COLUMNS 
                 WHERE TABLE_NAME = 'Games' AND TABLE_SCHEMA = 'dbo')
            ELSE 
                (SELECT COUNT(*) FROM DailyActionsDB.INFORMATION_SCHEMA.COLUMNS 
                 WHERE TABLE_NAME = bti.TableName AND TABLE_SCHEMA = 'common')
        END > COUNT(bci.Id) THEN '⚠️ MISSING'
        ELSE '❌ EXTRA'
    END as Status
FROM BusinessTableInfo bti
LEFT JOIN BusinessColumnInfo bci ON bti.Id = bci.TableInfoId
GROUP BY bti.TableName, bti.Id
ORDER BY bti.TableName
