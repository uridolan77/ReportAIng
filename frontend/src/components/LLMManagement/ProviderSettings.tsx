/**
 * Provider Settings Component
 * 
 * Manages LLM provider configurations including API keys, endpoints,
 * connection testing, and provider health monitoring.
 */

import React, { useState, useEffect } from 'react';
import { 
  Table, 
  Button, 
  Modal, 
  Form, 
  Input, 
  Switch, 
  Select, 
  Badge, 
  Space, 
  Popconfirm,
  Alert,
  Spin,
  Card,
  Flex
} from 'antd';
import { 
  PlusOutlined, 
  EditOutlined, 
  DeleteOutlined, 
  CheckCircleOutlined,
  ExclamationCircleOutlined,
  ApiOutlined,
  ReloadOutlined
} from '@ant-design/icons';
import { 
  llmManagementService, 
  LLMProviderConfig, 
  LLMProviderStatus 
} from '../../services/llmManagementService';

const { Option } = Select;

export const ProviderSettings: React.FC = () => {
  const [providers, setProviders] = useState<LLMProviderConfig[]>([]);
  const [providerHealth, setProviderHealth] = useState<LLMProviderStatus[]>([]);
  const [loading, setLoading] = useState(true);
  const [modalVisible, setModalVisible] = useState(false);
  const [editingProvider, setEditingProvider] = useState<LLMProviderConfig | null>(null);
  const [testingProvider, setTestingProvider] = useState<string | null>(null);
  const [form] = Form.useForm();

  useEffect(() => {
    loadProviders();
    loadProviderHealth();
  }, []);

  const loadProviders = async () => {
    try {
      setLoading(true);
      const data = await llmManagementService.getProviders();
      setProviders(data);
    } catch (error) {
      console.error('Failed to load providers:', error);
    } finally {
      setLoading(false);
    }
  };

  const loadProviderHealth = async () => {
    try {
      const health = await llmManagementService.getProviderHealth();
      setProviderHealth(health);
    } catch (error) {
      console.error('Failed to load provider health:', error);
    }
  };

  const handleAddProvider = () => {
    setEditingProvider(null);
    form.resetFields();
    setModalVisible(true);
  };

  const handleEditProvider = (provider: LLMProviderConfig) => {
    setEditingProvider(provider);
    form.setFieldsValue({
      ...provider,
      // Don't show the actual API key for security
      apiKey: provider.apiKey === '***CONFIGURED***' ? '' : provider.apiKey
    });
    setModalVisible(true);
  };

  const handleDeleteProvider = async (providerId: string) => {
    try {
      await llmManagementService.deleteProvider(providerId);
      await loadProviders();
      await loadProviderHealth();
    } catch (error) {
      console.error('Failed to delete provider:', error);
    }
  };

  const handleTestProvider = async (providerId: string) => {
    try {
      setTestingProvider(providerId);
      const status = await llmManagementService.testProvider(providerId);
      
      // Update the health status for this provider
      setProviderHealth(prev => 
        prev.map(h => h.providerId === providerId ? status : h)
      );
      
      Modal.info({
        title: 'Connection Test Result',
        content: (
          <div>
            <p><strong>Provider:</strong> {status.name}</p>
            <p><strong>Status:</strong> {status.isHealthy ? '✅ Healthy' : '❌ Unhealthy'}</p>
            <p><strong>Response Time:</strong> {status.lastResponseTime}ms</p>
            {status.lastError && <p><strong>Error:</strong> {status.lastError}</p>}
          </div>
        ),
      });
    } catch (error) {
      console.error('Failed to test provider:', error);
    } finally {
      setTestingProvider(null);
    }
  };

  const handleSaveProvider = async (values: any) => {
    try {
      const providerData: LLMProviderConfig = {
        providerId: editingProvider?.providerId || `provider_${Date.now()}`,
        name: values.name,
        type: values.type,
        apiKey: values.apiKey || editingProvider?.apiKey || '',
        endpoint: values.endpoint || '',
        organization: values.organization || '',
        isEnabled: values.isEnabled ?? true,
        isDefault: values.isDefault ?? false,
        settings: values.settings || {},
        createdAt: editingProvider?.createdAt || new Date().toISOString(),
        updatedAt: new Date().toISOString(),
      };

      await llmManagementService.saveProvider(providerData);
      setModalVisible(false);
      await loadProviders();
      await loadProviderHealth();
    } catch (error) {
      console.error('Failed to save provider:', error);
    }
  };

  const getHealthStatus = (providerId: string) => {
    const health = providerHealth.find(h => h.providerId === providerId);
    if (!health) return { status: 'default', text: 'Unknown' };
    
    if (health.isHealthy) {
      return { status: 'success', text: 'Healthy' };
    } else {
      return { status: 'error', text: 'Unhealthy' };
    }
  };

  const columns = [
    {
      title: 'Provider',
      dataIndex: 'name',
      key: 'name',
      render: (text: string, record: LLMProviderConfig) => (
        <Space>
          <ApiOutlined />
          <div>
            <div style={{ fontWeight: 'bold' }}>{text}</div>
            <div style={{ fontSize: '12px', color: '#666' }}>{record.type}</div>
          </div>
        </Space>
      ),
    },
    {
      title: 'Status',
      key: 'status',
      render: (_: any, record: LLMProviderConfig) => {
        const health = getHealthStatus(record.providerId);
        return (
          <Space direction="vertical" size="small">
            <Badge status={health.status as any} text={health.text} />
            <Badge 
              status={record.isEnabled ? 'success' : 'default'} 
              text={record.isEnabled ? 'Enabled' : 'Disabled'} 
            />
            {record.isDefault && <Badge status="processing" text="Default" />}
          </Space>
        );
      },
    },
    {
      title: 'Configuration',
      key: 'config',
      render: (_: any, record: LLMProviderConfig) => (
        <Space direction="vertical" size="small">
          <div style={{ fontSize: '12px' }}>
            <strong>API Key:</strong> {record.apiKey ? '✅ Configured' : '❌ Missing'}
          </div>
          {record.endpoint && (
            <div style={{ fontSize: '12px' }}>
              <strong>Endpoint:</strong> {record.endpoint}
            </div>
          )}
          {record.organization && (
            <div style={{ fontSize: '12px' }}>
              <strong>Org:</strong> {record.organization}
            </div>
          )}
        </Space>
      ),
    },
    {
      title: 'Actions',
      key: 'actions',
      render: (_: any, record: LLMProviderConfig) => (
        <Space>
          <Button
            size="small"
            icon={<CheckCircleOutlined />}
            loading={testingProvider === record.providerId}
            onClick={() => handleTestProvider(record.providerId)}
          >
            Test
          </Button>
          <Button
            size="small"
            icon={<EditOutlined />}
            onClick={() => handleEditProvider(record)}
          >
            Edit
          </Button>
          <Popconfirm
            title="Are you sure you want to delete this provider?"
            onConfirm={() => handleDeleteProvider(record.providerId)}
            okText="Yes"
            cancelText="No"
          >
            <Button
              size="small"
              danger
              icon={<DeleteOutlined />}
            >
              Delete
            </Button>
          </Popconfirm>
        </Space>
      ),
    },
  ];

  return (
    <div style={{ padding: '24px' }}>
      {/* Header Actions */}
      <Card size="small" style={{ marginBottom: '16px' }}>
        <Flex justify="between" align="center">
          <div>
            <h4 style={{ margin: 0 }}>LLM Provider Configuration</h4>
            <p style={{ margin: 0, color: '#666' }}>
              Configure and manage AI providers for your application
            </p>
          </div>
          <Space>
            <Button 
              icon={<ReloadOutlined />} 
              onClick={() => {
                loadProviders();
                loadProviderHealth();
              }}
            >
              Refresh
            </Button>
            <Button 
              type="primary" 
              icon={<PlusOutlined />}
              onClick={handleAddProvider}
            >
              Add Provider
            </Button>
          </Space>
        </Flex>
      </Card>

      {/* Providers Table */}
      <Table
        columns={columns}
        dataSource={providers}
        rowKey="providerId"
        loading={loading}
        pagination={{
          pageSize: 10,
          showSizeChanger: true,
          showQuickJumper: true,
        }}
      />

      {/* Provider Configuration Modal */}
      <Modal
        title={editingProvider ? 'Edit Provider' : 'Add Provider'}
        open={modalVisible}
        onCancel={() => setModalVisible(false)}
        onOk={() => form.submit()}
        width={600}
      >
        <Form
          form={form}
          layout="vertical"
          onFinish={handleSaveProvider}
        >
          <Form.Item
            name="name"
            label="Provider Name"
            rules={[{ required: true, message: 'Please enter provider name' }]}
          >
            <Input placeholder="e.g., OpenAI Production" />
          </Form.Item>

          <Form.Item
            name="type"
            label="Provider Type"
            rules={[{ required: true, message: 'Please select provider type' }]}
          >
            <Select placeholder="Select provider type">
              <Option value="OpenAI">OpenAI</Option>
              <Option value="AzureOpenAI">Azure OpenAI</Option>
              <Option value="Anthropic">Anthropic</Option>
              <Option value="Google">Google AI</Option>
            </Select>
          </Form.Item>

          <Form.Item
            name="apiKey"
            label="API Key"
            rules={[{ required: !editingProvider, message: 'Please enter API key' }]}
          >
            <Input.Password 
              placeholder={editingProvider ? "Leave empty to keep current key" : "Enter API key"}
            />
          </Form.Item>

          <Form.Item
            name="endpoint"
            label="Endpoint URL (Optional)"
          >
            <Input placeholder="https://api.openai.com/v1" />
          </Form.Item>

          <Form.Item
            name="organization"
            label="Organization ID (Optional)"
          >
            <Input placeholder="org-xxxxxxxxxx" />
          </Form.Item>

          <Space style={{ width: '100%', justifyContent: 'space-between' }}>
            <Form.Item
              name="isEnabled"
              valuePropName="checked"
              style={{ margin: 0 }}
            >
              <Switch checkedChildren="Enabled" unCheckedChildren="Disabled" />
            </Form.Item>

            <Form.Item
              name="isDefault"
              valuePropName="checked"
              style={{ margin: 0 }}
            >
              <Switch checkedChildren="Default" unCheckedChildren="Not Default" />
            </Form.Item>
          </Space>
        </Form>
      </Modal>
    </div>
  );
};
