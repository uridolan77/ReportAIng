-- =============================================
-- Query Suggestions System Migration
-- Creates tables for managing dynamic query suggestions
-- =============================================

-- Categories table for organizing suggestions
CREATE TABLE [dbo].[SuggestionCategories] (
    [Id] BIGINT IDENTITY(1,1) PRIMARY KEY,
    [CategoryKey] NVARCHAR(50) NOT NULL UNIQUE,
    [Title] NVARCHAR(100) NOT NULL,
    [Icon] NVARCHAR(10) NULL,
    [Description] NVARCHAR(200) NULL,
    [SortOrder] INT NOT NULL DEFAULT 0,
    [IsActive] BIT NOT NULL DEFAULT 1,
    [CreatedDate] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    [CreatedBy] NVARCHAR(256) NOT NULL,
    [UpdatedDate] DATETIME2 NULL,
    [UpdatedBy] NVARCHAR(256) NULL,
    
    INDEX IX_SuggestionCategories_Active_Sort (IsActive, SortOrder),
    INDEX IX_SuggestionCategories_Key (CategoryKey)
);

-- Main suggestions table
CREATE TABLE [dbo].[QuerySuggestions] (
    [Id] BIGINT IDENTITY(1,1) PRIMARY KEY,
    [CategoryId] BIGINT NOT NULL,
    [QueryText] NVARCHAR(500) NOT NULL,
    [Description] NVARCHAR(200) NOT NULL,
    [DefaultTimeFrame] NVARCHAR(50) NULL, -- 'today', 'yesterday', 'last_7_days', 'last_30_days', 'this_month', 'last_month', etc.
    [SortOrder] INT NOT NULL DEFAULT 0,
    [IsActive] BIT NOT NULL DEFAULT 1,
    [TargetTables] NVARCHAR(500) NULL, -- JSON array of tables this query uses
    [Complexity] TINYINT NOT NULL DEFAULT 1, -- 1=Simple, 2=Medium, 3=Complex
    [RequiredPermissions] NVARCHAR(200) NULL, -- JSON array of required permissions
    [Tags] NVARCHAR(300) NULL, -- JSON array of tags for filtering
    [UsageCount] INT NOT NULL DEFAULT 0,
    [LastUsed] DATETIME2 NULL,
    [CreatedDate] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    [CreatedBy] NVARCHAR(256) NOT NULL,
    [UpdatedDate] DATETIME2 NULL,
    [UpdatedBy] NVARCHAR(256) NULL,
    
    FOREIGN KEY ([CategoryId]) REFERENCES [dbo].[SuggestionCategories]([Id]) ON DELETE CASCADE,
    
    INDEX IX_QuerySuggestions_Category_Active_Sort (CategoryId, IsActive, SortOrder),
    INDEX IX_QuerySuggestions_Usage (UsageCount DESC, LastUsed DESC),
    INDEX IX_QuerySuggestions_TimeFrame (DefaultTimeFrame),
    INDEX IX_QuerySuggestions_Active (IsActive)
);

-- Usage analytics table
CREATE TABLE [dbo].[SuggestionUsageAnalytics] (
    [Id] BIGINT IDENTITY(1,1) PRIMARY KEY,
    [SuggestionId] BIGINT NOT NULL,
    [UserId] NVARCHAR(256) NOT NULL,
    [SessionId] NVARCHAR(50) NULL,
    [UsedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    [TimeFrameUsed] NVARCHAR(50) NULL, -- The actual time frame user selected/used
    [ResultCount] INT NULL, -- Number of results returned
    [ExecutionTimeMs] INT NULL,
    [WasSuccessful] BIT NOT NULL DEFAULT 1,
    [UserFeedback] TINYINT NULL, -- 1=Helpful, 0=Not helpful, NULL=No feedback
    
    FOREIGN KEY ([SuggestionId]) REFERENCES [dbo].[QuerySuggestions]([Id]) ON DELETE CASCADE,
    
    INDEX IX_SuggestionUsage_Suggestion_Date (SuggestionId, UsedAt DESC),
    INDEX IX_SuggestionUsage_User_Date (UserId, UsedAt DESC),
    INDEX IX_SuggestionUsage_Date (UsedAt DESC)
);

-- Time frame definitions table
CREATE TABLE [dbo].[TimeFrameDefinitions] (
    [Id] BIGINT IDENTITY(1,1) PRIMARY KEY,
    [TimeFrameKey] NVARCHAR(50) NOT NULL UNIQUE,
    [DisplayName] NVARCHAR(100) NOT NULL,
    [Description] NVARCHAR(200) NULL,
    [SqlExpression] NVARCHAR(500) NOT NULL, -- SQL expression for the time frame
    [SortOrder] INT NOT NULL DEFAULT 0,
    [IsActive] BIT NOT NULL DEFAULT 1,
    [CreatedDate] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    
    INDEX IX_TimeFrameDefinitions_Active_Sort (IsActive, SortOrder)
);

-- =============================================
-- Seed Data: Time Frame Definitions
-- =============================================
INSERT INTO [dbo].[TimeFrameDefinitions] 
    ([TimeFrameKey], [DisplayName], [Description], [SqlExpression], [SortOrder])
VALUES
    ('today', 'Today', 'Current business date', 'Date = CAST(GETDATE() AS DATE)', 1),
    ('yesterday', 'Yesterday', 'Previous business date', 'Date = CAST(DATEADD(day, -1, GETDATE()) AS DATE)', 2),
    ('last_7_days', 'Last 7 Days', 'Past week including today', 'Date >= CAST(DATEADD(day, -7, GETDATE()) AS DATE)', 3),
    ('last_30_days', 'Last 30 Days', 'Past month including today', 'Date >= CAST(DATEADD(day, -30, GETDATE()) AS DATE)', 4),
    ('this_week', 'This Week', 'Current week (Monday to Sunday)', 'Date >= CAST(DATEADD(day, -(DATEPART(weekday, GETDATE()) - 2), GETDATE()) AS DATE)', 5),
    ('last_week', 'Last Week', 'Previous complete week', 'Date >= CAST(DATEADD(day, -(DATEPART(weekday, GETDATE()) + 5), GETDATE()) AS DATE) AND Date < CAST(DATEADD(day, -(DATEPART(weekday, GETDATE()) - 2), GETDATE()) AS DATE)', 6),
    ('this_month', 'This Month', 'Current calendar month', 'Date >= CAST(DATEADD(month, DATEDIFF(month, 0, GETDATE()), 0) AS DATE)', 7),
    ('last_month', 'Last Month', 'Previous complete month', 'Date >= CAST(DATEADD(month, DATEDIFF(month, 0, GETDATE()) - 1, 0) AS DATE) AND Date < CAST(DATEADD(month, DATEDIFF(month, 0, GETDATE()), 0) AS DATE)', 8),
    ('this_quarter', 'This Quarter', 'Current business quarter', 'Date >= CAST(DATEADD(quarter, DATEDIFF(quarter, 0, GETDATE()), 0) AS DATE)', 9),
    ('last_quarter', 'Last Quarter', 'Previous complete quarter', 'Date >= CAST(DATEADD(quarter, DATEDIFF(quarter, 0, GETDATE()) - 1, 0) AS DATE) AND Date < CAST(DATEADD(quarter, DATEDIFF(quarter, 0, GETDATE()), 0) AS DATE)', 10),
    ('this_year', 'This Year', 'Current calendar year', 'Date >= CAST(DATEADD(year, DATEDIFF(year, 0, GETDATE()), 0) AS DATE)', 11),
    ('last_year', 'Last Year', 'Previous complete year', 'Date >= CAST(DATEADD(year, DATEDIFF(year, 0, GETDATE()) - 1, 0) AS DATE) AND Date < CAST(DATEADD(year, DATEDIFF(year, 0, GETDATE()), 0) AS DATE)', 12),
    ('all_time', 'All Time', 'No time restriction', '1=1', 99);

-- =============================================
-- Seed Data: Categories
-- =============================================
INSERT INTO [dbo].[SuggestionCategories] 
    ([CategoryKey], [Title], [Icon], [Description], [SortOrder], [CreatedBy])
VALUES
    ('financial', '💰 Financial & Revenue', '💰', 'Revenue, deposits, withdrawals, and financial performance metrics', 1, 'System'),
    ('players', '👥 Player Analytics', '👥', 'Player behavior, demographics, and lifecycle analysis', 2, 'System'),
    ('gaming', '🎮 Gaming & Products', '🎮', 'Game performance, provider analysis, and product metrics', 3, 'System'),
    ('transactions', '💳 Transactions & Payments', '💳', 'Payment methods, transaction analysis, and processing metrics', 4, 'System'),
    ('demographics', '📊 Demographics & Behavior', '📊', 'Player demographics, behavior patterns, and segmentation', 5, 'System'),
    ('accounts', '👤 Account & Status', '👤', 'Account management, verification, and player lifecycle', 6, 'System'),
    ('bonuses', '🎁 Bonus & Promotions', '🎁', 'Bonus utilization, promotional effectiveness, and campaigns', 7, 'System'),
    ('compliance', '🔒 Compliance & Risk', '🔒', 'Risk monitoring, compliance reporting, and regulatory metrics', 8, 'System'),
    ('business_intelligence', '📈 Business Intelligence', '📈', 'KPIs, predictive analytics, and strategic insights', 9, 'System'),
    ('operations', '📊 Operations & Trends', '📊', 'Operational metrics, trends, and performance monitoring', 10, 'System');
