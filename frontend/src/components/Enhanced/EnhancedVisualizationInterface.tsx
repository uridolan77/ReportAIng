import React, { useState, useEffect, useMemo } from 'react';
import { 
  Card, 
  Button, 
  Select, 
  Row, 
  Col, 
  Typography, 
  Space, 
  Tabs, 
  Switch, 
  Slider,
  Tooltip,
  Alert,
  Spin,
  Badge,
  Divider,
  Input,
  Collapse
} from 'antd';
import { 
  BarChartOutlined,
  LineChartOutlined,
  PieChartOutlined,
  DotChartOutlined,
  HeatMapOutlined,
  AreaChartOutlined,
  NodeIndexOutlined,
  BranchesOutlined,
  SettingOutlined,
  DownloadOutlined,
  FullscreenOutlined,
  ReloadOutlined,
  EyeOutlined,
  BulbOutlined,
  ThunderboltOutlined
} from '@ant-design/icons';

// Import D3 chart components (with error handling)
const HeatmapChart = React.lazy(() =>
  import('../Visualization/D3Charts/HeatmapChart').then(m => ({ default: m.HeatmapChart })).catch(() => ({ default: () => <div>Heatmap chart not available</div> }))
);
const NetworkChart = React.lazy(() =>
  import('../Visualization/D3Charts/NetworkChart').then(m => ({ default: m.NetworkChart })).catch(() => ({ default: () => <div>Network chart not available</div> }))
);
// const SankeyChart = React.lazy(() =>
//   import('../Visualization/D3Charts/SankeyChart').then(m => ({ default: m.SankeyChart })).catch(() => ({ default: () => <div>Sankey chart not available</div> }))
// );
const TreemapChart = React.lazy(() =>
  import('../Visualization/D3Charts/TreemapChart').then(m => ({ default: m.TreemapChart })).catch(() => ({ default: () => <div>Treemap chart not available</div> }))
);

// Import Recharts for standard charts
import {
  ResponsiveContainer,
  BarChart,
  Bar,
  LineChart,
  Line,
  PieChart,
  Pie,
  Cell,
  ScatterChart,
  Scatter,
  AreaChart,
  Area,
  XAxis,
  YAxis,
  CartesianGrid,
  Tooltip as RechartsTooltip,
  Legend,
  Brush
} from 'recharts';

const { Title, Text } = Typography;
const { Option } = Select;
const { TabPane } = Tabs;
const { Panel } = Collapse;

interface VisualizationConfig {
  type: string;
  title: string;
  xAxis?: string;
  yAxis?: string;
  colorBy?: string;
  size?: string;
  aggregation?: string;
  showLegend: boolean;
  showGrid: boolean;
  showAnimation: boolean;
  colorScheme: string;
  interactive: boolean;
}

interface ChartData {
  data: any[];
  columns: string[];
  metadata: {
    totalRows: number;
    dataTypes: Record<string, string>;
    summary: Record<string, any>;
  };
}

const CHART_TYPES = [
  { value: 'bar', label: 'Bar Chart', icon: <BarChartOutlined />, category: 'Standard' },
  { value: 'line', label: 'Line Chart', icon: <LineChartOutlined />, category: 'Standard' },
  { value: 'area', label: 'Area Chart', icon: <AreaChartOutlined />, category: 'Standard' },
  { value: 'pie', label: 'Pie Chart', icon: <PieChartOutlined />, category: 'Standard' },
  { value: 'scatter', label: 'Scatter Plot', icon: <DotChartOutlined />, category: 'Standard' },
  { value: 'heatmap', label: 'Heatmap', icon: <HeatMapOutlined />, category: 'Advanced' },
  { value: 'network', label: 'Network Graph', icon: <NodeIndexOutlined />, category: 'Advanced' },
  // { value: 'sankey', label: 'Sankey Diagram', icon: <BranchesOutlined />, category: 'Advanced' }, // Temporarily disabled - requires d3-sankey
  { value: 'treemap', label: 'Treemap', icon: <AreaChartOutlined />, category: 'Advanced' }
];

const COLOR_SCHEMES = [
  { value: 'default', label: 'Default', colors: ['#1890ff', '#52c41a', '#faad14', '#f5222d', '#722ed1'] },
  { value: 'viridis', label: 'Viridis', colors: ['#440154', '#31688e', '#35b779', '#fde725'] },
  { value: 'plasma', label: 'Plasma', colors: ['#0d0887', '#7e03a8', '#cc4778', '#f89441', '#f0f921'] },
  { value: 'cool', label: 'Cool', colors: ['#6baed6', '#4292c6', '#2171b5', '#08519c', '#08306b'] },
  { value: 'warm', label: 'Warm', colors: ['#fd8d3c', '#fc4e2a', '#e31a1c', '#bd0026', '#800026'] }
];

export const EnhancedVisualizationInterface: React.FC = () => {
  const [chartData, setChartData] = useState<ChartData | null>(null);
  const [config, setConfig] = useState<VisualizationConfig>({
    type: 'bar',
    title: 'Enhanced Visualization',
    showLegend: true,
    showGrid: true,
    showAnimation: true,
    colorScheme: 'default',
    interactive: true
  });
  const [loading, setLoading] = useState(false);
  const [fullscreen, setFullscreen] = useState(false);

  // Load sample data for demonstration
  useEffect(() => {
    loadSampleData();
  }, []);

  const loadSampleData = () => {
    // Generate sample data for demonstration
    const sampleData = Array.from({ length: 20 }, (_, i) => ({
      name: `Item ${i + 1}`,
      value: Math.floor(Math.random() * 100) + 10,
      category: ['A', 'B', 'C'][i % 3],
      revenue: Math.floor(Math.random() * 10000) + 1000,
      count: Math.floor(Math.random() * 50) + 5,
      date: new Date(2024, 0, i + 1).toISOString().split('T')[0]
    }));

    setChartData({
      data: sampleData,
      columns: ['name', 'value', 'category', 'revenue', 'count', 'date'],
      metadata: {
        totalRows: sampleData.length,
        dataTypes: {
          name: 'string',
          value: 'number',
          category: 'string',
          revenue: 'number',
          count: 'number',
          date: 'date'
        },
        summary: {
          totalValue: sampleData.reduce((sum, item) => sum + item.value, 0),
          avgRevenue: sampleData.reduce((sum, item) => sum + item.revenue, 0) / sampleData.length
        }
      }
    });
  };

  const getColorScheme = (schemeName: string) => {
    const scheme = COLOR_SCHEMES.find(s => s.value === schemeName);
    return scheme?.colors || COLOR_SCHEMES[0].colors;
  };

  const renderStandardChart = () => {
    if (!chartData) return null;

    const colors = getColorScheme(config.colorScheme);
    const commonProps = {
      width: '100%',
      height: fullscreen ? 600 : 400
    };

    switch (config.type) {
      case 'bar':
        return (
          <ResponsiveContainer {...commonProps}>
            <BarChart data={chartData.data}>
              {config.showGrid && <CartesianGrid strokeDasharray="3 3" />}
              <XAxis dataKey={config.xAxis || 'name'} />
              <YAxis />
              <RechartsTooltip />
              {config.showLegend && <Legend />}
              <Bar 
                dataKey={config.yAxis || 'value'} 
                fill={colors[0]}
                animationDuration={config.showAnimation ? 1000 : 0}
              />
              <Brush dataKey={config.xAxis || 'name'} height={30} />
            </BarChart>
          </ResponsiveContainer>
        );

      case 'line':
        return (
          <ResponsiveContainer {...commonProps}>
            <LineChart data={chartData.data}>
              {config.showGrid && <CartesianGrid strokeDasharray="3 3" />}
              <XAxis dataKey={config.xAxis || 'name'} />
              <YAxis />
              <RechartsTooltip />
              {config.showLegend && <Legend />}
              <Line 
                type="monotone" 
                dataKey={config.yAxis || 'value'} 
                stroke={colors[0]}
                strokeWidth={2}
                animationDuration={config.showAnimation ? 1000 : 0}
              />
              <Brush dataKey={config.xAxis || 'name'} height={30} />
            </LineChart>
          </ResponsiveContainer>
        );

      case 'area':
        return (
          <ResponsiveContainer {...commonProps}>
            <AreaChart data={chartData.data}>
              {config.showGrid && <CartesianGrid strokeDasharray="3 3" />}
              <XAxis dataKey={config.xAxis || 'name'} />
              <YAxis />
              <RechartsTooltip />
              {config.showLegend && <Legend />}
              <Area 
                type="monotone" 
                dataKey={config.yAxis || 'value'} 
                stroke={colors[0]}
                fill={colors[0]}
                fillOpacity={0.6}
                animationDuration={config.showAnimation ? 1000 : 0}
              />
              <Brush dataKey={config.xAxis || 'name'} height={30} />
            </AreaChart>
          </ResponsiveContainer>
        );

      case 'pie':
        return (
          <ResponsiveContainer {...commonProps}>
            <PieChart>
              <Pie
                data={chartData.data}
                dataKey={config.yAxis || 'value'}
                nameKey={config.xAxis || 'name'}
                cx="50%"
                cy="50%"
                outerRadius={fullscreen ? 200 : 120}
                fill={colors[0]}
                animationDuration={config.showAnimation ? 1000 : 0}
              >
                {chartData.data.map((_, index) => (
                  <Cell key={`cell-${index}`} fill={colors[index % colors.length]} />
                ))}
              </Pie>
              <RechartsTooltip />
              {config.showLegend && <Legend />}
            </PieChart>
          </ResponsiveContainer>
        );

      case 'scatter':
        return (
          <ResponsiveContainer {...commonProps}>
            <ScatterChart data={chartData.data}>
              {config.showGrid && <CartesianGrid strokeDasharray="3 3" />}
              <XAxis dataKey={config.xAxis || 'value'} />
              <YAxis dataKey={config.yAxis || 'revenue'} />
              <RechartsTooltip />
              {config.showLegend && <Legend />}
              <Scatter 
                fill={colors[0]}
                animationDuration={config.showAnimation ? 1000 : 0}
              />
            </ScatterChart>
          </ResponsiveContainer>
        );

      default:
        return <div>Chart type not supported</div>;
    }
  };

  const renderAdvancedChart = () => {
    if (!chartData) return null;

    const chartProps = {
      data: chartData.data,
      config: {
        ...config,
        colors: getColorScheme(config.colorScheme),
        width: fullscreen ? 800 : 600,
        height: fullscreen ? 600 : 400
      }
    };

    try {
      switch (config.type) {
        case 'heatmap':
          return (
            <React.Suspense fallback={<Spin size="large" />}>
              <HeatmapChart {...chartProps} />
            </React.Suspense>
          );
        case 'network':
          return (
            <React.Suspense fallback={<Spin size="large" />}>
              <NetworkChart {...chartProps} />
            </React.Suspense>
          );
        // case 'sankey': // Temporarily disabled - requires d3-sankey package
        //   return (
        //     <React.Suspense fallback={<Spin size="large" />}>
        //       <SankeyChart {...chartProps} />
        //     </React.Suspense>
        //   );
        case 'treemap':
          return (
            <React.Suspense fallback={<Spin size="large" />}>
              <TreemapChart {...chartProps} />
            </React.Suspense>
          );
        default:
          return <div>Advanced chart type not supported</div>;
      }
    } catch (error) {
      console.error('Error rendering advanced chart:', error);
      return <Alert message="Chart rendering error" description="This advanced chart type is not available." type="warning" />;
    }
  };

  const renderChart = () => {
    const selectedChart = CHART_TYPES.find(chart => chart.value === config.type);
    const isAdvanced = selectedChart?.category === 'Advanced';

    return (
      <Card
        title={
          <Space>
            {selectedChart?.icon}
            {config.title}
            <Badge color={isAdvanced ? 'purple' : 'blue'} text={selectedChart?.category} />
          </Space>
        }
        extra={
          <Space>
            <Tooltip title="Refresh">
              <Button icon={<ReloadOutlined />} onClick={loadSampleData} />
            </Tooltip>
            <Tooltip title="Download">
              <Button icon={<DownloadOutlined />} />
            </Tooltip>
            <Tooltip title={fullscreen ? "Exit Fullscreen" : "Fullscreen"}>
              <Button 
                icon={<FullscreenOutlined />} 
                onClick={() => setFullscreen(!fullscreen)}
              />
            </Tooltip>
          </Space>
        }
        style={{ height: fullscreen ? '700px' : '500px' }}
      >
        {loading ? (
          <div style={{ textAlign: 'center', padding: '100px' }}>
            <Spin size="large" />
            <div style={{ marginTop: '16px' }}>
              <Text>Generating visualization...</Text>
            </div>
          </div>
        ) : (
          <div style={{ height: '100%' }}>
            {isAdvanced ? renderAdvancedChart() : renderStandardChart()}
          </div>
        )}
      </Card>
    );
  };

  const renderConfigurationPanel = () => (
    <Card title={<Space><SettingOutlined />Configuration</Space>} size="small">
      <Collapse defaultActiveKey={['basic']} ghost>
        <Panel header="Basic Settings" key="basic">
          <Space direction="vertical" style={{ width: '100%' }}>
            <div>
              <Text strong>Chart Type</Text>
              <Select
                value={config.type}
                onChange={(value) => setConfig(prev => ({ ...prev, type: value }))}
                style={{ width: '100%', marginTop: '4px' }}
              >
                {CHART_TYPES.map(chart => (
                  <Option key={chart.value} value={chart.value}>
                    <Space>
                      {chart.icon}
                      {chart.label}
                      <Badge color={chart.category === 'Advanced' ? 'purple' : 'blue'} text={chart.category} />
                    </Space>
                  </Option>
                ))}
              </Select>
            </div>

            <div>
              <Text strong>Title</Text>
              <Input
                value={config.title}
                onChange={(e) => setConfig(prev => ({ ...prev, title: e.target.value }))}
                style={{ marginTop: '4px' }}
              />
            </div>

            {chartData && (
              <>
                <div>
                  <Text strong>X-Axis</Text>
                  <Select
                    value={config.xAxis}
                    onChange={(value) => setConfig(prev => ({ ...prev, xAxis: value }))}
                    style={{ width: '100%', marginTop: '4px' }}
                    placeholder="Select X-axis column"
                  >
                    {chartData.columns.map(col => (
                      <Option key={col} value={col}>{col}</Option>
                    ))}
                  </Select>
                </div>

                <div>
                  <Text strong>Y-Axis</Text>
                  <Select
                    value={config.yAxis}
                    onChange={(value) => setConfig(prev => ({ ...prev, yAxis: value }))}
                    style={{ width: '100%', marginTop: '4px' }}
                    placeholder="Select Y-axis column"
                  >
                    {chartData.columns.map(col => (
                      <Option key={col} value={col}>{col}</Option>
                    ))}
                  </Select>
                </div>
              </>
            )}
          </Space>
        </Panel>

        <Panel header="Appearance" key="appearance">
          <Space direction="vertical" style={{ width: '100%' }}>
            <div>
              <Text strong>Color Scheme</Text>
              <Select
                value={config.colorScheme}
                onChange={(value) => setConfig(prev => ({ ...prev, colorScheme: value }))}
                style={{ width: '100%', marginTop: '4px' }}
              >
                {COLOR_SCHEMES.map(scheme => (
                  <Option key={scheme.value} value={scheme.value}>
                    <Space>
                      <div style={{ display: 'flex', gap: '2px' }}>
                        {scheme.colors.slice(0, 3).map((color, i) => (
                          <div
                            key={i}
                            style={{
                              width: '12px',
                              height: '12px',
                              backgroundColor: color,
                              borderRadius: '2px'
                            }}
                          />
                        ))}
                      </div>
                      {scheme.label}
                    </Space>
                  </Option>
                ))}
              </Select>
            </div>

            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
              <Text strong>Show Legend</Text>
              <Switch
                checked={config.showLegend}
                onChange={(checked) => setConfig(prev => ({ ...prev, showLegend: checked }))}
              />
            </div>

            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
              <Text strong>Show Grid</Text>
              <Switch
                checked={config.showGrid}
                onChange={(checked) => setConfig(prev => ({ ...prev, showGrid: checked }))}
              />
            </div>

            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
              <Text strong>Animation</Text>
              <Switch
                checked={config.showAnimation}
                onChange={(checked) => setConfig(prev => ({ ...prev, showAnimation: checked }))}
              />
            </div>

            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
              <Text strong>Interactive</Text>
              <Switch
                checked={config.interactive}
                onChange={(checked) => setConfig(prev => ({ ...prev, interactive: checked }))}
              />
            </div>
          </Space>
        </Panel>
      </Collapse>
    </Card>
  );

  return (
    <div style={{ padding: '24px', maxWidth: '1600px', margin: '0 auto' }}>
      <div style={{ marginBottom: '24px' }}>
        <Title level={2}>
          <BulbOutlined style={{ color: '#1890ff', marginRight: '8px' }} />
          Enhanced Visualization Engine
        </Title>
        <Text type="secondary">
          Advanced D3.js-powered visualizations with interactive controls and real-time updates
        </Text>
      </div>

      <Alert
        message="Advanced Visualization Features"
        description="This interface demonstrates both standard charts (Recharts) and advanced D3.js visualizations including heatmaps, network graphs, Sankey diagrams, and treemaps."
        type="info"
        style={{ marginBottom: '24px' }}
        showIcon
      />

      <Row gutter={[24, 24]}>
        <Col xs={24} lg={18}>
          {renderChart()}
        </Col>
        <Col xs={24} lg={6}>
          {renderConfigurationPanel()}
          
          {chartData && (
            <Card title="Data Summary" size="small" style={{ marginTop: '16px' }}>
              <Space direction="vertical" style={{ width: '100%' }}>
                <div style={{ display: 'flex', justifyContent: 'space-between' }}>
                  <Text>Total Rows:</Text>
                  <Text strong>{chartData.metadata.totalRows}</Text>
                </div>
                <div style={{ display: 'flex', justifyContent: 'space-between' }}>
                  <Text>Columns:</Text>
                  <Text strong>{chartData.columns.length}</Text>
                </div>
                <Divider style={{ margin: '8px 0' }} />
                <Text type="secondary" style={{ fontSize: '12px' }}>
                  Data includes: {chartData.columns.join(', ')}
                </Text>
              </Space>
            </Card>
          )}
        </Col>
      </Row>
    </div>
  );
};

export default EnhancedVisualizationInterface;
