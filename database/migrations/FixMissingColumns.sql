-- Fix missing columns in QuerySuggestions table
-- Run this script against your BIReportingCopilot_Dev database

USE [BIReportingCopilot_Dev]
GO

PRINT 'Starting database schema fix...'

-- Check if QuerySuggestions table exists
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'QuerySuggestions')
BEGIN
    PRINT 'ERROR: QuerySuggestions table does not exist!'
    PRINT 'Please run the CreateQuerySuggestionsSystem.sql script first.'
    RETURN
END

PRINT 'QuerySuggestions table found.'

-- Add Text column if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.QuerySuggestions') AND name = 'Text')
BEGIN
    PRINT 'Adding Text column...'
    ALTER TABLE [dbo].[QuerySuggestions] ADD [Text] NVARCHAR(500) NOT NULL DEFAULT ''
    
    -- Update Text column with QueryText values
    UPDATE [dbo].[QuerySuggestions] SET [Text] = [QueryText] WHERE [Text] = ''
    
    PRINT 'Text column added and populated successfully.'
END
ELSE
BEGIN
    PRINT 'Text column already exists.'
END

-- Add Relevance column if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.QuerySuggestions') AND name = 'Relevance')
BEGIN
    PRINT 'Adding Relevance column...'
    ALTER TABLE [dbo].[QuerySuggestions] ADD [Relevance] DECIMAL(3,2) NOT NULL DEFAULT 0.8
    
    PRINT 'Relevance column added successfully.'
END
ELSE
BEGIN
    PRINT 'Relevance column already exists.'
    
    -- Check if Relevance column is FLOAT and needs to be converted to DECIMAL
    IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.QuerySuggestions') AND name = 'Relevance' AND system_type_id = 62) -- 62 = FLOAT
    BEGIN
        PRINT 'Converting Relevance column from FLOAT to DECIMAL(3,2)...'
        
        -- Add temporary column
        ALTER TABLE [dbo].[QuerySuggestions] ADD [Relevance_New] DECIMAL(3,2) NOT NULL DEFAULT 0.8
        
        -- Copy data
        UPDATE [dbo].[QuerySuggestions] SET [Relevance_New] = CAST([Relevance] AS DECIMAL(3,2))
        
        -- Drop old column
        ALTER TABLE [dbo].[QuerySuggestions] DROP COLUMN [Relevance]
        
        -- Rename new column
        EXEC sp_rename 'dbo.QuerySuggestions.Relevance_New', 'Relevance', 'COLUMN'
        
        PRINT 'Relevance column converted to DECIMAL(3,2) successfully.'
    END
END

-- Fix PromptTemplates SuccessRate precision if table exists
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'PromptTemplates')
BEGIN
    PRINT 'PromptTemplates table found, checking SuccessRate precision...'
    
    IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.PromptTemplates') AND name = 'SuccessRate')
    BEGIN
        -- Check if SuccessRate column needs precision fix
        IF EXISTS (SELECT * FROM sys.columns c
                   WHERE c.object_id = OBJECT_ID('dbo.PromptTemplates') 
                   AND c.name = 'SuccessRate'
                   AND (c.precision != 5 OR c.scale != 2))
        BEGIN
            PRINT 'Fixing PromptTemplates.SuccessRate column precision...'
            
            -- Add temporary column
            ALTER TABLE [dbo].[PromptTemplates] ADD [SuccessRate_New] DECIMAL(5,2) NULL
            
            -- Copy data
            UPDATE [dbo].[PromptTemplates] SET [SuccessRate_New] = CAST([SuccessRate] AS DECIMAL(5,2))
            
            -- Drop old column
            ALTER TABLE [dbo].[PromptTemplates] DROP COLUMN [SuccessRate]
            
            -- Rename new column
            EXEC sp_rename 'dbo.PromptTemplates.SuccessRate_New', 'SuccessRate', 'COLUMN'
            
            PRINT 'PromptTemplates.SuccessRate column precision fixed successfully.'
        END
        ELSE
        BEGIN
            PRINT 'PromptTemplates.SuccessRate column precision is already correct.'
        END
    END
    ELSE
    BEGIN
        PRINT 'PromptTemplates.SuccessRate column does not exist.'
    END
END
ELSE
BEGIN
    PRINT 'PromptTemplates table does not exist, skipping precision fix.'
END

-- Verify the changes
PRINT 'Verifying schema changes...'

-- Check QuerySuggestions columns
SELECT 
    c.name AS ColumnName,
    t.name AS DataType,
    c.max_length,
    c.precision,
    c.scale,
    c.is_nullable
FROM sys.columns c
JOIN sys.types t ON c.system_type_id = t.system_type_id
WHERE c.object_id = OBJECT_ID('dbo.QuerySuggestions')
AND c.name IN ('Text', 'Relevance', 'QueryText')
ORDER BY c.name

PRINT 'Schema fix completed successfully!'
PRINT 'You can now restart your application.'

GO
