import React, { useState, useEffect, useMemo, useRef, useCallback } from 'react';
import {
  ConfigProvider,
  theme,
  message,
  notification,
  Spin,
  Alert,
  Empty,
  Typography
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
import { VirtualizedTable, StandardTable } from './components/DataTableRenderer';
import { ExportService } from './services/ExportService';

const { Text, Title, Paragraph } = Typography;
const { Option } = Select;
const { RangePicker } = DatePicker;
const { TextArea } = Input;
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

interface DataTableProps {
  // Data
  data: any[];
  columns: DataTableColumn[];
  keyField?: string;
  loading?: boolean;
  error?: Error | null;
  
  // Features flags
  features?: {
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
  };
  
  // Configuration
  config?: {
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
  };
  
  // Callbacks
  onDataChange?: (data: any[]) => void;
  onRowClick?: (record: any, index: number) => void;
  onRowDoubleClick?: (record: any, index: number) => void;
  onCellClick?: (value: any, record: any, column: DataTableColumn, index: number) => void;
  onCellDoubleClick?: (value: any, record: any, column: DataTableColumn, index: number) => void;
  onSelectionChange?: (selectedRows: any[]) => void;
  onSort?: (sortConfig: any) => void;
  onFilter?: (filterConfig: any) => void;
  onSearch?: (searchText: string) => void;
  onPageChange?: (page: number, pageSize: number) => void;
  onColumnReorder?: (columns: DataTableColumn[]) => void;
  onColumnResize?: (column: string, width: number) => void;
  onEdit?: (record: any, column: string, value: any, index: number) => void;
  onEditComplete?: (changes: any[]) => void;
  onExport?: (format: string, data: any[]) => void;
  onStateChange?: (state: any) => void;
  onError?: (error: Error) => void;
  
  // Custom components
  components?: {
    toolbar?: React.ComponentType<any>;
    footer?: React.ComponentType<any>;
    loadingIndicator?: React.ComponentType<any>;
    emptyState?: React.ComponentType<any>;
    errorState?: React.ComponentType<any>;
  };
  
  // Styling
  className?: string;
  style?: React.CSSProperties;
  tableStyle?: React.CSSProperties;
  headerStyle?: React.CSSProperties;
  rowStyle?: React.CSSProperties | ((record: any, index: number) => React.CSSProperties);
  cellStyle?: React.CSSProperties | ((value: any, record: any, column: DataTableColumn, index: number) => React.CSSProperties);
}

// Main component
const DataTable: React.FC<DataTableProps> = ({
  data = [],
  columns = [],
  keyField = 'id',
  loading = false,
  error = null,
  features = {},
  config = {},
  onDataChange,
  onRowClick,
  onRowDoubleClick,
  onCellClick,
  onCellDoubleClick,
  onSelectionChange,
  onSort,
  onFilter,
  onSearch,
  onPageChange,
  onColumnReorder,
  onColumnResize,
  onEdit,
  onEditComplete,
  onExport,
  onStateChange,
  onError,
  components = {},
  className,
  style,
  tableStyle,
  headerStyle,
  rowStyle,
  cellStyle
}) => {
  const { token } = useToken();
  
  // State management
  const [processedData, setProcessedData] = useState<any[]>([]);
  const [displayColumns, setDisplayColumns] = useState<DataTableColumn[]>(columns);
  const [selectedRows, setSelectedRows] = useState<any[]>([]);
  const [sortConfig, setSortConfig] = useState<any[]>(config.defaultSort || []);
  const [filterConfig, setFilterConfig] = useState<Record<string, any>>(config.defaultFilters || {});
  const [searchText, setSearchText] = useState('');
  const [currentPage, setCurrentPage] = useState(1);
  const [pageSize, setPageSize] = useState(config.pageSize || 50);
  const [editingCell, setEditingCell] = useState<{ row: number; column: string } | null>(null);
  const [editingRow, setEditingRow] = useState<number | null>(null);
  const [pendingEdits, setPendingEdits] = useState<Map<string, any>>(new Map());
  const [isFullscreen, setIsFullscreen] = useState(false);
  const [showColumnChooser, setShowColumnChooser] = useState(false);
  const [showFilterPanel, setShowFilterPanel] = useState(false);
  const [showExportModal, setShowExportModal] = useState(false);
  const [showSettingsModal, setShowSettingsModal] = useState(false);
  const [tableState, setTableState] = useState<any>({});
  const [aiInsights, setAiInsights] = useState<any[]>([]);
  const [realtimeConnection, setRealtimeConnection] = useState<any>(null);
  
  // Refs
  const tableRef = useRef<HTMLDivElement>(null);
  const exportRef = useRef<any>(null);
  const printRef = useRef<any>(null);
  
  // Feature flags with defaults
  const enabledFeatures = {
    virtualScroll: features.virtualScroll ?? false,
    pagination: features.pagination ?? true,
    sorting: features.sorting ?? true,
    filtering: features.filtering ?? true,
    searching: features.searching ?? true,
    grouping: features.grouping ?? false,
    pivoting: features.pivoting ?? false,
    aggregation: features.aggregation ?? false,
    selection: features.selection ?? true,
    editing: features.editing ?? false,
    reordering: features.reordering ?? true,
    resizing: features.resizing ?? true,
    copying: features.copying ?? true,
    contextMenu: features.contextMenu ?? true,
    keyboard: features.keyboard ?? true,
    export: features.export ?? true,
    exportFormats: features.exportFormats ?? ['csv', 'excel', 'pdf'],
    print: features.print ?? true,
    columnChooser: features.columnChooser ?? true,
    saveState: features.saveState ?? true,
    templates: features.templates ?? false,
    formula: features.formula ?? false,
    validation: features.validation ?? false,
    history: features.history ?? false,
    collaboration: features.collaboration ?? false,
    realtime: features.realtime ?? false,
    ai: features.ai ?? false,
    theming: features.theming ?? true,
    darkMode: features.darkMode ?? true,
    compactMode: features.compactMode ?? true,
    fullscreen: features.fullscreen ?? true,
    minimap: features.minimap ?? false,
    heatmap: features.heatmap ?? false,
    sparklines: features.sparklines ?? false,
    lazyLoading: features.lazyLoading ?? false,
    infiniteScroll: features.infiniteScroll ?? false,
    debounceDelay: features.debounceDelay ?? 300,
    caching: features.caching ?? true
  };
  
  // Debounced search
  const [debouncedSearchText] = useDebounce(searchText, enabledFeatures.debounceDelay);
  
  // Process data with sorting, filtering, searching, grouping
  useEffect(() => {
    let processed = [...data];
    
    // Apply search
    if (debouncedSearchText && enabledFeatures.searching) {
      processed = processed.filter(row => {
        return displayColumns.some(col => {
          if (col.searchable !== false) {
            const value = _.get(row, col.dataIndex);
            return String(value).toLowerCase().includes(debouncedSearchText.toLowerCase());
          }
          return false;
        });
      });
    }
    
    // Apply filters
    if (enabledFeatures.filtering) {
      Object.entries(filterConfig).forEach(([columnKey, filterValue]) => {
        if (filterValue !== undefined && filterValue !== null && filterValue !== '') {
          const column = displayColumns.find(col => col.key === columnKey);
          if (column) {
            if (column.customFilter) {
              processed = processed.filter(row => 
                column.customFilter!(_.get(row, column.dataIndex), filterValue)
              );
            } else {
              processed = processed.filter(row => {
                const value = _.get(row, column.dataIndex);
                if (Array.isArray(filterValue)) {
                  return filterValue.includes(value);
                }
                return String(value).toLowerCase().includes(String(filterValue).toLowerCase());
              });
            }
          }
        }
      });
    }
    
    // Apply sorting
    if (enabledFeatures.sorting && sortConfig.length > 0) {
      processed = _.orderBy(
        processed,
        sortConfig.map(s => s.column),
        sortConfig.map(s => s.order)
      );
    }
    
    setProcessedData(processed);
  }, [data, debouncedSearchText, filterConfig, sortConfig, displayColumns, enabledFeatures]);
  
  // Pagination
  const paginatedData = useMemo(() => {
    if (!enabledFeatures.pagination) return processedData;
    const start = (currentPage - 1) * pageSize;
    return processedData.slice(start, start + pageSize);
  }, [processedData, currentPage, pageSize, enabledFeatures.pagination]);
  
  // Column visibility management
  const visibleColumns = useMemo(() => 
    displayColumns.filter(col => !col.hidden),
    [displayColumns]
  );
  
  // Keyboard shortcuts
  useHotkeys('ctrl+f', () => enabledFeatures.searching && document.getElementById('datatable-search')?.focus());
  useHotkeys('ctrl+a', () => enabledFeatures.selection && handleSelectAll());
  useHotkeys('ctrl+c', () => enabledFeatures.copying && handleCopy());
  useHotkeys('ctrl+e', () => enabledFeatures.export && setShowExportModal(true));
  useHotkeys('ctrl+p', () => enabledFeatures.print && handlePrint());
  useHotkeys('f11', () => enabledFeatures.fullscreen && toggleFullscreen());
  useHotkeys('escape', () => {
    setEditingCell(null);
    setEditingRow(null);
    setShowColumnChooser(false);
    setShowFilterPanel(false);
    setShowExportModal(false);
    setShowSettingsModal(false);
  });
  
  // Handlers
  const handleSort = (column: DataTableColumn) => {
    if (!enabledFeatures.sorting || column.sortable === false) return;
    
    const existingSort = sortConfig.find(s => s.column === column.dataIndex);
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
  onPageChange,
  onExport,
  onRefresh,
  onStateChange,
  onRealtimeUpdate,
  onAISuggestion
}) => {
  const { token } = theme.useToken();
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
  const [isDensityMenuOpen, setIsDensityMenuOpen] = useState(false);
  
  // Feature flags with defaults
  const enabledFeatures = {
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
    exportFormats: ['csv', 'excel', 'pdf', 'json'] as const,
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
  const handleSort = (column: DataTableColumn) => {
    if (!enabledFeatures.sorting || column.sortable === false) return;
    
    const existingSort = sortConfig.find(s => s.column === column.dataIndex);
    let newSortConfig;
    
    if (!existingSort) {
      newSortConfig = [...sortConfig, { column: column.dataIndex, order: 'asc' }];
    } else if (existingSort.order === 'asc') {
      newSortConfig = sortConfig.map(s => 
        s.column === column.dataIndex ? { ...s, order: 'desc' } : s
      );
    } else {
      newSortConfig = sortConfig.filter(s => s.column !== column.dataIndex);
    }
    
    setSortConfig(newSortConfig);
    onSort?.(newSortConfig);
  };

  const handleFilter = (column: string, value: any) => {
    const newFilterConfig = { ...filterConfig };
    if (value === undefined || value === null || value === '') {
      delete newFilterConfig[column];
    } else {
      newFilterConfig[column] = value;
    }
    setFilterConfig(newFilterConfig);
    onFilter?.(newFilterConfig);
  };

  const handleSearch = (value: string) => {
    setSearchText(value);
    onSearch?.(value);
  };

  const handleExport = async (format: string) => {
    const exportData = selectedRows.length > 0 ? selectedRows : processedData;
    
    try {
      await exportService.exportData(
        exportData,
        visibleColumns,
        format as any,
        config.exportFileName || 'data'
      );
      
      message.success(`Data exported as ${format.toUpperCase()}`);
      onExport?.(format, exportData);
    } catch (error) {
      message.error(`Export failed: ${error}`);
    }
    
    setShowExportModal(false);
  };

  const handleColumnReorder = (newColumns: DataTableColumn[]) => {
    setDisplayColumns(newColumns);
    onColumnReorder?.(newColumns);
  };

  const handleSelectionChange = (newSelection: any[]) => {
    setSelectedRows(newSelection);
    onSelectionChange?.(newSelection);
  };

  const handleEdit = (record: any, column: string, value: any, index: number) => {
    const key = `${record[keyField]}-${column}`;
    const newPendingEdits = new Map(pendingEdits);
    newPendingEdits.set(key, { record, column, value, index });
    setPendingEdits(newPendingEdits);
    onEdit?.(record, column, value, index);
  };

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
      
      default:
        return null;
    }
  };
  
  const renderFilterPanel = () => {
    if (!showFilterPanel) return null;
    
    return (
      <Card
        style={{
          margin: '16px',
          background: token.colorBgContainer,
          borderRadius: token.borderRadius,
          boxShadow: token.boxShadow
        }}
        title="Filters"
        extra={
          <Button
            type="text"
            icon={<CloseCircleOutlined />}
            onClick={() => setShowFilterPanel(false)}
          />
        }
      >
        <Space direction="vertical" style={{ width: '100%' }}>
          {visibleColumns.filter(col => col.filterable !== false).map(column => (
            <div key={column.key}>
              <Text strong>{column.title}</Text>
              {renderFilterControl(column)}
            </div>
          ))}
          <Space>
            <Button
              type="primary"
              onClick={() => setShowFilterPanel(false)}
            >
              Apply
            </Button>
            <Button
              onClick={() => {
                setFilterConfig({});
                setShowFilterPanel(false);
              }}
            >
              Clear All
            </Button>
          </Space>
        </Space>
      </Card>
    );
  };
  
  const renderFilterControl = (column: DataTableColumn) => {
    const filterValue = filterConfig[column.key];
    
    switch (column.filterType || column.dataType) {
      case 'text':
      case 'string':
        return (
          <Input
            placeholder={`Filter ${column.title}`}
            value={filterValue || ''}
            onChange={e => handleFilter(column.key, e.target.value)}
            style={{ marginTop: 8 }}
          />
        );
      
      case 'number':
        return (
          <Space style={{ marginTop: 8 }}>
            <InputNumber
              placeholder="Min"
              value={filterValue?.min}
              onChange={min => handleFilter(column.key, { ...filterValue, min })}
            />
            <InputNumber
              placeholder="Max"
              value={filterValue?.max}
              onChange={max => handleFilter(column.key, { ...filterValue, max })}
            />
          </Space>
        );
      
      case 'date':
        return (
          <DatePicker
            value={filterValue ? moment(filterValue) : null}
            onChange={date => handleFilter(column.key, date?.toISOString())}
            style={{ marginTop: 8, width: '100%' }}
          />
        );
      
      case 'dateRange':
        return (
          <RangePicker
            value={filterValue}
            onChange={dates => handleFilter(column.key, dates)}
            style={{ marginTop: 8, width: '100%' }}
          />
        );
      
      case 'select':
        return (
          <Select
            placeholder={`Select ${column.title}`}
            value={filterValue}
            onChange={value => handleFilter(column.key, value)}
            options={column.filterOptions}
            style={{ marginTop: 8, width: '100%' }}
            allowClear
          />
        );
      
      case 'multiselect':
        return (
          <Select
            mode="multiple"
            placeholder={`Select ${column.title}`}
            value={filterValue || []}
            onChange={value => handleFilter(column.key, value)}
            options={column.filterOptions}
            style={{ marginTop: 8, width: '100%' }}
          />
        );
      
      case 'boolean':
        return (
          <Radio.Group
            value={filterValue}
            onChange={e => handleFilter(column.key, e.target.value)}
            style={{ marginTop: 8 }}
          >
            <Radio value={true}>Yes</Radio>
            <Radio value={false}>No</Radio>
            <Radio value={undefined}>All</Radio>
          </Radio.Group>
        );
      
      default:
        return null;
    }
  };
  
  const renderAggregationRow = () => {
    if (!enabledFeatures.aggregation || !config.showAggregationRow) return null;
    
    return (
      <tr style={{ background: token.colorBgTextHover, fontWeight: 'bold' }}>
        {enabledFeatures.selection && <td />}
        {visibleColumns.map(column => {
          if (!column.aggregation) return <td key={column.key} />;
          
          const values = processedData.map(row => _.get(row, column.dataIndex));
          let aggregatedValue;
          
          switch (column.aggregation) {
            case 'sum':
              aggregatedValue = _.sum(values.filter(v => typeof v === 'number'));
              break;
            case 'avg':
              aggregatedValue = _.mean(values.filter(v => typeof v === 'number'));
              break;
            case 'min':
              aggregatedValue = _.min(values);
              break;
            case 'max':
              aggregatedValue = _.max(values);
              break;
            case 'count':
              aggregatedValue = values.filter(v => v != null).length;
              break;
            case 'custom':
              aggregatedValue = column.customAggregation?.(values);
              break;
          }
          
          return (
            <td key={column.key} style={{ padding: '8px 16px' }}>
              {column.aggregationFormatter
                ? column.aggregationFormatter(aggregatedValue)
                : aggregatedValue}
            </td>
          );
        })}
      </tr>
    );
  };
  
  const renderTable = () => {
    if (loading) {
      return components.loadingIndicator ? (
        <components.loadingIndicator />
      ) : (
        <div style={{ textAlign: 'center', padding: 100 }}>
          <Spin size="large" />
        </div>
      );
    }
    
    if (error) {
      return components.errorState ? (
        <components.errorState error={error} />
      ) : (
        <Alert
          message="Error loading data"
          description={error.message}
          type="error"
          showIcon
        />
      );
    }
    
    if (paginatedData.length === 0) {
      return components.emptyState ? (
        <components.emptyState />
      ) : (
        <Empty
          description="No data available"
          image={Empty.PRESENTED_IMAGE_SIMPLE}
        />
      );
    }
    
    // Virtual scroll implementation
    if (enabledFeatures.virtualScroll) {
      return (
        <AutoSizer>
          {({ height, width }) => (
            <Table
              dataSource={paginatedData}
              columns={visibleColumns.map(col => ({
                title: () => renderColumnHeader(col),
                dataIndex: col.dataIndex,
                key: col.key,
                width: col.width,
                render: (value: any, record: any, index: number) =>
                  renderCell(value, record, col, index),
                onHeaderCell: () => ({
                  onClick: () => handleSort(col)
                })
              }))}
              pagination={false}
              scroll={{ y: height - 100 }}
              rowKey={keyField}
              rowSelection={enabledFeatures.selection ? {
                selectedRowKeys: selectedRows.map(row => row[keyField]),
                onChange: (keys, rows) => {
                  setSelectedRows(rows);
                  onSelectionChange?.(rows);
                },
                type: config.selectionType || 'checkbox'
              } : undefined}
              style={tableStyle}
            />
          )}
        </AutoSizer>
      );
    }
    
    // Regular table
    return (
      <Table
        dataSource={paginatedData}
        columns={visibleColumns.map(col => ({
          title: () => renderColumnHeader(col),
          dataIndex: col.dataIndex,
          key: col.key,
          width: col.width,
          render: (value: any, record: any, index: number) =>
            renderCell(value, record, col, index),
          onHeaderCell: () => ({
            onClick: () => handleSort(col)
          })
        }))}
        pagination={enabledFeatures.pagination ? {
          current: currentPage,
          pageSize,
          total: processedData.length,
          showSizeChanger: true,
          showQuickJumper: true,
          showTotal: (total, range) => `${range[0]}-${range[1]} of ${total} items`,
          pageSizeOptions: config.pageSizeOptions || [10, 20, 50, 100, 200],
          onChange: (page, size) => {
            setCurrentPage(page);
            setPageSize(size || pageSize);
            onPageChange?.(page, size || pageSize);
          }
        } : false}
        rowKey={keyField}
        rowSelection={enabledFeatures.selection ? {
          selectedRowKeys: selectedRows.map(row => row[keyField]),
          onChange: (keys, rows) => {
            setSelectedRows(rows);
            onSelectionChange?.(rows);
          },
          type: config.selectionType || 'checkbox'
        } : undefined}
        onRow={(record, index) => ({
          onClick: () => onRowClick?.(record, index as number),
          onDoubleClick: () => onRowDoubleClick?.(record, index as number),
          style: typeof rowStyle === 'function' 
            ? rowStyle(record, index as number) 
            : rowStyle
        })}
        style={tableStyle}
        loading={loading}
      />
    );
  };
  
  // Main render
  return (
    <ConfigProvider
      theme={{
        algorithm: config.theme === 'dark' ? theme.darkAlgorithm : theme.defaultAlgorithm,
      }}
    >
      <div
        ref={tableRef}
        className={`comprehensive-datatable ${className || ''}`}
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
        {renderToolbar()}
        {renderFilterPanel()}
        
        <div style={{ flex: 1, overflow: 'auto', position: 'relative' }}>
          {renderTable()}
        </div>
        
        {components.footer && <components.footer />}
        
        {/* Modals */}
        <Modal
          title="Column Chooser"
          open={showColumnChooser}
          onCancel={() => setShowColumnChooser(false)}
          onOk={() => setShowColumnChooser(false)}
          width={600}
        >
          <DndContext
            sensors={useSensors(
              useSensor(PointerSensor),
              useSensor(KeyboardSensor, {
                coordinateGetter: sortableKeyboardCoordinates,
              })
            )}
            collisionDetection={closestCenter}
            onDragEnd={(event) => {
              const { active, over } = event;
              if (active.id !== over?.id) {
                const oldIndex = displayColumns.findIndex(col => col.key === active.id);
                const newIndex = displayColumns.findIndex(col => col.key === over?.id);
                const reordered = arrayMove(displayColumns, oldIndex, newIndex);
                setDisplayColumns(reordered);
                onColumnReorder?.(reordered);
              }
            }}
          >
            <SortableContext
              items={displayColumns.map(col => col.key)}
              strategy={horizontalListSortingStrategy}
            >
              <Space direction="vertical" style={{ width: '100%' }}>
                {displayColumns.map(column => (
                  <div key={column.key} style={{ display: 'flex', alignItems: 'center', gap: 8 }}>
                    <Checkbox
                      checked={!column.hidden}
                      onChange={e => {
                        const updated = displayColumns.map(col =>
                          col.key === column.key
                            ? { ...col, hidden: !e.target.checked }
                            : col
                        );
                        setDisplayColumns(updated);
                      }}
                      disabled={column.locked}
                    />
                    <span>{column.title}</span>
                    {column.locked && <LockOutlined />}
                  </div>
                ))}
              </Space>
            </SortableContext>
          </DndContext>
        </Modal>
        
        <Modal
          title="Export Data"
          open={showExportModal}
          onCancel={() => setShowExportModal(false)}
          footer={null}
          width={400}
        >
          <Space direction="vertical" style={{ width: '100%' }}>
            <Text>Choose export format:</Text>
            {enabledFeatures.exportFormats?.map(format => (
              <Button
                key={format}
                block
                icon={
                  format === 'excel' ? <FileExcelOutlined /> :
                  format === 'pdf' ? <FilePdfOutlined /> :
                  format === 'csv' ? <FileTextOutlined /> :
                  <DownloadOutlined />
                }
                onClick={() => handleExport(format)}
              >
                Export as {format.toUpperCase()}
              </Button>
            ))}
          </Space>
        </Modal>
      </div>
    </ConfigProvider>
  );
};

export default DataTable;