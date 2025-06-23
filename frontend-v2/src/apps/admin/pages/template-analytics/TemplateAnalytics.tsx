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
  Tooltip
} from 'antd'
import { 
  DashboardOutlined, 
  ThunderboltOutlined, 
  ExperimentOutlined,
  BarChartOutlined,
  LineChartOutlined,
  TrophyOutlined,
  SettingOutlined,
  ReloadOutlined,
  BellOutlined
} from '@ant-design/icons'
import { PageLayout } from '@shared/components/core/Layout'

// Import existing components
import { ComprehensiveAnalyticsDashboard } from './ComprehensiveAnalyticsDashboard'
import TemplatePerformanceDashboard from './TemplatePerformanceDashboard'
import ABTestingDashboard from './ABTestingDashboard'

// Import hooks for real-time data
import { 
  useGetComprehensiveDashboardQuery,
  useGetPerformanceAlertsQuery,
  useGetABTestsQuery
} from '@shared/store/api/templateAnalyticsApi'

const { Title, Text } = Typography

export const TemplateAnalytics: React.FC = () => {
  const [activeTab, setActiveTab] = useState('overview')
  const [timeRange, setTimeRange] = useState('30d')

  // API hooks for alert counts
  const { data: comprehensiveData } = useGetComprehensiveDashboardQuery({
    startDate: new Date(Date.now() - 30 * 24 * 60 * 60 * 1000).toISOString(),
    endDate: new Date().toISOString()
  })

  const { data: performanceAlerts } = useGetPerformanceAlertsQuery({
    resolved: false
  })

  const { data: abTests } = useGetABTestsQuery({
    status: 'running'
  })

  // Calculate alert counts
  const totalAlerts = (performanceAlerts?.length || 0)
  const runningTests = (abTests?.length || 0)

  const tabItems = [
    {
      key: 'overview',
      label: (
        <Space>
          <DashboardOutlined />
          Analytics Overview
        </Space>
      ),
      children: (
        <div>
          {/* Quick Stats Row */}
          <Row gutter={[16, 16]} style={{ marginBottom: '24px' }}>
            <Col xs={24} sm={12} lg={6}>
              <Card>
                <div style={{ textAlign: 'center' }}>
                  <div style={{ fontSize: '24px', fontWeight: 'bold', color: '#1890ff' }}>
                    {comprehensiveData?.performanceOverview?.totalTemplates || 0}
                  </div>
                  <div style={{ color: '#666' }}>Total Templates</div>
                </div>
              </Card>
            </Col>
            <Col xs={24} sm={12} lg={6}>
              <Card>
                <div style={{ textAlign: 'center' }}>
                  <div style={{ fontSize: '24px', fontWeight: 'bold', color: '#52c41a' }}>
                    {((comprehensiveData?.performanceOverview?.averageSuccessRate || 0) * 100).toFixed(1)}%
                  </div>
                  <div style={{ color: '#666' }}>Avg Success Rate</div>
                </div>
              </Card>
            </Col>
            <Col xs={24} sm={12} lg={6}>
              <Card>
                <div style={{ textAlign: 'center' }}>
                  <div style={{ fontSize: '24px', fontWeight: 'bold', color: '#faad14' }}>
                    {totalAlerts}
                  </div>
                  <div style={{ color: '#666' }}>Active Alerts</div>
                </div>
              </Card>
            </Col>
            <Col xs={24} sm={12} lg={6}>
              <Card>
                <div style={{ textAlign: 'center' }}>
                  <div style={{ fontSize: '24px', fontWeight: 'bold', color: '#722ed1' }}>
                    {runningTests}
                  </div>
                  <div style={{ color: '#666' }}>Running Tests</div>
                </div>
              </Card>
            </Col>
          </Row>

          {/* Main Analytics Dashboard */}
          <ComprehensiveAnalyticsDashboard />
        </div>
      )
    },
    {
      key: 'performance',
      label: (
        <Space>
          <ThunderboltOutlined />
          Performance Monitoring
          {totalAlerts > 0 && (
            <Badge count={totalAlerts} size="small" />
          )}
        </Space>
      ),
      children: <TemplatePerformanceDashboard />
    },
    {
      key: 'experiments',
      label: (
        <Space>
          <ExperimentOutlined />
          A/B Testing
          {runningTests > 0 && (
            <Badge count={runningTests} size="small" status="processing" />
          )}
        </Space>
      ),
      children: <ABTestingDashboard />
    },
    {
      key: 'insights',
      label: (
        <Space>
          <BarChartOutlined />
          Advanced Insights
        </Space>
      ),
      children: (
        <div>
          <Alert
            message="Advanced Analytics Insights"
            description="Deep dive into template performance patterns, user behavior analysis, and predictive insights."
            type="info"
            showIcon
            style={{ marginBottom: '24px' }}
          />
          
          <Row gutter={[16, 16]}>
            <Col span={24}>
              <Card title="Performance Trends Analysis">
                <div style={{ textAlign: 'center', padding: '40px 0' }}>
                  <LineChartOutlined style={{ fontSize: '48px', color: '#d9d9d9', marginBottom: '16px' }} />
                  <Title level={4}>Advanced Insights Coming Soon</Title>
                  <Text type="secondary">
                    Comprehensive trend analysis, pattern recognition, and predictive insights
                    will be available in this section.
                  </Text>
                </div>
              </Card>
            </Col>
          </Row>

          <Row gutter={[16, 16]} style={{ marginTop: '16px' }}>
            <Col xs={24} lg={12}>
              <Card title="Usage Patterns" size="small">
                <div style={{ textAlign: 'center', padding: '20px 0' }}>
                  <BarChartOutlined style={{ fontSize: '32px', color: '#1890ff' }} />
                  <div style={{ marginTop: '8px' }}>
                    <Text>Template usage pattern analysis</Text>
                  </div>
                </div>
              </Card>
            </Col>
            <Col xs={24} lg={12}>
              <Card title="Quality Metrics" size="small">
                <div style={{ textAlign: 'center', padding: '20px 0' }}>
                  <TrophyOutlined style={{ fontSize: '32px', color: '#52c41a' }} />
                  <div style={{ marginTop: '8px' }}>
                    <Text>Content quality scoring trends</Text>
                  </div>
                </div>
              </Card>
            </Col>
          </Row>
        </div>
      )
    }
  ]

  return (
    <PageLayout
      title="Template Analytics"
      subtitle="Comprehensive analytics, performance monitoring, and experimentation platform"
      extra={
        <Space>
          {(totalAlerts > 0 || runningTests > 0) && (
            <Alert
              message={`${totalAlerts} Alert${totalAlerts !== 1 ? 's' : ''} • ${runningTests} Running Test${runningTests !== 1 ? 's' : ''}`}
              type={totalAlerts > 0 ? 'warning' : 'info'}
              showIcon
              style={{ marginBottom: 0 }}
            />
          )}
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

export default TemplateAnalytics
