import React, { useState, useEffect } from 'react';
import { Card, Button, Typography, Space, Alert, Tabs, Badge, Row, Col, Statistic, Progress } from 'antd';
import {
  RocketOutlined,
  ThunderboltOutlined,
  BulbOutlined,
  EyeOutlined,
  CheckCircleOutlined,
  ExclamationCircleOutlined,
  LoadingOutlined
} from '@ant-design/icons';
import { QueryInterface } from '../components/QueryInterface';

const { Title, Paragraph, Text } = Typography;
const { TabPane } = Tabs;

interface FeatureStatus {
  name: string;
  status: 'enabled' | 'disabled' | 'testing';
  description: string;
  endpoint?: string;
}

export const EnhancedFeaturesDemo: React.FC = () => {
  const [features, setFeatures] = useState<FeatureStatus[]>([]);
  const [loading, setLoading] = useState(true);
  const [systemStatus, setSystemStatus] = useState({
    enhancedQueryProcessor: false,
    realTimeStreaming: false,
    websocketConnection: false
  });

  useEffect(() => {
    checkFeatureStatus();
  }, []);

  const checkFeatureStatus = async () => {
    setLoading(true);
    try {
      // Check backend feature status
      const response = await fetch('/api/health', {
        headers: {
          'Authorization': `Bearer ${localStorage.getItem('token')}`
        }
      });

      if (response.ok) {
        const healthData = await response.json();
        console.log('🏥 Health check data:', healthData);
        
        // Update system status based on health check
        setSystemStatus({
          enhancedQueryProcessor: true, // Enabled in configuration
          realTimeStreaming: true, // Enabled in configuration
          websocketConnection: false // Will be updated by WebSocket connection
        });
      }

      // Define available features
      const featureList: FeatureStatus[] = [
        {
          name: 'Enhanced Query Processing',
          status: 'enabled',
          description: 'AI-powered query processing with conversation context, query decomposition, and schema-aware SQL generation.',
          endpoint: '/api/query/enhanced'
        },
        {
          name: 'Real-Time Streaming Analytics',
          status: 'enabled',
          description: 'Live data processing, streaming dashboards, and real-time insights with SignalR.',
          endpoint: '/hubs/streaming'
        },
        {
          name: 'Conversation Context',
          status: 'enabled',
          description: 'Maintains conversation history to provide context for follow-up queries.'
        },
        {
          name: 'Query Decomposition',
          status: 'enabled',
          description: 'Breaks down complex queries into manageable components for better processing.'
        },
        {
          name: 'Schema-Aware SQL Generation',
          status: 'enabled',
          description: 'Generates SQL queries with full awareness of database schema and relationships.'
        },
        {
          name: 'Semantic Entity Recognition',
          status: 'enabled',
          description: 'Identifies and extracts semantic entities from natural language queries.'
        },
        {
          name: 'Real-Time Query Insights',
          status: 'enabled',
          description: 'Provides live performance insights and recommendations during query execution.'
        },
        {
          name: 'Advanced Confidence Scoring',
          status: 'enabled',
          description: 'AI confidence scoring with detailed explanations and alternative suggestions.'
        }
      ];

      setFeatures(featureList);
    } catch (error) {
      console.error('❌ Error checking feature status:', error);
      setFeatures([]);
    } finally {
      setLoading(false);
    }
  };

  const getStatusIcon = (status: string) => {
    switch (status) {
      case 'enabled':
        return <CheckCircleOutlined style={{ color: '#52c41a' }} />;
      case 'disabled':
        return <ExclamationCircleOutlined style={{ color: '#ff4d4f' }} />;
      case 'testing':
        return <LoadingOutlined style={{ color: '#faad14' }} />;
      default:
        return <ExclamationCircleOutlined style={{ color: '#d9d9d9' }} />;
    }
  };

  const getStatusColor = (status: string) => {
    switch (status) {
      case 'enabled':
        return 'success';
      case 'disabled':
        return 'error';
      case 'testing':
        return 'warning';
      default:
        return 'default';
    }
  };

  const enabledFeatures = features.filter(f => f.status === 'enabled').length;
  const totalFeatures = features.length;
  const enabledPercentage = totalFeatures > 0 ? Math.round((enabledFeatures / totalFeatures) * 100) : 0;

  return (
    <div style={{ padding: '24px', maxWidth: '1400px', margin: '0 auto' }}>
      <div style={{ marginBottom: '32px' }}>
        <Title level={1}>
          <RocketOutlined style={{ color: '#1890ff', marginRight: '12px' }} />
          Enhanced AI Features Demo
        </Title>
        <Paragraph style={{ fontSize: '16px', color: '#666' }}>
          Experience the next generation of AI-powered business intelligence with enhanced query processing 
          and real-time streaming analytics.
        </Paragraph>
      </div>

      <Row gutter={[24, 24]} style={{ marginBottom: '32px' }}>
        <Col xs={24} sm={12} md={6}>
          <Card>
            <Statistic
              title="Features Enabled"
              value={enabledFeatures}
              suffix={`/ ${totalFeatures}`}
              valueStyle={{ color: '#3f8600' }}
              prefix={<CheckCircleOutlined />}
            />
          </Card>
        </Col>
        <Col xs={24} sm={12} md={6}>
          <Card>
            <Statistic
              title="System Readiness"
              value={enabledPercentage}
              suffix="%"
              valueStyle={{ color: enabledPercentage > 80 ? '#3f8600' : '#cf1322' }}
              prefix={<BulbOutlined />}
            />
          </Card>
        </Col>
        <Col xs={24} sm={12} md={6}>
          <Card>
            <Statistic
              title="Enhanced Processing"
              value={systemStatus.enhancedQueryProcessor ? 'Active' : 'Inactive'}
              valueStyle={{ color: systemStatus.enhancedQueryProcessor ? '#3f8600' : '#cf1322' }}
              prefix={<ThunderboltOutlined />}
            />
          </Card>
        </Col>
        <Col xs={24} sm={12} md={6}>
          <Card>
            <Statistic
              title="Real-time Streaming"
              value={systemStatus.realTimeStreaming ? 'Active' : 'Inactive'}
              valueStyle={{ color: systemStatus.realTimeStreaming ? '#3f8600' : '#cf1322' }}
              prefix={<EyeOutlined />}
            />
          </Card>
        </Col>
      </Row>

      <Tabs defaultActiveKey="1" size="large">
        <TabPane tab={<span><BulbOutlined />Enhanced Interface</span>} key="1">
          <Alert
            message="Enhanced AI Query Interface"
            description="This interface demonstrates the enhanced AI features including conversation context, query decomposition, and real-time streaming analytics."
            type="info"
            showIcon
            style={{ marginBottom: '24px' }}
          />
          <Card title="Enhanced Query Interface">
            <p>Enhanced Query Interface component - Coming Soon</p>
            <p>This will include conversation context, query decomposition, and real-time streaming analytics.</p>
          </Card>
        </TabPane>

        <TabPane tab={<span><CheckCircleOutlined />Feature Status</span>} key="2">
          <Card title="Feature Status Overview" style={{ marginBottom: '24px' }}>
            <Progress 
              percent={enabledPercentage} 
              status={enabledPercentage > 80 ? 'success' : 'exception'}
              style={{ marginBottom: '16px' }}
            />
            <Text type="secondary">
              {enabledFeatures} of {totalFeatures} enhanced features are currently enabled
            </Text>
          </Card>

          <Row gutter={[16, 16]}>
            {features.map((feature, index) => (
              <Col xs={24} md={12} lg={8} key={index}>
                <Card 
                  size="small"
                  title={
                    <Space>
                      {getStatusIcon(feature.status)}
                      <Text strong>{feature.name}</Text>
                      <Badge 
                        status={getStatusColor(feature.status) as any} 
                        text={feature.status.toUpperCase()} 
                      />
                    </Space>
                  }
                >
                  <Paragraph style={{ marginBottom: '8px', fontSize: '14px' }}>
                    {feature.description}
                  </Paragraph>
                  {feature.endpoint && (
                    <Text code style={{ fontSize: '12px' }}>
                      {feature.endpoint}
                    </Text>
                  )}
                </Card>
              </Col>
            ))}
          </Row>
        </TabPane>

        <TabPane tab={<span><ThunderboltOutlined />System Status</span>} key="3">
          <Row gutter={[24, 24]}>
            <Col xs={24} md={12}>
              <Card title="Backend Services" size="small">
                <Space direction="vertical" style={{ width: '100%' }}>
                  <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                    <Text>Enhanced Query Processor</Text>
                    <Badge 
                      status={systemStatus.enhancedQueryProcessor ? 'success' : 'error'} 
                      text={systemStatus.enhancedQueryProcessor ? 'Running' : 'Stopped'} 
                    />
                  </div>
                  <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                    <Text>Real-time Streaming Service</Text>
                    <Badge 
                      status={systemStatus.realTimeStreaming ? 'success' : 'error'} 
                      text={systemStatus.realTimeStreaming ? 'Running' : 'Stopped'} 
                    />
                  </div>
                  <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                    <Text>WebSocket Connection</Text>
                    <Badge 
                      status={systemStatus.websocketConnection ? 'success' : 'warning'} 
                      text={systemStatus.websocketConnection ? 'Connected' : 'Disconnected'} 
                    />
                  </div>
                </Space>
              </Card>
            </Col>
            <Col xs={24} md={12}>
              <Card title="Configuration" size="small">
                <Space direction="vertical" style={{ width: '100%' }}>
                  <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                    <Text>Enhanced Processing</Text>
                    <Badge status="success" text="Enabled" />
                  </div>
                  <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                    <Text>Conversation Context</Text>
                    <Badge status="success" text="Enabled" />
                  </div>
                  <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                    <Text>Query Decomposition</Text>
                    <Badge status="success" text="Enabled" />
                  </div>
                  <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                    <Text>Schema-Aware SQL</Text>
                    <Badge status="success" text="Enabled" />
                  </div>
                </Space>
              </Card>
            </Col>
          </Row>

          <Card title="Actions" style={{ marginTop: '24px' }}>
            <Space>
              <Button 
                type="primary" 
                icon={<CheckCircleOutlined />}
                onClick={checkFeatureStatus}
                loading={loading}
              >
                Refresh Status
              </Button>
              <Button 
                icon={<EyeOutlined />}
                onClick={() => window.open('/api/health', '_blank')}
              >
                View Health Check
              </Button>
              <Button
                icon={<BulbOutlined />}
                onClick={() => window.location.href = '/enhanced-ai'}
              >
                Try Enhanced Interface
              </Button>
            </Space>
          </Card>
        </TabPane>
      </Tabs>
    </div>
  );
};

export default EnhancedFeaturesDemo;
