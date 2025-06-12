/**
 * Modern Visualization Page
 * 
 * Consolidated visualization interface that merges Interactive Visualizations 
 * and AI-Powered Charts into a single, comprehensive visualization experience.
 */

import React, { useState, useCallback } from 'react';
import {
  Card,
  Button,
  Tabs,
  Container,
  Stack,
  Flex,
  Alert,
  Badge,
  Grid
} from '../components/core';
import { ModernPageLayout } from '../components/core/Layouts';
import { Breadcrumb } from '../components/core/Navigation';
import { HomeOutlined, BarChartOutlined } from '@ant-design/icons';
import { InteractiveVisualization } from '../components/Visualization/InteractiveVisualization';
import { Chart } from '../components/Visualization/Chart';
import ChartConfigurationPanel from '../components/Visualization/ChartConfigurationPanel';
import VisualizationRecommendations from '../components/Visualization/VisualizationRecommendations';
import { useCurrentResult } from '../hooks/useCurrentResult';
import { useVisualizationStore } from '../stores/visualizationStore';

const VisualizationPage: React.FC = () => {
  const [activeTab, setActiveTab] = useState('interactive');
  const [showConfig, setShowConfig] = useState(false);
  const { hasResult, currentResult } = useCurrentResult();
  const { charts, addChart, removeChart } = useVisualizationStore();

  const handleTabChange = useCallback((key: string) => {
    setActiveTab(key);
  }, []);

  const handleCreateChart = useCallback((chartConfig: any) => {
    addChart({
      id: Date.now().toString(),
      type: chartConfig.type,
      data: currentResult?.data || [],
      config: chartConfig,
      createdAt: new Date().toISOString(),
    });
  }, [addChart, currentResult]);

  const chartTypes = [
    { type: 'bar', label: 'Bar Chart', icon: '📊', description: 'Compare categories' },
    { type: 'line', label: 'Line Chart', icon: '📈', description: 'Show trends over time' },
    { type: 'pie', label: 'Pie Chart', icon: '🥧', description: 'Show proportions' },
    { type: 'area', label: 'Area Chart', icon: '📉', description: 'Filled line charts' },
    { type: 'scatter', label: 'Scatter Plot', icon: '⚪', description: 'Show correlations' },
    { type: 'heatmap', label: 'Heatmap', icon: '🔥', description: 'Show data density' },
  ];

  const tabItems = [
    {
      key: 'interactive',
      label: (
        <Flex align="center" gap="sm">
          🎯 Interactive Charts
          {charts.length > 0 && <Badge variant="secondary">{charts.length}</Badge>}
        </Flex>
      ),
      children: (
        <div className="full-width-content">
          <Stack spacing="lg">
            {/* Chart Creation Panel */}
            {hasResult ? (
              <Card variant="filled" size="medium">
                <Card.Header>
                  <Flex justify="between" align="center">
                    <h3 style={{ margin: 0 }}>Create New Chart</h3>
                    <Button
                      variant="outline"
                      onClick={() => setShowConfig(!showConfig)}
                    >
                      {showConfig ? '🔧 Hide Config' : '⚙️ Show Config'}
                    </Button>
                  </Flex>
                </Card.Header>
                <Card.Content>
                  <Grid columns={3} gap="md">
                    {chartTypes.map((chart) => (
                      <Card 
                        key={chart.type}
                        variant="outlined" 
                        interactive
                        onClick={() => handleCreateChart({ type: chart.type })}
                      >
                        <Card.Content>
                          <Flex direction="column" align="center" gap="sm">
                            <div style={{ fontSize: '2rem' }}>{chart.icon}</div>
                            <div style={{ fontWeight: 'bold' }}>{chart.label}</div>
                            <div style={{ fontSize: '0.875rem', color: '#6b7280', textAlign: 'center' }}>
                              {chart.description}
                            </div>
                          </Flex>
                        </Card.Content>
                      </Card>
                    ))}
                  </Grid>
                </Card.Content>
              </Card>
            ) : (
              <Alert
                variant="info"
                message="No Data Available"
                description="Execute a query first to create visualizations from your data."
                action={
                  <Button variant="primary" size="small" href="/">
                    Go to Query Interface
                  </Button>
                }
              />
            )}

            {/* Configuration Panel */}
            {showConfig && hasResult && (
              <Card variant="outlined" size="large">
                <Card.Header>
                  <h3 style={{ margin: 0 }}>Chart Configuration</h3>
                </Card.Header>
                <Card.Content>
                  <ChartConfigurationPanel 
                    data={currentResult?.data || []}
                    onConfigChange={handleCreateChart}
                  />
                </Card.Content>
              </Card>
            )}

            {/* Interactive Visualization */}
            <Card variant="default" size="large">
              <Card.Header>
                <Flex justify="between" align="center">
                  <h3 style={{ margin: 0 }}>Interactive Visualization</h3>
                  <Flex gap="sm">
                    <Button variant="outline" size="small">
                      📤 Export
                    </Button>
                    <Button variant="outline" size="small">
                      🔗 Share
                    </Button>
                  </Flex>
                </Flex>
              </Card.Header>
              <Card.Content>
                <InteractiveVisualization />
              </Card.Content>
            </Card>

            {/* Created Charts */}
            {charts.length > 0 && (
              <Card variant="default" size="large">
                <Card.Header>
                  <Flex justify="between" align="center">
                    <h3 style={{ margin: 0 }}>Your Charts</h3>
                    <Badge variant="secondary">{charts.length} charts</Badge>
                  </Flex>
                </Card.Header>
                <Card.Content>
                  <Grid columns={2} gap="md">
                    {charts.map((chart) => (
                      <Card key={chart.id} variant="outlined">
                        <Card.Header>
                          <Flex justify="between" align="center">
                            <span style={{ fontWeight: 'bold' }}>
                              {chart.type.charAt(0).toUpperCase() + chart.type.slice(1)} Chart
                            </span>
                            <Button
                              variant="ghost"
                              size="small"
                              onClick={() => removeChart(chart.id)}
                            >
                              🗑️
                            </Button>
                          </Flex>
                        </Card.Header>
                        <Card.Content>
                          <Chart
                            type={chart.type}
                            data={chart.data}
                            config={chart.config}
                          />
                        </Card.Content>
                      </Card>
                    ))}
                  </Grid>
                </Card.Content>
              </Card>
            )}
          </Stack>
        </div>
      ),
    },
    {
      key: 'ai-powered',
      label: '🤖 AI-Powered Charts',
      children: (
        <div className="full-width-content">
          <Stack spacing="lg">
            {/* AI Recommendations */}
            <Card variant="filled" size="medium">
              <Card.Header>
                <h3 style={{ margin: 0 }}>AI Chart Recommendations</h3>
              </Card.Header>
              <Card.Content>
                <VisualizationRecommendations data={currentResult?.data || []} />
              </Card.Content>
            </Card>

            {/* AI-Generated Charts */}
            <Card variant="default" size="large">
              <Card.Header>
                <Flex justify="between" align="center">
                  <h3 style={{ margin: 0 }}>AI-Generated Visualizations</h3>
                  <Button variant="primary">
                    🤖 Generate Charts
                  </Button>
                </Flex>
              </Card.Header>
              <Card.Content>
                {hasResult ? (
                  <div style={{ 
                    padding: '2rem', 
                    textAlign: 'center', 
                    color: '#6b7280',
                    backgroundColor: '#f9fafb',
                    borderRadius: '8px',
                    border: '2px dashed #d1d5db'
                  }}>
                    <div style={{ fontSize: '3rem', marginBottom: '1rem' }}>🤖</div>
                    <h4 style={{ margin: '0 0 0.5rem 0' }}>AI Chart Generation</h4>
                    <p style={{ margin: 0 }}>
                      Click "Generate Charts" to let AI analyze your data and create optimal visualizations
                    </p>
                  </div>
                ) : (
                  <Alert
                    variant="info"
                    message="No Data for AI Analysis"
                    description="Execute a query first to enable AI-powered chart generation."
                  />
                )}
              </Card.Content>
            </Card>
          </Stack>
        </div>
      ),
    },
    {
      key: 'gallery',
      label: '🖼️ Chart Gallery',
      children: (
        <div className="full-width-content">
          <Card variant="default" size="large">
            <Card.Header>
              <h3 style={{ margin: 0 }}>Chart Gallery</h3>
            </Card.Header>
            <Card.Content>
              <div style={{ 
                padding: '2rem', 
                textAlign: 'center', 
                color: '#6b7280' 
              }}>
                <div style={{ fontSize: '3rem', marginBottom: '1rem' }}>🖼️</div>
                <h4 style={{ margin: '0 0 0.5rem 0' }}>Chart Gallery</h4>
                <p style={{ margin: 0 }}>
                  Browse and reuse charts from your visualization history
                </p>
              </div>
            </Card.Content>
          </Card>
        </div>
      ),
    },
  ];

  return (
    <ModernPageLayout
      title="Visualizations"
      subtitle="Create interactive charts and AI-powered visualizations"
      breadcrumb={
        <Breadcrumb
          items={[
            { title: 'Home', path: '/', icon: <HomeOutlined /> },
            { title: 'Visualizations', icon: <BarChartOutlined /> }
          ]}
        />
      }
      tabs={
        <Tabs
          variant="line"
          size="large"
          activeKey={activeTab}
          onChange={handleTabChange}
          items={tabItems}
        />
      }
    >
      {/* Tab content is handled by the Tabs component */}
    </ModernPageLayout>
  );
};

export default VisualizationPage;
