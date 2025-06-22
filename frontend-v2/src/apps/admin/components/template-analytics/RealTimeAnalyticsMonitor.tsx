import React, { useState, useEffect } from 'react'
import { 
  Card, 
  Row, 
  Col, 
  Badge, 
  Button, 
  Space, 
  Typography,
  Statistic,
  List,
  Tag,
  Alert,
  Tooltip,
  Switch,
  Progress,
  notification
} from 'antd'
import { 
  ThunderboltOutlined,
  UserOutlined,
  ClockCircleOutlined,
  ExclamationCircleOutlined,
  CheckCircleOutlined,
  DisconnectOutlined,
  ReloadOutlined,
  SettingOutlined,
  BellOutlined
} from '@ant-design/icons'
import dayjs from 'dayjs'
import { useTemplateAnalyticsHub } from '@shared/hooks/useTemplateAnalyticsHub'
import type {
  RealTimeAnalyticsData,
  PerformanceAlert,
  TemplatePerformanceMetrics,
  ABTestDetails,
  ComprehensiveAnalyticsDashboard
} from '@shared/types/templateAnalytics'

const { Title, Text } = Typography

interface RealTimeAnalyticsMonitorProps {
  intentType?: string
  showAlerts?: boolean
  showPerformanceUpdates?: boolean
  showABTestUpdates?: boolean
  autoRefreshInterval?: number
}

export const RealTimeAnalyticsMonitor: React.FC<RealTimeAnalyticsMonitorProps> = ({
  intentType,
  showAlerts = true,
  showPerformanceUpdates = true,
  showABTestUpdates = true,
  autoRefreshInterval = 30000
}) => {
  // State
  const [realTimeData, setRealTimeData] = useState<RealTimeAnalyticsData | null>(null)
  const [recentAlerts, setRecentAlerts] = useState<PerformanceAlert[]>([])
  const [recentPerformanceUpdates, setRecentPerformanceUpdates] = useState<Array<{
    templateKey: string
    data: TemplatePerformanceMetrics
    timestamp: Date
  }>>([])
  const [recentABTestUpdates, setRecentABTestUpdates] = useState<Array<{
    testId: number
    data: ABTestDetails
    timestamp: Date
  }>>([])
  const [isMonitoring, setIsMonitoring] = useState(true)
  const [notificationsEnabled, setNotificationsEnabled] = useState(true)

  // SignalR Hub
  const {
    isConnected,
    connectionState,
    connect,
    disconnect,
    subscribeToPerformanceUpdates,
    subscribeToABTestUpdates,
    subscribeToAlerts,
    getRealTimeDashboard,
    lastUpdate,
    error
  } = useTemplateAnalyticsHub({
    autoConnect: true,
    subscribeToPerformanceUpdates: showPerformanceUpdates,
    subscribeToABTestUpdates: showABTestUpdates,
    subscribeToAlerts: showAlerts,
    intentType,
    onRealTimeUpdate: (data) => {
      setRealTimeData(data)
    },
    onNewAlert: (alert) => {
      setRecentAlerts(prev => [alert, ...prev.slice(0, 9)]) // Keep last 10 alerts
      
      if (notificationsEnabled) {
        notification.warning({
          message: 'Performance Alert',
          description: alert.message,
          placement: 'topRight',
          duration: 5,
          icon: <ExclamationCircleOutlined style={{ color: '#faad14' }} />
        })
      }
    },
    onPerformanceUpdate: (templateKey, data) => {
      setRecentPerformanceUpdates(prev => [
        { templateKey, data, timestamp: new Date() },
        ...prev.slice(0, 9)
      ])
      
      if (notificationsEnabled && data.successRate < 0.8) {
        notification.info({
          message: 'Performance Update',
          description: `Template ${data.templateName} performance updated`,
          placement: 'topRight',
          duration: 3
        })
      }
    },
    onABTestUpdate: (testId, data) => {
      setRecentABTestUpdates(prev => [
        { testId, data, timestamp: new Date() },
        ...prev.slice(0, 9)
      ])
      
      if (notificationsEnabled) {
        notification.info({
          message: 'A/B Test Update',
          description: `Test ${data.testName} status changed to ${data.status}`,
          placement: 'topRight',
          duration: 3
        })
      }
    },
    onError: (errorMessage) => {
      notification.error({
        message: 'Real-time Connection Error',
        description: errorMessage,
        placement: 'topRight',
        duration: 5
      })
    }
  })

  // Auto-refresh real-time data
  useEffect(() => {
    if (isConnected && isMonitoring) {
      const interval = setInterval(async () => {
        try {
          const data = await getRealTimeDashboard()
          setRealTimeData(data)
        } catch (error) {
          console.error('Failed to refresh real-time data:', error)
        }
      }, autoRefreshInterval)

      return () => clearInterval(interval)
    }
  }, [isConnected, isMonitoring, autoRefreshInterval, getRealTimeDashboard])

  // Initial data load
  useEffect(() => {
    if (isConnected) {
      getRealTimeDashboard()
        .then(setRealTimeData)
        .catch(console.error)
    }
  }, [isConnected, getRealTimeDashboard])

  const handleToggleMonitoring = () => {
    setIsMonitoring(!isMonitoring)
    if (!isMonitoring && !isConnected) {
      connect()
    } else if (isMonitoring && isConnected) {
      disconnect()
    }
  }

  const handleRefresh = async () => {
    try {
      const data = await getRealTimeDashboard()
      setRealTimeData(data)
    } catch (error) {
      console.error('Failed to refresh data:', error)
    }
  }

  const getConnectionStatusColor = () => {
    switch (connectionState) {
      case 'Connected': return 'success'
      case 'Connecting': return 'processing'
      case 'Reconnecting': return 'warning'
      default: return 'error'
    }
  }

  const getMetricColor = (value: number, threshold: number, inverse = false) => {
    const isGood = inverse ? value < threshold : value > threshold
    return isGood ? '#52c41a' : '#ff4d4f'
  }

  return (
    <div className="real-time-analytics-monitor">
      {/* Header */}
      <Card style={{ marginBottom: '16px' }}>
        <Row gutter={16} align="middle">
          <Col span={12}>
            <Space>
              <ThunderboltOutlined style={{ fontSize: '20px', color: '#1890ff' }} />
              <Title level={4} style={{ margin: 0 }}>Real-Time Analytics Monitor</Title>
              <Badge 
                status={getConnectionStatusColor()} 
                text={connectionState}
              />
            </Space>
          </Col>
          <Col span={12} style={{ textAlign: 'right' }}>
            <Space>
              <Text type="secondary">Notifications:</Text>
              <Switch
                checked={notificationsEnabled}
                onChange={setNotificationsEnabled}
                size="small"
              />
              <Text type="secondary">Monitoring:</Text>
              <Switch
                checked={isMonitoring}
                onChange={handleToggleMonitoring}
                size="small"
              />
              <Button 
                icon={<ReloadOutlined />} 
                onClick={handleRefresh}
                size="small"
              >
                Refresh
              </Button>
            </Space>
          </Col>
        </Row>
        
        {lastUpdate && (
          <div style={{ marginTop: '8px' }}>
            <Text type="secondary" style={{ fontSize: '12px' }}>
              Last updated: {dayjs(lastUpdate).format('HH:mm:ss')}
            </Text>
          </div>
        )}
      </Card>

      {/* Connection Error */}
      {error && (
        <Alert
          message="Connection Error"
          description={error}
          type="error"
          showIcon
          closable
          style={{ marginBottom: '16px' }}
          action={
            <Button size="small" onClick={connect}>
              Reconnect
            </Button>
          }
        />
      )}

      {/* Real-Time Metrics */}
      {realTimeData && (
        <Card title="Live Metrics" style={{ marginBottom: '16px' }}>
          <Row gutter={16}>
            <Col span={4}>
              <Statistic
                title="Active Users"
                value={realTimeData.activeUsers}
                prefix={<UserOutlined />}
                valueStyle={{ color: '#1890ff' }}
              />
            </Col>
            <Col span={4}>
              <Statistic
                title="Queries/Min"
                value={realTimeData.queriesPerMinute}
                valueStyle={{ color: '#52c41a' }}
              />
            </Col>
            <Col span={4}>
              <Statistic
                title="Success Rate"
                value={realTimeData.currentSuccessRate * 100}
                precision={1}
                suffix="%"
                valueStyle={{ 
                  color: getMetricColor(realTimeData.currentSuccessRate, 0.9) 
                }}
              />
            </Col>
            <Col span={4}>
              <Statistic
                title="Avg Response"
                value={realTimeData.averageResponseTime}
                precision={2}
                suffix="s"
                valueStyle={{ 
                  color: getMetricColor(realTimeData.averageResponseTime, 2, true) 
                }}
              />
            </Col>
            <Col span={4}>
              <Statistic
                title="Errors/Hour"
                value={realTimeData.errorsInLastHour}
                valueStyle={{ 
                  color: getMetricColor(realTimeData.errorsInLastHour, 10, true) 
                }}
              />
            </Col>
            <Col span={4}>
              <Statistic
                title="Activities"
                value={realTimeData.recentActivities.length}
                valueStyle={{ color: '#722ed1' }}
              />
            </Col>
          </Row>
        </Card>
      )}

      {/* Recent Updates */}
      <Row gutter={16}>
        {/* Recent Alerts */}
        {showAlerts && (
          <Col span={8}>
            <Card 
              title={
                <Space>
                  <BellOutlined />
                  Recent Alerts
                  {recentAlerts.length > 0 && <Badge count={recentAlerts.length} />}
                </Space>
              }
              size="small"
              style={{ height: '300px' }}
            >
              <List
                size="small"
                dataSource={recentAlerts}
                renderItem={(alert) => (
                  <List.Item>
                    <List.Item.Meta
                      avatar={
                        <ExclamationCircleOutlined 
                          style={{ 
                            color: alert.severity === 'critical' ? '#ff4d4f' : 
                                   alert.severity === 'high' ? '#fa8c16' : '#1890ff' 
                          }} 
                        />
                      }
                      title={
                        <div style={{ fontSize: '12px' }}>
                          <Text strong>{alert.templateKey}</Text>
                          <Tag 
                            color={alert.severity === 'critical' ? 'red' : 
                                   alert.severity === 'high' ? 'orange' : 'blue'}
                            size="small"
                            style={{ marginLeft: '8px' }}
                          >
                            {alert.severity}
                          </Tag>
                        </div>
                      }
                      description={
                        <div style={{ fontSize: '11px' }}>
                          <div>{alert.message}</div>
                          <Text type="secondary">
                            {dayjs(alert.timestamp).format('HH:mm:ss')}
                          </Text>
                        </div>
                      }
                    />
                  </List.Item>
                )}
              />
            </Card>
          </Col>
        )}

        {/* Recent Performance Updates */}
        {showPerformanceUpdates && (
          <Col span={8}>
            <Card 
              title={
                <Space>
                  <CheckCircleOutlined />
                  Performance Updates
                  {recentPerformanceUpdates.length > 0 && <Badge count={recentPerformanceUpdates.length} />}
                </Space>
              }
              size="small"
              style={{ height: '300px' }}
            >
              <List
                size="small"
                dataSource={recentPerformanceUpdates}
                renderItem={(update) => (
                  <List.Item>
                    <List.Item.Meta
                      title={
                        <div style={{ fontSize: '12px' }}>
                          <Text strong>{update.data.templateName}</Text>
                        </div>
                      }
                      description={
                        <div style={{ fontSize: '11px' }}>
                          <div>
                            Success Rate: {(update.data.successRate * 100).toFixed(1)}%
                          </div>
                          <div>
                            Usage: {update.data.totalUsages}
                          </div>
                          <Text type="secondary">
                            {dayjs(update.timestamp).format('HH:mm:ss')}
                          </Text>
                        </div>
                      }
                    />
                  </List.Item>
                )}
              />
            </Card>
          </Col>
        )}

        {/* Recent A/B Test Updates */}
        {showABTestUpdates && (
          <Col span={8}>
            <Card 
              title={
                <Space>
                  <SettingOutlined />
                  A/B Test Updates
                  {recentABTestUpdates.length > 0 && <Badge count={recentABTestUpdates.length} />}
                </Space>
              }
              size="small"
              style={{ height: '300px' }}
            >
              <List
                size="small"
                dataSource={recentABTestUpdates}
                renderItem={(update) => (
                  <List.Item>
                    <List.Item.Meta
                      title={
                        <div style={{ fontSize: '12px' }}>
                          <Text strong>{update.data.testName}</Text>
                          <Tag 
                            color={update.data.status === 'running' ? 'green' : 
                                   update.data.status === 'completed' ? 'blue' : 'orange'}
                            size="small"
                            style={{ marginLeft: '8px' }}
                          >
                            {update.data.status}
                          </Tag>
                        </div>
                      }
                      description={
                        <div style={{ fontSize: '11px' }}>
                          <div>
                            Original: {(update.data.originalSuccessRate * 100).toFixed(1)}%
                          </div>
                          <div>
                            Variant: {(update.data.variantSuccessRate * 100).toFixed(1)}%
                          </div>
                          <Text type="secondary">
                            {dayjs(update.timestamp).format('HH:mm:ss')}
                          </Text>
                        </div>
                      }
                    />
                  </List.Item>
                )}
              />
            </Card>
          </Col>
        )}
      </Row>
    </div>
  )
}

export default RealTimeAnalyticsMonitor
