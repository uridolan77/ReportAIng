-- Migration: Fix QuerySuggestions table schema mismatch
-- This fixes the schema mismatch where Entity Framework expects 'Text' and 'Relevance' columns
-- but the database doesn't have these columns

-- Check if we need to add the Text column (alias for QueryText)
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[QuerySuggestions]') AND name = 'Text')
BEGIN
    -- Add Text column as computed column pointing to QueryText
    ALTER TABLE [dbo].[QuerySuggestions] 
    ADD [Text] AS [QueryText];
    
    PRINT 'Added Text computed column to QuerySuggestions table';
END
ELSE
BEGIN
    PRINT 'Text column already exists in QuerySuggestions table';
END

-- Check if we need to add the Relevance column
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[QuerySuggestions]') AND name = 'Relevance')
BEGIN
    -- Add Relevance column with default value
    ALTER TABLE [dbo].[QuerySuggestions] 
    ADD [Relevance] FLOAT NOT NULL DEFAULT 0.8;
    
    PRINT 'Added Relevance column to QuerySuggestions table';
END
ELSE
BEGIN
    PRINT 'Relevance column already exists in QuerySuggestions table';
END

-- Add index for Relevance column if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[QuerySuggestions]') AND name = 'IX_QuerySuggestions_Relevance')
BEGIN
    CREATE NONCLUSTERED INDEX [IX_QuerySuggestions_Relevance]
        ON [dbo].[QuerySuggestions] ([Relevance] DESC);
    
    PRINT 'Created index IX_QuerySuggestions_Relevance';
END
ELSE
BEGIN
    PRINT 'Index IX_QuerySuggestions_Relevance already exists';
END

PRINT 'QuerySuggestions schema migration completed successfully';
