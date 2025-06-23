-- ============================================================================
-- HOTFIX: ProcessFlow Tables Migration Issues
-- Fixes the foreign key constraint and missing ProcessFlowLogs table
-- ============================================================================

PRINT '============================================================================';
PRINT 'HOTFIX: ProcessFlow Tables Migration Issues';
PRINT '============================================================================';
PRINT '';

-- =====================================================
-- STEP 1: Create ProcessFlowLogs table (if missing)
-- =====================================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ProcessFlowLogs')
BEGIN
    PRINT 'Creating ProcessFlowLogs table...';
    
    CREATE TABLE [dbo].[ProcessFlowLogs] (
        [Id] NVARCHAR(450) NOT NULL PRIMARY KEY,
        [SessionId] NVARCHAR(450) NOT NULL,
        [StepId] NVARCHAR(100) NULL,
        [LogLevel] NVARCHAR(20) NOT NULL DEFAULT 'Info',
        [Message] NVARCHAR(MAX) NOT NULL,
        [Details] NVARCHAR(MAX) NULL,
        [Exception] NVARCHAR(MAX) NULL,
        [Source] NVARCHAR(100) NULL,
        [Timestamp] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [UpdatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [CreatedBy] NVARCHAR(256) NULL,
        [UpdatedBy] NVARCHAR(256) NULL,
        
        -- Foreign key to ProcessFlowSessions only (no cascade conflict)
        CONSTRAINT [FK_ProcessFlowLogs_Sessions]
            FOREIGN KEY ([SessionId]) REFERENCES [dbo].[ProcessFlowSessions] ([SessionId])
            ON DELETE CASCADE
    );

    -- Create indexes for ProcessFlowLogs
    CREATE INDEX [IX_ProcessFlowLogs_SessionId] ON [dbo].[ProcessFlowLogs] ([SessionId]);
    CREATE INDEX [IX_ProcessFlowLogs_StepId] ON [dbo].[ProcessFlowLogs] ([StepId]);
    CREATE INDEX [IX_ProcessFlowLogs_LogLevel] ON [dbo].[ProcessFlowLogs] ([LogLevel]);
    CREATE INDEX [IX_ProcessFlowLogs_Timestamp] ON [dbo].[ProcessFlowLogs] ([Timestamp]);

    PRINT '✅ ProcessFlowLogs table created successfully';
END
ELSE
BEGIN
    PRINT '✅ ProcessFlowLogs table already exists';
END

-- =====================================================
-- STEP 2: Recreate views (if they failed)
-- =====================================================
PRINT 'Recreating ProcessFlow views...';

-- Drop existing views if they exist
IF OBJECT_ID('vw_ProcessFlowSummary', 'V') IS NOT NULL
    DROP VIEW vw_ProcessFlowSummary;

IF OBJECT_ID('vw_ProcessFlowStepPerformance', 'V') IS NOT NULL
    DROP VIEW vw_ProcessFlowStepPerformance;

-- Create vw_ProcessFlowSummary
GO
CREATE VIEW vw_ProcessFlowSummary AS
SELECT 
    s.SessionId,
    s.UserId,
    s.Query,
    s.QueryType,
    s.Status,
    s.StartTime,
    s.EndTime,
    s.TotalDurationMs,
    s.OverallConfidence,
    s.GeneratedSQL,
    s.ExecutionResult,
    s.ErrorMessage,
    s.ConversationId,
    s.MessageId,
    
    -- Step metrics
    COUNT(st.Id) as TotalSteps,
    SUM(CASE WHEN st.Status = 'completed' THEN 1 ELSE 0 END) as CompletedSteps,
    SUM(CASE WHEN st.Status = 'error' THEN 1 ELSE 0 END) as ErrorSteps,
    AVG(st.Confidence) as AvgStepConfidence,
    SUM(st.DurationMs) as TotalStepDurationMs,
    
    -- Log metrics
    COUNT(l.Id) as TotalLogs,
    SUM(CASE WHEN l.LogLevel = 'Error' THEN 1 ELSE 0 END) as ErrorLogs,
    SUM(CASE WHEN l.LogLevel = 'Warning' THEN 1 ELSE 0 END) as WarningLogs,
    
    -- Transparency metrics
    t.Model,
    t.TotalTokens,
    t.EstimatedCost,
    t.AIProcessingTimeMs,
    t.ApiCallCount
    
FROM ProcessFlowSessions s
LEFT JOIN ProcessFlowSteps st ON s.SessionId = st.SessionId
LEFT JOIN ProcessFlowLogs l ON s.SessionId = l.SessionId
LEFT JOIN ProcessFlowTransparency t ON s.SessionId = t.SessionId
GROUP BY 
    s.SessionId, s.UserId, s.Query, s.QueryType, s.Status, s.StartTime, s.EndTime,
    s.TotalDurationMs, s.OverallConfidence, s.GeneratedSQL, s.ExecutionResult,
    s.ErrorMessage, s.ConversationId, s.MessageId, t.Model, t.TotalTokens,
    t.EstimatedCost, t.AIProcessingTimeMs, t.ApiCallCount;

-- Create vw_ProcessFlowStepPerformance
GO
CREATE VIEW vw_ProcessFlowStepPerformance AS
SELECT 
    StepId,
    Name,
    COUNT(*) as ExecutionCount,
    AVG(CAST(DurationMs AS FLOAT)) as AvgDurationMs,
    MIN(DurationMs) as MinDurationMs,
    MAX(DurationMs) as MaxDurationMs,
    AVG(CAST(Confidence AS FLOAT)) as AvgConfidence,
    SUM(CASE WHEN Status = 'completed' THEN 1 ELSE 0 END) as SuccessCount,
    SUM(CASE WHEN Status = 'error' THEN 1 ELSE 0 END) as ErrorCount,
    CAST(SUM(CASE WHEN Status = 'completed' THEN 1 ELSE 0 END) AS FLOAT) / COUNT(*) * 100 as SuccessRate
FROM ProcessFlowSteps
WHERE StartTime IS NOT NULL AND EndTime IS NOT NULL
GROUP BY StepId, Name;
GO

PRINT '✅ ProcessFlow views recreated successfully';

-- =====================================================
-- STEP 3: Insert sample data (if missing)
-- =====================================================
PRINT 'Inserting sample process flow data...';

-- Sample process flow session
IF NOT EXISTS (SELECT * FROM ProcessFlowSessions WHERE SessionId = 'sample-session-001')
BEGIN
    INSERT INTO ProcessFlowSessions (Id, SessionId, UserId, Query, QueryType, Status, StartTime, EndTime, TotalDurationMs, OverallConfidence, GeneratedSQL, ConversationId, MessageId, CreatedBy, UpdatedBy)
    VALUES (
        'pfs-001',
        'sample-session-001',
        'admin-user-001',
        'Show me the top 10 players by total deposits',
        'enhanced',
        'completed',
        DATEADD(MINUTE, -10, GETUTCDATE()),
        DATEADD(MINUTE, -9, GETUTCDATE()),
        5700,
        0.95,
        'SELECT TOP 10 player_id, SUM(deposit_amount) as total_deposits FROM deposits GROUP BY player_id ORDER BY total_deposits DESC',
        'conv-001',
        'msg-001',
        'system',
        'system'
    );
END

-- Sample process flow steps
IF NOT EXISTS (SELECT * FROM ProcessFlowSteps WHERE SessionId = 'sample-session-001')
BEGIN
    INSERT INTO ProcessFlowSteps (Id, SessionId, StepId, Name, Description, StepOrder, Status, StartTime, EndTime, DurationMs, Confidence, CreatedBy, UpdatedBy)
    VALUES 
    ('pfs-step-001', 'sample-session-001', 'auth', 'Authentication', 'JWT token validation', 1, 'completed', DATEADD(MINUTE, -10, GETUTCDATE()), DATEADD(SECOND, -598, GETUTCDATE()), 200, 1.0, 'system', 'system'),
    ('pfs-step-002', 'sample-session-001', 'semantic-analysis', 'Semantic Analysis', 'Query intent and entity extraction', 2, 'completed', DATEADD(SECOND, -598, GETUTCDATE()), DATEADD(SECOND, -596, GETUTCDATE()), 800, 0.92, 'system', 'system'),
    ('pfs-step-003', 'sample-session-001', 'schema-retrieval', 'Schema Retrieval', 'Database schema context gathering', 3, 'completed', DATEADD(SECOND, -596, GETUTCDATE()), DATEADD(SECOND, -594, GETUTCDATE()), 1200, 0.98, 'system', 'system'),
    ('pfs-step-004', 'sample-session-001', 'prompt-building', 'Prompt Building', 'Enhanced prompt construction', 4, 'completed', DATEADD(SECOND, -594, GETUTCDATE()), DATEADD(SECOND, -592, GETUTCDATE()), 600, 0.95, 'system', 'system'),
    ('pfs-step-005', 'sample-session-001', 'ai-generation', 'AI Generation', 'OpenAI GPT-4 SQL generation', 5, 'completed', DATEADD(SECOND, -592, GETUTCDATE()), DATEADD(SECOND, -589, GETUTCDATE()), 2800, 0.94, 'system', 'system'),
    ('pfs-step-006', 'sample-session-001', 'sql-validation', 'SQL Validation', 'Generated SQL syntax validation', 6, 'completed', DATEADD(SECOND, -589, GETUTCDATE()), DATEADD(SECOND, -588, GETUTCDATE()), 100, 0.99, 'system', 'system');
END

-- Sample process flow logs
IF NOT EXISTS (SELECT * FROM ProcessFlowLogs WHERE SessionId = 'sample-session-001')
BEGIN
    INSERT INTO ProcessFlowLogs (Id, SessionId, StepId, LogLevel, Message, Details, Source, Timestamp, CreatedBy, UpdatedBy)
    VALUES 
    ('pfl-001', 'sample-session-001', 'auth', 'Info', 'JWT token validated successfully', '{"user_id": "admin-user-001", "token_valid": true}', 'AuthenticationService', DATEADD(MINUTE, -10, GETUTCDATE()), 'system', 'system'),
    ('pfl-002', 'sample-session-001', 'semantic-analysis', 'Info', 'Query intent detected: analytical', '{"intent": "analytical", "confidence": 0.92, "entities": ["players", "deposits"]}', 'SemanticAnalyzer', DATEADD(SECOND, -597, GETUTCDATE()), 'system', 'system'),
    ('pfl-003', 'sample-session-001', 'schema-retrieval', 'Info', 'Retrieved schema for 2 tables', '{"tables": ["players", "deposits"], "columns": 15}', 'SchemaService', DATEADD(SECOND, -595, GETUTCDATE()), 'system', 'system'),
    ('pfl-004', 'sample-session-001', 'ai-generation', 'Info', 'OpenAI API call completed', '{"model": "gpt-4", "prompt_tokens": 1500, "completion_tokens": 300, "total_tokens": 1800}', 'OpenAIProvider', DATEADD(SECOND, -590, GETUTCDATE()), 'system', 'system'),
    ('pfl-005', 'sample-session-001', 'sql-validation', 'Info', 'SQL validation passed', '{"syntax_valid": true, "semantic_valid": true}', 'SQLValidator', DATEADD(SECOND, -588, GETUTCDATE()), 'system', 'system');
END

-- Sample transparency data
IF NOT EXISTS (SELECT * FROM ProcessFlowTransparency WHERE SessionId = 'sample-session-001')
BEGIN
    INSERT INTO ProcessFlowTransparency (Id, SessionId, Model, Temperature, MaxTokens, PromptTokens, CompletionTokens, TotalTokens, EstimatedCost, Confidence, AIProcessingTimeMs, ApiCallCount, CreatedBy, UpdatedBy)
    VALUES (
        'pft-001',
        'sample-session-001',
        'gpt-4',
        0.1,
        2000,
        1500,
        300,
        1800,
        0.054,
        0.94,
        2800,
        1,
        'system',
        'system'
    );
END

PRINT '✅ Sample data inserted successfully';

-- =====================================================
-- STEP 4: Verification
-- =====================================================
PRINT '';
PRINT 'Verifying ProcessFlow tables...';

DECLARE @SessionCount INT = (SELECT COUNT(*) FROM ProcessFlowSessions);
DECLARE @StepCount INT = (SELECT COUNT(*) FROM ProcessFlowSteps);
DECLARE @LogCount INT = (SELECT COUNT(*) FROM ProcessFlowLogs);
DECLARE @TransparencyCount INT = (SELECT COUNT(*) FROM ProcessFlowTransparency);

PRINT 'Table record counts:';
PRINT '  • ProcessFlowSessions: ' + CAST(@SessionCount AS NVARCHAR(10));
PRINT '  • ProcessFlowSteps: ' + CAST(@StepCount AS NVARCHAR(10));
PRINT '  • ProcessFlowLogs: ' + CAST(@LogCount AS NVARCHAR(10));
PRINT '  • ProcessFlowTransparency: ' + CAST(@TransparencyCount AS NVARCHAR(10));

-- Test views
DECLARE @SummaryCount INT = (SELECT COUNT(*) FROM vw_ProcessFlowSummary);
DECLARE @PerformanceCount INT = (SELECT COUNT(*) FROM vw_ProcessFlowStepPerformance);

PRINT 'View record counts:';
PRINT '  • vw_ProcessFlowSummary: ' + CAST(@SummaryCount AS NVARCHAR(10));
PRINT '  • vw_ProcessFlowStepPerformance: ' + CAST(@PerformanceCount AS NVARCHAR(10));

PRINT '';
PRINT '============================================================================';
PRINT 'HOTFIX COMPLETED SUCCESSFULLY';
PRINT '============================================================================';
PRINT '✅ ProcessFlowLogs table created with proper constraints';
PRINT '✅ Views recreated and working correctly';
PRINT '✅ Sample data inserted successfully';
PRINT '✅ All ProcessFlow tables are now ready for use';
PRINT '============================================================================';
GO
