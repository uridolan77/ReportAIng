import React, { useState } from 'react'
import { 
  Card, 
  List, 
  Tag, 
  Button, 
  Collapse, 
  Empty, 
  Typography, 
  Space,
  Tooltip,
  Badge
} from 'antd'
import { 
  BulbOutlined, 
  DollarOutlined, 
  ThunderboltOutlined,
  InfoCircleOutlined,
  CheckOutlined
} from '@ant-design/icons'
import type { CostRecommendation } from '../../types/cost'

const { Text, Paragraph } = Typography
const { Panel } = Collapse

interface RecommendationsWidgetProps {
  recommendations?: CostRecommendation[]
  loading?: boolean
  onImplement?: (recommendationId: string) => void
  showImplementButton?: boolean
}

export const RecommendationsWidget: React.FC<RecommendationsWidgetProps> = ({
  recommendations = [],
  loading = false,
  onImplement,
  showImplementButton = false
}) => {
  const [implementedIds, setImplementedIds] = useState<Set<string>>(new Set())

  const handleImplement = (recommendationId: string) => {
    setImplementedIds(prev => new Set([...prev, recommendationId]))
    onImplement?.(recommendationId)
  }

  const getImpactColor = (impact: string) => {
    switch (impact.toLowerCase()) {
      case 'high': return '#f5222d'
      case 'medium': return '#faad14'
      case 'low': return '#52c41a'
      default: return '#d9d9d9'
    }
  }

  const getEffortColor = (effort: string) => {
    switch (effort.toLowerCase()) {
      case 'high': return '#722ed1'
      case 'medium': return '#1890ff'
      case 'low': return '#13c2c2'
      default: return '#d9d9d9'
    }
  }

  const getPriorityScore = (recommendation: CostRecommendation) => {
    const impactScore = recommendation.impact === 'High' ? 3 : recommendation.impact === 'Medium' ? 2 : 1
    const effortScore = recommendation.effort === 'Low' ? 3 : recommendation.effort === 'Medium' ? 2 : 1
    const savingsScore = recommendation.potentialSavings > 1000 ? 3 : recommendation.potentialSavings > 100 ? 2 : 1
    
    return impactScore + effortScore + savingsScore
  }

  const sortedRecommendations = [...recommendations].sort((a, b) => 
    getPriorityScore(b) - getPriorityScore(a)
  )

  const totalPotentialSavings = recommendations.reduce((sum, rec) => sum + rec.potentialSavings, 0)

  if (recommendations.length === 0) {
    return (
      <Card title="Cost Optimization Recommendations">
        <Empty 
          description="No recommendations available"
          image={Empty.PRESENTED_IMAGE_SIMPLE}
        />
      </Card>
    )
  }

  return (
    <Card 
      title={
        <Space>
          <BulbOutlined />
          Cost Optimization Recommendations
          <Badge count={recommendations.length} style={{ backgroundColor: '#52c41a' }} />
        </Space>
      }
      extra={
        <Tooltip title="Total potential savings from all recommendations">
          <Tag color="green" icon={<DollarOutlined />}>
            ${totalPotentialSavings.toLocaleString()}
          </Tag>
        </Tooltip>
      }
    >
      <List
        size="small"
        dataSource={sortedRecommendations.slice(0, 5)} // Show top 5 recommendations
        renderItem={(recommendation) => {
          const isImplemented = implementedIds.has(recommendation.id)
          
          return (
            <List.Item style={{ padding: '12px 0' }}>
              <div style={{ width: '100%' }}>
                {/* Recommendation Header */}
                <div style={{ 
                  display: 'flex', 
                  justifyContent: 'space-between', 
                  alignItems: 'flex-start',
                  marginBottom: '8px'
                }}>
                  <div style={{ flex: 1 }}>
                    <div style={{ display: 'flex', alignItems: 'center', gap: '8px', marginBottom: '4px' }}>
                      <Text strong style={{ 
                        textDecoration: isImplemented ? 'line-through' : 'none',
                        color: isImplemented ? '#999' : 'inherit'
                      }}>
                        {recommendation.title}
                      </Text>
                      {isImplemented && <CheckOutlined style={{ color: '#52c41a' }} />}
                    </div>
                    
                    <Space size="small" wrap>
                      <Tag color={getImpactColor(recommendation.impact)} size="small">
                        {recommendation.impact} Impact
                      </Tag>
                      <Tag color={getEffortColor(recommendation.effort)} size="small">
                        {recommendation.effort} Effort
                      </Tag>
                      <Tag color="blue" size="small">
                        {recommendation.category}
                      </Tag>
                    </Space>
                  </div>
                  
                  <div style={{ textAlign: 'right', marginLeft: '16px' }}>
                    <div style={{ 
                      fontSize: '14px', 
                      fontWeight: 'bold', 
                      color: '#52c41a',
                      marginBottom: '4px'
                    }}>
                      ${recommendation.potentialSavings.toLocaleString()}
                    </div>
                    <Text type="secondary" style={{ fontSize: '11px' }}>
                      potential savings
                    </Text>
                  </div>
                </div>

                {/* Description */}
                <Paragraph 
                  style={{ 
                    margin: '8px 0',
                    fontSize: '12px',
                    color: '#666'
                  }}
                  ellipsis={{ rows: 2, expandable: true, symbol: 'more' }}
                >
                  {recommendation.description}
                </Paragraph>

                {/* Action Items */}
                {recommendation.actionItems && recommendation.actionItems.length > 0 && (
                  <Collapse ghost size="small">
                    <Panel 
                      header={
                        <Text style={{ fontSize: '12px' }}>
                          <InfoCircleOutlined /> View Action Items ({recommendation.actionItems.length})
                        </Text>
                      } 
                      key="1"
                    >
                      <List
                        size="small"
                        dataSource={recommendation.actionItems}
                        renderItem={(item, index) => (
                          <List.Item style={{ padding: '4px 0', fontSize: '11px' }}>
                            <Text type="secondary">
                              {index + 1}. {item}
                            </Text>
                          </List.Item>
                        )}
                      />
                    </Panel>
                  </Collapse>
                )}

                {/* Implement Button */}
                {showImplementButton && onImplement && !isImplemented && (
                  <div style={{ marginTop: '8px', textAlign: 'right' }}>
                    <Button 
                      size="small" 
                      type="primary"
                      icon={<ThunderboltOutlined />}
                      onClick={() => handleImplement(recommendation.id)}
                    >
                      Implement
                    </Button>
                  </div>
                )}
              </div>
            </List.Item>
          )
        }}
      />

      {recommendations.length > 5 && (
        <div style={{ 
          textAlign: 'center', 
          marginTop: '16px',
          padding: '8px',
          backgroundColor: '#fafafa',
          borderRadius: '4px'
        }}>
          <Text type="secondary" style={{ fontSize: '12px' }}>
            Showing top 5 of {recommendations.length} recommendations
          </Text>
        </div>
      )}
    </Card>
  )
}
