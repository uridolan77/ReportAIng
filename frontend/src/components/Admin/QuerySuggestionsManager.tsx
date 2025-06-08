/**
 * Query Suggestions Manager Component
 * Provides admin interface for managing query suggestions and categories
 */

import React, { useState, useEffect } from 'react';
import {
  Card,
  Tabs,
  Typography,
  Row,
  Col,
  Statistic,
  Space,
  Button,
  Spin,
  Badge,
  message
} from 'antd';
import {
  BulbOutlined,
  AppstoreOutlined,
  FileTextOutlined,
  BarChartOutlined,
  PlusOutlined,
  ReloadOutlined,
  SyncOutlined
} from '@ant-design/icons';
import { CategoriesManager } from './QuerySuggestions/CategoriesManager';
import { SuggestionsManager } from './QuerySuggestions/SuggestionsManager';
import { SuggestionAnalytics } from './QuerySuggestions/SuggestionAnalytics';
import { SuggestionSyncUtility } from './QuerySuggestions/SuggestionSyncUtility';
import { querySuggestionService, SuggestionCategory, QuerySuggestion } from '../../services/querySuggestionService';
import '../../styles/enhanced-ui.css';

const { Title, Text } = Typography;
const { TabPane } = Tabs;

interface SuggestionStats {
  totalCategories: number;
  totalSuggestions: number;
  activeSuggestions: number;
  totalUsage: number;
  popularCategories: string[];
  recentlyUsed: string[];
}

export const QuerySuggestionsManager: React.FC = () => {
  const [activeTab, setActiveTab] = useState('overview');
  const [loading, setLoading] = useState(true);
  const [stats, setStats] = useState<SuggestionStats>({
    totalCategories: 0,
    totalSuggestions: 0,
    activeSuggestions: 0,
    totalUsage: 0,
    popularCategories: [],
    recentlyUsed: []
  });
  const [categories, setCategories] = useState<SuggestionCategory[]>([]);
  const [suggestions, setSuggestions] = useState<QuerySuggestion[]>([]);

  // Load initial data
  useEffect(() => {
    loadData();
  }, []);

  const loadData = async () => {
    try {
      setLoading(true);
      
      // Load categories and suggestions
      const [categoriesData, suggestionsData] = await Promise.all([
        querySuggestionService.getCategories(true), // Include inactive
        querySuggestionService.searchSuggestions({ take: 1000 }) // Get all suggestions
      ]);

      setCategories(categoriesData);
      setSuggestions(suggestionsData.suggestions);

      // Calculate stats
      const activeSuggestions = suggestionsData.suggestions.filter(s => s.isActive).length;
      const totalUsage = suggestionsData.suggestions.reduce((sum, s) => sum + s.usageCount, 0);
      
      // Get popular categories (top 5 by suggestion count)
      const categoryUsage = categoriesData.map(cat => ({
        title: cat.title,
        count: cat.suggestionCount
      })).sort((a, b) => b.count - a.count).slice(0, 5);

      // Get recently used suggestions (top 5 by last used)
      const recentSuggestions = suggestionsData.suggestions
        .filter(s => s.lastUsed)
        .sort((a, b) => new Date(b.lastUsed!).getTime() - new Date(a.lastUsed!).getTime())
        .slice(0, 5)
        .map(s => s.description);

      setStats({
        totalCategories: categoriesData.length,
        totalSuggestions: suggestionsData.suggestions.length,
        activeSuggestions,
        totalUsage,
        popularCategories: categoryUsage.map(c => c.title),
        recentlyUsed: recentSuggestions
      });

    } catch (error) {
      console.error('Error loading suggestion data:', error);
      message.error('Failed to load suggestion data');
    } finally {
      setLoading(false);
    }
  };

  const handleRefresh = () => {
    loadData();
  };

  const renderOverview = () => (
    <div style={{ padding: '24px' }}>
      {/* Stats Overview */}
      <Row gutter={[24, 24]} style={{ marginBottom: '32px' }}>
        <Col xs={24} sm={12} md={6}>
          <Card>
            <Statistic
              title="Total Categories"
              value={stats.totalCategories}
              prefix={<AppstoreOutlined />}
              valueStyle={{ color: '#1890ff' }}
            />
          </Card>
        </Col>
        <Col xs={24} sm={12} md={6}>
          <Card>
            <Statistic
              title="Total Suggestions"
              value={stats.totalSuggestions}
              prefix={<FileTextOutlined />}
              valueStyle={{ color: '#52c41a' }}
            />
          </Card>
        </Col>
        <Col xs={24} sm={12} md={6}>
          <Card>
            <Statistic
              title="Active Suggestions"
              value={stats.activeSuggestions}
              prefix={<BulbOutlined />}
              valueStyle={{ color: '#faad14' }}
            />
          </Card>
        </Col>
        <Col xs={24} sm={12} md={6}>
          <Card>
            <Statistic
              title="Total Usage"
              value={stats.totalUsage}
              prefix={<BarChartOutlined />}
              valueStyle={{ color: '#722ed1' }}
            />
          </Card>
        </Col>
      </Row>

      {/* Quick Actions */}
      <Card
        title="Quick Actions"
        style={{ marginBottom: '24px' }}
        extra={
          <Button
            icon={<ReloadOutlined />}
            onClick={handleRefresh}
            loading={loading}
          >
            Refresh
          </Button>
        }
      >
        <Space wrap>
          <Button
            type="primary"
            icon={<PlusOutlined />}
            onClick={() => setActiveTab('categories')}
          >
            Add Category
          </Button>
          <Button
            icon={<PlusOutlined />}
            onClick={() => setActiveTab('suggestions')}
          >
            Add Suggestion
          </Button>
          <Button
            icon={<BarChartOutlined />}
            onClick={() => setActiveTab('analytics')}
          >
            View Analytics
          </Button>
          <Button
            icon={<SyncOutlined />}
            onClick={() => setActiveTab('sync')}
          >
            Sync Database
          </Button>
        </Space>
      </Card>

      {/* Popular Categories */}
      <Row gutter={[24, 24]}>
        <Col xs={24} lg={12}>
          <Card title="Popular Categories" size="small">
            <Space direction="vertical" style={{ width: '100%' }}>
              {stats.popularCategories.map((category, index) => (
                <div key={category} style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                  <Text>{category}</Text>
                  <Badge count={index + 1} style={{ backgroundColor: '#52c41a' }} />
                </div>
              ))}
              {stats.popularCategories.length === 0 && (
                <Text type="secondary">No categories available</Text>
              )}
            </Space>
          </Card>
        </Col>
        <Col xs={24} lg={12}>
          <Card title="Recently Used Suggestions" size="small">
            <Space direction="vertical" style={{ width: '100%' }}>
              {stats.recentlyUsed.map((suggestion, index) => (
                <div key={index} style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                  <Text ellipsis style={{ maxWidth: '200px' }}>{suggestion}</Text>
                  <Text type="secondary" style={{ fontSize: '12px' }}>#{index + 1}</Text>
                </div>
              ))}
              {stats.recentlyUsed.length === 0 && (
                <Text type="secondary">No recent usage data</Text>
              )}
            </Space>
          </Card>
        </Col>
      </Row>
    </div>
  );

  if (loading && activeTab === 'overview') {
    return (
      <div style={{ padding: '24px', textAlign: 'center' }}>
        <Spin size="large" />
        <div style={{ marginTop: '16px' }}>
          <Text type="secondary">Loading suggestion management data...</Text>
        </div>
      </div>
    );
  }

  return (
    <div style={{
      padding: '24px',
      background: '#f5f5f5',
      minHeight: '100vh'
    }}>
      <div style={{ marginBottom: '24px' }}>
        <Title level={3}>
          <BulbOutlined /> Query Suggestions Management
        </Title>
        <Text type="secondary">
          Manage AI query suggestions, categories, and analyze usage patterns
        </Text>
      </div>

      <Tabs
        activeKey={activeTab}
        onChange={setActiveTab}
        type="card"
        size="large"
        className="suggestions-management-tabs"
        style={{
          background: 'white',
          borderRadius: '12px',
          padding: '16px',
          boxShadow: '0 4px 16px rgba(0, 0, 0, 0.06)'
        }}
      >
        <TabPane
          tab={
            <Space>
              <BarChartOutlined />
              <span>Overview</span>
              <Badge count={stats.totalSuggestions} style={{ backgroundColor: '#52c41a' }} />
            </Space>
          }
          key="overview"
        >
          {renderOverview()}
        </TabPane>

        <TabPane
          tab={
            <Space>
              <AppstoreOutlined />
              <span>Categories</span>
              <Badge count={stats.totalCategories} style={{ backgroundColor: '#1890ff' }} />
            </Space>
          }
          key="categories"
        >
          <CategoriesManager
            categories={categories}
            onCategoriesChange={setCategories}
            onRefresh={loadData}
          />
        </TabPane>

        <TabPane
          tab={
            <Space>
              <FileTextOutlined />
              <span>Suggestions</span>
              <Badge count={stats.activeSuggestions} style={{ backgroundColor: '#faad14' }} />
            </Space>
          }
          key="suggestions"
        >
          <SuggestionsManager
            suggestions={suggestions}
            categories={categories}
            onSuggestionsChange={setSuggestions}
            onRefresh={loadData}
          />
        </TabPane>

        <TabPane
          tab={
            <Space>
              <BarChartOutlined />
              <span>Analytics</span>
            </Space>
          }
          key="analytics"
        >
          <SuggestionAnalytics
            suggestions={suggestions}
            categories={categories}
            stats={stats}
          />
        </TabPane>

        <TabPane
          tab={
            <Space>
              <SyncOutlined />
              <span>Sync Database</span>
            </Space>
          }
          key="sync"
        >
          <SuggestionSyncUtility onSyncComplete={loadData} />
        </TabPane>
      </Tabs>
    </div>
  );
};
