import React, { useState } from 'react'
import {
  Card,
  Button,
  Space,
  Typography,
  Alert,
  Collapse,
  Tag,
  Progress,
  Row,
  Col,
  Statistic,
  List,
  Spin,
  message,
} from 'antd'
import {
  PlayCircleOutlined,
  CheckCircleOutlined,
  CloseCircleOutlined,
  ExclamationCircleOutlined,
  ReloadOutlined,
  ApiOutlined,
  DatabaseOutlined,
} from '@ant-design/icons'
import {
  useGetEnhancedBusinessTablesQuery,
  useGetEnhancedBusinessMetadataStatisticsQuery,
  useSearchEnhancedBusinessTablesMutation,
  useValidateEnhancedBusinessTableMutation,
} from '@shared/store/api/businessApi'

const { Title, Text } = Typography


interface TestResult {
  name: string
  status: 'pending' | 'running' | 'success' | 'error'
  message: string
  duration?: number
  details?: any
}

export const EnhancedAPITester: React.FC = () => {
  const [testResults, setTestResults] = useState<TestResult[]>([])
  const [isRunning, setIsRunning] = useState(false)
  const [currentTest, setCurrentTest] = useState<string>('')

  // API hooks for testing
  const { refetch: refetchTables } = useGetEnhancedBusinessTablesQuery({ page: 1, pageSize: 5 }, { skip: true })
  const { refetch: refetchStatistics } = useGetEnhancedBusinessMetadataStatisticsQuery(undefined, { skip: true })
  const [searchTables] = useSearchEnhancedBusinessTablesMutation()
  const [validateTable] = useValidateEnhancedBusinessTableMutation()

  const updateTestResult = (name: string, status: TestResult['status'], message: string, details?: any, duration?: number) => {
    setTestResults(prev => {
      const existing = prev.find(r => r.name === name)
      if (existing) {
        return prev.map(r => r.name === name ? { ...r, status, message, details, duration } : r)
      } else {
        return [...prev, { name, status, message, details, duration }]
      }
    })
  }

  const runTest = async (testName: string, testFn: () => Promise<any>) => {
    setCurrentTest(testName)
    updateTestResult(testName, 'running', 'Running test...')
    
    const startTime = Date.now()
    try {
      const result = await testFn()
      const duration = Date.now() - startTime
      updateTestResult(testName, 'success', 'Test passed', result, duration)
      return result
    } catch (error: any) {
      const duration = Date.now() - startTime
      updateTestResult(testName, 'error', error.message || 'Test failed', error, duration)
      throw error
    }
  }

  const runAllTests = async () => {
    setIsRunning(true)
    setTestResults([])
    
    try {
      // Test 1: Basic API Connection
      await runTest('API Connection', async () => {
        const response = await fetch('/api/business-metadata/statistics', {
          headers: {
            'Authorization': `Bearer ${localStorage.getItem('accessToken')}`,
          },
        })
        if (!response.ok) {
          throw new Error(`HTTP ${response.status}: ${response.statusText}`)
        }
        return await response.json()
      })

      // Test 2: Get Business Tables
      await runTest('Get Business Tables', async () => {
        const result = await refetchTables()
        if (result.error) {
          throw new Error('Failed to fetch business tables')
        }
        return result.data
      })

      // Test 3: Get Statistics
      await runTest('Get Statistics', async () => {
        const result = await refetchStatistics()
        if (result.error) {
          throw new Error('Failed to fetch statistics')
        }
        return result.data
      })

      // Test 4: Advanced Search
      await runTest('Advanced Search', async () => {
        const result = await searchTables({
          searchQuery: 'customer data',
          schemas: [],
          domains: [],
          tags: [],
          includeColumns: true,
          includeGlossaryTerms: true,
          maxResults: 10,
          minRelevanceScore: 0.1,
        }).unwrap()
        return result
      })

      // Test 5: Validation
      await runTest('Table Validation', async () => {
        const result = await validateTable({
          tableId: 1,
          validateBusinessRules: true,
          validateDataQuality: true,
          validateRelationships: false,
        }).unwrap()
        return result
      })

      message.success('All tests completed successfully!')
    } catch (error) {
      message.error('Some tests failed. Check the results below.')
    } finally {
      setIsRunning(false)
      setCurrentTest('')
    }
  }

  const getStatusIcon = (status: TestResult['status']) => {
    switch (status) {
      case 'running':
        return <Spin size="small" />
      case 'success':
        return <CheckCircleOutlined style={{ color: '#52c41a' }} />
      case 'error':
        return <CloseCircleOutlined style={{ color: '#ff4d4f' }} />
      default:
        return <ExclamationCircleOutlined style={{ color: '#d9d9d9' }} />
    }
  }

  const getStatusColor = (status: TestResult['status']) => {
    switch (status) {
      case 'running':
        return 'processing'
      case 'success':
        return 'success'
      case 'error':
        return 'error'
      default:
        return 'default'
    }
  }

  const successCount = testResults.filter(r => r.status === 'success').length
  const errorCount = testResults.filter(r => r.status === 'error').length
  const totalTests = testResults.length

  return (
    <div>
      <Card
        title={
          <Space>
            <ApiOutlined />
            <span>Enhanced API Integration Tester</span>
          </Space>
        }
        extra={
          <Button
            type="primary"
            icon={<PlayCircleOutlined />}
            onClick={runAllTests}
            loading={isRunning}
          >
            Run All Tests
          </Button>
        }
      >
        <Alert
          message="API Integration Testing"
          description="This tool tests the integration with the enhanced Business Metadata API endpoints. Make sure the backend is running on localhost:55244."
          type="info"
          showIcon
          style={{ marginBottom: 16 }}
        />

        {/* Test Progress */}
        {isRunning && (
          <Card size="small" style={{ marginBottom: 16 }}>
            <Space direction="vertical" style={{ width: '100%' }}>
              <Text>Running: {currentTest}</Text>
              <Progress
                percent={(successCount + errorCount) / Math.max(totalTests, 5) * 100}
                status={errorCount > 0 ? 'exception' : 'active'}
              />
            </Space>
          </Card>
        )}

        {/* Test Results Summary */}
        {testResults.length > 0 && (
          <Row gutter={16} style={{ marginBottom: 16 }}>
            <Col span={8}>
              <Card size="small">
                <Statistic
                  title="Total Tests"
                  value={totalTests}
                  prefix={<DatabaseOutlined />}
                />
              </Card>
            </Col>
            <Col span={8}>
              <Card size="small">
                <Statistic
                  title="Passed"
                  value={successCount}
                  valueStyle={{ color: '#3f8600' }}
                  prefix={<CheckCircleOutlined />}
                />
              </Card>
            </Col>
            <Col span={8}>
              <Card size="small">
                <Statistic
                  title="Failed"
                  value={errorCount}
                  valueStyle={{ color: '#cf1322' }}
                  prefix={<CloseCircleOutlined />}
                />
              </Card>
            </Col>
          </Row>
        )}

        {/* Test Results */}
        {testResults.length > 0 && (
          <Collapse
            items={testResults.map((result, index) => ({
              key: index.toString(),
              label: (
                <Space>
                  {getStatusIcon(result.status)}
                  <span>{result.name}</span>
                  <Tag color={getStatusColor(result.status)}>
                    {result.status}
                  </Tag>
                  {result.duration && (
                    <Text type="secondary">({result.duration}ms)</Text>
                  )}
                </Space>
              ),
              children: (
                <Space direction="vertical" style={{ width: '100%' }}>
                  <Text>{result.message}</Text>

                  {result.details && (
                    <Card size="small" title="Details">
                      <pre style={{
                        background: '#f5f5f5',
                        padding: '8px',
                        borderRadius: '4px',
                        fontSize: '12px',
                        overflow: 'auto',
                        maxHeight: '200px'
                      }}>
                        {JSON.stringify(result.details, null, 2)}
                      </pre>
                    </Card>
                  )}
                </Space>
              )
            }))}
          />
        )}

        {/* API Endpoints List */}
        <Card title="Enhanced API Endpoints" size="small" style={{ marginTop: 16 }}>
          <List
            size="small"
            dataSource={[
              { method: 'GET', endpoint: '/api/business-metadata/tables', description: 'Get paginated business tables' },
              { method: 'GET', endpoint: '/api/business-metadata/tables/{id}', description: 'Get specific business table' },
              { method: 'POST', endpoint: '/api/business-metadata/tables', description: 'Create new business table' },
              { method: 'PUT', endpoint: '/api/business-metadata/tables/{id}', description: 'Update business table' },
              { method: 'DELETE', endpoint: '/api/business-metadata/tables/{id}', description: 'Delete business table' },
              { method: 'POST', endpoint: '/api/business-metadata/tables/search', description: 'Advanced semantic search' },
              { method: 'POST', endpoint: '/api/business-metadata/tables/bulk', description: 'Bulk operations' },
              { method: 'POST', endpoint: '/api/business-metadata/tables/validate', description: 'Table validation' },
              { method: 'GET', endpoint: '/api/business-metadata/statistics', description: 'Metadata statistics' },
            ]}
            renderItem={(item) => (
              <List.Item>
                <Space>
                  <Tag color={item.method === 'GET' ? 'blue' : item.method === 'POST' ? 'green' : item.method === 'PUT' ? 'orange' : 'red'}>
                    {item.method}
                  </Tag>
                  <Text code>{item.endpoint}</Text>
                  <Text type="secondary">{item.description}</Text>
                </Space>
              </List.Item>
            )}
          />
        </Card>

        {/* Connection Info */}
        <Alert
          message="Backend Connection"
          description={
            <div>
              <p><strong>Expected Backend URL:</strong> http://localhost:55244</p>
              <p><strong>Authentication:</strong> JWT Bearer token required</p>
              <p><strong>Documentation:</strong> Available at http://localhost:55244/swagger</p>
            </div>
          }
          type="warning"
          showIcon
          style={{ marginTop: 16 }}
        />
      </Card>
    </div>
  )
}

export default EnhancedAPITester
