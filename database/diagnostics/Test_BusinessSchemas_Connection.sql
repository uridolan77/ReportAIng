-- Quick diagnostic script to confirm the database state and connection
-- Run this to verify that your application is connecting to the right database

USE [BIReportingCopilot_Dev]
GO

-- Check if we're connected to the right database
SELECT 
    DB_NAME() as CurrentDatabase,
    GETDATE() as CurrentTime;

-- Verify BusinessSchemas table structure
SELECT 
    c.COLUMN_NAME,
    c.DATA_TYPE,
    c.IS_NULLABLE,
    c.COLUMN_DEFAULT
FROM INFORMATION_SCHEMA.COLUMNS c
WHERE c.TABLE_NAME = 'BusinessSchemas'
ORDER BY c.ORDINAL_POSITION;

-- Check if any data exists
SELECT COUNT(*) as RecordCount FROM BusinessSchemas;

-- Try a test query that should work if the columns exist
SELECT TOP 1 
    Id,
    Name,
    IsActive,
    IsDefault,
    CreatedAt
FROM BusinessSchemas;
