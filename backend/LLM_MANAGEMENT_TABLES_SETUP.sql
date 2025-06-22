-- =====================================================
-- LLM MANAGEMENT TABLES SETUP SCRIPT
-- =====================================================
-- This script creates all missing LLM management tables
-- Run this script to fix the 404 errors for LLM endpoints
-- Database: BIReportingCopilot_dev
-- =====================================================

USE [BIReportingCopilot_dev]
GO

BEGIN TRANSACTION;

BEGIN TRY
    PRINT 'Starting LLM Management Tables Setup...';

    -- =====================================================
    -- 1. LLM Provider Configs Table
    -- =====================================================
    IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[LLMProviderConfigs]') AND type in (N'U'))
    BEGIN
        CREATE TABLE [dbo].[LLMProviderConfigs] (
            [ProviderId] NVARCHAR(50) NOT NULL,
            [Name] NVARCHAR(100) NOT NULL,
            [Type] NVARCHAR(50) NOT NULL,
            [ApiKey] NVARCHAR(500) NULL,
            [Endpoint] NVARCHAR(500) NULL,
            [Organization] NVARCHAR(100) NULL,
            [IsEnabled] BIT NOT NULL DEFAULT 1,
            [IsDefault] BIT NOT NULL DEFAULT 0,
            [Settings] NVARCHAR(MAX) NOT NULL DEFAULT '{}',
            [CreatedAt] DATETIME2(7) NOT NULL DEFAULT GETUTCDATE(),
            [UpdatedAt] DATETIME2(7) NOT NULL DEFAULT GETUTCDATE(),
            CONSTRAINT [PK_LLMProviderConfigs] PRIMARY KEY CLUSTERED ([ProviderId] ASC)
        );

        CREATE NONCLUSTERED INDEX [IX_LLMProviderConfigs_IsEnabled] ON [dbo].[LLMProviderConfigs] ([IsEnabled] ASC);
        CREATE NONCLUSTERED INDEX [IX_LLMProviderConfigs_Type] ON [dbo].[LLMProviderConfigs] ([Type] ASC);

        PRINT 'Created LLMProviderConfigs table with indexes';
    END
    ELSE
        PRINT 'LLMProviderConfigs table already exists';

    -- =====================================================
    -- 2. LLM Model Configs Table
    -- =====================================================
    IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[LLMModelConfigs]') AND type in (N'U'))
    BEGIN
        CREATE TABLE [dbo].[LLMModelConfigs] (
            [ModelId] NVARCHAR(100) NOT NULL,
            [ProviderId] NVARCHAR(50) NOT NULL,
            [Name] NVARCHAR(100) NOT NULL,
            [DisplayName] NVARCHAR(150) NOT NULL,
            [Temperature] REAL NOT NULL DEFAULT 0.7,
            [MaxTokens] INT NOT NULL DEFAULT 4000,
            [TopP] REAL NOT NULL DEFAULT 1.0,
            [FrequencyPenalty] REAL NOT NULL DEFAULT 0.0,
            [PresencePenalty] REAL NOT NULL DEFAULT 0.0,
            [IsEnabled] BIT NOT NULL DEFAULT 1,
            [UseCase] NVARCHAR(50) NULL,
            [CostPerToken] DECIMAL(18,8) NOT NULL DEFAULT 0.0,
            [Capabilities] NVARCHAR(MAX) NOT NULL DEFAULT '{}',
            [CreatedAt] DATETIME2(7) NOT NULL DEFAULT GETUTCDATE(),
            [UpdatedAt] DATETIME2(7) NOT NULL DEFAULT GETUTCDATE(),
            CONSTRAINT [PK_LLMModelConfigs] PRIMARY KEY CLUSTERED ([ModelId] ASC),
            CONSTRAINT [FK_LLMModelConfigs_LLMProviderConfigs] FOREIGN KEY ([ProviderId]) 
                REFERENCES [dbo].[LLMProviderConfigs] ([ProviderId]) ON DELETE CASCADE
        );

        CREATE NONCLUSTERED INDEX [IX_LLMModelConfigs_ProviderId] ON [dbo].[LLMModelConfigs] ([ProviderId] ASC);
        CREATE NONCLUSTERED INDEX [IX_LLMModelConfigs_IsEnabled] ON [dbo].[LLMModelConfigs] ([IsEnabled] ASC);
        CREATE NONCLUSTERED INDEX [IX_LLMModelConfigs_UseCase] ON [dbo].[LLMModelConfigs] ([UseCase] ASC);

        PRINT 'Created LLMModelConfigs table with indexes';
    END
    ELSE
        PRINT 'LLMModelConfigs table already exists';

    -- =====================================================
    -- 3. LLM Usage Logs Table
    -- =====================================================
    IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[LLMUsageLogs]') AND type in (N'U'))
    BEGIN
        CREATE TABLE [dbo].[LLMUsageLogs] (
            [Id] BIGINT IDENTITY(1,1) NOT NULL,
            [RequestId] NVARCHAR(100) NOT NULL,
            [UserId] NVARCHAR(256) NOT NULL,
            [ProviderId] NVARCHAR(50) NOT NULL,
            [ModelId] NVARCHAR(100) NOT NULL,
            [RequestType] NVARCHAR(50) NOT NULL,
            [RequestText] NVARCHAR(MAX) NOT NULL,
            [ResponseText] NVARCHAR(MAX) NOT NULL,
            [InputTokens] INT NOT NULL DEFAULT 0,
            [OutputTokens] INT NOT NULL DEFAULT 0,
            [TotalTokens] INT NOT NULL DEFAULT 0,
            [Cost] DECIMAL(18,8) NOT NULL DEFAULT 0.0,
            [DurationMs] BIGINT NOT NULL DEFAULT 0,
            [Success] BIT NOT NULL DEFAULT 1,
            [ErrorMessage] NVARCHAR(1000) NULL,
            [Timestamp] DATETIME2(7) NOT NULL DEFAULT GETUTCDATE(),
            [Metadata] NVARCHAR(MAX) NULL,
            CONSTRAINT [PK_LLMUsageLogs] PRIMARY KEY CLUSTERED ([Id] ASC)
        );

        CREATE NONCLUSTERED INDEX [IX_LLMUsageLogs_UserId] ON [dbo].[LLMUsageLogs] ([UserId] ASC);
        CREATE NONCLUSTERED INDEX [IX_LLMUsageLogs_ProviderId] ON [dbo].[LLMUsageLogs] ([ProviderId] ASC);
        CREATE NONCLUSTERED INDEX [IX_LLMUsageLogs_ModelId] ON [dbo].[LLMUsageLogs] ([ModelId] ASC);
        CREATE NONCLUSTERED INDEX [IX_LLMUsageLogs_Timestamp] ON [dbo].[LLMUsageLogs] ([Timestamp] ASC);
        CREATE NONCLUSTERED INDEX [IX_LLMUsageLogs_RequestType] ON [dbo].[LLMUsageLogs] ([RequestType] ASC);

        PRINT 'Created LLMUsageLogs table with indexes';
    END
    ELSE
        PRINT 'LLMUsageLogs table already exists';

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
            CONSTRAINT [PK_LLMPerformanceMetrics] PRIMARY KEY CLUSTERED ([Id] ASC)
        );

        CREATE NONCLUSTERED INDEX [IX_LLMPerformanceMetrics_EntityType] ON [dbo].[LLMPerformanceMetrics] ([EntityType] ASC);
        CREATE NONCLUSTERED INDEX [IX_LLMPerformanceMetrics_EntityId] ON [dbo].[LLMPerformanceMetrics] ([EntityId] ASC);
        CREATE NONCLUSTERED INDEX [IX_LLMPerformanceMetrics_MetricName] ON [dbo].[LLMPerformanceMetrics] ([MetricName] ASC);
        CREATE NONCLUSTERED INDEX [IX_LLMPerformanceMetrics_LastUpdated] ON [dbo].[LLMPerformanceMetrics] ([LastUpdated] ASC);

        PRINT 'Created LLMPerformanceMetrics table with indexes';
    END
    ELSE
        PRINT 'LLMPerformanceMetrics table already exists';

    -- =====================================================
    -- 5. Insert Sample Data
    -- =====================================================
    
    -- Get admin user ID
    DECLARE @AdminUserId NVARCHAR(256) = (SELECT TOP 1 Id FROM Users WHERE Email LIKE '%admin%' OR UserName LIKE '%admin%');
    
    IF @AdminUserId IS NULL
        SET @AdminUserId = 'admin-user-001';

    PRINT 'Using admin user ID: ' + ISNULL(@AdminUserId, 'NULL');

    -- Sample Provider Data
    IF NOT EXISTS (SELECT 1 FROM [dbo].[LLMProviderConfigs])
    BEGIN
        INSERT INTO [dbo].[LLMProviderConfigs] 
        ([ProviderId], [Name], [Type], [ApiKey], [Endpoint], [IsEnabled], [IsDefault], [Settings])
        VALUES 
        ('openai', 'OpenAI', 'openai', 'sk-***CONFIGURED***', 'https://api.openai.com/v1', 1, 1, '{"timeout": 30000, "retries": 3}'),
        ('azure-openai', 'Azure OpenAI', 'azure', 'azure-***CONFIGURED***', 'https://your-resource.openai.azure.com/', 1, 0, '{"apiVersion": "2023-12-01-preview"}'),
        ('anthropic', 'Anthropic Claude', 'anthropic', 'sk-ant-***CONFIGURED***', 'https://api.anthropic.com', 0, 0, '{"version": "2023-06-01"}');

        PRINT 'Inserted sample provider data';
    END

    -- Sample Model Data
    IF NOT EXISTS (SELECT 1 FROM [dbo].[LLMModelConfigs])
    BEGIN
        INSERT INTO [dbo].[LLMModelConfigs]
        ([ModelId], [ProviderId], [Name], [DisplayName], [Temperature], [MaxTokens], [TopP], [FrequencyPenalty], [PresencePenalty], [IsEnabled], [UseCase], [CostPerToken], [Capabilities])
        VALUES 
        ('gpt-4', 'openai', 'gpt-4', 'GPT-4', 0.7, 8000, 1.0, 0.0, 0.0, 1, 'general', 0.00003, '{"reasoning": true, "coding": true, "analysis": true}'),
        ('gpt-3.5-turbo', 'openai', 'gpt-3.5-turbo', 'GPT-3.5 Turbo', 0.7, 4000, 1.0, 0.0, 0.0, 1, 'chat', 0.000002, '{"chat": true, "completion": true}'),
        ('gpt-4-azure', 'azure-openai', 'gpt-4', 'GPT-4 (Azure)', 0.7, 8000, 1.0, 0.0, 0.0, 1, 'enterprise', 0.00003, '{"reasoning": true, "enterprise": true}'),
        ('claude-3-sonnet', 'anthropic', 'claude-3-sonnet-20240229', 'Claude 3 Sonnet', 0.7, 4000, 1.0, 0.0, 0.0, 0, 'analysis', 0.000015, '{"analysis": true, "reasoning": true}');

        PRINT 'Inserted sample model data';
    END

    -- Sample Usage Logs
    IF NOT EXISTS (SELECT 1 FROM [dbo].[LLMUsageLogs])
    BEGIN
        INSERT INTO [dbo].[LLMUsageLogs]
        ([RequestId], [UserId], [ProviderId], [ModelId], [RequestType], [RequestText], [ResponseText], [InputTokens], [OutputTokens], [TotalTokens], [Cost], [DurationMs], [Success])
        VALUES 
        ('req-001', @AdminUserId, 'openai', 'gpt-4', 'sql_generation', 'Generate SQL for sales report', 'SELECT * FROM Sales WHERE Date >= ''2024-01-01''', 45, 23, 68, 0.00204, 2340, 1),
        ('req-002', @AdminUserId, 'openai', 'gpt-3.5-turbo', 'chat', 'Explain this data', 'This data shows sales trends over time...', 32, 18, 50, 0.0001, 1890, 1),
        ('req-003', @AdminUserId, 'azure-openai', 'gpt-4-azure', 'analysis', 'Analyze customer behavior', 'Based on the data, customers prefer...', 67, 45, 112, 0.00336, 3200, 1),
        ('req-004', @AdminUserId, 'openai', 'gpt-4', 'completion', 'Complete this query', 'SELECT customer_id, SUM(amount) FROM orders GROUP BY customer_id', 28, 19, 47, 0.00141, 1560, 1),
        ('req-005', @AdminUserId, 'openai', 'gpt-3.5-turbo', 'sql_generation', 'Show top products', 'SELECT product_name, COUNT(*) as sales FROM products...', 38, 25, 63, 0.000126, 2100, 1);

        PRINT 'Inserted sample usage logs';
    END

    -- Sample Performance Metrics
    IF NOT EXISTS (SELECT 1 FROM [dbo].[LLMPerformanceMetrics])
    BEGIN
        INSERT INTO [dbo].[LLMPerformanceMetrics]
        ([EntityType], [EntityId], [MetricName], [MetricValue], [MetricUnit], [AverageExecutionTime], [TotalOperations], [SuccessCount], [ErrorCount])
        VALUES 
        ('Provider', 'openai', 'ResponseTime', 2340.0, 'ms', 2340, 156, 152, 4),
        ('Provider', 'azure-openai', 'ResponseTime', 1890.0, 'ms', 1890, 89, 87, 2),
        ('Model', 'gpt-4', 'TokensPerSecond', 45.5, 'tokens/sec', 2200, 98, 96, 2),
        ('Model', 'gpt-3.5-turbo', 'TokensPerSecond', 67.8, 'tokens/sec', 1650, 78, 77, 1),
        ('Provider', 'openai', 'SuccessRate', 97.4, 'percentage', 2100, 156, 152, 4);

        PRINT 'Inserted sample performance metrics';
    END

    COMMIT TRANSACTION;
    PRINT 'LLM Management Tables Setup completed successfully!';
    PRINT '';
    PRINT '✅ All LLM management tables created and populated with sample data';
    PRINT '✅ LLM Management endpoints should now work properly';
    PRINT '✅ Frontend LLM Management dashboard should load correctly';

END TRY
BEGIN CATCH
    ROLLBACK TRANSACTION;
    
    DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
    DECLARE @ErrorSeverity INT = ERROR_SEVERITY();
    DECLARE @ErrorState INT = ERROR_STATE();
    
    PRINT 'Error occurred during LLM Management setup:';
    PRINT @ErrorMessage;
    
    RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
END CATCH
