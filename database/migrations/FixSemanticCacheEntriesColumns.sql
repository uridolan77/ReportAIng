-- Migration: Fix SemanticCacheEntries table schema mismatch
-- This fixes the schema mismatch where Entity Framework expects base entity columns
-- that don't exist in the database table, and fixes the ID column type mismatch

-- First, fix the ID column type mismatch (Int32 -> Int64)
-- Check if we need to change the ID column from int to bigint
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[SemanticCacheEntries]') AND name = 'Id' AND system_type_id = 56) -- 56 = int
BEGIN
    PRINT 'Converting Id column from int to bigint...';

    -- Drop any foreign key constraints that reference this table first
    DECLARE @sql NVARCHAR(MAX) = '';
    SELECT @sql = @sql + 'ALTER TABLE [' + OBJECT_SCHEMA_NAME(parent_object_id) + '].[' + OBJECT_NAME(parent_object_id) + '] DROP CONSTRAINT [' + name + '];' + CHAR(13)
    FROM sys.foreign_keys
    WHERE referenced_object_id = OBJECT_ID(N'[dbo].[SemanticCacheEntries]');

    IF @sql != ''
    BEGIN
        PRINT 'Dropping foreign key constraints...';
        EXEC sp_executesql @sql;
    END

    -- Change the ID column type
    ALTER TABLE [dbo].[SemanticCacheEntries]
    ALTER COLUMN [Id] BIGINT NOT NULL;

    PRINT 'Successfully converted Id column to bigint';
END
ELSE
BEGIN
    PRINT 'Id column is already bigint or does not exist';
END

-- Add missing base entity columns to SemanticCacheEntries table
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[SemanticCacheEntries]') AND name = 'CreatedBy')
BEGIN
    ALTER TABLE [dbo].[SemanticCacheEntries] 
    ADD [CreatedBy] NVARCHAR(500) NOT NULL DEFAULT 'System';
    
    PRINT 'Added CreatedBy column to SemanticCacheEntries table';
END
ELSE
BEGIN
    PRINT 'CreatedBy column already exists in SemanticCacheEntries table';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[SemanticCacheEntries]') AND name = 'UpdatedBy')
BEGIN
    ALTER TABLE [dbo].[SemanticCacheEntries] 
    ADD [UpdatedBy] NVARCHAR(500) NULL;
    
    PRINT 'Added UpdatedBy column to SemanticCacheEntries table';
END
ELSE
BEGIN
    PRINT 'UpdatedBy column already exists in SemanticCacheEntries table';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[SemanticCacheEntries]') AND name = 'CreatedDate')
BEGIN
    ALTER TABLE [dbo].[SemanticCacheEntries] 
    ADD [CreatedDate] DATETIME2 NOT NULL DEFAULT GETUTCDATE();
    
    PRINT 'Added CreatedDate column to SemanticCacheEntries table';
END
ELSE
BEGIN
    PRINT 'CreatedDate column already exists in SemanticCacheEntries table';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[SemanticCacheEntries]') AND name = 'LastUpdated')
BEGIN
    ALTER TABLE [dbo].[SemanticCacheEntries] 
    ADD [LastUpdated] DATETIME2 NOT NULL DEFAULT GETUTCDATE();
    
    PRINT 'Added LastUpdated column to SemanticCacheEntries table';
END
ELSE
BEGIN
    PRINT 'LastUpdated column already exists in SemanticCacheEntries table';
END

-- Check if we need to add the IsActive column (might already exist)
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[SemanticCacheEntries]') AND name = 'IsActive')
BEGIN
    ALTER TABLE [dbo].[SemanticCacheEntries] 
    ADD [IsActive] BIT NOT NULL DEFAULT 1;
    
    PRINT 'Added IsActive column to SemanticCacheEntries table';
END
ELSE
BEGIN
    PRINT 'IsActive column already exists in SemanticCacheEntries table';
END

-- Add indexes for the new columns if they don't exist
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[SemanticCacheEntries]') AND name = 'IX_SemanticCacheEntries_CreatedDate')
BEGIN
    CREATE NONCLUSTERED INDEX [IX_SemanticCacheEntries_CreatedDate]
        ON [dbo].[SemanticCacheEntries] ([CreatedDate] DESC);
    
    PRINT 'Created index IX_SemanticCacheEntries_CreatedDate';
END
ELSE
BEGIN
    PRINT 'Index IX_SemanticCacheEntries_CreatedDate already exists';
END

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[SemanticCacheEntries]') AND name = 'IX_SemanticCacheEntries_IsActive_ExpiresAt')
BEGIN
    CREATE NONCLUSTERED INDEX [IX_SemanticCacheEntries_IsActive_ExpiresAt]
        ON [dbo].[SemanticCacheEntries] ([IsActive] ASC, [ExpiresAt] ASC);
    
    PRINT 'Created index IX_SemanticCacheEntries_IsActive_ExpiresAt';
END
ELSE
BEGIN
    PRINT 'Index IX_SemanticCacheEntries_IsActive_ExpiresAt already exists';
END

-- Verify the changes
SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    IS_NULLABLE,
    CHARACTER_MAXIMUM_LENGTH
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'SemanticCacheEntries' 
    AND COLUMN_NAME IN ('CreatedBy', 'UpdatedBy', 'CreatedDate', 'LastUpdated', 'IsActive')
ORDER BY COLUMN_NAME;

PRINT 'Migration completed successfully. SemanticCacheEntries table now has all required base entity columns.';
