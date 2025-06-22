import React, { useState, useCallback, useMemo, useEffect } from 'react'
import {
  Card,
  Empty,
  Typography,
  Space,
  Row,
  Col,
  Statistic,
  Progress,
  Alert,
  List,
  Avatar,
  Tag,
  Button,
  Tooltip,
  Timeline,
  Tabs,
  Badge,
  Rate,
  Divider,
  Table,
  message
} from 'antd'
import {
  ThunderboltOutlined,
  ClockCircleOutlined,
  TrophyOutlined,
  WarningOutlined,
  CheckCircleOutlined,
  InfoCircleOutlined,
  RocketOutlined,
  DashboardOutlined,
  LineChartOutlined,
  BarChartOutlined,
  PieChartOutlined,
  ReloadOutlined,
  SettingOutlined,
  BulbOutlined,
  FireOutlined,
  StarOutlined,
  ExclamationCircleOutlined
} from '@ant-design/icons'

const { Text, Title, Paragraph } = Typography
const { TabPane } = Tabs

interface PerformanceMetric {
  id: string
  name: string
  value: number
  unit: string
  trend: 'up' | 'down' | 'stable'
  trendValue: number
  status: 'excellent' | 'good' | 'warning' | 'critical'
  description: string
  target?: number
}

interface OptimizationSuggestion {
  id: string
  type: 'performance' | 'accuracy' | 'efficiency' | 'cost'
  title: string
  description: string
  impact: 'high' | 'medium' | 'low'
  effort: 'high' | 'medium' | 'low'
  estimatedImprovement: string
  priority: number
}

interface UsageAnalytics {
  totalQueries: number
  averageResponseTime: number
  successRate: number
  popularQueries: string[]
  peakUsageHours: number[]
  userSatisfaction: number
}

interface PerformanceMetricsPanelProps {
  query: string
  analysisResults?: any
  showOptimizations?: boolean
  showUsageAnalytics?: boolean
  showRealTimeMetrics?: boolean
  interactive?: boolean
  onOptimizationApply?: (suggestionId: string) => void
  onMetricRefresh?: () => void
}

/**
 * PerformanceMetricsPanel - Comprehensive performance analytics and optimization
 *
 * Features:
 * - Real-time performance metrics with trend analysis
 * - Optimization suggestions with impact assessment
 * - Usage analytics and user behavior insights
 * - System health monitoring and alerts
 * - Interactive performance tuning recommendations
 * - Historical performance tracking and comparison
 */
export const PerformanceMetricsPanel: React.FC<PerformanceMetricsPanelProps> = ({
  query,
  analysisResults,
  showOptimizations = true,
  showUsageAnalytics = true,
  showRealTimeMetrics = true,
  interactive = true,
  onOptimizationApply,
  onMetricRefresh
}) => {
  const [activeTab, setActiveTab] = useState('metrics')
  const [refreshing, setRefreshing] = useState(false)
  const [selectedOptimization, setSelectedOptimization] = useState<string | null>(null)

  // Mock data for comprehensive performance analytics - will be replaced with real API data
  const mockPerformanceData = useMemo(() => ({
    metrics: [
      {
        id: 'response-time',
        name: 'Average Response Time',
        value: 1.2,
        unit: 'seconds',
        trend: 'down' as const,
        trendValue: -15,
        status: 'good' as const,
        description: 'Time taken to analyze and respond to queries',
        target: 1.0
      },
      {
        id: 'accuracy',
        name: 'Analysis Accuracy',
        value: 94.5,
        unit: '%',
        trend: 'up' as const,
        trendValue: 2.3,
        status: 'excellent' as const,
        description: 'Accuracy of entity detection and intent classification',
        target: 95.0
      },
      {
        id: 'throughput',
        name: 'Query Throughput',
        value: 156,
        unit: 'queries/hour',
        trend: 'up' as const,
        trendValue: 8.2,
        status: 'good' as const,
        description: 'Number of queries processed per hour',
        target: 200
      },
      {
        id: 'cache-hit',
        name: 'Cache Hit Rate',
        value: 78.3,
        unit: '%',
        trend: 'stable' as const,
        trendValue: 0.5,
        status: 'good' as const,
        description: 'Percentage of queries served from cache',
        target: 85.0
      },
      {
        id: 'error-rate',
        name: 'Error Rate',
        value: 2.1,
        unit: '%',
        trend: 'down' as const,
        trendValue: -0.8,
        status: 'good' as const,
        description: 'Percentage of queries that resulted in errors',
        target: 1.0
      },
      {
        id: 'confidence',
        name: 'Average Confidence',
        value: 87.6,
        unit: '%',
        trend: 'up' as const,
        trendValue: 1.4,
        status: 'good' as const,
        description: 'Average confidence score of analysis results',
        target: 90.0
      }
    ],
    optimizations: [
      {
        id: 'opt-1',
        type: 'performance' as const,
        title: 'Enable Query Result Caching',
        description: 'Implement intelligent caching for frequently used query patterns to reduce response time',
        impact: 'high' as const,
        effort: 'medium' as const,
        estimatedImprovement: '40% faster response time',
        priority: 1
      },
      {
        id: 'opt-2',
        type: 'accuracy' as const,
        title: 'Enhance Entity Recognition Model',
        description: 'Update the NLP model with domain-specific training data to improve entity detection accuracy',
        impact: 'high' as const,
        effort: 'high' as const,
        estimatedImprovement: '5% accuracy increase',
        priority: 2
      },
      {
        id: 'opt-3',
        type: 'efficiency' as const,
        title: 'Optimize Database Queries',
        description: 'Add database indexes and optimize query execution plans for better performance',
        impact: 'medium' as const,
        effort: 'low' as const,
        estimatedImprovement: '25% faster database operations',
        priority: 3
      },
      {
        id: 'opt-4',
        type: 'cost' as const,
        title: 'Implement Request Batching',
        description: 'Batch multiple analysis requests to reduce API calls and improve resource utilization',
        impact: 'medium' as const,
        effort: 'medium' as const,
        estimatedImprovement: '30% cost reduction',
        priority: 4
      }
    ],
    usageAnalytics: {
      totalQueries: 2847,
      averageResponseTime: 1.2,
      successRate: 97.9,
      popularQueries: [
        'Show me sales by region',
        'What are the top customers',
        'Revenue trends last quarter',
        'Product performance analysis'
      ],
      peakUsageHours: [9, 10, 11, 14, 15, 16],
      userSatisfaction: 4.3
    }
  }), [])

  const handleOptimizationApply = useCallback((suggestionId: string) => {
    setSelectedOptimization(suggestionId)
    if (onOptimizationApply) {
      onOptimizationApply(suggestionId)
    }
    message.success('Optimization suggestion applied successfully')
  }, [onOptimizationApply])

  const handleMetricRefresh = useCallback(async () => {
    setRefreshing(true)
    if (onMetricRefresh) {
      onMetricRefresh()
    }
    // Simulate refresh delay
    setTimeout(() => {
      setRefreshing(false)
      message.success('Performance metrics refreshed')
    }, 1000)
  }, [onMetricRefresh])

  // Utility functions
  const getMetricStatusColor = (status: string) => {
    const colors: Record<string, string> = {
      'excellent': '#52c41a',
      'good': '#1890ff',
      'warning': '#fa8c16',
      'critical': '#ff4d4f'
    }
    return colors[status] || '#d9d9d9'
  }

  const getTrendIcon = (trend: string) => {
    const icons: Record<string, React.ReactNode> = {
      'up': <RocketOutlined style={{ color: '#52c41a' }} />,
      'down': <RocketOutlined style={{ color: '#ff4d4f', transform: 'rotate(180deg)' }} />,
      'stable': <LineChartOutlined style={{ color: '#1890ff' }} />
    }
    return icons[trend] || <LineChartOutlined />
  }

  const getImpactColor = (impact: string) => {
    const colors: Record<string, string> = {
      'high': '#ff4d4f',
      'medium': '#fa8c16',
      'low': '#52c41a'
    }
    return colors[impact] || '#d9d9d9'
  }

  const getOptimizationTypeIcon = (type: string) => {
    const icons: Record<string, React.ReactNode> = {
      'performance': <ThunderboltOutlined style={{ color: '#1890ff' }} />,
      'accuracy': <TrophyOutlined style={{ color: '#52c41a' }} />,
      'efficiency': <RocketOutlined style={{ color: '#fa8c16' }} />,
      'cost': <StarOutlined style={{ color: '#722ed1' }} />
    }
    return icons[type] || <InfoCircleOutlined />
  }

  if (!query && !analysisResults) {
    return (
      <Card>
        <Empty
          image={Empty.PRESENTED_IMAGE_SIMPLE}
          description={
            <div>
              <Text>Performance metrics will be available after query analysis</Text>
              <br />
              <Text type="secondary" style={{ fontSize: '12px' }}>
                Enter a query to see real-time performance analytics and optimization suggestions
              </Text>
            </div>
          }
        />
      </Card>
    )
  }

  const overallPerformanceScore = mockPerformanceData.metrics.reduce((sum, metric) => {
    const score = metric.target ? (metric.value / metric.target) * 100 : 100
    return sum + Math.min(score, 100)
  }, 0) / mockPerformanceData.metrics.length

  return (
    <Space direction="vertical" style={{ width: '100%' }} size="large">
      {/* Overview Header */}
      <Card
        title={
          <Space>
            <DashboardOutlined />
            <span>Performance Analytics</span>
            <Badge
              count={`${Math.round(overallPerformanceScore)}% health`}
              style={{ backgroundColor: getMetricStatusColor(overallPerformanceScore > 90 ? 'excellent' : overallPerformanceScore > 80 ? 'good' : overallPerformanceScore > 70 ? 'warning' : 'critical') }}
            />
          </Space>
        }
        extra={
          interactive && (
            <Space>
              <Tooltip title="Refresh metrics">
                <Button
                  icon={<ReloadOutlined />}
                  size="small"
                  loading={refreshing}
                  onClick={handleMetricRefresh}
                />
              </Tooltip>
              <Tooltip title="Performance settings">
                <Button icon={<SettingOutlined />} size="small" />
              </Tooltip>
            </Space>
          )
        }
      >
        <Row gutter={[16, 16]}>
          <Col xs={24} sm={12} md={6}>
            <Statistic
              title="Overall Health"
              value={overallPerformanceScore}
              precision={1}
              suffix="%"
              valueStyle={{ color: getMetricStatusColor(overallPerformanceScore > 90 ? 'excellent' : overallPerformanceScore > 80 ? 'good' : 'warning') }}
              prefix={<TrophyOutlined />}
            />
          </Col>
          <Col xs={24} sm={12} md={6}>
            <Statistic
              title="Response Time"
              value={mockPerformanceData.metrics[0].value}
              precision={1}
              suffix="s"
              valueStyle={{ color: '#1890ff' }}
              prefix={<ClockCircleOutlined />}
            />
          </Col>
          <Col xs={24} sm={12} md={6}>
            <Statistic
              title="Success Rate"
              value={mockPerformanceData.usageAnalytics.successRate}
              precision={1}
              suffix="%"
              valueStyle={{ color: '#52c41a' }}
              prefix={<CheckCircleOutlined />}
            />
          </Col>
          <Col xs={24} sm={12} md={6}>
            <Statistic
              title="Total Queries"
              value={mockPerformanceData.usageAnalytics.totalQueries}
              valueStyle={{ color: '#fa8c16' }}
              prefix={<BarChartOutlined />}
            />
          </Col>
        </Row>

        <Alert
          message="System Performance Status"
          description={`System is performing ${overallPerformanceScore > 90 ? 'excellently' : overallPerformanceScore > 80 ? 'well' : 'adequately'} with ${mockPerformanceData.optimizations.length} optimization opportunities identified.`}
          type={overallPerformanceScore > 90 ? 'success' : overallPerformanceScore > 80 ? 'info' : 'warning'}
          showIcon
          style={{ marginTop: 16 }}
        />
      </Card>

      {/* Main Content Tabs */}
      <Card>
        <Tabs
          activeKey={activeTab}
          onChange={setActiveTab}
          size="large"
          type="card"
        >
          {/* Performance Metrics Tab */}
          {showRealTimeMetrics && (
            <TabPane
              tab={
                <Space>
                  <LineChartOutlined />
                  <span>Real-time Metrics</span>
                  <Badge count={mockPerformanceData.metrics.length} size="small" />
                </Space>
              }
              key="metrics"
            >
              <Space direction="vertical" style={{ width: '100%' }} size="large">
                <Row gutter={[16, 16]}>
                  {mockPerformanceData.metrics.map((metric) => (
                    <Col xs={24} sm={12} md={8} key={metric.id}>
                      <Card
                        size="small"
                        style={{
                          borderLeft: `4px solid ${getMetricStatusColor(metric.status)}`,
                          height: '100%'
                        }}
                      >
                        <Space direction="vertical" style={{ width: '100%' }}>
                          <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                            <Text strong style={{ fontSize: '14px' }}>{metric.name}</Text>
                            {getTrendIcon(metric.trend)}
                          </div>

                          <div style={{ display: 'flex', alignItems: 'baseline', gap: '8px' }}>
                            <Text style={{ fontSize: '24px', fontWeight: 'bold', color: getMetricStatusColor(metric.status) }}>
                              {metric.value}
                            </Text>
                            <Text type="secondary">{metric.unit}</Text>
                            <Tag
                              color={metric.trend === 'up' ? 'green' : metric.trend === 'down' ? 'red' : 'blue'}
                              size="small"
                            >
                              {metric.trend === 'up' ? '+' : metric.trend === 'down' ? '-' : '±'}{Math.abs(metric.trendValue)}%
                            </Tag>
                          </div>

                          <Text type="secondary" style={{ fontSize: '12px' }}>
                            {metric.description}
                          </Text>

                          {metric.target && (
                            <div>
                              <Text strong style={{ fontSize: '11px' }}>Target: {metric.target}{metric.unit}</Text>
                              <Progress
                                percent={Math.min((metric.value / metric.target) * 100, 100)}
                                strokeColor={getMetricStatusColor(metric.status)}
                                size="small"
                                style={{ marginTop: 4 }}
                              />
                            </div>
                          )}
                        </Space>
                      </Card>
                    </Col>
                  ))}
                </Row>
              </Space>
            </TabPane>
          )}

          {/* Optimization Suggestions Tab */}
          {showOptimizations && (
            <TabPane
              tab={
                <Space>
                  <BulbOutlined />
                  <span>Optimizations</span>
                  <Badge count={mockPerformanceData.optimizations.length} size="small" />
                </Space>
              }
              key="optimizations"
            >
              <Space direction="vertical" style={{ width: '100%' }} size="middle">
                <Alert
                  message="Performance Optimization Opportunities"
                  description="AI-powered suggestions to improve system performance, accuracy, and efficiency."
                  type="info"
                  showIcon
                />

                {mockPerformanceData.optimizations
                  .sort((a, b) => a.priority - b.priority)
                  .map((optimization) => (
                    <Card
                      key={optimization.id}
                      size="small"
                      hoverable={interactive}
                      onClick={() => interactive && setSelectedOptimization(optimization.id)}
                      style={{
                        cursor: interactive ? 'pointer' : 'default',
                        border: selectedOptimization === optimization.id ? '2px solid #1890ff' : undefined
                      }}
                      title={
                        <Space>
                          {getOptimizationTypeIcon(optimization.type)}
                          <span style={{ fontWeight: 600 }}>{optimization.title}</span>
                          <Tag color={getImpactColor(optimization.impact)}>
                            {optimization.impact} impact
                          </Tag>
                          <Tag color="blue">Priority {optimization.priority}</Tag>
                          {selectedOptimization === optimization.id && (
                            <Tag color="green">SELECTED</Tag>
                          )}
                        </Space>
                      }
                      extra={
                        interactive && (
                          <Space>
                            <Button
                              type="primary"
                              size="small"
                              icon={<CheckCircleOutlined />}
                              onClick={(e) => {
                                e.stopPropagation()
                                handleOptimizationApply(optimization.id)
                              }}
                            >
                              Apply
                            </Button>
                            <Button
                              type="link"
                              size="small"
                              icon={<InfoCircleOutlined />}
                            >
                              Details
                            </Button>
                          </Space>
                        )
                      }
                    >
                      <Space direction="vertical" style={{ width: '100%' }}>
                        <div>
                          <Text>{optimization.description}</Text>
                        </div>

                        <Row gutter={[16, 8]}>
                          <Col span={8}>
                            <Text strong>Expected Improvement:</Text>
                            <div style={{ marginTop: 4 }}>
                              <Tag color="green">{optimization.estimatedImprovement}</Tag>
                            </div>
                          </Col>
                          <Col span={8}>
                            <Text strong>Implementation Effort:</Text>
                            <div style={{ marginTop: 4 }}>
                              <Tag color={optimization.effort === 'high' ? 'red' : optimization.effort === 'medium' ? 'orange' : 'green'}>
                                {optimization.effort}
                              </Tag>
                            </div>
                          </Col>
                          <Col span={8}>
                            <Text strong>Optimization Type:</Text>
                            <div style={{ marginTop: 4 }}>
                              <Tag color="blue">{optimization.type}</Tag>
                            </div>
                          </Col>
                        </Row>

                        <div style={{ display: 'flex', alignItems: 'center', gap: '8px' }}>
                          <Text strong style={{ fontSize: '12px' }}>Impact Level:</Text>
                          <Progress
                            percent={optimization.impact === 'high' ? 100 : optimization.impact === 'medium' ? 66 : 33}
                            strokeColor={getImpactColor(optimization.impact)}
                            size="small"
                            style={{ flex: 1 }}
                            format={() => optimization.impact}
                          />
                        </div>
                      </Space>
                    </Card>
                  ))}
              </Space>
            </TabPane>
          )}

          {/* Usage Analytics Tab */}
          {showUsageAnalytics && (
            <TabPane
              tab={
                <Space>
                  <PieChartOutlined />
                  <span>Usage Analytics</span>
                </Space>
              }
              key="analytics"
            >
              <Space direction="vertical" style={{ width: '100%' }} size="large">
                {/* Usage Summary */}
                <Card size="small" title="Usage Summary">
                  <Row gutter={[16, 16]}>
                    <Col xs={24} sm={12} md={6}>
                      <Card size="small" style={{ textAlign: 'center' }}>
                        <Statistic
                          title="Total Queries"
                          value={mockPerformanceData.usageAnalytics.totalQueries}
                          valueStyle={{ color: '#1890ff' }}
                          prefix={<BarChartOutlined />}
                        />
                      </Card>
                    </Col>
                    <Col xs={24} sm={12} md={6}>
                      <Card size="small" style={{ textAlign: 'center' }}>
                        <Statistic
                          title="Avg Response Time"
                          value={mockPerformanceData.usageAnalytics.averageResponseTime}
                          precision={1}
                          suffix="s"
                          valueStyle={{ color: '#52c41a' }}
                          prefix={<ClockCircleOutlined />}
                        />
                      </Card>
                    </Col>
                    <Col xs={24} sm={12} md={6}>
                      <Card size="small" style={{ textAlign: 'center' }}>
                        <Statistic
                          title="Success Rate"
                          value={mockPerformanceData.usageAnalytics.successRate}
                          precision={1}
                          suffix="%"
                          valueStyle={{ color: '#fa8c16' }}
                          prefix={<TrophyOutlined />}
                        />
                      </Card>
                    </Col>
                    <Col xs={24} sm={12} md={6}>
                      <Card size="small" style={{ textAlign: 'center' }}>
                        <Statistic
                          title="User Satisfaction"
                          value={mockPerformanceData.usageAnalytics.userSatisfaction}
                          precision={1}
                          suffix="/ 5.0"
                          valueStyle={{ color: '#722ed1' }}
                          prefix={<StarOutlined />}
                        />
                        <Rate
                          disabled
                          value={mockPerformanceData.usageAnalytics.userSatisfaction}
                          style={{ fontSize: '12px', marginTop: 4 }}
                        />
                      </Card>
                    </Col>
                  </Row>
                </Card>

                {/* Popular Queries */}
                <Card size="small" title="Most Popular Queries">
                  <List
                    dataSource={mockPerformanceData.usageAnalytics.popularQueries}
                    renderItem={(query, index) => (
                      <List.Item>
                        <List.Item.Meta
                          avatar={
                            <Avatar
                              style={{ backgroundColor: '#1890ff' }}
                            >
                              {index + 1}
                            </Avatar>
                          }
                          title={query}
                          description={`Frequently used query pattern`}
                        />
                        <div>
                          <Progress
                            percent={Math.max(100 - (index * 20), 20)}
                            strokeColor="#1890ff"
                            size="small"
                            style={{ width: 100 }}
                            format={(percent) => `${percent}%`}
                          />
                        </div>
                      </List.Item>
                    )}
                  />
                </Card>

                {/* Peak Usage Hours */}
                <Card size="small" title="Peak Usage Analysis">
                  <div style={{ marginBottom: 16 }}>
                    <Text strong>Peak Usage Hours: </Text>
                    <Space wrap>
                      {Array.from({ length: 24 }, (_, hour) => (
                        <Tag
                          key={hour}
                          color={mockPerformanceData.usageAnalytics.peakUsageHours.includes(hour) ? 'blue' : 'default'}
                        >
                          {hour.toString().padStart(2, '0')}:00
                        </Tag>
                      ))}
                    </Space>
                  </div>

                  <Alert
                    message="Usage Pattern Insights"
                    description="Peak usage occurs during business hours (9 AM - 4 PM). Consider scaling resources during these periods for optimal performance."
                    type="info"
                    showIcon
                  />
                </Card>

                {/* Performance Timeline */}
                <Card size="small" title="Recent Performance Events">
                  <Timeline>
                    <Timeline.Item color="green" dot={<CheckCircleOutlined />}>
                      <Text strong>System optimization applied</Text>
                      <br />
                      <Text type="secondary">Cache hit rate improved by 15% - 2 hours ago</Text>
                    </Timeline.Item>
                    <Timeline.Item color="blue" dot={<InfoCircleOutlined />}>
                      <Text strong>Peak usage period detected</Text>
                      <br />
                      <Text type="secondary">High query volume between 10-11 AM - 4 hours ago</Text>
                    </Timeline.Item>
                    <Timeline.Item color="orange" dot={<WarningOutlined />}>
                      <Text strong>Performance alert resolved</Text>
                      <br />
                      <Text type="secondary">Response time spike resolved automatically - 6 hours ago</Text>
                    </Timeline.Item>
                    <Timeline.Item color="green" dot={<TrophyOutlined />}>
                      <Text strong>Accuracy milestone achieved</Text>
                      <br />
                      <Text type="secondary">Analysis accuracy reached 95% target - 1 day ago</Text>
                    </Timeline.Item>
                  </Timeline>
                </Card>
              </Space>
            </TabPane>
          )}
        </Tabs>
      </Card>
    </Space>
  )
}

export default PerformanceMetricsPanel
