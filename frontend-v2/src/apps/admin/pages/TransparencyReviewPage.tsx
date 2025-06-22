import React, { useState, useMemo } from 'react'
import { 
  Card, 
  Row, 
  Col, 
  Table, 
  Space, 
  Typography, 
  Input, 
  Select, 
  DatePicker, 
  Button,
  Tag,
  Drawer,
  Alert,
  Tabs
} from 'antd'
import {
  SearchOutlined,
  FilterOutlined,
  EyeOutlined,
  DownloadOutlined,
  ReloadOutlined,
  BarChartOutlined,
  ClockCircleOutlined,
  UserOutlined
} from '@ant-design/icons'
import { TraceDetailViewer } from '@shared/components/ai/transparency/TraceDetailViewer'
import { QueryFlowAnalyzer } from '@shared/components/ai/transparency/QueryFlowAnalyzer'
import { LiveQueryTracker } from '@shared/components/ai/transparency/LiveQueryTracker'
import { InstantFeedbackPanel } from '@shared/components/ai/transparency/InstantFeedbackPanel'
import { ConfidenceIndicator } from '@shared/components/ai/common/ConfidenceIndicator'
import { useGetTransparencyTraceQuery } from '@shared/store/api/transparencyApi'
import type { TransparencyTrace } from '@shared/types/transparency'
import dayjs from 'dayjs'

const { Title, Text } = Typography
const { Search } = Input
const { RangePicker } = DatePicker
const { TabPane } = Tabs

interface TraceListItem {
  id: string
  traceId: string
  userId: string
  userQuestion: string
  intentType: string
  overallConfidence: number
  totalTokens: number
  success: boolean
  createdAt: string
  processingTime: number
  stepCount: number
}

/**
 * TransparencyReviewPage - Comprehensive review interface for transparency data
 * 
 * Features:
 * - Advanced trace filtering and search
 * - Detailed trace analysis
 * - Query flow visualization
 * - Live query tracking
 * - User feedback collection
 * - Performance analytics
 * - Export capabilities
 */
export const TransparencyReviewPage: React.FC = () => {
  const [selectedTrace, setSelectedTrace] = useState<string | null>(null)
  const [drawerVisible, setDrawerVisible] = useState(false)
  const [searchQuery, setSearchQuery] = useState('')
  const [dateRange, setDateRange] = useState<[dayjs.Dayjs, dayjs.Dayjs] | null>(null)
  const [confidenceFilter, setConfidenceFilter] = useState<string>('all')
  const [statusFilter, setStatusFilter] = useState<string>('all')
  const [userFilter, setUserFilter] = useState<string>('all')
  const [activeTab, setActiveTab] = useState('traces')

  // Mock trace data - in real app, this would come from API
  const mockTraces: TraceListItem[] = [
    {
      id: '1',
      traceId: 'trace-001',
      userId: 'user-123',
      userQuestion: 'Show me quarterly sales by region',
      intentType: 'analytics',
      overallConfidence: 0.94,
      totalTokens: 1250,
      success: true,
      createdAt: '2024-01-15T10:30:00Z',
      processingTime: 2400,
      stepCount: 5
    },
    {
      id: '2',
      traceId: 'trace-002',
      userId: 'user-456',
      userQuestion: 'What are the top performing products?',
      intentType: 'ranking',
      overallConfidence: 0.87,
      totalTokens: 980,
      success: true,
      createdAt: '2024-01-15T10:25:00Z',
      processingTime: 1800,
      stepCount: 4
    },
    {
      id: '3',
      traceId: 'trace-003',
      userId: 'user-789',
      userQuestion: 'Compare this year vs last year revenue',
      intentType: 'comparison',
      overallConfidence: 0.76,
      totalTokens: 1450,
      success: false,
      createdAt: '2024-01-15T10:20:00Z',
      processingTime: 3200,
      stepCount: 6
    }
  ]

  const { data: traceDetails } = useGetTransparencyTraceQuery(selectedTrace!, {
    skip: !selectedTrace
  })

  // Filter traces based on search and filter criteria
  const filteredTraces = useMemo(() => {
    let filtered = mockTraces

    // Search filter
    if (searchQuery) {
      filtered = filtered.filter(trace => 
        trace.userQuestion.toLowerCase().includes(searchQuery.toLowerCase()) ||
        trace.traceId.toLowerCase().includes(searchQuery.toLowerCase()) ||
        trace.userId.toLowerCase().includes(searchQuery.toLowerCase())
      )
    }

    // Confidence filter
    if (confidenceFilter !== 'all') {
      filtered = filtered.filter(trace => {
        switch (confidenceFilter) {
          case 'high': return trace.overallConfidence >= 0.8
          case 'medium': return trace.overallConfidence >= 0.6 && trace.overallConfidence < 0.8
          case 'low': return trace.overallConfidence < 0.6
          default: return true
        }
      })
    }

    // Status filter
    if (statusFilter !== 'all') {
      filtered = filtered.filter(trace => {
        switch (statusFilter) {
          case 'success': return trace.success
          case 'failed': return !trace.success
          default: return true
        }
      })
    }

    // Date range filter
    if (dateRange) {
      filtered = filtered.filter(trace => {
        const traceDate = dayjs(trace.createdAt)
        return traceDate.isAfter(dateRange[0]) && traceDate.isBefore(dateRange[1])
      })
    }

    return filtered
  }, [mockTraces, searchQuery, confidenceFilter, statusFilter, dateRange])

  const handleTraceSelect = (traceId: string) => {
    setSelectedTrace(traceId)
    setDrawerVisible(true)
  }

  const handleExportTraces = () => {
    // Implementation for exporting traces
    console.log('Exporting traces:', filteredTraces)
  }

  const columns = [
    {
      title: 'Trace ID',
      dataIndex: 'traceId',
      key: 'traceId',
      render: (traceId: string) => (
        <Button 
          type="link" 
          onClick={() => handleTraceSelect(traceId)}
          style={{ padding: 0 }}
        >
          {traceId}
        </Button>
      )
    },
    {
      title: 'User Question',
      dataIndex: 'userQuestion',
      key: 'userQuestion',
      ellipsis: true,
      render: (question: string) => (
        <Text>{question}</Text>
      )
    },
    {
      title: 'User',
      dataIndex: 'userId',
      key: 'userId',
      render: (userId: string) => (
        <Space>
          <UserOutlined />
          <Text>{userId}</Text>
        </Space>
      )
    },
    {
      title: 'Intent',
      dataIndex: 'intentType',
      key: 'intentType',
      render: (intent: string) => (
        <Tag color="blue">{intent}</Tag>
      )
    },
    {
      title: 'Confidence',
      dataIndex: 'overallConfidence',
      key: 'confidence',
      render: (confidence: number) => (
        <ConfidenceIndicator
          confidence={confidence}
          size="small"
          type="badge"
        />
      ),
      sorter: (a: TraceListItem, b: TraceListItem) => a.overallConfidence - b.overallConfidence
    },
    {
      title: 'Tokens',
      dataIndex: 'totalTokens',
      key: 'tokens',
      render: (tokens: number) => (
        <Space>
          <BarChartOutlined />
          <Text>{tokens}</Text>
        </Space>
      ),
      sorter: (a: TraceListItem, b: TraceListItem) => a.totalTokens - b.totalTokens
    },
    {
      title: 'Processing Time',
      dataIndex: 'processingTime',
      key: 'processingTime',
      render: (time: number) => (
        <Space>
          <ClockCircleOutlined />
          <Text>{time}ms</Text>
        </Space>
      ),
      sorter: (a: TraceListItem, b: TraceListItem) => a.processingTime - b.processingTime
    },
    {
      title: 'Status',
      dataIndex: 'success',
      key: 'status',
      render: (success: boolean) => (
        <Tag color={success ? 'green' : 'red'}>
          {success ? 'Success' : 'Failed'}
        </Tag>
      )
    },
    {
      title: 'Created',
      dataIndex: 'createdAt',
      key: 'createdAt',
      render: (date: string) => (
        <Text type="secondary">
          {dayjs(date).format('MMM DD, HH:mm')}
        </Text>
      ),
      sorter: (a: TraceListItem, b: TraceListItem) => 
        dayjs(a.createdAt).unix() - dayjs(b.createdAt).unix()
    },
    {
      title: 'Actions',
      key: 'actions',
      render: (_, record: TraceListItem) => (
        <Space>
          <Button
            type="link"
            size="small"
            icon={<EyeOutlined />}
            onClick={() => handleTraceSelect(record.traceId)}
          >
            View
          </Button>
        </Space>
      )
    }
  ]

  const renderTracesTab = () => (
    <Space direction="vertical" style={{ width: '100%' }} size="large">
      {/* Filters */}
      <Card size="small">
        <Row gutter={[16, 16]} align="middle">
          <Col xs={24} sm={8}>
            <Search
              placeholder="Search traces, questions, or users..."
              value={searchQuery}
              onChange={(e) => setSearchQuery(e.target.value)}
              allowClear
            />
          </Col>
          <Col xs={24} sm={4}>
            <Select
              value={confidenceFilter}
              onChange={setConfidenceFilter}
              style={{ width: '100%' }}
              placeholder="Confidence"
            >
              <Select.Option value="all">All Confidence</Select.Option>
              <Select.Option value="high">High (≥80%)</Select.Option>
              <Select.Option value="medium">Medium (60-80%)</Select.Option>
              <Select.Option value="low">Low (&lt;60%)</Select.Option>
            </Select>
          </Col>
          <Col xs={24} sm={4}>
            <Select
              value={statusFilter}
              onChange={setStatusFilter}
              style={{ width: '100%' }}
              placeholder="Status"
            >
              <Select.Option value="all">All Status</Select.Option>
              <Select.Option value="success">Success</Select.Option>
              <Select.Option value="failed">Failed</Select.Option>
            </Select>
          </Col>
          <Col xs={24} sm={6}>
            <RangePicker
              value={dateRange}
              onChange={setDateRange}
              style={{ width: '100%' }}
            />
          </Col>
          <Col xs={24} sm={2}>
            <Button
              icon={<DownloadOutlined />}
              onClick={handleExportTraces}
              title="Export filtered traces"
            >
              Export
            </Button>
          </Col>
        </Row>
      </Card>

      {/* Summary Stats */}
      <Row gutter={[16, 16]}>
        <Col xs={24} sm={6}>
          <Card size="small">
            <Space direction="vertical" style={{ width: '100%' }}>
              <Text strong>Total Traces</Text>
              <Text style={{ fontSize: '24px', color: '#1890ff' }}>
                {filteredTraces.length}
              </Text>
            </Space>
          </Card>
        </Col>
        <Col xs={24} sm={6}>
          <Card size="small">
            <Space direction="vertical" style={{ width: '100%' }}>
              <Text strong>Success Rate</Text>
              <Text style={{ fontSize: '24px', color: '#52c41a' }}>
                {((filteredTraces.filter(t => t.success).length / filteredTraces.length) * 100).toFixed(1)}%
              </Text>
            </Space>
          </Card>
        </Col>
        <Col xs={24} sm={6}>
          <Card size="small">
            <Space direction="vertical" style={{ width: '100%' }}>
              <Text strong>Avg Confidence</Text>
              <Text style={{ fontSize: '24px', color: '#722ed1' }}>
                {((filteredTraces.reduce((sum, t) => sum + t.overallConfidence, 0) / filteredTraces.length) * 100).toFixed(1)}%
              </Text>
            </Space>
          </Card>
        </Col>
        <Col xs={24} sm={6}>
          <Card size="small">
            <Space direction="vertical" style={{ width: '100%' }}>
              <Text strong>Avg Tokens</Text>
              <Text style={{ fontSize: '24px', color: '#faad14' }}>
                {Math.round(filteredTraces.reduce((sum, t) => sum + t.totalTokens, 0) / filteredTraces.length)}
              </Text>
            </Space>
          </Card>
        </Col>
      </Row>

      {/* Traces Table */}
      <Card title="Transparency Traces" size="small">
        <Table
          columns={columns}
          dataSource={filteredTraces}
          rowKey="id"
          pagination={{
            pageSize: 20,
            showSizeChanger: true,
            showQuickJumper: true,
            showTotal: (total, range) => 
              `${range[0]}-${range[1]} of ${total} traces`
          }}
          size="small"
        />
      </Card>
    </Space>
  )

  const renderLiveTrackingTab = () => (
    <Space direction="vertical" style={{ width: '100%' }} size="large">
      <Alert
        message="Live Query Tracking"
        description="Track queries in real-time as they are processed by the AI system."
        type="info"
        showIcon
      />
      
      <LiveQueryTracker
        queryId="live-query-demo"
        autoTrack={true}
        showStepDetails={true}
        showUserInfo={true}
      />
    </Space>
  )

  const renderFeedbackTab = () => (
    <Space direction="vertical" style={{ width: '100%' }} size="large">
      <Alert
        message="User Feedback Collection"
        description="Collect and analyze user feedback on AI responses and system performance."
        type="info"
        showIcon
      />
      
      <InstantFeedbackPanel
        queryId={selectedTrace || 'demo-query'}
        traceId={selectedTrace}
        showRating={true}
        showComments={true}
        showSuggestions={true}
      />
    </Space>
  )

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
              Transparency Review
            </Space>
          </Title>
          <Text type="secondary">
            Analyze query flows, review transparency traces, and monitor AI decision-making processes
          </Text>
        </div>
        <Space>
          <Button icon={<ReloadOutlined />}>
            Refresh
          </Button>
          <Button type="primary" icon={<DownloadOutlined />}>
            Export All
          </Button>
        </Space>
      </div>

      {/* Main Content */}
      <Tabs
        activeKey={activeTab}
        onChange={setActiveTab}
        size="large"
      >
        <TabPane 
          tab={
            <Space>
              <SearchOutlined />
              <span>Trace Analysis</span>
            </Space>
          } 
          key="traces"
        >
          {renderTracesTab()}
        </TabPane>
        
        <TabPane 
          tab={
            <Space>
              <ClockCircleOutlined />
              <span>Live Tracking</span>
            </Space>
          } 
          key="live"
        >
          {renderLiveTrackingTab()}
        </TabPane>
        
        <TabPane 
          tab={
            <Space>
              <UserOutlined />
              <span>User Feedback</span>
            </Space>
          } 
          key="feedback"
        >
          {renderFeedbackTab()}
        </TabPane>
      </Tabs>

      {/* Trace Details Drawer */}
      <Drawer
        title="Trace Details"
        placement="right"
        width="80%"
        onClose={() => setDrawerVisible(false)}
        open={drawerVisible}
      >
        {selectedTrace && traceDetails && (
          <TraceDetailViewer
            trace={traceDetails}
            showAllTabs={true}
            onExport={(trace) => console.log('Export trace:', trace)}
            onShare={(trace) => console.log('Share trace:', trace)}
          />
        )}
      </Drawer>
    </div>
  )
}

export default TransparencyReviewPage
