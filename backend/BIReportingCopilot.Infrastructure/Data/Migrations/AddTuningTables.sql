-- Migration: Add AI Tuning Tables
-- Description: Creates tables for business schema documentation and AI tuning
-- Target Database: BIReportingCopilot_Dev (System Database)

USE [BIReportingCopilot_Dev]
GO

-- BusinessTableInfo table
CREATE TABLE [dbo].[BusinessTableInfo] (
    [Id] bigint IDENTITY(1,1) NOT NULL,
    [TableName] nvarchar(128) NOT NULL,
    [SchemaName] nvarchar(128) NOT NULL DEFAULT 'common',
    [BusinessPurpose] nvarchar(500) NULL,
    [BusinessContext] nvarchar(2000) NULL,
    [PrimaryUseCase] nvarchar(500) NULL,
    [CommonQueryPatterns] nvarchar(4000) NULL,
    [BusinessRules] nvarchar(2000) NULL,
    [IsActive] bit NOT NULL DEFAULT 1,
    [CreatedDate] datetime2 NOT NULL DEFAULT GETUTCDATE(),
    [UpdatedDate] datetime2 NULL,
    [CreatedBy] nvarchar(256) NOT NULL,
    [UpdatedBy] nvarchar(256) NULL,
    CONSTRAINT [PK_BusinessTableInfo] PRIMARY KEY CLUSTERED ([Id] ASC)
);

-- BusinessColumnInfo table
CREATE TABLE [dbo].[BusinessColumnInfo] (
    [Id] bigint IDENTITY(1,1) NOT NULL,
    [TableInfoId] bigint NOT NULL,
    [ColumnName] nvarchar(128) NOT NULL,
    [BusinessMeaning] nvarchar(500) NULL,
    [BusinessContext] nvarchar(1000) NULL,
    [DataExamples] nvarchar(2000) NULL,
    [ValidationRules] nvarchar(1000) NULL,
    [IsKeyColumn] bit NOT NULL DEFAULT 0,
    [IsActive] bit NOT NULL DEFAULT 1,
    [CreatedDate] datetime2 NOT NULL DEFAULT GETUTCDATE(),
    [UpdatedDate] datetime2 NULL,
    [CreatedBy] nvarchar(256) NOT NULL,
    [UpdatedBy] nvarchar(256) NULL,
    CONSTRAINT [PK_BusinessColumnInfo] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_BusinessColumnInfo_BusinessTableInfo] FOREIGN KEY ([TableInfoId])
        REFERENCES [dbo].[BusinessTableInfo] ([Id]) ON DELETE CASCADE
);

-- QueryPatterns table
CREATE TABLE [dbo].[QueryPatterns] (
    [Id] bigint IDENTITY(1,1) NOT NULL,
    [PatternName] nvarchar(200) NOT NULL,
    [NaturalLanguagePattern] nvarchar(500) NOT NULL,
    [SqlTemplate] nvarchar(4000) NOT NULL,
    [Description] nvarchar(1000) NULL,
    [BusinessContext] nvarchar(1000) NULL,
    [Keywords] nvarchar(2000) NULL,
    [RequiredTables] nvarchar(1000) NULL,
    [Priority] int NOT NULL DEFAULT 1,
    [IsActive] bit NOT NULL DEFAULT 1,
    [UsageCount] int NOT NULL DEFAULT 0,
    [LastUsed] datetime2 NULL,
    [CreatedDate] datetime2 NOT NULL DEFAULT GETUTCDATE(),
    [UpdatedDate] datetime2 NULL,
    [CreatedBy] nvarchar(256) NOT NULL,
    [UpdatedBy] nvarchar(256) NULL,
    CONSTRAINT [PK_QueryPatterns] PRIMARY KEY CLUSTERED ([Id] ASC)
);

-- BusinessGlossary table
CREATE TABLE [dbo].[BusinessGlossary] (
    [Id] bigint IDENTITY(1,1) NOT NULL,
    [Term] nvarchar(200) NOT NULL,
    [Definition] nvarchar(2000) NOT NULL,
    [BusinessContext] nvarchar(1000) NULL,
    [Synonyms] nvarchar(1000) NULL,
    [RelatedTerms] nvarchar(1000) NULL,
    [Category] nvarchar(100) NULL,
    [IsActive] bit NOT NULL DEFAULT 1,
    [UsageCount] int NOT NULL DEFAULT 0,
    [LastUsed] datetime2 NULL,
    [CreatedDate] datetime2 NOT NULL DEFAULT GETUTCDATE(),
    [UpdatedDate] datetime2 NULL,
    [CreatedBy] nvarchar(256) NOT NULL,
    [UpdatedBy] nvarchar(256) NULL,
    CONSTRAINT [PK_BusinessGlossary] PRIMARY KEY CLUSTERED ([Id] ASC)
);

-- AITuningSettings table
CREATE TABLE [dbo].[AITuningSettings] (
    [Id] bigint IDENTITY(1,1) NOT NULL,
    [SettingKey] nvarchar(200) NOT NULL,
    [SettingValue] nvarchar(4000) NOT NULL,
    [Description] nvarchar(1000) NULL,
    [Category] nvarchar(100) NULL,
    [DataType] nvarchar(50) NOT NULL DEFAULT 'string',
    [IsActive] bit NOT NULL DEFAULT 1,
    [CreatedDate] datetime2 NOT NULL DEFAULT GETUTCDATE(),
    [UpdatedDate] datetime2 NULL,
    [CreatedBy] nvarchar(256) NOT NULL,
    [UpdatedBy] nvarchar(256) NULL,
    CONSTRAINT [PK_AITuningSettings] PRIMARY KEY CLUSTERED ([Id] ASC)
);

-- PromptLogs table for admin debugging
CREATE TABLE [dbo].[PromptLogs] (
    [Id] bigint IDENTITY(1,1) NOT NULL,
    [PromptType] nvarchar(100) NOT NULL,
    [UserQuery] nvarchar(2000) NOT NULL,
    [FullPrompt] nvarchar(MAX) NOT NULL,
    [Metadata] nvarchar(MAX) NULL,
    [GeneratedSQL] nvarchar(MAX) NULL,
    [Success] bit NULL,
    [ErrorMessage] nvarchar(2000) NULL,
    [PromptLength] int NOT NULL,
    [ResponseLength] int NULL,
    [ExecutionTimeMs] int NULL,
    [CreatedDate] datetime2 NOT NULL DEFAULT GETUTCDATE(),
    [UserId] nvarchar(256) NULL,
    [SessionId] nvarchar(256) NULL,
    CONSTRAINT [PK_PromptLogs] PRIMARY KEY CLUSTERED ([Id] ASC)
);

-- Create indexes
CREATE UNIQUE NONCLUSTERED INDEX [IX_BusinessTableInfo_Schema_Table]
    ON [dbo].[BusinessTableInfo] ([SchemaName] ASC, [TableName] ASC);

CREATE NONCLUSTERED INDEX [IX_BusinessTableInfo_IsActive]
    ON [dbo].[BusinessTableInfo] ([IsActive] ASC);

CREATE UNIQUE NONCLUSTERED INDEX [IX_BusinessColumnInfo_Table_Column]
    ON [dbo].[BusinessColumnInfo] ([TableInfoId] ASC, [ColumnName] ASC);

CREATE NONCLUSTERED INDEX [IX_BusinessColumnInfo_IsKeyColumn]
    ON [dbo].[BusinessColumnInfo] ([IsKeyColumn] ASC);

CREATE UNIQUE NONCLUSTERED INDEX [IX_QueryPatterns_PatternName]
    ON [dbo].[QueryPatterns] ([PatternName] ASC);

CREATE NONCLUSTERED INDEX [IX_QueryPatterns_Priority_Active]
    ON [dbo].[QueryPatterns] ([Priority] ASC, [IsActive] ASC);

CREATE NONCLUSTERED INDEX [IX_QueryPatterns_UsageCount]
    ON [dbo].[QueryPatterns] ([UsageCount] ASC);

CREATE UNIQUE NONCLUSTERED INDEX [IX_BusinessGlossary_Term]
    ON [dbo].[BusinessGlossary] ([Term] ASC);

CREATE NONCLUSTERED INDEX [IX_BusinessGlossary_Category]
    ON [dbo].[BusinessGlossary] ([Category] ASC);

CREATE UNIQUE NONCLUSTERED INDEX [IX_AITuningSettings_SettingKey]
    ON [dbo].[AITuningSettings] ([SettingKey] ASC);

CREATE NONCLUSTERED INDEX [IX_AITuningSettings_Category]
    ON [dbo].[AITuningSettings] ([Category] ASC);

CREATE NONCLUSTERED INDEX [IX_PromptLogs_CreatedDate]
    ON [dbo].[PromptLogs] ([CreatedDate] DESC);

CREATE NONCLUSTERED INDEX [IX_PromptLogs_PromptType]
    ON [dbo].[PromptLogs] ([PromptType] ASC);

CREATE NONCLUSTERED INDEX [IX_PromptLogs_Success]
    ON [dbo].[PromptLogs] ([Success] ASC);

CREATE NONCLUSTERED INDEX [IX_PromptLogs_UserId]
    ON [dbo].[PromptLogs] ([UserId] ASC);

-- Insert initial data for tbl_Daily_actions
INSERT INTO [dbo].[BusinessTableInfo]
    ([TableName], [SchemaName], [BusinessPurpose], [BusinessContext], [PrimaryUseCase], [CommonQueryPatterns], [BusinessRules], [CreatedBy])
VALUES
    ('tbl_Daily_actions', 'common',
     'Main statistics table holding all player statistics aggregated by player by day',
     'Core table for daily reporting and player activity analysis. Contains comprehensive daily financial and gaming metrics per player.',
     'Daily reporting, player activity tracking, financial summaries, gaming analytics',
     '["deposits today", "deposits by brand today", "player activity today", "total revenue today", "sports vs casino", "bonus analysis"]',
     'Date field represents business date. WhiteLabelID identifies casino brand. All amounts are in player currency.',
     'System');

-- Insert sample query patterns
INSERT INTO [dbo].[QueryPatterns]
    ([PatternName], [NaturalLanguagePattern], [SqlTemplate], [Description], [BusinessContext], [Keywords], [RequiredTables], [Priority], [CreatedBy])
VALUES
    ('Deposits Today', 'deposits today',
     'SELECT SUM(Deposits) as TotalDeposits, COUNT(DISTINCT PlayerID) as PlayerCount FROM common.tbl_Daily_actions WHERE Date = CAST(GETDATE() AS DATE)',
     'Get total deposits for today', 'Daily financial summary', '["deposits", "today", "total"]', '["tbl_Daily_actions"]', 1, 'System'),

    ('Deposits by Brand Today', 'deposits by brand today',
     'SELECT da.WhiteLabelID, SUM(da.Deposits) as TotalDeposits, COUNT(DISTINCT da.PlayerID) as PlayerCount FROM common.tbl_Daily_actions da WHERE da.Date = CAST(GETDATE() AS DATE) GROUP BY da.WhiteLabelID ORDER BY TotalDeposits DESC',
     'Get deposits breakdown by casino brand for today', 'Brand performance analysis', '["deposits", "brand", "whitelabel", "today"]', '["tbl_Daily_actions"]', 1, 'System');

-- Insert sample glossary terms
INSERT INTO [dbo].[BusinessGlossary]
    ([Term], [Definition], [BusinessContext], [Synonyms], [RelatedTerms], [Category], [CreatedBy])
VALUES
    ('Deposits', 'Money deposited by players into their casino accounts', 'Financial transaction representing player funding', '["funding", "money in"]', '["withdrawals", "cashouts"]', 'Financial', 'System'),
    ('WhiteLabelID', 'Unique identifier for casino brand/operator within the platform', 'Multi-brand casino operations identifier', '["brand id", "operator id"]', '["brand", "operator"]', 'Business', 'System'),
    ('Daily Actions', 'Player activities aggregated by day', 'Core business metrics tracked daily per player', '["daily stats", "player activity"]', '["player", "statistics"]', 'Gaming', 'System');

-- Insert AI tuning settings
INSERT INTO [dbo].[AITuningSettings]
    ([SettingKey], [SettingValue], [Description], [Category], [DataType], [CreatedBy])
VALUES
    ('EnableQueryCaching', 'true', 'Enable/disable query result caching for testing purposes', 'Performance', 'boolean', 'System'),
    ('EnablePromptLogging', 'true', 'Enable/disable detailed prompt logging for debugging', 'Debugging', 'boolean', 'System'),
    ('DefaultQueryTimeout', '30', 'Default timeout for SQL queries in seconds', 'Performance', 'integer', 'System'),
    ('MaxPromptLength', '50000', 'Maximum allowed prompt length in characters', 'AI', 'integer', 'System');

PRINT 'AI Tuning tables created successfully!';
