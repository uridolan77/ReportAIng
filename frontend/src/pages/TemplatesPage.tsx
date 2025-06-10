/**
 * Templates Page - Browse and use query templates
 */

import React, { useState } from 'react';
import {
  Space,
  Input,
  Select,
  Tag,
  Empty,
  Badge,
  Row,
  Col,
  Typography,
  Card,
  Button
} from 'antd';
import {
  HomeOutlined,
  BookOutlined,
  SearchOutlined,
  StarOutlined,
  StarFilled,
  FireOutlined
} from '@ant-design/icons';
import { useNavigate } from 'react-router-dom';
import { QueryProvider } from '../components/QueryInterface/QueryProvider';
import { useQueryContext } from '../components/QueryInterface/QueryProvider';

const { Option } = Select;
const { Text } = Typography;

const TemplatesPageContent: React.FC = () => {
  const navigate = useNavigate();
  const { setQuery } = useQueryContext();
  const [searchTerm, setSearchTerm] = useState('');
  const [selectedCategory, setSelectedCategory] = useState('all');

  // Mock template data - replace with real data from your service
  const templates = [
    {
      id: '1',
      name: 'Revenue Analysis',
      description: 'Analyze revenue trends and patterns',
      category: 'Financial',
      difficulty: 'beginner',
      usageCount: 156,
      isFavorite: true,
      template: 'Show me total revenue for the last 7 days broken down by day',
      estimatedRows: '~50 rows'
    },
    {
      id: '2',
      name: 'Top Players Report',
      description: 'Find top performing players by various metrics',
      category: 'Players',
      difficulty: 'intermediate',
      usageCount: 89,
      isFavorite: false,
      template: 'Show me top 10 players by deposits in the last 7 days',
      estimatedRows: '~10 rows'
    },
    {
      id: '3',
      name: 'Geographic Performance',
      description: 'Analyze performance by country and region',
      category: 'Geographic',
      difficulty: 'beginner',
      usageCount: 234,
      isFavorite: true,
      template: 'Show me revenue breakdown by country for last week',
      estimatedRows: '~25 rows'
    },
    {
      id: '4',
      name: 'Game Performance Analysis',
      description: 'Compare different game types and performance',
      category: 'Gaming',
      difficulty: 'advanced',
      usageCount: 67,
      isFavorite: false,
      template: 'Compare casino vs sports betting revenue for the last quarter',
      estimatedRows: '~100 rows'
    }
  ];

  const categories = ['Financial', 'Players', 'Geographic', 'Gaming'];

  const filteredTemplates = templates.filter(template => {
    const matchesSearch = template.name.toLowerCase().includes(searchTerm.toLowerCase()) ||
                         template.description.toLowerCase().includes(searchTerm.toLowerCase());
    const matchesCategory = selectedCategory === 'all' ||
                           selectedCategory === 'favorites' ? template.isFavorite :
                           template.category === selectedCategory;
    return matchesSearch && matchesCategory;
  });

  const popularTemplates = templates.filter(t => t.usageCount > 100);

  const handleTemplateSelect = (template: any) => {
    setQuery(template.template);
    navigate('/', { state: { selectedQuery: template.template } });
  };

  const getDifficultyColor = (difficulty: string) => {
    switch (difficulty) {
      case 'beginner': return 'green';
      case 'intermediate': return 'orange';
      case 'advanced': return 'red';
      default: return 'default';
    }
  };

  return (
    <div style={{ padding: '24px', background: '#f5f5f5', minHeight: '100vh' }}>
      <div style={{ marginBottom: '24px' }}>
        <Space>
          <HomeOutlined />
          <span
            onClick={() => navigate('/')}
            style={{ cursor: 'pointer', color: '#1890ff' }}
          >
            Home
          </span>
          <span>/</span>
          <BookOutlined />
          <span>Query Templates</span>
        </Space>
      </div>

      <div style={{ marginBottom: '24px' }}>
        <h2 style={{ margin: 0, color: '#667eea', fontSize: '2rem', fontWeight: 600 }}>
          Query Templates
        </h2>
        <p style={{ color: '#8c8c8c', fontSize: '16px', margin: 0 }}>
          Pre-built queries to help you get started quickly ({templates.length} available)
        </p>
      </div>

      {/* Search and Filters */}
      <Card style={{ marginBottom: '24px' }}>
        <Row gutter={[16, 16]} align="middle">
          <Col xs={24} md={12}>
            <Input
              placeholder="Search templates..."
              prefix={<SearchOutlined />}
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
              allowClear
            />
          </Col>
          <Col xs={24} md={8}>
            <Select
              value={selectedCategory}
              onChange={setSelectedCategory}
              style={{ width: '100%' }}
              placeholder="Select category"
            >
              <Option value="all">All Categories</Option>
              <Option value="favorites">
                <Space>
                  <StarFilled style={{ color: '#faad14' }} />
                  Favorites
                </Space>
              </Option>
              {categories.map(category => (
                <Option key={category} value={category}>
                  {category}
                </Option>
              ))}
            </Select>
          </Col>
          <Col xs={24} md={4}>
            <Text type="secondary">
              {filteredTemplates.length} templates
            </Text>
          </Col>
        </Row>
      </Card>

      {/* Popular Templates */}
      {popularTemplates.length > 0 && selectedCategory === 'all' && !searchTerm && (
        <Card
          title={
            <Space>
              <FireOutlined style={{ color: '#ff4d4f' }} />
              <span style={{ color: '#667eea' }}>Popular Templates</span>
            </Space>
          }
          style={{ marginBottom: '24px' }}
        >
          <Row gutter={[16, 16]}>
            {popularTemplates.slice(0, 3).map((template, index) => (
              <Col xs={24} md={8} key={template.id}>
                <Card
                  size="small"
                  hoverable
                  onClick={() => handleTemplateSelect(template)}
                  style={{ cursor: 'pointer' }}
                >
                  <Space direction="vertical" style={{ width: '100%' }}>
                    <Space>
                      <Text strong>{template.name}</Text>
                      <Badge count={template.usageCount} />
                    </Space>
                    <Text type="secondary" style={{ fontSize: '12px' }}>
                      {template.description}
                    </Text>
                  </Space>
                </Card>
              </Col>
            ))}
          </Row>
        </Card>
      )}

      {/* Templates Grid */}
      <Row gutter={[24, 24]}>
        {filteredTemplates.length > 0 ? (
          filteredTemplates.map((template, index) => (
            <Col xs={24} lg={12} key={template.id}>
              <Card
                hoverable
                style={{ cursor: 'pointer', height: '100%' }}
                onClick={() => handleTemplateSelect(template)}
                actions={[
                  <Space key="stats">
                    <Badge count={template.usageCount} />
                    <Text type="secondary">uses</Text>
                  </Space>,
                  template.isFavorite ? (
                    <StarFilled key="favorite" style={{ color: '#faad14' }} />
                  ) : (
                    <StarOutlined key="favorite" />
                  )
                ]}
              >
                <Space direction="vertical" style={{ width: '100%' }}>
                  <Space>
                    <Text strong style={{ fontSize: '16px' }}>{template.name}</Text>
                    <Tag color={getDifficultyColor(template.difficulty)}>
                      {template.difficulty}
                    </Tag>
                  </Space>

                  <Text type="secondary" style={{ fontSize: '14px' }}>
                    {template.description}
                  </Text>

                  <Space wrap>
                    <Tag color="blue">{template.category}</Tag>
                    <Tag>{template.estimatedRows}</Tag>
                  </Space>

                  <div style={{
                    background: '#f6f8fa',
                    padding: '8px',
                    borderRadius: '4px',
                    fontSize: '12px',
                    fontFamily: 'monospace',
                    color: '#586069'
                  }}>
                    {template.template}
                  </div>
                </Space>
              </Card>
            </Col>
          ))
        ) : (
          <Col span={24}>
            <Card>
              <Empty
                description="No templates found"
                style={{ padding: '40px 0' }}
              >
                <Button
                  type="primary"
                  onClick={() => {
                    setSearchTerm('');
                    setSelectedCategory('all');
                  }}
                  style={{
                    background: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)',
                    border: 'none',
                    borderRadius: '8px'
                  }}
                >
                  Clear Filters
                </Button>
              </Empty>
            </Card>
          </Col>
        )}
      </Row>

      {/* Quick Actions */}
      <Card
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
            onClick={() => navigate('/history')}
            style={{
              borderRadius: '8px',
              border: '1px solid #52c41a',
              color: '#52c41a'
            }}
          >
            View History
          </Button>
          <Button
            onClick={() => navigate('/suggestions')}
            style={{
              borderRadius: '8px',
              border: '1px solid #722ed1',
              color: '#722ed1'
            }}
          >
            Smart Suggestions
          </Button>
        </Space>
      </Card>
    </div>
  );
};

export const TemplatesPage: React.FC = () => {
  return (
    <QueryProvider>
      <TemplatesPageContent />
    </QueryProvider>
  );
};

// Default export for lazy loading
export default TemplatesPage;
