import React from 'react'
import { Card, Space, Typography, Tag, Progress, List, Avatar, Button } from 'antd'
import { DatabaseOutlined, TeamOutlined, EyeOutlined } from '@ant-design/icons'
import type { BusinessContextProfile } from '@shared/types/business-intelligence'

const { Text, Title } = Typography

interface DomainContextSectionProps {
  context: BusinessContextProfile
  interactive?: boolean
  onDomainExplore?: (domain: string) => void
}

export const DomainContextSection: React.FC<DomainContextSectionProps> = ({
  context,
  interactive = true,
  onDomainExplore
}) => {
  const handleDomainExplore = (domainName: string) => {
    if (onDomainExplore) {
      onDomainExplore(domainName)
    }
  }

  const getDomainIcon = (domainName: string) => {
    switch (domainName.toLowerCase()) {
      case 'sales': return <DatabaseOutlined style={{ color: '#52c41a' }} />
      case 'marketing': return <DatabaseOutlined style={{ color: '#1890ff' }} />
      case 'finance': return <DatabaseOutlined style={{ color: '#fa8c16' }} />
      case 'operations': return <DatabaseOutlined style={{ color: '#722ed1' }} />
      case 'hr': return <TeamOutlined style={{ color: '#13c2c2' }} />
      default: return <DatabaseOutlined style={{ color: '#666' }} />
    }
  }

  if (!context.domain) {
    return null
  }

  return (
    <Card size="small">
      <Space direction="vertical" style={{ width: '100%' }} size="middle">
        <div>
          <Title level={5} style={{ margin: 0, color: '#722ed1' }}>
            {context.domain.name}
          </Title>
          <Text type="secondary">
            Relevance Score: {Math.round(context.domain.relevance * 100)}%
          </Text>
        </div>

        <div>
          <Text strong style={{ color: '#1890ff' }}>
            <DatabaseOutlined /> Domain Description:
          </Text>
          <br />
          <Text type="secondary">
            {context.domain.description || 'No description available'}
          </Text>
        </div>

        {context.domain.keyMetrics && context.domain.keyMetrics.length > 0 && (
          <div>
            <Text strong style={{ color: '#52c41a' }}>
              <DatabaseOutlined /> Key Metrics:
            </Text>
            <div style={{ marginTop: 8 }}>
              <Space wrap>
                {context.domain.keyMetrics.map((metric, index) => (
                  <Tag key={index} color="green" style={{ fontSize: '11px' }}>
                    {metric}
                  </Tag>
                ))}
              </Space>
            </div>
          </div>
        )}

        {context.domain.relationships && context.domain.relationships.length > 0 && (
          <div>
            <Text strong style={{ color: '#fa8c16' }}>
              <TeamOutlined /> Domain Relationships:
            </Text>
            <List
              size="small"
              style={{ marginTop: 8 }}
              dataSource={context.domain.relationships}
              renderItem={(relationship, index) => (
                <List.Item>
                  <List.Item.Meta
                    avatar={<Avatar size="small" style={{ backgroundColor: '#fa8c16' }}>{index + 1}</Avatar>}
                    description={relationship}
                  />
                </List.Item>
              )}
            />
          </div>
        )}

        {interactive && (
          <div style={{ textAlign: 'center', marginTop: 16 }}>
            <Button
              type="primary"
              ghost
              icon={<EyeOutlined />}
              onClick={() => handleDomainExplore(context.domain!.name)}
            >
              Explore Domain
            </Button>
          </div>
        )}
      </Space>
    </Card>
  )
}

export default DomainContextSection
