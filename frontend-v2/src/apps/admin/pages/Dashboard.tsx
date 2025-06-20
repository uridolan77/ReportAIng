import React from 'react'
import { Card, Row, Col, Statistic, Typography, Space, Button } from 'antd'
import { UserOutlined, DatabaseOutlined, BarChartOutlined, SettingOutlined } from '@ant-design/icons'
import { PageLayout } from '@shared/components/core/Layout'
import { Chart } from '@shared/components/core'
import { useGetSystemStatisticsQuery, useGetQueryAnalyticsQuery } from '@shared/store/api/adminApi'

const { Title } = Typography

export default function Dashboard() {
  const { data: systemStats, isLoading: statsLoading } = useGetSystemStatisticsQuery({ days: 30 })
  const { data: queryAnalytics, isLoading: analyticsLoading } = useGetQueryAnalyticsQuery({ period: 'week' })

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
