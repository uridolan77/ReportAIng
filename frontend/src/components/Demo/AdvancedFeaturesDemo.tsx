import React, { useState, useEffect } from 'react';
import { Card, Row, Col, Tabs, Button, Space, Typography, Statistic, Progress } from 'antd';
import { HeatmapChart } from '../Visualization/D3Charts/HeatmapChart';
import { TreemapChart } from '../Visualization/D3Charts/TreemapChart';
import { NetworkChart } from '../Visualization/D3Charts/NetworkChart';
import { VirtualDataTable } from '../Performance/VirtualScrollList';
import { AccessibleBarChart } from '../Visualization/AccessibleChart';
import { useOptimizedSearch, useChunkedProcessing, useSortedData } from '../../hooks/useOptimization';
import { usePerformanceMeasure, useMemoryMonitor } from '../../hooks/usePerformance';
import { useAdvancedQueryStore } from '../../stores/advancedQueryStore';
import { useVisualizationStore } from '../../stores/visualizationStore';

const { TabPane } = Tabs;
const { Title, Text } = Typography;

// Mock data generators
const generateHeatmapData = () => {
  const data = [];
  const categories = ['Q1', 'Q2', 'Q3', 'Q4'];
  const regions = ['North', 'South', 'East', 'West', 'Central'];
  
  for (const x of categories) {
    for (const y of regions) {
      data.push({
        x,
        y,
        value: Math.random() * 100 + 10,
        label: `${x} ${y}: ${(Math.random() * 100 + 10).toFixed(1)}`
      });
    }
  }
  return data;
};

const generateTreemapData = () => ({
  name: 'Sales',
  children: [
    {
      name: 'Products',
      children: [
        { name: 'Product A', value: 1000, category: 'Electronics', description: 'High-end electronics' },
        { name: 'Product B', value: 800, category: 'Electronics', description: 'Mid-range electronics' },
        { name: 'Product C', value: 600, category: 'Clothing', description: 'Fashion items' },
        { name: 'Product D', value: 400, category: 'Clothing', description: 'Accessories' }
      ]
    },
    {
      name: 'Services',
      children: [
        { name: 'Consulting', value: 500, category: 'Professional', description: 'Business consulting' },
        { name: 'Support', value: 300, category: 'Professional', description: 'Technical support' }
      ]
    }
  ]
});

const generateNetworkData = () => ({
  nodes: [
    { id: '1', name: 'Customer A', group: 'customer', size: 20, description: 'High-value customer' },
    { id: '2', name: 'Customer B', group: 'customer', size: 15, description: 'Regular customer' },
    { id: '3', name: 'Product X', group: 'product', size: 25, description: 'Best-selling product' },
    { id: '4', name: 'Product Y', group: 'product', size: 18, description: 'New product' },
    { id: '5', name: 'Service Z', group: 'service', size: 12, description: 'Premium service' }
  ],
  links: [
    { source: '1', target: '3', value: 10, label: 'Purchased' },
    { source: '1', target: '5', value: 5, label: 'Subscribed' },
    { source: '2', target: '3', value: 8, label: 'Purchased' },
    { source: '2', target: '4', value: 6, label: 'Purchased' }
  ]
});

const generateLargeDataset = (size: number) => {
  return Array.from({ length: size }, (_, i) => ({
    id: `item-${i}`,
    name: `Item ${i + 1}`,
    value: Math.floor(Math.random() * 1000),
    category: ['Electronics', 'Clothing', 'Books', 'Sports'][i % 4],
    description: `Description for item ${i + 1}`
  }));
};

export const AdvancedFeaturesDemo: React.FC = () => {
  const [activeTab, setActiveTab] = useState('charts');
  const [searchTerm, setSearchTerm] = useState('');
  const [sortKey, setSortKey] = useState<'name' | 'value' | null>('name');
  const [sortDirection, setSortDirection] = useState<'asc' | 'desc'>('asc');
  
  // Performance monitoring
  const { measureAsync } = usePerformanceMeasure();
  const memoryInfo = useMemoryMonitor();
  
  // Store usage
  const { preferences, updatePreferences } = useVisualizationStore();
  const { settings: querySettings, updateSettings } = useAdvancedQueryStore();
  
  // Large dataset for performance testing
  const [largeDataset] = useState(() => generateLargeDataset(10000));
  
  // Optimized data processing
  const filteredData = useOptimizedSearch(
    largeDataset,
    searchTerm,
    ['name', 'category'],
    300
  );
  
  const sortedData = useSortedData(filteredData, sortKey, sortDirection);
  
  const { processedData, isProcessing, progress } = useChunkedProcessing(
    sortedData,
    (chunk) => chunk.map(item => ({ ...item, processed: true })),
    1000
  );
  
  // Chart data
  const [heatmapData] = useState(generateHeatmapData);
  const [treemapData] = useState(generateTreemapData);
  const [networkData] = useState(generateNetworkData);
  
  const handleChartInteraction = async (type: string, data: any) => {
    const { result, duration } = await measureAsync(
      `${type}-interaction`,
      async () => {
        // Simulate some processing
        await new Promise(resolve => setTimeout(resolve, 100));
        return data;
      }
    );
    
    console.log(`${type} interaction took ${duration.toFixed(2)}ms:`, result);
  };

  return (
    <div style={{ padding: '24px' }}>
      <Title level={2}>Advanced Features Demo</Title>
      <Text type="secondary">
        Showcasing advanced D3.js charts, performance optimizations, and sophisticated state management
      </Text>
      
      {/* Performance Stats */}
      <Card style={{ marginTop: '16px', marginBottom: '24px' }}>
        <Row gutter={16}>
          <Col span={6}>
            <Statistic
              title="Memory Usage"
              value={memoryInfo?.usedJSHeapSize || 0}
              formatter={(value) => `${(Number(value) / 1024 / 1024).toFixed(1)} MB`}
            />
          </Col>
          <Col span={6}>
            <Statistic
              title="Data Points"
              value={processedData.length}
              suffix={`/ ${largeDataset.length}`}
            />
          </Col>
          <Col span={6}>
            <Statistic
              title="Processing"
              value={progress}
              suffix="%"
            />
          </Col>
          <Col span={6}>
            <Statistic
              title="Animations"
              value={preferences.enableAnimations ? 'ON' : 'OFF'}
            />
          </Col>
        </Row>
        {isProcessing && (
          <Progress percent={progress} style={{ marginTop: '16px' }} />
        )}
      </Card>

      <Tabs activeKey={activeTab} onChange={setActiveTab}>
        <TabPane tab="Advanced Charts" key="charts">
          <Row gutter={[16, 16]}>
            <Col span={12}>
              <Card title="Interactive Heatmap">
                <HeatmapChart
                  data={heatmapData}
                  config={{
                    title: 'Quarterly Sales by Region',
                    colorScheme: 'RdYlBu',
                    showValues: true,
                    interactive: true
                  }}
                  onCellClick={(data) => handleChartInteraction('heatmap', data)}
                />
              </Card>
            </Col>
            <Col span={12}>
              <Card title="Hierarchical Treemap">
                <TreemapChart
                  data={treemapData}
                  config={{
                    title: 'Sales Breakdown',
                    showLabels: true,
                    interactive: true
                  }}
                  onNodeClick={(data) => handleChartInteraction('treemap', data)}
                />
              </Card>
            </Col>
            <Col span={24}>
              <Card title="Network Visualization">
                <NetworkChart
                  data={networkData}
                  config={{
                    title: 'Customer-Product Relationships',
                    interactive: true,
                    showLabels: true,
                    linkDistance: 150
                  }}
                  onNodeClick={(data) => handleChartInteraction('network-node', data)}
                  onLinkClick={(data) => handleChartInteraction('network-link', data)}
                />
              </Card>
            </Col>
          </Row>
        </TabPane>

        <TabPane tab="Performance Features" key="performance">
          <Space direction="vertical" style={{ width: '100%' }}>
            <Card title="Virtual Scrolling Demo">
              <Space style={{ marginBottom: '16px' }}>
                <input
                  placeholder="Search items..."
                  value={searchTerm}
                  onChange={(e) => setSearchTerm(e.target.value)}
                  style={{ padding: '8px', width: '200px' }}
                />
                <Button
                  onClick={() => setSortKey(sortKey === 'name' ? 'value' : 'name')}
                >
                  Sort by {sortKey === 'name' ? 'Value' : 'Name'}
                </Button>
                <Button
                  onClick={() => setSortDirection(sortDirection === 'asc' ? 'desc' : 'asc')}
                >
                  {sortDirection === 'asc' ? '↑' : '↓'}
                </Button>
              </Space>
              <VirtualDataTable
                data={processedData.slice(0, 1000)} // Limit for demo
                onRowClick={(row) => console.log('Row clicked:', row)}
              />
            </Card>

            <Card title="Accessible Chart">
              <AccessibleBarChart
                data={[
                  { name: 'Jan', value: 100 },
                  { name: 'Feb', value: 150 },
                  { name: 'Mar', value: 120 },
                  { name: 'Apr', value: 180 }
                ]}
                title="Monthly Sales (Keyboard Navigable)"
              />
            </Card>
          </Space>
        </TabPane>

        <TabPane tab="State Management" key="state">
          <Row gutter={[16, 16]}>
            <Col span={12}>
              <Card title="Visualization Preferences">
                <Space direction="vertical" style={{ width: '100%' }}>
                  <div>
                    <label>
                      <input
                        type="checkbox"
                        checked={preferences.enableAnimations}
                        onChange={(e) => updatePreferences({ enableAnimations: e.target.checked })}
                      />
                      Enable Animations
                    </label>
                  </div>
                  <div>
                    <label>
                      <input
                        type="checkbox"
                        checked={preferences.enableInteractivity}
                        onChange={(e) => updatePreferences({ enableInteractivity: e.target.checked })}
                      />
                      Enable Interactivity
                    </label>
                  </div>
                  <div>
                    <label>
                      Theme:
                      <select
                        value={preferences.theme}
                        onChange={(e) => updatePreferences({ theme: e.target.value as any })}
                        style={{ marginLeft: '8px' }}
                      >
                        <option value="light">Light</option>
                        <option value="dark">Dark</option>
                        <option value="auto">Auto</option>
                      </select>
                    </label>
                  </div>
                </Space>
              </Card>
            </Col>
            <Col span={12}>
              <Card title="Query Settings">
                <Space direction="vertical" style={{ width: '100%' }}>
                  <div>
                    <label>
                      <input
                        type="checkbox"
                        checked={querySettings.cacheEnabled}
                        onChange={(e) => updateSettings({ cacheEnabled: e.target.checked })}
                      />
                      Enable Query Cache
                    </label>
                  </div>
                  <div>
                    <label>
                      <input
                        type="checkbox"
                        checked={querySettings.autoSuggestEnabled}
                        onChange={(e) => updateSettings({ autoSuggestEnabled: e.target.checked })}
                      />
                      Auto Suggestions
                    </label>
                  </div>
                  <div>
                    <label>
                      Max History:
                      <input
                        type="number"
                        value={querySettings.maxHistoryItems}
                        onChange={(e) => updateSettings({ maxHistoryItems: parseInt(e.target.value) })}
                        style={{ marginLeft: '8px', width: '80px' }}
                      />
                    </label>
                  </div>
                </Space>
              </Card>
            </Col>
          </Row>
        </TabPane>
      </Tabs>
    </div>
  );
};
