-- =====================================================================================
-- TEMPLATE ANALYTICS SYSTEM - COMPLETE MIGRATION SCRIPT
-- =====================================================================================
-- This script creates all necessary tables, columns, indexes, and seed data
-- for the Template Analytics System including Performance Metrics, A/B Testing,
-- and Template Improvement features.
-- 
-- Version: 1.0
-- Date: 2024-12-22
-- =====================================================================================

USE [BIReportingCopilot_Dev]
GO

PRINT '🚀 Starting Template Analytics System Migration...'
PRINT ''

-- =====================================================================================
-- 1. UPDATE EXISTING TEMPLATE PERFORMANCE METRICS TABLE
-- =====================================================================================

PRINT '📊 Updating TemplatePerformanceMetrics table...'

-- Check if table exists and add missing columns
IF EXISTS (SELECT * FROM sysobjects WHERE name='TemplatePerformanceMetrics' AND xtype='U')
BEGIN
    -- Add missing columns that are in the entity but not in the existing schema
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('TemplatePerformanceMetrics') AND name = 'AnalysisDate')
    BEGIN
        ALTER TABLE [dbo].[TemplatePerformanceMetrics] 
        ADD [AnalysisDate] datetime2(7) NOT NULL DEFAULT GETUTCDATE()
        PRINT '  ✅ Added AnalysisDate column'
    END

    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('TemplatePerformanceMetrics') AND name = 'CreatedBy')
    BEGIN
        ALTER TABLE [dbo].[TemplatePerformanceMetrics] 
        ADD [CreatedBy] nvarchar(256) NOT NULL DEFAULT 'System'
        PRINT '  ✅ Added CreatedBy column'
    END

    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('TemplatePerformanceMetrics') AND name = 'AdditionalMetrics')
    BEGIN
        ALTER TABLE [dbo].[TemplatePerformanceMetrics] 
        ADD [AdditionalMetrics] nvarchar(max) NULL
        PRINT '  ✅ Added AdditionalMetrics column (JSON)'
    END

    -- Add missing indexes
    IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID('TemplatePerformanceMetrics') AND name = 'IX_TemplatePerformanceMetrics_TemplateKey')
    BEGIN
        CREATE INDEX [IX_TemplatePerformanceMetrics_TemplateKey] ON [dbo].[TemplatePerformanceMetrics] ([TemplateKey])
        PRINT '  ✅ Added TemplateKey index'
    END

    IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID('TemplatePerformanceMetrics') AND name = 'IX_TemplatePerformanceMetrics_AnalysisDate')
    BEGIN
        CREATE INDEX [IX_TemplatePerformanceMetrics_AnalysisDate] ON [dbo].[TemplatePerformanceMetrics] ([AnalysisDate])
        PRINT '  ✅ Added AnalysisDate index'
    END
END
ELSE
BEGIN
    -- Create the table if it doesn't exist
    CREATE TABLE [dbo].[TemplatePerformanceMetrics] (
        [Id] bigint IDENTITY(1,1) NOT NULL,
        [TemplateId] bigint NOT NULL,
        [TemplateKey] nvarchar(100) NOT NULL,
        [IntentType] nvarchar(50) NOT NULL,
        [TotalUsages] int NOT NULL DEFAULT 0,
        [SuccessfulUsages] int NOT NULL DEFAULT 0,
        [SuccessRate] decimal(5,4) NOT NULL DEFAULT 0,
        [AverageConfidenceScore] decimal(5,4) NOT NULL DEFAULT 0,
        [AverageProcessingTimeMs] int NOT NULL DEFAULT 0,
        [AverageUserRating] decimal(3,2) NULL,
        [LastUsedDate] datetime2(7) NULL,
        [AnalysisDate] datetime2(7) NOT NULL DEFAULT GETUTCDATE(),
        [CreatedBy] nvarchar(256) NOT NULL DEFAULT 'System',
        [CreatedDate] datetime2(7) NOT NULL DEFAULT GETUTCDATE(),
        [UpdatedDate] datetime2(7) NOT NULL DEFAULT GETUTCDATE(),
        [AdditionalMetrics] nvarchar(max) NULL,
        CONSTRAINT [PK_TemplatePerformanceMetrics] PRIMARY KEY CLUSTERED ([Id] ASC)
    )
    
    CREATE INDEX [IX_TemplatePerformanceMetrics_TemplateId] ON [dbo].[TemplatePerformanceMetrics] ([TemplateId])
    CREATE INDEX [IX_TemplatePerformanceMetrics_TemplateKey] ON [dbo].[TemplatePerformanceMetrics] ([TemplateKey])
    CREATE INDEX [IX_TemplatePerformanceMetrics_IntentType] ON [dbo].[TemplatePerformanceMetrics] ([IntentType])
    CREATE INDEX [IX_TemplatePerformanceMetrics_AnalysisDate] ON [dbo].[TemplatePerformanceMetrics] ([AnalysisDate])
    PRINT '  ✅ Created TemplatePerformanceMetrics table'
END

-- =====================================================================================
-- 2. UPDATE EXISTING TEMPLATE A/B TESTS TABLE
-- =====================================================================================

PRINT '🧪 Updating TemplateABTests table...'

IF EXISTS (SELECT * FROM sysobjects WHERE name='TemplateABTests' AND xtype='U')
BEGIN
    -- Add missing columns for enhanced A/B testing
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('TemplateABTests') AND name = 'UpdatedDate')
    BEGIN
        ALTER TABLE [dbo].[TemplateABTests] 
        ADD [UpdatedDate] datetime2(7) NULL
        PRINT '  ✅ Added UpdatedDate column'
    END

    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('TemplateABTests') AND name = 'ConfidenceLevel')
    BEGIN
        ALTER TABLE [dbo].[TemplateABTests] 
        ADD [ConfidenceLevel] decimal(5,4) NULL
        PRINT '  ✅ Added ConfidenceLevel column'
    END

    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('TemplateABTests') AND name = 'Conclusion')
    BEGIN
        ALTER TABLE [dbo].[TemplateABTests] 
        ADD [Conclusion] nvarchar(1000) NULL
        PRINT '  ✅ Added Conclusion column'
    END

    -- Add missing indexes
    IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID('TemplateABTests') AND name = 'IX_TemplateABTests_TestName')
    BEGIN
        CREATE INDEX [IX_TemplateABTests_TestName] ON [dbo].[TemplateABTests] ([TestName])
        PRINT '  ✅ Added TestName index'
    END
END
ELSE
BEGIN
    -- Create the table if it doesn't exist
    CREATE TABLE [dbo].[TemplateABTests] (
        [Id] bigint IDENTITY(1,1) NOT NULL,
        [TestName] nvarchar(100) NOT NULL,
        [OriginalTemplateId] bigint NOT NULL,
        [VariantTemplateId] bigint NOT NULL,
        [TrafficSplitPercent] int NOT NULL DEFAULT 50,
        [StartDate] datetime2(7) NOT NULL,
        [EndDate] datetime2(7) NULL,
        [Status] nvarchar(20) NOT NULL DEFAULT 'active',
        [OriginalSuccessRate] decimal(5,4) NULL,
        [VariantSuccessRate] decimal(5,4) NULL,
        [StatisticalSignificance] decimal(5,4) NULL,
        [WinnerTemplateId] bigint NULL,
        [TestResults] nvarchar(max) NULL,
        [CompletedDate] datetime2(7) NULL,
        [CreatedBy] nvarchar(512) NOT NULL,
        [CreatedDate] datetime2(7) NOT NULL DEFAULT GETUTCDATE(),
        [UpdatedDate] datetime2(7) NULL,
        [ConfidenceLevel] decimal(5,4) NULL,
        [Conclusion] nvarchar(1000) NULL,
        CONSTRAINT [PK_TemplateABTests] PRIMARY KEY CLUSTERED ([Id] ASC)
    )
    
    CREATE INDEX [IX_TemplateABTests_TestName] ON [dbo].[TemplateABTests] ([TestName])
    CREATE INDEX [IX_TemplateABTests_Status] ON [dbo].[TemplateABTests] ([Status])
    CREATE INDEX [IX_TemplateABTests_StartDate] ON [dbo].[TemplateABTests] ([StartDate])
    PRINT '  ✅ Created TemplateABTests table'
END

-- =====================================================================================
-- 3. UPDATE TEMPLATE IMPROVEMENT SUGGESTIONS TABLE
-- =====================================================================================

PRINT '💡 Updating TemplateImprovementSuggestions table...'

IF EXISTS (SELECT * FROM sysobjects WHERE name='TemplateImprovementSuggestions' AND xtype='U')
BEGIN
    -- Add missing columns for enhanced improvement tracking
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('TemplateImprovementSuggestions') AND name = 'UpdatedDate')
    BEGIN
        ALTER TABLE [dbo].[TemplateImprovementSuggestions] 
        ADD [UpdatedDate] datetime2(7) NULL
        PRINT '  ✅ Added UpdatedDate column'
    END

    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('TemplateImprovementSuggestions') AND name = 'TemplateKey')
    BEGIN
        ALTER TABLE [dbo].[TemplateImprovementSuggestions] 
        ADD [TemplateKey] nvarchar(100) NOT NULL DEFAULT ''
        PRINT '  ✅ Added TemplateKey column'
    END

    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('TemplateImprovementSuggestions') AND name = 'TemplateName')
    BEGIN
        ALTER TABLE [dbo].[TemplateImprovementSuggestions] 
        ADD [TemplateName] nvarchar(200) NOT NULL DEFAULT ''
        PRINT '  ✅ Added TemplateName column'
    END

    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('TemplateImprovementSuggestions') AND name = 'ImprovementType')
    BEGIN
        ALTER TABLE [dbo].[TemplateImprovementSuggestions] 
        ADD [ImprovementType] nvarchar(50) NOT NULL DEFAULT ''
        PRINT '  ✅ Added ImprovementType column'
    END

    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('TemplateImprovementSuggestions') AND name = 'ConfidenceScore')
    BEGIN
        ALTER TABLE [dbo].[TemplateImprovementSuggestions] 
        ADD [ConfidenceScore] decimal(5,4) NULL
        PRINT '  ✅ Added ConfidenceScore column'
    END

    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('TemplateImprovementSuggestions') AND name = 'ReviewNotes')
    BEGIN
        ALTER TABLE [dbo].[TemplateImprovementSuggestions] 
        ADD [ReviewNotes] nvarchar(2000) NULL
        PRINT '  ✅ Added ReviewNotes column'
    END

    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('TemplateImprovementSuggestions') AND name = 'ImplementedDate')
    BEGIN
        ALTER TABLE [dbo].[TemplateImprovementSuggestions] 
        ADD [ImplementedDate] datetime2(7) NULL
        PRINT '  ✅ Added ImplementedDate column'
    END

    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('TemplateImprovementSuggestions') AND name = 'Title')
    BEGIN
        ALTER TABLE [dbo].[TemplateImprovementSuggestions] 
        ADD [Title] nvarchar(200) NULL
        PRINT '  ✅ Added Title column'
    END

    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('TemplateImprovementSuggestions') AND name = 'Description')
    BEGIN
        ALTER TABLE [dbo].[TemplateImprovementSuggestions] 
        ADD [Description] nvarchar(1000) NULL
        PRINT '  ✅ Added Description column'
    END

    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('TemplateImprovementSuggestions') AND name = 'Priority')
    BEGIN
        ALTER TABLE [dbo].[TemplateImprovementSuggestions] 
        ADD [Priority] int NOT NULL DEFAULT 1
        PRINT '  ✅ Added Priority column'
    END

    -- Add missing indexes
    IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID('TemplateImprovementSuggestions') AND name = 'IX_TemplateImprovementSuggestions_TemplateKey')
    BEGIN
        CREATE INDEX [IX_TemplateImprovementSuggestions_TemplateKey] ON [dbo].[TemplateImprovementSuggestions] ([TemplateKey])
        PRINT '  ✅ Added TemplateKey index'
    END

    IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID('TemplateImprovementSuggestions') AND name = 'IX_TemplateImprovementSuggestions_ImprovementType')
    BEGIN
        CREATE INDEX [IX_TemplateImprovementSuggestions_ImprovementType] ON [dbo].[TemplateImprovementSuggestions] ([ImprovementType])
        PRINT '  ✅ Added ImprovementType index'
    END
END
ELSE
BEGIN
    -- Create the table if it doesn't exist with all columns
    CREATE TABLE [dbo].[TemplateImprovementSuggestions] (
        [Id] bigint IDENTITY(1,1) NOT NULL,
        [TemplateId] bigint NOT NULL,
        [SuggestionType] nvarchar(50) NOT NULL,
        [CurrentVersion] nvarchar(20) NOT NULL,
        [SuggestedChanges] nvarchar(max) NOT NULL,
        [ReasoningExplanation] nvarchar(2000) NOT NULL,
        [ExpectedImprovementPercent] decimal(5,2) NULL,
        [BasedOnDataPoints] int NOT NULL,
        [Status] nvarchar(20) NOT NULL DEFAULT 'pending',
        [ReviewedBy] nvarchar(512) NULL,
        [ReviewedDate] datetime2(7) NULL,
        [ReviewComments] nvarchar(1000) NULL,
        [CreatedBy] nvarchar(512) NOT NULL,
        [CreatedDate] datetime2(7) NOT NULL DEFAULT GETUTCDATE(),
        [UpdatedDate] datetime2(7) NULL,
        [TemplateKey] nvarchar(100) NOT NULL DEFAULT '',
        [TemplateName] nvarchar(200) NOT NULL DEFAULT '',
        [ImprovementType] nvarchar(50) NOT NULL DEFAULT '',
        [ConfidenceScore] decimal(5,4) NULL,
        [ReviewNotes] nvarchar(2000) NULL,
        [ImplementedDate] datetime2(7) NULL,
        [Title] nvarchar(200) NULL,
        [Description] nvarchar(1000) NULL,
        [Priority] int NOT NULL DEFAULT 1,
        CONSTRAINT [PK_TemplateImprovementSuggestions] PRIMARY KEY CLUSTERED ([Id] ASC)
    )
    
    CREATE INDEX [IX_TemplateImprovementSuggestions_TemplateId] ON [dbo].[TemplateImprovementSuggestions] ([TemplateId])
    CREATE INDEX [IX_TemplateImprovementSuggestions_TemplateKey] ON [dbo].[TemplateImprovementSuggestions] ([TemplateKey])
    CREATE INDEX [IX_TemplateImprovementSuggestions_Status] ON [dbo].[TemplateImprovementSuggestions] ([Status])
    CREATE INDEX [IX_TemplateImprovementSuggestions_SuggestionType] ON [dbo].[TemplateImprovementSuggestions] ([SuggestionType])
    CREATE INDEX [IX_TemplateImprovementSuggestions_ImprovementType] ON [dbo].[TemplateImprovementSuggestions] ([ImprovementType])
    PRINT '  ✅ Created TemplateImprovementSuggestions table'
END

-- =====================================================================================
-- 4. ADD FOREIGN KEY CONSTRAINTS (IF NOT EXISTS)
-- =====================================================================================

PRINT '🔗 Adding Foreign Key Constraints...'

-- Add FK for TemplatePerformanceMetrics to PromptTemplates
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_TemplatePerformanceMetrics_PromptTemplates')
BEGIN
    IF EXISTS (SELECT * FROM sysobjects WHERE name='PromptTemplates' AND xtype='U')
    BEGIN
        ALTER TABLE [dbo].[TemplatePerformanceMetrics]
        ADD CONSTRAINT [FK_TemplatePerformanceMetrics_PromptTemplates]
        FOREIGN KEY ([TemplateId]) REFERENCES [dbo].[PromptTemplates] ([Id])
        PRINT '  ✅ Added FK: TemplatePerformanceMetrics -> PromptTemplates'
    END
    ELSE
    BEGIN
        PRINT '  ⚠️  Warning: PromptTemplates table not found, skipping FK constraint'
    END
END

-- Add FK for TemplateABTests to PromptTemplates
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_TemplateABTests_OriginalTemplate')
BEGIN
    IF EXISTS (SELECT * FROM sysobjects WHERE name='PromptTemplates' AND xtype='U')
    BEGIN
        ALTER TABLE [dbo].[TemplateABTests]
        ADD CONSTRAINT [FK_TemplateABTests_OriginalTemplate]
        FOREIGN KEY ([OriginalTemplateId]) REFERENCES [dbo].[PromptTemplates] ([Id])
        PRINT '  ✅ Added FK: TemplateABTests -> PromptTemplates (Original)'
    END
END

IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_TemplateABTests_VariantTemplate')
BEGIN
    IF EXISTS (SELECT * FROM sysobjects WHERE name='PromptTemplates' AND xtype='U')
    BEGIN
        ALTER TABLE [dbo].[TemplateABTests]
        ADD CONSTRAINT [FK_TemplateABTests_VariantTemplate]
        FOREIGN KEY ([VariantTemplateId]) REFERENCES [dbo].[PromptTemplates] ([Id])
        PRINT '  ✅ Added FK: TemplateABTests -> PromptTemplates (Variant)'
    END
END

-- Add FK for TemplateImprovementSuggestions to PromptTemplates
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_TemplateImprovementSuggestions_PromptTemplates')
BEGIN
    IF EXISTS (SELECT * FROM sysobjects WHERE name='PromptTemplates' AND xtype='U')
    BEGIN
        ALTER TABLE [dbo].[TemplateImprovementSuggestions]
        ADD CONSTRAINT [FK_TemplateImprovementSuggestions_PromptTemplates]
        FOREIGN KEY ([TemplateId]) REFERENCES [dbo].[PromptTemplates] ([Id])
        PRINT '  ✅ Added FK: TemplateImprovementSuggestions -> PromptTemplates'
    END
END

-- =====================================================================================
-- 5. SEED DATA FOR TEMPLATE ANALYTICS
-- =====================================================================================

PRINT '🌱 Inserting Seed Data...'

-- Insert sample template performance metrics (if PromptTemplates exist)
IF EXISTS (SELECT * FROM sysobjects WHERE name='PromptTemplates' AND xtype='U')
BEGIN
    -- Check if we have any templates to work with
    DECLARE @TemplateCount int
    SELECT @TemplateCount = COUNT(*) FROM [dbo].[PromptTemplates]

    IF @TemplateCount > 0
    BEGIN
        -- Insert sample performance metrics for existing templates
        INSERT INTO [dbo].[TemplatePerformanceMetrics]
        ([TemplateId], [TemplateKey], [IntentType], [TotalUsages], [SuccessfulUsages], [SuccessRate],
         [AverageConfidenceScore], [AverageProcessingTimeMs], [AverageUserRating], [LastUsedDate],
         [AnalysisDate], [CreatedBy], [AdditionalMetrics])
        SELECT TOP 5
            pt.Id,
            pt.TemplateKey,
            COALESCE(pt.IntentType, 'general') as IntentType,
            CAST(RAND(CHECKSUM(NEWID())) * 1000 + 100 AS int) as TotalUsages,
            CAST(RAND(CHECKSUM(NEWID())) * 800 + 80 AS int) as SuccessfulUsages,
            CAST(RAND(CHECKSUM(NEWID())) * 0.3 + 0.7 AS decimal(5,4)) as SuccessRate,
            CAST(RAND(CHECKSUM(NEWID())) * 0.2 + 0.8 AS decimal(5,4)) as AverageConfidenceScore,
            CAST(RAND(CHECKSUM(NEWID())) * 500 + 200 AS int) as AverageProcessingTimeMs,
            CAST(RAND(CHECKSUM(NEWID())) * 2 + 3 AS decimal(3,2)) as AverageUserRating,
            DATEADD(day, -CAST(RAND(CHECKSUM(NEWID())) * 30 AS int), GETUTCDATE()) as LastUsedDate,
            GETUTCDATE() as AnalysisDate,
            'System' as CreatedBy,
            '{"responseQuality": 0.85, "userSatisfaction": 0.78, "processingEfficiency": 0.92}' as AdditionalMetrics
        FROM [dbo].[PromptTemplates] pt
        WHERE NOT EXISTS (
            SELECT 1 FROM [dbo].[TemplatePerformanceMetrics] tpm
            WHERE tpm.TemplateId = pt.Id
        )

        PRINT '  ✅ Inserted sample performance metrics'
    END
END

-- Insert sample A/B test data
IF EXISTS (SELECT * FROM sysobjects WHERE name='PromptTemplates' AND xtype='U')
BEGIN
    DECLARE @Template1Id bigint, @Template2Id bigint

    -- Get first two templates for A/B testing
    SELECT TOP 1 @Template1Id = Id FROM [dbo].[PromptTemplates] ORDER BY Id
    SELECT TOP 1 @Template2Id = Id FROM [dbo].[PromptTemplates] WHERE Id > @Template1Id ORDER BY Id

    IF @Template1Id IS NOT NULL AND @Template2Id IS NOT NULL
    BEGIN
        INSERT INTO [dbo].[TemplateABTests]
        ([TestName], [OriginalTemplateId], [VariantTemplateId], [TrafficSplitPercent],
         [StartDate], [EndDate], [Status], [OriginalSuccessRate], [VariantSuccessRate],
         [StatisticalSignificance], [CreatedBy], [ConfidenceLevel], [Conclusion])
        VALUES
        ('Performance Optimization Test', @Template1Id, @Template2Id, 50,
         DATEADD(day, -14, GETUTCDATE()), DATEADD(day, 7, GETUTCDATE()), 'active',
         0.8250, 0.8750, 0.9500, 'System', 0.9500,
         'Variant shows 5% improvement in success rate with high statistical significance'),
        ('Response Quality Test', @Template1Id, @Template2Id, 30,
         DATEADD(day, -7, GETUTCDATE()), NULL, 'active',
         NULL, NULL, NULL, 'System', NULL, NULL)

        PRINT '  ✅ Inserted sample A/B test data'
    END
END

-- Insert sample improvement suggestions
IF EXISTS (SELECT * FROM sysobjects WHERE name='PromptTemplates' AND xtype='U')
BEGIN
    INSERT INTO [dbo].[TemplateImprovementSuggestions]
    ([TemplateId], [SuggestionType], [CurrentVersion], [SuggestedChanges], [ReasoningExplanation],
     [ExpectedImprovementPercent], [BasedOnDataPoints], [Status], [CreatedBy], [TemplateKey],
     [TemplateName], [ImprovementType], [ConfidenceScore], [Title], [Description], [Priority])
    SELECT TOP 3
        pt.Id,
        'performance' as SuggestionType,
        '1.0' as CurrentVersion,
        '{"promptStructure": "Add more specific context", "parameters": "Increase temperature to 0.7"}' as SuggestedChanges,
        'Analysis of user feedback indicates that more specific context improves response quality by reducing ambiguity.' as ReasoningExplanation,
        15.50 as ExpectedImprovementPercent,
        250 as BasedOnDataPoints,
        'pending' as Status,
        'System' as CreatedBy,
        pt.TemplateKey,
        COALESCE(pt.Name, 'Template ' + CAST(pt.Id AS nvarchar(10))) as TemplateName,
        'context_enhancement' as ImprovementType,
        0.8750 as ConfidenceScore,
        'Enhance Context Specificity' as Title,
        'Improve template performance by adding more specific contextual information' as Description,
        2 as Priority
    FROM [dbo].[PromptTemplates] pt
    WHERE NOT EXISTS (
        SELECT 1 FROM [dbo].[TemplateImprovementSuggestions] tis
        WHERE tis.TemplateId = pt.Id
    )

    PRINT '  ✅ Inserted sample improvement suggestions'
END

-- =====================================================================================
-- 6. UPDATE ADMIN CONFIGURATION
-- =====================================================================================

PRINT '⚙️  Updating Admin Configuration...'

-- Insert/Update template analytics configuration
MERGE [dbo].[AdminConfiguration] AS target
USING (VALUES
    ('TEMPLATE_ANALYTICS_ENABLED', 'true', 'analytics', 'Enable template performance analytics and monitoring'),
    ('TEMPLATE_PERFORMANCE_RETENTION_DAYS', '365', 'analytics', 'Number of days to retain template performance data'),
    ('AB_TEST_MIN_SAMPLE_SIZE', '100', 'template', 'Minimum sample size required for A/B test statistical significance'),
    ('AB_TEST_CONFIDENCE_THRESHOLD', '0.95', 'template', 'Confidence threshold for A/B test conclusions'),
    ('IMPROVEMENT_SUGGESTION_AUTO_APPROVE_THRESHOLD', '0.90', 'ml', 'Auto-approve improvement suggestions above this confidence score'),
    ('PERFORMANCE_METRICS_UPDATE_INTERVAL_HOURS', '6', 'analytics', 'How often to update template performance metrics (hours)'),
    ('TEMPLATE_USAGE_TRACKING_ENABLED', 'true', 'analytics', 'Enable detailed template usage tracking'),
    ('REAL_TIME_ANALYTICS_ENABLED', 'true', 'analytics', 'Enable real-time analytics updates via SignalR')
) AS source ([ConfigurationKey], [ConfigurationValue], [ConfigurationType], [Description])
ON target.[ConfigurationKey] = source.[ConfigurationKey]
WHEN MATCHED THEN
    UPDATE SET
        [ConfigurationValue] = source.[ConfigurationValue],
        [Description] = source.[Description],
        [UpdatedBy] = 'Migration Script',
        [UpdatedDate] = GETUTCDATE()
WHEN NOT MATCHED THEN
    INSERT ([ConfigurationKey], [ConfigurationValue], [ConfigurationType], [Description], [CreatedBy])
    VALUES (source.[ConfigurationKey], source.[ConfigurationValue], source.[ConfigurationType], source.[Description], 'Migration Script');

PRINT '  ✅ Updated admin configuration settings'

PRINT ''
PRINT '🎉 Template Analytics Migration Completed Successfully!'
PRINT ''
PRINT '📊 Summary:'
PRINT '  • TemplatePerformanceMetrics table updated/created with all required columns'
PRINT '  • TemplateABTests table updated/created with enhanced analytics features'
PRINT '  • TemplateImprovementSuggestions table updated/created with comprehensive tracking'
PRINT '  • Foreign key constraints added for data integrity'
PRINT '  • Sample seed data inserted for testing and development'
PRINT '  • Admin configuration updated with analytics settings'
PRINT ''
PRINT '✅ The Template Analytics System is now ready for use!'
PRINT ''
