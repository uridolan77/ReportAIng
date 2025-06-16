-- Comprehensive fix for QuerySuggestions table missing columns
-- This script adds all missing columns that Entity Framework expects

USE [BIReportingCopilot_Dev]
GO

PRINT 'Starting comprehensive QuerySuggestions table column fix...'

-- Check if QuerySuggestions table exists
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'QuerySuggestions')
BEGIN
    PRINT 'ERROR: QuerySuggestions table does not exist!'
    PRINT 'Please create the table first.'
    RETURN
END

PRINT 'QuerySuggestions table found. Checking for missing columns...'

-- Add Confidence column if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.QuerySuggestions') AND name = 'Confidence')
BEGIN
    PRINT 'Adding Confidence column...'
    ALTER TABLE [dbo].[QuerySuggestions] ADD [Confidence] DECIMAL(3,2) NOT NULL DEFAULT 0.8
    PRINT 'Confidence column added successfully.'
END
ELSE
BEGIN
    PRINT 'Confidence column already exists.'
END

-- Add CreatedAt column if it doesn't exist  
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.QuerySuggestions') AND name = 'CreatedAt')
BEGIN
    PRINT 'Adding CreatedAt column...'
    ALTER TABLE [dbo].[QuerySuggestions] ADD [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE()
    PRINT 'CreatedAt column added successfully.'
END
ELSE
BEGIN
    PRINT 'CreatedAt column already exists.'
END

-- Add ErrorMessage column if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.QuerySuggestions') AND name = 'ErrorMessage')
BEGIN
    PRINT 'Adding ErrorMessage column...'
    ALTER TABLE [dbo].[QuerySuggestions] ADD [ErrorMessage] NVARCHAR(MAX) NULL
    PRINT 'ErrorMessage column added successfully.'
END
ELSE
BEGIN
    PRINT 'ErrorMessage column already exists.'
END

-- Add Keywords column if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.QuerySuggestions') AND name = 'Keywords')
BEGIN
    PRINT 'Adding Keywords column...'
    ALTER TABLE [dbo].[QuerySuggestions] ADD [Keywords] NVARCHAR(500) NULL
    PRINT 'Keywords column added successfully.'
END
ELSE
BEGIN
    PRINT 'Keywords column already exists.'
END

-- Add Query column if it doesn't exist (alias for QueryText)
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.QuerySuggestions') AND name = 'Query')
BEGIN
    PRINT 'Adding Query column...'
    ALTER TABLE [dbo].[QuerySuggestions] ADD [Query] NVARCHAR(500) NULL
    PRINT 'Query column added successfully.'
END
ELSE
BEGIN
    PRINT 'Query column already exists.'
END

-- Add RequiredTables column if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.QuerySuggestions') AND name = 'RequiredTables')
BEGIN
    PRINT 'Adding RequiredTables column...'
    ALTER TABLE [dbo].[QuerySuggestions] ADD [RequiredTables] NVARCHAR(500) NULL
    PRINT 'RequiredTables column added successfully.'
END
ELSE
BEGIN
    PRINT 'RequiredTables column already exists.'
END

-- Add Source column if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.QuerySuggestions') AND name = 'Source')
BEGIN
    PRINT 'Adding Source column...'
    ALTER TABLE [dbo].[QuerySuggestions] ADD [Source] NVARCHAR(100) NULL DEFAULT 'System'
    PRINT 'Source column added successfully.'
END
ELSE
BEGIN
    PRINT 'Source column already exists.'
END

-- Add Text column if it doesn't exist (alias for QueryText)
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.QuerySuggestions') AND name = 'Text')
BEGIN
    PRINT 'Adding Text column...'
    ALTER TABLE [dbo].[QuerySuggestions] ADD [Text] NVARCHAR(500) NULL
    
    -- Populate Text column with QueryText values
    UPDATE [dbo].[QuerySuggestions] SET [Text] = [QueryText] WHERE [Text] IS NULL
    
    PRINT 'Text column added and populated successfully.'
END
ELSE
BEGIN
    PRINT 'Text column already exists.'
END

-- Add Relevance column if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.QuerySuggestions') AND name = 'Relevance')
BEGIN
    PRINT 'Adding Relevance column...'
    ALTER TABLE [dbo].[QuerySuggestions] ADD [Relevance] DECIMAL(3,2) NOT NULL DEFAULT 0.8
    PRINT 'Relevance column added successfully.'
END
ELSE
BEGIN
    PRINT 'Relevance column already exists.'
END

-- Verify all columns exist
PRINT 'Verifying all required columns exist...'

DECLARE @MissingColumns TABLE (ColumnName NVARCHAR(128))

-- Check for missing columns
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.QuerySuggestions') AND name = 'Confidence')
    INSERT INTO @MissingColumns VALUES ('Confidence')

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.QuerySuggestions') AND name = 'CreatedAt')
    INSERT INTO @MissingColumns VALUES ('CreatedAt')

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.QuerySuggestions') AND name = 'ErrorMessage')
    INSERT INTO @MissingColumns VALUES ('ErrorMessage')

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.QuerySuggestions') AND name = 'Keywords')
    INSERT INTO @MissingColumns VALUES ('Keywords')

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.QuerySuggestions') AND name = 'Query')
    INSERT INTO @MissingColumns VALUES ('Query')

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.QuerySuggestions') AND name = 'RequiredTables')
    INSERT INTO @MissingColumns VALUES ('RequiredTables')

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.QuerySuggestions') AND name = 'Source')
    INSERT INTO @MissingColumns VALUES ('Source')

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.QuerySuggestions') AND name = 'Text')
    INSERT INTO @MissingColumns VALUES ('Text')

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.QuerySuggestions') AND name = 'Relevance')
    INSERT INTO @MissingColumns VALUES ('Relevance')

-- Report missing columns
IF EXISTS (SELECT * FROM @MissingColumns)
BEGIN
    PRINT 'WARNING: The following columns are still missing:'
    SELECT ColumnName FROM @MissingColumns
END
ELSE
BEGIN
    PRINT 'SUCCESS: All required columns have been added!'
END

-- Show final column structure
PRINT 'Current QuerySuggestions table structure:'
SELECT 
    c.name AS ColumnName,
    t.name AS DataType,
    CASE 
        WHEN t.name IN ('nvarchar', 'varchar', 'char', 'nchar') THEN 
            CASE WHEN c.max_length = -1 THEN 'MAX' ELSE CAST(c.max_length AS VARCHAR(10)) END
        WHEN t.name IN ('decimal', 'numeric') THEN 
            CAST(c.precision AS VARCHAR(10)) + ',' + CAST(c.scale AS VARCHAR(10))
        ELSE ''
    END AS Length_Precision,
    c.is_nullable AS IsNullable
FROM sys.columns c
JOIN sys.types t ON c.system_type_id = t.system_type_id
WHERE c.object_id = OBJECT_ID('dbo.QuerySuggestions')
ORDER BY c.column_id

PRINT 'QuerySuggestions column fix completed successfully!'

GO
