import React, { useRef, useEffect, useState } from 'react';
import { Table } from 'antd';
import { VariableSizeGrid as Grid } from 'react-window';

import { VirtualizationService, createPerformanceMonitor } from '../services/VirtualizationService';
import { DataTableColumn } from '../types';

interface VirtualizedTableProps {
  data: any[];
  columns: DataTableColumn[];
  keyField: string;
  height: number;
  width: number;
  onRowClick?: (record: any, index: number) => void;
  onContextMenu?: (event: React.MouseEvent, context: any) => void;
  rowSelection?: any;
  rowStyle?: React.CSSProperties | ((record: any, index: number) => React.CSSProperties);
  virtualizationService?: VirtualizationService | null;
  enableDynamicHeight?: boolean;
  enablePerformanceMonitoring?: boolean;
}

interface StandardTableProps {
  data: any[];
  columns: DataTableColumn[];
  keyField: string;
  pagination?: any;
  rowSelection?: any;
  onRow?: (record: any, index: number) => any;
  onContextMenu?: (event: React.MouseEvent, context: any) => void;
  style?: React.CSSProperties;
  loading?: boolean;
  summary?: (data: any[]) => React.ReactNode;
}

export const VirtualizedTable: React.FC<VirtualizedTableProps> = ({
  data,
  columns,
  keyField,
  height,
  width,
  onRowClick,
  onContextMenu,
  rowSelection,
  rowStyle,
  virtualizationService,
  enableDynamicHeight = false,
  enablePerformanceMonitoring = false
}) => {
  const gridRef = useRef<Grid>(null);
  const perfMonitor = useRef(createPerformanceMonitor());
  const [rowHeights, setRowHeights] = useState<Map<number, number>>(new Map());
  
  const columnCount = columns.length + (rowSelection ? 1 : 0);
  const rowCount = data.length + 1; // +1 for header

  // Performance monitoring
  useEffect(() => {
    if (enablePerformanceMonitoring) {
      const startTime = perfMonitor.current.startRender();
      const currentPerfMonitor = perfMonitor.current;
      return () => {
        currentPerfMonitor.endRender(startTime, data.length);
      };
    }
  }, [data.length, enablePerformanceMonitoring]);

  const getColumnWidth = (index: number) => {
    if (rowSelection && index === 0) return 50;
    const columnIndex = rowSelection ? index - 1 : index;
    return columns[columnIndex]?.width || 150;
  };

  const getRowHeight = (index: number) => {
    if (enableDynamicHeight && rowHeights.has(index)) {
      return rowHeights.get(index) || 40;
    }
    if (virtualizationService) {
      return virtualizationService.getItemSize(index);
    }
    return 40;
  };

  const measureRowHeight = (index: number, element: HTMLDivElement) => {
    if (enableDynamicHeight && element) {
      const height = element.getBoundingClientRect().height;
      setRowHeights(prev => new Map(prev.set(index, height)));
      virtualizationService?.setItemSize(index, height);
    }
  };

  const Cell = ({ columnIndex, rowIndex, style }: any) => {
    const cellRef = useRef<HTMLDivElement>(null);

    useEffect(() => {
      if (cellRef.current && columnIndex === 0) {
        measureRowHeight(rowIndex, cellRef.current);
      }
    }, [columnIndex, rowIndex]);    // Header row
    if (rowIndex === 0) {
      if (rowSelection && columnIndex === 0) {
        return (
          <div 
            ref={cellRef}
            style={{ ...style, fontWeight: 'bold', background: '#fafafa' }}
            onContextMenu={(e) => {
              onContextMenu?.(e, {
                type: 'header',
                context: 'selection',
                column: null,
                rowIndex: null,
                record: null
              });
            }}
          >
            Select
          </div>
        );
      }
      const column = columns[rowSelection ? columnIndex - 1 : columnIndex];
      return (
        <div 
          ref={cellRef}
          style={{ ...style, fontWeight: 'bold', background: '#fafafa' }}
          onContextMenu={(e) => {
            onContextMenu?.(e, {
              type: 'header',
              context: 'column',
              column: column,
              rowIndex: null,
              record: null
            });
          }}
        >
          {column.title}
        </div>
      );
    }    // Data rows
    const record = data[rowIndex - 1];
    
    if (rowSelection && columnIndex === 0) {
      return (
        <div 
          ref={cellRef} 
          style={style}
          onContextMenu={(e) => {
            onContextMenu?.(e, {
              type: 'cell',
              context: 'selection',
              column: null,
              rowIndex: rowIndex - 1,
              record: record
            });
          }}
        >
          <input 
            type={rowSelection.type || 'checkbox'} 
            checked={rowSelection.selectedRowKeys?.includes(record[keyField])}
            onChange={() => {
              // Handle selection change
            }}
          />
        </div>
      );
    }

    const column = columns[rowSelection ? columnIndex - 1 : columnIndex];
    const value = record[column.dataIndex];
    const cellContent = column.render 
      ? column.render(value, record, rowIndex - 1)
      : value;

    return (
      <div 
        ref={cellRef}
        style={{
          ...style,
          ...(typeof rowStyle === 'function' ? rowStyle(record, rowIndex - 1) : rowStyle),
          borderBottom: '1px solid #f0f0f0',
          borderRight: '1px solid #f0f0f0',
          padding: '8px',
          overflow: 'hidden',
          textOverflow: 'ellipsis',
          whiteSpace: 'nowrap'
        }}
        onClick={() => {
          onRowClick?.(record, rowIndex - 1);
          if (enablePerformanceMonitoring) {
            perfMonitor.current.trackScroll();
          }
        }}
        onContextMenu={(e) => {
          onContextMenu?.(e, {
            type: 'cell',
            context: 'data',
            column: column,
            value: value,
            rowIndex: rowIndex - 1,
            record: record
          });
        }}
      >
        {cellContent}
      </div>
    );
  };

  const handleScroll = ({ scrollTop }: { scrollTop: number }) => {
    if (virtualizationService) {
      virtualizationService.handleScroll(scrollTop);
    }
    if (enablePerformanceMonitoring) {
      perfMonitor.current.trackScroll();
    }
  };

  return (
    <Grid
      ref={gridRef}
      columnCount={columnCount}
      columnWidth={getColumnWidth}
      height={height}
      rowCount={rowCount}
      rowHeight={getRowHeight}
      width={width}
      onScroll={handleScroll}
      overscanRowCount={virtualizationService ? 5 : 2}
      overscanColumnCount={2}
    >
      {Cell}
    </Grid>
  );
};

export const StandardTable: React.FC<StandardTableProps> = ({
  data,
  columns,
  keyField,
  pagination,
  rowSelection,
  onRow,
  onContextMenu,
  style,
  loading,
  summary
}) => {
  // Transform columns to add context menu support and sorting
  const enhancedColumns = columns.map(column => ({
    ...column,
    sorter: column.sortable ? {
      compare: (a: any, b: any) => {
        const aVal = a[column.dataIndex];
        const bVal = b[column.dataIndex];
        
        // Handle null/undefined values
        if (aVal == null && bVal == null) return 0;
        if (aVal == null) return -1;
        if (bVal == null) return 1;
        
        // Handle different data types
        if (column.dataType === 'number') {
          return Number(aVal) - Number(bVal);
        } else if (column.dataType === 'date') {
          return new Date(aVal).getTime() - new Date(bVal).getTime();
        } else {
          return String(aVal).localeCompare(String(bVal));
        }
      }
    } : false,
    showSorterTooltip: column.sortable,
    onHeaderCell: () => ({
      onContextMenu: (e: React.MouseEvent) => {
        onContextMenu?.(e, {
          type: 'header',
          context: 'column',
          column: column,
          rowIndex: null,
          record: null
        });
      }
    }),
    onCell: (record: any, rowIndex?: number) => ({
      onContextMenu: (e: React.MouseEvent) => {
        onContextMenu?.(e, {
          type: 'cell',
          context: 'data',
          column: column,
          value: record[column.dataIndex],
          rowIndex: rowIndex || 0,
          record: record
        });
      }
    })
  }));

  return (
    <Table
      dataSource={data}
      columns={enhancedColumns}
      rowKey={keyField}
      pagination={pagination}
      rowSelection={rowSelection}
      onRow={(record, index) => ({
        ...onRow?.(record, index || 0),
        onContextMenu: (e: React.MouseEvent) => {
          onContextMenu?.(e, {
            type: 'row',
            context: 'data',
            column: null,
            rowIndex: index || 0,
            record: record
          });
        }
      })}
      style={style}
      loading={loading}
      summary={(data: readonly any[]) => summary?.(Array.from(data))}
    />
  );
};
