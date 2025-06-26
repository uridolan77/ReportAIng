import React, { useState, useMemo } from 'react'
import {
  Card,
  Tabs,
  Typography,
  Space,
  Progress,
  Tag,
  Alert,
  Spin,
  Empty,
  Timeline,
  Tooltip,
  Button,
  Row,
  Col,
  Statistic,
  Badge,
  Collapse
} from 'antd'
import {
  EyeOutlined,
  BulbOutlined,
  ClockCircleOutlined,
  CheckCircleOutlined,
  InfoCircleOutlined,
  ThunderboltOutlined,
  BarChartOutlined,
  RobotOutlined,
  SettingOutlined
} from '@ant-design/icons'
import { ConfidenceIndicator } from '../common/ConfidenceIndicator'
import { useGetTransparencyTraceQuery } from '@shared/store/api/transparencyApi'

const { Title, Text, Paragraph } = Typography

export interface AITransparencyPanelProps {
  query?: string
  traceId?: string
  showPromptConstruction?: boolean
  showConfidenceBreakdown?: boolean
  showDecisionExplanation?: boolean
  showPerformanceMetrics?: boolean
  interactive?: boolean
  compact?: boolean
  onOptimizationSuggestion?: (suggestion: any) => void
  onAlternativeSelect?: (alternative: any) => void
  className?: string
  testId?: string
}

/**
 * AITransparencyPanel - Comprehensive AI transparency and explainability interface
 * 
 * Features:
 * - Prompt construction visualization
 * - Confidence breakdown analysis
 * - Decision explanation and rationale
 * - Performance metrics and optimization suggestions
 * - Interactive exploration of AI reasoning
 */
export const AITransparencyPanel: React.FC<AITransparencyPanelProps> = ({
  query = '',
  traceId,
  showPromptConstruction = true,
  showConfidenceBreakdown = true,
  showDecisionExplanation = true,
  showPerformanceMetrics = false,
  interactive = true,
  compact = false,
  onOptimizationSuggestion,
  onAlternativeSelect,
  className,
  testId = 'ai-transparency-panel'
}) => {
  const [activeTab, setActiveTab] = useState('overview')
  
  // Mock data for now - replace with real API call when available
  const mockTransparencyData = useMemo(() => ({
    confidence: 0.87,
    promptConstruction: {
      steps: [
        { step: 'Intent Analysis', confidence: 0.92, description: 'Identified analytical query intent' },
        { step: 'Entity Extraction', confidence: 0.85, description: 'Found 3 business entities' },
        { step: 'Context Selection', confidence: 0.89, description: 'Selected relevant business context' },
        { step: 'SQL Generation', confidence: 0.84, description: 'Generated optimized SQL query' }
      ]
    },
    decisionExplanation: {
      reasoning: [
        'Query contains aggregation patterns suggesting analytical intent',
        'Business entities match sales domain schema',
        'Time-based filtering indicates trend analysis requirement',
        'Generated query optimized for performance with proper indexing'
      ],
      alternatives: [
        { option: 'Alternative SQL approach', confidence: 0.78, reason: 'Different join strategy' },
        { option: 'NoSQL aggregation', confidence: 0.65, reason: 'Document-based approach' }
      ]
    },
    performanceMetrics: {
      processingTime: 245,
      tokenUsage: 1250,
      cacheHitRate: 0.73,
      optimizationScore: 0.91
    }
  }), [])

  const renderOverview = () => (
    <Space direction="vertical" style={{ width: '100%' }} size="large">
      <Row gutter={[16, 16]}>
        <Col xs={24} sm={12} md={6}>
          <Card size="small">
            <Statistic
              title="Overall Confidence"
              value={Math.round(mockTransparencyData.confidence * 100)}
              suffix="%"
              valueStyle={{ color: mockTransparencyData.confidence > 0.8 ? '#52c41a' : '#faad14' }}
            />
          </Card>
        </Col>
        <Col xs={24} sm={12} md={6}>
          <Card size="small">
            <Statistic
              title="Processing Time"
              value={mockTransparencyData.performanceMetrics.processingTime}
              suffix="ms"
              prefix={<ClockCircleOutlined />}
            />
          </Card>
        </Col>
        <Col xs={24} sm={12} md={6}>
          <Card size="small">
            <Statistic
              title="Token Usage"
              value={mockTransparencyData.performanceMetrics.tokenUsage}
              prefix={<RobotOutlined />}
            />
          </Card>
        </Col>
        <Col xs={24} sm={12} md={6}>
          <Card size="small">
            <Statistic
              title="Cache Hit Rate"
              value={Math.round(mockTransparencyData.performanceMetrics.cacheHitRate * 100)}
              suffix="%"
              prefix={<ThunderboltOutlined />}
            />
          </Card>
        </Col>
      </Row>

      <Alert
        message="AI Transparency Active"
        description="This panel shows how the AI system processed your query and made decisions."
        type="info"
        showIcon
        icon={<EyeOutlined />}
      />
    </Space>
  )

  const renderPromptConstruction = () => (
    <Timeline
      items={mockTransparencyData.promptConstruction.steps.map((step, index) => ({
        key: index,
        dot: <CheckCircleOutlined style={{ color: '#52c41a' }} />,
        children: (
          <Space direction="vertical" size="small">
            <Space>
              <Text strong>{step.step}</Text>
              <ConfidenceIndicator confidence={step.confidence} size="small" />
            </Space>
            <Text type="secondary" style={{ fontSize: '12px' }}>
              {step.description}
            </Text>
          </Space>
        )
      }))}
    />
  )

  const renderDecisionExplanation = () => (
    <Space direction="vertical" style={{ width: '100%' }} size="middle">
      <Card size="small" title="AI Reasoning">
        <ul style={{ paddingLeft: '20px', margin: 0 }}>
          {mockTransparencyData.decisionExplanation.reasoning.map((reason, index) => (
            <li key={index} style={{ marginBottom: '8px' }}>
              <Text>{reason}</Text>
            </li>
          ))}
        </ul>
      </Card>

      {mockTransparencyData.decisionExplanation.alternatives.length > 0 && (
        <Card size="small" title="Alternative Approaches">
          <Space direction="vertical" style={{ width: '100%' }}>
            {mockTransparencyData.decisionExplanation.alternatives.map((alt, index) => (
              <Card key={index} size="small" style={{ backgroundColor: '#fafafa' }}>
                <Space direction="vertical" size="small" style={{ width: '100%' }}>
                  <Space>
                    <Text strong>{alt.option}</Text>
                    <ConfidenceIndicator confidence={alt.confidence} size="small" />
                    {interactive && (
                      <Button 
                        size="small" 
                        type="link"
                        onClick={() => onAlternativeSelect?.(alt)}
                      >
                        Try This
                      </Button>
                    )}
                  </Space>
                  <Text type="secondary" style={{ fontSize: '12px' }}>
                    {alt.reason}
                  </Text>
                </Space>
              </Card>
            ))}
          </Space>
        </Card>
      )}
    </Space>
  )

  const renderPerformanceMetrics = () => (
    <Row gutter={[16, 16]}>
      <Col span={24}>
        <Card size="small" title="Performance Analysis">
          <Space direction="vertical" style={{ width: '100%' }}>
            <div>
              <Text strong>Optimization Score: </Text>
              <Progress 
                percent={Math.round(mockTransparencyData.performanceMetrics.optimizationScore * 100)}
                size="small"
                status={mockTransparencyData.performanceMetrics.optimizationScore > 0.8 ? 'success' : 'normal'}
              />
            </div>
            <div>
              <Text strong>Cache Efficiency: </Text>
              <Progress 
                percent={Math.round(mockTransparencyData.performanceMetrics.cacheHitRate * 100)}
                size="small"
                strokeColor="#1890ff"
              />
            </div>
          </Space>
        </Card>
      </Col>
    </Row>
  )

  const tabItems = [
    {
      key: 'overview',
      label: (
        <Space>
          <BarChartOutlined />
          <span>Overview</span>
        </Space>
      ),
      children: renderOverview()
    },
    ...(showPromptConstruction ? [{
      key: 'construction',
      label: (
        <Space>
          <BulbOutlined />
          <span>Prompt Construction</span>
        </Space>
      ),
      children: renderPromptConstruction()
    }] : []),
    ...(showDecisionExplanation ? [{
      key: 'explanation',
      label: (
        <Space>
          <InfoCircleOutlined />
          <span>Decision Explanation</span>
        </Space>
      ),
      children: renderDecisionExplanation()
    }] : []),
    ...(showPerformanceMetrics ? [{
      key: 'performance',
      label: (
        <Space>
          <ThunderboltOutlined />
          <span>Performance</span>
        </Space>
      ),
      children: renderPerformanceMetrics()
    }] : [])
  ]

  if (compact) {
    return (
      <Card 
        size="small" 
        className={className}
        data-testid={testId}
        title={
          <Space>
            <EyeOutlined />
            <span>AI Transparency</span>
            <ConfidenceIndicator confidence={mockTransparencyData.confidence} size="small" />
          </Space>
        }
      >
        <Collapse
          ghost
          size="small"
          items={[{
            key: 'details',
            label: 'View AI Decision Process',
            children: (
              <Tabs
                activeKey={activeTab}
                onChange={setActiveTab}
                size="small"
                items={tabItems}
              />
            )
          }]}
        />
      </Card>
    )
  }

  return (
    <Card 
      className={className}
      data-testid={testId}
      title={
        <Space>
          <EyeOutlined />
          <span>AI Transparency & Explainability</span>
          <Badge 
            count={`${Math.round(mockTransparencyData.confidence * 100)}%`}
            style={{ backgroundColor: mockTransparencyData.confidence > 0.8 ? '#52c41a' : '#faad14' }}
          />
        </Space>
      }
      extra={
        interactive && (
          <Space>
            <Tooltip title="Transparency Settings">
              <Button icon={<SettingOutlined />} size="small" />
            </Tooltip>
          </Space>
        )
      }
    >
      <Tabs
        activeKey={activeTab}
        onChange={setActiveTab}
        items={tabItems}
      />
    </Card>
  )
}

export default AITransparencyPanel
