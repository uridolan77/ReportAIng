import React, { useState, useEffect, useRef } from 'react'
import { 
  Card, 
  Space, 
  Typography, 
  Progress, 
  Row, 
  Col, 
  Statistic,
  Alert,
  Timeline,
  Tag,
  Button,
  Switch,
  Badge
} from 'antd'
import {
  PlayCircleOutlined,
  PauseCircleOutlined,
  StopOutlined,
  EyeOutlined,
  ClockCircleOutlined,
  BarChartOutlined,
  ThunderboltOutlined,
  CheckCircleOutlined,
  ExclamationCircleOutlined,
  WifiOutlined,
  DisconnectOutlined
} from '@ant-design/icons'
import { ConfidenceIndicator } from '../common/ConfidenceIndicator'

const { Title, Text } = Typography

export interface RealTimeMonitorProps {
  isActive?: boolean
  autoStart?: boolean
  updateInterval?: number
  maxHistoryItems?: number
  onStatusChange?: (status: 'connected' | 'disconnected' | 'error' | 'active') => void
  onDataReceived?: (data: RealTimeData) => void
  data?: {
    activeQueries: number
    totalQueries: number
    averageConfidence: number
    averageProcessingTime: number
    errorRate: number
    lastUpdate: string
  }
  className?: string
  testId?: string
}

export interface RealTimeData {
  timestamp: string
  type: 'query_start' | 'step_complete' | 'query_complete' | 'error'
  queryId: string
  stepName?: string
  confidence?: number
  processingTime?: number
  tokens?: number
  error?: string
  metadata?: any
}

interface MonitorState {
  status: 'connected' | 'disconnected' | 'error'
  activeQueries: number
  totalQueries: number
  averageConfidence: number
  averageProcessingTime: number
  errorRate: number
  lastUpdate: string
}

/**
 * RealTimeMonitor - Real-time monitoring of AI transparency events
 * 
 * Features:
 * - Live connection status monitoring
 * - Real-time query tracking
 * - Performance metrics updates
 * - Error detection and alerts
 * - Historical data visualization
 * - Connection management
 */
export const RealTimeMonitor: React.FC<RealTimeMonitorProps> = ({
  isActive = false,
  autoStart = false,
  updateInterval = 1000,
  maxHistoryItems = 50,
  onStatusChange,
  onDataReceived,
  data,
  className,
  testId = 'real-time-monitor'
}) => {
  const [monitoring, setMonitoring] = useState(autoStart)
  const [state, setState] = useState<MonitorState>({
    status: 'disconnected',
    activeQueries: 0,
    totalQueries: 0,
    averageConfidence: 0,
    averageProcessingTime: 0,
    errorRate: 0,
    lastUpdate: ''
  })

  // Update state when real data is provided
  useEffect(() => {
    if (data) {
      setState(prev => ({
        ...prev,
        status: 'connected',
        activeQueries: data.activeQueries,
        totalQueries: data.totalQueries,
        averageConfidence: data.averageConfidence,
        averageProcessingTime: data.averageProcessingTime,
        errorRate: data.errorRate,
        lastUpdate: data.lastUpdate
      }))
    }
  }, [data])
  const [recentEvents, setRecentEvents] = useState<RealTimeData[]>([])
  const [connectionAttempts, setConnectionAttempts] = useState(0)
  
  const wsRef = useRef<WebSocket | null>(null)
  const reconnectTimeoutRef = useRef<NodeJS.Timeout | null>(null)

  // WebSocket connection management - only if no real data is provided
  useEffect(() => {
    if (monitoring && isActive && !data) {
      connectWebSocket()
    } else {
      disconnectWebSocket()
    }

    return () => {
      disconnectWebSocket()
    }
  }, [monitoring, isActive, data])

  const connectWebSocket = () => {
    try {
      // In a real implementation, this would connect to your WebSocket endpoint
      // For demo purposes, we'll simulate the connection
      wsRef.current = new WebSocket('ws://localhost:55244/transparency-hub')
      
      wsRef.current.onopen = () => {
        setState(prev => ({ ...prev, status: 'connected', lastUpdate: new Date().toISOString() }))
        setConnectionAttempts(0)
        onStatusChange?.('connected')
        
        // Start simulating data for demo
        startDataSimulation()
      }

      wsRef.current.onmessage = (event) => {
        try {
          const data: RealTimeData = JSON.parse(event.data)
          handleRealTimeData(data)
        } catch (error) {
          console.error('Failed to parse WebSocket message:', error)
        }
      }

      wsRef.current.onclose = () => {
        setState(prev => ({ ...prev, status: 'disconnected' }))
        onStatusChange?.('disconnected')
        
        // Attempt to reconnect
        if (monitoring && connectionAttempts < 5) {
          reconnectTimeoutRef.current = setTimeout(() => {
            setConnectionAttempts(prev => prev + 1)
            connectWebSocket()
          }, Math.pow(2, connectionAttempts) * 1000) // Exponential backoff
        }
      }

      wsRef.current.onerror = () => {
        setState(prev => ({ ...prev, status: 'error' }))
        onStatusChange?.('error')
      }

    } catch (error) {
      setState(prev => ({ ...prev, status: 'error' }))
      onStatusChange?.('error')
    }
  }

  const disconnectWebSocket = () => {
    if (wsRef.current) {
      wsRef.current.close()
      wsRef.current = null
    }
    
    if (reconnectTimeoutRef.current) {
      clearTimeout(reconnectTimeoutRef.current)
      reconnectTimeoutRef.current = null
    }
  }

  // Simulate real-time data for demo purposes
  const startDataSimulation = () => {
    const interval = setInterval(() => {
      if (!monitoring || state.status !== 'connected') {
        clearInterval(interval)
        return
      }

      // Simulate various types of events
      const eventTypes: RealTimeData['type'][] = ['query_start', 'step_complete', 'query_complete']
      const randomType = eventTypes[Math.floor(Math.random() * eventTypes.length)]
      
      const simulatedData: RealTimeData = {
        timestamp: new Date().toISOString(),
        type: randomType,
        queryId: `query-${Date.now()}`,
        stepName: randomType === 'step_complete' ? `Step ${Math.floor(Math.random() * 5) + 1}` : undefined,
        confidence: Math.random() * 0.4 + 0.6, // 0.6 to 1.0
        processingTime: Math.random() * 2000 + 500, // 500ms to 2.5s
        tokens: Math.floor(Math.random() * 500) + 100,
        metadata: { source: 'simulation' }
      }

      handleRealTimeData(simulatedData)
    }, updateInterval)

    return () => clearInterval(interval)
  }

  const handleRealTimeData = (data: RealTimeData) => {
    // Update recent events
    setRecentEvents(prev => {
      const updated = [data, ...prev].slice(0, maxHistoryItems)
      return updated
    })

    // Update state metrics
    setState(prev => {
      const newState = { ...prev, lastUpdate: data.timestamp }

      switch (data.type) {
        case 'query_start':
          newState.activeQueries += 1
          newState.totalQueries += 1
          break
        case 'query_complete':
          newState.activeQueries = Math.max(0, newState.activeQueries - 1)
          if (data.confidence) {
            newState.averageConfidence = (newState.averageConfidence + data.confidence) / 2
          }
          if (data.processingTime) {
            newState.averageProcessingTime = (newState.averageProcessingTime + data.processingTime) / 2
          }
          break
        case 'error':
          newState.errorRate = (newState.errorRate + 0.1) / 2 // Increase error rate
          break
      }

      return newState
    })

    onDataReceived?.(data)
  }

  const handleToggleMonitoring = () => {
    setMonitoring(!monitoring)
  }

  const handleClearHistory = () => {
    setRecentEvents([])
  }

  const getStatusColor = (status: string) => {
    switch (status) {
      case 'connected': return '#52c41a'
      case 'disconnected': return '#d9d9d9'
      case 'error': return '#ff4d4f'
      default: return '#d9d9d9'
    }
  }

  const getStatusIcon = (status: string) => {
    switch (status) {
      case 'connected': return <WifiOutlined />
      case 'disconnected': return <DisconnectOutlined />
      case 'error': return <ExclamationCircleOutlined />
      default: return <DisconnectOutlined />
    }
  }

  const getEventIcon = (type: string) => {
    switch (type) {
      case 'query_start': return <PlayCircleOutlined style={{ color: '#1890ff' }} />
      case 'step_complete': return <ThunderboltOutlined style={{ color: '#faad14' }} />
      case 'query_complete': return <CheckCircleOutlined style={{ color: '#52c41a' }} />
      case 'error': return <ExclamationCircleOutlined style={{ color: '#ff4d4f' }} />
      default: return <EyeOutlined />
    }
  }

  return (
    <Card 
      title={
        <Space>
          <EyeOutlined />
          <span>Real-Time Monitor</span>
          <Badge 
            status={state.status === 'connected' ? 'processing' : 'default'} 
            text={state.status.toUpperCase()}
          />
        </Space>
      }
      extra={
        <Space>
          <Switch
            checked={monitoring}
            onChange={handleToggleMonitoring}
            checkedChildren="ON"
            unCheckedChildren="OFF"
          />
          <Button 
            size="small" 
            onClick={handleClearHistory}
            disabled={recentEvents.length === 0}
          >
            Clear
          </Button>
        </Space>
      }
      className={className}
      data-testid={testId}
    >
      <Space direction="vertical" style={{ width: '100%' }} size="large">
        {/* Connection Status */}
        <Alert
          message={
            <Space>
              {getStatusIcon(state.status)}
              <span>Connection Status: {state.status.toUpperCase()}</span>
              {state.lastUpdate && (
                <Text type="secondary" style={{ fontSize: '12px' }}>
                  Last update: {new Date(state.lastUpdate).toLocaleTimeString()}
                </Text>
              )}
            </Space>
          }
          type={state.status === 'connected' ? 'success' : state.status === 'error' ? 'error' : 'info'}
          showIcon={false}
        />

        {/* Real-time Metrics */}
        <Row gutter={[16, 16]}>
          <Col xs={24} sm={12} lg={6}>
            <Statistic
              title="Active Queries"
              value={state.activeQueries}
              prefix={<PlayCircleOutlined />}
              valueStyle={{ color: state.activeQueries > 0 ? '#1890ff' : '#d9d9d9' }}
            />
          </Col>
          <Col xs={24} sm={12} lg={6}>
            <Statistic
              title="Total Queries"
              value={state.totalQueries}
              prefix={<BarChartOutlined />}
            />
          </Col>
          <Col xs={24} sm={12} lg={6}>
            <Statistic
              title="Avg Confidence"
              value={(state.averageConfidence * 100).toFixed(1)}
              suffix="%"
              prefix={<CheckCircleOutlined />}
              valueStyle={{ 
                color: state.averageConfidence > 0.8 ? '#52c41a' : 
                       state.averageConfidence > 0.6 ? '#faad14' : '#ff4d4f' 
              }}
            />
          </Col>
          <Col xs={24} sm={12} lg={6}>
            <Statistic
              title="Avg Processing Time"
              value={state.averageProcessingTime.toFixed(0)}
              suffix="ms"
              prefix={<ClockCircleOutlined />}
            />
          </Col>
        </Row>

        {/* Error Rate */}
        {state.errorRate > 0 && (
          <Alert
            message={`Error Rate: ${(state.errorRate * 100).toFixed(1)}%`}
            description="Elevated error rate detected in real-time monitoring"
            type="warning"
            showIcon
          />
        )}

        {/* Recent Events Timeline */}
        <Card title={`Recent Events (${recentEvents.length})`} size="small">
          {recentEvents.length > 0 ? (
            <Timeline style={{ maxHeight: 300, overflow: 'auto' }}>
              {recentEvents.slice(0, 10).map((event, index) => (
                <Timeline.Item
                  key={`${event.queryId}-${index}`}
                  dot={getEventIcon(event.type)}
                >
                  <Space direction="vertical" size="small">
                    <Space>
                      <Text strong>{event.type.replace('_', ' ').toUpperCase()}</Text>
                      <Tag size="small">{event.queryId.substring(0, 8)}...</Tag>
                      <Text type="secondary" style={{ fontSize: '12px' }}>
                        {new Date(event.timestamp).toLocaleTimeString()}
                      </Text>
                    </Space>
                    
                    {event.stepName && (
                      <Text type="secondary" style={{ fontSize: '12px' }}>
                        Step: {event.stepName}
                      </Text>
                    )}
                    
                    <Space size="large">
                      {event.confidence && (
                        <Space size="small">
                          <Text style={{ fontSize: '12px' }}>Confidence:</Text>
                          <ConfidenceIndicator
                            confidence={event.confidence}
                            size="small"
                            type="badge"
                          />
                        </Space>
                      )}
                      {event.processingTime && (
                        <Space size="small">
                          <ClockCircleOutlined style={{ fontSize: '12px' }} />
                          <Text style={{ fontSize: '12px' }}>{event.processingTime.toFixed(0)}ms</Text>
                        </Space>
                      )}
                      {event.tokens && (
                        <Space size="small">
                          <BarChartOutlined style={{ fontSize: '12px' }} />
                          <Text style={{ fontSize: '12px' }}>{event.tokens} tokens</Text>
                        </Space>
                      )}
                    </Space>
                    
                    {event.error && (
                      <Alert
                        message={event.error}
                        type="error"
                        size="small"
                        showIcon
                      />
                    )}
                  </Space>
                </Timeline.Item>
              ))}
            </Timeline>
          ) : (
            <div style={{ textAlign: 'center', padding: '20px 0' }}>
              <Space direction="vertical">
                <EyeOutlined style={{ fontSize: '24px', color: '#d9d9d9' }} />
                <Text type="secondary">No recent events</Text>
              </Space>
            </div>
          )}
        </Card>

        {/* Connection Attempts */}
        {connectionAttempts > 0 && (
          <Alert
            message={`Reconnection attempt ${connectionAttempts}/5`}
            description="Attempting to reconnect to real-time monitoring service"
            type="info"
            showIcon
          />
        )}
      </Space>
    </Card>
  )
}

export default RealTimeMonitor
