-- ============================================================================
-- CHECK PROCESSFLOW DATA
-- Verify if our enhanced query test saved data to ProcessFlow tables
-- ============================================================================

PRINT '============================================================================';
PRINT 'CHECKING PROCESSFLOW DATA FROM ENHANCED QUERY TEST';
PRINT '============================================================================';
PRINT '';

-- Check ProcessFlowSessions
PRINT 'ProcessFlowSessions:';
SELECT 
    SessionId,
    UserId,
    LEFT(Query, 100) + '...' as QueryPreview,
    QueryType,
    Status,
    StartTime,
    EndTime,
    TotalDurationMs,
    OverallConfidence,
    CASE WHEN GeneratedSQL IS NOT NULL THEN 'Yes' ELSE 'No' END as HasSQL,
    CreatedAt
FROM ProcessFlowSessions 
ORDER BY CreatedAt DESC;

PRINT '';
PRINT 'ProcessFlowSteps:';
SELECT 
    SessionId,
    StepId,
    Name,
    Status,
    StartTime,
    EndTime,
    DurationMs,
    Confidence,
    CASE WHEN InputData IS NOT NULL THEN 'Yes' ELSE 'No' END as HasInput,
    CASE WHEN OutputData IS NOT NULL THEN 'Yes' ELSE 'No' END as HasOutput,
    CreatedAt
FROM ProcessFlowSteps 
ORDER BY SessionId, StepOrder, CreatedAt DESC;

PRINT '';
PRINT 'ProcessFlowLogs:';
SELECT 
    SessionId,
    StepId,
    LogLevel,
    LEFT(Message, 100) + '...' as MessagePreview,
    Source,
    Timestamp
FROM ProcessFlowLogs 
ORDER BY Timestamp DESC;

PRINT '';
PRINT 'ProcessFlowTransparency:';
SELECT 
    SessionId,
    Model,
    Temperature,
    PromptTokens,
    CompletionTokens,
    TotalTokens,
    EstimatedCost,
    Confidence,
    AIProcessingTimeMs,
    ApiCallCount,
    CreatedAt
FROM ProcessFlowTransparency 
ORDER BY CreatedAt DESC;

PRINT '';
PRINT 'Summary Counts:';
SELECT 
    'ProcessFlowSessions' as TableName,
    COUNT(*) as RecordCount
FROM ProcessFlowSessions
UNION ALL
SELECT 
    'ProcessFlowSteps' as TableName,
    COUNT(*) as RecordCount
FROM ProcessFlowSteps
UNION ALL
SELECT 
    'ProcessFlowLogs' as TableName,
    COUNT(*) as RecordCount
FROM ProcessFlowLogs
UNION ALL
SELECT 
    'ProcessFlowTransparency' as TableName,
    COUNT(*) as RecordCount
FROM ProcessFlowTransparency;

PRINT '';
PRINT 'Recent Sessions (Last 10):';
SELECT TOP 10
    s.SessionId,
    s.UserId,
    LEFT(s.Query, 50) + '...' as QueryPreview,
    s.Status,
    s.StartTime,
    COUNT(st.Id) as StepCount,
    COUNT(l.Id) as LogCount,
    CASE WHEN t.Id IS NOT NULL THEN 'Yes' ELSE 'No' END as HasTransparency
FROM ProcessFlowSessions s
LEFT JOIN ProcessFlowSteps st ON s.SessionId = st.SessionId
LEFT JOIN ProcessFlowLogs l ON s.SessionId = l.SessionId
LEFT JOIN ProcessFlowTransparency t ON s.SessionId = t.SessionId
GROUP BY s.SessionId, s.UserId, s.Query, s.Status, s.StartTime, t.Id
ORDER BY s.StartTime DESC;

PRINT '';
PRINT '============================================================================';
PRINT 'PROCESSFLOW DATA CHECK COMPLETED';
PRINT '============================================================================';
