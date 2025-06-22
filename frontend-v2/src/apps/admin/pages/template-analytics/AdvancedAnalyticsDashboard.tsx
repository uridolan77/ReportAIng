import React, { useState, useEffect } from 'react'
import { 
  Card, 
  Row, 
  Col, 
  Typography, 
  Select, 
  DatePicker, 
  Button, 
  Space, 
  Alert, 
  Tabs,
  Table,
  Tag,
  Progress,
  Statistic,
  Tooltip,
  Badge,
  List,
  Avatar
} from 'antd'
import {
  RocketOutlined,
  RiseOutlined,
  RobotOutlined,
  AlertOutlined,
  BarChartOutlined,
  LineChartOutlined,
  PieChartOutlined,
  ThunderboltOutlined,
  BulbOutlined,
  FireOutlined,
  EyeOutlined,
  SettingOutlined,
  ClockCircleOutlined,
  UserOutlined
} from '@ant-design/icons'
import dayjs from 'dayjs'
import { 
  PerformanceLineChart, 
  PerformanceBarChart, 
  PerformancePieChart,
  PerformanceAreaChart,
  MetricCard 
} from '@shared/components/charts/PerformanceChart'

import PredictiveInsights from '../../components/template-analytics/PredictiveInsights'
import PerformanceOptimizer from '../../components/template-analytics/PerformanceOptimizer'
import RealTimeMonitoring from '../../components/template-analytics/RealTimeMonitoring'
import AIRecommendationEngine from '../../components/template-analytics/AIRecommendationEngine'

const { Title, Text } = Typography
const { RangePicker } = DatePicker

export const AdvancedAnalyticsDashboard: React.FC = () => {
  const [timeRange, setTimeRange] = useState<[dayjs.Dayjs, dayjs.Dayjs]>([
    dayjs().subtract(30, 'day'),
    dayjs()
  ])
  const [selectedMetric, setSelectedMetric] = useState('performance')
  const [activeTab, setActiveTab] = useState('overview')
  const [isRealTimeEnabled, setIsRealTimeEnabled] = useState(true)

  // Mock advanced analytics data
  const [analyticsData, setAnalyticsData] = useState({
    predictiveMetrics: {
      expectedGrowth: 15.3,
      riskScore: 23,
      optimizationPotential: 67,
      forecastAccuracy: 94.2
    },
    performanceTrends: [
      { date: '2024-01-01', successRate: 85, responseTime: 1.2, usageCount: 450 },
      { date: '2024-01-02', successRate: 87, responseTime: 1.1, usageCount: 520 },
      { date: '2024-01-03', successRate: 89, responseTime: 1.0, usageCount: 580 },
      { date: '2024-01-04', successRate: 91, responseTime: 0.9, usageCount: 620 },
      { date: '2024-01-05', successRate: 93, responseTime: 0.8, usageCount: 680 }
    ],
    aiInsights: [
      {
        type: 'optimization',
        priority: 'high',
        title: 'SQL Template Performance Opportunity',
        description: 'SQL generation templates show 23% improvement potential through prompt optimization',
        impact: 'High',
        effort: 'Medium',
        recommendation: 'Implement context-aware prompt engineering'
      },
      {
        type: 'anomaly',
        priority: 'medium',
        title: 'Unusual Error Pattern Detected',
        description: 'Insight generation templates showing increased errors during peak hours',
        impact: 'Medium',
        effort: 'Low',
        recommendation: 'Implement load balancing for peak hour traffic'
      },
      {
        type: 'trend',
        priority: 'low',
        title: 'Emerging Usage Pattern',
        description: 'New use case detected: Financial reporting templates gaining popularity',
        impact: 'Low',
        effort: 'High',
        recommendation: 'Consider creating specialized financial templates'
      }
    ],
    realTimeMetrics: {
      activeUsers: 47,
      templatesInUse: 23,
      avgResponseTime: 0.85,
      errorRate: 2.3,
      throughput: 156
    }
  })

  useEffect(() => {
    // Simulate real-time data updates
    if (isRealTimeEnabled) {
      const interval = setInterval(() => {
        setAnalyticsData(prev => ({
          ...prev,
          realTimeMetrics: {
            ...prev.realTimeMetrics,
            activeUsers: prev.realTimeMetrics.activeUsers + Math.floor(Math.random() * 10 - 5),
            avgResponseTime: Math.max(0.1, prev.realTimeMetrics.avgResponseTime + (Math.random() - 0.5) * 0.1),
            errorRate: Math.max(0, prev.realTimeMetrics.errorRate + (Math.random() - 0.5) * 0.5),
            throughput: Math.max(0, prev.realTimeMetrics.throughput + Math.floor(Math.random() * 20 - 10))
          }
        }))
      }, 5000)

      return () => clearInterval(interval)
    }
  }, [isRealTimeEnabled])

  const getPriorityColor = (priority: string) => {
    switch (priority) {
      case 'high': return '#f5222d'
      case 'medium': return '#fa8c16'
      case 'low': return '#52c41a'
      default: return '#1890ff'
    }
  }

  const getInsightIcon = (type: string) => {
    switch (type) {
      case 'optimization': return <RocketOutlined />
      case 'anomaly': return <AlertOutlined />
      case 'trend': return <RiseOutlined />
      default: return <BulbOutlined />
    }
  }

  const overviewTab = (
    <div>
      {/* Predictive Metrics */}
      <Row gutter={16} style={{ marginBottom: '24px' }}>
        <Col span={6}>
          <MetricCard
            title="Expected Growth"
            value={`+${analyticsData.predictiveMetrics.expectedGrowth}%`}
            subtitle="Next 30 days"
            icon={<RiseOutlined />}
            color="#52c41a"
            trend={{ value: 12.5, isPositive: true }}
          />
        </Col>
        <Col span={6}>
          <MetricCard
            title="Risk Score"
            value={analyticsData.predictiveMetrics.riskScore}
            subtitle="Low risk"
            icon={<AlertOutlined />}
            color="#1890ff"
            trend={{ value: 5.2, isPositive: false }}
          />
        </Col>
        <Col span={6}>
          <MetricCard
            title="Optimization Potential"
            value={`${analyticsData.predictiveMetrics.optimizationPotential}%`}
            subtitle="Performance gain"
            icon={<RocketOutlined />}
            color="#722ed1"
            trend={{ value: 8.3, isPositive: true }}
          />
        </Col>
        <Col span={6}>
          <MetricCard
            title="Forecast Accuracy"
            value={`${analyticsData.predictiveMetrics.forecastAccuracy}%`}
            subtitle="Model confidence"
            icon={<BrainOutlined />}
            color="#fa8c16"
            trend={{ value: 2.1, isPositive: true }}
          />
        </Col>
      </Row>

      {/* Performance Trends */}
      <Row gutter={16} style={{ marginBottom: '24px' }}>
        <Col span={16}>
          <Card title="Performance Trends & Predictions" extra={
            <Space>
              <Select value={selectedMetric} onChange={setSelectedMetric} size="small">
                <Select.Option value="performance">Performance</Select.Option>
                <Select.Option value="usage">Usage</Select.Option>
                <Select.Option value="errors">Errors</Select.Option>
              </Select>
              <RangePicker 
                value={timeRange} 
                onChange={(dates) => dates && setTimeRange(dates)}
                size="small"
              />
            </Space>
          }>
            <PerformanceLineChart
              data={analyticsData.performanceTrends}
              xAxisKey="date"
              lines={[
                { key: 'successRate', color: '#52c41a', name: 'Success Rate (%)' },
                { key: 'responseTime', color: '#1890ff', name: 'Response Time (s)' },
                { key: 'usageCount', color: '#722ed1', name: 'Usage Count' }
              ]}
              height={300}
            />
          </Card>
        </Col>
        <Col span={8}>
          <Card title="Real-Time Metrics" extra={
            <Badge status={isRealTimeEnabled ? 'processing' : 'default'} text="Live" />
          }>
            <Space direction="vertical" style={{ width: '100%' }}>
              <div style={{ display: 'flex', justifyContent: 'space-between' }}>
                <Text>Active Users</Text>
                <Text strong>{analyticsData.realTimeMetrics.activeUsers}</Text>
              </div>
              <div style={{ display: 'flex', justifyContent: 'space-between' }}>
                <Text>Templates in Use</Text>
                <Text strong>{analyticsData.realTimeMetrics.templatesInUse}</Text>
              </div>
              <div style={{ display: 'flex', justifyContent: 'space-between' }}>
                <Text>Avg Response Time</Text>
                <Text strong>{analyticsData.realTimeMetrics.avgResponseTime.toFixed(2)}s</Text>
              </div>
              <div style={{ display: 'flex', justifyContent: 'space-between' }}>
                <Text>Error Rate</Text>
                <Text strong style={{ color: analyticsData.realTimeMetrics.errorRate > 5 ? '#f5222d' : '#52c41a' }}>
                  {analyticsData.realTimeMetrics.errorRate.toFixed(1)}%
                </Text>
              </div>
              <div style={{ display: 'flex', justifyContent: 'space-between' }}>
                <Text>Throughput</Text>
                <Text strong>{analyticsData.realTimeMetrics.throughput}/min</Text>
              </div>
            </Space>
          </Card>
        </Col>
      </Row>

      {/* AI Insights */}
      <Card 
        title={
          <Space>
            <RobotOutlined />
            AI-Powered Insights
            <Badge count={analyticsData.aiInsights.length} />
          </Space>
        }
        extra={
          <Button size="small" icon={<SettingOutlined />}>
            Configure AI
          </Button>
        }
      >
        <List
          dataSource={analyticsData.aiInsights}
          renderItem={(insight, index) => (
            <List.Item
              key={index}
              actions={[
                <Button size="small" type="link" icon={<EyeOutlined />}>
                  View Details
                </Button>,
                <Button size="small" type="primary">
                  Apply
                </Button>
              ]}
            >
              <List.Item.Meta
                avatar={
                  <Avatar 
                    icon={getInsightIcon(insight.type)} 
                    style={{ backgroundColor: getPriorityColor(insight.priority) }}
                  />
                }
                title={
                  <Space>
                    <Text strong>{insight.title}</Text>
                    <Tag color={getPriorityColor(insight.priority)}>
                      {insight.priority.toUpperCase()}
                    </Tag>
                  </Space>
                }
                description={
                  <div>
                    <Text>{insight.description}</Text>
                    <div style={{ marginTop: '8px' }}>
                      <Space>
                        <Tag>Impact: {insight.impact}</Tag>
                        <Tag>Effort: {insight.effort}</Tag>
                      </Space>
                    </div>
                    <div style={{ marginTop: '4px' }}>
                      <Text strong>Recommendation: </Text>
                      <Text>{insight.recommendation}</Text>
                    </div>
                  </div>
                }
              />
            </List.Item>
          )}
        />
      </Card>
    </div>
  )

  const tabs = [
    {
      key: 'overview',
      label: (
        <Space>
          <BarChartOutlined />
          Overview
        </Space>
      ),
      children: overviewTab
    },
    {
      key: 'predictive',
      label: (
        <Space>
          <BrainOutlined />
          Predictive Insights
        </Space>
      ),
      children: <PredictiveInsights />
    },
    {
      key: 'optimization',
      label: (
        <Space>
          <RocketOutlined />
          Performance Optimizer
        </Space>
      ),
      children: <PerformanceOptimizer />
    },
    {
      key: 'monitoring',
      label: (
        <Space>
          <ThunderboltOutlined />
          Real-Time Monitoring
        </Space>
      ),
      children: <RealTimeMonitoring />
    },
    {
      key: 'ai-engine',
      label: (
        <Space>
          <FireOutlined />
          AI Recommendation Engine
        </Space>
      ),
      children: <AIRecommendationEngine />
    }
  ]

  return (
    <div>
        {/* Header */}
        <div style={{ marginBottom: '24px' }}>
          <Row gutter={16} align="middle">
            <Col span={16}>
              <Title level={2} style={{ margin: 0 }}>
                Advanced Analytics Dashboard
              </Title>
              <Text type="secondary">
                AI-powered insights, predictive analytics, and performance optimization
              </Text>
            </Col>
            <Col span={8} style={{ textAlign: 'right' }}>
              <Space>
                <Button icon={<ClockCircleOutlined />}>
                  Schedule Report
                </Button>
                <Button icon={<SettingOutlined />}>
                  Configure Alerts
                </Button>
                <Button type="primary" icon={<BrainOutlined />}>
                  AI Assistant
                </Button>
              </Space>
            </Col>
          </Row>
        </div>

        {/* Alert Banner */}
        <Alert
          message="Advanced Analytics Active"
          description="AI-powered insights are continuously analyzing your template performance. New recommendations will appear automatically."
          type="info"
          showIcon
          closable
          style={{ marginBottom: '24px' }}
        />

        {/* Main Content */}
        <Tabs
          activeKey={activeTab}
          onChange={setActiveTab}
          items={tabs}
          size="large"
        />
      </div>
  )
}

export default AdvancedAnalyticsDashboard
