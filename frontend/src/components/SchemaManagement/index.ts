/**
 * Schema Management Components
 * Centralized exports for all schema management functionality
 */

// Main Dashboard
// Import CSS
import './SchemaManagement.css';

export { SchemaManagementDashboard } from './SchemaManagementDashboard';

// Core Schema Components
export { DatabaseSchemaViewer } from './DatabaseSchemaViewer';
export { SchemaEditor } from './SchemaEditor';
export { SchemaList } from './SchemaList';
export { SchemaVersions } from './SchemaVersions';
export { SchemaComparison } from './SchemaComparison';

// Editors
export { ColumnContextEditor } from './ColumnContextEditor';
export { TableContextEditor } from './TableContextEditor';
export { GlossaryTermEditor } from './GlossaryTermEditor';
export { RelationshipEditor } from './RelationshipEditor';

// Dialogs
export { CreateSchemaDialog } from './CreateSchemaDialog';
export { ImportSchemaDialog } from './ImportSchemaDialog';

// Type definitions for schema management
export interface SchemaManagementProps {
  onSchemaChange?: (schema: any) => void;
  onError?: (error: string) => void;
  readOnly?: boolean;
}

export interface SchemaEditorProps extends SchemaManagementProps {
  schema?: any;
  mode?: 'create' | 'edit' | 'view';
}

export interface SchemaDialogProps {
  visible: boolean;
  onClose: () => void;
  onSuccess?: (result: any) => void;
}
