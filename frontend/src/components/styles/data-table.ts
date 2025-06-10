/**
 * Data Table Styles Export
 * Type-safe style constants for data table components
 */

// Import CSS files
import './data-table.css';

// Export style constants for TypeScript usage
export const dataTableStyles = {
  container: 'data-table-container',
  header: 'data-table-header',
  title: 'data-table-title',
  actions: 'data-table-actions',
  wrapper: 'data-table-wrapper',
  pagination: 'data-table-pagination',
  loading: 'data-table-loading',
  skeleton: 'data-table-skeleton',
  empty: 'data-table-empty',
  emptyIcon: 'data-table-empty-icon',
  emptyText: 'data-table-empty-text',
  emptyDescription: 'data-table-empty-description'
} as const;

export const dbExplorerStyles = {
  container: 'db-explorer-container'
} as const;

export const filterStyles = {
  filters: 'data-table-filters',
  search: 'data-table-search',
  filterGroup: 'data-table-filter-group'
} as const;

export const columnStyles = {
  controls: 'data-table-column-controls',
  toggle: 'data-table-column-toggle',
  toggleActive: 'data-table-column-toggle active'
} as const;

export const exportStyles = {
  export: 'data-table-export',
  exportButton: 'data-table-export-button'
} as const;

// Type definitions
export interface DataTableProps {
  className?: string;
  loading?: boolean;
  empty?: boolean;
  showFilters?: boolean;
  showExport?: boolean;
}

export interface ColumnControlProps {
  columns: string[];
  visibleColumns: string[];
  onToggleColumn: (column: string) => void;
}

export interface ExportProps {
  formats: ('csv' | 'excel' | 'pdf')[];
  onExport: (format: string) => void;
}
