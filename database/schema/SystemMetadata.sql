-- AI-Powered BI Reporting Copilot - System Metadata Schema
-- This schema supports the copilot's metadata management and query optimization

-- Table to store database schema metadata with semantic information
CREATE TABLE [dbo].[SchemaMetadata] (
    [Id] BIGINT IDENTITY(1,1) PRIMARY KEY,
    [DatabaseName] NVARCHAR(128) NOT NULL,
    [SchemaName] NVARCHAR(128) NOT NULL,
    [TableName] NVARCHAR(128) NOT NULL,
    [ColumnName] NVARCHAR(128) NOT NULL,
    [DataType] NVARCHAR(50) NOT NULL,
    [IsNullable] BIT NOT NULL,
    [IsPrimaryKey] BIT NOT NULL,
    [IsForeignKey] BIT NOT NULL,
    [BusinessDescription] NVARCHAR(500) NULL,
    [SemanticTags] NVARCHAR(1000) NULL, -- JSON array of semantic tags
    [SampleValues] NVARCHAR(2000) NULL, -- JSON array of sample values
    [LastUpdated] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    [IsActive] BIT NOT NULL DEFAULT 1,
    INDEX IX_SchemaMetadata_Table (DatabaseName, SchemaName, TableName),
    INDEX IX_SchemaMetadata_Column (TableName, ColumnName)
);

-- Table to store user queries and their metadata for learning and optimization
CREATE TABLE [dbo].[QueryHistory] (
    [Id] BIGINT IDENTITY(1,1) PRIMARY KEY,
    [UserId] NVARCHAR(256) NOT NULL,
    [SessionId] NVARCHAR(50) NOT NULL,
    [NaturalLanguageQuery] NVARCHAR(MAX) NOT NULL,
    [GeneratedSQL] NVARCHAR(MAX) NOT NULL,
    [ExecutionTimeMs] INT NULL,
    [ResultRowCount] INT NULL,
    [ConfidenceScore] DECIMAL(3,2) NULL,
    [UserFeedback] TINYINT NULL, -- 1=Positive, 0=Negative, NULL=No feedback
    [ErrorMessage] NVARCHAR(1000) NULL,
    [QueryTimestamp] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    [IsSuccessful] BIT NOT NULL,
    [QueryComplexity] TINYINT NULL, -- 1=Simple, 2=Medium, 3=Complex
    INDEX IX_QueryHistory_User (UserId, QueryTimestamp DESC),
    INDEX IX_QueryHistory_Session (SessionId),
    INDEX IX_QueryHistory_Timestamp (QueryTimestamp DESC)
);

-- Table to store prompt templates and their versions
CREATE TABLE [dbo].[PromptTemplates] (
    [Id] INT IDENTITY(1,1) PRIMARY KEY,
    [Name] NVARCHAR(100) NOT NULL,
    [Version] NVARCHAR(20) NOT NULL,
    [TemplateContent] NVARCHAR(MAX) NOT NULL,
    [Description] NVARCHAR(500) NULL,
    [IsActive] BIT NOT NULL DEFAULT 1,
    [CreatedBy] NVARCHAR(256) NOT NULL,
    [CreatedDate] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    [SuccessRate] DECIMAL(5,2) NULL,
    [UsageCount] INT NOT NULL DEFAULT 0,
    UNIQUE (Name, Version),
    INDEX IX_PromptTemplates_Active (IsActive, Name)
);

-- Table to store cached query results for performance optimization
CREATE TABLE [dbo].[QueryCache] (
    [Id] BIGINT IDENTITY(1,1) PRIMARY KEY,
    [QueryHash] NVARCHAR(64) NOT NULL, -- SHA256 hash of normalized query
    [NormalizedQuery] NVARCHAR(MAX) NOT NULL,
    [ResultData] NVARCHAR(MAX) NOT NULL, -- JSON serialized result
    [ResultMetadata] NVARCHAR(2000) NULL, -- Column info, row count, etc.
    [CacheTimestamp] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    [ExpiryTimestamp] DATETIME2 NOT NULL,
    [HitCount] INT NOT NULL DEFAULT 0,
    [LastAccessedDate] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UNIQUE (QueryHash),
    INDEX IX_QueryCache_Expiry (ExpiryTimestamp),
    INDEX IX_QueryCache_LastAccessed (LastAccessedDate)
);

-- Table to store user preferences and personalization settings
CREATE TABLE [dbo].[UserPreferences] (
    [UserId] NVARCHAR(256) PRIMARY KEY,
    [PreferredChartTypes] NVARCHAR(500) NULL, -- JSON array
    [DefaultDateRange] NVARCHAR(50) NULL,
    [MaxRowsPerQuery] INT NOT NULL DEFAULT 1000,
    [EnableQuerySuggestions] BIT NOT NULL DEFAULT 1,
    [EnableAutoVisualization] BIT NOT NULL DEFAULT 1,
    [NotificationSettings] NVARCHAR(1000) NULL, -- JSON object
    [LastLoginDate] DATETIME2 NULL,
    [CreatedDate] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    [UpdatedDate] DATETIME2 NOT NULL DEFAULT GETUTCDATE()
);

-- Table to store system configuration and feature flags
CREATE TABLE [dbo].[SystemConfiguration] (
    [Key] NVARCHAR(100) PRIMARY KEY,
    [Value] NVARCHAR(MAX) NOT NULL,
    [DataType] NVARCHAR(20) NOT NULL, -- 'string', 'int', 'bool', 'json'
    [Description] NVARCHAR(500) NULL,
    [IsEncrypted] BIT NOT NULL DEFAULT 0,
    [LastUpdated] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    [UpdatedBy] NVARCHAR(256) NOT NULL
);

-- Table to store audit logs for compliance and monitoring
CREATE TABLE [dbo].[AuditLog] (
    [Id] BIGINT IDENTITY(1,1) PRIMARY KEY,
    [UserId] NVARCHAR(256) NOT NULL,
    [Action] NVARCHAR(100) NOT NULL,
    [EntityType] NVARCHAR(50) NOT NULL,
    [EntityId] NVARCHAR(100) NULL,
    [Details] NVARCHAR(MAX) NULL, -- JSON object with action details
    [IpAddress] NVARCHAR(45) NULL,
    [UserAgent] NVARCHAR(500) NULL,
    [Timestamp] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    [Severity] NVARCHAR(20) NOT NULL DEFAULT 'Info',
    INDEX IX_AuditLog_User (UserId, Timestamp DESC),
    INDEX IX_AuditLog_Action (Action, Timestamp DESC),
    INDEX IX_AuditLog_Timestamp (Timestamp DESC)
);

-- Insert default system configuration
INSERT INTO [dbo].[SystemConfiguration] ([Key], [Value], [DataType], [Description], [UpdatedBy])
VALUES 
    ('MaxQueryExecutionTimeSeconds', '30', 'int', 'Maximum time allowed for query execution', 'System'),
    ('MaxResultRows', '10000', 'int', 'Maximum number of rows returned per query', 'System'),
    ('EnableQueryCaching', 'true', 'bool', 'Enable caching of query results', 'System'),
    ('CacheExpiryHours', '24', 'int', 'Default cache expiry time in hours', 'System'),
    ('EnableAuditLogging', 'true', 'bool', 'Enable comprehensive audit logging', 'System'),
    ('OpenAIModel', 'gpt-4', 'string', 'OpenAI model to use for query generation', 'System'),
    ('MaxTokensPerRequest', '2000', 'int', 'Maximum tokens per OpenAI request', 'System');
