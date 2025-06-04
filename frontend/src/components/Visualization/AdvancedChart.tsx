import React, { useState, useMemo, useCallback, useEffect } from 'react';
import { Card, Select, Button, Space, Row, Col, Switch, Typography, Tooltip, Alert } from 'antd';
import {
  SettingOutlined,
  DownloadOutlined,
  FullscreenOutlined
} from '@ant-design/icons';
import {
  ResponsiveContainer,
  BarChart, Bar,
  LineChart, Line,
  PieChart, Pie, Cell,
  ScatterChart, Scatter,
  AreaChart, Area,
  XAxis, YAxis,
  CartesianGrid,
  Tooltip as RechartsTooltip,
  Legend,
  Brush
} from 'recharts';
import { AdvancedVisualizationConfig, ChartPerformanceMetrics } from '../../types/visualization';

const { Title, Text } = Typography;
const { Option } = Select;

interface AdvancedChartProps {
  config: AdvancedVisualizationConfig;
  data: any[];
  loading?: boolean;
  onConfigChange?: (config: AdvancedVisualizationConfig) => void;
  onExport?: (format: string) => void;
  performanceMetrics?: ChartPerformanceMetrics;
}

const AdvancedChart: React.FC<AdvancedChartProps> = ({
  config,
  data,
  loading = false,
  onConfigChange,
  onExport,
  performanceMetrics
}) => {
  const [isFullscreen, setIsFullscreen] = useState(false);
  const [showSettings, setShowSettings] = useState(false);
  const [selectedDataPoints, setSelectedDataPoints] = useState<any[]>([]);
  const [chartSettings, setChartSettings] = useState({
    showGrid: config.interaction?.enableCrosshair ?? true,
    showLegend: config.interaction?.enableLegendToggle ?? true,
    enableAnimation: config.animation?.enabled ?? true,
    enableZoom: config.interaction?.enableZoom ?? true,
    enableBrush: config.interaction?.enableBrush ?? false,
    colorScheme: config.theme?.colors?.primary?.[0] ?? '#1890ff'
  });

  // Sync chartSettings with config changes, but preserve user modifications
  useEffect(() => {
    console.log('AdvancedChart - Syncing settings from config:', {
      configCustomSettings: config.customSettings,
      configInteraction: config.interaction,
      configAnimation: config.animation
    });

    setChartSettings(prevSettings => {
      const newSettings = {
        showGrid: config.interaction?.enableCrosshair ?? config.customSettings?.showGrid ?? prevSettings.showGrid,
        showLegend: config.interaction?.enableLegendToggle ?? config.customSettings?.showLegend ?? prevSettings.showLegend,
        enableAnimation: config.animation?.enabled ?? config.customSettings?.enableAnimation ?? prevSettings.enableAnimation,
        enableZoom: config.interaction?.enableZoom ?? config.customSettings?.enableZoom ?? prevSettings.enableZoom,
        enableBrush: config.interaction?.enableBrush ?? config.customSettings?.enableBrush ?? prevSettings.enableBrush,
        colorScheme: config.theme?.colors?.primary?.[0] ?? config.customSettings?.colorScheme ?? prevSettings.colorScheme
      };

      console.log('AdvancedChart - Settings updated:', { prevSettings, newSettings });
      return newSettings;
    });
  }, [config.chartType, config.xAxis, config.yAxis]); // Only sync on major config changes, not on every config update

  // Memoized chart data with performance optimizations
  const chartData = useMemo(() => {
    let processedData = [...data];

    // Validate that the data has the expected keys
    if (data.length > 0) {
      const firstRow = data[0];
      const hasXAxis = config.xAxis && firstRow.hasOwnProperty(config.xAxis);
      const hasYAxis = config.yAxis && firstRow.hasOwnProperty(config.yAxis);

      if (!hasXAxis || !hasYAxis) {
        console.warn('AdvancedChart - Missing required data keys!', {
          expectedXAxis: config.xAxis,
          expectedYAxis: config.yAxis,
          actualKeys: Object.keys(firstRow)
        });
      }
    }

    // Apply data sampling if enabled
    if (config.dataProcessing?.enableSampling && data.length > (config.dataProcessing.sampleSize || 1000)) {
      const sampleSize = config.dataProcessing.sampleSize || 1000;
      const step = Math.floor(data.length / sampleSize);
      processedData = data.filter((_, index) => index % step === 0);
    }

    // Apply outlier filtering if enabled
    if (config.dataProcessing?.outliers?.removeOutliers) {
      // Simple outlier removal based on IQR
      processedData = removeOutliers(processedData, config.xAxis || 'value');
    }

    return processedData;
  }, [data, config]);

  // Color palette based on theme
  const colorPalette = useMemo(() => {
    return config.theme?.colors?.primary || [
      '#1890ff', '#52c41a', '#faad14', '#f5222d', '#722ed1', '#13c2c2'
    ];
  }, [config.theme]);

  const handleSettingChange = useCallback((key: string, value: any) => {
    setChartSettings(prev => ({ ...prev, [key]: value }));

    if (onConfigChange) {
      const updatedConfig = { ...config };

      // Ensure nested objects exist
      if (!updatedConfig.animation) updatedConfig.animation = {};
      if (!updatedConfig.interaction) updatedConfig.interaction = {};

      // Update the appropriate config property based on the setting key
      switch (key) {
        case 'enableAnimation':
          updatedConfig.animation.enabled = value;
          break;
        case 'enableZoom':
          updatedConfig.interaction.enableZoom = value;
          break;
        case 'enableBrush':
          updatedConfig.interaction.enableBrush = value;
          break;
        case 'showGrid':
          updatedConfig.interaction.enableCrosshair = value;
          break;
        case 'showLegend':
          updatedConfig.interaction.enableLegendToggle = value;
          break;
        case 'colorScheme':
          if (!updatedConfig.theme) updatedConfig.theme = {};
          if (!updatedConfig.theme.colors) updatedConfig.theme.colors = {};
          updatedConfig.theme.colors.primary = [value];
          break;
        default:
          // For any other settings, store them in a custom settings object
          if (!updatedConfig.customSettings) updatedConfig.customSettings = {};
          updatedConfig.customSettings[key] = value;
          break;
      }

      console.log('AdvancedChart - Setting changed:', {
        key,
        value,
        updatedConfig: {
          customSettings: updatedConfig.customSettings,
          interaction: updatedConfig.interaction,
          animation: updatedConfig.animation
        }
      });
      onConfigChange(updatedConfig);
    }
  }, [config, onConfigChange]);

  const handleDataPointClick = useCallback((data: any) => {
    if (config.interaction?.enableDataPointSelection) {
      setSelectedDataPoints(prev => {
        const exists = prev.find(p => p.name === data.name);
        if (exists) {
          return prev.filter(p => p.name !== data.name);
        } else {
          return [...prev, data];
        }
      });
    }
  }, [config.interaction]);

  const renderChart = () => {
    const commonProps = {
      width: '100%',
      height: isFullscreen ? 600 : 400,
      data: chartData,
      margin: { top: 20, right: 30, left: 20, bottom: 20 }
    };

    const animationProps = chartSettings.enableAnimation ? {
      animationDuration: config.animation?.duration || 1000,
      animationEasing: config.animation?.easing || 'ease-in-out'
    } : { isAnimationActive: false };

    switch (config.chartType) {
      case 'Bar':
        return (
          <ResponsiveContainer {...commonProps}>
            <BarChart data={chartData} {...animationProps}>
              {chartSettings.showGrid && <CartesianGrid strokeDasharray="3 3" />}
              <XAxis
                dataKey={config.xAxis}
                tick={{ fontSize: 12 }}
                tickFormatter={(value) => {
                  // Format date values for better display
                  if (typeof value === 'string' && value.includes('T')) {
                    return new Date(value).toLocaleDateString();
                  }
                  return value;
                }}
              />
              <YAxis
                tick={{ fontSize: 12 }}
                tickFormatter={(value) => {
                  // Format large numbers
                  if (typeof value === 'number' && value > 1000) {
                    return (value / 1000).toFixed(0) + 'K';
                  }
                  return value;
                }}
              />
              <RechartsTooltip
                content={<CustomTooltip config={config.interaction?.tooltip} />}
                formatter={(value, name) => [
                  typeof value === 'number' ? value.toLocaleString() : value,
                  name
                ]}
                labelFormatter={(label) => {
                  if (typeof label === 'string' && label.includes('T')) {
                    return new Date(label).toLocaleDateString();
                  }
                  return label;
                }}
              />
              {chartSettings.showLegend && <Legend />}
              {chartSettings.enableBrush && <Brush dataKey={config.xAxis} height={30} />}
              {/* Render multiple bars if series is defined, otherwise single bar */}
              {config.series && config.series.length > 1 ? (
                config.series.map((series, index) => (
                  <Bar
                    key={series}
                    dataKey={series}
                    fill={colorPalette[index % colorPalette.length]}
                    onClick={handleDataPointClick}
                    name={series}
                  />
                ))
              ) : (
                <Bar
                  dataKey={config.yAxis || 'value'}
                  fill={chartSettings.colorScheme}
                  onClick={handleDataPointClick}
                >
                  {chartData.map((_entry, index) => (
                    <Cell
                      key={`cell-${index}`}
                      fill={colorPalette[index % colorPalette.length]}
                    />
                  ))}
                </Bar>
              )}
            </BarChart>
          </ResponsiveContainer>
        );

      case 'Line':
        return (
          <ResponsiveContainer {...commonProps}>
            <LineChart data={chartData} {...animationProps}>
              {chartSettings.showGrid && <CartesianGrid strokeDasharray="3 3" />}
              <XAxis dataKey={config.xAxis} />
              <YAxis />
              <RechartsTooltip
                content={<CustomTooltip config={config.interaction?.tooltip} />}
              />
              {chartSettings.showLegend && <Legend />}
              {chartSettings.enableBrush && <Brush dataKey={config.xAxis} height={30} />}
              {/* Render multiple lines if series is defined, otherwise single line */}
              {config.series && config.series.length > 1 ? (
                config.series.map((series, index) => (
                  <Line
                    key={series}
                    type="monotone"
                    dataKey={series}
                    stroke={colorPalette[index % colorPalette.length]}
                    strokeWidth={2}
                    dot={{ fill: colorPalette[index % colorPalette.length], strokeWidth: 2, r: 4 }}
                    activeDot={{ r: 6 }}
                    onClick={handleDataPointClick}
                    name={series}
                  />
                ))
              ) : (
                <Line
                  type="monotone"
                  dataKey={config.yAxis || 'value'}
                  stroke={chartSettings.colorScheme}
                  strokeWidth={2}
                  dot={{ fill: chartSettings.colorScheme, strokeWidth: 2, r: 4 }}
                  activeDot={{ r: 6 }}
                  onClick={handleDataPointClick}
                />
              )}
            </LineChart>
          </ResponsiveContainer>
        );

      case 'Area':
        return (
          <ResponsiveContainer {...commonProps}>
            <AreaChart data={chartData} {...animationProps}>
              {chartSettings.showGrid && <CartesianGrid strokeDasharray="3 3" />}
              <XAxis dataKey={config.xAxis} />
              <YAxis />
              <RechartsTooltip
                content={<CustomTooltip config={config.interaction?.tooltip} />}
              />
              {chartSettings.showLegend && <Legend />}
              {chartSettings.enableBrush && <Brush dataKey={config.xAxis} height={30} />}
              {/* Render multiple areas if series is defined, otherwise single area */}
              {config.series && config.series.length > 1 ? (
                config.series.map((series, index) => (
                  <Area
                    key={series}
                    type="monotone"
                    dataKey={series}
                    stroke={colorPalette[index % colorPalette.length]}
                    fill={colorPalette[index % colorPalette.length]}
                    fillOpacity={0.6}
                    onClick={handleDataPointClick}
                    name={series}
                  />
                ))
              ) : (
                <Area
                  type="monotone"
                  dataKey={config.yAxis || 'value'}
                  stroke={chartSettings.colorScheme}
                  fill={chartSettings.colorScheme}
                  fillOpacity={0.6}
                  onClick={handleDataPointClick}
                />
              )}
            </AreaChart>
          </ResponsiveContainer>
        );

      case 'Pie':
        return (
          <ResponsiveContainer {...commonProps}>
            <PieChart {...animationProps}>
              <Pie
                data={chartData}
                dataKey={config.yAxis || 'value'}
                nameKey={config.xAxis || 'name'}
                cx="50%"
                cy="50%"
                outerRadius={isFullscreen ? 200 : 120}
                fill={chartSettings.colorScheme}
                onClick={handleDataPointClick}
              >
                {chartData.map((_entry, index) => (
                  <Cell
                    key={`cell-${index}`}
                    fill={colorPalette[index % colorPalette.length]}
                  />
                ))}
              </Pie>
              <RechartsTooltip
                content={<CustomTooltip config={config.interaction?.tooltip} />}
              />
              {chartSettings.showLegend && <Legend />}
            </PieChart>
          </ResponsiveContainer>
        );

      case 'Scatter':
        return (
          <ResponsiveContainer {...commonProps}>
            <ScatterChart data={chartData} {...animationProps}>
              {chartSettings.showGrid && <CartesianGrid strokeDasharray="3 3" />}
              <XAxis dataKey={config.xAxis} />
              <YAxis dataKey={config.yAxis || 'value'} />
              <RechartsTooltip
                content={<CustomTooltip config={config.interaction?.tooltip} />}
              />
              {chartSettings.enableBrush && <Brush dataKey={config.xAxis} height={30} />}
              <Scatter
                fill={chartSettings.colorScheme}
                onClick={handleDataPointClick}
              />
            </ScatterChart>
          </ResponsiveContainer>
        );

      case 'Bubble':
        return (
          <ResponsiveContainer {...commonProps}>
            <ScatterChart data={chartData} {...animationProps}>
              {chartSettings.showGrid && <CartesianGrid strokeDasharray="3 3" />}
              <XAxis dataKey={config.xAxis} />
              <YAxis dataKey={config.yAxis || 'value'} />
              <RechartsTooltip
                content={<CustomTooltip config={config.interaction?.tooltip} />}
              />
              {chartSettings.enableBrush && <Brush dataKey={config.xAxis} height={30} />}
              <Scatter
                fill={chartSettings.colorScheme}
                onClick={handleDataPointClick}
              >
                {chartData.map((_entry, index) => (
                  <Cell
                    key={`cell-${index}`}
                    fill={colorPalette[index % colorPalette.length]}
                  />
                ))}
              </Scatter>
            </ScatterChart>
          </ResponsiveContainer>
        );

      case 'Heatmap':
        return (
          <div style={{ padding: '20px', textAlign: 'center' }}>
            <Alert
              message="Heatmap Visualization"
              description="Advanced heatmap visualization requires specialized charting library. This feature will be implemented with D3.js or similar advanced visualization library."
              type="info"
              showIcon
            />
          </div>
        );

      case 'Treemap':
        return (
          <div style={{ padding: '20px', textAlign: 'center' }}>
            <Alert
              message="Treemap Visualization"
              description="Treemap charts require specialized components. This will be implemented with D3.js or similar advanced visualization library."
              type="info"
              showIcon
            />
          </div>
        );

      case 'Gauge':
        return (
          <div style={{ padding: '20px', textAlign: 'center' }}>
            <Alert
              message="Gauge Visualization"
              description="Gauge charts require specialized components. This will be implemented with custom SVG or advanced charting library."
              type="info"
              showIcon
            />
          </div>
        );

      case 'Sunburst':
        return (
          <div style={{ padding: '20px', textAlign: 'center' }}>
            <Alert
              message="Sunburst Visualization"
              description="Sunburst charts require specialized components. This will be implemented with D3.js or similar advanced visualization library."
              type="info"
              showIcon
            />
          </div>
        );

      case 'Radar':
        return (
          <div style={{ padding: '20px', textAlign: 'center' }}>
            <Alert
              message="Radar Chart"
              description="Radar charts require specialized components. This will be implemented with Recharts RadarChart or similar library."
              type="info"
              showIcon
            />
          </div>
        );

      case 'Funnel':
        return (
          <div style={{ padding: '20px', textAlign: 'center' }}>
            <Alert
              message="Funnel Chart"
              description="Funnel charts require specialized components. This will be implemented with custom SVG or advanced charting library."
              type="info"
              showIcon
            />
          </div>
        );

      case 'Timeline':
        return (
          <ResponsiveContainer {...commonProps}>
            <LineChart data={chartData} {...animationProps}>
              {chartSettings.showGrid && <CartesianGrid strokeDasharray="3 3" />}
              <XAxis
                dataKey={config.xAxis}
                type="category"
                scale="time"
              />
              <YAxis />
              <RechartsTooltip
                content={<CustomTooltip config={config.interaction?.tooltip} />}
              />
              {chartSettings.showLegend && <Legend />}
              {chartSettings.enableBrush && <Brush dataKey={config.xAxis} height={30} />}
              <Line
                type="monotone"
                dataKey={config.yAxis || 'value'}
                stroke={chartSettings.colorScheme}
                strokeWidth={3}
                dot={{ fill: chartSettings.colorScheme, strokeWidth: 2, r: 6 }}
                activeDot={{ r: 8 }}
                onClick={handleDataPointClick}
              />
            </LineChart>
          </ResponsiveContainer>
        );

      case 'Histogram':
        return (
          <ResponsiveContainer {...commonProps}>
            <BarChart data={chartData} {...animationProps}>
              {chartSettings.showGrid && <CartesianGrid strokeDasharray="3 3" />}
              <XAxis dataKey={config.xAxis} />
              <YAxis />
              <RechartsTooltip
                content={<CustomTooltip config={config.interaction?.tooltip} />}
              />
              {chartSettings.showLegend && <Legend />}
              <Bar
                dataKey={config.yAxis || 'frequency'}
                fill={chartSettings.colorScheme}
                onClick={handleDataPointClick}
              >
                {chartData.map((_entry, index) => (
                  <Cell
                    key={`cell-${index}`}
                    fill={colorPalette[index % colorPalette.length]}
                  />
                ))}
              </Bar>
            </BarChart>
          </ResponsiveContainer>
        );

      case 'BoxPlot':
        return (
          <div style={{ padding: '20px', textAlign: 'center' }}>
            <Alert
              message="Box Plot Visualization"
              description="Box plots require specialized statistical charting components. This will be implemented with D3.js or similar advanced visualization library."
              type="info"
              showIcon
            />
          </div>
        );

      case 'Violin':
        return (
          <div style={{ padding: '20px', textAlign: 'center' }}>
            <Alert
              message="Violin Plot Visualization"
              description="Violin plots require specialized statistical charting components. This will be implemented with D3.js or similar advanced visualization library."
              type="info"
              showIcon
            />
          </div>
        );

      default:
        return (
          <div style={{ padding: '40px', textAlign: 'center' }}>
            <Text>Chart type "{config.chartType}" is not yet implemented</Text>
          </div>
        );
    }
  };

  const renderSettings = () => (
    <Card size="small" title="Chart Settings" style={{ marginBottom: 16 }}>
      <Row gutter={16}>
        <Col span={12}>
          <Space direction="vertical" style={{ width: '100%' }}>
            <div>
              <Text>Show Grid</Text>
              <Switch
                checked={chartSettings.showGrid}
                onChange={(checked) => handleSettingChange('showGrid', checked)}
                size="small"
              />
            </div>
            <div>
              <Text>Show Legend</Text>
              <Switch
                checked={chartSettings.showLegend}
                onChange={(checked) => handleSettingChange('showLegend', checked)}
                size="small"
              />
            </div>
            <div>
              <Text>Enable Animation</Text>
              <Switch
                checked={chartSettings.enableAnimation}
                onChange={(checked) => handleSettingChange('enableAnimation', checked)}
                size="small"
              />
            </div>
          </Space>
        </Col>
        <Col span={12}>
          <Space direction="vertical" style={{ width: '100%' }}>
            <div>
              <Text>Enable Zoom</Text>
              <Switch
                checked={chartSettings.enableZoom}
                onChange={(checked) => handleSettingChange('enableZoom', checked)}
                size="small"
              />
            </div>
            <div>
              <Text>Enable Brush</Text>
              <Switch
                checked={chartSettings.enableBrush}
                onChange={(checked) => handleSettingChange('enableBrush', checked)}
                size="small"
              />
            </div>
            <div>
              <Text>Color Scheme</Text>
              <Select
                value={chartSettings.colorScheme}
                onChange={(value) => handleSettingChange('colorScheme', value)}
                size="small"
                style={{ width: '100%' }}
              >
                <Option value="#1890ff">Blue</Option>
                <Option value="#52c41a">Green</Option>
                <Option value="#faad14">Orange</Option>
                <Option value="#f5222d">Red</Option>
                <Option value="#722ed1">Purple</Option>
              </Select>
            </div>
          </Space>
        </Col>
      </Row>
    </Card>
  );

  return (
    <div className={`advanced-chart ${isFullscreen ? 'fullscreen' : ''}`}>
      <Card
        title={
          <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
            <Title level={4} style={{ margin: 0 }}>{config.title}</Title>
            <Space>
              <Tooltip title="Chart Settings">
                <Button
                  icon={<SettingOutlined />}
                  size="small"
                  onClick={() => setShowSettings(!showSettings)}
                />
              </Tooltip>
              <Tooltip title="Fullscreen">
                <Button
                  icon={<FullscreenOutlined />}
                  size="small"
                  onClick={() => setIsFullscreen(!isFullscreen)}
                />
              </Tooltip>
              <Tooltip title="Export">
                <Button
                  icon={<DownloadOutlined />}
                  size="small"
                  onClick={() => onExport?.('png')}
                />
              </Tooltip>
            </Space>
          </div>
        }
        loading={loading}
        style={{
          height: isFullscreen ? '100vh' : 'auto',
          position: isFullscreen ? 'fixed' : 'relative',
          top: isFullscreen ? 0 : 'auto',
          left: isFullscreen ? 0 : 'auto',
          right: isFullscreen ? 0 : 'auto',
          bottom: isFullscreen ? 0 : 'auto',
          zIndex: isFullscreen ? 1000 : 'auto'
        }}
      >
        {showSettings && renderSettings()}

        <div style={{ minHeight: isFullscreen ? 600 : 400 }}>
          {renderChart()}
        </div>

        {/* Performance Metrics */}
        {performanceMetrics && (
          <div style={{ marginTop: 16, padding: 8, backgroundColor: '#f5f5f5', borderRadius: 4 }}>
            <Text type="secondary" style={{ fontSize: 12 }}>
              Render Time: {performanceMetrics.renderTime}ms |
              Memory: {performanceMetrics.memoryUsage}MB |
              Data Points: {performanceMetrics.dataPointsRendered}
              {performanceMetrics.usedSampling && ' (Sampled)'}
              {performanceMetrics.usedWebGL && ' (WebGL)'}
            </Text>
          </div>
        )}

        {/* Selected Data Points */}
        {selectedDataPoints.length > 0 && (
          <div style={{ marginTop: 16 }}>
            <Text strong>Selected: </Text>
            <Text>{selectedDataPoints.map(p => p.name).join(', ')}</Text>
            <Button
              size="small"
              onClick={() => setSelectedDataPoints([])}
              style={{ marginLeft: 8 }}
            >
              Clear
            </Button>
          </div>
        )}
      </Card>
    </div>
  );
};

// Custom tooltip component
const CustomTooltip = ({ active, payload, label, config }: any) => {
  if (active && payload && payload.length) {
    return (
      <div style={{
        backgroundColor: 'white',
        padding: '8px',
        border: '1px solid #ccc',
        borderRadius: '4px',
        boxShadow: '0 2px 8px rgba(0,0,0,0.1)'
      }}>
        <p style={{ margin: 0, fontWeight: 'bold' }}>{`${label}`}</p>
        {payload.map((entry: any, index: number) => (
          <p key={index} style={{ margin: 0, color: entry.color }}>
            {`${entry.name}: ${entry.value}`}
          </p>
        ))}
        {config?.showStatistics && payload.length > 1 && (
          <div style={{ marginTop: 4, fontSize: 12, color: '#666' }}>
            <p style={{ margin: 0 }}>
              Avg: {(payload.reduce((sum: number, p: any) => sum + p.value, 0) / payload.length).toFixed(2)}
            </p>
          </div>
        )}
      </div>
    );
  }
  return null;
};

// Utility function to remove outliers
const removeOutliers = (data: any[], key: string) => {
  const values = data.map(d => d[key]).filter(v => typeof v === 'number').sort((a, b) => a - b);
  const q1 = values[Math.floor(values.length * 0.25)];
  const q3 = values[Math.floor(values.length * 0.75)];
  const iqr = q3 - q1;
  const lowerBound = q1 - 1.5 * iqr;
  const upperBound = q3 + 1.5 * iqr;

  return data.filter(d => {
    const value = d[key];
    return typeof value !== 'number' || (value >= lowerBound && value <= upperBound);
  });
};

export default AdvancedChart;
