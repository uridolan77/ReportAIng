import React, { useState, useCallback, useMemo } from 'react'
import {
  Card,
  Empty,
  Spin,
  Typography,
  Space,
  Tabs,
  Table,
  Tag,
  Button,
  Tooltip,
  Alert,
  Row,
  Col,
  Statistic,
  Progress,
  List,
  Avatar,
  Badge,
  Collapse,
  Tree,
  Divider,
  Timeline,
  message
} from 'antd'
import {
  DatabaseOutlined,
  TableOutlined,
  LinkOutlined,
  ThunderboltOutlined,
  WarningOutlined,
  InfoCircleOutlined,
  CheckCircleOutlined,
  ClockCircleOutlined,
  BarChartOutlined,
  EyeOutlined,
  SettingOutlined,
  BulbOutlined,
  SearchOutlined,
  FileTextOutlined,
  NodeIndexOutlined
} from '@ant-design/icons'

const { Text, Title, Paragraph } = Typography
const { TabPane } = Tabs
const { Panel } = Collapse

interface TableInfo {
  name: string
  relevanceScore: number
  description: string
  businessPurpose: string
  columns: ColumnInfo[]
  relationships: TableRelationship[]
  estimatedRowCount: number
  lastUpdated: string
  dataQuality: DataQuality
}

interface ColumnInfo {
  name: string
  type: string
  description: string
  businessMeaning: string
  nullable: boolean
  isPrimaryKey: boolean
  isForeignKey: boolean
  relevanceScore: number
}

interface TableRelationship {
  relatedTable: string
  relationshipType: 'one-to-one' | 'one-to-many' | 'many-to-one' | 'many-to-many'
  strength: number
  description: string
  joinCondition: string
}

interface JoinSuggestion {
  tables: string[]
  joinType: string
  condition: string
  confidence: number
  reasoning: string
}

interface OptimizationTip {
  type: 'indexing' | 'partitioning' | 'caching' | 'query_rewrite'
  suggestion: string
  impact: 'low' | 'medium' | 'high'
  estimatedImprovement: string
}

interface DataIssue {
  type: 'data_quality' | 'performance' | 'access' | 'completeness'
  description: string
  severity: 'low' | 'medium' | 'high'
  recommendation: string
}

interface DataQuality {
  completeness: number
  accuracy: number
  consistency: number
}

interface SchemaIntelligencePanelProps {
  query: string
  loading?: boolean
  showBusinessTerms?: boolean
  showRelationships?: boolean
  showOptimizations?: boolean
  interactive?: boolean
  onTableSelect?: (tableName: string) => void
  onJoinSuggestionApply?: (suggestion: JoinSuggestion) => void
  onOptimizationApply?: (tip: OptimizationTip) => void
}

/**
 * SchemaIntelligencePanel - Advanced schema analysis and optimization
 *
 * Features:
 * - Relevant table identification with confidence scoring
 * - Column analysis with business meaning mapping
 * - Table relationship visualization
 * - Join suggestions with reasoning
 * - Performance optimization recommendations
 * - Data quality assessment
 * - Interactive schema exploration
 */
export const SchemaIntelligencePanel: React.FC<SchemaIntelligencePanelProps> = ({
  query,
  loading = false,
  showBusinessTerms = true,
  showRelationships = true,
  showOptimizations = true,
  interactive = true,
  onTableSelect,
  onJoinSuggestionApply,
  onOptimizationApply
}) => {
  const [activeTab, setActiveTab] = useState('tables')
  const [selectedTable, setSelectedTable] = useState<string | null>(null)
  const [expandedSections, setExpandedSections] = useState<string[]>(['overview'])

  // Mock data for development - will be replaced with real API data
  const mockSchemaData = useMemo(() => ({
    relevantTables: [
      {
        name: 'sales_fact',
        relevanceScore: 0.95,
        description: 'Main sales transaction table with detailed sales records',
        businessPurpose: 'Core sales data for analytics and reporting',
        columns: [
          {
            name: 'sale_id',
            type: 'string',
            description: 'Unique sale identifier',
            businessMeaning: 'Primary key for sales transactions',
            nullable: false,
            isPrimaryKey: true,
            isForeignKey: false,
            relevanceScore: 0.8
          },
          {
            name: 'amount',
            type: 'number',
            description: 'Sale amount in USD',
            businessMeaning: 'Revenue value of the transaction',
            nullable: false,
            isPrimaryKey: false,
            isForeignKey: false,
            relevanceScore: 0.98
          },
          {
            name: 'region_id',
            type: 'string',
            description: 'Foreign key to region dimension',
            businessMeaning: 'Geographic region identifier',
            nullable: false,
            isPrimaryKey: false,
            isForeignKey: true,
            relevanceScore: 0.92
          },
          {
            name: 'sale_date',
            type: 'date',
            description: 'Date of the sale transaction',
            businessMeaning: 'Temporal dimension for time-based analysis',
            nullable: false,
            isPrimaryKey: false,
            isForeignKey: false,
            relevanceScore: 0.90
          }
        ],
        relationships: [
          {
            relatedTable: 'region_dim',
            relationshipType: 'many-to-one' as const,
            strength: 0.9,
            description: 'Sales belong to regions',
            joinCondition: 'sales_fact.region_id = region_dim.region_id'
          },
          {
            relatedTable: 'time_dim',
            relationshipType: 'many-to-one' as const,
            strength: 0.85,
            description: 'Sales occur on specific dates',
            joinCondition: 'sales_fact.sale_date = time_dim.date'
          }
        ],
        estimatedRowCount: 2500000,
        lastUpdated: '2024-01-15T08:00:00Z',
        dataQuality: {
          completeness: 0.98,
          accuracy: 0.95,
          consistency: 0.92
        }
      },
      {
        name: 'region_dim',
        relevanceScore: 0.88,
        description: 'Geographic region dimension table',
        businessPurpose: 'Regional categorization for geographic analysis',
        columns: [
          {
            name: 'region_id',
            type: 'string',
            description: 'Unique region identifier',
            businessMeaning: 'Primary key for regions',
            nullable: false,
            isPrimaryKey: true,
            isForeignKey: false,
            relevanceScore: 0.85
          },
          {
            name: 'region_name',
            type: 'string',
            description: 'Human-readable region name',
            businessMeaning: 'Display name for regional grouping',
            nullable: false,
            isPrimaryKey: false,
            isForeignKey: false,
            relevanceScore: 0.95
          }
        ],
        relationships: [
          {
            relatedTable: 'sales_fact',
            relationshipType: 'one-to-many' as const,
            strength: 0.9,
            description: 'Regions contain multiple sales',
            joinCondition: 'region_dim.region_id = sales_fact.region_id'
          }
        ],
        estimatedRowCount: 50,
        lastUpdated: '2024-01-10T10:00:00Z',
        dataQuality: {
          completeness: 1.0,
          accuracy: 0.98,
          consistency: 0.95
        }
      }
    ],
    suggestedJoins: [
      {
        tables: ['sales_fact', 'region_dim'],
        joinType: 'INNER JOIN',
        condition: 'sales_fact.region_id = region_dim.region_id',
        confidence: 0.95,
        reasoning: 'Required for regional analysis - high confidence foreign key relationship'
      },
      {
        tables: ['sales_fact', 'time_dim'],
        joinType: 'LEFT JOIN',
        condition: 'sales_fact.sale_date = time_dim.date',
        confidence: 0.85,
        reasoning: 'Recommended for time-based aggregation and quarterly analysis'
      }
    ],
    optimizationTips: [
      {
        type: 'indexing' as const,
        suggestion: 'Consider adding composite index on (region_id, sale_date) for time-based regional queries',
        impact: 'high' as const,
        estimatedImprovement: '60% faster execution for regional time series queries'
      },
      {
        type: 'partitioning' as const,
        suggestion: 'Partition sales_fact table by sale_date for improved query performance',
        impact: 'medium' as const,
        estimatedImprovement: '40% faster execution for date range queries'
      }
    ],
    potentialIssues: [
      {
        type: 'data_quality' as const,
        description: 'Some regions may have incomplete data for Q4 2023',
        severity: 'medium' as const,
        recommendation: 'Filter out incomplete records or use data imputation'
      },
      {
        type: 'performance' as const,
        description: 'Large table scan detected for sales_fact without proper indexing',
        severity: 'high' as const,
        recommendation: 'Add indexes on frequently queried columns'
      }
    ]
  }), [])

  const handleTableSelect = useCallback((tableName: string) => {
    setSelectedTable(tableName)
    if (onTableSelect) {
      onTableSelect(tableName)
    }
  }, [onTableSelect])

  const handleJoinApply = useCallback((suggestion: JoinSuggestion) => {
    if (onJoinSuggestionApply) {
      onJoinSuggestionApply(suggestion)
    }
    message.success(`Applied join suggestion: ${suggestion.tables.join(' → ')}`)
  }, [onJoinSuggestionApply])

  const handleOptimizationApply = useCallback((tip: OptimizationTip) => {
    if (onOptimizationApply) {
      onOptimizationApply(tip)
    }
    message.success(`Applied optimization: ${tip.type}`)
  }, [onOptimizationApply])

  // Utility functions
  const getRelevanceColor = (score: number) => {
    if (score >= 0.9) return '#52c41a'
    if (score >= 0.8) return '#1890ff'
    if (score >= 0.7) return '#fa8c16'
    return '#ff4d4f'
  }

  const getDataQualityStatus = (quality: DataQuality) => {
    const average = (quality.completeness + quality.accuracy + quality.consistency) / 3
    if (average >= 0.9) return { status: 'success' as const, text: 'Excellent' }
    if (average >= 0.8) return { status: 'normal' as const, text: 'Good' }
    if (average >= 0.7) return { status: 'normal' as const, text: 'Fair' }
    return { status: 'exception' as const, text: 'Poor' }
  }

  const getSeverityColor = (severity: string) => {
    const colors: Record<string, string> = {
      'high': '#ff4d4f',
      'medium': '#fa8c16',
      'low': '#52c41a'
    }
    return colors[severity] || '#d9d9d9'
  }

  const getImpactIcon = (impact: string) => {
    const icons: Record<string, React.ReactNode> = {
      'high': <ThunderboltOutlined style={{ color: '#ff4d4f' }} />,
      'medium': <BarChartOutlined style={{ color: '#fa8c16' }} />,
      'low': <InfoCircleOutlined style={{ color: '#52c41a' }} />
    }
    return icons[impact] || <InfoCircleOutlined />
  }

  if (loading) {
    return (
      <Card>
        <div style={{ textAlign: 'center', padding: '40px 0' }}>
          <Spin size="large" />
          <div style={{ marginTop: 16 }}>
            <Text>Analyzing schema context...</Text>
            <br />
            <Text type="secondary" style={{ fontSize: '12px' }}>
              Identifying relevant tables, relationships, and optimization opportunities...
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
              <Text>Enter a query to see schema intelligence analysis</Text>
              <br />
              <Text type="secondary" style={{ fontSize: '12px' }}>
                AI will analyze relevant tables, relationships, and suggest optimizations
              </Text>
            </div>
          }
        />
      </Card>
    )
  }

  return (
    <Space direction="vertical" style={{ width: '100%' }} size="large">
      {/* Overview Statistics */}
      <Card
        title={
          <Space>
            <DatabaseOutlined />
            <span>Schema Intelligence Overview</span>
            <Badge count={mockSchemaData.relevantTables.length} style={{ backgroundColor: '#1890ff' }} />
          </Space>
        }
        extra={
          interactive && (
            <Space>
              <Tooltip title="Refresh analysis">
                <Button icon={<SearchOutlined />} size="small" />
              </Tooltip>
              <Tooltip title="Export schema">
                <Button icon={<FileTextOutlined />} size="small" />
              </Tooltip>
            </Space>
          )
        }
      >
        <Row gutter={[16, 16]}>
          <Col xs={24} sm={12} md={6}>
            <Statistic
              title="Relevant Tables"
              value={mockSchemaData.relevantTables.length}
              prefix={<TableOutlined />}
              valueStyle={{ color: '#1890ff' }}
            />
          </Col>
          <Col xs={24} sm={12} md={6}>
            <Statistic
              title="Join Suggestions"
              value={mockSchemaData.suggestedJoins.length}
              prefix={<LinkOutlined />}
              valueStyle={{ color: '#52c41a' }}
            />
          </Col>
          <Col xs={24} sm={12} md={6}>
            <Statistic
              title="Optimizations"
              value={mockSchemaData.optimizationTips.length}
              prefix={<ThunderboltOutlined />}
              valueStyle={{ color: '#fa8c16' }}
            />
          </Col>
          <Col xs={24} sm={12} md={6}>
            <Statistic
              title="Issues Found"
              value={mockSchemaData.potentialIssues.length}
              prefix={<WarningOutlined />}
              valueStyle={{ color: '#ff4d4f' }}
            />
          </Col>
        </Row>
      </Card>

      {/* Main Content Tabs */}
      <Card>
        <Tabs
          activeKey={activeTab}
          onChange={setActiveTab}
          size="large"
          type="card"
        >
          {/* Relevant Tables Tab */}
          <TabPane
            tab={
              <Space>
                <TableOutlined />
                <span>Relevant Tables</span>
                <Badge count={mockSchemaData.relevantTables.length} size="small" />
              </Space>
            }
            key="tables"
          >
            <Space direction="vertical" style={{ width: '100%' }} size="middle">
              {mockSchemaData.relevantTables.map((table) => (
                <Card
                  key={table.name}
                  size="small"
                  hoverable={interactive}
                  onClick={() => interactive && handleTableSelect(table.name)}
                  style={{
                    cursor: interactive ? 'pointer' : 'default',
                    border: selectedTable === table.name ? '2px solid #1890ff' : undefined
                  }}
                  title={
                    <Space>
                      <TableOutlined style={{ color: getRelevanceColor(table.relevanceScore) }} />
                      <span>{table.name}</span>
                      <Tag color={getRelevanceColor(table.relevanceScore)}>
                        {Math.round(table.relevanceScore * 100)}% relevant
                      </Tag>
                      <Tag color={getDataQualityStatus(table.dataQuality).status}>
                        {getDataQualityStatus(table.dataQuality).text} Quality
                      </Tag>
                    </Space>
                  }
                  extra={
                    interactive && (
                      <Button
                        type="link"
                        size="small"
                        icon={<EyeOutlined />}
                        onClick={(e) => {
                          e.stopPropagation()
                          handleTableSelect(table.name)
                        }}
                      >
                        Explore
                      </Button>
                    )
                  }
                >
                  <Space direction="vertical" style={{ width: '100%' }}>
                    <Paragraph style={{ marginBottom: 8 }}>
                      {table.description}
                    </Paragraph>

                    <Row gutter={[16, 8]}>
                      <Col xs={24} sm={8}>
                        <Statistic
                          title="Columns"
                          value={table.columns.length}
                          valueStyle={{ fontSize: '14px' }}
                        />
                      </Col>
                      <Col xs={24} sm={8}>
                        <Statistic
                          title="Relationships"
                          value={table.relationships.length}
                          valueStyle={{ fontSize: '14px' }}
                        />
                      </Col>
                      <Col xs={24} sm={8}>
                        <Statistic
                          title="Est. Rows"
                          value={table.estimatedRowCount.toLocaleString()}
                          valueStyle={{ fontSize: '14px' }}
                        />
                      </Col>
                    </Row>

                    <Divider style={{ margin: '12px 0' }} />

                    <div>
                      <Text strong style={{ color: '#1890ff' }}>Key Columns:</Text>
                      <div style={{ marginTop: 4 }}>
                        <Space wrap>
                          {table.columns
                            .filter(col => col.relevanceScore > 0.8)
                            .slice(0, 4)
                            .map((col) => (
                              <Tag
                                key={col.name}
                                color={col.isPrimaryKey ? 'gold' : col.isForeignKey ? 'blue' : 'green'}
                                style={{ fontSize: '11px' }}
                              >
                                {col.name}
                                {col.isPrimaryKey && ' (PK)'}
                                {col.isForeignKey && ' (FK)'}
                              </Tag>
                            ))}
                          {table.columns.filter(col => col.relevanceScore > 0.8).length > 4 && (
                            <Tag color="default" style={{ fontSize: '11px' }}>
                              +{table.columns.filter(col => col.relevanceScore > 0.8).length - 4} more
                            </Tag>
                          )}
                        </Space>
                      </div>
                    </div>
                  </Space>
                </Card>
              ))}
            </Space>
          </TabPane>

          {/* Table Relationships Tab */}
          {showRelationships && (
            <TabPane
              tab={
                <Space>
                  <LinkOutlined />
                  <span>Relationships</span>
                </Space>
              }
              key="relationships"
            >
              <Space direction="vertical" style={{ width: '100%' }} size="middle">
                <Alert
                  message="Table Relationship Analysis"
                  description="Understanding how tables connect helps optimize joins and query performance"
                  type="info"
                  showIcon
                />

                {mockSchemaData.relevantTables.map((table) => (
                  <Card
                    key={`rel-${table.name}`}
                    title={
                      <Space>
                        <NodeIndexOutlined />
                        <span>{table.name} Relationships</span>
                        <Badge count={table.relationships.length} />
                      </Space>
                    }
                    size="small"
                  >
                    {table.relationships.length > 0 ? (
                      <List
                        dataSource={table.relationships}
                        renderItem={(rel) => (
                          <List.Item
                            actions={interactive ? [
                              <Button
                                key="apply"
                                type="link"
                                size="small"
                                onClick={() => console.log('Apply relationship:', rel)}
                              >
                                Apply Join
                              </Button>
                            ] : undefined}
                          >
                            <List.Item.Meta
                              avatar={
                                <Avatar
                                  style={{
                                    backgroundColor: getRelevanceColor(rel.strength),
                                    fontSize: '12px'
                                  }}
                                >
                                  {Math.round(rel.strength * 100)}%
                                </Avatar>
                              }
                              title={
                                <Space>
                                  <Text strong>{table.name}</Text>
                                  <Tag color="blue">{rel.relationshipType}</Tag>
                                  <Text strong>{rel.relatedTable}</Text>
                                </Space>
                              }
                              description={
                                <div>
                                  <Text type="secondary">{rel.description}</Text>
                                  <br />
                                  <Text code style={{ fontSize: '11px' }}>
                                    {rel.joinCondition}
                                  </Text>
                                </div>
                              }
                            />
                          </List.Item>
                        )}
                      />
                    ) : (
                      <Empty
                        image={Empty.PRESENTED_IMAGE_SIMPLE}
                        description="No relationships found for this table"
                      />
                    )}
                  </Card>
                ))}
              </Space>
            </TabPane>
          )}

          {/* Join Suggestions Tab */}
          <TabPane
            tab={
              <Space>
                <BulbOutlined />
                <span>Join Suggestions</span>
                <Badge count={mockSchemaData.suggestedJoins.length} size="small" />
              </Space>
            }
            key="joins"
          >
            <Space direction="vertical" style={{ width: '100%' }} size="middle">
              <Alert
                message="Intelligent Join Recommendations"
                description="AI-powered suggestions for optimal table joins based on your query context"
                type="success"
                showIcon
              />

              {mockSchemaData.suggestedJoins.map((suggestion, index) => (
                <Card
                  key={index}
                  size="small"
                  title={
                    <Space>
                      <LinkOutlined style={{ color: '#52c41a' }} />
                      <span>{suggestion.tables.join(' → ')}</span>
                      <Tag color="green">{Math.round(suggestion.confidence * 100)}% confidence</Tag>
                    </Space>
                  }
                  extra={
                    interactive && (
                      <Button
                        type="primary"
                        size="small"
                        icon={<CheckCircleOutlined />}
                        onClick={() => handleJoinApply(suggestion)}
                      >
                        Apply Join
                      </Button>
                    )
                  }
                >
                  <Space direction="vertical" style={{ width: '100%' }}>
                    <div>
                      <Text strong>Join Type: </Text>
                      <Tag color="blue">{suggestion.joinType}</Tag>
                    </div>

                    <div>
                      <Text strong>Condition: </Text>
                      <Text code>{suggestion.condition}</Text>
                    </div>

                    <div>
                      <Text strong>Reasoning: </Text>
                      <Text>{suggestion.reasoning}</Text>
                    </div>

                    <Progress
                      percent={Math.round(suggestion.confidence * 100)}
                      strokeColor="#52c41a"
                      format={(percent) => `${percent}% confidence`}
                    />
                  </Space>
                </Card>
              ))}
            </Space>
          </TabPane>

          {/* Optimization Tips Tab */}
          {showOptimizations && (
            <TabPane
              tab={
                <Space>
                  <ThunderboltOutlined />
                  <span>Optimizations</span>
                  <Badge count={mockSchemaData.optimizationTips.length} size="small" />
                </Space>
              }
              key="optimizations"
            >
              <Space direction="vertical" style={{ width: '100%' }} size="middle">
                <Alert
                  message="Performance Optimization Recommendations"
                  description="Suggestions to improve query performance and database efficiency"
                  type="warning"
                  showIcon
                />

                {mockSchemaData.optimizationTips.map((tip, index) => (
                  <Card
                    key={index}
                    size="small"
                    title={
                      <Space>
                        {getImpactIcon(tip.impact)}
                        <span>{tip.type.charAt(0).toUpperCase() + tip.type.slice(1)} Optimization</span>
                        <Tag color={tip.impact === 'high' ? 'red' : tip.impact === 'medium' ? 'orange' : 'green'}>
                          {tip.impact.toUpperCase()} IMPACT
                        </Tag>
                      </Space>
                    }
                    extra={
                      interactive && (
                        <Button
                          type="primary"
                          size="small"
                          icon={<SettingOutlined />}
                          onClick={() => handleOptimizationApply(tip)}
                        >
                          Apply
                        </Button>
                      )
                    }
                  >
                    <Space direction="vertical" style={{ width: '100%' }}>
                      <Paragraph>{tip.suggestion}</Paragraph>

                      <div>
                        <Text strong style={{ color: '#52c41a' }}>Expected Improvement: </Text>
                        <Text>{tip.estimatedImprovement}</Text>
                      </div>
                    </Space>
                  </Card>
                ))}
              </Space>
            </TabPane>
          )}

          {/* Issues & Warnings Tab */}
          <TabPane
            tab={
              <Space>
                <WarningOutlined />
                <span>Issues</span>
                <Badge count={mockSchemaData.potentialIssues.length} size="small" />
              </Space>
            }
            key="issues"
          >
            <Space direction="vertical" style={{ width: '100%' }} size="middle">
              <Alert
                message="Potential Data & Performance Issues"
                description="Issues that may affect query results or performance"
                type="error"
                showIcon
              />

              {mockSchemaData.potentialIssues.map((issue, index) => (
                <Card
                  key={index}
                  size="small"
                  title={
                    <Space>
                      <WarningOutlined style={{ color: getSeverityColor(issue.severity) }} />
                      <span>{issue.type.replace('_', ' ').toUpperCase()} Issue</span>
                      <Tag color={getSeverityColor(issue.severity)}>
                        {issue.severity.toUpperCase()}
                      </Tag>
                    </Space>
                  }
                >
                  <Space direction="vertical" style={{ width: '100%' }}>
                    <div>
                      <Text strong>Description: </Text>
                      <Text>{issue.description}</Text>
                    </div>

                    <div>
                      <Text strong style={{ color: '#52c41a' }}>Recommendation: </Text>
                      <Text>{issue.recommendation}</Text>
                    </div>
                  </Space>
                </Card>
              ))}
            </Space>
          </TabPane>
        </Tabs>
      </Card>
    </Space>
  )
}

export default SchemaIntelligencePanel
