/**
 * Enhanced Query Input Component
 * Provides intelligent query input with shortcuts, autocomplete, and templates
 */

import React, { useState, useEffect, useRef, useMemo } from 'react';
import {
  Input,
  Card,
  Space,
  Typography,
  Tag,
  Button,
  Tooltip,
  Dropdown,
  Menu,
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
      <Card
        className="enhanced-card"
        size="small"
        style={{
          marginBottom: 16,
          background: 'rgba(255, 255, 255, 0.98)',
          border: '2px solid #e8f4fd'
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
              onDropdownVisibleChange={setShowSuggestions}
              style={{ width: '100%' }}
              dropdownStyle={{
                maxHeight: 300,
                overflow: 'auto',
                borderRadius: '12px',
                boxShadow: '0 8px 32px rgba(0, 0, 0, 0.12)'
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
                  fontSize: '16px',
                  lineHeight: '1.6',
                  padding: '16px'
                }}
              />
            </AutoComplete>

            {/* Enhanced Input Actions */}
            <div
              style={{
                position: 'absolute',
                bottom: 12,
                right: 12,
                display: 'flex',
                gap: 8,
              }}
            >
              {value && (
                <Tooltip title="Clear input">
                  <Button
                    size="small"
                    type="text"
                    icon={<ClearOutlined />}
                    onClick={handleClear}
                    style={{
                      borderRadius: '8px',
                      background: 'rgba(255, 255, 255, 0.9)',
                      border: '1px solid #d9d9d9'
                    }}
                  />
                </Tooltip>
              )}

              <Tooltip title="Submit query (Ctrl+Enter)">
                <Button
                  size="small"
                  type="primary"
                  icon={<SendOutlined />}
                  loading={loading}
                  onClick={handleSubmit}
                  disabled={!value.trim()}
                  className="query-submit-btn"
                  style={{
                    height: '36px',
                    borderRadius: '8px',
                    fontWeight: 600
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
              padding: '12px',
              background: 'linear-gradient(135deg, #f8f9ff 0%, #e8f4fd 100%)',
              borderRadius: '8px',
              border: '1px solid #e8f4fd'
            }}>
              <Text type="secondary" style={{ fontSize: '13px', fontWeight: 500 }}>
                ⚡ Quick shortcuts:
              </Text>
              {shortcutHints.map(hint => (
                <Tooltip key={hint.shortcut} title={hint.example}>
                  <Tag
                    color="blue"
                    style={{
                      cursor: 'pointer',
                      fontSize: '12px',
                      borderRadius: '6px',
                      fontWeight: 500,
                      transition: 'all 0.3s ease'
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
                    borderRadius: '6px',
                    fontWeight: 500
                  }}
                >
                  More
                </Button>
              </Dropdown>
            </div>
          )}

          {/* Enhanced Help Text */}
          <div style={{
            padding: '8px 12px',
            background: 'rgba(102, 126, 234, 0.05)',
            borderRadius: '6px',
            border: '1px solid rgba(102, 126, 234, 0.1)'
          }}>
            <Text type="secondary" style={{ fontSize: '12px', color: '#667eea' }}>
              💡 <strong>Tip:</strong> Use shortcuts like "rev", "users", "top10" or start typing to see suggestions.
              Press <kbd style={{
                background: '#f0f0f0',
                padding: '2px 6px',
                borderRadius: '4px',
                fontSize: '11px'
              }}>Ctrl+Enter</kbd> to submit.
            </Text>
          </div>
        </Space>
      </Card>
    </div>
  );
};
