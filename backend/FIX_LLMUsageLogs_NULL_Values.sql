-- =====================================================
-- FIX LLMUsageLogs TABLE - HANDLE NULL VALUES
-- =====================================================
-- This script fixes NULL values in NOT NULL string columns
-- that are causing SqlNullValueException in Entity Framework
-- Database: BIReportingCopilot_dev
-- =====================================================

USE [BIReportingCopilot_dev]
GO

BEGIN TRANSACTION;

BEGIN TRY
    PRINT 'Starting LLMUsageLogs NULL value fixes...';

    -- =====================================================
    -- Check for NULL values in NOT NULL columns
    -- =====================================================
    
    DECLARE @NullCount INT;
    
    -- Check RequestId
    SELECT @NullCount = COUNT(*) FROM [dbo].[LLMUsageLogs] WHERE [RequestId] IS NULL;
    IF @NullCount > 0
        PRINT 'Found ' + CAST(@NullCount AS NVARCHAR(10)) + ' NULL RequestId values';
    
    -- Check UserId
    SELECT @NullCount = COUNT(*) FROM [dbo].[LLMUsageLogs] WHERE [UserId] IS NULL;
    IF @NullCount > 0
        PRINT 'Found ' + CAST(@NullCount AS NVARCHAR(10)) + ' NULL UserId values';
    
    -- Check ProviderId
    SELECT @NullCount = COUNT(*) FROM [dbo].[LLMUsageLogs] WHERE [ProviderId] IS NULL;
    IF @NullCount > 0
        PRINT 'Found ' + CAST(@NullCount AS NVARCHAR(10)) + ' NULL ProviderId values';
    
    -- Check ModelId
    SELECT @NullCount = COUNT(*) FROM [dbo].[LLMUsageLogs] WHERE [ModelId] IS NULL;
    IF @NullCount > 0
        PRINT 'Found ' + CAST(@NullCount AS NVARCHAR(10)) + ' NULL ModelId values';
    
    -- Check RequestType
    SELECT @NullCount = COUNT(*) FROM [dbo].[LLMUsageLogs] WHERE [RequestType] IS NULL;
    IF @NullCount > 0
        PRINT 'Found ' + CAST(@NullCount AS NVARCHAR(10)) + ' NULL RequestType values';
    
    -- Check RequestText
    SELECT @NullCount = COUNT(*) FROM [dbo].[LLMUsageLogs] WHERE [RequestText] IS NULL;
    IF @NullCount > 0
        PRINT 'Found ' + CAST(@NullCount AS NVARCHAR(10)) + ' NULL RequestText values';
    
    -- Check ResponseText
    SELECT @NullCount = COUNT(*) FROM [dbo].[LLMUsageLogs] WHERE [ResponseText] IS NULL;
    IF @NullCount > 0
        PRINT 'Found ' + CAST(@NullCount AS NVARCHAR(10)) + ' NULL ResponseText values';

    -- =====================================================
    -- Fix NULL values with appropriate defaults
    -- =====================================================

    -- Fix RequestId
    UPDATE [dbo].[LLMUsageLogs] 
    SET [RequestId] = 'req-' + CAST([Id] AS NVARCHAR(20))
    WHERE [RequestId] IS NULL;
    
    PRINT 'Fixed NULL RequestId values';

    -- Fix UserId
    UPDATE [dbo].[LLMUsageLogs] 
    SET [UserId] = 'unknown-user'
    WHERE [UserId] IS NULL;
    
    PRINT 'Fixed NULL UserId values';

    -- Fix ProviderId
    UPDATE [dbo].[LLMUsageLogs] 
    SET [ProviderId] = 'unknown-provider'
    WHERE [ProviderId] IS NULL;
    
    PRINT 'Fixed NULL ProviderId values';

    -- Fix ModelId
    UPDATE [dbo].[LLMUsageLogs] 
    SET [ModelId] = 'unknown-model'
    WHERE [ModelId] IS NULL;
    
    PRINT 'Fixed NULL ModelId values';

    -- Fix RequestType
    UPDATE [dbo].[LLMUsageLogs] 
    SET [RequestType] = 'unknown'
    WHERE [RequestType] IS NULL;
    
    PRINT 'Fixed NULL RequestType values';

    -- Fix RequestText
    UPDATE [dbo].[LLMUsageLogs] 
    SET [RequestText] = 'No request text available'
    WHERE [RequestText] IS NULL;
    
    PRINT 'Fixed NULL RequestText values';

    -- Fix ResponseText
    UPDATE [dbo].[LLMUsageLogs] 
    SET [ResponseText] = 'No response text available'
    WHERE [ResponseText] IS NULL;
    
    PRINT 'Fixed NULL ResponseText values';

    -- =====================================================
    -- Verify all NULL values are fixed
    -- =====================================================
    
    SELECT @NullCount = COUNT(*) 
    FROM [dbo].[LLMUsageLogs] 
    WHERE [RequestId] IS NULL 
       OR [UserId] IS NULL 
       OR [ProviderId] IS NULL 
       OR [ModelId] IS NULL 
       OR [RequestType] IS NULL 
       OR [RequestText] IS NULL 
       OR [ResponseText] IS NULL;
    
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
    IF NOT EXISTS (SELECT * FROM sys.check_constraints WHERE name = 'CK_LLMUsageLogs_RequestId_NotEmpty')
    BEGIN
        ALTER TABLE [dbo].[LLMUsageLogs] 
        ADD CONSTRAINT CK_LLMUsageLogs_RequestId_NotEmpty 
        CHECK ([RequestId] IS NOT NULL AND LEN([RequestId]) > 0);
        PRINT 'Added constraint: CK_LLMUsageLogs_RequestId_NotEmpty';
    END

    IF NOT EXISTS (SELECT * FROM sys.check_constraints WHERE name = 'CK_LLMUsageLogs_UserId_NotEmpty')
    BEGIN
        ALTER TABLE [dbo].[LLMUsageLogs] 
        ADD CONSTRAINT CK_LLMUsageLogs_UserId_NotEmpty 
        CHECK ([UserId] IS NOT NULL AND LEN([UserId]) > 0);
        PRINT 'Added constraint: CK_LLMUsageLogs_UserId_NotEmpty';
    END

    IF NOT EXISTS (SELECT * FROM sys.check_constraints WHERE name = 'CK_LLMUsageLogs_ProviderId_NotEmpty')
    BEGIN
        ALTER TABLE [dbo].[LLMUsageLogs] 
        ADD CONSTRAINT CK_LLMUsageLogs_ProviderId_NotEmpty 
        CHECK ([ProviderId] IS NOT NULL AND LEN([ProviderId]) > 0);
        PRINT 'Added constraint: CK_LLMUsageLogs_ProviderId_NotEmpty';
    END

    IF NOT EXISTS (SELECT * FROM sys.check_constraints WHERE name = 'CK_LLMUsageLogs_ModelId_NotEmpty')
    BEGIN
        ALTER TABLE [dbo].[LLMUsageLogs] 
        ADD CONSTRAINT CK_LLMUsageLogs_ModelId_NotEmpty 
        CHECK ([ModelId] IS NOT NULL AND LEN([ModelId]) > 0);
        PRINT 'Added constraint: CK_LLMUsageLogs_ModelId_NotEmpty';
    END

    IF NOT EXISTS (SELECT * FROM sys.check_constraints WHERE name = 'CK_LLMUsageLogs_RequestType_NotEmpty')
    BEGIN
        ALTER TABLE [dbo].[LLMUsageLogs] 
        ADD CONSTRAINT CK_LLMUsageLogs_RequestType_NotEmpty 
        CHECK ([RequestType] IS NOT NULL AND LEN([RequestType]) > 0);
        PRINT 'Added constraint: CK_LLMUsageLogs_RequestType_NotEmpty';
    END

    -- =====================================================
    -- Insert sample data to test the fixes
    -- =====================================================

    -- Get admin user ID
    DECLARE @AdminUserId NVARCHAR(256) = (SELECT TOP 1 Id FROM Users WHERE Email LIKE '%admin%' OR UserName LIKE '%admin%');
    
    IF @AdminUserId IS NULL
        SET @AdminUserId = 'admin-user-001';

    -- Insert sample usage logs with proper values
    IF NOT EXISTS (SELECT 1 FROM [dbo].[LLMUsageLogs] WHERE [RequestId] = 'test-fix-001')
    BEGIN
        INSERT INTO [dbo].[LLMUsageLogs]
        ([RequestId], [UserId], [ProviderId], [ModelId], [RequestType], [RequestText], [ResponseText], [InputTokens], [OutputTokens], [TotalTokens], [Cost], [DurationMs], [Success], [Timestamp])
        VALUES
        ('test-fix-001', @AdminUserId, 'openai-main', 'gpt-4-turbo', 'sql_generation', 'Generate SQL for sales report', 'SELECT * FROM Sales WHERE Date >= ''2024-01-01''', 45, 23, 68, 0.00204, 2340, 1, GETUTCDATE()),
        
        ('test-fix-002', @AdminUserId, 'azure-openai-prod', 'gpt-4-azure', 'analysis', 'Analyze customer data trends', 'Based on the data, customer acquisition has increased by 15%...', 78, 45, 123, 0.00369, 3200, 1, DATEADD(minute, -30, GETUTCDATE())),
        
        ('test-fix-003', @AdminUserId, 'openai-main', 'gpt-3.5-turbo', 'chat', 'Explain this chart', 'This chart shows quarterly revenue growth with a clear upward trend...', 32, 18, 50, 0.0001, 1890, 1, DATEADD(hour, -2, GETUTCDATE()));

        PRINT 'Inserted sample usage logs for testing';
    END

    COMMIT TRANSACTION;
    PRINT 'LLMUsageLogs NULL value fixes completed successfully!';
    PRINT '';
    PRINT '✅ Fixed NULL values in string columns';
    PRINT '✅ Added constraints to prevent future NULL values';
    PRINT '✅ Inserted sample data for testing';
    PRINT '';
    PRINT '✅ LLM Management endpoints should now work without SqlNullValueException!';

END TRY
BEGIN CATCH
    ROLLBACK TRANSACTION;
    
    DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
    DECLARE @ErrorSeverity INT = ERROR_SEVERITY();
    DECLARE @ErrorState INT = ERROR_STATE();
    
    PRINT 'Error occurred during LLMUsageLogs fixes:';
    PRINT @ErrorMessage;
    
    RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
END CATCH
