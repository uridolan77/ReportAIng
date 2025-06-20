import React, { useState, useRef, useEffect } from 'react'
import {
  Input,
  Button,
  Space,
  Card,
  List,
  Typography,
  Tag,
  Tooltip,
  Dropdown,
  AutoComplete,
  Mentions
} from 'antd'
import {
  SendOutlined,
  PhoneOutlined,
  PaperClipOutlined,
  HistoryOutlined,
  BulbOutlined,
  DatabaseOutlined,
  SettingOutlined
} from '@ant-design/icons'
import { useAppSelector, useAppDispatch } from '@shared/hooks'
import {
  selectInputValue,
  selectSuggestions,
  selectShowSuggestions,
  selectRecentQueries,
  selectFavoriteQueries,
  selectChatSettings,
  chatActions
} from '@shared/store/chat'
import {
  useGetQuerySuggestionsQuery,
  useGetPopularQueriesQuery,
  useGetQueryTemplatesQuery,
  useGetBusinessContextQuery
} from '@shared/store/api/chatApi'
import type { QuerySuggestion } from '@shared/types/chat'

const { TextArea } = Input
const { Text } = Typography

interface ChatInputProps {
  onSend: (message: string) => void
  disabled?: boolean
  placeholder?: string
  maxLength?: number
}

export const ChatInput: React.FC<ChatInputProps> = ({
  onSend,
  disabled = false,
  placeholder = "Ask me anything about your data...",
  maxLength = 2000,
}) => {
  const dispatch = useAppDispatch()
  const inputRef = useRef<any>(null)
  
  const inputValue = useAppSelector(selectInputValue)
  const suggestions = useAppSelector(selectSuggestions)
  const showSuggestions = useAppSelector(selectShowSuggestions)
  const recentQueries = useAppSelector(selectRecentQueries)
  const favoriteQueries = useAppSelector(selectFavoriteQueries)
  const settings = useAppSelector(selectChatSettings)

  const [isRecording, setIsRecording] = useState(false)
  const [showTemplates, setShowTemplates] = useState(false)

  // API queries for suggestions
  const { data: querySuggestions } = useGetQuerySuggestionsQuery(
    { query: inputValue, limit: 5 },
    { skip: inputValue.length < 2 }
  )

  const { data: popularQueries } = useGetPopularQueriesQuery(
    { limit: 10, timeframe: 'week' }
  )

  const { data: queryTemplates } = useGetQueryTemplatesQuery(
    { limit: 20 }
  )

  const { data: businessContext } = useGetBusinessContextQuery(
    { query: inputValue },
    { skip: inputValue.length < 3 }
  )

  useEffect(() => {
    if (querySuggestions) {
      dispatch(chatActions.setSuggestions(querySuggestions))
    }
  }, [querySuggestions, dispatch])

  const handleInputChange = (value: string) => {
    dispatch(chatActions.setInputValue(value))
    
    // Show suggestions if enabled and input has content
    if (settings.showSuggestions && value.length > 0) {
      dispatch(chatActions.setShowSuggestions(true))
    } else {
      dispatch(chatActions.setShowSuggestions(false))
    }
  }

  const handleSend = () => {
    if (inputValue.trim() && !disabled) {
      onSend(inputValue.trim())
      dispatch(chatActions.setInputValue(''))
      dispatch(chatActions.setShowSuggestions(false))
      inputRef.current?.focus()
    }
  }

  const handleKeyPress = (e: React.KeyboardEvent) => {
    if (e.key === 'Enter' && !e.shiftKey) {
      e.preventDefault()
      handleSend()
    }
  }

  const handleSuggestionSelect = (suggestion: QuerySuggestion | string) => {
    const text = typeof suggestion === 'string' ? suggestion : suggestion.text
    dispatch(chatActions.setInputValue(text))
    dispatch(chatActions.setShowSuggestions(false))
    inputRef.current?.focus()
  }

  const handleVoiceInput = () => {
    if ('webkitSpeechRecognition' in window || 'SpeechRecognition' in window) {
      setIsRecording(!isRecording)
      // Implement speech recognition
    }
  }

  const renderSuggestionItem = (suggestion: QuerySuggestion) => (
    <List.Item
      style={{ cursor: 'pointer', padding: '8px 12px' }}
      onClick={() => handleSuggestionSelect(suggestion)}
    >
      <Space direction="vertical" style={{ width: '100%' }}>
        <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
          <Text strong>{suggestion.text}</Text>
          <Space>
            <Tag color={suggestion.category === 'recent' ? 'blue' : 
                       suggestion.category === 'popular' ? 'green' : 
                       suggestion.category === 'recommended' ? 'orange' : 'purple'}>
              {suggestion.category}
            </Tag>
            {suggestion.confidence && (
              <Tag color="cyan">
                {(suggestion.confidence * 100).toFixed(0)}%
              </Tag>
            )}
          </Space>
        </div>
        {suggestion.description && (
          <Text type="secondary" style={{ fontSize: '12px' }}>
            {suggestion.description}
          </Text>
        )}
        {suggestion.metadata && (
          <Space size="small">
            {suggestion.metadata.tablesUsed?.map(table => (
              <Tag key={table} icon={<DatabaseOutlined />}>
                {table}
              </Tag>
            ))}
            {suggestion.metadata.complexity && (
              <Tag color={
                suggestion.metadata.complexity === 'Simple' ? 'green' :
                suggestion.metadata.complexity === 'Medium' ? 'orange' : 'red'
              }>
                {suggestion.metadata.complexity}
              </Tag>
            )}
          </Space>
        )}
      </Space>
    </List.Item>
  )

  const quickActions = [
    {
      key: 'recent',
      icon: <HistoryOutlined />,
      label: 'Recent Queries',
      children: recentQueries.slice(0, 5).map((query, index) => ({
        key: `recent-${index}`,
        label: query,
        onClick: () => handleSuggestionSelect(query),
      })),
    },
    {
      key: 'favorites',
      icon: <BulbOutlined />,
      label: 'Favorite Queries',
      children: favoriteQueries.slice(0, 5).map((query, index) => ({
        key: `favorite-${index}`,
        label: query,
        onClick: () => handleSuggestionSelect(query),
      })),
    },
    {
      key: 'templates',
      icon: <SettingOutlined />,
      label: 'Query Templates',
      children: queryTemplates?.slice(0, 10).map((template, index) => ({
        key: `template-${index}`,
        label: template.text,
        onClick: () => handleSuggestionSelect(template),
      })) || [],
    },
  ]

  const autoCompleteOptions = [
    ...suggestions.map(s => ({ value: s.text, label: s.text })),
    ...recentQueries.slice(0, 3).map(q => ({ value: q, label: q })),
  ]

  return (
    <div style={{ position: 'relative' }}>
      {/* Suggestions Dropdown */}
      {showSuggestions && suggestions.length > 0 && (
        <Card
          size="small"
          style={{
            position: 'absolute',
            bottom: '100%',
            left: 0,
            right: 0,
            marginBottom: 8,
            maxHeight: 300,
            overflow: 'auto',
            zIndex: 1000,
            boxShadow: '0 4px 12px rgba(0, 0, 0, 0.15)',
          }}
          title={
            <Space>
              <BulbOutlined />
              <Text>Suggestions</Text>
            </Space>
          }
          extra={
            <Button
              type="text"
              size="small"
              onClick={() => dispatch(chatActions.setShowSuggestions(false))}
            >
              ×
            </Button>
          }
        >
          <List
            size="small"
            dataSource={suggestions}
            renderItem={renderSuggestionItem}
          />
        </Card>
      )}

      {/* Business Context Helper */}
      {businessContext && inputValue.length > 3 && (
        <Card
          size="small"
          style={{
            position: 'absolute',
            bottom: '100%',
            right: 0,
            width: 300,
            marginBottom: 8,
            zIndex: 999,
            boxShadow: '0 4px 12px rgba(0, 0, 0, 0.15)',
          }}
          title="Business Context"
        >
          {businessContext.tables.length > 0 && (
            <div style={{ marginBottom: 8 }}>
              <Text strong>Relevant Tables:</Text>
              <div>
                {businessContext.tables.slice(0, 3).map(table => (
                  <Tag key={table.name} style={{ margin: '2px' }}>
                    {table.name}
                  </Tag>
                ))}
              </div>
            </div>
          )}
          {businessContext.glossaryTerms.length > 0 && (
            <div>
              <Text strong>Business Terms:</Text>
              <div>
                {businessContext.glossaryTerms.slice(0, 3).map(term => (
                  <Tooltip key={term.term} title={term.definition}>
                    <Tag style={{ margin: '2px', cursor: 'help' }}>
                      {term.term}
                    </Tag>
                  </Tooltip>
                ))}
              </div>
            </div>
          )}
        </Card>
      )}

      {/* Main Input */}
      <Space.Compact style={{ width: '100%' }}>
        <TextArea
          ref={inputRef}
          value={inputValue}
          onChange={(e) => handleInputChange(e.target.value)}
          onKeyPress={handleKeyPress}
          placeholder={placeholder}
          autoSize={{ minRows: 1, maxRows: 4 }}
          disabled={disabled}
          maxLength={maxLength}
          showCount
          style={{ flex: 1 }}
        />
        
        <Space>
          {/* Quick Actions */}
          <Dropdown
            menu={{ items: quickActions }}
            placement="topRight"
            trigger={['click']}
          >
            <Button icon={<HistoryOutlined />} />
          </Dropdown>

          {/* Voice Input */}
          <Tooltip title="Voice input">
            <Button
              icon={<PhoneOutlined />}
              onClick={handleVoiceInput}
              type={isRecording ? 'primary' : 'default'}
              danger={isRecording}
            />
          </Tooltip>

          {/* Send Button */}
          <Button
            type="primary"
            icon={<SendOutlined />}
            onClick={handleSend}
            disabled={disabled || !inputValue.trim()}
          >
            Send
          </Button>
        </Space>
      </Space.Compact>

      {/* Character Count */}
      <div style={{ textAlign: 'right', marginTop: 4 }}>
        <Text type="secondary" style={{ fontSize: '11px' }}>
          {inputValue.length}/{maxLength}
        </Text>
      </div>
    </div>
  )
}
