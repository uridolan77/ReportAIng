-- Step 1: Add missing columns to QuerySuggestions table
-- Run this script first in SQL Server Management Studio against BIReportingCopilot_Dev database

USE [BIReportingCopilot_Dev]
GO

PRINT 'Step 1: Adding missing columns to QuerySuggestions table...'

-- Check if QuerySuggestions table exists
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'QuerySuggestions')
BEGIN
    PRINT 'ERROR: QuerySuggestions table does not exist!'
    RETURN
END
PRINT 'QuerySuggestions table found.'

-- Add Text column if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.QuerySuggestions') AND name = 'Text')
BEGIN
    PRINT 'Adding Text column...'
    ALTER TABLE [dbo].[QuerySuggestions] ADD [Text] NVARCHAR(500) NULL
    PRINT 'Text column added successfully.'
END
ELSE
BEGIN
    PRINT 'Text column already exists.'
END

-- Add Relevance column if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.QuerySuggestions') AND name = 'Relevance')
BEGIN
    PRINT 'Adding Relevance column...'
    ALTER TABLE [dbo].[QuerySuggestions] ADD [Relevance] DECIMAL(3,2) NULL
    PRINT 'Relevance column added successfully.'
END
ELSE
BEGIN
    PRINT 'Relevance column already exists.'
END

PRINT 'Step 1 completed. Now run Step2_PopulateColumns.sql'

GO
