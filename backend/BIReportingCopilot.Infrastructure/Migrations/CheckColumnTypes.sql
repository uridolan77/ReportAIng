-- Check the actual column types in PromptTemplates table
USE [BICopilot]
GO

SELECT 
    c.COLUMN_NAME,
    c.DATA_TYPE,
    c.CHARACTER_MAXIMUM_LENGTH,
    c.NUMERIC_PRECISION,
    c.NUMERIC_SCALE,
    c.IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS c
WHERE c.TABLE_NAME = 'PromptTemplates'
    AND c.COLUMN_NAME IN ('UsageFrequency', 'ImportanceScore', 'BusinessMetadata', 'LastBusinessReviewDate', 'LastUsedDate')
ORDER BY c.COLUMN_NAME;
