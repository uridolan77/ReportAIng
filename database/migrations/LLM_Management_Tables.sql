-- =====================================================
-- LLM Management Tables Creation Script
-- =====================================================
-- This script creates the necessary tables for the LLM Management system
-- Run this script on your database to add LLM management functionality

-- =====================================================
-- 1. LLM Provider Configurations Table
-- =====================================================
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='LLMProviderConfigs' AND xtype='U')
BEGIN
    CREATE TABLE [dbo].[LLMProviderConfigs] (
        [ProviderId] NVARCHAR(50) NOT NULL PRIMARY KEY,
        [Name] NVARCHAR(100) NOT NULL,
        [Type] NVARCHAR(50) NOT NULL,
        [ApiKey] NVARCHAR(500) NULL,
        [Endpoint] NVARCHAR(500) NULL,
        [Organization] NVARCHAR(100) NULL,
        [IsEnabled] BIT NOT NULL DEFAULT 1,
        [IsDefault] BIT NOT NULL DEFAULT 0,
        [Settings] NVARCHAR(MAX) NULL, -- JSON data
        [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [UpdatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE()
    );

    -- Create indexes for LLMProviderConfigs
    CREATE INDEX [IX_LLMProviderConfigs_Name] ON [dbo].[LLMProviderConfigs] ([Name]);
    CREATE INDEX [IX_LLMProviderConfigs_IsEnabled_IsDefault] ON [dbo].[LLMProviderConfigs] ([IsEnabled], [IsDefault]);

    PRINT 'Created table: LLMProviderConfigs';
END
ELSE
BEGIN
    PRINT 'Table LLMProviderConfigs already exists';
END

-- =====================================================
-- 2. LLM Model Configurations Table
-- =====================================================
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='LLMModelConfigs' AND xtype='U')
BEGIN
    CREATE TABLE [dbo].[LLMModelConfigs] (
        [ModelId] NVARCHAR(100) NOT NULL PRIMARY KEY,
        [ProviderId] NVARCHAR(50) NOT NULL,
        [Name] NVARCHAR(100) NOT NULL,
        [DisplayName] NVARCHAR(150) NOT NULL,
        [Temperature] REAL NOT NULL DEFAULT 0.1,
        [MaxTokens] INT NOT NULL DEFAULT 2000,
        [TopP] REAL NOT NULL DEFAULT 1.0,
        [FrequencyPenalty] REAL NOT NULL DEFAULT 0.0,
        [PresencePenalty] REAL NOT NULL DEFAULT 0.0,
        [IsEnabled] BIT NOT NULL DEFAULT 1,
        [UseCase] NVARCHAR(50) NULL,
        [CostPerToken] DECIMAL(18,8) NOT NULL DEFAULT 0.0,
        [Capabilities] NVARCHAR(MAX) NULL, -- JSON data
        
        -- Foreign key constraint
        CONSTRAINT [FK_LLMModelConfigs_LLMProviderConfigs] 
            FOREIGN KEY ([ProviderId]) REFERENCES [dbo].[LLMProviderConfigs]([ProviderId]) 
            ON DELETE CASCADE
    );

    -- Create indexes for LLMModelConfigs
    CREATE INDEX [IX_LLMModelConfigs_ProviderId] ON [dbo].[LLMModelConfigs] ([ProviderId]);
    CREATE INDEX [IX_LLMModelConfigs_IsEnabled_UseCase] ON [dbo].[LLMModelConfigs] ([IsEnabled], [UseCase]);

    PRINT 'Created table: LLMModelConfigs';
END
ELSE
BEGIN
    PRINT 'Table LLMModelConfigs already exists';
END

-- =====================================================
-- 3. LLM Usage Logs Table
-- =====================================================
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='LLMUsageLogs' AND xtype='U')
BEGIN
    CREATE TABLE [dbo].[LLMUsageLogs] (
        [Id] BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
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
        [Timestamp] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [Metadata] NVARCHAR(MAX) NULL -- JSON data
    );

    -- Create indexes for LLMUsageLogs
    CREATE INDEX [IX_LLMUsageLogs_RequestId] ON [dbo].[LLMUsageLogs] ([RequestId]);
    CREATE INDEX [IX_LLMUsageLogs_UserId_Timestamp] ON [dbo].[LLMUsageLogs] ([UserId], [Timestamp] DESC);
    CREATE INDEX [IX_LLMUsageLogs_ProviderId_Timestamp] ON [dbo].[LLMUsageLogs] ([ProviderId], [Timestamp] DESC);
    CREATE INDEX [IX_LLMUsageLogs_ModelId_Timestamp] ON [dbo].[LLMUsageLogs] ([ModelId], [Timestamp] DESC);
    CREATE INDEX [IX_LLMUsageLogs_RequestType_Timestamp] ON [dbo].[LLMUsageLogs] ([RequestType], [Timestamp] DESC);
    CREATE INDEX [IX_LLMUsageLogs_Timestamp] ON [dbo].[LLMUsageLogs] ([Timestamp] DESC);

    PRINT 'Created table: LLMUsageLogs';
END
ELSE
BEGIN
    PRINT 'Table LLMUsageLogs already exists';
END

-- =====================================================
-- 4. Insert Sample Data (Optional)
-- =====================================================
-- Insert sample OpenAI provider configuration
IF NOT EXISTS (SELECT 1 FROM [dbo].[LLMProviderConfigs] WHERE [ProviderId] = 'openai-default')
BEGIN
    INSERT INTO [dbo].[LLMProviderConfigs] (
        [ProviderId], [Name], [Type], [ApiKey], [Endpoint], [Organization], 
        [IsEnabled], [IsDefault], [Settings], [CreatedAt], [UpdatedAt]
    ) VALUES (
        'openai-default', 
        'OpenAI Default', 
        'OpenAI', 
        NULL, -- API key should be configured through the UI
        'https://api.openai.com/v1', 
        NULL,
        0, -- Disabled by default until API key is configured
        1, -- Set as default
        '{}', -- Empty JSON settings
        GETUTCDATE(), 
        GETUTCDATE()
    );

    PRINT 'Inserted sample OpenAI provider configuration';
END

-- Insert sample Azure OpenAI provider configuration
IF NOT EXISTS (SELECT 1 FROM [dbo].[LLMProviderConfigs] WHERE [ProviderId] = 'azure-openai-default')
BEGIN
    INSERT INTO [dbo].[LLMProviderConfigs] (
        [ProviderId], [Name], [Type], [ApiKey], [Endpoint], [Organization], 
        [IsEnabled], [IsDefault], [Settings], [CreatedAt], [UpdatedAt]
    ) VALUES (
        'azure-openai-default', 
        'Azure OpenAI Default', 
        'AzureOpenAI', 
        NULL, -- API key should be configured through the UI
        NULL, -- Endpoint should be configured through the UI
        NULL,
        0, -- Disabled by default until configured
        0, -- Not default
        '{"apiVersion": "2023-12-01-preview"}', -- Default API version
        GETUTCDATE(), 
        GETUTCDATE()
    );

    PRINT 'Inserted sample Azure OpenAI provider configuration';
END

-- Insert sample model configurations for OpenAI
IF NOT EXISTS (SELECT 1 FROM [dbo].[LLMModelConfigs] WHERE [ModelId] = 'gpt-4-sql')
BEGIN
    INSERT INTO [dbo].[LLMModelConfigs] (
        [ModelId], [ProviderId], [Name], [DisplayName], [Temperature], [MaxTokens], 
        [TopP], [FrequencyPenalty], [PresencePenalty], [IsEnabled], [UseCase], 
        [CostPerToken], [Capabilities]
    ) VALUES (
        'gpt-4-sql', 
        'openai-default', 
        'gpt-4', 
        'GPT-4 for SQL Generation', 
        0.1, 
        2000, 
        1.0, 
        0.0, 
        0.0, 
        1, 
        'SQL', 
        0.00003, -- Approximate cost per token
        '{"supportsStreaming": true, "maxContextLength": 8192, "supportsFunctions": true}'
    );

    PRINT 'Inserted sample GPT-4 SQL model configuration';
END

IF NOT EXISTS (SELECT 1 FROM [dbo].[LLMModelConfigs] WHERE [ModelId] = 'gpt-4-insights')
BEGIN
    INSERT INTO [dbo].[LLMModelConfigs] (
        [ModelId], [ProviderId], [Name], [DisplayName], [Temperature], [MaxTokens], 
        [TopP], [FrequencyPenalty], [PresencePenalty], [IsEnabled], [UseCase], 
        [CostPerToken], [Capabilities]
    ) VALUES (
        'gpt-4-insights', 
        'openai-default', 
        'gpt-4', 
        'GPT-4 for Insights Generation', 
        0.3, 
        1000, 
        1.0, 
        0.0, 
        0.0, 
        1, 
        'Insights', 
        0.00003, -- Approximate cost per token
        '{"supportsStreaming": true, "maxContextLength": 8192, "supportsFunctions": true}'
    );

    PRINT 'Inserted sample GPT-4 Insights model configuration';
END

IF NOT EXISTS (SELECT 1 FROM [dbo].[LLMModelConfigs] WHERE [ModelId] = 'gpt-35-turbo-sql')
BEGIN
    INSERT INTO [dbo].[LLMModelConfigs] (
        [ModelId], [ProviderId], [Name], [DisplayName], [Temperature], [MaxTokens], 
        [TopP], [FrequencyPenalty], [PresencePenalty], [IsEnabled], [UseCase], 
        [CostPerToken], [Capabilities]
    ) VALUES (
        'gpt-35-turbo-sql', 
        'openai-default', 
        'gpt-3.5-turbo', 
        'GPT-3.5 Turbo for SQL Generation', 
        0.1, 
        2000, 
        1.0, 
        0.0, 
        0.0, 
        1, 
        'SQL', 
        0.000002, -- Approximate cost per token
        '{"supportsStreaming": true, "maxContextLength": 4096, "supportsFunctions": true}'
    );

    PRINT 'Inserted sample GPT-3.5 Turbo SQL model configuration';
END

-- =====================================================
-- 5. Create Views for Reporting (Optional)
-- =====================================================

-- View for daily usage summary
IF NOT EXISTS (SELECT * FROM sys.views WHERE name = 'vw_LLMDailyUsageSummary')
BEGIN
    EXEC('
    CREATE VIEW [dbo].[vw_LLMDailyUsageSummary] AS
    SELECT 
        CAST([Timestamp] AS DATE) AS [Date],
        [ProviderId],
        [ModelId],
        [RequestType],
        COUNT(*) AS [TotalRequests],
        SUM([TotalTokens]) AS [TotalTokens],
        SUM([Cost]) AS [TotalCost],
        AVG([DurationMs]) AS [AvgDurationMs],
        SUM(CASE WHEN [Success] = 1 THEN 1 ELSE 0 END) AS [SuccessfulRequests],
        CAST(SUM(CASE WHEN [Success] = 1 THEN 1 ELSE 0 END) AS FLOAT) / COUNT(*) AS [SuccessRate]
    FROM [dbo].[LLMUsageLogs]
    GROUP BY 
        CAST([Timestamp] AS DATE),
        [ProviderId],
        [ModelId],
        [RequestType]
    ');

    PRINT 'Created view: vw_LLMDailyUsageSummary';
END

-- View for provider health summary
IF NOT EXISTS (SELECT * FROM sys.views WHERE name = 'vw_LLMProviderHealthSummary')
BEGIN
    EXEC('
    CREATE VIEW [dbo].[vw_LLMProviderHealthSummary] AS
    SELECT 
        p.[ProviderId],
        p.[Name] AS [ProviderName],
        p.[Type] AS [ProviderType],
        p.[IsEnabled],
        p.[IsDefault],
        COUNT(l.[Id]) AS [TotalRequests],
        SUM(CASE WHEN l.[Success] = 1 THEN 1 ELSE 0 END) AS [SuccessfulRequests],
        CASE 
            WHEN COUNT(l.[Id]) > 0 
            THEN CAST(SUM(CASE WHEN l.[Success] = 1 THEN 1 ELSE 0 END) AS FLOAT) / COUNT(l.[Id])
            ELSE 0 
        END AS [SuccessRate],
        AVG(l.[DurationMs]) AS [AvgResponseTime],
        SUM(l.[Cost]) AS [TotalCost],
        MAX(l.[Timestamp]) AS [LastUsed]
    FROM [dbo].[LLMProviderConfigs] p
    LEFT JOIN [dbo].[LLMUsageLogs] l ON p.[ProviderId] = l.[ProviderId]
        AND l.[Timestamp] >= DATEADD(day, -30, GETUTCDATE()) -- Last 30 days
    GROUP BY 
        p.[ProviderId],
        p.[Name],
        p.[Type],
        p.[IsEnabled],
        p.[IsDefault]
    ');

    PRINT 'Created view: vw_LLMProviderHealthSummary';
END

-- =====================================================
-- 6. Create Stored Procedures for Common Operations (Optional)
-- =====================================================

-- Stored procedure to get usage analytics
IF NOT EXISTS (SELECT * FROM sys.procedures WHERE name = 'sp_GetLLMUsageAnalytics')
BEGIN
    EXEC('
    CREATE PROCEDURE [dbo].[sp_GetLLMUsageAnalytics]
        @StartDate DATETIME2,
        @EndDate DATETIME2,
        @ProviderId NVARCHAR(50) = NULL,
        @ModelId NVARCHAR(100) = NULL
    AS
    BEGIN
        SET NOCOUNT ON;
        
        SELECT 
            COUNT(*) AS TotalRequests,
            SUM([TotalTokens]) AS TotalTokens,
            SUM([Cost]) AS TotalCost,
            AVG([DurationMs]) AS AverageResponseTime,
            CAST(SUM(CASE WHEN [Success] = 1 THEN 1 ELSE 0 END) AS FLOAT) / COUNT(*) AS SuccessRate,
            [ProviderId],
            [ModelId],
            [RequestType]
        FROM [dbo].[LLMUsageLogs]
        WHERE [Timestamp] BETWEEN @StartDate AND @EndDate
            AND (@ProviderId IS NULL OR [ProviderId] = @ProviderId)
            AND (@ModelId IS NULL OR [ModelId] = @ModelId)
        GROUP BY [ProviderId], [ModelId], [RequestType]
        ORDER BY TotalRequests DESC;
    END
    ');

    PRINT 'Created stored procedure: sp_GetLLMUsageAnalytics';
END

-- =====================================================
-- Script Completion
-- =====================================================
PRINT '';
PRINT '=====================================================';
PRINT 'LLM Management Tables Creation Complete!';
PRINT '=====================================================';
PRINT 'Tables created:';
PRINT '  - LLMProviderConfigs (Provider configurations)';
PRINT '  - LLMModelConfigs (Model configurations)';
PRINT '  - LLMUsageLogs (Usage tracking and analytics)';
PRINT '';
PRINT 'Views created:';
PRINT '  - vw_LLMDailyUsageSummary (Daily usage summary)';
PRINT '  - vw_LLMProviderHealthSummary (Provider health summary)';
PRINT '';
PRINT 'Stored procedures created:';
PRINT '  - sp_GetLLMUsageAnalytics (Usage analytics)';
PRINT '';
PRINT 'Sample data inserted:';
PRINT '  - Default OpenAI and Azure OpenAI provider configurations';
PRINT '  - Sample model configurations for different use cases';
PRINT '';
PRINT 'Next steps:';
PRINT '  1. Configure API keys for providers through the LLM Management UI';
PRINT '  2. Enable providers after configuration';
PRINT '  3. Test provider connections';
PRINT '  4. Start using the LLM Management system!';
PRINT '=====================================================';
