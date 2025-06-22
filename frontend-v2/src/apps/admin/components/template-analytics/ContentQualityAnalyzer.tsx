import React, { useState } from 'react'
import { 
  Card, 
  Button, 
  Input, 
  Progress, 
  List, 
  Tag, 
  Row, 
  Col, 
  Typography,
  Space,
  Divider,
  Alert,
  Tooltip,
  Badge,
  Statistic,
  message
} from 'antd'
import { 
  CheckCircleOutlined, 
  ExclamationCircleOutlined,
  InfoCircleOutlined,
  BulbOutlined,
  FileTextOutlined,
  BarChartOutlined,
  StarOutlined,
  WarningOutlined,
  ThunderboltOutlined
} from '@ant-design/icons'
import { useAnalyzeContentQualityMutation } from '@shared/store/api/templateAnalyticsApi'
import type {
  ContentQualityAnalysis,
  QualityIssue,
  QualityStrength,
  ReadabilityMetrics,
  StructureAnalysis,
  ContentCompleteness
} from '@shared/types/templateAnalytics'

const { TextArea } = Input
const { Title, Text, Paragraph } = Typography

interface ContentQualityAnalyzerProps {
  initialContent?: string
  onAnalysisComplete?: (analysis: ContentQualityAnalysis) => void
}

export const ContentQualityAnalyzer: React.FC<ContentQualityAnalyzerProps> = ({ 
  initialContent = '',
  onAnalysisComplete 
}) => {
  const [content, setContent] = useState(initialContent)
  const [analyzeQuality, { data: analysis, isLoading }] = useAnalyzeContentQualityMutation()

  const handleAnalyze = async () => {
    if (!content.trim()) {
      message.warning('Please enter template content to analyze')
      return
    }
    
    try {
      const result = await analyzeQuality({ templateContent: content }).unwrap()
      onAnalysisComplete?.(result)
      message.success('Content quality analysis completed')
    } catch (error) {
      message.error('Failed to analyze content quality')
    }
  }

  const getQualityColor = (score: number): string => {
    if (score >= 0.8) return '#52c41a'
    if (score >= 0.6) return '#1890ff'
    if (score >= 0.4) return '#faad14'
    return '#ff4d4f'
  }

  const getQualityStatus = (score: number): 'success' | 'normal' | 'exception' => {
    if (score >= 0.8) return 'success'
    if (score >= 0.4) return 'normal'
    return 'exception'
  }

  const getSeverityIcon = (severity: number) => {
    if (severity >= 0.8) return <WarningOutlined style={{ color: '#ff4d4f' }} />
    if (severity >= 0.6) return <ExclamationCircleOutlined style={{ color: '#faad14' }} />
    return <InfoCircleOutlined style={{ color: '#1890ff' }} />
  }

  const getImpactIcon = (impact: number) => {
    if (impact >= 0.8) return <ThunderboltOutlined style={{ color: '#52c41a' }} />
    if (impact >= 0.6) return <StarOutlined style={{ color: '#1890ff' }} />
    return <CheckCircleOutlined style={{ color: '#faad14' }} />
  }

  return (
    <div className="content-quality-analyzer">
      <Card title="Content Quality Analysis" style={{ marginBottom: '24px' }}>
        <Row gutter={24}>
          <Col span={12}>
            <div>
              <Title level={4}>Template Content</Title>
              <TextArea
                value={content}
                onChange={(e) => setContent(e.target.value)}
                placeholder="Enter template content to analyze..."
                rows={12}
                style={{ marginBottom: '16px' }}
                showCount
                maxLength={5000}
              />
              <Button
                type="primary"
                onClick={handleAnalyze}
                loading={isLoading}
                disabled={!content.trim()}
                block
                size="large"
                icon={<BarChartOutlined />}
              >
                Analyze Quality
              </Button>
            </div>
          </Col>

          <Col span={12}>
            {analysis ? (
              <div className="analysis-results">
                <Title level={4}>Quality Analysis Results</Title>

                {/* Overall Score */}
                <Card size="small" style={{ marginBottom: '16px' }}>
                  <div style={{ textAlign: 'center' }}>
                    <Progress
                      type="circle"
                      percent={analysis.overallQualityScore * 100}
                      status={getQualityStatus(analysis.overallQualityScore)}
                      size={120}
                      format={(percent) => (
                        <div>
                          <div style={{ fontSize: '24px', fontWeight: 600 }}>
                            {percent?.toFixed(0)}
                          </div>
                          <div style={{ fontSize: '12px', color: '#666' }}>
                            Quality Score
                          </div>
                        </div>
                      )}
                    />
                  </div>
                </Card>

                {/* Quality Dimensions */}
                <Card size="small" title="Quality Dimensions" style={{ marginBottom: '16px' }}>
                  <Space direction="vertical" style={{ width: '100%' }}>
                    {Object.entries(analysis.qualityDimensions).map(([dimension, score]) => (
                      <div key={dimension}>
                        <div style={{ display: 'flex', justifyContent: 'space-between', marginBottom: '4px' }}>
                          <Text style={{ fontSize: '12px' }}>
                            {dimension.replace(/([A-Z])/g, ' $1').trim()}
                          </Text>
                          <Text style={{ fontSize: '12px', fontWeight: 600 }}>
                            {(score * 100).toFixed(0)}%
                          </Text>
                        </div>
                        <Progress
                          percent={score * 100}
                          size="small"
                          status={getQualityStatus(score)}
                          showInfo={false}
                        />
                      </div>
                    ))}
                  </Space>
                </Card>

                {/* Readability Metrics */}
                <Card size="small" title="Readability Metrics" style={{ marginBottom: '16px' }}>
                  <Row gutter={8}>
                    <Col span={12}>
                      <Statistic
                        title="Overall Score"
                        value={analysis.readability.overallScore * 100}
                        precision={0}
                        suffix="%"
                        valueStyle={{ fontSize: '14px', color: getQualityColor(analysis.readability.overallScore) }}
                      />
                    </Col>
                    <Col span={12}>
                      <Statistic
                        title="Reading Level"
                        value={analysis.readability.readingLevel}
                        valueStyle={{ fontSize: '14px' }}
                      />
                    </Col>
                  </Row>
                  <Divider style={{ margin: '8px 0' }} />
                  <Row gutter={8}>
                    <Col span={12}>
                      <Text style={{ fontSize: '11px' }}>Sentence Complexity</Text>
                      <Progress 
                        percent={analysis.readability.sentenceComplexity * 100} 
                        size="small" 
                        showInfo={false}
                      />
                    </Col>
                    <Col span={12}>
                      <Text style={{ fontSize: '11px' }}>Vocabulary Level</Text>
                      <Progress 
                        percent={analysis.readability.vocabularyLevel * 100} 
                        size="small" 
                        showInfo={false}
                      />
                    </Col>
                  </Row>
                </Card>

                {/* Structure Analysis */}
                <Card size="small" title="Structure Analysis" style={{ marginBottom: '16px' }}>
                  <Row gutter={8}>
                    <Col span={12}>
                      <Statistic
                        title="Structure Score"
                        value={analysis.structure.overallScore * 100}
                        precision={0}
                        suffix="%"
                        valueStyle={{ fontSize: '14px', color: getQualityColor(analysis.structure.overallScore) }}
                      />
                    </Col>
                    <Col span={12}>
                      <Statistic
                        title="Complexity"
                        value={analysis.structure.complexityScore * 100}
                        precision={0}
                        suffix="%"
                        valueStyle={{ fontSize: '14px' }}
                      />
                    </Col>
                  </Row>
                  <Divider style={{ margin: '8px 0' }} />
                  <Space wrap>
                    <Badge 
                      status={analysis.structure.hasClearSections ? 'success' : 'error'} 
                      text={<Text style={{ fontSize: '11px' }}>Clear Sections</Text>}
                    />
                    <Badge 
                      status={analysis.structure.hasNumberedSteps ? 'success' : 'error'} 
                      text={<Text style={{ fontSize: '11px' }}>Numbered Steps</Text>}
                    />
                    <Badge 
                      status={analysis.structure.hasBulletPoints ? 'success' : 'error'} 
                      text={<Text style={{ fontSize: '11px' }}>Bullet Points</Text>}
                    />
                  </Space>
                </Card>

                {/* Content Completeness */}
                <Card size="small" title="Content Completeness">
                  <Row gutter={8}>
                    <Col span={24}>
                      <Statistic
                        title="Completeness Score"
                        value={analysis.completeness.overallScore * 100}
                        precision={0}
                        suffix="%"
                        valueStyle={{ fontSize: '14px', color: getQualityColor(analysis.completeness.overallScore) }}
                      />
                    </Col>
                  </Row>
                  <Divider style={{ margin: '8px 0' }} />
                  <Space wrap>
                    <Badge 
                      status={analysis.completeness.hasInstructions ? 'success' : 'error'} 
                      text={<Text style={{ fontSize: '11px' }}>Instructions</Text>}
                    />
                    <Badge 
                      status={analysis.completeness.hasExamples ? 'success' : 'error'} 
                      text={<Text style={{ fontSize: '11px' }}>Examples</Text>}
                    />
                    <Badge 
                      status={analysis.completeness.hasContext ? 'success' : 'error'} 
                      text={<Text style={{ fontSize: '11px' }}>Context</Text>}
                    />
                  </Space>
                  {analysis.completeness.missingElements.length > 0 && (
                    <div style={{ marginTop: '8px' }}>
                      <Text style={{ fontSize: '11px', color: '#ff4d4f' }}>
                        Missing: {analysis.completeness.missingElements.join(', ')}
                      </Text>
                    </div>
                  )}
                </Card>
              </div>
            ) : (
              <div style={{ textAlign: 'center', padding: '60px 20px' }}>
                <FileTextOutlined style={{ fontSize: '48px', color: '#d9d9d9', marginBottom: '16px' }} />
                <Text type="secondary">
                  Enter template content and click "Analyze Quality" to see detailed analysis results
                </Text>
              </div>
            )}
          </Col>
        </Row>
      </Card>

      {/* Detailed Analysis Results */}
      {analysis && (
        <Row gutter={16}>
          {/* Issues */}
          {analysis.identifiedIssues.length > 0 && (
            <Col span={8}>
              <Card title="Identified Issues" size="small" style={{ marginBottom: '16px' }}>
                <List
                  size="small"
                  dataSource={analysis.identifiedIssues}
                  renderItem={(issue: QualityIssue) => (
                    <List.Item>
                      <List.Item.Meta
                        avatar={getSeverityIcon(issue.severity)}
                        title={
                          <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                            <Text strong style={{ fontSize: '12px' }}>{issue.issueType}</Text>
                            <Tag
                              color={issue.severity >= 0.8 ? 'red' : issue.severity >= 0.6 ? 'orange' : 'blue'}
                              size="small"
                            >
                              {(issue.severity * 100).toFixed(0)}%
                            </Tag>
                          </div>
                        }
                        description={
                          <div>
                            <Paragraph
                              style={{ fontSize: '11px', margin: '4px 0' }}
                              ellipsis={{ rows: 2, expandable: true }}
                            >
                              {issue.description}
                            </Paragraph>
                            <Alert
                              message={issue.suggestion}
                              type="info"
                              showIcon
                              style={{ fontSize: '10px', padding: '4px 8px' }}
                              icon={<BulbOutlined />}
                            />
                          </div>
                        }
                      />
                    </List.Item>
                  )}
                />
              </Card>
            </Col>
          )}

          {/* Strengths */}
          {analysis.strengths.length > 0 && (
            <Col span={8}>
              <Card title="Strengths" size="small" style={{ marginBottom: '16px' }}>
                <List
                  size="small"
                  dataSource={analysis.strengths}
                  renderItem={(strength: QualityStrength) => (
                    <List.Item>
                      <List.Item.Meta
                        avatar={getImpactIcon(strength.impactScore)}
                        title={
                          <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                            <Text strong style={{ fontSize: '12px' }}>{strength.strengthType}</Text>
                            <Tag
                              color="green"
                              size="small"
                            >
                              {(strength.impactScore * 100).toFixed(0)}%
                            </Tag>
                          </div>
                        }
                        description={
                          <Paragraph
                            style={{ fontSize: '11px', margin: '4px 0' }}
                            ellipsis={{ rows: 2, expandable: true }}
                          >
                            {strength.description}
                          </Paragraph>
                        }
                      />
                    </List.Item>
                  )}
                />
              </Card>
            </Col>
          )}

          {/* Improvement Suggestions */}
          {analysis.improvementSuggestions.length > 0 && (
            <Col span={8}>
              <Card title="Improvement Suggestions" size="small" style={{ marginBottom: '16px' }}>
                <List
                  size="small"
                  dataSource={analysis.improvementSuggestions}
                  renderItem={(suggestion: string, index: number) => (
                    <List.Item>
                      <List.Item.Meta
                        avatar={
                          <div style={{
                            width: '20px',
                            height: '20px',
                            borderRadius: '50%',
                            backgroundColor: '#1890ff',
                            color: 'white',
                            display: 'flex',
                            alignItems: 'center',
                            justifyContent: 'center',
                            fontSize: '10px',
                            fontWeight: 600
                          }}>
                            {index + 1}
                          </div>
                        }
                        description={
                          <Paragraph
                            style={{ fontSize: '11px', margin: 0 }}
                            ellipsis={{ rows: 3, expandable: true }}
                          >
                            {suggestion}
                          </Paragraph>
                        }
                      />
                    </List.Item>
                  )}
                />
              </Card>
            </Col>
          )}
        </Row>
      )}
    </div>
  )
}

export default ContentQualityAnalyzer
