import React, { useState } from 'react'
import {
  Card,
  Row,
  Col,
  Statistic,
  Typography,
  Progress,
  Button,
  Space,
  Tabs,
  Table,
  Tag,
  Modal,
  Form,
  Input,
  Select,
  Switch,
  Alert,
  Tooltip,
  Badge,
  Divider
} from 'antd'
import {
  DatabaseOutlined,
  SettingOutlined,
  BulbOutlined,
  FileTextOutlined,
  PlayCircleOutlined,
  EditOutlined,
  DeleteOutlined,
  PlusOutlined,
  ExportOutlined,
  ImportOutlined,
  EyeOutlined,
  BarChartOutlined,
  RobotOutlined,
  ThunderboltOutlined
} from '@ant-design/icons'
import {
  useGetTuningDashboardQuery,
  useGetTuningTablesQuery,
  useGetQueryPatternsQuery,
  useGetPromptTemplatesQuery,
  useUpdateTuningTableMutation,
  useCreateQueryPatternMutation,
  useUpdatePromptTemplateMutation
} from '@shared/store/api/tuningApi'
import { useGetTransparencyDashboardMetricsQuery } from '@shared/store/api/transparencyApi'
import { useGetAgentAnalyticsQuery } from '@shared/store/api/intelligentAgentsApi'
import { AIFeatureWrapper } from '@shared/components/ai/common/AIFeatureWrapper'
import { ConfidenceIndicator } from '@shared/components/ai/common/ConfidenceIndicator'
import { AITransparencyPanel } from '@shared/components/ai/transparency/AITransparencyPanel'
import { useResponsive } from '@shared/hooks/useResponsive'

const { Title, Text } = Typography
const { TabPane } = Tabs
const { TextArea } = Input
const { Option } = Select

export const TuningDashboard: React.FC = () => {
  const responsive = useResponsive()
  const [activeTab, setActiveTab] = useState('overview')
  const [selectedTable, setSelectedTable] = useState<any>(null)
  const [showTableModal, setShowTableModal] = useState(false)
  const [showPatternModal, setShowPatternModal] = useState(false)
  const [showTransparencyPanel, setShowTransparencyPanel] = useState(false)
  const [selectedTraceId, setSelectedTraceId] = useState<string | null>(null)
  const [form] = Form.useForm()

  const {
    data: dashboardData,
    isLoading: dashboardLoading
  } = useGetTuningDashboardQuery()

  const {
    data: tuningTables,
    isLoading: tablesLoading
  } = useGetTuningTablesQuery({})

  const {
    data: queryPatterns,
    isLoading: patternsLoading
  } = useGetQueryPatternsQuery({})

  const {
    data: promptTemplates,
    isLoading: templatesLoading
  } = useGetPromptTemplatesQuery({})

  // AI Analytics data
  const {
    data: transparencyMetrics,
    isLoading: transparencyLoading
  } = useGetTransparencyDashboardMetricsQuery({ days: 30 })

  const {
    data: agentAnalytics,
    isLoading: agentLoading
  } = useGetAgentAnalyticsQuery({ days: 30 })

  const [updateTable] = useUpdateTuningTableMutation()
  const [createPattern] = useCreateQueryPatternMutation()
  const [updateTemplate] = useUpdatePromptTemplateMutation()

  const handleEditTable = (table: any) => {
    setSelectedTable(table)
    form.setFieldsValue(table)
    setShowTableModal(true)
  }

  const handleSaveTable = async (values: any) => {
    try {
      await updateTable({
        id: selectedTable.id,
        data: values
      }).unwrap()
      
      setShowTableModal(false)
      setSelectedTable(null)
      form.resetFields()
    } catch (error) {
      console.error('Failed to update table:', error)
    }
  }

  const handleCreatePattern = async (values: any) => {
    try {
      await createPattern(values).unwrap()
      setShowPatternModal(false)
      form.resetFields()
    } catch (error) {
      console.error('Failed to create pattern:', error)
    }
  }

  const getStatusColor = (status: string) => {
    switch (status) {
      case 'Complete': return 'green'
      case 'In Progress': return 'blue'
      case 'Needs Review': return 'orange'
      default: return 'default'
    }
  }

  const getPriorityColor = (priority: string) => {
    switch (priority) {
      case 'High': return 'red'
      case 'Medium': return 'orange'
      case 'Low': return 'green'
      default: return 'default'
    }
  }

  const tableColumns = [
    {
      title: 'Table Name',
      dataIndex: 'tableName',
      key: 'tableName',
      render: (text: string, record: any) => (
        <div>
          <Text strong>{record.schemaName}.{text}</Text>
          {record.businessName && (
            <div>
              <Text type="secondary" style={{ fontSize: '12px' }}>
                {record.businessName}
              </Text>
            </div>
          )}
        </div>
      ),
    },
    {
      title: 'Status',
      dataIndex: 'tuningStatus',
      key: 'tuningStatus',
      render: (status: string) => (
        <Tag color={getStatusColor(status)}>{status}</Tag>
      ),
    },
    {
      title: 'Priority',
      dataIndex: 'priority',
      key: 'priority',
      render: (priority: string) => (
        <Tag color={getPriorityColor(priority)}>{priority}</Tag>
      ),
    },
    {
      title: 'Domain',
      dataIndex: 'domainClassification',
      key: 'domainClassification',
      render: (domain: string) => domain && <Tag>{domain}</Tag>,
    },
    {
      title: 'Last Tuned',
      dataIndex: 'lastTuned',
      key: 'lastTuned',
      render: (date: string) => date ? new Date(date).toLocaleDateString() : 'Never',
    },
    {
      title: 'Actions',
      key: 'actions',
      render: (_, record: any) => (
        <Space>
          <Tooltip title="Edit Table">
            <Button
              type="text"
              size="small"
              icon={<EditOutlined />}
              onClick={() => handleEditTable(record)}
            />
          </Tooltip>
        </Space>
      ),
    },
  ]

  const patternColumns = [
    {
      title: 'Name',
      dataIndex: 'name',
      key: 'name',
      render: (text: string, record: any) => (
        <div>
          <Text strong>{text}</Text>
          <div>
            <Text type="secondary" style={{ fontSize: '12px' }}>
              {record.description}
            </Text>
          </div>
        </div>
      ),
    },
    {
      title: 'Category',
      dataIndex: 'category',
      key: 'category',
      render: (category: string) => <Tag color="blue">{category}</Tag>,
    },
    {
      title: 'Difficulty',
      dataIndex: 'difficulty',
      key: 'difficulty',
      render: (difficulty: string) => (
        <Tag color={
          difficulty === 'Beginner' ? 'green' :
          difficulty === 'Intermediate' ? 'orange' : 'red'
        }>
          {difficulty}
        </Tag>
      ),
    },
    {
      title: 'Usage',
      dataIndex: 'usageCount',
      key: 'usageCount',
      render: (count: number) => count.toLocaleString(),
    },
    {
      title: 'Success Rate',
      dataIndex: 'successRate',
      key: 'successRate',
      render: (rate: number) => (
        <Progress
          percent={rate * 100}
          size="small"
          strokeColor={rate > 0.8 ? '#52c41a' : rate > 0.6 ? '#faad14' : '#ff4d4f'}
        />
      ),
    },
    {
      title: 'Status',
      dataIndex: 'isActive',
      key: 'isActive',
      render: (isActive: boolean) => (
        <Badge status={isActive ? 'success' : 'default'} text={isActive ? 'Active' : 'Inactive'} />
      ),
    },
  ]

  if (dashboardLoading) {
    return (
      <div style={{ padding: '24px' }}>
        <Card loading style={{ minHeight: '400px' }} />
      </div>
    )
  }

  return (
    <div style={{ padding: responsive.isMobile ? '16px' : '24px' }}>
      {/* Header */}
      <div style={{
        display: 'flex',
        justifyContent: 'space-between',
        alignItems: 'center',
        marginBottom: '24px'
      }}>
        <div>
          <Title level={2} style={{ margin: 0 }}>
            AI Tuning Dashboard
          </Title>
          <Text type="secondary">
            Optimize AI performance through business context tuning
          </Text>
        </div>
        
        <Space>
          <Button icon={<ImportOutlined />}>
            Import
          </Button>
          <Button icon={<ExportOutlined />}>
            Export
          </Button>
          <Button
            type="primary"
            icon={<PlusOutlined />}
            onClick={() => setShowPatternModal(true)}
          >
            Add Pattern
          </Button>
        </Space>
      </div>

      <Tabs activeKey={activeTab} onChange={setActiveTab}>
        {/* Overview Tab */}
        <TabPane tab="Overview" key="overview">
          {/* Key Metrics */}
          <Row gutter={[16, 16]} style={{ marginBottom: '24px' }}>
            <Col xs={24} sm={12} md={6}>
              <Card>
                <Statistic
                  title="Total Tables"
                  value={dashboardData?.totalTables || 0}
                  prefix={<DatabaseOutlined />}
                />
              </Card>
            </Col>

            <Col xs={24} sm={12} md={6}>
              <Card>
                <Statistic
                  title="Tuned Tables"
                  value={dashboardData?.tunedTables || 0}
                  prefix={<SettingOutlined />}
                  valueStyle={{ color: '#52c41a' }}
                />
              </Card>
            </Col>

            <Col xs={24} sm={12} md={6}>
              <Card>
                <Statistic
                  title="Query Patterns"
                  value={dashboardData?.totalPatterns || 0}
                  prefix={<BulbOutlined />}
                  valueStyle={{ color: '#1890ff' }}
                />
              </Card>
            </Col>

            <Col xs={24} sm={12} md={6}>
              <Card>
                <Statistic
                  title="Active Prompts"
                  value={dashboardData?.activePrompts || 0}
                  prefix={<FileTextOutlined />}
                  valueStyle={{ color: '#722ed1' }}
                />
              </Card>
            </Col>
          </Row>

          {/* AI Analytics Section */}
          <AIFeatureWrapper feature="transparencyPanelEnabled">
            <Row gutter={[16, 16]} style={{ marginBottom: '24px' }}>
              <Col span={24}>
                <Card
                  title={
                    <Space>
                      <RobotOutlined />
                      <span>AI Performance Analytics</span>
                      <Button
                        type="text"
                        size="small"
                        icon={<EyeOutlined />}
                        onClick={() => setShowTransparencyPanel(!showTransparencyPanel)}
                      >
                        {showTransparencyPanel ? 'Hide' : 'Show'} Transparency
                      </Button>
                    </Space>
                  }
                >
                  <Row gutter={[16, 16]}>
                    <Col xs={24} sm={12} lg={6}>
                      <div style={{ textAlign: 'center' }}>
                        <ConfidenceIndicator
                          confidence={transparencyMetrics?.averageConfidence || 0.87}
                          size="large"
                          type="circle"
                          showLabel={true}
                        />
                        <Text type="secondary" style={{ display: 'block', marginTop: 8 }}>
                          Avg AI Confidence
                        </Text>
                      </div>
                    </Col>
                    <Col xs={24} sm={12} lg={6}>
                      <Statistic
                        title="AI Queries"
                        value={transparencyMetrics?.totalTraces || 1247}
                        prefix={<ThunderboltOutlined />}
                        valueStyle={{ color: '#1890ff' }}
                      />
                    </Col>
                    <Col xs={24} sm={12} lg={6}>
                      <Statistic
                        title="Active Agents"
                        value={agentAnalytics?.agentUtilization?.length || 8}
                        prefix={<RobotOutlined />}
                        valueStyle={{ color: '#52c41a' }}
                      />
                    </Col>
                    <Col xs={24} sm={12} lg={6}>
                      <Statistic
                        title="Success Rate"
                        value={agentAnalytics?.successRate ? (agentAnalytics.successRate * 100).toFixed(1) : '94.5'}
                        suffix="%"
                        prefix={<BarChartOutlined />}
                        valueStyle={{ color: '#722ed1' }}
                      />
                    </Col>
                  </Row>
                </Card>
              </Col>
            </Row>
          </AIFeatureWrapper>

          {/* Tuning Coverage */}
          <Row gutter={[16, 16]}>
            <Col xs={24} md={12}>
              <Card title="Tuning Coverage">
                <div style={{ textAlign: 'center' }}>
                  <Progress
                    type="circle"
                    percent={dashboardData?.tuningCoverage || 0}
                    strokeColor={{
                      '0%': '#ff4d4f',
                      '50%': '#faad14',
                      '100%': '#52c41a',
                    }}
                    size={120}
                  />
                  <div style={{ marginTop: '16px' }}>
                    <Text type="secondary">
                      {dashboardData?.tunedTables || 0} of {dashboardData?.totalTables || 0} tables tuned
                    </Text>
                  </div>
                </div>
              </Card>
            </Col>

            <Col xs={24} md={12}>
              <Card title="Performance Metrics">
                <Space direction="vertical" style={{ width: '100%' }}>
                  <div>
                    <Text>Query Accuracy</Text>
                    <Progress
                      percent={(dashboardData?.performanceMetrics?.avgQueryAccuracy || 0) * 100}
                      strokeColor="#52c41a"
                    />
                  </div>
                  <div>
                    <Text>Response Time</Text>
                    <Progress
                      percent={Math.max(0, 100 - (dashboardData?.performanceMetrics?.avgResponseTime || 0) / 10)}
                      strokeColor="#1890ff"
                    />
                  </div>
                  <div>
                    <Text>User Satisfaction</Text>
                    <Progress
                      percent={(dashboardData?.performanceMetrics?.userSatisfactionScore || 0) * 100}
                      strokeColor="#722ed1"
                    />
                  </div>
                </Space>
              </Card>
            </Col>
          </Row>

          {/* AI Transparency Panel */}
          {showTransparencyPanel && (
            <AIFeatureWrapper feature="transparencyPanelEnabled">
              <Row gutter={[16, 16]} style={{ marginTop: '24px' }}>
                <Col span={24}>
                  <Card
                    title={
                      <Space>
                        <EyeOutlined />
                        <span>AI Decision Transparency</span>
                      </Space>
                    }
                  >
                    <Alert
                      message="AI Transparency Integration"
                      description="This section shows AI decision-making transparency for tuning operations. Select a recent trace to explore AI reasoning."
                      type="info"
                      showIcon
                      style={{ marginBottom: 16 }}
                    />

                    {selectedTraceId ? (
                      <AITransparencyPanel
                        traceId={selectedTraceId}
                        showDetailedMetrics={true}
                        compact={false}
                      />
                    ) : (
                      <div style={{ textAlign: 'center', padding: '40px 0' }}>
                        <Space direction="vertical">
                          <RobotOutlined style={{ fontSize: '48px', color: '#d9d9d9' }} />
                          <Text type="secondary">
                            No transparency trace selected. Recent AI operations will appear here.
                          </Text>
                          <Button
                            type="primary"
                            onClick={() => setSelectedTraceId('demo-tuning-trace-001')}
                          >
                            View Demo Trace
                          </Button>
                        </Space>
                      </div>
                    )}
                  </Card>
                </Col>
              </Row>
            </AIFeatureWrapper>
          )}
        </TabPane>

        {/* Business Tables Tab */}
        <TabPane tab="Business Tables" key="tables">
          <Card
            title={
              <Space>
                <DatabaseOutlined />
                <span>Business Tables</span>
                <Badge count={tuningTables?.length || 0} />
              </Space>
            }
          >
            <Table
              columns={tableColumns}
              dataSource={tuningTables}
              loading={tablesLoading}
              rowKey="id"
              pagination={{
                pageSize: 20,
                showSizeChanger: true,
                showQuickJumper: true,
              }}
              scroll={{ x: 'max-content' }}
              size={responsive.isMobile ? 'small' : 'middle'}
            />
          </Card>
        </TabPane>

        {/* Query Patterns Tab */}
        <TabPane tab="Query Patterns" key="patterns">
          <Card
            title={
              <Space>
                <BulbOutlined />
                <span>Query Patterns</span>
                <Badge count={queryPatterns?.length || 0} />
              </Space>
            }
            extra={
              <Button
                type="primary"
                icon={<PlusOutlined />}
                onClick={() => setShowPatternModal(true)}
              >
                Add Pattern
              </Button>
            }
          >
            <Table
              columns={patternColumns}
              dataSource={queryPatterns}
              loading={patternsLoading}
              rowKey="id"
              pagination={{
                pageSize: 20,
                showSizeChanger: true,
                showQuickJumper: true,
              }}
              scroll={{ x: 'max-content' }}
              size={responsive.isMobile ? 'small' : 'middle'}
            />
          </Card>
        </TabPane>

        {/* AI Transparency Tab */}
        <TabPane
          tab={
            <AIFeatureWrapper feature="transparencyPanelEnabled" fallback={null}>
              <Space>
                <EyeOutlined />
                <span>AI Transparency</span>
              </Space>
            </AIFeatureWrapper>
          }
          key="transparency"
        >
          <AIFeatureWrapper feature="transparencyPanelEnabled">
            <Card
              title={
                <Space>
                  <EyeOutlined />
                  <span>AI Decision Transparency</span>
                </Space>
              }
              extra={
                <Space>
                  <Select
                    placeholder="Select trace"
                    style={{ width: 200 }}
                    value={selectedTraceId}
                    onChange={setSelectedTraceId}
                  >
                    <Select.Option value="demo-tuning-trace-001">Demo Tuning Trace</Select.Option>
                    <Select.Option value="demo-pattern-trace-002">Pattern Analysis Trace</Select.Option>
                    <Select.Option value="demo-optimization-trace-003">Optimization Trace</Select.Option>
                  </Select>
                  <Button
                    icon={<ReloadOutlined />}
                    onClick={() => setSelectedTraceId(null)}
                  >
                    Clear
                  </Button>
                </Space>
              }
            >
              {selectedTraceId ? (
                <AITransparencyPanel
                  traceId={selectedTraceId}
                  showDetailedMetrics={true}
                  compact={false}
                />
              ) : (
                <div style={{ textAlign: 'center', padding: '60px 0' }}>
                  <Space direction="vertical" size="large">
                    <RobotOutlined style={{ fontSize: '64px', color: '#d9d9d9' }} />
                    <div>
                      <Title level={4} type="secondary">AI Transparency Dashboard</Title>
                      <Text type="secondary">
                        Select a trace from the dropdown above to explore AI decision-making processes
                      </Text>
                    </div>
                    <Space>
                      <Button
                        type="primary"
                        onClick={() => setSelectedTraceId('demo-tuning-trace-001')}
                      >
                        View Demo Trace
                      </Button>
                      <Button onClick={() => setSelectedTraceId('demo-pattern-trace-002')}>
                        Pattern Analysis
                      </Button>
                    </Space>
                  </Space>
                </div>
              )}
            </Card>
          </AIFeatureWrapper>
        </TabPane>

        {/* Prompt Templates Tab */}
        <TabPane tab="Prompt Templates" key="prompts">
          <Card
            title={
              <Space>
                <FileTextOutlined />
                <span>Prompt Templates</span>
                <Badge count={promptTemplates?.length || 0} />
              </Space>
            }
          >
            <div style={{ display: 'grid', gap: '16px' }}>
              {promptTemplates?.map((template) => (
                <Card
                  key={template.id}
                  size="small"
                  title={
                    <Space>
                      <Text strong>{template.name}</Text>
                      <Tag color="blue">{template.category}</Tag>
                      <Badge status={template.isActive ? 'success' : 'default'} />
                    </Space>
                  }
                  extra={
                    <Space>
                      <Text type="secondary" style={{ fontSize: '12px' }}>
                        v{template.version}
                      </Text>
                      <Button type="text" size="small" icon={<EditOutlined />} />
                    </Space>
                  }
                >
                  <Text type="secondary">{template.description}</Text>
                  <div style={{ marginTop: '8px' }}>
                    <Space>
                      <Text style={{ fontSize: '12px' }}>
                        Usage: {template.usageStats.totalUsage}
                      </Text>
                      <Text style={{ fontSize: '12px' }}>
                        Success: {(template.usageStats.successRate * 100).toFixed(1)}%
                      </Text>
                      <Text style={{ fontSize: '12px' }}>
                        Avg Time: {template.usageStats.avgResponseTime}ms
                      </Text>
                    </Space>
                  </div>
                </Card>
              ))}
            </div>
          </Card>
        </TabPane>
      </Tabs>

      {/* Table Edit Modal */}
      <Modal
        title="Edit Business Table"
        open={showTableModal}
        onCancel={() => setShowTableModal(false)}
        onOk={() => form.submit()}
        width={800}
      >
        <Form
          form={form}
          layout="vertical"
          onFinish={handleSaveTable}
        >
          <Row gutter={16}>
            <Col span={12}>
              <Form.Item name="businessName" label="Business Name">
                <Input />
              </Form.Item>
            </Col>
            <Col span={12}>
              <Form.Item name="domainClassification" label="Domain">
                <Select>
                  <Option value="Sales">Sales</Option>
                  <Option value="Marketing">Marketing</Option>
                  <Option value="Finance">Finance</Option>
                  <Option value="Operations">Operations</Option>
                  <Option value="HR">HR</Option>
                </Select>
              </Form.Item>
            </Col>
          </Row>
          
          <Form.Item name="businessPurpose" label="Business Purpose">
            <TextArea rows={3} />
          </Form.Item>
          
          <Row gutter={16}>
            <Col span={12}>
              <Form.Item name="priority" label="Priority">
                <Select>
                  <Option value="High">High</Option>
                  <Option value="Medium">Medium</Option>
                  <Option value="Low">Low</Option>
                </Select>
              </Form.Item>
            </Col>
            <Col span={12}>
              <Form.Item name="tuningStatus" label="Status">
                <Select>
                  <Option value="Not Started">Not Started</Option>
                  <Option value="In Progress">In Progress</Option>
                  <Option value="Complete">Complete</Option>
                  <Option value="Needs Review">Needs Review</Option>
                </Select>
              </Form.Item>
            </Col>
          </Row>
        </Form>
      </Modal>

      {/* Pattern Create Modal */}
      <Modal
        title="Create Query Pattern"
        open={showPatternModal}
        onCancel={() => setShowPatternModal(false)}
        onOk={() => form.submit()}
        width={800}
      >
        <Form
          form={form}
          layout="vertical"
          onFinish={handleCreatePattern}
        >
          <Row gutter={16}>
            <Col span={12}>
              <Form.Item name="name" label="Pattern Name" rules={[{ required: true }]}>
                <Input />
              </Form.Item>
            </Col>
            <Col span={12}>
              <Form.Item name="category" label="Category" rules={[{ required: true }]}>
                <Select>
                  <Option value="Aggregation">Aggregation</Option>
                  <Option value="Filtering">Filtering</Option>
                  <Option value="Joining">Joining</Option>
                  <Option value="Grouping">Grouping</Option>
                  <Option value="Sorting">Sorting</Option>
                </Select>
              </Form.Item>
            </Col>
          </Row>
          
          <Form.Item name="description" label="Description" rules={[{ required: true }]}>
            <TextArea rows={2} />
          </Form.Item>
          
          <Form.Item name="pattern" label="Pattern" rules={[{ required: true }]}>
            <TextArea rows={3} placeholder="Describe the query pattern..." />
          </Form.Item>
          
          <Row gutter={16}>
            <Col span={12}>
              <Form.Item name="difficulty" label="Difficulty" rules={[{ required: true }]}>
                <Select>
                  <Option value="Beginner">Beginner</Option>
                  <Option value="Intermediate">Intermediate</Option>
                  <Option value="Advanced">Advanced</Option>
                </Select>
              </Form.Item>
            </Col>
            <Col span={12}>
              <Form.Item name="businessContext" label="Business Context">
                <Input />
              </Form.Item>
            </Col>
          </Row>
        </Form>
      </Modal>
    </div>
  )
}
