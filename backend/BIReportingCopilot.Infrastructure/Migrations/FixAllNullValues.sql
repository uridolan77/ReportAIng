-- Fix NULL values in all BaseEntity tables
-- Replace all NULL string values with empty strings to prevent SqlNullValueException

PRINT 'Fixing NULL values in all BaseEntity tables...';

-- Fix SchemaMetadata table
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'SchemaMetadata')
BEGIN
    PRINT 'Fixing SchemaMetadata...';
    UPDATE SchemaMetadata SET
        DatabaseName = ISNULL(DatabaseName, ''),
        SchemaName = ISNULL(SchemaName, ''),
        TableName = ISNULL(TableName, ''),
        ColumnName = ISNULL(ColumnName, ''),
        DataType = ISNULL(DataType, ''),
        BusinessDescription = ISNULL(BusinessDescription, ''),
        SemanticTags = ISNULL(SemanticTags, ''),
        SampleValues = ISNULL(SampleValues, ''),
        CreatedBy = ISNULL(CreatedBy, ''),
        UpdatedBy = ISNULL(UpdatedBy, ''),
        BusinessPurpose = ISNULL(BusinessPurpose, ''),
        RelatedBusinessTerms = ISNULL(RelatedBusinessTerms, ''),
        BusinessFriendlyName = ISNULL(BusinessFriendlyName, ''),
        NaturalLanguageDescription = ISNULL(NaturalLanguageDescription, ''),
        BusinessRules = ISNULL(BusinessRules, ''),
        RelationshipContext = ISNULL(RelationshipContext, ''),
        DataGovernanceLevel = ISNULL(DataGovernanceLevel, '');
    PRINT 'SchemaMetadata fixed: ' + CAST(@@ROWCOUNT AS VARCHAR(10)) + ' rows updated';
END

-- Fix BusinessTableInfo table
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'BusinessTableInfo')
BEGIN
    PRINT 'Fixing BusinessTableInfo...';
    UPDATE BusinessTableInfo SET
        SchemaName = ISNULL(SchemaName, ''),
        TableName = ISNULL(TableName, ''),
        BusinessName = ISNULL(BusinessName, ''),
        BusinessDescription = ISNULL(BusinessDescription, ''),
        BusinessPurpose = ISNULL(BusinessPurpose, ''),
        DataDomain = ISNULL(DataDomain, ''),
        BusinessOwner = ISNULL(BusinessOwner, ''),
        TechnicalOwner = ISNULL(TechnicalOwner, ''),
        DataClassification = ISNULL(DataClassification, ''),
        CreatedBy = ISNULL(CreatedBy, ''),
        UpdatedBy = ISNULL(UpdatedBy, ''),
        BusinessFriendlyName = ISNULL(BusinessFriendlyName, ''),
        NaturalLanguageDescription = ISNULL(NaturalLanguageDescription, ''),
        RelatedBusinessTerms = ISNULL(RelatedBusinessTerms, ''),
        BusinessRules = ISNULL(BusinessRules, ''),
        RelationshipContext = ISNULL(RelationshipContext, ''),
        DataGovernanceLevel = ISNULL(DataGovernanceLevel, '');
    PRINT 'BusinessTableInfo fixed: ' + CAST(@@ROWCOUNT AS VARCHAR(10)) + ' rows updated';
END

-- Fix BusinessColumnInfo table
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'BusinessColumnInfo')
BEGIN
    PRINT 'Fixing BusinessColumnInfo...';
    UPDATE BusinessColumnInfo SET
        SchemaName = ISNULL(SchemaName, ''),
        TableName = ISNULL(TableName, ''),
        ColumnName = ISNULL(ColumnName, ''),
        BusinessName = ISNULL(BusinessName, ''),
        BusinessDescription = ISNULL(BusinessDescription, ''),
        BusinessDataType = ISNULL(BusinessDataType, ''),
        BusinessMeaning = ISNULL(BusinessMeaning, ''),
        CreatedBy = ISNULL(CreatedBy, ''),
        UpdatedBy = ISNULL(UpdatedBy, ''),
        BusinessPurpose = ISNULL(BusinessPurpose, ''),
        RelatedBusinessTerms = ISNULL(RelatedBusinessTerms, ''),
        BusinessFriendlyName = ISNULL(BusinessFriendlyName, ''),
        NaturalLanguageDescription = ISNULL(NaturalLanguageDescription, ''),
        BusinessRules = ISNULL(BusinessRules, ''),
        RelationshipContext = ISNULL(RelationshipContext, ''),
        DataGovernanceLevel = ISNULL(DataGovernanceLevel, '');
    PRINT 'BusinessColumnInfo fixed: ' + CAST(@@ROWCOUNT AS VARCHAR(10)) + ' rows updated';
END

-- Fix PromptTemplates table
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'PromptTemplates')
BEGIN
    PRINT 'Fixing PromptTemplates...';
    UPDATE PromptTemplates SET
        Name = ISNULL(Name, ''),
        Version = ISNULL(Version, ''),
        Content = ISNULL(Content, ''),
        Description = ISNULL(Description, ''),
        Parameters = ISNULL(Parameters, ''),
        CreatedBy = ISNULL(CreatedBy, ''),
        UpdatedBy = ISNULL(UpdatedBy, ''),
        BusinessPurpose = ISNULL(BusinessPurpose, ''),
        RelatedBusinessTerms = ISNULL(RelatedBusinessTerms, ''),
        BusinessFriendlyName = ISNULL(BusinessFriendlyName, ''),
        NaturalLanguageDescription = ISNULL(NaturalLanguageDescription, ''),
        BusinessRules = ISNULL(BusinessRules, ''),
        RelationshipContext = ISNULL(RelationshipContext, ''),
        DataGovernanceLevel = ISNULL(DataGovernanceLevel, '');
    PRINT 'PromptTemplates fixed: ' + CAST(@@ROWCOUNT AS VARCHAR(10)) + ' rows updated';
END

-- Fix PromptLogs table
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'PromptLogs')
BEGIN
    PRINT 'Fixing PromptLogs...';
    UPDATE PromptLogs SET
        UserId = ISNULL(UserId, ''),
        PromptText = ISNULL(PromptText, ''),
        ResponseText = ISNULL(ResponseText, ''),
        ModelUsed = ISNULL(ModelUsed, ''),
        CreatedBy = ISNULL(CreatedBy, ''),
        UpdatedBy = ISNULL(UpdatedBy, ''),
        BusinessPurpose = ISNULL(BusinessPurpose, ''),
        RelatedBusinessTerms = ISNULL(RelatedBusinessTerms, ''),
        BusinessFriendlyName = ISNULL(BusinessFriendlyName, ''),
        NaturalLanguageDescription = ISNULL(NaturalLanguageDescription, ''),
        BusinessRules = ISNULL(BusinessRules, ''),
        RelationshipContext = ISNULL(RelationshipContext, ''),
        DataGovernanceLevel = ISNULL(DataGovernanceLevel, '');
    PRINT 'PromptLogs fixed: ' + CAST(@@ROWCOUNT AS VARCHAR(10)) + ' rows updated';
END

-- Fix Users table
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Users')
BEGIN
    PRINT 'Fixing Users...';
    UPDATE Users SET
        Id = ISNULL(Id, ''),
        Username = ISNULL(Username, ''),
        Email = ISNULL(Email, ''),
        DisplayName = ISNULL(DisplayName, ''),
        PasswordHash = ISNULL(PasswordHash, ''),
        Roles = ISNULL(Roles, ''),
        MfaSecret = ISNULL(MfaSecret, ''),
        MfaMethod = ISNULL(MfaMethod, ''),
        PhoneNumber = ISNULL(PhoneNumber, ''),
        BackupCodes = ISNULL(BackupCodes, ''),
        CreatedBy = ISNULL(CreatedBy, ''),
        UpdatedBy = ISNULL(UpdatedBy, ''),
        BusinessPurpose = ISNULL(BusinessPurpose, ''),
        RelatedBusinessTerms = ISNULL(RelatedBusinessTerms, ''),
        BusinessFriendlyName = ISNULL(BusinessFriendlyName, ''),
        NaturalLanguageDescription = ISNULL(NaturalLanguageDescription, ''),
        BusinessRules = ISNULL(BusinessRules, ''),
        RelationshipContext = ISNULL(RelationshipContext, ''),
        DataGovernanceLevel = ISNULL(DataGovernanceLevel, '');
    PRINT 'Users fixed: ' + CAST(@@ROWCOUNT AS VARCHAR(10)) + ' rows updated';
END

PRINT 'All NULL values fixed in BaseEntity tables!';

-- Summary verification
PRINT '';
PRINT '=== VERIFICATION SUMMARY ===';
SELECT 
    'BusinessGlossary' as TableName,
    COUNT(*) as TotalRows,
    SUM(CASE WHEN Term IS NULL THEN 1 ELSE 0 END) as NullStringColumns
FROM BusinessGlossary
UNION ALL
SELECT 
    'SchemaMetadata' as TableName,
    COUNT(*) as TotalRows,
    SUM(CASE WHEN DatabaseName IS NULL OR SchemaName IS NULL OR TableName IS NULL OR ColumnName IS NULL THEN 1 ELSE 0 END) as NullStringColumns
FROM SchemaMetadata
WHERE EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'SchemaMetadata')
UNION ALL
SELECT 
    'BusinessTableInfo' as TableName,
    COUNT(*) as TotalRows,
    SUM(CASE WHEN SchemaName IS NULL OR TableName IS NULL THEN 1 ELSE 0 END) as NullStringColumns
FROM BusinessTableInfo
WHERE EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'BusinessTableInfo');
