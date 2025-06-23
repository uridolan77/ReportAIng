import React, { useState, useEffect, useCallback } from 'react'
import { 
  Row, 
  Col, 
  Card, 
  Space, 
  Typography, 
  Switch, 
  Button, 
  Drawer, 
  Tabs, 
  FloatButton,
  Badge,
  Tooltip,
  Dropdown,
  Menu,
  Alert,
  Divider
} from 'antd'
import {
  EyeOutlined,
  EyeInvisibleOutlined,
  SettingOutlined,
  HistoryOutlined,
  BookOutlined,
  BulbOutlined,
  BarChartOutlined,
  UserOutlined,
  RobotOutlined,
  ThunderboltOutlined,
  DatabaseOutlined,
  ExperimentOutlined,
  MenuOutlined
} from '@ant-design/icons'

// Core Chat Components
import { ChatInterface } from './ChatInterface'
import { MessageList } from './MessageList'
import { ChatInput } from './ChatInput'
import { StreamingProgress } from './StreamingProgress'

// ProcessFlow Transparency Components
import { ProcessFlowSessionViewer, ProcessFlowDashboard } from '@shared/components/ai/transparency'

// Enhanced Features Components
import { QuerySuggestions } from './QuerySuggestions'
import { ConversationHistory } from './ConversationHistory'
import { BusinessContextPanel } from './BusinessContextPanel'
import { PerformanceMetrics } from './PerformanceMetrics'

// Hooks and Store
import { useAppSelector, useAppDispatch } from '@shared/hooks'
import { selectMessages, selectIsLoading, selectCurrentConversation, chatActions } from '@shared/store/chat'
import { useRealTimeMonitoring, useTransparencyReview } from '@shared/store/api/transparencyApi'

const { Title, Text } = Typography
const { TabPane } = Tabs

export interface ComprehensiveChatInterfaceProps {
  conversationId?: string
  mode?: 'normal' | 'advanced'
  showHeader?: boolean
  className?: string
}

interface ChatSettings {
  // Display Settings
  showTransparency: boolean
  showBusinessContext: boolean
  showPerformanceMetrics: boolean
  showQuerySuggestions: boolean
  showConversationHistory: boolean
  
  // Transparency Settings
  enableRealTimeTracking: boolean
  showQueryFlow: boolean
  showConfidenceIndicators: boolean
  showTokenUsage: boolean
  enableFeedbackCollection: boolean
  
  // Advanced Features
  enableModelComparison: boolean
  showOptimizationInsights: boolean
  enableAdvancedAnalytics: boolean
  showBusinessIntelligence: boolean
  
  // Layout Settings
  transparencyPanelPosition: 'right' | 'bottom' | 'drawer'
  compactMode: boolean
  autoCollapsePanels: boolean
}

const defaultSettings: ChatSettings = {
  // Display Settings
  showTransparency: false,
  showBusinessContext: false,
  showPerformanceMetrics: false,
  showQuerySuggestions: true,
  showConversationHistory: false,
  
  // Transparency Settings
  enableRealTimeTracking: true,
  showQueryFlow: true,
  showConfidenceIndicators: true,
  showTokenUsage: true,
  enableFeedbackCollection: true,
  
  // Advanced Features
  enableModelComparison: false,
  showOptimizationInsights: false,
  enableAdvancedAnalytics: false,
  showBusinessIntelligence: false,
  
  // Layout Settings
  transparencyPanelPosition: 'right',
  compactMode: false,
  autoCollapsePanels: true
}

/**
 * ComprehensiveChatInterface - Advanced chat interface with all available features
 * 
 * NORMAL MODE Features:
 * - Standard chat interface
 * - Basic query suggestions
 * - Simple confidence indicators
 * - Optional transparency toggle
 * 
 * ADVANCED MODE Features:
 * - Real-time transparency tracking
 * - Live query flow analysis
 * - Business context analysis
 * - Performance metrics monitoring
 * - Model comparison tools
 * - Advanced analytics dashboard
 * - Optimization insights
 * - Comprehensive feedback system
 * - Multi-panel layout options
 */
export const ComprehensiveChatInterface: React.FC<ComprehensiveChatInterfaceProps> = ({
  conversationId,
  mode = 'normal',
  showHeader = true,
  className = ''
}) => {
  const dispatch = useAppDispatch()
  const messages = useAppSelector(selectMessages)
  const isLoading = useAppSelector(selectIsLoading)
  const currentConversation = useAppSelector(selectCurrentConversation)
  
  const [settings, setSettings] = useState<ChatSettings>({
    ...defaultSettings,
    // Auto-enable features based on mode
    showTransparency: mode === 'advanced',
    showBusinessContext: mode === 'advanced',
    showPerformanceMetrics: mode === 'advanced',
    enableAdvancedAnalytics: mode === 'advanced'
  })
  
  const [currentQueryId, setCurrentQueryId] = useState<string | null>(null)
  const [selectedTraceId, setSelectedTraceId] = useState<string | null>(null)
  const [drawerVisible, setDrawerVisible] = useState(false)
  const [activePanel, setActivePanel] = useState<string>('transparency')

  // Real-time monitoring
  const { data: monitoringData, isConnected } = useRealTimeMonitoring({
    enabled: settings.enableRealTimeTracking && mode === 'advanced'
  })

  // Track current query for transparency
  useEffect(() => {
    if (isLoading && messages.length > 0) {
      const lastMessage = messages[messages.length - 1]
      if (lastMessage.type === 'user') {
        setCurrentQueryId(lastMessage.id)
      }
    } else if (!isLoading) {
      setTimeout(() => setCurrentQueryId(null), 3000)
    }
  }, [isLoading, messages])

  const handleSettingChange = useCallback((key: keyof ChatSettings, value: boolean | string) => {
    setSettings(prev => ({ ...prev, [key]: value }))
  }, [])

  const handleModeToggle = useCallback(() => {
    const newMode = mode === 'normal' ? 'advanced' : 'normal'
    
    if (newMode === 'advanced') {
      setSettings(prev => ({
        ...prev,
        showTransparency: true,
        showBusinessContext: true,
        showPerformanceMetrics: true,
        enableAdvancedAnalytics: true
      }))
    } else {
      setSettings(prev => ({
        ...prev,
        showTransparency: false,
        showBusinessContext: false,
        showPerformanceMetrics: false,
        enableAdvancedAnalytics: false
      }))
    }
  }, [mode])

  const renderModeToggle = () => (
    <Card size="small" style={{ marginBottom: '16px' }}>
      <Space style={{ width: '100%', justifyContent: 'space-between' }}>
        <Space>
          <Text strong>Interface Mode:</Text>
          <Badge 
            status={mode === 'advanced' ? 'processing' : 'default'} 
            text={mode === 'normal' ? 'Normal' : 'Advanced'}
          />
        </Space>
        <Switch
          checked={mode === 'advanced'}
          onChange={handleModeToggle}
          checkedChildren="Advanced"
          unCheckedChildren="Normal"
        />
      </Space>
    </Card>
  )

  const renderQuickSettings = () => (
    <Card size="small" title="Quick Settings">
      <Space direction="vertical" style={{ width: '100%' }}>
        <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
          <Text style={{ fontSize: '12px' }}>Transparency</Text>
          <Switch
            size="small"
            checked={settings.showTransparency}
            onChange={(checked) => handleSettingChange('showTransparency', checked)}
          />
        </div>
        
        {mode === 'advanced' && (
          <>
            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
              <Text style={{ fontSize: '12px' }}>Business Context</Text>
              <Switch
                size="small"
                checked={settings.showBusinessContext}
                onChange={(checked) => handleSettingChange('showBusinessContext', checked)}
              />
            </div>
            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
              <Text style={{ fontSize: '12px' }}>Performance Metrics</Text>
              <Switch
                size="small"
                checked={settings.showPerformanceMetrics}
                onChange={(checked) => handleSettingChange('showPerformanceMetrics', checked)}
              />
            </div>
            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
              <Text style={{ fontSize: '12px' }}>Real-time Tracking</Text>
              <Switch
                size="small"
                checked={settings.enableRealTimeTracking}
                onChange={(checked) => handleSettingChange('enableRealTimeTracking', checked)}
              />
            </div>
          </>
        )}
      </Space>
    </Card>
  )

  const renderTransparencyPanel = () => {
    if (!settings.showTransparency) return null

    return (
      <Tabs activeKey={activePanel} onChange={setActivePanel} size="small">
        <TabPane 
          tab={
            <Space>
              <EyeOutlined />
              <span>Live Tracking</span>
              {currentQueryId && <Badge status="processing" />}
            </Space>
          } 
          key="transparency"
        >
          <Space direction="vertical" style={{ width: '100%' }}>
            {/* Real-time Monitor */}
            {settings.enableRealTimeTracking && (
              <Card size="small" title="System Status">
                <Space direction="vertical" style={{ width: '100%' }}>
                  <div style={{ display: 'flex', justifyContent: 'space-between' }}>
                    <Text style={{ fontSize: '12px' }}>Connection:</Text>
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

            {/* ProcessFlow Session Tracking */}
            {currentQueryId && settings.showQueryFlow && (
              <ProcessFlowSessionViewer
                sessionId={currentQueryId}
                showDetailedSteps={true}
                showLogs={true}
                showTransparency={true}
                compact={true}
              />
            )}

            {/* ProcessFlow Dashboard */}
            {settings.enableAdvancedAnalytics && (
              <ProcessFlowDashboard
                filters={{ days: 1, includeDetails: true }}
                showCharts={false}
                showMetrics={true}
              />
            )}
          </Space>
        </TabPane>

        {mode === 'advanced' && (
          <>
            <TabPane 
              tab={
                <Space>
                  <DatabaseOutlined />
                  <span>Business Context</span>
                </Space>
              } 
              key="business"
            >
              <BusinessContextPanel 
                conversationId={conversationId}
                showEntityDetails={true}
                showRelationships={true}
              />
            </TabPane>

            <TabPane 
              tab={
                <Space>
                  <BarChartOutlined />
                  <span>Performance</span>
                </Space>
              } 
              key="performance"
            >
              <PerformanceMetrics 
                showRealTime={true}
                showHistorical={true}
                showOptimizations={true}
              />
            </TabPane>

            <TabPane 
              tab={
                <Space>
                  <ExperimentOutlined />
                  <span>Analytics</span>
                </Space>
              } 
              key="analytics"
            >
              <ModelPerformanceComparison
                models={[]} // Would be populated from API
                showRadarChart={true}
                showRecommendations={true}
              />
            </TabPane>
          </>
        )}
      </Tabs>
    )
  }

  const renderSidePanel = () => {
    const panelWidth = settings.compactMode ? 300 : 400
    const showPanel = settings.showTransparency || 
                     settings.showBusinessContext || 
                     settings.showPerformanceMetrics

    if (!showPanel) return null

    return (
      <Col span={6} style={{ height: '100%' }}>
        <Card 
          title={
            <Space>
              <EyeOutlined />
              <span>AI Insights</span>
              <Badge 
                status={isConnected ? 'processing' : 'default'} 
                text={mode}
              />
            </Space>
          }
          size="small"
          style={{ height: '100%' }}
          bodyStyle={{ 
            height: 'calc(100% - 57px)', 
            overflow: 'hidden',
            padding: '12px'
          }}
          extra={
            <Button
              type="text"
              size="small"
              icon={<SettingOutlined />}
              onClick={() => setDrawerVisible(true)}
            />
          }
        >
          <div style={{ height: '100%', overflow: 'auto' }}>
            {renderModeToggle()}
            {renderQuickSettings()}
            <Divider />
            {renderTransparencyPanel()}
          </div>
        </Card>
      </Col>
    )
  }

  const renderMainChat = () => {
    const showPanel = settings.showTransparency || 
                     settings.showBusinessContext || 
                     settings.showPerformanceMetrics
    
    return (
      <Col span={showPanel ? 18 : 24} style={{ height: '100%' }}>
        <div style={{ height: '100%', display: 'flex', flexDirection: 'column' }}>
          {/* Enhanced Chat Header */}
          {showHeader && (
            <Card size="small" style={{ marginBottom: '8px' }}>
              <Space style={{ width: '100%', justifyContent: 'space-between' }}>
                <Space>
                  <RobotOutlined />
                  <Title level={4} style={{ margin: 0 }}>
                    BI Reporting Copilot
                  </Title>
                  <Badge 
                    status={mode === 'advanced' ? 'processing' : 'default'} 
                    text={mode === 'normal' ? 'Standard Mode' : 'Advanced Mode'}
                  />
                </Space>
                <Space>
                  {mode === 'advanced' && (
                    <Tooltip title="Advanced features enabled">
                      <ThunderboltOutlined style={{ color: '#1890ff' }} />
                    </Tooltip>
                  )}
                  <Button
                    type="text"
                    size="small"
                    icon={<SettingOutlined />}
                    onClick={() => setDrawerVisible(true)}
                  />
                </Space>
              </Space>
            </Card>
          )}

          {/* Main Chat Interface */}
          <div style={{ flex: 1 }}>
            <ChatInterface
              conversationId={conversationId}
              showHeader={false}
              showExamples={!currentConversation}
            />
          </div>
        </div>
      </Col>
    )
  }

  const renderSettingsDrawer = () => (
    <Drawer
      title="Chat Interface Settings"
      placement="right"
      width={400}
      onClose={() => setDrawerVisible(false)}
      open={drawerVisible}
    >
      <Space direction="vertical" style={{ width: '100%' }} size="large">
        {/* Mode Selection */}
        <Card size="small" title="Interface Mode">
          <Space direction="vertical" style={{ width: '100%' }}>
            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
              <Text strong>Advanced Mode</Text>
              <Switch
                checked={mode === 'advanced'}
                onChange={handleModeToggle}
                checkedChildren="ON"
                unCheckedChildren="OFF"
              />
            </div>
            <Text type="secondary" style={{ fontSize: '12px' }}>
              {mode === 'normal' 
                ? 'Standard chat interface with basic features'
                : 'Advanced interface with transparency, analytics, and business intelligence'
              }
            </Text>
          </Space>
        </Card>

        {/* Display Settings */}
        <Card size="small" title="Display Settings">
          <Space direction="vertical" style={{ width: '100%' }}>
            {Object.entries({
              showTransparency: 'Transparency Panel',
              showBusinessContext: 'Business Context',
              showPerformanceMetrics: 'Performance Metrics',
              showQuerySuggestions: 'Query Suggestions',
              showConversationHistory: 'Conversation History'
            }).map(([key, label]) => (
              <div key={key} style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                <Text>{label}</Text>
                <Switch
                  size="small"
                  checked={settings[key as keyof ChatSettings] as boolean}
                  onChange={(checked) => handleSettingChange(key as keyof ChatSettings, checked)}
                  disabled={mode === 'normal' && ['showBusinessContext', 'showPerformanceMetrics'].includes(key)}
                />
              </div>
            ))}
          </Space>
        </Card>

        {/* Advanced Features (only in advanced mode) */}
        {mode === 'advanced' && (
          <Card size="small" title="Advanced Features">
            <Space direction="vertical" style={{ width: '100%' }}>
              {Object.entries({
                enableModelComparison: 'Model Comparison',
                showOptimizationInsights: 'Optimization Insights',
                enableAdvancedAnalytics: 'Advanced Analytics',
                showBusinessIntelligence: 'Business Intelligence'
              }).map(([key, label]) => (
                <div key={key} style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                  <Text>{label}</Text>
                  <Switch
                    size="small"
                    checked={settings[key as keyof ChatSettings] as boolean}
                    onChange={(checked) => handleSettingChange(key as keyof ChatSettings, checked)}
                  />
                </div>
              ))}
            </Space>
          </Card>
        )}
      </Space>
    </Drawer>
  )

  return (
    <div className={`comprehensive-chat-interface ${className}`} style={{ height: '100%' }}>
      <Row gutter={[8, 0]} style={{ height: '100%' }}>
        {renderMainChat()}
        {renderSidePanel()}
      </Row>

      {/* Floating Action Buttons */}
      <FloatButton.Group
        trigger="hover"
        type="primary"
        style={{ right: 24, bottom: 24 }}
        icon={<MenuOutlined />}
      >
        <FloatButton
          icon={settings.showTransparency ? <EyeInvisibleOutlined /> : <EyeOutlined />}
          tooltip={settings.showTransparency ? "Hide Transparency" : "Show Transparency"}
          onClick={() => handleSettingChange('showTransparency', !settings.showTransparency)}
        />
        <FloatButton
          icon={<SettingOutlined />}
          tooltip="Settings"
          onClick={() => setDrawerVisible(true)}
        />
        {mode === 'advanced' && (
          <FloatButton
            icon={<BarChartOutlined />}
            tooltip="Performance Dashboard"
            onClick={() => {
              handleSettingChange('showPerformanceMetrics', true)
              setActivePanel('performance')
            }}
          />
        )}
      </FloatButton.Group>

      {/* Settings Drawer */}
      {renderSettingsDrawer()}

      {/* Connection Status Indicator */}
      {settings.enableRealTimeTracking && (
        <div style={{
          position: 'fixed',
          top: '20px',
          right: settings.showTransparency ? '320px' : '20px',
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

      {/* Mode Indicator */}
      <div style={{
        position: 'fixed',
        top: '20px',
        left: '20px',
        zIndex: 1000
      }}>
        <Badge 
          status={mode === 'advanced' ? 'processing' : 'default'} 
          text={mode === 'normal' ? 'Normal Mode' : 'Advanced Mode'}
          style={{ 
            background: 'rgba(255,255,255,0.9)', 
            padding: '4px 8px', 
            borderRadius: '4px',
            fontSize: '12px'
          }}
        />
      </div>
    </div>
  )
}

export default ComprehensiveChatInterface
