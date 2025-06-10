/**
 * Suggestion Analytics Component
 * Displays analytics and insights for query suggestions
 */

import React, { useMemo } from 'react';
import {
  Card,
  Row,
  Col,
  Statistic,
  Progress,
  List,
  Tag,
  Space,
  Typography,
  Empty,
  Tooltip
} from 'antd';
import {
  BarChartOutlined,
  TrophyOutlined,
  ClockCircleOutlined,
  RiseOutlined,
  FireOutlined,
  EyeOutlined
} from '@ant-design/icons';
import { QuerySuggestion, SuggestionCategory } from '../../services/querySuggestionService';

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

interface CategoryAnalytics {
  categoryTitle: string;
  categoryKey: string;
  totalSuggestions: number;
  activeSuggestions: number;
  totalUsage: number;
  averageUsage: number;
  topSuggestion: QuerySuggestion | null;
}

interface SuggestionAnalyticsItem {
  suggestion: QuerySuggestion;
  usageRank: number;
  categoryRank: number;
}

export const SuggestionAnalytics: React.FC<SuggestionAnalyticsProps> = ({
  suggestions,
  categories,
  stats
}) => {
  // Calculate category analytics
  const categoryAnalytics = useMemo((): CategoryAnalytics[] => {
    return categories.map(category => {
      const categorySuggestions = suggestions.filter(s => s.categoryKey === category.categoryKey);
      const activeSuggestions = categorySuggestions.filter(s => s.isActive);
      const totalUsage = categorySuggestions.reduce((sum, s) => sum + s.usageCount, 0);
      const topSuggestion = categorySuggestions.sort((a, b) => b.usageCount - a.usageCount)[0] || null;

      return {
        categoryTitle: category.title,
        categoryKey: category.categoryKey,
        totalSuggestions: categorySuggestions.length,
        activeSuggestions: activeSuggestions.length,
        totalUsage,
        averageUsage: categorySuggestions.length > 0 ? totalUsage / categorySuggestions.length : 0,
        topSuggestion
      };
    }).sort((a, b) => b.totalUsage - a.totalUsage);
  }, [suggestions, categories]);

  // Calculate top performing suggestions
  const topSuggestions = useMemo((): SuggestionAnalyticsItem[] => {
    const sortedSuggestions = [...suggestions].sort((a, b) => b.usageCount - a.usageCount);
    
    return sortedSuggestions.slice(0, 10).map((suggestion, index) => {
      const categoryRank = suggestions
        .filter(s => s.categoryKey === suggestion.categoryKey)
        .sort((a, b) => b.usageCount - a.usageCount)
        .findIndex(s => s.id === suggestion.id) + 1;

      return {
        suggestion,
        usageRank: index + 1,
        categoryRank
      };
    });
  }, [suggestions]);

  // Calculate usage trends
  const usageDistribution = useMemo(() => {
    const totalUsage = suggestions.reduce((sum, s) => sum + s.usageCount, 0);
    const usedSuggestions = suggestions.filter(s => s.usageCount > 0);
    const unusedSuggestions = suggestions.filter(s => s.usageCount === 0);

    return {
      totalUsage,
      usedCount: usedSuggestions.length,
      unusedCount: unusedSuggestions.length,
      usageRate: suggestions.length > 0 ? (usedSuggestions.length / suggestions.length) * 100 : 0,
      averageUsage: suggestions.length > 0 ? totalUsage / suggestions.length : 0
    };
  }, [suggestions]);

  // Calculate complexity distribution
  const complexityDistribution = useMemo(() => {
    const distribution = suggestions.reduce((acc, s) => {
      acc[s.complexity] = (acc[s.complexity] || 0) + 1;
      return acc;
    }, {} as Record<number, number>);

    const complexityLabels = {
      1: 'Simple',
      2: 'Medium', 
      3: 'Complex',
      4: 'Advanced',
      5: 'Expert'
    };

    return Object.entries(distribution).map(([complexity, count]) => ({
      complexity: parseInt(complexity),
      label: complexityLabels[parseInt(complexity) as keyof typeof complexityLabels] || `Level ${complexity}`,
      count: count as number,
      percentage: (count / suggestions.length) * 100
    })).sort((a, b) => a.complexity - b.complexity);
  }, [suggestions]);

  return (
    <div style={{ padding: '24px' }}>
      {/* Overview Stats */}
      <Row gutter={[24, 24]} style={{ marginBottom: '32px' }}>
        <Col xs={24} sm={12} md={6}>
          <Card>
            <Statistic
              title="Usage Rate"
              value={usageDistribution.usageRate}
              precision={1}
              suffix="%"
              prefix={<EyeOutlined />}
              valueStyle={{ color: usageDistribution.usageRate > 50 ? '#52c41a' : '#faad14' }}
            />
            <Progress 
              percent={usageDistribution.usageRate} 
              strokeColor={usageDistribution.usageRate > 50 ? '#52c41a' : '#faad14'}
              size="small"
              style={{ marginTop: '8px' }}
            />
          </Card>
        </Col>
        <Col xs={24} sm={12} md={6}>
          <Card>
            <Statistic
              title="Average Usage"
              value={usageDistribution.averageUsage}
              precision={1}
              prefix={<BarChartOutlined />}
              valueStyle={{ color: '#1890ff' }}
            />
          </Card>
        </Col>
        <Col xs={24} sm={12} md={6}>
          <Card>
            <Statistic
              title="Used Suggestions"
              value={usageDistribution.usedCount}
              suffix={`/ ${suggestions.length}`}
              prefix={<FireOutlined />}
              valueStyle={{ color: '#52c41a' }}
            />
          </Card>
        </Col>
        <Col xs={24} sm={12} md={6}>
          <Card>
            <Statistic
              title="Unused Suggestions"
              value={usageDistribution.unusedCount}
              prefix={<ClockCircleOutlined />}
              valueStyle={{ color: '#ff4d4f' }}
            />
          </Card>
        </Col>
      </Row>

      <Row gutter={[24, 24]}>
        {/* Category Performance */}
        <Col xs={24} lg={12}>
          <Card
            title={
              <Space>
                <TrophyOutlined />
                <span>Category Performance</span>
              </Space>
            }
          >
            {categoryAnalytics.length > 0 ? (
              <List
                dataSource={categoryAnalytics}
                renderItem={(item, index) => (
                  <List.Item>
                    <List.Item.Meta
                      avatar={
                        <div style={{
                          width: '32px',
                          height: '32px',
                          borderRadius: '50%',
                          background: index < 3 ? '#52c41a' : '#1890ff',
                          color: 'white',
                          display: 'flex',
                          alignItems: 'center',
                          justifyContent: 'center',
                          fontSize: '12px',
                          fontWeight: 'bold'
                        }}>
                          #{index + 1}
                        </div>
                      }
                      title={
                        <Space>
                          <span>{item.categoryTitle}</span>
                          {index < 3 && <TrophyOutlined style={{ color: '#faad14' }} />}
                        </Space>
                      }
                      description={
                        <Space direction="vertical" size="small">
                          <Space wrap>
                            <Tag color="blue">{item.totalSuggestions} suggestions</Tag>
                            <Tag color="green">{item.activeSuggestions} active</Tag>
                            <Tag color="orange">{item.totalUsage} uses</Tag>
                          </Space>
                          {item.topSuggestion && (
                            <Text type="secondary" style={{ fontSize: '12px' }}>
                              Top: {item.topSuggestion.description}
                            </Text>
                          )}
                        </Space>
                      }
                    />
                    <div style={{ textAlign: 'right' }}>
                      <Statistic
                        value={item.averageUsage}
                        precision={1}
                        suffix="avg"
                        valueStyle={{ fontSize: '14px' }}
                      />
                    </div>
                  </List.Item>
                )}
              />
            ) : (
              <Empty description="No category data available" />
            )}
          </Card>
        </Col>

        {/* Top Performing Suggestions */}
        <Col xs={24} lg={12}>
          <Card
            title={
              <Space>
                <RiseOutlined />
                <span>Top Performing Suggestions</span>
              </Space>
            }
          >
            {topSuggestions.length > 0 ? (
              <List
                dataSource={topSuggestions}
                renderItem={(item) => (
                  <List.Item>
                    <List.Item.Meta
                      avatar={
                        <div style={{
                          width: '32px',
                          height: '32px',
                          borderRadius: '50%',
                          background: item.usageRank <= 3 ? '#52c41a' : '#1890ff',
                          color: 'white',
                          display: 'flex',
                          alignItems: 'center',
                          justifyContent: 'center',
                          fontSize: '12px',
                          fontWeight: 'bold'
                        }}>
                          #{item.usageRank}
                        </div>
                      }
                      title={
                        <Space>
                          <span>{item.suggestion.description}</span>
                          {item.usageRank <= 3 && <FireOutlined style={{ color: '#ff4d4f' }} />}
                        </Space>
                      }
                      description={
                        <Space direction="vertical" size="small">
                          <Space wrap>
                            <Tag color="blue">{item.suggestion.categoryTitle}</Tag>
                            <Tag color="green">{item.suggestion.usageCount} uses</Tag>
                            <Tag color="purple">#{item.categoryRank} in category</Tag>
                          </Space>
                          <Text 
                            type="secondary" 
                            style={{ 
                              fontSize: '11px',
                              fontFamily: 'monospace',
                              maxWidth: '250px',
                              display: 'block',
                              overflow: 'hidden',
                              textOverflow: 'ellipsis',
                              whiteSpace: 'nowrap'
                            }}
                          >
                            {item.suggestion.queryText}
                          </Text>
                        </Space>
                      }
                    />
                  </List.Item>
                )}
              />
            ) : (
              <Empty description="No usage data available" />
            )}
          </Card>
        </Col>

        {/* Complexity Distribution */}
        <Col xs={24} lg={12}>
          <Card
            title={
              <Space>
                <BarChartOutlined />
                <span>Complexity Distribution</span>
              </Space>
            }
          >
            {complexityDistribution.length > 0 ? (
              <Space direction="vertical" style={{ width: '100%' }}>
                {complexityDistribution.map((item) => (
                  <div key={item.complexity}>
                    <div style={{ display: 'flex', justifyContent: 'space-between', marginBottom: '4px' }}>
                      <Text>{item.label}</Text>
                      <Text type="secondary">{`${item.count} (${item.percentage.toFixed(1)}%)`}</Text>
                    </div>
                    <Progress
                      percent={item.percentage}
                      strokeColor={
                        item.complexity <= 2 ? '#52c41a' :
                        item.complexity <= 3 ? '#faad14' :
                        '#ff4d4f'
                      }
                      size="small"
                    />
                  </div>
                ))}
              </Space>
            ) : (
              <Empty description="No complexity data available" />
            )}
          </Card>
        </Col>

        {/* Recent Activity */}
        <Col xs={24} lg={12}>
          <Card
            title={
              <Space>
                <ClockCircleOutlined />
                <span>Recent Activity</span>
              </Space>
            }
          >
            {suggestions.filter(s => s.lastUsed).length > 0 ? (
              <List
                dataSource={suggestions
                  .filter(s => s.lastUsed)
                  .sort((a, b) => new Date(b.lastUsed!).getTime() - new Date(a.lastUsed!).getTime())
                  .slice(0, 8)
                }
                renderItem={(suggestion) => (
                  <List.Item>
                    <List.Item.Meta
                      title={suggestion.description}
                      description={
                        <Space>
                          <Tag color="blue">{suggestion.categoryTitle}</Tag>
                          <Text type="secondary" style={{ fontSize: '12px' }}>
                            {new Date(suggestion.lastUsed!).toLocaleDateString()}
                          </Text>
                        </Space>
                      }
                    />
                    <Tooltip title={`Used ${suggestion.usageCount} times`}>
                      <Tag color="green">{suggestion.usageCount}</Tag>
                    </Tooltip>
                  </List.Item>
                )}
              />
            ) : (
              <Empty description="No recent activity" />
            )}
          </Card>
        </Col>
      </Row>
    </div>
  );
};
