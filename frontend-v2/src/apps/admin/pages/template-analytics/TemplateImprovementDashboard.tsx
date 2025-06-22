import React, { useState } from 'react'
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
  Divider,
  Progress,
  Tooltip,
  message,
  Spin
} from 'antd'
import {
  BulbOutlined,
  ExperimentOutlined,
  BarChartOutlined,
  CheckCircleOutlined,
  ExclamationCircleOutlined,
  InfoCircleOutlined,
  RocketOutlined,
  StarOutlined
} from '@ant-design/icons'
import {
  useAnalyzeTemplatePerformanceMutation,
  useOptimizeTemplateMutation,
  useReviewImprovementSuggestionMutation,
  useGenerateTemplateVariantsMutation,
  usePredictTemplatePerformanceMutation
} from '@shared/store/api/templateAnalyticsApi'
import type {
  TemplateImprovementSuggestion,
  OptimizedTemplate,
  OptimizationStrategy,
  SuggestionReviewAction,
  ImprovementType,
  SuggestionStatus,
  TemplateVariant,
  PerformancePrediction
} from '@shared/types/templateAnalytics'

const { Title, Text, Paragraph } = Typography
const { Option } = Select
const { TextArea } = Input

interface TemplateImprovementDashboardProps {
  templateKey?: string
  onTemplateSelect?: (templateKey: string) => void
}

export const TemplateImprovementDashboard: React.FC<TemplateImprovementDashboardProps> = ({ 
  templateKey,
  onTemplateSelect 
}) => {
  // State management
  const [selectedStrategy, setSelectedStrategy] = useState<OptimizationStrategy>('Balanced')
  const [reviewModalVisible, setReviewModalVisible] = useState(false)
  const [selectedSuggestion, setSelectedSuggestion] = useState<TemplateImprovementSuggestion | null>(null)
  const [optimizationModalVisible, setOptimizationModalVisible] = useState(false)
  const [variantsModalVisible, setVariantsModalVisible] = useState(false)
  const [predictionModalVisible, setPredictionModalVisible] = useState(false)
  const [selectedTemplate, setSelectedTemplate] = useState<string>(templateKey || '')

  // API hooks
  const [analyzePerformance, { data: suggestions, isLoading: analyzing }] = useAnalyzeTemplatePerformanceMutation()
  const [optimizeTemplate, { data: optimizedTemplate, isLoading: optimizing }] = useOptimizeTemplateMutation()
  const [reviewSuggestion, { isLoading: reviewing }] = useReviewImprovementSuggestionMutation()
  const [generateVariants, { data: variants, isLoading: generatingVariants }] = useGenerateTemplateVariantsMutation()
  const [predictPerformance, { data: prediction, isLoading: predicting }] = usePredictTemplatePerformanceMutation()

  // Event handlers
  const handleAnalyze = async () => {
    if (!selectedTemplate) {
      message.warning('Please select a template first')
      return
    }
    
    try {
      await analyzePerformance(selectedTemplate).unwrap()
      message.success('Performance analysis completed')
    } catch (error) {
      message.error('Failed to analyze template performance')
    }
  }

  const handleOptimize = async () => {
    if (!selectedTemplate) {
      message.warning('Please select a template first')
      return
    }
    
    try {
      await optimizeTemplate({ templateKey: selectedTemplate, strategy: selectedStrategy }).unwrap()
      setOptimizationModalVisible(true)
      message.success('Template optimization completed')
    } catch (error) {
      message.error('Failed to optimize template')
    }
  }

  const handleGenerateVariants = async () => {
    if (!selectedTemplate) {
      message.warning('Please select a template first')
      return
    }
    
    try {
      await generateVariants({ templateKey: selectedTemplate, variantCount: 3 }).unwrap()
      setVariantsModalVisible(true)
      message.success('Template variants generated')
    } catch (error) {
      message.error('Failed to generate template variants')
    }
  }

  const handlePredictPerformance = async (content: string, intentType: string) => {
    try {
      await predictPerformance({ templateContent: content, intentType }).unwrap()
      setPredictionModalVisible(true)
      message.success('Performance prediction completed')
    } catch (error) {
      message.error('Failed to predict template performance')
    }
  }

  const handleReviewSuggestion = async (values: any) => {
    if (!selectedSuggestion) return
    
    try {
      await reviewSuggestion({
        suggestionId: selectedSuggestion.id,
        action: values.action,
        reviewComments: values.comments
      }).unwrap()
      setReviewModalVisible(false)
      setSelectedSuggestion(null)
      message.success('Suggestion reviewed successfully')
    } catch (error) {
      message.error('Failed to review suggestion')
    }
  }

  // Utility functions
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

  const getConfidenceIcon = (score: number) => {
    if (score >= 0.8) return <CheckCircleOutlined style={{ color: '#52c41a' }} />
    if (score >= 0.6) return <InfoCircleOutlined style={{ color: '#1890ff' }} />
    return <ExclamationCircleOutlined style={{ color: '#faad14' }} />
  }

  // Table columns for suggestions
  const suggestionColumns = [
    {
      title: 'Type',
      dataIndex: 'type',
      key: 'type',
      render: (type: ImprovementType) => (
        <Tag color={getTypeColor(type)}>{type.replace(/([A-Z])/g, ' $1').trim()}</Tag>
      ),
    },
    {
      title: 'Expected Improvement',
      dataIndex: 'expectedImprovementPercent',
      key: 'improvement',
      render: (percent: number) => (
        <div style={{ textAlign: 'center' }}>
          <div style={{ fontSize: '16px', fontWeight: 600, color: '#52c41a' }}>
            +{percent.toFixed(1)}%
          </div>
        </div>
      ),
      sorter: (a: any, b: any) => a.expectedImprovementPercent - b.expectedImprovementPercent,
    },
    {
      title: 'Confidence',
      dataIndex: 'confidenceScore',
      key: 'confidence',
      render: (score: number) => (
        <div style={{ display: 'flex', alignItems: 'center', gap: '8px' }}>
          {getConfidenceIcon(score)}
          <span>{(score * 100).toFixed(1)}%</span>
        </div>
      ),
      sorter: (a: any, b: any) => a.confidenceScore - b.confidenceScore,
    },
    {
      title: 'Data Points',
      dataIndex: 'basedOnDataPoints',
      key: 'dataPoints',
      render: (points: number) => (
        <Text type="secondary">{points.toLocaleString()}</Text>
      ),
    },
    {
      title: 'Status',
      dataIndex: 'status',
      key: 'status',
      render: (status: SuggestionStatus) => (
        <Tag color={getStatusColor(status)}>{status.replace(/([A-Z])/g, ' $1').trim()}</Tag>
      ),
    },
    {
      title: 'Actions',
      key: 'actions',
      render: (record: TemplateImprovementSuggestion) => (
        <Button
          size="small"
          type="primary"
          onClick={() => {
            setSelectedSuggestion(record)
            setReviewModalVisible(true)
          }}
        >
          Review
        </Button>
      ),
    },
  ]

  return (
    <div className="template-improvement-dashboard">
      {/* Header */}
      <div style={{ marginBottom: '24px' }}>
        <Title level={2}>Template Improvement Dashboard</Title>
        <Text type="secondary">
          AI-powered template optimization and performance enhancement
        </Text>
      </div>

      {/* Template Selection */}
      <Card style={{ marginBottom: '24px' }}>
        <Row gutter={16} align="middle">
          <Col span={8}>
            <Text strong>Select Template:</Text>
            <Select
              style={{ width: '100%', marginTop: '8px' }}
              placeholder="Choose a template to analyze"
              value={selectedTemplate}
              onChange={(value) => {
                setSelectedTemplate(value)
                onTemplateSelect?.(value)
              }}
              showSearch
              filterOption={(input, option) =>
                option?.children?.toString().toLowerCase().includes(input.toLowerCase()) ?? false
              }
            >
              <Option value="sql_generation_basic">SQL Generation - Basic</Option>
              <Option value="insight_generation_advanced">Insight Generation - Advanced</Option>
              <Option value="explanation_detailed">Explanation - Detailed</Option>
              <Option value="data_analysis_comprehensive">Data Analysis - Comprehensive</Option>
            </Select>
          </Col>
          <Col span={16}>
            <Space>
              <Button
                type="primary"
                icon={<BarChartOutlined />}
                loading={analyzing}
                onClick={handleAnalyze}
                disabled={!selectedTemplate}
              >
                Analyze Performance
              </Button>
              <Button
                icon={<RocketOutlined />}
                loading={optimizing}
                onClick={handleOptimize}
                disabled={!selectedTemplate}
              >
                Optimize Template
              </Button>
              <Button
                icon={<ExperimentOutlined />}
                loading={generatingVariants}
                onClick={handleGenerateVariants}
                disabled={!selectedTemplate}
              >
                Generate Variants
              </Button>
            </Space>
          </Col>
        </Row>
      </Card>

      {/* Action Cards */}
      <Row gutter={16} style={{ marginBottom: '24px' }}>
        <Col span={8}>
          <Card title="Performance Analysis" size="small" style={{ height: '200px' }}>
            <div style={{ textAlign: 'center', padding: '20px 0' }}>
              <BarChartOutlined style={{ fontSize: '48px', color: '#1890ff', marginBottom: '16px' }} />
              <Paragraph>
                Analyze current template performance and identify improvement opportunities
              </Paragraph>
              <Button
                type="primary"
                loading={analyzing}
                onClick={handleAnalyze}
                disabled={!selectedTemplate}
                block
              >
                Analyze Performance
              </Button>
            </div>
          </Card>
        </Col>
        <Col span={8}>
          <Card title="Template Optimization" size="small" style={{ height: '200px' }}>
            <div style={{ marginBottom: '16px' }}>
              <Text strong>Optimization Strategy:</Text>
              <Select
                value={selectedStrategy}
                onChange={setSelectedStrategy}
                style={{ width: '100%', marginTop: '8px' }}
              >
                <Option value="PerformanceFocused">Performance Focused</Option>
                <Option value="AccuracyFocused">Accuracy Focused</Option>
                <Option value="UserSatisfactionFocused">User Satisfaction</Option>
                <Option value="ResponseTimeFocused">Response Time</Option>
                <Option value="Balanced">Balanced</Option>
              </Select>
            </div>
            <Button
              type="primary"
              icon={<BulbOutlined />}
              loading={optimizing}
              onClick={handleOptimize}
              disabled={!selectedTemplate}
              block
            >
              Optimize Template
            </Button>
          </Card>
        </Col>
        <Col span={8}>
          <Card title="A/B Testing" size="small" style={{ height: '200px' }}>
            <div style={{ textAlign: 'center', padding: '20px 0' }}>
              <ExperimentOutlined style={{ fontSize: '48px', color: '#722ed1', marginBottom: '16px' }} />
              <Paragraph>
                Generate template variants for A/B testing and performance comparison
              </Paragraph>
              <Button
                type="primary"
                loading={generatingVariants}
                onClick={handleGenerateVariants}
                disabled={!selectedTemplate}
                block
              >
                Create A/B Test
              </Button>
            </div>
          </Card>
        </Col>
      </Row>

      {/* Improvement Suggestions Table */}
      {suggestions && suggestions.length > 0 && (
        <Card title="Improvement Suggestions" style={{ marginBottom: '24px' }}>
          <Table
            dataSource={suggestions}
            columns={suggestionColumns}
            rowKey="id"
            size="small"
            pagination={{ pageSize: 10 }}
            expandable={{
              expandedRowRender: (record) => (
                <div style={{ padding: '16px', backgroundColor: '#fafafa' }}>
                  <Row gutter={16}>
                    <Col span={12}>
                      <div>
                        <Text strong>Reasoning:</Text>
                        <Paragraph style={{ marginTop: '8px' }}>
                          {record.reasoningExplanation}
                        </Paragraph>
                      </div>
                    </Col>
                    <Col span={12}>
                      <div>
                        <Text strong>Suggested Changes:</Text>
                        <div style={{ marginTop: '8px', padding: '8px', backgroundColor: '#fff', border: '1px solid #d9d9d9', borderRadius: '4px' }}>
                          <pre style={{ margin: 0, fontSize: '12px' }}>
                            {JSON.stringify(JSON.parse(record.suggestedChanges), null, 2)}
                          </pre>
                        </div>
                      </div>
                    </Col>
                  </Row>
                </div>
              ),
            }}
          />
        </Card>
      )}

      {/* Loading States */}
      {analyzing && (
        <Card style={{ textAlign: 'center', marginBottom: '24px' }}>
          <Spin size="large" />
          <div style={{ marginTop: '16px' }}>
            <Text>Analyzing template performance...</Text>
          </div>
        </Card>
      )}

      {/* Review Suggestion Modal */}
      <Modal
        title="Review Improvement Suggestion"
        open={reviewModalVisible}
        onCancel={() => setReviewModalVisible(false)}
        footer={null}
        width={800}
      >
        {selectedSuggestion && (
          <div>
            <div style={{ marginBottom: '24px', padding: '16px', backgroundColor: '#f5f5f5', borderRadius: '8px' }}>
              <Row gutter={16}>
                <Col span={12}>
                  <div>
                    <Text strong>Improvement Type:</Text>
                    <div style={{ marginTop: '4px' }}>
                      <Tag color={getTypeColor(selectedSuggestion.type)}>
                        {selectedSuggestion.type.replace(/([A-Z])/g, ' $1').trim()}
                      </Tag>
                    </div>
                  </div>
                  <div style={{ marginTop: '12px' }}>
                    <Text strong>Expected Improvement:</Text>
                    <div style={{ fontSize: '18px', fontWeight: 600, color: '#52c41a' }}>
                      +{selectedSuggestion.expectedImprovementPercent.toFixed(1)}%
                    </div>
                  </div>
                </Col>
                <Col span={12}>
                  <div>
                    <Text strong>Confidence Score:</Text>
                    <div style={{ marginTop: '4px' }}>
                      <Progress
                        percent={selectedSuggestion.confidenceScore * 100}
                        size="small"
                        status={selectedSuggestion.confidenceScore >= 0.8 ? 'success' : 'normal'}
                      />
                    </div>
                  </div>
                  <div style={{ marginTop: '12px' }}>
                    <Text strong>Based on Data Points:</Text>
                    <div style={{ fontSize: '16px', fontWeight: 600 }}>
                      {selectedSuggestion.basedOnDataPoints.toLocaleString()}
                    </div>
                  </div>
                </Col>
              </Row>
            </div>

            <div style={{ marginBottom: '24px' }}>
              <Text strong>Reasoning:</Text>
              <Paragraph style={{ marginTop: '8px' }}>
                {selectedSuggestion.reasoningExplanation}
              </Paragraph>
            </div>

            <div style={{ marginBottom: '24px' }}>
              <Text strong>Suggested Changes:</Text>
              <div style={{
                marginTop: '8px',
                padding: '12px',
                backgroundColor: '#fff',
                border: '1px solid #d9d9d9',
                borderRadius: '4px',
                maxHeight: '200px',
                overflow: 'auto'
              }}>
                <pre style={{ margin: 0, fontSize: '12px' }}>
                  {JSON.stringify(JSON.parse(selectedSuggestion.suggestedChanges), null, 2)}
                </pre>
              </div>
            </div>

            <Form onFinish={handleReviewSuggestion} layout="vertical">
              <Form.Item
                name="action"
                label="Review Action"
                rules={[{ required: true, message: 'Please select an action' }]}
              >
                <Select placeholder="Select action" size="large">
                  <Option value="Approve">
                    <CheckCircleOutlined style={{ color: '#52c41a', marginRight: '8px' }} />
                    Approve
                  </Option>
                  <Option value="Reject">
                    <ExclamationCircleOutlined style={{ color: '#ff4d4f', marginRight: '8px' }} />
                    Reject
                  </Option>
                  <Option value="RequestChanges">
                    <InfoCircleOutlined style={{ color: '#1890ff', marginRight: '8px' }} />
                    Request Changes
                  </Option>
                  <Option value="ScheduleABTest">
                    <ExperimentOutlined style={{ color: '#722ed1', marginRight: '8px' }} />
                    Schedule A/B Test
                  </Option>
                </Select>
              </Form.Item>

              <Form.Item name="comments" label="Comments">
                <TextArea
                  rows={4}
                  placeholder="Add review comments..."
                  showCount
                  maxLength={500}
                />
              </Form.Item>

              <Form.Item>
                <Space>
                  <Button type="primary" htmlType="submit" loading={reviewing}>
                    Submit Review
                  </Button>
                  <Button onClick={() => setReviewModalVisible(false)}>
                    Cancel
                  </Button>
                </Space>
              </Form.Item>
            </Form>
          </div>
        )}
      </Modal>

      {/* Optimization Results Modal */}
      <Modal
        title="Template Optimization Results"
        open={optimizationModalVisible}
        onCancel={() => setOptimizationModalVisible(false)}
        footer={null}
        width={1200}
      >
        {optimizedTemplate && (
          <div>
            <div style={{ marginBottom: '24px' }}>
              <Row gutter={16}>
                <Col span={8}>
                  <Card size="small" title="Expected Improvement">
                    <div style={{ textAlign: 'center' }}>
                      <div style={{ fontSize: '24px', fontWeight: 600, color: '#52c41a' }}>
                        +{optimizedTemplate.expectedPerformanceImprovement.toFixed(1)}%
                      </div>
                    </div>
                  </Card>
                </Col>
                <Col span={8}>
                  <Card size="small" title="Confidence Score">
                    <div style={{ textAlign: 'center' }}>
                      <Progress
                        type="circle"
                        percent={optimizedTemplate.confidenceScore * 100}
                        size={80}
                        status={optimizedTemplate.confidenceScore >= 0.8 ? 'success' : 'normal'}
                      />
                    </div>
                  </Card>
                </Col>
                <Col span={8}>
                  <Card size="small" title="Strategy Used">
                    <div style={{ textAlign: 'center' }}>
                      <Tag color="blue" style={{ fontSize: '14px', padding: '4px 12px' }}>
                        {optimizedTemplate.strategyUsed.replace(/([A-Z])/g, ' $1').trim()}
                      </Tag>
                    </div>
                  </Card>
                </Col>
              </Row>
            </div>

            <div style={{ marginBottom: '24px' }}>
              <Text strong>Optimization Reasoning:</Text>
              <Paragraph style={{ marginTop: '8px' }}>
                {optimizedTemplate.optimizationReasoning}
              </Paragraph>
            </div>

            <Row gutter={16}>
              <Col span={12}>
                <Card title="Original Template" size="small">
                  <div style={{
                    padding: '12px',
                    backgroundColor: '#f5f5f5',
                    borderRadius: '4px',
                    maxHeight: '300px',
                    overflow: 'auto'
                  }}>
                    <Text code>Original template content would be displayed here</Text>
                  </div>
                </Card>
              </Col>
              <Col span={12}>
                <Card title="Optimized Template" size="small">
                  <div style={{
                    padding: '12px',
                    backgroundColor: '#f0f9ff',
                    borderRadius: '4px',
                    maxHeight: '300px',
                    overflow: 'auto'
                  }}>
                    <pre style={{ margin: 0, fontSize: '12px', whiteSpace: 'pre-wrap' }}>
                      {optimizedTemplate.optimizedContent}
                    </pre>
                  </div>
                </Card>
              </Col>
            </Row>

            <div style={{ marginTop: '24px' }}>
              <Text strong>Changes Applied:</Text>
              <div style={{ marginTop: '12px' }}>
                {optimizedTemplate.changesApplied.map((change, index) => (
                  <Card key={index} size="small" style={{ marginBottom: '8px' }}>
                    <Row gutter={16} align="middle">
                      <Col span={4}>
                        <Tag color="blue">{change.changeType}</Tag>
                      </Col>
                      <Col span={16}>
                        <Text>{change.description}</Text>
                      </Col>
                      <Col span={4}>
                        <div style={{ textAlign: 'right' }}>
                          <StarOutlined style={{ color: '#faad14', marginRight: '4px' }} />
                          <Text strong>{(change.impactScore * 100).toFixed(0)}%</Text>
                        </div>
                      </Col>
                    </Row>
                  </Card>
                ))}
              </div>
            </div>

            <div style={{ marginTop: '24px', textAlign: 'right' }}>
              <Space>
                <Button onClick={() => setOptimizationModalVisible(false)}>
                  Close
                </Button>
                <Button type="primary">
                  Apply Optimization
                </Button>
                <Button>
                  Create A/B Test
                </Button>
              </Space>
            </div>
          </div>
        )}
      </Modal>

      {/* Template Variants Modal */}
      <Modal
        title="Generated Template Variants"
        open={variantsModalVisible}
        onCancel={() => setVariantsModalVisible(false)}
        footer={null}
        width={1200}
      >
        {variants && (
          <div>
            <div style={{ marginBottom: '24px' }}>
              <Text type="secondary">
                Generated {variants.length} template variants for A/B testing.
                Each variant is designed to test different aspects of template performance.
              </Text>
            </div>

            <Row gutter={16}>
              {variants.map((variant, index) => (
                <Col span={8} key={index}>
                  <Card
                    title={`Variant ${index + 1}`}
                    size="small"
                    extra={
                      <Tag color="purple">
                        {variant.variantType.replace(/([A-Z])/g, ' $1').trim()}
                      </Tag>
                    }
                  >
                    <div style={{ marginBottom: '12px' }}>
                      <Text strong>Expected Change:</Text>
                      <div style={{
                        fontSize: '16px',
                        fontWeight: 600,
                        color: variant.expectedPerformanceChange >= 0 ? '#52c41a' : '#ff4d4f'
                      }}>
                        {variant.expectedPerformanceChange >= 0 ? '+' : ''}
                        {variant.expectedPerformanceChange.toFixed(1)}%
                      </div>
                    </div>

                    <div style={{ marginBottom: '12px' }}>
                      <Text strong>Confidence:</Text>
                      <Progress
                        percent={variant.confidenceScore * 100}
                        size="small"
                        status={variant.confidenceScore >= 0.7 ? 'success' : 'normal'}
                      />
                    </div>

                    <div style={{ marginBottom: '12px' }}>
                      <Text strong>Reasoning:</Text>
                      <Paragraph
                        ellipsis={{ rows: 3, expandable: true }}
                        style={{ fontSize: '12px', marginTop: '4px' }}
                      >
                        {variant.generationReasoning}
                      </Paragraph>
                    </div>

                    <div style={{
                      padding: '8px',
                      backgroundColor: '#f5f5f5',
                      borderRadius: '4px',
                      maxHeight: '150px',
                      overflow: 'auto'
                    }}>
                      <pre style={{ margin: 0, fontSize: '11px', whiteSpace: 'pre-wrap' }}>
                        {variant.variantContent.substring(0, 200)}
                        {variant.variantContent.length > 200 && '...'}
                      </pre>
                    </div>

                    <div style={{ marginTop: '12px', textAlign: 'center' }}>
                      <Button size="small" type="primary">
                        Create A/B Test
                      </Button>
                    </div>
                  </Card>
                </Col>
              ))}
            </Row>

            <div style={{ marginTop: '24px', textAlign: 'right' }}>
              <Space>
                <Button onClick={() => setVariantsModalVisible(false)}>
                  Close
                </Button>
                <Button type="primary">
                  Create A/B Tests for All Variants
                </Button>
              </Space>
            </div>
          </div>
        )}
      </Modal>
    </div>
  )
}

export default TemplateImprovementDashboard
