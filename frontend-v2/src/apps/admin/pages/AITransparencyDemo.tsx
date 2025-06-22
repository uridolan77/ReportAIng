import React, { useState } from 'react'
import { Card, Row, Col, Space, Typography, Button, Divider, Alert, Tabs } from 'antd'
import {
  EyeOutlined,
  BarChartOutlined,
  BulbOutlined,
  RobotOutlined,
  ExperimentOutlined
} from '@ant-design/icons'
import { AITransparencyPanel } from '@shared/components/ai/transparency/AITransparencyPanel'
import { PromptConstructionViewer } from '@shared/components/ai/transparency/PromptConstructionViewer'
import { ConfidenceBreakdownChart } from '@shared/components/ai/transparency/ConfidenceBreakdownChart'
import { AIDecisionExplainer } from '@shared/components/ai/transparency/AIDecisionExplainer'
import { ConfidenceIndicator } from '@shared/components/ai/common/ConfidenceIndicator'
import { AIFeatureWrapper } from '@shared/components/ai/common/AIFeatureWrapper'

const { Title, Text, Paragraph } = Typography

/**
 * AITransparencyAnalysis - AI transparency analysis and exploration page
 *
 * This page provides:
 * - Complete AI transparency panel
 * - Individual transparency components
 * - Interactive analysis tools
 * - Decision exploration
 * - Component examples
 */
export const AITransparencyAnalysis: React.FC = () => {
  const [activeTab, setActiveTab] = useState('complete')
  const [demoTraceId] = useState('demo-trace-001')

  // Mock data for demonstrations
  const mockTrace = {
    traceId: demoTraceId,
    steps: [
      {
        stepName: 'Query Understanding',
        description: 'Analyzing user intent and extracting key information',
        confidence: 0.94,
        context: ['user_query', 'conversation_history', 'user_preferences'],
        alternatives: ['Keyword-based parsing', 'Rule-based extraction'],
        reasoning: 'High confidence due to clear query structure and rich context',
        timestamp: new Date().toISOString()
      },
      {
        stepName: 'Business Context Mapping',
        description: 'Mapping query elements to business concepts and metadata',
        confidence: 0.87,
        context: ['business_glossary', 'schema_metadata', 'domain_knowledge'],
        alternatives: ['Simple schema matching', 'Manual mapping'],
        reasoning: 'Good business context understanding with some ambiguity in terminology',
        timestamp: new Date().toISOString()
      },
      {
        stepName: 'SQL Generation',
        description: 'Generating optimized SQL query with proper joins and filters',
        confidence: 0.91,
        context: ['schema_relationships', 'query_patterns', 'performance_rules'],
        alternatives: ['Template-based generation', 'Basic query building'],
        reasoning: 'Strong pattern recognition with identified optimization opportunities',
        timestamp: new Date().toISOString()
      }
    ],
    finalPrompt: `Generate SQL query for quarterly sales analysis:

Context: User wants to analyze sales performance by product category for Q4 2024
Business Rules: Include only active products, exclude test data
Schema: sales table joined with products and categories
Optimization: Use appropriate indexes and limit result set

SQL Query:
SELECT 
  c.category_name,
  p.product_name,
  SUM(s.amount) as total_sales,
  COUNT(s.id) as transaction_count,
  AVG(s.amount) as avg_transaction
FROM sales s
JOIN products p ON s.product_id = p.id
JOIN categories c ON p.category_id = c.id
WHERE s.sale_date >= '2024-10-01' 
  AND s.sale_date < '2025-01-01'
  AND p.is_active = true
  AND s.amount > 0
GROUP BY c.category_name, p.product_name
ORDER BY total_sales DESC
LIMIT 100`,
    totalConfidence: 0.91,
    optimizationSuggestions: [
      {
        id: 'opt-1',
        type: 'performance',
        title: 'Add Composite Index',
        description: 'Create composite index on (sale_date, product_id) for better performance',
        impact: 'high',
        effort: 'medium',
        expectedImprovement: 0.4,
        implementation: 'CREATE INDEX idx_sales_date_product ON sales(sale_date, product_id)'
      },
      {
        id: 'opt-2',
        type: 'accuracy',
        title: 'Add Data Validation',
        description: 'Include additional validation for data quality',
        impact: 'medium',
        effort: 'low',
        expectedImprovement: 0.15,
        implementation: 'AND s.amount BETWEEN 0.01 AND 1000000'
      }
    ],
    metadata: {
      modelUsed: 'gpt-4',
      provider: 'openai',
      tokensUsed: 1450,
      processingTime: 950
    }
  }

  const mockAnalysis = {
    overallConfidence: 0.91,
    factors: [
      {
        name: 'Query Clarity',
        score: 0.94,
        explanation: 'User query is well-structured with clear business intent',
        impact: 'high',
        category: 'context'
      },
      {
        name: 'Schema Understanding',
        score: 0.89,
        explanation: 'Good understanding of table relationships and constraints',
        impact: 'high',
        category: 'business'
      },
      {
        name: 'Business Logic',
        score: 0.88,
        explanation: 'Proper application of business rules and filters',
        impact: 'medium',
        category: 'semantics'
      },
      {
        name: 'SQL Optimization',
        score: 0.93,
        explanation: 'Effective query optimization with performance considerations',
        impact: 'medium',
        category: 'syntax'
      }
    ],
    breakdown: {
      contextualRelevance: 0.94,
      syntacticCorrectness: 0.89,
      semanticClarity: 0.91,
      businessAlignment: 0.88
    },
    recommendations: [
      'Consider adding more specific date range validation',
      'Review business rules for edge cases',
      'Monitor query performance with larger datasets'
    ]
  }

  const mockExplanation = {
    decision: 'Generated optimized SQL query for quarterly sales analysis with proper business logic',
    reasoning: [
      'Identified clear business intent for quarterly sales analysis',
      'Mapped query requirements to appropriate database schema',
      'Applied business rules for data filtering and validation',
      'Optimized query structure for performance and readability',
      'Included appropriate aggregations and sorting logic'
    ],
    confidence: 0.91,
    alternatives: [],
    factors: mockAnalysis.factors,
    recommendations: mockTrace.optimizationSuggestions
  }

  const tabItems = [
    {
      key: 'complete',
      label: (
        <Space>
          <EyeOutlined />
          <span>Complete Panel</span>
        </Space>
      ),
      children: (
        <div>
          <AITransparencyPanel
            traceId={demoTraceId}
            showDetailedMetrics={true}
            compact={false}
          />
        </div>
      )
    },
    {
      key: 'prompt',
      label: (
        <Space>
          <RobotOutlined />
          <span>Prompt Construction</span>
        </Space>
      ),
      children: (
        <div>
          <PromptConstructionViewer
            trace={mockTrace}
            interactive={true}
            showTimeline={true}
          />
        </div>
      )
    },
    {
      key: 'confidence',
      label: (
        <Space>
          <BarChartOutlined />
          <span>Confidence Analysis</span>
        </Space>
      ),
      children: (
        <div>
          <ConfidenceBreakdownChart
            analysis={mockAnalysis}
            showFactors={true}
            interactive={true}
            chartType="bar"
          />
        </div>
      )
    },
    {
      key: 'explanation',
      label: (
        <Space>
          <BulbOutlined />
          <span>Decision Explanation</span>
        </Space>
      ),
      children: (
        <div>
          <AIDecisionExplainer
            explanation={mockExplanation}
            showAlternatives={true}
            showRecommendations={true}
          />
        </div>
      )
    },
    {
      key: 'components',
      label: (
        <Space>
          <ExperimentOutlined />
          <span>Component Examples</span>
        </Space>
      ),
      children: (
        <div>
          <Row gutter={[16, 16]}>
            <Col span={8}>
              <Card title="Confidence Indicators" size="small">
                <Space direction="vertical" style={{ width: '100%' }}>
                  <div>
                    <Text strong>Circle Type:</Text>
                    <div style={{ marginTop: 8 }}>
                      <ConfidenceIndicator confidence={0.91} type="circle" size="large" showLabel={true} />
                    </div>
                  </div>
                  <div>
                    <Text strong>Bar Type:</Text>
                    <div style={{ marginTop: 8 }}>
                      <ConfidenceIndicator confidence={0.87} type="bar" size="medium" showLabel={true} />
                    </div>
                  </div>
                  <div>
                    <Text strong>Badge Type:</Text>
                    <div style={{ marginTop: 8 }}>
                      <ConfidenceIndicator confidence={0.94} type="badge" size="small" showLabel={true} />
                    </div>
                  </div>
                </Space>
              </Card>
            </Col>
            
            <Col span={8}>
              <Card title="Feature Wrapper" size="small">
                <AIFeatureWrapper 
                  feature="transparencyPanelEnabled"
                  fallback={<Text type="secondary">Transparency features disabled</Text>}
                >
                  <Alert
                    message="Transparency Enabled"
                    description="This content is wrapped with AI feature detection."
                    type="success"
                    showIcon
                  />
                </AIFeatureWrapper>
              </Card>
            </Col>
            
            <Col span={8}>
              <Card title="Compact Mode" size="small">
                <AITransparencyPanel
                  traceId={demoTraceId}
                  compact={true}
                  showDetailedMetrics={false}
                />
              </Card>
            </Col>
          </Row>
        </div>
      )
    }
  ]

  return (
    <div style={{ padding: 24 }}>
      <div style={{ marginBottom: 24 }}>
        <Title level={2}>
          <Space>
            <EyeOutlined />
            AI Transparency Analysis
          </Space>
        </Title>
        <Paragraph>
          Explore AI decision-making processes with comprehensive transparency tools.
          Analyze confidence factors, examine prompt construction, and understand
          how AI arrives at its conclusions.
        </Paragraph>
      </div>

      <Tabs
        activeKey={activeTab}
        onChange={setActiveTab}
        items={tabItems}
        size="large"
      />
    </div>
  )
}

export default AITransparencyAnalysis
