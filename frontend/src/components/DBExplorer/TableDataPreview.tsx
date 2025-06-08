import React, { useState, useEffect } from 'react';
import { 
  Card, 
  Typography, 
  Space, 
  Button, 
  Spin, 
  Alert, 
  Tag,
  Tooltip,
  message
} from 'antd';
import {
  EyeOutlined,
  DownloadOutlined,
  ReloadOutlined,
  InfoCircleOutlined,
  TableOutlined
} from '@ant-design/icons';
import DataTable from '../DataTable';
import type { DataTableColumn } from '../DataTable/types';
import { DatabaseTable, TableDataPreview as TableDataPreviewType } from '../../types/dbExplorer';
import DBExplorerAPI from '../../services/dbExplorerApi';

const { Title, Text } = Typography;

interface TableDataPreviewProps {
  table: DatabaseTable;
  onClose?: () => void;
  maxRows?: number;
}

export const TableDataPreview: React.FC<TableDataPreviewProps> = ({
  table,
  onClose,
  maxRows = 100
}) => {
  const [previewData, setPreviewData] = useState<TableDataPreviewType | null>(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  // Load table data preview
  const loadPreviewData = async () => {
    setLoading(true);
    setError(null);
    
    try {
      const data = await DBExplorerAPI.getTableDataPreview(table.name, {
        limit: maxRows
      });
      setPreviewData(data);
    } catch (err) {
      const errorMessage = err instanceof Error ? err.message : 'Failed to load table data';
      setError(errorMessage);
      message.error(errorMessage);
    } finally {
      setLoading(false);
    }
  };

  // Load data when component mounts or table changes
  useEffect(() => {
    loadPreviewData();
  }, [table.name, maxRows]);

  // Convert database columns to DataTable columns
  const getDataTableColumns = (): DataTableColumn[] => {
    if (!previewData) return [];

    return table.columns.map(column => ({
      key: column.name,
      title: (
        <Space size="small">
          <span>{column.name}</span>
          {column.isPrimaryKey && (
            <Tooltip title="Primary Key">
              <Tag color="gold" size="small">PK</Tag>
            </Tooltip>
          )}
          {column.isForeignKey && (
            <Tooltip title={`Foreign Key → ${column.referencedTable}.${column.referencedColumn}`}>
              <Tag color="green" size="small">FK</Tag>
            </Tooltip>
          )}
          {!column.isNullable && (
            <Tooltip title="Not Nullable">
              <Tag color="red" size="small">NN</Tag>
            </Tooltip>
          )}
        </Space>
      ),
      dataIndex: column.name,
      width: 150,
      ellipsis: true,
      sortable: true,
      filterable: true,
      dataType: mapDatabaseTypeToDataTableType(column.dataType),
      render: (value: any) => {
        if (value === null || value === undefined) {
          return <Text type="secondary" italic>NULL</Text>;
        }
        if (typeof value === 'string' && value.length > 50) {
          return (
            <Tooltip title={value}>
              <Text>{value.substring(0, 50)}...</Text>
            </Tooltip>
          );
        }
        return value;
      }
    }));
  };

  // Map database column types to DataTable types
  const mapDatabaseTypeToDataTableType = (dbType: string): string => {
    const type = dbType.toLowerCase();
    if (type.includes('int') || type.includes('decimal') || type.includes('float') || type.includes('numeric')) {
      return 'number';
    }
    if (type.includes('date') || type.includes('time')) {
      return 'date';
    }
    if (type.includes('bit') || type.includes('bool')) {
      return 'boolean';
    }
    if (type.includes('money') || type.includes('currency')) {
      return 'money';
    }
    return 'string';
  };

  // Handle export
  const handleExport = () => {
    if (!previewData) return;
    
    // Use DataTable's built-in export functionality
    message.info('Export functionality will be handled by the DataTable component');
  };

  return (
    <Card
      title={
        <Space>
          <TableOutlined />
          <Title level={4} style={{ margin: 0 }}>
            {table.name}
          </Title>
          {table.type === 'view' && <Tag color="purple">VIEW</Tag>}
          {previewData && (
            <Text type="secondary">
              Showing {previewData.data.length} of {previewData.totalRows.toLocaleString()} rows
            </Text>
          )}
        </Space>
      }
      extra={
        <Space>
          <Tooltip title="Table Information">
            <Button
              type="text"
              icon={<InfoCircleOutlined />}
              onClick={() => {
                message.info(`Table: ${table.name}\nColumns: ${table.columns.length}\nType: ${table.type}`);
              }}
            />
          </Tooltip>
          <Button
            type="text"
            icon={<ReloadOutlined />}
            onClick={loadPreviewData}
            loading={loading}
          />
          <Button
            type="text"
            icon={<DownloadOutlined />}
            onClick={handleExport}
            disabled={!previewData}
          />
          {onClose && (
            <Button type="primary" onClick={onClose}>
              Close
            </Button>
          )}
        </Space>
      }
      style={{ height: '100%' }}
      bodyStyle={{ padding: '16px', height: 'calc(100% - 57px)', overflow: 'hidden' }}
    >
      {loading && (
        <div style={{ textAlign: 'center', padding: '40px' }}>
          <Spin size="large" />
          <div style={{ marginTop: '16px' }}>
            <Text type="secondary">Loading table data...</Text>
          </div>
        </div>
      )}

      {error && (
        <Alert
          message="Error Loading Data"
          description={error}
          type="error"
          showIcon
          action={
            <Button size="small" onClick={loadPreviewData}>
              Retry
            </Button>
          }
          style={{ marginBottom: '16px' }}
        />
      )}

      {previewData && !loading && (
        <div style={{ height: '100%' }}>
          <DataTable
            data={previewData.data}
            columns={getDataTableColumns()}
            keyField={table.primaryKeys?.[0] || table.columns[0]?.name || 'id'}
            features={{
              pagination: true,
              sorting: true,
              filtering: true,
              searching: true,
              export: true,
              columnChooser: true,
              virtualScroll: previewData.data.length > 100
            }}
            config={{
              pageSize: 25,
              pageSizeOptions: [10, 25, 50, 100],
              maxHeight: 'calc(100vh - 300px)',
              exportFileName: `${table.name}_preview`
            }}
            style={{ height: '100%' }}
          />
        </div>
      )}

      {!previewData && !loading && !error && (
        <div style={{ textAlign: 'center', padding: '40px' }}>
          <EyeOutlined style={{ fontSize: '48px', color: '#d9d9d9' }} />
          <div style={{ marginTop: '16px' }}>
            <Text type="secondary">No data to display</Text>
          </div>
        </div>
      )}
    </Card>
  );
};
