import React, { useState } from 'react'
import { 
  Tabs, 
  Card, 
  Row, 
  Col, 
  Button, 
  Space, 
  Typography, 
  Alert,
  Statistic,
  Select,
  message,
  Badge
} from 'antd'
import { 
  DashboardOutlined, 
  ThunderboltOutlined, 
  DollarOutlined,
  WarningOutlined,
  TrophyOutlined,
  ToolOutlined,
  BarChartOutlined,
  WalletOutlined,
  LineChartOutlined
} from '@ant-design/icons'
import { PageLayout } from '@shared/components/core/Layout'
import { 
  usePerformanceMonitoring, 
  usePerformanceAlerts, 
  usePerformanceBenchmarks 
} from '@shared/hooks/usePerformanceMonitoring'
import { useCostAlerts } from '@shared/hooks/useCostMetrics'
import type { PerformanceEntityType } from '@shared/types/performance'

// Import existing components
import { CostDashboard } from '@shared/components/cost/CostDashboard'
import { BudgetManagementComponent } from '@shared/components/cost/BudgetManagement'
import { useCostMetrics } from '@shared/hooks/useCostMetrics'

const { Title, Text } = Typography

// Cost Analytics Component
const CostAnalyticsTab: React.FC = () => {
  const { analytics, recommendations } = useCostMetrics('30d')

  return (
    <div>
      <Row gutter={[16, 16]}>
        <Col span={24}>
          <Card title="Cost Analytics Overview">
            <Row gutter={[16, 16]}>
              <Col xs={24} sm={12} md={6}>
                <Card>
                  <div style={{ textAlign: 'center' }}>
                    <div style={{ fontSize: '24px', fontWeight: 'bold', color: '#1890ff' }}>
                      ${analytics?.totalCost?.toLocaleString() || '0'}
                    </div>
                    <div style={{ color: '#666' }}>Total Cost (30 days)</div>
                  </div>
                </Card>
              </Col>
              <Col xs={24} sm={12} md={6}>
                <Card>
                  <div style={{ textAlign: 'center' }}>
                    <div style={{ fontSize: '24px', fontWeight: 'bold', color: '#52c41a' }}>
                      ${analytics?.dailyCost?.toFixed(2) || '0'}
                    </div>
                    <div style={{ color: '#666' }}>Average Daily Cost</div>
                  </div>
                </Card>
              </Col>
              <Col xs={24} sm={12} md={6}>
                <Card>
                  <div style={{ textAlign: 'center' }}>
                    <div style={{ fontSize: '24px', fontWeight: 'bold', color: '#faad14' }}>
                      {((analytics?.costEfficiency || 0) * 100).toFixed(1)}%
                    </div>
                    <div style={{ color: '#666' }}>Cost Efficiency</div>
                  </div>
                </Card>
              </Col>
              <Col xs={24} sm={12} md={6}>
                <Card>
                  <div style={{ textAlign: 'center' }}>
                    <div style={{ fontSize: '24px', fontWeight: 'bold', color: '#f5222d' }}>
                      ${analytics?.costSavingsOpportunities?.toLocaleString() || '0'}
                    </div>
                    <div style={{ color: '#666' }}>Potential Savings</div>
                  </div>
                </Card>
              </Col>
            </Row>
          </Card>
        </Col>
      </Row>

      {/* Cost Breakdown by Provider */}
      <Row gutter={[16, 16]} style={{ marginTop: '16px' }}>
        <Col span={24}>
          <Card title="Cost Breakdown by Provider">
            {analytics?.costByProvider && Object.keys(analytics.costByProvider).length > 0 ? (
              <Row gutter={[16, 16]}>
                {Object.entries(analytics.costByProvider).map(([provider, cost]) => (
                  <Col key={provider} xs={24} sm={12} md={8} lg={6}>
                    <Card size="small">
                      <div style={{ textAlign: 'center' }}>
                        <div style={{ fontSize: '18px', fontWeight: 'bold' }}>
                          ${cost.toLocaleString()}
                        </div>
                        <div style={{ color: '#666', textTransform: 'capitalize' }}>
                          {provider}
                        </div>
                      </div>
                    </Card>
                  </Col>
                ))}
              </Row>
            ) : (
              <Text type="secondary">No provider cost data available</Text>
            )}
          </Card>
        </Col>
      </Row>

      {/* Recommendations Preview */}
      {recommendations && recommendations.recommendations.length > 0 && (
        <Row gutter={[16, 16]} style={{ marginTop: '16px' }}>
          <Col span={24}>
            <Card
              title="Top Cost Optimization Recommendations"
              extra={<Text type="secondary">{recommendations.recommendations.length} recommendations</Text>}
            >
              <Row gutter={[16, 16]}>
                {recommendations.recommendations.slice(0, 3).map((rec) => (
                  <Col key={rec.id} xs={24} md={8}>
                    <Card size="small" style={{ height: '100%' }}>
                      <div style={{ marginBottom: '8px' }}>
                        <Text strong>{rec.title}</Text>
                      </div>
                      <div style={{ marginBottom: '8px', fontSize: '12px', color: '#666' }}>
                        {rec.description}
                      </div>
                      <div style={{
                        display: 'flex',
                        justifyContent: 'space-between',
                        alignItems: 'center'
                      }}>
                        <Text type="success" strong>
                          ${rec.potentialSavings.toLocaleString()}
                        </Text>
                        <Space size="small">
                          <Text style={{ fontSize: '11px' }}>{rec.impact} Impact</Text>
                          <Text style={{ fontSize: '11px' }}>{rec.effort} Effort</Text>
                        </Space>
                      </div>
                    </Card>
                  </Col>
                ))}
              </Row>
            </Card>
          </Col>
        </Row>
      )}
    </div>
  )
}

// Cost Forecast Component
const CostForecastTab: React.FC = () => {
  const { forecast } = useCostMetrics('30d')

  return (
    <Card title="Cost Forecast">
      {forecast && forecast.forecast.length > 0 ? (
        <div>
          <Row gutter={[16, 16]} style={{ marginBottom: '16px' }}>
            <Col span={24}>
              <Alert
                message={`Forecast Accuracy: ${(forecast.accuracy * 100).toFixed(1)}%`}
                description={`Methodology: ${forecast.methodology}`}
                type="info"
                showIcon
              />
            </Col>
          </Row>

          <Row gutter={[16, 16]}>
            {forecast.forecast.slice(0, 7).map((item, index) => (
              <Col key={index} xs={24} sm={12} md={8} lg={6} xl={4}>
                <Card size="small">
                  <div style={{ textAlign: 'center' }}>
                    <div style={{ fontSize: '16px', fontWeight: 'bold' }}>
                      ${item.predictedCost.toFixed(2)}
                    </div>
                    <div style={{ color: '#666', fontSize: '12px' }}>
                      {new Date(item.date).toLocaleDateString()}
                    </div>
                    <div style={{ color: '#999', fontSize: '11px' }}>
                      {(item.confidence * 100).toFixed(0)}% confidence
                    </div>
                  </div>
                </Card>
              </Col>
            ))}
          </Row>
        </div>
      ) : (
        <Text type="secondary">No forecast data available</Text>
      )}
    </Card>
  )
}

// Performance Overview Component
const PerformanceOverviewTab: React.FC = () => {
  const [selectedEntity, setSelectedEntity] = useState<{type: PerformanceEntityType, id: string}>({
    type: 'system',
    id: 'global'
  })

  const { 
    metrics, 
    bottlenecks, 
    suggestions, 
    autoTune, 
    autoTuneLoading,
    isLoading 
  } = usePerformanceMonitoring(selectedEntity.type, selectedEntity.id)

  const handleAutoTune = async () => {
    try {
      await autoTune()
      message.success('Auto-tuning completed successfully')
    } catch (error) {
      message.error('Auto-tuning failed')
    }
  }

  const getScoreColor = (score: number) => {
    if (score >= 80) return '#52c41a'
    if (score >= 60) return '#faad14'
    return '#f5222d'
  }

  return (
    <div>
      {/* Entity Selection */}
      <Card style={{ marginBottom: '16px' }}>
        <Space>
          <Text>Monitor Entity:</Text>
          <Select
            style={{ width: 200 }}
            value={`${selectedEntity.type}:${selectedEntity.id}`}
            onChange={(value) => {
              const [type, id] = value.split(':')
              setSelectedEntity({ type: type as PerformanceEntityType, id })
            }}
          >
            <Select.Option value="system:global">System (Global)</Select.Option>
            <Select.Option value="database:main">Database (Main)</Select.Option>
            <Select.Option value="api:v1">API (v1)</Select.Option>
          </Select>
          <Button 
            type="primary"
            icon={<ToolOutlined />}
            loading={autoTuneLoading}
            onClick={handleAutoTune}
          >
            Auto-Tune Performance
          </Button>
        </Space>
      </Card>

      {/* Performance Metrics */}
      <Row gutter={[16, 16]} style={{ marginBottom: '16px' }}>
        <Col xs={24} sm={12} lg={6}>
          <Card>
            <Statistic
              title="Performance Score"
              value={metrics?.metrics.performanceScore || 0}
              suffix="/100"
              valueStyle={{ color: getScoreColor(metrics?.metrics.performanceScore || 0) }}
              prefix={<TrophyOutlined />}
            />
          </Card>
        </Col>
        <Col xs={24} sm={12} lg={6}>
          <Card>
            <Statistic
              title="Avg Response Time"
              value={metrics?.metrics.averageResponseTime || 0}
              suffix="ms"
              precision={2}
            />
          </Card>
        </Col>
        <Col xs={24} sm={12} lg={6}>
          <Card>
            <Statistic
              title="Throughput"
              value={metrics?.metrics.throughputPerSecond || 0}
              suffix="req/s"
              precision={1}
            />
          </Card>
        </Col>
        <Col xs={24} sm={12} lg={6}>
          <Card>
            <Statistic
              title="Error Rate"
              value={(metrics?.metrics.errorRate || 0) * 100}
              suffix="%"
              precision={2}
              valueStyle={{ 
                color: (metrics?.metrics.errorRate || 0) > 0.05 ? '#f5222d' : '#52c41a' 
              }}
            />
          </Card>
        </Col>
      </Row>

      {/* Bottlenecks and Suggestions */}
      <Row gutter={[16, 16]}>
        <Col xs={24} lg={12}>
          <Card title="Performance Bottlenecks" loading={isLoading}>
            {bottlenecks && bottlenecks.bottlenecks.length > 0 ? (
              <div>
                {bottlenecks.bottlenecks.slice(0, 3).map((bottleneck) => (
                  <Alert
                    key={bottleneck.id}
                    message={bottleneck.description}
                    description={`Impact Score: ${bottleneck.impactScore}/10`}
                    type={bottleneck.severity === 'Critical' ? 'error' : 'warning'}
                    showIcon
                    style={{ marginBottom: '8px' }}
                  />
                ))}
              </div>
            ) : (
              <Text type="secondary">No bottlenecks detected</Text>
            )}
          </Card>
        </Col>

        <Col xs={24} lg={12}>
          <Card title="Optimization Suggestions" loading={isLoading}>
            {suggestions && suggestions.suggestions.length > 0 ? (
              <div>
                {suggestions.suggestions.slice(0, 3).map((suggestion) => (
                  <Alert
                    key={suggestion.id}
                    message={suggestion.title}
                    description={`${suggestion.description} - Expected improvement: +${suggestion.estimatedImprovement}%`}
                    type="info"
                    showIcon
                    style={{ marginBottom: '8px' }}
                  />
                ))}
              </div>
            ) : (
              <Text type="secondary">No optimization suggestions available</Text>
            )}
          </Card>
        </Col>
      </Row>
    </div>
  )
}

// Performance Alerts Component
const PerformanceAlertsTab: React.FC = () => {
  const { alerts, criticalAlerts, highAlerts, totalCount } = usePerformanceAlerts()

  return (
    <div>
      <Row gutter={[16, 16]} style={{ marginBottom: '16px' }}>
        <Col xs={24} sm={8}>
          <Card>
            <Statistic
              title="Total Alerts"
              value={totalCount}
              prefix={<WarningOutlined />}
            />
          </Card>
        </Col>
        <Col xs={24} sm={8}>
          <Card>
            <Statistic
              title="Critical Alerts"
              value={criticalAlerts.length}
              valueStyle={{ color: '#f5222d' }}
              prefix={<WarningOutlined />}
            />
          </Card>
        </Col>
        <Col xs={24} sm={8}>
          <Card>
            <Statistic
              title="High Priority"
              value={highAlerts.length}
              valueStyle={{ color: '#faad14' }}
              prefix={<WarningOutlined />}
            />
          </Card>
        </Col>
      </Row>

      <Card title="Recent Performance Alerts">
        <div>
          {alerts.slice(0, 5).map((alert) => (
            <Alert
              key={alert.id}
              message={alert.title}
              description={alert.description}
              type={alert.severity === 'Critical' ? 'error' : alert.severity === 'High' ? 'warning' : 'info'}
              showIcon
              style={{ marginBottom: '8px' }}
            />
          ))}
        </div>
      </Card>
    </div>
  )
}

// Performance Benchmarks Component
const PerformanceBenchmarksTab: React.FC = () => {
  const { benchmarks, overallScore, passingCount, warningCount, failingCount } = usePerformanceBenchmarks()

  return (
    <div>
      <Row gutter={[16, 16]} style={{ marginBottom: '16px' }}>
        <Col xs={24} sm={6}>
          <Card>
            <Statistic
              title="Overall Score"
              value={overallScore}
              suffix="/100"
              precision={1}
              valueStyle={{ color: overallScore >= 80 ? '#52c41a' : overallScore >= 60 ? '#faad14' : '#f5222d' }}
              prefix={<TrophyOutlined />}
            />
          </Card>
        </Col>
        <Col xs={24} sm={6}>
          <Card>
            <Statistic
              title="Passing"
              value={passingCount}
              valueStyle={{ color: '#52c41a' }}
            />
          </Card>
        </Col>
        <Col xs={24} sm={6}>
          <Card>
            <Statistic
              title="Warning"
              value={warningCount}
              valueStyle={{ color: '#faad14' }}
            />
          </Card>
        </Col>
        <Col xs={24} sm={6}>
          <Card>
            <Statistic
              title="Failing"
              value={failingCount}
              valueStyle={{ color: '#f5222d' }}
            />
          </Card>
        </Col>
      </Row>

      <Card title="Performance Benchmarks">
        <Row gutter={[16, 16]}>
          {benchmarks.slice(0, 6).map((benchmark) => (
            <Col key={benchmark.id} xs={24} sm={12} lg={8}>
              <Card size="small">
                <Statistic
                  title={benchmark.name}
                  value={benchmark.currentValue}
                  suffix={`/ ${benchmark.targetValue}`}
                  valueStyle={{ 
                    color: benchmark.status === 'passing' ? '#52c41a' : 
                           benchmark.status === 'warning' ? '#faad14' : '#f5222d' 
                  }}
                />
              </Card>
            </Col>
          ))}
        </Row>
      </Card>
    </div>
  )
}

// Main Performance & Cost Management Component
export default function PerformanceCostManagement() {
  const { criticalAlerts: perfCriticalAlerts, highAlerts: perfHighAlerts } = usePerformanceAlerts()
  const { criticalCount: costCriticalCount, highCount: costHighCount } = useCostAlerts()

  const totalAlerts = perfCriticalAlerts.length + perfHighAlerts.length + costCriticalCount + costHighCount

  const tabItems = [
    // Performance Tabs
    {
      key: 'performance-overview',
      label: (
        <span>
          <ThunderboltOutlined />
          Performance Overview
        </span>
      ),
      children: <PerformanceOverviewTab />
    },
    {
      key: 'performance-alerts',
      label: (
        <span>
          <WarningOutlined />
          Performance Alerts
          {(perfCriticalAlerts.length + perfHighAlerts.length) > 0 && (
            <Badge
              count={perfCriticalAlerts.length + perfHighAlerts.length}
              style={{ marginLeft: '4px' }}
            />
          )}
        </span>
      ),
      children: <PerformanceAlertsTab />
    },
    {
      key: 'performance-benchmarks',
      label: (
        <span>
          <TrophyOutlined />
          Benchmarks
        </span>
      ),
      children: <PerformanceBenchmarksTab />
    },
    // Cost Management Tabs
    {
      key: 'cost-dashboard',
      label: (
        <span>
          <DashboardOutlined />
          Cost Dashboard
        </span>
      ),
      children: <CostDashboard />
    },
    {
      key: 'cost-budgets',
      label: (
        <span>
          <WalletOutlined />
          Budget Management
          {(costCriticalCount + costHighCount) > 0 && (
            <Badge
              count={costCriticalCount + costHighCount}
              style={{ marginLeft: '4px' }}
            />
          )}
        </span>
      ),
      children: <BudgetManagementComponent />
    },
    {
      key: 'cost-analytics',
      label: (
        <span>
          <BarChartOutlined />
          Cost Analytics
        </span>
      ),
      children: <CostAnalyticsTab />
    },
    {
      key: 'cost-forecast',
      label: (
        <span>
          <LineChartOutlined />
          Cost Forecast
        </span>
      ),
      children: <CostForecastTab />
    }
  ]

  return (
    <PageLayout
      title="Performance & Cost Management"
      subtitle="Monitor and optimize system performance and costs"
      extra={
        <Space>
          {totalAlerts > 0 && (
            <Alert
              message={`${totalAlerts} Active Alert${totalAlerts > 1 ? 's' : ''}`}
              type={perfCriticalAlerts.length > 0 || costCriticalCount > 0 ? 'error' : 'warning'}
              showIcon
              style={{ marginBottom: 0 }}
            />
          )}
        </Space>
      }
    >
      <Tabs
        defaultActiveKey="performance-overview"
        items={tabItems}
        size="large"
        style={{ marginTop: '16px' }}
        tabPosition="top"
      />
    </PageLayout>
  )
}
