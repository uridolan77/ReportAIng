-- =====================================================
-- CONSOLIDATE TRANSPARENCY TABLES - REMOVE DUPLICATES
-- =====================================================
-- This script consolidates overlapping transparency tables
-- and removes redundant data structures
-- Database: BIReportingCopilot_dev
-- =====================================================

USE [BIReportingCopilot_dev]
GO

BEGIN TRANSACTION;

BEGIN TRY
    PRINT '🔍 Starting Transparency Tables Consolidation...';
    PRINT '';

    -- =====================================================
    -- STEP 1: ANALYZE CURRENT DATA
    -- =====================================================
    
    PRINT '📊 Current Table Status:';
    
    DECLARE @PromptConstructionTracesCount INT = (SELECT COUNT(*) FROM [dbo].[PromptConstructionTraces]);
    DECLARE @PromptConstructionStepsCount INT = (SELECT COUNT(*) FROM [dbo].[PromptConstructionSteps]);
    DECLARE @PromptGenerationLogsCount INT = (SELECT COUNT(*) FROM [dbo].[PromptGenerationLogs]);
    DECLARE @PromptLogsCount INT = (SELECT COUNT(*) FROM [dbo].[PromptLogs]);
    DECLARE @AIGenerationAttemptsCount INT = (SELECT COUNT(*) FROM [dbo].[AIGenerationAttempts]);
    
    PRINT '  • PromptConstructionTraces: ' + CAST(@PromptConstructionTracesCount AS NVARCHAR(10)) + ' records (KEEP - Primary transparency)';
    PRINT '  • PromptConstructionSteps: ' + CAST(@PromptConstructionStepsCount AS NVARCHAR(10)) + ' records (KEEP - Detailed steps)';
    PRINT '  • AIGenerationAttempts: ' + CAST(@AIGenerationAttemptsCount AS NVARCHAR(10)) + ' records (KEEP - SQL generation focus)';
    PRINT '  • PromptGenerationLogs: ' + CAST(@PromptGenerationLogsCount AS NVARCHAR(10)) + ' records (REMOVE - Redundant)';
    PRINT '  • PromptLogs: ' + CAST(@PromptLogsCount AS NVARCHAR(10)) + ' records (REMOVE - Basic logging)';
    PRINT '';

    -- =====================================================
    -- STEP 2: MIGRATE USEFUL DATA FROM REDUNDANT TABLES
    -- =====================================================
    
    PRINT '🔄 Migrating useful data from redundant tables...';
    
    -- Migrate PromptGenerationLogs data to PromptConstructionTraces (if not already there)
    IF @PromptGenerationLogsCount > 0
    BEGIN
        PRINT '  • Checking PromptGenerationLogs for unique data...';
        
        -- Insert unique records from PromptGenerationLogs that don't exist in PromptConstructionTraces
        INSERT INTO [dbo].[PromptConstructionTraces]
        ([Id], [TraceId], [UserId], [UserQuestion], [IntentType], [StartTime], [EndTime], 
         [OverallConfidence], [TotalTokens], [FinalPrompt], [Success], [ErrorMessage], 
         [Metadata], [CreatedAt], [UpdatedAt])
        SELECT 
            NEWID() as [Id],
            'migrated-' + CAST([Id] AS NVARCHAR(20)) as [TraceId],
            [UserId],
            [UserQuestion],
            [IntentType],
            [Timestamp] as [StartTime],
            DATEADD(millisecond, [GenerationTimeMs], [Timestamp]) as [EndTime],
            [ConfidenceScore] as [OverallConfidence],
            ISNULL([TokensUsed], 0) as [TotalTokens],
            [GeneratedPrompt] as [FinalPrompt],
            [WasSuccessful] as [Success],
            [ErrorMessage],
            JSON_QUERY('{"source": "PromptGenerationLogs", "domain": "' + [Domain] + '", "template": "' + ISNULL([TemplateUsed], '') + '", "tables_used": ' + CAST([TablesUsed] AS NVARCHAR(10)) + '}') as [Metadata],
            [Timestamp] as [CreatedAt],
            [Timestamp] as [UpdatedAt]
        FROM [dbo].[PromptGenerationLogs] pgl
        WHERE NOT EXISTS (
            SELECT 1 FROM [dbo].[PromptConstructionTraces] pct 
            WHERE pct.[UserId] = pgl.[UserId] 
            AND pct.[UserQuestion] = pgl.[UserQuestion]
            AND ABS(DATEDIFF(second, pct.[CreatedAt], pgl.[Timestamp])) < 60
        );
        
        PRINT '    ✅ Migrated unique PromptGenerationLogs data';
    END
    
    -- Migrate PromptLogs data to PromptConstructionTraces (if not already there)
    IF @PromptLogsCount > 0
    BEGIN
        PRINT '  • Checking PromptLogs for unique data...';
        
        -- Insert unique records from PromptLogs that don't exist in PromptConstructionTraces
        INSERT INTO [dbo].[PromptConstructionTraces]
        ([Id], [TraceId], [UserId], [UserQuestion], [IntentType], [StartTime], [EndTime], 
         [OverallConfidence], [TotalTokens], [FinalPrompt], [Success], [ErrorMessage], 
         [Metadata], [CreatedAt], [UpdatedAt])
        SELECT 
            NEWID() as [Id],
            'prompt-log-' + CAST([Id] AS NVARCHAR(20)) as [TraceId],
            [UserId],
            'Prompt: ' + LEFT([PromptText], 100) + '...' as [UserQuestion],
            'GENERAL' as [IntentType],
            [CreatedAt] as [StartTime],
            [CreatedAt] as [EndTime],
            [ImportanceScore] as [OverallConfidence],
            0 as [TotalTokens],
            [PromptText] as [FinalPrompt],
            CASE WHEN [ResponseText] IS NOT NULL THEN 1 ELSE 0 END as [Success],
            NULL as [ErrorMessage],
            JSON_QUERY('{"source": "PromptLogs", "business_purpose": "' + ISNULL([BusinessPurpose], '') + '", "importance": ' + CAST([ImportanceScore] AS NVARCHAR(10)) + '}') as [Metadata],
            [CreatedAt],
            [CreatedAt] as [UpdatedAt]
        FROM [dbo].[PromptLogs] pl
        WHERE NOT EXISTS (
            SELECT 1 FROM [dbo].[PromptConstructionTraces] pct 
            WHERE pct.[UserId] = pl.[UserId] 
            AND ABS(DATEDIFF(second, pct.[CreatedAt], pl.[CreatedAt])) < 60
        );
        
        PRINT '    ✅ Migrated unique PromptLogs data';
    END

    -- =====================================================
    -- STEP 3: BACKUP REDUNDANT TABLES (OPTIONAL)
    -- =====================================================
    
    PRINT '';
    PRINT '💾 Creating backup tables (optional - can be removed later)...';
    
    -- Create backup tables with _BACKUP suffix
    IF @PromptGenerationLogsCount > 0
    BEGIN
        IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'PromptGenerationLogs_BACKUP')
        BEGIN
            SELECT * INTO [dbo].[PromptGenerationLogs_BACKUP] FROM [dbo].[PromptGenerationLogs];
            PRINT '  ✅ Created PromptGenerationLogs_BACKUP';
        END
    END
    
    IF @PromptLogsCount > 0
    BEGIN
        IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'PromptLogs_BACKUP')
        BEGIN
            SELECT * INTO [dbo].[PromptLogs_BACKUP] FROM [dbo].[PromptLogs];
            PRINT '  ✅ Created PromptLogs_BACKUP';
        END
    END

    -- =====================================================
    -- STEP 4: DROP REDUNDANT TABLES
    -- =====================================================
    
    PRINT '';
    PRINT '🗑️ Removing redundant tables...';
    
    -- Drop foreign key constraints first
    IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_PromptGenerationLogs_PromptTemplates')
    BEGIN
        ALTER TABLE [dbo].[PromptGenerationLogs] DROP CONSTRAINT [FK_PromptGenerationLogs_PromptTemplates];
        PRINT '  • Dropped FK_PromptGenerationLogs_PromptTemplates constraint';
    END
    
    IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_PromptGenerationLogs_Users')
    BEGIN
        ALTER TABLE [dbo].[PromptGenerationLogs] DROP CONSTRAINT [FK_PromptGenerationLogs_Users];
        PRINT '  • Dropped FK_PromptGenerationLogs_Users constraint';
    END
    
    -- Drop the redundant tables
    IF EXISTS (SELECT * FROM sys.tables WHERE name = 'PromptGenerationLogs')
    BEGIN
        DROP TABLE [dbo].[PromptGenerationLogs];
        PRINT '  ✅ Dropped PromptGenerationLogs table';
    END
    
    IF EXISTS (SELECT * FROM sys.tables WHERE name = 'PromptLogs')
    BEGIN
        DROP TABLE [dbo].[PromptLogs];
        PRINT '  ✅ Dropped PromptLogs table';
    END

    -- =====================================================
    -- STEP 5: VERIFY FINAL STATE
    -- =====================================================
    
    PRINT '';
    PRINT '✅ Final Table Status:';
    
    DECLARE @FinalTracesCount INT = (SELECT COUNT(*) FROM [dbo].[PromptConstructionTraces]);
    DECLARE @FinalStepsCount INT = (SELECT COUNT(*) FROM [dbo].[PromptConstructionSteps]);
    DECLARE @FinalAttemptsCount INT = (SELECT COUNT(*) FROM [dbo].[AIGenerationAttempts]);
    
    PRINT '  • PromptConstructionTraces: ' + CAST(@FinalTracesCount AS NVARCHAR(10)) + ' records (PRIMARY transparency table)';
    PRINT '  • PromptConstructionSteps: ' + CAST(@FinalStepsCount AS NVARCHAR(10)) + ' records (Detailed step tracking)';
    PRINT '  • AIGenerationAttempts: ' + CAST(@FinalAttemptsCount AS NVARCHAR(10)) + ' records (SQL generation tracking)';
    PRINT '';
    
    -- =====================================================
    -- STEP 6: UPDATE APPLICATION INTEGRATION
    -- =====================================================
    
    PRINT '🔧 Transparency System Integration Status:';
    PRINT '  ✅ Primary Table: PromptConstructionTraces (integrated with QueryController)';
    PRINT '  ✅ Step Details: PromptConstructionSteps (automatic via PromptConstructionTracer)';
    PRINT '  ✅ SQL Tracking: AIGenerationAttempts (existing functionality)';
    PRINT '';
    PRINT '📋 Recommended Next Steps:';
    PRINT '  1. Update any code references to removed tables';
    PRINT '  2. Test transparency endpoints with consolidated data';
    PRINT '  3. Remove backup tables after verification (optional)';
    PRINT '  4. Frontend should now display real transparency data!';

    COMMIT TRANSACTION;
    PRINT '';
    PRINT '🎉 TRANSPARENCY TABLES CONSOLIDATION COMPLETED SUCCESSFULLY! 🎉';
    PRINT '';
    PRINT '✅ Removed duplicate/redundant tables';
    PRINT '✅ Preserved all useful data';
    PRINT '✅ Streamlined transparency system';
    PRINT '✅ Ready for frontend integration';

END TRY
BEGIN CATCH
    ROLLBACK TRANSACTION;
    
    DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
    DECLARE @ErrorSeverity INT = ERROR_SEVERITY();
    DECLARE @ErrorState INT = ERROR_STATE();
    
    PRINT 'Error occurred during transparency tables consolidation:';
    PRINT @ErrorMessage;
    
    RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
END CATCH
