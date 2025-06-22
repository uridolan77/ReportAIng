import React, { useState, useMemo } from 'react'
import { 
  Card, 
  Space, 
  Typography, 
  Progress, 
  Alert, 
  Row, 
  Col, 
  Statistic,
  Timeline,
  Tag,
  Button,
  Tooltip,
  Divider
} from 'antd'
import {
  TrophyOutlined,
  WarningOutlined,
  CheckCircleOutlined,
  ExclamationCircleOutlined,
  LineChartOutlined,
  BarChartOutlined,
  EyeOutlined,
  BulbOutlined
} from '@ant-design/icons'
import { LineChart, Line, AreaChart, Area, XAxis, YAxis, CartesianGrid, Tooltip as RechartsTooltip, ResponsiveContainer } from 'recharts'
import { ConfidenceIndicator } from '../common/ConfidenceIndicator'
import type { PromptConstructionStep } from '@shared/types/transparency'

const { Title, Text } = Typography

export interface ConfidenceTrackerProps {
  steps: PromptConstructionStep[]
  showTrend?: boolean
  showBreakdown?: boolean
  showRecommendations?: boolean
  thresholds?: {
    high: number
    medium: number
    low: number
  }
  onConfidenceAlert?: (step: PromptConstructionStep, level: 'high' | 'medium' | 'low') => void
  className?: string
  testId?: string
}

interface ConfidenceMetrics {
  overall: number
  trend: 'improving' | 'declining' | 'stable'
  volatility: number
  lowConfidenceSteps: PromptConstructionStep[]
  highConfidenceSteps: PromptConstructionStep[]
  recommendations: string[]
}

/**
 * ConfidenceTracker - Tracks and analyzes confidence levels throughout query processing
 * 
 * Features:
 * - Real-time confidence monitoring
 * - Confidence trend analysis
 * - Step-by-step confidence breakdown
 * - Confidence threshold alerts
 * - Improvement recommendations
 * - Visual confidence progression
 */
export const ConfidenceTracker: React.FC<ConfidenceTrackerProps> = ({
  steps,
  showTrend = true,
  showBreakdown = true,
  showRecommendations = true,
  thresholds = { high: 0.8, medium: 0.6, low: 0.4 },
  onConfidenceAlert,
  className,
  testId = 'confidence-tracker'
}) => {
  const [selectedStep, setSelectedStep] = useState<number | null>(null)

  // Calculate confidence metrics
  const metrics = useMemo((): ConfidenceMetrics => {
    if (!steps || steps.length === 0) {
      return {
        overall: 0,
        trend: 'stable',
        volatility: 0,
        lowConfidenceSteps: [],
        highConfidenceSteps: [],
        recommendations: []
      }
    }

    const confidenceValues = steps.map(step => step.confidence)
    const overall = confidenceValues.reduce((sum, conf) => sum + conf, 0) / confidenceValues.length

    // Calculate trend
    const firstHalf = confidenceValues.slice(0, Math.floor(confidenceValues.length / 2))
    const secondHalf = confidenceValues.slice(Math.floor(confidenceValues.length / 2))
    const firstAvg = firstHalf.reduce((sum, conf) => sum + conf, 0) / firstHalf.length
    const secondAvg = secondHalf.reduce((sum, conf) => sum + conf, 0) / secondHalf.length
    
    let trend: 'improving' | 'declining' | 'stable' = 'stable'
    if (secondAvg > firstAvg + 0.05) trend = 'improving'
    else if (secondAvg < firstAvg - 0.05) trend = 'declining'

    // Calculate volatility (standard deviation)
    const mean = overall
    const variance = confidenceValues.reduce((sum, conf) => sum + Math.pow(conf - mean, 2), 0) / confidenceValues.length
    const volatility = Math.sqrt(variance)

    // Categorize steps
    const lowConfidenceSteps = steps.filter(step => step.confidence < thresholds.medium)
    const highConfidenceSteps = steps.filter(step => step.confidence >= thresholds.high)

    // Generate recommendations
    const recommendations: string[] = []
    if (overall < thresholds.medium) {
      recommendations.push('Overall confidence is low - consider reviewing prompt construction logic')
    }
    if (volatility > 0.2) {
      recommendations.push('High confidence volatility detected - stabilize decision-making process')
    }
    if (trend === 'declining') {
      recommendations.push('Confidence is declining throughout the process - investigate later steps')
    }
    if (lowConfidenceSteps.length > steps.length * 0.3) {
      recommendations.push('Many steps have low confidence - review step dependencies and logic')
    }

    return {
      overall,
      trend,
      volatility,
      lowConfidenceSteps,
      highConfidenceSteps,
      recommendations
    }
  }, [steps, thresholds])

  // Prepare chart data
  const chartData = useMemo(() => {
    return steps.map((step, index) => ({
      step: index + 1,
      stepName: step.stepName.substring(0, 20) + (step.stepName.length > 20 ? '...' : ''),
      confidence: step.confidence * 100,
      processingTime: step.processingTimeMs,
      tokens: step.tokensAdded
    }))
  }, [steps])

  const getConfidenceLevel = (confidence: number): 'high' | 'medium' | 'low' => {
    if (confidence >= thresholds.high) return 'high'
    if (confidence >= thresholds.medium) return 'medium'
    return 'low'
  }

  const getConfidenceLevelColor = (level: 'high' | 'medium' | 'low') => {
    switch (level) {
      case 'high': return '#52c41a'
      case 'medium': return '#faad14'
      case 'low': return '#ff4d4f'
    }
  }

  const getTrendIcon = (trend: string) => {
    switch (trend) {
      case 'improving': return <CheckCircleOutlined style={{ color: '#52c41a' }} />
      case 'declining': return <ExclamationCircleOutlined style={{ color: '#ff4d4f' }} />
      default: return <BarChartOutlined style={{ color: '#1890ff' }} />
    }
  }

  const renderConfidenceBreakdown = () => (
    <Timeline>
      {steps.map((step, index) => {
        const level = getConfidenceLevel(step.confidence)
        return (
          <Timeline.Item
            key={step.id}
            color={getConfidenceLevelColor(level)}
            dot={
              <Tooltip title={`Confidence: ${(step.confidence * 100).toFixed(1)}%`}>
                {level === 'high' ? <CheckCircleOutlined /> : 
                 level === 'medium' ? <WarningOutlined /> : 
                 <ExclamationCircleOutlined />}
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
                <Tag size="small" color={getConfidenceLevelColor(level)}>
                  {level.toUpperCase()}
                </Tag>
              </Space>
              <Text type="secondary" style={{ fontSize: '12px' }}>
                Processing: {step.processingTimeMs}ms | Tokens: {step.tokensAdded}
              </Text>
            </Space>
          </Timeline.Item>
        )
      })}
    </Timeline>
  )

  const renderTrendChart = () => (
    <ResponsiveContainer width="100%" height={200}>
      <AreaChart data={chartData}>
        <CartesianGrid strokeDasharray="3 3" />
        <XAxis dataKey="step" />
        <YAxis domain={[0, 100]} />
        <RechartsTooltip 
          formatter={(value: any, name: string) => [
            name === 'confidence' ? `${value.toFixed(1)}%` : value,
            name === 'confidence' ? 'Confidence' : name
          ]}
          labelFormatter={(label) => `Step ${label}`}
        />
        <Area 
          type="monotone" 
          dataKey="confidence" 
          stroke="#1890ff" 
          fill="#1890ff" 
          fillOpacity={0.3}
        />
      </AreaChart>
    </ResponsiveContainer>
  )

  const renderRecommendations = () => (
    <Space direction="vertical" style={{ width: '100%' }}>
      {metrics.recommendations.map((recommendation, index) => (
        <Alert
          key={index}
          message={recommendation}
          type="info"
          showIcon
          icon={<BulbOutlined />}
        />
      ))}
      {metrics.recommendations.length === 0 && (
        <Alert
          message="No specific recommendations at this time"
          description="Confidence levels are within acceptable ranges"
          type="success"
          showIcon
        />
      )}
    </Space>
  )

  if (!steps || steps.length === 0) {
    return (
      <Card className={className} data-testid={testId}>
        <div style={{ textAlign: 'center', padding: '40px 0' }}>
          <Space direction="vertical">
            <TrophyOutlined style={{ fontSize: '48px', color: '#d9d9d9' }} />
            <Text type="secondary">No confidence data available</Text>
          </Space>
        </div>
      </Card>
    )
  }

  return (
    <Card 
      title={
        <Space>
          <TrophyOutlined />
          <span>Confidence Tracker</span>
          <Tag color="blue">{steps.length} steps</Tag>
        </Space>
      }
      className={className}
      data-testid={testId}
    >
      {/* Overall Metrics */}
      <Row gutter={[16, 16]} style={{ marginBottom: 24 }}>
        <Col xs={24} sm={8}>
          <Statistic
            title="Overall Confidence"
            value={(metrics.overall * 100).toFixed(1)}
            suffix="%"
            prefix={<TrophyOutlined />}
            valueStyle={{ 
              color: metrics.overall >= thresholds.high ? '#3f8600' : 
                     metrics.overall >= thresholds.medium ? '#faad14' : '#cf1322' 
            }}
          />
        </Col>
        <Col xs={24} sm={8}>
          <Statistic
            title="Trend"
            value={metrics.trend.charAt(0).toUpperCase() + metrics.trend.slice(1)}
            prefix={getTrendIcon(metrics.trend)}
            valueStyle={{ 
              color: metrics.trend === 'improving' ? '#3f8600' : 
                     metrics.trend === 'declining' ? '#cf1322' : '#1890ff'
            }}
          />
        </Col>
        <Col xs={24} sm={8}>
          <Statistic
            title="Volatility"
            value={(metrics.volatility * 100).toFixed(1)}
            suffix="%"
            prefix={<LineChartOutlined />}
            valueStyle={{ 
              color: metrics.volatility < 0.1 ? '#3f8600' : 
                     metrics.volatility < 0.2 ? '#faad14' : '#cf1322'
            }}
          />
        </Col>
      </Row>

      {/* Confidence Distribution */}
      <Row gutter={[16, 16]} style={{ marginBottom: 24 }}>
        <Col span={8}>
          <Card size="small">
            <Statistic
              title="High Confidence Steps"
              value={metrics.highConfidenceSteps.length}
              suffix={`/ ${steps.length}`}
              valueStyle={{ color: '#3f8600' }}
            />
            <Progress 
              percent={(metrics.highConfidenceSteps.length / steps.length) * 100}
              strokeColor="#52c41a"
              showInfo={false}
              size="small"
            />
          </Card>
        </Col>
        <Col span={8}>
          <Card size="small">
            <Statistic
              title="Medium Confidence Steps"
              value={steps.filter(s => s.confidence >= thresholds.medium && s.confidence < thresholds.high).length}
              suffix={`/ ${steps.length}`}
              valueStyle={{ color: '#faad14' }}
            />
            <Progress 
              percent={(steps.filter(s => s.confidence >= thresholds.medium && s.confidence < thresholds.high).length / steps.length) * 100}
              strokeColor="#faad14"
              showInfo={false}
              size="small"
            />
          </Card>
        </Col>
        <Col span={8}>
          <Card size="small">
            <Statistic
              title="Low Confidence Steps"
              value={metrics.lowConfidenceSteps.length}
              suffix={`/ ${steps.length}`}
              valueStyle={{ color: '#cf1322' }}
            />
            <Progress 
              percent={(metrics.lowConfidenceSteps.length / steps.length) * 100}
              strokeColor="#ff4d4f"
              showInfo={false}
              size="small"
            />
          </Card>
        </Col>
      </Row>

      {/* Trend Chart */}
      {showTrend && (
        <>
          <Divider orientation="left">Confidence Trend</Divider>
          {renderTrendChart()}
        </>
      )}

      {/* Step Breakdown */}
      {showBreakdown && (
        <>
          <Divider orientation="left">Step-by-Step Breakdown</Divider>
          {renderConfidenceBreakdown()}
        </>
      )}

      {/* Recommendations */}
      {showRecommendations && (
        <>
          <Divider orientation="left">Recommendations</Divider>
          {renderRecommendations()}
        </>
      )}
    </Card>
  )
}

export default ConfidenceTracker
