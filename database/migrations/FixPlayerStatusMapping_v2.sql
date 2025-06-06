-- Fix Player Status Mapping - Correct approach
-- This addresses the issue where AI generates SQL with 'Suspended' status which doesn't exist in the database

-- First, let's check what we have
SELECT [Name], [Version], [Description], [IsActive], [CreatedDate], 
       LEFT([Content], 200) as ContentPreview
FROM [dbo].[PromptTemplates] 
WHERE [Name] = 'sql_generation';

-- Update the prompt template with explicit status mapping rules
-- We'll add the rules to the business rules section
UPDATE [dbo].[PromptTemplates] 
SET 
    [Content] = CASE 
        WHEN [Content] LIKE '%BUSINESS LOGIC RULES:%{business_rules}%' THEN
            REPLACE([Content], 
                'BUSINESS LOGIC RULES:
{business_rules}',
                'BUSINESS LOGIC RULES:
{business_rules}

CRITICAL PLAYER STATUS MAPPING:
- Player Status field accepts ONLY ''Active'' or ''Blocked'' values
- When user asks for "suspended" players, ALWAYS use Status = ''Blocked''
- NEVER use ''Suspended'', ''Inactive'', or ''Closed'' as status values
- These values do not exist in this database schema
- Example: WHERE Status = ''Blocked'' (for suspended/blocked players)')
        ELSE
            [Content] + '

CRITICAL PLAYER STATUS MAPPING:
- Player Status field accepts ONLY ''Active'' or ''Blocked'' values  
- When user asks for "suspended" players, ALWAYS use Status = ''Blocked''
- NEVER use ''Suspended'', ''Inactive'', or ''Closed'' as status values
- These values do not exist in this database schema
- Example: WHERE Status = ''Blocked'' (for suspended/blocked players)'
    END,
    [Version] = '2.4',
    [Description] = 'Enhanced SQL generation template with correct player status mapping (suspended -> Blocked)',
    [UpdatedDate] = GETUTCDATE(),
    [UpdatedBy] = 'System-StatusFix'
WHERE [Name] = 'sql_generation' AND [IsActive] = 1;

-- Verify the update worked
SELECT [Name], [Version], [Description], [IsActive], [UpdatedDate]
FROM [dbo].[PromptTemplates] 
WHERE [Name] = 'sql_generation';

-- Show a portion of the updated content to verify the rules were added
SELECT [Name], [Version], 
       CASE 
           WHEN [Content] LIKE '%CRITICAL PLAYER STATUS MAPPING%' THEN 'STATUS MAPPING RULES ADDED ✓'
           ELSE 'STATUS MAPPING RULES NOT FOUND ✗'
       END as StatusMappingCheck
FROM [dbo].[PromptTemplates] 
WHERE [Name] = 'sql_generation';

PRINT 'Prompt template updated successfully. AI will now correctly map "suspended" to "Blocked" status.';
