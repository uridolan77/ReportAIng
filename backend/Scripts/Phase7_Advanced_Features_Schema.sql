-- =====================================================================================
-- Phase 7: Advanced Features & Admin Tools - Database Schema
-- =====================================================================================

USE [BIReportingCopilot_Dev]
GO

-- =====================================================================================
-- 1. PROMPT SUCCESS TRACKING TABLES
-- =====================================================================================

-- Table for tracking prompt generation and SQL success
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='PromptSuccessTracking' AND xtype='U')
BEGIN
    CREATE TABLE [dbo].[PromptSuccessTracking] (
        [Id] bigint IDENTITY(1,1) NOT NULL,
        [SessionId] nvarchar(128) NOT NULL,
        [UserId] nvarchar(512) NOT NULL,
        [UserQuestion] nvarchar(2000) NOT NULL,
        [GeneratedPrompt] nvarchar(max) NOT NULL,
        [TemplateUsed] nvarchar(100) NULL,
        [IntentClassified] nvarchar(50) NOT NULL,
        [DomainClassified] nvarchar(50) NOT NULL,
        [TablesRetrieved] nvarchar(1000) NULL,
        [GeneratedSQL] nvarchar(max) NULL,
        [SQLExecutionSuccess] bit NULL,
        [SQLExecutionError] nvarchar(2000) NULL,
        [UserFeedbackRating] int NULL, -- 1-5 scale
        [UserFeedbackComments] nvarchar(1000) NULL,
        [ProcessingTimeMs] int NOT NULL,
        [ConfidenceScore] decimal(5,4) NOT NULL,
        [CreatedDate] datetime2(7) NOT NULL DEFAULT GETUTCDATE(),
        [UpdatedDate] datetime2(7) NULL,
        CONSTRAINT [PK_PromptSuccessTracking] PRIMARY KEY CLUSTERED ([Id] ASC)
    )
    
    CREATE INDEX [IX_PromptSuccessTracking_UserId] ON [dbo].[PromptSuccessTracking] ([UserId])
    CREATE INDEX [IX_PromptSuccessTracking_CreatedDate] ON [dbo].[PromptSuccessTracking] ([CreatedDate])
    CREATE INDEX [IX_PromptSuccessTracking_IntentClassified] ON [dbo].[PromptSuccessTracking] ([IntentClassified])
    CREATE INDEX [IX_PromptSuccessTracking_TemplateUsed] ON [dbo].[PromptSuccessTracking] ([TemplateUsed])
END

-- Table for tracking template performance metrics
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='TemplatePerformanceMetrics' AND xtype='U')
BEGIN
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
        [CreatedDate] datetime2(7) NOT NULL DEFAULT GETUTCDATE(),
        [UpdatedDate] datetime2(7) NOT NULL DEFAULT GETUTCDATE(),
        CONSTRAINT [PK_TemplatePerformanceMetrics] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [FK_TemplatePerformanceMetrics_PromptTemplates] FOREIGN KEY ([TemplateId]) REFERENCES [dbo].[PromptTemplates] ([Id])
    )
    
    CREATE UNIQUE INDEX [IX_TemplatePerformanceMetrics_TemplateId] ON [dbo].[TemplatePerformanceMetrics] ([TemplateId])
    CREATE INDEX [IX_TemplatePerformanceMetrics_IntentType] ON [dbo].[TemplatePerformanceMetrics] ([IntentType])
END

-- =====================================================================================
-- 2. USER PREFERENCE LEARNING TABLES
-- =====================================================================================

-- Table for storing user preferences and behavior patterns
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='UserPreferences' AND xtype='U')
BEGIN
    CREATE TABLE [dbo].[UserPreferences] (
        [Id] bigint IDENTITY(1,1) NOT NULL,
        [UserId] nvarchar(512) NOT NULL,
        [PreferredIntentTypes] nvarchar(500) NULL, -- JSON array of preferred intents
        [PreferredDomains] nvarchar(500) NULL, -- JSON array of preferred domains
        [PreferredTemplateStyles] nvarchar(500) NULL, -- JSON array of template preferences
        [AvgQueryComplexity] decimal(3,2) NOT NULL DEFAULT 0,
        [PreferredResponseFormat] nvarchar(50) NULL,
        [LanguagePreference] nvarchar(10) NOT NULL DEFAULT 'en',
        [TimeZone] nvarchar(50) NULL,
        [NotificationPreferences] nvarchar(1000) NULL, -- JSON object
        [CreatedDate] datetime2(7) NOT NULL DEFAULT GETUTCDATE(),
        [UpdatedDate] datetime2(7) NOT NULL DEFAULT GETUTCDATE(),
        CONSTRAINT [PK_UserPreferences] PRIMARY KEY CLUSTERED ([Id] ASC)
    )
    
    CREATE UNIQUE INDEX [IX_UserPreferences_UserId] ON [dbo].[UserPreferences] ([UserId])
END

-- Table for tracking user behavior patterns
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='UserBehaviorPatterns' AND xtype='U')
BEGIN
    CREATE TABLE [dbo].[UserBehaviorPatterns] (
        [Id] bigint IDENTITY(1,1) NOT NULL,
        [UserId] nvarchar(512) NOT NULL,
        [SessionId] nvarchar(128) NOT NULL,
        [QueryPattern] nvarchar(100) NOT NULL, -- e.g., "daily_metrics", "comparison_analysis"
        [FrequencyScore] decimal(5,4) NOT NULL DEFAULT 0,
        [SuccessRate] decimal(5,4) NOT NULL DEFAULT 0,
        [PreferredTables] nvarchar(1000) NULL, -- JSON array of frequently used tables
        [PreferredTimeRanges] nvarchar(500) NULL, -- JSON array of time preferences
        [TypicalQueryTime] time(7) NULL, -- Time of day user typically queries
        [SessionDuration] int NULL, -- Average session duration in minutes
        [CreatedDate] datetime2(7) NOT NULL DEFAULT GETUTCDATE(),
        [UpdatedDate] datetime2(7) NOT NULL DEFAULT GETUTCDATE(),
        CONSTRAINT [PK_UserBehaviorPatterns] PRIMARY KEY CLUSTERED ([Id] ASC)
    )
    
    CREATE INDEX [IX_UserBehaviorPatterns_UserId] ON [dbo].[UserBehaviorPatterns] ([UserId])
    CREATE INDEX [IX_UserBehaviorPatterns_QueryPattern] ON [dbo].[UserBehaviorPatterns] ([QueryPattern])
END

-- =====================================================================================
-- 3. AUTOMATED TEMPLATE IMPROVEMENT TABLES
-- =====================================================================================

-- Table for storing template improvement suggestions
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='TemplateImprovementSuggestions' AND xtype='U')
BEGIN
    CREATE TABLE [dbo].[TemplateImprovementSuggestions] (
        [Id] bigint IDENTITY(1,1) NOT NULL,
        [TemplateId] bigint NOT NULL,
        [SuggestionType] nvarchar(50) NOT NULL, -- 'performance', 'accuracy', 'user_feedback'
        [CurrentVersion] nvarchar(20) NOT NULL,
        [SuggestedChanges] nvarchar(max) NOT NULL, -- JSON object with suggested modifications
        [ReasoningExplanation] nvarchar(2000) NOT NULL,
        [ExpectedImprovementPercent] decimal(5,2) NULL,
        [BasedOnDataPoints] int NOT NULL, -- Number of data points used for suggestion
        [Status] nvarchar(20) NOT NULL DEFAULT 'pending', -- 'pending', 'approved', 'rejected', 'implemented'
        [ReviewedBy] nvarchar(512) NULL,
        [ReviewedDate] datetime2(7) NULL,
        [ReviewComments] nvarchar(1000) NULL,
        [CreatedDate] datetime2(7) NOT NULL DEFAULT GETUTCDATE(),
        CONSTRAINT [PK_TemplateImprovementSuggestions] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [FK_TemplateImprovementSuggestions_PromptTemplates] FOREIGN KEY ([TemplateId]) REFERENCES [dbo].[PromptTemplates] ([Id])
    )
    
    CREATE INDEX [IX_TemplateImprovementSuggestions_TemplateId] ON [dbo].[TemplateImprovementSuggestions] ([TemplateId])
    CREATE INDEX [IX_TemplateImprovementSuggestions_Status] ON [dbo].[TemplateImprovementSuggestions] ([Status])
    CREATE INDEX [IX_TemplateImprovementSuggestions_SuggestionType] ON [dbo].[TemplateImprovementSuggestions] ([SuggestionType])
END

-- Table for A/B testing template variations
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='TemplateABTests' AND xtype='U')
BEGIN
    CREATE TABLE [dbo].[TemplateABTests] (
        [Id] bigint IDENTITY(1,1) NOT NULL,
        [TestName] nvarchar(100) NOT NULL,
        [OriginalTemplateId] bigint NOT NULL,
        [VariantTemplateId] bigint NOT NULL,
        [TrafficSplitPercent] int NOT NULL DEFAULT 50, -- Percentage of traffic to variant
        [StartDate] datetime2(7) NOT NULL,
        [EndDate] datetime2(7) NULL,
        [Status] nvarchar(20) NOT NULL DEFAULT 'active', -- 'active', 'completed', 'paused'
        [OriginalSuccessRate] decimal(5,4) NULL,
        [VariantSuccessRate] decimal(5,4) NULL,
        [StatisticalSignificance] decimal(5,4) NULL,
        [WinnerTemplateId] bigint NULL,
        [TestResults] nvarchar(max) NULL, -- JSON object with detailed results
        [CreatedBy] nvarchar(512) NOT NULL,
        [CreatedDate] datetime2(7) NOT NULL DEFAULT GETUTCDATE(),
        [CompletedDate] datetime2(7) NULL,
        CONSTRAINT [PK_TemplateABTests] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [FK_TemplateABTests_OriginalTemplate] FOREIGN KEY ([OriginalTemplateId]) REFERENCES [dbo].[PromptTemplates] ([Id]),
        CONSTRAINT [FK_TemplateABTests_VariantTemplate] FOREIGN KEY ([VariantTemplateId]) REFERENCES [dbo].[PromptTemplates] ([Id])
    )
    
    CREATE INDEX [IX_TemplateABTests_Status] ON [dbo].[TemplateABTests] ([Status])
    CREATE INDEX [IX_TemplateABTests_StartDate] ON [dbo].[TemplateABTests] ([StartDate])
END

-- =====================================================================================
-- 4. SYSTEM ANALYTICS AND MONITORING TABLES
-- =====================================================================================

-- Table for system performance metrics
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='SystemPerformanceMetrics' AND xtype='U')
BEGIN
    CREATE TABLE [dbo].[SystemPerformanceMetrics] (
        [Id] bigint IDENTITY(1,1) NOT NULL,
        [MetricName] nvarchar(100) NOT NULL,
        [MetricValue] decimal(18,6) NOT NULL,
        [MetricUnit] nvarchar(20) NOT NULL, -- 'ms', 'percent', 'count', 'bytes'
        [Category] nvarchar(50) NOT NULL, -- 'performance', 'usage', 'quality', 'errors'
        [SubCategory] nvarchar(50) NULL,
        [Dimensions] nvarchar(1000) NULL, -- JSON object for additional dimensions
        [Timestamp] datetime2(7) NOT NULL DEFAULT GETUTCDATE(),
        CONSTRAINT [PK_SystemPerformanceMetrics] PRIMARY KEY CLUSTERED ([Id] ASC)
    )
    
    CREATE INDEX [IX_SystemPerformanceMetrics_MetricName] ON [dbo].[SystemPerformanceMetrics] ([MetricName])
    CREATE INDEX [IX_SystemPerformanceMetrics_Timestamp] ON [dbo].[SystemPerformanceMetrics] ([Timestamp])
    CREATE INDEX [IX_SystemPerformanceMetrics_Category] ON [dbo].[SystemPerformanceMetrics] ([Category])
END

-- Table for usage analytics aggregations
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='UsageAnalytics' AND xtype='U')
BEGIN
    CREATE TABLE [dbo].[UsageAnalytics] (
        [Id] bigint IDENTITY(1,1) NOT NULL,
        [AnalyticsDate] date NOT NULL,
        [TotalQueries] int NOT NULL DEFAULT 0,
        [SuccessfulQueries] int NOT NULL DEFAULT 0,
        [FailedQueries] int NOT NULL DEFAULT 0,
        [UniqueUsers] int NOT NULL DEFAULT 0,
        [AverageResponseTime] decimal(8,2) NOT NULL DEFAULT 0,
        [AverageConfidenceScore] decimal(5,4) NOT NULL DEFAULT 0,
        [TopIntentTypes] nvarchar(500) NULL, -- JSON array of top intents
        [TopDomains] nvarchar(500) NULL, -- JSON array of top domains
        [TopTemplates] nvarchar(500) NULL, -- JSON array of most used templates
        [ErrorCategories] nvarchar(1000) NULL, -- JSON object with error breakdowns
        [CreatedDate] datetime2(7) NOT NULL DEFAULT GETUTCDATE(),
        [UpdatedDate] datetime2(7) NOT NULL DEFAULT GETUTCDATE(),
        CONSTRAINT [PK_UsageAnalytics] PRIMARY KEY CLUSTERED ([Id] ASC)
    )
    
    CREATE UNIQUE INDEX [IX_UsageAnalytics_AnalyticsDate] ON [dbo].[UsageAnalytics] ([AnalyticsDate])
END

-- =====================================================================================
-- 5. ADMIN CONFIGURATION TABLES
-- =====================================================================================

-- Table for admin configuration settings
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='AdminConfiguration' AND xtype='U')
BEGIN
    CREATE TABLE [dbo].[AdminConfiguration] (
        [Id] bigint IDENTITY(1,1) NOT NULL,
        [ConfigurationKey] nvarchar(100) NOT NULL,
        [ConfigurationValue] nvarchar(max) NOT NULL,
        [ConfigurationType] nvarchar(50) NOT NULL, -- 'system', 'template', 'analytics', 'ml'
        [Description] nvarchar(500) NULL,
        [IsActive] bit NOT NULL DEFAULT 1,
        [RequiresRestart] bit NOT NULL DEFAULT 0,
        [CreatedBy] nvarchar(512) NOT NULL,
        [CreatedDate] datetime2(7) NOT NULL DEFAULT GETUTCDATE(),
        [UpdatedBy] nvarchar(512) NULL,
        [UpdatedDate] datetime2(7) NULL,
        CONSTRAINT [PK_AdminConfiguration] PRIMARY KEY CLUSTERED ([Id] ASC)
    )
    
    CREATE UNIQUE INDEX [IX_AdminConfiguration_ConfigurationKey] ON [dbo].[AdminConfiguration] ([ConfigurationKey])
    CREATE INDEX [IX_AdminConfiguration_ConfigurationType] ON [dbo].[AdminConfiguration] ([ConfigurationType])
END

-- =====================================================================================
-- 6. INITIAL CONFIGURATION DATA
-- =====================================================================================

-- Insert default admin configuration
IF NOT EXISTS (SELECT * FROM [dbo].[AdminConfiguration] WHERE [ConfigurationKey] = 'ML_IMPROVEMENT_ENABLED')
BEGIN
    INSERT INTO [dbo].[AdminConfiguration] ([ConfigurationKey], [ConfigurationValue], [ConfigurationType], [Description], [CreatedBy])
    VALUES 
        ('ML_IMPROVEMENT_ENABLED', 'true', 'ml', 'Enable automated ML-based template improvement suggestions', 'System'),
        ('AB_TESTING_ENABLED', 'true', 'template', 'Enable A/B testing for template variations', 'System'),
        ('USER_PREFERENCE_LEARNING_ENABLED', 'true', 'system', 'Enable user preference learning and personalization', 'System'),
        ('SUCCESS_TRACKING_RETENTION_DAYS', '90', 'analytics', 'Number of days to retain detailed success tracking data', 'System'),
        ('ANALYTICS_AGGREGATION_SCHEDULE', '0 2 * * *', 'analytics', 'Cron expression for analytics aggregation schedule', 'System'),
        ('TEMPLATE_IMPROVEMENT_THRESHOLD', '0.15', 'ml', 'Minimum improvement threshold for template suggestions (15%)', 'System'),
        ('MIN_DATA_POINTS_FOR_SUGGESTIONS', '100', 'ml', 'Minimum number of data points required for improvement suggestions', 'System'),
        ('PERFORMANCE_ALERT_THRESHOLD_MS', '1000', 'system', 'Performance alert threshold in milliseconds', 'System'),
        ('SUCCESS_RATE_ALERT_THRESHOLD', '0.80', 'system', 'Success rate alert threshold (80%)', 'System'),
        ('MAX_CONCURRENT_AB_TESTS', '5', 'template', 'Maximum number of concurrent A/B tests allowed', 'System')
END

PRINT 'Phase 7: Advanced Features & Admin Tools schema created successfully!'
