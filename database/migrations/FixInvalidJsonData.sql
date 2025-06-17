-- Migration: Fix invalid JSON data in QuerySuggestions table
-- This fixes JSON parsing errors by cleaning up invalid JSON values

USE [BIReportingCopilot_Dev];
GO

PRINT 'Starting JSON data cleanup for QuerySuggestions table...';

-- Check for invalid JSON data in each JSON column
PRINT 'Checking for invalid JSON data...';

-- Check TargetTables column
DECLARE @InvalidTargetTables INT = 0;
SELECT @InvalidTargetTables = COUNT(*)
FROM QuerySuggestions 
WHERE TargetTables IS NOT NULL 
  AND TargetTables != ''
  AND TargetTables != '[]'
  AND TargetTables != 'null'
  AND NOT (
    TargetTables LIKE '[%]' OR 
    TargetTables LIKE '{%}' OR
    TargetTables = 'null'
  );

PRINT 'Invalid TargetTables entries: ' + CAST(@InvalidTargetTables AS VARCHAR(10));

-- Check RequiredPermissions column
DECLARE @InvalidRequiredPermissions INT = 0;
SELECT @InvalidRequiredPermissions = COUNT(*)
FROM QuerySuggestions 
WHERE RequiredPermissions IS NOT NULL 
  AND RequiredPermissions != ''
  AND RequiredPermissions != '[]'
  AND RequiredPermissions != 'null'
  AND NOT (
    RequiredPermissions LIKE '[%]' OR 
    RequiredPermissions LIKE '{%}' OR
    RequiredPermissions = 'null'
  );

PRINT 'Invalid RequiredPermissions entries: ' + CAST(@InvalidRequiredPermissions AS VARCHAR(10));

-- Check Tags column
DECLARE @InvalidTags INT = 0;
SELECT @InvalidTags = COUNT(*)
FROM QuerySuggestions 
WHERE Tags IS NOT NULL 
  AND Tags != ''
  AND Tags != '[]'
  AND Tags != 'null'
  AND NOT (
    Tags LIKE '[%]' OR 
    Tags LIKE '{%}' OR
    Tags = 'null'
  );

PRINT 'Invalid Tags entries: ' + CAST(@InvalidTags AS VARCHAR(10));

-- Check Keywords column
DECLARE @InvalidKeywords INT = 0;
SELECT @InvalidKeywords = COUNT(*)
FROM QuerySuggestions 
WHERE Keywords IS NOT NULL 
  AND Keywords != ''
  AND Keywords != '[]'
  AND Keywords != 'null'
  AND NOT (
    Keywords LIKE '[%]' OR 
    Keywords LIKE '{%}' OR
    Keywords = 'null'
  );

PRINT 'Invalid Keywords entries: ' + CAST(@InvalidKeywords AS VARCHAR(10));

-- Check RequiredTables column
DECLARE @InvalidRequiredTables INT = 0;
SELECT @InvalidRequiredTables = COUNT(*)
FROM QuerySuggestions 
WHERE RequiredTables IS NOT NULL 
  AND RequiredTables != ''
  AND RequiredTables != '[]'
  AND RequiredTables != 'null'
  AND NOT (
    RequiredTables LIKE '[%]' OR 
    RequiredTables LIKE '{%}' OR
    RequiredTables = 'null'
  );

PRINT 'Invalid RequiredTables entries: ' + CAST(@InvalidRequiredTables AS VARCHAR(10));

-- Show problematic records
PRINT 'Showing problematic records...';

SELECT 
    Id,
    CASE 
        WHEN TargetTables IS NOT NULL AND TargetTables != '' AND TargetTables != '[]' AND TargetTables != 'null' 
             AND NOT (TargetTables LIKE '[%]' OR TargetTables LIKE '{%}' OR TargetTables = 'null')
        THEN 'TargetTables: ' + ISNULL(TargetTables, 'NULL')
        ELSE ''
    END +
    CASE 
        WHEN RequiredPermissions IS NOT NULL AND RequiredPermissions != '' AND RequiredPermissions != '[]' AND RequiredPermissions != 'null'
             AND NOT (RequiredPermissions LIKE '[%]' OR RequiredPermissions LIKE '{%}' OR RequiredPermissions = 'null')
        THEN ' RequiredPermissions: ' + ISNULL(RequiredPermissions, 'NULL')
        ELSE ''
    END +
    CASE 
        WHEN Tags IS NOT NULL AND Tags != '' AND Tags != '[]' AND Tags != 'null'
             AND NOT (Tags LIKE '[%]' OR Tags LIKE '{%}' OR Tags = 'null')
        THEN ' Tags: ' + ISNULL(Tags, 'NULL')
        ELSE ''
    END +
    CASE 
        WHEN Keywords IS NOT NULL AND Keywords != '' AND Keywords != '[]' AND Keywords != 'null'
             AND NOT (Keywords LIKE '[%]' OR Keywords LIKE '{%}' OR Keywords = 'null')
        THEN ' Keywords: ' + ISNULL(Keywords, 'NULL')
        ELSE ''
    END +
    CASE 
        WHEN RequiredTables IS NOT NULL AND RequiredTables != '' AND RequiredTables != '[]' AND RequiredTables != 'null'
             AND NOT (RequiredTables LIKE '[%]' OR RequiredTables LIKE '{%}' OR RequiredTables = 'null')
        THEN ' RequiredTables: ' + ISNULL(RequiredTables, 'NULL')
        ELSE ''
    END AS InvalidJsonData
FROM QuerySuggestions 
WHERE (
    (TargetTables IS NOT NULL AND TargetTables != '' AND TargetTables != '[]' AND TargetTables != 'null' 
     AND NOT (TargetTables LIKE '[%]' OR TargetTables LIKE '{%}' OR TargetTables = 'null'))
    OR
    (RequiredPermissions IS NOT NULL AND RequiredPermissions != '' AND RequiredPermissions != '[]' AND RequiredPermissions != 'null'
     AND NOT (RequiredPermissions LIKE '[%]' OR RequiredPermissions LIKE '{%}' OR RequiredPermissions = 'null'))
    OR
    (Tags IS NOT NULL AND Tags != '' AND Tags != '[]' AND Tags != 'null'
     AND NOT (Tags LIKE '[%]' OR Tags LIKE '{%}' OR Tags = 'null'))
    OR
    (Keywords IS NOT NULL AND Keywords != '' AND Keywords != '[]' AND Keywords != 'null'
     AND NOT (Keywords LIKE '[%]' OR Keywords LIKE '{%}' OR Keywords = 'null'))
    OR
    (RequiredTables IS NOT NULL AND RequiredTables != '' AND RequiredTables != '[]' AND RequiredTables != 'null'
     AND NOT (RequiredTables LIKE '[%]' OR RequiredTables LIKE '{%}' OR RequiredTables = 'null'))
);

-- Fix invalid JSON data
PRINT 'Fixing invalid JSON data...';

-- Fix TargetTables column (handle comma-separated values)
UPDATE QuerySuggestions
SET TargetTables = CASE
    WHEN TargetTables IS NULL OR TargetTables = '' THEN NULL
    WHEN TargetTables = 'null' THEN NULL
    WHEN TargetTables LIKE '[%]' OR TargetTables LIKE '{%}' THEN TargetTables
    WHEN TargetTables LIKE '%,%' THEN
        '["' + REPLACE(REPLACE(REPLACE(TargetTables, '"', ''), '''', ''), ',', '","') + '"]'
    ELSE '["' + REPLACE(REPLACE(TargetTables, '"', ''), '''', '') + '"]'
END
WHERE TargetTables IS NOT NULL
  AND TargetTables != ''
  AND TargetTables != '[]'
  AND TargetTables != 'null'
  AND NOT (
    TargetTables LIKE '[%]' OR
    TargetTables LIKE '{%}' OR
    TargetTables = 'null'
  );

PRINT 'Fixed TargetTables column.';

-- Fix RequiredPermissions column (handle comma-separated values)
UPDATE QuerySuggestions
SET RequiredPermissions = CASE
    WHEN RequiredPermissions IS NULL OR RequiredPermissions = '' THEN NULL
    WHEN RequiredPermissions = 'null' THEN NULL
    WHEN RequiredPermissions LIKE '[%]' OR RequiredPermissions LIKE '{%}' THEN RequiredPermissions
    WHEN RequiredPermissions LIKE '%,%' THEN
        '["' + REPLACE(REPLACE(REPLACE(RequiredPermissions, '"', ''), '''', ''), ',', '","') + '"]'
    ELSE '["' + REPLACE(REPLACE(RequiredPermissions, '"', ''), '''', '') + '"]'
END
WHERE RequiredPermissions IS NOT NULL
  AND RequiredPermissions != ''
  AND RequiredPermissions != '[]'
  AND RequiredPermissions != 'null'
  AND NOT (
    RequiredPermissions LIKE '[%]' OR
    RequiredPermissions LIKE '{%}' OR
    RequiredPermissions = 'null'
  );

PRINT 'Fixed RequiredPermissions column.';

-- Fix Tags column (handle comma-separated values)
UPDATE QuerySuggestions
SET Tags = CASE
    WHEN Tags IS NULL OR Tags = '' THEN NULL
    WHEN Tags = 'null' THEN NULL
    WHEN Tags LIKE '[%]' OR Tags LIKE '{%}' THEN Tags
    WHEN Tags LIKE '%,%' THEN
        '["' + REPLACE(REPLACE(REPLACE(Tags, '"', ''), '''', ''), ',', '","') + '"]'
    ELSE '["' + REPLACE(REPLACE(Tags, '"', ''), '''', '') + '"]'
END
WHERE Tags IS NOT NULL
  AND Tags != ''
  AND Tags != '[]'
  AND Tags != 'null'
  AND NOT (
    Tags LIKE '[%]' OR
    Tags LIKE '{%}' OR
    Tags = 'null'
  );

PRINT 'Fixed Tags column.';

-- Fix Keywords column (handle comma-separated values)
UPDATE QuerySuggestions
SET Keywords = CASE
    WHEN Keywords IS NULL OR Keywords = '' THEN NULL
    WHEN Keywords = 'null' THEN NULL
    WHEN Keywords LIKE '[%]' OR Keywords LIKE '{%}' THEN Keywords
    WHEN Keywords LIKE '%,%' THEN
        '["' + REPLACE(REPLACE(REPLACE(Keywords, '"', ''), '''', ''), ',', '","') + '"]'
    ELSE '["' + REPLACE(REPLACE(Keywords, '"', ''), '''', '') + '"]'
END
WHERE Keywords IS NOT NULL
  AND Keywords != ''
  AND Keywords != '[]'
  AND Keywords != 'null'
  AND NOT (
    Keywords LIKE '[%]' OR
    Keywords LIKE '{%}' OR
    Keywords = 'null'
  );

PRINT 'Fixed Keywords column.';

-- Fix RequiredTables column (handle comma-separated values)
UPDATE QuerySuggestions
SET RequiredTables = CASE
    WHEN RequiredTables IS NULL OR RequiredTables = '' THEN NULL
    WHEN RequiredTables = 'null' THEN NULL
    WHEN RequiredTables LIKE '[%]' OR RequiredTables LIKE '{%}' THEN RequiredTables
    WHEN RequiredTables LIKE '%,%' THEN
        '["' + REPLACE(REPLACE(REPLACE(RequiredTables, '"', ''), '''', ''), ',', '","') + '"]'
    ELSE '["' + REPLACE(REPLACE(RequiredTables, '"', ''), '''', '') + '"]'
END
WHERE RequiredTables IS NOT NULL
  AND RequiredTables != ''
  AND RequiredTables != '[]'
  AND RequiredTables != 'null'
  AND NOT (
    RequiredTables LIKE '[%]' OR
    RequiredTables LIKE '{%}' OR
    RequiredTables = 'null'
  );

PRINT 'Fixed RequiredTables column.';

-- Verify the fixes
PRINT 'Verifying fixes...';

-- Count remaining invalid entries
DECLARE @RemainingInvalid INT = 0;
SELECT @RemainingInvalid = COUNT(*)
FROM QuerySuggestions 
WHERE (
    (TargetTables IS NOT NULL AND TargetTables != '' AND TargetTables != '[]' AND TargetTables != 'null' 
     AND NOT (TargetTables LIKE '[%]' OR TargetTables LIKE '{%}' OR TargetTables = 'null'))
    OR
    (RequiredPermissions IS NOT NULL AND RequiredPermissions != '' AND RequiredPermissions != '[]' AND RequiredPermissions != 'null'
     AND NOT (RequiredPermissions LIKE '[%]' OR RequiredPermissions LIKE '{%}' OR RequiredPermissions = 'null'))
    OR
    (Tags IS NOT NULL AND Tags != '' AND Tags != '[]' AND Tags != 'null'
     AND NOT (Tags LIKE '[%]' OR Tags LIKE '{%}' OR Tags = 'null'))
    OR
    (Keywords IS NOT NULL AND Keywords != '' AND Keywords != '[]' AND Keywords != 'null'
     AND NOT (Keywords LIKE '[%]' OR Keywords LIKE '{%}' OR Keywords = 'null'))
    OR
    (RequiredTables IS NOT NULL AND RequiredTables != '' AND RequiredTables != '[]' AND RequiredTables != 'null'
     AND NOT (RequiredTables LIKE '[%]' OR RequiredTables LIKE '{%}' OR RequiredTables = 'null'))
);

PRINT 'Remaining invalid JSON entries: ' + CAST(@RemainingInvalid AS VARCHAR(10));

IF @RemainingInvalid = 0
BEGIN
    PRINT 'All JSON data has been successfully cleaned up!';
END
ELSE
BEGIN
    PRINT 'WARNING: Some invalid JSON entries still remain. Manual review may be required.';
END

PRINT 'JSON data cleanup completed.';
