/**
 * Advanced Chart Component
 * Wrapper for various chart types with enhanced features
 */

import React, { useState, useEffect, useMemo } from 'react';
import { Card, Select, Button, Space, Tooltip, Spin } from 'antd';
import {
  BarChartOutlined,
  LineChartOutlined,
  PieChartOutlined,
  AreaChartOutlined,
  DotChartOutlined as ScatterChartOutlined,
  DownloadOutlined,
  FullscreenOutlined,
  SettingOutlined
} from '@ant-design/icons';
import {
  BarChart,
  Bar,
  LineChart,
  Line,
  PieChart,
  Pie,
  AreaChart,
  Area,
  ScatterChart,
  Scatter,
  XAxis,
  YAxis,
  CartesianGrid,
  Tooltip as RechartsTooltip,
  Legend,
  ResponsiveContainer,
  Cell
} from 'recharts';

const { Option } = Select;

export type ChartType = 'bar' | 'line' | 'pie' | 'area' | 'scatter';

interface ChartData {
  [key: string]: any;
}

interface AdvancedChartProps {
  data: ChartData[];
  type?: ChartType;
  title?: string;
  xAxisKey?: string;
  yAxisKey?: string;
  width?: number | string;
  height?: number | string;
  loading?: boolean;
  showControls?: boolean;
  showLegend?: boolean;
  showTooltip?: boolean;
  colors?: string[];
  onTypeChange?: (type: ChartType) => void;
  onExport?: () => void;
  onFullscreen?: () => void;
  onSettings?: () => void;
}

const DEFAULT_COLORS = [
  '#667eea', '#764ba2', '#f093fb', '#f5576c',
  '#4facfe', '#00f2fe', '#43e97b', '#38f9d7',
  '#ffecd2', '#fcb69f', '#a8edea', '#fed6e3'
];

const CHART_ICONS = {
  bar: BarChartOutlined,
  line: LineChartOutlined,
  pie: PieChartOutlined,
  area: AreaChartOutlined,
  scatter: ScatterChartOutlined
};

export const AdvancedChart: React.FC<AdvancedChartProps> = ({
  data = [],
  type = 'bar',
  title,
  xAxisKey = 'name',
  yAxisKey = 'value',
  width = '100%',
  height = 400,
  loading = false,
  showControls = true,
  showLegend = true,
  showTooltip = true,
  colors = DEFAULT_COLORS,
  onTypeChange,
  onExport,
  onFullscreen,
  onSettings
}) => {
  const [currentType, setCurrentType] = useState<ChartType>(type);

  useEffect(() => {
    setCurrentType(type);
  }, [type]);

  const handleTypeChange = (newType: ChartType) => {
    setCurrentType(newType);
    onTypeChange?.(newType);
  };

  const chartData = useMemo(() => {
    if (!data || data.length === 0) return [];
    
    // Ensure data has the required keys
    return data.map((item, index) => ({
      ...item,
      [xAxisKey]: item[xAxisKey] || `Item ${index + 1}`,
      [yAxisKey]: typeof item[yAxisKey] === 'number' ? item[yAxisKey] : 0
    }));
  }, [data, xAxisKey, yAxisKey]);

  const renderChart = () => {
    if (loading) {
      return (
        <div style={{ 
          display: 'flex', 
          alignItems: 'center', 
          justifyContent: 'center', 
          height: height 
        }}>
          <Spin size="large" />
        </div>
      );
    }

    if (!chartData || chartData.length === 0) {
      return (
        <div style={{ 
          display: 'flex', 
          alignItems: 'center', 
          justifyContent: 'center', 
          height: height,
          color: '#8c8c8c'
        }}>
          No data available
        </div>
      );
    }

    const commonProps = {
      data: chartData,
      margin: { top: 20, right: 30, left: 20, bottom: 5 }
    };

    switch (currentType) {
      case 'bar':
        return (
          <ResponsiveContainer width={width} height={height}>
            <BarChart {...commonProps}>
              <CartesianGrid strokeDasharray="3 3" />
              <XAxis dataKey={xAxisKey} />
              <YAxis />
              {showTooltip && <RechartsTooltip />}
              {showLegend && <Legend />}
              <Bar dataKey={yAxisKey} fill={colors[0]} />
            </BarChart>
          </ResponsiveContainer>
        );

      case 'line':
        return (
          <ResponsiveContainer width={width} height={height}>
            <LineChart {...commonProps}>
              <CartesianGrid strokeDasharray="3 3" />
              <XAxis dataKey={xAxisKey} />
              <YAxis />
              {showTooltip && <RechartsTooltip />}
              {showLegend && <Legend />}
              <Line 
                type="monotone" 
                dataKey={yAxisKey} 
                stroke={colors[0]} 
                strokeWidth={2}
                dot={{ fill: colors[0] }}
              />
            </LineChart>
          </ResponsiveContainer>
        );

      case 'area':
        return (
          <ResponsiveContainer width={width} height={height}>
            <AreaChart {...commonProps}>
              <CartesianGrid strokeDasharray="3 3" />
              <XAxis dataKey={xAxisKey} />
              <YAxis />
              {showTooltip && <RechartsTooltip />}
              {showLegend && <Legend />}
              <Area 
                type="monotone" 
                dataKey={yAxisKey} 
                stroke={colors[0]} 
                fill={colors[0]}
                fillOpacity={0.6}
              />
            </AreaChart>
          </ResponsiveContainer>
        );

      case 'pie':
        return (
          <ResponsiveContainer width={width} height={height}>
            <PieChart>
              <Pie
                data={chartData}
                dataKey={yAxisKey}
                nameKey={xAxisKey}
                cx="50%"
                cy="50%"
                outerRadius={Math.min(Number(height) || 400, 200) * 0.3}
                fill={colors[0]}
                label
              >
                {chartData.map((entry, index) => (
                  <Cell key={`cell-${index}`} fill={colors[index % colors.length]} />
                ))}
              </Pie>
              {showTooltip && <RechartsTooltip />}
              {showLegend && <Legend />}
            </PieChart>
          </ResponsiveContainer>
        );

      case 'scatter':
        return (
          <ResponsiveContainer width={width} height={height}>
            <ScatterChart {...commonProps}>
              <CartesianGrid strokeDasharray="3 3" />
              <XAxis dataKey={xAxisKey} />
              <YAxis dataKey={yAxisKey} />
              {showTooltip && <RechartsTooltip />}
              {showLegend && <Legend />}
              <Scatter dataKey={yAxisKey} fill={colors[0]} />
            </ScatterChart>
          </ResponsiveContainer>
        );

      default:
        return null;
    }
  };

  return (
    <Card
      title={title}
      extra={showControls && (
        <Space>
          <Select
            value={currentType}
            onChange={handleTypeChange}
            style={{ width: 120 }}
            size="small"
          >
            {Object.entries(CHART_ICONS).map(([chartType, IconComponent]) => (
              <Option key={chartType} value={chartType}>
                <Space>
                  <IconComponent />
                  {chartType.charAt(0).toUpperCase() + chartType.slice(1)}
                </Space>
              </Option>
            ))}
          </Select>
          
          {onSettings && (
            <Tooltip title="Chart Settings">
              <Button 
                type="text" 
                icon={<SettingOutlined />} 
                size="small"
                onClick={onSettings}
              />
            </Tooltip>
          )}
          
          {onExport && (
            <Tooltip title="Export Chart">
              <Button 
                type="text" 
                icon={<DownloadOutlined />} 
                size="small"
                onClick={onExport}
              />
            </Tooltip>
          )}
          
          {onFullscreen && (
            <Tooltip title="Fullscreen">
              <Button 
                type="text" 
                icon={<FullscreenOutlined />} 
                size="small"
                onClick={onFullscreen}
              />
            </Tooltip>
          )}
        </Space>
      )}
      style={{ width: '100%' }}
    >
      {renderChart()}
    </Card>
  );
};

export default AdvancedChart;
