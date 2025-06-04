// filepath: c:\dev\ReportAIng\frontend\src\components\DataTable\types\index.ts
import React from 'react';

export interface DataTableColumn {
  key: string;
  title: string;
  dataIndex: string;
  width?: number;
  minWidth?: number;
  maxWidth?: number;
  fixed?: 'left' | 'right' | boolean;
  hidden?: boolean;
  sortable?: boolean;
  filterable?: boolean;
  searchable?: boolean;
  resizable?: boolean;
  copyable?: boolean;
  editable?: boolean;
  required?: boolean;
  align?: 'left' | 'center' | 'right';
  ellipsis?: boolean;
  
  // Data types and formatting
  dataType?: 'string' | 'number' | 'date' | 'boolean' | 'object' | 'array';
  format?: string | ((value: any) => string);
  precision?: number;
  
  // Editing
  editType?: 'input' | 'number' | 'date' | 'select' | 'checkbox' | 'textarea' | 'custom';
  editOptions?: any[];
  editComponent?: React.ComponentType<any>;
  editorOptions?: any;
  validator?: (value: any, record: any) => boolean | string;
  
  // Aggregation
  aggregation?: 'sum' | 'avg' | 'min' | 'max' | 'count' | 'custom';
  aggregationFormatter?: (value: any) => string;
  customAggregation?: (values: any[]) => any;
  
  // Filtering
  filterType?: 'text' | 'number' | 'date' | 'dateRange' | 'select' | 'multiselect' | 'boolean' | 'custom';
  filterOptions?: any[];
  customFilter?: (value: any, filterValue: any) => boolean;
  
  // Export
  exportable?: boolean;
  exportFormatter?: (value: any) => string;
  
  // UI
  tooltip?: boolean | ((value: any, record: any) => string);
  render?: (value: any, record: any, index: number) => React.ReactNode;
  headerRender?: () => React.ReactNode;
  cellStyle?: React.CSSProperties | ((value: any, record: any, index: number) => React.CSSProperties);
  headerStyle?: React.CSSProperties;
  onCell?: (record: any, index: number) => any;
  onHeaderCell?: () => any;
}

export interface DataTableFeatures {
  virtualScroll?: boolean;
  advancedVirtualization?: boolean;
  dynamicRowHeight?: boolean;
  virtualColumnPinning?: boolean;
  performanceMonitoring?: boolean;
  pagination?: boolean;
  sorting?: boolean;
  filtering?: boolean;
  searching?: boolean;
  grouping?: boolean;
  aggregation?: boolean;
  selection?: boolean;
  editing?: boolean;
  reordering?: boolean;
  resizing?: boolean;
  copying?: boolean;
  contextMenu?: boolean;
  export?: boolean;
  exportFormats?: ('csv' | 'excel' | 'pdf' | 'json' | 'xml' | 'sql')[];
  print?: boolean;
  columnChooser?: boolean;
  saveState?: boolean;
  autoSaveState?: boolean;
  fullscreen?: boolean;
  debounceDelay?: number;
}

export interface DataTableConfig {
  pageSize?: number;
  pageSizeOptions?: number[];
  defaultSort?: { column: string; order: 'asc' | 'desc' }[];
  defaultFilters?: Record<string, any>;
  selectionType?: 'checkbox' | 'radio';
  exportFileName?: string;
  theme?: 'light' | 'dark' | 'auto';
  density?: 'comfortable' | 'standard' | 'compact';
  maxHeight?: number | string;
  showAggregationRow?: boolean;
  stateStorage?: 'localStorage' | 'sessionStorage';
  statePersistencePrefix?: string;
  enableStateEncryption?: boolean;
  virtualization?: {
    itemHeight?: number;
    overscan?: number;
    scrollBehavior?: 'auto' | 'smooth';
    bufferSize?: number;
    estimatedItemSize?: number;
    enableDynamicSizing?: boolean;
    enablePerfMode?: boolean;
  };
}

export interface DataTableComponents {
  toolbar?: React.ComponentType<any>;
  footer?: React.ComponentType<any>;
  loadingIndicator?: React.ComponentType<any>;
  emptyState?: React.ComponentType<any>;
  errorState?: React.ComponentType<any>;
}

export interface DataTableProps {
  // Data
  data: any[];
  columns: DataTableColumn[];
  keyField?: string;
  loading?: boolean;
  error?: Error | null;
  
  // Table identification for state persistence
  tableId?: string;
  
  // Configuration
  features?: DataTableFeatures;
  config?: DataTableConfig;
  components?: DataTableComponents;
  
  // Styling
  className?: string;
  style?: React.CSSProperties;
  tableStyle?: React.CSSProperties;
  rowStyle?: React.CSSProperties | ((record: any, index: number) => React.CSSProperties);
  
  // Event Callbacks
  onDataChange?: (data: any[]) => void;
  onRowClick?: (record: any, index: number) => void;
  onRowDoubleClick?: (record: any, index: number) => void;
  onCellClick?: (value: any, record: any, column: DataTableColumn, index: number) => void;
  onSelectionChange?: (selectedRows: any[]) => void;
  onSort?: (sortConfig: any) => void;
  onFilter?: (filterConfig: any) => void;
  onSearch?: (searchText: string) => void;
  onPageChange?: (page: number, pageSize: number) => void;
  onColumnReorder?: (columns: DataTableColumn[]) => void;
  onColumnResize?: (column: string, width: number) => void;
  onEdit?: (record: any, column: string, value: any, index: number) => void;
  onExport?: (format: string, data: any[]) => void;
  onStateChange?: (state: any) => void;
  onError?: (error: Error) => void;
  onRefresh?: () => void;
  onRealtimeUpdate?: (data: any[]) => void;
  onAISuggestion?: (suggestion: any) => void;
}

export interface DataTableState {
  displayColumns: DataTableColumn[];
  sortConfig: any[];
  filterConfig: Record<string, any>;
  searchText: string;
  currentPage: number;
  pageSize: number;
  selectedRows: any[];
  showFilterPanel: boolean;
  showColumnChooser: boolean;
  showExportModal: boolean;
  isFullscreen: boolean;
}

export interface SortConfig {
  column: string;
  order: 'asc' | 'desc';
}

export interface FilterConfig {
  [key: string]: any;
}

export interface ContextMenuContext {
  type: 'cell' | 'row' | 'header';
  value?: any;
  record?: any;
  column?: DataTableColumn;
  rowIndex?: number;
  event?: React.MouseEvent;
}

export interface VirtualizationOptions {
  itemHeight: number;
  containerHeight: number;
  overscan: number;
  bufferSize: number;
  scrollThreshold: number;
  estimatedItemSize: number;
  dynamicHeight: boolean;
}

export interface PersistenceOptions {
  storage: 'localStorage' | 'sessionStorage';
  prefix: string;
  encryption: boolean;
}
