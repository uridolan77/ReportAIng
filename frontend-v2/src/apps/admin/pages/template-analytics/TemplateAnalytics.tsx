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
  BellOutlined,
  SafetyOutlined,
  RocketOutlined,
  StarOutlined,
  BulbOutlined,
  PlusOutlined,
  ImportOutlined,
  ExportOutlined
} from '@ant-design/icons'
import { PageLayout } from '@shared/components/core/Layout'

// Import existing components
import { ComprehensiveAnalyticsDashboard } from './ComprehensiveAnalyticsDashboard'
import TemplatePerformanceDashboard from './TemplatePerformanceDashboard'
import ABTestingDashboard from './ABTestingDashboard'
import TemplateManagementHub from './TemplateManagementHub'
import { TemplateFeatures } from './TemplateFeatures'

// Import hooks for real-time data
import {
  useGetComprehensiveDashboardQuery,
  useGetPerformanceAlertsQuery,
  useGetABTestsQuery,
  useGetTemplateManagementDashboardQuery
} from '@shared/store/api/templateAnalyticsApi'

const { Title, Text } = Typography

export const TemplateAnalytics: React.FC = () => {
  const [activeTab, setActiveTab] = useState('overview')
  const [timeRange, setTimeRange] = useState('30d')
  const [selectedTemplate, setSelectedTemplate] = useState<string>('')

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

  const {
    data: managementData,
    isLoading: isLoadingManagement,
    refetch: refetchManagement
  } = useGetTemplateManagementDashboardQuery()

  // Calculate alert counts
  const totalAlerts = (performanceAlerts?.length || 0)
  const runningTests = (abTests?.length || 0)

  // Calculate management stats
  const totalTemplates = managementData?.templates?.length || 0
  const activeTemplates = managementData?.templates?.filter(t => t.isActive)?.length || 0
  const templatesNeedingReview = managementData?.templates?.filter(t =>
    t.metrics && t.metrics.successRate < 0.8
  )?.length || 0

  const handleTemplateSelect = (templateKey: string) => {
    setSelectedTemplate(templateKey)
    setActiveTab('features') // Switch to features tab when template is selected
  }

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
    },
    {
      key: 'management',
      label: (
        <Space>
          <SafetyOutlined />
          Template Management
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
                    {totalTemplates}
                  </div>
                  <div style={{ color: '#666' }}>Total Templates</div>
                </div>
              </Card>
            </Col>
            <Col xs={24} sm={12} lg={6}>
              <Card>
                <div style={{ textAlign: 'center' }}>
                  <div style={{ fontSize: '24px', fontWeight: 'bold', color: '#52c41a' }}>
                    {activeTemplates}
                  </div>
                  <div style={{ color: '#666' }}>Active Templates</div>
                </div>
              </Card>
            </Col>
            <Col xs={24} sm={12} lg={6}>
              <Card>
                <div style={{ textAlign: 'center' }}>
                  <div style={{ fontSize: '24px', fontWeight: 'bold', color: '#faad14' }}>
                    {templatesNeedingReview}
                  </div>
                  <div style={{ color: '#666' }}>Need Review</div>
                </div>
              </Card>
            </Col>
            <Col xs={24} sm={12} lg={6}>
              <Card>
                <div style={{ textAlign: 'center' }}>
                  <div style={{ fontSize: '24px', fontWeight: 'bold', color: '#722ed1' }}>
                    {managementData?.totalIntentTypes || 0}
                  </div>
                  <div style={{ color: '#666' }}>Intent Types</div>
                </div>
              </Card>
            </Col>
          </Row>

          {/* Template Management Hub */}
          <TemplateManagementHub onTemplateSelect={handleTemplateSelect} />
        </div>
      )
    },
    {
      key: 'features',
      label: (
        <Space>
          <RocketOutlined />
          AI Features
          {selectedTemplate && (
            <Badge status="processing" />
          )}
        </Space>
      ),
      children: (
        <div>
          {selectedTemplate ? (
            <div>
              {/* Selected Template Info */}
              <Card style={{ marginBottom: '16px' }}>
                <Row gutter={16} align="middle">
                  <Col span={16}>
                    <Space>
                      <Text strong>Selected Template:</Text>
                      <Badge status="success" text={selectedTemplate} />
                    </Space>
                  </Col>
                  <Col span={8} style={{ textAlign: 'right' }}>
                    <Button
                      onClick={() => setSelectedTemplate('')}
                      size="small"
                    >
                      Clear Selection
                    </Button>
                  </Col>
                </Row>
              </Card>

              {/* Template Features Component with selected template */}
              <TemplateFeatures selectedTemplateKey={selectedTemplate} />
            </div>
          ) : (
            <Card>
              <div style={{ textAlign: 'center', padding: '60px 0' }}>
                <RocketOutlined style={{ fontSize: '64px', color: '#d9d9d9', marginBottom: '24px' }} />
                <Title level={3}>AI-Powered Template Features</Title>
                <Text type="secondary" style={{ fontSize: '16px', display: 'block', marginBottom: '24px' }}>
                  Select a template from the Management tab to access AI features including:
                </Text>

                <Row gutter={[16, 16]} style={{ marginTop: '32px', maxWidth: '600px', margin: '32px auto 0' }}>
                  <Col span={12}>
                    <Card size="small" style={{ textAlign: 'center' }}>
                      <StarOutlined style={{ fontSize: '24px', color: '#faad14', marginBottom: '8px' }} />
                      <div style={{ fontWeight: 'bold' }}>Quality Analysis</div>
                      <Text type="secondary" style={{ fontSize: '12px' }}>
                        AI-powered content scoring
                      </Text>
                    </Card>
                  </Col>
                  <Col span={12}>
                    <Card size="small" style={{ textAlign: 'center' }}>
                      <BulbOutlined style={{ fontSize: '24px', color: '#52c41a', marginBottom: '8px' }} />
                      <div style={{ fontWeight: 'bold' }}>Optimization</div>
                      <Text type="secondary" style={{ fontSize: '12px' }}>
                        Strategic improvements
                      </Text>
                    </Card>
                  </Col>
                  <Col span={12}>
                    <Card size="small" style={{ textAlign: 'center' }}>
                      <ExperimentOutlined style={{ fontSize: '24px', color: '#722ed1', marginBottom: '8px' }} />
                      <div style={{ fontWeight: 'bold' }}>A/B Variants</div>
                      <Text type="secondary" style={{ fontSize: '12px' }}>
                        Automated variant generation
                      </Text>
                    </Card>
                  </Col>
                  <Col span={12}>
                    <Card size="small" style={{ textAlign: 'center' }}>
                      <BarChartOutlined style={{ fontSize: '24px', color: '#1890ff', marginBottom: '8px' }} />
                      <div style={{ fontWeight: 'bold' }}>ML Predictions</div>
                      <Text type="secondary" style={{ fontSize: '12px' }}>
                        Performance forecasting
                      </Text>
                    </Card>
                  </Col>
                </Row>

                <div style={{ marginTop: '32px' }}>
                  <Space>
                    <Button
                      type="primary"
                      size="large"
                      onClick={() => setActiveTab('management')}
                    >
                      Go to Template Management
                    </Button>
                    <Button
                      size="large"
                      onClick={() => {
                        // Set a demo template and switch to features
                        if (managementData?.templates && managementData.templates.length > 0) {
                          setSelectedTemplate(managementData.templates[0].templateKey)
                          setActiveTab('features')
                        }
                      }}
                    >
                      Try Demo Features
                    </Button>
                  </Space>
                </div>
              </div>
            </Card>
          )}
        </div>
      )
    },
    {
      key: 'operations',
      label: (
        <Space>
          <SettingOutlined />
          Operations
        </Space>
      ),
      children: (
        <div>
          <Alert
            message="Template Operations"
            description="Bulk operations, import/export, and administrative functions for template management."
            type="info"
            showIcon
            style={{ marginBottom: '24px' }}
          />

          <Row gutter={[16, 16]}>
            <Col xs={24} lg={8}>
              <Card title="Bulk Operations" size="small">
                <Space direction="vertical" style={{ width: '100%' }}>
                  <Button block icon={<PlusOutlined />}>
                    Create Multiple Templates
                  </Button>
                  <Button block icon={<StarOutlined />}>
                    Bulk Quality Analysis
                  </Button>
                  <Button block icon={<RocketOutlined />}>
                    Batch Optimization
                  </Button>
                </Space>
              </Card>
            </Col>

            <Col xs={24} lg={8}>
              <Card title="Import/Export" size="small">
                <Space direction="vertical" style={{ width: '100%' }}>
                  <Button block icon={<ImportOutlined />}>
                    Import Templates
                  </Button>
                  <Button block icon={<ExportOutlined />}>
                    Export Templates
                  </Button>
                  <Button block icon={<ExportOutlined />}>
                    Export Analytics
                  </Button>
                </Space>
              </Card>
            </Col>

            <Col xs={24} lg={8}>
              <Card title="Administration" size="small">
                <Space direction="vertical" style={{ width: '100%' }}>
                  <Button block icon={<SettingOutlined />}>
                    Template Policies
                  </Button>
                  <Button block icon={<SafetyOutlined />}>
                    Access Control
                  </Button>
                  <Button block icon={<BarChartOutlined />}>
                    Usage Reports
                  </Button>
                </Space>
              </Card>
            </Col>
          </Row>

          <Card title="Recent Operations" style={{ marginTop: '16px' }}>
            <div style={{ textAlign: 'center', padding: '40px 0' }}>
              <Text type="secondary">No recent operations to display</Text>
            </div>
          </Card>
        </div>
      )
    }
  ]

  return (
    <PageLayout
      title="Template Analytics"
      subtitle="Comprehensive analytics, performance monitoring, management, and AI-powered features"
      extra={
        <Space>
          {(totalAlerts > 0 || runningTests > 0 || templatesNeedingReview > 0) && (
            <Alert
              message={`${totalAlerts} Alert${totalAlerts !== 1 ? 's' : ''} • ${runningTests} Running Test${runningTests !== 1 ? 's' : ''} • ${templatesNeedingReview} Need Review`}
              type={totalAlerts > 0 || templatesNeedingReview > 0 ? 'warning' : 'info'}
              showIcon
              style={{ marginBottom: 0 }}
            />
          )}
          <Tooltip title="Refresh data">
            <Button
              icon={<ReloadOutlined />}
              onClick={() => refetchManagement()}
              loading={isLoadingManagement}
            />
          </Tooltip>
          <Button icon={<PlusOutlined />} type="primary">
            New Template
          </Button>
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
