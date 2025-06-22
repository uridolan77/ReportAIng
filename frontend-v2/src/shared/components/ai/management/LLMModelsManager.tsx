import React, { useState } from 'react'
import { 
  Card, 
  Table, 
  Tag, 
  Button, 
  Space, 
  Typography, 
  Modal,
  Form,
  Input,
  Select,
  InputNumber,
  Tooltip,
  Row,
  Col,
  Switch
} from 'antd'
import {
  PlusOutlined,
  EditOutlined,
  DeleteOutlined,
  CheckCircleOutlined,
  RobotOutlined
} from '@ant-design/icons'
import { 
  useGetModelsQuery,
  useGetProvidersQuery,
  useSaveModelMutation,
  useDeleteModelMutation,
  useSetDefaultModelMutation,
  type LLMModelConfig,
  type LLMProviderConfig
} from '@shared/store/api/llmManagementApi'

const { Text } = Typography
const { confirm } = Modal

interface LLMModelsManagerProps {
  compact?: boolean
  showActions?: boolean
}

/**
 * LLMModelsManager - Component for managing LLM models within LLM Management dashboard
 * 
 * Features:
 * - Model CRUD operations
 * - Provider-specific model configuration
 * - Default model assignment
 * - Model parameter tuning
 * - Use case mapping
 */
export const LLMModelsManager: React.FC<LLMModelsManagerProps> = ({
  compact = false,
  showActions = true
}) => {
  // API hooks
  const { data: models, isLoading: modelsLoading, refetch: refetchModels } = useGetModelsQuery({})
  const { data: providers, isLoading: providersLoading } = useGetProvidersQuery()
  const [saveModel, { isLoading: saveLoading }] = useSaveModelMutation()
  const [deleteModel, { isLoading: deleteLoading }] = useDeleteModelMutation()
  const [setDefaultModel, { isLoading: setDefaultLoading }] = useSetDefaultModelMutation()

  // Local state
  const [selectedModel, setSelectedModel] = useState<LLMModelConfig | null>(null)
  const [modalVisible, setModalVisible] = useState(false)
  const [isEditing, setIsEditing] = useState(false)
  const [form] = Form.useForm()

  const isLoading = modelsLoading || providersLoading

  // Handle add new model
  const handleAddModel = () => {
    setSelectedModel(null)
    setIsEditing(false)
    form.resetFields()
    setModalVisible(true)
  }

  // Handle edit model
  const handleEditModel = (model: LLMModelConfig) => {
    setSelectedModel(model)
    setIsEditing(true)
    form.setFieldsValue(model)
    setModalVisible(true)
  }

  // Handle save model
  const handleSaveModel = async () => {
    try {
      const values = await form.validateFields()
      const modelData: LLMModelConfig = {
        ...values,
        modelId: isEditing ? selectedModel!.modelId : `${values.providerId}-${values.name.toLowerCase().replace(/\s+/g, '-')}`,
      }

      await saveModel(modelData).unwrap()
      setModalVisible(false)
      refetchModels()
    } catch (error) {
      console.error('Failed to save model:', error)
    }
  }

  // Handle delete model
  const handleDeleteModel = (model: LLMModelConfig) => {
    confirm({
      title: 'Delete Model',
      content: `Are you sure you want to delete ${model.name}? This action cannot be undone.`,
      onOk: async () => {
        try {
          await deleteModel(model.modelId).unwrap()
          refetchModels()
        } catch (error) {
          console.error('Failed to delete model:', error)
        }
      }
    })
  }

  // Handle set default model
  const handleSetDefault = async (model: LLMModelConfig, useCase: string) => {
    try {
      await setDefaultModel({ modelId: model.modelId, useCase }).unwrap()
      refetchModels()
    } catch (error) {
      console.error('Failed to set default model:', error)
    }
  }

  // Table columns
  const columns = [
    {
      title: 'Model',
      dataIndex: 'name',
      key: 'name',
      render: (text: string, record: LLMModelConfig) => (
        <div>
          <div style={{ fontWeight: 'bold' }}>{text || 'Unknown Model'}</div>
          <Text type="secondary" style={{ fontSize: '12px' }}>
            {record.displayName || record.modelId}
          </Text>
        </div>
      )
    },
    {
      title: 'Provider',
      dataIndex: 'providerId',
      key: 'providerId',
      render: (providerId: string) => {
        const provider = providers?.find(p => p.providerId === providerId)
        return (
          <Tag color="blue">
            {provider?.name || providerId}
          </Tag>
        )
      }
    },
    {
      title: 'Use Case',
      dataIndex: 'useCase',
      key: 'useCase',
      render: (useCase: string) => (
        <Tag color="green">{useCase || 'General'}</Tag>
      )
    },
    ...(!compact ? [{
      title: 'Configuration',
      key: 'config',
      render: (_: any, record: LLMModelConfig) => (
        <Space direction="vertical" size="small">
          <div>
            <Text type="secondary">Temp: </Text>
            <Text>{record.temperature || 0.7}</Text>
          </div>
          <div>
            <Text type="secondary">Max Tokens: </Text>
            <Text>{record.maxTokens || 4096}</Text>
          </div>
        </Space>
      )
    }] : []),
    {
      title: 'Cost',
      dataIndex: 'costPerToken',
      key: 'costPerToken',
      render: (cost: number) => (
        <Text>${cost?.toFixed(6) || '0.000000'}/token</Text>
      )
    },
    {
      title: 'Status',
      dataIndex: 'isEnabled',
      key: 'isEnabled',
      render: (isEnabled: boolean) => (
        <Tag color={isEnabled ? 'green' : 'red'}>
          {isEnabled ? 'ENABLED' : 'DISABLED'}
        </Tag>
      )
    },
    ...(showActions ? [{
      title: 'Actions',
      key: 'actions',
      render: (_: any, record: LLMModelConfig) => (
        <Space>
          <Tooltip title="Edit">
            <Button 
              type="text" 
              icon={<EditOutlined />}
              onClick={() => handleEditModel(record)}
            />
          </Tooltip>
          <Tooltip title="Set as Default">
            <Button 
              type="text" 
              icon={<CheckCircleOutlined />}
              loading={setDefaultLoading}
              onClick={() => handleSetDefault(record, record.useCase || 'general')}
            />
          </Tooltip>
          <Tooltip title="Delete">
            <Button 
              type="text" 
              danger
              icon={<DeleteOutlined />}
              loading={deleteLoading}
              onClick={() => handleDeleteModel(record)}
            />
          </Tooltip>
        </Space>
      )
    }] : [])
  ]

  return (
    <div>
      <Card 
        title={
          <Space>
            <RobotOutlined />
            <span>Models Configuration</span>
          </Space>
        }
        extra={
          showActions && (
            <Button
              type="primary"
              icon={<PlusOutlined />}
              onClick={handleAddModel}
            >
              Add Model
            </Button>
          )
        }
      >
        <Table
          columns={columns}
          dataSource={models}
          rowKey="modelId"
          loading={isLoading}
          pagination={compact ? false : { pageSize: 10 }}
          size={compact ? 'small' : 'default'}
          locale={{
            emptyText: isLoading ? 'Loading models...' : 'No models configured. Click "Add Model" to get started.'
          }}
        />
      </Card>

      {/* Add/Edit Model Modal */}
      <Modal
        title={isEditing ? `Edit ${selectedModel?.name}` : 'Add New Model'}
        open={modalVisible}
        onOk={handleSaveModel}
        onCancel={() => setModalVisible(false)}
        confirmLoading={saveLoading}
        width={600}
      >
        <Form form={form} layout="vertical">
          <Row gutter={16}>
            <Col span={12}>
              <Form.Item
                name="name"
                label="Model Name"
                rules={[{ required: true, message: 'Please enter model name' }]}
              >
                <Input placeholder="e.g., GPT-4" />
              </Form.Item>
            </Col>
            <Col span={12}>
              <Form.Item
                name="displayName"
                label="Display Name"
              >
                <Input placeholder="e.g., GPT-4 Turbo" />
              </Form.Item>
            </Col>
          </Row>
          <Row gutter={16}>
            <Col span={12}>
              <Form.Item
                name="providerId"
                label="Provider"
                rules={[{ required: true, message: 'Please select provider' }]}
              >
                <Select placeholder="Select provider">
                  {providers?.map(provider => (
                    <Select.Option key={provider.providerId} value={provider.providerId}>
                      {provider.name}
                    </Select.Option>
                  ))}
                </Select>
              </Form.Item>
            </Col>
            <Col span={12}>
              <Form.Item
                name="useCase"
                label="Use Case"
              >
                <Select placeholder="Select use case">
                  <Select.Option value="general">General</Select.Option>
                  <Select.Option value="chat">Chat</Select.Option>
                  <Select.Option value="analysis">Analysis</Select.Option>
                  <Select.Option value="coding">Coding</Select.Option>
                  <Select.Option value="creative">Creative</Select.Option>
                </Select>
              </Form.Item>
            </Col>
          </Row>
          <Row gutter={16}>
            <Col span={8}>
              <Form.Item
                name="temperature"
                label="Temperature"
                rules={[{ required: true, type: 'number', min: 0, max: 2 }]}
              >
                <InputNumber step={0.1} style={{ width: '100%' }} />
              </Form.Item>
            </Col>
            <Col span={8}>
              <Form.Item
                name="maxTokens"
                label="Max Tokens"
                rules={[{ required: true, type: 'number', min: 1 }]}
              >
                <InputNumber style={{ width: '100%' }} />
              </Form.Item>
            </Col>
            <Col span={8}>
              <Form.Item
                name="costPerToken"
                label="Cost per Token"
                rules={[{ required: true, type: 'number', min: 0 }]}
              >
                <InputNumber step={0.000001} style={{ width: '100%' }} />
              </Form.Item>
            </Col>
          </Row>
          <Row gutter={16}>
            <Col span={8}>
              <Form.Item
                name="topP"
                label="Top P"
                rules={[{ type: 'number', min: 0, max: 1 }]}
              >
                <InputNumber step={0.1} style={{ width: '100%' }} />
              </Form.Item>
            </Col>
            <Col span={8}>
              <Form.Item
                name="frequencyPenalty"
                label="Frequency Penalty"
                rules={[{ type: 'number', min: -2, max: 2 }]}
              >
                <InputNumber step={0.1} style={{ width: '100%' }} />
              </Form.Item>
            </Col>
            <Col span={8}>
              <Form.Item
                name="presencePenalty"
                label="Presence Penalty"
                rules={[{ type: 'number', min: -2, max: 2 }]}
              >
                <InputNumber step={0.1} style={{ width: '100%' }} />
              </Form.Item>
            </Col>
          </Row>
          <Row>
            <Col span={24}>
              <Form.Item
                name="isEnabled"
                valuePropName="checked"
                initialValue={true}
              >
                <Switch /> Enable this model
              </Form.Item>
            </Col>
          </Row>
        </Form>
      </Modal>
    </div>
  )
}

export default LLMModelsManager
