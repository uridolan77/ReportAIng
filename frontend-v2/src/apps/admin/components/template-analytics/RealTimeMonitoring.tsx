import React, { useState, useEffect, useMemo } from 'react'
import { 
  Card, 
  Row, 
  Col, 
  Typography, 
  Badge, 
  Alert, 
  Table, 
  Tag, 
  Button,
  Space,
  Switch,
  Tooltip,
  Progress,
  Statistic,
  List,
  Avatar,
  Timeline,
  Select
} from 'antd'
import { 
  ThunderboltOutlined, 
  AlertOutlined,
  CheckCircleOutlined,
  CloseCircleOutlined,
  ClockCircleOutlined,
  UserOutlined,
  BarChartOutlined,
  LineChartOutlined,
  PlayCircleOutlined,
  PauseCircleOutlined,
  SettingOutlined,
  BellOutlined,
  EyeOutlined
} from '@ant-design/icons'
import { PerformanceLineChart } from '@shared/components/charts/PerformanceChart'
import {
  useGetRealTimeAnalyticsQuery
} from '@shared/store/api/templateAnalyticsApi'

const { Title, Text } = Typography

interface RealTimeMetric {
  name: string
  value: number
  unit: string
  status: 'normal' | 'warning' | 'critical'
  trend: 'up' | 'down' | 'stable'
  threshold: number
}

interface SystemEvent {
  id: string
  timestamp: string
  type: 'error' | 'warning' | 'info' | 'success'
  source: string
  message: string
  details?: string
}

interface ActiveSession {
  id: string
  userId: string
  templateKey: string
  startTime: string
  status: 'active' | 'idle' | 'error'
  requestCount: number
  avgResponseTime: number
}

export const RealTimeMonitoring: React.FC = () => {
  const [isMonitoringActive, setIsMonitoringActive] = useState(true)
  const [selectedTimeWindow, setSelectedTimeWindow] = useState('5m')
  const [autoRefresh, setAutoRefresh] = useState(true)

  // Real API calls
  const {
    data: realTimeData,
    isLoading: isRealTimeLoading,
    refetch: refetchRealTime
  } = useGetRealTimeAnalyticsQuery()

  // Real-time metrics from API
  const metrics: RealTimeMetric[] = useMemo(() => {
    if (!realTimeData) {
      return [
        { name: 'Active Users', value: 0, unit: '', status: 'normal', trend: 'stable', threshold: 100 },
        { name: 'Requests/Min', value: 0, unit: '/min', status: 'normal', trend: 'stable', threshold: 200 }
      ]
    }

    return [
      { name: 'Active Users', value: realTimeData.activeUsers || 0, unit: '', status: 'normal', trend: 'up', threshold: 100 },
      { name: 'Requests/Min', value: realTimeData.throughput || 0, unit: '/min', status: 'normal', trend: 'stable', threshold: 200 },
      { name: 'Avg Response Time', value: realTimeData.averageResponseTime || 0, unit: 's', status: 'normal', trend: 'down', threshold: 2.0 },
      { name: 'Error Rate', value: realTimeData.errorRate || 0, unit: '%', status: 'normal', trend: 'down', threshold: 5.0 },
      { name: 'Active Templates', value: realTimeData.activeTemplates || 0, unit: '', status: 'normal', trend: 'stable', threshold: 50 }
    ]
  }, [realTimeData])

  // Real events from API
  const events: SystemEvent[] = useMemo(() => {
    if (!realTimeData?.recentEvents) {
      return []
    }

    return realTimeData.recentEvents.map((event, index) => ({
      id: event.id || `event-${index}`,
      timestamp: event.timestamp || new Date().toISOString(),
      type: event.type as any || 'info',
      source: event.source || 'System',
      message: event.message,
      details: event.details
    }))
  }, [realTimeData])

  // Real active sessions from API
  const activeSessions: ActiveSession[] = useMemo(() => {
    if (!realTimeData?.activeSessions) {
      return []
    }

    return realTimeData.activeSessions.map((session, index) => ({
      id: session.id || `session-${index}`,
      userId: session.userId || 'Unknown User',
      templateKey: session.templateKey || 'unknown',
      startTime: session.startTime || new Date().toISOString(),
      status: session.status as any || 'active',
      requestCount: session.requestCount || 0,
      avgResponseTime: session.avgResponseTime || 0
    }))
  }, [realTimeData])

  // Real chart data from API
  const chartData = useMemo(() => {
    if (!realTimeData?.timeSeriesData) {
      return [
        { time: '10:00', requests: 0, responseTime: 0, errors: 0 }
      ]
    }

    return realTimeData.timeSeriesData.map(point => ({
      time: new Date(point.timestamp).toLocaleTimeString('en-US', { hour: '2-digit', minute: '2-digit' }),
      requests: point.requests || 0,
      responseTime: point.responseTime || 0,
      errors: point.errors || 0
    }))
  }, [realTimeData])

  // Real-time data refresh
  useEffect(() => {
    if (!autoRefresh || !isMonitoringActive) return

    const interval = setInterval(() => {
      refetchRealTime()
    }, 5000) // Refresh every 5 seconds

    return () => clearInterval(interval)
  }, [autoRefresh, isMonitoringActive, refetchRealTime])

  const getMetricStatus = (metric: RealTimeMetric) => {
    if (metric.value > metric.threshold) return 'critical'
    if (metric.value > metric.threshold * 0.8) return 'warning'
    return 'normal'
  }

  const getStatusColor = (status: string) => {
    switch (status) {
      case 'normal': return '#52c41a'
      case 'warning': return '#fa8c16'
      case 'critical': return '#f5222d'
      default: return '#d9d9d9'
    }
  }

  const getEventIcon = (type: string) => {
    switch (type) {
      case 'error': return <CloseCircleOutlined style={{ color: '#f5222d' }} />
      case 'warning': return <AlertOutlined style={{ color: '#fa8c16' }} />
      case 'success': return <CheckCircleOutlined style={{ color: '#52c41a' }} />
      default: return <ClockCircleOutlined style={{ color: '#1890ff' }} />
    }
  }

  const sessionColumns = [
    {
      title: 'User',
      dataIndex: 'userId',
      key: 'userId',
      render: (userId: string) => (
        <Space>
          <Avatar size="small" icon={<UserOutlined />} />
          <Text>{userId}</Text>
        </Space>
      )
    },
    {
      title: 'Template',
      dataIndex: 'templateKey',
      key: 'templateKey'
    },
    {
      title: 'Duration',
      key: 'duration',
      render: (record: ActiveSession) => {
        const duration = Math.floor((Date.now() - new Date(record.startTime).getTime()) / 60000)
        return `${duration}m`
      }
    },
    {
      title: 'Requests',
      dataIndex: 'requestCount',
      key: 'requestCount'
    },
    {
      title: 'Avg Response',
      dataIndex: 'avgResponseTime',
      key: 'avgResponseTime',
      render: (time: number) => `${time.toFixed(2)}s`
    },
    {
      title: 'Status',
      dataIndex: 'status',
      key: 'status',
      render: (status: string) => (
        <Badge 
          status={status === 'active' ? 'processing' : status === 'error' ? 'error' : 'default'} 
          text={status.toUpperCase()} 
        />
      )
    }
  ]

  return (
    <div>
      {/* Controls */}
      <Row gutter={16} style={{ marginBottom: '24px' }}>
        <Col span={12}>
          <Space>
            <Text strong>Monitoring:</Text>
            <Switch 
              checked={isMonitoringActive} 
              onChange={setIsMonitoringActive}
              checkedChildren={<PlayCircleOutlined />}
              unCheckedChildren={<PauseCircleOutlined />}
            />
            <Text strong>Auto Refresh:</Text>
            <Switch 
              checked={autoRefresh} 
              onChange={setAutoRefresh}
            />
          </Space>
        </Col>
        <Col span={12} style={{ textAlign: 'right' }}>
          <Space>
            <Select value={selectedTimeWindow} onChange={setSelectedTimeWindow} size="small">
              <Select.Option value="1m">1 Minute</Select.Option>
              <Select.Option value="5m">5 Minutes</Select.Option>
              <Select.Option value="15m">15 Minutes</Select.Option>
              <Select.Option value="1h">1 Hour</Select.Option>
            </Select>
            <Button icon={<SettingOutlined />} size="small">
              Configure Alerts
            </Button>
          </Space>
        </Col>
      </Row>

      {/* Status Banner */}
      {isMonitoringActive && (
        <Alert
          message={
            <Space>
              <Badge status="processing" />
              Real-time monitoring active
              <Text type="secondary">• Last update: {new Date().toLocaleTimeString()}</Text>
            </Space>
          }
          type="info"
          style={{ marginBottom: '24px' }}
        />
      )}

      {/* Real-time Metrics */}
      <Row gutter={16} style={{ marginBottom: '24px' }}>
        {metrics.map((metric, index) => (
          <Col span={4} key={index}>
            <Card size="small">
              <div style={{ textAlign: 'center' }}>
                <div style={{ 
                  fontSize: '20px', 
                  fontWeight: 'bold',
                  color: getStatusColor(getMetricStatus(metric))
                }}>
                  {typeof metric.value === 'number' && metric.value < 10 
                    ? metric.value.toFixed(2) 
                    : Math.round(metric.value)
                  }{metric.unit}
                </div>
                <div style={{ fontSize: '12px', color: '#666' }}>
                  {metric.name}
                </div>
                <Progress 
                  percent={(metric.value / metric.threshold) * 100} 
                  size="small" 
                  strokeColor={getStatusColor(getMetricStatus(metric))}
                  showInfo={false}
                />
              </div>
            </Card>
          </Col>
        ))}
      </Row>

      {/* Real-time Charts */}
      <Row gutter={16} style={{ marginBottom: '24px' }}>
        <Col span={16}>
          <Card title="Real-time Performance Metrics" loading={isRealTimeLoading}>
            <PerformanceLineChart
              data={chartData}
              xAxisKey="time"
              lines={[
                { key: 'requests', color: '#1890ff', name: 'Requests/Min' },
                { key: 'responseTime', color: '#52c41a', name: 'Response Time (s)' },
                { key: 'errors', color: '#f5222d', name: 'Errors' }
              ]}
              height={250}
            />
          </Card>
        </Col>
        <Col span={8}>
          <Card title="System Events" loading={isRealTimeLoading} style={{ height: '350px' }}>
            <div style={{ height: '280px', overflow: 'auto' }}>
              <Timeline size="small">
                {events.map((event) => (
                  <Timeline.Item
                    key={event.id}
                    dot={getEventIcon(event.type)}
                  >
                    <div>
                      <Text strong>{event.message}</Text>
                      <div style={{ fontSize: '12px', color: '#666' }}>
                        {event.source} • {new Date(event.timestamp).toLocaleTimeString()}
                      </div>
                      {event.details && (
                        <div style={{ fontSize: '12px', marginTop: '4px' }}>
                          {event.details}
                        </div>
                      )}
                    </div>
                  </Timeline.Item>
                ))}
              </Timeline>
            </div>
          </Card>
        </Col>
      </Row>

      {/* Active Sessions */}
      <Card
        title={
          <Space>
            <UserOutlined />
            Active User Sessions
            <Badge count={activeSessions.filter(s => s.status === 'active').length} />
          </Space>
        }
        loading={isRealTimeLoading}
        extra={
          <Button size="small" icon={<EyeOutlined />}>
            View All Sessions
          </Button>
        }
      >
        <Table
          columns={sessionColumns}
          dataSource={activeSessions}
          pagination={false}
          size="small"
        />
      </Card>
    </div>
  )
}

export default RealTimeMonitoring
