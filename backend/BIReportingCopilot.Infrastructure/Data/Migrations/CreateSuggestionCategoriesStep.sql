-- Create SuggestionCategories table step by step
-- Run each section separately to identify any issues

USE [BIReportingCopilot_dev]
GO

-- STEP 1: Drop existing table (run this first)
PRINT 'STEP 1: Dropping existing table...'

-- Drop foreign key constraints first
IF EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_QuerySuggestions_SuggestionCategories_CategoryId]'))
BEGIN
    ALTER TABLE [dbo].[QuerySuggestions] DROP CONSTRAINT [FK_QuerySuggestions_SuggestionCategories_CategoryId]
    PRINT 'Dropped foreign key constraint'
END

-- Drop the table
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SuggestionCategories]') AND type in (N'U'))
BEGIN
    DROP TABLE [dbo].[SuggestionCategories]
    PRINT 'SuggestionCategories table dropped.'
END
ELSE
BEGIN
    PRINT 'SuggestionCategories table does not exist.'
END

GO

-- STEP 2: Create new table (run this second)
PRINT 'STEP 2: Creating new table...'

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

PRINT 'SuggestionCategories table created successfully.'

GO

-- STEP 3: Create indexes (run this third)
PRINT 'STEP 3: Creating indexes...'

-- Unique index on CategoryKey
CREATE UNIQUE NONCLUSTERED INDEX [IX_SuggestionCategories_CategoryKey] 
ON [dbo].[SuggestionCategories]([CategoryKey] ASC)

-- Index on IsActive and DisplayOrder
CREATE NONCLUSTERED INDEX [IX_SuggestionCategories_IsActive_DisplayOrder] 
ON [dbo].[SuggestionCategories]([IsActive] ASC, [DisplayOrder] ASC)

PRINT 'Indexes created successfully.'

GO

-- STEP 4: Insert data (run this fourth)
PRINT 'STEP 4: Inserting default categories...'

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
('inventory-management', 'Inventory Management', 'Inventory levels, turnover, and optimization', 1, 5, 'system')

PRINT 'Categories inserted: ' + CAST(@@ROWCOUNT AS VARCHAR(10))

GO

-- STEP 5: Verify table structure (run this fifth)
PRINT 'STEP 5: Verifying table structure...'

SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    IS_NULLABLE,
    CHARACTER_MAXIMUM_LENGTH
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'SuggestionCategories'
ORDER BY ORDINAL_POSITION

GO

-- STEP 6: Show data (run this sixth)
PRINT 'STEP 6: Showing inserted data...'

SELECT 
    [Id],
    [CategoryKey],
    [Name],
    [IsActive],
    [DisplayOrder]
FROM [dbo].[SuggestionCategories]
ORDER BY [DisplayOrder]

PRINT 'Table creation completed successfully!'
