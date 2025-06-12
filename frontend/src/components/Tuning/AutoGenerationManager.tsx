/**
 * Auto Generation Manager Components
 * AI-powered content generation and automation features
 */

import React, { useState } from 'react';
import {
  Card,
  Typography,
  Button,
  Space,
  Row,
  Col,
  Progress,
  Alert,
  List,
  Tag,
  Statistic,
  Switch,
  Select,
  Form,
  Input,
  Modal,
  message,
  Tabs,
  Divider
} from 'antd';
import {
  PlayCircleOutlined,
  PauseCircleOutlined,
  ReloadOutlined,
  SettingOutlined,
  BulbOutlined,
  DatabaseOutlined,
  FileTextOutlined,
  CheckCircleOutlined,
  ClockCircleOutlined,
  ExclamationCircleOutlined,
  RobotOutlined
} from '@ant-design/icons';

const { Title, Text, Paragraph } = Typography;
const { Option } = Select;
const { TabPane } = Tabs;

interface GenerationTask {
  id: string;
  type: 'glossary' | 'patterns' | 'tables' | 'prompts';
  name: string;
  status: 'pending' | 'running' | 'completed' | 'failed';
  progress: number;
  startTime?: string;
  endTime?: string;
  itemsGenerated?: number;
  totalItems?: number;
}

export const AutoGenerationManager: React.FC = () => {
  const [isGenerating, setIsGenerating] = useState(false);
  const [modalVisible, setModalVisible] = useState(false);
  const [selectedType, setSelectedType] = useState<string>('');
  const [form] = Form.useForm();

  // Mock data for demonstration
  const [tasks, setTasks] = useState<GenerationTask[]>([
    {
      id: '1',
      type: 'glossary',
      name: 'Business Terms Auto-Generation',
      status: 'completed',
      progress: 100,
      startTime: '2024-01-15 10:30:00',
      endTime: '2024-01-15 10:45:00',
      itemsGenerated: 45,
      totalItems: 45
    },
    {
      id: '2',
      type: 'patterns',
      name: 'Query Pattern Analysis',
      status: 'running',
      progress: 67,
      startTime: '2024-01-15 11:00:00',
      itemsGenerated: 23,
      totalItems: 34
    }
  ]);

  const generationTypes = [
    {
      key: 'glossary',
      title: 'Business Glossary Terms',
      description: 'Automatically generate business terms and definitions from database schema and query patterns',
      icon: <BulbOutlined />,
      estimatedTime: '5-10 minutes'
    },
    {
      key: 'patterns',
      title: 'Query Patterns',
      description: 'Analyze historical queries to identify common patterns and optimize AI responses',
      icon: <DatabaseOutlined />,
      estimatedTime: '10-15 minutes'
    },
    {
      key: 'tables',
      title: 'Table Descriptions',
      description: 'Generate comprehensive descriptions for database tables and columns',
      icon: <FileTextOutlined />,
      estimatedTime: '3-5 minutes'
    },
    {
      key: 'prompts',
      title: 'Prompt Templates',
      description: 'Create optimized prompt templates based on successful query patterns',
      icon: <RobotOutlined />,
      estimatedTime: '8-12 minutes'
    }
  ];

  const handleStartGeneration = async (values: any) => {
    try {
      setIsGenerating(true);

      const newTask: GenerationTask = {
        id: Date.now().toString(),
        type: values.type,
        name: values.name || `${generationTypes.find(t => t.key === values.type)?.title} Generation`,
        status: 'running',
        progress: 0,
        startTime: new Date().toLocaleString(),
        totalItems: Math.floor(Math.random() * 50) + 10
      };

      setTasks(prev => [newTask, ...prev]);
      setModalVisible(false);
      form.resetFields();

      message.success('Auto-generation task started successfully');

      // Simulate progress
      const progressInterval = setInterval(() => {
        setTasks(prev => prev.map(task => {
          if (task.id === newTask.id && task.status === 'running') {
            const newProgress = Math.min(task.progress + Math.random() * 15, 100);
            const updatedTask = {
              ...task,
              progress: newProgress,
              itemsGenerated: Math.floor((newProgress / 100) * (task.totalItems || 0))
            };

            if (newProgress >= 100) {
              updatedTask.status = 'completed';
              updatedTask.endTime = new Date().toLocaleString();
              clearInterval(progressInterval);
              setIsGenerating(false);
              message.success(`${task.name} completed successfully!`);
            }

            return updatedTask;
          }
          return task;
        }));
      }, 2000);

    } catch (error) {
      message.error('Failed to start auto-generation task');
      setIsGenerating(false);
    }
  };

  const getStatusColor = (status: string) => {
    switch (status) {
      case 'completed': return 'success';
      case 'running': return 'processing';
      case 'failed': return 'error';
      default: return 'default';
    }
  };

  const getStatusIcon = (status: string) => {
    switch (status) {
      case 'completed': return <CheckCircleOutlined />;
      case 'running': return <ClockCircleOutlined />;
      case 'failed': return <ExclamationCircleOutlined />;
      default: return <ClockCircleOutlined />;
    }
  };

  return (
    <div>
      <Alert
        message="AI-Powered Content Generation"
        description="Automatically generate business glossary terms, query patterns, table descriptions, and prompt templates using advanced AI analysis."
        type="info"
        showIcon
        style={{ marginBottom: 24 }}
      />

      <Row gutter={[24, 24]}>
        <Col xs={24} lg={16}>
          <Card
            title={
              <Space>
                <PlayCircleOutlined />
                <span>Auto-Generation Dashboard</span>
              </Space>
            }
            extra={
              <Button
                type="primary"
                icon={<PlayCircleOutlined />}
                onClick={() => setModalVisible(true)}
                disabled={isGenerating}
              >
                Start New Generation
              </Button>
            }
          >
            <Row gutter={[16, 16]}>
              {generationTypes.map(type => (
                <Col xs={24} sm={12} key={type.key}>
                  <Card size="small" hoverable>
                    <Space direction="vertical" style={{ width: '100%' }}>
                      <Space>
                        {type.icon}
                        <Text strong>{type.title}</Text>
                      </Space>
                      <Text type="secondary" style={{ fontSize: '12px' }}>
                        {type.description}
                      </Text>
                      <Text type="secondary" style={{ fontSize: '11px' }}>
                        ⏱️ Est. time: {type.estimatedTime}
                      </Text>
                    </Space>
                  </Card>
                </Col>
              ))}
            </Row>
          </Card>
        </Col>

        <Col xs={24} lg={8}>
          <Card title="Generation Statistics">
            <Row gutter={[16, 16]}>
              <Col span={24}>
                <Statistic
                  title="Total Tasks"
                  value={tasks.length}
                  prefix={<RobotOutlined />}
                />
              </Col>
              <Col span={24}>
                <Statistic
                  title="Completed"
                  value={tasks.filter(t => t.status === 'completed').length}
                  prefix={<CheckCircleOutlined />}
                  valueStyle={{ color: '#52c41a' }}
                />
              </Col>
              <Col span={24}>
                <Statistic
                  title="Running"
                  value={tasks.filter(t => t.status === 'running').length}
                  prefix={<ClockCircleOutlined />}
                  valueStyle={{ color: '#1890ff' }}
                />
              </Col>
            </Row>
          </Card>
        </Col>
      </Row>

      <Row gutter={[24, 24]} style={{ marginTop: 24 }}>
        <Col xs={24} lg={16}>
          <Card
            title="Generation Task History"
            extra={
              <Button icon={<ReloadOutlined />} onClick={() => message.info('Refreshing task history...')}>
                Refresh
              </Button>
            }
          >
            <List
              dataSource={tasks}
              renderItem={(task) => (
                <List.Item>
                  <List.Item.Meta
                    avatar={getStatusIcon(task.status)}
                    title={
                      <Space>
                        <Text strong>{task.name}</Text>
                        <Tag color={getStatusColor(task.status)}>{task.status.toUpperCase()}</Tag>
                      </Space>
                    }
                    description={
                      <Space direction="vertical" size="small">
                        <Text type="secondary">
                          Started: {task.startTime}
                          {task.endTime && ` • Completed: ${task.endTime}`}
                        </Text>
                        {task.status === 'running' && (
                          <Progress
                            percent={Math.round(task.progress)}
                            size="small"
                            status="active"
                          />
                        )}
                        {task.itemsGenerated !== undefined && (
                          <Text type="secondary">
                            Generated: {task.itemsGenerated}/{task.totalItems} items
                          </Text>
                        )}
                      </Space>
                    }
                  />
                </List.Item>
              )}
            />
          </Card>
        </Col>

        <Col xs={24} lg={8}>
          <Card title="Generation Settings">
            <Space direction="vertical" style={{ width: '100%' }}>
              <div>
                <Text strong>Auto-approve content</Text>
                <br />
                <Switch defaultChecked={false} />
                <br />
                <Text type="secondary" style={{ fontSize: '12px' }}>
                  Auto-approve generated content
                </Text>
              </div>

              <Divider />

              <div>
                <Text strong>Scheduled generation</Text>
                <br />
                <Switch defaultChecked={true} />
                <br />
                <Text type="secondary" style={{ fontSize: '12px' }}>
                  Run tasks on schedule
                </Text>
              </div>

              <Divider />

              <div>
                <Text strong>Confidence threshold</Text>
                <br />
                <Select defaultValue="high" style={{ width: '100%' }}>
                  <Option value="low">Low (60%)</Option>
                  <Option value="medium">Medium (75%)</Option>
                  <Option value="high">High (85%)</Option>
                </Select>
              </div>
            </Space>
          </Card>
        </Col>
      </Row>

      {/* Start Generation Modal */}
      <Modal
        title="Start Auto-Generation Task"
        open={modalVisible}
        onCancel={() => setModalVisible(false)}
        onOk={() => form.submit()}
        width={600}
      >
        <Form
          form={form}
          layout="vertical"
          onFinish={handleStartGeneration}
        >
          <Form.Item
            name="type"
            label="Generation Type"
            rules={[{ required: true, message: 'Please select a generation type' }]}
          >
            <Select placeholder="Select what to generate">
              {generationTypes.map(type => (
                <Option key={type.key} value={type.key}>
                  <Space>
                    {type.icon}
                    {type.title}
                  </Space>
                </Option>
              ))}
            </Select>
          </Form.Item>

          <Form.Item
            name="name"
            label="Task Name (Optional)"
          >
            <Input placeholder="Custom name for this generation task" />
          </Form.Item>

          <Alert
            message="Generation Process"
            description="The AI will analyze your existing data and generate relevant content. This process may take several minutes depending on the amount of data."
            type="info"
            showIcon
          />
        </Form>
      </Modal>
    </div>
  );
};

// Legacy placeholder components for backward compatibility
export const AutoGenerationProgress: React.FC = () => (
  <Card title="Auto Generation Progress">
    <Text>This functionality is now integrated into the main AutoGenerationManager component.</Text>
  </Card>
);

export const AutoGenerationResults: React.FC = () => (
  <Card title="Auto Generation Results">
    <Text>This functionality is now integrated into the main AutoGenerationManager component.</Text>
  </Card>
);

export const ProcessingLogViewer: React.FC = () => (
  <Card title="Processing Log Viewer">
    <Text>This functionality is now integrated into the main AutoGenerationManager component.</Text>
  </Card>
);
