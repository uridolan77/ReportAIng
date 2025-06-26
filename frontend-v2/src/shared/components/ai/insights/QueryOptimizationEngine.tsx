import React, { useState, useMemo } from 'react'
import { 
  Card, 
  Typography, 
  Space, 
  Button,
  Alert,
  List,
  Tag,
  Progress,
  Collapse,
  Row,
  Col,
  Statistic,
  Tooltip,
  Badge,
  Divider,
  Timeline
} from 'antd'
import {
  ThunderboltOutlined,
  BulbOutlined,
  ClockCircleOutlined,
  TrophyOutlined,
  ExclamationCircleOutlined,
  CheckCircleOutlined,
  RobotOutlined,
  BarChartOutlined,
  ReloadOutlined,
  PlayCircleOutlined
} from '@ant-design/icons'
import { ConfidenceIndicator } from '../common/ConfidenceIndicator'
import { useGetQueryOptimizationQuery, useApplyOptimizationMutation } from '@shared/store/api/intelligentAgentsApi'
import type { QueryOptimization, OptimizationSuggestion, PerformanceMetrics } from '@shared/types/intelligentAgents'

const { Title, Text, Paragraph } = Typography

export interface QueryOptimizationEngineProps {
  query: string
  showPerformanceMetrics?: boolean
  showOptimizationHistory?: boolean
  autoOptimize?: boolean
  onOptimizationApply?: (optimization: QueryOptimization) => void
  onQueryUpdate?: (optimizedQuery: string) => void
  className?: string
  testId?: string
}

/**
 * QueryOptimizationEngine - AI-powered query optimization and performance analysis
 * 
 * Features:
 * - Real-time query performance analysis
 * - AI-powered optimization suggestions with confidence scoring
 * - Performance metrics tracking and comparison
 * - Automated optimization application
 * - Query execution plan analysis
 * - Historical optimization tracking
 */
export const QueryOptimizationEngine: React.FC<QueryOptimizationEngineProps> = ({
  query,
  showPerformanceMetrics = true,
  showOptimizationHistory = true,
  autoOptimize = false,
  onOptimizationApply,
  onQueryUpdate,
  className,
  testId = 'query-optimization-engine'
}) => {
  const [selectedOptimization, setSelectedOptimization] = useState<QueryOptimization | null>(null)
  const [optimizationHistory, setOptimizationHistory] = useState<QueryOptimization[]>([])

  // Real API data
  const { data: optimization, isLoading: optimizationLoading, refetch: refetchOptimization } = 
    useGetQueryOptimizationQuery({ query }, { skip: !query })
  
  const [applyOptimization, { isLoading: applyLoading }] = useApplyOptimizationMutation()

  // Calculate performance improvement
  const performanceImprovement = useMemo(() => {
    if (!optimization?.suggestions.length) return null
    
    const bestSuggestion = optimization.suggestions.reduce((best, current) => 
      current.expectedImprovement > best.expectedImprovement ? current : best
    )
    
    return {
      timeReduction: bestSuggestion.expectedImprovement,
      costReduction: bestSuggestion.costImpact || 0,
      complexityReduction: bestSuggestion.complexityReduction || 0
    }
  }, [optimization])

  // Handle optimization application
  const handleApplyOptimization = async (optimizationToApply: QueryOptimization) => {
    try {
      const result = await applyOptimization({
        queryId: optimization?.queryId || '',
        optimizationId: optimizationToApply.id
      }).unwrap()
      
      setOptimizationHistory(prev => [optimizationToApply, ...prev])
      setSelectedOptimization(optimizationToApply)
      onOptimizationApply?.(optimizationToApply)
      onQueryUpdate?.(result.optimizedQuery)
    } catch (error) {
      console.error('Failed to apply optimization:', error)
    }
  }

  // Get optimization type color
  const getOptimizationTypeColor = (type: string) => {
    switch (type) {
      case 'index': return '#1890ff'
      case 'join': return '#52c41a'
      case 'filter': return '#faad14'
      case 'aggregation': return '#722ed1'
      case 'subquery': return '#13c2c2'
      default: return '#d9d9d9'
    }
  }

  // Get impact level color
  const getImpactColor = (impact: string) => {
    switch (impact) {
      case 'high': return '#52c41a'
      case 'medium': return '#faad14'
      case 'low': return '#ff7875'
      default: return '#d9d9d9'
    }
  }

  // Render performance metrics
  const renderPerformanceMetrics = () => {
    if (!showPerformanceMetrics || !optimization?.currentPerformance) return null

    const metrics = optimization.currentPerformance

    return (
      <Card 
        title={
          <Space>
            <BarChartOutlined />
            <span>Performance Analysis</span>
          </Space>
        }
        size="small"
        style={{ marginBottom: 16 }}
      >
        <Row gutter={[16, 16]}>
          <Col xs={24} sm={12} lg={6}>
            <Statistic
              title="Execution Time"
              value={metrics.executionTime}
              suffix="ms"
              prefix={<ClockCircleOutlined />}
              valueStyle={{ color: metrics.executionTime > 5000 ? '#ff4d4f' : '#52c41a' }}
            />
          </Col>
          <Col xs={24} sm={12} lg={6}>
            <Statistic
              title="Complexity Score"
              value={metrics.complexityScore}
              prefix={<ThunderboltOutlined />}
              valueStyle={{ color: metrics.complexityScore > 7 ? '#ff4d4f' : '#52c41a' }}
            />
          </Col>
          <Col xs={24} sm={12} lg={6}>
            <Statistic
              title="Resource Usage"
              value={metrics.resourceUsage}
              suffix="%"
              prefix={<TrophyOutlined />}
              valueStyle={{ color: metrics.resourceUsage > 80 ? '#ff4d4f' : '#52c41a' }}
            />
          </Col>
          <Col xs={24} sm={12} lg={6}>
            <Statistic
              title="Optimization Score"
              value={optimization.optimizationScore * 100}
              suffix="%"
              precision={1}
              prefix={<RobotOutlined />}
              valueStyle={{ color: '#1890ff' }}
            />
          </Col>
        </Row>

        {performanceImprovement && (
          <div style={{ marginTop: 16 }}>
            <Alert
              message="Optimization Potential"
              description={
                <Space direction="vertical" style={{ width: '100%' }}>
                  <Text>AI has identified potential improvements:</Text>
                  <Row gutter={[16, 8]}>
                    <Col span={8}>
                      <Text strong>Time Reduction: </Text>
                      <Text style={{ color: '#52c41a' }}>
                        {(performanceImprovement.timeReduction * 100).toFixed(1)}%
                      </Text>
                    </Col>
                    <Col span={8}>
                      <Text strong>Cost Reduction: </Text>
                      <Text style={{ color: '#52c41a' }}>
                        {(performanceImprovement.costReduction * 100).toFixed(1)}%
                      </Text>
                    </Col>
                    <Col span={8}>
                      <Text strong>Complexity: </Text>
                      <Text style={{ color: '#52c41a' }}>
                        -{(performanceImprovement.complexityReduction * 100).toFixed(1)}%
                      </Text>
                    </Col>
                  </Row>
                </Space>
              }
              type="success"
              showIcon
            />
          </div>
        )}
      </Card>
    )
  }

  // Render optimization suggestions
  const renderOptimizationSuggestions = () => {
    if (!optimization?.suggestions.length) return null

    return (
      <Card 
        title={
          <Space>
            <BulbOutlined />
            <span>AI Optimization Suggestions</span>
            <Badge count={optimization.suggestions.length} />
          </Space>
        }
        size="small"
        style={{ marginBottom: 16 }}
      >
        <List
          dataSource={optimization.suggestions}
          renderItem={(suggestion, index) => (
            <List.Item
              actions={[
                <Tooltip title={`Confidence: ${(suggestion.confidence * 100).toFixed(1)}%`}>
                  <ConfidenceIndicator
                    confidence={suggestion.confidence}
                    size="small"
                    type="badge"
                    showPercentage={false}
                  />
                </Tooltip>,
                <Button 
                  type="primary" 
                  size="small"
                  loading={applyLoading}
                  onClick={() => handleApplyOptimization({
                    id: `opt-${index}`,
                    type: suggestion.type,
                    description: suggestion.description,
                    optimizedQuery: suggestion.optimizedQuery,
                    confidence: suggestion.confidence,
                    expectedImprovement: suggestion.expectedImprovement,
                    appliedAt: new Date().toISOString()
                  })}
                  icon={<PlayCircleOutlined />}
                >
                  Apply
                </Button>
              ]}
            >
              <List.Item.Meta
                avatar={
                  <div style={{ display: 'flex', alignItems: 'center' }}>
                    <RobotOutlined style={{ color: getOptimizationTypeColor(suggestion.type) }} />
                  </div>
                }
                title={
                  <Space>
                    <Text strong>{suggestion.title}</Text>
                    <Tag color={getOptimizationTypeColor(suggestion.type)}>
                      {suggestion.type.toUpperCase()}
                    </Tag>
                    <Tag color={getImpactColor(suggestion.impact)}>
                      {suggestion.impact.toUpperCase()} IMPACT
                    </Tag>
                  </Space>
                }
                description={
                  <div>
                    <Paragraph style={{ marginBottom: 8 }}>
                      {suggestion.description}
                    </Paragraph>
                    <Space>
                      <Text type="secondary">Expected improvement:</Text>
                      <Text strong style={{ color: '#52c41a' }}>
                        {(suggestion.expectedImprovement * 100).toFixed(1)}%
                      </Text>
                    </Space>
                    {suggestion.reasoning && (
                      <div style={{ marginTop: 4 }}>
                        <Text type="secondary" style={{ fontSize: '12px', fontStyle: 'italic' }}>
                          Reasoning: {suggestion.reasoning}
                        </Text>
                      </div>
                    )}
                  </div>
                }
              />
            </List.Item>
          )}
        />
      </Card>
    )
  }

  // Render optimization history
  const renderOptimizationHistory = () => {
    if (!showOptimizationHistory || !optimizationHistory.length) return null

    return (
      <Card 
        title={
          <Space>
            <ClockCircleOutlined />
            <span>Optimization History</span>
          </Space>
        }
        size="small"
      >
        <Timeline>
          {optimizationHistory.map((opt, index) => (
            <Timeline.Item
              key={index}
              dot={<CheckCircleOutlined style={{ color: '#52c41a' }} />}
            >
              <Space direction="vertical" size="small">
                <div>
                  <Text strong>{opt.type.toUpperCase()} Optimization</Text>
                  <Text type="secondary" style={{ marginLeft: 8 }}>
                    {new Date(opt.appliedAt).toLocaleString()}
                  </Text>
                </div>
                <Text>{opt.description}</Text>
                <Space>
                  <Text type="secondary">Improvement:</Text>
                  <Text style={{ color: '#52c41a' }}>
                    {(opt.expectedImprovement * 100).toFixed(1)}%
                  </Text>
                  <ConfidenceIndicator
                    confidence={opt.confidence}
                    size="small"
                    type="badge"
                    showPercentage={false}
                  />
                </Space>
              </Space>
            </Timeline.Item>
          ))}
        </Timeline>
      </Card>
    )
  }

  // Render query analysis
  const renderQueryAnalysis = () => {
    if (!optimization?.analysis) return null

    return (
      <Card 
        title={
          <Space>
            <ExclamationCircleOutlined />
            <span>Query Analysis</span>
          </Space>
        }
        size="small"
        style={{ marginBottom: 16 }}
      >
        <Collapse
          size="small"
          items={[
            {
              key: 'execution-plan',
              label: 'Execution Plan Analysis',
              children: (
                <Space direction="vertical" style={{ width: '100%' }}>
                  {optimization.analysis.bottlenecks.map((bottleneck, index) => (
                    <Alert
                      key={index}
                      message={bottleneck.type}
                      description={bottleneck.description}
                      type="warning"
                      showIcon
                      style={{ marginBottom: 8 }}
                    />
                  ))}
                </Space>
              )
            },
            {
              key: 'insights',
              label: 'Performance Insights',
              children: (
                <List
                  size="small"
                  dataSource={optimization.analysis.insights}
                  renderItem={(insight) => (
                    <List.Item>
                      <Space>
                        <BulbOutlined style={{ color: '#faad14' }} />
                        <Text>{insight}</Text>
                      </Space>
                    </List.Item>
                  )}
                />
              )
            }
          ]}
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
          Query Optimization Engine
        </Title>
        <Space>
          <Button 
            icon={<ReloadOutlined />}
            loading={optimizationLoading}
            onClick={() => refetchOptimization()}
          >
            Analyze
          </Button>
        </Space>
      </div>

      {/* Query Context */}
      {query && (
        <Alert
          message="Query Analysis"
          description={
            <div>
              <Text>Analyzing query for optimization opportunities...</Text>
              <div style={{ 
                marginTop: 8, 
                padding: 8, 
                backgroundColor: '#f5f5f5', 
                borderRadius: 4,
                fontFamily: 'monospace',
                fontSize: '12px'
              }}>
                {query.length > 200 ? `${query.substring(0, 200)}...` : query}
              </div>
            </div>
          }
          type="info"
          showIcon
          style={{ marginBottom: 16 }}
        />
      )}

      {/* Loading State */}
      {optimizationLoading && (
        <Card style={{ textAlign: 'center', padding: '40px 0' }}>
          <Space direction="vertical">
            <RobotOutlined style={{ fontSize: '48px', color: '#1890ff' }} />
            <Text>AI is analyzing your query for optimization opportunities...</Text>
            <Progress percent={75} status="active" />
          </Space>
        </Card>
      )}

      {/* Optimization Results */}
      {optimization && !optimizationLoading && (
        <div>
          {/* Performance Metrics */}
          {renderPerformanceMetrics()}

          {/* Query Analysis */}
          {renderQueryAnalysis()}

          {/* Optimization Suggestions */}
          {renderOptimizationSuggestions()}

          {/* Optimization History */}
          {renderOptimizationHistory()}
        </div>
      )}

      {/* No Optimization Available */}
      {!optimization && !optimizationLoading && query && (
        <Card style={{ textAlign: 'center', padding: '40px 0' }}>
          <Space direction="vertical">
            <CheckCircleOutlined style={{ fontSize: '48px', color: '#52c41a' }} />
            <Title level={4}>Query Already Optimized</Title>
            <Text type="secondary">
              Your query appears to be well-optimized. No significant improvements were identified.
            </Text>
          </Space>
        </Card>
      )}
    </div>
  )
}

export default QueryOptimizationEngine
