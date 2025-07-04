import React from 'react';
import { List, Card, Typography, Tag, Space, Empty, Spin, Alert } from 'antd';
import { ClockCircleOutlined, CheckCircleOutlined, CloseCircleOutlined } from '@ant-design/icons';
import { useQueryHistory } from '../../hooks/useQueryApi';

const { Text } = Typography;

interface QueryHistoryProps {
  onQuerySelect: (query: string) => void;
}

export const QueryHistory: React.FC<QueryHistoryProps> = ({ onQuerySelect }) => {
  const {
    data: queryHistoryData,
    isLoading,
    error
  } = useQueryHistory(1, 20);

  if (isLoading) {
    return (
      <div style={{ textAlign: 'center', padding: '40px' }}>
        <Spin size="large" />
        <div style={{ marginTop: '16px' }}>
          <Text type="secondary">Loading query history...</Text>
        </div>
      </div>
    );
  }

  if (error) {
    return (
      <Alert
        message="Error Loading History"
        description="Unable to load query history. Please try again later."
        type="error"
        showIcon
      />
    );
  }

  const queryHistory = queryHistoryData?.items || [];

  if (queryHistory.length === 0) {
    return (
      <Empty
        description="No query history yet"
        image={Empty.PRESENTED_IMAGE_SIMPLE}
      >
        <Text type="secondary">
          Your executed queries will appear here
        </Text>
      </Empty>
    );
  }

  return (
    <List
      dataSource={queryHistory}
      renderItem={(item: any) => (
        <List.Item>
          <Card
            size="small"
            style={{ width: '100%' }}
            hoverable
            onClick={() => onQuerySelect(item.question)}
          >
            <Space direction="vertical" style={{ width: '100%' }}>
              <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start' }}>
                <Text strong style={{ flex: 1 }}>
                  {item.question}
                </Text>
                <Space>
                  {item.successful ? (
                    <CheckCircleOutlined style={{ color: '#52c41a' }} />
                  ) : (
                    <CloseCircleOutlined style={{ color: '#ff4d4f' }} />
                  )}
                </Space>
              </div>

              <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                <Space size="small">
                  <Tag icon={<ClockCircleOutlined />} color="blue">
                    {item.executionTimeMs}ms
                  </Tag>
                  {item.confidence && (
                    <Tag color={item.confidence > 0.8 ? 'green' : item.confidence > 0.6 ? 'orange' : 'red'}>
                      {(item.confidence * 100).toFixed(0)}%
                    </Tag>
                  )}
                </Space>

                <Text type="secondary" style={{ fontSize: '12px' }}>
                  {new Date(item.timestamp).toLocaleString()}
                </Text>
              </div>

              {item.error && (
                <Text type="danger" style={{ fontSize: '12px' }}>
                  Error: {typeof item.error === 'string' ? item.error : item.error?.message || 'An error occurred'}
                </Text>
              )}
            </Space>
          </Card>
        </List.Item>
      )}
    />
  );
};
