import React, { useState } from 'react'
import { Input, Button, Space, Select, Spin, Typography, Card } from 'antd'
import { SearchOutlined, BulbOutlined } from '@ant-design/icons'
import type { QuerySuggestion } from '@shared/types/business-intelligence'

const { TextArea } = Input
const { Text } = Typography
const { Option } = Select

interface QueryAnalysisInputProps {
  value: string
  onChange: (value: string) => void
  onAnalyze: (query: string) => void
  suggestions?: QuerySuggestion[]
  onSuggestionSelect?: (suggestion: string) => void
  loading?: boolean
  suggestionsLoading?: boolean
  placeholder?: string
  disabled?: boolean
}

/**
 * QueryAnalysisInput - Input component for natural language queries
 * 
 * Features:
 * - Multi-line text input for complex queries
 * - Query suggestions dropdown
 * - Real-time analysis trigger
 * - Loading states and validation
 */
export const QueryAnalysisInput: React.FC<QueryAnalysisInputProps> = ({
  value,
  onChange,
  onAnalyze,
  suggestions = [],
  onSuggestionSelect,
  loading = false,
  suggestionsLoading = false,
  placeholder = "Enter your natural language query here...",
  disabled = false
}) => {
  const [selectedSuggestion, setSelectedSuggestion] = useState<string>('')

  const handleAnalyze = () => {
    if (value.trim()) {
      onAnalyze(value.trim())
    }
  }

  const handleSuggestionChange = (suggestionQuery: string) => {
    setSelectedSuggestion(suggestionQuery)
    onChange(suggestionQuery)
    if (onSuggestionSelect) {
      onSuggestionSelect(suggestionQuery)
    }
  }

  const handleKeyPress = (e: React.KeyboardEvent) => {
    if (e.key === 'Enter' && (e.ctrlKey || e.metaKey)) {
      e.preventDefault()
      handleAnalyze()
    }
  }

  return (
    <Space direction="vertical" style={{ width: '100%' }} size="middle">
      {/* Query Suggestions */}
      {suggestions.length > 0 && (
        <div>
          <Text strong style={{ marginBottom: 8, display: 'block' }}>
            <BulbOutlined /> Quick Start - Select a sample query:
          </Text>
          <Select
            value={selectedSuggestion}
            onChange={handleSuggestionChange}
            style={{ width: '100%', maxWidth: 600 }}
            placeholder="Choose from popular queries..."
            loading={suggestionsLoading}
            disabled={disabled}
            showSearch
            filterOption={(input, option) =>
              (option?.children as string)?.toLowerCase().includes(input.toLowerCase())
            }
          >
            {suggestions.map((suggestion) => (
              <Option key={suggestion.id} value={suggestion.query}>
                <div>
                  <div>{suggestion.query}</div>
                  <Text type="secondary" style={{ fontSize: '12px' }}>
                    {suggestion.description} • {suggestion.category}
                  </Text>
                </div>
              </Option>
            ))}
          </Select>
        </div>
      )}

      {/* Query Input */}
      <div>
        <Text strong style={{ marginBottom: 8, display: 'block' }}>
          Natural Language Query:
        </Text>
        <Space.Compact style={{ width: '100%' }}>
          <TextArea
            value={value}
            onChange={(e) => onChange(e.target.value)}
            onKeyDown={handleKeyPress}
            placeholder={placeholder}
            rows={3}
            style={{ 
              fontSize: '16px',
              resize: 'vertical'
            }}
            disabled={disabled}
            maxLength={1000}
          />
          <Button
            type="primary"
            icon={<SearchOutlined />}
            onClick={handleAnalyze}
            loading={loading}
            disabled={disabled || !value.trim()}
            style={{ 
              height: 'auto',
              minHeight: '76px', // Match textarea height
              display: 'flex',
              alignItems: 'center',
              justifyContent: 'center'
            }}
          >
            Analyze
          </Button>
        </Space.Compact>
      </div>

      {/* Help Text */}
      <Text type="secondary" style={{ fontSize: '12px' }}>
        💡 Tip: Use Ctrl+Enter (Cmd+Enter on Mac) to quickly analyze your query. 
        Try queries like "Show me sales by region" or "What are the top customers this month?"
      </Text>

      {/* Loading Indicator */}
      {loading && (
        <Card size="small" style={{ textAlign: 'center' }}>
          <Spin />
          <Text style={{ marginLeft: 8 }}>Analyzing your query...</Text>
        </Card>
      )}
    </Space>
  )
}

export default QueryAnalysisInput
