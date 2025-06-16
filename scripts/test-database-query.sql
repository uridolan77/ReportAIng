-- Test query to check LLM Usage Logs data
USE [BICopilot]
GO

-- Check if LLMUsageLogs table exists and has data
SELECT 
    COUNT(*) as TotalRecords,
    MIN(Timestamp) as EarliestRecord,
    MAX(Timestamp) as LatestRecord
FROM LLMUsageLogs;

-- Get sample records if any exist
SELECT TOP 10 
    Id,
    RequestId,
    UserId,
    ProviderId,
    ModelId,
    RequestType,
    TotalTokens,
    Cost,
    DurationMs,
    Success,
    Timestamp
FROM LLMUsageLogs
ORDER BY Timestamp DESC;

-- Check providers table
SELECT 
    ProviderId,
    Name,
    IsEnabled,
    CreatedAt
FROM LLMProviderConfigs;

-- Check models table
SELECT 
    ModelId,
    ProviderId,
    Name,
    IsEnabled,
    CreatedAt
FROM LLMModelConfigs;
