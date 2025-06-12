/**
 * Admin Feature Module
 * 
 * Consolidated administrative components for system management.
 * Includes AI tuning, schema management, cache, security, and suggestions.
 */

// AI Tuning & Configuration - New Consolidated Components
export { TuningOverview } from '../../Tuning/TuningOverview';
export { AIConfigurationManager } from '../../Tuning/AIConfigurationManager';
export { PromptManagementHub } from '../../Tuning/PromptManagementHub';
export { KnowledgeBaseManager } from '../../Tuning/KnowledgeBaseManager';
export { MonitoringDashboard } from '../../Tuning/MonitoringDashboard';

// Legacy AI Tuning Components (still used internally)
export { TuningDashboard } from '../../Tuning/TuningDashboard';
export { AISettingsManager } from '../../Tuning/AISettingsManager';
export { PromptTemplateManager } from '../../Tuning/PromptTemplateManager';
export { BusinessGlossaryManager } from '../../Tuning/BusinessGlossaryManager';
export { BusinessTableManager } from '../../Tuning/BusinessTableManager';
export { QueryPatternManager } from '../../Tuning/QueryPatternManager';
export { PromptLogsViewer } from '../../Tuning/PromptLogsViewer';
export { AutoGenerationManager } from '../../Tuning/AutoGenerationManager';

// Schema Management
export { SchemaManagementDashboard } from '../../SchemaManagement/SchemaManagementDashboard';
export { SchemaEditor } from '../../SchemaManagement/SchemaEditor';
export { SchemaList } from '../../SchemaManagement/SchemaList';
export { SchemaVersions } from '../../SchemaManagement/SchemaVersions';
export { SchemaComparison } from '../../SchemaManagement/SchemaComparison';
export { DatabaseSchemaViewer } from '../../SchemaManagement/DatabaseSchemaViewer';
export { TableContextEditor } from '../../SchemaManagement/TableContextEditor';
export { ColumnContextEditor } from '../../SchemaManagement/ColumnContextEditor';
export { RelationshipEditor } from '../../SchemaManagement/RelationshipEditor';
export { GlossaryTermEditor } from '../../SchemaManagement/GlossaryTermEditor';
export { CreateSchemaDialog } from '../../SchemaManagement/CreateSchemaDialog';
export { ImportSchemaDialog } from '../../SchemaManagement/ImportSchemaDialog';

// Query Suggestions Management
export { QuerySuggestionsManager } from '../../Admin/QuerySuggestionsManager';
export { SuggestionsManager } from '../../Admin/SuggestionsManager';
export { SuggestionAnalytics } from '../../Admin/SuggestionAnalytics';
export { SuggestionSyncUtility } from '../../Admin/SuggestionSyncUtility';

// Cache Management
export { CacheManager } from '../../Cache/CacheManager';

// Security Management
export { SecurityDashboard } from '../../Security/SecurityDashboard';

// Types
export type * from '../../../types/admin';
