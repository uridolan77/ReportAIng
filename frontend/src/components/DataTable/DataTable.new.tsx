import React, { useState, useEffect, useMemo, useRef, useCallback } from 'react';
import {
  ConfigProvider,
  theme,
  message,
  notification,
  Spin,
  Alert,
  Empty
} from 'antd';
import { useHotkeys } from 'react-hotkeys-hook';
import { useDebounce } from 'use-debounce';
import _ from 'lodash';
import moment from 'moment';

// Import sub-components
import { DataTableToolbar } from './components/DataTableToolbar';
import { FilterPanel } from './components/FilterPanel';
import { DataTableModals } from './components/DataTableModals';
import { AggregationRow } from './components/AggregationRow';
import { DataTableRenderer } from './components/DataTableRenderer';
import { ExportService } from './services/ExportService';

const { useToken } = theme;

// Type definitions
interface DataTableColumn {
  key: string;
  title: string;
  dataIndex: string;
  dataType?: 'string' | 'number' | 'date' | 'boolean' | 'json' | 'array' | 'object' | 'custom';
  width?: number;
  minWidth?: number;
  maxWidth?: number;
  fixed?: 'left' | 'right';
  align?: 'left' | 'center' | 'right';
  ellipsis?: boolean;
  sortable?: boolean;
  filterable?: boolean;
  searchable?: boolean;
  groupable?: boolean;
  editable?: boolean;
  resizable?: boolean;
  hidden?: boolean;
  locked?: boolean;
  formatter?: (value: any, record: any, index: number) => React.ReactNode;
  editor?: 'text' | 'number' | 'date' | 'select' | 'multiselect' | 'boolean' | 'custom';
  editorOptions?: any;
  validator?: (value: any, record: any) => boolean | string;
  aggregation?: 'sum' | 'avg' | 'min' | 'max' | 'count' | 'custom';
  aggregationFormatter?: (value: any) => string;
  customAggregation?: (values: any[]) => any;
  filterType?: 'text' | 'number' | 'date' | 'dateRange' | 'select' | 'multiselect' | 'boolean' | 'custom';
  filterOptions?: any[];
  customFilter?: (value: any, filterValue: any) => boolean;
  exportable?: boolean;
  exportFormatter?: (value: any) => string;
  tooltip?: boolean | ((value: any, record: any) => string);
  copyable?: boolean;
  render?: (value: any, record: any, index: number) => React.ReactNode;
  headerRender?: () => React.ReactNode;
  cellStyle?: React.CSSProperties | ((value: any, record: any, index: number) => React.CSSProperties);
  headerStyle?: React.CSSProperties;
  onCell?: (record: any, index: number) => any;
  onHeaderCell?: () => any;
}

interface DataTableFeatures {
  // Display features
  virtualScroll?: boolean;
  pagination?: boolean;
  sorting?: boolean;
  filtering?: boolean;
  searching?: boolean;
  grouping?: boolean;
  pivoting?: boolean;
  aggregation?: boolean;
  
  // Interaction features
  selection?: boolean;
  editing?: boolean;
  reordering?: boolean;
  resizing?: boolean;
  copying?: boolean;
  contextMenu?: boolean;
  keyboard?: boolean;
  
  // Export features
  export?: boolean;
  exportFormats?: ('csv' | 'excel' | 'pdf' | 'json' | 'xml' | 'sql')[];
  print?: boolean;
  
  // Advanced features
  columnChooser?: boolean;
  saveState?: boolean;
  templates?: boolean;
  formula?: boolean;
  validation?: boolean;
  history?: boolean;
  collaboration?: boolean;
  realtime?: boolean;
  ai?: boolean;
  
  // Visual features
  theming?: boolean;
  darkMode?: boolean;
  compactMode?: boolean;
  fullscreen?: boolean;
  minimap?: boolean;
  heatmap?: boolean;
  sparklines?: boolean;
  
  // Performance features
  lazyLoading?: boolean;
  infiniteScroll?: boolean;
  debounceDelay?: number;
  caching?: boolean;
}

interface DataTableConfig {
  pageSize?: number;
  pageSizeOptions?: number[];
  defaultSort?: { column: string; order: 'asc' | 'desc' }[];
  defaultFilters?: Record<string, any>;
  defaultGroupBy?: string[];
  selectionType?: 'checkbox' | 'radio';
  editMode?: 'cell' | 'row' | 'form' | 'inline';
  exportFileName?: string;
  dateFormat?: string;
  numberFormat?: string;
  locale?: string;
  timezone?: string;
  theme?: 'light' | 'dark' | 'auto';
  density?: 'comfortable' | 'standard' | 'compact';
  stripedRows?: boolean;
  borderedCells?: boolean;
  hoverEffect?: boolean;
  rowHeight?: number | 'auto';
  headerHeight?: number;
  maxHeight?: number | string;
  stickyHeader?: boolean;
  stickyColumns?: number;
  
  // Aggregation config
  showAggregationRow?: boolean;
  aggregationRowPosition?: 'top' | 'bottom';
  
  // Real-time config
  realtimeEndpoint?: string;
  realtimeInterval?: number;
  
  // AI config
  aiEndpoint?: string;
  aiFeatures?: ('suggestions' | 'insights' | 'predictions' | 'anomalies')[];
}

interface DataTableComponents {
  toolbar?: React.ComponentType<any>;
  filterPanel?: React.ComponentType<any>;
  emptyState?: React.ComponentType<any>;
  loadingIndicator?: React.ComponentType<any>;
  errorState?: React.ComponentType<{ error: Error }>;
  footer?: React.ComponentType<any>;
  contextMenu?: React.ComponentType<any>;
  cellEditor?: React.ComponentType<any>;
  columnHeader?: React.ComponentType<any>;
  aggregationRow?: React.ComponentType<any>;
}

interface DataTableProps {
  // Data
  data: any[];
  columns: DataTableColumn[];
  keyField?: string;
  loading?: boolean;
  error?: Error | null;
  
  // Configuration
  features?: DataTableFeatures;
  config?: DataTableConfig;
  components?: DataTableComponents;
  
  // Styling
  className?: string;
  style?: React.CSSProperties;
  tableStyle?: React.CSSProperties;
  cellStyle?: React.CSSProperties | ((value: any, record: any, column: DataTableColumn, index: number) => React.CSSProperties);
  rowStyle?: React.CSSProperties | ((record: any, index: number) => React.CSSProperties);
  
  // Callbacks
  onDataChange?: (data: any[]) => void;
  onRowClick?: (record: any, index: number) => void;
  onRowDoubleClick?: (record: any, index: number) => void;
  onCellClick?: (value: any, record: any, column: DataTableColumn, index: number) => void;
  onCellDoubleClick?: (value: any, record: any, column: DataTableColumn, index: number) => void;
  onSelectionChange?: (selectedRows: any[]) => void;
  onSort?: (sortConfig: { column: string; order: 'asc' | 'desc' }[]) => void;
  onFilter?: (filterConfig: Record<string, any>) => void;
  onSearch?: (searchText: string) => void;
  onPageChange?: (page: number, pageSize: number) => void;
  onColumnReorder?: (columns: DataTableColumn[]) => void;
  onColumnResize?: (column: DataTableColumn, width: number) => void;
  onEdit?: (record: any, column: string, value: any, index: number) => void;
  onEditComplete?: (changes: any[]) => void;
  onExport?: (format: string, data: any[]) => void;
  onStateChange?: (state: any) => void;
  onError?: (error: Error) => void;
}

// Enhanced DataTable component with modular architecture
const DataTable: React.FC<DataTableProps> = ({
  data = [],
  columns = [],
  keyField = 'id',
  loading = false,
  error = null,
  features = {},
  config = {},
  components = {},
  className,
  style,
  tableStyle,
  cellStyle,
  rowStyle,
  // Event handlers
  onDataChange,
  onSort,
  onFilter,
  onSearch,
  onSelectionChange,
  onEdit,
  onEditComplete,
  onRowClick,
  onRowDoubleClick,
  onCellClick,
  onCellDoubleClick,
  onColumnReorder,
  onColumnResize,
  onPageChange,
  onExport,
  onStateChange,
  onError
}) => {
  const { token } = useToken();
  const tableRef = useRef<HTMLDivElement>(null);
  const exportService = useMemo(() => new ExportService(), []);
  
  // Core state
  const [displayColumns, setDisplayColumns] = useState(columns);
  const [sortConfig, setSortConfig] = useState(config.defaultSort || []);
  const [filterConfig, setFilterConfig] = useState(config.defaultFilters || {});
  const [searchText, setSearchText] = useState('');
  const [currentPage, setCurrentPage] = useState(1);
  const [pageSize, setPageSize] = useState(config.pageSize || 20);
  const [selectedRows, setSelectedRows] = useState<any[]>([]);
  const [editingCell, setEditingCell] = useState<{ row: number; column: string } | null>(null);
  const [editingRow, setEditingRow] = useState<number | null>(null);
  const [pendingEdits, setPendingEdits] = useState(new Map());
  
  // UI state
  const [showFilterPanel, setShowFilterPanel] = useState(false);
  const [showColumnChooser, setShowColumnChooser] = useState(false);
  const [showExportModal, setShowExportModal] = useState(false);
  const [isFullscreen, setIsFullscreen] = useState(false);
  
  // Feature flags with defaults
  const enabledFeatures: Required<DataTableFeatures> = {
    virtualScroll: false,
    pagination: true,
    sorting: true,
    filtering: true,
    searching: true,
    grouping: false,
    pivoting: false,
    aggregation: false,
    selection: true,
    editing: false,
    reordering: true,
    resizing: true,
    copying: true,
    contextMenu: false,
    keyboard: true,
    export: true,
    exportFormats: ['csv', 'excel', 'pdf', 'json'],
    print: true,
    columnChooser: true,
    saveState: false,
    templates: false,
    formula: false,
    validation: false,
    history: false,
    collaboration: false,
    realtime: false,
    ai: false,
    theming: true,
    darkMode: false,
    compactMode: false,
    fullscreen: true,
    minimap: false,
    heatmap: false,
    sparklines: false,
    lazyLoading: false,
    infiniteScroll: false,
    debounceDelay: 300,
    caching: false,
    ...features
  };

  // Debounced search
  const [debouncedSearchText] = useDebounce(searchText, enabledFeatures.debounceDelay);

  // Computed values
  const visibleColumns = useMemo(() => 
    displayColumns.filter(col => !col.hidden), 
    [displayColumns]
  );

  const processedData = useMemo(() => {
    let result = [...data];
    
    // Apply search
    if (debouncedSearchText && enabledFeatures.searching) {
      const searchLower = debouncedSearchText.toLowerCase();
      result = result.filter(row =>
        visibleColumns.some(col => {
          if (col.searchable === false) return false;
          const value = _.get(row, col.dataIndex);
          return String(value || '').toLowerCase().includes(searchLower);
        })
      );
    }
    
    // Apply filters
    if (Object.keys(filterConfig).length > 0 && enabledFeatures.filtering) {
      result = result.filter(row => {
        return Object.entries(filterConfig).every(([columnKey, filterValue]) => {
          const column = visibleColumns.find(col => col.key === columnKey);
          if (!column || !filterValue) return true;
          
          const cellValue = _.get(row, column.dataIndex);
          
          if (column.customFilter) {
            return column.customFilter(cellValue, filterValue);
          }
          
          // Default filter logic based on data type
          switch (column.filterType || column.dataType) {
            case 'number':
              const numValue = Number(cellValue);
              if (filterValue.min !== undefined && numValue < filterValue.min) return false;
              if (filterValue.max !== undefined && numValue > filterValue.max) return false;
              return true;
            case 'date':
              return moment(cellValue).isSame(moment(filterValue), 'day');
            case 'dateRange':
              const dateValue = moment(cellValue);
              return dateValue.isBetween(filterValue[0], filterValue[1], 'day', '[]');
            case 'select':
              return cellValue === filterValue;
            case 'multiselect':
              return Array.isArray(filterValue) && filterValue.includes(cellValue);
            case 'boolean':
              return cellValue === filterValue;
            default:
              return String(cellValue || '').toLowerCase().includes(String(filterValue).toLowerCase());
          }
        });
      });
    }
    
    // Apply sorting
    if (sortConfig.length > 0 && enabledFeatures.sorting) {
      result = _.orderBy(
        result,
        sortConfig.map(sort => (row: any) => _.get(row, sort.column)),
        sortConfig.map(sort => sort.order as 'asc' | 'desc')
      );
    }
    
    return result;
  }, [data, debouncedSearchText, filterConfig, sortConfig, visibleColumns, enabledFeatures]);

  const paginatedData = useMemo(() => {
    if (!enabledFeatures.pagination) return processedData;
    const start = (currentPage - 1) * pageSize;
    return processedData.slice(start, start + pageSize);
  }, [processedData, currentPage, pageSize, enabledFeatures.pagination]);

  // Event handlers
  const handleSort = useCallback((column: DataTableColumn) => {
    if (!enabledFeatures.sorting || column.sortable === false) return;
    
    const existingSort = sortConfig.find(s => s.column === column.dataIndex);
    let newSortConfig;
    
    if (!existingSort) {
      newSortConfig = [...sortConfig, { column: column.dataIndex, order: 'asc' as const }];
    } else if (existingSort.order === 'asc') {
      newSortConfig = sortConfig.map(s => 
        s.column === column.dataIndex ? { ...s, order: 'desc' as const } : s
      );
    } else {
      newSortConfig = sortConfig.filter(s => s.column !== column.dataIndex);
    }
    
    setSortConfig(newSortConfig);
    onSort?.(newSortConfig);
  }, [sortConfig, enabledFeatures.sorting, onSort]);

  const handleFilter = useCallback((column: string, value: any) => {
    const newFilterConfig = { ...filterConfig };
    if (value === undefined || value === null || value === '') {
      delete newFilterConfig[column];
    } else {
      newFilterConfig[column] = value;
    }
    setFilterConfig(newFilterConfig);
    onFilter?.(newFilterConfig);
  }, [filterConfig, onFilter]);

  const handleSearch = useCallback((value: string) => {
    setSearchText(value);
    onSearch?.(value);
  }, [onSearch]);

  const handleExport = useCallback(async (format: string) => {
    const exportData = selectedRows.length > 0 ? selectedRows : processedData;
    
    try {
      await exportService.export(
        exportData,
        visibleColumns,
        format as any,
        config.exportFileName || 'data'
      );
      
      message.success(`Data exported as ${format.toUpperCase()}`);
      onExport?.(format, exportData);
    } catch (error) {
      message.error(`Export failed: ${error}`);
      onError?.(error as Error);
    }
    
    setShowExportModal(false);
  }, [selectedRows, processedData, exportService, visibleColumns, config.exportFileName, onExport, onError]);

  const handleColumnReorder = useCallback((newColumns: DataTableColumn[]) => {
    setDisplayColumns(newColumns);
    onColumnReorder?.(newColumns);
  }, [onColumnReorder]);

  const handleSelectionChange = useCallback((newSelection: any[]) => {
    setSelectedRows(newSelection);
    onSelectionChange?.(newSelection);
  }, [onSelectionChange]);

  const handleEdit = useCallback((record: any, column: string, value: any, index: number) => {
    const key = `${record[keyField]}-${column}`;
    const newPendingEdits = new Map(pendingEdits);
    newPendingEdits.set(key, { record, column, value, index });
    setPendingEdits(newPendingEdits);
    onEdit?.(record, column, value, index);
  }, [pendingEdits, keyField, onEdit]);

  const handlePageChange = useCallback((page: number, size: number) => {
    setCurrentPage(page);
    setPageSize(size);
    onPageChange?.(page, size);
  }, [onPageChange]);

  // Keyboard shortcuts
  useHotkeys('ctrl+c, cmd+c', () => {
    if (enabledFeatures.copying && selectedRows.length > 0) {
      const text = selectedRows.map(row => 
        visibleColumns.map(col => _.get(row, col.dataIndex)).join('\t')
      ).join('\n');
      
      navigator.clipboard.writeText(text);
      message.success('Data copied to clipboard');
    }
  }, { enableOnFormTags: false });

  useHotkeys('ctrl+f, cmd+f', (e) => {
    if (enabledFeatures.searching) {
      e.preventDefault();
      document.getElementById('datatable-search')?.focus();
    }
  }, { enableOnFormTags: false });

  useHotkeys('escape', () => {
    setEditingCell(null);
    setEditingRow(null);
    setShowColumnChooser(false);
    setShowFilterPanel(false);
    setShowExportModal(false);
  }, { enableOnFormTags: false });

  // Effects
  useEffect(() => {
    setDisplayColumns(columns);
  }, [columns]);

  // Error boundary
  if (error) {
    return components.errorState ? (
      <components.errorState error={error} />
    ) : (
      <Alert
        message="Error loading data"
        description={error.message}
        type="error"
        showIcon
        style={{ margin: 16 }}
      />
    );
  }

  // Loading state
  if (loading) {
    return components.loadingIndicator ? (
      <components.loadingIndicator />
    ) : (
      <div style={{ textAlign: 'center', padding: 100 }}>
        <Spin size="large" />
      </div>
    );
  }

  // Empty state
  if (data.length === 0) {
    return components.emptyState ? (
      <components.emptyState />
    ) : (
      <Empty
        description="No data available"
        image={Empty.PRESENTED_IMAGE_SIMPLE}
        style={{ padding: 100 }}
      />
    );
  }

  // Main render
  return (
    <ConfigProvider
      theme={{
        algorithm: config.theme === 'dark' ? theme.darkAlgorithm : theme.defaultAlgorithm,
      }}
    >
      <div
        ref={tableRef}
        className={`enhanced-datatable ${className || ''}`}
        style={{
          height: '100%',
          display: 'flex',
          flexDirection: 'column',
          background: token.colorBgContainer,
          borderRadius: token.borderRadius,
          overflow: 'hidden',
          ...style
        }}
      >
        {/* Toolbar */}
        <DataTableToolbar
          searchText={searchText}
          onSearch={handleSearch}
          filterConfig={filterConfig}
          showFilterPanel={showFilterPanel}
          onToggleFilterPanel={() => setShowFilterPanel(!showFilterPanel)}
          onExport={() => setShowExportModal(true)}
          onColumnChooser={() => setShowColumnChooser(true)}
          onFullscreen={() => setIsFullscreen(!isFullscreen)}
          isFullscreen={isFullscreen}
          enabledFeatures={enabledFeatures}
          selectedRows={selectedRows}
        />

        {/* Filter Panel */}
        {showFilterPanel && (
          <FilterPanel
            columns={visibleColumns}
            filterConfig={filterConfig}
            onFilter={handleFilter}
            onClose={() => setShowFilterPanel(false)}
            onClear={() => setFilterConfig({})}
          />
        )}

        {/* Table Container */}
        <div style={{ flex: 1, overflow: 'auto', position: 'relative' }}>
          {/* Aggregation Row */}
          {enabledFeatures.aggregation && config.showAggregationRow && (
            <AggregationRow
              data={processedData}
              columns={visibleColumns}
              position={config.aggregationRowPosition || 'bottom'}
            />
          )}

          {/* Main Table */}
          <DataTableRenderer
            data={paginatedData}
            columns={visibleColumns}
            keyField={keyField}
            sortConfig={sortConfig}
            selectedRows={selectedRows}
            editingCell={editingCell}
            onSort={handleSort}
            onSelectionChange={handleSelectionChange}
            onEdit={handleEdit}
            onRowClick={onRowClick}
            onRowDoubleClick={onRowDoubleClick}
            onCellClick={onCellClick}
            onCellDoubleClick={onCellDoubleClick}
            onEditingCellChange={setEditingCell}
            enabledFeatures={enabledFeatures}
            config={config}
            tableStyle={tableStyle}
            cellStyle={cellStyle}
            rowStyle={rowStyle}
            // Pagination
            pagination={enabledFeatures.pagination ? {
              current: currentPage,
              pageSize,
              total: processedData.length,
              showSizeChanger: true,
              showQuickJumper: true,
              showTotal: (total, range) => `${range[0]}-${range[1]} of ${total} items`,
              pageSizeOptions: config.pageSizeOptions || [10, 20, 50, 100, 200],
              onChange: handlePageChange
            } : false}
          />
        </div>

        {/* Footer */}
        {components.footer && <components.footer />}

        {/* Modals */}
        <DataTableModals
          // Column Chooser Modal
          showColumnChooser={showColumnChooser}
          onCloseColumnChooser={() => setShowColumnChooser(false)}
          columns={displayColumns}
          onColumnReorder={handleColumnReorder}
          onColumnVisibilityChange={(columnKey, visible) => {
            const updated = displayColumns.map(col =>
              col.key === columnKey ? { ...col, hidden: !visible } : col
            );
            setDisplayColumns(updated);
          }}
          
          // Export Modal
          showExportModal={showExportModal}
          onCloseExportModal={() => setShowExportModal(false)}
          onExport={handleExport}
          exportFormats={enabledFeatures.exportFormats || []}
          selectedRowsCount={selectedRows.length}
          totalRowsCount={processedData.length}
        />
      </div>
    </ConfigProvider>
  );
};

export default DataTable;
export type { DataTableProps, DataTableColumn, DataTableFeatures, DataTableConfig, DataTableComponents };
