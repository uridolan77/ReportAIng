import React, { useState } from 'react';
import {
  Card,
  Typography,
  Space,
  Descriptions,
  Table,
  Tag,
  Button,
  Tabs,
  Tooltip,
  message
} from 'antd';
import {
  TableOutlined,
  KeyOutlined,
  LinkOutlined,
  CopyOutlined,
  DatabaseOutlined
} from '@ant-design/icons';
import { DatabaseTable, DatabaseColumn } from '../../types/dbExplorer';

const { Title, Text, Paragraph } = Typography;
const { TabPane } = Tabs;

interface TableExplorerProps {
  table: DatabaseTable;
  onPreviewData?: () => void;
  onGenerateQuery?: (query: string) => void;
  // Selection mode props
  selectionMode?: boolean;
  selectedFields?: Set<string>;
  onFieldSelectionChange?: (fieldName: string, selected: boolean) => void;
  onSelectAllColumns?: () => void;
  onClearAllColumns?: () => void;
  // Table selection props
  isTableSelected?: boolean;
  onTableSelectionChange?: (selected: boolean) => void;
}

export const TableExplorer: React.FC<TableExplorerProps> = ({
  table,
  onPreviewData,
  onGenerateQuery,
  selectionMode = false,
  selectedFields = new Set(),
  onFieldSelectionChange,
  onSelectAllColumns,
  onClearAllColumns,
  isTableSelected = false,
  onTableSelectionChange
}) => {
  const [activeTab, setActiveTab] = useState('columns');

  // Debug logging
  console.log('TableExplorer render:', {
    tableName: table.name,
    selectionMode,
    selectedFieldsCount: selectedFields.size,
    selectedFieldsArray: Array.from(selectedFields),
    isTableSelected
  });

  // Check if all columns are selected
  const allColumnsSelected = table.columns.length > 0 && table.columns.every(col => selectedFields.has(col.name));
  const someColumnsSelected = table.columns.some(col => selectedFields.has(col.name));

  // Handle select all columns
  const handleSelectAllChange = (checked: boolean) => {
    if (checked) {
      onSelectAllColumns?.();
    } else {
      onClearAllColumns?.();
    }
  };

  // Column definitions for the columns table
  const columnTableColumns = [
    ...(selectionMode ? [{
      title: (
        <div style={{ display: 'flex', alignItems: 'center', gap: '8px' }}>
          <div
            className={`selection-indicator table-indicator ${
              allColumnsSelected ? 'field-selected' : someColumnsSelected ? 'partial' : 'unselected'
            }`}
            onClick={() => handleSelectAllChange(!allColumnsSelected)}
          >
            {allColumnsSelected && (
              <div style={{
                width: '6px',
                height: '6px',
                backgroundColor: 'white',
                borderRadius: '1px'
              }} />
            )}
            {!allColumnsSelected && someColumnsSelected && (
              <div style={{
                width: '4px',
                height: '2px',
                backgroundColor: '#faad14',
                borderRadius: '1px'
              }} />
            )}
          </div>
          <span>Select All</span>
        </div>
      ),
      key: 'select',
      width: 100,
      render: (_: any, record: DatabaseColumn) => {
        const isSelected = selectedFields.has(record.name);
        console.log(`Column ${record.name} selected:`, isSelected, 'selectedFields:', Array.from(selectedFields));
        return (
          <div
            className={`selection-indicator field-indicator ${
              isSelected ? 'field-selected' : 'unselected'
            }`}
            style={{
              width: '12px',
              height: '12px',
              border: '1px solid',
              borderColor: isSelected ? '#52c41a' : '#d9d9d9',
              backgroundColor: isSelected ? '#52c41a' : 'transparent',
              borderRadius: '2px',
              display: 'flex',
              alignItems: 'center',
              justifyContent: 'center',
              cursor: 'pointer',
              transition: 'all 0.2s ease'
            }}
            onClick={() => {
              console.log(`Clicking column ${record.name}, current state:`, isSelected);
              onFieldSelectionChange?.(record.name, !isSelected);
            }}
          >
            {isSelected && (
              <div style={{
                width: '6px',
                height: '6px',
                backgroundColor: 'white',
                borderRadius: '1px'
              }} />
            )}
          </div>
        );
      }
    }] : []),
    {
      title: 'Column Name',
      dataIndex: 'name',
      key: 'name',
      render: (name: string, record: DatabaseColumn) => (
        <Space>
          <Text>{name}</Text>
          {record.isPrimaryKey && (
            <Tooltip title="Primary Key">
              <KeyOutlined style={{ color: '#faad14' }} />
            </Tooltip>
          )}
          {record.isForeignKey && (
            <Tooltip title={`Foreign Key → ${record.referencedTable}.${record.referencedColumn}`}>
              <LinkOutlined style={{ color: '#52c41a' }} />
            </Tooltip>
          )}
        </Space>
      )
    },
    {
      title: 'Data Type',
      dataIndex: 'dataType',
      key: 'dataType',
      render: (dataType: string, record: DatabaseColumn) => (
        <Text code>
          {dataType}
          {record.maxLength && `(${record.maxLength})`}
          {record.precision && record.scale && `(${record.precision},${record.scale})`}
        </Text>
      )
    },
    {
      title: 'Nullable',
      dataIndex: 'isNullable',
      key: 'isNullable',
      render: (isNullable: boolean) => (
        <Tag color={isNullable ? 'green' : 'red'}>
          {isNullable ? 'YES' : 'NO'}
        </Tag>
      )
    },
    {
      title: 'Default',
      dataIndex: 'defaultValue',
      key: 'defaultValue',
      render: (defaultValue: string) => (
        defaultValue ? <Text code>{defaultValue}</Text> : <Text type="secondary">-</Text>
      )
    },
    {
      title: 'Description',
      dataIndex: 'description',
      key: 'description',
      render: (description: string) => (
        description ? <Text>{description}</Text> : <Text type="secondary">-</Text>
      )
    }
  ];

  // Foreign key table columns
  const foreignKeyColumns = [
    {
      title: 'Column',
      dataIndex: 'column',
      key: 'column',
      render: (column: string) => <Text>{column}</Text>
    },
    {
      title: 'Referenced Table',
      dataIndex: 'referencedTable',
      key: 'referencedTable',
      render: (referencedTable: string) => <Text code>{referencedTable}</Text>
    },
    {
      title: 'Referenced Column',
      dataIndex: 'referencedColumn',
      key: 'referencedColumn',
      render: (referencedColumn: string) => <Text code>{referencedColumn}</Text>
    },
    {
      title: 'On Delete',
      dataIndex: 'onDelete',
      key: 'onDelete',
      render: (onDelete: string) => onDelete ? <Tag>{onDelete}</Tag> : <Text type="secondary">-</Text>
    },
    {
      title: 'On Update',
      dataIndex: 'onUpdate',
      key: 'onUpdate',
      render: (onUpdate: string) => onUpdate ? <Tag>{onUpdate}</Tag> : <Text type="secondary">-</Text>
    }
  ];

  // Generate sample queries
  const generateSampleQueries = () => {
    const tableName = table.name;
    const primaryKey = table.primaryKeys?.[0] || table.columns[0]?.name;
    
    return [
      {
        title: 'Select All',
        query: `SELECT * FROM ${tableName}`,
        description: 'Get all columns and rows from the table'
      },
      {
        title: 'Select Top 10',
        query: `SELECT TOP 10 * FROM ${tableName}`,
        description: 'Get first 10 rows from the table'
      },
      {
        title: 'Count Rows',
        query: `SELECT COUNT(*) as total_rows FROM ${tableName}`,
        description: 'Count total number of rows'
      },
      ...(primaryKey ? [{
        title: 'Select by Primary Key',
        query: `SELECT * FROM ${tableName} WHERE ${primaryKey} = ?`,
        description: 'Select specific row by primary key'
      }] : [])
    ];
  };

  const handleCopyQuery = (query: string) => {
    navigator.clipboard.writeText(query);
    message.success('Query copied to clipboard');
  };

  const handleUseQuery = (query: string) => {
    onGenerateQuery?.(query);
    message.success('Query sent to query interface');
  };

  return (
    <Card
      title={
        <Space size="small" style={{ display: 'flex', alignItems: 'center', width: '100%' }}>
          {selectionMode && (
            <div
              className={`selection-indicator table-indicator ${
                isTableSelected ? 'selected' : selectedFields.size > 0 ? 'partial' : 'unselected'
              }`}
              style={{ marginRight: '6px' }}
              onClick={() => onTableSelectionChange?.(!isTableSelected)}
            >
              {isTableSelected && (
                <div style={{
                  width: '6px',
                  height: '6px',
                  backgroundColor: 'white',
                  borderRadius: '1px'
                }} />
              )}
              {!isTableSelected && selectedFields.size > 0 && (
                <div style={{
                  width: '4px',
                  height: '2px',
                  backgroundColor: '#faad14',
                  borderRadius: '1px'
                }} />
              )}
            </div>
          )}
          <TableOutlined style={{ fontSize: '12px' }} />
          <span style={{ fontSize: '12px', fontWeight: 500 }}>
            {table.name}
          </span>
          {table.type === 'view' && <Tag color="purple" style={{ fontSize: '9px', padding: '0 4px' }}>VIEW</Tag>}
          {selectionMode && selectedFields.size > 0 && (
            <Tag color="blue" style={{ fontSize: '9px', padding: '1px 6px', marginLeft: '6px' }}>
              {selectedFields.size}/{table.columns.length} columns
            </Tag>
          )}
        </Space>
      }
      headStyle={{
        fontSize: '12px',
        fontWeight: 'normal',
        padding: '6px 12px',
        minHeight: '40px'
      }}
      extra={
        <Space size="small">
          <Button
            type="primary"
            icon={<DatabaseOutlined style={{ fontSize: '11px' }} />}
            onClick={onPreviewData}
            size="small"
            style={{ fontSize: '11px' }}
          >
            Preview Data
          </Button>
        </Space>
      }
      style={{ height: '100%', display: 'flex', flexDirection: 'column' }}
      styles={{ body: { padding: '12px', flex: 1, overflow: 'hidden', display: 'flex', flexDirection: 'column' } }}
      className="table-info-container"
    >
      <Space direction="vertical" style={{ width: '100%', height: '100%', display: 'flex', flexDirection: 'column' }} size="small">
        {/* Table Information */}
        <Descriptions
          title={<span style={{ fontSize: '11px', fontWeight: 500 }}>Table Information</span>}
          bordered
          size="small"
          column={2}
          style={{ fontSize: '10px' }}
        >
          <Descriptions.Item label="Schema">{table.schema}</Descriptions.Item>
          <Descriptions.Item label="Type">{table.type}</Descriptions.Item>
          <Descriptions.Item label="Columns">{table.columns?.length || 0}</Descriptions.Item>
          <Descriptions.Item label="Row Count">
            {table.rowCount !== undefined ? table.rowCount.toLocaleString() : 'Unknown'}
          </Descriptions.Item>
          <Descriptions.Item label="Primary Keys" span={2}>
            {table.primaryKeys?.length ? (
              <Space wrap>
                {table.primaryKeys.map(key => (
                  <Tag key={key} color="gold" icon={<KeyOutlined />}>
                    {key}
                  </Tag>
                ))}
              </Space>
            ) : (
              <Text type="secondary">None</Text>
            )}
          </Descriptions.Item>
          {table.description && (
            <Descriptions.Item label="Description" span={2}>
              {table.description}
            </Descriptions.Item>
          )}
        </Descriptions>

        {/* Detailed Information Tabs */}
        <Tabs
          activeKey={activeTab}
          onChange={setActiveTab}
          style={{
            fontSize: '11px',
            flex: 1,
            display: 'flex',
            flexDirection: 'column',
            overflow: 'hidden'
          }}
          size="small"
        >
          <TabPane
            tab={
              <span style={{ fontSize: '11px' }}>
                Columns
                {selectionMode && selectedFields.size > 0 && (
                  <Tag color="blue" style={{ marginLeft: '4px', fontSize: '9px' }}>
                    {selectedFields.size} selected
                  </Tag>
                )}
              </span>
            }
            key="columns"
          >
            <Table
              dataSource={table.columns || []}
              columns={columnTableColumns}
              rowKey="name"
              size="small"
              pagination={false}
              scroll={{ y: 'calc(100vh - 300px)' }}
              style={{ fontSize: '11px' }}
            />
          </TabPane>

          {table.foreignKeys && table.foreignKeys.length > 0 && (
            <TabPane tab={<span style={{ fontSize: '11px' }}>{`Foreign Keys (${table.foreignKeys.length})`}</span>} key="foreignKeys">
              <Table
                dataSource={table.foreignKeys}
                columns={foreignKeyColumns}
                rowKey="name"
                size="small"
                pagination={false}
                scroll={{ y: 'calc(100vh - 300px)' }}
                style={{ fontSize: '11px' }}
              />
            </TabPane>
          )}

          <TabPane tab={<span style={{ fontSize: '11px' }}>Sample Queries</span>} key="queries">
            <Space direction="vertical" style={{ width: '100%' }} size="middle">
              {generateSampleQueries().map((queryInfo, index) => (
                <Card key={index} size="small">
                  <Space direction="vertical" style={{ width: '100%' }}>
                    <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                      <Text>{queryInfo.title}</Text>
                      <Space>
                        <Button
                          type="text"
                          size="small"
                          icon={<CopyOutlined />}
                          onClick={() => handleCopyQuery(queryInfo.query)}
                        />
                        <Button
                          type="primary"
                          size="small"
                          onClick={() => handleUseQuery(queryInfo.query)}
                        >
                          Use Query
                        </Button>
                      </Space>
                    </div>
                    <Paragraph
                      code
                      copyable
                      style={{ 
                        margin: 0, 
                        backgroundColor: '#f5f5f5', 
                        padding: '8px',
                        borderRadius: '4px'
                      }}
                    >
                      {queryInfo.query}
                    </Paragraph>
                    <Text type="secondary" style={{ fontSize: '12px' }}>
                      {queryInfo.description}
                    </Text>
                  </Space>
                </Card>
              ))}
            </Space>
          </TabPane>
        </Tabs>
      </Space>
    </Card>
  );
};
