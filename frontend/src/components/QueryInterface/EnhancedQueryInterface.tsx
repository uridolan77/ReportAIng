import React, { useState, useRef, useEffect } from 'react';
import { Button, Input, Typography, Space, Card, Tooltip, Spin } from 'antd';
import {
  SendOutlined,
  StarOutlined,
  HistoryOutlined,
  BulbOutlined,
  LoadingOutlined
} from '@ant-design/icons';
import { useTheme } from '../../contexts/ThemeContext';

const { TextArea } = Input;
const { Title, Text } = Typography;

interface EnhancedQueryInterfaceProps {
  onSubmit?: (query: string) => void;
  loading?: boolean;
  placeholder?: string;
  suggestions?: string[];
  showExamples?: boolean;
}

export const EnhancedQueryInterface: React.FC<EnhancedQueryInterfaceProps> = ({
  onSubmit,
  loading = false,
  placeholder = "Ask me anything about your data...",
  suggestions = [],
  showExamples = true
}) => {
  const [query, setQuery] = useState('');
  const [isExpanded, setIsExpanded] = useState(false);
  const [showSuggestions, setShowSuggestions] = useState(false);
  const inputRef = useRef<any>(null);
  const { isDarkMode } = useTheme();

  const exampleQueries = [
    "Show me the top 10 players by total deposits this month",
    "What's the average session duration for active players?",
    "Compare revenue between different countries last quarter",
    "Which games have the highest player retention rate?"
  ];

  const handleSubmit = () => {
    if (query.trim() && onSubmit) {
      onSubmit(query.trim());
      setQuery('');
      setIsExpanded(false);
    }
  };

  const handleKeyPress = (e: React.KeyboardEvent) => {
    if (e.key === 'Enter' && (e.ctrlKey || e.metaKey)) {
      e.preventDefault();
      handleSubmit();
    }
  };

  const handleExampleClick = (example: string) => {
    setQuery(example);
    setIsExpanded(true);
    inputRef.current?.focus();
  };

  const handleInputFocus = () => {
    setIsExpanded(true);
    setShowSuggestions(true);
  };

  const handleInputBlur = () => {
    // Delay hiding suggestions to allow clicking
    setTimeout(() => setShowSuggestions(false), 200);
  };

  useEffect(() => {
    if (isExpanded && inputRef.current) {
      inputRef.current.focus();
    }
  }, [isExpanded]);

  return (
    <div className="enhanced-query-interface" style={{ 
      padding: '40px 20px',
      maxWidth: '1400px',
      margin: '0 auto',
      minHeight: '100vh',
      display: 'flex',
      flexDirection: 'column',
      justifyContent: 'center',
      background: isDarkMode 
        ? 'linear-gradient(135deg, #0f172a 0%, #1e293b 100%)'
        : 'linear-gradient(135deg, #f7fafc 0%, #edf2f7 100%)'
    }}>
      {/* Header Section */}
      <div style={{ 
        textAlign: 'center', 
        marginBottom: '48px',
        animation: 'fadeInUp 0.6s ease-out'
      }}>
        <div style={{
          display: 'inline-flex',
          alignItems: 'center',
          gap: '16px',
          marginBottom: '24px'
        }}>
          <div style={{
            width: '64px',
            height: '64px',
            borderRadius: '20px',
            background: 'linear-gradient(135deg, #3b82f6 0%, #2563eb 100%)',
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'center',
            boxShadow: '0 20px 40px rgba(59, 130, 246, 0.3)',
            animation: 'slideInRight 0.4s ease-out'
          }}>
            <StarOutlined style={{ fontSize: '32px', color: 'white' }} />
          </div>
          <Title 
            level={1} 
            className="text-gradient"
            style={{ 
              margin: 0,
              fontSize: '3.5rem',
              fontWeight: 800,
              background: 'linear-gradient(135deg, #3b82f6 0%, #2563eb 100%)',
              WebkitBackgroundClip: 'text',
              WebkitTextFillColor: 'transparent',
              backgroundClip: 'text',
              letterSpacing: '-0.02em'
            }}
          >
            Talk with DailyActionsDB
          </Title>
        </div>
        <Text style={{ 
          fontSize: '1.25rem',
          color: isDarkMode ? '#94a3b8' : '#4a5568',
          fontWeight: 400,
          maxWidth: '600px',
          display: 'block',
          margin: '0 auto'
        }}>
          Ask questions about your data in natural language and get instant insights
        </Text>
      </div>

      {/* Main Query Input */}
      <div className="enhanced-query-input-container fade-in-up" style={{ marginBottom: '32px' }}>
        <div className="query-input-passepartout">
          <TextArea
            ref={inputRef}
            value={query}
            onChange={(e) => setQuery(e.target.value)}
            onKeyDown={handleKeyPress}
            onFocus={handleInputFocus}
            onBlur={handleInputBlur}
            placeholder={placeholder}
            className="enhanced-query-input"
            autoSize={{ minRows: isExpanded ? 4 : 3, maxRows: 12 }}
            disabled={loading}
            style={{
              fontSize: '18px',
              lineHeight: '1.6',
              fontFamily: "'Inter', -apple-system, BlinkMacSystemFont, sans-serif"
            }}
          />
          
          {/* Submit Button */}
          <div style={{ 
            display: 'flex', 
            justifyContent: 'flex-end', 
            marginTop: '20px',
            gap: '12px'
          }}>
            <Button
              type="primary"
              size="large"
              onClick={handleSubmit}
              disabled={!query.trim() || loading}
              className="enhanced-submit-button"
              icon={loading ? <LoadingOutlined /> : <SendOutlined className="button-icon" />}
              style={{ minWidth: '160px' }}
            >
              {loading ? 'Processing...' : 'Ask Question'}
            </Button>
          </div>
        </div>
      </div>

      {/* Example Questions */}
      {showExamples && !isExpanded && (
        <div className="fade-in-up" style={{ animationDelay: '0.2s' }}>
          <div style={{ 
            textAlign: 'center', 
            marginBottom: '24px' 
          }}>
            <Text style={{ 
              fontSize: '16px',
              color: isDarkMode ? '#64748b' : '#718096',
              fontWeight: 500
            }}>
              Try these example questions:
            </Text>
          </div>
          
          <div style={{
            display: 'grid',
            gridTemplateColumns: 'repeat(auto-fit, minmax(300px, 1fr))',
            gap: '16px',
            maxWidth: '1200px',
            margin: '0 auto'
          }}>
            {exampleQueries.map((example, index) => (
              <Card
                key={index}
                className="enhanced-card-modern"
                hoverable
                onClick={() => handleExampleClick(example)}
                style={{
                  cursor: 'pointer',
                  transition: 'all 0.3s cubic-bezier(0.4, 0, 0.2, 1)',
                  animationDelay: `${0.1 * index}s`
                }}
                bodyStyle={{ padding: '20px' }}
              >
                <div style={{ display: 'flex', alignItems: 'flex-start', gap: '12px' }}>
                  <BulbOutlined style={{ 
                    color: '#3b82f6', 
                    fontSize: '18px',
                    marginTop: '2px',
                    flexShrink: 0
                  }} />
                  <Text style={{ 
                    fontSize: '15px',
                    lineHeight: '1.5',
                    color: isDarkMode ? '#e2e8f0' : '#1a202c'
                  }}>
                    {example}
                  </Text>
                </div>
              </Card>
            ))}
          </div>
        </div>
      )}

      {/* Suggestions Dropdown */}
      {showSuggestions && suggestions.length > 0 && (
        <Card
          className="enhanced-card-modern"
          style={{
            position: 'absolute',
            top: '100%',
            left: 0,
            right: 0,
            marginTop: '8px',
            zIndex: 1000,
            maxHeight: '200px',
            overflow: 'auto'
          }}
          bodyStyle={{ padding: '8px' }}
        >
          {suggestions.map((suggestion, index) => (
            <div
              key={index}
              style={{
                padding: '12px 16px',
                cursor: 'pointer',
                borderRadius: '8px',
                transition: 'background-color 0.2s ease'
              }}
              onClick={() => handleExampleClick(suggestion)}
              onMouseEnter={(e) => {
                e.currentTarget.style.backgroundColor = isDarkMode ? '#2a2a2a' : '#f7fafc';
              }}
              onMouseLeave={(e) => {
                e.currentTarget.style.backgroundColor = 'transparent';
              }}
            >
              <Text>{suggestion}</Text>
            </div>
          ))}
        </Card>
      )}

      {/* Loading State */}
      {loading && (
        <div className="enhanced-loading-container fade-in-up">
          <Spin 
            indicator={<LoadingOutlined style={{ fontSize: 40, color: '#3b82f6' }} spin />}
            size="large"
          />
          <Text className="enhanced-loading-text">
            Analyzing your question and generating insights...
          </Text>
          <div className="enhanced-loading-progress">
            <div className="enhanced-loading-progress-bar" />
          </div>
        </div>
      )}

      {/* Footer Hint */}
      <div style={{ 
        textAlign: 'center', 
        marginTop: '40px',
        opacity: 0.7
      }}>
        <Text style={{ 
          fontSize: '14px',
          color: isDarkMode ? '#64748b' : '#718096'
        }}>
          Press <kbd style={{ 
            padding: '2px 6px',
            background: isDarkMode ? '#374151' : '#e2e8f0',
            borderRadius: '4px',
            fontSize: '12px'
          }}>Ctrl + Enter</kbd> to submit quickly
        </Text>
      </div>
    </div>
  );
};

export default EnhancedQueryInterface;
