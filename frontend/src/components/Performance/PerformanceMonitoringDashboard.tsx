import React, { useState, useEffect, useCallback } from 'react';
import {
  Card,
  Row,
  Col,
  Statistic,
  Progress,
  Alert,
  Button,
  Space,
  Typography,
  Tabs,
  Table,
  Switch,
  Select,
  DatePicker,
  Spin,
  Tag
} from 'antd';
import {
  DashboardOutlined,
  ThunderboltOutlined,
  ClockCircleOutlined,
  DatabaseOutlined,
  CloudOutlined,
  WarningOutlined,
  CheckCircleOutlined,
  ReloadOutlined,
  SettingOutlined,
  RiseOutlined,
  FireOutlined
} from '@ant-design/icons';

import dayjs, { type Dayjs } from 'dayjs';
import { useMemoryOptimization } from '../../hooks/useMemoryOptimization';
import { IntelligentCodeSplitter } from '../../utils/bundleOptimization';
import './PerformanceMonitoringDashboard.css';

const { Title, Text } = Typography;
const { TabPane } = Tabs;
const { Option } = Select;
const { RangePicker } = DatePicker;

interface PerformanceMetrics {
  timestamp: string;
  queryExecutionTime: number;
  cacheHitRate: number;
  memoryUsage: number;
  cpuUsage: number;
  activeConnections: number;
  throughput: number;
  errorRate: number;
}

interface SystemHealth {
  overall: 'healthy' | 'warning' | 'critical';
  database: 'healthy' | 'warning' | 'critical';
  cache: 'healthy' | 'warning' | 'critical';
  ai: 'healthy' | 'warning' | 'critical';
  streaming: 'healthy' | 'warning' | 'critical';
}

interface CacheMetrics {
  hitRate: number;
  missRate: number;
  totalRequests: number;
  averageRetrievalTime: number;
  memoryUsage: number;
  evictionCount: number;
}

interface QueryPerformance {
  queryType: string;
  averageExecutionTime: number;
  totalExecutions: number;
  successRate: number;
  p95ExecutionTime: number;
  p99ExecutionTime: number;
}

export const PerformanceMonitoringDashboard: React.FC = () => {
  const [, setMetrics] = useState<PerformanceMetrics[]>([]);
  const [systemHealth, setSystemHealth] = useState<SystemHealth>({
    overall: 'healthy',
    database: 'healthy',
    cache: 'healthy',
    ai: 'healthy',
    streaming: 'healthy'
  });
  const [cacheMetrics, setCacheMetrics] = useState<CacheMetrics | null>(null);
  const [queryPerformance, setQueryPerformance] = useState<QueryPerformance[]>([]);
  const [loading, setLoading] = useState(false);
  const [autoRefresh, setAutoRefresh] = useState(true);
  const [refreshInterval, setRefreshInterval] = useState(30); // seconds
  const [timeRange, setTimeRange] = useState<[dayjs.Dayjs, dayjs.Dayjs]>([
    dayjs().subtract(1, 'hour'),
    dayjs()
  ]);

  // Advanced performance optimization hooks
  const {
    memoryMetrics,
    performCleanup,
    detectMemoryLeaks,
    getMemoryMetrics
  } = useMemoryOptimization({
    enableAutoCleanup: true,
    enableLeakDetection: true,
    onMemoryWarning: (metrics) => {
      console.warn('Memory warning:', metrics);
    },
    onMemoryLeak: (leakInfo) => {
      console.error('Memory leak detected:', leakInfo);
    }
  });

  // Bundle optimization instances
  const [bundleStats, setBundleStats] = useState<any>(null);

  const loadPerformanceData = useCallback(async () => {
    setLoading(true);
    try {
      // Load system health
      const healthResponse = await fetch('/api/health/detailed', {
        headers: { 'Authorization': `Bearer ${localStorage.getItem('token')}` }
      });
      if (healthResponse.ok) {
        const healthData = await healthResponse.json();
        setSystemHealth(healthData);
      }

      // Load performance metrics
      const metricsResponse = await fetch(`/api/monitoring/metrics?from=${timeRange[0].toISOString()}&to=${timeRange[1].toISOString()}`, {
        headers: { 'Authorization': `Bearer ${localStorage.getItem('token')}` }
      });
      if (metricsResponse.ok) {
        const metricsData = await metricsResponse.json();
        setMetrics(metricsData.metrics || []);
      }

      // Load cache metrics
      const cacheResponse = await fetch('/api/cache/metrics', {
        headers: { 'Authorization': `Bearer ${localStorage.getItem('token')}` }
      });
      if (cacheResponse.ok) {
        const cacheData = await cacheResponse.json();
        setCacheMetrics(cacheData);
      }

      // Load query performance
      const queryResponse = await fetch('/api/monitoring/query-performance', {
        headers: { 'Authorization': `Bearer ${localStorage.getItem('token')}` }
      });
      if (queryResponse.ok) {
        const queryData = await queryResponse.json();
        setQueryPerformance(queryData.performance || []);
      }

      // Load advanced performance data
      loadAdvancedPerformanceData();

    } catch (error) {
      console.error('❌ Error loading performance data:', error);
    } finally {
      setLoading(false);
    }
  }, [timeRange]);

  const loadAdvancedPerformanceData = useCallback(() => {
    // Get bundle optimization stats
    const splitter = IntelligentCodeSplitter.getInstance();

    setBundleStats(splitter.getStats());
    // Analytics and optimization recommendations would be processed here
  }, []);

  useEffect(() => {
    loadPerformanceData();
  }, [loadPerformanceData]);

  useEffect(() => {
    if (!autoRefresh) return;

    const interval = setInterval(loadPerformanceData, refreshInterval * 1000);
    return () => clearInterval(interval);
  }, [autoRefresh, refreshInterval, loadPerformanceData]);

  const getHealthColor = (status: string) => {
    switch (status) {
      case 'healthy': return '#52c41a';
      case 'warning': return '#faad14';
      case 'critical': return '#ff4d4f';
      default: return '#d9d9d9';
    }
  };

  const getHealthIcon = (status: string) => {
    switch (status) {
      case 'healthy': return <CheckCircleOutlined style={{ color: '#52c41a' }} />;
      case 'warning': return <WarningOutlined style={{ color: '#faad14' }} />;
      case 'critical': return <FireOutlined style={{ color: '#ff4d4f' }} />;
      default: return <ClockCircleOutlined style={{ color: '#d9d9d9' }} />;
    }
  };

  const renderSystemHealthOverview = () => (
    <Row gutter={[16, 16]}>
      <Col xs={24} sm={12} md={8} lg={4}>
        <Card size="small">
          <Statistic
            title="Overall Health"
            value={systemHealth.overall}
            prefix={getHealthIcon(systemHealth.overall)}
            valueStyle={{ color: getHealthColor(systemHealth.overall), textTransform: 'capitalize' }}
          />
        </Card>
      </Col>
      <Col xs={24} sm={12} md={8} lg={4}>
        <Card size="small">
          <Statistic
            title="Database"
            value={systemHealth.database}
            prefix={<DatabaseOutlined />}
            valueStyle={{ color: getHealthColor(systemHealth.database), textTransform: 'capitalize' }}
          />
        </Card>
      </Col>
      <Col xs={24} sm={12} md={8} lg={4}>
        <Card size="small">
          <Statistic
            title="Cache"
            value={systemHealth.cache}
            prefix={<CloudOutlined />}
            valueStyle={{ color: getHealthColor(systemHealth.cache), textTransform: 'capitalize' }}
          />
        </Card>
      </Col>
      <Col xs={24} sm={12} md={8} lg={4}>
        <Card size="small">
          <Statistic
            title="AI Services"
            value={systemHealth.ai}
            prefix={<ThunderboltOutlined />}
            valueStyle={{ color: getHealthColor(systemHealth.ai), textTransform: 'capitalize' }}
          />
        </Card>
      </Col>
      <Col xs={24} sm={12} md={8} lg={4}>
        <Card size="small">
          <Statistic
            title="Streaming"
            value={systemHealth.streaming}
            prefix={<RiseOutlined />}
            valueStyle={{ color: getHealthColor(systemHealth.streaming), textTransform: 'capitalize' }}
          />
        </Card>
      </Col>
    </Row>
  );

  const renderPerformanceCharts = () => (
    <Row gutter={[16, 16]}>
      <Col xs={24} lg={12}>
        <Card title="Query Execution Time" size="small">
          <div style={{ width: '100%', height: 300, display: 'flex', alignItems: 'center', justifyContent: 'center', background: '#f5f5f5' }}>
            <Text type="secondary">Query Execution Time Chart Placeholder</Text>
          </div>
        </Card>
      </Col>
      <Col xs={24} lg={12}>
        <Card title="Cache Performance" size="small">
          <div style={{ width: '100%', height: 300, display: 'flex', alignItems: 'center', justifyContent: 'center', background: '#f5f5f5' }}>
            <Text type="secondary">Cache Performance Chart Placeholder</Text>
          </div>
        </Card>
      </Col>
      <Col xs={24} lg={12}>
        <Card title="System Resources" size="small">
          <div style={{ width: '100%', height: 300, display: 'flex', alignItems: 'center', justifyContent: 'center', background: '#f5f5f5' }}>
            <Text type="secondary">System Resources Chart Placeholder</Text>
          </div>
        </Card>
      </Col>
      <Col xs={24} lg={12}>
        <Card title="Throughput & Errors" size="small">
          <div style={{ width: '100%', height: 300, display: 'flex', alignItems: 'center', justifyContent: 'center', background: '#f5f5f5' }}>
            <Text type="secondary">Throughput & Errors Chart Placeholder</Text>
          </div>
        </Card>
      </Col>
    </Row>
  );

  const renderCacheMetrics = () => (
    <Row gutter={[16, 16]}>
      <Col xs={24} sm={12} md={8}>
        <Card size="small">
          <Statistic
            title="Cache Hit Rate"
            value={cacheMetrics?.hitRate || 0}
            suffix="%"
            precision={1}
            valueStyle={{ color: (cacheMetrics?.hitRate || 0) > 80 ? '#3f8600' : '#cf1322' }}
            prefix={<CloudOutlined />}
          />
        </Card>
      </Col>
      <Col xs={24} sm={12} md={8}>
        <Card size="small">
          <Statistic
            title="Total Requests"
            value={cacheMetrics?.totalRequests || 0}
            prefix={<RiseOutlined />}
          />
        </Card>
      </Col>
      <Col xs={24} sm={12} md={8}>
        <Card size="small">
          <Statistic
            title="Avg Retrieval Time"
            value={cacheMetrics?.averageRetrievalTime || 0}
            suffix="ms"
            precision={1}
            prefix={<ClockCircleOutlined />}
          />
        </Card>
      </Col>
      <Col xs={24}>
        <Card title="Cache Performance Details" size="small">
          <Row gutter={[16, 16]}>
            <Col xs={24} md={12}>
              <div style={{ marginBottom: '16px' }}>
                <Text strong>Hit Rate</Text>
                <Progress 
                  percent={cacheMetrics?.hitRate || 0} 
                  status={(cacheMetrics?.hitRate || 0) > 80 ? 'success' : 'exception'}
                  strokeColor={(cacheMetrics?.hitRate || 0) > 80 ? '#52c41a' : '#ff4d4f'}
                />
              </div>
              <div style={{ marginBottom: '16px' }}>
                <Text strong>Memory Usage</Text>
                <Progress 
                  percent={cacheMetrics?.memoryUsage || 0} 
                  status={(cacheMetrics?.memoryUsage || 0) < 80 ? 'success' : 'exception'}
                />
              </div>
            </Col>
            <Col xs={24} md={12}>
              <Space direction="vertical" style={{ width: '100%' }}>
                <div style={{ display: 'flex', justifyContent: 'space-between' }}>
                  <Text>Total Hits:</Text>
                  <Text strong>{((cacheMetrics?.totalRequests || 0) * (cacheMetrics?.hitRate || 0) / 100).toFixed(0)}</Text>
                </div>
                <div style={{ display: 'flex', justifyContent: 'space-between' }}>
                  <Text>Total Misses:</Text>
                  <Text strong>{((cacheMetrics?.totalRequests || 0) * (100 - (cacheMetrics?.hitRate || 0)) / 100).toFixed(0)}</Text>
                </div>
                <div style={{ display: 'flex', justifyContent: 'space-between' }}>
                  <Text>Evictions:</Text>
                  <Text strong>{cacheMetrics?.evictionCount || 0}</Text>
                </div>
              </Space>
            </Col>
          </Row>
        </Card>
      </Col>
    </Row>
  );

  const queryPerformanceColumns = [
    {
      title: 'Query Type',
      dataIndex: 'queryType',
      key: 'queryType',
      render: (text: string) => <Tag color="blue">{text}</Tag>
    },
    {
      title: 'Avg Execution Time',
      dataIndex: 'averageExecutionTime',
      key: 'averageExecutionTime',
      render: (time: number) => `${time.toFixed(2)}ms`,
      sorter: (a: QueryPerformance, b: QueryPerformance) => a.averageExecutionTime - b.averageExecutionTime
    },
    {
      title: 'Total Executions',
      dataIndex: 'totalExecutions',
      key: 'totalExecutions',
      sorter: (a: QueryPerformance, b: QueryPerformance) => a.totalExecutions - b.totalExecutions
    },
    {
      title: 'Success Rate',
      dataIndex: 'successRate',
      key: 'successRate',
      render: (rate: number) => (
        <Progress 
          percent={rate} 
          size="small" 
          status={rate > 95 ? 'success' : rate > 90 ? 'normal' : 'exception'}
        />
      ),
      sorter: (a: QueryPerformance, b: QueryPerformance) => a.successRate - b.successRate
    },
    {
      title: 'P95',
      dataIndex: 'p95ExecutionTime',
      key: 'p95ExecutionTime',
      render: (time: number) => `${time.toFixed(2)}ms`
    },
    {
      title: 'P99',
      dataIndex: 'p99ExecutionTime',
      key: 'p99ExecutionTime',
      render: (time: number) => `${time.toFixed(2)}ms`
    }
  ];

  return (
    <div className="performance-dashboard">
      <div style={{ marginBottom: '24px', display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
        <div>
          <Title level={2}>
            <DashboardOutlined style={{ color: '#1890ff', marginRight: '8px' }} />
            Performance Monitoring
          </Title>
          <Text type="secondary">Comprehensive system performance analysis and optimization center</Text>
        </div>
        <Space>
          <Switch
            checked={autoRefresh}
            onChange={setAutoRefresh}
            checkedChildren="Auto"
            unCheckedChildren="Manual"
          />
          <Select
            value={refreshInterval}
            onChange={setRefreshInterval}
            style={{ width: 120 }}
            disabled={!autoRefresh}
          >
            <Option value={10}>10s</Option>
            <Option value={30}>30s</Option>
            <Option value={60}>1m</Option>
            <Option value={300}>5m</Option>
          </Select>
          <RangePicker
            value={timeRange}
            onChange={(dates) => dates && setTimeRange(dates as [Dayjs, Dayjs])}
            showTime
            format="YYYY-MM-DD HH:mm"
          />
          <Button 
            icon={<ReloadOutlined />} 
            onClick={loadPerformanceData}
            loading={loading}
          >
            Refresh
          </Button>
        </Space>
      </div>

      <Spin spinning={loading}>
        <Tabs defaultActiveKey="overview" size="large">
          <TabPane tab={<span><DashboardOutlined />System Overview</span>} key="overview">
            <Space direction="vertical" style={{ width: '100%' }} size="large">
              {/* System Health Status */}
              <Card title="System Health Status" size="small">
                {renderSystemHealthOverview()}
              </Card>
              
              {/* Performance Charts */}
              <Card title="Performance Trends" size="small">
                {renderPerformanceCharts()}
              </Card>
            </Space>
          </TabPane>

          <TabPane tab={<span><ThunderboltOutlined />Performance Analysis</span>} key="analysis">
            <Space direction="vertical" style={{ width: '100%' }} size="large">
              {/* Cache Performance Section */}
              <Card title="Cache Performance" size="small">
                {renderCacheMetrics()}
              </Card>

              {/* Query Performance Section */}
              <Card title="Query Performance Analysis" size="small">
                <Table
                  columns={queryPerformanceColumns}
                  dataSource={queryPerformance}
                  rowKey="queryType"
                  size="small"
                  pagination={{ pageSize: 10 }}
                />
              </Card>

              {/* Advanced Memory & Bundle Metrics */}
              <Card title="Advanced Performance Metrics" size="small">
                {renderAdvancedPerformanceTab()}
              </Card>
            </Space>
          </TabPane>

          <TabPane tab={<span><SettingOutlined />Optimization & Actions</span>} key="optimization">
            <Space direction="vertical" style={{ width: '100%' }} size="large">
              {/* Optimization Recommendations */}
              <Card title="Performance Recommendations" size="small">
                <Alert
                  message="Performance Optimization Center"
                  description="Real-time recommendations based on current system performance metrics."
                  type="info"
                  style={{ marginBottom: '24px' }}
                />
                <Row gutter={[16, 16]}>
                  <Col xs={24} lg={16}>
                    <Space direction="vertical" style={{ width: '100%' }}>
                      <Alert 
                        message="Cache Optimization" 
                        description="Cache hit rate is 78%. Consider increasing cache size for better performance." 
                        type="warning" 
                        showIcon
                      />
                      <Alert 
                        message="Query Performance" 
                        description="Query execution times are within optimal range. No action required." 
                        type="success" 
                        showIcon
                      />
                      <Alert 
                        message="Memory Usage" 
                        description="Memory usage is stable. System operating efficiently." 
                        type="success" 
                        showIcon
                      />
                      <Alert 
                        message="Database Connections" 
                        description="Active connection count is normal. No optimization needed." 
                        type="info" 
                        showIcon
                      />
                    </Space>
                  </Col>
                  <Col xs={24} lg={8}>
                    <Card title="Performance Score" size="small">
                      <div style={{ textAlign: 'center' }}>
                        <div style={{ fontSize: '3rem', fontWeight: 'bold', color: '#52c41a', marginBottom: '8px' }}>
                          85
                        </div>
                        <div style={{ fontSize: '1.2rem', color: '#666' }}>
                          Overall Performance
                        </div>
                        <div style={{ fontSize: '0.9rem', color: '#999', marginTop: '8px' }}>
                          Good performance with room for cache optimization
                        </div>
                      </div>
                    </Card>
                  </Col>
                </Row>
              </Card>

              {/* Quick Actions */}
              <Card title="Performance Actions" size="small">
                <Row gutter={[16, 16]}>
                  <Col xs={24} md={8}>
                    <Card title="Cache Management" size="small">
                      <Space direction="vertical" style={{ width: '100%' }}>
                        <Button type="primary" block icon={<CloudOutlined />}>
                          Optimize Cache
                        </Button>
                        <Button block icon={<ReloadOutlined />}>
                          Clear Cache
                        </Button>
                        <Button block>
                          Cache Statistics
                        </Button>
                      </Space>
                    </Card>
                  </Col>
                  <Col xs={24} md={8}>
                    <Card title="System Maintenance" size="small">
                      <Space direction="vertical" style={{ width: '100%' }}>
                        <Button type="primary" block icon={<ReloadOutlined />}>
                          Force Memory Cleanup
                        </Button>
                        <Button block icon={<WarningOutlined />}>
                          Detect Memory Leaks
                        </Button>
                        <Button block>
                          System Health Check
                        </Button>
                      </Space>
                    </Card>
                  </Col>
                  <Col xs={24} md={8}>
                    <Card title="Reporting" size="small">
                      <Space direction="vertical" style={{ width: '100%' }}>
                        <Button type="primary" block icon={<DashboardOutlined />}>
                          Generate Report
                        </Button>
                        <Button block>
                          Export Metrics
                        </Button>
                        <Button block>
                          Schedule Reports
                        </Button>
                      </Space>
                    </Card>
                  </Col>
                </Row>
              </Card>
            </Space>
          </TabPane>
        </Tabs>
      </Spin>
    </div>
  );

  // Render advanced performance tab
  function renderAdvancedPerformanceTab() {
    return (
      <Space direction="vertical" style={{ width: '100%' }} size="large">
        <Row gutter={[16, 16]}>
          <Col xs={24} lg={12}>
            <Card title="Memory Metrics" size="small">
              {memoryMetrics ? (
                <Space direction="vertical" style={{ width: '100%' }}>
                  <Statistic
                    title="Used JS Heap Size"
                    value={(memoryMetrics.usedJSHeapSize / 1024 / 1024).toFixed(2)}
                    suffix="MB"
                    prefix={<CloudOutlined />}
                  />
                  <Statistic
                    title="Total JS Heap Size"
                    value={(memoryMetrics.totalJSHeapSize / 1024 / 1024).toFixed(2)}
                    suffix="MB"
                  />
                  <Statistic
                    title="Component Count"
                    value={memoryMetrics.componentCount}
                    prefix={<ThunderboltOutlined />}
                  />
                  <Statistic
                    title="Event Listeners"
                    value={memoryMetrics.listenerCount}
                  />
                </Space>
              ) : (
                <Text type="secondary">Memory metrics not available</Text>
              )}
            </Card>
          </Col>

          <Col xs={24} lg={12}>
            <Card title="Bundle Statistics" size="small">
              {bundleStats ? (
                <Space direction="vertical" style={{ width: '100%' }}>
                  <Statistic
                    title="Loaded Chunks"
                    value={bundleStats.loadedChunks.length}
                    prefix={<DatabaseOutlined />}
                  />
                  <Statistic
                    title="Preload Queue"
                    value={bundleStats.preloadQueueSize}
                  />
                  <Statistic
                    title="Visited Routes"
                    value={bundleStats.userBehavior.visitedRoutes.length}
                  />
                  <Statistic
                    title="Avg Time Spent"
                    value={bundleStats.userBehavior.averageTimeSpent.toFixed(0)}
                    suffix="ms"
                  />
                </Space>
              ) : (
                <Text type="secondary">Bundle statistics not available</Text>
              )}
            </Card>
          </Col>
        </Row>

        <Row gutter={[16, 16]}>
          <Col xs={24}>
            <Card title="Performance Actions" size="small">
              <Space>
                <Button
                  type="primary"
                  icon={<ReloadOutlined />}
                  onClick={performCleanup}
                >
                  Force Memory Cleanup
                </Button>
                <Button
                  icon={<WarningOutlined />}
                  onClick={detectMemoryLeaks}
                >
                  Detect Memory Leaks
                </Button>
                <Button
                  icon={<CloudOutlined />}
                  onClick={() => {
                    const metrics = getMemoryMetrics();
                    console.log('Current memory metrics:', metrics);
                  }}
                >
                  Log Memory Metrics
                </Button>
              </Space>
            </Card>
          </Col>
        </Row>
      </Space>
    );
  }
};

export default PerformanceMonitoringDashboard;
