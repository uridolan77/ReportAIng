/**
 * Templates Page - Browse and use query templates
 */

import React, { useState } from 'react';
import {
  Input,
  Select,
  Tag,
  Empty,
  Badge,
  Row,
  Col,
  Typography
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
import { ModernPageLayout, PageSection, PageGrid } from '../components/core/Layouts';
import { Card, CardContent } from '../components/core/Card';
import { Button } from '../components/core/Button';
import { Breadcrumb } from '../components/core/Navigation';

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
    <ModernPageLayout
      title="Query Templates"
      subtitle={`Pre-built queries to help you get started quickly (${templates.length} available)`}
      breadcrumb={
        <Breadcrumb
          items={[
            { title: 'Home', path: '/', icon: <HomeOutlined /> },
            { title: 'Query Templates', icon: <BookOutlined /> }
          ]}
        />
      }
      className="full-width-content"
    >
      {/* Search and Filters */}
      <PageSection
        title="Search & Filter"
        subtitle="Find the perfect template for your needs"
        background="card"
        padding="lg"
      >
        <Row gutter={[16, 16]} align="middle">
          <Col xs={24} md={12}>
            <Input
              placeholder="Search templates..."
              prefix={<SearchOutlined />}
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
              allowClear
              style={{
                borderRadius: 'var(--radius-md)',
                border: '1px solid var(--border-primary)'
              }}
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
                <div style={{ display: 'flex', alignItems: 'center', gap: 'var(--space-2)' }}>
                  <StarFilled style={{ color: 'var(--color-warning)' }} />
                  Favorites
                </div>
              </Option>
              {categories.map(category => (
                <Option key={category} value={category}>
                  {category}
                </Option>
              ))}
            </Select>
          </Col>
          <Col xs={24} md={4}>
            <Text type="secondary" style={{ color: 'var(--text-secondary)' }}>
              {filteredTemplates.length} templates
            </Text>
          </Col>
        </Row>
      </PageSection>

      {/* Popular Templates */}
      {popularTemplates.length > 0 && selectedCategory === 'all' && !searchTerm && (
        <PageSection
          title="Popular Templates"
          subtitle="Most frequently used templates"
          background="card"
          padding="lg"
          actions={<FireOutlined style={{ color: 'var(--color-error)' }} />}
        >
          <PageGrid columns={3} gap="md">
            {popularTemplates.slice(0, 3).map((template, index) => (
              <Card
                key={template.id}
                variant="outlined"
                hover
                style={{ cursor: 'pointer' }}
                onClick={() => handleTemplateSelect(template)}
              >
                <CardContent padding="medium">
                  <div style={{
                    display: 'flex',
                    flexDirection: 'column',
                    gap: 'var(--space-3)'
                  }}>
                    <div style={{
                      display: 'flex',
                      alignItems: 'center',
                      gap: 'var(--space-2)'
                    }}>
                      <Text strong style={{ color: 'var(--text-primary)' }}>
                        {template.name}
                      </Text>
                      <Badge count={template.usageCount} />
                    </div>
                    <Text
                      type="secondary"
                      style={{
                        fontSize: 'var(--text-sm)',
                        color: 'var(--text-secondary)'
                      }}
                    >
                      {template.description}
                    </Text>
                  </div>
                </CardContent>
              </Card>
            ))}
          </PageGrid>
        </PageSection>
      )}

      {/* Templates Grid */}
      <PageSection
        title="All Templates"
        subtitle="Browse all available query templates"
        background="transparent"
        padding="none"
      >
        {filteredTemplates.length > 0 ? (
          <PageGrid columns={2} gap="lg">
            {filteredTemplates.map((template, index) => (
              <Card
                key={template.id}
                variant="outlined"
                hover
                style={{ cursor: 'pointer', height: '100%' }}
                onClick={() => handleTemplateSelect(template)}
              >
                <CardContent padding="large">
                  <div style={{
                    display: 'flex',
                    flexDirection: 'column',
                    gap: 'var(--space-4)',
                    height: '100%'
                  }}>
                    <div style={{
                      display: 'flex',
                      alignItems: 'center',
                      gap: 'var(--space-2)',
                      justifyContent: 'space-between'
                    }}>
                      <div style={{ display: 'flex', alignItems: 'center', gap: 'var(--space-2)' }}>
                        <Text strong style={{ fontSize: 'var(--text-lg)', color: 'var(--text-primary)' }}>
                          {template.name}
                        </Text>
                        <Tag color={getDifficultyColor(template.difficulty)}>
                          {template.difficulty}
                        </Tag>
                      </div>
                      {template.isFavorite ? (
                        <StarFilled style={{ color: 'var(--color-warning)' }} />
                      ) : (
                        <StarOutlined style={{ color: 'var(--text-tertiary)' }} />
                      )}
                    </div>

                    <Text
                      type="secondary"
                      style={{
                        fontSize: 'var(--text-base)',
                        color: 'var(--text-secondary)'
                      }}
                    >
                      {template.description}
                    </Text>

                    <div style={{ display: 'flex', gap: 'var(--space-2)', flexWrap: 'wrap' }}>
                      <Tag color="blue">{template.category}</Tag>
                      <Tag>{template.estimatedRows}</Tag>
                      <div style={{
                        display: 'flex',
                        alignItems: 'center',
                        gap: 'var(--space-1)',
                        marginLeft: 'auto'
                      }}>
                        <Badge count={template.usageCount} />
                        <Text type="secondary" style={{ fontSize: 'var(--text-sm)' }}>uses</Text>
                      </div>
                    </div>

                    <div style={{
                      background: 'var(--bg-tertiary)',
                      padding: 'var(--space-3)',
                      borderRadius: 'var(--radius-md)',
                      fontSize: 'var(--text-sm)',
                      fontFamily: 'var(--font-family-mono)',
                      color: 'var(--text-secondary)',
                      border: '1px solid var(--border-primary)',
                      marginTop: 'auto'
                    }}>
                      {template.template}
                    </div>
                  </div>
                </CardContent>
              </Card>
            ))}
          </PageGrid>
        ) : (
          <PageSection background="card" padding="lg">
            <div style={{ textAlign: 'center', padding: 'var(--space-16) 0' }}>
              <Empty
                description="No templates found"
              >
                <Button
                  variant="primary"
                  onClick={() => {
                    setSearchTerm('');
                    setSelectedCategory('all');
                  }}
                >
                  Clear Filters
                </Button>
              </Empty>
            </div>
          </PageSection>
        )}
      </PageSection>

      {/* Quick Actions */}
      <PageSection
        title="Quick Actions"
        subtitle="Navigate to related tools and features"
        background="card"
        padding="lg"
      >
        <div style={{
          display: 'flex',
          gap: 'var(--space-4)',
          flexWrap: 'wrap'
        }}>
          <Button
            variant="primary"
            onClick={() => navigate('/')}
          >
            New Query
          </Button>
          <Button
            variant="secondary"
            onClick={() => navigate('/history')}
          >
            View History
          </Button>
          <Button
            variant="outline"
            onClick={() => navigate('/suggestions')}
          >
            Smart Suggestions
          </Button>
        </div>
      </PageSection>
    </ModernPageLayout>
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
