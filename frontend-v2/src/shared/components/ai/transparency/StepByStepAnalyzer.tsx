import React, { useState, useMemo } from 'react'
import { 
  Card, 
  Steps, 
  Space, 
  Typography, 
  Tag, 
  Button, 
  Drawer, 
  Descriptions,
  Progress,
  Alert,
  Tabs,
  Row,
  Col,
  Statistic,
  Timeline,
  Tooltip
} from 'antd'
import {
  PlayCircleOutlined,
  PauseCircleOutlined,
  CheckCircleOutlined,
  ExclamationCircleOutlined,
  InfoCircleOutlined,
  ClockCircleOutlined,
  BarChartOutlined,
  EyeOutlined,
  CodeOutlined,
  BulbOutlined,
  ThunderboltOutlined
} from '@ant-design/icons'
import { ConfidenceIndicator } from '../common/ConfidenceIndicator'
import type { PromptConstructionStep } from '@shared/types/transparency'

const { Title, Text, Paragraph } = Typography
const { Step } = Steps
const { TabPane } = Tabs

export interface StepByStepAnalyzerProps {
  steps: PromptConstructionStep[]
  currentStep?: number
  showContent?: boolean
  showMetrics?: boolean
  interactive?: boolean
  onStepChange?: (stepIndex: number) => void
  onStepAnalyze?: (step: PromptConstructionStep) => void
  className?: string
  testId?: string
}

interface StepAnalysis {
  efficiency: number
  complexity: number
  impact: number
  suggestions: string[]
}

/**
 * StepByStepAnalyzer - Detailed analysis of individual prompt construction steps
 * 
 * Features:
 * - Interactive step navigation
 * - Detailed step content analysis
 * - Performance metrics per step
 * - Confidence tracking progression
 * - Step optimization suggestions
 * - Content inspection and debugging
 */
export const StepByStepAnalyzer: React.FC<StepByStepAnalyzerProps> = ({
  steps,
  currentStep = 0,
  showContent = true,
  showMetrics = true,
  interactive = true,
  onStepChange,
  onStepAnalyze,
  className,
  testId = 'step-by-step-analyzer'
}) => {
  const [selectedStep, setSelectedStep] = useState(currentStep)
  const [drawerVisible, setDrawerVisible] = useState(false)
  const [activeTab, setActiveTab] = useState('content')

  // Analyze each step for insights
  const stepAnalyses = useMemo((): StepAnalysis[] => {
    return steps.map(step => {
      const efficiency = Math.min(100, (1000 / Math.max(step.processingTimeMs, 1)) * 100)
      const complexity = Math.min(100, (step.content.length / 10))
      const impact = step.confidence * 100
      
      const suggestions: string[] = []
      if (step.processingTimeMs > 2000) suggestions.push('Consider optimizing processing time')
      if (step.confidence < 0.7) suggestions.push('Low confidence - review step logic')
      if (step.tokensAdded > 500) suggestions.push('High token usage - consider compression')
      if (!step.success) suggestions.push('Step failed - requires attention')

      return { efficiency, complexity, impact, suggestions }
    })
  }, [steps])

  // Calculate cumulative metrics
  const cumulativeMetrics = useMemo(() => {
    let cumulativeTime = 0
    let cumulativeTokens = 0
    let cumulativeConfidence = 0

    return steps.map((step, index) => {
      cumulativeTime += step.processingTimeMs
      cumulativeTokens += step.tokensAdded
      cumulativeConfidence = (cumulativeConfidence * index + step.confidence) / (index + 1)

      return {
        time: cumulativeTime,
        tokens: cumulativeTokens,
        confidence: cumulativeConfidence
      }
    })
  }, [steps])

  const handleStepClick = (stepIndex: number) => {
    setSelectedStep(stepIndex)
    onStepChange?.(stepIndex)
  }

  const handleStepAnalyze = () => {
    if (steps[selectedStep]) {
      onStepAnalyze?.(steps[selectedStep])
      setDrawerVisible(true)
    }
  }

  const getStepStatus = (step: PromptConstructionStep) => {
    if (!step.success) return 'error'
    if (step.confidence < 0.7) return 'warning'
    return 'finish'
  }

  const getStepIcon = (step: PromptConstructionStep, index: number) => {
    if (index === selectedStep && interactive) return <PlayCircleOutlined />
    if (!step.success) return <ExclamationCircleOutlined />
    return <CheckCircleOutlined />
  }

  const renderStepContent = (step: PromptConstructionStep, index: number) => (
    <Card size="small" style={{ marginTop: 16 }}>
      <Tabs activeKey={activeTab} onChange={setActiveTab} size="small">
        <TabPane tab="Content" key="content">
          <div style={{ maxHeight: 300, overflow: 'auto' }}>
            <Paragraph>
              <pre style={{ 
                whiteSpace: 'pre-wrap', 
                fontSize: '12px',
                background: '#f5f5f5',
                padding: '12px',
                borderRadius: '4px'
              }}>
                {step.content}
              </pre>
            </Paragraph>
          </div>
        </TabPane>
        
        <TabPane tab="Metrics" key="metrics">
          <Row gutter={[16, 16]}>
            <Col span={8}>
              <Statistic
                title="Processing Time"
                value={step.processingTimeMs}
                suffix="ms"
                prefix={<ClockCircleOutlined />}
              />
            </Col>
            <Col span={8}>
              <Statistic
                title="Tokens Added"
                value={step.tokensAdded}
                prefix={<BarChartOutlined />}
              />
            </Col>
            <Col span={8}>
              <Statistic
                title="Confidence"
                value={(step.confidence * 100).toFixed(1)}
                suffix="%"
                prefix={<CheckCircleOutlined />}
                valueStyle={{ 
                  color: step.confidence > 0.8 ? '#3f8600' : step.confidence > 0.6 ? '#faad14' : '#cf1322' 
                }}
              />
            </Col>
          </Row>
        </TabPane>
        
        <TabPane tab="Analysis" key="analysis">
          <Space direction="vertical" style={{ width: '100%' }}>
            <div>
              <Text strong>Efficiency Score</Text>
              <Progress 
                percent={stepAnalyses[index]?.efficiency || 0} 
                strokeColor="#52c41a"
                size="small"
              />
            </div>
            <div>
              <Text strong>Complexity Score</Text>
              <Progress 
                percent={stepAnalyses[index]?.complexity || 0} 
                strokeColor="#1890ff"
                size="small"
              />
            </div>
            <div>
              <Text strong>Impact Score</Text>
              <Progress 
                percent={stepAnalyses[index]?.impact || 0} 
                strokeColor="#722ed1"
                size="small"
              />
            </div>
            
            {stepAnalyses[index]?.suggestions.length > 0 && (
              <Alert
                message="Optimization Suggestions"
                description={
                  <ul style={{ margin: 0, paddingLeft: 16 }}>
                    {stepAnalyses[index].suggestions.map((suggestion, i) => (
                      <li key={i}>{suggestion}</li>
                    ))}
                  </ul>
                }
                type="info"
                showIcon
              />
            )}
          </Space>
        </TabPane>
      </Tabs>
    </Card>
  )

  const renderProgressTimeline = () => (
    <Timeline>
      {steps.map((step, index) => (
        <Timeline.Item
          key={step.id}
          color={step.success ? (step.confidence > 0.7 ? 'green' : 'orange') : 'red'}
          dot={
            <Tooltip title={`Step ${index + 1}: ${step.stepName}`}>
              {getStepIcon(step, index)}
            </Tooltip>
          }
        >
          <Space direction="vertical" size="small">
            <Space>
              <Text strong>{step.stepName}</Text>
              <ConfidenceIndicator
                confidence={step.confidence}
                size="small"
                type="badge"
              />
            </Space>
            <Space size="large">
              <Space size="small">
                <ClockCircleOutlined style={{ fontSize: '12px' }} />
                <Text style={{ fontSize: '12px' }}>
                  {cumulativeMetrics[index]?.time}ms total
                </Text>
              </Space>
              <Space size="small">
                <BarChartOutlined style={{ fontSize: '12px' }} />
                <Text style={{ fontSize: '12px' }}>
                  {cumulativeMetrics[index]?.tokens} tokens total
                </Text>
              </Space>
            </Space>
          </Space>
        </Timeline.Item>
      ))}
    </Timeline>
  )

  if (!steps || steps.length === 0) {
    return (
      <Card className={className} data-testid={testId}>
        <div style={{ textAlign: 'center', padding: '40px 0' }}>
          <Space direction="vertical">
            <ThunderboltOutlined style={{ fontSize: '48px', color: '#d9d9d9' }} />
            <Text type="secondary">No steps available for analysis</Text>
          </Space>
        </div>
      </Card>
    )
  }

  return (
    <div className={className} data-testid={testId}>
      <Card 
        title={
          <Space>
            <ThunderboltOutlined />
            <span>Step-by-Step Analysis</span>
            <Tag color="blue">{steps.length} steps</Tag>
          </Space>
        }
        extra={
          interactive && (
            <Space>
              <Button 
                size="small" 
                icon={<EyeOutlined />}
                onClick={handleStepAnalyze}
                disabled={!steps[selectedStep]}
              >
                Analyze
              </Button>
              <Button 
                size="small" 
                icon={<BulbOutlined />}
                onClick={() => setDrawerVisible(true)}
              >
                Insights
              </Button>
            </Space>
          )
        }
      >
        <Steps
          current={selectedStep}
          onChange={interactive ? handleStepClick : undefined}
          direction="vertical"
          size="small"
        >
          {steps.map((step, index) => (
            <Step
              key={step.id}
              title={step.stepName}
              description={
                <Space direction="vertical" size="small" style={{ width: '100%' }}>
                  <Space>
                    <ConfidenceIndicator
                      confidence={step.confidence}
                      size="small"
                      type="badge"
                    />
                    <Tag size="small">{step.processingTimeMs}ms</Tag>
                    <Tag size="small">{step.tokensAdded} tokens</Tag>
                  </Space>
                  {showContent && index === selectedStep && renderStepContent(step, index)}
                </Space>
              }
              status={getStepStatus(step)}
              icon={getStepIcon(step, index)}
            />
          ))}
        </Steps>
      </Card>

      <Drawer
        title="Step Analysis & Insights"
        placement="right"
        width={600}
        onClose={() => setDrawerVisible(false)}
        open={drawerVisible}
      >
        {steps[selectedStep] && (
          <Space direction="vertical" style={{ width: '100%' }} size="large">
            <Card title="Step Details" size="small">
              <Descriptions column={1} size="small">
                <Descriptions.Item label="Step Name">
                  {steps[selectedStep].stepName}
                </Descriptions.Item>
                <Descriptions.Item label="Order">
                  {steps[selectedStep].stepOrder}
                </Descriptions.Item>
                <Descriptions.Item label="Success">
                  {steps[selectedStep].success ? 'Yes' : 'No'}
                </Descriptions.Item>
                <Descriptions.Item label="Processing Time">
                  {steps[selectedStep].processingTimeMs}ms
                </Descriptions.Item>
                <Descriptions.Item label="Tokens Added">
                  {steps[selectedStep].tokensAdded}
                </Descriptions.Item>
                <Descriptions.Item label="Confidence">
                  <ConfidenceIndicator
                    confidence={steps[selectedStep].confidence}
                    size="small"
                    type="badge"
                    showPercentage
                  />
                </Descriptions.Item>
              </Descriptions>
            </Card>

            <Card title="Progress Timeline" size="small">
              {renderProgressTimeline()}
            </Card>
          </Space>
        )}
      </Drawer>
    </div>
  )
}

export default StepByStepAnalyzer
