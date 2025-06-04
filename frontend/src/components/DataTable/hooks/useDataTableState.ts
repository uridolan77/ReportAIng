// filepath: c:\dev\ReportAIng\frontend\src\components\DataTable\hooks\useDataTableState.ts
import { useState, useRef, useMemo } from 'react';
import { useDebounce } from 'use-debounce';
import { DataTableColumn, DataTableFeatures, DataTableConfig, DataTableState } from '../types';
import { VirtualizationService } from '../services/VirtualizationService';

interface UseDataTableStateProps {
  columns: DataTableColumn[];
  features: DataTableFeatures;
  config: DataTableConfig;
}

export const useDataTableState = ({ columns, features, config }: UseDataTableStateProps) => {
  const virtualizationServiceRef = useRef<VirtualizationService | null>(null);
  const autoSaveTimeoutRef = useRef<NodeJS.Timeout | null>(null);
  
  // Feature flags with defaults
  const enabledFeatures: Required<DataTableFeatures> = {
    virtualScroll: false,
    advancedVirtualization: false,
    dynamicRowHeight: false,
    virtualColumnPinning: false,
    performanceMonitoring: false,
    pagination: true,
    sorting: true,
    filtering: true,
    searching: true,
    grouping: false,
    aggregation: false,
    selection: true,
    editing: false,
    reordering: true,
    resizing: true,
    copying: true,
    contextMenu: true,
    export: true,
    exportFormats: ['csv', 'excel', 'pdf', 'json'],
    print: true,
    columnChooser: true,
    saveState: false,
    autoSaveState: false,
    fullscreen: true,
    debounceDelay: 300,
    ...features
  };

  // State persistence options
  const persistenceOptions = {
    storage: config.stateStorage || 'localStorage',
    prefix: config.statePersistencePrefix || 'datatable_',
    encryption: config.enableStateEncryption || false
  };

  // Core state
  const [displayColumns, setDisplayColumns] = useState<DataTableColumn[]>(columns);
  const [sortConfig, setSortConfig] = useState<any[]>(config.defaultSort || []);
  const [filterConfig, setFilterConfig] = useState<Record<string, any>>(config.defaultFilters || {});
  const [searchText, setSearchText] = useState('');
  const [currentPage, setCurrentPage] = useState(1);
  const [pageSize, setPageSize] = useState(config.pageSize || 20);
  const [selectedRows, setSelectedRows] = useState<any[]>([]);
  
  // UI state
  const [showFilterPanel, setShowFilterPanel] = useState(false);
  const [showColumnChooser, setShowColumnChooser] = useState(false);
  const [showExportModal, setShowExportModal] = useState(false);
  const [isFullscreen, setIsFullscreen] = useState(false);

  // Debounced search
  const [debouncedSearchText] = useDebounce(searchText, enabledFeatures.debounceDelay);

  // Computed values
  const visibleColumns = useMemo(() => 
    displayColumns.filter(col => !col.hidden), 
    [displayColumns]
  );

  const state: DataTableState = {
    displayColumns,
    sortConfig,
    filterConfig,
    searchText,
    currentPage,
    pageSize,
    selectedRows,
    showFilterPanel,
    showColumnChooser,
    showExportModal,
    isFullscreen
  };

  const actions = {
    setDisplayColumns,
    setSortConfig,
    setFilterConfig,
    setSearchText,
    setCurrentPage,
    setPageSize,
    setSelectedRows,
    setShowFilterPanel,
    setShowColumnChooser,
    setShowExportModal,
    setIsFullscreen
  };

  return {
    state,
    actions,
    enabledFeatures,
    persistenceOptions,
    visibleColumns,
    debouncedSearchText,
    virtualizationServiceRef,
    autoSaveTimeoutRef
  };
};
