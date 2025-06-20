/**
 * Real API Tester Component
 * 
 * Tests connection to real backend API endpoints and displays results.
 * Helps verify that the backend is running and endpoints are accessible.
 */

import React, { useState } from 'react'
import { Card, Button, Space, Alert, Typography, Spin, Collapse, Tag, Divider } from 'antd'
import { 
  ApiOutlined, 
  CheckCircleOutlined, 
  ExclamationCircleOutlined,
  ReloadOutlined,
  DatabaseOutlined,
  TableOutlined
} from '@ant-design/icons'
import { 
  useGetBusinessTablesQuery,
  useGetBusinessMetadataStatusQuery,
  useGetAllSchemaTablesQuery,
  useGetSchemaSummaryQuery,
  usePopulateRelevantBusinessMetadataMutation
} from '@shared/store/api/businessApi'

const { Text, Title } = Typography
const { Panel } = Collapse

export const RealApiTester: React.FC = () => {
  const [testResults, setTestResults] = useState<Record<string, any>>({})
  const [isTestingAll, setIsTestingAll] = useState(false)

  // API Hooks
  const { 
    data: businessTables, 
    isLoading: tablesLoading, 
    error: tablesError,
    refetch: refetchTables 
  } = useGetBusinessTablesQuery()

  const { 
    data: metadataStatus, 
    isLoading: statusLoading, 
    error: statusError,
    refetch: refetchStatus 
  } = useGetBusinessMetadataStatusQuery()

  const { 
    data: schemaTables, 
    isLoading: schemaLoading, 
    error: schemaError,
    refetch: refetchSchema 
  } = useGetAllSchemaTablesQuery()

  const { 
    data: schemaSummary, 
    isLoading: summaryLoading, 
    error: summaryError,
    refetch: refetchSummary 
  } = useGetSchemaSummaryQuery()

  const [populateRelevant, { isLoading: populateLoading }] = usePopulateRelevantBusinessMetadataMutation()

  const testEndpoint = async (name: string, testFn: () => Promise<any>) => {
    try {
      setTestResults(prev => ({ ...prev, [name]: { status: 'testing' } }))
      const result = await testFn()
      setTestResults(prev => ({ 
        ...prev, 
        [name]: { 
          status: 'success', 
          data: result,
          timestamp: new Date().toISOString()
        } 
      }))
    } catch (error: any) {
      setTestResults(prev => ({ 
        ...prev, 
        [name]: { 
          status: 'error', 
          error: error.message || 'Unknown error',
          timestamp: new Date().toISOString()
        } 
      }))
    }
  }

  const testAllEndpoints = async () => {
    setIsTestingAll(true)
    setTestResults({})

    const tests = [
      { name: 'Business Tables', fn: () => refetchTables().unwrap() },
      { name: 'Metadata Status', fn: () => refetchStatus().unwrap() },
      { name: 'Schema Tables', fn: () => refetchSchema().unwrap() },
      { name: 'Schema Summary', fn: () => refetchSummary().unwrap() },
    ]

    for (const test of tests) {
      await testEndpoint(test.name, test.fn)
      // Small delay between tests
      await new Promise(resolve => setTimeout(resolve, 500))
    }

    setIsTestingAll(false)
  }

  const handlePopulateRelevant = async () => {
    try {
      const result = await populateRelevant({ useAI: true, overwriteExisting: false }).unwrap()
      setTestResults(prev => ({ 
        ...prev, 
        'Populate Relevant': { 
          status: 'success', 
          data: result,
          timestamp: new Date().toISOString()
        } 
      }))
    } catch (error: any) {
      setTestResults(prev => ({ 
        ...prev, 
        'Populate Relevant': { 
          status: 'error', 
          error: error.message || 'Unknown error',
          timestamp: new Date().toISOString()
        } 
      }))
    }
  }

  const getStatusIcon = (status: string) => {
    switch (status) {
      case 'success': return <CheckCircleOutlined style={{ color: '#52c41a' }} />
      case 'error': return <ExclamationCircleOutlined style={{ color: '#f5222d' }} />
      case 'testing': return <Spin size="small" />
      default: return <ApiOutlined />
    }
  }

  const getStatusColor = (status: string) => {
    switch (status) {
      case 'success': return 'green'
      case 'error': return 'red'
      case 'testing': return 'blue'
      default: return 'default'
    }
  }

  return (
    <Card title="Real API Connection Tester" size="small">
      <Space direction="vertical" style={{ width: '100%' }}>
        {/* Test Controls */}
        <Space>
          <Button 
            type="primary" 
            icon={<ApiOutlined />}
            onClick={testAllEndpoints}
            loading={isTestingAll}
          >
            Test All Endpoints
          </Button>
          <Button 
            icon={<DatabaseOutlined />}
            onClick={handlePopulateRelevant}
            loading={populateLoading}
          >
            Populate Relevant Tables
          </Button>
          <Button 
            icon={<ReloadOutlined />}
            onClick={() => {
              refetchTables()
              refetchStatus()
              refetchSchema()
              refetchSummary()
            }}
          >
            Refresh All
          </Button>
        </Space>

        {/* Quick Status Overview */}
        <Card size="small" title="Quick Status">
          <Space wrap>
            <Tag color={tablesError ? 'red' : businessTables ? 'green' : 'blue'}>
              <TableOutlined /> Business Tables: {businessTables?.length || 0}
            </Tag>
            <Tag color={statusError ? 'red' : metadataStatus ? 'green' : 'blue'}>
              <DatabaseOutlined /> Metadata Status: {statusError ? 'Error' : 'OK'}
            </Tag>
            <Tag color={schemaError ? 'red' : schemaTables ? 'green' : 'blue'}>
              <TableOutlined /> Schema Tables: {schemaTables?.length || 0}
            </Tag>
            <Tag color={summaryError ? 'red' : schemaSummary ? 'green' : 'blue'}>
              <DatabaseOutlined /> DB: {schemaSummary?.databaseName || 'Unknown'}
            </Tag>
          </Space>
        </Card>

        {/* Test Results */}
        {Object.keys(testResults).length > 0 && (
          <Collapse>
            <Panel header="Test Results" key="results">
              <Space direction="vertical" style={{ width: '100%' }}>
                {Object.entries(testResults).map(([name, result]: [string, any]) => (
                  <Card key={name} size="small">
                    <Space>
                      {getStatusIcon(result.status)}
                      <Text strong>{name}</Text>
                      <Tag color={getStatusColor(result.status)}>
                        {result.status.toUpperCase()}
                      </Tag>
                      {result.timestamp && (
                        <Text type="secondary" style={{ fontSize: '12px' }}>
                          {new Date(result.timestamp).toLocaleTimeString()}
                        </Text>
                      )}
                    </Space>
                    
                    {result.error && (
                      <Alert 
                        message="Error" 
                        description={result.error} 
                        type="error" 
                        size="small"
                        style={{ marginTop: 8 }}
                      />
                    )}
                    
                    {result.data && (
                      <div style={{ marginTop: 8 }}>
                        <Text type="secondary">Response:</Text>
                        <pre style={{ 
                          fontSize: '11px', 
                          background: '#f5f5f5', 
                          padding: '8px', 
                          borderRadius: '4px',
                          maxHeight: '200px',
                          overflow: 'auto'
                        }}>
                          {JSON.stringify(result.data, null, 2)}
                        </pre>
                      </div>
                    )}
                  </Card>
                ))}
              </Space>
            </Panel>
          </Collapse>
        )}

        {/* API Errors */}
        {(tablesError || statusError || schemaError || summaryError) && (
          <Alert
            message="API Connection Issues"
            description={
              <div>
                <p>Some API endpoints are not responding. This could mean:</p>
                <ul>
                  <li>Backend server is not running on port 55244</li>
                  <li>Database connection issues</li>
                  <li>Authentication problems</li>
                  <li>CORS configuration issues</li>
                </ul>
                <p>Check the browser console and network tab for more details.</p>
              </div>
            }
            type="warning"
            showIcon
          />
        )}
      </Space>
    </Card>
  )
}
