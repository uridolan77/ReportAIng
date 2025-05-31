/**
 * History Page - Dedicated page for browsing and managing query history
 */

import React from 'react';
import {
  Card,
  Typography,
  Space,
  Button,
  Breadcrumb,
  Row,
  Col
} from 'antd';
import {
  HomeOutlined,
  HistoryOutlined,
  ArrowLeftOutlined
} from '@ant-design/icons';
import { useNavigate } from 'react-router-dom';
import { QueryProvider } from '../components/QueryInterface/QueryProvider';
import { useQueryContext } from '../components/QueryInterface/QueryProvider';
import { QueryHistory } from '../components/QueryInterface/QueryHistory';

const { Title, Text } = Typography;

const HistoryPageContent: React.FC = () => {
  const navigate = useNavigate();
  const { setQuery, setActiveTab, queryHistory } = useQueryContext();

  const handleQuerySelect = (selectedQuery: string) => {
    setQuery(selectedQuery);
    navigate('/', { state: { selectedQuery } });
  };

  return (
    <div style={{ maxWidth: '1200px', margin: '0 auto', padding: '24px' }}>
      <Breadcrumb style={{ marginBottom: '24px' }}>
        <Breadcrumb.Item>
          <HomeOutlined />
          <span onClick={() => navigate('/')} style={{ cursor: 'pointer', marginLeft: '8px' }}>
            Home
          </span>
        </Breadcrumb.Item>
        <Breadcrumb.Item>
          <HistoryOutlined />
          Query History
        </Breadcrumb.Item>
      </Breadcrumb>

      <div style={{ marginBottom: '32px' }}>
        <Title level={2} style={{ margin: 0, color: '#667eea' }}>
          Query History
        </Title>
        <Text type="secondary" style={{ fontSize: '16px' }}>
          Browse and reuse your previous queries ({queryHistory.length} saved)
        </Text>
      </div>

      <Row gutter={[24, 24]}>
        <Col span={24}>
          <Card className="enhanced-card">
            <QueryHistory onQuerySelect={handleQuerySelect} />
          </Card>
        </Col>
      </Row>

      {/* Quick Actions */}
      <Card 
        className="enhanced-card" 
        style={{ marginTop: '24px' }}
        title="Quick Actions"
      >
        <Space wrap>
          <Button 
            type="primary"
            onClick={() => navigate('/')}
            style={{
              background: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)',
              border: 'none',
              borderRadius: '8px'
            }}
          >
            New Query
          </Button>
          <Button 
            onClick={() => navigate('/templates')}
            style={{
              borderRadius: '8px',
              border: '1px solid #52c41a',
              color: '#52c41a'
            }}
          >
            Browse Templates
          </Button>
          <Button 
            onClick={() => navigate('/suggestions')}
            style={{
              borderRadius: '8px',
              border: '1px solid #722ed1',
              color: '#722ed1'
            }}
          >
            Smart Suggestions
          </Button>
        </Space>
      </Card>
    </div>
  );
};

export const HistoryPage: React.FC = () => {
  return (
    <QueryProvider>
      <HistoryPageContent />
    </QueryProvider>
  );
};
