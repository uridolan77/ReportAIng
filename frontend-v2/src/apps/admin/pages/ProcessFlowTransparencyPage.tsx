import React, { useState } from 'react'
import {
  Card,
  Row,
  Col,
  Tabs,
  Space,
  Typography,
  Button,
  Alert,
  Select,
  DatePicker,
  Statistic,
  Badge
} from 'antd'
import {
  EyeOutlined,
  ApiOutlined,
  BarChartOutlined,
  DatabaseOutlined,
  ReloadOutlined,
  DownloadOutlined,
  SettingOutlined,
  ThunderboltOutlined,
  DollarOutlined,
  ClockCircleOutlined,
  CheckCircleOutlined
} from '@ant-design/icons'
import {
  ProcessFlowDashboard,
  ProcessFlowAnalytics,
  ProcessFlowSessionViewer,
  TokenUsageAnalyzer,
  PerformanceMetricsViewer
} from '@shared/components/ai/transparency'
import { useGetProcessFlowDashboardQuery, useGetProcessFlowAnalyticsQuery } from '@shared/store/api/transparencyApi'
import dayjs from 'dayjs'

const { Title, Text } = Typography
const { RangePicker } = DatePicker

/**
 * ProcessFlowTransparencyPage - Dedicated admin page for ProcessFlow transparency
 * 
 * Features:
 * - Comprehensive ProcessFlow dashboard
 * - Real-time analytics and metrics
 * - Session monitoring and analysis
 * - Token usage and cost tracking
 * - Performance monitoring
 * - Export capabilities
 */
export const ProcessFlowTransparencyPage: React.FC = () => {
  const [activeTab, setActiveTab] = useState('dashboard')
  const [timeRange, setTimeRange] = useState(7)
  const [selectedSessionId, setSelectedSessionId] = useState<string | null>(null)
  const [dateRange, setDateRange] = useState<[dayjs.Dayjs, dayjs.Dayjs] | null>(null)

  // API queries
  const { data: dashboardData, isLoading: dashboardLoading, refetch: refetchDashboard } = 
    useGetProcessFlowDashboardQuery({ 
      days: timeRange, 
      includeDetails: true,
      ...(dateRange && {
        startDate: dateRange[0].format('YYYY-MM-DD'),
        endDate: dateRange[1].format('YYYY-MM-DD')
      })
    })

  const { data: analyticsData, isLoading: analyticsLoading, refetch: refetchAnalytics } = 
    useGetProcessFlowAnalyticsQuery({
      days: timeRange,
      includeStepDetails: true,
      includeTokenUsage: true,
      includePerformanceMetrics: true,
      ...(dateRange && {
        startDate: dateRange[0].format('YYYY-MM-DD'),
        endDate: dateRange[1].format('YYYY-MM-DD')
      })
    })

  const isLoading = dashboardLoading || analyticsLoading

  const handleRefresh = () => {
    refetchDashboard()
    refetchAnalytics()
  }

  const handleTimeRangeChange = (value: number) => {
    setTimeRange(value)
    setDateRange(null) // Clear custom date range
  }

  const handleDateRangeChange = (dates: [dayjs.Dayjs, dayjs.Dayjs] | null) => {
    setDateRange(dates)
    if (dates) {
      setTimeRange(0) // Clear predefined time range
    }
  }

  const renderOverviewTab = () => (
    <div>
      {/* Key Metrics */}
      <Row gutter={[16, 16]} style={{ marginBottom: 24 }}>
        <Col xs={24} sm={12} lg={6}>
          <Card>
            <Statistic
              title="Total Sessions"
              value={dashboardData?.totalSessions || 0}
              prefix={<ApiOutlined />}
              valueStyle={{ color: '#1890ff' }}
            />
          </Card>
        </Col>
        <Col xs={24} sm={12} lg={6}>
          <Card>
            <Statistic
              title="Success Rate"
              value={dashboardData?.successRate?.toFixed(1) || '0.0'}
              suffix="%"
              prefix={<CheckCircleOutlined />}
              valueStyle={{ 
                color: (dashboardData?.successRate || 0) > 90 ? '#52c41a' : 
                       (dashboardData?.successRate || 0) > 70 ? '#faad14' : '#ff4d4f' 
              }}
            />
          </Card>
        </Col>
        <Col xs={24} sm={12} lg={6}>
          <Card>
            <Statistic
              title="Avg Duration"
              value={dashboardData?.averageDuration || 0}
              suffix="ms"
              prefix={<ClockCircleOutlined />}
              valueStyle={{ color: '#722ed1' }}
            />
          </Card>
        </Col>
        <Col xs={24} sm={12} lg={6}>
          <Card>
            <Statistic
              title="Total Cost"
              value={analyticsData?.totalCost || 0}
              prefix={<DollarOutlined />}
              precision={4}
              valueStyle={{ color: '#cf1322' }}
            />
          </Card>
        </Col>
      </Row>

      {/* ProcessFlow Dashboard */}
      <ProcessFlowDashboard 
        filters={{ 
          days: timeRange || undefined,
          ...(dateRange && {
            startDate: dateRange[0].format('YYYY-MM-DD'),
            endDate: dateRange[1].format('YYYY-MM-DD')
          }),
          includeDetails: true 
        }}
        showCharts={true}
        showMetrics={true}
      />
    </div>
  )

  const renderAnalyticsTab = () => (
    <ProcessFlowAnalytics 
      defaultFilters={{
        days: timeRange || undefined,
        ...(dateRange && {
          startDate: dateRange[0].format('YYYY-MM-DD'),
          endDate: dateRange[1].format('YYYY-MM-DD')
        }),
        includeStepDetails: true,
        includeTokenUsage: true,
        includePerformanceMetrics: true
      }}
      showExport={true}
    />
  )

  const renderTokenUsageTab = () => (
    <TokenUsageAnalyzer 
      timeRange={timeRange}
      showCostBreakdown={true}
      showModelComparison={true}
      compact={false}
    />
  )

  const renderPerformanceTab = () => (
    <PerformanceMetricsViewer 
      timeRange={timeRange}
      showTrends={true}
      showAlerts={true}
      compact={false}
    />
  )

  const renderSessionViewerTab = () => (
    <div>
      <Alert
        message="Session Viewer"
        description="Select a session ID to view detailed ProcessFlow information, steps, logs, and transparency data."
        type="info"
        showIcon
        style={{ marginBottom: 16 }}
      />
      
      {selectedSessionId ? (
        <ProcessFlowSessionViewer
          sessionId={selectedSessionId}
          showDetailedSteps={true}
          showLogs={true}
          showTransparency={true}
          compact={false}
        />
      ) : (
        <Card>
          <div style={{ textAlign: 'center', padding: '60px 0' }}>
            <DatabaseOutlined style={{ fontSize: '48px', color: '#d9d9d9', marginBottom: '16px' }} />
            <Title level={4}>No Session Selected</Title>
            <Text type="secondary">
              Enter a session ID to view detailed ProcessFlow information
            </Text>
          </div>
        </Card>
      )}
    </div>
  )

  const tabItems = [
    {
      key: 'dashboard',
      label: (
        <Space>
          <EyeOutlined />
          <span>Dashboard</span>
        </Space>
      ),
      children: renderOverviewTab()
    },
    {
      key: 'analytics',
      label: (
        <Space>
          <BarChartOutlined />
          <span>Analytics</span>
        </Space>
      ),
      children: renderAnalyticsTab()
    },
    {
      key: 'token-usage',
      label: (
        <Space>
          <DollarOutlined />
          <span>Token Usage</span>
        </Space>
      ),
      children: renderTokenUsageTab()
    },
    {
      key: 'performance',
      label: (
        <Space>
          <ThunderboltOutlined />
          <span>Performance</span>
        </Space>
      ),
      children: renderPerformanceTab()
    },
    {
      key: 'sessions',
      label: (
        <Space>
          <DatabaseOutlined />
          <span>Session Viewer</span>
        </Space>
      ),
      children: renderSessionViewerTab()
    }
  ]

  return (
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
              <EyeOutlined />
              ProcessFlow Transparency
            </Space>
          </Title>
          <Text type="secondary">
            Comprehensive ProcessFlow monitoring, analytics, and transparency dashboard
          </Text>
        </div>
        
        <Space>
          <Select
            value={timeRange}
            onChange={handleTimeRangeChange}
            style={{ width: 120 }}
          >
            <Select.Option value={1}>Last Day</Select.Option>
            <Select.Option value={7}>Last Week</Select.Option>
            <Select.Option value={30}>Last Month</Select.Option>
            <Select.Option value={90}>Last 3 Months</Select.Option>
          </Select>
          
          <RangePicker
            value={dateRange}
            onChange={handleDateRangeChange}
            format="YYYY-MM-DD"
          />
          
          <Button 
            icon={<ReloadOutlined />} 
            loading={isLoading}
            onClick={handleRefresh}
          >
            Refresh
          </Button>
          
          <Button icon={<DownloadOutlined />}>
            Export
          </Button>
        </Space>
      </div>

      {/* Status Indicators */}
      <Row gutter={[16, 16]} style={{ marginBottom: 16 }}>
        <Col>
          <Badge 
            status={isLoading ? 'processing' : 'success'} 
            text={isLoading ? 'Loading...' : 'Data Updated'} 
          />
        </Col>
        <Col>
          <Badge 
            status="processing" 
            text={`${timeRange ? `Last ${timeRange} days` : 'Custom Range'}`} 
          />
        </Col>
      </Row>

      {/* Main Content */}
      <Tabs
        activeKey={activeTab}
        onChange={setActiveTab}
        items={tabItems}
        size="large"
      />
    </div>
  )
}

export default ProcessFlowTransparencyPage
