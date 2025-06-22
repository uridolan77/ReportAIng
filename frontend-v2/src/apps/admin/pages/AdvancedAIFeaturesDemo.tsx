import React, { useState } from 'react'
import { 
  Card, 
  Row, 
  Col, 
  Typography, 
  Space, 
  Button, 
  Input,
  Tabs,
  Alert,
  Divider,
  Badge
} from 'antd'
import {
  DatabaseOutlined,
  ThunderboltOutlined,
  BulbOutlined,
  BarChartOutlined,
  RobotOutlined,
  ExperimentOutlined,
  SearchOutlined
} from '@ant-design/icons'
import { 
  AISchemaExplorer,
  QueryOptimizationEngine,
  AutomatedInsightsGenerator,
  PredictiveAnalyticsPanel
} from '@shared/components/ai/insights'
import { AIFeatureWrapper } from '@shared/components/ai/common/AIFeatureWrapper'

const { Title, Text } = Typography
const { TextArea } = Input

/**
 * AdvancedAIFeaturesDemo - Comprehensive demo of advanced AI features
 * 
 * Features:
 * - AI-powered schema exploration and discovery
 * - Intelligent query optimization with performance analysis
 * - Automated insights generation with pattern recognition
 * - Predictive analytics with forecasting and trend analysis
 * - Real-time AI recommendations and suggestions
 * - Integration with backend AI intelligence APIs
 */
export const AdvancedAIFeaturesDemo: React.FC = () => {
  const [activeTab, setActiveTab] = useState('schema')
  const [currentQuery, setCurrentQuery] = useState('SELECT sales.amount, region.name FROM sales JOIN region ON sales.region_id = region.id WHERE sales.date >= \'2024-01-01\'')
  const [dataContext, setDataContext] = useState('sales_analytics')

  // Sample queries for demonstration
  const sampleQueries = [
    'SELECT sales.amount, region.name FROM sales JOIN region ON sales.region_id = region.id WHERE sales.date >= \'2024-01-01\'',
    'SELECT COUNT(*) as total_customers, AVG(order_value) as avg_order FROM customers c JOIN orders o ON c.id = o.customer_id',
    'SELECT product_category, SUM(revenue) as total_revenue FROM products p JOIN sales s ON p.id = s.product_id GROUP BY product_category',
    'SELECT DATE_TRUNC(\'month\', order_date) as month, COUNT(*) as orders FROM orders WHERE order_date >= \'2023-01-01\' GROUP BY month'
  ]

  // Handle schema table selection
  const handleTableSelect = (table: any) => {
    console.log('Table selected:', table)
    setDataContext(table.name)
  }

  // Handle query optimization
  const handleOptimizationApply = (optimization: any) => {
    console.log('Optimization applied:', optimization)
    setCurrentQuery(optimization.optimizedQuery)
  }

  // Handle insight generation
  const handleInsightGenerate = (insight: any) => {
    console.log('Insight generated:', insight)
  }

  // Handle forecast generation
  const handleForecastGenerate = (forecast: any) => {
    console.log('Forecast generated:', forecast)
  }

  const tabItems = [
    {
      key: 'schema',
      label: (
        <Space>
          <DatabaseOutlined />
          <span>Schema Intelligence</span>
        </Space>
      ),
      children: (
        <div>
          <Alert
            message="AI Schema Explorer"
            description="Intelligent schema discovery with AI-powered recommendations and business context integration."
            type="info"
            showIcon
            style={{ marginBottom: 16 }}
          />
          <AISchemaExplorer
            currentQuery={currentQuery}
            showRecommendations={true}
            showRelationships={true}
            interactive={true}
            onTableSelect={handleTableSelect}
            onColumnSelect={(column) => console.log('Column selected:', column)}
            onRecommendationApply={(rec) => console.log('Recommendation applied:', rec)}
          />
        </div>
      )
    },
    {
      key: 'optimization',
      label: (
        <Space>
          <ThunderboltOutlined />
          <span>Query Optimization</span>
        </Space>
      ),
      children: (
        <div>
          <Alert
            message="AI Query Optimization Engine"
            description="Advanced query optimization with performance analysis and AI-powered suggestions."
            type="success"
            showIcon
            style={{ marginBottom: 16 }}
          />
          
          {/* Query Input */}
          <Card title="Query Input" size="small" style={{ marginBottom: 16 }}>
            <Space direction="vertical" style={{ width: '100%' }}>
              <div>
                <Text strong>Current Query:</Text>
                <div style={{ marginTop: 8 }}>
                  <TextArea
                    value={currentQuery}
                    onChange={(e) => setCurrentQuery(e.target.value)}
                    rows={4}
                    placeholder="Enter your SQL query here..."
                    style={{ fontFamily: 'monospace' }}
                  />
                </div>
              </div>
              <div>
                <Text strong>Sample Queries:</Text>
                <div style={{ marginTop: 8 }}>
                  <Space wrap>
                    {sampleQueries.map((query, index) => (
                      <Button
                        key={index}
                        size="small"
                        onClick={() => setCurrentQuery(query)}
                      >
                        Sample {index + 1}
                      </Button>
                    ))}
                  </Space>
                </div>
              </div>
            </Space>
          </Card>

          <QueryOptimizationEngine
            query={currentQuery}
            showPerformanceMetrics={true}
            showOptimizationHistory={true}
            autoOptimize={false}
            onOptimizationApply={handleOptimizationApply}
            onQueryUpdate={setCurrentQuery}
          />
        </div>
      )
    },
    {
      key: 'insights',
      label: (
        <Space>
          <BulbOutlined />
          <span>Automated Insights</span>
        </Space>
      ),
      children: (
        <div>
          <Alert
            message="AI Insights Generator"
            description="Automated pattern recognition, anomaly detection, and business intelligence recommendations."
            type="warning"
            showIcon
            style={{ marginBottom: 16 }}
          />
          
          {/* Data Context Input */}
          <Card title="Data Context" size="small" style={{ marginBottom: 16 }}>
            <Space>
              <Text strong>Context:</Text>
              <Input
                value={dataContext}
                onChange={(e) => setDataContext(e.target.value)}
                placeholder="Enter data context (e.g., sales_analytics, customer_behavior)"
                style={{ width: 300 }}
              />
            </Space>
          </Card>

          <AutomatedInsightsGenerator
            dataContext={dataContext}
            insightCategories={['performance', 'cost', 'usage', 'anomaly', 'trend']}
            showTrends={true}
            showRecommendations={true}
            autoRefresh={false}
            refreshInterval={30000}
            onInsightGenerate={handleInsightGenerate}
            onInsightApply={(insight) => console.log('Insight applied:', insight)}
          />
        </div>
      )
    },
    {
      key: 'predictive',
      label: (
        <Space>
          <BarChartOutlined />
          <span>Predictive Analytics</span>
        </Space>
      ),
      children: (
        <div>
          <Alert
            message="AI Predictive Analytics"
            description="Advanced forecasting, trend analysis, and anomaly detection with machine learning models."
            type="error"
            showIcon
            style={{ marginBottom: 16 }}
          />
          
          <PredictiveAnalyticsPanel
            dataSource={dataContext}
            metrics={['usage', 'performance', 'cost', 'revenue']}
            forecastPeriod={30}
            showConfidenceIntervals={true}
            showTrendAnalysis={true}
            showAnomalyDetection={true}
            onForecastGenerate={handleForecastGenerate}
            onModelSelect={(model) => console.log('Model selected:', model)}
          />
        </div>
      )
    }
  ]

  return (
    <AIFeatureWrapper feature="advancedAIEnabled">
      <div style={{ padding: 24 }}>
        {/* Header */}
        <div style={{ 
          display: 'flex', 
          justifyContent: 'space-between', 
          alignItems: 'center',
          marginBottom: 24 
        }}>
          <div>
            <Title level={2} style={{ margin: 0 }}>
              <Space>
                <RobotOutlined />
                Advanced AI Features Demo
              </Space>
            </Title>
            <Text type="secondary">
              Cutting-edge AI intelligence, optimization, and predictive analytics
            </Text>
          </div>
          <Space>
            <Button icon={<SearchOutlined />}>
              Explore Features
            </Button>
            <Button icon={<ExperimentOutlined />}>
              Run Experiments
            </Button>
          </Space>
        </div>

        {/* Feature Overview Cards */}
        <Row gutter={[16, 16]} style={{ marginBottom: 24 }}>
          <Col xs={24} sm={12} lg={6}>
            <Card>
              <div style={{ textAlign: 'center' }}>
                <DatabaseOutlined style={{ fontSize: '24px', color: '#1890ff', marginBottom: 8 }} />
                <div style={{ fontSize: '16px', fontWeight: 'bold' }}>Schema Intelligence</div>
                <div style={{ color: '#666', fontSize: '12px' }}>AI-powered schema discovery</div>
              </div>
            </Card>
          </Col>
          <Col xs={24} sm={12} lg={6}>
            <Card>
              <div style={{ textAlign: 'center' }}>
                <ThunderboltOutlined style={{ fontSize: '24px', color: '#52c41a', marginBottom: 8 }} />
                <div style={{ fontSize: '16px', fontWeight: 'bold' }}>Query Optimization</div>
                <div style={{ color: '#666', fontSize: '12px' }}>Performance analysis & tuning</div>
              </div>
            </Card>
          </Col>
          <Col xs={24} sm={12} lg={6}>
            <Card>
              <div style={{ textAlign: 'center' }}>
                <BulbOutlined style={{ fontSize: '24px', color: '#faad14', marginBottom: 8 }} />
                <div style={{ fontSize: '16px', fontWeight: 'bold' }}>Automated Insights</div>
                <div style={{ color: '#666', fontSize: '12px' }}>Pattern recognition & alerts</div>
              </div>
            </Card>
          </Col>
          <Col xs={24} sm={12} lg={6}>
            <Card>
              <div style={{ textAlign: 'center' }}>
                <BarChartOutlined style={{ fontSize: '24px', color: '#722ed1', marginBottom: 8 }} />
                <div style={{ fontSize: '16px', fontWeight: 'bold' }}>Predictive Analytics</div>
                <div style={{ color: '#666', fontSize: '12px' }}>Forecasting & trend analysis</div>
              </div>
            </Card>
          </Col>
        </Row>

        {/* Main AI Features Interface */}
        <Card>
          <Tabs
            activeKey={activeTab}
            onChange={setActiveTab}
            items={tabItems}
            size="large"
          />
        </Card>

        {/* Integration Information */}
        <Card style={{ marginTop: 24 }}>
          <Title level={4}>
            <Space>
              <RobotOutlined />
              Advanced AI Integration
            </Space>
          </Title>
          <Row gutter={[16, 16]}>
            <Col xs={24} lg={12}>
              <div>
                <Text strong>AI Intelligence Features:</Text>
                <ul style={{ marginTop: 8, paddingLeft: 20 }}>
                  <li>Schema discovery with business context understanding</li>
                  <li>Query optimization with performance impact analysis</li>
                  <li>Automated pattern recognition and anomaly detection</li>
                  <li>Predictive forecasting with multiple ML models</li>
                </ul>
              </div>
            </Col>
            <Col xs={24} lg={12}>
              <div>
                <Text strong>Backend Integration:</Text>
                <ul style={{ marginTop: 8, paddingLeft: 20 }}>
                  <li>Real-time AI analysis via IntelligentAgentsController</li>
                  <li>Machine learning model orchestration</li>
                  <li>Advanced analytics pipeline integration</li>
                  <li>Confidence scoring and uncertainty quantification</li>
                </ul>
              </div>
            </Col>
          </Row>
          
          <Divider />
          
          <Alert
            message="Advanced AI Capabilities"
            description={
              <div>
                <Text>This demo showcases the most advanced AI features:</Text>
                <ul style={{ marginTop: 8, paddingLeft: 20, marginBottom: 0 }}>
                  <li><strong>Schema Intelligence:</strong> AI understands database structure and business context</li>
                  <li><strong>Query Optimization:</strong> Automatic performance analysis and optimization suggestions</li>
                  <li><strong>Automated Insights:</strong> Pattern recognition, anomaly detection, and business recommendations</li>
                  <li><strong>Predictive Analytics:</strong> Time series forecasting, trend analysis, and predictive modeling</li>
                </ul>
              </div>
            }
            type="info"
            showIcon
          />
        </Card>
      </div>
    </AIFeatureWrapper>
  )
}

export default AdvancedAIFeaturesDemo
