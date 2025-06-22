import React, { useState, useMemo } from 'react'
import { 
  Card, 
  Tree, 
  Typography, 
  Space, 
  Tag, 
  Button,
  Input,
  Tooltip,
  Badge,
  List,
  Collapse,
  Alert,
  Row,
  Col,
  Progress,
  Divider
} from 'antd'
import {
  DatabaseOutlined,
  TableOutlined,
  SearchOutlined,
  BulbOutlined,
  LinkOutlined,
  InfoCircleOutlined,
  ThunderboltOutlined,
  EyeOutlined,
  StarOutlined,
  RobotOutlined
} from '@ant-design/icons'
import { ConfidenceIndicator } from '../common/ConfidenceIndicator'
import { useGetSchemaNavigationQuery, useGetSchemaRecommendationsQuery } from '@shared/store/api/intelligentAgentsApi'
import type { SchemaNode, SchemaRecommendation, TableRelationship } from '@shared/types/intelligentAgents'

const { Title, Text, Paragraph } = Typography
const { Search } = Input
const { Panel } = Collapse
const { TreeNode } = Tree

export interface AISchemaExplorerProps {
  currentQuery?: string
  showRecommendations?: boolean
  showRelationships?: boolean
  interactive?: boolean
  onTableSelect?: (table: SchemaNode) => void
  onColumnSelect?: (column: SchemaNode) => void
  onRecommendationApply?: (recommendation: SchemaRecommendation) => void
  className?: string
  testId?: string
}

/**
 * AISchemaExplorer - AI-powered schema discovery and exploration
 * 
 * Features:
 * - Intelligent schema navigation with AI recommendations
 * - Context-aware table and column suggestions
 * - Relationship visualization and discovery
 * - Query-relevant schema highlighting
 * - Business context integration
 * - Interactive exploration with confidence scoring
 */
export const AISchemaExplorer: React.FC<AISchemaExplorerProps> = ({
  currentQuery,
  showRecommendations = true,
  showRelationships = true,
  interactive = true,
  onTableSelect,
  onColumnSelect,
  onRecommendationApply,
  className,
  testId = 'ai-schema-explorer'
}) => {
  const [searchTerm, setSearchTerm] = useState('')
  const [selectedNode, setSelectedNode] = useState<SchemaNode | null>(null)
  const [expandedKeys, setExpandedKeys] = useState<string[]>([])

  // Real API data
  const { data: schemaNavigation, isLoading: navigationLoading, refetch: refetchNavigation } = 
    useGetSchemaNavigationQuery({ query: currentQuery })
  
  const { data: recommendations, isLoading: recommendationsLoading } = 
    useGetSchemaRecommendationsQuery({ 
      query: currentQuery,
      context: selectedNode?.name 
    })

  // Process schema data for tree display
  const treeData = useMemo(() => {
    if (!schemaNavigation?.schemas) return []
    
    return schemaNavigation.schemas.map(schema => ({
      title: (
        <Space>
          <DatabaseOutlined />
          <Text strong>{schema.name}</Text>
          <Badge count={schema.tables.length} size="small" />
        </Space>
      ),
      key: `schema-${schema.name}`,
      children: schema.tables
        .filter(table => 
          !searchTerm || 
          table.name.toLowerCase().includes(searchTerm.toLowerCase()) ||
          table.businessPurpose?.toLowerCase().includes(searchTerm.toLowerCase())
        )
        .map(table => ({
          title: (
            <Space>
              <TableOutlined />
              <Text>{table.name}</Text>
              {table.relevanceScore > 0.8 && <StarOutlined style={{ color: '#faad14' }} />}
              <ConfidenceIndicator
                confidence={table.relevanceScore}
                size="small"
                type="circle"
                showPercentage={false}
              />
            </Space>
          ),
          key: `table-${table.name}`,
          isLeaf: false,
          children: table.columns.map(column => ({
            title: (
              <Space>
                <InfoCircleOutlined />
                <Text>{column.name}</Text>
                <Tag size="small" color="blue">{column.type}</Tag>
                {column.isPrimaryKey && <Tag size="small" color="gold">PK</Tag>}
                {column.isForeignKey && <Tag size="small" color="purple">FK</Tag>}
              </Space>
            ),
            key: `column-${table.name}-${column.name}`,
            isLeaf: true,
            data: { ...column, tableName: table.name }
          })),
          data: table
        }))
    }))
  }, [schemaNavigation, searchTerm])

  // Handle node selection
  const handleNodeSelect = (selectedKeys: React.Key[], info: any) => {
    const key = selectedKeys[0] as string
    const node = info.node
    
    if (key?.startsWith('table-')) {
      setSelectedNode(node.data)
      onTableSelect?.(node.data)
    } else if (key?.startsWith('column-')) {
      setSelectedNode(node.data)
      onColumnSelect?.(node.data)
    }
  }

  // Handle recommendation application
  const handleApplyRecommendation = (recommendation: SchemaRecommendation) => {
    onRecommendationApply?.(recommendation)
  }

  // Get relevance color
  const getRelevanceColor = (score: number) => {
    if (score >= 0.8) return '#52c41a'
    if (score >= 0.6) return '#faad14'
    if (score >= 0.4) return '#ff7875'
    return '#d9d9d9'
  }

  // Render schema recommendations
  const renderRecommendations = () => {
    if (!showRecommendations || !recommendations?.recommendations.length) return null

    return (
      <Card 
        title={
          <Space>
            <BulbOutlined />
            <span>AI Recommendations</span>
            <Badge count={recommendations.recommendations.length} />
          </Space>
        }
        size="small"
        style={{ marginBottom: 16 }}
      >
        <List
          size="small"
          dataSource={recommendations.recommendations}
          renderItem={(recommendation) => (
            <List.Item
              actions={interactive ? [
                <Button 
                  type="link" 
                  size="small"
                  onClick={() => handleApplyRecommendation(recommendation)}
                >
                  Apply
                </Button>
              ] : []}
            >
              <List.Item.Meta
                avatar={
                  <div style={{ display: 'flex', alignItems: 'center' }}>
                    <RobotOutlined style={{ color: '#1890ff' }} />
                    <ConfidenceIndicator
                      confidence={recommendation.confidence}
                      size="small"
                      type="circle"
                      showPercentage={false}
                      style={{ marginLeft: 8 }}
                    />
                  </div>
                }
                title={
                  <Space>
                    <Text strong>{recommendation.title}</Text>
                    <Tag color={recommendation.type === 'join' ? 'blue' : recommendation.type === 'filter' ? 'green' : 'orange'}>
                      {recommendation.type.toUpperCase()}
                    </Tag>
                  </Space>
                }
                description={
                  <div>
                    <Paragraph style={{ marginBottom: 4, fontSize: '13px' }}>
                      {recommendation.description}
                    </Paragraph>
                    {recommendation.reasoning && (
                      <Text type="secondary" style={{ fontSize: '12px', fontStyle: 'italic' }}>
                        Reasoning: {recommendation.reasoning}
                      </Text>
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

  // Render selected node details
  const renderNodeDetails = () => {
    if (!selectedNode) return null

    const isTable = 'columns' in selectedNode
    
    return (
      <Card 
        title={
          <Space>
            {isTable ? <TableOutlined /> : <InfoCircleOutlined />}
            <span>{isTable ? 'Table' : 'Column'} Details</span>
          </Space>
        }
        size="small"
      >
        <Space direction="vertical" style={{ width: '100%' }}>
          <div>
            <Text strong>Name: </Text>
            <Text>{selectedNode.name}</Text>
          </div>
          
          {selectedNode.description && (
            <div>
              <Text strong>Description: </Text>
              <Paragraph style={{ marginBottom: 8 }}>
                {selectedNode.description}
              </Paragraph>
            </div>
          )}

          {selectedNode.businessPurpose && (
            <div>
              <Text strong>Business Purpose: </Text>
              <Paragraph style={{ marginBottom: 8 }}>
                {selectedNode.businessPurpose}
              </Paragraph>
            </div>
          )}

          <div>
            <Text strong>Relevance: </Text>
            <ConfidenceIndicator
              confidence={selectedNode.relevanceScore}
              size="small"
              type="bar"
              showLabel={true}
            />
          </div>

          {isTable && selectedNode.estimatedRowCount && (
            <div>
              <Text strong>Estimated Rows: </Text>
              <Text>{selectedNode.estimatedRowCount.toLocaleString()}</Text>
            </div>
          )}

          {!isTable && selectedNode.type && (
            <div>
              <Text strong>Data Type: </Text>
              <Tag color="blue">{selectedNode.type}</Tag>
            </div>
          )}

          {showRelationships && selectedNode.relationships && selectedNode.relationships.length > 0 && (
            <div>
              <Text strong>Relationships: </Text>
              <div style={{ marginTop: 4 }}>
                {selectedNode.relationships.map((rel, index) => (
                  <Tag key={index} color="purple" style={{ marginBottom: 4 }}>
                    <LinkOutlined /> {rel.relatedTable}
                  </Tag>
                ))}
              </div>
            </div>
          )}
        </Space>
      </Card>
    )
  }

  // Render relationship insights
  const renderRelationshipInsights = () => {
    if (!showRelationships || !schemaNavigation?.relationships?.length) return null

    return (
      <Card 
        title={
          <Space>
            <LinkOutlined />
            <span>Relationship Insights</span>
          </Space>
        }
        size="small"
      >
        <Collapse size="small">
          <Panel header="Discovered Relationships" key="relationships">
            <List
              size="small"
              dataSource={schemaNavigation.relationships}
              renderItem={(relationship) => (
                <List.Item>
                  <Space>
                    <Text>{relationship.fromTable}</Text>
                    <LinkOutlined />
                    <Text>{relationship.toTable}</Text>
                    <Tag color="blue">{relationship.relationshipType}</Tag>
                    <ConfidenceIndicator
                      confidence={relationship.confidence}
                      size="small"
                      type="badge"
                      showPercentage={false}
                    />
                  </Space>
                </List.Item>
              )}
            />
          </Panel>
        </Collapse>
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
          AI Schema Explorer
        </Title>
        <Space>
          <Button 
            icon={<SearchOutlined />}
            loading={navigationLoading}
            onClick={() => refetchNavigation()}
          >
            Refresh
          </Button>
        </Space>
      </div>

      {/* Query Context */}
      {currentQuery && (
        <Alert
          message="Query Context Analysis"
          description={`AI is analyzing your query: "${currentQuery}" to provide relevant schema recommendations.`}
          type="info"
          showIcon
          style={{ marginBottom: 16 }}
        />
      )}

      {/* Search */}
      <Search
        placeholder="Search tables, columns, or business purposes..."
        value={searchTerm}
        onChange={(e) => setSearchTerm(e.target.value)}
        style={{ marginBottom: 16 }}
        allowClear
      />

      {/* Main Content */}
      <Row gutter={[16, 16]}>
        <Col xs={24} lg={selectedNode ? 12 : 16}>
          {/* Schema Tree */}
          <Card 
            title={
              <Space>
                <DatabaseOutlined />
                <span>Schema Navigation</span>
                {schemaNavigation?.schemas && (
                  <Badge count={schemaNavigation.schemas.reduce((acc, s) => acc + s.tables.length, 0)} />
                )}
              </Space>
            }
            size="small"
            loading={navigationLoading}
          >
            <Tree
              treeData={treeData}
              onSelect={handleNodeSelect}
              expandedKeys={expandedKeys}
              onExpand={setExpandedKeys}
              showLine
              showIcon
              height={400}
              style={{ overflow: 'auto' }}
            />
          </Card>

          {/* Recommendations */}
          {renderRecommendations()}

          {/* Relationship Insights */}
          {renderRelationshipInsights()}
        </Col>

        {/* Node Details Sidebar */}
        {selectedNode && (
          <Col xs={24} lg={12}>
            {renderNodeDetails()}
          </Col>
        )}

        {/* Schema Statistics */}
        {!selectedNode && (
          <Col xs={24} lg={8}>
            <Card title="Schema Statistics" size="small">
              <Space direction="vertical" style={{ width: '100%' }}>
                <div>
                  <Text strong>Total Tables: </Text>
                  <Text>{schemaNavigation?.schemas?.reduce((acc, s) => acc + s.tables.length, 0) || 0}</Text>
                </div>
                <div>
                  <Text strong>Relevant Tables: </Text>
                  <Text>
                    {schemaNavigation?.schemas?.reduce((acc, s) => 
                      acc + s.tables.filter(t => t.relevanceScore > 0.6).length, 0
                    ) || 0}
                  </Text>
                </div>
                <div>
                  <Text strong>Relationships: </Text>
                  <Text>{schemaNavigation?.relationships?.length || 0}</Text>
                </div>
                <div>
                  <Text strong>AI Confidence: </Text>
                  <ConfidenceIndicator
                    confidence={schemaNavigation?.confidence || 0}
                    size="small"
                    type="bar"
                    showLabel={true}
                  />
                </div>
              </Space>
            </Card>
          </Col>
        )}
      </Row>
    </div>
  )
}

export default AISchemaExplorer
