/**
 * Unified Components Index
 * Consolidated exports after deep cleanup and consolidation
 */

// Cache Management
export * from './Cache';

// Dashboard Components
export * from './Dashboard';

// Developer Tools
export * from './DevTools/DevTools';

// Query Interface
export {
  QueryInterface,
  QueryBuilder,
  MinimalQueryInterface,
  QueryEditor,
  SqlEditor,
  QueryResult,
  QueryProvider,
  useQueryContext,
  QueryTabs,
  QueryModals,
  LoadingStates,
  QueryHistory,
  QuerySuggestions,
  ProactiveSuggestions,
  QueryWizard,
  GuidedQueryWizard,
  AIProcessingFeedback,
  QueryProcessingViewer,
  PromptDetailsPanel,
  AdvancedStreamingQuery,
  InteractiveResultsDisplay,
  AccessibilityFeatures,
  QueryShortcuts
} from './QueryInterface';
export { ExportModal as QueryExportModal } from './QueryInterface';

// Visualization Components
export * from './Visualization';

// Data Components
export { default as DataTable } from './DataTable';
export type { DataTableProps, DataTableColumn, DataTableFeatures, DataTableConfig } from './DataTable';
export { ExportModal as DataTableExportModal } from './DataTable';
export * from './DBExplorer';

// Layout Components
export * from './Layout/Layout';
export * from './Layout/DatabaseConnectionBanner';
export * from './Layout/DatabaseStatusIndicator';

// Navigation
export * from './Navigation';

// Common Components (consolidated single-file components)
export * from './Common';

// Admin Components
export * from './Admin/QuerySuggestionsManager';

// Performance Components
export * from './Performance';

// Tuning Components
export * from './Tuning';

// Schema Management
export {
  SchemaManagementDashboard,
  DatabaseSchemaViewer,
  SchemaEditor,
  SchemaList,
  SchemaVersions,
  SchemaComparison,
  ColumnContextEditor,
  TableContextEditor,
  GlossaryTermEditor,
  RelationshipEditor,
  CreateSchemaDialog,
  ImportSchemaDialog
} from './SchemaManagement';
export type {
  SchemaManagementProps as SchemaManagementComponentProps,
  SchemaEditorProps as SchemaEditorComponentProps,
  SchemaDialogProps
} from './SchemaManagement';

// Providers
export * from './Providers/ReactQueryProvider';
export * from './Providers/StateSyncProvider';

// Demo Components
export * from './Demo';

// Debug Components (moved to DevTools)
export * from './DevTools/ConnectionStatus';
export * from './DevTools/DatabaseStatus';
export * from './DevTools/KeyVaultStatus';

// Onboarding
export * from './Onboarding';

// Styles (centralized design system)
export * from './styles';
