import React, { useState } from 'react'
import { 
  Card, 
  Table, 
  Button, 
  Modal, 
  Form, 
  Input, 
  Select, 
  Progress, 
  Tag, 
  Space, 
  Typography,
  Row,
  Col,
  Statistic,
  Alert,
  Tooltip,
  Divider,
  Tabs
} from 'antd'
import { 
  PlusOutlined, 
  PlayCircleOutlined, 
  PauseCircleOutlined, 
  CheckCircleOutlined,
  StopOutlined,
  BarChartOutlined,
  TrophyOutlined,
  ExperimentOutlined,
  InfoCircleOutlined
} from '@ant-design/icons'
import { 
  useGetABTestsQuery,
  useCreateABTestMutation,
  useCompleteABTestMutation,
  usePauseABTestMutation,
  useResumeABTestMutation,
  useGetABTestRecommendationsQuery
} from '@shared/store/api/templateAnalyticsApi'
import { MetricCard } from '@shared/components/charts/PerformanceChart'

import TestCreationWizard from '../../components/template-analytics/TestCreationWizard'
import StatisticalAnalysisPanel from '../../components/template-analytics/StatisticalAnalysisPanel'
import ABTestResults from '../../components/template-analytics/ABTestResults'
import TestRecommendations from '../../components/template-analytics/TestRecommendations'

const { Title, Text } = Typography
const { TextArea } = Input

export const ABTestingDashboard: React.FC = () => {
  const [isCreateModalVisible, setIsCreateModalVisible] = useState(false)
  const [selectedTest, setSelectedTest] = useState<any>(null)
  const [isAnalysisModalVisible, setIsAnalysisModalVisible] = useState(false)
  const [activeTab, setActiveTab] = useState('active')
  const [form] = Form.useForm()

  const { data: activeTests, isLoading } = useGetABTestsQuery({ status: 'running' })
  const { data: completedTests } = useGetABTestsQuery({ status: 'completed' })
  const { data: recommendations } = useGetABTestRecommendationsQuery({})
  
  const [createABTest] = useCreateABTestMutation()
  const [completeTest] = useCompleteABTestMutation()
  const [pauseTest] = usePauseABTestMutation()
  const [resumeTest] = useResumeABTestMutation()

  const handleCreateTest = async (values: any) => {
    try {
      await createABTest({
        testName: values.testName,
        originalTemplateKey: values.originalTemplate,
        variantTemplateContent: values.variantContent,
        trafficSplitPercent: values.trafficSplit,
        startDate: new Date().toISOString(),
        endDate: new Date(Date.now() + 30 * 24 * 60 * 60 * 1000).toISOString(), // 30 days
        createdBy: 'current-user', // Get from auth context
        minimumSampleSize: values.minimumSampleSize || 1000,
        confidenceLevel: values.confidenceLevel || 95
      }).unwrap()

      setIsCreateModalVisible(false)
      form.resetFields()
    } catch (error) {
      console.error('Failed to create A/B test:', error)
    }
  }

  const handleCompleteTest = async (testId: number) => {
    try {
      await completeTest({ testId, implementWinner: true }).unwrap()
    } catch (error) {
      console.error('Failed to complete test:', error)
    }
  }

  const handlePauseTest = async (testId: number) => {
    try {
      await pauseTest(testId).unwrap()
    } catch (error) {
      console.error('Failed to pause test:', error)
    }
  }

  const handleResumeTest = async (testId: number) => {
    try {
      await resumeTest(testId).unwrap()
    } catch (error) {
      console.error('Failed to resume test:', error)
    }
  }

  const getStatusTag = (status: string) => {
    const statusConfig = {
      running: { color: 'blue', icon: <PlayCircleOutlined /> },
      paused: { color: 'orange', icon: <PauseCircleOutlined /> },
      completed: { color: 'green', icon: <CheckCircleOutlined /> },
      cancelled: { color: 'red', icon: <StopOutlined /> }
    }

    const config = statusConfig[status as keyof typeof statusConfig] || statusConfig.running

    return (
      <Tag color={config.color} icon={config.icon}>
        {status.toUpperCase()}
      </Tag>
    )
  }

  const getSignificanceIndicator = (significance?: number) => {
    if (!significance) return null
    
    const isSignificant = significance >= 0.95
    return (
      <Tooltip title={`Statistical significance: ${(significance * 100).toFixed(1)}%`}>
        <Tag color={isSignificant ? 'green' : 'orange'}>
          {isSignificant ? 'Significant' : 'Not Significant'}
        </Tag>
      </Tooltip>
    )
  }

  const activeTestColumns = [
    {
      title: 'Test Name',
      dataIndex: 'testName',
      key: 'testName',
      render: (text: string, record: any) => (
        <div>
          <div style={{ fontWeight: 600 }}>{text}</div>
          <Text type="secondary" style={{ fontSize: '12px' }}>
            {record.originalTemplateKey} vs Variant
          </Text>
        </div>
      )
    },
    {
      title: 'Status',
      dataIndex: 'status',
      key: 'status',
      render: (status: string) => getStatusTag(status)
    },
    {
      title: 'Progress',
      key: 'progress',
      render: (record: any) => {
        const totalSamples = record.originalUsageCount + record.variantUsageCount
        const targetSamples = record.minimumSampleSize || 1000
        const progress = Math.min((totalSamples / targetSamples) * 100, 100)

        return (
          <div>
            <Progress 
              percent={Math.round(progress)} 
              size="small" 
              status={progress >= 100 ? 'success' : 'active'}
            />
            <Text style={{ fontSize: '12px', color: '#666' }}>
              {totalSamples} / {targetSamples} samples
            </Text>
          </div>
        )
      }
    },
    {
      title: 'Performance',
      key: 'performance',
      render: (record: any) => (
        <div>
          <div style={{ display: 'flex', justifyContent: 'space-between', marginBottom: '4px' }}>
            <Text style={{ fontSize: '12px' }}>Original:</Text>
            <Text style={{ fontSize: '12px', fontWeight: 600 }}>
              {((record.originalSuccessRate || 0) * 100).toFixed(1)}%
            </Text>
          </div>
          <div style={{ display: 'flex', justifyContent: 'space-between', marginBottom: '4px' }}>
            <Text style={{ fontSize: '12px' }}>Variant:</Text>
            <Text style={{ fontSize: '12px', fontWeight: 600 }}>
              {((record.variantSuccessRate || 0) * 100).toFixed(1)}%
            </Text>
          </div>
          {getSignificanceIndicator(record.statisticalSignificance)}
        </div>
      )
    },
    {
      title: 'Actions',
      key: 'actions',
      render: (record: any) => (
        <Space>
          <Button 
            size="small" 
            icon={<BarChartOutlined />}
            onClick={() => {
              setSelectedTest(record)
              setIsAnalysisModalVisible(true)
            }}
          >
            Analyze
          </Button>
          {record.status === 'running' ? (
            <Button 
              size="small" 
              icon={<PauseCircleOutlined />}
              onClick={() => handlePauseTest(record.id)}
            >
              Pause
            </Button>
          ) : record.status === 'paused' ? (
            <Button 
              size="small" 
              icon={<PlayCircleOutlined />}
              onClick={() => handleResumeTest(record.id)}
            >
              Resume
            </Button>
          ) : null}
          <Button 
            size="small" 
            type="primary"
            icon={<CheckCircleOutlined />}
            onClick={() => handleCompleteTest(record.id)}
            disabled={record.status !== 'running'}
          >
            Complete
          </Button>
        </Space>
      )
    }
  ]

  const getDashboardMetrics = () => {
    const totalActive = activeTests?.length || 0
    const totalCompleted = completedTests?.length || 0
    const significantTests = activeTests?.filter(test => 
      test.statisticalSignificance && test.statisticalSignificance >= 0.95
    ).length || 0

    return {
      totalActive,
      totalCompleted,
      significantTests,
      totalTests: totalActive + totalCompleted
    }
  }

  const metrics = getDashboardMetrics()

  return (
    <div>
        {/* Header */}
        <div style={{ marginBottom: '24px' }}>
          <Row gutter={16} align="middle">
            <Col span={12}>
              <Title level={2} style={{ margin: 0 }}>
                A/B Testing Dashboard
              </Title>
              <Text type="secondary">
                Manage and analyze template A/B tests
              </Text>
            </Col>
            <Col span={12} style={{ textAlign: 'right' }}>
              <Button
                type="primary"
                icon={<PlusOutlined />}
                onClick={() => setIsCreateModalVisible(true)}
                size="large"
              >
                Create Test
              </Button>
            </Col>
          </Row>
        </div>

        {/* Metrics Overview */}
        <Row gutter={16} style={{ marginBottom: '24px' }}>
          <Col span={6}>
            <MetricCard
              title="Active Tests"
              value={metrics.totalActive}
              icon={<ExperimentOutlined />}
              color="#1890ff"
            />
          </Col>
          <Col span={6}>
            <MetricCard
              title="Completed Tests"
              value={metrics.totalCompleted}
              icon={<CheckCircleOutlined />}
              color="#52c41a"
            />
          </Col>
          <Col span={6}>
            <MetricCard
              title="Significant Results"
              value={metrics.significantTests}
              icon={<TrophyOutlined />}
              color="#722ed1"
            />
          </Col>
          <Col span={6}>
            <MetricCard
              title="Total Tests"
              value={metrics.totalTests}
              icon={<BarChartOutlined />}
              color="#fa8c16"
            />
          </Col>
        </Row>

        {/* Recommendations */}
        {recommendations && recommendations.length > 0 && (
          <Card 
            title="Test Recommendations" 
            style={{ marginBottom: '24px' }}
            extra={<InfoCircleOutlined />}
          >
            <Row gutter={16}>
              {recommendations.slice(0, 3).map((rec, index) => (
                <Col span={8} key={index}>
                  <Alert
                    message={rec.recommendationType.replace('_', ' ').toUpperCase()}
                    description={
                      <div>
                        <div>{rec.description}</div>
                        <div style={{ marginTop: '8px' }}>
                          <Text strong>Template: </Text>
                          <Text>{rec.templateKey}</Text>
                        </div>
                        <div>
                          <Text strong>Expected Improvement: </Text>
                          <Text>{rec.expectedImprovement}%</Text>
                        </div>
                      </div>
                    }
                    type="info"
                    showIcon
                    action={
                      <Button size="small" type="link">
                        Create Test
                      </Button>
                    }
                  />
                </Col>
              ))}
            </Row>
          </Card>
        )}

        {/* Tabs for different views */}
        <Tabs
          activeKey={activeTab}
          onChange={setActiveTab}
          items={[
            {
              key: 'active',
              label: 'Active Tests',
              children: (
                <Card>
                  <Table
                    columns={activeTestColumns}
                    dataSource={activeTests}
                    loading={isLoading}
                    rowKey="id"
                    pagination={{ pageSize: 10 }}
                    locale={{
                      emptyText: 'No active tests. Create your first A/B test to get started!'
                    }}
                  />
                </Card>
              )
            },
            {
              key: 'results',
              label: 'Test Results',
              children: (
                <ABTestResults
                  onViewDetails={(test) => {
                    setSelectedTest(test)
                    setIsAnalysisModalVisible(true)
                  }}
                />
              )
            },
            {
              key: 'recommendations',
              label: 'AI Recommendations',
              children: (
                <TestRecommendations
                  onCreateTest={(recommendation) => {
                    // Pre-fill the creation wizard with recommendation data
                    setIsCreateModalVisible(true)
                  }}
                />
              )
            }
          ]}
        />

        {/* Create Test Modal */}
        <TestCreationWizard
          visible={isCreateModalVisible}
          onCancel={() => setIsCreateModalVisible(false)}
          onSubmit={handleCreateTest}
        />

        {/* Analysis Modal */}
        <StatisticalAnalysisPanel
          visible={isAnalysisModalVisible}
          onCancel={() => setIsAnalysisModalVisible(false)}
          testData={selectedTest}
        />
      </div>
  )
}

export default ABTestingDashboard
