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
  Empty,
  Checkbox
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
    const filteredTables = tables.filter(table => {
      if (!searchValue) return true;
      const searchLower = searchValue.toLowerCase();
      
      return (
        table.name.toLowerCase().includes(searchLower) ||
        table.description?.toLowerCase().includes(searchLower) ||
        table.columns.some(col => 
          col.name.toLowerCase().includes(searchLower) ||
          col.description?.toLowerCase().includes(searchLower)
        )
      );
    });

    return filteredTables.map(table => {
      const tableKey = `table-${table.name}`;
      const isTableSelected = selectedTables.has(table.name);

      return {
        title: (
          <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', width: '100%', padding: '2px 0' }}>
            <Space size="small" style={{ flex: 1 }}>
              {selectionMode && (
                <Checkbox
                  checked={isTableSelected}
                  onChange={(e) => {
                    e.stopPropagation();
                    onTableSelectionChange?.(table.name, e.target.checked);
                  }}
                  onClick={(e) => e.stopPropagation()}
                />
              )}
              <TableOutlined style={{ color: '#1890ff', fontSize: '16px' }} />
              <Text strong style={{ fontSize: '14px', fontWeight: 600 }}>{table.name}</Text>
              {table.type === 'view' && <Tag color="purple" style={{ fontSize: '11px', padding: '2px 6px', lineHeight: '16px', borderRadius: '4px' }}>VIEW</Tag>}
              {table.rowCount !== undefined && (
                <Text type="secondary" style={{ fontSize: '12px', fontWeight: 400 }}>
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
              style={{ opacity: 0.7, padding: '4px 6px', height: '24px', borderRadius: '4px', fontSize: '11px' }}
            />
          </div>
        ),
        key: tableKey,
        icon: <TableOutlined style={{ fontSize: '16px', color: '#1890ff' }} />,
        children: table.columns.map(column => {
          const tableFields = selectedFields.get(table.name) || new Set();
          const isFieldSelected = tableFields.has(column.name);

          return {
            title: (
              <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', width: '100%', padding: '1px 0' }}>
                <Space size={4} style={{ flex: 1 }}>
                  {selectionMode && (
                    <Checkbox
                      checked={isFieldSelected}
                      onChange={(e) => {
                        e.stopPropagation();
                        onFieldSelectionChange?.(table.name, column.name, e.target.checked);
                      }}
                      onClick={(e) => e.stopPropagation()}
                    />
                  )}
                  <Text style={{ fontSize: '13px', fontWeight: 500 }}>{column.name}</Text>
                  <Text type="secondary" style={{ fontSize: '12px', fontFamily: 'Monaco, Consolas, monospace', color: '#722ed1' }}>
                    {column.dataType}
                    {column.maxLength && `(${column.maxLength})`}
                  </Text>
                  {column.isPrimaryKey && (
                    <Tooltip title="Primary Key">
                      <KeyOutlined style={{ color: '#faad14', fontSize: '12px' }} />
                    </Tooltip>
                  )}
                  {column.isForeignKey && (
                    <Tooltip title={`Foreign Key → ${column.referencedTable}.${column.referencedColumn}`}>
                      <KeyOutlined style={{ color: '#52c41a', fontSize: '12px' }} />
                    </Tooltip>
                  )}
                  {!column.isNullable && (
                    <Tag color="red" style={{ fontSize: '10px', padding: '0 4px', lineHeight: '14px', borderRadius: '3px' }}>
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
  }, [tables, searchValue, onPreviewTable]);

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
          <Text type="secondary" style={{ fontSize: '13px' }}>({tables.length} tables)</Text>
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
      style={{ height: '100%', borderRadius: '8px' }}
      bodyStyle={{ padding: '12px', height: 'calc(100% - 55px)' }}
      headStyle={{ padding: '10px 16px', minHeight: '55px', background: 'linear-gradient(135deg, #fafafa 0%, #f5f5f5 100%)' }}
      className="schema-tree-container"
    >
      <Space direction="vertical" style={{ width: '100%', height: '100%' }} size={12}>
        <Search
          placeholder="Search tables and columns..."
          allowClear
          onChange={(e) => handleSearch(e.target.value)}
          style={{ marginBottom: '8px' }}
          prefix={<SearchOutlined />}
          size="middle"
        />

        <div style={{ flex: 1, overflow: 'auto', paddingRight: '4px' }} className="schema-tree-scroll">
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
      </Space>
    </Card>
  );
};
