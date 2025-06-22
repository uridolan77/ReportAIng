import React, { useState, useMemo } from 'react'
import { 
  Card, 
  Space, 
  Typography, 
  Table, 
  Tag, 
  Progress,
  Row, 
  Col, 
  Statistic,
  Select,
  Button,
  Tooltip,
  Alert
} from 'antd'
import {
  RobotOutlined,
  TrophyOutlined,
  ClockCircleOutlined,
  DollarOutlined,
  BarChartOutlined,
  ThunderboltOutlined,
  CheckCircleOutlined,
  ExclamationCircleOutlined
} from '@ant-design/icons'
import { 
  BarChart, 
  Bar, 
  RadarChart, 
  PolarGrid, 
  PolarAngleAxis, 
  PolarRadiusAxis, 
  Radar,
  XAxis, 
  YAxis, 
  CartesianGrid, 
  Tooltip as RechartsTooltip, 
  Legend,
  ResponsiveContainer 
} from 'recharts'
import { ConfidenceIndicator } from '../common/ConfidenceIndicator'

const { Title, Text } = Typography

export interface ModelPerformanceComparisonProps {
  models: ModelPerformanceData[]
  comparisonMetrics?: string[]
  showRadarChart?: boolean
  showRecommendations?: boolean
  onModelSelect?: (model: ModelPerformanceData) => void
  className?: string
  testId?: string
}

export interface ModelPerformanceData {
  id: string
  name: string
  version?: string
  provider: string
  metrics: {
    averageConfidence: number
    successRate: number
    averageResponseTime: number
    tokenEfficiency: number
    costPerQuery: number
    totalQueries: number
    errorRate: number
  }
  capabilities: string[]
  lastUpdated: string
}

interface ComparisonAnalysis {
  bestOverall: ModelPerformanceData | null
  bestConfidence: ModelPerformanceData | null
  fastestResponse: ModelPerformanceData | null
  mostCostEffective: ModelPerformanceData | null
  recommendations: string[]
}

/**
 * ModelPerformanceComparison - Compares AI model performance across multiple metrics
 * 
 * Features:
 * - Multi-model performance comparison
 * - Radar chart visualization
 * - Detailed metrics breakdown
 * - Cost-benefit analysis
 * - Performance recommendations
 * - Model selection guidance
 */
export const ModelPerformanceComparison: React.FC<ModelPerformanceComparisonProps> = ({
  models,
  comparisonMetrics = ['confidence', 'speed', 'cost', 'accuracy'],
  showRadarChart = true,
  showRecommendations = true,
  onModelSelect,
  className,
  testId = 'model-performance-comparison'
}) => {
  const [selectedMetric, setSelectedMetric] = useState<string>('overall')
  const [sortBy, setSortBy] = useState<string>('averageConfidence')

  // Analyze model performance
  const analysis = useMemo((): ComparisonAnalysis => {
    if (!models || models.length === 0) {
      return {
        bestOverall: null,
        bestConfidence: null,
        fastestResponse: null,
        mostCostEffective: null,
        recommendations: []
      }
    }

    // Calculate overall scores
    const modelsWithScores = models.map(model => ({
      ...model,
      overallScore: (
        model.metrics.averageConfidence * 0.3 +
        model.metrics.successRate * 0.25 +
        (1 - model.metrics.averageResponseTime / 10000) * 0.2 + // Normalize response time
        model.metrics.tokenEfficiency * 0.15 +
        (1 - model.metrics.costPerQuery) * 0.1 // Lower cost is better
      )
    }))

    const bestOverall = modelsWithScores.reduce((best, current) => 
      current.overallScore > (best?.overallScore || 0) ? current : best, modelsWithScores[0])

    const bestConfidence = models.reduce((best, current) => 
      current.metrics.averageConfidence > (best?.metrics.averageConfidence || 0) ? current : best, models[0])

    const fastestResponse = models.reduce((fastest, current) => 
      current.metrics.averageResponseTime < (fastest?.metrics.averageResponseTime || Infinity) ? current : fastest, models[0])

    const mostCostEffective = models.reduce((cheapest, current) => 
      current.metrics.costPerQuery < (cheapest?.metrics.costPerQuery || Infinity) ? current : cheapest, models[0])

    // Generate recommendations
    const recommendations: string[] = []
    if (bestOverall) {
      recommendations.push(`${bestOverall.name} offers the best overall performance balance`)
    }
    if (bestConfidence && bestConfidence.id !== bestOverall?.id) {
      recommendations.push(`${bestConfidence.name} provides highest confidence scores`)
    }
    if (fastestResponse && fastestResponse.id !== bestOverall?.id) {
      recommendations.push(`${fastestResponse.name} offers fastest response times`)
    }
    if (mostCostEffective && mostCostEffective.id !== bestOverall?.id) {
      recommendations.push(`${mostCostEffective.name} is most cost-effective`)
    }

    return {
      bestOverall,
      bestConfidence,
      fastestResponse,
      mostCostEffective,
      recommendations
    }
  }, [models])

  // Prepare radar chart data
  const radarData = useMemo(() => {
    if (!models || models.length === 0) return []

    const metrics = ['Confidence', 'Success Rate', 'Speed', 'Efficiency', 'Cost Effectiveness']
    
    return metrics.map(metric => {
      const dataPoint: any = { metric }
      
      models.forEach(model => {
        let value = 0
        switch (metric) {
          case 'Confidence':
            value = model.metrics.averageConfidence * 100
            break
          case 'Success Rate':
            value = model.metrics.successRate * 100
            break
          case 'Speed':
            value = Math.max(0, 100 - (model.metrics.averageResponseTime / 100)) // Inverse for speed
            break
          case 'Efficiency':
            value = model.metrics.tokenEfficiency * 100
            break
          case 'Cost Effectiveness':
            value = Math.max(0, 100 - (model.metrics.costPerQuery * 1000)) // Inverse for cost
            break
        }
        dataPoint[model.name] = Math.min(100, Math.max(0, value))
      })
      
      return dataPoint
    })
  }, [models])

  // Prepare comparison chart data
  const comparisonData = useMemo(() => {
    return models.map(model => ({
      name: model.name,
      confidence: model.metrics.averageConfidence * 100,
      successRate: model.metrics.successRate * 100,
      responseTime: model.metrics.averageResponseTime,
      efficiency: model.metrics.tokenEfficiency * 100,
      cost: model.metrics.costPerQuery * 1000 // Convert to more readable scale
    }))
  }, [models])

  const getPerformanceColor = (value: number, metric: string) => {
    const thresholds = {
      confidence: { good: 80, fair: 60 },
      successRate: { good: 90, fair: 70 },
      responseTime: { good: 1000, fair: 3000 }, // Lower is better
      efficiency: { good: 80, fair: 60 },
      cost: { good: 0.01, fair: 0.05 } // Lower is better
    }

    const threshold = thresholds[metric as keyof typeof thresholds]
    if (!threshold) return '#1890ff'

    if (metric === 'responseTime' || metric === 'cost') {
      // Lower is better
      if (value <= threshold.good) return '#52c41a'
      if (value <= threshold.fair) return '#faad14'
      return '#ff4d4f'
    } else {
      // Higher is better
      if (value >= threshold.good) return '#52c41a'
      if (value >= threshold.fair) return '#faad14'
      return '#ff4d4f'
    }
  }

  const columns = [
    {
      title: 'Model',
      dataIndex: 'name',
      key: 'name',
      render: (name: string, record: ModelPerformanceData) => (
        <Space direction="vertical" size="small">
          <Space>
            <RobotOutlined />
            <Text strong>{name}</Text>
            {record.id === analysis.bestOverall?.id && (
              <Tag color="gold" icon={<TrophyOutlined />}>Best Overall</Tag>
            )}
          </Space>
          <Text type="secondary" style={{ fontSize: '12px' }}>
            {record.provider} • {record.version || 'Latest'}
          </Text>
        </Space>
      )
    },
    {
      title: 'Confidence',
      dataIndex: ['metrics', 'averageConfidence'],
      key: 'confidence',
      render: (confidence: number) => (
        <ConfidenceIndicator
          confidence={confidence}
          size="small"
          type="badge"
          showPercentage
        />
      ),
      sorter: (a: ModelPerformanceData, b: ModelPerformanceData) => 
        a.metrics.averageConfidence - b.metrics.averageConfidence
    },
    {
      title: 'Success Rate',
      dataIndex: ['metrics', 'successRate'],
      key: 'successRate',
      render: (rate: number) => (
        <Space>
          <Progress 
            percent={rate * 100} 
            size="small" 
            strokeColor={getPerformanceColor(rate * 100, 'successRate')}
            format={(percent) => `${percent?.toFixed(1)}%`}
          />
        </Space>
      ),
      sorter: (a: ModelPerformanceData, b: ModelPerformanceData) => 
        a.metrics.successRate - b.metrics.successRate
    },
    {
      title: 'Avg Response Time',
      dataIndex: ['metrics', 'averageResponseTime'],
      key: 'responseTime',
      render: (time: number) => (
        <Space>
          <ClockCircleOutlined style={{ color: getPerformanceColor(time, 'responseTime') }} />
          <Text style={{ color: getPerformanceColor(time, 'responseTime') }}>
            {time}ms
          </Text>
        </Space>
      ),
      sorter: (a: ModelPerformanceData, b: ModelPerformanceData) => 
        a.metrics.averageResponseTime - b.metrics.averageResponseTime
    },
    {
      title: 'Cost per Query',
      dataIndex: ['metrics', 'costPerQuery'],
      key: 'cost',
      render: (cost: number) => (
        <Space>
          <DollarOutlined style={{ color: getPerformanceColor(cost, 'cost') }} />
          <Text style={{ color: getPerformanceColor(cost, 'cost') }}>
            ${cost.toFixed(4)}
          </Text>
        </Space>
      ),
      sorter: (a: ModelPerformanceData, b: ModelPerformanceData) => 
        a.metrics.costPerQuery - b.metrics.costPerQuery
    },
    {
      title: 'Total Queries',
      dataIndex: ['metrics', 'totalQueries'],
      key: 'totalQueries',
      render: (queries: number) => (
        <Space>
          <BarChartOutlined />
          <Text>{queries.toLocaleString()}</Text>
        </Space>
      ),
      sorter: (a: ModelPerformanceData, b: ModelPerformanceData) => 
        a.metrics.totalQueries - b.metrics.totalQueries
    },
    {
      title: 'Actions',
      key: 'actions',
      render: (_, record: ModelPerformanceData) => (
        <Button
          type="link"
          size="small"
          onClick={() => onModelSelect?.(record)}
        >
          Select
        </Button>
      )
    }
  ]

  if (!models || models.length === 0) {
    return (
      <Card className={className} data-testid={testId}>
        <div style={{ textAlign: 'center', padding: '40px 0' }}>
          <Space direction="vertical">
            <RobotOutlined style={{ fontSize: '48px', color: '#d9d9d9' }} />
            <Text type="secondary">No model performance data available</Text>
          </Space>
        </div>
      </Card>
    )
  }

  return (
    <Card 
      title={
        <Space>
          <RobotOutlined />
          <span>Model Performance Comparison</span>
          <Tag color="blue">{models.length} models</Tag>
        </Space>
      }
      extra={
        <Space>
          <Select
            value={sortBy}
            onChange={setSortBy}
            style={{ width: 150 }}
          >
            <Select.Option value="averageConfidence">Confidence</Select.Option>
            <Select.Option value="successRate">Success Rate</Select.Option>
            <Select.Option value="averageResponseTime">Response Time</Select.Option>
            <Select.Option value="costPerQuery">Cost</Select.Option>
          </Select>
        </Space>
      }
      className={className}
      data-testid={testId}
    >
      <Space direction="vertical" style={{ width: '100%' }} size="large">
        {/* Performance Leaders */}
        <Row gutter={[16, 16]}>
          <Col xs={24} sm={12} lg={6}>
            <Card size="small">
              <Statistic
                title="Best Overall"
                value={analysis.bestOverall?.name || 'N/A'}
                prefix={<TrophyOutlined style={{ color: '#faad14' }} />}
              />
            </Card>
          </Col>
          <Col xs={24} sm={12} lg={6}>
            <Card size="small">
              <Statistic
                title="Highest Confidence"
                value={analysis.bestConfidence?.name || 'N/A'}
                prefix={<CheckCircleOutlined style={{ color: '#52c41a' }} />}
              />
            </Card>
          </Col>
          <Col xs={24} sm={12} lg={6}>
            <Card size="small">
              <Statistic
                title="Fastest Response"
                value={analysis.fastestResponse?.name || 'N/A'}
                prefix={<ThunderboltOutlined style={{ color: '#1890ff' }} />}
              />
            </Card>
          </Col>
          <Col xs={24} sm={12} lg={6}>
            <Card size="small">
              <Statistic
                title="Most Cost-Effective"
                value={analysis.mostCostEffective?.name || 'N/A'}
                prefix={<DollarOutlined style={{ color: '#722ed1' }} />}
              />
            </Card>
          </Col>
        </Row>

        {/* Radar Chart */}
        {showRadarChart && (
          <Row gutter={[16, 16]}>
            <Col xs={24} lg={12}>
              <Card title="Performance Radar" size="small">
                <ResponsiveContainer width="100%" height={300}>
                  <RadarChart data={radarData}>
                    <PolarGrid />
                    <PolarAngleAxis dataKey="metric" />
                    <PolarRadiusAxis domain={[0, 100]} />
                    {models.map((model, index) => (
                      <Radar
                        key={model.id}
                        name={model.name}
                        dataKey={model.name}
                        stroke={`hsl(${index * 360 / models.length}, 70%, 50%)`}
                        fill={`hsl(${index * 360 / models.length}, 70%, 50%)`}
                        fillOpacity={0.1}
                      />
                    ))}
                    <Legend />
                  </RadarChart>
                </ResponsiveContainer>
              </Card>
            </Col>
            <Col xs={24} lg={12}>
              <Card title="Metric Comparison" size="small">
                <ResponsiveContainer width="100%" height={300}>
                  <BarChart data={comparisonData}>
                    <CartesianGrid strokeDasharray="3 3" />
                    <XAxis dataKey="name" />
                    <YAxis />
                    <RechartsTooltip />
                    <Legend />
                    <Bar dataKey="confidence" fill="#52c41a" name="Confidence %" />
                    <Bar dataKey="successRate" fill="#1890ff" name="Success Rate %" />
                  </BarChart>
                </ResponsiveContainer>
              </Card>
            </Col>
          </Row>
        )}

        {/* Detailed Comparison Table */}
        <Card title="Detailed Comparison" size="small">
          <Table
            columns={columns}
            dataSource={models}
            rowKey="id"
            pagination={false}
            size="small"
          />
        </Card>

        {/* Recommendations */}
        {showRecommendations && analysis.recommendations.length > 0 && (
          <Card title="Recommendations" size="small">
            <Space direction="vertical" style={{ width: '100%' }}>
              {analysis.recommendations.map((recommendation, index) => (
                <Alert
                  key={index}
                  message={recommendation}
                  type="info"
                  showIcon
                />
              ))}
            </Space>
          </Card>
        )}
      </Space>
    </Card>
  )
}

export default ModelPerformanceComparison
