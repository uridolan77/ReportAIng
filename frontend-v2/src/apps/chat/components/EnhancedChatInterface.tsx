import React, { useState, useEffect } from 'react'
import { Row, Col, FloatButton, Card, Space, Typography, Switch, Tooltip } from 'antd'
import { EyeOutlined, EyeInvisibleOutlined, SettingOutlined } from '@ant-design/icons'
import { ChatInterface } from './ChatInterface'
import { LiveQueryTracker } from '@shared/components/ai/transparency/LiveQueryTracker'
import { RealTimeMonitor } from '@shared/components/ai/transparency/RealTimeMonitor'
import { InstantFeedbackPanel } from '@shared/components/ai/transparency/InstantFeedbackPanel'
import { useAppSelector, useAppDispatch } from '@shared/hooks'
import { selectMessages, selectIsLoading, chatActions } from '@shared/store/chat'
import { useRealTimeMonitoring } from '@shared/store/api/transparencyApi'

const { Text } = Typography

export interface EnhancedChatInterfaceProps {
  conversationId?: string
  showHeader?: boolean
  showExamples?: boolean
  className?: string
}

/**
 * EnhancedChatInterface - Chat interface with integrated AI transparency
 * 
 * Features:
 * - Real-time transparency tracking during query execution
 * - Live query flow visualization
 * - Instant feedback collection
 * - Toggle transparency view
 * - Performance monitoring
 */
export const EnhancedChatInterface: React.FC<EnhancedChatInterfaceProps> = ({
  conversationId,
  showHeader = true,
  showExamples = true,
  className = ''
}) => {
  const dispatch = useAppDispatch()
  const messages = useAppSelector(selectMessages)
  const isLoading = useAppSelector(selectIsLoading)
  
  const [showTransparency, setShowTransparency] = useState(false)
  const [currentQueryId, setCurrentQueryId] = useState<string | null>(null)
  const [transparencySettings, setTransparencySettings] = useState({
    enableRealTimeTracking: true,
    showQueryFlow: true,
    showFeedback: true,
    autoCollapse: false
  })

  // Real-time monitoring hook
  const { data: monitoringData, isConnected } = useRealTimeMonitoring({
    enabled: showTransparency && transparencySettings.enableRealTimeTracking
  })

  // Track current query for transparency
  useEffect(() => {
    if (isLoading && messages.length > 0) {
      const lastMessage = messages[messages.length - 1]
      if (lastMessage.type === 'user') {
        setCurrentQueryId(lastMessage.id)
      }
    } else if (!isLoading) {
      // Keep query ID for a bit longer to show completion
      setTimeout(() => setCurrentQueryId(null), 3000)
    }
  }, [isLoading, messages])

  const handleToggleTransparency = () => {
    setShowTransparency(!showTransparency)
    
    // Track transparency usage
    if (!showTransparency) {
      console.log('Transparency view enabled')
    }
  }

  const handleTransparencySettingChange = (setting: string, value: boolean) => {
    setTransparencySettings(prev => ({
      ...prev,
      [setting]: value
    }))
  }

  const renderTransparencyPanel = () => {
    if (!showTransparency) return null

    return (
      <div style={{ 
        height: '100%', 
        display: 'flex', 
        flexDirection: 'column',
        gap: '16px',
        overflow: 'auto'
      }}>
        {/* Transparency Settings */}
        <Card size="small" title="Transparency Settings">
          <Space direction="vertical" style={{ width: '100%' }}>
            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
              <Text style={{ fontSize: '12px' }}>Real-time Tracking</Text>
              <Switch
                size="small"
                checked={transparencySettings.enableRealTimeTracking}
                onChange={(checked) => handleTransparencySettingChange('enableRealTimeTracking', checked)}
              />
            </div>
            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
              <Text style={{ fontSize: '12px' }}>Query Flow</Text>
              <Switch
                size="small"
                checked={transparencySettings.showQueryFlow}
                onChange={(checked) => handleTransparencySettingChange('showQueryFlow', checked)}
              />
            </div>
            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
              <Text style={{ fontSize: '12px' }}>Feedback Panel</Text>
              <Switch
                size="small"
                checked={transparencySettings.showFeedback}
                onChange={(checked) => handleTransparencySettingChange('showFeedback', checked)}
              />
            </div>
          </Space>
        </Card>

        {/* Real-time Monitoring */}
        {transparencySettings.enableRealTimeTracking && (
          <Card size="small" title="System Monitor">
            <Space direction="vertical" style={{ width: '100%' }}>
              <div style={{ display: 'flex', justifyContent: 'space-between' }}>
                <Text style={{ fontSize: '12px' }}>Status:</Text>
                <Text style={{ 
                  fontSize: '12px', 
                  color: isConnected ? '#52c41a' : '#ff4d4f' 
                }}>
                  {isConnected ? 'Connected' : 'Disconnected'}
                </Text>
              </div>
              {monitoringData && (
                <>
                  <div style={{ display: 'flex', justifyContent: 'space-between' }}>
                    <Text style={{ fontSize: '12px' }}>Active Queries:</Text>
                    <Text style={{ fontSize: '12px' }}>{monitoringData.activeQueries}</Text>
                  </div>
                  <div style={{ display: 'flex', justifyContent: 'space-between' }}>
                    <Text style={{ fontSize: '12px' }}>Avg Confidence:</Text>
                    <Text style={{ fontSize: '12px' }}>
                      {(monitoringData.averageConfidence * 100).toFixed(1)}%
                    </Text>
                  </div>
                </>
              )}
            </Space>
          </Card>
        )}

        {/* Live Query Tracking */}
        {transparencySettings.showQueryFlow && currentQueryId && (
          <LiveQueryTracker
            queryId={currentQueryId}
            autoTrack={true}
            showStepDetails={true}
            showUserInfo={false}
          />
        )}

        {/* Instant Feedback */}
        {transparencySettings.showFeedback && currentQueryId && !isLoading && (
          <InstantFeedbackPanel
            queryId={currentQueryId}
            showRating={true}
            showComments={true}
            showSuggestions={false}
          />
        )}

        {/* Help Text */}
        {!currentQueryId && !isLoading && (
          <Card size="small">
            <Space direction="vertical" style={{ width: '100%', textAlign: 'center' }}>
              <EyeOutlined style={{ fontSize: '24px', color: '#d9d9d9' }} />
              <Text type="secondary" style={{ fontSize: '12px' }}>
                Send a query to see real-time transparency tracking
              </Text>
            </Space>
          </Card>
        )}
      </div>
    )
  }

  return (
    <div className={`enhanced-chat-interface ${className}`} style={{ height: '100%' }}>
      <Row gutter={[16, 0]} style={{ height: '100%' }}>
        {/* Main Chat Area */}
        <Col span={showTransparency ? 16 : 24} style={{ height: '100%' }}>
          <ChatInterface
            conversationId={conversationId}
            showHeader={showHeader}
            showExamples={showExamples}
          />
        </Col>
        
        {/* Transparency Panel */}
        {showTransparency && (
          <Col span={8} style={{ height: '100%' }}>
            <Card 
              title={
                <Space>
                  <EyeOutlined />
                  <span>AI Transparency</span>
                </Space>
              }
              size="small"
              style={{ height: '100%' }}
              bodyStyle={{ 
                height: 'calc(100% - 57px)', 
                overflow: 'hidden',
                padding: '12px'
              }}
            >
              {renderTransparencyPanel()}
            </Card>
          </Col>
        )}
      </Row>

      {/* Floating Action Buttons */}
      <FloatButton.Group
        trigger="hover"
        type="primary"
        style={{ right: 24, bottom: 24 }}
        icon={showTransparency ? <EyeInvisibleOutlined /> : <EyeOutlined />}
        tooltip={showTransparency ? "Hide Transparency" : "Show Transparency"}
        onClick={handleToggleTransparency}
      >
        <FloatButton
          icon={<SettingOutlined />}
          tooltip="Transparency Settings"
          onClick={() => {
            if (!showTransparency) {
              setShowTransparency(true)
            }
          }}
        />
      </FloatButton.Group>

      {/* Connection Status Indicator */}
      {showTransparency && transparencySettings.enableRealTimeTracking && (
        <div style={{
          position: 'fixed',
          top: '20px',
          right: showTransparency ? '320px' : '20px',
          zIndex: 1000,
          transition: 'right 0.3s ease'
        }}>
          <Tooltip title={`Transparency monitoring: ${isConnected ? 'Connected' : 'Disconnected'}`}>
            <div style={{
              width: '12px',
              height: '12px',
              borderRadius: '50%',
              backgroundColor: isConnected ? '#52c41a' : '#ff4d4f',
              border: '2px solid white',
              boxShadow: '0 2px 4px rgba(0,0,0,0.2)'
            }} />
          </Tooltip>
        </div>
      )}
    </div>
  )
}

export default EnhancedChatInterface
