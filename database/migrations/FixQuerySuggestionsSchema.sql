-- Fix QuerySuggestions table schema
-- Add missing columns if they don't exist

USE [DailyActionsDB]
GO

-- Check if QuerySuggestions table exists
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'QuerySuggestions')
BEGIN
    PRINT 'QuerySuggestions table does not exist. Creating the full table...'
    
    -- Create the full table with all required columns
    CREATE TABLE [dbo].[QuerySuggestions] (
        [Id] BIGINT IDENTITY(1,1) PRIMARY KEY,
        [CategoryId] BIGINT NOT NULL,
        [QueryText] NVARCHAR(500) NOT NULL,
        [Text] NVARCHAR(500) NOT NULL, -- Alias for QueryText to fix interface conflicts
        [Description] NVARCHAR(200) NOT NULL,
        [DefaultTimeFrame] NVARCHAR(50) NULL,
        [SortOrder] INT NOT NULL DEFAULT 0,
        [IsActive] BIT NOT NULL DEFAULT 1,
        [TargetTables] NVARCHAR(500) NULL,
        [Complexity] TINYINT NOT NULL DEFAULT 1,
        [RequiredPermissions] NVARCHAR(200) NULL,
        [Tags] NVARCHAR(300) NULL,
        [UsageCount] INT NOT NULL DEFAULT 0,
        [LastUsed] DATETIME2 NULL,
        [Relevance] DECIMAL(3,2) NOT NULL DEFAULT 0.8, -- Changed from FLOAT to DECIMAL for precision
        [CreatedDate] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [CreatedBy] NVARCHAR(256) NOT NULL,
        [UpdatedDate] DATETIME2 NULL,
        [UpdatedBy] NVARCHAR(256) NULL
    );
    
    PRINT 'QuerySuggestions table created successfully.'
END
ELSE
BEGIN
    PRINT 'QuerySuggestions table exists. Checking for missing columns...'
    
    -- Add Text column if it doesn't exist
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.QuerySuggestions') AND name = 'Text')
    BEGIN
        ALTER TABLE [dbo].[QuerySuggestions] ADD [Text] NVARCHAR(500) NOT NULL DEFAULT '';
        PRINT 'Added Text column to QuerySuggestions table.'
        
        -- Update Text column with QueryText values
        UPDATE [dbo].[QuerySuggestions] SET [Text] = [QueryText] WHERE [Text] = '';
        PRINT 'Updated Text column with QueryText values.'
    END
    ELSE
    BEGIN
        PRINT 'Text column already exists.'
    END
    
    -- Add Relevance column if it doesn't exist
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.QuerySuggestions') AND name = 'Relevance')
    BEGIN
        ALTER TABLE [dbo].[QuerySuggestions] ADD [Relevance] DECIMAL(3,2) NOT NULL DEFAULT 0.8;
        PRINT 'Added Relevance column to QuerySuggestions table.'
    END
    ELSE
    BEGIN
        PRINT 'Relevance column already exists.'
    END
    
    -- Check if Relevance column is FLOAT and convert to DECIMAL if needed
    IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.QuerySuggestions') AND name = 'Relevance' AND system_type_id = 62) -- 62 = FLOAT
    BEGIN
        PRINT 'Converting Relevance column from FLOAT to DECIMAL(3,2)...'
        
        -- Add temporary column
        ALTER TABLE [dbo].[QuerySuggestions] ADD [Relevance_New] DECIMAL(3,2) NOT NULL DEFAULT 0.8;
        
        -- Copy data
        UPDATE [dbo].[QuerySuggestions] SET [Relevance_New] = CAST([Relevance] AS DECIMAL(3,2));
        
        -- Drop old column
        ALTER TABLE [dbo].[QuerySuggestions] DROP COLUMN [Relevance];
        
        -- Rename new column
        EXEC sp_rename 'dbo.QuerySuggestions.Relevance_New', 'Relevance', 'COLUMN';
        
        PRINT 'Relevance column converted to DECIMAL(3,2).'
    END
END

-- Check if SuggestionCategories table exists
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'SuggestionCategories')
BEGIN
    PRINT 'SuggestionCategories table does not exist. Creating...'
    
    CREATE TABLE [dbo].[SuggestionCategories] (
        [Id] BIGINT IDENTITY(1,1) PRIMARY KEY,
        [CategoryKey] NVARCHAR(50) NOT NULL UNIQUE,
        [Title] NVARCHAR(100) NOT NULL,
        [Icon] NVARCHAR(10) NULL,
        [Description] NVARCHAR(200) NULL,
        [SortOrder] INT NOT NULL DEFAULT 0,
        [IsActive] BIT NOT NULL DEFAULT 1,
        [CreatedDate] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [CreatedBy] NVARCHAR(256) NOT NULL,
        [UpdatedDate] DATETIME2 NULL,
        [UpdatedBy] NVARCHAR(256) NULL
    );
    
    PRINT 'SuggestionCategories table created successfully.'
END

-- Add foreign key constraint if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_QuerySuggestions_SuggestionCategories')
BEGIN
    IF EXISTS (SELECT * FROM sys.tables WHERE name = 'SuggestionCategories')
    BEGIN
        ALTER TABLE [dbo].[QuerySuggestions] 
        ADD CONSTRAINT FK_QuerySuggestions_SuggestionCategories 
        FOREIGN KEY ([CategoryId]) REFERENCES [dbo].[SuggestionCategories]([Id]) ON DELETE CASCADE;
        
        PRINT 'Added foreign key constraint between QuerySuggestions and SuggestionCategories.'
    END
END

-- Fix PromptTemplates table SuccessRate column precision
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'PromptTemplates')
BEGIN
    IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.PromptTemplates') AND name = 'SuccessRate')
    BEGIN
        -- Check if SuccessRate is not already DECIMAL(5,2)
        IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.PromptTemplates') AND name = 'SuccessRate' AND precision = 5 AND scale = 2)
        BEGIN
            PRINT 'Fixing PromptTemplates.SuccessRate column precision...'
            
            -- Add temporary column
            ALTER TABLE [dbo].[PromptTemplates] ADD [SuccessRate_New] DECIMAL(5,2) NULL;
            
            -- Copy data
            UPDATE [dbo].[PromptTemplates] SET [SuccessRate_New] = CAST([SuccessRate] AS DECIMAL(5,2));
            
            -- Drop old column
            ALTER TABLE [dbo].[PromptTemplates] DROP COLUMN [SuccessRate];
            
            -- Rename new column
            EXEC sp_rename 'dbo.PromptTemplates.SuccessRate_New', 'SuccessRate', 'COLUMN';
            
            PRINT 'PromptTemplates.SuccessRate column precision fixed.'
        END
        ELSE
        BEGIN
            PRINT 'PromptTemplates.SuccessRate column already has correct precision.'
        END
    END
END

PRINT 'Schema fix completed successfully.'
