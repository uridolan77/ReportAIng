// DB Explorer Types

export interface DatabaseTable {
  name: string;
  schema: string;
  type: 'table' | 'view';
  rowCount?: number;
  description?: string;
  columns: DatabaseColumn[];
  indexes?: DatabaseIndex[];
  foreignKeys?: ForeignKey[];
  primaryKeys?: string[];
}

export interface DatabaseColumn {
  name: string;
  dataType: string;
  isNullable: boolean;
  isPrimaryKey: boolean;
  isForeignKey: boolean;
  defaultValue?: string;
  maxLength?: number;
  precision?: number;
  scale?: number;
  description?: string;
  referencedTable?: string;
  referencedColumn?: string;
}

export interface DatabaseIndex {
  name: string;
  columns: string[];
  isUnique: boolean;
  isPrimary: boolean;
  type: string;
}

export interface ForeignKey {
  name: string;
  column: string;
  referencedTable: string;
  referencedColumn: string;
  onDelete?: string;
  onUpdate?: string;
}

export interface DatabaseSchema {
  name: string;
  tables: DatabaseTable[];
  views: DatabaseTable[];
  lastUpdated: string;
  version: string;
}

export interface TableDataPreview {
  tableName: string;
  data: any[];
  totalRows: number;
  columns: DatabaseColumn[];
  sampleSize: number;
}

export interface DBExplorerState {
  selectedTable?: DatabaseTable;
  selectedSchema?: string;
  searchTerm: string;
  showDataPreview: boolean;
  previewData?: TableDataPreview;
  expandedTables: Set<string>;
  loading: boolean;
  error?: string;
  // Selection state for auto-generation
  selectedTables: Set<string>;
  selectedFields: Map<string, Set<string>>; // tableName -> Set of field names
  selectionMode: boolean;
  autoGenerationInProgress: boolean;
}

// Auto-generation related types
export interface AutoGenerationRequest {
  generateTableContexts: boolean;
  generateGlossaryTerms: boolean;
  analyzeRelationships: boolean;
  specificTables?: string[];
  specificSchemas?: string[];
  overwriteExisting: boolean;
  minimumConfidenceThreshold: number;
  mockMode: boolean;
}

export interface AutoGenerationProgress {
  currentTask: string;
  completedItems: number;
  totalItems: number;
  progressPercentage: number;
  currentTable?: string;
  recentlyCompleted: string[];
  elapsedTime: string;
  estimatedTimeRemaining?: string;
}

export interface SelectionSummary {
  totalTablesSelected: number;
  totalFieldsSelected: number;
  selectedTableNames: string[];
  fieldsByTable: Map<string, string[]>;
}

// Content versioning types
export interface ContentVersion {
  id: string;
  timestamp: string;
  author: string;
  changes: ContentChange[];
  comment?: string;
}

export interface ContentChange {
  type: 'table_context' | 'glossary_term';
  action: 'created' | 'updated' | 'deleted';
  itemId: string;
  itemName: string;
  fieldChanges?: FieldChange[];
}

export interface FieldChange {
  field: string;
  oldValue: string;
  newValue: string;
}

export interface VersionedContent {
  currentVersion: any;
  versions: ContentVersion[];
  lastModified: string;
  modifiedBy: string;
}

export interface TableSearchResult {
  table: DatabaseTable;
  matchType: 'table_name' | 'column_name' | 'description';
  matchedText: string;
  score: number;
}

export interface DBExplorerConfig {
  maxPreviewRows: number;
  enableDataPreview: boolean;
  enableExport: boolean;
  enableSearch: boolean;
  defaultExpandLevel: number;
  showSystemTables: boolean;
}
