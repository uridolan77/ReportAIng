-- Update CategoryKey values for existing SuggestionCategories
-- This script populates the empty CategoryKey fields

USE [BIReportingCopilot_dev]
GO

PRINT 'Updating CategoryKey values...'

-- Update CategoryKey values based on the Name field
UPDATE [dbo].[SuggestionCategories]
SET [CategoryKey] = CASE 
    WHEN [Name] = 'Sales Analysis' THEN 'sales-analysis'
    WHEN [Name] = 'Financial Reports' THEN 'financial-reports'
    WHEN [Name] = 'Customer Insights' THEN 'customer-insights'
    WHEN [Name] = 'Operational Metrics' THEN 'operational-metrics'
    WHEN [Name] = 'Inventory Management' THEN 'inventory-management'
    WHEN [Name] = 'Data Quality' THEN 'data-quality'
    WHEN [Name] = 'Performance Monitoring' THEN 'performance-monitoring'
    WHEN [Name] = 'Compliance Reporting' THEN 'compliance-reporting'
    WHEN [Name] = 'Trend Analysis' THEN 'trend-analysis'
    WHEN [Name] = 'Ad-hoc Queries' THEN 'ad-hoc-queries'
    ELSE LOWER(REPLACE([Name], ' ', '-'))
END
WHERE [CategoryKey] IS NULL OR [CategoryKey] = ''

PRINT 'CategoryKey values updated: ' + CAST(@@ROWCOUNT AS VARCHAR(10))

-- Show updated data
SELECT 
    [Id],
    [CategoryKey],
    [Name],
    [DisplayOrder],
    [IsActive]
FROM [dbo].[SuggestionCategories]
ORDER BY [DisplayOrder]

PRINT 'CategoryKey update completed successfully!'
