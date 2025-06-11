/**
 * Unified Interactive Visualization Component
 * Consolidates standalone and query-specific interactive visualizations
 */

import React, { useState, useEffect, useMemo, useCallback } from 'react';
import {
  Select,
  Button,
  Space,
  Row,
  Col,
  Switch,
  Slider,
  Typography,
  Tooltip,
  Spin,
  Tabs,
  Input,
  DatePicker,
  Divider,
  Tag
} from 'antd';
import {
  SettingOutlined,
  DownloadOutlined,
  FullscreenOutlined,
  ReloadOutlined,
  FilterOutlined,
  PlayCircleOutlined,
  PauseCircleOutlined,
  CheckCircleOutlined,
  ExclamationCircleOutlined,
  HomeOutlined,
  DotChartOutlined
} from '@ant-design/icons';
import { PageLayout, PageSection, PageGrid } from '../core/Layouts';
import { Card } from '../core/Card';
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
  ReferenceLine
} from 'recharts';
import dayjs from 'dayjs';
import { useVisualizationResult } from '../../hooks/useCurrentResult';

const { Title, Text } = Typography;
const { Option } = Select;
const { TabPane } = Tabs;
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

interface InteractiveVisualizationProps {
  // For query-specific mode
  data?: any[];
  columns?: any[];
  query?: string;
  onConfigChange?: (config: any) => void;
  
  // For standalone mode
  standalone?: boolean;
  
  // Common props
  height?: number;
  enableFilters?: boolean;
  enableExport?: boolean;
  autoRefresh?: boolean;
}

export const InteractiveVisualization: React.FC<InteractiveVisualizationProps> = ({
  data: propData,
  columns: propColumns,
  query,
  onConfigChange,
  standalone = false,
  height = 400,
  enableFilters = true,
  enableExport = true,
  autoRefresh = false
}) => {
  // Use global result system for visualization
  const { result, hasResult, hasVisualizableData, dataLength, columnCount } = useVisualizationResult();

  // Debug logging in development
  useEffect(() => {
    if (process.env.NODE_ENV === 'development') {
      console.log('🎨 InteractiveVisualization - Result status:', {
        hasResult,
        hasVisualizableData,
        dataLength,
        columnCount,
        resultSource: result?.source,
        resultSuccess: result?.result?.success,
        standalone
      });
    }
  }, [hasResult, hasVisualizableData, result, standalone]);

  const [loading, setLoading] = useState(false);
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
  const [zoomLevel, setZoomLevel] = useState(100);
  
  // Data management for standalone mode
  const [availableData, setAvailableData] = useState<any[]>([]);
  const [selectedDataSource, setSelectedDataSource] = useState<string>('');

  // Chart settings
  const [chartSettings, setChartSettings] = useState({
    showGrid: true,
    showLegend: true,
    enableAnimation: true,
    height: height
  });

  // Get current data - either from props, global result system, or standalone sources
  const currentData = useMemo(() => {
    // Priority 1: Props data (when used as a component)
    if (propData && propData.length > 0) {
      return propData;
    }

    // Priority 2: Global result system (when used as a standalone page)
    if (hasResult && result?.result?.data && result.result.data.length > 0) {
      return result.result.data;
    }

    // Priority 3: Local standalone data (fallback)
    if (standalone && availableData.length > 0) {
      return availableData;
    }

    return [];
  }, [propData, hasResult, result, standalone, availableData]);

  // Get current columns
  const currentColumns = useMemo(() => {
    // Priority 1: Props columns (when used as a component)
    if (propColumns && propColumns.length > 0) {
      return propColumns.map(col => typeof col === 'string' ? col : col.name || col.key);
    }

    // Priority 2: Global result system columns
    if (hasResult && result?.result?.columns && result.result.columns.length > 0) {
      return result.result.columns.map(col => typeof col === 'string' ? col : col.name || col.key || col);
    }

    // Priority 3: Infer from data
    if (currentData.length > 0) {
      return Object.keys(currentData[0]);
    }

    return [];
  }, [propColumns, hasResult, result, currentData]);

  // Load data sources for standalone mode
  useEffect(() => {
    if (standalone) {
      loadStandaloneData();
    }
  }, [standalone]);

  const loadStandaloneData = () => {
    const sources: any[] = [];

    // Priority 1: Global result system (already handled in currentData)
    // This function is now mainly for fallback sample data

    // Priority 2: Load from localStorage (legacy support)
    const storedResult = localStorage.getItem('current-query-result');
    if (storedResult) {
      try {
        const parsed = JSON.parse(storedResult);
        sources.push(...(parsed.data || []));
      } catch (error) {
        console.error('Error parsing stored data:', error);
      }
    }

    // Priority 3: Add sample data if no stored data
    if (sources.length === 0) {
      sources.push(...generateSampleData());
    }

    setAvailableData(sources);
    initializeFilters(sources);
  };

  const generateSampleData = () => {
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

  // Initialize filters based on data
  const initializeFilters = (data: any[]) => {
    if (!enableFilters || !data.length) return;

    const newFilters: FilterConfig[] = [];
    const columns = Object.keys(data[0]);
    const sampleData = data.slice(0, 100);

    columns.forEach(column => {
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
      }
    });

    setFilters(newFilters);
    setActiveFilters({});
  };

  // Initialize chart config when data changes
  useEffect(() => {
    if (currentData.length > 0 && currentColumns.length > 0) {
      setChartConfig(prev => ({
        ...prev,
        xAxis: prev.xAxis || currentColumns[0],
        yAxis: prev.yAxis || currentColumns.find(col => 
          typeof currentData[0][col] === 'number'
        ) || currentColumns[1]
      }));
      
      if (enableFilters) {
        initializeFilters(currentData);
      }
    }
  }, [currentData, currentColumns, enableFilters]);

  // Apply filters to data
  const filteredData = useMemo(() => {
    if (!currentData.length || Object.keys(activeFilters).length === 0) {
      return currentData;
    }

    return currentData.filter(row => {
      return Object.entries(activeFilters).every(([column, filterValue]) => {
        const filter = filters.find(f => f.column === column);
        if (!filter || !filterValue) return true;

        const cellValue = row[column];

        switch (filter.type) {
          case 'range':
            if (Array.isArray(filterValue) && filterValue.length === 2) {
              const value = Number(cellValue);
              return value >= filterValue[0] && value <= filterValue[1];
            }
            return true;
          case 'multiSelect':
            if (Array.isArray(filterValue) && filterValue.length > 0) {
              return filterValue.includes(cellValue);
            }
            return true;
          case 'dateRange':
            if (Array.isArray(filterValue) && filterValue.length === 2) {
              const date = dayjs(cellValue);
              return date.isAfter(filterValue[0]) && date.isBefore(filterValue[1]);
            }
            return true;
          case 'text':
            if (typeof filterValue === 'string' && filterValue.trim()) {
              return String(cellValue).toLowerCase().includes(filterValue.toLowerCase());
            }
            return true;
          default:
            return true;
        }
      });
    });
  }, [currentData, activeFilters, filters]);

  // Process data for visualization
  const processedData = useMemo(() => {
    if (!filteredData.length || !chartConfig.xAxis || !chartConfig.yAxis) return [];

    const grouped = filteredData.reduce((acc, row) => {
      const key = chartConfig.groupBy ? 
        `${row[chartConfig.xAxis]}-${row[chartConfig.groupBy]}` : 
        row[chartConfig.xAxis];
      
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
    if (!processedData.length) {
      return (
        <div style={{ display: 'flex', justifyContent: 'center', alignItems: 'center', height: chartSettings.height }}>
          <Text type="secondary">No data available for visualization</Text>
        </div>
      );
    }

    const commonProps = {
      width: '100%',
      height: chartSettings.height,
      data: processedData
    };

    switch (chartConfig.type) {
      case 'bar':
        return (
          <ResponsiveContainer {...commonProps}>
            <BarChart data={processedData}>
              {chartSettings.showGrid && <CartesianGrid strokeDasharray="3 3" />}
              <XAxis dataKey={chartConfig.xAxis} />
              <YAxis />
              <RechartsTooltip />
              {chartSettings.showLegend && <Legend />}
              <Bar
                dataKey={chartConfig.yAxis}
                fill={getColor(0)}
                animationDuration={chartSettings.enableAnimation ? 1000 : 0}
              />
              {chartConfig.showTrend && (
                <ReferenceLine
                  y={processedData.reduce((sum, item) => sum + item[chartConfig.yAxis], 0) / processedData.length}
                  stroke="red"
                  strokeDasharray="5 5"
                />
              )}
              <Brush dataKey={chartConfig.xAxis} height={30} />
            </BarChart>
          </ResponsiveContainer>
        );

      case 'line':
        return (
          <ResponsiveContainer {...commonProps}>
            <LineChart data={processedData}>
              {chartSettings.showGrid && <CartesianGrid strokeDasharray="3 3" />}
              <XAxis dataKey={chartConfig.xAxis} />
              <YAxis />
              <RechartsTooltip />
              {chartSettings.showLegend && <Legend />}
              <Line
                type="monotone"
                dataKey={chartConfig.yAxis}
                stroke={getColor(0)}
                strokeWidth={2}
                dot={{ fill: getColor(0), strokeWidth: 2, r: 4 }}
                animationDuration={chartSettings.enableAnimation ? 1000 : 0}
              />
              {chartConfig.showTrend && (
                <ReferenceLine
                  y={processedData.reduce((sum, item) => sum + item[chartConfig.yAxis], 0) / processedData.length}
                  stroke="red"
                  strokeDasharray="5 5"
                />
              )}
              <Brush dataKey={chartConfig.xAxis} height={30} />
            </LineChart>
          </ResponsiveContainer>
        );

      case 'area':
        return (
          <ResponsiveContainer {...commonProps}>
            <AreaChart data={processedData}>
              {chartSettings.showGrid && <CartesianGrid strokeDasharray="3 3" />}
              <XAxis dataKey={chartConfig.xAxis} />
              <YAxis />
              <RechartsTooltip />
              {chartSettings.showLegend && <Legend />}
              <Area
                type="monotone"
                dataKey={chartConfig.yAxis}
                stroke={getColor(0)}
                fill={getColor(0)}
                fillOpacity={0.6}
                animationDuration={chartSettings.enableAnimation ? 1000 : 0}
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
                outerRadius={Math.min(chartSettings.height * 0.3, 150)}
                fill={getColor(0)}
                animationDuration={chartSettings.enableAnimation ? 1000 : 0}
                label={({ name, percent }) => `${name}: ${(percent * 100).toFixed(0)}%`}
              >
                {processedData.map((entry, index) => (
                  <Cell key={`cell-${index}`} fill={getColor(index)} />
                ))}
              </Pie>
              <RechartsTooltip />
              {chartSettings.showLegend && <Legend />}
            </PieChart>
          </ResponsiveContainer>
        );

      case 'scatter':
        return (
          <ResponsiveContainer {...commonProps}>
            <ScatterChart data={processedData}>
              {chartSettings.showGrid && <CartesianGrid strokeDasharray="3 3" />}
              <XAxis dataKey={chartConfig.xAxis} />
              <YAxis dataKey={chartConfig.yAxis} />
              <RechartsTooltip />
              <Scatter fill={getColor(0)} />
            </ScatterChart>
          </ResponsiveContainer>
        );

      default:
        return (
          <div style={{ display: 'flex', justifyContent: 'center', alignItems: 'center', height: chartSettings.height }}>
            <Text>Chart type "{chartConfig.type}" not implemented</Text>
          </div>
        );
    }
  };

  // Render filter controls
  const renderFilters = () => {
    if (!enableFilters || filters.length === 0) return null;

    return (
      <Card size="small" title={<Space><FilterOutlined />Filters</Space>} style={{ marginBottom: '16px' }}>
        <Row gutter={[16, 16]}>
          {filters.map(filter => (
            <Col span={8} key={filter.column}>
              <div>
                <Text strong>{filter.label}</Text>
                {filter.type === 'multiSelect' && (
                  <Select
                    mode="multiple"
                    placeholder={`Select ${filter.label}`}
                    style={{ width: '100%', marginTop: '4px' }}
                    value={activeFilters[filter.column] || []}
                    onChange={(value) => setActiveFilters(prev => ({ ...prev, [filter.column]: value }))}
                  >
                    {filter.options?.map(option => (
                      <Option key={option.value} value={option.value}>{option.label}</Option>
                    ))}
                  </Select>
                )}
                {filter.type === 'range' && (
                  <Slider
                    range
                    style={{ marginTop: '8px' }}
                    min={filter.min}
                    max={filter.max}
                    value={activeFilters[filter.column] || [filter.min, filter.max]}
                    onChange={(value) => setActiveFilters(prev => ({ ...prev, [filter.column]: value }))}
                    tooltip={{ formatter: (value) => value?.toFixed(2) }}
                  />
                )}
                {filter.type === 'dateRange' && (
                  <RangePicker
                    style={{ width: '100%', marginTop: '4px' }}
                    value={activeFilters[filter.column] || null}
                    onChange={(dates) => {
                      if (dates) {
                        setActiveFilters(prev => ({
                          ...prev,
                          [filter.column]: [dates[0]?.toDate(), dates[1]?.toDate()]
                        }));
                      } else {
                        setActiveFilters(prev => {
                          const newFilters = { ...prev };
                          delete newFilters[filter.column];
                          return newFilters;
                        });
                      }
                    }}
                  />
                )}
                {filter.type === 'text' && (
                  <Input
                    placeholder={`Filter by ${filter.label}`}
                    style={{ marginTop: '4px' }}
                    value={activeFilters[filter.column] || ''}
                    onChange={(e) => setActiveFilters(prev => ({ ...prev, [filter.column]: e.target.value }))}
                  />
                )}
              </div>
            </Col>
          ))}
        </Row>
      </Card>
    );
  };

  // Export data
  const exportData = () => {
    if (!enableExport) return;

    const csv = convertToCSV(filteredData);
    const blob = new Blob([csv], { type: 'text/csv' });
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = `interactive_chart_data_${Date.now()}.csv`;
    a.click();
    URL.revokeObjectURL(url);
  };

  const convertToCSV = (data: any[]): string => {
    if (data.length === 0) return '';

    const headers = Object.keys(data[0]);
    const csvContent = [
      headers.join(','),
      ...data.map(row =>
        headers.map(header =>
          JSON.stringify(row[header] ?? '')
        ).join(',')
      )
    ].join('\n');

    return csvContent;
  };

  if (loading) {
    return (
      <Card>
        <div style={{ textAlign: 'center', padding: '40px' }}>
          <Spin size="large" />
          <div style={{ marginTop: '16px' }}>
            <Text>Loading interactive visualization...</Text>
          </div>
        </div>
      </Card>
    );
  }

  return (
    <div className="interactive-visualization">
      {/* Global Result Status */}
      {standalone && (
        <Card
          variant="outlined"
          padding="medium"
          style={{ marginBottom: 'var(--space-4)', background: 'var(--bg-tertiary)' }}
        >
          <div style={{
            display: 'flex',
            justifyContent: 'space-between',
            alignItems: 'center',
            flexWrap: 'wrap',
            gap: 'var(--space-4)'
          }}>
            <Typography.Title level={4} style={{ margin: 0, color: 'var(--text-primary)' }}>
              Interactive Visualization
            </Typography.Title>
            <div style={{ display: 'flex', gap: 'var(--space-2)', flexWrap: 'wrap' }}>
              {hasResult && (
                <Tag color="green" icon={<CheckCircleOutlined />}>
                  Current Result Available ({dataLength} rows)
                </Tag>
              )}
              {hasVisualizableData && (
                <Tag color="blue">
                  Visualizable Data ({columnCount} columns)
                </Tag>
              )}
              {!hasResult && (
                <Tag color="orange" icon={<ExclamationCircleOutlined />}>
                  No Current Result - Using Sample Data
                </Tag>
              )}
              {/* Debug info in development */}
              {process.env.NODE_ENV === 'development' && (
                <Tag color="purple">
                  Debug: hasResult={String(hasResult)}, source={result?.source || 'none'}
                </Tag>
              )}
            </div>
          </div>
        </Card>
      )}

      {/* Chart Controls */}
      <Card
        variant="outlined"
        padding="large"
        title={chartConfig.title}
        actions={[
          ...(autoRefresh ? [
            <Tooltip key="play-pause" title={isPlaying ? 'Pause Auto-refresh' : 'Start Auto-refresh'}>
              <Button
                icon={isPlaying ? <PauseCircleOutlined /> : <PlayCircleOutlined />}
                onClick={() => setIsPlaying(!isPlaying)}
              />
            </Tooltip>
          ] : []),
          <Tooltip key="settings" title="Chart Settings">
            <Button icon={<SettingOutlined />} />
          </Tooltip>,
          ...(enableExport ? [
            <Tooltip key="export" title="Export Data">
              <Button icon={<DownloadOutlined />} onClick={exportData} />
            </Tooltip>
          ] : []),
          <Tooltip key="fullscreen" title="Fullscreen">
            <Button
              icon={<FullscreenOutlined />}
              onClick={() => setIsFullscreen(!isFullscreen)}
            />
          </Tooltip>,
          <Tooltip key="refresh" title="Refresh">
            <Button
              icon={<ReloadOutlined />}
              onClick={() => standalone ? loadStandaloneData() : window.location.reload()}
            />
          </Tooltip>
        ]}
        style={{ marginBottom: 'var(--space-4)' }}
      >

        <Divider />

        <Tabs size="small">
          <TabPane tab="Chart Type" key="type">
            <Row gutter={[16, 16]}>
              <Col span={8}>
                <Text strong>Chart Type</Text>
                <Select
                  value={chartConfig.type}
                  onChange={(value) => setChartConfig(prev => ({ ...prev, type: value }))}
                  style={{ width: '100%', marginTop: 4 }}
                >
                  <Option value="bar">Bar Chart</Option>
                  <Option value="line">Line Chart</Option>
                  <Option value="area">Area Chart</Option>
                  <Option value="pie">Pie Chart</Option>
                  <Option value="scatter">Scatter Plot</Option>
                </Select>
              </Col>
              <Col span={8}>
                <Text strong>X-Axis</Text>
                <Select
                  value={chartConfig.xAxis}
                  onChange={(value) => setChartConfig(prev => ({ ...prev, xAxis: value }))}
                  style={{ width: '100%', marginTop: 4 }}
                >
                  {currentColumns.map(col => (
                    <Option key={col} value={col}>{col}</Option>
                  ))}
                </Select>
              </Col>
              <Col span={8}>
                <Text strong>Y-Axis</Text>
                <Select
                  value={chartConfig.yAxis}
                  onChange={(value) => setChartConfig(prev => ({ ...prev, yAxis: value }))}
                  style={{ width: '100%', marginTop: 4 }}
                >
                  {currentColumns.map(col => (
                    <Option key={col} value={col}>{col}</Option>
                  ))}
                </Select>
              </Col>
            </Row>
          </TabPane>

          <TabPane tab="Appearance" key="appearance">
            <Row gutter={[16, 16]}>
              <Col span={6}>
                <Space direction="vertical" size="small">
                  <Text>Show Grid:</Text>
                  <Switch
                    checked={chartSettings.showGrid}
                    onChange={(checked) => setChartSettings(prev => ({ ...prev, showGrid: checked }))}
                  />
                </Space>
              </Col>
              <Col span={6}>
                <Space direction="vertical" size="small">
                  <Text>Show Legend:</Text>
                  <Switch
                    checked={chartSettings.showLegend}
                    onChange={(checked) => setChartSettings(prev => ({ ...prev, showLegend: checked }))}
                  />
                </Space>
              </Col>
              <Col span={6}>
                <Space direction="vertical" size="small">
                  <Text>Animation:</Text>
                  <Switch
                    checked={chartSettings.enableAnimation}
                    onChange={(checked) => setChartSettings(prev => ({ ...prev, enableAnimation: checked }))}
                  />
                </Space>
              </Col>
              <Col span={6}>
                <Space direction="vertical" size="small">
                  <Text>Show Trend:</Text>
                  <Switch
                    checked={chartConfig.showTrend}
                    onChange={(checked) => setChartConfig(prev => ({ ...prev, showTrend: checked }))}
                  />
                </Space>
              </Col>
            </Row>
          </TabPane>
        </Tabs>
      </Card>

      {/* Filters */}
      {renderFilters()}

      {/* Chart */}
      <Card variant="outlined" padding="large">
        <div style={{ minHeight: chartSettings.height + 50 }}>
          {renderChart()}
        </div>
        <div style={{ marginTop: 'var(--space-4)', textAlign: 'center' }}>
          <Text type="secondary" style={{ color: 'var(--text-secondary)' }}>
            Showing {filteredData.length} of {currentData.length} rows
            {Object.keys(activeFilters).length > 0 && ' (filtered)'}
          </Text>
        </div>
      </Card>
    </div>
  );
};

// Wrapper component for standalone page usage
export const InteractiveVisualizationPage: React.FC<Omit<InteractiveVisualizationProps, 'standalone'>> = (props) => {
  return (
    <PageLayout
      title="Interactive Visualizations"
      subtitle="Create dynamic, interactive charts and explore your data with advanced filtering and customization options"
      breadcrumbs={[
        { title: 'Home', href: '/', icon: <HomeOutlined /> },
        { title: 'Interactive Visualizations', icon: <DotChartOutlined /> }
      ]}
      maxWidth="xl"
    >
      <PageSection
        background="transparent"
        padding="none"
      >
        <InteractiveVisualization {...props} standalone={true} />
      </PageSection>
    </PageLayout>
  );
};
