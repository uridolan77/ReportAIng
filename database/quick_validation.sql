-- =====================================================
-- Quick Validation Script for BusinessColumnInfo
-- =====================================================
-- This script quickly checks each table for missing or extra columns

-- =====================================================
-- Quick Check: All Tables Coverage Summary
-- =====================================================

WITH ActualColumns AS (
    -- Get all actual columns from INFORMATION_SCHEMA
    SELECT 
        CASE 
            WHEN TABLE_SCHEMA = 'common' THEN TABLE_NAME
            WHEN TABLE_SCHEMA = 'dbo' THEN TABLE_NAME
            ELSE TABLE_SCHEMA + '.' + TABLE_NAME
        END as TableName,
        COLUMN_NAME as ColumnName,
        TABLE_SCHEMA as SchemaName
    FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME IN (
        'tbl_Currencies', 'tbl_Countries', 'tbl_White_labels', 
        'Games', 'tbl_Daily_actions_games', 'tbl_Daily_actions', 
        'tbl_Daily_actions_players', 'tbl_Daily_actionsGBP_transactions'
    )
),
BusinessColumns AS (
    -- Get all columns from BusinessColumnInfo
    SELECT 
        bti.TableName,
        bci.ColumnName
    FROM BusinessColumnInfo bci
    JOIN BusinessTableInfo bti ON bci.TableInfoId = bti.Id
),
TableStats AS (
    -- Calculate statistics for each table
    SELECT 
        COALESCE(ac.TableName, bc.TableName) as TableName,
        COUNT(DISTINCT ac.ColumnName) as ActualColumnCount,
        COUNT(DISTINCT bc.ColumnName) as BusinessColumnCount,
        COUNT(DISTINCT ac.ColumnName) - COUNT(DISTINCT bc.ColumnName) as MissingCount
    FROM ActualColumns ac
    FULL OUTER JOIN BusinessColumns bc ON ac.TableName = bc.TableName AND ac.ColumnName = bc.ColumnName
    GROUP BY COALESCE(ac.TableName, bc.TableName)
)
SELECT 
    '=== TABLE COVERAGE SUMMARY ===' as Section,
    TableName,
    ActualColumnCount,
    BusinessColumnCount,
    MissingCount,
    CASE 
        WHEN MissingCount = 0 THEN '✅ COMPLETE'
        WHEN MissingCount > 0 THEN '⚠️ MISSING ' + CAST(MissingCount as VARCHAR(10))
        ELSE '❌ EXTRA COLUMNS'
    END as Status,
    CASE 
        WHEN ActualColumnCount > 0 THEN 
            CAST(ROUND((BusinessColumnCount * 100.0 / ActualColumnCount), 1) as VARCHAR(10)) + '%'
        ELSE 'N/A'
    END as CoveragePercentage
FROM TableStats
ORDER BY TableName

-- =====================================================
-- Quick Check: Missing Columns by Table
-- =====================================================

SELECT 
    '=== MISSING COLUMNS ===' as Section,
    ac.TableName,
    ac.ColumnName as MissingColumn,
    'Column exists in actual table but missing from BusinessColumnInfo' as Issue
FROM (
    SELECT 
        CASE 
            WHEN TABLE_SCHEMA = 'common' THEN TABLE_NAME
            WHEN TABLE_SCHEMA = 'dbo' THEN TABLE_NAME
            ELSE TABLE_SCHEMA + '.' + TABLE_NAME
        END as TableName,
        COLUMN_NAME as ColumnName
    FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME IN (
        'tbl_Currencies', 'tbl_Countries', 'tbl_White_labels', 
        'Games', 'tbl_Daily_actions_games', 'tbl_Daily_actions', 
        'tbl_Daily_actions_players', 'tbl_Daily_actionsGBP_transactions'
    )
) ac
LEFT JOIN (
    SELECT 
        bti.TableName,
        bci.ColumnName
    FROM BusinessColumnInfo bci
    JOIN BusinessTableInfo bti ON bci.TableInfoId = bti.Id
) bc ON ac.TableName = bc.TableName AND ac.ColumnName = bc.ColumnName
WHERE bc.ColumnName IS NULL
ORDER BY ac.TableName, ac.ColumnName

-- =====================================================
-- Quick Check: Extra/Invented Columns
-- =====================================================

SELECT 
    '=== EXTRA/INVENTED COLUMNS ===' as Section,
    bc.TableName,
    bc.ColumnName as ExtraColumn,
    'Column exists in BusinessColumnInfo but not in actual table (INVENTED)' as Issue
FROM (
    SELECT 
        bti.TableName,
        bci.ColumnName
    FROM BusinessColumnInfo bci
    JOIN BusinessTableInfo bti ON bci.TableInfoId = bti.Id
) bc
LEFT JOIN (
    SELECT 
        CASE 
            WHEN TABLE_SCHEMA = 'common' THEN TABLE_NAME
            WHEN TABLE_SCHEMA = 'dbo' THEN TABLE_NAME
            ELSE TABLE_SCHEMA + '.' + TABLE_NAME
        END as TableName,
        COLUMN_NAME as ColumnName
    FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME IN (
        'tbl_Currencies', 'tbl_Countries', 'tbl_White_labels', 
        'Games', 'tbl_Daily_actions_games', 'tbl_Daily_actions', 
        'tbl_Daily_actions_players', 'tbl_Daily_actionsGBP_transactions'
    )
) ac ON bc.TableName = ac.TableName AND bc.ColumnName = ac.ColumnName
WHERE ac.ColumnName IS NULL
ORDER BY bc.TableName, bc.ColumnName

-- =====================================================
-- Quick Check: Overall Statistics
-- =====================================================

WITH OverallStats AS (
    SELECT 
        (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS 
         WHERE TABLE_NAME IN (
            'tbl_Currencies', 'tbl_Countries', 'tbl_White_labels', 
            'Games', 'tbl_Daily_actions_games', 'tbl_Daily_actions', 
            'tbl_Daily_actions_players', 'tbl_Daily_actionsGBP_transactions'
         )) as TotalActualColumns,
        (SELECT COUNT(*) FROM BusinessColumnInfo bci
         JOIN BusinessTableInfo bti ON bci.TableInfoId = bti.Id) as TotalBusinessColumns
)
SELECT 
    '=== OVERALL STATISTICS ===' as Section,
    TotalActualColumns,
    TotalBusinessColumns,
    TotalActualColumns - TotalBusinessColumns as MissingColumns,
    CASE 
        WHEN TotalActualColumns > 0 THEN 
            CAST(ROUND((TotalBusinessColumns * 100.0 / TotalActualColumns), 1) as VARCHAR(10)) + '%'
        ELSE 'N/A'
    END as OverallCoverage
FROM OverallStats
