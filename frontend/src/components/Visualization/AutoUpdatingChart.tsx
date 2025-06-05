import React, { useEffect, useState, useMemo } from 'react';
import { Card, Typography, Space, Tag, Button, Tooltip } from 'antd';
import {
  ReloadOutlined,
  InfoCircleOutlined,
  BarChartOutlined,
  LineChartOutlined,
  PieChartOutlined
} from '@ant-design/icons';
import AdvancedChart from './AdvancedChart';
import ChartConfigurationPanel from './ChartConfigurationPanel';
import { useGamingChartProcessor, processGamingChartData } from './GamingChartProcessor';
import { AdvancedVisualizationConfig } from '../../types/visualization';

const { Text, Title } = Typography;

interface AutoUpdatingChartProps {
  data: any[];
  columns: string[];
  queryResult?: any;
  onConfigChange?: (config: AdvancedVisualizationConfig) => void;
  title?: string;
  showConfiguration?: boolean;
}

/**
 * Auto-updating chart component that:
 * 1. Automatically detects gaming data
 * 2. Applies optimal chart configurations
 * 3. Updates immediately when data changes
 * 4. Provides gaming-specific visualizations
 */
export const AutoUpdatingChart: React.FC<AutoUpdatingChartProps> = ({
  data,
  columns,
  queryResult,
  onConfigChange,
  title = "Data Visualization",
  showConfiguration = true
}) => {
  const [currentConfig, setCurrentConfig] = useState<AdvancedVisualizationConfig | null>(null);
  const [isConfiguring, setIsConfiguring] = useState(false);
  const [lastUpdateTime, setLastUpdateTime] = useState<Date>(new Date());

  // Process gaming data
  const gamingData = useGamingChartProcessor(data, columns);
  const processedGamingData = useMemo(() => processGamingChartData(gamingData), [gamingData]);

  // Auto-update when data changes
  useEffect(() => {
    if (data.length > 0 && columns.length > 0) {
      console.log('AutoUpdatingChart - Data changed, updating chart:', {
        dataLength: data.length,
        columnsLength: columns.length,
        isGamingData: gamingData.isGamingData,
        recommendations: processedGamingData.recommendations.length
      });

      // Apply gaming-specific configuration if available
      if (gamingData.isGamingData && processedGamingData.config) {
        const enhancedConfig: AdvancedVisualizationConfig = {
          type: 'advanced',
          chartType: processedGamingData.config.chartType || 'Bar',
          title: processedGamingData.config.title || title,
          xAxis: processedGamingData.config.xAxis || columns[0],
          yAxis: processedGamingData.config.yAxis || columns[1],
          series: processedGamingData.config.series || [columns[1]],
          config: {
            showLegend: true,
            showTooltip: true,
            enableAnimation: true,
            ...processedGamingData.config.config
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
              displayFields: [processedGamingData.config.xAxis || columns[0], ...(processedGamingData.config.series || [columns[1]])],
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

        setCurrentConfig(enhancedConfig);
        setLastUpdateTime(new Date());

        if (onConfigChange) {
          onConfigChange(enhancedConfig);
        }
      }
    }
  }, [data, columns, gamingData.isGamingData, processedGamingData, title, onConfigChange]);

  const handleConfigChange = (newConfig: AdvancedVisualizationConfig) => {
    console.log('AutoUpdatingChart - Configuration changed:', newConfig);
    setCurrentConfig(newConfig);
    setLastUpdateTime(new Date());
    
    if (onConfigChange) {
      onConfigChange(newConfig);
    }
  };

  const handleRefresh = () => {
    setLastUpdateTime(new Date());
    // Force re-processing of gaming data
    if (gamingData.isGamingData && processedGamingData.config) {
      handleConfigChange(currentConfig!);
    }
  };

  const getChartTypeIcon = (chartType: string) => {
    switch (chartType) {
      case 'Bar': return <BarChartOutlined />;
      case 'Line': return <LineChartOutlined />;
      case 'Pie': return <PieChartOutlined />;
      default: return <BarChartOutlined />;
    }
  };

  if (!data || data.length === 0) {
    return (
      <Card
        style={{
          borderRadius: '12px',
          border: '2px dashed #d1d5db',
          background: 'white',
          minHeight: '400px',
          display: 'flex',
          alignItems: 'center',
          justifyContent: 'center'
        }}
        bodyStyle={{
          padding: '40px',
          textAlign: 'center',
          display: 'flex',
          flexDirection: 'column',
          alignItems: 'center',
          justifyContent: 'center',
          height: '100%'
        }}
      >
        <BarChartOutlined style={{ fontSize: '48px', color: '#d1d5db', marginBottom: '16px' }} />
        <Title level={4} style={{ color: '#6b7280', margin: 0 }}>
          No Data Available
        </Title>
        <Text style={{ color: '#9ca3af', marginTop: '8px' }}>
          Execute a query to see visualizations
        </Text>
      </Card>
    );
  }

  return (
    <div style={{ display: 'flex', flexDirection: 'column', gap: '16px' }}>
      {/* Chart Header */}
      <div style={{
        display: 'flex',
        justifyContent: 'space-between',
        alignItems: 'center',
        padding: '0 4px'
      }}>
        <div style={{ display: 'flex', alignItems: 'center', gap: '12px' }}>
          <Title level={4} style={{ margin: 0, color: '#1f2937' }}>
            {currentConfig?.title || title}
          </Title>
          {gamingData.isGamingData && (
            <Tag color="blue" style={{ fontSize: '11px' }}>
              Gaming Data
            </Tag>
          )}
          {currentConfig && (
            <Tag color="green" style={{ fontSize: '11px', display: 'flex', alignItems: 'center', gap: '4px' }}>
              {getChartTypeIcon(currentConfig.chartType)}
              {currentConfig.chartType}
            </Tag>
          )}
        </div>
        
        <Space>
          <Tooltip title="Refresh chart">
            <Button
              size="small"
              icon={<ReloadOutlined />}
              onClick={handleRefresh}
              style={{ borderRadius: '6px' }}
            />
          </Tooltip>
          <Tooltip title={`Last updated: ${lastUpdateTime.toLocaleTimeString()}`}>
            <Button
              size="small"
              icon={<InfoCircleOutlined />}
              style={{ borderRadius: '6px' }}
            />
          </Tooltip>
          {showConfiguration && (
            <Button
              size="small"
              type={isConfiguring ? 'primary' : 'default'}
              onClick={() => setIsConfiguring(!isConfiguring)}
              style={{ borderRadius: '6px' }}
            >
              Configure
            </Button>
          )}
        </Space>
      </div>

      {/* Configuration Panel */}
      {showConfiguration && isConfiguring && (
        <Card
          size="small"
          style={{
            borderRadius: '8px',
            border: '1px solid #e5e7eb',
            background: '#f9fafb'
          }}
          bodyStyle={{ padding: '16px' }}
        >
          <ChartConfigurationPanel
            data={data}
            columns={columns}
            currentConfig={currentConfig}
            onConfigChange={handleConfigChange}
          />
        </Card>
      )}

      {/* Chart Display */}
      {currentConfig && (
        <Card
          style={{
            borderRadius: '12px',
            border: '1px solid #e5e7eb',
            background: 'white'
          }}
          bodyStyle={{ padding: '20px' }}
        >
          <AdvancedChart
            config={currentConfig}
            data={data}
            onConfigChange={handleConfigChange}
            onExport={(format) => {
              console.log('Export requested:', format);
            }}
          />
        </Card>
      )}

      {/* Gaming Recommendations */}
      {gamingData.isGamingData && processedGamingData.recommendations.length > 0 && (
        <Card
          size="small"
          title={
            <Space>
              <InfoCircleOutlined style={{ color: '#1890ff' }} />
              <Text strong>Gaming Analytics Suggestions</Text>
            </Space>
          }
          style={{
            borderRadius: '8px',
            border: '1px solid #bae6fd',
            background: '#f0f9ff'
          }}
          bodyStyle={{ padding: '12px' }}
        >
          <div style={{ display: 'flex', flexWrap: 'wrap', gap: '8px' }}>
            {processedGamingData.recommendations.slice(0, 3).map((rec, index) => (
              <Button
                key={index}
                size="small"
                onClick={() => {
                  const newConfig: AdvancedVisualizationConfig = {
                    ...currentConfig!,
                    chartType: rec.type,
                    title: rec.title,
                    xAxis: rec.xAxis,
                    yAxis: rec.metrics[0],
                    series: rec.metrics
                  };
                  handleConfigChange(newConfig);
                }}
                style={{
                  borderRadius: '6px',
                  fontSize: '11px',
                  height: 'auto',
                  padding: '4px 8px'
                }}
              >
                {getChartTypeIcon(rec.type)} {rec.title}
              </Button>
            ))}
          </div>
        </Card>
      )}
    </div>
  );
};

export default AutoUpdatingChart;
