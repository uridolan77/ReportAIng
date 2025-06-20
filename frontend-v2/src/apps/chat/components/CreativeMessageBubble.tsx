import React, { useState } from 'react'
import { Typography, Space, Button, Tag, Tooltip, Card } from 'antd'
import {
  UserOutlined,
  RobotOutlined,
  CopyOutlined,
  ShareAltOutlined,
  StarOutlined,
  StarFilled,
  PlayCircleOutlined,
  MoreOutlined,
  ThunderboltOutlined,
  CheckCircleOutlined,
  ClockCircleOutlined
} from '@ant-design/icons'
import { formatDistanceToNow } from 'date-fns'
import type { ChatMessage } from '@shared/types/chat'

const { Text, Paragraph } = Typography

interface CreativeMessageBubbleProps {
  message: ChatMessage
  onRerun?: (query: string) => void
  onCopy?: () => void
  onShare?: () => void
  onToggleFavorite?: () => void
}

export const CreativeMessageBubble: React.FC<CreativeMessageBubbleProps> = ({
  message,
  onRerun,
  onCopy,
  onShare,
  onToggleFavorite
}) => {
  const [isHovered, setIsHovered] = useState(false)
  const isUser = message.type === 'user'
  const isAssistant = message.type === 'assistant'

  const getBubbleStyle = () => {
    if (isUser) {
      return {
        background: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)',
        color: 'white',
        borderRadius: '24px 24px 8px 24px',
        marginLeft: 'auto',
        maxWidth: '70%',
        boxShadow: '0 8px 24px rgba(102, 126, 234, 0.3)',
        transform: isHovered ? 'translateY(-2px)' : 'translateY(0)',
        transition: 'all 0.3s cubic-bezier(0.4, 0, 0.2, 1)'
      }
    }

    return {
      background: 'linear-gradient(135deg, #ffffff 0%, #f8fafc 100%)',
      color: '#1f2937',
      borderRadius: '24px 24px 24px 8px',
      marginRight: 'auto',
      maxWidth: '85%',
      border: '1px solid #e5e7eb',
      boxShadow: '0 8px 24px rgba(0, 0, 0, 0.08)',
      transform: isHovered ? 'translateY(-2px)' : 'translateY(0)',
      transition: 'all 0.3s cubic-bezier(0.4, 0, 0.2, 1)'
    }
  }

  const getAvatarStyle = () => {
    if (isUser) {
      return {
        background: 'linear-gradient(135deg, #10b981 0%, #059669 100%)',
        border: '3px solid white',
        boxShadow: '0 4px 12px rgba(16, 185, 129, 0.3)'
      }
    }

    return {
      background: 'linear-gradient(135deg, #f59e0b 0%, #d97706 100%)',
      border: '3px solid white',
      boxShadow: '0 4px 12px rgba(245, 158, 11, 0.3)'
    }
  }

  const renderStatusIndicator = () => {
    switch (message.status) {
      case 'sending':
        return (
          <div style={{
            display: 'flex',
            alignItems: 'center',
            gap: '4px',
            marginTop: '8px'
          }}>
            <div className="typing-indicator">
              <span></span>
              <span></span>
              <span></span>
            </div>
            <Text type="secondary" style={{ fontSize: '12px' }}>
              Sending...
            </Text>
          </div>
        )
      case 'delivered':
        return (
          <CheckCircleOutlined 
            style={{ 
              color: '#10b981', 
              fontSize: '12px',
              marginTop: '4px'
            }} 
          />
        )
      case 'error':
        return (
          <Tag color="red" style={{ marginTop: '4px' }}>
            Failed
          </Tag>
        )
      default:
        return null
    }
  }

  const renderActions = () => {
    if (!isHovered) return null

    return (
      <div style={{
        position: 'absolute',
        top: '-20px',
        right: isUser ? 'auto' : '0',
        left: isUser ? '0' : 'auto',
        background: 'rgba(255, 255, 255, 0.95)',
        backdropFilter: 'blur(10px)',
        borderRadius: '12px',
        padding: '4px',
        border: '1px solid rgba(0, 0, 0, 0.1)',
        boxShadow: '0 4px 12px rgba(0, 0, 0, 0.15)',
        animation: 'fadeInScale 0.2s ease-out'
      }}>
        <Space size="small">
          <Tooltip title="Copy">
            <Button
              type="text"
              size="small"
              icon={<CopyOutlined />}
              onClick={onCopy}
              style={{ width: '28px', height: '28px' }}
            />
          </Tooltip>
          
          {message.isFavorite ? (
            <Tooltip title="Remove from favorites">
              <Button
                type="text"
                size="small"
                icon={<StarFilled style={{ color: '#faad14' }} />}
                onClick={onToggleFavorite}
                style={{ width: '28px', height: '28px' }}
              />
            </Tooltip>
          ) : (
            <Tooltip title="Add to favorites">
              <Button
                type="text"
                size="small"
                icon={<StarOutlined />}
                onClick={onToggleFavorite}
                style={{ width: '28px', height: '28px' }}
              />
            </Tooltip>
          )}
          
          {(isUser || message.sql) && onRerun && (
            <Tooltip title={isUser ? "Ask again" : "Re-run query"}>
              <Button
                type="text"
                size="small"
                icon={<PlayCircleOutlined />}
                onClick={() => onRerun(message.sql || message.content)}
                style={{ width: '28px', height: '28px' }}
              />
            </Tooltip>
          )}
          
          <Tooltip title="Share">
            <Button
              type="text"
              size="small"
              icon={<ShareAltOutlined />}
              onClick={onShare}
              style={{ width: '28px', height: '28px' }}
            />
          </Tooltip>
        </Space>
      </div>
    )
  }

  const renderMetadata = () => {
    if (!message.resultMetadata) return null

    const { executionTime, rowCount, queryComplexity } = message.resultMetadata

    return (
      <div style={{
        marginTop: '12px',
        padding: '8px 12px',
        background: 'rgba(0, 0, 0, 0.05)',
        borderRadius: '8px',
        fontSize: '12px'
      }}>
        <Space size="small">
          <Tag color="blue">
            <ClockCircleOutlined /> {executionTime}ms
          </Tag>
          <Tag color="green">
            {rowCount} rows
          </Tag>
          <Tag color={queryComplexity === 'Simple' ? 'green' : queryComplexity === 'Medium' ? 'orange' : 'red'}>
            {queryComplexity}
          </Tag>
        </Space>
      </div>
    )
  }

  return (
    <div style={{
      display: 'flex',
      flexDirection: isUser ? 'row-reverse' : 'row',
      alignItems: 'flex-start',
      gap: '12px',
      marginBottom: '24px',
      position: 'relative'
    }}>
      {/* Avatar */}
      <div style={{
        width: '40px',
        height: '40px',
        borderRadius: '50%',
        display: 'flex',
        alignItems: 'center',
        justifyContent: 'center',
        fontSize: '18px',
        flexShrink: 0,
        ...getAvatarStyle()
      }}>
        {isUser ? (
          <UserOutlined style={{ color: 'white' }} />
        ) : (
          <RobotOutlined style={{ color: 'white' }} />
        )}
      </div>

      {/* Message Bubble */}
      <div
        style={{
          position: 'relative',
          ...getBubbleStyle()
        }}
        onMouseEnter={() => setIsHovered(true)}
        onMouseLeave={() => setIsHovered(false)}
      >
        {/* Actions */}
        {renderActions()}

        {/* Content */}
        <div style={{ padding: '16px 20px' }}>
          {/* Message Text */}
          <Paragraph style={{
            margin: 0,
            fontSize: '15px',
            lineHeight: '1.6',
            color: isUser ? 'white' : '#374151'
          }}>
            {message.content}
          </Paragraph>

          {/* SQL Code Block */}
          {message.sql && (
            <Card
              size="small"
              style={{
                marginTop: '12px',
                background: 'rgba(0, 0, 0, 0.05)',
                border: 'none'
              }}
              bodyStyle={{ padding: '12px' }}
            >
              <pre style={{
                margin: 0,
                fontFamily: 'Monaco, Menlo, monospace',
                fontSize: '13px',
                color: '#1f2937',
                whiteSpace: 'pre-wrap',
                wordBreak: 'break-word'
              }}>
                {message.sql}
              </pre>
            </Card>
          )}

          {/* Results Preview */}
          {message.results && message.results.length > 0 && (
            <div style={{
              marginTop: '12px',
              padding: '12px',
              background: 'rgba(59, 130, 246, 0.1)',
              borderRadius: '8px',
              border: '1px solid rgba(59, 130, 246, 0.2)'
            }}>
              <Space>
                <ThunderboltOutlined style={{ color: '#3b82f6' }} />
                <Text style={{ color: '#3b82f6', fontWeight: 500 }}>
                  {message.results.length} results found
                </Text>
              </Space>
            </div>
          )}

          {/* Metadata */}
          {renderMetadata()}

          {/* Timestamp and Status */}
          <div style={{
            display: 'flex',
            justifyContent: 'space-between',
            alignItems: 'center',
            marginTop: '8px'
          }}>
            <Text style={{
              fontSize: '11px',
              color: isUser ? 'rgba(255, 255, 255, 0.7)' : '#9ca3af'
            }}>
              {formatDistanceToNow(new Date(message.timestamp), { addSuffix: true })}
            </Text>
            {renderStatusIndicator()}
          </div>
        </div>
      </div>

      {/* CSS Animations */}
      <style jsx>{`
        @keyframes fadeInScale {
          from {
            opacity: 0;
            transform: scale(0.8);
          }
          to {
            opacity: 1;
            transform: scale(1);
          }
        }
        
        .typing-indicator {
          display: flex;
          gap: 2px;
        }
        
        .typing-indicator span {
          width: 4px;
          height: 4px;
          border-radius: 50%;
          background: #9ca3af;
          animation: typing 1.4s infinite ease-in-out;
        }
        
        .typing-indicator span:nth-child(1) { animation-delay: 0s; }
        .typing-indicator span:nth-child(2) { animation-delay: 0.2s; }
        .typing-indicator span:nth-child(3) { animation-delay: 0.4s; }
        
        @keyframes typing {
          0%, 60%, 100% {
            transform: translateY(0);
            opacity: 0.5;
          }
          30% {
            transform: translateY(-8px);
            opacity: 1;
          }
        }
      `}</style>
    </div>
  )
}
