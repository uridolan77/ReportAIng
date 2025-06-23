-- ============================================================================
-- TRANSPARENCY TABLES CONSOLIDATION MIGRATION
-- Migrates data from old transparency tables to new ProcessFlow tables
-- ============================================================================

PRINT '============================================================================';
PRINT 'STARTING TRANSPARENCY TABLES CONSOLIDATION MIGRATION';
PRINT '============================================================================';
PRINT '';

-- =====================================================
-- STEP 1: Ensure ProcessFlow tables exist
-- =====================================================
PRINT 'STEP 1: Verifying ProcessFlow tables exist...';

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ProcessFlowSessions')
BEGIN
    PRINT '❌ ERROR: ProcessFlow tables not found. Please run Migration_ProcessFlow_Tables.sql first.';
    RETURN;
END

PRINT '✅ ProcessFlow tables verified.';
PRINT '';

-- =====================================================
-- STEP 2: Migrate PromptConstructionTraces to ProcessFlowSessions
-- =====================================================
PRINT 'STEP 2: Migrating PromptConstructionTraces to ProcessFlowSessions...';

INSERT INTO ProcessFlowSessions (
    Id, SessionId, UserId, Query, QueryType, Status, StartTime, EndTime, 
    TotalDurationMs, OverallConfidence, GeneratedSQL, ExecutionResult, 
    ErrorMessage, Metadata, ConversationId, MessageId, CreatedAt, UpdatedAt, 
    CreatedBy, UpdatedBy
)
SELECT 
    NEWID() as Id,
    'migrated-' + TraceId as SessionId,
    UserId,
    UserQuestion as Query,
    'enhanced' as QueryType,
    CASE WHEN Success = 1 THEN 'completed' ELSE 'error' END as Status,
    StartTime,
    EndTime,
    CASE 
        WHEN EndTime IS NOT NULL THEN DATEDIFF(MILLISECOND, StartTime, EndTime)
        ELSE NULL 
    END as TotalDurationMs,
    OverallConfidence,
    FinalPrompt as GeneratedSQL,  -- FinalPrompt often contains the generated SQL
    NULL as ExecutionResult,
    ErrorMessage,
    Metadata,
    NULL as ConversationId,
    NULL as MessageId,
    CreatedAt,
    UpdatedAt,
    'migration' as CreatedBy,
    'migration' as UpdatedBy
FROM PromptConstructionTraces pct
WHERE NOT EXISTS (
    SELECT 1 FROM ProcessFlowSessions pfs 
    WHERE pfs.SessionId = 'migrated-' + pct.TraceId
);

DECLARE @MigratedTraces INT = @@ROWCOUNT;
PRINT '✅ Migrated ' + CAST(@MigratedTraces AS NVARCHAR(10)) + ' traces to ProcessFlowSessions.';

-- =====================================================
-- STEP 3: Migrate PromptConstructionSteps to ProcessFlowSteps
-- =====================================================
PRINT 'STEP 3: Migrating PromptConstructionSteps to ProcessFlowSteps...';

INSERT INTO ProcessFlowSteps (
    Id, SessionId, StepId, ParentStepId, Name, Description, StepOrder, 
    Status, StartTime, EndTime, DurationMs, Confidence, InputData, 
    OutputData, ErrorMessage, Metadata, RetryCount, CreatedAt, UpdatedAt, 
    CreatedBy, UpdatedBy
)
SELECT 
    NEWID() as Id,
    'migrated-' + pcs.TraceId as SessionId,
    LOWER(REPLACE(REPLACE(pcs.StepName, ' ', '-'), '_', '-')) as StepId,
    NULL as ParentStepId,
    pcs.StepName as Name,
    pcs.Content as Description,
    pcs.StepOrder,
    CASE WHEN pcs.Success = 1 THEN 'completed' ELSE 'error' END as Status,
    pcs.StartTime,
    pcs.EndTime,
    CASE 
        WHEN pcs.EndTime IS NOT NULL THEN DATEDIFF(MILLISECOND, pcs.StartTime, pcs.EndTime)
        ELSE NULL 
    END as DurationMs,
    pcs.Confidence,
    NULL as InputData,
    pcs.Content as OutputData,
    pcs.ErrorMessage,
    pcs.Details as Metadata,
    0 as RetryCount,
    pcs.CreatedAt,
    pcs.CreatedAt as UpdatedAt,
    'migration' as CreatedBy,
    'migration' as UpdatedBy
FROM PromptConstructionSteps pcs
INNER JOIN PromptConstructionTraces pct ON pcs.TraceId = pct.TraceId
WHERE NOT EXISTS (
    SELECT 1 FROM ProcessFlowSteps pfs 
    WHERE pfs.SessionId = 'migrated-' + pcs.TraceId 
    AND pfs.StepId = LOWER(REPLACE(REPLACE(pcs.StepName, ' ', '-'), '_', '-'))
);

DECLARE @MigratedSteps INT = @@ROWCOUNT;
PRINT '✅ Migrated ' + CAST(@MigratedSteps AS NVARCHAR(10)) + ' steps to ProcessFlowSteps.';

-- =====================================================
-- STEP 4: Migrate PromptSuccessTracking to ProcessFlowSessions
-- =====================================================
PRINT 'STEP 4: Migrating PromptSuccessTracking to ProcessFlowSessions...';

INSERT INTO ProcessFlowSessions (
    Id, SessionId, UserId, Query, QueryType, Status, StartTime, EndTime, 
    TotalDurationMs, OverallConfidence, GeneratedSQL, ExecutionResult, 
    ErrorMessage, Metadata, ConversationId, MessageId, CreatedAt, UpdatedAt, 
    CreatedBy, UpdatedBy
)
SELECT 
    NEWID() as Id,
    'prompt-success-' + CAST(pst.Id AS NVARCHAR(20)) as SessionId,
    pst.UserId,
    pst.UserQuestion as Query,
    'enhanced' as QueryType,
    CASE 
        WHEN pst.SQLExecutionSuccess = 1 THEN 'completed' 
        WHEN pst.SQLExecutionSuccess = 0 THEN 'error'
        ELSE 'running'
    END as Status,
    pst.CreatedDate as StartTime,
    pst.UpdatedDate as EndTime,
    pst.ProcessingTimeMs as TotalDurationMs,
    pst.ConfidenceScore as OverallConfidence,
    pst.GeneratedSQL,
    CASE 
        WHEN pst.SQLExecutionSuccess = 1 THEN 'Success'
        WHEN pst.SQLExecutionError IS NOT NULL THEN pst.SQLExecutionError
        ELSE NULL
    END as ExecutionResult,
    pst.SQLExecutionError as ErrorMessage,
    '{"TemplateUsed":"' + ISNULL(pst.TemplateUsed, '') +
    '","IntentClassified":"' + ISNULL(pst.IntentClassified, '') +
    '","DomainClassified":"' + ISNULL(pst.DomainClassified, '') +
    '","TablesRetrieved":"' + ISNULL(pst.TablesRetrieved, '') +
    '","UserFeedbackRating":' + ISNULL(CAST(pst.UserFeedbackRating AS NVARCHAR(10)), 'null') +
    ',"UserFeedbackComments":"' + ISNULL(pst.UserFeedbackComments, '') + '"}' as Metadata,
    NULL as ConversationId,
    NULL as MessageId,
    pst.CreatedDate as CreatedAt,
    ISNULL(pst.UpdatedDate, pst.CreatedDate) as UpdatedAt,
    'migration' as CreatedBy,
    'migration' as UpdatedBy
FROM PromptSuccessTracking pst
WHERE NOT EXISTS (
    SELECT 1 FROM ProcessFlowSessions pfs 
    WHERE pfs.SessionId = 'prompt-success-' + CAST(pst.Id AS NVARCHAR(20))
);

DECLARE @MigratedPromptSuccess INT = @@ROWCOUNT;
PRINT '✅ Migrated ' + CAST(@MigratedPromptSuccess AS NVARCHAR(10)) + ' prompt success records to ProcessFlowSessions.';

-- =====================================================
-- STEP 5: Create ProcessFlowSessions for LLM usage FIRST
-- =====================================================
PRINT 'STEP 5: Creating ProcessFlowSessions for LLM usage...';

INSERT INTO ProcessFlowSessions (
    Id, SessionId, UserId, Query, QueryType, Status, StartTime, EndTime,
    TotalDurationMs, OverallConfidence, GeneratedSQL, ExecutionResult,
    ErrorMessage, Metadata, ConversationId, MessageId, CreatedAt, UpdatedAt,
    CreatedBy, UpdatedBy
)
SELECT
    NEWID() as Id,
    'llm-usage-' + CAST(lul.Id AS NVARCHAR(20)) as SessionId,
    lul.UserId,
    LEFT(lul.RequestText, 1000) as Query,  -- Truncate long requests
    lul.RequestType as QueryType,
    CASE WHEN lul.Success = 1 THEN 'completed' ELSE 'error' END as Status,
    lul.Timestamp as StartTime,
    DATEADD(MILLISECOND, lul.DurationMs, lul.Timestamp) as EndTime,
    lul.DurationMs as TotalDurationMs,
    NULL as OverallConfidence,
    CASE
        WHEN lul.RequestType = 'SQL_GENERATION' THEN lul.ResponseText
        ELSE NULL
    END as GeneratedSQL,
    CASE WHEN lul.Success = 1 THEN 'Success' ELSE lul.ErrorMessage END as ExecutionResult,
    CASE WHEN lul.Success = 0 THEN lul.ErrorMessage ELSE NULL END as ErrorMessage,
    lul.Metadata,
    NULL as ConversationId,
    NULL as MessageId,
    lul.Timestamp as CreatedAt,
    lul.Timestamp as UpdatedAt,
    'migration' as CreatedBy,
    'migration' as UpdatedBy
FROM LLMUsageLogs lul
WHERE NOT EXISTS (
    SELECT 1 FROM ProcessFlowSessions pfs
    WHERE pfs.SessionId = 'llm-usage-' + CAST(lul.Id AS NVARCHAR(20))
);

DECLARE @CreatedLLMSessions INT = @@ROWCOUNT;
PRINT '✅ Created ' + CAST(@CreatedLLMSessions AS NVARCHAR(10)) + ' ProcessFlowSessions for LLM usage.';

-- =====================================================
-- STEP 6: Now migrate LLMUsageLogs to ProcessFlowTransparency
-- =====================================================
PRINT 'STEP 6: Migrating LLMUsageLogs to ProcessFlowTransparency...';

-- Create ProcessFlowTransparency records for LLM usage logs
INSERT INTO ProcessFlowTransparency (
    Id, SessionId, Model, Temperature, MaxTokens, PromptTokens, 
    CompletionTokens, TotalTokens, EstimatedCost, Confidence, 
    AIProcessingTimeMs, ApiCallCount, PromptDetails, ResponseAnalysis, 
    QualityMetrics, OptimizationSuggestions, CreatedAt, UpdatedAt, 
    CreatedBy, UpdatedBy
)
SELECT 
    NEWID() as Id,
    'llm-usage-' + CAST(lul.Id AS NVARCHAR(20)) as SessionId,
    lul.ModelId as Model,
    NULL as Temperature,
    NULL as MaxTokens,
    lul.InputTokens as PromptTokens,
    lul.OutputTokens as CompletionTokens,
    lul.TotalTokens,
    lul.Cost as EstimatedCost,
    NULL as Confidence,
    lul.DurationMs as AIProcessingTimeMs,
    1 as ApiCallCount,
    lul.RequestText as PromptDetails,
    lul.ResponseText as ResponseAnalysis,
    '{"Success":' + CAST(lul.Success AS NVARCHAR(5)) +
    ',"ProviderId":"' + ISNULL(lul.ProviderId, '') +
    '","RequestType":"' + ISNULL(lul.RequestType, '') + '"}' as QualityMetrics,
    NULL as OptimizationSuggestions,
    lul.Timestamp as CreatedAt,
    lul.Timestamp as UpdatedAt,
    'migration' as CreatedBy,
    'migration' as UpdatedBy
FROM LLMUsageLogs lul
WHERE NOT EXISTS (
    SELECT 1 FROM ProcessFlowTransparency pft 
    WHERE pft.SessionId = 'llm-usage-' + CAST(lul.Id AS NVARCHAR(20))
);

DECLARE @MigratedLLMUsage INT = @@ROWCOUNT;
PRINT '✅ Migrated ' + CAST(@MigratedLLMUsage AS NVARCHAR(10)) + ' LLM usage logs to ProcessFlowTransparency.';

-- =====================================================
-- STEP 7: Migration Summary
-- =====================================================
PRINT '';
PRINT '============================================================================';
PRINT 'CONSOLIDATION MIGRATION COMPLETED SUCCESSFULLY';
PRINT '============================================================================';
PRINT 'Migration Summary:';
PRINT '  • PromptConstructionTraces → ProcessFlowSessions: ' + CAST(@MigratedTraces AS NVARCHAR(10)) + ' records';
PRINT '  • PromptConstructionSteps → ProcessFlowSteps: ' + CAST(@MigratedSteps AS NVARCHAR(10)) + ' records';
PRINT '  • PromptSuccessTracking → ProcessFlowSessions: ' + CAST(@MigratedPromptSuccess AS NVARCHAR(10)) + ' records';
PRINT '  • LLMUsageLogs → ProcessFlowTransparency: ' + CAST(@MigratedLLMUsage AS NVARCHAR(10)) + ' records';
PRINT '  • LLMUsageLogs → ProcessFlowSessions: ' + CAST(@CreatedLLMSessions AS NVARCHAR(10)) + ' records';
PRINT '  • LLMUsageLogs → ProcessFlowTransparency: ' + CAST(@MigratedLLMUsage AS NVARCHAR(10)) + ' records';
PRINT '';

-- Get final counts
DECLARE @TotalProcessFlowSessions INT = (SELECT COUNT(*) FROM ProcessFlowSessions);
DECLARE @TotalProcessFlowSteps INT = (SELECT COUNT(*) FROM ProcessFlowSteps);
DECLARE @TotalProcessFlowTransparency INT = (SELECT COUNT(*) FROM ProcessFlowTransparency);

PRINT 'Final ProcessFlow table counts:';
PRINT '  • ProcessFlowSessions: ' + CAST(@TotalProcessFlowSessions AS NVARCHAR(10)) + ' total records';
PRINT '  • ProcessFlowSteps: ' + CAST(@TotalProcessFlowSteps AS NVARCHAR(10)) + ' total records';
PRINT '  • ProcessFlowTransparency: ' + CAST(@TotalProcessFlowTransparency AS NVARCHAR(10)) + ' total records';
PRINT '';
PRINT '✅ All historical transparency data has been consolidated into ProcessFlow tables.';
PRINT '✅ Old transparency tables can now be safely deprecated.';
PRINT '============================================================================';
GO
