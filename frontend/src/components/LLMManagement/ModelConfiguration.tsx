/**
 * Model Configuration Component
 *
 * Manages LLM model configurations including parameters, use cases,
 * cost settings, and model capabilities.
 */

import React, { useState, useEffect } from 'react';
import {
  Button,
  Form,
  Input,
  InputNumber,
  Select,
  Switch,
  message,
  Tag
} from 'antd';
import {
  SettingOutlined,
  RobotOutlined,
  EditOutlined,
  DeleteOutlined
} from '@ant-design/icons';
import { llmManagementService, LLMModelConfig, LLMProviderConfig } from '../../services/llmManagementService';
import { designTokens } from '../core/design-system';
import {
  LLMTable,
  LLMFormModal,
  LLMPageHeader,
  modelActions,
  createModelHeaderActions,
  type LLMTableColumn
} from './components';

const { Option } = Select;

export const ModelConfiguration: React.FC = () => {
  const [models, setModels] = useState<LLMModelConfig[]>([]);
  const [providers, setProviders] = useState<LLMProviderConfig[]>([]);
  const [loading, setLoading] = useState(true);
  const [modalVisible, setModalVisible] = useState(false);
  const [editingModel, setEditingModel] = useState<LLMModelConfig | null>(null);
  const [form] = Form.useForm();

  useEffect(() => {
    loadData();
  }, []);

  const loadData = async () => {
    try {
      setLoading(true);
      const [modelsData, providersData] = await Promise.all([
        llmManagementService.getModels(),
        llmManagementService.getProviders()
      ]);
      setModels(modelsData);
      setProviders(providersData);
    } catch (error) {
      message.error('Failed to load model configuration data');
      console.error('Error loading data:', error);
    } finally {
      setLoading(false);
    }
  };

  const handleAddModel = () => {
    setEditingModel(null);
    form.resetFields();
    setModalVisible(true);
  };

  const handleEditModel = (model: LLMModelConfig) => {
    setEditingModel(model);
    form.setFieldsValue({
      ...model,
      capabilities: JSON.stringify(model.capabilities, null, 2)
    });
    setModalVisible(true);
  };

  const handleDeleteModel = async (modelId: string) => {
    try {
      await llmManagementService.deleteModel(modelId);
      message.success('Model deleted successfully');
      await loadData();
    } catch (error) {
      message.error('Failed to delete model');
      console.error('Error deleting model:', error);
    }
  };

  const handleSaveModel = async (values: any) => {
    try {
      const modelData: LLMModelConfig = {
        modelId: editingModel?.modelId || `model_${Date.now()}`,
        providerId: values.providerId,
        name: values.name,
        displayName: values.displayName,
        temperature: values.temperature,
        maxTokens: values.maxTokens,
        topP: values.topP,
        frequencyPenalty: values.frequencyPenalty,
        presencePenalty: values.presencePenalty,
        isEnabled: values.isEnabled ?? true,
        useCase: values.useCase,
        costPerToken: values.costPerToken,
        capabilities: values.capabilities ? JSON.parse(values.capabilities) : {}
      };

      await llmManagementService.saveModel(modelData);
      setModalVisible(false);
      message.success('Model saved successfully');
      await loadData();
    } catch (error) {
      message.error('Failed to save model');
      console.error('Error saving model:', error);
    }
  };

  const columns: LLMTableColumn[] = [
    {
      title: 'Model',
      dataIndex: 'displayName',
      key: 'displayName',
      render: (text: string, record: LLMModelConfig) => (
        <div>
          <div style={{
            fontWeight: designTokens.typography.fontWeight.semibold,
            color: designTokens.colors.text,
          }}>
            <RobotOutlined style={{
              color: designTokens.colors.primary,
              marginRight: designTokens.spacing.xs
            }} />
            {text}
          </div>
          <div style={{
            fontSize: designTokens.typography.fontSize.xs,
            color: designTokens.colors.textSecondary,
            marginLeft: '20px'
          }}>
            {record.name}
          </div>
        </div>
      ),
    },
    {
      title: 'Provider',
      dataIndex: 'providerId',
      key: 'providerId',
      render: (providerId: string) => {
        const provider = providers.find(p => p.providerId === providerId);
        return (
          <Tag
            color="blue"
            style={{ borderRadius: designTokens.borderRadius.medium }}
          >
            {provider?.name || providerId}
          </Tag>
        );
      },
    },
    {
      title: 'Use Case',
      dataIndex: 'useCase',
      key: 'useCase',
      render: (useCase: string) => (
        <Tag
          color={useCase === 'SQL' ? 'blue' : useCase === 'Insights' ? 'green' : 'default'}
          style={{ borderRadius: designTokens.borderRadius.medium }}
        >
          {useCase}
        </Tag>
      ),
    },
    {
      title: 'Parameters',
      key: 'parameters',
      render: (record: LLMModelConfig) => (
        <div style={{ fontSize: designTokens.typography.fontSize.xs }}>
          <div style={{ marginBottom: designTokens.spacing.xs }}>
            <strong>Temp:</strong> {record.temperature}
          </div>
          <div style={{ marginBottom: designTokens.spacing.xs }}>
            <strong>Max Tokens:</strong> {record.maxTokens}
          </div>
          <div>
            <strong>Top P:</strong> {record.topP}
          </div>
        </div>
      ),
    },
    {
      title: 'Cost/Token',
      dataIndex: 'costPerToken',
      key: 'costPerToken',
      render: (cost: number) => (
        <span style={{
          fontFamily: designTokens.typography.fontFamily.mono,
          fontSize: designTokens.typography.fontSize.xs,
          color: designTokens.colors.textSecondary
        }}>
          ${cost.toFixed(8)}
        </span>
      ),
    },
    {
      title: 'Status',
      dataIndex: 'isEnabled',
      key: 'isEnabled',
      render: (isEnabled: boolean) => (
        <Tag
          color={isEnabled ? 'green' : 'red'}
          style={{ borderRadius: designTokens.borderRadius.medium }}
        >
          {isEnabled ? 'Enabled' : 'Disabled'}
        </Tag>
      ),
    },
  ];

  const tableActions = modelActions(
    handleEditModel,
    handleDeleteModel
  );

  return (
    <div style={{ padding: designTokens.spacing.lg }}>
      {/* Header */}
      <LLMPageHeader
        title="Model Configuration"
        description="Configure model parameters, use cases, and capabilities for each LLM provider"
        actions={createModelHeaderActions(handleAddModel)}
        onRefresh={loadData}
        refreshLoading={loading}
      />

      {/* Models Table */}
      <LLMTable
        columns={columns}
        dataSource={models}
        rowKey="modelId"
        loading={loading}
        actions={tableActions}
        actionColumnWidth={120}
      />

      {/* Model Configuration Modal */}
      <LLMFormModal
        title={editingModel ? 'Edit Model' : 'Add Model'}
        open={modalVisible}
        onCancel={() => setModalVisible(false)}
        form={form}
        onFinish={handleSaveModel}
        width={900}
      >
        <Form.Item
          name="providerId"
          label="Provider"
          rules={[{ required: true, message: 'Please select a provider' }]}
        >
          <Select
            placeholder="Select provider"
            style={{ borderRadius: designTokens.borderRadius.medium }}
          >
            {providers.map(provider => (
              <Option key={provider.providerId} value={provider.providerId}>
                {provider.name}
              </Option>
            ))}
          </Select>
        </Form.Item>

        <div style={{
          display: 'grid',
          gridTemplateColumns: '1fr 1fr',
          gap: designTokens.spacing.md
        }}>
          <Form.Item
            name="name"
            label="Model Name"
            rules={[{ required: true, message: 'Please enter model name' }]}
          >
            <Input
              placeholder="e.g., gpt-4"
              style={{ borderRadius: designTokens.borderRadius.medium }}
            />
          </Form.Item>

          <Form.Item
            name="displayName"
            label="Display Name"
            rules={[{ required: true, message: 'Please enter display name' }]}
          >
            <Input
              placeholder="e.g., GPT-4 for SQL Generation"
              style={{ borderRadius: designTokens.borderRadius.medium }}
            />
          </Form.Item>
        </div>

        <Form.Item
          name="useCase"
          label="Use Case"
          rules={[{ required: true, message: 'Please select use case' }]}
        >
          <Select
            placeholder="Select use case"
            style={{ borderRadius: designTokens.borderRadius.medium }}
          >
            <Option value="SQL">SQL Generation</Option>
            <Option value="Insights">Insights Generation</Option>
            <Option value="General">General Purpose</Option>
          </Select>
        </Form.Item>

        <div style={{
          display: 'grid',
          gridTemplateColumns: 'repeat(3, 1fr)',
          gap: designTokens.spacing.md
        }}>
          <Form.Item
            name="temperature"
            label="Temperature"
            rules={[{ required: true, message: 'Please enter temperature' }]}
          >
            <InputNumber
              min={0}
              max={2}
              step={0.1}
              style={{
                width: '100%',
                borderRadius: designTokens.borderRadius.medium
              }}
            />
          </Form.Item>

          <Form.Item
            name="maxTokens"
            label="Max Tokens"
            rules={[{ required: true, message: 'Please enter max tokens' }]}
          >
            <InputNumber
              min={1}
              max={8192}
              style={{
                width: '100%',
                borderRadius: designTokens.borderRadius.medium
              }}
            />
          </Form.Item>

          <Form.Item
            name="topP"
            label="Top P"
            rules={[{ required: true, message: 'Please enter top P' }]}
          >
            <InputNumber
              min={0}
              max={1}
              step={0.1}
              style={{
                width: '100%',
                borderRadius: designTokens.borderRadius.medium
              }}
            />
          </Form.Item>

          <Form.Item
            name="costPerToken"
            label="Cost Per Token"
            rules={[{ required: true, message: 'Please enter cost per token' }]}
          >
            <InputNumber
              min={0}
              step={0.000001}
              style={{
                width: '100%',
                borderRadius: designTokens.borderRadius.medium
              }}
            />
          </Form.Item>

          <Form.Item
            name="frequencyPenalty"
            label="Frequency Penalty"
          >
            <InputNumber
              min={-2}
              max={2}
              step={0.1}
              style={{
                width: '100%',
                borderRadius: designTokens.borderRadius.medium
              }}
            />
          </Form.Item>

          <Form.Item
            name="presencePenalty"
            label="Presence Penalty"
          >
            <InputNumber
              min={-2}
              max={2}
              step={0.1}
              style={{
                width: '100%',
                borderRadius: designTokens.borderRadius.medium
              }}
            />
          </Form.Item>
        </div>

        <Form.Item
          name="capabilities"
          label="Capabilities (JSON)"
        >
          <Input.TextArea
            rows={4}
            placeholder='{"reasoning": "high", "speed": "medium", "maxContextLength": 8192}'
            style={{
              borderRadius: designTokens.borderRadius.medium,
              fontFamily: designTokens.typography.fontFamily.mono,
              fontSize: designTokens.typography.fontSize.sm
            }}
          />
        </Form.Item>

        <Form.Item
          name="isEnabled"
          valuePropName="checked"
        >
          <div style={{
            display: 'flex',
            alignItems: 'center',
            gap: designTokens.spacing.sm
          }}>
            <Switch checkedChildren="Enabled" unCheckedChildren="Disabled" />
            <span style={{ fontSize: designTokens.typography.fontSize.sm }}>
              Enable Model
            </span>
          </div>
        </Form.Item>
      </LLMFormModal>
    </div>
  );
};
