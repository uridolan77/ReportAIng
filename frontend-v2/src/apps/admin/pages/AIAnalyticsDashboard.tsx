import React, { useState } from 'react'
import { 
  Card, 
  Row, 
  Col, 
  Statistic, 
  Typography, 
  Space, 
  Select, 
  DatePicker, 
  Button,
  Table,
  Tag,
  Progress,
  Alert,
  Tabs,
  List,
  Tooltip
} from 'antd'
import {
  BarChartOutlined,
  LineChartOutlined,
  PieChartOutlined,
  TrophyOutlined,
  ExclamationCircleOutlined,
  RobotOutlined,
  ThunderboltOutlined,
  DollarOutlined,
  ClockCircleOutlined,
  ReloadOutlined,
  DownloadOutlined,
  WarningOutlined,
  ToolOutlined,
  WalletOutlined,
  EyeOutlined,
  ApiOutlined
} from '@ant-design/icons'
import { LineChart, Line, BarChart, Bar, PieChart, Pie, Cell, XAxis, YAxis, CartesianGrid, Tooltip as RechartsTooltip, ResponsiveContainer, AreaChart, Area } from 'recharts'
import { useGetProcessFlowDashboardQuery, useGetProcessFlowAnalyticsQuery } from '@shared/store/api/transparencyApi'
import { ProcessFlowDashboard, ProcessFlowAnalytics } from '@shared/components/ai/transparency'
import { useGetStreamingAnalyticsQuery } from '@shared/store/api/aiStreamingApi'
import { useGetAgentAnalyticsQuery } from '@shared/store/api/intelligentAgentsApi'
import {
  usePerformanceMonitoring,
  usePerformanceAlerts,
  usePerformanceBenchmarks
} from '@shared/hooks/usePerformanceMonitoring'
import { useCostAlerts, useCostMetrics } from '@shared/hooks/useCostMetrics'
import type { PerformanceEntityType } from '@shared/types/performance'
import { CostDashboard } from '@shared/components/cost/CostDashboard'
import { BudgetManagementComponent } from '@shared/components/cost/BudgetManagement'
import dayjs from 'dayjs'

const { Title, Text } = Typography
const { RangePicker } = DatePicker

/**
 * AIAnalyticsDashboard - Main AI analytics dashboard
 * 
 * Features:
 * - Real-time AI usage statistics
 * - Query pattern analysis
 * - Performance metrics and trends
 * - Cost optimization insights
 * - Agent performance monitoring
 * - Transparency analytics
 */
export const AIAnalyticsDashboard: React.FC = () => {
  const [timeRange, setTimeRange] = useState(30) // days
  const [dateRange, setDateRange] = useState<[dayjs.Dayjs, dayjs.Dayjs] | null>(null)
  const [activeTab, setActiveTab] = useState('overview')
  const [selectedEntity, setSelectedEntity] = useState<{type: PerformanceEntityType, id: string}>({
    type: 'system',
    id: 'global'
  })

  // API queries
  const { data: processFlowDashboard, isLoading: dashboardLoading, refetch: refetchDashboard } =
    useGetProcessFlowDashboardQuery({ days: timeRange, includeDetails: true })

  const { data: processFlowAnalytics, isLoading: analyticsLoading, refetch: refetchAnalytics } =
    useGetProcessFlowAnalyticsQuery({
      days: timeRange,
      includeStepDetails: true,
      includeTokenUsage: true,
      includePerformanceMetrics: true
    })
  
  const { data: streamingAnalytics, isLoading: streamingLoading, refetch: refetchStreaming } = 
    useGetStreamingAnalyticsQuery({ days: timeRange })
  
  const { data: agentAnalytics, isLoading: agentLoading, refetch: refetchAgents } =
    useGetAgentAnalyticsQuery({ days: timeRange })

  // Performance monitoring
  const {
    metrics: performanceMetrics,
    bottlenecks,
    suggestions,
    autoTune,
    autoTuneLoading,
    isLoading: performanceLoading
  } = usePerformanceMonitoring(selectedEntity.type, selectedEntity.id)

  const { alerts: performanceAlerts, criticalAlerts, highAlerts, totalCount } = usePerformanceAlerts()
  const { benchmarks, overallScore, passingCount, warningCount, failingCount } = usePerformanceBenchmarks()

  // Cost management
  const { analytics: costAnalytics, recommendations: costRecommendations, forecast } = useCostMetrics('30d')
  const { alerts: costAlerts, criticalCount: costCriticalCount, highCount: costHighCount } = useCostAlerts()

  const isLoading = dashboardLoading || analyticsLoading || streamingLoading || agentLoading || performanceLoading

  // Handle refresh
  const handleRefresh = () => {
    refetchDashboard()
    refetchAnalytics()
    refetchStreaming()
    refetchAgents()
  }

  // Handle export
  const handleExport = () => {
    // TODO: Implement export functionality
    console.log('Exporting analytics data...')
  }

  // Mock data for charts (replace with real data)
  const usageTrendData = [
    { date: '2024-01-01', queries: 120, insights: 45, cost: 25.50 },
    { date: '2024-01-02', queries: 135, insights: 52, cost: 28.75 },
    { date: '2024-01-03', queries: 98, insights: 38, cost: 21.20 },
    { date: '2024-01-04', queries: 156, insights: 61, cost: 34.80 },
    { date: '2024-01-05', queries: 142, insights: 55, cost: 31.25 },
    { date: '2024-01-06', queries: 178, insights: 68, cost: 39.60 },
    { date: '2024-01-07', queries: 165, insights: 63, cost: 36.45 }
  ]

  const modelUsageData = [
    { name: 'GPT-4', value: 45, cost: 125.50, color: '#8884d8' },
    { name: 'GPT-3.5', value: 30, cost: 67.25, color: '#82ca9d' },
    { name: 'Claude-3', value: 15, cost: 89.75, color: '#ffc658' },
    { name: 'Gemini', value: 10, cost: 34.20, color: '#ff7c7c' }
  ]

  const performanceData = [
    { metric: 'Avg Response Time', value: '1.2s', trend: 'down', color: 'green' },
    { metric: 'Success Rate', value: '94.5%', trend: 'up', color: 'green' },
    { metric: 'Error Rate', value: '2.1%', trend: 'down', color: 'green' },
    { metric: 'Confidence Score', value: '87.3%', trend: 'up', color: 'blue' }
  ]

  const topQueriesData = [
    { query: 'Show me sales data for Q4', count: 45, avgConfidence: 0.92 },
    { query: 'What are the top products by revenue?', count: 38, avgConfidence: 0.89 },
    { query: 'Compare this year vs last year', count: 32, avgConfidence: 0.85 },
    { query: 'Show customer demographics', count: 28, avgConfidence: 0.91 },
    { query: 'Analyze profit margins by category', count: 24, avgConfidence: 0.87 }
  ]

  const renderOverviewTab = () => (
    <div>
      {/* Key Metrics */}
      <Row gutter={[16, 16]} style={{ marginBottom: 24 }}>
        <Col xs={24} sm={12} lg={6}>
          <Card>
            <Statistic
              title="Total Sessions"
              value={processFlowDashboard?.totalSessions || 1247}
              prefix={<RobotOutlined />}
              valueStyle={{ color: '#3f8600' }}
            />
          </Card>
        </Col>
        <Col xs={24} sm={12} lg={6}>
          <Card>
            <Statistic
              title="Success Rate"
              value={processFlowDashboard?.successRate?.toFixed(1) || '87.3'}
              suffix="%"
              prefix={<TrophyOutlined />}
              valueStyle={{ color: '#1890ff' }}
            />
          </Card>
        </Col>
        <Col xs={24} sm={12} lg={6}>
          <Card>
            <Statistic
              title="Active Agents"
              value={agentAnalytics?.agentUtilization?.length || 8}
              prefix={<ThunderboltOutlined />}
              valueStyle={{ color: '#722ed1' }}
            />
          </Card>
        </Col>
        <Col xs={24} sm={12} lg={6}>
          <Card>
            <Statistic
              title="Total Cost"
              value={processFlowAnalytics?.totalCost || 456.78}
              prefix={<DollarOutlined />}
              precision={4}
              valueStyle={{ color: '#cf1322' }}
            />
          </Card>
        </Col>
      </Row>

      {/* Usage Trends */}
      <Row gutter={[16, 16]} style={{ marginBottom: 24 }}>
        <Col xs={24} lg={16}>
          <Card title="Usage Trends" extra={
            <Space>
              <Select value={timeRange} onChange={setTimeRange} style={{ width: 120 }}>
                <Select.Option value={7}>Last 7 days</Select.Option>
                <Select.Option value={30}>Last 30 days</Select.Option>
                <Select.Option value={90}>Last 90 days</Select.Option>
              </Select>
            </Space>
          }>
            <ResponsiveContainer width="100%" height={300}>
              <AreaChart data={usageTrendData}>
                <CartesianGrid strokeDasharray="3 3" />
                <XAxis dataKey="date" />
                <YAxis />
                <RechartsTooltip />
                <Area type="monotone" dataKey="queries" stackId="1" stroke="#8884d8" fill="#8884d8" />
                <Area type="monotone" dataKey="insights" stackId="1" stroke="#82ca9d" fill="#82ca9d" />
              </AreaChart>
            </ResponsiveContainer>
          </Card>
        </Col>
        <Col xs={24} lg={8}>
          <Card title="Model Usage Distribution">
            <ResponsiveContainer width="100%" height={300}>
              <PieChart>
                <Pie
                  data={modelUsageData}
                  cx="50%"
                  cy="50%"
                  labelLine={false}
                  label={({ name, percent }) => `${name} ${(percent * 100).toFixed(0)}%`}
                  outerRadius={80}
                  fill="#8884d8"
                  dataKey="value"
                >
                  {modelUsageData.map((entry, index) => (
                    <Cell key={`cell-${index}`} fill={entry.color} />
                  ))}
                </Pie>
                <RechartsTooltip />
              </PieChart>
            </ResponsiveContainer>
          </Card>
        </Col>
      </Row>

      {/* Performance Metrics */}
      <Row gutter={[16, 16]}>
        <Col xs={24} lg={12}>
          <Card title="Performance Metrics">
            <Space direction="vertical" style={{ width: '100%' }}>
              {performanceData.map((metric, index) => (
                <div key={index} style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                  <Text strong>{metric.metric}</Text>
                  <Space>
                    <Text style={{ color: metric.color }}>{metric.value}</Text>
                    <Tag color={metric.trend === 'up' ? 'green' : 'red'}>
                      {metric.trend === 'up' ? '↑' : '↓'}
                    </Tag>
                  </Space>
                </div>
              ))}
            </Space>
          </Card>
        </Col>
        <Col xs={24} lg={12}>
          <Card title="Top Queries">
            <List
              size="small"
              dataSource={topQueriesData}
              renderItem={(item, index) => (
                <List.Item>
                  <div style={{ width: '100%' }}>
                    <div style={{ display: 'flex', justifyContent: 'space-between', marginBottom: 4 }}>
                      <Text ellipsis style={{ maxWidth: '60%' }}>{item.query}</Text>
                      <Space>
                        <Tag color="blue">{item.count}</Tag>
                        <Progress 
                          percent={item.avgConfidence * 100} 
                          size="small" 
                          style={{ width: 60 }}
                          showInfo={false}
                        />
                      </Space>
                    </div>
                  </div>
                </List.Item>
              )}
            />
          </Card>
        </Col>
      </Row>
    </div>
  )

  const renderPerformanceTab = () => (
    <div>
      <Alert
        message="Performance Analytics"
        description="Detailed performance metrics and optimization recommendations."
        type="info"
        showIcon
        style={{ marginBottom: 16 }}
      />
      {/* Performance content will be added here */}
      <Card title="Response Time Trends">
        <ResponsiveContainer width="100%" height={400}>
          <LineChart data={usageTrendData}>
            <CartesianGrid strokeDasharray="3 3" />
            <XAxis dataKey="date" />
            <YAxis />
            <RechartsTooltip />
            <Line type="monotone" dataKey="queries" stroke="#8884d8" strokeWidth={2} />
          </LineChart>
        </ResponsiveContainer>
      </Card>
    </div>
  )

  const renderCostTab = () => (
    <div>
      <Alert
        message="Cost Analytics"
        description="AI usage costs, optimization opportunities, and budget tracking."
        type="warning"
        showIcon
        style={{ marginBottom: 16 }}
      />
      {/* Cost content will be added here */}
      <Row gutter={[16, 16]}>
        <Col span={12}>
          <Card title="Cost by Model">
            <ResponsiveContainer width="100%" height={300}>
              <BarChart data={modelUsageData}>
                <CartesianGrid strokeDasharray="3 3" />
                <XAxis dataKey="name" />
                <YAxis />
                <RechartsTooltip />
                <Bar dataKey="cost" fill="#8884d8" />
              </BarChart>
            </ResponsiveContainer>
          </Card>
        </Col>
        <Col span={12}>
          <Card title="Cost Optimization">
            <List
              dataSource={[
                { title: 'Switch to GPT-3.5 for simple queries', savings: '$45.20/month' },
                { title: 'Implement response caching', savings: '$32.15/month' },
                { title: 'Optimize prompt length', savings: '$18.90/month' }
              ]}
              renderItem={item => (
                <List.Item>
                  <List.Item.Meta
                    title={item.title}
                    description={`Potential savings: ${item.savings}`}
                  />
                </List.Item>
              )}
            />
          </Card>
        </Col>
      </Row>
    </div>
  )

  // Performance Overview Tab
  const renderPerformanceOverviewTab = () => {
    const handleAutoTune = async () => {
      try {
        await autoTune()
        // message.success('Auto-tuning completed successfully')
      } catch (error) {
        // message.error('Auto-tuning failed')
      }
    }

    const getScoreColor = (score: number) => {
      if (score >= 80) return '#52c41a'
      if (score >= 60) return '#faad14'
      return '#f5222d'
    }

    return (
      <div>
        {/* Entity Selection */}
        <Card style={{ marginBottom: '16px' }}>
          <Space>
            <Text>Monitor Entity:</Text>
            <Select
              style={{ width: 200 }}
              value={`${selectedEntity.type}:${selectedEntity.id}`}
              onChange={(value) => {
                const [type, id] = value.split(':')
                setSelectedEntity({ type: type as PerformanceEntityType, id })
              }}
            >
              <Select.Option value="system:global">System (Global)</Select.Option>
              <Select.Option value="database:main">Database (Main)</Select.Option>
              <Select.Option value="api:v1">API (v1)</Select.Option>
            </Select>
            <Button
              type="primary"
              icon={<ToolOutlined />}
              loading={autoTuneLoading}
              onClick={handleAutoTune}
            >
              Auto-Tune Performance
            </Button>
          </Space>
        </Card>

        {/* Performance Metrics */}
        <Row gutter={[16, 16]} style={{ marginBottom: '16px' }}>
          <Col xs={24} sm={12} lg={6}>
            <Card>
              <Statistic
                title="Performance Score"
                value={performanceMetrics?.metrics.performanceScore || 0}
                suffix="/100"
                valueStyle={{ color: getScoreColor(performanceMetrics?.metrics.performanceScore || 0) }}
                prefix={<TrophyOutlined />}
              />
            </Card>
          </Col>
          <Col xs={24} sm={12} lg={6}>
            <Card>
              <Statistic
                title="Avg Response Time"
                value={performanceMetrics?.metrics.averageResponseTime || 0}
                suffix="ms"
                precision={2}
              />
            </Card>
          </Col>
          <Col xs={24} sm={12} lg={6}>
            <Card>
              <Statistic
                title="Throughput"
                value={performanceMetrics?.metrics.throughputPerSecond || 0}
                suffix="req/s"
                precision={1}
              />
            </Card>
          </Col>
          <Col xs={24} sm={12} lg={6}>
            <Card>
              <Statistic
                title="Error Rate"
                value={(performanceMetrics?.metrics.errorRate || 0) * 100}
                suffix="%"
                precision={2}
                valueStyle={{
                  color: (performanceMetrics?.metrics.errorRate || 0) > 0.05 ? '#f5222d' : '#52c41a'
                }}
              />
            </Card>
          </Col>
        </Row>

        {/* Bottlenecks and Suggestions */}
        <Row gutter={[16, 16]}>
          <Col xs={24} lg={12}>
            <Card title="Performance Bottlenecks" loading={isLoading}>
              {bottlenecks && bottlenecks.bottlenecks.length > 0 ? (
                <div>
                  {bottlenecks.bottlenecks.slice(0, 3).map((bottleneck) => (
                    <Alert
                      key={bottleneck.id}
                      message={bottleneck.description}
                      description={`Impact Score: ${bottleneck.impactScore}/10`}
                      type={bottleneck.severity === 'Critical' ? 'error' : 'warning'}
                      showIcon
                      style={{ marginBottom: '8px' }}
                    />
                  ))}
                </div>
              ) : (
                <Text type="secondary">No bottlenecks detected</Text>
              )}
            </Card>
          </Col>

          <Col xs={24} lg={12}>
            <Card title="Optimization Suggestions" loading={isLoading}>
              {suggestions && suggestions.suggestions.length > 0 ? (
                <div>
                  {suggestions.suggestions.slice(0, 3).map((suggestion) => (
                    <Alert
                      key={suggestion.id}
                      message={suggestion.title}
                      description={`${suggestion.description} - Expected improvement: +${suggestion.estimatedImprovement}%`}
                      type="info"
                      showIcon
                      style={{ marginBottom: '8px' }}
                    />
                  ))}
                </div>
              ) : (
                <Text type="secondary">No optimization suggestions available</Text>
              )}
            </Card>
          </Col>
        </Row>
      </div>
    )
  }

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
      key: 'performance',
      label: (
        <Space>
          <LineChartOutlined />
          <span>Performance</span>
        </Space>
      ),
      children: renderPerformanceTab()
    },
    {
      key: 'cost',
      label: (
        <Space>
          <DollarOutlined />
          <span>Cost Analysis</span>
        </Space>
      ),
      children: renderCostTab()
    },
    {
      key: 'performance-overview',
      label: (
        <Space>
          <ThunderboltOutlined />
          <span>Performance</span>
        </Space>
      ),
      children: renderPerformanceOverviewTab()
    },
    {
      key: 'performance-alerts',
      label: (
        <Space>
          <WarningOutlined />
          <span>Alerts</span>
          {(criticalAlerts.length + highAlerts.length + costCriticalCount + costHighCount) > 0 && (
            <Tag color="red" style={{ marginLeft: 4 }}>
              {criticalAlerts.length + highAlerts.length + costCriticalCount + costHighCount}
            </Tag>
          )}
        </Space>
      ),
      children: (
        <div>
          <Row gutter={[16, 16]} style={{ marginBottom: '16px' }}>
            <Col xs={24} sm={8}>
              <Card>
                <Statistic
                  title="Total Alerts"
                  value={totalCount + (costAlerts?.length || 0)}
                  prefix={<WarningOutlined />}
                />
              </Card>
            </Col>
            <Col xs={24} sm={8}>
              <Card>
                <Statistic
                  title="Critical Alerts"
                  value={criticalAlerts.length + costCriticalCount}
                  valueStyle={{ color: '#f5222d' }}
                  prefix={<WarningOutlined />}
                />
              </Card>
            </Col>
            <Col xs={24} sm={8}>
              <Card>
                <Statistic
                  title="High Priority"
                  value={highAlerts.length + costHighCount}
                  valueStyle={{ color: '#faad14' }}
                  prefix={<WarningOutlined />}
                />
              </Card>
            </Col>
          </Row>

          <Card title="Recent Performance Alerts">
            <div>
              {criticalAlerts.slice(0, 3).map((alert) => (
                <Alert
                  key={alert.id}
                  message={alert.title}
                  description={alert.description}
                  type="error"
                  showIcon
                  style={{ marginBottom: '8px' }}
                />
              ))}
              {highAlerts.slice(0, 2).map((alert) => (
                <Alert
                  key={alert.id}
                  message={alert.title}
                  description={alert.description}
                  type="warning"
                  showIcon
                  style={{ marginBottom: '8px' }}
                />
              ))}
              {(criticalAlerts.length === 0 && highAlerts.length === 0) && (
                <Text type="secondary">No active performance alerts</Text>
              )}
            </div>
          </Card>
        </div>
      )
    },
    {
      key: 'cost-dashboard',
      label: (
        <Space>
          <WalletOutlined />
          <span>Cost Dashboard</span>
        </Space>
      ),
      children: <CostDashboard />
    },
    {
      key: 'budget-management',
      label: (
        <Space>
          <DollarOutlined />
          <span>Budget Management</span>
        </Space>
      ),
      children: <BudgetManagementComponent />
    },
    {
      key: 'processflow-dashboard',
      label: (
        <Space>
          <EyeOutlined />
          <span>ProcessFlow Dashboard</span>
        </Space>
      ),
      children: (
        <ProcessFlowDashboard
          filters={{ days: timeRange, includeDetails: true }}
          showCharts={true}
          showMetrics={true}
        />
      )
    },
    {
      key: 'processflow-analytics',
      label: (
        <Space>
          <ApiOutlined />
          <span>ProcessFlow Analytics</span>
        </Space>
      ),
      children: (
        <ProcessFlowAnalytics
          defaultFilters={{ days: timeRange, includeStepDetails: true, includeTokenUsage: true }}
          showExport={true}
        />
      )
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
              <BarChartOutlined />
              AI Analytics Dashboard
            </Space>
          </Title>
          <Text type="secondary">
            Comprehensive AI usage analytics, performance metrics, and optimization insights
          </Text>
        </div>
        <Space>
          <Tooltip title="Refresh data">
            <Button 
              icon={<ReloadOutlined />} 
              loading={isLoading}
              onClick={handleRefresh}
            >
              Refresh
            </Button>
          </Tooltip>
          <Button 
            icon={<DownloadOutlined />}
            onClick={handleExport}
          >
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
  )
}

export default AIAnalyticsDashboard
