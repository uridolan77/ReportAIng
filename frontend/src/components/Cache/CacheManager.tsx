/**
 * Unified Cache Manager Component
 * Consolidates admin and performance cache management functionality
 */

import React, { useState, useEffect, useCallback } from 'react';
import {
  Card,
  Switch,
  Button,
  Space,
  Typography,
  Divider,
  Alert,
  Statistic,
  Row,
  Col,
  List,
  Tag,
  Modal,
  message,
  Tooltip,
  Progress,
  Input,
  Tabs
} from 'antd';
import {
  DeleteOutlined,
  ReloadOutlined,
  InfoCircleOutlined,
  ThunderboltOutlined,
  DatabaseOutlined,
  ClockCircleOutlined,
  ExportOutlined,
  ImportOutlined,
  ClearOutlined
} from '@ant-design/icons';
import { tuningApi } from '../../services/tuningApi';
import { useQueryStore } from '../../stores/queryStore';
import { queryCacheService } from '../../services/queryCacheService';

const { Title, Text, Paragraph } = Typography;
const { Search } = Input;
const { TabPane } = Tabs;

interface CacheStats {
  totalEntries: number;
  totalSize: string;
  hitRate: number;
  missRate: number;
  lastCleared: string | null;
}

interface CacheEntry {
  key: string;
  size: string;
  createdAt: string;
  lastAccessed: string;
  hitCount: number;
  type: 'query' | 'prompt' | 'schema';
}

interface CacheMetrics {
  totalQueries: number;
  cacheHits: number;
  cacheMisses: number;
  totalSize: number;
  hitRate: number;
}

export const CacheManager: React.FC = () => {
  // Admin cache settings
  const [cacheEnabled, setCacheEnabled] = useState(true);
  const [promptCacheEnabled, setPromptCacheEnabled] = useState(true);
  const [loading, setLoading] = useState(false);
  const [aiSettings, setAiSettings] = useState<any[]>([]);
  const [cacheEnabledSetting, setCacheEnabledSetting] = useState<any>(null);
  const [promptCacheEnabledSetting, setPromptCacheEnabledSetting] = useState<any>(null);
  const [stats, setStats] = useState<CacheStats>({
    totalEntries: 0,
    totalSize: '0 KB',
    hitRate: 0,
    missRate: 0,
    lastCleared: null
  });
  const [cacheEntries, setCacheEntries] = useState<CacheEntry[]>([]);

  // Performance cache metrics
  const [metrics, setMetrics] = useState<CacheMetrics | null>(null);
  const [clearPattern, setClearPattern] = useState('');

  // Load AI settings and cache settings
  useEffect(() => {
    loadAISettings();
    loadCacheStats();
    loadCacheEntries();
    loadCacheMetrics();
  }, []);

  const loadAISettings = useCallback(async () => {
    try {
      const settings = await tuningApi.getAISettings();
      setAiSettings(settings);
      
      // Find cache-related settings
      const cacheEnabledSetting = settings.find((s: any) => s.key === 'cache-enabled');
      const promptCacheEnabledSetting = settings.find((s: any) => s.key === 'prompt-cache-enabled');

      if (cacheEnabledSetting) {
        setCacheEnabledSetting(cacheEnabledSetting);
        setCacheEnabled(cacheEnabledSetting.settingValue === 'true');
      }
      if (promptCacheEnabledSetting) {
        setPromptCacheEnabledSetting(promptCacheEnabledSetting);
        setPromptCacheEnabled(promptCacheEnabledSetting.settingValue === 'true');
      }
    } catch (error) {
      console.error('Failed to load AI settings:', error);
      // Fallback to localStorage
      const savedCacheEnabled = localStorage.getItem('cache-enabled');
      const savedPromptCacheEnabled = localStorage.getItem('prompt-cache-enabled');

      if (savedCacheEnabled !== null) {
        setCacheEnabled(JSON.parse(savedCacheEnabled));
      }
      if (savedPromptCacheEnabled !== null) {
        setPromptCacheEnabled(JSON.parse(savedPromptCacheEnabled));
      }
    }
  }, []);

  const loadCacheStats = useCallback(() => {
    // Mock cache stats - in real implementation, this would call an API
    const mockStats: CacheStats = {
      totalEntries: 45,
      totalSize: '2.3 MB',
      hitRate: 78.5,
      missRate: 21.5,
      lastCleared: localStorage.getItem('cache-last-cleared')
    };
    setStats(mockStats);
  }, []);

  const loadCacheEntries = useCallback(() => {
    // Mock cache entries - in real implementation, this would call an API
    const mockEntries: CacheEntry[] = [
      {
        key: 'query:player-stats-last-week',
        size: '156 KB',
        createdAt: '2024-01-15 10:30:00',
        lastAccessed: '2024-01-15 14:22:00',
        hitCount: 12,
        type: 'query'
      },
      {
        key: 'prompt:schema-context-filtered',
        size: '89 KB',
        createdAt: '2024-01-15 09:15:00',
        lastAccessed: '2024-01-15 14:20:00',
        hitCount: 8,
        type: 'prompt'
      },
      {
        key: 'schema:daily-actions-metadata',
        size: '234 KB',
        createdAt: '2024-01-15 08:00:00',
        lastAccessed: '2024-01-15 14:18:00',
        hitCount: 25,
        type: 'schema'
      }
    ];
    setCacheEntries(mockEntries);
  }, []);

  const loadCacheMetrics = useCallback(async () => {
    try {
      const cacheMetrics = await queryCacheService.getCacheMetrics();
      setMetrics(cacheMetrics);
    } catch (error) {
      console.error('Failed to load cache metrics:', error);
      // Mock metrics for fallback
      setMetrics({
        totalQueries: 150,
        cacheHits: 118,
        cacheMisses: 32,
        totalSize: 2.3 * 1024 * 1024, // 2.3 MB in bytes
        hitRate: 78.7
      });
    }
  }, []);

  const handleCacheToggle = async (enabled: boolean) => {
    try {
      if (cacheEnabledSetting) {
        await tuningApi.updateAISetting(cacheEnabledSetting.id, {
          ...cacheEnabledSetting,
          settingValue: enabled.toString()
        });
      }
      setCacheEnabled(enabled);
      localStorage.setItem('cache-enabled', JSON.stringify(enabled));
      message.success(`Query caching ${enabled ? 'enabled' : 'disabled'}`);
    } catch (error) {
      console.error('Failed to update cache setting:', error);
      message.error('Failed to update cache setting');
    }
  };

  const handlePromptCacheToggle = async (enabled: boolean) => {
    try {
      if (promptCacheEnabledSetting) {
        await tuningApi.updateAISetting(promptCacheEnabledSetting.id, {
          ...promptCacheEnabledSetting,
          settingValue: enabled.toString()
        });
      }
      setPromptCacheEnabled(enabled);
      localStorage.setItem('prompt-cache-enabled', JSON.stringify(enabled));
      message.success(`Prompt caching ${enabled ? 'enabled' : 'disabled'}`);
    } catch (error) {
      console.error('Failed to update prompt cache setting:', error);
      message.error('Failed to update prompt cache setting');
    }
  };

  const handleClearAllCache = () => {
    Modal.confirm({
      title: 'Clear All Cache',
      content: 'Are you sure you want to clear all cached data? This action cannot be undone.',
      okText: 'Clear All',
      okType: 'danger',
      onOk: () => {
        setLoading(true);

        // Clear localStorage cache items
        Object.keys(localStorage).forEach(key => {
          if (key.includes('cache') || key.includes('query-result') || key.includes('prompt-cache')) {
            localStorage.removeItem(key);
          }
        });

        // Update last cleared timestamp
        const now = new Date().toISOString();
        localStorage.setItem('cache-last-cleared', now);

        setTimeout(() => {
          setLoading(false);
          loadCacheStats();
          loadCacheEntries();
          loadCacheMetrics();
          message.success('All cache cleared successfully');
        }, 1000);
      }
    });
  };

  const handleClearPattern = async (pattern: string) => {
    if (!pattern.trim()) {
      message.warning('Please enter a pattern to clear');
      return;
    }

    try {
      await queryCacheService.invalidateCache(pattern);
      message.success(`Cleared cache entries matching pattern: ${pattern}`);
      setClearPattern('');
      loadCacheMetrics();
    } catch (error) {
      console.error('Failed to clear cache pattern:', error);
      message.error('Failed to clear cache pattern');
    }
  };

  const formatBytes = (bytes: number): string => {
    if (bytes === 0) return '0 B';
    const k = 1024;
    const sizes = ['B', 'KB', 'MB', 'GB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
  };

  const getHitRateColor = (rate: number): string => {
    if (rate >= 80) return '#52c41a';
    if (rate >= 60) return '#faad14';
    return '#ff4d4f';
  };

  const getTypeColor = (type: string): string => {
    switch (type) {
      case 'query': return 'blue';
      case 'prompt': return 'green';
      case 'schema': return 'orange';
      default: return 'default';
    }
  };

  const renderAdminTab = () => (
    <div>
      {/* Cache Statistics */}
      <Card title="Cache Statistics" style={{ marginBottom: '16px' }}>
        <Row gutter={16}>
          <Col xs={24} sm={12} md={6}>
            <Statistic
              title="Total Entries"
              value={stats.totalEntries}
              prefix={<DatabaseOutlined />}
            />
          </Col>
          <Col xs={24} sm={12} md={6}>
            <Statistic
              title="Total Size"
              value={stats.totalSize}
              prefix={<ThunderboltOutlined />}
            />
          </Col>
          <Col xs={24} sm={12} md={6}>
            <Statistic
              title="Hit Rate"
              value={stats.hitRate}
              suffix="%"
              precision={1}
              valueStyle={{ color: getHitRateColor(stats.hitRate) }}
            />
          </Col>
          <Col xs={24} sm={12} md={6}>
            <Statistic
              title="Last Cleared"
              value={stats.lastCleared ? new Date(stats.lastCleared).toLocaleDateString() : 'Never'}
              prefix={<ClockCircleOutlined />}
            />
          </Col>
        </Row>
      </Card>

      {/* Cache Settings */}
      <Card title="Cache Settings" style={{ marginBottom: '16px' }}>
        <Row gutter={[16, 16]}>
          <Col xs={24} md={12}>
            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
              <div>
                <Text strong>Query Result Caching</Text>
                <br />
                <Text type="secondary">
                  Cache query results to improve performance
                </Text>
              </div>
              <Switch
                checked={cacheEnabled}
                onChange={handleCacheToggle}
                checkedChildren="ON"
                unCheckedChildren="OFF"
              />
            </div>
          </Col>
          <Col xs={24} md={12}>
            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
              <div>
                <Text strong>AI Prompt Caching</Text>
                <br />
                <Text type="secondary">
                  Cache AI prompts and responses for debugging and testing
                </Text>
              </div>
              <Switch
                checked={promptCacheEnabled}
                onChange={handlePromptCacheToggle}
                checkedChildren="ON"
                unCheckedChildren="OFF"
              />
            </div>
          </Col>
        </Row>

        <Divider />

        <Space>
          <Button
            type="primary"
            danger
            icon={<DeleteOutlined />}
            onClick={handleClearAllCache}
            loading={loading}
          >
            Clear All Cache
          </Button>
          <Button
            icon={<ReloadOutlined />}
            onClick={() => {
              loadCacheStats();
              loadCacheEntries();
              loadCacheMetrics();
              message.success('Cache data refreshed');
            }}
          >
            Refresh
          </Button>
        </Space>
      </Card>

      {/* Cache Entries */}
      <Card title="Cache Entries">
        <List
          dataSource={cacheEntries}
          renderItem={(entry) => (
            <List.Item
              actions={[
                <Tooltip title="Delete Entry">
                  <Button
                    type="text"
                    danger
                    icon={<DeleteOutlined />}
                    onClick={() => {
                      Modal.confirm({
                        title: 'Delete Cache Entry',
                        content: `Are you sure you want to delete "${entry.key}"?`,
                        onOk: () => {
                          setCacheEntries(prev => prev.filter(e => e.key !== entry.key));
                          message.success('Cache entry deleted');
                        }
                      });
                    }}
                  />
                </Tooltip>
              ]}
            >
              <List.Item.Meta
                title={
                  <Space>
                    <Text code>{entry.key}</Text>
                    <Tag color={getTypeColor(entry.type)}>{entry.type}</Tag>
                  </Space>
                }
                description={
                  <Space direction="vertical" size="small">
                    <Space>
                      <Text type="secondary">Size: {entry.size}</Text>
                      <Text type="secondary">Hits: {entry.hitCount}</Text>
                    </Space>
                    <Space>
                      <Text type="secondary">Created: {entry.createdAt}</Text>
                      <Text type="secondary">Last accessed: {entry.lastAccessed}</Text>
                    </Space>
                  </Space>
                }
              />
            </List.Item>
          )}
        />
      </Card>
    </div>
  );

  const renderPerformanceTab = () => (
    <div>
      {/* Performance Metrics */}
      <Row gutter={[16, 16]} style={{ marginBottom: '16px' }}>
        <Col xs={24} sm={12} md={6}>
          <Card>
            <Statistic
              title="Total Queries"
              value={metrics?.totalQueries || 0}
              prefix={<DatabaseOutlined />}
            />
          </Card>
        </Col>

        <Col xs={24} sm={12} md={6}>
          <Card>
            <Statistic
              title="Cache Hits"
              value={metrics?.cacheHits || 0}
              valueStyle={{ color: '#3f8600' }}
              prefix={<ThunderboltOutlined />}
            />
          </Card>
        </Col>

        <Col xs={24} sm={12} md={6}>
          <Card>
            <Statistic
              title="Cache Size"
              value={formatBytes(metrics?.totalSize || 0)}
              prefix={<InfoCircleOutlined />}
            />
          </Card>
        </Col>

        <Col xs={24} sm={12} md={6}>
          <Card>
            <div>
              <Text strong>Hit Rate</Text>
              <div style={{ marginTop: 8 }}>
                <Progress
                  percent={Math.round(metrics?.hitRate || 0)}
                  strokeColor={getHitRateColor(metrics?.hitRate || 0)}
                  size="small"
                />
                <Text style={{ color: getHitRateColor(metrics?.hitRate || 0) }}>
                  {(metrics?.hitRate || 0).toFixed(1)}%
                </Text>
              </div>
            </div>
          </Card>
        </Col>
      </Row>

      <Card title="Cache Actions" style={{ marginTop: 16 }}>
        <Space direction="vertical" style={{ width: '100%' }}>
          <Row gutter={[8, 8]} align="middle">
            <Col flex="auto">
              <Search
                placeholder="Enter pattern to clear specific cache entries"
                value={clearPattern}
                onChange={(e) => setClearPattern(e.target.value)}
                onSearch={handleClearPattern}
                enterButton="Clear Pattern"
              />
            </Col>
          </Row>

          <Row gutter={[8, 8]}>
            <Col>
              <Button
                icon={<ClearOutlined />}
                onClick={handleClearAllCache}
                danger
              >
                Clear All
              </Button>
            </Col>
            <Col>
              <Button
                icon={<ExportOutlined />}
                onClick={() => message.info('Export functionality coming soon')}
              >
                Export Cache
              </Button>
            </Col>
            <Col>
              <Button
                icon={<ImportOutlined />}
                onClick={() => message.info('Import functionality coming soon')}
              >
                Import Cache
              </Button>
            </Col>
            <Col>
              <Button
                icon={<ReloadOutlined />}
                onClick={loadCacheMetrics}
              >
                Refresh Metrics
              </Button>
            </Col>
          </Row>
        </Space>
      </Card>

      <Card title="Cache Information" style={{ marginTop: 16 }}>
        <Space direction="vertical" style={{ width: '100%' }}>
          <Text>
            <strong>Cache Strategy:</strong> LRU (Least Recently Used) eviction with TTL (Time To Live) expiration
          </Text>
          <Text>
            <strong>Default TTL:</strong> 1 hour per query result
          </Text>
          <Text>
            <strong>Max Cache Size:</strong> 100 MB
          </Text>
          <Text>
            <strong>Max Entries:</strong> 1,000 cached queries
          </Text>
          <Text>
            <strong>Storage:</strong> IndexedDB (client-side persistent storage)
          </Text>
        </Space>
      </Card>
    </div>
  );

  return (
    <div style={{ padding: '24px' }}>
      <div style={{ marginBottom: '24px' }}>
        <Title level={2}>
          <DatabaseOutlined style={{ color: '#1890ff', marginRight: '8px' }} />
          Cache Manager
        </Title>
        <Paragraph>
          Manage query caching, AI prompt caching, and monitor cache performance metrics.
        </Paragraph>
      </div>

      <Tabs defaultActiveKey="admin">
        <TabPane tab="Admin Settings" key="admin">
          {renderAdminTab()}
        </TabPane>
        <TabPane tab="Performance Metrics" key="performance">
          {renderPerformanceTab()}
        </TabPane>
      </Tabs>
    </div>
  );
};
