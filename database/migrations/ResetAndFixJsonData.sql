-- Reset and fix corrupted JSON data in QuerySuggestions table
-- This script completely resets the corrupted JSON data and rebuilds it properly

USE [BIReportingCopilot_Dev];
GO

PRINT 'Starting JSON data reset and fix for QuerySuggestions table...';

-- Show current corrupted data
PRINT 'Current corrupted data (first 3 records):';
SELECT TOP 3 Id, Tags, Keywords FROM QuerySuggestions WHERE Id IN (1,2,3);

-- Reset all JSON columns to proper values based on the original data we know
PRINT 'Resetting JSON data to proper values...';

-- Reset record 1
UPDATE QuerySuggestions 
SET Tags = '["users","basic","list"]',
    Keywords = '["users","all","list","show"]'
WHERE Id = 1;

-- Reset record 2
UPDATE QuerySuggestions 
SET Tags = '["count","basic","total"]',
    Keywords = '["count","total","records","number"]'
WHERE Id = 2;

-- Reset record 3
UPDATE QuerySuggestions 
SET Tags = '["activity","recent","basic"]',
    Keywords = '["recent","activity","log","latest"]'
WHERE Id = 3;

-- Reset record 4
UPDATE QuerySuggestions 
SET Tags = '["analytics","monthly","users"]',
    Keywords = '["monthly","activity","users","trend"]'
WHERE Id = 4;

-- Reset record 5
UPDATE QuerySuggestions 
SET Tags = '["analytics","top","users"]',
    Keywords = '["top","active","users","ranking"]'
WHERE Id = 5;

-- Reset record 6
UPDATE QuerySuggestions 
SET Tags = '["analytics","daily","queries"]',
    Keywords = '["daily","query","trends","statistics"]'
WHERE Id = 6;

-- Reset record 7
UPDATE QuerySuggestions 
SET Tags = '["performance","slow","queries"]',
    Keywords = '["slow","queries","performance","analysis"]'
WHERE Id = 7;

-- Reset record 8
UPDATE QuerySuggestions 
SET Tags = '["performance","database","size"]',
    Keywords = '["database","size","tables","space"]'
WHERE Id = 8;

-- Reset record 9
UPDATE QuerySuggestions 
SET Tags = '["bi","engagement","metrics"]',
    Keywords = '["engagement","metrics","users","sessions"]'
WHERE Id = 9;

-- Reset record 10
UPDATE QuerySuggestions 
SET Tags = '["bi","revenue","monthly"]',
    Keywords = '["revenue","monthly","trends","financial"]'
WHERE Id = 10;

-- Reset record 11
UPDATE QuerySuggestions 
SET Tags = '["exploration","quality","data"]',
    Keywords = '["data","quality","check","validation"]'
WHERE Id = 11;

-- Reset record 12
UPDATE QuerySuggestions 
SET Tags = '["exploration","schema","structure"]',
    Keywords = '["schema","structure","tables","columns"]'
WHERE Id = 12;

PRINT 'JSON data reset completed.';

-- Show fixed data
PRINT 'Data after reset:';
SELECT Id, Tags, Keywords FROM QuerySuggestions WHERE Id IN (1,2,3,4,5,6,7,8,9,10,11,12);

-- Verify the fix
PRINT 'Verifying JSON validity...';

-- Test if the JSON is valid by checking if it starts with [ and ends with ]
DECLARE @ValidJsonCount INT = 0;
SELECT @ValidJsonCount = COUNT(*)
FROM QuerySuggestions 
WHERE Id BETWEEN 1 AND 12
  AND Tags LIKE '[%]'
  AND Tags LIKE '%]'
  AND Keywords LIKE '[%]'
  AND Keywords LIKE '%]';

PRINT 'Records with valid JSON format: ' + CAST(@ValidJsonCount AS VARCHAR(10)) + ' out of 12';

IF @ValidJsonCount = 12
BEGIN
    PRINT 'SUCCESS: All JSON data has been properly reset and fixed!';
END
ELSE
BEGIN
    PRINT 'WARNING: Some records may still have issues.';
END

-- Final verification - check for any remaining problematic JSON
DECLARE @ProblematicCount INT = 0;
SELECT @ProblematicCount = COUNT(*)
FROM QuerySuggestions 
WHERE Id BETWEEN 1 AND 12
  AND (
    Tags LIKE '%[%[%' OR 
    Tags LIKE '%]%]%' OR
    Tags LIKE '%"""%' OR
    Keywords LIKE '%[%[%' OR 
    Keywords LIKE '%]%]%' OR
    Keywords LIKE '%"""%'
  );

PRINT 'Records with nested/corrupted JSON: ' + CAST(@ProblematicCount AS VARCHAR(10));

PRINT 'JSON data reset and fix completed.';
