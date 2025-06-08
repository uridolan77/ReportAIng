// filepath: c:\dev\ReportAIng\frontend\src\components\DataTable\hooks\useEnhancedColumns.ts
import { useMemo } from 'react';
import { DataTableColumn } from '../types';
import { DataTypeDetector, enhanceColumnsWithTypeDetection } from '../utils/dataTypeDetection';

interface UseEnhancedColumnsProps {
  data: any[];
  columns: Partial<DataTableColumn>[];
  autoDetectTypes?: boolean;
  sampleSize?: number;
}

/**
 * Hook to enhance column configurations with automatic type detection and filtering
 */
export const useEnhancedColumns = ({
  data,
  columns,
  autoDetectTypes = true,
  sampleSize = 100
}: UseEnhancedColumnsProps) => {
  
  const enhancedColumns = useMemo(() => {
    if (!autoDetectTypes || !data.length) {
      return columns as DataTableColumn[];
    }

    // Use the type detection utility to enhance columns
    return enhanceColumnsWithTypeDetection(data, columns);
  }, [data, columns, autoDetectTypes, sampleSize]);

  // Generate filter options for multiselect columns
  const columnsWithFilterOptions = useMemo(() => {
    return enhancedColumns.map(column => {
      // Skip if column already has filter options or isn't filterable
      if (column.filterOptions || column.filterable === false || !data.length) {
        return column;
      }

      // Generate filter options for text fields that should be multiselect
      if ((column.filterType === 'multiselect' || column.dataType === 'string') && column.dataType === 'string') {
        const values = data
          .map(row => row[column.dataIndex])
          .filter(val => val != null && val !== '');

        const uniqueValues = [...new Set(values)];

        // Only create options if we have a reasonable number of unique values
        if (uniqueValues.length <= 50 && uniqueValues.length > 1) {
          return {
            ...column,
            filterType: 'multiselect' as const,
            filterOptions: uniqueValues
              .sort()
              .map(val => ({
                label: String(val),
                value: val
              }))
          } as DataTableColumn;
        }
      }

      return column;
    });
  }, [enhancedColumns, data]);

  // Provide column analysis for debugging/inspection
  const columnAnalysis = useMemo(() => {
    if (!data.length) return [];
    
    const columnNames = columns.map(col => col.dataIndex || col.key || '');
    return DataTypeDetector.analyzeColumns(data, columnNames);
  }, [data, columns]);

  return {
    enhancedColumns: columnsWithFilterOptions,
    columnAnalysis,
    // Utility functions
    getColumnByKey: (key: string) => columnsWithFilterOptions.find(col => col.key === key),
    getFilterableColumns: () => columnsWithFilterOptions.filter(col => col.filterable !== false),
    getSearchableColumns: () => columnsWithFilterOptions.filter(col => col.searchable !== false),
    getSortableColumns: () => columnsWithFilterOptions.filter(col => col.sortable !== false),
    getNumericColumns: () => columnsWithFilterOptions.filter(col => 
      col.dataType === 'number' || col.dataType === 'money'
    ),
    getDateColumns: () => columnsWithFilterOptions.filter(col => col.dataType === 'date'),
    getTextColumns: () => columnsWithFilterOptions.filter(col => col.dataType === 'string'),
    getMoneyColumns: () => columnsWithFilterOptions.filter(col => col.dataType === 'money')
  };
};

/**
 * Utility function to create basic column configuration from data
 */
export const createColumnsFromData = (data: any[], excludeColumns: string[] = ['id']): Partial<DataTableColumn>[] => {
  if (!data.length) return [];

  const sampleRow = data[0];
  const columnNames = Object.keys(sampleRow).filter(key => !excludeColumns.includes(key));

  return columnNames.map(columnName => ({
    key: columnName,
    title: formatColumnTitle(columnName),
    dataIndex: columnName,
    filterable: true,
    sortable: true,
    searchable: true
  }));
};

/**
 * Format column name to a readable title
 */
const formatColumnTitle = (columnName: string): string => {
  return columnName
    .replace(/([A-Z])/g, ' $1') // Add space before capital letters
    .replace(/[_-]/g, ' ')      // Replace underscores and hyphens with spaces
    .replace(/\b\w/g, l => l.toUpperCase()) // Capitalize first letter of each word
    .trim();
};

/**
 * Hook for automatic column generation from data
 */
export const useAutoColumns = (data: any[], excludeColumns: string[] = ['id']) => {
  const autoColumns = useMemo(() => {
    return createColumnsFromData(data, excludeColumns);
  }, [data, excludeColumns]);

  const { enhancedColumns, columnAnalysis } = useEnhancedColumns({
    data,
    columns: autoColumns,
    autoDetectTypes: true
  });

  return {
    columns: enhancedColumns,
    columnAnalysis,
    rawColumns: autoColumns
  };
};
