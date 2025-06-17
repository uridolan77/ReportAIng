-- Check Existing Table Structure
-- Run this to see what columns exist in the SuggestionCategories table

USE [BIReportingCopilot_Dev];
GO

-- Check SuggestionCategories table structure
PRINT 'SuggestionCategories table columns:';
SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    IS_NULLABLE,
    COLUMN_DEFAULT
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'SuggestionCategories'
ORDER BY ORDINAL_POSITION;

-- Check if there's any data in SuggestionCategories
PRINT 'SuggestionCategories data count:';
SELECT COUNT(*) as RecordCount FROM SuggestionCategories;

-- Show sample data if any exists
PRINT 'Sample SuggestionCategories data:';
SELECT TOP 5 * FROM SuggestionCategories;

-- Check QuerySuggestions table structure
PRINT 'QuerySuggestions table columns:';
SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    IS_NULLABLE,
    COLUMN_DEFAULT
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'QuerySuggestions'
ORDER BY ORDINAL_POSITION;
