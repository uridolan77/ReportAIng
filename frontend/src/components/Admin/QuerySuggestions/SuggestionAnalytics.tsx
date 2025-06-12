/**
 * Suggestion Analytics Component
 * Provides analytics and insights for query suggestions
 */

import React, { useMemo } from 'react';
import {
  Card,
  Row,
  Col,
  Statistic,
  Progress,
  Table,
  Tag,
  Space,
  Typography,
  List,
  Avatar
} from 'antd';
import {
  BarChartOutlined,
  TrophyOutlined,
  ClockCircleOutlined,
  UserOutlined,
  FireOutlined,
  RiseOutlined
} from '@ant-design/icons';
import { QuerySuggestion, SuggestionCategory } from '../../../services/querySuggestionService';

const { Text } = Typography;

interface SuggestionAnalyticsProps {
  suggestions: QuerySuggestion[];
  categories: SuggestionCategory[];
  stats: {
    totalCategories: number;
    totalSuggestions: number;
    activeSuggestions: number;
    totalUsage: number;
    popularCategories: string[];
    recentlyUsed: string[];
  };
}

export const SuggestionAnalytics: React.FC<SuggestionAnalyticsProps> = ({
  suggestions,
  categories,
  stats
}) => {
  // Calculate analytics data
  const analytics = useMemo(() => {
    // Most used suggestions
    const mostUsed = suggestions
      .filter(s => s.usageCount > 0)
      .sort((a, b) => b.usageCount - a.usageCount)
      .slice(0, 10);

    // Category usage distribution
    const categoryUsage = categories.map(cat => {
      const catSuggestions = suggestions.filter(s => s.categoryId === cat.id);
      const totalUsage = catSuggestions.reduce((sum, s) => sum + s.usageCount, 0);
      return {
        category: cat.title,
        suggestionCount: catSuggestions.length,
        totalUsage,
        averageUsage: catSuggestions.length > 0 ? totalUsage / catSuggestions.length : 0
      };
    }).sort((a, b) => b.totalUsage - a.totalUsage);

    // Complexity distribution
    const complexityDist = suggestions.reduce((acc, s) => {
      acc[s.complexity] = (acc[s.complexity] || 0) + 1;
      return acc;
    }, {} as Record<number, number>);

    // Recent activity (last 30 days)
    const thirtyDaysAgo = new Date();
    thirtyDaysAgo.setDate(thirtyDaysAgo.getDate() - 30);
    
    const recentActivity = suggestions.filter(s => 
      s.lastUsed && new Date(s.lastUsed) > thirtyDaysAgo
    ).length;

    // Unused suggestions
    const unusedSuggestions = suggestions.filter(s => s.usageCount === 0);

    return {
      mostUsed,
      categoryUsage,
      complexityDist,
      recentActivity,
      unusedSuggestions
    };
  }, [suggestions, categories]);

  const complexityLabels = {
    1: 'Simple',
    2: 'Medium', 
    3: 'Complex',
    4: 'Advanced',
    5: 'Expert'
  };

  const complexityColors = {
    1: '#52c41a',
    2: '#1890ff',
    3: '#faad14',
    4: '#f5222d',
    5: '#722ed1'
  };

  const mostUsedColumns = [
    {
      title: 'Rank',
      dataIndex: 'rank',
      key: 'rank',
      width: 60,
      render: (_: any, __: any, index: number) => (
        <Avatar
          size="small"
          style={{
            backgroundColor: index < 3 ? '#faad14' : '#d9d9d9',
            color: index < 3 ? '#fff' : '#666'
          }}
        >
          {index + 1}
        </Avatar>
      )
    },
    {
      title: 'Suggestion',
      dataIndex: 'description',
      key: 'description',
      ellipsis: true
    },
    {
      title: 'Category',
      dataIndex: 'categoryTitle',
      key: 'categoryTitle',
      width: 120,
      render: (categoryTitle: string) => (
        <Tag color="blue">{categoryTitle}</Tag>
      )
    },
    {
      title: 'Usage Count',
      dataIndex: 'usageCount',
      key: 'usageCount',
      width: 100,
      render: (count: number) => (
        <Statistic
          value={count}
          prefix={<BarChartOutlined />}
          valueStyle={{ fontSize: '14px' }}
        />
      )
    },
    {
      title: 'Last Used',
      dataIndex: 'lastUsed',
      key: 'lastUsed',
      width: 120,
      render: (lastUsed: string) => lastUsed ? (
        <Text type="secondary">
          {new Date(lastUsed).toLocaleDateString()}
        </Text>
      ) : (
        <Text type="secondary">Never</Text>
      )
    }
  ];

  return (
    <div style={{ padding: '24px' }}>
      {/* Key Metrics */}
      <Row gutter={[24, 24]} style={{ marginBottom: '32px' }}>
        <Col xs={24} sm={12} md={6}>
          <Card>
            <Statistic
              title="Usage Rate"
              value={stats.totalSuggestions > 0 ? 
                ((stats.totalSuggestions - analytics.unusedSuggestions.length) / stats.totalSuggestions * 100) : 0
              }
              precision={1}
              suffix="%"
              prefix={<RiseOutlined />}
              valueStyle={{ color: '#52c41a' }}
            />
          </Card>
        </Col>
        <Col xs={24} sm={12} md={6}>
          <Card>
            <Statistic
              title="Avg Usage per Suggestion"
              value={stats.totalSuggestions > 0 ? stats.totalUsage / stats.totalSuggestions : 0}
              precision={1}
              prefix={<BarChartOutlined />}
              valueStyle={{ color: '#1890ff' }}
            />
          </Card>
        </Col>
        <Col xs={24} sm={12} md={6}>
          <Card>
            <Statistic
              title="Recent Activity (30d)"
              value={analytics.recentActivity}
              prefix={<ClockCircleOutlined />}
              valueStyle={{ color: '#faad14' }}
            />
          </Card>
        </Col>
        <Col xs={24} sm={12} md={6}>
          <Card>
            <Statistic
              title="Unused Suggestions"
              value={analytics.unusedSuggestions.length}
              prefix={<UserOutlined />}
              valueStyle={{ color: '#f5222d' }}
            />
          </Card>
        </Col>
      </Row>

      {/* Charts and Analysis */}
      <Row gutter={[24, 24]}>
        {/* Most Used Suggestions */}
        <Col xs={24} lg={16}>
          <Card
            title={
              <Space>
                <TrophyOutlined />
                <span>Most Used Suggestions</span>
              </Space>
            }
          >
            <Table
              dataSource={analytics.mostUsed}
              columns={mostUsedColumns}
              pagination={false}
              size="small"
              rowKey="id"
            />
          </Card>
        </Col>

        {/* Complexity Distribution */}
        <Col xs={24} lg={8}>
          <Card
            title={
              <Space>
                <BarChartOutlined />
                <span>Complexity Distribution</span>
              </Space>
            }
          >
            <Space direction="vertical" style={{ width: '100%' }}>
              {Object.entries(analytics.complexityDist).map(([complexity, count]) => (
                <div key={complexity}>
                  <div style={{ display: 'flex', justifyContent: 'space-between', marginBottom: '4px' }}>
                    <Text>{complexityLabels[parseInt(complexity) as keyof typeof complexityLabels]}</Text>
                    <Text strong>{count}</Text>
                  </div>
                  <Progress
                    percent={stats.totalSuggestions > 0 ? (count / stats.totalSuggestions) * 100 : 0}
                    strokeColor={complexityColors[parseInt(complexity) as keyof typeof complexityColors]}
                    showInfo={false}
                    size="small"
                  />
                </div>
              ))}
            </Space>
          </Card>
        </Col>
      </Row>

      {/* Category Performance */}
      <Row gutter={[24, 24]} style={{ marginTop: '24px' }}>
        <Col xs={24}>
          <Card
            title={
              <Space>
                <FireOutlined />
                <span>Category Performance</span>
              </Space>
            }
          >
            <List
              dataSource={analytics.categoryUsage}
              renderItem={(item, index) => (
                <List.Item>
                  <List.Item.Meta
                    avatar={
                      <Avatar
                        style={{
                          backgroundColor: index < 3 ? '#52c41a' : '#d9d9d9',
                          color: '#fff'
                        }}
                      >
                        {index + 1}
                      </Avatar>
                    }
                    title={item.category}
                    description={`${item.suggestionCount} suggestions • ${item.totalUsage} total uses • ${item.averageUsage.toFixed(1)} avg per suggestion`}
                  />
                  <div>
                    <Progress
                      type="circle"
                      size={60}
                      percent={stats.totalUsage > 0 ? (item.totalUsage / stats.totalUsage) * 100 : 0}
                      format={() => item.totalUsage}
                    />
                  </div>
                </List.Item>
              )}
            />
          </Card>
        </Col>
      </Row>
    </div>
  );
};

export default SuggestionAnalytics;
