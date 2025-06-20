import React, { useEffect, useState } from 'react'
import { Badge, Tooltip, Button, Space, Typography, Alert } from 'antd'
import {
  WifiOutlined,
  DisconnectOutlined,
  LoadingOutlined,
  ExclamationCircleOutlined,
  ReloadOutlined
} from '@ant-design/icons'
import { useAppSelector } from '@shared/hooks'
import { selectIsConnected, selectConnectionError } from '@shared/store/chat'
import { socketService } from '@shared/services/socketService'

const { Text } = Typography

interface ConnectionStatusProps {
  showDetails?: boolean
  onReconnect?: () => void
}

export const ConnectionStatus: React.FC<ConnectionStatusProps> = ({
  showDetails = false,
  onReconnect,
}) => {
  const isConnected = useAppSelector(selectIsConnected)
  const connectionError = useAppSelector(selectConnectionError)
  const [connectionState, setConnectionState] = useState<'connected' | 'connecting' | 'disconnected' | 'error'>('disconnected')
  const [lastConnected, setLastConnected] = useState<Date | null>(null)

  useEffect(() => {
    const updateConnectionState = () => {
      const state = socketService.getConnectionState()
      setConnectionState(state)
      
      if (state === 'connected' && !lastConnected) {
        setLastConnected(new Date())
      } else if (state !== 'connected') {
        setLastConnected(null)
      }
    }

    // Update immediately
    updateConnectionState()

    // Set up interval to check connection state
    const interval = setInterval(updateConnectionState, 1000)

    return () => clearInterval(interval)
  }, [isConnected, lastConnected])

  const handleReconnect = async () => {
    try {
      await socketService.connect()
      onReconnect?.()
    } catch (error) {
      console.error('Failed to reconnect:', error)
    }
  }

  const getStatusConfig = () => {
    switch (connectionState) {
      case 'connected':
        return {
          status: 'success' as const,
          icon: <WifiOutlined />,
          text: 'Connected',
          color: '#52c41a',
          tooltip: `Connected to real-time service${lastConnected ? ` since ${lastConnected.toLocaleTimeString()}` : ''}`,
        }
      case 'connecting':
        return {
          status: 'processing' as const,
          icon: <LoadingOutlined spin />,
          text: 'Connecting',
          color: '#1890ff',
          tooltip: 'Connecting to real-time service...',
        }
      case 'error':
        return {
          status: 'error' as const,
          icon: <ExclamationCircleOutlined />,
          text: 'Error',
          color: '#ff4d4f',
          tooltip: connectionError || 'Connection error occurred',
        }
      default:
        return {
          status: 'default' as const,
          icon: <DisconnectOutlined />,
          text: 'Disconnected',
          color: '#d9d9d9',
          tooltip: 'Not connected to real-time service',
        }
    }
  }

  const statusConfig = getStatusConfig()

  if (showDetails) {
    return (
      <div>
        {connectionState === 'error' && connectionError && (
          <Alert
            type="error"
            message="Connection Error"
            description={connectionError}
            action={
              <Button size="small" icon={<ReloadOutlined />} onClick={handleReconnect}>
                Retry
              </Button>
            }
            style={{ marginBottom: 16 }}
            showIcon
          />
        )}
        
        {connectionState === 'disconnected' && (
          <Alert
            type="warning"
            message="Real-time Features Unavailable"
            description="Some features like live query progress may not work properly."
            action={
              <Button size="small" icon={<ReloadOutlined />} onClick={handleReconnect}>
                Connect
              </Button>
            }
            style={{ marginBottom: 16 }}
            showIcon
          />
        )}

        <Space>
          <Badge status={statusConfig.status} />
          {statusConfig.icon}
          <Text style={{ color: statusConfig.color }}>
            {statusConfig.text}
          </Text>
          {connectionState !== 'connected' && connectionState !== 'connecting' && (
            <Button size="small" icon={<ReloadOutlined />} onClick={handleReconnect}>
              Reconnect
            </Button>
          )}
        </Space>
      </div>
    )
  }

  return (
    <Tooltip title={statusConfig.tooltip}>
      <Badge status={statusConfig.status} text={statusConfig.text} />
    </Tooltip>
  )
}

// Hook for connection status
export const useConnectionStatus = () => {
  const isConnected = useAppSelector(selectIsConnected)
  const connectionError = useAppSelector(selectConnectionError)
  
  const connect = async () => {
    try {
      await socketService.connect()
      return true
    } catch (error) {
      console.error('Connection failed:', error)
      return false
    }
  }

  const disconnect = () => {
    socketService.disconnect()
  }

  const getStatus = () => {
    return socketService.getConnectionState()
  }

  return {
    isConnected,
    connectionError,
    connect,
    disconnect,
    getStatus,
  }
}
