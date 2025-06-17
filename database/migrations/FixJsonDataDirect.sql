-- Direct fix for JSON data in QuerySuggestions table
-- This script directly converts comma-separated values to JSON arrays

USE [BIReportingCopilot_Dev];
GO

PRINT 'Starting direct JSON data fix for QuerySuggestions table...';

-- Show current problematic data
PRINT 'Current problematic data:';
SELECT Id, Tags, Keywords 
FROM QuerySuggestions 
WHERE (Tags IS NOT NULL AND Tags NOT LIKE '[%]' AND Tags != '' AND Tags != 'null')
   OR (Keywords IS NOT NULL AND Keywords NOT LIKE '[%]' AND Keywords != '' AND Keywords != 'null');

-- Fix Tags column - convert comma-separated to JSON array
PRINT 'Fixing Tags column...';
UPDATE QuerySuggestions 
SET Tags = '["' + REPLACE(Tags, ',', '","') + '"]'
WHERE Tags IS NOT NULL 
  AND Tags != '' 
  AND Tags != 'null'
  AND Tags NOT LIKE '[%]'
  AND Tags LIKE '%,%';

-- Fix single-value Tags (no commas)
UPDATE QuerySuggestions 
SET Tags = '["' + Tags + '"]'
WHERE Tags IS NOT NULL 
  AND Tags != '' 
  AND Tags != 'null'
  AND Tags NOT LIKE '[%]'
  AND Tags NOT LIKE '%,%';

PRINT 'Tags column fixed.';

-- Fix Keywords column - convert comma-separated to JSON array
PRINT 'Fixing Keywords column...';
UPDATE QuerySuggestions 
SET Keywords = '["' + REPLACE(Keywords, ',', '","') + '"]'
WHERE Keywords IS NOT NULL 
  AND Keywords != '' 
  AND Keywords != 'null'
  AND Keywords NOT LIKE '[%]'
  AND Keywords LIKE '%,%';

-- Fix single-value Keywords (no commas)
UPDATE QuerySuggestions 
SET Keywords = '["' + Keywords + '"]'
WHERE Keywords IS NOT NULL 
  AND Keywords != '' 
  AND Keywords != 'null'
  AND Keywords NOT LIKE '[%]'
  AND Keywords NOT LIKE '%,%';

PRINT 'Keywords column fixed.';

-- Show results after fix
PRINT 'Data after fix:';
SELECT Id, Tags, Keywords 
FROM QuerySuggestions 
WHERE Id IN (1,2,3,4,5,6,7,8,9,10,11,12);

-- Verify no invalid JSON remains
PRINT 'Verifying fix...';
DECLARE @InvalidCount INT = 0;
SELECT @InvalidCount = COUNT(*)
FROM QuerySuggestions 
WHERE (Tags IS NOT NULL AND Tags != '' AND Tags != 'null' AND Tags NOT LIKE '[%]')
   OR (Keywords IS NOT NULL AND Keywords != '' AND Keywords != 'null' AND Keywords NOT LIKE '[%]');

PRINT 'Remaining invalid JSON entries: ' + CAST(@InvalidCount AS VARCHAR(10));

IF @InvalidCount = 0
BEGIN
    PRINT 'SUCCESS: All JSON data has been fixed!';
END
ELSE
BEGIN
    PRINT 'WARNING: Some invalid entries still remain.';
    
    -- Show remaining problematic records
    SELECT Id, Tags, Keywords 
    FROM QuerySuggestions 
    WHERE (Tags IS NOT NULL AND Tags != '' AND Tags != 'null' AND Tags NOT LIKE '[%]')
       OR (Keywords IS NOT NULL AND Keywords != '' AND Keywords != 'null' AND Keywords NOT LIKE '[%]');
END

PRINT 'Direct JSON fix completed.';
