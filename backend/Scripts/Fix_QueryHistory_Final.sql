-- =====================================================================================
-- Final Fix for QueryHistory Table - Populate Missing Data
-- =====================================================================================

USE [BIReportingCopilot_Dev]
GO

PRINT 'Final QueryHistory table data population...'

-- Populate the new columns with data from existing columns
UPDATE [dbo].[QueryHistory] 
SET 
    [OriginalQuery] = COALESCE([OriginalQuery], [NaturalLanguageQuery], ''),
    [GeneratedSql] = COALESCE([GeneratedSql], [GeneratedSQL], ''),
    [CreatedAt] = COALESCE([CreatedAt], [CreatedDate], GETUTCDATE()),
    [UpdatedAt] = COALESCE([UpdatedAt], [UpdatedDate], [LastUpdated], GETUTCDATE()),
    [Status] = COALESCE([Status], CASE WHEN [IsSuccessful] = 1 THEN 'Success' ELSE 'Failed' END),
    [Classification] = COALESCE([Classification], 'General'),
    [Explanation] = COALESCE([Explanation], ''),
    [Metadata] = COALESCE([Metadata], '{}'),
    [ResultData] = COALESCE([ResultData], '')
WHERE 
    [OriginalQuery] IS NULL 
    OR [GeneratedSql] IS NULL 
    OR [CreatedAt] IS NULL 
    OR [UpdatedAt] IS NULL 
    OR [Status] IS NULL 
    OR [Classification] IS NULL

PRINT 'Data population completed!'

-- Verify the data
PRINT ''
PRINT '=== FINAL VERIFICATION ==='
SELECT 
    COUNT(*) as TotalRows,
    COUNT(CASE WHEN OriginalQuery IS NOT NULL AND OriginalQuery != '' THEN 1 END) as RowsWithOriginalQuery,
    COUNT(CASE WHEN GeneratedSql IS NOT NULL AND GeneratedSql != '' THEN 1 END) as RowsWithGeneratedSql,
    COUNT(CASE WHEN CreatedAt IS NOT NULL THEN 1 END) as RowsWithCreatedAt,
    COUNT(CASE WHEN UpdatedAt IS NOT NULL THEN 1 END) as RowsWithUpdatedAt,
    COUNT(CASE WHEN Status IS NOT NULL AND Status != '' THEN 1 END) as RowsWithStatus,
    COUNT(CASE WHEN Classification IS NOT NULL AND Classification != '' THEN 1 END) as RowsWithClassification
FROM [dbo].[QueryHistory]

-- Check for any remaining NULL values in critical fields
PRINT ''
PRINT '=== NULL VALUE CHECK ==='
SELECT 
    'OriginalQuery' as ColumnName,
    COUNT(*) as NullCount
FROM [dbo].[QueryHistory] 
WHERE OriginalQuery IS NULL OR OriginalQuery = ''
UNION ALL
SELECT 
    'GeneratedSql' as ColumnName,
    COUNT(*) as NullCount
FROM [dbo].[QueryHistory] 
WHERE GeneratedSql IS NULL OR GeneratedSql = ''
UNION ALL
SELECT 
    'Status' as ColumnName,
    COUNT(*) as NullCount
FROM [dbo].[QueryHistory] 
WHERE Status IS NULL OR Status = ''
UNION ALL
SELECT 
    'Classification' as ColumnName,
    COUNT(*) as NullCount
FROM [dbo].[QueryHistory] 
WHERE Classification IS NULL OR Classification = ''

PRINT 'QueryHistory table is now ready for Entity Framework! ✅'
