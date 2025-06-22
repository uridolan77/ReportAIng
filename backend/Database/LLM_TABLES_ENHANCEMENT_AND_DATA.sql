-- =====================================================
-- LLM TABLES ENHANCEMENT AND SAMPLE DATA SCRIPT
-- =====================================================
-- This script enhances existing LLM tables and adds sample data
-- Run this script to make LLM Management endpoints work properly
-- Database: BIReportingCopilot_dev
-- =====================================================

USE [BIReportingCopilot_dev]
GO

BEGIN TRANSACTION;

BEGIN TRY
    PRINT 'Starting LLM Tables Enhancement and Data Population...';

    -- =====================================================
    -- 1. Add Missing Indexes for Performance
    -- =====================================================
    
    -- LLMProviderConfigs indexes
    IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID('LLMProviderConfigs') AND name = 'IX_LLMProviderConfigs_IsEnabled')
    BEGIN
        CREATE NONCLUSTERED INDEX [IX_LLMProviderConfigs_IsEnabled] ON [dbo].[LLMProviderConfigs] ([IsEnabled] ASC);
        PRINT 'Added IX_LLMProviderConfigs_IsEnabled index';
    END

    IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID('LLMProviderConfigs') AND name = 'IX_LLMProviderConfigs_Type')
    BEGIN
        CREATE NONCLUSTERED INDEX [IX_LLMProviderConfigs_Type] ON [dbo].[LLMProviderConfigs] ([Type] ASC);
        PRINT 'Added IX_LLMProviderConfigs_Type index';
    END

    -- LLMModelConfigs indexes
    IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID('LLMModelConfigs') AND name = 'IX_LLMModelConfigs_ProviderId')
    BEGIN
        CREATE NONCLUSTERED INDEX [IX_LLMModelConfigs_ProviderId] ON [dbo].[LLMModelConfigs] ([ProviderId] ASC);
        PRINT 'Added IX_LLMModelConfigs_ProviderId index';
    END

    IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID('LLMModelConfigs') AND name = 'IX_LLMModelConfigs_IsEnabled')
    BEGIN
        CREATE NONCLUSTERED INDEX [IX_LLMModelConfigs_IsEnabled] ON [dbo].[LLMModelConfigs] ([IsEnabled] ASC);
        PRINT 'Added IX_LLMModelConfigs_IsEnabled index';
    END

    -- LLMUsageLogs indexes
    IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID('LLMUsageLogs') AND name = 'IX_LLMUsageLogs_UserId')
    BEGIN
        CREATE NONCLUSTERED INDEX [IX_LLMUsageLogs_UserId] ON [dbo].[LLMUsageLogs] ([UserId] ASC);
        PRINT 'Added IX_LLMUsageLogs_UserId index';
    END

    IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID('LLMUsageLogs') AND name = 'IX_LLMUsageLogs_ProviderId')
    BEGIN
        CREATE NONCLUSTERED INDEX [IX_LLMUsageLogs_ProviderId] ON [dbo].[LLMUsageLogs] ([ProviderId] ASC);
        PRINT 'Added IX_LLMUsageLogs_ProviderId index';
    END

    IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID('LLMUsageLogs') AND name = 'IX_LLMUsageLogs_Timestamp')
    BEGIN
        CREATE NONCLUSTERED INDEX [IX_LLMUsageLogs_Timestamp] ON [dbo].[LLMUsageLogs] ([Timestamp] ASC);
        PRINT 'Added IX_LLMUsageLogs_Timestamp index';
    END

    -- =====================================================
    -- 2. Add Missing Columns (if needed)
    -- =====================================================

    -- Check if LLMModelConfigs needs CreatedDate/UpdatedDate columns (BaseEntity standard)
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('LLMModelConfigs') AND name = 'CreatedDate')
    BEGIN
        ALTER TABLE [dbo].[LLMModelConfigs] ADD [CreatedDate] DATETIME2(7) NOT NULL DEFAULT GETUTCDATE();
        PRINT 'Added CreatedDate column to LLMModelConfigs';
    END

    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('LLMModelConfigs') AND name = 'UpdatedDate')
    BEGIN
        ALTER TABLE [dbo].[LLMModelConfigs] ADD [UpdatedDate] DATETIME2(7) NULL;
        PRINT 'Added UpdatedDate column to LLMModelConfigs';
    END

    -- Check if LLMProviderConfigs needs CreatedDate/UpdatedDate columns (BaseEntity standard)
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('LLMProviderConfigs') AND name = 'CreatedDate')
    BEGIN
        ALTER TABLE [dbo].[LLMProviderConfigs] ADD [CreatedDate] DATETIME2(7) NOT NULL DEFAULT GETUTCDATE();
        PRINT 'Added CreatedDate column to LLMProviderConfigs';
    END

    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('LLMProviderConfigs') AND name = 'UpdatedDate')
    BEGIN
        ALTER TABLE [dbo].[LLMProviderConfigs] ADD [UpdatedDate] DATETIME2(7) NULL;
        PRINT 'Added UpdatedDate column to LLMProviderConfigs';
    END

    -- Check if LLMUsageLogs needs CreatedDate/UpdatedDate columns (BaseEntity standard)
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('LLMUsageLogs') AND name = 'CreatedDate')
    BEGIN
        ALTER TABLE [dbo].[LLMUsageLogs] ADD [CreatedDate] DATETIME2(7) NOT NULL DEFAULT GETUTCDATE();
        PRINT 'Added CreatedDate column to LLMUsageLogs';
    END

    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('LLMUsageLogs') AND name = 'UpdatedDate')
    BEGIN
        ALTER TABLE [dbo].[LLMUsageLogs] ADD [UpdatedDate] DATETIME2(7) NULL;
        PRINT 'Added UpdatedDate column to LLMUsageLogs';
    END

    -- =====================================================
    -- 3. Insert Sample Provider Data
    -- =====================================================
    
    -- Get admin user ID
    DECLARE @AdminUserId NVARCHAR(256) = (SELECT TOP 1 Id FROM Users WHERE Email LIKE '%admin%' OR UserName LIKE '%admin%');
    
    IF @AdminUserId IS NULL
        SET @AdminUserId = 'admin-user-001';

    PRINT 'Using admin user ID: ' + ISNULL(@AdminUserId, 'NULL');

    -- Clear existing sample data to avoid duplicates
    DELETE FROM [dbo].[LLMUsageLogs] WHERE UserId = @AdminUserId AND RequestId LIKE 'sample-%';
    DELETE FROM [dbo].[LLMModelConfigs] WHERE ModelId LIKE 'sample-%';
    DELETE FROM [dbo].[LLMProviderConfigs] WHERE ProviderId LIKE 'sample-%';

    -- Check if LLMProviderConfigs has CreatedAt/UpdatedAt columns
    DECLARE @ProviderHasCreatedAt BIT = 0;
    DECLARE @ProviderHasUpdatedAt BIT = 0;

    IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('LLMProviderConfigs') AND name = 'CreatedAt')
        SET @ProviderHasCreatedAt = 1;

    IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('LLMProviderConfigs') AND name = 'UpdatedAt')
        SET @ProviderHasUpdatedAt = 1;

    -- Insert Sample Provider Configurations
    IF @ProviderHasCreatedAt = 1 AND @ProviderHasUpdatedAt = 1
    BEGIN
        -- Insert with CreatedAt and UpdatedAt columns
        INSERT INTO [dbo].[LLMProviderConfigs]
        ([ProviderId], [Name], [Type], [ApiKey], [Endpoint], [Organization], [IsEnabled], [IsDefault], [Settings], [CreatedAt], [UpdatedAt])
        VALUES
        ('openai-main', 'OpenAI Primary', 'openai', 'sk-***CONFIGURED***', 'https://api.openai.com/v1', 'ReportAIng', 1, 1,
         '{"timeout": 30000, "retries": 3, "maxTokens": 8000}', GETUTCDATE(), GETUTCDATE()),

        ('azure-openai-prod', 'Azure OpenAI Production', 'azure', 'azure-***CONFIGURED***', 'https://reportaing.openai.azure.com/', 'ReportAIng', 1, 0,
         '{"apiVersion": "2024-02-01", "deployment": "gpt-4", "timeout": 45000}', GETUTCDATE(), GETUTCDATE()),

        ('anthropic-claude', 'Anthropic Claude', 'anthropic', 'sk-ant-***CONFIGURED***', 'https://api.anthropic.com', NULL, 0, 0,
         '{"version": "2023-06-01", "maxTokens": 4000}', GETUTCDATE(), GETUTCDATE()),

        ('google-palm', 'Google PaLM', 'google', 'google-***CONFIGURED***', 'https://generativelanguage.googleapis.com', NULL, 0, 0,
         '{"version": "v1beta", "temperature": 0.7}', GETUTCDATE(), GETUTCDATE()),

        ('local-llama', 'Local Llama', 'local', NULL, 'http://localhost:11434', NULL, 0, 0,
         '{"model": "llama2", "temperature": 0.7}', GETUTCDATE(), GETUTCDATE());
    END
    ELSE
    BEGIN
        -- Insert without CreatedAt and UpdatedAt columns
        INSERT INTO [dbo].[LLMProviderConfigs]
        ([ProviderId], [Name], [Type], [ApiKey], [Endpoint], [Organization], [IsEnabled], [IsDefault], [Settings])
        VALUES
        ('openai-main', 'OpenAI Primary', 'openai', 'sk-***CONFIGURED***', 'https://api.openai.com/v1', 'ReportAIng', 1, 1,
         '{"timeout": 30000, "retries": 3, "maxTokens": 8000}'),

        ('azure-openai-prod', 'Azure OpenAI Production', 'azure', 'azure-***CONFIGURED***', 'https://reportaing.openai.azure.com/', 'ReportAIng', 1, 0,
         '{"apiVersion": "2024-02-01", "deployment": "gpt-4", "timeout": 45000}'),

        ('anthropic-claude', 'Anthropic Claude', 'anthropic', 'sk-ant-***CONFIGURED***', 'https://api.anthropic.com', NULL, 0, 0,
         '{"version": "2023-06-01", "maxTokens": 4000}'),

        ('google-palm', 'Google PaLM', 'google', 'google-***CONFIGURED***', 'https://generativelanguage.googleapis.com', NULL, 0, 0,
         '{"version": "v1beta", "temperature": 0.7}'),

        ('local-llama', 'Local Llama', 'local', NULL, 'http://localhost:11434', NULL, 0, 0,
         '{"model": "llama2", "temperature": 0.7}');
    END

    PRINT 'Inserted sample provider configurations';

    -- =====================================================
    -- 4. Insert Sample Model Configurations
    -- =====================================================

    -- Insert model configurations (LLMModelConfigs table does not have CreatedAt/UpdatedAt columns)
    INSERT INTO [dbo].[LLMModelConfigs]
    ([ModelId], [ProviderId], [Name], [DisplayName], [Temperature], [MaxTokens], [TopP], [FrequencyPenalty], [PresencePenalty], [IsEnabled], [UseCase], [CostPerToken], [Capabilities])
    VALUES
    -- OpenAI Models
    ('gpt-4-turbo', 'openai-main', 'gpt-4-turbo', 'GPT-4 Turbo', 0.7, 8000, 1.0, 0.0, 0.0, 1, 'general', 0.00003,
     '{"reasoning": true, "coding": true, "analysis": true, "multimodal": false}'),

    ('gpt-4', 'openai-main', 'gpt-4', 'GPT-4', 0.7, 8000, 1.0, 0.0, 0.0, 1, 'analysis', 0.00003,
     '{"reasoning": true, "coding": true, "analysis": true}'),

    ('gpt-3.5-turbo', 'openai-main', 'gpt-3.5-turbo', 'GPT-3.5 Turbo', 0.7, 4000, 1.0, 0.0, 0.0, 1, 'chat', 0.000002,
     '{"chat": true, "completion": true, "fast": true}'),

    -- Azure OpenAI Models
    ('gpt-4-azure', 'azure-openai-prod', 'gpt-4', 'GPT-4 (Azure)', 0.7, 8000, 1.0, 0.0, 0.0, 1, 'enterprise', 0.00003,
     '{"reasoning": true, "enterprise": true, "compliance": true}'),

    ('gpt-35-turbo-azure', 'azure-openai-prod', 'gpt-35-turbo', 'GPT-3.5 Turbo (Azure)', 0.7, 4000, 1.0, 0.0, 0.0, 1, 'production', 0.000002,
     '{"chat": true, "enterprise": true, "fast": true}'),

    -- Anthropic Models
    ('claude-3-opus', 'anthropic-claude', 'claude-3-opus-20240229', 'Claude 3 Opus', 0.7, 4000, 1.0, 0.0, 0.0, 0, 'premium', 0.000075,
     '{"reasoning": true, "analysis": true, "creative": true}'),

    ('claude-3-sonnet', 'anthropic-claude', 'claude-3-sonnet-20240229', 'Claude 3 Sonnet', 0.7, 4000, 1.0, 0.0, 0.0, 0, 'balanced', 0.000015,
     '{"reasoning": true, "analysis": true, "balanced": true}'),

    -- Google Models
    ('palm-2', 'google-palm', 'text-bison-001', 'PaLM 2 Text', 0.7, 1024, 0.95, 0.0, 0.0, 0, 'experimental', 0.000001,
     '{"text": true, "experimental": true}'),

    -- Local Models
    ('llama2-7b', 'local-llama', 'llama2:7b', 'Llama 2 7B', 0.7, 2048, 0.9, 0.0, 0.0, 0, 'local', 0.0,
     '{"local": true, "opensource": true, "privacy": true}');

    PRINT 'Inserted sample model configurations';

    -- =====================================================
    -- 5. Insert Sample Usage Logs
    -- =====================================================
    
    INSERT INTO [dbo].[LLMUsageLogs]
    ([RequestId], [UserId], [ProviderId], [ModelId], [RequestType], [RequestText], [ResponseText], [InputTokens], [OutputTokens], [TotalTokens], [Cost], [DurationMs], [Success], [Timestamp])
    VALUES 
    ('sample-001', @AdminUserId, 'openai-main', 'gpt-4-turbo', 'sql_generation', 
     'Generate SQL to show sales by region for Q4 2024', 
     'SELECT region, SUM(sales_amount) as total_sales FROM sales WHERE quarter = ''Q4'' AND year = 2024 GROUP BY region ORDER BY total_sales DESC', 
     67, 45, 112, 0.00336, 2340, 1, DATEADD(hour, -2, GETUTCDATE())),
     
    ('sample-002', @AdminUserId, 'openai-main', 'gpt-3.5-turbo', 'chat', 
     'Explain the sales trend in the data', 
     'Based on the sales data, there is a clear upward trend in Q4 with a 15% increase compared to Q3. The growth is primarily driven by the North American region.', 
     45, 32, 77, 0.000154, 1890, 1, DATEADD(hour, -1, GETUTCDATE())),
     
    ('sample-003', @AdminUserId, 'azure-openai-prod', 'gpt-4-azure', 'analysis', 
     'Analyze customer behavior patterns from the purchase data', 
     'The analysis reveals three distinct customer segments: 1) High-value customers (20%) contributing 60% of revenue, 2) Regular customers (50%) with consistent monthly purchases, 3) Occasional buyers (30%) with seasonal patterns.', 
     89, 67, 156, 0.00468, 3200, 1, DATEADD(minute, -30, GETUTCDATE())),
     
    ('sample-004', @AdminUserId, 'openai-main', 'gpt-4', 'completion', 
     'Complete this SQL query: SELECT customer_id, COUNT(*) as order_count FROM orders WHERE', 
     'SELECT customer_id, COUNT(*) as order_count FROM orders WHERE order_date >= DATEADD(month, -12, GETDATE()) GROUP BY customer_id HAVING COUNT(*) > 5 ORDER BY order_count DESC', 
     34, 28, 62, 0.00186, 1560, 1, DATEADD(minute, -15, GETUTCDATE())),
     
    ('sample-005', @AdminUserId, 'openai-main', 'gpt-3.5-turbo', 'sql_generation', 
     'Show top 10 products by revenue', 
     'SELECT TOP 10 p.product_name, SUM(oi.quantity * oi.unit_price) as total_revenue FROM products p JOIN order_items oi ON p.product_id = oi.product_id GROUP BY p.product_name ORDER BY total_revenue DESC', 
     52, 38, 90, 0.00018, 2100, 1, DATEADD(minute, -5, GETUTCDATE())),
     
    ('sample-006', @AdminUserId, 'azure-openai-prod', 'gpt-35-turbo-azure', 'chat', 
     'What insights can you provide about our inventory levels?', 
     'Your inventory analysis shows: 1) 15% of products are overstocked, 2) 8% are understocked, 3) Fast-moving items need reorder point adjustments, 4) Seasonal items should be reviewed quarterly.', 
     41, 29, 70, 0.00014, 1750, 1, GETUTCDATE());

    PRINT 'Inserted sample usage logs';

    -- =====================================================
    -- 6. Insert Sample Performance Metrics
    -- =====================================================
    
    INSERT INTO [dbo].[LLMPerformanceMetrics]
    ([EntityType], [EntityId], [MetricName], [MetricValue], [MetricUnit], [AverageExecutionTime], [TotalOperations], [SuccessCount], [ErrorCount], [LastUpdated], [CreatedDate])
    VALUES 
    ('Provider', 'openai-main', 'ResponseTime', 2340.0, 'ms', 2340, 156, 152, 4, GETUTCDATE(), GETUTCDATE()),
    ('Provider', 'openai-main', 'SuccessRate', 97.4, 'percentage', 2100, 156, 152, 4, GETUTCDATE(), GETUTCDATE()),
    ('Provider', 'azure-openai-prod', 'ResponseTime', 1890.0, 'ms', 1890, 89, 87, 2, GETUTCDATE(), GETUTCDATE()),
    ('Provider', 'azure-openai-prod', 'SuccessRate', 97.8, 'percentage', 1890, 89, 87, 2, GETUTCDATE(), GETUTCDATE()),
    ('Model', 'gpt-4-turbo', 'TokensPerSecond', 45.5, 'tokens/sec', 2200, 98, 96, 2, GETUTCDATE(), GETUTCDATE()),
    ('Model', 'gpt-3.5-turbo', 'TokensPerSecond', 67.8, 'tokens/sec', 1650, 78, 77, 1, GETUTCDATE(), GETUTCDATE()),
    ('Model', 'gpt-4-azure', 'TokensPerSecond', 42.1, 'tokens/sec', 2400, 45, 44, 1, GETUTCDATE(), GETUTCDATE()),
    ('Provider', 'anthropic-claude', 'ResponseTime', 3200.0, 'ms', 3200, 12, 11, 1, GETUTCDATE(), GETUTCDATE()),
    ('Provider', 'anthropic-claude', 'SuccessRate', 91.7, 'percentage', 3200, 12, 11, 1, GETUTCDATE(), GETUTCDATE());

    PRINT 'Inserted sample performance metrics';

    COMMIT TRANSACTION;
    PRINT 'LLM Tables Enhancement and Data Population completed successfully!';
    PRINT '';
    PRINT '✅ Added performance indexes to all LLM tables';
    PRINT '✅ Added missing columns where needed';
    PRINT '✅ Populated comprehensive sample data:';
    PRINT '   - 5 Provider configurations (OpenAI, Azure, Anthropic, Google, Local)';
    PRINT '   - 9 Model configurations across all providers';
    PRINT '   - 6 Usage log entries with realistic data';
    PRINT '   - 9 Performance metrics for monitoring';
    PRINT '';
    PRINT '✅ LLM Management endpoints should now work perfectly!';
    PRINT '✅ Frontend LLM Management dashboard will load with real data!';

END TRY
BEGIN CATCH
    ROLLBACK TRANSACTION;
    
    DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
    DECLARE @ErrorSeverity INT = ERROR_SEVERITY();
    DECLARE @ErrorState INT = ERROR_STATE();
    
    PRINT 'Error occurred during LLM Tables enhancement:';
    PRINT @ErrorMessage;
    
    RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
END CATCH
