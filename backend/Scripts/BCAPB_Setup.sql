-- =============================================
-- Business-Context-Aware Prompt Building (BCAPB) Setup Script
-- This script creates all necessary tables and seeds initial data
-- =============================================

USE [BIReportingCopilot_Dev]
GO

-- =============================================
-- 1. CREATE TABLES
-- =============================================

-- Create IntentTypes table
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='IntentTypes' AND xtype='U')
BEGIN
    CREATE TABLE [dbo].[IntentTypes] (
        [Id] bigint IDENTITY(1,1) NOT NULL,
        [Name] nvarchar(50) NOT NULL,
        [Description] nvarchar(500) NOT NULL,
        [PromptInstructions] nvarchar(2000) NULL,
        [IsActive] bit NOT NULL DEFAULT 1,
        [CreatedDate] datetime2(7) NOT NULL DEFAULT GETUTCDATE(),
        [CreatedBy] nvarchar(256) NOT NULL,
        CONSTRAINT [PK_IntentTypes] PRIMARY KEY CLUSTERED ([Id] ASC)
    )
    
    CREATE UNIQUE NONCLUSTERED INDEX [IX_IntentTypes_Name] ON [dbo].[IntentTypes] ([Name] ASC)
    CREATE NONCLUSTERED INDEX [IX_IntentTypes_IsActive] ON [dbo].[IntentTypes] ([IsActive] ASC)
    
    PRINT 'IntentTypes table created successfully'
END
ELSE
    PRINT 'IntentTypes table already exists'
GO

-- Enhance existing PromptTemplates table with BCAPB columns
IF EXISTS (SELECT * FROM sysobjects WHERE name='PromptTemplates' AND xtype='U')
BEGIN
    -- Add TemplateKey column if it doesn't exist
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('PromptTemplates') AND name = 'TemplateKey')
    BEGIN
        ALTER TABLE [dbo].[PromptTemplates] ADD [TemplateKey] nvarchar(100) NULL
        PRINT 'Added TemplateKey column to PromptTemplates'
    END

    -- Add IntentType column if it doesn't exist
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('PromptTemplates') AND name = 'IntentType')
    BEGIN
        ALTER TABLE [dbo].[PromptTemplates] ADD [IntentType] nvarchar(50) NULL
        PRINT 'Added IntentType column to PromptTemplates'
    END

    -- Add Priority column if it doesn't exist
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('PromptTemplates') AND name = 'Priority')
    BEGIN
        ALTER TABLE [dbo].[PromptTemplates] ADD [Priority] int NOT NULL DEFAULT 100
        PRINT 'Added Priority column to PromptTemplates'
    END

    -- Add Tags column if it doesn't exist
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('PromptTemplates') AND name = 'Tags')
    BEGIN
        ALTER TABLE [dbo].[PromptTemplates] ADD [Tags] nvarchar(500) NULL
        PRINT 'Added Tags column to PromptTemplates'
    END

    -- Create index on TemplateKey if it doesn't exist
    IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID('PromptTemplates') AND name = 'IX_PromptTemplates_TemplateKey')
    BEGIN
        CREATE UNIQUE NONCLUSTERED INDEX [IX_PromptTemplates_TemplateKey] ON [dbo].[PromptTemplates] ([TemplateKey] ASC) WHERE [TemplateKey] IS NOT NULL
        PRINT 'Created index on TemplateKey'
    END

    -- Create index on IntentType if it doesn't exist
    IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID('PromptTemplates') AND name = 'IX_PromptTemplates_IntentType_IsActive')
    BEGIN
        CREATE NONCLUSTERED INDEX [IX_PromptTemplates_IntentType_IsActive] ON [dbo].[PromptTemplates] ([IntentType] ASC, [IsActive] ASC) WHERE [IntentType] IS NOT NULL
        PRINT 'Created index on IntentType and IsActive'
    END

    PRINT 'PromptTemplates table enhanced successfully'
END

-- Create QueryExamples table
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='QueryExamples' AND xtype='U')
BEGIN
    CREATE TABLE [dbo].[QueryExamples] (
        [Id] bigint IDENTITY(1,1) NOT NULL,
        [NaturalLanguageQuery] nvarchar(1000) NOT NULL,
        [GeneratedSql] nvarchar(max) NOT NULL,
        [IntentType] nvarchar(50) NOT NULL,
        [Domain] nvarchar(100) NOT NULL,
        [UsedTables] nvarchar(1000) NOT NULL,
        [BusinessContext] nvarchar(2000) NOT NULL,
        [SuccessRate] decimal(5,4) NOT NULL DEFAULT 0.0,
        [UsageCount] int NOT NULL DEFAULT 0,
        [IsActive] bit NOT NULL DEFAULT 1,
        [CreatedDate] datetime2(7) NOT NULL DEFAULT GETUTCDATE(),
        [LastUsed] datetime2(7) NULL,
        [CreatedBy] nvarchar(256) NOT NULL,
        [SemanticTags] nvarchar(500) NULL,
        [ComplexityScore] decimal(3,2) NULL,
        [TargetAudience] nvarchar(100) NULL,
        [AverageExecutionTimeMs] int NULL,
        [UserSatisfactionScore] decimal(3,2) NULL,
        CONSTRAINT [PK_QueryExamples] PRIMARY KEY CLUSTERED ([Id] ASC)
    )
    
    CREATE NONCLUSTERED INDEX [IX_QueryExamples_IntentType_Domain] ON [dbo].[QueryExamples] ([IntentType] ASC, [Domain] ASC)
    CREATE NONCLUSTERED INDEX [IX_QueryExamples_SuccessRate] ON [dbo].[QueryExamples] ([SuccessRate] DESC)
    CREATE NONCLUSTERED INDEX [IX_QueryExamples_Active_Usage] ON [dbo].[QueryExamples] ([IsActive] ASC, [UsageCount] DESC)
    
    PRINT 'QueryExamples table created successfully'
END
ELSE
    PRINT 'QueryExamples table already exists'
GO

-- Create VectorEmbeddings table
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='VectorEmbeddings' AND xtype='U')
BEGIN
    CREATE TABLE [dbo].[VectorEmbeddings] (
        [Id] bigint IDENTITY(1,1) NOT NULL,
        [EntityType] nvarchar(50) NOT NULL,
        [EntityId] bigint NOT NULL,
        [EmbeddingVector] nvarchar(max) NOT NULL,
        [ModelVersion] nvarchar(50) NOT NULL,
        [CreatedDate] datetime2(7) NOT NULL DEFAULT GETUTCDATE(),
        [UpdatedDate] datetime2(7) NULL,
        [SourceText] nvarchar(1000) NULL,
        [Language] nvarchar(100) NULL DEFAULT 'en',
        [QualityScore] decimal(5,4) NULL,
        [VectorDimensions] int NOT NULL DEFAULT 1536,
        [IndexingStrategy] nvarchar(100) NULL,
        [IsActive] bit NOT NULL DEFAULT 1,
        CONSTRAINT [PK_VectorEmbeddings] PRIMARY KEY CLUSTERED ([Id] ASC)
    )
    
    CREATE UNIQUE NONCLUSTERED INDEX [IX_VectorEmbeddings_EntityType_EntityId] ON [dbo].[VectorEmbeddings] ([EntityType] ASC, [EntityId] ASC)
    CREATE NONCLUSTERED INDEX [IX_VectorEmbeddings_Model_Active] ON [dbo].[VectorEmbeddings] ([ModelVersion] ASC, [IsActive] ASC)
    
    PRINT 'VectorEmbeddings table created successfully'
END
ELSE
    PRINT 'VectorEmbeddings table already exists'
GO

-- Create PromptGenerationLogs table
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='PromptGenerationLogs' AND xtype='U')
BEGIN
    CREATE TABLE [dbo].[PromptGenerationLogs] (
        [Id] bigint IDENTITY(1,1) NOT NULL,
        [UserId] nvarchar(256) NOT NULL,
        [UserQuestion] nvarchar(1000) NOT NULL,
        [GeneratedPrompt] nvarchar(max) NOT NULL,
        [PromptLength] int NOT NULL,
        [IntentType] nvarchar(50) NOT NULL,
        [Domain] nvarchar(100) NOT NULL,
        [ConfidenceScore] decimal(5,4) NOT NULL,
        [TablesUsed] int NOT NULL,
        [GenerationTimeMs] int NOT NULL,
        [TemplateUsed] nvarchar(100) NULL,
        [WasSuccessful] bit NOT NULL DEFAULT 1,
        [ErrorMessage] nvarchar(1000) NULL,
        [Timestamp] datetime2(7) NOT NULL DEFAULT GETUTCDATE(),
        [ExtractedEntities] nvarchar(500) NULL,
        [TimeContext] nvarchar(200) NULL,
        [TokensUsed] int NULL,
        [CostEstimate] decimal(10,4) NULL,
        [ModelUsed] nvarchar(50) NULL,
        [UserRating] decimal(3,2) NULL,
        [SqlGeneratedSuccessfully] bit NULL,
        [QueryExecutedSuccessfully] bit NULL,
        [UserFeedback] nvarchar(500) NULL,
        [PromptTemplateId] bigint NULL,
        [SessionId] nvarchar(100) NULL,
        [RequestId] nvarchar(100) NULL,
        CONSTRAINT [PK_PromptGenerationLogs] PRIMARY KEY CLUSTERED ([Id] ASC)
    )
    
    CREATE NONCLUSTERED INDEX [IX_PromptGenerationLogs_UserId_Timestamp] ON [dbo].[PromptGenerationLogs] ([UserId] ASC, [Timestamp] DESC)
    CREATE NONCLUSTERED INDEX [IX_PromptGenerationLogs_IntentType_Domain] ON [dbo].[PromptGenerationLogs] ([IntentType] ASC, [Domain] ASC)
    CREATE NONCLUSTERED INDEX [IX_PromptGenerationLogs_Success_Timestamp] ON [dbo].[PromptGenerationLogs] ([WasSuccessful] ASC, [Timestamp] DESC)
    CREATE NONCLUSTERED INDEX [IX_PromptGenerationLogs_PromptTemplateId] ON [dbo].[PromptGenerationLogs] ([PromptTemplateId] ASC)
    
    -- Add foreign key constraints
    IF EXISTS (SELECT * FROM sysobjects WHERE name='PromptTemplates' AND xtype='U')
    BEGIN
        ALTER TABLE [dbo].[PromptGenerationLogs]
        ADD CONSTRAINT [FK_PromptGenerationLogs_PromptTemplates_PromptTemplateId]
        FOREIGN KEY ([PromptTemplateId]) REFERENCES [dbo].[PromptTemplates] ([Id]) ON DELETE SET NULL
    END

    -- Add foreign key constraint to Users table (if it exists)
    IF EXISTS (SELECT * FROM sysobjects WHERE name='Users' AND xtype='U')
    BEGIN
        ALTER TABLE [dbo].[PromptGenerationLogs]
        ADD CONSTRAINT [FK_PromptGenerationLogs_Users_UserId]
        FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users] ([Id]) ON DELETE CASCADE
    END
    
    PRINT 'PromptGenerationLogs table created successfully'
END
ELSE
    PRINT 'PromptGenerationLogs table already exists'
GO

PRINT 'All BCAPB tables created successfully!'
PRINT ''
PRINT '=============================================';
PRINT '2. SEED INITIAL DATA';
PRINT '============================================='

-- Seed IntentTypes
IF NOT EXISTS (SELECT * FROM [dbo].[IntentTypes])
BEGIN
    INSERT INTO [dbo].[IntentTypes] ([Name], [Description], [PromptInstructions], [IsActive], [CreatedBy])
    VALUES
        ('Analytical', 'Complex analytical queries requiring aggregations, calculations, and deep insights', 'Focus on aggregations, calculations, and analytical insights. Use appropriate GROUP BY, HAVING, and window functions.', 1, 'System'),
        ('Comparison', 'Queries comparing values across different dimensions or time periods', 'Use CASE statements, PIVOT operations, or CTEs to clearly show comparisons. Label compared entities clearly.', 1, 'System'),
        ('Trend', 'Time-series analysis and trend identification queries', 'Focus on time-based grouping and ordering. Consider window functions for running totals and moving averages.', 1, 'System'),
        ('Aggregation', 'Data summarization and aggregation queries', 'Use appropriate aggregate functions (SUM, COUNT, AVG, etc.) with proper GROUP BY clauses.', 1, 'System'),
        ('Exploratory', 'Data exploration and discovery queries', 'Generate queries that help discover patterns. Include sample data and use appropriate LIMIT/TOP clauses.', 1, 'System'),
        ('Operational', 'Operational and transactional queries for business processes', 'Focus on specific business operations. Include relevant filters and business logic.', 1, 'System')

    PRINT 'IntentTypes seeded successfully'
END
ELSE
    PRINT 'IntentTypes already contain data'
GO

-- Seed PromptTemplates with BCAPB templates
IF NOT EXISTS (SELECT * FROM [dbo].[PromptTemplates] WHERE [TemplateKey] LIKE 'bcapb_%')
BEGIN
    INSERT INTO [dbo].[PromptTemplates] ([Name], [Version], [Content], [Description], [IsActive], [CreatedBy], [CreatedDate], [TemplateKey], [IntentType], [Priority], [Tags])
    VALUES
    ('BCAPB Analytical Template', '1.0',
'You are a SQL expert helping to analyze business data.

## User Question
{USER_QUESTION}

## Business Context
{BUSINESS_CONTEXT}

## Available Tables
{PRIMARY_TABLES}

## Business Rules
{BUSINESS_RULES}

## Requirements
- Generate analytical SQL with proper aggregations
- Include relevant GROUP BY and ORDER BY clauses
- Consider performance for large datasets
- Use appropriate window functions if needed

## Examples
{EXAMPLES}

## Additional Context
- Current Date: {CURRENT_DATE}
- Metrics: {METRICS}
- Dimensions: {DIMENSIONS}
- Time Context: {TIME_CONTEXT}

Generate a well-structured SQL query that provides analytical insights.',
    'Template for complex analytical queries', 1, 'System', GETUTCDATE(), 'bcapb_analytical_template', 'Analytical', 100, 'analytical,aggregation,insights'),

    ('BCAPB Comparison Template', '1.0',
'You are a SQL expert specializing in comparative analysis.

## User Question
{USER_QUESTION}

## Business Context
{BUSINESS_CONTEXT}

## Available Tables
{PRIMARY_TABLES}

## Comparison Requirements
- Generate SQL that compares values across dimensions
- Use CASE, PIVOT, or CTEs for clear comparisons
- Label compared entities clearly
- Include ratios and percentages where appropriate

## Examples
{EXAMPLES}

## Additional Context
- Comparison Terms: {COMPARISON_TERMS}
- Time Context: {TIME_CONTEXT}

Generate SQL that clearly shows the requested comparison.',
    'Template for comparison queries', 1, 'System', GETUTCDATE(), 'bcapb_comparison_template', 'Comparison', 100, 'comparison,analysis,pivot'),

    ('bcapb_trend_template', 'Trend',
'You are a SQL expert specializing in trend analysis.

## User Question
{USER_QUESTION}

## Business Context
{BUSINESS_CONTEXT}

## Available Tables
{PRIMARY_TABLES}

## Trend Analysis Requirements
- Generate SQL for time-based analysis
- Include proper date grouping (daily, weekly, monthly)
- Use window functions for running totals or moving averages
- Ensure chronological ordering

## Examples
{EXAMPLES}

## Time Context
{TIME_CONTEXT}

Generate SQL that reveals trends and patterns over time.',
    'Template for trend analysis queries', 100, 1, 'trend,time-series,temporal', 'System'),

    ('bcapb_aggregation_template', 'Aggregation',
'You are a SQL expert specializing in data aggregation.

## User Question
{USER_QUESTION}

## Business Context
{BUSINESS_CONTEXT}

## Available Tables
{PRIMARY_TABLES}

## Aggregation Requirements
- Generate SQL that properly aggregates data
- Use appropriate aggregate functions (SUM, COUNT, AVG, etc.)
- Include proper GROUP BY clauses
- Consider HAVING clauses for filtered aggregations

## Examples
{EXAMPLES}

## Additional Context
- Metrics to Calculate: {METRICS}
- Grouping Dimensions: {DIMENSIONS}

Generate a SQL query that provides the requested aggregated results.',
    'Template for data aggregation queries', 100, 1, 'aggregation,summary,grouping', 'System'),

    ('bcapb_exploratory_template', 'Exploratory',
'You are a SQL expert helping users explore and understand their data.

## User Question
{USER_QUESTION}

## Business Context
{BUSINESS_CONTEXT}

## Available Tables
{PRIMARY_TABLES}

## Exploration Guidelines
- Generate SQL that helps discover patterns and insights
- Include relevant sample data where appropriate
- Consider showing distributions, counts, and summaries
- Limit results appropriately for exploration (TOP/LIMIT)

## Examples
{EXAMPLES}

Generate a SQL query that helps the user explore and understand their data.',
    'Template for data exploration queries', 100, 1, 'exploration,discovery,analysis', 'System')

    PRINT 'BCAPBPromptTemplates seeded successfully'
END
ELSE
    PRINT 'BCAPBPromptTemplates already contain data'
GO

-- Seed QueryExamples
IF NOT EXISTS (SELECT * FROM [dbo].[QueryExamples])
BEGIN
    INSERT INTO [dbo].[QueryExamples]
    ([NaturalLanguageQuery], [GeneratedSql], [IntentType], [Domain], [UsedTables], [BusinessContext],
     [SuccessRate], [UsageCount], [IsActive], [CreatedBy], [SemanticTags], [ComplexityScore], [TargetAudience])
    VALUES
    ('What is the total revenue by country for the last 30 days?',
'SELECT
    Country,
    SUM(Revenue) as TotalRevenue,
    COUNT(DISTINCT UserId) as UniqueUsers
FROM tbl_Daily_actions
WHERE ActionDate >= DATEADD(day, -30, GETDATE())
GROUP BY Country
ORDER BY TotalRevenue DESC',
    'Aggregation', 'Gaming', 'tbl_Daily_actions',
    'Revenue aggregation by geographic dimension for recent period',
    0.95, 25, 1, 'System', 'revenue,country,geographic,aggregation', 0.6, 'Business Analyst'),

    ('Show me the revenue trend over the last 6 months',
'SELECT
    YEAR(ActionDate) as Year,
    MONTH(ActionDate) as Month,
    SUM(Revenue) as MonthlyRevenue,
    LAG(SUM(Revenue)) OVER (ORDER BY YEAR(ActionDate), MONTH(ActionDate)) as PreviousMonthRevenue,
    CASE
        WHEN LAG(SUM(Revenue)) OVER (ORDER BY YEAR(ActionDate), MONTH(ActionDate)) > 0
        THEN ((SUM(Revenue) - LAG(SUM(Revenue)) OVER (ORDER BY YEAR(ActionDate), MONTH(ActionDate))) / LAG(SUM(Revenue)) OVER (ORDER BY YEAR(ActionDate), MONTH(ActionDate))) * 100
        ELSE NULL
    END as GrowthPercentage
FROM tbl_Daily_actions
WHERE ActionDate >= DATEADD(month, -6, GETDATE())
GROUP BY YEAR(ActionDate), MONTH(ActionDate)
ORDER BY Year, Month',
    'Trend', 'Gaming', 'tbl_Daily_actions',
    'Time-series revenue analysis with growth calculation',
    0.88, 18, 1, 'System', 'revenue,trend,time-series,growth', 0.8, 'Business Analyst'),

    ('Compare daily active users between mobile and desktop platforms this week vs last week',
'WITH WeeklyDAU AS (
    SELECT
        Platform,
        CASE
            WHEN ActionDate >= DATEADD(WEEK, -1, CAST(GETDATE() AS DATE))
            THEN ''This Week''
            ELSE ''Last Week''
        END as WeekPeriod,
        COUNT(DISTINCT UserId) as DAU
    FROM tbl_Daily_actions
    WHERE ActionDate >= DATEADD(WEEK, -2, CAST(GETDATE() AS DATE))
    GROUP BY
        Platform,
        CASE
            WHEN ActionDate >= DATEADD(WEEK, -1, CAST(GETDATE() AS DATE))
            THEN ''This Week''
            ELSE ''Last Week''
        END
)
SELECT
    Platform,
    MAX(CASE WHEN WeekPeriod = ''Last Week'' THEN DAU END) as LastWeekDAU,
    MAX(CASE WHEN WeekPeriod = ''This Week'' THEN DAU END) as ThisWeekDAU,
    CAST((MAX(CASE WHEN WeekPeriod = ''This Week'' THEN DAU END) -
          MAX(CASE WHEN WeekPeriod = ''Last Week'' THEN DAU END)) * 100.0 /
          MAX(CASE WHEN WeekPeriod = ''Last Week'' THEN DAU END) AS DECIMAL(5,2)) as PercentChange
FROM WeeklyDAU
GROUP BY Platform
ORDER BY Platform',
    'Comparison', 'Gaming', 'tbl_Daily_actions',
    'Platform comparison of daily active users with week-over-week change',
    0.92, 87, 1, 'System', 'dau,platform,comparison,weekly', 0.9, 'Product Manager'),

    ('What are the top 10 users by total spending this month?',
'SELECT TOP 10
    UserId,
    SUM(Revenue) as TotalSpending,
    COUNT(*) as TransactionCount,
    AVG(Revenue) as AvgTransactionValue,
    MAX(ActionDate) as LastActivity
FROM tbl_Daily_actions
WHERE ActionDate >= DATEADD(MONTH, DATEDIFF(MONTH, 0, GETDATE()), 0)
    AND ActionDate < DATEADD(MONTH, DATEDIFF(MONTH, 0, GETDATE()) + 1, 0)
    AND Revenue > 0
GROUP BY UserId
ORDER BY TotalSpending DESC',
    'Aggregation', 'Gaming', 'tbl_Daily_actions',
    'Top spenders analysis for current month with transaction metrics',
    0.94, 156, 1, 'System', 'users,spending,top,monthly', 0.7, 'Business Analyst'),

    ('Show me user activity patterns by hour of day',
'SELECT
    DATEPART(HOUR, ActionDate) as HourOfDay,
    COUNT(DISTINCT UserId) as UniqueUsers,
    COUNT(*) as TotalActions,
    SUM(Revenue) as TotalRevenue,
    AVG(CAST(Revenue as FLOAT)) as AvgRevenue
FROM tbl_Daily_actions
WHERE ActionDate >= DATEADD(day, -7, GETDATE())
GROUP BY DATEPART(HOUR, ActionDate)
ORDER BY HourOfDay',
    'Exploratory', 'Gaming', 'tbl_Daily_actions',
    'Hourly activity pattern analysis for user behavior insights',
    0.91, 73, 1, 'System', 'activity,patterns,hourly,behavior', 0.6, 'Data Analyst')

    PRINT 'QueryExamples seeded successfully'
END
ELSE
    PRINT 'QueryExamples already contain data'
GO

PRINT ''
PRINT '============================================='
PRINT '3. VERIFICATION'
PRINT '============================================='

-- Verify table creation and data seeding
SELECT 'IntentTypes' as TableName, COUNT(*) as RecordCount FROM [dbo].[IntentTypes]
UNION ALL
SELECT 'BCAPBPromptTemplates' as TableName, COUNT(*) as RecordCount FROM [dbo].[BCAPBPromptTemplates]
UNION ALL
SELECT 'QueryExamples' as TableName, COUNT(*) as RecordCount FROM [dbo].[QueryExamples]
UNION ALL
SELECT 'VectorEmbeddings' as TableName, COUNT(*) as RecordCount FROM [dbo].[VectorEmbeddings]
UNION ALL
SELECT 'PromptGenerationLogs' as TableName, COUNT(*) as RecordCount FROM [dbo].[PromptGenerationLogs]

PRINT ''
PRINT '============================================='
PRINT 'BCAPB SETUP COMPLETED SUCCESSFULLY!'
PRINT '============================================='
PRINT 'Tables created:'
PRINT '- IntentTypes'
PRINT '- BCAPBPromptTemplates'
PRINT '- QueryExamples'
PRINT '- VectorEmbeddings'
PRINT '- PromptGenerationLogs'
PRINT ''
PRINT 'Initial data seeded:'
PRINT '- 6 Intent Types'
PRINT '- 5 Prompt Templates'
PRINT '- 5 Query Examples'
PRINT ''
PRINT 'You can now use the BCAPB API endpoints:'
PRINT '- POST /api/businesscontext/analyze'
PRINT '- POST /api/businesscontext/metadata'
PRINT '- POST /api/businesscontext/prompt'
PRINT '- POST /api/businesscontext/intent'
PRINT '- POST /api/businesscontext/entities'
PRINT '- POST /api/businesscontext/tables'
PRINT '- POST /api/businesscontext/seed'
PRINT '============================================='
