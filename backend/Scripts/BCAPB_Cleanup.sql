-- =============================================
-- Business-Context-Aware Prompt Building (BCAPB) Cleanup Script
-- This script removes all BCAPB tables and data
-- USE WITH CAUTION - THIS WILL DELETE ALL BCAPB DATA!
-- =============================================

USE [BIReportingCopilot_Dev]
GO

PRINT '============================================='
PRINT 'BCAPB CLEANUP SCRIPT'
PRINT '============================================='
PRINT 'WARNING: This will delete all BCAPB tables and data!'
PRINT 'Press Ctrl+C to cancel if you do not want to proceed.'
PRINT ''

-- Wait for 5 seconds to allow cancellation
WAITFOR DELAY '00:00:05'

PRINT 'Starting cleanup...'
PRINT ''

-- Drop foreign key constraints first
IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_PromptGenerationLogs_BCAPBPromptTemplates_PromptTemplateId')
BEGIN
    ALTER TABLE [dbo].[PromptGenerationLogs] 
    DROP CONSTRAINT [FK_PromptGenerationLogs_BCAPBPromptTemplates_PromptTemplateId]
    PRINT 'Foreign key constraint dropped'
END

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

IF EXISTS (SELECT * FROM sysobjects WHERE name='BCAPBPromptTemplates' AND xtype='U')
BEGIN
    DROP TABLE [dbo].[BCAPBPromptTemplates]
    PRINT 'BCAPBPromptTemplates table dropped'
END

IF EXISTS (SELECT * FROM sysobjects WHERE name='IntentTypes' AND xtype='U')
BEGIN
    DROP TABLE [dbo].[IntentTypes]
    PRINT 'IntentTypes table dropped'
END

PRINT ''
PRINT '============================================='
PRINT 'BCAPB CLEANUP COMPLETED!'
PRINT '============================================='
PRINT 'All BCAPB tables have been removed.'
PRINT 'You can run BCAPB_Setup.sql to recreate them.'
PRINT '============================================='
