-- =====================================================================================
-- TEMPLATE ANALYTICS SYSTEM - QUICK FIX FOR MISSING COLUMNS
-- =====================================================================================
-- This script quickly adds the most critical missing columns
-- Run this if you encounter column errors in the validation script
-- 
-- Version: 1.0
-- Date: 2024-12-22
-- =====================================================================================

USE [BIReportingCopilot_Dev]
GO

PRINT '🔧 Quick Fix: Adding Missing Columns...'
PRINT ''

-- =====================================================================================
-- 1. ADD MISSING COLUMNS TO TEMPLATEIMPROVEMENTSUGGESTIONS
-- =====================================================================================

PRINT '💡 Adding missing columns to TemplateImprovementSuggestions...'

-- Add TemplateKey column
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('TemplateImprovementSuggestions') AND name = 'TemplateKey')
BEGIN
    ALTER TABLE [dbo].[TemplateImprovementSuggestions] 
    ADD [TemplateKey] nvarchar(100) NOT NULL DEFAULT ''
    PRINT '  ✅ Added TemplateKey column'
END

-- Add TemplateName column
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('TemplateImprovementSuggestions') AND name = 'TemplateName')
BEGIN
    ALTER TABLE [dbo].[TemplateImprovementSuggestions] 
    ADD [TemplateName] nvarchar(200) NOT NULL DEFAULT ''
    PRINT '  ✅ Added TemplateName column'
END

-- Add ImprovementType column
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('TemplateImprovementSuggestions') AND name = 'ImprovementType')
BEGIN
    ALTER TABLE [dbo].[TemplateImprovementSuggestions] 
    ADD [ImprovementType] nvarchar(50) NOT NULL DEFAULT ''
    PRINT '  ✅ Added ImprovementType column'
END

-- Add ConfidenceScore column
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('TemplateImprovementSuggestions') AND name = 'ConfidenceScore')
BEGIN
    ALTER TABLE [dbo].[TemplateImprovementSuggestions] 
    ADD [ConfidenceScore] decimal(5,4) NULL
    PRINT '  ✅ Added ConfidenceScore column'
END

-- Add UpdatedDate column
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('TemplateImprovementSuggestions') AND name = 'UpdatedDate')
BEGIN
    ALTER TABLE [dbo].[TemplateImprovementSuggestions] 
    ADD [UpdatedDate] datetime2(7) NULL
    PRINT '  ✅ Added UpdatedDate column'
END

-- Add Priority column
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('TemplateImprovementSuggestions') AND name = 'Priority')
BEGIN
    ALTER TABLE [dbo].[TemplateImprovementSuggestions] 
    ADD [Priority] int NOT NULL DEFAULT 1
    PRINT '  ✅ Added Priority column'
END

-- Add Title column
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('TemplateImprovementSuggestions') AND name = 'Title')
BEGIN
    ALTER TABLE [dbo].[TemplateImprovementSuggestions] 
    ADD [Title] nvarchar(200) NULL
    PRINT '  ✅ Added Title column'
END

-- Add Description column
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('TemplateImprovementSuggestions') AND name = 'Description')
BEGIN
    ALTER TABLE [dbo].[TemplateImprovementSuggestions] 
    ADD [Description] nvarchar(1000) NULL
    PRINT '  ✅ Added Description column'
END

-- =====================================================================================
-- 2. ADD MISSING COLUMNS TO TEMPLATEPERFORMANCEMETRICS
-- =====================================================================================

PRINT ''
PRINT '📊 Adding missing columns to TemplatePerformanceMetrics...'

-- Add AnalysisDate column
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('TemplatePerformanceMetrics') AND name = 'AnalysisDate')
BEGIN
    ALTER TABLE [dbo].[TemplatePerformanceMetrics] 
    ADD [AnalysisDate] datetime2(7) NOT NULL DEFAULT GETUTCDATE()
    PRINT '  ✅ Added AnalysisDate column'
END

-- Add CreatedBy column
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('TemplatePerformanceMetrics') AND name = 'CreatedBy')
BEGIN
    ALTER TABLE [dbo].[TemplatePerformanceMetrics] 
    ADD [CreatedBy] nvarchar(256) NOT NULL DEFAULT 'System'
    PRINT '  ✅ Added CreatedBy column'
END

-- Add AdditionalMetrics column
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('TemplatePerformanceMetrics') AND name = 'AdditionalMetrics')
BEGIN
    ALTER TABLE [dbo].[TemplatePerformanceMetrics] 
    ADD [AdditionalMetrics] nvarchar(max) NULL
    PRINT '  ✅ Added AdditionalMetrics column'
END

-- =====================================================================================
-- 3. ADD MISSING COLUMNS TO TEMPLATEABTESTS
-- =====================================================================================

PRINT ''
PRINT '🧪 Adding missing columns to TemplateABTests...'

-- Add UpdatedDate column
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('TemplateABTests') AND name = 'UpdatedDate')
BEGIN
    ALTER TABLE [dbo].[TemplateABTests] 
    ADD [UpdatedDate] datetime2(7) NULL
    PRINT '  ✅ Added UpdatedDate column'
END

-- Add ConfidenceLevel column
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('TemplateABTests') AND name = 'ConfidenceLevel')
BEGIN
    ALTER TABLE [dbo].[TemplateABTests] 
    ADD [ConfidenceLevel] decimal(5,4) NULL
    PRINT '  ✅ Added ConfidenceLevel column'
END

-- Add Conclusion column
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('TemplateABTests') AND name = 'Conclusion')
BEGIN
    ALTER TABLE [dbo].[TemplateABTests] 
    ADD [Conclusion] nvarchar(1000) NULL
    PRINT '  ✅ Added Conclusion column'
END

-- =====================================================================================
-- 4. UPDATE EXISTING DATA (SAFELY)
-- =====================================================================================

PRINT ''
PRINT '🔄 Updating existing data with default values...'

-- Update TemplateKey for existing records (only if column exists and is empty)
DECLARE @UpdateSQL nvarchar(max)

-- Check if we can safely update TemplateKey
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('TemplateImprovementSuggestions') AND name = 'TemplateKey')
BEGIN
    -- Update empty TemplateKey values
    UPDATE TemplateImprovementSuggestions 
    SET TemplateKey = 'template_' + CAST(TemplateId AS nvarchar(10))
    WHERE TemplateKey = '' OR TemplateKey IS NULL
    
    PRINT '  ✅ Updated TemplateKey values for existing records'
END

-- Check if we can safely update TemplateName
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('TemplateImprovementSuggestions') AND name = 'TemplateName')
BEGIN
    -- Update empty TemplateName values
    UPDATE TemplateImprovementSuggestions 
    SET TemplateName = 'Template ' + CAST(TemplateId AS nvarchar(10))
    WHERE TemplateName = '' OR TemplateName IS NULL
    
    PRINT '  ✅ Updated TemplateName values for existing records'
END

-- Check if we can safely update ImprovementType
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('TemplateImprovementSuggestions') AND name = 'ImprovementType')
BEGIN
    -- Update empty ImprovementType values based on SuggestionType
    UPDATE TemplateImprovementSuggestions 
    SET ImprovementType = CASE 
        WHEN SuggestionType = 'performance' THEN 'performance_optimization'
        WHEN SuggestionType = 'accuracy' THEN 'accuracy_improvement'
        ELSE 'general_improvement'
    END
    WHERE ImprovementType = '' OR ImprovementType IS NULL
    
    PRINT '  ✅ Updated ImprovementType values for existing records'
END

-- Check if we can safely update ConfidenceScore
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('TemplateImprovementSuggestions') AND name = 'ConfidenceScore')
BEGIN
    -- Update NULL ConfidenceScore values
    UPDATE TemplateImprovementSuggestions 
    SET ConfidenceScore = 0.75
    WHERE ConfidenceScore IS NULL
    
    PRINT '  ✅ Updated ConfidenceScore values for existing records'
END

-- =====================================================================================
-- 5. ADD CRITICAL INDEXES
-- =====================================================================================

PRINT ''
PRINT '📇 Adding critical indexes...'

-- Add TemplateKey index for TemplateImprovementSuggestions
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID('TemplateImprovementSuggestions') AND name = 'IX_TemplateImprovementSuggestions_TemplateKey')
BEGIN
    CREATE INDEX [IX_TemplateImprovementSuggestions_TemplateKey] ON [dbo].[TemplateImprovementSuggestions] ([TemplateKey])
    PRINT '  ✅ Added TemplateKey index'
END

-- Add TemplateKey index for TemplatePerformanceMetrics
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID('TemplatePerformanceMetrics') AND name = 'IX_TemplatePerformanceMetrics_TemplateKey')
BEGIN
    CREATE INDEX [IX_TemplatePerformanceMetrics_TemplateKey] ON [dbo].[TemplatePerformanceMetrics] ([TemplateKey])
    PRINT '  ✅ Added TemplateKey index'
END

PRINT ''
PRINT '🎉 Quick Fix Completed Successfully!'
PRINT ''
PRINT '✅ All critical missing columns have been added'
PRINT '✅ Existing data has been updated with default values'
PRINT '✅ Critical indexes have been created'
PRINT ''
PRINT '📝 You can now run the validation script again'
PRINT ''
