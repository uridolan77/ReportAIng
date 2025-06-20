-- Step 2: Populate the newly added columns
-- Run this script second in SQL Server Management Studio against BIReportingCopilot_Dev database

USE [BIReportingCopilot_Dev]
GO

PRINT 'Step 2: Populating newly added columns...'

-- Update Text column with QueryText values
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.QuerySuggestions') AND name = 'Text')
BEGIN
    PRINT 'Updating Text column with QueryText values...'
    UPDATE [dbo].[QuerySuggestions] 
    SET [Text] = [QueryText] 
    WHERE [Text] IS NULL OR [Text] = ''
    PRINT 'Text column updated successfully.'
END

-- Update Relevance column with default values
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.QuerySuggestions') AND name = 'Relevance')
BEGIN
    PRINT 'Updating Relevance column with default values...'
    UPDATE [dbo].[QuerySuggestions] 
    SET [Relevance] = 0.8 
    WHERE [Relevance] IS NULL
    PRINT 'Relevance column updated successfully.'
END

PRINT 'Step 2 completed. Now run Step3_MakeNotNull.sql'

GO
