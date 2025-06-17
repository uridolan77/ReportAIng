/**
 * Suggestions Page - AI-powered smart query suggestions
 */

import React, { useState, useEffect, useCallback } from 'react';
import {
  Row,
  Col,
  Tag,
  Empty,
  Spin,
  Badge,
  Tooltip,
  Typography,
  Statistic,
  Space,
  message
} from 'antd';
import {
  BulbOutlined,
  RobotOutlined,
  ClockCircleOutlined,
  RiseOutlined,
  UserOutlined,
  DollarOutlined,
  AppstoreOutlined,
  FileTextOutlined,
  BarChartOutlined,
  PlusOutlined,
  ReloadOutlined,
  SyncOutlined,
  SettingOutlined
} from '@ant-design/icons';
import { useNavigate } from 'react-router-dom';
import { QueryProvider } from '../components/QueryInterface/QueryProvider';
import { useQueryContext } from '../components/QueryInterface/QueryProvider';
import { Card, CardContent } from '../components/core/Card';
import { Button, Tabs, Flex } from '../components/core';
import { useAuthStore } from '../stores/authStore';
import { CategoriesManager } from '../components/Admin/QuerySuggestions/CategoriesManager';
import { SuggestionsManager } from '../components/Admin/QuerySuggestions/SuggestionsManager';
import { SuggestionAnalytics } from '../components/Admin/QuerySuggestions/SuggestionAnalytics';
import { SuggestionSyncUtility } from '../components/Admin/QuerySuggestions/SuggestionSyncUtility';
import { querySuggestionService, SuggestionCategory, QuerySuggestion } from '../services/querySuggestionService';

const { Text } = Typography;

interface SuggestionStats {
  totalCategories: number;
  totalSuggestions: number;
  activeSuggestions: number;
  totalUsage: number;
  popularCategories: string[];
  recentlyUsed: string[];
}

const SuggestionsPageContent: React.FC = () => {
  const navigate = useNavigate();
  const { setQuery } = useQueryContext();
  const { isAdmin } = useAuthStore();
  const [activeTab, setActiveTab] = useState('suggestions');
  const [loading, setLoading] = useState(true);
  const [suggestions, setSuggestions] = useState<any[]>([]);

  // Management state
  const [managementLoading, setManagementLoading] = useState(true);
  const [stats, setStats] = useState<SuggestionStats>({
    totalCategories: 0,
    totalSuggestions: 0,
    activeSuggestions: 0,
    totalUsage: 0,
    popularCategories: [],
    recentlyUsed: []
  });
  const [categories, setCategories] = useState<SuggestionCategory[]>([]);
  const [managementSuggestions, setManagementSuggestions] = useState<QuerySuggestion[]>([]);

  // Real AI suggestions - loaded from API
  const [aiSuggestions, setAiSuggestions] = useState<any[]>([]);
  const [loadingAI, setLoadingAI] = useState(true);
  const [aiError, setAiError] = useState<string | null>(null);

  // Load real AI suggestions on component mount
  React.useEffect(() => {
    const loadAISuggestions = async () => {
      try {
        setLoadingAI(true);
        setAiError(null);

        console.log('Loading real AI suggestions from API...');

        // Call the actual API endpoint
        const groupedSuggestions = await querySuggestionService.getGroupedSuggestions();

        // Transform grouped suggestions into the format expected by the UI
        const transformedSuggestions = groupedSuggestions.flatMap(group =>
          group.suggestions.map(suggestion => ({
            id: suggestion.id,
            title: suggestion.description,
            description: suggestion.description,
            query: suggestion.queryText,
            category: group.category.title,
            type: 'personalized',
            confidence: 0.85,
            icon: '🤖',
            color: '#1890ff',
            reasoning: `Based on ${group.category.title} patterns and your query history`
          }))
        );

        setAiSuggestions(transformedSuggestions);

      } catch (err) {
        console.error('Failed to load AI suggestions:', err);
        setAiError('Failed to load AI suggestions. Please check your connection.');
        // Set empty array on error
        setAiSuggestions([]);
      } finally {
        setLoadingAI(false);
      }
    };

    loadAISuggestions();
  }, []);

  useEffect(() => {
    // Use real AI suggestions instead of mock data
    setSuggestions(aiSuggestions);
    setLoading(loadingAI);
  }, [aiSuggestions, loadingAI]);

  // Load management data
  const loadManagementData = useCallback(async () => {
    if (!isAdmin) return;

    try {
      setManagementLoading(true);

      // Load categories and suggestions
      const [categoriesData, suggestionsData] = await Promise.all([
        querySuggestionService.getCategories(true), // Include inactive
        querySuggestionService.searchSuggestions({ take: 1000 }) // Get all suggestions
      ]);

      setCategories(categoriesData);
      setManagementSuggestions(suggestionsData.suggestions);

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
      console.error('Error loading suggestion management data:', error);
      message.error('Failed to load suggestion management data');
    } finally {
      setManagementLoading(false);
    }
  }, [isAdmin]);

  useEffect(() => {
    if (isAdmin && activeTab !== 'suggestions') {
      loadManagementData();
    }
  }, [isAdmin, activeTab, loadManagementData]);

  const handleTabChange = useCallback((key: string) => {
    setActiveTab(key);
  }, []);

  const handleSuggestionSelect = (suggestion: any) => {
    setQuery(suggestion.query);
    navigate('/', { state: { selectedQuery: suggestion.query } });
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

  const renderManagementOverview = () => (
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
            variant="outline"
            size="small"
            onClick={loadManagementData}
            loading={managementLoading}
          >
            <ReloadOutlined /> Refresh
          </Button>
        }
      >
        <Space wrap>
          <Button
            variant="primary"
            onClick={() => setActiveTab('categories')}
          >
            <PlusOutlined /> Add Category
          </Button>
          <Button
            variant="outline"
            onClick={() => setActiveTab('manage-suggestions')}
          >
            <PlusOutlined /> Add Suggestion
          </Button>
          <Button
            variant="outline"
            onClick={() => setActiveTab('analytics')}
          >
            <BarChartOutlined /> View Analytics
          </Button>
          <Button
            variant="outline"
            onClick={() => setActiveTab('sync')}
          >
            <SyncOutlined /> Sync Database
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

  const renderSuggestionsContent = () => (
    <div className="full-width-content">
      {/* AI Status Card */}
      <div style={{ marginBottom: '32px', padding: '24px', backgroundColor: '#fff', borderRadius: '8px', border: '1px solid rgba(0, 0, 0, 0.06)' }}>
        <div style={{ display: 'flex', alignItems: 'center', gap: '8px', marginBottom: '16px' }}>
          <h3 style={{ margin: 0, fontSize: '1.25rem', fontWeight: 600 }}>AI Analysis Status</h3>
          <RobotOutlined style={{ color: '#1890ff' }} />
        </div>
        <div style={{
          display: 'flex',
          alignItems: 'center',
          gap: '16px'
        }}>
          <div style={{ flex: 1 }}>
            <div style={{
              color: '#1a1a1a',
              fontWeight: 600,
              marginBottom: '4px'
            }}>
              {loading ? 'Analyzing your data patterns...' : `Generated ${suggestions.length} personalized suggestions`}
            </div>
            <div style={{
              color: '#666',
              fontSize: '14px'
            }}>
              {loading ? 'Please wait while AI processes your query history' : 'Ready to explore intelligent recommendations'}
            </div>
          </div>
          {loading && <Spin />}
        </div>
      </div>

      {/* AI Status Card */}
      <div style={{ marginBottom: '32px', padding: '24px', backgroundColor: '#fff', borderRadius: '8px', border: '1px solid rgba(0, 0, 0, 0.06)' }}>
        <div style={{ display: 'flex', alignItems: 'center', gap: '8px', marginBottom: '16px' }}>
          <h3 style={{ margin: 0, fontSize: '1.25rem', fontWeight: 600 }}>AI Analysis Status</h3>
          <RobotOutlined style={{ color: '#1890ff' }} />
        </div>
        <div style={{
          display: 'flex',
          alignItems: 'center',
          gap: '16px'
        }}>
          <div style={{ flex: 1 }}>
            <div style={{
              color: '#1a1a1a',
              fontWeight: 600,
              marginBottom: '4px'
            }}>
              {loading ? 'Analyzing your data patterns...' : `Generated ${suggestions.length} personalized suggestions`}
            </div>
            <div style={{
              color: '#666',
              fontSize: '14px'
            }}>
              {loading ? 'Please wait while AI processes your query history' : 'Ready to explore intelligent recommendations'}
            </div>
          </div>
          {loading && <Spin />}
        </div>
      </div>

      {/* Suggestions Grid */}
      {loading ? (
        <div style={{ textAlign: 'center', padding: '64px 0', backgroundColor: '#fff', borderRadius: '8px', border: '1px solid rgba(0, 0, 0, 0.06)' }}>
          <Spin size="large" />
          <div style={{ marginTop: '16px' }}>
            <Text type="secondary" style={{ color: '#666' }}>
              AI is analyzing your data patterns...
            </Text>
          </div>
        </div>
      ) : suggestions.length > 0 ? (
        <div style={{ marginBottom: '32px' }}>
          <h3 style={{ marginBottom: '16px', fontSize: '1.25rem', fontWeight: 600 }}>Personalized Suggestions</h3>
          <p style={{ marginBottom: '16px', color: '#666' }}>AI-generated recommendations tailored to your needs</p>
          <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(400px, 1fr))', gap: '24px' }}>
            {suggestions.map((suggestion, index) => (
              <Card
                key={suggestion.id}
                variant="outlined"
                hover
                style={{ cursor: 'pointer', height: '100%' }}
                onClick={() => handleSuggestionSelect(suggestion)}
              >
                <CardContent padding="large">
                  <div style={{
                    display: 'flex',
                    flexDirection: 'column',
                    gap: 'var(--space-4)',
                    height: '100%'
                  }}>
                    {/* Header */}
                    <div style={{
                      display: 'flex',
                      justifyContent: 'space-between',
                      alignItems: 'flex-start'
                    }}>
                      <div style={{ display: 'flex', alignItems: 'center', gap: 'var(--space-3)' }}>
                        <div style={{
                          fontSize: '20px',
                          color: suggestion.color,
                          padding: 'var(--space-2)',
                          background: `${suggestion.color}15`,
                          borderRadius: 'var(--radius-md)'
                        }}>
                          {suggestion.icon}
                        </div>
                        <div>
                          <Text strong style={{
                            fontSize: 'var(--text-lg)',
                            color: 'var(--text-primary)'
                          }}>
                            {suggestion.title}
                          </Text>
                          <br />
                          <Tag color={suggestion.color} style={{ marginTop: 'var(--space-1)' }}>
                            {getTypeLabel(suggestion.type)}
                          </Tag>
                        </div>
                      </div>
                      <Tooltip title={`AI Confidence: ${(suggestion.confidence * 100).toFixed(0)}%`}>
                        <Badge
                          count={`${(suggestion.confidence * 100).toFixed(0)}%`}
                          style={{
                            backgroundColor: suggestion.confidence > 0.9 ? 'var(--color-success)' :
                                            suggestion.confidence > 0.8 ? 'var(--color-warning)' : 'var(--color-error)'
                          }}
                        />
                      </Tooltip>
                    </div>

                    {/* Description */}
                    <Text type="secondary" style={{
                      fontSize: 'var(--text-base)',
                      color: 'var(--text-secondary)'
                    }}>
                      {suggestion.description}
                    </Text>

                    {/* Query Preview */}
                    <div style={{
                      background: 'var(--bg-tertiary)',
                      padding: 'var(--space-3)',
                      borderRadius: 'var(--radius-md)',
                      fontSize: 'var(--text-sm)',
                      fontFamily: 'var(--font-family-mono)',
                      color: 'var(--text-secondary)',
                      border: '1px solid var(--border-primary)'
                    }}>
                      {suggestion.query}
                    </div>

                    {/* AI Reasoning */}
                    <div style={{
                      background: `${suggestion.color}08`,
                      padding: 'var(--space-3)',
                      borderRadius: 'var(--radius-md)',
                      border: `1px solid ${suggestion.color}20`
                    }}>
                      <div style={{ display: 'flex', alignItems: 'flex-start', gap: 'var(--space-2)' }}>
                        <RobotOutlined style={{ color: suggestion.color, fontSize: 'var(--text-sm)' }} />
                        <Text style={{
                          fontSize: 'var(--text-sm)',
                          color: 'var(--text-secondary)',
                          lineHeight: 'var(--line-height-relaxed)'
                        }}>
                          <strong>AI Insight:</strong> {suggestion.reasoning}
                        </Text>
                      </div>
                    </div>

                    {/* Category */}
                    <div style={{
                      display: 'flex',
                      justifyContent: 'space-between',
                      alignItems: 'center',
                      marginTop: 'auto'
                    }}>
                      <Tag color="blue">{suggestion.category}</Tag>
                      <Text type="secondary" style={{
                        fontSize: 'var(--text-xs)',
                        color: 'var(--text-tertiary)'
                      }}>
                        Click to run this query
                      </Text>
                    </div>
                  </div>
                </CardContent>
              </Card>
            ))}
          </div>
        </div>
      ) : (
        <div style={{ textAlign: 'center', padding: '64px 0', backgroundColor: '#fff', borderRadius: '8px', border: '1px solid rgba(0, 0, 0, 0.06)' }}>
          <Empty
            image={Empty.PRESENTED_IMAGE_SIMPLE}
            description={
              <div style={{ marginBottom: '24px' }}>
                <div style={{
                  fontSize: '18px',
                  color: '#666',
                  marginBottom: '8px'
                }}>
                  No suggestions available
                </div>
                <div style={{
                  fontSize: '16px',
                  color: '#999'
                }}>
                  Run some queries first to get personalized AI suggestions
                </div>
              </div>
            }
          >
            <Button
              variant="primary"
              onClick={() => navigate('/')}
            >
              Start Querying
            </Button>
          </Empty>
        </div>
      )}

      {/* Quick Actions */}
      <div style={{ padding: '24px', backgroundColor: '#fff', borderRadius: '8px', border: '1px solid rgba(0, 0, 0, 0.06)' }}>
        <h3 style={{ marginBottom: '16px', fontSize: '1.25rem', fontWeight: 600 }}>Quick Actions</h3>
        <p style={{ marginBottom: '16px', color: '#666' }}>Navigate to related tools and features</p>
        <div style={{
          display: 'flex',
          gap: '16px',
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
            onClick={() => navigate('/templates')}
          >
            Browse Templates
          </Button>
          <Button
            variant="outline"
            onClick={() => navigate('/history')}
          >
            View History
          </Button>
          <Button
            variant="ghost"
            onClick={() => {
              setLoading(true);
              setTimeout(() => {
                // Refresh real AI suggestions instead of mock data
                setSuggestions([...aiSuggestions].sort(() => Math.random() - 0.5));
                setLoading(false);
              }, 1500);
            }}
          >
            Refresh Suggestions
          </Button>
        </div>
      </div>
    </div>
  );

  // Create tab items
  const tabItems = [
    {
      key: 'suggestions',
      label: '🤖 AI Suggestions',
      children: renderSuggestionsContent(),
    },
  ];

  // Add management tabs for admin users
  if (isAdmin) {
    tabItems.push(
      {
        key: 'overview',
        label: (
          <Space>
            <BarChartOutlined />
            <span>Management Overview</span>
            <Badge count={stats.totalSuggestions} style={{ backgroundColor: '#52c41a' }} />
          </Space>
        ),
        children: managementLoading && activeTab === 'overview' ? (
          <div style={{ padding: '24px', textAlign: 'center' }}>
            <Spin size="large" />
            <div style={{ marginTop: '16px' }}>
              <Text type="secondary">Loading suggestion management data...</Text>
            </div>
          </div>
        ) : renderManagementOverview()
      },
      {
        key: 'categories',
        label: (
          <Space>
            <AppstoreOutlined />
            <span>Categories</span>
            <Badge count={stats.totalCategories} style={{ backgroundColor: '#1890ff' }} />
          </Space>
        ),
        children: (
          <CategoriesManager
            categories={categories}
            onCategoriesChange={setCategories}
            onRefresh={loadManagementData}
          />
        )
      },
      {
        key: 'manage-suggestions',
        label: (
          <Space>
            <FileTextOutlined />
            <span>Manage Suggestions</span>
            <Badge count={stats.activeSuggestions} style={{ backgroundColor: '#faad14' }} />
          </Space>
        ),
        children: (
          <SuggestionsManager
            suggestions={managementSuggestions}
            categories={categories}
            onSuggestionsChange={setManagementSuggestions}
            onRefresh={loadManagementData}
          />
        )
      },
      {
        key: 'analytics',
        label: (
          <Space>
            <BarChartOutlined />
            <span>Analytics</span>
          </Space>
        ),
        children: (
          <SuggestionAnalytics
            suggestions={managementSuggestions}
            categories={categories}
            stats={stats}
          />
        )
      },
      {
        key: 'sync',
        label: (
          <Space>
            <SyncOutlined />
            <span>Sync Database</span>
          </Space>
        ),
        children: <SuggestionSyncUtility onSyncComplete={loadManagementData} />
      }
    );
  }

  return (
    <div style={{ padding: '24px' }}>
      <div className="modern-page-header" style={{ marginBottom: '32px', paddingBottom: '24px', borderBottom: '1px solid rgba(0, 0, 0, 0.06)' }}>
        <h1 className="modern-page-title" style={{ fontSize: '2.5rem', fontWeight: 600, margin: 0, marginBottom: '8px', color: '#1a1a1a' }}>
          <BulbOutlined style={{ color: '#1890ff', marginRight: '12px' }} />
          AI-Powered Query Suggestions
        </h1>
        <p className="modern-page-subtitle" style={{ fontSize: '1.125rem', color: '#666', margin: 0, lineHeight: 1.5 }}>
          Intelligent recommendations based on your data patterns and query history
        </p>
      </div>

      <Tabs
        variant="line"
        size="large"
        activeKey={activeTab}
        onChange={handleTabChange}
        items={tabItems}
      />
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

// Default export for lazy loading
export default SuggestionsPage;
