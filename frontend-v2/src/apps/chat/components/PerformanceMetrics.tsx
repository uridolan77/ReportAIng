import React, { useState, useEffect } from 'react'
import { Card, Space, Typography, Progress, Statistic, Row, Col, Alert, Tag, Tooltip } from 'antd'
import {
  ClockCircleOutlined,
  BarChartOutlined,
  ThunderboltOutlined,
  DollarOutlined,
  TrophyOutlined,
  WarningOutlined,
  CheckCircleOutlined,
  InfoCircleOutlined
} from '@ant-design/icons'
import { Line } from '@ant-design/plots'

const { Text, Title } = Typography

export interface PerformanceMetricsProps {
  showRealTime?: boolean
  showHistorical?: boolean
  showOptimizations?: boolean
  className?: string
}

interface PerformanceData {
  responseTime: number
  tokenUsage: number
  confidence: number
  cost: number
  successRate: number
  throughput: number
}

interface OptimizationSuggestion {
  id: string
  title: string
  impact: 'high' | 'medium' | 'low'
  effort: 'easy' | 'medium' | 'hard'
  description: string
  estimatedImprovement: number
}

/**
 * PerformanceMetrics - Real-time and historical performance monitoring
 * 
 * Features:
 * - Real-time performance metrics
 * - Historical trend analysis
 * - Optimization suggestions
 * - Cost tracking
 * - Success rate monitoring
 */
export const PerformanceMetrics: React.FC<PerformanceMetricsProps> = ({
  showRealTime = true,
  showHistorical = true,
  showOptimizations = true,
  className = ''
}) => {
  const [currentMetrics, setCurrentMetrics] = useState<PerformanceData>({
    responseTime: 1250,
    tokenUsage: 850,
    confidence: 0.92,
    cost: 0.045,
    successRate: 0.96,
    throughput: 12.5
  })

  const [historicalData, setHistoricalData] = useState([
    { time: '10:00', responseTime: 1100, tokenUsage: 780, confidence: 0.89 },
    { time: '10:05', responseTime: 1200, tokenUsage: 820, confidence: 0.91 },
    { time: '10:10', responseTime: 1150, tokenUsage: 800, confidence: 0.93 },
    { time: '10:15', responseTime: 1300, tokenUsage: 900, confidence: 0.88 },
    { time: '10:20', responseTime: 1250, tokenUsage: 850, confidence: 0.92 }
  ])

  const optimizationSuggestions: OptimizationSuggestion[] = [
    {
      id: 'opt-1',
      title: 'Reduce Token Usage',
      impact: 'high',
      effort: 'medium',
      description: 'Optimize prompt construction to reduce average token consumption by 15%',
      estimatedImprovement: 15
    },
    {
      id: 'opt-2',
      title: 'Cache Common Queries',
      impact: 'medium',
      effort: 'easy',
      description: 'Implement caching for frequently asked questions',
      estimatedImprovement: 25
    },
    {
      id: 'opt-3',
      title: 'Model Selection',
      impact: 'high',
      effort: 'hard',
      description: 'Use different models based on query complexity',
      estimatedImprovement: 30
    }
  ]

  // Simulate real-time updates
  useEffect(() => {
    if (!showRealTime) return

    const interval = setInterval(() => {
      setCurrentMetrics(prev => ({
        responseTime: prev.responseTime + (Math.random() - 0.5) * 200,
        tokenUsage: prev.tokenUsage + (Math.random() - 0.5) * 100,
        confidence: Math.max(0.7, Math.min(1.0, prev.confidence + (Math.random() - 0.5) * 0.1)),
        cost: prev.cost + (Math.random() - 0.5) * 0.01,
        successRate: Math.max(0.8, Math.min(1.0, prev.successRate + (Math.random() - 0.5) * 0.05)),
        throughput: prev.throughput + (Math.random() - 0.5) * 2
      }))
    }, 5000)

    return () => clearInterval(interval)
  }, [showRealTime])

  const getMetricStatus = (value: number, thresholds: { good: number; warning: number }) => {
    if (value >= thresholds.good) return 'success'
    if (value >= thresholds.warning) return 'warning'
    return 'error'
  }

  const getImpactColor = (impact: string) => {
    switch (impact) {
      case 'high': return '#ff4d4f'
      case 'medium': return '#faad14'
      case 'low': return '#52c41a'
      default: return '#d9d9d9'
    }
  }

  const getEffortColor = (effort: string) => {
    switch (effort) {
      case 'easy': return '#52c41a'
      case 'medium': return '#faad14'
      case 'hard': return '#ff4d4f'
      default: return '#d9d9d9'
    }
  }

  const renderRealTimeMetrics = () => {
    if (!showRealTime) return null

    return (
      <Card size="small" title="Real-time Performance">
        <Row gutter={[8, 8]}>
          <Col span={12}>
            <Statistic
              title="Response Time"
              value={currentMetrics.responseTime}
              suffix="ms"
              precision={0}
              valueStyle={{ 
                fontSize: '14px',
                color: currentMetrics.responseTime < 1500 ? '#52c41a' : '#faad14'
              }}
              prefix={<ClockCircleOutlined />}
            />
          </Col>
          <Col span={12}>
            <Statistic
              title="Token Usage"
              value={currentMetrics.tokenUsage}
              precision={0}
              valueStyle={{ 
                fontSize: '14px',
                color: currentMetrics.tokenUsage < 1000 ? '#52c41a' : '#faad14'
              }}
              prefix={<BarChartOutlined />}
            />
          </Col>
          <Col span={12}>
            <Statistic
              title="Confidence"
              value={currentMetrics.confidence * 100}
              suffix="%"
              precision={1}
              valueStyle={{ 
                fontSize: '14px',
                color: currentMetrics.confidence > 0.9 ? '#52c41a' : '#faad14'
              }}
              prefix={<TrophyOutlined />}
            />
          </Col>
          <Col span={12}>
            <Statistic
              title="Cost"
              value={currentMetrics.cost}
              prefix="$"
              precision={3}
              valueStyle={{ 
                fontSize: '14px',
                color: currentMetrics.cost < 0.05 ? '#52c41a' : '#faad14'
              }}
              prefix={<DollarOutlined />}
            />
          </Col>
        </Row>

        <div style={{ marginTop: '16px' }}>
          <Space direction="vertical" style={{ width: '100%' }}>
            <div>
              <Text strong style={{ fontSize: '12px' }}>Success Rate</Text>
              <Progress
                percent={currentMetrics.successRate * 100}
                size="small"
                status={currentMetrics.successRate > 0.95 ? 'success' : 'normal'}
                format={(percent) => `${percent?.toFixed(1)}%`}
              />
            </div>
            <div>
              <Text strong style={{ fontSize: '12px' }}>Throughput</Text>
              <Progress
                percent={Math.min(100, (currentMetrics.throughput / 20) * 100)}
                size="small"
                format={() => `${currentMetrics.throughput.toFixed(1)} q/min`}
              />
            </div>
          </Space>
        </div>
      </Card>
    )
  }

  const renderHistoricalChart = () => {
    if (!showHistorical) return null

    const config = {
      data: historicalData,
      xField: 'time',
      yField: 'responseTime',
      height: 150,
      smooth: true,
      point: {
        size: 3,
        shape: 'circle',
      },
      line: {
        color: '#1890ff',
      },
      xAxis: {
        label: {
          style: {
            fontSize: 10,
          },
        },
      },
      yAxis: {
        label: {
          style: {
            fontSize: 10,
          },
        },
      },
    }

    return (
      <Card size="small" title="Performance Trends">
        <Line {...config} />
        <div style={{ marginTop: '8px', textAlign: 'center' }}>
          <Text type="secondary" style={{ fontSize: '11px' }}>
            Response time over last 20 minutes
          </Text>
        </div>
      </Card>
    )
  }

  const renderOptimizations = () => {
    if (!showOptimizations) return null

    return (
      <Card size="small" title="Optimization Suggestions">
        <Space direction="vertical" style={{ width: '100%' }}>
          {optimizationSuggestions.map(suggestion => (
            <Card key={suggestion.id} size="small" style={{ background: '#fafafa' }}>
              <Space direction="vertical" style={{ width: '100%' }}>
                <Space style={{ width: '100%', justifyContent: 'space-between' }}>
                  <Text strong style={{ fontSize: '12px' }}>{suggestion.title}</Text>
                  <Space>
                    <Tag color={getImpactColor(suggestion.impact)} style={{ fontSize: '10px' }}>
                      {suggestion.impact} impact
                    </Tag>
                    <Tag color={getEffortColor(suggestion.effort)} style={{ fontSize: '10px' }}>
                      {suggestion.effort}
                    </Tag>
                  </Space>
                </Space>
                
                <Text style={{ fontSize: '11px' }}>{suggestion.description}</Text>
                
                <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                  <Text type="secondary" style={{ fontSize: '10px' }}>
                    Estimated improvement: {suggestion.estimatedImprovement}%
                  </Text>
                  <Progress
                    percent={suggestion.estimatedImprovement}
                    size="small"
                    style={{ width: '60px' }}
                    format={() => ''}
                  />
                </div>
              </Space>
            </Card>
          ))}
        </Space>
      </Card>
    )
  }

  const renderPerformanceAlerts = () => {
    const alerts = []

    if (currentMetrics.responseTime > 2000) {
      alerts.push({
        type: 'warning' as const,
        message: 'High response time detected',
        description: 'Consider optimizing query complexity'
      })
    }

    if (currentMetrics.confidence < 0.8) {
      alerts.push({
        type: 'error' as const,
        message: 'Low confidence responses',
        description: 'Review recent queries for accuracy'
      })
    }

    if (currentMetrics.cost > 0.1) {
      alerts.push({
        type: 'warning' as const,
        message: 'High cost per query',
        description: 'Token usage optimization recommended'
      })
    }

    if (alerts.length === 0) return null

    return (
      <Card size="small" title="Performance Alerts">
        <Space direction="vertical" style={{ width: '100%' }}>
          {alerts.map((alert, index) => (
            <Alert
              key={index}
              type={alert.type}
              message={alert.message}
              description={alert.description}
              size="small"
              showIcon
            />
          ))}
        </Space>
      </Card>
    )
  }

  return (
    <div className={`performance-metrics ${className}`}>
      <Space direction="vertical" style={{ width: '100%' }}>
        {/* Performance Alerts */}
        {renderPerformanceAlerts()}

        {/* Real-time Metrics */}
        {renderRealTimeMetrics()}

        {/* Historical Trends */}
        {renderHistoricalChart()}

        {/* Optimization Suggestions */}
        {renderOptimizations()}

        {/* Performance Summary */}
        <Card size="small" title="Performance Summary">
          <Space direction="vertical" style={{ width: '100%' }}>
            <div style={{ display: 'flex', justifyContent: 'space-between' }}>
              <Text style={{ fontSize: '11px' }}>Overall Health:</Text>
              <Space>
                <CheckCircleOutlined style={{ color: '#52c41a' }} />
                <Text style={{ fontSize: '11px', color: '#52c41a' }}>Good</Text>
              </Space>
            </div>
            <div style={{ display: 'flex', justifyContent: 'space-between' }}>
              <Text style={{ fontSize: '11px' }}>Optimization Potential:</Text>
              <Text style={{ fontSize: '11px' }}>
                {optimizationSuggestions.reduce((sum, opt) => sum + opt.estimatedImprovement, 0) / optimizationSuggestions.length}% avg
              </Text>
            </div>
            <div style={{ display: 'flex', justifyContent: 'space-between' }}>
              <Text style={{ fontSize: '11px' }}>Last Updated:</Text>
              <Text style={{ fontSize: '11px' }}>{new Date().toLocaleTimeString()}</Text>
            </div>
          </Space>
        </Card>
      </Space>
    </div>
  )
}

export default PerformanceMetrics
