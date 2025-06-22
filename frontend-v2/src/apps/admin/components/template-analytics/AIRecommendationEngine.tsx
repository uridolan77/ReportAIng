import React, { useState, useEffect } from 'react'
import { 
  Card, 
  Row, 
  Col, 
  Typography, 
  Button, 
  List, 
  Avatar, 
  Tag, 
  Space,
  Progress,
  Rate,
  Modal,
  Form,
  Input,
  Select,
  Slider,
  Alert,
  Tabs,
  Statistic,
  Badge,
  Tooltip,
  message
} from 'antd'
import {
  RobotOutlined,
  RocketOutlined,
  BulbOutlined,
  ThunderboltOutlined,
  TrophyOutlined,
  AlertOutlined,
  CheckCircleOutlined,
  CloseCircleOutlined,
  SettingOutlined,
  PlayCircleOutlined,
  EyeOutlined,
  HeartOutlined,
  DislikeOutlined,
  LikeOutlined,
  StarOutlined,
  FireOutlined
} from '@ant-design/icons'

const { Title, Text, Paragraph } = Typography
const { TextArea } = Input

interface AIRecommendation {
  id: string
  type: 'optimization' | 'new_template' | 'deprecation' | 'security' | 'performance' | 'usage'
  title: string
  description: string
  reasoning: string
  confidence: number
  impact: 'low' | 'medium' | 'high' | 'critical'
  effort: 'low' | 'medium' | 'high'
  category: string
  templateKey?: string
  estimatedImprovement: number
  priority: number
  tags: string[]
  createdAt: string
  status: 'new' | 'reviewing' | 'accepted' | 'rejected' | 'implemented'
  feedback?: {
    rating: number
    comments: string
  }
}

interface AIInsight {
  id: string
  title: string
  description: string
  type: 'pattern' | 'anomaly' | 'trend' | 'opportunity'
  severity: 'info' | 'warning' | 'critical'
  affectedTemplates: string[]
  dataPoints: any[]
  actionable: boolean
}

export const AIRecommendationEngine: React.FC = () => {
  const [selectedCategory, setSelectedCategory] = useState('all')
  const [isConfigModalVisible, setIsConfigModalVisible] = useState(false)
  const [selectedRecommendation, setSelectedRecommendation] = useState<AIRecommendation | null>(null)
  const [form] = Form.useForm()

  // Mock AI recommendations
  const [recommendations, setRecommendations] = useState<AIRecommendation[]>([
    {
      id: 'rec_1',
      type: 'optimization',
      title: 'Optimize SQL Generation Prompt Structure',
      description: 'Analysis shows that restructuring the SQL generation prompt can improve success rate by 15%',
      reasoning: 'Pattern analysis of 10,000+ queries reveals that templates with specific instruction ordering perform significantly better',
      confidence: 92,
      impact: 'high',
      effort: 'medium',
      category: 'Performance',
      templateKey: 'sql_generation',
      estimatedImprovement: 15.3,
      priority: 95,
      tags: ['sql', 'optimization', 'prompt-engineering'],
      createdAt: '2024-01-15T10:30:00Z',
      status: 'new'
    },
    {
      id: 'rec_2',
      type: 'new_template',
      title: 'Create Financial Reporting Template',
      description: 'Emerging usage pattern detected: 23% increase in financial reporting queries',
      reasoning: 'Users are adapting general templates for financial use cases, creating a specialized template would improve efficiency',
      confidence: 87,
      impact: 'medium',
      effort: 'high',
      category: 'New Feature',
      estimatedImprovement: 28.5,
      priority: 78,
      tags: ['financial', 'reporting', 'new-template'],
      createdAt: '2024-01-15T09:15:00Z',
      status: 'reviewing'
    },
    {
      id: 'rec_3',
      type: 'security',
      title: 'Enhance Input Validation for Data Analysis Template',
      description: 'Potential security vulnerability detected in data analysis template input handling',
      reasoning: 'Static analysis identified patterns that could lead to injection attacks',
      confidence: 96,
      impact: 'critical',
      effort: 'low',
      category: 'Security',
      templateKey: 'data_analysis',
      estimatedImprovement: 0,
      priority: 98,
      tags: ['security', 'validation', 'critical'],
      createdAt: '2024-01-15T08:45:00Z',
      status: 'accepted'
    }
  ])

  const [insights, setInsights] = useState<AIInsight[]>([
    {
      id: 'insight_1',
      title: 'Peak Hour Performance Pattern',
      description: 'Templates show 23% slower response times during 9-11 AM',
      type: 'pattern',
      severity: 'warning',
      affectedTemplates: ['sql_generation', 'insight_generation'],
      dataPoints: [],
      actionable: true
    },
    {
      id: 'insight_2',
      title: 'Unusual Error Spike Detected',
      description: 'Error rate increased 340% in the last 2 hours for explanation templates',
      type: 'anomaly',
      severity: 'critical',
      affectedTemplates: ['explanation'],
      dataPoints: [],
      actionable: true
    }
  ])

  const handleAcceptRecommendation = async (recommendation: AIRecommendation) => {
    try {
      setRecommendations(prev => 
        prev.map(rec => 
          rec.id === recommendation.id 
            ? { ...rec, status: 'accepted' }
            : rec
        )
      )
      message.success('Recommendation accepted and queued for implementation')
    } catch (error) {
      message.error('Failed to accept recommendation')
    }
  }

  const handleRejectRecommendation = async (recommendation: AIRecommendation) => {
    try {
      setRecommendations(prev => 
        prev.map(rec => 
          rec.id === recommendation.id 
            ? { ...rec, status: 'rejected' }
            : rec
        )
      )
      message.success('Recommendation rejected')
    } catch (error) {
      message.error('Failed to reject recommendation')
    }
  }

  const handleProvideFeedback = async (recommendation: AIRecommendation, rating: number, comments: string) => {
    try {
      setRecommendations(prev => 
        prev.map(rec => 
          rec.id === recommendation.id 
            ? { ...rec, feedback: { rating, comments } }
            : rec
        )
      )
      message.success('Feedback submitted successfully')
    } catch (error) {
      message.error('Failed to submit feedback')
    }
  }

  const getImpactColor = (impact: string) => {
    switch (impact) {
      case 'critical': return '#f5222d'
      case 'high': return '#fa8c16'
      case 'medium': return '#1890ff'
      case 'low': return '#52c41a'
      default: return '#d9d9d9'
    }
  }

  const getTypeIcon = (type: string) => {
    switch (type) {
      case 'optimization': return <RocketOutlined />
      case 'new_template': return <BulbOutlined />
      case 'security': return <AlertOutlined />
      case 'performance': return <ThunderboltOutlined />
      case 'usage': return <TrophyOutlined />
      default: return <BrainOutlined />
    }
  }

  const getStatusColor = (status: string) => {
    switch (status) {
      case 'new': return 'blue'
      case 'reviewing': return 'orange'
      case 'accepted': return 'green'
      case 'rejected': return 'red'
      case 'implemented': return 'purple'
      default: return 'default'
    }
  }

  const getSeverityIcon = (severity: string) => {
    switch (severity) {
      case 'critical': return <AlertOutlined style={{ color: '#f5222d' }} />
      case 'warning': return <AlertOutlined style={{ color: '#fa8c16' }} />
      default: return <CheckCircleOutlined style={{ color: '#52c41a' }} />
    }
  }

  const filteredRecommendations = selectedCategory === 'all' 
    ? recommendations 
    : recommendations.filter(rec => rec.category.toLowerCase() === selectedCategory.toLowerCase())

  const recommendationsTab = (
    <div>
      {/* Controls */}
      <Row gutter={16} style={{ marginBottom: '24px' }}>
        <Col span={12}>
          <Space>
            <Text strong>Category:</Text>
            <Select value={selectedCategory} onChange={setSelectedCategory} style={{ width: 200 }}>
              <Select.Option value="all">All Categories</Select.Option>
              <Select.Option value="performance">Performance</Select.Option>
              <Select.Option value="security">Security</Select.Option>
              <Select.Option value="new feature">New Feature</Select.Option>
              <Select.Option value="optimization">Optimization</Select.Option>
            </Select>
          </Space>
        </Col>
        <Col span={12} style={{ textAlign: 'right' }}>
          <Space>
            <Button icon={<SettingOutlined />} onClick={() => setIsConfigModalVisible(true)}>
              Configure AI
            </Button>
            <Button type="primary" icon={<RobotOutlined />}>
              Generate New Recommendations
            </Button>
          </Space>
        </Col>
      </Row>

      {/* Recommendations List */}
      <List
        dataSource={filteredRecommendations}
        renderItem={(recommendation) => (
          <List.Item
            key={recommendation.id}
            actions={[
              <Button 
                size="small" 
                icon={<EyeOutlined />}
                onClick={() => setSelectedRecommendation(recommendation)}
              >
                Details
              </Button>,
              <Button 
                size="small" 
                type="primary"
                icon={<CheckCircleOutlined />}
                onClick={() => handleAcceptRecommendation(recommendation)}
                disabled={recommendation.status !== 'new'}
              >
                Accept
              </Button>,
              <Button 
                size="small" 
                danger
                icon={<CloseCircleOutlined />}
                onClick={() => handleRejectRecommendation(recommendation)}
                disabled={recommendation.status !== 'new'}
              >
                Reject
              </Button>
            ]}
          >
            <List.Item.Meta
              avatar={
                <Avatar 
                  icon={getTypeIcon(recommendation.type)} 
                  style={{ backgroundColor: getImpactColor(recommendation.impact) }}
                />
              }
              title={
                <Space>
                  <Text strong>{recommendation.title}</Text>
                  <Tag color={getStatusColor(recommendation.status)}>
                    {recommendation.status.toUpperCase()}
                  </Tag>
                  <Tag color={getImpactColor(recommendation.impact)}>
                    {recommendation.impact.toUpperCase()} IMPACT
                  </Tag>
                </Space>
              }
              description={
                <div>
                  <Paragraph style={{ margin: 0, marginBottom: '8px' }}>
                    {recommendation.description}
                  </Paragraph>
                  <Space>
                    <Text strong>Confidence:</Text>
                    <Progress 
                      percent={recommendation.confidence} 
                      size="small" 
                      style={{ width: '100px' }}
                    />
                    <Text strong>Priority:</Text>
                    <Text>{recommendation.priority}/100</Text>
                    {recommendation.estimatedImprovement > 0 && (
                      <>
                        <Text strong>Expected Improvement:</Text>
                        <Text style={{ color: '#52c41a' }}>+{recommendation.estimatedImprovement}%</Text>
                      </>
                    )}
                  </Space>
                  <div style={{ marginTop: '8px' }}>
                    {recommendation.tags.map(tag => (
                      <Tag key={tag} size="small">{tag}</Tag>
                    ))}
                  </div>
                </div>
              }
            />
          </List.Item>
        )}
      />
    </div>
  )

  const insightsTab = (
    <div>
      <List
        dataSource={insights}
        renderItem={(insight) => (
          <List.Item
            key={insight.id}
            actions={[
              <Button size="small" icon={<EyeOutlined />}>
                Investigate
              </Button>,
              insight.actionable && (
                <Button size="small" type="primary" icon={<PlayCircleOutlined />}>
                  Take Action
                </Button>
              )
            ]}
          >
            <List.Item.Meta
              avatar={getSeverityIcon(insight.severity)}
              title={
                <Space>
                  <Text strong>{insight.title}</Text>
                  <Tag color={insight.type === 'anomaly' ? 'red' : 'blue'}>
                    {insight.type.toUpperCase()}
                  </Tag>
                </Space>
              }
              description={
                <div>
                  <Text>{insight.description}</Text>
                  <div style={{ marginTop: '8px' }}>
                    <Text strong>Affected Templates:</Text>
                    <Space style={{ marginLeft: '8px' }}>
                      {insight.affectedTemplates.map(template => (
                        <Tag key={template} size="small">{template}</Tag>
                      ))}
                    </Space>
                  </div>
                </div>
              }
            />
          </List.Item>
        )}
      />
    </div>
  )

  const tabs = [
    {
      key: 'recommendations',
      label: (
        <Space>
          <BrainOutlined />
          AI Recommendations
          <Badge count={recommendations.filter(r => r.status === 'new').length} />
        </Space>
      ),
      children: recommendationsTab
    },
    {
      key: 'insights',
      label: (
        <Space>
          <FireOutlined />
          AI Insights
          <Badge count={insights.length} />
        </Space>
      ),
      children: insightsTab
    }
  ]

  return (
    <div>
      {/* AI Engine Status */}
      <Row gutter={16} style={{ marginBottom: '24px' }}>
        <Col span={6}>
          <Card>
            <Statistic
              title="Active Recommendations"
              value={recommendations.filter(r => r.status === 'new').length}
              prefix={<RobotOutlined />}
              valueStyle={{ color: '#1890ff' }}
            />
          </Card>
        </Col>
        <Col span={6}>
          <Card>
            <Statistic
              title="Avg Confidence"
              value={recommendations.reduce((sum, r) => sum + r.confidence, 0) / recommendations.length}
              suffix="%"
              precision={1}
              prefix={<TrophyOutlined />}
              valueStyle={{ color: '#52c41a' }}
            />
          </Card>
        </Col>
        <Col span={6}>
          <Card>
            <Statistic
              title="Implemented"
              value={recommendations.filter(r => r.status === 'implemented').length}
              prefix={<CheckCircleOutlined />}
              valueStyle={{ color: '#722ed1' }}
            />
          </Card>
        </Col>
        <Col span={6}>
          <Card>
            <Statistic
              title="Total Impact"
              value={recommendations.reduce((sum, r) => sum + r.estimatedImprovement, 0)}
              suffix="% improvement"
              precision={1}
              prefix={<RocketOutlined />}
              valueStyle={{ color: '#fa8c16' }}
            />
          </Card>
        </Col>
      </Row>

      {/* Main Content */}
      <Tabs items={tabs} />

      {/* Recommendation Details Modal */}
      <Modal
        title={selectedRecommendation?.title}
        open={!!selectedRecommendation}
        onCancel={() => setSelectedRecommendation(null)}
        width={800}
        footer={[
          <Button key="close" onClick={() => setSelectedRecommendation(null)}>
            Close
          </Button>,
          <Button 
            key="accept" 
            type="primary"
            onClick={() => {
              if (selectedRecommendation) {
                handleAcceptRecommendation(selectedRecommendation)
                setSelectedRecommendation(null)
              }
            }}
          >
            Accept Recommendation
          </Button>
        ]}
      >
        {selectedRecommendation && (
          <div>
            <Alert
              message={`${selectedRecommendation.impact.toUpperCase()} Impact Recommendation`}
              description={selectedRecommendation.description}
              type={selectedRecommendation.impact === 'critical' ? 'error' : 'info'}
              showIcon
              style={{ marginBottom: '16px' }}
            />
            
            <Title level={5}>AI Reasoning</Title>
            <Paragraph>{selectedRecommendation.reasoning}</Paragraph>
            
            <Row gutter={16} style={{ marginBottom: '16px' }}>
              <Col span={8}>
                <Text strong>Confidence Level</Text>
                <Progress percent={selectedRecommendation.confidence} />
              </Col>
              <Col span={8}>
                <Text strong>Expected Improvement</Text>
                <div style={{ fontSize: '20px', color: '#52c41a' }}>
                  +{selectedRecommendation.estimatedImprovement}%
                </div>
              </Col>
              <Col span={8}>
                <Text strong>Implementation Effort</Text>
                <Tag color={getImpactColor(selectedRecommendation.effort)}>
                  {selectedRecommendation.effort.toUpperCase()}
                </Tag>
              </Col>
            </Row>

            <Title level={5}>Provide Feedback</Title>
            <Space direction="vertical" style={{ width: '100%' }}>
              <div>
                <Text>Rate this recommendation:</Text>
                <Rate style={{ marginLeft: '8px' }} />
              </div>
              <TextArea 
                rows={3} 
                placeholder="Optional feedback about this recommendation..."
              />
            </Space>
          </div>
        )}
      </Modal>

      {/* AI Configuration Modal */}
      <Modal
        title="Configure AI Recommendation Engine"
        open={isConfigModalVisible}
        onCancel={() => setIsConfigModalVisible(false)}
        footer={[
          <Button key="cancel" onClick={() => setIsConfigModalVisible(false)}>
            Cancel
          </Button>,
          <Button key="save" type="primary">
            Save Configuration
          </Button>
        ]}
      >
        <Form form={form} layout="vertical">
          <Form.Item name="analysisFrequency" label="Analysis Frequency">
            <Select>
              <Select.Option value="realtime">Real-time</Select.Option>
              <Select.Option value="hourly">Hourly</Select.Option>
              <Select.Option value="daily">Daily</Select.Option>
              <Select.Option value="weekly">Weekly</Select.Option>
            </Select>
          </Form.Item>
          
          <Form.Item name="confidenceThreshold" label="Minimum Confidence Threshold">
            <Slider min={50} max={100} marks={{ 50: '50%', 75: '75%', 90: '90%', 100: '100%' }} />
          </Form.Item>
          
          <Form.Item name="categories" label="Active Categories">
            <Select mode="multiple">
              <Select.Option value="performance">Performance</Select.Option>
              <Select.Option value="security">Security</Select.Option>
              <Select.Option value="optimization">Optimization</Select.Option>
              <Select.Option value="new_features">New Features</Select.Option>
            </Select>
          </Form.Item>
        </Form>
      </Modal>
    </div>
  )
}

export default AIRecommendationEngine
