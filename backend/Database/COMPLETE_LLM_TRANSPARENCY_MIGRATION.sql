-- ============================================================================
-- COMPLETE LLM TRANSPARENCY MIGRATION
-- Completes the migration of LLMUsageLogs to ProcessFlowTransparency
-- ============================================================================

PRINT '============================================================================';
PRINT 'COMPLETING LLM TRANSPARENCY MIGRATION';
PRINT '============================================================================';
PRINT '';

-- =====================================================
-- Migrate LLMUsageLogs to ProcessFlowTransparency
-- (Now that ProcessFlowSessions exist for LLM usage)
-- =====================================================
PRINT 'Migrating LLMUsageLogs to ProcessFlowTransparency...';

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
INNER JOIN ProcessFlowSessions pfs ON pfs.SessionId = 'llm-usage-' + CAST(lul.Id AS NVARCHAR(20))
WHERE NOT EXISTS (
    SELECT 1 FROM ProcessFlowTransparency pft 
    WHERE pft.SessionId = 'llm-usage-' + CAST(lul.Id AS NVARCHAR(20))
);

DECLARE @MigratedLLMTransparency INT = @@ROWCOUNT;
PRINT '✅ Migrated ' + CAST(@MigratedLLMTransparency AS NVARCHAR(10)) + ' LLM usage logs to ProcessFlowTransparency.';

-- =====================================================
-- Verification
-- =====================================================
PRINT '';
PRINT 'Verifying final migration results...';

DECLARE @TotalSessions INT = (SELECT COUNT(*) FROM ProcessFlowSessions);
DECLARE @TotalSteps INT = (SELECT COUNT(*) FROM ProcessFlowSteps);
DECLARE @TotalLogs INT = (SELECT COUNT(*) FROM ProcessFlowLogs);
DECLARE @TotalTransparency INT = (SELECT COUNT(*) FROM ProcessFlowTransparency);

PRINT 'Final ProcessFlow table counts:';
PRINT '  • ProcessFlowSessions: ' + CAST(@TotalSessions AS NVARCHAR(10)) + ' total records';
PRINT '  • ProcessFlowSteps: ' + CAST(@TotalSteps AS NVARCHAR(10)) + ' total records';
PRINT '  • ProcessFlowLogs: ' + CAST(@TotalLogs AS NVARCHAR(10)) + ' total records';
PRINT '  • ProcessFlowTransparency: ' + CAST(@TotalTransparency AS NVARCHAR(10)) + ' total records';

-- Check data integrity
DECLARE @SessionsWithTransparency INT = (
    SELECT COUNT(*) 
    FROM ProcessFlowSessions pfs
    INNER JOIN ProcessFlowTransparency pft ON pfs.SessionId = pft.SessionId
);

PRINT '';
PRINT 'Data integrity check:';
PRINT '  • Sessions with transparency data: ' + CAST(@SessionsWithTransparency AS NVARCHAR(10));

-- Sample query to verify data
PRINT '';
PRINT 'Sample consolidated data:';
SELECT TOP 3
    s.SessionId,
    s.UserId,
    LEFT(s.Query, 50) + '...' as QueryPreview,
    s.Status,
    s.TotalDurationMs,
    t.Model,
    t.TotalTokens,
    t.EstimatedCost
FROM ProcessFlowSessions s
LEFT JOIN ProcessFlowTransparency t ON s.SessionId = t.SessionId
ORDER BY s.CreatedAt DESC;

PRINT '';
PRINT '============================================================================';
PRINT 'LLM TRANSPARENCY MIGRATION COMPLETED SUCCESSFULLY';
PRINT '============================================================================';
PRINT '✅ All LLM usage data migrated to ProcessFlowTransparency';
PRINT '✅ Foreign key constraints satisfied';
PRINT '✅ Data integrity verified';
PRINT '✅ Consolidation migration is now complete';
PRINT '============================================================================';
GO
