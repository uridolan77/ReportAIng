-- Test script to check LLMProviderConfigs table in BIReportingCopilot_Dev
USE [BIReportingCopilot_Dev]
GO

PRINT '=== TESTING LLMProviderConfigs TABLE ===';
PRINT '';

-- Check if table exists
IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'LLMProviderConfigs')
BEGIN
    PRINT '✅ LLMProviderConfigs table EXISTS';
    
    -- Check table structure
    PRINT '';
    PRINT 'Table Structure:';
    SELECT 
        COLUMN_NAME,
        DATA_TYPE,
        IS_NULLABLE,
        COLUMN_DEFAULT
    FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'LLMProviderConfigs'
    ORDER BY ORDINAL_POSITION;
    
    -- Check data count
    DECLARE @RecordCount INT;
    SELECT @RecordCount = COUNT(*) FROM LLMProviderConfigs;
    PRINT '';
    PRINT 'Record Count: ' + CAST(@RecordCount AS VARCHAR(10));
    
    -- Show all records
    IF @RecordCount > 0
    BEGIN
        PRINT '';
        PRINT 'All Records:';
        SELECT 
            ProviderId,
            Name,
            Type,
            IsEnabled,
            IsDefault,
            CreatedAt,
            UpdatedAt
        FROM LLMProviderConfigs
        ORDER BY Name;
    END
    ELSE
    BEGIN
        PRINT '';
        PRINT '⚠️  Table is EMPTY - No LLM provider configurations found';
        PRINT '';
        PRINT 'Inserting sample OpenAI provider...';
        
        INSERT INTO LLMProviderConfigs (
            ProviderId, Name, Type, ApiKey, Endpoint, Organization, 
            IsEnabled, IsDefault, Settings, CreatedAt, UpdatedAt
        ) VALUES (
            'openai-default', 
            'OpenAI Default', 
            'OpenAI', 
            NULL,
            'https://api.openai.com/v1', 
            NULL,
            1, -- Enabled
            1, -- Set as default
            '{}',
            GETUTCDATE(), 
            GETUTCDATE()
        );
        
        PRINT '✅ Sample OpenAI provider inserted';
        
        -- Show the inserted record
        SELECT 
            ProviderId,
            Name,
            Type,
            IsEnabled,
            IsDefault,
            CreatedAt
        FROM LLMProviderConfigs
        WHERE ProviderId = 'openai-default';
    END
END
ELSE
BEGIN
    PRINT '❌ LLMProviderConfigs table does NOT exist';
    PRINT '';
    PRINT 'Available tables in database:';
    SELECT TABLE_NAME 
    FROM INFORMATION_SCHEMA.TABLES 
    WHERE TABLE_TYPE = 'BASE TABLE'
    ORDER BY TABLE_NAME;
END

PRINT '';
PRINT '=== TEST COMPLETE ===';
