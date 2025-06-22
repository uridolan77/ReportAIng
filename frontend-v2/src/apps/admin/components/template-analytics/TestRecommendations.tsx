import React from 'react'
import { 
  Card, 
  List, 
  Button, 
  Tag, 
  Typography, 
  Space, 
  Progress,
  Alert,
  Row,
  Col
} from 'antd'
import { 
  BulbOutlined, 
  RocketOutlined, 
  TrophyOutlined,
  ExperimentOutlined,
  InfoCircleOutlined
} from '@ant-design/icons'
import { useGetABTestRecommendationsQuery } from '@shared/store/api/templateAnalyticsApi'

const { Title, Text, Paragraph } = Typography

interface TestRecommendationsProps {
  onCreateTest?: (recommendation: any) => void
}

export const TestRecommendations: React.FC<TestRecommendationsProps> = ({ 
  onCreateTest 
}) => {
  const { data: recommendations, isLoading } = useGetABTestRecommendationsQuery({})

  const getRecommendationIcon = (type: string) => {
    switch (type) {
      case 'performance_improvement': return <RocketOutlined />
      case 'usage_optimization': return <TrophyOutlined />
      case 'error_reduction': return <InfoCircleOutlined />
      default: return <BulbOutlined />
    }
  }

  const getRecommendationColor = (type: string) => {
    switch (type) {
      case 'performance_improvement': return '#52c41a'
      case 'usage_optimization': return '#1890ff'
      case 'error_reduction': return '#fa8c16'
      default: return '#722ed1'
    }
  }

  const getConfidenceColor = (confidence: number) => {
    if (confidence >= 80) return '#52c41a'
    if (confidence >= 60) return '#fa8c16'
    return '#f5222d'
  }

  const getRecommendationTitle = (type: string) => {
    switch (type) {
      case 'performance_improvement': return 'Performance Improvement'
      case 'usage_optimization': return 'Usage Optimization'
      case 'error_reduction': return 'Error Reduction'
      default: return 'General Improvement'
    }
  }

  if (isLoading) {
    return (
      <Card title="AI Test Recommendations" loading={true}>
        <div style={{ height: '200px' }} />
      </Card>
    )
  }

  if (!recommendations || recommendations.length === 0) {
    return (
      <Card 
        title={
          <Space>
            <BulbOutlined />
            AI Test Recommendations
          </Space>
        }
      >
        <Alert
          message="No recommendations available"
          description="Our AI system will analyze your templates and suggest A/B tests when opportunities are identified."
          type="info"
          showIcon
        />
      </Card>
    )
  }

  return (
    <Card 
      title={
        <Space>
          <BulbOutlined />
          AI Test Recommendations
          <Tag color="blue">{recommendations.length} suggestions</Tag>
        </Space>
      }
      extra={
        <Button type="link" size="small">
          View All
        </Button>
      }
    >
      <List
        dataSource={recommendations.slice(0, 5)} // Show top 5 recommendations
        renderItem={(recommendation, index) => (
          <List.Item
            key={index}
            actions={[
              <Button 
                type="primary" 
                size="small"
                icon={<ExperimentOutlined />}
                onClick={() => onCreateTest?.(recommendation)}
              >
                Create Test
              </Button>
            ]}
          >
            <List.Item.Meta
              avatar={
                <div style={{ 
                  width: '40px', 
                  height: '40px', 
                  borderRadius: '50%', 
                  backgroundColor: getRecommendationColor(recommendation.recommendationType),
                  display: 'flex',
                  alignItems: 'center',
                  justifyContent: 'center',
                  color: 'white',
                  fontSize: '16px'
                }}>
                  {getRecommendationIcon(recommendation.recommendationType)}
                </div>
              }
              title={
                <Space>
                  <Text strong>{getRecommendationTitle(recommendation.recommendationType)}</Text>
                  <Tag color={getRecommendationColor(recommendation.recommendationType)}>
                    +{recommendation.expectedImprovement}% expected
                  </Tag>
                </Space>
              }
              description={
                <div>
                  <Paragraph style={{ margin: 0, marginBottom: '8px' }}>
                    {recommendation.description}
                  </Paragraph>
                  
                  <Row gutter={16}>
                    <Col span={12}>
                      <Text type="secondary" style={{ fontSize: '12px' }}>
                        Template: <Text strong>{recommendation.templateKey}</Text>
                      </Text>
                    </Col>
                    <Col span={12}>
                      <Space>
                        <Text type="secondary" style={{ fontSize: '12px' }}>
                          Confidence:
                        </Text>
                        <Progress
                          percent={recommendation.confidence}
                          size="small"
                          strokeColor={getConfidenceColor(recommendation.confidence)}
                          style={{ width: '60px' }}
                        />
                        <Text 
                          style={{ 
                            fontSize: '12px',
                            color: getConfidenceColor(recommendation.confidence),
                            fontWeight: 600
                          }}
                        >
                          {recommendation.confidence}%
                        </Text>
                      </Space>
                    </Col>
                  </Row>
                </div>
              }
            />
          </List.Item>
        )}
      />
      
      {recommendations.length > 5 && (
        <div style={{ textAlign: 'center', marginTop: '16px' }}>
          <Button type="link">
            View {recommendations.length - 5} more recommendations
          </Button>
        </div>
      )}
    </Card>
  )
}

export default TestRecommendations
