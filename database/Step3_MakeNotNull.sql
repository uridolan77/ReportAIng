-- Step 3: Make columns NOT NULL and verify changes
-- Run this script third in SQL Server Management Studio against BIReportingCopilot_Dev database

USE [BIReportingCopilot_Dev]
GO

PRINT 'Step 3: Making columns NOT NULL and verifying changes...'

-- Make Text column NOT NULL
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.QuerySuggestions') AND name = 'Text' AND is_nullable = 1)
BEGIN
    PRINT 'Making Text column NOT NULL...'
    ALTER TABLE [dbo].[QuerySuggestions] ALTER COLUMN [Text] NVARCHAR(500) NOT NULL
    PRINT 'Text column is now NOT NULL.'
END

-- Make Relevance column NOT NULL
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.QuerySuggestions') AND name = 'Relevance' AND is_nullable = 1)
BEGIN
    PRINT 'Making Relevance column NOT NULL...'
    ALTER TABLE [dbo].[QuerySuggestions] ALTER COLUMN [Relevance] DECIMAL(3,2) NOT NULL
    PRINT 'Relevance column is now NOT NULL.'
END

-- Verify the changes
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

PRINT 'All steps completed successfully!'
PRINT 'Database schema fix is complete. You can now restart your application.'

GO
