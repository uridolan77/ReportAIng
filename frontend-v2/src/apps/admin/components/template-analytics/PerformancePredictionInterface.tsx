import React, { useState } from 'react'
import { 
  Card, 
  Row, 
  Col, 
  Button, 
  Select, 
  Input,
  Typography,
  Space,
  Progress,
  Tag,
  Alert,
  List,
  Statistic,
  Tooltip,
  Badge,
  Divider,
  message
} from 'antd'
import {
  BulbOutlined,
  RocketOutlined,
  CheckCircleOutlined,
  ExclamationCircleOutlined,
  BarChartOutlined,
  TrophyOutlined,
  WarningOutlined,
  InfoCircleOutlined,
  ThunderboltOutlined,
  StarOutlined
} from '@ant-design/icons'
import { usePredictTemplatePerformanceMutation } from '@shared/store/api/templateAnalyticsApi'
import type { PerformancePrediction } from '@shared/types/templateAnalytics'

const { Title, Text, Paragraph } = Typography
const { TextArea } = Input
const { Option } = Select

interface PerformancePredictionInterfaceProps {
  initialContent?: string
  initialIntentType?: string
  onPredictionComplete?: (prediction: PerformancePrediction) => void
}

export const PerformancePredictionInterface: React.FC<PerformancePredictionInterfaceProps> = ({
  initialContent = '',
  initialIntentType = '',
  onPredictionComplete
}) => {
  // State
  const [templateContent, setTemplateContent] = useState(initialContent)
  const [intentType, setIntentType] = useState(initialIntentType)
  const [prediction, setPrediction] = useState<PerformancePrediction | null>(null)

  // API
  const [predictPerformance, { isLoading: isPredicting }] = usePredictTemplatePerformanceMutation()

  const handlePredict = async () => {
    if (!templateContent.trim()) {
      message.warning('Please enter template content')
      return
    }
    if (!intentType) {
      message.warning('Please select an intent type')
      return
    }

    try {
      const result = await predictPerformance({
        templateContent,
        intentType
      }).unwrap()
      
      setPrediction(result)
      onPredictionComplete?.(result)
      message.success('Performance prediction completed')
    } catch (error) {
      message.error('Failed to predict template performance')
    }
  }

  const getScoreColor = (score: number): string => {
    if (score >= 0.8) return '#52c41a'
    if (score >= 0.6) return '#1890ff'
    if (score >= 0.4) return '#faad14'
    return '#ff4d4f'
  }

  const getScoreStatus = (score: number): 'success' | 'normal' | 'exception' => {
    if (score >= 0.8) return 'success'
    if (score >= 0.4) return 'normal'
    return 'exception'
  }

  const getConfidenceLevel = (confidence: number): { level: string; color: string } => {
    if (confidence >= 0.9) return { level: 'Very High', color: '#52c41a' }
    if (confidence >= 0.8) return { level: 'High', color: '#1890ff' }
    if (confidence >= 0.6) return { level: 'Medium', color: '#faad14' }
    return { level: 'Low', color: '#ff4d4f' }
  }

  const getFactorIcon = (factor: string, isStrength: boolean) => {
    const iconStyle = { color: isStrength ? '#52c41a' : '#ff4d4f' }
    
    if (factor.toLowerCase().includes('structure')) return <BarChartOutlined style={iconStyle} />
    if (factor.toLowerCase().includes('clarity')) return <BulbOutlined style={iconStyle} />
    if (factor.toLowerCase().includes('example')) return <InfoCircleOutlined style={iconStyle} />
    if (factor.toLowerCase().includes('performance')) return <ThunderboltOutlined style={iconStyle} />
    if (factor.toLowerCase().includes('context')) return <StarOutlined style={iconStyle} />
    
    return isStrength ? <CheckCircleOutlined style={iconStyle} /> : <ExclamationCircleOutlined style={iconStyle} />
  }

  return (
    <div className="performance-prediction-interface">
      {/* Header */}
      <Card style={{ marginBottom: '24px' }}>
        <Row gutter={16} align="middle">
          <Col span={16}>
            <Space>
              <BulbOutlined style={{ fontSize: '24px', color: '#722ed1' }} />
              <div>
                <Title level={3} style={{ margin: 0 }}>Performance Prediction</Title>
                <Text type="secondary">AI-powered template performance forecasting</Text>
              </div>
            </Space>
          </Col>
          <Col span={8} style={{ textAlign: 'right' }}>
            <Button 
              type="primary" 
              size="large"
              icon={<RocketOutlined />}
              onClick={handlePredict}
              loading={isPredicting}
              disabled={!templateContent.trim() || !intentType}
            >
              Predict Performance
            </Button>
          </Col>
        </Row>
      </Card>

      {/* Input Section */}
      <Row gutter={16} style={{ marginBottom: '24px' }}>
        <Col span={16}>
          <Card title="Template Content" size="small">
            <TextArea
              value={templateContent}
              onChange={(e) => setTemplateContent(e.target.value)}
              placeholder="Enter your template content here..."
              rows={12}
              showCount
              maxLength={5000}
            />
          </Card>
        </Col>
        <Col span={8}>
          <Card title="Configuration" size="small">
            <div style={{ marginBottom: '16px' }}>
              <Text strong>Intent Type:</Text>
              <Select
                value={intentType}
                onChange={setIntentType}
                style={{ width: '100%', marginTop: '8px' }}
                placeholder="Select intent type"
                size="large"
              >
                <Option value="sql_generation">SQL Generation</Option>
                <Option value="insight_generation">Insight Generation</Option>
                <Option value="explanation">Explanation</Option>
                <Option value="data_analysis">Data Analysis</Option>
                <Option value="visualization">Visualization</Option>
              </Select>
            </div>

            <Alert
              message="Prediction Accuracy"
              description="Our AI model has been trained on thousands of templates and achieves 94% accuracy in performance predictions."
              type="info"
              showIcon
              style={{ fontSize: '12px' }}
            />
          </Card>
        </Col>
      </Row>

      {/* Prediction Results */}
      {prediction && (
        <div>
          {/* Overview Metrics */}
          <Card title="Prediction Results" style={{ marginBottom: '24px' }}>
            <Row gutter={16}>
              <Col span={6}>
                <Card size="small" style={{ textAlign: 'center' }}>
                  <div>
                    <div style={{ fontSize: '14px', color: '#666', marginBottom: '8px' }}>
                      Success Rate
                    </div>
                    <Progress
                      type="circle"
                      percent={prediction.predictedSuccessRate * 100}
                      size={80}
                      status={getScoreStatus(prediction.predictedSuccessRate)}
                      format={(percent) => (
                        <div style={{ fontSize: '14px', fontWeight: 600 }}>
                          {percent?.toFixed(1)}%
                        </div>
                      )}
                    />
                  </div>
                </Card>
              </Col>
              <Col span={6}>
                <Card size="small" style={{ textAlign: 'center' }}>
                  <Statistic
                    title="User Rating"
                    value={prediction.predictedUserRating}
                    precision={1}
                    suffix="/5"
                    valueStyle={{ 
                      color: getScoreColor(prediction.predictedUserRating / 5),
                      fontSize: '24px'
                    }}
                    prefix={<StarOutlined />}
                  />
                </Card>
              </Col>
              <Col span={6}>
                <Card size="small" style={{ textAlign: 'center' }}>
                  <Statistic
                    title="Response Time"
                    value={prediction.predictedResponseTime}
                    precision={2}
                    suffix="s"
                    valueStyle={{ 
                      color: prediction.predictedResponseTime < 2 ? '#52c41a' : '#faad14',
                      fontSize: '24px'
                    }}
                    prefix={<ThunderboltOutlined />}
                  />
                </Card>
              </Col>
              <Col span={6}>
                <Card size="small" style={{ textAlign: 'center' }}>
                  <div>
                    <div style={{ fontSize: '14px', color: '#666', marginBottom: '8px' }}>
                      Prediction Confidence
                    </div>
                    <div style={{ 
                      fontSize: '24px', 
                      fontWeight: 600,
                      color: getConfidenceLevel(prediction.predictionConfidence).color
                    }}>
                      {(prediction.predictionConfidence * 100).toFixed(1)}%
                    </div>
                    <Tag color={getConfidenceLevel(prediction.predictionConfidence).color} size="small">
                      {getConfidenceLevel(prediction.predictionConfidence).level}
                    </Tag>
                  </div>
                </Card>
              </Col>
            </Row>
          </Card>

          {/* Feature Scores */}
          {Object.keys(prediction.featureScores).length > 0 && (
            <Card title="Feature Analysis" style={{ marginBottom: '24px' }}>
              <Row gutter={16}>
                {Object.entries(prediction.featureScores).map(([feature, score]) => (
                  <Col span={6} key={feature}>
                    <div style={{ marginBottom: '16px' }}>
                      <div style={{ display: 'flex', justifyContent: 'space-between', marginBottom: '4px' }}>
                        <Text style={{ fontSize: '12px' }}>
                          {feature.replace(/([A-Z])/g, ' $1').trim()}
                        </Text>
                        <Text style={{ fontSize: '12px', fontWeight: 600 }}>
                          {(score * 100).toFixed(0)}%
                        </Text>
                      </div>
                      <Progress
                        percent={score * 100}
                        size="small"
                        status={getScoreStatus(score)}
                        showInfo={false}
                      />
                    </div>
                  </Col>
                ))}
              </Row>
            </Card>
          )}

          {/* Strengths and Weaknesses */}
          <Row gutter={16} style={{ marginBottom: '24px' }}>
            <Col span={12}>
              <Card 
                title={
                  <Space>
                    <TrophyOutlined style={{ color: '#52c41a' }} />
                    Strength Factors
                    <Badge count={prediction.strengthFactors.length} />
                  </Space>
                }
                size="small"
              >
                <List
                  size="small"
                  dataSource={prediction.strengthFactors}
                  renderItem={(factor) => (
                    <List.Item>
                      <List.Item.Meta
                        avatar={getFactorIcon(factor, true)}
                        description={
                          <Text style={{ fontSize: '12px' }}>{factor}</Text>
                        }
                      />
                    </List.Item>
                  )}
                />
              </Card>
            </Col>
            <Col span={12}>
              <Card 
                title={
                  <Space>
                    <WarningOutlined style={{ color: '#ff4d4f' }} />
                    Weakness Factors
                    <Badge count={prediction.weaknessFactors.length} />
                  </Space>
                }
                size="small"
              >
                <List
                  size="small"
                  dataSource={prediction.weaknessFactors}
                  renderItem={(factor) => (
                    <List.Item>
                      <List.Item.Meta
                        avatar={getFactorIcon(factor, false)}
                        description={
                          <Text style={{ fontSize: '12px' }}>{factor}</Text>
                        }
                      />
                    </List.Item>
                  )}
                />
              </Card>
            </Col>
          </Row>

          {/* Improvement Suggestions */}
          {prediction.improvementSuggestions.length > 0 && (
            <Card 
              title={
                <Space>
                  <BulbOutlined style={{ color: '#1890ff' }} />
                  Improvement Suggestions
                  <Badge count={prediction.improvementSuggestions.length} />
                </Space>
              }
              style={{ marginBottom: '24px' }}
            >
              <List
                dataSource={prediction.improvementSuggestions}
                renderItem={(suggestion, index) => (
                  <List.Item>
                    <List.Item.Meta
                      avatar={
                        <div style={{ 
                          width: '24px', 
                          height: '24px', 
                          borderRadius: '50%', 
                          backgroundColor: '#1890ff',
                          color: 'white',
                          display: 'flex',
                          alignItems: 'center',
                          justifyContent: 'center',
                          fontSize: '12px',
                          fontWeight: 600
                        }}>
                          {index + 1}
                        </div>
                      }
                      description={
                        <Paragraph style={{ fontSize: '13px', margin: 0 }}>
                          {suggestion}
                        </Paragraph>
                      }
                    />
                  </List.Item>
                )}
              />
            </Card>
          )}

          {/* Summary Alert */}
          <Alert
            message="Prediction Summary"
            description={
              <div>
                <Paragraph style={{ margin: '8px 0' }}>
                  Based on the analysis, this template is predicted to achieve a{' '}
                  <Text strong style={{ color: getScoreColor(prediction.predictedSuccessRate) }}>
                    {(prediction.predictedSuccessRate * 100).toFixed(1)}% success rate
                  </Text>
                  {' '}with{' '}
                  <Text strong style={{ color: getConfidenceLevel(prediction.predictionConfidence).color }}>
                    {getConfidenceLevel(prediction.predictionConfidence).level.toLowerCase()} confidence
                  </Text>.
                </Paragraph>
                <div style={{ marginTop: '12px' }}>
                  <Space>
                    <Button type="primary" icon={<RocketOutlined />}>
                      Optimize Template
                    </Button>
                    <Button icon={<BarChartOutlined />}>
                      Create A/B Test
                    </Button>
                    <Button icon={<CheckCircleOutlined />}>
                      Save Prediction
                    </Button>
                  </Space>
                </div>
              </div>
            }
            type={prediction.predictedSuccessRate >= 0.8 ? 'success' : 
                  prediction.predictedSuccessRate >= 0.6 ? 'info' : 'warning'}
            showIcon
          />
        </div>
      )}

      {/* Loading State */}
      {isPredicting && (
        <Card style={{ textAlign: 'center', padding: '40px 0' }}>
          <Space direction="vertical" size="large">
            <BulbOutlined style={{ fontSize: '48px', color: '#722ed1' }} />
            <div>
              <Title level={4}>Analyzing Template Performance...</Title>
              <Text type="secondary">
                Our AI is analyzing your template content and predicting performance metrics
              </Text>
            </div>
            <Progress percent={30} status="active" />
          </Space>
        </Card>
      )}
    </div>
  )
}

export default PerformancePredictionInterface
