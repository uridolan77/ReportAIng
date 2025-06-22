import React, { useState, useEffect } from 'react'
import { 
  Card, 
  Row, 
  Col, 
  Button, 
  Select, 
  Table, 
  Tag, 
  Modal, 
  Form, 
  Input,
  Typography,
  Space,
  Progress,
  Alert,
  Tooltip,
  Badge,
  Statistic,
  List,
  Avatar,
  message,
  Tabs
} from 'antd'
import {
  BulbOutlined,
  RocketOutlined,
  ExperimentOutlined,
  BarChartOutlined,
  CheckCircleOutlined,
  ExclamationCircleOutlined,
  ClockCircleOutlined,
  TrophyOutlined,
  WarningOutlined,
  InfoCircleOutlined,
  ThunderboltOutlined,
  StarOutlined,
  EyeOutlined,
  PlayCircleOutlined
} from '@ant-design/icons'
import {
  useAnalyzeTemplatePerformanceMutation,
  useOptimizeTemplateMutation,
  useReviewImprovementSuggestionMutation,
  useGenerateImprovementSuggestionsMutation,
  useGetUsageInsightsQuery
} from '@shared/store/api/templateAnalyticsApi'
import type { 
  TemplateImprovementSuggestion, 
  OptimizedTemplate,
  OptimizationStrategy,
  SuggestionStatus,
  ImprovementType,
  SuggestionReviewAction
} from '@shared/types/templateAnalytics'

const { Title, Text } = Typography
const { Option } = Select
const { TextArea } = Input

interface TemplateImprovementDashboardProps {
  templateKey?: string
  onTemplateSelect?: (templateKey: string) => void
}

export const TemplateImprovementDashboard: React.FC<TemplateImprovementDashboardProps> = ({ 
  templateKey = '',
  onTemplateSelect 
}) => {
  // State
  const [selectedStrategy, setSelectedStrategy] = useState<OptimizationStrategy>('Balanced')
  const [reviewModalVisible, setReviewModalVisible] = useState(false)
  const [selectedSuggestion, setSelectedSuggestion] = useState<TemplateImprovementSuggestion | null>(null)
  const [activeTab, setActiveTab] = useState('analysis')

  // API hooks
  const [analyzePerformance, { data: suggestions, isLoading: analyzing }] = useAnalyzeTemplatePerformanceMutation()
  const [optimizeTemplate, { data: optimizedTemplate, isLoading: optimizing }] = useOptimizeTemplateMutation()
  const [reviewSuggestion] = useReviewImprovementSuggestionMutation()
  const [generateSuggestions, { data: generatedSuggestions, isLoading: generating }] = useGenerateImprovementSuggestionsMutation()
  const { data: usageInsights } = useGetUsageInsightsQuery({})

  // Handlers
  const handleAnalyze = async () => {
    if (!templateKey) {
      message.warning('Please select a template first')
      return
    }
    try {
      await analyzePerformance(templateKey).unwrap()
      message.success('Performance analysis completed')
    } catch (error) {
      message.error('Failed to analyze template performance')
    }
  }

  const handleOptimize = async () => {
    if (!templateKey) {
      message.warning('Please select a template first')
      return
    }
    try {
      await optimizeTemplate({ templateKey, strategy: selectedStrategy }).unwrap()
      message.success('Template optimization completed')
    } catch (error) {
      message.error('Failed to optimize template')
    }
  }

  const handleGenerateSuggestions = async () => {
    try {
      await generateSuggestions({ performanceThreshold: 0.7, minDataPoints: 100 }).unwrap()
      message.success('Improvement suggestions generated')
    } catch (error) {
      message.error('Failed to generate suggestions')
    }
  }

  const handleReviewSuggestion = async (values: any) => {
    if (selectedSuggestion) {
      try {
        await reviewSuggestion({
          suggestionId: selectedSuggestion.id,
          action: values.action,
          reviewComments: values.comments
        }).unwrap()
        message.success('Suggestion reviewed successfully')
        setReviewModalVisible(false)
        setSelectedSuggestion(null)
      } catch (error) {
        message.error('Failed to review suggestion')
      }
    }
  }

  // Table columns for suggestions
  const suggestionColumns = [
    {
      title: 'Type',
      dataIndex: 'type',
      key: 'type',
      render: (type: ImprovementType) => (
        <Tag color={getTypeColor(type)}>{type}</Tag>
      ),
    },
    {
      title: 'Template',
      dataIndex: 'templateName',
      key: 'templateName',
      render: (name: string) => (
        <Text strong>{name}</Text>
      ),
    },
    {
      title: 'Expected Improvement',
      dataIndex: 'expectedImprovementPercent',
      key: 'improvement',
      render: (percent: number) => (
        <Statistic 
          value={percent} 
          suffix="%" 
          precision={1}
          valueStyle={{ fontSize: '14px' }}
        />
      ),
    },
    {
      title: 'Confidence',
      dataIndex: 'confidenceScore',
      key: 'confidence',
      render: (score: number) => (
        <Progress 
          percent={score * 100} 
          size="small" 
          status={score > 0.8 ? 'success' : score > 0.6 ? 'normal' : 'exception'}
        />
      ),
    },
    {
      title: 'Status',
      dataIndex: 'status',
      key: 'status',
      render: (status: SuggestionStatus) => (
        <Tag color={getStatusColor(status)}>{status}</Tag>
      ),
    },
    {
      title: 'Actions',
      key: 'actions',
      render: (record: TemplateImprovementSuggestion) => (
        <Space>
          <Button
            size="small"
            icon={<EyeOutlined />}
            onClick={() => {
              setSelectedSuggestion(record)
              setReviewModalVisible(true)
            }}
          >
            Review
          </Button>
          {onTemplateSelect && (
            <Button
              size="small"
              icon={<PlayCircleOutlined />}
              onClick={() => onTemplateSelect(record.templateKey)}
            >
              Select
            </Button>
          )}
        </Space>
      ),
    },
  ]

  const tabItems = [
    {
      key: 'analysis',
      label: (
        <Space>
          <BarChartOutlined />
          Performance Analysis
        </Space>
      ),
      children: (
        <div>
          <Row gutter={16} style={{ marginBottom: '24px' }}>
            <Col span={8}>
              <Card size="small">
                <Statistic
                  title="Templates Analyzed"
                  value={usageInsights?.topPerformingTemplates?.length || 0}
                  prefix={<BarChartOutlined />}
                />
              </Card>
            </Col>
            <Col span={8}>
              <Card size="small">
                <Statistic
                  title="Avg Success Rate"
                  value={usageInsights?.averageSuccessRate ? (usageInsights.averageSuccessRate * 100) : 0}
                  suffix="%"
                  precision={1}
                  prefix={<TrophyOutlined />}
                />
              </Card>
            </Col>
            <Col span={8}>
              <Card size="small">
                <Statistic
                  title="Total Usage"
                  value={usageInsights?.totalUsage || 0}
                  prefix={<ThunderboltOutlined />}
                />
              </Card>
            </Col>
          </Row>

          <Card title="Template Performance Analysis">
            <Space style={{ marginBottom: '16px' }}>
              <Button
                type="primary"
                icon={<BarChartOutlined />}
                loading={analyzing}
                onClick={handleAnalyze}
                disabled={!templateKey}
              >
                Analyze Performance
              </Button>
              <Text type="secondary">
                {templateKey ? `Selected: ${templateKey}` : 'No template selected'}
              </Text>
            </Space>

            {suggestions && suggestions.length > 0 && (
              <Table
                dataSource={suggestions}
                columns={suggestionColumns}
                rowKey="id"
                size="small"
                pagination={{ pageSize: 5 }}
              />
            )}
          </Card>
        </div>
      )
    },
    {
      key: 'optimization',
      label: (
        <Space>
          <RocketOutlined />
          Template Optimization
        </Space>
      ),
      children: (
        <Card title="Template Optimization">
          <Row gutter={16} style={{ marginBottom: '16px' }}>
            <Col span={12}>
              <Select
                value={selectedStrategy}
                onChange={setSelectedStrategy}
                style={{ width: '100%' }}
                placeholder="Select optimization strategy"
              >
                <Option value="PerformanceFocused">Performance Focused</Option>
                <Option value="AccuracyFocused">Accuracy Focused</Option>
                <Option value="UserSatisfactionFocused">User Satisfaction</Option>
                <Option value="ResponseTimeFocused">Response Time</Option>
                <Option value="Balanced">Balanced</Option>
              </Select>
            </Col>
            <Col span={12}>
              <Button
                type="primary"
                icon={<RocketOutlined />}
                loading={optimizing}
                onClick={handleOptimize}
                disabled={!templateKey}
                block
              >
                Optimize Template
              </Button>
            </Col>
          </Row>

          {optimizedTemplate && (
            <Alert
              message="Optimization Complete"
              description={
                <div>
                  <p><strong>Strategy:</strong> {optimizedTemplate.strategyUsed}</p>
                  <p><strong>Expected Improvement:</strong> {optimizedTemplate.expectedPerformanceImprovement.toFixed(1)}%</p>
                  <p><strong>Confidence:</strong> {(optimizedTemplate.confidenceScore * 100).toFixed(1)}%</p>
                </div>
              }
              type="success"
              showIcon
              style={{ marginTop: '16px' }}
            />
          )}
        </Card>
      )
    },
    {
      key: 'suggestions',
      label: (
        <Space>
          <BulbOutlined />
          Improvement Suggestions
          {generatedSuggestions && <Badge count={generatedSuggestions.length} size="small" />}
        </Space>
      ),
      children: (
        <Card title="AI-Generated Improvement Suggestions">
          <Button
            type="primary"
            icon={<BulbOutlined />}
            loading={generating}
            onClick={handleGenerateSuggestions}
            style={{ marginBottom: '16px' }}
          >
            Generate New Suggestions
          </Button>

          {generatedSuggestions && generatedSuggestions.length > 0 && (
            <Table
              dataSource={generatedSuggestions}
              columns={suggestionColumns}
              rowKey="id"
              size="small"
              pagination={{ pageSize: 10 }}
            />
          )}
        </Card>
      )
    }
  ]

  return (
    <div className="template-improvement-dashboard">
      <Card style={{ marginBottom: '24px' }}>
        <Row gutter={16} align="middle">
          <Col span={16}>
            <Space>
              <BulbOutlined style={{ fontSize: '24px', color: '#1890ff' }} />
              <div>
                <Title level={3} style={{ margin: 0 }}>Template Improvement Dashboard</Title>
                <Text type="secondary">AI-powered template optimization and enhancement</Text>
              </div>
            </Space>
          </Col>
          <Col span={8} style={{ textAlign: 'right' }}>
            <Space>
              <Button icon={<ExperimentOutlined />}>
                Create A/B Test
              </Button>
            </Space>
          </Col>
        </Row>
      </Card>

      <Tabs
        activeKey={activeTab}
        onChange={setActiveTab}
        items={tabItems}
        size="large"
      />

      {/* Review Modal */}
      <Modal
        title="Review Improvement Suggestion"
        open={reviewModalVisible}
        onCancel={() => setReviewModalVisible(false)}
        footer={null}
        width={600}
      >
        {selectedSuggestion && (
          <Form onFinish={handleReviewSuggestion} layout="vertical">
            <div style={{ marginBottom: '16px', padding: '16px', background: '#f5f5f5', borderRadius: '6px' }}>
              <p><strong>Type:</strong> {selectedSuggestion.type}</p>
              <p><strong>Template:</strong> {selectedSuggestion.templateName}</p>
              <p><strong>Expected Improvement:</strong> {selectedSuggestion.expectedImprovementPercent}%</p>
              <p><strong>Reasoning:</strong> {selectedSuggestion.reasoningExplanation}</p>
            </div>

            <Form.Item name="action" label="Review Action" rules={[{ required: true }]}>
              <Select placeholder="Select action">
                <Option value="Approve">Approve</Option>
                <Option value="Reject">Reject</Option>
                <Option value="RequestChanges">Request Changes</Option>
                <Option value="ScheduleABTest">Schedule A/B Test</Option>
              </Select>
            </Form.Item>

            <Form.Item name="comments" label="Comments">
              <TextArea rows={4} placeholder="Add review comments..." />
            </Form.Item>

            <Form.Item>
              <Button type="primary" htmlType="submit" block>
                Submit Review
              </Button>
            </Form.Item>
          </Form>
        )}
      </Modal>
    </div>
  )
}

// Helper functions
const getTypeColor = (type: ImprovementType): string => {
  switch (type) {
    case 'PerformanceOptimization': return 'red'
    case 'ContentOptimization': return 'blue'
    case 'StructureImprovement': return 'green'
    case 'ExampleAddition': return 'orange'
    case 'InstructionClarification': return 'purple'
    case 'ContextEnhancement': return 'cyan'
    default: return 'default'
  }
}

const getStatusColor = (status: SuggestionStatus): string => {
  switch (status) {
    case 'Pending': return 'orange'
    case 'Approved': return 'green'
    case 'Rejected': return 'red'
    case 'Implemented': return 'blue'
    case 'NeedsChanges': return 'yellow'
    case 'ScheduledForTesting': return 'purple'
    default: return 'default'
  }
}

export default TemplateImprovementDashboard
