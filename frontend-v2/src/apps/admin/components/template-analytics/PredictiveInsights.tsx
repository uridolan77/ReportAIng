import React, { useState, useEffect } from 'react'
import { 
  Card, 
  Row, 
  Col, 
  Typography, 
  Progress, 
  Alert, 
  Table, 
  Tag, 
  Button,
  Space,
  Tooltip,
  Select,
  Statistic,
  Timeline,
  Badge
} from 'antd'
import {
  RiseOutlined,
  FallOutlined,
  RobotOutlined,
  AlertOutlined,
  RocketOutlined,
  ClockCircleOutlined,
  ThunderboltOutlined,
  BarChartOutlined,
  LineChartOutlined,
  InfoCircleOutlined
} from '@ant-design/icons'
import { PerformanceLineChart, PerformanceAreaChart } from '@shared/components/charts/PerformanceChart'

const { Title, Text } = Typography

interface PredictionModel {
  name: string
  accuracy: number
  lastTrained: string
  status: 'active' | 'training' | 'outdated'
}

interface Forecast {
  metric: string
  current: number
  predicted: number
  confidence: number
  trend: 'up' | 'down' | 'stable'
  timeframe: string
}

interface RiskFactor {
  factor: string
  severity: 'low' | 'medium' | 'high' | 'critical'
  probability: number
  impact: string
  mitigation: string
}

export const PredictiveInsights: React.FC = () => {
  const [selectedTimeframe, setSelectedTimeframe] = useState('30d')
  const [selectedModel, setSelectedModel] = useState('ensemble')

  // Mock predictive data
  const [predictionModels] = useState<PredictionModel[]>([
    { name: 'Performance Predictor', accuracy: 94.2, lastTrained: '2024-01-15', status: 'active' },
    { name: 'Usage Forecaster', accuracy: 91.8, lastTrained: '2024-01-14', status: 'active' },
    { name: 'Error Rate Predictor', accuracy: 88.5, lastTrained: '2024-01-13', status: 'training' },
    { name: 'Capacity Planner', accuracy: 96.1, lastTrained: '2024-01-12', status: 'active' }
  ])

  const [forecasts] = useState<Forecast[]>([
    {
      metric: 'Template Usage',
      current: 1250,
      predicted: 1580,
      confidence: 92,
      trend: 'up',
      timeframe: '30 days'
    },
    {
      metric: 'Success Rate',
      current: 89.5,
      predicted: 91.2,
      confidence: 88,
      trend: 'up',
      timeframe: '30 days'
    },
    {
      metric: 'Response Time',
      current: 1.2,
      predicted: 1.0,
      confidence: 85,
      trend: 'down',
      timeframe: '30 days'
    },
    {
      metric: 'Error Rate',
      current: 3.2,
      predicted: 2.8,
      confidence: 90,
      trend: 'down',
      timeframe: '30 days'
    }
  ])

  const [riskFactors] = useState<RiskFactor[]>([
    {
      factor: 'Peak Hour Overload',
      severity: 'high',
      probability: 78,
      impact: 'Response time degradation during 9-11 AM',
      mitigation: 'Implement auto-scaling for template processing'
    },
    {
      factor: 'Model Drift',
      severity: 'medium',
      probability: 45,
      impact: 'Gradual decrease in template accuracy',
      mitigation: 'Schedule monthly model retraining'
    },
    {
      factor: 'Data Quality Issues',
      severity: 'low',
      probability: 23,
      impact: 'Inconsistent template outputs',
      mitigation: 'Enhance input validation rules'
    }
  ])

  // Mock forecast data for charts
  const forecastData = [
    { date: '2024-01-01', actual: 1200, predicted: 1180, confidence_upper: 1250, confidence_lower: 1110 },
    { date: '2024-01-02', actual: 1250, predicted: 1220, confidence_upper: 1290, confidence_lower: 1150 },
    { date: '2024-01-03', actual: 1180, predicted: 1200, confidence_upper: 1270, confidence_lower: 1130 },
    { date: '2024-01-04', actual: null, predicted: 1280, confidence_upper: 1350, confidence_lower: 1210 },
    { date: '2024-01-05', actual: null, predicted: 1320, confidence_upper: 1390, confidence_lower: 1250 },
    { date: '2024-01-06', actual: null, predicted: 1380, confidence_upper: 1450, confidence_lower: 1310 },
    { date: '2024-01-07', actual: null, predicted: 1420, confidence_upper: 1490, confidence_lower: 1350 }
  ]

  const getTrendIcon = (trend: string) => {
    switch (trend) {
      case 'up': return <RiseOutlined style={{ color: '#52c41a' }} />
      case 'down': return <FallOutlined style={{ color: '#f5222d' }} />
      default: return <BarChartOutlined style={{ color: '#1890ff' }} />
    }
  }

  const getSeverityColor = (severity: string) => {
    switch (severity) {
      case 'critical': return '#f5222d'
      case 'high': return '#fa8c16'
      case 'medium': return '#faad14'
      case 'low': return '#52c41a'
      default: return '#d9d9d9'
    }
  }

  const getModelStatusColor = (status: string) => {
    switch (status) {
      case 'active': return 'green'
      case 'training': return 'blue'
      case 'outdated': return 'red'
      default: return 'default'
    }
  }

  const forecastColumns = [
    {
      title: 'Metric',
      dataIndex: 'metric',
      key: 'metric',
      render: (text: string) => <Text strong>{text}</Text>
    },
    {
      title: 'Current Value',
      dataIndex: 'current',
      key: 'current',
      render: (value: number, record: Forecast) => (
        <Text>{typeof value === 'number' && value < 10 ? value.toFixed(1) : value}</Text>
      )
    },
    {
      title: 'Predicted Value',
      dataIndex: 'predicted',
      key: 'predicted',
      render: (value: number, record: Forecast) => (
        <Space>
          {getTrendIcon(record.trend)}
          <Text strong style={{ color: record.trend === 'up' ? '#52c41a' : record.trend === 'down' ? '#f5222d' : '#1890ff' }}>
            {typeof value === 'number' && value < 10 ? value.toFixed(1) : value}
          </Text>
        </Space>
      )
    },
    {
      title: 'Confidence',
      dataIndex: 'confidence',
      key: 'confidence',
      render: (confidence: number) => (
        <div style={{ width: '100px' }}>
          <Progress 
            percent={confidence} 
            size="small" 
            strokeColor={confidence > 90 ? '#52c41a' : confidence > 80 ? '#1890ff' : '#fa8c16'}
          />
        </div>
      )
    },
    {
      title: 'Change',
      key: 'change',
      render: (record: Forecast) => {
        const change = ((record.predicted - record.current) / record.current * 100)
        return (
          <Tag color={change > 0 ? 'green' : change < 0 ? 'red' : 'blue'}>
            {change > 0 ? '+' : ''}{change.toFixed(1)}%
          </Tag>
        )
      }
    }
  ]

  const riskColumns = [
    {
      title: 'Risk Factor',
      dataIndex: 'factor',
      key: 'factor',
      render: (text: string) => <Text strong>{text}</Text>
    },
    {
      title: 'Severity',
      dataIndex: 'severity',
      key: 'severity',
      render: (severity: string) => (
        <Tag color={getSeverityColor(severity)}>
          {severity.toUpperCase()}
        </Tag>
      )
    },
    {
      title: 'Probability',
      dataIndex: 'probability',
      key: 'probability',
      render: (probability: number) => (
        <div style={{ width: '80px' }}>
          <Progress 
            percent={probability} 
            size="small" 
            strokeColor={probability > 70 ? '#f5222d' : probability > 40 ? '#fa8c16' : '#52c41a'}
          />
        </div>
      )
    },
    {
      title: 'Impact',
      dataIndex: 'impact',
      key: 'impact'
    },
    {
      title: 'Mitigation',
      dataIndex: 'mitigation',
      key: 'mitigation',
      render: (text: string) => (
        <Tooltip title={text}>
          <Text ellipsis style={{ maxWidth: '200px' }}>{text}</Text>
        </Tooltip>
      )
    }
  ]

  return (
    <div>
      {/* Controls */}
      <Row gutter={16} style={{ marginBottom: '24px' }}>
        <Col span={12}>
          <Space>
            <Text strong>Prediction Timeframe:</Text>
            <Select value={selectedTimeframe} onChange={setSelectedTimeframe}>
              <Select.Option value="7d">7 Days</Select.Option>
              <Select.Option value="30d">30 Days</Select.Option>
              <Select.Option value="90d">90 Days</Select.Option>
              <Select.Option value="1y">1 Year</Select.Option>
            </Select>
          </Space>
        </Col>
        <Col span={12}>
          <Space>
            <Text strong>Prediction Model:</Text>
            <Select value={selectedModel} onChange={setSelectedModel}>
              <Select.Option value="ensemble">Ensemble Model</Select.Option>
              <Select.Option value="neural">Neural Network</Select.Option>
              <Select.Option value="regression">Linear Regression</Select.Option>
              <Select.Option value="arima">ARIMA</Select.Option>
            </Select>
          </Space>
        </Col>
      </Row>

      {/* Model Status */}
      <Card title="Prediction Models Status" style={{ marginBottom: '24px' }}>
        <Row gutter={16}>
          {predictionModels.map((model, index) => (
            <Col span={6} key={index}>
              <Card size="small">
                <div style={{ textAlign: 'center' }}>
                  <div style={{ marginBottom: '8px' }}>
                    <Badge status={getModelStatusColor(model.status)} />
                    <Text strong>{model.name}</Text>
                  </div>
                  <Statistic
                    value={model.accuracy}
                    suffix="%"
                    precision={1}
                    valueStyle={{ fontSize: '20px' }}
                  />
                  <Text type="secondary" style={{ fontSize: '12px' }}>
                    Last trained: {model.lastTrained}
                  </Text>
                </div>
              </Card>
            </Col>
          ))}
        </Row>
      </Card>

      {/* Forecasts */}
      <Row gutter={16} style={{ marginBottom: '24px' }}>
        <Col span={16}>
          <Card title="Usage Forecast with Confidence Intervals">
            <PerformanceAreaChart
              data={forecastData}
              xAxisKey="date"
              areas={[
                { key: 'confidence_upper', color: '#1890ff', name: 'Upper Confidence' },
                { key: 'predicted', color: '#52c41a', name: 'Predicted' },
                { key: 'confidence_lower', color: '#1890ff', name: 'Lower Confidence' },
                { key: 'actual', color: '#722ed1', name: 'Actual' }
              ]}
              height={300}
            />
          </Card>
        </Col>
        <Col span={8}>
          <Card title="Key Predictions">
            <Timeline>
              <Timeline.Item color="green">
                <Text strong>Next 7 days</Text>
                <div>15% increase in template usage expected</div>
              </Timeline.Item>
              <Timeline.Item color="blue">
                <Text strong>Next 30 days</Text>
                <div>Performance optimization will improve success rate by 2%</div>
              </Timeline.Item>
              <Timeline.Item color="orange">
                <Text strong>Next 90 days</Text>
                <div>New template categories may be needed for emerging use cases</div>
              </Timeline.Item>
            </Timeline>
          </Card>
        </Col>
      </Row>

      {/* Detailed Forecasts Table */}
      <Card title="Detailed Forecasts" style={{ marginBottom: '24px' }}>
        <Table
          columns={forecastColumns}
          dataSource={forecasts}
          pagination={false}
          size="small"
        />
      </Card>

      {/* Risk Assessment */}
      <Card 
        title={
          <Space>
            <AlertOutlined />
            Risk Assessment
          </Space>
        }
        extra={
          <Button size="small" icon={<InfoCircleOutlined />}>
            Risk Methodology
          </Button>
        }
      >
        <Alert
          message="Predictive Risk Analysis"
          description="AI models continuously analyze patterns to identify potential risks and their likelihood. Take proactive measures to mitigate high-probability risks."
          type="info"
          showIcon
          style={{ marginBottom: '16px' }}
        />
        <Table
          columns={riskColumns}
          dataSource={riskFactors}
          pagination={false}
          size="small"
        />
      </Card>
    </div>
  )
}

export default PredictiveInsights
