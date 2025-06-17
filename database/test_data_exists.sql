-- Test if the data was inserted successfully
USE [BIReportingCopilot_Dev];
GO

PRINT 'Testing if data exists in the tables...';

-- Check SuggestionCategories
PRINT 'SuggestionCategories count:';
SELECT COUNT(*) as CategoryCount FROM [dbo].[SuggestionCategories];

-- Check QuerySuggestions
PRINT 'QuerySuggestions count:';
SELECT COUNT(*) as SuggestionCount FROM [dbo].[QuerySuggestions];

-- Show sample data
PRINT 'Sample categories:';
SELECT TOP 3 Id, [Name], [Description], DisplayOrder, IsActive FROM [dbo].[SuggestionCategories] ORDER BY DisplayOrder;

PRINT 'Sample suggestions:';
SELECT TOP 3 Id, CategoryId, [Text], [Description] FROM [dbo].[QuerySuggestions] ORDER BY Id;
