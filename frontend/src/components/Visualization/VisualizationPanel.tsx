/**
 * Unified Visualization Panel Component
 * Consolidates visualization functionality from multiple sources
 */

import React, { useState, useEffect } from 'react';
import {
  Card,
  Row,
  Col,
  Typography,
  Button,
  Space,
  Select,
  Switch,
  Tabs,
  Alert,
  Spin
} from 'antd';
import {
  BarChartOutlined,
  LineChartOutlined,
  PieChartOutlined,
  DotChartOutlined,
  AreaChartOutlined,
  HeatMapOutlined,
  NodeIndexOutlined,
  AppstoreOutlined,
  SettingOutlined,
  FullscreenOutlined
} from '@ant-design/icons';
import { Chart } from './Chart';
import ChartConfigurationPanel from './ChartConfigurationPanel';
import VisualizationRecommendations from './VisualizationRecommendations';

const { Title, Text } = Typography;
const { Option } = Select;
const { TabPane } = Tabs;

interface VisualizationConfig {
  type: string;
  title: string;
  xAxis?: string;
  yAxis?: string;
  colorScheme: string;
  showGrid: boolean;
  showLegend: boolean;
  showAnimation: boolean;
  interactive: boolean;
}

interface VisualizationPanelProps {
  data: any[];
  columns: string[];
  query?: string;
  onConfigChange?: (config: VisualizationConfig) => void;
  autoGenerate?: boolean;
  showRecommendations?: boolean;
}

export const VisualizationPanel: React.FC<VisualizationPanelProps> = ({
  data,
  columns,
  query,
  onConfigChange,
  autoGenerate = true,
  showRecommendations = true
}) => {
  const [config, setConfig] = useState<VisualizationConfig>({
    type: 'bar',
    title: 'Data Visualization',
    xAxis: columns[0] || 'name',
    yAxis: columns[1] || 'value',
    colorScheme: 'default',
    showGrid: true,
    showLegend: true,
    showAnimation: true,
    interactive: true
  });

  const [loading, setLoading] = useState(false);
  const [fullscreen, setFullscreen] = useState(false);
  const [activeTab, setActiveTab] = useState('chart');
  const [recommendations, setRecommendations] = useState<any[]>([]);

  useEffect(() => {
    if (autoGenerate && data.length > 0 && columns.length > 0) {
      generateRecommendations();
    }
  }, [data, columns, autoGenerate]);

  const generateRecommendations = async () => {
    setLoading(true);
    try {
      // Real AI recommendations - call actual AI service
      // TODO: Replace with actual AI service API calls
      // const response = await aiVisualizationApi.getRecommendations(data, columns);
      // setRecommendations(response.recommendations);

      console.log('Loading real AI visualization recommendations...');

      // For now, show empty state until real AI service is connected
      setRecommendations([]);

    } catch (error) {
      console.error('Failed to generate recommendations:', error);
    } finally {
      setLoading(false);
    }
  };

  const handleConfigChange = (newConfig: Partial<VisualizationConfig>) => {
    const updatedConfig = { ...config, ...newConfig };
    setConfig(updatedConfig);
    onConfigChange?.(updatedConfig);
  };

  const chartTypes = [
    { value: 'bar', label: 'Bar Chart', icon: <BarChartOutlined /> },
    { value: 'line', label: 'Line Chart', icon: <LineChartOutlined /> },
    { value: 'area', label: 'Area Chart', icon: <AreaChartOutlined /> },
    { value: 'pie', label: 'Pie Chart', icon: <PieChartOutlined /> },
    { value: 'scatter', label: 'Scatter Plot', icon: <DotChartOutlined /> },
    { value: 'heatmap', label: 'Heatmap', icon: <HeatMapOutlined /> },
    { value: 'network', label: 'Network', icon: <NodeIndexOutlined /> },
    { value: 'treemap', label: 'Treemap', icon: <AppstoreOutlined /> }
  ];

  const renderChartTypeSelector = () => (
    <Card size="small" title="Chart Type" style={{ marginBottom: 16 }}>
      <Row gutter={[8, 8]}>
        {chartTypes.map(type => (
          <Col span={6} key={type.value}>
            <Button
              type={config.type === type.value ? 'primary' : 'default'}
              icon={type.icon}
              onClick={() => handleConfigChange({ type: type.value })}
              style={{ width: '100%', height: 60 }}
            >
              <div style={{ marginTop: 4, fontSize: '12px' }}>
                {type.label}
              </div>
            </Button>
          </Col>
        ))}
      </Row>
    </Card>
  );

  const renderQuickSettings = () => (
    <Card size="small" title="Quick Settings" style={{ marginBottom: 16 }}>
      <Row gutter={[16, 16]}>
        <Col span={12}>
          <Text strong>X-Axis</Text>
          <Select
            value={config.xAxis}
            onChange={(value) => handleConfigChange({ xAxis: value })}
            style={{ width: '100%', marginTop: 4 }}
          >
            {columns.map(col => (
              <Option key={col} value={col}>{col}</Option>
            ))}
          </Select>
        </Col>
        <Col span={12}>
          <Text strong>Y-Axis</Text>
          <Select
            value={config.yAxis}
            onChange={(value) => handleConfigChange({ yAxis: value })}
            style={{ width: '100%', marginTop: 4 }}
          >
            {columns.map(col => (
              <Option key={col} value={col}>{col}</Option>
            ))}
          </Select>
        </Col>
        <Col span={24}>
          <Space wrap>
            <Switch
              checked={config.showGrid}
              onChange={(checked) => handleConfigChange({ showGrid: checked })}
              checkedChildren="Grid"
              unCheckedChildren="Grid"
            />
            <Switch
              checked={config.showLegend}
              onChange={(checked) => handleConfigChange({ showLegend: checked })}
              checkedChildren="Legend"
              unCheckedChildren="Legend"
            />
            <Switch
              checked={config.showAnimation}
              onChange={(checked) => handleConfigChange({ showAnimation: checked })}
              checkedChildren="Animation"
              unCheckedChildren="Animation"
            />
            <Switch
              checked={config.interactive}
              onChange={(checked) => handleConfigChange({ interactive: checked })}
              checkedChildren="Interactive"
              unCheckedChildren="Interactive"
            />
          </Space>
        </Col>
      </Row>
    </Card>
  );

  const renderRecommendationsTab = () => (
    <div>
      {loading ? (
        <div style={{ textAlign: 'center', padding: 40 }}>
          <Spin size="large" />
          <div style={{ marginTop: 16 }}>
            <Text>Analyzing data for visualization recommendations...</Text>
          </div>
        </div>
      ) : (
        <VisualizationRecommendations
          data={data}
          columns={columns}
          query={query || ''}
          onRecommendationSelect={(rec) => {
            // Handle recommendation selection
            console.log('Recommendation selected:', rec);
          }}
          onConfigGenerated={(config) => {
            handleConfigChange(config);
            setActiveTab('chart');
          }}
        />
      )}
    </div>
  );

  const renderConfigurationTab = () => (
    <ChartConfigurationPanel
      data={data}
      columns={columns}
      currentConfig={config as any}
      onConfigChange={handleConfigChange}
    />
  );

  const renderChartTab = () => (
    <div>
      {renderChartTypeSelector()}
      {renderQuickSettings()}
      
      <Chart
        data={data}
        columns={columns}
        config={config as any}
        onConfigChange={handleConfigChange}
        height={fullscreen ? 600 : 400}
        loading={loading}
      />
    </div>
  );

  if (!data || data.length === 0) {
    return (
      <Alert
        message="No Data Available"
        description="Please execute a query to generate visualizations."
        type="info"
        showIcon
      />
    );
  }

  return (
    <div style={{ padding: 16 }}>
      <div style={{ marginBottom: 16, display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
        <Title level={3} style={{ margin: 0 }}>
          Data Visualization
        </Title>
        <Space>
          <Button
            icon={<SettingOutlined />}
            onClick={() => setActiveTab('configuration')}
          >
            Advanced Settings
          </Button>
          <Button
            icon={<FullscreenOutlined />}
            onClick={() => setFullscreen(!fullscreen)}
          >
            {fullscreen ? 'Exit Fullscreen' : 'Fullscreen'}
          </Button>
        </Space>
      </div>

      <Tabs activeKey={activeTab} onChange={setActiveTab}>
        <TabPane tab="Chart" key="chart">
          {renderChartTab()}
        </TabPane>
        
        {showRecommendations && (
          <TabPane tab="AI Recommendations" key="recommendations">
            {renderRecommendationsTab()}
          </TabPane>
        )}
        
        <TabPane tab="Configuration" key="configuration">
          {renderConfigurationTab()}
        </TabPane>
      </Tabs>
    </div>
  );
};
