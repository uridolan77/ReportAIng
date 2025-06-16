-- Insert sample LLM usage data for testing
USE [BICopilot]
GO

-- First, ensure we have some providers
IF NOT EXISTS (SELECT 1 FROM LLMProviderConfigs WHERE ProviderId = 'openai')
BEGIN
    INSERT INTO LLMProviderConfigs (ProviderId, Name, Type, BaseUrl, IsEnabled, CreatedAt, UpdatedAt, Settings)
    VALUES ('openai', 'OpenAI', 'OpenAI', 'https://api.openai.com/v1', 1, GETUTCDATE(), GETUTCDATE(), '{}');
END

IF NOT EXISTS (SELECT 1 FROM LLMProviderConfigs WHERE ProviderId = 'azure-openai')
BEGIN
    INSERT INTO LLMProviderConfigs (ProviderId, Name, Type, BaseUrl, IsEnabled, CreatedAt, UpdatedAt, Settings)
    VALUES ('azure-openai', 'Azure OpenAI', 'AzureOpenAI', 'https://your-resource.openai.azure.com', 1, GETUTCDATE(), GETUTCDATE(), '{}');
END

-- Ensure we have some models
IF NOT EXISTS (SELECT 1 FROM LLMModelConfigs WHERE ModelId = 'gpt-4')
BEGIN
    INSERT INTO LLMModelConfigs (ModelId, ProviderId, Name, Type, MaxTokens, Temperature, IsEnabled, CreatedAt, UpdatedAt, Settings)
    VALUES ('gpt-4', 'openai', 'GPT-4', 'chat', 4096, 0.7, 1, GETUTCDATE(), GETUTCDATE(), '{}');
END

IF NOT EXISTS (SELECT 1 FROM LLMModelConfigs WHERE ModelId = 'gpt-35-turbo')
BEGIN
    INSERT INTO LLMModelConfigs (ModelId, ProviderId, Name, Type, MaxTokens, Temperature, IsEnabled, CreatedAt, UpdatedAt, Settings)
    VALUES ('gpt-35-turbo', 'azure-openai', 'GPT-3.5 Turbo', 'chat', 4096, 0.7, 1, GETUTCDATE(), GETUTCDATE(), '{}');
END

-- Insert sample usage logs
INSERT INTO LLMUsageLogs (
    RequestId, UserId, ProviderId, ModelId, RequestType, 
    RequestText, ResponseText, InputTokens, OutputTokens, TotalTokens,
    Cost, DurationMs, Success, Timestamp, ErrorMessage, Metadata
) VALUES 
-- Recent data (last few hours)
(NEWID(), 'test-user-1', 'openai', 'gpt-4', 'SQL', 
 'Generate SQL for sales report', 'SELECT * FROM sales WHERE date >= ''2024-01-01''', 
 50, 30, 80, 0.002, 1500, 1, DATEADD(HOUR, -2, GETUTCDATE()), NULL, '{"useCase":"SQL"}'),

(NEWID(), 'test-user-1', 'openai', 'gpt-4', 'Insights', 
 'Analyze customer trends', 'Customer acquisition has increased by 15% this quarter', 
 75, 45, 120, 0.003, 2200, 1, DATEADD(HOUR, -1, GETUTCDATE()), NULL, '{"useCase":"Insights"}'),

(NEWID(), 'test-user-2', 'azure-openai', 'gpt-35-turbo', 'SQL', 
 'Create query for revenue analysis', 'SELECT SUM(revenue) FROM transactions GROUP BY month', 
 40, 25, 65, 0.001, 1200, 1, DATEADD(MINUTE, -30, GETUTCDATE()), NULL, '{"useCase":"SQL"}'),

-- Data from yesterday
(NEWID(), 'test-user-1', 'openai', 'gpt-4', 'SQL', 
 'Generate customer report query', 'SELECT c.*, COUNT(o.id) as order_count FROM customers c LEFT JOIN orders o ON c.id = o.customer_id GROUP BY c.id', 
 60, 40, 100, 0.0025, 1800, 1, DATEADD(DAY, -1, GETUTCDATE()), NULL, '{"useCase":"SQL"}'),

(NEWID(), 'test-user-3', 'azure-openai', 'gpt-35-turbo', 'Insights', 
 'Summarize quarterly performance', 'Q3 shows strong growth in all key metrics with 20% revenue increase', 
 80, 50, 130, 0.002, 2500, 1, DATEADD(DAY, -1, GETUTCDATE()), NULL, '{"useCase":"Insights"}'),

-- Data from last week
(NEWID(), 'test-user-2', 'openai', 'gpt-4', 'SQL', 
 'Complex analytics query', 'SELECT DATE_TRUNC(''month'', created_at) as month, COUNT(*) as total FROM users WHERE status = ''active'' GROUP BY month ORDER BY month', 
 90, 60, 150, 0.004, 3000, 1, DATEADD(DAY, -7, GETUTCDATE()), NULL, '{"useCase":"SQL"}'),

(NEWID(), 'test-user-1', 'azure-openai', 'gpt-35-turbo', 'General', 
 'Explain database optimization', 'Database optimization involves indexing, query optimization, and proper schema design...', 
 100, 200, 300, 0.005, 4000, 1, DATEADD(DAY, -5, GETUTCDATE()), NULL, '{"useCase":"General"}'),

-- Some failed requests for testing error handling
(NEWID(), 'test-user-2', 'openai', 'gpt-4', 'SQL', 
 'Invalid request that fails', '', 
 30, 0, 30, 0.0, 500, 0, DATEADD(DAY, -3, GETUTCDATE()), 'Rate limit exceeded', '{"useCase":"SQL"}'),

-- Data from 2 weeks ago
(NEWID(), 'test-user-3', 'openai', 'gpt-4', 'Insights', 
 'Market analysis request', 'The market shows positive trends with increasing demand in key sectors', 
 70, 80, 150, 0.003, 2800, 1, DATEADD(DAY, -14, GETUTCDATE()), NULL, '{"useCase":"Insights"}'),

(NEWID(), 'test-user-1', 'azure-openai', 'gpt-35-turbo', 'SQL', 
 'Performance metrics query', 'SELECT AVG(response_time), MAX(response_time), COUNT(*) FROM api_logs WHERE date >= CURRENT_DATE - INTERVAL ''7 days''', 
 55, 45, 100, 0.002, 1600, 1, DATEADD(DAY, -10, GETUTCDATE()), NULL, '{"useCase":"SQL"}');

-- Verify the data was inserted
SELECT 
    COUNT(*) as TotalRecords,
    MIN(Timestamp) as EarliestRecord,
    MAX(Timestamp) as LatestRecord,
    SUM(CASE WHEN Success = 1 THEN 1 ELSE 0 END) as SuccessfulRequests,
    SUM(CASE WHEN Success = 0 THEN 1 ELSE 0 END) as FailedRequests,
    SUM(Cost) as TotalCost,
    SUM(TotalTokens) as TotalTokens
FROM LLMUsageLogs;

PRINT 'Sample LLM usage data inserted successfully!';
