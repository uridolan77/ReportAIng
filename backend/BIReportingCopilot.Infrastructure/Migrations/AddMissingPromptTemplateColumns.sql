-- Migration to add missing columns to PromptTemplates table
-- These columns are expected by Core.Models.PromptTemplateEntity but missing from the main BICopilotContext

USE [BICopilot]
GO

-- Check if columns exist before adding them
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('PromptTemplates') AND name = 'BusinessMetadata')
BEGIN
    ALTER TABLE [PromptTemplates] ADD [BusinessMetadata] NVARCHAR(MAX) NULL;
    PRINT 'Added BusinessMetadata column to PromptTemplates table';
END
ELSE
BEGIN
    PRINT 'BusinessMetadata column already exists in PromptTemplates table';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('PromptTemplates') AND name = 'LastBusinessReviewDate')
BEGIN
    ALTER TABLE [PromptTemplates] ADD [LastBusinessReviewDate] DATETIME2 NULL;
    PRINT 'Added LastBusinessReviewDate column to PromptTemplates table';
END
ELSE
BEGIN
    PRINT 'LastBusinessReviewDate column already exists in PromptTemplates table';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('PromptTemplates') AND name = 'LastUsedDate')
BEGIN
    ALTER TABLE [PromptTemplates] ADD [LastUsedDate] DATETIME2 NULL;
    PRINT 'Added LastUsedDate column to PromptTemplates table';
END
ELSE
BEGIN
    PRINT 'LastUsedDate column already exists in PromptTemplates table';
END

-- Add other missing business context columns that are expected by the Core model
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('PromptTemplates') AND name = 'BusinessPurpose')
BEGIN
    ALTER TABLE [PromptTemplates] ADD [BusinessPurpose] NVARCHAR(1000) NULL;
    PRINT 'Added BusinessPurpose column to PromptTemplates table';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('PromptTemplates') AND name = 'RelatedBusinessTerms')
BEGIN
    ALTER TABLE [PromptTemplates] ADD [RelatedBusinessTerms] NVARCHAR(1000) NULL;
    PRINT 'Added RelatedBusinessTerms column to PromptTemplates table';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('PromptTemplates') AND name = 'BusinessFriendlyName')
BEGIN
    ALTER TABLE [PromptTemplates] ADD [BusinessFriendlyName] NVARCHAR(200) NULL;
    PRINT 'Added BusinessFriendlyName column to PromptTemplates table';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('PromptTemplates') AND name = 'NaturalLanguageDescription')
BEGIN
    ALTER TABLE [PromptTemplates] ADD [NaturalLanguageDescription] NVARCHAR(1000) NULL;
    PRINT 'Added NaturalLanguageDescription column to PromptTemplates table';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('PromptTemplates') AND name = 'BusinessRules')
BEGIN
    ALTER TABLE [PromptTemplates] ADD [BusinessRules] NVARCHAR(2000) NULL;
    PRINT 'Added BusinessRules column to PromptTemplates table';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('PromptTemplates') AND name = 'RelationshipContext')
BEGIN
    ALTER TABLE [PromptTemplates] ADD [RelationshipContext] NVARCHAR(1000) NULL;
    PRINT 'Added RelationshipContext column to PromptTemplates table';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('PromptTemplates') AND name = 'DataGovernanceLevel')
BEGIN
    ALTER TABLE [PromptTemplates] ADD [DataGovernanceLevel] NVARCHAR(50) NULL;
    PRINT 'Added DataGovernanceLevel column to PromptTemplates table';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('PromptTemplates') AND name = 'ImportanceScore')
BEGIN
    ALTER TABLE [PromptTemplates] ADD [ImportanceScore] DECIMAL(5,2) NULL DEFAULT 0.5;
    PRINT 'Added ImportanceScore column to PromptTemplates table';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('PromptTemplates') AND name = 'UsageFrequency')
BEGIN
    ALTER TABLE [PromptTemplates] ADD [UsageFrequency] NVARCHAR(50) NULL;
    PRINT 'Added UsageFrequency column to PromptTemplates table';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('PromptTemplates') AND name = 'TemplateKey')
BEGIN
    ALTER TABLE [PromptTemplates] ADD [TemplateKey] NVARCHAR(100) NULL;
    PRINT 'Added TemplateKey column to PromptTemplates table';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('PromptTemplates') AND name = 'IntentType')
BEGIN
    ALTER TABLE [PromptTemplates] ADD [IntentType] NVARCHAR(50) NULL;
    PRINT 'Added IntentType column to PromptTemplates table';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('PromptTemplates') AND name = 'Priority')
BEGIN
    ALTER TABLE [PromptTemplates] ADD [Priority] INT NULL DEFAULT 1;
    PRINT 'Added Priority column to PromptTemplates table';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('PromptTemplates') AND name = 'Tags')
BEGIN
    ALTER TABLE [PromptTemplates] ADD [Tags] NVARCHAR(1000) NULL;
    PRINT 'Added Tags column to PromptTemplates table';
END

-- Update existing records with default values for TemplateKey and IntentType if they are NULL
UPDATE [PromptTemplates] 
SET [TemplateKey] = LOWER(REPLACE([Name], ' ', '_'))
WHERE [TemplateKey] IS NULL;

UPDATE [PromptTemplates] 
SET [IntentType] = 'QUERY_GENERATION'
WHERE [IntentType] IS NULL;

PRINT 'Migration completed successfully. All missing columns have been added to PromptTemplates table.';
