import React from 'react'
import { 
  Modal, 
  Card, 
  Row, 
  Col, 
  Statistic, 
  Progress, 
  Typography, 
  Tag, 
  Alert, 
  Divider,
  Space,
  Button
} from 'antd'
import { 
  TrophyOutlined, 
  RiseOutlined, 
  FallOutlined,
  InfoCircleOutlined,
  CheckCircleOutlined,
  CloseCircleOutlined
} from '@ant-design/icons'
import { PerformanceLineChart, PerformanceBarChart } from '@shared/components/charts/PerformanceChart'
import { useGetABTestAnalysisQuery } from '@shared/store/api/templateAnalyticsApi'

const { Title, Text } = Typography

interface StatisticalAnalysisPanelProps {
  visible: boolean
  onCancel: () => void
  testData: any
}

export const StatisticalAnalysisPanel: React.FC<StatisticalAnalysisPanelProps> = ({
  visible,
  onCancel,
  testData
}) => {
  const { data: analysis, isLoading } = useGetABTestAnalysisQuery(
    testData?.id, 
    { skip: !testData?.id }
  )

  if (!testData) return null

  const getRecommendationColor = (recommendation: string) => {
    switch (recommendation) {
      case 'implement_variant': return 'green'
      case 'keep_original': return 'blue'
      case 'continue_test': return 'orange'
      case 'inconclusive': return 'red'
      default: return 'default'
    }
  }

  const getRecommendationIcon = (recommendation: string) => {
    switch (recommendation) {
      case 'implement_variant': return <CheckCircleOutlined />
      case 'keep_original': return <CloseCircleOutlined />
      case 'continue_test': return <InfoCircleOutlined />
      case 'inconclusive': return <InfoCircleOutlined />
      default: return <InfoCircleOutlined />
    }
  }

  const getRecommendationText = (recommendation: string) => {
    switch (recommendation) {
      case 'implement_variant': return 'Implement Variant'
      case 'keep_original': return 'Keep Original'
      case 'continue_test': return 'Continue Test'
      case 'inconclusive': return 'Inconclusive'
      default: return recommendation
    }
  }

  const calculateImprovement = () => {
    if (!analysis) return 0
    const originalRate = analysis.originalMetrics.successRate
    const variantRate = analysis.variantMetrics.successRate
    return ((variantRate - originalRate) / originalRate * 100)
  }

  const isStatisticallySignificant = analysis?.statisticalSignificance >= 0.95

  const comparisonData = [
    {
      metric: 'Success Rate',
      original: analysis?.originalMetrics.successRate * 100 || 0,
      variant: analysis?.variantMetrics.successRate * 100 || 0
    },
    {
      metric: 'Avg Confidence',
      original: analysis?.originalMetrics.averageConfidenceScore * 100 || 0,
      variant: analysis?.variantMetrics.averageConfidenceScore * 100 || 0
    },
    {
      metric: 'Response Time',
      original: analysis?.originalMetrics.averageResponseTime || 0,
      variant: analysis?.variantMetrics.averageResponseTime || 0
    }
  ]

  return (
    <Modal
      title={`Statistical Analysis: ${testData.testName}`}
      open={visible}
      onCancel={onCancel}
      width={1000}
      footer={[
        <Button key="close" onClick={onCancel}>
          Close
        </Button>
      ]}
    >
      <div>
        {/* Test Overview */}
        <Card style={{ marginBottom: '16px' }}>
          <Row gutter={16}>
            <Col span={6}>
              <Statistic
                title="Test Status"
                value={testData.status}
                valueStyle={{ color: testData.status === 'running' ? '#1890ff' : '#52c41a' }}
              />
            </Col>
            <Col span={6}>
              <Statistic
                title="Total Samples"
                value={testData.originalUsageCount + testData.variantUsageCount}
                suffix={`/ ${testData.minimumSampleSize}`}
              />
            </Col>
            <Col span={6}>
              <Statistic
                title="Traffic Split"
                value={`${testData.trafficSplitPercent}%`}
                suffix="Variant"
              />
            </Col>
            <Col span={6}>
              <Statistic
                title="Confidence Level"
                value={`${testData.confidenceLevel}%`}
              />
            </Col>
          </Row>
        </Card>

        {/* Statistical Significance */}
        {analysis && (
          <Alert
            message={
              <Space>
                <span>Statistical Significance:</span>
                <Tag 
                  color={isStatisticallySignificant ? 'green' : 'orange'}
                  icon={isStatisticallySignificant ? <CheckCircleOutlined /> : <InfoCircleOutlined />}
                >
                  {(analysis.statisticalSignificance * 100).toFixed(1)}%
                </Tag>
                <span>P-Value: {analysis.pValue.toFixed(4)}</span>
              </Space>
            }
            description={
              isStatisticallySignificant 
                ? "The results are statistically significant. You can be confident in the findings."
                : "The results are not yet statistically significant. Consider running the test longer."
            }
            type={isStatisticallySignificant ? 'success' : 'warning'}
            showIcon
            style={{ marginBottom: '16px' }}
          />
        )}

        {/* Performance Comparison */}
        <Row gutter={16} style={{ marginBottom: '16px' }}>
          <Col span={12}>
            <Card title="Original Template Performance">
              <Row gutter={16}>
                <Col span={12}>
                  <Statistic
                    title="Success Rate"
                    value={(testData.originalSuccessRate * 100).toFixed(1)}
                    suffix="%"
                    valueStyle={{ color: '#1890ff' }}
                  />
                </Col>
                <Col span={12}>
                  <Statistic
                    title="Sample Size"
                    value={testData.originalUsageCount}
                    valueStyle={{ color: '#1890ff' }}
                  />
                </Col>
              </Row>
            </Card>
          </Col>
          <Col span={12}>
            <Card title="Variant Template Performance">
              <Row gutter={16}>
                <Col span={12}>
                  <Statistic
                    title="Success Rate"
                    value={(testData.variantSuccessRate * 100).toFixed(1)}
                    suffix="%"
                    valueStyle={{ color: '#52c41a' }}
                    prefix={
                      calculateImprovement() > 0 ? <RiseOutlined /> : 
                      calculateImprovement() < 0 ? <FallOutlined /> : null
                    }
                  />
                </Col>
                <Col span={12}>
                  <Statistic
                    title="Sample Size"
                    value={testData.variantUsageCount}
                    valueStyle={{ color: '#52c41a' }}
                  />
                </Col>
              </Row>
            </Card>
          </Col>
        </Row>

        {/* Improvement Metrics */}
        {analysis && (
          <Card title="Performance Improvement" style={{ marginBottom: '16px' }}>
            <Row gutter={16}>
              <Col span={8}>
                <Statistic
                  title="Relative Improvement"
                  value={calculateImprovement().toFixed(2)}
                  suffix="%"
                  valueStyle={{ 
                    color: calculateImprovement() > 0 ? '#52c41a' : '#f5222d' 
                  }}
                  prefix={
                    calculateImprovement() > 0 ? <RiseOutlined /> : <FallOutlined />
                  }
                />
              </Col>
              <Col span={8}>
                <Statistic
                  title="Effect Size"
                  value={analysis.effectSize.toFixed(3)}
                  valueStyle={{ color: '#722ed1' }}
                />
              </Col>
              <Col span={8}>
                <div>
                  <Text strong>Confidence Interval</Text>
                  <div style={{ fontSize: '24px', fontWeight: 600, color: '#fa8c16' }}>
                    [{analysis.confidenceInterval.lower.toFixed(2)}, {analysis.confidenceInterval.upper.toFixed(2)}]
                  </div>
                </div>
              </Col>
            </Row>
          </Card>
        )}

        {/* Recommendation */}
        {analysis && (
          <Card title="Recommendation" style={{ marginBottom: '16px' }}>
            <Space direction="vertical" style={{ width: '100%' }}>
              <Tag 
                color={getRecommendationColor(analysis.recommendation)}
                icon={getRecommendationIcon(analysis.recommendation)}
                style={{ fontSize: '16px', padding: '8px 16px' }}
              >
                {getRecommendationText(analysis.recommendation)}
              </Tag>
              
              {analysis.recommendation === 'implement_variant' && (
                <Alert
                  message="Variant is performing significantly better"
                  description="The variant template shows statistically significant improvement. Consider implementing it as the new default."
                  type="success"
                  showIcon
                />
              )}
              
              {analysis.recommendation === 'keep_original' && (
                <Alert
                  message="Original template is performing better"
                  description="The original template is performing better than the variant. Keep using the original template."
                  type="info"
                  showIcon
                />
              )}
              
              {analysis.recommendation === 'continue_test' && (
                <Alert
                  message="Continue testing for more data"
                  description="The test needs more samples to reach statistical significance. Continue running the test."
                  type="warning"
                  showIcon
                />
              )}
            </Space>
          </Card>
        )}

        {/* Performance Comparison Chart */}
        <Card title="Performance Comparison">
          <PerformanceBarChart
            data={comparisonData}
            xAxisKey="metric"
            yAxisKey="original"
            height={300}
            color="#1890ff"
          />
        </Card>
      </div>
    </Modal>
  )
}

export default StatisticalAnalysisPanel
