import React, { useState, useEffect } from 'react';
import {
  Card,
  Row,
  Col,
  Button,
  Space,
  Typography,
  Alert,
  Divider,
  Tag,
  Statistic,
  Progress
} from 'antd';
import {
  BarChartOutlined,
  DashboardOutlined,
  BulbOutlined,
  ThunderboltOutlined,
  RocketOutlined,
  TrophyOutlined
} from '@ant-design/icons';
import AdvancedVisualizationPanel from '../Visualization/AdvancedVisualizationPanel';

const { Title, Text, Paragraph } = Typography;

// Sample data for demonstration
const generateSampleData = (rows: number = 100) => {
  const categories = ['Technology', 'Healthcare', 'Finance', 'Retail', 'Manufacturing'];
  const regions = ['North America', 'Europe', 'Asia Pacific', 'Latin America', 'Middle East'];
  
  return Array.from({ length: rows }, (_, i) => ({
    id: i + 1,
    category: categories[Math.floor(Math.random() * categories.length)],
    region: regions[Math.floor(Math.random() * regions.length)],
    revenue: Math.floor(Math.random() * 1000000) + 50000,
    profit: Math.floor(Math.random() * 200000) + 10000,
    customers: Math.floor(Math.random() * 10000) + 100,
    date: new Date(2023, Math.floor(Math.random() * 12), Math.floor(Math.random() * 28) + 1).toISOString(),
    satisfaction: Math.random() * 5 + 1,
    growth_rate: (Math.random() - 0.5) * 0.4,
    market_share: Math.random() * 0.3 + 0.05
  }));
};

const sampleColumns = [
  { name: 'id', type: 'number', description: 'Unique identifier' },
  { name: 'category', type: 'string', description: 'Business category' },
  { name: 'region', type: 'string', description: 'Geographic region' },
  { name: 'revenue', type: 'number', description: 'Revenue in USD' },
  { name: 'profit', type: 'number', description: 'Profit in USD' },
  { name: 'customers', type: 'number', description: 'Number of customers' },
  { name: 'date', type: 'date', description: 'Transaction date' },
  { name: 'satisfaction', type: 'number', description: 'Customer satisfaction score' },
  { name: 'growth_rate', type: 'number', description: 'Growth rate percentage' },
  { name: 'market_share', type: 'number', description: 'Market share percentage' }
];

const AdvancedVisualizationDemo: React.FC = () => {
  const [sampleData, setSampleData] = useState<any[]>([]);
  const [currentQuery, setCurrentQuery] = useState('');
  const [demoStats, setDemoStats] = useState({
    chartsGenerated: 0,
    dashboardsCreated: 0,
    recommendationsProvided: 0,
    aiAccuracy: 0
  });

  // Initialize demo data
  useEffect(() => {
    const data = generateSampleData(150);
    setSampleData(data);
    setCurrentQuery('Show me revenue and profit analysis by category and region');
    
    // Simulate demo statistics
    setDemoStats({
      chartsGenerated: 12,
      dashboardsCreated: 3,
      recommendationsProvided: 8,
      aiAccuracy: 94.5
    });
  }, []);

  const handleConfigChange = (config: any) => {
    console.log('Demo: Configuration changed:', config);
    setDemoStats(prev => ({
      ...prev,
      chartsGenerated: prev.chartsGenerated + 1
    }));
  };

  const handleExport = (format: string, config: any) => {
    console.log('Demo: Export requested:', format, config);
  };

  const sampleQueries = [
    'Show me revenue trends by category over time',
    'Compare profit margins across different regions',
    'Analyze customer satisfaction vs revenue correlation',
    'Display market share distribution by category',
    'Create a comprehensive business performance dashboard'
  ];

  return (
    <div style={{ padding: '24px', background: '#f0f2f5', minHeight: '100vh' }}>
      {/* Header Section */}
      <Card style={{ marginBottom: 24 }}>
        <Row align="middle" justify="space-between">
          <Col>
            <Space align="center">
              <RocketOutlined style={{ fontSize: 32, color: '#1890ff' }} />
              <div>
                <Title level={2} style={{ margin: 0 }}>
                  Advanced AI Visualization Demo
                </Title>
                <Text type="secondary">
                  Experience the power of AI-driven business intelligence visualizations
                </Text>
              </div>
            </Space>
          </Col>
          <Col>
            <Space>
              <Tag color="blue" icon={<ThunderboltOutlined />}>
                AI-Powered
              </Tag>
              <Tag color="green" icon={<TrophyOutlined />}>
                Enterprise Ready
              </Tag>
            </Space>
          </Col>
        </Row>
      </Card>

      {/* Demo Statistics */}
      <Row gutter={16} style={{ marginBottom: 24 }}>
        <Col span={6}>
          <Card>
            <Statistic
              title="Charts Generated"
              value={demoStats.chartsGenerated}
              prefix={<BarChartOutlined />}
              valueStyle={{ color: '#1890ff' }}
            />
          </Card>
        </Col>
        <Col span={6}>
          <Card>
            <Statistic
              title="Dashboards Created"
              value={demoStats.dashboardsCreated}
              prefix={<DashboardOutlined />}
              valueStyle={{ color: '#52c41a' }}
            />
          </Card>
        </Col>
        <Col span={6}>
          <Card>
            <Statistic
              title="AI Recommendations"
              value={demoStats.recommendationsProvided}
              prefix={<BulbOutlined />}
              valueStyle={{ color: '#faad14' }}
            />
          </Card>
        </Col>
        <Col span={6}>
          <Card>
            <Statistic
              title="AI Accuracy"
              value={demoStats.aiAccuracy}
              suffix="%"
              prefix={<ThunderboltOutlined />}
              valueStyle={{ color: '#722ed1' }}
            />
            <Progress 
              percent={demoStats.aiAccuracy} 
              size="small" 
              status="active"
              strokeColor="#722ed1"
            />
          </Card>
        </Col>
      </Row>

      {/* Feature Highlights */}
      <Card title="🚀 Key Features" style={{ marginBottom: 24 }}>
        <Row gutter={16}>
          <Col span={8}>
            <Alert
              message="AI-Powered Chart Selection"
              description="Our advanced AI analyzes your data characteristics and automatically recommends the most effective visualization types."
              type="info"
              showIcon
              icon={<BulbOutlined />}
            />
          </Col>
          <Col span={8}>
            <Alert
              message="Advanced Dashboard Builder"
              description="Create sophisticated multi-chart dashboards with real-time updates, collaboration features, and responsive design."
              type="success"
              showIcon
              icon={<DashboardOutlined />}
            />
          </Col>
          <Col span={8}>
            <Alert
              message="Performance Optimization"
              description="Automatic data sampling, WebGL acceleration, and virtualization for handling large datasets efficiently."
              type="warning"
              showIcon
              icon={<ThunderboltOutlined />}
            />
          </Col>
        </Row>
      </Card>

      {/* Sample Queries */}
      <Card title="💡 Try These Sample Queries" style={{ marginBottom: 24 }}>
        <Paragraph>
          Click on any of these sample queries to see the AI visualization system in action:
        </Paragraph>
        <Space wrap>
          {sampleQueries.map((query, index) => (
            <Button
              key={index}
              type={currentQuery === query ? 'primary' : 'default'}
              onClick={() => setCurrentQuery(query)}
              style={{ marginBottom: 8 }}
            >
              {query}
            </Button>
          ))}
        </Space>
        <Divider />
        <Text strong>Current Query: </Text>
        <Text code>{currentQuery}</Text>
      </Card>

      {/* Data Overview */}
      <Card title="📊 Sample Dataset" style={{ marginBottom: 24 }}>
        <Row gutter={16}>
          <Col span={12}>
            <Text strong>Dataset Information:</Text>
            <ul>
              <li>Rows: {sampleData.length.toLocaleString()}</li>
              <li>Columns: {sampleColumns.length}</li>
              <li>Data Types: Numbers, Strings, Dates</li>
              <li>Business Domain: Multi-category revenue analysis</li>
            </ul>
          </Col>
          <Col span={12}>
            <Text strong>Available Columns:</Text>
            <div style={{ marginTop: 8 }}>
              {sampleColumns.map((col, index) => (
                <Tag key={index} color={col.type === 'number' ? 'blue' : col.type === 'date' ? 'green' : 'orange'}>
                  {col.name} ({col.type})
                </Tag>
              ))}
            </div>
          </Col>
        </Row>
      </Card>

      {/* Main Demo Component */}
      <AdvancedVisualizationPanel
        data={sampleData}
        columns={sampleColumns}
        query={currentQuery}
        onConfigChange={handleConfigChange}
        onExport={handleExport}
      />

      {/* Footer */}
      <Card style={{ marginTop: 24, textAlign: 'center' }}>
        <Space direction="vertical">
          <Title level={4}>🎯 Ready to Transform Your Business Intelligence?</Title>
          <Paragraph>
            This demo showcases the advanced AI-powered visualization capabilities of the BI Reporting Copilot.
            Experience intelligent chart recommendations, automated dashboard generation, and performance-optimized visualizations.
          </Paragraph>
          <Space>
            <Button type="primary" size="large" icon={<RocketOutlined />}>
              Get Started
            </Button>
            <Button size="large" icon={<BulbOutlined />}>
              Learn More
            </Button>
          </Space>
        </Space>
      </Card>
    </div>
  );
};

export default AdvancedVisualizationDemo;
