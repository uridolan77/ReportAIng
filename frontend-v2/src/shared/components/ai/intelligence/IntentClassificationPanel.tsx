import React, { useState, useMemo } from 'react'
import { 
  Card, 
  Row, 
  Col, 
  Typography, 
  Space, 
  Tag, 
  Progress,
  List,
  Tooltip,
  Button,
  Alert,
  Collapse,
  Badge,
  Divider
} from 'antd'
import {
  BulbOutlined,
  AimOutlined,
  QuestionCircleOutlined,
  CheckCircleOutlined,
  ExclamationCircleOutlined,
  InfoCircleOutlined,
  ThunderboltOutlined,
  EyeOutlined,
  BarChartOutlined
} from '@ant-design/icons'
import { PieChart, Pie, Cell, BarChart, Bar, XAxis, YAxis, CartesianGrid, Tooltip as RechartsTooltip, ResponsiveContainer } from 'recharts'
import { ConfidenceIndicator } from '../common/ConfidenceIndicator'
import type { QueryIntent, IntentAlternative, IntentConfidenceBreakdown } from '@shared/types/ai'

const { Title, Text, Paragraph } = Typography

export interface IntentClassificationPanelProps {
  intent: QueryIntent
  alternatives?: IntentAlternative[]
  confidenceBreakdown?: IntentConfidenceBreakdown
  showAlternatives?: boolean
  showConfidenceBreakdown?: boolean
  showRecommendations?: boolean
  interactive?: boolean
  onIntentSelect?: (intent: QueryIntent | IntentAlternative) => void
  onExploreIntent?: (intentType: string) => void
  className?: string
  testId?: string
}

/**
 * IntentClassificationPanel - Displays AI intent classification analysis
 * 
 * Features:
 * - Primary intent display with confidence scoring
 * - Alternative intent suggestions with comparison
 * - Confidence breakdown visualization
 * - Intent reasoning and explanation
 * - Interactive intent exploration
 * - Business goal alignment indicators
 */
export const IntentClassificationPanel: React.FC<IntentClassificationPanelProps> = ({
  intent,
  alternatives = [],
  confidenceBreakdown,
  showAlternatives = true,
  showConfidenceBreakdown = true,
  showRecommendations = true,
  interactive = true,
  onIntentSelect,
  onExploreIntent,
  className,
  testId = 'intent-classification-panel'
}) => {
  const [selectedAlternative, setSelectedAlternative] = useState<IntentAlternative | null>(null)
  const [expandedSections, setExpandedSections] = useState<string[]>(['primary'])

  // Get intent type color
  const getIntentTypeColor = (type: string) => {
    const colors = {
      data_retrieval: '#1890ff',
      aggregation: '#52c41a',
      comparison: '#fa8c16',
      trend_analysis: '#722ed1',
      exploration: '#13c2c2',
      filtering: '#eb2f96',
      ranking: '#faad14',
      calculation: '#f759ab'
    }
    return colors[type as keyof typeof colors] || '#d9d9d9'
  }

  // Get complexity level color
  const getComplexityColor = (complexity: string) => {
    switch (complexity) {
      case 'simple': return '#52c41a'
      case 'moderate': return '#faad14'
      case 'complex': return '#ff7875'
      case 'very_complex': return '#ff4d4f'
      default: return '#d9d9d9'
    }
  }

  // Get confidence level description
  const getConfidenceDescription = (confidence: number) => {
    if (confidence >= 0.9) return 'Very High'
    if (confidence >= 0.8) return 'High'
    if (confidence >= 0.7) return 'Moderate'
    if (confidence >= 0.6) return 'Low'
    return 'Very Low'
  }

  // Prepare chart data for confidence breakdown
  const confidenceChartData = useMemo(() => {
    if (!confidenceBreakdown) return []
    
    return confidenceBreakdown.factors.map(factor => ({
      name: factor.factor,
      value: factor.score * 100,
      impact: factor.impact
    }))
  }, [confidenceBreakdown])

  // Prepare pie chart data for intent distribution
  const intentDistributionData = useMemo(() => {
    const data = [
      { name: intent.type, value: intent.confidence * 100, color: getIntentTypeColor(intent.type) }
    ]
    
    alternatives.slice(0, 3).forEach(alt => {
      data.push({
        name: alt.type,
        value: alt.confidence * 100,
        color: getIntentTypeColor(alt.type)
      })
    })
    
    return data
  }, [intent, alternatives])

  // Handle alternative selection
  const handleAlternativeSelect = (alternative: IntentAlternative) => {
    setSelectedAlternative(alternative)
    onIntentSelect?.(alternative)
  }

  // Render primary intent
  const renderPrimaryIntent = () => (
    <Card 
      size="small" 
      title={
        <Space>
          <AimOutlined />
          <span>Primary Intent</span>
          <ConfidenceIndicator
            confidence={intent.confidence}
            size="small"
            type="badge"
          />
        </Space>
      }
      extra={
        interactive && (
          <Button
            type="link"
            size="small"
            onClick={() => onExploreIntent?.(intent.type)}
          >
            Explore
          </Button>
        )
      }
    >
      <Space direction="vertical" style={{ width: '100%' }}>
        {/* Intent Type and Confidence */}
        <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
          <Space>
            <Tag color={getIntentTypeColor(intent.type)} style={{ fontSize: '14px', padding: '4px 8px' }}>
              {intent.type.replace('_', ' ').toUpperCase()}
            </Tag>
            <Tag color={getComplexityColor(intent.complexity)}>
              {intent.complexity.toUpperCase()}
            </Tag>
          </Space>
          <div style={{ textAlign: 'right' }}>
            <div style={{ fontSize: '12px', color: '#666' }}>
              {getConfidenceDescription(intent.confidence)}
            </div>
            <ConfidenceIndicator
              confidence={intent.confidence}
              size="medium"
              type="circle"
              showPercentage={false}
            />
          </div>
        </div>

        {/* Description */}
        <div>
          <Text strong>Description:</Text>
          <Paragraph style={{ marginTop: 4, marginBottom: 8 }}>
            {intent.description}
          </Paragraph>
        </div>

        {/* Business Goal */}
        {intent.businessGoal && (
          <div>
            <Text strong>Business Goal:</Text>
            <Paragraph style={{ marginTop: 4, marginBottom: 8 }}>
              {intent.businessGoal}
            </Paragraph>
          </div>
        )}

        {/* Sub-intents */}
        {intent.subIntents && intent.subIntents.length > 0 && (
          <div>
            <Text strong>Sub-intents:</Text>
            <div style={{ marginTop: 4 }}>
              <Space wrap>
                {intent.subIntents.map((subIntent, index) => (
                  <Tag key={index} color="geekblue">{subIntent}</Tag>
                ))}
              </Space>
            </div>
          </div>
        )}

        {/* Reasoning */}
        {intent.reasoning && intent.reasoning.length > 0 && (
          <div>
            <Text strong>AI Reasoning:</Text>
            <List
              size="small"
              dataSource={intent.reasoning}
              renderItem={(reason) => (
                <List.Item style={{ padding: '4px 0' }}>
                  <Text style={{ fontSize: '13px' }}>• {reason}</Text>
                </List.Item>
              )}
            />
          </div>
        )}
      </Space>
    </Card>
  )

  // Render alternative intents
  const renderAlternatives = () => {
    if (!showAlternatives || alternatives.length === 0) return null

    return (
      <Card 
        size="small" 
        title={
          <Space>
            <QuestionCircleOutlined />
            <span>Alternative Interpretations</span>
            <Badge count={alternatives.length} />
          </Space>
        }
      >
        <List
          size="small"
          dataSource={alternatives}
          renderItem={(alternative) => (
            <List.Item
              style={{ 
                cursor: interactive ? 'pointer' : 'default',
                backgroundColor: selectedAlternative?.id === alternative.id ? '#f0f8ff' : 'transparent',
                borderRadius: 4,
                padding: 8
              }}
              onClick={() => interactive && handleAlternativeSelect(alternative)}
            >
              <List.Item.Meta
                title={
                  <Space>
                    <Tag color={getIntentTypeColor(alternative.type)}>
                      {alternative.type.replace('_', ' ').toUpperCase()}
                    </Tag>
                    <ConfidenceIndicator
                      confidence={alternative.confidence}
                      size="small"
                      type="badge"
                      showPercentage={false}
                    />
                  </Space>
                }
                description={
                  <div>
                    <Text style={{ fontSize: '13px' }}>{alternative.description}</Text>
                    {alternative.reasoning && (
                      <div style={{ marginTop: 4, fontSize: '12px', color: '#666' }}>
                        Reasoning: {alternative.reasoning}
                      </div>
                    )}
                  </div>
                }
              />
              {interactive && (
                <Button type="text" size="small">
                  Select
                </Button>
              )}
            </List.Item>
          )}
        />
      </Card>
    )
  }

  // Render confidence breakdown
  const renderConfidenceBreakdown = () => {
    if (!showConfidenceBreakdown || !confidenceBreakdown) return null

    return (
      <Card 
        size="small" 
        title={
          <Space>
            <BarChartOutlined />
            <span>Confidence Analysis</span>
          </Space>
        }
      >
        <Row gutter={[16, 16]}>
          <Col xs={24} lg={12}>
            <div>
              <Text strong style={{ marginBottom: 8, display: 'block' }}>
                Confidence Factors:
              </Text>
              <Space direction="vertical" style={{ width: '100%' }}>
                {confidenceBreakdown.factors.map((factor, index) => (
                  <div key={index}>
                    <div style={{ display: 'flex', justifyContent: 'space-between', marginBottom: 4 }}>
                      <Text style={{ fontSize: '13px' }}>{factor.factor}</Text>
                      <Space>
                        <Tag 
                          color={factor.impact === 'high' ? 'red' : factor.impact === 'medium' ? 'orange' : 'green'}
                          size="small"
                        >
                          {factor.impact}
                        </Tag>
                        <Text style={{ fontSize: '12px' }}>{(factor.score * 100).toFixed(0)}%</Text>
                      </Space>
                    </div>
                    <Progress
                      percent={factor.score * 100}
                      size="small"
                      strokeColor={factor.impact === 'high' ? '#ff4d4f' : factor.impact === 'medium' ? '#faad14' : '#52c41a'}
                      showInfo={false}
                    />
                  </div>
                ))}
              </Space>
            </div>
          </Col>
          <Col xs={24} lg={12}>
            <div>
              <Text strong style={{ marginBottom: 8, display: 'block' }}>
                Intent Distribution:
              </Text>
              <ResponsiveContainer width="100%" height={200}>
                <PieChart>
                  <Pie
                    data={intentDistributionData}
                    cx="50%"
                    cy="50%"
                    outerRadius={60}
                    dataKey="value"
                    label={({ name, value }) => `${name}: ${value.toFixed(0)}%`}
                  >
                    {intentDistributionData.map((entry, index) => (
                      <Cell key={`cell-${index}`} fill={entry.color} />
                    ))}
                  </Pie>
                  <RechartsTooltip />
                </PieChart>
              </ResponsiveContainer>
            </div>
          </Col>
        </Row>
      </Card>
    )
  }

  // Render recommendations
  const renderRecommendations = () => {
    if (!showRecommendations || !intent.recommendations || intent.recommendations.length === 0) {
      return null
    }

    return (
      <Card 
        size="small" 
        title={
          <Space>
            <BulbOutlined />
            <span>AI Recommendations</span>
          </Space>
        }
      >
        <List
          size="small"
          dataSource={intent.recommendations}
          renderItem={(recommendation) => (
            <List.Item>
              <Alert
                message={recommendation.title}
                description={recommendation.description}
                type={recommendation.type === 'optimization' ? 'info' : 'warning'}
                showIcon
                style={{ width: '100%' }}
              />
            </List.Item>
          )}
        />
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
          Intent Classification
        </Title>
        <Space>
          <Text type="secondary">
            Overall Confidence: {(intent.confidence * 100).toFixed(1)}%
          </Text>
          {interactive && (
            <Button
              size="small"
              icon={<EyeOutlined />}
              onClick={() => setExpandedSections(
                expandedSections.length === 0 ? ['primary', 'alternatives', 'breakdown'] : []
              )}
            >
              {expandedSections.length === 0 ? 'Expand All' : 'Collapse All'}
            </Button>
          )}
        </Space>
      </div>

      {/* Main Content */}
      <Space direction="vertical" style={{ width: '100%' }}>
        {/* Primary Intent */}
        {renderPrimaryIntent()}

        {/* Alternative Intents */}
        {renderAlternatives()}

        {/* Confidence Breakdown */}
        {renderConfidenceBreakdown()}

        {/* Recommendations */}
        {renderRecommendations()}
      </Space>
    </div>
  )
}

export default IntentClassificationPanel
