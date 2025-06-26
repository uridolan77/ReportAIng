import React, { useState, useEffect } from 'react';
import {
  Card,
  Button,
  Modal,
  Form,
  Input,
  Select,
  Checkbox,
  Space,
  Table,
  Tag,
  Popconfirm,
  message,
  Tooltip,
  Divider,
  Alert,
  Row,
  Col,
  InputNumber,
  Switch,
  Tabs
} from 'antd';
import {
  SaveOutlined,
  LoadingOutlined,
  DeleteOutlined,
  EditOutlined,
  CopyOutlined,
  SettingOutlined,
  CheckCircleOutlined,
  ExclamationCircleOutlined
} from '@ant-design/icons';
import { useAIPipelineTestApi } from '../hooks/useAIPipelineTestApi';
import AdvancedParameterControls from './AdvancedParameterControls';
import {
  PipelineTestConfiguration,
  PipelineStep,
  PipelineTestParameters,
  ParameterValidationResult,
  formatPipelineStepName
} from '../types/aiPipelineTest';

const { Option } = Select;
const { TextArea } = Input;
const { TabPane } = Tabs;

interface PipelineTestConfigurationManagerProps {
  currentSteps: PipelineStep[];
  currentParameters: PipelineTestParameters;
  onLoadConfiguration: (config: PipelineTestConfiguration) => void;
  onParametersChange: (parameters: PipelineTestParameters) => void;
}

const PipelineTestConfigurationManager: React.FC<PipelineTestConfigurationManagerProps> = ({
  currentSteps,
  currentParameters,
  onLoadConfiguration,
  onParametersChange
}) => {
  const [form] = Form.useForm();
  const [configurations, setConfigurations] = useState<PipelineTestConfiguration[]>([]);
  const [isModalVisible, setIsModalVisible] = useState(false);
  const [editingConfig, setEditingConfig] = useState<PipelineTestConfiguration | null>(null);
  const [validationResult, setValidationResult] = useState<ParameterValidationResult | null>(null);
  const [isValidating, setIsValidating] = useState(false);

  const {
    saveTestConfiguration,
    getTestConfigurations,
    deleteTestConfiguration,
    validateTestConfiguration,
    loading
  } = useAIPipelineTestApi();

  useEffect(() => {
    loadConfigurations();
  }, []);

  const loadConfigurations = async () => {
    try {
      const configs = await getTestConfigurations();
      setConfigurations(configs);
    } catch (error) {
      console.error('Failed to load configurations:', error);
    }
  };

  const handleSaveConfiguration = async (values: any) => {
    try {
      const config = await saveTestConfiguration({
        name: values.name,
        description: values.description,
        steps: values.steps || currentSteps,
        parameters: values.parameters || currentParameters,
        category: values.category
      });

      setConfigurations(prev => [...prev, config]);
      setIsModalVisible(false);
      form.resetFields();
      message.success('Configuration saved successfully');
    } catch (error) {
      console.error('Failed to save configuration:', error);
    }
  };

  const handleLoadConfiguration = (config: PipelineTestConfiguration) => {
    onLoadConfiguration(config);
    message.success(`Loaded configuration: ${config.name}`);
  };

  const handleDeleteConfiguration = async (configId: string) => {
    try {
      await deleteTestConfiguration(configId);
      setConfigurations(prev => prev.filter(c => c.id !== configId));
      message.success('Configuration deleted successfully');
    } catch (error) {
      console.error('Failed to delete configuration:', error);
    }
  };

  const handleValidateParameters = async () => {
    setIsValidating(true);
    try {
      const result = await validateTestConfiguration(currentSteps, currentParameters);
      setValidationResult(result);
    } catch (error) {
      console.error('Failed to validate parameters:', error);
      message.error('Failed to validate parameters');
    } finally {
      setIsValidating(false);
    }
  };

  const renderParameterControls = () => (
    <Card title="Parameter Configuration" size="small" className="mb-4">
      <Tabs defaultActiveKey="advanced">
        <TabPane tab="Advanced Controls" key="advanced">
          <AdvancedParameterControls
            parameters={currentParameters}
            onParametersChange={onParametersChange}
            validationResult={validationResult}
            onValidate={handleValidateParameters}
            isValidating={isValidating}
          />
        </TabPane>

        <TabPane tab="Quick Setup" key="quick">
          <Row gutter={[16, 16]}>
            <Col span={12}>
              <div className="space-y-3">
                <div>
                  <label className="block text-sm font-medium mb-1">Max Tokens</label>
                  <InputNumber
                    value={currentParameters.maxTokens}
                    onChange={(value) => onParametersChange({ ...currentParameters, maxTokens: value || undefined })}
                    min={100}
                    max={8000}
                    step={100}
                    className="w-full"
                    placeholder="4000"
                  />
                </div>

                <div>
                  <label className="block text-sm font-medium mb-1">Confidence Threshold</label>
                  <InputNumber
                    value={currentParameters.confidenceThreshold}
                    onChange={(value) => onParametersChange({ ...currentParameters, confidenceThreshold: value || undefined })}
                    min={0}
                    max={1}
                    step={0.1}
                    className="w-full"
                    placeholder="0.7"
                  />
                </div>
              </div>
            </Col>

            <Col span={12}>
              <div className="space-y-3">
                <div>
                  <label className="block text-sm font-medium mb-1">Max Tables</label>
                  <InputNumber
                    value={currentParameters.maxTables}
                    onChange={(value) => onParametersChange({ ...currentParameters, maxTables: value || undefined })}
                    min={1}
                    max={50}
                    step={1}
                    className="w-full"
                    placeholder="10"
                  />
                </div>

                <div className="space-y-2">
                  <div className="flex items-center space-x-2">
                    <Switch
                      checked={currentParameters.includeExamples}
                      onChange={(checked) => onParametersChange({ ...currentParameters, includeExamples: checked })}
                      size="small"
                    />
                    <span className="text-sm">Include Examples</span>
                  </div>

                  <div className="flex items-center space-x-2 p-2 border border-red-200 rounded bg-red-50">
                    <Switch
                      checked={currentParameters.enableAIGeneration}
                      onChange={(checked) => onParametersChange({ ...currentParameters, enableAIGeneration: checked })}
                      size="small"
                    />
                    <div>
                      <span className="text-sm text-red-600 font-medium">Enable AI Generation</span>
                      <div className="text-xs text-red-500">Required for AI Generation step - incurs costs!</div>
                    </div>
                  </div>

                  <div className="flex items-center space-x-2">
                    <Switch
                      checked={currentParameters.enableExecution}
                      onChange={(checked) => onParametersChange({ ...currentParameters, enableExecution: checked })}
                      size="small"
                    />
                    <span className="text-sm text-orange-600">Enable SQL Execution</span>
                  </div>
                </div>
              </div>
            </Col>
          </Row>

          <Divider />

          <div className="flex justify-between items-center">
            <Button
              icon={<CheckCircleOutlined />}
              onClick={handleValidateParameters}
              loading={isValidating}
            >
              Validate Parameters
            </Button>

            <Button
              type="primary"
              icon={<SaveOutlined />}
              onClick={() => setIsModalVisible(true)}
            >
              Save Configuration
            </Button>
          </div>
        </TabPane>
      </Tabs>
    </Card>
  );

  const renderConfigurationTable = () => {
    const columns = [
      {
        title: 'Name',
        dataIndex: 'name',
        key: 'name',
        render: (text: string, record: PipelineTestConfiguration) => (
          <div>
            <div className="font-medium">{text}</div>
            <div className="text-sm text-gray-500">{record.description}</div>
          </div>
        )
      },
      {
        title: 'Category',
        dataIndex: 'category',
        key: 'category',
        render: (category: string) => {
          const colors = {
            preset: 'blue',
            debugging: 'orange',
            performance: 'green',
            custom: 'purple'
          };
          return <Tag color={colors[category as keyof typeof colors] || 'default'}>{category}</Tag>;
        }
      },
      {
        title: 'Steps',
        dataIndex: 'steps',
        key: 'steps',
        render: (steps: PipelineStep[]) => (
          <div className="space-x-1">
            {steps.slice(0, 3).map(step => (
              <Tag key={step} size="small">{formatPipelineStepName(step)}</Tag>
            ))}
            {steps.length > 3 && <Tag size="small">+{steps.length - 3} more</Tag>}
          </div>
        )
      },
      {
        title: 'Saved',
        dataIndex: 'savedAt',
        key: 'savedAt',
        render: (date: string) => new Date(date).toLocaleDateString()
      },
      {
        title: 'Actions',
        key: 'actions',
        render: (_, record: PipelineTestConfiguration) => (
          <Space>
            <Tooltip title="Load Configuration">
              <Button
                type="text"
                size="small"
                icon={<EditOutlined />}
                onClick={() => handleLoadConfiguration(record)}
              />
            </Tooltip>
            <Tooltip title="Copy Configuration">
              <Button
                type="text"
                size="small"
                icon={<CopyOutlined />}
                onClick={() => {
                  setEditingConfig({ ...record, id: '', name: `${record.name} (Copy)` });
                  setIsModalVisible(true);
                }}
              />
            </Tooltip>
            <Popconfirm
              title="Are you sure you want to delete this configuration?"
              onConfirm={() => handleDeleteConfiguration(record.id)}
              okText="Yes"
              cancelText="No"
            >
              <Tooltip title="Delete Configuration">
                <Button
                  type="text"
                  size="small"
                  icon={<DeleteOutlined />}
                  danger
                />
              </Tooltip>
            </Popconfirm>
          </Space>
        )
      }
    ];

    return (
      <Card title="Saved Configurations" className="mb-4">
        <Table
          columns={columns}
          dataSource={configurations}
          rowKey="id"
          size="small"
          pagination={{ pageSize: 5 }}
          loading={loading}
        />
      </Card>
    );
  };

  return (
    <div>
      {renderParameterControls()}
      {renderConfigurationTable()}

      <Modal
        title={editingConfig ? "Edit Configuration" : "Save Configuration"}
        open={isModalVisible}
        onCancel={() => {
          setIsModalVisible(false);
          setEditingConfig(null);
          form.resetFields();
        }}
        footer={null}
        width={600}
      >
        <Form
          form={form}
          layout="vertical"
          onFinish={handleSaveConfiguration}
          initialValues={editingConfig || {
            steps: currentSteps,
            parameters: currentParameters,
            category: 'custom'
          }}
        >
          <Form.Item
            name="name"
            label="Configuration Name"
            rules={[{ required: true, message: 'Please enter a configuration name' }]}
          >
            <Input placeholder="Enter configuration name" />
          </Form.Item>

          <Form.Item
            name="description"
            label="Description"
            rules={[{ required: true, message: 'Please enter a description' }]}
          >
            <TextArea rows={3} placeholder="Describe this configuration" />
          </Form.Item>

          <Form.Item
            name="category"
            label="Category"
          >
            <Select>
              <Option value="custom">Custom</Option>
              <Option value="debugging">Debugging</Option>
              <Option value="performance">Performance</Option>
              <Option value="preset">Preset</Option>
            </Select>
          </Form.Item>

          <div className="flex justify-end space-x-2">
            <Button onClick={() => setIsModalVisible(false)}>
              Cancel
            </Button>
            <Button type="primary" htmlType="submit" loading={loading}>
              Save Configuration
            </Button>
          </div>
        </Form>
      </Modal>
    </div>
  );
};

export default PipelineTestConfigurationManager;
