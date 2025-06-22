import React, { useState } from 'react'
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
  Tooltip,
  Badge,
  Slider,
  Switch,
  Collapse,
  Statistic,
  message,
  Modal
} from 'antd'
import {
  ExperimentOutlined,
  BranchesOutlined,
  RocketOutlined,
  SwapOutlined,
  BulbOutlined,
  BarChartOutlined,
  ThunderboltOutlined,
  StarOutlined,
  CheckCircleOutlined,
  PlayCircleOutlined,
  CopyOutlined,
  EyeOutlined
} from '@ant-design/icons'
import { useGenerateTemplateVariantsMutation } from '@shared/store/api/templateAnalyticsApi'
import type { TemplateVariant, VariantType } from '@shared/types/templateAnalytics'

const { Title, Text, Paragraph } = Typography
const { Panel } = Collapse

interface TemplateVariantsGeneratorProps {
  templateKey: string
  templateName: string
  originalContent: string
  onVariantsGenerated?: (variants: TemplateVariant[]) => void
  onCreateABTest?: (variants: TemplateVariant[]) => void
}

export const TemplateVariantsGenerator: React.FC<TemplateVariantsGeneratorProps> = ({
  templateKey,
  templateName,
  originalContent,
  onVariantsGenerated,
  onCreateABTest
}) => {
  // State
  const [variants, setVariants] = useState<TemplateVariant[]>([])
  const [variantCount, setVariantCount] = useState(3)
  const [selectedVariants, setSelectedVariants] = useState<Set<number>>(new Set())
  const [previewModalVisible, setPreviewModalVisible] = useState(false)
  const [selectedVariantForPreview, setSelectedVariantForPreview] = useState<TemplateVariant | null>(null)
  const [autoCreateABTest, setAutoCreateABTest] = useState(false)

  // API
  const [generateVariants, { isLoading: isGenerating }] = useGenerateTemplateVariantsMutation()

  // Variant type configurations
  const variantTypeConfig = {
    ContentVariation: {
      description: 'Modifies the content while maintaining structure',
      color: '#1890ff',
      icon: <BulbOutlined />,
      focus: ['Wording', 'Examples', 'Explanations']
    },
    StructureVariation: {
      description: 'Changes the organization and flow of content',
      color: '#52c41a',
      icon: <BarChartOutlined />,
      focus: ['Layout', 'Order', 'Sections']
    },
    StyleVariation: {
      description: 'Adjusts tone, formality, and presentation style',
      color: '#722ed1',
      icon: <StarOutlined />,
      focus: ['Tone', 'Formality', 'Style']
    },
    ComplexityVariation: {
      description: 'Varies the complexity and detail level',
      color: '#fa8c16',
      icon: <ThunderboltOutlined />,
      focus: ['Detail Level', 'Complexity', 'Depth']
    }
  }

  const handleGenerateVariants = async () => {
    try {
      const result = await generateVariants({
        templateKey,
        variantCount
      }).unwrap()
      
      setVariants(result)
      setSelectedVariants(new Set(result.map((_, index) => index)))
      onVariantsGenerated?.(result)
      
      if (autoCreateABTest && result.length > 0) {
        onCreateABTest?.(result)
        message.success(`Generated ${result.length} variants and created A/B tests`)
      } else {
        message.success(`Generated ${result.length} template variants`)
      }
    } catch (error) {
      message.error('Failed to generate template variants')
    }
  }

  const handleVariantSelection = (index: number) => {
    const newSelection = new Set(selectedVariants)
    if (newSelection.has(index)) {
      newSelection.delete(index)
    } else {
      newSelection.add(index)
    }
    setSelectedVariants(newSelection)
  }

  const handlePreviewVariant = (variant: TemplateVariant) => {
    setSelectedVariantForPreview(variant)
    setPreviewModalVisible(true)
  }

  const handleCreateABTests = () => {
    const selectedVariantsList = variants.filter((_, index) => selectedVariants.has(index))
    if (selectedVariantsList.length === 0) {
      message.warning('Please select at least one variant')
      return
    }
    onCreateABTest?.(selectedVariantsList)
    message.success(`Creating A/B tests for ${selectedVariantsList.length} variants`)
  }

  const getPerformanceChangeColor = (change: number): string => {
    if (change > 0) return '#52c41a'
    if (change < 0) return '#ff4d4f'
    return '#666666'
  }

  const getConfidenceColor = (confidence: number): string => {
    if (confidence >= 0.8) return '#52c41a'
    if (confidence >= 0.6) return '#1890ff'
    return '#faad14'
  }

  return (
    <div className="template-variants-generator">
      {/* Header */}
      <Card style={{ marginBottom: '24px' }}>
        <Row gutter={16} align="middle">
          <Col span={16}>
            <Space>
              <BranchesOutlined style={{ fontSize: '24px', color: '#722ed1' }} />
              <div>
                <Title level={3} style={{ margin: 0 }}>Template Variants Generator</Title>
                <Text type="secondary">{templateName}</Text>
              </div>
            </Space>
          </Col>
          <Col span={8} style={{ textAlign: 'right' }}>
            <Space>
              <Button
                icon={<SwapOutlined />}
                disabled={variants.length === 0}
                onClick={() => setPreviewModalVisible(true)}
              >
                Compare All
              </Button>
              <Button 
                type="primary" 
                icon={<ExperimentOutlined />}
                onClick={handleGenerateVariants}
                loading={isGenerating}
              >
                Generate Variants
              </Button>
            </Space>
          </Col>
        </Row>
      </Card>

      {/* Configuration */}
      <Card title="Generation Settings" style={{ marginBottom: '24px' }}>
        <Row gutter={16} align="middle">
          <Col span={8}>
            <div>
              <Text strong>Number of Variants:</Text>
              <div style={{ marginTop: '8px' }}>
                <Slider
                  min={1}
                  max={5}
                  value={variantCount}
                  onChange={setVariantCount}
                  marks={{
                    1: '1',
                    2: '2',
                    3: '3',
                    4: '4',
                    5: '5'
                  }}
                />
              </div>
            </div>
          </Col>
          <Col span={8}>
            <div>
              <Text strong>Auto-create A/B Tests:</Text>
              <div style={{ marginTop: '8px' }}>
                <Switch
                  checked={autoCreateABTest}
                  onChange={setAutoCreateABTest}
                  checkedChildren="ON"
                  unCheckedChildren="OFF"
                />
                <div style={{ marginTop: '4px' }}>
                  <Text type="secondary" style={{ fontSize: '12px' }}>
                    Automatically create A/B tests after generation
                  </Text>
                </div>
              </div>
            </div>
          </Col>
          <Col span={8}>
            <Alert
              message="AI Variant Generation"
              description="Our AI will create diverse variants optimized for different performance aspects"
              type="info"
              showIcon
              style={{ fontSize: '12px' }}
            />
          </Col>
        </Row>
      </Card>

      {/* Variant Types Info */}
      <Card title="Variant Types" style={{ marginBottom: '24px' }}>
        <Row gutter={16}>
          {Object.entries(variantTypeConfig).map(([type, config]) => (
            <Col span={6} key={type}>
              <Card size="small" style={{ height: '140px' }}>
                <div style={{ textAlign: 'center', marginBottom: '8px' }}>
                  <div style={{ 
                    fontSize: '24px', 
                    color: config.color,
                    marginBottom: '4px'
                  }}>
                    {config.icon}
                  </div>
                  <Text strong style={{ fontSize: '12px' }}>
                    {type.replace(/([A-Z])/g, ' $1').trim()}
                  </Text>
                </div>
                <Paragraph style={{ fontSize: '11px', margin: '8px 0' }}>
                  {config.description}
                </Paragraph>
                <div>
                  {config.focus.map(focus => (
                    <Tag key={focus} color={config.color} size="small" style={{ margin: '1px' }}>
                      {focus}
                    </Tag>
                  ))}
                </div>
              </Card>
            </Col>
          ))}
        </Row>
      </Card>

      {/* Generated Variants */}
      {variants.length > 0 && (
        <div>
          <Card 
            title={
              <Space>
                <BranchesOutlined />
                Generated Variants
                <Badge count={variants.length} />
              </Space>
            }
            extra={
              <Space>
                <Text type="secondary">
                  Selected: {selectedVariants.size}/{variants.length}
                </Text>
                <Button 
                  type="primary"
                  icon={<PlayCircleOutlined />}
                  onClick={handleCreateABTests}
                  disabled={selectedVariants.size === 0}
                >
                  Create A/B Tests
                </Button>
              </Space>
            }
            style={{ marginBottom: '24px' }}
          >
            <Row gutter={16}>
              {variants.map((variant, index) => {
                const config = variantTypeConfig[variant.variantType]
                const isSelected = selectedVariants.has(index)
                
                return (
                  <Col span={8} key={index}>
                    <Card 
                      size="small"
                      style={{ 
                        marginBottom: '16px',
                        border: isSelected ? `2px solid ${config.color}` : '1px solid #d9d9d9',
                        cursor: 'pointer'
                      }}
                      onClick={() => handleVariantSelection(index)}
                      title={
                        <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                          <Space>
                            <div style={{ color: config.color }}>{config.icon}</div>
                            <Text strong style={{ fontSize: '12px' }}>
                              Variant {index + 1}
                            </Text>
                          </Space>
                          <Badge 
                            status={isSelected ? 'processing' : 'default'} 
                            text={isSelected ? 'Selected' : 'Click to select'}
                          />
                        </div>
                      }
                    >
                      <div style={{ marginBottom: '12px' }}>
                        <Tag color={config.color} style={{ marginBottom: '8px' }}>
                          {variant.variantType.replace(/([A-Z])/g, ' $1').trim()}
                        </Tag>
                      </div>

                      <Row gutter={8} style={{ marginBottom: '12px' }}>
                        <Col span={12}>
                          <Statistic
                            title="Expected Change"
                            value={variant.expectedPerformanceChange}
                            precision={1}
                            suffix="%"
                            prefix={variant.expectedPerformanceChange >= 0 ? '+' : ''}
                            valueStyle={{ 
                              fontSize: '16px',
                              color: getPerformanceChangeColor(variant.expectedPerformanceChange)
                            }}
                          />
                        </Col>
                        <Col span={12}>
                          <div style={{ textAlign: 'center' }}>
                            <div style={{ fontSize: '12px', color: '#666', marginBottom: '4px' }}>
                              Confidence
                            </div>
                            <Progress
                              type="circle"
                              percent={variant.confidenceScore * 100}
                              size={50}
                              strokeColor={getConfidenceColor(variant.confidenceScore)}
                              format={(percent) => (
                                <span style={{ fontSize: '10px' }}>
                                  {percent?.toFixed(0)}%
                                </span>
                              )}
                            />
                          </div>
                        </Col>
                      </Row>

                      <div style={{ marginBottom: '12px' }}>
                        <Text strong style={{ fontSize: '11px' }}>Reasoning:</Text>
                        <Paragraph 
                          style={{ fontSize: '10px', margin: '4px 0' }}
                          ellipsis={{ rows: 2, expandable: true }}
                        >
                          {variant.generationReasoning}
                        </Paragraph>
                      </div>

                      <div style={{ 
                        padding: '8px', 
                        backgroundColor: '#f5f5f5', 
                        borderRadius: '4px',
                        marginBottom: '12px',
                        maxHeight: '100px',
                        overflow: 'hidden'
                      }}>
                        <Text style={{ fontSize: '10px', fontFamily: 'monospace' }}>
                          {variant.variantContent.substring(0, 150)}
                          {variant.variantContent.length > 150 && '...'}
                        </Text>
                      </div>

                      <div style={{ textAlign: 'center' }}>
                        <Space>
                          <Button 
                            size="small" 
                            icon={<EyeOutlined />}
                            onClick={(e) => {
                              e.stopPropagation()
                              handlePreviewVariant(variant)
                            }}
                          >
                            Preview
                          </Button>
                          <Button 
                            size="small" 
                            icon={<CopyOutlined />}
                            onClick={(e) => {
                              e.stopPropagation()
                              navigator.clipboard.writeText(variant.variantContent)
                              message.success('Variant content copied to clipboard')
                            }}
                          >
                            Copy
                          </Button>
                        </Space>
                      </div>
                    </Card>
                  </Col>
                )
              })}
            </Row>
          </Card>
        </div>
      )}

      {/* Loading State */}
      {isGenerating && (
        <Card style={{ textAlign: 'center', padding: '40px 0' }}>
          <Space direction="vertical" size="large">
            <BranchesOutlined style={{ fontSize: '48px', color: '#722ed1' }} />
            <div>
              <Title level={4}>Generating Template Variants...</Title>
              <Text type="secondary">
                Creating {variantCount} unique variants optimized for different performance aspects
              </Text>
            </div>
            <Progress percent={30} status="active" />
          </Space>
        </Card>
      )}

      {/* Preview Modal */}
      <Modal
        title={
          selectedVariantForPreview ? (
            <Space>
              {variantTypeConfig[selectedVariantForPreview.variantType].icon}
              Variant Preview - {selectedVariantForPreview.variantType.replace(/([A-Z])/g, ' $1').trim()}
            </Space>
          ) : 'Variant Preview'
        }
        open={previewModalVisible}
        onCancel={() => setPreviewModalVisible(false)}
        footer={null}
        width={1000}
      >
        {selectedVariantForPreview && (
          <div>
            <Row gutter={16} style={{ marginBottom: '16px' }}>
              <Col span={8}>
                <Statistic
                  title="Expected Performance Change"
                  value={selectedVariantForPreview.expectedPerformanceChange}
                  precision={1}
                  suffix="%"
                  prefix={selectedVariantForPreview.expectedPerformanceChange >= 0 ? '+' : ''}
                  valueStyle={{ 
                    color: getPerformanceChangeColor(selectedVariantForPreview.expectedPerformanceChange)
                  }}
                />
              </Col>
              <Col span={8}>
                <Statistic
                  title="Confidence Score"
                  value={selectedVariantForPreview.confidenceScore * 100}
                  precision={1}
                  suffix="%"
                  valueStyle={{ 
                    color: getConfidenceColor(selectedVariantForPreview.confidenceScore)
                  }}
                />
              </Col>
              <Col span={8}>
                <div>
                  <Text strong>Variant Type:</Text>
                  <div style={{ marginTop: '4px' }}>
                    <Tag color={variantTypeConfig[selectedVariantForPreview.variantType].color}>
                      {selectedVariantForPreview.variantType.replace(/([A-Z])/g, ' $1').trim()}
                    </Tag>
                  </div>
                </div>
              </Col>
            </Row>

            <div style={{ marginBottom: '16px' }}>
              <Text strong>Generation Reasoning:</Text>
              <Paragraph style={{ marginTop: '8px' }}>
                {selectedVariantForPreview.generationReasoning}
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
                <Card title="Variant Template" size="small">
                  <div style={{ 
                    padding: '12px', 
                    backgroundColor: '#f0f9ff', 
                    borderRadius: '4px',
                    maxHeight: '300px',
                    overflow: 'auto',
                    fontFamily: 'monospace',
                    fontSize: '12px',
                    whiteSpace: 'pre-wrap'
                  }}>
                    {selectedVariantForPreview.variantContent}
                  </div>
                </Card>
              </Col>
            </Row>
          </div>
        )}
      </Modal>
    </div>
  )
}

export default TemplateVariantsGenerator
