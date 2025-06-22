import React, { useState, useMemo } from 'react'
import { 
  Card, 
  Row, 
  Col, 
  Typography, 
  Space, 
  Statistic,
  Progress,
  Alert,
  List,
  Button,
  Select,
  DatePicker,
  Table,
  Tag,
  Tooltip,
  Badge
} from 'antd'
import {
  DollarOutlined,
  RiseOutlined,
  FallOutlined,
  BulbOutlined,
  ThunderboltOutlined,
  ClockCircleOutlined,
  ExclamationCircleOutlined,
  CheckCircleOutlined,
  BarChartOutlined,
  PieChartOutlined
} from '@ant-design/icons'
import { PieChart, Pie, Cell, BarChart, Bar, LineChart, Line, XAxis, YAxis, CartesianGrid, Tooltip as RechartsTooltip, ResponsiveContainer, AreaChart, Area } from 'recharts'
import { ConfidenceIndicator } from '../common/ConfidenceIndicator'
import { useGetTransparencyMetricsQuery } from '@shared/store/api/transparencyApi'
import dayjs from 'dayjs'

const { Title, Text } = Typography
const { RangePicker } = DatePicker

export interface CostOptimizationPanelProps {
  timeRange?: number
  showRecommendations?: boolean
  showProjections?: boolean
  compact?: boolean
  onOptimizationApply?: (optimization: CostOptimization) => void
  className?: string
  testId?: string
}

interface CostOptimization {
  id: string
  title: string
  description: string
  type: 'model_switching' | 'caching' | 'batching' | 'scheduling' | 'configuration'
  impact: 'low' | 'medium' | 'high'
  effort: 'low' | 'medium' | 'high'
  estimatedSavings: number
  timeframe: string
  confidence: number
}

interface CostBreakdown {
  category: string
  amount: number
  percentage: number
  trend: 'up' | 'down' | 'stable'
  color: string
}

/**
 * CostOptimizationPanel - AI cost analysis and optimization recommendations
 * 
 * Features:
 * - Real-time cost tracking and analysis
 * - Cost breakdown by model, usage type, and time period
 * - AI-powered optimization recommendations
 * - Cost projection and budget planning
 * - Interactive optimization implementation
 * - ROI analysis and savings tracking
 */
export const CostOptimizationPanel: React.FC<CostOptimizationPanelProps> = ({
  timeRange = 30,
  showRecommendations = true,
  showProjections = true,
  compact = false,
  onOptimizationApply,
  className,
  testId = 'cost-optimization-panel'
}) => {
  const [selectedTimeRange, setSelectedTimeRange] = useState(timeRange)
  const [selectedOptimization, setSelectedOptimization] = useState<CostOptimization | null>(null)

  // Real API data
  const { data: agentAnalytics, isLoading: analyticsLoading } = useGetTransparencyMetricsQuery({ days: selectedTimeRange, includeDetails: true })
  const { data: transparencyMetrics, isLoading: metricsLoading } = useGetTransparencyMetricsQuery({ days: selectedTimeRange })

  // Mock cost data (replace with real API data)
  const costData = {
    totalCost: 1247.85,
    monthlyBudget: 2000,
    dailyAverage: 41.60,
    projectedMonthly: 1456.20,
    lastMonthCost: 1189.45,
    trend: 'up' as const
  }

  // Cost breakdown data
  const costBreakdown: CostBreakdown[] = [
    { category: 'GPT-4', amount: 567.23, percentage: 45.5, trend: 'up', color: '#1890ff' },
    { category: 'Claude-3', amount: 324.67, percentage: 26.0, trend: 'stable', color: '#52c41a' },
    { category: 'GPT-3.5', amount: 198.45, percentage: 15.9, trend: 'down', color: '#faad14' },
    { category: 'Gemini', amount: 157.50, percentage: 12.6, trend: 'up', color: '#722ed1' }
  ]

  // Cost trend data
  const costTrendData = [
    { date: '2024-01-01', cost: 38.50, budget: 66.67, savings: 0 },
    { date: '2024-01-02', cost: 42.75, budget: 66.67, savings: 5.20 },
    { date: '2024-01-03', cost: 35.20, budget: 66.67, savings: 8.15 },
    { date: '2024-01-04', cost: 48.80, budget: 66.67, savings: 12.30 },
    { date: '2024-01-05', cost: 41.25, budget: 66.67, savings: 15.45 },
    { date: '2024-01-06', cost: 52.60, budget: 66.67, savings: 18.90 },
    { date: '2024-01-07', cost: 45.45, budget: 66.67, savings: 22.15 }
  ]

  // Optimization recommendations
  const optimizations: CostOptimization[] = [
    {
      id: 'opt-001',
      title: 'Switch to GPT-3.5 for Simple Queries',
      description: 'Use GPT-3.5 for queries with low complexity scores to reduce costs by 70%',
      type: 'model_switching',
      impact: 'high',
      effort: 'low',
      estimatedSavings: 245.30,
      timeframe: '30 days',
      confidence: 0.92
    },
    {
      id: 'opt-002',
      title: 'Implement Response Caching',
      description: 'Cache responses for frequently asked questions to reduce API calls by 35%',
      type: 'caching',
      impact: 'medium',
      effort: 'medium',
      estimatedSavings: 156.75,
      timeframe: '30 days',
      confidence: 0.87
    },
    {
      id: 'opt-003',
      title: 'Batch Processing for Analytics',
      description: 'Process multiple analytics queries in batches during off-peak hours',
      type: 'batching',
      impact: 'medium',
      effort: 'high',
      estimatedSavings: 89.40,
      timeframe: '30 days',
      confidence: 0.78
    },
    {
      id: 'opt-004',
      title: 'Optimize Token Usage',
      description: 'Reduce prompt length and optimize context window usage',
      type: 'configuration',
      impact: 'low',
      effort: 'low',
      estimatedSavings: 67.20,
      timeframe: '30 days',
      confidence: 0.85
    }
  ]

  // Calculate metrics
  const budgetUtilization = (costData.totalCost / costData.monthlyBudget) * 100
  const costChange = ((costData.totalCost - costData.lastMonthCost) / costData.lastMonthCost) * 100
  const totalPotentialSavings = optimizations.reduce((acc, opt) => acc + opt.estimatedSavings, 0)

  // Get impact color
  const getImpactColor = (impact: string) => {
    switch (impact) {
      case 'high': return '#ff4d4f'
      case 'medium': return '#faad14'
      case 'low': return '#52c41a'
      default: return '#d9d9d9'
    }
  }

  // Get effort color
  const getEffortColor = (effort: string) => {
    switch (effort) {
      case 'high': return '#ff4d4f'
      case 'medium': return '#faad14'
      case 'low': return '#52c41a'
      default: return '#d9d9d9'
    }
  }

  // Handle optimization application
  const handleApplyOptimization = (optimization: CostOptimization) => {
    setSelectedOptimization(optimization)
    onOptimizationApply?.(optimization)
  }

  // Optimization table columns
  const optimizationColumns = [
    {
      title: 'Optimization',
      dataIndex: 'title',
      key: 'title',
      render: (text: string, record: CostOptimization) => (
        <div>
          <div style={{ fontWeight: 'bold' }}>{text}</div>
          <Text type="secondary" style={{ fontSize: '12px' }}>
            {record.description}
          </Text>
        </div>
      )
    },
    {
      title: 'Type',
      dataIndex: 'type',
      key: 'type',
      render: (type: string) => (
        <Tag color="blue">{type.replace('_', ' ').toUpperCase()}</Tag>
      )
    },
    {
      title: 'Impact',
      dataIndex: 'impact',
      key: 'impact',
      render: (impact: string) => (
        <Tag color={getImpactColor(impact)}>{impact.toUpperCase()}</Tag>
      )
    },
    {
      title: 'Effort',
      dataIndex: 'effort',
      key: 'effort',
      render: (effort: string) => (
        <Tag color={getEffortColor(effort)}>{effort.toUpperCase()}</Tag>
      )
    },
    {
      title: 'Savings',
      dataIndex: 'estimatedSavings',
      key: 'savings',
      render: (savings: number) => (
        <Text strong style={{ color: '#52c41a' }}>
          ${savings.toFixed(2)}
        </Text>
      ),
      sorter: (a: CostOptimization, b: CostOptimization) => a.estimatedSavings - b.estimatedSavings
    },
    {
      title: 'Confidence',
      dataIndex: 'confidence',
      key: 'confidence',
      render: (confidence: number) => (
        <ConfidenceIndicator
          confidence={confidence}
          size="small"
          type="badge"
          showPercentage={false}
        />
      )
    },
    {
      title: 'Actions',
      key: 'actions',
      render: (_: any, record: CostOptimization) => (
        <Button
          type="primary"
          size="small"
          onClick={() => handleApplyOptimization(record)}
        >
          Apply
        </Button>
      )
    }
  ]

  return (
    <div className={className} data-testid={testId}>
      {/* Header */}
      <div style={{ 
        display: 'flex', 
        justifyContent: 'space-between', 
        alignItems: 'center',
        marginBottom: 24 
      }}>
        <div>
          <Title level={4} style={{ margin: 0 }}>
            Cost Optimization
          </Title>
          <Text type="secondary">
            AI cost analysis, optimization recommendations, and savings tracking
          </Text>
        </div>
        <Space>
          <Select
            value={selectedTimeRange}
            onChange={setSelectedTimeRange}
            style={{ width: 120 }}
          >
            <Select.Option value={7}>Last 7 days</Select.Option>
            <Select.Option value={30}>Last 30 days</Select.Option>
            <Select.Option value={90}>Last 90 days</Select.Option>
          </Select>
        </Space>
      </div>

      {/* Cost Overview */}
      <Row gutter={[16, 16]} style={{ marginBottom: 24 }}>
        <Col xs={24} sm={12} lg={6}>
          <Card>
            <Statistic
              title="Total Cost"
              value={costData.totalCost}
              prefix={<DollarOutlined />}
              precision={2}
              valueStyle={{ color: '#1890ff' }}
            />
            <div style={{ marginTop: 8 }}>
              <Text type="secondary" style={{ fontSize: '12px' }}>
                {costChange > 0 ? '+' : ''}{costChange.toFixed(1)}% vs last month
              </Text>
              {costChange > 0 ? 
                <RiseOutlined style={{ color: '#ff4d4f', marginLeft: 4 }} /> :
                <FallOutlined style={{ color: '#52c41a', marginLeft: 4 }} />
              }
            </div>
          </Card>
        </Col>
        <Col xs={24} sm={12} lg={6}>
          <Card>
            <Statistic
              title="Budget Utilization"
              value={budgetUtilization}
              suffix="%"
              prefix={<BarChartOutlined />}
              valueStyle={{ color: budgetUtilization > 80 ? '#ff4d4f' : '#52c41a' }}
              precision={1}
            />
            <Progress 
              percent={budgetUtilization} 
              size="small" 
              strokeColor={budgetUtilization > 80 ? '#ff4d4f' : '#52c41a'}
              style={{ marginTop: 8 }}
            />
          </Card>
        </Col>
        <Col xs={24} sm={12} lg={6}>
          <Card>
            <Statistic
              title="Daily Average"
              value={costData.dailyAverage}
              prefix={<ClockCircleOutlined />}
              precision={2}
              valueStyle={{ color: '#722ed1' }}
            />
            <div style={{ marginTop: 8 }}>
              <Text type="secondary" style={{ fontSize: '12px' }}>
                Projected: ${costData.projectedMonthly.toFixed(2)}/month
              </Text>
            </div>
          </Card>
        </Col>
        <Col xs={24} sm={12} lg={6}>
          <Card>
            <Statistic
              title="Potential Savings"
              value={totalPotentialSavings}
              prefix={<BulbOutlined />}
              precision={2}
              valueStyle={{ color: '#52c41a' }}
            />
            <div style={{ marginTop: 8 }}>
              <Text type="secondary" style={{ fontSize: '12px' }}>
                {optimizations.length} optimizations available
              </Text>
            </div>
          </Card>
        </Col>
      </Row>

      {/* Cost Breakdown and Trends */}
      <Row gutter={[16, 16]} style={{ marginBottom: 24 }}>
        <Col xs={24} lg={8}>
          <Card title="Cost Breakdown" size={compact ? 'small' : 'default'}>
            <ResponsiveContainer width="100%" height={250}>
              <PieChart>
                <Pie
                  data={costBreakdown}
                  cx="50%"
                  cy="50%"
                  outerRadius={80}
                  dataKey="amount"
                  label={({ name, percentage }) => `${name}: ${percentage}%`}
                >
                  {costBreakdown.map((entry, index) => (
                    <Cell key={`cell-${index}`} fill={entry.color} />
                  ))}
                </Pie>
                <RechartsTooltip formatter={(value: number) => [`$${value.toFixed(2)}`, 'Cost']} />
              </PieChart>
            </ResponsiveContainer>
          </Card>
        </Col>
        <Col xs={24} lg={16}>
          <Card title="Cost Trends" size={compact ? 'small' : 'default'}>
            <ResponsiveContainer width="100%" height={250}>
              <AreaChart data={costTrendData}>
                <CartesianGrid strokeDasharray="3 3" />
                <XAxis dataKey="date" />
                <YAxis />
                <RechartsTooltip />
                <Area type="monotone" dataKey="cost" stackId="1" stroke="#1890ff" fill="#1890ff" fillOpacity={0.6} />
                <Area type="monotone" dataKey="savings" stackId="1" stroke="#52c41a" fill="#52c41a" fillOpacity={0.6} />
              </AreaChart>
            </ResponsiveContainer>
          </Card>
        </Col>
      </Row>

      {/* Optimization Recommendations */}
      {showRecommendations && (
        <Card 
          title={
            <Space>
              <BulbOutlined />
              <span>Optimization Recommendations</span>
              <Badge count={optimizations.length} />
            </Space>
          }
          size={compact ? 'small' : 'default'}
        >
          <Alert
            message="AI-Powered Cost Optimization"
            description="These recommendations are generated based on your usage patterns and can help reduce costs while maintaining performance."
            type="info"
            showIcon
            style={{ marginBottom: 16 }}
          />
          
          <Table
            columns={optimizationColumns}
            dataSource={optimizations}
            rowKey="id"
            pagination={false}
            size={compact ? 'small' : 'default'}
          />
        </Card>
      )}

      {/* Cost Projections */}
      {showProjections && (
        <Card 
          title="Cost Projections" 
          style={{ marginTop: 16 }}
          size={compact ? 'small' : 'default'}
        >
          <Row gutter={[16, 16]}>
            <Col xs={24} lg={12}>
              <div>
                <Text strong>Current Trajectory:</Text>
                <div style={{ marginTop: 8 }}>
                  <Progress 
                    percent={(costData.projectedMonthly / costData.monthlyBudget) * 100}
                    strokeColor={costData.projectedMonthly > costData.monthlyBudget ? '#ff4d4f' : '#1890ff'}
                    format={() => `$${costData.projectedMonthly.toFixed(2)}`}
                  />
                  <Text type="secondary" style={{ fontSize: '12px' }}>
                    Projected monthly cost based on current usage
                  </Text>
                </div>
              </div>
            </Col>
            <Col xs={24} lg={12}>
              <div>
                <Text strong>With Optimizations:</Text>
                <div style={{ marginTop: 8 }}>
                  <Progress 
                    percent={((costData.projectedMonthly - totalPotentialSavings) / costData.monthlyBudget) * 100}
                    strokeColor="#52c41a"
                    format={() => `$${(costData.projectedMonthly - totalPotentialSavings).toFixed(2)}`}
                  />
                  <Text type="secondary" style={{ fontSize: '12px' }}>
                    Projected cost after applying all optimizations
                  </Text>
                </div>
              </div>
            </Col>
          </Row>
        </Card>
      )}
    </div>
  )
}

export default CostOptimizationPanel
