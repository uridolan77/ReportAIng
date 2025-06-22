import React, { useState, useEffect } from 'react'
import { 
  Card, 
  Row, 
  Col, 
  Button, 
  Typography,
  Space,
  Progress,
  Tag,
  Alert,
  List,
  Tooltip,
  Badge,
  Statistic,
  Collapse,
  Avatar,
  message,
  Modal,
  Tabs
} from 'antd'
import {
  BulbOutlined,
  RocketOutlined,
  TrophyOutlined,
  ExclamationCircleOutlined,
  CheckCircleOutlined,
  InfoCircleOutlined,
  ThunderboltOutlined,
  BarChartOutlined,
  StarOutlined,
  EyeOutlined,
  PlayCircleOutlined,
  ClockCircleOutlined,
  FireOutlined
} from '@ant-design/icons'
import dayjs from 'dayjs'
import {
  useGenerateImprovementSuggestionsMutation,
  useGetUsageInsightsQuery,
  useGetQualityMetricsQuery
} from '@shared/store/api/templateAnalyticsApi'
import type {
  TemplateImprovementSuggestion,
  UsageInsightsData,
  QualityMetricsData,
  ImprovementType,
  SuggestionStatus
} from '@shared/types/templateAnalytics'

const { Title, Text, Paragraph } = Typography
const { Panel } = Collapse

interface MLRecommendation {
  id: string
  type: 'optimization' | 'quality' | 'performance' | 'usage'
  priority: 'high' | 'medium' | 'low'
  title: string
  description: string
  impact: number
  confidence: number
  actionable: boolean
  estimatedEffort: 'low' | 'medium' | 'high'
  expectedOutcome: string
  relatedTemplates: string[]
  createdAt: Date
}

interface MLRecommendationEngineProps {
  intentType?: string
  onRecommendationApply?: (recommendation: MLRecommendation) => void
}

export const MLRecommendationEngine: React.FC<MLRecommendationEngineProps> = ({
  intentType,
  onRecommendationApply
}) => {
  // State
  const [recommendations, setRecommendations] = useState<MLRecommendation[]>([])
  const [selectedRecommendation, setSelectedRecommendation] = useState<MLRecommendation | null>(null)
  const [previewModalVisible, setPreviewModalVisible] = useState(false)
  const [activeTab, setActiveTab] = useState('all')
  const [isGeneratingRecommendations, setIsGeneratingRecommendations] = useState(false)

  // API hooks
  const [generateSuggestions, { isLoading: isGenerating }] = useGenerateImprovementSuggestionsMutation()
  const { data: usageInsights } = useGetUsageInsightsQuery({
    startDate: dayjs().subtract(30, 'day').toISOString(),
    endDate: dayjs().toISOString(),
    intentType
  })
  const { data: qualityMetrics } = useGetQualityMetricsQuery({ intentType })

  // Generate ML recommendations based on data
  useEffect(() => {
    if (usageInsights && qualityMetrics) {
      generateMLRecommendations()
    }
  }, [usageInsights, qualityMetrics])

  const generateMLRecommendations = async () => {
    setIsGeneratingRecommendations(true)
    
    // Simulate ML processing
    await new Promise(resolve => setTimeout(resolve, 2000))
    
    const mlRecommendations: MLRecommendation[] = [
      {
        id: '1',
        type: 'performance',
        priority: 'high',
        title: 'Optimize High-Usage Templates',
        description: 'Templates with >1000 daily uses but <85% success rate should be prioritized for optimization',
        impact: 0.92,
        confidence: 0.89,
        actionable: true,
        estimatedEffort: 'medium',
        expectedOutcome: 'Increase overall success rate by 12-15%',
        relatedTemplates: ['sql_generation_basic', 'insight_generation_advanced'],
        createdAt: new Date()
      },
      {
        id: '2',
        type: 'quality',
        priority: 'high',
        title: 'Improve Content Clarity',
        description: 'Analysis shows 23% of templates have clarity issues affecting user satisfaction',
        impact: 0.85,
        confidence: 0.91,
        actionable: true,
        estimatedEffort: 'low',
        expectedOutcome: 'Improve user rating by 0.8 points',
        relatedTemplates: ['explanation_detailed', 'data_analysis_comprehensive'],
        createdAt: new Date()
      },
      {
        id: '3',
        type: 'optimization',
        priority: 'medium',
        title: 'Implement A/B Testing Strategy',
        description: 'Templates without recent A/B tests show 18% lower performance improvement rates',
        impact: 0.76,
        confidence: 0.82,
        actionable: true,
        estimatedEffort: 'high',
        expectedOutcome: 'Establish continuous improvement pipeline',
        relatedTemplates: ['sql_generation_basic', 'visualization_charts'],
        createdAt: new Date()
      },
      {
        id: '4',
        type: 'usage',
        priority: 'medium',
        title: 'Address Underperforming Templates',
        description: 'Templates with declining usage patterns need immediate attention or deprecation',
        impact: 0.68,
        confidence: 0.87,
        actionable: true,
        estimatedEffort: 'medium',
        expectedOutcome: 'Reduce maintenance overhead by 25%',
        relatedTemplates: ['legacy_report_generator', 'old_chart_template'],
        createdAt: new Date()
      },
      {
        id: '5',
        type: 'performance',
        priority: 'low',
        title: 'Optimize Response Times',
        description: 'Templates with >3s response time can be optimized for better user experience',
        impact: 0.54,
        confidence: 0.75,
        actionable: true,
        estimatedEffort: 'low',
        expectedOutcome: 'Reduce average response time by 1.2s',
        relatedTemplates: ['complex_analysis_template'],
        createdAt: new Date()
      }
    ]
    
    setRecommendations(mlRecommendations)
    setIsGeneratingRecommendations(false)
    message.success('ML recommendations generated successfully')
  }

  const handleApplyRecommendation = (recommendation: MLRecommendation) => {
    onRecommendationApply?.(recommendation)
    message.success(`Applied recommendation: ${recommendation.title}`)
  }

  const handlePreviewRecommendation = (recommendation: MLRecommendation) => {
    setSelectedRecommendation(recommendation)
    setPreviewModalVisible(true)
  }

  const getPriorityColor = (priority: string): string => {
    switch (priority) {
      case 'high': return '#ff4d4f'
      case 'medium': return '#faad14'
      case 'low': return '#52c41a'
      default: return '#666666'
    }
  }

  const getTypeIcon = (type: string) => {
    switch (type) {
      case 'performance': return <ThunderboltOutlined />
      case 'quality': return <StarOutlined />
      case 'optimization': return <RocketOutlined />
      case 'usage': return <BarChartOutlined />
      default: return <BulbOutlined />
    }
  }

  const getTypeColor = (type: string): string => {
    switch (type) {
      case 'performance': return '#fa8c16'
      case 'quality': return '#1890ff'
      case 'optimization': return '#52c41a'
      case 'usage': return '#722ed1'
      default: return '#666666'
    }
  }

  const getEffortIcon = (effort: string) => {
    switch (effort) {
      case 'low': return <CheckCircleOutlined style={{ color: '#52c41a' }} />
      case 'medium': return <ClockCircleOutlined style={{ color: '#faad14' }} />
      case 'high': return <ExclamationCircleOutlined style={{ color: '#ff4d4f' }} />
      default: return <InfoCircleOutlined />
    }
  }

  const filteredRecommendations = recommendations.filter(rec => 
    activeTab === 'all' || rec.type === activeTab
  )

  const highPriorityCount = recommendations.filter(r => r.priority === 'high').length
  const actionableCount = recommendations.filter(r => r.actionable).length

  return (
    <div className="ml-recommendation-engine">
      {/* Header */}
      <Card style={{ marginBottom: '24px' }}>
        <Row gutter={16} align="middle">
          <Col span={16}>
            <Space>
              <BulbOutlined style={{ fontSize: '24px', color: '#722ed1' }} />
              <div>
                <Title level={3} style={{ margin: 0 }}>ML Recommendation Engine</Title>
                <Text type="secondary">AI-powered insights and optimization suggestions</Text>
              </div>
            </Space>
          </Col>
          <Col span={8} style={{ textAlign: 'right' }}>
            <Space>
              <Badge count={highPriorityCount} offset={[10, 0]}>
                <Button icon={<FireOutlined />}>
                  High Priority
                </Button>
              </Badge>
              <Button 
                type="primary" 
                icon={<BulbOutlined />}
                onClick={generateMLRecommendations}
                loading={isGeneratingRecommendations}
              >
                Refresh Insights
              </Button>
            </Space>
          </Col>
        </Row>
      </Card>

      {/* Summary Stats */}
      <Row gutter={16} style={{ marginBottom: '24px' }}>
        <Col span={6}>
          <Card size="small" style={{ textAlign: 'center' }}>
            <Statistic
              title="Total Recommendations"
              value={recommendations.length}
              valueStyle={{ color: '#1890ff' }}
              prefix={<BulbOutlined />}
            />
          </Card>
        </Col>
        <Col span={6}>
          <Card size="small" style={{ textAlign: 'center' }}>
            <Statistic
              title="High Priority"
              value={highPriorityCount}
              valueStyle={{ color: '#ff4d4f' }}
              prefix={<ExclamationCircleOutlined />}
            />
          </Card>
        </Col>
        <Col span={6}>
          <Card size="small" style={{ textAlign: 'center' }}>
            <Statistic
              title="Actionable"
              value={actionableCount}
              valueStyle={{ color: '#52c41a' }}
              prefix={<CheckCircleOutlined />}
            />
          </Card>
        </Col>
        <Col span={6}>
          <Card size="small" style={{ textAlign: 'center' }}>
            <Statistic
              title="Avg Impact"
              value={recommendations.reduce((acc, r) => acc + r.impact, 0) / recommendations.length * 100}
              precision={1}
              suffix="%"
              valueStyle={{ color: '#722ed1' }}
              prefix={<TrophyOutlined />}
            />
          </Card>
        </Col>
      </Row>

      {/* Recommendations */}
      <Card 
        title={
          <Space>
            <BulbOutlined />
            AI Recommendations
            <Badge count={recommendations.length} />
          </Space>
        }
      >
        <Tabs
          activeKey={activeTab}
          onChange={setActiveTab}
          items={[
            { key: 'all', label: 'All Recommendations' },
            { key: 'performance', label: 'Performance' },
            { key: 'quality', label: 'Quality' },
            { key: 'optimization', label: 'Optimization' },
            { key: 'usage', label: 'Usage' }
          ]}
        />

        {isGeneratingRecommendations ? (
          <div style={{ textAlign: 'center', padding: '40px 0' }}>
            <Space direction="vertical" size="large">
              <BulbOutlined style={{ fontSize: '48px', color: '#722ed1' }} />
              <div>
                <Title level={4}>Generating ML Recommendations...</Title>
                <Text type="secondary">
                  Analyzing patterns and generating personalized insights
                </Text>
              </div>
              <Progress percent={30} status="active" />
            </Space>
          </div>
        ) : (
          <List
            dataSource={filteredRecommendations}
            renderItem={(recommendation) => (
              <List.Item
                actions={[
                  <Button 
                    key="preview"
                    icon={<EyeOutlined />}
                    onClick={() => handlePreviewRecommendation(recommendation)}
                  >
                    Preview
                  </Button>,
                  <Button 
                    key="apply"
                    type="primary"
                    icon={<PlayCircleOutlined />}
                    onClick={() => handleApplyRecommendation(recommendation)}
                    disabled={!recommendation.actionable}
                  >
                    Apply
                  </Button>
                ]}
              >
                <List.Item.Meta
                  avatar={
                    <Avatar 
                      style={{ backgroundColor: getTypeColor(recommendation.type) }}
                      icon={getTypeIcon(recommendation.type)}
                    />
                  }
                  title={
                    <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                      <Space>
                        <Text strong>{recommendation.title}</Text>
                        <Tag color={getPriorityColor(recommendation.priority)}>
                          {recommendation.priority.toUpperCase()}
                        </Tag>
                        <Tag color={getTypeColor(recommendation.type)}>
                          {recommendation.type.toUpperCase()}
                        </Tag>
                      </Space>
                      <Space>
                        <Tooltip title="Impact Score">
                          <div style={{ display: 'flex', alignItems: 'center', gap: '4px' }}>
                            <TrophyOutlined style={{ color: '#faad14' }} />
                            <Text strong>{(recommendation.impact * 100).toFixed(0)}%</Text>
                          </div>
                        </Tooltip>
                        <Tooltip title="Confidence Level">
                          <div style={{ display: 'flex', alignItems: 'center', gap: '4px' }}>
                            <CheckCircleOutlined style={{ color: '#52c41a' }} />
                            <Text strong>{(recommendation.confidence * 100).toFixed(0)}%</Text>
                          </div>
                        </Tooltip>
                      </Space>
                    </div>
                  }
                  description={
                    <div>
                      <Paragraph style={{ margin: '8px 0' }}>
                        {recommendation.description}
                      </Paragraph>
                      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                        <Space>
                          <div style={{ display: 'flex', alignItems: 'center', gap: '4px' }}>
                            {getEffortIcon(recommendation.estimatedEffort)}
                            <Text type="secondary" style={{ fontSize: '12px' }}>
                              {recommendation.estimatedEffort.toUpperCase()} EFFORT
                            </Text>
                          </div>
                          <Text type="secondary" style={{ fontSize: '12px' }}>
                            {recommendation.relatedTemplates.length} templates affected
                          </Text>
                        </Space>
                        <Text type="secondary" style={{ fontSize: '12px' }}>
                          {dayjs(recommendation.createdAt).fromNow()}
                        </Text>
                      </div>
                    </div>
                  }
                />
              </List.Item>
            )}
          />
        )}
      </Card>

      {/* Preview Modal */}
      <Modal
        title={
          selectedRecommendation ? (
            <Space>
              {getTypeIcon(selectedRecommendation.type)}
              {selectedRecommendation.title}
              <Tag color={getPriorityColor(selectedRecommendation.priority)}>
                {selectedRecommendation.priority.toUpperCase()}
              </Tag>
            </Space>
          ) : 'Recommendation Preview'
        }
        open={previewModalVisible}
        onCancel={() => setPreviewModalVisible(false)}
        footer={null}
        width={800}
      >
        {selectedRecommendation && (
          <div>
            <Row gutter={16} style={{ marginBottom: '24px' }}>
              <Col span={8}>
                <Statistic
                  title="Impact Score"
                  value={selectedRecommendation.impact * 100}
                  precision={1}
                  suffix="%"
                  valueStyle={{ color: '#52c41a' }}
                />
              </Col>
              <Col span={8}>
                <Statistic
                  title="Confidence Level"
                  value={selectedRecommendation.confidence * 100}
                  precision={1}
                  suffix="%"
                  valueStyle={{ color: '#1890ff' }}
                />
              </Col>
              <Col span={8}>
                <div>
                  <Text strong>Estimated Effort:</Text>
                  <div style={{ marginTop: '4px' }}>
                    <Space>
                      {getEffortIcon(selectedRecommendation.estimatedEffort)}
                      <Text>{selectedRecommendation.estimatedEffort.toUpperCase()}</Text>
                    </Space>
                  </div>
                </div>
              </Col>
            </Row>

            <div style={{ marginBottom: '16px' }}>
              <Text strong>Description:</Text>
              <Paragraph style={{ marginTop: '8px' }}>
                {selectedRecommendation.description}
              </Paragraph>
            </div>

            <div style={{ marginBottom: '16px' }}>
              <Text strong>Expected Outcome:</Text>
              <Alert
                message={selectedRecommendation.expectedOutcome}
                type="success"
                showIcon
                style={{ marginTop: '8px' }}
              />
            </div>

            <div style={{ marginBottom: '16px' }}>
              <Text strong>Affected Templates:</Text>
              <div style={{ marginTop: '8px' }}>
                {selectedRecommendation.relatedTemplates.map(template => (
                  <Tag key={template} style={{ margin: '2px' }}>
                    {template}
                  </Tag>
                ))}
              </div>
            </div>

            <div style={{ textAlign: 'right', marginTop: '24px' }}>
              <Space>
                <Button onClick={() => setPreviewModalVisible(false)}>
                  Close
                </Button>
                <Button 
                  type="primary"
                  onClick={() => {
                    handleApplyRecommendation(selectedRecommendation)
                    setPreviewModalVisible(false)
                  }}
                  disabled={!selectedRecommendation.actionable}
                >
                  Apply Recommendation
                </Button>
              </Space>
            </div>
          </div>
        )}
      </Modal>
    </div>
  )
}

export default MLRecommendationEngine
