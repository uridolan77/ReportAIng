-- Check which records were enhanced by the MetadataEnhancementService
-- This script shows the recently modified records with their enhanced semantic fields

USE [BIReportingCopilot_Dev]
GO

PRINT '🔍 Checking recently enhanced metadata records...'
PRINT '================================================='

-- Check BusinessColumnInfo records enhanced in the last hour
PRINT ''
PRINT '📊 BusinessColumnInfo - Recently Enhanced Records:'
PRINT '================================================='

SELECT TOP 20
    bci.ColumnName,
    bti.TableName,
    bci.BusinessMeaning,
    bci.SemanticContext,
    bci.AnalyticalContext,
    bci.SemanticRelevanceScore,
    bci.UpdatedDate,
    bci.UpdatedBy
FROM [dbo].[BusinessColumnInfo] bci
LEFT JOIN [dbo].[BusinessTableInfo] bti ON bci.TableInfoId = bti.Id
WHERE bci.UpdatedDate >= DATEADD(HOUR, -1, GETUTCDATE())
   AND bci.UpdatedBy = 'MetadataEnhancementService'
ORDER BY bci.UpdatedDate DESC

-- Check BusinessTableInfo records enhanced in the last hour
PRINT ''
PRINT '📊 BusinessTableInfo - Recently Enhanced Records:'
PRINT '=============================================='

SELECT TOP 20
    TableName,
    BusinessPurpose,
    SemanticDescription,
    LLMContextHints,
    UpdatedDate,
    UpdatedBy
FROM [dbo].[BusinessTableInfo]
WHERE UpdatedDate >= DATEADD(HOUR, -1, GETUTCDATE())
   AND UpdatedBy = 'MetadataEnhancementService'
ORDER BY UpdatedDate DESC

-- Check BusinessGlossary records enhanced in the last hour
PRINT ''
PRINT '📊 BusinessGlossary - Recently Enhanced Records:'
PRINT '============================================='

SELECT TOP 20
    Term,
    Definition,
    Category,
    ContextualVariations,
    QueryPatterns,
    UpdatedDate,
    UpdatedBy
FROM [dbo].[BusinessGlossary]
WHERE UpdatedDate >= DATEADD(HOUR, -1, GETUTCDATE())
   AND UpdatedBy = 'MetadataEnhancementService'
ORDER BY UpdatedDate DESC

-- Summary of enhancements
PRINT ''
PRINT '📈 Enhancement Summary (Last Hour):'
PRINT '=================================='

SELECT 
    'BusinessColumnInfo' as TableName,
    COUNT(*) as RecordsEnhanced
FROM [dbo].[BusinessColumnInfo]
WHERE UpdatedDate >= DATEADD(HOUR, -1, GETUTCDATE())
   AND UpdatedBy = 'MetadataEnhancementService'

UNION ALL

SELECT 
    'BusinessTableInfo' as TableName,
    COUNT(*) as RecordsEnhanced
FROM [dbo].[BusinessTableInfo]
WHERE UpdatedDate >= DATEADD(HOUR, -1, GETUTCDATE())
   AND UpdatedBy = 'MetadataEnhancementService'

UNION ALL

SELECT 
    'BusinessGlossary' as TableName,
    COUNT(*) as RecordsEnhanced
FROM [dbo].[BusinessGlossary]
WHERE UpdatedDate >= DATEADD(HOUR, -1, GETUTCDATE())
   AND UpdatedBy = 'MetadataEnhancementService'

-- Check what fields were actually populated
PRINT ''
PRINT '🔧 Field Population Analysis:'
PRINT '============================'

SELECT 
    'SemanticContext populated' as Enhancement,
    COUNT(*) as Count
FROM [dbo].[BusinessColumnInfo]
WHERE UpdatedDate >= DATEADD(HOUR, -1, GETUTCDATE())
   AND UpdatedBy = 'MetadataEnhancementService'
   AND SemanticContext IS NOT NULL 
   AND SemanticContext != ''

UNION ALL

SELECT 
    'AnalyticalContext populated' as Enhancement,
    COUNT(*) as Count
FROM [dbo].[BusinessColumnInfo]
WHERE UpdatedDate >= DATEADD(HOUR, -1, GETUTCDATE())
   AND UpdatedBy = 'MetadataEnhancementService'
   AND AnalyticalContext IS NOT NULL 
   AND AnalyticalContext != ''

UNION ALL

SELECT 
    'SemanticRelevanceScore set' as Enhancement,
    COUNT(*) as Count
FROM [dbo].[BusinessColumnInfo]
WHERE UpdatedDate >= DATEADD(HOUR, -1, GETUTCDATE())
   AND UpdatedBy = 'MetadataEnhancementService'
   AND SemanticRelevanceScore > 0

UNION ALL

SELECT 
    'SemanticDescription populated' as Enhancement,
    COUNT(*) as Count
FROM [dbo].[BusinessTableInfo]
WHERE UpdatedDate >= DATEADD(HOUR, -1, GETUTCDATE())
   AND UpdatedBy = 'MetadataEnhancementService'
   AND SemanticDescription IS NOT NULL 
   AND SemanticDescription != ''

UNION ALL

SELECT 
    'LLMContextHints populated' as Enhancement,
    COUNT(*) as Count
FROM [dbo].[BusinessTableInfo]
WHERE UpdatedDate >= DATEADD(HOUR, -1, GETUTCDATE())
   AND UpdatedBy = 'MetadataEnhancementService'
   AND LLMContextHints IS NOT NULL 
   AND LLMContextHints != ''

-- Show audit trail
PRINT ''
PRINT '📋 Audit Trail (Recent Enhancement Activities):'
PRINT '=============================================='

SELECT TOP 20
    Action,
    EntityType,
    EntityId,
    Details,
    UserId,
    Timestamp
FROM [dbo].[AuditLogs]
WHERE Action LIKE '%METADATA_ENHANCEMENT%'
   AND Timestamp >= DATEADD(HOUR, -1, GETUTCDATE())
ORDER BY Timestamp DESC

PRINT ''
PRINT '✅ Analysis complete!'
