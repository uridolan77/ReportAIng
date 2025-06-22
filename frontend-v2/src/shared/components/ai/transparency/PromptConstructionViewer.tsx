import React, { useState, useMemo } from 'react'
import { 
  Timeline, 
  Card, 
  Tag, 
  Tooltip, 
  Button, 
  Space, 
  Typography, 
  Collapse,
  Progress,
  List,
  Divider
} from 'antd'
import {
  CheckCircleOutlined,
  ClockCircleOutlined,
  InfoCircleOutlined,
  EyeOutlined,
  EyeInvisibleOutlined,
  CopyOutlined,
  ExpandAltOutlined
} from '@ant-design/icons'
import { ConfidenceIndicator } from '../common/ConfidenceIndicator'
import type { PromptConstructionViewerProps, PromptStepVisualization } from './types'

const { Text, Paragraph, Title } = Typography
const { Panel } = Collapse

/**
 * PromptConstructionViewer - Visualizes step-by-step prompt construction
 * 
 * Features:
 * - Timeline visualization of prompt building steps
 * - Interactive step exploration
 * - Context inclusion rationale
 * - Alternative approaches display
 * - Copy functionality for prompts
 * - Expandable detailed view
 */
export const PromptConstructionViewer: React.FC<PromptConstructionViewerProps> = ({
  trace,
  interactive = true,
  showTimeline = true,
  onStepClick,
  className,
  testId = 'prompt-construction-viewer'
}) => {
  const [expandedSteps, setExpandedSteps] = useState<Set<number>>(new Set())
  const [selectedStep, setSelectedStep] = useState<number | null>(null)
  const [showAlternatives, setShowAlternatives] = useState(false)

  // Convert trace steps to visualization format
  const visualizationSteps: PromptStepVisualization[] = useMemo(() => {
    return trace.steps.map((step, index) => ({
      step: index + 1,
      title: step.stepName,
      description: step.description,
      confidence: step.confidence,
      status: 'complete',
      duration: index * 200 + 150, // Mock duration
      details: {
        context: step.context,
        alternatives: step.alternatives,
        reasoning: step.reasoning,
        timestamp: step.timestamp
      }
    }))
  }, [trace.steps])

  // Handle step expansion
  const toggleStepExpansion = (stepIndex: number) => {
    const newExpanded = new Set(expandedSteps)
    if (newExpanded.has(stepIndex)) {
      newExpanded.delete(stepIndex)
    } else {
      newExpanded.add(stepIndex)
    }
    setExpandedSteps(newExpanded)
  }

  // Handle step selection
  const handleStepClick = (stepIndex: number) => {
    setSelectedStep(stepIndex)
    onStepClick?.(stepIndex)
  }

  // Copy prompt to clipboard
  const copyPrompt = async () => {
    try {
      await navigator.clipboard.writeText(trace.finalPrompt)
      // Could add a toast notification here
    } catch (err) {
      console.error('Failed to copy prompt:', err)
    }
  }

  // Get confidence color for step
  const getStepColor = (confidence: number) => {
    if (confidence >= 0.8) return 'green'
    if (confidence >= 0.6) return 'orange'
    return 'red'
  }

  // Render step content
  const renderStepContent = (step: PromptStepVisualization, index: number) => {
    const isExpanded = expandedSteps.has(index)
    const isSelected = selectedStep === index

    return (
      <div 
        key={index}
        style={{ 
          cursor: interactive ? 'pointer' : 'default',
          padding: '8px',
          borderRadius: '4px',
          backgroundColor: isSelected ? '#f0f8ff' : 'transparent',
          border: isSelected ? '1px solid #1890ff' : '1px solid transparent'
        }}
        onClick={() => interactive && handleStepClick(index)}
      >
        <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
          <div style={{ flex: 1 }}>
            <Space>
              <Text strong>{step.title}</Text>
              <ConfidenceIndicator 
                confidence={step.confidence}
                size="small"
                type="badge"
                showPercentage={false}
              />
              {step.duration && (
                <Text type="secondary" style={{ fontSize: '12px' }}>
                  {step.duration}ms
                </Text>
              )}
            </Space>
            <div style={{ marginTop: 4 }}>
              <Text type="secondary">{step.description}</Text>
            </div>
          </div>
          {interactive && (
            <Button
              type="text"
              size="small"
              icon={isExpanded ? <EyeInvisibleOutlined /> : <EyeOutlined />}
              onClick={(e) => {
                e.stopPropagation()
                toggleStepExpansion(index)
              }}
            />
          )}
        </div>

        {isExpanded && step.details && (
          <div style={{ marginTop: 16, paddingTop: 16, borderTop: '1px solid #f0f0f0' }}>
            <Space direction="vertical" style={{ width: '100%' }}>
              {/* Context Information */}
              {step.details.context && (
                <div>
                  <Text strong>Context Used:</Text>
                  <div style={{ marginTop: 4 }}>
                    <Space wrap>
                      {step.details.context.map((ctx, i) => (
                        <Tag key={i} color="blue">{ctx}</Tag>
                      ))}
                    </Space>
                  </div>
                </div>
              )}

              {/* Reasoning */}
              {step.details.reasoning && (
                <div>
                  <Text strong>Reasoning:</Text>
                  <Paragraph style={{ marginTop: 4, marginBottom: 0 }}>
                    {step.details.reasoning}
                  </Paragraph>
                </div>
              )}

              {/* Alternatives */}
              {step.details.alternatives && step.details.alternatives.length > 0 && (
                <div>
                  <Text strong>Alternative Approaches:</Text>
                  <List
                    size="small"
                    dataSource={step.details.alternatives}
                    renderItem={(alt, i) => (
                      <List.Item key={i}>
                        <Text type="secondary">• {alt}</Text>
                      </List.Item>
                    )}
                  />
                </div>
              )}
            </Space>
          </div>
        )}
      </div>
    )
  }

  // Render timeline view
  const renderTimelineView = () => {
    const timelineItems = visualizationSteps.map((step, index) => ({
      dot: (
        <Tooltip title={`Confidence: ${(step.confidence * 100).toFixed(1)}%`}>
          <CheckCircleOutlined 
            style={{ 
              color: getStepColor(step.confidence),
              fontSize: '16px'
            }} 
          />
        </Tooltip>
      ),
      color: getStepColor(step.confidence),
      children: renderStepContent(step, index)
    }))

    return (
      <Timeline items={timelineItems} />
    )
  }

  // Render card view (compact)
  const renderCardView = () => {
    return (
      <Space direction="vertical" style={{ width: '100%' }}>
        {visualizationSteps.map((step, index) => (
          <Card 
            key={index}
            size="small"
            style={{ 
              borderColor: selectedStep === index ? '#1890ff' : undefined,
              cursor: interactive ? 'pointer' : 'default'
            }}
            onClick={() => interactive && handleStepClick(index)}
          >
            {renderStepContent(step, index)}
          </Card>
        ))}
      </Space>
    )
  }

  return (
    <div className={className} data-testid={testId}>
      {/* Header with controls */}
      <div style={{ 
        display: 'flex', 
        justifyContent: 'space-between', 
        alignItems: 'center',
        marginBottom: 16 
      }}>
        <Space>
          <Title level={5} style={{ margin: 0 }}>
            Prompt Construction Steps
          </Title>
          <Text type="secondary">
            ({visualizationSteps.length} steps)
          </Text>
        </Space>
        <Space>
          {interactive && (
            <Button
              size="small"
              type={showAlternatives ? 'primary' : 'default'}
              onClick={() => setShowAlternatives(!showAlternatives)}
            >
              {showAlternatives ? 'Hide' : 'Show'} Alternatives
            </Button>
          )}
          <Tooltip title="Copy final prompt">
            <Button
              size="small"
              icon={<CopyOutlined />}
              onClick={copyPrompt}
            >
              Copy Prompt
            </Button>
          </Tooltip>
        </Space>
      </div>

      {/* Overall progress */}
      <div style={{ marginBottom: 24 }}>
        <div style={{ display: 'flex', justifyContent: 'space-between', marginBottom: 8 }}>
          <Text strong>Overall Confidence</Text>
          <Text>{(trace.totalConfidence * 100).toFixed(1)}%</Text>
        </div>
        <Progress 
          percent={trace.totalConfidence * 100}
          strokeColor={getStepColor(trace.totalConfidence)}
          showInfo={false}
        />
      </div>

      {/* Steps visualization */}
      {showTimeline ? renderTimelineView() : renderCardView()}

      {/* Final prompt display */}
      <Divider />
      <div>
        <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 8 }}>
          <Text strong>Final Prompt</Text>
          <Space>
            <Text type="secondary">
              {trace.metadata.tokensUsed} tokens
            </Text>
            <Text type="secondary">
              {trace.metadata.processingTime}ms
            </Text>
          </Space>
        </div>
        <Card size="small" style={{ backgroundColor: '#fafafa' }}>
          <Paragraph 
            copyable={{ text: trace.finalPrompt }}
            style={{ 
              fontFamily: 'monospace',
              fontSize: '12px',
              marginBottom: 0,
              maxHeight: '200px',
              overflow: 'auto'
            }}
          >
            {trace.finalPrompt}
          </Paragraph>
        </Card>
      </div>

      {/* Optimization suggestions */}
      {trace.optimizationSuggestions.length > 0 && (
        <>
          <Divider />
          <div>
            <Text strong>Optimization Suggestions</Text>
            <List
              size="small"
              dataSource={trace.optimizationSuggestions}
              renderItem={(suggestion) => (
                <List.Item>
                  <Card size="small" style={{ width: '100%' }}>
                    <Space direction="vertical" style={{ width: '100%' }}>
                      <div style={{ display: 'flex', justifyContent: 'space-between' }}>
                        <Text strong>{suggestion.title}</Text>
                        <Tag color={suggestion.impact === 'high' ? 'red' : suggestion.impact === 'medium' ? 'orange' : 'green'}>
                          {suggestion.impact} impact
                        </Tag>
                      </div>
                      <Text type="secondary">{suggestion.description}</Text>
                      <Text style={{ fontSize: '12px', fontFamily: 'monospace' }}>
                        {suggestion.implementation}
                      </Text>
                    </Space>
                  </Card>
                </List.Item>
              )}
            />
          </div>
        </>
      )}
    </div>
  )
}

export default PromptConstructionViewer
