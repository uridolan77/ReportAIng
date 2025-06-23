import React from 'react'
import { 
  Card, 
  Row, 
  Col, 
  Statistic, 
  Space, 
  Typography, 
  Alert,
  Spin,
  Button
} from 'antd'
import {
  ApiOutlined,
  ClockCircleOutlined,
  CheckCircleOutlined,
  ExclamationCircleOutlined,
  ThunderboltOutlined,
  DollarOutlined,
  ReloadOutlined
} from '@ant-design/icons'
import { LineChart, Line, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer, BarChart, Bar } from 'recharts'
import { useProcessFlowDashboard } from '@shared/store/api/transparencyApi'
import type { ProcessFlowMetricsFilters } from '@shared/types/transparency'

const { Title, Text } = Typography

export interface ProcessFlowDashboardProps {
  filters?: ProcessFlowMetricsFilters
  showCharts?: boolean
  showMetrics?: boolean
  refreshInterval?: number
  className?: string
  testId?: string
}

/**
 * ProcessFlowDashboard - Main dashboard for ProcessFlow analytics
 * 
 * Features:
 * - Real-time ProcessFlow metrics
 * - Performance trends
 * - Cost analysis
 * - Success rate tracking
 * - Token usage analytics
 */
export const ProcessFlowDashboard: React.FC<ProcessFlowDashboardProps> = ({
  filters = { days: 7, includeDetails: true },
  showCharts = true,
  showMetrics = true,
  refreshInterval,
  className,
  testId = 'processflow-dashboard'
}) => {
  const { 
    dashboard, 
    analytics, 
    isLoading, 
    error, 
    refetch 
  } = useProcessFlowDashboard(filters)

  if (isLoading) {
    return (
      <div style={{ textAlign: 'center', padding: '40px' }}>
        <Spin size="large" />
        <div style={{ marginTop: 16 }}>
          <Text type="secondary">Loading ProcessFlow dashboard...</Text>
        </div>
      </div>
    )
  }

  if (error) {
    return (
      <Alert
        message="Error Loading Dashboard"
        description="Failed to load ProcessFlow dashboard data"
        type="error"
        showIcon
        action={
          <Button size="small" icon={<ReloadOutlined />} onClick={refetch}>
            Retry
          </Button>
        }
      />
    )
  }

  if (!dashboard || !analytics) {
    return (
      <Alert
        message="No Data Available"
        description="No ProcessFlow data available for the selected time period"
        type="info"
        showIcon
      />
    )
  }

  return (
    <div className={className} data-testid={testId}>
      <Space direction="vertical" style={{ width: '100%' }} size="large">
        {/* Header */}
        <Card>
          <Space>
            <ApiOutlined style={{ fontSize: '24px', color: '#1890ff' }} />
            <Title level={3} style={{ margin: 0 }}>ProcessFlow Dashboard</Title>
            <Button icon={<ReloadOutlined />} onClick={refetch}>
              Refresh
            </Button>
          </Space>
        </Card>

        {/* Key Metrics */}
        {showMetrics && (
          <Row gutter={[16, 16]}>
            <Col xs={24} sm={12} lg={6}>
              <Card>
                <Statistic
                  title="Total Sessions"
                  value={dashboard.totalSessions || 0}
                  prefix={<ApiOutlined />}
                />
              </Card>
            </Col>
            <Col xs={24} sm={12} lg={6}>
              <Card>
                <Statistic
                  title="Success Rate"
                  value={dashboard.successRate || 0}
                  precision={1}
                  suffix="%"
                  prefix={<CheckCircleOutlined />}
                  valueStyle={{ 
                    color: (dashboard.successRate || 0) > 90 ? '#3f8600' : 
                           (dashboard.successRate || 0) > 70 ? '#faad14' : '#cf1322' 
                  }}
                />
              </Card>
            </Col>
            <Col xs={24} sm={12} lg={6}>
              <Card>
                <Statistic
                  title="Avg Confidence"
                  value={(dashboard.averageConfidence || 0) * 100}
                  precision={1}
                  suffix="%"
                  prefix={<ThunderboltOutlined />}
                  valueStyle={{ 
                    color: (dashboard.averageConfidence || 0) > 0.8 ? '#3f8600' : 
                           (dashboard.averageConfidence || 0) > 0.6 ? '#faad14' : '#cf1322' 
                  }}
                />
              </Card>
            </Col>
            <Col xs={24} sm={12} lg={6}>
              <Card>
                <Statistic
                  title="Avg Duration"
                  value={dashboard.averageDuration || 0}
                  suffix="ms"
                  prefix={<ClockCircleOutlined />}
                />
              </Card>
            </Col>
          </Row>
        )}

        {/* Additional Metrics */}
        {showMetrics && (
          <Row gutter={[16, 16]}>
            <Col xs={24} sm={12} lg={6}>
              <Card>
                <Statistic
                  title="Total Tokens"
                  value={analytics.totalTokens || 0}
                  formatter={(value) => value.toLocaleString()}
                  prefix={<ApiOutlined />}
                />
              </Card>
            </Col>
            <Col xs={24} sm={12} lg={6}>
              <Card>
                <Statistic
                  title="Total Cost"
                  value={analytics.totalCost || 0}
                  precision={4}
                  prefix={<DollarOutlined />}
                />
              </Card>
            </Col>
            <Col xs={24} sm={12} lg={6}>
              <Card>
                <Statistic
                  title="API Calls"
                  value={analytics.totalApiCalls || 0}
                  prefix={<ApiOutlined />}
                />
              </Card>
            </Col>
            <Col xs={24} sm={12} lg={6}>
              <Card>
                <Statistic
                  title="Failed Sessions"
                  value={dashboard.failedSessions || 0}
                  prefix={<ExclamationCircleOutlined />}
                  valueStyle={{ color: '#cf1322' }}
                />
              </Card>
            </Col>
          </Row>
        )}

        {/* Charts */}
        {showCharts && (
          <Row gutter={[16, 16]}>
            <Col xs={24} lg={12}>
              <Card title="Session Trends">
                <ResponsiveContainer width="100%" height={200}>
                  <LineChart data={analytics.dailyMetrics || []}>
                    <CartesianGrid strokeDasharray="3 3" />
                    <XAxis dataKey="date" />
                    <YAxis />
                    <Tooltip />
                    <Line 
                      type="monotone" 
                      dataKey="sessionCount" 
                      stroke="#1890ff" 
                      strokeWidth={2}
                    />
                  </LineChart>
                </ResponsiveContainer>
              </Card>
            </Col>
            <Col xs={24} lg={12}>
              <Card title="Success Rate Trends">
                <ResponsiveContainer width="100%" height={200}>
                  <LineChart data={analytics.dailyMetrics || []}>
                    <CartesianGrid strokeDasharray="3 3" />
                    <XAxis dataKey="date" />
                    <YAxis />
                    <Tooltip />
                    <Line 
                      type="monotone" 
                      dataKey="successRate" 
                      stroke="#52c41a" 
                      strokeWidth={2}
                    />
                  </LineChart>
                </ResponsiveContainer>
              </Card>
            </Col>
          </Row>
        )}

        {/* Performance Alerts */}
        {dashboard.averageConfidence && dashboard.averageConfidence < 0.7 && (
          <Alert
            message="Low Confidence Detected"
            description={`Average confidence is ${(dashboard.averageConfidence * 100).toFixed(1)}%. Consider reviewing ProcessFlow configuration.`}
            type="warning"
            showIcon
          />
        )}

        {dashboard.successRate && dashboard.successRate < 80 && (
          <Alert
            message="Low Success Rate"
            description={`Success rate is ${dashboard.successRate.toFixed(1)}%. Review failed sessions for optimization opportunities.`}
            type="error"
            showIcon
          />
        )}
      </Space>
    </div>
  )
}

export default ProcessFlowDashboard
