/**
 * Design System Showcase
 * 
 * Comprehensive showcase of the standardized design system components,
 * demonstrating consistent styling and interactions across the application.
 */

import React, { useState } from 'react';
import {
  Card,
  Button,
  Input,
  Typography,
  Space,
  Row,
  Col,
  Divider,
  Tag,
  Badge,
  Alert,
  Progress,
  Switch,
  Slider,
  Rate,
  Avatar,
  Tooltip,
  Popover,
  Modal,
  message,
  notification,
} from 'antd';
import {
  UserOutlined,
  HeartOutlined,
  StarOutlined,
  SettingOutlined,
  BellOutlined,
  SearchOutlined,
  PlusOutlined,
  EditOutlined,
  DeleteOutlined,
  DownloadOutlined,
} from '@ant-design/icons';
import { useTheme } from '../contexts/ThemeContext';

const { Title, Text, Paragraph } = Typography;

const DesignSystemShowcase: React.FC = () => {
  const { isDarkMode, toggleTheme } = useTheme();
  const [modalVisible, setModalVisible] = useState(false);
  const [loading, setLoading] = useState(false);

  const showMessage = () => {
    message.success('This is a success message!');
  };

  const showNotification = () => {
    notification.info({
      message: 'Design System',
      description: 'This notification demonstrates our standardized styling.',
      placement: 'topRight',
    });
  };

  const handleAsyncAction = async () => {
    setLoading(true);
    await new Promise(resolve => setTimeout(resolve, 2000));
    setLoading(false);
    message.success('Action completed successfully!');
  };

  return (
    <div className="page-layout">
      <div className="container">
        <div className="page-header">
          <Title level={1}>Design System Showcase</Title>
          <Paragraph>
            Comprehensive demonstration of our standardized UI components and design tokens.
            This showcase ensures consistency across the entire ReportAIng application.
          </Paragraph>
          
          <Space>
            <Button type="primary" onClick={toggleTheme}>
              Switch to {isDarkMode ? 'Light' : 'Dark'} Mode
            </Button>
            <Button onClick={showMessage}>Show Message</Button>
            <Button onClick={showNotification}>Show Notification</Button>
          </Space>
        </div>

        <div className="page-content">
          {/* Typography Section */}
          <Card title="Typography System" className="mb-6">
            <Row gutter={[24, 24]}>
              <Col span={12}>
                <Title level={1}>Heading 1 - Main Title</Title>
                <Title level={2}>Heading 2 - Section Title</Title>
                <Title level={3}>Heading 3 - Subsection</Title>
                <Title level={4}>Heading 4 - Component Title</Title>
                <Title level={5}>Heading 5 - Small Title</Title>
                <Text>Regular body text with normal weight and standard line height.</Text>
                <br />
                <Text strong>Strong text for emphasis and important information.</Text>
                <br />
                <Text type="secondary">Secondary text for less important information.</Text>
                <br />
                <Text type="success">Success text for positive feedback.</Text>
                <br />
                <Text type="warning">Warning text for cautionary messages.</Text>
                <br />
                <Text type="danger">Error text for critical information.</Text>
              </Col>
              <Col span={12}>
                <Paragraph>
                  This is a paragraph demonstrating our typography system. The text uses our 
                  standardized font family (Inter), consistent line heights, and proper spacing 
                  for optimal readability across all devices and screen sizes.
                </Paragraph>
                <Paragraph className="text-lead">
                  This is lead text that stands out from regular paragraphs. It's typically 
                  used for introductory content or important summaries.
                </Paragraph>
                <Text code>Code text using monospace font</Text>
                <br />
                <Text keyboard>Keyboard shortcut: Ctrl+S</Text>
                <br />
                <Text mark>Highlighted text for emphasis</Text>
                <br />
                <Text underline>Underlined text for links</Text>
                <br />
                <Text delete>Deleted text with strikethrough</Text>
              </Col>
            </Row>
          </Card>

          {/* Button Components */}
          <Card title="Button Components" className="mb-6">
            <Row gutter={[16, 16]}>
              <Col span={8}>
                <Title level={4}>Primary Buttons</Title>
                <Space direction="vertical" style={{ width: '100%' }}>
                  <Button type="primary" size="large" block>
                    Large Primary Button
                  </Button>
                  <Button type="primary" block>
                    Default Primary Button
                  </Button>
                  <Button type="primary" size="small" block>
                    Small Primary Button
                  </Button>
                  <Button type="primary" loading={loading} onClick={handleAsyncAction} block>
                    {loading ? 'Processing...' : 'Async Action'}
                  </Button>
                </Space>
              </Col>
              <Col span={8}>
                <Title level={4}>Secondary Buttons</Title>
                <Space direction="vertical" style={{ width: '100%' }}>
                  <Button size="large" block>
                    Large Default Button
                  </Button>
                  <Button block>
                    Default Button
                  </Button>
                  <Button size="small" block>
                    Small Button
                  </Button>
                  <Button disabled block>
                    Disabled Button
                  </Button>
                </Space>
              </Col>
              <Col span={8}>
                <Title level={4}>Icon Buttons</Title>
                <Space direction="vertical" style={{ width: '100%' }}>
                  <Button type="primary" icon={<PlusOutlined />} block>
                    Add New Item
                  </Button>
                  <Button icon={<EditOutlined />} block>
                    Edit Content
                  </Button>
                  <Button danger icon={<DeleteOutlined />} block>
                    Delete Item
                  </Button>
                  <Button type="dashed" icon={<DownloadOutlined />} block>
                    Download File
                  </Button>
                </Space>
              </Col>
            </Row>
          </Card>

          {/* Form Components */}
          <Card title="Form Components" className="mb-6">
            <Row gutter={[24, 24]}>
              <Col span={12}>
                <Space direction="vertical" style={{ width: '100%' }}>
                  <div>
                    <Text strong>Text Input</Text>
                    <Input placeholder="Enter your text here" />
                  </div>
                  <div>
                    <Text strong>Search Input</Text>
                    <Input.Search 
                      placeholder="Search for anything..." 
                      prefix={<SearchOutlined />}
                      enterButton="Search"
                    />
                  </div>
                  <div>
                    <Text strong>Password Input</Text>
                    <Input.Password placeholder="Enter your password" />
                  </div>
                  <div>
                    <Text strong>Text Area</Text>
                    <Input.TextArea 
                      rows={4} 
                      placeholder="Enter multiple lines of text..."
                    />
                  </div>
                </Space>
              </Col>
              <Col span={12}>
                <Space direction="vertical" style={{ width: '100%' }}>
                  <div>
                    <Text strong>Switch Component</Text>
                    <br />
                    <Switch defaultChecked />
                    <Switch size="small" style={{ marginLeft: 8 }} />
                  </div>
                  <div>
                    <Text strong>Slider Component</Text>
                    <Slider defaultValue={30} />
                    <Slider range defaultValue={[20, 50]} />
                  </div>
                  <div>
                    <Text strong>Rate Component</Text>
                    <br />
                    <Rate defaultValue={3} />
                    <Rate character={<HeartOutlined />} defaultValue={4} />
                  </div>
                  <div>
                    <Text strong>Progress Component</Text>
                    <Progress percent={30} />
                    <Progress percent={70} status="active" />
                    <Progress percent={100} />
                  </div>
                </Space>
              </Col>
            </Row>
          </Card>

          {/* Feedback Components */}
          <Card title="Feedback Components" className="mb-6">
            <Space direction="vertical" style={{ width: '100%' }}>
              <Alert
                message="Success Alert"
                description="This is a success alert with a detailed description."
                type="success"
                showIcon
                closable
              />
              <Alert
                message="Information Alert"
                description="This is an informational alert providing helpful context."
                type="info"
                showIcon
                closable
              />
              <Alert
                message="Warning Alert"
                description="This is a warning alert indicating potential issues."
                type="warning"
                showIcon
                closable
              />
              <Alert
                message="Error Alert"
                description="This is an error alert indicating critical problems."
                type="error"
                showIcon
                closable
              />
            </Space>
          </Card>

          {/* Data Display Components */}
          <Card title="Data Display Components" className="mb-6">
            <Row gutter={[24, 24]}>
              <Col span={12}>
                <Title level={4}>Tags & Badges</Title>
                <Space wrap>
                  <Tag color="blue">Blue Tag</Tag>
                  <Tag color="green">Green Tag</Tag>
                  <Tag color="orange">Orange Tag</Tag>
                  <Tag color="red">Red Tag</Tag>
                  <Tag color="purple">Purple Tag</Tag>
                  <Tag closable>Closable Tag</Tag>
                </Space>
                <Divider />
                <Space>
                  <Badge count={5}>
                    <Avatar shape="square" icon={<UserOutlined />} />
                  </Badge>
                  <Badge count={0} showZero>
                    <Avatar shape="square" icon={<UserOutlined />} />
                  </Badge>
                  <Badge count={99}>
                    <Avatar shape="square" icon={<UserOutlined />} />
                  </Badge>
                  <Badge count={100}>
                    <Avatar shape="square" icon={<UserOutlined />} />
                  </Badge>
                  <Badge count={99} overflowCount={10}>
                    <Avatar shape="square" icon={<UserOutlined />} />
                  </Badge>
                </Space>
              </Col>
              <Col span={12}>
                <Title level={4}>Avatars</Title>
                <Space>
                  <Avatar size={64} icon={<UserOutlined />} />
                  <Avatar size="large" icon={<UserOutlined />} />
                  <Avatar icon={<UserOutlined />} />
                  <Avatar size="small" icon={<UserOutlined />} />
                </Space>
                <Divider />
                <Space>
                  <Avatar size="large" src="https://api.dicebear.com/7.x/miniavs/svg?seed=1" />
                  <Avatar size="large" style={{ backgroundColor: '#87d068' }} icon={<UserOutlined />} />
                  <Avatar size="large" style={{ backgroundColor: '#1890ff' }}>U</Avatar>
                  <Avatar size="large" style={{ backgroundColor: '#f56a00' }}>USER</Avatar>
                </Space>
              </Col>
            </Row>
          </Card>

          {/* Interactive Components */}
          <Card title="Interactive Components" className="mb-6">
            <Space>
              <Tooltip title="This is a helpful tooltip">
                <Button icon={<SettingOutlined />}>Hover for Tooltip</Button>
              </Tooltip>
              
              <Popover 
                content={
                  <div>
                    <p>This is a popover with more detailed content.</p>
                    <p>It can contain multiple elements and actions.</p>
                  </div>
                }
                title="Popover Title"
                trigger="click"
              >
                <Button icon={<BellOutlined />}>Click for Popover</Button>
              </Popover>
              
              <Button type="primary" onClick={() => setModalVisible(true)}>
                Open Modal
              </Button>
            </Space>
          </Card>

          {/* Color Palette */}
          <Card title="Color Palette" className="mb-6">
            <Row gutter={[16, 16]}>
              <Col span={6}>
                <Title level={5}>Primary Colors</Title>
                <div style={{ display: 'flex', flexDirection: 'column', gap: '8px' }}>
                  <div style={{ background: '#3b82f6', height: '40px', borderRadius: '8px', display: 'flex', alignItems: 'center', justifyContent: 'center', color: 'white' }}>
                    Primary
                  </div>
                  <div style={{ background: '#2563eb', height: '40px', borderRadius: '8px', display: 'flex', alignItems: 'center', justifyContent: 'center', color: 'white' }}>
                    Primary Hover
                  </div>
                  <div style={{ background: '#1d4ed8', height: '40px', borderRadius: '8px', display: 'flex', alignItems: 'center', justifyContent: 'center', color: 'white' }}>
                    Primary Active
                  </div>
                </div>
              </Col>
              <Col span={6}>
                <Title level={5}>Semantic Colors</Title>
                <div style={{ display: 'flex', flexDirection: 'column', gap: '8px' }}>
                  <div style={{ background: '#10b981', height: '40px', borderRadius: '8px', display: 'flex', alignItems: 'center', justifyContent: 'center', color: 'white' }}>
                    Success
                  </div>
                  <div style={{ background: '#f59e0b', height: '40px', borderRadius: '8px', display: 'flex', alignItems: 'center', justifyContent: 'center', color: 'white' }}>
                    Warning
                  </div>
                  <div style={{ background: '#ef4444', height: '40px', borderRadius: '8px', display: 'flex', alignItems: 'center', justifyContent: 'center', color: 'white' }}>
                    Error
                  </div>
                </div>
              </Col>
              <Col span={12}>
                <Title level={5}>Design Tokens</Title>
                <Text>
                  Our design system uses CSS custom properties for consistent theming:
                </Text>
                <ul style={{ marginTop: '8px' }}>
                  <li><Text code>--brand-primary</Text> - Primary brand color</li>
                  <li><Text code>--color-success</Text> - Success state color</li>
                  <li><Text code>--space-4</Text> - 16px spacing unit</li>
                  <li><Text code>--radius-base</Text> - 8px border radius</li>
                  <li><Text code>--font-size-base</Text> - 16px base font size</li>
                </ul>
              </Col>
            </Row>
          </Card>

          {/* Enhanced Features Demo */}
          <Card title="Enhanced Features Status" className="mb-6">
            <Row gutter={[16, 16]}>
              <Col span={8}>
                <Alert
                  message="Enhanced Query Processing"
                  description="AI-powered query processing with conversation context and schema-aware SQL generation."
                  type="success"
                  showIcon
                />
              </Col>
              <Col span={8}>
                <Alert
                  message="Real-Time Streaming"
                  description="Live data processing and streaming dashboards with SignalR integration."
                  type="success"
                  showIcon
                />
              </Col>
              <Col span={8}>
                <Alert
                  message="Design System"
                  description="Comprehensive design system with standardized components and tokens."
                  type="success"
                  showIcon
                />
              </Col>
            </Row>
          </Card>
        </div>
      </div>

      {/* Modal Example */}
      <Modal
        title="Design System Modal"
        open={modalVisible}
        onOk={() => setModalVisible(false)}
        onCancel={() => setModalVisible(false)}
        width={600}
      >
        <p>This modal demonstrates our standardized modal styling with consistent spacing, typography, and colors.</p>
        <p>All modals across the application will follow this design pattern for a cohesive user experience.</p>
        <Alert
          message="Modal Content"
          description="Modals can contain any type of content including alerts, forms, and other components."
          type="info"
          showIcon
        />
      </Modal>
    </div>
  );
};

export default DesignSystemShowcase;
