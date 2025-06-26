import React, { useState, useMemo } from 'react'
import { 
  Card, 
  Row, 
  Col, 
  Tag, 
  Typography, 
  Space, 
  Tooltip, 
  Button,
  Collapse,
  List,
  Progress,
  Badge,
  Divider,
  Tree,
  Select
} from 'antd'
import {
  BulbOutlined,
  DatabaseOutlined,
  TagsOutlined,
  NodeIndexOutlined,
  EyeOutlined,
  InfoCircleOutlined,
  LinkOutlined,
  BookOutlined,
  ThunderboltOutlined,
  BarChartOutlined
} from '@ant-design/icons'
import { ConfidenceIndicator } from '../common/ConfidenceIndicator'
import type { BusinessContextProfile, BusinessEntity, BusinessDomain, QueryIntent } from '@shared/types/ai'

const { Title, Text, Paragraph } = Typography


export interface BusinessContextVisualizerProps {
  context: BusinessContextProfile
  interactive?: boolean
  showDomainDetails?: boolean
  showEntityRelationships?: boolean
  onEntityClick?: (entity: BusinessEntity) => void
  onDomainExplore?: (domain: string) => void
  className?: string
  testId?: string
}

/**
 * BusinessContextVisualizer - Displays comprehensive business context analysis
 * 
 * Features:
 * - Visual business context display with confidence indicators
 * - Interactive entity exploration with relationships
 * - Domain context visualization
 * - Intent classification with alternatives
 * - Business term glossary integration
 * - Contextual help and explanations
 */
export const BusinessContextVisualizer: React.FC<BusinessContextVisualizerProps> = ({
  context,
  interactive = true,
  showDomainDetails = true,
  showEntityRelationships = true,
  onEntityClick,
  onDomainExplore,
  className,
  testId = 'business-context-visualizer'
}) => {
  const [selectedEntity, setSelectedEntity] = useState<BusinessEntity | null>(null)
  const [expandedPanels, setExpandedPanels] = useState<string[]>(['intent', 'entities'])
  const [viewMode, setViewMode] = useState<'grid' | 'tree' | 'network'>('grid')

  // Group entities by type
  const entitiesByType = useMemo(() => {
    return context.entities.reduce((acc, entity) => {
      if (!acc[entity.type]) {
        acc[entity.type] = []
      }
      acc[entity.type].push(entity)
      return acc
    }, {} as Record<string, BusinessEntity[]>)
  }, [context.entities])

  // Get entity type color
  const getEntityTypeColor = (type: string) => {
    const colors = {
      table: '#1890ff',
      column: '#52c41a',
      metric: '#722ed1',
      dimension: '#fa8c16',
      filter: '#13c2c2',
      value: '#eb2f96',
      time: '#faad14'
    }
    return colors[type as keyof typeof colors] || '#d9d9d9'
  }

  // Get entity type icon
  const getEntityTypeIcon = (type: string) => {
    const icons = {
      table: <DatabaseOutlined />,
      column: <TagsOutlined />,
      metric: <BarChartOutlined />,
      dimension: <NodeIndexOutlined />,
      filter: <ThunderboltOutlined />,
      value: <InfoCircleOutlined />,
      time: <BulbOutlined />
    }
    return icons[type as keyof typeof icons] || <InfoCircleOutlined />
  }

  // Get intent complexity color
  const getComplexityColor = (complexity: string) => {
    switch (complexity) {
      case 'simple': return '#52c41a'
      case 'moderate': return '#faad14'
      case 'complex': return '#ff7875'
      default: return '#d9d9d9'
    }
  }

  // Handle entity selection
  const handleEntityClick = (entity: BusinessEntity) => {
    setSelectedEntity(entity)
    onEntityClick?.(entity)
  }

  // Render intent classification
  const renderIntentClassification = () => (
    <Card size="small" title={
      <Space>
        <BulbOutlined />
        <span>Query Intent</span>
        <ConfidenceIndicator
          confidence={context.intent.confidence}
          size="small"
          type="badge"
          showPercentage={false}
        />
      </Space>
    }>
      <Space direction="vertical" style={{ width: '100%' }}>
        <div>
          <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 8 }}>
            <Text strong>Primary Intent</Text>
            <Tag color="blue">{context.intent.type.replace('_', ' ').toUpperCase()}</Tag>
          </div>
          <Paragraph style={{ marginBottom: 8 }}>
            {context.intent.description}
          </Paragraph>
          <div style={{ marginBottom: 8 }}>
            <Text type="secondary">Complexity: </Text>
            <Tag color={getComplexityColor(context.intent.complexity)}>
              {context.intent.complexity.toUpperCase()}
            </Tag>
          </div>
        </div>

        {context.intent.subIntents && context.intent.subIntents.length > 0 && (
          <div>
            <Text strong>Sub-intents:</Text>
            <div style={{ marginTop: 4 }}>
              <Space wrap>
                {context.intent.subIntents.map((subIntent, index) => (
                  <Tag key={index} color="geekblue">{subIntent}</Tag>
                ))}
              </Space>
            </div>
          </div>
        )}

        {context.intent.businessGoal && (
          <div>
            <Text strong>Business Goal:</Text>
            <Paragraph style={{ marginTop: 4, marginBottom: 0 }}>
              {context.intent.businessGoal}
            </Paragraph>
          </div>
        )}
      </Space>
    </Card>
  )

  // Render entities grid
  const renderEntitiesGrid = () => (
    <Card 
      size="small" 
      title={
        <Space>
          <TagsOutlined />
          <span>Business Entities</span>
          <Badge count={context.entities.length} style={{ backgroundColor: '#52c41a' }} />
        </Space>
      }
      extra={
        interactive && (
          <Select
            value={viewMode}
            onChange={setViewMode}
            size="small"
            style={{ width: 100 }}
          >
            <Select.Option value="grid">Grid</Select.Option>
            <Select.Option value="tree">Tree</Select.Option>
            <Select.Option value="network">Network</Select.Option>
          </Select>
        )
      }
    >
      <Space direction="vertical" style={{ width: '100%' }}>
        {Object.entries(entitiesByType).map(([type, entities]) => (
          <div key={type}>
            <div style={{ display: 'flex', alignItems: 'center', marginBottom: 8 }}>
              {getEntityTypeIcon(type)}
              <Text strong style={{ marginLeft: 8 }}>
                {type.charAt(0).toUpperCase() + type.slice(1)}s
              </Text>
              <Badge count={entities.length} style={{ marginLeft: 8 }} />
            </div>
            <div style={{ marginLeft: 24 }}>
              <Space wrap>
                {entities.map((entity, index) => (
                  <Tooltip
                    key={index}
                    title={
                      <div>
                        <div><strong>Business Meaning:</strong> {entity.businessMeaning || 'Not specified'}</div>
                        <div><strong>Context:</strong> {entity.context}</div>
                        <div><strong>Confidence:</strong> {(entity.confidence * 100).toFixed(1)}%</div>
                        {entity.relationships && entity.relationships.length > 0 && (
                          <div><strong>Relationships:</strong> {entity.relationships.length}</div>
                        )}
                      </div>
                    }
                  >
                    <Tag
                      color={getEntityTypeColor(type)}
                      style={{ 
                        cursor: interactive ? 'pointer' : 'default',
                        border: selectedEntity?.id === entity.id ? '2px solid #1890ff' : undefined
                      }}
                      onClick={() => interactive && handleEntityClick(entity)}
                    >
                      <Space size="small">
                        <span>{entity.name}</span>
                        <ConfidenceIndicator
                          confidence={entity.confidence}
                          size="small"
                          type="circle"
                          showPercentage={false}
                        />
                      </Space>
                    </Tag>
                  </Tooltip>
                ))}
              </Space>
            </div>
          </div>
        ))}
      </Space>
    </Card>
  )

  // Render domain context
  const renderDomainContext = () => (
    <Card 
      size="small" 
      title={
        <Space>
          <BookOutlined />
          <span>Business Domain</span>
          <ConfidenceIndicator
            confidence={context.domain.relevance}
            size="small"
            type="badge"
            showPercentage={false}
          />
        </Space>
      }
      extra={
        interactive && showDomainDetails && (
          <Button
            type="link"
            size="small"
            onClick={() => onDomainExplore?.(context.domain.name)}
          >
            Explore Domain
          </Button>
        )
      }
    >
      <Space direction="vertical" style={{ width: '100%' }}>
        <div>
          <Text strong>{context.domain.name}</Text>
          <Paragraph style={{ marginTop: 4, marginBottom: 8 }}>
            {context.domain.description}
          </Paragraph>
        </div>

        {context.domain.concepts.length > 0 && (
          <div>
            <Text strong>Key Concepts:</Text>
            <div style={{ marginTop: 4 }}>
              <Space wrap>
                {context.domain.concepts.map((concept, index) => (
                  <Tag key={index} color="purple">{concept}</Tag>
                ))}
              </Space>
            </div>
          </div>
        )}

        {context.domain.relationships.length > 0 && (
          <div>
            <Text strong>Domain Relationships:</Text>
            <List
              size="small"
              dataSource={context.domain.relationships}
              renderItem={(relationship) => (
                <List.Item>
                  <Space>
                    <LinkOutlined />
                    <Text>{relationship}</Text>
                  </Space>
                </List.Item>
              )}
            />
          </div>
        )}
      </Space>
    </Card>
  )

  // Render business terms
  const renderBusinessTerms = () => (
    <Card 
      size="small" 
      title={
        <Space>
          <InfoCircleOutlined />
          <span>Business Terms</span>
          <Badge count={context.businessTerms.length} />
        </Space>
      }
    >
      <Space wrap>
        {context.businessTerms.map((term, index) => (
          <Tag key={index} color="cyan">{term}</Tag>
        ))}
      </Space>
    </Card>
  )

  // Render time context if available
  const renderTimeContext = () => {
    if (!context.timeContext) return null

    return (
      <Card 
        size="small" 
        title={
          <Space>
            <BulbOutlined />
            <span>Time Context</span>
          </Space>
        }
      >
        <Space direction="vertical" style={{ width: '100%' }}>
          <div>
            <Text strong>Period: </Text>
            <Tag color="orange">{context.timeContext.period}</Tag>
          </div>
          <div>
            <Text strong>Granularity: </Text>
            <Tag color="green">{context.timeContext.granularity}</Tag>
          </div>
          {context.timeContext.relativeTo && (
            <div>
              <Text strong>Relative to: </Text>
              <Tag color="blue">{context.timeContext.relativeTo}</Tag>
            </div>
          )}
        </Space>
      </Card>
    )
  }

  // Render selected entity details
  const renderEntityDetails = () => {
    if (!selectedEntity) return null

    return (
      <Card 
        size="small" 
        title={
          <Space>
            {getEntityTypeIcon(selectedEntity.type)}
            <span>Entity Details: {selectedEntity.name}</span>
          </Space>
        }
        extra={
          <Button 
            type="text" 
            size="small"
            onClick={() => setSelectedEntity(null)}
          >
            ×
          </Button>
        }
      >
        <Space direction="vertical" style={{ width: '100%' }}>
          <div>
            <Text strong>Type: </Text>
            <Tag color={getEntityTypeColor(selectedEntity.type)}>
              {selectedEntity.type}
            </Tag>
          </div>
          
          <div>
            <Text strong>Confidence: </Text>
            <ConfidenceIndicator
              confidence={selectedEntity.confidence}
              size="small"
              type="bar"
              showLabel={true}
            />
          </div>

          {selectedEntity.businessMeaning && (
            <div>
              <Text strong>Business Meaning:</Text>
              <Paragraph style={{ marginTop: 4 }}>
                {selectedEntity.businessMeaning}
              </Paragraph>
            </div>
          )}

          <div>
            <Text strong>Context:</Text>
            <Paragraph style={{ marginTop: 4 }}>
              {selectedEntity.context}
            </Paragraph>
          </div>

          {selectedEntity.relationships && selectedEntity.relationships.length > 0 && (
            <div>
              <Text strong>Relationships:</Text>
              <List
                size="small"
                dataSource={selectedEntity.relationships}
                renderItem={(rel) => (
                  <List.Item>
                    <Space>
                      <LinkOutlined />
                      <Text>{rel.relationshipType}</Text>
                      <Text type="secondary">({(rel.strength * 100).toFixed(0)}%)</Text>
                    </Space>
                  </List.Item>
                )}
              />
            </div>
          )}
        </Space>
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
          Business Context Analysis
        </Title>
        <Space>
          <ConfidenceIndicator
            confidence={context.confidence}
            size="small"
            type="badge"
          />
          {interactive && (
            <Button
              size="small"
              icon={<EyeOutlined />}
              onClick={() => setExpandedPanels(
                expandedPanels.length === 0 ? ['intent', 'entities', 'domain', 'terms'] : []
              )}
            >
              {expandedPanels.length === 0 ? 'Expand All' : 'Collapse All'}
            </Button>
          )}
        </Space>
      </div>

      {/* Main Content */}
      <Row gutter={[16, 16]}>
        <Col xs={24} lg={selectedEntity ? 16 : 24}>
          <Space direction="vertical" style={{ width: '100%' }}>
            {/* Intent Classification */}
            {renderIntentClassification()}

            {/* Business Entities */}
            {renderEntitiesGrid()}

            {/* Domain Context */}
            {showDomainDetails && renderDomainContext()}

            {/* Business Terms */}
            {context.businessTerms.length > 0 && renderBusinessTerms()}

            {/* Time Context */}
            {renderTimeContext()}
          </Space>
        </Col>

        {/* Entity Details Sidebar */}
        {selectedEntity && (
          <Col xs={24} lg={8}>
            {renderEntityDetails()}
          </Col>
        )}
      </Row>
    </div>
  )
}

export default BusinessContextVisualizer
