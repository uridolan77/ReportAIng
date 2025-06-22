-- =====================================================
-- FIX LLMProviderConfigs TABLE - HANDLE NULL VALUES
-- =====================================================
-- This script fixes NULL values in NOT NULL string columns
-- that are causing SqlNullValueException in Entity Framework
-- Database: BIReportingCopilot_dev
-- =====================================================

USE [BIReportingCopilot_dev]
GO

BEGIN TRANSACTION;

BEGIN TRY
    PRINT 'Starting LLMProviderConfigs NULL value fixes...';

    -- =====================================================
    -- Check for NULL values in NOT NULL columns
    -- =====================================================
    
    DECLARE @NullCount INT;
    
    -- Check ProviderId
    SELECT @NullCount = COUNT(*) FROM [dbo].[LLMProviderConfigs] WHERE [ProviderId] IS NULL;
    IF @NullCount > 0
        PRINT 'Found ' + CAST(@NullCount AS NVARCHAR(10)) + ' NULL ProviderId values';
    
    -- Check Name
    SELECT @NullCount = COUNT(*) FROM [dbo].[LLMProviderConfigs] WHERE [Name] IS NULL;
    IF @NullCount > 0
        PRINT 'Found ' + CAST(@NullCount AS NVARCHAR(10)) + ' NULL Name values';
    
    -- Check Type
    SELECT @NullCount = COUNT(*) FROM [dbo].[LLMProviderConfigs] WHERE [Type] IS NULL;
    IF @NullCount > 0
        PRINT 'Found ' + CAST(@NullCount AS NVARCHAR(10)) + ' NULL Type values';
    
    -- Check Settings (might be required in some configurations)
    SELECT @NullCount = COUNT(*) FROM [dbo].[LLMProviderConfigs] WHERE [Settings] IS NULL;
    IF @NullCount > 0
        PRINT 'Found ' + CAST(@NullCount AS NVARCHAR(10)) + ' NULL Settings values';

    -- =====================================================
    -- Fix NULL values with appropriate defaults
    -- =====================================================

    -- Fix ProviderId (this should never be NULL as it's the primary key)
    UPDATE [dbo].[LLMProviderConfigs] 
    SET [ProviderId] = 'provider-' + CAST(NEWID() AS NVARCHAR(36))
    WHERE [ProviderId] IS NULL;
    
    PRINT 'Fixed NULL ProviderId values';

    -- Fix Name
    UPDATE [dbo].[LLMProviderConfigs] 
    SET [Name] = 'Unknown Provider'
    WHERE [Name] IS NULL;
    
    PRINT 'Fixed NULL Name values';

    -- Fix Type
    UPDATE [dbo].[LLMProviderConfigs] 
    SET [Type] = 'unknown'
    WHERE [Type] IS NULL;
    
    PRINT 'Fixed NULL Type values';

    -- Fix Settings (ensure it's valid JSON)
    UPDATE [dbo].[LLMProviderConfigs] 
    SET [Settings] = '{}'
    WHERE [Settings] IS NULL OR [Settings] = '';
    
    PRINT 'Fixed NULL Settings values';

    -- =====================================================
    -- Verify all NULL values are fixed
    -- =====================================================
    
    SELECT @NullCount = COUNT(*) 
    FROM [dbo].[LLMProviderConfigs] 
    WHERE [ProviderId] IS NULL 
       OR [Name] IS NULL 
       OR [Type] IS NULL 
       OR [Settings] IS NULL;
    
    IF @NullCount = 0
    BEGIN
        PRINT 'All NULL values have been fixed successfully!';
    END
    ELSE
    BEGIN
        PRINT 'Warning: ' + CAST(@NullCount AS NVARCHAR(10)) + ' NULL values still remain';
    END

    -- =====================================================
    -- Add constraints to prevent future NULL values
    -- =====================================================
    
    -- Check if constraints already exist before adding them
    IF NOT EXISTS (SELECT * FROM sys.check_constraints WHERE name = 'CK_LLMProviderConfigs_ProviderId_NotEmpty')
    BEGIN
        ALTER TABLE [dbo].[LLMProviderConfigs] 
        ADD CONSTRAINT CK_LLMProviderConfigs_ProviderId_NotEmpty 
        CHECK ([ProviderId] IS NOT NULL AND LEN([ProviderId]) > 0);
        PRINT 'Added constraint: CK_LLMProviderConfigs_ProviderId_NotEmpty';
    END

    IF NOT EXISTS (SELECT * FROM sys.check_constraints WHERE name = 'CK_LLMProviderConfigs_Name_NotEmpty')
    BEGIN
        ALTER TABLE [dbo].[LLMProviderConfigs] 
        ADD CONSTRAINT CK_LLMProviderConfigs_Name_NotEmpty 
        CHECK ([Name] IS NOT NULL AND LEN([Name]) > 0);
        PRINT 'Added constraint: CK_LLMProviderConfigs_Name_NotEmpty';
    END

    IF NOT EXISTS (SELECT * FROM sys.check_constraints WHERE name = 'CK_LLMProviderConfigs_Type_NotEmpty')
    BEGIN
        ALTER TABLE [dbo].[LLMProviderConfigs] 
        ADD CONSTRAINT CK_LLMProviderConfigs_Type_NotEmpty 
        CHECK ([Type] IS NOT NULL AND LEN([Type]) > 0);
        PRINT 'Added constraint: CK_LLMProviderConfigs_Type_NotEmpty';
    END

    IF NOT EXISTS (SELECT * FROM sys.check_constraints WHERE name = 'CK_LLMProviderConfigs_Settings_NotNull')
    BEGIN
        ALTER TABLE [dbo].[LLMProviderConfigs] 
        ADD CONSTRAINT CK_LLMProviderConfigs_Settings_NotNull 
        CHECK ([Settings] IS NOT NULL);
        PRINT 'Added constraint: CK_LLMProviderConfigs_Settings_NotNull';
    END

    -- =====================================================
    -- Ensure we have at least one provider for testing
    -- =====================================================

    -- Check if we have any providers
    DECLARE @ProviderCount INT;
    SELECT @ProviderCount = COUNT(*) FROM [dbo].[LLMProviderConfigs];
    
    IF @ProviderCount = 0
    BEGIN
        PRINT 'No providers found. Inserting default OpenAI provider...';
        
        INSERT INTO [dbo].[LLMProviderConfigs]
        ([ProviderId], [Name], [Type], [ApiKey], [Endpoint], [Organization], [IsEnabled], [IsDefault], [Settings], [CreatedAt], [UpdatedAt])
        VALUES
        ('openai-default', 'OpenAI Default', 'openai', NULL, 'https://api.openai.com/v1', NULL, 1, 1, '{"timeout": 30000, "retries": 3}', GETUTCDATE(), GETUTCDATE());
        
        PRINT 'Inserted default OpenAI provider';
    END
    ELSE
    BEGIN
        PRINT 'Found ' + CAST(@ProviderCount AS NVARCHAR(10)) + ' existing providers';
    END

    -- =====================================================
    -- Show current provider status
    -- =====================================================
    
    PRINT '';
    PRINT 'Current LLM Provider Configurations:';
    SELECT 
        [ProviderId],
        [Name],
        [Type],
        [IsEnabled],
        [IsDefault],
        CASE 
            WHEN [ApiKey] IS NULL THEN 'Not Configured'
            ELSE 'Configured'
        END AS ApiKeyStatus,
        [CreatedAt]
    FROM [dbo].[LLMProviderConfigs]
    ORDER BY [Name];

    COMMIT TRANSACTION;
    PRINT '';
    PRINT 'LLMProviderConfigs NULL value fixes completed successfully!';
    PRINT '';
    PRINT '✅ Fixed NULL values in string columns';
    PRINT '✅ Added constraints to prevent future NULL values';
    PRINT '✅ Ensured at least one provider exists';
    PRINT '';
    PRINT '✅ LLM Management dashboard should now work without SqlNullValueException!';

END TRY
BEGIN CATCH
    ROLLBACK TRANSACTION;
    
    DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
    DECLARE @ErrorSeverity INT = ERROR_SEVERITY();
    DECLARE @ErrorState INT = ERROR_STATE();
    
    PRINT 'Error occurred during LLMProviderConfigs fixes:';
    PRINT @ErrorMessage;
    
    RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
END CATCH
