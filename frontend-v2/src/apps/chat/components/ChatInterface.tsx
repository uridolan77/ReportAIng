import React, { useState, useRef, useEffect } from 'react'
import { Button, Input, Typography, Card, Space, Spin, message } from 'antd'
import {
  SendOutlined,
  StarOutlined,
  BulbOutlined,
  LoadingOutlined,
  HistoryOutlined
} from '@ant-design/icons'
import { MessageList } from './MessageList'
import { ChatInput } from './ChatInput'
import { StreamingProgress } from './StreamingProgress'
import { ConnectionStatus } from './ConnectionStatus'
import { useAppSelector, useAppDispatch } from '@shared/hooks'
import { 
  selectMessages, 
  selectCurrentConversation, 
  selectIsLoading,
  selectStreamingProgress,
  chatActions 
} from '@shared/store/chat'
import { useSendMessageMutation, useExecuteEnhancedQueryMutation } from '@shared/store/api/chatApi'
import { socketService } from '@shared/services/socketService'

const { Title, Text } = Typography

interface ChatInterfaceProps {
  conversationId?: string
  showHeader?: boolean
  showExamples?: boolean
  className?: string
}

export const ChatInterface: React.FC<ChatInterfaceProps> = ({
  conversationId,
  showHeader = true,
  showExamples = true,
  className = ''
}) => {
  const dispatch = useAppDispatch()
  const messages = useAppSelector(selectMessages)
  const currentConversation = useAppSelector(selectCurrentConversation)
  const isLoading = useAppSelector(selectIsLoading)
  const streamingProgress = useAppSelector(selectStreamingProgress)
  
  const [sendMessage] = useSendMessageMutation()
  const [executeEnhancedQuery] = useExecuteEnhancedQueryMutation()
  const [isFirstVisit, setIsFirstVisit] = useState(false)

  // Example queries for first-time users
  const exampleQueries = [
    "Show me the top 10 players by total deposits this month",
    "What's the average session duration for active players?", 
    "Compare revenue between different countries last quarter",
    "Which games have the highest player retention rate?"
  ]

  // Check if this is user's first visit
  useEffect(() => {
    const hasVisited = localStorage.getItem('has-visited-chat')
    if (!hasVisited) {
      setIsFirstVisit(true)
      localStorage.setItem('has-visited-chat', 'true')
    }
  }, [])

  // Initialize socket connection
  useEffect(() => {
    const initializeSocket = async () => {
      try {
        if (!socketService.isConnected()) {
          await socketService.connect()
        }
        
        if (conversationId) {
          socketService.joinConversation(conversationId)
        }
      } catch (error) {
        console.error('Failed to initialize socket:', error)
      }
    }

    initializeSocket()

    return () => {
      if (conversationId) {
        socketService.leaveConversation(conversationId)
      }
    }
  }, [conversationId])

  const handleSendMessage = async (content: string) => {
    if (!content.trim()) return

    try {
      // Create optimistic message
      const tempMessage = {
        id: `temp-${Date.now()}`,
        content,
        type: 'user' as const,
        timestamp: new Date().toISOString(),
        status: 'sending' as const
      }

      dispatch(chatActions.addMessage(tempMessage))

      // Execute enhanced query with streaming support
      const result = await executeEnhancedQuery({
        query: content,
        conversationId: conversationId || currentConversation?.id,
        options: {
          includeSemanticAnalysis: true,
          enableStreaming: true,
          maxResults: 100
        }
      }).unwrap()

      // Update with real message
      dispatch(chatActions.updateMessage({
        id: tempMessage.id,
        updates: {
          id: result.message.id,
          status: 'delivered'
        }
      }))

      // Add the AI response message
      if (result.message.type === 'assistant') {
        dispatch(chatActions.addMessage(result.message))
      }

      // Join query group to receive real-time updates if sessionId is provided
      if (result.sessionId) {
        console.log('🔗 Joining query group for real-time updates:', result.sessionId)
        socketService.joinQuerySession(result.sessionId)
      } else {
        console.log('ℹ️ No sessionId provided - streaming not available for this query')
      }

    } catch (error: any) {
      console.error('Failed to send message:', error)
      message.error('Failed to send message')
      
      // Update message with error status
      dispatch(chatActions.updateMessage({
        id: `temp-${Date.now()}`,
        updates: {
          status: 'error',
          error: {
            code: 'SEND_FAILED',
            message: error.message || 'Failed to send message'
          }
        }
      }))
    }
  }

  const handleExampleClick = (example: string) => {
    handleSendMessage(example)
  }

  const handleCancelStreaming = () => {
    if (streamingProgress?.sessionId) {
      socketService.cancelStreamingQuery(streamingProgress.sessionId)
    }
  }

  const showWelcomeState = messages.length === 0 && !isLoading

  return (
    <div className={`chat-interface ${className}`} style={{
      display: 'flex',
      flexDirection: 'column',
      height: '100%',
      maxHeight: '100vh',
      background: 'transparent'
    }}>
      {/* Header */}
      {showHeader && (
        <div style={{
          textAlign: 'center',
          padding: '32px 20px 24px',
          borderBottom: showWelcomeState ? 'none' : '1px solid #f0f0f0'
        }}>
          <div style={{
            display: 'inline-flex',
            alignItems: 'center',
            gap: '16px',
            marginBottom: '16px'
          }}>
            <div style={{
              width: '48px',
              height: '48px',
              borderRadius: '12px',
              background: 'linear-gradient(135deg, #3b82f6 0%, #2563eb 100%)',
              display: 'flex',
              alignItems: 'center',
              justifyContent: 'center',
              boxShadow: '0 8px 24px rgba(59, 130, 246, 0.3)'
            }}>
              <StarOutlined style={{ fontSize: '24px', color: 'white' }} />
            </div>
            <Title level={2} style={{
              margin: 0,
              fontSize: '28px',
              fontWeight: 700,
              background: 'linear-gradient(135deg, #3b82f6 0%, #2563eb 100%)',
              WebkitBackgroundClip: 'text',
              WebkitTextFillColor: 'transparent',
              backgroundClip: 'text'
            }}>
              Talk with DailyActionsDB
            </Title>
          </div>
          
          <Text style={{
            fontSize: '16px',
            color: '#64748b',
            fontWeight: 400,
            maxWidth: '500px',
            display: 'block',
            margin: '0 auto'
          }}>
            Ask questions about your data in natural language and get instant insights
          </Text>

          {/* Connection Status */}
          <div style={{ marginTop: '16px' }}>
            <ConnectionStatus />
          </div>
        </div>
      )}

      {/* Welcome State with Examples */}
      {showWelcomeState && showExamples && (
        <div style={{
          flex: 1,
          display: 'flex',
          flexDirection: 'column',
          justifyContent: 'center',
          padding: '40px 20px'
        }}>
          <div style={{
            textAlign: 'center',
            marginBottom: '32px'
          }}>
            <div style={{ fontSize: '48px', marginBottom: '16px' }}>🚀</div>
            <Text style={{
              fontSize: '18px',
              color: '#374151',
              fontWeight: 600,
              display: 'block',
              marginBottom: '8px'
            }}>
              Ready to Explore Your Data
            </Text>
            <Text style={{
              fontSize: '14px',
              color: '#6b7280',
              display: 'block',
              marginBottom: '24px'
            }}>
              Try these example questions:
            </Text>
          </div>

          <div style={{
            display: 'grid',
            gridTemplateColumns: 'repeat(auto-fit, minmax(280px, 1fr))',
            gap: '16px',
            maxWidth: '800px',
            margin: '0 auto 32px'
          }}>
            {exampleQueries.map((example, index) => (
              <Card
                key={index}
                hoverable
                onClick={() => handleExampleClick(example)}
                style={{
                  cursor: 'pointer',
                  borderRadius: '12px',
                  border: '1px solid #e5e7eb',
                  transition: 'all 0.2s ease'
                }}
                bodyStyle={{ padding: '16px' }}
              >
                <div style={{ display: 'flex', alignItems: 'flex-start', gap: '12px' }}>
                  <BulbOutlined style={{
                    color: '#3b82f6',
                    fontSize: '16px',
                    marginTop: '2px',
                    flexShrink: 0
                  }} />
                  <Text style={{
                    fontSize: '14px',
                    lineHeight: '1.5',
                    color: '#374151'
                  }}>
                    {example}
                  </Text>
                </div>
              </Card>
            ))}
          </div>
        </div>
      )}

      {/* Messages Area */}
      {!showWelcomeState && (
        <div style={{
          flex: 1,
          overflow: 'hidden',
          display: 'flex',
          flexDirection: 'column'
        }}>
          <MessageList
            messages={messages}
            isLoading={isLoading}
            onRerun={handleSendMessage}
          />
        </div>
      )}

      {/* Streaming Progress */}
      {streamingProgress && (
        <div style={{ padding: '16px 20px 0' }}>
          <StreamingProgress
            onCancel={handleCancelStreaming}
            compact={!showWelcomeState}
          />
        </div>
      )}

      {/* Chat Input */}
      <div style={{
        padding: '16px 20px 20px',
        borderTop: !showWelcomeState ? '1px solid #f0f0f0' : 'none'
      }}>
        <ChatInput
          onSend={handleSendMessage}
          disabled={isLoading}
          placeholder="Ask me anything about your data..."
        />
      </div>
    </div>
  )
}
