import React, { useState, useEffect, useCallback, useMemo } from 'react';
import {
  Card,
  Row,
  Col,
  Button,
  Space,
  Typography,
  Spin,
  Alert,
  Select,
  Switch,
  Slider,
  Tooltip,
  Badge,
  Divider,
  Modal,
  message,
  Tabs
} from 'antd';
import {
  BarChartOutlined,
  DashboardOutlined,
  BulbOutlined,
  SettingOutlined,
  DownloadOutlined,
  ShareAltOutlined,
  ReloadOutlined,
  ThunderboltOutlined,
  EyeOutlined
} from '@ant-design/icons';
import {
  AdvancedVisualizationConfig,
  AdvancedDashboardConfig,
  VisualizationRecommendation,
  VisualizationPreferences,
  DashboardPreferences,
  ChartPerformanceMetrics
} from '../../types/visualization';
import AdvancedChart from './AdvancedChart';
import AdvancedDashboard from './AdvancedDashboard';
import VisualizationRecommendations from './VisualizationRecommendations';
import AdvancedDashboardBuilder from './AdvancedDashboardBuilder';
import advancedVisualizationService from '../../services/advancedVisualizationService';
import './AdvancedVisualization.css';

const { Title, Text } = Typography;
const { Option } = Select;
const { TabPane } = Tabs;

interface AdvancedVisualizationPanelProps {
  data: any[];
  columns: any[];
  query: string;
  onConfigChange?: (config: AdvancedVisualizationConfig | AdvancedDashboardConfig) => void;
  onExport?: (format: string, config: any) => void;
}

const AdvancedVisualizationPanel: React.FC<AdvancedVisualizationPanelProps> = ({
  data,
  columns,
  query,
  onConfigChange,
  onExport
}) => {
  // State management
  const [activeTab, setActiveTab] = useState('single');
  const [loading, setLoading] = useState(false);
  const [currentVisualization, setCurrentVisualization] = useState<AdvancedVisualizationConfig | null>(null);
  const [currentDashboard, setCurrentDashboard] = useState<AdvancedDashboardConfig | null>(null);
  const [recommendations, setRecommendations] = useState<VisualizationRecommendation[]>([]);
  const [showSettings, setShowSettings] = useState(false);
  const [performanceMetrics, setPerformanceMetrics] = useState<ChartPerformanceMetrics | null>(null);

  // User preferences
  const [visualizationPreferences, setVisualizationPreferences] = useState<VisualizationPreferences>({
    enableAnimations: true,
    enableInteractivity: true,
    performance: 'Balanced',
    accessibility: 'Standard'
  });

  const [dashboardPreferences, setDashboardPreferences] = useState<DashboardPreferences>({
    enableRealTime: false,
    enableCollaboration: false,
    enableAnalytics: true,
    layout: 'Auto',
    preferredChartTypes: ['Bar', 'Line', 'Pie']
  });

  // Generate single advanced visualization
  const generateVisualization = useCallback(async () => {
    if (!data.length || !query) return;

    setLoading(true);
    try {
      const response = await advancedVisualizationService.generateAdvancedVisualization({
        query,
        preferences: visualizationPreferences,
        optimizeForPerformance: visualizationPreferences.performance === 'HighPerformance'
      });

      if (response.success && response.visualization) {
        setCurrentVisualization(response.visualization);
        setPerformanceMetrics(response.performanceMetrics || null);
        onConfigChange?.(response.visualization);
        message.success('Advanced visualization generated successfully!');
      } else {
        message.error(response.errorMessage || 'Failed to generate visualization');
      }
    } catch (error) {
      console.error('Error generating visualization:', error);
      message.error('Failed to generate visualization');
    } finally {
      setLoading(false);
    }
  }, [data, query, visualizationPreferences, onConfigChange]);

  // Generate advanced dashboard
  const generateDashboard = useCallback(async () => {
    if (!data.length || !query) return;

    setLoading(true);
    try {
      const response = await advancedVisualizationService.generateAdvancedDashboard({
        query,
        preferences: dashboardPreferences
      });

      if (response.success && response.dashboard) {
        setCurrentDashboard(response.dashboard);
        onConfigChange?.(response.dashboard);
        message.success('Advanced dashboard generated successfully!');
      } else {
        message.error(response.errorMessage || 'Failed to generate dashboard');
      }
    } catch (error) {
      console.error('Error generating dashboard:', error);
      message.error('Failed to generate dashboard');
    } finally {
      setLoading(false);
    }
  }, [data, query, dashboardPreferences, onConfigChange]);

  // Get AI-powered recommendations
  const getRecommendations = useCallback(async () => {
    if (!data.length || !query) return;

    setLoading(true);
    try {
      const response = await advancedVisualizationService.getVisualizationRecommendations({
        query,
        context: `Data has ${data.length} rows and ${columns.length} columns`
      });

      if (response.success && response.recommendations) {
        setRecommendations(response.recommendations);
        message.success(`Found ${response.recommendations.length} visualization recommendations!`);
      } else {
        message.error(response.errorMessage || 'Failed to get recommendations');
      }
    } catch (error) {
      console.error('Error getting recommendations:', error);
      message.error('Failed to get recommendations');
    } finally {
      setLoading(false);
    }
  }, [data, query, columns]);

  // Auto-generate on data change
  useEffect(() => {
    if (data.length > 0 && query && activeTab === 'single') {
      generateVisualization();
    }
  }, [data, query, activeTab, generateVisualization]);

  // Auto-get recommendations on data change
  useEffect(() => {
    if (data.length > 0 && query && activeTab === 'recommendations') {
      getRecommendations();
    }
  }, [data, query, activeTab, getRecommendations]);

  const handleExport = useCallback(async (format: string) => {
    if (!currentVisualization && !currentDashboard) return;

    try {
      const config = currentVisualization || currentDashboard;
      const blob = await advancedVisualizationService.exportVisualization(
        config as AdvancedVisualizationConfig,
        format as any,
        data
      );

      const filename = advancedVisualizationService.generateExportFilename(
        config as AdvancedVisualizationConfig,
        format
      );

      advancedVisualizationService.downloadFile(blob, filename, format);
      message.success(`Exported as ${format.toUpperCase()}`);
      onExport?.(format, config);
    } catch (error) {
      console.error('Export failed:', error);
      message.error('Export failed');
    }
  }, [currentVisualization, currentDashboard, data, onExport]);

  const handleRecommendationSelect = useCallback((recommendation: VisualizationRecommendation) => {
    setRecommendations([recommendation, ...recommendations.filter(r => r.chartType !== recommendation.chartType)]);
  }, [recommendations]);

  const handleConfigFromRecommendation = useCallback((config: AdvancedVisualizationConfig) => {
    setCurrentVisualization(config);
    setActiveTab('single');
    onConfigChange?.(config);
  }, [onConfigChange]);

  const renderSettings = () => (
    <Modal
      title="Advanced Visualization Settings"
      open={showSettings}
      onCancel={() => setShowSettings(false)}
      footer={null}
      width={600}
    >
      <Tabs defaultActiveKey="visualization">
        <TabPane tab="Visualization" key="visualization">
          <Space direction="vertical" style={{ width: '100%' }}>
            <div>
              <Text strong>Performance Mode</Text>
              <Select
                style={{ width: '100%', marginTop: 8 }}
                value={visualizationPreferences.performance}
                onChange={(value) => setVisualizationPreferences(prev => ({ ...prev, performance: value }))}
              >
                <Option value="HighQuality">High Quality</Option>
                <Option value="Balanced">Balanced</Option>
                <Option value="HighPerformance">High Performance</Option>
              </Select>
            </div>

            <div>
              <Text strong>Accessibility Level</Text>
              <Select
                style={{ width: '100%', marginTop: 8 }}
                value={visualizationPreferences.accessibility}
                onChange={(value) => setVisualizationPreferences(prev => ({ ...prev, accessibility: value }))}
              >
                <Option value="Basic">Basic</Option>
                <Option value="Standard">Standard</Option>
                <Option value="Enhanced">Enhanced</Option>
              </Select>
            </div>

            <div>
              <Text strong>Enable Animations</Text>
              <br />
              <Switch
                checked={visualizationPreferences.enableAnimations}
                onChange={(checked) => setVisualizationPreferences(prev => ({ ...prev, enableAnimations: checked }))}
              />
            </div>

            <div>
              <Text strong>Enable Interactivity</Text>
              <br />
              <Switch
                checked={visualizationPreferences.enableInteractivity}
                onChange={(checked) => setVisualizationPreferences(prev => ({ ...prev, enableInteractivity: checked }))}
              />
            </div>
          </Space>
        </TabPane>

        <TabPane tab="Dashboard" key="dashboard">
          <Space direction="vertical" style={{ width: '100%' }}>
            <div>
              <Text strong>Layout Type</Text>
              <Select
                style={{ width: '100%', marginTop: 8 }}
                value={dashboardPreferences.layout}
                onChange={(value) => setDashboardPreferences(prev => ({ ...prev, layout: value }))}
              >
                <Option value="Auto">Auto</Option>
                <Option value="Grid">Grid</Option>
                <Option value="Masonry">Masonry</Option>
                <Option value="Responsive">Responsive</Option>
              </Select>
            </div>

            <div>
              <Text strong>Enable Real-time Updates</Text>
              <br />
              <Switch
                checked={dashboardPreferences.enableRealTime}
                onChange={(checked) => setDashboardPreferences(prev => ({ ...prev, enableRealTime: checked }))}
              />
            </div>

            <div>
              <Text strong>Enable Collaboration</Text>
              <br />
              <Switch
                checked={dashboardPreferences.enableCollaboration}
                onChange={(checked) => setDashboardPreferences(prev => ({ ...prev, enableCollaboration: checked }))}
              />
            </div>

            <div>
              <Text strong>Enable Analytics</Text>
              <br />
              <Switch
                checked={dashboardPreferences.enableAnalytics}
                onChange={(checked) => setDashboardPreferences(prev => ({ ...prev, enableAnalytics: checked }))}
              />
            </div>
          </Space>
        </TabPane>
      </Tabs>
    </Modal>
  );

  return (
    <div className="advanced-visualization-panel">
      <Card
        title={
          <Space>
            <ThunderboltOutlined />
            <Title level={4} style={{ margin: 0 }}>Advanced Visualizations</Title>
            <Badge count={data.length} style={{ backgroundColor: '#52c41a' }} />
          </Space>
        }
        extra={
          <Space>
            <Tooltip title="Settings">
              <Button icon={<SettingOutlined />} onClick={() => setShowSettings(true)} />
            </Tooltip>
            <Tooltip title="Export">
              <Button
                icon={<DownloadOutlined />}
                onClick={() => handleExport('png')}
                disabled={!currentVisualization && !currentDashboard}
              />
            </Tooltip>
            <Tooltip title="Refresh">
              <Button
                icon={<ReloadOutlined />}
                onClick={activeTab === 'single' ? generateVisualization : generateDashboard}
                loading={loading}
              />
            </Tooltip>
          </Space>
        }
      >
        <Tabs activeKey={activeTab} onChange={setActiveTab}>
          <TabPane
            tab={
              <span>
                <BarChartOutlined />
                Single Chart
              </span>
            }
            key="single"
          >
            <Spin spinning={loading}>
              {currentVisualization ? (
                <AdvancedChart
                  config={currentVisualization}
                  data={data}
                  loading={loading}
                  onConfigChange={(config) => {
                    setCurrentVisualization(config);
                    onConfigChange?.(config);
                  }}
                  onExport={handleExport}
                  performanceMetrics={performanceMetrics || undefined}
                />
              ) : (
                <div style={{ textAlign: 'center', padding: 40 }}>
                  <EyeOutlined style={{ fontSize: 48, color: '#ccc' }} />
                  <br />
                  <Text type="secondary">
                    {data.length > 0 ? 'Generating advanced visualization...' : 'Execute a query to generate visualizations'}
                  </Text>
                </div>
              )}
            </Spin>
          </TabPane>

          <TabPane
            tab={
              <span>
                <DashboardOutlined />
                Dashboard Builder
              </span>
            }
            key="dashboard"
          >
            <AdvancedDashboardBuilder
              data={data}
              columns={columns}
              query={query}
              onDashboardGenerated={(dashboard) => {
                setCurrentDashboard(dashboard);
                onConfigChange?.(dashboard);
              }}
              onSave={(dashboard) => {
                console.log('Dashboard saved:', dashboard);
                message.success('Dashboard saved successfully!');
              }}
            />
          </TabPane>

          <TabPane
            tab={
              <span>
                <BulbOutlined />
                AI Recommendations ({recommendations.length})
              </span>
            }
            key="recommendations"
          >
            <VisualizationRecommendations
              data={data}
              columns={columns}
              query={query}
              onRecommendationSelect={handleRecommendationSelect}
              onConfigGenerated={handleConfigFromRecommendation}
            />
          </TabPane>
        </Tabs>
      </Card>

      {renderSettings()}
    </div>
  );
};

export default AdvancedVisualizationPanel;
