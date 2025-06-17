-- Update SuggestionCategories table schema to match entity model
-- This script adds the CategoryKey column and ensures all columns match the entity model

USE [BIReportingCopilot_dev]
GO

-- Check if CategoryKey column exists, if not add it
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[SuggestionCategories]') AND name = 'CategoryKey')
BEGIN
    PRINT 'Adding CategoryKey column to SuggestionCategories table...'
    
    -- Add CategoryKey column
    ALTER TABLE [dbo].[SuggestionCategories]
    ADD [CategoryKey] NVARCHAR(50) NULL;
    
    -- Update existing records to have CategoryKey values based on Name
    UPDATE [dbo].[SuggestionCategories]
    SET [CategoryKey] = LOWER(REPLACE([Name], ' ', '-'));
    
    -- Make CategoryKey NOT NULL after populating values
    ALTER TABLE [dbo].[SuggestionCategories]
    ALTER COLUMN [CategoryKey] NVARCHAR(50) NOT NULL;
    
    -- Add unique index on CategoryKey
    CREATE UNIQUE NONCLUSTERED INDEX [IX_SuggestionCategories_CategoryKey] 
    ON [dbo].[SuggestionCategories]([CategoryKey] ASC);
    
    PRINT 'CategoryKey column added successfully.'
END
ELSE
BEGIN
    PRINT 'CategoryKey column already exists.'
END

-- Check if we need to rename columns to match entity model
-- The entity model expects: Name, DisplayOrder, CreatedAt, UpdatedAt
-- But the database might have: Title, SortOrder, CreatedDate, UpdatedDate

-- Check if Title column exists and Name doesn't (need to rename Title to Name)
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[SuggestionCategories]') AND name = 'Title')
   AND NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[SuggestionCategories]') AND name = 'Name')
BEGIN
    PRINT 'Renaming Title column to Name...'
    EXEC sp_rename 'SuggestionCategories.Title', 'Name', 'COLUMN';
    PRINT 'Title column renamed to Name.'
END

-- Check if SortOrder column exists and DisplayOrder doesn't (need to rename SortOrder to DisplayOrder)
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[SuggestionCategories]') AND name = 'SortOrder')
   AND NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[SuggestionCategories]') AND name = 'DisplayOrder')
BEGIN
    PRINT 'Renaming SortOrder column to DisplayOrder...'
    EXEC sp_rename 'SuggestionCategories.SortOrder', 'DisplayOrder', 'COLUMN';
    PRINT 'SortOrder column renamed to DisplayOrder.'
END

-- Check if CreatedDate column exists and CreatedAt doesn't (need to rename CreatedDate to CreatedAt)
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[SuggestionCategories]') AND name = 'CreatedDate')
   AND NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[SuggestionCategories]') AND name = 'CreatedAt')
BEGIN
    PRINT 'Renaming CreatedDate column to CreatedAt...'
    EXEC sp_rename 'SuggestionCategories.CreatedDate', 'CreatedAt', 'COLUMN';
    PRINT 'CreatedDate column renamed to CreatedAt.'
END

-- Check if UpdatedDate column exists and UpdatedAt doesn't (need to rename UpdatedDate to UpdatedAt)
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[SuggestionCategories]') AND name = 'UpdatedDate')
   AND NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[SuggestionCategories]') AND name = 'UpdatedAt')
BEGIN
    PRINT 'Renaming UpdatedDate column to UpdatedAt...'
    EXEC sp_rename 'SuggestionCategories.UpdatedDate', 'UpdatedAt', 'COLUMN';
    PRINT 'UpdatedDate column renamed to UpdatedAt.'
END

-- Update index on DisplayOrder and IsActive if it exists with old name
IF EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[SuggestionCategories]') AND name = 'IX_SuggestionCategories_IsActive_SortOrder')
BEGIN
    PRINT 'Dropping old index IX_SuggestionCategories_IsActive_SortOrder...'
    DROP INDEX [IX_SuggestionCategories_IsActive_SortOrder] ON [dbo].[SuggestionCategories];
    PRINT 'Old index dropped.'
END

-- Create new index with correct column names
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[SuggestionCategories]') AND name = 'IX_SuggestionCategories_IsActive_DisplayOrder')
BEGIN
    PRINT 'Creating new index IX_SuggestionCategories_IsActive_DisplayOrder...'
    CREATE NONCLUSTERED INDEX [IX_SuggestionCategories_IsActive_DisplayOrder] 
    ON [dbo].[SuggestionCategories]([IsActive] ASC, [DisplayOrder] ASC);
    PRINT 'New index created.'
END

-- Verify the final schema
PRINT 'Final SuggestionCategories table schema:'
SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    IS_NULLABLE,
    CHARACTER_MAXIMUM_LENGTH,
    COLUMN_DEFAULT
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'SuggestionCategories'
ORDER BY ORDINAL_POSITION;

PRINT 'SuggestionCategories schema update completed successfully!'
