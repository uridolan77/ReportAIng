-- Comprehensive migration script to add missing IsActive and IsDefault columns
-- to ALL BusinessSchema-related tables
-- Run Date: Generated automatically  
-- Database: BIReportingCopilot_Dev
-- Description: Adds IsActive and IsDefault columns to SchemaTableContexts and SchemaColumnContexts tables

USE [BIReportingCopilot_Dev]
GO

BEGIN TRANSACTION;

BEGIN TRY
    PRINT 'Starting comprehensive BusinessSchema tables migration...';
    
    -- =====================================================
    -- 1. Fix SchemaTableContexts table
    -- =====================================================
    IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'SchemaTableContexts')
    BEGIN
        PRINT 'Processing SchemaTableContexts table...';
        
        -- Add IsActive column if missing
        IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
                       WHERE TABLE_NAME = 'SchemaTableContexts' AND COLUMN_NAME = 'IsActive')
        BEGIN
            PRINT '  Adding IsActive column to SchemaTableContexts...';
            ALTER TABLE SchemaTableContexts ADD IsActive BIT NOT NULL DEFAULT(1);
            PRINT '  ✓ IsActive column added to SchemaTableContexts.';
        END
        ELSE
        BEGIN
            PRINT '  ✓ IsActive column already exists in SchemaTableContexts.';
        END

        -- Add IsDefault column if missing
        IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
                       WHERE TABLE_NAME = 'SchemaTableContexts' AND COLUMN_NAME = 'IsDefault')
        BEGIN
            PRINT '  Adding IsDefault column to SchemaTableContexts...';
            ALTER TABLE SchemaTableContexts ADD IsDefault BIT NOT NULL DEFAULT(0);
            PRINT '  ✓ IsDefault column added to SchemaTableContexts.';
        END
        ELSE
        BEGIN
            PRINT '  ✓ IsDefault column already exists in SchemaTableContexts.';
        END
    END
    ELSE
    BEGIN
        PRINT '  ⚠️ SchemaTableContexts table does not exist.';
    END

    -- =====================================================
    -- 2. Fix SchemaColumnContexts table  
    -- =====================================================
    IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'SchemaColumnContexts')
    BEGIN
        PRINT 'Processing SchemaColumnContexts table...';
        
        -- Add IsActive column if missing
        IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
                       WHERE TABLE_NAME = 'SchemaColumnContexts' AND COLUMN_NAME = 'IsActive')
        BEGIN
            PRINT '  Adding IsActive column to SchemaColumnContexts...';
            ALTER TABLE SchemaColumnContexts ADD IsActive BIT NOT NULL DEFAULT(1);
            PRINT '  ✓ IsActive column added to SchemaColumnContexts.';
        END
        ELSE
        BEGIN
            PRINT '  ✓ IsActive column already exists in SchemaColumnContexts.';
        END

        -- Add IsDefault column if missing (for consistency)
        IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
                       WHERE TABLE_NAME = 'SchemaColumnContexts' AND COLUMN_NAME = 'IsDefault')
        BEGIN
            PRINT '  Adding IsDefault column to SchemaColumnContexts...';
            ALTER TABLE SchemaColumnContexts ADD IsDefault BIT NOT NULL DEFAULT(0);
            PRINT '  ✓ IsDefault column added to SchemaColumnContexts.';
        END
        ELSE
        BEGIN
            PRINT '  ✓ IsDefault column already exists in SchemaColumnContexts.';
        END
    END
    ELSE
    BEGIN
        PRINT '  ⚠️ SchemaColumnContexts table does not exist.';
    END

    -- =====================================================
    -- 3. Verify BusinessSchemas table (should already be fixed)
    -- =====================================================
    IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'BusinessSchemas')
    BEGIN
        PRINT 'Verifying BusinessSchemas table...';
        
        IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
                       WHERE TABLE_NAME = 'BusinessSchemas' AND COLUMN_NAME = 'IsActive')
        BEGIN
            PRINT '  Adding IsActive column to BusinessSchemas...';
            ALTER TABLE BusinessSchemas ADD IsActive BIT NOT NULL DEFAULT(1);
            PRINT '  ✓ IsActive column added to BusinessSchemas.';
        END
        ELSE
        BEGIN
            PRINT '  ✓ IsActive column already exists in BusinessSchemas.';
        END

        IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
                       WHERE TABLE_NAME = 'BusinessSchemas' AND COLUMN_NAME = 'IsDefault')
        BEGIN
            PRINT '  Adding IsDefault column to BusinessSchemas...';
            ALTER TABLE BusinessSchemas ADD IsDefault BIT NOT NULL DEFAULT(0);
            PRINT '  ✓ IsDefault column added to BusinessSchemas.';
        END
        ELSE
        BEGIN
            PRINT '  ✓ IsDefault column already exists in BusinessSchemas.';
        END
    END

    -- =====================================================
    -- 4. Update existing records with proper defaults
    -- =====================================================
    PRINT 'Updating existing records with proper defaults...';
    
    -- Update SchemaTableContexts
    IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'SchemaTableContexts')
    BEGIN
        UPDATE SchemaTableContexts SET IsActive = 1 WHERE IsActive IS NULL;
        UPDATE SchemaTableContexts SET IsDefault = 0 WHERE IsDefault IS NULL;
        PRINT '  ✓ Updated SchemaTableContexts defaults.';
    END

    -- Update SchemaColumnContexts
    IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'SchemaColumnContexts')
    BEGIN
        UPDATE SchemaColumnContexts SET IsActive = 1 WHERE IsActive IS NULL;
        UPDATE SchemaColumnContexts SET IsDefault = 0 WHERE IsDefault IS NULL;
        PRINT '  ✓ Updated SchemaColumnContexts defaults.';
    END

    -- Update BusinessSchemas
    IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'BusinessSchemas')
    BEGIN
        UPDATE BusinessSchemas SET IsActive = 1 WHERE IsActive IS NULL;
        UPDATE BusinessSchemas SET IsDefault = 0 WHERE IsDefault IS NULL;
        PRINT '  ✓ Updated BusinessSchemas defaults.';
    END

    -- =====================================================
    -- 5. Display final table structures
    -- =====================================================
    PRINT '';
    PRINT '=== FINAL TABLE STRUCTURES ===';
    
    -- SchemaTableContexts structure
    IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'SchemaTableContexts')
    BEGIN
        PRINT '';
        PRINT 'SchemaTableContexts columns:';
        SELECT 
            '  ' + COLUMN_NAME as ColumnName,
            DATA_TYPE as DataType,
            IS_NULLABLE as IsNullable,
            COLUMN_DEFAULT as DefaultValue
        FROM INFORMATION_SCHEMA.COLUMNS 
        WHERE TABLE_NAME = 'SchemaTableContexts'
        AND COLUMN_NAME IN ('IsActive', 'IsDefault')
        ORDER BY COLUMN_NAME;
    END

    -- SchemaColumnContexts structure  
    IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'SchemaColumnContexts')
    BEGIN
        PRINT '';
        PRINT 'SchemaColumnContexts columns:';
        SELECT 
            '  ' + COLUMN_NAME as ColumnName,
            DATA_TYPE as DataType,
            IS_NULLABLE as IsNullable,
            COLUMN_DEFAULT as DefaultValue
        FROM INFORMATION_SCHEMA.COLUMNS 
        WHERE TABLE_NAME = 'SchemaColumnContexts'
        AND COLUMN_NAME IN ('IsActive', 'IsDefault')
        ORDER BY COLUMN_NAME;
    END

    COMMIT TRANSACTION;
    PRINT '';
    PRINT '🎉 COMPREHENSIVE MIGRATION COMPLETED SUCCESSFULLY! 🎉';
    PRINT '';
    PRINT 'All BusinessSchema-related tables now have IsActive and IsDefault columns.';
    PRINT 'You can now restart your API application.';

END TRY
BEGIN CATCH
    ROLLBACK TRANSACTION;
    
    PRINT '';
    PRINT '❌ MIGRATION FAILED ❌';
    PRINT 'Error Number: ' + CAST(ERROR_NUMBER() AS VARCHAR(10));
    PRINT 'Error Message: ' + ERROR_MESSAGE();
    PRINT 'Error Line: ' + CAST(ERROR_LINE() AS VARCHAR(10));
    
    -- Re-throw the error
    THROW;
END CATCH
