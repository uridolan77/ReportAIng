import React, { useState, useEffect, useCallback } from 'react';
import {
  Card,
  Row,
  Col,
  Typography,
  Button,
  Space,
  Select,
  Input,
  Modal,
  Form,
  Switch,
  Slider,
  Alert,
  Tooltip,
  Divider,
  Tag,
  Spin,
} from 'antd';
import {
  PlusOutlined,
  SettingOutlined,
  SaveOutlined,
  ShareAltOutlined,
  FullscreenOutlined,
  DeleteOutlined,
  DragOutlined,
  BarChartOutlined,
  LineChartOutlined,
  PieChartOutlined,
  TableOutlined,
  DotChartOutlined,
  HeatMapOutlined,
  FundOutlined,
  AreaChartOutlined,
} from '@ant-design/icons';
import { DragDropContext, Droppable, Draggable } from 'react-beautiful-dnd';
import { ResponsiveContainer, BarChart, Bar, LineChart, Line, PieChart, Pie, Cell, XAxis, YAxis, CartesianGrid, Tooltip as RechartsTooltip, Legend } from 'recharts';

const { Title, Text } = Typography;
const { Option } = Select;
const { TextArea } = Input;

interface ChartConfig {
  id: string;
  type: 'bar' | 'line' | 'pie' | 'table' | 'scatter' | 'area';
  title: string;
  dataSource: string;
  xAxis?: string;
  yAxis?: string;
  size: 'small' | 'medium' | 'large' | 'full';
  position: { x: number; y: number };
  config: Record<string, any>;
}

interface DashboardConfig {
  id: string;
  title: string;
  description: string;
  charts: ChartConfig[];
  layout: 'grid' | 'freeform';
  theme: 'light' | 'dark';
  autoRefresh: boolean;
  refreshInterval: number;
}

interface StoredData {
  data: any[];
  columns: string[];
  query: string;
  timestamp: number;
}

const DashboardBuilder: React.FC = () => {
  const [dashboardConfig, setDashboardConfig] = useState<DashboardConfig>({
    id: 'dashboard-' + Date.now(),
    title: 'New Dashboard',
    description: 'Interactive dashboard created with AI assistance',
    charts: [],
    layout: 'grid',
    theme: 'light',
    autoRefresh: false,
    refreshInterval: 30,
  });

  const [availableData, setAvailableData] = useState<StoredData[]>([]);
  const [isAddChartModalVisible, setIsAddChartModalVisible] = useState(false);
  const [isSettingsModalVisible, setIsSettingsModalVisible] = useState(false);
  const [selectedChart, setSelectedChart] = useState<ChartConfig | null>(null);
  const [loading, setLoading] = useState(false);
  const [previewMode, setPreviewMode] = useState(false);

  // Chart type options
  const chartTypes = [
    { value: 'bar', label: 'Bar Chart', icon: <BarChartOutlined /> },
    { value: 'line', label: 'Line Chart', icon: <LineChartOutlined /> },
    { value: 'pie', label: 'Pie Chart', icon: <PieChartOutlined /> },
    { value: 'area', label: 'Area Chart', icon: <AreaChartOutlined /> },
    { value: 'scatter', label: 'Scatter Plot', icon: <DotChartOutlined /> },
    { value: 'table', label: 'Data Table', icon: <TableOutlined /> },
  ];

  // Load available data sources
  useEffect(() => {
    const loadDataSources = () => {
      const sources: StoredData[] = [];
      
      // Load from localStorage
      const storedResult = localStorage.getItem('current-query-result');
      if (storedResult) {
        try {
          const parsed = JSON.parse(storedResult);
          sources.push({
            ...parsed,
            query: parsed.query || 'Current Query Result'
          });
        } catch (error) {
          console.error('Error parsing stored data:', error);
        }
      }

      // Add sample data sources
      sources.push({
        data: generateSampleData('sales'),
        columns: ['month', 'revenue', 'customers', 'region'],
        query: 'Sample Sales Data',
        timestamp: Date.now() - 86400000
      });

      sources.push({
        data: generateSampleData('users'),
        columns: ['date', 'active_users', 'new_signups', 'churn_rate'],
        query: 'Sample User Analytics',
        timestamp: Date.now() - 172800000
      });

      setAvailableData(sources);
    };

    loadDataSources();
  }, []);

  // Generate sample data
  const generateSampleData = (type: string) => {
    const data = [];
    const months = ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun'];
    
    if (type === 'sales') {
      const regions = ['North', 'South', 'East', 'West'];
      for (let i = 0; i < 6; i++) {
        for (const region of regions) {
          data.push({
            month: months[i],
            revenue: Math.floor(Math.random() * 100000) + 50000,
            customers: Math.floor(Math.random() * 1000) + 500,
            region
          });
        }
      }
    } else if (type === 'users') {
      for (let i = 0; i < 30; i++) {
        data.push({
          date: `2024-01-${String(i + 1).padStart(2, '0')}`,
          active_users: Math.floor(Math.random() * 10000) + 5000,
          new_signups: Math.floor(Math.random() * 500) + 100,
          churn_rate: Math.random() * 5 + 1
        });
      }
    }
    
    return data;
  };

  // Add new chart
  const handleAddChart = (values: any) => {
    const newChart: ChartConfig = {
      id: 'chart-' + Date.now(),
      type: values.type,
      title: values.title,
      dataSource: values.dataSource,
      xAxis: values.xAxis,
      yAxis: values.yAxis,
      size: values.size || 'medium',
      position: { x: 0, y: dashboardConfig.charts.length },
      config: {}
    };

    setDashboardConfig(prev => ({
      ...prev,
      charts: [...prev.charts, newChart]
    }));

    setIsAddChartModalVisible(false);
  };

  // Remove chart
  const handleRemoveChart = (chartId: string) => {
    setDashboardConfig(prev => ({
      ...prev,
      charts: prev.charts.filter(chart => chart.id !== chartId)
    }));
  };

  // Handle drag end
  const handleDragEnd = (result: any) => {
    if (!result.destination) return;

    const items = Array.from(dashboardConfig.charts);
    const [reorderedItem] = items.splice(result.source.index, 1);
    items.splice(result.destination.index, 0, reorderedItem);

    setDashboardConfig(prev => ({
      ...prev,
      charts: items
    }));
  };

  // Get data for chart
  const getChartData = (chart: ChartConfig) => {
    const dataSource = availableData.find(d => d.query === chart.dataSource);
    return dataSource?.data || [];
  };

  // Render chart
  const renderChart = (chart: ChartConfig) => {
    const data = getChartData(chart);
    if (!data.length) {
      return (
        <div style={{ padding: 40, textAlign: 'center' }}>
          <Text type="secondary">No data available</Text>
        </div>
      );
    }

    const chartData = data.slice(0, 100); // Limit for performance

    switch (chart.type) {
      case 'bar':
        return (
          <ResponsiveContainer width="100%" height={300}>
            <BarChart data={chartData}>
              <CartesianGrid strokeDasharray="3 3" />
              <XAxis dataKey={chart.xAxis} />
              <YAxis />
              <RechartsTooltip />
              <Legend />
              <Bar dataKey={chart.yAxis} fill="#1890ff" />
            </BarChart>
          </ResponsiveContainer>
        );

      case 'line':
        return (
          <ResponsiveContainer width="100%" height={300}>
            <LineChart data={chartData}>
              <CartesianGrid strokeDasharray="3 3" />
              <XAxis dataKey={chart.xAxis} />
              <YAxis />
              <RechartsTooltip />
              <Legend />
              <Line type="monotone" dataKey={chart.yAxis} stroke="#1890ff" />
            </LineChart>
          </ResponsiveContainer>
        );

      case 'pie':
        return (
          <ResponsiveContainer width="100%" height={300}>
            <PieChart>
              <Pie
                data={chartData}
                dataKey={chart.yAxis}
                nameKey={chart.xAxis}
                cx="50%"
                cy="50%"
                outerRadius={100}
                fill="#1890ff"
              >
                {chartData.map((entry, index) => (
                  <Cell key={`cell-${index}`} fill={`hsl(${index * 45}, 70%, 60%)`} />
                ))}
              </Pie>
              <RechartsTooltip />
              <Legend />
            </PieChart>
          </ResponsiveContainer>
        );

      default:
        return (
          <div style={{ padding: 40, textAlign: 'center' }}>
            <Text>Chart type "{chart.type}" not implemented</Text>
          </div>
        );
    }
  };

  return (
    <div style={{ padding: 24 }}>
      {/* Dashboard Header */}
      <Card style={{ marginBottom: 16 }}>
        <Row justify="space-between" align="middle">
          <Col>
            <Title level={3} style={{ margin: 0 }}>
              {dashboardConfig.title}
            </Title>
            <Text type="secondary">{dashboardConfig.description}</Text>
          </Col>
          <Col>
            <Space>
              <Button
                type="primary"
                icon={<PlusOutlined />}
                onClick={() => setIsAddChartModalVisible(true)}
              >
                Add Chart
              </Button>
              <Button
                icon={<SettingOutlined />}
                onClick={() => setIsSettingsModalVisible(true)}
              >
                Settings
              </Button>
              <Button icon={<SaveOutlined />}>
                Save Dashboard
              </Button>
              <Button icon={<ShareAltOutlined />}>
                Share
              </Button>
              <Switch
                checkedChildren="Preview"
                unCheckedChildren="Edit"
                checked={previewMode}
                onChange={setPreviewMode}
              />
            </Space>
          </Col>
        </Row>
      </Card>

      {/* Add Chart Modal */}
      <Modal
        title="Add New Chart"
        open={isAddChartModalVisible}
        onCancel={() => setIsAddChartModalVisible(false)}
        footer={null}
        width={600}
      >
        <Form
          layout="vertical"
          onFinish={handleAddChart}
          initialValues={{ size: 'medium' }}
        >
          <Form.Item
            name="title"
            label="Chart Title"
            rules={[{ required: true, message: 'Please enter chart title' }]}
          >
            <Input placeholder="Enter chart title" />
          </Form.Item>

          <Form.Item
            name="type"
            label="Chart Type"
            rules={[{ required: true, message: 'Please select chart type' }]}
          >
            <Select placeholder="Select chart type">
              {chartTypes.map(type => (
                <Option key={type.value} value={type.value}>
                  <Space>
                    {type.icon}
                    {type.label}
                  </Space>
                </Option>
              ))}
            </Select>
          </Form.Item>

          <Form.Item
            name="dataSource"
            label="Data Source"
            rules={[{ required: true, message: 'Please select data source' }]}
          >
            <Select placeholder="Select data source">
              {availableData.map((source, index) => (
                <Option key={index} value={source.query}>
                  {source.query} ({source.data.length} rows)
                </Option>
              ))}
            </Select>
          </Form.Item>

          <Row gutter={16}>
            <Col span={12}>
              <Form.Item name="xAxis" label="X-Axis Column">
                <Select placeholder="Select X-axis">
                  {availableData[0]?.columns.map(col => (
                    <Option key={col} value={col}>{col}</Option>
                  ))}
                </Select>
              </Form.Item>
            </Col>
            <Col span={12}>
              <Form.Item name="yAxis" label="Y-Axis Column">
                <Select placeholder="Select Y-axis">
                  {availableData[0]?.columns.map(col => (
                    <Option key={col} value={col}>{col}</Option>
                  ))}
                </Select>
              </Form.Item>
            </Col>
          </Row>

          <Form.Item name="size" label="Chart Size">
            <Select>
              <Option value="small">Small (1/3 width)</Option>
              <Option value="medium">Medium (1/2 width)</Option>
              <Option value="large">Large (2/3 width)</Option>
              <Option value="full">Full Width</Option>
            </Select>
          </Form.Item>

          <Form.Item>
            <Space>
              <Button type="primary" htmlType="submit">
                Add Chart
              </Button>
              <Button onClick={() => setIsAddChartModalVisible(false)}>
                Cancel
              </Button>
            </Space>
          </Form.Item>
        </Form>
      </Modal>

      {/* Dashboard Settings Modal */}
      <Modal
        title="Dashboard Settings"
        open={isSettingsModalVisible}
        onCancel={() => setIsSettingsModalVisible(false)}
        footer={null}
        width={500}
      >
        <Form
          layout="vertical"
          initialValues={dashboardConfig}
          onFinish={(values) => {
            setDashboardConfig(prev => ({ ...prev, ...values }));
            setIsSettingsModalVisible(false);
          }}
        >
          <Form.Item name="title" label="Dashboard Title">
            <Input />
          </Form.Item>

          <Form.Item name="description" label="Description">
            <TextArea rows={3} />
          </Form.Item>

          <Form.Item name="layout" label="Layout Type">
            <Select>
              <Option value="grid">Grid Layout</Option>
              <Option value="freeform">Freeform Layout</Option>
            </Select>
          </Form.Item>

          <Form.Item name="theme" label="Theme">
            <Select>
              <Option value="light">Light Theme</Option>
              <Option value="dark">Dark Theme</Option>
            </Select>
          </Form.Item>

          <Form.Item name="autoRefresh" label="Auto Refresh" valuePropName="checked">
            <Switch />
          </Form.Item>

          <Form.Item name="refreshInterval" label="Refresh Interval (seconds)">
            <Slider min={10} max={300} marks={{ 10: '10s', 60: '1m', 300: '5m' }} />
          </Form.Item>

          <Form.Item>
            <Space>
              <Button type="primary" htmlType="submit">
                Save Settings
              </Button>
              <Button onClick={() => setIsSettingsModalVisible(false)}>
                Cancel
              </Button>
            </Space>
          </Form.Item>
        </Form>
      </Modal>

      {/* Dashboard Stats */}
      <Row gutter={16} style={{ marginBottom: 16 }}>
        <Col span={6}>
          <Card size="small">
            <Text type="secondary">Total Charts</Text>
            <div style={{ fontSize: 24, fontWeight: 'bold', color: '#1890ff' }}>
              {dashboardConfig.charts.length}
            </div>
          </Card>
        </Col>
        <Col span={6}>
          <Card size="small">
            <Text type="secondary">Data Sources</Text>
            <div style={{ fontSize: 24, fontWeight: 'bold', color: '#52c41a' }}>
              {availableData.length}
            </div>
          </Card>
        </Col>
        <Col span={6}>
          <Card size="small">
            <Text type="secondary">Layout</Text>
            <div style={{ fontSize: 16, fontWeight: 'bold', color: '#722ed1' }}>
              {dashboardConfig.layout}
            </div>
          </Card>
        </Col>
        <Col span={6}>
          <Card size="small">
            <Text type="secondary">Auto Refresh</Text>
            <div style={{ fontSize: 16, fontWeight: 'bold', color: dashboardConfig.autoRefresh ? '#52c41a' : '#ff4d4f' }}>
              {dashboardConfig.autoRefresh ? `${dashboardConfig.refreshInterval}s` : 'Off'}
            </div>
          </Card>
        </Col>
      </Row>

      {/* Charts Grid */}
      {dashboardConfig.charts.length === 0 ? (
        <Card style={{ textAlign: 'center', padding: 60 }}>
          <BarChartOutlined style={{ fontSize: 64, color: '#d9d9d9', marginBottom: 16 }} />
          <Title level={4} type="secondary">No Charts Added</Title>
          <Text type="secondary">Start building your dashboard by adding charts</Text>
          <div style={{ marginTop: 16 }}>
            <Button
              type="primary"
              icon={<PlusOutlined />}
              onClick={() => setIsAddChartModalVisible(true)}
            >
              Add Your First Chart
            </Button>
          </div>
        </Card>
      ) : (
        <DragDropContext onDragEnd={handleDragEnd}>
          <Droppable droppableId="dashboard">
            {(provided) => (
              <div {...provided.droppableProps} ref={provided.innerRef}>
                <Row gutter={16}>
                  {dashboardConfig.charts.map((chart, index) => (
                    <Draggable key={chart.id} draggableId={chart.id} index={index}>
                      {(provided, snapshot) => (
                        <Col
                          span={chart.size === 'full' ? 24 : chart.size === 'large' ? 16 : chart.size === 'medium' ? 12 : 8}
                          ref={provided.innerRef}
                          {...provided.draggableProps}
                          style={{
                            marginBottom: 16,
                            ...provided.draggableProps.style,
                            opacity: snapshot.isDragging ? 0.8 : 1
                          }}
                        >
                          <Card
                            title={
                              <Space>
                                <div {...provided.dragHandleProps}>
                                  <DragOutlined style={{ cursor: 'grab' }} />
                                </div>
                                {chart.title}
                                <Tag color="blue">{chart.type}</Tag>
                              </Space>
                            }
                            extra={
                              !previewMode && (
                                <Space>
                                  <Tooltip title="Chart Settings">
                                    <Button
                                      size="small"
                                      icon={<SettingOutlined />}
                                      onClick={() => setSelectedChart(chart)}
                                    />
                                  </Tooltip>
                                  <Tooltip title="Remove Chart">
                                    <Button
                                      size="small"
                                      danger
                                      icon={<DeleteOutlined />}
                                      onClick={() => handleRemoveChart(chart.id)}
                                    />
                                  </Tooltip>
                                </Space>
                              )
                            }
                            style={{ height: 400 }}
                          >
                            {renderChart(chart)}
                          </Card>
                        </Col>
                      )}
                    </Draggable>
                  ))}
                </Row>
                {provided.placeholder}
              </div>
            )}
          </Droppable>
        </DragDropContext>
      )}
    </div>
  );
};

export default DashboardBuilder;
