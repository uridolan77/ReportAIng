import React, { useState } from 'react'
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
  Tooltip
} from 'antd'
import {
  DollarOutlined,
  ArrowUpOutlined,
  ArrowDownOutlined,
  WarningOutlined,
  BulbOutlined,
  ReloadOutlined
} from '@ant-design/icons'
import { useCostMetrics, useCostBreakdown, useCostAlerts, useCostEfficiency } from '../../hooks/useCostMetrics'
import { CostTrendsChart } from './CostTrendsChart'
import { CostBreakdownChart } from './CostBreakdownChart'
import { BudgetStatusWidget } from './BudgetStatusWidget'
import { RecommendationsWidget } from './RecommendationsWidget'
import type { TimeRange } from '../../types/cost'

const { Title } = Typography
const { RangePicker } = DatePicker

interface MetricCardProps {
  title: string
  value?: number
  format?: 'currency' | 'percentage' | 'number'
  trend?: number
  loading?: boolean
  prefix?: React.ReactNode
}

const MetricCard: React.FC<MetricCardProps> = ({ 
  title, 
  value, 
  format = 'number', 
  trend, 
  loading,
  prefix 
}) => {
  const formatValue = (val?: number) => {
    if (val === undefined) return 'N/A'
    
    switch (format) {
      case 'currency':
        return `$${val.toLocaleString(undefined, { minimumFractionDigits: 2 })}`
      case 'percentage':
        return `${val.toFixed(1)}%`
      default:
        return val.toLocaleString()
    }
  }

  const getTrendIcon = () => {
    if (trend === undefined) return null
    return trend > 0 ? <ArrowUpOutlined /> : <ArrowDownOutlined />
  }

  const getTrendColor = () => {
    if (trend === undefined) return undefined
    return trend > 0 ? '#ff4d4f' : '#52c41a'
  }

  return (
    <Card>
      <Statistic
        title={title}
        value={formatValue(value)}
        loading={loading}
        prefix={prefix}
        suffix={
          trend !== undefined && (
            <span style={{ color: getTrendColor(), fontSize: '14px' }}>
              {getTrendIcon()} {Math.abs(trend).toFixed(1)}%
            </span>
          )
        }
      />
    </Card>
  )
}

export const CostDashboard: React.FC = () => {
  const [timeRange, setTimeRange] = useState<TimeRange>('30d')
  const [breakdownDimension, setBreakdownDimension] = useState('provider')

  const {
    analytics,
    trends,
    realTime,
    _forecast,
    recommendations,
    isLoading,
    error,
    refetch
  } = useCostMetrics(timeRange)

  const { breakdown } = useCostBreakdown(breakdownDimension, timeRange)
  const { _alerts, criticalCount, _highCount, hasCritical } = useCostAlerts()
  const { efficiency, savingsOpportunities, _trendDirection, changePercentage } = useCostEfficiency(timeRange)

  if (error) {
    return (
      <Alert
        message="Error Loading Cost Data"
        description="Failed to load cost analytics. Please try again."
        type="error"
        showIcon
        action={
          <Button size="small" onClick={refetch}>
            Retry
          </Button>
        }
      />
    )
  }

  return (
    <div style={{ padding: '24px' }}>
      {/* Header */}
      <div style={{ marginBottom: '24px', display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
        <Title level={2}>Cost Management Dashboard</Title>
        <Space>
          <Select
            value={timeRange}
            onChange={setTimeRange}
            style={{ width: 120 }}
          >
            <Select.Option value="1h">Last Hour</Select.Option>
            <Select.Option value="24h">Last 24h</Select.Option>
            <Select.Option value="7d">Last 7 days</Select.Option>
            <Select.Option value="30d">Last 30 days</Select.Option>
            <Select.Option value="90d">Last 90 days</Select.Option>
            <Select.Option value="1y">Last Year</Select.Option>
          </Select>
          <Tooltip title="Refresh Data">
            <Button icon={<ReloadOutlined />} onClick={refetch} loading={isLoading} />
          </Tooltip>
        </Space>
      </div>

      {/* Alerts */}
      {hasCritical && (
        <Alert
          message={`${criticalCount} Critical Cost Alert${criticalCount > 1 ? 's' : ''}`}
          description="Immediate attention required for cost management."
          type="error"
          showIcon
          style={{ marginBottom: '24px' }}
        />
      )}

      {/* Key Metrics Cards */}
      <Row gutter={[16, 16]} style={{ marginBottom: '24px' }}>
        <Col xs={24} sm={12} lg={6}>
          <MetricCard
            title="Total Cost (Period)"
            value={analytics?.totalCost}
            format="currency"
            trend={changePercentage}
            loading={isLoading}
            prefix={<DollarOutlined />}
          />
        </Col>
        <Col xs={24} sm={12} lg={6}>
          <MetricCard
            title="Daily Average"
            value={analytics?.dailyCost}
            format="currency"
            loading={isLoading}
            prefix={<DollarOutlined />}
          />
        </Col>
        <Col xs={24} sm={12} lg={6}>
          <MetricCard
            title="Cost Efficiency"
            value={efficiency * 100}
            format="percentage"
            loading={isLoading}
            prefix={<BulbOutlined />}
          />
        </Col>
        <Col xs={24} sm={12} lg={6}>
          <MetricCard
            title="Potential Savings"
            value={savingsOpportunities}
            format="currency"
            loading={isLoading}
            prefix={<WarningOutlined />}
          />
        </Col>
      </Row>

      {/* Real-time Metrics */}
      {realTime && (
        <Row gutter={[16, 16]} style={{ marginBottom: '24px' }}>
          <Col xs={24} sm={12} lg={6}>
            <MetricCard
              title="Current Cost"
              value={realTime.currentCost}
              format="currency"
              prefix={<DollarOutlined />}
            />
          </Col>
          <Col xs={24} sm={12} lg={6}>
            <MetricCard
              title="Cost per Minute"
              value={realTime.costPerMinute}
              format="currency"
              prefix={<DollarOutlined />}
            />
          </Col>
          <Col xs={24} sm={12} lg={6}>
            <MetricCard
              title="Active Queries"
              value={realTime.activeQueries}
              format="number"
            />
          </Col>
          <Col xs={24} sm={12} lg={6}>
            <MetricCard
              title="Cost per Query"
              value={realTime.costPerQuery}
              format="currency"
              prefix={<DollarOutlined />}
            />
          </Col>
        </Row>
      )}

      {/* Charts and Widgets */}
      <Row gutter={[16, 16]}>
        {/* Cost Trends Chart */}
        <Col xs={24} lg={16}>
          <CostTrendsChart data={trends?.trends} loading={isLoading} />
        </Col>

        {/* Cost Breakdown */}
        <Col xs={24} lg={8}>
          <Card 
            title="Cost Breakdown" 
            extra={
              <Select
                value={breakdownDimension}
                onChange={setBreakdownDimension}
                size="small"
              >
                <Select.Option value="provider">By Provider</Select.Option>
                <Select.Option value="user">By User</Select.Option>
                <Select.Option value="department">By Department</Select.Option>
                <Select.Option value="model">By Model</Select.Option>
              </Select>
            }
          >
            <CostBreakdownChart data={breakdown} loading={isLoading} />
          </Card>
        </Col>

        {/* Budget Status */}
        <Col xs={24} lg={12}>
          <BudgetStatusWidget />
        </Col>

        {/* Recommendations */}
        <Col xs={24} lg={12}>
          <RecommendationsWidget recommendations={recommendations?.recommendations} />
        </Col>
      </Row>
    </div>
  )
}
