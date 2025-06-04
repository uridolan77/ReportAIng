// Advanced Virtualization Service for DataTable
import React, { useMemo, useCallback, useRef, useEffect } from 'react';
import { FixedSizeList as List, VariableSizeList, ListChildComponentProps } from 'react-window';
import AutoSizer from 'react-virtualized-auto-sizer';
import { DataTableColumn } from '../DataTable';

export interface VirtualizationConfig {
  itemSize: number;
  variableSize?: boolean;
  overscan?: number;
  threshold?: number; // When to enable virtualization
  cacheSize?: number;
  buffer?: number;
  scrollBehavior?: 'smooth' | 'auto';
}

interface VirtualizedRowProps extends ListChildComponentProps {
  data: {
    items: any[];
    columns: DataTableColumn[];
    keyField: string;
    onRowClick?: (record: any, index: number) => void;
    rowSelection?: any;
    rowStyle?: React.CSSProperties | ((record: any, index: number) => React.CSSProperties);
    onCellClick?: (value: any, record: any, column: DataTableColumn, index: number) => void;
  };
}

const VirtualizedRow: React.FC<VirtualizedRowProps> = ({ index, style, data }) => {
  const { items, columns, keyField, onRowClick, rowSelection, rowStyle, onCellClick } = data;
  const record = items[index];
  
  const handleRowClick = useCallback(() => {
    onRowClick?.(record, index);
  }, [record, index, onRowClick]);

  const rowStyles = useMemo(() => ({
    ...style,
    ...(typeof rowStyle === 'function' ? rowStyle(record, index) : rowStyle),
    display: 'flex',
    alignItems: 'center',
    borderBottom: '1px solid #f0f0f0',
    cursor: onRowClick ? 'pointer' : 'default'
  }), [style, rowStyle, record, index, onRowClick]);

  return (
    <div style={rowStyles} onClick={handleRowClick}>
      {rowSelection && (
        <div style={{ width: 50, padding: '0 8px', flexShrink: 0 }}>
          <input
            type={rowSelection.type || 'checkbox'}
            checked={rowSelection.selectedRowKeys?.includes(record[keyField])}
            onChange={(e) => {
              e.stopPropagation();
              const newSelection = e.target.checked
                ? [...(rowSelection.selectedRowKeys || []), record[keyField]]
                : (rowSelection.selectedRowKeys || []).filter((key: any) => key !== record[keyField]);
              
              rowSelection.onChange?.(
                newSelection,
                newSelection.map(key => items.find(item => item[keyField] === key))
              );
            }}
          />
        </div>
      )}
      
      {columns.map((column, colIndex) => {
        const value = record[column.dataIndex];
        const cellContent = column.render ? column.render(value, record, index) : value;
        
        return (
          <div
            key={column.key}
            style={{
              width: column.width || 150,
              minWidth: column.minWidth || 50,
              maxWidth: column.maxWidth,
              padding: '8px',
              overflow: 'hidden',
              textOverflow: 'ellipsis',
              whiteSpace: 'nowrap',
              flexShrink: 0,
              textAlign: column.align || 'left'
            }}
            onClick={(e) => {
              e.stopPropagation();
              onCellClick?.(value, record, column, index);
            }}
          >
            {cellContent}
          </div>
        );
      })}
    </div>
  );
};

interface VirtualizedHeaderProps {
  columns: DataTableColumn[];
  rowSelection?: any;
  onSort?: (column: DataTableColumn) => void;
  sortConfig?: Array<{ column: string; order: 'asc' | 'desc' }>;
  onColumnResize?: (column: string, width: number) => void;
  onColumnReorder?: (columns: DataTableColumn[]) => void;
}

const VirtualizedHeader: React.FC<VirtualizedHeaderProps> = ({
  columns,
  rowSelection,
  onSort,
  sortConfig,
  onColumnResize,
  onColumnReorder
}) => {
  const resizingRef = useRef<{ column: string; startX: number; startWidth: number } | null>(null);

  const handleMouseDown = useCallback((e: React.MouseEvent, column: DataTableColumn) => {
    e.preventDefault();
    resizingRef.current = {
      column: column.key,
      startX: e.clientX,
      startWidth: column.width || 150
    };

    const handleMouseMove = (e: MouseEvent) => {
      if (!resizingRef.current) return;
      
      const deltaX = e.clientX - resizingRef.current.startX;
      const newWidth = Math.max(50, resizingRef.current.startWidth + deltaX);
      
      onColumnResize?.(resizingRef.current.column, newWidth);
    };

    const handleMouseUp = () => {
      resizingRef.current = null;
      document.removeEventListener('mousemove', handleMouseMove);
      document.removeEventListener('mouseup', handleMouseUp);
    };

    document.addEventListener('mousemove', handleMouseMove);
    document.addEventListener('mouseup', handleMouseUp);
  }, [onColumnResize]);

  const getSortIcon = (column: DataTableColumn) => {
    const sort = sortConfig?.find(s => s.column === column.dataIndex);
    if (!sort) return null;
    return sort.order === 'asc' ? '↑' : '↓';
  };

  return (
    <div style={{
      display: 'flex',
      alignItems: 'center',
      borderBottom: '2px solid #f0f0f0',
      background: '#fafafa',
      fontWeight: 'bold',
      position: 'sticky',
      top: 0,
      zIndex: 1
    }}>
      {rowSelection && (
        <div style={{ width: 50, padding: '0 8px', flexShrink: 0 }}>
          <input
            type={rowSelection.type || 'checkbox'}
            checked={rowSelection.selectedRowKeys?.length > 0}
            indeterminate={
              rowSelection.selectedRowKeys?.length > 0 && 
              rowSelection.selectedRowKeys.length < rowSelection.totalCount
            }
            onChange={(e) => {
              // Handle select all
              rowSelection.onSelectAll?.(e.target.checked);
            }}
          />
        </div>
      )}
      
      {columns.map((column) => (
        <div
          key={column.key}
          style={{
            width: column.width || 150,
            minWidth: column.minWidth || 50,
            maxWidth: column.maxWidth,
            padding: '8px',
            overflow: 'hidden',
            textOverflow: 'ellipsis',
            whiteSpace: 'nowrap',
            flexShrink: 0,
            textAlign: column.align || 'left',
            position: 'relative',
            cursor: column.sortable !== false ? 'pointer' : 'default',
            userSelect: 'none'
          }}
          onClick={() => onSort?.(column)}
        >
          {column.title}
          {getSortIcon(column)}
          
          {/* Resize handle */}
          <div
            style={{
              position: 'absolute',
              right: 0,
              top: 0,
              bottom: 0,
              width: 4,
              cursor: 'col-resize',
              background: 'transparent'
            }}
            onMouseDown={(e) => handleMouseDown(e, column)}
          />
        </div>
      ))}
    </div>
  );
};

export interface EnhancedVirtualTableProps {
  data: any[];
  columns: DataTableColumn[];
  keyField: string;
  height?: number;
  config?: VirtualizationConfig;
  onRowClick?: (record: any, index: number) => void;
  onCellClick?: (value: any, record: any, column: DataTableColumn, index: number) => void;
  rowSelection?: any;
  rowStyle?: React.CSSProperties | ((record: any, index: number) => React.CSSProperties);
  onSort?: (column: DataTableColumn) => void;
  sortConfig?: Array<{ column: string; order: 'asc' | 'desc' }>;
  onColumnResize?: (column: string, width: number) => void;
  onColumnReorder?: (columns: DataTableColumn[]) => void;
}

export const EnhancedVirtualTable: React.FC<EnhancedVirtualTableProps> = ({
  data,
  columns,
  keyField,
  height = 400,
  config = {},
  onRowClick,
  onCellClick,
  rowSelection,
  rowStyle,
  onSort,
  sortConfig,
  onColumnResize,
  onColumnReorder
}) => {
  const defaultConfig: VirtualizationConfig = {
    itemSize: 40,
    overscan: 5,
    threshold: 100,
    cacheSize: 50,
    buffer: 10,
    scrollBehavior: 'smooth',
    ...config
  };

  const shouldVirtualize = data.length >= defaultConfig.threshold!;
  
  const itemData = useMemo(() => ({
    items: data,
    columns,
    keyField,
    onRowClick,
    rowSelection,
    rowStyle,
    onCellClick
  }), [data, columns, keyField, onRowClick, rowSelection, rowStyle, onCellClick]);

  const getItemSize = useCallback((index: number) => {
    if (!defaultConfig.variableSize) return defaultConfig.itemSize;
    
    // Calculate dynamic size based on content
    const record = data[index];
    let maxLines = 1;
    
    columns.forEach(column => {
      const value = record?.[column.dataIndex];
      if (typeof value === 'string') {
        const lines = Math.ceil(value.length / 30); // Rough estimation
        maxLines = Math.max(maxLines, lines);
      }
    });
    
    return Math.max(defaultConfig.itemSize, maxLines * 20 + 16);
  }, [data, columns, defaultConfig]);

  if (!shouldVirtualize) {
    // Render regular table for small datasets
    return (
      <div style={{ height, overflow: 'auto' }}>
        <VirtualizedHeader
          columns={columns}
          rowSelection={rowSelection}
          onSort={onSort}
          sortConfig={sortConfig}
          onColumnResize={onColumnResize}
          onColumnReorder={onColumnReorder}
        />
        
        {data.map((record, index) => (
          <VirtualizedRow
            key={record[keyField]}
            index={index}
            style={{ height: defaultConfig.itemSize }}
            data={itemData}
          />
        ))}
      </div>
    );
  }

  return (
    <div style={{ height }}>
      <VirtualizedHeader
        columns={columns}
        rowSelection={rowSelection}
        onSort={onSort}
        sortConfig={sortConfig}
        onColumnResize={onColumnResize}
        onColumnReorder={onColumnReorder}
      />
      
      <AutoSizer>
        {({ width, height: autoHeight }) => (
          defaultConfig.variableSize ? (
            <VariableSizeList
              height={autoHeight - 42} // Subtract header height
              width={width}
              itemCount={data.length}
              itemSize={getItemSize}
              itemData={itemData}
              overscanCount={defaultConfig.overscan}
            >
              {VirtualizedRow}
            </VariableSizeList>
          ) : (
            <List
              height={autoHeight - 42} // Subtract header height
              width={width}
              itemCount={data.length}
              itemSize={defaultConfig.itemSize}
              itemData={itemData}
              overscanCount={defaultConfig.overscan}
            >
              {VirtualizedRow}
            </List>
          )
        )}
      </AutoSizer>
    </div>
  );
};

// Hook for virtualization performance monitoring
export const useVirtualizationMetrics = (enabled: boolean = false) => {
  const metricsRef = useRef({
    renderCount: 0,
    scrollEvents: 0,
    lastRenderTime: 0,
    averageRenderTime: 0
  });

  const startRender = useCallback(() => {
    if (!enabled) return () => {};
    
    const startTime = performance.now();
    
    return () => {
      const endTime = performance.now();
      const renderTime = endTime - startTime;
      
      metricsRef.current.renderCount++;
      metricsRef.current.lastRenderTime = renderTime;
      metricsRef.current.averageRenderTime = 
        (metricsRef.current.averageRenderTime + renderTime) / 2;
    };
  }, [enabled]);

  const recordScrollEvent = useCallback(() => {
    if (enabled) {
      metricsRef.current.scrollEvents++;
    }
  }, [enabled]);

  const getMetrics = useCallback(() => {
    return { ...metricsRef.current };
  }, []);

  const resetMetrics = useCallback(() => {
    metricsRef.current = {
      renderCount: 0,
      scrollEvents: 0,
      lastRenderTime: 0,
      averageRenderTime: 0
    };
  }, []);

  return {
    startRender,
    recordScrollEvent,
    getMetrics,
    resetMetrics
  };
};
