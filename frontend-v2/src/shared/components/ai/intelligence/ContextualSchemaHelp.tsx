import React, { useState, useMemo } from 'react'
import { 
  Card, 
  Typography, 
  Space, 
  Tag, 
  Button,
  Tooltip,
  Collapse,
  List,
  Input,
  Tree,
  Badge,
  Popover,
  Divider,
  Alert
} from 'antd'
import {
  DatabaseOutlined,
  TableOutlined,
  InfoCircleOutlined,
  SearchOutlined,
  BookOutlined,
  BulbOutlined,
  LinkOutlined,
  QuestionCircleOutlined,
  ThunderboltOutlined,
  TagsOutlined,
  EyeOutlined
} from '@ant-design/icons'
import { ConfidenceIndicator } from '../common/ConfidenceIndicator'
import type { SchemaContext, BusinessTerm, TableMetadata, ColumnMetadata } from '@shared/types/ai'

const { Title, Text, Paragraph } = Typography
const { Panel } = Collapse
const { Search } = Input
const { TreeNode } = Tree

export interface ContextualSchemaHelpProps {
  schemaContext: SchemaContext
  currentQuery?: string
  showBusinessTerms?: boolean
  showRelationships?: boolean
  showExamples?: boolean
  interactive?: boolean
  onTableSelect?: (table: TableMetadata) => void
  onColumnSelect?: (column: ColumnMetadata) => void
  onTermExplore?: (term: BusinessTerm) => void
  className?: string
  testId?: string
}

/**
 * ContextualSchemaHelp - AI-powered contextual schema assistance
 * 
 * Features:
 * - Contextual schema explanations based on current query
 * - Business term definitions and relationships
 * - Interactive table and column exploration
 * - AI-suggested schema improvements
 * - Query-relevant schema highlighting
 * - Business context integration
 */
export const ContextualSchemaHelp: React.FC<ContextualSchemaHelpProps> = ({
  schemaContext,
  currentQuery,
  showBusinessTerms = true,
  showRelationships = true,
  showExamples = true,
  interactive = true,
  onTableSelect,
  onColumnSelect,
  onTermExplore,
  className,
  testId = 'contextual-schema-help'
}) => {
  const [searchTerm, setSearchTerm] = useState('')
  const [selectedTable, setSelectedTable] = useState<TableMetadata | null>(null)
  const [expandedPanels, setExpandedPanels] = useState<string[]>(['relevant'])
  const [viewMode, setViewMode] = useState<'list' | 'tree' | 'graph'>('list')

  // Filter relevant tables based on query context
  const relevantTables = useMemo(() => {
    return schemaContext.tables.filter(table => 
      table.relevanceScore > 0.3 || 
      (currentQuery && table.name.toLowerCase().includes(searchTerm.toLowerCase()))
    ).sort((a, b) => b.relevanceScore - a.relevanceScore)
  }, [schemaContext.tables, currentQuery, searchTerm])

  // Filter business terms
  const filteredTerms = useMemo(() => {
    if (!searchTerm) return schemaContext.businessTerms
    return schemaContext.businessTerms.filter(term =>
      term.term.toLowerCase().includes(searchTerm.toLowerCase()) ||
      term.definition.toLowerCase().includes(searchTerm.toLowerCase())
    )
  }, [schemaContext.businessTerms, searchTerm])

  // Get relevance color
  const getRelevanceColor = (score: number) => {
    if (score >= 0.8) return '#52c41a'
    if (score >= 0.6) return '#faad14'
    if (score >= 0.4) return '#ff7875'
    return '#d9d9d9'
  }

  // Get column type icon
  const getColumnTypeIcon = (type: string) => {
    const icons = {
      string: <TagsOutlined />,
      number: <ThunderboltOutlined />,
      date: <BulbOutlined />,
      boolean: <QuestionCircleOutlined />,
      json: <DatabaseOutlined />
    }
    return icons[type as keyof typeof icons] || <InfoCircleOutlined />
  }

  // Handle table selection
  const handleTableSelect = (table: TableMetadata) => {
    setSelectedTable(table)
    onTableSelect?.(table)
  }

  // Render query context alert
  const renderQueryContext = () => {
    if (!currentQuery) return null

    return (
      <Alert
        message="Query Context Analysis"
        description={
          <div>
            <Text>AI has analyzed your query and highlighted the most relevant schema elements below.</Text>
            {schemaContext.suggestions.length > 0 && (
              <div style={{ marginTop: 8 }}>
                <Text strong>Suggestions: </Text>
                <Space wrap>
                  {schemaContext.suggestions.slice(0, 3).map((suggestion, index) => (
                    <Tag key={index} color="blue">{suggestion}</Tag>
                  ))}
                </Space>
              </div>
            )}
          </div>
        }
        type="info"
        showIcon
        style={{ marginBottom: 16 }}
      />
    )
  }

  // Render relevant tables
  const renderRelevantTables = () => (
    <Card 
      size="small" 
      title={
        <Space>
          <TableOutlined />
          <span>Relevant Tables</span>
          <Badge count={relevantTables.length} />
        </Space>
      }
      extra={
        <Search
          placeholder="Search tables..."
          value={searchTerm}
          onChange={(e) => setSearchTerm(e.target.value)}
          style={{ width: 200 }}
          size="small"
        />
      }
    >
      <List
        size="small"
        dataSource={relevantTables}
        renderItem={(table) => (
          <List.Item
            style={{ 
              cursor: interactive ? 'pointer' : 'default',
              backgroundColor: selectedTable?.name === table.name ? '#f0f8ff' : 'transparent',
              borderRadius: 4,
              padding: 8
            }}
            onClick={() => interactive && handleTableSelect(table)}
          >
            <List.Item.Meta
              avatar={
                <div style={{ display: 'flex', alignItems: 'center' }}>
                  <DatabaseOutlined style={{ color: getRelevanceColor(table.relevanceScore) }} />
                  <ConfidenceIndicator
                    confidence={table.relevanceScore}
                    size="small"
                    type="circle"
                    showPercentage={false}
                    style={{ marginLeft: 8 }}
                  />
                </div>
              }
              title={
                <Space>
                  <Text strong>{table.name}</Text>
                  {table.businessPurpose && (
                    <Tag color="purple">{table.businessPurpose}</Tag>
                  )}
                </Space>
              }
              description={
                <div>
                  <Text style={{ fontSize: '13px' }}>{table.description}</Text>
                  <div style={{ marginTop: 4 }}>
                    <Space>
                      <Text type="secondary" style={{ fontSize: '12px' }}>
                        {table.columns.length} columns
                      </Text>
                      {table.estimatedRowCount && (
                        <Text type="secondary" style={{ fontSize: '12px' }}>
                          ~{table.estimatedRowCount.toLocaleString()} rows
                        </Text>
                      )}
                      {table.lastUpdated && (
                        <Text type="secondary" style={{ fontSize: '12px' }}>
                          Updated {new Date(table.lastUpdated).toLocaleDateString()}
                        </Text>
                      )}
                    </Space>
                  </div>
                </div>
              }
            />
            {interactive && (
              <Button type="text" size="small">
                Explore
              </Button>
            )}
          </List.Item>
        )}
      />
    </Card>
  )

  // Render table details
  const renderTableDetails = () => {
    if (!selectedTable) return null

    return (
      <Card 
        size="small" 
        title={
          <Space>
            <DatabaseOutlined />
            <span>Table Details: {selectedTable.name}</span>
          </Space>
        }
        extra={
          <Button 
            type="text" 
            size="small"
            onClick={() => setSelectedTable(null)}
          >
            ×
          </Button>
        }
      >
        <Space direction="vertical" style={{ width: '100%' }}>
          {/* Table Info */}
          <div>
            <Text strong>Description:</Text>
            <Paragraph style={{ marginTop: 4 }}>
              {selectedTable.description}
            </Paragraph>
          </div>

          {selectedTable.businessPurpose && (
            <div>
              <Text strong>Business Purpose:</Text>
              <Paragraph style={{ marginTop: 4 }}>
                {selectedTable.businessPurpose}
              </Paragraph>
            </div>
          )}

          {/* Key Columns */}
          {selectedTable.keyColumns.length > 0 && (
            <div>
              <Text strong>Key Columns:</Text>
              <div style={{ marginTop: 4 }}>
                <Space wrap>
                  {selectedTable.keyColumns.map((column, index) => (
                    <Tag key={index} color="gold">{column}</Tag>
                  ))}
                </Space>
              </div>
            </div>
          )}

          {/* Columns */}
          <div>
            <Text strong>Columns ({selectedTable.columns.length}):</Text>
            <List
              size="small"
              dataSource={selectedTable.columns}
              style={{ marginTop: 8 }}
              renderItem={(column) => (
                <List.Item
                  style={{ 
                    cursor: interactive ? 'pointer' : 'default',
                    padding: '4px 8px',
                    borderRadius: 4
                  }}
                  onClick={() => interactive && onColumnSelect?.(column)}
                >
                  <Space>
                    {getColumnTypeIcon(column.type)}
                    <Text>{column.name}</Text>
                    <Tag size="small" color="blue">{column.type}</Tag>
                    {column.businessMeaning && (
                      <Tooltip title={column.businessMeaning}>
                        <InfoCircleOutlined style={{ color: '#1890ff' }} />
                      </Tooltip>
                    )}
                  </Space>
                </List.Item>
              )}
            />
          </div>

          {/* Relationships */}
          {showRelationships && selectedTable.relationships.length > 0 && (
            <div>
              <Text strong>Relationships:</Text>
              <List
                size="small"
                dataSource={selectedTable.relationships}
                style={{ marginTop: 8 }}
                renderItem={(relationship) => (
                  <List.Item>
                    <Space>
                      <LinkOutlined />
                      <Text>{relationship.relationshipType}</Text>
                      <Text type="secondary">({(relationship.strength * 100).toFixed(0)}%)</Text>
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

  // Render business terms
  const renderBusinessTerms = () => {
    if (!showBusinessTerms || filteredTerms.length === 0) return null

    return (
      <Card 
        size="small" 
        title={
          <Space>
            <BookOutlined />
            <span>Business Terms</span>
            <Badge count={filteredTerms.length} />
          </Space>
        }
      >
        <List
          size="small"
          dataSource={filteredTerms}
          renderItem={(term) => (
            <List.Item
              style={{ 
                cursor: interactive ? 'pointer' : 'default'
              }}
              onClick={() => interactive && onTermExplore?.(term)}
            >
              <List.Item.Meta
                title={
                  <Space>
                    <Text strong>{term.term}</Text>
                    <Tag color="cyan">{term.category}</Tag>
                  </Space>
                }
                description={
                  <div>
                    <Text style={{ fontSize: '13px' }}>{term.definition}</Text>
                    {term.synonyms.length > 0 && (
                      <div style={{ marginTop: 4 }}>
                        <Text type="secondary" style={{ fontSize: '12px' }}>
                          Synonyms: {term.synonyms.join(', ')}
                        </Text>
                      </div>
                    )}
                    {term.relatedTerms.length > 0 && (
                      <div style={{ marginTop: 2 }}>
                        <Text type="secondary" style={{ fontSize: '12px' }}>
                          Related: {term.relatedTerms.slice(0, 3).join(', ')}
                          {term.relatedTerms.length > 3 && '...'}
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

  // Render schema suggestions
  const renderSchemaSuggestions = () => {
    if (schemaContext.suggestions.length === 0) return null

    return (
      <Card 
        size="small" 
        title={
          <Space>
            <BulbOutlined />
            <span>AI Suggestions</span>
          </Space>
        }
      >
        <List
          size="small"
          dataSource={schemaContext.suggestions}
          renderItem={(suggestion) => (
            <List.Item>
              <Space>
                <BulbOutlined style={{ color: '#faad14' }} />
                <Text>{suggestion}</Text>
              </Space>
            </List.Item>
          )}
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
          Schema Context Help
        </Title>
        <Space>
          <ConfidenceIndicator
            confidence={schemaContext.confidence}
            size="small"
            type="badge"
          />
          {interactive && (
            <Button
              size="small"
              icon={<EyeOutlined />}
              onClick={() => setExpandedPanels(
                expandedPanels.length === 0 ? ['relevant', 'terms', 'suggestions'] : []
              )}
            >
              {expandedPanels.length === 0 ? 'Expand All' : 'Collapse All'}
            </Button>
          )}
        </Space>
      </div>

      {/* Query Context */}
      {renderQueryContext()}

      {/* Main Content */}
      <Space direction="vertical" style={{ width: '100%' }}>
        {/* Relevant Tables */}
        {renderRelevantTables()}

        {/* Table Details */}
        {renderTableDetails()}

        {/* Business Terms */}
        {renderBusinessTerms()}

        {/* Schema Suggestions */}
        {renderSchemaSuggestions()}
      </Space>
    </div>
  )
}

export default ContextualSchemaHelp
