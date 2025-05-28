import React, { useState } from 'react';
import { Drawer, Tabs, Button, Space, List, Tag, Statistic, Row, Col, Typography } from 'antd';
import { BugOutlined, ThunderboltOutlined, DatabaseOutlined } from '@ant-design/icons';
import { useAuthStore } from '../../stores/authStore';
import { useVisualizationStore } from '../../stores/visualizationStore';

const { TabPane } = Tabs;
const { Text } = Typography;

export const DevTools: React.FC = () => {
  const [visible, setVisible] = useState(false);
  const currentUser = useAuthStore((state) => state.user);
  const dashboards = useVisualizationStore((state) => state.dashboards);
  const preferences = useVisualizationStore((state) => state.preferences);

  if (process.env.NODE_ENV === 'production') {
    return null;
  }

  const mockQueryHistory = [
    {
      id: '1',
      question: 'Show me total sales',
      sql: 'SELECT SUM(amount) FROM sales',
      successful: true,
      executionTimeMs: 150,
      timestamp: Date.now() - 3600000
    },
    {
      id: '2',
      question: 'Top 10 customers',
      sql: 'SELECT * FROM customers ORDER BY total_spent DESC LIMIT 10',
      successful: true,
      executionTimeMs: 89,
      timestamp: Date.now() - 1800000
    }
  ];

  const calculateAvgQueryTime = (queries: any[]) => {
    if (queries.length === 0) return 0;
    const total = queries.reduce((sum, q) => sum + q.executionTimeMs, 0);
    return Math.round(total / queries.length);
  };

  const calculateCacheHitRate = (queries: any[]) => {
    if (queries.length === 0) return 0;
    const cached = queries.filter(q => q.cached).length;
    return Math.round((cached / queries.length) * 100);
  };

  return (
    <>
      <Button
        type="primary"
        shape="circle"
        icon={<BugOutlined />}
        style={{
          position: 'fixed',
          bottom: 20,
          right: 20,
          zIndex: 1000,
        }}
        onClick={() => setVisible(true)}
      />

      <Drawer
        title="Developer Tools"
        placement="right"
        onClose={() => setVisible(false)}
        open={visible}
        width={600}
      >
        <Tabs defaultActiveKey="queries">
          <TabPane tab="Query History" key="queries">
            <List
              dataSource={mockQueryHistory}
              renderItem={(item) => (
                <List.Item>
                  <List.Item.Meta
                    title={item.question}
                    description={
                      <Space direction="vertical">
                        <Text code>{item.sql}</Text>
                        <Space>
                          <Tag color={item.successful ? 'green' : 'red'}>
                            {item.successful ? 'Success' : 'Failed'}
                          </Tag>
                          <Tag>{item.executionTimeMs}ms</Tag>
                        </Space>
                      </Space>
                    }
                  />
                </List.Item>
              )}
            />
          </TabPane>

          <TabPane tab="Performance" key="performance">
            <Row gutter={16}>
              <Col span={8}>
                <Statistic
                  title="Avg Query Time"
                  value={calculateAvgQueryTime(mockQueryHistory)}
                  suffix="ms"
                />
              </Col>
              <Col span={8}>
                <Statistic
                  title="Cache Hit Rate"
                  value={calculateCacheHitRate(mockQueryHistory)}
                  suffix="%"
                />
              </Col>
              <Col span={8}>
                <Statistic
                  title="Total Queries"
                  value={mockQueryHistory.length}
                />
              </Col>
            </Row>
          </TabPane>

          <TabPane tab="State Inspector" key="state">
            <Space direction="vertical" style={{ width: '100%' }}>
              <div>
                <Text strong>Current User:</Text>
                <pre style={{ background: '#f5f5f5', padding: '8px', borderRadius: '4px' }}>
                  {JSON.stringify(currentUser, null, 2)}
                </pre>
              </div>
              <div>
                <Text strong>Dashboards:</Text>
                <pre style={{ background: '#f5f5f5', padding: '8px', borderRadius: '4px' }}>
                  {JSON.stringify(dashboards, null, 2)}
                </pre>
              </div>
              <div>
                <Text strong>Preferences:</Text>
                <pre style={{ background: '#f5f5f5', padding: '8px', borderRadius: '4px' }}>
                  {JSON.stringify(preferences, null, 2)}
                </pre>
              </div>
            </Space>
          </TabPane>

          <TabPane tab="Mock Data" key="mock">
            <Space direction="vertical" style={{ width: '100%' }}>
              <Button 
                onClick={() => {
                  console.log('Generating mock data...');
                  // Add mock data generation logic here
                }}
              >
                Generate Mock Query Data
              </Button>
              <Button 
                onClick={() => {
                  console.log('Clearing cache...');
                  localStorage.clear();
                  sessionStorage.clear();
                }}
              >
                Clear All Cache
              </Button>
              <Button 
                onClick={() => {
                  console.log('Testing API connection...');
                  // Add connection test logic here
                }}
              >
                Test API Connection
              </Button>
            </Space>
          </TabPane>
        </Tabs>
      </Drawer>
    </>
  );
};
