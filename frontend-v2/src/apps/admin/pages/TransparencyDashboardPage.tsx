import React, { useState, useEffect } from 'react'
import { 
  Row, 
  Col, 
  Card, 
  Tabs, 
  Space, 
  Typography, 
  Button, 
  Select,
  DatePicker,
  Statistic,
  Alert,
  Spin,
  message
} from 'antd'
import {
  EyeOutlined,
  BarChartOutlined,
  LineChartOutlined,
  DownloadOutlined,
  ReloadOutlined,
  SettingOutlined,
  ThunderboltOutlined,
  TrophyOutlined
} from '@ant-design/icons'
import { 
  ConfidenceTrendsChart,
  TokenUsageChart,
  AlternativeOptionsPanel,
  LiveTransparencyPanel,
  TransparencyExportPanel,
  RealTimeMonitor,
  TransparencyMetricsChart,
  OptimizationInsights
} from '@shared/components/ai/transparency'
import { 
  useGetTransparencyDashboardMetricsQuery,
  useGetTransparencyTracesQuery,
  useGetTransparencySettingsQuery
} from '@shared/store/api/transparencyApi'
import { transparencyHub } from '@shared/services/signalr/transparencyHub'
import dayjs from 'dayjs'

const { Title, Text } = Typography
const { TabPane } = Tabs
const { RangePicker } = DatePicker

/**
 * TransparencyDashboardPage - Comprehensive transparency analytics dashboard
 * 
 * Features:
 * - Real-time transparency monitoring
 * - Confidence trends analysis
 * - Token usage analytics
 * - Alternative options tracking
 * - Export capabilities
 * - Live query processing
 * - Performance insights
 */
export const TransparencyDashboardPage: React.FC = () => {
  const [activeTab, setActiveTab] = useState('overview')
  const [timeRange, setTimeRange] = useState<'day' | 'week' | 'month'>('week')
  const [dateRange, setDateRange] = useState<[dayjs.Dayjs, dayjs.Dayjs]>([
    dayjs().subtract(7, 'days'),
    dayjs()
  ])
  const [selectedTraceId, setSelectedTraceId] = useState<string>('')
  const [isHubConnected, setIsHubConnected] = useState(false)

  // API queries
  const { 
    data: dashboardMetrics, 
    isLoading: metricsLoading, 
    refetch: refetchMetrics 
  } = useGetTransparencyDashboardMetricsQuery({
    days: timeRange === 'day' ? 1 : timeRange === 'week' ? 7 : 30
  })

  const { 
    data: tracesData, 
    isLoading: tracesLoading 
  } = useGetTransparencyTracesQuery({
    page: 1,
    pageSize: 10,
    dateFrom: dateRange[0].toISOString(),
    dateTo: dateRange[1].toISOString(),
    sortBy: 'createdAt',
    sortOrder: 'desc'
  })

  const { 
    data: settings 
  } = useGetTransparencySettingsQuery()

  // Initialize SignalR connection
  useEffect(() => {
    const initializeHub = async () => {
      try {
        if (!transparencyHub.isConnected) {
          await transparencyHub.connect()
        }
        setIsHubConnected(true)
        message.success('Connected to real-time transparency updates')
      } catch (error) {
        console.error('Failed to connect to transparency hub:', error)
        message.error('Failed to connect to real-time updates')
      }
    }

    initializeHub()

    // Listen for connection state changes
    transparencyHub.on('connectionStateChanged', (state) => {
      setIsHubConnected(state.state === 'connected')
    })

    return () => {
      transparencyHub.off('connectionStateChanged')
    }
  }, [])

  // Handle time range change
  const handleTimeRangeChange = (range: 'day' | 'week' | 'month') => {
    setTimeRange(range)
    const days = range === 'day' ? 1 : range === 'week' ? 7 : 30
    setDateRange([dayjs().subtract(days, 'days'), dayjs()])
  }

  // Handle date range change
  const handleDateRangeChange = (dates: [dayjs.Dayjs, dayjs.Dayjs] | null) => {
    if (dates) {
      setDateRange(dates)
    }
  }

  // Handle refresh
  const handleRefresh = () => {
    refetchMetrics()
    message.success('Dashboard refreshed')
  }

  // Prepare chart data
  const confidenceTrendsData = dashboardMetrics?.confidenceTrends?.map(trend => ({
    date: trend.date,
    confidence: trend.confidence,
    traceCount: trend.traceCount || 0
  })) || []

  const tokenUsageData = dashboardMetrics?.usageByModel?.map((usage, index) => ({
    date: dayjs().subtract(index, 'days').format('YYYY-MM-DD'),
    totalTokens: usage.totalTokens || 0,
    inputTokens: Math.round((usage.totalTokens || 0) * 0.7), // Estimated split
    outputTokens: Math.round((usage.totalTokens || 0) * 0.3),
    model: usage.model
  })) || []

  return (
    <div style={{ padding: '24px' }}>
      {/* Header */}
      <div style={{ marginBottom: 24 }}>
        <Row justify="space-between" align="middle">
          <Col>
            <Space direction="vertical" size="small">
              <Title level={2} style={{ margin: 0 }}>
                <EyeOutlined /> AI Transparency Dashboard
              </Title>
              <Text type="secondary">
                Real-time insights into AI decision-making and performance
              </Text>
            </Space>
          </Col>
          <Col>
            <Space>
              <RangePicker
                value={dateRange}
                onChange={handleDateRangeChange}
                presets={[
                  { label: 'Last 7 days', value: [dayjs().subtract(7, 'days'), dayjs()] },
                  { label: 'Last 30 days', value: [dayjs().subtract(30, 'days'), dayjs()] },
                  { label: 'Last 3 months', value: [dayjs().subtract(3, 'months'), dayjs()] },
                ]}
              />
              <Select
                value={timeRange}
                onChange={handleTimeRangeChange}
                style={{ width: 100 }}
              >
                <Select.Option value="day">Day</Select.Option>
                <Select.Option value="week">Week</Select.Option>
                <Select.Option value="month">Month</Select.Option>
              </Select>
              <Button icon={<ReloadOutlined />} onClick={handleRefresh}>
                Refresh
              </Button>
            </Space>
          </Col>
        </Row>
      </div>

      {/* Connection Status */}
      {!isHubConnected && (
        <Alert
          message="Real-time Updates Unavailable"
          description="Some features may not work properly without real-time connection."
          type="warning"
          showIcon
          style={{ marginBottom: 16 }}
        />
      )}

      {/* Key Metrics */}
      {dashboardMetrics && (
        <Row gutter={16} style={{ marginBottom: 24 }}>
          <Col span={6}>
            <Card>
              <Statistic
                title="Total Traces"
                value={dashboardMetrics.totalTraces}
                prefix={<EyeOutlined />}
                valueStyle={{ color: '#1890ff' }}
              />
            </Card>
          </Col>
          <Col span={6}>
            <Card>
              <Statistic
                title="Average Confidence"
                value={Math.round(dashboardMetrics.averageConfidence * 100)}
                suffix="%"
                prefix={<TrophyOutlined />}
                valueStyle={{ 
                  color: dashboardMetrics.averageConfidence > 0.8 ? '#52c41a' : 
                         dashboardMetrics.averageConfidence > 0.6 ? '#faad14' : '#ff4d4f' 
                }}
              />
            </Card>
          </Col>
          <Col span={6}>
            <Card>
              <Statistic
                title="Top Optimizations"
                value={dashboardMetrics.topOptimizations?.length || 0}
                prefix={<ThunderboltOutlined />}
                valueStyle={{ color: '#722ed1' }}
              />
            </Card>
          </Col>
          <Col span={6}>
            <Card>
              <Statistic
                title="Models Used"
                value={dashboardMetrics.usageByModel?.length || 0}
                prefix={<SettingOutlined />}
                valueStyle={{ color: '#fa8c16' }}
              />
            </Card>
          </Col>
        </Row>
      )}

      {/* Main Content Tabs */}
      <Tabs activeKey={activeTab} onChange={setActiveTab} size="large">
        <TabPane 
          tab={
            <Space>
              <BarChartOutlined />
              <span>Overview</span>
            </Space>
          } 
          key="overview"
        >
          <Row gutter={[16, 16]}>
            <Col span={12}>
              <ConfidenceTrendsChart
                data={confidenceTrendsData}
                timeRange={timeRange}
                onTimeRangeChange={handleTimeRangeChange}
                loading={metricsLoading}
              />
            </Col>
            <Col span={12}>
              <TokenUsageChart
                data={tokenUsageData}
                timeRange={timeRange}
                onTimeRangeChange={handleTimeRangeChange}
                loading={metricsLoading}
              />
            </Col>
            <Col span={24}>
              <TransparencyMetricsChart />
            </Col>
          </Row>
        </TabPane>

        <TabPane 
          tab={
            <Space>
              <LineChartOutlined />
              <span>Real-time Monitoring</span>
            </Space>
          } 
          key="monitoring"
        >
          <Row gutter={[16, 16]}>
            <Col span={12}>
              <RealTimeMonitor />
            </Col>
            <Col span={12}>
              {selectedTraceId ? (
                <LiveTransparencyPanel
                  traceId={selectedTraceId}
                  onTraceComplete={(result) => {
                    message.success(`Trace ${result.traceId} completed with ${Math.round(result.finalConfidence * 100)}% confidence`)
                  }}
                />
              ) : (
                <Card>
                  <Alert
                    message="Select a Trace"
                    description="Choose a trace from the monitoring panel to view live processing details."
                    type="info"
                    showIcon
                  />
                </Card>
              )}
            </Col>
            <Col span={24}>
              <OptimizationInsights />
            </Col>
          </Row>
        </TabPane>

        <TabPane 
          tab={
            <Space>
              <ThunderboltOutlined />
              <span>Alternatives & Optimization</span>
            </Space>
          } 
          key="optimization"
        >
          <Row gutter={[16, 16]}>
            <Col span={12}>
              {tracesData?.traces?.[0] && (
                <AlternativeOptionsPanel
                  traceId={tracesData.traces[0].traceId}
                  onSelectAlternative={(option) => {
                    message.success(`Applied alternative: ${option.description}`)
                  }}
                />
              )}
            </Col>
            <Col span={12}>
              <Card title="Recent Traces" loading={tracesLoading}>
                {tracesData?.traces?.map(trace => (
                  <Card.Grid 
                    key={trace.traceId}
                    style={{ width: '100%', cursor: 'pointer' }}
                    onClick={() => setSelectedTraceId(trace.traceId)}
                  >
                    <Space direction="vertical" size="small" style={{ width: '100%' }}>
                      <Text strong>{trace.userQuestion}</Text>
                      <Space>
                        <Text type="secondary">Confidence: {Math.round(trace.overallConfidence * 100)}%</Text>
                        <Text type="secondary">Tokens: {trace.totalTokens}</Text>
                      </Space>
                    </Space>
                  </Card.Grid>
                ))}
              </Card>
            </Col>
          </Row>
        </TabPane>

        <TabPane 
          tab={
            <Space>
              <DownloadOutlined />
              <span>Export</span>
            </Space>
          } 
          key="export"
        >
          <Row justify="center">
            <Col span={12}>
              <TransparencyExportPanel
                onExportComplete={(filename) => {
                  message.success(`Export completed: ${filename}`)
                }}
              />
            </Col>
          </Row>
        </TabPane>
      </Tabs>
    </div>
  )
}

export default TransparencyDashboardPage
