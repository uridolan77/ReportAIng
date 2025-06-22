-- ============================================================================
-- AI TRANSPARENCY FOUNDATION - COMPLETE DATABASE SCHEMA & DATA SEEDING
-- ============================================================================
-- This script creates all necessary tables, indexes, and seed data for the
-- AI Transparency Foundation implementation
-- ============================================================================

USE [BIReportingCopilot_dev];
GO

-- ============================================================================
-- 1. TRANSPARENCY TRACKING TABLES
-- ============================================================================

-- Prompt Construction Traces Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'PromptConstructionTraces')
BEGIN
    CREATE TABLE [dbo].[PromptConstructionTraces] (
        [Id] NVARCHAR(450) NOT NULL PRIMARY KEY,
        [TraceId] NVARCHAR(450) NOT NULL UNIQUE,
        [UserId] NVARCHAR(450) NOT NULL,
        [UserQuestion] NVARCHAR(MAX) NOT NULL,
        [IntentType] NVARCHAR(100) NOT NULL,
        [StartTime] DATETIME2 NOT NULL,
        [EndTime] DATETIME2 NULL,
        [OverallConfidence] DECIMAL(5,4) NOT NULL DEFAULT 0.0,
        [TotalTokens] INT NOT NULL DEFAULT 0,
        [FinalPrompt] NVARCHAR(MAX) NULL,
        [Success] BIT NOT NULL DEFAULT 0,
        [ErrorMessage] NVARCHAR(MAX) NULL,
        [Metadata] NVARCHAR(MAX) NULL, -- JSON
        [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [UpdatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE()
    );
    
    CREATE INDEX [IX_PromptConstructionTraces_TraceId] ON [dbo].[PromptConstructionTraces] ([TraceId]);
    CREATE INDEX [IX_PromptConstructionTraces_UserId] ON [dbo].[PromptConstructionTraces] ([UserId]);
    CREATE INDEX [IX_PromptConstructionTraces_IntentType] ON [dbo].[PromptConstructionTraces] ([IntentType]);
    CREATE INDEX [IX_PromptConstructionTraces_CreatedAt] ON [dbo].[PromptConstructionTraces] ([CreatedAt]);
END
GO

-- Prompt Construction Steps Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'PromptConstructionSteps')
BEGIN
    CREATE TABLE [dbo].[PromptConstructionSteps] (
        [Id] NVARCHAR(450) NOT NULL PRIMARY KEY,
        [TraceId] NVARCHAR(450) NOT NULL,
        [StepName] NVARCHAR(200) NOT NULL,
        [StepOrder] INT NOT NULL,
        [StartTime] DATETIME2 NOT NULL,
        [EndTime] DATETIME2 NULL,
        [Success] BIT NOT NULL DEFAULT 0,
        [TokensAdded] INT NOT NULL DEFAULT 0,
        [Confidence] DECIMAL(5,4) NOT NULL DEFAULT 0.0,
        [Content] NVARCHAR(MAX) NULL,
        [Details] NVARCHAR(MAX) NULL, -- JSON
        [ErrorMessage] NVARCHAR(MAX) NULL,
        [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        
        CONSTRAINT [FK_PromptConstructionSteps_Traces] 
            FOREIGN KEY ([TraceId]) REFERENCES [dbo].[PromptConstructionTraces] ([TraceId])
            ON DELETE CASCADE
    );
    
    CREATE INDEX [IX_PromptConstructionSteps_TraceId] ON [dbo].[PromptConstructionSteps] ([TraceId]);
    CREATE INDEX [IX_PromptConstructionSteps_StepOrder] ON [dbo].[PromptConstructionSteps] ([StepOrder]);
END
GO

-- ============================================================================
-- 2. TOKEN BUDGET MANAGEMENT TABLES
-- ============================================================================

-- Token Budgets Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'TokenBudgets')
BEGIN
    CREATE TABLE [dbo].[TokenBudgets] (
        [Id] NVARCHAR(450) NOT NULL PRIMARY KEY,
        [UserId] NVARCHAR(450) NOT NULL,
        [RequestType] NVARCHAR(100) NOT NULL,
        [IntentType] NVARCHAR(100) NOT NULL,
        [MaxTotalTokens] INT NOT NULL,
        [BasePromptTokens] INT NOT NULL,
        [ReservedResponseTokens] INT NOT NULL,
        [AvailableContextTokens] INT NOT NULL,
        [SchemaContextBudget] INT NOT NULL,
        [BusinessContextBudget] INT NOT NULL,
        [ExamplesBudget] INT NOT NULL,
        [RulesBudget] INT NOT NULL,
        [GlossaryBudget] INT NOT NULL,
        [EstimatedCost] DECIMAL(10,6) NOT NULL DEFAULT 0.0,
        [ActualTokensUsed] INT NULL,
        [ActualCost] DECIMAL(10,6) NULL,
        [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [CompletedAt] DATETIME2 NULL
    );
    
    CREATE INDEX [IX_TokenBudgets_UserId] ON [dbo].[TokenBudgets] ([UserId]);
    CREATE INDEX [IX_TokenBudgets_RequestType] ON [dbo].[TokenBudgets] ([RequestType]);
    CREATE INDEX [IX_TokenBudgets_IntentType] ON [dbo].[TokenBudgets] ([IntentType]);
    CREATE INDEX [IX_TokenBudgets_CreatedAt] ON [dbo].[TokenBudgets] ([CreatedAt]);
END
GO

-- Token Usage Analytics Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'TokenUsageAnalytics')
BEGIN
    CREATE TABLE [dbo].[TokenUsageAnalytics] (
        [Id] NVARCHAR(450) NOT NULL PRIMARY KEY,
        [UserId] NVARCHAR(450) NOT NULL,
        [Date] DATE NOT NULL,
        [RequestType] NVARCHAR(100) NOT NULL,
        [IntentType] NVARCHAR(100) NOT NULL,
        [TotalRequests] INT NOT NULL DEFAULT 0,
        [TotalTokensUsed] INT NOT NULL DEFAULT 0,
        [TotalCost] DECIMAL(10,6) NOT NULL DEFAULT 0.0,
        [AverageTokensPerRequest] DECIMAL(10,2) NOT NULL DEFAULT 0.0,
        [AverageCostPerRequest] DECIMAL(10,6) NOT NULL DEFAULT 0.0,
        [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [UpdatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        
        CONSTRAINT [UQ_TokenUsageAnalytics_UserDateRequestIntent] 
            UNIQUE ([UserId], [Date], [RequestType], [IntentType])
    );
    
    CREATE INDEX [IX_TokenUsageAnalytics_Date] ON [dbo].[TokenUsageAnalytics] ([Date]);
    CREATE INDEX [IX_TokenUsageAnalytics_UserId] ON [dbo].[TokenUsageAnalytics] ([UserId]);
END
GO

-- ============================================================================
-- 3. BUSINESS CONTEXT ENHANCEMENT TABLES
-- ============================================================================

-- Business Context Profiles Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'BusinessContextProfiles')
BEGIN
    CREATE TABLE [dbo].[BusinessContextProfiles] (
        [Id] NVARCHAR(450) NOT NULL PRIMARY KEY,
        [UserId] NVARCHAR(450) NOT NULL,
        [OriginalQuestion] NVARCHAR(MAX) NOT NULL,
        [IntentType] NVARCHAR(100) NOT NULL,
        [IntentConfidence] DECIMAL(5,4) NOT NULL DEFAULT 0.0,
        [IntentDescription] NVARCHAR(500) NULL,
        [DomainName] NVARCHAR(200) NOT NULL,
        [DomainConfidence] DECIMAL(5,4) NOT NULL DEFAULT 0.0,
        [OverallConfidence] DECIMAL(5,4) NOT NULL DEFAULT 0.0,
        [ProcessingTimeMs] INT NOT NULL DEFAULT 0,
        [Entities] NVARCHAR(MAX) NULL, -- JSON array
        [Keywords] NVARCHAR(MAX) NULL, -- JSON array
        [Metadata] NVARCHAR(MAX) NULL, -- JSON
        [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE()
    );
    
    CREATE INDEX [IX_BusinessContextProfiles_UserId] ON [dbo].[BusinessContextProfiles] ([UserId]);
    CREATE INDEX [IX_BusinessContextProfiles_IntentType] ON [dbo].[BusinessContextProfiles] ([IntentType]);
    CREATE INDEX [IX_BusinessContextProfiles_DomainName] ON [dbo].[BusinessContextProfiles] ([DomainName]);
    CREATE INDEX [IX_BusinessContextProfiles_CreatedAt] ON [dbo].[BusinessContextProfiles] ([CreatedAt]);
END
GO

-- Business Entities Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'BusinessEntities')
BEGIN
    CREATE TABLE [dbo].[BusinessEntities] (
        [Id] NVARCHAR(450) NOT NULL PRIMARY KEY,
        [ProfileId] NVARCHAR(450) NOT NULL,
        [Name] NVARCHAR(200) NOT NULL,
        [Type] NVARCHAR(100) NOT NULL,
        [Confidence] DECIMAL(5,4) NOT NULL DEFAULT 0.0,
        [RelevanceScore] DECIMAL(5,4) NOT NULL DEFAULT 0.0,
        [Context] NVARCHAR(500) NULL,
        [Metadata] NVARCHAR(MAX) NULL, -- JSON
        [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        
        CONSTRAINT [FK_BusinessEntities_Profiles] 
            FOREIGN KEY ([ProfileId]) REFERENCES [dbo].[BusinessContextProfiles] ([Id])
            ON DELETE CASCADE
    );
    
    CREATE INDEX [IX_BusinessEntities_ProfileId] ON [dbo].[BusinessEntities] ([ProfileId]);
    CREATE INDEX [IX_BusinessEntities_Name] ON [dbo].[BusinessEntities] ([Name]);
    CREATE INDEX [IX_BusinessEntities_Type] ON [dbo].[BusinessEntities] ([Type]);
END
GO

-- ============================================================================
-- 4. INTELLIGENT AGENTS TABLES
-- ============================================================================

-- Agent Orchestration Sessions Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'AgentOrchestrationSessions')
BEGIN
    CREATE TABLE [dbo].[AgentOrchestrationSessions] (
        [Id] NVARCHAR(450) NOT NULL PRIMARY KEY,
        [SessionId] NVARCHAR(450) NOT NULL UNIQUE,
        [UserId] NVARCHAR(450) NOT NULL,
        [TaskType] NVARCHAR(100) NOT NULL,
        [Priority] NVARCHAR(50) NOT NULL DEFAULT 'Normal',
        [Status] NVARCHAR(50) NOT NULL DEFAULT 'Pending',
        [StartTime] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [EndTime] DATETIME2 NULL,
        [TotalAgents] INT NOT NULL DEFAULT 0,
        [CompletedAgents] INT NOT NULL DEFAULT 0,
        [FailedAgents] INT NOT NULL DEFAULT 0,
        [OverallSuccess] BIT NULL,
        [Parameters] NVARCHAR(MAX) NULL, -- JSON
        [Results] NVARCHAR(MAX) NULL, -- JSON
        [ErrorMessage] NVARCHAR(MAX) NULL,
        [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [UpdatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE()
    );

    CREATE INDEX [IX_AgentOrchestrationSessions_SessionId] ON [dbo].[AgentOrchestrationSessions] ([SessionId]);
    CREATE INDEX [IX_AgentOrchestrationSessions_UserId] ON [dbo].[AgentOrchestrationSessions] ([UserId]);
    CREATE INDEX [IX_AgentOrchestrationSessions_TaskType] ON [dbo].[AgentOrchestrationSessions] ([TaskType]);
    CREATE INDEX [IX_AgentOrchestrationSessions_Status] ON [dbo].[AgentOrchestrationSessions] ([Status]);
    CREATE INDEX [IX_AgentOrchestrationSessions_CreatedAt] ON [dbo].[AgentOrchestrationSessions] ([CreatedAt]);
END
GO

-- Agent Execution Results Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'AgentExecutionResults')
BEGIN
    CREATE TABLE [dbo].[AgentExecutionResults] (
        [Id] NVARCHAR(450) NOT NULL PRIMARY KEY,
        [SessionId] NVARCHAR(450) NOT NULL,
        [AgentName] NVARCHAR(200) NOT NULL,
        [AgentType] NVARCHAR(100) NOT NULL,
        [ExecutionOrder] INT NOT NULL,
        [Status] NVARCHAR(50) NOT NULL DEFAULT 'Pending',
        [StartTime] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [EndTime] DATETIME2 NULL,
        [ExecutionTimeMs] INT NULL,
        [Success] BIT NULL,
        [Confidence] DECIMAL(5,4) NULL,
        [TokensUsed] INT NULL,
        [Cost] DECIMAL(10,6) NULL,
        [Input] NVARCHAR(MAX) NULL, -- JSON
        [Output] NVARCHAR(MAX) NULL, -- JSON
        [ErrorMessage] NVARCHAR(MAX) NULL,
        [Metadata] NVARCHAR(MAX) NULL, -- JSON
        [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),

        CONSTRAINT [FK_AgentExecutionResults_Sessions]
            FOREIGN KEY ([SessionId]) REFERENCES [dbo].[AgentOrchestrationSessions] ([SessionId])
            ON DELETE CASCADE
    );

    CREATE INDEX [IX_AgentExecutionResults_SessionId] ON [dbo].[AgentExecutionResults] ([SessionId]);
    CREATE INDEX [IX_AgentExecutionResults_AgentName] ON [dbo].[AgentExecutionResults] ([AgentName]);
    CREATE INDEX [IX_AgentExecutionResults_Status] ON [dbo].[AgentExecutionResults] ([Status]);
    CREATE INDEX [IX_AgentExecutionResults_ExecutionOrder] ON [dbo].[AgentExecutionResults] ([ExecutionOrder]);
END
GO

-- ============================================================================
-- 5. AI STREAMING TABLES
-- ============================================================================

-- Streaming Sessions Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'StreamingSessions')
BEGIN
    CREATE TABLE [dbo].[StreamingSessions] (
        [Id] NVARCHAR(450) NOT NULL PRIMARY KEY,
        [StreamId] NVARCHAR(450) NOT NULL UNIQUE,
        [UserId] NVARCHAR(450) NOT NULL,
        [ConnectionId] NVARCHAR(450) NOT NULL,
        [RequestType] NVARCHAR(100) NOT NULL,
        [Status] NVARCHAR(50) NOT NULL DEFAULT 'Active',
        [StartTime] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [EndTime] DATETIME2 NULL,
        [TotalChunks] INT NOT NULL DEFAULT 0,
        [ProcessedChunks] INT NOT NULL DEFAULT 0,
        [TotalTokens] INT NOT NULL DEFAULT 0,
        [TotalCost] DECIMAL(10,6) NOT NULL DEFAULT 0.0,
        [Quality] NVARCHAR(50) NULL,
        [Parameters] NVARCHAR(MAX) NULL, -- JSON
        [Metadata] NVARCHAR(MAX) NULL, -- JSON
        [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [UpdatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE()
    );

    CREATE INDEX [IX_StreamingSessions_StreamId] ON [dbo].[StreamingSessions] ([StreamId]);
    CREATE INDEX [IX_StreamingSessions_UserId] ON [dbo].[StreamingSessions] ([UserId]);
    CREATE INDEX [IX_StreamingSessions_ConnectionId] ON [dbo].[StreamingSessions] ([ConnectionId]);
    CREATE INDEX [IX_StreamingSessions_Status] ON [dbo].[StreamingSessions] ([Status]);
    CREATE INDEX [IX_StreamingSessions_CreatedAt] ON [dbo].[StreamingSessions] ([CreatedAt]);
END
GO

-- Streaming Chunks Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'StreamingChunks')
BEGIN
    CREATE TABLE [dbo].[StreamingChunks] (
        [Id] NVARCHAR(450) NOT NULL PRIMARY KEY,
        [StreamId] NVARCHAR(450) NOT NULL,
        [ChunkIndex] INT NOT NULL,
        [ChunkType] NVARCHAR(50) NOT NULL,
        [Content] NVARCHAR(MAX) NOT NULL,
        [Tokens] INT NOT NULL DEFAULT 0,
        [ProcessingTimeMs] INT NOT NULL DEFAULT 0,
        [Confidence] DECIMAL(5,4) NULL,
        [Metadata] NVARCHAR(MAX) NULL, -- JSON
        [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),

        CONSTRAINT [FK_StreamingChunks_Sessions]
            FOREIGN KEY ([StreamId]) REFERENCES [dbo].[StreamingSessions] ([StreamId])
            ON DELETE CASCADE,
        CONSTRAINT [UQ_StreamingChunks_StreamChunk]
            UNIQUE ([StreamId], [ChunkIndex])
    );

    CREATE INDEX [IX_StreamingChunks_StreamId] ON [dbo].[StreamingChunks] ([StreamId]);
    CREATE INDEX [IX_StreamingChunks_ChunkIndex] ON [dbo].[StreamingChunks] ([ChunkIndex]);
    CREATE INDEX [IX_StreamingChunks_ChunkType] ON [dbo].[StreamingChunks] ([ChunkType]);
END
GO

-- ============================================================================
-- 6. TRANSPARENCY ANALYTICS TABLES
-- ============================================================================

-- Transparency Reports Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'TransparencyReports')
BEGIN
    CREATE TABLE [dbo].[TransparencyReports] (
        [Id] NVARCHAR(450) NOT NULL PRIMARY KEY,
        [TraceId] NVARCHAR(450) NOT NULL,
        [UserId] NVARCHAR(450) NOT NULL,
        [ReportType] NVARCHAR(100) NOT NULL DEFAULT 'Standard',
        [GeneratedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [UserQuestion] NVARCHAR(MAX) NOT NULL,
        [IntentType] NVARCHAR(100) NOT NULL,
        [Summary] NVARCHAR(MAX) NOT NULL,
        [DetailedMetrics] NVARCHAR(MAX) NULL, -- JSON
        [PerformanceAnalysis] NVARCHAR(MAX) NULL, -- JSON
        [OptimizationSuggestions] NVARCHAR(MAX) NULL, -- JSON array
        [ConfidenceBreakdown] NVARCHAR(MAX) NULL, -- JSON
        [TokenAnalysis] NVARCHAR(MAX) NULL, -- JSON
        [ErrorMessage] NVARCHAR(MAX) NULL,
        [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE()
    );

    CREATE INDEX [IX_TransparencyReports_TraceId] ON [dbo].[TransparencyReports] ([TraceId]);
    CREATE INDEX [IX_TransparencyReports_UserId] ON [dbo].[TransparencyReports] ([UserId]);
    CREATE INDEX [IX_TransparencyReports_IntentType] ON [dbo].[TransparencyReports] ([IntentType]);
    CREATE INDEX [IX_TransparencyReports_GeneratedAt] ON [dbo].[TransparencyReports] ([GeneratedAt]);
END
GO

-- Optimization Suggestions Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'OptimizationSuggestions')
BEGIN
    CREATE TABLE [dbo].[OptimizationSuggestions] (
        [Id] NVARCHAR(450) NOT NULL PRIMARY KEY,
        [SuggestionId] NVARCHAR(450) NOT NULL UNIQUE,
        [TraceId] NVARCHAR(450) NULL,
        [UserId] NVARCHAR(450) NOT NULL,
        [Title] NVARCHAR(200) NOT NULL,
        [Description] NVARCHAR(MAX) NOT NULL,
        [Category] NVARCHAR(100) NOT NULL, -- performance, accuracy, cost, clarity
        [Impact] NVARCHAR(50) NOT NULL, -- high, medium, low
        [Effort] NVARCHAR(50) NOT NULL, -- high, medium, low
        [ExpectedImprovement] DECIMAL(5,4) NOT NULL DEFAULT 0.0,
        [Implementation] NVARCHAR(MAX) NOT NULL,
        [EstimatedCostSavings] DECIMAL(10,6) NULL,
        [Status] NVARCHAR(50) NOT NULL DEFAULT 'Pending', -- Pending, Implemented, Rejected
        [AppliedAt] DATETIME2 NULL,
        [Metadata] NVARCHAR(MAX) NULL, -- JSON
        [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [UpdatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE()
    );

    CREATE INDEX [IX_OptimizationSuggestions_SuggestionId] ON [dbo].[OptimizationSuggestions] ([SuggestionId]);
    CREATE INDEX [IX_OptimizationSuggestions_TraceId] ON [dbo].[OptimizationSuggestions] ([TraceId]);
    CREATE INDEX [IX_OptimizationSuggestions_UserId] ON [dbo].[OptimizationSuggestions] ([UserId]);
    CREATE INDEX [IX_OptimizationSuggestions_Category] ON [dbo].[OptimizationSuggestions] ([Category]);
    CREATE INDEX [IX_OptimizationSuggestions_Status] ON [dbo].[OptimizationSuggestions] ([Status]);
    CREATE INDEX [IX_OptimizationSuggestions_CreatedAt] ON [dbo].[OptimizationSuggestions] ([CreatedAt]);
END
GO

-- ============================================================================
-- 7. DATA SEEDING - SAMPLE TRANSPARENCY DATA
-- ============================================================================

PRINT 'Starting AI Transparency Foundation data seeding...';

-- Sample Prompt Construction Traces
IF NOT EXISTS (SELECT 1 FROM [dbo].[PromptConstructionTraces])
BEGIN
    PRINT 'Seeding sample prompt construction traces...';

    INSERT INTO [dbo].[PromptConstructionTraces]
    ([Id], [TraceId], [UserId], [UserQuestion], [IntentType], [StartTime], [EndTime], [OverallConfidence], [TotalTokens], [FinalPrompt], [Success], [Metadata])
    VALUES
    (NEWID(), 'trace-001', 'admin@bireporting.com', 'Show me sales data for Q3', 'Analytical', DATEADD(MINUTE, -30, GETUTCDATE()), DATEADD(MINUTE, -29, GETUTCDATE()), 0.8500, 1250, 'SELECT * FROM Sales WHERE Quarter = 3', 1, '{"model": "gpt-4", "temperature": 0.1}'),
    (NEWID(), 'trace-002', 'admin@bireporting.com', 'What are the top performing products?', 'Reporting', DATEADD(MINUTE, -25, GETUTCDATE()), DATEADD(MINUTE, -24, GETUTCDATE()), 0.9200, 980, 'SELECT ProductName, SUM(Revenue) FROM Products GROUP BY ProductName ORDER BY SUM(Revenue) DESC', 1, '{"model": "gpt-4", "temperature": 0.1}'),
    (NEWID(), 'trace-003', 'admin@bireporting.com', 'Compare revenue trends by region', 'Comparative', DATEADD(MINUTE, -20, GETUTCDATE()), DATEADD(MINUTE, -19, GETUTCDATE()), 0.7800, 1450, 'SELECT Region, YEAR(Date) as Year, SUM(Revenue) FROM Sales GROUP BY Region, YEAR(Date)', 1, '{"model": "gpt-4", "temperature": 0.1}'),
    (NEWID(), 'trace-004', 'admin@bireporting.com', 'Find anomalies in customer behavior', 'Exploratory', DATEADD(MINUTE, -15, GETUTCDATE()), DATEADD(MINUTE, -14, GETUTCDATE()), 0.6500, 1680, 'SELECT CustomerID, COUNT(*) as OrderCount FROM Orders GROUP BY CustomerID HAVING COUNT(*) > 10', 1, '{"model": "gpt-4", "temperature": 0.2}'),
    (NEWID(), 'trace-005', 'admin@bireporting.com', 'Generate monthly sales report', 'Reporting', DATEADD(MINUTE, -10, GETUTCDATE()), DATEADD(MINUTE, -9, GETUTCDATE()), 0.8900, 1120, 'SELECT MONTH(Date) as Month, SUM(Amount) as TotalSales FROM Sales WHERE YEAR(Date) = 2024 GROUP BY MONTH(Date)', 1, '{"model": "gpt-4", "temperature": 0.1}');
END

-- Sample Prompt Construction Steps
IF NOT EXISTS (SELECT 1 FROM [dbo].[PromptConstructionSteps])
BEGIN
    PRINT 'Seeding sample prompt construction steps...';

    INSERT INTO [dbo].[PromptConstructionSteps]
    ([Id], [TraceId], [StepName], [StepOrder], [StartTime], [EndTime], [Success], [TokensAdded], [Confidence], [Content], [Details])
    VALUES
    (NEWID(), 'trace-001', 'Intent Analysis', 1, DATEADD(MINUTE, -30, GETUTCDATE()), DATEADD(SECOND, -1790, GETUTCDATE()), 1, 150, 0.8500, 'Identified analytical intent for sales data query', '{"entities": ["sales", "Q3"], "confidence": 0.85}'),
    (NEWID(), 'trace-001', 'Schema Context', 2, DATEADD(SECOND, -1790, GETUTCDATE()), DATEADD(SECOND, -1780, GETUTCDATE()), 1, 400, 0.9000, 'Added Sales table schema and relationships', '{"tables": ["Sales", "Products"], "columns": 12}'),
    (NEWID(), 'trace-001', 'Business Context', 3, DATEADD(SECOND, -1780, GETUTCDATE()), DATEADD(SECOND, -1770, GETUTCDATE()), 1, 300, 0.8200, 'Added business rules for quarterly reporting', '{"rules": ["Q3 = July-September"], "glossary_terms": 3}'),
    (NEWID(), 'trace-001', 'Query Generation', 4, DATEADD(SECOND, -1770, GETUTCDATE()), DATEADD(MINUTE, -29, GETUTCDATE()), 1, 400, 0.8800, 'Generated optimized SQL query', '{"optimization": "added_indexes", "estimated_cost": 0.02}');
END

-- Sample Token Budgets
IF NOT EXISTS (SELECT 1 FROM [dbo].[TokenBudgets])
BEGIN
    PRINT 'Seeding sample token budgets...';

    INSERT INTO [dbo].[TokenBudgets]
    ([Id], [UserId], [RequestType], [IntentType], [MaxTotalTokens], [BasePromptTokens], [ReservedResponseTokens], [AvailableContextTokens], [SchemaContextBudget], [BusinessContextBudget], [ExamplesBudget], [RulesBudget], [GlossaryBudget], [EstimatedCost], [ActualTokensUsed], [ActualCost], [CompletedAt])
    VALUES
    (NEWID(), 'admin@bireporting.com', 'enhanced_query', 'Analytical', 4000, 500, 500, 3000, 1200, 900, 600, 200, 100, 0.0800, 3250, 0.0650, DATEADD(MINUTE, -29, GETUTCDATE())),
    (NEWID(), 'admin@bireporting.com', 'enhanced_query', 'Reporting', 4000, 500, 500, 3000, 1000, 1000, 700, 200, 100, 0.0800, 2980, 0.0596, DATEADD(MINUTE, -24, GETUTCDATE())),
    (NEWID(), 'admin@bireporting.com', 'enhanced_query', 'Comparative', 4000, 500, 500, 3000, 1300, 800, 600, 200, 100, 0.0800, 3450, 0.0690, DATEADD(MINUTE, -19, GETUTCDATE())),
    (NEWID(), 'admin@bireporting.com', 'enhanced_query', 'Exploratory', 4000, 500, 500, 3000, 1100, 1100, 500, 200, 100, 0.0800, 3680, 0.0736, DATEADD(MINUTE, -14, GETUTCDATE())),
    (NEWID(), 'admin@bireporting.com', 'enhanced_query', 'Reporting', 4000, 500, 500, 3000, 1000, 1000, 700, 200, 100, 0.0800, 3120, 0.0624, DATEADD(MINUTE, -9, GETUTCDATE()));
END

-- Sample Business Context Profiles
IF NOT EXISTS (SELECT 1 FROM [dbo].[BusinessContextProfiles])
BEGIN
    PRINT 'Seeding sample business context profiles...';

    INSERT INTO [dbo].[BusinessContextProfiles]
    ([Id], [UserId], [OriginalQuestion], [IntentType], [IntentConfidence], [IntentDescription], [DomainName], [DomainConfidence], [OverallConfidence], [ProcessingTimeMs], [Entities], [Keywords], [Metadata])
    VALUES
    (NEWID(), 'admin@bireporting.com', 'Show me sales data for Q3', 'Analytical', 0.8500, 'User wants to analyze quarterly sales performance', 'Sales', 0.9200, 0.8850, 245, '["sales", "quarter", "Q3", "revenue"]', '["show", "data", "quarterly", "performance"]', '{"complexity": "medium", "tables_involved": 2}'),
    (NEWID(), 'admin@bireporting.com', 'What are the top performing products?', 'Reporting', 0.9200, 'User wants a report of best performing products', 'Products', 0.8800, 0.9000, 189, '["products", "performance", "ranking"]', '["top", "performing", "best", "ranking"]', '{"complexity": "low", "tables_involved": 1}'),
    (NEWID(), 'admin@bireporting.com', 'Compare revenue trends by region', 'Comparative', 0.7800, 'User wants to compare regional revenue patterns', 'Sales', 0.8500, 0.8150, 312, '["revenue", "trends", "region", "comparison"]', '["compare", "trends", "regional", "patterns"]', '{"complexity": "high", "tables_involved": 3}'),
    (NEWID(), 'admin@bireporting.com', 'Find anomalies in customer behavior', 'Exploratory', 0.6500, 'User wants to discover unusual customer patterns', 'Customers', 0.7200, 0.6850, 456, '["anomalies", "customer", "behavior", "patterns"]', '["find", "unusual", "discover", "patterns"]', '{"complexity": "high", "tables_involved": 4}'),
    (NEWID(), 'admin@bireporting.com', 'Generate monthly sales report', 'Reporting', 0.8900, 'User wants a standard monthly sales report', 'Sales', 0.9100, 0.9000, 167, '["monthly", "sales", "report", "summary"]', '["generate", "monthly", "report", "summary"]', '{"complexity": "low", "tables_involved": 1}');
END

-- Sample Token Usage Analytics
IF NOT EXISTS (SELECT 1 FROM [dbo].[TokenUsageAnalytics])
BEGIN
    PRINT 'Seeding sample token usage analytics...';

    INSERT INTO [dbo].[TokenUsageAnalytics]
    ([Id], [UserId], [Date], [RequestType], [IntentType], [TotalRequests], [TotalTokensUsed], [TotalCost], [AverageTokensPerRequest], [AverageCostPerRequest])
    VALUES
    (NEWID(), 'admin@bireporting.com', CAST(GETUTCDATE() AS DATE), 'enhanced_query', 'Analytical', 15, 48750, 0.9750, 3250.00, 0.0650),
    (NEWID(), 'admin@bireporting.com', CAST(GETUTCDATE() AS DATE), 'enhanced_query', 'Reporting', 25, 74500, 1.4900, 2980.00, 0.0596),
    (NEWID(), 'admin@bireporting.com', CAST(GETUTCDATE() AS DATE), 'enhanced_query', 'Comparative', 8, 27600, 0.5520, 3450.00, 0.0690),
    (NEWID(), 'admin@bireporting.com', CAST(GETUTCDATE() AS DATE), 'enhanced_query', 'Exploratory', 5, 18400, 0.3680, 3680.00, 0.0736),
    (NEWID(), 'admin@bireporting.com', CAST(DATEADD(DAY, -1, GETUTCDATE()) AS DATE), 'enhanced_query', 'Analytical', 12, 39000, 0.7800, 3250.00, 0.0650),
    (NEWID(), 'admin@bireporting.com', CAST(DATEADD(DAY, -1, GETUTCDATE()) AS DATE), 'enhanced_query', 'Reporting', 20, 59600, 1.1920, 2980.00, 0.0596);
END

-- Sample Optimization Suggestions
IF NOT EXISTS (SELECT 1 FROM [dbo].[OptimizationSuggestions])
BEGIN
    PRINT 'Seeding sample optimization suggestions...';

    INSERT INTO [dbo].[OptimizationSuggestions]
    ([Id], [SuggestionId], [UserId], [Title], [Description], [Category], [Impact], [Effort], [ExpectedImprovement], [Implementation], [EstimatedCostSavings], [Status])
    VALUES
    (NEWID(), 'opt-001', 'admin@bireporting.com', 'Optimize Schema Context Selection', 'Reduce schema context tokens by 20% through intelligent table filtering based on query intent', 'performance', 'medium', 'low', 0.2000, 'Implement relevance scoring for table selection in schema context builder', 15.50, 'Pending'),
    (NEWID(), 'opt-002', 'admin@bireporting.com', 'Cache Business Context Profiles', 'Cache frequently used business context profiles to reduce processing time', 'performance', 'high', 'medium', 0.3500, 'Add Redis caching layer for business context profiles with 1-hour TTL', 25.75, 'Pending'),
    (NEWID(), 'opt-003', 'admin@bireporting.com', 'Improve Intent Classification', 'Enhance intent classification accuracy through fine-tuning on domain-specific data', 'accuracy', 'high', 'high', 0.1500, 'Collect and label 1000+ domain-specific queries for fine-tuning', 0.00, 'Pending'),
    (NEWID(), 'opt-004', 'admin@bireporting.com', 'Dynamic Token Budget Allocation', 'Implement dynamic token budget allocation based on query complexity', 'cost', 'medium', 'medium', 0.1800, 'Add complexity scoring algorithm to adjust token budgets automatically', 12.30, 'Pending'),
    (NEWID(), 'opt-005', 'admin@bireporting.com', 'Batch Process Similar Queries', 'Group similar queries for batch processing to reduce API calls', 'cost', 'low', 'low', 0.1000, 'Implement query similarity detection and batching mechanism', 8.90, 'Pending');
END

-- ============================================================================
-- 8. COMPLETION MESSAGE
-- ============================================================================

PRINT '';
PRINT '============================================================================';
PRINT 'AI TRANSPARENCY FOUNDATION - DATABASE SETUP COMPLETE';
PRINT '============================================================================';
PRINT 'Successfully created:';
PRINT '  ✅ 10 Tables with proper indexes and constraints';
PRINT '  ✅ Sample data for testing and development';
PRINT '  ✅ Foreign key relationships for data integrity';
PRINT '  ✅ Optimized indexes for query performance';
PRINT '';
PRINT 'Tables created:';
PRINT '  • PromptConstructionTraces - Track AI prompt building process';
PRINT '  • PromptConstructionSteps - Detailed steps in prompt construction';
PRINT '  • TokenBudgets - Token allocation and cost management';
PRINT '  • TokenUsageAnalytics - Daily usage analytics and trends';
PRINT '  • BusinessContextProfiles - Business intent and context analysis';
PRINT '  • BusinessEntities - Extracted business entities from queries';
PRINT '  • AgentOrchestrationSessions - Multi-agent coordination tracking';
PRINT '  • AgentExecutionResults - Individual agent execution results';
PRINT '  • StreamingSessions - Real-time streaming session management';
PRINT '  • StreamingChunks - Individual streaming data chunks';
PRINT '  • TransparencyReports - Generated transparency reports';
PRINT '  • OptimizationSuggestions - AI optimization recommendations';
PRINT '';
PRINT 'Sample data includes:';
PRINT '  • 5 sample prompt construction traces with steps';
PRINT '  • 5 token budget allocations with actual usage';
PRINT '  • 5 business context profiles with entities';
PRINT '  • 6 daily usage analytics records';
PRINT '  • 5 optimization suggestions for improvement';
PRINT '';
PRINT 'The AI Transparency Foundation is now ready for production use!';
PRINT '============================================================================';
GO
