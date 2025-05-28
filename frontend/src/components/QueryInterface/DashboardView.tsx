import React, { useState, useEffect, useCallback } from 'react';
import { Card, Row, Col, Typography, Button, Space, Spin, Select, DatePicker, Tooltip } from 'antd';
import { FullscreenOutlined, DownloadOutlined, ReloadOutlined, SettingOutlined } from '@ant-design/icons';
import { DashboardConfig, VisualizationConfig, ColumnInfo, VisualizationRequest } from '../../types/query';
import { ResponsiveContainer, BarChart, Bar, LineChart, Line, PieChart, Pie, Cell, XAxis, YAxis, CartesianGrid, Tooltip as RechartsTooltip, Legend } from 'recharts';
import dayjs from 'dayjs';

const { Title, Text } = Typography;
const { Option } = Select;
const { RangePicker } = DatePicker;

interface DashboardViewProps {
  data: any[];
  columns: ColumnInfo[];
  query: string;
  onConfigChange?: (config: DashboardConfig) => void;
}

export const DashboardView: React.FC<DashboardViewProps> = ({
  data,
  columns,
  query,
  onConfigChange
}) => {
  const [loading, setLoading] = useState(false);
  const [dashboardConfig, setDashboardConfig] = useState<DashboardConfig | null>(null);
  const [globalFilters, setGlobalFilters] = useState<Record<string, any>>({});
  const [refreshInterval, setRefreshInterval] = useState<number | null>(null);

  const generateDashboard = useCallback(async () => {
    setLoading(true);
    try {
      const request: VisualizationRequest = {
        query,
        columns,
        data: data.slice(0, 100) // Send sample for config generation
      };

      const response = await fetch(`${process.env.REACT_APP_API_URL}/api/visualization/dashboard`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Authorization': `Bearer ${localStorage.getItem('token')}`
        },
        body: JSON.stringify(request)
      });

      if (response.ok) {
        const config: DashboardConfig = await response.json();
        setDashboardConfig(config);
        setRefreshInterval(config.refreshInterval || null);
        onConfigChange?.(config);
      } else {
        console.error('Failed to generate dashboard config');
      }
    } catch (error) {
      console.error('Error generating dashboard:', error);
    } finally {
      setLoading(false);
    }
  }, [query, columns, data, onConfigChange]);

  useEffect(() => {
    if (data.length > 0 && columns.length > 0) {
      generateDashboard();
    }
  }, [data, columns, query, generateDashboard]);

  // Auto-refresh functionality
  useEffect(() => {
    if (refreshInterval && refreshInterval > 0) {
      const interval = setInterval(() => {
        generateDashboard();
      }, refreshInterval * 1000);

      return () => clearInterval(interval);
    }
  }, [refreshInterval, generateDashboard]);

  // Apply global filters to data
  const getFilteredData = () => {
    if (!dashboardConfig || Object.keys(globalFilters).length === 0) {
      return data;
    }

    return data.filter(row => {
      return Object.entries(globalFilters).every(([columnName, filterValue]) => {
        const filter = dashboardConfig.globalFilters.find(f => f.columnName === columnName);
        if (!filter || filterValue === undefined || filterValue === null) return true;

        const cellValue = row[columnName];

        switch (filter.filterType) {
          case 'dateRange':
            if (Array.isArray(filterValue) && filterValue.length === 2) {
              const cellDate = new Date(cellValue);
              return cellDate >= filterValue[0] && cellDate <= filterValue[1];
            }
            return true;
          case 'multiSelect':
            return Array.isArray(filterValue) ? filterValue.includes(cellValue) : cellValue === filterValue;
          default:
            return true;
        }
      });
    });
  };

  // Render individual chart
  const renderChart = (chartConfig: VisualizationConfig, index: number) => {
    const filteredData = getFilteredData();
    const chartData = filteredData.slice(0, 500); // Limit for performance

    const getChartSize = (size: string) => {
      switch (size) {
        case 'full': return 24;
        case 'half': return 12;
        case 'quarter': return 6;
        case 'third': return 8;
        default: return 12;
      }
    };

    const chartSize = dashboardConfig?.layout.chartSizes[index] || 'half';
    const colSpan = getChartSize(chartSize);

    const renderChartContent = () => {
      const commonProps = {
        width: '100%',
        height: 300,
        data: chartData
      };

      switch (chartConfig.type) {
        case 'bar':
          return (
            <ResponsiveContainer {...commonProps}>
              <BarChart data={chartData}>
                <CartesianGrid strokeDasharray="3 3" />
                <XAxis dataKey={chartConfig.xAxis} />
                <YAxis />
                <RechartsTooltip />
                <Legend />
                <Bar dataKey={chartConfig.yAxis || 'value'} fill="#8884d8" />
              </BarChart>
            </ResponsiveContainer>
          );

        case 'line':
          return (
            <ResponsiveContainer {...commonProps}>
              <LineChart data={chartData}>
                <CartesianGrid strokeDasharray="3 3" />
                <XAxis dataKey={chartConfig.xAxis} />
                <YAxis />
                <RechartsTooltip />
                <Legend />
                <Line type="monotone" dataKey={chartConfig.yAxis} stroke="#8884d8" />
              </LineChart>
            </ResponsiveContainer>
          );

        case 'pie':
          return (
            <ResponsiveContainer {...commonProps}>
              <PieChart>
                <Pie
                  data={chartData}
                  dataKey={chartConfig.yAxis || 'value'}
                  nameKey={chartConfig.xAxis || 'name'}
                  cx="50%"
                  cy="50%"
                  outerRadius={100}
                  fill="#8884d8"
                >
                  {chartData.map((entry, idx) => (
                    <Cell key={`cell-${idx}`} fill={`hsl(${idx * 45}, 70%, 60%)`} />
                  ))}
                </Pie>
                <RechartsTooltip />
                <Legend />
              </PieChart>
            </ResponsiveContainer>
          );

        default:
          return (
            <div style={{ padding: '40px', textAlign: 'center' }}>
              <Text>Chart type "{chartConfig.type}" not implemented</Text>
            </div>
          );
      }
    };

    return (
      <Col span={colSpan} key={index}>
        <Card
          size="small"
          title={chartConfig.title}
          extra={
            <Space>
              <Tooltip title="Chart Settings">
                <Button size="small" icon={<SettingOutlined />} />
              </Tooltip>
              <Tooltip title="Fullscreen">
                <Button size="small" icon={<FullscreenOutlined />} />
              </Tooltip>
            </Space>
          }
          style={{ height: '400px', marginBottom: '16px' }}
        >
          <div style={{ height: '320px' }}>
            {renderChartContent()}
          </div>
        </Card>
      </Col>
    );
  };

  // Render global filters
  const renderGlobalFilters = () => {
    if (!dashboardConfig || dashboardConfig.globalFilters.length === 0) return null;

    return (
      <Card size="small" title="Global Filters" style={{ marginBottom: '16px' }}>
        <Row gutter={16}>
          {dashboardConfig.globalFilters.map(filter => (
            <Col span={8} key={filter.columnName}>
              <div>
                <Text strong>{filter.label}</Text>
                {filter.filterType === 'dateRange' && (
                  <RangePicker
                    style={{ width: '100%', marginTop: '4px' }}
                    value={globalFilters[filter.columnName] || null}
                    onChange={(dates) => {
                      if (dates) {
                        setGlobalFilters(prev => ({
                          ...prev,
                          [filter.columnName]: [dates[0]?.toDate(), dates[1]?.toDate()]
                        }));
                      } else {
                        setGlobalFilters(prev => {
                          const newFilters = { ...prev };
                          delete newFilters[filter.columnName];
                          return newFilters;
                        });
                      }
                    }}
                  />
                )}
                {filter.filterType === 'multiSelect' && (
                  <Select
                    mode="multiple"
                    placeholder={`Select ${filter.label}`}
                    style={{ width: '100%', marginTop: '4px' }}
                    value={globalFilters[filter.columnName] || []}
                    onChange={(value) => setGlobalFilters(prev => ({ ...prev, [filter.columnName]: value }))}
                  >
                    {getUniqueValues(filter.columnName).map(option => (
                      <Option key={option} value={option}>{String(option)}</Option>
                    ))}
                  </Select>
                )}
              </div>
            </Col>
          ))}
        </Row>
      </Card>
    );
  };

  const getUniqueValues = (columnName: string): any[] => {
    const uniqueValues = [...new Set(data.map(row => row[columnName]))];
    return uniqueValues.filter(val => val !== null && val !== undefined);
  };

  const exportDashboard = () => {
    const filteredData = getFilteredData();
    const csv = convertToCSV(filteredData);
    const blob = new Blob([csv], { type: 'text/csv' });
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = `dashboard_data_${Date.now()}.csv`;
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
            <Text>Generating dashboard...</Text>
          </div>
        </div>
      </Card>
    );
  }

  if (!dashboardConfig) {
    return (
      <Card>
        <div style={{ textAlign: 'center', padding: '40px' }}>
          <Text>No dashboard configuration available</Text>
          <div style={{ marginTop: '16px' }}>
            <Button onClick={generateDashboard}>
              Generate Dashboard
            </Button>
          </div>
        </div>
      </Card>
    );
  }

  return (
    <div>
      {/* Dashboard Header */}
      <Card style={{ marginBottom: '16px' }}>
        <Row justify="space-between" align="middle">
          <Col>
            <Title level={3} style={{ margin: 0 }}>
              {dashboardConfig.title}
            </Title>
            <Text type="secondary">{dashboardConfig.description}</Text>
          </Col>
          <Col>
            <Space>
              <Select
                placeholder="Auto-refresh"
                style={{ width: 120 }}
                value={refreshInterval}
                onChange={setRefreshInterval}
                allowClear
              >
                <Option value={30}>30s</Option>
                <Option value={60}>1m</Option>
                <Option value={300}>5m</Option>
                <Option value={600}>10m</Option>
              </Select>
              <Button icon={<DownloadOutlined />} onClick={exportDashboard}>
                Export
              </Button>
              <Button icon={<ReloadOutlined />} onClick={generateDashboard}>
                Refresh
              </Button>
            </Space>
          </Col>
        </Row>
      </Card>

      {/* Global Filters */}
      {renderGlobalFilters()}

      {/* Dashboard Charts */}
      <Row gutter={16}>
        {dashboardConfig.charts.map((chart, index) => renderChart(chart, index))}
      </Row>

      {/* Dashboard Info */}
      <Card size="small" style={{ marginTop: '16px' }}>
        <Row justify="space-between">
          <Col>
            <Text type="secondary">
              Layout: {dashboardConfig.layout.rows} × {dashboardConfig.layout.columns} grid
            </Text>
          </Col>
          <Col>
            <Text type="secondary">
              Data: {getFilteredData().length} of {data.length} rows
              {Object.keys(globalFilters).length > 0 && ' (filtered)'}
            </Text>
          </Col>
          <Col>
            <Text type="secondary">
              Last updated: {dayjs().format('HH:mm:ss')}
            </Text>
          </Col>
        </Row>
      </Card>
    </div>
  );
};

export default DashboardView;
