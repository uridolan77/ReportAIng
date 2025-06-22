import React, { useState, useMemo } from 'react'
import { 
  Card, 
  Typography, 
  Space, 
  Button,
  Alert,
  List,
  Tag,
  Progress,
  Row,
  Col,
  Statistic,
  Tooltip,
  Badge,
  Select,
  Switch,
  Divider,
  Timeline
} from 'antd'
import {
  BulbOutlined,
  TrophyOutlined,
  ExclamationCircleOutlined,
  CheckCircleOutlined,
  RobotOutlined,
  BarChartOutlined,
  ReloadOutlined,
  SettingOutlined,
  EyeOutlined,
  ThunderboltOutlined,
  StarOutlined
} from '@ant-design/icons'
import { LineChart, Line, BarChart, Bar, XAxis, YAxis, CartesianGrid, Tooltip as RechartsTooltip, ResponsiveContainer } from 'recharts'
import { ConfidenceIndicator } from '../common/ConfidenceIndicator'
import { useGetAutomatedInsightsQuery, useGenerateInsightMutation } from '@shared/store/api/intelligentAgentsApi'
import type { AutomatedInsight, InsightCategory, InsightTrend } from '@shared/types/intelligentAgents'

const { Title, Text, Paragraph } = Typography

export interface AutomatedInsightsGeneratorProps {
  dataContext?: string
  insightCategories?: InsightCategory[]
  showTrends?: boolean
  showRecommendations?: boolean
  autoRefresh?: boolean
  refreshInterval?: number
  onInsightGenerate?: (insight: AutomatedInsight) => void
  onInsightApply?: (insight: AutomatedInsight) => void
  className?: string
  testId?: string
}

/**
 * AutomatedInsightsGenerator - AI-powered automated insights and recommendations
 * 
 * Features:
 * - Real-time automated insight generation
 * - Pattern recognition and trend analysis
 * - Business intelligence recommendations
 * - Anomaly detection and alerting
 * - Interactive insight exploration
 * - Customizable insight categories and filters
 */
export const AutomatedInsightsGenerator: React.FC<AutomatedInsightsGeneratorProps> = ({
  dataContext,
  insightCategories = ['performance', 'cost', 'usage', 'anomaly'],
  showTrends = true,
  showRecommendations = true,
  autoRefresh = false,
  refreshInterval = 30000,
  onInsightGenerate,
  onInsightApply,
  className,
  testId = 'automated-insights-generator'
}) => {
  const [selectedCategory, setSelectedCategory] = useState<InsightCategory>('all')
  const [insightHistory, setInsightHistory] = useState<AutomatedInsight[]>([])
  const [autoRefreshEnabled, setAutoRefreshEnabled] = useState(autoRefresh)

  // Real API data
  const { data: insights, isLoading: insightsLoading, refetch: refetchInsights } = 
    useGetAutomatedInsightsQuery({ 
      context: dataContext,
      categories: selectedCategory === 'all' ? insightCategories : [selectedCategory],
      limit: 20
    }, {
      pollingInterval: autoRefreshEnabled ? refreshInterval : 0
    })
  
  const [generateInsight, { isLoading: generateLoading }] = useGenerateInsightMutation()

  // Filter insights by category
  const filteredInsights = useMemo(() => {
    if (!insights?.insights) return []
    
    return selectedCategory === 'all' 
      ? insights.insights 
      : insights.insights.filter(insight => insight.category === selectedCategory)
  }, [insights, selectedCategory])

  // Group insights by priority
  const insightsByPriority = useMemo(() => {
    const grouped = filteredInsights.reduce((acc, insight) => {
      if (!acc[insight.priority]) {
        acc[insight.priority] = []
      }
      acc[insight.priority].push(insight)
      return acc
    }, {} as Record<string, AutomatedInsight[]>)
    
    return {
      critical: grouped.critical || [],
      high: grouped.high || [],
      medium: grouped.medium || [],
      low: grouped.low || []
    }
  }, [filteredInsights])

  // Calculate insight metrics
  const insightMetrics = useMemo(() => {
    if (!insights?.insights) return null
    
    const total = insights.insights.length
    const actionable = insights.insights.filter(i => i.actionable).length
    const avgConfidence = insights.insights.reduce((acc, i) => acc + i.confidence, 0) / total
    const categories = new Set(insights.insights.map(i => i.category)).size
    
    return { total, actionable, avgConfidence, categories }
  }, [insights])

  // Handle manual insight generation
  const handleGenerateInsight = async () => {
    try {
      const result = await generateInsight({
        context: dataContext,
        category: selectedCategory === 'all' ? undefined : selectedCategory
      }).unwrap()
      
      setInsightHistory(prev => [result, ...prev.slice(0, 9)]) // Keep last 10
      onInsightGenerate?.(result)
    } catch (error) {
      console.error('Failed to generate insight:', error)
    }
  }

  // Handle insight application
  const handleApplyInsight = (insight: AutomatedInsight) => {
    onInsightApply?.(insight)
  }

  // Get category color
  const getCategoryColor = (category: string) => {
    switch (category) {
      case 'performance': return '#1890ff'
      case 'cost': return '#52c41a'
      case 'usage': return '#faad14'
      case 'anomaly': return '#ff4d4f'
      case 'trend': return '#722ed1'
      case 'optimization': return '#13c2c2'
      default: return '#d9d9d9'
    }
  }

  // Get priority color
  const getPriorityColor = (priority: string) => {
    switch (priority) {
      case 'critical': return '#ff4d4f'
      case 'high': return '#faad14'
      case 'medium': return '#1890ff'
      case 'low': return '#52c41a'
      default: return '#d9d9d9'
    }
  }

  // Render insight metrics
  const renderInsightMetrics = () => {
    if (!insightMetrics) return null

    return (
      <Row gutter={[16, 16]} style={{ marginBottom: 24 }}>
        <Col xs={24} sm={12} lg={6}>
          <Card>
            <Statistic
              title="Total Insights"
              value={insightMetrics.total}
              prefix={<BulbOutlined />}
              valueStyle={{ color: '#1890ff' }}
            />
          </Card>
        </Col>
        <Col xs={24} sm={12} lg={6}>
          <Card>
            <Statistic
              title="Actionable"
              value={insightMetrics.actionable}
              suffix={`/ ${insightMetrics.total}`}
              prefix={<ThunderboltOutlined />}
              valueStyle={{ color: '#52c41a' }}
            />
          </Card>
        </Col>
        <Col xs={24} sm={12} lg={6}>
          <Card>
            <Statistic
              title="Avg Confidence"
              value={insightMetrics.avgConfidence * 100}
              suffix="%"
              precision={1}
              prefix={<TrophyOutlined />}
              valueStyle={{ color: '#722ed1' }}
            />
          </Card>
        </Col>
        <Col xs={24} sm={12} lg={6}>
          <Card>
            <Statistic
              title="Categories"
              value={insightMetrics.categories}
              prefix={<BarChartOutlined />}
              valueStyle={{ color: '#faad14' }}
            />
          </Card>
        </Col>
      </Row>
    )
  }

  // Render priority insights
  const renderPriorityInsights = () => {
    const priorities = ['critical', 'high', 'medium', 'low'] as const
    
    return (
      <Row gutter={[16, 16]} style={{ marginBottom: 24 }}>
        {priorities.map(priority => {
          const priorityInsights = insightsByPriority[priority]
          if (!priorityInsights.length) return null
          
          return (
            <Col key={priority} xs={24} lg={6}>
              <Card 
                title={
                  <Space>
                    <span style={{ textTransform: 'capitalize' }}>{priority} Priority</span>
                    <Badge count={priorityInsights.length} style={{ backgroundColor: getPriorityColor(priority) }} />
                  </Space>
                }
                size="small"
              >
                <List
                  size="small"
                  dataSource={priorityInsights.slice(0, 3)}
                  renderItem={(insight) => (
                    <List.Item>
                      <Space direction="vertical" style={{ width: '100%' }}>
                        <Text strong style={{ fontSize: '12px' }}>
                          {insight.title}
                        </Text>
                        <Space>
                          <Tag size="small" color={getCategoryColor(insight.category)}>
                            {insight.category}
                          </Tag>
                          <ConfidenceIndicator
                            confidence={insight.confidence}
                            size="small"
                            type="circle"
                            showPercentage={false}
                          />
                        </Space>
                      </Space>
                    </List.Item>
                  )}
                />
                {priorityInsights.length > 3 && (
                  <Text type="secondary" style={{ fontSize: '11px' }}>
                    +{priorityInsights.length - 3} more
                  </Text>
                )}
              </Card>
            </Col>
          )
        })}
      </Row>
    )
  }

  // Render detailed insights
  const renderDetailedInsights = () => (
    <Card 
      title={
        <Space>
          <RobotOutlined />
          <span>AI-Generated Insights</span>
          <Badge count={filteredInsights.length} />
        </Space>
      }
      extra={
        <Space>
          <Select
            value={selectedCategory}
            onChange={setSelectedCategory}
            style={{ width: 120 }}
          >
            <Select.Option value="all">All Categories</Select.Option>
            {insightCategories.map(category => (
              <Select.Option key={category} value={category}>
                {category.charAt(0).toUpperCase() + category.slice(1)}
              </Select.Option>
            ))}
          </Select>
        </Space>
      }
    >
      <List
        dataSource={filteredInsights}
        renderItem={(insight) => (
          <List.Item
            actions={[
              <ConfidenceIndicator
                confidence={insight.confidence}
                size="small"
                type="badge"
              />,
              ...(insight.actionable ? [
                <Button 
                  type="primary" 
                  size="small"
                  onClick={() => handleApplyInsight(insight)}
                >
                  Apply
                </Button>
              ] : [])
            ]}
          >
            <List.Item.Meta
              avatar={
                <div style={{ display: 'flex', alignItems: 'center' }}>
                  {insight.priority === 'critical' ? 
                    <ExclamationCircleOutlined style={{ color: '#ff4d4f' }} /> :
                    <BulbOutlined style={{ color: getCategoryColor(insight.category) }} />
                  }
                </div>
              }
              title={
                <Space>
                  <Text strong>{insight.title}</Text>
                  <Tag color={getCategoryColor(insight.category)}>
                    {insight.category.toUpperCase()}
                  </Tag>
                  <Tag color={getPriorityColor(insight.priority)}>
                    {insight.priority.toUpperCase()}
                  </Tag>
                  {insight.trending && <StarOutlined style={{ color: '#faad14' }} />}
                </Space>
              }
              description={
                <div>
                  <Paragraph style={{ marginBottom: 8 }}>
                    {insight.description}
                  </Paragraph>
                  {insight.impact && (
                    <div style={{ marginBottom: 4 }}>
                      <Text strong>Impact: </Text>
                      <Text>{insight.impact}</Text>
                    </div>
                  )}
                  {insight.recommendation && (
                    <div style={{ marginBottom: 4 }}>
                      <Text strong>Recommendation: </Text>
                      <Text>{insight.recommendation}</Text>
                    </div>
                  )}
                  <div>
                    <Text type="secondary" style={{ fontSize: '12px' }}>
                      Generated: {new Date(insight.generatedAt).toLocaleString()}
                    </Text>
                  </div>
                </div>
              }
            />
          </List.Item>
        )}
      />
    </Card>
  )

  // Render trends visualization
  const renderTrendsVisualization = () => {
    if (!showTrends || !insights?.trends) return null

    return (
      <Card 
        title={
          <Space>
            <BarChartOutlined />
            <span>Insight Trends</span>
          </Space>
        }
        style={{ marginTop: 16 }}
      >
        <ResponsiveContainer width="100%" height={300}>
          <LineChart data={insights.trends}>
            <CartesianGrid strokeDasharray="3 3" />
            <XAxis dataKey="date" />
            <YAxis />
            <RechartsTooltip />
            <Line type="monotone" dataKey="insightCount" stroke="#1890ff" strokeWidth={2} />
            <Line type="monotone" dataKey="actionableCount" stroke="#52c41a" strokeWidth={2} />
          </LineChart>
        </ResponsiveContainer>
      </Card>
    )
  }

  return (
    <div className={className} data-testid={testId}>
      {/* Header */}
      <div style={{ 
        display: 'flex', 
        justifyContent: 'space-between', 
        alignItems: 'center',
        marginBottom: 16 
      }}>
        <Title level={5} style={{ margin: 0 }}>
          Automated Insights Generator
        </Title>
        <Space>
          <Switch
            checked={autoRefreshEnabled}
            onChange={setAutoRefreshEnabled}
            checkedChildren="Auto"
            unCheckedChildren="Manual"
          />
          <Button 
            icon={<ThunderboltOutlined />}
            loading={generateLoading}
            onClick={handleGenerateInsight}
          >
            Generate
          </Button>
          <Button 
            icon={<ReloadOutlined />}
            loading={insightsLoading}
            onClick={() => refetchInsights()}
          >
            Refresh
          </Button>
        </Space>
      </div>

      {/* Context Alert */}
      {dataContext && (
        <Alert
          message="Data Context Analysis"
          description={`AI is analyzing data context: "${dataContext}" to generate relevant insights and recommendations.`}
          type="info"
          showIcon
          style={{ marginBottom: 16 }}
        />
      )}

      {/* Loading State */}
      {insightsLoading && (
        <Card style={{ textAlign: 'center', padding: '40px 0' }}>
          <Space direction="vertical">
            <RobotOutlined style={{ fontSize: '48px', color: '#1890ff' }} />
            <Text>AI is generating insights from your data...</Text>
            <Progress percent={65} status="active" />
          </Space>
        </Card>
      )}

      {/* Insights Content */}
      {insights && !insightsLoading && (
        <div>
          {/* Insight Metrics */}
          {renderInsightMetrics()}

          {/* Priority Insights */}
          {renderPriorityInsights()}

          {/* Detailed Insights */}
          {renderDetailedInsights()}

          {/* Trends Visualization */}
          {renderTrendsVisualization()}
        </div>
      )}

      {/* No Insights Available */}
      {!insights && !insightsLoading && (
        <Card style={{ textAlign: 'center', padding: '40px 0' }}>
          <Space direction="vertical">
            <EyeOutlined style={{ fontSize: '48px', color: '#d9d9d9' }} />
            <Title level={4}>No Insights Available</Title>
            <Text type="secondary">
              Click "Generate" to create AI-powered insights from your data.
            </Text>
            <Button 
              type="primary"
              icon={<ThunderboltOutlined />}
              onClick={handleGenerateInsight}
              loading={generateLoading}
            >
              Generate Insights
            </Button>
          </Space>
        </Card>
      )}
    </div>
  )
}

export default AutomatedInsightsGenerator
