import React, { useState, useRef, useEffect } from 'react'
import { Button, Input, Typography, Space, Avatar, Card, Tooltip } from 'antd'
import {
  SendOutlined,
  PhoneOutlined,
  PaperClipOutlined,
  SmileOutlined,
  ThunderboltOutlined,
  StarOutlined,
  HistoryOutlined
} from '@ant-design/icons'

const { TextArea } = Input
const { Text } = Typography

interface CreativeChatInterfaceProps {
  onSendMessage?: (message: string) => void
  isLoading?: boolean
}

export const CreativeChatInterface: React.FC<CreativeChatInterfaceProps> = ({
  onSendMessage,
  isLoading = false
}) => {
  const [message, setMessage] = useState('')
  const [isRecording, setIsRecording] = useState(false)
  const [showSuggestions, setShowSuggestions] = useState(true)
  const inputRef = useRef<any>(null)

  const quickSuggestions = [
    { text: "Show me revenue trends", icon: "📈", color: "#10b981" },
    { text: "Top performing products", icon: "🏆", color: "#f59e0b" },
    { text: "Customer analytics", icon: "👥", color: "#3b82f6" },
    { text: "Sales by region", icon: "🌍", color: "#8b5cf6" },
    { text: "Monthly reports", icon: "📊", color: "#ef4444" },
    { text: "User engagement", icon: "💡", color: "#06b6d4" }
  ]

  const handleSend = () => {
    if (message.trim() && onSendMessage) {
      onSendMessage(message.trim())
      setMessage('')
      setShowSuggestions(false)
    }
  }

  const handleKeyPress = (e: React.KeyboardEvent) => {
    if (e.key === 'Enter' && (e.ctrlKey || e.metaKey)) {
      e.preventDefault()
      handleSend()
    }
  }

  const handleSuggestionClick = (suggestion: string) => {
    setMessage(suggestion)
    setShowSuggestions(false)
    setTimeout(() => inputRef.current?.focus(), 100)
  }

  return (
    <div style={{
      height: '100vh',
      display: 'flex',
      flexDirection: 'column',
      background: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)',
      position: 'relative',
      overflow: 'hidden'
    }}>
      {/* Floating Orbs Background */}
      <div style={{
        position: 'absolute',
        top: 0,
        left: 0,
        right: 0,
        bottom: 0,
        pointerEvents: 'none'
      }}>
        {[...Array(6)].map((_, i) => (
          <div
            key={i}
            style={{
              position: 'absolute',
              width: `${Math.random() * 100 + 50}px`,
              height: `${Math.random() * 100 + 50}px`,
              borderRadius: '50%',
              background: `linear-gradient(135deg, ${['#ff6b6b', '#4ecdc4', '#45b7d1', '#96ceb4', '#feca57', '#ff9ff3'][i]} 0%, transparent 70%)`,
              top: `${Math.random() * 100}%`,
              left: `${Math.random() * 100}%`,
              animation: `float ${5 + Math.random() * 10}s ease-in-out infinite`,
              animationDelay: `${Math.random() * 5}s`,
              opacity: 0.1
            }}
          />
        ))}
      </div>

      {/* Main Chat Container */}
      <div style={{
        flex: 1,
        display: 'flex',
        flexDirection: 'column',
        justifyContent: 'center',
        alignItems: 'center',
        padding: '40px',
        position: 'relative',
        zIndex: 1
      }}>
        {/* Welcome Header */}
        <div style={{
          textAlign: 'center',
          marginBottom: '40px',
          animation: 'fadeInUp 0.8s ease-out'
        }}>
          <div style={{
            fontSize: '80px',
            marginBottom: '20px',
            animation: 'bounce 2s ease-in-out infinite'
          }}>
            🤖
          </div>
          <h1 style={{
            fontSize: '48px',
            fontWeight: 800,
            background: 'linear-gradient(135deg, #fff 0%, #f0f0f0 100%)',
            WebkitBackgroundClip: 'text',
            WebkitTextFillColor: 'transparent',
            backgroundClip: 'text',
            margin: 0,
            marginBottom: '16px',
            textShadow: '0 4px 8px rgba(0, 0, 0, 0.1)'
          }}>
            What can I help you discover?
          </h1>
          <Text style={{
            fontSize: '18px',
            color: 'rgba(255, 255, 255, 0.8)',
            fontWeight: 400
          }}>
            Ask me anything about your data in natural language
          </Text>
        </div>

        {/* Suggestions Grid */}
        {showSuggestions && (
          <div style={{
            display: 'grid',
            gridTemplateColumns: 'repeat(auto-fit, minmax(200px, 1fr))',
            gap: '16px',
            maxWidth: '800px',
            width: '100%',
            marginBottom: '40px',
            animation: 'fadeInUp 0.8s ease-out 0.2s both'
          }}>
            {quickSuggestions.map((suggestion, index) => (
              <Card
                key={index}
                hoverable
                onClick={() => handleSuggestionClick(suggestion.text)}
                style={{
                  background: 'rgba(255, 255, 255, 0.1)',
                  backdropFilter: 'blur(10px)',
                  border: '1px solid rgba(255, 255, 255, 0.2)',
                  borderRadius: '16px',
                  cursor: 'pointer',
                  transition: 'all 0.3s cubic-bezier(0.4, 0, 0.2, 1)',
                  animationDelay: `${index * 0.1}s`
                }}
                bodyStyle={{ padding: '20px' }}
                onMouseEnter={(e) => {
                  e.currentTarget.style.transform = 'translateY(-4px) scale(1.02)'
                  e.currentTarget.style.boxShadow = `0 20px 40px ${suggestion.color}30`
                }}
                onMouseLeave={(e) => {
                  e.currentTarget.style.transform = 'translateY(0) scale(1)'
                  e.currentTarget.style.boxShadow = 'none'
                }}
              >
                <div style={{
                  display: 'flex',
                  alignItems: 'center',
                  gap: '12px'
                }}>
                  <div style={{
                    fontSize: '24px',
                    width: '40px',
                    height: '40px',
                    borderRadius: '12px',
                    background: suggestion.color,
                    display: 'flex',
                    alignItems: 'center',
                    justifyContent: 'center',
                    boxShadow: `0 8px 16px ${suggestion.color}40`
                  }}>
                    {suggestion.icon}
                  </div>
                  <Text style={{
                    color: 'white',
                    fontWeight: 500,
                    fontSize: '14px'
                  }}>
                    {suggestion.text}
                  </Text>
                </div>
              </Card>
            ))}
          </div>
        )}

        {/* Chat Input */}
        <div style={{
          width: '100%',
          maxWidth: '700px',
          animation: 'fadeInUp 0.8s ease-out 0.4s both'
        }}>
          <div style={{
            background: 'rgba(255, 255, 255, 0.95)',
            backdropFilter: 'blur(20px)',
            borderRadius: '24px',
            padding: '24px',
            border: '1px solid rgba(255, 255, 255, 0.3)',
            boxShadow: '0 20px 40px rgba(0, 0, 0, 0.1)'
          }}>
            <TextArea
              ref={inputRef}
              value={message}
              onChange={(e) => setMessage(e.target.value)}
              onKeyDown={handleKeyPress}
              placeholder="Type your question here..."
              autoSize={{ minRows: 3, maxRows: 6 }}
              style={{
                fontSize: '16px',
                lineHeight: '1.6',
                border: 'none',
                background: 'transparent',
                resize: 'none',
                boxShadow: 'none'
              }}
              disabled={isLoading}
            />
            
            <div style={{
              display: 'flex',
              justifyContent: 'space-between',
              alignItems: 'center',
              marginTop: '16px'
            }}>
              <Space>
                <Tooltip title="Voice input">
                  <Button
                    type="text"
                    icon={<PhoneOutlined />}
                    onClick={() => setIsRecording(!isRecording)}
                    style={{
                      width: '40px',
                      height: '40px',
                      borderRadius: '12px',
                      background: isRecording ? '#ff4d4f' : 'rgba(0, 0, 0, 0.05)',
                      color: isRecording ? 'white' : '#666'
                    }}
                  />
                </Tooltip>
                
                <Tooltip title="Attach file">
                  <Button
                    type="text"
                    icon={<PaperClipOutlined />}
                    style={{
                      width: '40px',
                      height: '40px',
                      borderRadius: '12px',
                      background: 'rgba(0, 0, 0, 0.05)',
                      color: '#666'
                    }}
                  />
                </Tooltip>
                
                <Tooltip title="Quick actions">
                  <Button
                    type="text"
                    icon={<SmileOutlined />}
                    style={{
                      width: '40px',
                      height: '40px',
                      borderRadius: '12px',
                      background: 'rgba(0, 0, 0, 0.05)',
                      color: '#666'
                    }}
                  />
                </Tooltip>
              </Space>

              <Button
                type="primary"
                icon={<SendOutlined />}
                onClick={handleSend}
                loading={isLoading}
                disabled={!message.trim()}
                style={{
                  height: '48px',
                  borderRadius: '16px',
                  background: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)',
                  border: 'none',
                  fontSize: '16px',
                  fontWeight: 600,
                  padding: '0 24px',
                  boxShadow: '0 8px 16px rgba(102, 126, 234, 0.4)'
                }}
              >
                {isLoading ? 'Thinking...' : 'Send'}
              </Button>
            </div>
          </div>
        </div>

        {/* Quick Actions Bar */}
        <div style={{
          marginTop: '24px',
          display: 'flex',
          gap: '12px',
          animation: 'fadeInUp 0.8s ease-out 0.6s both'
        }}>
          <Tooltip title="Recent queries">
            <Button
              type="text"
              icon={<HistoryOutlined />}
              style={{
                color: 'rgba(255, 255, 255, 0.8)',
                background: 'rgba(255, 255, 255, 0.1)',
                border: '1px solid rgba(255, 255, 255, 0.2)',
                borderRadius: '12px',
                height: '40px'
              }}
            >
              Recent
            </Button>
          </Tooltip>
          
          <Tooltip title="Saved queries">
            <Button
              type="text"
              icon={<StarOutlined />}
              style={{
                color: 'rgba(255, 255, 255, 0.8)',
                background: 'rgba(255, 255, 255, 0.1)',
                border: '1px solid rgba(255, 255, 255, 0.2)',
                borderRadius: '12px',
                height: '40px'
              }}
            >
              Favorites
            </Button>
          </Tooltip>
          
          <Tooltip title="AI insights">
            <Button
              type="text"
              icon={<ThunderboltOutlined />}
              style={{
                color: 'rgba(255, 255, 255, 0.8)',
                background: 'rgba(255, 255, 255, 0.1)',
                border: '1px solid rgba(255, 255, 255, 0.2)',
                borderRadius: '12px',
                height: '40px'
              }}
            >
              Insights
            </Button>
          </Tooltip>
        </div>
      </div>

      {/* CSS Animations */}
      <style jsx>{`
        @keyframes fadeInUp {
          from {
            opacity: 0;
            transform: translateY(30px);
          }
          to {
            opacity: 1;
            transform: translateY(0);
          }
        }
        
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
        
        @keyframes float {
          0%, 100% {
            transform: translateY(0px) rotate(0deg);
          }
          33% {
            transform: translateY(-20px) rotate(1deg);
          }
          66% {
            transform: translateY(-10px) rotate(-1deg);
          }
        }
      `}</style>
    </div>
  )
}
