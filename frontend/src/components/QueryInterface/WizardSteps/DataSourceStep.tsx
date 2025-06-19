/**
 * Data Source Selection Step
 */

import React, { useState, useEffect } from 'react';
import { Card, List, Input, Space, Tag, Button, Empty } from 'antd';
import { SearchOutlined, TableOutlined, EyeOutlined, CodeOutlined } from '@ant-design/icons';
import { WizardStepProps, DataSource } from './types';

const { Search } = Input;

// TODO: Replace with actual API call to get data sources from database
const mockDataSources: DataSource[] = [];

const DataSourceStep: React.FC<WizardStepProps> = ({
  onNext,
  data,
  onChange
}) => {
  const [searchTerm, setSearchTerm] = useState('');
  const [selectedSource, setSelectedSource] = useState<DataSource | null>(
    data?.dataSource || null
  );
  const [dataSources, setDataSources] = useState<DataSource[]>(mockDataSources);

  useEffect(() => {
    // Filter data sources based on search term
    const filtered = mockDataSources.filter(source =>
      source.name.toLowerCase().includes(searchTerm.toLowerCase()) ||
      source.description?.toLowerCase().includes(searchTerm.toLowerCase())
    );
    setDataSources(filtered);
  }, [searchTerm]);

  const handleSourceSelect = (source: DataSource) => {
    setSelectedSource(source);
    onChange?.({ ...data, dataSource: source });
  };

  const handleNext = () => {
    if (selectedSource) {
      onNext?.();
    }
  };

  const getTypeIcon = (type: string) => {
    switch (type) {
      case 'table':
        return <TableOutlined />;
      case 'view':
        return <EyeOutlined />;
      case 'query':
        return <CodeOutlined />;
      default:
        return <TableOutlined />;
    }
  };

  const getTypeColor = (type: string) => {
    switch (type) {
      case 'table':
        return 'blue';
      case 'view':
        return 'green';
      case 'query':
        return 'purple';
      default:
        return 'default';
    }
  };

  return (
    <div>
      <div style={{ marginBottom: '24px' }}>
        <h3 style={{ marginBottom: '8px' }}>Select Data Source</h3>
        <p style={{ color: '#8c8c8c', marginBottom: '16px' }}>
          Choose the table or view you want to query from
        </p>
        
        <Search
          placeholder="Search tables and views..."
          value={searchTerm}
          onChange={(e) => setSearchTerm(e.target.value)}
          style={{ marginBottom: '16px' }}
          prefix={<SearchOutlined />}
        />
      </div>

      {dataSources.length > 0 ? (
        <List
          grid={{ gutter: 16, xs: 1, sm: 1, md: 2, lg: 2, xl: 2 }}
          dataSource={dataSources}
          renderItem={(source) => (
            <List.Item>
              <Card
                hoverable
                onClick={() => handleSourceSelect(source)}
                style={{
                  cursor: 'pointer',
                  border: selectedSource?.id === source.id ? '2px solid #1890ff' : '1px solid #f0f0f0',
                  background: selectedSource?.id === source.id ? '#f0f8ff' : '#fff'
                }}
              >
                <div style={{ display: 'flex', alignItems: 'flex-start', gap: '12px' }}>
                  <div style={{ fontSize: '24px', color: '#1890ff' }}>
                    {getTypeIcon(source.type)}
                  </div>
                  <div style={{ flex: 1 }}>
                    <div style={{ display: 'flex', alignItems: 'center', gap: '8px', marginBottom: '8px' }}>
                      <h4 style={{ margin: 0, fontSize: '16px' }}>{source.name}</h4>
                      <Tag color={getTypeColor(source.type)}>
                        {source.type.toUpperCase()}
                      </Tag>
                    </div>
                    {source.schema && (
                      <div style={{ fontSize: '12px', color: '#8c8c8c', marginBottom: '8px' }}>
                        Schema: {source.schema}
                      </div>
                    )}
                    {source.description && (
                      <p style={{ 
                        margin: 0, 
                        fontSize: '14px', 
                        color: '#595959',
                        lineHeight: '1.4'
                      }}>
                        {source.description}
                      </p>
                    )}
                  </div>
                </div>
              </Card>
            </List.Item>
          )}
        />
      ) : (
        <Empty
          description="No data sources found"
          style={{ margin: '40px 0' }}
        />
      )}

      <div style={{ 
        marginTop: '24px', 
        paddingTop: '24px', 
        borderTop: '1px solid #f0f0f0',
        display: 'flex',
        justifyContent: 'space-between',
        alignItems: 'center'
      }}>
        <div>
          {selectedSource && (
            <Space>
              <span style={{ color: '#8c8c8c' }}>Selected:</span>
              <Tag color="blue">{selectedSource.name}</Tag>
            </Space>
          )}
        </div>
        <Button
          type="primary"
          onClick={handleNext}
          disabled={!selectedSource}
          style={{
            background: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)',
            border: 'none'
          }}
        >
          Next: Select Fields
        </Button>
      </div>
    </div>
  );
};

export default DataSourceStep;
