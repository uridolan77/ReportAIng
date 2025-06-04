import React, { useState, useEffect, useMemo } from 'react';
import { Row, Col, Select, Button, Space, Typography, Checkbox, Card, Divider } from 'antd';
import { BarChartOutlined, LineChartOutlined, AreaChartOutlined, PieChartOutlined } from '@ant-design/icons';
import { AdvancedVisualizationConfig, AdvancedChartType } from '../../types/visualization';

const { Text } = Typography;
const { Option } = Select;

interface ChartConfigurationPanelProps {
  data: any[];
  columns: any[];
  currentConfig?: AdvancedVisualizationConfig | null;
  onConfigChange: (config: AdvancedVisualizationConfig) => void;
}

const ChartConfigurationPanel: React.FC<ChartConfigurationPanelProps> = ({
  data,
  columns,
  currentConfig,
  onConfigChange
}) => {
  const [chartType, setChartType] = useState<AdvancedChartType>('Bar');
  const [xAxis, setXAxis] = useState<string>('');
  const [selectedMetrics, setSelectedMetrics] = useState<string[]>([]);

  // Detect available columns
  const { dateColumns, numericColumns } = useMemo(() => {
    const dataKeys = data.length > 0 ? Object.keys(data[0]).filter(key => key !== 'id') : [];
    
    const dateColumns = dataKeys.filter(key => {
      if (data.length === 0) return false;
      const value = data[0][key];
      return (typeof value === 'string' && (
        value.includes('T') || 
        (value.includes('-') && value.length >= 10) ||
        key.toLowerCase().includes('date')
      )) || key.toLowerCase() === 'date';
    });
    
    const numericColumns = dataKeys.filter(key => {
      if (data.length === 0) return false;
      const value = data[0][key];
      return (typeof value === 'number' || 
             (typeof value === 'string' && !isNaN(parseFloat(value)))) &&
             key !== 'id' && 
             !dateColumns.includes(key);
    });
    
    return { dateColumns, numericColumns };
  }, [data]);

  // Initialize with detected values
  useEffect(() => {
    if (currentConfig) {
      setChartType(currentConfig.chartType);
      setXAxis(currentConfig.xAxis || '');
      setSelectedMetrics(currentConfig.series || [currentConfig.yAxis || '']);
    } else {
      // Auto-detect initial values
      if (dateColumns.length > 0) {
        setXAxis(dateColumns[0]);
      }
      if (numericColumns.length > 0) {
        // Default to NetRevenue if available, otherwise first numeric column
        const defaultMetric = numericColumns.find(col => 
          col.toLowerCase().includes('revenue') || 
          col.toLowerCase().includes('netrevenue')
        ) || numericColumns[0];
        setSelectedMetrics([defaultMetric]);
      }
    }
  }, [currentConfig, dateColumns, numericColumns]);

  const chartTypeOptions = [
    { value: 'Bar', label: 'Bar Chart', icon: <BarChartOutlined /> },
    { value: 'Line', label: 'Line Chart', icon: <LineChartOutlined /> },
    { value: 'Area', label: 'Area Chart', icon: <AreaChartOutlined /> },
    { value: 'Pie', label: 'Pie Chart', icon: <PieChartOutlined /> }
  ];

  const generateConfig = () => {
    if (!xAxis || selectedMetrics.length === 0) return;

    // Preserve existing configuration settings if available
    const existingConfig = currentConfig || {};

    const config: AdvancedVisualizationConfig = {
      type: 'advanced',
      chartType: chartType,
      title: `${chartType} Chart - ${selectedMetrics.join(', ')} by ${xAxis}`,
      xAxis: xAxis,
      yAxis: selectedMetrics[0], // Primary metric
      series: selectedMetrics, // All selected metrics
      config: existingConfig.config || {},

      // Preserve existing animation settings or use defaults
      animation: existingConfig.animation || {
        enabled: true,
        duration: 1000,
        easing: 'ease-in-out',
        delayByCategory: false,
        delayIncrement: 100,
        animateOnDataChange: true,
        animatedProperties: ['opacity', 'transform']
      },

      // Preserve existing interaction settings or use defaults
      interaction: existingConfig.interaction || {
        enableZoom: true,
        enablePan: true,
        enableBrush: false,
        enableCrosshair: true,
        enableTooltip: true,
        enableLegendToggle: true,
        enableDataPointSelection: true,
        enableDrillDown: false,
        tooltip: {
          enabled: true,
          position: 'auto',
          displayFields: [xAxis, ...selectedMetrics],
          showStatistics: true,
          enableHtml: false
        }
      },

      // Preserve existing theme settings or use defaults
      theme: existingConfig.theme || {
        name: 'default',
        darkMode: false,
        colors: {
          primary: ['#1890ff', '#52c41a', '#faad14', '#f5222d', '#722ed1', '#13c2c2'],
          secondary: ['#d9d9d9', '#bfbfbf'],
          background: '#ffffff',
          text: '#000000',
          grid: '#f0f0f0',
          axis: '#d9d9d9'
        }
      },

      // Preserve existing performance settings or use defaults
      performance: existingConfig.performance || {
        enableVirtualization: data.length > 10000,
        virtualizationThreshold: 10000,
        enableLazyLoading: true,
        enableCaching: true,
        cacheTTL: 300,
        enableWebGL: data.length > 50000,
        maxDataPoints: 100000
      },

      // Preserve existing accessibility settings or use defaults
      accessibility: existingConfig.accessibility || {
        enabled: true,
        highContrast: false,
        screenReaderSupport: true,
        keyboardNavigation: true,
        ariaLabels: [`${chartType} chart showing ${selectedMetrics.join(', ')}`],
        colorBlindFriendly: true
      }
    };

    console.log('ChartConfigurationPanel - Generated config:', {
      chartType: config.chartType,
      preservedSettings: config.customSettings,
      interaction: config.interaction,
      animation: config.animation
    });
    onConfigChange(config);
  };

  const handleMetricToggle = (metric: string, checked: boolean) => {
    if (checked) {
      setSelectedMetrics(prev => [...prev, metric]);
    } else {
      setSelectedMetrics(prev => prev.filter(m => m !== metric));
    }
  };

  return (
    <div>
      <Row gutter={[16, 16]}>
        {/* Chart Type Selection */}
        <Col xs={24} sm={12} md={6}>
          <div>
            <Text strong style={{ display: 'block', marginBottom: 8 }}>Chart Type</Text>
            <Select
              value={chartType}
              onChange={setChartType}
              style={{ width: '100%' }}
              size="small"
            >
              {chartTypeOptions.map(option => (
                <Option key={option.value} value={option.value}>
                  <Space>
                    {option.icon}
                    {option.label}
                  </Space>
                </Option>
              ))}
            </Select>
          </div>
        </Col>

        {/* X-Axis Selection */}
        <Col xs={24} sm={12} md={6}>
          <div>
            <Text strong style={{ display: 'block', marginBottom: 8 }}>X-Axis</Text>
            <Select
              value={xAxis}
              onChange={setXAxis}
              style={{ width: '100%' }}
              size="small"
              placeholder="Select X-axis"
            >
              {dateColumns.map(col => (
                <Option key={col} value={col}>{col}</Option>
              ))}
              {dateColumns.length === 0 && (
                <Option value="" disabled>No date columns found</Option>
              )}
            </Select>
          </div>
        </Col>

        {/* Metrics Selection */}
        <Col xs={24} sm={24} md={8}>
          <div>
            <Text strong style={{ display: 'block', marginBottom: 8 }}>
              Metrics to Display ({selectedMetrics.length} selected)
            </Text>
            <div style={{ 
              maxHeight: '120px', 
              overflowY: 'auto', 
              border: '1px solid #d9d9d9', 
              borderRadius: '6px', 
              padding: '8px',
              background: '#fafafa'
            }}>
              {numericColumns.map(metric => (
                <div key={metric} style={{ marginBottom: 4 }}>
                  <Checkbox
                    checked={selectedMetrics.includes(metric)}
                    onChange={(e) => handleMetricToggle(metric, e.target.checked)}
                  >
                    <Text style={{ fontSize: '12px' }}>{metric}</Text>
                  </Checkbox>
                </div>
              ))}
              {numericColumns.length === 0 && (
                <Text type="secondary" style={{ fontSize: '12px' }}>
                  No numeric columns found
                </Text>
              )}
            </div>
          </div>
        </Col>

        {/* Apply Button */}
        <Col xs={24} sm={24} md={4}>
          <div style={{ display: 'flex', alignItems: 'end', height: '100%' }}>
            <Button
              type="primary"
              onClick={generateConfig}
              disabled={!xAxis || selectedMetrics.length === 0}
              style={{ width: '100%' }}
              size="small"
            >
              Apply Chart
            </Button>
          </div>
        </Col>
      </Row>

      {/* Quick Presets */}
      <Divider style={{ margin: '12px 0' }} />
      <div>
        <Text strong style={{ marginRight: 16 }}>Quick Presets:</Text>
        <Space wrap>
          <Button
            size="small"
            onClick={() => {
              setChartType('Bar');
              setSelectedMetrics(['NetRevenue']);
              setTimeout(generateConfig, 100);
            }}
            disabled={!numericColumns.includes('NetRevenue')}
          >
            Revenue Bar
          </Button>
          <Button
            size="small"
            onClick={() => {
              setChartType('Line');
              setSelectedMetrics(['NetRevenue']);
              setTimeout(generateConfig, 100);
            }}
            disabled={!numericColumns.includes('NetRevenue')}
          >
            Revenue Trend
          </Button>
          <Button
            size="small"
            onClick={() => {
              setChartType('Line');
              setSelectedMetrics(['TotalDeposits', 'TotalPaidCashouts', 'NetRevenue']);
              setTimeout(generateConfig, 100);
            }}
            disabled={!['TotalDeposits', 'TotalPaidCashouts', 'NetRevenue'].every(m => numericColumns.includes(m))}
          >
            All KPIs
          </Button>
          <Button
            size="small"
            onClick={() => {
              setChartType('Area');
              setSelectedMetrics(['TotalDeposits', 'TotalPaidCashouts']);
              setTimeout(generateConfig, 100);
            }}
            disabled={!['TotalDeposits', 'TotalPaidCashouts'].every(m => numericColumns.includes(m))}
          >
            Deposits vs Cashouts
          </Button>
        </Space>
      </div>
    </div>
  );
};

export default ChartConfigurationPanel;
