import React, { useState, useEffect, useCallback, useMemo } from 'react';
import {
  Card,
  Row,
  Col,
  Typography,
  Button,
  Space,
  Select,
  Slider,
  Switch,
  Tooltip,
  Tag,
  Alert,
  Spin,
  Tabs,
  Collapse,
  Form,
  Input,
  DatePicker,
  Checkbox,
  Divider,
} from 'antd';
import {
  BarChartOutlined,
  LineChartOutlined,
  PieChartOutlined,
  DotChartOutlined,
  HeatMapOutlined,
  AreaChartOutlined,
  FilterOutlined,
  SettingOutlined,
  FullscreenOutlined,
  DownloadOutlined,
  PlayCircleOutlined,
  PauseCircleOutlined,
  ReloadOutlined,
  ZoomInOutlined,
  ZoomOutOutlined,
} from '@ant-design/icons';
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
  Brush,
  ReferenceLine,
} from 'recharts';
import dayjs from 'dayjs';

const { Title, Text } = Typography;
const { Option } = Select;
const { TabPane } = Tabs;
const { Panel } = Collapse;
const { RangePicker } = DatePicker;

interface FilterConfig {
  id: string;
  column: string;
  type: 'range' | 'select' | 'multiSelect' | 'dateRange' | 'text';
  label: string;
  value: any;
  options?: any[];
  min?: number;
  max?: number;
}

interface ChartConfig {
  type: 'bar' | 'line' | 'pie' | 'scatter' | 'area' | 'heatmap';
  title: string;
  xAxis: string;
  yAxis: string;
  groupBy?: string;
  aggregation: 'sum' | 'avg' | 'count' | 'max' | 'min';
  showTrend: boolean;
  showAnimation: boolean;
  showGrid: boolean;
  showLegend: boolean;
  colorScheme: string;
}

interface StoredData {
  data: any[];
  columns: string[];
  query: string;
  timestamp: number;
}

const InteractiveVisualization: React.FC = () => {
  const [availableData, setAvailableData] = useState<StoredData[]>([]);
  const [selectedDataSource, setSelectedDataSource] = useState<string>('');
  const [chartConfig, setChartConfig] = useState<ChartConfig>({
    type: 'bar',
    title: 'Interactive Visualization',
    xAxis: '',
    yAxis: '',
    aggregation: 'sum',
    showTrend: false,
    showAnimation: true,
    showGrid: true,
    showLegend: true,
    colorScheme: 'default',
  });
  const [filters, setFilters] = useState<FilterConfig[]>([]);
  const [activeFilters, setActiveFilters] = useState<Record<string, any>>({});
  const [isFullscreen, setIsFullscreen] = useState(false);
  const [isPlaying, setIsPlaying] = useState(false);
  const [loading, setLoading] = useState(false);
  const [zoomLevel, setZoomLevel] = useState(100);

  // Chart type options
  const chartTypes = [
    { value: 'bar', label: 'Bar Chart', icon: <BarChartOutlined /> },
    { value: 'line', label: 'Line Chart', icon: <LineChartOutlined /> },
    { value: 'area', label: 'Area Chart', icon: <AreaChartOutlined /> },
    { value: 'pie', label: 'Pie Chart', icon: <PieChartOutlined /> },
    { value: 'scatter', label: 'Scatter Plot', icon: <DotChartOutlined /> },
    { value: 'heatmap', label: 'Heatmap', icon: <HeatMapOutlined /> },
  ];

  // Color schemes
  const colorSchemes = [
    { value: 'default', label: 'Default Blue' },
    { value: 'rainbow', label: 'Rainbow' },
    { value: 'warm', label: 'Warm Colors' },
    { value: 'cool', label: 'Cool Colors' },
    { value: 'monochrome', label: 'Monochrome' },
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
            data: parsed.data || [],
            columns: parsed.columns || [],
            query: parsed.query || 'Current Query Result',
            timestamp: parsed.timestamp || Date.now()
          });
        } catch (error) {
          console.error('Error parsing stored data:', error);
        }
      }

      // Add comprehensive sample data
      sources.push({
        data: generateTimeSeriesData(),
        columns: ['date', 'revenue', 'users', 'conversion_rate', 'region', 'product'],
        query: 'Sample Time Series Data',
        timestamp: Date.now() - 86400000
      });

      sources.push({
        data: generateCategoricalData(),
        columns: ['category', 'value', 'count', 'percentage', 'status'],
        query: 'Sample Categorical Data',
        timestamp: Date.now() - 172800000
      });

      setAvailableData(sources);
      if (sources.length > 0) {
        setSelectedDataSource(sources[0].query);
        initializeFilters(sources[0]);
      }
    };

    loadDataSources();
  }, []);

  // Generate time series sample data
  const generateTimeSeriesData = () => {
    const data: any[] = [];
    const regions = ['North', 'South', 'East', 'West'];
    const products = ['Product A', 'Product B', 'Product C'];
    
    for (let i = 0; i < 90; i++) {
      const date = dayjs().subtract(90 - i, 'day').format('YYYY-MM-DD');
      for (const region of regions) {
        for (const product of products) {
          data.push({
            date,
            revenue: Math.floor(Math.random() * 50000) + 10000,
            users: Math.floor(Math.random() * 1000) + 200,
            conversion_rate: Math.random() * 10 + 2,
            region,
            product
          });
        }
      }
    }
    return data;
  };

  // Generate categorical sample data
  const generateCategoricalData = () => {
    const categories = ['Electronics', 'Clothing', 'Books', 'Home', 'Sports', 'Beauty'];
    const statuses = ['Active', 'Inactive', 'Pending'];
    
    return categories.map(category => ({
      category,
      value: Math.floor(Math.random() * 100000) + 10000,
      count: Math.floor(Math.random() * 1000) + 100,
      percentage: Math.random() * 100,
      status: statuses[Math.floor(Math.random() * statuses.length)]
    }));
  };

  // Initialize filters based on data
  const initializeFilters = (dataSource: StoredData) => {
    const newFilters: FilterConfig[] = [];

    // Safety checks
    if (!dataSource || !Array.isArray(dataSource.data) || !Array.isArray(dataSource.columns)) {
      console.warn('Invalid data source structure:', dataSource);
      setFilters([]);
      setActiveFilters({});
      return;
    }

    const sampleData = dataSource.data.slice(0, 100);

    dataSource.columns.forEach(column => {
      const values = sampleData.map(row => row[column]).filter(v => v != null);
      if (values.length === 0) return;

      const uniqueValues = [...new Set(values)];
      const isNumeric = values.every(v => typeof v === 'number' || !isNaN(Number(v)));
      const isDate = values.some(v => dayjs(v).isValid() && typeof v === 'string');

      if (isDate) {
        newFilters.push({
          id: `filter-${column}`,
          column,
          type: 'dateRange',
          label: column.replace(/_/g, ' ').replace(/\b\w/g, l => l.toUpperCase()),
          value: null
        });
      } else if (isNumeric) {
        const numValues = values.map(v => Number(v));
        newFilters.push({
          id: `filter-${column}`,
          column,
          type: 'range',
          label: column.replace(/_/g, ' ').replace(/\b\w/g, l => l.toUpperCase()),
          value: [Math.min(...numValues), Math.max(...numValues)],
          min: Math.min(...numValues),
          max: Math.max(...numValues)
        });
      } else if (uniqueValues.length <= 20) {
        newFilters.push({
          id: `filter-${column}`,
          column,
          type: 'multiSelect',
          label: column.replace(/_/g, ' ').replace(/\b\w/g, l => l.toUpperCase()),
          value: [],
          options: uniqueValues.map(v => ({ label: v, value: v }))
        });
      } else {
        newFilters.push({
          id: `filter-${column}`,
          column,
          type: 'text',
          label: column.replace(/_/g, ' ').replace(/\b\w/g, l => l.toUpperCase()),
          value: ''
        });
      }
    });

    setFilters(newFilters);
    setActiveFilters({});
  };

  // Get current data source
  const currentDataSource = useMemo(() => {
    return availableData.find(d => d.query === selectedDataSource);
  }, [availableData, selectedDataSource]);

  // Apply filters to data
  const filteredData = useMemo(() => {
    if (!currentDataSource || !Array.isArray(currentDataSource.data)) return [];

    let data = [...currentDataSource.data];
    
    Object.entries(activeFilters).forEach(([column, filterValue]) => {
      const filter = filters.find(f => f.column === column);
      if (!filter || !filterValue) return;

      switch (filter.type) {
        case 'range':
          if (Array.isArray(filterValue) && filterValue.length === 2) {
            data = data.filter(row => {
              const value = Number(row[column]);
              return value >= filterValue[0] && value <= filterValue[1];
            });
          }
          break;
        case 'multiSelect':
          if (Array.isArray(filterValue) && filterValue.length > 0) {
            data = data.filter(row => filterValue.includes(row[column]));
          }
          break;
        case 'dateRange':
          if (Array.isArray(filterValue) && filterValue.length === 2) {
            data = data.filter(row => {
              const date = dayjs(row[column]);
              return date.isAfter(filterValue[0]) && date.isBefore(filterValue[1]);
            });
          }
          break;
        case 'text':
          if (typeof filterValue === 'string' && filterValue.trim()) {
            data = data.filter(row => 
              String(row[column]).toLowerCase().includes(filterValue.toLowerCase())
            );
          }
          break;
      }
    });

    return data;
  }, [currentDataSource, activeFilters, filters]);

  // Process data for visualization
  const processedData = useMemo(() => {
    if (!filteredData.length || !chartConfig.xAxis || !chartConfig.yAxis) return [];

    const grouped = filteredData.reduce((acc, row) => {
      const key = chartConfig.groupBy ? `${row[chartConfig.xAxis]}-${row[chartConfig.groupBy]}` : row[chartConfig.xAxis];
      if (!acc[key]) {
        acc[key] = {
          [chartConfig.xAxis]: row[chartConfig.xAxis],
          ...(chartConfig.groupBy && { [chartConfig.groupBy]: row[chartConfig.groupBy] }),
          values: []
        };
      }
      acc[key].values.push(Number(row[chartConfig.yAxis]) || 0);
      return acc;
    }, {} as Record<string, any>);

    return Object.values(grouped).map((group: any) => {
      let aggregatedValue;
      switch (chartConfig.aggregation) {
        case 'sum':
          aggregatedValue = group.values.reduce((a: number, b: number) => a + b, 0);
          break;
        case 'avg':
          aggregatedValue = group.values.reduce((a: number, b: number) => a + b, 0) / group.values.length;
          break;
        case 'count':
          aggregatedValue = group.values.length;
          break;
        case 'max':
          aggregatedValue = Math.max(...group.values);
          break;
        case 'min':
          aggregatedValue = Math.min(...group.values);
          break;
        default:
          aggregatedValue = group.values.reduce((a: number, b: number) => a + b, 0);
      }

      return {
        ...group,
        [chartConfig.yAxis]: aggregatedValue
      };
    });
  }, [filteredData, chartConfig]);

  // Get color for chart
  const getColor = (index: number) => {
    const schemes = {
      default: ['#1890ff', '#52c41a', '#faad14', '#f5222d', '#722ed1', '#13c2c2'],
      rainbow: ['#ff0000', '#ff8000', '#ffff00', '#80ff00', '#00ff00', '#00ff80'],
      warm: ['#ff6b6b', '#ffa726', '#ffcc02', '#ff8a65', '#f06292', '#ba68c8'],
      cool: ['#42a5f5', '#26c6da', '#66bb6a', '#9ccc65', '#d4e157', '#ffee58'],
      monochrome: ['#212121', '#424242', '#616161', '#757575', '#9e9e9e', '#bdbdbd']
    };
    
    const colors = schemes[chartConfig.colorScheme as keyof typeof schemes] || schemes.default;
    return colors[index % colors.length];
  };

  // Render chart based on type
  const renderChart = () => {
    if (!processedData.length) return null;

    const commonProps = {
      width: '100%',
      height: 400,
      data: processedData
    };

    switch (chartConfig.type) {
      case 'bar':
        return (
          <ResponsiveContainer {...commonProps}>
            <BarChart data={processedData}>
              {chartConfig.showGrid && <CartesianGrid strokeDasharray="3 3" />}
              <XAxis dataKey={chartConfig.xAxis} />
              <YAxis />
              <RechartsTooltip />
              {chartConfig.showLegend && <Legend />}
              <Bar
                dataKey={chartConfig.yAxis}
                fill={getColor(0)}
                animationDuration={chartConfig.showAnimation ? 1000 : 0}
              />
              {chartConfig.showTrend && (
                <ReferenceLine y={processedData.reduce((sum, item) => sum + item[chartConfig.yAxis], 0) / processedData.length} stroke="red" strokeDasharray="5 5" />
              )}
              <Brush dataKey={chartConfig.xAxis} height={30} />
            </BarChart>
          </ResponsiveContainer>
        );

      case 'line':
        return (
          <ResponsiveContainer {...commonProps}>
            <LineChart data={processedData}>
              {chartConfig.showGrid && <CartesianGrid strokeDasharray="3 3" />}
              <XAxis dataKey={chartConfig.xAxis} />
              <YAxis />
              <RechartsTooltip />
              {chartConfig.showLegend && <Legend />}
              <Line
                type="monotone"
                dataKey={chartConfig.yAxis}
                stroke={getColor(0)}
                strokeWidth={2}
                dot={{ fill: getColor(0), strokeWidth: 2, r: 4 }}
                animationDuration={chartConfig.showAnimation ? 1000 : 0}
              />
              {chartConfig.showTrend && (
                <ReferenceLine y={processedData.reduce((sum, item) => sum + item[chartConfig.yAxis], 0) / processedData.length} stroke="red" strokeDasharray="5 5" />
              )}
              <Brush dataKey={chartConfig.xAxis} height={30} />
            </LineChart>
          </ResponsiveContainer>
        );

      case 'area':
        return (
          <ResponsiveContainer {...commonProps}>
            <AreaChart data={processedData}>
              {chartConfig.showGrid && <CartesianGrid strokeDasharray="3 3" />}
              <XAxis dataKey={chartConfig.xAxis} />
              <YAxis />
              <RechartsTooltip />
              {chartConfig.showLegend && <Legend />}
              <Area
                type="monotone"
                dataKey={chartConfig.yAxis}
                stroke={getColor(0)}
                fill={getColor(0)}
                fillOpacity={0.6}
                animationDuration={chartConfig.showAnimation ? 1000 : 0}
              />
              <Brush dataKey={chartConfig.xAxis} height={30} />
            </AreaChart>
          </ResponsiveContainer>
        );

      case 'pie':
        return (
          <ResponsiveContainer {...commonProps}>
            <PieChart>
              <Pie
                data={processedData}
                dataKey={chartConfig.yAxis}
                nameKey={chartConfig.xAxis}
                cx="50%"
                cy="50%"
                outerRadius={150}
                fill={getColor(0)}
                animationDuration={chartConfig.showAnimation ? 1000 : 0}
                label={({ name, percent }) => `${name}: ${(percent * 100).toFixed(0)}%`}
              >
                {processedData.map((entry, index) => (
                  <Cell key={`cell-${index}`} fill={getColor(index)} />
                ))}
              </Pie>
              <RechartsTooltip />
              {chartConfig.showLegend && <Legend />}
            </PieChart>
          </ResponsiveContainer>
        );

      case 'scatter':
        return (
          <ResponsiveContainer {...commonProps}>
            <ScatterChart data={processedData}>
              {chartConfig.showGrid && <CartesianGrid strokeDasharray="3 3" />}
              <XAxis dataKey={chartConfig.xAxis} />
              <YAxis dataKey={chartConfig.yAxis} />
              <RechartsTooltip cursor={{ strokeDasharray: '3 3' }} />
              <Scatter
                fill={getColor(0)}
                animationDuration={chartConfig.showAnimation ? 1000 : 0}
              />
            </ScatterChart>
          </ResponsiveContainer>
        );

      case 'heatmap':
        // For heatmap, we'll create a simple grid representation
        const heatmapData = processedData.slice(0, 100); // Limit for performance
        return (
          <div style={{ padding: 20 }}>
            <div style={{ display: 'grid', gridTemplateColumns: 'repeat(10, 1fr)', gap: 2 }}>
              {heatmapData.map((item, index) => {
                const intensity = item[chartConfig.yAxis] / Math.max(...heatmapData.map(d => d[chartConfig.yAxis]));
                return (
                  <Tooltip key={index} title={`${item[chartConfig.xAxis]}: ${item[chartConfig.yAxis]}`}>
                    <div
                      style={{
                        width: 40,
                        height: 40,
                        backgroundColor: `rgba(24, 144, 255, ${intensity})`,
                        border: '1px solid #f0f0f0',
                        cursor: 'pointer'
                      }}
                    />
                  </Tooltip>
                );
              })}
            </div>
          </div>
        );

      default:
        return (
          <div style={{ padding: 40, textAlign: 'center' }}>
            <Text>Chart type "{chartConfig.type}" not implemented</Text>
          </div>
        );
    }
  };

  return (
    <div style={{ padding: 24 }}>
      {/* Header */}
      <Card style={{ marginBottom: 16 }}>
        <Row justify="space-between" align="middle">
          <Col>
            <Title level={3} style={{ margin: 0 }}>
              Interactive Visualization
            </Title>
            <Text type="secondary">
              Create dynamic, interactive charts with real-time filtering and customization
            </Text>
          </Col>
          <Col>
            <Space>
              <Button
                icon={isPlaying ? <PauseCircleOutlined /> : <PlayCircleOutlined />}
                onClick={() => setIsPlaying(!isPlaying)}
              >
                {isPlaying ? 'Pause' : 'Play'}
              </Button>
              <Button icon={<ReloadOutlined />} onClick={() => window.location.reload()}>
                Refresh
              </Button>
              <Button icon={<DownloadOutlined />}>
                Export
              </Button>
              <Button
                icon={<FullscreenOutlined />}
                onClick={() => setIsFullscreen(!isFullscreen)}
              >
                Fullscreen
              </Button>
            </Space>
          </Col>
        </Row>
      </Card>

      <Row gutter={16}>
        {/* Configuration Panel */}
        <Col span={6}>
          <Card title="Configuration" size="small">
            <Space direction="vertical" style={{ width: '100%' }}>
              {/* Data Source Selection */}
              <div>
                <Text strong>Data Source</Text>
                <Select
                  style={{ width: '100%', marginTop: 8 }}
                  value={selectedDataSource}
                  onChange={(value) => {
                    setSelectedDataSource(value);
                    const dataSource = availableData.find(d => d.query === value);
                    if (dataSource) {
                      initializeFilters(dataSource);
                    }
                  }}
                >
                  {availableData.map((source, index) => (
                    <Option key={index} value={source.query}>
                      {source.query} ({Array.isArray(source.data) ? source.data.length : 0} rows)
                    </Option>
                  ))}
                </Select>
              </div>

              {/* Chart Type */}
              <div>
                <Text strong>Chart Type</Text>
                <Select
                  style={{ width: '100%', marginTop: 8 }}
                  value={chartConfig.type}
                  onChange={(value) => setChartConfig(prev => ({ ...prev, type: value }))}
                >
                  {chartTypes.map(type => (
                    <Option key={type.value} value={type.value}>
                      <Space>
                        {type.icon}
                        {type.label}
                      </Space>
                    </Option>
                  ))}
                </Select>
              </div>

              {/* Axis Configuration */}
              {currentDataSource && Array.isArray(currentDataSource.columns) && (
                <>
                  <div>
                    <Text strong>X-Axis</Text>
                    <Select
                      style={{ width: '100%', marginTop: 8 }}
                      value={chartConfig.xAxis}
                      onChange={(value) => setChartConfig(prev => ({ ...prev, xAxis: value }))}
                      placeholder="Select X-axis column"
                    >
                      {currentDataSource.columns.map(col => (
                        <Option key={col} value={col}>{col}</Option>
                      ))}
                    </Select>
                  </div>

                  <div>
                    <Text strong>Y-Axis</Text>
                    <Select
                      style={{ width: '100%', marginTop: 8 }}
                      value={chartConfig.yAxis}
                      onChange={(value) => setChartConfig(prev => ({ ...prev, yAxis: value }))}
                      placeholder="Select Y-axis column"
                    >
                      {currentDataSource.columns.map(col => (
                        <Option key={col} value={col}>{col}</Option>
                      ))}
                    </Select>
                  </div>

                  <div>
                    <Text strong>Group By (Optional)</Text>
                    <Select
                      style={{ width: '100%', marginTop: 8 }}
                      value={chartConfig.groupBy}
                      onChange={(value) => setChartConfig(prev => ({ ...prev, groupBy: value }))}
                      placeholder="Select grouping column"
                      allowClear
                    >
                      {currentDataSource.columns.map(col => (
                        <Option key={col} value={col}>{col}</Option>
                      ))}
                    </Select>
                  </div>
                </>
              )}

              {/* Aggregation */}
              <div>
                <Text strong>Aggregation</Text>
                <Select
                  style={{ width: '100%', marginTop: 8 }}
                  value={chartConfig.aggregation}
                  onChange={(value) => setChartConfig(prev => ({ ...prev, aggregation: value }))}
                >
                  <Option value="sum">Sum</Option>
                  <Option value="avg">Average</Option>
                  <Option value="count">Count</Option>
                  <Option value="max">Maximum</Option>
                  <Option value="min">Minimum</Option>
                </Select>
              </div>

              {/* Color Scheme */}
              <div>
                <Text strong>Color Scheme</Text>
                <Select
                  style={{ width: '100%', marginTop: 8 }}
                  value={chartConfig.colorScheme}
                  onChange={(value) => setChartConfig(prev => ({ ...prev, colorScheme: value }))}
                >
                  {colorSchemes.map(scheme => (
                    <Option key={scheme.value} value={scheme.value}>
                      {scheme.label}
                    </Option>
                  ))}
                </Select>
              </div>

              <Divider />

              {/* Chart Options */}
              <Space direction="vertical" style={{ width: '100%' }}>
                <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                  <Text>Show Animation</Text>
                  <Switch
                    checked={chartConfig.showAnimation}
                    onChange={(checked) => setChartConfig(prev => ({ ...prev, showAnimation: checked }))}
                  />
                </div>
                <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                  <Text>Show Grid</Text>
                  <Switch
                    checked={chartConfig.showGrid}
                    onChange={(checked) => setChartConfig(prev => ({ ...prev, showGrid: checked }))}
                  />
                </div>
                <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                  <Text>Show Legend</Text>
                  <Switch
                    checked={chartConfig.showLegend}
                    onChange={(checked) => setChartConfig(prev => ({ ...prev, showLegend: checked }))}
                  />
                </div>
                <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                  <Text>Show Trend</Text>
                  <Switch
                    checked={chartConfig.showTrend}
                    onChange={(checked) => setChartConfig(prev => ({ ...prev, showTrend: checked }))}
                  />
                </div>
              </Space>

              <Divider />

              {/* Zoom Control */}
              <div>
                <Text strong>Zoom Level: {zoomLevel}%</Text>
                <Slider
                  style={{ marginTop: 8 }}
                  min={50}
                  max={200}
                  value={zoomLevel}
                  onChange={setZoomLevel}
                  marks={{ 50: '50%', 100: '100%', 200: '200%' }}
                />
              </div>
            </Space>
          </Card>
        </Col>

        {/* Main Content */}
        <Col span={18}>
          <Tabs defaultActiveKey="chart">
            <TabPane tab="Chart" key="chart">
              <Card
                title={
                  <Space>
                    <Text strong>{chartConfig.title}</Text>
                    <Tag color="blue">{chartConfig.type}</Tag>
                    <Tag color="green">{processedData.length} data points</Tag>
                  </Space>
                }
                extra={
                  <Space>
                    <Button size="small" icon={<ZoomOutOutlined />} onClick={() => setZoomLevel(Math.max(50, zoomLevel - 25))} />
                    <Button size="small" icon={<ZoomInOutlined />} onClick={() => setZoomLevel(Math.min(200, zoomLevel + 25))} />
                    <Button size="small" icon={<SettingOutlined />} />
                  </Space>
                }
                style={{ height: 600 }}
              >
                {processedData.length === 0 ? (
                  <div style={{ textAlign: 'center', padding: 60 }}>
                    <BarChartOutlined style={{ fontSize: 64, color: '#d9d9d9', marginBottom: 16 }} />
                    <Title level={4} type="secondary">No Data to Display</Title>
                    <Text type="secondary">
                      {!chartConfig.xAxis || !chartConfig.yAxis 
                        ? 'Please select X and Y axis columns to create a chart'
                        : 'No data matches the current filters'
                      }
                    </Text>
                  </div>
                ) : (
                  <div style={{ transform: `scale(${zoomLevel / 100})`, transformOrigin: 'top left' }}>
                    {renderChart()}
                  </div>
                )}
              </Card>
            </TabPane>

            <TabPane tab={<span><FilterOutlined />Filters</span>} key="filters">
              <Card title="Data Filters" style={{ height: 600, overflow: 'auto' }}>
                {filters.length === 0 ? (
                  <div style={{ textAlign: 'center', padding: 40 }}>
                    <FilterOutlined style={{ fontSize: 48, color: '#d9d9d9', marginBottom: 16 }} />
                    <Text type="secondary">No filters available</Text>
                  </div>
                ) : (
                  <Collapse>
                    {filters.map(filter => (
                      <Panel
                        key={filter.id}
                        header={
                          <Space>
                            {filter.label}
                            {activeFilters[filter.column] && (
                              <Tag color="blue" size="small">Active</Tag>
                            )}
                          </Space>
                        }
                      >
                        {filter.type === 'range' && (
                          <div>
                            <Text>Range: {filter.min} - {filter.max}</Text>
                            <Slider
                              range
                              style={{ marginTop: 8 }}
                              min={filter.min}
                              max={filter.max}
                              value={activeFilters[filter.column] || [filter.min, filter.max]}
                              onChange={(value) => setActiveFilters(prev => ({ ...prev, [filter.column]: value }))}
                              tooltip={{ formatter: (value) => value?.toFixed(2) }}
                            />
                          </div>
                        )}

                        {filter.type === 'multiSelect' && (
                          <Checkbox.Group
                            style={{ width: '100%' }}
                            options={filter.options}
                            value={activeFilters[filter.column] || []}
                            onChange={(value) => setActiveFilters(prev => ({ ...prev, [filter.column]: value }))}
                          />
                        )}

                        {filter.type === 'dateRange' && (
                          <RangePicker
                            style={{ width: '100%' }}
                            value={activeFilters[filter.column]}
                            onChange={(dates) => setActiveFilters(prev => ({ ...prev, [filter.column]: dates }))}
                          />
                        )}

                        {filter.type === 'text' && (
                          <Input
                            placeholder={`Filter by ${filter.label}`}
                            value={activeFilters[filter.column] || ''}
                            onChange={(e) => setActiveFilters(prev => ({ ...prev, [filter.column]: e.target.value }))}
                          />
                        )}

                        <div style={{ marginTop: 8, textAlign: 'right' }}>
                          <Button
                            size="small"
                            onClick={() => setActiveFilters(prev => {
                              const newFilters = { ...prev };
                              delete newFilters[filter.column];
                              return newFilters;
                            })}
                          >
                            Clear
                          </Button>
                        </div>
                      </Panel>
                    ))}
                  </Collapse>
                )}

                {Object.keys(activeFilters).length > 0 && (
                  <div style={{ marginTop: 16, textAlign: 'center' }}>
                    <Button
                      type="primary"
                      danger
                      onClick={() => setActiveFilters({})}
                    >
                      Clear All Filters
                    </Button>
                  </div>
                )}
              </Card>
            </TabPane>
          </Tabs>
        </Col>
      </Row>
    </div>
  );
};

export default InteractiveVisualization;
