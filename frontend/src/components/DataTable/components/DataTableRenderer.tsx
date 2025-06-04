import React from 'react';
import { Table } from 'antd';
import { VariableSizeGrid as Grid } from 'react-window';
import AutoSizer from 'react-virtualized-auto-sizer';

interface DataTableColumn {
  key: string;
  title: string;
  dataIndex: string;
  width?: number;
  render?: (value: any, record: any, index: number) => React.ReactNode;
  onHeaderCell?: () => any;
}

interface VirtualizedTableProps {
  data: any[];
  columns: DataTableColumn[];
  keyField: string;
  height: number;
  width: number;
  onRowClick?: (record: any, index: number) => void;
  rowSelection?: any;
  rowStyle?: React.CSSProperties | ((record: any, index: number) => React.CSSProperties);
}

interface StandardTableProps {
  data: any[];
  columns: DataTableColumn[];
  keyField: string;
  pagination?: any;
  rowSelection?: any;
  onRow?: (record: any, index: number) => any;
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
  rowSelection,
  rowStyle
}) => {
  const columnCount = columns.length + (rowSelection ? 1 : 0);
  const rowCount = data.length + 1; // +1 for header

  const getColumnWidth = (index: number) => {
    if (rowSelection && index === 0) return 50;
    const columnIndex = rowSelection ? index - 1 : index;
    return columns[columnIndex]?.width || 150;
  };

  const Cell = ({ columnIndex, rowIndex, style }: any) => {
    // Header row
    if (rowIndex === 0) {
      if (rowSelection && columnIndex === 0) {
        return <div style={style}>Select</div>;
      }
      const column = columns[rowSelection ? columnIndex - 1 : columnIndex];
      return (
        <div style={{ ...style, fontWeight: 'bold', background: '#fafafa' }}>
          {column.title}
        </div>
      );
    }

    // Data rows
    const record = data[rowIndex - 1];
    
    if (rowSelection && columnIndex === 0) {
      return (
        <div style={style}>
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
        onClick={() => onRowClick?.(record, rowIndex - 1)}
      >
        {cellContent}
      </div>
    );
  };

  return (
    <Grid
      columnCount={columnCount}
      columnWidth={getColumnWidth}
      height={height}
      rowCount={rowCount}
      rowHeight={() => 40}
      width={width}
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
  style,
  loading,
  summary
}) => {  return (
    <Table
      dataSource={data}
      columns={columns}
      rowKey={keyField}
      pagination={pagination}
      rowSelection={rowSelection}
      onRow={(record, index) => onRow?.(record, index || 0)}
      style={style}
      loading={loading}
      summary={(data: readonly any[]) => summary?.(Array.from(data))}
    />
  );
};
