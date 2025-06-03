import React, { useState, useEffect } from 'react';
import {
  Card,
  Row,
  Col,
  Button,
  Space,
  Typography,
  Tag,
  Tooltip,
  Badge,
  Divider,
  Spin,
  Empty
} from 'antd';
import {
  ArrowUpOutlined,
  FireOutlined,
  ClockCircleOutlined,
  DatabaseOutlined,
  UserOutlined,
  BarChartOutlined,
  ThunderboltOutlined,
  BulbOutlined,
  RocketOutlined,
  StarOutlined
} from '@ant-design/icons';

const { Title, Text } = Typography;

interface TrendingQuery {
  id: string;
  query: string;
  category: string;
  popularity: number;
  lastUsed: string;
  avgExecutionTime: number;
  successRate: number;
  description: string;
  tags: string[];
}

interface SchemaBasedSuggestion {
  id: string;
  query: string;
  description: string;
  tables: string[];
  columns: string[];
  complexity: 'beginner' | 'intermediate' | 'advanced';
  estimatedRows: number;
}

interface ProactiveSuggestionsProps {
  onQuerySelect: (query: string) => void;
  onStartWizard: () => void;
  userRole?: string;
  recentQueries?: string[];
}

export const ProactiveSuggestions: React.FC<ProactiveSuggestionsProps> = ({
  onQuerySelect,
  onStartWizard,
  userRole = 'user',
  recentQueries = []
}) => {
  const [loading, setLoading] = useState(false);
  const [trendingQueries, setTrendingQueries] = useState<TrendingQuery[]>([]);
  const [schemaBasedSuggestions, setSchemaBasedSuggestions] = useState<SchemaBasedSuggestion[]>([]);
  const [selectedCategory, setSelectedCategory] = useState<string>('all');

  // Mock trending queries - in real app, fetch from API
  useEffect(() => {
    setLoading(true);
    // Simulate API call
    setTimeout(() => {
      setTrendingQueries([
        {
          id: '1',
          query: 'Show me total deposits for yesterday',
          category: 'Revenue',
          popularity: 95,
          lastUsed: '2 hours ago',
          avgExecutionTime: 1.2,
          successRate: 98,
          description: 'Daily deposit analysis',
          tags: ['deposits', 'daily', 'revenue']
        },
        {
          id: '2',
          query: 'Top 10 players by deposits in the last 7 days',
          category: 'Players',
          popularity: 87,
          lastUsed: '4 hours ago',
          avgExecutionTime: 2.1,
          successRate: 95,
          description: 'High-value player identification',
          tags: ['players', 'top10', 'deposits']
        },
        {
          id: '3',
          query: 'Revenue breakdown by country for last week',
          category: 'Geography',
          popularity: 82,
          lastUsed: '1 day ago',
          avgExecutionTime: 3.5,
          successRate: 92,
          description: 'Geographic revenue analysis',
          tags: ['revenue', 'country', 'geographic']
        },
        {
          id: '4',
          query: 'Casino vs sports betting revenue comparison',
          category: 'Products',
          popularity: 78,
          lastUsed: '6 hours ago',
          avgExecutionTime: 1.8,
          successRate: 96,
          description: 'Product performance comparison',
          tags: ['casino', 'sports', 'comparison']
        }
      ]);

      setSchemaBasedSuggestions([
        {
          id: 's1',
          query: 'Show me user registration trends by month',
          description: 'Analyze user growth patterns over time',
          tables: ['users', 'registrations'],
          columns: ['created_at', 'user_id', 'registration_date'],
          complexity: 'beginner',
          estimatedRows: 15000
        },
        {
          id: 's2',
          query: 'Calculate average session duration by device type',
          description: 'Understand user engagement across platforms',
          tables: ['sessions', 'devices'],
          columns: ['session_duration', 'device_type', 'start_time'],
          complexity: 'intermediate',
          estimatedRows: 45000
        },
        {
          id: 's3',
          query: 'Find correlation between deposit amount and player lifetime value',
          description: 'Advanced analytics for player value prediction',
          tables: ['deposits', 'players', 'transactions'],
          columns: ['amount', 'player_id', 'lifetime_value'],
          complexity: 'advanced',
          estimatedRows: 120000
        }
      ]);
      setLoading(false);
    }, 1000);
  }, []);

  const categories = ['all', 'Revenue', 'Players', 'Geography', 'Products'];

  const filteredTrendingQueries = selectedCategory === 'all' 
    ? trendingQueries 
    : trendingQueries.filter(q => q.category === selectedCategory);

  const getComplexityColor = (complexity: string) => {
    switch (complexity) {
      case 'beginner': return '#52c41a';
      case 'intermediate': return '#faad14';
      case 'advanced': return '#f5222d';
      default: return '#1890ff';
    }
  };

  const getPopularityIcon = (popularity: number) => {
    if (popularity >= 90) return <FireOutlined style={{ color: '#ff4d4f' }} />;
    if (popularity >= 80) return <ArrowUpOutlined style={{ color: '#faad14' }} />;
    return <BarChartOutlined style={{ color: '#1890ff' }} />;
  };

  const renderTrendingQuery = (query: TrendingQuery) => (
    <Card
      key={query.id}
      size="small"
      hoverable
      className="trending-query-card"
      style={{
        marginBottom: '12px',
        borderRadius: '12px',
        border: '1px solid #f0f0f0',
        transition: 'all 0.3s ease'
      }}
      bodyStyle={{ padding: '16px' }}
      onClick={() => onQuerySelect(query.query)}
    >
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', marginBottom: '8px' }}>
        <Space>
          {getPopularityIcon(query.popularity)}
          <Text strong style={{ fontSize: '14px' }}>{query.query}</Text>
        </Space>
        <Badge count={`${query.popularity}%`} style={{ backgroundColor: '#52c41a' }} />
      </div>
      
      <Text type="secondary" style={{ fontSize: '12px', display: 'block', marginBottom: '8px' }}>
        {query.description}
      </Text>
      
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '8px' }}>
        <Space size="small">
          <Tag color="blue">{query.category}</Tag>
          <Tooltip title="Average execution time">
            <Tag icon={<ClockCircleOutlined />}>{query.avgExecutionTime}s</Tag>
          </Tooltip>
          <Tooltip title="Success rate">
            <Tag color="green">{query.successRate}%</Tag>
          </Tooltip>
        </Space>
        <Text type="secondary" style={{ fontSize: '11px' }}>
          Used {query.lastUsed}
        </Text>
      </div>
      
      <div style={{ display: 'flex', flexWrap: 'wrap', gap: '4px' }}>
        {query.tags.map(tag => (
          <Tag key={tag} size="small" style={{ fontSize: '10px' }}>{tag}</Tag>
        ))}
      </div>
    </Card>
  );

  const renderSchemaBasedSuggestion = (suggestion: SchemaBasedSuggestion) => (
    <Card
      key={suggestion.id}
      size="small"
      hoverable
      style={{
        marginBottom: '12px',
        borderRadius: '12px',
        border: '1px solid #f0f0f0'
      }}
      bodyStyle={{ padding: '16px' }}
      onClick={() => onQuerySelect(suggestion.query)}
    >
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', marginBottom: '8px' }}>
        <Space>
          <DatabaseOutlined style={{ color: '#1890ff' }} />
          <Text strong style={{ fontSize: '14px' }}>{suggestion.query}</Text>
        </Space>
        <Tag color={getComplexityColor(suggestion.complexity)}>
          {suggestion.complexity}
        </Tag>
      </div>
      
      <Text type="secondary" style={{ fontSize: '12px', display: 'block', marginBottom: '8px' }}>
        {suggestion.description}
      </Text>
      
      <div style={{ marginBottom: '8px' }}>
        <Text style={{ fontSize: '11px', color: '#666' }}>
          <strong>Tables:</strong> {suggestion.tables.join(', ')}
        </Text>
      </div>
      
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
        <Text style={{ fontSize: '11px', color: '#666' }}>
          ~{suggestion.estimatedRows.toLocaleString()} rows
        </Text>
        <Button size="small" type="link" style={{ padding: 0, height: 'auto' }}>
          Try it →
        </Button>
      </div>
    </Card>
  );

  return (
    <div style={{ padding: '24px 0' }}>
      {/* Header */}
      <div style={{ textAlign: 'center', marginBottom: '32px' }}>
        <Title level={3} style={{
          margin: '0 0 8px 0',
          background: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)',
          WebkitBackgroundClip: 'text',
          WebkitTextFillColor: 'transparent',
          fontFamily: "'Poppins', sans-serif"
        }}>
          🚀 Get Started with AI-Powered Insights
        </Title>
        <Text style={{ fontSize: '16px', color: '#6b7280' }}>
          Discover trending queries and smart suggestions based on your data
        </Text>
      </div>

      {/* Quick Actions */}
      <Row gutter={[16, 16]} style={{ marginBottom: '32px' }}>
        <Col xs={24} sm={8}>
          <Button
            type="primary"
            size="large"
            icon={<RocketOutlined />}
            onClick={onStartWizard}
            style={{
              width: '100%',
              height: '60px',
              borderRadius: '12px',
              background: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)',
              border: 'none',
              fontSize: '16px',
              fontWeight: 600
            }}
          >
            Start Query Wizard
          </Button>
        </Col>
        <Col xs={24} sm={8}>
          <Button
            size="large"
            icon={<BulbOutlined />}
            style={{
              width: '100%',
              height: '60px',
              borderRadius: '12px',
              fontSize: '16px',
              fontWeight: 600
            }}
          >
            Browse Templates
          </Button>
        </Col>
        <Col xs={24} sm={8}>
          <Button
            size="large"
            icon={<StarOutlined />}
            style={{
              width: '100%',
              height: '60px',
              borderRadius: '12px',
              fontSize: '16px',
              fontWeight: 600
            }}
          >
            My Favorites
          </Button>
        </Col>
      </Row>

      <Row gutter={[24, 24]}>
        {/* Trending Queries */}
        <Col xs={24} lg={12}>
          <Card
            title={
              <Space>
                <ArrowUpOutlined style={{ color: '#ff4d4f' }} />
                <span>Trending Queries</span>
                <Badge count={trendingQueries.length} style={{ backgroundColor: '#ff4d4f' }} />
              </Space>
            }
            extra={
              <Space>
                {categories.map(category => (
                  <Button
                    key={category}
                    size="small"
                    type={selectedCategory === category ? 'primary' : 'text'}
                    onClick={() => setSelectedCategory(category)}
                    style={{ borderRadius: '6px' }}
                  >
                    {category}
                  </Button>
                ))}
              </Space>
            }
            style={{ height: '100%' }}
          >
            {loading ? (
              <div style={{ textAlign: 'center', padding: '40px' }}>
                <Spin size="large" />
                <div style={{ marginTop: '16px' }}>
                  <Text>Loading trending queries...</Text>
                </div>
              </div>
            ) : filteredTrendingQueries.length > 0 ? (
              <div style={{ maxHeight: '400px', overflowY: 'auto' }}>
                {filteredTrendingQueries.map(renderTrendingQuery)}
              </div>
            ) : (
              <Empty description="No trending queries found" />
            )}
          </Card>
        </Col>

        {/* Schema-Based Suggestions */}
        <Col xs={24} lg={12}>
          <Card
            title={
              <Space>
                <DatabaseOutlined style={{ color: '#1890ff' }} />
                <span>Smart Suggestions</span>
                <Tag color="blue">Based on your data</Tag>
              </Space>
            }
            style={{ height: '100%' }}
          >
            {loading ? (
              <div style={{ textAlign: 'center', padding: '40px' }}>
                <Spin size="large" />
                <div style={{ marginTop: '16px' }}>
                  <Text>Analyzing your data schema...</Text>
                </div>
              </div>
            ) : (
              <div style={{ maxHeight: '400px', overflowY: 'auto' }}>
                {schemaBasedSuggestions.map(renderSchemaBasedSuggestion)}
              </div>
            )}
          </Card>
        </Col>
      </Row>
    </div>
  );
};
