-- Migration script to add missing IsActive and IsDefault columns to PRODUCTION BusinessSchemas table
-- Run Date: Generated automatically
-- Database: BIReportingCopilot (PRODUCTION)
-- Description: Adds IsActive and IsDefault columns to BusinessSchemas table in production database

USE [BIReportingCopilot]
GO

BEGIN TRANSACTION;

BEGIN TRY
    -- Check if BusinessSchemas table exists
    IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'BusinessSchemas')
    BEGIN
        PRINT 'ERROR: BusinessSchemas table does not exist in production database. Please ensure the table is created first.';
        ROLLBACK TRANSACTION;
        RETURN;
    END

    -- Check if IsActive column already exists
    IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
                   WHERE TABLE_NAME = 'BusinessSchemas' AND COLUMN_NAME = 'IsActive')
    BEGIN
        PRINT 'Adding IsActive column to BusinessSchemas table in PRODUCTION...';
        
        ALTER TABLE BusinessSchemas 
        ADD IsActive BIT NOT NULL DEFAULT(1);
        
        PRINT 'IsActive column added successfully to PRODUCTION.';
    END
    ELSE
    BEGIN
        PRINT 'IsActive column already exists in PRODUCTION BusinessSchemas table.';
    END

    -- Check if IsDefault column already exists
    IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
                   WHERE TABLE_NAME = 'BusinessSchemas' AND COLUMN_NAME = 'IsDefault')
    BEGIN
        PRINT 'Adding IsDefault column to BusinessSchemas table in PRODUCTION...';
        
        ALTER TABLE BusinessSchemas 
        ADD IsDefault BIT NOT NULL DEFAULT(0);
        
        PRINT 'IsDefault column added successfully to PRODUCTION.';
    END
    ELSE
    BEGIN
        PRINT 'IsDefault column already exists in PRODUCTION BusinessSchemas table.';
    END

    -- Update existing records to ensure proper defaults
    UPDATE BusinessSchemas 
    SET IsActive = 1 
    WHERE IsActive IS NULL;

    -- Ensure only one schema is marked as default (if any exist)
    -- Set the first created schema as default if none is currently set
    IF NOT EXISTS (SELECT 1 FROM BusinessSchemas WHERE IsDefault = 1)
    BEGIN
        UPDATE TOP(1) BusinessSchemas 
        SET IsDefault = 1 
        WHERE Id IN (SELECT TOP(1) Id FROM BusinessSchemas ORDER BY CreatedAt);
        
        PRINT 'Set first schema as default in PRODUCTION.';
    END

    -- Display current table structure
    PRINT 'Current PRODUCTION BusinessSchemas table structure:';
    SELECT 
        COLUMN_NAME,
        DATA_TYPE,
        IS_NULLABLE,
        COLUMN_DEFAULT
    FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'BusinessSchemas'
    ORDER BY ORDINAL_POSITION;

    COMMIT TRANSACTION;
    PRINT 'PRODUCTION migration completed successfully!';

END TRY
BEGIN CATCH
    ROLLBACK TRANSACTION;
    
    PRINT 'PRODUCTION migration failed with error:';
    PRINT 'Error Number: ' + CAST(ERROR_NUMBER() AS VARCHAR(10));
    PRINT 'Error Message: ' + ERROR_MESSAGE();
    PRINT 'Error Line: ' + CAST(ERROR_LINE() AS VARCHAR(10));
    
    -- Re-throw the error
    THROW;
END CATCH
