import React, { useState, useEffect, useCallback } from 'react';
import { 
  Card, 
  Button, 
  Input, 
  Select, 
  Row, 
  Col, 
  Typography, 
  Space, 
  Modal, 
  Form, 
  Switch, 
  Tag, 
  Tooltip,
  Divider,
  Alert,
  Spin,
  Badge,
  Dropdown,
  Menu
} from 'antd';
import { 
  PlusOutlined, 
  DashboardOutlined, 
  SaveOutlined, 
  ShareAltOutlined,
  SettingOutlined,
  EyeOutlined,
  EditOutlined,
  DeleteOutlined,
  CopyOutlined,
  DownloadOutlined,
  BulbOutlined,
  ThunderboltOutlined,
  RobotOutlined
} from '@ant-design/icons';
import { DragDropContext, Droppable, Draggable } from 'react-beautiful-dnd';

const { Title, Text, Paragraph } = Typography;
const { TextArea } = Input;
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

export const EnhancedDashboardInterface: React.FC = () => {
  const [dashboards, setDashboards] = useState<Dashboard[]>([]);
  const [selectedDashboard, setSelectedDashboard] = useState<Dashboard | null>(null);
  const [loading, setLoading] = useState(false);
  const [createModalVisible, setCreateModalVisible] = useState(false);
  const [aiGenerateModalVisible, setAiGenerateModalVisible] = useState(false);
  const [form] = Form.useForm();
  const [aiForm] = Form.useForm();

  useEffect(() => {
    loadDashboards();
  }, []);

  const loadDashboards = async () => {
    setLoading(true);
    try {
      const response = await fetch('/api/dashboard/list', {
        headers: {
          'Authorization': `Bearer ${localStorage.getItem('token')}`
        }
      });

      if (response.ok) {
        const data = await response.json();
        setDashboards(data.dashboards || []);
      }
    } catch (error) {
      console.error('❌ Error loading dashboards:', error);
    } finally {
      setLoading(false);
    }
  };

  const createDashboard = async (values: CreateDashboardRequest) => {
    try {
      const response = await fetch('/api/dashboard/create', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Authorization': `Bearer ${localStorage.getItem('token')}`
        },
        body: JSON.stringify(values)
      });

      if (response.ok) {
        const newDashboard = await response.json();
        setDashboards(prev => [...prev, newDashboard]);
        setCreateModalVisible(false);
        form.resetFields();
        return newDashboard;
      }
    } catch (error) {
      console.error('❌ Error creating dashboard:', error);
    }
  };

  const generateAIDashboard = async (values: { description: string; category: string }) => {
    try {
      setLoading(true);
      const response = await fetch('/api/dashboard/generate', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Authorization': `Bearer ${localStorage.getItem('token')}`
        },
        body: JSON.stringify({
          description: values.description,
          category: values.category
        })
      });

      if (response.ok) {
        const generatedDashboard = await response.json();
        setDashboards(prev => [...prev, generatedDashboard]);
        setAiGenerateModalVisible(false);
        aiForm.resetFields();
        return generatedDashboard;
      }
    } catch (error) {
      console.error('❌ Error generating AI dashboard:', error);
    } finally {
      setLoading(false);
    }
  };

  const deleteDashboard = async (dashboardId: string) => {
    try {
      const response = await fetch(`/api/dashboard/${dashboardId}`, {
        method: 'DELETE',
        headers: {
          'Authorization': `Bearer ${localStorage.getItem('token')}`
        }
      });

      if (response.ok) {
        setDashboards(prev => prev.filter(d => d.id !== dashboardId));
        if (selectedDashboard?.id === dashboardId) {
          setSelectedDashboard(null);
        }
      }
    } catch (error) {
      console.error('❌ Error deleting dashboard:', error);
    }
  };

  const cloneDashboard = async (dashboardId: string, newName: string) => {
    try {
      const response = await fetch(`/api/dashboard/${dashboardId}/clone`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Authorization': `Bearer ${localStorage.getItem('token')}`
        },
        body: JSON.stringify({ newName })
      });

      if (response.ok) {
        const clonedDashboard = await response.json();
        setDashboards(prev => [...prev, clonedDashboard]);
        return clonedDashboard;
      }
    } catch (error) {
      console.error('❌ Error cloning dashboard:', error);
    }
  };

  const getDashboardActions = (dashboard: Dashboard) => (
    <Menu>
      <Menu.Item key="view" icon={<EyeOutlined />} onClick={() => setSelectedDashboard(dashboard)}>
        View Dashboard
      </Menu.Item>
      <Menu.Item key="edit" icon={<EditOutlined />}>
        Edit Dashboard
      </Menu.Item>
      <Menu.Item key="clone" icon={<CopyOutlined />} onClick={() => cloneDashboard(dashboard.id, `${dashboard.name} (Copy)`)}>
        Clone Dashboard
      </Menu.Item>
      <Menu.Item key="share" icon={<ShareAltOutlined />}>
        Share Dashboard
      </Menu.Item>
      <Menu.Divider />
      <Menu.Item key="export" icon={<DownloadOutlined />}>
        Export Dashboard
      </Menu.Item>
      <Menu.Item key="delete" icon={<DeleteOutlined />} danger onClick={() => deleteDashboard(dashboard.id)}>
        Delete Dashboard
      </Menu.Item>
    </Menu>
  );

  const renderDashboardCard = (dashboard: Dashboard) => (
    <Card
      key={dashboard.id}
      hoverable
      style={{ marginBottom: '16px' }}
      actions={[
        <Tooltip title="View Dashboard">
          <EyeOutlined onClick={() => setSelectedDashboard(dashboard)} />
        </Tooltip>,
        <Tooltip title="Edit Dashboard">
          <EditOutlined />
        </Tooltip>,
        <Dropdown overlay={getDashboardActions(dashboard)} trigger={['click']}>
          <SettingOutlined />
        </Dropdown>
      ]}
    >
      <Card.Meta
        title={
          <Space>
            <DashboardOutlined />
            {dashboard.name}
            {dashboard.isPublic && <Badge status="success" text="Public" />}
          </Space>
        }
        description={
          <div>
            <Paragraph ellipsis={{ rows: 2 }}>{dashboard.description}</Paragraph>
            <Space wrap style={{ marginTop: '8px' }}>
              <Tag color="blue">{dashboard.category}</Tag>
              <Tag>{dashboard.widgets.length} widgets</Tag>
              {dashboard.tags.map(tag => (
                <Tag key={tag} color="default">{tag}</Tag>
              ))}
            </Space>
            <div style={{ marginTop: '8px' }}>
              <Text type="secondary" style={{ fontSize: '12px' }}>
                Updated: {new Date(dashboard.updatedAt).toLocaleDateString()}
              </Text>
            </div>
          </div>
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
            Multi-Modal Dashboards
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
          description="Create your first dashboard to get started with multi-modal analytics."
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
        footer={null}
        width={600}
      >
        <Form
          form={form}
          layout="vertical"
          onFinish={createDashboard}
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
            <TextArea rows={3} placeholder="Describe your dashboard" />
          </Form.Item>

          <Form.Item
            name="category"
            label="Category"
            rules={[{ required: true, message: 'Please select a category' }]}
          >
            <Select placeholder="Select category">
              <Option value="Analytics">Analytics</Option>
              <Option value="Operations">Operations</Option>
              <Option value="Finance">Finance</Option>
              <Option value="Marketing">Marketing</Option>
              <Option value="Executive">Executive</Option>
            </Select>
          </Form.Item>

          <Form.Item
            name="isPublic"
            label="Visibility"
            valuePropName="checked"
          >
            <Switch checkedChildren="Public" unCheckedChildren="Private" />
          </Form.Item>

          <Form.Item>
            <Space>
              <Button type="primary" htmlType="submit" icon={<SaveOutlined />}>
                Create Dashboard
              </Button>
              <Button onClick={() => setCreateModalVisible(false)}>
                Cancel
              </Button>
            </Space>
          </Form.Item>
        </Form>
      </Modal>

      {/* AI Generate Dashboard Modal */}
      <Modal
        title={
          <Space>
            <RobotOutlined style={{ color: '#1890ff' }} />
            AI Dashboard Generator
          </Space>
        }
        open={aiGenerateModalVisible}
        onCancel={() => setAiGenerateModalVisible(false)}
        footer={null}
        width={600}
      >
        <Form
          form={aiForm}
          layout="vertical"
          onFinish={generateAIDashboard}
        >
          <Alert
            message="AI-Powered Dashboard Generation"
            description="Describe what you want to analyze and AI will create an appropriate dashboard with relevant widgets."
            type="info"
            style={{ marginBottom: '16px' }}
          />

          <Form.Item
            name="description"
            label="Dashboard Description"
            rules={[{ required: true, message: 'Please describe your dashboard requirements' }]}
          >
            <TextArea 
              rows={4} 
              placeholder="e.g., 'Create an executive dashboard showing revenue trends, top players, and game performance metrics'"
            />
          </Form.Item>

          <Form.Item
            name="category"
            label="Category"
            rules={[{ required: true, message: 'Please select a category' }]}
          >
            <Select placeholder="Select category">
              <Option value="Analytics">Analytics</Option>
              <Option value="Operations">Operations</Option>
              <Option value="Finance">Finance</Option>
              <Option value="Marketing">Marketing</Option>
              <Option value="Executive">Executive</Option>
            </Select>
          </Form.Item>

          <Form.Item>
            <Space>
              <Button type="primary" htmlType="submit" icon={<BulbOutlined />} loading={loading}>
                Generate Dashboard
              </Button>
              <Button onClick={() => setAiGenerateModalVisible(false)}>
                Cancel
              </Button>
            </Space>
          </Form.Item>
        </Form>
      </Modal>
    </div>
  );
};

export default EnhancedDashboardInterface;
