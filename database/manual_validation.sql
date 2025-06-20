-- =====================================================
-- Manual Validation Scripts
-- =====================================================
-- Run these scripts manually against both databases to validate columns

-- =====================================================
-- STEP 1: Run this against REMOTE database (185.64.56.157 - DailyActionsDB)
-- =====================================================

-- Get all columns from remote gaming tables
SELECT 'tbl_Daily_actions' as TableName, COLUMN_NAME, DATA_TYPE, ORDINAL_POSITION
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'tbl_Daily_actions' AND TABLE_SCHEMA = 'common'
UNION ALL
SELECT 'tbl_Daily_actions_players' as TableName, COLUMN_NAME, DATA_TYPE, ORDINAL_POSITION
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'tbl_Daily_actions_players' AND TABLE_SCHEMA = 'common'
UNION ALL
SELECT 'tbl_Daily_actionsGBP_transactions' as TableName, COLUMN_NAME, DATA_TYPE, ORDINAL_POSITION
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'tbl_Daily_actionsGBP_transactions' AND TABLE_SCHEMA = 'common'
UNION ALL
SELECT 'tbl_Daily_actions_games' as TableName, COLUMN_NAME, DATA_TYPE, ORDINAL_POSITION
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'tbl_Daily_actions_games' AND TABLE_SCHEMA = 'common'
UNION ALL
SELECT 'tbl_Currencies' as TableName, COLUMN_NAME, DATA_TYPE, ORDINAL_POSITION
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'tbl_Currencies' AND TABLE_SCHEMA = 'common'
UNION ALL
SELECT 'tbl_Countries' as TableName, COLUMN_NAME, DATA_TYPE, ORDINAL_POSITION
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'tbl_Countries' AND TABLE_SCHEMA = 'common'
UNION ALL
SELECT 'tbl_White_labels' as TableName, COLUMN_NAME, DATA_TYPE, ORDINAL_POSITION
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'tbl_White_labels' AND TABLE_SCHEMA = 'common'
UNION ALL
SELECT 'Games' as TableName, COLUMN_NAME, DATA_TYPE, ORDINAL_POSITION
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'Games' AND TABLE_SCHEMA = 'dbo'
ORDER BY TableName, ORDINAL_POSITION

-- Get column counts by table
SELECT 
    'REMOTE COLUMN COUNTS' as Section,
    TABLE_NAME as TableName,
    COUNT(*) as ColumnCount
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME IN (
    'tbl_Daily_actions', 'tbl_Daily_actions_players', 'tbl_Daily_actionsGBP_transactions',
    'tbl_Daily_actions_games', 'tbl_Currencies', 'tbl_Countries', 'tbl_White_labels'
) AND TABLE_SCHEMA = 'common'
GROUP BY TABLE_NAME
UNION ALL
SELECT 
    'REMOTE COLUMN COUNTS' as Section,
    TABLE_NAME as TableName,
    COUNT(*) as ColumnCount
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'Games' AND TABLE_SCHEMA = 'dbo'
ORDER BY TableName

-- =====================================================
-- STEP 2: Run this against LOCAL database (BIReportingCopilot_Dev)
-- =====================================================

-- Get all columns from BusinessColumnInfo
SELECT 
    'LOCAL BUSINESS COLUMNS' as Section,
    bti.TableName,
    bci.ColumnName,
    bci.BusinessDataType
FROM BusinessColumnInfo bci
JOIN BusinessTableInfo bti ON bci.TableInfoId = bti.Id
ORDER BY bti.TableName, bci.ColumnName

-- Get column counts from BusinessColumnInfo
SELECT 
    'LOCAL COLUMN COUNTS' as Section,
    bti.TableName,
    COUNT(bci.Id) as ColumnCount
FROM BusinessTableInfo bti
LEFT JOIN BusinessColumnInfo bci ON bti.Id = bci.TableInfoId
GROUP BY bti.TableName, bti.Id
ORDER BY bti.TableName

-- =====================================================
-- STEP 3: Validation Queries for Specific Tables
-- =====================================================

-- Check tbl_Daily_actions specifically
-- Run this against REMOTE database first, then compare with local results

-- REMOTE: Get tbl_Daily_actions columns
SELECT 
    'tbl_Daily_actions - REMOTE' as Source,
    COLUMN_NAME,
    DATA_TYPE,
    IS_NULLABLE,
    ORDINAL_POSITION
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'tbl_Daily_actions' AND TABLE_SCHEMA = 'common'
ORDER BY ORDINAL_POSITION

-- LOCAL: Get tbl_Daily_actions columns from BusinessColumnInfo
-- Run this against LOCAL database
SELECT 
    'tbl_Daily_actions - LOCAL' as Source,
    bci.ColumnName,
    bci.BusinessDataType,
    'N/A' as IS_NULLABLE,
    ROW_NUMBER() OVER (ORDER BY bci.ColumnName) as Position
FROM BusinessColumnInfo bci
JOIN BusinessTableInfo bti ON bci.TableInfoId = bti.Id
WHERE bti.TableName = 'tbl_Daily_actions'
ORDER BY bci.ColumnName

-- =====================================================
-- STEP 4: Generate Missing Columns Report
-- =====================================================

-- For each table, you'll need to manually compare the results from STEP 1 and STEP 2
-- Look for:
-- 1. Columns that exist in REMOTE but not in LOCAL (MISSING)
-- 2. Columns that exist in LOCAL but not in REMOTE (EXTRA/INVENTED)

-- Example comparison template:
/*
REMOTE tbl_Daily_actions columns: [list from STEP 1]
LOCAL tbl_Daily_actions columns:  [list from STEP 2]

MISSING (in remote but not local): [manual comparison]
EXTRA (in local but not remote):   [manual comparison]
*/

-- =====================================================
-- STEP 5: Quick Table Existence Check
-- =====================================================

-- Run this against REMOTE database to confirm tables exist
SELECT 
    'TABLE EXISTS CHECK' as Section,
    TABLE_SCHEMA,
    TABLE_NAME,
    COUNT(*) as ColumnCount
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME IN (
    'tbl_Daily_actions', 
    'tbl_Daily_actions_players', 
    'tbl_Daily_actionsGBP_transactions',
    'tbl_Daily_actions_games',
    'tbl_Currencies',
    'tbl_Countries', 
    'tbl_White_labels',
    'Games'
)
GROUP BY TABLE_SCHEMA, TABLE_NAME
ORDER BY TABLE_SCHEMA, TABLE_NAME

-- =====================================================
-- STEP 6: Sample Data Check (Optional)
-- =====================================================

-- Run against REMOTE database to verify tables have data
SELECT 'tbl_Daily_actions' as TableName, COUNT(*) as RowCount FROM tbl_Daily_actions
UNION ALL
SELECT 'tbl_Daily_actions_players' as TableName, COUNT(*) as RowCount FROM tbl_Daily_actions_players  
UNION ALL
SELECT 'tbl_Daily_actionsGBP_transactions' as TableName, COUNT(*) as RowCount FROM tbl_Daily_actionsGBP_transactions
UNION ALL
SELECT 'tbl_Daily_actions_games' as TableName, COUNT(*) as RowCount FROM tbl_Daily_actions_games
UNION ALL
SELECT 'tbl_Currencies' as TableName, COUNT(*) as RowCount FROM tbl_Currencies
UNION ALL
SELECT 'tbl_Countries' as TableName, COUNT(*) as RowCount FROM tbl_Countries
UNION ALL
SELECT 'tbl_White_labels' as TableName, COUNT(*) as RowCount FROM tbl_White_labels
UNION ALL
SELECT 'Games' as TableName, COUNT(*) as RowCount FROM Games
