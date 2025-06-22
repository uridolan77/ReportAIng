import React, { useState, useMemo, useCallback } from 'react'
import { 
  Card, 
  Tabs, 
  Space, 
  Typography, 
  Tag, 
  Button, 
  Descriptions,
  Row,
  Col,
  Statistic,
  Alert,
  Timeline,
  Progress,
  Tooltip,
  Divider,
  Collapse
} from 'antd'
import {
  EyeOutlined,
  ClockCircleOutlined,
  BarChartOutlined,
  UserOutlined,
  DatabaseOutlined,
  ThunderboltOutlined,
  CheckCircleOutlined,
  ExclamationCircleOutlined,
  InfoCircleOutlined,
  DownloadOutlined,
  ShareAltOutlined
} from '@ant-design/icons'
import { ConfidenceIndicator } from '../common/ConfidenceIndicator'
import { QueryFlowAnalyzer } from './QueryFlowAnalyzer'
import { StepByStepAnalyzer } from './StepByStepAnalyzer'
import { ConfidenceTracker } from './ConfidenceTracker'
import { usePerformanceMonitor, transparencyCache, createMemoizedSelector } from '@shared/utils/performance'
import type { TransparencyTrace } from '@shared/types/transparency'
import dayjs from 'dayjs'

const { Title, Text, Paragraph } = Typography
const { TabPane } = Tabs
const { Panel } = Collapse

export interface TraceDetailViewerProps {
  trace: TransparencyTrace
  showAllTabs?: boolean
  defaultActiveTab?: string
  onExport?: (trace: TransparencyTrace) => void
  onShare?: (trace: TransparencyTrace) => void
  className?: string
  testId?: string
}

interface TraceMetrics {
  totalSteps: number
  successfulSteps: number
  failedSteps: number
  totalProcessingTime: number
  totalTokens: number
  averageConfidence: number
  successRate: number
}

/**
 * TraceDetailViewer - Comprehensive viewer for transparency trace details
 * 
 * Features:
 * - Complete trace information display
 * - Multi-tab detailed analysis
 * - Performance metrics overview
 * - Step-by-step breakdown
 * - Business context analysis
 * - Export and sharing capabilities
 */
export const TraceDetailViewer: React.FC<TraceDetailViewerProps> = ({
  trace,
  showAllTabs = true,
  defaultActiveTab = 'overview',
  onExport,
  onShare,
  className,
  testId = 'trace-detail-viewer'
}) => {
  const [activeTab, setActiveTab] = useState(defaultActiveTab)

  // Performance monitoring
  const { renderTime } = usePerformanceMonitor('TraceDetailViewer')

  // Memoized selectors for performance
  const selectMetrics = useMemo(() => createMemoizedSelector(
    (trace: TransparencyTrace) => trace?.steps || [],
    (a, b) => a.length === b.length && a.every((step, i) => step.id === b[i]?.id)
  ), [])

  // Cache trace data
  const cacheKey = `trace-${trace?.traceId}`
  const cachedTrace = useMemo(() => {
    if (!trace) return null

    const cached = transparencyCache.get(cacheKey)
    if (cached) return cached

    transparencyCache.set(cacheKey, trace)
    return trace
  }, [trace, cacheKey])

  // Optimized tab change handler
  const handleTabChange = useCallback((key: string) => {
    setActiveTab(key)
  }, [])

  // Calculate trace metrics with caching
  const metrics = useMemo((): TraceMetrics => {
    if (!cachedTrace?.steps) {
      return {
        totalSteps: 0,
        successfulSteps: 0,
        failedSteps: 0,
        totalProcessingTime: 0,
        totalTokens: 0,
        averageConfidence: 0,
        successRate: 0
      }
    }

    const steps = selectMetrics(cachedTrace)
    const totalSteps = steps.length
    const successfulSteps = steps.filter(step => step.success).length
    const failedSteps = totalSteps - successfulSteps
    const totalProcessingTime = steps.reduce((sum, step) => sum + step.processingTimeMs, 0)
    const totalTokens = steps.reduce((sum, step) => sum + step.tokensAdded, 0)
    const averageConfidence = totalSteps > 0 ? steps.reduce((sum, step) => sum + step.confidence, 0) / totalSteps : 0
    const successRate = totalSteps > 0 ? successfulSteps / totalSteps : 0

    return {
      totalSteps,
      successfulSteps,
      failedSteps,
      totalProcessingTime,
      totalTokens,
      averageConfidence,
      successRate
    }
  }, [cachedTrace, selectMetrics])

  const renderOverviewTab = () => (
    <Space direction="vertical" style={{ width: '100%' }} size="large">
      {/* Basic Information */}
      <Card title="Trace Information" size="small">
        <Descriptions column={2} size="small">
          <Descriptions.Item label="Trace ID">
            <Space>
              <Text code>{trace.traceId}</Text>
              <Button 
                size="small" 
                type="text" 
                icon={<ShareAltOutlined />}
                onClick={() => onShare?.(trace)}
              />
            </Space>
          </Descriptions.Item>
          <Descriptions.Item label="User ID">
            <Space>
              <UserOutlined />
              <Text>{trace.userId}</Text>
            </Space>
          </Descriptions.Item>
          <Descriptions.Item label="Created At">
            <Space>
              <ClockCircleOutlined />
              <Text>{dayjs(trace.createdAt).format('YYYY-MM-DD HH:mm:ss')}</Text>
            </Space>
          </Descriptions.Item>
          <Descriptions.Item label="Intent Type">
            <Tag color="blue">{trace.intentType}</Tag>
          </Descriptions.Item>
          <Descriptions.Item label="Overall Confidence">
            <ConfidenceIndicator
              confidence={trace.overallConfidence}
              size="small"
              type="badge"
              showPercentage
            />
          </Descriptions.Item>
          <Descriptions.Item label="Success">
            <Space>
              {trace.success ? (
                <CheckCircleOutlined style={{ color: '#52c41a' }} />
              ) : (
                <ExclamationCircleOutlined style={{ color: '#ff4d4f' }} />
              )}
              <Text>{trace.success ? 'Yes' : 'No'}</Text>
            </Space>
          </Descriptions.Item>
        </Descriptions>
      </Card>

      {/* User Question */}
      <Card title="User Question" size="small">
        <Paragraph>
          <Text strong>{trace.userQuestion}</Text>
        </Paragraph>
      </Card>

      {/* Performance Metrics */}
      <Card title="Performance Metrics" size="small">
        <Row gutter={[16, 16]}>
          <Col xs={24} sm={12} lg={6}>
            <Statistic
              title="Total Steps"
              value={metrics.totalSteps}
              prefix={<ThunderboltOutlined />}
            />
          </Col>
          <Col xs={24} sm={12} lg={6}>
            <Statistic
              title="Processing Time"
              value={metrics.totalProcessingTime}
              suffix="ms"
              prefix={<ClockCircleOutlined />}
            />
          </Col>
          <Col xs={24} sm={12} lg={6}>
            <Statistic
              title="Total Tokens"
              value={metrics.totalTokens}
              prefix={<BarChartOutlined />}
            />
          </Col>
          <Col xs={24} sm={12} lg={6}>
            <Statistic
              title="Success Rate"
              value={(metrics.successRate * 100).toFixed(1)}
              suffix="%"
              prefix={<CheckCircleOutlined />}
              valueStyle={{ 
                color: metrics.successRate > 0.9 ? '#3f8600' : 
                       metrics.successRate > 0.7 ? '#faad14' : '#cf1322' 
              }}
            />
          </Col>
        </Row>
      </Card>

      {/* Quick Stats */}
      <Row gutter={[16, 16]}>
        <Col span={8}>
          <Card size="small">
            <Statistic
              title="Successful Steps"
              value={metrics.successfulSteps}
              suffix={`/ ${metrics.totalSteps}`}
              valueStyle={{ color: '#3f8600' }}
            />
            <Progress 
              percent={(metrics.successfulSteps / metrics.totalSteps) * 100}
              strokeColor="#52c41a"
              showInfo={false}
              size="small"
            />
          </Card>
        </Col>
        <Col span={8}>
          <Card size="small">
            <Statistic
              title="Average Confidence"
              value={(metrics.averageConfidence * 100).toFixed(1)}
              suffix="%"
              valueStyle={{ 
                color: metrics.averageConfidence > 0.8 ? '#3f8600' : 
                       metrics.averageConfidence > 0.6 ? '#faad14' : '#cf1322' 
              }}
            />
            <Progress 
              percent={metrics.averageConfidence * 100}
              strokeColor={
                metrics.averageConfidence > 0.8 ? '#52c41a' : 
                metrics.averageConfidence > 0.6 ? '#faad14' : '#ff4d4f'
              }
              showInfo={false}
              size="small"
            />
          </Card>
        </Col>
        <Col span={8}>
          <Card size="small">
            <Statistic
              title="Failed Steps"
              value={metrics.failedSteps}
              suffix={`/ ${metrics.totalSteps}`}
              valueStyle={{ color: metrics.failedSteps > 0 ? '#cf1322' : '#3f8600' }}
            />
            <Progress 
              percent={(metrics.failedSteps / metrics.totalSteps) * 100}
              strokeColor="#ff4d4f"
              showInfo={false}
              size="small"
            />
          </Card>
        </Col>
      </Row>

      {/* Alerts */}
      {metrics.successRate < 0.8 && (
        <Alert
          message="Low Success Rate"
          description="This trace has a lower than expected success rate. Review failed steps for optimization opportunities."
          type="warning"
          showIcon
        />
      )}
      
      {metrics.averageConfidence < 0.7 && (
        <Alert
          message="Low Average Confidence"
          description="The average confidence for this trace is below recommended levels. Consider reviewing the decision-making process."
          type="warning"
          showIcon
        />
      )}
    </Space>
  )

  const renderStepsTab = () => (
    <StepByStepAnalyzer
      steps={trace.steps || []}
      showContent={true}
      showMetrics={true}
      interactive={true}
    />
  )

  const renderFlowTab = () => (
    <QueryFlowAnalyzer
      trace={trace}
      showDetailedSteps={true}
      showPerformanceMetrics={true}
      showConfidenceBreakdown={true}
    />
  )

  const renderConfidenceTab = () => (
    <ConfidenceTracker
      steps={trace.steps || []}
      showTrend={true}
      showBreakdown={true}
      showRecommendations={true}
    />
  )

  const renderBusinessContextTab = () => (
    <Card title="Business Context" size="small">
      {trace.businessContext ? (
        <Descriptions column={1} size="small">
          <Descriptions.Item label="Domain">
            {trace.businessContext.domain || 'Not specified'}
          </Descriptions.Item>
          <Descriptions.Item label="Context">
            {trace.businessContext.context || 'No context available'}
          </Descriptions.Item>
          <Descriptions.Item label="Entities">
            <Space wrap>
              {trace.businessContext.entities?.map((entity, index) => (
                <Tag key={index} color="blue">{entity}</Tag>
              )) || <Text type="secondary">No entities identified</Text>}
            </Space>
          </Descriptions.Item>
        </Descriptions>
      ) : (
        <div style={{ textAlign: 'center', padding: '40px 0' }}>
          <Space direction="vertical">
            <DatabaseOutlined style={{ fontSize: '48px', color: '#d9d9d9' }} />
            <Text type="secondary">No business context available</Text>
          </Space>
        </div>
      )}
    </Card>
  )

  const renderTokenBudgetTab = () => (
    <Card title="Token Budget Analysis" size="small">
      {trace.tokenBudget ? (
        <Space direction="vertical" style={{ width: '100%' }}>
          <Row gutter={[16, 16]}>
            <Col span={8}>
              <Statistic
                title="Budget Limit"
                value={trace.tokenBudget.limit}
                prefix={<BarChartOutlined />}
              />
            </Col>
            <Col span={8}>
              <Statistic
                title="Used Tokens"
                value={trace.tokenBudget.used}
                prefix={<BarChartOutlined />}
              />
            </Col>
            <Col span={8}>
              <Statistic
                title="Remaining"
                value={trace.tokenBudget.limit - trace.tokenBudget.used}
                prefix={<BarChartOutlined />}
                valueStyle={{ 
                  color: (trace.tokenBudget.limit - trace.tokenBudget.used) > trace.tokenBudget.limit * 0.2 ? 
                    '#3f8600' : '#cf1322' 
                }}
              />
            </Col>
          </Row>
          
          <Progress
            percent={(trace.tokenBudget.used / trace.tokenBudget.limit) * 100}
            strokeColor={
              trace.tokenBudget.used / trace.tokenBudget.limit > 0.9 ? '#ff4d4f' :
              trace.tokenBudget.used / trace.tokenBudget.limit > 0.7 ? '#faad14' : '#52c41a'
            }
            format={(percent) => `${percent?.toFixed(1)}% used`}
          />
          
          {trace.tokenBudget.used / trace.tokenBudget.limit > 0.9 && (
            <Alert
              message="Token Budget Nearly Exhausted"
              description="This trace used over 90% of the available token budget. Consider optimizing token usage."
              type="warning"
              showIcon
            />
          )}
        </Space>
      ) : (
        <div style={{ textAlign: 'center', padding: '40px 0' }}>
          <Space direction="vertical">
            <BarChartOutlined style={{ fontSize: '48px', color: '#d9d9d9' }} />
            <Text type="secondary">No token budget information available</Text>
          </Space>
        </div>
      )}
    </Card>
  )

  if (!trace) {
    return (
      <Card className={className} data-testid={testId}>
        <div style={{ textAlign: 'center', padding: '40px 0' }}>
          <Space direction="vertical">
            <EyeOutlined style={{ fontSize: '48px', color: '#d9d9d9' }} />
            <Text type="secondary">No trace data available</Text>
          </Space>
        </div>
      </Card>
    )
  }

  return (
    <Card 
      title={
        <Space>
          <EyeOutlined />
          <span>Trace Details</span>
          <Tag color="blue">{trace.traceId.substring(0, 8)}...</Tag>
        </Space>
      }
      extra={
        <Space>
          <Button 
            size="small" 
            icon={<DownloadOutlined />}
            onClick={() => onExport?.(trace)}
          >
            Export
          </Button>
          <Button 
            size="small" 
            icon={<ShareAltOutlined />}
            onClick={() => onShare?.(trace)}
          >
            Share
          </Button>
        </Space>
      }
      className={className}
      data-testid={testId}
    >
      <Tabs activeKey={activeTab} onChange={handleTabChange}>
        <TabPane tab="Overview" key="overview">
          {renderOverviewTab()}
        </TabPane>
        
        {showAllTabs && (
          <>
            <TabPane tab="Steps Analysis" key="steps">
              {renderStepsTab()}
            </TabPane>
            
            <TabPane tab="Query Flow" key="flow">
              {renderFlowTab()}
            </TabPane>
            
            <TabPane tab="Confidence" key="confidence">
              {renderConfidenceTab()}
            </TabPane>
            
            <TabPane tab="Business Context" key="context">
              {renderBusinessContextTab()}
            </TabPane>
            
            <TabPane tab="Token Budget" key="budget">
              {renderTokenBudgetTab()}
            </TabPane>
          </>
        )}
      </Tabs>
    </Card>
  )
}

export default TraceDetailViewer
