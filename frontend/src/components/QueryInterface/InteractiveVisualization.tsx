import React, { useState, useEffect, useMemo, useCallback } from 'react';
import { Card, Select, Button, Space, Row, Col, Switch, Slider, Typography, Tooltip, Spin } from 'antd';
import { SettingOutlined, DownloadOutlined, FullscreenOutlined, ReloadOutlined } from '@ant-design/icons';
import {
  InteractiveVisualizationConfig,
  ColumnInfo,
  VisualizationRequest
} from '../../types/query';
import { ResponsiveContainer, BarChart, Bar, LineChart, Line, PieChart, Pie, Cell, ScatterChart, Scatter, XAxis, YAxis, CartesianGrid, Tooltip as RechartsTooltip, Legend } from 'recharts';
import '../Interactive/InteractiveVisualization.css';

const { Title, Text } = Typography;
const { Option } = Select;

interface InteractiveVisualizationProps {
  data: any[];
  columns: ColumnInfo[];
  query: string;
  onConfigChange?: (config: InteractiveVisualizationConfig) => void;
}

export const InteractiveVisualization: React.FC<InteractiveVisualizationProps> = ({
  data,
  columns,
  query,
  onConfigChange
}) => {
  const [loading, setLoading] = useState(false);
  const [config, setConfig] = useState<InteractiveVisualizationConfig | null>(null);
  const [activeFilters, setActiveFilters] = useState<Record<string, any>>({});
  const [chartSettings, setChartSettings] = useState({
    showGrid: true,
    showLegend: true,
    enableAnimation: true,
    height: 400
  });

  const generateInteractiveConfig = useCallback(async () => {
    setLoading(true);
    try {
      const request: VisualizationRequest = {
        query,
        columns,
        data: data.slice(0, 100) // Send sample for config generation
      };

      const response = await fetch(`${process.env.REACT_APP_API_URL}/api/visualization/interactive`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Authorization': `Bearer ${localStorage.getItem('token')}`
        },
        body: JSON.stringify(request)
      });

      if (response.ok) {
        const interactiveConfig: InteractiveVisualizationConfig = await response.json();
        setConfig(interactiveConfig);
        onConfigChange?.(interactiveConfig);
      } else {
        console.error('Failed to generate interactive visualization config');
      }
    } catch (error) {
      console.error('Error generating interactive config:', error);
    } finally {
      setLoading(false);
    }
  }, [query, columns, data, onConfigChange]);

  // Generate interactive visualization config
  useEffect(() => {
    if (data.length > 0 && columns.length > 0) {
      generateInteractiveConfig();
    }
  }, [data, columns, query, generateInteractiveConfig]);

  // Filter data based on active filters
  const filteredData = useMemo(() => {
    if (!config || Object.keys(activeFilters).length === 0) {
      return data;
    }

    return data.filter(row => {
      return Object.entries(activeFilters).every(([columnName, filterValue]) => {
        const filter = config.filters.find(f => f.columnName === columnName);
        if (!filter || filterValue === undefined || filterValue === null) return true;

        const cellValue = row[columnName];

        switch (filter.filterType) {
          case 'range':
            return cellValue >= filterValue[0] && cellValue <= filterValue[1];
          case 'multiSelect':
            return Array.isArray(filterValue) ? filterValue.includes(cellValue) : cellValue === filterValue;
          case 'dateRange':
            const cellDate = new Date(cellValue);
            return cellDate >= new Date(filterValue[0]) && cellDate <= new Date(filterValue[1]);
          default:
            return true;
        }
      });
    });
  }, [data, config, activeFilters]);

  // Get unique values for multiSelect filters
  const getFilterOptions = (columnName: string): any[] => {
    const uniqueValues = [...new Set(data.map(row => row[columnName]))];
    return uniqueValues.filter(val => val !== null && val !== undefined);
  };

  // Get numeric range for range filters
  const getNumericRange = (columnName: string): [number, number] => {
    const values = data.map(row => parseFloat(row[columnName])).filter(val => !isNaN(val));
    return [Math.min(...values), Math.max(...values)];
  };

  // Render filter controls
  const renderFilters = () => {
    if (!config || config.filters.length === 0) return null;

    return (
      <Card size="small" title="Filters" style={{ marginBottom: '16px' }}>
        <Row gutter={[16, 16]}>
          {config.filters.map(filter => (
            <Col span={8} key={filter.columnName}>
              <div>
                <Text strong>{filter.label}</Text>
                {filter.filterType === 'multiSelect' && (
                  <Select
                    mode="multiple"
                    placeholder={`Select ${filter.label}`}
                    style={{ width: '100%', marginTop: '4px' }}
                    value={activeFilters[filter.columnName] || []}
                    onChange={(value) => setActiveFilters(prev => ({ ...prev, [filter.columnName]: value }))}
                  >
                    {getFilterOptions(filter.columnName).map(option => (
                      <Option key={option} value={option}>{String(option)}</Option>
                    ))}
                  </Select>
                )}
                {filter.filterType === 'range' && (
                  <Slider
                    range
                    style={{ marginTop: '8px' }}
                    min={getNumericRange(filter.columnName)[0]}
                    max={getNumericRange(filter.columnName)[1]}
                    value={activeFilters[filter.columnName] || getNumericRange(filter.columnName)}
                    onChange={(value) => setActiveFilters(prev => ({ ...prev, [filter.columnName]: value }))}
                    tooltip={{ formatter: (value) => value?.toFixed(2) }}
                  />
                )}
              </div>
            </Col>
          ))}
        </Row>
      </Card>
    );
  };

  // Render chart based on type
  const renderChart = () => {
    if (!config || filteredData.length === 0) return null;

    const { baseVisualization } = config;
    const chartData = filteredData.slice(0, 1000); // Limit for performance

    const commonProps = {
      width: '100%',
      height: chartSettings.height,
      data: chartData
    };

    switch (baseVisualization.type) {
      case 'bar':
        return (
          <ResponsiveContainer {...commonProps}>
            <BarChart data={chartData}>
              {chartSettings.showGrid && <CartesianGrid strokeDasharray="3 3" />}
              <XAxis dataKey={baseVisualization.xAxis} />
              <YAxis />
              <RechartsTooltip />
              {chartSettings.showLegend && <Legend />}
              <Bar
                dataKey={baseVisualization.yAxis || 'value'}
                fill="#8884d8"
                animationDuration={chartSettings.enableAnimation ? 1000 : 0}
              />
            </BarChart>
          </ResponsiveContainer>
        );

      case 'line':
        return (
          <ResponsiveContainer {...commonProps}>
            <LineChart data={chartData}>
              {chartSettings.showGrid && <CartesianGrid strokeDasharray="3 3" />}
              <XAxis dataKey={baseVisualization.xAxis} />
              <YAxis />
              <RechartsTooltip />
              {chartSettings.showLegend && <Legend />}
              <Line
                type="monotone"
                dataKey={baseVisualization.yAxis}
                stroke="#8884d8"
                animationDuration={chartSettings.enableAnimation ? 1000 : 0}
              />
            </LineChart>
          </ResponsiveContainer>
        );

      case 'pie':
        return (
          <ResponsiveContainer {...commonProps}>
            <PieChart>
              <Pie
                data={chartData}
                dataKey={baseVisualization.yAxis || 'value'}
                nameKey={baseVisualization.xAxis || 'name'}
                cx="50%"
                cy="50%"
                outerRadius={150}
                fill="#8884d8"
                animationDuration={chartSettings.enableAnimation ? 1000 : 0}
              >
                {chartData.map((entry, index) => (
                  <Cell key={`cell-${index}`} fill={`hsl(${index * 45}, 70%, 60%)`} />
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
            <ScatterChart data={chartData}>
              {chartSettings.showGrid && <CartesianGrid strokeDasharray="3 3" />}
              <XAxis dataKey={baseVisualization.xAxis} />
              <YAxis dataKey={baseVisualization.yAxis} />
              <RechartsTooltip />
              <Scatter fill="#8884d8" />
            </ScatterChart>
          </ResponsiveContainer>
        );

      default:
        return (
          <div style={{ padding: '40px', textAlign: 'center' }}>
            <Text>Chart type "{baseVisualization.type}" not yet implemented</Text>
          </div>
        );
    }
  };

  // Export chart data
  const exportData = () => {
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
            <Text>Generating interactive visualization...</Text>
          </div>
        </div>
      </Card>
    );
  }

  if (!config) {
    return (
      <Card>
        <div style={{ textAlign: 'center', padding: '40px' }}>
          <Text>No visualization configuration available</Text>
          <div style={{ marginTop: '16px' }}>
            <Button onClick={generateInteractiveConfig}>
              Generate Visualization
            </Button>
          </div>
        </div>
      </Card>
    );
  }

  return (
    <div className="interactive-visualization">
      {/* Chart Controls */}
      <Card size="small" className="hover-lift" style={{ marginBottom: '16px' }}>
        <Row justify="space-between" align="middle">
          <Col>
            <Title level={4} style={{ margin: 0 }}>
              {config.baseVisualization.title || 'Interactive Visualization'}
            </Title>
          </Col>
          <Col>
            <Space>
              <Tooltip title="Chart Settings">
                <Button icon={<SettingOutlined />} />
              </Tooltip>
              <Tooltip title="Export Data">
                <Button icon={<DownloadOutlined />} onClick={exportData} />
              </Tooltip>
              <Tooltip title="Fullscreen">
                <Button icon={<FullscreenOutlined />} />
              </Tooltip>
              <Tooltip title="Refresh">
                <Button icon={<ReloadOutlined />} onClick={generateInteractiveConfig} />
              </Tooltip>
            </Space>
          </Col>
        </Row>

        <Row gutter={16} style={{ marginTop: '16px' }}>
          <Col span={4}>
            <Space direction="vertical" size="small">
              <Text>Show Grid:</Text>
              <Switch
                checked={chartSettings.showGrid}
                onChange={(checked) => setChartSettings(prev => ({ ...prev, showGrid: checked }))}
              />
            </Space>
          </Col>
          <Col span={4}>
            <Space direction="vertical" size="small">
              <Text>Show Legend:</Text>
              <Switch
                checked={chartSettings.showLegend}
                onChange={(checked) => setChartSettings(prev => ({ ...prev, showLegend: checked }))}
              />
            </Space>
          </Col>
          <Col span={4}>
            <Space direction="vertical" size="small">
              <Text>Animation:</Text>
              <Switch
                checked={chartSettings.enableAnimation}
                onChange={(checked) => setChartSettings(prev => ({ ...prev, enableAnimation: checked }))}
              />
            </Space>
          </Col>
          <Col span={8}>
            <Space direction="vertical" size="small" style={{ width: '100%' }}>
              <Text>Height: {chartSettings.height}px</Text>
              <Slider
                min={300}
                max={800}
                value={chartSettings.height}
                onChange={(value) => setChartSettings(prev => ({ ...prev, height: value }))}
              />
            </Space>
          </Col>
        </Row>
      </Card>

      {/* Filters */}
      {renderFilters()}

      {/* Chart */}
      <Card>
        <div style={{ minHeight: chartSettings.height + 50 }}>
          {renderChart()}
        </div>
        <div style={{ marginTop: '16px', textAlign: 'center' }}>
          <Text type="secondary">
            Showing {filteredData.length} of {data.length} rows
            {Object.keys(activeFilters).length > 0 && ' (filtered)'}
          </Text>
        </div>
      </Card>

      {/* Interactive Features Info */}
      {config.interactiveFeatures && Object.keys(config.interactiveFeatures).length > 0 && (
        <Card size="small" title="Interactive Features" style={{ marginTop: '16px' }}>
          <Space wrap>
            {Object.entries(config.interactiveFeatures).map(([feature, enabled]) => (
              <Text key={feature} type={enabled ? 'success' : 'secondary'}>
                {feature}: {enabled ? '✓' : '✗'}
              </Text>
            ))}
          </Space>
        </Card>
      )}
    </div>
  );
};

export default InteractiveVisualization;
