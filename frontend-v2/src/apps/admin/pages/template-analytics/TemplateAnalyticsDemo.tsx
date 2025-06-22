import React, { useState } from 'react'
import { 
  Card, 
  Row, 
  Col, 
  Button, 
  Typography,
  Space,
  Alert,
  Tabs,
  message
} from 'antd'
import {
  RocketOutlined,
  BulbOutlined,
  ExperimentOutlined,
  BarChartOutlined,
  StarOutlined
} from '@ant-design/icons'
import {
  ContentQualityAnalyzer,
  TemplateOptimizationInterface,
  PerformancePredictionInterface,
  TemplateVariantsGenerator,
  MLRecommendationEngine
} from '../../components/template-analytics'

const { Title, Text, Paragraph } = Typography

export const TemplateAnalyticsDemo: React.FC = () => {
  const [activeTab, setActiveTab] = useState('quality')
  const [selectedTemplate] = useState('sql_generation_basic')

  const sampleTemplateContent = `You are an expert SQL analyst. Generate a SQL query based on the user's request.

Context: {context}
Database Schema: {schema}
User Request: {user_request}

Instructions:
1. Analyze the user's request carefully
2. Consider the database schema provided
3. Generate an efficient SQL query
4. Include comments explaining complex parts
5. Ensure the query follows best practices

SQL Query:
SELECT 
  -- Add your query here
FROM 
  -- Specify tables
WHERE 
  -- Add conditions
ORDER BY 
  -- Add sorting if needed;

Explanation:
Provide a clear explanation of what the query does and why you chose this approach.`

  const handleQualityAnalysis = (analysis: any) => {
    message.success(`Quality analysis completed! Overall score: ${(analysis.overallQualityScore * 100).toFixed(1)}%`)
  }

  const handleOptimizationComplete = (result: any) => {
    message.success(`Optimization completed with ${result.expectedPerformanceImprovement.toFixed(1)}% expected improvement`)
  }

  const handlePredictionComplete = (prediction: any) => {
    message.success(`Performance prediction: ${(prediction.predictedSuccessRate * 100).toFixed(1)}% success rate`)
  }

  const handleVariantsGenerated = (variants: any[]) => {
    message.success(`Generated ${variants.length} template variants for A/B testing`)
  }

  const handleRecommendationApply = (recommendation: any) => {
    message.success(`Applied recommendation: ${recommendation.title}`)
  }

  const tabItems = [
    {
      key: 'quality',
      label: (
        <Space>
          <StarOutlined />
          Quality Analysis
        </Space>
      ),
      children: (
        <Card>
          <div style={{ marginBottom: '16px' }}>
            <Title level={4}>Content Quality Analyzer Demo</Title>
            <Paragraph>
              This component analyzes template content quality, providing scores for readability, 
              structure, completeness, and identifying issues with improvement suggestions.
            </Paragraph>
          </div>
          <ContentQualityAnalyzer 
            initialContent={sampleTemplateContent}
            onAnalysisComplete={handleQualityAnalysis}
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
        </Space>
      ),
      children: (
        <Card>
          <div style={{ marginBottom: '16px' }}>
            <Title level={4}>Template Optimization Demo</Title>
            <Paragraph>
              This interface allows you to optimize templates using different strategies, 
              showing before/after comparisons and expected performance improvements.
            </Paragraph>
          </div>
          <TemplateOptimizationInterface
            templateKey={selectedTemplate}
            templateName="SQL Generation Template"
            originalContent={sampleTemplateContent}
            onOptimizationComplete={handleOptimizationComplete}
            onCreateABTest={(original, optimized) => {
              message.success('A/B test created successfully')
            }}
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
        </Space>
      ),
      children: (
        <Card>
          <div style={{ marginBottom: '16px' }}>
            <Title level={4}>Performance Prediction Demo</Title>
            <Paragraph>
              This component uses ML to predict template performance, analyzing content features 
              and providing success rate forecasts with confidence levels.
            </Paragraph>
          </div>
          <PerformancePredictionInterface
            initialContent={sampleTemplateContent}
            initialIntentType="sql_generation"
            onPredictionComplete={handlePredictionComplete}
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
        </Space>
      ),
      children: (
        <Card>
          <div style={{ marginBottom: '16px' }}>
            <Title level={4}>Template Variants Generator Demo</Title>
            <Paragraph>
              This component generates multiple template variants for A/B testing, 
              creating different versions optimized for various performance aspects.
            </Paragraph>
          </div>
          <TemplateVariantsGenerator
            templateKey={selectedTemplate}
            templateName="SQL Generation Template"
            originalContent={sampleTemplateContent}
            onVariantsGenerated={handleVariantsGenerated}
            onCreateABTest={(variants) => {
              message.success(`Created A/B tests for ${variants.length} variants`)
            }}
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
            <Title level={4}>ML Recommendation Engine Demo</Title>
            <Paragraph>
              This component provides AI-powered recommendations for template improvements, 
              analyzing patterns and suggesting actionable optimizations.
            </Paragraph>
          </div>
          <MLRecommendationEngine
            intentType="sql_generation"
            onRecommendationApply={handleRecommendationApply}
          />
        </Card>
      )
    }
  ]

  return (
    <div style={{ padding: '24px' }}>
      {/* Header */}
      <Card style={{ marginBottom: '24px' }}>
        <Row gutter={16} align="middle">
          <Col span={16}>
            <Space>
              <RocketOutlined style={{ fontSize: '24px', color: '#1890ff' }} />
              <div>
                <Title level={2} style={{ margin: 0 }}>Template Analytics Integration Demo</Title>
                <Text type="secondary">
                  Demonstration of the new AI-powered template analytics features
                </Text>
              </div>
            </Space>
          </Col>
          <Col span={8} style={{ textAlign: 'right' }}>
            <Space>
              <Button type="primary" onClick={() => message.info('Demo mode - all features are functional!')}>
                Demo Info
              </Button>
            </Space>
          </Col>
        </Row>
      </Card>

      {/* Feature Overview */}
      <Alert
        message="Enhanced Template Analytics Features"
        description={
          <div>
            <Paragraph style={{ margin: '8px 0' }}>
              This demo showcases the new AI-powered template analytics capabilities including:
            </Paragraph>
            <ul style={{ margin: '8px 0', paddingLeft: '20px' }}>
              <li><strong>Content Quality Analysis:</strong> AI-powered quality scoring and improvement suggestions</li>
              <li><strong>Template Optimization:</strong> Strategic optimization with multiple approaches</li>
              <li><strong>Performance Prediction:</strong> ML-based success rate forecasting</li>
              <li><strong>Variant Generation:</strong> Automated A/B testing variant creation</li>
              <li><strong>ML Recommendations:</strong> Intelligent insights and actionable recommendations</li>
            </ul>
          </div>
        }
        type="info"
        showIcon
        style={{ marginBottom: '24px' }}
      />

      {/* Demo Tabs */}
      <Card>
        <Tabs
          activeKey={activeTab}
          onChange={setActiveTab}
          items={tabItems}
          size="large"
        />
      </Card>

      {/* Footer */}
      <Card style={{ marginTop: '24px', textAlign: 'center' }}>
        <Space direction="vertical">
          <Text type="secondary">
            All components are fully integrated with the backend API and SignalR for real-time updates
          </Text>
          <Space>
            <Button type="link" onClick={() => message.info('Navigate to /admin/template-analytics/enhanced')}>
              View Full Enhanced Analytics
            </Button>
            <Button type="link" onClick={() => message.info('Check the comprehensive dashboard')}>
              View Dashboard
            </Button>
          </Space>
        </Space>
      </Card>
    </div>
  )
}

export default TemplateAnalyticsDemo
