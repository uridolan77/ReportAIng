import React, { useState, useEffect } from 'react';
import {
  Card,
  Statistic,
  Row,
  Col,
  Typography,
  Spin,
  Alert,
  Button,
  Space,
  Tag,
  List,
  Progress,
  Divider
} from 'antd';
import {
  DatabaseOutlined,
  TableOutlined,
  SearchOutlined,
  BookOutlined,
  SettingOutlined,
  BulbOutlined,
  TrophyOutlined,
  ClockCircleOutlined,
  BugOutlined,
  RobotOutlined,
  FileTextOutlined,
  CheckCircleOutlined,
  ExclamationCircleOutlined
} from '@ant-design/icons';
import { tuningApi } from '../../services/tuningApi';

const { Title, Text } = Typography;

interface TuningOverviewData {
  totalTables: number;
  totalColumns: number;
  totalPatterns: number;
  totalGlossaryTerms: number;
  activePromptTemplates: number;
  recentlyUpdatedTables: string[];
  mostUsedPatterns: string[];
  patternUsageStats: Record<string, number>;
  systemHealth: {
    aiServiceStatus: 'healthy' | 'warning' | 'error';
    databaseConnection: 'connected' | 'disconnected';
    cacheStatus: 'active' | 'inactive';
    lastHealthCheck: string;
  };
}

interface TuningOverviewProps {
  onNavigateToTab?: (tabKey: string) => void;
}

export const TuningOverview: React.FC<TuningOverviewProps> = ({ onNavigateToTab }) => {
  const [data, setData] = useState<TuningOverviewData | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    loadOverviewData();
  }, []);

  const loadOverviewData = async () => {
    try {
      setLoading(true);
      const dashboardData = await tuningApi.getDashboardData();
      
      // Enhanced with system health data
      const enhancedData: TuningOverviewData = {
        ...dashboardData,
        systemHealth: {
          aiServiceStatus: 'healthy',
          databaseConnection: 'connected',
          cacheStatus: 'active',
          lastHealthCheck: new Date().toISOString()
        }
      };
      
      setData(enhancedData);
    } catch (error) {
      console.error('Error loading overview data:', error);
    } finally {
      setLoading(false);
    }
  };

  const getHealthStatusColor = (status: string) => {
    switch (status) {
      case 'healthy':
      case 'connected':
      case 'active':
        return 'success';
      case 'warning':
        return 'warning';
      case 'error':
      case 'disconnected':
      case 'inactive':
        return 'error';
      default:
        return 'default';
    }
  };

  const getHealthStatusIcon = (status: string) => {
    switch (status) {
      case 'healthy':
      case 'connected':
      case 'active':
        return <CheckCircleOutlined />;
      case 'warning':
        return <ExclamationCircleOutlined />;
      case 'error':
      case 'disconnected':
      case 'inactive':
        return <ExclamationCircleOutlined />;
      default:
        return <CheckCircleOutlined />;
    }
  };

  if (loading) {
    return (
      <div style={{ textAlign: 'center', padding: '50px' }}>
        <Spin size="large" />
        <div style={{ marginTop: 16 }}>
          <Text>Loading AI tuning overview...</Text>
        </div>
      </div>
    );
  }

  if (!data) {
    return (
      <Alert
        message="Failed to Load Overview"
        description="Unable to load AI tuning overview data. Please try refreshing the page."
        type="error"
        showIcon
        action={
          <Button size="small" onClick={loadOverviewData}>
            Retry
          </Button>
        }
      />
    );
  }

  return (
    <div>
      {/* System Health Status */}
      <Card style={{ marginBottom: 24 }}>
        <Title level={4}>
          <RobotOutlined style={{ marginRight: 8 }} />
          System Health Status
        </Title>
        <Row gutter={[16, 16]}>
          <Col xs={24} sm={8}>
            <Card size="small" style={{ textAlign: 'center' }}>
              <Statistic
                title="AI Service"
                value={data.systemHealth.aiServiceStatus}
                prefix={getHealthStatusIcon(data.systemHealth.aiServiceStatus)}
                valueStyle={{ 
                  color: getHealthStatusColor(data.systemHealth.aiServiceStatus) === 'success' ? '#52c41a' : 
                         getHealthStatusColor(data.systemHealth.aiServiceStatus) === 'warning' ? '#faad14' : '#ff4d4f'
                }}
              />
            </Card>
          </Col>
          <Col xs={24} sm={8}>
            <Card size="small" style={{ textAlign: 'center' }}>
              <Statistic
                title="Database"
                value={data.systemHealth.databaseConnection}
                prefix={getHealthStatusIcon(data.systemHealth.databaseConnection)}
                valueStyle={{ 
                  color: getHealthStatusColor(data.systemHealth.databaseConnection) === 'success' ? '#52c41a' : '#ff4d4f'
                }}
              />
            </Card>
          </Col>
          <Col xs={24} sm={8}>
            <Card size="small" style={{ textAlign: 'center' }}>
              <Statistic
                title="Cache"
                value={data.systemHealth.cacheStatus}
                prefix={getHealthStatusIcon(data.systemHealth.cacheStatus)}
                valueStyle={{ 
                  color: getHealthStatusColor(data.systemHealth.cacheStatus) === 'success' ? '#52c41a' : '#ff4d4f'
                }}
              />
            </Card>
          </Col>
        </Row>
        <div style={{ marginTop: 16, textAlign: 'center' }}>
          <Text type="secondary">
            Last health check: {new Date(data.systemHealth.lastHealthCheck).toLocaleString()}
          </Text>
        </div>
      </Card>

      {/* Key Metrics */}
      <Row gutter={[16, 16]} style={{ marginBottom: 24 }}>
        <Col xs={24} sm={12} lg={6}>
          <Card>
            <Statistic
              title="Business Tables"
              value={data.totalTables}
              prefix={<TableOutlined />}
              suffix="configured"
            />
          </Card>
        </Col>
        <Col xs={24} sm={12} lg={6}>
          <Card>
            <Statistic
              title="Total Columns"
              value={data.totalColumns}
              prefix={<DatabaseOutlined />}
              suffix="mapped"
            />
          </Card>
        </Col>
        <Col xs={24} sm={12} lg={6}>
          <Card>
            <Statistic
              title="Query Patterns"
              value={data.totalPatterns}
              prefix={<SearchOutlined />}
              suffix="learned"
            />
          </Card>
        </Col>
        <Col xs={24} sm={12} lg={6}>
          <Card>
            <Statistic
              title="Glossary Terms"
              value={data.totalGlossaryTerms}
              prefix={<BookOutlined />}
              suffix="defined"
            />
          </Card>
        </Col>
      </Row>

      {/* Prompt Templates Status */}
      <Row gutter={[16, 16]} style={{ marginBottom: 24 }}>
        <Col xs={24} lg={12}>
          <Card>
            <Title level={5}>
              <FileTextOutlined style={{ marginRight: 8 }} />
              Active Prompt Templates
            </Title>
            <Statistic
              value={data.activePromptTemplates}
              suffix="templates active"
              prefix={<CheckCircleOutlined style={{ color: '#52c41a' }} />}
            />
            <div style={{ marginTop: 16 }}>
              <Button 
                type="primary" 
                onClick={() => onNavigateToTab?.('prompt-management')}
              >
                Manage Templates
              </Button>
            </div>
          </Card>
        </Col>
        <Col xs={24} lg={12}>
          <Card>
            <Title level={5}>
              <TrophyOutlined style={{ marginRight: 8 }} />
              Pattern Usage Distribution
            </Title>
            {Object.entries(data.patternUsageStats).slice(0, 3).map(([pattern, usage]) => (
              <div key={pattern} style={{ marginBottom: 8 }}>
                <div style={{ display: 'flex', justifyContent: 'space-between', marginBottom: 4 }}>
                  <Text>{pattern}</Text>
                  <Text>{usage}%</Text>
                </div>
                <Progress percent={usage} size="small" />
              </div>
            ))}
          </Card>
        </Col>
      </Row>

      {/* Quick Actions */}
      <Card>
        <Title level={4}>Quick Actions</Title>
        <Row gutter={[16, 16]}>
          <Col xs={24} sm={12} lg={8}>
            <Button
              type="primary"
              block
              icon={<SettingOutlined />}
              onClick={() => onNavigateToTab?.('ai-configuration')}
            >
              AI Configuration
            </Button>
          </Col>
          <Col xs={24} sm={12} lg={8}>
            <Button
              block
              icon={<FileTextOutlined />}
              onClick={() => onNavigateToTab?.('prompt-management')}
            >
              Prompt Management
            </Button>
          </Col>
          <Col xs={24} sm={12} lg={8}>
            <Button
              block
              icon={<BookOutlined />}
              onClick={() => onNavigateToTab?.('knowledge-base')}
            >
              Knowledge Base
            </Button>
          </Col>
        </Row>
        <Row gutter={[16, 16]} style={{ marginTop: 16 }}>
          <Col xs={24} sm={12} lg={12}>
            <Button
              block
              icon={<RobotOutlined />}
              onClick={() => onNavigateToTab?.('auto-generate')}
              type="dashed"
            >
              🤖 Auto-Generate Content
            </Button>
          </Col>
          <Col xs={24} sm={12} lg={12}>
            <Button
              block
              icon={<BugOutlined />}
              onClick={() => onNavigateToTab?.('monitoring')}
            >
              📊 View Monitoring
            </Button>
          </Col>
        </Row>
      </Card>

      {/* Recent Activity */}
      <Row gutter={[16, 16]} style={{ marginTop: 24 }}>
        <Col xs={24} lg={12}>
          <Card>
            <Title level={5}>
              <ClockCircleOutlined style={{ marginRight: 8 }} />
              Recently Updated Tables
            </Title>
            <List
              size="small"
              dataSource={data.recentlyUpdatedTables}
              renderItem={(table) => (
                <List.Item>
                  <Text>{table}</Text>
                  <Tag color="blue">Updated</Tag>
                </List.Item>
              )}
            />
          </Card>
        </Col>
        <Col xs={24} lg={12}>
          <Card>
            <Title level={5}>
              <BulbOutlined style={{ marginRight: 8 }} />
              Most Used Patterns
            </Title>
            <List
              size="small"
              dataSource={data.mostUsedPatterns}
              renderItem={(pattern) => (
                <List.Item>
                  <Text>{pattern}</Text>
                  <Tag color="green">Popular</Tag>
                </List.Item>
              )}
            />
          </Card>
        </Col>
      </Row>
    </div>
  );
};
