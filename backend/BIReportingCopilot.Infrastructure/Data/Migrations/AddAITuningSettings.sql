-- Add AI Tuning Settings for Admin Control
-- Execute this in your BIReportingCopilot_Dev database

USE [BIReportingCopilot_Dev]
GO

-- Insert AI tuning settings if they don't already exist
IF NOT EXISTS (SELECT 1 FROM [dbo].[AITuningSettings] WHERE [SettingKey] = 'EnableQueryCaching')
BEGIN
    INSERT INTO [dbo].[AITuningSettings]
        ([SettingKey], [SettingValue], [Description], [Category], [DataType], [CreatedBy])
    VALUES
        ('EnableQueryCaching', 'true', 'Enable/disable query result caching for testing purposes', 'Performance', 'boolean', 'System')
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

PRINT 'AI Tuning Settings added successfully!'

-- Verify the settings were inserted
SELECT 
    [SettingKey], 
    [SettingValue], 
    [Description], 
    [Category], 
    [DataType],
    [CreatedDate]
FROM [dbo].[AITuningSettings]
ORDER BY [Category], [SettingKey]
