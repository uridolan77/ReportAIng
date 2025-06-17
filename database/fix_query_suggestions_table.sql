-- Fix Missing QuerySuggestions Table
-- Run this script to create the missing QuerySuggestions table and related objects

USE [BIReportingCopilot_Dev];
GO

-- Check if QuerySuggestions table exists
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[QuerySuggestions]') AND type in (N'U'))
BEGIN
    PRINT 'Creating QuerySuggestions table...';
    
    -- Create QuerySuggestions table
    CREATE TABLE [dbo].[QuerySuggestions] (
        [Id] int IDENTITY(1,1) NOT NULL,
        [CategoryId] int NULL,
        [Text] nvarchar(500) NOT NULL,
        [Description] nvarchar(1000) NULL,
        [Query] nvarchar(max) NULL,
        [QueryText] nvarchar(max) NULL,
        [Tags] nvarchar(500) NULL,
        [Keywords] nvarchar(500) NULL,
        [Complexity] int NOT NULL DEFAULT(1),
        [Confidence] decimal(3,2) NOT NULL DEFAULT(0.5),
        [Relevance] decimal(3,2) NOT NULL DEFAULT(0.5),
        [UsageCount] int NOT NULL DEFAULT(0),
        [LastUsed] datetime2(7) NULL,
        [IsActive] bit NOT NULL DEFAULT(1),
        [SortOrder] int NOT NULL DEFAULT(0),
        [Source] nvarchar(100) NULL,
        [RequiredTables] nvarchar(500) NULL,
        [TargetTables] nvarchar(500) NULL,
        [RequiredPermissions] nvarchar(500) NULL,
        [DefaultTimeFrame] nvarchar(100) NULL,
        [ErrorMessage] nvarchar(1000) NULL,
        [CreatedBy] nvarchar(450) NULL,
        [CreatedDate] datetime2(7) NOT NULL DEFAULT(GETUTCDATE()),
        [UpdatedBy] nvarchar(450) NULL,
        [UpdatedDate] datetime2(7) NULL,
        [CreatedAt] datetime2(7) NOT NULL DEFAULT(GETUTCDATE()),
        CONSTRAINT [PK_QuerySuggestions] PRIMARY KEY CLUSTERED ([Id] ASC)
    );
    
    PRINT 'QuerySuggestions table created successfully.';
END
ELSE
BEGIN
    PRINT 'QuerySuggestions table already exists.';
END
GO

-- Check if SuggestionCategories table exists
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SuggestionCategories]') AND type in (N'U'))
BEGIN
    PRINT 'Creating SuggestionCategories table...';
    
    -- Create SuggestionCategories table
    CREATE TABLE [dbo].[SuggestionCategories] (
        [Id] int IDENTITY(1,1) NOT NULL,
        [CategoryKey] nvarchar(100) NOT NULL,
        [Title] nvarchar(200) NOT NULL,
        [Description] nvarchar(1000) NULL,
        [Icon] nvarchar(100) NULL,
        [SortOrder] int NOT NULL DEFAULT(0),
        [IsActive] bit NOT NULL DEFAULT(1),
        [CreatedBy] nvarchar(450) NULL,
        [CreatedDate] datetime2(7) NOT NULL DEFAULT(GETUTCDATE()),
        [UpdatedBy] nvarchar(450) NULL,
        [UpdatedDate] datetime2(7) NULL,
        CONSTRAINT [PK_SuggestionCategories] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [IX_SuggestionCategories_CategoryKey] UNIQUE NONCLUSTERED ([CategoryKey] ASC)
    );
    
    PRINT 'SuggestionCategories table created successfully.';
END
ELSE
BEGIN
    PRINT 'SuggestionCategories table already exists.';
END
GO

-- Add foreign key constraint if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_QuerySuggestions_SuggestionCategories_CategoryId]'))
BEGIN
    PRINT 'Adding foreign key constraint...';
    
    ALTER TABLE [dbo].[QuerySuggestions]
    ADD CONSTRAINT [FK_QuerySuggestions_SuggestionCategories_CategoryId] 
    FOREIGN KEY([CategoryId]) REFERENCES [dbo].[SuggestionCategories] ([Id]);
    
    PRINT 'Foreign key constraint added successfully.';
END
ELSE
BEGIN
    PRINT 'Foreign key constraint already exists.';
END
GO

-- Insert default categories if they don't exist
IF NOT EXISTS (SELECT * FROM [dbo].[SuggestionCategories])
BEGIN
    PRINT 'Inserting default suggestion categories...';
    
    INSERT INTO [dbo].[SuggestionCategories] ([CategoryKey], [Title], [Description], [Icon], [SortOrder], [IsActive])
    VALUES 
        ('basic-queries', 'Basic Queries', 'Simple data retrieval queries', 'search', 1, 1),
        ('analytics', 'Analytics', 'Data analysis and reporting queries', 'chart-bar', 2, 1),
        ('performance', 'Performance', 'Performance monitoring queries', 'tachometer-alt', 3, 1),
        ('business-intelligence', 'Business Intelligence', 'BI and dashboard queries', 'chart-pie', 4, 1),
        ('data-exploration', 'Data Exploration', 'Exploratory data analysis', 'search-plus', 5, 1);
    
    PRINT 'Default categories inserted successfully.';
END
ELSE
BEGIN
    PRINT 'Suggestion categories already exist.';
END
GO

-- Insert sample query suggestions if they don't exist
IF NOT EXISTS (SELECT * FROM [dbo].[QuerySuggestions])
BEGIN
    PRINT 'Inserting sample query suggestions...';
    
    DECLARE @BasicQueriesId int = (SELECT Id FROM [dbo].[SuggestionCategories] WHERE CategoryKey = 'basic-queries');
    DECLARE @AnalyticsId int = (SELECT Id FROM [dbo].[SuggestionCategories] WHERE CategoryKey = 'analytics');
    
    INSERT INTO [dbo].[QuerySuggestions] ([CategoryId], [Text], [Description], [QueryText], [Tags], [Keywords], [Complexity], [Confidence], [IsActive], [SortOrder])
    VALUES 
        (@BasicQueriesId, 'Show all users', 'Display all users in the system', 'SELECT * FROM Users', 'users,basic', 'users,all,list', 1, 0.9, 1, 1),
        (@BasicQueriesId, 'Count total records', 'Count total number of records in a table', 'SELECT COUNT(*) FROM [TableName]', 'count,basic', 'count,total,records', 1, 0.9, 1, 2),
        (@AnalyticsId, 'Monthly user activity', 'Show user activity by month', 'SELECT YEAR(CreatedDate) as Year, MONTH(CreatedDate) as Month, COUNT(*) as UserCount FROM Users GROUP BY YEAR(CreatedDate), MONTH(CreatedDate)', 'analytics,monthly', 'monthly,activity,users', 2, 0.8, 1, 1),
        (@AnalyticsId, 'Top 10 most active users', 'Find the most active users', 'SELECT TOP 10 UserId, COUNT(*) as ActivityCount FROM UserActivity GROUP BY UserId ORDER BY ActivityCount DESC', 'analytics,top', 'top,active,users', 2, 0.8, 1, 2);
    
    PRINT 'Sample query suggestions inserted successfully.';
END
ELSE
BEGIN
    PRINT 'Query suggestions already exist.';
END
GO

PRINT 'Database fix completed successfully!';
PRINT 'The QuerySuggestions table and related data have been created.';
