import React, { useState, useEffect } from 'react'
import {
  Card,
  Typography,
  Spin,
  Alert,
  Button,
  Space,
  Tag,
  Statistic,
  Row,
  Col,
  Tabs,
  Breadcrumb,
  Tooltip,
  Dropdown
} from 'antd'
import {
  ArrowLeftOutlined,
  DownloadOutlined,
  ShareAltOutlined,
  ReloadOutlined,
  ClockCircleOutlined,
  DatabaseOutlined,
  CheckCircleOutlined,
  WarningOutlined,
  MoreOutlined
} from '@ant-design/icons'
import { PageLayout } from '@shared/components/core/Layout'
import { DataTable } from '@shared/components/core/DataTable'
import { Chart } from '@shared/components/core/Chart'
import { MonacoSQLEditor } from '@shared/components/core/MonacoSQLEditor'
import { useParams, useNavigate } from 'react-router-dom'
import { useGetQueryResultsQuery, useGetQueryDetailsQuery } from '@shared/store/api/queryApi'

const { Text, Title } = Typography
const { TabPane } = Tabs

export default function QueryResults() {
  const { queryId } = useParams<{ queryId: string }>()
  const navigate = useNavigate()
  const [activeTab, setActiveTab] = useState('table')

  const {
    data: queryDetails,
    isLoading: detailsLoading,
    error: detailsError
  } = useGetQueryDetailsQuery(queryId || '', { skip: !queryId })

  const {
    data: results,
    isLoading: resultsLoading,
    error: resultsError,
    refetch: refetchResults
  } = useGetQueryResultsQuery(queryId || '', { skip: !queryId })

  // Mock data for demonstration
  const mockQueryDetails = {
    id: queryId || '1',
    question: 'Show me the top 10 customers by total order value in the last quarter',
    sql: `SELECT
  c.customer_id,
  c.customer_name,
  c.email,
  SUM(o.total_amount) as total_spent,
  COUNT(o.order_id) as order_count,
  AVG(o.total_amount) as avg_order_value
FROM customers c
JOIN orders o ON c.customer_id = o.customer_id
WHERE o.order_date >= DATEADD(quarter, -1, GETDATE())
GROUP BY c.customer_id, c.customer_name, c.email
ORDER BY total_spent DESC
LIMIT 10`,
    executedAt: '2024-01-15T14:30:00Z',
    executionTime: 245,
    status: 'completed',
    rowCount: 10,
    columnCount: 6,
    complexity: 'Medium',
    cost: 0.0045,
    user: 'john.doe@company.com'
  }

  const mockResults = [
    {
      customer_id: 'CUST001',
      customer_name: 'Acme Corporation',
      email: 'contact@acme.com',
      total_spent: 125000.50,
      order_count: 45,
      avg_order_value: 2777.79
    },
    {
      customer_id: 'CUST002',
      customer_name: 'Global Industries',
      email: 'orders@global.com',
      total_spent: 98750.25,
      order_count: 32,
      avg_order_value: 3085.95
    },
    {
      customer_id: 'CUST003',
      customer_name: 'Tech Solutions Ltd',
      email: 'billing@techsol.com',
      total_spent: 87500.00,
      order_count: 28,
      avg_order_value: 3125.00
    },
    {
      customer_id: 'CUST004',
      customer_name: 'Innovation Partners',
      email: 'finance@innovation.com',
      total_spent: 76250.75,
      order_count: 35,
      avg_order_value: 2178.59
    },
    {
      customer_id: 'CUST005',
      customer_name: 'Enterprise Systems',
      email: 'procurement@enterprise.com',
      total_spent: 65000.00,
      order_count: 20,
      avg_order_value: 3250.00
    }
  ]

  const displayDetails = queryDetails || mockQueryDetails
  const displayResults = results || mockResults

  const columns = displayResults.length > 0 ? Object.keys(displayResults[0]).map(key => ({
    key,
    title: key.replace(/_/g, ' ').replace(/\b\w/g, l => l.toUpperCase()),
    dataIndex: key,
    type: typeof displayResults[0][key] === 'number' ? 'number' : 'text',
    sortable: true,
    filterable: true
  })) : []

  const handleExport = (format: string) => {
    // Export functionality
    console.log(`Exporting as ${format}`)
  }

  const handleShare = () => {
    // Share functionality
    console.log('Sharing query results')
  }

  const getStatusColor = (status: string) => {
    switch (status) {
      case 'completed': return 'green'
      case 'running': return 'blue'
      case 'failed': return 'red'
      default: return 'default'
    }
  }

  const getComplexityColor = (complexity: string) => {
    switch (complexity) {
      case 'Simple': return 'green'
      case 'Medium': return 'orange'
      case 'Complex': return 'red'
      default: return 'default'
    }
  }

  if (!queryId) {
    return (
      <PageLayout
        title="Query Results"
        subtitle="No query selected"
      >
        <Alert
          message="No Query Selected"
          description="Please select a query from the history to view its results."
          type="info"
          showIcon
          action={
            <Button onClick={() => navigate('/chat/history')}>
              View Query History
            </Button>
          }
        />
      </PageLayout>
    )
  }

  if (detailsError || resultsError) {
    return (
      <PageLayout
        title="Query Results"
        subtitle="Error loading query"
      >
        <Alert
          message="Error Loading Query"
          description="There was an error loading the query details or results."
          type="error"
          showIcon
          action={
            <Button onClick={() => refetchResults()}>
              Retry
            </Button>
          }
        />
      </PageLayout>
    )
  }

  return (
    <PageLayout
      title="Query Results"
      subtitle={displayDetails.question}
      breadcrumb={
        <Breadcrumb>
          <Breadcrumb.Item>
            <Button
              type="link"
              icon={<ArrowLeftOutlined />}
              onClick={() => navigate('/chat')}
            >
              Back to Chat
            </Button>
          </Breadcrumb.Item>
          <Breadcrumb.Item>Query Results</Breadcrumb.Item>
          <Breadcrumb.Item>{queryId}</Breadcrumb.Item>
        </Breadcrumb>
      }
    >
      {/* Query Information */}
      <Card style={{ marginBottom: 24 }}>
        <Row gutter={[16, 16]}>
          <Col xs={24} sm={12} md={6}>
            <Statistic
              title="Execution Time"
              value={displayDetails.executionTime}
              suffix="ms"
              prefix={<ClockCircleOutlined />}
              valueStyle={{
                color: displayDetails.executionTime < 200 ? '#52c41a' :
                       displayDetails.executionTime < 500 ? '#faad14' : '#ff4d4f'
              }}
            />
          </Col>
          <Col xs={24} sm={12} md={6}>
            <Statistic
              title="Rows Returned"
              value={displayDetails.rowCount}
              prefix={<DatabaseOutlined />}
              valueStyle={{ color: '#1890ff' }}
            />
          </Col>
          <Col xs={24} sm={12} md={6}>
            <div>
              <Text type="secondary">Status</Text>
              <div style={{ marginTop: 4 }}>
                <Tag
                  color={getStatusColor(displayDetails.status)}
                  icon={displayDetails.status === 'completed' ? <CheckCircleOutlined /> : <WarningOutlined />}
                >
                  {displayDetails.status.toUpperCase()}
                </Tag>
              </div>
            </div>
          </Col>
          <Col xs={24} sm={12} md={6}>
            <div>
              <Text type="secondary">Complexity</Text>
              <div style={{ marginTop: 4 }}>
                <Tag color={getComplexityColor(displayDetails.complexity)}>
                  {displayDetails.complexity}
                </Tag>
              </div>
            </div>
          </Col>
        </Row>

        <div style={{
          display: 'flex',
          justifyContent: 'space-between',
          alignItems: 'center',
          marginTop: 16,
          paddingTop: 16,
          borderTop: '1px solid #f0f0f0'
        }}>
          <Space>
            <Text type="secondary">
              Executed by {displayDetails.user} on {new Date(displayDetails.executedAt).toLocaleString()}
            </Text>
            {displayDetails.cost && (
              <Text type="secondary">
                • Cost: ${displayDetails.cost.toFixed(4)}
              </Text>
            )}
          </Space>

          <Space>
            <Button
              icon={<ReloadOutlined />}
              onClick={() => refetchResults()}
              loading={resultsLoading}
            >
              Refresh
            </Button>

            <Dropdown
              menu={{
                items: [
                  {
                    key: 'csv',
                    label: 'Export as CSV',
                    icon: <DownloadOutlined />,
                    onClick: () => handleExport('csv'),
                  },
                  {
                    key: 'excel',
                    label: 'Export as Excel',
                    icon: <DownloadOutlined />,
                    onClick: () => handleExport('excel'),
                  },
                  {
                    key: 'json',
                    label: 'Export as JSON',
                    icon: <DownloadOutlined />,
                    onClick: () => handleExport('json'),
                  },
                ],
              }}
              trigger={['click']}
            >
              <Button icon={<DownloadOutlined />}>
                Export
              </Button>
            </Dropdown>

            <Button
              icon={<ShareAltOutlined />}
              onClick={handleShare}
            >
              Share
            </Button>
          </Space>
        </div>
      </Card>

      {/* Results Display */}
      <Card>
        <Tabs activeKey={activeTab} onChange={setActiveTab}>
          <TabPane
            tab={
              <Space>
                <DatabaseOutlined />
                Table View
              </Space>
            }
            key="table"
          >
            {resultsLoading ? (
              <div style={{ textAlign: 'center', padding: '40px' }}>
                <Spin size="large" />
                <div style={{ marginTop: 16 }}>
                  <Text>Loading query results...</Text>
                </div>
              </div>
            ) : (
              <DataTable
                data={displayResults}
                columns={columns}
                loading={resultsLoading}
                pagination={true}
                pageSize={50}
                searchable={true}
                filterable={true}
                exportable={false} // Handled by parent
                height={500}
              />
            )}
          </TabPane>

          <TabPane
            tab={
              <Space>
                <CheckCircleOutlined />
                Chart View
              </Space>
            }
            key="chart"
          >
            {displayResults.length > 0 && columns.length > 1 && (
              <Chart
                data={displayResults}
                columns={columns.map(col => col.key)}
                config={{
                  type: 'bar',
                  title: 'Query Results Visualization',
                  xAxis: columns[0]?.key || 'x',
                  yAxis: columns.find(col => col.type === 'number')?.key || columns[1]?.key || 'y',
                  colorScheme: 'default',
                  showGrid: true,
                  showLegend: true,
                  showAnimation: true,
                  interactive: true
                }}
                height={400}
              />
            )}
          </TabPane>

          <TabPane
            tab={
              <Space>
                <MoreOutlined />
                SQL Query
              </Space>
            }
            key="sql"
          >
            <div style={{ marginBottom: 16 }}>
              <Title level={5}>Executed SQL Query</Title>
              <Text type="secondary">
                This is the SQL query that was executed to generate the results above.
              </Text>
            </div>

            <MonacoSQLEditor
              value={displayDetails.sql}
              readOnly={true}
              height={300}
              options={{
                minimap: { enabled: false },
                scrollBeyondLastLine: false,
                readOnly: true,
                fontSize: 14,
                lineNumbers: 'on',
                wordWrap: 'on'
              }}
            />

            <div style={{
              marginTop: 16,
              padding: '12px',
              background: '#fafafa',
              borderRadius: '6px'
            }}>
              <Row gutter={16}>
                <Col span={8}>
                  <Text strong>Query Complexity:</Text>
                  <div>
                    <Tag color={getComplexityColor(displayDetails.complexity)}>
                      {displayDetails.complexity}
                    </Tag>
                  </div>
                </Col>
                <Col span={8}>
                  <Text strong>Execution Time:</Text>
                  <div>{displayDetails.executionTime}ms</div>
                </Col>
                <Col span={8}>
                  <Text strong>Data Processed:</Text>
                  <div>{displayDetails.rowCount} rows</div>
                </Col>
              </Row>
            </div>
          </TabPane>
        </Tabs>
      </Card>
    </PageLayout>
  )
}
