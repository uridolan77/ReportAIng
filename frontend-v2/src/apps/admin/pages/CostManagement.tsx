import { FC } from 'react'
import { Tabs, Card, Row, Col, Button, Space, Typography, Alert } from 'antd'
import {
  DashboardOutlined,
  WalletOutlined,
  LineChartOutlined,
  BulbOutlined,
  BarChartOutlined
} from '@ant-design/icons'
import { PageLayout } from '@shared/components/core/Layout'
import { CostDashboard } from '@shared/components/cost/CostDashboard'
import { BudgetManagementComponent } from '@shared/components/cost/BudgetManagement'
import { useCostMetrics, useCostAlerts } from '@shared/hooks/useCostMetrics'

const { Title, Text } = Typography

const CostAnalyticsTab: React.FC = () => {
  const { analytics, _forecast, recommendations } = useCostMetrics('30d')
  
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

export default function CostManagement() {
  const { _alerts, criticalCount, highCount } = useCostAlerts()

  const tabItems = [
    {
      key: 'dashboard',
      label: (
        <span>
          <DashboardOutlined />
          Dashboard
        </span>
      ),
      children: <CostDashboard />
    },
    {
      key: 'budgets',
      label: (
        <span>
          <WalletOutlined />
          Budget Management
        </span>
      ),
      children: <BudgetManagementComponent />
    },
    {
      key: 'analytics',
      label: (
        <span>
          <BarChartOutlined />
          Analytics
        </span>
      ),
      children: <CostAnalyticsTab />
    },
    {
      key: 'forecast',
      label: (
        <span>
          <LineChartOutlined />
          Forecast
        </span>
      ),
      children: <CostForecastTab />
    }
  ]

  return (
    <PageLayout
      title="Cost Management"
      subtitle="Monitor and optimize AI service costs"
      extra={
        <Space>
          {(criticalCount > 0 || highCount > 0) && (
            <Alert
              message={`${criticalCount + highCount} Active Alert${criticalCount + highCount > 1 ? 's' : ''}`}
              type={criticalCount > 0 ? 'error' : 'warning'}
              showIcon
              style={{ marginBottom: 0 }}
            />
          )}
        </Space>
      }
    >
      <Tabs
        defaultActiveKey="dashboard"
        items={tabItems}
        size="large"
        style={{ marginTop: '16px' }}
      />
    </PageLayout>
  )
}
