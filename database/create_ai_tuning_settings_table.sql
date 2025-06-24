-- Create AITuningSettings table for BIReportingCopilot_Dev database
-- This script creates the missing AITuningSettings table that was causing the application to hang
-- Execute this script in your BIReportingCopilot_Dev database

USE [BIReportingCopilot_Dev]
GO

-- Check if table already exists
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'AITuningSettings')
BEGIN
    PRINT 'Creating AITuningSettings table...'
    
    -- Create the AITuningSettings table
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

    -- Create unique index on SettingKey
    CREATE UNIQUE NONCLUSTERED INDEX [IX_AITuningSettings_SettingKey] 
    ON [dbo].[AITuningSettings] ([SettingKey] ASC);

    -- Create index on Category for performance
    CREATE NONCLUSTERED INDEX [IX_AITuningSettings_Category] 
    ON [dbo].[AITuningSettings] ([Category] ASC);

    PRINT 'AITuningSettings table created successfully!'
END
ELSE
BEGIN
    PRINT 'AITuningSettings table already exists, skipping creation.'
END
GO

-- Insert essential default settings
PRINT 'Inserting default AI tuning settings...'

-- Insert settings only if they don't already exist
IF NOT EXISTS (SELECT 1 FROM [dbo].[AITuningSettings] WHERE [SettingKey] = 'EnableQueryCaching')
BEGIN
    INSERT INTO [dbo].[AITuningSettings]
        ([SettingKey], [SettingValue], [Description], [Category], [DataType], [CreatedBy])
    VALUES
        ('EnableQueryCaching', 'true', 'Enable/disable query result caching for performance optimization', 'Performance', 'boolean', 'System')
END

IF NOT EXISTS (SELECT 1 FROM [dbo].[AITuningSettings] WHERE [SettingKey] = 'EnableEnhancedSemanticCache')
BEGIN
    INSERT INTO [dbo].[AITuningSettings]
        ([SettingKey], [SettingValue], [Description], [Category], [DataType], [CreatedBy])
    VALUES
        ('EnableEnhancedSemanticCache', 'false', 'Enable/disable enhanced semantic caching for AI queries', 'Performance', 'boolean', 'System')
END

IF NOT EXISTS (SELECT 1 FROM [dbo].[AITuningSettings] WHERE [SettingKey] = 'DefaultAIProvider')
BEGIN
    INSERT INTO [dbo].[AITuningSettings]
        ([SettingKey], [SettingValue], [Description], [Category], [DataType], [CreatedBy])
    VALUES
        ('DefaultAIProvider', 'OpenAI', 'Default AI provider for query processing', 'AI', 'string', 'System')
END

IF NOT EXISTS (SELECT 1 FROM [dbo].[AITuningSettings] WHERE [SettingKey] = 'DefaultModel')
BEGIN
    INSERT INTO [dbo].[AITuningSettings]
        ([SettingKey], [SettingValue], [Description], [Category], [DataType], [CreatedBy])
    VALUES
        ('DefaultModel', 'gpt-4', 'Default AI model for query processing', 'AI', 'string', 'System')
END

IF NOT EXISTS (SELECT 1 FROM [dbo].[AITuningSettings] WHERE [SettingKey] = 'MaxTokens')
BEGIN
    INSERT INTO [dbo].[AITuningSettings]
        ([SettingKey], [SettingValue], [Description], [Category], [DataType], [CreatedBy])
    VALUES
        ('MaxTokens', '4000', 'Maximum tokens allowed for AI model responses', 'AI', 'integer', 'System')
END

IF NOT EXISTS (SELECT 1 FROM [dbo].[AITuningSettings] WHERE [SettingKey] = 'Temperature')
BEGIN
    INSERT INTO [dbo].[AITuningSettings]
        ([SettingKey], [SettingValue], [Description], [Category], [DataType], [CreatedBy])
    VALUES
        ('Temperature', '0.1', 'AI model temperature setting for response creativity', 'AI', 'decimal', 'System')
END

IF NOT EXISTS (SELECT 1 FROM [dbo].[AITuningSettings] WHERE [SettingKey] = 'EnableAIAnalytics')
BEGIN
    INSERT INTO [dbo].[AITuningSettings]
        ([SettingKey], [SettingValue], [Description], [Category], [DataType], [CreatedBy])
    VALUES
        ('EnableAIAnalytics', 'true', 'Enable/disable AI analytics and performance tracking', 'Analytics', 'boolean', 'System')
END

IF NOT EXISTS (SELECT 1 FROM [dbo].[AITuningSettings] WHERE [SettingKey] = 'CacheExpiryHours')
BEGIN
    INSERT INTO [dbo].[AITuningSettings]
        ([SettingKey], [SettingValue], [Description], [Category], [DataType], [CreatedBy])
    VALUES
        ('CacheExpiryHours', '24', 'Number of hours before cached results expire', 'Performance', 'integer', 'System')
END

IF NOT EXISTS (SELECT 1 FROM [dbo].[AITuningSettings] WHERE [SettingKey] = 'EnablePromptLogging')
BEGIN
    INSERT INTO [dbo].[AITuningSettings]
        ([SettingKey], [SettingValue], [Description], [Category], [DataType], [CreatedBy])
    VALUES
        ('EnablePromptLogging', 'true', 'Enable/disable detailed prompt logging for debugging', 'Debugging', 'boolean', 'System')
END

IF NOT EXISTS (SELECT 1 FROM [dbo].[AITuningSettings] WHERE [SettingKey] = 'DefaultQueryTimeout')
BEGIN
    INSERT INTO [dbo].[AITuningSettings]
        ([SettingKey], [SettingValue], [Description], [Category], [DataType], [CreatedBy])
    VALUES
        ('DefaultQueryTimeout', '30', 'Default timeout for SQL queries in seconds', 'Performance', 'integer', 'System')
END

IF NOT EXISTS (SELECT 1 FROM [dbo].[AITuningSettings] WHERE [SettingKey] = 'MaxPromptLength')
BEGIN
    INSERT INTO [dbo].[AITuningSettings]
        ([SettingKey], [SettingValue], [Description], [Category], [DataType], [CreatedBy])
    VALUES
        ('MaxPromptLength', '50000', 'Maximum allowed prompt length in characters', 'AI', 'integer', 'System')
END

PRINT 'Default AI tuning settings inserted successfully!'

-- Verify the settings were created
PRINT 'Verifying AITuningSettings table and data...'
SELECT 
    COUNT(*) as TotalSettings,
    COUNT(CASE WHEN IsActive = 1 THEN 1 END) as ActiveSettings
FROM [dbo].[AITuningSettings]

-- Display all settings
SELECT 
    [SettingKey], 
    [SettingValue], 
    [Description], 
    [Category], 
    [DataType],
    [IsActive],
    [CreatedDate]
FROM [dbo].[AITuningSettings]
ORDER BY [Category], [SettingKey]

PRINT '✅ AITuningSettings table setup completed successfully!'
PRINT 'You can now restart the backend application - the hanging issue should be resolved.'
