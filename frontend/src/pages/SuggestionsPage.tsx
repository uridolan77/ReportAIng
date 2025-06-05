/**
 * Suggestions Page - AI-powered smart query suggestions
 */

import React, { useState, useEffect } from 'react';
import {
  Card,
  Typography,
  Space,
  Button,
  Breadcrumb,
  Row,
  Col,
  Tag,
  Empty,
  Spin,
  Badge,
  Tooltip
} from 'antd';
import {
  HomeOutlined,
  BulbOutlined,
  RobotOutlined,
  ClockCircleOutlined,
  RiseOutlined,
  UserOutlined,
  DollarOutlined
} from '@ant-design/icons';
import { useNavigate } from 'react-router-dom';
import { QueryProvider } from '../components/QueryInterface/QueryProvider';
import { useQueryContext } from '../components/QueryInterface/QueryProvider';

const { Title, Text } = Typography;

const SuggestionsPageContent: React.FC = () => {
  const navigate = useNavigate();
  const { setQuery } = useQueryContext();
  const [loading, setLoading] = useState(true);
  const [suggestions, setSuggestions] = useState<any[]>([]);

  // Mock AI suggestions - replace with real AI service
  const mockSuggestions = [
    {
      id: '1',
      type: 'trending',
      title: 'Revenue Trend Analysis',
      description: 'Analyze revenue trends for the current quarter',
      query: 'Show me daily revenue trends for the last 7 days with moving average',
      confidence: 0.95,
      category: 'Financial',
      icon: <RiseOutlined />,
      color: '#52c41a',
      reasoning: 'Based on your recent financial queries, this analysis could provide valuable insights into revenue patterns.'
    },
    {
      id: '2',
      type: 'personalized',
      title: 'Player Retention Analysis',
      description: 'Understand player behavior and retention patterns',
      query: 'Show me player retention rates for the last 7 days',
      confidence: 0.88,
      category: 'Players',
      icon: <UserOutlined />,
      color: '#1890ff',
      reasoning: 'You frequently analyze player data. This query can help identify retention trends.'
    },
    {
      id: '3',
      type: 'optimization',
      title: 'High-Value Customer Segments',
      description: 'Identify your most valuable customer segments',
      query: 'Show me top 10 players by deposits in the last 7 days',
      confidence: 0.92,
      category: 'Business Intelligence',
      icon: <DollarOutlined />,
      color: '#faad14',
      reasoning: 'Identifying high-value segments can help optimize marketing and retention strategies.'
    },
    {
      id: '4',
      type: 'seasonal',
      title: 'Seasonal Performance Patterns',
      description: 'Discover seasonal trends in your business',
      query: 'Compare daily performance metrics for the last week vs previous week',
      confidence: 0.85,
      category: 'Analytics',
      icon: <ClockCircleOutlined />,
      color: '#722ed1',
      reasoning: 'Understanding seasonal patterns can help with planning and forecasting.'
    }
  ];

  useEffect(() => {
    // Simulate AI processing
    const timer = setTimeout(() => {
      setSuggestions(mockSuggestions);
      setLoading(false);
    }, 2000);

    return () => clearTimeout(timer);
  }, []);

  const handleSuggestionSelect = (suggestion: any) => {
    setQuery(suggestion.query);
    navigate('/', { state: { selectedQuery: suggestion.query } });
  };

  const getTypeIcon = (type: string) => {
    switch (type) {
      case 'trending': return <RiseOutlined />;
      case 'personalized': return <UserOutlined />;
      case 'optimization': return <DollarOutlined />;
      case 'seasonal': return <ClockCircleOutlined />;
      default: return <BulbOutlined />;
    }
  };

  const getTypeLabel = (type: string) => {
    switch (type) {
      case 'trending': return 'Trending';
      case 'personalized': return 'Personalized';
      case 'optimization': return 'Optimization';
      case 'seasonal': return 'Seasonal';
      default: return 'General';
    }
  };

  return (
    <div style={{ maxWidth: '1400px', margin: '0 auto', padding: '24px' }}>
      <Breadcrumb style={{ marginBottom: '24px' }}>
        <Breadcrumb.Item>
          <HomeOutlined />
          <span onClick={() => navigate('/')} style={{ cursor: 'pointer', marginLeft: '8px' }}>
            Home
          </span>
        </Breadcrumb.Item>
        <Breadcrumb.Item>
          <BulbOutlined />
          Smart Suggestions
        </Breadcrumb.Item>
      </Breadcrumb>

      <div className="animate-fade-in-up" style={{ marginBottom: '32px' }}>
        <Title level={2} style={{ margin: 0, color: '#667eea' }}>
          AI-Powered Query Suggestions
        </Title>
        <Text type="secondary" style={{ fontSize: '16px' }}>
          Intelligent recommendations based on your data patterns and query history
        </Text>
      </div>

      {/* AI Status Card */}
      <Card
        className="enhanced-card animate-fade-in-up stagger-1"
        style={{ marginBottom: '24px' }}
      >
        <Space>
          <RobotOutlined style={{ fontSize: '24px', color: '#667eea' }} />
          <div>
            <Text strong style={{ color: '#667eea' }}>AI Analysis Status</Text>
            <br />
            <Text type="secondary">
              {loading ? 'Analyzing your data patterns...' : `Generated ${suggestions.length} personalized suggestions`}
            </Text>
          </div>
          {loading && <Spin />}
        </Space>
      </Card>

      {/* Suggestions Grid */}
      {loading ? (
        <Card className="enhanced-card">
          <div style={{ textAlign: 'center', padding: '60px 0' }}>
            <Spin size="large" />
            <div style={{ marginTop: '16px' }}>
              <Text type="secondary">AI is analyzing your data patterns...</Text>
            </div>
          </div>
        </Card>
      ) : suggestions.length > 0 ? (
        <Row gutter={[24, 24]}>
          {suggestions.map((suggestion, index) => (
            <Col xs={24} lg={12} key={suggestion.id}>
              <Card
                hoverable
                className={`enhanced-card hover-lift btn-press animate-fade-in-up stagger-${index + 2}`}
                style={{ cursor: 'pointer', height: '100%' }}
                onClick={() => handleSuggestionSelect(suggestion)}
              >
                <Space direction="vertical" style={{ width: '100%' }}>
                  {/* Header */}
                  <Space style={{ width: '100%', justifyContent: 'space-between' }}>
                    <Space>
                      <div style={{
                        fontSize: '20px',
                        color: suggestion.color,
                        padding: '8px',
                        background: `${suggestion.color}15`,
                        borderRadius: '8px'
                      }}>
                        {suggestion.icon}
                      </div>
                      <div>
                        <Text strong style={{ fontSize: '16px' }}>{suggestion.title}</Text>
                        <br />
                        <Tag color={suggestion.color} style={{ marginTop: '4px' }}>
                          {getTypeLabel(suggestion.type)}
                        </Tag>
                      </div>
                    </Space>
                    <Tooltip title={`AI Confidence: ${(suggestion.confidence * 100).toFixed(0)}%`}>
                      <Badge
                        count={`${(suggestion.confidence * 100).toFixed(0)}%`}
                        style={{
                          backgroundColor: suggestion.confidence > 0.9 ? '#52c41a' :
                                          suggestion.confidence > 0.8 ? '#faad14' : '#ff4d4f'
                        }}
                      />
                    </Tooltip>
                  </Space>

                  {/* Description */}
                  <Text type="secondary" style={{ fontSize: '14px' }}>
                    {suggestion.description}
                  </Text>

                  {/* Query Preview */}
                  <div style={{
                    background: '#f6f8fa',
                    padding: '12px',
                    borderRadius: '6px',
                    fontSize: '12px',
                    fontFamily: 'monospace',
                    color: '#586069',
                    border: '1px solid #e1e4e8'
                  }}>
                    {suggestion.query}
                  </div>

                  {/* AI Reasoning */}
                  <div style={{
                    background: `${suggestion.color}08`,
                    padding: '8px 12px',
                    borderRadius: '6px',
                    border: `1px solid ${suggestion.color}20`
                  }}>
                    <Space>
                      <RobotOutlined style={{ color: suggestion.color, fontSize: '12px' }} />
                      <Text style={{ fontSize: '12px', color: '#595959' }}>
                        <strong>AI Insight:</strong> {suggestion.reasoning}
                      </Text>
                    </Space>
                  </div>

                  {/* Category */}
                  <Space>
                    <Tag color="blue">{suggestion.category}</Tag>
                    <Text type="secondary" style={{ fontSize: '11px' }}>
                      Click to run this query
                    </Text>
                  </Space>
                </Space>
              </Card>
            </Col>
          ))}
        </Row>
      ) : (
        <Card className="enhanced-card">
          <Empty
            image={Empty.PRESENTED_IMAGE_SIMPLE}
            description={
              <Space direction="vertical">
                <Text type="secondary" style={{ fontSize: '16px' }}>
                  No suggestions available
                </Text>
                <Text type="secondary" style={{ fontSize: '14px' }}>
                  Run some queries first to get personalized AI suggestions
                </Text>
              </Space>
            }
            style={{ padding: '60px 0' }}
          >
            <Button
              type="primary"
              onClick={() => navigate('/')}
              style={{
                background: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)',
                border: 'none',
                borderRadius: '8px'
              }}
            >
              Start Querying
            </Button>
          </Empty>
        </Card>
      )}

      {/* Quick Actions */}
      <Card
        className="enhanced-card animate-fade-in-up"
        style={{ marginTop: '24px' }}
        title="Quick Actions"
      >
        <Space wrap>
          <Button
            type="primary"
            onClick={() => navigate('/')}
            style={{
              background: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)',
              border: 'none',
              borderRadius: '8px'
            }}
          >
            New Query
          </Button>
          <Button
            onClick={() => navigate('/templates')}
            style={{
              borderRadius: '8px',
              border: '1px solid #52c41a',
              color: '#52c41a'
            }}
          >
            Browse Templates
          </Button>
          <Button
            onClick={() => navigate('/history')}
            style={{
              borderRadius: '8px',
              border: '1px solid #722ed1',
              color: '#722ed1'
            }}
          >
            View History
          </Button>
          <Button
            onClick={() => {
              setLoading(true);
              setTimeout(() => {
                setSuggestions([...mockSuggestions].sort(() => Math.random() - 0.5));
                setLoading(false);
              }, 1500);
            }}
            style={{
              borderRadius: '8px',
              border: '1px solid #fa8c16',
              color: '#fa8c16'
            }}
          >
            Refresh Suggestions
          </Button>
        </Space>
      </Card>
    </div>
  );
};

export const SuggestionsPage: React.FC = () => {
  return (
    <QueryProvider>
      <SuggestionsPageContent />
    </QueryProvider>
  );
};
