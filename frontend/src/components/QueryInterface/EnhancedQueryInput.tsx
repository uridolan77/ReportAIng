/**
 * Enhanced Query Input Component
 * Advanced text input with AI suggestions, syntax highlighting, and smart features
 */

import React, { useState, useRef, useEffect, useCallback } from 'react';
import { Input, Button, Space, Tooltip, Dropdown, Menu, Tag } from 'antd';
import {
  SendOutlined,
  BulbOutlined,
  HistoryOutlined,
  StarOutlined,
  ClearOutlined,
  ExpandOutlined,
  CompressOutlined
} from '@ant-design/icons';
import { LLMSelector } from '../AI/LLMSelector';

const { TextArea } = Input;

interface QuerySuggestion {
  id: string;
  text: string;
  type: 'completion' | 'template' | 'history';
  confidence: number;
}

interface EnhancedQueryInputProps {
  value?: string;
  onChange?: (value: string) => void;
  onSubmit?: (query: string, options?: { providerId?: string; modelId?: string }) => void;
  placeholder?: string;
  disabled?: boolean;
  loading?: boolean;
  suggestions?: QuerySuggestion[];
  onSuggestionSelect?: (suggestion: QuerySuggestion) => void;
  showHistory?: boolean;
  showTemplates?: boolean;
  showLLMSelector?: boolean;
  autoResize?: boolean;
  maxRows?: number;
  minRows?: number;
}

export const EnhancedQueryInput: React.FC<EnhancedQueryInputProps> = ({
  value = '',
  onChange,
  onSubmit,
  placeholder = 'Ask me anything about your data...',
  disabled = false,
  loading = false,
  suggestions = [],
  onSuggestionSelect,
  showHistory = true,
  showTemplates = true,
  showLLMSelector = false,
  autoResize = true,
  maxRows = 8,
  minRows = 3
}) => {
  const [inputValue, setInputValue] = useState(value);
  const [isExpanded, setIsExpanded] = useState(false);
  const [showSuggestions, setShowSuggestions] = useState(false);
  const [cursorPosition, setCursorPosition] = useState(0);
  const [selectedProviderId, setSelectedProviderId] = useState<string>();
  const [selectedModelId, setSelectedModelId] = useState<string>();
  const inputRef = useRef<any>(null);

  useEffect(() => {
    setInputValue(value);
  }, [value]);

  const handleInputChange = useCallback((e: React.ChangeEvent<HTMLTextAreaElement>) => {
    const newValue = e.target.value;
    setInputValue(newValue);
    setCursorPosition(e.target.selectionStart || 0);
    onChange?.(newValue);
    
    // Show suggestions when typing
    if (newValue.length > 2) {
      setShowSuggestions(true);
    } else {
      setShowSuggestions(false);
    }
  }, [onChange]);

  const handleSubmit = useCallback(() => {
    if (inputValue.trim() && !loading) {
      onSubmit?.(inputValue.trim(), {
        providerId: selectedProviderId,
        modelId: selectedModelId
      });
    }
  }, [inputValue, loading, onSubmit, selectedProviderId, selectedModelId]);

  const handleKeyPress = useCallback((e: React.KeyboardEvent) => {
    if (e.key === 'Enter' && (e.ctrlKey || e.metaKey)) {
      e.preventDefault();
      handleSubmit();
    }
  }, [handleSubmit]);

  const handleSuggestionClick = useCallback((suggestion: QuerySuggestion) => {
    setInputValue(suggestion.text);
    onChange?.(suggestion.text);
    onSuggestionSelect?.(suggestion);
    setShowSuggestions(false);
    inputRef.current?.focus();
  }, [onChange, onSuggestionSelect]);

  const handleClear = useCallback(() => {
    setInputValue('');
    onChange?.('');
    inputRef.current?.focus();
  }, [onChange]);

  const toggleExpanded = useCallback(() => {
    setIsExpanded(!isExpanded);
  }, [isExpanded]);

  const historyMenu = (
    <Menu>
      <Menu.Item key="recent1">Show me sales data for last month</Menu.Item>
      <Menu.Item key="recent2">Top performing players by revenue</Menu.Item>
      <Menu.Item key="recent3">Daily active users trend</Menu.Item>
    </Menu>
  );

  const templatesMenu = (
    <Menu>
      <Menu.Item key="template1">Player Analytics Template</Menu.Item>
      <Menu.Item key="template2">Revenue Analysis Template</Menu.Item>
      <Menu.Item key="template3">Performance Metrics Template</Menu.Item>
    </Menu>
  );

  const suggestionItems = suggestions.map(suggestion => ({
    key: suggestion.id,
    label: (
      <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between' }}>
        <span>{suggestion.text}</span>
        <Tag color={
          suggestion.type === 'completion' ? 'blue' :
          suggestion.type === 'template' ? 'green' : 'orange'
        }>
          {suggestion.type}
        </Tag>
      </div>
    ),
    onClick: () => handleSuggestionClick(suggestion)
  }));

  return (
    <div style={{ position: 'relative' }}>
      {/* LLM Selector */}
      {showLLMSelector && (
        <LLMSelector
          selectedProviderId={selectedProviderId}
          selectedModelId={selectedModelId}
          useCase="SQL"
          onProviderChange={setSelectedProviderId}
          onModelChange={setSelectedModelId}
          compact={true}
          showStatus={true}
        />
      )}

      <div style={{
        border: '2px solid #f0f0f0',
        borderRadius: '12px',
        padding: '16px',
        background: '#fff',
        transition: 'all 0.3s ease',
        marginTop: showLLMSelector ? '8px' : '0',
        ...(inputRef.current?.focused && {
          borderColor: '#667eea',
          boxShadow: '0 0 0 3px rgba(102, 126, 234, 0.1)'
        })
      }}>
        <TextArea
          ref={inputRef}
          value={inputValue}
          onChange={handleInputChange}
          onKeyDown={handleKeyPress}
          placeholder={placeholder}
          disabled={disabled}
          autoSize={autoResize ? { minRows, maxRows: isExpanded ? 20 : maxRows } : false}
          style={{
            border: 'none',
            outline: 'none',
            resize: 'none',
            fontSize: '16px',
            lineHeight: '1.5',
            background: 'transparent'
          }}
        />
        
        <div style={{ 
          display: 'flex', 
          alignItems: 'center', 
          justifyContent: 'space-between',
          marginTop: '12px',
          paddingTop: '12px',
          borderTop: '1px solid #f0f0f0'
        }}>
          <Space>
            {showHistory && (
              <Dropdown overlay={historyMenu} trigger={['click']}>
                <Button 
                  type="text" 
                  icon={<HistoryOutlined />} 
                  size="small"
                  style={{ color: '#8c8c8c' }}
                >
                  History
                </Button>
              </Dropdown>
            )}
            
            {showTemplates && (
              <Dropdown overlay={templatesMenu} trigger={['click']}>
                <Button 
                  type="text" 
                  icon={<StarOutlined />}
                  size="small"
                  style={{ color: '#8c8c8c' }}
                >
                  Templates
                </Button>
              </Dropdown>
            )}
            
            <Tooltip title="AI Suggestions">
              <Button 
                type="text" 
                icon={<BulbOutlined />} 
                size="small"
                style={{ color: suggestions.length > 0 ? '#667eea' : '#8c8c8c' }}
                onClick={() => setShowSuggestions(!showSuggestions)}
              >
                {suggestions.length > 0 && (
                  <span style={{ marginLeft: '4px' }}>({suggestions.length})</span>
                )}
              </Button>
            </Tooltip>
          </Space>
          
          <Space>
            {inputValue && (
              <Tooltip title="Clear">
                <Button 
                  type="text" 
                  icon={<ClearOutlined />} 
                  size="small"
                  onClick={handleClear}
                  style={{ color: '#8c8c8c' }}
                />
              </Tooltip>
            )}
            
            <Tooltip title={isExpanded ? "Collapse" : "Expand"}>
              <Button 
                type="text" 
                icon={isExpanded ? <CompressOutlined /> : <ExpandOutlined />} 
                size="small"
                onClick={toggleExpanded}
                style={{ color: '#8c8c8c' }}
              />
            </Tooltip>
            
            <Button
              type="primary"
              icon={<SendOutlined />}
              onClick={handleSubmit}
              disabled={!inputValue.trim() || disabled}
              loading={loading}
              style={{
                background: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)',
                border: 'none',
                borderRadius: '8px'
              }}
            >
              {loading ? 'Processing...' : 'Ask AI'}
            </Button>
          </Space>
        </div>
      </div>
      
      {/* Suggestions Dropdown */}
      {showSuggestions && suggestions.length > 0 && (
        <div style={{
          position: 'absolute',
          top: '100%',
          left: 0,
          right: 0,
          background: '#fff',
          border: '1px solid #f0f0f0',
          borderRadius: '8px',
          boxShadow: '0 4px 12px rgba(0, 0, 0, 0.1)',
          zIndex: 1000,
          marginTop: '4px',
          maxHeight: '200px',
          overflowY: 'auto'
        }}>
          <Menu items={suggestionItems} />
        </div>
      )}
    </div>
  );
};

export default EnhancedQueryInput;
