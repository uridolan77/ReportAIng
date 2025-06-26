import React, { useState, useMemo } from 'react'
import { 
  Card, 
  Steps, 
  Typography, 
  Space, 
  Tag, 
  Button,
  Collapse,
  Timeline,
  Progress,
  Alert,
  List,
  Tooltip,
  Badge
} from 'antd'
import {
  SearchOutlined,
  BulbOutlined,
  DatabaseOutlined,
  CheckCircleOutlined,
  ClockCircleOutlined,
  ExclamationCircleOutlined,
  EyeOutlined,
  ArrowRightOutlined,
  ThunderboltOutlined,
  TagsOutlined
} from '@ant-design/icons'
import { ConfidenceIndicator } from '../common/ConfidenceIndicator'
import { EntityHighlighter } from './EntityHighlighter'
import type { QueryUnderstandingResult, QueryProcessingStep, BusinessEntity } from '@shared/types/ai'

const { Title, Text, Paragraph } = Typography

const { Step } = Steps

export interface QueryUnderstandingFlowProps {
  understanding: QueryUnderstandingResult
  showStepDetails?: boolean
  showEntityHighlighting?: boolean
  interactive?: boolean
  onStepClick?: (step: QueryProcessingStep) => void
  onEntityClick?: (entity: BusinessEntity) => void
  className?: string
  testId?: string
}

/**
 * QueryUnderstandingFlow - Visualizes AI query understanding process
 * 
 * Features:
 * - Step-by-step query processing visualization
 * - Interactive entity highlighting in original query
 * - Confidence tracking throughout the process
 * - Alternative interpretations display
 * - Processing timeline with performance metrics
 * - Expandable step details with reasoning
 */
export const QueryUnderstandingFlow: React.FC<QueryUnderstandingFlowProps> = ({
  understanding,
  showStepDetails = true,
  showEntityHighlighting = true,
  interactive = true,
  onStepClick,
  onEntityClick,
  className,
  testId = 'query-understanding-flow'
}) => {
  const [selectedStep, setSelectedStep] = useState<number>(0)
  const [expandedPanels, setExpandedPanels] = useState<string[]>(['overview'])
  const [showTimeline, setShowTimeline] = useState(false)

  // Calculate overall progress
  const overallProgress = useMemo(() => {
    const completedSteps = understanding.processingSteps.filter(step => step.status === 'completed').length
    return (completedSteps / understanding.processingSteps.length) * 100
  }, [understanding.processingSteps])

  // Get step status icon
  const getStepStatusIcon = (status: string) => {
    switch (status) {
      case 'completed': return <CheckCircleOutlined style={{ color: '#52c41a' }} />
      case 'processing': return <ClockCircleOutlined style={{ color: '#1890ff' }} />
      case 'error': return <ExclamationCircleOutlined style={{ color: '#ff4d4f' }} />
      default: return <ClockCircleOutlined style={{ color: '#d9d9d9' }} />
    }
  }

  // Get complexity color
  const getComplexityColor = (complexity: string) => {
    switch (complexity) {
      case 'simple': return '#52c41a'
      case 'moderate': return '#faad14'
      case 'complex': return '#ff7875'
      default: return '#d9d9d9'
    }
  }

  // Handle step selection
  const handleStepClick = (stepIndex: number) => {
    setSelectedStep(stepIndex)
    onStepClick?.(understanding.processingSteps[stepIndex])
  }

  // Render overview section
  const renderOverview = () => (
    <Card size="small" title="Query Understanding Overview">
      <Space direction="vertical" style={{ width: '100%' }}>
        {/* Original Query with Entity Highlighting */}
        <div>
          <Text strong>Original Query:</Text>
          <div style={{ 
            marginTop: 8, 
            padding: 12, 
            backgroundColor: '#fafafa', 
            borderRadius: 6,
            border: '1px solid #f0f0f0'
          }}>
            {showEntityHighlighting ? (
              <EntityHighlighter
                text={understanding.originalQuery}
                entities={understanding.entities}
                interactive={interactive}
                onEntityClick={onEntityClick}
              />
            ) : (
              <Text style={{ fontSize: '14px' }}>{understanding.originalQuery}</Text>
            )}
          </div>
        </div>

        {/* Understanding Summary */}
        <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
          <Space>
            <Text strong>Intent:</Text>
            <Tag color="blue">{understanding.intent.type.replace('_', ' ').toUpperCase()}</Tag>
            <Text strong>Complexity:</Text>
            <Tag color={getComplexityColor(understanding.complexity.level)}>
              {understanding.complexity.level.toUpperCase()}
            </Tag>
          </Space>
          <ConfidenceIndicator
            confidence={understanding.confidence}
            size="medium"
            type="circle"
            showLabel={true}
          />
        </div>

        {/* Processing Progress */}
        <div>
          <div style={{ display: 'flex', justifyContent: 'space-between', marginBottom: 8 }}>
            <Text strong>Processing Progress</Text>
            <Text>{Math.round(overallProgress)}% Complete</Text>
          </div>
          <Progress 
            percent={overallProgress}
            strokeColor="#1890ff"
            trailColor="#f0f0f0"
          />
        </div>

        {/* Key Metrics */}
        <div style={{ display: 'flex', justifyContent: 'space-between' }}>
          <div>
            <Text type="secondary">Processing Time:</Text>
            <Text style={{ marginLeft: 8 }}>{understanding.processingTime}ms</Text>
          </div>
          <div>
            <Text type="secondary">Entities Found:</Text>
            <Text style={{ marginLeft: 8 }}>{understanding.entities.length}</Text>
          </div>
          <div>
            <Text type="secondary">Suggestions:</Text>
            <Text style={{ marginLeft: 8 }}>{understanding.suggestions.length}</Text>
          </div>
        </div>
      </Space>
    </Card>
  )

  // Render processing steps
  const renderProcessingSteps = () => (
    <Card 
      size="small" 
      title="Processing Steps"
      extra={
        <Space>
          <Button
            size="small"
            type={showTimeline ? 'primary' : 'default'}
            onClick={() => setShowTimeline(!showTimeline)}
          >
            {showTimeline ? 'Steps View' : 'Timeline View'}
          </Button>
        </Space>
      }
    >
      {showTimeline ? renderTimeline() : renderStepsView()}
    </Card>
  )

  // Render steps view
  const renderStepsView = () => (
    <Steps
      current={selectedStep}
      direction="vertical"
      size="small"
      onChange={interactive ? handleStepClick : undefined}
    >
      {understanding.processingSteps.map((step, index) => (
        <Step
          key={index}
          title={
            <Space>
              <span>{step.name}</span>
              <ConfidenceIndicator
                confidence={step.confidence}
                size="small"
                type="badge"
                showPercentage={false}
              />
            </Space>
          }
          description={
            <div>
              <Text type="secondary">{step.description}</Text>
              {step.duration && (
                <div style={{ marginTop: 4 }}>
                  <Text type="secondary" style={{ fontSize: '12px' }}>
                    Duration: {step.duration}ms
                  </Text>
                </div>
              )}
            </div>
          }
          status={step.status === 'completed' ? 'finish' : step.status === 'error' ? 'error' : 'process'}
          icon={getStepStatusIcon(step.status)}
        />
      ))}
    </Steps>
  )

  // Render timeline view
  const renderTimeline = () => (
    <Timeline mode="left">
      {understanding.processingSteps.map((step, index) => (
        <Timeline.Item
          key={index}
          dot={getStepStatusIcon(step.status)}
          label={step.startTime && new Date(step.startTime).toLocaleTimeString()}
        >
          <Card size="small" style={{ marginBottom: 8 }}>
            <Space direction="vertical" style={{ width: '100%' }}>
              <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                <Text strong>{step.name}</Text>
                <ConfidenceIndicator
                  confidence={step.confidence}
                  size="small"
                  type="badge"
                />
              </div>
              <Text type="secondary">{step.description}</Text>
              {step.reasoning && (
                <Text style={{ fontSize: '12px', fontStyle: 'italic' }}>
                  {step.reasoning}
                </Text>
              )}
              {step.duration && (
                <Text type="secondary" style={{ fontSize: '11px' }}>
                  Completed in {step.duration}ms
                </Text>
              )}
            </Space>
          </Card>
        </Timeline.Item>
      ))}
    </Timeline>
  )

  // Render step details
  const renderStepDetails = () => {
    const step = understanding.processingSteps[selectedStep]
    if (!step) return null

    return (
      <Card size="small" title={`Step Details: ${step.name}`}>
        <Space direction="vertical" style={{ width: '100%' }}>
          <div>
            <Text strong>Status: </Text>
            <Tag color={step.status === 'completed' ? 'green' : step.status === 'error' ? 'red' : 'blue'}>
              {step.status.toUpperCase()}
            </Tag>
          </div>

          <div>
            <Text strong>Confidence: </Text>
            <ConfidenceIndicator
              confidence={step.confidence}
              size="small"
              type="bar"
              showLabel={true}
            />
          </div>

          <div>
            <Text strong>Description:</Text>
            <Paragraph style={{ marginTop: 4 }}>
              {step.description}
            </Paragraph>
          </div>

          {step.reasoning && (
            <div>
              <Text strong>Reasoning:</Text>
              <Paragraph style={{ marginTop: 4 }}>
                {step.reasoning}
              </Paragraph>
            </div>
          )}

          {step.alternatives && step.alternatives.length > 0 && (
            <div>
              <Text strong>Alternative Approaches:</Text>
              <List
                size="small"
                dataSource={step.alternatives}
                renderItem={(alt) => (
                  <List.Item>
                    <Text>• {alt}</Text>
                  </List.Item>
                )}
              />
            </div>
          )}

          {step.metadata && Object.keys(step.metadata).length > 0 && (
            <div>
              <Text strong>Technical Details:</Text>
              <div style={{ marginTop: 4 }}>
                {Object.entries(step.metadata).map(([key, value]) => (
                  <div key={key} style={{ fontSize: '12px' }}>
                    <Text type="secondary">{key}: </Text>
                    <Text>{String(value)}</Text>
                  </div>
                ))}
              </div>
            </div>
          )}
        </Space>
      </Card>
    )
  }

  // Render suggestions
  const renderSuggestions = () => {
    if (understanding.suggestions.length === 0) return null

    return (
      <Card size="small" title="AI Suggestions">
        <List
          size="small"
          dataSource={understanding.suggestions}
          renderItem={(suggestion) => (
            <List.Item>
              <List.Item.Meta
                avatar={
                  <Tag color={suggestion.impact === 'high' ? 'red' : suggestion.impact === 'medium' ? 'orange' : 'green'}>
                    {suggestion.type.toUpperCase()}
                  </Tag>
                }
                title={suggestion.title}
                description={
                  <div>
                    <Text type="secondary">{suggestion.description}</Text>
                    {suggestion.suggestedQuery && (
                      <div style={{ marginTop: 4, fontSize: '12px', fontFamily: 'monospace' }}>
                        <Text copyable={{ text: suggestion.suggestedQuery }}>
                          {suggestion.suggestedQuery}
                        </Text>
                      </div>
                    )}
                  </div>
                }
              />
              <ConfidenceIndicator
                confidence={suggestion.confidence}
                size="small"
                type="badge"
                showPercentage={false}
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
          Query Understanding Flow
        </Title>
        <Space>
          <Badge count={understanding.entities.length} size="small">
            <Button size="small" icon={<TagsOutlined />}>
              Entities
            </Button>
          </Badge>
          <Badge count={understanding.suggestions.length} size="small">
            <Button size="small" icon={<BulbOutlined />}>
              Suggestions
            </Button>
          </Badge>
        </Space>
      </div>

      {/* Main Content */}
      <Space direction="vertical" style={{ width: '100%' }}>
        {/* Overview */}
        {renderOverview()}

        {/* Processing Steps */}
        {renderProcessingSteps()}

        {/* Step Details */}
        {showStepDetails && renderStepDetails()}

        {/* Suggestions */}
        {renderSuggestions()}
      </Space>
    </div>
  )
}

export default QueryUnderstandingFlow
