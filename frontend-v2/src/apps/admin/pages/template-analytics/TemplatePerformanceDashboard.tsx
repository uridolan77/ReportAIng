import React, { useState, useEffect } from 'react'
import { 
  Card, 
  Row, 
  Col, 
  Select, 
  DatePicker, 
  Button, 
  Spin, 
  Alert, 
  Table,
  Tag,
  Space,
  Typography,
  Statistic
} from 'antd'
import { 
  ReloadOutlined, 
  DownloadOutlined,
  TrophyOutlined,
  AlertOutlined,
  RiseOutlined,
  UsergroupAddOutlined
} from '@ant-design/icons'
import dayjs from 'dayjs'
import { 
  useGetPerformanceDashboardQuery,
  useGetPerformanceAlertsQuery,
  useResolvePerformanceAlertMutation
} from '@shared/store/api/templateAnalyticsApi'
import { 
  PerformanceLineChart, 
  PerformanceBarChart, 
  PerformancePieChart,
  MetricCard 
} from '@shared/components/charts/PerformanceChart'


const { RangePicker } = DatePicker
const { Title } = Typography

export const TemplatePerformanceDashboard: React.FC = () => {
  const [timeRange, setTimeRange] = useState<[dayjs.Dayjs, dayjs.Dayjs]>([
    dayjs().subtract(7, 'day'),
    dayjs()
  ])
  const [selectedIntentType, setSelectedIntentType] = useState<string>('all')
  const [autoRefresh, setAutoRefresh] = useState(true)

  const {
    data: dashboardData,
    isLoading,
    error,
    refetch
  } = useGetPerformanceDashboardQuery({
    startDate: timeRange[0].toISOString(),
    endDate: timeRange[1].toISOString(),
    intentType: selectedIntentType === 'all' ? undefined : selectedIntentType
  })

  const { data: alerts } = useGetPerformanceAlertsQuery({
    resolved: false
  })

  const [resolveAlert] = useResolvePerformanceAlertMutation()

  // Auto-refresh logic
  useEffect(() => {
    if (!autoRefresh) return

    const interval = setInterval(() => {
      refetch()
    }, 30000) // 30 seconds

    return () => clearInterval(interval)
  }, [autoRefresh, refetch])

  const handleResolveAlert = async (alertId: number) => {
    try {
      await resolveAlert(alertId).unwrap()
    } catch (error) {
      console.error('Failed to resolve alert:', error)
    }
  }

  const handleExport = () => {
    // TODO: Implement export functionality
    console.log('Export functionality to be implemented')
  }

  if (isLoading) {
    return (
      <div style={{ display: 'flex', justifyContent: 'center', alignItems: 'center', height: '400px' }}>
        <Spin size="large" />
      </div>
    )
  }

  if (error) {
    return (
      <Alert
        message="Error Loading Performance Data"
        description="Unable to load template performance metrics. Please try again."
        type="error"
        showIcon
        action={
          <Button size="small" onClick={() => refetch()}>
            Retry
          </Button>
        }
      />
    )
  }

  const alertColumns = [
    {
      title: 'Template',
      dataIndex: 'templateKey',
      key: 'templateKey',
    },
    {
      title: 'Alert Type',
      dataIndex: 'alertType',
      key: 'alertType',
      render: (type: string) => (
        <Tag color={
          type === 'critical' ? 'red' : 
          type === 'high' ? 'orange' : 
          type === 'medium' ? 'yellow' : 'blue'
        }>
          {type.replace('_', ' ').toUpperCase()}
        </Tag>
      )
    },
    {
      title: 'Message',
      dataIndex: 'message',
      key: 'message',
    },
    {
      title: 'Severity',
      dataIndex: 'severity',
      key: 'severity',
      render: (severity: string) => (
        <Tag color={
          severity === 'critical' ? 'red' : 
          severity === 'high' ? 'orange' : 
          severity === 'medium' ? 'yellow' : 'blue'
        }>
          {severity.toUpperCase()}
        </Tag>
      )
    },
    {
      title: 'Actions',
      key: 'actions',
      render: (record: any) => (
        <Button 
          size="small" 
          onClick={() => handleResolveAlert(record.id)}
        >
          Resolve
        </Button>
      )
    }
  ]

  return (
    <div>
        {/* Header Controls */}
        <div style={{ marginBottom: '24px' }}>
          <Row gutter={16} align="middle">
            <Col span={12}>
              <Title level={2} style={{ margin: 0 }}>
                Template Performance Dashboard
              </Title>
            </Col>
            <Col span={12} style={{ textAlign: 'right' }}>
              <Space>
                <Select
                  value={selectedIntentType}
                  onChange={setSelectedIntentType}
                  style={{ width: 200 }}
                  placeholder="Select Intent Type"
                >
                  <Select.Option value="all">All Intent Types</Select.Option>
                  <Select.Option value="sql_generation">SQL Generation</Select.Option>
                  <Select.Option value="insight_generation">Insight Generation</Select.Option>
                  <Select.Option value="explanation">Explanation</Select.Option>
                </Select>
                <RangePicker
                  value={timeRange}
                  onChange={(dates) => dates && setTimeRange(dates)}
                />
                <Button 
                  icon={<ReloadOutlined />} 
                  onClick={() => refetch()}
                >
                  Refresh
                </Button>
                <Button 
                  icon={<DownloadOutlined />} 
                  onClick={handleExport}
                >
                  Export
                </Button>
              </Space>
            </Col>
          </Row>
        </div>

        {/* Alerts Section */}
        {alerts && alerts.length > 0 && (
          <div style={{ marginBottom: '24px' }}>
            <Alert
              message={`${alerts.length} Performance Alert${alerts.length > 1 ? 's' : ''}`}
              description={
                <Table
                  dataSource={alerts.slice(0, 5)}
                  columns={alertColumns}
                  pagination={false}
                  size="small"
                  rowKey="id"
                />
              }
              type="warning"
              showIcon
              closable
            />
          </div>
        )}

        {/* Key Metrics Grid */}
        <Row gutter={16} style={{ marginBottom: '24px' }}>
          <Col span={6}>
            <MetricCard
              title="Total Templates"
              value={dashboardData?.totalTemplates || 0}
              icon={<TrophyOutlined />}
              color="#1890ff"
            />
          </Col>
          <Col span={6}>
            <MetricCard
              title="Overall Success Rate"
              value={`${((dashboardData?.overallSuccessRate || 0) * 100).toFixed(1)}%`}
              icon={<RiseOutlined />}
              color="#52c41a"
            />
          </Col>
          <Col span={6}>
            <MetricCard
              title="Usage Today"
              value={dashboardData?.totalUsagesToday || 0}
              icon={<UsergroupAddOutlined />}
              color="#722ed1"
            />
          </Col>
          <Col span={6}>
            <MetricCard
              title="Needs Attention"
              value={dashboardData?.needsAttention?.length || 0}
              icon={<AlertOutlined />}
              color="#fa8c16"
            />
          </Col>
        </Row>

        {/* Charts Section */}
        <Row gutter={16} style={{ marginBottom: '24px' }}>
          <Col span={12}>
            <Card title="Top Performing Templates" style={{ height: '400px' }}>
              <PerformanceBarChart
                data={dashboardData?.topPerformers?.map(template => ({
                  name: template.templateName,
                  successRate: template.successRate * 100,
                  usageCount: template.totalUsages
                })) || []}
                xAxisKey="name"
                yAxisKey="successRate"
                height={300}
                color="#52c41a"
              />
            </Card>
          </Col>
          <Col span={12}>
            <Card title="Usage by Intent Type" style={{ height: '400px' }}>
              <PerformancePieChart
                data={Object.entries(dashboardData?.usageByIntentType || {}).map(([type, count]) => ({
                  name: type,
                  value: count
                }))}
                height={300}
              />
            </Card>
          </Col>
        </Row>

        {/* Performance Trends */}
        <Row gutter={16}>
          <Col span={24}>
            <Card title="Performance Trends" style={{ height: '400px' }}>
              <PerformanceLineChart
                data={dashboardData?.recentTrends?.map(trend => ({
                  date: dayjs(trend.timestamp).format('MM/DD'),
                  successRate: trend.successRate * 100,
                  usageCount: trend.usageCount,
                  avgConfidence: trend.averageConfidenceScore * 100
                })) || []}
                xAxisKey="date"
                lines={[
                  { key: 'successRate', color: '#52c41a', name: 'Success Rate (%)' },
                  { key: 'avgConfidence', color: '#1890ff', name: 'Avg Confidence (%)' }
                ]}
                height={300}
              />
            </Card>
          </Col>
        </Row>
      </div>
  )
}

export default TemplatePerformanceDashboard
