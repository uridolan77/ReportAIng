-- Create fresh SuggestionCategories table with proper schema
-- This script drops and recreates the table cleanly

USE [BIReportingCopilot_dev]
GO

PRINT 'Creating fresh SuggestionCategories table...'

-- Step 1: Drop existing table and related objects
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SuggestionCategories]') AND type in (N'U'))
BEGIN
    PRINT 'Dropping existing SuggestionCategories table...'
    
    -- Drop foreign key constraints from QuerySuggestions table if they exist
    IF EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_QuerySuggestions_SuggestionCategories_CategoryId]'))
    BEGIN
        ALTER TABLE [dbo].[QuerySuggestions] DROP CONSTRAINT [FK_QuerySuggestions_SuggestionCategories_CategoryId]
        PRINT 'Dropped foreign key constraint FK_QuerySuggestions_SuggestionCategories_CategoryId'
    END
    
    -- Drop the table
    DROP TABLE [dbo].[SuggestionCategories]
    PRINT 'SuggestionCategories table dropped.'
END

-- Step 2: Create new SuggestionCategories table with enhanced schema
PRINT 'Creating new SuggestionCategories table...'

CREATE TABLE [dbo].[SuggestionCategories] (
    [Id] BIGINT IDENTITY(1,1) NOT NULL,
    [CategoryKey] NVARCHAR(50) NOT NULL,
    [Name] NVARCHAR(100) NOT NULL,
    [Description] NVARCHAR(500) NULL,
    [IsActive] BIT NOT NULL DEFAULT 1,
    [DisplayOrder] INT NOT NULL DEFAULT 0,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    [UpdatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    [CreatedBy] NVARCHAR(256) NULL,
    [UpdatedBy] NVARCHAR(256) NULL,
    
    CONSTRAINT [PK_SuggestionCategories] PRIMARY KEY CLUSTERED ([Id] ASC)
)

PRINT 'SuggestionCategories table created.'

-- Step 3: Create indexes
PRINT 'Creating indexes...'

-- Unique index on CategoryKey
CREATE UNIQUE NONCLUSTERED INDEX [IX_SuggestionCategories_CategoryKey] 
ON [dbo].[SuggestionCategories]([CategoryKey] ASC)

-- Index on IsActive and DisplayOrder for efficient querying
CREATE NONCLUSTERED INDEX [IX_SuggestionCategories_IsActive_DisplayOrder] 
ON [dbo].[SuggestionCategories]([IsActive] ASC, [DisplayOrder] ASC)

PRINT 'Indexes created.'

-- Step 4: Insert default categories
PRINT 'Inserting default categories...'

INSERT INTO [dbo].[SuggestionCategories] (
    [CategoryKey],
    [Name],
    [Description],
    [IsActive],
    [DisplayOrder],
    [CreatedBy]
) VALUES 
('sales-analysis', 'Sales Analysis', 'Queries related to sales performance and trends', 1, 1, 'system'),
('financial-reports', 'Financial Reports', 'Financial reporting and analysis queries', 1, 2, 'system'),
('customer-insights', 'Customer Insights', 'Customer behavior and demographic analysis', 1, 3, 'system'),
('operational-metrics', 'Operational Metrics', 'Operational performance and efficiency metrics', 1, 4, 'system'),
('inventory-management', 'Inventory Management', 'Inventory levels, turnover, and optimization', 1, 5, 'system'),
('data-quality', 'Data Quality', 'Data validation and quality assessment queries', 1, 6, 'system'),
('performance-monitoring', 'Performance Monitoring', 'System and application performance metrics', 1, 7, 'system'),
('compliance-reporting', 'Compliance Reporting', 'Regulatory and compliance related queries', 1, 8, 'system'),
('trend-analysis', 'Trend Analysis', 'Historical trends and forecasting queries', 1, 9, 'system'),
('ad-hoc-queries', 'Ad-hoc Queries', 'Custom and exploratory data queries', 1, 10, 'system')

PRINT 'Default categories inserted: ' + CAST(@@ROWCOUNT AS VARCHAR(10))

-- Step 5: Recreate foreign key constraint if QuerySuggestions table exists
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[QuerySuggestions]') AND type in (N'U'))
BEGIN
    PRINT 'Recreating foreign key constraint...'
    
    ALTER TABLE [dbo].[QuerySuggestions]
    ADD CONSTRAINT [FK_QuerySuggestions_SuggestionCategories_CategoryId] 
    FOREIGN KEY ([CategoryId]) REFERENCES [dbo].[SuggestionCategories] ([Id])
    ON DELETE CASCADE
    
    PRINT 'Foreign key constraint recreated.'
END

-- Step 6: Show final schema
PRINT 'Final SuggestionCategories table schema:'
SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    IS_NULLABLE,
    CHARACTER_MAXIMUM_LENGTH,
    COLUMN_DEFAULT
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'SuggestionCategories'
ORDER BY ORDINAL_POSITION

-- Show data
PRINT 'Current data in SuggestionCategories:'
SELECT 
    [Id],
    [CategoryKey],
    [Name],
    [Description],
    [IsActive],
    [DisplayOrder]
FROM [dbo].[SuggestionCategories]
ORDER BY [DisplayOrder]

PRINT 'SuggestionCategories table creation completed successfully!'
