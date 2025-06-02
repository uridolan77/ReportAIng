import React, { useState, useEffect } from 'react';
import { Card, Button, Space, Statistic, Row, Col, Input, message, Modal, Progress, Typography } from 'antd';
import {
  ReloadOutlined,
  InfoCircleOutlined,
  ExportOutlined,
  ImportOutlined,
  ClearOutlined
} from '@ant-design/icons';
import { useQueryStore } from '../../stores/queryStore';
import { queryCacheService } from '../../services/queryCacheService';

const { Text, Title } = Typography;
const { Search } = Input;

interface CacheMetrics {
  totalQueries: number;
  cacheHits: number;
  cacheMisses: number;
  totalSize: number;
  hitRate: number;
}

export const CacheManager: React.FC = () => {
  const [metrics, setMetrics] = useState<CacheMetrics | null>(null);
  const [loading, setLoading] = useState(false);
  const [clearPattern, setClearPattern] = useState('');
  const { clearCache, getCacheMetrics } = useQueryStore();

  const loadMetrics = async () => {
    setLoading(true);
    try {
      const cacheMetrics = await getCacheMetrics();
      setMetrics(cacheMetrics);
    } catch (error) {
      console.error('Failed to load cache metrics:', error);
      message.error('Failed to load cache metrics');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadMetrics();
  }, []);

  const handleClearAll = () => {
    Modal.confirm({
      title: 'Clear All Cache',
      content: 'Are you sure you want to clear all cached query results? This action cannot be undone.',
      okText: 'Clear All',
      okType: 'danger',
      cancelText: 'Cancel',
      onOk: async () => {
        try {
          await clearCache();
          await loadMetrics();
          message.success('All cache cleared successfully');
        } catch (error) {
          message.error('Failed to clear cache');
        }
      },
    });
  };

  const handleClearPattern = async () => {
    if (!clearPattern.trim()) {
      message.warning('Please enter a pattern to clear');
      return;
    }

    try {
      await clearCache(clearPattern);
      await loadMetrics();
      message.success(`Cache entries matching "${clearPattern}" cleared successfully`);
      setClearPattern('');
    } catch (error) {
      message.error('Failed to clear cache entries');
    }
  };

  const handleExportCache = async () => {
    try {
      const cacheData = await queryCacheService.exportCache();
      const dataStr = JSON.stringify(cacheData, null, 2);
      const dataBlob = new Blob([dataStr], { type: 'application/json' });
      
      const url = URL.createObjectURL(dataBlob);
      const link = document.createElement('a');
      link.href = url;
      link.download = `query-cache-export-${new Date().toISOString().split('T')[0]}.json`;
      document.body.appendChild(link);
      link.click();
      document.body.removeChild(link);
      URL.revokeObjectURL(url);
      
      message.success('Cache exported successfully');
    } catch (error) {
      message.error('Failed to export cache');
    }
  };

  const handleImportCache = () => {
    const input = document.createElement('input');
    input.type = 'file';
    input.accept = '.json';
    input.onchange = async (e) => {
      const file = (e.target as HTMLInputElement).files?.[0];
      if (!file) return;

      try {
        const text = await file.text();
        const cacheData = JSON.parse(text);
        await queryCacheService.importCache(cacheData);
        await loadMetrics();
        message.success('Cache imported successfully');
      } catch (error) {
        message.error('Failed to import cache');
      }
    };
    input.click();
  };

  const formatBytes = (bytes: number): string => {
    if (bytes === 0) return '0 Bytes';
    const k = 1024;
    const sizes = ['Bytes', 'KB', 'MB', 'GB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
  };

  const getHitRateColor = (hitRate: number): string => {
    if (hitRate >= 80) return '#52c41a';
    if (hitRate >= 60) return '#faad14';
    return '#ff4d4f';
  };

  return (
    <div style={{ padding: '16px' }}>
      <Title level={4}>Query Cache Management</Title>
      
      <Row gutter={[16, 16]}>
        <Col xs={24} sm={12} md={6}>
          <Card>
            <Statistic
              title="Total Cached Queries"
              value={metrics?.totalQueries || 0}
              prefix={<InfoCircleOutlined />}
            />
          </Card>
        </Col>
        
        <Col xs={24} sm={12} md={6}>
          <Card>
            <Statistic
              title="Cache Hits"
              value={metrics?.cacheHits || 0}
              valueStyle={{ color: '#3f8600' }}
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
          
          <Space wrap>
            <Button
              icon={<ReloadOutlined />}
              onClick={loadMetrics}
              loading={loading}
            >
              Refresh Metrics
            </Button>
            
            <Button
              icon={<ClearOutlined />}
              onClick={handleClearAll}
              danger
            >
              Clear All Cache
            </Button>
            
            <Button
              icon={<ExportOutlined />}
              onClick={handleExportCache}
            >
              Export Cache
            </Button>
            
            <Button
              icon={<ImportOutlined />}
              onClick={handleImportCache}
            >
              Import Cache
            </Button>
          </Space>
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
};
