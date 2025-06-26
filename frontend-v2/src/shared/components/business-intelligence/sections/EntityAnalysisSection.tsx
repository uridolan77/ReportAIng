import React from 'react'
import { Card, Space, Typography, Tag, List, Avatar, Button, Alert, Row, Col, Progress } from 'antd'
import { TagsOutlined, DatabaseOutlined, LinkOutlined } from '@ant-design/icons'
import type { BusinessContextProfile } from '@shared/types/business-intelligence'

const { Text, Title } = Typography

interface EntityAnalysisSectionProps {
  context: BusinessContextProfile
  interactive?: boolean
  showEntityRelationships?: boolean
  onEntityClick?: (entityId: string) => void
}

export const EntityAnalysisSection: React.FC<EntityAnalysisSectionProps> = ({
  context,
  interactive = true,
  showEntityRelationships = true,
  onEntityClick
}) => {
  const handleEntityClick = (entityId: string) => {
    if (onEntityClick) {
      onEntityClick(entityId)
    }
  }

  const getEntityTypeColor = (type: string) => {
    switch (type) {
      case 'table': return 'blue'
      case 'column': return 'green'
      case 'metric': return 'orange'
      case 'dimension': return 'purple'
      default: return 'default'
    }
  }

  const getEntityTypeIcon = (type: string) => {
    switch (type) {
      case 'table': return <DatabaseOutlined />
      case 'column': return <TagsOutlined />
      case 'metric': return <TagsOutlined />
      case 'dimension': return <TagsOutlined />
      default: return <TagsOutlined />
    }
  }

  if (context.entities.length === 0) {
    return (
      <Alert
        message="No entities detected"
        description="No entities detected in this query"
        type="info"
        showIcon
      />
    )
  }

  return (
    <Space direction="vertical" style={{ width: '100%' }}>
      {/* Entity Overview */}
      <Card size="small">
        <Row gutter={[16, 16]}>
          <Col xs={24} sm={8}>
            <div style={{ textAlign: 'center' }}>
              <div style={{ fontSize: '20px', fontWeight: 'bold', color: '#13c2c2' }}>
                {context.entities.length}
              </div>
              <Text type="secondary" style={{ fontSize: '12px' }}>Total Entities</Text>
            </div>
          </Col>
          <Col xs={24} sm={8}>
            <div style={{ textAlign: 'center' }}>
              <div style={{ fontSize: '20px', fontWeight: 'bold', color: '#52c41a' }}>
                {Math.round((context.entities.reduce((sum, e) => sum + e.confidence, 0) / context.entities.length) * 100)}%
              </div>
              <Text type="secondary" style={{ fontSize: '12px' }}>Avg Confidence</Text>
            </div>
          </Col>
          <Col xs={24} sm={8}>
            <div style={{ textAlign: 'center' }}>
              <div style={{ fontSize: '20px', fontWeight: 'bold', color: '#1890ff' }}>
                {new Set(context.entities.map(e => e.type)).size}
              </div>
              <Text type="secondary" style={{ fontSize: '12px' }}>Entity Types</Text>
            </div>
          </Col>
        </Row>
      </Card>

      {/* Entity Type Breakdown */}
      <Card size="small">
        <Text strong style={{ marginBottom: 8, display: 'block' }}>Entity Types:</Text>
        <Space wrap>
          {Array.from(new Set(context.entities.map(e => e.type))).map(type => {
            const count = context.entities.filter(e => e.type === type).length
            return (
              <Tag key={type} color={getEntityTypeColor(type)} icon={getEntityTypeIcon(type)}>
                {type} ({count})
              </Tag>
            )
          })}
        </Space>
      </Card>

      {/* Detailed Entity List */}
      {showEntityRelationships && (
        <List
          size="small"
          header={<Text strong>Entity Details & Relationships</Text>}
          dataSource={context.entities}
          renderItem={(entity) => (
            <List.Item
              actions={interactive ? [
                <Button
                  key="explore"
                  type="link"
                  size="small"
                  onClick={() => handleEntityClick(entity.id)}
                >
                  Explore
                </Button>
              ] : undefined}
            >
              <List.Item.Meta
                avatar={
                  <Avatar
                    style={{
                      backgroundColor: entity.confidence > 0.8 ? '#52c41a' : '#1890ff'
                    }}
                  >
                    {entity.name.charAt(0).toUpperCase()}
                  </Avatar>
                }
                title={
                  <Space>
                    <Text strong style={{ fontSize: '13px' }}>{entity.name}</Text>
                    <Tag color={getEntityTypeColor(entity.type)} size="small">
                      {entity.type}
                    </Tag>
                  </Space>
                }
                description={
                  <Space direction="vertical" size="small" style={{ width: '100%' }}>
                    <div>
                      <Text type="secondary" style={{ fontSize: '11px' }}>
                        {entity.description || 'No description available'}
                      </Text>
                    </div>
                    <div style={{ display: 'flex', alignItems: 'center', gap: '8px' }}>
                      <Text type="secondary" style={{ fontSize: '10px' }}>
                        Confidence:
                      </Text>
                      <Progress
                        percent={Math.round(entity.confidence * 100)}
                        size="small"
                        style={{ width: 80 }}
                        strokeColor={entity.confidence > 0.8 ? '#52c41a' : '#1890ff'}
                      />
                      <Text style={{ fontSize: '10px' }}>
                        {Math.round(entity.confidence * 100)}%
                      </Text>
                    </div>
                    {entity.relationships && entity.relationships.length > 0 && (
                      <div>
                        <Text type="secondary" style={{ fontSize: '10px' }}>
                          <LinkOutlined /> Related: {entity.relationships.slice(0, 2).join(', ')}
                          {entity.relationships.length > 2 && ` +${entity.relationships.length - 2} more`}
                        </Text>
                      </div>
                    )}
                  </Space>
                }
              />
            </List.Item>
          )}
        />
      )}
    </Space>
  )
}

export default EntityAnalysisSection
