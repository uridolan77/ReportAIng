import React from 'react';
import { Card, Alert, Button, Space, Typography } from 'antd';
import { RocketOutlined, ReloadOutlined } from '@ant-design/icons';
import { useNavigate } from 'react-router-dom';
import { useActiveResult, useActiveResultActions } from '../../stores/activeResultStore';
import AdvancedVisualizationPanel from './AdvancedVisualizationPanel';

const { Title, Text } = Typography;

// StoredQueryResult interface removed - not used in this component

const AdvancedVisualizationWrapper: React.FC = () => {
  const navigate = useNavigate();
  // loading state removed - not used in current implementation
  const { result: activeResult, query: activeQuery } = useActiveResult();
  const { setActiveResult } = useActiveResultActions();

  // Convert active result to the format expected by the visualization panel
  const queryData = activeResult ? {
    data: activeResult.result?.data || [],
    columns: activeResult.result?.metadata?.columns?.map(col => col.name) || [],
    query: activeQuery,
    timestamp: Date.now()
  } : null;

  // Refresh data manually
  const handleRefresh = () => {
    // Just refresh the page or trigger a re-render since we're using the store
    window.location.reload();
  };

  // Navigate to query interface
  const handleGoToQuery = () => {
    navigate('/');
  };

  // Loading state removed - component loads instantly from store

  // Show welcome message if no data
  if (!queryData || !queryData.data || queryData.data.length === 0) {
    return (
      <div style={{ padding: 24 }}>
        <Card>
          <div style={{ textAlign: 'center', padding: 40 }}>
            <RocketOutlined style={{ fontSize: 64, color: '#1890ff', marginBottom: 16 }} />
            <Title level={3}>AI-Powered Charts</Title>
            <Text type="secondary" style={{ display: 'block', marginBottom: 24 }}>
              Create stunning visualizations with AI assistance. Execute a query first to get started.
            </Text>

            <Alert
              message="No Data Available"
              description="To create AI-powered charts, you need to execute a query first. The system will analyze your data and suggest the best visualizations."
              type="info"
              showIcon
              style={{ marginBottom: 24, textAlign: 'left' }}
            />

            <Space>
              <Button type="primary" onClick={handleGoToQuery}>
                Go to Query Interface
              </Button>
              <Button icon={<ReloadOutlined />} onClick={handleRefresh}>
                Refresh
              </Button>
              <Button
                type="dashed"
                onClick={() => {
                  // Create sample data for testing using the active result store
                  const sampleResult = {
                    success: true,
                    queryId: 'sample-' + Date.now(),
                    sql: 'SELECT player_id, deposits, country, date FROM player_deposits WHERE date >= \'2024-01-01\' ORDER BY deposits DESC LIMIT 10',
                    result: {
                      data: [
                        { player_id: 'P001', deposits: 1500, country: 'US', date: '2024-01-01' },
                        { player_id: 'P002', deposits: 2300, country: 'UK', date: '2024-01-01' },
                        { player_id: 'P003', deposits: 800, country: 'CA', date: '2024-01-01' },
                        { player_id: 'P004', deposits: 3200, country: 'US', date: '2024-01-02' },
                        { player_id: 'P005', deposits: 1900, country: 'DE', date: '2024-01-02' },
                        { player_id: 'P006', deposits: 2700, country: 'UK', date: '2024-01-02' },
                        { player_id: 'P007', deposits: 1200, country: 'CA', date: '2024-01-03' },
                        { player_id: 'P008', deposits: 4100, country: 'US', date: '2024-01-03' },
                        { player_id: 'P009', deposits: 1600, country: 'DE', date: '2024-01-03' },
                        { player_id: 'P010', deposits: 2900, country: 'UK', date: '2024-01-03' }
                      ],
                      metadata: {
                        columns: [
                          { name: 'player_id', type: 'string' },
                          { name: 'deposits', type: 'number' },
                          { name: 'country', type: 'string' },
                          { name: 'date', type: 'string' }
                        ],
                        rowCount: 10
                      }
                    },
                    executionTimeMs: 150,
                    timestamp: new Date().toISOString(),
                    confidence: 0.95
                  };
                  setActiveResult(sampleResult, 'Show me sample player deposit data');
                }}
              >
                Load Sample Data
              </Button>
            </Space>
          </div>
        </Card>
      </div>
    );
  }

  // Show the advanced visualization panel with data
  return (
    <div style={{ padding: 24 }}>
      <AdvancedVisualizationPanel
        data={queryData.data}
        columns={queryData.columns}
        query={queryData.query}
        onConfigChange={(config) => {
          console.log('Visualization config changed:', config);
        }}
      />
    </div>
  );
};

export default AdvancedVisualizationWrapper;
