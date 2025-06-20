import React, { useState, useRef, useEffect } from 'react'
import { Card, Input, Button, Space, Typography, List, Avatar, Spin, Alert } from 'antd'
import { SendOutlined, UserOutlined, RobotOutlined, HistoryOutlined, ApiOutlined } from '@ant-design/icons'
import { PageLayout } from '@shared/components/core/Layout'
import { SqlEditor, Chart } from '@shared/components/core'
import { useExecuteEnhancedQueryMutation } from '@shared/store/api/queryApi'
import { useEnhancedQueryExecution } from '@shared/hooks/useEnhancedApi'
import { useApiToggle } from '@shared/services/apiToggleService'
import { useSocket } from '@shared/services/socketService'

const { Text, Title } = Typography
const { TextArea } = Input

interface Message {
  id: string
  type: 'user' | 'assistant' | 'system'
  content: string
  timestamp: Date
  sql?: string
  results?: any[]
  metadata?: any
}

export default function ChatInterface() {
  const [messages, setMessages] = useState<Message[]>([
    {
      id: '1',
      type: 'system',
      content: 'Welcome to BI Reporting Copilot! Ask me anything about your data using natural language.',
      timestamp: new Date(),
    },
  ])
  const [inputValue, setInputValue] = useState('')
  const [isLoading, setIsLoading] = useState(false)
  const messagesEndRef = useRef<HTMLDivElement>(null)

  const [executeQuery, { isLoading: isQueryLoading }] = useExecuteEnhancedQueryMutation()
  const { mutate: executeEnhancedQuery } = useEnhancedQueryExecution()
  const { isUsingMockData, toggleMockData } = useApiToggle()
  const socket = useSocket()

  useEffect(() => {
    messagesEndRef.current?.scrollIntoView({ behavior: 'smooth' })
  }, [messages])

  useEffect(() => {
    // Connect to socket for real-time updates
    socket.connect()

    return () => {
      socket.disconnect()
    }
  }, [socket])

  const handleSendMessage = async () => {
    if (!inputValue.trim() || isLoading) return

    const userMessage: Message = {
      id: Date.now().toString(),
      type: 'user',
      content: inputValue,
      timestamp: new Date(),
    }

    setMessages(prev => [...prev, userMessage])
    setInputValue('')
    setIsLoading(true)

    try {
      const result = await executeQuery({
        query: inputValue,
        includeExplanation: true,
        optimizeForPerformance: true,
      }).unwrap()

      const assistantMessage: Message = {
        id: (Date.now() + 1).toString(),
        type: 'assistant',
        content: result.explanation || 'Query executed successfully',
        timestamp: new Date(),
        sql: result.sql,
        results: result.results,
        metadata: result.metadata,
      }

      setMessages(prev => [...prev, assistantMessage])
    } catch (error: any) {
      const errorMessage: Message = {
        id: (Date.now() + 1).toString(),
        type: 'assistant',
        content: `Sorry, I encountered an error: ${error.data?.message || error.message || 'Unknown error'}`,
        timestamp: new Date(),
      }

      setMessages(prev => [...prev, errorMessage])
    } finally {
      setIsLoading(false)
    }
  }

  const handleKeyPress = (e: React.KeyboardEvent) => {
    if (e.key === 'Enter' && !e.shiftKey) {
      e.preventDefault()
      handleSendMessage()
    }
  }

  const renderMessage = (message: Message) => {
    const isUser = message.type === 'user'
    const isSystem = message.type === 'system'

    return (
      <div
        key={message.id}
        style={{
          display: 'flex',
          justifyContent: isUser ? 'flex-end' : 'flex-start',
          marginBottom: 16,
        }}
      >
        <div
          style={{
            maxWidth: '70%',
            display: 'flex',
            flexDirection: isUser ? 'row-reverse' : 'row',
            alignItems: 'flex-start',
            gap: 8,
          }}
        >
          <Avatar
            icon={isUser ? <UserOutlined /> : <RobotOutlined />}
            style={{
              backgroundColor: isUser ? '#1890ff' : isSystem ? '#52c41a' : '#722ed1',
            }}
          />
          <div
            style={{
              backgroundColor: isUser ? '#e6f7ff' : isSystem ? '#f6ffed' : '#f9f0ff',
              padding: '12px 16px',
              borderRadius: 12,
              border: `1px solid ${isUser ? '#91d5ff' : isSystem ? '#b7eb8f' : '#d3adf7'}`,
            }}
          >
            <Text>{message.content}</Text>
            <div style={{ marginTop: 4, fontSize: '12px', color: '#8c8c8c' }}>
              {message.timestamp.toLocaleTimeString()}
            </div>

            {/* Render SQL if available */}
            {message.sql && (
              <div style={{ marginTop: 12 }}>
                <SqlEditor
                  value={message.sql}
                  height={150}
                  readOnly
                  showExecuteButton={false}
                  showFormatButton={false}
                />
              </div>
            )}

            {/* Render results chart if available */}
            {message.results && message.results.length > 0 && (
              <div style={{ marginTop: 12 }}>
                <Chart
                  data={message.results}
                  columns={Object.keys(message.results[0] || {})}
                  height={300}
                />
              </div>
            )}

            {/* Render metadata if available */}
            {message.metadata && (
              <div style={{ marginTop: 8, fontSize: '12px', color: '#8c8c8c' }}>
                <Space split={<span>•</span>}>
                  <span>Execution time: {message.metadata.executionTime}ms</span>
                  <span>Rows: {message.metadata.rowCount}</span>
                  <span>Columns: {message.metadata.columnCount}</span>
                </Space>
              </div>
            )}
          </div>
        </div>
      </div>
    )
  }

  return (
    <PageLayout
      title="Chat Interface"
      subtitle="Ask questions about your data in natural language"
      extra={
        <Space>
          <Button
            icon={<ApiOutlined />}
            onClick={toggleMockData}
            type={isUsingMockData ? 'default' : 'primary'}
            size="small"
          >
            {isUsingMockData ? 'Mock Data' : 'Real API'}
          </Button>
          <Button
            icon={<HistoryOutlined />}
            onClick={() => window.open('/chat/history', '_blank')}
          >
            Query History
          </Button>
        </Space>
      }
    >
      {/* API Status Alert */}
      {isUsingMockData && (
        <Alert
          message="Development Mode"
          description="Using mock data for demonstration. Real AI query generation available when backend is connected."
          type="info"
          showIcon
          style={{ marginBottom: 16 }}
          action={
            <Button size="small" onClick={toggleMockData}>
              Switch to Real API
            </Button>
          }
        />
      )}

      <div style={{ display: 'flex', flexDirection: 'column', height: 'calc(100vh - 200px)' }}>
        {/* Messages Area */}
        <Card
          style={{
            flex: 1,
            marginBottom: 16,
            overflow: 'hidden',
          }}
          bodyStyle={{
            height: '100%',
            overflow: 'auto',
            padding: '16px',
          }}
        >
          <div>
            {messages.map(renderMessage)}
            {isLoading && (
              <div style={{ display: 'flex', justifyContent: 'flex-start', marginBottom: 16 }}>
                <div style={{ display: 'flex', alignItems: 'flex-start', gap: 8 }}>
                  <Avatar icon={<RobotOutlined />} style={{ backgroundColor: '#722ed1' }} />
                  <div
                    style={{
                      backgroundColor: '#f9f0ff',
                      padding: '12px 16px',
                      borderRadius: 12,
                      border: '1px solid #d3adf7',
                    }}
                  >
                    <Spin size="small" />
                    <Text style={{ marginLeft: 8 }}>Thinking...</Text>
                  </div>
                </div>
              </div>
            )}
            <div ref={messagesEndRef} />
          </div>
        </Card>

        {/* Input Area */}
        <Card size="small">
          <Space.Compact style={{ width: '100%' }}>
            <TextArea
              value={inputValue}
              onChange={(e) => setInputValue(e.target.value)}
              onKeyPress={handleKeyPress}
              placeholder="Ask me anything about your data... (Press Enter to send, Shift+Enter for new line)"
              autoSize={{ minRows: 1, maxRows: 4 }}
              disabled={isLoading}
            />
            <Button
              type="primary"
              icon={<SendOutlined />}
              onClick={handleSendMessage}
              loading={isLoading}
              disabled={!inputValue.trim()}
            >
              Send
            </Button>
          </Space.Compact>
        </Card>
      </div>
    </PageLayout>
  )
}
