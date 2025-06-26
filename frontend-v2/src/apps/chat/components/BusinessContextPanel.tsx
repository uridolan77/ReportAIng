import React, { useState, useEffect } from 'react'
import { Card, Space, Typography, Tag, List, Avatar, Tooltip, Collapse, Tree } from 'antd'
import {
  DatabaseOutlined,
  TableOutlined,
  LinkOutlined,
  UserOutlined,
  TeamOutlined,
  BarChartOutlined,
  InfoCircleOutlined
} from '@ant-design/icons'
import { useGetBusinessContextQuery } from '@shared/store/api/chatApi'

const { Text, Title } = Typography

export interface BusinessContextPanelProps {
  conversationId?: string
  showEntityDetails?: boolean
  showRelationships?: boolean
  className?: string
}

interface BusinessEntity {
  id: string
  name: string
  type: 'table' | 'column' | 'metric' | 'dimension'
  description?: string
  usage: number
  confidence: number
  relationships: string[]
}

interface BusinessDomain {
  id: string
  name: string
  description: string
  entities: BusinessEntity[]
  color: string
}

/**
 * BusinessContextPanel - Shows business context and entity relationships
 * 
 * Features:
 * - Domain-specific entity mapping
 * - Relationship visualization
 * - Usage statistics
 * - Confidence indicators
 * - Interactive entity exploration
 */
export const BusinessContextPanel: React.FC<BusinessContextPanelProps> = ({
  conversationId,
  showEntityDetails = true,
  showRelationships = true,
  className = ''
}) => {
  const [selectedEntity, setSelectedEntity] = useState<string | null>(null)
  const [expandedDomains, setExpandedDomains] = useState<string[]>(['sales'])

  // Mock business context data - in real app, this would come from API
  const mockDomains: BusinessDomain[] = [
    {
      id: 'sales',
      name: 'Sales & Revenue',
      description: 'Sales performance, revenue tracking, and customer metrics',
      color: '#52c41a',
      entities: [
        {
          id: 'sales_table',
          name: 'Sales Transactions',
          type: 'table',
          description: 'Main sales transaction data',
          usage: 85,
          confidence: 0.95,
          relationships: ['customer_table', 'product_table']
        },
        {
          id: 'revenue_metric',
          name: 'Total Revenue',
          type: 'metric',
          description: 'Sum of all sales amounts',
          usage: 92,
          confidence: 0.98,
          relationships: ['sales_table']
        },
        {
          id: 'customer_segment',
          name: 'Customer Segment',
          type: 'dimension',
          description: 'Customer categorization',
          usage: 67,
          confidence: 0.87,
          relationships: ['customer_table']
        }
      ]
    },
    {
      id: 'marketing',
      name: 'Marketing & Campaigns',
      description: 'Marketing campaign performance and lead generation',
      color: '#1890ff',
      entities: [
        {
          id: 'campaign_table',
          name: 'Marketing Campaigns',
          type: 'table',
          description: 'Campaign tracking and performance',
          usage: 73,
          confidence: 0.89,
          relationships: ['lead_table']
        },
        {
          id: 'conversion_rate',
          name: 'Conversion Rate',
          type: 'metric',
          description: 'Lead to customer conversion percentage',
          usage: 81,
          confidence: 0.93,
          relationships: ['campaign_table', 'lead_table']
        }
      ]
    },
    {
      id: 'operations',
      name: 'Operations & Supply',
      description: 'Operational metrics and supply chain data',
      color: '#faad14',
      entities: [
        {
          id: 'inventory_table',
          name: 'Inventory Levels',
          type: 'table',
          description: 'Current stock and inventory tracking',
          usage: 56,
          confidence: 0.82,
          relationships: ['product_table']
        }
      ]
    }
  ]

  const getEntityIcon = (type: string) => {
    switch (type) {
      case 'table': return <TableOutlined />
      case 'column': return <DatabaseOutlined />
      case 'metric': return <BarChartOutlined />
      case 'dimension': return <TeamOutlined />
      default: return <InfoCircleOutlined />
    }
  }

  const getUsageColor = (usage: number) => {
    if (usage >= 80) return '#52c41a'
    if (usage >= 60) return '#faad14'
    return '#ff4d4f'
  }

  const getConfidenceColor = (confidence: number) => {
    if (confidence >= 0.9) return '#52c41a'
    if (confidence >= 0.7) return '#faad14'
    return '#ff4d4f'
  }

  const renderEntityCard = (entity: BusinessEntity) => (
    <Card
      key={entity.id}
      size="small"
      style={{
        marginBottom: '8px',
        cursor: 'pointer',
        border: selectedEntity === entity.id ? '2px solid #1890ff' : '1px solid #d9d9d9'
      }}
      onClick={() => setSelectedEntity(selectedEntity === entity.id ? null : entity.id)}
    >
      <Space direction="vertical" style={{ width: '100%' }}>
        <Space style={{ width: '100%', justifyContent: 'space-between' }}>
          <Space>
            {getEntityIcon(entity.type)}
            <Text strong style={{ fontSize: '12px' }}>{entity.name}</Text>
          </Space>
          <Tag color={entity.type === 'table' ? 'blue' : entity.type === 'metric' ? 'green' : 'orange'}>
            {entity.type}
          </Tag>
        </Space>

        {entity.description && (
          <Text type="secondary" style={{ fontSize: '11px' }}>
            {entity.description}
          </Text>
        )}

        <Space style={{ width: '100%', justifyContent: 'space-between' }}>
          <Space>
            <Tooltip title="Usage frequency">
              <div style={{ display: 'flex', alignItems: 'center', gap: '4px' }}>
                <div style={{
                  width: '8px',
                  height: '8px',
                  borderRadius: '50%',
                  backgroundColor: getUsageColor(entity.usage)
                }} />
                <Text style={{ fontSize: '10px' }}>{entity.usage}%</Text>
              </div>
            </Tooltip>
            <Tooltip title="Confidence score">
              <div style={{ display: 'flex', alignItems: 'center', gap: '4px' }}>
                <div style={{
                  width: '8px',
                  height: '8px',
                  borderRadius: '50%',
                  backgroundColor: getConfidenceColor(entity.confidence)
                }} />
                <Text style={{ fontSize: '10px' }}>{(entity.confidence * 100).toFixed(0)}%</Text>
              </div>
            </Tooltip>
          </Space>
          {entity.relationships.length > 0 && (
            <Tooltip title={`${entity.relationships.length} relationships`}>
              <Space>
                <LinkOutlined style={{ fontSize: '10px' }} />
                <Text style={{ fontSize: '10px' }}>{entity.relationships.length}</Text>
              </Space>
            </Tooltip>
          )}
        </Space>

        {/* Entity Details */}
        {selectedEntity === entity.id && showEntityDetails && (
          <div style={{ 
            marginTop: '8px', 
            padding: '8px', 
            background: '#fafafa', 
            borderRadius: '4px' 
          }}>
            <Space direction="vertical" style={{ width: '100%' }}>
              <div>
                <Text strong style={{ fontSize: '11px' }}>Usage Statistics:</Text>
                <div style={{ marginTop: '4px' }}>
                  <div style={{ display: 'flex', justifyContent: 'space-between' }}>
                    <Text style={{ fontSize: '10px' }}>Frequency:</Text>
                    <Text style={{ fontSize: '10px' }}>{entity.usage}%</Text>
                  </div>
                  <div style={{ display: 'flex', justifyContent: 'space-between' }}>
                    <Text style={{ fontSize: '10px' }}>Confidence:</Text>
                    <Text style={{ fontSize: '10px' }}>{(entity.confidence * 100).toFixed(1)}%</Text>
                  </div>
                </div>
              </div>

              {showRelationships && entity.relationships.length > 0 && (
                <div>
                  <Text strong style={{ fontSize: '11px' }}>Related Entities:</Text>
                  <div style={{ marginTop: '4px' }}>
                    {entity.relationships.map(rel => (
                      <Tag key={rel} size="small" style={{ fontSize: '10px', margin: '1px' }}>
                        {rel.replace('_', ' ')}
                      </Tag>
                    ))}
                  </div>
                </div>
              )}
            </Space>
          </div>
        )}
      </Space>
    </Card>
  )

  const renderDomainSection = (domain: BusinessDomain) => ({
    key: domain.id,
    label: (
      <Space>
        <div style={{
          width: '12px',
          height: '12px',
          borderRadius: '50%',
          backgroundColor: domain.color
        }} />
        <Text strong>{domain.name}</Text>
        <Tag size="small">{domain.entities.length} entities</Tag>
      </Space>
    ),
    children: (
      <Space direction="vertical" style={{ width: '100%' }}>
        <Text type="secondary" style={{ fontSize: '11px' }}>
          {domain.description}
        </Text>

        <div style={{ marginTop: '8px' }}>
          {domain.entities.map(renderEntityCard)}
        </div>
      </Space>
    )
  })

  return (
    <div className={`business-context-panel ${className}`}>
      <Space direction="vertical" style={{ width: '100%' }}>
        {/* Summary */}
        <Card size="small">
          <Space direction="vertical" style={{ width: '100%' }}>
            <Text strong style={{ fontSize: '12px' }}>Business Context Summary</Text>
            <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '8px' }}>
              <div style={{ textAlign: 'center' }}>
                <div style={{ fontSize: '16px', fontWeight: 'bold', color: '#1890ff' }}>
                  {mockDomains.length}
                </div>
                <Text type="secondary" style={{ fontSize: '10px' }}>Domains</Text>
              </div>
              <div style={{ textAlign: 'center' }}>
                <div style={{ fontSize: '16px', fontWeight: 'bold', color: '#52c41a' }}>
                  {mockDomains.reduce((sum, domain) => sum + domain.entities.length, 0)}
                </div>
                <Text type="secondary" style={{ fontSize: '10px' }}>Entities</Text>
              </div>
            </div>
          </Space>
        </Card>

        {/* Domain Breakdown */}
        <Card size="small" title="Business Domains">
          <Collapse
            ghost
            size="small"
            activeKey={expandedDomains}
            onChange={(keys) => setExpandedDomains(keys as string[])}
            items={mockDomains.map(renderDomainSection)}
          />
        </Card>

        {/* Quick Actions */}
        <Card size="small" title="Quick Actions">
          <Space direction="vertical" style={{ width: '100%' }}>
            <Text style={{ fontSize: '11px' }}>
              • Click entities to see details
            </Text>
            <Text style={{ fontSize: '11px' }}>
              • Green dots = high usage/confidence
            </Text>
            <Text style={{ fontSize: '11px' }}>
              • Relationships show data connections
            </Text>
          </Space>
        </Card>
      </Space>
    </div>
  )
}

export default BusinessContextPanel
