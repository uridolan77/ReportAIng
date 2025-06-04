import React from 'react';
import { Space, Tooltip } from 'antd';
import { 
  SortAscendingOutlined, 
  SortDescendingOutlined, 
  CopyOutlined 
} from '@ant-design/icons';

interface DataTableColumn {
  key: string;
  title: string;
  dataIndex: string;
  sortable?: boolean;
  headerRender?: () => React.ReactNode;
  headerStyle?: React.CSSProperties;
}

interface DataTableHeaderCellProps {
  column: DataTableColumn;
  sortOrder?: 'asc' | 'desc';
  enableSorting: boolean;
  onSort: (column: DataTableColumn) => void;
}

interface DataTableCellProps {
  value: any;
  record: any;
  column: DataTableColumn;
  index: number;
  isEditing: boolean;
  enableCopying: boolean;
  onCellClick?: (value: any, record: any, column: DataTableColumn, index: number) => void;
  onCellDoubleClick?: (value: any, record: any, column: DataTableColumn, index: number) => void;
  cellStyle?: React.CSSProperties | ((value: any, record: any, column: DataTableColumn, index: number) => React.CSSProperties);
  children: React.ReactNode;
}

export const DataTableHeaderCell: React.FC<DataTableHeaderCellProps> = ({
  column,
  sortOrder,
  enableSorting,
  onSort
}) => {
  return (
    <div style={{
      display: 'flex',
      alignItems: 'center',
      justifyContent: 'space-between',
      width: '100%',
      cursor: column.sortable !== false && enableSorting ? 'pointer' : 'default',
      ...column.headerStyle
    }}
    onClick={() => column.sortable !== false && enableSorting && onSort(column)}
    >
      <Space>
        {column.headerRender ? column.headerRender() : column.title}
        {column.sortable !== false && enableSorting && (
          <div style={{ marginLeft: 8 }}>
            {sortOrder === 'asc' && <SortAscendingOutlined />}
            {sortOrder === 'desc' && <SortDescendingOutlined />}
            {!sortOrder && <SortAscendingOutlined style={{ opacity: 0.3 }} />}
          </div>
        )}
      </Space>
    </div>
  );
};

export const DataTableCell: React.FC<DataTableCellProps> = ({
  value,
  record,
  column,
  index,
  isEditing,
  enableCopying,
  onCellClick,
  onCellDoubleClick,
  cellStyle,
  children
}) => {
  const handleCopy = (e: React.MouseEvent) => {
    e.stopPropagation();
    navigator.clipboard.writeText(String(value));
  };

  const cellProps = {
    style: {
      ...(typeof column.cellStyle === 'function'
        ? column.cellStyle(value, record, index)
        : column.cellStyle),
      ...(typeof cellStyle === 'function'
        ? cellStyle(value, record, column, index)
        : cellStyle)
    },
    onClick: () => onCellClick?.(value, record, column, index),
    onDoubleClick: () => onCellDoubleClick?.(value, record, column, index)
  };

  if (isEditing) {
    return <div {...cellProps}>{children}</div>;
  }

  return (
    <div {...cellProps}>
      {column.copyable !== false && enableCopying && (
        <Tooltip title="Copy">
          <CopyOutlined
            style={{ marginRight: 8, cursor: 'pointer', opacity: 0.6 }}
            onClick={handleCopy}
          />
        </Tooltip>
      )}
      {children}
    </div>
  );
};
