import React, { useState } from 'react'
import { 
  Card, 
  Tabs, 
  Row, 
  Col, 
  Select, 
  DatePicker, 
  Space, 
  Typography, 
  Button,
  Alert,
  Spin
} from 'antd'
import {
  BarChartOutlined,
  LineChartOutlined,
  PieChartOutlined,
  DownloadOutlined,
  FilterOutlined
} from '@ant-design/icons'
import { LineChart, Line, BarChart, Bar, PieChart, Pie, Cell, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer, Legend } from 'recharts'
import dayjs from 'dayjs'
import { useGetProcessFlowAnalyticsQuery, useExportProcessFlowDataMutation } from '@shared/store/api/transparencyApi'
import type { ProcessFlowAnalyticsRequest } from '@shared/types/transparency'

const { Title, Text } = Typography
const { TabPane } = Tabs
const { RangePicker } = DatePicker

export interface ProcessFlowAnalyticsProps {
  defaultFilters?: Partial<ProcessFlowAnalyticsRequest>
  showExport?: boolean
  className?: string
  testId?: string
}

/**
 * ProcessFlowAnalytics - Comprehensive analytics for ProcessFlow data
 * 
 * Features:
 * - Token usage analytics
 * - Performance metrics
 * - Cost analysis
 * - Step-by-step breakdown
 * - Export functionality
 */
export const ProcessFlowAnalytics: React.FC<ProcessFlowAnalyticsProps> = ({
  defaultFilters = {},
  showExport = true,
  className,
  testId = 'processflow-analytics'
}) => {
  const [filters, setFilters] = useState<ProcessFlowAnalyticsRequest>({
    includeStepDetails: true,
    includeTokenUsage: true,
    includePerformanceMetrics: true,
    days: 7,
    ...defaultFilters
  })

  const { data: analytics, isLoading, error, refetch } = useGetProcessFlowAnalyticsQuery(filters)
  const [exportData] = useExportProcessFlowDataMutation()

  const handleFilterChange = (key: string, value: any) => {
    setFilters(prev => ({ ...prev, [key]: value }))
  }

  const handleDateRangeChange = (dates: any) => {
    if (dates && dates.length === 2) {
      setFilters(prev => ({
        ...prev,
        startDate: dates[0].format('YYYY-MM-DD'),
        endDate: dates[1].format('YYYY-MM-DD'),
        days: undefined
      }))
    } else {
      setFilters(prev => ({
        ...prev,
        startDate: undefined,
        endDate: undefined,
        days: 7
      }))
    }
  }

  const handleExport = async () => {
    try {
      const blob = await exportData({
        format: 'excel',
        filters,
        includeCharts: true
      }).unwrap()
      
      const url = window.URL.createObjectURL(blob)
      const a = document.createElement('a')
      a.href = url
      a.download = `processflow-analytics-${dayjs().format('YYYY-MM-DD')}.xlsx`
      document.body.appendChild(a)
      a.click()
      window.URL.revokeObjectURL(url)
      document.body.removeChild(a)
    } catch (error) {
      console.error('Export failed:', error)
    }
  }

  if (isLoading) {
    return (
      <div style={{ textAlign: 'center', padding: '40px' }}>
        <Spin size="large" />
        <div style={{ marginTop: 16 }}>
          <Text type="secondary">Loading ProcessFlow analytics...</Text>
        </div>
      </div>
    )
  }

  if (error) {
    return (
      <Alert
        message="Error Loading Analytics"
        description="Failed to load ProcessFlow analytics data"
        type="error"
        showIcon
      />
    )
  }

  if (!analytics) {
    return (
      <Alert
        message="No Data Available"
        description="No ProcessFlow analytics data available for the selected filters"
        type="info"
        showIcon
      />
    )
  }

  const COLORS = ['#1890ff', '#52c41a', '#faad14', '#ff4d4f', '#722ed1', '#13c2c2']

  return (
    <div className={className} data-testid={testId}>
      <Space direction="vertical" style={{ width: '100%' }} size="large">
        {/* Header with Filters */}
        <Card>
          <Row justify="space-between" align="middle">
            <Col>
              <Title level={3} style={{ margin: 0 }}>
                <BarChartOutlined /> ProcessFlow Analytics
              </Title>
            </Col>
            <Col>
              <Space>
                <Select
                  value={filters.days}
                  onChange={(value) => handleFilterChange('days', value)}
                  style={{ width: 120 }}
                >
                  <Select.Option value={1}>Last Day</Select.Option>
                  <Select.Option value={7}>Last Week</Select.Option>
                  <Select.Option value={30}>Last Month</Select.Option>
                  <Select.Option value={90}>Last 3 Months</Select.Option>
                </Select>
                <RangePicker
                  onChange={handleDateRangeChange}
                  format="YYYY-MM-DD"
                />
                {showExport && (
                  <Button 
                    icon={<DownloadOutlined />} 
                    onClick={handleExport}
                  >
                    Export
                  </Button>
                )}
              </Space>
            </Col>
          </Row>
        </Card>

        {/* Analytics Tabs */}
        <Tabs defaultActiveKey="overview">
          <TabPane tab="Overview" key="overview">
            <Row gutter={[16, 16]}>
              <Col xs={24} lg={12}>
                <Card title="Token Usage Trends">
                  <ResponsiveContainer width="100%" height={300}>
                    <LineChart data={analytics.dailyMetrics}>
                      <CartesianGrid strokeDasharray="3 3" />
                      <XAxis dataKey="date" />
                      <YAxis />
                      <Tooltip />
                      <Legend />
                      <Line 
                        type="monotone" 
                        dataKey="totalTokens" 
                        stroke="#1890ff" 
                        name="Total Tokens"
                      />
                      <Line 
                        type="monotone" 
                        dataKey="promptTokens" 
                        stroke="#52c41a" 
                        name="Prompt Tokens"
                      />
                      <Line 
                        type="monotone" 
                        dataKey="completionTokens" 
                        stroke="#faad14" 
                        name="Completion Tokens"
                      />
                    </LineChart>
                  </ResponsiveContainer>
                </Card>
              </Col>
              <Col xs={24} lg={12}>
                <Card title="Cost Analysis">
                  <ResponsiveContainer width="100%" height={300}>
                    <BarChart data={analytics.dailyMetrics}>
                      <CartesianGrid strokeDasharray="3 3" />
                      <XAxis dataKey="date" />
                      <YAxis />
                      <Tooltip />
                      <Bar dataKey="totalCost" fill="#ff4d4f" name="Daily Cost" />
                    </BarChart>
                  </ResponsiveContainer>
                </Card>
              </Col>
            </Row>
          </TabPane>

          <TabPane tab="Performance" key="performance">
            <Row gutter={[16, 16]}>
              <Col xs={24} lg={12}>
                <Card title="Processing Time Distribution">
                  <ResponsiveContainer width="100%" height={300}>
                    <BarChart data={analytics.stepMetrics}>
                      <CartesianGrid strokeDasharray="3 3" />
                      <XAxis dataKey="stepType" />
                      <YAxis />
                      <Tooltip />
                      <Bar dataKey="averageDuration" fill="#1890ff" name="Avg Duration (ms)" />
                    </BarChart>
                  </ResponsiveContainer>
                </Card>
              </Col>
              <Col xs={24} lg={12}>
                <Card title="Confidence Distribution">
                  <ResponsiveContainer width="100%" height={300}>
                    <PieChart>
                      <Pie
                        data={analytics.confidenceDistribution}
                        cx="50%"
                        cy="50%"
                        outerRadius={80}
                        fill="#8884d8"
                        dataKey="count"
                        label={({ name, value }) => `${name}: ${value}`}
                      >
                        {analytics.confidenceDistribution?.map((entry, index) => (
                          <Cell key={`cell-${index}`} fill={COLORS[index % COLORS.length]} />
                        ))}
                      </Pie>
                      <Tooltip />
                      <Legend />
                    </PieChart>
                  </ResponsiveContainer>
                </Card>
              </Col>
            </Row>
          </TabPane>

          <TabPane tab="Step Analysis" key="steps">
            <Card title="Step Performance Breakdown">
              <ResponsiveContainer width="100%" height={400}>
                <BarChart data={analytics.stepMetrics} layout="horizontal">
                  <CartesianGrid strokeDasharray="3 3" />
                  <XAxis type="number" />
                  <YAxis dataKey="stepType" type="category" width={150} />
                  <Tooltip />
                  <Legend />
                  <Bar dataKey="averageDuration" fill="#1890ff" name="Avg Duration (ms)" />
                  <Bar dataKey="averageConfidence" fill="#52c41a" name="Avg Confidence (%)" />
                </BarChart>
              </ResponsiveContainer>
            </Card>
          </TabPane>
        </Tabs>
      </Space>
    </div>
  )
}

export default ProcessFlowAnalytics
