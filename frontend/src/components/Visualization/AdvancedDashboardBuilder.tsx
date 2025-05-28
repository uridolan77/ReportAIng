import React, { useState, useCallback } from 'react';
import {
  Card,
  Row,
  Col,
  Button,
  Space,
  Typography,
  Spin,
  Alert,
  Form,
  Input,
  Select,
  Switch,
  Slider,

  Modal,
  message,
  Steps,
  Badge
} from 'antd';
import {
  DashboardOutlined,

  SettingOutlined,
  SaveOutlined,
  EyeOutlined,
  ThunderboltOutlined,

} from '@ant-design/icons';
import {
  AdvancedDashboardConfig,
  DashboardPreferences,

} from '../../types/visualization';
import AdvancedDashboard from './AdvancedDashboard';
import advancedVisualizationService from '../../services/advancedVisualizationService';

const { Title, Text } = Typography;
const { Option } = Select;
const { Step } = Steps;
// const { TextArea } = Input;

interface AdvancedDashboardBuilderProps {
  data: any[];
  columns: any[];
  query: string;
  onDashboardGenerated?: (dashboard: AdvancedDashboardConfig) => void;
  onSave?: (dashboard: AdvancedDashboardConfig) => void;
}

const AdvancedDashboardBuilder: React.FC<AdvancedDashboardBuilderProps> = ({
  data,
  columns,
  query,
  onDashboardGenerated,
  onSave
}) => {
  const [currentStep, setCurrentStep] = useState(0);
  const [loading, setLoading] = useState(false);
  const [dashboard, setDashboard] = useState<AdvancedDashboardConfig | null>(null);
  const [showPreview, setShowPreview] = useState(false);
  const [form] = Form.useForm();

  // Default preferences
  const [preferences, setPreferences] = useState<DashboardPreferences>({
    title: `Dashboard - ${new Date().toLocaleDateString()}`,
    enableRealTime: false,
    enableCollaboration: false,
    enableAnalytics: true,
    layout: 'Auto',
    preferredChartTypes: ['Bar', 'Line', 'Pie'],
    refreshInterval: 30
  });

  // Generate dashboard
  const generateDashboard = useCallback(async () => {
    if (!data.length || !query) return;

    setLoading(true);
    try {
      const response = await advancedVisualizationService.generateAdvancedDashboard({
        query,
        preferences
      });

      if (response.success && response.dashboard) {
        setDashboard(response.dashboard);
        onDashboardGenerated?.(response.dashboard);
        message.success('Advanced dashboard generated successfully!');
        setCurrentStep(2); // Move to preview step
      } else {
        message.error(response.errorMessage || 'Failed to generate dashboard');
      }
    } catch (error) {
      console.error('Error generating dashboard:', error);
      message.error('Failed to generate dashboard');
    } finally {
      setLoading(false);
    }
  }, [data, query, preferences, onDashboardGenerated]);

  // Save dashboard
  const saveDashboard = useCallback(async () => {
    if (!dashboard) return;

    try {
      // Here you would typically save to your backend
      onSave?.(dashboard);
      message.success('Dashboard saved successfully!');
    } catch (error) {
      console.error('Error saving dashboard:', error);
      message.error('Failed to save dashboard');
    }
  }, [dashboard, onSave]);

  // Step 1: Configuration
  const renderConfigurationStep = () => (
    <Card title="Dashboard Configuration" style={{ marginBottom: 16 }}>
      <Form
        form={form}
        layout="vertical"
        initialValues={preferences}
        onValuesChange={(changedValues, allValues) => {
          setPreferences({ ...preferences, ...allValues });
        }}
      >
        <Row gutter={16}>
          <Col span={12}>
            <Form.Item
              label="Dashboard Title"
              name="title"
              rules={[{ required: true, message: 'Please enter a title' }]}
            >
              <Input placeholder="Enter dashboard title" />
            </Form.Item>
          </Col>
          <Col span={12}>
            <Form.Item
              label="Layout Type"
              name="layout"
            >
              <Select>
                <Option value="Auto">Auto Layout</Option>
                <Option value="Grid">Grid Layout</Option>
                <Option value="Masonry">Masonry Layout</Option>
                <Option value="Responsive">Responsive Layout</Option>
              </Select>
            </Form.Item>
          </Col>
        </Row>

        <Row gutter={16}>
          <Col span={8}>
            <Form.Item
              label="Preferred Chart Types"
              name="preferredChartTypes"
            >
              <Select mode="multiple" placeholder="Select chart types">
                <Option value="Bar">Bar Chart</Option>
                <Option value="Line">Line Chart</Option>
                <Option value="Pie">Pie Chart</Option>
                <Option value="Scatter">Scatter Plot</Option>
                <Option value="Area">Area Chart</Option>
                <Option value="Heatmap">Heatmap</Option>
                <Option value="Gauge">Gauge</Option>
                <Option value="Funnel">Funnel</Option>
              </Select>
            </Form.Item>
          </Col>
          <Col span={8}>
            <Form.Item
              label="Refresh Interval (seconds)"
              name="refreshInterval"
            >
              <Slider
                min={10}
                max={300}
                marks={{
                  10: '10s',
                  30: '30s',
                  60: '1m',
                  300: '5m'
                }}
              />
            </Form.Item>
          </Col>
          <Col span={8}>
            <Space direction="vertical" style={{ width: '100%' }}>
              <Form.Item
                label="Real-time Updates"
                name="enableRealTime"
                valuePropName="checked"
              >
                <Switch />
              </Form.Item>
              <Form.Item
                label="Enable Collaboration"
                name="enableCollaboration"
                valuePropName="checked"
              >
                <Switch />
              </Form.Item>
              <Form.Item
                label="Enable Analytics"
                name="enableAnalytics"
                valuePropName="checked"
              >
                <Switch />
              </Form.Item>
            </Space>
          </Col>
        </Row>
      </Form>
    </Card>
  );

  // Step 2: Generation
  const renderGenerationStep = () => (
    <Card title="Dashboard Generation" style={{ marginBottom: 16 }}>
      <div style={{ textAlign: 'center', padding: 40 }}>
        {loading ? (
          <>
            <Spin size="large" />
            <br />
            <br />
            <Text>Generating your advanced dashboard...</Text>
            <br />
            <Text type="secondary">
              AI is analyzing your data and creating optimized visualizations
            </Text>
          </>
        ) : dashboard ? (
          <>
            <DashboardOutlined style={{ fontSize: 48, color: '#52c41a' }} />
            <br />
            <br />
            <Title level={4}>Dashboard Generated Successfully!</Title>
            <Text type="secondary">
              Your dashboard contains {dashboard.charts.length} charts
            </Text>
            <br />
            <br />
            <Space>
              <Button type="primary" onClick={() => setShowPreview(true)}>
                Preview Dashboard
              </Button>
              <Button onClick={() => setCurrentStep(0)}>
                Modify Configuration
              </Button>
            </Space>
          </>
        ) : (
          <>
            <ThunderboltOutlined style={{ fontSize: 48, color: '#1890ff' }} />
            <br />
            <br />
            <Title level={4}>Ready to Generate</Title>
            <Text type="secondary">
              Click the button below to generate your AI-powered dashboard
            </Text>
            <br />
            <br />
            <Button
              type="primary"
              size="large"
              onClick={generateDashboard}
              disabled={!data.length || !query}
            >
              Generate Dashboard
            </Button>
          </>
        )}
      </div>
    </Card>
  );

  // Step 3: Preview and Save
  const renderPreviewStep = () => (
    <Card
      title="Dashboard Preview"
      style={{ marginBottom: 16 }}
      extra={
        <Space>
          <Button onClick={() => setCurrentStep(1)}>
            Back to Generation
          </Button>
          <Button type="primary" onClick={saveDashboard}>
            Save Dashboard
          </Button>
        </Space>
      }
    >
      {dashboard ? (
        <div style={{ border: '1px solid #d9d9d9', borderRadius: 6, padding: 16 }}>
          <AdvancedDashboard
            config={dashboard}
            data={data}
            loading={false}
            onRefresh={() => {
              message.info('Dashboard refreshed');
            }}
            onExport={(format) => {
              message.success(`Dashboard exported as ${format}`);
            }}
          />
        </div>
      ) : (
        <Alert
          message="No Dashboard Available"
          description="Please generate a dashboard first."
          type="warning"
          showIcon
        />
      )}
    </Card>
  );

  const steps = [
    {
      title: 'Configure',
      icon: <SettingOutlined />,
      content: renderConfigurationStep()
    },
    {
      title: 'Generate',
      icon: <ThunderboltOutlined />,
      content: renderGenerationStep()
    },
    {
      title: 'Preview',
      icon: <EyeOutlined />,
      content: renderPreviewStep()
    }
  ];

  return (
    <div className="advanced-dashboard-builder">
      <Card
        title={
          <Space>
            <DashboardOutlined />
            <Title level={4} style={{ margin: 0 }}>Advanced Dashboard Builder</Title>
            <Badge count={data.length} style={{ backgroundColor: '#52c41a' }} />
          </Space>
        }
        extra={
          <Space>
            <Button
              icon={<EyeOutlined />}
              onClick={() => setShowPreview(true)}
              disabled={!dashboard}
            >
              Quick Preview
            </Button>
            <Button
              icon={<SaveOutlined />}
              onClick={saveDashboard}
              disabled={!dashboard}
            >
              Save
            </Button>
          </Space>
        }
      >
        {!data.length || !query ? (
          <Alert
            message="No Data Available"
            description="Execute a query first to build an advanced dashboard."
            type="info"
            showIcon
          />
        ) : (
          <>
            <Steps
              current={currentStep}
              style={{ marginBottom: 24 }}
              onChange={setCurrentStep}
            >
              {steps.map((step, index) => (
                <Step
                  key={index}
                  title={step.title}
                  icon={step.icon}
                  disabled={index > currentStep && !dashboard}
                />
              ))}
            </Steps>

            {steps[currentStep].content}

            <div style={{ textAlign: 'center', marginTop: 16 }}>
              <Space>
                {currentStep > 0 && (
                  <Button onClick={() => setCurrentStep(currentStep - 1)}>
                    Previous
                  </Button>
                )}
                {currentStep < steps.length - 1 && (
                  <Button
                    type="primary"
                    onClick={() => {
                      if (currentStep === 0) {
                        form.validateFields().then(() => {
                          setCurrentStep(currentStep + 1);
                        });
                      } else {
                        setCurrentStep(currentStep + 1);
                      }
                    }}
                    disabled={currentStep === 1 && !dashboard}
                  >
                    Next
                  </Button>
                )}
              </Space>
            </div>
          </>
        )}
      </Card>

      {/* Preview Modal */}
      <Modal
        title="Dashboard Preview"
        open={showPreview}
        onCancel={() => setShowPreview(false)}
        width="90%"
        style={{ top: 20 }}
        footer={[
          <Button key="close" onClick={() => setShowPreview(false)}>
            Close
          </Button>,
          <Button key="save" type="primary" onClick={saveDashboard}>
            Save Dashboard
          </Button>
        ]}
      >
        {dashboard && (
          <AdvancedDashboard
            config={dashboard}
            data={data}
            loading={false}
            onRefresh={() => {
              message.info('Dashboard refreshed');
            }}
            onExport={(format) => {
              message.success(`Dashboard exported as ${format}`);
            }}
          />
        )}
      </Modal>
    </div>
  );
};

export default AdvancedDashboardBuilder;
