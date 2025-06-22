-- =====================================================
-- COST MANAGEMENT COMPLETE SETUP SCRIPT
-- =====================================================
-- This script creates all cost management tables and seed data
-- Run this script to fix the CostTracking table errors
-- Database: BIReportingCopilot_dev
-- =====================================================

USE [BIReportingCopilot_dev]
GO

BEGIN TRANSACTION;

BEGIN TRY
    PRINT 'Starting Cost Management Complete Setup...';

    -- =====================================================
    -- 1. LLM Cost Tracking Table
    -- =====================================================
    IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[LLMCostTracking]') AND type in (N'U'))
    BEGIN
        CREATE TABLE [dbo].[LLMCostTracking] (
            [Id] BIGINT IDENTITY(1,1) NOT NULL,
            [UserId] NVARCHAR(256) NOT NULL,
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
            [RequestType] NVARCHAR(50) NULL,
            [RequestId] NVARCHAR(256) NULL,
            [Department] NVARCHAR(100) NULL,
            [Project] NVARCHAR(100) NULL,
            [Metadata] NVARCHAR(MAX) NULL,
            [CreatedDate] DATETIME2(7) NOT NULL DEFAULT GETUTCDATE(),
            [UpdatedDate] DATETIME2(7) NULL,
            [CreatedBy] NVARCHAR(256) NULL,
            [UpdatedBy] NVARCHAR(256) NULL,
            [Timestamp] DATETIME2(7) NOT NULL DEFAULT GETUTCDATE(),
            CONSTRAINT [PK_LLMCostTracking] PRIMARY KEY CLUSTERED ([Id] ASC)
        );

        CREATE NONCLUSTERED INDEX [IX_LLMCostTracking_UserId] ON [dbo].[LLMCostTracking] ([UserId] ASC);
        CREATE NONCLUSTERED INDEX [IX_LLMCostTracking_Timestamp] ON [dbo].[LLMCostTracking] ([Timestamp] ASC);
        CREATE NONCLUSTERED INDEX [IX_LLMCostTracking_Provider_Model] ON [dbo].[LLMCostTracking] ([ProviderId] ASC, [ModelId] ASC);
        CREATE NONCLUSTERED INDEX [IX_LLMCostTracking_QueryId] ON [dbo].[LLMCostTracking] ([QueryId] ASC);

        PRINT 'Created LLMCostTracking table with indexes';
    END
    ELSE
        PRINT 'LLMCostTracking table already exists';

    -- =====================================================
    -- 2. LLM Budget Management Table
    -- =====================================================
    IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[LLMBudgetManagement]') AND type in (N'U'))
    BEGIN
        CREATE TABLE [dbo].[LLMBudgetManagement] (
            [Id] BIGINT IDENTITY(1,1) NOT NULL,
            [Name] NVARCHAR(200) NOT NULL,
            [Type] NVARCHAR(50) NOT NULL DEFAULT 'User',
            [EntityId] NVARCHAR(256) NOT NULL,
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
            [CreatedBy] NVARCHAR(256) NULL,
            [UpdatedBy] NVARCHAR(256) NULL,
            CONSTRAINT [PK_LLMBudgetManagement] PRIMARY KEY CLUSTERED ([Id] ASC)
        );

        CREATE NONCLUSTERED INDEX [IX_LLMBudgetManagement_EntityId] ON [dbo].[LLMBudgetManagement] ([EntityId] ASC);
        CREATE NONCLUSTERED INDEX [IX_LLMBudgetManagement_Type_EntityId] ON [dbo].[LLMBudgetManagement] ([Type] ASC, [EntityId] ASC);

        PRINT 'Created LLMBudgetManagement table with indexes';
    END
    ELSE
        PRINT 'LLMBudgetManagement table already exists';

    -- =====================================================
    -- 3. Resource Usage Table
    -- =====================================================
    IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ResourceUsage]') AND type in (N'U'))
    BEGIN
        CREATE TABLE [dbo].[ResourceUsage] (
            [Id] BIGINT IDENTITY(1,1) NOT NULL,
            [UserId] NVARCHAR(256) NOT NULL,
            [ResourceType] NVARCHAR(50) NOT NULL,
            [ResourceId] NVARCHAR(256) NULL,
            [ResourceName] NVARCHAR(100) NULL,
            [Quantity] INT NOT NULL DEFAULT 0,
            [Unit] NVARCHAR(20) NOT NULL DEFAULT 'Count',
            [DurationMs] BIGINT NOT NULL DEFAULT 0,
            [Cost] DECIMAL(18,8) NOT NULL DEFAULT 0,
            [Timestamp] DATETIME2(7) NOT NULL DEFAULT GETUTCDATE(),
            [RequestId] NVARCHAR(256) NULL,
            [Tags] NVARCHAR(500) NULL,
            [Metadata] NVARCHAR(MAX) NULL,
            [CreatedDate] DATETIME2(7) NOT NULL DEFAULT GETUTCDATE(),
            [UpdatedDate] DATETIME2(7) NULL,
            [CreatedBy] NVARCHAR(256) NULL,
            [UpdatedBy] NVARCHAR(256) NULL,
            CONSTRAINT [PK_ResourceUsage] PRIMARY KEY CLUSTERED ([Id] ASC)
        );

        CREATE NONCLUSTERED INDEX [IX_ResourceUsage_UserId] ON [dbo].[ResourceUsage] ([UserId] ASC);
        CREATE NONCLUSTERED INDEX [IX_ResourceUsage_Timestamp] ON [dbo].[ResourceUsage] ([Timestamp] ASC);
        CREATE NONCLUSTERED INDEX [IX_ResourceUsage_ResourceType] ON [dbo].[ResourceUsage] ([ResourceType] ASC);

        PRINT 'Created ResourceUsage table with indexes';
    END
    ELSE
        PRINT 'ResourceUsage table already exists';

    -- =====================================================
    -- 4. Cost Predictions Table
    -- =====================================================
    IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CostPredictions]') AND type in (N'U'))
    BEGIN
        CREATE TABLE [dbo].[CostPredictions] (
            [Id] BIGINT IDENTITY(1,1) NOT NULL,
            [QueryId] NVARCHAR(256) NOT NULL,
            [UserId] NVARCHAR(256) NOT NULL,
            [ModelUsed] NVARCHAR(100) NULL,
            [EstimatedCost] DECIMAL(18,8) NOT NULL,
            [ActualCost] DECIMAL(18,8) NULL,
            [ConfidenceScore] DECIMAL(3,2) NULL,
            [PredictionAccuracy] DECIMAL(5,4) NULL,
            [Factors] NVARCHAR(MAX) NULL,
            [CreatedDate] DATETIME2(7) NOT NULL DEFAULT GETUTCDATE(),
            [UpdatedDate] DATETIME2(7) NULL,
            [CreatedBy] NVARCHAR(256) NULL,
            [UpdatedBy] NVARCHAR(256) NULL,
            CONSTRAINT [PK_CostPredictions] PRIMARY KEY CLUSTERED ([Id] ASC)
        );

        CREATE NONCLUSTERED INDEX [IX_CostPredictions_QueryId] ON [dbo].[CostPredictions] ([QueryId] ASC);
        CREATE NONCLUSTERED INDEX [IX_CostPredictions_UserId] ON [dbo].[CostPredictions] ([UserId] ASC);

        PRINT 'Created CostPredictions table with indexes';
    END
    ELSE
        PRINT 'CostPredictions table already exists';

    -- =====================================================
    -- 5. Cost Optimization Recommendations Table
    -- =====================================================
    IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CostOptimizationRecommendations]') AND type in (N'U'))
    BEGIN
        CREATE TABLE [dbo].[CostOptimizationRecommendations] (
            [Id] BIGINT IDENTITY(1,1) NOT NULL,
            [Type] NVARCHAR(50) NOT NULL,
            [Title] NVARCHAR(200) NOT NULL,
            [Description] NVARCHAR(MAX) NOT NULL,
            [Priority] NVARCHAR(20) NOT NULL DEFAULT 'Medium',
            [PotentialSavings] DECIMAL(18,2) NOT NULL DEFAULT 0,
            [ImpactScore] DECIMAL(3,2) NOT NULL DEFAULT 0,
            [Implementation] NVARCHAR(MAX) NULL,
            [Benefits] NVARCHAR(MAX) NULL,
            [Risks] NVARCHAR(MAX) NULL,
            [IsImplemented] BIT NOT NULL DEFAULT 0,
            [ImplementedDate] DATETIME2(7) NULL,
            [CreatedDate] DATETIME2(7) NOT NULL DEFAULT GETUTCDATE(),
            [UpdatedDate] DATETIME2(7) NULL,
            [CreatedBy] NVARCHAR(256) NULL,
            [UpdatedBy] NVARCHAR(256) NULL,
            CONSTRAINT [PK_CostOptimizationRecommendations] PRIMARY KEY CLUSTERED ([Id] ASC)
        );

        CREATE NONCLUSTERED INDEX [IX_CostOptimizationRecommendations_Type] ON [dbo].[CostOptimizationRecommendations] ([Type] ASC);
        CREATE NONCLUSTERED INDEX [IX_CostOptimizationRecommendations_Priority] ON [dbo].[CostOptimizationRecommendations] ([Priority] ASC);

        PRINT 'Created CostOptimizationRecommendations table with indexes';
    END
    ELSE
        PRINT 'CostOptimizationRecommendations table already exists';

    -- =====================================================
    -- 6. Insert Sample Data
    -- =====================================================
    
    -- Get admin user ID
    DECLARE @AdminUserId NVARCHAR(256) = (SELECT TOP 1 Id FROM Users WHERE Email LIKE '%admin%' OR UserName LIKE '%admin%');
    
    IF @AdminUserId IS NULL
        SET @AdminUserId = 'admin-user-001';

    PRINT 'Using admin user ID: ' + ISNULL(@AdminUserId, 'NULL');

    -- Sample Cost Tracking Data
    IF NOT EXISTS (SELECT 1 FROM [dbo].[LLMCostTracking])
    BEGIN
        -- Check if RequestType column exists before using it
        IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('[dbo].[LLMCostTracking]') AND name = 'RequestType')
        BEGIN
            INSERT INTO [dbo].[LLMCostTracking]
            ([UserId], [QueryId], [ProviderId], [ModelId], [Cost], [CostPerToken], [InputTokens], [OutputTokens], [TotalTokens], [DurationMs], [Category], [RequestType])
            VALUES
            (@AdminUserId, 'query-001', 'openai', 'gpt-4', 0.0234, 0.00003, 780, 156, 936, 2340, 'Query', 'chat'),
            (@AdminUserId, 'query-002', 'openai', 'gpt-3.5-turbo', 0.0089, 0.000015, 592, 98, 690, 1890, 'Query', 'completion'),
            (@AdminUserId, 'query-003', 'azure', 'gpt-4', 0.0198, 0.000028, 708, 142, 850, 2100, 'Analysis', 'chat'),
            (@AdminUserId, 'query-004', 'openai', 'gpt-4', 0.0267, 0.00003, 890, 178, 1068, 2890, 'Report', 'completion'),
            (@AdminUserId, 'query-005', 'azure', 'gpt-3.5-turbo', 0.0076, 0.000012, 634, 89, 723, 1560, 'Query', 'chat');
        END
        ELSE
        BEGIN
            -- Insert without RequestType column if it doesn't exist
            INSERT INTO [dbo].[LLMCostTracking]
            ([UserId], [QueryId], [ProviderId], [ModelId], [Cost], [CostPerToken], [InputTokens], [OutputTokens], [TotalTokens], [DurationMs], [Category])
            VALUES
            (@AdminUserId, 'query-001', 'openai', 'gpt-4', 0.0234, 0.00003, 780, 156, 936, 2340, 'Query'),
            (@AdminUserId, 'query-002', 'openai', 'gpt-3.5-turbo', 0.0089, 0.000015, 592, 98, 690, 1890, 'Query'),
            (@AdminUserId, 'query-003', 'azure', 'gpt-4', 0.0198, 0.000028, 708, 142, 850, 2100, 'Analysis'),
            (@AdminUserId, 'query-004', 'openai', 'gpt-4', 0.0267, 0.00003, 890, 178, 1068, 2890, 'Report'),
            (@AdminUserId, 'query-005', 'azure', 'gpt-3.5-turbo', 0.0076, 0.000012, 634, 89, 723, 1560, 'Query');
        END

        PRINT 'Inserted sample cost tracking data';
    END

    -- Sample Budget Data
    IF NOT EXISTS (SELECT 1 FROM [dbo].[LLMBudgetManagement])
    BEGIN
        INSERT INTO [dbo].[LLMBudgetManagement]
        ([Name], [Type], [EntityId], [BudgetAmount], [SpentAmount], [RemainingAmount], [Period], [StartDate], [EndDate], [AlertThreshold], [BlockThreshold])
        VALUES 
        ('Monthly AI Budget', 'User', @AdminUserId, 1000.00, 156.78, 843.22, 'Monthly', 
         DATEFROMPARTS(YEAR(GETUTCDATE()), MONTH(GETUTCDATE()), 1), 
         EOMONTH(GETUTCDATE()), 0.80, 0.95),
        ('Weekly Development Budget', 'User', @AdminUserId, 250.00, 67.45, 182.55, 'Weekly',
         DATEADD(day, -(DATEPART(weekday, GETUTCDATE()) - 1), CAST(GETUTCDATE() AS DATE)),
         DATEADD(day, 7 - DATEPART(weekday, GETUTCDATE()), CAST(GETUTCDATE() AS DATE)), 0.75, 0.90);

        PRINT 'Inserted sample budget data';
    END

    -- Sample Optimization Recommendations
    IF NOT EXISTS (SELECT 1 FROM [dbo].[CostOptimizationRecommendations])
    BEGIN
        INSERT INTO [dbo].[CostOptimizationRecommendations]
        ([Type], [Title], [Description], [Priority], [PotentialSavings], [ImpactScore], [Implementation], [Benefits])
        VALUES 
        ('Token Optimization', 'Reduce Context Window Size', 'Optimize prompt context to reduce token usage by 15-20%', 'High', 45.67, 0.85, 
         'Implement smart context trimming and relevance filtering', 'Lower costs, faster responses, maintained accuracy'),
        ('Model Selection', 'Use GPT-3.5 for Simple Queries', 'Route simple queries to more cost-effective models', 'Medium', 23.45, 0.72,
         'Implement query complexity analysis and model routing', 'Significant cost reduction for routine queries'),
        ('Caching Strategy', 'Implement Semantic Caching', 'Cache similar query results to avoid redundant API calls', 'High', 67.89, 0.91,
         'Deploy semantic similarity matching for query results', 'Reduced API calls, faster responses, lower costs');

        PRINT 'Inserted sample optimization recommendations';
    END

    COMMIT TRANSACTION;
    PRINT 'Cost Management Complete Setup completed successfully!';
    PRINT '';
    PRINT '✅ All cost management tables created and populated with sample data';
    PRINT '✅ CostTracking table errors should now be resolved';
    PRINT '✅ Cost management endpoints should work properly';

END TRY
BEGIN CATCH
    ROLLBACK TRANSACTION;
    
    DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
    DECLARE @ErrorSeverity INT = ERROR_SEVERITY();
    DECLARE @ErrorState INT = ERROR_STATE();
    
    PRINT 'Error occurred during Cost Management setup:';
    PRINT @ErrorMessage;
    
    RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
END CATCH
