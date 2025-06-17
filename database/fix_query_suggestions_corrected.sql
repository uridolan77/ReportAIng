-- Fix Missing QuerySuggestions Data - CORRECTED VERSION
-- Run this script to populate the existing tables with proper data

USE [BIReportingCopilot_Dev];
GO

-- Insert default categories if they don't exist (using correct column names)
IF NOT EXISTS (SELECT * FROM [dbo].[SuggestionCategories])
BEGIN
    PRINT 'Inserting default suggestion categories...';
    
    INSERT INTO [dbo].[SuggestionCategories] ([Name], [Description], [DisplayOrder], [IsActive], [CreatedAt], [UpdatedAt])
    VALUES 
        ('Basic Queries', 'Simple data retrieval queries', 1, 1, GETUTCDATE(), GETUTCDATE()),
        ('Analytics', 'Data analysis and reporting queries', 2, 1, GETUTCDATE(), GETUTCDATE()),
        ('Performance', 'Performance monitoring queries', 3, 1, GETUTCDATE(), GETUTCDATE()),
        ('Business Intelligence', 'BI and dashboard queries', 4, 1, GETUTCDATE(), GETUTCDATE()),
        ('Data Exploration', 'Exploratory data analysis', 5, 1, GETUTCDATE(), GETUTCDATE());
    
    PRINT 'Default categories inserted successfully.';
END
ELSE
BEGIN
    PRINT 'Suggestion categories already exist.';
END
GO

-- Insert sample query suggestions if they don't exist
IF NOT EXISTS (SELECT * FROM [dbo].[QuerySuggestions])
BEGIN
    PRINT 'Inserting sample query suggestions...';
    
    DECLARE @BasicQueriesId int = (SELECT Id FROM [dbo].[SuggestionCategories] WHERE [Name] = 'Basic Queries');
    DECLARE @AnalyticsId int = (SELECT Id FROM [dbo].[SuggestionCategories] WHERE [Name] = 'Analytics');
    DECLARE @PerformanceId int = (SELECT Id FROM [dbo].[SuggestionCategories] WHERE [Name] = 'Performance');
    DECLARE @BIId int = (SELECT Id FROM [dbo].[SuggestionCategories] WHERE [Name] = 'Business Intelligence');
    DECLARE @ExplorationId int = (SELECT Id FROM [dbo].[SuggestionCategories] WHERE [Name] = 'Data Exploration');
    
    INSERT INTO [dbo].[QuerySuggestions] ([CategoryId], [Text], [Description], [QueryText], [Tags], [Keywords], [Complexity], [Confidence], [IsActive], [SortOrder], [CreatedAt])
    VALUES 
        -- Basic Queries
        (@BasicQueriesId, 'Show all users', 'Display all users in the system', 'SELECT * FROM Users ORDER BY CreatedDate DESC', 'users,basic,list', 'users,all,list,show', 1, 0.9, 1, 1, GETUTCDATE()),
        (@BasicQueriesId, 'Count total records', 'Count total number of records in a table', 'SELECT COUNT(*) as TotalRecords FROM [TableName]', 'count,basic,total', 'count,total,records,number', 1, 0.9, 1, 2, GETUTCDATE()),
        (@BasicQueriesId, 'Recent activity', 'Show recent activity in the system', 'SELECT TOP 100 * FROM ActivityLog ORDER BY CreatedDate DESC', 'activity,recent,basic', 'recent,activity,log,latest', 1, 0.8, 1, 3, GETUTCDATE()),
        
        -- Analytics
        (@AnalyticsId, 'Monthly user activity', 'Show user activity by month', 'SELECT YEAR(CreatedDate) as Year, MONTH(CreatedDate) as Month, COUNT(*) as UserCount FROM Users GROUP BY YEAR(CreatedDate), MONTH(CreatedDate) ORDER BY Year DESC, Month DESC', 'analytics,monthly,users', 'monthly,activity,users,trend', 2, 0.8, 1, 1, GETUTCDATE()),
        (@AnalyticsId, 'Top 10 most active users', 'Find the most active users', 'SELECT TOP 10 u.Username, COUNT(a.Id) as ActivityCount FROM Users u LEFT JOIN ActivityLog a ON u.Id = a.UserId GROUP BY u.Id, u.Username ORDER BY ActivityCount DESC', 'analytics,top,users', 'top,active,users,ranking', 2, 0.8, 1, 2, GETUTCDATE()),
        (@AnalyticsId, 'Daily query trends', 'Analyze daily query execution trends', 'SELECT CAST(CreatedDate as DATE) as QueryDate, COUNT(*) as QueryCount FROM QueryHistory GROUP BY CAST(CreatedDate as DATE) ORDER BY QueryDate DESC', 'analytics,daily,queries', 'daily,query,trends,statistics', 2, 0.7, 1, 3, GETUTCDATE()),
        
        -- Performance
        (@PerformanceId, 'Slow queries analysis', 'Find queries that take longer to execute', 'SELECT QueryText, AVG(ExecutionTimeMs) as AvgExecutionTime, COUNT(*) as ExecutionCount FROM QueryHistory WHERE ExecutionTimeMs > 1000 GROUP BY QueryText ORDER BY AvgExecutionTime DESC', 'performance,slow,queries', 'slow,queries,performance,analysis', 3, 0.7, 1, 1, GETUTCDATE()),
        (@PerformanceId, 'Database size analysis', 'Analyze database table sizes', 'SELECT t.NAME AS TableName, s.Name AS SchemaName, p.rows AS RowCounts, SUM(a.total_pages) * 8 AS TotalSpaceKB FROM sys.tables t INNER JOIN sys.indexes i ON t.OBJECT_ID = i.object_id INNER JOIN sys.partitions p ON i.object_id = p.OBJECT_ID AND i.index_id = p.index_id INNER JOIN sys.allocation_units a ON p.partition_id = a.container_id LEFT OUTER JOIN sys.schemas s ON t.schema_id = s.schema_id GROUP BY t.Name, s.Name, p.Rows ORDER BY TotalSpaceKB DESC', 'performance,database,size', 'database,size,tables,space', 3, 0.6, 1, 2, GETUTCDATE()),
        
        -- Business Intelligence
        (@BIId, 'User engagement metrics', 'Calculate user engagement metrics', 'SELECT COUNT(DISTINCT UserId) as ActiveUsers, AVG(CAST(SessionDuration as FLOAT)) as AvgSessionDuration, COUNT(*) as TotalSessions FROM UserSessions WHERE CreatedDate >= DATEADD(day, -30, GETDATE())', 'bi,engagement,metrics', 'engagement,metrics,users,sessions', 2, 0.8, 1, 1, GETUTCDATE()),
        (@BIId, 'Revenue by month', 'Show revenue trends by month', 'SELECT YEAR(OrderDate) as Year, MONTH(OrderDate) as Month, SUM(TotalAmount) as MonthlyRevenue FROM Orders WHERE OrderDate >= DATEADD(year, -1, GETDATE()) GROUP BY YEAR(OrderDate), MONTH(OrderDate) ORDER BY Year DESC, Month DESC', 'bi,revenue,monthly', 'revenue,monthly,trends,financial', 2, 0.7, 1, 2, GETUTCDATE()),
        
        -- Data Exploration
        (@ExplorationId, 'Data quality check', 'Check for data quality issues', 'SELECT ''Users'' as TableName, COUNT(*) as TotalRecords, COUNT(CASE WHEN Email IS NULL OR Email = '''' THEN 1 END) as MissingEmails, COUNT(CASE WHEN Username IS NULL OR Username = '''' THEN 1 END) as MissingUsernames FROM Users', 'exploration,quality,data', 'data,quality,check,validation', 2, 0.7, 1, 1, GETUTCDATE()),
        (@ExplorationId, 'Schema exploration', 'Explore database schema structure', 'SELECT TABLE_SCHEMA, TABLE_NAME, COLUMN_NAME, DATA_TYPE, IS_NULLABLE FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = ''dbo'' ORDER BY TABLE_NAME, ORDINAL_POSITION', 'exploration,schema,structure', 'schema,structure,tables,columns', 1, 0.8, 1, 2, GETUTCDATE());
    
    PRINT 'Sample query suggestions inserted successfully.';
END
ELSE
BEGIN
    PRINT 'Query suggestions already exist.';
END
GO

-- Verify the data was inserted correctly
PRINT 'Verification - Categories count:';
SELECT COUNT(*) as CategoryCount FROM [dbo].[SuggestionCategories];

PRINT 'Verification - Query suggestions count:';
SELECT COUNT(*) as SuggestionCount FROM [dbo].[QuerySuggestions];

PRINT 'Verification - Categories with suggestion counts:';
SELECT 
    sc.[Name] as CategoryName,
    COUNT(qs.Id) as SuggestionCount
FROM [dbo].[SuggestionCategories] sc
LEFT JOIN [dbo].[QuerySuggestions] qs ON sc.Id = qs.CategoryId
GROUP BY sc.Id, sc.[Name], sc.DisplayOrder
ORDER BY sc.DisplayOrder;

PRINT 'Database fix completed successfully!';
PRINT 'The QuerySuggestions table has been populated with sample data.';
