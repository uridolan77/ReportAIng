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
import type { PromptConstructionStep, TokenBudget } from '@shared/types/transparency'

const { Title, Text } = Typography

export interface TokenUsageAnalyzerProps {
  steps: PromptConstructionStep[]
  budget?: TokenBudget
  showCostAnalysis?: boolean
  showOptimizationSuggestions?: boolean
  tokenCostPerK?: number // Cost per 1000 tokens
  className?: string
  testId?: string
}

interface TokenAnalysis {
  totalTokens: number
  averagePerStep: number
  maxStep: PromptConstructionStep | null
  minStep: PromptConstructionStep | null
  efficiency: number
  costEstimate: number
  budgetUtilization: number
  optimizationPotential: number
  suggestions: string[]
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
  steps,
  budget,
  showCostAnalysis = true,
  showOptimizationSuggestions = true,
  tokenCostPerK = 0.002, // Default cost per 1000 tokens
  className,
  testId = 'token-usage-analyzer'
}) => {
  // Analyze token usage
  const analysis = useMemo((): TokenAnalysis => {
    if (!steps || steps.length === 0) {
      return {
        totalTokens: 0,
        averagePerStep: 0,
        maxStep: null,
        minStep: null,
        efficiency: 0,
        costEstimate: 0,
        budgetUtilization: 0,
        optimizationPotential: 0,
        suggestions: []
      }
    }

    const totalTokens = steps.reduce((sum, step) => sum + step.tokensAdded, 0)
    const averagePerStep = totalTokens / steps.length
    
    const maxStep = steps.reduce((max, step) => 
      step.tokensAdded > (max?.tokensAdded || 0) ? step : max, steps[0])
    const minStep = steps.reduce((min, step) => 
      step.tokensAdded < (min?.tokensAdded || Infinity) ? step : min, steps[0])

    // Calculate efficiency (tokens per ms)
    const totalTime = steps.reduce((sum, step) => sum + step.processingTimeMs, 0)
    const efficiency = totalTime > 0 ? totalTokens / totalTime : 0

    // Calculate cost
    const costEstimate = (totalTokens / 1000) * tokenCostPerK

    // Calculate budget utilization
    const budgetUtilization = budget ? (totalTokens / budget.limit) * 100 : 0

    // Calculate optimization potential
    const highUsageSteps = steps.filter(step => step.tokensAdded > averagePerStep * 1.5)
    const optimizationPotential = (highUsageSteps.length / steps.length) * 100

    // Generate suggestions
    const suggestions: string[] = []
    if (optimizationPotential > 30) {
      suggestions.push('Consider optimizing high-token steps for better efficiency')
    }
    if (budgetUtilization > 90) {
      suggestions.push('Token budget nearly exhausted - consider increasing limit or optimizing usage')
    }
    if (efficiency < 1) {
      suggestions.push('Low token generation efficiency - review processing logic')
    }
    if (maxStep && maxStep.tokensAdded > averagePerStep * 3) {
      suggestions.push(`Step "${maxStep.stepName}" uses excessive tokens - consider optimization`)
    }

    return {
      totalTokens,
      averagePerStep,
      maxStep,
      minStep,
      efficiency,
      costEstimate,
      budgetUtilization,
      optimizationPotential,
      suggestions
    }
  }, [steps, budget, tokenCostPerK])

  // Prepare chart data
  const stepData = useMemo(() => {
    return steps.map((step, index) => ({
      step: index + 1,
      stepName: step.stepName.substring(0, 15) + (step.stepName.length > 15 ? '...' : ''),
      tokens: step.tokensAdded,
      efficiency: step.processingTimeMs > 0 ? step.tokensAdded / step.processingTimeMs : 0,
      confidence: step.confidence * 100
    }))
  }, [steps])

  const distributionData = useMemo(() => {
    const ranges = [
      { name: 'Low (0-100)', min: 0, max: 100, color: '#52c41a' },
      { name: 'Medium (101-500)', min: 101, max: 500, color: '#1890ff' },
      { name: 'High (501-1000)', min: 501, max: 1000, color: '#faad14' },
      { name: 'Very High (1000+)', min: 1001, max: Infinity, color: '#ff4d4f' }
    ]

    return ranges.map(range => ({
      name: range.name,
      value: steps.filter(step => 
        step.tokensAdded >= range.min && step.tokensAdded <= range.max
      ).length,
      color: range.color
    })).filter(item => item.value > 0)
  }, [steps])

  const renderUsageBreakdown = () => (
    <Timeline>
      {steps.map((step, index) => {
        const isHighUsage = step.tokensAdded > analysis.averagePerStep * 1.5
        const isLowUsage = step.tokensAdded < analysis.averagePerStep * 0.5
        
        return (
          <Timeline.Item
            key={step.id}
            color={isHighUsage ? 'red' : isLowUsage ? 'gray' : 'blue'}
            dot={
              <Tooltip title={`${step.tokensAdded} tokens`}>
                <BarChartOutlined />
              </Tooltip>
            }
          >
            <Space direction="vertical" size="small">
              <Space>
                <Text strong>{step.stepName}</Text>
                <Tag color={isHighUsage ? 'red' : isLowUsage ? 'gray' : 'blue'}>
                  {step.tokensAdded} tokens
                </Tag>
                {isHighUsage && (
                  <Tag color="orange" icon={<ExclamationCircleOutlined />}>
                    High Usage
                  </Tag>
                )}
              </Space>
              <Text type="secondary" style={{ fontSize: '12px' }}>
                {((step.tokensAdded / analysis.totalTokens) * 100).toFixed(1)}% of total usage
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

  const renderUsageChart = () => (
    <ResponsiveContainer width="100%" height={200}>
      <BarChart data={stepData}>
        <CartesianGrid strokeDasharray="3 3" />
        <XAxis dataKey="step" />
        <YAxis />
        <RechartsTooltip 
          formatter={(value: any, name: string) => [
            name === 'tokens' ? `${value} tokens` : value,
            name === 'tokens' ? 'Token Usage' : name
          ]}
        />
        <Bar dataKey="tokens" fill="#1890ff" />
      </BarChart>
    </ResponsiveContainer>
  )

  if (!steps || steps.length === 0) {
    return (
      <Card className={className} data-testid={testId}>
        <div style={{ textAlign: 'center', padding: '40px 0' }}>
          <Space direction="vertical">
            <BarChartOutlined style={{ fontSize: '48px', color: '#d9d9d9' }} />
            <Text type="secondary">No token usage data available</Text>
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
              title="Average per Step"
              value={analysis.averagePerStep.toFixed(0)}
              prefix={<ThunderboltOutlined />}
            />
          </Col>
          <Col xs={24} sm={12} lg={6}>
            <Statistic
              title="Efficiency"
              value={analysis.efficiency.toFixed(2)}
              suffix="tokens/ms"
              prefix={<CheckCircleOutlined />}
            />
          </Col>
          {showCostAnalysis && (
            <Col xs={24} sm={12} lg={6}>
              <Statistic
                title="Est. Cost"
                value={analysis.costEstimate.toFixed(4)}
                prefix="$"
                suffix="USD"
                prefix={<DollarOutlined />}
              />
            </Col>
          )}
        </Row>

        {/* Budget Information */}
        {budget && (
          <Card title="Budget Utilization" size="small">
            <Row gutter={[16, 16]}>
              <Col span={8}>
                <Statistic
                  title="Budget Limit"
                  value={budget.limit}
                  prefix={<BarChartOutlined />}
                />
              </Col>
              <Col span={8}>
                <Statistic
                  title="Used"
                  value={analysis.totalTokens}
                  prefix={<BarChartOutlined />}
                />
              </Col>
              <Col span={8}>
                <Statistic
                  title="Remaining"
                  value={budget.limit - analysis.totalTokens}
                  prefix={<BarChartOutlined />}
                  valueStyle={{ 
                    color: (budget.limit - analysis.totalTokens) > budget.limit * 0.2 ? 
                      '#3f8600' : '#cf1322' 
                  }}
                />
              </Col>
            </Row>
            <Progress
              percent={analysis.budgetUtilization}
              strokeColor={
                analysis.budgetUtilization > 90 ? '#ff4d4f' :
                analysis.budgetUtilization > 70 ? '#faad14' : '#52c41a'
              }
              format={(percent) => `${percent?.toFixed(1)}% used`}
              style={{ marginTop: 16 }}
            />
          </Card>
        )}

        {/* Usage Distribution */}
        <Row gutter={[16, 16]}>
          <Col xs={24} lg={12}>
            <Card title="Usage Distribution" size="small">
              {renderDistributionChart()}
            </Card>
          </Col>
          <Col xs={24} lg={12}>
            <Card title="Step-by-Step Usage" size="small">
              {renderUsageChart()}
            </Card>
          </Col>
        </Row>

        {/* Detailed Breakdown */}
        <Card title="Usage Breakdown" size="small">
          {renderUsageBreakdown()}
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

        {/* High Usage Alert */}
        {analysis.optimizationPotential > 50 && (
          <Alert
            message="High Optimization Potential"
            description={`${analysis.optimizationPotential.toFixed(0)}% of steps have above-average token usage. Consider reviewing these steps for optimization opportunities.`}
            type="warning"
            showIcon
          />
        )}
      </Space>
    </Card>
  )
}

export default TokenUsageAnalyzer
