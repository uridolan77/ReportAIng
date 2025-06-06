// filepath: c:\dev\ReportAIng\frontend\src\components\DataTable\hooks\useDataProcessing.ts
import { useMemo } from 'react';
import _ from 'lodash';
import dayjs from 'dayjs';
import { DataTableColumn, DataTableFeatures, SortConfig, FilterConfig } from '../types';

interface UseDataProcessingProps {
  data: any[];
  debouncedSearchText: string;
  sortConfig: SortConfig[];
  filterConfig: FilterConfig;
  visibleColumns: DataTableColumn[];
  enabledFeatures: DataTableFeatures;
  currentPage: number;
  pageSize: number;
  hiddenRows?: any[];
  keyField?: string;
}

export const useDataProcessing = ({
  data,
  debouncedSearchText,
  sortConfig,
  filterConfig,
  visibleColumns,
  enabledFeatures,
  currentPage,
  pageSize,
  hiddenRows = [],
  keyField = 'id'
}: UseDataProcessingProps) => {
  
  // Process data: filter hidden rows, search, filter, sort
  const processedData = useMemo(() => {
    let result = [...data];

    // Filter out hidden rows first
    if (hiddenRows.length > 0 && enabledFeatures.rowHiding) {
      result = result.filter(row =>
        !hiddenRows.some(hiddenRow => hiddenRow[keyField] === row[keyField])
      );
    }

    // Apply search
    if (debouncedSearchText && enabledFeatures.searching) {
      const searchLower = debouncedSearchText.toLowerCase();
      result = result.filter(row =>
        visibleColumns.some(col => {
          if (col.searchable === false) return false;
          const cellValue = _.get(row, col.dataIndex);
          return String(cellValue || '').toLowerCase().includes(searchLower);
        })
      );
    }
    
    // Apply filters
    if (Object.keys(filterConfig).length > 0 && enabledFeatures.filtering) {
      result = result.filter(row => {
        return Object.entries(filterConfig).every(([columnKey, filterValue]) => {
          if (filterValue === undefined || filterValue === null || filterValue === '') return true;
          
          const column = visibleColumns.find(col => col.dataIndex === columnKey);
          if (!column) return true;
          
          const cellValue = _.get(row, columnKey);
          
          // Use custom filter if available
          if (column.customFilter) {
            return column.customFilter(cellValue, filterValue);
          }
          
          // Apply built-in filters based on filter type
          switch (column.filterType) {
            case 'number':
              const numValue = Number(cellValue);
              if (typeof filterValue === 'object' && filterValue !== null) {
                if (filterValue.min !== undefined && numValue < filterValue.min) return false;
                if (filterValue.max !== undefined && numValue > filterValue.max) return false;
                return true;
              }
              return numValue === Number(filterValue);
            
            case 'date':
              return dayjs(cellValue).isSame(dayjs(filterValue), 'day');
            
            case 'dateRange':
              const dateValue = dayjs(cellValue);
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
  }, [data, debouncedSearchText, filterConfig, sortConfig, visibleColumns, enabledFeatures, hiddenRows, keyField]);

  // Paginated data
  const paginatedData = useMemo(() => {
    if (!enabledFeatures.pagination) return processedData;
    const start = (currentPage - 1) * pageSize;
    return processedData.slice(start, start + pageSize);
  }, [processedData, currentPage, pageSize, enabledFeatures.pagination]);

  return {
    processedData,
    paginatedData,
    totalRecords: processedData.length,
    totalPages: Math.ceil(processedData.length / pageSize)
  };
};
