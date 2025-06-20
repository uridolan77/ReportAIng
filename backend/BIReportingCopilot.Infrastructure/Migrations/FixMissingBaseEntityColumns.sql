-- Fix missing BaseEntity columns across all tables that inherit from BaseEntity
-- This script adds BusinessPurpose and RelatedBusinessTerms columns to tables that are missing them

PRINT 'Starting BaseEntity columns fix...';

-- List of tables that inherit from BaseEntity and might be missing columns
DECLARE @Tables TABLE (TableName NVARCHAR(128));
INSERT INTO @Tables VALUES 
    ('BusinessTableInfo'),
    ('BusinessColumnInfo'),
    ('QueryPatterns'),
    ('BusinessGlossary'),
    ('AITuningSettings'),
    ('PromptTemplates'),
    ('PromptLogs'),
    ('SemanticSchemaMapping'),
    ('BusinessDomain'),
    ('QueryCache'),
    ('UserPreferences'),
    ('SystemConfiguration'),
    ('AuditLog'),
    ('Users'),
    ('UserSessions'),
    ('RefreshTokens'),
    ('QueryPerformance'),
    ('SystemMetrics'),
    ('MfaChallenges'),
    ('CostTracking'),
    ('BudgetManagement'),
    ('ResourceUsage'),
    ('PerformanceMetrics'),
    ('CacheStatistics'),
    ('CacheConfigurations'),
    ('ResourceQuotas'),
    ('CostPredictions'),
    ('CostOptimizationRecommendations');

-- Check and add BusinessPurpose column where missing
DECLARE @TableName NVARCHAR(128);
DECLARE @SQL NVARCHAR(MAX);
DECLARE table_cursor CURSOR FOR SELECT TableName FROM @Tables;

OPEN table_cursor;
FETCH NEXT FROM table_cursor INTO @TableName;

WHILE @@FETCH_STATUS = 0
BEGIN
    -- Check if table exists
    IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = @TableName)
    BEGIN
        -- Check if BusinessPurpose column exists
        IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = @TableName AND COLUMN_NAME = 'BusinessPurpose')
        BEGIN
            SET @SQL = 'ALTER TABLE [' + @TableName + '] ADD BusinessPurpose nvarchar(max) NULL';
            EXEC sp_executesql @SQL;
            PRINT '✅ Added BusinessPurpose to ' + @TableName;
        END
        ELSE
        BEGIN
            PRINT '⏭️ BusinessPurpose already exists in ' + @TableName;
        END

        -- Check if RelatedBusinessTerms column exists
        IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = @TableName AND COLUMN_NAME = 'RelatedBusinessTerms')
        BEGIN
            SET @SQL = 'ALTER TABLE [' + @TableName + '] ADD RelatedBusinessTerms nvarchar(max) NULL';
            EXEC sp_executesql @SQL;
            PRINT '✅ Added RelatedBusinessTerms to ' + @TableName;
        END
        ELSE
        BEGIN
            PRINT '⏭️ RelatedBusinessTerms already exists in ' + @TableName;
        END

        -- Check and add other BaseEntity columns if missing
        IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = @TableName AND COLUMN_NAME = 'BusinessFriendlyName')
        BEGIN
            SET @SQL = 'ALTER TABLE [' + @TableName + '] ADD BusinessFriendlyName nvarchar(max) NULL';
            EXEC sp_executesql @SQL;
            PRINT '✅ Added BusinessFriendlyName to ' + @TableName;
        END

        IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = @TableName AND COLUMN_NAME = 'NaturalLanguageDescription')
        BEGIN
            SET @SQL = 'ALTER TABLE [' + @TableName + '] ADD NaturalLanguageDescription nvarchar(max) NULL';
            EXEC sp_executesql @SQL;
            PRINT '✅ Added NaturalLanguageDescription to ' + @TableName;
        END

        IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = @TableName AND COLUMN_NAME = 'BusinessRules')
        BEGIN
            SET @SQL = 'ALTER TABLE [' + @TableName + '] ADD BusinessRules nvarchar(max) NULL';
            EXEC sp_executesql @SQL;
            PRINT '✅ Added BusinessRules to ' + @TableName;
        END

        IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = @TableName AND COLUMN_NAME = 'RelationshipContext')
        BEGIN
            SET @SQL = 'ALTER TABLE [' + @TableName + '] ADD RelationshipContext nvarchar(max) NULL';
            EXEC sp_executesql @SQL;
            PRINT '✅ Added RelationshipContext to ' + @TableName;
        END

        IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = @TableName AND COLUMN_NAME = 'DataGovernanceLevel')
        BEGIN
            SET @SQL = 'ALTER TABLE [' + @TableName + '] ADD DataGovernanceLevel nvarchar(max) NULL';
            EXEC sp_executesql @SQL;
            PRINT '✅ Added DataGovernanceLevel to ' + @TableName;
        END

        IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = @TableName AND COLUMN_NAME = 'LastBusinessReview')
        BEGIN
            SET @SQL = 'ALTER TABLE [' + @TableName + '] ADD LastBusinessReview datetime2 NULL';
            EXEC sp_executesql @SQL;
            PRINT '✅ Added LastBusinessReview to ' + @TableName;
        END

        -- Check for ImportanceScore and UsageFrequency (decimal columns)
        IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = @TableName AND COLUMN_NAME = 'ImportanceScore')
        BEGIN
            SET @SQL = 'ALTER TABLE [' + @TableName + '] ADD ImportanceScore decimal(18,2) NOT NULL DEFAULT 0.5';
            EXEC sp_executesql @SQL;
            PRINT '✅ Added ImportanceScore to ' + @TableName;
        END

        IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = @TableName AND COLUMN_NAME = 'UsageFrequency')
        BEGIN
            SET @SQL = 'ALTER TABLE [' + @TableName + '] ADD UsageFrequency decimal(18,2) NOT NULL DEFAULT 0.0';
            EXEC sp_executesql @SQL;
            PRINT '✅ Added UsageFrequency to ' + @TableName;
        END
    END
    ELSE
    BEGIN
        PRINT '⚠️ Table ' + @TableName + ' does not exist';
    END

    FETCH NEXT FROM table_cursor INTO @TableName;
END

CLOSE table_cursor;
DEALLOCATE table_cursor;

PRINT 'BaseEntity columns fix completed!';

-- Summary report
PRINT '';
PRINT '=== SUMMARY REPORT ===';
SELECT 
    t.TABLE_NAME,
    CASE WHEN c1.COLUMN_NAME IS NOT NULL THEN '✅' ELSE '❌' END as BusinessPurpose,
    CASE WHEN c2.COLUMN_NAME IS NOT NULL THEN '✅' ELSE '❌' END as RelatedBusinessTerms,
    CASE WHEN c3.COLUMN_NAME IS NOT NULL THEN '✅' ELSE '❌' END as BusinessFriendlyName,
    CASE WHEN c4.COLUMN_NAME IS NOT NULL THEN '✅' ELSE '❌' END as NaturalLanguageDescription
FROM INFORMATION_SCHEMA.TABLES t
LEFT JOIN INFORMATION_SCHEMA.COLUMNS c1 ON t.TABLE_NAME = c1.TABLE_NAME AND c1.COLUMN_NAME = 'BusinessPurpose'
LEFT JOIN INFORMATION_SCHEMA.COLUMNS c2 ON t.TABLE_NAME = c2.TABLE_NAME AND c2.COLUMN_NAME = 'RelatedBusinessTerms'
LEFT JOIN INFORMATION_SCHEMA.COLUMNS c3 ON t.TABLE_NAME = c3.TABLE_NAME AND c3.COLUMN_NAME = 'BusinessFriendlyName'
LEFT JOIN INFORMATION_SCHEMA.COLUMNS c4 ON t.TABLE_NAME = c4.TABLE_NAME AND c4.COLUMN_NAME = 'NaturalLanguageDescription'
WHERE t.TABLE_NAME IN (
    'BusinessTableInfo', 'BusinessColumnInfo', 'QueryPatterns', 'BusinessGlossary', 'AITuningSettings',
    'PromptTemplates', 'PromptLogs', 'SemanticSchemaMapping', 'BusinessDomain', 'QueryCache',
    'UserPreferences', 'SystemConfiguration', 'AuditLog', 'Users', 'UserSessions', 'RefreshTokens',
    'QueryPerformance', 'SystemMetrics', 'MfaChallenges', 'SchemaMetadata'
)
ORDER BY t.TABLE_NAME;
