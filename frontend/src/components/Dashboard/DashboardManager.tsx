/**
 * Unified Dashboard Manager Component
 * Consolidates dashboard management functionality from multiple sources
 */

import React, { useState, useEffect } from 'react';
import {
  Card,
  Button,
  Space,
  Typography,
  Row,
  Col,
  Alert,
  Spin,
  Modal,
  Form,
  Input,
  Select,
  Tag,
  List,
  Tooltip
} from 'antd';
import {
  DashboardOutlined,
  PlusOutlined,
  EditOutlined,
  DeleteOutlined,
  ShareAltOutlined,
  DownloadOutlined,
  RobotOutlined,
  EyeOutlined
} from '@ant-design/icons';

const { Title, Text, Paragraph } = Typography;
const { Option } = Select;

interface Dashboard {
  id: string;
  name: string;
  description: string;
  category: string;
  widgets: DashboardWidget[];
  layout: DashboardLayout;
  isPublic: boolean;
  tags: string[];
  createdAt: string;
  updatedAt: string;
  lastViewedAt?: string;
}

interface DashboardWidget {
  id: string;
  title: string;
  type: 'chart' | 'table' | 'metric' | 'text' | 'filter';
  position: { x: number; y: number; w: number; h: number };
  configuration: any;
  dataSource: any;
}

interface DashboardLayout {
  type: 'grid' | 'freeform';
  columns: number;
  rowHeight: number;
  margin: [number, number];
  padding: [number, number];
}

interface CreateDashboardRequest {
  name: string;
  description?: string;
  category: string;
  widgets?: any[];
  layout?: DashboardLayout;
  isPublic?: boolean;
  tags?: string[];
}

export const DashboardManager: React.FC = () => {
  const [dashboards, setDashboards] = useState<Dashboard[]>([]);
  const [loading, setLoading] = useState(false);
  const [selectedDashboard, setSelectedDashboard] = useState<Dashboard | null>(null);
  const [createModalVisible, setCreateModalVisible] = useState(false);
  const [aiGenerateModalVisible, setAiGenerateModalVisible] = useState(false);
  const [form] = Form.useForm();

  // Real dashboards - loaded from API
  const [realDashboards, setRealDashboards] = useState<Dashboard[]>([]);
  const [loadingReal, setLoadingReal] = useState(true);
  const [dashboardError, setDashboardError] = useState<string | null>(null);

  // Load real dashboards on component mount
  React.useEffect(() => {
    const loadRealDashboards = async () => {
      try {
        setLoadingReal(true);
        setDashboardError(null);

        // TODO: Replace with actual dashboard API calls
        // const response = await dashboardApi.getUserDashboards();
        // setRealDashboards(response.dashboards);

        console.log('Loading real dashboards...');

        // For now, show empty state until real API is connected
        setRealDashboards([]);

      } catch (err) {
        console.error('Failed to load dashboards:', err);
        setDashboardError('Failed to load dashboards. Please check your connection.');
      } finally {
        setLoadingReal(false);
      }
    };

    loadRealDashboards();
  }, []);

  useEffect(() => {
    loadDashboards();
  }, []);

  const loadDashboards = async () => {
    setLoading(true);
    try {
      // Use real dashboards instead of mock data
      setDashboards(realDashboards);
    } catch (error) {
      console.error('Failed to load dashboards:', error);
    } finally {
      setLoading(false);
    }
  };

  const handleCreateDashboard = async (values: CreateDashboardRequest) => {
    try {
      const newDashboard: Dashboard = {
        id: Date.now().toString(),
        name: values.name,
        description: values.description || '',
        category: values.category,
        widgets: [],
        layout: { type: 'grid', columns: 12, rowHeight: 30, margin: [10, 10], padding: [10, 10] },
        isPublic: values.isPublic || false,
        tags: values.tags || [],
        createdAt: new Date().toISOString(),
        updatedAt: new Date().toISOString()
      };

      setDashboards(prev => [...prev, newDashboard]);
      setCreateModalVisible(false);
      form.resetFields();
    } catch (error) {
      console.error('Failed to create dashboard:', error);
    }
  };

  const handleDeleteDashboard = (dashboardId: string) => {
    Modal.confirm({
      title: 'Delete Dashboard',
      content: 'Are you sure you want to delete this dashboard? This action cannot be undone.',
      okText: 'Delete',
      okType: 'danger',
      onOk: () => {
        setDashboards(prev => prev.filter(d => d.id !== dashboardId));
      }
    });
  };

  const getCategoryColor = (category: string): string => {
    const colors: Record<string, string> = {
      'Analytics': 'blue',
      'Finance': 'green',
      'Operations': 'orange',
      'Marketing': 'purple',
      'Customer': 'cyan'
    };
    return colors[category] || 'default';
  };

  const renderDashboardCard = (dashboard: Dashboard) => (
    <Card
      key={dashboard.id}
      hoverable
      actions={[
        <Tooltip title="View Dashboard">
          <EyeOutlined onClick={() => setSelectedDashboard(dashboard)} />
        </Tooltip>,
        <Tooltip title="Edit Dashboard">
          <EditOutlined onClick={() => console.log('Edit dashboard:', dashboard.id)} />
        </Tooltip>,
        <Tooltip title="Share Dashboard">
          <ShareAltOutlined onClick={() => console.log('Share dashboard:', dashboard.id)} />
        </Tooltip>,
        <Tooltip title="Delete Dashboard">
          <DeleteOutlined onClick={() => handleDeleteDashboard(dashboard.id)} />
        </Tooltip>
      ]}
    >
      <Card.Meta
        title={
          <Space>
            {dashboard.name}
            {dashboard.isPublic && <Tag color="green">Public</Tag>}
          </Space>
        }
        description={
          <Space direction="vertical" size="small" style={{ width: '100%' }}>
            <Text type="secondary">{dashboard.description}</Text>
            <Space wrap>
              <Tag color={getCategoryColor(dashboard.category)}>{dashboard.category}</Tag>
              {dashboard.tags.map(tag => (
                <Tag key={tag}>{tag}</Tag>
              ))}
            </Space>
            <Text type="secondary" style={{ fontSize: '12px' }}>
              Updated: {new Date(dashboard.updatedAt).toLocaleDateString()}
            </Text>
          </Space>
        }
      />
    </Card>
  );

  const renderDashboardList = () => (
    <div>
      <div style={{ marginBottom: '24px', display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
        <div>
          <Title level={2}>
            <DashboardOutlined style={{ color: '#1890ff', marginRight: '8px' }} />
            Dashboard Manager
          </Title>
          <Text type="secondary">Create, manage, and share interactive dashboards</Text>
        </div>
        <Space>
          <Button 
            type="primary" 
            icon={<PlusOutlined />} 
            onClick={() => setCreateModalVisible(true)}
          >
            Create Dashboard
          </Button>
          <Button 
            icon={<RobotOutlined />} 
            onClick={() => setAiGenerateModalVisible(true)}
          >
            AI Generate
          </Button>
        </Space>
      </div>

      {loading ? (
        <div style={{ textAlign: 'center', padding: '40px' }}>
          <Spin size="large" />
          <div style={{ marginTop: '16px' }}>
            <Text>Loading dashboards...</Text>
          </div>
        </div>
      ) : dashboards.length === 0 ? (
        <Alert
          message="No Dashboards Found"
          description="Create your first dashboard to get started with analytics."
          type="info"
          showIcon
          action={
            <Space>
              <Button type="primary" icon={<PlusOutlined />} onClick={() => setCreateModalVisible(true)}>
                Create Dashboard
              </Button>
              <Button icon={<RobotOutlined />} onClick={() => setAiGenerateModalVisible(true)}>
                AI Generate
              </Button>
            </Space>
          }
        />
      ) : (
        <Row gutter={[16, 16]}>
          {dashboards.map(dashboard => (
            <Col xs={24} sm={12} lg={8} xl={6} key={dashboard.id}>
              {renderDashboardCard(dashboard)}
            </Col>
          ))}
        </Row>
      )}
    </div>
  );

  const renderDashboardView = () => (
    <div>
      <div style={{ marginBottom: '24px', display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
        <div>
          <Button type="link" onClick={() => setSelectedDashboard(null)}>
            ← Back to Dashboards
          </Button>
          <Title level={2} style={{ margin: 0 }}>
            {selectedDashboard?.name}
          </Title>
          <Text type="secondary">{selectedDashboard?.description}</Text>
        </div>
        <Space>
          <Button icon={<EditOutlined />}>Edit</Button>
          <Button icon={<ShareAltOutlined />}>Share</Button>
          <Button icon={<DownloadOutlined />}>Export</Button>
        </Space>
      </div>

      <Alert
        message="Dashboard View"
        description="Dashboard rendering will be implemented with the widget system and real-time updates."
        type="info"
        style={{ marginBottom: '24px' }}
      />

      {/* Dashboard widgets will be rendered here */}
      <div style={{ minHeight: '400px', background: '#f5f5f5', padding: '24px', borderRadius: '8px' }}>
        <Text type="secondary">Dashboard widgets and layout will be rendered here</Text>
      </div>
    </div>
  );

  return (
    <div style={{ padding: '24px', maxWidth: '1400px', margin: '0 auto' }}>
      {selectedDashboard ? renderDashboardView() : renderDashboardList()}

      {/* Create Dashboard Modal */}
      <Modal
        title="Create New Dashboard"
        open={createModalVisible}
        onCancel={() => setCreateModalVisible(false)}
        onOk={() => form.submit()}
        width={600}
      >
        <Form
          form={form}
          layout="vertical"
          onFinish={handleCreateDashboard}
        >
          <Form.Item
            name="name"
            label="Dashboard Name"
            rules={[{ required: true, message: 'Please enter dashboard name' }]}
          >
            <Input placeholder="Enter dashboard name" />
          </Form.Item>
          
          <Form.Item
            name="description"
            label="Description"
          >
            <Input.TextArea placeholder="Enter dashboard description" rows={3} />
          </Form.Item>
          
          <Form.Item
            name="category"
            label="Category"
            rules={[{ required: true, message: 'Please select a category' }]}
          >
            <Select placeholder="Select category">
              <Option value="Analytics">Analytics</Option>
              <Option value="Finance">Finance</Option>
              <Option value="Operations">Operations</Option>
              <Option value="Marketing">Marketing</Option>
              <Option value="Customer">Customer</Option>
            </Select>
          </Form.Item>
          
          <Form.Item
            name="tags"
            label="Tags"
          >
            <Select mode="tags" placeholder="Add tags">
              <Option value="revenue">revenue</Option>
              <Option value="players">players</Option>
              <Option value="performance">performance</Option>
              <Option value="analytics">analytics</Option>
            </Select>
          </Form.Item>
          
          <Form.Item
            name="isPublic"
            valuePropName="checked"
          >
            <input type="checkbox" /> Make dashboard public
          </Form.Item>
        </Form>
      </Modal>

      {/* AI Generate Modal */}
      <Modal
        title="AI Dashboard Generator"
        open={aiGenerateModalVisible}
        onCancel={() => setAiGenerateModalVisible(false)}
        footer={null}
        width={600}
      >
        <Alert
          message="AI Dashboard Generation"
          description="AI-powered dashboard generation will analyze your data and create optimized dashboard layouts automatically."
          type="info"
          style={{ marginBottom: '16px' }}
        />
        <Text>This feature will be implemented with AI integration.</Text>
      </Modal>
    </div>
  );
};
