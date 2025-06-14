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
  onExpandedKeysChange
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
      
      return {
        title: (
          <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', width: '100%' }}>
            <Space size="small">
              <TableOutlined style={{ color: '#1890ff' }} />
              <Text strong>{table.name}</Text>
              {table.type === 'view' && <Tag color="purple">VIEW</Tag>}
              {table.rowCount !== undefined && (
                <Text type="secondary" style={{ fontSize: '12px' }}>
                  ({table.rowCount.toLocaleString()} rows)
                </Text>
              )}
            </Space>
            <Button
              type="text"
              size="small"
              icon={<EyeOutlined />}
              onClick={(e) => {
                e.stopPropagation();
                onPreviewTable?.(table);
              }}
              style={{ opacity: 0.6 }}
            />
          </div>
        ),
        key: tableKey,
        icon: <TableOutlined />,
        children: table.columns.map(column => ({
          title: (
            <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', width: '100%' }}>
              <Space size="small">
                <Text style={{ fontSize: '13px' }}>{column.name}</Text>
                <Text type="secondary" style={{ fontSize: '11px' }}>
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
                  <Tag color="red" style={{ fontSize: '10px', padding: '0 4px' }}>
                    NOT NULL
                  </Tag>
                )}
              </Space>
            </div>
          ),
          key: `column-${table.name}-${column.name}`,
          icon: column.isPrimaryKey ? 
            <KeyOutlined style={{ color: '#faad14' }} /> : 
            <div style={{ width: '14px', height: '14px', backgroundColor: '#d9d9d9', borderRadius: '2px' }} />,
          isLeaf: true,
          data: { table, column }
        })),
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
        <Space>
          <DatabaseOutlined />
          <span>Database Schema</span>
          <Text type="secondary">({tables.length} tables)</Text>
        </Space>
      }
      extra={
        <Button
          type="text"
          icon={<ReloadOutlined />}
          onClick={onRefresh}
          loading={loading}
          size="small"
        />
      }
      size="small"
      style={{ height: '100%' }}
      bodyStyle={{ padding: '12px', height: 'calc(100% - 57px)' }}
      className="schema-tree-container"
    >
      <Space direction="vertical" style={{ width: '100%', height: '100%' }}>
        <Search
          placeholder="Search tables and columns..."
          allowClear
          onChange={(e) => handleSearch(e.target.value)}
          style={{ marginBottom: '8px' }}
          prefix={<SearchOutlined />}
        />
        
        <div style={{ flex: 1, overflow: 'auto' }} className="schema-tree-container">
          {loading ? (
            <div style={{ textAlign: 'center', padding: '40px' }}>
              <Spin size="large" />
            </div>
          ) : treeData.length === 0 ? (
            <Empty 
              description="No tables found"
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
              style={{ fontSize: '13px' }}
            />
          )}
        </div>
      </Space>
    </Card>
  );
};
