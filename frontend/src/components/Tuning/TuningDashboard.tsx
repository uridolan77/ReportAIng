import React, { useState, useEffect } from 'react';
import {
  Card,
  Tabs,
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
  Progress
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
  FileTextOutlined
} from '@ant-design/icons';
import { BusinessTableManager } from './BusinessTableManager';
import { QueryPatternManager } from './QueryPatternManager';
import { BusinessGlossaryManager } from './BusinessGlossaryManager';
import { AISettingsManager } from './AISettingsManager';
import { PromptLogsViewer } from './PromptLogsViewer';
import { PromptTemplateManager } from './PromptTemplateManager';
import { AutoGenerationManager } from '../TuningDashboard/AutoGeneration/AutoGenerationManager';
import { tuningApi } from '../../services/tuningApi';

const { Title, Text } = Typography;
const { TabPane } = Tabs;

interface TuningDashboardData {
  totalTables: number;
  totalColumns: number;
  totalPatterns: number;
  totalGlossaryTerms: number;
  activePromptTemplates: number;
  recentlyUpdatedTables: string[];
  mostUsedPatterns: string[];
  patternUsageStats: Record<string, number>;
}

export const TuningDashboard: React.FC = () => {
  const [dashboardData, setDashboardData] = useState<TuningDashboardData | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [activeTab, setActiveTab] = useState('dashboard');

  useEffect(() => {
    loadDashboardData();
  }, []);

  const loadDashboardData = async () => {
    try {
      setLoading(true);
      const data = await tuningApi.getDashboard();
      setDashboardData(data);
      setError(null);
    } catch (err) {
      setError('Failed to load dashboard data');
      console.error('Error loading dashboard:', err);
    } finally {
      setLoading(false);
    }
  };

  const renderDashboardOverview = () => {
    if (!dashboardData) return null;

    return (
      <div>
        <Title level={2}>
          <BulbOutlined /> AI Tuning Dashboard
        </Title>
        <Text type="secondary">
          Manage business schema documentation, query patterns, and AI training data to improve SQL generation quality.
        </Text>

        <Row gutter={[16, 16]} style={{ marginTop: 24 }}>
          <Col xs={24} sm={12} md={8} lg={6}>
            <Card>
              <Statistic
                title="Business Tables"
                value={dashboardData.totalTables}
                prefix={<TableOutlined />}
                valueStyle={{ color: '#1890ff' }}
              />
            </Card>
          </Col>
          <Col xs={24} sm={12} md={8} lg={6}>
            <Card>
              <Statistic
                title="Documented Columns"
                value={dashboardData.totalColumns}
                prefix={<DatabaseOutlined />}
                valueStyle={{ color: '#52c41a' }}
              />
            </Card>
          </Col>
          <Col xs={24} sm={12} md={8} lg={6}>
            <Card>
              <Statistic
                title="Query Patterns"
                value={dashboardData.totalPatterns}
                prefix={<SearchOutlined />}
                valueStyle={{ color: '#722ed1' }}
              />
            </Card>
          </Col>
          <Col xs={24} sm={12} md={8} lg={6}>
            <Card>
              <Statistic
                title="Glossary Terms"
                value={dashboardData.totalGlossaryTerms}
                prefix={<BookOutlined />}
                valueStyle={{ color: '#fa8c16' }}
              />
            </Card>
          </Col>
        </Row>

        <Row gutter={[16, 16]} style={{ marginTop: 24 }}>
          <Col xs={24} lg={12}>
            <Card title={<><ClockCircleOutlined /> Recently Updated Tables</>}>
              <List
                size="small"
                dataSource={dashboardData.recentlyUpdatedTables}
                renderItem={(table) => (
                  <List.Item>
                    <Tag color="blue">{table}</Tag>
                  </List.Item>
                )}
                locale={{ emptyText: 'No recent updates' }}
              />
            </Card>
          </Col>
          <Col xs={24} lg={12}>
            <Card title={<><TrophyOutlined /> Most Used Patterns</>}>
              <List
                size="small"
                dataSource={dashboardData.mostUsedPatterns}
                renderItem={(pattern, index) => (
                  <List.Item>
                    <Space>
                      <Tag color="green">#{index + 1}</Tag>
                      <Text>{pattern}</Text>
                    </Space>
                  </List.Item>
                )}
                locale={{ emptyText: 'No pattern usage data' }}
              />
            </Card>
          </Col>
        </Row>

        <Row gutter={[16, 16]} style={{ marginTop: 24 }}>
          <Col xs={24}>
            <Card title="Pattern Usage Distribution">
              {Object.entries(dashboardData.patternUsageStats).map(([key, value]) => (
                <div key={key} style={{ marginBottom: 16 }}>
                  <div style={{ display: 'flex', justifyContent: 'space-between', marginBottom: 4 }}>
                    <Text>{key}</Text>
                    <Text strong>{value}</Text>
                  </div>
                  <Progress
                    percent={Math.round((value / Math.max(...Object.values(dashboardData.patternUsageStats))) * 100)}
                    showInfo={false}
                  />
                </div>
              ))}
            </Card>
          </Col>
        </Row>

        <Row gutter={[16, 16]} style={{ marginTop: 24 }}>
          <Col xs={24}>
            <Card>
              <Title level={4}>Quick Actions</Title>
              <Space wrap>
                <Button
                  type="primary"
                  icon={<TableOutlined />}
                  onClick={() => setActiveTab('tables')}
                >
                  Manage Tables
                </Button>
                <Button
                  icon={<SearchOutlined />}
                  onClick={() => setActiveTab('patterns')}
                >
                  Query Patterns
                </Button>
                <Button
                  icon={<BookOutlined />}
                  onClick={() => setActiveTab('glossary')}
                >
                  Business Glossary
                </Button>
                <Button
                  icon={<RobotOutlined />}
                  onClick={() => setActiveTab('auto-generation')}
                  type="dashed"
                >
                  Auto-Generate
                </Button>
                <Button
                  icon={<SettingOutlined />}
                  onClick={() => setActiveTab('settings')}
                >
                  AI Settings
                </Button>
                <Button
                  icon={<FileTextOutlined />}
                  onClick={() => setActiveTab('prompt-templates')}
                >
                  Prompt Templates
                </Button>
                <Button
                  icon={<BulbOutlined />}
                  onClick={loadDashboardData}
                >
                  Refresh Data
                </Button>
              </Space>
            </Card>
          </Col>
        </Row>
      </div>
    );
  };

  if (loading) {
    return (
      <div style={{ textAlign: 'center', padding: '50px' }}>
        <Spin size="large" />
        <div style={{ marginTop: 16 }}>Loading tuning dashboard...</div>
      </div>
    );
  }

  if (error) {
    return (
      <Alert
        message="Error Loading Dashboard"
        description={typeof error === 'string' ? error : error?.message || 'An error occurred'}
        type="error"
        showIcon
        action={
          <Button size="small" onClick={loadDashboardData}>
            Retry
          </Button>
        }
      />
    );
  }

  return (
    <div style={{
      padding: '24px',
      background: '#f5f5f5',
      minHeight: '100vh'
    }}>
      <Tabs
        activeKey={activeTab}
        onChange={setActiveTab}
        type="card"
        size="large"
        className="tuning-dashboard-tabs"
        style={{
          background: 'white',
          borderRadius: '12px',
          padding: '16px',
          boxShadow: '0 4px 16px rgba(0, 0, 0, 0.06)'
        }}
      >
        <TabPane
          tab={
            <span style={{ display: 'flex', alignItems: 'center', gap: '8px', fontSize: '14px', fontWeight: '500' }}>
              <BulbOutlined style={{ fontSize: '16px' }} />
              Dashboard
            </span>
          }
          key="dashboard"
        >
          {renderDashboardOverview()}
        </TabPane>

        <TabPane
          tab={
            <span style={{ display: 'flex', alignItems: 'center', gap: '8px', fontSize: '14px', fontWeight: '500' }}>
              <TableOutlined style={{ fontSize: '16px' }} />
              Business Tables
            </span>
          }
          key="tables"
        >
          <BusinessTableManager onDataChange={loadDashboardData} />
        </TabPane>

        <TabPane
          tab={
            <span style={{ display: 'flex', alignItems: 'center', gap: '8px', fontSize: '14px', fontWeight: '500' }}>
              <SearchOutlined style={{ fontSize: '16px' }} />
              Query Patterns
            </span>
          }
          key="patterns"
        >
          <QueryPatternManager onDataChange={loadDashboardData} />
        </TabPane>

        <TabPane
          tab={
            <span style={{ display: 'flex', alignItems: 'center', gap: '8px', fontSize: '14px', fontWeight: '500' }}>
              <BookOutlined style={{ fontSize: '16px' }} />
              Business Glossary
            </span>
          }
          key="glossary"
        >
          <BusinessGlossaryManager onDataChange={loadDashboardData} />
        </TabPane>

        <TabPane
          tab={
            <span style={{ display: 'flex', alignItems: 'center', gap: '8px', fontSize: '14px', fontWeight: '500' }}>
              <RobotOutlined style={{ fontSize: '16px' }} />
              Auto-Generate
            </span>
          }
          key="auto-generation"
        >
          <AutoGenerationManager onRefresh={loadDashboardData} />
        </TabPane>

        <TabPane
          tab={
            <span style={{ display: 'flex', alignItems: 'center', gap: '8px', fontSize: '14px', fontWeight: '500' }}>
              <SettingOutlined style={{ fontSize: '16px' }} />
              AI Settings
            </span>
          }
          key="settings"
        >
          <AISettingsManager onDataChange={loadDashboardData} />
        </TabPane>

        <TabPane
          tab={
            <span style={{ display: 'flex', alignItems: 'center', gap: '8px', fontSize: '14px', fontWeight: '500' }}>
              <FileTextOutlined style={{ fontSize: '16px' }} />
              Prompt Templates
            </span>
          }
          key="prompt-templates"
        >
          <PromptTemplateManager onDataChange={loadDashboardData} />
        </TabPane>

        <TabPane
          tab={
            <span style={{ display: 'flex', alignItems: 'center', gap: '8px', fontSize: '14px', fontWeight: '500' }}>
              <BugOutlined style={{ fontSize: '16px' }} />
              Prompt Logs
            </span>
          }
          key="prompt-logs"
        >
          <PromptLogsViewer />
        </TabPane>
      </Tabs>
    </div>
  );
};
