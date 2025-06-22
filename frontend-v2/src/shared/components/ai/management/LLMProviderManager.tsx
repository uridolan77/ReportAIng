import React, { useState, useMemo } from 'react'
import { 
  Card, 
  Table, 
  Tag, 
  Button, 
  Space, 
  Switch, 
  Progress,
  Modal,
  Form,
  Input,
  InputNumber,
  Select,
  Alert,
  Tooltip,
  Badge,
  Divider,
  Statistic,
  Row,
  Col,
  Typography
} from 'antd'
import {
  SettingOutlined,
  PlayCircleOutlined,
  PauseCircleOutlined,
  EditOutlined,
  DeleteOutlined,
  PlusOutlined,
  ReloadOutlined,
  ExclamationCircleOutlined,
  CheckCircleOutlined,
  CloseCircleOutlined,
  ThunderboltOutlined,
  ClockCircleOutlined,
  DollarOutlined
} from '@ant-design/icons'
import { ConfidenceIndicator } from '../common/ConfidenceIndicator'
import {
  useGetProvidersQuery,
  useGetProviderHealthQuery,
  useSaveProviderMutation,
  useDeleteProviderMutation,
  useTestProviderMutation,
  type LLMProviderConfig,
  type LLMProviderStatus
} from '@shared/store/api/llmManagementApi'

const { confirm } = Modal
const { Text } = Typography

export interface LLMProviderManagerProps {
  showMetrics?: boolean
  showConfiguration?: boolean
  compact?: boolean
  onProviderSelect?: (provider: LLMProviderConfig) => void
  className?: string
  testId?: string
}

/**
 * LLMProviderManager - Comprehensive LLM provider management interface
 * 
 * Features:
 * - Real-time provider health monitoring
 * - Interactive configuration management
 * - Performance metrics and analytics
 * - Provider enable/disable controls
 * - Cost tracking and optimization
 * - Integration with backend APIs
 */
export const LLMProviderManager: React.FC<LLMProviderManagerProps> = ({
  showMetrics = true,
  showConfiguration = true,
  compact = false,
  onProviderSelect,
  className,
  testId = 'llm-provider-manager'
}) => {
  const [selectedProvider, setSelectedProvider] = useState<LLMProviderConfig | null>(null)
  const [configModalVisible, setConfigModalVisible] = useState(false)
  const [form] = Form.useForm()

  // Real API data
  const { data: providers, isLoading: providersLoading, refetch: refetchProviders } = useGetProvidersQuery()
  const { data: healthStatus, isLoading: healthLoading, refetch: refetchHealth } = useGetProviderHealthQuery()
  const [saveProvider, { isLoading: saveLoading }] = useSaveProviderMutation()
  const [deleteProvider, { isLoading: deleteLoading }] = useDeleteProviderMutation()
  const [testProvider, { isLoading: testLoading }] = useTestProviderMutation()

  // Combine providers with health status
  const providersWithHealth = useMemo(() => {
    if (!providers || !healthStatus) return []

    return providers.map(provider => {
      const health = healthStatus.find(h => h.providerId === provider.providerId)
      return {
        ...provider,
        health: health || {
          providerId: provider.providerId,
          isHealthy: false,
          status: 'offline' as const,
          lastChecked: new Date().toISOString(),
          errorMessage: 'No health data available'
        }
      }
    })
  }, [providers, healthStatus])

  // Get status color
  const getStatusColor = (status: string) => {
    switch (status) {
      case 'healthy': return 'green'
      case 'degraded': return 'orange'
      case 'unhealthy': return 'red'
      case 'offline': return 'default'
      default: return 'default'
    }
  }

  // Get status icon
  const getStatusIcon = (status: string) => {
    switch (status) {
      case 'healthy': return <CheckCircleOutlined style={{ color: '#52c41a' }} />
      case 'degraded': return <ExclamationCircleOutlined style={{ color: '#faad14' }} />
      case 'unhealthy': return <CloseCircleOutlined style={{ color: '#ff4d4f' }} />
      case 'offline': return <PauseCircleOutlined style={{ color: '#d9d9d9' }} />
      default: return null
    }
  }

  // Handle provider toggle
  const handleToggleProvider = async (provider: LLMProviderConfig, enabled: boolean) => {
    try {
      await saveProvider({
        ...provider,
        isEnabled: enabled
      }).unwrap()

      refetchProviders()
      refetchHealth()
    } catch (error) {
      console.error('Failed to toggle provider:', error)
    }
  }

  // Handle provider configuration
  const handleConfigureProvider = (provider: LLMProviderConfig) => {
    setSelectedProvider(provider)
    form.setFieldsValue({
      name: provider.name,
      type: provider.type,
      endpoint: provider.endpoint,
      organization: provider.organization,
      isDefault: provider.isDefault
    })
    setConfigModalVisible(true)
    onProviderSelect?.(provider)
  }

  // Handle configuration save
  const handleSaveConfiguration = async () => {
    try {
      const values = await form.validateFields()
      if (selectedProvider) {
        await saveProvider({
          ...selectedProvider,
          name: values.name,
          type: values.type,
          endpoint: values.endpoint,
          organization: values.organization,
          isDefault: values.isDefault
        }).unwrap()

        setConfigModalVisible(false)
        setSelectedProvider(null)
        refetchProviders()
        refetchHealth()
      }
    } catch (error) {
      console.error('Configuration save failed:', error)
    }
  }

  // Handle provider deletion
  const handleDeleteProvider = (provider: LLMProviderConfig) => {
    confirm({
      title: 'Delete Provider',
      content: `Are you sure you want to delete ${provider.name}? This action cannot be undone.`,
      onOk: async () => {
        try {
          await deleteProvider(provider.providerId).unwrap()
          refetchProviders()
          refetchHealth()
        } catch (error) {
          console.error('Failed to delete provider:', error)
        }
      }
    })
  }

  // Handle provider test
  const handleTestProvider = async (provider: LLMProviderConfig) => {
    try {
      await testProvider(provider.providerId).unwrap()
      refetchHealth()
    } catch (error) {
      console.error('Failed to test provider:', error)
    }
  }

  // Table columns
  const columns = [
    {
      title: 'Provider',
      dataIndex: 'name',
      key: 'name',
      render: (text: string, record: any) => (
        <Space>
          {getStatusIcon(record.health.status)}
          <div>
            <div style={{ fontWeight: 'bold' }}>{text}</div>
            <Text type="secondary" style={{ fontSize: '12px' }}>
              {record.type} • {record.providerId}
            </Text>
          </div>
        </Space>
      )
    },
    {
      title: 'Status',
      dataIndex: ['health', 'status'],
      key: 'status',
      render: (status: string) => (
        <Tag color={getStatusColor(status)}>
          {status.toUpperCase()}
        </Tag>
      )
    },
    {
      title: 'Enabled',
      dataIndex: 'isEnabled',
      key: 'enabled',
      render: (isEnabled: boolean, record: any) => (
        <Switch
          checked={isEnabled}
          loading={saveLoading}
          onChange={(checked) => handleToggleProvider(record, checked)}
        />
      )
    },
    ...(showMetrics ? [{
      title: 'Health',
      key: 'health',
      render: (_: any, record: any) => (
        <Space direction="vertical" size="small">
          <div>
            <Text type="secondary">Status: </Text>
            <Text>{record.health.isHealthy ? 'Healthy' : 'Unhealthy'}</Text>
          </div>
          <div>
            <Text type="secondary">Response: </Text>
            <Text>{record.health.responseTime || 'N/A'}ms</Text>
          </div>
        </Space>
      )
    }] : []),
    {
      title: 'Last Checked',
      key: 'lastChecked',
      render: (_: any, record: any) => (
        <Space direction="vertical" size="small">
          <Text style={{ fontSize: '12px' }}>
            {new Date(record.health.lastChecked).toLocaleString()}
          </Text>
          {record.health.errorMessage && (
            <Text type="danger" style={{ fontSize: '11px' }}>
              {record.health.errorMessage}
            </Text>
          )}
        </Space>
      )
    },
    ...(showConfiguration ? [{
      title: 'Actions',
      key: 'actions',
      render: (_: any, record: any) => (
        <Space>
          <Tooltip title="Test Connection">
            <Button
              type="text"
              icon={<PlayCircleOutlined />}
              loading={testLoading}
              onClick={() => handleTestProvider(record)}
            />
          </Tooltip>
          <Tooltip title="Configure">
            <Button
              type="text"
              icon={<EditOutlined />}
              onClick={() => handleConfigureProvider(record)}
            />
          </Tooltip>
          <Tooltip title="Delete">
            <Button
              type="text"
              danger
              icon={<DeleteOutlined />}
              loading={deleteLoading}
              onClick={() => handleDeleteProvider(record)}
            />
          </Tooltip>
        </Space>
      )
    }] : [])
  ]

  // Summary metrics
  const summaryMetrics = useMemo(() => {
    if (!providersWithHealth.length) return null

    const activeProviders = providersWithHealth.filter(p => p.isEnabled).length
    const healthyProviders = providersWithHealth.filter(p => p.health.isHealthy).length
    const avgResponseTime = providersWithHealth
      .filter(p => p.health.responseTime)
      .reduce((acc, p) => acc + (p.health.responseTime || 0), 0) /
      (providersWithHealth.filter(p => p.health.responseTime).length || 1)

    return {
      activeProviders,
      healthyProviders,
      totalProviders: providersWithHealth.length,
      avgResponseTime,
      defaultProviders: providersWithHealth.filter(p => p.isDefault).length
    }
  }, [providersWithHealth])

  return (
    <div className={className} data-testid={testId}>
      {/* Summary Cards */}
      {showMetrics && summaryMetrics && !compact && (
        <Row gutter={[16, 16]} style={{ marginBottom: 24 }}>
          <Col xs={24} sm={12} lg={6}>
            <Card>
              <Statistic
                title="Active Providers"
                value={summaryMetrics.activeProviders}
                suffix={`/ ${summaryMetrics.totalProviders}`}
                prefix={<ThunderboltOutlined />}
                valueStyle={{ color: '#3f8600' }}
              />
            </Card>
          </Col>
          <Col xs={24} sm={12} lg={6}>
            <Card>
              <Statistic
                title="Healthy Providers"
                value={summaryMetrics.healthyProviders}
                suffix={`/ ${summaryMetrics.totalProviders}`}
                prefix={<CheckCircleOutlined />}
                valueStyle={{ color: '#52c41a' }}
              />
            </Card>
          </Col>
          <Col xs={24} sm={12} lg={6}>
            <Card>
              <Statistic
                title="Avg Response Time"
                value={summaryMetrics.avgResponseTime.toFixed(0)}
                suffix="ms"
                prefix={<ClockCircleOutlined />}
                valueStyle={{ color: '#1890ff' }}
              />
            </Card>
          </Col>
          <Col xs={24} sm={12} lg={6}>
            <Card>
              <Statistic
                title="Default Providers"
                value={summaryMetrics.defaultProviders}
                suffix={`/ ${summaryMetrics.totalProviders}`}
                prefix={<SettingOutlined />}
                valueStyle={{ color: '#722ed1' }}
              />
            </Card>
          </Col>
        </Row>
      )}

      {/* Providers Table */}
      <Card 
        title={
          <Space>
            <span>LLM Providers</span>
            <Badge count={providersWithHealth.length} />
          </Space>
        }
        extra={
          <Space>
            <Button
              icon={<ReloadOutlined />}
              loading={providersLoading || healthLoading}
              onClick={() => {
                refetchProviders()
                refetchHealth()
              }}
            >
              Refresh
            </Button>
            {showConfiguration && (
              <Button type="primary" icon={<PlusOutlined />}>
                Add Provider
              </Button>
            )}
          </Space>
        }
        size={compact ? 'small' : 'default'}
      >
        <Table
          columns={columns}
          dataSource={providersWithHealth}
          rowKey="providerId"
          loading={providersLoading || healthLoading}
          pagination={compact ? false : { pageSize: 10 }}
          size={compact ? 'small' : 'default'}
        />
      </Card>

      {/* Configuration Modal */}
      {showConfiguration && (
        <Modal
          title={`Configure ${selectedProvider?.name}`}
          open={configModalVisible}
          onOk={handleSaveConfiguration}
          onCancel={() => setConfigModalVisible(false)}
          confirmLoading={saveLoading}
          width={600}
        >
          <Form form={form} layout="vertical">
            <Row gutter={16}>
              <Col span={12}>
                <Form.Item
                  name="name"
                  label="Provider Name"
                  rules={[{ required: true, message: 'Please enter provider name' }]}
                >
                  <Input />
                </Form.Item>
              </Col>
              <Col span={12}>
                <Form.Item
                  name="type"
                  label="Provider Type"
                  rules={[{ required: true, message: 'Please select provider type' }]}
                >
                  <Select>
                    <Select.Option value="openai">OpenAI</Select.Option>
                    <Select.Option value="anthropic">Anthropic</Select.Option>
                    <Select.Option value="azure">Azure OpenAI</Select.Option>
                    <Select.Option value="google">Google</Select.Option>
                    <Select.Option value="local">Local</Select.Option>
                  </Select>
                </Form.Item>
              </Col>
            </Row>
            <Row gutter={16}>
              <Col span={12}>
                <Form.Item
                  name="endpoint"
                  label="API Endpoint"
                >
                  <Input placeholder="https://api.openai.com/v1" />
                </Form.Item>
              </Col>
              <Col span={12}>
                <Form.Item
                  name="organization"
                  label="Organization ID"
                >
                  <Input placeholder="Optional organization ID" />
                </Form.Item>
              </Col>
            </Row>
            <Row gutter={16}>
              <Col span={24}>
                <Form.Item
                  name="isDefault"
                  valuePropName="checked"
                >
                  <Switch /> Set as default provider
                </Form.Item>
              </Col>
            </Row>
          </Form>
        </Modal>
      )}
    </div>
  )
}

export default LLMProviderManager
