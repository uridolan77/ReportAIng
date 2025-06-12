/**
 * Performance Monitoring Component
 *
 * Monitors LLM performance including response times, success rates,
 * error analysis, and optimization recommendations.
 */

import React, { useState, useEffect } from 'react';
import {
  Card,
  Table,
  Button,
  Space,
  Statistic,
  Row,
  Col,
  DatePicker,
  message,
  Flex,
  Tag,
  Progress,
  Alert,
  Tooltip,
  List
} from 'antd';
import {
  ThunderboltOutlined,
  LineChartOutlined,
  ReloadOutlined,
  WarningOutlined,
  CheckCircleOutlined,
  ClockCircleOutlined,
  ExclamationCircleOutlined
} from '@ant-design/icons';
import {
  llmManagementService,
  ProviderPerformanceMetrics,
  ErrorAnalysis,
  LLMProviderConfig
} from '../../services/llmManagementService';
import dayjs from 'dayjs';

const { RangePicker } = DatePicker;

export const PerformanceMonitoring: React.FC = () => {
  const [performanceMetrics, setPerformanceMetrics] = useState<Record<string, ProviderPerformanceMetrics>>({});
  const [errorAnalysis, setErrorAnalysis] = useState<Record<string, ErrorAnalysis>>({});
  const [cacheHitRates, setCacheHitRates] = useState<Record<string, number>>({});
  const [providers, setProviders] = useState<LLMProviderConfig[]>([]);
  const [loading, setLoading] = useState(true);

  const [dateRange, setDateRange] = useState<[dayjs.Dayjs, dayjs.Dayjs]>([
    dayjs().subtract(30, 'days'),
    dayjs()
  ]);

  useEffect(() => {
    loadData();
    loadProviders();
  }, []);

  useEffect(() => {
    loadData();
  }, [dateRange]);

  const loadProviders = async () => {
    try {
      const providersData = await llmManagementService.getProviders();
      setProviders(providersData);
    } catch (error) {
      console.error('Error loading providers:', error);
    }
  };

  const loadData = async () => {
    try {
      setLoading(true);
      const startDate = dateRange[0].format('YYYY-MM-DD');
      const endDate = dateRange[1].format('YYYY-MM-DD');

      const [metrics, errors, cacheRates] = await Promise.all([
        llmManagementService.getPerformanceMetrics(startDate, endDate),
        llmManagementService.getErrorAnalysis(startDate, endDate),
        llmManagementService.getCacheHitRates()
      ]);

      setPerformanceMetrics(metrics);
      setErrorAnalysis(errors);
      setCacheHitRates(cacheRates);
    } catch (error) {
      message.error('Failed to load performance monitoring data');
      console.error('Error loading performance data:', error);
    } finally {
      setLoading(false);
    }
  };

  const getProviderName = (providerId: string) => {
    const provider = providers.find(p => p.providerId === providerId);
    return provider?.name || providerId;
  };

  const performanceColumns = [
    {
      title: 'Provider',
      dataIndex: 'providerId',
      key: 'providerId',
      render: (providerId: string) => getProviderName(providerId),
    },
    {
      title: 'Total Requests',
      dataIndex: 'totalRequests',
      key: 'totalRequests',
      render: (requests: number) => requests.toLocaleString(),
    },
    {
      title: 'Success Rate',
      dataIndex: 'successRate',
      key: 'successRate',
      render: (rate: number) => (
        <div>
          <Progress
            percent={rate * 100}
            size="small"
            status={rate > 0.95 ? 'success' : rate > 0.9 ? 'active' : 'exception'}
            format={(percent) => `${percent?.toFixed(1)}%`}
          />
        </div>
      ),
    },
    {
      title: 'Avg Response Time',
      dataIndex: 'averageResponseTime',
      key: 'averageResponseTime',
      render: (time: number) => (
        <span style={{ color: time > 2000 ? '#ff4d4f' : time > 1000 ? '#fa8c16' : '#52c41a' }}>
          {Math.round(time)}ms
        </span>
      ),
    },
    {
      title: 'P95 Response Time',
      dataIndex: 'p95ResponseTime',
      key: 'p95ResponseTime',
      render: (time: number) => `${Math.round(time)}ms`,
    },
    {
      title: 'Error Rate',
      dataIndex: 'errorRate',
      key: 'errorRate',
      render: (rate: number) => (
        <Tag color={rate > 0.1 ? 'red' : rate > 0.05 ? 'orange' : 'green'}>
          {(rate * 100).toFixed(2)}%
        </Tag>
      ),
    },
    {
      title: 'Cache Hit Rate',
      key: 'cacheHitRate',
      render: (record: ProviderPerformanceMetrics) => {
        const hitRate = cacheHitRates[record.providerId] || 0;
        return (
          <Tag color={hitRate > 0.8 ? 'green' : hitRate > 0.5 ? 'orange' : 'red'}>
            {(hitRate * 100).toFixed(1)}%
          </Tag>
        );
      },
    },
  ];

  const errorColumns = [
    {
      title: 'Provider',
      dataIndex: 'providerId',
      key: 'providerId',
      render: (providerId: string) => getProviderName(providerId),
    },
    {
      title: 'Total Errors',
      dataIndex: 'totalErrors',
      key: 'totalErrors',
      render: (errors: number) => errors.toLocaleString(),
    },
    {
      title: 'Error Rate',
      dataIndex: 'errorRate',
      key: 'errorRate',
      render: (rate: number) => (
        <Tag color={rate > 0.1 ? 'red' : rate > 0.05 ? 'orange' : 'green'}>
          {(rate * 100).toFixed(2)}%
        </Tag>
      ),
    },
    {
      title: 'Top Error Types',
      dataIndex: 'errorsByType',
      key: 'errorsByType',
      render: (errorsByType: Record<string, number>) => (
        <div>
          {Object.entries(errorsByType)
            .sort(([,a], [,b]) => b - a)
            .slice(0, 3)
            .map(([type, count]) => (
              <Tag key={type} style={{ marginBottom: '2px' }}>
                {type}: {count}
              </Tag>
            ))}
        </div>
      ),
    },
  ];

  const performanceData = Object.values(performanceMetrics);
  const errorData = Object.values(errorAnalysis);

  const overallSuccessRate = performanceData.length > 0
    ? performanceData.reduce((sum, p) => sum + p.successRate, 0) / performanceData.length
    : 0;

  const overallAvgResponseTime = performanceData.length > 0
    ? performanceData.reduce((sum, p) => sum + p.averageResponseTime, 0) / performanceData.length
    : 0;

  const totalErrors = errorData.reduce((sum, e) => sum + e.totalErrors, 0);
  const overallCacheHitRate = Object.values(cacheHitRates).length > 0
    ? Object.values(cacheHitRates).reduce((sum, rate) => sum + rate, 0) / Object.values(cacheHitRates).length
    : 0;

  const getPerformanceRecommendations = () => {
    const recommendations = [];

    if (overallSuccessRate < 0.95) {
      recommendations.push({
        type: 'error',
        title: 'Low Success Rate',
        description: `Overall success rate is ${(overallSuccessRate * 100).toFixed(1)}%. Consider reviewing error patterns and model configurations.`
      });
    }

    if (overallAvgResponseTime > 2000) {
      recommendations.push({
        type: 'warning',
        title: 'High Response Times',
        description: `Average response time is ${Math.round(overallAvgResponseTime)}ms. Consider optimizing model parameters or using faster models.`
      });
    }

    if (overallCacheHitRate < 0.5) {
      recommendations.push({
        type: 'info',
        title: 'Low Cache Hit Rate',
        description: `Cache hit rate is ${(overallCacheHitRate * 100).toFixed(1)}%. Consider enabling or optimizing caching strategies.`
      });
    }

    if (recommendations.length === 0) {
      recommendations.push({
        type: 'success',
        title: 'Performance Looks Good',
        description: 'All performance metrics are within acceptable ranges.'
      });
    }

    return recommendations;
  };

  const recommendations = getPerformanceRecommendations();

  return (
    <div style={{ padding: '24px' }}>
      {/* Performance Overview */}
      <Row gutter={16} style={{ marginBottom: '24px' }}>
        <Col span={6}>
          <Card size="small">
            <Statistic
              title="Overall Success Rate"
              value={overallSuccessRate * 100}
              precision={1}
              suffix="%"
              valueStyle={{ color: overallSuccessRate > 0.95 ? '#52c41a' : '#fa541c' }}
              prefix={overallSuccessRate > 0.95 ? <CheckCircleOutlined /> : <ExclamationCircleOutlined />}
            />
          </Card>
        </Col>
        <Col span={6}>
          <Card size="small">
            <Statistic
              title="Avg Response Time"
              value={overallAvgResponseTime}
              precision={0}
              suffix="ms"
              valueStyle={{ color: overallAvgResponseTime < 1000 ? '#52c41a' : overallAvgResponseTime < 2000 ? '#fa8c16' : '#ff4d4f' }}
              prefix={<ClockCircleOutlined />}
            />
          </Card>
        </Col>
        <Col span={6}>
          <Card size="small">
            <Statistic
              title="Total Errors"
              value={totalErrors}
              valueStyle={{ color: totalErrors === 0 ? '#52c41a' : '#ff4d4f' }}
              prefix={totalErrors > 0 ? <WarningOutlined /> : <CheckCircleOutlined />}
            />
          </Card>
        </Col>
        <Col span={6}>
          <Card size="small">
            <Statistic
              title="Cache Hit Rate"
              value={overallCacheHitRate * 100}
              precision={1}
              suffix="%"
              valueStyle={{ color: overallCacheHitRate > 0.8 ? '#52c41a' : overallCacheHitRate > 0.5 ? '#fa8c16' : '#ff4d4f' }}
            />
          </Card>
        </Col>
      </Row>

      {/* Performance Recommendations */}
      <Card title="Performance Recommendations" size="small" style={{ marginBottom: '16px' }}>
        <List
          dataSource={recommendations}
          renderItem={(item) => (
            <List.Item>
              <Alert
                message={item.title}
                description={item.description}
                type={item.type as any}
                showIcon
                style={{ width: '100%' }}
              />
            </List.Item>
          )}
        />
      </Card>

      {/* Filters */}
      <Card size="small" style={{ marginBottom: '16px' }}>
        <Flex justify="between" align="center">
          <Space>
            <RangePicker
              value={dateRange}
              onChange={(dates) => dates && setDateRange(dates as [dayjs.Dayjs, dayjs.Dayjs])}
              format="YYYY-MM-DD"
            />
          </Space>
          <Space>
            <Button
              icon={<ReloadOutlined />}
              onClick={loadData}
            >
              Refresh
            </Button>
          </Space>
        </Flex>
      </Card>

      {/* Performance Metrics Table */}
      <Card title="Provider Performance Metrics" size="small" style={{ marginBottom: '16px' }}>
        <Table
          columns={performanceColumns}
          dataSource={performanceData}
          rowKey="providerId"
          loading={loading}
          pagination={false}
          scroll={{ x: 800 }}
        />
      </Card>

      {/* Error Analysis Table */}
      <Card title="Error Analysis" size="small">
        <Table
          columns={errorColumns}
          dataSource={errorData}
          rowKey="providerId"
          loading={loading}
          pagination={false}
          locale={{ emptyText: 'No errors found in the selected time range' }}
        />
      </Card>
    </div>
  );
};
