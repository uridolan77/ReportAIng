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
  Tag,
  Tooltip,
  Switch,
  Select,
  DatePicker,
  Spin,
  Badge
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
  FallOutlined,
  FireOutlined
} from '@ant-design/icons';
import {
  LineChart,
  Line,
  AreaChart,
  Area,
  BarChart,
  Bar,
  XAxis,
  YAxis,
  CartesianGrid,
  Tooltip as RechartsTooltip,
  Legend,
  ResponsiveContainer
} from 'recharts';
import dayjs, { type Dayjs } from 'dayjs';
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
  const [metrics, setMetrics] = useState<PerformanceMetrics[]>([]);
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

    } catch (error) {
      console.error('❌ Error loading performance data:', error);
    } finally {
      setLoading(false);
    }
  }, [timeRange]);

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
          <ResponsiveContainer width="100%" height={300}>
            <LineChart data={metrics}>
              <CartesianGrid strokeDasharray="3 3" />
              <XAxis dataKey="timestamp" tickFormatter={(value) => dayjs(value).format('HH:mm')} />
              <YAxis />
              <RechartsTooltip labelFormatter={(value) => dayjs(value).format('HH:mm:ss')} />
              <Legend />
              <Line type="monotone" dataKey="queryExecutionTime" stroke="#1890ff" strokeWidth={2} name="Avg Execution Time (ms)" />
            </LineChart>
          </ResponsiveContainer>
        </Card>
      </Col>
      <Col xs={24} lg={12}>
        <Card title="Cache Performance" size="small">
          <ResponsiveContainer width="100%" height={300}>
            <AreaChart data={metrics}>
              <CartesianGrid strokeDasharray="3 3" />
              <XAxis dataKey="timestamp" tickFormatter={(value) => dayjs(value).format('HH:mm')} />
              <YAxis />
              <RechartsTooltip labelFormatter={(value) => dayjs(value).format('HH:mm:ss')} />
              <Legend />
              <Area type="monotone" dataKey="cacheHitRate" stackId="1" stroke="#52c41a" fill="#52c41a" fillOpacity={0.6} name="Cache Hit Rate %" />
            </AreaChart>
          </ResponsiveContainer>
        </Card>
      </Col>
      <Col xs={24} lg={12}>
        <Card title="System Resources" size="small">
          <ResponsiveContainer width="100%" height={300}>
            <LineChart data={metrics}>
              <CartesianGrid strokeDasharray="3 3" />
              <XAxis dataKey="timestamp" tickFormatter={(value) => dayjs(value).format('HH:mm')} />
              <YAxis />
              <RechartsTooltip labelFormatter={(value) => dayjs(value).format('HH:mm:ss')} />
              <Legend />
              <Line type="monotone" dataKey="memoryUsage" stroke="#faad14" strokeWidth={2} name="Memory Usage %" />
              <Line type="monotone" dataKey="cpuUsage" stroke="#ff4d4f" strokeWidth={2} name="CPU Usage %" />
            </LineChart>
          </ResponsiveContainer>
        </Card>
      </Col>
      <Col xs={24} lg={12}>
        <Card title="Throughput & Errors" size="small">
          <ResponsiveContainer width="100%" height={300}>
            <BarChart data={metrics}>
              <CartesianGrid strokeDasharray="3 3" />
              <XAxis dataKey="timestamp" tickFormatter={(value) => dayjs(value).format('HH:mm')} />
              <YAxis />
              <RechartsTooltip labelFormatter={(value) => dayjs(value).format('HH:mm:ss')} />
              <Legend />
              <Bar dataKey="throughput" fill="#1890ff" name="Throughput (req/min)" />
              <Bar dataKey="errorRate" fill="#ff4d4f" name="Error Rate %" />
            </BarChart>
          </ResponsiveContainer>
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
            Performance Monitoring Dashboard
          </Title>
          <Text type="secondary">Real-time system performance and optimization insights</Text>
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
          <TabPane tab={<span><DashboardOutlined />Overview</span>} key="overview">
            <Space direction="vertical" style={{ width: '100%' }} size="large">
              {renderSystemHealthOverview()}
              {renderPerformanceCharts()}
            </Space>
          </TabPane>

          <TabPane tab={<span><CloudOutlined />Cache Performance</span>} key="cache">
            {renderCacheMetrics()}
          </TabPane>

          <TabPane tab={<span><ThunderboltOutlined />Query Performance</span>} key="queries">
            <Card title="Query Performance Analysis" size="small">
              <Table
                columns={queryPerformanceColumns}
                dataSource={queryPerformance}
                rowKey="queryType"
                size="small"
                pagination={{ pageSize: 10 }}
              />
            </Card>
          </TabPane>

          <TabPane tab={<span><SettingOutlined />Optimization</span>} key="optimization">
            <Alert
              message="Performance Optimization"
              description="Advanced optimization features and recommendations will be displayed here."
              type="info"
              style={{ marginBottom: '24px' }}
            />
            <Row gutter={[16, 16]}>
              <Col xs={24} md={12}>
                <Card title="Optimization Recommendations" size="small">
                  <Space direction="vertical" style={{ width: '100%' }}>
                    <Alert message="Cache hit rate is below 80%. Consider increasing cache size." type="warning" />
                    <Alert message="Query execution time is optimal." type="success" />
                    <Alert message="Memory usage is within normal range." type="success" />
                  </Space>
                </Card>
              </Col>
              <Col xs={24} md={12}>
                <Card title="Quick Actions" size="small">
                  <Space direction="vertical" style={{ width: '100%' }}>
                    <Button type="primary" block>Optimize Cache</Button>
                    <Button block>Clear Cache</Button>
                    <Button block>Restart Services</Button>
                    <Button block>Generate Report</Button>
                  </Space>
                </Card>
              </Col>
            </Row>
          </TabPane>
        </Tabs>
      </Spin>
    </div>
  );
};

export default PerformanceMonitoringDashboard;
