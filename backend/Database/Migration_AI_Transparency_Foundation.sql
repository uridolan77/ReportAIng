-- ============================================================================
-- AI TRANSPARENCY FOUNDATION - PRODUCTION MIGRATION SCRIPT
-- ============================================================================
-- This script safely migrates the database to support AI Transparency Foundation
-- Can be run multiple times safely (idempotent)
-- ============================================================================

USE [BIReportingCopilot_dev];
GO

SET NOCOUNT ON;
PRINT 'Starting AI Transparency Foundation migration...';
PRINT 'Database: ' + DB_NAME();
PRINT 'Timestamp: ' + CONVERT(VARCHAR, GETUTCDATE(), 120) + ' UTC';
PRINT '';

-- ============================================================================
-- MIGRATION STEP 1: CREATE TRANSPARENCY TRACKING TABLES
-- ============================================================================

PRINT 'Step 1: Creating transparency tracking tables...';

-- Prompt Construction Traces Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'PromptConstructionTraces')
BEGIN
    PRINT '  Creating PromptConstructionTraces table...';
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
        [Metadata] NVARCHAR(MAX) NULL,
        [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [UpdatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE()
    );
    
    CREATE INDEX [IX_PromptConstructionTraces_TraceId] ON [dbo].[PromptConstructionTraces] ([TraceId]);
    CREATE INDEX [IX_PromptConstructionTraces_UserId] ON [dbo].[PromptConstructionTraces] ([UserId]);
    CREATE INDEX [IX_PromptConstructionTraces_IntentType] ON [dbo].[PromptConstructionTraces] ([IntentType]);
    CREATE INDEX [IX_PromptConstructionTraces_CreatedAt] ON [dbo].[PromptConstructionTraces] ([CreatedAt]);
    PRINT '  ✅ PromptConstructionTraces table created successfully';
END
ELSE
BEGIN
    PRINT '  ⏭️  PromptConstructionTraces table already exists';
END

-- Prompt Construction Steps Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'PromptConstructionSteps')
BEGIN
    PRINT '  Creating PromptConstructionSteps table...';
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
        [Details] NVARCHAR(MAX) NULL,
        [ErrorMessage] NVARCHAR(MAX) NULL,
        [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        
        CONSTRAINT [FK_PromptConstructionSteps_Traces] 
            FOREIGN KEY ([TraceId]) REFERENCES [dbo].[PromptConstructionTraces] ([TraceId])
            ON DELETE CASCADE
    );
    
    CREATE INDEX [IX_PromptConstructionSteps_TraceId] ON [dbo].[PromptConstructionSteps] ([TraceId]);
    CREATE INDEX [IX_PromptConstructionSteps_StepOrder] ON [dbo].[PromptConstructionSteps] ([StepOrder]);
    PRINT '  ✅ PromptConstructionSteps table created successfully';
END
ELSE
BEGIN
    PRINT '  ⏭️  PromptConstructionSteps table already exists';
END

-- ============================================================================
-- MIGRATION STEP 2: CREATE TOKEN MANAGEMENT TABLES
-- ============================================================================

PRINT '';
PRINT 'Step 2: Creating token management tables...';

-- Token Budgets Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'TokenBudgets')
BEGIN
    PRINT '  Creating TokenBudgets table...';
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
    PRINT '  ✅ TokenBudgets table created successfully';
END
ELSE
BEGIN
    PRINT '  ⏭️  TokenBudgets table already exists';
END

-- Token Usage Analytics Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'TokenUsageAnalytics')
BEGIN
    PRINT '  Creating TokenUsageAnalytics table...';
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
    PRINT '  ✅ TokenUsageAnalytics table created successfully';
END
ELSE
BEGIN
    PRINT '  ⏭️  TokenUsageAnalytics table already exists';
END

PRINT '';
PRINT 'Migration completed successfully!';
PRINT 'All AI Transparency Foundation tables are now available.';
PRINT '';
PRINT 'Next steps:';
PRINT '1. Run the complete schema script to add remaining tables if needed';
PRINT '2. Restart the application to register new services';
PRINT '3. Test transparency features through the enhanced query endpoint';
PRINT '';
PRINT 'Migration timestamp: ' + CONVERT(VARCHAR, GETUTCDATE(), 120) + ' UTC';
GO
