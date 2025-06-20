import React, { useEffect, useRef } from 'react'
import { List, Typography, Empty, Spin } from 'antd'
import { MessageItem } from './MessageItem'
import { TypingIndicator } from './TypingIndicator'
import { useAppSelector } from '@shared/hooks'
import { selectIsTyping } from '@shared/store/chat'
import type { ChatMessage } from '@shared/types/chat'

const { Text } = Typography

interface MessageListProps {
  messages: ChatMessage[]
  isLoading?: boolean
  showMetadata?: boolean
  onRerun?: (query: string) => void
  onEdit?: (messageId: string) => void
  onDelete?: (messageId: string) => void
  className?: string
}

export const MessageList: React.FC<MessageListProps> = ({
  messages,
  isLoading = false,
  showMetadata = false,
  onRerun,
  onEdit,
  onDelete,
  className = ''
}) => {
  const messagesEndRef = useRef<HTMLDivElement>(null)
  const isTyping = useAppSelector(selectIsTyping)

  // Auto-scroll to bottom when new messages arrive
  useEffect(() => {
    if (messagesEndRef.current) {
      messagesEndRef.current.scrollIntoView({ 
        behavior: 'smooth',
        block: 'end'
      })
    }
  }, [messages.length, isLoading, isTyping])

  // Group messages by date for better organization
  const groupMessagesByDate = (messages: ChatMessage[]) => {
    const groups: { [key: string]: ChatMessage[] } = {}
    
    messages.forEach(message => {
      const date = new Date(message.timestamp).toDateString()
      if (!groups[date]) {
        groups[date] = []
      }
      groups[date].push(message)
    })
    
    return groups
  }

  const messageGroups = groupMessagesByDate(messages)
  const today = new Date().toDateString()
  const yesterday = new Date(Date.now() - 24 * 60 * 60 * 1000).toDateString()

  const formatDateHeader = (dateString: string) => {
    if (dateString === today) return 'Today'
    if (dateString === yesterday) return 'Yesterday'
    return new Date(dateString).toLocaleDateString('en-US', {
      weekday: 'long',
      year: 'numeric',
      month: 'long',
      day: 'numeric'
    })
  }

  if (messages.length === 0 && !isLoading) {
    return (
      <div className={`message-list-empty ${className}`} style={{
        display: 'flex',
        alignItems: 'center',
        justifyContent: 'center',
        height: '100%',
        padding: '40px 20px'
      }}>
        <Empty
          image={Empty.PRESENTED_IMAGE_SIMPLE}
          description={
            <Text type="secondary" style={{ fontSize: '16px' }}>
              Start a conversation by asking a question about your data
            </Text>
          }
        />
      </div>
    )
  }

  return (
    <div className={`message-list ${className}`} style={{
      height: '100%',
      overflow: 'auto',
      padding: '16px 20px',
      display: 'flex',
      flexDirection: 'column'
    }}>
      {/* Message Groups by Date */}
      {Object.entries(messageGroups).map(([date, groupMessages]) => (
        <div key={date} style={{ marginBottom: '24px' }}>
          {/* Date Header */}
          <div style={{
            textAlign: 'center',
            margin: '16px 0',
            position: 'relative'
          }}>
            <div style={{
              display: 'inline-block',
              background: '#f5f5f5',
              padding: '4px 12px',
              borderRadius: '12px',
              fontSize: '12px',
              color: '#666',
              fontWeight: 500
            }}>
              {formatDateHeader(date)}
            </div>
          </div>

          {/* Messages for this date */}
          <div style={{ display: 'flex', flexDirection: 'column', gap: '16px' }}>
            {groupMessages.map((message) => (
              <MessageItem
                key={message.id}
                message={message}
                showMetadata={showMetadata}
                onRerun={onRerun}
                onEdit={onEdit}
                onDelete={onDelete}
              />
            ))}
          </div>
        </div>
      ))}

      {/* Typing Indicator */}
      {isTyping && (
        <div style={{ marginTop: '16px' }}>
          <TypingIndicator />
        </div>
      )}

      {/* Loading Indicator */}
      {isLoading && (
        <div style={{
          display: 'flex',
          justifyContent: 'center',
          alignItems: 'center',
          padding: '20px',
          marginTop: '16px'
        }}>
          <Spin size="large" />
          <Text style={{ marginLeft: '12px', color: '#666' }}>
            Processing your request...
          </Text>
        </div>
      )}

      {/* Scroll anchor */}
      <div ref={messagesEndRef} />
    </div>
  )
}

// Typing Indicator Component
const TypingIndicator: React.FC = () => {
  return (
    <div style={{
      display: 'flex',
      alignItems: 'flex-start',
      gap: '12px',
      maxWidth: '85%'
    }}>
      <div style={{
        width: '32px',
        height: '32px',
        borderRadius: '50%',
        background: '#722ed1',
        display: 'flex',
        alignItems: 'center',
        justifyContent: 'center',
        color: 'white',
        fontSize: '14px'
      }}>
        🤖
      </div>
      
      <div style={{
        background: '#f9f0ff',
        border: '1px solid #d3adf7',
        borderRadius: '12px',
        padding: '12px 16px',
        minWidth: '60px'
      }}>
        <div style={{
          display: 'flex',
          gap: '4px',
          alignItems: 'center'
        }}>
          <div className="typing-dot" style={{
            width: '6px',
            height: '6px',
            borderRadius: '50%',
            background: '#722ed1',
            animation: 'typing 1.4s infinite ease-in-out'
          }} />
          <div className="typing-dot" style={{
            width: '6px',
            height: '6px',
            borderRadius: '50%',
            background: '#722ed1',
            animation: 'typing 1.4s infinite ease-in-out 0.2s'
          }} />
          <div className="typing-dot" style={{
            width: '6px',
            height: '6px',
            borderRadius: '50%',
            background: '#722ed1',
            animation: 'typing 1.4s infinite ease-in-out 0.4s'
          }} />
        </div>
      </div>

      <style jsx>{`
        @keyframes typing {
          0%, 60%, 100% {
            transform: translateY(0);
            opacity: 0.5;
          }
          30% {
            transform: translateY(-10px);
            opacity: 1;
          }
        }
      `}</style>
    </div>
  )
}
