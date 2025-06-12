import React, { useState, useEffect } from 'react';
import {
  Card,
  Table,
  Button,
  Modal,
  Form,
  Input,
  Select,
  Space,
  message,
  Tag,
  Typography,
  Row,
  Col,
  Switch,
  InputNumber,
  Alert,
  Tabs,
  Divider,
  Statistic
} from 'antd';
import {
  EditOutlined,
  SettingOutlined,
  RobotOutlined,
  ThunderboltOutlined,
  SafetyOutlined,
  ExperimentOutlined,
  PlayCircleOutlined
} from '@ant-design/icons';
import { tuningApi, AITuningSetting } from '../../services/tuningApi';

const { Text, Title } = Typography;
const { TextArea } = Input;
const { Option } = Select;
const { TabPane } = Tabs;

interface AIConfigurationManagerProps {
  onDataChange?: () => void;
}

const CATEGORIES = [
  'Prompts',
  'Business Rules',
  'Query Generation',
  'Performance',
  'AI Behavior',
  'Validation',
  'Caching',
  'Other'
];

const DATA_TYPES = [
  'string',
  'int',
  'bool',
  'json',
  'float'
];

export const AIConfigurationManager: React.FC<AIConfigurationManagerProps> = ({ onDataChange }) => {
  const [settings, setSettings] = useState<AITuningSetting[]>([]);
  const [loading, setLoading] = useState(false);
  const [modalVisible, setModalVisible] = useState(false);
  const [editingSetting, setEditingSetting] = useState<AITuningSetting | null>(null);
  const [selectedCategory, setSelectedCategory] = useState<string>('');
  const [activeTab, setActiveTab] = useState('quick-controls');
  const [form] = Form.useForm();

  useEffect(() => {
    loadSettings();
  }, []);

  const loadSettings = async () => {
    try {
      setLoading(true);
      const data = await tuningApi.getAISettings();
      setSettings(data);
    } catch (error) {
      message.error('Failed to load AI settings');
      console.error('Error loading settings:', error);
    } finally {
      setLoading(false);
    }
  };

  const handleEdit = (setting: AITuningSetting) => {
    setEditingSetting(setting);

    // Parse value based on data type
    let parsedValue: any = setting.settingValue;
    if (setting.dataType === 'bool') {
      parsedValue = setting.settingValue === 'true';
    } else if (setting.dataType === 'int' || setting.dataType === 'float') {
      parsedValue = Number(setting.settingValue);
    }

    form.setFieldsValue({
      settingKey: setting.settingKey,
      settingValue: parsedValue,
      description: setting.description,
      category: setting.category,
      dataType: setting.dataType
    });
    setModalVisible(true);
  };

  const handleSubmit = async (values: any) => {
    if (!editingSetting) return;

    try {
      // Convert value to string based on data type
      let stringValue = String(values.settingValue);
      if (values.dataType === 'bool') {
        stringValue = values.settingValue ? 'true' : 'false';
      }

      const request: AITuningSetting = {
        ...editingSetting,
        settingValue: stringValue,
        description: values.description,
        category: values.category,
        dataType: values.dataType
      };

      await tuningApi.updateAISetting(editingSetting.id, request);
      message.success('Setting updated successfully');

      setModalVisible(false);
      loadSettings();
      onDataChange?.();
    } catch (error) {
      message.error('Failed to save setting');
      console.error('Error saving setting:', error);
    }
  };

  const updateQuickSetting = async (settingKey: string, value: boolean) => {
    const setting = settings.find(s => s.settingKey === settingKey);
    if (setting) {
      try {
        await tuningApi.updateAISetting(setting.id, {
          ...setting,
          settingValue: value ? 'true' : 'false'
        });
        message.success(`${settingKey} ${value ? 'enabled' : 'disabled'}`);
        loadSettings();
        onDataChange?.();
      } catch (error) {
        message.error(`Failed to update ${settingKey}`);
      }
    }
  };

  const getCategoryColor = (category: string) => {
    const colors: Record<string, string> = {
      'Prompts': 'blue',
      'Business Rules': 'green',
      'Query Generation': 'purple',
      'Performance': 'orange',
      'AI Behavior': 'cyan',
      'Validation': 'magenta',
      'Caching': 'red',
      'Other': 'default'
    };
    return colors[category] || 'default';
  };

  const getDataTypeColor = (dataType: string) => {
    const colors: Record<string, string> = {
      'string': 'blue',
      'int': 'green',
      'bool': 'orange',
      'json': 'purple',
      'float': 'cyan'
    };
    return colors[dataType] || 'default';
  };

  const renderValueInput = (dataType: string) => {
    switch (dataType) {
      case 'bool':
        return (
          <Form.Item
            name="settingValue"
            label="Value"
            valuePropName="checked"
          >
            <Switch />
          </Form.Item>
        );
      case 'int':
        return (
          <Form.Item
            name="settingValue"
            label="Value"
            rules={[{ required: true, message: 'Please enter a value' }]}
          >
            <InputNumber style={{ width: '100%' }} />
          </Form.Item>
        );
      case 'float':
        return (
          <Form.Item
            name="settingValue"
            label="Value"
            rules={[{ required: true, message: 'Please enter a value' }]}
          >
            <InputNumber step={0.1} style={{ width: '100%' }} />
          </Form.Item>
        );
      case 'json':
        return (
          <Form.Item
            name="settingValue"
            label="Value (JSON)"
            rules={[{ required: true, message: 'Please enter a JSON value' }]}
          >
            <TextArea
              rows={4}
              placeholder='{"key": "value"}'
              style={{ fontFamily: 'monospace' }}
            />
          </Form.Item>
        );
      default:
        return (
          <Form.Item
            name="settingValue"
            label="Value"
            rules={[{ required: true, message: 'Please enter a value' }]}
          >
            <Input />
          </Form.Item>
        );
    }
  };

  const filteredSettings = selectedCategory
    ? settings.filter(setting => setting.category === selectedCategory)
    : settings;

  const columns = [
    {
      title: 'Setting Key',
      dataIndex: 'settingKey',
      key: 'settingKey',
      render: (text: string, record: AITuningSetting) => (
        <Space direction="vertical" size="small">
          <Text strong style={{ fontSize: '13px' }}>{text}</Text>
          <Space>
            <Tag color={getCategoryColor(record.category)}>
              {record.category}
            </Tag>
            <Tag color={getDataTypeColor(record.dataType)}>
              {record.dataType}
            </Tag>
          </Space>
        </Space>
      ),
    },
    {
      title: 'Current Value',
      dataIndex: 'settingValue',
      key: 'settingValue',
      render: (value: string, record: AITuningSetting) => {
        if (record.dataType === 'bool') {
          return <Switch checked={value === 'true'} disabled />;
        }
        if (record.dataType === 'json') {
          return (
            <div style={{
              fontFamily: 'monospace',
              fontSize: '11px',
              maxWidth: '200px',
              overflow: 'hidden',
              textOverflow: 'ellipsis'
            }}>
              {value}
            </div>
          );
        }
        return <Text code>{value}</Text>;
      },
    },
    {
      title: 'Description',
      dataIndex: 'description',
      key: 'description',
      ellipsis: true,
      render: (text: string) => (
        <Text style={{ fontSize: '12px' }}>{text}</Text>
      ),
    },
    {
      title: 'Actions',
      key: 'actions',
      width: 80,
      render: (_: any, record: AITuningSetting) => (
        <Button
          type="link"
          icon={<EditOutlined />}
          onClick={() => handleEdit(record)}
          size="small"
        />
      ),
    },
  ];

  return (
    <div>
      <Alert
        message="AI Configuration Center"
        description="Manage all AI behavior, model settings, and system parameters from this centralized location. Changes take effect immediately."
        type="info"
        showIcon
        style={{ marginBottom: 24 }}
      />

      <Tabs activeKey={activeTab} onChange={setActiveTab}>
        <TabPane
          tab={
            <span>
              <ThunderboltOutlined />
              Quick Controls
            </span>
          }
          key="quick-controls"
        >
          <Row gutter={[24, 24]}>
            <Col xs={24} lg={8}>
              <Card>
                <Title level={5}>
                  <SafetyOutlined style={{ marginRight: 8 }} />
                  System Controls
                </Title>
                <Space direction="vertical" size="large" style={{ width: '100%' }}>
                  <div>
                    <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 8 }}>
                      <Text strong>Query Caching</Text>
                      <Switch
                        checked={settings.find(s => s.settingKey === 'EnableQueryCaching')?.settingValue === 'true'}
                        onChange={(checked) => updateQuickSetting('EnableQueryCaching', checked)}
                      />
                    </div>
                    <Text type="secondary" style={{ fontSize: '12px' }}>
                      Disable to test same query multiple times
                    </Text>
                  </div>
                  
                  <div>
                    <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 8 }}>
                      <Text strong>Prompt Logging</Text>
                      <Switch
                        checked={settings.find(s => s.settingKey === 'EnablePromptLogging')?.settingValue === 'true'}
                        onChange={(checked) => updateQuickSetting('EnablePromptLogging', checked)}
                      />
                    </div>
                    <Text type="secondary" style={{ fontSize: '12px' }}>
                      Enable detailed AI prompt logging
                    </Text>
                  </div>
                </Space>
              </Card>
            </Col>
            
            <Col xs={24} lg={8}>
              <Card>
                <Title level={5}>
                  <RobotOutlined style={{ marginRight: 8 }} />
                  AI Behavior
                </Title>
                <Space direction="vertical" size="large" style={{ width: '100%' }}>
                  <div>
                    <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 8 }}>
                      <Text strong>Debug Mode</Text>
                      <Switch disabled />
                    </div>
                    <Text type="secondary" style={{ fontSize: '12px' }}>
                      Show detailed error messages (Coming soon)
                    </Text>
                  </div>
                  
                  <div>
                    <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 8 }}>
                      <Text strong>Auto-Learning</Text>
                      <Switch disabled />
                    </div>
                    <Text type="secondary" style={{ fontSize: '12px' }}>
                      Automatically learn from user patterns (Coming soon)
                    </Text>
                  </div>
                </Space>
              </Card>
            </Col>
            
            <Col xs={24} lg={8}>
              <Card>
                <Title level={5}>
                  <ExperimentOutlined style={{ marginRight: 8 }} />
                  Performance Metrics
                </Title>
                <Row gutter={[16, 16]}>
                  <Col span={24}>
                    <Statistic
                      title="Active Settings"
                      value={settings.length}
                      suffix="configured"
                    />
                  </Col>
                  <Col span={24}>
                    <Statistic
                      title="Categories"
                      value={CATEGORIES.length}
                      suffix="available"
                    />
                  </Col>
                </Row>
              </Card>
            </Col>
          </Row>
        </TabPane>

        <TabPane
          tab={
            <span>
              <SettingOutlined />
              All Settings
            </span>
          }
          key="all-settings"
        >
          <Card
            title={
              <Space>
                <SettingOutlined />
                <span>Detailed AI Settings</span>
              </Space>
            }
            extra={
              <Select
                placeholder="Filter by category"
                style={{ width: 150 }}
                allowClear
                value={selectedCategory}
                onChange={setSelectedCategory}
              >
                {CATEGORIES.map(category => (
                  <Option key={category} value={category}>
                    {category}
                  </Option>
                ))}
              </Select>
            }
          >
            <Table
              columns={columns}
              dataSource={filteredSettings}
              rowKey="id"
              loading={loading}
              pagination={{
                pageSize: 15,
                showSizeChanger: true,
                showQuickJumper: true,
              }}
              size="small"
            />
          </Card>
        </TabPane>
      </Tabs>

      <Modal
        title="Edit AI Setting"
        open={modalVisible}
        onCancel={() => setModalVisible(false)}
        onOk={() => form.submit()}
        width={600}
        destroyOnClose
      >
        {editingSetting && (
          <div>
            <Alert
              message={`Editing: ${editingSetting.settingKey}`}
              description={editingSetting.description}
              type="info"
              showIcon
              style={{ marginBottom: 16 }}
            />

            <Form
              form={form}
              layout="vertical"
              onFinish={handleSubmit}
            >
              <Form.Item
                name="settingKey"
                label="Setting Key"
              >
                <Input disabled />
              </Form.Item>

              <Row gutter={16}>
                <Col span={12}>
                  <Form.Item
                    name="category"
                    label="Category"
                    rules={[{ required: true, message: 'Please select a category' }]}
                  >
                    <Select>
                      {CATEGORIES.map(category => (
                        <Option key={category} value={category}>
                          {category}
                        </Option>
                      ))}
                    </Select>
                  </Form.Item>
                </Col>
                <Col span={12}>
                  <Form.Item
                    name="dataType"
                    label="Data Type"
                    rules={[{ required: true, message: 'Please select a data type' }]}
                  >
                    <Select>
                      {DATA_TYPES.map(type => (
                        <Option key={type} value={type}>
                          {type}
                        </Option>
                      ))}
                    </Select>
                  </Form.Item>
                </Col>
              </Row>

              <Form.Item dependencies={['dataType']}>
                {({ getFieldValue }) => renderValueInput(getFieldValue('dataType'))}
              </Form.Item>

              <Form.Item
                name="description"
                label="Description"
              >
                <TextArea
                  rows={3}
                  placeholder="Describe what this setting controls..."
                />
              </Form.Item>
            </Form>
          </div>
        )}
      </Modal>
    </div>
  );
};
