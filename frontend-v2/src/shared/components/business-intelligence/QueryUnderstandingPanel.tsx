import React, { useState, useCallback, useMemo } from 'react'
import {
  Card,
  Empty,
  Spin,
  Typography,
  Space,
  Steps,
  Timeline,
  Tabs,
  Button,
  Tag,
  Progress,
  Alert,
  List,
  Avatar,
  Tooltip,
  Badge,
  Row,
  Col,
  Statistic,
  Divider,
  Collapse,
  Tree,
  Rate,
  message
} from 'antd'
import {
  BulbOutlined,
  CheckCircleOutlined,
  ClockCircleOutlined,
  ExclamationCircleOutlined,
  InfoCircleOutlined,
  ThunderboltOutlined,
  BranchesOutlined,
  SearchOutlined,
  EyeOutlined,
  EditOutlined,
  PlayCircleOutlined,
  PauseCircleOutlined,
  ReloadOutlined,
  SettingOutlined,
  QuestionCircleOutlined,
  RightOutlined,
  DownOutlined,
  NodeIndexOutlined,
  FileTextOutlined,
  StarOutlined
} from '@ant-design/icons'

const { Text, Title, Paragraph } = Typography
const { Step } = Steps
const { TabPane } = Tabs
const { Panel } = Collapse

interface ProcessingStep {
  id: string
  name: string
  description: string
  status: 'pending' | 'processing' | 'completed' | 'error'
  confidence: number
  duration: number
  details: string[]
  subSteps?: ProcessingSubStep[]
  alternatives?: Alternative[]
}

interface ProcessingSubStep {
  id: string
  name: string
  status: 'pending' | 'processing' | 'completed' | 'error'
  confidence: number
  details: string
}

interface Alternative {
  id: string
  interpretation: string
  confidence: number
  reasoning: string
  suggestedQuery: string
}

interface QueryBreakdown {
  originalQuery: string
  normalizedQuery: string
  tokens: QueryToken[]
  entities: DetectedEntity[]
  intent: QueryIntent
  complexity: QueryComplexity
}

interface QueryToken {
  text: string
  type: 'keyword' | 'entity' | 'operator' | 'value' | 'function'
  confidence: number
  position: number
  suggestions?: string[]
}

interface DetectedEntity {
  text: string
  type: string
  confidence: number
  businessMeaning: string
  suggestedMapping: string
}

interface QueryIntent {
  primary: string
  secondary: string[]
  confidence: number
  businessGoal: string
}

interface QueryComplexity {
  level: 'simple' | 'moderate' | 'complex' | 'very_complex'
  score: number
  factors: string[]
  recommendations: string[]
}

interface QueryUnderstandingPanelProps {
  query: string
  loading?: boolean
  showSteps?: boolean
  showAlternatives?: boolean
  showConfidenceBreakdown?: boolean
  interactive?: boolean
  onAlternativeSelect?: (alternative: Alternative) => void
  onStepRerun?: (stepId: string) => void
  onQueryRefine?: (refinedQuery: string) => void
}

/**
 * QueryUnderstandingPanel - Advanced query processing flow visualization
 *
 * Features:
 * - Step-by-step processing visualization with real-time updates
 * - Alternative interpretations with confidence scoring
 * - Query breakdown and token analysis
 * - Interactive refinement and reprocessing
 * - Confidence breakdown and detailed explanations
 * - Performance metrics and optimization suggestions
 */
export const QueryUnderstandingPanel: React.FC<QueryUnderstandingPanelProps> = ({
  query,
  loading = false,
  showSteps = true,
  showAlternatives = true,
  showConfidenceBreakdown = true,
  interactive = true,
  onAlternativeSelect,
  onStepRerun,
  onQueryRefine
}) => {
  const [activeTab, setActiveTab] = useState('flow')
  const [expandedSteps, setExpandedSteps] = useState<string[]>(['step-1'])
  const [selectedAlternative, setSelectedAlternative] = useState<string | null>(null)

  // Mock data for development - will be replaced with real API data
  const mockProcessingData = useMemo(() => ({
    processingSteps: [
      {
        id: 'step-1',
        name: 'Query Parsing & Tokenization',
        description: 'Breaking down the natural language query into analyzable components',
        status: 'completed' as const,
        confidence: 0.95,
        duration: 120,
        details: [
          'Tokenized query into 8 meaningful components',
          'Identified 3 business entities and 2 metrics',
          'Detected temporal context: "last quarter"',
          'Normalized query structure for processing'
        ],
        subSteps: [
          {
            id: 'substep-1-1',
            name: 'Lexical Analysis',
            status: 'completed' as const,
            confidence: 0.98,
            details: 'Successfully tokenized all query components'
          },
          {
            id: 'substep-1-2',
            name: 'Syntax Parsing',
            status: 'completed' as const,
            confidence: 0.92,
            details: 'Parsed query structure and relationships'
          }
        ]
      },
      {
        id: 'step-2',
        name: 'Entity Recognition & Mapping',
        description: 'Identifying and mapping business entities to data sources',
        status: 'completed' as const,
        confidence: 0.88,
        duration: 200,
        details: [
          'Identified "sales" entity with 95% confidence',
          'Mapped "region" to geographic dimension',
          'Recognized "revenue" as primary metric',
          'Detected time dimension: quarterly aggregation'
        ],
        subSteps: [
          {
            id: 'substep-2-1',
            name: 'Named Entity Recognition',
            status: 'completed' as const,
            confidence: 0.91,
            details: 'Extracted business entities from query text'
          },
          {
            id: 'substep-2-2',
            name: 'Entity-to-Schema Mapping',
            status: 'completed' as const,
            confidence: 0.85,
            details: 'Mapped entities to database schema elements'
          }
        ]
      },
      {
        id: 'step-3',
        name: 'Intent Classification',
        description: 'Determining the business intent and query complexity',
        status: 'completed' as const,
        confidence: 0.92,
        duration: 150,
        details: [
          'Classified as aggregation query (95% confidence)',
          'Detected comparison intent (secondary)',
          'Identified reporting use case',
          'Complexity level: moderate'
        ],
        subSteps: [
          {
            id: 'substep-3-1',
            name: 'Primary Intent Detection',
            status: 'completed' as const,
            confidence: 0.95,
            details: 'Identified aggregation as primary intent'
          },
          {
            id: 'substep-3-2',
            name: 'Secondary Intent Analysis',
            status: 'completed' as const,
            confidence: 0.89,
            details: 'Detected comparison and trend analysis intents'
          }
        ]
      },
      {
        id: 'step-4',
        name: 'Context & Domain Analysis',
        description: 'Analyzing business context and domain-specific requirements',
        status: 'completed' as const,
        confidence: 0.87,
        duration: 180,
        details: [
          'Identified sales domain context',
          'Applied regional business rules',
          'Considered quarterly reporting standards',
          'Integrated user role permissions'
        ],
        subSteps: [
          {
            id: 'substep-4-1',
            name: 'Domain Classification',
            status: 'completed' as const,
            confidence: 0.90,
            details: 'Classified query within sales analytics domain'
          },
          {
            id: 'substep-4-2',
            name: 'Business Rules Application',
            status: 'completed' as const,
            confidence: 0.84,
            details: 'Applied domain-specific business rules and constraints'
          }
        ]
      }
    ],
    alternatives: [
      {
        id: 'alt-1',
        interpretation: 'Regional sales performance comparison for Q4 2023',
        confidence: 0.92,
        reasoning: 'High confidence based on temporal indicators and comparison keywords',
        suggestedQuery: 'Show me sales performance by region for Q4 2023 compared to Q3 2023'
      },
      {
        id: 'alt-2',
        interpretation: 'Year-over-year regional sales analysis',
        confidence: 0.78,
        reasoning: 'Alternative interpretation focusing on annual comparison',
        suggestedQuery: 'Compare regional sales performance Q4 2023 vs Q4 2022'
      },
      {
        id: 'alt-3',
        interpretation: 'Regional sales trend analysis with forecasting',
        confidence: 0.65,
        reasoning: 'Lower confidence interpretation including predictive elements',
        suggestedQuery: 'Analyze regional sales trends and forecast for next quarter'
      }
    ],
    queryBreakdown: {
      originalQuery: query,
      normalizedQuery: 'SELECT region, SUM(sales) FROM sales_data WHERE date >= Q4_2023 GROUP BY region',
      tokens: [
        {
          text: 'sales',
          type: 'entity' as const,
          confidence: 0.95,
          position: 0,
          suggestions: ['revenue', 'transactions', 'orders']
        },
        {
          text: 'by',
          type: 'operator' as const,
          confidence: 0.98,
          position: 1
        },
        {
          text: 'region',
          type: 'entity' as const,
          confidence: 0.92,
          position: 2,
          suggestions: ['geography', 'territory', 'area']
        },
        {
          text: 'last quarter',
          type: 'value' as const,
          confidence: 0.88,
          position: 3,
          suggestions: ['Q4 2023', 'previous quarter', 'last 3 months']
        }
      ],
      entities: [
        {
          text: 'sales',
          type: 'metric',
          confidence: 0.95,
          businessMeaning: 'Revenue or transaction volume',
          suggestedMapping: 'sales_fact.amount'
        },
        {
          text: 'region',
          type: 'dimension',
          confidence: 0.92,
          businessMeaning: 'Geographic grouping for analysis',
          suggestedMapping: 'region_dim.region_name'
        }
      ],
      intent: {
        primary: 'aggregation',
        secondary: ['comparison', 'grouping'],
        confidence: 0.92,
        businessGoal: 'Analyze sales performance across different regions'
      },
      complexity: {
        level: 'moderate' as const,
        score: 0.6,
        factors: ['Multiple entities', 'Temporal context', 'Grouping operation'],
        recommendations: ['Consider adding filters', 'Specify time range clearly', 'Add sorting preferences']
      }
    }
  }), [query])

  const handleAlternativeSelect = useCallback((alternative: Alternative) => {
    setSelectedAlternative(alternative.id)
    if (onAlternativeSelect) {
      onAlternativeSelect(alternative)
    }
    message.success(`Selected alternative interpretation: ${alternative.interpretation}`)
  }, [onAlternativeSelect])

  const handleStepRerun = useCallback((stepId: string) => {
    if (onStepRerun) {
      onStepRerun(stepId)
    }
    message.info(`Rerunning step: ${stepId}`)
  }, [onStepRerun])

  const handleQueryRefine = useCallback((refinedQuery: string) => {
    if (onQueryRefine) {
      onQueryRefine(refinedQuery)
    }
    message.success('Query refinement applied')
  }, [onQueryRefine])

  // Utility functions
  const getStepIcon = (status: string) => {
    const icons: Record<string, React.ReactNode> = {
      'pending': <ClockCircleOutlined style={{ color: '#d9d9d9' }} />,
      'processing': <PlayCircleOutlined style={{ color: '#1890ff' }} />,
      'completed': <CheckCircleOutlined style={{ color: '#52c41a' }} />,
      'error': <ExclamationCircleOutlined style={{ color: '#ff4d4f' }} />
    }
    return icons[status] || <QuestionCircleOutlined />
  }

  const getStepStatus = (status: string) => {
    const statusMap: Record<string, 'wait' | 'process' | 'finish' | 'error'> = {
      'pending': 'wait',
      'processing': 'process',
      'completed': 'finish',
      'error': 'error'
    }
    return statusMap[status] || 'wait'
  }

  const getConfidenceColor = (confidence: number) => {
    if (confidence >= 0.9) return '#52c41a'
    if (confidence >= 0.8) return '#1890ff'
    if (confidence >= 0.7) return '#fa8c16'
    return '#ff4d4f'
  }

  const getComplexityColor = (level: string) => {
    const colors: Record<string, string> = {
      'simple': '#52c41a',
      'moderate': '#1890ff',
      'complex': '#fa8c16',
      'very_complex': '#ff4d4f'
    }
    return colors[level] || '#d9d9d9'
  }

  const getTokenTypeColor = (type: string) => {
    const colors: Record<string, string> = {
      'keyword': '#722ed1',
      'entity': '#52c41a',
      'operator': '#1890ff',
      'value': '#fa8c16',
      'function': '#13c2c2'
    }
    return colors[type] || '#d9d9d9'
  }

  if (loading) {
    return (
      <Card>
        <div style={{ textAlign: 'center', padding: '40px 0' }}>
          <Spin size="large" />
          <div style={{ marginTop: 16 }}>
            <Text>Understanding query flow...</Text>
            <br />
            <Text type="secondary" style={{ fontSize: '12px' }}>
              Processing natural language, extracting entities, and analyzing intent...
            </Text>
          </div>
        </div>
      </Card>
    )
  }

  if (!query) {
    return (
      <Card>
        <Empty
          image={Empty.PRESENTED_IMAGE_SIMPLE}
          description={
            <div>
              <Text>Enter a query to see understanding flow</Text>
              <br />
              <Text type="secondary" style={{ fontSize: '12px' }}>
                AI will break down your query step-by-step and show alternative interpretations
              </Text>
            </div>
          }
        />
      </Card>
    )
  }

  const overallConfidence = mockProcessingData.processingSteps.reduce((sum, step) => sum + step.confidence, 0) / mockProcessingData.processingSteps.length
  const totalDuration = mockProcessingData.processingSteps.reduce((sum, step) => sum + step.duration, 0)

  return (
    <Space direction="vertical" style={{ width: '100%' }} size="large">
      {/* Overview Header */}
      <Card
        title={
          <Space>
            <BulbOutlined />
            <span>Query Understanding Flow</span>
            <Badge
              count={`${Math.round(overallConfidence * 100)}%`}
              style={{ backgroundColor: getConfidenceColor(overallConfidence) }}
            />
          </Space>
        }
        extra={
          interactive && (
            <Space>
              <Tooltip title="Reprocess query">
                <Button icon={<ReloadOutlined />} size="small" />
              </Tooltip>
              <Tooltip title="Export analysis">
                <Button icon={<FileTextOutlined />} size="small" />
              </Tooltip>
            </Space>
          )
        }
      >
        <Row gutter={[16, 16]}>
          <Col xs={24} sm={12} md={6}>
            <Statistic
              title="Overall Confidence"
              value={overallConfidence}
              precision={2}
              suffix="/ 1.0"
              valueStyle={{ color: getConfidenceColor(overallConfidence) }}
              prefix={<StarOutlined />}
            />
          </Col>
          <Col xs={24} sm={12} md={6}>
            <Statistic
              title="Processing Time"
              value={totalDuration}
              suffix="ms"
              valueStyle={{ color: '#1890ff' }}
              prefix={<ClockCircleOutlined />}
            />
          </Col>
          <Col xs={24} sm={12} md={6}>
            <Statistic
              title="Processing Steps"
              value={mockProcessingData.processingSteps.length}
              valueStyle={{ color: '#52c41a' }}
              prefix={<NodeIndexOutlined />}
            />
          </Col>
          <Col xs={24} sm={12} md={6}>
            <Statistic
              title="Alternatives Found"
              value={mockProcessingData.alternatives.length}
              valueStyle={{ color: '#fa8c16' }}
              prefix={<BranchesOutlined />}
            />
          </Col>
        </Row>

        <Alert
          message="Query Processing Complete"
          description={`Your query "${query}" has been successfully analyzed and understood with ${Math.round(overallConfidence * 100)}% confidence.`}
          type="success"
          showIcon
          style={{ marginTop: 16 }}
        />
      </Card>

      {/* Main Content Tabs */}
      <Card>
        <Tabs
          activeKey={activeTab}
          onChange={setActiveTab}
          size="large"
          type="card"
        >
          {/* Processing Flow Tab */}
          {showSteps && (
            <TabPane
              tab={
                <Space>
                  <PlayCircleOutlined />
                  <span>Processing Flow</span>
                  <Badge count={mockProcessingData.processingSteps.length} size="small" />
                </Space>
              }
              key="flow"
            >
              <Space direction="vertical" style={{ width: '100%' }} size="large">
                {/* Step-by-Step Visualization */}
                <Steps
                  direction="vertical"
                  current={mockProcessingData.processingSteps.length}
                  items={mockProcessingData.processingSteps.map((step, index) => ({
                    title: step.name,
                    description: step.description,
                    status: getStepStatus(step.status),
                    icon: getStepIcon(step.status)
                  }))}
                />

                {/* Detailed Step Information */}
                <Collapse
                  activeKey={expandedSteps}
                  onChange={(keys) => setExpandedSteps(Array.isArray(keys) ? keys : [keys])}
                  ghost
                >
                  {mockProcessingData.processingSteps.map((step) => (
                    <Panel
                      key={step.id}
                      header={
                        <Space>
                          {getStepIcon(step.status)}
                          <span style={{ fontWeight: 600 }}>{step.name}</span>
                          <Tag color={getConfidenceColor(step.confidence)}>
                            {Math.round(step.confidence * 100)}% confidence
                          </Tag>
                          <Tag color="blue">{step.duration}ms</Tag>
                        </Space>
                      }
                      extra={
                        interactive && (
                          <Button
                            type="link"
                            size="small"
                            icon={<ReloadOutlined />}
                            onClick={(e) => {
                              e.stopPropagation()
                              handleStepRerun(step.id)
                            }}
                          >
                            Rerun
                          </Button>
                        )
                      }
                    >
                      <Space direction="vertical" style={{ width: '100%' }}>
                        <div>
                          <Text strong>Processing Details:</Text>
                          <List
                            size="small"
                            dataSource={step.details}
                            renderItem={(detail) => (
                              <List.Item>
                                <CheckCircleOutlined style={{ color: '#52c41a', marginRight: 8 }} />
                                <Text>{detail}</Text>
                              </List.Item>
                            )}
                          />
                        </div>

                        {step.subSteps && step.subSteps.length > 0 && (
                          <div>
                            <Text strong>Sub-steps:</Text>
                            <Timeline style={{ marginTop: 8 }}>
                              {step.subSteps.map((subStep) => (
                                <Timeline.Item
                                  key={subStep.id}
                                  color={getConfidenceColor(subStep.confidence)}
                                  dot={getStepIcon(subStep.status)}
                                >
                                  <Space>
                                    <Text strong>{subStep.name}</Text>
                                    <Tag color={getConfidenceColor(subStep.confidence)}>
                                      {Math.round(subStep.confidence * 100)}%
                                    </Tag>
                                  </Space>
                                  <br />
                                  <Text type="secondary">{subStep.details}</Text>
                                </Timeline.Item>
                              ))}
                            </Timeline>
                          </div>
                        )}

                        <Progress
                          percent={Math.round(step.confidence * 100)}
                          strokeColor={getConfidenceColor(step.confidence)}
                          format={(percent) => `${percent}% confidence`}
                        />
                      </Space>
                    </Panel>
                  ))}
                </Collapse>
              </Space>
            </TabPane>
          )}

          {/* Alternative Interpretations Tab */}
          {showAlternatives && (
            <TabPane
              tab={
                <Space>
                  <BranchesOutlined />
                  <span>Alternatives</span>
                  <Badge count={mockProcessingData.alternatives.length} size="small" />
                </Space>
              }
              key="alternatives"
            >
              <Space direction="vertical" style={{ width: '100%' }} size="middle">
                <Alert
                  message="Alternative Query Interpretations"
                  description="AI has identified multiple possible interpretations of your query. Select the one that best matches your intent."
                  type="info"
                  showIcon
                />

                {mockProcessingData.alternatives.map((alternative) => (
                  <Card
                    key={alternative.id}
                    size="small"
                    hoverable={interactive}
                    onClick={() => interactive && handleAlternativeSelect(alternative)}
                    style={{
                      cursor: interactive ? 'pointer' : 'default',
                      border: selectedAlternative === alternative.id ? '2px solid #1890ff' : undefined
                    }}
                    title={
                      <Space>
                        <BulbOutlined style={{ color: getConfidenceColor(alternative.confidence) }} />
                        <span>Alternative Interpretation</span>
                        <Tag color={getConfidenceColor(alternative.confidence)}>
                          {Math.round(alternative.confidence * 100)}% confidence
                        </Tag>
                        {selectedAlternative === alternative.id && (
                          <Tag color="blue">SELECTED</Tag>
                        )}
                      </Space>
                    }
                    extra={
                      interactive && (
                        <Button
                          type="primary"
                          size="small"
                          icon={<CheckCircleOutlined />}
                          onClick={(e) => {
                            e.stopPropagation()
                            handleAlternativeSelect(alternative)
                          }}
                        >
                          Select
                        </Button>
                      )
                    }
                  >
                    <Space direction="vertical" style={{ width: '100%' }}>
                      <div>
                        <Text strong style={{ fontSize: '16px' }}>
                          {alternative.interpretation}
                        </Text>
                      </div>

                      <div>
                        <Text strong>Reasoning: </Text>
                        <Text>{alternative.reasoning}</Text>
                      </div>

                      <div>
                        <Text strong>Suggested Query: </Text>
                        <Text code style={{ fontSize: '12px' }}>
                          {alternative.suggestedQuery}
                        </Text>
                        {interactive && (
                          <Button
                            type="link"
                            size="small"
                            icon={<EditOutlined />}
                            onClick={(e) => {
                              e.stopPropagation()
                              handleQueryRefine(alternative.suggestedQuery)
                            }}
                          >
                            Apply
                          </Button>
                        )}
                      </div>

                      <Progress
                        percent={Math.round(alternative.confidence * 100)}
                        strokeColor={getConfidenceColor(alternative.confidence)}
                        showInfo={false}
                      />
                    </Space>
                  </Card>
                ))}
              </Space>
            </TabPane>
          )}

          {/* Query Breakdown Tab */}
          {showConfidenceBreakdown && (
            <TabPane
              tab={
                <Space>
                  <SearchOutlined />
                  <span>Query Breakdown</span>
                </Space>
              }
              key="breakdown"
            >
              <Space direction="vertical" style={{ width: '100%' }} size="large">
                {/* Query Transformation */}
                <Card size="small" title="Query Transformation">
                  <Space direction="vertical" style={{ width: '100%' }}>
                    <div>
                      <Text strong>Original Query:</Text>
                      <div style={{ marginTop: 4, padding: 8, backgroundColor: '#f6f6f6', borderRadius: 4 }}>
                        <Text code>{mockProcessingData.queryBreakdown.originalQuery}</Text>
                      </div>
                    </div>

                    <div>
                      <Text strong>Normalized Query:</Text>
                      <div style={{ marginTop: 4, padding: 8, backgroundColor: '#f6ffed', borderRadius: 4 }}>
                        <Text code>{mockProcessingData.queryBreakdown.normalizedQuery}</Text>
                      </div>
                    </div>
                  </Space>
                </Card>

                {/* Token Analysis */}
                <Card size="small" title="Token Analysis">
                  <Space wrap>
                    {mockProcessingData.queryBreakdown.tokens.map((token, index) => (
                      <Tooltip
                        key={index}
                        title={
                          <div>
                            <div><strong>Type:</strong> {token.type}</div>
                            <div><strong>Confidence:</strong> {Math.round(token.confidence * 100)}%</div>
                            {token.suggestions && (
                              <div><strong>Suggestions:</strong> {token.suggestions.join(', ')}</div>
                            )}
                          </div>
                        }
                      >
                        <Tag
                          color={getTokenTypeColor(token.type)}
                          style={{
                            fontSize: '13px',
                            padding: '4px 8px',
                            margin: '2px'
                          }}
                        >
                          {token.text}
                          <Badge
                            count={Math.round(token.confidence * 100)}
                            size="small"
                            style={{
                              backgroundColor: getConfidenceColor(token.confidence),
                              marginLeft: 4
                            }}
                          />
                        </Tag>
                      </Tooltip>
                    ))}
                  </Space>
                </Card>

                {/* Entity Analysis */}
                <Card size="small" title="Detected Entities">
                  <List
                    dataSource={mockProcessingData.queryBreakdown.entities}
                    renderItem={(entity) => (
                      <List.Item>
                        <List.Item.Meta
                          avatar={
                            <Avatar
                              style={{
                                backgroundColor: getConfidenceColor(entity.confidence)
                              }}
                            >
                              {entity.text.charAt(0).toUpperCase()}
                            </Avatar>
                          }
                          title={
                            <Space>
                              <Text strong>{entity.text}</Text>
                              <Tag color="blue">{entity.type}</Tag>
                              <Progress
                                percent={Math.round(entity.confidence * 100)}
                                size="small"
                                style={{ width: 100 }}
                                showInfo={false}
                              />
                            </Space>
                          }
                          description={
                            <div>
                              <Text type="secondary">{entity.businessMeaning}</Text>
                              <br />
                              <Text strong style={{ fontSize: '11px' }}>Mapping: </Text>
                              <Text code style={{ fontSize: '11px' }}>{entity.suggestedMapping}</Text>
                            </div>
                          }
                        />
                      </List.Item>
                    )}
                  />
                </Card>

                {/* Intent & Complexity Analysis */}
                <Row gutter={[16, 16]}>
                  <Col xs={24} md={12}>
                    <Card size="small" title="Intent Analysis">
                      <Space direction="vertical" style={{ width: '100%' }}>
                        <div>
                          <Text strong>Primary Intent:</Text>
                          <div style={{ marginTop: 4 }}>
                            <Tag color="green" style={{ fontSize: '13px' }}>
                              {mockProcessingData.queryBreakdown.intent.primary}
                            </Tag>
                            <Progress
                              percent={Math.round(mockProcessingData.queryBreakdown.intent.confidence * 100)}
                              size="small"
                              style={{ width: 150, marginLeft: 8 }}
                            />
                          </div>
                        </div>

                        <div>
                          <Text strong>Secondary Intents:</Text>
                          <div style={{ marginTop: 4 }}>
                            <Space wrap>
                              {mockProcessingData.queryBreakdown.intent.secondary.map((intent, idx) => (
                                <Tag key={idx} color="blue">{intent}</Tag>
                              ))}
                            </Space>
                          </div>
                        </div>

                        <div>
                          <Text strong>Business Goal:</Text>
                          <Paragraph style={{ marginTop: 4, marginBottom: 0 }}>
                            {mockProcessingData.queryBreakdown.intent.businessGoal}
                          </Paragraph>
                        </div>
                      </Space>
                    </Card>
                  </Col>

                  <Col xs={24} md={12}>
                    <Card size="small" title="Complexity Analysis">
                      <Space direction="vertical" style={{ width: '100%' }}>
                        <div>
                          <Text strong>Complexity Level:</Text>
                          <div style={{ marginTop: 4 }}>
                            <Tag
                              color={getComplexityColor(mockProcessingData.queryBreakdown.complexity.level)}
                              style={{ fontSize: '13px' }}
                            >
                              {mockProcessingData.queryBreakdown.complexity.level.toUpperCase()}
                            </Tag>
                            <Progress
                              percent={Math.round(mockProcessingData.queryBreakdown.complexity.score * 100)}
                              size="small"
                              style={{ width: 150, marginLeft: 8 }}
                              strokeColor={getComplexityColor(mockProcessingData.queryBreakdown.complexity.level)}
                            />
                          </div>
                        </div>

                        <div>
                          <Text strong>Complexity Factors:</Text>
                          <List
                            size="small"
                            dataSource={mockProcessingData.queryBreakdown.complexity.factors}
                            renderItem={(factor) => (
                              <List.Item style={{ padding: '4px 0' }}>
                                <InfoCircleOutlined style={{ color: '#1890ff', marginRight: 8 }} />
                                <Text style={{ fontSize: '12px' }}>{factor}</Text>
                              </List.Item>
                            )}
                          />
                        </div>

                        <div>
                          <Text strong>Recommendations:</Text>
                          <List
                            size="small"
                            dataSource={mockProcessingData.queryBreakdown.complexity.recommendations}
                            renderItem={(rec) => (
                              <List.Item style={{ padding: '4px 0' }}>
                                <BulbOutlined style={{ color: '#fa8c16', marginRight: 8 }} />
                                <Text style={{ fontSize: '12px' }}>{rec}</Text>
                              </List.Item>
                            )}
                          />
                        </div>
                      </Space>
                    </Card>
                  </Col>
                </Row>
              </Space>
            </TabPane>
          )}
        </Tabs>
      </Card>
    </Space>
  )
}

export default QueryUnderstandingPanel
