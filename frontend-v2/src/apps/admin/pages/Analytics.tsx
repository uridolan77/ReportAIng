import React, { useState } from 'react'
import {
  Card,
  Row,
  Col,
  Statistic,
  Typography,
  Select,
  DatePicker,
  Space,
  Table,
  Tag,
  Progress,
  Alert,
  Tabs,
  List,
  Avatar,
  Button,
  Tooltip
} from 'antd'
import {
  LineChartOutlined,
  BarChartOutlined,
  UserOutlined,
  DatabaseOutlined,
  ClockCircleOutlined,
  TrophyOutlined,
  WarningOutlined,
  CheckCircleOutlined,
  ArrowUpOutlined,
  ArrowDownOutlined,
  ReloadOutlined,
  DownloadOutlined
} from '@ant-design/icons'
import { PageLayout } from '@shared/components/core/Layout'
import { Chart } from '@shared/components/core/Chart'
import { useGetAnalyticsQuery, useGetSystemMetricsQuery } from '@shared/store/api/adminApi'

const { Title, Text } = Typography
const { RangePicker } = DatePicker
const { Option } = Select
const { TabPane } = Tabs

export default function Analytics() {
  const [timeRange, setTimeRange] = useState('7d')
  const [selectedMetric, setSelectedMetric] = useState('queries')

  const { data: analytics, isLoading: analyticsLoading, refetch: refetchAnalytics } = useGetAnalyticsQuery({
    timeRange,
    metric: selectedMetric
  })

  const { data: systemMetrics, isLoading: metricsLoading } = useGetSystemMetricsQuery()

  // Mock data for demonstration
  const mockAnalytics = {
    overview: {
      totalQueries: 15420,
      activeUsers: 89,
      avgResponseTime: 245,
      successRate: 98.5,
      totalCost: 1247.50,
      dataProcessed: 2.4 // TB
    },
    trends: {
      queries: [
        { date: '2024-01-08', value: 1200 },
        { date: '2024-01-09', value: 1350 },
        { date: '2024-01-10', value: 1180 },
        { date: '2024-01-11', value: 1420 },
        { date: '2024-01-12', value: 1380 },
        { date: '2024-01-13', value: 1650 },
        { date: '2024-01-14', value: 1580 },
        { date: '2024-01-15', value: 1720 }
      ],
      users: [
        { date: '2024-01-08', value: 78 },
        { date: '2024-01-09', value: 82 },
        { date: '2024-01-10', value: 75 },
        { date: '2024-01-11', value: 89 },
        { date: '2024-01-12', value: 85 },
        { date: '2024-01-13', value: 92 },
        { date: '2024-01-14', value: 88 },
        { date: '2024-01-15', value: 89 }
      ],
      performance: [
        { date: '2024-01-08', value: 280 },
        { date: '2024-01-09', value: 265 },
        { date: '2024-01-10', value: 290 },
        { date: '2024-01-11', value: 245 },
        { date: '2024-01-12', value: 255 },
        { date: '2024-01-13', value: 235 },
        { date: '2024-01-14', value: 250 },
        { date: '2024-01-15', value: 245 }
      ]
    },
    topQueries: [
      { query: 'SELECT * FROM sales WHERE date >= ?', count: 1250, avgTime: 180 },
      { query: 'SELECT customer_id, SUM(amount) FROM orders GROUP BY customer_id', count: 980, avgTime: 320 },
      { query: 'SELECT * FROM products WHERE category = ?', count: 750, avgTime: 95 },
      { query: 'SELECT COUNT(*) FROM users WHERE active = 1', count: 650, avgTime: 45 },
      { query: 'SELECT * FROM inventory WHERE stock < 10', count: 520, avgTime: 150 }
    ],
    topUsers: [
      { name: 'John Doe', email: 'john.doe@company.com', queries: 450, avgTime: 210 },
      { name: 'Jane Smith', email: 'jane.smith@company.com', queries: 380, avgTime: 195 },
      { name: 'Mike Wilson', email: 'mike.wilson@company.com', queries: 320, avgTime: 240 },
      { name: 'Sarah Johnson', email: 'sarah.johnson@company.com', queries: 290, avgTime: 180 },
      { name: 'David Brown', email: 'david.brown@company.com', queries: 275, avgTime: 225 }
    ],
    alerts: [
      { type: 'warning', message: 'High query volume detected in the last hour', time: '5 minutes ago' },
      { type: 'info', message: 'Database maintenance scheduled for tonight', time: '2 hours ago' },
      { type: 'success', message: 'Performance optimization completed', time: '1 day ago' }
    ]
  }

  const displayData = analytics || mockAnalytics

  const getStatisticColor = (value: number, threshold: number, reverse = false) => {
    if (reverse) {
      return value <= threshold ? '#52c41a' : '#ff4d4f'
    }
    return value >= threshold ? '#52c41a' : '#ff4d4f'
  }

  const formatNumber = (num: number) => {
    if (num >= 1000000) return `${(num / 1000000).toFixed(1)}M`
    if (num >= 1000) return `${(num / 1000).toFixed(1)}K`
    return num.toString()
  }

  const queryColumns = [
    {
      title: 'Query',
      dataIndex: 'query',
      key: 'query',
      ellipsis: true,
      render: (text: string) => (
        <Tooltip title={text}>
          <Text code style={{ fontSize: '12px' }}>
            {text.length > 60 ? `${text.substring(0, 60)}...` : text}
          </Text>
        </Tooltip>
      ),
    },
    {
      title: 'Count',
      dataIndex: 'count',
      key: 'count',
      width: 100,
      render: (count: number) => formatNumber(count),
    },
    {
      title: 'Avg Time',
      dataIndex: 'avgTime',
      key: 'avgTime',
      width: 100,
      render: (time: number) => `${time}ms`,
    },
  ]

  const userColumns = [
    {
      title: 'User',
      key: 'user',
      render: (record: any) => (
        <Space>
          <Avatar icon={<UserOutlined />} />
          <div>
            <div>{record.name}</div>
            <Text type="secondary" style={{ fontSize: '12px' }}>
              {record.email}
            </Text>
          </div>
        </Space>
      ),
    },
    {
      title: 'Queries',
      dataIndex: 'queries',
      key: 'queries',
      width: 100,
    },
    {
      title: 'Avg Time',
      dataIndex: 'avgTime',
      key: 'avgTime',
      width: 100,
      render: (time: number) => `${time}ms`,
    },
  ]

  return (
    <PageLayout
      title="Analytics & Monitoring"
      subtitle="View system analytics and performance metrics"
    >
      {/* Controls */}
      <Card style={{ marginBottom: 24 }}>
        <Space wrap>
          <Select
            value={timeRange}
            onChange={setTimeRange}
            style={{ width: 120 }}
          >
            <Option value="1d">Last 24h</Option>
            <Option value="7d">Last 7 days</Option>
            <Option value="30d">Last 30 days</Option>
            <Option value="90d">Last 90 days</Option>
          </Select>

          <Select
            value={selectedMetric}
            onChange={setSelectedMetric}
            style={{ width: 150 }}
          >
            <Option value="queries">Query Volume</Option>
            <Option value="users">Active Users</Option>
            <Option value="performance">Performance</Option>
            <Option value="costs">Costs</Option>
          </Select>

          <Button
            icon={<ReloadOutlined />}
            onClick={() => refetchAnalytics()}
            loading={analyticsLoading}
          >
            Refresh
          </Button>

          <Button icon={<DownloadOutlined />}>
            Export Report
          </Button>
        </Space>
      </Card>

      {/* Overview Statistics */}
      <Row gutter={[16, 16]} style={{ marginBottom: 24 }}>
        <Col xs={24} sm={12} lg={6}>
          <Card>
            <Statistic
              title="Total Queries"
              value={displayData.overview.totalQueries}
              prefix={<DatabaseOutlined />}
              valueStyle={{ color: '#1890ff' }}
              suffix={
                <Tag color="green" style={{ marginLeft: 8 }}>
                  <ArrowUpOutlined /> 12%
                </Tag>
              }
            />
          </Card>
        </Col>
        <Col xs={24} sm={12} lg={6}>
          <Card>
            <Statistic
              title="Active Users"
              value={displayData.overview.activeUsers}
              prefix={<UserOutlined />}
              valueStyle={{ color: '#52c41a' }}
              suffix={
                <Tag color="green" style={{ marginLeft: 8 }}>
                  <ArrowUpOutlined /> 8%
                </Tag>
              }
            />
          </Card>
        </Col>
        <Col xs={24} sm={12} lg={6}>
          <Card>
            <Statistic
              title="Avg Response Time"
              value={displayData.overview.avgResponseTime}
              suffix="ms"
              prefix={<ClockCircleOutlined />}
              valueStyle={{
                color: getStatisticColor(displayData.overview.avgResponseTime, 300, true)
              }}
            />
            <Progress
              percent={Math.max(0, 100 - (displayData.overview.avgResponseTime / 5))}
              size="small"
              showInfo={false}
              strokeColor={getStatisticColor(displayData.overview.avgResponseTime, 300, true)}
            />
          </Card>
        </Col>
        <Col xs={24} sm={12} lg={6}>
          <Card>
            <Statistic
              title="Success Rate"
              value={displayData.overview.successRate}
              suffix="%"
              prefix={<CheckCircleOutlined />}
              valueStyle={{
                color: getStatisticColor(displayData.overview.successRate, 95)
              }}
            />
            <Progress
              percent={displayData.overview.successRate}
              size="small"
              showInfo={false}
              strokeColor={getStatisticColor(displayData.overview.successRate, 95)}
            />
          </Card>
        </Col>
      </Row>

      {/* Charts and Tables */}
      <Row gutter={[16, 16]}>
        <Col xs={24} lg={16}>
          <Card
            title={
              <Space>
                <LineChartOutlined />
                <span>Trends</span>
              </Space>
            }
            loading={analyticsLoading}
          >
            <Chart
              data={displayData.trends[selectedMetric as keyof typeof displayData.trends] || displayData.trends.queries}
              columns={['date', 'value']}
              config={{
                type: 'line',
                title: `${selectedMetric.charAt(0).toUpperCase() + selectedMetric.slice(1)} Trend`,
                xAxis: 'date',
                yAxis: 'value',
                colorScheme: 'default',
                showGrid: true,
                showLegend: false,
                showAnimation: true,
                interactive: true
              }}
              height={300}
            />
          </Card>
        </Col>

        <Col xs={24} lg={8}>
          <Card
            title="System Alerts"
            extra={<Tag color="orange">{displayData.alerts.length}</Tag>}
          >
            <List
              size="small"
              dataSource={displayData.alerts}
              renderItem={(alert) => (
                <List.Item>
                  <Space>
                    {alert.type === 'warning' && <WarningOutlined style={{ color: '#faad14' }} />}
                    {alert.type === 'info' && <DatabaseOutlined style={{ color: '#1890ff' }} />}
                    {alert.type === 'success' && <CheckCircleOutlined style={{ color: '#52c41a' }} />}
                    <div>
                      <div style={{ fontSize: '12px' }}>{alert.message}</div>
                      <Text type="secondary" style={{ fontSize: '11px' }}>
                        {alert.time}
                      </Text>
                    </div>
                  </Space>
                </List.Item>
              )}
            />
          </Card>
        </Col>
      </Row>

      {/* Detailed Analytics */}
      <Row gutter={[16, 16]} style={{ marginTop: 24 }}>
        <Col xs={24} lg={12}>
          <Card
            title={
              <Space>
                <TrophyOutlined />
                <span>Top Queries</span>
              </Space>
            }
          >
            <Table
              columns={queryColumns}
              dataSource={displayData.topQueries}
              pagination={false}
              size="small"
            />
          </Card>
        </Col>

        <Col xs={24} lg={12}>
          <Card
            title={
              <Space>
                <UserOutlined />
                <span>Top Users</span>
              </Space>
            }
          >
            <Table
              columns={userColumns}
              dataSource={displayData.topUsers}
              pagination={false}
              size="small"
            />
          </Card>
        </Col>
      </Row>
    </PageLayout>
  )
}
