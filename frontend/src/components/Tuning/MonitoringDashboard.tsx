import React, { useState } from 'react';
import {
  Card,
  Tabs,
  Alert,
  Typography,
  Row,
  Col,
  Statistic,
  Space,
  Button,
  Tag,
  Progress
} from 'antd';
import {
  BarChartOutlined,
  FileTextOutlined,
  ClockCircleOutlined,
  TrophyOutlined,
  ExclamationCircleOutlined,
  CheckCircleOutlined,
  DownloadOutlined,
  ReloadOutlined,
  EyeOutlined
} from '@ant-design/icons';
import { PromptLogsViewer } from './PromptLogsViewer';

const { Title, Text } = Typography;
const { TabPane } = Tabs;

interface MonitoringDashboardProps {
  onDataChange?: () => void;
}

export const MonitoringDashboard: React.FC<MonitoringDashboardProps> = ({ onDataChange }) => {
  const [activeTab, setActiveTab] = useState('logs');

  // Mock data for demonstration
  const performanceMetrics = {
    totalQueries: 1247,
    successfulQueries: 1189,
    failedQueries: 58,
    averageResponseTime: 2.3,
    cacheHitRate: 78.5,
    activeUsers: 23
  };

  const successRate = ((performanceMetrics.successfulQueries / performanceMetrics.totalQueries) * 100).toFixed(1);

  return (
    <div>
      <Alert
        message="AI Performance & Usage Analytics"
        description="Monitor AI system performance, usage patterns, and troubleshoot issues with comprehensive logging and analytics."
        type="info"
        showIcon
        style={{ marginBottom: 24 }}
      />

      {/* Performance Overview */}
      <Row gutter={[16, 16]} style={{ marginBottom: 24 }}>
        <Col xs={24} sm={12} lg={4}>
          <Card>
            <Statistic
              title="Total Queries"
              value={performanceMetrics.totalQueries}
              prefix={<BarChartOutlined />}
              valueStyle={{ color: '#1890ff' }}
            />
          </Card>
        </Col>
        <Col xs={24} sm={12} lg={4}>
          <Card>
            <Statistic
              title="Success Rate"
              value={successRate}
              suffix="%"
              prefix={<CheckCircleOutlined />}
              valueStyle={{ color: '#52c41a' }}
            />
          </Card>
        </Col>
        <Col xs={24} sm={12} lg={4}>
          <Card>
            <Statistic
              title="Failed Queries"
              value={performanceMetrics.failedQueries}
              prefix={<ExclamationCircleOutlined />}
              valueStyle={{ color: '#ff4d4f' }}
            />
          </Card>
        </Col>
        <Col xs={24} sm={12} lg={4}>
          <Card>
            <Statistic
              title="Avg Response"
              value={performanceMetrics.averageResponseTime}
              suffix="s"
              prefix={<ClockCircleOutlined />}
              valueStyle={{ color: '#faad14' }}
            />
          </Card>
        </Col>
        <Col xs={24} sm={12} lg={4}>
          <Card>
            <Statistic
              title="Cache Hit Rate"
              value={performanceMetrics.cacheHitRate}
              suffix="%"
              prefix={<TrophyOutlined />}
              valueStyle={{ color: '#722ed1' }}
            />
          </Card>
        </Col>
        <Col xs={24} sm={12} lg={4}>
          <Card>
            <Statistic
              title="Active Users"
              value={performanceMetrics.activeUsers}
              prefix={<EyeOutlined />}
              valueStyle={{ color: '#13c2c2' }}
            />
          </Card>
        </Col>
      </Row>

      <Tabs activeKey={activeTab} onChange={setActiveTab} size="large">
        <TabPane
          tab={
            <span>
              <FileTextOutlined />
              Prompt Logs
            </span>
          }
          key="logs"
        >
          <Card
            title={
              <Space>
                <FileTextOutlined />
                <span>AI Prompt Logs & Request History</span>
              </Space>
            }
            extra={
              <Space>
                <Tag color="green">Real-time</Tag>
                <Button
                  icon={<DownloadOutlined />}
                  size="small"
                >
                  Export
                </Button>
                <Button
                  icon={<ReloadOutlined />}
                  size="small"
                >
                  Refresh
                </Button>
              </Space>
            }
          >
            <Alert
              message="Prompt Logging"
              description="Monitor all AI interactions, request/response patterns, and system performance in real-time."
              type="info"
              showIcon
              style={{ marginBottom: 16 }}
            />
            <PromptLogsViewer />
          </Card>
        </TabPane>

        <TabPane
          tab={
            <span>
              <BarChartOutlined />
              Performance Analytics
            </span>
          }
          key="analytics"
        >
          <Card
            title={
              <Space>
                <BarChartOutlined />
                <span>System Performance Metrics</span>
              </Space>
            }
          >
            <Row gutter={[24, 24]}>
              <Col xs={24} lg={12}>
                <Card size="small" title="Query Success Rate Trend">
                  <div style={{ textAlign: 'center', padding: '20px' }}>
                    <Progress
                      type="circle"
                      percent={parseFloat(successRate)}
                      format={(percent) => `${percent}%`}
                      strokeColor={{
                        '0%': '#108ee9',
                        '100%': '#87d068',
                      }}
                    />
                    <div style={{ marginTop: 16 }}>
                      <Text type="secondary">Overall Success Rate</Text>
                    </div>
                  </div>
                </Card>
              </Col>
              
              <Col xs={24} lg={12}>
                <Card size="small" title="Cache Performance">
                  <div style={{ textAlign: 'center', padding: '20px' }}>
                    <Progress
                      type="circle"
                      percent={performanceMetrics.cacheHitRate}
                      format={(percent) => `${percent}%`}
                      strokeColor={{
                        '0%': '#ff4d4f',
                        '50%': '#faad14',
                        '100%': '#52c41a',
                      }}
                    />
                    <div style={{ marginTop: 16 }}>
                      <Text type="secondary">Cache Hit Rate</Text>
                    </div>
                  </div>
                </Card>
              </Col>

              <Col xs={24}>
                <Card size="small" title="Response Time Distribution">
                  <Alert
                    message="Coming Soon"
                    description="Detailed response time analytics and performance trends will be available here."
                    type="info"
                    showIcon
                  />
                </Card>
              </Col>
            </Row>
          </Card>
        </TabPane>

        <TabPane
          tab={
            <span>
              <ExclamationCircleOutlined />
              Error Analysis
            </span>
          }
          key="errors"
        >
          <Card
            title={
              <Space>
                <ExclamationCircleOutlined />
                <span>Error Analysis & Troubleshooting</span>
              </Space>
            }
          >
            <Row gutter={[16, 16]}>
              <Col xs={24} lg={8}>
                <Card size="small">
                  <Statistic
                    title="Failed Queries Today"
                    value={performanceMetrics.failedQueries}
                    valueStyle={{ color: '#ff4d4f' }}
                  />
                </Card>
              </Col>
              <Col xs={24} lg={8}>
                <Card size="small">
                  <Statistic
                    title="Error Rate"
                    value={(100 - parseFloat(successRate)).toFixed(1)}
                    suffix="%"
                    valueStyle={{ color: '#ff4d4f' }}
                  />
                </Card>
              </Col>
              <Col xs={24} lg={8}>
                <Card size="small">
                  <Statistic
                    title="Avg Error Resolution"
                    value="2.1"
                    suffix="min"
                    valueStyle={{ color: '#faad14' }}
                  />
                </Card>
              </Col>
            </Row>

            <div style={{ marginTop: 24 }}>
              <Alert
                message="Error Tracking"
                description="Detailed error analysis, common failure patterns, and resolution suggestions will be displayed here."
                type="warning"
                showIcon
              />
            </div>
          </Card>
        </TabPane>

        <TabPane
          tab={
            <span>
              <TrophyOutlined />
              Usage Patterns
            </span>
          }
          key="usage"
        >
          <Card
            title={
              <Space>
                <TrophyOutlined />
                <span>Usage Patterns & User Behavior</span>
              </Space>
            }
          >
            <Row gutter={[16, 16]}>
              <Col xs={24} lg={12}>
                <Card size="small" title="Most Active Users">
                  <Alert
                    message="Coming Soon"
                    description="User activity rankings and usage statistics will be available here."
                    type="info"
                    showIcon
                  />
                </Card>
              </Col>
              
              <Col xs={24} lg={12}>
                <Card size="small" title="Popular Query Types">
                  <Alert
                    message="Coming Soon"
                    description="Analysis of most common query patterns and user preferences will be displayed here."
                    type="info"
                    showIcon
                  />
                </Card>
              </Col>

              <Col xs={24}>
                <Card size="small" title="Peak Usage Times">
                  <Alert
                    message="Coming Soon"
                    description="Time-based usage analytics and system load patterns will be shown here."
                    type="info"
                    showIcon
                  />
                </Card>
              </Col>
            </Row>
          </Card>
        </TabPane>
      </Tabs>
    </div>
  );
};
