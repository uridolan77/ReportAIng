-- =====================================================================================
-- Fix QueryHistory Table Schema - Complete Alignment with Entity Framework Model
-- =====================================================================================

USE [BIReportingCopilot_Dev]
GO

PRINT 'Starting QueryHistory table schema alignment...'

-- Check if QueryHistory table exists
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='QueryHistory' AND xtype='U')
BEGIN
    PRINT 'ERROR: QueryHistory table does not exist!'
    RETURN
END

-- Add missing columns that Entity Framework model expects
PRINT 'Adding missing columns...'

-- Add Classification column if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('QueryHistory') AND name = 'Classification')
BEGIN
    ALTER TABLE [dbo].[QueryHistory] ADD [Classification] nvarchar(100) NULL
    PRINT '✅ Added Classification column'
END

-- Add CreatedAt column if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('QueryHistory') AND name = 'CreatedAt')
BEGIN
    ALTER TABLE [dbo].[QueryHistory] ADD [CreatedAt] datetime2(7) NULL
    PRINT '✅ Added CreatedAt column'
END

-- Add Explanation column if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('QueryHistory') AND name = 'Explanation')
BEGIN
    ALTER TABLE [dbo].[QueryHistory] ADD [Explanation] nvarchar(max) NULL
    PRINT '✅ Added Explanation column'
END

-- Add Metadata column if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('QueryHistory') AND name = 'Metadata')
BEGIN
    ALTER TABLE [dbo].[QueryHistory] ADD [Metadata] nvarchar(max) NULL
    PRINT '✅ Added Metadata column'
END

-- Add OriginalQuery column if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('QueryHistory') AND name = 'OriginalQuery')
BEGIN
    ALTER TABLE [dbo].[QueryHistory] ADD [OriginalQuery] nvarchar(2000) NULL
    PRINT '✅ Added OriginalQuery column'
END

-- Add ResultData column if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('QueryHistory') AND name = 'ResultData')
BEGIN
    ALTER TABLE [dbo].[QueryHistory] ADD [ResultData] nvarchar(max) NULL
    PRINT '✅ Added ResultData column'
END

-- Add Status column if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('QueryHistory') AND name = 'Status')
BEGIN
    ALTER TABLE [dbo].[QueryHistory] ADD [Status] nvarchar(50) NULL
    PRINT '✅ Added Status column'
END

-- Add UpdatedAt column if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('QueryHistory') AND name = 'UpdatedAt')
BEGIN
    ALTER TABLE [dbo].[QueryHistory] ADD [UpdatedAt] datetime2(7) NULL
    PRINT '✅ Added UpdatedAt column'
END

-- Add GeneratedSql column if it doesn't exist (Entity Framework expects this name)
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('QueryHistory') AND name = 'GeneratedSql')
BEGIN
    ALTER TABLE [dbo].[QueryHistory] ADD [GeneratedSql] nvarchar(max) NULL
    PRINT '✅ Added GeneratedSql column'
END

-- Add ExecutedAt column if it doesn't exist (Entity Framework expects this name)
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('QueryHistory') AND name = 'ExecutedAt')
BEGIN
    ALTER TABLE [dbo].[QueryHistory] ADD [ExecutedAt] datetime2(7) NULL
    PRINT '✅ Added ExecutedAt column'
END

-- Add RowCount column if it doesn't exist (Entity Framework expects this name)
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('QueryHistory') AND name = 'RowCount')
BEGIN
    ALTER TABLE [dbo].[QueryHistory] ADD [RowCount] int NULL
    PRINT '✅ Added RowCount column'
END

PRINT 'Populating new columns with data from existing columns...'

-- Populate new columns with data from existing columns
UPDATE [dbo].[QueryHistory] 
SET 
    [OriginalQuery] = COALESCE([OriginalQuery], [NaturalLanguageQuery], ''),
    [GeneratedSql] = COALESCE([GeneratedSql], [GeneratedSQL], ''),
    [ExecutedAt] = COALESCE([ExecutedAt], [QueryTimestamp], GETUTCDATE()),
    [RowCount] = COALESCE([RowCount], [ResultRowCount], 0),
    [CreatedAt] = COALESCE([CreatedAt], [CreatedDate], GETUTCDATE()),
    [UpdatedAt] = COALESCE([UpdatedAt], [UpdatedDate], [LastUpdated], GETUTCDATE()),
    [Status] = COALESCE([Status], CASE WHEN [IsSuccessful] = 1 THEN 'Success' ELSE 'Failed' END),
    [Classification] = COALESCE([Classification], 'General'),
    [Explanation] = COALESCE([Explanation], ''),
    [Metadata] = COALESCE([Metadata], '{}'),
    [ResultData] = COALESCE([ResultData], '')

PRINT 'Data migration completed successfully!'

-- Verify the schema
PRINT ''
PRINT '=== SCHEMA VERIFICATION ==='
SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    IS_NULLABLE,
    CHARACTER_MAXIMUM_LENGTH
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'QueryHistory'
ORDER BY ORDINAL_POSITION

PRINT ''
PRINT '=== DATA VERIFICATION ==='
SELECT 
    COUNT(*) as TotalRows,
    COUNT(CASE WHEN OriginalQuery IS NOT NULL AND OriginalQuery != '' THEN 1 END) as RowsWithOriginalQuery,
    COUNT(CASE WHEN GeneratedSql IS NOT NULL AND GeneratedSql != '' THEN 1 END) as RowsWithGeneratedSql,
    COUNT(CASE WHEN ExecutedAt IS NOT NULL THEN 1 END) as RowsWithExecutedAt,
    COUNT(CASE WHEN Status IS NOT NULL AND Status != '' THEN 1 END) as RowsWithStatus
FROM [dbo].[QueryHistory]

PRINT 'QueryHistory table schema alignment completed successfully! ✅'
