import React, { useState, useMemo } from 'react'
import { 
  Card, 
  Space, 
  Typography, 
  List, 
  Tag, 
  Button,
  Progress,
  Row, 
  Col, 
  Statistic,
  Alert,
  Collapse,
  Timeline,
  Tooltip
} from 'antd'
import {
  BulbOutlined,
  TrophyOutlined,
  ClockCircleOutlined,
  DollarOutlined,
  BarChartOutlined,
  ThunderboltOutlined,
  CheckCircleOutlined,
  ExclamationCircleOutlined,
  RocketOutlined,
  SettingOutlined
} from '@ant-design/icons'
import type { OptimizationSuggestion } from '@shared/types/transparency'

const { Title, Text, Paragraph } = Typography
const { Panel } = Collapse

export interface OptimizationInsightsProps {
  suggestions: OptimizationSuggestion[]
  showPriorityFilter?: boolean
  showCategoryFilter?: boolean
  showImplementationGuide?: boolean
  onApplySuggestion?: (suggestion: OptimizationSuggestion) => void
  onDismissSuggestion?: (suggestion: OptimizationSuggestion) => void
  className?: string
  testId?: string
}

interface InsightAnalysis {
  totalSuggestions: number
  highPriority: number
  mediumPriority: number
  lowPriority: number
  categories: { [key: string]: number }
  potentialImpact: {
    performance: number
    cost: number
    accuracy: number
  }
  implementationComplexity: {
    easy: number
    medium: number
    hard: number
  }
}

/**
 * OptimizationInsights - Displays AI optimization suggestions and insights
 * 
 * Features:
 * - Categorized optimization suggestions
 * - Priority-based filtering
 * - Impact assessment
 * - Implementation guidance
 * - Progress tracking
 * - Actionable recommendations
 */
export const OptimizationInsights: React.FC<OptimizationInsightsProps> = ({
  suggestions,
  showPriorityFilter = true,
  showCategoryFilter = true,
  showImplementationGuide = true,
  onApplySuggestion,
  onDismissSuggestion,
  className,
  testId = 'optimization-insights'
}) => {
  const [selectedPriority, setSelectedPriority] = useState<string>('all')
  const [selectedCategory, setSelectedCategory] = useState<string>('all')
  const [expandedPanels, setExpandedPanels] = useState<string[]>(['overview'])

  // Analyze optimization insights
  const analysis = useMemo((): InsightAnalysis => {
    if (!suggestions || suggestions.length === 0) {
      return {
        totalSuggestions: 0,
        highPriority: 0,
        mediumPriority: 0,
        lowPriority: 0,
        categories: {},
        potentialImpact: { performance: 0, cost: 0, accuracy: 0 },
        implementationComplexity: { easy: 0, medium: 0, hard: 0 }
      }
    }

    const totalSuggestions = suggestions.length
    const highPriority = suggestions.filter(s => s.priority === 'high').length
    const mediumPriority = suggestions.filter(s => s.priority === 'medium').length
    const lowPriority = suggestions.filter(s => s.priority === 'low').length

    // Count by category
    const categories: { [key: string]: number } = {}
    suggestions.forEach(suggestion => {
      categories[suggestion.category] = (categories[suggestion.category] || 0) + 1
    })

    // Calculate potential impact
    const potentialImpact = {
      performance: suggestions.reduce((sum, s) => sum + (s.estimatedImpact?.performance || 0), 0) / totalSuggestions,
      cost: suggestions.reduce((sum, s) => sum + (s.estimatedImpact?.cost || 0), 0) / totalSuggestions,
      accuracy: suggestions.reduce((sum, s) => sum + (s.estimatedImpact?.accuracy || 0), 0) / totalSuggestions
    }

    // Count by implementation complexity
    const implementationComplexity = {
      easy: suggestions.filter(s => s.implementationComplexity === 'easy').length,
      medium: suggestions.filter(s => s.implementationComplexity === 'medium').length,
      hard: suggestions.filter(s => s.implementationComplexity === 'hard').length
    }

    return {
      totalSuggestions,
      highPriority,
      mediumPriority,
      lowPriority,
      categories,
      potentialImpact,
      implementationComplexity
    }
  }, [suggestions])

  // Filter suggestions based on selected criteria
  const filteredSuggestions = useMemo(() => {
    let filtered = suggestions || []

    if (selectedPriority !== 'all') {
      filtered = filtered.filter(s => s.priority === selectedPriority)
    }

    if (selectedCategory !== 'all') {
      filtered = filtered.filter(s => s.category === selectedCategory)
    }

    return filtered.sort((a, b) => {
      const priorityOrder = { high: 3, medium: 2, low: 1 }
      return priorityOrder[b.priority] - priorityOrder[a.priority]
    })
  }, [suggestions, selectedPriority, selectedCategory])

  const getPriorityColor = (priority: string) => {
    switch (priority) {
      case 'high': return '#ff4d4f'
      case 'medium': return '#faad14'
      case 'low': return '#52c41a'
      default: return '#d9d9d9'
    }
  }

  const getCategoryIcon = (category: string) => {
    switch (category.toLowerCase()) {
      case 'performance': return <ThunderboltOutlined />
      case 'cost': return <DollarOutlined />
      case 'accuracy': return <TrophyOutlined />
      case 'clarity': return <BulbOutlined />
      default: return <SettingOutlined />
    }
  }

  const getComplexityColor = (complexity: string) => {
    switch (complexity) {
      case 'easy': return '#52c41a'
      case 'medium': return '#faad14'
      case 'hard': return '#ff4d4f'
      default: return '#d9d9d9'
    }
  }

  const renderOverview = () => (
    <Space direction="vertical" style={{ width: '100%' }}>
      {/* Key Metrics */}
      <Row gutter={[16, 16]}>
        <Col xs={24} sm={12} lg={6}>
          <Statistic
            title="Total Suggestions"
            value={analysis.totalSuggestions}
            prefix={<BulbOutlined />}
          />
        </Col>
        <Col xs={24} sm={12} lg={6}>
          <Statistic
            title="High Priority"
            value={analysis.highPriority}
            prefix={<ExclamationCircleOutlined />}
            valueStyle={{ color: '#ff4d4f' }}
          />
        </Col>
        <Col xs={24} sm={12} lg={6}>
          <Statistic
            title="Quick Wins"
            value={analysis.implementationComplexity.easy}
            prefix={<RocketOutlined />}
            valueStyle={{ color: '#52c41a' }}
          />
        </Col>
        <Col xs={24} sm={12} lg={6}>
          <Statistic
            title="Categories"
            value={Object.keys(analysis.categories).length}
            prefix={<BarChartOutlined />}
          />
        </Col>
      </Row>

      {/* Priority Distribution */}
      <Row gutter={[16, 16]}>
        <Col span={8}>
          <Card size="small">
            <Space direction="vertical" style={{ width: '100%' }}>
              <Text strong>High Priority</Text>
              <Progress 
                percent={(analysis.highPriority / analysis.totalSuggestions) * 100}
                strokeColor="#ff4d4f"
                format={(percent) => `${analysis.highPriority}/${analysis.totalSuggestions}`}
              />
            </Space>
          </Card>
        </Col>
        <Col span={8}>
          <Card size="small">
            <Space direction="vertical" style={{ width: '100%' }}>
              <Text strong>Medium Priority</Text>
              <Progress 
                percent={(analysis.mediumPriority / analysis.totalSuggestions) * 100}
                strokeColor="#faad14"
                format={(percent) => `${analysis.mediumPriority}/${analysis.totalSuggestions}`}
              />
            </Space>
          </Card>
        </Col>
        <Col span={8}>
          <Card size="small">
            <Space direction="vertical" style={{ width: '100%' }}>
              <Text strong>Low Priority</Text>
              <Progress 
                percent={(analysis.lowPriority / analysis.totalSuggestions) * 100}
                strokeColor="#52c41a"
                format={(percent) => `${analysis.lowPriority}/${analysis.totalSuggestions}`}
              />
            </Space>
          </Card>
        </Col>
      </Row>

      {/* Potential Impact */}
      <Card title="Potential Impact" size="small">
        <Row gutter={[16, 16]}>
          <Col span={8}>
            <Space direction="vertical" style={{ width: '100%' }}>
              <Text strong>Performance Improvement</Text>
              <Progress 
                percent={analysis.potentialImpact.performance * 100}
                strokeColor="#1890ff"
                format={(percent) => `${percent?.toFixed(1)}%`}
              />
            </Space>
          </Col>
          <Col span={8}>
            <Space direction="vertical" style={{ width: '100%' }}>
              <Text strong>Cost Reduction</Text>
              <Progress 
                percent={analysis.potentialImpact.cost * 100}
                strokeColor="#52c41a"
                format={(percent) => `${percent?.toFixed(1)}%`}
              />
            </Space>
          </Col>
          <Col span={8}>
            <Space direction="vertical" style={{ width: '100%' }}>
              <Text strong>Accuracy Improvement</Text>
              <Progress 
                percent={analysis.potentialImpact.accuracy * 100}
                strokeColor="#722ed1"
                format={(percent) => `${percent?.toFixed(1)}%`}
              />
            </Space>
          </Col>
        </Row>
      </Card>
    </Space>
  )

  const renderSuggestionsList = () => (
    <List
      dataSource={filteredSuggestions}
      renderItem={(suggestion) => (
        <List.Item
          actions={[
            <Button
              key="apply"
              type="primary"
              size="small"
              onClick={() => onApplySuggestion?.(suggestion)}
            >
              Apply
            </Button>,
            <Button
              key="dismiss"
              size="small"
              onClick={() => onDismissSuggestion?.(suggestion)}
            >
              Dismiss
            </Button>
          ]}
        >
          <List.Item.Meta
            avatar={getCategoryIcon(suggestion.category)}
            title={
              <Space>
                <Text strong>{suggestion.title}</Text>
                <Tag color={getPriorityColor(suggestion.priority)}>
                  {suggestion.priority.toUpperCase()}
                </Tag>
                <Tag color={getComplexityColor(suggestion.implementationComplexity)}>
                  {suggestion.implementationComplexity}
                </Tag>
              </Space>
            }
            description={
              <Space direction="vertical" style={{ width: '100%' }}>
                <Paragraph style={{ margin: 0 }}>
                  {suggestion.description}
                </Paragraph>
                {suggestion.estimatedImpact && (
                  <Space>
                    <Text type="secondary" style={{ fontSize: '12px' }}>
                      Impact: Performance {(suggestion.estimatedImpact.performance * 100).toFixed(0)}% • 
                      Cost {(suggestion.estimatedImpact.cost * 100).toFixed(0)}% • 
                      Accuracy {(suggestion.estimatedImpact.accuracy * 100).toFixed(0)}%
                    </Text>
                  </Space>
                )}
              </Space>
            }
          />
        </List.Item>
      )}
    />
  )

  const renderImplementationGuide = () => (
    <Timeline>
      {filteredSuggestions
        .filter(s => s.priority === 'high')
        .slice(0, 5) // Show top 5 high priority items
        .map((suggestion, index) => (
          <Timeline.Item
            key={suggestion.id}
            color={getPriorityColor(suggestion.priority)}
            dot={getCategoryIcon(suggestion.category)}
          >
            <Card size="small">
              <Space direction="vertical" style={{ width: '100%' }}>
                <Space>
                  <Text strong>{suggestion.title}</Text>
                  <Tag color={getComplexityColor(suggestion.implementationComplexity)}>
                    {suggestion.implementationComplexity}
                  </Tag>
                </Space>
                <Paragraph style={{ margin: 0 }}>
                  {suggestion.description}
                </Paragraph>
                {suggestion.implementationSteps && (
                  <div>
                    <Text strong style={{ fontSize: '12px' }}>Implementation Steps:</Text>
                    <ol style={{ margin: '4px 0 0 16px', fontSize: '12px' }}>
                      {suggestion.implementationSteps.map((step, stepIndex) => (
                        <li key={stepIndex}>{step}</li>
                      ))}
                    </ol>
                  </div>
                )}
              </Space>
            </Card>
          </Timeline.Item>
        ))}
    </Timeline>
  )

  if (!suggestions || suggestions.length === 0) {
    return (
      <Card className={className} data-testid={testId}>
        <div style={{ textAlign: 'center', padding: '40px 0' }}>
          <Space direction="vertical">
            <BulbOutlined style={{ fontSize: '48px', color: '#d9d9d9' }} />
            <Text type="secondary">No optimization suggestions available</Text>
          </Space>
        </div>
      </Card>
    )
  }

  return (
    <Card 
      title={
        <Space>
          <BulbOutlined />
          <span>Optimization Insights</span>
          <Tag color="blue">{analysis.totalSuggestions} suggestions</Tag>
        </Space>
      }
      className={className}
      data-testid={testId}
    >
      <Collapse 
        activeKey={expandedPanels}
        onChange={setExpandedPanels}
        ghost
      >
        <Panel header="Overview" key="overview">
          {renderOverview()}
        </Panel>
        
        <Panel header={`Suggestions (${filteredSuggestions.length})`} key="suggestions">
          {renderSuggestionsList()}
        </Panel>
        
        {showImplementationGuide && (
          <Panel header="Implementation Guide" key="implementation">
            {renderImplementationGuide()}
          </Panel>
        )}
      </Collapse>

      {analysis.highPriority > 0 && (
        <Alert
          message={`${analysis.highPriority} High Priority Optimization${analysis.highPriority > 1 ? 's' : ''} Available`}
          description="These optimizations can significantly improve your AI system performance. Consider implementing them soon."
          type="warning"
          showIcon
          style={{ marginTop: 16 }}
        />
      )}
    </Card>
  )
}

export default OptimizationInsights
