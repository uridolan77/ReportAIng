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
