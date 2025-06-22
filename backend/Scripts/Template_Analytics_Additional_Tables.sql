-- =====================================================================================
-- TEMPLATE ANALYTICS SYSTEM - ADDITIONAL ANALYTICS TABLES
-- =====================================================================================
-- This script creates additional supporting tables for advanced analytics features
-- including real-time monitoring, detailed usage tracking, and performance alerts.
-- 
-- Version: 1.0
-- Date: 2024-12-22
-- =====================================================================================

USE [BIReportingCopilot_Dev]
GO

PRINT '📈 Creating Additional Analytics Tables...'
PRINT ''

-- =====================================================================================
-- 1. TEMPLATE USAGE TRACKING TABLE
-- =====================================================================================

PRINT '📊 Creating TemplateUsageTracking table...'

IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='TemplateUsageTracking' AND xtype='U')
BEGIN
    CREATE TABLE [dbo].[TemplateUsageTracking] (
        [Id] bigint IDENTITY(1,1) NOT NULL,
        [TemplateId] bigint NOT NULL,
        [TemplateKey] nvarchar(100) NOT NULL,
        [UserId] nvarchar(256) NOT NULL,
        [SessionId] nvarchar(128) NULL,
        [IntentType] nvarchar(50) NOT NULL,
        [QueryText] nvarchar(max) NULL,
        [ResponseGenerated] bit NOT NULL DEFAULT 0,
        [ResponseQuality] decimal(3,2) NULL, -- 1-5 rating
        [ProcessingTimeMs] int NOT NULL DEFAULT 0,
        [ConfidenceScore] decimal(5,4) NULL,
        [UserFeedback] nvarchar(20) NULL, -- 'positive', 'negative', 'neutral'
        [UserRating] decimal(3,2) NULL, -- 1-5 rating
        [ErrorOccurred] bit NOT NULL DEFAULT 0,
        [ErrorMessage] nvarchar(1000) NULL,
        [ContextData] nvarchar(max) NULL, -- JSON with additional context
        [UsageDate] datetime2(7) NOT NULL DEFAULT GETUTCDATE(),
        [CreatedDate] datetime2(7) NOT NULL DEFAULT GETUTCDATE(),
        CONSTRAINT [PK_TemplateUsageTracking] PRIMARY KEY CLUSTERED ([Id] ASC)
    )
    
    -- Indexes for performance
    CREATE INDEX [IX_TemplateUsageTracking_TemplateId] ON [dbo].[TemplateUsageTracking] ([TemplateId])
    CREATE INDEX [IX_TemplateUsageTracking_TemplateKey] ON [dbo].[TemplateUsageTracking] ([TemplateKey])
    CREATE INDEX [IX_TemplateUsageTracking_UserId] ON [dbo].[TemplateUsageTracking] ([UserId])
    CREATE INDEX [IX_TemplateUsageTracking_UsageDate] ON [dbo].[TemplateUsageTracking] ([UsageDate])
    CREATE INDEX [IX_TemplateUsageTracking_IntentType] ON [dbo].[TemplateUsageTracking] ([IntentType])
    CREATE INDEX [IX_TemplateUsageTracking_ResponseGenerated] ON [dbo].[TemplateUsageTracking] ([ResponseGenerated])
    
    PRINT '  ✅ Created TemplateUsageTracking table'
END

-- =====================================================================================
-- 2. PERFORMANCE ALERTS TABLE
-- =====================================================================================

PRINT '🚨 Creating PerformanceAlerts table...'

IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='PerformanceAlerts' AND xtype='U')
BEGIN
    CREATE TABLE [dbo].[PerformanceAlerts] (
        [Id] bigint IDENTITY(1,1) NOT NULL,
        [AlertType] nvarchar(50) NOT NULL, -- 'performance_degradation', 'low_success_rate', 'high_error_rate'
        [Severity] nvarchar(20) NOT NULL, -- 'low', 'medium', 'high', 'critical'
        [TemplateId] bigint NULL,
        [TemplateKey] nvarchar(100) NULL,
        [IntentType] nvarchar(50) NULL,
        [AlertTitle] nvarchar(200) NOT NULL,
        [AlertMessage] nvarchar(1000) NOT NULL,
        [MetricName] nvarchar(100) NULL,
        [CurrentValue] decimal(18,6) NULL,
        [ThresholdValue] decimal(18,6) NULL,
        [TrendDirection] nvarchar(20) NULL, -- 'increasing', 'decreasing', 'stable'
        [Status] nvarchar(20) NOT NULL DEFAULT 'active', -- 'active', 'acknowledged', 'resolved', 'dismissed'
        [AcknowledgedBy] nvarchar(256) NULL,
        [AcknowledgedDate] datetime2(7) NULL,
        [ResolvedBy] nvarchar(256) NULL,
        [ResolvedDate] datetime2(7) NULL,
        [ResolutionNotes] nvarchar(1000) NULL,
        [AlertData] nvarchar(max) NULL, -- JSON with additional alert context
        [CreatedDate] datetime2(7) NOT NULL DEFAULT GETUTCDATE(),
        [UpdatedDate] datetime2(7) NOT NULL DEFAULT GETUTCDATE(),
        CONSTRAINT [PK_PerformanceAlerts] PRIMARY KEY CLUSTERED ([Id] ASC)
    )
    
    -- Indexes for performance
    CREATE INDEX [IX_PerformanceAlerts_AlertType] ON [dbo].[PerformanceAlerts] ([AlertType])
    CREATE INDEX [IX_PerformanceAlerts_Severity] ON [dbo].[PerformanceAlerts] ([Severity])
    CREATE INDEX [IX_PerformanceAlerts_Status] ON [dbo].[PerformanceAlerts] ([Status])
    CREATE INDEX [IX_PerformanceAlerts_TemplateId] ON [dbo].[PerformanceAlerts] ([TemplateId])
    CREATE INDEX [IX_PerformanceAlerts_CreatedDate] ON [dbo].[PerformanceAlerts] ([CreatedDate])
    
    PRINT '  ✅ Created PerformanceAlerts table'
END

-- =====================================================================================
-- 3. ANALYTICS DASHBOARD CACHE TABLE
-- =====================================================================================

PRINT '💾 Creating AnalyticsDashboardCache table...'

IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='AnalyticsDashboardCache' AND xtype='U')
BEGIN
    CREATE TABLE [dbo].[AnalyticsDashboardCache] (
        [Id] bigint IDENTITY(1,1) NOT NULL,
        [CacheKey] nvarchar(200) NOT NULL,
        [CacheType] nvarchar(50) NOT NULL, -- 'dashboard_summary', 'performance_trends', 'usage_stats'
        [DataScope] nvarchar(50) NULL, -- 'global', 'template', 'user', 'intent_type'
        [ScopeValue] nvarchar(100) NULL, -- specific template key, user id, etc.
        [CachedData] nvarchar(max) NOT NULL, -- JSON data
        [ExpiresAt] datetime2(7) NOT NULL,
        [CreatedDate] datetime2(7) NOT NULL DEFAULT GETUTCDATE(),
        [UpdatedDate] datetime2(7) NOT NULL DEFAULT GETUTCDATE(),
        CONSTRAINT [PK_AnalyticsDashboardCache] PRIMARY KEY CLUSTERED ([Id] ASC)
    )
    
    -- Indexes for performance
    CREATE UNIQUE INDEX [IX_AnalyticsDashboardCache_CacheKey] ON [dbo].[AnalyticsDashboardCache] ([CacheKey])
    CREATE INDEX [IX_AnalyticsDashboardCache_CacheType] ON [dbo].[AnalyticsDashboardCache] ([CacheType])
    CREATE INDEX [IX_AnalyticsDashboardCache_ExpiresAt] ON [dbo].[AnalyticsDashboardCache] ([ExpiresAt])
    CREATE INDEX [IX_AnalyticsDashboardCache_DataScope] ON [dbo].[AnalyticsDashboardCache] ([DataScope])
    
    PRINT '  ✅ Created AnalyticsDashboardCache table'
END

-- =====================================================================================
-- 4. TEMPLATE ANALYTICS AGGREGATIONS TABLE
-- =====================================================================================

PRINT '📊 Creating TemplateAnalyticsAggregations table...'

IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='TemplateAnalyticsAggregations' AND xtype='U')
BEGIN
    CREATE TABLE [dbo].[TemplateAnalyticsAggregations] (
        [Id] bigint IDENTITY(1,1) NOT NULL,
        [AggregationDate] date NOT NULL,
        [AggregationType] nvarchar(20) NOT NULL, -- 'daily', 'weekly', 'monthly'
        [TemplateId] bigint NULL,
        [TemplateKey] nvarchar(100) NULL,
        [IntentType] nvarchar(50) NULL,
        [TotalUsages] int NOT NULL DEFAULT 0,
        [SuccessfulUsages] int NOT NULL DEFAULT 0,
        [FailedUsages] int NOT NULL DEFAULT 0,
        [SuccessRate] decimal(5,4) NOT NULL DEFAULT 0,
        [AverageProcessingTime] decimal(8,2) NOT NULL DEFAULT 0,
        [AverageConfidenceScore] decimal(5,4) NOT NULL DEFAULT 0,
        [AverageUserRating] decimal(3,2) NULL,
        [UniqueUsers] int NOT NULL DEFAULT 0,
        [TotalErrors] int NOT NULL DEFAULT 0,
        [PositiveFeedbackCount] int NOT NULL DEFAULT 0,
        [NegativeFeedbackCount] int NOT NULL DEFAULT 0,
        [NeutralFeedbackCount] int NOT NULL DEFAULT 0,
        [AggregationData] nvarchar(max) NULL, -- JSON with additional metrics
        [CreatedDate] datetime2(7) NOT NULL DEFAULT GETUTCDATE(),
        [UpdatedDate] datetime2(7) NOT NULL DEFAULT GETUTCDATE(),
        CONSTRAINT [PK_TemplateAnalyticsAggregations] PRIMARY KEY CLUSTERED ([Id] ASC)
    )
    
    -- Indexes for performance
    CREATE INDEX [IX_TemplateAnalyticsAggregations_AggregationDate] ON [dbo].[TemplateAnalyticsAggregations] ([AggregationDate])
    CREATE INDEX [IX_TemplateAnalyticsAggregations_AggregationType] ON [dbo].[TemplateAnalyticsAggregations] ([AggregationType])
    CREATE INDEX [IX_TemplateAnalyticsAggregations_TemplateId] ON [dbo].[TemplateAnalyticsAggregations] ([TemplateId])
    CREATE INDEX [IX_TemplateAnalyticsAggregations_TemplateKey] ON [dbo].[TemplateAnalyticsAggregations] ([TemplateKey])
    CREATE INDEX [IX_TemplateAnalyticsAggregations_IntentType] ON [dbo].[TemplateAnalyticsAggregations] ([IntentType])
    CREATE UNIQUE INDEX [IX_TemplateAnalyticsAggregations_Unique] ON [dbo].[TemplateAnalyticsAggregations] 
        ([AggregationDate], [AggregationType], [TemplateId], [IntentType])
    
    PRINT '  ✅ Created TemplateAnalyticsAggregations table'
END

-- =====================================================================================
-- 5. REAL-TIME ANALYTICS SESSIONS TABLE
-- =====================================================================================

PRINT '⚡ Creating RealTimeAnalyticsSessions table...'

IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='RealTimeAnalyticsSessions' AND xtype='U')
BEGIN
    CREATE TABLE [dbo].[RealTimeAnalyticsSessions] (
        [Id] bigint IDENTITY(1,1) NOT NULL,
        [SessionId] nvarchar(128) NOT NULL,
        [UserId] nvarchar(256) NOT NULL,
        [ConnectionId] nvarchar(128) NOT NULL,
        [SubscriptionType] nvarchar(50) NOT NULL, -- 'dashboard', 'template_specific', 'alerts'
        [SubscriptionFilters] nvarchar(1000) NULL, -- JSON with filter criteria
        [IsActive] bit NOT NULL DEFAULT 1,
        [LastActivity] datetime2(7) NOT NULL DEFAULT GETUTCDATE(),
        [CreatedDate] datetime2(7) NOT NULL DEFAULT GETUTCDATE(),
        [DisconnectedDate] datetime2(7) NULL,
        CONSTRAINT [PK_RealTimeAnalyticsSessions] PRIMARY KEY CLUSTERED ([Id] ASC)
    )
    
    -- Indexes for performance
    CREATE INDEX [IX_RealTimeAnalyticsSessions_SessionId] ON [dbo].[RealTimeAnalyticsSessions] ([SessionId])
    CREATE INDEX [IX_RealTimeAnalyticsSessions_UserId] ON [dbo].[RealTimeAnalyticsSessions] ([UserId])
    CREATE INDEX [IX_RealTimeAnalyticsSessions_ConnectionId] ON [dbo].[RealTimeAnalyticsSessions] ([ConnectionId])
    CREATE INDEX [IX_RealTimeAnalyticsSessions_IsActive] ON [dbo].[RealTimeAnalyticsSessions] ([IsActive])
    CREATE INDEX [IX_RealTimeAnalyticsSessions_SubscriptionType] ON [dbo].[RealTimeAnalyticsSessions] ([SubscriptionType])
    
    PRINT '  ✅ Created RealTimeAnalyticsSessions table'
END

-- =====================================================================================
-- 6. ADD FOREIGN KEY CONSTRAINTS
-- =====================================================================================

PRINT '🔗 Adding Foreign Key Constraints for Additional Tables...'

-- Add FK for TemplateUsageTracking to PromptTemplates
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_TemplateUsageTracking_PromptTemplates')
BEGIN
    IF EXISTS (SELECT * FROM sysobjects WHERE name='PromptTemplates' AND xtype='U')
    BEGIN
        ALTER TABLE [dbo].[TemplateUsageTracking]
        ADD CONSTRAINT [FK_TemplateUsageTracking_PromptTemplates] 
        FOREIGN KEY ([TemplateId]) REFERENCES [dbo].[PromptTemplates] ([Id])
        PRINT '  ✅ Added FK: TemplateUsageTracking -> PromptTemplates'
    END
END

-- Add FK for PerformanceAlerts to PromptTemplates (optional)
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_PerformanceAlerts_PromptTemplates')
BEGIN
    IF EXISTS (SELECT * FROM sysobjects WHERE name='PromptTemplates' AND xtype='U')
    BEGIN
        ALTER TABLE [dbo].[PerformanceAlerts]
        ADD CONSTRAINT [FK_PerformanceAlerts_PromptTemplates] 
        FOREIGN KEY ([TemplateId]) REFERENCES [dbo].[PromptTemplates] ([Id])
        PRINT '  ✅ Added FK: PerformanceAlerts -> PromptTemplates'
    END
END

-- Add FK for TemplateAnalyticsAggregations to PromptTemplates (optional)
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_TemplateAnalyticsAggregations_PromptTemplates')
BEGIN
    IF EXISTS (SELECT * FROM sysobjects WHERE name='PromptTemplates' AND xtype='U')
    BEGIN
        ALTER TABLE [dbo].[TemplateAnalyticsAggregations]
        ADD CONSTRAINT [FK_TemplateAnalyticsAggregations_PromptTemplates] 
        FOREIGN KEY ([TemplateId]) REFERENCES [dbo].[PromptTemplates] ([Id])
        PRINT '  ✅ Added FK: TemplateAnalyticsAggregations -> PromptTemplates'
    END
END

-- =====================================================================================
-- 7. INSERT SAMPLE DATA FOR ADDITIONAL TABLES
-- =====================================================================================

PRINT '🌱 Inserting Sample Data for Additional Tables...'

-- Insert sample performance alerts
INSERT INTO [dbo].[PerformanceAlerts] 
([AlertType], [Severity], [TemplateKey], [IntentType], [AlertTitle], [AlertMessage], 
 [MetricName], [CurrentValue], [ThresholdValue], [TrendDirection], [Status])
VALUES 
('performance_degradation', 'medium', 'query_analysis_template', 'data_analysis', 
 'Performance Degradation Detected', 
 'Template response time has increased by 25% over the last 24 hours', 
 'average_response_time_ms', 850.00, 600.00, 'increasing', 'active'),
('low_success_rate', 'high', 'report_generation_template', 'report_creation', 
 'Low Success Rate Alert', 
 'Template success rate has dropped below 80% threshold', 
 'success_rate', 0.7500, 0.8000, 'decreasing', 'active'),
('high_error_rate', 'critical', 'data_visualization_template', 'visualization', 
 'High Error Rate Detected', 
 'Template error rate has exceeded 10% in the last hour', 
 'error_rate', 0.1250, 0.1000, 'increasing', 'acknowledged')

PRINT '  ✅ Inserted sample performance alerts'

-- Insert sample dashboard cache entries
INSERT INTO [dbo].[AnalyticsDashboardCache] 
([CacheKey], [CacheType], [DataScope], [CachedData], [ExpiresAt])
VALUES 
('dashboard_summary_global', 'dashboard_summary', 'global', 
 '{"totalTemplates": 25, "totalUsages": 15420, "averageSuccessRate": 0.8750, "topPerformingTemplate": "query_analysis_template"}', 
 DATEADD(hour, 1, GETUTCDATE())),
('performance_trends_weekly', 'performance_trends', 'global', 
 '{"weeklyTrends": [{"date": "2024-12-15", "usages": 2100, "successRate": 0.87}, {"date": "2024-12-22", "usages": 2350, "successRate": 0.89}]}', 
 DATEADD(hour, 6, GETUTCDATE()))

PRINT '  ✅ Inserted sample dashboard cache entries'

PRINT ''
PRINT '🎉 Additional Analytics Tables Created Successfully!'
PRINT ''
PRINT '📊 Summary of Additional Tables:'
PRINT '  • TemplateUsageTracking - Detailed usage tracking for each template interaction'
PRINT '  • PerformanceAlerts - Real-time alerts for performance issues'
PRINT '  • AnalyticsDashboardCache - Caching layer for dashboard performance'
PRINT '  • TemplateAnalyticsAggregations - Pre-aggregated analytics data'
PRINT '  • RealTimeAnalyticsSessions - SignalR session management for real-time updates'
PRINT ''
PRINT '✅ Enhanced Template Analytics System is now fully operational!'
PRINT ''
