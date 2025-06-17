-- Check the current structure of BusinessSchemas table
USE [BIReportingCopilot_Dev]
GO

-- Get column information for BusinessSchemas table
SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    IS_NULLABLE,
    COLUMN_DEFAULT,
    CHARACTER_MAXIMUM_LENGTH
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'BusinessSchemas'
ORDER BY ORDINAL_POSITION;

-- Check if table exists
SELECT 
    TABLE_NAME,
    TABLE_TYPE
FROM INFORMATION_SCHEMA.TABLES 
WHERE TABLE_NAME = 'BusinessSchemas';

-- Get sample data if any exists
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'BusinessSchemas')
BEGIN
    SELECT TOP 5 * FROM BusinessSchemas;
END
ELSE
BEGIN
    SELECT 'BusinessSchemas table does not exist' as Status;
END
