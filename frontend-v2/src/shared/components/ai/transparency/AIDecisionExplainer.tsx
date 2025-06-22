import React, { useState } from 'react'
import { 
  Card, 
  Space, 
  Typography, 
  List, 
  Button, 
  Tag, 
  Collapse, 
  Alert,
  Tooltip,
  Progress,
  Divider
} from 'antd'
import {
  BulbOutlined,
  CheckCircleOutlined,
  ExclamationCircleOutlined,
  RocketOutlined,
  SwapOutlined,
  InfoCircleOutlined,
  ThunderboltOutlined,
  TrophyOutlined
} from '@ant-design/icons'
import { ConfidenceIndicator } from '../common/ConfidenceIndicator'
import type { AIDecisionExplainerProps } from './types'
import type { AlternativeOption, OptimizationSuggestion } from '@shared/types/ai'

const { Title, Text, Paragraph } = Typography
const { Panel } = Collapse

/**
 * AIDecisionExplainer - Explains AI reasoning and provides alternatives
 * 
 * Features:
 * - Clear decision explanation
 * - Step-by-step reasoning
 * - Alternative options with trade-offs
 * - Optimization suggestions
 * - Interactive recommendations
 * - Impact analysis
 */
export const AIDecisionExplainer: React.FC<AIDecisionExplainerProps> = ({
  explanation,
  showAlternatives = true,
  showRecommendations = true,
  onAlternativeSelect,
  onRecommendationApply,
  className,
  testId = 'ai-decision-explainer'
}) => {
  const [selectedAlternative, setSelectedAlternative] = useState<string | null>(null)
  const [appliedRecommendations, setAppliedRecommendations] = useState<Set<string>>(new Set())

  // Handle alternative selection
  const handleAlternativeSelect = (alternative: AlternativeOption) => {
    setSelectedAlternative(alternative.id)
    onAlternativeSelect?.(alternative)
  }

  // Handle recommendation application
  const handleRecommendationApply = (recommendation: OptimizationSuggestion) => {
    const newApplied = new Set(appliedRecommendations)
    newApplied.add(recommendation.id)
    setAppliedRecommendations(newApplied)
    onRecommendationApply?.(recommendation)
  }

  // Get impact color
  const getImpactColor = (impact: string) => {
    switch (impact) {
      case 'high': return '#ff4d4f'
      case 'medium': return '#faad14'
      case 'low': return '#52c41a'
      default: return '#d9d9d9'
    }
  }

  // Get effort color
  const getEffortColor = (effort: string) => {
    switch (effort) {
      case 'high': return '#ff4d4f'
      case 'medium': return '#faad14'
      case 'low': return '#52c41a'
      default: return '#d9d9d9'
    }
  }

  // Get type icon
  const getTypeIcon = (type: string) => {
    switch (type) {
      case 'performance': return <ThunderboltOutlined />
      case 'accuracy': return <TrophyOutlined />
      case 'cost': return <ExclamationCircleOutlined />
      case 'clarity': return <BulbOutlined />
      default: return <InfoCircleOutlined />
    }
  }

  return (
    <div className={className} data-testid={testId}>
      {/* Main Decision */}
      <Card size="small" style={{ marginBottom: 16 }}>
        <Space direction="vertical" style={{ width: '100%' }}>
          <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
            <Title level={5} style={{ margin: 0 }}>
              AI Decision
            </Title>
            <ConfidenceIndicator
              confidence={explanation.confidence}
              size="small"
              type="badge"
            />
          </div>
          
          <Alert
            message={explanation.decision}
            type="info"
            showIcon
            icon={<CheckCircleOutlined />}
          />
        </Space>
      </Card>

      {/* Reasoning Steps */}
      <Card size="small" title="Reasoning Process" style={{ marginBottom: 16 }}>
        <List
          size="small"
          dataSource={explanation.reasoning}
          renderItem={(reason, index) => (
            <List.Item>
              <Space>
                <div style={{ 
                  width: 24, 
                  height: 24, 
                  borderRadius: '50%', 
                  backgroundColor: '#1890ff',
                  color: 'white',
                  display: 'flex',
                  alignItems: 'center',
                  justifyContent: 'center',
                  fontSize: '12px',
                  fontWeight: 'bold'
                }}>
                  {index + 1}
                </div>
                <Text>{reason}</Text>
              </Space>
            </List.Item>
          )}
        />
      </Card>

      {/* Contributing Factors */}
      {explanation.factors.length > 0 && (
        <Card size="small" title="Contributing Factors" style={{ marginBottom: 16 }}>
          <Space direction="vertical" style={{ width: '100%' }}>
            {explanation.factors.map((factor, index) => (
              <div key={index} style={{ 
                padding: 12, 
                border: '1px solid #f0f0f0', 
                borderRadius: 6,
                backgroundColor: '#fafafa'
              }}>
                <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 8 }}>
                  <Text strong>{factor.name}</Text>
                  <Space>
                    <Tag color={factor.impact === 'high' ? 'red' : factor.impact === 'medium' ? 'orange' : 'green'}>
                      {factor.impact} impact
                    </Tag>
                    <ConfidenceIndicator
                      confidence={factor.score}
                      size="small"
                      type="circle"
                      showPercentage={false}
                    />
                  </Space>
                </div>
                <Text type="secondary" style={{ fontSize: '13px' }}>
                  {factor.explanation}
                </Text>
              </div>
            ))}
          </Space>
        </Card>
      )}

      {/* Alternative Options */}
      {showAlternatives && explanation.alternatives.length > 0 && (
        <Card 
          size="small" 
          title={
            <Space>
              <SwapOutlined />
              <span>Alternative Approaches</span>
            </Space>
          }
          style={{ marginBottom: 16 }}
        >
          <Space direction="vertical" style={{ width: '100%' }}>
            {explanation.alternatives.map((alternative) => (
              <Card
                key={alternative.id}
                size="small"
                style={{ 
                  cursor: 'pointer',
                  borderColor: selectedAlternative === alternative.id ? '#1890ff' : undefined
                }}
                onClick={() => handleAlternativeSelect(alternative)}
              >
                <Space direction="vertical" style={{ width: '100%' }}>
                  <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                    <Text strong>{alternative.description}</Text>
                    <ConfidenceIndicator
                      confidence={alternative.confidence}
                      size="small"
                      type="badge"
                      showPercentage={false}
                    />
                  </div>
                  
                  <Text type="secondary" style={{ fontSize: '13px' }}>
                    {alternative.reasoning}
                  </Text>

                  {/* Impact metrics */}
                  <div style={{ display: 'flex', gap: 16 }}>
                    <div style={{ flex: 1 }}>
                      <Text type="secondary" style={{ fontSize: '12px' }}>Performance</Text>
                      <Progress 
                        percent={alternative.estimatedImpact.performance * 100}
                        size="small"
                        strokeColor="#52c41a"
                        showInfo={false}
                      />
                    </div>
                    <div style={{ flex: 1 }}>
                      <Text type="secondary" style={{ fontSize: '12px' }}>Accuracy</Text>
                      <Progress 
                        percent={alternative.estimatedImpact.accuracy * 100}
                        size="small"
                        strokeColor="#1890ff"
                        showInfo={false}
                      />
                    </div>
                    <div style={{ flex: 1 }}>
                      <Text type="secondary" style={{ fontSize: '12px' }}>Cost</Text>
                      <Progress 
                        percent={alternative.estimatedImpact.cost * 100}
                        size="small"
                        strokeColor="#faad14"
                        showInfo={false}
                      />
                    </div>
                  </div>

                  {/* Trade-offs */}
                  {alternative.tradeoffs.length > 0 && (
                    <Collapse size="small" ghost>
                      <Panel header="Trade-offs" key="tradeoffs">
                        <List
                          size="small"
                          dataSource={alternative.tradeoffs}
                          renderItem={(tradeoff) => (
                            <List.Item>
                              <Text type="secondary">• {tradeoff}</Text>
                            </List.Item>
                          )}
                        />
                      </Panel>
                    </Collapse>
                  )}
                </Space>
              </Card>
            ))}
          </Space>
        </Card>
      )}

      {/* Optimization Recommendations */}
      {showRecommendations && explanation.recommendations.length > 0 && (
        <Card 
          size="small" 
          title={
            <Space>
              <RocketOutlined />
              <span>Optimization Suggestions</span>
            </Space>
          }
        >
          <Space direction="vertical" style={{ width: '100%' }}>
            {explanation.recommendations.map((recommendation) => {
              const isApplied = appliedRecommendations.has(recommendation.id)
              
              return (
                <Card
                  key={recommendation.id}
                  size="small"
                  style={{ 
                    backgroundColor: isApplied ? '#f6ffed' : undefined,
                    borderColor: isApplied ? '#52c41a' : undefined
                  }}
                >
                  <Space direction="vertical" style={{ width: '100%' }}>
                    <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                      <Space>
                        {getTypeIcon(recommendation.type)}
                        <Text strong>{recommendation.title}</Text>
                      </Space>
                      <Space>
                        <Tag color={getImpactColor(recommendation.impact)}>
                          {recommendation.impact} impact
                        </Tag>
                        <Tag color={getEffortColor(recommendation.effort)}>
                          {recommendation.effort} effort
                        </Tag>
                      </Space>
                    </div>

                    <Text type="secondary">{recommendation.description}</Text>

                    <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                      <div>
                        <Text type="secondary" style={{ fontSize: '12px' }}>
                          Expected improvement: +{(recommendation.expectedImprovement * 100).toFixed(1)}%
                        </Text>
                      </div>
                      <Button
                        size="small"
                        type={isApplied ? 'default' : 'primary'}
                        disabled={isApplied}
                        onClick={() => handleRecommendationApply(recommendation)}
                      >
                        {isApplied ? 'Applied' : 'Apply'}
                      </Button>
                    </div>

                    <Collapse size="small" ghost>
                      <Panel header="Implementation Details" key="implementation">
                        <Text style={{ fontSize: '12px', fontFamily: 'monospace' }}>
                          {recommendation.implementation}
                        </Text>
                      </Panel>
                    </Collapse>
                  </Space>
                </Card>
              )
            })}
          </Space>
        </Card>
      )}
    </div>
  )
}

export default AIDecisionExplainer
