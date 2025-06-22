import React, { useState, useEffect } from 'react'
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

  // Real-time metrics state
  const [metrics, setMetrics] = useState<RealTimeMetric[]>([
    { name: 'Active Users', value: 47, unit: '', status: 'normal', trend: 'up', threshold: 100 },
    { name: 'Requests/Min', value: 156, unit: '/min', status: 'normal', trend: 'stable', threshold: 200 },
    { name: 'Avg Response Time', value: 0.85, unit: 's', status: 'normal', trend: 'down', threshold: 2.0 },
    { name: 'Error Rate', value: 2.3, unit: '%', status: 'normal', trend: 'down', threshold: 5.0 },
    { name: 'CPU Usage', value: 67, unit: '%', status: 'warning', trend: 'up', threshold: 80 },
    { name: 'Memory Usage', value: 78, unit: '%', status: 'warning', trend: 'up', threshold: 85 }
  ])

  const [events, setEvents] = useState<SystemEvent[]>([
    {
      id: '1',
      timestamp: new Date().toISOString(),
      type: 'warning',
      source: 'SQL Template',
      message: 'High response time detected',
      details: 'Average response time exceeded 2s threshold'
    },
    {
      id: '2',
      timestamp: new Date(Date.now() - 60000).toISOString(),
      type: 'info',
      source: 'System',
      message: 'Auto-scaling triggered',
      details: 'Additional processing capacity added'
    },
    {
      id: '3',
      timestamp: new Date(Date.now() - 120000).toISOString(),
      type: 'success',
      source: 'Optimization',
      message: 'Template optimization completed',
      details: 'Insight generation template improved by 12%'
    }
  ])

  const [activeSessions] = useState<ActiveSession[]>([
    {
      id: 'session_1',
      userId: 'john.doe@company.com',
      templateKey: 'sql_generation',
      startTime: new Date(Date.now() - 300000).toISOString(),
      status: 'active',
      requestCount: 15,
      avgResponseTime: 1.2
    },
    {
      id: 'session_2',
      userId: 'jane.smith@company.com',
      templateKey: 'insight_generation',
      startTime: new Date(Date.now() - 180000).toISOString(),
      status: 'active',
      requestCount: 8,
      avgResponseTime: 0.9
    },
    {
      id: 'session_3',
      userId: 'mike.wilson@company.com',
      templateKey: 'explanation',
      startTime: new Date(Date.now() - 600000).toISOString(),
      status: 'idle',
      requestCount: 23,
      avgResponseTime: 1.5
    }
  ])

  // Mock real-time data for charts
  const [chartData, setChartData] = useState([
    { time: '10:00', requests: 120, responseTime: 0.8, errors: 2 },
    { time: '10:01', requests: 135, responseTime: 0.9, errors: 1 },
    { time: '10:02', requests: 142, responseTime: 0.7, errors: 3 },
    { time: '10:03', requests: 156, responseTime: 0.85, errors: 2 },
    { time: '10:04', requests: 148, responseTime: 0.9, errors: 1 }
  ])

  // Simulate real-time updates
  useEffect(() => {
    if (!autoRefresh || !isMonitoringActive) return

    const interval = setInterval(() => {
      // Update metrics
      setMetrics(prev => prev.map(metric => ({
        ...metric,
        value: Math.max(0, metric.value + (Math.random() - 0.5) * (metric.value * 0.1))
      })))

      // Update chart data
      setChartData(prev => {
        const newData = [...prev.slice(1)]
        const lastTime = prev[prev.length - 1].time
        const [hours, minutes] = lastTime.split(':').map(Number)
        const newMinutes = minutes + 1
        const newTime = `${hours}:${newMinutes.toString().padStart(2, '0')}`
        
        newData.push({
          time: newTime,
          requests: Math.floor(Math.random() * 50) + 120,
          responseTime: Math.random() * 0.5 + 0.5,
          errors: Math.floor(Math.random() * 5)
        })
        
        return newData
      })

      // Occasionally add new events
      if (Math.random() < 0.1) {
        const newEvent: SystemEvent = {
          id: Date.now().toString(),
          timestamp: new Date().toISOString(),
          type: ['info', 'warning', 'success'][Math.floor(Math.random() * 3)] as any,
          source: ['SQL Template', 'System', 'Optimization'][Math.floor(Math.random() * 3)],
          message: 'Real-time event detected',
          details: 'Simulated real-time monitoring event'
        }
        setEvents(prev => [newEvent, ...prev.slice(0, 9)])
      }
    }, 2000)

    return () => clearInterval(interval)
  }, [autoRefresh, isMonitoringActive])

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
          <Card title="Real-time Performance Metrics">
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
          <Card title="System Events" style={{ height: '350px' }}>
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
