import React, { useState, useMemo } from 'react'
import { 
  Card, 
  Typography, 
  Space, 
  Button,
  Alert,
  Row,
  Col,
  Statistic,
  Progress,
  Select,
  DatePicker,
  Tabs,
  List,
  Tag,
  Tooltip,
  Badge
} from 'antd'
import {
  RiseOutlined,
  FallOutlined,
  BarChartOutlined,
  LineChartOutlined,
  PieChartOutlined,
  ThunderboltOutlined,
  ExclamationCircleOutlined,
  CheckCircleOutlined,
  RobotOutlined,
  CalendarOutlined,
  AimOutlined
} from '@ant-design/icons'
import { LineChart, Line, AreaChart, Area, BarChart, Bar, XAxis, YAxis, CartesianGrid, Tooltip as RechartsTooltip, ResponsiveContainer, PieChart, Pie, Cell } from 'recharts'
import { ConfidenceIndicator } from '../common/ConfidenceIndicator'
import { useGetPredictiveAnalyticsQuery, useGenerateForecastMutation } from '@shared/store/api/intelligentAgentsApi'
import type { PredictiveModel, Forecast, PredictionMetrics, TrendAnalysis } from '@shared/types/intelligentAgents'
import dayjs from 'dayjs'

const { Title, Text, Paragraph } = Typography
const { RangePicker } = DatePicker

export interface PredictiveAnalyticsPanelProps {
  dataSource?: string
  metrics?: string[]
  forecastPeriod?: number
  showConfidenceIntervals?: boolean
  showTrendAnalysis?: boolean
  showAnomalyDetection?: boolean
  onForecastGenerate?: (forecast: Forecast) => void
  onModelSelect?: (model: PredictiveModel) => void
  className?: string
  testId?: string
}

/**
 * PredictiveAnalyticsPanel - AI-powered predictive analytics and forecasting
 * 
 * Features:
 * - Advanced time series forecasting with multiple models
 * - Trend analysis and pattern recognition
 * - Anomaly detection and alerting
 * - Confidence intervals and uncertainty quantification
 * - Interactive forecast visualization
 * - Model performance comparison and selection
 */
export const PredictiveAnalyticsPanel: React.FC<PredictiveAnalyticsPanelProps> = ({
  dataSource,
  metrics = ['usage', 'performance', 'cost'],
  forecastPeriod = 30,
  showConfidenceIntervals = true,
  showTrendAnalysis = true,
  showAnomalyDetection = true,
  onForecastGenerate,
  onModelSelect,
  className,
  testId = 'predictive-analytics-panel'
}) => {
  const [selectedMetric, setSelectedMetric] = useState(metrics[0])
  const [selectedModel, setSelectedModel] = useState<string>('auto')
  const [forecastHorizon, setForecastHorizon] = useState(forecastPeriod)
  const [activeTab, setActiveTab] = useState('forecast')

  // Real API data
  const { data: analytics, isLoading: analyticsLoading, refetch: refetchAnalytics } = 
    useGetPredictiveAnalyticsQuery({ 
      dataSource,
      metric: selectedMetric,
      forecastDays: forecastHorizon,
      model: selectedModel === 'auto' ? undefined : selectedModel
    })
  
  const [generateForecast, { isLoading: generateLoading }] = useGenerateForecastMutation()

  // Process forecast data for visualization
  const forecastData = useMemo(() => {
    if (!analytics?.forecast) return []
    
    return analytics.forecast.predictions.map((prediction, index) => ({
      date: dayjs().add(index + 1, 'day').format('YYYY-MM-DD'),
      predicted: prediction.value,
      upperBound: prediction.upperBound,
      lowerBound: prediction.lowerBound,
      confidence: prediction.confidence
    }))
  }, [analytics])

  // Historical data with predictions
  const combinedData = useMemo(() => {
    if (!analytics?.historical || !forecastData.length) return []
    
    const historical = analytics.historical.map(point => ({
      date: point.date,
      actual: point.value,
      type: 'historical'
    }))
    
    const forecast = forecastData.map(point => ({
      date: point.date,
      predicted: point.predicted,
      upperBound: point.upperBound,
      lowerBound: point.lowerBound,
      type: 'forecast'
    }))
    
    return [...historical, ...forecast]
  }, [analytics, forecastData])

  // Calculate prediction metrics
  const predictionMetrics = useMemo(() => {
    if (!analytics?.metrics) return null
    
    return {
      accuracy: analytics.metrics.accuracy,
      mape: analytics.metrics.mape,
      rmse: analytics.metrics.rmse,
      trend: analytics.metrics.trend,
      seasonality: analytics.metrics.seasonality,
      volatility: analytics.metrics.volatility
    }
  }, [analytics])

  // Handle forecast generation
  const handleGenerateForecast = async () => {
    try {
      const result = await generateForecast({
        dataSource,
        metric: selectedMetric,
        forecastDays: forecastHorizon,
        model: selectedModel === 'auto' ? undefined : selectedModel
      }).unwrap()
      
      onForecastGenerate?.(result)
      refetchAnalytics()
    } catch (error) {
      console.error('Failed to generate forecast:', error)
    }
  }

  // Get trend color
  const getTrendColor = (trend: string) => {
    switch (trend) {
      case 'increasing': return '#52c41a'
      case 'decreasing': return '#ff4d4f'
      case 'stable': return '#1890ff'
      default: return '#d9d9d9'
    }
  }

  // Get trend icon
  const getTrendIcon = (trend: string) => {
    switch (trend) {
      case 'increasing': return <RiseOutlined />
      case 'decreasing': return <FallOutlined />
      case 'stable': return <BarChartOutlined />
      default: return <BarChartOutlined />
    }
  }

  // Render prediction metrics
  const renderPredictionMetrics = () => {
    if (!predictionMetrics) return null

    return (
      <Row gutter={[16, 16]} style={{ marginBottom: 24 }}>
        <Col xs={24} sm={12} lg={6}>
          <Card>
            <Statistic
              title="Model Accuracy"
              value={predictionMetrics.accuracy * 100}
              suffix="%"
              precision={1}
              prefix={<AimOutlined />}
              valueStyle={{ color: predictionMetrics.accuracy > 0.8 ? '#52c41a' : '#faad14' }}
            />
          </Card>
        </Col>
        <Col xs={24} sm={12} lg={6}>
          <Card>
            <Statistic
              title="Trend Direction"
              value={predictionMetrics.trend}
              prefix={getTrendIcon(predictionMetrics.trend)}
              valueStyle={{ color: getTrendColor(predictionMetrics.trend) }}
            />
          </Card>
        </Col>
        <Col xs={24} sm={12} lg={6}>
          <Card>
            <Statistic
              title="Volatility"
              value={predictionMetrics.volatility * 100}
              suffix="%"
              precision={1}
              prefix={<ThunderboltOutlined />}
              valueStyle={{ color: predictionMetrics.volatility > 0.3 ? '#ff4d4f' : '#52c41a' }}
            />
          </Card>
        </Col>
        <Col xs={24} sm={12} lg={6}>
          <Card>
            <Statistic
              title="Seasonality"
              value={predictionMetrics.seasonality ? 'Detected' : 'None'}
              prefix={<CalendarOutlined />}
              valueStyle={{ color: predictionMetrics.seasonality ? '#1890ff' : '#d9d9d9' }}
            />
          </Card>
        </Col>
      </Row>
    )
  }

  // Render forecast chart
  const renderForecastChart = () => (
    <Card 
      title={
        <Space>
          <LineChartOutlined />
          <span>Forecast Visualization</span>
          <ConfidenceIndicator
            confidence={analytics?.forecast?.confidence || 0}
            size="small"
            type="badge"
          />
        </Space>
      }
      extra={
        <Space>
          <Select
            value={selectedMetric}
            onChange={setSelectedMetric}
            style={{ width: 120 }}
          >
            {metrics.map(metric => (
              <Select.Option key={metric} value={metric}>
                {metric.charAt(0).toUpperCase() + metric.slice(1)}
              </Select.Option>
            ))}
          </Select>
          <Select
            value={forecastHorizon}
            onChange={setForecastHorizon}
            style={{ width: 100 }}
          >
            <Select.Option value={7}>7 days</Select.Option>
            <Select.Option value={14}>14 days</Select.Option>
            <Select.Option value={30}>30 days</Select.Option>
            <Select.Option value={90}>90 days</Select.Option>
          </Select>
        </Space>
      }
    >
      <ResponsiveContainer width="100%" height={400}>
        <LineChart data={combinedData}>
          <CartesianGrid strokeDasharray="3 3" />
          <XAxis dataKey="date" />
          <YAxis />
          <RechartsTooltip />
          <Line 
            type="monotone" 
            dataKey="actual" 
            stroke="#1890ff" 
            strokeWidth={2}
            dot={false}
          />
          <Line 
            type="monotone" 
            dataKey="predicted" 
            stroke="#52c41a" 
            strokeWidth={2}
            strokeDasharray="5 5"
            dot={false}
          />
          {showConfidenceIntervals && (
            <>
              <Line 
                type="monotone" 
                dataKey="upperBound" 
                stroke="#52c41a" 
                strokeWidth={1}
                strokeOpacity={0.5}
                dot={false}
              />
              <Line 
                type="monotone" 
                dataKey="lowerBound" 
                stroke="#52c41a" 
                strokeWidth={1}
                strokeOpacity={0.5}
                dot={false}
              />
            </>
          )}
        </LineChart>
      </ResponsiveContainer>
    </Card>
  )

  // Render trend analysis
  const renderTrendAnalysis = () => {
    if (!showTrendAnalysis || !analytics?.trendAnalysis) return null

    return (
      <Card 
        title={
          <Space>
            <RiseOutlined />
            <span>Trend Analysis</span>
          </Space>
        }
        style={{ marginTop: 16 }}
      >
        <List
          dataSource={analytics.trendAnalysis.patterns}
          renderItem={(pattern) => (
            <List.Item>
              <List.Item.Meta
                avatar={
                  <div style={{ display: 'flex', alignItems: 'center' }}>
                    {getTrendIcon(pattern.type)}
                  </div>
                }
                title={
                  <Space>
                    <Text strong>{pattern.name}</Text>
                    <Tag color={getTrendColor(pattern.type)}>
                      {pattern.type.toUpperCase()}
                    </Tag>
                    <ConfidenceIndicator
                      confidence={pattern.confidence}
                      size="small"
                      type="badge"
                      showPercentage={false}
                    />
                  </Space>
                }
                description={
                  <div>
                    <Paragraph style={{ marginBottom: 4 }}>
                      {pattern.description}
                    </Paragraph>
                    <Text type="secondary" style={{ fontSize: '12px' }}>
                      Duration: {pattern.duration} • Impact: {pattern.impact}
                    </Text>
                  </div>
                }
              />
            </List.Item>
          )}
        />
      </Card>
    )
  }

  // Render anomaly detection
  const renderAnomalyDetection = () => {
    if (!showAnomalyDetection || !analytics?.anomalies?.length) return null

    return (
      <Card 
        title={
          <Space>
            <ExclamationCircleOutlined />
            <span>Anomaly Detection</span>
            <Badge count={analytics.anomalies.length} />
          </Space>
        }
        style={{ marginTop: 16 }}
      >
        <List
          dataSource={analytics.anomalies}
          renderItem={(anomaly) => (
            <List.Item>
              <Alert
                message={anomaly.title}
                description={
                  <div>
                    <Text>{anomaly.description}</Text>
                    <div style={{ marginTop: 4 }}>
                      <Space>
                        <Text type="secondary">Severity:</Text>
                        <Tag color={anomaly.severity === 'high' ? 'red' : anomaly.severity === 'medium' ? 'orange' : 'blue'}>
                          {anomaly.severity.toUpperCase()}
                        </Tag>
                        <Text type="secondary">Detected:</Text>
                        <Text>{dayjs(anomaly.detectedAt).format('MMM DD, HH:mm')}</Text>
                      </Space>
                    </div>
                  </div>
                }
                type={anomaly.severity === 'high' ? 'error' : 'warning'}
                showIcon
                style={{ marginBottom: 8 }}
              />
            </List.Item>
          )}
        />
      </Card>
    )
  }

  const tabItems = [
    {
      key: 'forecast',
      label: (
        <Space>
          <LineChartOutlined />
          <span>Forecast</span>
        </Space>
      ),
      children: (
        <div>
          {renderPredictionMetrics()}
          {renderForecastChart()}
        </div>
      )
    },
    ...(showTrendAnalysis ? [{
      key: 'trends',
      label: (
        <Space>
          <RiseOutlined />
          <span>Trends</span>
        </Space>
      ),
      children: renderTrendAnalysis()
    }] : []),
    ...(showAnomalyDetection ? [{
      key: 'anomalies',
      label: (
        <Space>
          <ExclamationCircleOutlined />
          <span>Anomalies</span>
          {analytics?.anomalies?.length ? (
            <Badge count={analytics.anomalies.length} size="small" />
          ) : null}
        </Space>
      ),
      children: renderAnomalyDetection()
    }] : [])
  ]

  return (
    <div className={className} data-testid={testId}>
      {/* Header */}
      <div style={{ 
        display: 'flex', 
        justifyContent: 'space-between', 
        alignItems: 'center',
        marginBottom: 16 
      }}>
        <Title level={5} style={{ margin: 0 }}>
          Predictive Analytics
        </Title>
        <Space>
          <Select
            value={selectedModel}
            onChange={setSelectedModel}
            style={{ width: 120 }}
          >
            <Select.Option value="auto">Auto Select</Select.Option>
            <Select.Option value="arima">ARIMA</Select.Option>
            <Select.Option value="lstm">LSTM</Select.Option>
            <Select.Option value="prophet">Prophet</Select.Option>
          </Select>
          <Button 
            type="primary"
            icon={<ThunderboltOutlined />}
            loading={generateLoading}
            onClick={handleGenerateForecast}
          >
            Generate Forecast
          </Button>
        </Space>
      </div>

      {/* Data Source Context */}
      {dataSource && (
        <Alert
          message="Predictive Analysis"
          description={`AI is analyzing data from "${dataSource}" to generate forecasts and identify patterns.`}
          type="info"
          showIcon
          style={{ marginBottom: 16 }}
        />
      )}

      {/* Loading State */}
      {analyticsLoading && (
        <Card style={{ textAlign: 'center', padding: '40px 0' }}>
          <Space direction="vertical">
            <RobotOutlined style={{ fontSize: '48px', color: '#1890ff' }} />
            <Text>AI is generating predictive analytics...</Text>
            <Progress percent={80} status="active" />
          </Space>
        </Card>
      )}

      {/* Analytics Content */}
      {analytics && !analyticsLoading && (
        <Tabs
          activeKey={activeTab}
          onChange={setActiveTab}
          items={tabItems}
        />
      )}

      {/* No Data Available */}
      {!analytics && !analyticsLoading && (
        <Card style={{ textAlign: 'center', padding: '40px 0' }}>
          <Space direction="vertical">
            <BarChartOutlined style={{ fontSize: '48px', color: '#d9d9d9' }} />
            <Title level={4}>No Predictive Data Available</Title>
            <Text type="secondary">
              Generate a forecast to see predictive analytics and trends.
            </Text>
            <Button 
              type="primary"
              icon={<ThunderboltOutlined />}
              onClick={handleGenerateForecast}
              loading={generateLoading}
            >
              Generate Forecast
            </Button>
          </Space>
        </Card>
      )}
    </div>
  )
}

export default PredictiveAnalyticsPanel
