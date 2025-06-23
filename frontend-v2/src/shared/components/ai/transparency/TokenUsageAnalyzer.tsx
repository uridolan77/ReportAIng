import React, { useMemo } from 'react'
import { 
  Card, 
  Space, 
  Typography, 
  Progress, 
  Row, 
  Col, 
  Statistic,
  Alert,
  Timeline,
  Tag,
  Tooltip
} from 'antd'
import {
  BarChartOutlined,
  DollarOutlined,
  ThunderboltOutlined,
  ExclamationCircleOutlined,
  CheckCircleOutlined,
  InfoCircleOutlined
} from '@ant-design/icons'
import { PieChart, Pie, Cell, BarChart, Bar, XAxis, YAxis, CartesianGrid, Tooltip as RechartsTooltip, ResponsiveContainer } from 'recharts'
import type {
  ProcessFlowStep,
  ProcessFlowTransparency
} from '@shared/types/transparency'

const { Title, Text } = Typography

export interface TokenUsageAnalyzerProps {
  processFlowSteps: ProcessFlowStep[]
  transparency: ProcessFlowTransparency
  showCostAnalysis?: boolean
  showOptimizationSuggestions?: boolean
  tokenCostPerK?: number // Cost per 1000 tokens
  className?: string
  testId?: string
}

interface TokenAnalysis {
  totalTokens: number
  averagePerStep: number
  maxStep: ProcessFlowStep | null
  minStep: ProcessFlowStep | null
  efficiency: number
  costEstimate: number
  optimizationPotential: number
  suggestions: string[]
  promptTokens: number
  completionTokens: number
  apiCalls: number
  averageConfidence: number
}

/**
 * TokenUsageAnalyzer - Analyzes token usage patterns and costs
 * 
 * Features:
 * - Token usage breakdown by step
 * - Cost analysis and estimation
 * - Budget utilization tracking
 * - Optimization suggestions
 * - Usage pattern visualization
 * - Efficiency metrics
 */
export const TokenUsageAnalyzer: React.FC<TokenUsageAnalyzerProps> = ({
  processFlowSteps,
  transparency,
  showCostAnalysis = true,
  showOptimizationSuggestions = true,
  tokenCostPerK = 0.002, // Default cost per 1000 tokens
  className,
  testId = 'token-usage-analyzer'
}) => {
  // Analyze token usage
  const analysis = useMemo((): TokenAnalysis => {
    if (processFlowSteps.length === 0 || !transparency) {
      return {
        totalTokens: 0,
        averagePerStep: 0,
        maxStep: null,
        minStep: null,
        efficiency: 0,
        costEstimate: 0,
        optimizationPotential: 0,
        suggestions: [],
        promptTokens: 0,
        completionTokens: 0,
        apiCalls: 0,
        averageConfidence: 0
      }
    }

    // Use ProcessFlow transparency data
    const totalTokens = transparency.totalTokens || 0
    const promptTokens = transparency.promptTokens || 0
    const completionTokens = transparency.completionTokens || 0
    const apiCalls = transparency.apiCallCount || 0
    const averageConfidence = transparency.confidence || 0

    const averagePerStep = processFlowSteps.length > 0 ? totalTokens / processFlowSteps.length : 0

    // Find max and min steps by duration (proxy for complexity)
    const maxStep = processFlowSteps.reduce((max, step) =>
      (step.durationMs || 0) > (max?.durationMs || 0) ? step : max, processFlowSteps[0])
    const minStep = processFlowSteps.reduce((min, step) =>
      (step.durationMs || 0) < (min?.durationMs || Infinity) ? step : min, processFlowSteps[0])

    // Calculate efficiency (tokens per millisecond)
    const efficiency = transparency.aiProcessingTimeMs ? totalTokens / transparency.aiProcessingTimeMs : 0

    // Calculate cost
    const costEstimate = transparency.estimatedCost || ((totalTokens / 1000) * tokenCostPerK)

    // Calculate optimization potential based on confidence
    const optimizationPotential = averageConfidence < 0.8 ? (1 - averageConfidence) * 100 : 0

    // Generate ProcessFlow-specific suggestions
    const suggestions: string[] = []

    if (averageConfidence < 0.7) {
      suggestions.push('Low average confidence detected - consider improving query processing')
    }
    if (transparency.estimatedCost && transparency.estimatedCost > 0.01) {
      suggestions.push('High cost per query - consider optimizing model usage or prompt efficiency')
    }
    if (apiCalls > 5) {
      suggestions.push('Multiple API calls detected - consider consolidating requests for better efficiency')
    }
    if (transparency.aiProcessingTimeMs && transparency.aiProcessingTimeMs > 5000) {
      suggestions.push('Long processing time - consider optimizing AI processing steps')
    }
    if (efficiency < 1) {
      suggestions.push('Low token generation efficiency - review processing logic')
    }

    return {
      totalTokens,
      averagePerStep,
      maxStep,
      minStep,
      efficiency,
      costEstimate,
      optimizationPotential,
      suggestions,
      promptTokens,
      completionTokens,
      apiCalls,
      averageConfidence
    }
  }, [processFlowSteps, transparency, tokenCostPerK])

  // Prepare chart data
  const stepData = useMemo(() => {
    return processFlowSteps.map((step, index) => ({
      step: index + 1,
      stepName: step.name.substring(0, 15) + (step.name.length > 15 ? '...' : ''),
      duration: step.durationMs || 0,
      confidence: (step.confidence || 0) * 100,
      stepType: step.stepType
    }))
  }, [processFlowSteps])

  const distributionData = useMemo(() => {
    // Show confidence distribution for ProcessFlow steps
    const ranges = [
      { name: 'Low Confidence (0-50%)', min: 0, max: 0.5, color: '#ff4d4f' },
      { name: 'Medium Confidence (51-75%)', min: 0.51, max: 0.75, color: '#faad14' },
      { name: 'High Confidence (76-90%)', min: 0.76, max: 0.9, color: '#1890ff' },
      { name: 'Very High Confidence (90%+)', min: 0.91, max: 1, color: '#52c41a' }
    ]

    return ranges.map(range => ({
      name: range.name,
      value: processFlowSteps.filter(step =>
        (step.confidence || 0) >= range.min && (step.confidence || 0) <= range.max
      ).length,
      color: range.color
    })).filter(item => item.value > 0)
  }, [processFlowSteps])

  const renderStepBreakdown = () => (
    <Timeline>
      {processFlowSteps.map((step, index) => {
        const isLongDuration = (step.durationMs || 0) > analysis.averagePerStep * 1.5
        const isLowConfidence = (step.confidence || 0) < 0.7

        return (
          <Timeline.Item
            key={step.stepId}
            color={step.status === 'Completed' ? 'green' : step.status === 'Failed' ? 'red' : 'blue'}
            dot={
              <Tooltip title={`${step.durationMs || 0}ms duration`}>
                <BarChartOutlined />
              </Tooltip>
            }
          >
            <Space direction="vertical" size="small">
              <Space>
                <Text strong>{step.name}</Text>
                <Tag color={step.status === 'Completed' ? 'green' : step.status === 'Failed' ? 'red' : 'blue'}>
                  {step.status}
                </Tag>
                <Tag color="blue">
                  {step.durationMs || 0}ms
                </Tag>
                {step.confidence && (
                  <Tag color={isLowConfidence ? 'orange' : 'purple'}>
                    {(step.confidence * 100).toFixed(1)}%
                  </Tag>
                )}
                {isLongDuration && (
                  <Tag color="orange" icon={<ExclamationCircleOutlined />}>
                    Long Duration
                  </Tag>
                )}
              </Space>
              <Text type="secondary" style={{ fontSize: '12px' }}>
                {step.stepType} • {((step.durationMs || 0) / (analysis.totalTokens || 1) * 100).toFixed(1)}% of processing time
              </Text>
            </Space>
          </Timeline.Item>
        )
      })}
    </Timeline>
  )

  const renderDistributionChart = () => (
    <ResponsiveContainer width="100%" height={200}>
      <PieChart>
        <Pie
          data={distributionData}
          cx="50%"
          cy="50%"
          outerRadius={80}
          dataKey="value"
          label={({ name, value }) => `${name}: ${value}`}
        >
          {distributionData.map((entry, index) => (
            <Cell key={`cell-${index}`} fill={entry.color} />
          ))}
        </Pie>
        <RechartsTooltip />
      </PieChart>
    </ResponsiveContainer>
  )

  const renderDurationChart = () => (
    <ResponsiveContainer width="100%" height={200}>
      <BarChart data={stepData}>
        <CartesianGrid strokeDasharray="3 3" />
        <XAxis dataKey="step" />
        <YAxis />
        <RechartsTooltip
          formatter={(value: any, name: string) => [
            name === 'duration' ? `${value}ms` : `${value}%`,
            name === 'duration' ? 'Duration' : 'Confidence'
          ]}
        />
        <Bar dataKey="duration" fill="#1890ff" />
      </BarChart>
    </ResponsiveContainer>
  )

  if (!processFlowSteps || processFlowSteps.length === 0 || !transparency) {
    return (
      <Card className={className} data-testid={testId}>
        <div style={{ textAlign: 'center', padding: '40px 0' }}>
          <Space direction="vertical">
            <BarChartOutlined style={{ fontSize: '48px', color: '#d9d9d9' }} />
            <Text type="secondary">No ProcessFlow data available</Text>
          </Space>
        </div>
      </Card>
    )
  }

  return (
    <Card 
      title={
        <Space>
          <BarChartOutlined />
          <span>Token Usage Analysis</span>
          <Tag color="blue">{analysis.totalTokens} total tokens</Tag>
        </Space>
      }
      className={className}
      data-testid={testId}
    >
      <Space direction="vertical" style={{ width: '100%' }} size="large">
        {/* Key Metrics */}
        <Row gutter={[16, 16]}>
          <Col xs={24} sm={12} lg={6}>
            <Statistic
              title="Total Tokens"
              value={analysis.totalTokens}
              prefix={<BarChartOutlined />}
            />
          </Col>
          <Col xs={24} sm={12} lg={6}>
            <Statistic
              title="Prompt Tokens"
              value={analysis.promptTokens}
              prefix={<ThunderboltOutlined />}
            />
          </Col>
          <Col xs={24} sm={12} lg={6}>
            <Statistic
              title="Completion Tokens"
              value={analysis.completionTokens}
              prefix={<CheckCircleOutlined />}
            />
          </Col>
          <Col xs={24} sm={12} lg={6}>
            <Statistic
              title="API Calls"
              value={analysis.apiCalls}
              prefix={<ApiOutlined />}
            />
          </Col>
        </Row>

        {/* Additional Metrics */}
        <Row gutter={[16, 16]}>
          <Col xs={24} sm={12} lg={6}>
            <Statistic
              title="Avg Confidence"
              value={(analysis.averageConfidence * 100).toFixed(1)}
              suffix="%"
              prefix={<ThunderboltOutlined />}
            />
          </Col>
          <Col xs={24} sm={12} lg={6}>
            <Statistic
              title="Efficiency"
              value={analysis.efficiency.toFixed(4)}
              suffix="tokens/ms"
              prefix={<CheckCircleOutlined />}
            />
          </Col>
          {showCostAnalysis && (
            <Col xs={24} sm={12} lg={6}>
              <Statistic
                title="Est. Cost"
                value={analysis.costEstimate.toFixed(4)}
                prefix={<DollarOutlined />}
                suffix="USD"
              />
            </Col>
          )}
          <Col xs={24} sm={12} lg={6}>
            <Statistic
              title="Processing Steps"
              value={processFlowSteps.length}
              prefix={<BarChartOutlined />}
            />
          </Col>
        </Row>

        {/* ProcessFlow Analysis */}
        <Row gutter={[16, 16]}>
          <Col xs={24} lg={12}>
            <Card title="Confidence Distribution" size="small">
              {renderDistributionChart()}
            </Card>
          </Col>
          <Col xs={24} lg={12}>
            <Card title="Step Duration Analysis" size="small">
              {renderDurationChart()}
            </Card>
          </Col>
        </Row>

        {/* Detailed Step Breakdown */}
        <Card title="ProcessFlow Step Breakdown" size="small">
          {renderStepBreakdown()}
        </Card>

        {/* Optimization Suggestions */}
        {showOptimizationSuggestions && analysis.suggestions.length > 0 && (
          <Card title="Optimization Suggestions" size="small">
            <Space direction="vertical" style={{ width: '100%' }}>
              {analysis.suggestions.map((suggestion, index) => (
                <Alert
                  key={index}
                  message={suggestion}
                  type="info"
                  showIcon
                  icon={<InfoCircleOutlined />}
                />
              ))}
            </Space>
          </Card>
        )}

        {/* Low Confidence Alert */}
        {analysis.optimizationPotential > 30 && (
          <Alert
            message="Low Confidence Detected"
            description={`${analysis.optimizationPotential.toFixed(0)}% optimization potential detected. Consider reviewing ProcessFlow steps for confidence improvements.`}
            type="warning"
            showIcon
          />
        )}
      </Space>
    </Card>
  )
}

export default TokenUsageAnalyzer
