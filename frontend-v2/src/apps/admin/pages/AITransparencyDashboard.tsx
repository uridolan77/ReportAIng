import React, { useState } from 'react'
import { 
  Card, 
  Row, 
  Col, 
  Table, 
  Typography, 
  Space, 
  Button, 
  Select, 
  DatePicker,
  Statistic,
  Tag,
  Alert,
  Tabs,
  List,
  Progress,
  Tooltip,
  Input
} from 'antd'
import {
  EyeOutlined,
  BarChartOutlined,
  BulbOutlined,
  SearchOutlined,
  DownloadOutlined,
  ReloadOutlined,
  FilterOutlined,
  TrophyOutlined,
  ExclamationCircleOutlined,
  CheckCircleOutlined,
  ClockCircleOutlined
} from '@ant-design/icons'
import { LineChart, Line, BarChart, Bar, PieChart, Pie, Cell, XAxis, YAxis, CartesianGrid, Tooltip as RechartsTooltip, ResponsiveContainer } from 'recharts'
import { AIFeatureWrapper } from '@shared/components/ai/common/AIFeatureWrapper'
import { ConfidenceIndicator } from '@shared/components/ai/common/ConfidenceIndicator'
import { AITransparencyPanel } from '@shared/components/ai/transparency/AITransparencyPanel'
import { useGetTransparencyDashboardMetricsQuery, useTransparencyDashboard } from '@shared/store/api/transparencyApi'
import dayjs from 'dayjs'

const { Title, Text } = Typography
const { RangePicker } = DatePicker
const { Search } = Input

interface TransparencyTrace {
  id: string
  timestamp: string
  query: string
  confidence: number
  processingTime: number
  model: string
  status: 'success' | 'warning' | 'error'
  optimizations: number
}

/**
 * AITransparencyDashboard - Dedicated dashboard for AI decision transparency
 * 
 * Features:
 * - Transparency metrics and analytics
 * - Trace exploration and analysis
 * - Confidence trend monitoring
 * - Optimization tracking
 * - Model performance comparison
 * - Export capabilities
 */
export const AITransparencyDashboard: React.FC = () => {
  const [selectedTrace, setSelectedTrace] = useState<string | null>(null)
  const [timeRange, setTimeRange] = useState(30)
  const [searchQuery, setSearchQuery] = useState('')
  const [activeTab, setActiveTab] = useState('overview')

  const { metrics, settings, isLoading, refetch } = useTransparencyDashboard(timeRange)

  // Mock trace data
  const mockTraces: TransparencyTrace[] = [
    {
      id: 'trace-001',
      timestamp: '2024-01-15T10:30:00Z',
      query: 'Show me quarterly sales by region',
      confidence: 0.94,
      processingTime: 1200,
      model: 'gpt-4',
      status: 'success',
      optimizations: 3
    },
    {
      id: 'trace-002',
      timestamp: '2024-01-15T10:25:00Z',
      query: 'What are the top performing products?',
      confidence: 0.87,
      processingTime: 950,
      model: 'claude-3',
      status: 'success',
      optimizations: 2
    },
    {
      id: 'trace-003',
      timestamp: '2024-01-15T10:20:00Z',
      query: 'Compare this year vs last year revenue',
      confidence: 0.76,
      processingTime: 1800,
      model: 'gpt-3.5',
      status: 'warning',
      optimizations: 1
    }
  ]

  // Mock confidence trend data
  const confidenceTrendData = [
    { date: '2024-01-01', confidence: 0.85, traces: 45 },
    { date: '2024-01-02', confidence: 0.87, traces: 52 },
    { date: '2024-01-03', confidence: 0.83, traces: 38 },
    { date: '2024-01-04', confidence: 0.91, traces: 61 },
    { date: '2024-01-05', confidence: 0.89, traces: 55 },
    { date: '2024-01-06', confidence: 0.93, traces: 68 },
    { date: '2024-01-07', confidence: 0.88, traces: 63 }
  ]

  // Mock model performance data
  const modelPerformanceData = [
    { model: 'GPT-4', confidence: 0.92, count: 145, avgTime: 1200 },
    { model: 'Claude-3', confidence: 0.89, count: 98, avgTime: 950 },
    { model: 'GPT-3.5', confidence: 0.84, count: 76, avgTime: 800 }
  ]

  const getStatusColor = (status: string) => {
    switch (status) {
      case 'success': return 'green'
      case 'warning': return 'orange'
      case 'error': return 'red'
      default: return 'default'
    }
  }

  const getStatusIcon = (status: string) => {
    switch (status) {
      case 'success': return <CheckCircleOutlined style={{ color: '#52c41a' }} />
      case 'warning': return <ExclamationCircleOutlined style={{ color: '#faad14' }} />
      case 'error': return <ExclamationCircleOutlined style={{ color: '#ff4d4f' }} />
      default: return null
    }
  }

  const traceColumns = [
    {
      title: 'Timestamp',
      dataIndex: 'timestamp',
      key: 'timestamp',
      render: (timestamp: string) => (
        <Text type="secondary">
          {dayjs(timestamp).format('MMM DD, HH:mm')}
        </Text>
      )
    },
    {
      title: 'Query',
      dataIndex: 'query',
      key: 'query',
      ellipsis: true,
      render: (query: string) => (
        <Tooltip title={query}>
          <Text>{query}</Text>
        </Tooltip>
      )
    },
    {
      title: 'Confidence',
      dataIndex: 'confidence',
      key: 'confidence',
      render: (confidence: number) => (
        <ConfidenceIndicator
          confidence={confidence}
          size="small"
          type="badge"
        />
      )
    },
    {
      title: 'Model',
      dataIndex: 'model',
      key: 'model',
      render: (model: string) => <Tag color="blue">{model}</Tag>
    },
    {
      title: 'Time',
      dataIndex: 'processingTime',
      key: 'processingTime',
      render: (time: number) => (
        <Space>
          <ClockCircleOutlined />
          <Text>{time}ms</Text>
        </Space>
      )
    },
    {
      title: 'Status',
      dataIndex: 'status',
      key: 'status',
      render: (status: string) => (
        <Space>
          {getStatusIcon(status)}
          <Tag color={getStatusColor(status)}>
            {status.toUpperCase()}
          </Tag>
        </Space>
      )
    },
    {
      title: 'Actions',
      key: 'actions',
      render: (_, record: TransparencyTrace) => (
        <Button
          type="link"
          size="small"
          onClick={() => setSelectedTrace(record.id)}
        >
          Explore
        </Button>
      )
    }
  ]

  const renderOverviewTab = () => (
    <div>
      {/* Key Metrics */}
      <Row gutter={[16, 16]} style={{ marginBottom: 24 }}>
        <Col xs={24} sm={12} lg={6}>
          <Card>
            <Statistic
              title="Total Traces"
              value={metrics?.totalTraces || 1247}
              prefix={<EyeOutlined />}
              valueStyle={{ color: '#3f8600' }}
            />
          </Card>
        </Col>
        <Col xs={24} sm={12} lg={6}>
          <Card>
            <Statistic
              title="Avg Confidence"
              value={metrics?.averageConfidence ? (metrics.averageConfidence * 100).toFixed(1) : '87.3'}
              suffix="%"
              prefix={<TrophyOutlined />}
              valueStyle={{ color: '#1890ff' }}
            />
          </Card>
        </Col>
        <Col xs={24} sm={12} lg={6}>
          <Card>
            <Statistic
              title="Optimizations"
              value={metrics?.topOptimizations?.length || 156}
              prefix={<BulbOutlined />}
              valueStyle={{ color: '#722ed1' }}
            />
          </Card>
        </Col>
        <Col xs={24} sm={12} lg={6}>
          <Card>
            <Statistic
              title="Success Rate"
              value={94.5}
              suffix="%"
              prefix={<CheckCircleOutlined />}
              valueStyle={{ color: '#52c41a' }}
            />
          </Card>
        </Col>
      </Row>

      {/* Charts */}
      <Row gutter={[16, 16]} style={{ marginBottom: 24 }}>
        <Col xs={24} lg={16}>
          <Card title="Confidence Trends">
            <ResponsiveContainer width="100%" height={300}>
              <LineChart data={confidenceTrendData}>
                <CartesianGrid strokeDasharray="3 3" />
                <XAxis dataKey="date" />
                <YAxis domain={[0.7, 1]} />
                <RechartsTooltip />
                <Line type="monotone" dataKey="confidence" stroke="#1890ff" strokeWidth={2} />
              </LineChart>
            </ResponsiveContainer>
          </Card>
        </Col>
        <Col xs={24} lg={8}>
          <Card title="Model Performance">
            <Space direction="vertical" style={{ width: '100%' }}>
              {modelPerformanceData.map((model, index) => (
                <div key={index} style={{ marginBottom: 16 }}>
                  <div style={{ display: 'flex', justifyContent: 'space-between', marginBottom: 4 }}>
                    <Text strong>{model.model}</Text>
                    <Text>{(model.confidence * 100).toFixed(1)}%</Text>
                  </div>
                  <Progress 
                    percent={model.confidence * 100}
                    strokeColor="#1890ff"
                    showInfo={false}
                  />
                  <Text type="secondary" style={{ fontSize: '12px' }}>
                    {model.count} traces • {model.avgTime}ms avg
                  </Text>
                </div>
              ))}
            </Space>
          </Card>
        </Col>
      </Row>

      {/* Recent Traces */}
      <Card title="Recent Transparency Traces">
        <Table
          columns={traceColumns}
          dataSource={mockTraces}
          rowKey="id"
          pagination={{ pageSize: 10 }}
          size="small"
        />
      </Card>
    </div>
  )

  const renderExplorerTab = () => (
    <div>
      <Row gutter={[16, 16]}>
        <Col xs={24} lg={8}>
          <Card 
            title="Trace Explorer"
            extra={
              <Button 
                size="small"
                icon={<ReloadOutlined />}
                onClick={() => setSelectedTrace(null)}
              >
                Clear
              </Button>
            }
          >
            <Space direction="vertical" style={{ width: '100%', marginBottom: 16 }}>
              <Search
                placeholder="Search traces..."
                value={searchQuery}
                onChange={(e) => setSearchQuery(e.target.value)}
                style={{ width: '100%' }}
              />
              <Select
                placeholder="Select a trace"
                style={{ width: '100%' }}
                value={selectedTrace}
                onChange={setSelectedTrace}
              >
                {mockTraces.map(trace => (
                  <Select.Option key={trace.id} value={trace.id}>
                    {trace.query.substring(0, 50)}...
                  </Select.Option>
                ))}
              </Select>
            </Space>
            
            <List
              size="small"
              dataSource={mockTraces}
              renderItem={(trace) => (
                <List.Item
                  style={{ 
                    cursor: 'pointer',
                    backgroundColor: selectedTrace === trace.id ? '#f0f8ff' : 'transparent'
                  }}
                  onClick={() => setSelectedTrace(trace.id)}
                >
                  <List.Item.Meta
                    title={
                      <Space>
                        <Text ellipsis style={{ maxWidth: 200 }}>
                          {trace.query}
                        </Text>
                        <ConfidenceIndicator
                          confidence={trace.confidence}
                          size="small"
                          type="badge"
                          showPercentage={false}
                        />
                      </Space>
                    }
                    description={
                      <Space>
                        <Text type="secondary" style={{ fontSize: '12px' }}>
                          {dayjs(trace.timestamp).format('MMM DD, HH:mm')}
                        </Text>
                        <Tag size="small">{trace.model}</Tag>
                      </Space>
                    }
                  />
                </List.Item>
              )}
            />
          </Card>
        </Col>
        <Col xs={24} lg={16}>
          <Card title="Transparency Analysis">
            {selectedTrace ? (
              <AITransparencyPanel
                traceId={selectedTrace}
                showDetailedMetrics={true}
                compact={false}
              />
            ) : (
              <div style={{ textAlign: 'center', padding: '60px 0' }}>
                <Space direction="vertical">
                  <EyeOutlined style={{ fontSize: '48px', color: '#d9d9d9' }} />
                  <Text type="secondary">
                    Select a trace from the left panel to explore AI transparency
                  </Text>
                </Space>
              </div>
            )}
          </Card>
        </Col>
      </Row>
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
      key: 'explorer',
      label: (
        <Space>
          <SearchOutlined />
          <span>Trace Explorer</span>
        </Space>
      ),
      children: renderExplorerTab()
    }
  ]

  return (
    <AIFeatureWrapper feature="transparencyPanelEnabled">
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
                AI Transparency Dashboard
              </Space>
            </Title>
            <Text type="secondary">
              Explore AI decision-making processes, analyze confidence patterns, and optimize performance
            </Text>
          </div>
          <Space>
            <Select value={timeRange} onChange={setTimeRange} style={{ width: 120 }}>
              <Select.Option value={7}>Last 7 days</Select.Option>
              <Select.Option value={30}>Last 30 days</Select.Option>
              <Select.Option value={90}>Last 90 days</Select.Option>
            </Select>
            <Button icon={<ReloadOutlined />} loading={isLoading} onClick={refetch}>
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
          size="large"
        />
      </div>
    </AIFeatureWrapper>
  )
}

export default AITransparencyDashboard
