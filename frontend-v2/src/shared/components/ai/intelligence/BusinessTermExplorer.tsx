import React, { useState, useMemo } from 'react'
import { 
  Card, 
  Row, 
  Col, 
  Typography, 
  Space, 
  Tag, 
  Button,
  Input,
  List,
  Tree,
  Badge,
  Tooltip,
  Divider,
  Select,
  Tabs,
  Graph
} from 'antd'
import {
  BookOutlined,
  SearchOutlined,
  LinkOutlined,
  TagsOutlined,
  NodeIndexOutlined,
  BranchesOutlined,
  InfoCircleOutlined,
  ThunderboltOutlined,
  EyeOutlined,
  FilterOutlined
} from '@ant-design/icons'
// Note: Using simple visualization instead of @ant-design/plots for better compatibility
import type { BusinessTerm, TermRelationship, TermCategory } from '@shared/types/ai'

const { Title, Text, Paragraph } = Typography
const { Search } = Input
const { TreeNode } = Tree

export interface BusinessTermExplorerProps {
  terms: BusinessTerm[]
  relationships: TermRelationship[]
  categories: TermCategory[]
  selectedTerm?: BusinessTerm
  showRelationshipGraph?: boolean
  showCategoryTree?: boolean
  interactive?: boolean
  onTermSelect?: (term: BusinessTerm) => void
  onRelationshipExplore?: (relationship: TermRelationship) => void
  onCategoryFilter?: (category: string) => void
  className?: string
  testId?: string
}

/**
 * BusinessTermExplorer - Interactive business term exploration
 * 
 * Features:
 * - Comprehensive business term search and filtering
 * - Interactive relationship visualization
 * - Category-based organization
 * - Term definition and context display
 * - Synonym and related term exploration
 * - Visual relationship mapping
 */
export const BusinessTermExplorer: React.FC<BusinessTermExplorerProps> = ({
  terms,
  relationships,
  categories,
  selectedTerm,
  showRelationshipGraph = true,
  showCategoryTree = true,
  interactive = true,
  onTermSelect,
  onRelationshipExplore,
  onCategoryFilter,
  className,
  testId = 'business-term-explorer'
}) => {
  const [searchTerm, setSearchTerm] = useState('')
  const [selectedCategory, setSelectedCategory] = useState<string>('all')
  const [viewMode, setViewMode] = useState<'list' | 'tree' | 'graph'>('list')
  const [activeTab, setActiveTab] = useState('terms')

  // Filter terms based on search and category
  const filteredTerms = useMemo(() => {
    let filtered = terms

    // Filter by search term
    if (searchTerm) {
      filtered = filtered.filter(term =>
        term.term.toLowerCase().includes(searchTerm.toLowerCase()) ||
        term.definition.toLowerCase().includes(searchTerm.toLowerCase()) ||
        term.synonyms.some(synonym => 
          synonym.toLowerCase().includes(searchTerm.toLowerCase())
        )
      )
    }

    // Filter by category
    if (selectedCategory !== 'all') {
      filtered = filtered.filter(term => term.category === selectedCategory)
    }

    return filtered.sort((a, b) => a.term.localeCompare(b.term))
  }, [terms, searchTerm, selectedCategory])

  // Get category color
  const getCategoryColor = (category: string) => {
    const colors = {
      'Financial': '#1890ff',
      'Sales': '#52c41a',
      'Marketing': '#fa8c16',
      'Operations': '#722ed1',
      'Technical': '#13c2c2',
      'General': '#eb2f96'
    }
    return colors[category as keyof typeof colors] || '#d9d9d9'
  }

  // Get relationship type color
  const getRelationshipColor = (type: string) => {
    const colors = {
      'synonym': '#52c41a',
      'related': '#1890ff',
      'parent': '#fa8c16',
      'child': '#722ed1',
      'opposite': '#ff4d4f'
    }
    return colors[type as keyof typeof colors] || '#d9d9d9'
  }

  // Prepare graph data for relationship visualization
  const graphData = useMemo(() => {
    if (!selectedTerm) return { nodes: [], edges: [] }

    const nodes = [
      {
        id: selectedTerm.term,
        label: selectedTerm.term,
        type: 'main',
        style: { fill: '#1890ff', stroke: '#1890ff' }
      }
    ]

    const edges: any[] = []

    // Add related terms as nodes and edges
    const relatedTerms = relationships.filter(rel => 
      rel.fromTerm === selectedTerm.term || rel.toTerm === selectedTerm.term
    )

    relatedTerms.forEach(rel => {
      const relatedTermName = rel.fromTerm === selectedTerm.term ? rel.toTerm : rel.fromTerm
      const relatedTerm = terms.find(t => t.term === relatedTermName)

      if (relatedTerm) {
        nodes.push({
          id: relatedTerm.term,
          label: relatedTerm.term,
          type: 'related',
          style: { 
            fill: getCategoryColor(relatedTerm.category), 
            stroke: getCategoryColor(relatedTerm.category) 
          }
        })

        edges.push({
          source: rel.fromTerm,
          target: rel.toTerm,
          label: rel.relationshipType,
          style: { stroke: getRelationshipColor(rel.relationshipType) }
        })
      }
    })

    return { nodes, edges }
  }, [selectedTerm, relationships, terms])

  // Handle term selection
  const handleTermSelect = (term: BusinessTerm) => {
    onTermSelect?.(term)
  }

  // Render search and filters
  const renderSearchAndFilters = () => (
    <Card size="small">
      <Space direction="vertical" style={{ width: '100%' }}>
        <div style={{ display: 'flex', gap: 16, alignItems: 'center' }}>
          <Search
            placeholder="Search terms, definitions, or synonyms..."
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
            style={{ flex: 1 }}
            allowClear
          />
          <Select
            value={selectedCategory}
            onChange={setSelectedCategory}
            style={{ width: 150 }}
          >
            <Select.Option value="all">All Categories</Select.Option>
            {categories.map(category => (
              <Select.Option key={category.name} value={category.name}>
                <Space>
                  <div 
                    style={{ 
                      width: 8, 
                      height: 8, 
                      backgroundColor: getCategoryColor(category.name),
                      borderRadius: '50%' 
                    }} 
                  />
                  {category.name}
                </Space>
              </Select.Option>
            ))}
          </Select>
          <Select
            value={viewMode}
            onChange={setViewMode}
            style={{ width: 100 }}
          >
            <Select.Option value="list">List</Select.Option>
            <Select.Option value="tree">Tree</Select.Option>
            <Select.Option value="graph">Graph</Select.Option>
          </Select>
        </div>
        
        <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
          <Space wrap>
            <Text type="secondary">
              {filteredTerms.length} of {terms.length} terms
            </Text>
            {selectedCategory !== 'all' && (
              <Tag color={getCategoryColor(selectedCategory)}>
                {selectedCategory}
              </Tag>
            )}
          </Space>
          
          <Space wrap>
            {categories.map(category => {
              const count = terms.filter(t => t.category === category.name).length
              return (
                <Tag
                  key={category.name}
                  color={selectedCategory === category.name ? getCategoryColor(category.name) : 'default'}
                  style={{ cursor: 'pointer' }}
                  onClick={() => setSelectedCategory(
                    selectedCategory === category.name ? 'all' : category.name
                  )}
                >
                  {category.name} ({count})
                </Tag>
              )
            })}
          </Space>
        </div>
      </Space>
    </Card>
  )

  // Render terms list
  const renderTermsList = () => (
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
              cursor: interactive ? 'pointer' : 'default',
              backgroundColor: selectedTerm?.term === term.term ? '#f0f8ff' : 'transparent',
              borderRadius: 4,
              padding: 8
            }}
            onClick={() => interactive && handleTermSelect(term)}
          >
            <List.Item.Meta
              title={
                <Space>
                  <Text strong>{term.term}</Text>
                  <Tag color={getCategoryColor(term.category)}>{term.category}</Tag>
                </Space>
              }
              description={
                <div>
                  <Paragraph 
                    ellipsis={{ rows: 2, expandable: true }}
                    style={{ marginBottom: 8 }}
                  >
                    {term.definition}
                  </Paragraph>
                  
                  {term.synonyms.length > 0 && (
                    <div style={{ marginBottom: 4 }}>
                      <Text type="secondary" style={{ fontSize: '12px' }}>
                        Synonyms: 
                      </Text>
                      <Space wrap style={{ marginLeft: 4 }}>
                        {term.synonyms.map((synonym, index) => (
                          <Tag key={index} size="small" color="blue">
                            {synonym}
                          </Tag>
                        ))}
                      </Space>
                    </div>
                  )}
                  
                  {term.relatedTerms.length > 0 && (
                    <div>
                      <Text type="secondary" style={{ fontSize: '12px' }}>
                        Related: {term.relatedTerms.slice(0, 3).join(', ')}
                        {term.relatedTerms.length > 3 && '...'}
                      </Text>
                    </div>
                  )}
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

  // Render term details
  const renderTermDetails = () => {
    if (!selectedTerm) return null

    const termRelationships = relationships.filter(rel => 
      rel.fromTerm === selectedTerm.term || rel.toTerm === selectedTerm.term
    )

    return (
      <Card 
        size="small" 
        title={
          <Space>
            <InfoCircleOutlined />
            <span>Term Details: {selectedTerm.term}</span>
          </Space>
        }
      >
        <Space direction="vertical" style={{ width: '100%' }}>
          {/* Basic Info */}
          <div>
            <div style={{ display: 'flex', justifyContent: 'space-between', marginBottom: 8 }}>
              <Tag color={getCategoryColor(selectedTerm.category)}>
                {selectedTerm.category}
              </Tag>
            </div>
            <Paragraph>{selectedTerm.definition}</Paragraph>
          </div>

          {/* Synonyms */}
          {selectedTerm.synonyms.length > 0 && (
            <div>
              <Text strong>Synonyms:</Text>
              <div style={{ marginTop: 4 }}>
                <Space wrap>
                  {selectedTerm.synonyms.map((synonym, index) => (
                    <Tag key={index} color="blue">{synonym}</Tag>
                  ))}
                </Space>
              </div>
            </div>
          )}

          {/* Related Terms */}
          {selectedTerm.relatedTerms.length > 0 && (
            <div>
              <Text strong>Related Terms:</Text>
              <div style={{ marginTop: 4 }}>
                <Space wrap>
                  {selectedTerm.relatedTerms.map((relatedTerm, index) => (
                    <Tag 
                      key={index} 
                      color="green"
                      style={{ cursor: 'pointer' }}
                      onClick={() => {
                        const term = terms.find(t => t.term === relatedTerm)
                        if (term) handleTermSelect(term)
                      }}
                    >
                      {relatedTerm}
                    </Tag>
                  ))}
                </Space>
              </div>
            </div>
          )}

          {/* Relationships */}
          {termRelationships.length > 0 && (
            <div>
              <Text strong>Relationships:</Text>
              <List
                size="small"
                dataSource={termRelationships}
                style={{ marginTop: 8 }}
                renderItem={(relationship) => (
                  <List.Item>
                    <Space>
                      <LinkOutlined style={{ color: getRelationshipColor(relationship.relationshipType) }} />
                      <Text>{relationship.relationshipType}</Text>
                      <Text type="secondary">
                        {relationship.fromTerm === selectedTerm.term ? 
                          `→ ${relationship.toTerm}` : 
                          `← ${relationship.fromTerm}`
                        }
                      </Text>
                      {relationship.strength && (
                        <Text type="secondary">
                          ({(relationship.strength * 100).toFixed(0)}%)
                        </Text>
                      )}
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

  // Render relationship graph (simplified version)
  const renderRelationshipGraph = () => {
    if (!showRelationshipGraph || !selectedTerm) return null

    const termRelationships = relationships.filter(rel =>
      rel.fromTerm === selectedTerm.term || rel.toTerm === selectedTerm.term
    )

    return (
      <Card
        size="small"
        title={
          <Space>
            <BranchesOutlined />
            <span>Relationship Network</span>
          </Space>
        }
      >
        <div style={{
          height: 300,
          display: 'flex',
          flexDirection: 'column',
          alignItems: 'center',
          justifyContent: 'center',
          position: 'relative',
          backgroundColor: '#fafafa',
          borderRadius: 6,
          padding: 20
        }}>
          {/* Central node */}
          <div style={{
            position: 'absolute',
            top: '50%',
            left: '50%',
            transform: 'translate(-50%, -50%)',
            width: 80,
            height: 80,
            borderRadius: '50%',
            backgroundColor: '#1890ff',
            color: 'white',
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'center',
            fontWeight: 'bold',
            fontSize: '12px',
            textAlign: 'center',
            zIndex: 2,
            boxShadow: '0 2px 8px rgba(0,0,0,0.15)'
          }}>
            {selectedTerm.term}
          </div>

          {/* Related nodes */}
          {termRelationships.slice(0, 6).map((rel, index) => {
            const relatedTermName = rel.fromTerm === selectedTerm.term ? rel.toTerm : rel.fromTerm
            const relatedTerm = terms.find(t => t.term === relatedTermName)
            const angle = (index * 60) * (Math.PI / 180) // 60 degrees apart
            const radius = 100
            const x = Math.cos(angle) * radius
            const y = Math.sin(angle) * radius

            return (
              <div key={index}>
                {/* Connection line */}
                <div style={{
                  position: 'absolute',
                  top: '50%',
                  left: '50%',
                  width: radius,
                  height: 2,
                  backgroundColor: getRelationshipColor(rel.relationshipType),
                  transformOrigin: '0 50%',
                  transform: `translate(0, -50%) rotate(${angle}rad)`,
                  zIndex: 1,
                  opacity: 0.6
                }} />

                {/* Related node */}
                <div
                  style={{
                    position: 'absolute',
                    top: `calc(50% + ${y}px)`,
                    left: `calc(50% + ${x}px)`,
                    transform: 'translate(-50%, -50%)',
                    width: 60,
                    height: 60,
                    borderRadius: '50%',
                    backgroundColor: relatedTerm ? getCategoryColor(relatedTerm.category) : '#d9d9d9',
                    color: 'white',
                    display: 'flex',
                    alignItems: 'center',
                    justifyContent: 'center',
                    fontSize: '10px',
                    textAlign: 'center',
                    cursor: 'pointer',
                    zIndex: 2,
                    boxShadow: '0 2px 4px rgba(0,0,0,0.1)',
                    transition: 'transform 0.2s'
                  }}
                  onClick={() => {
                    if (relatedTerm) handleTermSelect(relatedTerm)
                  }}
                  onMouseEnter={(e) => {
                    e.currentTarget.style.transform = 'translate(-50%, -50%) scale(1.1)'
                  }}
                  onMouseLeave={(e) => {
                    e.currentTarget.style.transform = 'translate(-50%, -50%) scale(1)'
                  }}
                  title={`${relatedTermName} (${rel.relationshipType})`}
                >
                  {relatedTermName.length > 8 ? relatedTermName.substring(0, 8) + '...' : relatedTermName}
                </div>
              </div>
            )
          })}

          {/* Legend */}
          <div style={{
            position: 'absolute',
            bottom: 10,
            left: 10,
            fontSize: '11px',
            color: '#666'
          }}>
            <Space wrap>
              <Text type="secondary">Relationships:</Text>
              {['synonym', 'related', 'parent', 'child'].map(type => (
                <Space key={type} size={4}>
                  <div style={{
                    width: 12,
                    height: 2,
                    backgroundColor: getRelationshipColor(type)
                  }} />
                  <Text type="secondary">{type}</Text>
                </Space>
              ))}
            </Space>
          </div>
        </div>
      </Card>
    )
  }

  const tabItems = [
    {
      key: 'terms',
      label: (
        <Space>
          <BookOutlined />
          <span>Terms</span>
          <Badge count={filteredTerms.length} size="small" />
        </Space>
      ),
      children: renderTermsList()
    },
    {
      key: 'details',
      label: (
        <Space>
          <InfoCircleOutlined />
          <span>Details</span>
        </Space>
      ),
      children: renderTermDetails(),
      disabled: !selectedTerm
    },
    {
      key: 'relationships',
      label: (
        <Space>
          <BranchesOutlined />
          <span>Relationships</span>
        </Space>
      ),
      children: renderRelationshipGraph(),
      disabled: !selectedTerm || !showRelationshipGraph
    }
  ]

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
          Business Term Explorer
        </Title>
        <Space>
          <Text type="secondary">
            {terms.length} terms • {categories.length} categories
          </Text>
        </Space>
      </div>

      {/* Search and Filters */}
      {renderSearchAndFilters()}

      {/* Main Content */}
      <div style={{ marginTop: 16 }}>
        <Tabs
          activeKey={activeTab}
          onChange={setActiveTab}
          items={tabItems}
        />
      </div>
    </div>
  )
}

export default BusinessTermExplorer
