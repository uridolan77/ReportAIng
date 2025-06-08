-- Migration: Fix QueryHistory table schema mismatch
-- This fixes the schema mismatch where Entity Framework expects 'Query' and 'ExecutedAt' columns
-- but the database has 'NaturalLanguageQuery' and 'QueryTimestamp' columns

-- Check if we need to add the Query column (alias for NaturalLanguageQuery)
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[QueryHistory]') AND name = 'Query')
BEGIN
    -- Add Query column as computed column pointing to NaturalLanguageQuery
    ALTER TABLE [dbo].[QueryHistory] 
    ADD [Query] AS [NaturalLanguageQuery];
    
    PRINT 'Added Query computed column to QueryHistory table';
END
ELSE
BEGIN
    PRINT 'Query column already exists in QueryHistory table';
END

-- Check if we need to add the ExecutedAt column (alias for QueryTimestamp)
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[QueryHistory]') AND name = 'ExecutedAt')
BEGIN
    -- Add ExecutedAt column as computed column pointing to QueryTimestamp
    ALTER TABLE [dbo].[QueryHistory] 
    ADD [ExecutedAt] AS [QueryTimestamp];
    
    PRINT 'Added ExecutedAt computed column to QueryHistory table';
END
ELSE
BEGIN
    PRINT 'ExecutedAt column already exists in QueryHistory table';
END

-- Check if we need to add the GeneratedSql column (alias for GeneratedSQL)
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[QueryHistory]') AND name = 'GeneratedSql')
BEGIN
    -- Add GeneratedSql column as computed column pointing to GeneratedSQL
    ALTER TABLE [dbo].[QueryHistory] 
    ADD [GeneratedSql] AS [GeneratedSQL];
    
    PRINT 'Added GeneratedSql computed column to QueryHistory table';
END
ELSE
BEGIN
    PRINT 'GeneratedSql column already exists in QueryHistory table';
END

-- Check if we need to add the RowCount column (alias for ResultRowCount)
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[QueryHistory]') AND name = 'RowCount')
BEGIN
    -- Add RowCount column as computed column pointing to ResultRowCount
    ALTER TABLE [dbo].[QueryHistory] 
    ADD [RowCount] AS [ResultRowCount];
    
    PRINT 'Added RowCount computed column to QueryHistory table';
END
ELSE
BEGIN
    PRINT 'RowCount column already exists in QueryHistory table';
END

-- Add missing columns that don't exist in the original schema
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[QueryHistory]') AND name = 'DatabaseName')
BEGIN
    ALTER TABLE [dbo].[QueryHistory]
    ADD [DatabaseName] NVARCHAR(100) NULL;

    PRINT 'Added DatabaseName column to QueryHistory table';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[QueryHistory]') AND name = 'SchemaName')
BEGIN
    ALTER TABLE [dbo].[QueryHistory]
    ADD [SchemaName] NVARCHAR(100) NULL;

    PRINT 'Added SchemaName column to QueryHistory table';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[QueryHistory]') AND name = 'TablesUsed')
BEGIN
    ALTER TABLE [dbo].[QueryHistory]
    ADD [TablesUsed] NVARCHAR(MAX) NULL;

    PRINT 'Added TablesUsed column to QueryHistory table';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[QueryHistory]') AND name = 'ColumnsUsed')
BEGIN
    ALTER TABLE [dbo].[QueryHistory]
    ADD [ColumnsUsed] NVARCHAR(MAX) NULL;

    PRINT 'Added ColumnsUsed column to QueryHistory table';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[QueryHistory]') AND name = 'QueryType')
BEGIN
    ALTER TABLE [dbo].[QueryHistory]
    ADD [QueryType] NVARCHAR(50) NULL;

    PRINT 'Added QueryType column to QueryHistory table';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[QueryHistory]') AND name = 'FromCache')
BEGIN
    ALTER TABLE [dbo].[QueryHistory]
    ADD [FromCache] BIT NOT NULL DEFAULT 0;

    PRINT 'Added FromCache column to QueryHistory table';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[QueryHistory]') AND name = 'CacheKey')
BEGIN
    ALTER TABLE [dbo].[QueryHistory]
    ADD [CacheKey] NVARCHAR(255) NULL;

    PRINT 'Added CacheKey column to QueryHistory table';
END

-- Add additional missing columns from UnifiedQueryHistoryEntity
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[QueryHistory]') AND name = 'CreatedBy')
BEGIN
    ALTER TABLE [dbo].[QueryHistory]
    ADD [CreatedBy] NVARCHAR(500) NOT NULL DEFAULT 'System';

    PRINT 'Added CreatedBy column to QueryHistory table';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[QueryHistory]') AND name = 'UpdatedBy')
BEGIN
    ALTER TABLE [dbo].[QueryHistory]
    ADD [UpdatedBy] NVARCHAR(500) NULL;

    PRINT 'Added UpdatedBy column to QueryHistory table';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[QueryHistory]') AND name = 'CreatedDate')
BEGIN
    ALTER TABLE [dbo].[QueryHistory]
    ADD [CreatedDate] DATETIME2 NOT NULL DEFAULT GETUTCDATE();

    PRINT 'Added CreatedDate column to QueryHistory table';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[QueryHistory]') AND name = 'LastUpdated')
BEGIN
    ALTER TABLE [dbo].[QueryHistory]
    ADD [LastUpdated] DATETIME2 NOT NULL DEFAULT GETUTCDATE();

    PRINT 'Added LastUpdated column to QueryHistory table';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[QueryHistory]') AND name = 'IsActive')
BEGIN
    ALTER TABLE [dbo].[QueryHistory]
    ADD [IsActive] BIT NOT NULL DEFAULT 1;

    PRINT 'Added IsActive column to QueryHistory table';
END

-- Add indexes for the new computed columns if they don't exist
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[QueryHistory]') AND name = 'IX_QueryHistory_ExecutedAt')
BEGIN
    CREATE NONCLUSTERED INDEX [IX_QueryHistory_ExecutedAt]
        ON [dbo].[QueryHistory] ([ExecutedAt] DESC);
    
    PRINT 'Created index IX_QueryHistory_ExecutedAt';
END

-- Verify the changes
SELECT
    COLUMN_NAME,
    DATA_TYPE,
    IS_NULLABLE,
    CHARACTER_MAXIMUM_LENGTH,
    COLUMNPROPERTY(OBJECT_ID(TABLE_SCHEMA + '.' + TABLE_NAME), COLUMN_NAME, 'IsComputed') as IsComputed
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'QueryHistory'
    AND COLUMN_NAME IN ('Query', 'ExecutedAt', 'GeneratedSql', 'RowCount', 'DatabaseName', 'SchemaName', 'TablesUsed', 'ColumnsUsed', 'QueryType', 'FromCache', 'CacheKey', 'CreatedBy', 'UpdatedBy', 'CreatedDate', 'LastUpdated', 'IsActive')
ORDER BY COLUMN_NAME;

PRINT 'Migration completed successfully. QueryHistory table now has all required columns for UnifiedQueryHistoryEntity.';
