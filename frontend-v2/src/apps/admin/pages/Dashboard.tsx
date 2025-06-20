import { FC } from 'react'
import { Card, Row, Col, Statistic, Typography, Space, Button, Alert, Tag } from 'antd'
import {
  UserOutlined,
  DatabaseOutlined,
  BarChartOutlined,
  SettingOutlined,
  DollarOutlined,
  ThunderboltOutlined,
  WarningOutlined
} from '@ant-design/icons'
import { PageLayout } from '@shared/components/core/Layout'
import { Chart } from '@shared/components/core'
import { useGetSystemStatisticsQuery, useGetQueryAnalyticsQuery } from '@shared/store/api/adminApi'
import { useCostMetrics, useCostAlerts } from '@shared/hooks/useCostMetrics'
import { usePerformanceAlerts } from '@shared/hooks/usePerformanceMonitoring'

const { Title } = Typography

export default function Dashboard() {
  const { data: systemStats, isLoading: statsLoading } = useGetSystemStatisticsQuery({ days: 30 })
  const { data: queryAnalytics, isLoading: analyticsLoading } = useGetQueryAnalyticsQuery({ period: 'week' })

  // Cost and Performance Metrics
  const { analytics: costAnalytics, realTime: costRealTime } = useCostMetrics('7d')
  const { alerts: costAlerts, criticalCount: costCriticalCount } = useCostAlerts()
  const { criticalAlerts: perfCriticalAlerts, highAlerts: perfHighAlerts } = usePerformanceAlerts()

  // Mock data for charts
  const mockChartData = [
    { name: 'Mon', queries: 24 },
    { name: 'Tue', queries: 13 },
    { name: 'Wed', queries: 98 },
    { name: 'Thu', queries: 39 },
    { name: 'Fri', queries: 48 },
    { name: 'Sat', queries: 38 },
    { name: 'Sun', queries: 43 },
  ]

  return (
    <PageLayout
      title="Admin Dashboard"
      subtitle="System overview and management"
      extra={
        <Space>
          {(costCriticalCount > 0 || perfCriticalAlerts.length > 0) && (
            <Alert
              message={`${costCriticalCount + perfCriticalAlerts.length} Critical Alert${costCriticalCount + perfCriticalAlerts.length > 1 ? 's' : ''}`}
              type="error"
              showIcon
              style={{ marginBottom: 0 }}
            />
          )}
          <Button icon={<SettingOutlined />}>System Settings</Button>
        </Space>
      }
    >
      {/* Statistics Cards */}
      <Row gutter={[16, 16]} style={{ marginBottom: 24 }}>
        <Col xs={24} sm={12} lg={6}>
          <Card>
            <Statistic
              title="Total Users"
              value={systemStats?.totalUsers || 0}
              prefix={<UserOutlined />}
              loading={statsLoading}
            />
          </Card>
        </Col>
        <Col xs={24} sm={12} lg={6}>
          <Card>
            <Statistic
              title="Active Users"
              value={systemStats?.activeUsers || 0}
              prefix={<UserOutlined />}
              loading={statsLoading}
            />
          </Card>
        </Col>
        <Col xs={24} sm={12} lg={6}>
          <Card>
            <Statistic
              title="Total Queries"
              value={systemStats?.totalQueries || 0}
              prefix={<DatabaseOutlined />}
              loading={statsLoading}
            />
          </Card>
        </Col>
        <Col xs={24} sm={12} lg={6}>
          <Card>
            <Statistic
              title="Avg Query Time"
              value={systemStats?.averageQueryTime || 0}
              suffix="ms"
              prefix={<BarChartOutlined />}
              loading={statsLoading}
            />
          </Card>
        </Col>
      </Row>

      {/* Cost and Performance Metrics */}
      <Row gutter={[16, 16]} style={{ marginBottom: 24 }}>
        <Col xs={24} sm={12} lg={6}>
          <Card>
            <Statistic
              title="Weekly Cost"
              value={costAnalytics?.weeklyCost || 0}
              prefix={<DollarOutlined />}
              precision={2}
              formatter={(value) => `$${value}`}
            />
          </Card>
        </Col>
        <Col xs={24} sm={12} lg={6}>
          <Card>
            <Statistic
              title="Cost per Query"
              value={costRealTime?.costPerQuery || 0}
              prefix={<DollarOutlined />}
              precision={4}
              formatter={(value) => `$${value}`}
            />
          </Card>
        </Col>
        <Col xs={24} sm={12} lg={6}>
          <Card>
            <Statistic
              title="Cost Efficiency"
              value={(costAnalytics?.costEfficiency || 0) * 100}
              suffix="%"
              prefix={<ThunderboltOutlined />}
              precision={1}
              valueStyle={{
                color: (costAnalytics?.costEfficiency || 0) > 0.8 ? '#52c41a' :
                       (costAnalytics?.costEfficiency || 0) > 0.6 ? '#faad14' : '#f5222d'
              }}
            />
          </Card>
        </Col>
        <Col xs={24} sm={12} lg={6}>
          <Card>
            <div style={{ textAlign: 'center' }}>
              <div style={{ fontSize: '24px', fontWeight: 'bold', marginBottom: '8px' }}>
                <Space>
                  <WarningOutlined style={{ color: '#f5222d' }} />
                  {costCriticalCount + perfCriticalAlerts.length}
                </Space>
              </div>
              <div style={{ color: '#666', fontSize: '14px' }}>Critical Alerts</div>
              <div style={{ marginTop: '8px' }}>
                <Space size="small">
                  <Tag color="red">Cost: {costCriticalCount}</Tag>
                  <Tag color="orange">Perf: {perfCriticalAlerts.length}</Tag>
                </Space>
              </div>
            </div>
          </Card>
        </Col>
      </Row>

      {/* Charts */}
      <Row gutter={[16, 16]}>
        <Col xs={24} lg={12}>
          <Chart
            data={mockChartData}
            columns={['name', 'queries']}
            config={{
              type: 'bar',
              title: 'Weekly Query Volume',
              xAxis: 'name',
              yAxis: 'queries',
            }}
            height={300}
          />
        </Col>
        <Col xs={24} lg={12}>
          <Chart
            data={mockChartData}
            columns={['name', 'queries']}
            config={{
              type: 'line',
              title: 'Query Trend',
              xAxis: 'name',
              yAxis: 'queries',
            }}
            height={300}
          />
        </Col>
      </Row>

      {/* System Status */}
      <Row gutter={[16, 16]} style={{ marginTop: 24 }}>
        <Col span={24}>
          <Card title="System Status">
            <Row gutter={[16, 16]}>
              <Col xs={24} sm={8}>
                <Statistic
                  title="System Uptime"
                  value={systemStats?.systemUptime || 0}
                  suffix="hours"
                  loading={statsLoading}
                />
              </Col>
              <Col xs={24} sm={8}>
                <Statistic
                  title="Memory Usage"
                  value={systemStats?.memoryUsage || 0}
                  suffix="%"
                  loading={statsLoading}
                />
              </Col>
              <Col xs={24} sm={8}>
                <Statistic
                  title="CPU Usage"
                  value={systemStats?.cpuUsage || 0}
                  suffix="%"
                  loading={statsLoading}
                />
              </Col>
            </Row>
          </Card>
        </Col>
      </Row>
    </PageLayout>
  )
}
