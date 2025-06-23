import React, { useState } from 'react';
import { 
  Card, 
  Row, 
  Col, 
  Statistic, 
  Select, 
  DatePicker, 
  Space, 
  Alert,
  Spin,
  Typography,
  Button,
  Tooltip,
  Progress,
  Tag,
  Tabs
} from 'antd';
import {
  DollarOutlined,
  ThunderboltOutlined,
  ClockCircleOutlined,
  CheckCircleOutlined,
  ExclamationCircleOutlined,
  ReloadOutlined,
  DownloadOutlined,
  BarChartOutlined
} from '@ant-design/icons';
import dayjs from 'dayjs';
import {
  useTokenUsageDashboard,
  useAnalyticsDashboard,
  usePerformanceOverview,
  useGetRealTimeMetricsQuery,
  useExportAnalyticsDataMutation
} from '@shared/store/api/analyticsApi';
import type { TokenUsageFilters } from '@shared/types/transparency';

const { Title, Text } = Typography;
const { RangePicker } = DatePicker;
const { TabPane } = Tabs;

export interface ProcessFlowAnalyticsDashboardProps {
  defaultTimeRange?: [dayjs.Dayjs, dayjs.Dayjs];
  showRealTime?: boolean;
  compact?: boolean;
  className?: string;
  testId?: string;
}

/**
 * ProcessFlowAnalyticsDashboard - Comprehensive analytics dashboard for ProcessFlow system
 * 
 * Features:
 * - Token usage and cost analytics
 * - Performance metrics and trends
 * - Real-time monitoring
 * - Session analytics
 * - Export capabilities
 */
export const ProcessFlowAnalyticsDashboard: React.FC<ProcessFlowAnalyticsDashboardProps> = ({
  defaultTimeRange = [dayjs().subtract(7, 'days'), dayjs()],
  showRealTime = true,
  compact = false,
  className,
  testId = 'processflow-analytics-dashboard'
}) => {
  const [timeRange, setTimeRange] = useState(defaultTimeRange);
  const [filters, setFilters] = useState<TokenUsageFilters>({
    startDate: timeRange[0].toISOString(),
    endDate: timeRange[1].toISOString()
  });

  // API hooks
  const tokenDashboard = useTokenUsageDashboard(filters);
  const analyticsDashboard = useAnalyticsDashboard({
    startDate: filters.startDate,
    endDate: filters.endDate,
    includeRealTime: showRealTime
  });
  const performanceOverview = usePerformanceOverview(7);
  const { data: realTimeMetrics } = useGetRealTimeMetricsQuery(undefined, {
    pollingInterval: showRealTime ? 5000 : 0,
    skip: !showRealTime
  });

  const [exportAnalytics] = useExportAnalyticsDataMutation();

  const handleTimeRangeChange = (dates: [dayjs.Dayjs, dayjs.Dayjs] | null) => {
    if (dates) {
      setTimeRange(dates);
      setFilters({
        ...filters,
        startDate: dates[0].toISOString(),
        endDate: dates[1].toISOString()
      });
    }
  };

  const handleExport = async (format: 'json' | 'csv' | 'excel') => {
    try {
      await exportAnalytics({
        format,
        includeSessionDetails: true,
        includeStepBreakdown: true,
        includeTokenUsage: true,
        includeErrorLogs: false,
        dateRange: {
          start: filters.startDate!,
          end: filters.endDate!
        }
      }).unwrap();
    } catch (error) {
      console.error('Export failed:', error);
    }
  };

  const isLoading = tokenDashboard.isLoading || analyticsDashboard.isLoading || performanceOverview.isLoading;
  const hasError = tokenDashboard.error || analyticsDashboard.error || performanceOverview.error;

  if (hasError) {
    return (
      <Alert
        message="Error Loading Analytics"
        description="Failed to load analytics data. Please try again."
        type="error"
        showIcon
        action={
          <Button 
            size="small" 
            onClick={() => {
              tokenDashboard.refetch();
              analyticsDashboard.refetch();
              performanceOverview.refetch();
            }}
          >
            Retry
          </Button>
        }
      />
    );
  }

  return (
    <div className={className} data-testid={testId}>
      {/* Header */}
      <Card style={{ marginBottom: 16 }}>
        <Row justify="space-between" align="middle">
          <Col>
            <Title level={3} style={{ margin: 0 }}>
              <BarChartOutlined /> ProcessFlow Analytics
            </Title>
            <Text type="secondary">
              Comprehensive analytics for AI query processing
            </Text>
          </Col>
          <Col>
            <Space>
              <RangePicker
                value={timeRange}
                onChange={handleTimeRangeChange}
                presets={[
                  { label: 'Last 24 Hours', value: [dayjs().subtract(1, 'day'), dayjs()] },
                  { label: 'Last 7 Days', value: [dayjs().subtract(7, 'days'), dayjs()] },
                  { label: 'Last 30 Days', value: [dayjs().subtract(30, 'days'), dayjs()] },
                  { label: 'Last 90 Days', value: [dayjs().subtract(90, 'days'), dayjs()] }
                ]}
              />
              <Button
                icon={<DownloadOutlined />}
                onClick={() => handleExport('excel')}
              >
                Export
              </Button>
              <Button
                icon={<ReloadOutlined />}
                onClick={() => {
                  tokenDashboard.refetch();
                  analyticsDashboard.refetch();
                  performanceOverview.refetch();
                }}
              />
            </Space>
          </Col>
        </Row>
      </Card>

      {/* Real-time Status */}
      {showRealTime && realTimeMetrics && (
        <Card style={{ marginBottom: 16 }}>
          <Title level={5}>Real-time Status</Title>
          <Row gutter={[16, 8]}>
            <Col span={compact ? 12 : 6}>
              <Statistic
                title="Active Sessions"
                value={realTimeMetrics.activeSessions}
                prefix={<ThunderboltOutlined />}
                valueStyle={{ color: '#1890ff' }}
              />
            </Col>
            <Col span={compact ? 12 : 6}>
              <Statistic
                title="Throughput"
                value={realTimeMetrics.currentThroughput}
                suffix="req/min"
                prefix={<ClockCircleOutlined />}
              />
            </Col>
            <Col span={compact ? 12 : 6}>
              <Statistic
                title="Error Rate"
                value={realTimeMetrics.errorRate * 100}
                precision={2}
                suffix="%"
                prefix={<ExclamationCircleOutlined />}
                valueStyle={{ color: realTimeMetrics.errorRate > 0.05 ? '#ff4d4f' : '#52c41a' }}
              />
            </Col>
            <Col span={compact ? 12 : 6}>
              <div>
                <Text type="secondary" style={{ fontSize: '14px' }}>System Load</Text>
                <div style={{ marginTop: 4 }}>
                  <Progress
                    percent={realTimeMetrics.systemLoad * 100}
                    size="small"
                    status={realTimeMetrics.systemLoad > 0.8 ? 'exception' : 'normal'}
                  />
                </div>
              </div>
            </Col>
          </Row>
        </Card>
      )}

      {/* Key Metrics */}
      <Row gutter={[16, 16]} style={{ marginBottom: 16 }}>
        <Col xs={24} sm={12} lg={6}>
          <Card>
            <Statistic
              title="Total Sessions"
              value={analyticsDashboard.dashboard?.overview.totalSessions || 0}
              loading={isLoading}
              prefix={<CheckCircleOutlined />}
            />
          </Card>
        </Col>
        <Col xs={24} sm={12} lg={6}>
          <Card>
            <Statistic
              title="Success Rate"
              value={analyticsDashboard.dashboard?.overview.successRate || 0}
              precision={1}
              suffix="%"
              loading={isLoading}
              prefix={<CheckCircleOutlined />}
              valueStyle={{ 
                color: (analyticsDashboard.dashboard?.overview.successRate || 0) > 90 ? '#52c41a' : '#faad14' 
              }}
            />
          </Card>
        </Col>
        <Col xs={24} sm={12} lg={6}>
          <Card>
            <Statistic
              title="Total Cost"
              value={tokenDashboard.statistics?.totalCost || 0}
              precision={4}
              prefix={<DollarOutlined />}
              loading={isLoading}
            />
          </Card>
        </Col>
        <Col xs={24} sm={12} lg={6}>
          <Card>
            <Statistic
              title="Avg Confidence"
              value={analyticsDashboard.dashboard?.overview.averageConfidence || 0}
              precision={1}
              suffix="%"
              loading={isLoading}
              prefix={<ThunderboltOutlined />}
            />
          </Card>
        </Col>
      </Row>

      {/* Detailed Analytics Tabs */}
      <Card>
        <Tabs defaultActiveKey="tokens">
          <TabPane tab="Token Usage" key="tokens">
            {isLoading ? (
              <div style={{ textAlign: 'center', padding: '40px' }}>
                <Spin size="large" />
              </div>
            ) : (
              <Row gutter={[16, 16]}>
                <Col span={24}>
                  <Row gutter={[16, 8]}>
                    <Col xs={24} sm={8}>
                      <Statistic
                        title="Total Tokens"
                        value={tokenDashboard.statistics?.totalTokens || 0}
                        formatter={(value) => value.toLocaleString()}
                      />
                    </Col>
                    <Col xs={24} sm={8}>
                      <Statistic
                        title="Avg Tokens/Request"
                        value={tokenDashboard.statistics?.averageTokensPerRequest || 0}
                        precision={0}
                      />
                    </Col>
                    <Col xs={24} sm={8}>
                      <Statistic
                        title="Cost/Request"
                        value={tokenDashboard.statistics?.averageCostPerRequest || 0}
                        precision={4}
                        prefix={<DollarOutlined />}
                      />
                    </Col>
                  </Row>
                </Col>
                
                {/* Top Users */}
                {tokenDashboard.topUsers && tokenDashboard.topUsers.length > 0 && (
                  <Col span={24}>
                    <Title level={5}>Top Token Users</Title>
                    <Space wrap>
                      {tokenDashboard.topUsers.slice(0, 5).map((user, index) => (
                        <Tag key={user.userId} color={index === 0 ? 'gold' : 'blue'}>
                          {user.userName || user.userId}: {user.totalTokens.toLocaleString()} tokens
                        </Tag>
                      ))}
                    </Space>
                  </Col>
                )}
              </Row>
            )}
          </TabPane>

          <TabPane tab="Performance" key="performance">
            {isLoading ? (
              <div style={{ textAlign: 'center', padding: '40px' }}>
                <Spin size="large" />
              </div>
            ) : (
              <Row gutter={[16, 16]}>
                <Col xs={24} sm={8}>
                  <Statistic
                    title="Avg Response Time"
                    value={performanceOverview.performance?.averageSessionDuration || 0}
                    suffix="ms"
                    prefix={<ClockCircleOutlined />}
                  />
                </Col>
                <Col xs={24} sm={8}>
                  <Statistic
                    title="Throughput"
                    value={performanceOverview.performance?.throughput || 0}
                    suffix="req/min"
                    prefix={<ThunderboltOutlined />}
                  />
                </Col>
                <Col xs={24} sm={8}>
                  <Statistic
                    title="Error Rate"
                    value={performanceOverview.performance?.errorRate || 0}
                    precision={2}
                    suffix="%"
                    prefix={<ExclamationCircleOutlined />}
                    valueStyle={{ 
                      color: (performanceOverview.performance?.errorRate || 0) > 5 ? '#ff4d4f' : '#52c41a' 
                    }}
                  />
                </Col>
              </Row>
            )}
          </TabPane>

          <TabPane tab="Cost Analysis" key="cost">
            {isLoading ? (
              <div style={{ textAlign: 'center', padding: '40px' }}>
                <Spin size="large" />
              </div>
            ) : (
              <Row gutter={[16, 16]}>
                <Col xs={24} sm={8}>
                  <Statistic
                    title="Total Cost"
                    value={performanceOverview.cost?.totalCost || 0}
                    precision={4}
                    prefix={<DollarOutlined />}
                  />
                </Col>
                <Col xs={24} sm={8}>
                  <Statistic
                    title="Projected Monthly"
                    value={performanceOverview.cost?.projectedMonthlyCost || 0}
                    precision={2}
                    prefix={<DollarOutlined />}
                  />
                </Col>
                <Col xs={24} sm={8}>
                  <div>
                    <Text type="secondary" style={{ fontSize: '14px' }}>Cost Optimization</Text>
                    <div style={{ marginTop: 4 }}>
                      {performanceOverview.cost?.costOptimizationSuggestions?.length || 0} suggestions
                    </div>
                  </div>
                </Col>
              </Row>
            )}
          </TabPane>
        </Tabs>
      </Card>
    </div>
  );
};

export default ProcessFlowAnalyticsDashboard;
