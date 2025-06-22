import React, { useState } from 'react'
import { Card, Row, Col, Space, Typography, Button, Divider, Alert, Tabs, Select, Input, Spin } from 'antd'
import {
  EyeOutlined,
  BarChartOutlined,
  BulbOutlined,
  RobotOutlined,
  ExperimentOutlined,
  SearchOutlined,
  ReloadOutlined
} from '@ant-design/icons'
import { AITransparencyPanel } from '@shared/components/ai/transparency/AITransparencyPanel'
import { PromptConstructionViewer } from '@shared/components/ai/transparency/PromptConstructionViewer'
import { ConfidenceBreakdownChart } from '@shared/components/ai/transparency/ConfidenceBreakdownChart'
import { AIDecisionExplainer } from '@shared/components/ai/transparency/AIDecisionExplainer'
import { ConfidenceIndicator } from '@shared/components/ai/common/ConfidenceIndicator'
import { AIFeatureWrapper } from '@shared/components/ai/common/AIFeatureWrapper'
import {
  useGetTransparencyTraceQuery,
  useGetConfidenceBreakdownQuery,
  useGetAlternativeOptionsQuery,
  useGetTransparencyTracesQuery,
  useGetTransparencyDashboardMetricsQuery
} from '@shared/store/api/transparencyApi'

const { Title, Text, Paragraph } = Typography

/**
 * AITransparencyAnalysis - AI transparency analysis and exploration page
 *
 * This page provides:
 * - Real-time transparency data from API
 * - Interactive trace selection and analysis
 * - Confidence breakdown and metrics
 * - Decision explanation and reasoning
 * - Alternative options and recommendations
 */
export const AITransparencyAnalysis: React.FC = () => {
  const [activeTab, setActiveTab] = useState('overview')
  const [selectedTraceId, setSelectedTraceId] = useState<string | undefined>(undefined)

  // API queries
  const { data: recentTraces, isLoading: tracesLoading } = useGetTransparencyTracesQuery({
    page: 1,
    pageSize: 20,
    sortBy: 'createdAt',
    sortOrder: 'desc'
  })

  const { data: traceData, isLoading: traceLoading, error: traceError } = useGetTransparencyTraceQuery(
    selectedTraceId!,
    { skip: !selectedTraceId }
  )

  const { data: confidenceData, isLoading: confidenceLoading } = useGetConfidenceBreakdownQuery(
    selectedTraceId!,
    { skip: !selectedTraceId }
  )

  const { data: alternativeOptions } = useGetAlternativeOptionsQuery(
    selectedTraceId!,
    { skip: !selectedTraceId }
  )

  const { data: dashboardMetrics } = useGetTransparencyDashboardMetricsQuery({ days: 7 })

  // Use first available trace if none selected
  React.useEffect(() => {
    if (!selectedTraceId && recentTraces?.traces?.length > 0) {
      setSelectedTraceId(recentTraces.traces[0].traceId)
    }
  }, [recentTraces, selectedTraceId])

  // Handle loading states
  if (tracesLoading) {
    return (
      <div style={{ padding: 24, textAlign: 'center' }}>
        <Spin size="large" />
        <div style={{ marginTop: 16 }}>
          <Text>Loading transparency traces...</Text>
        </div>
      </div>
    )
  }

  // Function to generate test data
  const generateTestData = async () => {
    try {
      const testQueries = [
        'Show me sales data for Q4 2024',
        'What are the top performing products this year?',
        'Compare revenue trends by region for last 6 months',
        'Find customers with highest lifetime value',
        'Generate monthly sales report with trends'
      ]

      for (const query of testQueries) {
        await fetch('/api/test/query/enhanced', {
          method: 'POST',
          headers: {
            'Content-Type': 'application/json',
          },
          body: JSON.stringify({
            query: query,
            executeQuery: true,
            includeAlternatives: true,
            includeSemanticAnalysis: true,
          }),
        })
        // Small delay between requests
        await new Promise(resolve => setTimeout(resolve, 1000))
      }

      // Refresh the page after generating data
      window.location.reload()
    } catch (error) {
      console.error('Error generating test data:', error)
    }
  }

  // Handle no data state
  if (!recentTraces?.traces?.length) {
    return (
      <div style={{ padding: 24 }}>
        <Alert
          message="No Transparency Data Available"
          description="No transparency traces found. Generate some test data or run AI queries to create transparency traces."
          type="info"
          showIcon
          action={
            <Space>
              <Button
                type="primary"
                onClick={generateTestData}
                loading={tracesLoading}
              >
                Generate Test Data
              </Button>
              <Button onClick={() => window.location.reload()}>
                <ReloadOutlined /> Refresh
              </Button>
            </Space>
          }
        />
      </div>
    )
  }

  // Handle trace loading error
  if (traceError) {
    return (
      <div style={{ padding: 24 }}>
        <Alert
          message="Error Loading Transparency Data"
          description="Failed to load transparency trace data. Please try again."
          type="error"
          showIcon
          action={
            <Button onClick={() => window.location.reload()}>
              <ReloadOutlined /> Retry
            </Button>
          }
        />
      </div>
    )
  }

  // Transform API data for components
  const currentTrace = traceData || null
  const currentConfidence = confidenceData || null

  // Create explanation object from API data
  const explanationData = currentTrace ? {
    decision: currentTrace.summary || 'AI decision analysis',
    reasoning: [
      `Intent Type: ${currentTrace.intentType}`,
      `User Question: ${currentTrace.userQuestion}`,
      ...(currentTrace.detailedMetrics?.reasoning || [])
    ],
    confidence: currentConfidence?.overallConfidence || 0,
    alternatives: alternativeOptions || [],
    factors: currentConfidence?.confidenceFactors || [],
    recommendations: currentTrace.optimizationSuggestions || []
  } : null

  const tabItems = [
    {
      key: 'overview',
      label: (
        <Space>
          <EyeOutlined />
          <span>Overview</span>
        </Space>
      ),
      children: (
        <div>
          {/* Trace Selector */}
          <Card size="small" style={{ marginBottom: 24 }}>
            <Row gutter={16} align="middle">
              <Col span={6}>
                <Text strong>Select Trace:</Text>
              </Col>
              <Col span={12}>
                <Select
                  style={{ width: '100%' }}
                  placeholder="Select a transparency trace"
                  value={selectedTraceId}
                  onChange={setSelectedTraceId}
                  loading={tracesLoading}
                >
                  {recentTraces?.traces?.map(trace => (
                    <Select.Option key={trace.traceId} value={trace.traceId}>
                      <div>
                        <Text strong>{trace.userQuestion}</Text>
                        <br />
                        <Text type="secondary" style={{ fontSize: '12px' }}>
                          {trace.intentType} • {new Date(trace.createdAt).toLocaleString()} •
                          Confidence: {(trace.overallConfidence * 100).toFixed(0)}%
                        </Text>
                      </div>
                    </Select.Option>
                  ))}
                </Select>
              </Col>
              <Col span={6}>
                <Button
                  icon={<ReloadOutlined />}
                  onClick={() => window.location.reload()}
                  loading={traceLoading}
                >
                  Refresh
                </Button>
              </Col>
            </Row>
          </Card>

          {/* Content */}
          {traceLoading ? (
            <div style={{ textAlign: 'center', padding: '40px 0' }}>
              <Spin size="large" />
              <div style={{ marginTop: 16 }}>
                <Text>Loading trace data...</Text>
              </div>
            </div>
          ) : currentTrace ? (
            <Row gutter={[24, 24]}>
              <Col span={12}>
                <Card title="Query Information" size="small">
                  <Space direction="vertical" style={{ width: '100%' }}>
                    <div>
                      <Text strong>User Question:</Text>
                      <div style={{ marginTop: 4 }}>
                        <Text>{currentTrace.userQuestion}</Text>
                      </div>
                    </div>
                    <div>
                      <Text strong>Intent Type:</Text>
                      <div style={{ marginTop: 4 }}>
                        <Text>{currentTrace.intentType}</Text>
                      </div>
                    </div>
                    <div>
                      <Text strong>Generated At:</Text>
                      <div style={{ marginTop: 4 }}>
                        <Text type="secondary">{new Date(currentTrace.generatedAt).toLocaleString()}</Text>
                      </div>
                    </div>
                  </Space>
                </Card>
              </Col>
              <Col span={12}>
                <Card title="Overall Confidence" size="small">
                  <div style={{ textAlign: 'center', padding: '20px 0' }}>
                    <ConfidenceIndicator
                      confidence={currentConfidence?.overallConfidence || 0}
                      type="circle"
                      size="large"
                      showLabel={true}
                    />
                    <div style={{ marginTop: 16 }}>
                      <Text type="secondary">
                        Trace ID: {currentTrace.traceId}
                      </Text>
                    </div>
                  </div>
                </Card>
              </Col>
              <Col span={24}>
                <Card title="Summary" size="small">
                  <Text>{currentTrace.summary}</Text>
                  {currentTrace.errorMessage && (
                    <Alert
                      message="Error"
                      description={currentTrace.errorMessage}
                      type="error"
                      style={{ marginTop: 16 }}
                    />
                  )}
                </Card>
              </Col>
            </Row>
          ) : (
            <Alert
              message="No Trace Selected"
              description="Please select a transparency trace to view details."
              type="info"
            />
          )}
        </div>
      )
    },
    {
      key: 'confidence-analysis',
      label: (
        <Space>
          <BarChartOutlined />
          <span>Confidence Analysis</span>
        </Space>
      ),
      children: (
        <div>
          {currentConfidence ? (
            <Row gutter={[24, 24]}>
              <Col span={16}>
                <Card title="Confidence Breakdown" size="small">
                  <ConfidenceBreakdownChart
                    analysis={{
                      overallConfidence: currentConfidence.overallConfidence,
                      factors: currentConfidence.confidenceFactors,
                      breakdown: currentConfidence.factorBreakdown,
                      recommendations: []
                    }}
                    showFactors={true}
                    interactive={true}
                    chartType="bar"
                  />
                </Card>
              </Col>
              <Col span={8}>
                <Card title="Confidence Factors" size="small">
                  <Space direction="vertical" style={{ width: '100%' }}>
                    <div>
                      <Text strong>Overall Confidence:</Text>
                      <div style={{ marginTop: 8 }}>
                        <ConfidenceIndicator
                          confidence={currentConfidence.overallConfidence}
                          type="circle"
                          size="large"
                          showLabel={true}
                        />
                      </div>
                    </div>
                    {currentConfidence.confidenceFactors?.slice(0, 3).map((factor, index) => (
                      <div key={index}>
                        <Text strong>{factor.factorName}:</Text>
                        <div style={{ marginTop: 8 }}>
                          <ConfidenceIndicator
                            confidence={factor.score}
                            type="bar"
                            size="medium"
                            showLabel={true}
                          />
                        </div>
                      </div>
                    ))}
                  </Space>
                </Card>
              </Col>
            </Row>
          ) : (
            <Alert
              message="No Confidence Data"
              description="Confidence analysis data is not available for this trace."
              type="info"
            />
          )}
        </div>
      )
    },
    {
      key: 'decision-explanation',
      label: (
        <Space>
          <BulbOutlined />
          <span>Decision Explanation</span>
        </Space>
      ),
      children: (
        <div>
          {explanationData ? (
            <Card title="AI Reasoning & Recommendations" size="small">
              <AIDecisionExplainer
                explanation={explanationData}
                showAlternatives={true}
                showRecommendations={true}
              />
            </Card>
          ) : (
            <Alert
              message="No Explanation Data"
              description="Decision explanation data is not available for this trace."
              type="info"
            />
          )}
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
