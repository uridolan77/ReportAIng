import React, { useState } from 'react'
import { Card, Space, Typography, Button, Tag, List, Avatar, Tooltip } from 'antd'
import {
  BulbOutlined,
  HistoryOutlined,
  StarOutlined,
  TrendingUpOutlined,
  QuestionCircleOutlined,
  SendOutlined
} from '@ant-design/icons'

const { Text } = Typography

export interface QuerySuggestionsProps {
  onSuggestionSelect?: (query: string) => void
  showPopular?: boolean
  showRecent?: boolean
  showTemplates?: boolean
  className?: string
}

interface QuerySuggestion {
  id: string
  text: string
  category: 'popular' | 'recent' | 'template' | 'smart'
  usage: number
  confidence: number
  description?: string
}

/**
 * QuerySuggestions - Intelligent query suggestions and templates
 */
export const QuerySuggestions: React.FC<QuerySuggestionsProps> = ({
  onSuggestionSelect,
  showPopular = true,
  showRecent = true,
  showTemplates = true,
  className = ''
}) => {
  const [selectedCategory, setSelectedCategory] = useState<string>('smart')

  const suggestions: QuerySuggestion[] = [
    {
      id: 'smart-1',
      text: 'Show me quarterly sales by region',
      category: 'smart',
      usage: 85,
      confidence: 0.95,
      description: 'Based on your recent queries'
    },
    {
      id: 'popular-1',
      text: 'What are the top performing products?',
      category: 'popular',
      usage: 92,
      confidence: 0.98
    },
    {
      id: 'template-1',
      text: 'Compare [metric] between [time period]',
      category: 'template',
      usage: 78,
      confidence: 0.89,
      description: 'Comparison template'
    }
  ]

  const getCategoryIcon = (category: string) => {
    switch (category) {
      case 'popular': return <TrendingUpOutlined />
      case 'recent': return <HistoryOutlined />
      case 'template': return <QuestionCircleOutlined />
      case 'smart': return <BulbOutlined />
      default: return <BulbOutlined />
    }
  }

  const getCategoryColor = (category: string) => {
    switch (category) {
      case 'popular': return '#52c41a'
      case 'recent': return '#1890ff'
      case 'template': return '#faad14'
      case 'smart': return '#722ed1'
      default: return '#d9d9d9'
    }
  }

  const filteredSuggestions = suggestions.filter(s => s.category === selectedCategory)

  return (
    <div className={`query-suggestions ${className}`}>
      <Card size="small" title="Query Suggestions">
        <Space direction="vertical" style={{ width: '100%' }}>
          {/* Category Tabs */}
          <Space wrap>
            {['smart', 'popular', 'recent', 'template'].map(category => (
              <Button
                key={category}
                size="small"
                type={selectedCategory === category ? 'primary' : 'default'}
                icon={getCategoryIcon(category)}
                onClick={() => setSelectedCategory(category)}
                style={{ fontSize: '11px' }}
              >
                {category.charAt(0).toUpperCase() + category.slice(1)}
              </Button>
            ))}
          </Space>

          {/* Suggestions List */}
          <List
            size="small"
            dataSource={filteredSuggestions}
            renderItem={(suggestion) => (
              <List.Item
                style={{ padding: '8px 0' }}
                actions={[
                  <Button
                    key="use"
                    type="text"
                    size="small"
                    icon={<SendOutlined />}
                    onClick={() => onSuggestionSelect?.(suggestion.text)}
                  />
                ]}
              >
                <List.Item.Meta
                  avatar={
                    <Avatar 
                      size="small" 
                      icon={getCategoryIcon(suggestion.category)}
                      style={{ backgroundColor: getCategoryColor(suggestion.category) }}
                    />
                  }
                  title={
                    <Text style={{ fontSize: '12px' }}>{suggestion.text}</Text>
                  }
                  description={
                    <Space>
                      {suggestion.description && (
                        <Text type="secondary" style={{ fontSize: '10px' }}>
                          {suggestion.description}
                        </Text>
                      )}
                      <Tag size="small" color={getCategoryColor(suggestion.category)}>
                        {suggestion.usage}% usage
                      </Tag>
                    </Space>
                  }
                />
              </List.Item>
            )}
          />
        </Space>
      </Card>
    </div>
  )
}

export default QuerySuggestions
