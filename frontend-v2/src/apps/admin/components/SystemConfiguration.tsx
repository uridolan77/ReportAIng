import React, { useState } from 'react'
import {
  Card,
  Form,
  Input,
  InputNumber,
  Switch,
  Button,
  Space,
  Typography,
  Divider,
  Alert,
  Tabs,
  Select,
  Row,
  Col,
  Modal,
  Tag,
  Tooltip
} from 'antd'
import {
  SettingOutlined,
  SaveOutlined,
  ReloadOutlined,
  LockOutlined,
  ApiOutlined,
  DatabaseOutlined,
  SecurityScanOutlined,
  ExclamationCircleOutlined
} from '@ant-design/icons'
import {
  useGetSystemConfigurationQuery,
  useUpdateSystemConfigurationMutation,
  useGetAIProvidersQuery,
  useUpdateAIProviderMutation,
  useGetSecuritySettingsQuery,
  useUpdateSecuritySettingsMutation
} from '@shared/store/api/adminApi'
import { useResponsive } from '@shared/hooks/useResponsive'

const { Title, Text } = Typography
const { TabPane } = Tabs
const { TextArea } = Input
const { Option } = Select
const { confirm } = Modal

export const SystemConfiguration: React.FC = () => {
  const responsive = useResponsive()
  const [activeTab, setActiveTab] = useState('system')
  const [hasUnsavedChanges, setHasUnsavedChanges] = useState(false)
  
  const [systemForm] = Form.useForm()
  const [aiForm] = Form.useForm()
  const [securityForm] = Form.useForm()

  const {
    data: systemConfig,
    isLoading: systemLoading,
    refetch: refetchSystem
  } = useGetSystemConfigurationQuery()

  const {
    data: aiProviders,
    isLoading: aiLoading,
    refetch: refetchAI
  } = useGetAIProvidersQuery()

  const {
    data: securitySettings,
    isLoading: securityLoading,
    refetch: refetchSecurity
  } = useGetSecuritySettingsQuery()

  const [updateSystemConfig, { isLoading: updatingSystem }] = useUpdateSystemConfigurationMutation()
  const [updateAIProvider, { isLoading: updatingAI }] = useUpdateAIProviderMutation()
  const [updateSecuritySettings, { isLoading: updatingSecurity }] = useUpdateSecuritySettingsMutation()

  const handleSystemSave = async (values: any) => {
    try {
      await updateSystemConfig(values).unwrap()
      setHasUnsavedChanges(false)
      Modal.success({
        title: 'Configuration Updated',
        content: 'System configuration has been saved successfully.'
      })
    } catch (error: any) {
      Modal.error({
        title: 'Update Failed',
        content: error.message || 'Failed to update system configuration'
      })
    }
  }

  const handleAISave = async (values: any) => {
    try {
      // Update each AI provider
      for (const provider of aiProviders || []) {
        const providerConfig = values[provider.id]
        if (providerConfig) {
          await updateAIProvider({
            providerId: provider.id,
            config: providerConfig
          }).unwrap()
        }
      }
      
      setHasUnsavedChanges(false)
      Modal.success({
        title: 'AI Providers Updated',
        content: 'AI provider configurations have been saved successfully.'
      })
    } catch (error: any) {
      Modal.error({
        title: 'Update Failed',
        content: error.message || 'Failed to update AI provider configuration'
      })
    }
  }

  const handleSecuritySave = async (values: any) => {
    try {
      await updateSecuritySettings(values).unwrap()
      setHasUnsavedChanges(false)
      Modal.success({
        title: 'Security Settings Updated',
        content: 'Security settings have been saved successfully.'
      })
    } catch (error: any) {
      Modal.error({
        title: 'Update Failed',
        content: error.message || 'Failed to update security settings'
      })
    }
  }

  const handleReset = () => {
    confirm({
      title: 'Reset Configuration',
      icon: <ExclamationCircleOutlined />,
      content: 'Are you sure you want to reset all changes? This will discard any unsaved modifications.',
      onOk() {
        systemForm.resetFields()
        aiForm.resetFields()
        securityForm.resetFields()
        setHasUnsavedChanges(false)
        refetchSystem()
        refetchAI()
        refetchSecurity()
      },
    })
  }

  const handleFormChange = () => {
    setHasUnsavedChanges(true)
  }

  if (systemLoading || aiLoading || securityLoading) {
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
            System Configuration
          </Title>
          <Text type="secondary">
            Manage system settings, AI providers, and security configurations
          </Text>
        </div>
        
        <Space>
          {hasUnsavedChanges && (
            <Tag color="orange">Unsaved Changes</Tag>
          )}
          <Button
            icon={<ReloadOutlined />}
            onClick={handleReset}
            disabled={!hasUnsavedChanges}
          >
            Reset
          </Button>
        </Space>
      </div>

      {hasUnsavedChanges && (
        <Alert
          type="warning"
          message="You have unsaved changes"
          description="Don't forget to save your configuration changes before leaving this page."
          style={{ marginBottom: '24px' }}
          showIcon
        />
      )}

      <Tabs activeKey={activeTab} onChange={setActiveTab}>
        {/* System Configuration */}
        <TabPane
          tab={
            <Space>
              <SettingOutlined />
              <span>System</span>
            </Space>
          }
          key="system"
        >
          <Card>
            <Form
              form={systemForm}
              layout="vertical"
              initialValues={systemConfig}
              onFinish={handleSystemSave}
              onValuesChange={handleFormChange}
            >
              <Row gutter={[24, 0]}>
                <Col xs={24} md={12}>
                  <Title level={4}>
                    <DatabaseOutlined /> Database Settings
                  </Title>
                  
                  <Form.Item
                    name={['database', 'connectionTimeout']}
                    label="Connection Timeout (seconds)"
                  >
                    <InputNumber min={1} max={300} style={{ width: '100%' }} />
                  </Form.Item>
                  
                  <Form.Item
                    name={['database', 'queryTimeout']}
                    label="Query Timeout (seconds)"
                  >
                    <InputNumber min={1} max={3600} style={{ width: '100%' }} />
                  </Form.Item>
                  
                  <Form.Item
                    name={['database', 'maxConnections']}
                    label="Max Connections"
                  >
                    <InputNumber min={1} max={1000} style={{ width: '100%' }} />
                  </Form.Item>
                  
                  <Form.Item
                    name={['database', 'enableQueryLogging']}
                    label="Enable Query Logging"
                    valuePropName="checked"
                  >
                    <Switch />
                  </Form.Item>
                </Col>
                
                <Col xs={24} md={12}>
                  <Title level={4}>
                    <ApiOutlined /> API Settings
                  </Title>
                  
                  <Form.Item
                    name={['api', 'rateLimitPerMinute']}
                    label="Rate Limit (requests per minute)"
                  >
                    <InputNumber min={1} max={10000} style={{ width: '100%' }} />
                  </Form.Item>
                  
                  <Form.Item
                    name={['api', 'maxRequestSize']}
                    label="Max Request Size (MB)"
                  >
                    <InputNumber min={1} max={100} style={{ width: '100%' }} />
                  </Form.Item>
                  
                  <Form.Item
                    name={['api', 'enableCors']}
                    label="Enable CORS"
                    valuePropName="checked"
                  >
                    <Switch />
                  </Form.Item>
                  
                  <Form.Item
                    name={['api', 'corsOrigins']}
                    label="CORS Origins"
                  >
                    <TextArea
                      rows={3}
                      placeholder="https://example.com&#10;https://app.example.com"
                    />
                  </Form.Item>
                </Col>
              </Row>
              
              <Divider />
              
              <Row gutter={[24, 0]}>
                <Col xs={24} md={12}>
                  <Title level={4}>Performance</Title>
                  
                  <Form.Item
                    name={['performance', 'cacheEnabled']}
                    label="Enable Caching"
                    valuePropName="checked"
                  >
                    <Switch />
                  </Form.Item>
                  
                  <Form.Item
                    name={['performance', 'cacheTtlMinutes']}
                    label="Cache TTL (minutes)"
                  >
                    <InputNumber min={1} max={1440} style={{ width: '100%' }} />
                  </Form.Item>
                </Col>
                
                <Col xs={24} md={12}>
                  <Title level={4}>Logging</Title>
                  
                  <Form.Item
                    name={['logging', 'level']}
                    label="Log Level"
                  >
                    <Select style={{ width: '100%' }}>
                      <Option value="Debug">Debug</Option>
                      <Option value="Info">Info</Option>
                      <Option value="Warning">Warning</Option>
                      <Option value="Error">Error</Option>
                    </Select>
                  </Form.Item>
                  
                  <Form.Item
                    name={['logging', 'enableFileLogging']}
                    label="Enable File Logging"
                    valuePropName="checked"
                  >
                    <Switch />
                  </Form.Item>
                </Col>
              </Row>
              
              <Form.Item>
                <Button
                  type="primary"
                  htmlType="submit"
                  loading={updatingSystem}
                  icon={<SaveOutlined />}
                >
                  Save System Configuration
                </Button>
              </Form.Item>
            </Form>
          </Card>
        </TabPane>

        {/* AI Providers */}
        <TabPane
          tab={
            <Space>
              <ApiOutlined />
              <span>AI Providers</span>
            </Space>
          }
          key="ai"
        >
          <Card>
            <Form
              form={aiForm}
              layout="vertical"
              onFinish={handleAISave}
              onValuesChange={handleFormChange}
            >
              {aiProviders?.map((provider) => (
                <Card
                  key={provider.id}
                  size="small"
                  title={
                    <Space>
                      <Text strong>{provider.name}</Text>
                      <Tag color={provider.isEnabled ? 'green' : 'default'}>
                        {provider.isEnabled ? 'Enabled' : 'Disabled'}
                      </Tag>
                    </Space>
                  }
                  style={{ marginBottom: '16px' }}
                >
                  <Row gutter={[16, 0]}>
                    <Col xs={24} md={12}>
                      <Form.Item
                        name={[provider.id, 'isEnabled']}
                        label="Enable Provider"
                        valuePropName="checked"
                        initialValue={provider.isEnabled}
                      >
                        <Switch />
                      </Form.Item>
                      
                      <Form.Item
                        name={[provider.id, 'apiKey']}
                        label="API Key"
                        initialValue={provider.apiKey}
                      >
                        <Input.Password placeholder="Enter API key" />
                      </Form.Item>
                      
                      <Form.Item
                        name={[provider.id, 'endpoint']}
                        label="API Endpoint"
                        initialValue={provider.endpoint}
                      >
                        <Input placeholder="https://api.provider.com" />
                      </Form.Item>
                    </Col>
                    
                    <Col xs={24} md={12}>
                      <Form.Item
                        name={[provider.id, 'model']}
                        label="Default Model"
                        initialValue={provider.model}
                      >
                        <Input placeholder="gpt-4, claude-3, etc." />
                      </Form.Item>
                      
                      <Form.Item
                        name={[provider.id, 'maxTokens']}
                        label="Max Tokens"
                        initialValue={provider.maxTokens}
                      >
                        <InputNumber min={1} max={100000} style={{ width: '100%' }} />
                      </Form.Item>
                      
                      <Form.Item
                        name={[provider.id, 'temperature']}
                        label="Temperature"
                        initialValue={provider.temperature}
                      >
                        <InputNumber min={0} max={2} step={0.1} style={{ width: '100%' }} />
                      </Form.Item>
                    </Col>
                  </Row>
                </Card>
              ))}
              
              <Form.Item>
                <Button
                  type="primary"
                  htmlType="submit"
                  loading={updatingAI}
                  icon={<SaveOutlined />}
                >
                  Save AI Configuration
                </Button>
              </Form.Item>
            </Form>
          </Card>
        </TabPane>

        {/* Security Settings */}
        <TabPane
          tab={
            <Space>
              <SecurityScanOutlined />
              <span>Security</span>
            </Space>
          }
          key="security"
        >
          <Card>
            <Form
              form={securityForm}
              layout="vertical"
              initialValues={securitySettings}
              onFinish={handleSecuritySave}
              onValuesChange={handleFormChange}
            >
              <Row gutter={[24, 0]}>
                <Col xs={24} md={12}>
                  <Title level={4}>
                    <LockOutlined /> Authentication
                  </Title>
                  
                  <Form.Item
                    name={['auth', 'requireMfa']}
                    label="Require Multi-Factor Authentication"
                    valuePropName="checked"
                  >
                    <Switch />
                  </Form.Item>
                  
                  <Form.Item
                    name={['auth', 'sessionTimeoutMinutes']}
                    label="Session Timeout (minutes)"
                  >
                    <InputNumber min={5} max={1440} style={{ width: '100%' }} />
                  </Form.Item>
                  
                  <Form.Item
                    name={['auth', 'maxLoginAttempts']}
                    label="Max Login Attempts"
                  >
                    <InputNumber min={1} max={10} style={{ width: '100%' }} />
                  </Form.Item>
                  
                  <Form.Item
                    name={['auth', 'lockoutDurationMinutes']}
                    label="Lockout Duration (minutes)"
                  >
                    <InputNumber min={1} max={1440} style={{ width: '100%' }} />
                  </Form.Item>
                </Col>
                
                <Col xs={24} md={12}>
                  <Title level={4}>Data Protection</Title>
                  
                  <Form.Item
                    name={['data', 'encryptSensitiveData']}
                    label="Encrypt Sensitive Data"
                    valuePropName="checked"
                  >
                    <Switch />
                  </Form.Item>
                  
                  <Form.Item
                    name={['data', 'auditLogging']}
                    label="Enable Audit Logging"
                    valuePropName="checked"
                  >
                    <Switch />
                  </Form.Item>
                  
                  <Form.Item
                    name={['data', 'dataRetentionDays']}
                    label="Data Retention (days)"
                  >
                    <InputNumber min={1} max={3650} style={{ width: '100%' }} />
                  </Form.Item>
                  
                  <Form.Item
                    name={['data', 'allowDataExport']}
                    label="Allow Data Export"
                    valuePropName="checked"
                  >
                    <Switch />
                  </Form.Item>
                </Col>
              </Row>
              
              <Form.Item>
                <Button
                  type="primary"
                  htmlType="submit"
                  loading={updatingSecurity}
                  icon={<SaveOutlined />}
                >
                  Save Security Settings
                </Button>
              </Form.Item>
            </Form>
          </Card>
        </TabPane>
      </Tabs>
    </div>
  )
}
