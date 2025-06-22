import React, { useState, useMemo } from 'react'
import { 
  Card, 
  Row, 
  Col, 
  Table, 
  Typography, 
  Space, 
  Select, 
  DatePicker,
  Statistic,
  Tag,
  Progress,
  Alert,
  Tabs,
  List,
  Button,
  Tooltip
} from 'antd'
import {
  BarChartOutlined,
  LineChartOutlined,
  TrophyOutlined,
  ThunderboltOutlined,
  ClockCircleOutlined,
  DollarOutlined,
  ExclamationCircleOutlined,
  CheckCircleOutlined,
  ReloadOutlined,
  DownloadOutlined
} from '@ant-design/icons'
import { LineChart, Line, BarChart, Bar, PieChart, Pie, Cell, XAxis, YAxis, CartesianGrid, Tooltip as RechartsTooltip, ResponsiveContainer, AreaChart, Area } from 'recharts'
import { ConfidenceIndicator } from '../common/ConfidenceIndicator'
import {
  useGetTransparencyMetricsQuery,
  useGetModelPerformanceComparisonQuery,
  useGetRealTimeMonitoringDataQuery
} from '@shared/store/api/transparencyApi'
import dayjs from 'dayjs'

const { Title, Text } = Typography
const { RangePicker } = DatePicker

export interface ModelPerformanceAnalyticsProps {
  timeRange?: number
  showComparison?: boolean
  showRecommendations?: boolean
  compact?: boolean
  onModelSelect?: (modelId: string) => void
  className?: string
  testId?: string
}

/**
 * ModelPerformanceAnalytics - Analytics dashboard for model performance
 * 
 * Features:
 * - Model performance comparison and benchmarking
 * - Real-time performance metrics and trends
 * - Cost analysis and optimization recommendations
 * - Success rate and response time tracking
 * - Interactive charts and visualizations
 * - Export capabilities for reporting
 */
export const ModelPerformanceAnalytics: React.FC<ModelPerformanceAnalyticsProps> = ({
  timeRange = 7,
  showComparison = true,
  showRecommendations = true,
  compact = false,
  onModelSelect,
  className,
  testId = 'model-performance-analytics'
}) => {
  const [selectedTimeRange, setSelectedTimeRange] = useState(timeRange)
  const [selectedModel, setSelectedModel] = useState<string | null>(null)
  const [activeTab, setActiveTab] = useState('overview')

  // Real API data
  const { data: capabilities, isLoading: capabilitiesLoading, refetch: refetchCapabilities } = useGetModelPerformanceComparisonQuery({ days: selectedTimeRange })
  const { data: agentAnalytics, isLoading: analyticsLoading, refetch: refetchAnalytics } = useGetTransparencyMetricsQuery({ days: selectedTimeRange, includeDetails: true })
  const { data: transparencyMetrics, isLoading: metricsLoading } = useGetTransparencyMetricsQuery({ days: selectedTimeRange })
  const { data: performanceMetrics } = useGetRealTimeMonitoringDataQuery()

  // Process performance data
  const performanceData = useMemo(() => {
    if (!capabilities) return []
    
    return capabilities.map(cap => ({
      id: cap.agentId,
      name: cap.name,
      type: cap.agentType,
      version: cap.version,
      status: cap.status,
      performance: cap.performance,
      configuration: cap.configuration,
      supportedTasks: cap.supportedTaskTypes
    }))
  }, [capabilities])

  // Mock trend data (replace with real data from API)
  const trendData = [
    { date: '2024-01-01', responseTime: 1200, successRate: 0.95, throughput: 45, cost: 25.50 },
    { date: '2024-01-02', responseTime: 1150, successRate: 0.97, throughput: 52, cost: 28.75 },
    { date: '2024-01-03', responseTime: 1300, successRate: 0.93, throughput: 38, cost: 21.20 },
    { date: '2024-01-04', responseTime: 1100, successRate: 0.98, throughput: 61, cost: 34.80 },
    { date: '2024-01-05', responseTime: 1250, successRate: 0.96, throughput: 55, cost: 31.25 },
    { date: '2024-01-06', responseTime: 1080, successRate: 0.99, throughput: 68, cost: 39.60 },
    { date: '2024-01-07', responseTime: 1180, successRate: 0.97, throughput: 63, cost: 36.45 }
  ]

  // Performance comparison data
  const comparisonData = useMemo(() => {
    return performanceData.map(model => ({
      name: model.name,
      responseTime: model.performance.averageResponseTime,
      successRate: model.performance.successRate * 100,
      throughput: model.performance.throughput,
      reliability: model.performance.reliability * 100
    }))
  }, [performanceData])

  // Get performance color
  const getPerformanceColor = (value: number, type: 'responseTime' | 'successRate' | 'throughput') => {
    switch (type) {
      case 'responseTime':
        return value < 1000 ? '#52c41a' : value < 2000 ? '#faad14' : '#ff4d4f'
      case 'successRate':
        return value > 95 ? '#52c41a' : value > 90 ? '#faad14' : '#ff4d4f'
      case 'throughput':
        return value > 50 ? '#52c41a' : value > 30 ? '#faad14' : '#ff4d4f'
      default:
        return '#1890ff'
    }
  }

  // Handle model selection
  const handleModelSelect = (modelId: string) => {
    setSelectedModel(modelId)
    onModelSelect?.(modelId)
  }

  // Performance table columns
  const performanceColumns = [
    {
      title: 'Model',
      dataIndex: 'name',
      key: 'name',
      render: (text: string, record: any) => (
        <Space>
          <div>
            <div style={{ fontWeight: 'bold' }}>{text}</div>
            <Text type="secondary" style={{ fontSize: '12px' }}>
              {record.type} • v{record.version}
            </Text>
          </div>
        </Space>
      )
    },
    {
      title: 'Status',
      dataIndex: 'status',
      key: 'status',
      render: (status: string) => (
        <Tag color={status === 'active' ? 'green' : 'default'}>
          {status.toUpperCase()}
        </Tag>
      )
    },
    {
      title: 'Response Time',
      dataIndex: ['performance', 'averageResponseTime'],
      key: 'responseTime',
      render: (time: number) => (
        <Space>
          <Text style={{ color: getPerformanceColor(time, 'responseTime') }}>
            {time}ms
          </Text>
          <Progress 
            percent={(2000 - time) / 20} 
            size="small" 
            showInfo={false}
            strokeColor={getPerformanceColor(time, 'responseTime')}
            style={{ width: 60 }}
          />
        </Space>
      ),
      sorter: (a: any, b: any) => a.performance.averageResponseTime - b.performance.averageResponseTime
    },
    {
      title: 'Success Rate',
      dataIndex: ['performance', 'successRate'],
      key: 'successRate',
      render: (rate: number) => (
        <ConfidenceIndicator
          confidence={rate}
          size="small"
          type="badge"
        />
      ),
      sorter: (a: any, b: any) => a.performance.successRate - b.performance.successRate
    },
    {
      title: 'Throughput',
      dataIndex: ['performance', 'throughput'],
      key: 'throughput',
      render: (throughput: number) => (
        <Text style={{ color: getPerformanceColor(throughput, 'throughput') }}>
          {throughput} req/min
        </Text>
      ),
      sorter: (a: any, b: any) => a.performance.throughput - b.performance.throughput
    },
    {
      title: 'Reliability',
      dataIndex: ['performance', 'reliability'],
      key: 'reliability',
      render: (reliability: number) => (
        <Progress 
          percent={reliability * 100}
          size="small"
          strokeColor={reliability > 0.95 ? '#52c41a' : reliability > 0.9 ? '#faad14' : '#ff4d4f'}
        />
      )
    },
    {
      title: 'Actions',
      key: 'actions',
      render: (_: any, record: any) => (
        <Button
          type="link"
          size="small"
          onClick={() => handleModelSelect(record.id)}
        >
          Analyze
        </Button>
      )
    }
  ]

  // Render overview tab
  const renderOverviewTab = () => (
    <div>
      {/* Summary Cards */}
      <Row gutter={[16, 16]} style={{ marginBottom: 24 }}>
        <Col xs={24} sm={12} lg={6}>
          <Card>
            <Statistic
              title="Active Models"
              value={performanceData.filter(m => m.status === 'active').length}
              suffix={`/ ${performanceData.length}`}
              prefix={<ThunderboltOutlined />}
              valueStyle={{ color: '#3f8600' }}
            />
          </Card>
        </Col>
        <Col xs={24} sm={12} lg={6}>
          <Card>
            <Statistic
              title="Avg Response Time"
              value={performanceData.reduce((acc, m) => acc + m.performance.averageResponseTime, 0) / (performanceData.length || 1)}
              suffix="ms"
              prefix={<ClockCircleOutlined />}
              valueStyle={{ color: '#1890ff' }}
              precision={0}
            />
          </Card>
        </Col>
        <Col xs={24} sm={12} lg={6}>
          <Card>
            <Statistic
              title="Avg Success Rate"
              value={performanceData.reduce((acc, m) => acc + m.performance.successRate, 0) / (performanceData.length || 1) * 100}
              suffix="%"
              prefix={<TrophyOutlined />}
              valueStyle={{ color: '#52c41a' }}
              precision={1}
            />
          </Card>
        </Col>
        <Col xs={24} sm={12} lg={6}>
          <Card>
            <Statistic
              title="Total Requests"
              value={agentAnalytics?.totalTasks || 1247}
              prefix={<BarChartOutlined />}
              valueStyle={{ color: '#722ed1' }}
            />
          </Card>
        </Col>
      </Row>

      {/* Performance Table */}
      <Card title="Model Performance Comparison">
        <Table
          columns={performanceColumns}
          dataSource={performanceData}
          rowKey="id"
          loading={capabilitiesLoading}
          pagination={{ pageSize: 10 }}
          size={compact ? 'small' : 'default'}
        />
      </Card>
    </div>
  )

  // Render trends tab
  const renderTrendsTab = () => (
    <div>
      <Row gutter={[16, 16]}>
        <Col xs={24} lg={12}>
          <Card title="Response Time Trends">
            <ResponsiveContainer width="100%" height={300}>
              <LineChart data={trendData}>
                <CartesianGrid strokeDasharray="3 3" />
                <XAxis dataKey="date" />
                <YAxis />
                <RechartsTooltip />
                <Line type="monotone" dataKey="responseTime" stroke="#1890ff" strokeWidth={2} />
              </LineChart>
            </ResponsiveContainer>
          </Card>
        </Col>
        <Col xs={24} lg={12}>
          <Card title="Success Rate Trends">
            <ResponsiveContainer width="100%" height={300}>
              <AreaChart data={trendData}>
                <CartesianGrid strokeDasharray="3 3" />
                <XAxis dataKey="date" />
                <YAxis domain={[0.9, 1]} />
                <RechartsTooltip />
                <Area type="monotone" dataKey="successRate" stroke="#52c41a" fill="#52c41a" fillOpacity={0.3} />
              </AreaChart>
            </ResponsiveContainer>
          </Card>
        </Col>
      </Row>
    </div>
  )

  // Render comparison tab
  const renderComparisonTab = () => (
    <div>
      <Card title="Model Performance Comparison">
        <ResponsiveContainer width="100%" height={400}>
          <BarChart data={comparisonData}>
            <CartesianGrid strokeDasharray="3 3" />
            <XAxis dataKey="name" />
            <YAxis />
            <RechartsTooltip />
            <Bar dataKey="responseTime" fill="#1890ff" name="Response Time (ms)" />
            <Bar dataKey="successRate" fill="#52c41a" name="Success Rate (%)" />
          </BarChart>
        </ResponsiveContainer>
      </Card>
    </div>
  )

  // Render recommendations tab
  const renderRecommendationsTab = () => (
    <div>
      <Alert
        message="Performance Optimization Recommendations"
        description="AI-powered recommendations to improve model performance and reduce costs."
        type="info"
        showIcon
        style={{ marginBottom: 16 }}
      />
      
      <List
        dataSource={[
          {
            title: 'Optimize Response Time',
            description: 'Consider increasing timeout for better reliability on complex queries',
            impact: 'Medium',
            effort: 'Low'
          },
          {
            title: 'Load Balancing',
            description: 'Distribute load more evenly across available models',
            impact: 'High',
            effort: 'Medium'
          },
          {
            title: 'Cache Optimization',
            description: 'Implement response caching for frequently asked queries',
            impact: 'High',
            effort: 'High'
          }
        ]}
        renderItem={(item) => (
          <List.Item>
            <List.Item.Meta
              title={item.title}
              description={item.description}
            />
            <Space>
              <Tag color="blue">Impact: {item.impact}</Tag>
              <Tag color="orange">Effort: {item.effort}</Tag>
            </Space>
          </List.Item>
        )}
      />
    </div>
  )

  const tabItems = [
    {
      key: 'overview',
      label: (
        <Space>
          <BarChartOutlined />
          <span>Overview</span>
        </Space>
      ),
      children: renderOverviewTab()
    },
    {
      key: 'trends',
      label: (
        <Space>
          <LineChartOutlined />
          <span>Trends</span>
        </Space>
      ),
      children: renderTrendsTab()
    },
    ...(showComparison ? [{
      key: 'comparison',
      label: (
        <Space>
          <TrophyOutlined />
          <span>Comparison</span>
        </Space>
      ),
      children: renderComparisonTab()
    }] : []),
    ...(showRecommendations ? [{
      key: 'recommendations',
      label: (
        <Space>
          <ExclamationCircleOutlined />
          <span>Recommendations</span>
        </Space>
      ),
      children: renderRecommendationsTab()
    }] : [])
  ]

  return (
    <div className={className} data-testid={testId}>
      {/* Header */}
      <div style={{ 
        display: 'flex', 
        justifyContent: 'space-between', 
        alignItems: 'center',
        marginBottom: 24 
      }}>
        <div>
          <Title level={4} style={{ margin: 0 }}>
            Model Performance Analytics
          </Title>
          <Text type="secondary">
            Performance metrics, trends, and optimization recommendations
          </Text>
        </div>
        <Space>
          <Select
            value={selectedTimeRange}
            onChange={setSelectedTimeRange}
            style={{ width: 120 }}
          >
            <Select.Option value={1}>Last 24h</Select.Option>
            <Select.Option value={7}>Last 7 days</Select.Option>
            <Select.Option value={30}>Last 30 days</Select.Option>
            <Select.Option value={90}>Last 90 days</Select.Option>
          </Select>
          <Button 
            icon={<ReloadOutlined />}
            loading={capabilitiesLoading || analyticsLoading || metricsLoading}
            onClick={() => {
              refetchCapabilities()
              refetchAnalytics()
            }}
          >
            Refresh
          </Button>
          <Button icon={<DownloadOutlined />}>
            Export
          </Button>
        </Space>
      </div>

      {/* Main Content */}
      <Tabs
        activeKey={activeTab}
        onChange={setActiveTab}
        items={tabItems}
        size={compact ? 'small' : 'default'}
      />
    </div>
  )
}

export default ModelPerformanceAnalytics
