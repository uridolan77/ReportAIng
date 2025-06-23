import React, { useState, useMemo } from 'react';
import { 
  Card, 
  Row, 
  Col, 
  Statistic, 
  Select, 
  DatePicker, 
  Space, 
  Table,
  Tag,
  Typography,
  Button,
  Tooltip,
  Progress,
  Alert
} from 'antd';
import {
  DollarOutlined,
  ThunderboltOutlined,
  TrendingUpOutlined,
  UserOutlined,
  ApiOutlined,
  BarChartOutlined,
  ExportOutlined
} from '@ant-design/icons';
import dayjs from 'dayjs';
import {
  useGetTokenUsageStatisticsQuery,
  useGetDailyTokenUsageQuery,
  useGetTokenUsageTrendsQuery,
  useGetTopTokenUsersQuery,
  useExportAnalyticsDataMutation
} from '@shared/store/api/analyticsApi';
import type { TokenUsageFilters, DailyTokenUsage } from '@shared/types/transparency';

const { Title, Text } = Typography;
const { RangePicker } = DatePicker;
const { Option } = Select;

export interface ProcessFlowTokenUsageProps {
  defaultTimeRange?: [dayjs.Dayjs, dayjs.Dayjs];
  showUserBreakdown?: boolean;
  showModelBreakdown?: boolean;
  compact?: boolean;
  className?: string;
  testId?: string;
}

/**
 * ProcessFlowTokenUsage - Comprehensive token usage analytics for ProcessFlow system
 * 
 * Features:
 * - Token usage statistics and trends
 * - Cost analysis and projections
 * - User and model breakdowns
 * - Daily usage patterns
 * - Export capabilities
 */
export const ProcessFlowTokenUsage: React.FC<ProcessFlowTokenUsageProps> = ({
  defaultTimeRange = [dayjs().subtract(30, 'days'), dayjs()],
  showUserBreakdown = true,
  showModelBreakdown = true,
  compact = false,
  className,
  testId = 'processflow-token-usage'
}) => {
  const [timeRange, setTimeRange] = useState(defaultTimeRange);
  const [selectedModel, setSelectedModel] = useState<string | undefined>();
  const [selectedUser, setSelectedUser] = useState<string | undefined>();

  const filters: TokenUsageFilters = useMemo(() => ({
    startDate: timeRange[0].toISOString(),
    endDate: timeRange[1].toISOString(),
    model: selectedModel,
    userId: selectedUser
  }), [timeRange, selectedModel, selectedUser]);

  // API queries
  const { data: statistics, isLoading: statsLoading, error: statsError } = useGetTokenUsageStatisticsQuery(filters);
  const { data: dailyUsage, isLoading: dailyLoading } = useGetDailyTokenUsageQuery(filters);
  const { data: trends, isLoading: trendsLoading } = useGetTokenUsageTrendsQuery(filters);
  const { data: topUsers, isLoading: usersLoading } = useGetTopTokenUsersQuery({ limit: 10, days: 30 });

  const [exportData] = useExportAnalyticsDataMutation();

  const handleTimeRangeChange = (dates: [dayjs.Dayjs, dayjs.Dayjs] | null) => {
    if (dates) {
      setTimeRange(dates);
    }
  };

  const handleExport = async () => {
    try {
      await exportData({
        format: 'excel',
        includeTokenUsage: true,
        includeSessionDetails: false,
        includeStepBreakdown: false,
        includeErrorLogs: false,
        dateRange: {
          start: filters.startDate!,
          end: filters.endDate!
        },
        filters
      }).unwrap();
    } catch (error) {
      console.error('Export failed:', error);
    }
  };

  // Calculate trends
  const costTrend = useMemo(() => {
    if (!trends || trends.length < 2) return 0;
    const recent = trends.slice(-7).reduce((sum, day) => sum + day.totalCost, 0) / 7;
    const previous = trends.slice(-14, -7).reduce((sum, day) => sum + day.totalCost, 0) / 7;
    return previous > 0 ? ((recent - previous) / previous) * 100 : 0;
  }, [trends]);

  const tokenTrend = useMemo(() => {
    if (!trends || trends.length < 2) return 0;
    const recent = trends.slice(-7).reduce((sum, day) => sum + day.totalTokens, 0) / 7;
    const previous = trends.slice(-14, -7).reduce((sum, day) => sum + day.totalTokens, 0) / 7;
    return previous > 0 ? ((recent - previous) / previous) * 100 : 0;
  }, [trends]);

  // Daily usage table columns
  const dailyUsageColumns = [
    {
      title: 'Date',
      dataIndex: 'date',
      key: 'date',
      render: (date: string) => dayjs(date).format('MMM DD, YYYY'),
      sorter: (a: DailyTokenUsage, b: DailyTokenUsage) => dayjs(a.date).unix() - dayjs(b.date).unix(),
    },
    {
      title: 'Requests',
      dataIndex: 'totalRequests',
      key: 'totalRequests',
      render: (value: number) => value.toLocaleString(),
      sorter: (a: DailyTokenUsage, b: DailyTokenUsage) => a.totalRequests - b.totalRequests,
    },
    {
      title: 'Total Tokens',
      dataIndex: 'totalTokens',
      key: 'totalTokens',
      render: (value: number) => value.toLocaleString(),
      sorter: (a: DailyTokenUsage, b: DailyTokenUsage) => a.totalTokens - b.totalTokens,
    },
    {
      title: 'Prompt Tokens',
      dataIndex: 'promptTokens',
      key: 'promptTokens',
      render: (value: number) => value.toLocaleString(),
    },
    {
      title: 'Completion Tokens',
      dataIndex: 'completionTokens',
      key: 'completionTokens',
      render: (value: number) => value.toLocaleString(),
    },
    {
      title: 'Cost',
      dataIndex: 'totalCost',
      key: 'totalCost',
      render: (value: number) => `$${value.toFixed(4)}`,
      sorter: (a: DailyTokenUsage, b: DailyTokenUsage) => a.totalCost - b.totalCost,
    },
    {
      title: 'Avg Confidence',
      dataIndex: 'averageConfidence',
      key: 'averageConfidence',
      render: (value: number) => (
        <Progress 
          percent={value * 100} 
          size="small" 
          format={(percent) => `${percent?.toFixed(1)}%`}
        />
      ),
    },
    {
      title: 'Intent Type',
      dataIndex: 'intentType',
      key: 'intentType',
      render: (value: string) => <Tag color="blue">{value}</Tag>,
    }
  ];

  // Top users table columns
  const topUsersColumns = [
    {
      title: 'User',
      dataIndex: 'userName',
      key: 'userName',
      render: (name: string, record: any) => (
        <Space>
          <UserOutlined />
          <span>{name || record.userId}</span>
        </Space>
      ),
    },
    {
      title: 'Total Tokens',
      dataIndex: 'totalTokens',
      key: 'totalTokens',
      render: (value: number) => value.toLocaleString(),
      sorter: (a: any, b: any) => a.totalTokens - b.totalTokens,
    },
    {
      title: 'Requests',
      dataIndex: 'requestCount',
      key: 'requestCount',
      render: (value: number) => value.toLocaleString(),
    },
    {
      title: 'Avg Tokens/Request',
      dataIndex: 'averageTokensPerRequest',
      key: 'averageTokensPerRequest',
      render: (value: number) => value.toFixed(0),
    },
    {
      title: 'Total Cost',
      dataIndex: 'totalCost',
      key: 'totalCost',
      render: (value: number) => `$${value.toFixed(4)}`,
      sorter: (a: any, b: any) => a.totalCost - b.totalCost,
    }
  ];

  if (statsError) {
    return (
      <Alert
        message="Error Loading Token Usage Data"
        description="Failed to load token usage statistics. Please try again."
        type="error"
        showIcon
      />
    );
  }

  return (
    <div className={className} data-testid={testId}>
      {/* Header */}
      <Card style={{ marginBottom: 16 }}>
        <Row justify="space-between" align="middle">
          <Col>
            <Title level={4} style={{ margin: 0 }}>
              <ThunderboltOutlined /> Token Usage Analytics
            </Title>
            <Text type="secondary">
              Comprehensive token usage and cost analysis
            </Text>
          </Col>
          <Col>
            <Space>
              <RangePicker
                value={timeRange}
                onChange={handleTimeRangeChange}
                presets={[
                  { label: 'Last 7 Days', value: [dayjs().subtract(7, 'days'), dayjs()] },
                  { label: 'Last 30 Days', value: [dayjs().subtract(30, 'days'), dayjs()] },
                  { label: 'Last 90 Days', value: [dayjs().subtract(90, 'days'), dayjs()] }
                ]}
              />
              <Select
                placeholder="Filter by model"
                allowClear
                style={{ width: 150 }}
                value={selectedModel}
                onChange={setSelectedModel}
              >
                <Option value="gpt-4">GPT-4</Option>
                <Option value="gpt-3.5-turbo">GPT-3.5 Turbo</Option>
                <Option value="claude-3">Claude 3</Option>
              </Select>
              <Button
                icon={<ExportOutlined />}
                onClick={handleExport}
              >
                Export
              </Button>
            </Space>
          </Col>
        </Row>
      </Card>

      {/* Key Metrics */}
      <Row gutter={[16, 16]} style={{ marginBottom: 16 }}>
        <Col xs={24} sm={12} lg={6}>
          <Card>
            <Statistic
              title="Total Requests"
              value={statistics?.totalRequests || 0}
              loading={statsLoading}
              prefix={<ApiOutlined />}
            />
          </Card>
        </Col>
        <Col xs={24} sm={12} lg={6}>
          <Card>
            <Statistic
              title="Total Tokens"
              value={statistics?.totalTokens || 0}
              loading={statsLoading}
              formatter={(value) => value.toLocaleString()}
              prefix={<ThunderboltOutlined />}
            />
            {tokenTrend !== 0 && (
              <div style={{ marginTop: 8 }}>
                <Text type={tokenTrend > 0 ? 'danger' : 'success'} style={{ fontSize: '12px' }}>
                  <TrendingUpOutlined rotate={tokenTrend < 0 ? 180 : 0} />
                  {Math.abs(tokenTrend).toFixed(1)}% vs last week
                </Text>
              </div>
            )}
          </Card>
        </Col>
        <Col xs={24} sm={12} lg={6}>
          <Card>
            <Statistic
              title="Total Cost"
              value={statistics?.totalCost || 0}
              precision={4}
              loading={statsLoading}
              prefix={<DollarOutlined />}
            />
            {costTrend !== 0 && (
              <div style={{ marginTop: 8 }}>
                <Text type={costTrend > 0 ? 'danger' : 'success'} style={{ fontSize: '12px' }}>
                  <TrendingUpOutlined rotate={costTrend < 0 ? 180 : 0} />
                  {Math.abs(costTrend).toFixed(1)}% vs last week
                </Text>
              </div>
            )}
          </Card>
        </Col>
        <Col xs={24} sm={12} lg={6}>
          <Card>
            <Statistic
              title="Avg Tokens/Request"
              value={statistics?.averageTokensPerRequest || 0}
              precision={0}
              loading={statsLoading}
              prefix={<BarChartOutlined />}
            />
          </Card>
        </Col>
      </Row>

      {/* Daily Usage Table */}
      <Card style={{ marginBottom: showUserBreakdown ? 16 : 0 }}>
        <Title level={5}>Daily Usage Breakdown</Title>
        <Table
          dataSource={dailyUsage}
          columns={dailyUsageColumns}
          loading={dailyLoading}
          rowKey="date"
          pagination={{ pageSize: 10 }}
          scroll={{ x: 800 }}
          size={compact ? 'small' : 'middle'}
        />
      </Card>

      {/* Top Users */}
      {showUserBreakdown && (
        <Card>
          <Title level={5}>Top Token Users</Title>
          <Table
            dataSource={topUsers}
            columns={topUsersColumns}
            loading={usersLoading}
            rowKey="userId"
            pagination={{ pageSize: 10 }}
            size={compact ? 'small' : 'middle'}
          />
        </Card>
      )}
    </div>
  );
};

export default ProcessFlowTokenUsage;
