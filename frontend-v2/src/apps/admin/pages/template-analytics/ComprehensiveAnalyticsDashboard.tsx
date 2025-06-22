import React, { useState, useEffect } from 'react'
import { 
  Card, 
  Row, 
  Col, 
  DatePicker, 
  Select, 
  Spin, 
  Typography,
  Space,
  Button,
  Alert,
  Tabs,
  Badge,
  Tooltip,
  message
} from 'antd'
import {
  DashboardOutlined,
  RiseOutlined,
  BulbOutlined,
  StarOutlined,
  ThunderboltOutlined,
  ExportOutlined,
  ReloadOutlined,
  SettingOutlined
} from '@ant-design/icons'
import dayjs from 'dayjs'
import {
  useGetComprehensiveDashboardQuery,
  useGetPerformanceTrendsQuery,
  useGetUsageInsightsQuery,
  useGetQualityMetricsQuery,
  useGetRealTimeAnalyticsQuery,
  useExportAnalyticsMutation
} from '@shared/store/api/templateAnalyticsApi'
import { 
  PerformanceLineChart, 
  PerformanceBarChart, 
  PerformancePieChart,
  MetricCard 
} from '@shared/components/charts/PerformanceChart'
import type {
  ComprehensiveAnalyticsDashboard as DashboardData,
  PerformanceTrendsData,
  UsageInsightsData,
  QualityMetricsData,
  RealTimeAnalyticsData
} from '@shared/types/templateAnalytics'

const { Title, Text } = Typography
const { RangePicker } = DatePicker
const { Option } = Select

export const ComprehensiveAnalyticsDashboard: React.FC = () => {
  // State management
  const [dateRange, setDateRange] = useState<[dayjs.Dayjs, dayjs.Dayjs]>([
    dayjs().subtract(30, 'day'),
    dayjs()
  ])
  const [intentType, setIntentType] = useState<string | undefined>()
  const [activeTab, setActiveTab] = useState('overview')
  const [autoRefresh, setAutoRefresh] = useState(true)

  // API queries
  const { 
    data: dashboardData, 
    isLoading: isDashboardLoading, 
    error: dashboardError,
    refetch: refetchDashboard
  } = useGetComprehensiveDashboardQuery({
    startDate: dateRange[0].toISOString(),
    endDate: dateRange[1].toISOString(),
    intentType
  })

  const { 
    data: trendsData, 
    isLoading: isTrendsLoading 
  } = useGetPerformanceTrendsQuery({
    startDate: dateRange[0].toISOString(),
    endDate: dateRange[1].toISOString(),
    intentType,
    granularity: 'daily'
  })

  const { 
    data: insightsData, 
    isLoading: isInsightsLoading 
  } = useGetUsageInsightsQuery({
    startDate: dateRange[0].toISOString(),
    endDate: dateRange[1].toISOString(),
    intentType
  })

  const { 
    data: qualityData, 
    isLoading: isQualityLoading 
  } = useGetQualityMetricsQuery({
    intentType
  })

  const { 
    data: realTimeData, 
    isLoading: isRealTimeLoading,
    refetch: refetchRealTime
  } = useGetRealTimeAnalyticsQuery()

  const [exportAnalytics, { isLoading: isExporting }] = useExportAnalyticsMutation()

  // Auto-refresh real-time data
  useEffect(() => {
    if (autoRefresh) {
      const interval = setInterval(() => {
        refetchRealTime()
      }, 30000) // Refresh every 30 seconds

      return () => clearInterval(interval)
    }
  }, [autoRefresh, refetchRealTime])

  // Event handlers
  const handleDateRangeChange = (dates: [dayjs.Dayjs, dayjs.Dayjs] | null) => {
    if (dates) {
      setDateRange(dates)
    }
  }

  const handleExport = async () => {
    try {
      const blob = await exportAnalytics({
        format: 'Excel',
        dateRange: {
          startDate: dateRange[0].toISOString(),
          endDate: dateRange[1].toISOString()
        },
        includedMetrics: ['performance', 'usage', 'quality', 'trends'],
        intentTypeFilter: intentType,
        includeCharts: true,
        includeRawData: true,
        exportedBy: 'current-user',
        requestedDate: new Date().toISOString()
      }).unwrap()

      // Create download link
      const url = window.URL.createObjectURL(blob)
      const link = document.createElement('a')
      link.href = url
      link.download = `template-analytics-${dayjs().format('YYYY-MM-DD')}.xlsx`
      document.body.appendChild(link)
      link.click()
      document.body.removeChild(link)
      window.URL.revokeObjectURL(url)

      message.success('Analytics data exported successfully')
    } catch (error) {
      message.error('Failed to export analytics data')
    }
  }

  const handleRefresh = () => {
    refetchDashboard()
    refetchRealTime()
    message.success('Dashboard refreshed')
  }

  // Loading state
  if (isDashboardLoading) {
    return (
      <div style={{ textAlign: 'center', padding: '100px 0' }}>
        <Spin size="large" />
        <div style={{ marginTop: '16px' }}>
          <Text>Loading comprehensive analytics...</Text>
        </div>
      </div>
    )
  }

  // Error state
  if (dashboardError) {
    return (
      <Alert
        message="Error Loading Dashboard"
        description="Failed to load analytics data. Please try again."
        type="error"
        showIcon
        action={
          <Button size="small" onClick={handleRefresh}>
            Retry
          </Button>
        }
      />
    )
  }

  return (
    <div className="comprehensive-analytics-dashboard">
      {/* Header */}
      <div style={{ marginBottom: '24px' }}>
        <Row gutter={16} align="middle">
          <Col span={12}>
            <Title level={2} style={{ margin: 0 }}>
              Comprehensive Analytics Dashboard
            </Title>
            <Text type="secondary">
              Complete template performance analytics with real-time insights
            </Text>
          </Col>
          <Col span={12} style={{ textAlign: 'right' }}>
            <Space>
              <Button 
                icon={<ReloadOutlined />} 
                onClick={handleRefresh}
                loading={isDashboardLoading}
              >
                Refresh
              </Button>
              <Button 
                icon={<ExportOutlined />} 
                onClick={handleExport}
                loading={isExporting}
              >
                Export
              </Button>
              <Button icon={<SettingOutlined />}>
                Settings
              </Button>
            </Space>
          </Col>
        </Row>
      </div>

      {/* Filters */}
      <Card style={{ marginBottom: '24px' }}>
        <Row gutter={16} align="middle">
          <Col span={8}>
            <Space>
              <Text strong>Date Range:</Text>
              <RangePicker
                value={dateRange}
                onChange={handleDateRangeChange}
                format="YYYY-MM-DD"
                allowClear={false}
              />
            </Space>
          </Col>
          <Col span={8}>
            <Space>
              <Text strong>Intent Type:</Text>
              <Select
                placeholder="All Intent Types"
                value={intentType}
                onChange={setIntentType}
                style={{ width: 200 }}
                allowClear
              >
                <Option value="sql_generation">SQL Generation</Option>
                <Option value="insight_generation">Insight Generation</Option>
                <Option value="explanation">Explanation</Option>
                <Option value="data_analysis">Data Analysis</Option>
              </Select>
            </Space>
          </Col>
          <Col span={8} style={{ textAlign: 'right' }}>
            <Space>
              <Text strong>Auto Refresh:</Text>
              <Button
                type={autoRefresh ? 'primary' : 'default'}
                size="small"
                onClick={() => setAutoRefresh(!autoRefresh)}
              >
                {autoRefresh ? 'ON' : 'OFF'}
              </Button>
              {realTimeData && (
                <Badge status="processing" text={`Last updated: ${dayjs(realTimeData.lastUpdated).format('HH:mm:ss')}`} />
              )}
            </Space>
          </Col>
        </Row>
      </Card>

      {/* Overview Metrics */}
      {dashboardData && (
        <Row gutter={16} style={{ marginBottom: '24px' }}>
          <Col span={6}>
            <MetricCard
              title="Total Templates"
              value={dashboardData.performanceOverview.totalTemplates}
              icon={<DashboardOutlined />}
              color="#1890ff"
            />
          </Col>
          <Col span={6}>
            <MetricCard
              title="Success Rate"
              value={`${(dashboardData.performanceOverview.overallSuccessRate * 100).toFixed(1)}%`}
              icon={<RiseOutlined />}
              color="#52c41a"
            />
          </Col>
          <Col span={6}>
            <MetricCard
              title="Active Tests"
              value={dashboardData.abTestingOverview.totalActiveTests}
              icon={<BulbOutlined />}
              color="#722ed1"
            />
          </Col>
          <Col span={6}>
            <MetricCard
              title="Active Alerts"
              value={dashboardData.activeAlerts.length}
              icon={<StarOutlined />}
              color="#fa8c16"
            />
          </Col>
        </Row>
      )}

      {/* Real-Time Metrics */}
      {realTimeData && (
        <Card 
          title={
            <Space>
              <ThunderboltOutlined />
              Real-Time Metrics
              <Badge status="processing" text="Live" />
            </Space>
          }
          style={{ marginBottom: '24px' }}
          loading={isRealTimeLoading}
        >
          <Row gutter={16}>
            <Col span={4}>
              <Tooltip title="Currently active users">
                <div style={{ textAlign: 'center' }}>
                  <div style={{ fontSize: '24px', fontWeight: 600, color: '#1890ff' }}>
                    {realTimeData.activeUsers}
                  </div>
                  <Text type="secondary">Active Users</Text>
                </div>
              </Tooltip>
            </Col>
            <Col span={4}>
              <Tooltip title="Queries processed per minute">
                <div style={{ textAlign: 'center' }}>
                  <div style={{ fontSize: '24px', fontWeight: 600, color: '#52c41a' }}>
                    {realTimeData.queriesPerMinute}
                  </div>
                  <Text type="secondary">Queries/Min</Text>
                </div>
              </Tooltip>
            </Col>
            <Col span={4}>
              <Tooltip title="Current success rate">
                <div style={{ textAlign: 'center' }}>
                  <div style={{ fontSize: '24px', fontWeight: 600, color: '#722ed1' }}>
                    {(realTimeData.currentSuccessRate * 100).toFixed(1)}%
                  </div>
                  <Text type="secondary">Success Rate</Text>
                </div>
              </Tooltip>
            </Col>
            <Col span={4}>
              <Tooltip title="Average response time">
                <div style={{ textAlign: 'center' }}>
                  <div style={{ fontSize: '24px', fontWeight: 600, color: '#fa8c16' }}>
                    {realTimeData.averageResponseTime.toFixed(2)}s
                  </div>
                  <Text type="secondary">Response Time</Text>
                </div>
              </Tooltip>
            </Col>
            <Col span={4}>
              <Tooltip title="Errors in the last hour">
                <div style={{ textAlign: 'center' }}>
                  <div style={{ 
                    fontSize: '24px', 
                    fontWeight: 600, 
                    color: realTimeData.errorsInLastHour > 10 ? '#ff4d4f' : '#52c41a' 
                  }}>
                    {realTimeData.errorsInLastHour}
                  </div>
                  <Text type="secondary">Errors/Hour</Text>
                </div>
              </Tooltip>
            </Col>
            <Col span={4}>
              <Tooltip title="Recent activities">
                <div style={{ textAlign: 'center' }}>
                  <div style={{ fontSize: '24px', fontWeight: 600, color: '#13c2c2' }}>
                    {realTimeData.recentActivities.length}
                  </div>
                  <Text type="secondary">Activities</Text>
                </div>
              </Tooltip>
            </Col>
          </Row>
        </Card>
      )}

      {/* Detailed Analytics Tabs */}
      <Tabs
        activeKey={activeTab}
        onChange={setActiveTab}
        items={[
          {
            key: 'trends',
            label: (
              <Space>
                <RiseOutlined />
                Performance Trends
              </Space>
            ),
            children: (
              <Card loading={isTrendsLoading}>
                {trendsData && (
                  <div>
                    <div style={{ marginBottom: '16px' }}>
                      <Text type="secondary">
                        Performance trends over the selected time period with {trendsData.granularity} granularity
                      </Text>
                    </div>
                    <PerformanceLineChart
                      data={trendsData.dataPoints.map(point => ({
                        timestamp: dayjs(point.timestamp).format('MM/DD'),
                        successRate: point.averageSuccessRate * 100,
                        confidenceScore: point.averageConfidenceScore * 100,
                        usage: point.totalUsage,
                        responseTime: point.averageResponseTime,
                        errors: point.errorCount
                      }))}
                      xAxisKey="timestamp"
                      lines={[
                        { key: 'successRate', color: '#52c41a', name: 'Success Rate (%)' },
                        { key: 'confidenceScore', color: '#1890ff', name: 'Confidence Score (%)' },
                        { key: 'responseTime', color: '#fa8c16', name: 'Response Time (s)' }
                      ]}
                      height={400}
                    />
                  </div>
                )}
              </Card>
            )
          },
          {
            key: 'insights',
            label: (
              <Space>
                <BulbOutlined />
                Usage Insights
                {insightsData && <Badge count={insightsData.insights.length} />}
              </Space>
            ),
            children: (
              <div>
                <Row gutter={16} style={{ marginBottom: '24px' }}>
                  <Col span={8}>
                    <Card title="Usage Overview" loading={isInsightsLoading}>
                      {insightsData && (
                        <div>
                          <div style={{ marginBottom: '16px' }}>
                            <Text strong>Total Usage:</Text>
                            <div style={{ fontSize: '24px', fontWeight: 600, color: '#1890ff' }}>
                              {insightsData.totalUsage.toLocaleString()}
                            </div>
                          </div>
                          <div style={{ marginBottom: '16px' }}>
                            <Text strong>Average Success Rate:</Text>
                            <div style={{ fontSize: '24px', fontWeight: 600, color: '#52c41a' }}>
                              {(insightsData.averageSuccessRate * 100).toFixed(1)}%
                            </div>
                          </div>
                        </div>
                      )}
                    </Card>
                  </Col>
                  <Col span={16}>
                    <Card title="Usage by Intent Type" loading={isInsightsLoading}>
                      {insightsData && (
                        <PerformancePieChart
                          data={Object.entries(insightsData.usageByIntentType).map(([type, count]) => ({
                            name: type.replace('_', ' ').toUpperCase(),
                            value: count
                          }))}
                          height={300}
                        />
                      )}
                    </Card>
                  </Col>
                </Row>

                {insightsData && (
                  <Row gutter={16}>
                    <Col span={12}>
                      <Card title="Top Performing Templates" size="small">
                        {insightsData.topPerformingTemplates.map((template, index) => (
                          <div key={template.templateKey} style={{
                            padding: '8px 0',
                            borderBottom: index < insightsData.topPerformingTemplates.length - 1 ? '1px solid #f0f0f0' : 'none'
                          }}>
                            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                              <div>
                                <Text strong style={{ fontSize: '12px' }}>{template.templateName}</Text>
                                <div>
                                  <Text type="secondary" style={{ fontSize: '11px' }}>
                                    {template.totalUsages} uses
                                  </Text>
                                </div>
                              </div>
                              <div style={{ textAlign: 'right' }}>
                                <div style={{ fontSize: '14px', fontWeight: 600, color: '#52c41a' }}>
                                  {(template.successRate * 100).toFixed(1)}%
                                </div>
                              </div>
                            </div>
                          </div>
                        ))}
                      </Card>
                    </Col>
                    <Col span={12}>
                      <Card title="Underperforming Templates" size="small">
                        {insightsData.underperformingTemplates.map((template, index) => (
                          <div key={template.templateKey} style={{
                            padding: '8px 0',
                            borderBottom: index < insightsData.underperformingTemplates.length - 1 ? '1px solid #f0f0f0' : 'none'
                          }}>
                            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                              <div>
                                <Text strong style={{ fontSize: '12px' }}>{template.templateName}</Text>
                                <div>
                                  <Text type="secondary" style={{ fontSize: '11px' }}>
                                    {template.totalUsages} uses
                                  </Text>
                                </div>
                              </div>
                              <div style={{ textAlign: 'right' }}>
                                <div style={{ fontSize: '14px', fontWeight: 600, color: '#ff4d4f' }}>
                                  {(template.successRate * 100).toFixed(1)}%
                                </div>
                              </div>
                            </div>
                          </div>
                        ))}
                      </Card>
                    </Col>
                  </Row>
                )}
              </div>
            )
          },
          {
            key: 'quality',
            label: (
              <Space>
                <StarOutlined />
                Quality Metrics
              </Space>
            ),
            children: (
              <Card loading={isQualityLoading}>
                {qualityData && (
                  <Row gutter={16}>
                    <Col span={8}>
                      <Card title="Quality Overview" size="small">
                        <div style={{ textAlign: 'center', marginBottom: '16px' }}>
                          <div style={{ fontSize: '32px', fontWeight: 600, color: '#52c41a' }}>
                            {qualityData.overallQualityScore.toFixed(1)}
                          </div>
                          <Text type="secondary">Overall Quality Score</Text>
                        </div>
                        <div style={{ marginBottom: '12px' }}>
                          <Text strong>Templates Analyzed:</Text>
                          <div style={{ fontSize: '18px', fontWeight: 600 }}>
                            {qualityData.totalTemplatesAnalyzed}
                          </div>
                        </div>
                        <div style={{ marginBottom: '12px' }}>
                          <Text strong>Above Threshold:</Text>
                          <div style={{ fontSize: '18px', fontWeight: 600, color: '#52c41a' }}>
                            {qualityData.templatesAboveThreshold}
                          </div>
                        </div>
                        <div>
                          <Text strong>Below Threshold:</Text>
                          <div style={{ fontSize: '18px', fontWeight: 600, color: '#ff4d4f' }}>
                            {qualityData.templatesBelowThreshold}
                          </div>
                        </div>
                      </Card>
                    </Col>
                    <Col span={16}>
                      <Card title="Quality Distribution" size="small">
                        <PerformancePieChart
                          data={Object.entries(qualityData.qualityDistribution).map(([category, count]) => ({
                            name: category,
                            value: count
                          }))}
                          height={300}
                        />
                      </Card>
                    </Col>
                  </Row>
                )}
              </Card>
            )
          }
        ]}
      />
    </div>
  )
}

export default ComprehensiveAnalyticsDashboard
