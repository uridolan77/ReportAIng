import React, { useState, useEffect } from 'react';
import { Card, List, Input, Typography, Space, Tag, Spin, Empty } from 'antd';
import { DatabaseOutlined, SearchOutlined, TableOutlined } from '@ant-design/icons';
import { QueryBuilderData } from '../QueryWizard';

const { Title, Text, Paragraph } = Typography;
const { Search } = Input;

interface DataSource {
  table: string;
  description: string;
  category: string;
  recordCount: number;
  lastUpdated: string;
  columns: Array<{
    name: string;
    type: string;
    description?: string;
  }>;
}

interface DataSourceSelectorProps {
  data: QueryBuilderData;
  onChange: (data: Partial<QueryBuilderData>) => void;
  onNext?: () => void;
  onPrevious?: () => void;
}

export const DataSourceSelector: React.FC<DataSourceSelectorProps> = ({
  data,
  onChange,
}) => {
  const [dataSources, setDataSources] = useState<DataSource[]>([]);
  const [filteredSources, setFilteredSources] = useState<DataSource[]>([]);
  const [loading, setLoading] = useState(true);
  const [searchTerm, setSearchTerm] = useState('');

  // TODO: Replace with actual API call to get data sources from database
  useEffect(() => {
    // This should call the actual DB Explorer API to get table metadata
    setLoading(false);
    setDataSources([]);
    setFilteredSources([]);
  }, []);

  useEffect(() => {
    if (searchTerm) {
      const filtered = dataSources.filter(source =>
        source.table.toLowerCase().includes(searchTerm.toLowerCase()) ||
        source.description.toLowerCase().includes(searchTerm.toLowerCase()) ||
        source.category.toLowerCase().includes(searchTerm.toLowerCase())
      );
      setFilteredSources(filtered);
    } else {
      setFilteredSources(dataSources);
    }
  }, [searchTerm, dataSources]);

  const handleSelectDataSource = (source: DataSource) => {
    onChange({
      dataSource: {
        table: source.table,
        description: source.description
      }
    });
  };

  const formatNumber = (num: number): string => {
    return new Intl.NumberFormat().format(num);
  };

  const getCategoryColor = (category: string): string => {
    const colors: Record<string, string> = {
      'Gaming Analytics': 'blue',
      'Player Management': 'green',
      'Reference Data': 'orange',
      'Partner Management': 'blue'
    };
    return colors[category] || 'default';
  };

  if (loading) {
    return (
      <div style={{ textAlign: 'center', padding: '60px' }}>
        <Spin size="large" />
        <div style={{ marginTop: '16px' }}>
          <Text>Loading available data sources...</Text>
        </div>
      </div>
    );
  }

  return (
    <div>
      <div style={{ marginBottom: '24px' }}>
        <Title level={4}>
          <DatabaseOutlined /> Select Data Source
        </Title>
        <Paragraph type="secondary">
          Choose the primary table you want to analyze. This will determine what data is available for your query.
        </Paragraph>
      </div>

      <div style={{ marginBottom: '16px' }}>
        <Search
          placeholder="Search tables by name, description, or category..."
          prefix={<SearchOutlined />}
          value={searchTerm}
          onChange={(e) => setSearchTerm(e.target.value)}
          style={{ width: '100%' }}
          size="large"
        />
      </div>

      {filteredSources.length === 0 ? (
        <Empty
          description="No data sources found"
          style={{ padding: '40px' }}
        />
      ) : (
        <List
          grid={{ gutter: 16, xs: 1, sm: 1, md: 2, lg: 2, xl: 2 }}
          dataSource={filteredSources}
          renderItem={(source) => (
            <List.Item>
              <Card
                hoverable
                className={data.dataSource?.table === source.table ? 'selected-card' : ''}
                onClick={() => handleSelectDataSource(source)}
                style={{
                  border: data.dataSource?.table === source.table ? '2px solid #1890ff' : '1px solid #d9d9d9',
                  cursor: 'pointer'
                }}
              >
                <Card.Meta
                  avatar={<TableOutlined style={{ fontSize: '24px', color: '#1890ff' }} />}
                  title={
                    <Space direction="vertical" size="small" style={{ width: '100%' }}>
                      <Text strong>{source.table}</Text>
                      <Tag color={getCategoryColor(source.category)}>{source.category}</Tag>
                    </Space>
                  }
                  description={
                    <Space direction="vertical" size="small" style={{ width: '100%' }}>
                      <Text>{source.description}</Text>
                      <div>
                        <Text type="secondary" style={{ fontSize: '12px' }}>
                          {formatNumber(source.recordCount)} records • Updated {source.lastUpdated}
                        </Text>
                      </div>
                      <div>
                        <Text type="secondary" style={{ fontSize: '12px' }}>
                          {source.columns.length} columns available
                        </Text>
                      </div>
                    </Space>
                  }
                />
              </Card>
            </List.Item>
          )}
        />
      )}

      {data.dataSource && (
        <Card style={{ marginTop: '16px', backgroundColor: '#f6ffed', border: '1px solid #b7eb8f' }}>
          <Text strong style={{ color: '#52c41a' }}>
            ✓ Selected: {data.dataSource.table}
          </Text>
          <br />
          <Text type="secondary">{data.dataSource.description}</Text>
        </Card>
      )}


    </div>
  );
};
