import React, { useState } from 'react'
import {
  Card,
  Row,
  Col,
  Button,
  Space,
  Typography,
  Alert,
  Statistic,
  Tabs,
  Spin
} from 'antd'
import {
  SettingOutlined,
  ReloadOutlined,
  ThunderboltOutlined,
  DollarOutlined,
  ClockCircleOutlined,
  BarChartOutlined,
  CheckCircleOutlined,
  DatabaseOutlined,
  ApiOutlined
} from '@ant-design/icons'
import { AIFeatureWrapper } from '@shared/components/ai/common/AIFeatureWrapper'
import {
  LLMProviderManager,
  ModelPerformanceAnalytics,
  CostOptimizationPanel,
  AIConfigurationManager
} from '@shared/components/ai/management'
import {
  useGetDashboardSummaryQuery,
  useGetProvidersQuery,
  useGetProviderHealthQuery
} from '@shared/store/api/llmManagementApi'

const { Title, Text } = Typography

/**
 * LLMManagementDashboard - Production LLM provider management interface
 *
 * Features:
 * - Real-time provider health monitoring
 * - Model configuration management
 * - Performance analytics and metrics
 * - Cost tracking and optimization
 * - Usage analytics and reporting
 * - Provider configuration management
 */
export const LLMManagementDashboard: React.FC = () => {
  // Real LLM Management API data
  const { data: dashboardSummary, isLoading: summaryLoading, refetch: refetchSummary } = useGetDashboardSummaryQuery()
  const { data: providers, isLoading: providersLoading, refetch: refetchProviders } = useGetProvidersQuery()
  const { data: providerHealth, isLoading: healthLoading, refetch: refetchHealth } = useGetProviderHealthQuery()

  // Loading state
  const isLoading = summaryLoading || providersLoading || healthLoading

  // Refresh all data
  const handleRefreshAll = () => {
    refetchSummary()
    refetchProviders()
    refetchHealth()
  }

  // Check for unhealthy providers
  const hasUnhealthyProviders = providerHealth?.some(p => !p.isHealthy) || false

  // Render dashboard summary cards
  const renderSummaryCards = () => (
    <Row gutter={[16, 16]} style={{ marginBottom: 24 }}>
      <Col xs={24} sm={12} lg={6}>
        <Card>
          <Statistic
            title="Active Providers"
            value={dashboardSummary?.providers.enabled || 0}
            suffix={`/ ${dashboardSummary?.providers.total || 0}`}
            prefix={<ThunderboltOutlined />}
            valueStyle={{ color: '#3f8600' }}
          />
        </Card>
      </Col>
      <Col xs={24} sm={12} lg={6}>
        <Card>
          <Statistic
            title="Healthy Providers"
            value={dashboardSummary?.providers.healthy || 0}
            suffix={`/ ${dashboardSummary?.providers.total || 0}`}
            prefix={<CheckCircleOutlined />}
            valueStyle={{ color: '#52c41a' }}
          />
        </Card>
      </Col>
      <Col xs={24} sm={12} lg={6}>
        <Card>
          <Statistic
            title="Avg Response Time"
            value={dashboardSummary?.performance.averageResponseTime || 0}
            suffix="ms"
            prefix={<ClockCircleOutlined />}
            valueStyle={{ color: '#1890ff' }}
            precision={0}
          />
        </Card>
      </Col>
      <Col xs={24} sm={12} lg={6}>
        <Card>
          <Statistic
            title="Current Month Cost"
            value={dashboardSummary?.costs.currentMonth || 0}
            prefix={<DollarOutlined />}
            valueStyle={{ color: '#cf1322' }}
            precision={2}
          />
        </Card>
      </Col>
    </Row>
  )

  // Tab items for the dashboard
  const tabItems = [
    {
      key: 'overview',
      label: (
        <Space>
          <DatabaseOutlined />
          <span>Overview</span>
        </Space>
      ),
      children: (
        <div>
          {renderSummaryCards()}
          <LLMProviderManager
            showMetrics={true}
            showConfiguration={true}
            compact={false}
          />
        </div>
      )
    },
    {
      key: 'providers',
      label: (
        <Space>
          <ApiOutlined />
          <span>Providers</span>
        </Space>
      ),
      children: (
        <LLMProviderManager
          showMetrics={true}
          showConfiguration={true}
          compact={false}
        />
      )
    },
    {
      key: 'performance',
      label: (
        <Space>
          <BarChartOutlined />
          <span>Performance</span>
        </Space>
      ),
      children: (
        <ModelPerformanceAnalytics
          timeRange={7}
          showComparison={true}
          showRecommendations={true}
          compact={false}
        />
      )
    },
    {
      key: 'cost',
      label: (
        <Space>
          <DollarOutlined />
          <span>Cost Management</span>
        </Space>
      ),
      children: (
        <CostOptimizationPanel
          timeRange={30}
          showRecommendations={true}
          showProjections={true}
          compact={false}
        />
      )
    },
    {
      key: 'configuration',
      label: (
        <Space>
          <SettingOutlined />
          <span>Configuration</span>
        </Space>
      ),
      children: (
        <AIConfigurationManager
          showAdvanced={true}
          showPresets={true}
        />
      )
    }
  ]

  return (
    <AIFeatureWrapper feature="llmManagementEnabled">
      <div style={{ padding: 24 }}>
        {/* Header */}
        <div style={{
          display: 'flex',
          justifyContent: 'space-between',
          alignItems: 'center',
          marginBottom: 24
        }}>
          <div>
            <Title level={2} style={{ margin: 0 }}>
              <Space>
                <SettingOutlined />
                LLM Management
              </Space>
            </Title>
            <Text type="secondary">
              Manage AI model providers, monitor performance, and optimize costs
            </Text>
          </div>
          <Button
            icon={<ReloadOutlined />}
            loading={isLoading}
            onClick={handleRefreshAll}
          >
            Refresh
          </Button>
        </div>

        {/* Health Alert */}
        {hasUnhealthyProviders && (
          <Alert
            message="Provider Health Issues"
            description="Some providers are experiencing issues. Check the status and configuration in the Providers tab."
            type="warning"
            showIcon
            closable
            style={{ marginBottom: 16 }}
          />
        )}

        {/* Loading State */}
        {isLoading && (
          <div style={{ textAlign: 'center', padding: 50 }}>
            <Spin size="large" />
            <div style={{ marginTop: 16 }}>
              <Text type="secondary">Loading LLM management data...</Text>
            </div>
          </div>
        )}

        {/* Main Content */}
        {!isLoading && <Tabs items={tabItems} defaultActiveKey="overview" />}
      </div>
    </AIFeatureWrapper>
  )
}

export default LLMManagementDashboard
