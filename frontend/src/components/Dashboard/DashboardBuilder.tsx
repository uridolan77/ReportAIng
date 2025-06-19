import React, { useState, useEffect } from 'react';
import {
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
  Tag,
  Tooltip,
} from 'antd';
import {
  PlusOutlined,
  SettingOutlined,
  SaveOutlined,
  ShareAltOutlined,
  DeleteOutlined,
  DragOutlined,
  BarChartOutlined,
  LineChartOutlined,
  PieChartOutlined,
  TableOutlined,
  DotChartOutlined,
  AreaChartOutlined,
  HomeOutlined,
  DashboardOutlined
} from '@ant-design/icons';
import { DragDropContext, Droppable, Draggable } from 'react-beautiful-dnd';
import { ResponsiveContainer, BarChart, Bar, LineChart, Line, PieChart, Pie, Cell, XAxis, YAxis, CartesianGrid, Tooltip as RechartsTooltip, Legend } from 'recharts';
import { useDashboardResult } from '../../hooks/useCurrentResult';
import { PageLayout, PageSection, PageGrid } from '../core/Layouts';
import { Card } from '../core/Card';

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
  // Use global result system for dashboard creation
  const { result, hasResult, canCreateDashboard, suggestedChartTypes } = useDashboardResult();

  // Debug logging in development
  useEffect(() => {
    if (process.env.NODE_ENV === 'development') {
      console.log('🎯 DashboardBuilder - Result status:', {
        hasResult,
        canCreateDashboard,
        suggestedChartTypesCount: suggestedChartTypes.length,
        resultSource: result?.source,
        resultSuccess: result?.result?.success,
        dataLength: result?.result?.data?.length
      });
    }
  }, [hasResult, canCreateDashboard, result]);

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
  const [previewMode, setPreviewMode] = useState(false);
  const [selectedDataSource, setSelectedDataSource] = useState<string>('');
  const [, setSelectedChart] = useState<ChartConfig | null>(null);


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

      // Load from global result system
      if (hasResult && result?.result?.data) {
        sources.push({
          data: result.result.data,
          columns: result.result.metadata?.columns?.map((col: any) => col.name || col) || [],
          query: result.query || 'Current Query Result',
          timestamp: Date.now()
        });
      }

      // Load from localStorage as fallback
      const storedResult = localStorage.getItem('current-query-result');
      if (storedResult) {
        try {
          const parsed = JSON.parse(storedResult);
          sources.push({
            data: parsed.data || [],
            columns: parsed.columns || [],
            query: parsed.query || 'Stored Query Result',
            timestamp: parsed.timestamp || Date.now()
          });
        } catch (error) {
          console.error('Error parsing stored data:', error);
        }
      }

      // TODO: Replace with actual data sources from database

      sources.push({
        data: generateSampleData('users'),
        columns: ['date', 'active_users', 'new_signups', 'churn_rate'],
        query: 'Sample User Analytics',
        timestamp: Date.now() - 172800000
      });

      setAvailableData(sources);
    };

    loadDataSources();
  }, [hasResult, result]);

  // TODO: Replace with actual data loading from database

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
    setSelectedDataSource('');
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
    return (dataSource && Array.isArray(dataSource.data)) ? dataSource.data : [];
  };

  // Render chart
  const renderChart = (chart: ChartConfig) => {
    try {
      const data = getChartData(chart);
      if (!data || !Array.isArray(data) || data.length === 0) {
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
              <XAxis dataKey={chart.xAxis || 'name'} />
              <YAxis />
              <RechartsTooltip />
              <Legend />
              <Bar dataKey={chart.yAxis || 'value'} fill="#1890ff" />
            </BarChart>
          </ResponsiveContainer>
        );

      case 'line':
        return (
          <ResponsiveContainer width="100%" height={300}>
            <LineChart data={chartData}>
              <CartesianGrid strokeDasharray="3 3" />
              <XAxis dataKey={chart.xAxis || 'name'} />
              <YAxis />
              <RechartsTooltip />
              <Legend />
              <Line type="monotone" dataKey={chart.yAxis || 'value'} stroke="#1890ff" />
            </LineChart>
          </ResponsiveContainer>
        );

      case 'pie':
        return (
          <ResponsiveContainer width="100%" height={300}>
            <PieChart>
              <Pie
                data={chartData}
                dataKey={chart.yAxis || 'value'}
                nameKey={chart.xAxis || 'name'}
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
    } catch (error) {
      console.error('Error rendering chart:', error);
      return (
        <div style={{ padding: 40, textAlign: 'center' }}>
          <Text type="danger">Error rendering chart</Text>
        </div>
      );
    }
  };

  return (
    <PageLayout
      title="Dashboard Builder"
      subtitle="Create interactive dashboards from your query results"
      breadcrumbs={[
        { title: 'Home', href: '/', icon: <HomeOutlined /> },
        { title: 'Dashboard Builder', icon: <DashboardOutlined /> }
      ]}
      maxWidth="xl"
      actions={
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
      }
    >
      {/* Dashboard Status */}
      <PageSection
        title={dashboardConfig.title}
        subtitle={dashboardConfig.description}
        background="card"
        padding="lg"
      >
        <div style={{ display: 'flex', gap: 'var(--space-2)', flexWrap: 'wrap' }}>
          {hasResult && (
            <Tag color="green">
              ✓ Current Result Available ({result?.result?.data?.length || 0} rows)
            </Tag>
          )}
          {canCreateDashboard && (
            <Tag color="blue">
              Dashboard Ready
            </Tag>
          )}
          {suggestedChartTypes.length > 0 && (
            <Tag color="purple">
              {suggestedChartTypes.length} Chart Types Available
            </Tag>
          )}
          {/* Debug info in development */}
          {process.env.NODE_ENV === 'development' && (
            <Tag color="orange">
              Debug: hasResult={String(hasResult)}, source={result?.source || 'none'}
            </Tag>
          )}
        </div>
      </PageSection>

      {/* Add Chart Modal */}
      <Modal
        title="Add New Chart"
        open={isAddChartModalVisible}
        onCancel={() => {
          setIsAddChartModalVisible(false);
          setSelectedDataSource('');
        }}
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
            <Select
              placeholder="Select data source"
              onChange={(value) => setSelectedDataSource(value)}
            >
              {availableData.map((source, index) => (
                <Option key={index} value={source.query}>
                  {source.query} ({source.data?.length || 0} rows)
                </Option>
              ))}
            </Select>
          </Form.Item>

          <Row gutter={16}>
            <Col span={12}>
              <Form.Item name="xAxis" label="X-Axis Column">
                <Select placeholder="Select X-axis">
                  {(() => {
                    const selectedSource = availableData.find(d => d.query === selectedDataSource);
                    return selectedSource?.columns?.map(col => (
                      <Option key={col} value={col}>{col}</Option>
                    )) || [];
                  })()}
                </Select>
              </Form.Item>
            </Col>
            <Col span={12}>
              <Form.Item name="yAxis" label="Y-Axis Column">
                <Select placeholder="Select Y-axis">
                  {(() => {
                    const selectedSource = availableData.find(d => d.query === selectedDataSource);
                    return selectedSource?.columns?.map(col => (
                      <Option key={col} value={col}>{col}</Option>
                    )) || [];
                  })()}
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
              <Button onClick={() => {
                setIsAddChartModalVisible(false);
                setSelectedDataSource('');
              }}>
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
      <PageSection
        title="Dashboard Statistics"
        subtitle="Overview of your dashboard configuration"
        background="transparent"
        padding="none"
      >
        <PageGrid columns={4} gap="md">
          <Card variant="outlined" padding="medium">
            <div style={{ textAlign: 'center' }}>
              <Text type="secondary" style={{ display: 'block', marginBottom: 'var(--space-2)' }}>
                Total Charts
              </Text>
              <div style={{
                fontSize: 'var(--text-2xl)',
                fontWeight: 'var(--font-weight-bold)',
                color: 'var(--color-primary)'
              }}>
                {dashboardConfig.charts.length}
              </div>
            </div>
          </Card>
          <Card variant="outlined" padding="medium">
            <div style={{ textAlign: 'center' }}>
              <Text type="secondary" style={{ display: 'block', marginBottom: 'var(--space-2)' }}>
                Data Sources
              </Text>
              <div style={{
                fontSize: 'var(--text-2xl)',
                fontWeight: 'var(--font-weight-bold)',
                color: 'var(--color-success)'
              }}>
                {availableData.length}
              </div>
            </div>
          </Card>
          <Card variant="outlined" padding="medium">
            <div style={{ textAlign: 'center' }}>
              <Text type="secondary" style={{ display: 'block', marginBottom: 'var(--space-2)' }}>
                Layout
              </Text>
              <div style={{
                fontSize: 'var(--text-lg)',
                fontWeight: 'var(--font-weight-bold)',
                color: 'var(--color-secondary)'
              }}>
                {dashboardConfig.layout}
              </div>
            </div>
          </Card>
          <Card variant="outlined" padding="medium">
            <div style={{ textAlign: 'center' }}>
              <Text type="secondary" style={{ display: 'block', marginBottom: 'var(--space-2)' }}>
                Auto Refresh
              </Text>
              <div style={{
                fontSize: 'var(--text-lg)',
                fontWeight: 'var(--font-weight-bold)',
                color: dashboardConfig.autoRefresh ? 'var(--color-success)' : 'var(--color-error)'
              }}>
                {dashboardConfig.autoRefresh ? `${dashboardConfig.refreshInterval}s` : 'Off'}
              </div>
            </div>
          </Card>
        </PageGrid>
      </PageSection>

      {/* Charts Grid */}
      <PageSection
        title="Dashboard Charts"
        subtitle="Drag and drop to rearrange your charts"
        background="transparent"
        padding="none"
      >
        {dashboardConfig.charts.length === 0 ? (
          <Card variant="outlined" padding="large">
            <div style={{ textAlign: 'center', padding: 'var(--space-16) 0' }}>
              <BarChartOutlined style={{
                fontSize: '64px',
                color: 'var(--text-tertiary)',
                marginBottom: 'var(--space-4)'
              }} />
              <Typography.Title level={4} style={{ color: 'var(--text-secondary)' }}>
                No Charts Added
              </Typography.Title>
              <Text type="secondary" style={{ display: 'block', marginBottom: 'var(--space-4)' }}>
                Start building your dashboard by adding charts
              </Text>
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
                            variant="outlined"
                            hover
                            title={
                              <div style={{ display: 'flex', alignItems: 'center', gap: 'var(--space-2)' }}>
                                <div {...provided.dragHandleProps}>
                                  <DragOutlined style={{ cursor: 'grab', color: 'var(--text-tertiary)' }} />
                                </div>
                                <span style={{ color: 'var(--text-primary)' }}>{chart.title}</span>
                                <Tag color="blue">{chart.type}</Tag>
                              </div>
                            }
                            actions={
                              !previewMode ? [
                                <Tooltip key="settings" title="Chart Settings">
                                  <Button
                                    size="small"
                                    icon={<SettingOutlined />}
                                    onClick={() => setSelectedChart(chart)}
                                  />
                                </Tooltip>,
                                <Tooltip key="remove" title="Remove Chart">
                                  <Button
                                    size="small"
                                    danger
                                    icon={<DeleteOutlined />}
                                    onClick={() => handleRemoveChart(chart.id)}
                                  />
                                </Tooltip>
                              ] : undefined
                            }
                            style={{ height: '400px' }}
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
      </PageSection>
    </PageLayout>
  );
};

export default DashboardBuilder;
