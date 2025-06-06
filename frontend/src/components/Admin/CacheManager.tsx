/**
 * Cache Manager Component
 * Provides admin interface for managing query caching
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
  Tooltip
} from 'antd';
import {
  DeleteOutlined,
  ReloadOutlined,
  InfoCircleOutlined,
  ThunderboltOutlined,
  DatabaseOutlined,
  ClockCircleOutlined
} from '@ant-design/icons';
import { tuningApi } from '../../services/tuningApi';

const { Title, Text, Paragraph } = Typography;

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

export const CacheManager: React.FC = () => {
  const [cacheEnabled, setCacheEnabled] = useState(true);
  const [promptCacheEnabled, setPromptCacheEnabled] = useState(true);
  const [loading, setLoading] = useState(false);
  const [aiSettings, setAiSettings] = useState<any[]>([]);
  const [stats, setStats] = useState<CacheStats>({
    totalEntries: 0,
    totalSize: '0 KB',
    hitRate: 0,
    missRate: 0,
    lastCleared: null
  });
  const [cacheEntries, setCacheEntries] = useState<CacheEntry[]>([]);

  // Load AI settings and cache settings
  useEffect(() => {
    loadAISettings();
    loadCacheStats();
    loadCacheEntries();
  }, [loadAISettings, loadCacheStats, loadCacheEntries]);

  const loadAISettings = useCallback(async () => {
    try {
      const settings = await tuningApi.getAISettings();
      setAiSettings(settings);

      // Update local state based on database settings
      const cacheSetting = settings.find((s: any) => s.settingKey === 'EnableQueryCaching');
      if (cacheSetting) {
        const isEnabled = cacheSetting.settingValue === 'true';
        setCacheEnabled(isEnabled);
        setPromptCacheEnabled(isEnabled);
        // Also update localStorage to keep it in sync
        localStorage.setItem('cache-enabled', JSON.stringify(isEnabled));
        localStorage.setItem('prompt-cache-enabled', JSON.stringify(isEnabled));
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

  const handleCacheToggle = async (enabled: boolean) => {
    try {
      // Find the EnableQueryCaching setting
      const cacheSetting = aiSettings.find((s: any) => s.settingKey === 'EnableQueryCaching');
      if (cacheSetting) {
        // Update the database setting via API
        await tuningApi.updateAISetting(cacheSetting.id, {
          ...cacheSetting,
          settingValue: enabled ? 'true' : 'false'
        });

        // Update local state
        setCacheEnabled(enabled);
        setPromptCacheEnabled(enabled);

        // Update localStorage to keep it in sync
        localStorage.setItem('cache-enabled', JSON.stringify(enabled));
        localStorage.setItem('prompt-cache-enabled', JSON.stringify(enabled));

        message.success(`Query caching ${enabled ? 'enabled' : 'disabled'} in database`);

        // Reload settings to ensure consistency
        await loadAISettings();
      } else {
        // Setting not found - use localStorage only and show warning
        console.warn('EnableQueryCaching setting not found in database. Using localStorage only.');

        // Update local state and localStorage
        setCacheEnabled(enabled);
        setPromptCacheEnabled(enabled);
        localStorage.setItem('cache-enabled', JSON.stringify(enabled));
        localStorage.setItem('prompt-cache-enabled', JSON.stringify(enabled));

        message.warning(`Query caching ${enabled ? 'enabled' : 'disabled'} locally only. Database setting not found - please run database migrations.`);
      }
    } catch (error) {
      console.error('Failed to update cache setting:', error);
      message.error('Failed to update cache setting');
    }
  };

  const handlePromptCacheToggle = (enabled: boolean) => {
    setPromptCacheEnabled(enabled);
    localStorage.setItem('prompt-cache-enabled', JSON.stringify(enabled));
    message.success(`Prompt caching ${enabled ? 'enabled' : 'disabled'}`);
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
          message.success('All cache cleared successfully');
        }, 1000);
      }
    });
  };

  const handleClearEntry = (key: string) => {
    Modal.confirm({
      title: 'Clear Cache Entry',
      content: `Are you sure you want to clear the cache entry: ${key}?`,
      okText: 'Clear',
      okType: 'danger',
      onOk: () => {
        // Remove from mock data
        setCacheEntries(prev => prev.filter(entry => entry.key !== key));
        message.success('Cache entry cleared');
      }
    });
  };

  const getTypeColor = (type: string) => {
    switch (type) {
      case 'query': return 'blue';
      case 'prompt': return 'green';
      case 'schema': return 'orange';
      default: return 'default';
    }
  };

  return (
    <div style={{ padding: '24px', maxWidth: '1200px', margin: '0 auto' }}>
      <div style={{ marginBottom: '24px' }}>
        <Title level={2}>
          <DatabaseOutlined style={{ marginRight: '12px', color: '#1890ff' }} />
          Cache Manager
        </Title>
        <Paragraph type="secondary">
          Manage query caching, prompt caching, and monitor cache performance for optimal system performance.
        </Paragraph>
      </div>

      {/* Cache Controls */}
      <Card title="Cache Settings" style={{ marginBottom: '24px' }}>
        <Row gutter={[24, 16]}>
          <Col span={12}>
            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
              <div>
                <Text strong>Query Result Caching</Text>
                <br />
                <Text type="secondary" style={{ fontSize: '12px' }}>
                  Cache database query results for faster repeated queries
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
          <Col span={12}>
            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
              <div>
                <Text strong>Prompt Caching</Text>
                <br />
                <Text type="secondary" style={{ fontSize: '12px' }}>
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
              message.success('Cache data refreshed');
            }}
          >
            Refresh
          </Button>
        </Space>
      </Card>

      {/* Cache Statistics */}
      <Card title="Cache Statistics" style={{ marginBottom: '24px' }}>
        <Row gutter={16}>
          <Col span={6}>
            <Statistic
              title="Total Entries"
              value={stats.totalEntries}
              prefix={<DatabaseOutlined />}
            />
          </Col>
          <Col span={6}>
            <Statistic
              title="Total Size"
              value={stats.totalSize}
              prefix={<ThunderboltOutlined />}
            />
          </Col>
          <Col span={6}>
            <Statistic
              title="Hit Rate"
              value={stats.hitRate}
              suffix="%"
              precision={1}
              valueStyle={{ color: '#3f8600' }}
            />
          </Col>
          <Col span={6}>
            <Statistic
              title="Miss Rate"
              value={stats.missRate}
              suffix="%"
              precision={1}
              valueStyle={{ color: '#cf1322' }}
            />
          </Col>
        </Row>

        {stats.lastCleared && (
          <Alert
            message={`Cache last cleared: ${new Date(stats.lastCleared).toLocaleString()}`}
            type="info"
            icon={<ClockCircleOutlined />}
            style={{ marginTop: '16px' }}
          />
        )}
      </Card>

      {/* Cache Entries */}
      <Card title="Cache Entries">
        <List
          dataSource={cacheEntries}
          renderItem={(entry) => (
            <List.Item
              actions={[
                <Tooltip title="Clear this cache entry">
                  <Button
                    type="text"
                    danger
                    icon={<DeleteOutlined />}
                    onClick={() => handleClearEntry(entry.key)}
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
                  <Space split={<Divider type="vertical" />}>
                    <Text type="secondary">Size: {entry.size}</Text>
                    <Text type="secondary">Hits: {entry.hitCount}</Text>
                    <Text type="secondary">Created: {entry.createdAt}</Text>
                    <Text type="secondary">Last accessed: {entry.lastAccessed}</Text>
                  </Space>
                }
              />
            </List.Item>
          )}
        />
      </Card>

      {/* Cache Status Alert */}
      {!cacheEnabled && (
        <Alert
          message="Query caching is disabled"
          description="Queries will not be cached and may take longer to execute. Enable caching for better performance."
          type="warning"
          icon={<InfoCircleOutlined />}
          style={{ marginTop: '24px' }}
          showIcon
        />
      )}
    </div>
  );
};
