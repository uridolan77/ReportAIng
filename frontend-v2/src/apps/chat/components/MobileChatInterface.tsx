import React, { useState, useRef, useEffect } from 'react'
import {
  Button,
  Input,
  Typography,
  Space,
  Card,
  Drawer,
  FloatButton,
  Badge,
  Tooltip,
  Affix
} from 'antd'
import {
  SendOutlined,
  MenuOutlined,
  HistoryOutlined,
  SettingOutlined,
  MicrophoneOutlined,
  PlusOutlined,
  ArrowUpOutlined,
  CloseOutlined
} from '@ant-design/icons'
import { useResponsive, getResponsiveSpacing, getResponsiveFontSize } from '@shared/hooks/useResponsive'
import { MessageList } from './MessageList'
import { StreamingProgress } from './StreamingProgress'
import { ConnectionStatus } from './ConnectionStatus'
import type { ChatMessage } from '@shared/types/chat'

const { TextArea } = Input
const { Text, Title } = Typography

interface MobileChatInterfaceProps {
  messages: ChatMessage[]
  onSendMessage: (message: string) => void
  isLoading?: boolean
  streamingProgress?: any
}

export const MobileChatInterface: React.FC<MobileChatInterfaceProps> = ({
  messages,
  onSendMessage,
  isLoading = false,
  streamingProgress
}) => {
  const [message, setMessage] = useState('')
  const [showSidebar, setShowSidebar] = useState(false)
  const [isRecording, setIsRecording] = useState(false)
  const [showScrollTop, setShowScrollTop] = useState(false)
  const [inputFocused, setInputFocused] = useState(false)
  
  const responsive = useResponsive()
  const spacing = getResponsiveSpacing(responsive)
  const fontSize = getResponsiveFontSize(responsive)
  const messagesEndRef = useRef<HTMLDivElement>(null)
  const inputRef = useRef<any>(null)

  // Handle scroll to show/hide scroll-to-top button
  useEffect(() => {
    const handleScroll = () => {
      setShowScrollTop(window.scrollY > 300)
    }
    
    window.addEventListener('scroll', handleScroll)
    return () => window.removeEventListener('scroll', handleScroll)
  }, [])

  // Auto-scroll to bottom on new messages
  useEffect(() => {
    if (messagesEndRef.current) {
      messagesEndRef.current.scrollIntoView({ behavior: 'smooth' })
    }
  }, [messages.length])

  const handleSend = () => {
    if (message.trim()) {
      onSendMessage(message.trim())
      setMessage('')
      inputRef.current?.focus()
    }
  }

  const handleKeyPress = (e: React.KeyboardEvent) => {
    if (e.key === 'Enter' && !e.shiftKey) {
      e.preventDefault()
      handleSend()
    }
  }

  const scrollToTop = () => {
    window.scrollTo({ top: 0, behavior: 'smooth' })
  }

  const quickActions = [
    { icon: <HistoryOutlined />, label: 'History', action: () => setShowSidebar(true) },
    { icon: <SettingOutlined />, label: 'Settings', action: () => {} },
  ]

  const mobileStyles = {
    container: {
      minHeight: '100vh',
      background: responsive.isMobile 
        ? 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)'
        : 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)',
      position: 'relative' as const,
      paddingBottom: inputFocused && responsive.isMobile ? '0' : '80px'
    },
    
    header: {
      position: 'sticky' as const,
      top: 0,
      zIndex: 100,
      background: 'rgba(255, 255, 255, 0.95)',
      backdropFilter: 'blur(20px)',
      borderBottom: '1px solid rgba(255, 255, 255, 0.2)',
      padding: spacing.padding,
      display: 'flex',
      justifyContent: 'space-between',
      alignItems: 'center'
    },
    
    messagesContainer: {
      flex: 1,
      padding: spacing.padding,
      paddingBottom: responsive.isMobile ? '100px' : spacing.padding
    },
    
    inputContainer: {
      position: responsive.isMobile ? 'fixed' as const : 'sticky' as const,
      bottom: 0,
      left: 0,
      right: 0,
      background: 'rgba(255, 255, 255, 0.95)',
      backdropFilter: 'blur(20px)',
      borderTop: '1px solid rgba(255, 255, 255, 0.2)',
      padding: spacing.padding,
      zIndex: 200,
      // Handle iOS keyboard
      paddingBottom: responsive.isMobile && inputFocused ? 'env(keyboard-inset-height, 0px)' : spacing.padding
    }
  }

  return (
    <div style={mobileStyles.container}>
      {/* Mobile Header */}
      <div style={mobileStyles.header}>
        <Space>
          <Button
            type="text"
            icon={<MenuOutlined />}
            onClick={() => setShowSidebar(true)}
            style={{ fontSize: fontSize.large }}
          />
          <div>
            <Title level={4} style={{ margin: 0, fontSize: fontSize.title }}>
              ReportAI
            </Title>
            <ConnectionStatus />
          </div>
        </Space>
        
        <Space>
          <Badge count={messages.length} size="small">
            <Button
              type="text"
              icon={<HistoryOutlined />}
              onClick={() => setShowSidebar(true)}
            />
          </Badge>
        </Space>
      </div>

      {/* Welcome State for Empty Chat */}
      {messages.length === 0 && !isLoading && (
        <div style={{
          display: 'flex',
          flexDirection: 'column',
          justifyContent: 'center',
          alignItems: 'center',
          minHeight: 'calc(100vh - 200px)',
          padding: spacing.padding,
          textAlign: 'center'
        }}>
          <div style={{
            fontSize: responsive.isMobile ? '60px' : '80px',
            marginBottom: spacing.margin,
            animation: 'bounce 2s ease-in-out infinite'
          }}>
            🤖
          </div>
          
          <Title 
            level={responsive.isMobile ? 3 : 2} 
            style={{ 
              color: 'white',
              marginBottom: spacing.margin,
              fontSize: fontSize.heading
            }}
          >
            Ask me anything!
          </Title>
          
          <Text style={{
            color: 'rgba(255, 255, 255, 0.8)',
            fontSize: fontSize.normal,
            maxWidth: '300px',
            lineHeight: 1.6
          }}>
            I can help you explore your data, create reports, and answer questions about your business.
          </Text>

          {/* Quick suggestion cards for mobile */}
          {responsive.isMobile && (
            <div style={{
              display: 'grid',
              gridTemplateColumns: '1fr',
              gap: spacing.gap,
              width: '100%',
              maxWidth: '320px',
              marginTop: '32px'
            }}>
              {[
                "Show revenue trends",
                "Top customers",
                "Monthly reports"
              ].map((suggestion, index) => (
                <Card
                  key={index}
                  size="small"
                  hoverable
                  onClick={() => {
                    setMessage(suggestion)
                    setTimeout(() => inputRef.current?.focus(), 100)
                  }}
                  style={{
                    background: 'rgba(255, 255, 255, 0.1)',
                    backdropFilter: 'blur(10px)',
                    border: '1px solid rgba(255, 255, 255, 0.2)',
                    borderRadius: '12px'
                  }}
                  bodyStyle={{ padding: '12px' }}
                >
                  <Text style={{ color: 'white', fontSize: fontSize.normal }}>
                    {suggestion}
                  </Text>
                </Card>
              ))}
            </div>
          )}
        </div>
      )}

      {/* Messages */}
      {messages.length > 0 && (
        <div style={mobileStyles.messagesContainer}>
          <MessageList
            messages={messages}
            isLoading={isLoading}
            onRerun={onSendMessage}
          />
          <div ref={messagesEndRef} />
        </div>
      )}

      {/* Streaming Progress */}
      {streamingProgress && (
        <div style={{ padding: spacing.padding }}>
          <StreamingProgress compact={responsive.isMobile} />
        </div>
      )}

      {/* Input Area */}
      <div style={mobileStyles.inputContainer}>
        <Space.Compact style={{ width: '100%' }}>
          <TextArea
            ref={inputRef}
            value={message}
            onChange={(e) => setMessage(e.target.value)}
            onKeyPress={handleKeyPress}
            onFocus={() => setInputFocused(true)}
            onBlur={() => setInputFocused(false)}
            placeholder="Type your question..."
            autoSize={{ 
              minRows: responsive.isMobile ? 1 : 2, 
              maxRows: responsive.isMobile ? 3 : 4 
            }}
            style={{
              fontSize: fontSize.normal,
              borderRadius: '12px',
              border: '1px solid #d9d9d9'
            }}
            disabled={isLoading}
          />
          
          <Space direction="vertical" size="small">
            <Tooltip title="Voice input">
              <Button
                icon={<MicrophoneOutlined />}
                onClick={() => setIsRecording(!isRecording)}
                style={{
                  height: '40px',
                  borderRadius: '12px',
                  background: isRecording ? '#ff4d4f' : undefined,
                  color: isRecording ? 'white' : undefined
                }}
              />
            </Tooltip>
            
            <Button
              type="primary"
              icon={<SendOutlined />}
              onClick={handleSend}
              loading={isLoading}
              disabled={!message.trim()}
              style={{
                height: '40px',
                borderRadius: '12px',
                background: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)',
                border: 'none'
              }}
            />
          </Space>
        </Space.Compact>
      </div>

      {/* Floating Action Buttons */}
      <FloatButton.Group
        trigger="click"
        type="primary"
        style={{ right: responsive.isMobile ? 16 : 24 }}
        icon={<PlusOutlined />}
      >
        {quickActions.map((action, index) => (
          <FloatButton
            key={index}
            icon={action.icon}
            tooltip={action.label}
            onClick={action.action}
          />
        ))}
      </FloatButton.Group>

      {/* Scroll to Top */}
      {showScrollTop && (
        <Affix offsetBottom={responsive.isMobile ? 100 : 24}>
          <FloatButton
            icon={<ArrowUpOutlined />}
            onClick={scrollToTop}
            style={{ 
              right: responsive.isMobile ? 16 : 80,
              bottom: responsive.isMobile ? 100 : 24
            }}
          />
        </Affix>
      )}

      {/* Mobile Sidebar */}
      <Drawer
        title="Menu"
        placement="left"
        onClose={() => setShowSidebar(false)}
        open={showSidebar}
        width={responsive.isMobile ? '80%' : 400}
        extra={
          <Button
            type="text"
            icon={<CloseOutlined />}
            onClick={() => setShowSidebar(false)}
          />
        }
      >
        <Space direction="vertical" style={{ width: '100%' }}>
          <Card size="small" title="Recent Conversations">
            <Text type="secondary">No recent conversations</Text>
          </Card>
          
          <Card size="small" title="Quick Actions">
            <Space direction="vertical" style={{ width: '100%' }}>
              <Button block icon={<HistoryOutlined />}>
                View History
              </Button>
              <Button block icon={<SettingOutlined />}>
                Settings
              </Button>
            </Space>
          </Card>
        </Space>
      </Drawer>

      {/* CSS Animations */}
      <style jsx>{`
        @keyframes bounce {
          0%, 20%, 50%, 80%, 100% {
            transform: translateY(0);
          }
          40% {
            transform: translateY(-10px);
          }
          60% {
            transform: translateY(-5px);
          }
        }
        
        /* iOS Safari specific fixes */
        @supports (-webkit-touch-callout: none) {
          .input-container {
            padding-bottom: env(safe-area-inset-bottom);
          }
        }
      `}</style>
    </div>
  )
}
