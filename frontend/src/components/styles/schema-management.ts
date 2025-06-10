/**
 * Schema Management Styles Export
 * Type-safe style constants for schema management components
 */

// Import CSS files
import './schema-management.css';

// Export style constants for TypeScript usage
export const schemaManagementStyles = {
  container: 'schema-management-container',
  header: 'schema-management-header',
  title: 'schema-management-title',
  mainContent: 'schema-main-content'
} as const;

export const schemaStatsStyles = {
  row: 'schema-stats-row',
  card: 'schema-stat-card',
  value: 'schema-stat-value',
  label: 'schema-stat-label'
} as const;

export const schemaListStyles = {
  card: 'schema-list-card',
  header: 'schema-list-header',
  content: 'schema-list-content',
  itemCard: 'schema-item-card',
  itemSelected: 'schema-item-selected',
  itemContent: 'schema-item-content',
  itemInfo: 'schema-item-info',
  itemName: 'schema-item-name',
  itemDescription: 'schema-item-description',
  itemMeta: 'schema-item-meta',
  itemTag: 'schema-item-tag',
  itemActions: 'schema-item-actions'
} as const;

export const schemaDetailsStyles = {
  card: 'schema-details-card',
  header: 'schema-details-header',
  title: 'schema-details-title',
  content: 'schema-details-content'
} as const;

export const schemaEditorStyles = {
  container: 'schema-editor-container',
  tabs: 'schema-editor-tabs',
  content: 'schema-editor-content',
  fieldGroup: 'schema-field-group',
  fieldLabel: 'schema-field-label',
  fieldInput: 'schema-field-input'
} as const;

export const schemaComparisonStyles = {
  container: 'schema-comparison-container',
  panel: 'schema-comparison-panel',
  header: 'schema-comparison-header',
  content: 'schema-comparison-content',
  diffItem: 'schema-diff-item',
  diffAdded: 'schema-diff-added',
  diffRemoved: 'schema-diff-removed',
  diffModified: 'schema-diff-modified'
} as const;

export const schemaDialogStyles = {
  content: 'schema-dialog-content',
  form: 'schema-dialog-form',
  actions: 'schema-dialog-actions'
} as const;

// Type definitions
export interface SchemaManagementProps {
  className?: string;
  onSchemaSelect?: (schema: any) => void;
  selectedSchema?: any;
}

export interface SchemaListProps {
  schemas: any[];
  selectedSchema?: any;
  onSelect: (schema: any) => void;
  loading?: boolean;
}

export interface SchemaEditorProps {
  schema?: any;
  mode: 'create' | 'edit' | 'view';
  onSave?: (schema: any) => void;
  onCancel?: () => void;
}

export interface SchemaComparisonProps {
  leftSchema: any;
  rightSchema: any;
  showDifferences?: boolean;
}
