/**
 * API Status Dashboard
 * 
 * Provides a comprehensive view of API status, mock data usage,
 * and development tools for switching between real and mock data.
 */

import React from 'react'
import { Card, Row, Col, Statistic, Button, Space, Tag, Alert, Typography, Divider } from 'antd'
import { 
  ApiOutlined, 
  DatabaseOutlined, 
  CheckCircleOutlined, 
  ExclamationCircleOutlined,
  ClockCircleOutlined,
  SettingOutlined,
  ReloadOutlined
} from '@ant-design/icons'
import { useApiToggle } from '../../services/apiToggleService'
import { useApiStatus, useApiDebugInfo } from '../../hooks/useEnhancedApi'

const { Text, Title } = Typography

interface ApiStatusDashboardProps {
  showControls?: boolean
  compact?: boolean
}

export const ApiStatusDashboard: React.FC<ApiStatusDashboardProps> = ({ 
  showControls = true, 
  compact = false 
}) => {
  const { config, isUsingMockData, toggleMockData, updateConfig, status } = useApiToggle()
  const { data: apiStatus, refetch: refetchStatus } = useApiStatus()
  const debugInfo = useApiDebugInfo()

  const getStatusColor = () => {
    if (isUsingMockData) return 'blue'
    return apiStatus?.isOnline ? 'green' : 'red'
  }

  const getStatusText = () => {
    if (isUsingMockData) return 'Mock Data Active'
    return apiStatus?.isOnline ? 'API Online' : 'API Offline'
  }

  const getStatusIcon = () => {
    if (isUsingMockData) return <DatabaseOutlined />
    return apiStatus?.isOnline ? <CheckCircleOutlined /> : <ExclamationCircleOutlined />
  }

  if (compact) {
    return (
      <Space>
        <Tag 
          color={getStatusColor()} 
          icon={getStatusIcon()}
        >
          {getStatusText()}
        </Tag>
        {showControls && (
          <Button 
            size="small" 
            onClick={toggleMockData}
            type={isUsingMockData ? 'default' : 'primary'}
          >
            {isUsingMockData ? 'Use Real API' : 'Use Mock Data'}
          </Button>
        )}
      </Space>
    )
  }

  return (
    <Card title="API Status Dashboard" size="small">
      <Row gutter={[16, 16]}>
        {/* Current Status */}
        <Col xs={24} sm={12} md={6}>
          <Card size="small">
            <Statistic
              title="Current Mode"
              value={isUsingMockData ? 'Mock Data' : 'Real API'}
              prefix={getStatusIcon()}
              valueStyle={{ color: getStatusColor() === 'green' ? '#52c41a' : getStatusColor() === 'red' ? '#f5222d' : '#1890ff' }}
            />
          </Card>
        </Col>

        {/* API Health */}
        <Col xs={24} sm={12} md={6}>
          <Card size="small">
            <Statistic
              title="API Health"
              value={apiStatus?.isOnline ? 'Online' : 'Offline'}
              prefix={apiStatus?.isOnline ? <CheckCircleOutlined /> : <ExclamationCircleOutlined />}
              valueStyle={{ color: apiStatus?.isOnline ? '#52c41a' : '#f5222d' }}
            />
          </Card>
        </Col>

        {/* Environment */}
        <Col xs={24} sm={12} md={6}>
          <Card size="small">
            <Statistic
              title="Environment"
              value={debugInfo.data?.environment || 'unknown'}
              prefix={<SettingOutlined />}
            />
          </Card>
        </Col>

        {/* Last Check */}
        <Col xs={24} sm={12} md={6}>
          <Card size="small">
            <Statistic
              title="Last Check"
              value={apiStatus?.timestamp ? new Date(apiStatus.timestamp).toLocaleTimeString() : 'Never'}
              prefix={<ClockCircleOutlined />}
            />
          </Card>
        </Col>
      </Row>

      {/* Configuration Details */}
      <Divider />
      <Row gutter={[16, 16]}>
        <Col xs={24} md={12}>
          <Title level={5}>Configuration</Title>
          <Space direction="vertical" style={{ width: '100%' }}>
            <div>
              <Text strong>Mock Data Enabled: </Text>
              <Tag color={config.useMockData ? 'blue' : 'default'}>
                {config.useMockData ? 'Yes' : 'No'}
              </Tag>
            </div>
            <div>
              <Text strong>Fallback to Mock: </Text>
              <Tag color={config.fallbackToMock ? 'green' : 'orange'}>
                {config.fallbackToMock ? 'Enabled' : 'Disabled'}
              </Tag>
            </div>
            <div>
              <Text strong>Debug Mode: </Text>
              <Tag color={config.debugMode ? 'purple' : 'default'}>
                {config.debugMode ? 'On' : 'Off'}
              </Tag>
            </div>
            <div>
              <Text strong>Mock Delay: </Text>
              <Text code>{config.mockDelay}ms</Text>
            </div>
          </Space>
        </Col>

        <Col xs={24} md={12}>
          <Title level={5}>Status Details</Title>
          <Space direction="vertical" style={{ width: '100%' }}>
            {apiStatus?.error && (
              <Alert
                message="API Error"
                description={apiStatus.error}
                type="error"
                size="small"
                showIcon
              />
            )}
            <div>
              <Text strong>Mock Data Available: </Text>
              <Tag color={status.mockDataAvailable ? 'green' : 'red'}>
                {status.mockDataAvailable ? 'Yes' : 'No'}
              </Tag>
            </div>
            <div>
              <Text strong>API Status Code: </Text>
              <Text code>{apiStatus?.status || 'N/A'}</Text>
            </div>
          </Space>
        </Col>
      </Row>

      {/* Controls */}
      {showControls && (
        <>
          <Divider />
          <Space wrap>
            <Button 
              type={isUsingMockData ? 'default' : 'primary'}
              onClick={toggleMockData}
              icon={<ApiOutlined />}
            >
              {isUsingMockData ? 'Switch to Real API' : 'Switch to Mock Data'}
            </Button>
            
            <Button 
              onClick={() => refetchStatus()}
              icon={<ReloadOutlined />}
            >
              Refresh Status
            </Button>

            <Button 
              onClick={() => updateConfig({ fallbackToMock: !config.fallbackToMock })}
              type={config.fallbackToMock ? 'default' : 'dashed'}
            >
              {config.fallbackToMock ? 'Disable' : 'Enable'} Fallback
            </Button>

            <Button 
              onClick={() => updateConfig({ debugMode: !config.debugMode })}
              type={config.debugMode ? 'default' : 'dashed'}
            >
              {config.debugMode ? 'Disable' : 'Enable'} Debug
            </Button>
          </Space>
        </>
      )}

      {/* Development Info */}
      {config.debugMode && (
        <>
          <Divider />
          <Alert
            message="Debug Information"
            description={
              <div>
                <Text code style={{ fontSize: '11px' }}>
                  {JSON.stringify(status, null, 2)}
                </Text>
              </div>
            }
            type="info"
            size="small"
          />
        </>
      )}
    </Card>
  )
}

// Compact version for headers/toolbars
export const ApiStatusIndicator: React.FC = () => {
  return <ApiStatusDashboard compact showControls={false} />
}

// Full dashboard for admin/debug pages
export const FullApiStatusDashboard: React.FC = () => {
  return <ApiStatusDashboard compact={false} showControls />
}
