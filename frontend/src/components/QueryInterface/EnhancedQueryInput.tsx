/**
 * Enhanced Query Input Component
 * Provides intelligent query input with shortcuts, autocomplete, and templates
 */

import React, { useState, useEffect, useRef } from 'react';
import {
  Input,
  Space,
  Typography,
  Tag,
  Button,
  Tooltip,
  Dropdown,
  AutoComplete,
  message,
} from 'antd';
import {
  SearchOutlined,
  ThunderboltOutlined,
  HistoryOutlined,
  SendOutlined,
  ClearOutlined,
  BookOutlined,
  BulbOutlined,
} from '@ant-design/icons';
import { queryTemplateService, QuerySuggestion } from '../../services/queryTemplateService';

const { TextArea } = Input;
const { Text } = Typography;

interface EnhancedQueryInputProps {
  value: string;
  onChange: (value: string) => void;
  onSubmit: (query: string) => void;
  loading?: boolean;
  placeholder?: string;
  showShortcuts?: boolean;
  autoHeight?: boolean;
  maxRows?: number;
}

interface ShortcutHint {
  shortcut: string;
  description: string;
  example: string;
}

export const EnhancedQueryInput: React.FC<EnhancedQueryInputProps> = ({
  value,
  onChange,
  onSubmit,
  loading = false,
  placeholder = "Type your query or use shortcuts like 'rev' for revenue, 'users' for active users...",
  showShortcuts = true,
  autoHeight = true,
  maxRows = 6,
}) => {
  const [suggestions, setSuggestions] = useState<QuerySuggestion[]>([]);
  const [showSuggestions, setShowSuggestions] = useState(false);
  const [selectedSuggestion, setSelectedSuggestion] = useState(-1);
  const [shortcutHints, setShortcutHints] = useState<ShortcutHint[]>([]);
  const inputRef = useRef<any>(null);

  // Load shortcut hints
  useEffect(() => {
    const shortcuts = queryTemplateService.getShortcuts();
    const hints: ShortcutHint[] = shortcuts.slice(0, 4).map(shortcut => ({
      shortcut: shortcut.shortcut,
      description: shortcut.name,
      example: shortcut.query,
    }));
    setShortcutHints(hints);
  }, []);

  // Update suggestions when value changes
  useEffect(() => {
    if (value.trim()) {
      const newSuggestions = queryTemplateService.searchSuggestions(value);
      setSuggestions(newSuggestions);
      setShowSuggestions(newSuggestions.length > 0);
      setSelectedSuggestion(-1);
    } else {
      setSuggestions([]);
      setShowSuggestions(false);
    }
  }, [value]);

  // Handle keyboard navigation
  const handleKeyDown = (e: React.KeyboardEvent) => {
    if (!showSuggestions || suggestions.length === 0) {
      if (e.key === 'Enter' && (e.ctrlKey || e.metaKey)) {
        e.preventDefault();
        handleSubmit();
      }
      return;
    }

    switch (e.key) {
      case 'ArrowDown':
        e.preventDefault();
        setSelectedSuggestion(prev =>
          prev < suggestions.length - 1 ? prev + 1 : 0
        );
        break;
      case 'ArrowUp':
        e.preventDefault();
        setSelectedSuggestion(prev =>
          prev > 0 ? prev - 1 : suggestions.length - 1
        );
        break;
      case 'Enter':
        e.preventDefault();
        if (selectedSuggestion >= 0) {
          applySuggestion(suggestions[selectedSuggestion]);
        } else if (e.ctrlKey || e.metaKey) {
          handleSubmit();
        }
        break;
      case 'Escape':
        setShowSuggestions(false);
        setSelectedSuggestion(-1);
        break;
    }
  };

  const applySuggestion = (suggestion: QuerySuggestion) => {
    onChange(suggestion.text);
    setShowSuggestions(false);
    setSelectedSuggestion(-1);

    // Track usage
    queryTemplateService.incrementUsage(
      suggestion.id,
      suggestion.type === 'template' ? 'template' : 'shortcut'
    );
    queryTemplateService.addToRecent(suggestion.text);

    message.success(`Applied ${suggestion.type}: ${suggestion.description}`);
  };

  const handleSubmit = () => {
    if (value.trim()) {
      onSubmit(value.trim());
      queryTemplateService.addToRecent(value.trim());
    }
  };

  const handleClear = () => {
    onChange('');
    setShowSuggestions(false);
    inputRef.current?.focus();
  };

  const getSuggestionIcon = (type: string) => {
    switch (type) {
      case 'template':
        return <ThunderboltOutlined style={{ color: '#1890ff' }} />;
      case 'shortcut':
        return <BulbOutlined style={{ color: '#52c41a' }} />;
      case 'recent':
        return <HistoryOutlined style={{ color: '#faad14' }} />;
      default:
        return <SearchOutlined />;
    }
  };

  const shortcutMenuItems = [
    ...shortcutHints.map(hint => ({
      key: hint.shortcut,
      label: (
        <Space direction="vertical" size={0}>
          <Space>
            <Tag color="blue">{hint.shortcut}</Tag>
            <Text strong>{hint.description}</Text>
          </Space>
          <Text type="secondary" style={{ fontSize: '11px' }}>
            {hint.example}
          </Text>
        </Space>
      ),
      onClick: () => onChange(hint.shortcut)
    })),
    {
      type: 'divider' as const
    },
    {
      key: 'view-all',
      label: <Text type="secondary">View all shortcuts...</Text>
    }
  ];

  const autoCompleteOptions = suggestions.map((suggestion, index) => ({
    value: suggestion.text,
    label: (
      <div
        style={{
          padding: '8px 0',
          backgroundColor: index === selectedSuggestion ? '#f0f0f0' : 'transparent',
        }}
      >
        <Space direction="vertical" size={0} style={{ width: '100%' }}>
          <Space>
            {getSuggestionIcon(suggestion.type)}
            <Text strong>{suggestion.description}</Text>
            <Tag color="blue">{suggestion.type}</Tag>
            <Tag>{suggestion.category}</Tag>
          </Space>
          <Text
            type="secondary"
            style={{
              fontSize: '11px',
              fontFamily: 'monospace',
              wordBreak: 'break-all',
            }}
          >
            {suggestion.text.length > 100
              ? `${suggestion.text.substring(0, 100)}...`
              : suggestion.text
            }
          </Text>
        </Space>
      </div>
    ),
  }));

  return (
    <div style={{ position: 'relative' }}>
      <div
        style={{
          background: 'transparent',
          border: 'none'
        }}
      >
        <Space direction="vertical" style={{ width: '100%' }}>
          {/* Enhanced Query Input */}
          <div style={{ position: 'relative' }}>
            <AutoComplete
              value={value}
              onChange={onChange}
              onSelect={(selectedValue) => {
                const suggestion = suggestions.find(s => s.text === selectedValue);
                if (suggestion) {
                  applySuggestion(suggestion);
                }
              }}
              options={autoCompleteOptions}
              open={showSuggestions}
              onOpenChange={setShowSuggestions}
              style={{ width: '100%' }}
              styles={{
                popup: {
                  root: {
                    maxHeight: 300,
                    overflow: 'auto',
                    borderRadius: '12px',
                    boxShadow: '0 8px 32px rgba(0, 0, 0, 0.12)'
                  }
                }
              }}
            >
              <TextArea
                ref={inputRef}
                placeholder={placeholder}
                autoSize={autoHeight ? { minRows: 3, maxRows } : false}
                onKeyDown={handleKeyDown}
                className="query-textarea"
                style={{
                  resize: 'vertical',
                  fontSize: '18px',
                  lineHeight: '1.6',
                  padding: '20px 24px',
                  border: 'none',
                  borderRadius: '12px',
                  background: 'transparent',
                  fontWeight: 400
                }}
              />
            </AutoComplete>

            {/* Enhanced Input Actions */}
            <div
              style={{
                position: 'absolute',
                bottom: 16,
                right: 16,
                display: 'flex',
                gap: 12,
              }}
            >
              {value && (
                <Tooltip title="Clear input">
                  <Button
                    size="middle"
                    type="text"
                    icon={<ClearOutlined />}
                    onClick={handleClear}
                    style={{
                      borderRadius: '10px',
                      background: 'rgba(255, 255, 255, 0.95)',
                      border: '1px solid #e2e8f0',
                      boxShadow: '0 2px 8px rgba(0, 0, 0, 0.1)',
                      height: '40px',
                      width: '40px'
                    }}
                  />
                </Tooltip>
              )}

              <Tooltip title="Submit query (Ctrl+Enter)">
                <Button
                  size="middle"
                  type="primary"
                  icon={<SendOutlined />}
                  loading={loading}
                  onClick={handleSubmit}
                  disabled={!value.trim()}
                  className="query-submit-btn"
                  style={{
                    height: '40px',
                    borderRadius: '10px',
                    fontWeight: 600,
                    background: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)',
                    border: 'none',
                    boxShadow: '0 4px 16px rgba(102, 126, 234, 0.3)',
                    minWidth: '80px'
                  }}
                />
              </Tooltip>
            </div>
          </div>

          {/* Enhanced Shortcut Hints */}
          {showShortcuts && shortcutHints.length > 0 && (
            <div style={{
              display: 'flex',
              alignItems: 'center',
              gap: 12,
              flexWrap: 'wrap',
              padding: '16px 20px',
              background: 'linear-gradient(135deg, #f8fafc 0%, #f1f5f9 100%)',
              borderRadius: '12px',
              border: '1px solid #e2e8f0',
              marginTop: '16px'
            }}>
              <Text type="secondary" style={{ fontSize: '14px', fontWeight: 500 }}>
                ⚡ Quick shortcuts:
              </Text>
              {shortcutHints.map(hint => (
                <Tooltip key={hint.shortcut} title={hint.example}>
                  <Tag
                    color="blue"
                    style={{
                      cursor: 'pointer',
                      fontSize: '13px',
                      borderRadius: '8px',
                      fontWeight: 500,
                      transition: 'all 0.3s ease',
                      padding: '4px 8px'
                    }}
                    onClick={() => onChange(hint.shortcut)}
                  >
                    {hint.shortcut}
                  </Tag>
                </Tooltip>
              ))}
              <Dropdown menu={{ items: shortcutMenuItems }} trigger={['click']}>
                <Button
                  size="small"
                  type="text"
                  icon={<BookOutlined />}
                  style={{
                    color: '#667eea',
                    border: '1px solid #667eea',
                    borderRadius: '8px',
                    fontWeight: 500,
                    height: '28px'
                  }}
                >
                  More
                </Button>
              </Dropdown>
            </div>
          )}

          {/* Enhanced Help Text */}
          <div style={{
            padding: '12px 16px',
            background: 'rgba(102, 126, 234, 0.05)',
            borderRadius: '10px',
            border: '1px solid rgba(102, 126, 234, 0.1)',
            marginTop: '12px'
          }}>
            <Text type="secondary" style={{ fontSize: '14px', color: '#667eea' }}>
              💡 <strong>Tip:</strong> Use shortcuts like "rev", "users", "top10" or start typing to see suggestions.
              Press <kbd style={{
                background: '#f0f0f0',
                padding: '3px 8px',
                borderRadius: '6px',
                fontSize: '12px',
                fontWeight: 500
              }}>Ctrl+Enter</kbd> to submit.
            </Text>
          </div>
        </Space>
      </div>
    </div>
  );
};
