// filepath: c:\dev\ReportAIng\frontend\src\components\DataTable\hooks\useDataTableHandlers.ts
import { useCallback } from 'react';
import { message } from 'antd';
import _ from 'lodash';
import { DataTableColumn, DataTableProps, DataTableState, DataTableFeatures } from '../types';
import { ExportService } from '../services/ExportService';

interface UseDataTableHandlersProps {
  state: DataTableState;
  actions: any;
  enabledFeatures: DataTableFeatures;
  processedData: any[];
  visibleColumns: DataTableColumn[];
  config: any;
  tableRef: React.RefObject<HTMLDivElement>;
  props: DataTableProps;
}

export const useDataTableHandlers = ({
  state,
  actions,
  enabledFeatures,
  processedData,
  visibleColumns,
  config,
  tableRef,
  props
}: UseDataTableHandlersProps) => {

  const handleSort = useCallback((column: DataTableColumn) => {
    if (!enabledFeatures.sorting || column.sortable === false) return;
    
    const existingSort = state.sortConfig.find(s => s.column === column.dataIndex);
    let newSortConfig;
    
    if (!existingSort) {
      newSortConfig = [...state.sortConfig, { column: column.dataIndex, order: 'asc' }];
    } else if (existingSort.order === 'asc') {
      newSortConfig = state.sortConfig.map(s => 
        s.column === column.dataIndex ? { ...s, order: 'desc' } : s
      );
    } else {
      newSortConfig = state.sortConfig.filter(s => s.column !== column.dataIndex);
    }
    
    actions.setSortConfig(newSortConfig);
    props.onSort?.(newSortConfig);
  }, [state.sortConfig, enabledFeatures.sorting, props.onSort, actions]);

  const handleFilter = useCallback((columnKey: string, value: any) => {
    const newFilterConfig = { ...state.filterConfig, [columnKey]: value };
    actions.setFilterConfig(newFilterConfig);
    props.onFilter?.(newFilterConfig);
    actions.setCurrentPage(1); // Reset to first page when filtering
  }, [state.filterConfig, props.onFilter, actions]);

  const handleSearch = useCallback((value: string) => {
    actions.setSearchText(value);
    props.onSearch?.(value);
    actions.setCurrentPage(1); // Reset to first page when searching
  }, [props.onSearch, actions]);

  const handlePageChange = useCallback((page: number, size?: number) => {
    actions.setCurrentPage(page);
    if (size && size !== state.pageSize) {
      actions.setPageSize(size);
    }
    props.onPageChange?.(page, size || state.pageSize);
  }, [state.pageSize, props.onPageChange, actions]);

  const handleSelectionChange = useCallback((selectedRowKeys: React.Key[], selectedRowsData: any[]) => {
    actions.setSelectedRows(selectedRowsData);
    props.onSelectionChange?.(selectedRowsData);
  }, [props.onSelectionChange, actions]);

  const handleColumnReorder = useCallback((newColumns: DataTableColumn[]) => {
    actions.setDisplayColumns(newColumns);
    props.onColumnReorder?.(newColumns);
  }, [props.onColumnReorder, actions]);

  const handleExport = useCallback(async (format: string) => {
    try {
      const exportData = processedData.map(row => {
        const exportRow: any = {};
        visibleColumns.forEach(col => {
          if (col.exportable !== false) {
            const value = _.get(row, col.dataIndex);
            exportRow[col.title] = col.exportFormatter ? col.exportFormatter(value) : value;
          }
        });
        return exportRow;
      });

      const fileName = config.exportFileName || 'data-export';
      
      switch (format) {
        case 'csv':
          ExportService.exportCSV(exportData, visibleColumns, fileName);
          break;
        case 'excel':
          ExportService.exportExcel(exportData, visibleColumns, fileName);
          break;
        case 'pdf':
          ExportService.exportPDF(exportData, visibleColumns, fileName);
          break;
        case 'json':
          ExportService.exportJSON(exportData, fileName);
          break;
        case 'xml':
          ExportService.exportXML(exportData, fileName);
          break;
        case 'sql':
          ExportService.exportSQL(exportData, 'data_table', fileName);
          break;
      }
      
      message.success(`Data exported as ${format.toUpperCase()}`);
      actions.setShowExportModal(false);
      props.onExport?.(format, exportData);
    } catch (error) {
      message.error('Export failed');
      props.onError?.(error as Error);
    }
  }, [processedData, visibleColumns, config.exportFileName, props.onExport, props.onError, actions]);

  const handlePrint = useCallback(() => {
    window.print();
  }, []);

  const handleRefresh = useCallback(() => {
    // Reset filters and sorting
    actions.setFilterConfig({});
    actions.setSortConfig([]);
    actions.setSearchText('');
    actions.setCurrentPage(1);
    props.onRefresh?.();
    message.success('Table refreshed');
  }, [actions, props.onRefresh]);

  const toggleFullscreen = useCallback(() => {
    if (!document.fullscreenElement) {
      tableRef.current?.requestFullscreen();
      actions.setIsFullscreen(true);
    } else {
      document.exitFullscreen();
      actions.setIsFullscreen(false);
    }
  }, [tableRef, actions]);

  const handleSelectAll = useCallback(() => {
    if (enabledFeatures.selection) {
      actions.setSelectedRows([...processedData]);
      props.onSelectionChange?.([...processedData]);
      message.success('All rows selected');
    }
  }, [enabledFeatures.selection, processedData, actions, props.onSelectionChange]);

  const handleCopy = useCallback((value: any) => {
    if (navigator.clipboard) {
      navigator.clipboard.writeText(String(value));
      message.success('Copied to clipboard');
    }
  }, []);

  return {
    handleSort,
    handleFilter,
    handleSearch,
    handlePageChange,
    handleSelectionChange,
    handleColumnReorder,
    handleExport,
    handlePrint,
    handleRefresh,
    toggleFullscreen,
    handleSelectAll,
    handleCopy
  };
};
