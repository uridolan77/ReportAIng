-- Clear all existing schema data from BusinessSchema-related tables
-- This will prepare the database for fresh schema discovery from DailyActionsDB

USE [BIReportingCopilot_Dev]
GO

BEGIN TRANSACTION;

BEGIN TRY
    PRINT '🧹 CLEARING ALL EXISTING SCHEMA DATA';
    PRINT '=====================================';
    PRINT '';

    -- Clear in proper order (child tables first to avoid FK violations)
    
    -- 1. Clear SchemaColumnContexts
    IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'SchemaColumnContexts')
    BEGIN
        DELETE FROM SchemaColumnContexts;
        PRINT '✓ Cleared SchemaColumnContexts table';
    END

    -- 2. Clear SchemaTableContexts
    IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'SchemaTableContexts')
    BEGIN
        DELETE FROM SchemaTableContexts;
        PRINT '✓ Cleared SchemaTableContexts table';
    END

    -- 3. Clear BusinessSchemaVersions
    IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'BusinessSchemaVersions')
    BEGIN
        DELETE FROM BusinessSchemaVersions;
        PRINT '✓ Cleared BusinessSchemaVersions table';
    END

    -- 4. Clear BusinessSchemas
    IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'BusinessSchemas')
    BEGIN
        DELETE FROM BusinessSchemas;
        PRINT '✓ Cleared BusinessSchemas table';
    END

    -- 5. Clear other schema-related tables
    IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'SchemaGlossaryTerms')
    BEGIN
        DELETE FROM SchemaGlossaryTerms;
        PRINT '✓ Cleared SchemaGlossaryTerms table';
    END

    IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'SchemaRelationships')
    BEGIN
        DELETE FROM SchemaRelationships;
        PRINT '✓ Cleared SchemaRelationships table';
    END

    IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'SchemaMetadata')
    BEGIN
        DELETE FROM SchemaMetadata;
        PRINT '✓ Cleared SchemaMetadata table';
    END

    IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'UserSchemaPreferences')
    BEGIN
        DELETE FROM UserSchemaPreferences;
        PRINT '✓ Cleared UserSchemaPreferences table';
    END

    -- 6. Clear business info tables (old auto-generated data)
    IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'BusinessTableInfo')
    BEGIN
        DELETE FROM BusinessTableInfo;
        PRINT '✓ Cleared BusinessTableInfo table';
    END

    IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'BusinessColumnInfo')
    BEGIN
        DELETE FROM BusinessColumnInfo;
        PRINT '✓ Cleared BusinessColumnInfo table';
    END

    IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'BusinessGlossary')
    BEGIN
        DELETE FROM BusinessGlossary;
        PRINT '✓ Cleared BusinessGlossary table';
    END

    COMMIT TRANSACTION;
    
    PRINT '';
    PRINT '🎉 ALL SCHEMA DATA CLEARED SUCCESSFULLY!';
    PRINT '';
    PRINT 'Your database is now ready for fresh schema discovery from DailyActionsDB.';
    PRINT 'Next steps:';
    PRINT '1. Test the schema discovery API endpoint';
    PRINT '2. Use the wizard to explore DailyActionsDB schema';
    PRINT '3. Save discovered schema to BusinessSchema tables';

END TRY
BEGIN CATCH
    ROLLBACK TRANSACTION;
    
    PRINT '';
    PRINT '❌ ERROR CLEARING SCHEMA DATA';
    PRINT 'Error Number: ' + CAST(ERROR_NUMBER() AS VARCHAR(10));
    PRINT 'Error Message: ' + ERROR_MESSAGE();
    PRINT 'Error Line: ' + CAST(ERROR_LINE() AS VARCHAR(10));
    
    THROW;
END CATCH
