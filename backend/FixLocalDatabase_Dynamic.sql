-- Dynamic SQL fix for missing columns in QuerySuggestions table
-- This version uses dynamic SQL to avoid column reference issues
-- Run this script in SQL Server Management Studio against BIReportingCopilot_Dev database

USE [BIReportingCopilot_Dev]
GO

PRINT 'Starting dynamic database schema fix...'

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
    
    -- Update Text column with QueryText values using dynamic SQL
    PRINT 'Updating Text column with QueryText values...'
    EXEC('UPDATE [dbo].[QuerySuggestions] SET [Text] = [QueryText] WHERE [Text] IS NULL OR [Text] = ''''')
    PRINT 'Text column updated.'
    
    -- Make Text column NOT NULL
    PRINT 'Making Text column NOT NULL...'
    ALTER TABLE [dbo].[QuerySuggestions] ALTER COLUMN [Text] NVARCHAR(500) NOT NULL
    PRINT 'Text column is now NOT NULL.'
END
ELSE
BEGIN
    PRINT 'Text column already exists.'
END

-- Step 3: Add Relevance column if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.QuerySuggestions') AND name = 'Relevance')
BEGIN
    PRINT 'Adding Relevance column...'
    ALTER TABLE [dbo].[QuerySuggestions] ADD [Relevance] DECIMAL(3,2) NULL
    PRINT 'Relevance column added.'
    
    -- Update Relevance column with default values using dynamic SQL
    PRINT 'Updating Relevance column with default values...'
    EXEC('UPDATE [dbo].[QuerySuggestions] SET [Relevance] = 0.8 WHERE [Relevance] IS NULL')
    PRINT 'Relevance column updated.'
    
    -- Make Relevance column NOT NULL
    PRINT 'Making Relevance column NOT NULL...'
    ALTER TABLE [dbo].[QuerySuggestions] ALTER COLUMN [Relevance] DECIMAL(3,2) NOT NULL
    PRINT 'Relevance column is now NOT NULL.'
END
ELSE
BEGIN
    PRINT 'Relevance column already exists.'
END

-- Step 4: Verify the changes
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

PRINT 'Dynamic database schema fix completed successfully!'
PRINT 'You can now restart your application.'

GO
