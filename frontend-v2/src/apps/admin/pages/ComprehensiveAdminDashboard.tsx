import React, { useState } from 'react'
import { 
  Tabs, 
  Card, 
  Row, 
  Col, 
  Button, 
  Space, 
  Typography, 
  Badge,
  Alert,
  Tooltip,
  Statistic,
  Progress
} from 'antd'
import { 
  DashboardOutlined, 
  BarChartOutlined, 
  FileTextOutlined,
  TrophyOutlined,
  UserOutlined,
  DatabaseOutlined,
  ThunderboltOutlined,
  BugOutlined,
  CheckCircleOutlined,
  ClockCircleOutlined,
  SettingOutlined,
  ReloadOutlined,
  DownloadOutlined
} from '@ant-design/icons'
import { PageLayout } from '@shared/components/core/Layout'

// Import existing dashboard components
import Dashboard from './Dashboard'
import Analytics from './Analytics'

const { Title, Text } = Typography

export const ComprehensiveAdminDashboard: React.FC = () => {
  const [activeTab, setActiveTab] = useState('overview')
  const [timeRange, setTimeRange] = useState('7d')

  // Mock data for overview - in real implementation, this would come from APIs
  const overviewStats = {
    totalUsers: 1247,
    activeUsers: 892,
    totalQueries: 15420,
    successRate: 94.2,
    avgResponseTime: 1.8,
    systemHealth: 98.5,
    costThisMonth: 2847.50,
    errorRate: 0.8
  }

  const tabItems = [
    {
      key: 'overview',
      label: (
        <Space>
          <DashboardOutlined />
          System Overview
        </Space>
      ),
      children: (
        <div>
          {/* Key Metrics Row */}
          <Row gutter={[16, 16]} style={{ marginBottom: '24px' }}>
            <Col xs={24} sm={12} lg={6}>
              <Card>
                <Statistic
                  title="Total Users"
                  value={overviewStats.totalUsers}
                  prefix={<UserOutlined />}
                  valueStyle={{ color: '#1890ff' }}
                />
              </Card>
            </Col>
            <Col xs={24} sm={12} lg={6}>
              <Card>
                <Statistic
                  title="Active Users"
                  value={overviewStats.activeUsers}
                  prefix={<CheckCircleOutlined />}
                  valueStyle={{ color: '#52c41a' }}
                />
              </Card>
            </Col>
            <Col xs={24} sm={12} lg={6}>
              <Card>
                <Statistic
                  title="Total Queries"
                  value={overviewStats.totalQueries}
                  prefix={<DatabaseOutlined />}
                  valueStyle={{ color: '#722ed1' }}
                />
              </Card>
            </Col>
            <Col xs={24} sm={12} lg={6}>
              <Card>
                <Statistic
                  title="Success Rate"
                  value={overviewStats.successRate}
                  suffix="%"
                  prefix={<TrophyOutlined />}
                  valueStyle={{ color: '#52c41a' }}
                />
              </Card>
            </Col>
          </Row>

          {/* Performance Metrics */}
          <Row gutter={[16, 16]} style={{ marginBottom: '24px' }}>
            <Col xs={24} lg={8}>
              <Card title="System Health" size="small">
                <div style={{ textAlign: 'center' }}>
                  <Progress
                    type="circle"
                    percent={overviewStats.systemHealth}
                    format={percent => `${percent}%`}
                    strokeColor={{
                      '0%': '#108ee9',
                      '100%': '#87d068',
                    }}
                  />
                  <div style={{ marginTop: '16px' }}>
                    <Text type="secondary">All systems operational</Text>
                  </div>
                </div>
              </Card>
            </Col>
            <Col xs={24} lg={8}>
              <Card title="Response Time" size="small">
                <div style={{ textAlign: 'center' }}>
                  <div style={{ fontSize: '32px', fontWeight: 'bold', color: '#1890ff' }}>
                    {overviewStats.avgResponseTime}s
                  </div>
                  <div style={{ marginTop: '8px' }}>
                    <Text type="secondary">Average response time</Text>
                  </div>
                  <div style={{ marginTop: '16px' }}>
                    <Badge status="success" text="Within SLA" />
                  </div>
                </div>
              </Card>
            </Col>
            <Col xs={24} lg={8}>
              <Card title="Cost Overview" size="small">
                <div style={{ textAlign: 'center' }}>
                  <div style={{ fontSize: '24px', fontWeight: 'bold', color: '#faad14' }}>
                    ${overviewStats.costThisMonth.toFixed(2)}
                  </div>
                  <div style={{ marginTop: '8px' }}>
                    <Text type="secondary">This month</Text>
                  </div>
                  <div style={{ marginTop: '16px' }}>
                    <Badge status="processing" text="12% vs last month" />
                  </div>
                </div>
              </Card>
            </Col>
          </Row>

          {/* Alerts and Status */}
          <Row gutter={[16, 16]}>
            <Col xs={24} lg={16}>
              <Card title="Recent Activity" size="small">
                <div style={{ textAlign: 'center', padding: '40px 0' }}>
                  <ClockCircleOutlined style={{ fontSize: '48px', color: '#d9d9d9', marginBottom: '16px' }} />
                  <Title level={4}>Activity Feed</Title>
                  <Text type="secondary">
                    Recent system activities and user interactions will be displayed here.
                  </Text>
                </div>
              </Card>
            </Col>
            <Col xs={24} lg={8}>
              <Card title="System Alerts" size="small">
                <Space direction="vertical" style={{ width: '100%' }}>
                  <Alert
                    message="System Update Available"
                    description="New features and security updates"
                    type="info"
                    showIcon
                    closable
                  />
                  <Alert
                    message={`Error Rate: ${overviewStats.errorRate}%`}
                    description="Within acceptable limits"
                    type="success"
                    showIcon
                  />
                </Space>
              </Card>
            </Col>
          </Row>
        </div>
      )
    },
    {
      key: 'dashboard',
      label: (
        <Space>
          <DashboardOutlined />
          Admin Dashboard
        </Space>
      ),
      children: <Dashboard />
    },
    {
      key: 'analytics',
      label: (
        <Space>
          <BarChartOutlined />
          Analytics & Reports
        </Space>
      ),
      children: <Analytics />
    },
    {
      key: 'reports',
      label: (
        <Space>
          <FileTextOutlined />
          Export & Reports
        </Space>
      ),
      children: (
        <div>
          <Alert
            message="Report Generation"
            description="Generate and export comprehensive reports for system analytics, user activity, and performance metrics."
            type="info"
            showIcon
            style={{ marginBottom: '24px' }}
          />
          
          <Row gutter={[16, 16]}>
            <Col xs={24} lg={8}>
              <Card title="System Reports" size="small">
                <Space direction="vertical" style={{ width: '100%' }}>
                  <Button block icon={<DownloadOutlined />}>
                    System Health Report
                  </Button>
                  <Button block icon={<DownloadOutlined />}>
                    Performance Analytics
                  </Button>
                  <Button block icon={<DownloadOutlined />}>
                    Error Analysis Report
                  </Button>
                </Space>
              </Card>
            </Col>
            
            <Col xs={24} lg={8}>
              <Card title="User Reports" size="small">
                <Space direction="vertical" style={{ width: '100%' }}>
                  <Button block icon={<DownloadOutlined />}>
                    User Activity Report
                  </Button>
                  <Button block icon={<DownloadOutlined />}>
                    Usage Statistics
                  </Button>
                  <Button block icon={<DownloadOutlined />}>
                    Access Logs
                  </Button>
                </Space>
              </Card>
            </Col>
            
            <Col xs={24} lg={8}>
              <Card title="Business Reports" size="small">
                <Space direction="vertical" style={{ width: '100%' }}>
                  <Button block icon={<DownloadOutlined />}>
                    Cost Analysis
                  </Button>
                  <Button block icon={<DownloadOutlined />}>
                    ROI Dashboard
                  </Button>
                  <Button block icon={<DownloadOutlined />}>
                    Executive Summary
                  </Button>
                </Space>
              </Card>
            </Col>
          </Row>
        </div>
      )
    }
  ]

  return (
    <PageLayout
      title="Admin Dashboard"
      subtitle="Comprehensive system overview, analytics, and reporting platform"
      extra={
        <Space>
          <Tooltip title="Refresh data">
            <Button icon={<ReloadOutlined />} />
          </Tooltip>
          <Button icon={<SettingOutlined />}>
            Configure
          </Button>
        </Space>
      }
    >
      <Tabs
        activeKey={activeTab}
        onChange={setActiveTab}
        items={tabItems}
        size="large"
        style={{ marginTop: '16px' }}
      />
    </PageLayout>
  )
}

export default ComprehensiveAdminDashboard
