import React, { useMemo } from 'react'
import { 
  Card, 
  Space, 
  Typography, 
  Row, 
  Col, 
  Statistic,
  Progress,
  Alert,
  Timeline,
  Tag,
  Tooltip
} from 'antd'
import {
  ClockCircleOutlined,
  ThunderboltOutlined,
  CheckCircleOutlined,
  ExclamationCircleOutlined,
  BarChartOutlined,
  TrophyOutlined,
  WarningOutlined
} from '@ant-design/icons'
import { LineChart, Line, AreaChart, Area, XAxis, YAxis, CartesianGrid, Tooltip as RechartsTooltip, ResponsiveContainer } from 'recharts'
import type { PromptConstructionStep } from '@shared/types/transparency'

const { Title, Text } = Typography

export interface PerformanceMetricsViewerProps {
  steps: PromptConstructionStep[]
  showTrendAnalysis?: boolean
  showBottleneckDetection?: boolean
  showOptimizationSuggestions?: boolean
  performanceThresholds?: {
    fast: number // ms
    acceptable: number // ms
    slow: number // ms
  }
  className?: string
  testId?: string
}

interface PerformanceAnalysis {
  totalTime: number
  averageTime: number
  fastestStep: PromptConstructionStep | null
  slowestStep: PromptConstructionStep | null
  bottlenecks: PromptConstructionStep[]
  efficiency: number
  throughput: number
  performanceGrade: 'excellent' | 'good' | 'fair' | 'poor'
  suggestions: string[]
}

/**
 * PerformanceMetricsViewer - Analyzes and visualizes performance metrics
 * 
 * Features:
 * - Processing time analysis
 * - Bottleneck detection
 * - Performance trend visualization
 * - Efficiency calculations
 * - Optimization recommendations
 * - Performance grading
 */
export const PerformanceMetricsViewer: React.FC<PerformanceMetricsViewerProps> = ({
  steps,
  showTrendAnalysis = true,
  showBottleneckDetection = true,
  showOptimizationSuggestions = true,
  performanceThresholds = { fast: 500, acceptable: 2000, slow: 5000 },
  className,
  testId = 'performance-metrics-viewer'
}) => {
  // Analyze performance metrics
  const analysis = useMemo((): PerformanceAnalysis => {
    if (!steps || steps.length === 0) {
      return {
        totalTime: 0,
        averageTime: 0,
        fastestStep: null,
        slowestStep: null,
        bottlenecks: [],
        efficiency: 0,
        throughput: 0,
        performanceGrade: 'poor',
        suggestions: []
      }
    }

    const totalTime = steps.reduce((sum, step) => sum + step.processingTimeMs, 0)
    const averageTime = totalTime / steps.length
    
    const fastestStep = steps.reduce((fastest, step) => 
      step.processingTimeMs < (fastest?.processingTimeMs || Infinity) ? step : fastest, steps[0])
    const slowestStep = steps.reduce((slowest, step) => 
      step.processingTimeMs > (slowest?.processingTimeMs || 0) ? step : slowest, steps[0])

    // Detect bottlenecks (steps taking significantly longer than average)
    const bottlenecks = steps.filter(step => 
      step.processingTimeMs > averageTime * 2 || 
      step.processingTimeMs > performanceThresholds.slow
    )

    // Calculate efficiency (confidence per ms)
    const totalConfidence = steps.reduce((sum, step) => sum + step.confidence, 0)
    const efficiency = totalTime > 0 ? totalConfidence / totalTime : 0

    // Calculate throughput (steps per second)
    const throughput = totalTime > 0 ? (steps.length / totalTime) * 1000 : 0

    // Determine performance grade
    let performanceGrade: PerformanceAnalysis['performanceGrade'] = 'poor'
    if (averageTime <= performanceThresholds.fast) performanceGrade = 'excellent'
    else if (averageTime <= performanceThresholds.acceptable) performanceGrade = 'good'
    else if (averageTime <= performanceThresholds.slow) performanceGrade = 'fair'

    // Generate suggestions
    const suggestions: string[] = []
    if (bottlenecks.length > 0) {
      suggestions.push(`${bottlenecks.length} bottleneck(s) detected - optimize slow steps`)
    }
    if (averageTime > performanceThresholds.acceptable) {
      suggestions.push('Average processing time exceeds acceptable threshold')
    }
    if (efficiency < 0.001) {
      suggestions.push('Low efficiency detected - review confidence vs processing time ratio')
    }
    if (slowestStep && slowestStep.processingTimeMs > performanceThresholds.slow) {
      suggestions.push(`Step "${slowestStep.stepName}" is significantly slow - requires optimization`)
    }

    return {
      totalTime,
      averageTime,
      fastestStep,
      slowestStep,
      bottlenecks,
      efficiency,
      throughput,
      performanceGrade,
      suggestions
    }
  }, [steps, performanceThresholds])

  // Prepare chart data
  const trendData = useMemo(() => {
    let cumulativeTime = 0
    return steps.map((step, index) => {
      cumulativeTime += step.processingTimeMs
      return {
        step: index + 1,
        stepName: step.stepName.substring(0, 15) + (step.stepName.length > 15 ? '...' : ''),
        processingTime: step.processingTimeMs,
        cumulativeTime,
        confidence: step.confidence * 100,
        efficiency: step.processingTimeMs > 0 ? (step.confidence / step.processingTimeMs) * 1000 : 0
      }
    })
  }, [steps])

  const getPerformanceColor = (time: number) => {
    if (time <= performanceThresholds.fast) return '#52c41a'
    if (time <= performanceThresholds.acceptable) return '#1890ff'
    if (time <= performanceThresholds.slow) return '#faad14'
    return '#ff4d4f'
  }

  const getGradeColor = (grade: string) => {
    switch (grade) {
      case 'excellent': return '#52c41a'
      case 'good': return '#1890ff'
      case 'fair': return '#faad14'
      case 'poor': return '#ff4d4f'
      default: return '#d9d9d9'
    }
  }

  const renderPerformanceTimeline = () => (
    <Timeline>
      {steps.map((step, index) => {
        const isBottleneck = analysis.bottlenecks.includes(step)
        const isFast = step.processingTimeMs <= performanceThresholds.fast
        
        return (
          <Timeline.Item
            key={step.id}
            color={getPerformanceColor(step.processingTimeMs)}
            dot={
              <Tooltip title={`${step.processingTimeMs}ms`}>
                {isBottleneck ? <WarningOutlined /> : 
                 isFast ? <ThunderboltOutlined /> : <ClockCircleOutlined />}
              </Tooltip>
            }
          >
            <Space direction="vertical" size="small">
              <Space>
                <Text strong>{step.stepName}</Text>
                <Tag color={getPerformanceColor(step.processingTimeMs)}>
                  {step.processingTimeMs}ms
                </Tag>
                {isBottleneck && (
                  <Tag color="red" icon={<WarningOutlined />}>
                    Bottleneck
                  </Tag>
                )}
                {isFast && (
                  <Tag color="green" icon={<ThunderboltOutlined />}>
                    Fast
                  </Tag>
                )}
              </Space>
              <Text type="secondary" style={{ fontSize: '12px' }}>
                {((step.processingTimeMs / analysis.totalTime) * 100).toFixed(1)}% of total time
              </Text>
            </Space>
          </Timeline.Item>
        )
      })}
    </Timeline>
  )

  const renderTrendChart = () => (
    <ResponsiveContainer width="100%" height={200}>
      <AreaChart data={trendData}>
        <CartesianGrid strokeDasharray="3 3" />
        <XAxis dataKey="step" />
        <YAxis />
        <RechartsTooltip 
          formatter={(value: any, name: string) => [
            name === 'processingTime' ? `${value}ms` : 
            name === 'cumulativeTime' ? `${value}ms` : value,
            name === 'processingTime' ? 'Processing Time' :
            name === 'cumulativeTime' ? 'Cumulative Time' : name
          ]}
        />
        <Area 
          type="monotone" 
          dataKey="processingTime" 
          stroke="#1890ff" 
          fill="#1890ff" 
          fillOpacity={0.3}
        />
        <Area 
          type="monotone" 
          dataKey="cumulativeTime" 
          stroke="#52c41a" 
          fill="#52c41a" 
          fillOpacity={0.1}
        />
      </AreaChart>
    </ResponsiveContainer>
  )

  if (!steps || steps.length === 0) {
    return (
      <Card className={className} data-testid={testId}>
        <div style={{ textAlign: 'center', padding: '40px 0' }}>
          <Space direction="vertical">
            <ClockCircleOutlined style={{ fontSize: '48px', color: '#d9d9d9' }} />
            <Text type="secondary">No performance data available</Text>
          </Space>
        </div>
      </Card>
    )
  }

  return (
    <Card 
      title={
        <Space>
          <ClockCircleOutlined />
          <span>Performance Metrics</span>
          <Tag color={getGradeColor(analysis.performanceGrade)}>
            {analysis.performanceGrade.toUpperCase()}
          </Tag>
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
              title="Total Time"
              value={analysis.totalTime}
              suffix="ms"
              prefix={<ClockCircleOutlined />}
            />
          </Col>
          <Col xs={24} sm={12} lg={6}>
            <Statistic
              title="Average Time"
              value={analysis.averageTime.toFixed(0)}
              suffix="ms"
              prefix={<BarChartOutlined />}
              valueStyle={{ color: getPerformanceColor(analysis.averageTime) }}
            />
          </Col>
          <Col xs={24} sm={12} lg={6}>
            <Statistic
              title="Efficiency"
              value={analysis.efficiency.toFixed(4)}
              suffix="conf/ms"
              prefix={<TrophyOutlined />}
            />
          </Col>
          <Col xs={24} sm={12} lg={6}>
            <Statistic
              title="Throughput"
              value={analysis.throughput.toFixed(2)}
              suffix="steps/sec"
              prefix={<ThunderboltOutlined />}
            />
          </Col>
        </Row>

        {/* Performance Distribution */}
        <Row gutter={[16, 16]}>
          <Col span={8}>
            <Card size="small">
              <Space direction="vertical" style={{ width: '100%' }}>
                <Text strong>Fast Steps</Text>
                <Text type="secondary">≤ {performanceThresholds.fast}ms</Text>
                <Progress 
                  percent={(steps.filter(s => s.processingTimeMs <= performanceThresholds.fast).length / steps.length) * 100}
                  strokeColor="#52c41a"
                  format={(percent) => `${steps.filter(s => s.processingTimeMs <= performanceThresholds.fast).length}/${steps.length}`}
                />
              </Space>
            </Card>
          </Col>
          <Col span={8}>
            <Card size="small">
              <Space direction="vertical" style={{ width: '100%' }}>
                <Text strong>Acceptable Steps</Text>
                <Text type="secondary">{performanceThresholds.fast + 1}-{performanceThresholds.acceptable}ms</Text>
                <Progress 
                  percent={(steps.filter(s => s.processingTimeMs > performanceThresholds.fast && s.processingTimeMs <= performanceThresholds.acceptable).length / steps.length) * 100}
                  strokeColor="#1890ff"
                  format={(percent) => `${steps.filter(s => s.processingTimeMs > performanceThresholds.fast && s.processingTimeMs <= performanceThresholds.acceptable).length}/${steps.length}`}
                />
              </Space>
            </Card>
          </Col>
          <Col span={8}>
            <Card size="small">
              <Space direction="vertical" style={{ width: '100%' }}>
                <Text strong>Slow Steps</Text>
                <Text type="secondary">&gt; {performanceThresholds.acceptable}ms</Text>
                <Progress 
                  percent={(steps.filter(s => s.processingTimeMs > performanceThresholds.acceptable).length / steps.length) * 100}
                  strokeColor="#ff4d4f"
                  format={(percent) => `${steps.filter(s => s.processingTimeMs > performanceThresholds.acceptable).length}/${steps.length}`}
                />
              </Space>
            </Card>
          </Col>
        </Row>

        {/* Performance Trend */}
        {showTrendAnalysis && (
          <Card title="Performance Trend" size="small">
            {renderTrendChart()}
          </Card>
        )}

        {/* Bottleneck Detection */}
        {showBottleneckDetection && analysis.bottlenecks.length > 0 && (
          <Card title="Detected Bottlenecks" size="small">
            <Alert
              message={`${analysis.bottlenecks.length} bottleneck(s) detected`}
              description="These steps are taking significantly longer than average and may need optimization."
              type="warning"
              showIcon
              style={{ marginBottom: 16 }}
            />
            <Space direction="vertical" style={{ width: '100%' }}>
              {analysis.bottlenecks.map((step, index) => (
                <Card key={step.id} size="small" style={{ background: '#fff2e8' }}>
                  <Space>
                    <WarningOutlined style={{ color: '#fa8c16' }} />
                    <Text strong>{step.stepName}</Text>
                    <Tag color="orange">{step.processingTimeMs}ms</Tag>
                    <Text type="secondary">
                      {((step.processingTimeMs / analysis.averageTime) * 100).toFixed(0)}% above average
                    </Text>
                  </Space>
                </Card>
              ))}
            </Space>
          </Card>
        )}

        {/* Performance Timeline */}
        <Card title="Step-by-Step Performance" size="small">
          {renderPerformanceTimeline()}
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
                />
              ))}
            </Space>
          </Card>
        )}

        {/* Performance Grade Alert */}
        {analysis.performanceGrade === 'poor' && (
          <Alert
            message="Poor Performance Detected"
            description="This trace has poor performance characteristics. Consider reviewing the processing logic and optimizing slow steps."
            type="error"
            showIcon
          />
        )}
      </Space>
    </Card>
  )
}

export default PerformanceMetricsViewer
