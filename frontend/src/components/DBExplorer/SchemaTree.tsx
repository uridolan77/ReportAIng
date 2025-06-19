import React, { useState, useMemo } from 'react';
import {
  Tree,
  Input,
  Card,
  Typography,
  Space,
  Tag,
  Tooltip,
  Button,
  Spin,
  Empty
} from 'antd';
import {
  DatabaseOutlined,
  TableOutlined,
  KeyOutlined,
  SearchOutlined,
  ReloadOutlined,
  EyeOutlined
} from '@ant-design/icons';
import type { DataNode } from 'antd/es/tree';
import { DatabaseTable, DatabaseColumn } from '../../types/dbExplorer';

const { Search } = Input;
const { Text } = Typography;

interface SchemaTreeProps {
  tables: DatabaseTable[];
  loading?: boolean;
  onTableSelect?: (table: DatabaseTable) => void;
  onColumnSelect?: (table: DatabaseTable, column: DatabaseColumn) => void;
  onPreviewTable?: (table: DatabaseTable) => void;
  onRefresh?: () => void;
  selectedTableName?: string;
  expandedKeys?: string[];
  onExpandedKeysChange?: (keys: string[]) => void;
  // Selection mode props
  selectionMode?: boolean;
  selectedTables?: Set<string>;
  selectedFields?: Map<string, Set<string>>;
  onTableSelectionChange?: (tableName: string, selected: boolean) => void;
  onFieldSelectionChange?: (tableName: string, fieldName: string, selected: boolean) => void;
}

export const SchemaTree: React.FC<SchemaTreeProps> = ({
  tables,
  loading = false,
  onTableSelect,
  onColumnSelect,
  onPreviewTable,
  onRefresh,
  selectedTableName,
  expandedKeys = [],
  onExpandedKeysChange,
  selectionMode = false,
  selectedTables = new Set(),
  selectedFields = new Map(),
  onTableSelectionChange,
  onFieldSelectionChange
}) => {
  const [searchValue, setSearchValue] = useState('');
  const [autoExpandParent, setAutoExpandParent] = useState(true);

  // Generate tree data from tables
  const treeData = useMemo(() => {
    const filteredTables = (tables || []).filter(table => {
      if (!searchValue) return true;
      const searchLower = searchValue.toLowerCase();
      
      return (
        table.name.toLowerCase().includes(searchLower) ||
        table.description?.toLowerCase().includes(searchLower) ||
        (table.columns || []).some(col =>
          col.name.toLowerCase().includes(searchLower) ||
          col.description?.toLowerCase().includes(searchLower)
        )
      );
    });

    return filteredTables.map(table => {
      const tableKey = `table-${table.name}`;
      const isTableSelected = selectedTables.has(table.name);
      const tableFields = selectedFields.get(table.name) || new Set();
      const someColumnsSelected = table.columns.some(col => tableFields.has(col.name));

      return {
        title: (
          <div style={{
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'space-between',
            width: '100%',
            padding: '6px 8px',
            borderRadius: '6px',
            background: isTableSelected ? '#e6f7ff' : 'transparent',
            border: isTableSelected ? '1px solid #91d5ff' : '1px solid transparent',
            transition: 'all 0.2s ease',
            minHeight: '32px'
          }}>
            <Space size="small" style={{ flex: 1, alignItems: 'center' }}>
              {selectionMode && (
                <div
                  className={`selection-indicator table-indicator ${
                    isTableSelected ? 'selected' : someColumnsSelected ? 'partial' : 'unselected'
                  }`}
                  style={{ marginRight: '8px' }}
                  onClick={(e) => {
                    e.stopPropagation();
                    onTableSelectionChange?.(table.name, !isTableSelected);
                  }}
                >
                  {isTableSelected && (
                    <div style={{
                      width: '8px',
                      height: '8px',
                      backgroundColor: 'white',
                      borderRadius: '1px'
                    }} />
                  )}
                  {!isTableSelected && someColumnsSelected && (
                    <div style={{
                      width: '6px',
                      height: '2px',
                      backgroundColor: '#faad14',
                      borderRadius: '1px'
                    }} />
                  )}
                </div>
              )}
              <TableOutlined style={{ color: '#1890ff', fontSize: '16px', marginRight: '4px' }} />
              <Text strong style={{ fontSize: '14px', fontWeight: 600, color: '#262626' }}>{table.name}</Text>
              {table.type === 'view' && (
                <Tag color="purple" style={{
                  fontSize: '10px',
                  padding: '1px 6px',
                  lineHeight: '16px',
                  borderRadius: '4px',
                  marginLeft: '6px'
                }}>
                  VIEW
                </Tag>
              )}
              {selectionMode && tableFields.size > 0 && (
                <Tag color="blue" style={{
                  fontSize: '10px',
                  padding: '1px 6px',
                  lineHeight: '16px',
                  borderRadius: '4px',
                  marginLeft: '6px'
                }}>
                  {tableFields.size}/{table.columns.length} cols
                </Tag>
              )}
              {table.rowCount !== undefined && (
                <Text type="secondary" style={{
                  fontSize: '12px',
                  fontWeight: 400,
                  marginLeft: '6px',
                  color: '#8c8c8c'
                }}>
                  ({table.rowCount.toLocaleString()} rows)
                </Text>
              )}
            </Space>
            <Button
              type="text"
              size="small"
              icon={<EyeOutlined style={{ fontSize: '12px' }} />}
              onClick={(e) => {
                e.stopPropagation();
                onPreviewTable?.(table);
              }}
              style={{
                opacity: 0.7,
                padding: '4px 6px',
                height: '24px',
                borderRadius: '4px',
                fontSize: '11px',
                marginLeft: '8px'
              }}
              onMouseEnter={(e) => {
                e.currentTarget.style.opacity = '1';
                e.currentTarget.style.background = '#f0f0f0';
              }}
              onMouseLeave={(e) => {
                e.currentTarget.style.opacity = '0.7';
                e.currentTarget.style.background = 'transparent';
              }}
            />
          </div>
        ),
        key: tableKey,
        icon: <TableOutlined style={{ fontSize: '16px', color: '#1890ff' }} />,
        children: (table.columns || []).map(column => {
          const tableFields = selectedFields.get(table.name) || new Set();
          const isFieldSelected = tableFields.has(column.name);

          return {
            title: (
              <div style={{
                display: 'flex',
                alignItems: 'center',
                justifyContent: 'space-between',
                width: '100%',
                padding: '4px 8px',
                borderRadius: '4px',
                background: isFieldSelected ? '#f6ffed' : 'transparent',
                border: isFieldSelected ? '1px solid #b7eb8f' : '1px solid transparent',
                transition: 'all 0.2s ease',
                minHeight: '28px',
                marginLeft: '8px'
              }}>
                <Space size={6} style={{ flex: 1, alignItems: 'center' }}>
                  {selectionMode && (
                    <div
                      className={`selection-indicator field-indicator ${
                        isFieldSelected ? 'field-selected' : 'unselected'
                      }`}
                      style={{ marginRight: '6px' }}
                      onClick={(e) => {
                        e.stopPropagation();
                        onFieldSelectionChange?.(table.name, column.name, !isFieldSelected);
                      }}
                    >
                      {isFieldSelected && (
                        <div style={{
                          width: '6px',
                          height: '6px',
                          backgroundColor: 'white',
                          borderRadius: '1px'
                        }} />
                      )}
                    </div>
                  )}
                  <Text style={{
                    fontSize: '13px',
                    fontWeight: 500,
                    color: '#262626',
                    minWidth: '80px'
                  }}>
                    {column.name}
                  </Text>
                  <Text type="secondary" style={{
                    fontSize: '11px',
                    fontFamily: 'Monaco, Consolas, "Courier New", monospace',
                    color: '#722ed1',
                    background: '#f9f0ff',
                    padding: '2px 6px',
                    borderRadius: '3px',
                    border: '1px solid #d3adf7'
                  }}>
                    {column.dataType}
                    {column.maxLength && `(${column.maxLength})`}
                  </Text>
                  {column.isPrimaryKey && (
                    <Tooltip title="Primary Key">
                      <KeyOutlined style={{
                        color: '#faad14',
                        fontSize: '12px',
                        background: '#fff7e6',
                        padding: '2px',
                        borderRadius: '2px',
                        border: '1px solid #ffd591'
                      }} />
                    </Tooltip>
                  )}
                  {column.isForeignKey && (
                    <Tooltip title={`Foreign Key → ${column.referencedTable}.${column.referencedColumn}`}>
                      <KeyOutlined style={{
                        color: '#52c41a',
                        fontSize: '12px',
                        background: '#f6ffed',
                        padding: '2px',
                        borderRadius: '2px',
                        border: '1px solid #b7eb8f'
                      }} />
                    </Tooltip>
                  )}
                  {!column.isNullable && (
                    <Tag color="red" style={{
                      fontSize: '9px',
                      padding: '1px 4px',
                      lineHeight: '14px',
                      borderRadius: '3px',
                      fontWeight: 500
                    }}>
                      NN
                    </Tag>
                  )}
                </Space>
              </div>
            ),
            key: `column-${table.name}-${column.name}`,
            icon: column.isPrimaryKey ?
              <KeyOutlined style={{ color: '#faad14', fontSize: '12px' }} /> :
              <div style={{ width: '8px', height: '8px', backgroundColor: '#d9d9d9', borderRadius: '2px', margin: '0 2px' }} />,
            isLeaf: true,
            data: { table, column }
          };
        }),
        data: { table }
      } as DataNode;
    });
  }, [tables, searchValue, onPreviewTable, selectionMode, selectedTables, selectedFields, onTableSelectionChange, onFieldSelectionChange]);

  // Handle search
  const handleSearch = (value: string) => {
    setSearchValue(value);
    if (value) {
      // Auto-expand all nodes when searching
      const allKeys = treeData.flatMap(node => [
        node.key as string,
        ...(node.children?.map(child => child.key as string) || [])
      ]);
      onExpandedKeysChange?.(allKeys);
      setAutoExpandParent(true);
    }
  };

  // Handle tree node selection
  const handleSelect = (selectedKeys: React.Key[], info: any) => {
    console.log('Tree node selected:', { selectedKeys, nodeData: info.node });
    if (selectedKeys.length === 0) return;

    const selectedKey = selectedKeys[0] as string;
    const { data } = info.node;

    console.log('Selected key:', selectedKey, 'Node data:', data);

    if (selectedKey.startsWith('table-') && data?.table) {
      console.log('Calling onTableSelect with:', data.table);
      onTableSelect?.(data.table);
    } else if (selectedKey.startsWith('column-') && data?.table && data?.column) {
      console.log('Calling onColumnSelect with:', data.table, data.column);
      onColumnSelect?.(data.table, data.column);
    }
  };

  // Handle tree expansion
  const handleExpand = (expandedKeys: React.Key[]) => {
    onExpandedKeysChange?.(expandedKeys as string[]);
    setAutoExpandParent(false);
  };

  return (
    <Card
      title={
        <Space size="small" style={{ alignItems: 'center' }}>
          <DatabaseOutlined style={{ fontSize: '15px', color: '#1890ff' }} />
          <span style={{ fontSize: '15px', fontWeight: 600 }}>Database Schema</span>
          <Text type="secondary" style={{ fontSize: '13px' }}>({(tables || []).length} tables)</Text>
        </Space>
      }
      extra={
        <Button
          type="text"
          icon={<ReloadOutlined style={{ fontSize: '13px' }} />}
          onClick={onRefresh}
          loading={loading}
          size="small"
          style={{ padding: '4px 8px', height: '28px' }}
        />
      }
      size="small"
      style={{
        height: '100%',
        borderRadius: '8px',
        border: '1px solid #e8e8e8'
      }}
      styles={{
        body: {
          padding: '12px',
          height: 'calc(100% - 55px)',
          overflow: 'hidden'
        },
        header: {
          padding: '10px 16px',
          minHeight: '55px',
          background: 'linear-gradient(135deg, #fafafa 0%, #f5f5f5 100%)',
          borderBottom: '1px solid #e8e8e8'
        }
      }}
      className="schema-tree-container"
    >
      <div style={{ width: '100%', height: '100%', display: 'flex', flexDirection: 'column' }}>
        <Search
          placeholder="Search tables and columns..."
          allowClear
          onChange={(e) => handleSearch(e.target.value)}
          style={{ marginBottom: '12px', flexShrink: 0 }}
          prefix={<SearchOutlined />}
          size="middle"
        />

        <div style={{ flex: 1, overflow: 'auto', paddingRight: '4px', minHeight: 0 }} className="schema-tree-scroll">
          {loading ? (
            <div style={{ textAlign: 'center', padding: '40px 20px' }}>
              <Spin size="large" />
              <div style={{ marginTop: '12px' }}>
                <Text type="secondary" style={{ fontSize: '13px' }}>Loading schema...</Text>
              </div>
            </div>
          ) : treeData.length === 0 ? (
            <Empty
              description={
                <Text type="secondary" style={{ fontSize: '13px' }}>
                  {searchValue ? 'No matching tables found' : 'No tables found'}
                </Text>
              }
              image={Empty.PRESENTED_IMAGE_SIMPLE}
              style={{ marginTop: '40px' }}
            />
          ) : (
            <Tree
              treeData={treeData}
              onSelect={handleSelect}
              onExpand={handleExpand}
              expandedKeys={expandedKeys}
              autoExpandParent={autoExpandParent}
              selectedKeys={selectedTableName ? [`table-${selectedTableName}`] : []}
              showIcon
              blockNode
              style={{ fontSize: '14px' }}
              className="schema-tree"
            />
          )}
        </div>
      </div>
    </Card>
  );
};
