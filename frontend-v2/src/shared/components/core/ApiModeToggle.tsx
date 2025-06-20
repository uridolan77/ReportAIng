/**
 * API Mode Toggle Component
 * 
 * Global toggle switch for switching between real API and mock data mode.
 * This component provides a centralized way to control the API mode across the entire application.
 */

import React from 'react'
import { Switch, Space, Typography, Tag, Tooltip } from 'antd'
import { 
  CloudOutlined, 
  ExperimentOutlined, 
  InfoCircleOutlined 
} from '@ant-design/icons'
import { ApiToggleService } from '../../services/apiToggleService'

const { Text } = Typography

export const ApiModeToggle: React.FC = () => {
  const [useMockData, setUseMockData] = React.useState(
    ApiToggleService.getConfig().useMockData
  )
  const [isConnected, setIsConnected] = React.useState(false)

  // Listen to service config changes
  React.useEffect(() => {
    const unsubscribe = ApiToggleService.addListener((config) => {
      setUseMockData(config.useMockData)
    })
    return unsubscribe
  }, [])

  // Check API connectivity status
  React.useEffect(() => {
    const checkConnectivity = async () => {
      try {
        // Try to ping the health endpoint
        const response = await fetch('/api/health', {
          method: 'GET',
          timeout: 3000
        } as any)
        setIsConnected(response.ok)
      } catch {
        setIsConnected(false)
      }
    }

    checkConnectivity()
    const interval = setInterval(checkConnectivity, 30000) // Check every 30 seconds

    return () => clearInterval(interval)
  }, [])

  const handleToggle = (checked: boolean) => {
    // Update the global API toggle service
    ApiToggleService.updateConfig({
      useMockData: checked,
      fallbackToMock: true,
      debugMode: import.meta.env.DEV
    })

    // Store preference in localStorage
    localStorage.setItem('apiMode', checked ? 'mock' : 'real')

    // Note: No page reload needed - components will react to the service change
    console.log(`🔄 API Mode switched to: ${checked ? 'Mock Data' : 'Real API'}`)
  }

  const getModeInfo = () => {
    if (useMockData) {
      return {
        icon: <ExperimentOutlined style={{ color: '#faad14' }} />,
        label: 'Mock Mode',
        color: 'orange',
        description: 'Using mock data for development and testing'
      }
    } else {
      return {
        icon: <CloudOutlined style={{ color: isConnected ? '#52c41a' : '#ff4d4f' }} />,
        label: isConnected ? 'API Mode' : 'API Mode (Offline)',
        color: isConnected ? 'green' : 'red',
        description: isConnected 
          ? 'Connected to real API backend' 
          : 'API backend unavailable, falling back to mock data'
      }
    }
  }

  const modeInfo = getModeInfo()

  return (
    <Space size="small">
      <Tooltip 
        title={
          <div>
            <div style={{ marginBottom: 8 }}>
              <strong>API Mode Control</strong>
            </div>
            <div style={{ marginBottom: 4 }}>
              • <strong>Mock Mode:</strong> Uses local mock data for development
            </div>
            <div style={{ marginBottom: 4 }}>
              • <strong>API Mode:</strong> Connects to real backend services
            </div>
            <div style={{ marginTop: 8, fontSize: '12px', opacity: 0.8 }}>
              {modeInfo.description}
            </div>
          </div>
        }
        placement="bottomRight"
      >
        <InfoCircleOutlined style={{ color: '#8c8c8c', cursor: 'help' }} />
      </Tooltip>

      <Tag 
        icon={modeInfo.icon}
        color={modeInfo.color}
        style={{ 
          margin: 0,
          display: 'flex',
          alignItems: 'center',
          gap: 4,
          fontSize: '12px',
          fontWeight: 500
        }}
      >
        {modeInfo.label}
      </Tag>

      <Switch
        checked={useMockData}
        onChange={handleToggle}
        size="small"
        checkedChildren="Mock"
        unCheckedChildren="API"
        style={{
          backgroundColor: useMockData ? '#faad14' : (isConnected ? '#52c41a' : '#ff4d4f')
        }}
      />

      {!useMockData && !isConnected && (
        <Text 
          type="secondary" 
          style={{ 
            fontSize: '11px',
            fontStyle: 'italic'
          }}
        >
          (Auto-fallback active)
        </Text>
      )}
    </Space>
  )
}

// Hook for components that need to know the current API mode
export const useApiMode = () => {
  const [config, setConfig] = React.useState(ApiToggleService.getConfig())

  React.useEffect(() => {
    // Listen for service config changes
    const unsubscribe = ApiToggleService.addListener(setConfig)

    // Also listen for localStorage changes (for cross-tab sync)
    const handleStorageChange = (e: StorageEvent) => {
      if (e.key === 'apiMode') {
        const savedMode = e.newValue
        const useMockData = savedMode === 'mock'
        ApiToggleService.updateConfig({ useMockData })
      }
    }

    window.addEventListener('storage', handleStorageChange)

    return () => {
      unsubscribe()
      window.removeEventListener('storage', handleStorageChange)
    }
  }, [])

  return {
    useMockData: config.useMockData,
    fallbackToMock: config.fallbackToMock,
    debugMode: config.debugMode,
    toggleMode: (useMock: boolean) => {
      ApiToggleService.updateConfig({
        useMockData: useMock
      })
      localStorage.setItem('apiMode', useMock ? 'mock' : 'real')
    }
  }
}

// Initialize API mode from localStorage on app start
export const initializeApiMode = () => {
  const savedMode = localStorage.getItem('apiMode')
  const useMockData = savedMode === 'mock' ||
                     (savedMode === null && import.meta.env.DEV)

  ApiToggleService.updateConfig({
    useMockData,
    fallbackToMock: true,
    debugMode: import.meta.env.DEV
  })
}
