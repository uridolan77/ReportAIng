/**
 * System Status Indicator Component
 *
 * Shows the real-time status of API, Database, and AI connections.
 * This component provides a hierarchical view of system health.
 */

import React from 'react'
import { Space, Typography, Tooltip, Badge } from 'antd'
import {
  CloudOutlined,
  DatabaseOutlined,
  RobotOutlined,
  CheckCircleOutlined,
  CloseCircleOutlined,
  LoadingOutlined,
  ExclamationCircleOutlined
} from '@ant-design/icons'

const { Text } = Typography

interface SystemStatus {
  api: {
    status: 'online' | 'offline' | 'checking'
    responseTime?: number
    lastChecked: string
  }
  database: {
    status: 'online' | 'offline' | 'checking'
    connectionPool?: number
    lastChecked: string
  }
  ai: {
    status: 'online' | 'offline' | 'checking'
    provider?: string
    responseTime?: number
    lastChecked: string
  }
}

interface SystemStatusIndicatorProps {
  collapsed?: boolean
}

export const SystemStatusIndicator: React.FC<SystemStatusIndicatorProps> = ({ collapsed = false }) => {
  const [systemStatus, setSystemStatus] = React.useState<SystemStatus>({
    api: { status: 'checking', lastChecked: '' },
    database: { status: 'checking', lastChecked: '' },
    ai: { status: 'checking', lastChecked: '' }
  })
  const [loading, setLoading] = React.useState(false)

  const checkSystemHealth = async () => {
    setLoading(true)
    try {
      // Check API health - use absolute URL to backend
      const apiResponse = await fetch('http://localhost:55244/api/system/health', {
        method: 'GET',
        timeout: 3000
      } as any)

      const apiOnline = apiResponse.ok
      const now = new Date().toISOString()

      if (apiOnline) {
        // If API is online, check database and AI
        try {
          const healthData = await apiResponse.json()
          setSystemStatus({
            api: {
              status: 'online',
              responseTime: 150, // Mock response time
              lastChecked: now
            },
            database: {
              status: healthData.database?.connected ? 'online' : 'offline',
              connectionPool: healthData.database?.connectionPool || 5,
              lastChecked: now
            },
            ai: {
              status: healthData.ai?.available ? 'online' : 'offline',
              provider: healthData.ai?.provider || 'openai',
              responseTime: healthData.ai?.responseTime || 200,
              lastChecked: now
            }
          })
        } catch {
          // API is online but can't get detailed status
          setSystemStatus({
            api: { status: 'online', responseTime: 150, lastChecked: now },
            database: { status: 'offline', lastChecked: now },
            ai: { status: 'offline', lastChecked: now }
          })
        }
      } else {
        // API is offline, so everything else is offline too
        setSystemStatus({
          api: { status: 'offline', lastChecked: now },
          database: { status: 'offline', lastChecked: now },
          ai: { status: 'offline', lastChecked: now }
        })
      }
    } catch {
      // Complete system failure
      const now = new Date().toISOString()
      setSystemStatus({
        api: { status: 'offline', lastChecked: now },
        database: { status: 'offline', lastChecked: now },
        ai: { status: 'offline', lastChecked: now }
      })
    } finally {
      setLoading(false)
    }
  }

  // Check system health on component mount and every 30 seconds
  React.useEffect(() => {
    checkSystemHealth()
    const interval = setInterval(checkSystemHealth, 30000)
    return () => clearInterval(interval)
  }, [])

  const getStatusIcon = (status: 'online' | 'offline' | 'checking') => {
    if (loading || status === 'checking') return <LoadingOutlined spin />
    return status === 'online'
      ? <CheckCircleOutlined style={{ color: '#52c41a' }} />
      : <CloseCircleOutlined style={{ color: '#ff4d4f' }} />
  }

  const getStatusColor = (status: 'online' | 'offline' | 'checking') => {
    if (loading || status === 'checking') return '#faad14'
    return status === 'online' ? '#52c41a' : '#ff4d4f'
  }

  const getOverallStatus = () => {
    const { api, database, ai } = systemStatus
    if (api.status === 'offline') return { status: 'offline', text: 'System Offline' }
    if (database.status === 'offline') return { status: 'degraded', text: 'Database Issues' }
    if (ai.status === 'offline') return { status: 'degraded', text: 'AI Unavailable' }
    if (api.status === 'online' && database.status === 'online' && ai.status === 'online') {
      return { status: 'online', text: 'All Systems Online' }
    }
    return { status: 'checking', text: 'Checking...' }
  }

  const overall = getOverallStatus()

  const getTooltipContent = () => {
    const formatTime = (timestamp: string) => {
      return timestamp ? new Date(timestamp).toLocaleTimeString() : 'Never'
    }

    return (
      <div style={{ maxWidth: 300 }}>
        <div style={{ marginBottom: 12 }}>
          <strong>System Status Overview</strong>
        </div>

        <div style={{ marginBottom: 8 }}>
          <div style={{ display: 'flex', alignItems: 'center', gap: 8, marginBottom: 4 }}>
            <CloudOutlined />
            <strong>API Server</strong>
            {getStatusIcon(systemStatus.api.status)}
          </div>
          <div style={{ fontSize: '12px', marginLeft: 24, opacity: 0.8 }}>
            Status: {systemStatus.api.status}
            {systemStatus.api.responseTime && ` • ${systemStatus.api.responseTime}ms`}
            <br />Last checked: {formatTime(systemStatus.api.lastChecked)}
          </div>
        </div>

        <div style={{ marginBottom: 8 }}>
          <div style={{ display: 'flex', alignItems: 'center', gap: 8, marginBottom: 4 }}>
            <DatabaseOutlined />
            <strong>Database</strong>
            {getStatusIcon(systemStatus.database.status)}
          </div>
          <div style={{ fontSize: '12px', marginLeft: 24, opacity: 0.8 }}>
            Status: {systemStatus.database.status}
            {systemStatus.database.connectionPool && ` • ${systemStatus.database.connectionPool} connections`}
            <br />Last checked: {formatTime(systemStatus.database.lastChecked)}
          </div>
        </div>

        <div style={{ marginBottom: 8 }}>
          <div style={{ display: 'flex', alignItems: 'center', gap: 8, marginBottom: 4 }}>
            <RobotOutlined />
            <strong>AI Service</strong>
            {getStatusIcon(systemStatus.ai.status)}
          </div>
          <div style={{ fontSize: '12px', marginLeft: 24, opacity: 0.8 }}>
            Status: {systemStatus.ai.status}
            {systemStatus.ai.provider && ` • ${systemStatus.ai.provider}`}
            {systemStatus.ai.responseTime && ` • ${systemStatus.ai.responseTime}ms`}
            <br />Last checked: {formatTime(systemStatus.ai.lastChecked)}
          </div>
        </div>

        <div style={{ marginTop: 12, padding: 8, background: 'rgba(0,0,0,0.1)', borderRadius: 4 }}>
          <div style={{ fontSize: '12px', opacity: 0.7 }}>
            Click to refresh status • Auto-refresh every 30s
          </div>
        </div>
      </div>
    )
  }

  if (collapsed) {
    // Collapsed view - just the status dots stacked vertically
    return (
      <Tooltip title={getTooltipContent()} placement="topRight">
        <div
          onClick={checkSystemHealth}
          style={{
            display: 'flex',
            flexDirection: 'column',
            alignItems: 'center',
            gap: '4px',
            padding: '6px',
            borderRadius: '4px',
            background: 'rgba(255, 255, 255, 0.05)',
            cursor: 'pointer',
            transition: 'all 0.2s ease',
            width: '100%',
          }}
          onMouseEnter={(e) => {
            e.currentTarget.style.background = 'rgba(255, 255, 255, 0.08)'
          }}
          onMouseLeave={(e) => {
            e.currentTarget.style.background = 'rgba(255, 255, 255, 0.05)'
          }}
        >
          <div style={{ position: 'relative', display: 'inline-flex', alignItems: 'center' }}>
            <CloudOutlined style={{ color: 'rgba(255, 255, 255, 0.7)', fontSize: '14px' }} />
            <div
              style={{
                position: 'absolute',
                top: '0px',
                right: '-1px',
                width: '4px',
                height: '4px',
                borderRadius: '50%',
                backgroundColor: getStatusColor(systemStatus.api.status),
                boxShadow: '0 0 0 1px #001529',
              }}
            />
          </div>
          <div style={{ position: 'relative', display: 'inline-flex', alignItems: 'center' }}>
            <DatabaseOutlined style={{ color: 'rgba(255, 255, 255, 0.7)', fontSize: '14px' }} />
            <div
              style={{
                position: 'absolute',
                top: '0px',
                right: '-1px',
                width: '4px',
                height: '4px',
                borderRadius: '50%',
                backgroundColor: getStatusColor(systemStatus.database.status),
                boxShadow: '0 0 0 1px #001529',
              }}
            />
          </div>
          <div style={{ position: 'relative', display: 'inline-flex', alignItems: 'center' }}>
            <RobotOutlined style={{ color: 'rgba(255, 255, 255, 0.7)', fontSize: '14px' }} />
            <div
              style={{
                position: 'absolute',
                top: '0px',
                right: '-1px',
                width: '4px',
                height: '4px',
                borderRadius: '50%',
                backgroundColor: getStatusColor(systemStatus.ai.status),
                boxShadow: '0 0 0 1px #001529',
              }}
            />
          </div>
        </div>
      </Tooltip>
    )
  }

  // Expanded view - full layout
  return (
    <Tooltip title={getTooltipContent()} placement="top">
      <div
        onClick={checkSystemHealth}
        style={{
          display: 'flex',
          alignItems: 'center',
          justifyContent: 'space-between',
          gap: '8px',
          padding: '8px 12px',
          borderRadius: '4px',
          background: 'rgba(255, 255, 255, 0.05)',
          border: 'none',
          cursor: 'pointer',
          transition: 'all 0.2s ease',
          width: '100%',
        }}
        onMouseEnter={(e) => {
          e.currentTarget.style.background = 'rgba(255, 255, 255, 0.08)'
        }}
        onMouseLeave={(e) => {
          e.currentTarget.style.background = 'rgba(255, 255, 255, 0.05)'
        }}
      >
        <Space size={6}>
          <div style={{ position: 'relative', display: 'inline-flex', alignItems: 'center' }}>
            <CloudOutlined style={{ color: 'rgba(255, 255, 255, 0.7)', fontSize: '12px' }} />
            <div
              style={{
                position: 'absolute',
                top: '0px',
                right: '-1px',
                width: '4px',
                height: '4px',
                borderRadius: '50%',
                backgroundColor: getStatusColor(systemStatus.api.status),
                boxShadow: '0 0 0 1px #001529',
              }}
            />
          </div>
          <div style={{ position: 'relative', display: 'inline-flex', alignItems: 'center' }}>
            <DatabaseOutlined style={{ color: 'rgba(255, 255, 255, 0.7)', fontSize: '12px' }} />
            <div
              style={{
                position: 'absolute',
                top: '0px',
                right: '-1px',
                width: '4px',
                height: '4px',
                borderRadius: '50%',
                backgroundColor: getStatusColor(systemStatus.database.status),
                boxShadow: '0 0 0 1px #001529',
              }}
            />
          </div>
          <div style={{ position: 'relative', display: 'inline-flex', alignItems: 'center' }}>
            <RobotOutlined style={{ color: 'rgba(255, 255, 255, 0.7)', fontSize: '12px' }} />
            <div
              style={{
                position: 'absolute',
                top: '0px',
                right: '-1px',
                width: '4px',
                height: '4px',
                borderRadius: '50%',
                backgroundColor: getStatusColor(systemStatus.ai.status),
                boxShadow: '0 0 0 1px #001529',
              }}
            />
          </div>
        </Space>
        <Text
          style={{
            fontSize: '10px',
            color: 'rgba(255, 255, 255, 0.8)',
            fontWeight: 400,
            textAlign: 'right',
          }}
        >
          {overall.text}
        </Text>
      </div>
    </Tooltip>
  )
}

// Hook for components that need to know the current API mode
export const useApiMode = () => {
  return {
    useMockData: false,
    fallbackToMock: false,
    debugMode: false,
    toggleMode: (useMock: boolean) => {
      console.log('Mock data functionality has been removed')
    }
  }
}

// Initialize API mode from localStorage on app start
export const initializeApiMode = () => {
  // Mock data functionality removed
}
