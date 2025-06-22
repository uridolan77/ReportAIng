import React, { useMemo } from 'react'
import { 
  Card, 
  Space, 
  Typography, 
  Row, 
  Col, 
  Statistic,
  Select,
  DatePicker,
  Button,
  Tag
} from 'antd'
import {
  BarChartOutlined,
  LineChartOutlined,
  PieChartOutlined,
  TrophyOutlined,
  ClockCircleOutlined,
  DollarOutlined,
  ReloadOutlined
} from '@ant-design/icons'
import { 
  LineChart, 
  Line, 
  AreaChart, 
  Area, 
  BarChart, 
  Bar, 
  PieChart, 
  Pie, 
  Cell,
  XAxis, 
  YAxis, 
  CartesianGrid, 
  Tooltip, 
  Legend,
  ResponsiveContainer 
} from 'recharts'
import type { TransparencyMetrics } from '@shared/types/transparency'
import dayjs from 'dayjs'

const { Title, Text } = Typography
const { RangePicker } = DatePicker

export interface TransparencyMetricsChartProps {
  metrics: TransparencyMetrics[]
  timeRange?: [string, string]
  chartType?: 'line' | 'area' | 'bar' | 'pie'
  showComparison?: boolean
  showTrends?: boolean
  onTimeRangeChange?: (range: [string, string]) => void
  onRefresh?: () => void
  className?: string
  testId?: string
}

interface ChartData {
  date: string
  totalQueries: number
  averageConfidence: number
  successRate: number
  averageTokenUsage: number
  totalCost: number
  averageResponseTime: number
}

/**
 * TransparencyMetricsChart - Comprehensive charts for transparency metrics visualization
 * 
 * Features:
 * - Multiple chart types (line, area, bar, pie)
 * - Time-based trend analysis
 * - Metric comparisons
 * - Interactive filtering
 * - Performance indicators
 * - Cost analysis
 */
export const TransparencyMetricsChart: React.FC<TransparencyMetricsChartProps> = ({
  metrics,
  timeRange,
  chartType = 'line',
  showComparison = true,
  showTrends = true,
  onTimeRangeChange,
  onRefresh,
  className,
  testId = 'transparency-metrics-chart'
}) => {
  // Transform metrics data for charts
  const chartData = useMemo((): ChartData[] => {
    if (!metrics || metrics.length === 0) return []

    return metrics.map((metric, index) => ({
      date: dayjs().subtract(metrics.length - index - 1, 'day').format('MMM DD'),
      totalQueries: metric.totalQueries,
      averageConfidence: metric.averageConfidence * 100,
      successRate: metric.successRate * 100,
      averageTokenUsage: metric.averageTokenUsage,
      totalCost: metric.costAnalytics?.totalCost || 0,
      averageResponseTime: metric.performanceMetrics?.averageResponseTime || 0
    }))
  }, [metrics])

  // Calculate summary statistics
  const summaryStats = useMemo(() => {
    if (!chartData.length) return null

    const latest = chartData[chartData.length - 1]
    const previous = chartData.length > 1 ? chartData[chartData.length - 2] : latest

    return {
      totalQueries: {
        current: latest.totalQueries,
        change: latest.totalQueries - previous.totalQueries,
        trend: latest.totalQueries > previous.totalQueries ? 'up' : 'down'
      },
      averageConfidence: {
        current: latest.averageConfidence,
        change: latest.averageConfidence - previous.averageConfidence,
        trend: latest.averageConfidence > previous.averageConfidence ? 'up' : 'down'
      },
      successRate: {
        current: latest.successRate,
        change: latest.successRate - previous.successRate,
        trend: latest.successRate > previous.successRate ? 'up' : 'down'
      },
      totalCost: {
        current: latest.totalCost,
        change: latest.totalCost - previous.totalCost,
        trend: latest.totalCost > previous.totalCost ? 'up' : 'down'
      }
    }
  }, [chartData])

  // Prepare pie chart data for distribution analysis
  const distributionData = useMemo(() => {
    if (!chartData.length) return []

    const latest = chartData[chartData.length - 1]
    return [
      { name: 'High Confidence (>80%)', value: latest.averageConfidence > 80 ? 1 : 0, color: '#52c41a' },
      { name: 'Medium Confidence (60-80%)', value: latest.averageConfidence >= 60 && latest.averageConfidence <= 80 ? 1 : 0, color: '#faad14' },
      { name: 'Low Confidence (<60%)', value: latest.averageConfidence < 60 ? 1 : 0, color: '#ff4d4f' }
    ].filter(item => item.value > 0)
  }, [chartData])

  const renderLineChart = () => (
    <ResponsiveContainer width="100%" height={300}>
      <LineChart data={chartData}>
        <CartesianGrid strokeDasharray="3 3" />
        <XAxis dataKey="date" />
        <YAxis yAxisId="left" />
        <YAxis yAxisId="right" orientation="right" />
        <Tooltip />
        <Legend />
        <Line 
          yAxisId="left"
          type="monotone" 
          dataKey="totalQueries" 
          stroke="#1890ff" 
          strokeWidth={2}
          name="Total Queries"
        />
        <Line 
          yAxisId="right"
          type="monotone" 
          dataKey="averageConfidence" 
          stroke="#52c41a" 
          strokeWidth={2}
          name="Avg Confidence (%)"
        />
        <Line 
          yAxisId="right"
          type="monotone" 
          dataKey="successRate" 
          stroke="#722ed1" 
          strokeWidth={2}
          name="Success Rate (%)"
        />
      </LineChart>
    </ResponsiveContainer>
  )

  const renderAreaChart = () => (
    <ResponsiveContainer width="100%" height={300}>
      <AreaChart data={chartData}>
        <CartesianGrid strokeDasharray="3 3" />
        <XAxis dataKey="date" />
        <YAxis />
        <Tooltip />
        <Legend />
        <Area 
          type="monotone" 
          dataKey="totalQueries" 
          stackId="1"
          stroke="#1890ff" 
          fill="#1890ff"
          fillOpacity={0.6}
          name="Total Queries"
        />
        <Area 
          type="monotone" 
          dataKey="averageTokenUsage" 
          stackId="2"
          stroke="#faad14" 
          fill="#faad14"
          fillOpacity={0.6}
          name="Avg Token Usage"
        />
      </AreaChart>
    </ResponsiveContainer>
  )

  const renderBarChart = () => (
    <ResponsiveContainer width="100%" height={300}>
      <BarChart data={chartData}>
        <CartesianGrid strokeDasharray="3 3" />
        <XAxis dataKey="date" />
        <YAxis />
        <Tooltip />
        <Legend />
        <Bar dataKey="totalQueries" fill="#1890ff" name="Total Queries" />
        <Bar dataKey="averageTokenUsage" fill="#faad14" name="Avg Token Usage" />
      </BarChart>
    </ResponsiveContainer>
  )

  const renderPieChart = () => (
    <ResponsiveContainer width="100%" height={300}>
      <PieChart>
        <Pie
          data={distributionData}
          cx="50%"
          cy="50%"
          outerRadius={80}
          dataKey="value"
          label={({ name, percent }) => `${name}: ${(percent * 100).toFixed(0)}%`}
        >
          {distributionData.map((entry, index) => (
            <Cell key={`cell-${index}`} fill={entry.color} />
          ))}
        </Pie>
        <Tooltip />
      </PieChart>
    </ResponsiveContainer>
  )

  const renderChart = () => {
    switch (chartType) {
      case 'area': return renderAreaChart()
      case 'bar': return renderBarChart()
      case 'pie': return renderPieChart()
      default: return renderLineChart()
    }
  }

  const getTrendIcon = (trend: 'up' | 'down') => {
    return trend === 'up' ? '↗️' : '↘️'
  }

  const getTrendColor = (trend: 'up' | 'down', isPositive: boolean) => {
    const isGood = (trend === 'up' && isPositive) || (trend === 'down' && !isPositive)
    return isGood ? '#3f8600' : '#cf1322'
  }

  if (!metrics || metrics.length === 0) {
    return (
      <Card className={className} data-testid={testId}>
        <div style={{ textAlign: 'center', padding: '40px 0' }}>
          <Space direction="vertical">
            <BarChartOutlined style={{ fontSize: '48px', color: '#d9d9d9' }} />
            <Text type="secondary">No metrics data available</Text>
          </Space>
        </div>
      </Card>
    )
  }

  return (
    <Card 
      title={
        <Space>
          <BarChartOutlined />
          <span>Transparency Metrics</span>
          <Tag color="blue">{chartData.length} data points</Tag>
        </Space>
      }
      extra={
        <Space>
          <Select
            value={chartType}
            onChange={(value) => {
              // Chart type change would be handled by parent component
            }}
            style={{ width: 100 }}
          >
            <Select.Option value="line">Line</Select.Option>
            <Select.Option value="area">Area</Select.Option>
            <Select.Option value="bar">Bar</Select.Option>
            <Select.Option value="pie">Pie</Select.Option>
          </Select>
          {onRefresh && (
            <Button 
              size="small" 
              icon={<ReloadOutlined />}
              onClick={onRefresh}
            >
              Refresh
            </Button>
          )}
        </Space>
      }
      className={className}
      data-testid={testId}
    >
      <Space direction="vertical" style={{ width: '100%' }} size="large">
        {/* Summary Statistics */}
        {summaryStats && (
          <Row gutter={[16, 16]}>
            <Col xs={24} sm={12} lg={6}>
              <Statistic
                title="Total Queries"
                value={summaryStats.totalQueries.current}
                prefix={<BarChartOutlined />}
                suffix={
                  <Space>
                    <span style={{ 
                      color: getTrendColor(summaryStats.totalQueries.trend, true),
                      fontSize: '14px'
                    }}>
                      {getTrendIcon(summaryStats.totalQueries.trend)}
                      {Math.abs(summaryStats.totalQueries.change)}
                    </span>
                  </Space>
                }
              />
            </Col>
            <Col xs={24} sm={12} lg={6}>
              <Statistic
                title="Avg Confidence"
                value={summaryStats.averageConfidence.current.toFixed(1)}
                suffix="%"
                prefix={<TrophyOutlined />}
                valueStyle={{ 
                  color: summaryStats.averageConfidence.current > 80 ? '#3f8600' : 
                         summaryStats.averageConfidence.current > 60 ? '#faad14' : '#cf1322' 
                }}
                suffix={
                  <Space>
                    <span>%</span>
                    <span style={{ 
                      color: getTrendColor(summaryStats.averageConfidence.trend, true),
                      fontSize: '14px'
                    }}>
                      {getTrendIcon(summaryStats.averageConfidence.trend)}
                      {Math.abs(summaryStats.averageConfidence.change).toFixed(1)}
                    </span>
                  </Space>
                }
              />
            </Col>
            <Col xs={24} sm={12} lg={6}>
              <Statistic
                title="Success Rate"
                value={summaryStats.successRate.current.toFixed(1)}
                suffix="%"
                prefix={<ClockCircleOutlined />}
                valueStyle={{ 
                  color: summaryStats.successRate.current > 90 ? '#3f8600' : 
                         summaryStats.successRate.current > 70 ? '#faad14' : '#cf1322' 
                }}
                suffix={
                  <Space>
                    <span>%</span>
                    <span style={{ 
                      color: getTrendColor(summaryStats.successRate.trend, true),
                      fontSize: '14px'
                    }}>
                      {getTrendIcon(summaryStats.successRate.trend)}
                      {Math.abs(summaryStats.successRate.change).toFixed(1)}
                    </span>
                  </Space>
                }
              />
            </Col>
            <Col xs={24} sm={12} lg={6}>
              <Statistic
                title="Total Cost"
                value={summaryStats.totalCost.current.toFixed(4)}
                prefix="$"
                prefix={<DollarOutlined />}
                suffix={
                  <Space>
                    <span style={{ 
                      color: getTrendColor(summaryStats.totalCost.trend, false),
                      fontSize: '14px'
                    }}>
                      {getTrendIcon(summaryStats.totalCost.trend)}
                      ${Math.abs(summaryStats.totalCost.change).toFixed(4)}
                    </span>
                  </Space>
                }
              />
            </Col>
          </Row>
        )}

        {/* Time Range Selector */}
        {onTimeRangeChange && (
          <Row>
            <Col>
              <Space>
                <Text>Time Range:</Text>
                <RangePicker
                  value={timeRange ? [dayjs(timeRange[0]), dayjs(timeRange[1])] : undefined}
                  onChange={(dates) => {
                    if (dates && dates[0] && dates[1]) {
                      onTimeRangeChange([
                        dates[0].format('YYYY-MM-DD'),
                        dates[1].format('YYYY-MM-DD')
                      ])
                    }
                  }}
                />
              </Space>
            </Col>
          </Row>
        )}

        {/* Main Chart */}
        <Card title={`${chartType.charAt(0).toUpperCase() + chartType.slice(1)} Chart`} size="small">
          {renderChart()}
        </Card>

        {/* Trend Analysis */}
        {showTrends && chartData.length > 1 && (
          <Row gutter={[16, 16]}>
            <Col xs={24} lg={12}>
              <Card title="Confidence Trend" size="small">
                <ResponsiveContainer width="100%" height={150}>
                  <LineChart data={chartData}>
                    <XAxis dataKey="date" />
                    <YAxis domain={[0, 100]} />
                    <Tooltip />
                    <Line 
                      type="monotone" 
                      dataKey="averageConfidence" 
                      stroke="#52c41a" 
                      strokeWidth={2}
                    />
                  </LineChart>
                </ResponsiveContainer>
              </Card>
            </Col>
            <Col xs={24} lg={12}>
              <Card title="Query Volume Trend" size="small">
                <ResponsiveContainer width="100%" height={150}>
                  <AreaChart data={chartData}>
                    <XAxis dataKey="date" />
                    <YAxis />
                    <Tooltip />
                    <Area 
                      type="monotone" 
                      dataKey="totalQueries" 
                      stroke="#1890ff" 
                      fill="#1890ff"
                      fillOpacity={0.6}
                    />
                  </AreaChart>
                </ResponsiveContainer>
              </Card>
            </Col>
          </Row>
        )}
      </Space>
    </Card>
  )
}

export default TransparencyMetricsChart
