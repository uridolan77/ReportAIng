import React, { useState } from 'react'
import { 
  Card, 
  Row, 
  Col, 
  Button, 
  Select, 
  Typography,
  Space,
  Divider,
  Progress,
  Tag,
  Alert,
  Tooltip,
  Collapse,
  List,
  Statistic,
  message,
  Modal
} from 'antd'
import {
  RocketOutlined,
  BulbOutlined,
  SwapOutlined,
  StarOutlined,
  CheckCircleOutlined,
  ExclamationCircleOutlined,
  InfoCircleOutlined,
  ThunderboltOutlined,
  BarChartOutlined,
  TrophyOutlined,
  ExperimentOutlined
} from '@ant-design/icons'
import { useOptimizeTemplateMutation } from '@shared/store/api/templateAnalyticsApi'
import type {
  OptimizedTemplate,
  OptimizationStrategy,
  OptimizationChange
} from '@shared/types/templateAnalytics'

const { Title, Text, Paragraph } = Typography
const { Option } = Select


interface TemplateOptimizationInterfaceProps {
  templateKey: string
  templateName: string
  originalContent: string
  onOptimizationComplete?: (result: OptimizedTemplate) => void
  onCreateABTest?: (originalContent: string, optimizedContent: string) => void
}

export const TemplateOptimizationInterface: React.FC<TemplateOptimizationInterfaceProps> = ({
  templateKey,
  templateName,
  originalContent,
  onOptimizationComplete,
  onCreateABTest
}) => {
  // State
  const [selectedStrategy, setSelectedStrategy] = useState<OptimizationStrategy>('Balanced')
  const [optimizationResult, setOptimizationResult] = useState<OptimizedTemplate | null>(null)
  const [showComparison, setShowComparison] = useState(false)
  const [previewModalVisible, setPreviewModalVisible] = useState(false)

  // API
  const [optimizeTemplate, { isLoading: isOptimizing }] = useOptimizeTemplateMutation()

  // Strategy descriptions
  const strategyDescriptions = {
    PerformanceFocused: {
      description: 'Optimizes for maximum performance metrics including success rate and response time',
      focus: ['Success Rate', 'Response Time', 'Error Reduction'],
      color: '#52c41a'
    },
    AccuracyFocused: {
      description: 'Prioritizes accuracy and precision of generated outputs',
      focus: ['Output Accuracy', 'Precision', 'Consistency'],
      color: '#1890ff'
    },
    UserSatisfactionFocused: {
      description: 'Optimizes for user experience and satisfaction metrics',
      focus: ['User Rating', 'Usability', 'Clarity'],
      color: '#722ed1'
    },
    ResponseTimeFocused: {
      description: 'Minimizes response time while maintaining quality',
      focus: ['Speed', 'Efficiency', 'Quick Response'],
      color: '#fa8c16'
    },
    Balanced: {
      description: 'Balances all optimization factors for overall improvement',
      focus: ['Overall Performance', 'Quality', 'User Experience'],
      color: '#13c2c2'
    }
  }

  const handleOptimize = async () => {
    try {
      const result = await optimizeTemplate({
        templateKey,
        strategy: selectedStrategy
      }).unwrap()
      
      setOptimizationResult(result)
      setShowComparison(true)
      onOptimizationComplete?.(result)
      message.success('Template optimization completed successfully')
    } catch (error) {
      message.error('Failed to optimize template')
    }
  }

  const handleCreateABTest = () => {
    if (optimizationResult) {
      onCreateABTest?.(originalContent, optimizationResult.optimizedContent)
      message.success('A/B test creation initiated')
    }
  }

  const handlePreviewOptimization = () => {
    setPreviewModalVisible(true)
  }

  const getChangeTypeIcon = (changeType: string) => {
    switch (changeType.toLowerCase()) {
      case 'content': return <BulbOutlined />
      case 'structure': return <BarChartOutlined />
      case 'performance': return <ThunderboltOutlined />
      case 'clarity': return <CheckCircleOutlined />
      default: return <InfoCircleOutlined />
    }
  }

  const getChangeTypeColor = (changeType: string) => {
    switch (changeType.toLowerCase()) {
      case 'content': return '#1890ff'
      case 'structure': return '#52c41a'
      case 'performance': return '#fa8c16'
      case 'clarity': return '#722ed1'
      default: return '#666666'
    }
  }

  const getImpactLevel = (score: number) => {
    if (score >= 0.8) return { level: 'High', color: '#52c41a' }
    if (score >= 0.6) return { level: 'Medium', color: '#fa8c16' }
    return { level: 'Low', color: '#1890ff' }
  }

  return (
    <div className="template-optimization-interface">
      {/* Header */}
      <Card style={{ marginBottom: '24px' }}>
        <Row gutter={16} align="middle">
          <Col span={16}>
            <Space>
              <RocketOutlined style={{ fontSize: '24px', color: '#1890ff' }} />
              <div>
                <Title level={3} style={{ margin: 0 }}>Template Optimization</Title>
                <Text type="secondary">{templateName}</Text>
              </div>
            </Space>
          </Col>
          <Col span={8} style={{ textAlign: 'right' }}>
            <Space>
              <Button
                icon={<SwapOutlined />}
                onClick={handlePreviewOptimization}
                disabled={!optimizationResult}
              >
                Preview
              </Button>
              <Button 
                type="primary" 
                icon={<RocketOutlined />}
                onClick={handleOptimize}
                loading={isOptimizing}
              >
                Optimize Template
              </Button>
            </Space>
          </Col>
        </Row>
      </Card>

      {/* Strategy Selection */}
      <Card title="Optimization Strategy" style={{ marginBottom: '24px' }}>
        <Row gutter={16}>
          <Col span={8}>
            <div>
              <Text strong>Select Strategy:</Text>
              <Select
                value={selectedStrategy}
                onChange={setSelectedStrategy}
                style={{ width: '100%', marginTop: '8px' }}
                size="large"
              >
                {Object.entries(strategyDescriptions).map(([strategy, info]) => (
                  <Option key={strategy} value={strategy}>
                    <Space>
                      <div style={{ 
                        width: '8px', 
                        height: '8px', 
                        borderRadius: '50%', 
                        backgroundColor: info.color 
                      }} />
                      {strategy.replace(/([A-Z])/g, ' $1').trim()}
                    </Space>
                  </Option>
                ))}
              </Select>
            </div>
          </Col>
          <Col span={16}>
            <div style={{ 
              padding: '16px', 
              backgroundColor: '#f5f5f5', 
              borderRadius: '8px',
              border: `2px solid ${strategyDescriptions[selectedStrategy].color}`
            }}>
              <div style={{ marginBottom: '12px' }}>
                <Text strong style={{ color: strategyDescriptions[selectedStrategy].color }}>
                  {selectedStrategy.replace(/([A-Z])/g, ' $1').trim()} Strategy
                </Text>
              </div>
              <Paragraph style={{ margin: '0 0 12px 0', fontSize: '14px' }}>
                {strategyDescriptions[selectedStrategy].description}
              </Paragraph>
              <div>
                <Text strong style={{ fontSize: '12px' }}>Focus Areas:</Text>
                <div style={{ marginTop: '4px' }}>
                  {strategyDescriptions[selectedStrategy].focus.map(area => (
                    <Tag 
                      key={area} 
                      color={strategyDescriptions[selectedStrategy].color}
                      style={{ margin: '2px' }}
                    >
                      {area}
                    </Tag>
                  ))}
                </div>
              </div>
            </div>
          </Col>
        </Row>
      </Card>

      {/* Optimization Results */}
      {optimizationResult && (
        <div>
          {/* Results Overview */}
          <Card title="Optimization Results" style={{ marginBottom: '24px' }}>
            <Row gutter={16}>
              <Col span={6}>
                <Card size="small" style={{ textAlign: 'center' }}>
                  <Statistic
                    title="Expected Improvement"
                    value={optimizationResult.expectedPerformanceImprovement}
                    precision={1}
                    suffix="%"
                    prefix="+"
                    valueStyle={{ color: '#52c41a', fontSize: '24px' }}
                  />
                </Card>
              </Col>
              <Col span={6}>
                <Card size="small" style={{ textAlign: 'center' }}>
                  <div>
                    <div style={{ fontSize: '14px', color: '#666', marginBottom: '8px' }}>
                      Confidence Score
                    </div>
                    <Progress
                      type="circle"
                      percent={optimizationResult.confidenceScore * 100}
                      size={80}
                      status={optimizationResult.confidenceScore >= 0.8 ? 'success' : 'normal'}
                    />
                  </div>
                </Card>
              </Col>
              <Col span={6}>
                <Card size="small" style={{ textAlign: 'center' }}>
                  <Statistic
                    title="Strategy Used"
                    value={optimizationResult.strategyUsed.replace(/([A-Z])/g, ' $1').trim()}
                    valueStyle={{ fontSize: '16px' }}
                  />
                </Card>
              </Col>
              <Col span={6}>
                <Card size="small" style={{ textAlign: 'center' }}>
                  <Statistic
                    title="Changes Applied"
                    value={optimizationResult.changesApplied.length}
                    valueStyle={{ color: '#1890ff', fontSize: '24px' }}
                  />
                </Card>
              </Col>
            </Row>
          </Card>

          {/* Optimization Reasoning */}
          <Card title="Optimization Reasoning" style={{ marginBottom: '24px' }}>
            <Alert
              message="AI Optimization Analysis"
              description={optimizationResult.optimizationReasoning}
              type="info"
              showIcon
              icon={<BulbOutlined />}
            />
          </Card>

          {/* Changes Applied */}
          <Card title="Changes Applied" style={{ marginBottom: '24px' }}>
            <List
              dataSource={optimizationResult.changesApplied}
              renderItem={(change: OptimizationChange, index: number) => {
                const impact = getImpactLevel(change.impactScore)
                return (
                  <List.Item>
                    <List.Item.Meta
                      avatar={
                        <div style={{ 
                          width: '40px', 
                          height: '40px', 
                          borderRadius: '50%', 
                          backgroundColor: getChangeTypeColor(change.changeType),
                          display: 'flex',
                          alignItems: 'center',
                          justifyContent: 'center',
                          color: 'white'
                        }}>
                          {getChangeTypeIcon(change.changeType)}
                        </div>
                      }
                      title={
                        <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                          <Space>
                            <Text strong>{change.changeType}</Text>
                            <Tag color={impact.color}>{impact.level} Impact</Tag>
                          </Space>
                          <div style={{ display: 'flex', alignItems: 'center', gap: '8px' }}>
                            <StarOutlined style={{ color: '#faad14' }} />
                            <Text strong>{(change.impactScore * 100).toFixed(0)}%</Text>
                          </div>
                        </div>
                      }
                      description={change.description}
                    />
                  </List.Item>
                )
              }}
            />
          </Card>

          {/* Metric Predictions */}
          {Object.keys(optimizationResult.metricPredictions).length > 0 && (
            <Card title="Predicted Metrics" style={{ marginBottom: '24px' }}>
              <Row gutter={16}>
                {Object.entries(optimizationResult.metricPredictions).map(([metric, value]) => (
                  <Col span={6} key={metric}>
                    <Card size="small" style={{ textAlign: 'center' }}>
                      <Statistic
                        title={metric.replace(/([A-Z])/g, ' $1').trim()}
                        value={typeof value === 'number' ? value : parseFloat(value as string)}
                        precision={metric.includes('Rate') || metric.includes('Score') ? 1 : 2}
                        suffix={metric.includes('Rate') || metric.includes('Score') ? '%' : 
                               metric.includes('Time') ? 's' : ''}
                        valueStyle={{ 
                          color: typeof value === 'number' && value > 0 ? '#52c41a' : '#1890ff' 
                        }}
                      />
                    </Card>
                  </Col>
                ))}
              </Row>
            </Card>
          )}

          {/* Action Buttons */}
          <Card>
            <Row gutter={16} justify="center">
              <Col>
                <Button 
                  type="primary" 
                  size="large"
                  icon={<CheckCircleOutlined />}
                >
                  Apply Optimization
                </Button>
              </Col>
              <Col>
                <Button 
                  size="large"
                  icon={<ExperimentOutlined />}
                  onClick={handleCreateABTest}
                >
                  Create A/B Test
                </Button>
              </Col>
              <Col>
                <Button
                  size="large"
                  icon={<SwapOutlined />}
                  onClick={() => setShowComparison(!showComparison)}
                >
                  {showComparison ? 'Hide' : 'Show'} Comparison
                </Button>
              </Col>
            </Row>
          </Card>
        </div>
      )}

      {/* Preview Modal */}
      <Modal
        title="Optimization Preview"
        open={previewModalVisible}
        onCancel={() => setPreviewModalVisible(false)}
        footer={null}
        width={1200}
      >
        {optimizationResult && (
          <Row gutter={16}>
            <Col span={12}>
              <Card title="Original Template" size="small">
                <div style={{ 
                  padding: '12px', 
                  backgroundColor: '#f5f5f5', 
                  borderRadius: '4px',
                  maxHeight: '400px',
                  overflow: 'auto',
                  fontFamily: 'monospace',
                  fontSize: '12px',
                  whiteSpace: 'pre-wrap'
                }}>
                  {originalContent}
                </div>
              </Card>
            </Col>
            <Col span={12}>
              <Card title="Optimized Template" size="small">
                <div style={{ 
                  padding: '12px', 
                  backgroundColor: '#f0f9ff', 
                  borderRadius: '4px',
                  maxHeight: '400px',
                  overflow: 'auto',
                  fontFamily: 'monospace',
                  fontSize: '12px',
                  whiteSpace: 'pre-wrap'
                }}>
                  {optimizationResult.optimizedContent}
                </div>
              </Card>
            </Col>
          </Row>
        )}
      </Modal>
    </div>
  )
}

export default TemplateOptimizationInterface
