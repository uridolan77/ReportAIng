import React, { useState, useEffect } from 'react';
import { Card, Alert, Button, Space, Typography } from 'antd';
import { RocketOutlined, ReloadOutlined } from '@ant-design/icons';
import { useNavigate } from 'react-router-dom';
import AdvancedVisualizationPanel from './AdvancedVisualizationPanel';

const { Title, Text } = Typography;

interface StoredQueryResult {
  data: any[];
  columns: string[];
  query: string;
  timestamp: number;
}

const AdvancedVisualizationWrapper: React.FC = () => {
  const [queryData, setQueryData] = useState<StoredQueryResult | null>(null);
  const [loading, setLoading] = useState(true);
  const navigate = useNavigate();

  // Load data from localStorage
  useEffect(() => {
    const loadStoredData = () => {
      try {
        const storedResult = localStorage.getItem('current-query-result');
        if (storedResult) {
          const parsed = JSON.parse(storedResult);
          setQueryData(parsed);
        }
      } catch (error) {
        console.error('Error loading stored query result:', error);
      } finally {
        setLoading(false);
      }
    };

    loadStoredData();

    // Listen for storage changes (when new queries are executed)
    const handleStorageChange = (e: StorageEvent) => {
      if (e.key === 'current-query-result') {
        loadStoredData();
      }
    };

    window.addEventListener('storage', handleStorageChange);
    return () => window.removeEventListener('storage', handleStorageChange);
  }, []);

  // Refresh data manually
  const handleRefresh = () => {
    setLoading(true);
    setTimeout(() => {
      const storedResult = localStorage.getItem('current-query-result');
      if (storedResult) {
        try {
          const parsed = JSON.parse(storedResult);
          setQueryData(parsed);
        } catch (error) {
          console.error('Error parsing stored data:', error);
        }
      }
      setLoading(false);
    }, 500);
  };

  // Navigate to query interface
  const handleGoToQuery = () => {
    navigate('/');
  };

  if (loading) {
    return (
      <Card loading={true}>
        <div style={{ textAlign: 'center', padding: 40 }}>
          <Text>Loading visualization data...</Text>
        </div>
      </Card>
    );
  }

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
                  // Create sample data for testing
                  const sampleData = {
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
                    columns: ['player_id', 'deposits', 'country', 'date'],
                    query: 'SELECT player_id, deposits, country, date FROM player_deposits WHERE date >= \'2024-01-01\' ORDER BY deposits DESC LIMIT 10',
                    timestamp: Date.now()
                  };
                  localStorage.setItem('current-query-result', JSON.stringify(sampleData));
                  setQueryData(sampleData);
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
