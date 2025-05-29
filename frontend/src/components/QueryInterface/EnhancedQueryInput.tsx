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
      <Card size="small" style={{ marginBottom: 8 }}>
        <Space direction="vertical" style={{ width: '100%' }}>
          {/* Query Input */}
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
              dropdownStyle={{ maxHeight: 300, overflow: 'auto' }}
            >
              <TextArea
                ref={inputRef}
                placeholder={placeholder}
                autoSize={autoHeight ? { minRows: 2, maxRows } : false}
                onKeyDown={handleKeyDown}
                style={{ resize: 'vertical' }}
              />
            </AutoComplete>

            {/* Input Actions */}
            <div
              style={{
                position: 'absolute',
                bottom: 8,
                right: 8,
                display: 'flex',
                gap: 4,
              }}
            >
              {value && (
                <Tooltip title="Clear input">
                  <Button
                    size="small"
                    type="text"
                    icon={<ClearOutlined />}
                    onClick={handleClear}
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
                />
              </Tooltip>
            </div>
          </div>

          {/* Shortcut Hints */}
          {showShortcuts && shortcutHints.length > 0 && (
            <div style={{ display: 'flex', alignItems: 'center', gap: 8, flexWrap: 'wrap' }}>
              <Text type="secondary" style={{ fontSize: '12px' }}>
                Quick shortcuts:
              </Text>
              {shortcutHints.map(hint => (
                <Tooltip key={hint.shortcut} title={hint.example}>
                  <Tag
                    color="blue"
                    style={{ cursor: 'pointer', fontSize: '11px' }}
                    onClick={() => onChange(hint.shortcut)}
                  >
                    {hint.shortcut}
                  </Tag>
                </Tooltip>
              ))}
              <Dropdown menu={{ items: shortcutMenuItems }} trigger={['click']}>
                <Button size="small" type="text" icon={<BookOutlined />}>
                  More
                </Button>
              </Dropdown>
            </div>
          )}

          {/* Help Text */}
          <Text type="secondary" style={{ fontSize: '11px' }}>
            💡 Tip: Use shortcuts like "rev", "users", "top10" or start typing to see suggestions.
            Press Ctrl+Enter to submit.
          </Text>
        </Space>
      </Card>
    </div>
  );
};
