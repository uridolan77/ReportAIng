-- Verification script to check which database you're connected to
-- and verify BusinessSchemas table structure

-- Check current database
SELECT 
    DB_NAME() as CurrentDatabase,
    'You should see: BIReportingCopilot_Dev' as ExpectedDatabase;

-- Verify BusinessSchemas table exists and has the required columns
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'BusinessSchemas')
BEGIN
    PRINT '✓ BusinessSchemas table exists';
    
    -- Check for required columns
    SELECT 
        COLUMN_NAME,
        DATA_TYPE,
        IS_NULLABLE,
        COLUMN_DEFAULT,
        CASE 
            WHEN COLUMN_NAME IN ('IsActive', 'IsDefault') THEN '✓ REQUIRED COLUMN'
            ELSE '  Regular column'
        END as Status
    FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'BusinessSchemas'
    ORDER BY 
        CASE WHEN COLUMN_NAME IN ('IsActive', 'IsDefault') THEN 0 ELSE 1 END,
        ORDINAL_POSITION;
        
    -- Check if both required columns exist
    DECLARE @IsActiveExists BIT = 0;
    DECLARE @IsDefaultExists BIT = 0;
    
    IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'BusinessSchemas' AND COLUMN_NAME = 'IsActive')
        SET @IsActiveExists = 1;
        
    IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'BusinessSchemas' AND COLUMN_NAME = 'IsDefault')
        SET @IsDefaultExists = 1;
    
    SELECT 
        @IsActiveExists as IsActiveColumnExists,
        @IsDefaultExists as IsDefaultColumnExists,
        CASE 
            WHEN @IsActiveExists = 1 AND @IsDefaultExists = 1 THEN '✅ ALL REQUIRED COLUMNS EXIST'
            ELSE '❌ MISSING REQUIRED COLUMNS'
        END as OverallStatus;
END
ELSE
BEGIN
    PRINT '❌ BusinessSchemas table does not exist';
    SELECT '❌ BusinessSchemas table not found' as Error;
END
