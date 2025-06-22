-- =====================================================================================
-- TEMPLATE ANALYTICS SYSTEM - SAFE COLUMN FIX
-- =====================================================================================
-- This script safely adds missing columns without trying to update existing data
-- Run this if you encounter column errors in the validation script
-- 
-- Version: 1.0
-- Date: 2024-12-22
-- =====================================================================================

USE [BIReportingCopilot_Dev]
GO

PRINT '🔧 Safe Fix: Adding Missing Columns Only...'
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
ELSE
    PRINT '  ✅ TemplateKey column already exists'

-- Add TemplateName column
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('TemplateImprovementSuggestions') AND name = 'TemplateName')
BEGIN
    ALTER TABLE [dbo].[TemplateImprovementSuggestions] 
    ADD [TemplateName] nvarchar(200) NOT NULL DEFAULT ''
    PRINT '  ✅ Added TemplateName column'
END
ELSE
    PRINT '  ✅ TemplateName column already exists'

-- Add ImprovementType column
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('TemplateImprovementSuggestions') AND name = 'ImprovementType')
BEGIN
    ALTER TABLE [dbo].[TemplateImprovementSuggestions] 
    ADD [ImprovementType] nvarchar(50) NOT NULL DEFAULT ''
    PRINT '  ✅ Added ImprovementType column'
END
ELSE
    PRINT '  ✅ ImprovementType column already exists'

-- Add ConfidenceScore column
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('TemplateImprovementSuggestions') AND name = 'ConfidenceScore')
BEGIN
    ALTER TABLE [dbo].[TemplateImprovementSuggestions] 
    ADD [ConfidenceScore] decimal(5,4) NULL
    PRINT '  ✅ Added ConfidenceScore column'
END
ELSE
    PRINT '  ✅ ConfidenceScore column already exists'

-- Add ReviewNotes column
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('TemplateImprovementSuggestions') AND name = 'ReviewNotes')
BEGIN
    ALTER TABLE [dbo].[TemplateImprovementSuggestions] 
    ADD [ReviewNotes] nvarchar(2000) NULL
    PRINT '  ✅ Added ReviewNotes column'
END
ELSE
    PRINT '  ✅ ReviewNotes column already exists'

-- Add ImplementedDate column
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('TemplateImprovementSuggestions') AND name = 'ImplementedDate')
BEGIN
    ALTER TABLE [dbo].[TemplateImprovementSuggestions] 
    ADD [ImplementedDate] datetime2(7) NULL
    PRINT '  ✅ Added ImplementedDate column'
END
ELSE
    PRINT '  ✅ ImplementedDate column already exists'

-- Add UpdatedDate column
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('TemplateImprovementSuggestions') AND name = 'UpdatedDate')
BEGIN
    ALTER TABLE [dbo].[TemplateImprovementSuggestions] 
    ADD [UpdatedDate] datetime2(7) NULL
    PRINT '  ✅ Added UpdatedDate column'
END
ELSE
    PRINT '  ✅ UpdatedDate column already exists'

-- Add Title column
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('TemplateImprovementSuggestions') AND name = 'Title')
BEGIN
    ALTER TABLE [dbo].[TemplateImprovementSuggestions] 
    ADD [Title] nvarchar(200) NULL
    PRINT '  ✅ Added Title column'
END
ELSE
    PRINT '  ✅ Title column already exists'

-- Add Description column
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('TemplateImprovementSuggestions') AND name = 'Description')
BEGIN
    ALTER TABLE [dbo].[TemplateImprovementSuggestions] 
    ADD [Description] nvarchar(1000) NULL
    PRINT '  ✅ Added Description column'
END
ELSE
    PRINT '  ✅ Description column already exists'

-- Add Priority column
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('TemplateImprovementSuggestions') AND name = 'Priority')
BEGIN
    ALTER TABLE [dbo].[TemplateImprovementSuggestions] 
    ADD [Priority] int NOT NULL DEFAULT 1
    PRINT '  ✅ Added Priority column'
END
ELSE
    PRINT '  ✅ Priority column already exists'

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
ELSE
    PRINT '  ✅ AnalysisDate column already exists'

-- Add CreatedBy column
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('TemplatePerformanceMetrics') AND name = 'CreatedBy')
BEGIN
    ALTER TABLE [dbo].[TemplatePerformanceMetrics] 
    ADD [CreatedBy] nvarchar(256) NOT NULL DEFAULT 'System'
    PRINT '  ✅ Added CreatedBy column'
END
ELSE
    PRINT '  ✅ CreatedBy column already exists'

-- Add AdditionalMetrics column
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('TemplatePerformanceMetrics') AND name = 'AdditionalMetrics')
BEGIN
    ALTER TABLE [dbo].[TemplatePerformanceMetrics] 
    ADD [AdditionalMetrics] nvarchar(max) NULL
    PRINT '  ✅ Added AdditionalMetrics column'
END
ELSE
    PRINT '  ✅ AdditionalMetrics column already exists'

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
ELSE
    PRINT '  ✅ UpdatedDate column already exists'

-- Add ConfidenceLevel column
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('TemplateABTests') AND name = 'ConfidenceLevel')
BEGIN
    ALTER TABLE [dbo].[TemplateABTests] 
    ADD [ConfidenceLevel] decimal(5,4) NULL
    PRINT '  ✅ Added ConfidenceLevel column'
END
ELSE
    PRINT '  ✅ ConfidenceLevel column already exists'

-- Add Conclusion column
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('TemplateABTests') AND name = 'Conclusion')
BEGIN
    ALTER TABLE [dbo].[TemplateABTests] 
    ADD [Conclusion] nvarchar(1000) NULL
    PRINT '  ✅ Added Conclusion column'
END
ELSE
    PRINT '  ✅ Conclusion column already exists'

-- =====================================================================================
-- 4. ADD MISSING INDEXES
-- =====================================================================================

PRINT ''
PRINT '📇 Adding missing indexes...'

-- Add TemplateKey index for TemplateImprovementSuggestions
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID('TemplateImprovementSuggestions') AND name = 'IX_TemplateImprovementSuggestions_TemplateKey')
BEGIN
    CREATE INDEX [IX_TemplateImprovementSuggestions_TemplateKey] ON [dbo].[TemplateImprovementSuggestions] ([TemplateKey])
    PRINT '  ✅ Added TemplateKey index for TemplateImprovementSuggestions'
END
ELSE
    PRINT '  ✅ TemplateKey index already exists'

-- Add ImprovementType index for TemplateImprovementSuggestions
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID('TemplateImprovementSuggestions') AND name = 'IX_TemplateImprovementSuggestions_ImprovementType')
BEGIN
    CREATE INDEX [IX_TemplateImprovementSuggestions_ImprovementType] ON [dbo].[TemplateImprovementSuggestions] ([ImprovementType])
    PRINT '  ✅ Added ImprovementType index for TemplateImprovementSuggestions'
END
ELSE
    PRINT '  ✅ ImprovementType index already exists'

-- Add TemplateKey index for TemplatePerformanceMetrics
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID('TemplatePerformanceMetrics') AND name = 'IX_TemplatePerformanceMetrics_TemplateKey')
BEGIN
    CREATE INDEX [IX_TemplatePerformanceMetrics_TemplateKey] ON [dbo].[TemplatePerformanceMetrics] ([TemplateKey])
    PRINT '  ✅ Added TemplateKey index for TemplatePerformanceMetrics'
END
ELSE
    PRINT '  ✅ TemplateKey index already exists'

-- Add AnalysisDate index for TemplatePerformanceMetrics
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID('TemplatePerformanceMetrics') AND name = 'IX_TemplatePerformanceMetrics_AnalysisDate')
BEGIN
    CREATE INDEX [IX_TemplatePerformanceMetrics_AnalysisDate] ON [dbo].[TemplatePerformanceMetrics] ([AnalysisDate])
    PRINT '  ✅ Added AnalysisDate index for TemplatePerformanceMetrics'
END
ELSE
    PRINT '  ✅ AnalysisDate index already exists'

-- Add TestName index for TemplateABTests
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID('TemplateABTests') AND name = 'IX_TemplateABTests_TestName')
BEGIN
    CREATE INDEX [IX_TemplateABTests_TestName] ON [dbo].[TemplateABTests] ([TestName])
    PRINT '  ✅ Added TestName index for TemplateABTests'
END
ELSE
    PRINT '  ✅ TestName index already exists'

PRINT ''
PRINT '🎉 Safe Column Fix Completed Successfully!'
PRINT ''
PRINT '✅ All missing columns have been added'
PRINT '✅ All missing indexes have been created'
PRINT '✅ No data updates attempted (safer approach)'
PRINT ''
PRINT '📝 Next steps:'
PRINT '   1. Run the validation script to verify installation'
PRINT '   2. The API will populate data automatically during normal operation'
PRINT '   3. You can manually populate test data if needed'
PRINT ''
