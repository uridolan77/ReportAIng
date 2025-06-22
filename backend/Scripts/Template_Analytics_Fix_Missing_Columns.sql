-- =====================================================================================
-- TEMPLATE ANALYTICS SYSTEM - FIX MISSING COLUMNS
-- =====================================================================================
-- This script fixes missing columns that may not have been added during migration
-- Run this if you encounter column errors in the validation script
-- 
-- Version: 1.0
-- Date: 2024-12-22
-- =====================================================================================

USE [BIReportingCopilot_Dev]
GO

PRINT '🔧 Fixing Missing Columns in Template Analytics Tables...'
PRINT ''

-- =====================================================================================
-- 1. FIX TEMPLATEIMPROVEMENTSUGGESTIONS TABLE
-- =====================================================================================

PRINT '💡 Checking TemplateImprovementSuggestions table...'

-- Add TemplateKey column if missing
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('TemplateImprovementSuggestions') AND name = 'TemplateKey')
BEGIN
    ALTER TABLE [dbo].[TemplateImprovementSuggestions] 
    ADD [TemplateKey] nvarchar(100) NOT NULL DEFAULT ''
    PRINT '  ✅ Added TemplateKey column'
END
ELSE
    PRINT '  ✅ TemplateKey column already exists'

-- Add TemplateName column if missing
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('TemplateImprovementSuggestions') AND name = 'TemplateName')
BEGIN
    ALTER TABLE [dbo].[TemplateImprovementSuggestions] 
    ADD [TemplateName] nvarchar(200) NOT NULL DEFAULT ''
    PRINT '  ✅ Added TemplateName column'
END
ELSE
    PRINT '  ✅ TemplateName column already exists'

-- Add ImprovementType column if missing
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('TemplateImprovementSuggestions') AND name = 'ImprovementType')
BEGIN
    ALTER TABLE [dbo].[TemplateImprovementSuggestions] 
    ADD [ImprovementType] nvarchar(50) NOT NULL DEFAULT ''
    PRINT '  ✅ Added ImprovementType column'
END
ELSE
    PRINT '  ✅ ImprovementType column already exists'

-- Add ConfidenceScore column if missing
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('TemplateImprovementSuggestions') AND name = 'ConfidenceScore')
BEGIN
    ALTER TABLE [dbo].[TemplateImprovementSuggestions] 
    ADD [ConfidenceScore] decimal(5,4) NULL
    PRINT '  ✅ Added ConfidenceScore column'
END
ELSE
    PRINT '  ✅ ConfidenceScore column already exists'

-- Add ReviewNotes column if missing
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('TemplateImprovementSuggestions') AND name = 'ReviewNotes')
BEGIN
    ALTER TABLE [dbo].[TemplateImprovementSuggestions] 
    ADD [ReviewNotes] nvarchar(2000) NULL
    PRINT '  ✅ Added ReviewNotes column'
END
ELSE
    PRINT '  ✅ ReviewNotes column already exists'

-- Add ImplementedDate column if missing
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('TemplateImprovementSuggestions') AND name = 'ImplementedDate')
BEGIN
    ALTER TABLE [dbo].[TemplateImprovementSuggestions] 
    ADD [ImplementedDate] datetime2(7) NULL
    PRINT '  ✅ Added ImplementedDate column'
END
ELSE
    PRINT '  ✅ ImplementedDate column already exists'

-- Add UpdatedDate column if missing
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('TemplateImprovementSuggestions') AND name = 'UpdatedDate')
BEGIN
    ALTER TABLE [dbo].[TemplateImprovementSuggestions] 
    ADD [UpdatedDate] datetime2(7) NULL
    PRINT '  ✅ Added UpdatedDate column'
END
ELSE
    PRINT '  ✅ UpdatedDate column already exists'

-- Add Title column if missing
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('TemplateImprovementSuggestions') AND name = 'Title')
BEGIN
    ALTER TABLE [dbo].[TemplateImprovementSuggestions] 
    ADD [Title] nvarchar(200) NULL
    PRINT '  ✅ Added Title column'
END
ELSE
    PRINT '  ✅ Title column already exists'

-- Add Description column if missing
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('TemplateImprovementSuggestions') AND name = 'Description')
BEGIN
    ALTER TABLE [dbo].[TemplateImprovementSuggestions] 
    ADD [Description] nvarchar(1000) NULL
    PRINT '  ✅ Added Description column'
END
ELSE
    PRINT '  ✅ Description column already exists'

-- Add Priority column if missing
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('TemplateImprovementSuggestions') AND name = 'Priority')
BEGIN
    ALTER TABLE [dbo].[TemplateImprovementSuggestions] 
    ADD [Priority] int NOT NULL DEFAULT 1
    PRINT '  ✅ Added Priority column'
END
ELSE
    PRINT '  ✅ Priority column already exists'

-- =====================================================================================
-- 2. FIX TEMPLATEPERFORMANCEMETRICS TABLE
-- =====================================================================================

PRINT ''
PRINT '📊 Checking TemplatePerformanceMetrics table...'

-- Add AnalysisDate column if missing
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('TemplatePerformanceMetrics') AND name = 'AnalysisDate')
BEGIN
    ALTER TABLE [dbo].[TemplatePerformanceMetrics] 
    ADD [AnalysisDate] datetime2(7) NOT NULL DEFAULT GETUTCDATE()
    PRINT '  ✅ Added AnalysisDate column'
END
ELSE
    PRINT '  ✅ AnalysisDate column already exists'

-- Add CreatedBy column if missing
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('TemplatePerformanceMetrics') AND name = 'CreatedBy')
BEGIN
    ALTER TABLE [dbo].[TemplatePerformanceMetrics] 
    ADD [CreatedBy] nvarchar(256) NOT NULL DEFAULT 'System'
    PRINT '  ✅ Added CreatedBy column'
END
ELSE
    PRINT '  ✅ CreatedBy column already exists'

-- Add AdditionalMetrics column if missing
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('TemplatePerformanceMetrics') AND name = 'AdditionalMetrics')
BEGIN
    ALTER TABLE [dbo].[TemplatePerformanceMetrics] 
    ADD [AdditionalMetrics] nvarchar(max) NULL
    PRINT '  ✅ Added AdditionalMetrics column'
END
ELSE
    PRINT '  ✅ AdditionalMetrics column already exists'

-- =====================================================================================
-- 3. FIX TEMPLATEABTESTS TABLE
-- =====================================================================================

PRINT ''
PRINT '🧪 Checking TemplateABTests table...'

-- Add UpdatedDate column if missing
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('TemplateABTests') AND name = 'UpdatedDate')
BEGIN
    ALTER TABLE [dbo].[TemplateABTests] 
    ADD [UpdatedDate] datetime2(7) NULL
    PRINT '  ✅ Added UpdatedDate column'
END
ELSE
    PRINT '  ✅ UpdatedDate column already exists'

-- Add ConfidenceLevel column if missing
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('TemplateABTests') AND name = 'ConfidenceLevel')
BEGIN
    ALTER TABLE [dbo].[TemplateABTests] 
    ADD [ConfidenceLevel] decimal(5,4) NULL
    PRINT '  ✅ Added ConfidenceLevel column'
END
ELSE
    PRINT '  ✅ ConfidenceLevel column already exists'

-- Add Conclusion column if missing
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

-- =====================================================================================
-- 5. UPDATE EXISTING DATA WITH MISSING VALUES
-- =====================================================================================

PRINT ''
PRINT '🔄 Updating existing data with missing values...'

-- Update existing data using dynamic SQL to avoid column reference errors
DECLARE @UpdateSQL nvarchar(max)
DECLARE @ColumnExists bit = 0

-- Check if all required columns exist
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('TemplateImprovementSuggestions') AND name = 'TemplateKey')
   AND EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('TemplateImprovementSuggestions') AND name = 'TemplateName')
   AND EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('TemplateImprovementSuggestions') AND name = 'ImprovementType')
   AND EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('TemplateImprovementSuggestions') AND name = 'ConfidenceScore')
   AND EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('TemplateImprovementSuggestions') AND name = 'Priority')
BEGIN
    SET @ColumnExists = 1
END

IF @ColumnExists = 1
BEGIN
    -- Use dynamic SQL to safely update the data
    SET @UpdateSQL = N'
    UPDATE tis
    SET
        TemplateKey = CASE WHEN tis.TemplateKey = '''' OR tis.TemplateKey IS NULL
                          THEN COALESCE(pt.TemplateKey, ''template_'' + CAST(tis.TemplateId AS nvarchar(10)))
                          ELSE tis.TemplateKey END,
        TemplateName = CASE WHEN tis.TemplateName = '''' OR tis.TemplateName IS NULL
                           THEN COALESCE(pt.Name, ''Template '' + CAST(tis.TemplateId AS nvarchar(10)))
                           ELSE tis.TemplateName END,
        ImprovementType = CASE WHEN tis.ImprovementType = '''' OR tis.ImprovementType IS NULL
                              THEN CASE
                                  WHEN tis.SuggestionType = ''performance'' THEN ''performance_optimization''
                                  WHEN tis.SuggestionType = ''accuracy'' THEN ''accuracy_improvement''
                                  ELSE ''general_improvement''
                              END
                              ELSE tis.ImprovementType END,
        ConfidenceScore = CASE WHEN tis.ConfidenceScore IS NULL THEN 0.75 ELSE tis.ConfidenceScore END,
        Priority = CASE WHEN tis.Priority IS NULL OR tis.Priority = 0 THEN 2 ELSE tis.Priority END
    FROM TemplateImprovementSuggestions tis
    LEFT JOIN PromptTemplates pt ON tis.TemplateId = pt.Id'

    EXEC sp_executesql @UpdateSQL

    PRINT '  ✅ Updated TemplateImprovementSuggestions records with missing values'
END
ELSE
BEGIN
    PRINT '  ⚠️  Skipping data update - some required columns not found'
    PRINT '      This is normal if columns were just added and will be populated later'
END

PRINT ''
PRINT '🎉 Column Fix Completed Successfully!'
PRINT ''
PRINT '✅ All missing columns have been added'
PRINT '✅ Indexes have been created'
PRINT '✅ Existing data has been updated'
PRINT ''
PRINT '📝 You can now run the validation script again'
PRINT ''
