import React, { useState, useEffect, useCallback } from 'react';
import {
  Card,
  Button,
  Space,
  Typography,
  Alert,
  Progress,
  Statistic,
  Row,
  Col,
  Switch,
  Tooltip,
  Badge,
  Divider,
  List,
  Tag,
  Modal,
  Spin
} from 'antd';
import {
  ThunderboltOutlined,
  RocketOutlined,
  SettingOutlined,
  CheckCircleOutlined,
  WarningOutlined,
  ClockCircleOutlined,
  CloudOutlined,
  DatabaseOutlined,
  ClearOutlined,
  ReloadOutlined,
  ControlOutlined
} from '@ant-design/icons';
import { usePerformanceMonitor, useRealTimePerformance, useCachePerformance } from '../../hooks/useOptimization';

const { Title, Text, Paragraph } = Typography;

interface OptimizationRecommendation {
  type: 'success' | 'warning' | 'error' | 'info';
  category: string;
  title: string;
  description: string;
  action: string;
  priority: 'low' | 'medium' | 'high';
  impact: string;
  effort: string;
}

interface OptimizationResult {
  success: boolean;
  message: string;
  timestamp: string;
  metricsImprovement?: {
    before: number;
    after: number;
    improvement: number;
  };
}

export const PerformanceOptimizer: React.FC = () => {
  const [recommendations, setRecommendations] = useState<OptimizationRecommendation[]>([]);
  const [optimizationResults, setOptimizationResults] = useState<OptimizationResult[]>([]);
  const [loading, setLoading] = useState(false);
  const [autoOptimization, setAutoOptimization] = useState(false);
  const [optimizing, setOptimizing] = useState(false);
  
  const performanceMetrics = usePerformanceMonitor('PerformanceOptimizer');
  const realTimeMetrics = useRealTimePerformance();
  const { cacheMetrics, updateCacheMetrics } = useCachePerformance();

  const loadRecommendations = useCallback(async () => {
    setLoading(true);
    try {
      const response = await fetch('/api/performance-monitoring/recommendations', {
        headers: { 'Authorization': `Bearer ${localStorage.getItem('token')}` }
      });
      
      if (response.ok) {
        const data = await response.json();
        setRecommendations(data.recommendations || []);
      }
    } catch (error) {
      console.error('Failed to load recommendations:', error);
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    loadRecommendations();
  }, [loadRecommendations]);

  const optimizeCache = async () => {
    setOptimizing(true);
    try {
      const beforeMetrics = cacheMetrics.hitRate;
      
      const response = await fetch('/api/performance-monitoring/optimize/cache', {
        method: 'POST',
        headers: { 'Authorization': `Bearer ${localStorage.getItem('token')}` }
      });
      
      if (response.ok) {
        const result = await response.json();
        
        // Update cache metrics
        await updateCacheMetrics();
        
        const optimizationResult: OptimizationResult = {
          success: true,
          message: result.message,
          timestamp: new Date().toISOString(),
          metricsImprovement: {
            before: beforeMetrics,
            after: cacheMetrics.hitRate,
            improvement: cacheMetrics.hitRate - beforeMetrics
          }
        };
        
        setOptimizationResults(prev => [optimizationResult, ...prev.slice(0, 9)]);
        await loadRecommendations(); // Refresh recommendations
      }
    } catch (error) {
      console.error('Cache optimization failed:', error);
      setOptimizationResults(prev => [{
        success: false,
        message: 'Cache optimization failed',
        timestamp: new Date().toISOString()
      }, ...prev.slice(0, 9)]);
    } finally {
      setOptimizing(false);
    }
  };

  const clearCache = async (pattern?: string) => {
    setOptimizing(true);
    try {
      const url = pattern 
        ? `/api/performance-monitoring/cache/clear?pattern=${encodeURIComponent(pattern)}`
        : '/api/performance-monitoring/cache/clear';
        
      const response = await fetch(url, {
        method: 'POST',
        headers: { 'Authorization': `Bearer ${localStorage.getItem('token')}` }
      });
      
      if (response.ok) {
        const result = await response.json();
        
        setOptimizationResults(prev => [{
          success: true,
          message: result.message,
          timestamp: new Date().toISOString()
        }, ...prev.slice(0, 9)]);
        
        await updateCacheMetrics();
      }
    } catch (error) {
      console.error('Cache clear failed:', error);
    } finally {
      setOptimizing(false);
    }
  };

  const getPriorityColor = (priority: string) => {
    switch (priority) {
      case 'high': return '#ff4d4f';
      case 'medium': return '#faad14';
      case 'low': return '#52c41a';
      default: return '#d9d9d9';
    }
  };

  const getTypeIcon = (type: string) => {
    switch (type) {
      case 'success': return <CheckCircleOutlined style={{ color: '#52c41a' }} />;
      case 'warning': return <WarningOutlined style={{ color: '#faad14' }} />;
      case 'error': return <WarningOutlined style={{ color: '#ff4d4f' }} />;
      default: return <ThunderboltOutlined style={{ color: '#1890ff' }} />;
    }
  };

  const renderQuickActions = () => (
    <Card title={<Space><RocketOutlined />Quick Optimizations</Space>} size="small">
      <Space direction="vertical" style={{ width: '100%' }}>
        <Button
          type="primary"
          icon={<ControlOutlined />}
          onClick={optimizeCache}
          loading={optimizing}
          block
        >
          Optimize Cache Performance
        </Button>
        
        <Button 
          icon={<ClearOutlined />} 
          onClick={() => clearCache()}
          loading={optimizing}
          block
        >
          Clear All Cache
        </Button>
        
        <Button 
          icon={<DatabaseOutlined />} 
          onClick={() => clearCache('query_*')}
          loading={optimizing}
          block
        >
          Clear Query Cache
        </Button>
        
        <Button 
          icon={<ReloadOutlined />} 
          onClick={loadRecommendations}
          loading={loading}
          block
        >
          Refresh Recommendations
        </Button>
      </Space>
    </Card>
  );

  const renderPerformanceMetrics = () => (
    <Row gutter={[16, 16]}>
      <Col xs={24} sm={12} md={6}>
        <Card size="small">
          <Statistic
            title="Cache Hit Rate"
            value={cacheMetrics.hitRate}
            suffix="%"
            precision={1}
            valueStyle={{ color: cacheMetrics.hitRate > 80 ? '#3f8600' : '#cf1322' }}
            prefix={<CloudOutlined />}
          />
        </Card>
      </Col>
      <Col xs={24} sm={12} md={6}>
        <Card size="small">
          <Statistic
            title="Memory Usage"
            value={realTimeMetrics.memoryUsage}
            suffix="%"
            precision={1}
            valueStyle={{ color: realTimeMetrics.memoryUsage < 80 ? '#3f8600' : '#cf1322' }}
            prefix={<DatabaseOutlined />}
          />
        </Card>
      </Col>
      <Col xs={24} sm={12} md={6}>
        <Card size="small">
          <Statistic
            title="Render Count"
            value={performanceMetrics.renderCount}
            prefix={<ThunderboltOutlined />}
          />
        </Card>
      </Col>
      <Col xs={24} sm={12} md={6}>
        <Card size="small">
          <Statistic
            title="Avg Retrieval"
            value={cacheMetrics.averageRetrievalTime}
            suffix="ms"
            precision={1}
            prefix={<ClockCircleOutlined />}
          />
        </Card>
      </Col>
    </Row>
  );

  const renderRecommendations = () => (
    <Card 
      title={<Space><SettingOutlined />Performance Recommendations</Space>}
      extra={
        <Space>
          <Switch
            checked={autoOptimization}
            onChange={setAutoOptimization}
            checkedChildren="Auto"
            unCheckedChildren="Manual"
          />
          <Button icon={<ReloadOutlined />} onClick={loadRecommendations} loading={loading} />
        </Space>
      }
      size="small"
    >
      {loading ? (
        <div style={{ textAlign: 'center', padding: '40px' }}>
          <Spin size="large" />
        </div>
      ) : recommendations.length === 0 ? (
        <Alert
          message="No Recommendations"
          description="Your system is performing optimally. No immediate optimizations needed."
          type="success"
          showIcon
        />
      ) : (
        <List
          dataSource={recommendations}
          renderItem={(item) => (
            <List.Item
              actions={[
                <Tag color={getPriorityColor(item.priority)} key="priority">
                  {item.priority.toUpperCase()}
                </Tag>,
                <Button type="link" size="small" key="action">
                  {item.action}
                </Button>
              ]}
            >
              <List.Item.Meta
                avatar={getTypeIcon(item.type)}
                title={
                  <Space>
                    {item.title}
                    <Badge color={getPriorityColor(item.priority)} />
                  </Space>
                }
                description={
                  <div>
                    <Paragraph style={{ margin: 0 }}>{item.description}</Paragraph>
                    <Space style={{ marginTop: '8px' }}>
                      <Tag color="blue">{item.category}</Tag>
                      <Text type="secondary">Impact: {item.impact}</Text>
                      <Text type="secondary">Effort: {item.effort}</Text>
                    </Space>
                  </div>
                }
              />
            </List.Item>
          )}
        />
      )}
    </Card>
  );

  const renderOptimizationHistory = () => (
    <Card title={<Space><CheckCircleOutlined />Recent Optimizations</Space>} size="small">
      {optimizationResults.length === 0 ? (
        <Text type="secondary">No optimizations performed yet</Text>
      ) : (
        <List
          dataSource={optimizationResults}
          renderItem={(item) => (
            <List.Item>
              <List.Item.Meta
                avatar={item.success ? 
                  <CheckCircleOutlined style={{ color: '#52c41a' }} /> : 
                  <WarningOutlined style={{ color: '#ff4d4f' }} />
                }
                title={item.message}
                description={
                  <Space>
                    <Text type="secondary">{new Date(item.timestamp).toLocaleString()}</Text>
                    {item.metricsImprovement && (
                      <Tag color="green">
                        +{item.metricsImprovement.improvement.toFixed(1)}% improvement
                      </Tag>
                    )}
                  </Space>
                }
              />
            </List.Item>
          )}
        />
      )}
    </Card>
  );

  return (
    <div className="page-container full-width">
      <div style={{ marginBottom: '24px' }}>
        <Title level={2}>
          <ThunderboltOutlined style={{ color: '#1890ff', marginRight: '8px' }} />
          Performance Optimizer
        </Title>
        <Text type="secondary">
          Automated performance optimization and system tuning
        </Text>
      </div>

      <Space direction="vertical" style={{ width: '100%' }} size="large">
        {renderPerformanceMetrics()}
        
        <Row gutter={[16, 16]}>
          <Col xs={24} lg={16}>
            {renderRecommendations()}
          </Col>
          <Col xs={24} lg={8}>
            <Space direction="vertical" style={{ width: '100%' }}>
              {renderQuickActions()}
              {renderOptimizationHistory()}
            </Space>
          </Col>
        </Row>
      </Space>
    </div>
  );
};

export default PerformanceOptimizer;
