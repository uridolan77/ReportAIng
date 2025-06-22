import React, { useState, useMemo } from 'react'
import { 
  Card, 
  Timeline, 
  Space, 
  Typography, 
  Tag, 
  Progress, 
  Collapse, 
  Button, 
  Tooltip, 
  Divider,
  Row,
  Col,
  Statistic,
  Alert
} from 'antd'
import {
  PlayCircleOutlined,
  CheckCircleOutlined,
  ExclamationCircleOutlined,
  ClockCircleOutlined,
  BranchesOutlined,
  ThunderboltOutlined,
  EyeOutlined,
  BarChartOutlined,
  BulbOutlined
} from '@ant-design/icons'
import { ConfidenceIndicator } from '../common/ConfidenceIndicator'
import type { TransparencyTrace, PromptConstructionStep } from '@shared/types/transparency'

const { Title, Text, Paragraph } = Typography
const { Panel } = Collapse

export interface QueryFlowAnalyzerProps {
  trace: TransparencyTrace
  showDetailedSteps?: boolean
  showPerformanceMetrics?: boolean
  showConfidenceBreakdown?: boolean
  compact?: boolean
  onStepSelect?: (step: PromptConstructionStep) => void
  className?: string
  testId?: string
}

interface FlowStep {
  id: string
  name: string
  type: 'input' | 'processing' | 'decision' | 'output' | 'optimization'
  status: 'pending' | 'running' | 'completed' | 'failed' | 'warning'
  confidence: number
  duration: number
  tokens: number
  description: string
  details?: any
  children?: FlowStep[]
}

/**
 * QueryFlowAnalyzer - Comprehensive component for analyzing query execution flow
 * 
 * Features:
 * - Visual flow representation with timeline
 * - Step-by-step analysis with confidence tracking
 * - Performance metrics and token usage
 * - Interactive exploration of each step
 * - Hierarchical view of decision trees
 * - Real-time progress tracking
 */
export const QueryFlowAnalyzer: React.FC<QueryFlowAnalyzerProps> = ({
  trace,
  showDetailedSteps = true,
  showPerformanceMetrics = true,
  showConfidenceBreakdown = true,
  compact = false,
  onStepSelect,
  className,
  testId = 'query-flow-analyzer'
}) => {
  const [selectedStep, setSelectedStep] = useState<string | null>(null)
  const [expandedPanels, setExpandedPanels] = useState<string[]>(['overview'])

  // Transform trace steps into flow steps
  const flowSteps = useMemo((): FlowStep[] => {
    if (!trace?.steps) return []

    return trace.steps.map((step, index) => ({
      id: step.id,
      name: step.stepName,
      type: getStepType(step.stepName),
      status: step.success ? 'completed' : 'failed',
      confidence: step.confidence,
      duration: step.processingTimeMs,
      tokens: step.tokensAdded,
      description: step.content.substring(0, 200) + (step.content.length > 200 ? '...' : ''),
      details: step
    }))
  }, [trace?.steps])

  // Calculate overall metrics
  const metrics = useMemo(() => {
    if (!flowSteps.length) return null

    const totalDuration = flowSteps.reduce((sum, step) => sum + step.duration, 0)
    const totalTokens = flowSteps.reduce((sum, step) => sum + step.tokens, 0)
    const avgConfidence = flowSteps.reduce((sum, step) => sum + step.confidence, 0) / flowSteps.length
    const successRate = flowSteps.filter(step => step.status === 'completed').length / flowSteps.length

    return {
      totalDuration,
      totalTokens,
      avgConfidence,
      successRate,
      stepCount: flowSteps.length
    }
  }, [flowSteps])

  function getStepType(stepName: string): FlowStep['type'] {
    const name = stepName.toLowerCase()
    if (name.includes('input') || name.includes('parse')) return 'input'
    if (name.includes('decision') || name.includes('classify')) return 'decision'
    if (name.includes('optimize') || name.includes('improve')) return 'optimization'
    if (name.includes('output') || name.includes('format')) return 'output'
    return 'processing'
  }

  function getStepIcon(step: FlowStep) {
    switch (step.type) {
      case 'input': return <PlayCircleOutlined />
      case 'decision': return <BranchesOutlined />
      case 'optimization': return <BulbOutlined />
      case 'output': return <CheckCircleOutlined />
      default: return <ThunderboltOutlined />
    }
  }

  function getStepColor(step: FlowStep) {
    if (step.status === 'failed') return '#ff4d4f'
    if (step.status === 'warning') return '#faad14'
    if (step.confidence < 0.7) return '#faad14'
    return '#52c41a'
  }

  const handleStepClick = (step: FlowStep) => {
    setSelectedStep(step.id)
    onStepSelect?.(step.details)
  }

  const renderOverview = () => (
    <Row gutter={[16, 16]}>
      <Col xs={24} sm={12} lg={6}>
        <Statistic
          title="Total Steps"
          value={metrics?.stepCount || 0}
          prefix={<BranchesOutlined />}
        />
      </Col>
      <Col xs={24} sm={12} lg={6}>
        <Statistic
          title="Total Duration"
          value={metrics?.totalDuration || 0}
          suffix="ms"
          prefix={<ClockCircleOutlined />}
        />
      </Col>
      <Col xs={24} sm={12} lg={6}>
        <Statistic
          title="Total Tokens"
          value={metrics?.totalTokens || 0}
          prefix={<BarChartOutlined />}
        />
      </Col>
      <Col xs={24} sm={12} lg={6}>
        <Statistic
          title="Success Rate"
          value={((metrics?.successRate || 0) * 100).toFixed(1)}
          suffix="%"
          prefix={<CheckCircleOutlined />}
          valueStyle={{ color: (metrics?.successRate || 0) > 0.9 ? '#3f8600' : '#cf1322' }}
        />
      </Col>
    </Row>
  )

  const renderFlowTimeline = () => (
    <Timeline>
      {flowSteps.map((step, index) => (
        <Timeline.Item
          key={step.id}
          color={getStepColor(step)}
          dot={getStepIcon(step)}
        >
          <Card 
            size="small" 
            className={`flow-step-card ${selectedStep === step.id ? 'selected' : ''}`}
            onClick={() => handleStepClick(step)}
            style={{ 
              cursor: 'pointer',
              border: selectedStep === step.id ? '2px solid #1890ff' : undefined
            }}
          >
            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start' }}>
              <div style={{ flex: 1 }}>
                <Space direction="vertical" size="small" style={{ width: '100%' }}>
                  <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                    <Text strong>{step.name}</Text>
                    <Space>
                      <ConfidenceIndicator
                        confidence={step.confidence}
                        size="small"
                        type="badge"
                      />
                      <Tag color={step.type === 'decision' ? 'purple' : 'blue'}>
                        {step.type}
                      </Tag>
                    </Space>
                  </div>
                  
                  {showDetailedSteps && (
                    <Paragraph 
                      ellipsis={{ rows: 2, expandable: true }}
                      style={{ margin: 0, fontSize: '12px' }}
                      type="secondary"
                    >
                      {step.description}
                    </Paragraph>
                  )}
                  
                  {showPerformanceMetrics && (
                    <Space size="large">
                      <Space size="small">
                        <ClockCircleOutlined style={{ fontSize: '12px' }} />
                        <Text style={{ fontSize: '12px' }}>{step.duration}ms</Text>
                      </Space>
                      <Space size="small">
                        <BarChartOutlined style={{ fontSize: '12px' }} />
                        <Text style={{ fontSize: '12px' }}>{step.tokens} tokens</Text>
                      </Space>
                    </Space>
                  )}
                </Space>
              </div>
            </div>
          </Card>
        </Timeline.Item>
      ))}
    </Timeline>
  )

  const renderConfidenceBreakdown = () => (
    <Space direction="vertical" style={{ width: '100%' }}>
      <Title level={5}>Confidence Progression</Title>
      {flowSteps.map((step, index) => (
        <div key={step.id} style={{ marginBottom: 8 }}>
          <div style={{ display: 'flex', justifyContent: 'space-between', marginBottom: 4 }}>
            <Text style={{ fontSize: '12px' }}>{step.name}</Text>
            <Text style={{ fontSize: '12px' }}>{(step.confidence * 100).toFixed(1)}%</Text>
          </div>
          <Progress
            percent={step.confidence * 100}
            strokeColor={getStepColor(step)}
            showInfo={false}
            size="small"
          />
        </div>
      ))}
    </Space>
  )

  if (!trace || !flowSteps.length) {
    return (
      <Card className={className} data-testid={testId}>
        <div style={{ textAlign: 'center', padding: '40px 0' }}>
          <Space direction="vertical">
            <EyeOutlined style={{ fontSize: '48px', color: '#d9d9d9' }} />
            <Text type="secondary">No query flow data available</Text>
          </Space>
        </div>
      </Card>
    )
  }

  return (
    <Card 
      title={
        <Space>
          <BranchesOutlined />
          <span>Query Flow Analysis</span>
          <Tag color="blue">{trace.traceId.substring(0, 8)}...</Tag>
        </Space>
      }
      className={className}
      data-testid={testId}
      size={compact ? 'small' : 'default'}
    >
      <Collapse 
        activeKey={expandedPanels}
        onChange={setExpandedPanels}
        ghost
      >
        <Panel header="Overview Metrics" key="overview">
          {renderOverview()}
        </Panel>
        
        <Panel header="Flow Timeline" key="timeline">
          {renderFlowTimeline()}
        </Panel>
        
        {showConfidenceBreakdown && (
          <Panel header="Confidence Breakdown" key="confidence">
            {renderConfidenceBreakdown()}
          </Panel>
        )}
      </Collapse>
      
      {metrics && metrics.successRate < 0.8 && (
        <Alert
          message="Low Success Rate Detected"
          description="Some steps in this query flow had issues. Consider reviewing the failed steps for optimization opportunities."
          type="warning"
          showIcon
          style={{ marginTop: 16 }}
        />
      )}
    </Card>
  )
}

export default QueryFlowAnalyzer
