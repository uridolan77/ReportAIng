-- Migration: Add missing TemplateId and PromptHash columns to PromptLogs table
-- This fixes the schema mismatch error where Entity Framework expects these columns but they don't exist in the database

-- Check if the columns already exist before adding them
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[PromptLogs]') AND name = 'TemplateId')
BEGIN
    ALTER TABLE [dbo].[PromptLogs] 
    ADD [TemplateId] bigint NULL;
    
    PRINT 'Added TemplateId column to PromptLogs table';
END
ELSE
BEGIN
    PRINT 'TemplateId column already exists in PromptLogs table';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[PromptLogs]') AND name = 'PromptHash')
BEGIN
    ALTER TABLE [dbo].[PromptLogs] 
    ADD [PromptHash] nvarchar(64) NULL;
    
    PRINT 'Added PromptHash column to PromptLogs table';
END
ELSE
BEGIN
    PRINT 'PromptHash column already exists in PromptLogs table';
END

-- Add indexes for the new columns if they don't exist
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[PromptLogs]') AND name = 'IX_PromptLogs_TemplateId_CreatedDate')
BEGIN
    CREATE NONCLUSTERED INDEX [IX_PromptLogs_TemplateId_CreatedDate]
        ON [dbo].[PromptLogs] ([TemplateId] ASC, [CreatedDate] DESC);
    
    PRINT 'Created index IX_PromptLogs_TemplateId_CreatedDate';
END
ELSE
BEGIN
    PRINT 'Index IX_PromptLogs_TemplateId_CreatedDate already exists';
END

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[PromptLogs]') AND name = 'IX_PromptLogs_PromptHash')
BEGIN
    CREATE NONCLUSTERED INDEX [IX_PromptLogs_PromptHash]
        ON [dbo].[PromptLogs] ([PromptHash] ASC);
    
    PRINT 'Created index IX_PromptLogs_PromptHash';
END
ELSE
BEGIN
    PRINT 'Index IX_PromptLogs_PromptHash already exists';
END

-- Verify the changes
SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    IS_NULLABLE,
    CHARACTER_MAXIMUM_LENGTH
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'PromptLogs' 
    AND COLUMN_NAME IN ('TemplateId', 'PromptHash')
ORDER BY COLUMN_NAME;

PRINT 'Migration completed successfully. PromptLogs table now has TemplateId and PromptHash columns.';
