-- Clear cache and test status mapping
-- This script clears the cache and verifies the status mapping rules are working

-- Clear all cached queries related to status/suspended
DELETE FROM [dbo].[CacheEntries] 
WHERE [Key] LIKE '%status%' OR [Key] LIKE '%suspended%' OR [Key] LIKE '%blocked%';

-- Clear all query caches to force fresh prompt generation
DELETE FROM [dbo].[CacheEntries] 
WHERE [Key] LIKE 'query:%';

-- Show current prompt template version
SELECT [Name], [Version], [Description], [IsActive], [CreatedDate],
       CASE 
           WHEN [Content] LIKE '%CRITICAL: When user asks for "suspended" players, use ''Blocked'' status instead%' THEN 'STATUS MAPPING FOUND ✓'
           ELSE 'STATUS MAPPING NOT FOUND ✗'
       END as StatusMappingCheck
FROM [dbo].[PromptTemplates] 
WHERE [Name] = 'sql_generation' AND [IsActive] = 1;

-- Show recent query history to see if suspended queries are still using wrong status
SELECT TOP 5 
    [NaturalLanguageQuery],
    [GeneratedSQL],
    [QueryTimestamp],
    CASE 
        WHEN [GeneratedSQL] LIKE '%Status = ''Suspended''%' THEN 'USING WRONG STATUS ✗'
        WHEN [GeneratedSQL] LIKE '%Status = ''Blocked''%' THEN 'USING CORRECT STATUS ✓'
        ELSE 'NO STATUS FILTER'
    END as StatusCheck
FROM [dbo].[QueryHistory] 
WHERE [NaturalLanguageQuery] LIKE '%suspended%'
ORDER BY [QueryTimestamp] DESC;

PRINT 'Cache cleared. Next query with "suspended" should use Status = ''Blocked''';
PRINT 'The business rules in PromptService.cs should now take effect on the next query.';
