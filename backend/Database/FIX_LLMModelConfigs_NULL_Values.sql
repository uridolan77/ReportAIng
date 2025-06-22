-- =====================================================
-- FIX LLMModelConfigs TABLE - HANDLE NULL VALUES
-- =====================================================
-- This script fixes NULL values in NOT NULL string columns
-- that are causing SqlNullValueException in Entity Framework
-- Database: BIReportingCopilot_dev
-- =====================================================

USE [BIReportingCopilot_dev]
GO

BEGIN TRANSACTION;

BEGIN TRY
    PRINT 'Starting LLMModelConfigs NULL value fixes...';

    -- =====================================================
    -- Check if table exists
    -- =====================================================
    
    IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'LLMModelConfigs')
    BEGIN
        PRINT '⚠️  LLMModelConfigs table does not exist. Creating it...';
        
        CREATE TABLE [dbo].[LLMModelConfigs] (
            [ModelId] NVARCHAR(100) NOT NULL PRIMARY KEY,
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
            [Capabilities] NVARCHAR(MAX) NOT NULL DEFAULT '{}'
        );
        
        PRINT '✅ LLMModelConfigs table created';
    END

    -- =====================================================
    -- Check for NULL values in NOT NULL columns
    -- =====================================================
    
    DECLARE @NullCount INT;
    
    -- Check ModelId
    SELECT @NullCount = COUNT(*) FROM [dbo].[LLMModelConfigs] WHERE [ModelId] IS NULL;
    IF @NullCount > 0
        PRINT 'Found ' + CAST(@NullCount AS NVARCHAR(10)) + ' NULL ModelId values';
    
    -- Check ProviderId
    SELECT @NullCount = COUNT(*) FROM [dbo].[LLMModelConfigs] WHERE [ProviderId] IS NULL;
    IF @NullCount > 0
        PRINT 'Found ' + CAST(@NullCount AS NVARCHAR(10)) + ' NULL ProviderId values';
    
    -- Check Name
    SELECT @NullCount = COUNT(*) FROM [dbo].[LLMModelConfigs] WHERE [Name] IS NULL;
    IF @NullCount > 0
        PRINT 'Found ' + CAST(@NullCount AS NVARCHAR(10)) + ' NULL Name values';
    
    -- Check DisplayName
    SELECT @NullCount = COUNT(*) FROM [dbo].[LLMModelConfigs] WHERE [DisplayName] IS NULL;
    IF @NullCount > 0
        PRINT 'Found ' + CAST(@NullCount AS NVARCHAR(10)) + ' NULL DisplayName values';
    
    -- Check Capabilities
    SELECT @NullCount = COUNT(*) FROM [dbo].[LLMModelConfigs] WHERE [Capabilities] IS NULL;
    IF @NullCount > 0
        PRINT 'Found ' + CAST(@NullCount AS NVARCHAR(10)) + ' NULL Capabilities values';

    -- =====================================================
    -- Fix NULL values with appropriate defaults
    -- =====================================================

    -- Fix ModelId (this should never be NULL as it's the primary key)
    UPDATE [dbo].[LLMModelConfigs] 
    SET [ModelId] = 'model-' + CAST(NEWID() AS NVARCHAR(36))
    WHERE [ModelId] IS NULL;
    
    PRINT 'Fixed NULL ModelId values';

    -- Fix ProviderId
    UPDATE [dbo].[LLMModelConfigs] 
    SET [ProviderId] = 'unknown-provider'
    WHERE [ProviderId] IS NULL;
    
    PRINT 'Fixed NULL ProviderId values';

    -- Fix Name
    UPDATE [dbo].[LLMModelConfigs] 
    SET [Name] = 'Unknown Model'
    WHERE [Name] IS NULL;
    
    PRINT 'Fixed NULL Name values';

    -- Fix DisplayName
    UPDATE [dbo].[LLMModelConfigs] 
    SET [DisplayName] = 'Unknown Model'
    WHERE [DisplayName] IS NULL;
    
    PRINT 'Fixed NULL DisplayName values';

    -- Fix Capabilities (ensure it's valid JSON)
    UPDATE [dbo].[LLMModelConfigs] 
    SET [Capabilities] = '{}'
    WHERE [Capabilities] IS NULL OR [Capabilities] = '';
    
    PRINT 'Fixed NULL Capabilities values';

    -- =====================================================
    -- Verify all NULL values are fixed
    -- =====================================================
    
    SELECT @NullCount = COUNT(*) 
    FROM [dbo].[LLMModelConfigs] 
    WHERE [ModelId] IS NULL 
       OR [ProviderId] IS NULL 
       OR [Name] IS NULL 
       OR [DisplayName] IS NULL 
       OR [Capabilities] IS NULL;
    
    IF @NullCount = 0
    BEGIN
        PRINT 'All NULL values have been fixed successfully!';
    END
    ELSE
    BEGIN
        PRINT 'Warning: ' + CAST(@NullCount AS NVARCHAR(10)) + ' NULL values still remain';
    END

    -- =====================================================
    -- Ensure we have at least one model for testing
    -- =====================================================

    -- Check if we have any models
    DECLARE @ModelCount INT;
    SELECT @ModelCount = COUNT(*) FROM [dbo].[LLMModelConfigs];
    
    IF @ModelCount = 0
    BEGIN
        PRINT 'No models found. Inserting default models...';
        
        INSERT INTO [dbo].[LLMModelConfigs]
        ([ModelId], [ProviderId], [Name], [DisplayName], [Temperature], [MaxTokens], [TopP], [FrequencyPenalty], [PresencePenalty], [IsEnabled], [UseCase], [CostPerToken], [Capabilities])
        VALUES
        ('gpt-4o-mini', 'openai-main', 'gpt-4o-mini', 'GPT-4o Mini', 0.1, 8000, 1.0, 0.0, 0.0, 1, 'sql_generation', 0.00000015, '{"supports_functions": true, "supports_vision": false}'),

        ('gpt-4-turbo', 'openai-main', 'gpt-4-turbo', 'GPT-4 Turbo', 0.1, 8000, 1.0, 0.0, 0.0, 1, 'analysis', 0.00001, '{"supports_functions": true, "supports_vision": true}'),

        ('gpt-3.5-turbo', 'openai-main', 'gpt-3.5-turbo', 'GPT-3.5 Turbo', 0.7, 4000, 1.0, 0.0, 0.0, 1, 'chat', 0.0000005, '{"supports_functions": true, "supports_vision": false}');
        
        PRINT 'Inserted default models';
    END
    ELSE
    BEGIN
        PRINT 'Found ' + CAST(@ModelCount AS NVARCHAR(10)) + ' existing models';
    END

    -- =====================================================
    -- Show current model status
    -- =====================================================
    
    PRINT '';
    PRINT 'Current LLM Model Configurations:';
    SELECT
        [ModelId],
        [ProviderId],
        [Name],
        [DisplayName],
        [IsEnabled],
        [UseCase],
        [CostPerToken]
    FROM [dbo].[LLMModelConfigs]
    ORDER BY [ProviderId], [Name];

    COMMIT TRANSACTION;
    PRINT '';
    PRINT 'LLMModelConfigs NULL value fixes completed successfully!';
    PRINT '';
    PRINT '✅ Fixed NULL values in string columns';
    PRINT '✅ Ensured at least one model exists';
    PRINT '';
    PRINT '✅ LLM Management should now work without SqlNullValueException!';

END TRY
BEGIN CATCH
    ROLLBACK TRANSACTION;
    
    DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
    DECLARE @ErrorSeverity INT = ERROR_SEVERITY();
    DECLARE @ErrorState INT = ERROR_STATE();
    
    PRINT 'Error occurred during LLMModelConfigs fixes:';
    PRINT @ErrorMessage;
    
    RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
END CATCH
