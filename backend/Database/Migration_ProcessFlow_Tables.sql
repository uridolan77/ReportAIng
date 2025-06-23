-- ============================================================================
-- PROCESS FLOW TRACKING TABLES - DATABASE MIGRATION SCRIPT
-- ============================================================================
-- This script creates tables for comprehensive process flow tracking and transparency
-- Can be run multiple times safely (idempotent)
-- ============================================================================

USE [BIReportingCopilot_dev];
GO

SET NOCOUNT ON;
PRINT 'Starting Process Flow Tables migration...';
PRINT 'Database: ' + DB_NAME();
PRINT 'Timestamp: ' + CONVERT(VARCHAR, GETUTCDATE(), 120) + ' UTC';
PRINT '';

-- ============================================================================
-- 1. ProcessFlowSessions Table
-- ============================================================================
PRINT 'Creating ProcessFlowSessions table...';

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ProcessFlowSessions')
BEGIN
    CREATE TABLE [dbo].[ProcessFlowSessions] (
        [Id] NVARCHAR(450) NOT NULL PRIMARY KEY,
        [SessionId] NVARCHAR(450) NOT NULL UNIQUE,
        [UserId] NVARCHAR(450) NOT NULL,
        [Query] NVARCHAR(MAX) NOT NULL,
        [QueryType] NVARCHAR(100) NOT NULL DEFAULT 'enhanced',
        [Status] NVARCHAR(50) NOT NULL DEFAULT 'running',
        [StartTime] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [EndTime] DATETIME2 NULL,
        [TotalDurationMs] BIGINT NULL,
        [OverallConfidence] DECIMAL(5,4) NULL,
        [GeneratedSQL] NVARCHAR(MAX) NULL,
        [ExecutionResult] NVARCHAR(MAX) NULL,
        [ErrorMessage] NVARCHAR(MAX) NULL,
        [Metadata] NVARCHAR(MAX) NULL,
        [ConversationId] NVARCHAR(450) NULL,
        [MessageId] NVARCHAR(450) NULL,
        [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [UpdatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [CreatedBy] NVARCHAR(256) NULL,
        [UpdatedBy] NVARCHAR(256) NULL
    );

    -- Create indexes for ProcessFlowSessions
    CREATE INDEX [IX_ProcessFlowSessions_SessionId] ON [dbo].[ProcessFlowSessions] ([SessionId]);
    CREATE INDEX [IX_ProcessFlowSessions_UserId] ON [dbo].[ProcessFlowSessions] ([UserId]);
    CREATE INDEX [IX_ProcessFlowSessions_Status] ON [dbo].[ProcessFlowSessions] ([Status]);
    CREATE INDEX [IX_ProcessFlowSessions_StartTime] ON [dbo].[ProcessFlowSessions] ([StartTime]);
    CREATE INDEX [IX_ProcessFlowSessions_UserId_StartTime] ON [dbo].[ProcessFlowSessions] ([UserId], [StartTime]);
    CREATE INDEX [IX_ProcessFlowSessions_Status_StartTime] ON [dbo].[ProcessFlowSessions] ([Status], [StartTime]);
    CREATE INDEX [IX_ProcessFlowSessions_ConversationId] ON [dbo].[ProcessFlowSessions] ([ConversationId]);
    CREATE INDEX [IX_ProcessFlowSessions_MessageId] ON [dbo].[ProcessFlowSessions] ([MessageId]);

    PRINT '✅ ProcessFlowSessions table created successfully';
END
ELSE
BEGIN
    PRINT '⚠️  ProcessFlowSessions table already exists';
END
GO

-- ============================================================================
-- 2. ProcessFlowSteps Table
-- ============================================================================
PRINT 'Creating ProcessFlowSteps table...';

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ProcessFlowSteps')
BEGIN
    CREATE TABLE [dbo].[ProcessFlowSteps] (
        [Id] NVARCHAR(450) NOT NULL PRIMARY KEY,
        [SessionId] NVARCHAR(450) NOT NULL,
        [StepId] NVARCHAR(100) NOT NULL,
        [ParentStepId] NVARCHAR(100) NULL,
        [Name] NVARCHAR(200) NOT NULL,
        [Description] NVARCHAR(500) NULL,
        [StepOrder] INT NOT NULL DEFAULT 0,
        [Status] NVARCHAR(50) NOT NULL DEFAULT 'pending',
        [StartTime] DATETIME2 NULL,
        [EndTime] DATETIME2 NULL,
        [DurationMs] BIGINT NULL,
        [Confidence] DECIMAL(5,4) NULL,
        [InputData] NVARCHAR(MAX) NULL,
        [OutputData] NVARCHAR(MAX) NULL,
        [ErrorMessage] NVARCHAR(MAX) NULL,
        [Metadata] NVARCHAR(MAX) NULL,
        [RetryCount] INT NOT NULL DEFAULT 0,
        [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [UpdatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [CreatedBy] NVARCHAR(256) NULL,
        [UpdatedBy] NVARCHAR(256) NULL,

        CONSTRAINT [FK_ProcessFlowSteps_Sessions]
            FOREIGN KEY ([SessionId]) REFERENCES [dbo].[ProcessFlowSessions] ([SessionId])
            ON DELETE CASCADE,
        CONSTRAINT [UQ_ProcessFlowSteps_SessionStep]
            UNIQUE ([SessionId], [StepId])
    );

    -- Create indexes for ProcessFlowSteps
    CREATE INDEX [IX_ProcessFlowSteps_SessionId_StepOrder] ON [dbo].[ProcessFlowSteps] ([SessionId], [StepOrder]);
    CREATE INDEX [IX_ProcessFlowSteps_Status] ON [dbo].[ProcessFlowSteps] ([Status]);
    CREATE INDEX [IX_ProcessFlowSteps_SessionId_Status] ON [dbo].[ProcessFlowSteps] ([SessionId], [Status]);
    CREATE INDEX [IX_ProcessFlowSteps_ParentStepId] ON [dbo].[ProcessFlowSteps] ([ParentStepId]);
    CREATE INDEX [IX_ProcessFlowSteps_StartTime] ON [dbo].[ProcessFlowSteps] ([StartTime]);

    PRINT '✅ ProcessFlowSteps table created successfully';
END
ELSE
BEGIN
    PRINT '⚠️  ProcessFlowSteps table already exists';
END
GO

-- ============================================================================
-- 3. ProcessFlowLogs Table
-- ============================================================================
PRINT 'Creating ProcessFlowLogs table...';

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ProcessFlowLogs')
BEGIN
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

        CONSTRAINT [FK_ProcessFlowLogs_Sessions]
            FOREIGN KEY ([SessionId]) REFERENCES [dbo].[ProcessFlowSessions] ([SessionId])
            ON DELETE CASCADE
    );

    -- Create indexes for ProcessFlowLogs
    CREATE INDEX [IX_ProcessFlowLogs_SessionId] ON [dbo].[ProcessFlowLogs] ([SessionId]);
    CREATE INDEX [IX_ProcessFlowLogs_StepId] ON [dbo].[ProcessFlowLogs] ([StepId]);
    CREATE INDEX [IX_ProcessFlowLogs_Timestamp] ON [dbo].[ProcessFlowLogs] ([Timestamp]);
    CREATE INDEX [IX_ProcessFlowLogs_SessionId_Timestamp] ON [dbo].[ProcessFlowLogs] ([SessionId], [Timestamp]);
    CREATE INDEX [IX_ProcessFlowLogs_LogLevel_Timestamp] ON [dbo].[ProcessFlowLogs] ([LogLevel], [Timestamp]);
    CREATE INDEX [IX_ProcessFlowLogs_Source] ON [dbo].[ProcessFlowLogs] ([Source]);

    PRINT '✅ ProcessFlowLogs table created successfully';
END
ELSE
BEGIN
    PRINT '⚠️  ProcessFlowLogs table already exists';
END
GO

-- ============================================================================
-- 4. ProcessFlowTransparency Table
-- ============================================================================
PRINT 'Creating ProcessFlowTransparency table...';

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ProcessFlowTransparency')
BEGIN
    CREATE TABLE [dbo].[ProcessFlowTransparency] (
        [Id] NVARCHAR(450) NOT NULL PRIMARY KEY,
        [SessionId] NVARCHAR(450) NOT NULL UNIQUE,
        [Model] NVARCHAR(100) NULL,
        [Temperature] DECIMAL(3,2) NULL,
        [MaxTokens] INT NULL,
        [PromptTokens] INT NULL,
        [CompletionTokens] INT NULL,
        [TotalTokens] INT NULL,
        [EstimatedCost] DECIMAL(10,6) NULL,
        [Confidence] DECIMAL(5,4) NULL,
        [AIProcessingTimeMs] BIGINT NULL,
        [ApiCallCount] INT NOT NULL DEFAULT 0,
        [PromptDetails] NVARCHAR(MAX) NULL,
        [ResponseAnalysis] NVARCHAR(MAX) NULL,
        [QualityMetrics] NVARCHAR(MAX) NULL,
        [OptimizationSuggestions] NVARCHAR(MAX) NULL,
        [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [UpdatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [CreatedBy] NVARCHAR(256) NULL,
        [UpdatedBy] NVARCHAR(256) NULL,

        CONSTRAINT [FK_ProcessFlowTransparency_Sessions]
            FOREIGN KEY ([SessionId]) REFERENCES [dbo].[ProcessFlowSessions] ([SessionId])
            ON DELETE CASCADE
    );

    -- Create indexes for ProcessFlowTransparency
    CREATE INDEX [IX_ProcessFlowTransparency_SessionId] ON [dbo].[ProcessFlowTransparency] ([SessionId]);
    CREATE INDEX [IX_ProcessFlowTransparency_Model] ON [dbo].[ProcessFlowTransparency] ([Model]);
    CREATE INDEX [IX_ProcessFlowTransparency_TotalTokens] ON [dbo].[ProcessFlowTransparency] ([TotalTokens]);
    CREATE INDEX [IX_ProcessFlowTransparency_EstimatedCost] ON [dbo].[ProcessFlowTransparency] ([EstimatedCost]);

    PRINT '✅ ProcessFlowTransparency table created successfully';
END
ELSE
BEGIN
    PRINT '⚠️  ProcessFlowTransparency table already exists';
END
GO

-- ============================================================================
-- 5. Create Views for Easy Querying
-- ============================================================================
PRINT 'Creating ProcessFlow views...';

-- View for complete process flow sessions with summary
IF NOT EXISTS (SELECT * FROM sys.views WHERE name = 'vw_ProcessFlowSummary')
BEGIN
    EXEC('
    CREATE VIEW [dbo].[vw_ProcessFlowSummary] AS
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
        s.ConversationId,
        s.MessageId,
        t.Model,
        t.TotalTokens,
        t.EstimatedCost,
        t.ApiCallCount,
        (SELECT COUNT(*) FROM ProcessFlowSteps WHERE SessionId = s.SessionId) as TotalSteps,
        (SELECT COUNT(*) FROM ProcessFlowSteps WHERE SessionId = s.SessionId AND Status = ''completed'') as CompletedSteps,
        (SELECT COUNT(*) FROM ProcessFlowSteps WHERE SessionId = s.SessionId AND Status = ''error'') as ErrorSteps,
        (SELECT COUNT(*) FROM ProcessFlowLogs WHERE SessionId = s.SessionId) as TotalLogs,
        (SELECT COUNT(*) FROM ProcessFlowLogs WHERE SessionId = s.SessionId AND LogLevel = ''Error'') as ErrorLogs
    FROM ProcessFlowSessions s
    LEFT JOIN ProcessFlowTransparency t ON s.SessionId = t.SessionId
    ');
    PRINT '✅ vw_ProcessFlowSummary view created successfully';
END
ELSE
BEGIN
    PRINT '⚠️  vw_ProcessFlowSummary view already exists';
END
GO

-- View for step performance analysis
IF NOT EXISTS (SELECT * FROM sys.views WHERE name = 'vw_ProcessFlowStepPerformance')
BEGIN
    EXEC('
    CREATE VIEW [dbo].[vw_ProcessFlowStepPerformance] AS
    SELECT 
        StepId,
        Name,
        COUNT(*) as ExecutionCount,
        AVG(CAST(DurationMs AS FLOAT)) as AvgDurationMs,
        MIN(DurationMs) as MinDurationMs,
        MAX(DurationMs) as MaxDurationMs,
        AVG(CAST(Confidence AS FLOAT)) as AvgConfidence,
        SUM(CASE WHEN Status = ''completed'' THEN 1 ELSE 0 END) as SuccessCount,
        SUM(CASE WHEN Status = ''error'' THEN 1 ELSE 0 END) as ErrorCount,
        CAST(SUM(CASE WHEN Status = ''completed'' THEN 1 ELSE 0 END) AS FLOAT) / COUNT(*) * 100 as SuccessRate
    FROM ProcessFlowSteps
    WHERE StartTime IS NOT NULL AND EndTime IS NOT NULL
    GROUP BY StepId, Name
    ');
    PRINT '✅ vw_ProcessFlowStepPerformance view created successfully';
END
ELSE
BEGIN
    PRINT '⚠️  vw_ProcessFlowStepPerformance view already exists';
END
GO

-- ============================================================================
-- 6. Insert Sample Data for Testing
-- ============================================================================
PRINT 'Inserting sample process flow data...';

-- Sample process flow session
IF NOT EXISTS (SELECT * FROM ProcessFlowSessions WHERE SessionId = 'sample-session-001')
BEGIN
    INSERT INTO ProcessFlowSessions (Id, SessionId, UserId, Query, QueryType, Status, StartTime, EndTime, TotalDurationMs, OverallConfidence, GeneratedSQL, ConversationId, MessageId)
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
        'msg-001'
    );

    -- Sample process flow steps
    INSERT INTO ProcessFlowSteps (Id, SessionId, StepId, Name, Description, StepOrder, Status, StartTime, EndTime, DurationMs, Confidence)
    VALUES 
        ('pfs-step-001', 'sample-session-001', 'auth', 'Authentication & Authorization', 'Validating JWT token and user permissions', 1, 'completed', DATEADD(MINUTE, -10, GETUTCDATE()), DATEADD(SECOND, -599, GETUTCDATE()), 150, 1.0),
        ('pfs-step-002', 'sample-session-001', 'semantic-analysis', 'Semantic Analysis', 'Analyzing query intent and extracting semantic meaning', 3, 'completed', DATEADD(SECOND, -598, GETUTCDATE()), DATEADD(SECOND, -597, GETUTCDATE()), 1000, 0.95),
        ('pfs-step-003', 'sample-session-001', 'ai-generation', 'AI SQL Generation', 'Calling OpenAI API to generate SQL query', 6, 'completed', DATEADD(SECOND, -595, GETUTCDATE()), DATEADD(SECOND, -592, GETUTCDATE()), 2500, 0.96),
        ('pfs-step-004', 'sample-session-001', 'sql-execution', 'SQL Execution', 'Executing validated SQL query against database', 8, 'completed', DATEADD(SECOND, -591, GETUTCDATE()), DATEADD(SECOND, -590, GETUTCDATE()), 700, 0.98);

    -- Sample transparency data
    INSERT INTO ProcessFlowTransparency (Id, SessionId, Model, Temperature, MaxTokens, PromptTokens, CompletionTokens, TotalTokens, EstimatedCost, Confidence, AIProcessingTimeMs, ApiCallCount)
    VALUES (
        'pft-001',
        'sample-session-001',
        'gpt-4',
        0.1,
        1000,
        850,
        120,
        970,
        0.0194,
        0.95,
        2200,
        1
    );

    -- Sample logs
    INSERT INTO ProcessFlowLogs (Id, SessionId, StepId, LogLevel, Message, Source, Timestamp)
    VALUES 
        ('pfl-001', 'sample-session-001', 'auth', 'Info', 'JWT Token validated successfully for user: admin-user-001', 'AuthenticationService', DATEADD(MINUTE, -10, GETUTCDATE())),
        ('pfl-002', 'sample-session-001', 'semantic-analysis', 'Info', 'Intent classification completed with confidence: 0.95', 'SemanticAnalyzer', DATEADD(SECOND, -598, GETUTCDATE())),
        ('pfl-003', 'sample-session-001', 'ai-generation', 'Info', 'OpenAI API Request - Model: gpt-4, Temperature: 0.1', 'OpenAIProvider', DATEADD(SECOND, -595, GETUTCDATE())),
        ('pfl-004', 'sample-session-001', 'sql-execution', 'Info', 'Query execution completed - Success: True, Rows: 10', 'QueryExecutor', DATEADD(SECOND, -590, GETUTCDATE()));

    PRINT '✅ Sample process flow data inserted successfully';
END
ELSE
BEGIN
    PRINT '⚠️  Sample process flow data already exists';
END
GO

-- ============================================================================
-- 7. Summary and Completion
-- ============================================================================
PRINT '';
PRINT '============================================================================';
PRINT 'PROCESS FLOW TRACKING TABLES - MIGRATION COMPLETE';
PRINT '============================================================================';
PRINT 'Successfully created:';
PRINT '  ✅ 4 Tables with proper indexes and constraints';
PRINT '  ✅ 2 Views for easy querying and analysis';
PRINT '  ✅ Sample data for testing and development';
PRINT '  ✅ Foreign key relationships for data integrity';
PRINT '  ✅ Optimized indexes for query performance';
PRINT '';
PRINT 'Tables created:';
PRINT '  • ProcessFlowSessions - Complete process flow session tracking';
PRINT '  • ProcessFlowSteps - Individual step tracking with hierarchy';
PRINT '  • ProcessFlowLogs - Detailed logging for each step';
PRINT '  • ProcessFlowTransparency - AI transparency and token usage';
PRINT '';
PRINT 'Views created:';
PRINT '  • vw_ProcessFlowSummary - Complete session overview';
PRINT '  • vw_ProcessFlowStepPerformance - Step performance analysis';
PRINT '';
PRINT 'Ready for process flow tracking and transparency!';
PRINT '============================================================================';
GO
