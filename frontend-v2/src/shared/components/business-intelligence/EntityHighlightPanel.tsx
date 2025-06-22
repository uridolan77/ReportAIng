import React, { useState, useCallback, useMemo } from 'react'
import {
  Card,
  Empty,
  Spin,
  Typography,
  Space,
  Tag,
  Button,
  Tooltip,
  Progress,
  Badge,
  List,
  Avatar,
  Row,
  Col,
  Statistic,
  Tabs,
  Tree,
  Timeline,
  Divider,
  Alert,
  Popover,
  Rate,
  message
} from 'antd'
import {
  TagsOutlined,
  LinkOutlined,
  InfoCircleOutlined,
  EyeOutlined,
  EditOutlined,
  BranchesOutlined,
  NodeIndexOutlined,
  ThunderboltOutlined,
  CheckCircleOutlined,
  WarningOutlined,
  StarOutlined,
  DatabaseOutlined,
  SearchOutlined,
  ClusterOutlined,
  BookOutlined
} from '@ant-design/icons'
import type { BusinessEntity } from '@shared/types/business-intelligence'

const { Text, Title, Paragraph } = Typography
const { TabPane } = Tabs

interface EntityRelationship {
  fromEntityId: string
  toEntityId: string
  relationshipType: 'synonym' | 'related' | 'parent' | 'child' | 'opposite' | 'contains'
  strength: number
  description: string
  businessContext: string
}

interface EntityMapping {
  entityId: string
  tableName: string
  columnName: string
  mappingType: 'direct' | 'calculated' | 'derived'
  confidence: number
  transformation?: string
}

interface EntityHighlightPanelProps {
  query: string
  entities: BusinessEntity[]
  loading?: boolean
  interactive?: boolean
  showConfidence?: boolean
  showTooltips?: boolean
  showRelationships?: boolean
  showMappings?: boolean
  showAdvancedAnalysis?: boolean
  onEntityClick?: (entityId: string) => void
  onEntityEdit?: (entityId: string) => void
  onRelationshipExplore?: (relationship: EntityRelationship) => void
}

/**
 * EntityHighlightPanel - Advanced entity detection and relationship analysis
 *
 * Features:
 * - Interactive entity highlighting with confidence scoring
 * - Relationship mapping and visualization
 * - Schema mapping with transformation details
 * - Advanced entity analysis with business context
 * - Interactive exploration and editing capabilities
 * - Performance metrics and optimization insights
 */
export const EntityHighlightPanel: React.FC<EntityHighlightPanelProps> = ({
  query,
  entities,
  loading = false,
  interactive = true,
  showConfidence = true,
  showTooltips = true,
  showRelationships = true,
  showMappings = true,
  showAdvancedAnalysis = true,
  onEntityClick,
  onEntityEdit,
  onRelationshipExplore
}) => {
  const [activeTab, setActiveTab] = useState('highlight')
  const [selectedEntity, setSelectedEntity] = useState<string | null>(null)
  const [hoveredEntity, setHoveredEntity] = useState<string | null>(null)

  // Mock data for enhanced features - will be replaced with real API data
  const mockEnhancedData = useMemo(() => ({
    relationships: [
      {
        fromEntityId: 'entity-1',
        toEntityId: 'entity-2',
        relationshipType: 'related' as const,
        strength: 0.85,
        description: 'Sales and revenue are closely related business metrics',
        businessContext: 'Both metrics are used for financial performance analysis'
      },
      {
        fromEntityId: 'entity-1',
        toEntityId: 'entity-3',
        relationshipType: 'contains' as const,
        strength: 0.92,
        description: 'Sales data contains regional information',
        businessContext: 'Regional breakdown is essential for sales analysis'
      }
    ],
    mappings: [
      {
        entityId: 'entity-1',
        tableName: 'sales_fact',
        columnName: 'amount',
        mappingType: 'direct' as const,
        confidence: 0.95,
        transformation: 'SUM(amount) for aggregation queries'
      },
      {
        entityId: 'entity-2',
        tableName: 'region_dim',
        columnName: 'region_name',
        mappingType: 'direct' as const,
        confidence: 0.88,
        transformation: 'GROUP BY region_name for regional analysis'
      }
    ],
    entityAnalysis: {
      totalEntities: entities.length,
      averageConfidence: entities.length > 0 ? entities.reduce((sum, e) => sum + e.confidence, 0) / entities.length : 0,
      highConfidenceEntities: entities.filter(e => e.confidence > 0.8).length,
      businessEntities: entities.filter(e => e.type === 'business_metric').length,
      technicalEntities: entities.filter(e => e.type === 'dimension').length
    }
  }), [entities])

  const handleEntityClick = useCallback((entityId: string) => {
    setSelectedEntity(entityId)
    if (onEntityClick) {
      onEntityClick(entityId)
    }
  }, [onEntityClick])

  const handleEntityEdit = useCallback((entityId: string) => {
    if (onEntityEdit) {
      onEntityEdit(entityId)
    }
    message.success('Entity editing functionality would open here')
  }, [onEntityEdit])

  const handleRelationshipExplore = useCallback((relationship: EntityRelationship) => {
    if (onRelationshipExplore) {
      onRelationshipExplore(relationship)
    }
    message.info(`Exploring relationship: ${relationship.relationshipType}`)
  }, [onRelationshipExplore])

  // Utility functions
  const getConfidenceColor = (confidence: number) => {
    if (confidence >= 0.9) return '#52c41a'
    if (confidence >= 0.8) return '#1890ff'
    if (confidence >= 0.7) return '#fa8c16'
    return '#ff4d4f'
  }

  const getEntityTypeIcon = (type: string) => {
    const icons: Record<string, React.ReactNode> = {
      'business_metric': <StarOutlined style={{ color: '#52c41a' }} />,
      'dimension': <DatabaseOutlined style={{ color: '#1890ff' }} />,
      'measure': <ThunderboltOutlined style={{ color: '#fa8c16' }} />,
      'attribute': <TagsOutlined style={{ color: '#722ed1' }} />,
      'time': <ClusterOutlined style={{ color: '#13c2c2' }} />
    }
    return icons[type] || <InfoCircleOutlined />
  }

  const getRelationshipIcon = (type: string) => {
    const icons: Record<string, React.ReactNode> = {
      'synonym': <TagsOutlined style={{ color: '#52c41a' }} />,
      'related': <LinkOutlined style={{ color: '#1890ff' }} />,
      'parent': <BranchesOutlined style={{ color: '#722ed1' }} />,
      'child': <NodeIndexOutlined style={{ color: '#fa8c16' }} />,
      'opposite': <WarningOutlined style={{ color: '#ff4d4f' }} />,
      'contains': <ClusterOutlined style={{ color: '#13c2c2' }} />
    }
    return icons[type] || <InfoCircleOutlined />
  }

  const getMappingTypeColor = (type: string) => {
    const colors: Record<string, string> = {
      'direct': '#52c41a',
      'calculated': '#1890ff',
      'derived': '#fa8c16'
    }
    return colors[type] || '#d9d9d9'
  }

  // Enhanced entity highlighting with interactive features
  const highlightEntities = (text: string, entities: BusinessEntity[]) => {
    if (!entities.length) return text

    let highlightedText = text
    entities.forEach((entity, index) => {
      const regex = new RegExp(`\\b${entity.name}\\b`, 'gi')
      const confidenceColor = getConfidenceColor(entity.confidence)
      const isSelected = selectedEntity === entity.id
      const isHovered = hoveredEntity === entity.id

      highlightedText = highlightedText.replace(
        regex,
        `<mark
          data-entity-id="${entity.id}"
          style="
            background-color: ${confidenceColor}20;
            color: ${confidenceColor};
            padding: 3px 6px;
            border-radius: 4px;
            border: 2px solid ${isSelected ? confidenceColor : 'transparent'};
            cursor: ${interactive ? 'pointer' : 'default'};
            font-weight: ${isSelected || isHovered ? 'bold' : 'normal'};
            transition: all 0.2s ease;
          "
          title="${entity.businessMeaning || entity.name} (${Math.round(entity.confidence * 100)}% confidence)"
        >${entity.name}</mark>`
      )
    })
    return highlightedText
  }

  if (loading) {
    return (
      <Card>
        <div style={{ textAlign: 'center', padding: '40px 0' }}>
          <Spin size="large" />
          <Text>Detecting entities...</Text>
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
              <Text>Enter a query to see entity detection</Text>
              <br />
              <Text type="secondary" style={{ fontSize: '12px' }}>
                AI will identify and highlight business entities with confidence scoring
              </Text>
            </div>
          }
        />
      </Card>
    )
  }

  const overallConfidence = mockEnhancedData.entityAnalysis.averageConfidence

  return (
    <Space direction="vertical" style={{ width: '100%' }} size="large">
      {/* Overview Header */}
      <Card
        title={
          <Space>
            <TagsOutlined />
            <span>Entity Detection & Analysis</span>
            <Badge
              count={`${entities.length} entities`}
              style={{ backgroundColor: '#52c41a' }}
            />
          </Space>
        }
        extra={
          interactive && (
            <Space>
              <Tooltip title="View entity relationships">
                <Button icon={<BranchesOutlined />} size="small" />
              </Tooltip>
              <Tooltip title="Export entity analysis">
                <Button icon={<SearchOutlined />} size="small" />
              </Tooltip>
            </Space>
          )
        }
      >
        <Row gutter={[16, 16]}>
          <Col xs={24} sm={12} md={6}>
            <Statistic
              title="Total Entities"
              value={mockEnhancedData.entityAnalysis.totalEntities}
              valueStyle={{ color: '#1890ff' }}
              prefix={<TagsOutlined />}
            />
          </Col>
          <Col xs={24} sm={12} md={6}>
            <Statistic
              title="Average Confidence"
              value={overallConfidence}
              precision={2}
              suffix="/ 1.0"
              valueStyle={{ color: getConfidenceColor(overallConfidence) }}
              prefix={<StarOutlined />}
            />
          </Col>
          <Col xs={24} sm={12} md={6}>
            <Statistic
              title="High Confidence"
              value={mockEnhancedData.entityAnalysis.highConfidenceEntities}
              valueStyle={{ color: '#52c41a' }}
              prefix={<CheckCircleOutlined />}
            />
          </Col>
          <Col xs={24} sm={12} md={6}>
            <Statistic
              title="Business Entities"
              value={mockEnhancedData.entityAnalysis.businessEntities}
              valueStyle={{ color: '#fa8c16' }}
              prefix={<BookOutlined />}
            />
          </Col>
        </Row>

        <Alert
          message="Entity Detection Complete"
          description={`Detected ${entities.length} entities with ${Math.round(overallConfidence * 100)}% average confidence. Click on highlighted entities for detailed analysis.`}
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
          {/* Entity Highlighting Tab */}
          <TabPane
            tab={
              <Space>
                <EyeOutlined />
                <span>Highlighted Query</span>
              </Space>
            }
            key="highlight"
          >
            <Space direction="vertical" style={{ width: '100%' }} size="large">
              <Card size="small" title="Query with Interactive Entity Highlighting">
                <div
                  style={{
                    fontSize: '16px',
                    lineHeight: '1.8',
                    padding: '20px',
                    backgroundColor: '#fafafa',
                    borderRadius: '8px',
                    border: '1px solid #d9d9d9',
                    minHeight: '60px'
                  }}
                  dangerouslySetInnerHTML={{
                    __html: highlightEntities(query, entities)
                  }}
                  onClick={(e) => {
                    const target = e.target as HTMLElement
                    const entityId = target.getAttribute('data-entity-id')
                    if (entityId && interactive) {
                      handleEntityClick(entityId)
                    }
                  }}
                  onMouseOver={(e) => {
                    const target = e.target as HTMLElement
                    const entityId = target.getAttribute('data-entity-id')
                    if (entityId) {
                      setHoveredEntity(entityId)
                    }
                  }}
                  onMouseOut={() => setHoveredEntity(null)}
                />

                <Divider />

                <Text type="secondary" style={{ fontSize: '12px' }}>
                  💡 <strong>Tip:</strong> Click on highlighted entities to view detailed analysis.
                  Colors indicate confidence levels:
                  <Tag color="green" size="small" style={{ margin: '0 4px' }}>High (90%+)</Tag>
                  <Tag color="blue" size="small" style={{ margin: '0 4px' }}>Good (80%+)</Tag>
                  <Tag color="orange" size="small" style={{ margin: '0 4px' }}>Medium (70%+)</Tag>
                  <Tag color="red" size="small" style={{ margin: '0 4px' }}>Low (&lt;70%)</Tag>
                </Text>
              </Card>
            </Space>
          </TabPane>

          {/* Entity Details Tab */}
          <TabPane
            tab={
              <Space>
                <InfoCircleOutlined />
                <span>Entity Details</span>
                <Badge count={entities.length} size="small" />
              </Space>
            }
            key="details"
          >
            <Space direction="vertical" style={{ width: '100%' }} size="middle">
              {entities.length > 0 ? (
                entities.map((entity) => (
                  <Card
                    key={entity.id}
                    size="small"
                    hoverable={interactive}
                    onClick={() => interactive && handleEntityClick(entity.id)}
                    style={{
                      cursor: interactive ? 'pointer' : 'default',
                      border: selectedEntity === entity.id ? '2px solid #1890ff' : undefined
                    }}
                    title={
                      <Space>
                        {getEntityTypeIcon(entity.type)}
                        <span style={{ fontWeight: 600 }}>{entity.name}</span>
                        <Tag color={getConfidenceColor(entity.confidence)}>
                          {Math.round(entity.confidence * 100)}% confidence
                        </Tag>
                        <Tag color="blue">{entity.type}</Tag>
                        {selectedEntity === entity.id && (
                          <Tag color="green">SELECTED</Tag>
                        )}
                      </Space>
                    }
                    extra={
                      interactive && (
                        <Space>
                          <Button
                            type="link"
                            size="small"
                            icon={<EyeOutlined />}
                            onClick={(e) => {
                              e.stopPropagation()
                              handleEntityClick(entity.id)
                            }}
                          >
                            View
                          </Button>
                          <Button
                            type="link"
                            size="small"
                            icon={<EditOutlined />}
                            onClick={(e) => {
                              e.stopPropagation()
                              handleEntityEdit(entity.id)
                            }}
                          >
                            Edit
                          </Button>
                        </Space>
                      )
                    }
                  >
                    <Space direction="vertical" style={{ width: '100%' }}>
                      {entity.businessMeaning && (
                        <div>
                          <Text strong>Business Meaning: </Text>
                          <Text>{entity.businessMeaning}</Text>
                        </div>
                      )}

                      {entity.suggestedColumns && entity.suggestedColumns.length > 0 && (
                        <div>
                          <Text strong>Suggested Database Columns: </Text>
                          <div style={{ marginTop: 4 }}>
                            <Space wrap>
                              {entity.suggestedColumns.map((col, idx) => (
                                <Tag key={idx} color="green">{col}</Tag>
                              ))}
                            </Space>
                          </div>
                        </div>
                      )}

                      <Row gutter={[16, 8]}>
                        <Col span={12}>
                          <Text strong>Confidence Score:</Text>
                          <div style={{ marginTop: 4 }}>
                            <Progress
                              percent={Math.round(entity.confidence * 100)}
                              strokeColor={getConfidenceColor(entity.confidence)}
                              size="small"
                            />
                          </div>
                        </Col>
                        <Col span={12}>
                          <Text strong>Entity Type:</Text>
                          <div style={{ marginTop: 4 }}>
                            <Space>
                              {getEntityTypeIcon(entity.type)}
                              <Text code>{entity.type}</Text>
                            </Space>
                          </div>
                        </Col>
                      </Row>
                    </Space>
                  </Card>
                ))
              ) : (
                <Empty
                  description="No entities detected in this query"
                  image={Empty.PRESENTED_IMAGE_SIMPLE}
                />
              )}
            </Space>
          </TabPane>

          {/* Relationships Tab */}
          {showRelationships && (
            <TabPane
              tab={
                <Space>
                  <BranchesOutlined />
                  <span>Relationships</span>
                  <Badge count={mockEnhancedData.relationships.length} size="small" />
                </Space>
              }
              key="relationships"
            >
              <Space direction="vertical" style={{ width: '100%' }} size="middle">
                <Alert
                  message="Entity Relationships"
                  description="Explore how detected entities relate to each other in business context."
                  type="info"
                  showIcon
                />

                {mockEnhancedData.relationships.map((relationship, index) => (
                  <Card
                    key={index}
                    size="small"
                    hoverable={interactive}
                    onClick={() => interactive && handleRelationshipExplore(relationship)}
                    style={{ cursor: interactive ? 'pointer' : 'default' }}
                    title={
                      <Space>
                        {getRelationshipIcon(relationship.relationshipType)}
                        <span>Entity Relationship</span>
                        <Tag color={getConfidenceColor(relationship.strength)}>
                          {Math.round(relationship.strength * 100)}% strength
                        </Tag>
                        <Tag color="purple">{relationship.relationshipType}</Tag>
                      </Space>
                    }
                    extra={
                      interactive && (
                        <Button
                          type="primary"
                          size="small"
                          icon={<EyeOutlined />}
                          onClick={(e) => {
                            e.stopPropagation()
                            handleRelationshipExplore(relationship)
                          }}
                        >
                          Explore
                        </Button>
                      )
                    }
                  >
                    <Space direction="vertical" style={{ width: '100%' }}>
                      <div>
                        <Text strong>Relationship: </Text>
                        <Text>{relationship.description}</Text>
                      </div>

                      <div>
                        <Text strong>Business Context: </Text>
                        <Text>{relationship.businessContext}</Text>
                      </div>

                      <div>
                        <Text strong>Connected Entities: </Text>
                        <Space>
                          <Tag color="blue">{relationship.fromEntityId}</Tag>
                          <span>→</span>
                          <Tag color="green">{relationship.toEntityId}</Tag>
                        </Space>
                      </div>

                      <Progress
                        percent={Math.round(relationship.strength * 100)}
                        strokeColor={getConfidenceColor(relationship.strength)}
                        format={(percent) => `${percent}% relationship strength`}
                      />
                    </Space>
                  </Card>
                ))}

                {mockEnhancedData.relationships.length === 0 && (
                  <Empty
                    description="No entity relationships detected"
                    image={Empty.PRESENTED_IMAGE_SIMPLE}
                  />
                )}
              </Space>
            </TabPane>
          )}

          {/* Schema Mappings Tab */}
          {showMappings && (
            <TabPane
              tab={
                <Space>
                  <DatabaseOutlined />
                  <span>Schema Mappings</span>
                  <Badge count={mockEnhancedData.mappings.length} size="small" />
                </Space>
              }
              key="mappings"
            >
              <Space direction="vertical" style={{ width: '100%' }} size="middle">
                <Alert
                  message="Database Schema Mappings"
                  description="See how detected entities map to actual database tables and columns."
                  type="info"
                  showIcon
                />

                {mockEnhancedData.mappings.map((mapping, index) => (
                  <Card
                    key={index}
                    size="small"
                    title={
                      <Space>
                        <DatabaseOutlined />
                        <span>Schema Mapping</span>
                        <Tag color={getMappingTypeColor(mapping.mappingType)}>
                          {mapping.mappingType}
                        </Tag>
                        <Tag color={getConfidenceColor(mapping.confidence)}>
                          {Math.round(mapping.confidence * 100)}% confidence
                        </Tag>
                      </Space>
                    }
                  >
                    <Space direction="vertical" style={{ width: '100%' }}>
                      <Row gutter={[16, 8]}>
                        <Col span={8}>
                          <Text strong>Entity:</Text>
                          <div style={{ marginTop: 4 }}>
                            <Tag color="blue">{mapping.entityId}</Tag>
                          </div>
                        </Col>
                        <Col span={8}>
                          <Text strong>Table:</Text>
                          <div style={{ marginTop: 4 }}>
                            <Text code>{mapping.tableName}</Text>
                          </div>
                        </Col>
                        <Col span={8}>
                          <Text strong>Column:</Text>
                          <div style={{ marginTop: 4 }}>
                            <Text code>{mapping.columnName}</Text>
                          </div>
                        </Col>
                      </Row>

                      {mapping.transformation && (
                        <div>
                          <Text strong>Transformation: </Text>
                          <div style={{ marginTop: 4, padding: 8, backgroundColor: '#f6f6f6', borderRadius: 4 }}>
                            <Text code style={{ fontSize: '12px' }}>{mapping.transformation}</Text>
                          </div>
                        </div>
                      )}

                      <div>
                        <Text strong>Mapping Quality:</Text>
                        <div style={{ marginTop: 4 }}>
                          <Progress
                            percent={Math.round(mapping.confidence * 100)}
                            strokeColor={getConfidenceColor(mapping.confidence)}
                            format={(percent) => `${percent}% mapping confidence`}
                          />
                        </div>
                      </div>
                    </Space>
                  </Card>
                ))}

                {mockEnhancedData.mappings.length === 0 && (
                  <Empty
                    description="No schema mappings available"
                    image={Empty.PRESENTED_IMAGE_SIMPLE}
                  />
                )}
              </Space>
            </TabPane>
          )}

          {/* Advanced Analysis Tab */}
          {showAdvancedAnalysis && (
            <TabPane
              tab={
                <Space>
                  <ThunderboltOutlined />
                  <span>Advanced Analysis</span>
                </Space>
              }
              key="analysis"
            >
              <Space direction="vertical" style={{ width: '100%' }} size="large">
                {/* Entity Statistics */}
                <Card size="small" title="Entity Analysis Summary">
                  <Row gutter={[16, 16]}>
                    <Col xs={24} md={12}>
                      <Card size="small" style={{ textAlign: 'center' }}>
                        <Statistic
                          title="Detection Accuracy"
                          value={overallConfidence}
                          precision={2}
                          suffix="/ 1.0"
                          valueStyle={{ color: getConfidenceColor(overallConfidence) }}
                        />
                        <Progress
                          percent={Math.round(overallConfidence * 100)}
                          strokeColor={getConfidenceColor(overallConfidence)}
                          showInfo={false}
                          style={{ marginTop: 8 }}
                        />
                      </Card>
                    </Col>
                    <Col xs={24} md={12}>
                      <Card size="small" style={{ textAlign: 'center' }}>
                        <Statistic
                          title="Entity Coverage"
                          value={entities.length}
                          suffix="entities"
                          valueStyle={{ color: '#1890ff' }}
                        />
                        <Text type="secondary" style={{ fontSize: '12px' }}>
                          Detected in query analysis
                        </Text>
                      </Card>
                    </Col>
                  </Row>
                </Card>

                {/* Entity Distribution */}
                <Card size="small" title="Entity Type Distribution">
                  <List
                    dataSource={[
                      { type: 'Business Metrics', count: mockEnhancedData.entityAnalysis.businessEntities, icon: <StarOutlined />, color: '#52c41a' },
                      { type: 'Dimensions', count: mockEnhancedData.entityAnalysis.technicalEntities, icon: <DatabaseOutlined />, color: '#1890ff' },
                      { type: 'High Confidence', count: mockEnhancedData.entityAnalysis.highConfidenceEntities, icon: <CheckCircleOutlined />, color: '#fa8c16' }
                    ]}
                    renderItem={(item) => (
                      <List.Item>
                        <List.Item.Meta
                          avatar={
                            <Avatar
                              style={{ backgroundColor: item.color }}
                              icon={item.icon}
                            />
                          }
                          title={item.type}
                          description={`${item.count} entities detected`}
                        />
                        <div>
                          <Progress
                            percent={Math.round((item.count / entities.length) * 100)}
                            strokeColor={item.color}
                            size="small"
                            style={{ width: 100 }}
                          />
                        </div>
                      </List.Item>
                    )}
                  />
                </Card>

                {/* Recommendations */}
                <Card size="small" title="Optimization Recommendations">
                  <Timeline>
                    <Timeline.Item color="green" dot={<CheckCircleOutlined />}>
                      <Text strong>Entity detection completed successfully</Text>
                      <br />
                      <Text type="secondary">All entities identified with good confidence scores</Text>
                    </Timeline.Item>
                    <Timeline.Item color="blue" dot={<InfoCircleOutlined />}>
                      <Text strong>Consider adding business context</Text>
                      <br />
                      <Text type="secondary">Enhance entity meanings for better analysis</Text>
                    </Timeline.Item>
                    <Timeline.Item color="orange" dot={<WarningOutlined />}>
                      <Text strong>Review low-confidence entities</Text>
                      <br />
                      <Text type="secondary">Validate entities with confidence below 80%</Text>
                    </Timeline.Item>
                  </Timeline>
                </Card>
              </Space>
            </TabPane>
          )}
        </Tabs>
      </Card>
    </Space>
  )
}

export default EntityHighlightPanel
