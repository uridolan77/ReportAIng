-- =====================================================
-- Cost Optimization & Performance Enhancement Tables
-- FIXED Migration Script for BI Reporting Copilot
-- Addresses foreign key data type mismatches
-- =====================================================

USE [BIReportingCopilot_Dev]
GO

PRINT '============================================='
PRINT 'Starting FIXED Cost Optimization Migration'
PRINT '============================================='

-- First, let's check the actual Users table structure
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'Users')
BEGIN
    DECLARE @UserIdDataType NVARCHAR(128)
    SELECT @UserIdDataType = 
        t.name + '(' + CAST(c.max_length AS NVARCHAR(10)) + ')'
    FROM sys.columns c
    INNER JOIN sys.types t ON c.user_type_id = t.user_type_id
    WHERE c.object_id = OBJECT_ID('Users') AND c.name = 'Id'
    
    PRINT 'Users.Id data type: ' + @UserIdDataType
END
GO

-- =====================================================
-- 1. LLM Cost Tracking Table
-- =====================================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[LLMCostTracking]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[LLMCostTracking] (
        [Id] BIGINT IDENTITY(1,1) NOT NULL,
        [UserId] NVARCHAR(256) NOT NULL,  -- Fixed: Match Users.Id data type
        [QueryId] NVARCHAR(256) NULL,
        [ProviderId] NVARCHAR(100) NOT NULL,
        [ModelId] NVARCHAR(100) NOT NULL,
        [Cost] DECIMAL(18,6) NOT NULL,
        [CostPerToken] DECIMAL(18,8) NOT NULL,
        [InputTokens] INT NOT NULL DEFAULT 0,
        [OutputTokens] INT NOT NULL DEFAULT 0,
        [TotalTokens] INT NOT NULL DEFAULT 0,
        [DurationMs] BIGINT NOT NULL DEFAULT 0,
        [Category] NVARCHAR(50) NOT NULL DEFAULT 'Query',
        [Metadata] NVARCHAR(MAX) NULL,
        [CreatedDate] DATETIME2(7) NOT NULL DEFAULT GETUTCDATE(),
        [UpdatedDate] DATETIME2(7) NULL,
        
        CONSTRAINT [PK_LLMCostTracking] PRIMARY KEY CLUSTERED ([Id] ASC)
    )
    
    -- Add foreign key constraint separately to handle potential issues
    IF EXISTS (SELECT * FROM sys.tables WHERE name = 'Users')
    BEGIN
        ALTER TABLE [dbo].[LLMCostTracking]
        ADD CONSTRAINT [FK_LLMCostTracking_Users] 
        FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users]([Id]) ON DELETE CASCADE
    END
    
    -- Create indexes for performance
    CREATE NONCLUSTERED INDEX [IX_LLMCostTracking_UserId] ON [dbo].[LLMCostTracking] ([UserId])
    CREATE NONCLUSTERED INDEX [IX_LLMCostTracking_QueryId] ON [dbo].[LLMCostTracking] ([QueryId])
    CREATE NONCLUSTERED INDEX [IX_LLMCostTracking_ProviderId] ON [dbo].[LLMCostTracking] ([ProviderId])
    CREATE NONCLUSTERED INDEX [IX_LLMCostTracking_CreatedDate] ON [dbo].[LLMCostTracking] ([CreatedDate])
    CREATE NONCLUSTERED INDEX [IX_LLMCostTracking_Category] ON [dbo].[LLMCostTracking] ([Category])
    
    PRINT 'Created LLMCostTracking table with indexes'
END
ELSE
    PRINT 'LLMCostTracking table already exists'
GO

-- =====================================================
-- 2. LLM Budget Management Table
-- =====================================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[LLMBudgetManagement]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[LLMBudgetManagement] (
        [Id] BIGINT IDENTITY(1,1) NOT NULL,
        [Name] NVARCHAR(200) NOT NULL,
        [Type] NVARCHAR(50) NOT NULL DEFAULT 'User',
        [EntityId] NVARCHAR(256) NOT NULL,  -- Fixed: Match Users.Id data type
        [BudgetAmount] DECIMAL(18,2) NOT NULL,
        [SpentAmount] DECIMAL(18,2) NOT NULL DEFAULT 0,
        [RemainingAmount] DECIMAL(18,2) NOT NULL,
        [Period] NVARCHAR(20) NOT NULL DEFAULT 'Monthly',
        [StartDate] DATETIME2(7) NOT NULL,
        [EndDate] DATETIME2(7) NOT NULL,
        [AlertThreshold] DECIMAL(5,2) NOT NULL DEFAULT 0.80,
        [BlockThreshold] DECIMAL(5,2) NOT NULL DEFAULT 0.95,
        [IsActive] BIT NOT NULL DEFAULT 1,
        [CreatedDate] DATETIME2(7) NOT NULL DEFAULT GETUTCDATE(),
        [UpdatedDate] DATETIME2(7) NULL,
        
        CONSTRAINT [PK_LLMBudgetManagement] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [CK_LLMBudgetManagement_BudgetAmount] CHECK ([BudgetAmount] >= 0),
        CONSTRAINT [CK_LLMBudgetManagement_SpentAmount] CHECK ([SpentAmount] >= 0),
        CONSTRAINT [CK_LLMBudgetManagement_AlertThreshold] CHECK ([AlertThreshold] BETWEEN 0 AND 1),
        CONSTRAINT [CK_LLMBudgetManagement_BlockThreshold] CHECK ([BlockThreshold] BETWEEN 0 AND 1)
    )
    
    -- Create indexes
    CREATE NONCLUSTERED INDEX [IX_LLMBudgetManagement_EntityId] ON [dbo].[LLMBudgetManagement] ([EntityId])
    CREATE NONCLUSTERED INDEX [IX_LLMBudgetManagement_Type] ON [dbo].[LLMBudgetManagement] ([Type])
    CREATE NONCLUSTERED INDEX [IX_LLMBudgetManagement_Period] ON [dbo].[LLMBudgetManagement] ([StartDate], [EndDate])
    CREATE NONCLUSTERED INDEX [IX_LLMBudgetManagement_IsActive] ON [dbo].[LLMBudgetManagement] ([IsActive])
    
    PRINT 'Created LLMBudgetManagement table with indexes'
END
ELSE
    PRINT 'LLMBudgetManagement table already exists'
GO

-- =====================================================
-- 3. LLM Resource Usage Table
-- =====================================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[LLMResourceUsage]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[LLMResourceUsage] (
        [Id] BIGINT IDENTITY(1,1) NOT NULL,
        [UserId] NVARCHAR(256) NOT NULL,  -- Fixed: Match Users.Id data type
        [ResourceType] NVARCHAR(100) NOT NULL,
        [ResourceId] NVARCHAR(200) NULL,
        [UsageAmount] DECIMAL(18,4) NOT NULL,
        [UsageUnit] NVARCHAR(50) NOT NULL DEFAULT 'Count',
        [Cost] DECIMAL(18,6) NULL,
        [StartTime] DATETIME2(7) NOT NULL,
        [EndTime] DATETIME2(7) NULL,
        [Duration] BIGINT NULL,
        [Metadata] NVARCHAR(MAX) NULL,
        [CreatedDate] DATETIME2(7) NOT NULL DEFAULT GETUTCDATE(),
        
        CONSTRAINT [PK_LLMResourceUsage] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [CK_LLMResourceUsage_UsageAmount] CHECK ([UsageAmount] >= 0)
    )
    
    -- Add foreign key constraint separately
    IF EXISTS (SELECT * FROM sys.tables WHERE name = 'Users')
    BEGIN
        ALTER TABLE [dbo].[LLMResourceUsage]
        ADD CONSTRAINT [FK_LLMResourceUsage_Users] 
        FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users]([Id]) ON DELETE CASCADE
    END
    
    -- Create indexes
    CREATE NONCLUSTERED INDEX [IX_LLMResourceUsage_UserId] ON [dbo].[LLMResourceUsage] ([UserId])
    CREATE NONCLUSTERED INDEX [IX_LLMResourceUsage_ResourceType] ON [dbo].[LLMResourceUsage] ([ResourceType])
    CREATE NONCLUSTERED INDEX [IX_LLMResourceUsage_StartTime] ON [dbo].[LLMResourceUsage] ([StartTime])
    CREATE NONCLUSTERED INDEX [IX_LLMResourceUsage_ResourceId] ON [dbo].[LLMResourceUsage] ([ResourceId])
    
    PRINT 'Created LLMResourceUsage table with indexes'
END
ELSE
    PRINT 'LLMResourceUsage table already exists'
GO

-- =====================================================
-- 4. LLM Performance Metrics Table
-- =====================================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[LLMPerformanceMetrics]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[LLMPerformanceMetrics] (
        [Id] BIGINT IDENTITY(1,1) NOT NULL,
        [EntityType] NVARCHAR(100) NOT NULL,
        [EntityId] NVARCHAR(200) NOT NULL,
        [MetricName] NVARCHAR(100) NOT NULL,
        [MetricValue] DECIMAL(18,4) NOT NULL,
        [MetricUnit] NVARCHAR(50) NOT NULL DEFAULT 'Count',
        [AverageExecutionTime] BIGINT NOT NULL DEFAULT 0,
        [TotalOperations] INT NOT NULL DEFAULT 0,
        [SuccessCount] INT NOT NULL DEFAULT 0,
        [ErrorCount] INT NOT NULL DEFAULT 0,
        [LastUpdated] DATETIME2(7) NOT NULL DEFAULT GETUTCDATE(),
        [Metadata] NVARCHAR(MAX) NULL,
        [CreatedDate] DATETIME2(7) NOT NULL DEFAULT GETUTCDATE(),
        
        CONSTRAINT [PK_LLMPerformanceMetrics] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [CK_LLMPerformanceMetrics_TotalOperations] CHECK ([TotalOperations] >= 0),
        CONSTRAINT [CK_LLMPerformanceMetrics_SuccessCount] CHECK ([SuccessCount] >= 0),
        CONSTRAINT [CK_LLMPerformanceMetrics_ErrorCount] CHECK ([ErrorCount] >= 0)
    )
    
    -- Create indexes
    CREATE NONCLUSTERED INDEX [IX_LLMPerformanceMetrics_EntityType] ON [dbo].[LLMPerformanceMetrics] ([EntityType])
    CREATE NONCLUSTERED INDEX [IX_LLMPerformanceMetrics_EntityId] ON [dbo].[LLMPerformanceMetrics] ([EntityId])
    CREATE NONCLUSTERED INDEX [IX_LLMPerformanceMetrics_MetricName] ON [dbo].[LLMPerformanceMetrics] ([MetricName])
    CREATE NONCLUSTERED INDEX [IX_LLMPerformanceMetrics_LastUpdated] ON [dbo].[LLMPerformanceMetrics] ([LastUpdated])
    CREATE UNIQUE NONCLUSTERED INDEX [IX_LLMPerformanceMetrics_Unique] ON [dbo].[LLMPerformanceMetrics] ([EntityType], [EntityId], [MetricName])
    
    PRINT 'Created LLMPerformanceMetrics table with indexes'
END
ELSE
    PRINT 'LLMPerformanceMetrics table already exists'
GO

-- =====================================================
-- 5. LLM Cache Statistics Table
-- =====================================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[LLMCacheStatistics]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[LLMCacheStatistics] (
        [Id] BIGINT IDENTITY(1,1) NOT NULL,
        [CacheType] NVARCHAR(100) NOT NULL,
        [TotalOperations] BIGINT NOT NULL DEFAULT 0,
        [HitCount] BIGINT NOT NULL DEFAULT 0,
        [MissCount] BIGINT NOT NULL DEFAULT 0,
        [SetCount] BIGINT NOT NULL DEFAULT 0,
        [DeleteCount] BIGINT NOT NULL DEFAULT 0,
        [HitRate] DECIMAL(5,4) NOT NULL DEFAULT 0,
        [AverageResponseTime] DECIMAL(10,2) NOT NULL DEFAULT 0,
        [TotalSizeBytes] BIGINT NOT NULL DEFAULT 0,
        [LastUpdated] DATETIME2(7) NOT NULL DEFAULT GETUTCDATE(),
        [PeriodStart] DATETIME2(7) NOT NULL,
        [PeriodEnd] DATETIME2(7) NOT NULL,
        [Metadata] NVARCHAR(MAX) NULL,
        [CreatedDate] DATETIME2(7) NOT NULL DEFAULT GETUTCDATE(),
        
        CONSTRAINT [PK_LLMCacheStatistics] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [CK_LLMCacheStatistics_HitRate] CHECK ([HitRate] BETWEEN 0 AND 1),
        CONSTRAINT [CK_LLMCacheStatistics_Counts] CHECK ([HitCount] >= 0 AND [MissCount] >= 0 AND [SetCount] >= 0 AND [DeleteCount] >= 0)
    )
    
    -- Create indexes
    CREATE NONCLUSTERED INDEX [IX_LLMCacheStatistics_CacheType] ON [dbo].[LLMCacheStatistics] ([CacheType])
    CREATE NONCLUSTERED INDEX [IX_LLMCacheStatistics_LastUpdated] ON [dbo].[LLMCacheStatistics] ([LastUpdated])
    CREATE NONCLUSTERED INDEX [IX_LLMCacheStatistics_Period] ON [dbo].[LLMCacheStatistics] ([PeriodStart], [PeriodEnd])
    
    PRINT 'Created LLMCacheStatistics table with indexes'
END
ELSE
    PRINT 'LLMCacheStatistics table already exists'
GO

-- =====================================================
-- 6. LLM Cache Configuration Table
-- =====================================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[LLMCacheConfiguration]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[LLMCacheConfiguration] (
        [Id] BIGINT IDENTITY(1,1) NOT NULL,
        [CacheType] NVARCHAR(100) NOT NULL,
        [MaxSize] BIGINT NOT NULL DEFAULT 1000,
        [TTLSeconds] INT NOT NULL DEFAULT 3600,
        [EvictionPolicy] NVARCHAR(50) NOT NULL DEFAULT 'LRU',
        [CompressionEnabled] BIT NOT NULL DEFAULT 0,
        [EncryptionEnabled] BIT NOT NULL DEFAULT 0,
        [WarmupEnabled] BIT NOT NULL DEFAULT 0,
        [WarmupQuery] NVARCHAR(MAX) NULL,
        [IsActive] BIT NOT NULL DEFAULT 1,
        [Configuration] NVARCHAR(MAX) NULL,
        [CreatedDate] DATETIME2(7) NOT NULL DEFAULT GETUTCDATE(),
        [UpdatedDate] DATETIME2(7) NULL,
        
        CONSTRAINT [PK_LLMCacheConfiguration] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [CK_LLMCacheConfiguration_MaxSize] CHECK ([MaxSize] > 0),
        CONSTRAINT [CK_LLMCacheConfiguration_TTLSeconds] CHECK ([TTLSeconds] > 0)
    )
    
    -- Create indexes
    CREATE UNIQUE NONCLUSTERED INDEX [IX_LLMCacheConfiguration_CacheType] ON [dbo].[LLMCacheConfiguration] ([CacheType])
    CREATE NONCLUSTERED INDEX [IX_LLMCacheConfiguration_IsActive] ON [dbo].[LLMCacheConfiguration] ([IsActive])
    
    PRINT 'Created LLMCacheConfiguration table with indexes'
END
ELSE
    PRINT 'LLMCacheConfiguration table already exists'
GO

-- =====================================================
-- 7. LLM Resource Quotas Table
-- =====================================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[LLMResourceQuotas]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[LLMResourceQuotas] (
        [Id] BIGINT IDENTITY(1,1) NOT NULL,
        [UserId] NVARCHAR(256) NOT NULL,  -- Fixed: Match Users.Id data type
        [ResourceType] NVARCHAR(100) NOT NULL,
        [MaxQuantity] INT NOT NULL,
        [CurrentUsage] INT NOT NULL DEFAULT 0,
        [PeriodSeconds] INT NOT NULL DEFAULT 86400, -- 24 hours
        [ResetDate] DATETIME2(7) NOT NULL,
        [IsActive] BIT NOT NULL DEFAULT 1,
        [CreatedDate] DATETIME2(7) NOT NULL DEFAULT GETUTCDATE(),
        [UpdatedDate] DATETIME2(7) NULL,

        CONSTRAINT [PK_LLMResourceQuotas] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [CK_LLMResourceQuotas_MaxQuantity] CHECK ([MaxQuantity] > 0),
        CONSTRAINT [CK_LLMResourceQuotas_CurrentUsage] CHECK ([CurrentUsage] >= 0),
        CONSTRAINT [CK_LLMResourceQuotas_PeriodSeconds] CHECK ([PeriodSeconds] > 0)
    )

    -- Add foreign key constraint separately
    IF EXISTS (SELECT * FROM sys.tables WHERE name = 'Users')
    BEGIN
        ALTER TABLE [dbo].[LLMResourceQuotas]
        ADD CONSTRAINT [FK_LLMResourceQuotas_Users]
        FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users]([Id]) ON DELETE CASCADE
    END

    -- Create indexes
    CREATE NONCLUSTERED INDEX [IX_LLMResourceQuotas_UserId] ON [dbo].[LLMResourceQuotas] ([UserId])
    CREATE NONCLUSTERED INDEX [IX_LLMResourceQuotas_ResourceType] ON [dbo].[LLMResourceQuotas] ([ResourceType])
    CREATE NONCLUSTERED INDEX [IX_LLMResourceQuotas_ResetDate] ON [dbo].[LLMResourceQuotas] ([ResetDate])
    CREATE NONCLUSTERED INDEX [IX_LLMResourceQuotas_IsActive] ON [dbo].[LLMResourceQuotas] ([IsActive])
    CREATE UNIQUE NONCLUSTERED INDEX [IX_LLMResourceQuotas_UserResource] ON [dbo].[LLMResourceQuotas] ([UserId], [ResourceType]) WHERE [IsActive] = 1

    PRINT 'Created LLMResourceQuotas table with indexes'
END
ELSE
    PRINT 'LLMResourceQuotas table already exists'
GO

-- =====================================================
-- 8. LLM Cost Predictions Table
-- =====================================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[LLMCostPredictions]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[LLMCostPredictions] (
        [Id] BIGINT IDENTITY(1,1) NOT NULL,
        [QueryId] NVARCHAR(256) NOT NULL,  -- Fixed: Match Users.Id data type
        [UserId] NVARCHAR(256) NOT NULL,   -- Fixed: Match Users.Id data type
        [PredictedCost] DECIMAL(18,6) NOT NULL,
        [ActualCost] DECIMAL(18,6) NULL,
        [PredictionAccuracy] DECIMAL(5,4) NULL,
        [ModelUsed] NVARCHAR(100) NOT NULL,
        [InputFeatures] NVARCHAR(MAX) NULL,
        [PredictionMetadata] NVARCHAR(MAX) NULL,
        [CreatedDate] DATETIME2(7) NOT NULL DEFAULT GETUTCDATE(),
        [UpdatedDate] DATETIME2(7) NULL,

        CONSTRAINT [PK_LLMCostPredictions] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [CK_LLMCostPredictions_PredictedCost] CHECK ([PredictedCost] >= 0),
        CONSTRAINT [CK_LLMCostPredictions_ActualCost] CHECK ([ActualCost] IS NULL OR [ActualCost] >= 0),
        CONSTRAINT [CK_LLMCostPredictions_Accuracy] CHECK ([PredictionAccuracy] IS NULL OR [PredictionAccuracy] BETWEEN 0 AND 1)
    )

    -- Add foreign key constraint separately
    IF EXISTS (SELECT * FROM sys.tables WHERE name = 'Users')
    BEGIN
        ALTER TABLE [dbo].[LLMCostPredictions]
        ADD CONSTRAINT [FK_LLMCostPredictions_Users]
        FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users]([Id]) ON DELETE CASCADE
    END

    -- Create indexes
    CREATE NONCLUSTERED INDEX [IX_LLMCostPredictions_QueryId] ON [dbo].[LLMCostPredictions] ([QueryId])
    CREATE NONCLUSTERED INDEX [IX_LLMCostPredictions_UserId] ON [dbo].[LLMCostPredictions] ([UserId])
    CREATE NONCLUSTERED INDEX [IX_LLMCostPredictions_ModelUsed] ON [dbo].[LLMCostPredictions] ([ModelUsed])
    CREATE NONCLUSTERED INDEX [IX_LLMCostPredictions_CreatedDate] ON [dbo].[LLMCostPredictions] ([CreatedDate])

    PRINT 'Created LLMCostPredictions table with indexes'
END
ELSE
    PRINT 'LLMCostPredictions table already exists'
GO

-- =====================================================
-- 9. Cost Optimization Recommendations Table
-- =====================================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CostOptimizationRecommendations]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[CostOptimizationRecommendations] (
        [Id] BIGINT IDENTITY(1,1) NOT NULL,
        [Type] NVARCHAR(100) NOT NULL,
        [Title] NVARCHAR(200) NOT NULL,
        [Description] NVARCHAR(MAX) NOT NULL,
        [PotentialSavings] DECIMAL(18,2) NOT NULL DEFAULT 0,
        [ImpactScore] DECIMAL(3,2) NOT NULL DEFAULT 0,
        [Priority] NVARCHAR(20) NOT NULL DEFAULT 'Medium',
        [Implementation] NVARCHAR(MAX) NULL,
        [Benefits] NVARCHAR(MAX) NULL,
        [Risks] NVARCHAR(MAX) NULL,
        [IsImplemented] BIT NOT NULL DEFAULT 0,
        [ImplementedDate] DATETIME2(7) NULL,
        [CreatedDate] DATETIME2(7) NOT NULL DEFAULT GETUTCDATE(),
        [UpdatedDate] DATETIME2(7) NULL,

        CONSTRAINT [PK_CostOptimizationRecommendations] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [CK_CostOptimizationRecommendations_PotentialSavings] CHECK ([PotentialSavings] >= 0),
        CONSTRAINT [CK_CostOptimizationRecommendations_ImpactScore] CHECK ([ImpactScore] BETWEEN 0 AND 1),
        CONSTRAINT [CK_CostOptimizationRecommendations_Priority] CHECK ([Priority] IN ('Low', 'Medium', 'High', 'Critical'))
    )

    -- Create indexes
    CREATE NONCLUSTERED INDEX [IX_CostOptimizationRecommendations_Type] ON [dbo].[CostOptimizationRecommendations] ([Type])
    CREATE NONCLUSTERED INDEX [IX_CostOptimizationRecommendations_Priority] ON [dbo].[CostOptimizationRecommendations] ([Priority])
    CREATE NONCLUSTERED INDEX [IX_CostOptimizationRecommendations_IsImplemented] ON [dbo].[CostOptimizationRecommendations] ([IsImplemented])
    CREATE NONCLUSTERED INDEX [IX_CostOptimizationRecommendations_ImpactScore] ON [dbo].[CostOptimizationRecommendations] ([ImpactScore] DESC)
    CREATE NONCLUSTERED INDEX [IX_CostOptimizationRecommendations_CreatedDate] ON [dbo].[CostOptimizationRecommendations] ([CreatedDate])

    PRINT 'Created CostOptimizationRecommendations table with indexes'
END
ELSE
    PRINT 'CostOptimizationRecommendations table already exists'
GO

-- =====================================================
-- 10. Insert Default Cache Configurations
-- =====================================================
IF NOT EXISTS (SELECT 1 FROM [dbo].[LLMCacheConfiguration] WHERE [CacheType] = 'SemanticCache')
BEGIN
    INSERT INTO [dbo].[LLMCacheConfiguration] ([CacheType], [MaxSize], [TTLSeconds], [EvictionPolicy], [CompressionEnabled], [EncryptionEnabled], [WarmupEnabled])
    VALUES
        ('SemanticCache', 10000, 3600, 'LRU', 1, 0, 1),
        ('QueryCache', 5000, 1800, 'LRU', 1, 0, 0),
        ('ResultCache', 2000, 900, 'LFU', 1, 0, 0),
        ('MetadataCache', 1000, 7200, 'LRU', 0, 0, 1),
        ('UserSessionCache', 500, 1800, 'TTL', 0, 1, 0)

    PRINT 'Inserted default LLM cache configurations'
END
ELSE
    PRINT 'Default LLM cache configurations already exist'
GO

-- =====================================================
-- 11. Create Views for Cost Analytics
-- =====================================================
IF NOT EXISTS (SELECT * FROM sys.views WHERE name = 'vw_LLMCostAnalyticsSummary')
BEGIN
    EXEC('
    CREATE VIEW [dbo].[vw_LLMCostAnalyticsSummary]
    AS
    SELECT
        ct.UserId,
        COUNT(*) as TotalQueries,
        SUM(ct.Cost) as TotalCost,
        AVG(ct.Cost) as AverageCost,
        SUM(ct.TotalTokens) as TotalTokens,
        AVG(ct.DurationMs) as AverageResponseTime,
        MIN(ct.CreatedDate) as FirstQuery,
        MAX(ct.CreatedDate) as LastQuery,
        CAST(GETUTCDATE() as DATE) as AnalysisDate
    FROM [dbo].[LLMCostTracking] ct
    WHERE ct.CreatedDate >= CAST(GETUTCDATE() as DATE)
    GROUP BY ct.UserId
    ')

    PRINT 'Created vw_LLMCostAnalyticsSummary view'
END
ELSE
    PRINT 'vw_LLMCostAnalyticsSummary view already exists'
GO

PRINT '============================================='
PRINT 'FIXED Cost Optimization Migration Complete'
PRINT '============================================='
