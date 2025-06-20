/**
 * Unified Chart Component
 * Consolidates all chart functionality from Advanced and Enhanced components
 */

import React, { useState, useEffect, useMemo } from 'react';
import {
  Card,
  Select,
  Button,
  Space,
  Typography,
  Tooltip,
  Switch,
  Row,
  Col,
  Spin
} from 'antd';
import {
  FullscreenOutlined,
  SettingOutlined,
  DownloadOutlined,
  BugOutlined
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
  AreaChart,
  Area,
  ScatterChart,
  Scatter,
  XAxis,
  YAxis,
  CartesianGrid,
  Tooltip as RechartsTooltip,
  Legend,
  Brush
} from 'recharts';

const { Text } = Typography;
const { Option } = Select;

interface ChartConfig {
  type: 'bar' | 'line' | 'pie' | 'area' | 'scatter' | 'heatmap' | 'network' | 'treemap';
  title: string;
  xAxis: string;
  yAxis: string;
  colorScheme: string;
  showGrid: boolean;
  showLegend: boolean;
  showAnimation: boolean;
  showTrend: boolean;
  interactive: boolean;
  responsive: boolean;
}

interface ChartProps {
  data: any[];
  columns: string[];
  config?: Partial<ChartConfig>;
  onConfigChange?: (config: ChartConfig) => void;
  height?: number;
  width?: string;
  loading?: boolean;
  debug?: boolean;
}

export const Chart: React.FC<ChartProps> = ({
  data,
  columns,
  config: initialConfig,
  onConfigChange,
  height = 400,
  width = '100%',
  loading = false,
  debug = false
}) => {
  // Use columns or extract from data if columns not available
  const effectiveColumns = useMemo(() => {
    if (columns && Array.isArray(columns) && columns.length > 0) {
      return columns;
    }
    if (data && data.length > 0) {
      return Object.keys(data[0]);
    }
    return [];
  }, [columns, data]);

  const [config, setConfig] = useState<ChartConfig>({
    type: 'bar',
    title: 'Chart',
    xAxis: (effectiveColumns && effectiveColumns.length > 0 && effectiveColumns[0]) ? effectiveColumns[0] : 'name',
    yAxis: (effectiveColumns && effectiveColumns.length > 1 && effectiveColumns[1]) ? effectiveColumns[1] : 'value',
    colorScheme: 'default',
    showGrid: true,
    showLegend: true,
    showAnimation: true,
    showTrend: false,
    interactive: true,
    responsive: true,
    ...initialConfig
  });

  const [fullscreen, setFullscreen] = useState(false);
  const [showSettings, setShowSettings] = useState(false);

  useEffect(() => {
    if (initialConfig) {
      setConfig(prev => ({ ...prev, ...initialConfig }));
    }
  }, [initialConfig]);

  const handleConfigChange = (newConfig: Partial<ChartConfig>) => {
    const updatedConfig = { ...config, ...newConfig };
    setConfig(updatedConfig);
    onConfigChange?.(updatedConfig);
  };

  const getColorScheme = (scheme: string) => {
    const schemes = {
      default: ['#8884d8', '#82ca9d', '#ffc658', '#ff7300', '#00ff00'],
      blue: ['#1890ff', '#40a9ff', '#69c0ff', '#91d5ff', '#bae7ff'],
      green: ['#52c41a', '#73d13d', '#95de64', '#b7eb8f', '#d9f7be'],
      red: ['#ff4d4f', '#ff7875', '#ffa39e', '#ffccc7', '#ffe1e1'],
      purple: ['#722ed1', '#9254de', '#b37feb', '#d3adf7', '#efdbff']
    };
    return schemes[scheme as keyof typeof schemes] || schemes.default;
  };

  const processedData = useMemo(() => {
    if (!data || !Array.isArray(data)) return [];
    return data.slice(0, 1000); // Limit for performance
  }, [data]);

  const getColor = (index: number) => {
    const colors = getColorScheme(config.colorScheme);
    return colors[index % colors.length];
  };

  const renderChart = () => {
    if (loading) {
      return (
        <div style={{ display: 'flex', justifyContent: 'center', alignItems: 'center', height }}>
          <Spin size="large" />
        </div>
      );
    }

    if (!processedData.length) {
      return (
        <div style={{ display: 'flex', justifyContent: 'center', alignItems: 'center', height }}>
          <Text type="secondary">No data available</Text>
        </div>
      );
    }

    const commonProps = {
      width: '100%',
      height: fullscreen ? 600 : height
    };

    switch (config.type) {
      case 'bar':
        return (
          <ResponsiveContainer {...commonProps}>
            <BarChart data={processedData}>
              {config.showGrid && <CartesianGrid strokeDasharray="3 3" />}
              <XAxis dataKey={config.xAxis} />
              <YAxis />
              <RechartsTooltip />
              {config.showLegend && <Legend />}
              <Bar
                dataKey={config.yAxis || 'value'}
                fill={getColor(0)}
                animationDuration={config.showAnimation ? 1000 : 0}
              />
              {config.interactive && <Brush dataKey={config.xAxis} height={30} />}
            </BarChart>
          </ResponsiveContainer>
        );

      case 'line':
        return (
          <ResponsiveContainer {...commonProps}>
            <LineChart data={processedData}>
              {config.showGrid && <CartesianGrid strokeDasharray="3 3" />}
              <XAxis dataKey={config.xAxis} />
              <YAxis />
              <RechartsTooltip />
              {config.showLegend && <Legend />}
              <Line
                type="monotone"
                dataKey={config.yAxis}
                stroke={getColor(0)}
                animationDuration={config.showAnimation ? 1000 : 0}
              />
              {config.interactive && <Brush dataKey={config.xAxis} height={30} />}
            </LineChart>
          </ResponsiveContainer>
        );

      case 'area':
        return (
          <ResponsiveContainer {...commonProps}>
            <AreaChart data={processedData}>
              {config.showGrid && <CartesianGrid strokeDasharray="3 3" />}
              <XAxis dataKey={config.xAxis} />
              <YAxis />
              <RechartsTooltip />
              {config.showLegend && <Legend />}
              <Area
                type="monotone"
                dataKey={config.yAxis || 'value'}
                stroke={getColor(0)}
                fill={getColor(0)}
                animationDuration={config.showAnimation ? 1000 : 0}
              />
              {config.interactive && <Brush dataKey={config.xAxis} height={30} />}
            </AreaChart>
          </ResponsiveContainer>
        );

      case 'pie':
        return (
          <ResponsiveContainer {...commonProps}>
            <PieChart>
              <Pie
                data={processedData}
                dataKey={config.yAxis || 'value'}
                nameKey={config.xAxis}
                cx="50%"
                cy="50%"
                outerRadius={Math.min(height * 0.3, 120)}
                fill={getColor(0)}
                animationDuration={config.showAnimation ? 1000 : 0}
              >
                {processedData.map((entry, index) => (
                  <Cell key={`cell-${index}`} fill={getColor(index)} />
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
            <ScatterChart data={processedData}>
              {config.showGrid && <CartesianGrid strokeDasharray="3 3" />}
              <XAxis dataKey={config.xAxis} />
              <YAxis dataKey={config.yAxis} />
              <RechartsTooltip />
              {config.showLegend && <Legend />}
              <Scatter
                dataKey={config.yAxis}
                fill={getColor(0)}
                animationDuration={config.showAnimation ? 1000 : 0}
              />
            </ScatterChart>
          </ResponsiveContainer>
        );

      default:
        return (
          <div style={{ display: 'flex', justifyContent: 'center', alignItems: 'center', height }}>
            <Text>Chart type "{config.type}" not implemented</Text>
          </div>
        );
    }
  };

  return (
    <Card
      title={config.title}
      extra={
        <Space>
          {debug && (
            <Tooltip title="Debug Info">
              <Button size="small" icon={<BugOutlined />} />
            </Tooltip>
          )}
          <Tooltip title="Chart Settings">
            <Button
              size="small"
              icon={<SettingOutlined />}
              onClick={() => setShowSettings(!showSettings)}
            />
          </Tooltip>
          <Tooltip title="Download Chart">
            <Button size="small" icon={<DownloadOutlined />} />
          </Tooltip>
          <Tooltip title="Fullscreen">
            <Button
              size="small"
              icon={<FullscreenOutlined />}
              onClick={() => setFullscreen(!fullscreen)}
            />
          </Tooltip>
        </Space>
      }
      style={{ width }}
    >
      {renderChart()}
    </Card>
  );
};
