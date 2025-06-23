import React, { useState, useEffect } from 'react'
import { 
  Card, 
  Row, 
  Col, 
  Button, 
  Typography,
  Space,
  Tabs,
  Select,
  message,
  Badge,
  Tooltip,
  Spin,
  Alert
} from 'antd'
import {
  RocketOutlined,
  BulbOutlined,
  ExperimentOutlined,
  BarChartOutlined,
  StarOutlined,
  ThunderboltOutlined,
  SettingOutlined,
  ReloadOutlined
} from '@ant-design/icons'
import {
  ContentQualityAnalyzer,
  TemplateOptimizationInterface,
  PerformancePredictionInterface,
  TemplateVariantsGenerator,
  MLRecommendationEngine
} from '../../components/template-analytics'
import { 
  useGetTemplateWithMetricsQuery,
  useGetTemplateManagementDashboardQuery,
  useAnalyzeTemplatePerformanceMutation,
  useOptimizeTemplateMutation,
  usePredictTemplatePerformanceMutation,
  useGenerateTemplateVariantsMutation,
  useAnalyzeContentQualityMutation
} from '@shared/store/api/templateAnalyticsApi'
import { PageLayout } from '@shared/components/core/Layout'

const { Title, Text, Paragraph } = Typography

interface TemplateFeaturesProps {
  selectedTemplateKey?: string
}

export const TemplateFeatures: React.FC<TemplateFeaturesProps> = ({
  selectedTemplateKey
}) => {
  const [activeTab, setActiveTab] = useState('quality')
  const [selectedTemplate, setSelectedTemplate] = useState<string>('')
  const [templateContent, setTemplateContent] = useState<string>('')
  const [isProcessing, setIsProcessing] = useState(false)

  // API hooks
  const { 
    data: managementData, 
    isLoading: isLoadingManagement 
  } = useGetTemplateManagementDashboardQuery()

  const { 
    data: templateData, 
    isLoading: isLoadingTemplate,
    refetch: refetchTemplate 
  } = useGetTemplateWithMetricsQuery(selectedTemplate, {
    skip: !selectedTemplate
  })

  // Mutations
  const [analyzePerformance, { isLoading: isAnalyzingPerformance }] = useAnalyzeTemplatePerformanceMutation()
  const [optimizeTemplate, { isLoading: isOptimizing }] = useOptimizeTemplateMutation()
  const [predictPerformance, { isLoading: isPredicting }] = usePredictTemplatePerformanceMutation()
  const [generateVariants, { isLoading: isGeneratingVariants }] = useGenerateTemplateVariantsMutation()
  const [analyzeQuality, { isLoading: isAnalyzingQuality }] = useAnalyzeContentQualityMutation()

  // Set initial template when management data loads or when selectedTemplateKey prop changes
  useEffect(() => {
    if (selectedTemplateKey) {
      setSelectedTemplate(selectedTemplateKey)
    } else if (managementData?.templates && managementData.templates.length > 0 && !selectedTemplate) {
      const firstTemplate = managementData.templates[0]
      setSelectedTemplate(firstTemplate.templateKey)
    }
  }, [managementData, selectedTemplate, selectedTemplateKey])

  // Update template content when template data loads
  useEffect(() => {
    if (templateData?.template?.content) {
      setTemplateContent(templateData.template.content)
    }
  }, [templateData])

  const handleTemplateChange = (templateKey: string) => {
    setSelectedTemplate(templateKey)
    setTemplateContent('')
  }

  const handleQualityAnalysis = async (content: string) => {
    if (!selectedTemplate) {
      message.error('Please select a template first')
      return
    }

    try {
      setIsProcessing(true)
      const result = await analyzeQuality({
        templateKey: selectedTemplate,
        content
      }).unwrap()
      
      message.success(`Quality analysis completed! Overall score: ${(result.overallQualityScore * 100).toFixed(1)}%`)
      return result
    } catch (error) {
      message.error('Failed to analyze template quality')
      console.error('Quality analysis error:', error)
    } finally {
      setIsProcessing(false)
    }
  }

  const handleOptimizationComplete = async (strategy: string, content: string) => {
    if (!selectedTemplate) {
      message.error('Please select a template first')
      return
    }

    try {
      setIsProcessing(true)
      const result = await optimizeTemplate({
        templateKey: selectedTemplate,
        strategy,
        content
      }).unwrap()
      
      message.success(`Optimization completed with ${result.expectedPerformanceImprovement.toFixed(1)}% expected improvement`)
      return result
    } catch (error) {
      message.error('Failed to optimize template')
      console.error('Optimization error:', error)
    } finally {
      setIsProcessing(false)
    }
  }

  const handlePredictionComplete = async (content: string, intentType: string) => {
    if (!selectedTemplate) {
      message.error('Please select a template first')
      return
    }

    try {
      setIsProcessing(true)
      const result = await predictPerformance({
        templateKey: selectedTemplate,
        content,
        intentType
      }).unwrap()
      
      message.success(`Performance prediction: ${(result.predictedSuccessRate * 100).toFixed(1)}% success rate`)
      return result
    } catch (error) {
      message.error('Failed to predict template performance')
      console.error('Prediction error:', error)
    } finally {
      setIsProcessing(false)
    }
  }

  const handleVariantsGenerated = async (strategy: string, count: number) => {
    if (!selectedTemplate) {
      message.error('Please select a template first')
      return
    }

    try {
      setIsProcessing(true)
      const result = await generateVariants({
        templateKey: selectedTemplate,
        strategy,
        variantCount: count
      }).unwrap()
      
      message.success(`Generated ${result.variants.length} template variants for A/B testing`)
      return result
    } catch (error) {
      message.error('Failed to generate template variants')
      console.error('Variant generation error:', error)
    } finally {
      setIsProcessing(false)
    }
  }

  const handleRecommendationApply = async (recommendation: any) => {
    try {
      setIsProcessing(true)
      // Apply the recommendation through the optimization API
      const result = await optimizeTemplate({
        templateKey: selectedTemplate,
        strategy: recommendation.strategy || 'general',
        content: templateContent
      }).unwrap()
      
      message.success(`Applied recommendation: ${recommendation.title}`)
      
      // Refresh template data to show updated metrics
      if (selectedTemplate) {
        refetchTemplate()
      }
      
      return result
    } catch (error) {
      message.error('Failed to apply recommendation')
      console.error('Recommendation application error:', error)
    } finally {
      setIsProcessing(false)
    }
  }

  const templateOptions = managementData?.templates?.map(template => ({
    label: `${template.templateName} (${template.templateKey})`,
    value: template.templateKey
  })) || []

  const tabItems = [
    {
      key: 'quality',
      label: (
        <Space>
          <StarOutlined />
          Quality Analysis
          {isAnalyzingQuality && <Spin size="small" />}
        </Space>
      ),
      children: (
        <Card>
          <div style={{ marginBottom: '16px' }}>
            <Title level={4}>Content Quality Analyzer</Title>
            <Paragraph>
              Analyze template content quality with AI-powered scoring for readability, 
              structure, completeness, and get actionable improvement suggestions.
            </Paragraph>
          </div>
          <ContentQualityAnalyzer 
            initialContent={templateContent}
            onAnalysisComplete={handleQualityAnalysis}
            loading={isAnalyzingQuality || isProcessing}
          />
        </Card>
      )
    },
    {
      key: 'optimization',
      label: (
        <Space>
          <RocketOutlined />
          Optimization
          {isOptimizing && <Spin size="small" />}
        </Space>
      ),
      children: (
        <Card>
          <div style={{ marginBottom: '16px' }}>
            <Title level={4}>Template Optimization</Title>
            <Paragraph>
              Optimize templates using different strategies with before/after comparisons 
              and expected performance improvements.
            </Paragraph>
          </div>
          <TemplateOptimizationInterface
            templateKey={selectedTemplate}
            templateName={templateData?.template?.templateName || 'Selected Template'}
            originalContent={templateContent}
            onOptimizationComplete={handleOptimizationComplete}
            onCreateABTest={(original, optimized) => {
              message.success('A/B test created successfully')
            }}
            loading={isOptimizing || isProcessing}
          />
        </Card>
      )
    },
    {
      key: 'prediction',
      label: (
        <Space>
          <BulbOutlined />
          Prediction
          {isPredicting && <Spin size="small" />}
        </Space>
      ),
      children: (
        <Card>
          <div style={{ marginBottom: '16px' }}>
            <Title level={4}>Performance Prediction</Title>
            <Paragraph>
              Use machine learning to predict template performance, analyzing content features 
              and providing success rate forecasts with confidence levels.
            </Paragraph>
          </div>
          <PerformancePredictionInterface
            initialContent={templateContent}
            initialIntentType={templateData?.template?.intentType || 'general'}
            onPredictionComplete={handlePredictionComplete}
            loading={isPredicting || isProcessing}
          />
        </Card>
      )
    },
    {
      key: 'variants',
      label: (
        <Space>
          <ExperimentOutlined />
          Variants
          {isGeneratingVariants && <Spin size="small" />}
        </Space>
      ),
      children: (
        <Card>
          <div style={{ marginBottom: '16px' }}>
            <Title level={4}>Template Variants Generator</Title>
            <Paragraph>
              Generate multiple template variants for A/B testing, creating different 
              versions optimized for various performance aspects.
            </Paragraph>
          </div>
          <TemplateVariantsGenerator
            templateKey={selectedTemplate}
            templateName={templateData?.template?.templateName || 'Selected Template'}
            originalContent={templateContent}
            onVariantsGenerated={handleVariantsGenerated}
            onCreateABTest={(variants) => {
              message.success(`Created A/B tests for ${variants.length} variants`)
            }}
            loading={isGeneratingVariants || isProcessing}
          />
        </Card>
      )
    },
    {
      key: 'recommendations',
      label: (
        <Space>
          <BarChartOutlined />
          ML Recommendations
        </Space>
      ),
      children: (
        <Card>
          <div style={{ marginBottom: '16px' }}>
            <Title level={4}>ML Recommendation Engine</Title>
            <Paragraph>
              Get AI-powered recommendations for template improvements, analyzing patterns 
              and suggesting actionable optimizations.
            </Paragraph>
          </div>
          <MLRecommendationEngine
            templateKey={selectedTemplate}
            intentType={templateData?.template?.intentType || 'general'}
            onRecommendationApply={handleRecommendationApply}
            loading={isProcessing}
          />
        </Card>
      )
    }
  ]

  return (
    <PageLayout
      title="Template Features"
      subtitle="AI-powered template analysis, optimization, and enhancement tools"
      extra={
        <Space>
          <Tooltip title="Refresh template data">
            <Button 
              icon={<ReloadOutlined />} 
              onClick={() => refetchTemplate()}
              loading={isLoadingTemplate}
            />
          </Tooltip>
          <Button icon={<SettingOutlined />}>
            Configure Features
          </Button>
        </Space>
      }
    >
      {/* Template Selection */}
      <Card style={{ marginBottom: '24px' }}>
        <Row gutter={16} align="middle">
          <Col span={12}>
            <Space>
              <Text strong>Select Template:</Text>
              <Select
                style={{ width: 300 }}
                value={selectedTemplate}
                onChange={handleTemplateChange}
                options={templateOptions}
                loading={isLoadingManagement}
                placeholder="Choose a template to analyze"
              />
            </Space>
          </Col>
          <Col span={12} style={{ textAlign: 'right' }}>
            {templateData && (
              <Space>
                <Badge 
                  status={templateData.metrics?.successRate > 0.8 ? 'success' : 'warning'} 
                  text={`Success Rate: ${((templateData.metrics?.successRate || 0) * 100).toFixed(1)}%`}
                />
                <Badge 
                  status="processing" 
                  text={`Usage Count: ${templateData.metrics?.usageCount || 0}`}
                />
              </Space>
            )}
          </Col>
        </Row>
      </Card>

      {/* Feature Tabs */}
      <Card>
        <Tabs
          activeKey={activeTab}
          onChange={setActiveTab}
          items={tabItems}
          size="large"
        />
      </Card>
    </PageLayout>
  )
}

export default TemplateFeatures
