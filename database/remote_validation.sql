-- =====================================================
-- Remote Database Validation Script
-- =====================================================
-- This script validates BusinessColumnInfo against actual tables on remote server
-- Remote Server: 185.64.56.157
-- Remote Database: DailyActionsDB  
-- Local Database: BIReportingCopilot_Dev (for BusinessColumnInfo)

-- =====================================================
-- Setup Linked Server (if not exists)
-- =====================================================
IF NOT EXISTS (SELECT * FROM sys.servers WHERE name = 'RemoteGameDB')
BEGIN
    EXEC sp_addlinkedserver 
        @server = 'RemoteGameDB',
        @srvproduct = '',
        @provider = 'SQLNCLI',
        @datasrc = '185.64.56.157',
        @catalog = 'DailyActionsDB'
    
    EXEC sp_addlinkedsrvlogin 
        @rmtsrvname = 'RemoteGameDB',
        @useself = 'false',
        @locallogin = NULL,
        @rmtuser = 'ReportsUser',
        @rmtpassword = 'YourPasswordHere' -- Replace with actual password
END

-- =====================================================
-- Validation Function for Each Table
-- =====================================================

-- Create temp table for results
IF OBJECT_ID('tempdb..#ValidationResults') IS NOT NULL DROP TABLE #ValidationResults
CREATE TABLE #ValidationResults (
    TableName NVARCHAR(100),
    Status NVARCHAR(20),
    ColumnName NVARCHAR(100),
    DataType NVARCHAR(50),
    ValidationMessage NVARCHAR(500)
)

-- =====================================================
-- Validate tbl_Daily_actions
-- =====================================================
DECLARE @TableName NVARCHAR(100) = 'tbl_Daily_actions'
DECLARE @TableId INT

-- Get table ID from local BusinessTableInfo
SELECT @TableId = Id FROM BIReportingCopilot_Dev.dbo.BusinessTableInfo WHERE TableName = @TableName

PRINT '=== VALIDATING ' + @TableName + ' ==='

-- Check missing columns (exist in remote but not in BusinessColumnInfo)
INSERT INTO #ValidationResults
SELECT 
    @TableName,
    'MISSING',
    COLUMN_NAME,
    DATA_TYPE,
    'Column exists in remote table but missing from BusinessColumnInfo'
FROM OPENQUERY(RemoteGameDB, 
    'SELECT COLUMN_NAME, DATA_TYPE 
     FROM INFORMATION_SCHEMA.COLUMNS 
     WHERE TABLE_NAME = ''tbl_Daily_actions'' 
     AND TABLE_SCHEMA = ''common''') AS remote_cols
WHERE COLUMN_NAME NOT IN (
    SELECT ColumnName 
    FROM BIReportingCopilot_Dev.dbo.BusinessColumnInfo 
    WHERE TableInfoId = @TableId
)

-- Check extra columns (exist in BusinessColumnInfo but not in remote)
INSERT INTO #ValidationResults
SELECT 
    @TableName,
    'EXTRA',
    bci.ColumnName,
    'N/A',
    'Column exists in BusinessColumnInfo but not in remote table (INVENTED)'
FROM BIReportingCopilot_Dev.dbo.BusinessColumnInfo bci
WHERE bci.TableInfoId = @TableId
AND bci.ColumnName NOT IN (
    SELECT COLUMN_NAME 
    FROM OPENQUERY(RemoteGameDB, 
        'SELECT COLUMN_NAME 
         FROM INFORMATION_SCHEMA.COLUMNS 
         WHERE TABLE_NAME = ''tbl_Daily_actions'' 
         AND TABLE_SCHEMA = ''common''')
)

-- =====================================================
-- Validate tbl_Daily_actions_players
-- =====================================================
SET @TableName = 'tbl_Daily_actions_players'
SELECT @TableId = Id FROM BIReportingCopilot_Dev.dbo.BusinessTableInfo WHERE TableName = @TableName

PRINT '=== VALIDATING ' + @TableName + ' ==='

-- Check missing columns
INSERT INTO #ValidationResults
SELECT 
    @TableName,
    'MISSING',
    COLUMN_NAME,
    DATA_TYPE,
    'Column exists in remote table but missing from BusinessColumnInfo'
FROM OPENQUERY(RemoteGameDB, 
    'SELECT COLUMN_NAME, DATA_TYPE 
     FROM INFORMATION_SCHEMA.COLUMNS 
     WHERE TABLE_NAME = ''tbl_Daily_actions_players'' 
     AND TABLE_SCHEMA = ''common''') AS remote_cols
WHERE COLUMN_NAME NOT IN (
    SELECT ColumnName 
    FROM BIReportingCopilot_Dev.dbo.BusinessColumnInfo 
    WHERE TableInfoId = @TableId
)

-- Check extra columns
INSERT INTO #ValidationResults
SELECT 
    @TableName,
    'EXTRA',
    bci.ColumnName,
    'N/A',
    'Column exists in BusinessColumnInfo but not in remote table (INVENTED)'
FROM BIReportingCopilot_Dev.dbo.BusinessColumnInfo bci
WHERE bci.TableInfoId = @TableId
AND bci.ColumnName NOT IN (
    SELECT COLUMN_NAME 
    FROM OPENQUERY(RemoteGameDB, 
        'SELECT COLUMN_NAME 
         FROM INFORMATION_SCHEMA.COLUMNS 
         WHERE TABLE_NAME = ''tbl_Daily_actions_players'' 
         AND TABLE_SCHEMA = ''common''')
)

-- =====================================================
-- Validate tbl_Daily_actionsGBP_transactions
-- =====================================================
SET @TableName = 'tbl_Daily_actionsGBP_transactions'
SELECT @TableId = Id FROM BIReportingCopilot_Dev.dbo.BusinessTableInfo WHERE TableName = @TableName

PRINT '=== VALIDATING ' + @TableName + ' ==='

-- Check missing columns
INSERT INTO #ValidationResults
SELECT 
    @TableName,
    'MISSING',
    COLUMN_NAME,
    DATA_TYPE,
    'Column exists in remote table but missing from BusinessColumnInfo'
FROM OPENQUERY(RemoteGameDB, 
    'SELECT COLUMN_NAME, DATA_TYPE 
     FROM INFORMATION_SCHEMA.COLUMNS 
     WHERE TABLE_NAME = ''tbl_Daily_actionsGBP_transactions'' 
     AND TABLE_SCHEMA = ''common''') AS remote_cols
WHERE COLUMN_NAME NOT IN (
    SELECT ColumnName 
    FROM BIReportingCopilot_Dev.dbo.BusinessColumnInfo 
    WHERE TableInfoId = @TableId
)

-- Check extra columns
INSERT INTO #ValidationResults
SELECT 
    @TableName,
    'EXTRA',
    bci.ColumnName,
    'N/A',
    'Column exists in BusinessColumnInfo but not in remote table (INVENTED)'
FROM BIReportingCopilot_Dev.dbo.BusinessColumnInfo bci
WHERE bci.TableInfoId = @TableId
AND bci.ColumnName NOT IN (
    SELECT COLUMN_NAME 
    FROM OPENQUERY(RemoteGameDB, 
        'SELECT COLUMN_NAME 
         FROM INFORMATION_SCHEMA.COLUMNS 
         WHERE TABLE_NAME = ''tbl_Daily_actionsGBP_transactions'' 
         AND TABLE_SCHEMA = ''common''')
)

-- =====================================================
-- Display Results
-- =====================================================

-- Summary by table
SELECT 
    '=== VALIDATION SUMMARY ===' as Section,
    TableName,
    Status,
    COUNT(*) as Count
FROM #ValidationResults
GROUP BY TableName, Status
ORDER BY TableName, Status

-- Detailed results
SELECT 
    '=== DETAILED ISSUES ===' as Section,
    TableName,
    Status,
    ColumnName,
    DataType,
    ValidationMessage
FROM #ValidationResults
ORDER BY TableName, Status, ColumnName

-- Overall summary
SELECT 
    '=== OVERALL SUMMARY ===' as Section,
    Status,
    COUNT(*) as TotalIssues
FROM #ValidationResults
GROUP BY Status

-- Clean up
DROP TABLE #ValidationResults

-- =====================================================
-- Cleanup Linked Server (optional)
-- =====================================================
-- EXEC sp_dropserver 'RemoteGameDB', 'droplogins'
