-- =============================================
-- Business-Context-Aware Prompt Building (BCAPB) Complete Cleanup Script
-- This script removes BCAPB enhancements and new tables
-- USE WITH CAUTION - THIS WILL DELETE BCAPB DATA!
-- =============================================

USE [BIReportingCopilot_Dev]
GO

PRINT '============================================='
PRINT 'BCAPB COMPLETE CLEANUP SCRIPT'
PRINT '============================================='
PRINT 'WARNING: This will remove BCAPB enhancements and data!'
PRINT 'Press Ctrl+C to cancel if you do not want to proceed.'
PRINT ''

-- Wait for 5 seconds to allow cancellation
WAITFOR DELAY '00:00:05'

PRINT 'Starting cleanup...'
PRINT ''

-- =============================================
-- 1. DROP FOREIGN KEY CONSTRAINTS
-- =============================================

-- Drop foreign key constraints first
IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_PromptGenerationLogs_PromptTemplates')
BEGIN
    ALTER TABLE [dbo].[PromptGenerationLogs] DROP CONSTRAINT [FK_PromptGenerationLogs_PromptTemplates]
    PRINT 'Dropped FK to PromptTemplates'
END

IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_PromptGenerationLogs_Users')
BEGIN
    ALTER TABLE [dbo].[PromptGenerationLogs] DROP CONSTRAINT [FK_PromptGenerationLogs_Users]
    PRINT 'Dropped FK to Users'
END

-- =============================================
-- 2. DROP NEW TABLES
-- =============================================

-- Drop tables in dependency order
IF EXISTS (SELECT * FROM sysobjects WHERE name='PromptGenerationLogs' AND xtype='U')
BEGIN
    DROP TABLE [dbo].[PromptGenerationLogs]
    PRINT 'PromptGenerationLogs table dropped'
END

IF EXISTS (SELECT * FROM sysobjects WHERE name='QueryExamples' AND xtype='U')
BEGIN
    DROP TABLE [dbo].[QueryExamples]
    PRINT 'QueryExamples table dropped'
END

IF EXISTS (SELECT * FROM sysobjects WHERE name='VectorEmbeddings' AND xtype='U')
BEGIN
    DROP TABLE [dbo].[VectorEmbeddings]
    PRINT 'VectorEmbeddings table dropped'
END

IF EXISTS (SELECT * FROM sysobjects WHERE name='IntentTypes' AND xtype='U')
BEGIN
    DROP TABLE [dbo].[IntentTypes]
    PRINT 'IntentTypes table dropped'
END

-- =============================================
-- 3. REMOVE BCAPB ENHANCEMENTS FROM EXISTING TABLES
-- =============================================

-- Remove BCAPB data from PromptTemplates
IF EXISTS (SELECT * FROM sysobjects WHERE name='PromptTemplates' AND xtype='U')
BEGIN
    -- Delete BCAPB templates
    DELETE FROM [dbo].[PromptTemplates] WHERE [TemplateKey] LIKE 'bcapb_%'
    PRINT 'Removed BCAPB templates from PromptTemplates'
    
    -- Drop BCAPB indexes
    IF EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID('PromptTemplates') AND name = 'IX_PromptTemplates_TemplateKey')
    BEGIN
        DROP INDEX [IX_PromptTemplates_TemplateKey] ON [dbo].[PromptTemplates]
        PRINT 'Dropped TemplateKey index'
    END
    
    IF EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID('PromptTemplates') AND name = 'IX_PromptTemplates_IntentType')
    BEGIN
        DROP INDEX [IX_PromptTemplates_IntentType] ON [dbo].[PromptTemplates]
        PRINT 'Dropped IntentType index'
    END
    
    -- Remove BCAPB columns (optional - uncomment if you want to remove columns completely)
    /*
    IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('PromptTemplates') AND name = 'TemplateKey')
    BEGIN
        ALTER TABLE [dbo].[PromptTemplates] DROP COLUMN [TemplateKey]
        PRINT 'Removed TemplateKey column'
    END
    
    IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('PromptTemplates') AND name = 'IntentType')
    BEGIN
        ALTER TABLE [dbo].[PromptTemplates] DROP COLUMN [IntentType]
        PRINT 'Removed IntentType column'
    END
    
    IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('PromptTemplates') AND name = 'Priority')
    BEGIN
        ALTER TABLE [dbo].[PromptTemplates] DROP COLUMN [Priority]
        PRINT 'Removed Priority column'
    END
    
    IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('PromptTemplates') AND name = 'Tags')
    BEGIN
        ALTER TABLE [dbo].[PromptTemplates] DROP COLUMN [Tags]
        PRINT 'Removed Tags column'
    END
    */
END

PRINT ''
PRINT '============================================='
PRINT 'BCAPB CLEANUP COMPLETED!'
PRINT '============================================='
PRINT 'Removed:'
PRINT '- All new BCAPB tables'
PRINT '- BCAPB data from PromptTemplates'
PRINT '- BCAPB indexes'
PRINT '- Foreign key constraints'
PRINT ''
PRINT 'Note: BCAPB columns in PromptTemplates were kept'
PRINT '(uncomment the column removal section if needed)'
PRINT ''
PRINT 'You can run BCAPB_Complete_Setup.sql to recreate everything.'
PRINT '============================================='
