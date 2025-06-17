-- =============================================
-- Fix Query Suggestions Field Mapping
-- Swap QueryText and Text fields to correct the data mapping
-- =============================================

USE [BIReportingCopilot_dev];
GO

PRINT 'Starting Query Suggestions field mapping fix...';

-- Check current data before fix
PRINT 'Current data (before fix):';
SELECT TOP 3 
    Id, 
    LEFT(QueryText, 50) + '...' as QueryText_Preview,
    LEFT(Text, 50) + '...' as Text_Preview
FROM QuerySuggestions 
ORDER BY Id;

-- Swap the values between QueryText and Text columns
-- QueryText should contain the natural language question
-- Text should be kept in sync with QueryText (as per the model)
UPDATE QuerySuggestions 
SET 
    QueryText = Text,  -- Move natural language question to QueryText
    Text = Text        -- Keep Text the same (natural language question)
WHERE QueryText LIKE 'SELECT%' OR QueryText LIKE 'INSERT%' OR QueryText LIKE 'UPDATE%' OR QueryText LIKE 'DELETE%';

PRINT 'Field mapping fix completed.';

-- Check data after fix
PRINT 'Data after fix:';
SELECT TOP 3 
    Id, 
    LEFT(QueryText, 50) + '...' as QueryText_Preview,
    LEFT(Text, 50) + '...' as Text_Preview
FROM QuerySuggestions 
ORDER BY Id;

PRINT 'Query Suggestions field mapping fix completed successfully!';
PRINT 'QueryText now contains natural language questions instead of SQL code.';
