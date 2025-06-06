-- Fix Player Status Mapping - Update prompt template to correctly map "suspended" to "Blocked"
-- This addresses the issue where AI generates SQL with 'Suspended' status which doesn't exist in the database

-- Update the prompt template to include correct status mapping rules
UPDATE [dbo].[PromptTemplates] 
SET 
    [Content] = REPLACE(
        REPLACE(
            [Content], 
            'BUSINESS LOGIC RULES:
{business_rules}',
            'BUSINESS LOGIC RULES:
{business_rules}

CRITICAL STATUS MAPPING RULES:
- Player Status field ONLY accepts ''Active'' or ''Blocked'' values
- When user asks for "suspended" players, ALWAYS use Status = ''Blocked''
- NEVER use ''Suspended'', ''Inactive'', or ''Closed'' as status values
- These values do not exist in this database schema'
        ),
        'Status values are case-sensitive strings, not numbers',
        'Status values are case-sensitive strings: ONLY ''Active'' or ''Blocked'' are valid
- CRITICAL: Map "suspended" queries to Status = ''Blocked''
- CRITICAL: Never use ''Suspended'' as a status value'
    ),
    [Version] = '2.3',
    [Description] = 'Enhanced SQL generation template with correct player status mapping (suspended -> Blocked)',
    [CreatedDate] = GETUTCDATE()
WHERE [Name] = 'sql_generation';

-- Verify the update
SELECT [Name], [Version], [Description], [IsActive], [CreatedDate]
FROM [dbo].[PromptTemplates] 
WHERE [Name] = 'sql_generation';

PRINT 'Prompt template updated successfully. AI will now correctly map "suspended" to "Blocked" status.';
