-- Simple step-by-step fix for missing columns in QuerySuggestions table
-- Run this script in SQL Server Management Studio against BIReportingCopilot_Dev database

USE [BIReportingCopilot_Dev]
GO

PRINT 'Starting simple database schema fix...'

-- Step 1: Check if QuerySuggestions table exists
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'QuerySuggestions')
BEGIN
    PRINT 'ERROR: QuerySuggestions table does not exist!'
    RETURN
END
PRINT 'QuerySuggestions table found.'

-- Step 2: Add Text column if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.QuerySuggestions') AND name = 'Text')
BEGIN
    PRINT 'Adding Text column...'
    ALTER TABLE [dbo].[QuerySuggestions] ADD [Text] NVARCHAR(500) NULL
    PRINT 'Text column added.'
END
ELSE
BEGIN
    PRINT 'Text column already exists.'
END

-- Step 3: Add Relevance column if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.QuerySuggestions') AND name = 'Relevance')
BEGIN
    PRINT 'Adding Relevance column...'
    ALTER TABLE [dbo].[QuerySuggestions] ADD [Relevance] DECIMAL(3,2) NULL DEFAULT 0.8
    PRINT 'Relevance column added.'
END
ELSE
BEGIN
    PRINT 'Relevance column already exists.'
END

-- Step 4: Update Text column with QueryText values (only if Text column exists and is empty)
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.QuerySuggestions') AND name = 'Text')
BEGIN
    PRINT 'Updating Text column with QueryText values...'
    UPDATE [dbo].[QuerySuggestions] 
    SET [Text] = [QueryText] 
    WHERE [Text] IS NULL OR [Text] = ''
    PRINT 'Text column updated.'
END

-- Step 5: Update Relevance column with default values (only if Relevance column exists and is empty)
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.QuerySuggestions') AND name = 'Relevance')
BEGIN
    PRINT 'Updating Relevance column with default values...'
    UPDATE [dbo].[QuerySuggestions] 
    SET [Relevance] = 0.8 
    WHERE [Relevance] IS NULL
    PRINT 'Relevance column updated.'
END

-- Step 6: Make columns NOT NULL after they have data
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.QuerySuggestions') AND name = 'Text' AND is_nullable = 1)
BEGIN
    PRINT 'Making Text column NOT NULL...'
    ALTER TABLE [dbo].[QuerySuggestions] ALTER COLUMN [Text] NVARCHAR(500) NOT NULL
    PRINT 'Text column is now NOT NULL.'
END

IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.QuerySuggestions') AND name = 'Relevance' AND is_nullable = 1)
BEGIN
    PRINT 'Making Relevance column NOT NULL...'
    ALTER TABLE [dbo].[QuerySuggestions] ALTER COLUMN [Relevance] DECIMAL(3,2) NOT NULL
    PRINT 'Relevance column is now NOT NULL.'
END

-- Step 7: Verify the changes
PRINT 'Verifying schema changes...'

SELECT 
    'QuerySuggestions' AS TableName,
    c.name AS ColumnName,
    t.name AS DataType,
    c.max_length,
    c.precision,
    c.scale,
    CASE WHEN c.is_nullable = 1 THEN 'YES' ELSE 'NO' END AS IsNullable
FROM sys.columns c
JOIN sys.types t ON c.system_type_id = t.system_type_id
WHERE c.object_id = OBJECT_ID('dbo.QuerySuggestions')
AND c.name IN ('Text', 'Relevance', 'QueryText')
ORDER BY c.name

PRINT 'Simple database schema fix completed successfully!'
PRINT 'You can now restart your application.'

GO
