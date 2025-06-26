import React from 'react'
import { Card, Space, Typography, Tag, Badge, Progress, Row, Col, Statistic } from 'antd'
import { BulbOutlined, InfoCircleOutlined, TrophyOutlined } from '@ant-design/icons'
import type { BusinessContextProfile } from '@shared/types/business-intelligence'

const { Text, Title, Paragraph } = Typography

interface IntentAnalysisSectionProps {
  context: BusinessContextProfile
  interactive?: boolean
}

export const IntentAnalysisSection: React.FC<IntentAnalysisSectionProps> = ({
  context,
  interactive = true
}) => {
  const getIntentColor = (type: string) => {
    switch (type) {
      case 'analytical': return 'blue'
      case 'operational': return 'green'
      case 'strategic': return 'purple'
      case 'exploratory': return 'orange'
      default: return 'default'
    }
  }

  const getComplexityColor = (complexity: string) => {
    switch (complexity) {
      case 'simple': return 'green'
      case 'moderate': return 'orange'
      case 'complex': return 'red'
      default: return 'default'
    }
  }

  return (
    <Card size="small" style={{ marginBottom: 16 }}>
      <Space direction="vertical" style={{ width: '100%' }}>
        <Paragraph>{context.intent.description}</Paragraph>

        {context.intent.businessGoal && (
          <div>
            <Text strong style={{ color: '#52c41a' }}>
              <TrophyOutlined /> Business Goal:
            </Text>
            <br />
            <Text type="secondary">{context.intent.businessGoal}</Text>
          </div>
        )}

        {/* Intent Metrics */}
        <Row gutter={[16, 16]} style={{ marginTop: 16 }}>
          <Col xs={24} sm={8}>
            <Statistic
              title="Confidence"
              value={Math.round(context.intent.confidence * 100)}
              suffix="%"
              prefix={<BulbOutlined />}
              valueStyle={{ color: context.intent.confidence > 0.8 ? '#52c41a' : '#1890ff' }}
            />
          </Col>
          <Col xs={24} sm={8}>
            <Statistic
              title="Type"
              value={context.intent.type.replace('_', ' ').toUpperCase()}
              prefix={<InfoCircleOutlined />}
              valueStyle={{ color: '#722ed1' }}
            />
          </Col>
          <Col xs={24} sm={8}>
            <Statistic
              title="Complexity"
              value={context.intent.complexity.toUpperCase()}
              prefix={<TrophyOutlined />}
              valueStyle={{ 
                color: context.intent.complexity === 'simple' ? '#52c41a' : 
                       context.intent.complexity === 'moderate' ? '#fa8c16' : '#ff4d4f'
              }}
            />
          </Col>
        </Row>

        {/* Intent Confidence Breakdown */}
        {context.intent.confidence && (
          <div style={{ marginTop: 16 }}>
            <Text strong>Intent Analysis Breakdown:</Text>
            <div style={{ marginTop: 8 }}>
              <div style={{ display: 'flex', justifyContent: 'space-between', marginBottom: 4 }}>
                <Text type="secondary" style={{ fontSize: '12px' }}>Semantic Understanding</Text>
                <Text style={{ fontSize: '12px' }}>{Math.round(context.intent.confidence * 100)}%</Text>
              </div>
              <Progress 
                percent={Math.round(context.intent.confidence * 100)} 
                size="small" 
                strokeColor="#1890ff"
                showInfo={false}
              />
            </div>
          </div>
        )}

        {/* Alternative Intents */}
        {context.intent.alternatives && context.intent.alternatives.length > 0 && (
          <div style={{ marginTop: 16 }}>
            <Text strong>Alternative Interpretations:</Text>
            <div style={{ marginTop: 8 }}>
              {context.intent.alternatives.slice(0, 3).map((alt, index) => (
                <div key={index} style={{ marginBottom: 8 }}>
                  <Space>
                    <Tag color={getIntentColor(alt.type)} style={{ fontSize: '11px' }}>
                      {alt.type.replace('_', ' ')}
                    </Tag>
                    <Text style={{ fontSize: '12px' }}>{alt.description}</Text>
                    <Badge 
                      count={`${Math.round(alt.confidence * 100)}%`} 
                      style={{ backgroundColor: '#f0f0f0', color: '#666' }}
                    />
                  </Space>
                </div>
              ))}
            </div>
          </div>
        )}
      </Space>
    </Card>
  )
}

export default IntentAnalysisSection
