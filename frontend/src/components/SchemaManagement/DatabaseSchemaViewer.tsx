import React, { useState, useMemo } from 'react';
import {
  Card,
  Table,
  Typography,
  Space,
  Tag,
  Input,
  Row,
  Col,
  Statistic,
  Button,
  Alert,
  Spin,
  Empty
} from 'antd';
import {
  DatabaseOutlined,
  TableOutlined,
  SearchOutlined,
  InfoCircleOutlined,
  KeyOutlined,
  LinkOutlined,
  ReloadOutlined
} from '@ant-design/icons';

const { Title, Text } = Typography;
const { Search } = Input;

interface DatabaseSchemaViewerProps {
  schema: any;
  loading: boolean;
  onRefresh: () => void;
}

export const DatabaseSchemaViewer: React.FC<DatabaseSchemaViewerProps> = ({
  schema,
  loading,
  onRefresh
}) => {
  const [searchTerm, setSearchTerm] = useState('');

  const filteredTables = useMemo(() => {
    if (!schema?.tables) return [];
    
    return schema.tables.filter((table: any) =>
      table.name.toLowerCase().includes(searchTerm.toLowerCase()) ||
      table.schema?.toLowerCase().includes(searchTerm.toLowerCase())
    );
  }, [schema?.tables, searchTerm]);

  const tableColumns = [
    {
      title: 'Table Name',
      dataIndex: 'name',
      key: 'name',
      render: (name: string, record: any) => (
        <Space>
          <TableOutlined />
          <Text strong>{record.schema ? `${record.schema}.${name}` : name}</Text>
        </Space>
      ),
    },
    {
      title: 'Columns',
      dataIndex: 'columns',
      key: 'columns',
      render: (columns: any[]) => (
        <Tag color="blue">{columns?.length || 0} columns</Tag>
      ),
    },
    {
      title: 'Row Count',
      dataIndex: 'rowCount',
      key: 'rowCount',
      render: (count: number) => (
        <Text type="secondary">{count?.toLocaleString() || 'N/A'}</Text>
      ),
    },
    {
      title: 'Last Updated',
      dataIndex: 'lastUpdated',
      key: 'lastUpdated',
      render: (date: string) => (
        <Text type="secondary">
          {date ? new Date(date).toLocaleDateString() : 'N/A'}
        </Text>
      ),
    },
  ];

  const columnColumns = [
    {
      title: 'Column Name',
      dataIndex: 'name',
      key: 'name',
      render: (name: string, record: any) => (
        <Space>
          {record.isPrimaryKey && <KeyOutlined style={{ color: '#faad14' }} />}
          {record.isForeignKey && <LinkOutlined style={{ color: '#52c41a' }} />}
          <Text strong={record.isPrimaryKey}>{name}</Text>
        </Space>
      ),
    },
    {
      title: 'Data Type',
      dataIndex: 'dataType',
      key: 'dataType',
      render: (type: string, record: any) => {
        let color = 'default';
        if (type?.includes('varchar') || type?.includes('text')) color = 'green';
        else if (type?.includes('int') || type?.includes('decimal')) color = 'blue';
        else if (type?.includes('date') || type?.includes('time')) color = 'orange';
        else if (type?.includes('bit') || type?.includes('bool')) color = 'purple';
        
        return (
          <Space>
            <Tag color={color}>{type}</Tag>
            {record.maxLength && <Text type="secondary">({record.maxLength})</Text>}
          </Space>
        );
      },
    },
    {
      title: 'Nullable',
      dataIndex: 'isNullable',
      key: 'isNullable',
      render: (nullable: boolean) => (
        <Tag color={nullable ? 'default' : 'red'}>
          {nullable ? 'NULL' : 'NOT NULL'}
        </Tag>
      ),
    },
    {
      title: 'Default',
      dataIndex: 'defaultValue',
      key: 'defaultValue',
      render: (value: string) => (
        <Text type="secondary" code={!!value}>
          {value || '-'}
        </Text>
      ),
    },
  ];

  if (loading) {
    return (
      <div style={{ display: 'flex', justifyContent: 'center', alignItems: 'center', height: '400px' }}>
        <Spin size="large" tip="Loading database schema..." />
      </div>
    );
  }

  if (!schema) {
    return (
      <Empty
        image={<DatabaseOutlined style={{ fontSize: '64px', color: '#d9d9d9' }} />}
        description={
          <div>
            <Title level={4}>No Database Schema</Title>
            <Text type="secondary">Unable to load database schema information.</Text>
          </div>
        }
      >
        <Button type="primary" icon={<ReloadOutlined />} onClick={onRefresh}>
          Retry Loading
        </Button>
      </Empty>
    );
  }

  return (
    <div style={{ padding: '16px' }}>
      {/* Database Info */}
      <Row gutter={[16, 16]} style={{ marginBottom: '24px' }}>
        <Col xs={24} sm={8}>
          <Card>
            <Statistic
              title="Database"
              value={schema.databaseName || 'Unknown'}
              prefix={<DatabaseOutlined style={{ color: '#1890ff' }} />}
            />
          </Card>
        </Col>
        <Col xs={24} sm={8}>
          <Card>
            <Statistic
              title="Total Tables"
              value={schema.tables?.length || 0}
              prefix={<TableOutlined style={{ color: '#52c41a' }} />}
            />
          </Card>
        </Col>
        <Col xs={24} sm={8}>
          <Card>
            <Statistic
              title="Last Updated"
              value={schema.lastUpdated ? new Date(schema.lastUpdated).toLocaleDateString() : 'N/A'}
              prefix={<InfoCircleOutlined style={{ color: '#722ed1' }} />}
            />
          </Card>
        </Col>
      </Row>

      {/* Search and Actions */}
      <Row justify="space-between" align="middle" style={{ marginBottom: '16px' }}>
        <Col xs={24} sm={16}>
          <Search
            placeholder="Search tables..."
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
            style={{ width: '100%' }}
            prefix={<SearchOutlined />}
          />
        </Col>
        <Col xs={24} sm={6}>
          <Button
            type="primary"
            icon={<ReloadOutlined />}
            onClick={onRefresh}
            style={{ width: '100%' }}
          >
            Refresh Schema
          </Button>
        </Col>
      </Row>

      {/* Tables List */}
      <Card title={`Database Tables (${filteredTables.length})`}>
        <Table
          dataSource={filteredTables}
          columns={tableColumns}
          rowKey="name"
          pagination={{
            pageSize: 10,
            showSizeChanger: true,
            showQuickJumper: true,
            showTotal: (total, range) => `${range[0]}-${range[1]} of ${total} tables`,
          }}
          expandable={{
            expandedRowRender: (record) => (
              <div style={{ margin: 0 }}>
                <Title level={5}>Columns for {record.schema}.{record.name}</Title>
                <Table
                  dataSource={record.columns || []}
                  columns={columnColumns}
                  rowKey="name"
                  pagination={false}
                  size="small"
                />
              </div>
            ),
            rowExpandable: (record) => !!(record.columns && record.columns.length > 0),
          }}
        />
      </Card>

      {/* Schema Information */}
      {schema.version && (
        <Alert
          message="Schema Information"
          description={`Schema version: ${schema.version} | Last updated: ${schema.lastUpdated ? new Date(schema.lastUpdated).toLocaleString() : 'Unknown'}`}
          type="info"
          showIcon
          style={{ marginTop: '16px' }}
        />
      )}
    </div>
  );
};
