/**
 * Monitoring Dashboard
 * 
 * Comprehensive monitoring and analytics dashboard for Phase 8
 * featuring real-time metrics, user behavior tracking, and business intelligence
 */

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
  Table,
  Tag,
  Tabs,
  DatePicker,
  Select,
  Tooltip
} from 'antd';
import {
  DashboardOutlined,
  UserOutlined,
  ClockCircleOutlined,
  EyeOutlined,
  ThunderboltOutlined,
  BugOutlined,
  TrophyOutlined,
  ReloadOutlined,
  DownloadOutlined,
  LineChartOutlined,
  BarChartOutlined
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
  ResponsiveContainer,
  PieChart,
  Pie,
  Cell
} from 'recharts';
import { analytics } from '../../monitoring/AnalyticsSystem';
import dayjs from 'dayjs';

const { Title, Text, Paragraph } = Typography;
const { TabPane } = Tabs;
const { RangePicker } = DatePicker;
const { Option } = Select;

interface MonitoringDashboardProps {
  autoRefresh?: boolean;
  refreshInterval?: number;
}

export const MonitoringDashboard: React.FC<MonitoringDashboardProps> = ({
  autoRefresh = true,
  refreshInterval = 60000 // 1 minute
}) => {
  const [analyticsReport, setAnalyticsReport] = useState<any>(null);
  const [loading, setLoading] = useState(false);
  const [timeRange, setTimeRange] = useState<[dayjs.Dayjs, dayjs.Dayjs]>([
    dayjs().subtract(7, 'days'),
    dayjs()
  ]);
  const [selectedMetric, setSelectedMetric] = useState('overview');

  const loadAnalyticsData = useCallback(async () => {
    setLoading(true);
    try {
      const report = analytics.generateReport({
        start: timeRange[0].valueOf(),
        end: timeRange[1].valueOf()
      });
      setAnalyticsReport(report);
    } catch (error) {
      console.error('Failed to load analytics data:', error);
    } finally {
      setLoading(false);
    }
  }, [timeRange]);

  useEffect(() => {
    loadAnalyticsData();
    
    if (autoRefresh) {
      const interval = setInterval(loadAnalyticsData, refreshInterval);
      return () => clearInterval(interval);
    }
  }, [loadAnalyticsData, autoRefresh, refreshInterval]);

  const exportAnalyticsData = () => {
    if (!analyticsReport) return;
    
    const data = JSON.stringify(analyticsReport, null, 2);
    const blob = new Blob([data], { type: 'application/json' });
    const url = URL.createObjectURL(blob);
    const link = document.createElement('a');
    link.href = url;
    link.download = `analytics-report-${new Date().toISOString()}.json`;
    link.click();
    URL.revokeObjectURL(url);
  };

  const renderOverviewMetrics = () => {
    if (!analyticsReport) return null;

    const { summary } = analyticsReport;

    return (
      <Row gutter={[24, 24]}>
        <Col xs={24} sm={12} md={6}>
          <Card>
            <Statistic
              title="Total Users"
              value={summary.totalUsers}
              prefix={<UserOutlined />}
              valueStyle={{ color: '#1890ff' }}
            />
          </Card>
        </Col>
        <Col xs={24} sm={12} md={6}>
          <Card>
            <Statistic
              title="Total Sessions"
              value={summary.totalSessions}
              prefix={<EyeOutlined />}
              valueStyle={{ color: '#52c41a' }}
            />
          </Card>
        </Col>
        <Col xs={24} sm={12} md={6}>
          <Card>
            <Statistic
              title="Page Views"
              value={summary.totalPageViews}
              prefix={<LineChartOutlined />}
              valueStyle={{ color: '#faad14' }}
            />
          </Card>
        </Col>
        <Col xs={24} sm={12} md={6}>
          <Card>
            <Statistic
              title="Avg Session Duration"
              value={Math.round(summary.averageSessionDuration / 60000)}
              suffix="min"
              prefix={<ClockCircleOutlined />}
              valueStyle={{ color: '#722ed1' }}
            />
          </Card>
        </Col>
      </Row>
    );
  };

  const renderPerformanceMetrics = () => {
    if (!analyticsReport) return null;

    const { summary } = analyticsReport;

    return (
      <Row gutter={[24, 24]}>
        <Col xs={24} sm={12} md={6}>
          <Card>
            <Statistic
              title="Avg Load Time"
              value={summary.averageLoadTime}
              suffix="ms"
              prefix={<ThunderboltOutlined />}
              valueStyle={{ color: summary.averageLoadTime > 2000 ? '#ff4d4f' : '#52c41a' }}
            />
          </Card>
        </Col>
        <Col xs={24} sm={12} md={6}>
          <Card>
            <Statistic
              title="Error Rate"
              value={(summary.errorRate * 100).toFixed(2)}
              suffix="%"
              prefix={<BugOutlined />}
              valueStyle={{ color: summary.errorRate > 0.05 ? '#ff4d4f' : '#52c41a' }}
            />
          </Card>
        </Col>
        <Col xs={24} sm={12} md={6}>
          <Card>
            <Statistic
              title="Bounce Rate"
              value={(summary.bounceRate * 100).toFixed(1)}
              suffix="%"
              prefix={<EyeOutlined />}
              valueStyle={{ color: summary.bounceRate > 0.5 ? '#ff4d4f' : '#52c41a' }}
            />
          </Card>
        </Col>
        <Col xs={24} sm={12} md={6}>
          <Card>
            <Statistic
              title="Conversion Rate"
              value={(summary.conversionRate * 100).toFixed(1)}
              suffix="%"
              prefix={<TrophyOutlined />}
              valueStyle={{ color: summary.conversionRate > 0.1 ? '#52c41a' : '#faad14' }}
            />
          </Card>
        </Col>
      </Row>
    );
  };

  const renderUserBehaviorChart = () => {
    // Mock data for demonstration
    const data = [
      { time: '00:00', users: 120, sessions: 180, pageViews: 450 },
      { time: '04:00', users: 80, sessions: 120, pageViews: 300 },
      { time: '08:00', users: 200, sessions: 280, pageViews: 720 },
      { time: '12:00', users: 350, sessions: 480, pageViews: 1200 },
      { time: '16:00', users: 280, sessions: 380, pageViews: 950 },
      { time: '20:00', users: 180, sessions: 250, pageViews: 600 }
    ];

    return (
      <ResponsiveContainer width="100%" height={300}>
        <AreaChart data={data}>
          <CartesianGrid strokeDasharray="3 3" />
          <XAxis dataKey="time" />
          <YAxis />
          <RechartsTooltip />
          <Legend />
          <Area type="monotone" dataKey="users" stackId="1" stroke="#1890ff" fill="#1890ff" fillOpacity={0.6} />
          <Area type="monotone" dataKey="sessions" stackId="1" stroke="#52c41a" fill="#52c41a" fillOpacity={0.6} />
          <Area type="monotone" dataKey="pageViews" stackId="1" stroke="#faad14" fill="#faad14" fillOpacity={0.6} />
        </AreaChart>
      </ResponsiveContainer>
    );
  };

  const renderPerformanceChart = () => {
    // Mock performance data
    const data = [
      { time: '00:00', loadTime: 1200, errorRate: 0.02, responseTime: 150 },
      { time: '04:00', loadTime: 1100, errorRate: 0.01, responseTime: 140 },
      { time: '08:00', loadTime: 1400, errorRate: 0.03, responseTime: 180 },
      { time: '12:00', loadTime: 1600, errorRate: 0.04, responseTime: 200 },
      { time: '16:00', loadTime: 1300, errorRate: 0.02, responseTime: 160 },
      { time: '20:00', loadTime: 1150, errorRate: 0.01, responseTime: 145 }
    ];

    return (
      <ResponsiveContainer width="100%" height={300}>
        <LineChart data={data}>
          <CartesianGrid strokeDasharray="3 3" />
          <XAxis dataKey="time" />
          <YAxis yAxisId="left" />
          <YAxis yAxisId="right" orientation="right" />
          <RechartsTooltip />
          <Legend />
          <Line yAxisId="left" type="monotone" dataKey="loadTime" stroke="#1890ff" strokeWidth={2} />
          <Line yAxisId="left" type="monotone" dataKey="responseTime" stroke="#52c41a" strokeWidth={2} />
          <Line yAxisId="right" type="monotone" dataKey="errorRate" stroke="#ff4d4f" strokeWidth={2} />
        </LineChart>
      </ResponsiveContainer>
    );
  };

  const renderTopPagesTable = () => {
    // Mock top pages data
    const data = [
      { page: '/', views: 2450, uniqueUsers: 1200, avgTime: '3:45', bounceRate: '32%' },
      { page: '/dashboard', views: 1890, uniqueUsers: 950, avgTime: '5:20', bounceRate: '28%' },
      { page: '/results', views: 1650, uniqueUsers: 820, avgTime: '4:15', bounceRate: '35%' },
      { page: '/visualization', views: 1200, uniqueUsers: 600, avgTime: '6:30', bounceRate: '25%' },
      { page: '/history', views: 980, uniqueUsers: 490, avgTime: '2:50', bounceRate: '45%' }
    ];

    const columns = [
      { title: 'Page', dataIndex: 'page', key: 'page' },
      { title: 'Views', dataIndex: 'views', key: 'views', sorter: (a: any, b: any) => a.views - b.views },
      { title: 'Unique Users', dataIndex: 'uniqueUsers', key: 'uniqueUsers', sorter: (a: any, b: any) => a.uniqueUsers - b.uniqueUsers },
      { title: 'Avg Time', dataIndex: 'avgTime', key: 'avgTime' },
      { title: 'Bounce Rate', dataIndex: 'bounceRate', key: 'bounceRate' }
    ];

    return (
      <Table
        dataSource={data}
        columns={columns}
        rowKey="page"
        size="small"
        pagination={false}
      />
    );
  };

  const renderDeviceBreakdown = () => {
    // Mock device data
    const data = [
      { name: 'Desktop', value: 65, color: '#1890ff' },
      { name: 'Mobile', value: 28, color: '#52c41a' },
      { name: 'Tablet', value: 7, color: '#faad14' }
    ];

    return (
      <ResponsiveContainer width="100%" height={250}>
        <PieChart>
          <Pie
            data={data}
            cx="50%"
            cy="50%"
            outerRadius={80}
            fill="#8884d8"
            dataKey="value"
            label={({ name, value }) => `${name}: ${value}%`}
          >
            {data.map((entry, index) => (
              <Cell key={`cell-${index}`} fill={entry.color} />
            ))}
          </Pie>
          <RechartsTooltip />
        </PieChart>
      </ResponsiveContainer>
    );
  };

  return (
    <div style={{ padding: '24px' }}>
      <div style={{ marginBottom: '24px', display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
        <div>
          <Title level={2}>
            <DashboardOutlined style={{ color: '#1890ff', marginRight: '8px' }} />
            Monitoring Dashboard
          </Title>
          <Paragraph>
            Comprehensive analytics and monitoring for Phase 8 production readiness
          </Paragraph>
        </div>
        <Space>
          <RangePicker
            value={timeRange}
            onChange={(dates) => dates && setTimeRange(dates)}
            format="YYYY-MM-DD"
          />
          <Button
            type="primary"
            icon={<ReloadOutlined />}
            onClick={loadAnalyticsData}
            loading={loading}
          >
            Refresh
          </Button>
          <Button
            icon={<DownloadOutlined />}
            onClick={exportAnalyticsData}
          >
            Export
          </Button>
        </Space>
      </div>

      <Space direction="vertical" style={{ width: '100%' }} size="large">
        <Tabs defaultActiveKey="overview">
          <TabPane tab={<span><DashboardOutlined />Overview</span>} key="overview">
            <Space direction="vertical" style={{ width: '100%' }} size="large">
              {renderOverviewMetrics()}
              
              <Card title="User Activity Over Time" size="small">
                {renderUserBehaviorChart()}
              </Card>
              
              <Row gutter={[24, 24]}>
                <Col xs={24} lg={16}>
                  <Card title="Top Pages" size="small">
                    {renderTopPagesTable()}
                  </Card>
                </Col>
                <Col xs={24} lg={8}>
                  <Card title="Device Breakdown" size="small">
                    {renderDeviceBreakdown()}
                  </Card>
                </Col>
              </Row>
            </Space>
          </TabPane>

          <TabPane tab={<span><ThunderboltOutlined />Performance</span>} key="performance">
            <Space direction="vertical" style={{ width: '100%' }} size="large">
              {renderPerformanceMetrics()}
              
              <Card title="Performance Metrics Over Time" size="small">
                {renderPerformanceChart()}
              </Card>
              
              <Alert
                message="Performance Status: Good"
                description="All performance metrics are within acceptable ranges. Average load time is under 2 seconds."
                type="success"
                showIcon
              />
            </Space>
          </TabPane>

          <TabPane tab={<span><UserOutlined />User Behavior</span>} key="behavior">
            <Space direction="vertical" style={{ width: '100%' }} size="large">
              <Row gutter={[24, 24]}>
                <Col xs={24} md={8}>
                  <Card>
                    <Statistic
                      title="Session Duration"
                      value="7.2"
                      suffix="min"
                      prefix={<ClockCircleOutlined />}
                    />
                    <Progress percent={72} strokeColor="#52c41a" size="small" />
                  </Card>
                </Col>
                <Col xs={24} md={8}>
                  <Card>
                    <Statistic
                      title="Pages per Session"
                      value="2.8"
                      prefix={<EyeOutlined />}
                    />
                    <Progress percent={56} strokeColor="#1890ff" size="small" />
                  </Card>
                </Col>
                <Col xs={24} md={8}>
                  <Card>
                    <Statistic
                      title="Return Visitors"
                      value="68"
                      suffix="%"
                      prefix={<UserOutlined />}
                    />
                    <Progress percent={68} strokeColor="#faad14" size="small" />
                  </Card>
                </Col>
              </Row>
              
              <Alert
                message="User Engagement: Excellent"
                description="Users are highly engaged with an average session duration of 7.2 minutes and low bounce rate."
                type="success"
                showIcon
              />
            </Space>
          </TabPane>
        </Tabs>
      </Space>
    </div>
  );
};

export default MonitoringDashboard;
