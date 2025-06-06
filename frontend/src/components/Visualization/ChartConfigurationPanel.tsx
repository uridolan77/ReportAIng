import React, { useState, useEffect, useMemo, useCallback } from 'react';
import { Row, Col, Select, Button, Space, Typography, Checkbox, Card, Divider, Tag, Tooltip } from 'antd';
import {
  BarChartOutlined,
  LineChartOutlined,
  AreaChartOutlined,
  PieChartOutlined,
  DotChartOutlined,
  HeatMapOutlined,
  FunnelPlotOutlined,
  RadarChartOutlined,
  ThunderboltOutlined,
  SettingOutlined
} from '@ant-design/icons';
import { AdvancedVisualizationConfig, AdvancedChartType } from '../../types/visualization';

const { Text } = Typography;
const { Option } = Select;

interface ChartConfigurationPanelProps {
  data: any[];
  columns: any[];
  currentConfig?: AdvancedVisualizationConfig | null;
  onConfigChange: (config: AdvancedVisualizationConfig) => void;
}

// Gaming-specific chart presets
const GAMING_CHART_PRESETS = {
  gameRevenue: {
    title: 'Game Revenue Analysis',
    xAxis: 'GameName',
    metrics: ['TotalRevenue', 'NetGamingRevenue'],
    chartType: 'Bar' as AdvancedChartType,
    description: 'Revenue performance by game'
  },
  providerComparison: {
    title: 'Provider Performance',
    xAxis: 'Provider',
    metrics: ['TotalRevenue', 'TotalBets', 'TotalSessions'],
    chartType: 'Bar' as AdvancedChartType,
    description: 'Compare providers across key metrics'
  },
  gameTypeBreakdown: {
    title: 'Game Type Distribution',
    xAxis: 'GameType',
    metrics: ['TotalRevenue'],
    chartType: 'Pie' as AdvancedChartType,
    description: 'Revenue distribution by game type'
  },
  sessionAnalysis: {
    title: 'Gaming Sessions Analysis',
    xAxis: 'GameName',
    metrics: ['TotalSessions', 'TotalBets'],
    chartType: 'Line' as AdvancedChartType,
    description: 'Session and betting patterns'
  }
};

const ChartConfigurationPanel: React.FC<ChartConfigurationPanelProps> = ({
  data,
  columns,
  currentConfig,
  onConfigChange
}) => {
  const [chartType, setChartType] = useState<AdvancedChartType>('Bar');
  const [xAxis, setXAxis] = useState<string>('');
  const [selectedMetrics, setSelectedMetrics] = useState<string[]>([]);
  const [showAdvanced, setShowAdvanced] = useState(false);

  // Detect if this is gaming data
  const isGamingData = useMemo(() => {
    const columnNames = columns.map(col => typeof col === 'string' ? col : col.name || col);
    const hasGamingColumns = columnNames.some(col =>
      ['GameName', 'Provider', 'GameType', 'TotalRevenue', 'NetGamingRevenue', 'TotalSessions', 'TotalNetGamingRevenue'].includes(col)
    );

    console.log('ChartConfigurationPanel - Gaming detection:', {
      columns: columnNames,
      isGamingData: hasGamingColumns,
      dataLength: data.length
    });

    return hasGamingColumns;
  }, [columns, data.length]);

  // Get gaming-specific column suggestions
  const getGamingColumns = useMemo(() => {
    const columnNames = columns.map(col => typeof col === 'string' ? col : col.name || col);
    return {
      labelColumns: columnNames.filter(col => ['GameName', 'Provider', 'GameType'].includes(col)),
      metricColumns: columnNames.filter(col =>
        ['TotalRevenue', 'NetGamingRevenue', 'TotalBets', 'TotalSessions', 'RealBetAmount', 'RealWinAmount'].includes(col)
      )
    };
  }, [columns]);

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
      // Exclude meaningful IDs that should be treated as categorical
      const isMeaningfulId = key === 'PlayerID' || key === 'Alias' || key === 'Country' || key === 'Currency';
      return (typeof value === 'number' ||
             (typeof value === 'string' && !isNaN(parseFloat(value)))) &&
             key !== 'id' &&
             !isMeaningfulId &&
             !dateColumns.includes(key);
    });
    
    return { dateColumns, numericColumns };
  }, [data]);

  // Track if we've initialized to prevent overriding user changes
  const [isInitialized, setIsInitialized] = useState(false);

  // Initialize with detected values only once
  useEffect(() => {
    console.log('ChartConfigurationPanel - Initialization effect:', {
      isInitialized,
      currentConfig: currentConfig?.chartType,
      dateColumns: dateColumns.length,
      numericColumns: numericColumns.length
    });

    if (!isInitialized) {
      if (currentConfig) {
        console.log('ChartConfigurationPanel - Initializing from currentConfig:', currentConfig);
        setChartType(currentConfig.chartType);
        setXAxis(currentConfig.xAxis || '');
        setSelectedMetrics(Array.isArray(currentConfig.series) ? currentConfig.series : [currentConfig.yAxis || ''].filter(Boolean));
      } else {
        console.log('ChartConfigurationPanel - Auto-detecting initial values');
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
      setIsInitialized(true);
    }
  }, [currentConfig, dateColumns, numericColumns, isInitialized]);

  const chartTypeOptions = [
    {
      value: 'Bar',
      label: 'Bar Chart',
      icon: <BarChartOutlined />,
      description: 'Compare categories',
      category: 'Basic',
      color: '#1890ff'
    },
    {
      value: 'Line',
      label: 'Line Chart',
      icon: <LineChartOutlined />,
      description: 'Show trends over time',
      category: 'Basic',
      color: '#52c41a'
    },
    {
      value: 'Area',
      label: 'Area Chart',
      icon: <AreaChartOutlined />,
      description: 'Filled line chart',
      category: 'Basic',
      color: '#faad14'
    },
    {
      value: 'Pie',
      label: 'Pie Chart',
      icon: <PieChartOutlined />,
      description: 'Show proportions',
      category: 'Basic',
      color: '#f5222d'
    },
    {
      value: 'Scatter',
      label: 'Scatter Plot',
      icon: <DotChartOutlined />,
      description: 'Show correlations',
      category: 'Advanced',
      color: '#722ed1'
    },
    {
      value: 'Bubble',
      label: 'Bubble Chart',
      icon: <DotChartOutlined />,
      description: 'Multi-dimensional data',
      category: 'Advanced',
      color: '#13c2c2'
    },
    {
      value: 'Heatmap',
      label: 'Heatmap',
      icon: <HeatMapOutlined />,
      description: 'Intensity visualization',
      category: 'Advanced',
      color: '#eb2f96'
    },
    {
      value: 'Funnel',
      label: 'Funnel Chart',
      icon: <FunnelPlotOutlined />,
      description: 'Process flow',
      category: 'Specialized',
      color: '#fa8c16'
    },
    {
      value: 'Radar',
      label: 'Radar Chart',
      icon: <RadarChartOutlined />,
      description: 'Multi-variable comparison',
      category: 'Specialized',
      color: '#a0d911'
    }
  ];

  const generateConfig = useCallback((overrideMetrics?: string[]) => {
    const metricsToUse = Array.isArray(overrideMetrics) ? overrideMetrics : Array.isArray(selectedMetrics) ? selectedMetrics : [];

    console.log('🔍 ChartConfigurationPanel - generateConfig called:', {
      xAxis,
      selectedMetrics,
      overrideMetrics,
      metricsToUse,
      chartType,
      dataLength: data.length,
      firstDataRow: data[0],
      dataKeys: data.length > 0 ? Object.keys(data[0]) : []
    });

    if (!xAxis || metricsToUse.length === 0) {
      console.error('❌ ChartConfigurationPanel - Cannot generate config:', {
        missingXAxis: !xAxis,
        missingMetrics: metricsToUse.length === 0,
        xAxis,
        metricsToUse,
        selectedMetrics
      });
      return;
    }

    // Validate that the selected columns exist in the data
    if (data.length > 0) {
      const dataKeys = Object.keys(data[0]);
      const missingXAxis = !dataKeys.includes(xAxis);
      const missingMetrics = metricsToUse.filter(metric => !dataKeys.includes(metric));

      if (missingXAxis || missingMetrics.length > 0) {
        console.error('❌ ChartConfigurationPanel - Column validation failed:', {
          missingXAxis,
          missingMetrics,
          availableKeys: dataKeys,
          requestedXAxis: xAxis,
          requestedMetrics: metricsToUse
        });
      }
    }

    // Preserve existing configuration settings if available
    const existingConfig = currentConfig || {};

    const config: AdvancedVisualizationConfig = {
      type: 'advanced',
      chartType: chartType,
      title: `${chartType} Chart - ${metricsToUse.join(', ')} by ${xAxis}`,
      xAxis: xAxis,
      yAxis: metricsToUse[0], // Primary metric
      series: metricsToUse, // All selected metrics
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
          displayFields: [xAxis, ...metricsToUse],
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
        ariaLabels: [`${chartType} chart showing ${metricsToUse.join(', ')}`],
        colorBlindFriendly: true
      }
    };

    console.log('✅ ChartConfigurationPanel - Generated config:', {
      chartType: config.chartType,
      xAxis: config.xAxis,
      yAxis: config.yAxis,
      series: config.series,
      preservedSettings: config.customSettings,
      interaction: config.interaction,
      animation: config.animation,
      fullConfig: config
    });

    // Validate the generated config against actual data
    if (data.length > 0) {
      const dataKeys = Object.keys(data[0]);
      const configValidation = {
        xAxisExists: dataKeys.includes(config.xAxis || ''),
        yAxisExists: dataKeys.includes(config.yAxis || ''),
        seriesExist: config.series?.every(s => dataKeys.includes(s)) || false,
        availableKeys: dataKeys
      };

      console.log('🔍 ChartConfigurationPanel - Config validation:', configValidation);

      if (!configValidation.xAxisExists || !configValidation.yAxisExists) {
        console.error('❌ ChartConfigurationPanel - Generated config has invalid column references!');
      }
    }

    onConfigChange(config);
  }, [xAxis, selectedMetrics, chartType, data.length, onConfigChange]); // Use data.length instead of data object

  const handleMetricToggle = (metric: string, checked: boolean) => {
    console.log('ChartConfigurationPanel - Metric toggle:', { metric, checked, currentMetrics: selectedMetrics });

    let newMetrics: string[];
    const currentMetrics = Array.isArray(selectedMetrics) ? selectedMetrics : [];
    if (checked) {
      newMetrics = [...currentMetrics, metric];
    } else {
      newMetrics = currentMetrics.filter(m => m !== metric);
    }

    console.log('ChartConfigurationPanel - New metrics:', newMetrics);
    setSelectedMetrics(newMetrics);

    // Auto-generation will be handled by useEffect
  };

  // Handle chart type change
  const handleChartTypeChange = (newChartType: AdvancedChartType) => {
    setChartType(newChartType);

    // Auto-generation will be handled by useEffect
  };

  // Handle X-axis change
  const handleXAxisChange = (newXAxis: string) => {
    setXAxis(newXAxis);

    // Auto-generation will be handled by useEffect
  };

  // Auto-generate chart when all required fields are available
  useEffect(() => {
    const currentMetrics = Array.isArray(selectedMetrics) ? selectedMetrics : [];
    if (xAxis && currentMetrics.length > 0 && data.length > 0) {
      console.log('ChartConfigurationPanel - Auto-generating chart on dependency change');
      // Use a timeout to prevent infinite loops
      const timeoutId = setTimeout(() => {
        generateConfig();
      }, 10);
      return () => clearTimeout(timeoutId);
    }
  }, [xAxis, selectedMetrics, chartType, data.length]); // Remove generateConfig from dependencies

  // Auto-apply gaming configuration when gaming data is detected
  useEffect(() => {
    console.log('ChartConfigurationPanel - Gaming auto-config effect triggered:', {
      isGamingData,
      dataLength: data.length,
      isInitialized,
      shouldTrigger: isGamingData && data.length > 0 && !currentConfig
    });

    if (isGamingData && data.length > 0 && !currentConfig) {
      console.log('ChartConfigurationPanel - Applying gaming auto-configuration');

      const { labelColumns, metricColumns } = getGamingColumns;

      // Apply gaming-specific configuration immediately
      if (labelColumns.length > 0 && metricColumns.length > 0) {
        const bestLabelColumn = labelColumns.find(col => col === 'GameName') ||
                               labelColumns.find(col => col === 'Provider') ||
                               labelColumns[0];

        const bestMetrics = metricColumns.filter(col =>
          ['TotalRevenue', 'NetGamingRevenue', 'TotalNetGamingRevenue'].includes(col)
        ).slice(0, 2);

        if (bestMetrics.length === 0) {
          bestMetrics.push(...metricColumns.slice(0, 2));
        }

        const selectedChartType = bestLabelColumn === 'GameType' ? 'Pie' : 'Bar';

        setXAxis(bestLabelColumn);
        setSelectedMetrics(bestMetrics);
        setChartType(selectedChartType);

        // Immediately generate the chart configuration
        setTimeout(() => {
          const config: AdvancedVisualizationConfig = {
            type: 'advanced',
            chartType: selectedChartType,
            title: `Gaming ${selectedChartType} Chart - ${bestMetrics.join(', ')} by ${bestLabelColumn}`,
            xAxis: bestLabelColumn,
            yAxis: bestMetrics[0],
            series: bestMetrics,
            labelColumn: bestLabelColumn, // This ensures the chart uses the correct column for labels
            config: {
              showLegend: bestMetrics.length > 1,
              showTooltip: true,
              enableAnimation: true
            },
            animation: {
              enabled: true,
              duration: 1000,
              easing: 'ease-in-out',
              delayByCategory: false,
              delayIncrement: 100,
              animateOnDataChange: true,
              animatedProperties: ['opacity', 'transform']
            },
            interaction: {
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
                displayFields: [bestLabelColumn, ...bestMetrics],
                showStatistics: true,
                enableHtml: false
              }
            },
            theme: {
              name: 'gaming',
              darkMode: false,
              colors: {
                primary: ['#1890ff', '#52c41a', '#faad14', '#f5222d', '#722ed1', '#13c2c2'],
                secondary: ['#d9d9d9', '#bfbfbf'],
                background: '#ffffff',
                text: '#000000',
                grid: '#f0f0f0',
                axis: '#d9d9d9'
              }
            }
          };

          console.log('ChartConfigurationPanel - Auto-generating gaming chart:', config);
          onConfigChange(config);
          setIsInitialized(true);
        }, 100);

        console.log('ChartConfigurationPanel - Gaming config applied:', {
          xAxis: bestLabelColumn,
          metrics: bestMetrics,
          chartType: selectedChartType
        });
      }
    }
  }, [isGamingData, data.length, getGamingColumns, currentConfig, onConfigChange]);

  // Initialize with smart defaults when data is available
  useEffect(() => {
    console.log('🔍 ChartConfigurationPanel - Smart defaults effect triggered:', {
      dataLength: data.length,
      columnsLength: columns.length,
      currentXAxis: xAxis,
      currentMetricsLength: selectedMetrics.length,
      shouldInitialize: data.length > 0 && columns.length > 0 && !xAxis && selectedMetrics.length === 0
    });

    if (data.length > 0 && columns.length > 0 && !xAxis && selectedMetrics.length === 0) {
      console.log('ChartConfigurationPanel - Initializing with smart defaults');

      if (isGamingData) {
        // Gaming-specific auto-configuration
        const { labelColumns, metricColumns } = getGamingColumns;

        // Prefer GameName > Provider > GameType for X-axis
        const bestLabelColumn = labelColumns.find(col => col === 'GameName') ||
                               labelColumns.find(col => col === 'Provider') ||
                               labelColumns.find(col => col === 'GameType') ||
                               labelColumns[0];

        // Prefer revenue metrics for Y-axis
        const revenueMetrics = metricColumns.filter(col =>
          ['TotalRevenue', 'NetGamingRevenue'].includes(col)
        );
        const sessionMetrics = metricColumns.filter(col =>
          ['TotalSessions', 'TotalBets'].includes(col)
        );

        const bestMetrics = [...revenueMetrics.slice(0, 2), ...sessionMetrics.slice(0, 1)];

        if (bestLabelColumn) {
          setXAxis(bestLabelColumn);
        }
        if (bestMetrics.length > 0) {
          setSelectedMetrics(bestMetrics);
        }

        // Set appropriate chart type for gaming data
        if (bestLabelColumn === 'GameType') {
          setChartType('Pie'); // Game type distribution works well as pie chart
        } else {
          setChartType('Bar'); // Game/Provider comparisons work well as bar charts
        }

        console.log('ChartConfigurationPanel - Gaming auto-config applied:', {
          xAxis: bestLabelColumn,
          metrics: bestMetrics,
          chartType: bestLabelColumn === 'GameType' ? 'Pie' : 'Bar'
        });
      } else {
        // Standard auto-configuration for non-gaming data
        console.log('🔍 ChartConfigurationPanel - Applying standard auto-configuration');

        // Get all available columns
        const dataKeys = Object.keys(data[0]).filter(key => key !== 'id');

        // Find categorical columns (strings, including meaningful IDs like PlayerID)
        const categoricalColumns = dataKeys.filter(key => {
          const value = data[0][key];
          // Include PlayerID and other meaningful identifiers, but exclude generic 'id' fields
          const isMeaningfulId = key === 'PlayerID' || key === 'Alias' || key === 'Country' || key === 'Currency';
          const isGenericId = key.toLowerCase() === 'id' || key.toLowerCase().endsWith('_id');
          return typeof value === 'string' && (isMeaningfulId || !isGenericId);
        });

        // Find numeric columns
        const availableNumericColumns = dataKeys.filter(key => {
          const value = data[0][key];
          return typeof value === 'number' || (!isNaN(Number(value)) && value !== null && value !== '');
        });

        console.log('🔍 ChartConfigurationPanel - Column detection:', {
          dataKeys,
          categoricalColumns,
          availableNumericColumns,
          dateColumns
        });

        // Smart X-axis selection: prefer meaningful categorical columns over dates
        let selectedXAxis = null;

        // Priority 1: Look for meaningful categorical columns first
        const meaningfulColumns = categoricalColumns.filter(col =>
          ['Country', 'CountryName', 'Currency', 'Alias', 'PlayerID', 'Provider', 'GameName', 'GameType'].includes(col)
        );

        if (meaningfulColumns.length > 0) {
          // Prefer Country > Currency > Alias > PlayerID > others
          selectedXAxis = meaningfulColumns.find(col => col === 'Country') ||
                         meaningfulColumns.find(col => col === 'CountryName') ||
                         meaningfulColumns.find(col => col === 'Currency') ||
                         meaningfulColumns.find(col => col === 'Alias') ||
                         meaningfulColumns.find(col => col === 'PlayerID') ||
                         meaningfulColumns[0];
        }
        // Priority 2: Date columns for time series
        else if (dateColumns.length > 0) {
          selectedXAxis = dateColumns[0];
        }
        // Priority 3: Any categorical column
        else if (categoricalColumns.length > 0) {
          selectedXAxis = categoricalColumns[0];
        }
        // Priority 4: Fallback to first column
        else if (dataKeys.length > 0) {
          selectedXAxis = dataKeys[0];
        }

        if (selectedXAxis) {
          console.log('✅ ChartConfigurationPanel - Setting X-axis to:', selectedXAxis);
          setXAxis(selectedXAxis);
        }

        const defaultMetrics = availableNumericColumns.slice(0, 3);
        if (defaultMetrics.length > 0) {
          console.log('✅ ChartConfigurationPanel - Setting metrics to:', defaultMetrics);
          setSelectedMetrics(defaultMetrics);
        }

        // Set appropriate chart type based on selected X-axis
        if (selectedXAxis === 'Country' || selectedXAxis === 'CountryName') {
          setChartType('Bar'); // Country data works better as bar chart for comparison
        } else if (selectedXAxis === 'Currency' || selectedXAxis === 'Alias') {
          setChartType('Pie'); // Currency/Alias distribution works well as pie
        } else if (dateColumns.includes(selectedXAxis)) {
          setChartType('Line'); // Time series data works well as line chart
        } else {
          setChartType('Bar'); // Default to bar chart
        }
      }
    }
  }, [data, columns, dateColumns, numericColumns, xAxis, selectedMetrics.length, isGamingData, getGamingColumns]);

  // Force auto-detection when data changes (more aggressive approach)
  useEffect(() => {
    if (data.length > 0 && (!xAxis || selectedMetrics.length === 0)) {
      console.log('🔄 ChartConfigurationPanel - Force auto-detection triggered');

      const dataKeys = Object.keys(data[0]).filter(key => key !== 'id');

      // Find categorical columns (strings that aren't IDs)
      const categoricalColumns = dataKeys.filter(key => {
        const value = data[0][key];
        return typeof value === 'string' && !key.toLowerCase().includes('id');
      });

      // Find numeric columns
      const availableNumericColumns = dataKeys.filter(key => {
        const value = data[0][key];
        return typeof value === 'number' || (!isNaN(Number(value)) && value !== null && value !== '');
      });

      console.log('🔄 Force detection results:', {
        dataKeys,
        categoricalColumns,
        availableNumericColumns,
        currentXAxis: xAxis,
        currentMetrics: selectedMetrics
      });

      // Set X-axis if not already set
      if (!xAxis && (categoricalColumns.length > 0 || dataKeys.length > 0)) {
        const selectedXAxis = categoricalColumns[0] || dataKeys[0];
        console.log('🔄 Setting X-axis to:', selectedXAxis);
        setXAxis(selectedXAxis);
      }

      // Set metrics if not already set
      if (selectedMetrics.length === 0 && availableNumericColumns.length > 0) {
        const defaultMetrics = availableNumericColumns.slice(0, 2);
        console.log('🔄 Setting metrics to:', defaultMetrics);
        setSelectedMetrics(defaultMetrics);
      }

      // Set appropriate chart type based on data
      if (categoricalColumns.includes('CountryName') || categoricalColumns.includes('Country')) {
        setChartType('Pie');
      } else if (availableNumericColumns.length > 1) {
        setChartType('Bar');
      }
    }
  }, [data, xAxis, selectedMetrics]);

  // Manual fix function
  const handleFixChart = () => {
    if (data.length === 0) return;

    console.log('🔧 Manual chart fix triggered');

    const dataKeys = Object.keys(data[0]).filter(key => key !== 'id');

    // Find categorical and numeric columns
    const categoricalColumns = dataKeys.filter(key => {
      const value = data[0][key];
      return typeof value === 'string' && !key.toLowerCase().includes('id');
    });

    const availableNumericColumns = dataKeys.filter(key => {
      const value = data[0][key];
      return typeof value === 'number' || (!isNaN(Number(value)) && value !== null && value !== '');
    });

    console.log('🔧 Fix chart - detected columns:', {
      categoricalColumns,
      availableNumericColumns,
      firstDataRow: data[0]
    });

    // Apply the fix
    if (categoricalColumns.length > 0) {
      setXAxis(categoricalColumns[0]);
    } else if (dataKeys.length > 0) {
      setXAxis(dataKeys[0]);
    }

    if (availableNumericColumns.length > 0) {
      setSelectedMetrics(availableNumericColumns.slice(0, 2));
    }

    // Set chart type based on data
    if (categoricalColumns.includes('CountryName')) {
      setChartType('Pie');
    } else {
      setChartType('Bar');
    }

    console.log('🔧 Chart fix applied:', {
      xAxis: categoricalColumns[0] || dataKeys[0],
      metrics: availableNumericColumns.slice(0, 2),
      chartType: categoricalColumns.includes('CountryName') ? 'Pie' : 'Bar'
    });
  };

  return (
    <div>
      {/* Emergency Fix Button */}
      {data.length > 0 && (!xAxis || selectedMetrics.length === 0) && (
        <div style={{
          marginBottom: '16px',
          padding: '12px',
          background: '#fff7e6',
          border: '1px solid #ffd591',
          borderRadius: '8px'
        }}>
          <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between' }}>
            <div>
              <Text strong style={{ color: '#d46b08' }}>Chart Configuration Issue</Text>
              <div style={{ fontSize: '12px', color: '#ad6800', marginTop: '4px' }}>
                Chart columns don't match your data. Click to auto-fix.
              </div>
            </div>
            <Button
              type="primary"
              size="small"
              onClick={handleFixChart}
              style={{ background: '#fa8c16', borderColor: '#fa8c16' }}
            >
              Fix Chart
            </Button>
          </div>
        </div>
      )}

      {/* Enhanced Chart Type Selection */}
      <div style={{ marginBottom: '20px' }}>
        <div style={{
          display: 'flex',
          alignItems: 'center',
          gap: '8px',
          marginBottom: '12px'
        }}>
          <ThunderboltOutlined style={{ color: '#667eea' }} />
          <Text strong style={{ color: '#1f2937' }}>Chart Type</Text>
          <Tag color="blue" style={{ fontSize: '10px' }}>
            {chartTypeOptions.find(opt => opt.value === chartType)?.category || 'Basic'}
          </Tag>
        </div>

        {/* Chart Type Grid */}
        <div style={{
          display: 'grid',
          gridTemplateColumns: 'repeat(auto-fit, minmax(120px, 1fr))',
          gap: '8px',
          marginBottom: '12px'
        }}>
          {chartTypeOptions.map(option => (
            <Tooltip
              key={option.value}
              title={option.description}
              placement="top"
            >
              <div
                onClick={() => handleChartTypeChange(option.value as AdvancedChartType)}
                style={{
                  padding: '12px 8px',
                  border: chartType === option.value ? `2px solid ${option.color}` : '1px solid #e5e7eb',
                  borderRadius: '8px',
                  background: chartType === option.value ? `${option.color}10` : 'white',
                  cursor: 'pointer',
                  textAlign: 'center',
                  transition: 'all 0.2s ease',
                  display: 'flex',
                  flexDirection: 'column',
                  alignItems: 'center',
                  gap: '4px',
                  minHeight: '70px',
                  justifyContent: 'center'
                }}
                onMouseEnter={(e) => {
                  if (chartType !== option.value) {
                    e.currentTarget.style.borderColor = option.color;
                    e.currentTarget.style.background = `${option.color}05`;
                  }
                }}
                onMouseLeave={(e) => {
                  if (chartType !== option.value) {
                    e.currentTarget.style.borderColor = '#e5e7eb';
                    e.currentTarget.style.background = 'white';
                  }
                }}
              >
                <div style={{
                  fontSize: '18px',
                  color: chartType === option.value ? option.color : '#6b7280'
                }}>
                  {option.icon}
                </div>
                <Text style={{
                  fontSize: '11px',
                  fontWeight: chartType === option.value ? 600 : 400,
                  color: chartType === option.value ? option.color : '#6b7280',
                  textAlign: 'center',
                  lineHeight: '1.2'
                }}>
                  {option.label.replace(' Chart', '')}
                </Text>
              </div>
            </Tooltip>
          ))}
        </div>
      </div>

      <Row gutter={[20, 20]}>
        {/* X-Axis Selection */}
        <Col xs={24} sm={12}>
          <div style={{
            background: '#f8fafc',
            padding: '16px',
            borderRadius: '12px',
            border: '1px solid #e2e8f0'
          }}>
            <div style={{
              display: 'flex',
              alignItems: 'center',
              gap: '8px',
              marginBottom: '12px'
            }}>
              <Text strong style={{ color: '#1f2937', fontSize: '14px' }}>X-Axis (Categories)</Text>
              <Tag color="blue" style={{ fontSize: '10px' }}>
                {xAxis ? 'SELECTED' : 'CHOOSE'}
              </Tag>
            </div>
            <Select
              value={xAxis}
              onChange={handleXAxisChange}
              style={{
                width: '100%',
                minHeight: '40px'
              }}
              size="middle"
              placeholder="Select X-axis column"
              dropdownStyle={{
                borderRadius: '8px',
                boxShadow: '0 4px 12px rgba(0,0,0,0.15)'
              }}
            >
              {/* Gaming data: show label columns first */}
              {isGamingData && getGamingColumns.labelColumns.map(col => (
                <Option key={col} value={col}>
                  <Space>
                    <Tag color="purple" style={{ fontSize: '10px', fontWeight: 500, borderRadius: '4px' }}>GAMING</Tag>
                    <Text style={{ fontSize: '13px', fontWeight: 500 }}>{col}</Text>
                  </Space>
                </Option>
              ))}

              {/* Date columns */}
              {dateColumns.map(col => (
                <Option key={col} value={col}>
                  <Space>
                    <Tag color="blue" style={{ fontSize: '10px', fontWeight: 500, borderRadius: '4px' }}>DATE</Tag>
                    <Text style={{ fontSize: '13px', fontWeight: 500 }}>{col}</Text>
                  </Space>
                </Option>
              ))}

              {/* Other categorical columns for non-gaming data */}
              {!isGamingData && (() => {
                // Get all available columns from actual data
                const dataKeys = data.length > 0 ? Object.keys(data[0]).filter(key => key !== 'id') : [];

                // Find categorical columns (strings, including meaningful IDs like PlayerID)
                const categoricalColumns = dataKeys.filter(key => {
                  if (dateColumns.includes(key) || numericColumns.includes(key)) return false;
                  const value = data[0][key];
                  // Include PlayerID and other meaningful identifiers, but exclude generic 'id' fields
                  const isMeaningfulId = key === 'PlayerID' || key === 'Alias' || key === 'Country' || key === 'Currency';
                  const isGenericId = key.toLowerCase() === 'id' || key.toLowerCase().endsWith('_id');
                  return typeof value === 'string' && (isMeaningfulId || !isGenericId);
                });

                console.log('🔍 X-axis dropdown - Available columns:', {
                  dataKeys,
                  dateColumns,
                  numericColumns,
                  categoricalColumns,
                  firstDataRow: data[0]
                });

                return categoricalColumns.map(col => (
                  <Option key={col} value={col}>
                    <Space>
                      <Tag color="green" style={{ fontSize: '10px', fontWeight: 500, borderRadius: '4px' }}>TEXT</Tag>
                      <Text style={{ fontSize: '13px', fontWeight: 500 }}>{col}</Text>
                    </Space>
                  </Option>
                ));
              })()}

              {/* Show message if no suitable columns */}
              {dateColumns.length === 0 && (!isGamingData || getGamingColumns.labelColumns.length === 0) && (
                <Option value="" disabled>
                  <Text type="secondary">No suitable X-axis columns found</Text>
                </Option>
              )}
            </Select>
          </div>
        </Col>

        {/* Metrics Selection */}
        <Col xs={24} sm={12}>
          <div style={{
            background: '#f8fafc',
            padding: '16px',
            borderRadius: '12px',
            border: '1px solid #e2e8f0'
          }}>
            <div style={{
              display: 'flex',
              alignItems: 'center',
              gap: '8px',
              marginBottom: '12px'
            }}>
              <Text strong style={{ color: '#1f2937', fontSize: '14px' }}>
                Metrics
              </Text>
              <Tag color="green" style={{ fontSize: '10px' }}>
                {Array.isArray(selectedMetrics) ? selectedMetrics.length : 0} SELECTED
              </Tag>
            </div>
            <div style={{
              maxHeight: '200px',
              overflowY: 'auto',
              border: '1px solid #e2e8f0',
              borderRadius: '8px',
              padding: '12px',
              backgroundColor: '#ffffff',
              boxShadow: '0 1px 3px rgba(0,0,0,0.1)'
            }}>
              {numericColumns.map(metric => (
                <div key={metric} style={{
                  marginBottom: '10px',
                  padding: '10px 12px',
                  borderRadius: '8px',
                  background: Array.isArray(selectedMetrics) && selectedMetrics.includes(metric) ? '#e0f2fe' : '#f8fafc',
                  border: Array.isArray(selectedMetrics) && selectedMetrics.includes(metric) ? '2px solid #0891b2' : '1px solid #e2e8f0',
                  transition: 'all 0.2s ease',
                  cursor: 'pointer',
                  boxShadow: Array.isArray(selectedMetrics) && selectedMetrics.includes(metric) ? '0 2px 4px rgba(8, 145, 178, 0.1)' : 'none'
                }}>
                  <Checkbox
                    checked={Array.isArray(selectedMetrics) ? selectedMetrics.includes(metric) : false}
                    onChange={(e) => handleMetricToggle(metric, e.target.checked)}
                    style={{ width: '100%' }}
                  >
                    <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', width: '100%' }}>
                      <Text style={{
                        fontSize: '13px',
                        fontWeight: Array.isArray(selectedMetrics) && selectedMetrics.includes(metric) ? 600 : 500,
                        color: Array.isArray(selectedMetrics) && selectedMetrics.includes(metric) ? '#0f172a' : '#475569'
                      }}>
                        {metric}
                      </Text>
                      <Tag
                        color={Array.isArray(selectedMetrics) && selectedMetrics.includes(metric) ? 'cyan' : 'default'}
                        style={{
                          fontSize: '10px',
                          margin: 0,
                          fontWeight: 500,
                          borderRadius: '4px'
                        }}
                      >
                        NUM
                      </Tag>
                    </div>
                  </Checkbox>
                </div>
              ))}
              {numericColumns.length === 0 && (
                <div style={{
                  textAlign: 'center',
                  padding: '24px',
                  background: '#fef3f2',
                  borderRadius: '6px',
                  border: '1px dashed #fca5a5'
                }}>
                  <Text type="secondary" style={{ fontSize: '13px', color: '#dc2626' }}>
                    No numeric columns found
                  </Text>
                </div>
              )}
            </div>
          </div>
        </Col>
      </Row>

      {/* Enhanced Quick Presets */}
      <Divider style={{ margin: '20px 0' }} />
      <div>
        <div style={{
          display: 'flex',
          alignItems: 'center',
          justifyContent: 'space-between',
          marginBottom: '12px'
        }}>
          <div style={{ display: 'flex', alignItems: 'center', gap: '8px' }}>
            <SettingOutlined style={{ color: '#667eea' }} />
            <Text strong style={{ color: '#1f2937' }}>Quick Presets</Text>
            <Tag color="purple" style={{ fontSize: '10px' }}>
              Smart Templates
            </Tag>
          </div>
          <Button
            size="small"
            type="primary"
            ghost
            onClick={() => {
              // Force re-detection
              setXAxis('');
              setSelectedMetrics([]);
              setIsInitialized(false);

              // Trigger auto-detection after a brief delay
              setTimeout(() => {
                if (data.length > 0) {
                  const dataKeys = Object.keys(data[0]).filter(key => key !== 'id');
                  const categoricalColumns = dataKeys.filter(key => {
                    const value = data[0][key];
                    // Include PlayerID and other meaningful identifiers, but exclude generic 'id' fields
                    const isMeaningfulId = key === 'PlayerID' || key === 'Alias' || key === 'Country' || key === 'Currency';
                    const isGenericId = key.toLowerCase() === 'id' || key.toLowerCase().endsWith('_id');
                    return typeof value === 'string' && (isMeaningfulId || !isGenericId);
                  });
                  const availableNumericColumns = dataKeys.filter(key => {
                    const value = data[0][key];
                    return typeof value === 'number' || (!isNaN(Number(value)) && value !== null && value !== '');
                  });

                  const selectedXAxis = dateColumns[0] || categoricalColumns[0] || dataKeys[0];
                  const defaultMetrics = availableNumericColumns.slice(0, 3);

                  if (selectedXAxis) setXAxis(selectedXAxis);
                  if (defaultMetrics.length > 0) setSelectedMetrics(defaultMetrics);

                  console.log('🔄 Manual auto-detection applied:', { selectedXAxis, defaultMetrics });
                }
              }, 100);
            }}
            style={{ fontSize: '11px' }}
          >
            Auto-Detect
          </Button>
        </div>

        <div style={{
          display: 'grid',
          gridTemplateColumns: 'repeat(auto-fit, minmax(140px, 1fr))',
          gap: '8px'
        }}>
          {isGamingData ? (
            // Gaming-specific presets
            <>
              {Object.entries(GAMING_CHART_PRESETS).map(([key, preset]) => (
                <Tooltip key={key} title={preset.description}>
                  <Button
                    size="small"
                    onClick={() => {
                      setChartType(preset.chartType);
                      setXAxis(preset.xAxis);
                      setSelectedMetrics(preset.metrics);
                    }}
                    style={{
                      height: 'auto',
                      padding: '8px',
                      display: 'flex',
                      flexDirection: 'column',
                      alignItems: 'center',
                      gap: '4px',
                      textAlign: 'center'
                    }}
                  >
                    <Text style={{ fontSize: '10px', fontWeight: 600 }}>
                      {preset.title}
                    </Text>
                    <Text style={{ fontSize: '9px', color: '#6b7280' }}>
                      {preset.chartType} • {preset.metrics.length} metrics
                    </Text>
                  </Button>
                </Tooltip>
              ))}
            </>
          ) : (
            // Standard presets for non-gaming data
            <>
              <Tooltip title="Single metric bar chart for revenue analysis">
                <Button
                  size="small"
                  onClick={() => {
                    setChartType('Bar');
                    setSelectedMetrics(['NetRevenue']);
                  }}
              disabled={!numericColumns.includes('NetRevenue')}
              style={{
                height: '40px',
                borderRadius: '8px',
                border: '1px solid #e5e7eb',
                background: 'white',
                display: 'flex',
                alignItems: 'center',
                justifyContent: 'center',
                gap: '6px'
              }}
            >
              <BarChartOutlined style={{ color: '#1890ff' }} />
              <span style={{ fontSize: '11px' }}>Revenue Bar</span>
            </Button>
          </Tooltip>

          <Tooltip title="Time series line chart for trend analysis">
            <Button
              size="small"
              onClick={() => {
                setChartType('Line');
                setSelectedMetrics(['NetRevenue']);
                // Auto-generate will trigger via useEffect
              }}
              disabled={!numericColumns.includes('NetRevenue')}
              style={{
                height: '40px',
                borderRadius: '8px',
                border: '1px solid #e5e7eb',
                background: 'white',
                display: 'flex',
                alignItems: 'center',
                justifyContent: 'center',
                gap: '6px'
              }}
            >
              <LineChartOutlined style={{ color: '#52c41a' }} />
              <span style={{ fontSize: '11px' }}>Revenue Trend</span>
            </Button>
          </Tooltip>

          <Tooltip title="Multi-metric comparison with all KPIs">
            <Button
              size="small"
              onClick={() => {
                setChartType('Line');
                setSelectedMetrics(['TotalDeposits', 'TotalPaidCashouts', 'NetRevenue']);
                // Auto-generate will trigger via useEffect
              }}
              disabled={!['TotalDeposits', 'TotalPaidCashouts', 'NetRevenue'].every(m => numericColumns.includes(m))}
              style={{
                height: '40px',
                borderRadius: '8px',
                border: '1px solid #e5e7eb',
                background: 'white',
                display: 'flex',
                alignItems: 'center',
                justifyContent: 'center',
                gap: '6px'
              }}
            >
              <LineChartOutlined style={{ color: '#faad14' }} />
              <span style={{ fontSize: '11px' }}>All KPIs</span>
            </Button>
          </Tooltip>

          <Tooltip title="Area chart comparing deposits and cashouts">
            <Button
              size="small"
              onClick={() => {
                setChartType('Area');
                setSelectedMetrics(['TotalDeposits', 'TotalPaidCashouts']);
                // Auto-generate will trigger via useEffect
              }}
              disabled={!['TotalDeposits', 'TotalPaidCashouts'].every(m => numericColumns.includes(m))}
              style={{
                height: '40px',
                borderRadius: '8px',
                border: '1px solid #e5e7eb',
                background: 'white',
                display: 'flex',
                alignItems: 'center',
                justifyContent: 'center',
                gap: '6px'
              }}
            >
              <AreaChartOutlined style={{ color: '#f5222d' }} />
              <span style={{ fontSize: '11px' }}>Deposits vs Cashouts</span>
            </Button>
          </Tooltip>
            </>
          )}
        </div>

        {/* Auto-generation status */}
        <div style={{
          marginTop: '12px',
          textAlign: 'center',
          padding: '8px',
          background: '#f0f9ff',
          borderRadius: '6px',
          border: '1px solid #bae6fd'
        }}>
          <Text style={{
            fontSize: '11px',
            color: '#0369a1',
            fontWeight: 500
          }}>
            <ThunderboltOutlined style={{ marginRight: '4px' }} />
            Chart updates automatically
          </Text>
        </div>
      </div>
    </div>
  );
};

export default ChartConfigurationPanel;
