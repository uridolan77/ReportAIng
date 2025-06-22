import React from 'react'
import { Card, Empty, Spin, Typography, Tag, Progress, Space } from 'antd'
import type { QueryIntent } from '@shared/types/business-intelligence'

const { Text } = Typography

interface IntentClassificationPanelProps {
  intent?: QueryIntent
  loading?: boolean
  showAlternatives?: boolean
  showConfidenceBreakdown?: boolean
  interactive?: boolean
}

export const IntentClassificationPanel: React.FC<IntentClassificationPanelProps> = ({
  intent,
  loading = false,
  showAlternatives = true,
  showConfidenceBreakdown = true,
  interactive = true
}) => {
  if (loading) {
    return (
      <Card>
        <div style={{ textAlign: 'center', padding: '40px 0' }}>
          <Spin size="large" />
          <Text>Classifying intent...</Text>
        </div>
      </Card>
    )
  }

  if (!intent) {
    return (
      <Card>
        <Empty description="Enter a query to see intent classification" />
      </Card>
    )
  }

  return (
    <Space direction="vertical" style={{ width: '100%' }} size="large">
      <Card title="Primary Intent">
        <Space direction="vertical" style={{ width: '100%' }}>
          <div>
            <Tag color="blue" style={{ fontSize: '14px', padding: '4px 12px' }}>
              {intent.type.replace('_', ' ').toUpperCase()}
            </Tag>
            <Tag color="orange">
              {intent.complexity.toUpperCase()}
            </Tag>
          </div>
          
          <div>
            <Text strong>Confidence: </Text>
            <Progress 
              percent={Math.round(intent.confidence * 100)} 
              size="small" 
              style={{ width: 200, display: 'inline-block', marginLeft: 8 }}
            />
          </div>
          
          <Text>{intent.description}</Text>
          
          {intent.businessGoal && (
            <div>
              <Text strong>Business Goal: </Text>
              <Text>{intent.businessGoal}</Text>
            </div>
          )}
        </Space>
      </Card>

      {intent.reasoning && intent.reasoning.length > 0 && (
        <Card title="Reasoning" size="small">
          <ul>
            {intent.reasoning.map((reason, index) => (
              <li key={index}>
                <Text>{reason}</Text>
              </li>
            ))}
          </ul>
        </Card>
      )}

      {intent.recommendedActions && intent.recommendedActions.length > 0 && (
        <Card title="Recommended Actions" size="small">
          <ul>
            {intent.recommendedActions.map((action, index) => (
              <li key={index}>
                <Text>{action}</Text>
              </li>
            ))}
          </ul>
        </Card>
      )}
    </Space>
  )
}

export default IntentClassificationPanel
