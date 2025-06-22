import React, { useState } from 'react'
import { Card, Space, Typography, List, Avatar, Button, Tag, Tooltip, Input } from 'antd'
import {
  HistoryOutlined,
  MessageOutlined,
  SearchOutlined,
  DeleteOutlined,
  StarOutlined,
  StarFilled,
  ClockCircleOutlined
} from '@ant-design/icons'

const { Text } = Typography
const { Search } = Input

export interface ConversationHistoryProps {
  onConversationSelect?: (conversationId: string) => void
  showSearch?: boolean
  showFavorites?: boolean
  className?: string
}

interface Conversation {
  id: string
  title: string
  lastMessage: string
  timestamp: string
  messageCount: number
  isFavorite: boolean
  tags: string[]
}

/**
 * ConversationHistory - Browse and manage conversation history
 */
export const ConversationHistory: React.FC<ConversationHistoryProps> = ({
  onConversationSelect,
  showSearch = true,
  showFavorites = true,
  className = ''
}) => {
  const [searchQuery, setSearchQuery] = useState('')
  const [showOnlyFavorites, setShowOnlyFavorites] = useState(false)

  const conversations: Conversation[] = [
    {
      id: 'conv-1',
      title: 'Sales Analysis Q4 2023',
      lastMessage: 'Show me quarterly sales by region',
      timestamp: '2024-01-15T10:30:00Z',
      messageCount: 12,
      isFavorite: true,
      tags: ['sales', 'quarterly']
    },
    {
      id: 'conv-2',
      title: 'Product Performance Review',
      lastMessage: 'What are the top performing products?',
      timestamp: '2024-01-14T15:20:00Z',
      messageCount: 8,
      isFavorite: false,
      tags: ['products', 'performance']
    },
    {
      id: 'conv-3',
      title: 'Customer Segmentation',
      lastMessage: 'Analyze customer segments by revenue',
      timestamp: '2024-01-13T09:15:00Z',
      messageCount: 15,
      isFavorite: true,
      tags: ['customers', 'segmentation']
    }
  ]

  const filteredConversations = conversations.filter(conv => {
    const matchesSearch = !searchQuery || 
      conv.title.toLowerCase().includes(searchQuery.toLowerCase()) ||
      conv.lastMessage.toLowerCase().includes(searchQuery.toLowerCase())
    
    const matchesFavorites = !showOnlyFavorites || conv.isFavorite
    
    return matchesSearch && matchesFavorites
  })

  const formatTimestamp = (timestamp: string) => {
    const date = new Date(timestamp)
    const now = new Date()
    const diffMs = now.getTime() - date.getTime()
    const diffDays = Math.floor(diffMs / (1000 * 60 * 60 * 24))
    
    if (diffDays === 0) return 'Today'
    if (diffDays === 1) return 'Yesterday'
    if (diffDays < 7) return `${diffDays} days ago`
    return date.toLocaleDateString()
  }

  const handleToggleFavorite = (conversationId: string, e: React.MouseEvent) => {
    e.stopPropagation()
    // In real app, this would update the conversation favorite status
    console.log('Toggle favorite for:', conversationId)
  }

  const handleDeleteConversation = (conversationId: string, e: React.MouseEvent) => {
    e.stopPropagation()
    // In real app, this would delete the conversation
    console.log('Delete conversation:', conversationId)
  }

  return (
    <div className={`conversation-history ${className}`}>
      <Card size="small" title="Conversation History">
        <Space direction="vertical" style={{ width: '100%' }}>
          {/* Search and Filters */}
          {showSearch && (
            <Space direction="vertical" style={{ width: '100%' }}>
              <Search
                placeholder="Search conversations..."
                value={searchQuery}
                onChange={(e) => setSearchQuery(e.target.value)}
                size="small"
                allowClear
              />
              
              {showFavorites && (
                <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                  <Text style={{ fontSize: '11px' }}>Show only favorites</Text>
                  <Button
                    type={showOnlyFavorites ? 'primary' : 'default'}
                    size="small"
                    icon={showOnlyFavorites ? <StarFilled /> : <StarOutlined />}
                    onClick={() => setShowOnlyFavorites(!showOnlyFavorites)}
                  />
                </div>
              )}
            </Space>
          )}

          {/* Conversations List */}
          <List
            size="small"
            dataSource={filteredConversations}
            renderItem={(conversation) => (
              <List.Item
                style={{ 
                  padding: '8px 0',
                  cursor: 'pointer',
                  borderRadius: '4px'
                }}
                onClick={() => onConversationSelect?.(conversation.id)}
                actions={[
                  <Tooltip key="favorite" title={conversation.isFavorite ? 'Remove from favorites' : 'Add to favorites'}>
                    <Button
                      type="text"
                      size="small"
                      icon={conversation.isFavorite ? <StarFilled style={{ color: '#faad14' }} /> : <StarOutlined />}
                      onClick={(e) => handleToggleFavorite(conversation.id, e)}
                    />
                  </Tooltip>,
                  <Tooltip key="delete" title="Delete conversation">
                    <Button
                      type="text"
                      size="small"
                      icon={<DeleteOutlined />}
                      onClick={(e) => handleDeleteConversation(conversation.id, e)}
                      danger
                    />
                  </Tooltip>
                ]}
              >
                <List.Item.Meta
                  avatar={
                    <Avatar 
                      size="small" 
                      icon={<MessageOutlined />}
                      style={{ backgroundColor: '#1890ff' }}
                    />
                  }
                  title={
                    <Space style={{ width: '100%', justifyContent: 'space-between' }}>
                      <Text strong style={{ fontSize: '12px' }}>
                        {conversation.title}
                      </Text>
                      <Text type="secondary" style={{ fontSize: '10px' }}>
                        {conversation.messageCount} msgs
                      </Text>
                    </Space>
                  }
                  description={
                    <Space direction="vertical" style={{ width: '100%' }}>
                      <Text type="secondary" style={{ fontSize: '11px' }}>
                        {conversation.lastMessage}
                      </Text>
                      
                      <Space style={{ width: '100%', justifyContent: 'space-between' }}>
                        <Space>
                          {conversation.tags.map(tag => (
                            <Tag key={tag} size="small" style={{ fontSize: '9px' }}>
                              {tag}
                            </Tag>
                          ))}
                        </Space>
                        
                        <Space>
                          <ClockCircleOutlined style={{ fontSize: '10px' }} />
                          <Text type="secondary" style={{ fontSize: '10px' }}>
                            {formatTimestamp(conversation.timestamp)}
                          </Text>
                        </Space>
                      </Space>
                    </Space>
                  }
                />
              </List.Item>
            )}
          />

          {/* Empty State */}
          {filteredConversations.length === 0 && (
            <div style={{ textAlign: 'center', padding: '20px 0' }}>
              <Space direction="vertical">
                <HistoryOutlined style={{ fontSize: '24px', color: '#d9d9d9' }} />
                <Text type="secondary" style={{ fontSize: '12px' }}>
                  {searchQuery || showOnlyFavorites ? 'No conversations found' : 'No conversation history'}
                </Text>
              </Space>
            </div>
          )}

          {/* Summary */}
          <Card size="small" style={{ background: '#fafafa' }}>
            <Space style={{ width: '100%', justifyContent: 'space-between' }}>
              <Text style={{ fontSize: '11px' }}>
                Total: {conversations.length} conversations
              </Text>
              <Text style={{ fontSize: '11px' }}>
                Favorites: {conversations.filter(c => c.isFavorite).length}
              </Text>
            </Space>
          </Card>
        </Space>
      </Card>
    </div>
  )
}

export default ConversationHistory
