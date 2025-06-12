/**
 * Provider Settings Component
 *
 * Manages LLM provider configurations including API keys, endpoints,
 * connection testing, and provider health monitoring.
 */

import React, { useState, useEffect } from 'react';
import {
  Button,
  Modal,
  Form,
  Input,
  Switch,
  Select,
  Badge,
  Space,
  Alert,
  Spin
} from 'antd';
import {
  EditOutlined,
  DeleteOutlined,
  CheckCircleOutlined,
  ExclamationCircleOutlined,
  ApiOutlined
} from '@ant-design/icons';
import {
  llmManagementService,
  LLMProviderConfig,
  LLMProviderStatus
} from '../../services/llmManagementService';
import { designTokens } from '../core/design-system';
import {
  LLMTable,
  LLMFormModal,
  LLMPageHeader,
  providerActions,
  createProviderHeaderActions,
  type LLMTableColumn
} from './components';

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

  const columns: LLMTableColumn[] = [
    {
      title: 'Provider',
      dataIndex: 'name',
      key: 'name',
      render: (text: string, record: LLMProviderConfig) => (
        <Space>
          <ApiOutlined style={{ color: designTokens.colors.primary }} />
          <div>
            <div style={{
              fontWeight: designTokens.typography.fontWeight.semibold,
              color: designTokens.colors.text,
            }}>
              {text}
            </div>
            <div style={{
              fontSize: designTokens.typography.fontSize.xs,
              color: designTokens.colors.textSecondary
            }}>
              {record.type}
            </div>
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
          <div style={{ fontSize: designTokens.typography.fontSize.xs }}>
            <strong>API Key:</strong> {record.apiKey ? '✅ Configured' : '❌ Missing'}
          </div>
          {record.endpoint && (
            <div style={{ fontSize: designTokens.typography.fontSize.xs }}>
              <strong>Endpoint:</strong> {record.endpoint}
            </div>
          )}
          {record.organization && (
            <div style={{ fontSize: designTokens.typography.fontSize.xs }}>
              <strong>Org:</strong> {record.organization}
            </div>
          )}
        </Space>
      ),
    },
  ];

  const handleRefresh = () => {
    loadProviders();
    loadProviderHealth();
  };

  const tableActions = providerActions(
    handleTestProvider,
    handleEditProvider,
    handleDeleteProvider,
    testingProvider
  );

  return (
    <div style={{ padding: designTokens.spacing.lg }}>
      {/* Header */}
      <LLMPageHeader
        title="LLM Provider Configuration"
        description="Configure and manage AI providers for your application"
        actions={createProviderHeaderActions(handleAddProvider)}
        onRefresh={handleRefresh}
        refreshLoading={loading}
      />

      {/* Providers Table */}
      <LLMTable
        columns={columns}
        dataSource={providers}
        rowKey="providerId"
        loading={loading}
        actions={tableActions}
        actionColumnWidth={120}
      />

      {/* Provider Configuration Modal */}
      <LLMFormModal
        title={editingProvider ? 'Edit Provider' : 'Add Provider'}
        open={modalVisible}
        onCancel={() => setModalVisible(false)}
        form={form}
        onFinish={handleSaveProvider}
        width={700}
      >
        <Form.Item
          name="name"
          label="Provider Name"
          rules={[{ required: true, message: 'Please enter provider name' }]}
        >
          <Input
            placeholder="e.g., OpenAI Production"
            style={{ borderRadius: designTokens.borderRadius.medium }}
          />
        </Form.Item>

        <Form.Item
          name="type"
          label="Provider Type"
          rules={[{ required: true, message: 'Please select provider type' }]}
        >
          <Select
            placeholder="Select provider type"
            style={{ borderRadius: designTokens.borderRadius.medium }}
          >
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
            style={{ borderRadius: designTokens.borderRadius.medium }}
          />
        </Form.Item>

        <Form.Item
          name="endpoint"
          label="Endpoint URL (Optional)"
        >
          <Input
            placeholder="https://api.openai.com/v1"
            style={{ borderRadius: designTokens.borderRadius.medium }}
          />
        </Form.Item>

        <Form.Item
          name="organization"
          label="Organization ID (Optional)"
        >
          <Input
            placeholder="org-xxxxxxxxxx"
            style={{ borderRadius: designTokens.borderRadius.medium }}
          />
        </Form.Item>

        <div style={{
          display: 'grid',
          gridTemplateColumns: '1fr 1fr',
          gap: designTokens.spacing.lg,
          marginTop: designTokens.spacing.md
        }}>
          <Form.Item
            name="isEnabled"
            valuePropName="checked"
            style={{ margin: 0 }}
          >
            <div style={{
              display: 'flex',
              alignItems: 'center',
              gap: designTokens.spacing.sm
            }}>
              <Switch checkedChildren="Enabled" unCheckedChildren="Disabled" />
              <span style={{ fontSize: designTokens.typography.fontSize.sm }}>
                Enable Provider
              </span>
            </div>
          </Form.Item>

          <Form.Item
            name="isDefault"
            valuePropName="checked"
            style={{ margin: 0 }}
          >
            <div style={{
              display: 'flex',
              alignItems: 'center',
              gap: designTokens.spacing.sm
            }}>
              <Switch checkedChildren="Default" unCheckedChildren="Not Default" />
              <span style={{ fontSize: designTokens.typography.fontSize.sm }}>
                Set as Default
              </span>
            </div>
          </Form.Item>
        </div>
      </LLMFormModal>
    </div>
  );
};
