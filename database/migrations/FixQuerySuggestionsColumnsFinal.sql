-- Fix QuerySuggestions table missing columns
-- Based on current table structure, add only the missing columns that Entity Framework expects

USE [BIReportingCopilot_Dev]
GO

PRINT 'Starting QuerySuggestions table column fix...'

-- Add Confidence column if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.QuerySuggestions') AND name = 'Confidence')
BEGIN
    PRINT 'Adding Confidence column...'
    ALTER TABLE [dbo].[QuerySuggestions] ADD [Confidence] DECIMAL(3,2) NOT NULL DEFAULT 0.8
    PRINT 'Confidence column added successfully.'
END

-- Add CreatedAt column if it doesn't exist  
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.QuerySuggestions') AND name = 'CreatedAt')
BEGIN
    PRINT 'Adding CreatedAt column...'
    ALTER TABLE [dbo].[QuerySuggestions] ADD [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE()
    PRINT 'CreatedAt column added successfully.'
END

-- Add ErrorMessage column if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.QuerySuggestions') AND name = 'ErrorMessage')
BEGIN
    PRINT 'Adding ErrorMessage column...'
    ALTER TABLE [dbo].[QuerySuggestions] ADD [ErrorMessage] NVARCHAR(MAX) NULL
    PRINT 'ErrorMessage column added successfully.'
END

-- Add Keywords column if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.QuerySuggestions') AND name = 'Keywords')
BEGIN
    PRINT 'Adding Keywords column...'
    ALTER TABLE [dbo].[QuerySuggestions] ADD [Keywords] NVARCHAR(500) NULL
    PRINT 'Keywords column added successfully.'
END

-- Add Query column if it doesn't exist (alias for QueryText)
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.QuerySuggestions') AND name = 'Query')
BEGIN
    PRINT 'Adding Query column...'
    ALTER TABLE [dbo].[QuerySuggestions] ADD [Query] NVARCHAR(500) NULL
    PRINT 'Query column added successfully.'
END

-- Add RequiredTables column if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.QuerySuggestions') AND name = 'RequiredTables')
BEGIN
    PRINT 'Adding RequiredTables column...'
    ALTER TABLE [dbo].[QuerySuggestions] ADD [RequiredTables] NVARCHAR(500) NULL
    PRINT 'RequiredTables column added successfully.'
END

-- Add Source column if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.QuerySuggestions') AND name = 'Source')
BEGIN
    PRINT 'Adding Source column...'
    ALTER TABLE [dbo].[QuerySuggestions] ADD [Source] NVARCHAR(100) NULL DEFAULT 'System'
    PRINT 'Source column added successfully.'
END

GO

-- Populate Query column with QueryText values (separate batch to avoid the error)
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.QuerySuggestions') AND name = 'Query')
AND EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.QuerySuggestions') AND name = 'QueryText')
BEGIN
    PRINT 'Populating Query column with QueryText values...'
    UPDATE [dbo].[QuerySuggestions] SET [Query] = [QueryText] WHERE [Query] IS NULL
    PRINT 'Query column populated successfully.'
END

PRINT 'All missing columns have been added!'
PRINT 'Text and Relevance columns already existed in the table.'
PRINT 'QuerySuggestions schema fix completed successfully!'

GO
