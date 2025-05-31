/**
 * Results Page - Dedicated page for viewing query results and basic charts
 */

import React from 'react';
import {
  Card,
  Typography,
  Space,
  Button,
  Row,
  Col,
  Empty,
  Breadcrumb
} from 'antd';
import {
  HomeOutlined,
  BarChartOutlined,
  ArrowLeftOutlined
} from '@ant-design/icons';
import { useNavigate } from 'react-router-dom';
import { QueryProvider } from '../components/QueryInterface/QueryProvider';
import { useQueryContext } from '../components/QueryInterface/QueryProvider';
import { QueryResult } from '../components/QueryInterface/QueryResult';
import { DataInsightsPanel } from '../components/Insights/DataInsightsPanel';

const { Title, Text } = Typography;

const ResultsPageContent: React.FC = () => {
  const navigate = useNavigate();
  const { currentResult, query, handleSubmitQuery } = useQueryContext();

  if (!currentResult) {
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
            <BarChartOutlined />
            Results
          </Breadcrumb.Item>
        </Breadcrumb>

        <Card className="enhanced-card">
          <Empty
            image={Empty.PRESENTED_IMAGE_SIMPLE}
            description={
              <Space direction="vertical">
                <Text type="secondary" style={{ fontSize: '16px' }}>
                  No query results to display
                </Text>
                <Text type="secondary" style={{ fontSize: '14px' }}>
                  Run a query first to see results here
                </Text>
              </Space>
            }
            style={{ padding: '60px 0' }}
          >
            <Button 
              type="primary" 
              icon={<ArrowLeftOutlined />}
              onClick={() => navigate('/')}
              style={{
                background: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)',
                border: 'none',
                borderRadius: '8px'
              }}
            >
              Go to Query Interface
            </Button>
          </Empty>
        </Card>
      </div>
    );
  }

  return (
    <div style={{ maxWidth: '1400px', margin: '0 auto', padding: '24px' }}>
      <Breadcrumb style={{ marginBottom: '24px' }}>
        <Breadcrumb.Item>
          <HomeOutlined />
          <span onClick={() => navigate('/')} style={{ cursor: 'pointer', marginLeft: '8px' }}>
            Home
          </span>
        </Breadcrumb.Item>
        <Breadcrumb.Item>
          <BarChartOutlined />
          Results
        </Breadcrumb.Item>
      </Breadcrumb>

      <div style={{ marginBottom: '24px' }}>
        <Title level={2} style={{ margin: 0, color: '#667eea' }}>
          Query Results & Analysis
        </Title>
        <Text type="secondary" style={{ fontSize: '16px' }}>
          Detailed view of your query results with insights and analysis
        </Text>
      </div>

      <Row gutter={[24, 24]}>
        <Col xs={24} lg={16}>
          <QueryResult
            result={currentResult}
            query={query}
            onRequery={handleSubmitQuery}
            onSuggestionClick={(suggestion) => {
              // Handle suggestion click - could navigate back to main page with the suggestion
              navigate('/', { state: { suggestedQuery: suggestion } });
            }}
          />
        </Col>
        
        <Col xs={24} lg={8}>
          <Card 
            className="enhanced-card"
            title={
              <Space>
                <BarChartOutlined style={{ color: '#667eea' }} />
                <span style={{ color: '#667eea' }}>Data Insights</span>
              </Space>
            }
          >
            <DataInsightsPanel
              queryResult={currentResult}
              onInsightAction={(action) => {
                console.log('Insight action:', action);
                // Handle insight actions like drill-down, filtering, etc.
              }}
              autoGenerate={true}
            />
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
            onClick={() => navigate('/')}
            style={{
              borderRadius: '8px',
              border: '1px solid #667eea',
              color: '#667eea'
            }}
          >
            New Query
          </Button>
          <Button 
            onClick={() => navigate('/dashboard')}
            disabled={!currentResult}
            style={{
              borderRadius: '8px',
              border: '1px solid #52c41a',
              color: '#52c41a'
            }}
          >
            Create Dashboard
          </Button>
          <Button 
            onClick={() => navigate('/interactive')}
            disabled={!currentResult}
            style={{
              borderRadius: '8px',
              border: '1px solid #722ed1',
              color: '#722ed1'
            }}
          >
            Interactive Visualization
          </Button>
          <Button 
            onClick={() => navigate('/advanced-viz')}
            disabled={!currentResult}
            style={{
              borderRadius: '8px',
              border: '1px solid #fa8c16',
              color: '#fa8c16'
            }}
          >
            AI-Powered Charts
          </Button>
        </Space>
      </Card>
    </div>
  );
};

export const ResultsPage: React.FC = () => {
  return (
    <QueryProvider>
      <ResultsPageContent />
    </QueryProvider>
  );
};
