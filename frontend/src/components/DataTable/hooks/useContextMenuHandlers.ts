// filepath: c:\dev\ReportAIng\frontend\src\components\DataTable\hooks\useContextMenuHandlers.ts
import { useCallback } from 'react';
import { message } from 'antd';
import { DataTableFeatures, ContextMenuContext } from '../types';

interface UseContextMenuHandlersProps {
  enabledFeatures: DataTableFeatures;
  visibleColumns: any[];
  processedData: any[];
  selectedRows: any[];
  onSelectionChange?: (rows: any[]) => void;
  onCellClick?: (value: any, record: any, column: any, index: number) => void;
  handleExport: (format: string) => void;
  handleRefresh: () => void;
  handleCopy: (value: any) => void;
}

export const useContextMenuHandlers = ({
  enabledFeatures,
  visibleColumns,
  processedData,
  selectedRows,
  onSelectionChange,
  onCellClick,
  handleExport,
  handleRefresh,
  handleCopy
}: UseContextMenuHandlersProps) => {

  const handleContextMenuAction = useCallback((action: string, context: ContextMenuContext) => {
    switch (action) {
      case 'copy':
        if (context.value != null) {
          handleCopy(context.value);
        }
        break;
      
      case 'copy-row':
        if (context.record && enabledFeatures.copying) {
          const rowData = visibleColumns.map(col => context.record[col.dataIndex]).join('\t');
          handleCopy(rowData);
        }
        break;
      
      case 'copy-column':
        if (context.column && enabledFeatures.copying) {
          const columnData = processedData.map(row => row[context.column!.dataIndex]).join('\n');
          handleCopy(columnData);
        }
        break;
      
      case 'sort-asc':
        if (context.column && enabledFeatures.sorting) {
          // This would need to be connected to the sort handler
          message.info(`Sort ${context.column.title} ascending`);
        }
        break;
      
      case 'sort-desc':
        if (context.column && enabledFeatures.sorting) {
          // This would need to be connected to the sort handler
          message.info(`Sort ${context.column.title} descending`);
        }
        break;
      
      case 'filter':
        if (context.column && enabledFeatures.filtering) {
          message.info(`Filter ${context.column.title}`);
        }
        break;
      
      case 'export-csv':
        if (enabledFeatures.export && enabledFeatures.exportFormats?.includes('csv')) {
          handleExport('csv');
        }
        break;
      
      case 'export-excel':
        if (enabledFeatures.export && enabledFeatures.exportFormats?.includes('excel')) {
          handleExport('excel');
        }
        break;
      
      case 'select-all':
        if (enabledFeatures.selection) {
          onSelectionChange?.([...processedData]);
          message.success('All rows selected');
        }
        break;
      
      case 'clear-selection':
        if (enabledFeatures.selection) {
          onSelectionChange?.([]);
          message.success('Selection cleared');
        }
        break;
      
      case 'refresh':
        handleRefresh();
        break;
      
      default:
        // Custom actions can be handled by parent component
        break;
    }
  }, [
    visibleColumns,
    enabledFeatures,
    processedData,
    onSelectionChange,
    handleExport,
    handleRefresh,
    handleCopy
  ]);

  // Context menu wrapper for table renderers
  const handleTableContextMenu = useCallback((event: React.MouseEvent, context: ContextMenuContext) => {
    // Prevent default browser context menu
    event.preventDefault();
    event.stopPropagation();
    
    // Based on the context type, determine appropriate default actions
    if (context.type === 'cell' && context.value != null) {
      // Default to copy for cells with values
      handleContextMenuAction('copy', context);
    } else if (context.type === 'row' && context.record) {
      // Default to copy row for rows
      handleContextMenuAction('copy-row', context);
    } else if (context.type === 'header' && context.column) {
      // Default to sort for column headers
      handleContextMenuAction('sort-asc', context);
    }
  }, [handleContextMenuAction]);

  return {
    handleContextMenuAction,
    handleTableContextMenu
  };
};
